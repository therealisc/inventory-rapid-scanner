using DScannerLibrary.Extensions;
using DScannerLibrary.Models;
using DScannerLibrary.DataAccess;

if (IntPtr.Size == 8)
{
    Console.WriteLine("Sorry this is not going to work in 64 bits");
    return;
}

Console.WriteLine("Type the id");
var articleCode = Console.ReadLine();

//00000001

var da = new DbfDataAccess();
var articleDataTable = da.ReadDbf($"Select * from articole where cod='{articleCode}'");
var articleList = articleDataTable.ConvertDataTable<ArticleModel>();

foreach (var item in articleList)
{
    Console.WriteLine($"{item.cod} {item.denumire}");
}

Console.ReadLine();


var foxProDbfDateTable = da.ReadDbf("Select top 1 * from ies_det order by id_u desc");

var foxProDbfAsList = foxProDbfDateTable.ConvertDataTable<InventoryExitModel>();

foreach (var item in foxProDbfAsList)
{
    Console.WriteLine(item.id_u);
}

//da.InsertIntoDbf<InventoryExitModel>();
