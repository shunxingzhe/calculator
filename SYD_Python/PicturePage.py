import os
import time
import shutil

from PyQt5.QtCore import QSettings
from PyQt5.QtWidgets import QMainWindow, QFileDialog, QMessageBox
from tools_ui import Ui_MainWindow
from PIL import Image

settings = QSettings("config.ini", QSettings.IniFormat)


class PictureProcess(QMainWindow, Ui_MainWindow):
    def __init__(self, parent=None):
        super(PictureProcess, self).__init__(parent)

        self.lastSrcFilePath_0 = ''
        self.lastTargetFilePath_0 = ''
        pass

    def btn0FileOpenClicked(self):
        if self.lineEditSrcFilePath_0.text() == '':
            indir= QFileDialog.getExistingDirectory(None, "选取源文件夹", "C:/")
        else:
            indir= QFileDialog.getExistingDirectory(None, "选取源文件夹", self.lineEditSrcFilePath_0.text())

        if indir:
            self.lineEditSrcFilePath_0.setText(indir)

    def btn0FileCopyClicked(self):
        if self.lineEditTargetFilePath_0.text() == '':
            outdir= QFileDialog.getExistingDirectory(None, "选取目标文件夹", "C:/")
        else:
            outdir= QFileDialog.getExistingDirectory(None, "选取目标文件夹", self.lineEditTargetFilePath_0.text())

        if outdir:
            self.lineEditTargetFilePath_0.setText(outdir )

    def btnConvertPictureClicked(self):

        def picture_filter(f):
            if f[-4:] in ['.jpg', '.png', '.bmp']:
                return True
            else:
                return False

        print("开始图片转换")
        path = self.lineEditSrcFilePath_0.text()
        outpath = self.lineEditTargetFilePath_0.text()
        print("图片目录:"+path)
        print("目标目录:" + outpath)
        # files = os.listdir("F:\\download\\pic")\
        files = os.listdir(path)
        files = list(filter(picture_filter, files))
        #print(files)
        print(self.comboBox_picture_rotate.currentIndex())
        for i in files:
            file = os.path.join(path, i)
            print("file:" + file)
            img = Image.open(file).convert('RGB')
            #img = img.rotate(-90, expand=1)#顺时针90度
            if self.comboBox_picture_rotate.currentIndex() == 1:
                img = img.rotate(-90, expand=1)  # 顺时针90度
            elif self.comboBox_picture_rotate.currentIndex() == 2:
                img = img.rotate(-180, expand=1)  # 顺时针180度
            elif self.comboBox_picture_rotate.currentIndex() == 3:
                img = img.rotate(-270, expand=1)  # 顺时针180度
            print("outpath:" + outpath)
            file_name, file_extend = os.path.splitext(i)
            dst = os.path.join(os.path.abspath(outpath), file_name + '.bmp')
            img.save(dst)


    def loadFilePageConfig(self):
        self.lastSrcFilePath_0 = settings.value("FilePage_srcFilePath_0")
        self.lineEditSrcFilePath_0.setText(self.lastSrcFilePath_0)
        self.lastTargetFilePath_0 = settings.value("FilePage_targetFilePath_0")
        self.lineEditTargetFilePath_0.setText(self.lastTargetFilePath_0)

    def saveFilePageConfig(self):
        settings.setValue("FilePage_srcFilePath_0", self.lineEditSrcFilePath_0.text())
        settings.setValue("FilePage_targetFilePath_0", self.lineEditTargetFilePath_0.text())
