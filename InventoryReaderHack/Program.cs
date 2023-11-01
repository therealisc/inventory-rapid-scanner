using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Data.OleDb;
using System.Text.RegularExpressions;

public class Program
{
    class InventoryMovementModel
    {
        public string? cod_art { get; set; }
        public string gestiune { get; set; } = null!;
        public decimal cantitate { get; set; }
    }

    public static int Main(string[] args)
    {
        var inventoryMovements = ReadDbf<InventoryMovementModel>($"Select cod_art, gestiune, SUM(cantitate) as cantitate from miscari " +
            $"where cod_art='{args[0]}' group by cod_art, gestiune order by gestiune");

        var json = JsonSerializer.Serialize(inventoryMovements);
        Console.WriteLine(json);
        return 0;
    }

    static string GetConnectionString()
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

    static List<T> ReadDbf<T>(string str_sql)
    {
        using (var connection = new OleDbConnection(GetConnectionString()))
        {
            connection.Open();

            using var adapter = new OleDbDataAdapter(str_sql, connection);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            connection.Close();

            return Extensions.ConvertDataTable<T>(dataTable);
        }
    }

}

static class Extensions
{
    public static List<T> ConvertDataTable<T>(this DataTable dt)
    {
        List<T> data = new List<T>();
        foreach (DataRow row in dt.Rows)
        {
            T item = GetItem<T>(row);
            data.Add(item);
        }
        return data;
    }

    private static T GetItem<T>(DataRow dr)
    {
        Type temp = typeof(T);
        T obj = Activator.CreateInstance<T>();
        foreach (DataColumn column in dr.Table.Columns)
        {
            foreach (PropertyInfo pro in temp.GetProperties())
            {
                if (dr[column.ColumnName] != System.DBNull.Value && pro.Name == column.ColumnName)
                    pro.SetValue(obj, dr[column.ColumnName], null);
                else
                    continue;
            }
        }
        return obj;
    }
}
