import sys
import toml
import time
import threading
import os
import logging
import webbrowser

from PyQt5 import QtWidgets, uic, QtCore
from PyQt5.QtWidgets import * #  Я знаю что так делать нельзя, но я не собераюсь заниматся се* ради того чтобы было как надо
from PyQt5.QtCore import *
from PyQt5.QtMultimedia import QMediaPlayer, QMediaPlaylist, QMediaContent

from yandex_music import Best, Client, Search
import yandex_music
import requests

logger = logging.getLogger('Yamux logger')

try:
    config = toml.load("config.toml")
    logger.debug("Загрузка config")
except:
    with open("config.toml", "w") as file:
        file.write('''token_yandex = ""''')
    logger.error("Перезайди в программу. Был кривой конфиг, я его пересоздал")

class MainWindow(QtWidgets.QMainWindow, QObject, QUrl):
    def __init__(self, parent=None):
        # Проверяем на рабочий токен и то что интернет работает
        logger.debug("Проверяем на рабочий токен и на рабочий интернет")
        super(MainWindow, self).__init__()
        logger.debug("Инициализация интерфейса")
        uic.loadUi('Ui/Main.ui', self)  # Это нужно для инициализации нашего дизайна
        self.page = 1
        self.current_track = -1
        self.current_track_changed = self.current_track
        self.list_result = []
        self.list_id = []
        self.type_search = "albums"

        self.media_player = QMediaPlayer()
        self.media_player.play()
        self.playlist = QMediaPlaylist()

        self.press_button_pause = lambda: self.media_player.pause()
        self.press_button_stop = lambda: self.media_player.stop()
        self.press_button_to_previous_track = lambda: self.playlist.previous()
        self.press_button_to_next_track = lambda: self.playlist.next()
        self.press_button_to_play = lambda: self.media_player.play()
        self.press_button_to_like = lambda: threading.Thread(target = lambda:self.like(), daemon = True).start()
        self.async_enter_link_to_play = lambda: threading.Thread(target=lambda:self.enter_link_to_play(), daemon=True).start()
        self.press_button_to_search_artist = lambda: self.get_text_write_artist()
        self.msg_btn = lambda i: i.text()
        self.close_event = lambda event: sys.exit()

        self.slider_change_volume.valueChanged.connect(self.media_player.setVolume)
        self.slider_track.valueChanged.connect(self.media_player.setPosition)
        self.media_player.durationChanged.connect(self.update_duration)
        self.media_player.positionChanged.connect(self.update_position)
        self.media_player.mediaStatusChanged.connect(self.next_track)

        self.push_button_to_next_page.clicked.connect(self.next_page_search)
        self.push_button_to_previous_page.clicked.connect(self.previous_page_search)
        self.push_button_to_play_track_url.clicked.connect(self.async_enter_link_to_play)
        self.push_button_to_play.clicked.connect(self.press_button_to_play)
        self.push_button_to_my_wave.clicked.connect(self.play_my_wave_start)
        self.push_button_to_like.clicked.connect(self.press_button_to_like)
        self.push_button_to_download.clicked.connect(self.enter_link_to_download)
        self.push_button_to_pause.clicked.connect(self.press_button_pause)
        self.push_button_to_stop.clicked.connect(self.press_button_stop)
        self.push_button_to_search_albums.clicked.connect(self.get_text_write_albums)
        self.push_button_to_seach_playlist.clicked.connect(self.get_text_write_playlists)
        self.push_button_to_search_tracks.clicked.connect(self.get_text_write_tracks)
        self.push_button_to_best_result.clicked.connect(self.get_text_write_best)
        self.push_button_to_search_artists.clicked.connect(self.press_button_to_search_artist)
        self.push_button_to_previous_track.clicked.connect(self.press_button_to_previous_track)
        self.push_button_to_next_track.clicked.connect(self.press_button_to_next_track)
        self.push_button_to_select_play1.clicked.connect(self.select_play_1)
        self.push_button_to_select_play2.clicked.connect(self.select_play_2)
        self.push_button_to_select_play3.clicked.connect(self.select_play_3)
        self.push_button_to_select_play4.clicked.connect(self.select_play_4)

    def next_track(self):
        media_status = self.media_player.mediaStatus()
        if media_status == 7:
            if self.type_search != "tracks":
                self.loadSound.current_track += 1
                self.loadSound.start()
                self.loadSound.mysignal.connect(self.play_track_qt, QtCore.Qt.QueuedConnection)

    def hhmmss(self, ms):
        h, r = divmod(ms, 36000)
        s, _ = divmod(r, 1000)
        return ("%d:%02d" % (h,s)) if h else ("%d:%02d" % (h,s))

    def update_duration(self, duration):
        self.slider_track.setMaximum(duration)
        if duration >= 0:
            self.total_slider_track.setText(self.hhmmss(duration))

    def update_position(self, position):
        if position >= 0:
            self.current_slider_track.setText(self.hhmmss(position))
        self.slider_track.blockSignals(True)
        self.slider_track.setValue(position)
        self.slider_track.blockSignals(False)

    def select_play_1(self):
        self.loadSound = LoadSound(0, self.list_id, self.type_search)
        self.loadSound.start()
        self.loadSound.mysignal.connect(self.play_track_qt, QtCore.Qt.QueuedConnection)

    def select_play_2(self):
        self.loadSound = LoadSound(1, self.list_id, self.type_search)
        self.loadSound.start()
        self.loadSound.mysignal.connect(self.play_track_qt, QtCore.Qt.QueuedConnection)

    def select_play_3(self):
        self.loadSound = LoadSound(2, self.list_id, self.type_search)
        self.loadSound.start()
        self.loadSound.mysignal.connect(self.play_track_qt, QtCore.Qt.QueuedConnection)

    def select_play_4(self):
        self.loadSound = LoadSound(3, self.list_id, self.type_search)
        self.loadSound.start()
        self.loadSound.mysignal.connect(self.play_track_qt, QtCore.Qt.QueuedConnection)

    def play_track_qt(self, track):
        if track != "End":
            track = QMediaContent(QUrl(track))
            self.media_player.setMedia(track)
            self.media_player.play()
        else:
            self.write_search.setText("Плейлист закончен")
    def get_text_write_artist(self):
        self.type_search = "artists"
        self.load_sound(self.write_search.text(), self.page)

    def get_text_write_albums(self):
        self.type_search = "albums"
        self.load_sound(self.write_search.text(), self.page)

    def get_text_write_tracks(self):
        self.type_search = "tracks"
        self.load_sound(self.write_search.text(), self.page)

    def get_text_write_playlists(self):
        self.type_search = "playlists"
        self.load_sound(self.write_search.text(), self.page)

    def get_text_write_best(self):
        self.type_search = "best"
        self.load_sound(self.write_search.text(), self.page)

    def parsing_sound(self, text):
        import music
        if self.type_search == "albums":
            result = music.send_search_request_and_print_result(text, "albums")
            if result != None:
                self.total_result.setText(f"Я нащёл {result.total} альбомов")
            return result
        if self.type_search == "artists":
            result = music.send_search_request_and_print_result(text, "artists")
            if result != None:
                self.total_result.setText(f"Я нащёл {result.total} артистов")
            return result
        if self.type_search == "tracks":
            result = music.send_search_request_and_print_result(text, "tracks")
            if result != None:
                self.total_result.setText(f"Я нащёл {result.total} треков")
            return result
        if self.type_search == "playlists":
            result = music.send_search_request_and_print_result(text, "playlists")
            if result != None:
                self.total_result.setText(f"Я нащел {result.total} плейлистов")
            return result
        if self.type_search == "best":
            result = music.send_search_request_and_print_result(text, "best")
            if result != None:
                self.total_result.setText(f"Лучший результат это {result.type}")
            return result

    def load_sound(self, text, page, type_search="albums"):
        if len(text.split()) != 0:
            output = self.parsing_sound(text)
            if output is not None:
                self.list_result = []
                self.list_id = []
                if self.type_search == "best":
                    best = "best"
                else:
                    best = None
                    output = output.results
                if best is None:
                    for i in output:
                        if self.type_search == "artists":
                            self.list_id.append(i.id)
                            self.list_result.append(i.name)
                        if self.type_search == "playlists":
                            self.list_id.append(i.kind)
                            self.list_id.append(i.uid)
                            self.list_result.append(i.title)
                        if self.type_search == "albums" or self.type_search == "tracks":
                            self.list_id.append(i.id)
                            self.list_result.append(i.title)
                            try:
                                self.list_result.append(i.artists[0].name)
                            except IndexError:
                                self.list_result.append("")
                    element = self.page * 4
                    if element > 4:
                        for i in range(0, 8):
                            self.list_result.pop(0)
                    if self.type_search == "albums" or self.type_search == "tracks":
                        if element > 4:
                            self.result_search0.setText(f"{self.list_result[0]} - {self.list_result[1]}")
                            self.result_search1.setText(f"{self.list_result[2]} - {self.list_result[3]}")
                            self.result_search2.setText(f"{self.list_result[4]} - {self.list_result[5]}")
                            self.result_search3.setText(f"{self.list_result[6]} - {self.list_result[7]}")
                        else:
                            try:
                                self.result_search0.setText(f"{self.list_result[0]} - {self.list_result[1]}")
                            except IndexError:
                                pass
                            try:
                                self.result_search1.setText(f"{self.list_result[2]} - {self.list_result[3]}")
                            except IndexError:
                                pass
                            try:
                                self.result_search2.setText(f"{self.list_result[4]} - {self.list_result[5]}")
                            except IndexError:
                                pass
                            try:
                                self.result_search3.setText(f"{self.list_result[6]} - {self.list_result[7]}")
                            except IndexError:
                                pass
                    if self.type_search == "artists":
                        if element > 4:
                            self.result_search0.setText(f"{self.list_result[1]}")
                            self.result_search1.setText(f"{self.list_result[2]}")
                            self.result_search2.setText(f"{self.list_result[3]}")
                            self.result_search3.setText(f"{self.list_result[4]}")
                        else:
                            try:
                                self.result_search0.setText(f"{self.list_result[1]}")
                            except IndexError:
                                pass
                            try:
                                self.result_search1.setText(f"{self.list_result[3]}")
                            except IndexError:
                                pass
                            try:
                                self.result_search2.setText(f"{self.list_result[5]}")
                            except IndexError:
                                pass
                            try:
                                self.result_search3.setText(f"{self.list_result[7]}")
                            except IndexError:
                                pass
                    if self.type_search == "playlists":
                        if element > 4:
                            self.result_search0.setText(f"{self.list_result[0]}")
                            self.result_search1.setText(f"{self.list_result[1]}")
                            self.result_search2.setText(f"{self.list_result[2]}")
                            self.result_search3.setText(f"{self.list_result[3]}")
                        else:
                            try:
                                self.result_search0.setText(f"{self.list_result[0]}")
                            except IndexError:
                                pass
                            try:
                                self.result_search1.setText(f"{self.list_result[1]}")
                            except IndexError:
                                pass
                            try:
                                self.result_search2.setText(f"{self.list_result[2]}")
                            except IndexError:
                                pass
                            try:
                                self.result_search3.setText(f"{self.list_result[3]}")
                            except IndexError:
                                pass
                else:
                    self.type_search = output.type
                    output = output.result
                    if self.type_search == "artist":
                        self.list_id.append(output.id)
                        self.list_result.append(output.name)
                    if self.type_search == "album" or self.type_search == "track":
                        self.list_id.append(output.id)
                        self.list_result.append(output.title)
                        try:
                            self.list_result.append(output.artists[0].name)
                        except IndexError:
                            self.list_result.append("")
                    self.type_search += "s"
                    try:
                        self.result_search0.setText(f"{self.list_result[0]} - {self.list_result[1]}")
                    except IndexError:
                        self.result_search0.setText(f"{self.list_result[0]}")
            else:
                self.result_search0.setText("Ничего не найдено")
        else:
            self.result_search0.setText("Вводи не пустую строку")

    def next_page_search(self):
        if self.page != 2:
            self.page += 1
            self.load_sound(self.write_search.text(), self.page, self.type_search)

    def previous_page_search(self):
        if self.page > 1:
            self.page -= 1
            self.load_sound(self.write_search.text(), self.page, self.type_search)
