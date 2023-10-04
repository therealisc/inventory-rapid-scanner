using System.Data;
using System.Data.OleDb;

namespace DScannerLibrary.DataAccess;

public class DbfDataAccess
{
    string constr = $"Provider=VFPOLEDB;Data Source=C:\\SAGA PS.3.0\\0001\\";

    public DataTable ReadDbf(string str_sql)
    {
        using (var connection = new OleDbConnection(constr))
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter(str_sql, connection);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }
    }
}
