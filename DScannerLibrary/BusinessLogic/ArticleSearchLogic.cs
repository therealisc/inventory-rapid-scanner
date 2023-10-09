using DScannerLibrary.DataAccess;
using DScannerLibrary.Extensions;
using DScannerLibrary.Models;

namespace DScannerLibrary.BusinessLogic;

public class ArticleSearchLogic
{
    public ArticleModel? GetArticleByBarcode(string articleBarcode)
    {
        var dataAccess = new DbfDataAccess();

        var articleAsDataTable = dataAccess.ReadDbf($"Select * from articole where cod_bare={articleBarcode}");
        var articleAsList = articleAsDataTable.ConvertDataTable<ArticleModel>();
        var article = articleAsList.SingleOrDefault();

        return article;
    }
}
