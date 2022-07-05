using Microsoft.International.Converters.PinYinConverter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SYD_COPY_FILE
{
    public partial class Form1
    {
        System.Drawing.Image _initImage = null;
        ZoomRate _zoomRate = new ZoomRate();
        Point _picStartPoint = new Point();
        Bitmap picBackGround;

        SpliteRectGroup _splitRectGroup = new SpliteRectGroup();

        bool _Hiddenborder = true;
        int widthNew = 0;
        int heightNew = 0;
        string File_Filter = "xls files (*.jpg;*.png;*.bmp)|*.jpg;*.png;*.bmp|All files (*.*)|*.*";
        byte ui_align_select = 0;
        public enum UI_ALIGN_SELECT_TYPE
        {
            ALIGN_CENTER = 0,
            ALIGN_BOTTOM,
            ALIGN_TOP,
            ALIGN_RIGHT,
            ALIGN_LEFT,
            ALIGN_LEFT_TOP,
            ALIGN_RIGHT_TOP,
            ALIGN_LEFT_BOTTOM,
            ALIGN_RIGHT_BOTTOM,
        };
        public void PicSplit_init()
        {
            comboBoxFileType.Items.Add("png");
            comboBoxFileType.Items.Add("jpg");
            comboBoxFileType.Items.Add("bmp");
            comboBoxFileType.Items.Add("gif");
            comboBoxFileType.SelectedIndex = 2;

            textBoxSaveDir.Text =Settings1.Default.Setting_textBoxSaveDir;

            _picStartPoint.X = int.Parse(textBoxStartX.Text);
            _picStartPoint.Y = int.Parse(textBoxStartY.Text);
        }
        private void buttonOpenPic_Doing(string FileName)
        {
            string name = Path.GetExtension(FileName);
            if (File_Filter.Contains(name.ToLower())==false)
            {
                MessageBox.Show("File type not suppert!");
                return;
            }
            textBoxSrcPic.Text = FileName;
            _initImage = PicManage.GetFromFile(textBoxSrcPic.Text);
            _zoomRate.Reset();
            _zoomRate.SetRate(10);
            _zoomRate.SetSize(_initImage.Width, _initImage.Height);

            ShowPicOnCtrl(_initImage);
            SetSpliteRect(false);
            pictureBoxSrc.Invalidate();

            label125.Text = _initImage.Width.ToString();
            label74.Text = _initImage.Height.ToString();

            checkBoxSplitWidth_Click(null, null);

            picBackGround = new Bitmap(this.pictureBoxSrc.Width, this.pictureBoxSrc.Height);
            this.pictureBoxSrc.DrawToBitmap(picBackGround, new Rectangle(0, 0, this.pictureBoxSrc.Width, this.pictureBoxSrc.Height));
        }
        private void buttonOpenPic_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog1 = new OpenFileDialog();
            fileDialog1.Filter = File_Filter;
            fileDialog1.FilterIndex = 1;
            fileDialog1.RestoreDirectory = true;
            if (fileDialog1.ShowDialog() == DialogResult.OK)
            {
                buttonOpenPic_Doing(fileDialog1.FileName);
            }
            else
            {
            }
        }
        private void buttonReset_Click(object sender, EventArgs e)
        {
            SetSpliteRect(false);
            pictureBoxSrc.Invalidate();
        }

        bool ShowPicOnCtrl(Image img)
        {
            if (pictureBoxSrc.Image != null
                && pictureBoxSrc.Image != _initImage)
            {
                pictureBoxSrc.Image.Dispose();
            }

            pictureBoxSrc.Image = img;

            picBackGround = new Bitmap(this.pictureBoxSrc.Width, this.pictureBoxSrc.Height);
            this.pictureBoxSrc.DrawToBitmap(picBackGround, new Rectangle(0, 0, this.pictureBoxSrc.Width, this.pictureBoxSrc.Height));
            return true;
        }

        private void buttonSplite_Click(object sender, EventArgs e)
        {
            DateTime t1 = DateTime.Now;
            if (checkBox17.Checked)
            {
                string filepath = source_copyfile_textBox_sync.Text;//等到的完整的文件名
                string dir = Path.GetDirectoryName(textBoxSrcPic.Text);
                string ext = Path.GetExtension(textBoxSrcPic.Text);
                string[] filenames = Directory.GetFiles(dir, "*" + ext, SearchOption.AllDirectories);//获取目录文件名称集合

                foreach (string filename in filenames)
                {
                    buttonOpenPic_Doing(filename);
                    DoSplite(false);
                }
            }
            else
            {
                DoSplite(false);
            }
            TimeSpan span = DateTime.Now-t1;
            MessageBox.Show(string.Format("切割完毕！耗时:{0}秒{1}毫秒", (int)(span.TotalMilliseconds/1000), ((int)span.TotalMilliseconds)%1000));
        }
        private void buttonSplite_firstpicture_Click(object sender, EventArgs e)
        {
            DateTime t1 = DateTime.Now;
            if (checkBox17.Checked)
            {
                string filepath = source_copyfile_textBox_sync.Text;//等到的完整的文件名
                string dir = Path.GetDirectoryName(textBoxSrcPic.Text);
                string ext = Path.GetExtension(textBoxSrcPic.Text);
                string[] filenames = Directory.GetFiles(dir, "*"+ext, SearchOption.AllDirectories);//获取目录文件名称集合
                
                foreach (string filename in filenames)
                {
                    buttonOpenPic_Doing(filename);
                    DoSplite(true);
                }
            }
            else
            {
                DoSplite(true);
            }
            TimeSpan span = DateTime.Now - t1;
            MessageBox.Show(string.Format("切割完毕！耗时:{0}秒{1}毫秒", (int)(span.TotalMilliseconds / 1000), ((int)span.TotalMilliseconds) % 1000));
        }
        //OnlySpliteFirst  false:根据有多少个框切割 true:只是切割第一个切图
        void DoSplite(bool OnlySpliteFirst)
        {
            if (_initImage == null)
                return;

            try
            {
                Directory.CreateDirectory(textBoxSaveDir.Text);
                string fileName = GetSaveFileName();
                string fileType = comboBoxFileType.Text;

                int i = 0;
                //切割文件
                using (FileStream sFileStream = new FileStream(textBoxSrcPic.Text, FileMode.Open))
                {
                    Image initImage = Image.FromStream(sFileStream, true);

                    int quality = 100; //图片质量
                    Image destImage = null;

                    foreach (Rectangle rect in _splitRectGroup.GetRectsSplit())
                    {
                        i++;
                        string filePath = "";
                        if (OnlySpliteFirst == false) filePath = Path.Combine(textBoxSaveDir.Text, string.Format("{0}_{1}.{2}", fileName, i, fileType));
                        else filePath = Path.Combine(textBoxSaveDir.Text, fileName+ "."+fileType);

                        File.Delete(filePath);

                        bool ret = PicManage.SplitPic(initImage, rect.X, rect.Y, rect.Width, rect.Height,
                            ref destImage);

                        if (ret)
                        {
                            //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                            ImageCodecInfo ici = PicManage.GetCodeInfo(filePath);
                            EncoderParameters ep = new EncoderParameters(1);
                            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
                            destImage.Save(filePath, ici, ep);
                        }
                        if (OnlySpliteFirst == true)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        bool SetSpliteRect(bool showMsg)
        {
            try
            {
                int width = int.Parse(textBoxSplitWidth.Text);
                int height = int.Parse(textBoxSplitHeight.Text);

                if (width == 0)
                    width = _initImage.Width;

                int startX = int.Parse(textBoxStartX.Text);
                int startY = int.Parse(textBoxStartY.Text);
                if (showMsg && (startX >= _initImage.Width || startY >= _initImage.Height))
                {
                    MessageBox.Show("切割起始点不能大于图片尺寸！");
                    return false;
                }

                _splitRectGroup.ClearNotUsedRect();
                _splitRectGroup.SetRect(_initImage.Width, _initImage.Height,startX, startY, width, height);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }

        public string GetSaveFileName()
        {
            string ret = Path.GetFileNameWithoutExtension(textBoxSrcPic.Text);
            return ret;
        }

        private void buttonZoomOut_Click(object sender, EventArgs e)
        {
            if (_initImage == null)
                return;

            _zoomRate.GetNextZoomOut(ref widthNew, ref heightNew);

            Image newImage = null; ;
            PicManage.ZoomAuto(_initImage, ref newImage, widthNew, heightNew);
            if (newImage != null)
            {
                ShowPicOnCtrl(newImage);
            }
        }

        private void buttonZoomIn_Click(object sender, EventArgs e)
        {
            if (_initImage == null)
                return;

            _zoomRate.GetNextZoomIn(ref widthNew, ref heightNew);

            Image newImage = null; ;
            PicManage.ZoomAuto(_initImage, ref newImage, widthNew, heightNew);
            if (newImage != null)
            {
                ShowPicOnCtrl(newImage);
            }
        }

        private void buttonActualSize_Click(object sender, EventArgs e)
        {
            _zoomRate.Reset();
            ShowPicOnCtrl(_initImage);
        }

        bool _showSpliteInfo = false;
        void ShowSpliteInfo()
        {
            _showSpliteInfo = true;
            int newX = _splitRectGroup.GetStartX();
            int newY = _splitRectGroup.GetStartY();
            SetShowStartPoint(newX, newY);

            int width = _splitRectGroup.GetSpliteWidth();
            textBoxSplitWidth.Text = width.ToString();
            _showSpliteInfo = false;
        }

        private void pictureBoxSrc_MouseMove(object sender, MouseEventArgs e)
        {
            if (_splitRectGroup.IsAllMove(e.X, e.Y))
            {
                pictureBoxSrc.Cursor = Cursors.Hand;
                if (_pictureBoxSrcMouseDown)
                {
                    _splitRectGroup.SetMoveAllFlag(true);
                    _splitRectGroup.StartPointMoveTo(e.X, e.Y);
                    ShowSpliteInfo();
                    pictureBoxSrc.Invalidate();
                }
                return;
            }
            //只要上一次是移动的标识  就移动
            if (_pictureBoxSrcMouseDown && _splitRectGroup.GetMoveAllFlag())
            {
                _splitRectGroup.SetMoveAllFlag(true);
                _splitRectGroup.StartPointMoveTo(e.X, e.Y);
                ShowSpliteInfo();
                pictureBoxSrc.Invalidate();
                return;
            }

            _splitRectGroup.SetMoveAllFlag(false);

            //水平或垂直分割 
            SpliteMoveIndex index = _splitRectGroup.PointHit(e.X, e.Y, _splitRectGroup._defaultHitSpace);
            label_Index.Text = index.rectIndex.ToString();
            label_Line.Text = index.lineIndex.ToString();
            label_xsite.Text = index.mouseX.ToString();
            label_ysite.Text = index.mouseY.ToString();
            double rate=_zoomRate.GetRate();
            label_11xsite.Text = ((int)(index.mouseX / rate)).ToString();
            label_11ysite.Text = ((int)(index.mouseY / rate)).ToString();

            if (_pictureBoxSrcMouseDown)
            {
                if (!index.IsIn())
                {
                    index = _splitRectGroup.PointHit(e.X, e.Y, _splitRectGroup._defaultHitSpace * 2 + 30);
                }

                if (index.IsIn())
                {
                    bool move = _splitRectGroup.SetMove(index);
                    if (move)
                    {
                        ShowSpliteInfo();
                        pictureBoxSrc.Invalidate();
                    }
                }
            }

            if (index.IsHorIn())
            {
                pictureBoxSrc.Cursor = Cursors.HSplit;
            }
            else if (index.IsVertIn())
            {
                pictureBoxSrc.Cursor = Cursors.VSplit;
            }
            else
            {
                pictureBoxSrc.Cursor = Cursors.Arrow;
            }
        }

        bool _pictureBoxSrcMouseDown = false;

        private void pictureBoxSrc_MouseDown(object sender, MouseEventArgs e)
        {
            _splitRectGroup.ResetMoveFlag();
            _pictureBoxSrcMouseDown = true;
            this.pictureBoxSrc.Focus();

            //水平或垂直分割 
            SpliteMoveIndex index = _splitRectGroup.PointHit(e.X, e.Y, _splitRectGroup._defaultHitSpace);
            if (picBackGround != null)
            {
                Color pixelColor = picBackGround.GetPixel(index.mouseX, index.mouseY);
                textBoxSplitR.Text = pixelColor.R.ToString();  //颜色的 RED 分量值
                textBoxSplitG.Text = pixelColor.G.ToString();  //颜色的 GREEN 分量值
                textBoxSplitB.Text = pixelColor.B.ToString();  //颜色的 BLUE 分量值
                cal_rgb_subtract(textBoxSplitR, textBoxSplitG, textBoxSplitB, textBoxSplitRGB565);
            }
        }
        private void pictureBoxSrc_MouseUp(object sender, MouseEventArgs e)
        {
            _pictureBoxSrcMouseDown = false;

            if (e.Button == MouseButtons.Right)
            {
            }
        }
        private void pictureBoxSrc_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            _splitRectGroup.Draw(g, _Hiddenborder);
        }

        public bool SetShowStartPoint(int x, int y)
        {
            int oldx = int.Parse(textBoxStartX.Text);
            int oldy = int.Parse(textBoxStartY.Text);
            if (oldx == x && oldy == y)
                return false;

            textBoxStartX.Text = x.ToString();
            textBoxStartY.Text = y.ToString();
            return true;
        }

        private void textBoxStartX_TextChanged(object sender, EventArgs e)
        {
            //显示信息 不触发动作
            if (_showSpliteInfo)
                return;

            try
            {
                _picStartPoint.X = int.Parse(textBoxStartX.Text);
            }
            catch
            {
                _picStartPoint.X = 0;
            }

            try
            {
                _picStartPoint.Y = int.Parse(textBoxStartY.Text);
            }
            catch
            {
                _picStartPoint.Y = 0;
            }

            _splitRectGroup.StartPointMoveTo(_picStartPoint.X, _picStartPoint.Y);
            pictureBoxSrc.Invalidate();
        }

        private void textBoxSplitWidth_TextChanged(object sender, EventArgs e)
        {
            //显示信息 不触发动作
            if (_showSpliteInfo)
                return;

            if (((TextBox)sender).Text.Length == 0)
                return;

            if (int.Parse(((TextBox)sender).Text) == 0)
                return;

            SetSpliteRect(true);
            pictureBoxSrc.Invalidate();
        }

        private void textBoxSplitWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                int kc = (int)e.KeyChar;
                if ((kc < 48 || kc > 57) && kc != 8)
                    e.Handled = true;
            }
            catch (Exception)
            {
            }
        }
        private void buttonSelSaveDir_Click(object sender, EventArgs e)
        {
            MyFolderBrowserDialog folderDlg = new MyFolderBrowserDialog();
            if (folderDlg.ShowDialog(this) == DialogResult.OK)
            {
                textBoxSaveDir.Text = folderDlg.DirectoryPath;
            }
        }
        private void button_Hiddenborder_Click(object sender, EventArgs e)
        {
            if (_Hiddenborder == true)
            {
                _Hiddenborder = false;
                this.button_Hiddenborder.Text = "显示边框";
            }
            else
            {
                _Hiddenborder = true;
                this.button_Hiddenborder.Text = "隐藏边框";
            }
            buttonZoomOut_Click(null, null);
            buttonZoomIn_Click(null, null);
        }
        private void ui_xy_cal()
        {
            if (textBox84.Text.Length == 0)
            {
                MessageBox.Show("屏幕宽度出错");
                return;
            }
            if (textBox83.Text.Length == 0)
            {
                MessageBox.Show("屏幕高度出错");
                return;
            }
            if (textBox54.Text.Length == 0)
            {
                MessageBox.Show("图片宽度出错");
                return;
            }
            if (textBox69.Text.Length == 0)
            {
                MessageBox.Show("图片高度出错");
                return;
            }
            if (textBox85.Text.Length == 0)
            {
                MessageBox.Show("X0偏移出错");
                return;
            }
            if (textBox86.Text.Length == 0)
            {
                MessageBox.Show("Y0偏移出错");
                return;
            }
            UInt32 sw = 0, sh = 0, pw = 0, ph = 0, x0 = 0, x1 = 0, y0 = 0, y1 = 0, x0_offect = 0, y0_offect = 0;
            sw = Convert.ToUInt32(textBox84.Text);
            sh = Convert.ToUInt32(textBox83.Text);
            pw = Convert.ToUInt32(textBox54.Text);
            ph = Convert.ToUInt32(textBox69.Text);
            x0_offect = Convert.ToUInt32(textBox85.Text);
            y0_offect = Convert.ToUInt32(textBox86.Text);
            if (pw> sw)
            {
                MessageBox.Show("图片宽度太大");
                return;
            }
            if (ph > sh)
            {
                MessageBox.Show("图片高度太大");
                return;
            }
            if (x0_offect > sw)
            {
                MessageBox.Show("X偏于太大");
                return;
            }
            if (y0_offect > sh)
            {
                MessageBox.Show("Y偏于太大");
                return;
            }
            sw -= x0_offect;  //减去偏移值,然后计算
            sh -= y0_offect;
            if (ui_align_select== (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_CENTER)  //居中
            { 
                x0 = (sw - pw) / 2;
                x1 = x0+ pw;
                y0 = (sh - ph) / 2;
                y1 = y0 + ph;
            }
            else if(ui_align_select == (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_BOTTOM)  //下对齐
            {
                x0 = (sw - pw) / 2;
                x1 = x0 + pw;
                y0 = sh - ph;
                y1 = sh;
            }
            else if (ui_align_select == (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_TOP)  //上对齐
            {
                x0 = (sw - pw) / 2;
                x1 = x0 + pw;
                y0 = 0;
                y1 = ph;
            }
            else if (ui_align_select == (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_RIGHT)  //右对齐
            {
                x0 = sw - pw;
                x1 = sh;
                y0 = (sh - ph) / 2;
                y1 = y0 + ph;
            }
            else if (ui_align_select == (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_LEFT)  //左对齐
            {
                x0 = 0;
                x1 = pw;
                y0 = (sh - ph) / 2;
                y1 = y0 + ph;
            }
            else if (ui_align_select == (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_LEFT_TOP)
            {
                x0 = 0;
                x1 = pw;
                y0 = 0;
                y1 = ph;
            }
            else if (ui_align_select == (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_RIGHT_TOP)
            {
                x0 = sw - pw;
                x1 = sw;
                y0 = 0;
                y1 = ph;
            }
            else if (ui_align_select == (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_LEFT_BOTTOM)
            {
                x0 = 0;
                x1 = pw;
                y0 = sh - ph;
                y1 = sh;
            }
            else if (ui_align_select == (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_RIGHT_BOTTOM)
            {
                x0 = sw - pw;
                x1 = pw;
                y0 = sh - ph;
                y1 = sh;
            }
            x0 += x0_offect;
            x1 += x0_offect;
            y0 += y0_offect;
            y1 += y0_offect;
            textBox71.Text = x0.ToString();
            textBox82.Text = x1.ToString();
            textBox73.Text = y0.ToString();
            textBox77.Text = y1.ToString();
        }
        private void textBox54_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ui_xy_cal();
            }
        }

        private void textBox69_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ui_xy_cal();
            }
        }
        private void ui_align_click(object sender)
        {
            if (checkBox11 != (CheckBox)sender)
            {
                checkBox11.Checked = false;
            }
            if (checkBox5 != (CheckBox)sender)
            {
                checkBox5.Checked = false;
            }
            if (checkBox3 != (CheckBox)sender)
            {
                checkBox3.Checked = false;
            }
            if (checkBox4 != (CheckBox)sender)
            {
                checkBox4.Checked = false;
            }
            if (checkBox2 != (CheckBox)sender)
            {
                checkBox2.Checked = false;
            }
            if (checkBox12 != (CheckBox)sender)
            {
                checkBox12.Checked = false;
            }
            if (checkBox13 != (CheckBox)sender)
            {
                checkBox13.Checked = false;
            }
            if (checkBox14 != (CheckBox)sender)
            {
                checkBox14.Checked = false;
            }
            if (checkBox15 != (CheckBox)sender)
            {
                checkBox15.Checked = false;
            }
            ui_xy_cal();
        }
        private void checkBox2_Click(object sender, EventArgs e)
        {
            ui_align_select = (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_CENTER;  //居中
            pictureBox1.Image = Properties.Resources.Align_center;
            ui_align_click(sender);
        }
        
        private void checkBox11_Click(object sender, EventArgs e)
        {
            ui_align_select = (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_BOTTOM; //下对齐
            pictureBox1.Image = Properties.Resources.Align_bottom;
            ui_align_click(sender);
        }

        private void checkBox5_Click(object sender, EventArgs e)
        {
            ui_align_select = (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_TOP; //上对齐
            pictureBox1.Image = Properties.Resources.Align_top;
            ui_align_click(sender);
            
        }

        private void checkBox3_Click(object sender, EventArgs e)
        {
            ui_align_select = (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_RIGHT; //右对齐
            pictureBox1.Image = Properties.Resources.Align_right;
            ui_align_click(sender);
            
        }

        private void checkBox4_Click(object sender, EventArgs e)
        {
            ui_align_select = (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_LEFT; //左对齐
            pictureBox1.Image = Properties.Resources.Align_left;
            ui_align_click(sender);
            
        }
        private void checkBox12_Click(object sender, EventArgs e)
        {
            ui_align_select = (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_LEFT_TOP; //左对齐
            pictureBox1.Image = Properties.Resources.Align_left_top;
            ui_align_click(sender);
        }

        private void checkBox13_Click(object sender, EventArgs e)
        {
            ui_align_select = (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_RIGHT_TOP; //左对齐
            pictureBox1.Image = Properties.Resources.Align_right_top;
            ui_align_click(sender);
        }

        private void checkBox14_Click(object sender, EventArgs e)
        {
            ui_align_select = (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_LEFT_BOTTOM; //左对齐
            pictureBox1.Image = Properties.Resources.Align_left_bottom;
            ui_align_click(sender);
        }

        private void checkBox15_Click(object sender, EventArgs e)
        {
            ui_align_select = (Byte)UI_ALIGN_SELECT_TYPE.ALIGN_RIGHT_BOTTOM; //左对齐
            pictureBox1.Image = Properties.Resources.Align_right_bottom;
            ui_align_click(sender);
        }
        private void splitContainerMain_Panel2_DragDrop(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;
            if (dataObject == null) return;
            string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);

            foreach (string file in files)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(file);
                if (fi.FullName != "")
                {
                    buttonOpenPic_Doing(fi.FullName);
                }
            }
        }
        private void checkBoxSplitWidth_Click(object sender, EventArgs e)
        {
            if (checkBoxSplitWidth.Checked == true)
            {
                textBoxSplitWidth.Text = _initImage.Width.ToString();
            }
        }
        private void splitContainerMain_Panel2_DragEnter(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;
            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void button_Bulk_zoom_Click(object sender, EventArgs e)
        {
            DateTime t1 = DateTime.Now;
            string filepath = source_copyfile_textBox_sync.Text;//等到的完整的文件名
            string dir = Path.GetDirectoryName(textBoxSrcPic.Text);
            string ext = Path.GetExtension(textBoxSrcPic.Text);
            string[] filenames = Directory.GetFiles(dir, "*" + ext, SearchOption.AllDirectories);//获取目录文件名称集合
            int quality = 100; //图片质量
            string fileType = comboBoxFileType.Text;

            foreach (string filename in filenames)
            {
                try
                {
                    Image output;
                    System.Drawing.Image source = System.Drawing.Image.FromFile(filename);
                    output = source.GetThumbnailImage(Convert.ToInt32(textBox_Bulk_zoom_w.Text), Convert.ToInt32(textBox_Bulk_zoom_h.Text), () => false, IntPtr.Zero);
                    string str=Path.GetFileNameWithoutExtension(filename);//没有扩展名
                    string filePath = Path.Combine(textBoxSaveDir.Text, str + "."+ fileType);
                    File.Delete(filePath);

                    //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                    ImageCodecInfo ici = PicManage.GetCodeInfo(filePath);
                    EncoderParameters ep = new EncoderParameters(1);
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
                    output.Save(filePath, ici, ep);
                }
                catch
                {
                    MessageBox.Show("缩放失败:"+ filename);
                    return;
                }
            }
            TimeSpan span = DateTime.Now - t1;
            MessageBox.Show(string.Format("缩放完毕！耗时:{0}秒{1}毫秒", (int)(span.TotalMilliseconds / 1000), ((int)span.TotalMilliseconds) % 1000));
        }
        private void button_Batch_convert_format_Click_Click(object sender, EventArgs e)
        {
            DateTime t1 = DateTime.Now;
            string filepath = source_copyfile_textBox_sync.Text;//等到的完整的文件名
            string dir = Path.GetDirectoryName(textBoxSrcPic.Text);
            string ext = Path.GetExtension(textBoxSrcPic.Text);
            string[] filenames = Directory.GetFiles(dir, "*" + ext, SearchOption.AllDirectories);//获取目录文件名称集合
            int quality = 100; //图片质量
            string fileType = comboBoxFileType.Text;

            foreach (string filename in filenames)
            {
                try
                {
                    System.Drawing.Image source = System.Drawing.Image.FromFile(filename);
                    string str = Path.GetFileNameWithoutExtension(filename);//没有扩展名
                    var pingyins = PinYinConverterHelp.GetTotalPingYin(str);
                    str = String.Join("_", pingyins.TotalPingYin);
                    string filePath = Path.Combine(textBoxSaveDir.Text, str + "." + fileType);
                    File.Delete(filePath);

                    //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                    ImageCodecInfo ici = PicManage.GetCodeInfo(filePath);
                    EncoderParameters ep = new EncoderParameters(1);
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
                    source.Save(filePath, ici, ep);
                }
                catch
                {
                    MessageBox.Show("批量转换格式失败:" + filename);
                    return;
                }
            }
            TimeSpan span = DateTime.Now - t1;
            MessageBox.Show(string.Format("批量转换格式完毕！耗时:{0}秒{1}毫秒", (int)(span.TotalMilliseconds / 1000), ((int)span.TotalMilliseconds) % 1000));
        }
        private void cal_rgb_subtract(int input, TextBox output)
        {
            Byte a, b, c;
            a = (Byte)(input >> 19);
            b = (Byte)(input >> 10);
            c = (Byte)(input >> 3);
            input = (a << 11) | (b << 5) | c;
            output.Text = "0x" + ((UInt32)input).ToString("X");
        }
        private void cal_rgb_subtract(TextBox R, TextBox G, TextBox B, TextBox output)
        {
            if (R.Text.Length == 0)
            {
                R.Text = "0";
            }
            if (G.Text.Length == 0)
            {
                G.Text = "0";
            }
            if (B.Text.Length == 0)
            {
                B.Text = "0";
            }
            int temp = 0;
            if(checkBoxRgbHex.Checked) temp = Convert.ToInt32(B.Text, 16) | (Convert.ToInt32(G.Text, 16) << 8) | (Convert.ToInt32(R.Text, 16) << 16);
            else temp = Convert.ToInt32(B.Text, 10) | (Convert.ToInt32(G.Text, 10) << 8) | (Convert.ToInt32(R.Text, 10) << 16);

            cal_rgb_subtract(temp, output);
        }
        private void buttonRGB_Click(object sender, EventArgs e)
        {
            cal_rgb_subtract(textBoxSplitR, textBoxSplitG, textBoxSplitB, textBoxSplitRGB565);
        }
        private void checkBoxRgbHex_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox radiobtn = (CheckBox)sender;
            if (radiobtn.Checked)
            {
                textBoxSplitR.Text = StringToHexString(textBoxSplitR.Text, "X");
                textBoxSplitG.Text = StringToHexString(textBoxSplitG.Text, "X");
                textBoxSplitB.Text = StringToHexString(textBoxSplitB.Text, "X");
            }
            else
            {
                textBoxSplitR.Text = HexStringToString(textBoxSplitR.Text);
                textBoxSplitG.Text = HexStringToString(textBoxSplitG.Text);
                textBoxSplitB.Text = HexStringToString(textBoxSplitB.Text);
            }
        }
    }
}
