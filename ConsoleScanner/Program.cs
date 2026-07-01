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

string? finalSplit = null;

foreach (var dbfLine in dbfLines)
{
	var lineSplit = dbfLine.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
	finalSplit = lineSplit[1].ToString().Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
	Console.WriteLine(finalSplit[1]);
}

var sql = $@"CREATE TABLE {tableName} (
		Id int PRIMARY KEY,
		Denumire varchar(255) NOT NULL
		)";

dataAccess.InsertData(sql);

int counter = 0;
foreach (var dbfLine in dbfLines)
{
	//var uniqueId = Guid.NewGuid();
	counter++;
	var numberId = $"{counter}";

	// the sql db converted varchar to int down below 
	sql = $@"INSERT INTO { tableName } (Id, Denumire) VALUES ({ numberId }, { finalSplit })";
	dataAccess.InsertData(sql);
}

Console.WriteLine("Id available:");
sql = $@"SELECT * FROM { tableName }";

var entries = dataAccess.ReadData<DisplayModel>(sql);
entries.ForEach(d => Console.WriteLine(d.Id));
entries.ForEach(d => Console.WriteLine(d.Denumire));

return;
