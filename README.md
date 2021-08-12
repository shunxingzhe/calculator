# SYD_Calculator 工具功能说明  
主要功能如下：  
Copy功能设计之初用于把一个文件从一个目录拷贝到另外一个目录，比如要修改测试SYD8811的Lib文件，这里就要把LIB工程下的输出文件拷贝到应用工程的目录下给应用程序使用，所以诞生了Copy的功能，另外有可能存在多级拷贝和换名字的情况，最终逐步演变成目前的界面。  
![image](https://gitee.com/SydtekInc/syd_-calculator/image/copy.png)  

Sync copy功能是在Copy完善之后产生的，有时候要同步拷贝文件，比如SYD8811有协议栈和软件定时器两个LIB，在某个时候需要同时修改两个LIB，这时候就相当于同步拷贝了，注意：这里的同步拷贝只是相互触发而已，具体拷贝执行的文件和目录都是独立的。  
![image](https://gitee.com/SydtekInc/syd_-calculator/image/Sync_copy.png)  

Rename功能是给发布文件准备的，他会根据界面的设置拷贝出一个新的文件，从而无需每次都比较繁琐的修改文件名发布文件。  
![image](https://gitee.com/SydtekInc/syd_-calculator/image/Rename.png)  

BIN_Split功能是切割bin文件使用，对于某些数据的分析非常有用。  
![image](https://gitee.com/SydtekInc/syd_-calculator/image/BIN_Split.png)  

BIN_Combin功能与BIN_Split是反功能，他是合并bin文件。  
![image](https://gitee.com/SydtekInc/syd_-calculator/image/BIN_Combin.png)  

timer_cal功能最开始是用于计算时间戳差值的，后来逐步演变成非常多种形式的计算功能，有十进制的也有十六进制的，非常方便，这项功能其实也是本软件设计之初的最原始作用。  
![image](https://gitee.com/SydtekInc/syd_-calculator/image/timer_cal.png)  

LED_RGB_BLE功能用于数码管和RGB颜色已经蓝牙的一些简易运算，对于我非常有用。  
![image](https://gitee.com/SydtekInc/syd_-calculator/image/LED_RGB_BLE.png)  

arr界面功能非常多而且复杂，这里设计当初主要是各种各样文本的处理，后来逐步演变成文件还有查找功能的处理，主要是通过切换模式来实现各种功能。  
![image](https://gitee.com/SydtekInc/syd_-calculator/image/arr.png)  

Picsplit功能最开始是因为WORD文档插入过于长的图片时会自动缩放到一页去，根本看不清，所以就想着弄一个切割图片的功能，谁知道发展到现在该界面功能非常复杂，因为本人主要从事手环UI等工作，所以这个界面是UI布局非常有用的辅助工具。  
![image](https://gitee.com/SydtekInc/syd_-calculator/image/Picsplit.png)  

ui功能最开始是想实现一个自动化布局UI界面的工具，然后一键导出C代码直接给芯片使用，最后发现并没有多大用于，或许目前最大的用处是可以导入一张背景，然后移动图片得到坐标吧。  
![image](https://gitee.com/SydtekInc/syd_-calculator/image/ui.png)  