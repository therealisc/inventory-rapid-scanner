using System;
using System.Collections.Generic;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using DScannerLibrary.BusinessLogic;
using DScannerLibrary.DataAccess;
using DScannerLibrary.Models;
using DScannerTool.Models;
using MapsterMapper;

namespace DScannerTool
{
    class MainWindow : Window
    {
        [UI] private Entry _dateEntry = null!;
        [UI] private Button _refreshButton = null!;
        [UI] private TreeView _exitsTreeView = null!;
        [UI] private TreeView _totalsTreeView = null!;
        [UI] private Entry _quantityEntry = null!;
        [UI] private Entry _barcodeEntry = null!;
        [UI] private Button _decreaseQuantityButton = null!;
        [UI] private Button _increaseQuantityButton = null!;
        [UI] private Button _processExitButton = null!;
        [UI] private Entry _selectedTotalEntry = null!;
        [UI] private Label _statusLabel = null!;
        [UI] private TreeSelection _exitsSelection = null!;

        private readonly IMapper _mapper;
        private readonly InventoryMovementsLogic _inventoryMovementsLogic;
        private List<InventoryExitDisplayModel> _inventoryExits = new();

        // Data stores
        private ListStore _exitsStore = null!;
        private ListStore _totalsStore = null!;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            _mapper = new Mapper(MapperConfig.GetAdapterConfig());
            var dataAccess = new SqliteDataAccess();
            var articleSearchLogic = new ArticleSearchLogic(dataAccess);
            var exitDocumentCheck = new ExitDocumentCheck(dataAccess);
            _inventoryMovementsLogic = new InventoryMovementsLogic(dataAccess, articleSearchLogic, exitDocumentCheck);

            SetupTreeView();
            SetupEventHandlers();

            // Set today's date
            _dateEntry.Text = DateTime.Now.ToString("yyyy-MM-dd");

            DeleteEvent += Window_DeleteEvent;
            KeyPressEvent += Window_KeyPressEvent;

            // Load initial data
            LoadInventoryExits();
        }

        private void SetupTreeView()
        {
            // Setup exits TreeView
            _exitsStore = new ListStore(typeof(int), typeof(string), typeof(string), typeof(int),
                                        typeof(decimal), typeof(decimal), typeof(decimal), typeof(string));
            _exitsTreeView.Model = _exitsStore;

            AddColumn(_exitsTreeView, "Nr", 0);
            AddColumn(_exitsTreeView, "Gestiune", 1);
            AddColumn(_exitsTreeView, "Denumire", 2);
            AddColumn(_exitsTreeView, "Cod", 3);
            AddColumn(_exitsTreeView, "Cantitate", 4);
            AddColumn(_exitsTreeView, "Pret", 5);
            AddColumn(_exitsTreeView, "Total", 6);

            // Setup totals TreeView
            _totalsStore = new ListStore(typeof(string), typeof(decimal), typeof(decimal));
            _totalsTreeView.Model = _totalsStore;

            AddColumn(_totalsTreeView, "Gestiune", 0);
            AddColumn(_totalsTreeView, "Total", 1);
            AddColumn(_totalsTreeView, "Adaos", 2);
        }

        private void AddColumn(TreeView treeView, string title, int column)
        {
            var renderer = new CellRendererText();
            var columnWidget = new TreeViewColumn(title, renderer, "text", column);
            columnWidget.Resizable = true;
            columnWidget.Reorderable = true;
            treeView.AppendColumn(columnWidget);
        }

