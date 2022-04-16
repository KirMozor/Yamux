# -*- coding: utf-8 -*-
from PyQt6.QtWidgets import QApplication, QMainWindow
from PyQt6 import QtCore, QtWidgets, uic
from PyQt6.QtGui import QIcon, QPixmap, QFont, QImage, QCursor, QMovie
from PyQt6.QtCore import QObject, Qt
from yandex_music import Client

import webbrowser
import requests
import toml
import sys

try:
    config = toml.load("Src/config.toml")
    from Src.LoadSound import GetSearchTrack
except:
    with open("Src/config.toml", "w") as file:
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

        self.main_page_select_button.clicked.connect(self.select_main_page_clicked)
        self.main_page_select_button_2.clicked.connect(self.select_main_page_clicked)
        self.radio_page_select_button.clicked.connect(self.select_radio_clicked)
        self.radio_page_select_button_2.clicked.connect(self.select_radio_clicked)
        self.podcasts_page_select_button.clicked.connect(self.select_podcasts_clicked)
        self.podcasts_page_select_button_2.clicked.connect(self.select_podcasts_clicked)
        self.select_playlist_button.clicked.connect(self.select_users_playlist_clicked)
        self.select_playlist_button_2.clicked.connect(self.select_users_playlist_clicked)

    def keyPressEvent(self, e):
        text = self.lineEdit.text()
        if e.key() == 16777220 and len(text.split()) != 0:
            self.get_search = GetSearchTrack(text)
            self.get_search.start()
            self.get_search.signal.connect(self.print_result)

    def print_result(self, res):
        print(res)

    def select_main_page_clicked(self):
        self.description_text.setText("Главная")

    def select_users_playlist_clicked(self):
        self.description_text.setText("Плейлисты")

    def select_podcasts_clicked(self):
        self.description_text.setText("Подкасты")

    def select_radio_clicked(self):
        self.description_text.setText("Радио")

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
        config = toml.load("Src/config.toml")

        try:
            if not config.get('token_yandex'):
                self.show()
                self.reg_button.clicked.connect(self.reg_button_click)
                self.password_recovery.clicked.connect(self.password_recovery_click)
                self.msg_btn = lambda i: i.text()
            else:
                uic.loadUi("Ui/Loading.ui", self)
                self.movie = QMovie("Assets/Gif/icons8-loading-circle.gif")
                self.label.setMovie(self.movie)
                self.movie.start()
                self.show()

                self.my_thread = CheckToken()
                self.my_thread.start()

                self.my_thread.mysignal.connect(self.show_main_window)
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

    def show_main_window(self, s):
        if s == "True":
            self.hide()
            mainWindow = MainWindow()
            mainWindow.show()

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
            with open("Src/config.toml", "w") as file:
                file.write(f'''token_yandex = "{text}"''')
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
                    with open("Src/config.toml", "w") as file:
                        file.write(f'''token_yandex = "{text}"''')
                    self.hide()
                    mainWindow = MainWindow()
                    mainWindow.show()
            except requests.exceptions.ConnectionError:
                self.error_standart("Проблемы с интернетом", "Проверьте интернет подключенние", exit_or_no=False)
        else:
            self.error_standart("Ввели пустую строку", "Вы ввели пустую строку :)", exit_or_no=False)

class CheckToken(QtCore.QThread):
    mysignal = QtCore.pyqtSignal(str)
    def __init__(self, parent=None):
        QtCore.QThread.__init__(self, parent)
    def run(self):
        try:
            Client(config.get('token_yandex')).init()
            self.mysignal.emit("True")
        except:
            self.mysignal.emit("False")
def main():
    app = QtWidgets.QApplication(sys.argv)  # Новый экземпляр QApplication
    check = Check()  # Создаём объект класса MainWindow
    sys.exit(app.exec())  # и запускаем приложение
if __name__ == '__main__': # Если мы запускаем файл напрямую, а не импортируем
    print("Запуск Yamux")
    main()  # то запускаем функцию main()
