using System;
using Gdk;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;

namespace Yamux
{
    class YamuxWindow : Window
    {
        [UI] private Box BoxResult0 = null;
        [UI] private Box BoxResult1 = null;
        [UI] private Box BoxResult2 = null;
        [UI] private Box BoxResult3 = null;
        [UI] private Box BoxResult4 = null;
        [UI] private Box BoxResult5 = null;
        [UI] private Entry SearchEntry = null;
        public YamuxWindow() : this(new Builder("Yamux.glade"))
        {
        }

        private YamuxWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;
            KeyPressEvent += EventKeyPress;
        }

        private void EventKeyPress(object sender, KeyPressEventArgs a)
        {
            Console.WriteLine(a.Event);
            Console.WriteLine(a.Args);
        }
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}