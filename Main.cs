using System;
using System.Collections.Generic;
using System.IO;
using Application = Gtk.Application;

namespace Yamux
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (!Directory.Exists("Svg"))
            {
                Directory.CreateDirectory("Svg");
                List<Uri> IconList = new List<Uri>()
                {
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/about_icon.svg"), //about_icon.svg
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/icon.svg"), //icon.svg
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/icons8_rock_music_100_negate.png"), //icons8_rock_music_100_negate.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/icons8-download.png"), //icons8-download.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/icons8-next.png"), //icons8-next.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/icons8-pause.png"), //icons8-pause.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/icons8-play.png"), //icons8-play.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/icons8-previous.png"), //icons8-previous.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/icons8-settings-20.png"), //icons8-settings-20.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/icons8-stop.png"), //icons8-stop.png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/yandex_en_icon-icons.com_61632(1).png"), //yandex_en_icon-icons.com_61632(1).png
                    new Uri("https://raw.githubusercontent.com/KirMozor/Yamux/main/Svg/icons8_rock_music_100_negate50x50.png"), //icons8_rock_music_100_negate50x50.png
                };
                Console.WriteLine("Icons are downloading\nPlease wait");
                Player.DownloadWithSync(IconList[0], "Svg/about_icon.svg");
                Player.DownloadWithSync(IconList[1], "Svg/icon.svg");
                Player.DownloadWithSync(IconList[2], "Svg/icons8_rock_music_100_negate.png");
                Player.DownloadWithSync(IconList[3], "Svg/icons8-download.png");
                Player.DownloadWithSync(IconList[4], "Svg/icons8-next.png");
                Player.DownloadWithSync(IconList[5], "Svg/icons8-pause.png");
                Player.DownloadWithSync(IconList[6], "Svg/icons8-play.png");
                Player.DownloadWithSync(IconList[7], "Svg/icons8-previous.png");
                Player.DownloadWithSync(IconList[8], "Svg/icons8-settings-20.png");
                Player.DownloadWithSync(IconList[9], "Svg/icons8-stop.png");
                Player.DownloadWithSync(IconList[10], "Svg/yandex_en_icon-icons.com_61632(1).png");
                Player.DownloadWithSync(IconList[11], "Svg/icons8_rock_music_100_negate50x50.png");
                Console.WriteLine("Downloaded!");
            }
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