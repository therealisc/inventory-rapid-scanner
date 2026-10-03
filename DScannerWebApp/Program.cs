using RCommerce.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Mapster;
using System.Security.Claims;
//HACK:
using DScannerLibrary.Helpers;
using DScannerLibrary.Models;
using DScannerLibrary.BusinessLogic;
using DScannerLibrary.DataAccess;
using System;
using System.Threading;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Login";
        o.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
});

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.StartsWith("postgresql://"))
{
    Console.WriteLine("WARNING: PostgreSQL DATABASE_URL detected but application uses SQLite. Falling back to local SQLite database.");
    connectionString = "Data Source=rcommerce.db";
}
else
{
    connectionString = databaseUrl ?? config.GetConnectionString("DefaultConnection") ?? "Data Source=rcommerce.db";
}

builder.Services.AddDbContext<RCommerceContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddMapster();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RCommerceContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseSwagger();
    app.UseSwaggerUI();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


//HACK:
var articleSearchLogic = new ArticleSearchLogic();
var nullDataAccess = new NullDataAccess();
var dataAccess = new SqliteDataAccess();
var dbfDataAccess = new DbfDataAccess();
var tableName = "articole";
var dbfLines = dbfDataAccess.ReadDbf($"{ tableName }.dbf");

var storesTable = "gestiuni";
var dbfStores = dbfDataAccess.ReadDbf($"{ storesTable }.dbf");

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
		);
		
		CREATE TABLE { storesTable } (
		Id int PRIMARY KEY,
		Name varchar(255) NOT NULL,
		Quantity decimal NOT NULL
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
	
	//var uniqueId = Guid.NewGuid();
	counter++;
	var numberId = $"{counter}";

	sql = $@"INSERT INTO { tableName } (Id, Name, Barcode) 
		VALUES ({ numberId }, '{ itemName }', '{ itemBarcode }' );
		
		-- INSERT INTO { storesTable } ( Id, Name,  )
		
		
		";
	
	dataAccess.InsertData(sql);
}
return;
app.Run();