        private void SetupEventHandlers()
        {
            _refreshButton.Clicked += (s, e) => LoadInventoryExits();
            _decreaseQuantityButton.Clicked += DecreaseQuantityButton_Click;
            _increaseQuantityButton.Clicked += IncreaseQuantityButton_Click;
            _processExitButton.Clicked += ProcessExitButton_Click;
            _exitsSelection.Changed += ExitsSelection_Changed;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void LoadInventoryExits()
        {
            try
            {
                DateTime? selectedDate = null;
                if (DateTime.TryParse(_dateEntry.Text, out DateTime parsedDate))
                {
                    selectedDate = parsedDate;
                }

                var exits = _inventoryMovementsLogic.GetInventoryExitsByDate(selectedDate);
                _inventoryExits = _mapper.Map<List<InventoryExitDisplayModel>>(exits);

                // Update Nr column
                for (int i = 0; i < _inventoryExits.Count; i++)
                {
                    _inventoryExits[i].Nr = i + 1;
                }

                RefreshExitsStore();
                CalculateInventoryTotals();

                _statusLabel.Text = $"Incărcat {_inventoryExits.Count} înregistrări";
            }
            catch (Exception ex)
            {
                ShowMessage($"Eroare: {ex.Message}", MessageType.Error);
            }
        }

        private void RefreshExitsStore()
        {
            _exitsStore.Clear();
            foreach (var exit in _inventoryExits)
            {
                _exitsStore.AppendValues(
                    exit.Nr,
                    exit.Gestiune,
                    exit.Denumire,
                    exit.CodProdus,
                    exit.Cantitate,
                    exit.PretUnitar,
                    exit.Total,
                    exit.TextSuplimentar
                );
            }

            // Scroll to last item if there are any
            if (_inventoryExits.Count > 0)
            {
                var path = new TreePath(new[] { _inventoryExits.Count - 1 });
                _exitsTreeView.ScrollToCell(path, null, false, 0, 0);
                _exitsSelection.SelectPath(path);
            }
        }

        private void CalculateInventoryTotals()
        {
            _totalsStore.Clear();

            var totalsPerInventory = new Dictionary<string, (decimal Total, decimal Adaos)>();

            foreach (var exit in _inventoryExits)
            {
                var key = exit.Gestiune;
                if (totalsPerInventory.ContainsKey(key))
                {
                    var existing = totalsPerInventory[key];
                    totalsPerInventory[key] = (existing.Total + exit.Total, existing.Adaos + exit.Adaos);
                }
                else
                {
                    totalsPerInventory[key] = (exit.Total, exit.Adaos);
                }
            }

            foreach (var kvp in totalsPerInventory)
            {
                _totalsStore.AppendValues(kvp.Key, kvp.Value.Total, kvp.Value.Adaos);
            }
        }

        private void ExitsSelection_Changed(object? sender, EventArgs e)
        {
            decimal total = 0;
            TreeIter iter;

            // Get all selected rows
            var paths = _exitsSelection.GetSelectedRows();
            if (paths != null)
            {
                foreach (var path in paths)
                {
                    if (_exitsStore.GetIter(out iter, path))
                    {
                        var totalValue = (decimal)_exitsStore.GetValue(iter, 6);
                        total += totalValue;
                    }
                }
            }

            _selectedTotalEntry.Text = total.ToString("F2");
        }

        private void DecreaseQuantityButton_Click(object? sender, EventArgs e)
        {
            if (decimal.TryParse(_quantityEntry.Text, out decimal quantity))
            {
                if (quantity > 1)
                {
                    quantity -= 1;
                    _quantityEntry.Text = quantity.ToString();
                }
            }
        }

        private void IncreaseQuantityButton_Click(object? sender, EventArgs e)
        {
            if (decimal.TryParse(_quantityEntry.Text, out decimal quantity))
            {
                quantity += 1;
                _quantityEntry.Text = quantity.ToString();
            }
        }

        private async void ProcessExitButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var barcode = _barcodeEntry.Text.Trim();
                if (string.IsNullOrWhiteSpace(barcode))
                {
                    ShowMessage("Codul de bare nu este completat", MessageType.Warning);
                    return;
                }

                if (!decimal.TryParse(_quantityEntry.Text, out decimal quantity) || quantity <= 0)
                {
                    ShowMessage("Cantitatea nu este valida", MessageType.Warning);
                    return;
                }

                await _inventoryMovementsLogic.GenerateInventoryExits(barcode, quantity);

                _barcodeEntry.Text = "";
                _quantityEntry.Text = "1";
                LoadInventoryExits();

                _statusLabel.Text = "Iesire procesata cu succes";
            }
            catch (Exception ex)
            {
                ShowMessage($"Eroare: {ex.Message}", MessageType.Error);
            }
        }

        // Keyboard handling for barcode scanner
        private string _scannedBarcode = string.Empty;
        private DateTime _lastKeyTime = DateTime.MinValue;

        private void Window_KeyPressEvent(object sender, KeyPressEventArgs args)
        {
            // Don't intercept if focus is on text entries
            if (_barcodeEntry.HasFocus || _quantityEntry.HasFocus || _dateEntry.HasFocus)
            {
                _scannedBarcode = string.Empty;
                return;
            }

            // Reset barcode if too much time between keys (not from scanner)
            if ((DateTime.Now - _lastKeyTime).TotalMilliseconds > 100)
            {
                _scannedBarcode = string.Empty;
            }
            _lastKeyTime = DateTime.Now;

            var keyChar = GetKeyChar(args.Event);
            if (keyChar != '\0')
            {
                _scannedBarcode += keyChar;
            }

            // Check for Enter key (barcode scanner sends Enter at end)
            if (args.Event.Key == Gdk.Key.Return || args.Event.Key == Gdk.Key.KP_Enter)
            {
                if (_scannedBarcode.Length > 3)
                {
                    ProcessScannedBarcode(_scannedBarcode);
                    _scannedBarcode = string.Empty;
                }
            }
        }

        private char GetKeyChar(Gdk.EventKey keyEvent)
        {
            // Get the key value for digit keys
            uint keyval = keyEvent.KeyValue;

            // Digits 0-9 have keyvals 48-57
            if (keyval >= 48 && keyval <= 57)
                return (char)keyval;

            // Keypad digits
            if (keyval >= 65456 && keyval <= 65465)
                return (char)('0' + (int)(keyval - 65456));

            return '\0';
        }

        private async void ProcessScannedBarcode(string barcode)
        {
            try
            {
                if (!decimal.TryParse(_quantityEntry.Text, out decimal quantity) || quantity <= 0)
                {
                    quantity = 1;
                }

                await _inventoryMovementsLogic.GenerateInventoryExits(barcode, quantity);

                _quantityEntry.Text = "1";
                LoadInventoryExits();

                _statusLabel.Text = $"Cod bare procesat: {barcode}";
            }
            catch (Exception ex)
            {
                ShowMessage($"Eroare la procesare: {ex.Message}", MessageType.Error);
            }
        }

        private void ShowMessage(string message, MessageType type)
        {
            var buttons = type == MessageType.Error || type == MessageType.Warning
                ? ButtonsType.Ok
                : ButtonsType.None;

            using var dialog = new MessageDialog(
                this,
                DialogFlags.Modal,
                type,
                buttons,
                message
            );

            dialog.Title = type == MessageType.Error ? "Eroare" : "Atentie";
            dialog.Run();
            dialog.Destroy();
        }
    }
}