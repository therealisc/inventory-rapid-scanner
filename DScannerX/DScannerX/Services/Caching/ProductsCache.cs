using DScannerLibrary.BusinessLogic;
using DScannerLibrary.DataAccess;

namespace DScannerX.Services.Caching;

public class ProductsCache
{
    public List<string> GetRecords()
    {
        var inventoryLogic = new InventoryMovementsLogic(
            new DbfDataAccess(), 
            new ArticleSearchLogic(new DbfDataAccess()), new ExitDocumentCheck(new DbfDataAccess()));

        var records = inventoryLogic.GetArticlesForTesting();

        return records;
    }
}