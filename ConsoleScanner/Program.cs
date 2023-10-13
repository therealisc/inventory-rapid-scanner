using DScannerLibrary.BusinessLogic;

Console.BackgroundColor = ConsoleColor.White;
Console.Clear();
Console.ForegroundColor = ConsoleColor.Black;

if (IntPtr.Size == 8)
{
    Console.WriteLine("Sorry this is not going to work in 64 bits");
    return;
}

decimal exitDocumentId = 0;

Console.WriteLine("Asigura-te ca atunci cand scanezi produse ai selectat aceasta fereastra!\n");

while (true)
{
    exitDocumentId = GetExitDocument();

    if (exitDocumentId == 0)
    {
        continue;
    }

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

    var inventoryMovements = new InventoryMovementsLogic();
    Console.WriteLine($"Rows affected: {await inventoryMovements.ProcessInventoryExit(exitDocumentId, barcode, quantity)}");
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

decimal GetExitDocument()
{
    var exitDocumentCheck = new ExitDocumentCheck();
    var exitDocumentId = exitDocumentCheck.GetExitDocumentId();

    if (exitDocumentId == 0)
    {
        Console.WriteLine("Adauga in SAGA o iesire cu data de azi mai intai!\n");
        Console.WriteLine("Asigura-te ca documentul de iesire nu este validat!\n");
        Thread.Sleep(3000);
    }
    return exitDocumentId;
}

