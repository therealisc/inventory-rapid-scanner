using System.Data;
using System.Text;
using System.Data.OleDb;
using DScannerLibrary.Extensions;
using DScannerLibrary.Helpers;
using DbfReaderNET;

namespace DScannerLibrary.DataAccess;

public class DbfDataAccess : IDataAccess
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

    public List<T> ReadData<T>(string sqlCommand, OleDbParameter[] parameters)
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

    public List<T> ReadData<T>(string str_sql)
    {
        using (var connection = new OleDbConnection(_connectionString))
        {
            using var adapter = new OleDbDataAdapter(str_sql, connection);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            return DataTableToListExtension.ConvertDataTable<T>(dataTable);
        }
    }

    public int InsertData<T>(T item)
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
