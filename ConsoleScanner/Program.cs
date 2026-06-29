using DScannerLibrary.Helpers;
using DScannerLibrary.Models;
using DScannerLibrary.BusinessLogic;
using DScannerLibrary.DataAccess;
using System;
using System.Threading;
using System.Linq;

var articleSearchLogic = new ArticleSearchLogic();
var nullDataAccess = new NullDataAccess();
var dataAccess = new SqliteDataAccess();
var dbfDataAccess = new DbfDataAccess();

if (Environment.OSVersion.ToString().Contains("Unix"))
{
	return;
}

Console.WriteLine("--- loadin dbf ---");
var tableName = "articole";
var dbfLines = dbfDataAccess.ReadDbf($"{tableName}.dbf");

Console.WriteLine(dbfLines.Count);
foreach (var dbfLine in dbfLines)
{
	Console.WriteLine(dbfLine.ToString());
}

var sql = $@"CREATE TABLE {tableName} (
		id int PRIMARY KEY
		)";

dataAccess.InsertData(sql);

int counter = 0;
foreach (var dbfLine in dbfLines)
{
	//var uniqueId = Guid.NewGuid();
	counter++;

	var numberId = $"{counter}";
	Console.WriteLine(numberId);

	// the sql db converted varchar to int down below 
	sql = $@"INSERT INTO {tableName} (id) VALUES ({numberId})";
	dataAccess.InsertData(sql);
}

Console.WriteLine("Inventory available:");

sql = $@"
	    SELECT * FROM {tableName}";

var entries = dataAccess.ReadData<List<int>>(sql);

foreach (int entry in entries)
{
	Console.WriteLine(entry);
}

return;

//async Task AddInventoryExit()
//{
    //var inventoryMovementsLogic = new InventoryMovementsLogic(dataAccess, articleSearchLogic, null);

    // searching in miscari.dbf => gestiunile
    //var article = new ArticleModel()
    //{
	    //cod = "00000002", 
    //};

    //var articleInventoryMovements = inventoryMovementsLogic.GetInventoryMovementsForArticle(article.cod);
    //var rows = await inventoryMovementsLogic.CreateMultipleExits(1, article, 72807, articleInventoryMovements);

    //Console.WriteLine(rows);
//}

//decimal RequestInventoryEntry()
//{
    //decimal newEntry = 1;
    //string? quantityAsString = "";

    //do
    //{
        //Console.WriteLine("New entry - Press 1 or new exit - Press 0");
        //quantityAsString = Console.ReadLine();

    //} while (string.IsNullOrWhiteSpace(quantityAsString) || Decimal.TryParse(
	//quantityAsString, out decimal result) == false);

    //newEntry = Convert.ToDecimal(quantityAsString);

    //return newEntry;
//}
