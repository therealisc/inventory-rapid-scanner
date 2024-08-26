using System;
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
        QuantityTextBox.Text = 1.ToString();
        LoadInventoryExitsFromDatabase();
    }

    private void LoadInventoryExitsFromDatabase()
    {
        try
        {
            InventoryExitsForDisplay = _mapper.Map<List<InventoryExitDisplayModel>>(
                    _inventoryMovementsLogic.GetInventoryExitsByDate("C:\\SAGA C.3.0\\0003", ExitsDatePicker.SelectedDate));

	    InventoryExitsForDisplay.ForEach(x => x.Nr = IndexOf(x) + 1);

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
            .Select(x => new { Gestiune = x.Key, Total = x.Sum(article => Convert.ToDecimal(article.Total)), Adaos = x.Sum(article => article.Adaos) });

        InventoryTotalsDataGrid.ItemsSource = totalsPerInventory;
    }
    private void InventoryExitsDataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var selectedArticles = InventoryExitsDataGrid.SelectedItems.Cast<InventoryExitDisplayModel>();
        SelectedArticlesPriceTextbox.Text = selectedArticles?.Sum(x => Convert.ToDecimal(x.Total)).ToString();
    }

    public string Barcode { get; set; } = string.Empty;
    private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (BarcodeTextbox.IsFocused || QuantityTextBox.IsFocused)
        {
            return;
        }

        try
        {
            if ((int)e.Key >= 34 && ((int)e.Key) <= 43) e.Handled = true;
            Barcode += e.Key;


            if (e.Key == (Key)6)
            {
                Barcode = Barcode.Replace("D", "");

                if (Barcode.Contains("Return"))
                {
                    Barcode = Barcode.Replace("Return", "");
                    BarcodeTextbox.Text = Barcode;

                    var quantity = QuantityTextBox.Text;
                    if (string.IsNullOrWhiteSpace(quantity) || decimal.TryParse(quantity, out decimal decimalQuantity) == false)
                        throw new Exception("Cantitatea nu e completata cum trebuie sau nu e un numar");

                    await _inventoryMovementsLogic.GenerateInventoryExits(Barcode, decimalQuantity);
                    Barcode = string.Empty;
                    LoadInventoryExitsFromDatabase();
                    QuantityTextBox.Text = 1.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Barcode = string.Empty;
            MessageBox.Show(ex.Message, "Atentie", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
    }

    private async void ProcessInventoryExitButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var barcode = BarcodeTextbox.Text;
            if (string.IsNullOrWhiteSpace(barcode) || decimal.TryParse(barcode, out decimal convertedBarcode) == false)
                throw new Exception("Codul de bare nu e completat cum trebuie sau nu e valid");

            var quantity = QuantityTextBox.Text;
            if (string.IsNullOrWhiteSpace(quantity) || decimal.TryParse(quantity, out decimal decimalQuantity) == false)
                throw new Exception("Cantitatea nu e completata cum trebuie sau nu e un numar");

            await _inventoryMovementsLogic.GenerateInventoryExits(barcode, decimalQuantity);
            LoadInventoryExitsFromDatabase();
            QuantityTextBox.Text = 1.ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Atentie", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
    }

    private void DecreaseQuantityButton_Click(object sender, RoutedEventArgs e)
    {
        var quantity = QuantityTextBox.Text;
        if (decimal.TryParse(quantity, out decimal decimalQuantity))
        {
            if (decimalQuantity == 1)
            {
                return;
            }

            decimalQuantity -= 1;

            QuantityTextBox.Text = decimalQuantity.ToString();
        }
    }

    private void IncreaseQuantityButton_Click(object sender, RoutedEventArgs e)
    {
        var quantity = QuantityTextBox.Text;
        if (decimal.TryParse(quantity, out decimal decimalQuantity))
        {
            decimalQuantity += 1;
            QuantityTextBox.Text = decimalQuantity.ToString();
        }
    }
}
