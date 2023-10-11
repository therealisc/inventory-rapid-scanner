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

            // ultimul produs iesit -> ce gestiune are?
            var lastInventoryExitOfArticleDataTable = _dataAccess.ReadDbf($"Select top 1 * from ies_det where cod='{article.cod}' order by gestiune desc");
            var lastInventoryExitOfArticle = lastInventoryExitOfArticleDataTable.ConvertDataTable<InventoryExitModel>();
            var inventoryExitOfArticle = lastInventoryExitOfArticle.FirstOrDefault();


            // daca nu exista ultimul produs (in tabela iesir detatlii), First where max(cantitate) si sa fie mai mare de 0
            if (quantity == 1)
            {
                // gestiunea care trebuie atribuita
                string? lastInventory = "";
                // works very well
                if (inventoryExitOfArticle == null)
                {
                    try
                    {
                        Console.WriteLine("null - none exit of this product");
                        lastInventory = inventoryMovements.OrderByDescending(x => x.cantitate).First().gestiune;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                else
                {
                    // schimb gestiunea pe rand in functie de cod
                    // de gandit ......lastInventory = inventoryMovements. inventoryExitOfArticle.gestiune;
                    // HACK
                    var list = inventoryMovements.Select(x => x.gestiune).ToList();
                    list.Remove(inventoryExitOfArticle.gestiune);

                    lastInventory = (list.FirstOrDefault());
                }

                // fac iesirea
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

                lastInventory = "";

                await AddExitToBackupFile(new List<InventoryExitModel>() { inventoryExit });

                return _dataAccess.InsertIntoIesiriDbf(inventoryExit);

            }

            if (quantity > 1)
            {
                Console.WriteLine("More than one quantity");
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
}
