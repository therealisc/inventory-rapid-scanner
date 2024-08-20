using DScannerLibrary.DataAccess;
using DScannerLibrary.Helpers;
using DScannerLibrary.Models;
using DScannerLibrary.Helpers;
using System.ComponentModel;
using System.Globalization;
using System.Data.OleDb;
using System.Text;
using System;
using DbfReaderNET;
using DbfDataReader;

namespace DScannerLibrary.BusinessLogic;

public class InventoryMovementsLogic
{
    private readonly DbfDataAccess _dataAccess;
    private readonly ArticleSearchLogic _articleSearchLogic;
    private readonly ExitDocumentCheck _exitDocumentCheck;

    public InventoryMovementsLogic(DbfDataAccess dbfDataAccess, ArticleSearchLogic articleSearchLogic, ExitDocumentCheck exitDocumentCheck)
    {
        _dataAccess = dbfDataAccess;
        _articleSearchLogic = articleSearchLogic;
        _exitDocumentCheck = exitDocumentCheck;
    }

    public List<InventoryExitModel> GetInventoryExitsByDate(DateTime? exitDate)
    {
        var parameters = new List<OleDbParameter>()
        {
            new OleDbParameter { Value = exitDate }
        };

        var inventoryExists = _dataAccess
            .ReadDbf<InventoryExitModel>(
                    $"Select d.den_gest, d.cod, d.denumire, d.den_tip, d.um, d.cantitate, d.pret_unitar, d.valoare, d.total, d.adaos, d.cont, d.text_supl " +
                    "from ies_det d inner join iesiri i on d.id_iesire = i.id_iesire where i.data=?",
                    parameters.ToArray());

        return inventoryExists;
    }

    public List<InventoryExitModel> GetInventoryExitsByDate(string dbDirectory, DateTime? selectedExitDate, string dbfName="IESIRI.DBF")
    {
        string dbfPath = $"{DatabaseDirectoryHelper.GetDatabaseDirectory(dbDirectory)}/{dbfName}";

        var inventoryExitIds = new List<decimal>();

        var options = new DbfDataReaderOptions
        {
            SkipDeletedRecords = true,
            Encoding = Encoding.UTF8
        };

        using (var dbfDataReader = new DbfDataReader.DbfDataReader(dbfPath, options))
        {
            while (dbfDataReader.Read())
            {
                var exitDate = dbfDataReader.GetDateTime(5);

                if( exitDate == selectedExitDate.Value.Date)
                {
                    var idIesire = dbfDataReader.GetInt64(1);
                    inventoryExitIds.Add(idIesire);
                }
            }
        }

        dbfName = "IES_DET.DBF";

	    var dbfDataRecords = _dataAccess.ReadDbf(dbDirectory, dbfName);
        var inventoryExitRecords = new List<InventoryExitModel>();

        foreach(var record in dbfDataRecords)
        {
            var exit = new InventoryExitModel()
            {
                //id_u = Convert.ToDecimal(record[0]),
                id_iesire = Convert.ToDecimal(record[1]),
                gestiune = Convert.ToString(record[2]),
                den_gest = Convert.ToString(record[3]),
                cod = Convert.ToString(record[4]),
                denumire = Convert.ToString(record[5]),

                den_tip = Convert.ToString(record[6]),
                um = Convert.ToString(record[7]),
                //tva_art = Convert.ToDecimal(record[8]),

                //HACK: handle extra zeors
                cantitate = Convert.ToInt32(record[9]) / 1000,
                pret_unitar = Convert.ToDecimal(record[10]) / 10000,
                valoare = Convert.ToDecimal(record[11]),
                //tva_ded = Convert.ToDecimal(record[12]),
                total = Convert.ToDecimal(record[13]) / 1000,
                adaos = Convert.ToDecimal(record[14]),

                cont = Convert.ToString(record[15]),
                text_supl = Convert.ToString(record[16]),
                //discount = Convert.ToDecimal(record[17]),
                //plan = Convert.ToString(record[18]),
                //sector = Convert.ToString(record[19]),
                //sursa = Convert.ToString(record[20]),
                //articol = Convert.ToString(record[22]),
                //capitol = Convert.ToString(record[23]),
                //categorie = Convert.ToString(record[24]),
            };


            if (inventoryExitIds.Contains(exit.id_iesire))
            {
                inventoryExitRecords.Add(exit);
            }
        }

        return inventoryExitRecords;
    }

    public List<InventoryMovementModel> GetInventoryMovementsForArticle(string? articleCode)
    {
        var inventoryMovements = _dataAccess.ReadDbf<InventoryMovementModel>($"Select cod_art, gestiune, SUM(cantitate) as cantitate from miscari " +
            $"where cod_art='{articleCode}' group by cod_art, gestiune order by gestiune");

        return inventoryMovements;
    }

    public async Task<int> GenerateInventoryExits(string barcode, decimal quantity)
    {
        var exitDocumentId = _exitDocumentCheck.GetExitDocumentId();

        if (exitDocumentId == 0)
        {
            throw new Exception(
                    "Adauga in SAGA o iesire cu data de azi mai intai!\nAsigura-te ca documentul de iesire nu este validat!\n");
        }

        var article = _articleSearchLogic.GetArticleByBarcode(barcode);

        // inventoryMovements va avea atatea randuri cate gestiuni sunt
        var inventoryMovements = GetInventoryMovementsForArticle(article?.cod.Trim());

        var numberOfInventories = inventoryMovements.Count;

        if (numberOfInventories == 1 && article != null)
        {
            var inventoryMovement = inventoryMovements.SingleOrDefault();
            if (inventoryMovement?.gestiune != null)
                return await ProcessInventoryExit(exitDocumentId, article, quantity, inventoryMovement.gestiune, quantity, quantity);
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
                    throw new Exception("Stocul este 0 din acest produs pe toate gestiunile! Adauga intrari mai intai!");
                }

                var inventoryCode = GetCorrectInventoryCode(lastMultipleInventoryExit, inventoryMovements, actualInventoriesQuantities);
                rowsInserted += await ProcessInventoryExit(exitDocumentId, article, 1, inventoryCode, i + 1, quantity);
            }
            return rowsInserted;
        }

        if (numberOfInventories == 0)
        {
            var errorMessage = "Nu au fost gasite intrari pentru acest produs!\n";

            if (article != null)
            {
                errorMessage += $"Adauga intrari in SAGA pentru {article?.denumire?.Trim()}.";
            }

            throw new Exception(errorMessage);
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
        decimal exitQuantity,
        string inventoryCode,
        decimal currentMultipleInventoryIteration,
        decimal totalQuantity)
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
            cantitate = exitQuantity,
            pret_unitar = article.pret_vanz,
            valoare = exitQuantity * article.pret_vanz,
            total = (exitQuantity * article.pret_vanz) + ((article.tva / 100) * article.pret_vanz),
            tva_art = article.tva,
            tva_ded = ((article.tva / 100) * article.pret_vanz),
            cont = "707",
            den_tip = "Marfuri",
            um = "BUC",
            text_supl = $"{currentMultipleInventoryIteration}/{totalQuantity} articol(e) scanat(e) la {DateTime.Now}"
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
                .SingleOrDefault();

            if (exitQuantity == null)
            {
                exitQuantity = new InventoryExitModel { cantitate = 0 };
            }

            var actualQuantity = item.cantitate - exitQuantity.cantitate;

            availableInventories.Add(item.gestiune, actualQuantity);
        }

        return availableInventories;
    }
}
