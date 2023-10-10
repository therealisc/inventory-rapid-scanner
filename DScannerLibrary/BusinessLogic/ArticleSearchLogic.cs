using DScannerLibrary.DataAccess;
using DScannerLibrary.Extensions;
using DScannerLibrary.Models;

namespace DScannerLibrary.BusinessLogic;

public class ArticleSearchLogic
{
    private readonly DbfDataAccess _dataAccess;

    public ArticleSearchLogic()
    {
        _dataAccess = new DbfDataAccess();
    }

    public ArticleModel? GetArticleByBarcode(string articleBarcode)
    {
        var articleAsDataTable = _dataAccess.ReadDbf($"Select * from articole where cod_bare={articleBarcode}");
        var articleAsList = articleAsDataTable.ConvertDataTable<ArticleModel>();
        var article = articleAsList.SingleOrDefault();

        return article;
    }
}
