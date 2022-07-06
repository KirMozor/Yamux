using System;
using Cairo;
using Application = Gtk.Application;
using Process = System.Diagnostics.Process;

namespace Yamux
{
    class Program
    {
        public static event EventHandler Closed;
        public static void Main(string[] args)
        { 
            Application.Init();

            Application app = new Application(IntPtr.Zero);
            app.Register(GLib.Cancellable.Current);
            YamuxWindow win = new YamuxWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();
        }
    }
}
