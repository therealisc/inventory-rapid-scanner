using DScannerLibrary.DataAccess;
using DScannerLibrary.Models;

namespace DScannerLibrary.BusinessLogic;

public class ExitDocumentCheck
{
    private readonly DbfDataAccess _dataAccess;

    public ExitDocumentCheck()
    {
        _dataAccess = new DbfDataAccess();
    }

    public decimal GetExitDocumentId()
    {
        var exitDocument = _dataAccess
            .ReadDbf<InventoryExitModel>("Select top 1 id_iesire from iesiri where data = DATE() and Validat <>'V' order by data desc")
            .SingleOrDefault();

        if (exitDocument == null)
            return 0;

        return exitDocument.id_iesire;
    }
}
