using System;
using System.Threading;
using Application = Gtk.Application;

namespace Yamux
{
    class Program
    {
        public static void Main(string[] args)
        {
            Application.Init();

            Application app = new Application("org.KirMozor.Yamux", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            LoginWindow win = new LoginWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();
        }
    }
}