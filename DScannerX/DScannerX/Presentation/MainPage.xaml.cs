using DScannerLibrary.Models;
using System.Linq;

namespace DScannerX.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        var _productsCache = new ProductsCache();
	var date = new DateTime(2024, 8, 12);

        var data = new List<InventoryExitModel>(_productsCache.GetRecords(DbfFilePath.Text, date));
	ListX.ItemsSource = data.Select(x => x.denumire);
    }
}
