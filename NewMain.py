from PyQt5.QtWidgets import QApplication, QMainWindow
from PyQt5 import QtCore, QtWidgets, uic
from PyQt5.QtGui import QIcon, QPixmap, QFont, QImage, QCursor
from PyQt5.QtCore import QObject
from yandex_music import Best, Client, Search
import yandex_music
import webbrowser
import threading
import requests
import toml
import sys

try:
    config = toml.load("config.toml")
except:
    with open("config.toml", "w") as file:
        file.write('''token_yandex = ""
block = 10''')

class MainWindow(QtWidgets.QMainWindow, QObject):
    def __init__(self):
        # Проверяем на рабочий токен и то что интернет работает
        super(MainWindow, self).__init__()
        uic.loadUi('Ui/Landing.ui', self)  # Это нужно для инициализации нашего дизайна
        self.hide_or_show_menu = True
        self.widget_4.hide()
        self.press_button_pause = lambda: self.media_player.pause()

        self.hide_bar_menu_button.clicked.connect(self.hide_menu_bar_or_show)
        self.hide_bar_menu_button_2.clicked.connect(self.hide_menu_bar_or_show)

    def hide_menu_bar_or_show(self):
        if self.hide_or_show_menu:
            self.widget_4.show()
            self.widget_3.hide()
            self.hide_or_show_menu = False
        else:
            self.widget_4.hide()
            self.widget_3.show()
            self.hide_or_show_menu = True

class Check(QtWidgets.QMainWindow, QObject):
    def __init__(self):
        # Проверяем на рабочий токен и то что интернет работает
        super(Check, self).__init__()
        uic.loadUi('Ui/Authorize.ui', self)  # Это нужно для инициализации нашего дизайна
        config = toml.load("config.toml")

        try:
            if not config.get('token_yandex'):
                self.show()
                self.reg_button.clicked.connect(self.reg_button_click)
                self.password_recovery.clicked.connect(self.password_recovery_click)
                self.msg_btn = lambda i: i.text()
            else:
                client = Client(config.get('token_yandex')).init()
                mainWindow = MainWindow()
                mainWindow.show()

        except yandex_music.exceptions.NetworkError:
            self.error_standart("Проблемы с интернетом",
                "Yandex Music Api не видит вашего интернета, проверь, всё ли с ним в порядке",
                exit_or_no=True)
            self.show()
            self.msg_btn = lambda i: i.text()
            self.reg_button.clicked.connect(self.reg_button_click)

        except yandex_music.exceptions.UnauthorizedError:
            self.show()
            self.error_standart("Неправильный токен", "YandexMusic говорит у вас неправильный токен, перезарегистрируйтесь", exit_or_no=False)
            self.msg_btn = lambda i: i.text()
            self.reg_button.clicked.connect(self.reg_button_click)
        except UnicodeEncodeError:
            self.show()
            self.error_standart("Токен из непонятных символов",
                              "YandexMusic говорит что у вас токен из непонятных символов, перезарегистрируйтесь", exit_or_no=False)
            self.msg_btn = lambda i: i.text()
            self.reg_button.clicked.connect(self.reg_button_click)
        except NameError:
            self.show()
            self.error_standart("Что-то не то с конфигом",
                "У вас что-то не то было с конфигом, перезарегистрируйтесь", exit_or_no=False)
            self.msg_btn = lambda i: i.text()
            self.reg_button.clicked.connect(self.reg_button_click)

    def error_standart(self, message_log, description, windows_title="Ошибка", exit_or_no=True, not_button_cancel=False):
        error = QMessageBox()
        error.setWindowTitle(windows_title)
        error.setIcon(QMessageBox.Warning)
        error.setText(description)
        error.setIcon(QMessageBox.Warning)
        if not not_button_cancel:
            error.setStandardButtons(QMessageBox.Ok | QMessageBox.Cancel)
            error.buttonClicked.connect(self.msg_btn)
            retval = error.exec_()
            if exit_or_no:
                sys.exit()
        else:
            error.setStandardButtons(QMessageBox.Ok)
            if exit_or_no:
                sys.exit()

    def error_config(self, message_log, description, windows_title="Ошибка", exit_or_no=True):
        text, ok = QInputDialog.getText(self, windows_title, description)
        if ok:
            with open("config.toml", "w") as file:
                file.write(f'''token_yandex = "{text}"
block = 10''')
        else:
            if exit_or_no:
                sys.exit()
    def password_recovery_click(self):
        webbrowser.open("https://passport.yandex.kz/auth/restore/login?mode=add-user", new=2)

    def reg_button_click(self):
        login = self.set_login.text()
        password = self.set_password.text()

        if len(login.split()) != 0 and len(password.split()) != 0:
            link_post = "https://oauth.yandex.com/token"
            user_agent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36"
            header = {
                "user-agent": user_agent
            }

            try:
                request_post = f"grant_type=password&client_id=23cabbbdc6cd418abb4b39c32c41195d&client_secret=53bc75238f0c4d08a118e51fe9203300&username={login}&password={password}"
                request_auth = requests.post(link_post, data=request_post, headers=header)

                if request_auth.status_code == 400:
                    self.error_standart("Неправильные данные", "Yandex сказал мне что вы ввели неправильные данные, введите правильные", exit_or_no=False)
                if request_auth.status_code == 200:
                    json_data = request_auth.json()
                    text = json_data.get('access_token')
                    with open("config.toml", "w") as file:
                        file.write(f'''token_yandex = "{text}"
block = 10''')
                    self.hide()
                    mainWindow = MainWindow()
                    mainWindow.show()
            except requests.exceptions.ConnectionError:
                self.error_standart("Проблемы с интернетом", "Проверьте интернет подключенние", exit_or_no=False)
        else:
            self.error_standart("Ввели пустую строку", "Вы ввели пустую строку :)", exit_or_no=False)

def main():
    app = QtWidgets.QApplication(sys.argv)  # Новый экземпляр QApplication
    check = Check()  # Создаём объект класса MainWindow
    sys.exit(app.exec_())  # и запускаем приложение
if __name__ == '__main__': # Если мы запускаем файл напрямую, а не импортируем
    print("Запуск Yamux")
    main()  # то запускаем функцию main()
