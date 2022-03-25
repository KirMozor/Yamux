import sys
import vlc
import toml
import time
import threading
import os
import logging

from PyQt5 import QtWidgets, uic
from PyQt5.QtWidgets import * #  Я знаю что так делать нельзя, но я не собераюсь заниматся се* ради того чтобы было как надо
from PyQt5.QtCore import *

from yandex_music import Best, Client, Search
import yandex_music

from bs4 import BeautifulSoup
import requests

logger = logging.getLogger('Yamux logger')

try:
    config = toml.load("config.toml")
    logger.debug("Загрузка config")
except:
    with open("config.toml", "w") as file:
        file.write('token_yandex = ""')
    logger.error("Перезайди в программу. Был кривой конфиг, я его пересоздал")

class MainWindow(QtWidgets.QMainWindow, QObject):
    def __init__(self):
        super(MainWindow, self).__init__()
        logger.debug("Инициализация интерфейса")
        uic.loadUi('Ui/Main.ui', self)  # Это нужно для инициализации нашего дизайна
        self.page = 1

        # Проверяем на рабочий токен и то что интернет работает
        logger.debug("Проверяем на рабочий токен и на рабочий интернет")

        try:
            if not config.get('token_yandex'):
                text, ok = QInputDialog.getText(self, 'Добавить токен', 'Я не обнаружил токена YandexMusic в программе. Если у вас нет токена то инструкция по получению токена находится в README.md')
                logger.warning("Не найден токен в конфиге, жду ввод от пользователя")
                if ok:
                    with open("config.toml", "w") as file:
                        file.write(f'token_yandex = "{text}"')
                        logger.debug(f"Записан токен в конфиг config.toml. Токен: {text}")
                else:
                    logger.debug("Нажана кнопка exit, выход")
                    sys.exit()
            client = Client(config.get('token_yandex')).init()
            logger.debug("Всё ок, токен загружен")

        except yandex_music.exceptions.NetworkError:
            logger.error("Проблемы с интернетом yandex_music.exceptions.NetworkError")
            self.error_standart("Проблемы с интернетом",
                "Yandex Music Api не видит вашего интернета, проверь, всё ли с ним в порядке",
                exit_or_no=True)
        except yandex_music.exceptions.UnauthorizedError:
            logger.error("Неправильный токен yandex_music.exceptions.UnauthorizedError")
            self.error_config("Неправильный токен",
                "Yandex Music Api говорит что у вас нерабочий токен. Инструкция по получению токена находится в README.md",
                windows_title="Добавить токен")
        except UnicodeEncodeError:
            logger.error("Токен из непонятных символов UnicodeEncodeError")
            self.error_config("Токен из непонятных символов",
                              "YandexMusic говорит что у вас токен из непонятных символов, ведите корректный токен сюда. Инструкция по получению токена находится в README.md",
                windows_title="Добавить токен")
        except NameError:
            logger.error("Только что создался конфиг, перезапустите программу NameError")
            self.error_config("Только что создался конфиг, перезапустите программу",
                "У вас нету файла с настройками программы, я его создам, но мне нужно знать токен от YandexMusic. Инструкция по получению токена находится в README.md",
                windows_title="Добавить токен")

        self.press_button_pause = lambda: self.media_player.pause()
        self.press_button_stop = lambda: self.media_player.stop()
        self.press_button_to_previous_track = lambda: self.media_player.previous()
        self.press_button_to_next_track = lambda: self.media_player.next()
        self.async_enter_link_to_play = lambda: threading.Thread(target=lambda:self.enter_link_to_play(), daemon=True).start()
        self.msg_btn = lambda i: i.text()
        self.close_event = lambda event: sys.exit()

        self.push_button_to_search.clicked.connect(self.get_text_write_sound)
        self.push_button_to_next_page.clicked.connect(self.next_page_search)
        self.push_button_to_previous_page.clicked.connect(self.previous_page_search)
        self.push_button_to_play.clicked.connect(self.async_enter_link_to_play)
        self.push_button_to_my_wave.clicked.connect(self.play_my_wave_start)
        self.push_button_to_download.clicked.connect(self.enter_link_to_download)
        self.push_button_to_pause.clicked.connect(self.press_button_pause)
        self.push_button_to_stop.clicked.connect(self.press_button_stop)
        self.push_button_to_previous_track.clicked.connect(self.press_button_to_previous_track)
        self.push_button_to_next_track.clicked.connect(self.press_button_to_next_track)

    def get_text_write_sound(self):
        self.load_sound(self.write_search.text(), self.page)

    def parsing_sound(self, text):
        import music
        result = music.send_search_request_and_print_result(text, "albums")
        self.total_result.setText(f"Я нащёл {result.total} альбомов")
        return result

    def load_sound(self, text, page):
        result = self.parsing_sound(text)
        list_result = []
        for i in result.results:
            list_result.append(i.id)
            list_result.append(i.title)
            list_result.append(i.artists[0].name)
        element = self.page * 4
        if element > 4:
            for i in range(0, 12):
                list_result.pop(0)
            self.result_search0.setText(f"{list_result[1]} - {list_result[2]}")
            self.result_search1.setText(f"{list_result[4]} - {list_result[5]}")
            self.result_search2.setText(f"{list_result[7]} - {list_result[8]}")
            self.result_search3.setText(f"{list_result[10]} - {list_result[11]}")
        else:
            self.result_search0.setText(f"{list_result[1]} - {list_result[2]}")
            self.result_search1.setText(f"{list_result[4]} - {list_result[5]}")
            self.result_search2.setText(f"{list_result[7]} - {list_result[8]}")
            self.result_search3.setText(f"{list_result[10]} - {list_result[11]}")

    def next_page_search(self):
        self.page += 1
        self.load_sound(self.write_search.text(), self.page)

    def previous_page_search(self):
        if self.page > 1:
            self.page -= 1
            self.load_sound(self.write_search.text(), self.page)

    def play_my_wave_start(self):
        text, ok = QInputDialog.getText(self, 'Сколько песен?', 'Сколько песен вы хотите послушать из Моей волны?')
        threading.Thread(target=lambda:self.play_my_wave(text=text, ok=ok), daemon=True).start()
        #threading.Thread(target=lambda:self.check(), daemon=True).start()
    def like(self):
        pass
        #Какой любопытный и внимательный (или внимательная ;)

    def play_my_wave(self, text, ok):
        import music

        self.media_player = vlc.MediaListPlayer()
        player = vlc.Instance()
        self.media_list = player.media_list_new()

        if ok:
            try:
                block = 1
                text = int(text)
                list_source = []
                for i in range(0, text):
                    my_wave = music.my_wave()
                    if my_wave.get('responce') == "ok":
                        track = music.extract_direct_link_to_track(my_wave.get('id'))
                        print(f"\n{track}")
                        list_source.append(track)
                        block += 1
                        if block == 11 or text != 10 and block == text + 1:
                            for i in list_source:
                                media = player.media_new(i)
                                self.media_list.add_media(media)
                            self.media_player.set_media_list(self.media_list)
                            new = player.media_player_new()
                            self.media_player.set_media_player(new)
                            self.media_player.play()
                            time.sleep(10)
                            while True:
                                if self.media_player.get_state() == vlc.State.Ended:
                                    block = 1
                                    break
                                else:
                                    time.sleep(10)
                    else:
                        self.error_standart("Ошибка", f"Ошибка: {my_wave.get('text')}", exit_or_no=False)
                self.media_player.set_media_list(self.media_list)
                new = player.media_player_new()
                self.media_player.set_media_player(new)
                self.media_player.play()
            except ValueError:
               self.error_standart("Ошибка", f"Ошибка: ValueError. Напишите цифрами, а не буквами ;)", exit_or_no=False)

    def enter_link_to_play(self):
        logger.debug("Запустилась функция enterLinkToPlay")
        import music
        url = self.write_link_to_play.text().rstrip('/')
        logger.debug(f"url = {url}")

        if url and url.strip():
            if url.split(".")[0] == "https://music" and url.split(".")[1] == "yandex":
                check_in_track = url.split("/")
                if check_in_track[-2] == "track":
                    logger.debug("Это трек")
                    url_parts=url.split('/')
                    track_id = url_parts[-1]
                    logger.debug(f"track")
                    track = music.extract_direct_link_to_track(track_id)

                    self.media_player = vlc.MediaPlayer(track)
                    self.media_player.play()
                    info = music.info_track(url)
                    self.current_track.setText(f" {info.get('name')} - {info.get('artists')}")
                    time.sleep(music.duration_track(url))
                else:
                    response = requests.get(url)
                    soup = BeautifulSoup(response.text, 'lxml')

                    quotes = soup.find_all('a', class_='d-track__title deco-link deco-link_stronger')
                    if not quotes:
                        self.error_standart("Неправильная ссылка", "Похоже вы вставили неправильную ссылку", exit_or_no=False)
                    else:
                        self.media_player = vlc.MediaListPlayer()
                        player = vlc.Instance()
                        self.media_list = player.media_list_new()
                        block = 1
                        list_source = []
                        for title in quotes:
                            s = title.text.strip(), title.get('href')
                            url = "https://music.yandex.ru" + s[1]

                            url_parts=url.split('/')
                            track_id = url_parts[-1]
                            if block == 11:
                                for i in list_source:
                                    media = player.media_new(i)
                                    self.media_list.add_media(media)
                                    print(f"\n{i}")

                                self.media_player.set_media_list(self.media_list)
                                new = player.media_player_new()
                                self.media_player.set_media_player(new)
                                self.media_player.play()
                                time.sleep(10)
                                while True:
                                    if self.media_player.get_state() == vlc.State.Ended:
                                        block = 1
                                        break
                                    else:
                                        time.sleep(10)
                            else:
                                track = music.extract_direct_link_to_track(track_id)
                                list_source.append(track)
                                print(f"\n{track}")
                                block += 1

            else:
                logger.debug("Неправильная ссылка")
                self.error_standart("Неправильная ссылка", "Похоже вы вставили неправильную ссылку", exit_or_no=False)
        else:
            logger.debug("Пустая строка")
            self.write_link_to_play.setText("Напиши сюда ссылку на трек или плейлист из YandexMusic, а не пустую строку :)")

    def enter_link_to_download(self):
        import music
        url = self.write_link_to_play.text()
        if url and url.strip():
            if url.split(".")[0] == "https://music" and url.split(".")[1] == "yandex":
                url = str(url)
                wb_patch = QtWidgets.QFileDialog.getExistingDirectory()
                if os.path.isdir(wb_patch):
                    self.write_link_to_play.setText("Начинаю скачивать :)")
                    path = music.download(url, wb_patch)
                    if path.get('responce') == "ok":
                        self.write_link_to_play.setText(f"Скачалось по пути {path.get('text')}")
                    else:
                        self.error_standart("Ошибка скачивания", f"{path.get('text')}", exit_or_no=False, not_button_cancel=True)
            else:
                self.error_standart("Неправильная ссылка", "Похоже вы вставили неправильную ссылку", exit_or_no=False)
        else:
            self.write_link_to_play.setText("Напиши сюда ссылку на трек или плейлист из YandexMusic, а не пустую строку :)")

    def error_standart(self, message_log, description, windows_title="Ошибка", exit_or_no=True, not_button_cancel=False):
        logger.error(message_log)
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
                self.media_player.stop()
                sys.exit()
            if retval == 4194304:
                self.media_player.stop()
                sys.exit()
        else:
            error.setStandardButtons(QMessageBox.Ok)
            if exit_or_no:
                self.media_player.stop()
                sys.exit()

    def error_config(self, message_log, description, windows_title="Ошибка", exit_or_no=True):
        logger.error(message_log)
        text, ok = QInputDialog.getText(self, windows_title, description)
        if ok:
            with open("config.toml", "w") as file:
                file.write(f'token_yandex = "{text}"')
        else:
            if exit_or_no:
                self.media_player.stop()
                sys.exit()

def main():
    app = QtWidgets.QApplication(sys.argv)  # Новый экземпляр QApplication
    mainWindow = MainWindow()  # Создаём объект класса MainWindow
    mainWindow.show()  # Показываем окно
    sys.exit(app.exec_())  # и запускаем приложение

if __name__ == '__main__': # Если мы запускаем файл напрямую, а не импортируем
    print("Запуск Yamux")
    main()  # то запускаем функцию main()
