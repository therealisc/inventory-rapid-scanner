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
    private readonly string _dbDirectory;
    private List<InventoryExitModel> _inventoryExitRecords;

    public InventoryMovementsLogic(DbfDataAccess dbfDataAccess, ArticleSearchLogic articleSearchLogic, ExitDocumentCheck exitDocumentCheck)
    {
        _dataAccess = dbfDataAccess;
        _articleSearchLogic = articleSearchLogic;
        _exitDocumentCheck = exitDocumentCheck;
        _inventoryExitRecords = new List<InventoryExitModel>();
    }

    private static decimal exitDocumentIdToRetain { get; set; }
    private static bool exitDocumentIsValidated { get; set; }

    //public List<InventoryExitModel> GetInventoryExitsByDate(DateTime? exitDate)
    //{
    //    var parameters = new List<OleDbParameter>()
    //    {
    //        new OleDbParameter { Value = exitDate }
    //    };

    //    var inventoryExists = _dataAccess
    //        .ReadDbf<InventoryExitModel>(
    //                $"Select d.den_gest, d.cod, d.denumire, d.den_tip, d.um, d.cantitate, d.pret_unitar, d.valoare, d.total, d.adaos, d.cont, d.text_supl " +
    //                "from ies_det d inner join iesiri i on d.id_iesire = i.id_iesire where i.data=?",
    //                parameters.ToArray());

    //    return inventoryExists;
    //}

    public List<InventoryExitModel> GetInventoryExitsByDate(string dbDirectory, DateTime? selectedExitDate, string dbfName="IESIRI.DBF")
    {
	_dbDirectory = dbDirectory;
        string dbfPath = $"{DatabaseDirectoryHelper.GetDatabaseDirectory(_dbDirectory)}/{dbfName}";

        exitDocumentIdToRetain = 0;
        exitDocumentIsValidated = false;
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

                var validated = dbfDataReader.GetString(9);

                if (exitDate == selectedExitDate.Value.Date)
                {
                    var idIesire = dbfDataReader.GetInt64(1);
                    inventoryExitIds.Add(idIesire);

                    exitDocumentIdToRetain = idIesire;

                    if (validated.Trim() == "V")
                    {
                        exitDocumentIsValidated = true;
                    }
                }
            }
        }

        dbfName = "IES_DET.DBF";
        dbfPath = $"{DatabaseDirectoryHelper.GetDatabaseDirectory(_dbDirectory)}/{dbfName}";

        _inventoryExitRecords = new List<InventoryExitModel>();

        using (var dbfDataReader = new DbfDataReader.DbfDataReader(dbfPath, options))
        {
            while (dbfDataReader.Read())
            {
                try
                {
                    var exit = new InventoryExitModel()
                    {
                        //id_u = Convert.ToDecimal(record[0]),
                        id_iesire = dbfDataReader.GetInt64(1),
                        gestiune = dbfDataReader.GetString(2),
                        den_gest = dbfDataReader.GetString(3),
                        cod = dbfDataReader.GetString(4),
                        denumire = dbfDataReader.GetString(5),

                        den_tip = dbfDataReader.GetString(6),
                        um = dbfDataReader.GetString(7),
                        //tva_art = Convert.ToDecimal(record[8]),

                        cantitate = dbfDataReader.GetDecimal(9),
                        pret_unitar = dbfDataReader.GetDecimal(10),
                        valoare = dbfDataReader.GetDecimal(11),
                        //tva_ded = Convert.ToDecimal(record[12]),
                        total = dbfDataReader.GetDecimal(13),
                        adaos = dbfDataReader.GetDecimal(14),

                        cont = dbfDataReader.GetString(15),
                        text_supl = dbfDataReader.GetString(16),
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
                        _inventoryExitRecords.Add(exit);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to display inventory exits!\n{ex.Message}");
                }
            }
        }

        return _inventoryExitRecords;
    }

    public List<InventoryMovementModel> GetInventoryMovementsForArticle(string? articleCode)
    {
        var options = new DbfDataReaderOptions
        {
            SkipDeletedRecords = true,
            Encoding = Encoding.UTF8
        };

        var dbfName = "MISCARI.DBF";
        var dbfPath = $"{DatabaseDirectoryHelper.GetDatabaseDirectory(_dbDirectory)}/{dbfName}";

        var inventoryMovements = new List<InventoryMovementModel>();

        using (var dbfDataReader = new DbfDataReader.DbfDataReader(dbfPath, options))
        {
            while (dbfDataReader.Read())
            {
                var inventoryMovement = new InventoryMovementModel()
                {
                    cod_art = dbfDataReader.GetString(3),
                    gestiune = dbfDataReader.GetString(4),
                    cantitate = dbfDataReader.GetDecimal(5),
                };
		
                if (inventoryMovement.cod_art == articleCode)
                {
                    inventoryMovements.Add(inventoryMovement);
                }
            }
        }

        var inventorySummary = inventoryMovements
            .GroupBy(x => new {x.cod_art, x.gestiune})
            .Select(i => new InventoryMovementModel()
                {
                    cod_art = i.Key.cod_art,
                    gestiune = i.Key.gestiune,
                    cantitate = i.Sum(i => i.cantitate)
                })
            .ToList();

        return inventorySummary;
    }

    public async Task<int> GenerateInventoryExits(string barcode, decimal quantity)
    {
        //var exitDocumentId = _exitDocumentCheck.GetExitDocumentId();
        var exitDocumentId = exitDocumentIdToRetain;
        var isValidated = exitDocumentIsValidated;

        if (exitDocumentId == 0 || isValidated == true)
        {
            throw new Exception(
                    "Adauga in SAGA o iesire cu data selectata mai intai!\nAsigura-te ca documentul de iesire nu este validat!\n");
        }

        var article = _articleSearchLogic.GetArticleByBarcode(barcode, _dbDirectory);

        // inventoryMovements va avea atatea randuri cate gestiuni sunt
        var inventoryMovements = GetInventoryMovementsForArticle(article.cod.Trim());

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
            var errorMessage = "Nu au fost gasite intrari pentru acest articol!\n";

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

    public async Task<int> ProcessInventoryExit(
	decimal exitDocumentId,
        ArticleModel article,
        decimal exitQuantity,
        string inventoryCode,
        decimal currentMultipleInventoryIteration,
        decimal totalQuantity)
    {
        var generatedId = GenerateId(exitDocumentId);

        var dbfName = "gestiuni.dbf";
        var dbfPath = $"{DatabaseDirectoryHelper.GetDatabaseDirectory(_dbDirectory)}/{dbfName}";

        var inventoryMovements = new List<InventoryMovementModel>();

        var options = new DbfDataReaderOptions
        {
            SkipDeletedRecords = true,
            Encoding = Encoding.UTF8
        };

        using (var dbfDataReader = new DbfDataReader.DbfDataReader(dbfPath, options))
        {
            while (dbfDataReader.Read())
            {
                var inventory = new
                {
                    cod = dbfDataReader.GetString(0),
                    denumire = dbfDataReader.GetString(1),
                };

                if (inventory.cod.Trim() == inventoryCode)
                {
  	            var inventoryMovement = new InventoryMovementModel()
		    {
			cod_art = inventory.cod,
			gestiune = inventory.denumire
		    };

                    inventoryMovements.Add(inventoryMovement);
                }
            }
        }

	var inventoryName = inventoryMovements.SingleOrDefault();

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
        var numberOfExistsOnCurrentDocument = _inventoryExitRecords
		.Where(x => x.id_iesire == exitDocumentId)
		.Count();

        var id = DateTime.Now.ToString("yyMMdd");
        id = id + numberOfExistsOnCurrentDocument.ToString();

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
