# SYD_Python 工具功能说明  

使用Pycharm开发  
Python安装软件包可以使用如下命令引用国内源进行安装:  
pip install pandas -i https://pypi.douban.com/simple  //安装pandas  
pip install PyQt5-tools -i https://pypi.douban.com/simple  //安装PyQt5-tools  

## 打包命令:  
pyinstaller -F Main.spec  
如果打包失败就去掉F,生成文件在文件夹:SYD_Calculator\SYD_Python\dist  
可以运行"生成exe.bat"进行打包,会把生成的exe拷贝到cal目录下  