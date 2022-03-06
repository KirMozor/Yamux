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
                # Это здесь нужно для доступа к переменным, методам
                # и т.д. в файле design.py
                super().__init__()
                self.setupUi(self)  # Это нужно для инициализации нашего дизайна
                
                # Проверяем на рабочий токен и то что интернет работает
                try:
                        if not config.get('tokenYandex'):
                                text, ok = QInputDialog.getText(self, 'Добавить токен', 'Я не обнаружил токена YandexMusic в программе, введите его:')
                                if ok:
                                        file = open("config.toml", "w")
                                        file.write(f'tokenYandex = "{text}"')
                                        file.close()
                        global client 
                        client = Client.from_token(config.get('tokenYandex'))
                except yandex_music.exceptions.NetworkError:
                        print("Проблемы с интернетом")
                        error = QMessageBox()
                        error.setWindowTitle("Ошибка")
                        error.setText("Yandex Music Api не видит вашего интернета, проверь, всё ли с ним в порядке")
                        error.setIcon(QMessageBox.Warning)
                        error.setStandardButtons(QMessageBox.Close)
                        error.buttonClicked.connect(self.exit)
                        error.exec_()
                except yandex_music.exceptions.Unauthorized:
                        print("Неправильный токен")
                        text, ok = QInputDialog.getText(self, 'Добавить токен', 'Yandex Music Api говорит что у вас нерабочий токен')
                        if ok:
                                file = open("config.toml", "w")
                                file.write(f'tokenYandex = "{text}"')
                                file.close()
                        self.exit()
                except UnicodeEncodeError:
                        print("Токен из непонятных символов")
                        text, ok = QInputDialog.getText(self, 'Добавить токен', 'YandexMusic говорит что у вас токен из непонятных символов, ведите корректный токен сюда')
                        if ok:
                                file = open("config.toml", "w")
                                file.write(f'tokenYandex = "{text}"')
                                file.close()
                        self.exit()
                except NameError:
                        print("Только что создался конфиг, перезапустите программу")
                        text, ok = QInputDialog.getText(self, 'Добавить токен', 'У вас нету файла с настройками программы, я его создам, но мне нужно знать токен от YandexMusic')
                        if ok:
                                file = open("config.toml", "w")
                                file.write(f'tokenYandex = "{text}"')
                                file.close()
                        self.exit
                self.pushButtonToPlay.clicked.connect(self.enterLinkToPlay)
                self.pushButtonToDownload.clicked.connect(self.enterLinkToDownload)
        def exit(self):
                sys.exit()
        def playLocalFile(self):
                wb_patch = QtWidgets.QFileDialog.getOpenFileName()[0]
                locale.setlocale(locale.LC_NUMERIC, 'C')
                player = mpv.MPV()
                player.play(wb_patch)
        def enterLinkToPlay(self):
                import music
                url = self.writeLinkToPlay.text()
                if url and url.strip():
                        self.writeLinkToPlay.setText("Запускаю трек")
                        url_parts=url.split('/')
                        trackID = url_parts[-1]
                        track = music.extractDirectLinkToTrack(trackID)
                        play(track)
                else:
                        self.writeLinkToPlay.setText("Напиши сюда ссылку на трек из YandexMusic, а не пустую строку :)")
        def enterLinkToDownload(self):
                import music
                url = self.writeLinkToPlay.text()
                if url and url.strip():
                        url = str(url)
                        wb_patch = QtWidgets.QFileDialog.getExistingDirectory()
                        if os.path.isdir(wb_patch):
                                self.writeLinkToPlay.setText("Начинаю скачивать :)")
                                path = music.download(url, wb_patch)
                                self.writeLinkToPlay.setText(f"Скачалось по пути {path}")
                else:
                        self.writeLinkToPlay.setText("Напиши сюда ссылку на трек из YandexMusic, а не пустую строку :)")
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