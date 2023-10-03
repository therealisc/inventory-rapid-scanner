using System.Data;
using System.Text;
using ExcelDataReader;
using System.Data.OleDb;
using Microsoft.Win32;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

var excelFilePath = Path.GetFullPath("C:\\Users\\therealisc\\source\\repos\\inventory-rapid-scanner\\xls-609-122312.xls");

using var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read);

using var reader = ExcelReaderFactory.CreateReader(stream);

using (reader)
{
    // 2. Use the AsDataSet extension method
    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
    {
        // Gets or sets a value indicating whether to set the DataColumn.DataType 
        // property in a second pass.
        UseColumnDataType = true,

        // Gets or sets a callback to determine whether to include the current sheet
        // in the DataSet. Called once per sheet before ConfigureDataTable.
        FilterSheet = (tableReader, sheetIndex) => true,

        // Gets or sets a callback to obtain configuration options for a DataTable. 
        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
        {
            // Gets or sets a value indicating the prefix of generated column names.
            EmptyColumnNamePrefix = "Column",

            // Gets or sets a value indicating whether to use a row from the 
            // data as column names.
            // set to false to include column names from xls
            UseHeaderRow = true,

            // Gets or sets a callback to determine which row is the header row. 
            // Only called when UseHeaderRow = true.
            //ReadHeaderRow = (rowReader) =>
            //{
            //    // F.ex skip the first row and use the 2nd row as column headers:
            //    rowReader.Read();
            //},

            // Gets or sets a callback to determine whether to include the 
            // current row in the DataTable.
            //FilterRow = (rowReader) =>
            //{
            //    return true;
            //},

            // Gets or sets a callback to determine whether to include the specific
            // column in the DataTable. Called once per column after reading the 
            // headers.
            //FilterColumn = (rowReader, columnIndex) =>
            //{
            //    return true;
            //}
        }
    });

    var firstTable = result.Tables[0];


    static bool IsInstalled()
    {
        return Registry.ClassesRoot.OpenSubKey("TypeLib\\{50BAEECA-ED25-11D2-B97B-000000000000}") != null;
    }

    if (IntPtr.Size == 8)
    {
        Console.WriteLine("Sorry this is not going to work in 64 bits");
        return;
    }

    Console.WriteLine();
    Console.WriteLine(IsInstalled());
    Console.WriteLine();

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

    //foreach (DataRow dataRow in foxProDbf.Rows)
    //{
    //    foreach (var item in dataRow.ItemArray)
    //    {
    //        Console.Write($"{item} ");
    //    }

    //    Console.WriteLine();
    //}

    using (OleDbConnection myCon = new OleDbConnection(constr))
    {
        OleDbCommand cmd = new OleDbCommand();
        cmd.CommandType = CommandType.Text;
        //cmd.CommandText = "insert into ies_det ([id_u],[id_iesire],[gestiune], [den_gest], [cod], [denumire],[den_tip], [um],[cantitate], [pret_unitar], [valoare]) values (?,?,?,?,?,?,?,?,?,?,?)";

        //var commandText = "insert into ies_det (";

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

        //cmd.Parameters.AddWithValue("@price", 15);
        //cmd.Parameters.AddWithValue("@gestiune", "0002");
        //cmd.Parameters.AddWithValue("@den_gestiune", "ARHIEP");
        //cmd.Parameters.AddWithValue("@cod", "00000027");
        //cmd.Parameters.AddWithValue("@denumire", "BIBILIA 60");
        //cmd.Parameters.AddWithValue("@den_tip", "BIBILIA 60");
        //cmd.Parameters.AddWithValue("@um", "");
        //cmd.Parameters.AddWithValue("@cantitate", 1);
        //cmd.Parameters.AddWithValue("@pret_unitar", 1);
        //cmd.Parameters.AddWithValue("@valoare", 1);
        cmd.CommandText = commandText.ToString();

        cmd.Connection = myCon;
        myCon.Open();
        cmd.ExecuteNonQuery();

        //System.Windows.Forms.MessageBox.Show("An Item has been successfully added", "Caption", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

        con.Close();
    }

    Console.WriteLine();
    Console.WriteLine();

    //lot	data_intr	den_gest	denumire	
    //cod_art	um	den_tip	cant_init	cant_int	
    //pret_vanz	cant_ies	cant_fin	com_int	comandat	disponibil	pret_mediu	val_init	val_int	val_ies	valoare	garantie	inf_lot

    List<Article> listName = firstTable.AsEnumerable().Select(m => new Article()
    {
        lot = m.Field<string>("lot"),
        data_intr = m.Field<string>("data_intr"),
        den_gest = m.Field<string>("den_gest"),
        denumire = m.Field<string>("denumire"),
    }).ToList();

    foreach (var item in listName)
    {
        Console.WriteLine($"{item.denumire} {item.den_gest}");
    }
}

public class Article
{
    public string? lot { get; set; }
    public string? data_intr { get; set; }
    public string? den_gest { get; set; }
    public string? denumire { get; set; }
}

