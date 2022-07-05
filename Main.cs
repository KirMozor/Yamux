using System;
using System.Collections.Generic;
using System.IO;
using YandexMusicApi;
using Application = Gtk.Application;

namespace Yamux
{
    class Program
    {
        public static void Main(string[] args)
        {
            Application.Init();

            Application app = new Application("com.github.KirMozor.Yamux", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            YamuxWindow win = new YamuxWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();
        }
    }
}
