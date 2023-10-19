using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;

namespace DScanner;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo("ro");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("ro");
	}
}

