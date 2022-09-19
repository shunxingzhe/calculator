from PyQt5.QtWidgets import QLineEdit


class MyQLineEdit(QLineEdit):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.setAcceptDrops(True)

    def dragEnterEvent(self, file):
        file.accept()

    def dropEvent(self, file):
        filePath = file.mimeData().text().replace('file:///', '')
        self.setText(filePath)
