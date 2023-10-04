using System.Data;
using System.Text;
using System.Data.OleDb;
using DScannerLibrary.Extensions;
using DScannerLibrary.Models;
using DScannerLibrary.DataAccess;

if (IntPtr.Size == 8)
{
    Console.WriteLine("Sorry this is not going to work in 64 bits");
    return;
}

var da  = new DbfDataAccess();
var foxProDbfDateTable = da.ReadDbf("Select top 1 * from ies_det order by id_u desc");

var foxProDbfAsList = foxProDbfDateTable.ConvertDataTable<InventoryExitModel>();

foreach (var item in foxProDbfAsList)
{
    Console.WriteLine(item.id_u);
}

return;

using (OleDbConnection myCon = new OleDbConnection(null))
{
    OleDbCommand cmd = new OleDbCommand();
    cmd.CommandType = CommandType.Text;

    StringBuilder commandText = new("insert into ies_det (");

    Type t = foxProDbfAsList.First().GetType();
    Console.WriteLine("Type is: {0}", t.Name);
    var props = t.GetProperties();

    Console.WriteLine("Properties (N = {0}):",
                      props.Length);

    foreach (var prop in props)
    {
        commandText.Append($"[{prop.Name}],");

        if (prop.GetIndexParameters().Length == 0)
        {
            cmd.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(foxProDbfAsList.First()));

            Console.WriteLine("   {0} ({1}): {2}", prop.Name,
                              prop.PropertyType.Name,
                              prop.GetValue(foxProDbfAsList.First()));
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

    cmd.CommandText = commandText.ToString();
    cmd.Connection = myCon;
    cmd.ExecuteNonQuery();
}
