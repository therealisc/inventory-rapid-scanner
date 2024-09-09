using DScannerLibrary.BusinessLogic;
using DScannerLibrary.DataAccess;
using System;
using System.Linq;


var articleSearchLogic = new ArticleSearchLogic(null);

if (Environment.OSVersion.ToString().Contains("Unix"))
{
	Console.WriteLine(Environment.OSVersion);
	var inventoryMovementsLogic = new InventoryMovementsLogic(null, articleSearchLogic, null);

        //var articleInventoryMovements = inventoryMovementsLogic.GetInventoryMovementsForArticle("00001381");
        var articleInventoryMovements = inventoryMovementsLogic.GetInventoryMovementsForArticle("00000004");
	var articleInventoriesAsDict = inventoryMovementsLogic.AvailableInventoryAsDictionary(articleInventoryMovements);

	foreach (var articleTotals in articleInventoriesAsDict)
	{
		Console.WriteLine(articleTotals.Key);
		Console.WriteLine(articleTotals.Value);
		Console.WriteLine();
	}

	Console.WriteLine(articleInventoriesAsDict.Sum(x => x.Value));

	return;
}

//var dbDirectory = "/home/therealisc/0003";
articleSearchLogic = new ArticleSearchLogic(new DbfDataAccess());
var inventoryMovements = new InventoryMovementsLogic(new DbfDataAccess(), articleSearchLogic, null);

var inventory = inventoryMovements.GetInventoryExitsByDate(new DateTime(2024, 08, 27));
//var inventory = inventoryMovements.GetInventoryExitsByDate(dbDirectory, new DateTime(2024, 08, 25));

//var article = articleSearchLogic.GetArticleByBarcode("9789731363882");
//Console.WriteLine($"c:#{article.cod}# d:#{article.denumire}# t:{article.tva} p:{article.pret_vanz}");

inventory.ForEach(x 
    => Console.WriteLine(
        $"line no:{inventory.IndexOf(x) + 1} _id:{x.id_iesire}-{x.cod}-{x.denumire}\n{x.pret_unitar} {x.valoare} {x.cantitate} {x.cont} {x.total} {x.adaos} {x.text_supl}"));


Console.WriteLine($"Rows affected: {await inventoryMovements.GenerateInventoryExits("9789731363882", 1)}");

return;
while (true)
{
    string? barcode;

    var quantity = InputQuantity();

    if (quantity > 10000)
    {
        barcode = quantity.ToString();
        quantity = 1; // default quantity is 1
    }
    else
    {
        barcode = InputBarCode();
    }

    Console.WriteLine(barcode);

    Console.WriteLine($"Rows affected: {await inventoryMovements.GenerateInventoryExits(barcode, quantity)}");
}

string InputBarCode()
{
    string? articleBarCode = "";

    do
    {
        Console.WriteLine("Enter the barcode:");
        articleBarCode = Console.ReadLine();

    } while (string.IsNullOrWhiteSpace(articleBarCode) || Decimal.TryParse(articleBarCode, out decimal result) == false);

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
        Console.WriteLine("Introdu cantitatea (dupa care apasa ENTER) sau scaneaza un articol si cantitatea va fi implicit 1:");
        quantityAsString = Console.ReadLine();

    } while (string.IsNullOrWhiteSpace(quantityAsString) || Decimal.TryParse(quantityAsString, out decimal result) == false || result == 0);

    quantity = Convert.ToDecimal(quantityAsString);

    return quantity;
}