#Я знаю что это плохой код. Но я не знаю как повесить на одну функцию 4 обработчика клавиш. Вообщем, пишите в тг если вы эксперт в PyQt и вы знаете как можно сделать лучше, или сделайте пулл реквест :)

    def play_my_wave_start(self):
        text, ok = QInputDialog.getText(self, 'Сколько песен?', 'Сколько песен вы хотите послушать из Моей волны?')
        self.play_my_wave(text=text, ok=ok)
    def like(self):
        print("Кнопка временно заблокирована")
        #import music
        #data = self.json_data_track.volumes[0]
        #music.client.users_likes_tracks_add(data[self.current_track].id)
    def play_my_wave(self, text, ok):
        import music

        self.type_search = "my_wave"
        if ok:
            try:
                self.loadSound = LoadSound(select_track_or_count=int(text), type_search=self.type_search)
                self.loadSound.start()
                self.loadSound.mysignal.connect(self.play_track_qt, QtCore.Qt.QueuedConnection)
            except ValueError:
               self.error_standart("Ошибка", f"Ошибка: ValueError. Напишите цифрами, а не буквами ;)", exit_or_no=False)

    def play_one_track(self, track_id):
        import music
        track = music.extract_direct_link_to_track(track_id)
        self.player = QMediaPlayer()
        self.player.play()
        self.player.setMedia(QMediaContent(QUrl(track)))
        self.player.play()
        time.sleep(music.duration_track(track_id))

    def enter_link_to_play(self):
        url = self.write_link_to_play.text()
        if url.split(".")[0] == "https://music" and url.split(".")[1] == "yandex":
            check_in_track = url.split("/")
            url_parts=url.split('/')
            list_source = []
            if check_in_track[-3] == "artist" or check_in_track[-2] == "artist":
                self.type_search = "artists"
                try:
                    id_ = check_in_track[-1]
                    self.list_result = []
                    self.list_result.append(id_)
                    self.loadSound = LoadSound(1, self.list_result, self.type_search)
                    self.loadSound.start()
                    self.loadSound.mysignal.connect(self.play_track_qt, QtCore.Qt.QueuedConnection)
                except TypeError():
                    id_ = check_in_track[-2]
                    self.list_result = []
                    self.list_result.append(id_)
                    self.loadSound = LoadSound(1, self.list_result, self.type_search)
                    self.loadSound.start()
                    self.loadSound.mysignal.connect(self.play_track_qt, QtCore.Qt.QueuedConnection)
            if check_in_track[-2] == "playlists":
                self.type_search = "playlists"
                id_ = check_in_track[-1]
                self.list_result = []
                self.list_result.append(id_)
                self.loadSound = LoadSound(1, self.list_result, self.type_search)
                self.loadSound.start()
                self.loadSound.mysignal.connect(self.play_track_qt, QtCore.Qt.QueuedConnection)

            if check_in_track[-2] == "album":
                self.type_search = "albums"
                id_ = check_in_track[-1]
                self.list_result = []
                self.list_result.append(id_)
                self.loadSound = LoadSound(1, self.list_result, self.type_search)
                self.loadSound.start()
                self.loadSound.mysignal.connect(self.play_track_qt, QtCore.Qt.QueuedConnection)
            try:
                if check_in_track[-2] == "track" and isinstance(int(check_in_track[-1]), int):
                    self.type_search = "tracks"
                    id_ = check_in_track[-1]
                    self.list_result = []
                    self.list_result.append(id_)
                    self.loadSound = LoadSound(1, self.list_result, self.type_search)
                    self.loadSound.start()
                    self.loadSound.mysignal.connect(self.play_track_qt, QtCore.Qt.QueuedConnection)
            except TypeError():
                pass
        else:
            self.write_link_to_play.setText(f"Похоже вы вставили неправильную ссылку")

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
                file.write(f'''token_yandex = "{text}"''')
        else:
            if exit_or_no:
                self.media_player.stop()
                sys.exit()

