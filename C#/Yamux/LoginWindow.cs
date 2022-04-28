using System;
using Gtk;
using Newtonsoft.Json;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Application = Gtk.Application;
using UI = Gtk.Builder.ObjectAttribute;

namespace Yamux
{
    class LoginWindow : Window
    {
        private static YandexMusicApi _api;

        [UI] private Entry _set_login = null;
        [UI] private Entry _set_password = null;
        [UI] private Button _login_yamux = null;
        [UI] private Label _current_login = null;
        [UI] private Button _reset_password = null;
        public LoginWindow() : this(new Builder("Login.glade"))
        {
        }

        private LoginWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;
            _reset_password.Clicked += ResetPasswordClick;
            _login_yamux.Clicked += LogInButton;
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
                string checkLogin = Login.LoginYamux(loginText, passwordText);
                Console.WriteLine(checkLogin);
                if (checkLogin != "The remote server returned an error: (400) Bad Request.")
                {
                    dynamic obj = JsonConvert.DeserializeObject(checkLogin);
                    string tokenYandex = (string)obj.access_token;
                    Login.WriteToken(tokenYandex);

                    _current_login.Text = "Добро пожаловать!";
                }
                else
                {
                    _current_login.Text = "Ты ввёл не правильные данные";
                }
            }
            else
            {
                _current_login.Text = "Ты ввёл пустую строку. Введи пароль и логин";
            }
        }
    }
}