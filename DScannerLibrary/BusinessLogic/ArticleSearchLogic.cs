using DScannerLibrary.DataAccess;
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
        var articles = _dataAccess.ReadDbf<ArticleModel>($"Select * from articole where cod_bare={articleBarcode}");
        var article = articles.SingleOrDefault();

        return article;
    }
}
