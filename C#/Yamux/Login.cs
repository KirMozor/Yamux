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
        public static string LoginYamux(string loginText, string passwordText)
        {
            var request = WebRequest.Create("https://oauth.yandex.com/token");
            request.ContentType = "application/json";
            request.Headers["User-Agent"] = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";

            request.Method = "POST";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write("grant_type=password&client_id=23cabbbdc6cd418abb4b39c32c41195d&client_secret=53bc75238f0c4d08a118e51fe9203300&username={0}&password={1}", loginText, passwordText);
            }


            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    var json = reader.ReadToEnd();
                    return json;
                }   
            } catch(WebException e)
            {
                string text = e.Message;
                return text;
            }
        }
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