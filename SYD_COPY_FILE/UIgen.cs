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
using System.Runtime.InteropServices;

namespace SYD_COPY_FILE
{
    public partial class Form1
    {
        #region Define
        //Arithmetic.dll
        [System.Runtime.InteropServices.DllImport("Arithmetic.dll", EntryPoint = "Rle_Decode_O", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Rle_Decode_O(byte[] inbuf, int inSize, [Out(), MarshalAs(UnmanagedType.LPArray)] byte[] outbuf, int onuBufSize);

        [System.Runtime.InteropServices.DllImport("Arithmetic.dll", EntryPoint = "Rle_Encode_O", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Rle_Encode_O(byte[] inbuf, int inSize, [Out(), MarshalAs(UnmanagedType.LPArray)] byte[] outbuf, int onuBufSize);

        public string[] Input_FileNames;

        public bool Output_Display_Extern = false;
        public string path = null;

        #endregion

        public void UIgen_init()
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "syd_24k_ok.bin";
            UIgen_label2.Text = path;

            AdjustComboBoxDropDownListWidth(UIgen_comboBox1);
            AdjustComboBoxDropDownListWidth(UIgen_comboBox2);
            AdjustComboBoxDropDownListWidth(UIgen_comboBox3);
            AdjustComboBoxDropDownListWidth(UIgen_comboBox4);
            AdjustComboBoxDropDownListWidth(UIgen_comboBox5);
            AdjustComboBoxDropDownListWidth(UIgen_comboBox6);

            UIgen_source_file_textBox.Text= Settings1.Default.UIgen_source_file_textBox;
            UIgen_comboBox1.SelectedIndex = Settings1.Default.UIgen_comboBox1;
            UIgen_checkBox5.Checked = Settings1.Default.UIgen_checkBox5;
            UIgen_comboBox2.SelectedIndex = Settings1.Default.UIgen_comboBox2;
            UIgen_comboBox3.SelectedIndex = Settings1.Default.UIgen_comboBox3;
            UIgen_comboBox4.SelectedIndex = Settings1.Default.UIgen_comboBox4;
            UIgen_comboBox5.SelectedIndex = Settings1.Default.UIgen_comboBox5;
            UIgen_comboBox6.SelectedIndex = Settings1.Default.UIgen_comboBox6;
            UIgen_checkBox2.Checked = Settings1.Default.UIgen_checkBox2;
            UIgen_checkBox3.Checked = Settings1.Default.UIgen_checkBox3;
            UIgen_checkBox1.Checked = Settings1.Default.UIgen_checkBox1;
            UIgen_checkBox4.Checked = Settings1.Default.UIgen_checkBox4;
        }
        public void UIgen_uninit()
        {
            Settings1.Default.UIgen_source_file_textBox= UIgen_source_file_textBox.Text;
            Settings1.Default.UIgen_comboBox1= UIgen_comboBox1.SelectedIndex;
            Settings1.Default.UIgen_checkBox5= UIgen_checkBox5.Checked;
            Settings1.Default.UIgen_comboBox2= UIgen_comboBox2.SelectedIndex;
            Settings1.Default.UIgen_comboBox3= UIgen_comboBox3.SelectedIndex;
            Settings1.Default.UIgen_comboBox4= UIgen_comboBox4.SelectedIndex;
            Settings1.Default.UIgen_comboBox5= UIgen_comboBox5.SelectedIndex;
            Settings1.Default.UIgen_comboBox6= UIgen_comboBox6.SelectedIndex;
            Settings1.Default.UIgen_checkBox2= UIgen_checkBox2.Checked;
            Settings1.Default.UIgen_checkBox3= UIgen_checkBox3.Checked;
            Settings1.Default.UIgen_checkBox1= UIgen_checkBox1.Checked;
            Settings1.Default.UIgen_checkBox4= UIgen_checkBox4.Checked;
        }
        private void UIgen_source_file_button_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Multiselect = true;

            dlg.FileName = "source_file";

            dlg.DefaultExt = ".txt";

            dlg.Filter = "txt file (.txt)|*.txt|C file (.c)|*.c|ebm file (.ebm)|*.ebm|bin file (.bin)|*.bin";

            if (dlg.ShowDialog() == false)
                return;
            UIgen_source_file_textBox.Text = dlg.FileName;
            FileInfo fi = new FileInfo(dlg.FileName);

            if ((fi.Extension).ToLower() == ".txt")
            {
                UIgen_label2.Text = UIgen_source_file_textBox.Text.Replace(".txt", string.Empty).Replace(".TXT", string.Empty) + "_ok.bin";
            }
            else if ((fi.Extension).ToLower() == ".c")
            {
                UIgen_label2.Text = UIgen_source_file_textBox.Text.Replace(".c", string.Empty).Replace(".C", string.Empty) + "_ok.bin";
            }
            else if ((fi.Extension).ToLower() == ".ebm")
            {
                UIgen_label2.Text = UIgen_source_file_textBox.Text.Replace(".ebm", string.Empty).Replace(".EBM", string.Empty) + "_ok.bin";
            }
            else if ((fi.Extension).ToLower() == ".bin")
            {
                UIgen_label2.Text = UIgen_source_file_textBox.Text.Replace(".bin", string.Empty).Replace(".bin", string.Empty) + "_ok.bin";
            }

            string str = "";

            UIgen_progressBar1.Value = 0;

            if (((fi.Extension).ToLower() == ".ebm") || ((fi.Extension).ToLower() == ".bin"))
            {
                for (int num = 0; num < dlg.FileNames.Length; num++)
                {
                    FileStream myFile = File.Open(dlg.FileNames[num].ToString(), FileMode.Open, FileAccess.Read);
                    BinaryReader myReader = new BinaryReader(myFile);

                    UInt32 Length = (UInt32)System.Convert.ToInt32(myFile.Length);
                    byte[] Bin = myReader.ReadBytes((int)Length);

                    myReader.Close();
                    myFile.Close();

                    str += "const unsigned char " + System.IO.Path.GetFileNameWithoutExtension(dlg.FileNames[num].ToString()) + "[] = { \r\n";
                    str += byteToHexText(Bin);
                    str += "};\r\n";

                    UIgen_progressBar1.Value = (int)((double)num / ((double)dlg.FileNames.Length) * 100);
                }
            }
            else
            {
                for (int num = 0; num < dlg.FileNames.Length; num++)
                {
                    str += System.IO.File.ReadAllText(dlg.FileNames[num].ToString());

                    UIgen_progressBar1.Value = (int)((double)num / ((double)dlg.FileNames.Length) * 100);
                }
            }
            UIgen_progressBar1.Value = 100;

            UIgen_textInput.Text = str;
            Input_FileNames = dlg.FileNames;
        }
        private void UIgen_source_file_textBox_DragEnter(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;
            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void UIgen_source_file_textBox_DragDrop(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;

            if (dataObject == null) return;

            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);

                foreach (string file in files)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    if (sender == UIgen_source_file_textBox)
                        this.UIgen_source_file_textBox.Text = fi.FullName;
                }
            }
        }
        private List<string> HoverTreeGetFistLine(string input)
        {
            List<string> commentList = new List<string>();
            int i = 0, j = 0, k = 0, z = 0;
            bool have_comment = false;
            string str = "";
            while (true)
            {
                have_comment = false;

                i = input.IndexOf("const", z);

                if (i != -1)
                {
                    k = input.IndexOf("\n", i + 5);
                    if (k != -1)
                    {
                        j = input.IndexOf("/*", i + 5, k - i - 5);

                        if (j != -1)
                        {
                            z = input.IndexOf("*/", j + 2, k - j - 2);

                            if (z != -1)
                            {
                                str = input.Substring(j, z + 2 - j);
                                commentList.Add(str);

                                have_comment = true;
                            }
                        }
                    }

                    if (have_comment == false)
                    {
                        z = i + 5;
                        str = "";
                        commentList.Add(str);
                    }
                }
                else
                    break;

            }

            return commentList;
        }
        private List<string> HoverTreeGetDataFistLine(ref string input)
        {
            List<string> commentList = new List<string>();
            int i = 0, j = 0, k = 0, z = 0;
            bool have_comment = false;
            string str = "";
            while (true)
            {
                have_comment = false;

                i = input.IndexOf("const", z);

                if (i != -1)
                {
                    k = input.IndexOf("\n", i + 5);
                    if (k != -1)
                    {
                        j = input.IndexOf("{", i + 5, k - i - 5);

                        if (j != -1)
                        {
                            z = k;
                            str = input.Substring(j + 1, z - j - 1);
                            commentList.Add(str);
                            input = input.Remove(j + 1, z - j - 1);
                            have_comment = true;
                        }
                    }

                    if (have_comment == false)
                    {
                        z = i + 5;
                        str = "";
                        commentList.Add(str);
                    }
                }
                else
                    break;

            }

            return commentList;
        }
        public byte charToHexByte(StringBuilder bytes, int index)
        {
            byte returnByte = 0;
            char num1 = bytes[index];
            char num2 = bytes[index + 1];

            if (num1 >= 0x61)
            {
                num1 = (char)(num1 - 87);
            }
            else if (num1 >= 0x41)
            {
                num1 = (char)(num1 - 55);
            }
            else if (num1 >= 0x30)
            {
                num1 = (char)(num1 - 0x30);
            }


            if (num2 >= 0x61)
            {
                num2 = (char)(num2 - 87);
            }
            else if (num2 >= 0x41)
            {
                num2 = (char)(num2 - 55);
            }
            else if (num2 >= 0x30)
            {
                num2 = (char)(num2 - 0x30);
            }

            returnByte = (byte)((byte)num1 * 16 + (byte)num2);

            return returnByte;
        }

