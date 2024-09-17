using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;
using DScannerLibrary.DataAccess;
using DScannerLibrary.BusinessLogic;

namespace DScanner;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Thread.CurrentThread.CurrentCulture = new CultureInfo("ro");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("ro");

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(e.Args);
        builder.Services.AddTransient<IDataAccess, DbfDataAccess>();
        builder.Services.AddTransient<InventoryMovementsLogic>();
        builder.Services.AddTransient<ArticleSearchLogic>();
        builder.Services.AddTransient<ExitDocumentCheck>();

        using IHost host = builder.Build();

        var window = new MainWindow(host.Services.GetRequiredService<InventoryMovementsLogic>());
        window.Show();
    }
}

