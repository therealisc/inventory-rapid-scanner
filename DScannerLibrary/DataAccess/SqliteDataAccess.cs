using System.Data;
using DScannerLibrary.Extensions;
using Microsoft.Data.Sqlite;
using System.Data.OleDb;

namespace DScannerLibrary.DataAccess;

public class SqliteDataAccess : IDataAccess
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

    public int InsertData<T>(T item)
    {
        throw new NotImplementedException();
    }

    public List<T> ReadData<T>(string sqlCommand, OleDbParameter[] parameters)
    {
        // Convert OleDb parameters to SQLite parameters by replacing ? placeholders
        // with named parameters for SQLite compatibility
        var sqliteQuery = sqlCommand;
        for (int i = 0; i < parameters.Length; i++)
        {
            sqliteQuery = ReplaceFirst(sqliteQuery, "?", $"@p{i}");
        }

        var dataTable = new DataTable();

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            using (var command = new SqliteCommand(sqliteQuery, connection))
            {
                // Add SQLite parameters
                for (int i = 0; i < parameters.Length; i++)
                {
                    command.Parameters.AddWithValue($"@p{i}", parameters[i].Value ?? DBNull.Value);
                }

                using (var dataReader = command.ExecuteReader())
                {
                    dataTable.Load(dataReader);
                }
            }

            connection.Close();
        }

        return DataTableToListExtension.ConvertDataTable<T>(dataTable);
    }

    // Helper method to replace first occurrence of a string
    private static string ReplaceFirst(string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
}
