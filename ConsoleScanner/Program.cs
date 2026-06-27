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
	Console.WriteLine("--- Convert existing fox pro database to a relational database ---");

	AddIesiriTable();

    var inventoryMovementsLogic = new InventoryMovementsLogic(nullDataAccess, articleSearchLogic, new ExitDocumentCheck(nullDataAccess));

    inventoryMovementsLogic.AddTemporaryDb();
	
    await DisplayArticles();

    while (true)
    {
		var input = RequestInventoryEntry().ToString();
		if (input.Contains("1"))
		{
				AddInventoryEntry();
		}

		if (input.Contains("0"))
		{
				await AddInventoryExit();
		}

		await DisplayArticles();
    }
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

return;

async Task DisplayArticles()
{
    Console.WriteLine("Inventory available:");

    var sql = $@"
	    SELECT * FROM intr_det";

    var entries = dataAccess.ReadData<OperationalInventoryModel>(sql);

    Console.WriteLine("cod gestiune cantitate");

    foreach (OperationalInventoryModel entry in entries)
    {
	    Console.WriteLine($"{entry.cod} {entry.gestiune} {entry.cantitate}");
    }
}

async Task AddInventoryExit()
{
    var inventoryMovementsLogic = new InventoryMovementsLogic(dataAccess, articleSearchLogic, null);

    // searching in miscari.dbf => gestiunile
    var article = new ArticleModel()
    {
	    cod = "00000002", 
    };

    var articleInventoryMovements = inventoryMovementsLogic.GetInventoryMovementsForArticle(article.cod);
    var rows = await inventoryMovementsLogic.CreateMultipleExits(1, article, 72807, articleInventoryMovements);

    Console.WriteLine(rows);
}

decimal RequestInventoryEntry()
{
    decimal newEntry = 1;
    string? quantityAsString = "";

    do
    {
        Console.WriteLine("New entry - Press 1 or new exit - Press 0");
        quantityAsString = Console.ReadLine();

    } while (string.IsNullOrWhiteSpace(quantityAsString) || Decimal.TryParse(
	quantityAsString, out decimal result) == false);

    newEntry = Convert.ToDecimal(quantityAsString);

    return newEntry;
}

void AddIesiriTable()
{
	var sql = $@"CREATE TABLE iesiri (
		id_iesire INTEGER NOT NULL
		)";

    dataAccess.InsertData(sql);
}

void AddInventoryEntry()
{
    Console.WriteLine("Type the article code:");
    string? code = Console.ReadLine();

    var sql = $@"
	    INSERT INTO intr_det (cod, gestiune, cantitate)
	    VALUES ('{code}', '0001', 1)";

    dataAccess.InsertData(sql);
}
