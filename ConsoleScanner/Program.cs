//using DScannerLibrary.BusinessLogic;
using DbfReaderNET;
using System;



var dbf = new Dbf();
string dbfPath = "/home/therealisc/sagadb/ARTICOLE.DBF";
dbf.Read(dbfPath);

string cod = DateTime.Now.ToString();

DbfRecord r = dbf.CreateRecord();

r.Data[0] = cod;
r.Data[1] = "CARTI COPII";

dbf.Write(dbfPath, DbfVersion.VisualFoxPro);

foreach(DbfRecord record in dbf.Records) {
    for(int i = 0;  i < dbf.Fields.Count; i++) {
        Console.Write(record[i]);
        Console.Write(" ");
    }

    Console.WriteLine();
}




foreach(DbfField field in dbf.Fields) {
	Console.WriteLine(field.Name);
}

//while (true)
//{
//    exitDocumentId = GetExitDocument();
//
//    if (exitDocumentId == 0)
//    {
//        continue;
//    }
//
//    string? barcode;
//
//    var quantity = InputQuantity();
//    if (quantity > 10000)
//    {
//        barcode = quantity.ToString();
//        quantity = 1; // default quantity is 1
//    }
//    else
//    {
//        barcode = InputBarCode();
//    }
//
//	exitDocumentId = GetExitDocument();
//
//	if (exitDocumentId == 0)
//	{
//		continue;
//	}
//
//	//var inventoryMovements = new InventoryMovementsLogic(null, null, null);
//    //inventoryMovements.GetInventoryMovementsForArticle("00000376");
//	//var inventoryMovements = new InventoryMovementsLogic();
//    //Console.WriteLine($"Rows affected: {await inventoryMovements.GenerateInventoryExits(barcode, quantity)}");
//}

//string InputBarCode()
//{
//    string? articleBarCode = "";
//
//    do
//    {
//        Console.WriteLine("Enter the barcode:");
//        articleBarCode = Console.ReadLine();
//
//    } while (string.IsNullOrWhiteSpace(articleBarCode) || Decimal.TryParse(articleBarCode, out decimal result) == false);
//
//    if (articleBarCode == "exit")
//    {
//        Environment.Exit(0);
//    }
//
//    return articleBarCode;
//}
//
//decimal InputQuantity()
//{
//    decimal quantity = 1;
//    string? quantityAsString = "";
//
//    do
//    {
//        Console.WriteLine("Introdu cantitatea (dupa care apasa ENTER) sau scaneaza un articol si cantitatea va fi implicit 1:");
//        quantityAsString = Console.ReadLine();
//
//    } while (string.IsNullOrWhiteSpace(quantityAsString) || Decimal.TryParse(quantityAsString, out decimal result) == false || result == 0);
//
//    quantity = Convert.ToDecimal(quantityAsString);
//
//    return quantity;
//}
//
//decimal GetExitDocument()
//{
//    var exitDocumentCheck = new ExitDocumentCheck(new DScannerLibrary.DataAccess.DbfDataAccess());
//    var exitDocumentId = exitDocumentCheck.GetExitDocumentId();
//
//    if (exitDocumentId == 0)
//    {
//        Console.WriteLine("Adauga in SAGA o iesire cu data de azi mai intai!\n");
//        Console.WriteLine("Asigura-te ca documentul de iesire nu este validat!\n");
//        Thread.Sleep(3000);
//    }
//    return exitDocumentId;
//}

