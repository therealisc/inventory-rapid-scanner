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

string InputBarCode()
{
    string? articleBarCode = "";

    do
    {
        Console.WriteLine("Enter the barcode:");
        articleBarCode = Console.ReadLine();

    } while (string.IsNullOrWhiteSpace(articleBarCode) || Decimal.TryParse(
	articleBarCode, out decimal result) == false);

    if (articleBarCode == "exit")
    {
        Environment.Exit(0);
    }

    return articleBarCode;
}

decimal InputQuantity()
{
    decimal quantity = 1;
    string? quantityAsString = "";

    do
    {
        Console.WriteLine("Introdu cantitatea (apoi apasa ENTER) sau scaneaza un articol si cantitatea va fi 1:");
        quantityAsString = Console.ReadLine();

    } while (string.IsNullOrWhiteSpace(quantityAsString) || Decimal.TryParse(
	quantityAsString, out decimal result) == false || result == 0);

    quantity = Convert.ToDecimal(quantityAsString);

    return quantity;
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
