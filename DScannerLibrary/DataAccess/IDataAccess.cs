using System.Linq;

namespace DScannerLibrary.DataAccess;

public interface IDataAccess
{
    public int InsertData<T>(T item);

    public void InsertData(string rawSql);
}
