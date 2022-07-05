using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System;
using Newtonsoft.Json.Linq;
using YandexMusicApi;
using Tomlyn;

namespace Yamux
{
    public class Login
    {
        public static void WriteToken(string text)
        {
            text = "yandex_token = '" + text + "'";
            using (FileStream fstream = new FileStream(System.IO.Path.GetFullPath("config.toml"), FileMode.OpenOrCreate))
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
            bool check = File.Exists(Path.GetFullPath("config.toml"));
            if (check)
            {
                try
                {
                    using (FileStream fstream = File.OpenRead(System.IO.Path.GetFullPath("config.toml")))
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
        public static string LogInButton(string loginText, string passwordText)
        {
            bool loginCheck = String.IsNullOrWhiteSpace(loginText);
            bool passwordCheck = String.IsNullOrWhiteSpace(passwordText);

            if (loginCheck == false && passwordCheck == false)
            {
                try
                {
                    JObject token = Token.GetToken(loginText, passwordText);
                    JToken jTokentoken = token.First.First;
                    WriteToken(jTokentoken.ToString());
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Message == "The remote server returned an error: (400) Bad Request.")
                    {
                        return "400: Данные не верные";
                    }
                    if (ex.Message == "The remote server returned an error: (403) Forbidden.")
                    {
                        return "403: Yandex включил защиту от брутфорса";
                    }
                }
            }
            else
            {
                return "Вы ввели пустую строку. Введите пароль и логин";
            }

            return "ok";
        }
    }
}