using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Windows.Data;
using System.Diagnostics;
namespace SYD_COPY_FILE
{
    public partial class Form1
    {
        public static object[] suffix_textBox_rename_static = new object[] {
        "_Integration ",
        "_changan_L31_V110_",
        "_CP_L32_V118_",
        "_letu_L32_V109_",
        "_wuling_L33_V124_"};
    public void copy_file_init()
        {
            this.label_copy_time.Text = "拷贝文件时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.label_copy_time_sync.Text = "拷贝文件时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.timer1.Interval = 1000;
            this.timer1.Start();

            comboBox_timetype_rename.SelectedIndex = 2;

            source_copyfile_textBox.Text = Settings1.Default.source_copyfile_textBox;
            destination_file_textBox.Text = Settings1.Default.destination_file_textBox;
            destination_file_textBox_two.Text = Settings1.Default.destination_file_textBox_two;
            textBox_copy_destinationfileend.Text = Settings1.Default.textBox_copy_destinationfileend;
            source_copyfile_textBox_sync.Text = Settings1.Default.source_copyfile_textBox_sync;
            destination_file_textBox_sync.Text = Settings1.Default.destination_file_textBox_sync;
            destination_file_textBox_two_sync.Text = Settings1.Default.destination_file_textBox_two_sync;
            textBox_copy_destinationfileend_sync.Text = Settings1.Default.textBox_copy_destinationfileend_sync;
            if (Settings1.Default.Setting_textBox_section1 != "")
            textBox_section1.Text = Settings1.Default.Setting_textBox_section1;
            if (Settings1.Default.Setting_textBox_section2 != "")
            textBox_section2.Text = Settings1.Default.Setting_textBox_section2;
            if (Settings1.Default.Setting_textBox_section3 != "")
            textBox_section3.Text = Settings1.Default.Setting_textBox_section3;
            if (Settings1.Default.Setting_textBox_section4 != "")
            textBox_section4.Text = Settings1.Default.Setting_textBox_section4;
            if (Settings1.Default.Setting_textBox_section5 != "")
            textBox_section5.Text = Settings1.Default.Setting_textBox_section5;
            if (Settings1.Default.Setting_source_copyfile_textBox_rename != "")
                source_copyfile_textBox_rename.Text = Settings1.Default.Setting_source_copyfile_textBox_rename;
            if (Settings1.Default.Setting_source_copyfile_prefix_textBox_rename != "")
                source_copyfile_prefix_textBox_rename.Text = Settings1.Default.Setting_source_copyfile_prefix_textBox_rename.Trim();
            if (Settings1.Default.Setting_source_copyfile_suffix_textBox_rename != "")
                source_copyfile_suffix_textBox_rename.Text = Settings1.Default.Setting_source_copyfile_suffix_textBox_rename.Trim(); 
            checkBox10.Checked = Settings1.Default.Setting_checkBox10;
            checkBox_systemtime_rename.Checked = Settings1.Default.Setting_checkBox_systemtime_rename;
            checkBox_delete_srcfile.Checked = Settings1.Default.Setting_checkBox_delete_srcfile;

            if (Settings1.Default.suffix_textBox_rename_line1 != "") this.source_copyfile_suffix_textBox_rename.Items.Add(Settings1.Default.suffix_textBox_rename_line1);
            if (Settings1.Default.suffix_textBox_rename_line2 != "") this.source_copyfile_suffix_textBox_rename.Items.Add(Settings1.Default.suffix_textBox_rename_line2);
            if (Settings1.Default.suffix_textBox_rename_line3 != "") this.source_copyfile_suffix_textBox_rename.Items.Add(Settings1.Default.suffix_textBox_rename_line3);
            if (Settings1.Default.suffix_textBox_rename_line4 != "") this.source_copyfile_suffix_textBox_rename.Items.Add(Settings1.Default.suffix_textBox_rename_line4);
            if (Settings1.Default.suffix_textBox_rename_line5 != "") this.source_copyfile_suffix_textBox_rename.Items.Add(Settings1.Default.suffix_textBox_rename_line5);
            if (Settings1.Default.suffix_textBox_rename_line6 != "") this.source_copyfile_suffix_textBox_rename.Items.Add(Settings1.Default.suffix_textBox_rename_line6);
            if (Settings1.Default.suffix_textBox_rename_line7 != "") this.source_copyfile_suffix_textBox_rename.Items.Add(Settings1.Default.suffix_textBox_rename_line7);
            if (Settings1.Default.suffix_textBox_rename_line8 != "") this.source_copyfile_suffix_textBox_rename.Items.Add(Settings1.Default.suffix_textBox_rename_line8);
            if (Settings1.Default.suffix_textBox_rename_line9 != "") this.source_copyfile_suffix_textBox_rename.Items.Add(Settings1.Default.suffix_textBox_rename_line9);
            if (Settings1.Default.suffix_textBox_rename_line10 != "") this.source_copyfile_suffix_textBox_rename.Items.Add(Settings1.Default.suffix_textBox_rename_line10);
            this.source_copyfile_suffix_textBox_rename.Items.AddRange(suffix_textBox_rename_static);

            comboBox7.SelectedIndex = Settings1.Default.rename_mode_sel;

            update_StripStatusLabel();
        }
        private void update_state(string name, string Clipboard_name)
        {

            this.label_copy_time.Text = "拷贝文件时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  //UP
            this.label_copy_time_sync.Text = "拷贝文件时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (this.label_copy_time.ForeColor == Color.Blue)
            {
                this.label_copy_time.ForeColor = Color.Red;
                this.label_copy_time_sync.ForeColor = Color.Red;
            }
            else if (this.label_copy_time.ForeColor == Color.Red)
            {
                this.label_copy_time.ForeColor = Color.Black;
                this.label_copy_time_sync.ForeColor = Color.Black;
            }
            else if (this.label_copy_time.ForeColor == Color.Black)
            {
                this.label_copy_time.ForeColor = Color.Blue;
                this.label_copy_time_sync.ForeColor = Color.Blue;
            }
            else
            {
                this.label_copy_time.ForeColor = Color.Blue;
                this.label_copy_time_sync.ForeColor = Color.Blue;
            }

            this.Text = name;
            if (Clipboard_name != null)
            {
                string[] file = new string[1];
                file[0] = Clipboard_name;
                DataObject dataObject = new DataObject();
                dataObject.SetData(DataFormats.FileDrop, file);
                Clipboard.SetDataObject(dataObject, true);
            }
        }

