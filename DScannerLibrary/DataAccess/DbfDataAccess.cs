using System.Data;
using System.Text;
using System.Data.OleDb;
using DScannerLibrary.Extensions;
using DScannerLibrary.Helpers;
using DbfReaderNET;

namespace DScannerLibrary.DataAccess;

public class DbfDataAccess
{
    private readonly string _connectionString;

    public DbfDataAccess()
    {
        //_connectionString = GetConnectionString();
    }

    string GetConnectionString()
    {
        string connectionString = $"Provider=VFPOLEDB;Data Source={DatabaseDirectoryHelper.GetDatabaseDirectory()}";
        return connectionString;
    }

    public List<string> ReadDbf(string path)
    {
        if (path == "")
        {
            return new List<string>();
        }
        var dbf = new Dbf();

        string dbfPath = path;
        dbf.Read(dbfPath);

        //string cod = DateTime.Now.ToString();

        //DbfRecord r = dbf.CreateRecord();

        //r.Data[0] = cod;
        //r.Data[1] = "CARTI COPII";

        //dbf.Write(dbfPath, DbfVersion.VisualFoxPro);
        //
        var dbfRecords = new List<string>();

        foreach(DbfRecord record in dbf.Records) {
            var stringRecords = "";

            for(int i = 0;  i < dbf.Fields.Count; i++) {
                stringRecords += record[i];
                stringRecords += " ";
            }

            dbfRecords.Add(stringRecords);
        }

        return dbfRecords;
    }

    public List<T> ReadDbf<T>(string sqlCommand, OleDbParameter[] parameters)
    {
        using (var connection = new OleDbConnection(_connectionString))
        {
            using var command = new OleDbCommand();
            command.CommandText = sqlCommand;
            command.Parameters.AddRange(parameters);
            command.Connection = connection;

            using var adapter = new OleDbDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            return DataTableToListExtension.ConvertDataTable<T>(dataTable);
        }
    }

    public List<T> ReadDbf<T>(string str_sql)
    {
        using (var connection = new OleDbConnection(_connectionString))
        {
            using var adapter = new OleDbDataAdapter(str_sql, connection);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            return DataTableToListExtension.ConvertDataTable<T>(dataTable);
        }
    }

    public int InsertIntoIesiriDbf<T>(T item)
    {
        using (OleDbConnection myCon = new OleDbConnection(_connectionString))
        {
            var command = new OleDbCommand();
            StringBuilder commandText = new("insert into ies_det (");
            Type t = item.GetType();
            var props = t.GetProperties();

            foreach (var prop in props)
            {
                commandText.Append($"[{prop.Name}],");

                if (prop.GetIndexParameters().Length == 0)
                {
                    command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(item));
                }
            }

            commandText.Remove(commandText.Length - 1, 1);
            commandText.Append(")");
            commandText.Append("values(");

            foreach (var prop in props)
            {
                commandText.Append($"?,");
            }

            commandText.Remove(commandText.Length - 1, 1);
            commandText.Append(")");

            command.CommandText = commandText.ToString();
            command.Connection = myCon;

            int rowsAffected = 0;
            try
            {
                myCon.Open();
                rowsAffected = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                myCon.Close();
            }

            return rowsAffected;
        }
    }
}
