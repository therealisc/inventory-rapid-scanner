using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using DScannerLibrary.Services;

namespace DScannerTool
{
    class MainWindow : Window
    {
        [UI] private Label _label1 = null;
        [UI] private Button _button1 = null;

        private int _counter;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            _button1.Clicked += Button1_Clicked;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private async void Button1_Clicked(object sender, EventArgs a)
        {
	    //Env.OSV
    	    var emailService = new EmailService();
	    await emailService.SendMailAsync("ruporfcjpzndjzhk");

            _counter++;
            _label1.Text = "Mail sent " + _counter + " time(s).";
        }
    }
}
