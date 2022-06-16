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
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=1lD--PBhOuFf1-MbhNu0dC6l6C9U-t4Xe"), //about_icon.svg
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=1-afcpIaXaBc7kEesJ4HHXiAjpVvEwJ_I"), //icon.svg
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=1AznTfYXfcSeXHkYYXP4ObCd3QHc7gm6F"), //icons8_rock_music_100_negate.png
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=1RD8GVXEHgLIsrjJjadozfrjv814vJoeI"), //icons8-download.png
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=19ThmVFjOfPuTRtUx63BBNcMlIVmDhKsY"), //icons8-next.png
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=1xzpRFD1Pm7q3LY8i_lOHliOGBtVQBaM4"), //icons8-pause.png
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=14-dOuiZzJSlWxG_ueFcKfAi8g4ghJ3ey"), //icons8-play.png
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=1IoXzhgrOIyimcGJxzZYe6Nq6W2e3HgvL"), //icons8-previous.png
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=1w5jcd4h8lL8lWsxHdRkxMafDdYOqMmY2"), //icons8-settings-20.png
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=1ZQRsi_Il5cyM1P1lSZbyA1egwXCw4MKx"), //icons8-stop.png
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=1cZccppheNu60uNeLfNatIsh-AtbA01AJ"), //yandex_en_icon-icons.com_61632(1).png
                    new Uri("https://drive.google.com/uc?export=download&confirm=no_antivirus&id=1QGFY6HVE1nz8l2hqfoHOCQOetAJghn1H"), //icons8_rock_music_100_negate50x50.png
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