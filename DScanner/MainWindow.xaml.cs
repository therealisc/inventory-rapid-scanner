using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DScannerLibrary.BusinessLogic;
using DScanner.Models;

namespace DScanner;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly InventoryMovementsLogic _inventoryMovementsLogic;

    private List<InventoryExitDisplayModel> _inventoryExits;

    public MainWindow()
    {
        InitializeComponent();
        _inventoryMovementsLogic = new InventoryMovementsLogic();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ExitsDatePicker.SelectedDate = DateTime.Now;
        LoadInventoryExitsFromDatabase();
    }

    private void LoadInventoryExitsFromDatabase()
    {
        try
        {
            InventoryExitsDataGrid.ItemsSource = _inventoryMovementsLogic.GetInventoryExitsByDate(ExitsDatePicker.SelectedDate);
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
}
