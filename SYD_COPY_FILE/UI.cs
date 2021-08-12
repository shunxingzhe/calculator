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
namespace SYD_COPY_FILE
{
    public partial class Form1
    {
        bool while_ture = true;
        List<int[]> clear_marray = new List<int[]>();
        int x_clear_star = 240;
        int y_clear_star = 240;
        int x_clear_stop = -1;
        int y_clear_stop = -1;
        int x_clear_stop_2 = -1;
        int x_clear_star_2 = 0;
        Bitmap myBmp;
        SYDPictureBox pictureBox_temp;
        List<SYDPictureBox> list_pictureBox = new List<SYDPictureBox>();
        Dictionary<string, string> dictionary_string = new Dictionary<string, string>();
        Point mouseDownPoint = new Point(); //记录拖拽过程鼠标位置
        bool isMove = false;  //判断鼠标在SYDPictureBox上移动时，是否处于拖拽过程(鼠标左键是否按下)
        int zoomStep = 20;   //缩放步长
        string filename_background = null;

        public void ui_init()
        {
            filename_background = null;
        }

        private void picture_addto_platfrom(string filename, int x, int y)
        {
            myBmp = new Bitmap(filename);
            myBmp = img_alpha(myBmp, 128);
            pictureBox_temp = new SYDPictureBox();

            //pictureBox_temp.BackColor = System.Drawing.Color.White;
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
            //this.pictureBox_temp.Click += new System.EventHandler(this.pictureBox_temp_Click);
            pictureBox_temp.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBox_temp_MouseDown);
            pictureBox_temp.MouseMove += new System.Windows.Forms.MouseEventHandler(pictureBox_temp_MouseMove);
            pictureBox_temp.MouseUp += new System.Windows.Forms.MouseEventHandler(pictureBox_temp_MouseUp);
            pictureBox_temp.MouseWheel += new System.Windows.Forms.MouseEventHandler(pictureBox_temp_MouseWheel);
            pictureBox_temp.KeyDown += new System.Windows.Forms.KeyEventHandler(pictureBox_temp_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(pictureBox_temp)).EndInit();
            this.panel_pictureplatfrom.Controls.Clear();
            list_pictureBox.Add(pictureBox_temp);
            for (int i = list_pictureBox.Count - 1; i >= 0; i--)
            {
                this.panel_pictureplatfrom.Controls.Add(list_pictureBox.ElementAt(i));
            }
            pictureBox_temp.Focus();
            text_out(pictureBox_temp);
            if (myBmp == null)
            {
                MessageBox.Show("读取失败");
                return;
            } 
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
        //图片上传
        private void button_openpicture_Click(object sender, EventArgs e)
        {
            string filename = "";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Tiff文件|*.tif|Bmp文件|*.bmp|Erdas img文件|*.img|EVNI文件|*.hdr|jpeg文件|*.jpg|raw文件|*.raw|vrt文件|*.vrt|所有文件|*.*";
            dlg.FilterIndex = 8;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filename = dlg.FileName;
                }
                catch
                {
 
                }
            }
            if (filename == "")
            {
                return;
            }
            picture_addto_platfrom(filename,20,20);
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
                    picture_addto_platfrom(fi.FullName, mousePos.X,  mousePos.Y);
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
            }
            if (e.Button == MouseButtons.Right)//右键点击
            {
                this.panel_pictureplatfrom.Controls.Remove(senderLabel);
                list_pictureBox.Remove(senderLabel);
                dictionary_string.Remove(senderLabel.Name);
            }
        }
        //鼠标松开功能
        private void pictureBox_temp_MouseUp(object sender, MouseEventArgs e)
        {
            SYDPictureBox senderLabel = (SYDPictureBox)sender;//根据sender引用控件。
            if (e.Button == MouseButtons.Left)
            {
                bool alread = true;
                isMove = false;
                richtextBox_picture_result.Text = null;
                int lastIndex = 0;
                int lastLen = 0;
                string stext = "";
                dictionary_string.Clear();
                clear_marray.Clear();
                foreach(SYDPictureBox SYDPictureBox in list_pictureBox)
                {
                    Point point = SYDPictureBox.Location;
                    if (SYDPictureBox.Equals(senderLabel))
                    lastIndex = richtextBox_picture_result.Text.Length;
                    stext = "draw(" + point.X + "," + point.Y + "," + (point.X + SYDPictureBox.Width) + "," + (point.Y + SYDPictureBox.Height) + "," + SYDPictureBox.Name + ")" + ";\n";
                    dictionary_string.Add(SYDPictureBox.Name,stext);
                    richtextBox_picture_result.Text += stext;
                    
                    if (SYDPictureBox.Equals(senderLabel))
                    {
                       lastLen = richtextBox_picture_result.Text.Length - lastIndex;
                    }
                }
                
                while_ture = true;
                while (while_ture)
                {
                    while_ture = false;
                    x_clear_stop_2 = 239;
                    x_clear_star_2 = 0;
                    for (int i = 0; i < 240; i++) 
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
                                //if ((j == x_clear_stop) && (i == 239))
                                //{
                                //    clear_marray.Add(new int[] { x_clear_star, y_clear_star, x_clear_stop + 1, y_clear_stop + 1 });
                                //    x_clear_star = 240;
                                //    y_clear_star = 240;
                                //    x_clear_stop = -1;
                                //    y_clear_stop = -1;
                                //    i = 0; j = 0;
                                //}

                            }
                            else
                            {
                                if (x_clear_stop != -1) x_clear_stop_2 = x_clear_stop;
                                if ((x_clear_star < 240) && (y_clear_star < 240)) 
                                {

                                    if (j < x_clear_stop)
                                    {
                                        if (j > x_clear_star)
                                        {
                                            y_clear_stop--;
                                        }
                                        clear_marray.Add(new int[] { x_clear_star, y_clear_star, x_clear_stop + 1, y_clear_stop + 1 });
                                        x_clear_star = 240;
                                        y_clear_star = 240;
                                        x_clear_stop = -1;
                                        y_clear_stop = -1;
                                        x_clear_star_2 = 0;
                                        x_clear_stop_2 = 239;
                                        i = 0; j = 0;
                                    }
                                    else 
                                    {

                                    }
                                }
                            }
                            if ((x_clear_star < 240) && (y_clear_star < 240))
                            {
                                if ((i == 239) && (j == x_clear_stop_2))
                                {
                                    clear_marray.Add(new int[] { x_clear_star, y_clear_star, x_clear_stop + 1, y_clear_stop + 1 });
                                    x_clear_star = 240;
                                    y_clear_star = 240;
                                    x_clear_stop = -1;
                                    y_clear_stop = -1;
                                    //if (j == 239)
                                    {
                                        i = 0; j = 0;
                                        x_clear_star_2 = 0;
                                        x_clear_stop_2 = 239;
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (int[] list in clear_marray)
                {

                    stext = "oled_clear(" + list[0] + "," + list[1] + "," + list[2] + "," + list[3] + "," + "Color_Black)" + ";\n";
                    //dictionary_string.Add(SYDPictureBox.Name, stext);
                    richtextBox_picture_result.Text += stext;

                    //if (SYDPictureBox.Equals(senderLabel))
                    //{
                    //    lastLen = richtextBox_picture_result.Text.Length - lastIndex;
                    //}
                }
                richtextBox_picture_result.Select(lastIndex, lastLen);
                richtextBox_picture_result.SelectionColor = Color.Red;
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
        //鼠标滚轮滚动功能
        private void pictureBox_temp_MouseWheel(object sender, MouseEventArgs e)
        {
            SYDPictureBox senderLabel = (SYDPictureBox)sender;//根据sender引用控件。
            int x = e.Location.X;
            int y = e.Location.Y;
            int ow = senderLabel.Width;
            int oh = senderLabel.Height;
            int VX, VY;
            if (e.Delta > 0)
            {
                senderLabel.Width += zoomStep;
                senderLabel.Height += zoomStep;
                PropertyInfo pInfo = pictureBox_temp.GetType().GetProperty("ImageRectangle", BindingFlags.Instance |
                  BindingFlags.NonPublic);
                Rectangle rect = (Rectangle)pInfo.GetValue(pictureBox_temp, null);
                senderLabel.Width = rect.Width;
                senderLabel.Height = rect.Height;
            }
            if (e.Delta < 0)
            {
                if (senderLabel.Width < myBmp.Width / 10)
                    return;
                senderLabel.Width -= zoomStep;
                senderLabel.Height -= zoomStep;
                PropertyInfo pInfo = senderLabel.GetType().GetProperty("ImageRectangle", BindingFlags.Instance |
                  BindingFlags.NonPublic);
                Rectangle rect = (Rectangle)pInfo.GetValue(pictureBox_temp, null);
                senderLabel.Width = rect.Width;
                senderLabel.Height = rect.Height;
            }
            VX = (int)((double)x * (ow - senderLabel.Width) / ow);
            VY = (int)((double)y * (oh - senderLabel.Height) / oh);
            senderLabel.Location = new Point(senderLabel.Location.X + VX, senderLabel.Location.Y + VY);
        }
        private void panel_pictureplatfrom_MouseDown(object sender, MouseEventArgs e)
        {
            SYDPictureBox senderLabel = (SYDPictureBox)sender;//根据sender引用控件。
            if (e.Button == MouseButtons.Left)
            {
                mouseDownPoint.X = Cursor.Position.X;
                mouseDownPoint.Y = Cursor.Position.Y;
                isMove = true;
            }
        }
        private void panel_pictureplatfrom_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMove = false;
            }
        }
        private void panel_pictureplatfrom_MouseMove(object sender, MouseEventArgs e)
        {
            SYDPictureBox senderLabel = (SYDPictureBox)sender;//根据sender引用控件。
            panel_pictureplatfrom.Focus();
            if (isMove)
            {
                int x, y;
                int moveX, moveY;
                moveX = Cursor.Position.X - mouseDownPoint.X;
                moveY = Cursor.Position.Y - mouseDownPoint.Y;
                x = senderLabel.Location.X + moveX;
                y = senderLabel.Location.Y + moveY;
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
                if (senderLabel.Location.Y < 240 - senderLabel.Height)
                    senderLabel.Location = new Point(senderLabel.Location.X, senderLabel.Location.Y + offect);
            }
            if (e.KeyCode == Keys.Left)
            {
                if (senderLabel.Location.X > 0)
                    senderLabel.Location = new Point(senderLabel.Location.X - offect, senderLabel.Location.Y);
            }
            if (e.KeyCode == Keys.Right)
            {
                if (senderLabel.Location.X < 240 - senderLabel.Width)
                    senderLabel.Location = new Point(senderLabel.Location.X + offect, senderLabel.Location.Y);
            }
            if (e.KeyCode == Keys.Up)
            {
                if (senderLabel.Location.Y > 0)
                    senderLabel.Location = new Point(senderLabel.Location.X, senderLabel.Location.Y - offect);
            }
            text_out(senderLabel);
        }

        private void text_out(SYDPictureBox senderLabel)
        {
            richtextBox_picture_result.Text = null;
            int lastIndex = 0;
            int lastLen = 0;
            string stext = "";
            dictionary_string.Clear();
            foreach (SYDPictureBox SYDPictureBox in list_pictureBox)
            {
                Point point = SYDPictureBox.Location;
                if (SYDPictureBox.Equals(senderLabel))
                    lastIndex = richtextBox_picture_result.Text.Length;
                stext = "flash_drawBmp(" + point.X + "," + point.Y + "," + (point.X + SYDPictureBox.Width) + "," + (point.Y + SYDPictureBox.Height) + "," + SYDPictureBox.Name + ")" + ";\n";
                dictionary_string.Add(SYDPictureBox.Name, stext);
                richtextBox_picture_result.Text += stext;

                if (SYDPictureBox.Equals(senderLabel))
                {
                    lastLen = richtextBox_picture_result.Text.Length - lastIndex;
                }
            }
            richtextBox_picture_result.Select(lastIndex, lastLen);
            richtextBox_picture_result.SelectionColor = Color.Red;
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
            dlg.Filter = "Tiff文件|*.tif|Bmp文件|*.bmp|Erdas img文件|*.img|EVNI文件|*.hdr|jpeg文件|*.jpg|raw文件|*.raw|vrt文件|*.vrt|所有文件|*.*";
            dlg.FilterIndex = 8;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filename = dlg.FileName;
                }
                catch
                {

                }

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
                    picture_addto_platfrom(filename, x, y);
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
    }

    class SYDPictureBox : PictureBox
    {
        public string filename;
    }
}