class LoadSound(QtCore.QThread):
    mysignal = QtCore.pyqtSignal(str)
    current_track = -1
    def __init__(self, select_track_or_count, list_result=[], type_search="track", parent=None):
        self.select_track_or_count = select_track_or_count
        self.list_result = list_result
        self.type_search = type_search

        self.list_id = []
        self.list_track = []
        self.len_list = 0
        self.block = 0

        QtCore.QThread.__init__(self, parent)

    def run(self):
        import music
        if self.list_result != [] or self.type_search == "my_wave":
            try:
                self.media_player.stop()
            except AttributeError:
                pass
            if self.current_track == -1:
                if len(self.list_result) > 0 or self.type_search == "my_wave":
                    self.current_track += 1
                    select_track_or_count = self.select_track_or_count

                    for i in self.list_result:
                        try:
                            int(i)
                        except:
                            self.list_result.pop(0)

                    if self.type_search == "albums":
                        album = music.client.albums_with_tracks(self.list_result[select_track_or_count])
                        for i in album.volumes[0]:
                            self.list_id.append(i.id)
                        self.len_list = len(album.volumes[0])

                    if self.type_search == "playlists":
                        self.list_id.append(1)

                    if self.type_search == "artists":
                        artist_track = music.client.artists_tracks(self.list_result[self.select_track_or_count - 1])
                        for i in artist_track.tracks:
                            self.list_id.append(i.id)
                        self.len_list = len(artist_track.tracks)

                    if self.type_search == "my_wave":
                        for i in range(0, int(self.select_track_or_count)):
                            my_wave = music.my_wave()
                            if my_wave.get('responce') == "ok":
                                self.list_id.append(my_wave.get('id'))
                        self.len_list = len(self.list_id)

                    if self.type_search == "tracks":
                        track = music.extract_direct_link_to_track(self.list_result[self.select_track_or_count - 1])
                        self.mysignal.emit(track)

                    if len(self.list_id) > 0:
                        if self.type_search == "playlists":
                            self.list_id.pop(0)
                            playlist_track = music.client.users_playlists(self.list_result[self.select_track_or_count], self.list_result[self.select_track_or_count + 1])
                            for i in playlist_track.tracks:
                                self.list_id.append(i.id)
                            self.len_list = len(playlist_track.tracks)

                        track = music.extract_direct_link_to_track(self.list_id[0])
                        self.mysignal.emit(track)

            if self.current_track > 0:
                try:
                    print(self.list_id)
                    id_track = self.list_id[self.current_track]
                    print(id_track)
                    track = music.extract_direct_link_to_track(id_track)
                    print(track)
                    self.mysignal.emit(track)
                except IndexError():
                    self.mysignal.emit("End")

