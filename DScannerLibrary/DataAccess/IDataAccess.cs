using System.Linq;
using System.Data.OleDb;

namespace DScannerLibrary.DataAccess;

public interface IDataAccess
{
    public List<T> ReadData<T>(string query);

    public List<T> ReadData<T>(string sqlCommand, OleDbParameter[] parameters);

    public int InsertData<T>(T item);
}
