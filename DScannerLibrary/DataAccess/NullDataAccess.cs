using System.Data;
using System.Data.OleDb;

namespace DScannerLibrary.DataAccess;

public class NullDataAccess : IDataAccess
{
	public List<T> ReadData<T>(string query) => new List<T>();

	public List<T> ReadData<T>(string sqlCommand, OleDbParameter[] parameters) => new List<T>();

	public int InsertData<T>(T item) => 0;

	public void InsertData(string rawSql) { }
}
