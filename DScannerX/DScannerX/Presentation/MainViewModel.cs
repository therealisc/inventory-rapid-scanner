using System.Collections.ObjectModel;

namespace DScannerX.Presentation;

public partial class MainViewModel : ObservableObject
{
    private INavigator _navigator;
    private ProductsCache _productsCache;


    [ObservableProperty]
    private ObservableCollection<string> data;

    [ObservableProperty]
    private string? name;

    public MainViewModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator,
        ProductsCache productsCache
        )
    {
        _navigator = navigator;
        _productsCache = productsCache;

        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
        GoToSecond = new AsyncRelayCommand(GoToSecondView);
        LoadDbf = new AsyncRelayCommand(LoadDbfRecords);

        
    }
    public string? Title { get; }

    public ICommand GoToSecond { get; }
    public ICommand LoadDbf { get; }

    private async Task GoToSecondView()
    {
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name!));
    }

    private async Task LoadDbfRecords()
    {
        //data = new ObservableCollection<string>(await _productsCache.GetRecords());

        var tj = data.Count == 0;
    }
}