        private void update_state_rename(string name)
        {

            this.label_copy_time.Text = "重命名/拷贝文件时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  //UP
            this.label_copy_time_sync.Text = "重命名/拷贝文件时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  //UP
            if (this.label_copy_time.ForeColor == Color.Blue)
            {
                this.label_copy_time.ForeColor = Color.Red;
                this.label_copy_time_sync.ForeColor = Color.Red;
            }
            else if (this.label_copy_time.ForeColor == Color.Red)
            {
                this.label_copy_time.ForeColor = Color.Black;
                this.label_copy_time_sync.ForeColor = Color.Black;
            }
            else if (this.label_copy_time.ForeColor == Color.Black)
            {
                this.label_copy_time.ForeColor = Color.Blue;
                this.label_copy_time_sync.ForeColor = Color.Blue;
            }
            else
            {
                this.label_copy_time.ForeColor = Color.Blue;
                this.label_copy_time_sync.ForeColor = Color.Blue;
            }

            this.Text = name;

            for (int i = 0; i < this.source_copyfile_suffix_textBox_rename.Items.Count; i++)
            {
                if (source_copyfile_suffix_textBox_rename.Text == this.source_copyfile_suffix_textBox_rename.Items[i].ToString()) return;
            }
            this.source_copyfile_suffix_textBox_rename.Items.Insert(0, source_copyfile_suffix_textBox_rename.Text);
        }
        private void source_copyfile_button_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "source_file";

           // dlg.DefaultExt = ".txt";

            dlg.Filter = "All files（*.*）|*.*|All files(*.*)|*.* ";

            if (dlg.ShowDialog() == false)
                return;

