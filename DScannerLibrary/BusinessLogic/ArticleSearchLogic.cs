using System.Text;
using DScannerLibrary.DataAccess;
using DScannerLibrary.Models;
using DScannerLibrary.Helpers;
using DbfDataReader;

namespace DScannerLibrary.BusinessLogic;

public class ArticleSearchLogic
{
    private readonly DbfDataAccess _dataAccess;

    public ArticleSearchLogic(DbfDataAccess dbfDataAccess)
    {
        _dataAccess = dbfDataAccess;
    }

    public ArticleModel GetArticleByBarcode(string articleBarcode)
    {
        var options = new DbfDataReaderOptions
        {
            SkipDeletedRecords = true,
            Encoding = Encoding.UTF8
        };

        var dbfName = "ARTICOLE.DBF";
        var dbfPath = $"{DatabaseDirectoryHelper.GetDatabaseDirectory()}/{dbfName}";

        var article = new ArticleModel();

        using (var dbfDataReader = new DbfDataReader.DbfDataReader(dbfPath, options))
        {
            while (dbfDataReader.Read())
            {
                article.cod = dbfDataReader.GetString(0),
                article.denumire = dbfDataReader.GetString(1),
                article.pret_vanz = dbfDataReader.GetDecimal(7),
                article.cod_bare = dbfDataReader.GetString(15),

                if (article.cod_bare.Trim() != articleBarcode.Trim())
                {
                    throw new Exception("Codul de bare nu exista sau e gresit! Verifica la articole!");
                }

                return article;
            }
        }

        return article;
    }
}
