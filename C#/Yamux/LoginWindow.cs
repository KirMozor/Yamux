using System;
using Gtk;
using Newtonsoft.Json;
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
                    var win = new YamuxWindow();
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
                string checkLogin = Login.LoginYamux(loginText, passwordText);
                if (checkLogin != "The remote server returned an error: (400) Bad Request.")
                {
                    dynamic obj = JsonConvert.DeserializeObject(checkLogin);
                    string tokenYandex = (string)obj.access_token;
                    Login.WriteToken(tokenYandex);

                    _current_login.Text = "Добро пожаловать!";
                    Window.Hide();
                    var win = new YamuxWindow();
                    Application.AddWindow(win);
                    win.Show();
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