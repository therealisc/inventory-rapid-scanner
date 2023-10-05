using System.Data;
using System.Text;
using System.Data.OleDb;

namespace DScannerLibrary.DataAccess;

public class DbfDataAccess
{
    string connectionString = $"Provider=VFPOLEDB;Data Source=C:\\SAGA PS.3.0\\0002\\";

    public DataTable ReadDbf(string str_sql)
    {
        using (var connection = new OleDbConnection(connectionString))
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter(str_sql, connection);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }
    }

    public int InsertIntoIesiriDbf<T>(T item)
    {
        using (OleDbConnection myCon = new OleDbConnection(connectionString))
        {
            var command = new OleDbCommand();
            //command.CommandType = CommandType.Text;
            StringBuilder commandText = new("insert into ies_det (");
            Type t = item.GetType();
            //Console.WriteLine("Type is: {0}", t.Name);
            var props = t.GetProperties();
            //Console.WriteLine("Properties (N = {0}):", props.Length);

            foreach (var prop in props)
            {
                commandText.Append($"[{prop.Name}],");

                if (prop.GetIndexParameters().Length == 0)
                {
                    command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(item));
                    //Console.WriteLine("   {0} ({1}): {2}", prop.Name, prop.PropertyType.Name, prop.GetValue(item));
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
