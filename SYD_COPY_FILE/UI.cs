using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using System.IO;

namespace SYD_COPY_FILE
{
    public partial class Form1
    {
        #region define 

        private static int UI_SCREEN_WIDTH = 454;//屏幕宽度
        private static int UI_SCREEN_HIGHT = 454;//屏幕高度

        public enum OPEN_PICTURE_ALIGN  //贴边打开图片时的方式
        {
            NON,//正常打开图片，非贴边打开
            RIGHT,//贴右边打开图片
            LEFT,
            UP,
            DOWN
        };
    #endregion
        List<int[]> clear_marray = new List<int[]>();
        int x_clear_star = UI_SCREEN_WIDTH;
        int y_clear_star = UI_SCREEN_HIGHT;
        int x_clear_stop = -1;
        int y_clear_stop = -1;
        int x_clear_stop_2 = -1;
        int x_clear_star_2 = 0;
        Bitmap myBmp;
        SYDPictureBox pictureBox_temp;
        List<SYDPictureBox> list_pictureBox = new List<SYDPictureBox>();
        Point mouseDownPoint = new Point(); //记录拖拽过程鼠标位置
        bool isMove = false;  //判断鼠标在SYDPictureBox上移动时，是否处于拖拽过程(鼠标左键是否按下)
        string filename_background = null;
        string[] filenames_old = new string[10];
        public void ui_init()
        {
            filename_background = null;
        }
        private bool picture_valid(string filename)
        {
            if(list_pictureBox.Count>0)
            {
                foreach (SYDPictureBox SYDPictureBox in list_pictureBox)
                {
                    if (SYDPictureBox.filename == filename) return false;
                }
            }
            return true;
        }
        //align 参数代表是否需要贴边打开，不需要的话传入OPEN_PICTURE_ALIGN.NON
        private void picture_addto_platfrom(string filename, int x, int y, OPEN_PICTURE_ALIGN align, SYDPictureBox picture)
        {
            if(picture_valid(filename)==false)
            {
                MessageBox.Show("不能够重复打开相同图片");
                return;
            }
            myBmp = new Bitmap(filename);
            myBmp = img_alpha(myBmp, 128);
            if (myBmp == null)
            {
                MessageBox.Show("读取失败");
                return;
            }
            if ((myBmp.Width > UI_SCREEN_WIDTH) || (myBmp.Height > UI_SCREEN_HIGHT))
            {
                MessageBox.Show("图片太大");
                return;
            }
            if (OPEN_PICTURE_ALIGN.NON != align)//有对齐
            {
                switch (align)
                {
                    case OPEN_PICTURE_ALIGN.RIGHT:
                        x = picture.Location.X + picture.Width;
                        y = picture.Location.Y;
                        break;
                    case OPEN_PICTURE_ALIGN.LEFT:
                        x = picture.Location.X - myBmp.Width;
                        y = picture.Location.Y;
                        break;
                    case OPEN_PICTURE_ALIGN.UP:
                        x = picture.Location.X;
                        y = picture.Location.Y- myBmp.Height;
                        break;
                    case OPEN_PICTURE_ALIGN.DOWN:
                        x = picture.Location.X;
                        y = picture.Location.Y + picture.Height;
                        break;
                }
                if ((x < 0) || (y < 0))
                {
                    MessageBox.Show("贴边模式剩余空间不足以放下该图片");
                    return;
                }
            }
            if ((myBmp.Width+ x)>= UI_SCREEN_WIDTH) x = UI_SCREEN_WIDTH - myBmp.Width;
            if ((myBmp.Height + y) >= UI_SCREEN_HIGHT) y = UI_SCREEN_WIDTH - myBmp.Height;
            pictureBox_temp = new SYDPictureBox();
            pictureBox_temp.BackColor = Color.Transparent;

            if (list_pictureBox.Count<SYDPictureBox>() != 0)
            {
                SYDPictureBox Parent = list_pictureBox.Last<SYDPictureBox>();
                pictureBox_temp.Parent = Parent;
            }
            pictureBox_temp.filename = filename;
            pictureBox_temp.Location = new System.Drawing.Point(x, y);
            pictureBox_temp.Margin = new System.Windows.Forms.Padding(2);
            pictureBox_temp.Name = filename.Substring(filename.LastIndexOf("\\") + 1).Replace(".jpg", "");
            pictureBox_temp.Size = new System.Drawing.Size(67, 34);
            pictureBox_temp.TabIndex = 0;
            pictureBox_temp.TabStop = false;
            pictureBox_temp.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBox_temp_MouseDown);
            pictureBox_temp.MouseMove += new System.Windows.Forms.MouseEventHandler(pictureBox_temp_MouseMove);
            pictureBox_temp.MouseUp += new System.Windows.Forms.MouseEventHandler(pictureBox_temp_MouseUp);
            pictureBox_temp.KeyDown += new System.Windows.Forms.KeyEventHandler(pictureBox_temp_KeyDown);
            pictureBox_temp.ContextMenuStrip = contextMenuStrip1;
            ((System.ComponentModel.ISupportInitialize)(pictureBox_temp)).EndInit();
            this.panel_pictureplatfrom.Controls.Clear();
            list_pictureBox.Add(pictureBox_temp);
            for (int i = list_pictureBox.Count - 1; i >= 0; i--)
            {
                this.panel_pictureplatfrom.Controls.Add(list_pictureBox.ElementAt(i));
            }
            pictureBox_temp.Focus();
            textBox_picture_path.Text = filename;
            pictureBox_temp.Image = myBmp;
            //pictureBox_temp.SizeMode = PictureBoxSizeMode.Zoom; //设置SYDPictureBox为缩放模式
            pictureBox_temp.Width = myBmp.Width;
            pictureBox_temp.Height = myBmp.Height;
        }


