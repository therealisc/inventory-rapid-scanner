using DScannerLibrary.DataAccess;

namespace DScannerLibrary.BusinessLogic;

public class ExitDocumentCheck
{
    public decimal GetExitDocumentId()
    {
        var dataAccess = new DbfDataAccess();
        var exitDocumentDataTable = dataAccess.ReadDbf("Select top 1 id_iesire from iesiri where data = DATE() order by data desc");

        if (exitDocumentDataTable.Rows.Count == 0)
            return 0;

        return Convert.ToDecimal(exitDocumentDataTable.Rows[0][0]);
    }
}
