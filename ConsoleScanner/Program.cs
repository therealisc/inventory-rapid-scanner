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

//if (Environment.OSVersion.ToString().Contains("Unix"))
//{
	//return;
//}

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

	// the sql db converted varchar to int down below 
	sql = $@"INSERT INTO {tableName} (id) VALUES ({numberId})";
	dataAccess.InsertData(sql);
}

Console.WriteLine("Id available:");
sql = $@"SELECT * FROM {tableName}";

var entries = dataAccess.ReadData<List<int>>(sql);
//string result = string.Join(", ", entries);

Console.WriteLine(entries.First().ToString()); 
//entries.ForEach(d => Console.WriteLine(d));
return;