        //改变图片透明度
        public static Bitmap img_alpha(Bitmap src, int alpha)
        {
            Bitmap bmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int h = 0; h < src.Height; h++)
                for (int w = 0; w < src.Width; w++)
                {
                    Color c = src.GetPixel(w, h);
                    bmp.SetPixel(w, h, Color.FromArgb(alpha, c.R, c.G, c.B));//色彩度最大为255，最小为0
                }
            return bmp;
        }
        private void openpicture_do(OPEN_PICTURE_ALIGN align, SYDPictureBox picture)
        {
            string filename = "";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Bmp文件|*.bmp|jpeg文件|*.jpg";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                filename = dlg.FileName;
            }
            if (filename == "")
            {
                return;
            }
            picture_addto_platfrom(filename, 20, 20, align, picture);
        }
        //图片上传
        private void button_openpicture_Click(object sender, EventArgs e)
        {
            openpicture_do(OPEN_PICTURE_ALIGN.NON,null);
        }
        private void panel_pictureplatfrom_DragDrop(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;
            if (dataObject == null) return;
            string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);

            foreach (string file in files)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(file);
                if (fi.FullName != "")
                {
                   // Point point = panel_pictureplatfrom.Location;
                    Point mousePos = panel_pictureplatfrom.PointToClient(MousePosition);
                    picture_addto_platfrom(fi.FullName, mousePos.X,  mousePos.Y,OPEN_PICTURE_ALIGN.NON, null);
                }
            }
        }

        private void panel_pictureplatfrom_DragEnter(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;
            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        //鼠标按下功能
        private void pictureBox_temp_MouseDown(object sender, MouseEventArgs e)
        {
            SYDPictureBox senderLabel = (SYDPictureBox)sender;//根据sender引用控件。
            if (e.Button == MouseButtons.Left)//左键点击
            {
                mouseDownPoint.X = Cursor.Position.X;
                mouseDownPoint.Y = Cursor.Position.Y;
                isMove = true;
                senderLabel.Focus();
                label_ui_picx.Text = senderLabel.Location.X.ToString();
                label_ui_picy.Text = senderLabel.Location.Y.ToString();
                label_ui_picw.Text = senderLabel.Width.ToString();
                label_ui_pich.Text = senderLabel.Height.ToString();
            }
        }
        //鼠标松开功能
        private void pictureBox_temp_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMove = false;
            }
        }
        //鼠标移动功能
        private void pictureBox_temp_MouseMove(object sender, MouseEventArgs e)
        {
            SYDPictureBox senderLabel = (SYDPictureBox)sender;//根据sender引用控件。
            senderLabel.Focus();
            if (isMove)
            {
                int x, y;
                int moveX, moveY;
                moveX = Cursor.Position.X - mouseDownPoint.X;
                moveY = Cursor.Position.Y - mouseDownPoint.Y;
                if (moveX < 3 && moveY < 3) return;//防止误操作移动
                x = senderLabel.Location.X + moveX;
                y = senderLabel.Location.Y + moveY;
                if (y>(panel_pictureplatfrom.Height-senderLabel.Height))
                {
                    y = panel_pictureplatfrom.Height - senderLabel.Height;
                }
                if (x > (panel_pictureplatfrom.Width - senderLabel.Width))
                {
                    x = panel_pictureplatfrom.Width - senderLabel.Width;
                }

                if (y < 0)
                {
                    y = 0;
                }
                if (x < 0)
                {
                    x =0;
                }
                senderLabel.Location = new Point(x, y);
                mouseDownPoint.X = Cursor.Position.X;
                mouseDownPoint.Y = Cursor.Position.Y;
            }
        }
        private void pictureBox_temp_KeyDown(object sender, KeyEventArgs e)
        {
            SYDPictureBox senderLabel = (SYDPictureBox)sender;//根据sender引用控件。
            int offect = 1;
            if (e.Modifiers.CompareTo(Keys.Shift) == 0)
            {
                offect = 2;
            }

            if (e.Modifiers.CompareTo(Keys.Control) == 0)
            {
                offect = 4;
            }

            if (e.KeyCode == Keys.Down)
            {
                if (senderLabel.Location.Y < UI_SCREEN_HIGHT - senderLabel.Height)
                    senderLabel.Location = new Point(senderLabel.Location.X, senderLabel.Location.Y + offect);
            }
            if (e.KeyCode == Keys.Left)
            {
                if (senderLabel.Location.X > 0)
                    senderLabel.Location = new Point(senderLabel.Location.X - offect, senderLabel.Location.Y);
            }
            if (e.KeyCode == Keys.Right)
            {
                if (senderLabel.Location.X < UI_SCREEN_WIDTH - senderLabel.Width)
                    senderLabel.Location = new Point(senderLabel.Location.X + offect, senderLabel.Location.Y);
            }
            if (e.KeyCode == Keys.Up)
            {
                if (senderLabel.Location.Y > 0)
                    senderLabel.Location = new Point(senderLabel.Location.X, senderLabel.Location.Y - offect);
            }
        }

        private void picture_background_platfrom(string filename)
        {
            if (filename != null)
            {
                myBmp = new Bitmap(filename);
                panel_pictureplatfrom.BackgroundImage = myBmp;
                panel_pictureplatfrom.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                panel_pictureplatfrom.BackgroundImage = null;
            }
        }
        private void button_picture_background_Click(object sender, EventArgs e)
        {
            string filename = "";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Bmp文件|*.bmp|jpeg文件|*.jpg";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                filename = dlg.FileName;
            }
            if (filename == "")
            {
                return;
            }
            picture_background_platfrom(filename);
            filename_background = filename;
        }
        private void button_save_interface_Click(object sender, EventArgs e)
        {
            string filename = "";
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "interface";

            dlg.DefaultExt = ".xml";

            dlg.Filter = "XML文件|*.xml";

            if (dlg.ShowDialog() == false)
                return;
            filename = dlg.FileName;

            XmlDocument xmldoc = new XmlDocument();
            //加入XML的声明段落,<?xml version="1.0" encoding="gb2312"?>
            XmlDeclaration xmldecl;
            xmldecl = xmldoc.CreateXmlDeclaration("1.0", "gb2312", null);
            xmldoc.AppendChild(xmldecl);

            //加入一个根元素
            XmlElement xmlelem = xmldoc.CreateElement("", "Interface", "");
            xmldoc.AppendChild(xmlelem);
            XmlNode root = xmldoc.SelectSingleNode("Interface");//查找<Employees>
            foreach (SYDPictureBox SYDPictureBox in list_pictureBox)
            {
                Point point = SYDPictureBox.Location;
                
                XmlElement xe1 = xmldoc.CreateElement("PictureBox");//创建一个<Node>节点
                xe1.SetAttribute("name", SYDPictureBox.Name);//设置该节点genre属性
                XmlElement xesub1 = xmldoc.CreateElement("filename");
                xesub1.InnerText = SYDPictureBox.filename;//设置文本节
                xe1.AppendChild(xesub1);//添加到<Node>节点中

                XmlElement xesub2 = xmldoc.CreateElement("point");
                xesub2.SetAttribute("x", point.X.ToString());//设置该节点genre属性
                xesub2.SetAttribute("y", point.Y.ToString());//设置该节点ISBN属性
                xe1.AppendChild(xesub2);
                root.AppendChild(xe1);//添加到<Employees>节点中
            }
            XmlElement xe2 = xmldoc.CreateElement("background");//创建一个<Node>节点
            XmlElement xesub3 = xmldoc.CreateElement("filename");
            xesub3.InnerText = filename_background;//设置文本节
            xe2.AppendChild(xesub3);//添加到<Node>节点中
            root.AppendChild(xe2);//添加到<Employees>节点中

            xmldoc.Save(filename);
        }
        private void button_open_interface_Click(object sender, EventArgs e)
        {
            string filename = "";
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "interface";

            dlg.DefaultExt = ".xml";

            dlg.Filter = "XML文件|*.xml";

            if (dlg.ShowDialog() == false)
                return;
            filename = dlg.FileName;

            XmlDocument xmldoc = new System.Xml.XmlDocument();
            xmldoc.Load(@filename);
            XmlNodeList nodeList = xmldoc.SelectSingleNode("Interface").ChildNodes;//获取Employees节点的所有子节点
            foreach (XmlNode xn in nodeList)//遍历所有子节点
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                XmlNodeList xnl0 = xe.ChildNodes;
                filename = xnl0.Item(0).InnerText;
                if (xe.Name == "PictureBox")
                {
                    string name = xe.GetAttribute("name");
                    XmlElement point = (XmlElement)xnl0.Item(1);
                    int x = Convert.ToInt32(point.GetAttribute("x"), 10);
                    int y = Convert.ToInt32(point.GetAttribute("y"), 10);
                    picture_addto_platfrom(filename, x, y, OPEN_PICTURE_ALIGN.NON, null);
                }
                else if (xe.Name == "background")
                {
                    picture_background_platfrom(filename);
                    filename_background = filename;
                }
            }
        }
        private void ccheckBox_backgrounddisplay_CheckedChanged(object sender, EventArgs e)
        {
            if (filename_background != null)
            {
                if (panel_pictureplatfrom.BackgroundImage == null)
                {
                    picture_background_platfrom(filename_background);
                }
                else
                {
                    picture_background_platfrom(null);
                }
            }
        }
        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            SYDPictureBox senderLabel = (SYDPictureBox)(contextMenuStrip1.SourceControl);//根据sender引用控件。
            for (int i = 0; i < contextMenuStrip1.Items.Count; i++)
            {
                if (contextMenuStrip1.Items[i].Selected)
                {
                    if (contextMenuStrip1.Items[i].Text.Trim() == "设置坐标")
                    {
                        string strx = string.Empty, stry = string.Empty;
                        if (InputDialog.Show(senderLabel.Location.X.ToString(), senderLabel.Location.Y.ToString(), out strx, out stry) == DialogResult.Cancel)
                        {
                            return;
                        }
                        if ((strx.Length == 0) || (stry.Length == 0))
                        {
                            MessageBox.Show("错误：输入为空");
                            return;
                        }
                        int x = Convert.ToInt32(strx, 10);
                        int y = Convert.ToInt32(stry, 10);
                        if((y < 0) || (x<0) || ((x + senderLabel.Width) > UI_SCREEN_WIDTH) || ((y + senderLabel.Height) > UI_SCREEN_HIGHT))
                        {
                            MessageBox.Show("错误：x或y错误");
                            return;
                        }
                        label_ui_picx.Text = strx;
                        label_ui_picy.Text = stry;
                        
                        senderLabel.Location = new Point(x, y);
                    }
                    else if (contextMenuStrip1.Items[i].Text.Trim() == "删除")
                    {
                        this.panel_pictureplatfrom.Controls.Remove(senderLabel);
                        list_pictureBox.Remove(senderLabel);
                    }
                    else if (contextMenuStrip1.Items[i].Text.Trim() == "贴边缘载入图片")
                    {

                    }
                }
            }
        }
        private void open_picture_DoWork(OPEN_PICTURE_ALIGN align, SYDPictureBox senderLabel)
        {
            this.Invoke(new EventHandler(delegate
            {
                openpicture_do(align, senderLabel);

            }));
        }
        private void toolStripMenuItem3_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            SYDPictureBox senderLabel = (SYDPictureBox)(contextMenuStrip1.SourceControl);//根据sender引用控件。
            for (int i = 0; i < toolStripMenuItem3.DropDownItems.Count; i++)
            {
                if (toolStripMenuItem3.DropDownItems[i].Selected)
                {
                    OPEN_PICTURE_ALIGN align= OPEN_PICTURE_ALIGN.NON;
                    if (toolStripMenuItem3.DropDownItems[i].Text.Trim() == "贴右边")
                    {
                        align = OPEN_PICTURE_ALIGN.RIGHT;
                    }
                    else if (toolStripMenuItem3.DropDownItems[i].Text.Trim() == "贴左边")
                    {
                        align = OPEN_PICTURE_ALIGN.LEFT;
                    }
                    else if (toolStripMenuItem3.DropDownItems[i].Text.Trim() == "贴上边")
                    {
                        align = OPEN_PICTURE_ALIGN.UP;
                    }
                    else if (toolStripMenuItem3.DropDownItems[i].Text.Trim() == "贴下边")
                    {
                        align = OPEN_PICTURE_ALIGN.DOWN;
                    }
                    BackgroundWorker work = new BackgroundWorker();

                    work.DoWork += (o, ea) =>
                    {
                        open_picture_DoWork(align, senderLabel); // 可以使用泛型
                    };

                    work.RunWorkerAsync();
                    
                }
            }
        }
        //返回成功要符合如下条件
        //1.有且仅有唯一一组从0到9的数据
        //2.传入图片必须为数字图片(文件名最后一位范围是0-9)
        private bool picture_num_define_isvalid(string filename)
        {
            int i = 0,j=0;
            if (filenames_old[0]!=null)//之前有存过
            {
                for (i = 0; i < filenames_old.Length; i++)
                { 
                    if(filenames_old[i]== filename) return true;
                }
            }
            char[] num = new char[10];
            string filename_withoutext= Path.GetFileNameWithoutExtension(filename);
            if ((filename_withoutext[filename_withoutext.Length - 1] < '0') || (filename_withoutext[filename_withoutext.Length - 1] > '9'))//最后一位是数字
            {
                return false;
            }
            string dir = Path.GetDirectoryName(filename);
            string ext = Path.GetExtension(filename);
            string[] filenames = Directory.GetFiles(dir, "*" + ext, SearchOption.TopDirectoryOnly);//获取目录文件名称集合
            if (filenames.Length < 10)
            {
                return false;
            }
            else 
            {
                for (i = 0; i < filenames.Length; i++)
                {
                    filename_withoutext = Path.GetFileNameWithoutExtension(filenames[i]);
                    if ((filename_withoutext[filename_withoutext.Length - 1] >= '0') && (filename_withoutext[filename_withoutext.Length - 1] <= '9'))//最后一位是数字
                    {
                        if (j >= 10) return false;
                        num[j] = (char)(filename_withoutext[filename_withoutext.Length - 1]- '0');
                        filenames_old[j] = filenames[i];
                        j++;
                    }
                }
                if (j != 10) return false;
                j = 0;
                for (i = 0; i < num.Length; i++)
                {
                    j = j + num[i];
                }
                if (j == 45)
                {
                    return true;
                }
            }
            return false;
        }
        private void picture_define_do(object sender,int select_index, SYDPictureBox picture)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (item==toolStripMenuItem9)
            {
                if (picture_num_define_isvalid(picture.filename))
                {
                    MessageBox.Show("定义为步数有效 " + item.DropDownItems[select_index].Text.Trim());
                }
                else
                {
                    MessageBox.Show("定义为步数无效 " + item.DropDownItems[select_index].Text.Trim());
                }
            }
            else if (item == toolStripMenuItem25)
            {
                MessageBox.Show("定义为卡路里 " + item.DropDownItems[select_index].Text.Trim());
            }
            else if (item == toolStripMenuItem26)
            {
                MessageBox.Show("定义为距离 " + item.DropDownItems[select_index].Text.Trim());
            }
            else if (item == toolStripMenuItem24)
            {
                MessageBox.Show("定义为心率 " + item.DropDownItems[select_index].Text.Trim());
            }
            else if (item == toolStripMenuItem27)
            {
                MessageBox.Show("定义为血氧 " + item.DropDownItems[select_index].Text.Trim());
            }
            else if (item == toolStripMenuItem28)
            {
                MessageBox.Show("定义为血压 " + item.DropDownItems[select_index].Text.Trim());
            }
            else if (item == toolStripMenuItem29)
            {
                MessageBox.Show("定义为电量 " + item.DropDownItems[select_index].Text.Trim());
            }
        }
        private void picture_define_DoWork(object sender,int select_index, SYDPictureBox senderLabel)
        {
            this.Invoke(new EventHandler(delegate
            {
                picture_define_do(sender,select_index, senderLabel);
            }));
        }
        private void toolStripMenuItem_PictureDefine_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            SYDPictureBox senderLabel = (SYDPictureBox)(contextMenuStrip1.SourceControl);//根据sender引用控件。
            for (int i = 0; i < item.DropDownItems.Count; i++)
            {
                if (item.DropDownItems[i].Selected)
                {
                    BackgroundWorker work = new BackgroundWorker();

                    work.DoWork += (o, ea) =>
                    {
                        picture_define_DoWork(sender,i, senderLabel); // 可以使用泛型
                    };

                    work.RunWorkerAsync();
                    break;
                }
            }
        }
        private void panel_pictureplatfrom_top_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = panel_pictureplatfrom.PointToClient(Control.MousePosition);
            label_ui_topx.Text = point.X.ToString();
            label_ui_topy.Text = point.Y.ToString();
        }
        private void Generate_interface_display_oled()
        {
            bool alread = true;
            richtextBox_picture_result.Text = null;
            string stext = "";
            clear_marray.Clear();
            foreach (SYDPictureBox SYDPictureBox in list_pictureBox)
            {
                Point point = SYDPictureBox.Location;
                stext = "draw(" + point.X + "," + point.Y + "," + (point.X + SYDPictureBox.Width) + "," + (point.Y + SYDPictureBox.Height) + "," + SYDPictureBox.Name + ")" + ";\n";
                richtextBox_picture_result.Text += stext;
            }
            x_clear_stop_2 = UI_SCREEN_WIDTH - 1;
            x_clear_star_2 = 0;
            for (int i = 0; i < UI_SCREEN_HIGHT; i++)
            {
                for (int j = x_clear_star_2; j < x_clear_stop_2 + 1; j++)
                {
                    alread = true;
                    foreach (SYDPictureBox SYDPictureBox in list_pictureBox)
                    {
                        Point point = SYDPictureBox.Location;
                        if (((j >= (int)point.X) && (j < (int)(point.X + SYDPictureBox.Width))) && ((i >= (int)point.Y) && (i < (int)(point.Y + SYDPictureBox.Height))))
                        {
                            alread = false;
                        }
                    }

                    foreach (int[] list in clear_marray)
                    {
                        if (((i >= list[1]) && (i < list[3])) && ((j >= list[0]) && (j < list[2])))
                        {
                            alread = false;
                        }
                    }
                    if (alread)
                    {
                        if (i < y_clear_star)
                        {
                            y_clear_star = i;
                        }
                        if (j < x_clear_star)
                        {
                            x_clear_star = j;
                            x_clear_star_2 = j;
                        }
                        if (i == y_clear_star)
                        {
                            if (j > x_clear_stop)
                            {
                                x_clear_stop = j;
                            }
                        }
                        else
                        {
                            x_clear_stop_2 = x_clear_stop;
                        }
                        if (j == x_clear_star)
                        {
                            if (i > y_clear_stop)
                            {
                                y_clear_stop = i;
                            }
                        }
                    }
                    else
                    {
                        if (x_clear_stop != -1) x_clear_stop_2 = x_clear_stop;
                        if ((x_clear_star < UI_SCREEN_WIDTH) && (y_clear_star < UI_SCREEN_WIDTH))
                        {

                            if (j < x_clear_stop)
                            {
                                if (j > x_clear_star)
                                {
                                    y_clear_stop--;
                                }
                                clear_marray.Add(new int[] { x_clear_star, y_clear_star, x_clear_stop + 1, y_clear_stop + 1 });
                                x_clear_star = UI_SCREEN_WIDTH;
                                y_clear_star = UI_SCREEN_HIGHT;
                                x_clear_stop = -1;
                                y_clear_stop = -1;
                                x_clear_star_2 = 0;
                                x_clear_stop_2 = UI_SCREEN_WIDTH - 1;
                                i = 0; j = 0;
                            }
                            else
                            {

                            }
                        }
                    }
                    if ((x_clear_star < UI_SCREEN_WIDTH) && (y_clear_star < UI_SCREEN_HIGHT))
                    {
                        if ((i == (UI_SCREEN_WIDTH - 1)) && (j == x_clear_stop_2))
                        {
                            clear_marray.Add(new int[] { x_clear_star, y_clear_star, x_clear_stop + 1, y_clear_stop + 1 });
                            x_clear_star = UI_SCREEN_WIDTH;
                            y_clear_star = UI_SCREEN_HIGHT;
                            x_clear_stop = -1;
                            y_clear_stop = -1;
                            //if (j == (UI_SCREEN_WIDTH - 1))
                            {
                                i = 0; j = 0;
                                x_clear_star_2 = 0;
                                x_clear_stop_2 = (UI_SCREEN_WIDTH - 1);
                            }
                        }
                    }
                }
            }
            foreach (int[] list in clear_marray)
            {

                stext = "oled_clear(" + list[0] + "," + list[1] + "," + list[2] + "," + list[3] + "," + "Color_Black)" + ";\n";
                richtextBox_picture_result.Text += stext;
            }
        }
        private void Generate_datafile_button_Click(object sender, EventArgs e)
        {
            bool is_contain_background = false;
            DialogResult key =MessageBox.Show("如果生成数据文件中要包含背景图选择是，不包含背景图选择后，其他选择退出本操作"," 是否包含背景图？",MessageBoxButtons.YesNoCancel);
            if (key == DialogResult.Yes)
            {
                is_contain_background = true;
            }
            else if (key == DialogResult.No)
            {
                is_contain_background = false;
            }
            else return;
            UInt32 j=0;
            int i = 0;
            UInt32 srcfilesize = 0;
            List<UInt32> picture_addr = new List<UInt32>();
            List<UInt16> picture_w = new List<UInt16>(), picture_h = new List<UInt16>(), picture_x = new List<UInt16>(), picture_y = new List<UInt16>();
            string path="";
            byte[] text;
            byte[] bin=new byte[64000000];
            byte[] buff = new byte[50*12+8];
            if ((filename_background != null) && is_contain_background==true)
            {
                path = filename_background;
                path = path.Replace(".bmp", string.Empty).Replace(".BMP", string.Empty);
                path = path + ".dta";
                if (File.Exists(path))
                {
                    text = System.IO.File.ReadAllBytes(path);
                    picture_w.Add((UInt16)(((UInt16)text[5] << 8) | (UInt16)text[4]));
                    picture_h.Add((UInt16)(((UInt16)text[7] << 8) | (UInt16)text[6]));
                    picture_x.Add(0);
                    picture_y.Add(0);
                    picture_addr.Add(srcfilesize);
                    srcfilesize += (UInt32)text.Length - 16;
                    for (i = 0; i < (text.Length - 16) / 2; i++)
                    {
                        bin[j++] = text[16 + i * 2 + 1];
                        bin[j++] = text[16 + i * 2];
                    }
                }
                else
                {
                    MessageBox.Show("错误：背景图片目录下没有同名的dta数据文件 " + path);
                    return;
                }
            }
            foreach (SYDPictureBox SYDPictureBox in list_pictureBox)
            {
                path = SYDPictureBox.filename;
                path = path.Replace(".bmp", string.Empty).Replace(".BMP", string.Empty);
                path = path + ".dta";
                if (File.Exists(path))
                {
                    text = System.IO.File.ReadAllBytes(path);
                    picture_w.Add((UInt16)(((UInt16)text[5] << 8) | (UInt16)text[4]));
                    picture_h.Add((UInt16)(((UInt16)text[7] << 8) | (UInt16)text[6]));
                    picture_x.Add((UInt16)SYDPictureBox.Location.X);
                    picture_y.Add((UInt16)SYDPictureBox.Location.Y);
                    picture_addr.Add(srcfilesize);
                    srcfilesize += (UInt32)text.Length - 16;
                    for (i = 0; i < ((UInt32)text.Length - 16) / 2; i++)
                    {
                        bin[j++] = text[16 + i * 2 + 1];
                        bin[j++] = text[16 + i * 2];
                    }
                }
                else
                {
                    MessageBox.Show("错误：目录下没有同名的dta数据文件 " + path);
                    return;
                }
            }
            Generate_interface_display_oled();//先生成界面布局语句

            buff[0] = 0xa5;//uint32_t reset_state;//0xa5a5a5a5 本结构体数据有效
            buff[1] = 0xa5;
            buff[2] = 0xa5;
            buff[3] = 0xa5;
            buff[4] = (byte)picture_w.Count;//uint8_t enum;//元素个数 一般指本界面有多少张图片组成
            if((filename_background != null) && is_contain_background == true)
                buff[5] = 0;//uint8_t explain;//特殊说明 00：无特殊说明 BIT0:本界面无背景图片
            else
                buff[5] = 0X01;
            UInt16 offset = (UInt16)(picture_w.Count * 12 + 8);//也代表了有效数据的起始位置
            buff[6] = (byte)offset;//uint16_t offset;//图片数据的整体偏移
            buff[7] = (byte)(offset >> 8);
            for (i = 0; i < picture_w.Count; i++)
            {
                buff[8+i*12]= (Byte)(picture_addr[i] & 0x000000FF);//uint32_t addr;//在数据区的起始位置
                buff[8 + i * 12+1] = (Byte)((picture_addr[i] >> 8) & 0x000000FF);
                buff[8 + i * 12 + 2] = (Byte)((picture_addr[i] >> 16) & 0x000000FF);
                buff[8 + i * 12 + 3] = (Byte)((picture_addr[i] >> 24) & 0x000000FF);

                buff[8 + i * 12 + 4] = (Byte)(picture_x[i] & 0x000000FF);//uint16_t x;//图片的X起点
                buff[8 + i * 12 + 5] = (Byte)((picture_x[i] >> 8) & 0x000000FF);

                buff[8 + i * 12 + 6] = (Byte)(picture_y[i] & 0x000000FF);//uint16_t y;//图片的Y起点
                buff[8 + i * 12 + 7] = (Byte)((picture_y[i] >> 8) & 0x000000FF);

                buff[8 + i * 12 + 8] = (Byte)(picture_w[i] & 0x000000FF);//uint16_t w;//图片的宽度
                buff[8 + i * 12 + 9] = (Byte)((picture_w[i] >> 8) & 0x000000FF);

                buff[8 + i * 12 + 10] = (Byte)(picture_h[i] & 0x000000FF);//uint16_t h;//图片高度
                buff[8 + i * 12 + 11] = (Byte)((picture_h[i] >> 8) & 0x000000FF);
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.FileName = "interface_info_file";

            dlg.DefaultExt = ".bin";

            dlg.Filter = "bin file (.bin)|*.bin";

            if (dlg.ShowDialog() == false)
                return;

            FileStream fs;
            BinaryWriter bw;

            if (System.IO.File.Exists(dlg.FileName))
            {
                System.IO.File.Delete(dlg.FileName);
            }
            fs = new FileStream(dlg.FileName, FileMode.CreateNew);
            bw = new BinaryWriter(fs);
            bw.Write(buff, 0, offset);
            bw.Write(bin, 0, (int)srcfilesize);
            bw.Close();
            fs.Close();
        }
    }

    class SYDPictureBox : PictureBox
    {
        public string filename;
    }
    public static class InputDialog
    {
        public static DialogResult Show(string x, string y, out string strText, out string strText1)
        {
            string strTemp = string.Empty, strTemp1 = string.Empty;

            FrmInputDialog inputDialog = new FrmInputDialog(x,y);
            inputDialog.TextHandler = (str, str1) => { strTemp = str; strTemp1 = str1; };

            DialogResult result = inputDialog.ShowDialog();
            strText = strTemp;
            strText1 = strTemp1;

            return result;
        }
    }
}