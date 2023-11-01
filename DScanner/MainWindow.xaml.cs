using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DScannerLibrary.BusinessLogic;
using DScanner.Models;
using MapsterMapper;

namespace DScanner;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IMapper _mapper;
    private readonly InventoryMovementsLogic _inventoryMovementsLogic;

    private List<InventoryExitDisplayModel> InventoryExitsForDisplay { get; set; } = new();

    public MainWindow(InventoryMovementsLogic inventoryMovementsLogic)
    {
        InitializeComponent();
        _mapper = new Mapper(MapperConfig.GetAdapterConfig());
        _inventoryMovementsLogic = inventoryMovementsLogic;
        ExitsDatePicker.SelectedDate = DateTime.Now;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        LoadInventoryExitsFromDatabase();
    }

    private void LoadInventoryExitsFromDatabase()
    {
        try
        {
            InventoryExitsForDisplay = _mapper.Map<List<InventoryExitDisplayModel>>(
                    _inventoryMovementsLogic.GetInventoryExitsByDate(ExitsDatePicker.SelectedDate));

            InventoryExitsDataGrid.ItemsSource = InventoryExitsForDisplay;

            if (InventoryExitsForDisplay.Any())
            {
                InventoryExitsDataGrid.SelectedItem = InventoryExitsForDisplay[^1];
                InventoryExitsDataGrid.ScrollIntoView(InventoryExitsDataGrid.SelectedItem);
            }

            CalculateInventoryTotals(InventoryExitsForDisplay);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            return;
        }
    }

    private void ExitsDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadInventoryExitsFromDatabase();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadInventoryExitsFromDatabase();
    }

    private void CalculateInventoryTotals(List<InventoryExitDisplayModel> inventoryExits)
    {
        var totalsPerInventory = inventoryExits
            .GroupBy(x => x.Gestiune)
            .Select(x => new { Gestiune = x.Key, Total = x.Sum(article => article.Total) });

        InventoryTotalsDataGrid.ItemsSource = totalsPerInventory;
    }
    private void InventoryExitsDataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var selectedArticles = InventoryExitsDataGrid.SelectedItems.Cast<InventoryExitDisplayModel>();
        SelectedArticlesPriceTextbox.Text = selectedArticles?.Sum(x => x.Total).ToString();
    }

    public string Barcode { get; set; } = string.Empty;
    private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if ((int)e.Key >= 34 && ((int)e.Key) <= 43) e.Handled = true;
            Barcode += e.Key;

            if (e.Key == (Key)6)
            {
                Barcode = Barcode.Replace("D", "").Replace("Return", "");
                await _inventoryMovementsLogic.GenerateInventoryExits(Barcode, 1);
                Barcode = string.Empty;
                LoadInventoryExitsFromDatabase();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Atentie", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
    }

    private async void ProcessInventoryExitButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var barcode = BarcodeTextbox.Text;
            if (string.IsNullOrWhiteSpace(barcode) || Decimal.TryParse(barcode, out decimal convertedBarcode))
                throw new Exception("Codul de bare nu e completat cum trebuie sau nu e valid");

            var quantity = QuantityTextBox.Text;
            if (string.IsNullOrWhiteSpace(quantity) || Decimal.TryParse(quantity, out decimal decimalQuantity))
                throw new Exception("Cantitatea nu e completata cum trebuie sau nu e un numar");

            await _inventoryMovementsLogic.GenerateInventoryExits(barcode, decimalQuantity);
            LoadInventoryExitsFromDatabase();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Atentie", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
    }
}
