using DScannerLibrary.Helpers;
using DScannerLibrary.Models;
using DScannerLibrary.BusinessLogic;
using DScannerLibrary.DataAccess;
using ConsoleScanner.Models;
using System;
using System.Threading;
using System.Linq;

var articleSearchLogic = new ArticleSearchLogic();
var nullDataAccess = new NullDataAccess();
var dataAccess = new SqliteDataAccess();
var dbfDataAccess = new DbfDataAccess();

//if (Environment.OSVersion.ToString().Contains("Unix"))
//{
	//return;
//}

Console.WriteLine("--- loadin dbf ---");
var tableName = "articole";
var dbfLines = dbfDataAccess.ReadDbf($"{ tableName }.dbf");

var storesTable = "gestiuni";
var dbfStores = dbfDataAccess.ReadDbf($"{ storesTable }.dbf");

// TODO: refactor
foreach (var dbfStore in dbfStores)
{
	Console.WriteLine(dbfStore.ToString());
}

string itemName = "";
string itemBarcode = "";

var sql = $@"CREATE TABLE { tableName } (
		Id int PRIMARY KEY,
		Name varchar(255) NOT NULL,
		Barcode varchar(255) NOT NULL
		)
		
		CREATE TABLE { storesTable } (
		Id int PRIMARY KEY
		)";

dataAccess.InsertData(sql);

int counter = 0;
foreach (var dbfLine in dbfLines)
{
	var lineSplit = dbfLine.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
	var nameArray = lineSplit[1].ToString().Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
	var barcodeArray = lineSplit[14].ToString().Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

	itemName = nameArray[1];
	itemBarcode = barcodeArray[1];
	
	Console.WriteLine(itemName);
	Console.WriteLine(itemBarcode);
	
	//var uniqueId = Guid.NewGuid();
	counter++;
	var numberId = $"{counter}";

	// the sql db converted varchar to int down below 
	sql = $@"INSERT INTO { tableName } (Id, Name, Barcode) VALUES ({ numberId }, '{ itemName }', '{ itemBarcode }' )";
	dataAccess.InsertData(sql);
}

Console.WriteLine("--- Rows available ---");
sql = $@"SELECT * FROM { tableName }";

var entries = dataAccess.ReadData<DisplayModel>(sql);
entries.ForEach(d => Console.WriteLine($"{ d.Id } { d.Name } { d.Barcode }"));

return;
