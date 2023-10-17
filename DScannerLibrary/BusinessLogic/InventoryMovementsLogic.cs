using DScannerLibrary.DataAccess;
using DScannerLibrary.Models;
using System.ComponentModel;

namespace DScannerLibrary.BusinessLogic;

public class InventoryMovementsLogic
{
    private readonly DbfDataAccess _dataAccess;
    private readonly ArticleSearchLogic _articleSearchLogic;

    public InventoryMovementsLogic()
    {
        _dataAccess = new DbfDataAccess();
        _articleSearchLogic = new ArticleSearchLogic();
    }

    public List<InventoryMovementModel> GetInventoryMovements(string? articleCode)
    {
        var inventoryMovements = _dataAccess.ReadDbf<InventoryMovementModel>($"Select cod_art, gestiune, SUM(cantitate) as cantitate from miscari " +
            $"where cod_art='{articleCode}' group by cod_art, gestiune order by gestiune");

        return inventoryMovements;
    }

    public async Task<int> GenerateInventoryExits(decimal exitDocumentId, string barcode, decimal quantity)
    {
        var article = _articleSearchLogic.GetArticleByBarcode(barcode);

        // inventoryMovements va avea atatea randuri cate gestiuni sunt
        var inventoryMovements = GetInventoryMovements(article?.cod);

        var numberOfInventories = inventoryMovements.Count;

        if (numberOfInventories == 1 && article != null)
        {
            var inventoryMovement = inventoryMovements.SingleOrDefault();
            if (inventoryMovement?.gestiune != null)
                return await ProcessInventoryExit(exitDocumentId, article, quantity, inventoryMovement.gestiune);
        }

        if (numberOfInventories > 1)
        {
            int rowsInserted = 0;
            for (int i = 0; i < quantity; i++)
            {
                var lastMultipleInventoryExit = GetLastMultipleInventoryExit(article, exitDocumentId);
                var actualInventoriesQuantities = CalculateAvailableInventory(inventoryMovements);

                if (actualInventoriesQuantities.Sum(x => x.Value) == 0)
                {
                    Console.WriteLine("Stocul este 0 din acest produs pe toate gestiunile! Adauga intrari mai intai!");
                    return 0;
                }

                var inventoryCode = GetCorrectInventoryCode(lastMultipleInventoryExit, inventoryMovements, actualInventoriesQuantities);
                rowsInserted += await ProcessInventoryExit(exitDocumentId, article, 1, inventoryCode);
            }
            return rowsInserted;
        }

        if (numberOfInventories == 0)
        {
            Console.WriteLine("Nu au fost gasite intrari pentru acest produs!");
            if (article != null)
            {
                Console.WriteLine($"Adauga intrari in SAGA pentru {article?.denumire?.Trim()}.");
            }
        }

        return 0;
    }

    string GetCorrectInventoryCode(InventoryExitModel lastMultipleInventoryExit,
            List<InventoryMovementModel> inventoryMovements,
            Dictionary<string, decimal> actualInventoriesQuantities)
    {
        // gestiunea care trebuie atribuita
        string? nextInventory = "";
        if (lastMultipleInventoryExit == null)
        {
            try
            {
                Console.WriteLine("First exit of this product ever.");
                nextInventory = inventoryMovements
                    .Where(x => x.cantitate > 0)
                    .OrderByDescending(x => x.cantitate)
                    .First().gestiune;
            }
            catch (Exception e)
            {
                // TODO: de testat cu first sa crape
                throw e;
            }
        }
        else
        {
            foreach (var item in actualInventoriesQuantities)
            {
                Console.WriteLine($"Stoc actual: {item.Key} {item.Value}");
            }

            var rotationAlgorithm = new InventoryRotationAlgorithm();
            nextInventory = rotationAlgorithm.GetNextInventoryForExitProcess(actualInventoriesQuantities, lastMultipleInventoryExit);
        }

        return nextInventory;
    }

