import enum
import os
import sys

if hasattr(sys, 'frozen'):
    os.environ['PATH'] = sys._MEIPASS + ";" + os.environ['PATH']

from PyQt5.QtCore import QSettings
from PyQt5.QtGui import QIcon
from PyQt5.QtWidgets import QApplication, QButtonGroup

from PicturePage import PictureProcess

toolsName = 'SYD Python V1.0.0'

settings = QSettings("config.ini", QSettings.IniFormat)


class Page(enum.Enum):
    CalculatePage = 0
    MergePage = 1
    HIDPage = 2
    ConvertPage = 3
    FilePage = 4
    UiPage = 5


class Tools(PictureProcess):
    def __init__(self, parent=None):
        super(Tools, self).__init__(parent)

        self.setupUi(self)

        self.setWindowTitle(toolsName)
        icon = os.path.dirname(os.path.abspath(__file__)).replace('\\', '\\\\') + "\\\\icon.ico"
        print(icon)
        self.setWindowIcon(QIcon(icon))  # 设置标题栏、任务栏等的图标

        index = settings.value("ToolsCurrentIndex")

        if index:
            self.tabWidgetTools.setCurrentIndex(int(index))
            self.tabWidgetToolsCurrentChanged(int(index))
        else:
            self.tabWidgetTools.setCurrentIndex(0)
            self.tabWidgetToolsCurrentChanged(0)

        self.tabWidgetTools.currentChanged.connect(self.tabWidgetToolsCurrentChanged)
        # Page5 PicturePage
        self.loadFilePageConfig()
        self.pushButtonFileOpen.clicked.connect(self.btn0FileOpenClicked)
        self.pushButtonFolderOpen.clicked.connect(self.btn0FileCopyClicked)
        self.pushButtonConvertPicture.clicked.connect(self.btnConvertPictureClicked)

    def tabWidgetToolsCurrentChanged(self, index):
        if index == Page.FilePage.value:
            width = 1000
            height = 350
        else:
            return
        self.tabWidgetTools.resize(width, height)
        self.resize(width, height)
        self.setFixedSize(width, height)

    def closeEvent(self, *args, **kwargs):
        settings.setValue("ToolsCurrentIndex", self.tabWidgetTools.currentIndex())
        self.saveFilePageConfig()


if __name__ == '__main__':
    app = QApplication(sys.argv)
    win = Tools()
    win.show()
    sys.exit(app.exec_())
