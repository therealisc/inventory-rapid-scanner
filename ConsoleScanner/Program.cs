using System.Data;
using System.Text;
using System.Data.OleDb;

if (IntPtr.Size == 8)
{
    Console.WriteLine("Sorry this is not going to work in 64 bits");
    return;
}

string constr = $"Provider=VFPOLEDB;Data Source=C:\\SAGA PS.3.0\\0001\\";
OleDbConnection con = new OleDbConnection();
con.ConnectionString = constr;
con.Open();

//Demo fox pro
DataTable ReadDbf(string path, string str_sql)
{
    OleDbDataAdapter adapter = new OleDbDataAdapter(str_sql, con);
    DataTable dt = new DataTable();
    adapter.Fill(dt);
    con.Close();
    return dt;
}

//var foxProDbf = ReadDbf(@"C:\Program Files (x86)\Common Files\System\Ole DB\vfpoledb.dll", "Select * from iesiri");
var foxProDbfDateTable = ReadDbf(constr, "Select top 1 * from ies_det order by id_u desc");

var foxProDbfAsList = foxProDbfDateTable.AsEnumerable().Select(e => new InventoryExitModel
{
    id_u = e.Field<decimal>(nameof(InventoryExitModel.id_u)) + 1,
    id_iesire = 15,
    gestiune = e.Field<string?>(nameof(InventoryExitModel.gestiune)),
    den_gest = e.Field<string?>(nameof(InventoryExitModel.den_gest)),
    cod = e.Field<string>(nameof(InventoryExitModel.cod)),
    denumire = e.Field<string>(nameof(InventoryExitModel.denumire)),
    den_tip = e.Field<string>(nameof(InventoryExitModel.den_tip)),
    um = e.Field<string>(nameof(InventoryExitModel.um)),
    tva_art = e.Field<decimal>(nameof(InventoryExitModel.tva_art)),
    cantitate = e.Field<decimal>(nameof(InventoryExitModel.cantitate)),
    pret_unitar = e.Field<decimal>(nameof(InventoryExitModel.pret_unitar)),
    valoare = e.Field<decimal>(nameof(InventoryExitModel.valoare)),
    tva_ded = e.Field<decimal>(nameof(InventoryExitModel.tva_ded)),
    total = e.Field<decimal>(nameof(InventoryExitModel.total)),
    adaos = e.Field<decimal>(nameof(InventoryExitModel.adaos)),
    tip_o = e.Field<string>(nameof(InventoryExitModel.tip_o)),
    cont = e.Field<string>(nameof(InventoryExitModel.cont)),
    text_supl = e.Field<string>(nameof(InventoryExitModel.text_supl)),
    discount = e.Field<decimal>(nameof(InventoryExitModel.discount)),
    plan = e.Field<string>(nameof(InventoryExitModel.plan)),
    sector = e.Field<string>(nameof(InventoryExitModel.sector)),
    sursa = e.Field<string>(nameof(InventoryExitModel.sursa)),
    articol = e.Field<string>(nameof(InventoryExitModel.articol)),
    capitol = e.Field<string>(nameof(InventoryExitModel.capitol)),
    categorie = e.Field<string>(nameof(InventoryExitModel.categorie)),
}).ToList();

foreach (var item in foxProDbfAsList)
{
    Console.WriteLine(item.id_u);
}

using (OleDbConnection myCon = new OleDbConnection(constr))
{
    OleDbCommand cmd = new OleDbCommand();
    cmd.CommandType = CommandType.Text;
    //cmd.CommandText = "insert into ies_det ([id_u],[id_iesire],[gestiune], [den_gest], [cod], [denumire],
    //[den_tip], [um],[cantitate], [pret_unitar], [valoare]) values (?,?,?,?,?,?,?,?,?,?,?)";
    //
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
    myCon.Open();
    cmd.ExecuteNonQuery();

    con.Close();
}
