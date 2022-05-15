using System;
using Application = Gtk.Application;

namespace Yamux
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            Application app = new Application("org.Yamux.Yamux", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            LoginWindow win = new LoginWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();
        }
    }
}