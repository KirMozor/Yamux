import sys  # sys нужен для передачи argv в QApplication
import mpv  # Модуль для проигрывания музыки
import locale  # Какая-то зависимость без которой Mpv не работает
import toml  #Библиотека для конфигов

from PyQt5 import QtWidgets  #  Импорт PyQt5
from PyQt5.QtWidgets import *
from PyQt5.QtCore import *

from yandex_music import Best, Client, Search  # Импорт библиотеки YandexMusic
import yandex_music

import os  # Автоматическая конвертация .ui файла в файл design.py, потому что мне лень лезть каждый раз в терминал
os.system("pyuic5 Main.ui -o design.py ")
import design # Это наш конвертированный файл дизайна

try:
        config = toml.load("config.toml")
except FileNotFoundError:
        file = open("config.toml", "w")
        file.write('tokenYandex = ""')
        file.close()
        
class ExampleApp(QtWidgets.QMainWindow, design.Ui_MainWindow):
        def __init__(self):
                # Это здесь нужно для доступа к переменным, методам
                # и т.д. в файле design.py
                super().__init__()
                self.setupUi(self)  # Это нужно для инициализации нашего дизайна
                
                # Проверяем на рабочий токен и то что интернет работает
                try:
                        client = Client.from_token(config.get('tokenYandex'))
                except yandex_music.exceptions.NetworkError:
                        print("Проблемы с интернетом")
                except yandex_music.exceptions.Unauthorized:
                        print("Неправильный токен")
                except NameError:
                        print("Перезапустите программу")

                self.playButton.clicked.connect(self.play)
                self.enterTokenButton.clicked.connect(self.enter)
        def play(self):
                wb_patch = QtWidgets.QFileDialog.getOpenFileName()[0]
                locale.setlocale(locale.LC_NUMERIC, 'C')
                player = mpv.MPV()
                player.play(wb_patch)
        def enter(self):
                text = self.lineEditToken.text()
                if text and text.strip():
                        self.lineEditToken.setText("Спасибо за токен, сейчас проверю его")
                else:
                        self.lineEditToken.setText("Напишите сюда токен от YandexMusic, а не пустую строку :)")
def main():
        app = QtWidgets.QApplication(sys.argv)  # Новый экземпляр QApplication
        window = ExampleApp()  # Создаём объект класса ExampleApp
        window.show()  # Показываем окно
        app.exec_()  # и запускаем приложение

if __name__ == '__main__':  # Если мы запускаем файл напрямую, а не импортируем
        main()  # то запускаем функцию main()