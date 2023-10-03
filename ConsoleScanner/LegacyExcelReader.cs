using System.Data;
using ExcelDataReader;

namespace ConsoleScanner;

public class LegacyExcelReader
{
    public static void ReadExel()
    {
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

            //foreach (DataRow dataRow in foxProDbf.Rows)
            //{
            //    foreach (var item in dataRow.ItemArray)
            //    {
            //        Console.Write($"{item} ");
            //    }

            //    Console.WriteLine();
            //}
        }
    }

    public class Article
    {
        public string? lot { get; set; }
        public string? data_intr { get; set; }
        public string? den_gest { get; set; }
        public string? denumire { get; set; }
    }
}
