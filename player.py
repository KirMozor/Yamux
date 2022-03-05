#!/usr/bin/env python3
import mpv
import sys

from PyQt5.QtWidgets import *
from PyQt5.QtCore import *

class Test(QMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.container = QWidget(self)
        self.setCentralWidget(self.container)
        self.container.setAttribute(Qt.WA_DontCreateNativeAncestors)
        self.container.setAttribute(Qt.WA_NativeWindow)
        player = mpv.MPV(wid=str(int(self.container.winId())),
                log_handler=print,
                loglevel='debug')
        player.play('test.mp3')

app = QApplication(sys.argv)

# This is necessary since PyQT stomps over the locale settings needed by libmpv.
# This needs to happen after importing PyQT before creating the first mpv.MPV instance.
import locale
locale.setlocale(locale.LC_NUMERIC, 'C')
win = Test()
win.show()
sys.exit(app.exec_())