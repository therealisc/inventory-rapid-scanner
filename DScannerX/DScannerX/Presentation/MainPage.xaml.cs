namespace DScannerX.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        var _productsCache = new ProductsCache();
        ListX.ItemsSource = new List<string>(_productsCache.GetRecords());;
    }
}
