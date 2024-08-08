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
        ListX.ItemsSource = new List<string>(_productsCache.GetRecords(DbfFilePath.Text));
    }
}
