using System.Data;

namespace DScannerLibrary.DataAccess;

public class NullDataAccess
{
	public List<T> ReadData<T>(string query) => new List<T>();

	public int InsertData<T>(T item) => 0;

	public void InsertData(string rawSql) { }
}
