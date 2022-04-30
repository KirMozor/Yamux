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

            var app = new Application("org.Yamux.Yamux", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new LoginWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();
        }
    }
}