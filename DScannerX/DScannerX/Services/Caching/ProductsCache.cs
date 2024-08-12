using DScannerLibrary.BusinessLogic;
using DScannerLibrary.DataAccess;
using DScannerLibrary.Models;

namespace DScannerX.Services.Caching;

public class ProductsCache
{
    public List<InventoryExitModel> GetRecords(string dirPath, DateTime? date)
    {
        var inventoryLogic = new InventoryMovementsLogic(
            new DbfDataAccess(), 
            new ArticleSearchLogic(new DbfDataAccess()), new ExitDocumentCheck(new DbfDataAccess()));

        var records = inventoryLogic.GetInventoryExitsByDate(dirPath, date);

        return records;
    }
}