    public async Task<int> ProcessInventoryExit(decimal exitDocumentId,
        ArticleModel article,
        decimal quantity,
        string inventoryCode)
    {

        var generatedId = GenerateId(exitDocumentId);
        var inventoryName = _dataAccess
            .ReadDbf<InventoryMovementModel>($"Select denumire as gestiune from gestiuni where cod='{inventoryCode}'")
            .SingleOrDefault();

        var inventoryExit = new InventoryExitModel
        {
            id_u = generatedId,
            id_iesire = exitDocumentId,
            gestiune = inventoryCode,
            den_gest = inventoryName?.gestiune,
            cod = article.cod,
            denumire = article?.denumire,
            cantitate = quantity,
            pret_unitar = article.pret_vanz,
            valoare = quantity * article.pret_vanz,
            total = (quantity * article.pret_vanz) + ((article.tva / 100) * article.pret_vanz),
            tva_art = article.tva,
            tva_ded = ((article.tva / 100) * article.pret_vanz),
            cont = "707",
            den_tip = "Marfuri",
            um = "BUC",
            text_supl = $"articol scanat la {DateTime.Now}"
        };

        await AddExitToBackupFile(new List<InventoryExitModel>() { inventoryExit });

        return _dataAccess.InsertIntoIesiriDbf(inventoryExit);
    }

    async Task AddExitToBackupFile(List<InventoryExitModel> inventoryExits)
    {
        var lines = new List<string>();
        IEnumerable<PropertyDescriptor> props = TypeDescriptor.GetProperties(typeof(InventoryExitModel)).OfType<PropertyDescriptor>();
        var header = string.Join(",", props.ToList().Select(x => x.Name));

        //lines.Add(header);
        var valueLines = inventoryExits.Select(row => string.Join(",", header.Split(',').Select(a => row.GetType().GetProperty(a).GetValue(row, null))));
        lines.AddRange(valueLines);
        await File.AppendAllLinesAsync($"{Directory.GetCurrentDirectory()}\\quick_backup.csv", lines);

        //using (StreamWriter writer = new StreamWriter("myfile.csv"))
        //{
        //    foreach (Molecule molecule in molecules)
        //    {
        //        writer.WriteLine($"{molecule.property_A},{molecule.property_B}");
        //    }
        //}
    }

    decimal GenerateId(decimal exitDocumentId)
    {
        var numberOfExistsOnCurrentDocument = _dataAccess
            .ReadDbf<InventoryExitModel>($"Select count(*) as id_u from ies_det where id_iesire={exitDocumentId}")
            .SingleOrDefault();

        if (numberOfExistsOnCurrentDocument == null)
            numberOfExistsOnCurrentDocument.id_u = 0;

        var id = DateTime.Now.ToString("yyMMdd");
        id = id + numberOfExistsOnCurrentDocument.id_u.ToString();

        return Convert.ToDecimal(id);
    }

    InventoryExitModel? GetLastMultipleInventoryExit(ArticleModel article, decimal exitDocumentId)
    {
        var articleExistsList = _dataAccess
            .ReadDbf<InventoryExitModel>($"Select * from ies_det where id_iesire={exitDocumentId} and cod='{article.cod}'");

        if (articleExistsList.Count == 0)
        {
            articleExistsList = _dataAccess
                .ReadDbf<InventoryExitModel>($"Select * from ies_det where cod='{article.cod}'");
        }

        if (articleExistsList.Count == 0)
        {
            return null;
        }

        return articleExistsList.Last();
    }

    Dictionary<string, decimal> CalculateAvailableInventory(List<InventoryMovementModel> registeredInventory)
    {
        var availableInventories = new Dictionary<string, decimal>();

        foreach (var item in registeredInventory)
        {
            var exitQuantity = _dataAccess
                .ReadDbf<InventoryExitModel>($"Select sum(cantitate) as cantitate from ies_det where cod='{item.cod_art}' and gestiune='{item.gestiune}'")
                .SingleOrDefault()?.cantitate;

            if (exitQuantity == null)
            {
                exitQuantity = 0;
            }

            var actualQuantity = item.cantitate - exitQuantity;

            availableInventories.Add(item.gestiune, actualQuantity.Value);
        }

        return availableInventories;
    }
}