        private void UIgen_draw_Click(object sender, EventArgs e)
        {
            UIgen_progressBar1.Value = 0;

            UIgen_progressBar1.Value = 2;

            string orgTxt1 = UIgen_textInput.Text.Trim();

            List<string> commentList = new List<string>();

            UIgen_progressBar1.Value = 5;

            if (UIgen_comboBox2.SelectedIndex == 1)
            {
                commentList = HoverTreeGetFistLine(orgTxt1);
            }
            else if (UIgen_comboBox2.SelectedIndex == 2)
            {
                commentList = HoverTreeGetDataFistLine(ref orgTxt1);
            }

            UIgen_progressBar1.Value = 10;

            int[] width_list = new int[commentList.Count];
            int[] hight_list = new int[commentList.Count];

            if (UIgen_comboBox2.SelectedIndex == 1)
            {
                string str = "";
                int scan = 0, width = 0, hight = 0;


                for (int m = 0; m < commentList.Count; m++)
                {
                    str = commentList[m];
                    scan = Convert.ToInt32(str.Substring(8, 4), 16);
                    if ((scan & 0x10) == 0x10)
                    {
                        //width = Convert.ToInt32(str.Substring(13, 4), 16) | Convert.ToInt32(str.Substring(18, 4), 16) << 8;
                        //hight = Convert.ToInt32(str.Substring(23, 4), 16) | Convert.ToInt32(str.Substring(28, 4), 16) << 8;
                        width = Convert.ToInt32(str.Substring(18, 4), 16) | Convert.ToInt32(str.Substring(13, 4), 16) << 8;
                        hight = Convert.ToInt32(str.Substring(28, 4), 16) | Convert.ToInt32(str.Substring(23, 4), 16) << 8;
                    }
                    else
                    {
                        width = Convert.ToInt32(str.Substring(18, 4), 16) | Convert.ToInt32(str.Substring(13, 4), 16) << 8;
                        hight = Convert.ToInt32(str.Substring(28, 4), 16) | Convert.ToInt32(str.Substring(23, 4), 16) << 8;
                    }
                    width_list[m] = width;
                    hight_list[m] = hight;
                }

                if (UIgen_checkBox2.Checked == true)
                {
                    for (int m = 0; m < commentList.Count; m++)
                    {
                        width = width_list[m];
                        hight = hight_list[m];
                        str = "Image2Lcd width:" + width.ToString() + " hight:" + hight.ToString();
                        if (UIgen_checkBox4.Checked == true)
                        {
                            //bit-01(00:黑白图, 01:256色图, 10:16位全彩图, 11:保留)   
                            str = str + " ,picture type:";
                            //bit-01(00:黑白图, 01:256色图, 10:16位全彩图, 11:保留)   
                            if (UIgen_comboBox6.SelectedIndex == 0)
                            {
                                str = str + " 'black and white',";
                            }
                            else if (UIgen_comboBox6.SelectedIndex == 1)
                            {
                                str = str + " 256 color,";
                            }
                            else if (UIgen_comboBox6.SelectedIndex == 2)
                            {
                                str = str + " 16 bit colours,";
                            }

                            //bit-23(00:未压缩, 01:RLE压缩,  10:tools压缩,  11:保留)
                            if (UIgen_comboBox5.SelectedIndex == 0)//未压缩
                            {
                                str = str + " Ununcompression";
                            }
                            else if (UIgen_comboBox5.SelectedIndex == 1)//RLE压缩
                            {
                                str = str + " RLE Compression ";
                            }
                            else if (UIgen_comboBox5.SelectedIndex == 2)//RLE tools压缩
                            {
                                str = str + " RLE Tools Compression ";
                            }
                        }
                        commentList[m] = commentList[m].Insert(commentList[m].Length - 2, str);
                    }
                }
                else
                {
                    if (UIgen_checkBox4.Checked == true)
                    {
                        for (int m = 0; m < commentList.Count; m++)
                        {
                            str = "type:";

                            //bit-01(00:黑白图, 01:256色图, 10:16位全彩图, 11:保留)   
                            if (UIgen_comboBox6.SelectedIndex == 0)
                            {
                                str = str + " 'black and white',";
                            }
                            else if (UIgen_comboBox6.SelectedIndex == 1)
                            {
                                str = str + " 256 color,";
                            }
                            else if (UIgen_comboBox6.SelectedIndex == 2)
                            {
                                str = str + " 16 bit colours,";
                            }

                            //bit-23(00:未压缩, 01:RLE压缩,  10:tools压缩,  11:保留)
                            if (UIgen_comboBox5.SelectedIndex == 0)//未压缩
                            {
                                str = str + " Ununcompression";
                            }
                            else if (UIgen_comboBox5.SelectedIndex == 1)//RLE压缩
                            {
                                str = str + " RLE Compression ";
                            }
                            else if (UIgen_comboBox5.SelectedIndex == 2)//RLE tools压缩
                            {
                                str = str + " RLE Tools Compression ";
                            }

                            commentList[m] = commentList[m].Insert(commentList[m].Length - 2, str);
                        }
                    }
                }
            }
            else if (UIgen_comboBox2.SelectedIndex == 2)
            {
                string str = "";
                int scan = 0, width = 0, hight = 0;
                for (int m = 0; m < commentList.Count; m++)
                {
                    str = commentList[m];
                    scan = Convert.ToInt32(str.Substring(1, 4), 16);
                    if ((scan & 0x10) == 0x10)
                    {
                        width = Convert.ToInt32(str.Substring(16, 4), 16) | Convert.ToInt32(str.Substring(11, 4), 16) << 8;
                        hight = Convert.ToInt32(str.Substring(26, 4), 16) | Convert.ToInt32(str.Substring(21, 4), 16) << 8;
                    }
                    else
                    {
                        width = Convert.ToInt32(str.Substring(11, 4), 16) | Convert.ToInt32(str.Substring(16, 4), 16) << 8;
                        hight = Convert.ToInt32(str.Substring(21, 4), 16) | Convert.ToInt32(str.Substring(26, 4), 16) << 8;
                    }
                    width_list[m] = width;
                    hight_list[m] = hight;
                }

                if (UIgen_checkBox2.Checked == true)
                {
                    for (int m = 0; m < commentList.Count; m++)
                    {
                        width = width_list[m];
                        hight = hight_list[m];
                        str = "Image2Lcd width:" + width.ToString() + " hight:" + hight.ToString();
                        if (UIgen_checkBox4.Checked == true)
                        {
                            str = str + " type:";
                            //bit-01(00:黑白图, 01:256色图, 10:16位全彩图, 11:保留)   
                            if (UIgen_comboBox6.SelectedIndex == 0)
                            {
                                str = str + " 'black and white',";
                            }
                            else if (UIgen_comboBox6.SelectedIndex == 1)
                            {
                                str = str + " 256 color,";
                            }
                            else if (UIgen_comboBox6.SelectedIndex == 2)
                            {
                                str = str + " 16 bit colours,";
                            }

                            //bit-23(00:未压缩, 01:RLE压缩,  10:tools压缩,  11:保留)
                            if (UIgen_comboBox5.SelectedIndex == 0)//未压缩
                            {
                                str = str + " Ununcompression";
                            }
                            else if (UIgen_comboBox5.SelectedIndex == 1)//RLE压缩
                            {
                                str = str + " RLE Compression ";
                            }
                            else if (UIgen_comboBox5.SelectedIndex == 2)//RLE tools压缩
                            {
                                str = str + " RLE Tools Compression ";
                            }
                        }
                        commentList[m] = "/*" + commentList[m] + str + "*/";
                    }
                }
                else
                {
                    if (UIgen_checkBox4.Checked == true)
                    {
                        for (int m = 0; m < commentList.Count; m++)
                        {
                            str = "type:";

                            //bit-01(00:黑白图, 01:256色图, 10:16位全彩图, 11:保留)   
                            if (UIgen_comboBox6.SelectedIndex == 0)
                            {
                                str = str + " 'black and white',";
                            }
                            else if (UIgen_comboBox6.SelectedIndex == 1)
                            {
                                str = str + " 256 color,";
                            }
                            else if (UIgen_comboBox6.SelectedIndex == 2)
                            {
                                str = str + " 16 bit colours,";
                            }

                            //bit-23(00:未压缩, 01:RLE压缩,  10:tools压缩,  11:保留)
                            if (UIgen_comboBox5.SelectedIndex == 0)//未压缩
                            {
                                str = str + " Ununcompression";
                            }
                            else if (UIgen_comboBox5.SelectedIndex == 1)//RLE压缩
                            {
                                str = str + " RLE Compression ";
                            }
                            else if (UIgen_comboBox5.SelectedIndex == 2)//RLE tools压缩
                            {
                                str = str + " RLE Tools Compression ";
                            }

                            commentList[m] = commentList[m].Insert(commentList[m].Length - 2, str);
                        }
                    }
                }
            }

            UIgen_progressBar1.Value = 20;

            orgTxt1 = HoverTreeClearMark(orgTxt1);
            int errorCount = 0;

            List<string> nameList = new List<string>();
            List<string> externList = new List<string>();

            orgTxt1 = orgTxt1.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(",", "").Replace("\r\n", "");
            List<string> lstArray = orgTxt1.Split(new char[1] { ';' }).ToList();
            lstArray.Remove("");

            foreach (string str in lstArray)
            {
                string str1 = str, str2 = "";
                try
                {
                    int i = str1.IndexOf("[");
                    int j = str1.IndexOf("]");
                    if (((i + 1) != j) & (j > i))
                    {
                        str1 = str1.Remove(i + 1, j - i - 1);
                    }

                    i = str1.IndexOf("const");
                    j = str1.IndexOf("=");
                    if (UIgen_comboBox3.SelectedIndex == 1)
                    {
                        str2 = str1.Substring(i, j - i).Replace(" gImage_", " ");
                    }
                    else if (UIgen_comboBox3.SelectedIndex == 2)
                    {
                        str2 = str1.Substring(i, j - i).Replace(" char ", " char __attribute__((aligned(4))) ");
                    }
                    else
                    {
                        str2 = str1.Substring(i, j - i);
                    }
                    nameList.Add(str2);
                    externList.Add("extern " + str2 + ";");
                    errorCount++;
                }
                catch
                {
                    MessageBox.Show("出错位置第" + errorCount.ToString() + "个数组");
                    return;
                }
            }

            UIgen_progressBar1.Value = 25;

            UInt32[] offset = new UInt32[lstArray.Count];
            UInt32[] size = new UInt32[lstArray.Count];
            UInt16 ii = 0;
            for (int m = 0; m < lstArray.Count; m++)
            {
                lstArray[m] = lstArray[m].Replace("0X", "").Replace("0x", "");
                lstArray[m] = lstArray[m].Replace(" ", "");
            }
            foreach (string str in lstArray)
            {
                int m = str.IndexOf("{");
                int n = str.IndexOf("}");
                size[ii++] = (UInt32)((n - m - 1) / 2);

            }
            for (int m = 0; m < lstArray.Count; m++)
                for (int k = 0; k < m; k++)
                    offset[m] += size[k];

            for (int m = 0; m < lstArray.Count; m++)
            {
                int i = lstArray[m].IndexOf('{');
                lstArray[m] = lstArray[m].Remove(0, i + 1);
            }

            for (int m = 0; m < lstArray.Count; m++)
            {
                lstArray[m] = lstArray[m].Remove(lstArray[m].Length - 1);
            }

            UIgen_progressBar1.Value = 30;

            byte[][] byteArrayencode = new byte[lstArray.Count][];  //320000

            if ((UIgen_comboBox5.SelectedIndex == 1) || (UIgen_comboBox5.SelectedIndex == 2))
            {
                StringBuilder strb = new StringBuilder(320000);
                for (int m = 0; m < lstArray.Count; m++)
                {
                    strb.Clear();
                    strb.Append(lstArray[m]);
                    int len = lstArray[m].Length / 2;
                    byte[] byteArray = new byte[len];
                    byteArrayencode[m] = new byte[len * 2];
                    for (int j = 0; j < len; j++)
                    {
                        byteArray[j] = charToHexByte(strb, j * 2);
                    }
                    int encode_size = Rle_Encode_O(byteArray, len, byteArrayencode[m], 240 * 240 * 2);
                    if (UIgen_comboBox5.SelectedIndex == 1)
                    {
                        if (encode_size >= (len * 2))
                        {
                            MessageBox.Show("压缩出错位置第" + (m + 1).ToString() + "个数组 encode_size:" + encode_size.ToString() + " len:" + len.ToString() + nameList[m]);
                            return;
                        }
                        size[m] = (uint)encode_size;
                    }
                    else if (UIgen_comboBox5.SelectedIndex == 2)
                    {
                        if (encode_size >= len)
                        {
                            Array.Copy(byteArray, byteArrayencode[m], len);
                            size[m] = (uint)len;
                        }
                        else
                        {
                            size[m] = (uint)encode_size;
                        }
                    }
                    //lstArray[m] = byteToHexStr(byteArrayencode[m], 0, encode_size);

                    if (m == 0) offset[m] = 0;
                    else offset[m] = offset[m - 1] + size[m - 1];

                    UIgen_progressBar1.Value = (int)((double)m / ((double)lstArray.Count) * 40) + 30;
                }

            }

            path = UIgen_label2.Text;
            path = path.Replace(".bin", string.Empty).Replace(".BIN", string.Empty);
            path = path + ".txt";
            using (FileStream fsWrite = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = null;
                string str = "";
                for (int i = 0; i < lstArray.Count; i++)
                {
                    if (UIgen_comboBox1.SelectedIndex == 0)
                    {
                        if (UIgen_checkBox5.Checked == true)
                            str = (nameList[i] + "={0x" + ((byte)size[i]).ToString("X2") + ",0x" + (size[i] >> 8).ToString("X2") + ",0x" + ((byte)offset[i]).ToString("X2") + ",0x" + (offset[i] >> 8).ToString("X2"));
                        else
                            str = (nameList[i] + "={0x" + ((byte)offset[i]).ToString("X2") + ",0x" + (offset[i] >> 8).ToString("X2"));
                    }
                    else if (UIgen_comboBox1.SelectedIndex == 1)
                    {
                        if (UIgen_checkBox5.Checked == true)
                            str = (nameList[i] + "={0x" + ((byte)size[i]).ToString("X2") + ",0x" + (size[i] >> 8).ToString("X2") + ",0x" + ((byte)offset[i]).ToString("X2") + ",0x" + ((byte)(offset[i] >> 8)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 16)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 24)).ToString("X2"));
                        else
                            str = (nameList[i] + "={0x" + ((byte)offset[i]).ToString("X2") + ",0x" + ((byte)(offset[i] >> 8)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 16)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 24)).ToString("X2"));
                    }
                    else if (UIgen_comboBox1.SelectedIndex == 2)
                    {
                        if (UIgen_checkBox5.Checked == true)
                            str = (nameList[i] + "={0x" + ((byte)(size[i] / 2)).ToString("X2") + ",0x" + ((size[i] / 2) >> 8).ToString("X2") + ",0x" + ((byte)offset[i]).ToString("X2") + ",0x" + ((byte)(offset[i] >> 8)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 16)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 24)).ToString("X2"));
                        else
                            str = (nameList[i] + "={0x" + ",0x" + ((byte)offset[i]).ToString("X2") + ",0x" + ((byte)(offset[i] >> 8)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 16)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 24)).ToString("X2"));
                    }
                    else if (UIgen_comboBox1.SelectedIndex == 3)
                    {
                        if (UIgen_checkBox5.Checked == true)
                            str = (nameList[i] + "={0x" + ((byte)size[i]).ToString("X2") + ",0x" + ((byte)(size[i] >> 8)).ToString("X2") + ",0x" + ((byte)(size[i] >> 16)).ToString("X2") + ",0x" + ((byte)(size[i] >> 24)).ToString("X2") + ",0x" + ((byte)offset[i]).ToString("X2") + ",0x" + ((byte)(offset[i] >> 8)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 16)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 24)).ToString("X2"));
                        else
                            str = (nameList[i] + "={0x" + ((byte)offset[i]).ToString("X2") + ",0x" + ((byte)(offset[i] >> 8)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 16)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 24)).ToString("X2"));
                    }
                    else if (UIgen_comboBox1.SelectedIndex == 4)
                    {
                        if (UIgen_checkBox5.Checked == true)
                            str = (nameList[i] + "={0x" + ((byte)offset[i]).ToString("X2") + ",0x" + ((byte)(offset[i] >> 8)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 16)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 24)).ToString("X2") + ",0x" + ((byte)size[i]).ToString("X2") + ",0x" + ((byte)(size[i] >> 8)).ToString("X2") + ",0x" + ((byte)(size[i] >> 16)).ToString("X2") + ",0x" + ((byte)(size[i] >> 24)).ToString("X2"));
                        else
                            str = (nameList[i] + "={0x" + ((byte)offset[i]).ToString("X2") + ",0x" + ((byte)(offset[i] >> 8)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 16)).ToString("X2") + ",0x" + ((byte)(offset[i] >> 24)).ToString("X2"));
                    }

                    if (UIgen_checkBox3.Checked == true)
                    {
                        if (UIgen_comboBox4.SelectedIndex == 0)
                            str = str + ",0x" + (width_list[i] & 0xff).ToString("X2") + ",0x" + (hight_list[i] & 0xff).ToString("X2");
                        else if (UIgen_comboBox4.SelectedIndex == 1)
                            str = str + ",0x" + (width_list[i] & 0xff).ToString("X2") + ",0x" + ((width_list[i] >> 8) & 0xff).ToString("X2") + ",0x" + (hight_list[i] & 0xff).ToString("X2") + ",0x" + ((hight_list[i] >> 8) & 0xff).ToString("X2");
                        else if (UIgen_comboBox4.SelectedIndex == 2)
                            str = str + ",0x" + (hight_list[i] & 0xff).ToString("X2") + ",0x" + (width_list[i] & 0xff).ToString("X2");
                        else if (UIgen_comboBox4.SelectedIndex == 3)
                            str = str + ",0x" + (hight_list[i] & 0xff).ToString("X2") + ",0x" + ((hight_list[i] >> 8) & 0xff).ToString("X2") + ",0x" + (width_list[i] & 0xff).ToString("X2") + ",0x" + ((width_list[i] >> 8) & 0xff).ToString("X2");
                    }

                    if (UIgen_checkBox4.Checked == true)
                    {
                        int picture_type = 0;

                        //bit-01(00:黑白图, 01:256色图, 10:16位全彩图, 11:保留)   
                        if (UIgen_comboBox6.SelectedIndex == 0)
                        {
                            picture_type = 0x00;
                        }
                        else if (UIgen_comboBox6.SelectedIndex == 1)
                        {
                            picture_type = 0x01;
                        }
                        else if (UIgen_comboBox6.SelectedIndex == 2)
                        {
                            picture_type = 0x02;
                        }

                        //bit-23(00:未压缩, 01:RLE压缩,  10:tools压缩,  11:保留)
                        if (UIgen_comboBox5.SelectedIndex == 0)//未压缩
                        {
                            picture_type = picture_type & 0xF3;
                        }
                        else if (UIgen_comboBox5.SelectedIndex == 1)//RLE压缩
                        {
                            picture_type = (picture_type & 0xF3) | 0x04;
                        }
                        else if (UIgen_comboBox5.SelectedIndex == 2)//RLE tools压缩
                        {
                            picture_type = (picture_type & 0xF3) | 0x08;
                        }
                        str = str + ",0x" + (picture_type & 0xff).ToString("X2");
                    }




                    str = str + "};";

                    if ((UIgen_comboBox2.SelectedIndex == 1) || (UIgen_comboBox2.SelectedIndex == 2))
                    {
                        str = str + commentList[i];
                    }

                    buffer = Encoding.Default.GetBytes(str + "\r");

                    fsWrite.Write(buffer, 0, buffer.Length);

                    UIgen_progressBar1.Value = (int)((double)i / ((double)lstArray.Count) * 10) + 70;
                }
            }

            UIgen_richTextBox1.Text = System.IO.File.ReadAllText(path);


            string path_h = path.Replace(".txt", string.Empty) + ".h";
            using (FileStream fsWrite = new FileStream(path_h, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = null;
                for (int i = 0; i < lstArray.Count; i++)
                {
                    buffer = Encoding.Default.GetBytes(externList[i] + "\r");

                    fsWrite.Write(buffer, 0, buffer.Length);

                    UIgen_progressBar1.Value = (int)((double)i / ((double)lstArray.Count) * 10) + 80;
                }
            }

            FileStream fs = new FileStream(UIgen_label2.Text, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            for (int i = 0; i < lstArray.Count; i++)
            {
                if ((UIgen_comboBox5.SelectedIndex == 1) || (UIgen_comboBox5.SelectedIndex == 2))  //前面已经有数组了
                {
                    bw.Write(byteArrayencode[i], 0, (int)size[i]);
                }
                else
                {
                    string str = lstArray[i];
                    int j = 0;
                    byte[] byteArray = new byte[320000];
                    try
                    {
                        while (str.Length != 0)
                        {
                            byteArray[j++] = Convert.ToByte(str.Substring(0, 2), 16);
                            str = str.Remove(0, 2);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("出错位置第" + (i + 1).ToString() + "个数组");
                        return;
                    }

                    bw.Write(byteArray, 0, j);
                }
                UIgen_progressBar1.Value = (int)((double)i / ((double)lstArray.Count) * 10) + 90;
            }
            bw.Flush();
            bw.Close();
            fs.Close();

            string path_c = path_h.Replace(".h", string.Empty) + ".c";
            if (System.IO.File.Exists(Path.GetFullPath(path_c)))
            {
                File.Delete(Path.GetFullPath(path_c));
            }
            File.Copy(path, path_c, true);

            MessageBox.Show("保存成功!");

            UIgen_progressBar1.Value = 100;

            Output_Display_Extern = false;
            UIgen_button4.Text = "输出显示为声明";
        }

        private void UIgen_button1_Click(object sender, EventArgs e)
        {
            UIgen_textInput.Clear();
            UIgen_richTextBox1.Clear();
        }

        private void UIgen_button2_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.SaveFileDialog DumpDlg = new Microsoft.Win32.SaveFileDialog();
            DumpDlg.FileName = "source_file";

            DumpDlg.DefaultExt = ".txt";

            DumpDlg.Filter = "txt file (.txt)|*.txt";

            if (DumpDlg.ShowDialog() == true)
            {
                File.WriteAllText(DumpDlg.FileName, UIgen_textInput.Text);
            }
            UIgen_label2.Text = DumpDlg.FileName.Replace(".txt", string.Empty).Replace(".TXT", string.Empty) + "_ok.bin";
        }

        private void UIgen_button3_Click(object sender, EventArgs e)
        {
            UIgen_progressBar1.Value = 0;
            string str = "";
            if (Input_FileNames.Length > 0)
            {
                for (int num = 0; num < Input_FileNames.Length; num++)
                {
                    str += System.IO.File.ReadAllText(Input_FileNames[num].ToString());
                    UIgen_progressBar1.Value = (int)((double)num / ((double)Input_FileNames.Length) * 100);
                }
                UIgen_progressBar1.Value = 100;
            }

            UIgen_textInput.Text = str;
        }

        private void UIgen_button4_Output_Click(object sender, EventArgs e)
        {
            if (path != null)
            {
                if (Output_Display_Extern == false)
                {
                    string str = path;
                    Output_Display_Extern = true;
                    string fileType = System.IO.Path.GetExtension(path);
                    if (fileType == ".txt")
                    {
                        str = path.Replace(".txt", string.Empty) + ".h";
                    }
                    UIgen_richTextBox1.Text = System.IO.File.ReadAllText(str);
                    UIgen_button4.Text = "输出显示为数组";
                }
                else
                {
                    string str = path;
                    Output_Display_Extern = false;
                    string fileType = System.IO.Path.GetExtension(path);
                    if (fileType == ".h")
                    {
                        str = path.Replace(".h", string.Empty) + ".txt";
                    }
                    UIgen_richTextBox1.Text = System.IO.File.ReadAllText(str);
                    UIgen_button4.Text = "输出显示为声明";
                }
            }
        }
    }
}


