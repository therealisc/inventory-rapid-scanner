using DScannerLibrary.BusinessLogic;
using DScannerLibrary.DataAccess;
using DScannerLibrary.Helpers;
using DScannerLibrary.Models;
using DScannerLibrary.Services;
using System;
using System.Threading;
using System.Linq;

var articleSearchLogic = new ArticleSearchLogic(null);

if (Environment.OSVersion.ToString().Contains("Unix"))
{
    await AddDb();
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

//Env.OSV
var emailService = new EmailService();
await emailService.SendMailAsync("ruporfcjpzndjzhk");
return;

async Task AddDb()
{
	var sql = @"

		DROP TABLE intr_det;
	DROP TABLE ies_det;
	DROP TABLE special;

	CREATE TABLE intr_det (
			id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
			cod TEXT NOT NULL,
			gestiune TEXT NOT NULL,
			cantitate INTEGER NOT NULL
			);


	CREATE TABLE ies_det (
			id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
			id_iesire INTEGER NOT NULL,
			cod TEXT NOT NULL,
			gestiune TEXT NOT NULL,
			cantitate INTEGER NOT NULL
			);

	CREATE TABLE special (
			id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
			cod TEXT NOT NULL,
			gestiune TEXT NOT NULL,
			cantitate INTEGER NOT NULL,
			operatie TEXT NOT NULL
			);

	INSERT INTO intr_det
		VALUES (1, '00001381', '0001', 818),
		       (2, '00001381', '0002', 200),
		       (3, '00000001', '0002', 20),
		       (4, '00000810', '0001', 20),
		       (5, '00000002', '0001', 323),
		       (6, '00000001', '0001', 13);

	INSERT INTO ies_det
		VALUES (1, 72807, '00001381', '0001', 1),
		       (2, 72807, '00001381', '0002', 1),
		       (3, 72807, '00000001', '0002', 1),
		       (4, 72807, '00000001', '0001', 1);

	INSERT INTO special
		VALUES (1, '00001381', '0001', 4, 'Descarcare cantitate'),
		       (2, '00001381', '0002', 1, 'Descarcare cantitate'),
		       (3, '00001381', '0002', 1, 'Incarcare cantitate'),
		       (4, '00001381', '0002', 2, 'Incarcare cantitate'),
		       (5, '00001381', '0002', 2, 'Incarcare cantitate'),
		       (6, '00000001', '0002', 1, 'Descarcare cantitate'),
		       (7, '00000001', '0001', 2, 'Descarcare cantitate');
	";

	var dataAccess = new SqliteDataAccess();
	//dataAccess.InsertData(sql);
}

async Task DisplayArticles()
{
    Console.WriteLine("Inventory available:");

    var sql = $@"
	    SELECT * FROM intr_det";

    var dataAccess = new SqliteDataAccess();
    var entries = dataAccess.ReadData<OperationalInventoryModel>(sql);

    Console.WriteLine("cod gestiune cantitate");

    foreach (OperationalInventoryModel entry in entries)
    {
	Console.WriteLine($"{entry.cod} {entry.gestiune} {entry.cantitate}");
    }
}

async Task AddInventoryExit()
{
    var dataAccess = new SqliteDataAccess();
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

void AddInventoryEntry()
{
    Console.WriteLine("Type the article code:");
    string? code = Console.ReadLine();

    var sql = $@"
	    INSERT INTO intr_det (cod, gestiune, cantitate)
	    VALUES ('{code}', '0001', 1)";

    var dataAccess = new SqliteDataAccess();
    dataAccess.InsertData(sql);
}
