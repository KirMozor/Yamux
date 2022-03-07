import sys  # sys нужен для передачи argv в QApplication
import mpv  # Модуль для проигрывания музыки
import locale  # Какая-то зависимость без которой Mpv не работает
import toml  #  Библиотека для конфигов

from PyQt5 import QtWidgets  #  Импорт PyQt5
from PyQt5.QtWidgets import *
from PyQt5.QtCore import *

from yandex_music import Best, Client, Search  # Импорт библиотеки YandexMusic
import yandex_music

import os  # Автоматическая конвертация .ui файла в файл design.py, потому что мне лень лезть каждый раз в терминал
os.system("pyuic5 Ui/Main.ui -o design.py ")
import design # Это наш конвертированный файл дизайна

try:
    config = toml.load("config.toml")
except FileNotFoundError:
    file = open("config.toml", "w")
    file.write('tokenYandex = ""')
    file.close()

class ExampleApp(QtWidgets.QMainWindow, design.Ui_MainWindow):
    def __init__(self):
        logText = ""
        # Это здесь нужно для доступа к переменным, методам
        # и т.д. в файле design.py
        super().__init__()
        logText += "\nИнициализация интерфейса"
        self.setupUi(self)  # Это нужно для инициализации нашего дизайна
        self.showLog.setText(logText)

        # Проверяем на рабочий токен и то что интернет работает
        logText += "\nПроверяем на рабочий токен и на рабочий интернет"
        self.showLog.setText(logText)
        try:
            if not config.get('tokenYandex'):
                text, ok = QInputDialog.getText(self, 'Добавить токен', 'Я не обнаружил токена YandexMusic в программе. Если у вас нет токена то инструкция по получению токена находится в README.md')
                if ok:
                    file = open("config.toml", "w")
                    file.write(f'tokenYandex = "{text}"')
                    file.close()
            global client 
            client = Client(config.get('tokenYandex')).init()
        except yandex_music.exceptions.NetworkError:
            logText += "\nПроблемы с интернетом yandex_music.exceptions.NetworkError"
            self.showLog.setText(logText)
            self.errorStandart("Проблемы с интернетом",
                "Yandex Music Api не видит вашего интернета, проверь, всё ли с ним в порядке",
                exitOrNo=True)
        except yandex_music.exceptions.Unauthorized:
            logText += "\nНеправильный токен yandex_music.exceptions.Unauthorized"
            self.showLog.setText(logText)
            self.errorConfig("Неправильный токен",
                "Yandex Music Api говорит что у вас нерабочий токен. Инструкция по получению токена находится в README.md",
                windowsTitle="Добавить токен")
        except UnicodeEncodeError:
            logText += "\nТокен из непонятных символов UnicodeEncodeError"
            self.showLog.setText(logText)
            self.errorConfig("Токен из непонятных символов", 
                "YandexMusic говорит что у вас токен из непонятных символов, ведите корректный токен сюда. Инструкция по получению токена находится в README.md",
                windowsTitle="Добавить токен")
        except NameError:
            logText += "\nТолько что создался конфиг, перезапустите программу NameError"
            self.showLog.setText(logText)
            self.errorConfig("Только что создался конфиг, перезапустите программу",
                "У вас нету файла с настройками программы, я его создам, но мне нужно знать токен от YandexMusic. Инструкция по получению токена находится в README.md",
                windowsTitle="Добавить токен")
        self.pushButtonToPlay.clicked.connect(self.enterLinkToPlay)
        self.pushButtonToDownload.clicked.connect(self.enterLinkToDownload)
    def exit(self):
        logText += "\nЗапускаю функцию exit"
        self.showLog.setText(logText)
        sys.exit()
    def playLocalFile(self):
        logText += "\nЗапустился playLocalFile"
        self.showLog.setText(logText)
        wb_patch = QtWidgets.QFileDialog.getOpenFileName()[0]
        locale.setlocale(locale.LC_NUMERIC, 'C')
        player = mpv.MPV()
        player.play(wb_patch)
    def enterLinkToPlay(self):
        import music
        url = self.writeLinkToPlay.text()
        if url and url.strip():
            if url.split(".")[0] == "https://music" and url.split(".")[1] == "yandex":
                self.writeLinkToPlay.setText("Запускаю трек")
                url_parts=url.split('/')
                trackID = url_parts[-1]
                track = music.extractDirectLinkToTrack(trackID)
                play(track)
            else:
                self.errorStandart("Неправильная ссылка", "Похоже вы вставили неправильную ссылку", exitOrNo=False)
        else:
            self.writeLinkToPlay.setText("Напиши сюда ссылку на трек или плейлист из YandexMusic, а не пустую строку :)")

    def enterLinkToDownload(self):
        import music
        url = self.writeLinkToPlay.text()
        if url and url.strip():
            if url.split(".")[0] == "https://music" and url.split(".")[1] == "yandex":
                url = str(url)
                wb_patch = QtWidgets.QFileDialog.getExistingDirectory()
                if os.path.isdir(wb_patch):
                    self.writeLinkToPlay.setText("Начинаю скачивать :)")
                    path = music.download(url, wb_patch)
                    self.writeLinkToPlay.setText(f"Скачалось по пути {path}")
            else:
                self.errorStandart("Неправильная ссылка", "Похоже вы вставили неправильную ссылку", exitOrNo=False)
        else:
            self.writeLinkToPlay.setText("Напиши сюда ссылку на трек или плейлист из YandexMusic, а не пустую строку :)")

    def errorStandart(self, messageLog, description, windowsTitle="Ошибка", exitOrNo=True):
        print(messageLog)
        error = QMessageBox()
        error.setWindowTitle(windowsTitle)
        error.setIcon(QMessageBox.Warning)
        error.setText(description)
        error.setIcon(QMessageBox.Warning)
        error.setStandardButtons(QMessageBox.Close)
        if exitOrNo:
            error.buttonClicked.connect(self.exit)
        error.exec_()
    def errorConfig(self, messageLog, description, windowsTitle="Ошибка", exitOrNo=True):
        print(messageLog)
        text, ok = QInputDialog.getText(self, windowsTitle, description)
        if ok:
            file = open("config.toml", "w")
            file.write(f'tokenYandex = "{text}"')
            file.close()
        if exitOrNo:
            error.buttonClicked.connect(self.exit)


def play(url):
    locale.setlocale(locale.LC_NUMERIC, 'C')
    player = mpv.MPV()
    player.play(url)


def main():
    app = QtWidgets.QApplication(sys.argv)  # Новый экземпляр QApplication
    window = ExampleApp()  # Создаём объект класса ExampleApp
    window.show()  # Показываем окно
    app.exec_()  # и запускаем приложение

if __name__ == '__main__':  # Если мы запускаем файл напрямую, а не импортируем
    main()  # то запускаем функцию main()