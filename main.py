import sys  # sys нужен для передачи argv в QApplication
import vlc  # Модуль для проигрывания музыки
import toml  #  Библиотека для конфигов
import time  #  Для того чтобы дожидался когда закончится трек
import threading  #  Библиотека для ассинхронности
import os  #  Для проверки на наличие папки, потом ещё функционалла докину

from PyQt5 import QtWidgets, uic  #  Импорт PyQt5
from PyQt5.QtWidgets import * #  Я знаю что так делать нельзя, но я не собераюсь заниматся се* ради того чтобы было как надо
from PyQt5.QtCore import *

from yandex_music import Best, Client, Search  # Импорт библиотеки YandexMusic
import yandex_music

from bs4 import BeautifulSoup  # Библиотеки для парсинга
import requests

try:
    config = toml.load("config.toml")
except FileNotFoundError:
    file = open("config.toml", "w")
    file.write('tokenYandex = ""')
    file.close()
except toml.decoder.TomlDecodeError:
    file = open("config.toml", "w")
    file.write('tokenYandex = ""')
    file.close()

    print("Перезайди в программу. Был кривой конфиг, я его пересоздал")
class MainWindow(QtWidgets.QMainWindow):
    def __init__(self):
        logText = ""
        # Это здесь нужно для доступа к переменным, методам
        # и т.д. в файле design.py
        super().__init__()
        logText += "Инициализация интерфейса"
        uic.loadUi('Ui/Main.ui', self)  # Это нужно для инициализации нашего дизайна
        self.showLog.setText(logText)

        self.notRunMusic = True

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
                else:
                    sys.exit()
            global client 
            client = Client(config.get('tokenYandex')).init()
            logText += "\nВсё ок"
            self.showLog.setText(logText)

        except yandex_music.exceptions.NetworkError:
            logText += "\nПроблемы с интернетом yandex_music.exceptions.NetworkError"
            self.showLog.setText(logText)
            self.errorStandart("Проблемы с интернетом",
                "Yandex Music Api не видит вашего интернета, проверь, всё ли с ним в порядке",
                exitOrNo=True)
        except yandex_music.exceptions.UnauthorizedError:
            logText += "\nНеправильный токен yandex_music.exceptions.UnauthorizedError"
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

        self.pressButtonPause = lambda: self.mediaPlayer.pause()
        self.pressButtonStop = lambda: self.mediaPlayer.stop()
        self.pressButtonToPreviousTrack = lambda: self.mediaPlayer.previous()
        self.pressButtonToNextTrack = lambda: self.mediaPlayer.next()
        self.asyncEnterLinkToPlay = lambda: threading.Thread(target=lambda:self.enterLinkToPlay(), daemon=True).start()
        self.msgbtn = lambda i: i.text()
        self.closeEvent = lambda event: sys.exit()

        self.pushButtonToPlay.clicked.connect(self.asyncEnterLinkToPlay)
        self.pushButtonToDownload.clicked.connect(self.enterLinkToDownload)
        self.pushButtonToPause.clicked.connect(self.pressButtonPause)
        self.pushButtonToStop.clicked.connect(self.pressButtonStop)
        self.pushButtonToPreviousTrack.clicked.connect(self.pressButtonToPreviousTrack)
        self.pushButtonToNextTrack.clicked.connect(self.pressButtonToNextTrack)

    def enterLinkToPlay(self):
        import music
        url = self.writeLinkToPlay.text().rstrip('/')
        if url and url.strip():
            if url.split(".")[0] == "https://music" and url.split(".")[1] == "yandex":
                checkInTrack = url.split("/")
                if checkInTrack[-2] == "track":
                    url_parts=url.split('/')
                    trackID = url_parts[-1]
                    track = music.extractDirectLinkToTrack(trackID)

                    self.mediaPlayer = vlc.MediaPlayer(track)
                    self.mediaPlayer.play()
                    time.sleep(music.durationTrack(url))
                else:
                    response = requests.get(url)
                    soup = BeautifulSoup(response.text, 'lxml')

                    quotes = soup.find_all('a', class_='d-track__title deco-link deco-link_stronger')
                    if not quotes:
                        self.errorStandart("Неправильная ссылка", "Похоже вы вставили неправильную ссылку", exitOrNo=False)
                    else:
                        self.mediaPlayer = vlc.MediaListPlayer()
                        player = vlc.Instance()
                        self.mediaList = player.media_list_new()
                        for title in quotes:
                                s = title.text.strip(), title.get('href')   
                                url = "https://music.yandex.ru" + s[1]

                                url_parts=url.split('/')
                                trackID = url_parts[-1]
                                track = music.extractDirectLinkToTrack(trackID)

                                media = player.media_new(track)
                                self.mediaList.add_media(media)
                        self.mediaPlayer.set_media_list(self.mediaList)
                        new = player.media_player_new()
                        self.mediaPlayer.set_media_player(new)
                        self.mediaPlayer.play()
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
                    if path.get('responce') == "ok":
                        self.writeLinkToPlay.setText(f"Скачалось по пути {path.get('text')}")
                    else:
                        self.errorStandart("Ошибка скачивания", f"{path.get('text')}", exitOrNo=False, notButtonCancel=True)
            else:
                self.errorStandart("Неправильная ссылка", "Похоже вы вставили неправильную ссылку", exitOrNo=False)
        else:
            self.writeLinkToPlay.setText("Напиши сюда ссылку на трек или плейлист из YandexMusic, а не пустую строку :)")

    def errorStandart(self, messageLog, description, windowsTitle="Ошибка", exitOrNo=True, notButtonCancel=False):
        print(messageLog)
        error = QMessageBox()
        error.setWindowTitle(windowsTitle)
        error.setIcon(QMessageBox.Warning)
        error.setText(description)
        error.setIcon(QMessageBox.Warning)
        if not notButtonCancel:
            error.setStandardButtons(QMessageBox.Ok | QMessageBox.Cancel)
            error.buttonClicked.connect(self.msgbtn)
            retval = error.exec_()
            if exitOrNo:
                self.mediaPlayer.stop()
                sys.exit()
            if retval == 4194304:
                self.mediaPlayer.stop()
                sys.exit()
        else:
            error.setStandardButtons(QMessageBox.Ok)
            if exitOrNo:
                self.mediaPlayer.stop()
                sys.exit()

    def errorConfig(self, messageLog, description, windowsTitle="Ошибка", exitOrNo=True):
        print(messageLog)
        text, ok = QInputDialog.getText(self, windowsTitle, description)
        if ok:
            file = open("config.toml", "w")
            file.write(f'tokenYandex = "{text}"')
            file.close()
        else:
            if exitOrNo:
                self.mediaPlayer.stop()
                sys.exit()

def main():
    app = QtWidgets.QApplication(sys.argv)  # Новый экземпляр QApplication
    mainWindow = MainWindow()  # Создаём объект класса MainWindow
    mainWindow.show()  # Показываем окно
    sys.exit( app.exec_() )  # и запускаем приложение

if __name__ == '__main__':  # Если мы запускаем файл напрямую, а не импортируем
    main()  # то запускаем функцию main()