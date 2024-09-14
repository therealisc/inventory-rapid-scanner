using DScannerLibrary.DataAccess;
using DScannerLibrary.Models;

namespace DScannerLibrary.BusinessLogic;

public class ExitDocumentCheck
{
    private IDataAccess _dataAccess;

    public ExitDocumentCheck(IDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public decimal GetExitDocumentId()
    {
        var exitDocument = _dataAccess
            .ReadData<InventoryExitModel>("Select top 1 id_iesire from iesiri where data = DATE() and Validat <>'V' order by data desc")
            .SingleOrDefault();

        if (exitDocument == null)
            return 0;

        return exitDocument.id_iesire;
    }
}
