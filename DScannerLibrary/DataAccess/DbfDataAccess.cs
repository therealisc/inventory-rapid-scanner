using System.Data;
using System.Text;
using DScannerLibrary.Extensions;
using DScannerLibrary.Helpers;
using DbfReaderNET;

namespace DScannerLibrary.DataAccess;

public class DbfDataAccess
{
    private readonly string _connectionString;

    public DbfDataAccess()
    {
        _connectionString = GetConnectionString();
    }

    string GetConnectionString()
    {
        string connectionString = $"Provider=VFPOLEDB;Data Source={DatabaseDirectoryHelper.GetDatabaseDirectory()}";
        return connectionString;
    }

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