class Check(QtWidgets.QMainWindow, QObject):
    def __init__(self):
        # Проверяем на рабочий токен и то что интернет работает
        logger.debug("Проверяем на рабочий токен и на рабочий интернет")
        super(Check, self).__init__()
        logger.debug("Инициализация интерфейса")
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
                logger.debug("Всё ок, токен загружен")
                mainWindow = MainWindow()
                mainWindow.show()

        except yandex_music.exceptions.NetworkError:
            logger.error("Проблемы с интернетом yandex_music.exceptions.NetworkError")
            self.error_standart("Проблемы с интернетом",
                "Yandex Music Api не видит вашего интернета, проверь, всё ли с ним в порядке",
                exit_or_no=True)
            self.show()
            self.msg_btn = lambda i: i.text()
            self.reg_button.clicked.connect(self.reg_button_click)

        except yandex_music.exceptions.UnauthorizedError:
            logger.error("Неправильный токен yandex_music.exceptions.UnauthorizedError")
            self.show()
            self.error_standart("Неправильный токен", "YandexMusic говорит у вас неправильный токен, перезарегистрируйтесь", exit_or_no=False)
            self.msg_btn = lambda i: i.text()
            self.reg_button.clicked.connect(self.reg_button_click)
        except UnicodeEncodeError:
            self.show()
            logger.error("Токен из непонятных символов UnicodeEncodeError")
            self.error_standart("Токен из непонятных символов",
                              "YandexMusic говорит что у вас токен из непонятных символов, перезарегистрируйтесь", exit_or_no=False)
            self.msg_btn = lambda i: i.text()
            self.reg_button.clicked.connect(self.reg_button_click)
        except NameError:
            self.show()
            logger.error("Только что создался конфиг, перезапустите программу NameError")
            self.error_standart("Что-то не то с конфигом",
                "У вас что-то не то было с конфигом, перезарегистрируйтесь", exit_or_no=False)
            self.msg_btn = lambda i: i.text()
            self.reg_button.clicked.connect(self.reg_button_click)

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
                sys.exit()
        else:
            error.setStandardButtons(QMessageBox.Ok)
            if exit_or_no:
                sys.exit()

    def error_config(self, message_log, description, windows_title="Ошибка", exit_or_no=True):
        logger.error(message_log)
        text, ok = QInputDialog.getText(self, windows_title, description)
        if ok:
            with open("config.toml", "w") as file:
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
                    with open("config.toml", "w") as file:
                        file.write(f'''token_yandex = "{text}"''')
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
