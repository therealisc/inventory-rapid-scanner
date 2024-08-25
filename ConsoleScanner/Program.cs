using DScannerLibrary.BusinessLogic;
using DScannerLibrary.DataAccess;
using System;
using System.Linq;

var dbDirectory = "/home/therealisc/0003";
var inventoryMovements = new InventoryMovementsLogic(null, new ArticleSearchLogic(null, dbDirectory), null, dbDirectory);

var inventory = inventoryMovements.GetInventoryExitsByDate(new DateTime(2024, 08, 23));

inventory.ForEach(x => Console.WriteLine($"{x.id_iesire}-{x.cod}-{x.denumire}\n{x.um} {x.pret_unitar} {x.valoare} {x.cantitate} {x.cont} {x.total} {x.adaos} {x.text_supl}"));

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
