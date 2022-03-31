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
        file.write('''token_yandex = ""
block = 10''')
    logger.error("Перезайди в программу. Был кривой конфиг, я его пересоздал")

class MainWindow(QtWidgets.QMainWindow, QObject):
    def __init__(self):
        # Проверяем на рабочий токен и то что интернет работает
        logger.debug("Проверяем на рабочий токен и на рабочий интернет")
        super(MainWindow, self).__init__()
        logger.debug("Инициализация интерфейса")
        uic.loadUi('Ui/Main.ui', self)  # Это нужно для инициализации нашего дизайна
        self.page = 1
        self.current_track = -1
        self.current_track_changed = self.current_track
        self.list_result = []

        self.press_button_pause = lambda: self.media_player.pause()
        self.press_button_stop = lambda: self.stop_media_player()
        self.press_button_to_previous_track = lambda: self.previous_track()
        self.press_button_to_next_track = lambda: self.next_track()
        self.press_button_to_like = lambda: threading.Thread(target = lambda:self.like(), daemon = True).start()
        self.async_enter_link_to_play = lambda: threading.Thread(target=lambda:self.enter_link_to_play(), daemon=True).start()
        self.select_play_1_play = lambda: threading.Thread(target=lambda:self.select_play_1(), daemon=True).start()
        self.select_play_2_play = lambda: threading.Thread(target=lambda:self.select_play_2(), daemon=True).start()
        self.select_play_3_play = lambda: threading.Thread(target=lambda:self.select_play_3(), daemon=True).start()
        self.select_play_4_play = lambda: threading.Thread(target=lambda:self.select_play_4(), daemon=True).start()
        self.msg_btn = lambda i: i.text()
        self.close_event = lambda event: sys.exit()

        self.push_button_to_search.clicked.connect(self.get_text_write_sound)
        self.push_button_to_next_page.clicked.connect(self.next_page_search)
        self.push_button_to_previous_page.clicked.connect(self.previous_page_search)
        self.push_button_to_play.clicked.connect(self.async_enter_link_to_play)
        self.push_button_to_my_wave.clicked.connect(self.play_my_wave_start)
        self.push_button_to_like.clicked.connect(self.press_button_to_like)
        self.push_button_to_download.clicked.connect(self.enter_link_to_download)
        self.push_button_to_pause.clicked.connect(self.press_button_pause)
        self.push_button_to_stop.clicked.connect(self.press_button_stop)
        self.push_button_to_previous_track.clicked.connect(self.press_button_to_previous_track)
        self.push_button_to_next_track.clicked.connect(self.press_button_to_next_track)
        self.push_button_to_select_play1.clicked.connect(self.select_play_1_play)
        self.push_button_to_select_play2.clicked.connect(self.select_play_2_play)
        self.push_button_to_select_play3.clicked.connect(self.select_play_3_play)
        self.push_button_to_select_play4.clicked.connect(self.select_play_4_play)

    def get_text_write_sound(self):
        self.load_sound(self.write_search.text(), self.page)

    def stop_media_player(self):
        self.media_player.stop()
        self.current_track -= 1
        self.current_track_changed = self.current_track

    def next_track(self):
        self.current_track_changed += 1

    def previous_track(self):
        if self.current_track_changed > 0:
            self.current_track_changed -= 1

    def parsing_sound(self, text):
        import music
        result = music.send_search_request_and_print_result(text, "albums")
        if result != None:
            self.total_result.setText(f"Я нащёл {result.total} альбомов")
        return result

    def load_sound(self, text, page):
        if len(text.split()) != 0:
            result = self.parsing_sound(text)
            if result is not None:
                self.list_result = []
                for i in result.results:
                    self.list_result.append(i.id)
                    self.list_result.append(i.title)
                    try:
                        self.list_result.append(i.artists[0].name)
                    except IndexError:
                        self.list_result.append("")
                element = self.page * 4
                if element > 4:
                    for i in range(0, 12):
                        self.list_result.pop(0)
                    self.result_search0.setText(f"{self.list_result[1]} - {self.list_result[2]}")
                    self.result_search1.setText(f"{self.list_result[4]} - {self.list_result[5]}")
                    self.result_search2.setText(f"{self.list_result[7]} - {self.list_result[8]}")
                    self.result_search3.setText(f"{self.list_result[10]} - {self.list_result[11]}")
                else:
                    try:
                        self.result_search0.setText(f"{self.list_result[1]} - {self.list_result[2]}")
                    except IndexError:
                        pass
                    try:
                        self.result_search1.setText(f"{self.list_result[4]} - {self.list_result[5]}")
                    except IndexError:
                        pass
                    try:
                        self.result_search2.setText(f"{self.list_result[7]} - {self.list_result[8]}")
                    except IndexError:
                        pass
                    try:
                        self.result_search3.setText(f"{self.list_result[10]} - {self.list_result[11]}")
                    except IndexError:
                        pass
            else:
                self.result_search0.setText("Ничего не найдено")
        else:
            self.result_search0.setText("Вводи не пустую строку")

    def next_page_search(self):
        if self.page != 2:
            self.page += 1
            self.load_sound(self.write_search.text(), self.page)

    def previous_page_search(self):
        if self.page > 1:
            self.page -= 1
            self.load_sound(self.write_search.text(), self.page)
