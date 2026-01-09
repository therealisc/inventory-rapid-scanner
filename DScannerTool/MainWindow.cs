using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using DScannerLibrary.Services;
using DScannerLibrary.BusinessLogic;
using DScannerLibrary.DataAccess;
using DScannerLibrary.Models;

namespace DScannerTool
{
    class MainWindow : Window
    {
        [UI] private Label _label1 = null;
        [UI] private Button _button1 = null;
        [UI] private ListBox _list1 = null;
        //[UI] private Notebook _list1 = null;

        private int _counter;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

	    DisplayData();

            DeleteEvent += Window_DeleteEvent;
            _button1.Clicked += Button1_Clicked;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private async void Button1_Clicked(object sender, EventArgs a)
        {
    	    var emailService = new EmailService();
	    await emailService.SendMailAsync("ruporfcjpzndjzhk");

            _counter++;
            _label1.Text = "Mail sent " + _counter + " time(s).";
        }

	private void DisplayData()
	{
	   var dataAccess = new SqliteDataAccess();
           var inventoryMovementsLogic = new InventoryMovementsLogic(dataAccess, null, null);

           inventoryMovementsLogic.AddTemporaryDb();

	   var sql = $@"
		   SELECT * FROM intr_det";

	   var entries = dataAccess.ReadData<OperationalInventoryModel>(sql);

	   foreach (OperationalInventoryModel entry in entries)
	   {
	      _list1.Add(new Label($"{entry.cod} {entry.gestiune} {entry.cantitate}"));
	   }

	   //_grid1.Attach(new Label("Number"), 0, 0, 1, 1);
	   //_grid1.Attach(new Label("Product"), 1, 0, 1, 1);
	   //ShowAll();
	    
	   ShowAll();
	}
    }
}
