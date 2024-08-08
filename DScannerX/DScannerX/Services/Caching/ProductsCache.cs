using DScannerLibrary.BusinessLogic;
using DScannerLibrary.DataAccess;

namespace DScannerX.Services.Caching;

public class ProductsCache
{
    public List<string> GetRecords(string path)
    {
        var inventoryLogic = new InventoryMovementsLogic(
            new DbfDataAccess(), 
            new ArticleSearchLogic(new DbfDataAccess()), new ExitDocumentCheck(new DbfDataAccess()));

        var records = inventoryLogic.GetArticlesForTesting(path);

        return records;
    }
}