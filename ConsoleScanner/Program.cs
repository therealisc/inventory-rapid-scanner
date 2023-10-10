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

do
{
    var exitDocumentCheck = new ExitDocumentCheck();
    exitDocumentId = exitDocumentCheck.GetExitDocumentId();

    if (exitDocumentId == 0)
    {
        Console.WriteLine("Adauga in SAGA o iesire cu data de azi mai intai!");
        Thread.Sleep(5000);
    }

} while (exitDocumentId == 0);

Console.WriteLine("Asigura-te ca atunci cand scanezi produse ai selectat aceasta fereastra!");

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

    var articleSearchLogic = new ArticleSearchLogic();
    var article = articleSearchLogic.GetArticleByBarcode(barcode);

    var inventoryMovementsLogic = new InventoryMovementsLogic();

    // articole pe gestiuni: inventoryMovements va avea atatea randuri cate gestiuni sunt
    var inventoryMovements = inventoryMovementsLogic.GetInventoryMovements(article?.cod);

    var numberOfInventories = inventoryMovements.Count;
    if (numberOfInventories == 1)
    {
        Console.WriteLine($"Rows affected: {inventoryMovementsLogic.ProcessSingleInventoryExit(exitDocumentId, inventoryMovements, article, quantity)}");
    }

    if (numberOfInventories > 1)
    {
        // Iesire alternativa, cate o bucata din fiecare gestiune pe rand!

        // Daca exista, caut daca mai exista o pozitie din acelasi produs
        // daca nu, adaug o pozitie cu cantitatea 1 sau adaug mai multe pozitii (maximum cate gestiuni sunt) in distribui alternativ cantitatea
        // daca mai exista pozitii din nou continui cu urmatorea gestiune 
        // ATENTIE! conteaza cantitatea pe gestiune deci o gestiune poate avea mai putine bucati => cand nu mai are nu mai descarc de acolo
        Console.WriteLine("More than one inventory");
    }

    if (numberOfInventories == 0)
    {
        Console.WriteLine("Nu au fost gasite intrari pentru acest produs!");
        if (article != null)
        {
            Console.WriteLine($"Adauga intrari in SAGA pentru {article?.denumire?.Trim()}.");
        }
    }
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