            if (sender == source_copyfile_button)
                this.source_copyfile_textBox.Text = dlg.FileName;
            else if (sender == source_copyfile_button_sync)
                this.source_copyfile_textBox_sync.Text = dlg.FileName;
            else if (sender == destination_file_button_sync)
                this.destination_file_textBox_sync.Text = dlg.FileName;
            else if (sender == destination_file_button_copy_sourcefile_sync)
                this.destination_file_textBox_two_sync.Text = dlg.FileName;
            else if (sender == button_copy_destinationfileend_sync)
                this.textBox_copy_destinationfileend_sync.Text = dlg.FileName;
            else if (sender == destination_file_button_copy_sourcefile)
                this.destination_file_textBox_two.Text = dlg.FileName;
            else if (sender == button_copy_destinationfileend)
                this.textBox_copy_destinationfileend.Text = dlg.FileName;
        }
        private void source_copyfile_textBox_DragEnter(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;
            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void source_copyfile_textBox_DragDrop(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;

            if (dataObject == null) return;

            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
                TextBox textbox=(TextBox)sender;
                foreach (string file in files)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    if (sender == source_copyfile_textBox)
                        this.source_copyfile_textBox.Text = fi.FullName;
                    else if (sender == destination_file_textBox)
                        this.destination_file_textBox.Text = fi.FullName;
                    else if (sender == destination_file_textBox_two)
                        this.destination_file_textBox_two.Text = fi.FullName;
                    else if (sender == textBox_copy_destinationfileend)
                        this.textBox_copy_destinationfileend.Text = fi.FullName;
                    else if (sender == source_copyfile_textBox_rename)
                        this.source_copyfile_textBox_rename.Text = fi.FullName;
                    else if (sender == source_copyfile_textBox_sync)
                        this.source_copyfile_textBox_sync.Text = fi.FullName;
                    else if (sender == destination_file_textBox_sync)
                        this.destination_file_textBox_sync.Text = fi.FullName;
                    else if (sender == destination_file_textBox_two_sync)
                        this.destination_file_textBox_two_sync.Text = fi.FullName;
                    else if (sender == textBox_copy_destinationfileend_sync)
                        this.textBox_copy_destinationfileend_sync.Text = fi.FullName;
                    else if (sender == textBox_splitbinfile)
                        this.textBox_splitbinfile.Text = fi.FullName;
                    else if (sender == textBoxSrcPic)
                    {
                        buttonOpenPic_Doing(fi.FullName);
                    }
                    else if (sender == source_file_textBox)
                    {
                        source_file_textBox.Text= fi.FullName;
                        reopen_source_file_button_Click(sender, e);
                    }
                    else if (sender == comboBox_indicate)
                    {
                        source_file_textBox.Text = fi.FullName;
                    }
                    else
                        textbox.Text = fi.FullName; 
                }
            }
        }
        private void source_copyfile_ComBox_DragDrop(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;

            if (dataObject == null) return;

            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
                ComboBox combox = (ComboBox)sender;
                foreach (string file in files)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    if (sender == comboBox_indicate)
                    {
                        comboBox_indicate.Text = fi.FullName;
                    }
                }
            }
        }
        private void destination_file_button_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "destination_file";

           // dlg.DefaultExt = ".txt";

            dlg.Filter = "All files（*.*）|*.*|All files(*.*)|*.* ";

            if (dlg.ShowDialog() == false)
                return;
            destination_file_textBox.Text = dlg.FileName;
        }
        private void destination_file_button_copy_sourcefile_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "destination_file";

            //dlg.DefaultExt = ".txt";

            dlg.Filter = "All files（*.*）|*.*|All files(*.*)|*.* ";

            if (dlg.ShowDialog() == false)
                return;
            destination_file_textBox_two.Text = dlg.FileName;
        }
        private bool ask_iscontinue()
        {
            DialogResult MsgBoxResult;//设置对话框的返回值

            MsgBoxResult = MessageBox.Show("源文件和目标文件格式不一致,是否继续拷贝",//对话框的显示内容

            "提示",//对话框的标题

            MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮

            MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号

            MessageBoxDefaultButton.Button2);//定义对话框的按钮式样

            if (MsgBoxResult == DialogResult.No)//如果对话框的返回值是YES（按"Y"按钮）

            {
                return false;
            }
            return true;
        }
        private void copy_button_Click(object sender, EventArgs e)
        {
            string extension_src = "";
            string extension_des = "";
            if (source_copyfile_textBox.TextLength != 0)
            {
                string name = Path.GetFileNameWithoutExtension(source_copyfile_textBox.Text);

                if (destination_file_textBox.TextLength != 0)
                {
                    if (File.Exists(source_copyfile_textBox.Text) == false)
                    {
                        MessageBox.Show("源文件不存在!");
                        return;
                    }
                    extension_src = Path.GetExtension(source_copyfile_textBox.Text);//扩展名 ".aspx"
                    extension_des = Path.GetExtension(destination_file_textBox.Text);//扩展名 ".aspx"

                    if (extension_src != extension_des)
                    {
                        if (ask_iscontinue() == false)
                        {
                            return;
                        }
                    }
                    File.Copy(@source_copyfile_textBox.Text, @destination_file_textBox.Text, true);
                }
                else
                    MessageBox.Show("destination file inexistence");

                if (destination_file_textBox_two_checkBox.Checked == true)
                {
                    if (destination_file_textBox_two.TextLength != 0)
                    {
                        extension_des = Path.GetExtension(@destination_file_textBox_two.Text);//扩展名 ".aspx"

                        if (extension_src != extension_des)
                        {
                            if (ask_iscontinue() == false)
                            {
                                return;
                            }
                        }
                        File.Copy(@source_copyfile_textBox.Text, @destination_file_textBox_two.Text, true);
                    }
                }
                //else
                //    MessageBox.Show("destination file1 inexistence");

                update_state(name,destination_file_textBox.Text);
            }
            else 
               MessageBox.Show("source file inexistence");

            if (sender != null)
                copy_sync(sender, e);
        }

        private void copy_sync(object sender, EventArgs e)
        {
            if (checkBox_synccopy.Checked)
            {
                if (sender == button_copy_sourcefile)
                {
                    copy_button_Click_sync(null, e);
                }
                else if (sender == button_copy_destinationfile)
                {
                    button_copy_destinationfile_Click_sync(null, e);
                }
                else if (sender == button_copy_sourcefile_all)
                {
                    button_copy_sourcefile_all_Click_sync(null, e);
                }

                else if (sender == button_copy_sourcefile_sync)
                {
                    copy_button_Click(null, e);
                }
                else if (sender == button_copy_destinationfile_sync)
                {
                    button_copy_destinationfile_Click(null, e);
                }
                else if (sender == button_copy_sourcefile_all_sync)
                {
                    button_copy_sourcefile_all_Click(null, e);
                }
            }
        }

        public static void DeleteDir(string file)
        {

            try
            {

                //去除文件夹和子文件的只读属性
                //去除文件夹的只读属性
                System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                //去除文件的只读属性
                System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);

                //判断文件夹是否还存在
                if (Directory.Exists(file))
                {

                    foreach (string f in Directory.GetFileSystemEntries(file))
                    {

                        if (File.Exists(f))
                        {
                            //如果有子文件删除文件
                            File.Delete(f);
                            Console.WriteLine(f);
                        }
                        else
                        {
                            //循环递归删除子文件夹
                            DeleteDir(f);
                        }

                    }

                    //删除空文件夹

                    //Directory.Delete(file);  //不删除本目录

                }

            }
            catch (Exception ex) // 异常处理
            {
                MessageBox.Show(ex.Message.ToString());  // 异常信息
            }

        }

        /// <summary>

        /// 文件夹拷贝

        /// 从指定的源目录拷贝所有文件到目标目录

        /// 如果目标文件存在则跳过拷贝

        /// </summary>

        /// <param name="fml_sDst">目标目录</param>

        /// <param name="fml_sSrc">源目录</param>

        /// <returns></returns>

        public static bool Copy(string fml_sDst, string fml_sSrc)
        {

            /* 迭代拷贝文件

             * 1.将源目录存入列表

             * 2.检测当前列表第一项目录是否存在，不存在则创建

             * 3.检测当前列表第一项目录下的所有文件，并拷贝到目标文件夹下

             * 4.检测当前列表第一项目录下的文件夹，添加到列表

             * 5.移除第一项

             * 6.循环第2到第4步，直到列表项为0 */

            List<string> dirs = new List<string>();

            dirs.Add(fml_sSrc);

            while (dirs.Count != 0)
            {

                string dst_dir = dirs[0].Replace(fml_sSrc, fml_sDst);

                // 如果目标目录不存在则创建

                if (!Directory.Exists(dst_dir))
                {

                    Directory.CreateDirectory(dst_dir);

                }

                List<string> fnames = new List<string>(Directory.EnumerateFiles(dirs[0]));

                foreach (string fname in fnames)
                {

                    FileInfo fi = new FileInfo(fname); //获取文件信息

                    string dst_name = dst_dir + @"\" + fi.Name;

                    try
                    {

                        // 检测本地是否已经存在该文件，如果存在则跳过

                        if (!File.Exists(dst_name))
                        {

                            File.Copy(fname, dst_name);

                        }

                        else
                        {

                            MessageBox.Show("文件 {0} 已存在，跳过拷贝。", dst_name);

                        }

                    }

                    catch (Exception ex)
                    {

                        MessageBox.Show(ex.Message + "拷贝文件错误");

                    }

                }

                // 列举当前目录下的子目录列表

                List<string> sub_dirs = new List<string>(Directory.EnumerateDirectories(dirs[0]));

                dirs.AddRange(sub_dirs);

                dirs.RemoveAt(0);

            }

            return true;

        }

        private void copy_button_Click_sync(object sender, EventArgs e)
        {
            string extension_src = "";
            string extension_des = "";
            if (source_copyfile_textBox_sync.TextLength != 0)
            {
                string name = Path.GetFileNameWithoutExtension(source_copyfile_textBox_sync.Text);
                if (destination_file_textBox_sync.TextLength != 0)
                {
                    if (File.Exists(@source_copyfile_textBox_sync.Text) == false)
                    {
                        MessageBox.Show("源文件不存在!");
                        return;
                    }
                    extension_src = Path.GetExtension(@source_copyfile_textBox_sync.Text);//扩展名 ".aspx"
                    extension_des = Path.GetExtension(@destination_file_textBox_sync.Text);//扩展名 ".aspx"

                    if (extension_src != extension_des)
                    {
                        if (ask_iscontinue() == false)
                        {
                            return;
                        }
                    }
                    File.Copy(@source_copyfile_textBox_sync.Text, @destination_file_textBox_sync.Text, true);
                }

                else
                    MessageBox.Show("destination file inexistence");

                if (destination_file_textBox_two_sync_checkBox.Checked == true)
                {
                    if (destination_file_textBox_two_sync.TextLength != 0)
                    {
                        extension_des = Path.GetExtension(@destination_file_textBox_two_sync.Text);//扩展名 ".aspx"

                        if (extension_src != extension_des)
                        {
                            if (ask_iscontinue() == false)
                            {
                                return;
                            }
                        }
                        File.Copy(@source_copyfile_textBox_sync.Text, @destination_file_textBox_two_sync.Text, true);
                    }
                }

                update_state(name, destination_file_textBox_sync.Text);
            }
            else
                MessageBox.Show("source file inexistence");
            if (sender != null)
                copy_sync(sender, e);
        }

        private void button_copy_destinationfile_Click(object sender, EventArgs e)
        {
            string extension_src = "";
            string extension_des = "";
            if (destination_file_textBox.TextLength != 0)
            {
                string name = Path.GetFileNameWithoutExtension(source_copyfile_textBox.Text);

                if (textBox_copy_destinationfileend.TextLength != 0)
                {
                    if (File.Exists(source_copyfile_textBox.Text) == false)
                    {
                        MessageBox.Show("二级源文件不存在!");
                        return;
                    }
                    extension_src = Path.GetExtension(@destination_file_textBox.Text);//扩展名 ".aspx"
                    extension_des = Path.GetExtension(textBox_copy_destinationfileend.Text);//扩展名 ".aspx"

                    if (extension_src != extension_des)
                    {
                        if (ask_iscontinue() == false)
                        {
                            return;
                        }
                    }
                    File.Copy(@destination_file_textBox.Text, textBox_copy_destinationfileend.Text, true);
                }
                else
                    MessageBox.Show("destination file inexistence");

                update_state(name, destination_file_textBox.Text);
            }
            else
                MessageBox.Show("source file inexistence");

            if (sender != null)
                copy_sync(sender, e);
        }

        private void button_copy_destinationfile_Click_sync(object sender, EventArgs e)
        {
            string extension_src = "";
            string extension_des = "";
            if (destination_file_textBox_sync.TextLength != 0)
            {
                string name = Path.GetFileNameWithoutExtension(destination_file_textBox_sync.Text);

                if (textBox_copy_destinationfileend_sync.TextLength != 0)
                {
                        if (File.Exists(source_copyfile_textBox_sync.Text) == false)
                        {
                            MessageBox.Show("二级源文件不存在!");
                            return;
                        }
                        extension_src = Path.GetExtension(@destination_file_textBox_sync.Text);//扩展名 ".aspx"
                        extension_des = Path.GetExtension(textBox_copy_destinationfileend_sync.Text);//扩展名 ".aspx"

                        if (extension_src != extension_des)
                        {
                            if (ask_iscontinue() == false)
                            {
                                return;
                            }
                        }
                        File.Copy(@destination_file_textBox_sync.Text, textBox_copy_destinationfileend_sync.Text, true);
                }
                else
                    MessageBox.Show("destination file inexistence");

                update_state(name, destination_file_textBox_sync.Text);
            }
            else
                MessageBox.Show("source file inexistence");

            if (sender != null)
                copy_sync(sender, e);
        }

        private void button_copy_sourcefile_all_Click(object sender, EventArgs e)
        {
            copy_button_Click(sender, e);
            button_copy_destinationfile_Click(sender, e);

            if (sender != null)
                copy_sync(sender, e);
        }

        private void button_copy_sourcefile_all_Click_sync(object sender, EventArgs e)
        {
            copy_button_Click_sync(sender, e);
            button_copy_destinationfile_Click_sync(sender, e);

            if (sender != null)
                copy_sync(sender, e);
        }
        private void update_StripStatusLabel()
        {
            this.toolStripStatusLabel1.Text = "当前系统时间：" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (SC_MAX == true)
            {
                SC_MAX = false;
                pictureBoxinterface_Click(null, null);
            }
            update_StripStatusLabel();
        }
        private void source_copyfile_button_rename_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "source_file";

            //dlg.DefaultExt = ".txt";

            dlg.Filter = "All files（*.*）|*.*|All files(*.*)|*.* ";

            if (dlg.ShowDialog() == false)
                return;
            source_copyfile_textBox_rename.Text = dlg.FileName;
        }

        public void cope_to_Clipboard(string destFileName)
        {
            string[] file = new string[1];
            file[0] = destFileName;
            DataObject dataObject = new DataObject();
            dataObject.SetData(DataFormats.FileDrop, file);
            Clipboard.SetDataObject(dataObject, true);
        }
        public void cope_string_to_Clipboard(string str)
        {
            DataObject dataObject = new DataObject();
            dataObject.SetData(DataFormats.Text, str);
            Clipboard.SetDataObject(dataObject, true);
        }

        private void destination_file_button_generate_rename_Click(object sender, EventArgs e)
        {
            if (source_copyfile_textBox_rename.TextLength != 0)
            {
                string name1 = Path.GetFileNameWithoutExtension(source_copyfile_textBox_rename.Text);
                try
                {
                    string pathcopy = Path.GetDirectoryName(source_copyfile_textBox_rename.Text);  //"获取文件所在的目录："  无文件名
                    string name = Path.GetFileNameWithoutExtension(source_copyfile_textBox_rename.Text);  //"获取文件的名称没有后缀："
                    string suffix = Path.GetExtension(source_copyfile_textBox_rename.Text);//获取路径的后缀扩展名称："  后缀
                    string destFileName = "";
                    if (checkBox_delete_prefile.Checked == true)
                    {
                        if (File.Exists(destination_file_textBox_rename.Text))
                            File.Delete(destination_file_textBox_rename.Text);
                    }
                    if (comboBox7.SelectedIndex == 0)
                    {
                        name = source_copyfile_prefix_textBox_rename.Text + name + source_copyfile_suffix_textBox_rename.Text;
                        if (checkBox_systemtime_rename.Checked == true)
                        {
                            FileInfo fi = new FileInfo(source_copyfile_textBox_rename.Text);
                            name = name + fi.LastWriteTime.ToString(comboBox_timetype_rename.SelectedItem.ToString());
                        }
                        else
                        {
                            name = name + DateTime.Now.ToString(comboBox_timetype_rename.SelectedItem.ToString());
                        }
                        
                        destFileName = pathcopy + "\\" + name + suffix;
                        File.Copy(@source_copyfile_textBox_rename.Text, destFileName, true);
                        destination_file_textBox_rename.Text = destFileName;
                    }
                    else if (comboBox7.SelectedIndex == 1)
                    {
                        name = source_copyfile_prefix_textBox_rename.Text + name + source_copyfile_suffix_textBox_rename.Text;
                        destFileName = pathcopy + "\\" + name + suffix;
                        File.Copy(@source_copyfile_textBox_rename.Text, destFileName, true);
                        destination_file_textBox_rename.Text = destFileName;
                    }
                    else if (comboBox7.SelectedIndex == 2)
                    {
                        name = name.Remove(name.Length - comboBox_timetype_rename.SelectedItem.ToString().Length);
                        File.Copy(@source_copyfile_textBox_rename.Text, destFileName, true);
                        destination_file_textBox_rename.Text = destFileName;
                    }
                    else if (comboBox7.SelectedIndex == 3)
                    {
                        name = name.Remove(name.Length - comboBox_timetype_rename.SelectedItem.ToString().Length);
                        if (checkBox_systemtime_rename.Checked == true)
                        {
                            FileInfo fi = new FileInfo(source_copyfile_textBox_rename.Text);
                            name = name + fi.LastWriteTime.ToString(comboBox_timetype_rename.SelectedItem.ToString());
                        }
                        else
                        {
                            name = name + DateTime.Now.ToString(comboBox_timetype_rename.SelectedItem.ToString());
                        }
                        destFileName = pathcopy + "\\" + name + suffix;
                        File.Copy(@source_copyfile_textBox_rename.Text, destFileName, true);
                        destination_file_textBox_rename.Text = destFileName;
                    }

                    cope_to_Clipboard(destFileName);

                    if (checkBox10.Checked == true)
                    {
                        source_copyfile_textBox_sync.Text = source_copyfile_textBox_rename.Text;
                        destination_file_textBox_sync.Text = destination_file_textBox_rename.Text;
                    }
                    
                    if (checkBox_delete_srcfile.Checked == true)
                    {
                        File.Delete(source_copyfile_textBox_rename.Text);
                    }
                }
                catch
                {
                    MessageBox.Show("source file error");
                }
                update_state_rename(name1);
            }
            else
                MessageBox.Show("source file inexistence");
        }
        private void destination_file_button_copy_filename_Click(object sender, EventArgs e)
        {
            if (File.Exists(destination_file_textBox_rename.Text))
                cope_string_to_Clipboard(Path.GetFileName(destination_file_textBox_rename.Text));
        }

        private void destination_file_button_open_dir_Click(object sender, EventArgs e)
        {
            if (File.Exists(destination_file_textBox_rename.Text))
                System.Diagnostics.Process.Start("Explorer.exe", Path.GetDirectoryName(destination_file_textBox_rename.Text));
        }

        private void checkBox_synccopy_sync_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == checkBox_synccopy)
            {
                if (checkBox_synccopy.Checked)
                    checkBox_synccopy_sync.Checked = true;
                else
                    checkBox_synccopy_sync.Checked = false;
            }
            else if (sender == checkBox_synccopy_sync)
            {
                if (checkBox_synccopy_sync.Checked)
                    checkBox_synccopy.Checked = true;
                else
                    checkBox_synccopy.Checked = false;
            }
        }

        private void button_copy_sourcefile4_Click(object sender, EventArgs e)
        {
            source_copyfile_prefix_textBox_rename.Text = "";
            source_copyfile_suffix_textBox_rename.Text = "";
        }

        private void button_splitbinfile_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "source_file";

            dlg.DefaultExt = ".bin";

            dlg.Filter = "bin file (.bin)|*.bin";

            if (dlg.ShowDialog() == false)
                return;
            textBox_splitbinfile.Text = dlg.FileName;
        }

        private void button_split_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox_splitbinfile.Text))//可以上传压缩包.zip 或者jar包
            {
                try
                {
                    string savepath, str;
                    byte[] byteArray = File2Bytes(textBox_splitbinfile.Text);//文件转成byte二进制数组
                    savepath = textBox_splitbinfile.Text.Replace(".bin", string.Empty).Replace(".BIN", string.Empty);
                    string str_addr = textBox_section1.Text;
                    TextBox textbox = textBox_sectionout1;
                    int byte_cnt = 0, len = 0;
                    if (str_addr.Length == 10)
                    {
                        str = str_addr.Substring(2, 8);
                        if (comboBox_splittype.SelectedIndex == 0)
                            len = Convert.ToInt32(str, 16);
                        else if (comboBox_splittype.SelectedIndex == 1)
                        {
                            len = Convert.ToInt32(str, 16);
                            if (len <= byte_cnt)
                            {
                                MessageBox.Show("input addr error!");
                                return;
                            }
                            len = len - byte_cnt;
                        }

                        if (len != 0)
                        {
                            if ((len + byte_cnt) > byteArray.Length)
                            {
                                len = byteArray.Length - byte_cnt;
                            }
                            str = savepath + "_section1" + ".bin";
                            Bytes2File(byteArray, byte_cnt, len, str);
                            byte_cnt += len;
                            textbox.Text = str;
                            if (byte_cnt >= byteArray.Length) return;
                        }
                        else textbox.Text = "";
                    }
                    else textbox.Text = "";

                    str_addr = textBox_section2.Text;
                    textbox = textBox_sectionout2;
                    if (str_addr.Length == 10)
                    {
                        str = str_addr.Substring(2, 8);
                        if (comboBox_splittype.SelectedIndex == 0)
                            len = Convert.ToInt32(str, 16);
                        else if (comboBox_splittype.SelectedIndex == 1)
                        {
                            len = Convert.ToInt32(str, 16);
                            if (len != 0)
                            {
                                if (len <= byte_cnt)
                                {
                                    MessageBox.Show("input addr error!");
                                    return;
                                }
                                len = len - byte_cnt;
                            }
                        }
                        if (len != 0)
                        {
                            if ((len + byte_cnt) > byteArray.Length)
                            {
                                len = byteArray.Length - byte_cnt;
                            }
                            str = savepath + "_section2" + ".bin";
                            Bytes2File(byteArray, byte_cnt, len, str);
                            byte_cnt += len;
                            textbox.Text = str;
                            if (byte_cnt >= byteArray.Length) return;
                        }
                        else textbox.Text = "";
                    }
                    else textbox.Text = "";

                    str_addr = textBox_section3.Text;
                    textbox = textBox_sectionout3;
                    if (str_addr.Length == 10)
                    {
                        str = str_addr.Substring(2, 8);
                        if (comboBox_splittype.SelectedIndex == 0)
                            len = Convert.ToInt32(str, 16);
                        else if (comboBox_splittype.SelectedIndex == 1)
                        {
                            len = Convert.ToInt32(str, 16);
                            if (len != 0)
                            {
                                if (len <= byte_cnt)
                                {
                                    MessageBox.Show("input addr error!");
                                    return;
                                }
                                len = len - byte_cnt;
                            }
                        }
                        if (len != 0)
                        {
                            if ((len + byte_cnt) > byteArray.Length)
                            {
                                len = byteArray.Length - byte_cnt;
                            }
                            str = savepath + "_section3" + ".bin";
                            Bytes2File(byteArray, byte_cnt, len, str);
                            byte_cnt += len;
                            textbox.Text = str;
                            if (byte_cnt >= byteArray.Length) return;
                        }
                        else textbox.Text = "";
                    }
                    else textbox.Text = "";

                    str_addr = textBox_section4.Text;
                    textbox = textBox_sectionout4;
                    if (str_addr.Length == 10)
                    {
                        str = str_addr.Substring(2, 8);
                        if (comboBox_splittype.SelectedIndex == 0)
                            len = Convert.ToInt32(str, 16);
                        else if (comboBox_splittype.SelectedIndex == 1)
                        {
                            len = Convert.ToInt32(str, 16);
                            if (len != 0)
                            {
                                if (len <= byte_cnt)
                                {
                                    MessageBox.Show("input addr error!");
                                    return;
                                }
                                len = len - byte_cnt;
                            }
                        }
                        if (len != 0)
                        {
                            if ((len + byte_cnt) > byteArray.Length)
                            {
                                len = byteArray.Length - byte_cnt;
                            }
                            str = savepath + "_section4" + ".bin";
                            Bytes2File(byteArray, byte_cnt, len, str);
                            byte_cnt += len;
                            textbox.Text = str;
                            if (byte_cnt >= byteArray.Length) return;
                        }
                        else textbox.Text = "";
                    }
                    else textbox.Text = "";

                    str_addr = textBox_section5.Text;
                    textbox = textBox_sectionout5;
                    if (str_addr.Length == 10)
                    {
                        str = str_addr.Substring(2, 8);
                        if (comboBox_splittype.SelectedIndex == 0)
                            len = Convert.ToInt32(str, 16);
                        else if (comboBox_splittype.SelectedIndex == 1)
                        {
                            len = Convert.ToInt32(str, 16);
                            if (len != 0)
                            {
                                if (len <= byte_cnt)
                                {
                                    MessageBox.Show("input addr error!");
                                    return;
                                }
                                len = len - byte_cnt;
                            }
                        }
                        if (len != 0)
                        {
                            if ((len + byte_cnt) > byteArray.Length)
                            {
                                len = byteArray.Length - byte_cnt;
                            }
                            str = savepath + "_section5" + ".bin";
                            Bytes2File(byteArray, byte_cnt, len, str);
                            byte_cnt += len;
                            textbox.Text = str;
                            if (byte_cnt >= byteArray.Length) return;
                        }
                        else textbox.Text = "";
                    }
                    else textbox.Text = "";

                    textbox = textBox_sectionout6;
                    len = byteArray.Length - byte_cnt;
                    if (len != 0)
                    {
                        str = savepath + "_section6" + ".bin";
                        Bytes2File(byteArray, byte_cnt, len, str);
                        byte_cnt += len;
                        textbox.Text = str;
                        if (byte_cnt >= byteArray.Length) return;
                    }
                    else textbox.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("source file error!");
            }
        }

        // 将byte数组转换为文件并保存到指定地址
        // param name="buff"byte数组
        // param name="savepath"保存地址
        // int lenght 分解个数
        public static void Bytes2File(byte[] buff, byte[] buff1, string savepath)
        {
            string path;
            FileStream fs;
            BinaryWriter bw;

            byte[] buff2 = new byte[3 * 4096];
            for (int i = 0; i < buff2.Length; i++)
            {
                buff2[i] = 0xff;
            }

            savepath = savepath.Replace(".bin", string.Empty).Replace(".BIN", string.Empty);
            path = savepath + "_Combine" + ".bin";
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            fs = new FileStream(path, FileMode.CreateNew);
            bw = new BinaryWriter(fs);
            bw.Write(buff, 0, buff.Length);
            bw.Write(buff2, 0, buff2.Length);
            bw.Write(buff1, 0, buff1.Length);
            bw.Close();
            fs.Close();
        }

        public static void Bytes2File(byte[] buff, UInt32 addr, byte[] buff1, UInt32 addr1, byte[] buff2, UInt32 addr2, byte[] buff3, UInt32 addr3, byte[] buff4, UInt32 addr4, byte[] buff5, UInt32 addr5, string savepath)
        {
             FileStream fs;
            BinaryWriter bw;
            int offect=0;

            if (System.IO.File.Exists(savepath))
            {
                System.IO.File.Delete(savepath);
            }
            fs = new FileStream(savepath, FileMode.CreateNew);
            bw = new BinaryWriter(fs);
            if (buff != null)
            {
                bw.Write(buff, 0, buff.Length);
                offect = buff.Length;
                if (offect < addr1)
                {
                    byte[] copy = new byte[addr1 - offect];
                    for (int i = 0; i != copy.Length; i++)
                    {
                        copy[i] = 0xFF;
                    }
                    bw.Write(copy, 0, copy.Length);
                }
            }
            if (buff1 != null)
            {
                
                bw.Write(buff1, 0, buff1.Length);
                offect = (int)addr1+buff.Length;
                if (offect < addr2)
                {
                    byte[] copy = new byte[addr2 - offect];
                    for (int i = 0; i != copy.Length; i++)
                    {
                        copy[i] = 0xFF;
                    }
                    bw.Write(copy, 0, copy.Length);
                }
            }
            if (buff2 != null)
            {
                bw.Write(buff2, 0, buff2.Length);
                offect = (int)addr2 + buff.Length;
                if (offect < addr3)
                {
                    byte[] copy = new byte[addr3 - offect];
                    for (int i = 0; i != copy.Length; i++)
                    {
                        copy[i] = 0xFF;
                    }
                    bw.Write(copy, 0, copy.Length);
                }
            }
            if (buff3 != null)
            {
                bw.Write(buff3, 0, buff3.Length);
                offect = (int)addr3 + buff.Length;
                if (offect < addr4)
                {
                    byte[] copy = new byte[addr4 - offect];
                    for (int i = 0; i != copy.Length; i++)
                    {
                        copy[i] = 0xFF;
                    }
                    bw.Write(copy, 0, copy.Length);
                }
            }
            if (buff4 != null)
            {
                bw.Write(buff4, 0, buff4.Length);
                offect = (int)addr4 + buff.Length;
                if (offect < addr5)
                {
                    byte[] copy = new byte[addr5 - offect];
                    for (int i = 0; i != copy.Length; i++)
                    {
                        copy[i] = 0xFF;
                    }
                    bw.Write(copy, 0, copy.Length);
                }
            }
            if (buff5 != null)
            {
                bw.Write(buff5, 0, buff5.Length);
            }
            bw.Close();
            fs.Close();
        }

        // 将byte数组转换为文件并保存到指定地址
        // param name="buff"byte数组
        // param name="savepath"保存地址
        // int lenght 分解个数
        public static void Bytes2File(byte[] buff, int index, int count, string savepath)
        {
            FileStream fs;
            BinaryWriter bw;

            if (System.IO.File.Exists(savepath))
            {
                System.IO.File.Delete(savepath);
            }
            fs = new FileStream(savepath, FileMode.CreateNew);
            bw = new BinaryWriter(fs);
            bw.Write(buff, index, count);
            bw.Close();
            fs.Close();
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="sFullName"></param>
        /// <returns></returns>
        public static long GetFileSize(string sFullName)
        {
            long lSize = 0;
            if (File.Exists(sFullName))
                lSize = new FileInfo(sFullName).Length;
            return lSize;
        }

        private void button_combinopen_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "source_file";

            dlg.DefaultExt = ".bin";

            dlg.Filter = "bin file (.bin)|*.bin";

            if (dlg.ShowDialog() == false)
                return;

            if ((Button)(sender) == button_sectionintput1)
            {
                textBox_sectionintput1.Text = dlg.FileName;
                if (checkBox7.Checked == true)
                {
                    textBox_combin_section1.Text = "0x00000000";
                }
            }
            else if ((Button)(sender) == button_sectionintput2)
            {
                if (!(File.Exists(textBox_sectionintput1.Text)))
                {
                    MessageBox.Show("之前段落文件无效!");
                    return;
                }
                textBox_sectionintput2.Text = dlg.FileName;

                long size = GetFileSize(textBox_sectionintput1.Text);
                if (comboBox6.SelectedIndex == 0)
                {
                    textBox_combin_section2.Text = "0x"+size.ToString("x8");
                }
                else if (comboBox6.SelectedIndex == 1)
                {
                    long addr = Convert.ToInt32(textBox_combin_section1.Text, 16);
                    if (size%4096!=0)
                    {
                        size = (size / 4096 + 1) * 4096;
                    }
                    textBox_combin_section2.Text = "0x" + (size+addr).ToString("x8");
                }
                else if (comboBox6.SelectedIndex == 2)
                {
                    long addr = Convert.ToInt32(textBox_combin_section2.Text, 16);
                    if (addr < size)
                    {
                        MessageBox.Show("输入数据有误!");
                        return;
                    }
                }
            }
            else if ((Button)(sender) == button_sectionintput3)
            {
                if ((!(File.Exists(textBox_sectionintput1.Text))) || (!(File.Exists(textBox_sectionintput2.Text))))
                {
                    MessageBox.Show("之前段落文件无效!");
                    return;
                }
                textBox_sectionintput3.Text = dlg.FileName;

                long size = GetFileSize(textBox_sectionintput2.Text);  //上个文件的大小
                if (comboBox6.SelectedIndex == 0)
                {
                    size += Convert.ToInt32(textBox_combin_section2.Text, 16);
                    textBox_combin_section3.Text = "0x" + size.ToString("x8");
                }
                else if (comboBox6.SelectedIndex == 1)
                {
                    long addr = Convert.ToInt32(textBox_combin_section2.Text, 16);
                    if (size % 4096 != 0)
                    {
                        size = (size / 4096 + 1) * 4096;
                    }
                    textBox_combin_section3.Text = "0x" + (size + addr).ToString("x8");
                }
                else if (comboBox6.SelectedIndex == 2)
                {
                    size += Convert.ToInt32(textBox_combin_section2.Text, 16);
                    long addr = Convert.ToInt32(textBox_combin_section3.Text, 16);
                    if (addr < size)
                    {
                        MessageBox.Show("输入数据有误!");
                        return;
                    }
                }
            }
            else if ((Button)(sender) == button_sectionintput4)
            {
                if ((!(File.Exists(textBox_sectionintput1.Text))) || (!(File.Exists(textBox_sectionintput2.Text))) || (!(File.Exists(textBox_sectionintput3.Text))))
                {
                    MessageBox.Show("之前段落文件无效!");
                    return;
                }
                textBox_sectionintput4.Text = dlg.FileName;

                long size = GetFileSize(textBox_sectionintput3.Text);  //上个文件的大小
                if (comboBox6.SelectedIndex == 0)
                {
                    size += Convert.ToInt32(textBox_combin_section3.Text, 16);
                    textBox_combin_section4.Text = "0x" + size.ToString("x8");
                }
                else if (comboBox6.SelectedIndex == 1)
                {
                    long addr = Convert.ToInt32(textBox_combin_section3.Text, 16);
                    if (size % 4096 != 0)
                    {
                        size = (size / 4096 + 1) * 4096;
                    }
                    textBox_combin_section4.Text = "0x" + (size + addr).ToString("x8");
                }
                else if (comboBox6.SelectedIndex == 2)
                {
                    size += Convert.ToInt32(textBox_combin_section3.Text, 16);
                    long addr = Convert.ToInt32(textBox_combin_section4.Text, 16);
                    if (addr < size)
                    {
                        MessageBox.Show("输入数据有误!");
                        return;
                    }
                }
            }
            else if ((Button)(sender) == button_sectionintput5)
            {
                if ((!(File.Exists(textBox_sectionintput1.Text))) || (!(File.Exists(textBox_sectionintput2.Text))) || (!(File.Exists(textBox_sectionintput3.Text))) || (!(File.Exists(textBox_sectionintput4.Text))))
                {
                    MessageBox.Show("之前段落文件无效!");
                    return;
                }
                textBox_sectionintput5.Text = dlg.FileName;

                long size = GetFileSize(textBox_sectionintput4.Text);  //上个文件的大小
                if (comboBox6.SelectedIndex == 0)
                {
                    size += Convert.ToInt32(textBox_combin_section4.Text, 16);
                    textBox_combin_section5.Text = "0x" + size.ToString("x8");
                }
                else if (comboBox6.SelectedIndex == 1)
                {
                    long addr = Convert.ToInt32(textBox_combin_section4.Text, 16);
                    if (size % 4096 != 0)
                    {
                        size = (size / 4096 + 1) * 4096;
                    }
                    textBox_combin_section5.Text = "0x" + (size + addr).ToString("x8");
                }
                else if (comboBox6.SelectedIndex == 2)
                {
                    size += Convert.ToInt32(textBox_combin_section4.Text, 16);
                    long addr = Convert.ToInt32(textBox_combin_section5.Text, 16);
                    if (addr < size)
                    {
                        MessageBox.Show("输入数据有误!");
                        return;
                    }
                }
            }
            else if ((Button)(sender) == button_sectionintput6)
            {
                if ((!(File.Exists(textBox_sectionintput1.Text))) || (!(File.Exists(textBox_sectionintput2.Text))) || (!(File.Exists(textBox_sectionintput3.Text))) || (!(File.Exists(textBox_sectionintput4.Text))) || (!(File.Exists(textBox_sectionintput5.Text))))
                {
                    MessageBox.Show("之前段落文件无效!");
                    return;
                }
                textBox_sectionintput6.Text = dlg.FileName;

                long size = GetFileSize(textBox_sectionintput5.Text);  //上个文件的大小
                if (comboBox6.SelectedIndex == 0)
                {
                    size += Convert.ToInt32(textBox_combin_section5.Text, 16);
                    textBox_combin_section6.Text = "0x" + size.ToString("x8");
                }
                else if (comboBox6.SelectedIndex == 1)
                {
                    long addr = Convert.ToInt32(textBox_combin_section5.Text, 16);
                    if (size % 4096 != 0)
                    {
                        size = (size / 4096 + 1) * 4096;
                    }
                    textBox_combin_section6.Text = "0x" + (size + addr).ToString("x8");
                }
                else if (comboBox6.SelectedIndex == 2)
                {
                    size += Convert.ToInt32(textBox_combin_section5.Text, 16);
                    long addr = Convert.ToInt32(textBox_combin_section6.Text, 16);
                    if (addr < size)
                    {
                        MessageBox.Show("输入数据有误!");
                        return;
                    }
                }
            }
        }

        private void button_combine_Click(object sender, EventArgs e)
        {
            byte[] byteArray1 = null, byteArray2 = null, byteArray3 = null, byteArray4 = null, byteArray5 = null, byteArray6 = null;
            if (textBox_sectionintput1.Text.Length != 0) byteArray1 = File2Bytes(textBox_sectionintput1.Text);//文件转成byte二进制数组
            if (textBox_sectionintput2.Text.Length != 0) byteArray2 = File2Bytes(textBox_sectionintput2.Text);
            if (textBox_sectionintput3.Text.Length != 0) byteArray3 = File2Bytes(textBox_sectionintput3.Text);
            if (textBox_sectionintput4.Text.Length != 0) byteArray4 = File2Bytes(textBox_sectionintput4.Text);
            if (textBox_sectionintput5.Text.Length != 0) byteArray5 = File2Bytes(textBox_sectionintput5.Text);
            if (textBox_sectionintput6.Text.Length != 0) byteArray6 = File2Bytes(textBox_sectionintput6.Text);
            string savepath = Path.GetDirectoryName(textBox_sectionintput1.Text); ;
            savepath = savepath + "\\BIN_Combin" + ".bin";
            Bytes2File(byteArray1, Convert.ToUInt32(textBox_combin_section1.Text, 16), byteArray2, Convert.ToUInt32(textBox_combin_section2.Text, 16), byteArray3, Convert.ToUInt32(textBox_combin_section3.Text, 16), byteArray4, Convert.ToUInt32(textBox_combin_section4.Text, 16), byteArray5, Convert.ToUInt32(textBox_combin_section5.Text, 16), byteArray6, Convert.ToUInt32(textBox_combin_section6.Text, 16), savepath);
        }

        private void button_copy_sourcefile5_Click(object sender, EventArgs e)
        {
            textBox_section1.Text = "0x00000000";
            textBox_section2.Text = "0x00000000";
            textBox_section3.Text = "0x00000000";
            textBox_section4.Text = "0x00000000";
            textBox_section5.Text = "0x00000000";
        }

        private void button19_Click(object sender, EventArgs e)
        {
            textBox_combin_section1.Text = "0x00000000";
            textBox_combin_section2.Text = "0x00000000";
            textBox_combin_section3.Text = "0x00000000";
            textBox_combin_section4.Text = "0x00000000";
            textBox_combin_section5.Text = "0x00000000";
            textBox_combin_section6.Text = "0x00000000";
        }
        private void source_copyfile_textBox_sync_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(source_copyfile_textBox_sync.Text))
            {
            }
            else
            {
                byte[] byteArray = File2Bytes(source_copyfile_textBox_sync.Text);//文件转成byte二进制数组
                source_copyfile_textBox_sync_size.Text = byteArray.Length.ToString() + "(0x" + byteArray.Length.ToString("X") + ")";
                source_copyfile_textBox_sync_crc32.Text = "0x" + crc32_fun(byteArray, (uint)byteArray.Length).ToString("X");
                UInt32 Checksum = 0;
                for (int i = 0; i < byteArray.Length; i++)
                {
                    Checksum += byteArray[i];
                }
                source_copyfile_textBox_sync_checksum.Text = "0x" + Checksum.ToString("X");
            }
        }
        private int filename_index_get(string filename, ref string outfilenames)
        {
            string index = "";
            int m = filename.LastIndexOf(")");
            int n = filename.LastIndexOf("(");
            if ((m != -1) && (n != -1))
            {
                if (m > n)
                {
                    index = filename.Substring(n+1, m - n-1);
                    outfilenames = filename.Substring(0, n);
                }
            }
            if (index == "")
            {
                return -1;
            }
            else
            {
                return Convert.ToInt32(index, 16);
            }
        }

        private void clear_copyfile_button_Click(object sender, EventArgs e)
        {
            source_copyfile_textBox.Text = "";
            destination_file_textBox.Text = "";
            destination_file_textBox_two.Text = "";
            textBox_copy_destinationfileend.Text = "";
        }

        private void clear_synccopyfile_button_Click(object sender, EventArgs e)
        {
            source_copyfile_textBox_sync.Text = "";
            destination_file_textBox_sync.Text = "";
            destination_file_textBox_two_sync.Text = "";
            textBox_copy_destinationfileend_sync.Text = "";
        }
        private void button33_Click(object sender, EventArgs e)
        {
            Process process1 = null;
            process1=Process.Start(@".\\SYDTEK Tools.exe");
            //this.Hide();    //隐藏窗口
            //process1.WaitForExit();
            //this.Show();//显示当前窗口
        }
        private void button35_Click(object sender, EventArgs e)
        {
            Process process1 = null;
            process1 = Process.Start(@".\\Animated GIF Producer V4.0.exe");
            //this.Hide();    //隐藏窗口
            //process1.WaitForExit();
            //this.Show();//显示当前窗口
        }
        private void button36_Click(object sender, EventArgs e)
        {
            //Process process1 = null;
            System.Diagnostics.Process.Start(@".\\Audacity.lnk");
            //this.Hide();    //隐藏窗口
            //process1.WaitForExit();
            //this.Show();//显示当前窗口
        }
        private void button37_Click(object sender, EventArgs e)
        {
            Process process1 = null;
            process1 = Process.Start(@".\\批量修改文件名.exe");
            //this.Hide();    //隐藏窗口
            //process1.WaitForExit();
            //this.Show();//显示当前窗口
        }
        private void button40_Click(object sender, EventArgs e)
        {
            Process process1 = null;
            process1 = Process.Start(@".\\LED段位码.exe");
            //this.Hide();    //隐藏窗口
            //process1.WaitForExit();
            //this.Show();//显示当前窗口
        }
        private void button43_Click(object sender, EventArgs e)
        {
            //建立新的系统进程    
            System.Diagnostics.Process process = new System.Diagnostics.Process();

            //设置图片的真实路径和文件名    
            process.StartInfo.FileName = Environment.CurrentDirectory + "//ascii-Table.jpg";//获取和设置当前目录（即该进程从中启动的目录）的完全限定路径。

            //设置进程运行参数，这里以最大化窗口方法显示图片。    
            process.StartInfo.Arguments = "rundl132.exe C://WINDOWS//system32//shimgvw.dll,ImageView_Fullscreen";

            //此项为是否使用Shell执行程序，因系统默认为true，此项也可不设，但若设置必须为true    
            process.StartInfo.UseShellExecute = true;

            //此处可以更改进程所打开窗体的显示样式，可以不设    
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.Start();
            //process.Close();
        }
    }
}
