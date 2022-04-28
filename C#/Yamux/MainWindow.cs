using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Yamux
{
    class MainWindow : Window
    {
        [UI] private Label _label_output = null;
        [UI] private Button _button_click_me = null;
        
        private int _count;
        
        public MainWindow() : this(new Builder("MainWindow.glade"))
        {
        }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;
            _button_click_me.Clicked += Clicked_Change_Text;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void Clicked_Change_Text(object sender, EventArgs a)
        {
            _count++;
            Console.WriteLine("Click!");
            _label_output.Text = "You click " + _count;
        }
    }
}