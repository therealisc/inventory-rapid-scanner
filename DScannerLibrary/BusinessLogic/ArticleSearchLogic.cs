using DScannerLibrary.DataAccess;
using DScannerLibrary.Models;

namespace DScannerLibrary.BusinessLogic;

public class ArticleSearchLogic
{
    private readonly DbfDataAccess _dataAccess;

    public ArticleSearchLogic(DbfDataAccess dbfDataAccess)
    {
        _dataAccess = dbfDataAccess;
    }

    public ArticleModel? GetArticleByBarcode(string articleBarcode)
    {
        var options = new DbfDataReaderOptions
        {
            SkipDeletedRecords = true,
            Encoding = Encoding.UTF8
        };

        dbfName = "ARTICOLE.DBF";
        dbfPath = $"{DatabaseDirectoryHelper.GetDatabaseDirectory()}/{dbfName}";

	    var dbfDataRecords = _dataAccess.ReadDbf(dbDirectory, dbfName);
        var inventoryExitRecords = new List<InventoryExitModel>();

        using (var dbfDataReader = new DbfDataReader.DbfDataReader(dbfPath, options))
        {
            while (dbfDataReader.Read())
            {
                var article = new ArticleModel()
                {
                    cod = dbfDataReader.GetString(0),
                    denumire = dbfDataReader.GetString(1),
                    pret_vanz = dbfDataReader.GetDecimal(7),
                };

                if (article.cod_bare.Trim() != articleBarcode.Trim())
                {
                    return null;
                }

                return article;
            }
        }
    }
}
