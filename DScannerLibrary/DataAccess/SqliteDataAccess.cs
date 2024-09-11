using System.Data;
using DScannerLibrary.Extensions;
using Microsoft.Data.Sqlite;

namespace DScannerLibrary.DataAccess;

public class SqliteDataAccess
{
    private readonly string _connectionString;

    public SqliteDataAccess()
    {
        _connectionString = GetConnectionString();
    }

    string GetConnectionString()
    {
        string connectionString = $"Data Source=Saga.db";
        return connectionString;
    }

    public List<T> ReadData<T>(string query)
    {
	var dataTable = new DataTable();
        using (var connection = new SqliteConnection(_connectionString))
        {
	    connection.Open();

	    using (var command = new SqliteCommand(query, connection))
	    {
		using (var dataReader = command.ExecuteReader())
		{
		    dataTable.Load(dataReader);
		}
	    }

	    connection.Close();
        }

	return DataTableToListExtension.ConvertDataTable<T>(dataTable);
    }

    public void InsertData(string rawSql)
    {
        using (var connection = new SqliteConnection(_connectionString))
	{
	    connection.Open();

	    var command = connection.CreateCommand();
	    command.CommandText = rawSql;
	    command.ExecuteNonQuery();

	    connection.Close();
	}
    }
}
