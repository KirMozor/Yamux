using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Tomlyn;

namespace Yamux
{
    public class Login
    {
        public static void WriteToken(string text)
        {
            text = "yandex_token = '" + text + "'";
            using (FileStream fstream = new FileStream("config.toml", FileMode.OpenOrCreate))
            {
                byte[] buffer = Encoding.Default.GetBytes(text);
                fstream.Write(buffer, 0, buffer.Length);
            }
        }
        public static void ResetPasswordOpen()
        {
            string url = "https://passport.yandex.kz/auth/restore/login?mode=add-user";
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                } else
                {
                    throw;  
                }
            }
        }
        
        public static bool CheckAviabilityConfig()
        {
            bool check = File.Exists("config.toml");
            if (check)
            {
                try
                {
                    using (FileStream fstream = File.OpenRead("config.toml"))
                    {
                        byte[] buffer = new byte[fstream.Length];
                        fstream.Read(buffer, 0, buffer.Length);
                        string textFromFile = Encoding.Default.GetString(buffer);
                    
                        var model = Toml.ToModel(textFromFile);
                        var tokenYandex = (string)model["yandex_token"]!;
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            } else
            {
                return false;
            } 
        }
    }
}