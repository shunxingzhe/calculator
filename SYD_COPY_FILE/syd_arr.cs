using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SYD_COPY_FILE
{
    public partial class Form1
    {

        #region define 

        private static UInt32 UTF8_CODE_BASS= 0X4E00;
        private static UInt32 UTF8_CODE_END= 0X9FA5;
       
        private static UInt32 UTF8_SYMBLE1_BASS= 0X2010;
        private static UInt32 UTF8_SYMBLE1_END= 0X201F;
        private static UInt32 UTF8_SYMBLE1_ADDR= ((UTF8_CODE_END-UTF8_CODE_BASS+1)*32);  //((0X9FA5-0X4E00+1)*20)
       
        private static UInt32 UTF8_SYMBLE2_BASS= 0X3001;
        private static UInt32 UTF8_SYMBLE2_END= 0X301F;
        private static UInt32 UTF8_SYMBLE2_ADDR= (UTF8_SYMBLE1_ADDR+(UTF8_SYMBLE1_END-UTF8_SYMBLE1_BASS+1)*32);  //((0X9FA5-0X4E00+1)*20+(0X301F-0X3001+1)*20)
       
        private static UInt32 UTF8_SYMBLE3_BASS= 0XFF01;
        private static UInt32 UTF8_SYMBLE3_END= 0XFF0F;
        private static UInt32 UTF8_SYMBLE3_ADDR = (UTF8_SYMBLE2_ADDR + (UTF8_SYMBLE2_END - UTF8_SYMBLE2_BASS + 1) * 32);  //((0X9FA5-0X4E00+1)*20+(0X301F-0X3001+1)*20+(0XFF0F-0XFF01+1)*20)

        private static UInt32 UTF8_ASCII_BASS = 0X20;
        private static UInt32 UTF8_ASCII_END = 0X7E;
        private static UInt32 UTF8_ASCII_ADDR = (UTF8_SYMBLE3_ADDR + (UTF8_SYMBLE3_END - UTF8_SYMBLE3_BASS + 1) * 32);


        private static UInt32 UTF8_MASK_32X32_ADDR = 0;

        private static UInt32 UTF8_SYMBLE1_32X32_ADDR = (UTF8_MASK_32X32_ADDR + (UTF8_CODE_END - UTF8_CODE_BASS + 1) * 128);

        private static UInt32 UTF8_SYMBLE2_32X32_ADDR = (UTF8_SYMBLE1_32X32_ADDR + (UTF8_SYMBLE1_END - UTF8_SYMBLE1_BASS + 1) * 128);

        private static UInt32 UTF8_SYMBLE3_32X32_ADDR = (UTF8_SYMBLE2_32X32_ADDR + (UTF8_SYMBLE2_END - UTF8_SYMBLE2_BASS + 1) * 128);

        private static UInt32 UTF8_ASCII_32X32_ADDR = (UTF8_SYMBLE3_32X32_ADDR + (UTF8_SYMBLE3_END - UTF8_SYMBLE3_BASS + 1) * 128);

        ToolTip p_draw;

        List<string> comboBox_indicate_text = new List<string>();
        #endregion

        public void syd_arr_init()
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "syd_arr_ok.txt";
            label_outfilename.Text = path;

            comboBox_mode.SelectedIndex = Settings1.Default.arr_fun_sel;
            if (Settings1.Default.arr_font_type >= 2) Settings1.Default.arr_font_type = 0;
            
            source_file_textBox.Text = Settings1.Default.arr_source_file_text;
            textBox_key.Text = Settings1.Default.textBox_key;

            p_draw = new ToolTip();
            comboBox_mode_DropDownClosed(null, null);

            comboBox_datatype.SelectedIndex=Settings1.Default.arr_data_type;
            comboBox_fonttype.SelectedIndex = Settings1.Default.arr_font_type;
        }

        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        public byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public byte[] strToToDecByte(string DecString)
        {
            DecString = DecString.Replace(" ", "");
            if ((DecString.Length % 2) != 0)
                DecString += " ";
            byte[] returnBytes = new byte[DecString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(DecString.Substring(i * 2, 2));
            return returnBytes;
        }

        private void AdjustComboBoxDropDownListWidth(ComboBox comboBox)
        {
            // 测量出最大的字符大小
            int maxSize = 0;
            System.Drawing.Graphics g = CreateGraphics();
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                comboBox.SelectedIndex = i;
                SizeF size = g.MeasureString(comboBox.Text, comboBox.Font);
                if (maxSize < (int)size.Width)
                {
                    maxSize = (int)size.Width;
                }
            }
            comboBox.DropDownWidth = comboBox.Width;
            if (comboBox.DropDownWidth < maxSize)
            {
                comboBox.DropDownWidth = maxSize;
            }
        }

        private void source_file_button_Click(object sender, EventArgs e)
        {
            if ((comboBox_mode.SelectedIndex == 20) || (comboBox_mode.SelectedIndex == 21))
            {
                //FolderBrowserDialog path = new FolderBrowserDialog();

                //if (path.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                //    source_file_textBox.Text = path.SelectedPath;
                //else
                //    return;
                MyFolderBrowserDialog folderBrowserDialog1 = new MyFolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    source_file_textBox.Text = folderBrowserDialog1.DirectoryPath;
                    label_outfilename.Text = source_file_textBox.Text + "_ok\\";
                }
                else
                    return;
            }
            else
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                dlg.FileName = "source_file";

                dlg.DefaultExt = ".txt";

                dlg.Filter = "txt file (.txt)|*.txt|wav file (.wav)|*.wav|bin file (.bin)|*.bin|bmp file (.bmp)|*.bmp|c file (.c)|*.c|csv file (.csv)|*.csv|log file (.log)|*.log|diary file (.diary)|*.diary";

                if (dlg.ShowDialog() == false)
                    return;
                source_file_textBox.Text = dlg.FileName;

                if (comboBox_mode.SelectedIndex != 3)
                {
                    reintput_file(source_file_textBox.Text);
                }
                
                label_intputsize.Text = (textInput.Text.Length / 2).ToString();
            }
        }
        //自定义一个类
        public class FileTimeInfo
        {
            public string FileName;  //文件名
            public DateTime FileCreateTime; //创建时间
        }
        //获取最近创建的文件名和创建时间
        //如果没有指定类型的文件，返回null
        static FileTimeInfo GetLatestFileTimeInfo(string dir, string ext)
        {
            List<FileTimeInfo> list = new List<FileTimeInfo>();
            DirectoryInfo d = new DirectoryInfo(dir);
            foreach (FileInfo fi in d.GetFiles())
            {
                if (fi.Extension.ToUpper() == ext.ToUpper())
                {
                    list.Add(new FileTimeInfo()
                    {
                        FileName = fi.FullName,
                        FileCreateTime = fi.CreationTime
                    });
                }
            }
            var qry = from x in list
                      orderby x.FileCreateTime
                      select x;
            return qry.LastOrDefault();
        }
        private void reopen_source_file_button_Click(object sender, EventArgs e)
        {
            string filepath = source_file_textBox.Text;//等到的完整的文件名
            if (System.IO.File.Exists(filepath) == false)  //如果存在返回值为true，如果不存在这个文件，则返回值为false
            {
                MessageBox.Show("源文件设置出错!");
                return;
            }
            FileTimeInfo fti =GetLatestFileTimeInfo(System.IO.Path.GetDirectoryName(filepath), System.IO.Path.GetExtension(filepath));
            if (fti.FileName == "")
            {
                MessageBox.Show("输入路径没有该类文件!");
            }
            source_file_textBox.Text = fti.FileName;
            reintput_file(source_file_textBox.Text);
            label_intputsize.Text = (textInput.Text.Length / 2).ToString();
        }
        private void source_file_textBox_DragEnter(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;
            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void source_file_textBox_DragDrop(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;

            if (dataObject == null) return;

            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);

                foreach (string file in files)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    if (sender == source_file_textBox)
                        this.source_file_textBox.Text = fi.FullName;
                }
            }
        }

       private string HoverTreeClearMark(string input)
        {
            input = Regex.Replace(input, @"/\*[\s\S]*?\*/", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"^\s*$\n", "", RegexOptions.Multiline);  //有点多
            input = Regex.Replace(input, @"//[^\n]*", "", RegexOptions.Multiline);
            return input;
        }

       /// <summary>读取文件，返回一个含有文件数据的行列表</summary>
       /// <param name="TxtFilePath">文件路径</param>
       /// <returns>文件数据的行列表</returns>
       private List<string> ReadTxtFromFile(string TxtFilePath)
       {
           // 1 首先创建一个泛型为string 的List列表
           List<string> AllRowStrList = new List<string>();
           {
               // 2 加载文件
               System.IO.StreamReader sr = new
               System.IO.StreamReader(TxtFilePath,Encoding.Default);
               String line; // 3 调用StreamReader的ReadLine()函数
               while ((line = sr.ReadLine()) != null)
               {   // 4 将每行添加到文件List中
                   AllRowStrList.Add(line);
               }
               // 5 关闭流
               sr.Close();
           }
           // 6 完成操作
           return AllRowStrList;
       }
        public void Director(string dir, List<string> list,string ext)
        {
            DirectoryInfo d = new DirectoryInfo(dir);
            FileInfo[] files = d.GetFiles();//文件
            DirectoryInfo[] directs = d.GetDirectories();//文件夹
            foreach (FileInfo f in files)
            {
                if (f.Extension.ToLower() == ext)
                {
                    list.Add(f.FullName);//添加文件名到列表中  
                }
            }
            //获取子文件夹内的文件列表，递归遍历  
            foreach (DirectoryInfo dd in directs)
            {
                Director(dd.FullName, list, ext);
            }
        }
        //遍历所有文件夹获取特定格式的文件
        private List<string> fine_filename(string FolderPath, string ext)
        {
            DirectoryInfo theFolder = new DirectoryInfo(FolderPath);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            List<string> file = new List<string>();
            Director(FolderPath, file, ext);
            ////遍历文件夹
            //foreach (DirectoryInfo NextFolder in dirInfo)
            //{
            //    FileInfo[] fileInfo = NextFolder.GetFiles();
            //    foreach (FileInfo NextFile in fileInfo)  //遍历文件
            //    {
            //        if(NextFile.Extension.ToLower()== ".c")
            //        {
            //            file.Add(NextFile.Name);
            //        }
            //    }
            //}
            return file;
        }

        private void bintoarr()
       {
           int i = 0, data_residue = 0;
           //string orgTxt1 = HoverTreeClearMark(textInput.Text.Trim());
           string orgTxt1 = textInput.Text.Trim();

           orgTxt1 = orgTxt1.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("0X", "").Replace("0x", "").Replace(",", "").Replace("\r\n", "");
           //List<string> lstArray = orgTxt1.Split(new char[1] { ';' }).ToList();
           List<string> lstArray = new List<string>();

            if ((comboBox_fonttype.SelectedIndex == 1) || (comboBox_fonttype.SelectedIndex == 2))
            {
                orgTxt1=orgTxt1.Remove(0, 0x28*2);
            }

            data_residue = orgTxt1.Length % 32;

           //将字符串分割为长度为4的字符数组
           for (i = 0; i < (orgTxt1.Length / 32); i = i + 1)
           {
               try
               {
                   lstArray.Add(orgTxt1.Substring(32 * i, 32)); //i-起始位置，4-子串长度
               }
               catch (Exception e1)
               {
                   MessageBox.Show(e1.ToString(), "异常提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                   continue;
               }
           }
           if (data_residue != 0)
           {
               lstArray.Add(orgTxt1.Substring(32 * i, orgTxt1.Length % 32)); //i-起始位置，4-子串长度
           }

           UInt16 ii = 0;

           string str = "", str1 = "";
           for (i = 0; i < lstArray.Count; i++)
           {
               str = lstArray[i];
               try
               {
                   if ((i == lstArray.Count - 1) & (data_residue != 0))  //最后一个
                   {
                       for (ii = 0; ii < data_residue; ii++)
                       {
                           if (ii == 0) str = str.Insert(0, "0x");
                           else if (ii == 1) str = str.Insert(4, ",0x");
                           else if ((ii > 1) & (ii < (data_residue / 2 - 1))) str = str.Insert((ii - 2) * 5 + 9, ",0x");
                           else if (ii == (data_residue / 2 - 1))
                           {
                               str = str.Insert((ii - 2) * 5 + 9, ",0x");
                               str = str.Insert((ii - 1) * 5 + 9, ",\r\n");
                           }
                       }
                   }
                   else
                   {
                       for (ii = 0; ii < 16; ii++)
                       {
                           if (ii == 0) str = str.Insert(0, "0x");
                           else if (ii == 1) str = str.Insert(4, ",0x");
                           else if ((ii > 1) & (ii < 15)) str = str.Insert((ii - 2) * 5 + 9, ",0x");
                           else if (ii == 15)
                           {
                               str = str.Insert((ii - 2) * 5 + 9, ",0x");
                               str = str.Insert((ii - 1) * 5 + 9, ",\r\n");
                           }
                       }
                   }
               }
               catch
               {
                   MessageBox.Show("出错位置第" + (i + 1).ToString() + "个数组");
                   Application.Exit();
               }
               lstArray[i] = str;
           }
           if (comboBox_datatype.SelectedIndex == 1)
           {
               for (i = 0; i < lstArray.Count; i++)
               {
                   str = lstArray[i];
                   str1 = "";
                   if ((i == lstArray.Count - 1) & (data_residue != 0))  //最后一个
                   {
                       for (ii = 0; ii < (data_residue / 4); ii++)
                       {
                           str1 += str.Substring(ii * 10 + 5, 4) + str.Substring(ii * 10 + 2, 3);
                       }
                       if (((data_residue / 2) % 2) != 0)
                       {
                           str1 += "0x00" + str.Substring(ii * 10 + 2, 3);
                       }
                   }
                   else
                   {
                       for (ii = 0; ii < 8; ii++)
                       {
                           str1 += str.Substring(ii * 10 + 5, 4) + str.Substring(ii * 10 + 2, 3);
                       }
                   }
                   if ((i % 2) != 0) str1 += "\r\n";
                   lstArray[i] = str1;
               }
           }
           else if (comboBox_datatype.SelectedIndex == 2)
           {
               for (i = 0; i < lstArray.Count; i++)
               {
                   str = lstArray[i];
                   str1 = "";
                   if ((i == lstArray.Count - 1) & (data_residue != 0))  //最后一个
                   {
                       for (ii = 0; ii < (data_residue / 8); ii++)
                       {
                           str1 += str.Substring(ii * 20 + 15, 4) + str.Substring(ii * 20 + 12, 2) + str.Substring(ii * 20 + 7, 2) + str.Substring(ii * 20 + 2, 3);
                       }
                       if (((data_residue / 2) % 4) == 1)
                       {
                           str1 += "0x000000" + str.Substring(ii * 20 + 2, 3);
                       }
                       else if (((data_residue / 2) % 4) == 2)
                       {
                           str1 += "0x0000" + str.Substring(ii * 20 + 7, 2) + str.Substring(ii * 20 + 2, 3);
                       }
                       else if (((data_residue / 2) % 4) == 3)
                       {
                           str1 += "0x00" + str.Substring(ii * 20 + 12, 2) + str.Substring(ii * 20 + 7, 2) + str.Substring(ii * 20 + 2, 3);
                       }
                   }
                   else
                   {
                       for (ii = 0; ii < 4; ii++)
                       {
                           str1 += str.Substring(ii * 20 + 15, 4) + str.Substring(ii * 20 + 12, 2) + str.Substring(ii * 20 + 7, 2) + str.Substring(ii * 20 + 2, 3);
                       }
                   }
                   if ((i % 2) != 0) str1 += "\r\n";
                   lstArray[i] = str1;
               }
           }

           int extract_len = lstArray.Count;
           if(textBox_filesize.Text.Length != 0)  //不是提取WAV的特殊操作
           {
               extract_len = Convert.ToInt32(textBox_filesize.Text) / 16;
               if ((extract_len == 0) | (extract_len > lstArray.Count)) extract_len = lstArray.Count;
           }

           string path = label_outfilename.Text;
           using (FileStream fsWrite = new FileStream(path, FileMode.Create, FileAccess.Write))
           {
               byte[] buffer = null;
               for (i = 0; i < extract_len; i++)
               {
                   buffer = Encoding.Default.GetBytes(lstArray[i]);
                   fsWrite.Write(buffer, 0, buffer.Length);
               }
           }

           if (extract_len != lstArray.Count)
           {
               richTextBox_out.Text = System.IO.File.ReadAllText(path);
           }
           else
           {
               richTextBox_out.Text = string.Join("", lstArray.ToArray());
           }

            if (comboBox_fonttype.SelectedIndex == 2)
            {
                string filePath = comboBox_indicate.Text.Trim().Replace("\"","");
                string array_name = textBox_key.Text.Trim();
                if (!File.Exists(filePath))
                {
                    MessageBox.Show(filePath + " not exists!");
                    return;
                }
                else
                {
                    int j = 0, k = 0;
                    str = System.IO.File.ReadAllText(filePath, Encoding.Default);
                    j = str.IndexOf(array_name, 0);
                    if (j == -1)
                    {
                        MessageBox.Show("函数名输入错误");
                        return;
                    }
                    j = str.IndexOf("{", j);
                    k = str.IndexOf("};", j);
                    str = str.Remove(j + 1, k-j - 1);
                    str = str.Insert(j+1, "\n"+richTextBox_out.Text);
                    File.WriteAllText(filePath, str);
                }
            }

            MessageBox.Show("保存成功!");
       }

       private void RGB_565()
       {
           int i = 0, data_residue = 0;
           string orgTxt1 = textInput.Text.Trim();

           orgTxt1 = orgTxt1.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("0X", "").Replace("0x", "").Replace(",", "").Replace("\r\n", "");
           List<string> lstArray = new List<string>();

           data_residue = orgTxt1.Length % 6;

           //将字符串分割为长度为4的字符数组
           for (i = 0; i < (orgTxt1.Length / 6); i = i + 1)
           {
               try
               {
                   lstArray.Add(orgTxt1.Substring(6 * i, 6)); //i-起始位置，4-子串长度
               }
               catch (Exception e1)
               {
                   MessageBox.Show(e1.ToString(), "异常提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                   continue;
               }
           }
           if (data_residue != 0)
           {
               lstArray.Add(orgTxt1.Substring(6 * i, orgTxt1.Length % 6)); //i-起始位置，4-子串长度
           }

           string str = "";

           Int32[] RGB_List = new Int32[lstArray.Count];

           Int32 temp = 0;

           for (i = 0; i < lstArray.Count; i++)
           {
               str = lstArray[i];
               try
               {
                   temp = Convert.ToInt32(lstArray[i],16);
                   Byte a, b, c;
                   a = (Byte)(temp >> 19);
                   b = (Byte)(temp >> 10);
                   c = (Byte)(temp >> 3);
                   temp = (a << 11) | (b << 5) | c;
                   RGB_List[i] = temp;
               }
               catch
               {
                   MessageBox.Show("出错位置第" + (i + 1).ToString() + "个数组");
                   Application.Exit();
               }
               lstArray[i] = str;
           }
           int extract_len = lstArray.Count;

           string path = label_outfilename.Text;
           path = path.Replace(".bin", string.Empty).Replace(".BIN", string.Empty);
           path = path + ".txt";
           using (FileStream fsWrite = new FileStream(path, FileMode.Create, FileAccess.Write))
           {
               byte[] buffer = null;
               for (i = 0; i < lstArray.Count/8; i++)
               {
                   str = "";
                   for (int j = 0; j < 8; j++)
                   {
                       str = str + "0x" + ((byte)RGB_List[i * 8 + j]).ToString("X2") + ",0x" + (RGB_List[i * 8 + j] >> 8).ToString("X2") + ",";
                   }

                   buffer = Encoding.Default.GetBytes(str + "\r");

                   fsWrite.Write(buffer, 0, buffer.Length);
               }
           }

           richTextBox_out.Text = System.IO.File.ReadAllText(path);

           MessageBox.Show("保存成功!");
       }

        private void Git_helper()
        {
            string orgTxt1 = "";
            if (comboBox_fonttype.SelectedIndex == 0)
            {
                orgTxt1 = textInput.Text.Trim();
                orgTxt1 = orgTxt1.Replace("\"","'").Replace("!", ",");
                richTextBox_out.Text = "git commit -m \"" + orgTxt1+ "\"";
            }
            if (comboBox_fonttype.SelectedIndex == 1)
            {
                orgTxt1 = textInput.Text;
                richTextBox_out.Text = orgTxt1.Replace("\r\n", "  \r\n") ;
            }
        }
       private void fonttxt_to_bin()
       {
           int i = 0, j = 0, k = 0;
           string orgTxt1 = textInput.Text.Trim();
           orgTxt1 = orgTxt1.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("0X", "").Replace("0x", "").Replace(",", "").Replace("\r\n", "");

           List<string> lstArray = orgTxt1.Split(new string[] { "/*", "*/" }, StringSplitOptions.RemoveEmptyEntries).ToList();

           string str = "";
           long bin_len = 0;

           for (i = 0,j=0,k=0; i < lstArray.Count / 2; i++)
           {
               bin_len = bin_len +lstArray[i * 2].Length/2;

               str = lstArray[i*2+1];

               j = str.IndexOf("\"",0);
               k = str.IndexOf("\"", j+1);

               try
               {
                   str = str.Substring(j+1,k-j-1);
                   lstArray[i * 2 + 1] = str;
               }
               catch
               {
                   MessageBox.Show("出错位置第" + (i + 1).ToString() + "个数组");
                   Application.Exit();
                   return;
               }
           }

           byte[] bin = new byte[bin_len];
           UInt32 index = 0,addr=0;

           for (i = 0; i < lstArray.Count / 2; i++)
           {
               str = lstArray[i * 2+1];
               byte[] bytes = System.Text.Encoding.Unicode.GetBytes(str);

               if (bytes.Length == 1)
               {
                   index = bytes[0];
               }
               else if (bytes.Length == 2)
               {
                   index = (UInt32)(bytes[1] << 8) | (UInt32)bytes[0];
               }

               if (comboBox_fonttype.SelectedIndex == 0)
               {
                   if ((index <= UTF8_CODE_END) && (index >= UTF8_CODE_BASS))
                   {
                       index = index - UTF8_CODE_BASS;
                       addr = index * 32;
                   }
                   else if ((index <= UTF8_SYMBLE1_END) && (index >= UTF8_SYMBLE1_BASS))
                   {
                       index = index - UTF8_SYMBLE1_BASS;
                       addr = index * 32 + UTF8_SYMBLE1_ADDR;
                   }
                   else if ((index <= UTF8_SYMBLE2_END) && (index >= UTF8_SYMBLE2_BASS))
                   {
                       index = index - UTF8_SYMBLE2_BASS;
                       addr = index * 32 + UTF8_SYMBLE2_ADDR;
                   }
                   else if ((index <= UTF8_SYMBLE3_END) && (index >= UTF8_SYMBLE3_BASS))
                   {
                       index = index - UTF8_SYMBLE3_BASS;
                       addr = index * 32 + UTF8_SYMBLE3_ADDR;
                   }
                   else if ((index <= UTF8_ASCII_END) && (index >= UTF8_ASCII_BASS))
                   {
                       index = index - UTF8_ASCII_BASS;
                       addr = index * (32 / 2) + UTF8_ASCII_ADDR;
                   }
               }
               else if (comboBox_fonttype.SelectedIndex == 1)
               {
                   if ((index <= UTF8_CODE_END) && (index >= UTF8_CODE_BASS))
                   {
                       index = index - UTF8_CODE_BASS;
                       addr = index * 128 + UTF8_MASK_32X32_ADDR;
                   }
                   else if ((index <= UTF8_SYMBLE1_END) && (index >= UTF8_SYMBLE1_BASS))
                   {
                       index = index - UTF8_SYMBLE1_BASS;
                       addr = index * 128 + UTF8_SYMBLE1_32X32_ADDR;
                   }
                   else if ((index <= UTF8_SYMBLE2_END) && (index >= UTF8_SYMBLE2_BASS))
                   {
                       index = index - UTF8_SYMBLE2_BASS;
                       addr = index * 128 + UTF8_SYMBLE2_32X32_ADDR;
                   }
                   else if ((index <= UTF8_SYMBLE3_END) && (index >= UTF8_SYMBLE3_BASS))
                   {
                       index = index - UTF8_SYMBLE3_BASS;
                       addr = index * 128 + UTF8_SYMBLE3_32X32_ADDR;
                   }
                   else if ((index <= UTF8_ASCII_END) && (index >= UTF8_ASCII_BASS))
                   {
                       index = index - UTF8_ASCII_BASS;
                       addr = index * (128 / 2) + UTF8_ASCII_32X32_ADDR;
                   }
               }

                

               for (k = 0; k < lstArray[i*2].Length/2; k++)
               {
                   bin[addr + k] = Convert.ToByte(lstArray[i*2].Substring(k*2,2), 16);
               }
           }

           string path = label_outfilename.Text;
           path = path.Replace(".txt", string.Empty).Replace(".TXT", string.Empty);
           path = path + ".bin";
           FileStream fs = new FileStream(path, FileMode.Create);
           BinaryWriter bw = new BinaryWriter(fs);
           bw.Write(bin, 0, bin.Length);
           bw.Flush();
           bw.Close();

           MessageBox.Show("保存成功!");
       }
        
        private void Diary()
        {
            if (textBox_key.Text.Length != 6)
            {
                MessageBox.Show("密码长度不对!");
                return;
            }
            if (comboBox_fonttype.SelectedIndex == 0)
            {
                string str = textInput.Text;
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(str);
                byte[] EncryData = GetEncryData(byteArray, byteArray.Length, textBox_key.Text);
                string path = "C://Users//HP//Desktop//" + "chengdong" + DateTime.Now.ToString("yyyyMMdd HHmmss") + ".diary";

                FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                BinaryWriter binWriter = new BinaryWriter(fs);
                binWriter.Write(EncryData, 0, EncryData.Length);
                binWriter.Close();
                fs.Close();
                cope_to_Clipboard(path);
                richTextBox_out.Text = "";
                richTextBox_out.AppendText("输出文件名:" + path + "\r\n");
                richTextBox_out.AppendText("已复制到剪切板\r\n");
                MessageBox.Show("保存成功!");
            }
            else if (comboBox_fonttype.SelectedIndex == 1)
            {
                if (source_file_textBox.Text.Contains(".diary") == false)
                {
                    MessageBox.Show("打开文件格式不对!");
                    return;
                }
                if (source_file_textBox.Text.Contains("chengdong") == false)
                {
                    MessageBox.Show("打开文件名称不对!");
                    return;
                }
                byte[] bBuffer;

                FileStream fs = new FileStream(source_file_textBox.Text, FileMode.Open);
                BinaryReader binReader = new BinaryReader(fs);

                bBuffer = new byte[fs.Length];
                binReader.Read(bBuffer, 0, (int)fs.Length);

                binReader.Close();
                fs.Close();
                byte[] EncryData = GetDeEncryData(bBuffer, textBox_key.Text);
                string str = System.Text.Encoding.Default.GetString(EncryData);
                richTextBox_out.Text = str;
                MessageBox.Show("解密完成!");
            }
        }

        private void Novel_Filtering()
        {
            int i = 0, j = 0;
            string orgTxt1 = textInput.Text.Trim();
            orgTxt1 = orgTxt1.Replace("\r\n\r\n", "\r\n");
            j = orgTxt1.IndexOf("本书最新章节", 0);
            j = orgTxt1.IndexOf("\r\n", j);
            j = orgTxt1.IndexOf("\r\n", j);
            i = orgTxt1.IndexOf("上一章", j);
            textInput.Text = orgTxt1.Substring(j, i - j);
        }
        private void dsview_analysis()
       {
            int i = 0,j=0;
            UInt32 frequency = 0;
            string f_str = "";
            string path = source_file_textBox.Text;
            label_outfilename.Text = path.Replace(".CSV", string.Empty).Replace(".csv", string.Empty) + "_ok.txt";
            List<string> str_List=new List<string>(); ;
            DataTable data = OpenCSV(path,4, ref str_List);
            StringBuilder str = new StringBuilder("", data.Rows.Count * 10);
            foreach (DataRow row in data.Rows)
            {
                str.Append(row[0] + Environment.NewLine);
            }
            textInput.Text = str.ToString();

            i = str_List[2].IndexOf("Hz");
            Double[] period = new Double[data.Rows.Count-2];
            Double max, min, average;
            if (i != -1)
            {
                j = str_List[2].IndexOf("Sample rate: ");
                if (j != -1)
                {
                    f_str = str_List[2].Substring(j + 13, i - (j + 15));
                    frequency = Convert.ToUInt32(f_str);
                    if (str_List[2][i-1] == 'M')
                    {
                        frequency = frequency * 1000000;
                    }
                    else if (str_List[2][i-1] == 'K')
                    {
                        frequency = frequency * 1000;
                    }
                    Double a, b;
                    str.Clear();
                    for (i = 2; i < data.Rows.Count; i++)
                    {
                        a = Convert.ToDouble(data.Rows[i][0]);
                        b = Convert.ToDouble(data.Rows[i - 1][0]);
                        period[i-2] = a-b;
                        str.Append(period[i-2].ToString("f6") + Environment.NewLine);
                    }
                    min = period.Min();
                    max = period.Max();
                    average = period.Average();
                    string header = "总共处理周期:"+ data.Rows.Count.ToString() + Environment.NewLine;
                    header += "最大周期:" + max.ToString("f6") + "  最小周期:" + min.ToString("f6") + "  平均周期:" + average.ToString("f6") + Environment.NewLine;
                    richTextBox_out.Text = header+str.ToString();
                }
            }

            
            MessageBox.Show("保存成功!");
       }

       private void Chinese_to_utf8_arr()
       {
           int i = 0,j=0;
           string orgTxt1 = textInput.Text.Trim();

           List<string> lstArray = orgTxt1.ToLower().Split(new char[2] { '\r', '\n' }).ToList();

           string str = "";
           byte[] buffer_utf8;

           for (i = 0; i < lstArray.Count; i++)
           {
               buffer_utf8 = Encoding.UTF8.GetBytes(lstArray[i]);

                if ((comboBox_datatype.SelectedIndex == 0) || (comboBox_datatype.SelectedIndex == 1))
                {
                    if (comboBox_datatype.SelectedIndex == 0)
                        str = "uint8_t buf[]={" + "0x" + buffer_utf8.Length.ToString("X02") + ",";
                    else if (comboBox_datatype.SelectedIndex == 1)
                        str = "uint8_t buf[]={";
                    for (j = 0; j < buffer_utf8.Length; j++)
                    {
                        str = str + "0x" + buffer_utf8[j].ToString("X") + ",";
                    }
                    lstArray[i] = str + "};" + "//" + lstArray[i];
                }
                else if (comboBox_datatype.SelectedIndex == 2)
                {
                    for (j = 0; j < buffer_utf8.Length; j++)
                    {
                        str = str + buffer_utf8[j].ToString("X") + " ";
                    }
                    lstArray[i] = str;
                }
                else if (comboBox_datatype.SelectedIndex == 3)
                {
                    for (j = 0; j < buffer_utf8.Length; j++)
                    {
                        str = str + buffer_utf8[j].ToString("X");
                    }
                    lstArray[i] = str;
                }
            }

           richTextBox_out.Text = "";

           for (i = 0; i < lstArray.Count; i++)
           {
               richTextBox_out.AppendText(lstArray[i] + "\r\n");
           }

           string path = label_outfilename.Text;
           path = path.Replace(".txt", "_ok.txt").Replace(".TXT", "_ok.TXT");
           using (FileStream fsWrite = new FileStream(path, FileMode.Create, FileAccess.Write))
           {
               byte[] buffer = null;
               for (i = 0; i < lstArray.Count; i++)
               {
                   buffer = Encoding.Default.GetBytes(lstArray[i] + "\r\n");
                   fsWrite.Write(buffer, 0, buffer.Length);
               }
           }

           MessageBox.Show("保存成功!");
       }

       private void keil_memery()
       {
           int i = 0;
           string orgTxt1 = textInput.Text.Trim();

           List<string> lstArray = orgTxt1.ToLower().Split(new char[2] { '\r', '\n' }).ToList();

           richTextBox_out.Text = "";

           for (i = 1; i < (lstArray.Count-2); i++)
           {
               richTextBox_out.AppendText(lstArray[i].Substring(9,32) + "\r\n");
           }

           string path = label_outfilename.Text;
           path = path.Replace(".txt", "_ok.txt").Replace(".TXT", "_ok.TXT");
           using (FileStream fsWrite = new FileStream(path, FileMode.Create, FileAccess.Write))
           {
               byte[] buffer = null;
               for (i = 1; i < (lstArray.Count-2); i++)
               {
                   buffer = Encoding.Default.GetBytes(lstArray[i].Substring(9, 32) + "\r\n");
                   fsWrite.Write(buffer, 0, buffer.Length);
               }
           }
           MessageBox.Show("保存成功!");
       }

       private void Data_filled_complement_zero()
       {
           int i = 0;
           string orgTxt1 = textInput.Text.Trim();

           List<string> lstArray = orgTxt1.ToLower().Split(new char[1] { ' '}).ToList();

           richTextBox_out.Text = "";

           for (i = 0; i < lstArray.Count; i++)
           {
               if (lstArray[i].Length==1) 
                  richTextBox_out.AppendText("0"+lstArray[i]+" ");
               else
                   richTextBox_out.AppendText(lstArray[i] + " ");
           }

           string path = label_outfilename.Text;
           path = path.Replace(".txt", "_ok.txt").Replace(".TXT", "_ok.TXT");
           using (FileStream fsWrite = new FileStream(path, FileMode.Create, FileAccess.Write))
           {
               byte[] buffer = null;
               buffer = Encoding.Default.GetBytes(richTextBox_out.Text);
               fsWrite.Write(buffer, 0, buffer.Length);
           }

           MessageBox.Show("保存成功!");
       }
    
       private string Byte_reversal(string input)//整个字符串调换
       {
           string str = "";
           for (int i = 0; i < (input.Length) / 2; i++)
           {
               str = str + input.Substring(input.Length-(i+1)*2,2);
           }
               return str;
       }

        private string Byte_reversal_endian(string input)//只是调换两个字节,局部变化
        {
            string str = "";
            for (int i = 0; i < (input.Length) / 2/2; i++)
            {
                str = str + "0X"+input.Substring( i*4+2, 2) +"," + "0X" + input.Substring(i * 4 , 2) + ",";
            }
            return str;
        }

        private bool effective_rows_judge(string input)
        {
            string str = "0123456789abcdefABCDEFxX,";
            int i = 0;
            for (i = 0; i < input.Length; i++)
            {
                if (str.Contains(input[i]) == false)
                {
                    return false;
                }
            }
            return true;
        }

        private void Data_reversal()
       {
           int i = 0;
            List<string> lstArray;
            string orgTxt1;
           
            richTextBox_out.Text = "";
            string str = "", str1 = "";

            if (comboBox_datatype.SelectedIndex == 2)
            {
                orgTxt1 = textInput.Text;
                lstArray = orgTxt1.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                for (i = 0; i < lstArray.Count; i++)
                {
                    str = lstArray[i];
                    if (effective_rows_judge(str) == true)
                    {
                        str = str.Replace("0X", "").Replace("0x", "").Replace(",", "");
                        str=Byte_reversal_endian(str);
                    }
                    //richTextBox_out.AppendText(str + "\r\n");
                    str1 += str + "\r\n";
                }
                richTextBox_out.Text = str1;
            }
            else
            {
                orgTxt1 = textInput.Text.Trim();
                orgTxt1 = orgTxt1.Replace(" ", "").Replace("-", "");
                lstArray = orgTxt1.ToLower().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                string[] strArray = new string[lstArray.Count];
                for (i = 0; i < lstArray.Count; i++)
                {
                    str = lstArray[i];
                    if (comboBox_fonttype.SelectedIndex != 0)
                    {
                        if (comboBox_fonttype.SelectedIndex == 1)
                        {
                            str = str.Replace("0X", "").Replace("0x", "").Replace(",", "");
                        }
                        else if (comboBox_fonttype.SelectedIndex == 2)
                        {
                            int index = str.IndexOf("\r\n");
                            str = str.Remove(0, index + 2);
                            index = str.LastIndexOf("\r\n");
                            str = str.Remove(index, str.Length - index);
                            str = str.Replace("0X", "").Replace("0x", "").Replace(",", "");
                        }
                    }
                    strArray[i] = Byte_reversal(str);
                    richTextBox_out.AppendText(strArray[i] + "\r\n");
                }

                if (comboBox_datatype.SelectedIndex == 0)
                {
                    richTextBox_out.AppendText("\r\n" + "difference value:" + "\r\n");
                    for (i = 0; i < lstArray.Count / 2; i++)
                    {
                        if ((strArray[i].Length <= 16) && (strArray[i + 1].Length <= 16))
                        {
                            UInt64 n = Convert.ToUInt64(strArray[i], 16);
                            UInt64 m = Convert.ToUInt64(strArray[i + 1], 16);
                            if (m >= n)
                            {
                                richTextBox_out.AppendText("0X" + (m - n).ToString("X") + "\r\n");
                            }
                            else
                            {
                                richTextBox_out.AppendText("Error m<n\r\n");
                            }
                        }
                        else
                        {
                            richTextBox_out.AppendText("Error m>16 || n>16\r\n");
                        }
                    }
                }
            }

           string path = label_outfilename.Text;
            if ((path.Contains(".txt")) || path.Contains(".TXT"))
                path = path.Replace(".txt", "_ok.txt").Replace(".TXT", "_ok.TXT");
            else if ((path.Contains(".c")) || path.Contains(".C"))
                path = path.Replace(".c", "_ok.c").Replace(".C", "_ok.C");
            else
                return;
            using (FileStream fsWrite = new FileStream(path, FileMode.Create, FileAccess.Write))
           {
               byte[] buffer = null;
               buffer = Encoding.Default.GetBytes(richTextBox_out.Text);
               fsWrite.Write(buffer, 0, buffer.Length);
           }

           MessageBox.Show("保存成功!");
       }
       private void text_to_bin()
       {
           string orgTxt1 = textInput.Text.Trim();
           if (comboBox_fonttype.SelectedIndex != 0)
           {
               if (comboBox_fonttype.SelectedIndex == 1)
               {
                   orgTxt1 = orgTxt1.Replace("0X", "").Replace("0x", "").Replace(",", " ");
               }
               else if (comboBox_fonttype.SelectedIndex == 2)
               {
                   int index = orgTxt1.IndexOf("\r\n");
                   orgTxt1 = orgTxt1.Remove(0, index+2);
                   index = orgTxt1.LastIndexOf("\r\n");
                   orgTxt1 = orgTxt1.Remove(index, orgTxt1.Length - index);
                   orgTxt1 = orgTxt1.Replace("0X", "").Replace("0x", "").Replace(",", " ");
               }
           }
           if (comboBox_datatype.SelectedIndex != 0)
           {
               orgTxt1 = orgTxt1.Replace(" ", "");
           }
           orgTxt1 = orgTxt1.Replace("\r\n", "");
           byte[] bin=new byte[10];
           if (comboBox_datatype.SelectedIndex == 0)
           {
               bin = System.Text.Encoding.ASCII.GetBytes(orgTxt1);
           }
           else if (comboBox_datatype.SelectedIndex == 1)
           {
               bin = strToToDecByte(orgTxt1);
           }
           else if (comboBox_datatype.SelectedIndex == 2)
           {
               bin = strToToHexByte(orgTxt1);
           }

           Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

           dlg.FileName = "bin";

           dlg.DefaultExt = ".bin";

           dlg.Filter = "bin file (.bin)|*.bin";

           if (dlg.ShowDialog() == false)
               return;

           FileStream fs = new FileStream(dlg.FileName, FileMode.Create);
           BinaryWriter bw = new BinaryWriter(fs);
           bw.Write(bin, 0, bin.Length);
           bw.Flush();
           bw.Close();

           MessageBox.Show("保存成功!");
       }

       public int[] MinToMax(int[] array)
       {
           int[] FianlMTM = new int[array.Length];

           for (int j = 0; j < array.Length; j++)
           {
               int last = 0;
               for (int i = 1; i < array.Length - j; i++)
               {
                   int t = array[i];

                   if ((array[i]) < (array[i - 1]))
                   {

                       array[i] = array[i - 1];
                       last = array[i];
                       array[i - 1] = t;

                   }
                   else
                       last = array[i];
               }
               FianlMTM[array.Length - (j + 1)] = last;

           }
           return FianlMTM;
       }

       private void Fine_max_notuse_index()
       {
           int i = 0,j=0,k=0;
           string orgTxt1 = textInput.Text.Trim();
           List<string> lstArray = new List<string>();
           string key = textBox_key.Text;
           Regex r = new Regex(key+@"([1-9]\d*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);//正则
           Match m = r.Match(orgTxt1);//匹配

           while (m.Success)
           {
               lstArray.Add(m.Groups[0].Value);
               m = m.NextMatch();//匹配下一个
           }

           richTextBox_out.Text = "input order:";
           int[] array = new int[lstArray.Count];

           for (i = 0; i < lstArray.Count; i++)
           {
               richTextBox_out.AppendText(lstArray[i] + " ");
               array[i] = Convert.ToInt32(lstArray[i].Substring(key.Length, lstArray[i].Length - key.Length));
           }

           for (i = 0; i < array.Length; i++)
	       {
               for (j = i + 1; j < array.Length; j++)
		        {
                    if (array[i] > array[j])
			        {
                        k = array[i];
                        array[i] = array[j];
                        array[j] = k;
			        }
		        }
	        }

           int[] array_index = new int[lstArray.Count];
           i = 0;
           richTextBox_out.AppendText("\r\nupon order:");
           for (j = 0; j < array.Length; j++)
           {
               richTextBox_out.AppendText(array[j] + " ");
               if (j == 0)
               {
                   if(array[j] != 1)
                     array_index[i++] = 1;
               }
               else
               {
                   if (array[j] > (array[j - 1] + 1))
                   {
                       array_index[i++] = array[j - 1] + 1;
                   }
               }
           }

           if (i >0)
            richTextBox_out.AppendText("\r\notuse min index:" + array_index[0].ToString());
       }

       private void extract_rank_data()
       {
           int i = 0;
           if (comboBox_indicate.Items.Count <= 0)
           {
               return;
           }
           List<string>[] lis = new List<string>[textInput.Lines.Length];

           for (i = 0; i < textInput.Lines.Length; i++)
           {
               string str = new Regex("[\\s]+").Replace(textInput.Lines[i].ToString(), " ").Trim(); ;
               lis[i] = str.Split(new string[] { " " }, StringSplitOptions.None).ToList();
           }
           for (i = 0; i < lis.Length; i++)
           {
               richTextBox_out.AppendText(lis[i][comboBox_indicate.SelectedIndex]+"\r\n");
           }
       }

       private UInt32 get_struct_element_size(string a)
       {
           UInt32 c_base = 0;
           int k = a.IndexOf("u");
           if (!(a.Substring(0, k).Contains("/")))
           {
               if (a.Contains("uint8_t"))
               {
                   c_base = 1;
               }
               else if (a.Contains("uint16_t"))
               {
                   c_base = 2;
               }
               else if (a.Contains("uint32_t"))
               {
                   c_base = 4;
               }

               int n = a.IndexOf("[");
               int m = a.IndexOf("]");
               if ((n > 0) && (m > n))
               {
                   if (m < (n + 4))
                   {
                       string str = a.Substring(n + 1, m - n - 1);
                       c_base = Convert.ToUInt32(str);
                   }
               }
           }
           
           return c_base;
       }

       private void Cstruct_element_size()
       {
           int max_len = 0;
           uint offect = 0;
           List<string> lstArray = textInput.Text.Split('\n').ToList(); ;

           for (int m = 0; m < lstArray.Count; m++)
           {
               lstArray[m] = lstArray[m].TrimEnd();
               if (lstArray[m].Length > max_len)
                   max_len = lstArray[m].Length;
           }
           for (int m = 0; m < lstArray.Count; m++)
           {
               if (lstArray[m].Length!=0)
               {
                   uint k = get_struct_element_size(lstArray[m]);
                   if (k > 0)
                   {
                       lstArray[m] = lstArray[m].PadRight(max_len + 4);
                       lstArray[m] = lstArray[m] + "//"  + k.ToString() + "  0x" + offect.ToString("X");
                       offect += k;
                   }
               }
           }
           foreach (string str in lstArray)
           {
               richTextBox_out.AppendText(str + "\r\n");
           }
       }

       private void Data_xor()
       {
           int i = 0;
           string orgTxt1 = textInput.Text.Trim();
           orgTxt1 = orgTxt1.Replace(" ", "").Replace("-", "");

           List<string> lstArray = orgTxt1.ToLower().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

           richTextBox_out.Text = "CRC:\r\n";

           for (i = 0; i < lstArray.Count; i++)
           {
               byte[] data= strToToHexByte(lstArray[i]);
               byte crc = 0;

               for (int j = 0; j < data.Length; j++)
               {
                   crc ^= data[j];
               }
               richTextBox_out.AppendText(crc.ToString("X") + "\r\n");
           }

           //richTextBox_out.AppendText("\r\n" + "difference value:" + "\r\n");
           //for (i = 0; i < lstArray.Count / 2; i++)
           //{
           //    if ((strArray[i].Length <= 16) && (strArray[i + 1].Length <= 16))
           //    {
           //        UInt64 n = Convert.ToUInt64(strArray[i], 16);
           //        UInt64 m = Convert.ToUInt64(strArray[i + 1], 16);
           //        if (m >= n)
           //        {
           //            richTextBox_out.AppendText("0X" + (m - n).ToString("X") + "\r\n");
           //        }
           //        else
           //        {
           //            richTextBox_out.AppendText("Error m<n\r\n");
           //        }
           //    }
           //    else
           //    {
           //        richTextBox_out.AppendText("Error m>16 || n>16\r\n");
           //    }
           //}

           string path = label_outfilename.Text;
           path = path.Replace(".txt", "_ok.txt").Replace(".TXT", "_ok.TXT");
           using (FileStream fsWrite = new FileStream(path, FileMode.Create, FileAccess.Write))
           {
               byte[] buffer = null;
               buffer = Encoding.Default.GetBytes(richTextBox_out.Text);
               fsWrite.Write(buffer, 0, buffer.Length);
           }

           MessageBox.Show("保存成功!");
       }

       private void Rtc_Deviation()
       {
           int i = 0;
           if (comboBox_indicate.Items.Count <= 0)
           {
               return;
           }
           List<string>[] lis = new List<string>[textInput.Lines.Length];

           for (i = 0; i < textInput.Lines.Length; i++)
           {
               string str = textInput.Lines[i].ToString();
               str = new Regex("[\\s]+").Replace(textInput.Lines[i].ToString(), " ").Trim() ; 
               lis[i] = str.Split(new string[] { " " }, StringSplitOptions.None).ToList();
           }

           if (comboBox_datatype.SelectedIndex == 0)
           {
               for (i = 1; i < lis.Length; i++)
               {
                   if ((lis[i - 1].Count < 3) || (lis[i].Count < 3))
                   {
                       MessageBox.Show("input empty line");
                       return;
                   }

                   richTextBox_out.Text = "运行时间：";
                   string str1 = lis[i - 1][3];
                   string str = lis[i][3];

                   Int32 date = 0;
                   if ((str.Length == 0) || (str1.Length == 0))
                   {
                       MessageBox.Show("input error");
                       return;
                   }



                   str = str.Trim();
                   str1 = str1.Trim();

                   if (comboBox_indicate.Text.Length != 0)
                   {
                       date = Convert.ToInt32(comboBox_indicate.Text, 10);
                   }

                   if (str.Length > 13)
                   {
                       cal_Calendar_time_differenceble_subtract(str, str1, textBox_key, textBox_filesize);
                   }
                   else
                   {
                       cal_Calendar_time_difference_subtract(str, str1, date, textBox_key, textBox_filesize);
                   }
                   richTextBox_out.AppendText(textBox_key.Text + " 手机时间戳差值：" + textBox_filesize.Text);

                   str1 = lis[i - 1][5].Substring(18, 11).Replace("-", "");
                   str = lis[i][5].Substring(18, 11).Replace("-", "");
                   str1 = Byte_reversal(str1);
                   str = Byte_reversal(str);
                   UInt32 timer1 = Convert.ToUInt32(str1, 16);
                   UInt32 timer = Convert.ToUInt32(str, 16);
                   timer = timer - timer1;
                   richTextBox_out.AppendText(" 手环时间戳差值：0x" + timer.ToString("X"));

                   timer1 = Convert.ToUInt32(textBox_filesize.Text, 16);
                   date = (Int32)((double)timer1 - (double)timer);
                   if (date > 0)
                       richTextBox_out.AppendText(" 手环时间偏差：+" + date.ToString());
                   else
                       richTextBox_out.AppendText(" 手环时间偏差：" + date.ToString());

                   richTextBox_out.AppendText("\r\n");
               }
           }
           else if (comboBox_datatype.SelectedIndex == 1)
           {
               for (i = 1; i < lis.Length; i++)
               {
                   if ((lis[i - 1].Count < 11) || (lis[i].Count < 11))
                   {
                       MessageBox.Show("input empty line");
                       return;
                   }
                   richTextBox_out.Text = "时间差值：";
                   string str1 = lis[i - 1][0x10];
                   string str = lis[i][0x10];

                   if ((str.Length == 0) || (str1.Length == 0))
                   {
                       MessageBox.Show("input error");
                       return;
                   }

                   str = str.Trim();
                   str1 = str1.Trim();

                   cal_Calendar_time_differenceble_subtract(str, str1, textBox_key, textBox_filesize);
                   richTextBox_out.AppendText(textBox_key.Text + " 时间戳差值：" + textBox_filesize.Text);
               }
           }
            else if (comboBox_datatype.SelectedIndex == 2)
            {
                richTextBox_out.Text = "\r\n";
                for (i = 1; i < lis.Length; i++)
                {
                    string str1 = lis[i - 1][0];
                    string str = lis[i][0];

                    if ((str.Length == 0) || (str1.Length == 0))
                    {
                        MessageBox.Show("input error");
                        return;
                    }

                    str = str.Trim();
                    str1 = str1.Trim();

                    
                    richTextBox_out.AppendText(" 和上行时间差值：" + cal_Calendar_time_difference_subtract(str, str1, 0, null, null)+ "\r\n");
                }
            }
            MessageBox.Show("保存成功!");
       }
        
       private void Bytestoutf8()
       {
            int i = 0;
            string orgTxt1 = textInput.Text.Trim();
            if (comboBox_datatype.SelectedIndex == 0)
            {
                orgTxt1 = orgTxt1.Replace(" ", "").Replace("-", "").Replace("\r\n", "").Replace("\r", "").Replace("0x", "").Replace("0X", "").Replace("\t", "").Replace(":", "");
                if (textBox_key.Text.Length > 0)
                    orgTxt1 = orgTxt1.Replace(textBox_key.Text, "");

                while (true)
                {
                    int n = orgTxt1.IndexOf("[");
                    int m = orgTxt1.IndexOf("]");
                    if ((n == -1) || (m == -1)) break;
                    orgTxt1 = orgTxt1.Remove(n, m - n + 1);
                }
            }
            else if (comboBox_datatype.SelectedIndex == 1)
            {
                do
                {
                    orgTxt1 = orgTxt1.Replace("  ", " ");
                } while ((orgTxt1.Contains("  ")==true));
                do
                {
                    orgTxt1 = orgTxt1.Replace("\t", "");
                } while ((orgTxt1.Contains("\t") == true));
                do
                {
                    orgTxt1 = orgTxt1.Replace("byte", "");
                } while ((orgTxt1.Contains("byte") == true));
                while (true)
                {
                    int n = orgTxt1.IndexOf("[");
                    int m = orgTxt1.IndexOf("]");
                    if ((n == -1) || (m == -1)) break;
                    orgTxt1 = orgTxt1.Remove(n, m - n + 1);
                }
                orgTxt1 = orgTxt1.Replace("0x", "").Replace("0X", "").Replace("\r\n", "").Replace("\r", "").Replace("\t", "").Replace(":", "");
            }
            else
            {
                List<string> lstArray = orgTxt1.ToLower().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                orgTxt1 = "";
                for (i = 0; i < lstArray.Count; i++)
                {
                    if (lstArray[i].Contains("J-Link") == false)
                    {
                        int n = lstArray[i].IndexOf("=");
                        if (n > 0)
                        {
                            orgTxt1 += lstArray[i].Substring(n + 1, lstArray[i].Length-(n + 1));
                        }
                        
                    }
                }
                do
                {
                    orgTxt1 = orgTxt1.Replace("  ", " ");
                } while ((orgTxt1.Contains("  ") == true));
                orgTxt1 = orgTxt1.Replace("0x", "").Replace("0X", "").Replace("\r\n", "").Replace("\r", "").Replace("\t", "").Replace(":", "");
            }
            //546869732069732074686520746573742064617461-->This is the test data
            byte[] data = strToToHexByte(orgTxt1);
            orgTxt1 = System.Text.Encoding.ASCII.GetString(data);
            //orgTxt1 = Regex.Replace(orgTxt1, "[^\x0d\x0a\x20-\x7e\t]", "");
            orgTxt1 = Regex.Replace(orgTxt1, "[^\x20-\x7e]", "");
            richTextBox_out.Text = orgTxt1;
           MessageBox.Show("保存成功!");
       }
       private void Keil_Undefined_symbol_extract()
       {
           int m = 0, n = 0;
           string orgTxt1 = textInput.Text.Trim();

           List<string> lstArray = orgTxt1.ToLower().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
           richTextBox_out.Text = "";
           foreach (string str in lstArray)
           {
               m = str.IndexOf("ndefined symbol");
               if (m != -1)
               {
                   m = str.IndexOf(" ", m + 11);
                   if (m != -1)
                   {
                       n = str.IndexOf(" ", m + 2);
                       if (m != -1)
                       {
                           richTextBox_out.AppendText(str.Substring(m + 1, n - m-1) + "\r\n");
                       }
                   }
               }
               
           }

           MessageBox.Show("保存成功!");
       }

        private void Get_rom_extract()
        {
            int i = 0;
            string orgTxt1 = textInput.Text.Trim();
            List<string> lstArray = orgTxt1.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> outTxt_line = new List<string>();
            List<string> line = new List<string>();
            string separator = textBox_filesize.Text;
            string outTxt1 = "";
            string separator_out = "";
            int row_index = 0;

            if (comboBox_fonttype.SelectedIndex == 0)
            {
                separator_out = " ";
            }
            else if (comboBox_fonttype.SelectedIndex == 1)
            {
                separator_out = "\r\n";
            }
            if (comboBox_datatype.SelectedIndex == 0)
            {
                line = lstArray[0].Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                for (i = 0; i < line.Count; i++)
                {
                    if (line[i].Contains(textBox_key.Text))
                    {
                        row_index = i;
                        break;
                    }
                }
                if (i >= line.Count)
                {
                    MessageBox.Show("找不到指定的关键字!");
                    return;
                }
                else
                {
                    for (i = 1; i < lstArray.Count; i++)
                    {
                        line = lstArray[i].Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        outTxt1 += line[row_index] + separator_out;
                        outTxt_line.Add(line[row_index]);
                    }
                }
            }
            else if (comboBox_datatype.SelectedIndex == 1)
            {
                row_index = Convert.ToInt32(textBox_key.Text);
                for (i = 0; i < lstArray.Count; i++)
                {
                    line = lstArray[i].Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    outTxt1 += line[row_index] + separator_out;
                    outTxt_line.Add(line[row_index]);
                }
            }

            if (comboBox_indicate.SelectedIndex == 1)
            {
                if (lstArray.Count == 3)
                {
                    string ret = "第三行和第一行时间差:";
                    string dif = "";
                    dif = cal_Calendar_time_differenceble_subtract(outTxt_line[2], outTxt_line[0], null, null);
                    ret += dif;
                    ret += "\r\n";

                    ret += "下一个连接事件的时间点:";
                    ret += cal_Calendar_ble_time_sum(outTxt_line[2], dif, null, null);
                    ret += "\r\n";
                    outTxt1 = outTxt1.Insert(0, ret);
                }
                else
                {
                    MessageBox.Show("输入内容有错,应该从sniffer的LE DATA粘贴第一个和第三个为主机/从机的数据包!");
                }
            }

            richTextBox_out.Text = outTxt1;
            MessageBox.Show("保存成功!");
        }

        private void SourceInsight_SearchResults_Analysis()
        {
            int i = 0;
            int m = 0, n = 0;
            string orgTxt1 = textInput.Text.Trim();
            List<string> lstArray = orgTxt1.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            int[] line_indexof = new int[lstArray.Count];
            string str = "", str1 = "";
            line_indexof[0] = 0;
            for (i = 1; i < lstArray.Count; i++)
            {
                line_indexof[i] = 0;
                str = lstArray[i];
                m = str.IndexOf(" ");
                if(m!=0)
                {
                    str1 = str.Substring(0, m);
                    if (str1.Contains(".h") || str1.Contains(".c"))  //开始就是文件名,不做处理
                    {
                        continue;
                    }
                }
                n = str.IndexOf(" in ", m - 1);
                if (n >= m)
                {
                    line_indexof[i] = n+4;
                }
            }
            for (i = 0; i < lstArray.Count; i++)
            {
                str += lstArray[i].Substring(line_indexof[i]);
            }
            richTextBox_out.Text = str;
            MessageBox.Show("保存成功!");
        }

        private void Get_arr()
        {
            int i,j=1;
            int m = 0;
            if (textBox_filesize.Text.Length == 0)
            {
                MessageBox.Show("长度输入无效!");
                return;
            }
            UInt32 len = Convert.ToUInt32(textBox_filesize.Text);
            StringBuilder str = new StringBuilder();
            Byte data = 0xff;
            if((comboBox_datatype.SelectedIndex == 3) || (comboBox_datatype.SelectedIndex == 4))
            {
                string str_input = textInput.Text.Trim();
                if (comboBox_datatype.SelectedIndex == 4)
                {
                    data = Convert.ToByte(str_input.Substring(0,2), 10);
                    str.Append("0x" + data.ToString("X2") + ",");
                    str_input=str_input.Remove(0,2);
                }
                byte[] array = System.Text.Encoding.ASCII.GetBytes(str_input);
                for (i = 0; i < len-1; i++)
                {
                    if (i < array.Length)
                    {
                        str.Append("0x" + array[i].ToString("X2") + ",");
                    }
                    else
                    {
                        data = Convert.ToByte(textBox_key.Text, 16);
                        str.Append("0x" + data.ToString("X2") + ",");
                    }
                    if (((i+2) % 16 == 0) && (i!=0)) str.Append("\r\n");
                }
            }
            else
            {
                for (i = 0; i < len; i++)
                {
                    if (comboBox_datatype.SelectedIndex == 2)
                    {
                        if (i == 0)
                            data = Convert.ToByte(textBox_key.Text, 16);
                        m = data;
                    }
                    str.Append("0x" + m.ToString("X2") + ",");
                    m++;
                    if ((i + 1) % 16 == 0) str.Append("\r\n");
                    if (comboBox_datatype.SelectedIndex == 0)
                    {
                        if (m > 255) m = 0;
                    }
                    else if (comboBox_datatype.SelectedIndex == 1)
                    {
                        if (m > 255)
                        {
                            m = j;
                            j++;
                            if (j > 0xfe) j = 0;
                        }
                    }
                }
            }
           
            richTextBox_out.Text = "" + str;
            MessageBox.Show("保存成功!");
        }

        private bool return_type_isvalid(List<string> return_type_Array,string type)
        {
            foreach (string str in return_type_Array)
            {
                if ((type.Replace(" ","")) == (str.Replace(" ", "")))
                    return true;
            }
            return false;
        }
        private void Get_api_symdef()
        {
            int i = 0;
            int m = 0, n = 0, k = 0,j=0,z=0, a = 0, b = 0, c = 0;
            string Path_source = comboBox_indicate.Text;

            string orgTxt1 = textInput.Text.Trim();
            List<string> lstArray = orgTxt1.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (i = 0; i < lstArray.Count; i++)
            {
                lstArray[i] = lstArray[i].Replace("\r", "").Replace("\n", "").Replace("\r\n", "");
            }

            List<string> line_funtion = new List<string>();
            List<string> line_addr = new List<string>();
            List<string> line_out = new List<string>();
            List<string> line_variable = new List<string>();
            List<string> line_variable_addr = new List<string>();
            List<string> line_variable_out = new List<string>();
            for (i = 0; i < lstArray.Count; i++)
            {
                m = lstArray[i].IndexOf(" T ");
                if (m > 0)
                {
                    line_funtion.Add(lstArray[i].Substring(m + 3, lstArray[i].Length - (m + 3)));
                    line_addr.Add(lstArray[i].Substring(0, m));
                    line_out.Add("");
                }
                else
                { 
                    n= lstArray[i].IndexOf(" D ");
                    if (n > 0)
                    {
                        line_variable.Add(lstArray[i].Substring(n + 3, lstArray[i].Length - (n + 3)));
                        line_variable_addr.Add(lstArray[i].Substring(0, n));
                        line_variable_out.Add("");
                    }
                }
            }
            string str = "";
            if (comboBox_datatype.SelectedIndex == 0)
            {
                if (System.IO.File.Exists(Path_source) == false)  //如果存在返回值为true，如果不存在这个文件，则返回值为false
                {
                    MessageBox.Show("源文件设置出错!");
                    return;
                }
                line_out = ReadTxtFromFile(Path_source);
                for (i = 0; i < line_out.Count; i++)
                {
                    str = line_out[i];
                    if (str.Length < 11)
                    {
                        break;
                    }
                    
                    if(i< line_funtion.Count)
                    {
                        str = str.Substring(0, str.Length - 12);  //去掉地址和)\
                        str += line_addr[i] + "))";
                    }
                    else
                    {
                        str = str.Substring(0, str.Length - 11);
                        str += line_variable_addr[i - line_funtion.Count] + ")";
                    }
                        
                    line_out[i] = str;
                }
            }
            else if (comboBox_datatype.SelectedIndex == 1)
            {
                if (System.IO.Directory.Exists(Path_source) == false)   //如果存在返回值为true，如果不存在这个文件，则返回值为false
                {
                    MessageBox.Show("源目录路径设置出错!");
                    return;
                }
                List<string> cfile = fine_filename(Path_source, ".c");
                List<string> hfile = fine_filename(Path_source, ".h");

                string str_default = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_api_symdef.txt", Encoding.Default);
                m = str_default.IndexOf("--funtion_return_type_start__");  //函数名称的起始位置'
                n = str_default.IndexOf("--funtion_return_type_end__");  //函数名称的起始位置'
                str = str_default.Substring(m + 29 + 2, n - (m + 29 + 2));
                List<string> return_type_Array = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                string str_line = "", str_out = "", str_temp = "";
                for (i = 0; i < cfile.Count; i++)
                {
                    z = 0;
                    str = System.IO.File.ReadAllText(cfile[i], Encoding.Default);
                    for (j = 0; j < line_funtion.Count; j++)
                    {
                        m = n = k = 0;
                        m = str.IndexOf(line_funtion[j], z);  //函数名称的起始位置
                        if (m > 0)
                        {
                            n = str.IndexOf("\r\n", m);  //字符串后面一个换行
                            if (n > 0)
                            {
                                k = str.LastIndexOf("\r\n", m, m);  //字符串前面一个换行
                                if (k > 0)
                                {
                                    str_line = str.Substring(k + 2, n - k - 2);   //得到函数所在的行
                                    str_temp = str_line.Substring(m - (k + 2)).Replace(" ", ""); //从名字开始没有空格的字符串
                                    if ((str_temp.Length <= line_funtion[j].Length) || (str_temp[line_funtion[j].Length] != 0x28) || (str_line.Contains(";")))  //函数名后面不是'(' 就不是函数  或者字符串长度不够
                                    {
                                        z = n;  //继续往下找本个函数
                                        j -= 1;
                                        continue;
                                    }

                                    str_temp = str.Substring(k + 2, m - k - 2).Trim();  //本行函数名称前面
                                    if (str_temp.Contains("="))  //包含等于说明不是函数
                                    {
                                        z = n;  //继续往下找本个函数
                                        j -= 1;
                                        continue;
                                    }

                                    if (str_temp == "")  //函数名称前面没有任何的数据,说明返回类型在上一行
                                    {
                                        a = str.LastIndexOf("\r\n", k, k);  //函数前面一行
                                        str_temp = str.Substring(a + 2, k - (a + 2));
                                    }
                                    if (return_type_isvalid(return_type_Array, str_temp) == false)  //返回类型不在默认列表里面
                                    {
                                        z = n;  //继续往下找本个函数
                                        j -= 1;
                                        continue;
                                    }
                                    if (str[n + 2] != 0x7b)  //是函数后立即是括号
                                    {
                                        a = str.IndexOf(")", n + 2);  //往后找反括号
                                        str_temp = str.Substring(n + 2, a - n - 2 + 1).Replace("\r\n", " "); //剩下几行的参数列表
                                        str_temp = new Regex("[\\s]+").Replace(str_temp, " ");  //替换多个空格为一个空格
                                        str_line += str_temp;
                                    }
                                    z = 0;
                                    str_out = "#define " + line_funtion[j] + " " + "((" + str_line.Replace(line_funtion[j] + "(", "(*)(") + ")((uint32_t *)" + line_addr[j] + "))";
                                    //line_out.Add(str_out);
                                    line_out[j] = str_out;
                                }
                            }
                        }
                        else  //本文件没有,下一个函数从头找
                        {
                            z = 0;
                        }
                    }
                }

                //下面开始获取带宏的函数
                m = str_default.IndexOf("--funtion_define_type_start__");  //函数名称的起始位置'
                if(m>0)
                { 
                    n = str_default.IndexOf("--funtion_define_type_end__");  //函数名称的起始位置'
                    str = str_default.Substring(m + 29 + 2, n - (m + 29 + 2));
                    List<string> define_type_Array = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    for (i = 0; i < cfile.Count; i++)
                    {
                        str = System.IO.File.ReadAllText(cfile[i], Encoding.Default);
                        for (j = 0; j < line_funtion.Count; j++)
                        {
                            if ((line_out[j] == "") && (line_funtion[j].Contains(define_type_Array[1])))   //上面没有找到 并且在宏定义列表里面
                            {
                                str_out = line_funtion[j].Replace(define_type_Array[1], "");
                                str_temp = define_type_Array[0] + str_out;
                                k = str.IndexOf(str_temp, 0);  //函数名称的起始位置
                                if (k > 0)
                                {
                                    n = str.IndexOf(")", k);  //字符串后面一个反括号
                                    str_line = str.Substring(k+ str_temp.Length+1, n - (k + str_temp.Length)-1);   //得到参数
                                    str_out = define_type_Array[2].Replace("msg_name##", str_out);
                                    str_out= str_out.Replace("param_struct", str_line);
                                    str_out = "#define " + line_funtion[j] + " " + "((" + str_out.Replace(line_funtion[j] + "(", "(*)(") + ")((uint32_t *)" + line_addr[j] + "))";
                                    line_out[j] = str_out;
                                }
                            }
                        }
                    }
                }

                m = str_default.IndexOf("--variable_type_start__");  //函数名称的起始位置'
                n = str_default.IndexOf("--variable_type_end__");  //函数名称的起始位置'
                str = str_default.Substring(m + 29 + 2, n - (m + 29 + 2));
                List<string> variable_type_Array = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                //下面开始获取变量
                for (i = 0; i < (cfile.Count+ hfile.Count); i++)
                //for (i = 0; i < (cfile.Count); i++)
                {
                    z = 0;
                    if(i< cfile.Count)
                        str = System.IO.File.ReadAllText(cfile[i], Encoding.Default);
                    else
                        str = System.IO.File.ReadAllText(hfile[i- cfile.Count], Encoding.Default);  //h文件也要查询
                    for (j = 0; j < line_variable.Count; j++)
                    {
                        m = n = k = 0;
                        m = str.IndexOf(line_variable[j], z);  //函数名称的起始位置
                        if (m > 0)
                        {
                            n = str.IndexOf("\r\n", m);  //字符串后面一个换行
                            if (n > 0)
                            {
                                k = str.LastIndexOf("\r\n", m, m);  //字符串前面一个换行
                                if (k > 0)
                                {
                                    str_line = str.Substring(k + 2, n - k - 2);   //得到函数所在的行
                                    if (i >= cfile.Count)  //在h文件中
                                    {
                                        if (str_line.Contains("extern"))  //h文件中不处理extern关键字的内容
                                        {
                                            z = n;  //继续往下找本个函数
                                            j -= 1;
                                            continue;
                                        }
                                    }
                                    a = str_line.IndexOf(line_variable[j]);
                                    if(a>0) b = str_line.IndexOf(line_variable[j],a+ line_variable[j].Length);
                                    if (b > 0)
                                    {
                                        c = str_line.IndexOf("=", a, b - a);
                                        if (c > 0) b = 0;  //这里忽略掉第二个
                                    }
                                    
                                    if (b > 0)//类似和变量包含了相同的字符串,从第二个字符串开始之前是类型
                                    {
                                        str_temp = str_line.Substring(0, b);
                                    }
                                    else
                                    {
                                        str_temp = str_line.Substring(0, a);
                                    }
                                    str_temp = new Regex("[\\s]+").Replace(str_temp, " ");  //替换多个空格为一个空格

                                    if (return_type_isvalid(variable_type_Array, str_temp) == false)  //变量类型不在默认列表里面
                                    {
                                        z = n;  //继续往下找本个函数
                                        j -= 1;
                                        continue;
                                    }

                                    str_out = "#define " + line_variable[j] + " " + "((" + str_temp.Replace("*","") + " *)"  + line_variable_addr[j] + ")";
                                    line_variable_out[j] = str_out;
                                }
                            }
                        }
                        else  //本文件没有,下一个函数从头找
                        {
                            z = 0;
                        }
                    }
                }
            }

            richTextBox_out.Text = "";
            for (i = 0; i < line_out.Count; i++)
            {
                richTextBox_out.AppendText(line_out[i] + "\r\n");
            }
            for (i = 0; i < line_variable_out.Count; i++)
            {
                richTextBox_out.AppendText(line_variable_out[i] + "\r\n");
            }

            string path = label_outfilename.Text;
            path = path.Replace(".txt", "_ok.h").Replace(".TXT", "_ok.h");
            using (FileStream fsWrite = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = null;
                buffer = Encoding.Default.GetBytes(richTextBox_out.Text);
                fsWrite.Write(buffer, 0, buffer.Length);
            }

            MessageBox.Show("保存成功!");
        }
        public void Director(string dir, List<string> list, uint size)
        {
            DirectoryInfo d = new DirectoryInfo(dir);
            FileInfo[] files = d.GetFiles();//文件
            DirectoryInfo[] directs = d.GetDirectories();//文件夹
            foreach (FileInfo f in files)
            {
                if (f.Length >= size)
                {
                    list.Add(f.FullName);//添加文件名到列表中  
                }
            }
            //获取子文件夹内的文件列表，递归遍历  
            foreach (DirectoryInfo dd in directs)
            {
                Director(dd.FullName, list, size);
            }
        }
        //遍历所有文件夹获取特定格式的文件
        private List<string> fine_filename(string FolderPath, uint size)
        {
            DirectoryInfo theFolder = new DirectoryInfo(FolderPath);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            List<string> file = new List<string>();
            Director(FolderPath, file, size);
            return file;
        }
        private void find_file()
        {
            int i = 0;
            if (comboBox_datatype.SelectedIndex == 0)
            {
                char unit = textBox_key.Text[textBox_key.Text.Length - 1];
                uint size = 0;
                if (unit == 'M')
                {
                    size = Convert.ToUInt32(textBox_key.Text.Substring(0, textBox_key.Text.Length - 1));
                    size = size * 1024 * 1000;
                }
                else if (unit == 'K')
                {
                    size = Convert.ToUInt32(textBox_key.Text.Substring(0, textBox_key.Text.Length - 1));
                    size = size * 1024;
                }
                else
                {
                    size = Convert.ToUInt32(textBox_key.Text);  //当做没有单位处理 默认byte
                }
                List<string> file = fine_filename(source_file_textBox.Text, size);
                richTextBox_out.Text = "";
                for (i = 0; i < file.Count; i++)
                {
                    richTextBox_out.AppendText(file[i] + "\r\n");
                }
            }
            else if (comboBox_datatype.SelectedIndex == 1)
            {
                string str = "", str_dir= source_file_textBox.Text,out_dir= label_outfilename.Text;
                List<string> bmpfile = fine_filename(str_dir, this.textBox_key.Text);
                str_dir = Directory.GetParent(source_file_textBox.Text).ToString();
                if (Directory.Exists(out_dir))
                {//do nothing
                }
                else
                {
                    Directory.CreateDirectory(out_dir);
                }
                richTextBox_out.Text = "";
                for (i = 0; i < bmpfile.Count; i++)
                {
                    str = bmpfile[i];
                    str =str.Replace(str_dir ,"").Replace("\\", "");
                    File.Copy(bmpfile[i], out_dir + str, true);
                    richTextBox_out.AppendText(str + "\r\n");
                }
            }
        }
        private void statr_Process(string exe, string strInput)
        {
            Process p = new Process();
            //设置要启动的应用程序
            p.StartInfo.FileName = exe;
            //是否使用操作系统shell启动
            p.StartInfo.UseShellExecute = false;
            // 接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardInput = true;
            //输出信息
            p.StartInfo.RedirectStandardOutput = true;
            // 输出错误
            p.StartInfo.RedirectStandardError = true;
            //不显示程序窗口
            p.StartInfo.CreateNoWindow = true;
            //启动程序
            p.Start();

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(strInput + "&exit");

            p.StandardInput.AutoFlush = true;

            //获取输出信息
            string strOuput = p.StandardOutput.ReadToEnd();
            //等待程序执行完退出进程
            p.WaitForExit();
            p.Close();
        }
        private void text_handle()
        {
            int i = 0, j = 0, m;
            string orgTxt1 = textInput.Text.Trim();
            List<string> lstArray = orgTxt1.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string outTxt1 = "";
            int row_index = 0;
            if (comboBox_datatype.SelectedIndex == 0)
            {
                richTextBox_out.Text = orgTxt1.Replace("\r\n", "").Replace("\n", "");
            }
            else if ((comboBox_datatype.SelectedIndex == 1) || (comboBox_datatype.SelectedIndex == 2))
            {
                string str = textBox_key.Text;
                if (str.Length == 0)
                {
                    MessageBox.Show("关键字长度错误!");
                    return;
                }
                for (i = 0; i < lstArray.Count; i++)
                {
                    if (comboBox_datatype.SelectedIndex == 1)
                    {
                        outTxt1 += str + lstArray[i];
                    }
                    else if (comboBox_datatype.SelectedIndex == 2)
                    {
                        outTxt1 += lstArray[i].Replace("\r", str+ "\r\n");
                    }
                }
                richTextBox_out.Text = outTxt1;
            }
            else if ((comboBox_datatype.SelectedIndex == 3) || (comboBox_datatype.SelectedIndex == 4))
            {
                for (i = 0; i < lstArray.Count; i++)
                {
                    m = lstArray[i].IndexOf(textBox_key.Text);
                    if (m != -1)
                    {
                        row_index = 1;
                        if (comboBox_datatype.SelectedIndex == 3)
                        {
                            outTxt1 += lstArray[i].Substring(m, lstArray[i].Length - m);
                        }
                        else if (comboBox_datatype.SelectedIndex == 4)
                        {
                            outTxt1 += lstArray[i].Substring(0, m) + "\r\n";
                        }
                        
                    }
                }
                if (row_index == 0)
                {
                    MessageBox.Show("找不到指定的关键字!");
                    return;
                }
                richTextBox_out.Text = outTxt1;
            }
            else if (comboBox_datatype.SelectedIndex == 5)
            {
                string prefix = textBox_key.Text;
                for (i = 0; i < lstArray.Count; i++)
                {
                    m = lstArray[i].IndexOf("[");
                    if (m != -1)
                    {
                        string str = lstArray[i].Substring(0, m).Trim();
                        j = str.LastIndexOf(" ");
                        if (j != -1)
                        {
                            row_index = 1;
                            outTxt1 += prefix+str.Substring(j, str.Length-j).Trim() + ",\r\n";
                        }
                    }
                }
                if (row_index == 0)
                {
                    MessageBox.Show("找不到有效数组!");
                    return;
                }
                richTextBox_out.Text = outTxt1;
            }

            MessageBox.Show("提取完成!");
        }
        private void find_latest_file_open()
        {
            int i = 0,j=0;
            DateTime LastTime;
            FileInfo fi;
            string Path_source = Path.GetDirectoryName(source_file_textBox.Text);
            if (System.IO.Directory.Exists(Path_source) == false)   //如果存在返回值为true，如果不存在这个文件，则返回值为false
            {
                MessageBox.Show("源目录路径设置出错!");
                return;
            }
            List<string> file = fine_filename(Path_source, ".log");
            fi = new FileInfo(file[0]);
            LastTime = fi.LastWriteTime;
            for (i = 1; i < file.Count; i++)
            {
                fi = new FileInfo(file[i]);
                if (DateTime.Compare(fi.LastWriteTime, LastTime) > 0)
                {
                    LastTime = fi.LastWriteTime;
                    j = i;
                }
            }
            comboBox_indicate.Text= Path.GetFileName(file[j]);
            label_outfilename.Text = file[j];
            textInput.Text = System.IO.File.ReadAllText(file[j], Encoding.Default);
        }
        

        private void draw_Click(object sender, EventArgs e)
        {
            if (comboBox_mode.SelectedIndex == 0)
            {
                bintoarr();
            }
            else if (comboBox_mode.SelectedIndex == 1)
            {
                if (comboBox_datatype.SelectedIndex == 0)
                {
                    Git_helper();
                }
                else
                {
                    RGB_565();
                }
            }
            else if (comboBox_mode.SelectedIndex == 2)
            {
                if (comboBox_datatype.SelectedIndex == 0)
                {
                    Novel_Filtering();
                }
                else if (comboBox_datatype.SelectedIndex == 1)
                {
                    fonttxt_to_bin();
                }
            }
            else if (comboBox_mode.SelectedIndex == 3)
            {
                dsview_analysis();
            }
            else if (comboBox_mode.SelectedIndex == 4)
            {
                Chinese_to_utf8_arr();
            }
            else if (comboBox_mode.SelectedIndex == 5)
            {
                keil_memery();
            }
            else if (comboBox_mode.SelectedIndex == 6)
            {
                Data_filled_complement_zero();
            }
            else if (comboBox_mode.SelectedIndex == 7)
            {
                Data_reversal();
            }
            else if (comboBox_mode.SelectedIndex == 8)
            {
                text_to_bin();
            }
            else if (comboBox_mode.SelectedIndex == 9)
            {
                Fine_max_notuse_index();
            }
            else if (comboBox_mode.SelectedIndex == 10)
            {
                extract_rank_data();
            }
            else if (comboBox_mode.SelectedIndex == 11)
            {
                Cstruct_element_size();
            }
            else if (comboBox_mode.SelectedIndex == 12)
            {
                Data_xor();
            }
            else if (comboBox_mode.SelectedIndex == 13)
            {
                Rtc_Deviation();
            }
            else if (comboBox_mode.SelectedIndex == 14)
            {
                Bytestoutf8();
            }
            else if (comboBox_mode.SelectedIndex == 15)
            {
                Keil_Undefined_symbol_extract();
            }
            else if (comboBox_mode.SelectedIndex == 16)
            {
                Get_rom_extract();
            }
            else if (comboBox_mode.SelectedIndex == 17)
            {
                SourceInsight_SearchResults_Analysis();
            }
            else if (comboBox_mode.SelectedIndex == 18)
            {
                Get_arr();
            }
            else if (comboBox_mode.SelectedIndex == 19)
            {
                Get_api_symdef();
            }
            else if (comboBox_mode.SelectedIndex == 20)
            {
                find_file();
            }
            else if (comboBox_mode.SelectedIndex == 21)
            {
                text_handle();
            }
            else if (comboBox_mode.SelectedIndex == 22)
            {
                find_latest_file_open();
            }
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("点击确认按钮后将清除本界面数据,\r\n当前界面下的数据将会被丢失,\r\n请谨慎选择!", "是否清除数据?", MessageBoxButtons.OKCancel);

            if (dr == DialogResult.Cancel)
            {
                return;
            }
            textInput.Clear();
            richTextBox_out.Clear();
        }

        private void button_replace_Click(object sender, EventArgs e)
        {
            //int length1 = richTextBox_out.Text.Length;
            //int length2 = textInput.Text.Length;
            //if (length1 > 0)
            //{
            //    FileStream fs = new FileStream(".\\richTextBox_out.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //    StreamWriter sw = new StreamWriter(fs);
            //    sw.Write(richTextBox_out.Text);//写你的字符串。
            //    sw.Close();
            //    fs.Close();
            //}
            //if (length2 > 0)
            //    File.WriteAllText("textInput.txt", textInput.Text);

            //if (length2 > 0)
            //    richTextBox_out.Text = System.IO.File.ReadAllText("textInput.txt", Encoding.Default);
            //if (length1 > 0)
            //    textInput.Text = System.IO.File.ReadAllText("richTextBox_out.txt", Encoding.Default);
            string str = textInput.Text;
            textInput.Text = richTextBox_out.Text;
            richTextBox_out.Text = str;
        }

        private void button_reintput_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("点击确认按钮后将重新更新输入数据,\r\n当前界面下的输入框数据将会被丢失,\r\n请谨慎选择!", "是否重载数据?", MessageBoxButtons.OKCancel);

            if  (dr == DialogResult.Cancel)
            {
                return;
            }
            reintput_file(source_file_textBox.Text);
        }

        private void reintput_file(string FileName)
        {
            FileInfo fi = new FileInfo(FileName);

            if ((fi.Extension).ToLower() == ".txt")
            {
                label_outfilename.Text = source_file_textBox.Text;
                textInput.Text = System.IO.File.ReadAllText(source_file_textBox.Text, Encoding.Default);
            }
            else if (fi.Extension == ".wav")
            {
                string path = FileName;
                label_outfilename.Text = path.Replace(".WAV", string.Empty).Replace(".wav", string.Empty) + "_ok.txt";
                byte[] text = System.IO.File.ReadAllBytes(source_file_textBox.Text);
                var str = DateTime.Now.ToString();
                var encode = Encoding.UTF8;
                var hex = BitConverter.ToString(text, 0).Replace("-", string.Empty).ToLower();
                textInput.Text = hex;
            }
            else if (fi.Extension == ".bin")
            {
                string path = FileName;
                label_outfilename.Text = path.Replace(".BIN", string.Empty).Replace(".bin", string.Empty) + "_ok.txt";
                byte[] text = System.IO.File.ReadAllBytes(source_file_textBox.Text);
                var str = DateTime.Now.ToString();
                var encode = Encoding.UTF8;
                var hex = BitConverter.ToString(text, 0).Replace("-", string.Empty).ToLower();
                textInput.Text = hex;
            }
            else if (fi.Extension == ".bmp")
            {
                string path = FileName;
                label_outfilename.Text = path.Replace(".BMP", string.Empty).Replace(".bmp", string.Empty) + "_ok.txt";
                Bitmap _Bitmap = (Bitmap)Image.FromFile(source_file_textBox.Text);
                BitmapData _BitmapData = _Bitmap.LockBits(new Rectangle(0, 0, _Bitmap.Width, _Bitmap.Height), ImageLockMode.ReadWrite, _Bitmap.PixelFormat);
                byte[] _Value = new byte[_BitmapData.Stride * _BitmapData.Height];
                Marshal.Copy(_BitmapData.Scan0, _Value, 0, _Value.Length);
                //写回去还是用 Marshal.Copy.
                var str = DateTime.Now.ToString();
                var encode = Encoding.UTF8;
                var hex = BitConverter.ToString(_Value, 0).Replace("-", string.Empty).ToLower();
                textInput.Text = hex;
            }
            else if (fi.Extension == ".csv")
            {
                string path = FileName;
                label_outfilename.Text = path.Replace(".CSV", string.Empty).Replace(".csv", string.Empty) + "_ok.txt";
                //string str = OpenCSV_Getline(path,0,"\r\n");
                //textInput.Text = str;

                DataTable data = OpenCSV(path);
                StringBuilder str = new StringBuilder("", data.Rows.Count*10);
                foreach (DataRow row in data.Rows)
                {
                    str.Append(row[0] + Environment.NewLine);
                }
                textInput.Text = str.ToString();
            }
            else if ((fi.Extension).ToLower() == ".c")
            {
                label_outfilename.Text = source_file_textBox.Text;
                textInput.Text = System.IO.File.ReadAllText(source_file_textBox.Text, Encoding.Default);
            }
            else if ((fi.Extension).ToLower() == ".log")
            {
                label_outfilename.Text = source_file_textBox.Text;
                textInput.Text = System.IO.File.ReadAllText(source_file_textBox.Text, Encoding.Default);
            }
        }
        private void arr_restore_Defaults()
        {
            int i = 0;
            this.comboBox_datatype.Items.Clear();
            this.comboBox_datatype.Items.Add("uint8");
            this.comboBox_datatype.Items.Add("uint16");
            this.comboBox_datatype.Items.Add("uint32");

            this.comboBox_fonttype.Items.Clear();
            this.comboBox_fonttype.Items.Add("16X16");
            this.comboBox_fonttype.Items.Add("32X32");
            this.label_font_type.Text = "字库类型：";
            this.label_indicator.Text = "指示符:";
            this.label_data_type.Text = "生成数据类型:";
            this.label_datasize.Text = "提取数组大小:";
            this.label_key_word.Text = "关键字";
            this.source_file_button.Text = "选择文件";
            this.label_outfile.Text = "输出文件名称:";
            if (comboBox_indicate_text.Count > 0)
            {
                comboBox_indicate.Items.Clear();
                for (i = 0; i < comboBox_indicate_text.Count; i++)
                {
                    this.comboBox_indicate.Items.Add(comboBox_indicate_text[i]);
                }
            }
        }
        private void arr_restore_Defaults_Adjust()
        {
            AdjustComboBoxDropDownListWidth(comboBox_datatype);
            AdjustComboBoxDropDownListWidth(comboBox_fonttype);
            AdjustComboBoxDropDownListWidth(comboBox_indicate);
            this.comboBox_datatype.SelectedItem = 0;
            this.comboBox_datatype.SelectedIndex = 0;
            this.comboBox_fonttype.SelectedItem = 0;
            this.comboBox_fonttype.SelectedIndex = 0;
            this.comboBox_indicate.SelectedItem = 0;
            this.comboBox_indicate.SelectedIndex = 0;
        }
        private void comboBox_mode_DropDownClosed(object sender, EventArgs e)
        {
            int i = 0;
            arr_restore_Defaults();
            if (comboBox_mode.SelectedIndex == 0)
            {
                this.comboBox_fonttype.Items.Clear();
                this.comboBox_fonttype.Items.Add("输出数据无特殊处理");
                this.comboBox_fonttype.Items.Add("输出数据根据WAV格式删除前面0X28个数据（0X28-0X2B为长度，0X2C开始为有效数据）");
                this.comboBox_fonttype.Items.Add("上面的基础上,替换文件名指定文件中由数组名指定的数组内容");
                this.label_font_type.Text = "输出处理：";
            }
            else if (comboBox_mode.SelectedIndex == 1)
            {
                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("Git helper");
                this.comboBox_datatype.Items.Add("RGB palette 24BIT TO 565");
                this.label_data_type.Text = "   模式选择：";

                this.comboBox_fonttype.Items.Clear();
                this.comboBox_fonttype.Items.Add("生成git commit命令");
                this.comboBox_fonttype.Items.Add("MD文件添加行尾");
                this.label_font_type.Text = "功能选择：";
            }
            else if (comboBox_mode.SelectedIndex == 2)
            {
                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("Novel Filtering");
                this.comboBox_datatype.Items.Add("Font txt to bin");
                this.label_data_type.Text = "   模式选择：";

                this.comboBox_fonttype.Items.Clear();
                this.comboBox_fonttype.Items.Add("加密");
                this.comboBox_fonttype.Items.Add("解密");
                this.label_font_type.Text = "功能选择：";

                this.label_key_word.Text = "密码:";
            }
            else if (comboBox_mode.SelectedIndex == 3)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_dsview_analysis.txt", Encoding.Default);
            }
            else if (comboBox_mode.SelectedIndex == 4)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_text_to_utf8_ASCII.txt", Encoding.Default);

                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("数据为数组格式，第一个字节为数组长度");
                this.comboBox_datatype.Items.Add("数据为数组格式，不带数组长度");
                this.comboBox_datatype.Items.Add("数据为带空格的数据");
                this.comboBox_datatype.Items.Add("数据为不带空格的数据");
                this.label_data_type.Text = "输出格式选择：";
            }
            else if (comboBox_mode.SelectedIndex == 7)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Data_reversal.txt", Encoding.Default);

                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("以整行数据作为整体翻转高低字节,实现输入数据的X轴对称,并计算第二行-第一行");
                this.comboBox_datatype.Items.Add("以整行数据作为整体翻转高低字节,实现输入数据的X轴对称,不计算数据");
                this.comboBox_datatype.Items.Add("寻找有用行(非注释行)按字节调换大小端");
                this.label_data_type.Text = "处理功能选择：";

                this.comboBox_fonttype.Items.Clear();
                this.comboBox_fonttype.Items.Add("不带0X的数据");
                this.comboBox_fonttype.Items.Add("带0X的数据");
                this.comboBox_fonttype.Items.Add("带0X的数组");
                this.label_font_type.Text = "数据类型：";
            }
            else if (comboBox_mode.SelectedIndex == 8)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Texttobin.txt", Encoding.Default);

                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("不改变输入数据");
                this.comboBox_datatype.Items.Add("提取输入十进制数据为Ascii码");
                this.comboBox_datatype.Items.Add("提取输入十六进制数据为Ascii码");
                this.label_data_type.Text = "输入数据处理：";

                this.comboBox_fonttype.Items.Clear();
                this.comboBox_fonttype.Items.Add("不带0X的数据");
                this.comboBox_fonttype.Items.Add("带0X的数据");
                this.comboBox_fonttype.Items.Add("带0X的数组");
                this.label_font_type.Text = "数据类型：";
            }
            else if (comboBox_mode.SelectedIndex == 10)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_extract_rank_data.txt", Encoding.Default);
            }
            else if (comboBox_mode.SelectedIndex == 11)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Cstruct_element_size.txt", Encoding.Default);
            }
            else if (comboBox_mode.SelectedIndex == 12)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_dataxor.txt", Encoding.Default);
            }
            else if (comboBox_mode.SelectedIndex == 13)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Rtc_Deviation.txt", Encoding.Default);

                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("输入数据为手环RTC统计数据");
                this.comboBox_datatype.Items.Add("输入数据为sniffer数据包");
                this.comboBox_datatype.Items.Add("输入数据为Studio的时间+数据复制行");
                this.label_data_type.Text = "输入数据类型：";
            }
            else if (comboBox_mode.SelectedIndex == 14)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Bytestoutf8.txt", Encoding.Default);

                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("普通的数组转UTF8");
                this.comboBox_datatype.Items.Add("C#数组转UTF8");
                this.comboBox_datatype.Items.Add("Jlink_commander数组转UTF8");
                this.label_data_type.Text = "   模式选择：";
            }
            else if (comboBox_mode.SelectedIndex == 15)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Keil_Undefined_symbol_extract.txt", Encoding.Default);
            }
            else if (comboBox_mode.SelectedIndex == 16)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Get_Row.txt", Encoding.Default);

                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("提取行号从第一行以关键字为准");
                this.comboBox_datatype.Items.Add("提取行号由关键字指定");
                this.label_data_type.Text = "提取行号来源：";
                this.label_datasize.Text = "      分隔符:";
                this.comboBox_fonttype.Items.Clear();
                this.comboBox_fonttype.Items.Add("输出以空格符隔开");
                this.comboBox_fonttype.Items.Add("输出以换行符符隔开");
                this.comboBox_fonttype.Items.Add("输出无间隙连接");
                this.label_font_type.Text = "输出格式：";
                this.label_indicator.Text = "附操作:";

                comboBox_indicate_text.Clear();
                for (i = 0; i < comboBox_indicate.Items.Count; i++)
                {
                    comboBox_indicate_text.Add(comboBox_indicate.Items[i].ToString());
                }
                comboBox_indicate.Items.Clear();
                this.comboBox_indicate.Items.Add("无额外操作");
                this.comboBox_indicate.Items.Add("BLE估算下一个连接事件的时间(两个主机数据包之间)");
            }
            else if (comboBox_mode.SelectedIndex == 17)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_SourceInsight_SearchResults_Analysis.txt", Encoding.Default);
            }
            else if (comboBox_mode.SelectedIndex == 18)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Get_ARR.txt", Encoding.Default);

                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("数据在00-FF自增");
                this.comboBox_datatype.Items.Add("数据在00-FF;01-FF;....FE-FF;00-FF自增");
                this.comboBox_datatype.Items.Add("数据全是关键字定义的值");
                this.comboBox_datatype.Items.Add("数据由输入框转为ASCII码然后填充关键字定义的值");
                this.comboBox_datatype.Items.Add("在上面基础上输入框前面两个数据为十进制数");

                textBox_key.Text = "0xFF";
            }
            else if (comboBox_mode.SelectedIndex == 19)
            {
                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_api_symdef.txt", Encoding.Default);

                this.label_indicator.Text = "源目录:";
                comboBox_indicate.Items.Clear();
                this.comboBox_indicate.Items.Add("E:\\SYD8802\\SYD8812Git\\rom");
                this.comboBox_indicate.Items.Add("E:\\SYD8802\\SYD8812Git\\rom\\app");
                this.comboBox_indicate.Items.Add("E:\\SYD8802\\SYD8812Git\\rom\\app\\src");
                this.comboBox_indicate.Items.Add("E:\\SYD8802\\SYD8812Git");
                this.comboBox_indicate.Items.Add("E:\\SYD8802\\SYD8812Git\\rom\\Objects\\rom_api_symdef_ok.h");
                this.comboBox_indicate.Items.Add("E:\\SYD8802\\SYD8812Git\\rom\\Objects\\rom_api_symdef.h");

                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("刷新地址");
                this.comboBox_datatype.Items.Add("从源码中更新接口");
                this.label_data_type.Text = "数据处理方式：";
            }
            else if (comboBox_mode.SelectedIndex == 20)
            {
                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("找出大于等于临界值的文件");
                this.comboBox_datatype.Items.Add("按照目录加文件名拷贝到输出目录");
                this.label_data_type.Text = "  要做的工作：";
                this.label_key_word.Text = "临界值";
                this.source_file_button.Text = "选择目录";
                this.label_outfile.Text = "输出目录名称:";
                this.textBox_key.Text = "50M";
            }
            else if (comboBox_mode.SelectedIndex == 21)
            {
                this.comboBox_datatype.Items.Clear();
                this.comboBox_datatype.Items.Add("合并所有行的数据");
                this.comboBox_datatype.Items.Add("把关键字加到每一行前面");
                this.comboBox_datatype.Items.Add("把关键字加到每一行后面");
                this.comboBox_datatype.Items.Add("提取关键字指定的关键字后面的字符到本行结束");
                this.comboBox_datatype.Items.Add("提取关键字指定的关键字前面的字符到本行开始");
                this.comboBox_datatype.Items.Add("提取数组名称并加入关键字指定的前导和逗号行尾");
                this.label_data_type.Text = " 处理功能选择：";

                textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_TEXT_handle_and_analysis.txt", Encoding.Default);
            }
            else
            {
                //arr_restore_Defaults();
            }
            arr_restore_Defaults_Adjust();  //自动调整显示长度
        }
        private void comboBox_datatype_DropDownClosed(object sender, EventArgs e)
        {
            if (comboBox_mode.SelectedIndex == 14)
            {
                if (comboBox_datatype.SelectedIndex == 0)
                {
                    textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Bytestoutf8.txt", Encoding.Default);
                }
                else if (comboBox_datatype.SelectedIndex == 1)
                {
                    textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Bytestoutf8_c#.txt", Encoding.Default);
                }
                else
                {
                    textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Bytestoutf8_jlinkcommander.txt", Encoding.Default);
                }
            }
            else if (comboBox_mode.SelectedIndex == 19)
            {
                if (comboBox_datatype.SelectedIndex == 0)
                {
                    this.label_indicator.Text = "源文件:";
                }
                else if (comboBox_datatype.SelectedIndex == 1)
                {
                    this.label_indicator.Text = "源目录:";
                }
                else
                {
                    this.label_indicator.Text = "指示符:";
                }
            }
            else if (comboBox_mode.SelectedIndex == 13)
            {
                if (comboBox_datatype.SelectedIndex == 0)
                {
                    this.label_indicator.Text = "天数差:";
                    comboBox_indicate.Items.Clear();
                    this.comboBox_indicate.Items.Add("0");
                    this.comboBox_indicate.SelectedIndex = 0;
                }     
                else
                {
                    this.label_indicator.Text = "指示符:";
                }
                if (comboBox_datatype.SelectedIndex == 2)
                {
                    textInput.Text = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\default\\default_Rtc_Deviation_studio.txt", Encoding.Default);
                }
                
            }
            else if (comboBox_mode.SelectedIndex == 20)
            {
                if (comboBox_datatype.SelectedIndex == 1)
                {
                    this.label_key_word.Text = "扩展名";
                    this.textBox_key.Text = ".bmp";
                }
                else
                {
                    this.label_key_word.Text = "临界值";
                    this.textBox_key.Text = "50M";
                }
            }
            else
            {
                if ((comboBox_mode.SelectedIndex == 1) || (comboBox_mode.SelectedIndex == 2))
                { 
                        if (comboBox_datatype.SelectedIndex == 1)
                        {
                            arr_restore_Defaults();
                            arr_restore_Defaults_Adjust();
                        }
                }
            }
        }

        private void comboBox_fonttype_DropDownClosed(object sender, EventArgs e)
        {
            if ((comboBox_mode.SelectedIndex == 0) && ((comboBox_fonttype.SelectedIndex == 1) || (comboBox_fonttype.SelectedIndex == 2)))
            {
                this.label_key_word.Text = "数组名:";
                this.label_indicator.Text = "文件名:";
            }
            else
            {
                //arr_restore_Defaults();
                //arr_restore_Defaults_Adjust();
            }
        }
        private void comboBox_indicate_DropDownClosed(object sender, EventArgs e)
        {
            if (comboBox_mode.SelectedIndex == 16)
            {
                textBox_filesize.Text = "	";
                this.comboBox_datatype.SelectedItem = 1;
                this.comboBox_datatype.SelectedIndex = 1;
                this.comboBox_fonttype.SelectedItem = 1;
                this.comboBox_fonttype.SelectedIndex = 1;
                this.textBox_key.Text = "14";
            }
            else
            {

            }
        }

        private void textInput_TextChanged(object sender, EventArgs e)
        {
            if (comboBox_mode.SelectedIndex == 10)
            {
                List<string> lis = new List<string>();
                string str = new Regex("[\\s]+").Replace(textInput.Lines[0].ToString(), " ").Trim();
                lis = str.Split(new string[] { " " }, StringSplitOptions.None).ToList();
                if (lis.Count<20)
                    comboBox_indicate.DataSource = lis;
            }
        }

        private void draw_MouseEnter(object sender, EventArgs e)
        {
            p_draw.ShowAlways = false;
            if (comboBox_mode.SelectedIndex == 13)
            {
                p_draw.SetToolTip(this.draw, "点击该按钮提取数据，在这个模式下：\r\n\"提取数组大小(Byte):\"输入框作为最后一行时间戳差值输出\r\n\"关键字:\"输入框作为最后一行时间差值输出\r\n\"指示符:\"输入框作为天数差值输入");
            }
            else
            {
                p_draw.SetToolTip(this.draw, "点击该按钮提取数据");
            }
        }
    }
}

