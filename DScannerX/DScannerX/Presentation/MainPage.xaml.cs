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
	    var date = new DateTime(2024, 5, 30);

        var data = new List<InventoryExitModel>(_productsCache.GetRecords(DbfFilePath.Text, date));
	    ListX.ItemsSource = data.Select(x => $"{x.id_iesire}-{x.cod}-{x.denumire}\n{x.um} {x.pret_unitar} {x.valoare} {x.cantitate} {x.cont} {x.total} {x.adaos} {x.text_supl}");
    }
}
