using System;
using Gdk;
using Gtk;
using Newtonsoft.Json.Linq;
using YandexMusicApi;
using Application = Gtk.Application;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;

namespace Yamux
{
    class LoginWindow : Window
    {
        [UI] private Entry _set_login = null;
        [UI] private Entry _set_password = null;
        [UI] private Button _login_yamux = null;
        [UI] private Label _current_login = null;
        [UI] private Button _reset_password = null;
        private byte countRun;
        public LoginWindow() : this(new Builder("Login.glade"))
        {
        }

        private LoginWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            WindowStateEvent += CheckConfig;
            DeleteEvent += Window_DeleteEvent;
            SetDefaultIconFromFile("Svg/icon.svg");
            
            _set_password.Visibility = false;
            _reset_password.Clicked += ResetPasswordClick;
            _login_yamux.Clicked += LogInButton;
        }
        void CheckConfig(object sender, WindowStateEventArgs a)
        {
            countRun++;
            if (countRun == 1)
            {
                if (Login.CheckAviabilityConfig())
                {
                    Window.Hide();
                    YamuxWindow win = new YamuxWindow();
                    Application.AddWindow(win);
                    win.Show();
                }
            }
        }
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void ResetPasswordClick(object sender, EventArgs e)
        {
            Login.ResetPasswordOpen();
        }
        private void LogInButton(object sender, EventArgs e)
        {
            string loginText = _set_login.Text;
            string passwordText = _set_password.Text;
            bool loginCheck = String.IsNullOrWhiteSpace(loginText);
            bool passwordCheck = String.IsNullOrWhiteSpace(passwordText);

            if (loginCheck == false && passwordCheck == false)
            {
                //JObject token = Token.GetToken(loginText, passwordText);
                try
                {
                    JObject token = Token.GetToken(loginText, passwordText);
                    JToken jTokentoken = token.First.First;
                    Login.WriteToken(jTokentoken.ToString());
                    
                    Window.Hide();
                    YamuxWindow win = new YamuxWindow();
                    Application.AddWindow(win);
                    win.Show();
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Message == "The remote server returned an error: (400) Bad Request.")
                    {
                        _current_login.Text = "400: Данные не верные";
                    }

                    if (ex.Message == "The remote server returned an error: (403) Forbidden.")
                    {
                        _current_login.Text = "403: Yandex включил защиту от брутфорса";
                    }
                }
            }
            else
            {
                _current_login.Text = "Ты ввёл пустую строку. Введи пароль и логин";
            }
        }
    }
}