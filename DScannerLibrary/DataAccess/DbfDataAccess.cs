using System.Data;
using System.Text;
using DScannerLibrary.Extensions;
using DScannerLibrary.Helpers;
using DbfReaderNET;

namespace DScannerLibrary.DataAccess;

public class DbfDataAccess
{
    public List<DbfRecord> ReadDbf(string dbfName)
    {
        var dbf = new Dbf();
        string dbfPath = $"{DatabaseDirectoryHelper.GetDatabaseDirectory()}/{dbfName}";

        dbf.Read(dbfPath);
        return dbf.Records;
    }

    public void InsertData(string rawSql)
    {
        throw new NotImplementedException();
    }
}
