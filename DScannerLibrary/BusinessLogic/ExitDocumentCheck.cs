using DScannerLibrary.DataAccess;

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
        var exitDocumentDataTable = _dataAccess.ReadDbf("Select top 1 id_iesire from iesiri where data = DATE() order by data desc");

        if (exitDocumentDataTable.Rows.Count == 0)
            return 0;

        return Convert.ToDecimal(exitDocumentDataTable.Rows[0][0]);
    }
}
