using DScannerLibrary.DataAccess;
using DScannerLibrary.Extensions;
using DScannerLibrary.Models;
using System.ComponentModel;

namespace DScannerLibrary.BusinessLogic;

public class InventoryMovementsLogic
{
    private readonly DbfDataAccess _dataAccess;

    public InventoryMovementsLogic()
    {
        _dataAccess = new DbfDataAccess();
    }

    public List<InventoryMovementModel> GetInventoryMovements(string? articleCode)
    {
        var articleMovementsDataTable = _dataAccess.ReadDbf($"Select cod_art, gestiune, SUM(cantitate) as cantitate from miscari " +
            $"where cod_art='{articleCode}' group by cod_art, gestiune");
        var inventoryMovements = articleMovementsDataTable.ConvertDataTable<InventoryMovementModel>();

        return inventoryMovements;
    }

    public async Task<int> ProcessInventoryExit(decimal exitDocumentId, string barcode, decimal quantity)
    {
        var articleSearchLogic = new ArticleSearchLogic();
        var article = articleSearchLogic.GetArticleByBarcode(barcode);

        // articole pe gestiuni: inventoryMovements va avea atatea randuri cate gestiuni sunt
        var inventoryMovements = GetInventoryMovements(article?.cod);

        var numberOfInventories = inventoryMovements.Count;
        if (numberOfInventories == 1)
        {
            return await ProcessSingleInventoryExit(exitDocumentId, inventoryMovements, article, quantity);
        }

        if (numberOfInventories > 1)
        {
            // Iesire alternativa, cate o bucata din fiecare gestiune pe rand!

            // daca nu, adaug o pozitie cu cantitatea 1 sau adaug mai multe pozitii (maximum cate gestiuni sunt) in distribui alternativ cantitatea
            // daca mai exista pozitii din nou continui cu urmatorea gestiune 
            // ATENTIE! conteaza cantitatea pe gestiune deci o gestiune poate avea mai putine bucati => cand nu mai are nu mai descarc de acolo

            Console.WriteLine("More than one inventory");

            foreach (var item in inventoryMovements)
            {
                Console.WriteLine(item.cod_art + " " + item.gestiune + " " + item.cantitate);
            }

            var lastMultipleInventoryExit = GetLastMultipleInventoryExit(article, exitDocumentId);
            var actualInventoriesQuantities = CalculateAvailableInventory(inventoryMovements);

            if (quantity == 1)
            {
                // gestiunea care trebuie atribuita
                string? lastInventory = "";
                // works very well
                if (lastMultipleInventoryExit == null)
                {
                    try
                    {
                        Console.WriteLine("First exit of this product ever.");
                        lastInventory = inventoryMovements
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
                    Console.WriteLine("Nu e prima iesire din acest produs.");
                    Console.WriteLine("gestiunea precedenta " + lastMultipleInventoryExit.gestiune + " " + lastMultipleInventoryExit.den_gest);

                    foreach (var item in actualInventoriesQuantities)
                    {
                        Console.WriteLine($"Stoc actual: {item.Key} {item.Value}");
                    }

                    var gestiuniSortate = inventoryMovements.Select(x => x.gestiune).Order().ToList();
                    gestiuniSortate.ForEach(x => Console.WriteLine(x));

                    // schimb gestiunea pe rand in functie de cod
                    // HACK
                    //lastInventory = urmatoarea gestiune din array ul sortat mereu la fel

                }

                // fac iesirea - TODO: refactor
                var generatedId = GenerateId(exitDocumentId);

                var inventoryName = _dataAccess.ReadDbf($"Select denumire from gestiuni where cod='{lastInventory}'").Rows[0][0];

                var inventoryExit = new InventoryExitModel
                {
                    id_u = generatedId,
                    id_iesire = exitDocumentId,
                    gestiune = lastInventory,
                    den_gest = (string)inventoryName,
                    cod = article?.cod,
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

            if (quantity > 1)
            {
                Console.WriteLine("Momentan nu se pot opera iesiri cu cantitatea mai mare ca 1 pentru produse in mai multe gestiuni.");
                Console.WriteLine("Scaneaza fiecare produs in parte, fara a introduce cantitatea.");
            }
        }

        if (numberOfInventories == 0)
        {
            // de sters
            Console.WriteLine("Nu au fost gasite intrari pentru acest produs!");
            if (article != null)
            {
                Console.WriteLine($"Adauga intrari in SAGA pentru {article?.denumire?.Trim()}.");
            }
        }

        return 0;
    }

    /// <summary>
    /// Creates an exit record for the only available inventory, without checking the available quantity.
    /// </summary>
    public async Task<int> ProcessSingleInventoryExit(decimal exitDocumentId,
        List<InventoryMovementModel> inventoryMovements,
        ArticleModel article,
        decimal quantity)
    {
        var inventoryMovement = inventoryMovements.Single();

        var generatedId = GenerateId(exitDocumentId);

        var inventoryName = _dataAccess.ReadDbf($"Select denumire from gestiuni where cod='{inventoryMovement?.gestiune}'").Rows[0][0];

        var inventoryExit = new InventoryExitModel
        {
            id_u = generatedId,
            id_iesire = exitDocumentId,
            gestiune = inventoryMovement?.gestiune,
            den_gest = (string)inventoryName,
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
        var numberOfExistsOnCurrentDocument = _dataAccess.ReadDbf($"Select count(*) id_u from ies_det where id_iesire={exitDocumentId}").Rows[0][0];
        var id = DateTime.Now.ToString("yyMMdd");
        id = id + numberOfExistsOnCurrentDocument;

        return Convert.ToDecimal(id);
    }

    InventoryExitModel? GetLastMultipleInventoryExit(ArticleModel article, decimal exitDocumentId)
    {
        var lastInventoryExitOfArticleDataTable = _dataAccess
            .ReadDbf($"Select * from ies_det where id_iesire={exitDocumentId} and cod='{article.cod}'");

        if (lastInventoryExitOfArticleDataTable.Rows.Count == 0)
        {
            lastInventoryExitOfArticleDataTable = _dataAccess
                .ReadDbf($"Select * from ies_det where cod='{article.cod}'");
        }

        if (lastInventoryExitOfArticleDataTable.Rows.Count == 0)
        {
            return null;
        }

        var lastInventoryExitOfArticle = lastInventoryExitOfArticleDataTable.ConvertDataTable<InventoryExitModel>();

        return lastInventoryExitOfArticle.Last();
    }

    Dictionary<string, decimal> CalculateAvailableInventory(List<InventoryMovementModel> registeredInventory)
    {
        var availableInventories = new Dictionary<string, decimal>();

        foreach (var item in registeredInventory)
        {
            decimal exitQuantity = 0;

            var queryResult =_dataAccess
                .ReadDbf($"Select sum(cantitate) as CantitateIesita from ies_det where cod='{item.cod_art}' and gestiune='{item.gestiune}'").Rows[0][0];

            if (queryResult.Equals(DBNull.Value) == false)
            {
                exitQuantity = (decimal)queryResult;
            }

            var actualQuantity = item.cantitate - exitQuantity;

            availableInventories.Add(item.gestiune, actualQuantity);
        }

        return availableInventories;
    }
}
