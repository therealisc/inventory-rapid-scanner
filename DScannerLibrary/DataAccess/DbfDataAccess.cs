using System.Data;
using System.Text;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using DScannerLibrary.Extensions;

namespace DScannerLibrary.DataAccess;

public class DbfDataAccess
{
    private string _connectionString;

    public DbfDataAccess()
    {
        _connectionString = GetConnectionString();
    }

    string GetConnectionString()
    {
        // TODO urgent refactoring
        //var sagaDbfsPath = Directory.GetFiles("C:\\SAGA C.3.0\\").First(x => x.);
        var dirInfo = new DirectoryInfo("C:\\SAGA C.3.0\\");
        var sagaDbfsPath = dirInfo.GetDirectories()
            .Where(x => Regex.IsMatch(x.Name, @"^\d{4}$"))
            .OrderByDescending(x => x.Name)
            .First();

        string connectionString = $"Provider=VFPOLEDB;Data Source={sagaDbfsPath}";
        return connectionString;
    }

    public List<T> ReadDbf<T>(string sqlCommand, OleDbParameter[] parameters)
    {
        using (var connection = new OleDbConnection(_connectionString))
        {
            connection.Open();

            using var command = new OleDbCommand();
            command.CommandText = sqlCommand;
            command.Parameters.AddRange(parameters);
            command.Connection = connection;

            using var adapter = new OleDbDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            connection.Close();

            return DataTableToListExtension.ConvertDataTable<T>(dataTable);
        }
    }

    public List<T> ReadDbf<T>(string str_sql)
    {
        Console.WriteLine("database read...");

        using (var connection = new OleDbConnection(_connectionString))
        {
            connection.Open();

            using var adapter = new OleDbDataAdapter(str_sql, connection);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            connection.Close();

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