#Я знаю что это плохой код. Но я не знаю как повесить на одну функцию 4 обработчика клавиш. Вообщем, пишите в тг если вы эксперт в PyQt и вы знаете как можно сделать лучше, или сделайте пулл реквест :)
    def select_play_1(self):
        import music
        if self.list_result != []:
            try:
                if self.media_player.get_state != vlc.State.Ended:
                    self.stop_media_player()
            except AttributeError:
                pass

            list_source = []
            if len(self.list_result) > 0:
                album = music.client.albums_with_tracks(self.list_result[0])
                self.json_data_track = album
                block = 0
                for i in album.volumes[0]:
                    track = music.extract_direct_link_to_track(i.id)
                    print(f"\n{track}")
                    list_source.append(track)
                    block += 1
                    if block == int(config.get("block")) or len(album.volumes[0]) == block:
                        self.play_media_list(list_source)
                        block = 0

    def select_play_2(self):
        import music
        if self.list_result != []:
            try:
                if self.media_player.get_state != vlc.State.Ended:
                    self.stop_media_player()
            except AttributeError:
                pass

            list_source = []
            if len(self.list_result) > 3:
                album = music.client.albums_with_tracks(self.list_result[3])
                self.json_data_track = album
                block = 0
                for i in album.volumes[0]:
                    track = music.extract_direct_link_to_track(i.id)
                    print(f"\n{track}")
                    list_source.append(track)
                    block += 1
                    if block == int(config.get("block")) or len(album.volumes[0]) == block:
                        self.play_media_list(list_source)
                        block = 0

    def select_play_3(self):
        import music
        if self.list_result != []:
            try:
                if self.media_player.get_state != vlc.State.Ended:
                    self.stop_media_player()
            except AttributeError:
                pass

            list_source = []
            if len(self.list_result) > 6:
                album = music.client.albums_with_tracks(self.list_result[6])
                self.json_data_track = album
                block = 0
                for i in album.volumes[0]:
                    track = music.extract_direct_link_to_track(i.id)
                    print(f"\n{track}")
                    list_source.append(track)
                    block += 1
                    if block == int(config.get("block")) or len(album.volumes[0]) == block:
                        self.play_media_list(list_source)
                        block = 0

    def select_play_4(self):
        import music
        if self.list_result != []:
            try:
                if self.media_player.get_state != vlc.State.Ended:
                    self.stop_media_player()
            except AttributeError:
                pass

            list_source = []
            if len(self.list_result) > 9:
                album = music.client.albums_with_tracks(self.list_result[9])
                self.json_data_track = album
                block = 0
                for i in album.volumes[0]:
                    track = music.extract_direct_link_to_track(i.id)
                    print(f"\n{track}")
                    list_source.append(track)
                    block += 1
                    if block == int(config.get("block")) or len(album.volumes[0]) == block:
                        self.play_media_list(list_source)
                        block = 0

    def play_my_wave_start(self):
        text, ok = QInputDialog.getText(self, 'Сколько песен?', 'Сколько песен вы хотите послушать из Моей волны?')
        threading.Thread(target=lambda:self.play_my_wave(text=text, ok=ok), daemon=True).start()
        #threading.Thread(target=lambda:self.check(), daemon=True).start()
    def like(self):
        import music
        data = self.json_data_track.volumes[0]
        music.client.users_likes_tracks_add(data[self.current_track].id)
    def play_my_wave(self, text, ok):
        import music

        if ok:
            try:
                block = 0
                text = int(text)
                list_source = []
                for i in range(0, text):
                    my_wave = music.my_wave()
                    if my_wave.get('responce') == "ok":
                        track = music.extract_direct_link_to_track(my_wave.get('id'))
                        print(f"\n{track}")
                        list_source.append(track)
                        block += 1
                        if block == config.get("block") or text != config.get("block") and block == text:
                            self.play_media_list(list_source)
                            block = 0
                    else:
                        self.error_standart("Ошибка", f"Ошибка: {my_wave.get('text')}", exit_or_no=False)
            except ValueError:
               self.error_standart("Ошибка", f"Ошибка: ValueError. Напишите цифрами, а не буквами ;)", exit_or_no=False)

    def play_media_list(self, list_source):
        while True:
            print(self.current_track)
            print(len(list_source))
            if self.current_track != len(list_source):
                self.current_track += 1
                self.current_track_changed += 1
                try:
                    self.media_player = vlc.MediaPlayer(list_source[self.current_track])
                except IndexError:
                    self.current_track = -1
                    self.current_track_changed = self.current_track
                    break
                self.media_player.play()
                while True:
                    print(self.media_player.get_state())
                    if self.media_player.get_state() == vlc.State.Ended:
                        print("Конец трека")
                        break
                    if self.media_player.get_state() == vlc.State.Stopped:
                        continue
                    if self.current_track_changed > self.current_track:
                        self.media_player.stop()
                        self.current_track_changed = self.current_track
                        break
                    if self.current_track_changed < self.current_track:
                        self.media_player.stop()
                        self.current_track -= 2
                        self.current_track_changed = self.current_track
                        break
                    else:
                        time.sleep(0.2)
            if self.current_track >= len(list_source):
                self.current_track = -1
                self.current_track_changed = self.current_track
                break

    def play_one_track(self, track_id):
        import music
        track = music.extract_direct_link_to_track(track_id)
        self.media_player = vlc.MediaPlayer(track)
        self.media_player.play()
        time.sleep(music.duration_track(track_id))

    def enter_link_to_play(self):
        url = self.write_link_to_play.text()
        if url and url.strip() or url.split(".") == "https://music" and url.split(".")[1] == "yandex":
            check_in_track = url.split("/")
            url_parts=url.split('/')
            id_ = url_parts[-1]
            list_source = []
            import music
            if check_in_track[-2] == "track":
                self.play_one_track(id_)

            if check_in_track[-2] != "track":
                response = requests.get(url)
                soup = BeautifulSoup(response.text, 'lxml')
                quotes = soup.find_all('a', class_='d-track__title deco-link deco-link_stronger')
                if not quotes:
                    self.write_link_to_play.setText("Похоже вы вставили неправильную ссылку")
                else:
                    block = 0
                    list_source = []
                    for title in quotes:
                        s = title.text.strip(), title.get('href')
                        url = "https://music.yandex.ru" + s[1]

                        url_parts=url.split('/')
                        track_id = url_parts[-1]
                        if block == int(config.get("block")):
                            self.play_media_list(list_source)
                        else:
                            track = music.extract_direct_link_to_track(track_id)
                            list_source.append(track)
                            print(f"\n{track}")
                            block += 1
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
                file.write(f'''token_yandex = "{text}"
block = 10''')
        else:
            if exit_or_no:
                self.media_player.stop()
                sys.exit()

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
                file.write(f'''token_yandex = "{text}"
block = 10''')
        else:
            if exit_or_no:
                sys.exit()

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
