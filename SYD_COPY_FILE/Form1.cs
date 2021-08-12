﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Windows.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace SYD_COPY_FILE
{
    public partial class Form1 : Form
    {
        #region define
        private static bool data_direction = true;

        public struct _OFFECT_
        {
            public int x;
            public int y;
            public int out_w;
            public int out_h;
            public int in_w;
            public int in_h;
            public int in_R_w;
            public int in_R_h;
            public int num;
            public int x1;
            public int y1;
        };

        private static bool pictureBox_interface_ismin = true;
        private static bool SC_MAX = false;
        //Arithmetic.dll
        [System.Runtime.InteropServices.DllImport("Arithmetic.dll", EntryPoint = "ll_crc24_generate", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt32 ll_crc24_generate(UInt32 seed, byte[] ppdata, int inSize);
        [System.Runtime.InteropServices.DllImport("Arithmetic.dll", EntryPoint = "bleWhiten", CallingConvention = CallingConvention.Cdecl)]
        private static extern void bleWhiten(Byte chan, [Out(), MarshalAs(UnmanagedType.LPArray)] byte[] buf, byte start_index, byte len);
        [System.Runtime.InteropServices.DllImport("Arithmetic.dll", EntryPoint = "reverseBits", CallingConvention = CallingConvention.Cdecl)]
        private static extern Byte reverseBits(Byte chan);
        [System.Runtime.InteropServices.DllImport("Arithmetic.dll", EntryPoint = "crc32_fun", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt32 crc32_fun(byte[] buf, UInt32 size);
        #endregion

        public Form1()
        {
            InitializeComponent();

            comboBox4.SelectedIndex = 2;
            comboBox3.SelectedIndex = 2;
            comboBox1.SelectedIndex = 1;
            comboBox2.SelectedIndex = 1;
            comboBox5.SelectedIndex = 0;

            data_direction = false;
            copy_file_init();
            syd_arr_init();
            PicSplit_init();
            ui_init();

            pictureBox_interface_ismin = Settings1.Default.pictureBox_interface_ismin;

            pictureBoxinterface_Click(null,null);

//#if _SYDTEK_INTERNAL_
//            this.Text = "SYD_Calculator_INTERNAL " + Settings1.Default.Setting_VERSION;
//#else
//            tabPage11.Parent = null;
//            this.Text="SYD_Calculator_release "+Settings1.Default.Setting_VERSION;
//#endif

            AdjustComboBoxDropDownListWidth(comboBox5);
        }

        /// <summary>
        /// 计算字符串解析表达式 1+2(2*(3+4))
        /// </summary>
        /// <param name="str">传入的字符串</param>
        /// <returns>计算得到的结果</returns>
        public decimal CalcStr(string str)
        {
            decimal num = 0m;
            //数字集合
            List<decimal> numList = new List<decimal>();
            //操作符集合
            List<Operation> operList = new List<Operation>();
            string strNum = "";
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                //判断如果是数字和.
                if ((47 < c && c < 58) || c == '.')
                {
                    strNum += c;

                    if (i == str.Length - 1)
                    {
                        if (!string.IsNullOrEmpty(strNum))
                        {
                            decimal.TryParse(strNum, out num);
                            numList.Add(num);
                            strNum = "";
                        }
                    }
                    continue;
                }
                else if (c == '(')
                {
                    int temp = 1;
                    for (int j = i + 1; j < str.Length; j++)
                    {
                        var k = str[j];
                        if (k == '(')
                        {
                            temp++;
                        }
                        else if (k == ')')
                        {
                            temp--;
                        }

                        if (temp == 0)
                        {
                            temp = j - i - 1;
                        }
                    }

                    strNum = str.Substring(i + 1, temp);
                    numList.Add(CalcStr(strNum));
                    strNum = "";
                    i += temp + 1;
                }
                else
                {
                    if (!string.IsNullOrEmpty(strNum))
                    {
                        decimal.TryParse(strNum, out num);
                        numList.Add(num);
                        strNum = "";
                    }

                    if (c == '+')
                    {
                        operList.Add(new AddOperation());
                    }
                    else if (c == '-')
                    {
                        operList.Add(new SubOperation());
                    }
                    else if (c == '*')
                    {
                        operList.Add(new MultipOperation());
                    }
                    else if (c == '/')
                    {
                        operList.Add(new DivOperation());
                    }
                    else if (c == '%')
                    {
                        operList.Add(new ModOperation());
                    }
                    else
                    {
                        operList.Add(null);
                    }
                }
            }

            List<int> tempOrder = new List<int>();
            operList.ForEach(w =>
            {
                if (!tempOrder.Contains(w.PrioRity))
                {
                    tempOrder.Add(w.PrioRity);
                }

            });

            tempOrder.Sort();
            for (int t = 0; t < tempOrder.Count; t++)
            {
                for (int i = 0; i < operList.Count; i++)
                {
                    if (operList[i].PrioRity == tempOrder[t])
                    {
                        numList[i] = operList[i].OperationResult(numList[i], numList[i + 1]);
                        numList.RemoveAt(i + 1);
                        operList.RemoveAt(i);
                        i--;
                    }
                }
            }

            if (numList.Count == 1) return numList[0];

            return 0m;
        }

        public class Operation
        {
            protected int priority = 99;
            /// <summary>
            /// 优先级
            /// </summary>
            public virtual int PrioRity
            {
                get
                {
                    return priority;
                }
                set
                {
                    priority = value;
                }
            }

            public virtual decimal OperationResult(decimal a, decimal b)
            {
                return 0m;
            }
        }

        public class AddOperation : Operation
        {
            public override decimal OperationResult(decimal a, decimal b)
            {
                return a + b;
            }
        }

        public class SubOperation : Operation
        {
            public override decimal OperationResult(decimal a, decimal b)
            {
                return a - b;
            }
        }

        public class MultipOperation : Operation
        {
            public override int PrioRity
            {
                get
                {
                    return 98;
                }
            }

            public override decimal OperationResult(decimal a, decimal b)
            {
                return a * b;
            }
        }

        public class DivOperation : Operation
        {
            public override int PrioRity
            {
                get
                {
                    return 98;
                }
            }
            public override decimal OperationResult(decimal a, decimal b)
            {
                return a / b;
            }
        }

        public class ModOperation : Operation
        {
            public override int PrioRity
            {
                get
                {
                    return 97;
                }
            }
            public override decimal OperationResult(decimal a, decimal b)
            {
                return a % b;
            }
        }

        private void cal_32768_subtract(string a, string b, string multiplier)
        {
            float result = 0, num = 0, num1 = 0;
            if ((a.Length == 0) | (b.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }
            if (multiplier.Length == 0)
            {
                multiplier = "1";
            }
            else 
            {
                multiplier=multiplier.Replace(" ", "");
            }
            num = Convert.ToSingle(a);
            num1 = Convert.ToSingle(b);
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    num = (float)num + (float)num1;
                    break;
                case 1:
                    num = (float)num - (float)num1;
                    break;
                case 2:
                    num = (float)num * (float)num1;
                    break;
                case 3:
                    num = (float)num / (float)num1;
                    break;
            }
            result = (float)CalcStr(multiplier);
            switch (comboBox3.SelectedIndex)
            {
                case 0:
                    result = num + result;
                    break;
                case 1:
                    result = num - result;
                    break;
                case 2:
                    result = num * result;
                    break;
                case 3:
                    result = num / result;
                    break;
            }
            textBox7.Text = result.ToString();
        }

        private void cal_overtrun_subtract(string a)
        {
            string str = "", str1 = "";
            if ((a.Length % 2)==1)
            {
                str = "0" + a;
            }
            else
            {
                str = a;
            }
            for (int i = 0; i < str.Length / 2; i++)
            {
                str1 = str1+str.Substring(str.Length - (i+1) * 2, 2);
            }
            textBox61.Text = str1;
        }

        private void cal_32768_hex_subtract(string a, string b, string multiplier)
        {
            Int32 num = 0, num1 = 0;
            float result = 0,temp=0;
            if ((a.Length == 0) | (b.Length == 0)) {
                MessageBox.Show("input error");
                return;
            }
            if (multiplier.Length == 0)
            {
                multiplier = "1";
            }
            else
            {
                multiplier = multiplier.Replace(" ", "");
            }
            num = Convert.ToInt32(a, 16);
            num1 = Convert.ToInt32(b, 16);
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    temp = (float)num + (float)num1;
                    break;
                case 1:
                    temp = (float)num - (float)num1;
                    break;
                case 2:
                    temp = (float)num * (float)num1;
                    break;
                case 3:
                    temp = (float)num / (float)num1;
                    break;
            }
            result = (float)CalcStr(multiplier);
            switch (comboBox4.SelectedIndex)
            { 
                case 0:
                    result = temp + result;
                    break;
                case 1:
                    result = temp - result;
                    break;
                case 2:
                    result = temp * result;
                    break;
                case 3:
                    result = temp / result;
                    break;
            }
            textBox4.Text = result.ToString();
            textBox9.Text = "0x" + ((UInt32)result).ToString("X");
        }
        private void cal_latency_hex_subtract(string d, string e, string a, string b, string c)
        {
            Int32 num = 0, num1 = 0, num2 = 0;
            //UInt16 timer_loop = 0;
            float result = 0;
            if ((a.Length == 0) | (b.Length == 0) | (c.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }
            num = Convert.ToInt32(a, 16);
            num1 = Convert.ToInt32(b, 16);
            num2 = Convert.ToInt32(c, 16);
            //if (num2 > 0)
            //    timer_loop = (((p_req->Instant - EvtCnt) * ((float)lclink->link.interval * 1.25)) + (((float)p_req->Interval * 1.25) * p_req->Latency)) / 10;
            result = ((float)num - (float)num1) * 1000 / 32768;
            textBox4.Text = result.ToString();
        }

        private void cal_time_difference_subtract(string a, string b)
        {
            Int32 num = 0, num1 = 0;
            float result = 0;
            if ((a.Length == 0) | (b.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }
            num = Convert.ToInt32(a, 10);
            num1 = Convert.ToInt32(b, 10);
            result = (float)num - ((float)num1 -8)*8;
            textBox24.Text = result.ToString();
        }

        private string cal_Calendar_time_difference_subtract(string a, string b, int c, TextBox testbox_display, TextBox testbox_display_timestamp)
        {
            Int32 hour = 0, minute = 0, second = 0, millisecond = 0, hour1 = 0, minute1 = 0, second1 = 0, millisecond1 = 0,day=0;
            UInt32 timestamp = 0, timestamp1 = 0;
            string result = "";
            if ((a.Length == 0) | (b.Length == 0))
            {
                MessageBox.Show("input error");
                return null;
            }
            hour = Convert.ToInt32(a.Substring(0, 2), 10);
            minute = Convert.ToInt32(a.Substring(3, 2), 10);
            second = Convert.ToInt32(a.Substring(6, 2), 10);
            millisecond = Convert.ToInt32(a.Substring(9, 3), 10);
            timestamp = ((UInt32)hour * 3600 + (UInt32)minute * 60 + (UInt32)second) * 1000 + (UInt32)millisecond;

            timestamp = (UInt32)c * 86400000 + timestamp;

            hour1 = Convert.ToInt32(b.Substring(0, 2), 10);
            minute1 = Convert.ToInt32(b.Substring(3, 2), 10);
            second1 = Convert.ToInt32(b.Substring(6, 2), 10);
            millisecond1 = Convert.ToInt32(b.Substring(9, 3), 10);

            timestamp1 = ((UInt32)hour1 * 3600 + (UInt32)minute1 * 60 + (UInt32)second1) * 1000 + (UInt32)millisecond1;

            timestamp = timestamp - timestamp1;
            timestamp1 = timestamp/1000;

            millisecond = (Int32)(timestamp % 1000);
            timestamp = timestamp / 1000;
            second = (Int32)(timestamp % 60);
            timestamp = timestamp / 60;
            minute = (Int32)(timestamp % 60);
            timestamp = timestamp / 60;
            hour = (Int32)(timestamp % 24);
            timestamp = timestamp / 24;
            day = (Int32)(timestamp % 31);

            if (day ==0)
                result = hour.ToString("D2") + ":" + minute.ToString("D2") + ":" + second.ToString("D2") + ":" + millisecond.ToString("D3");
            else
                result = day.ToString("D2") + " " + hour.ToString("D2") + ":" + minute.ToString("D2") + ":" + second.ToString("D2") + ":" + millisecond.ToString("D3");
            
            if (testbox_display != null)
                testbox_display.Text = result;

            if (testbox_display_timestamp != null)
                testbox_display_timestamp.Text = "0x" + timestamp1.ToString("x");

            return result;
        }

        private string cal_Calendar_time_differenceble_subtract(string a, string b, TextBox testbox_display, TextBox testbox_display_timestamp)
        {
            Int32 hour = 0, minute = 0, second = 0, millisecond = 0, microsecond = 0, hour1 = 0, minute1 = 0, second1 = 0, millisecond1 = 0 ,microsecond1 = 0;
            UInt64 timestamp = 0, timestamp1 = 0;
            string result = "";
            
            if ((a.Length == 0) | (b.Length == 0))
            {
                MessageBox.Show("input error");
                return null;
            }
            List<string> lstArray = a.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (lstArray.Count > 1)
            {
                a = lstArray[1];
            }
            lstArray = b.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (lstArray.Count > 1)
            {
                b = lstArray[1];
            }
            hour = Convert.ToInt32(a.Substring(0, 2), 10);
            minute = Convert.ToInt32(a.Substring(3, 2), 10);
            second = Convert.ToInt32(a.Substring(6, 2), 10);
            millisecond = Convert.ToInt32(a.Substring(9, 3), 10);
            microsecond = Convert.ToInt32(a.Substring(12, 3), 10);
            timestamp = ((UInt64)hour * 3600  + (UInt64)minute * 60  + (UInt64)second) * 1000 * 1000 + (UInt64)millisecond * 1000 + (UInt64)microsecond;

            hour1 = Convert.ToInt32(b.Substring(0, 2), 10);
            minute1 = Convert.ToInt32(b.Substring(3, 2), 10);
            second1 = Convert.ToInt32(b.Substring(6, 2), 10);
            millisecond1 = Convert.ToInt32(b.Substring(9, 3), 10);
            microsecond1 = Convert.ToInt32(b.Substring(12, 3), 10);
            timestamp1 = ((UInt64)hour1 * 3600 + (UInt64)minute1 * 60 + (UInt64)second1) * 1000 * 1000 + (UInt64)millisecond1 * 1000 + (UInt64)microsecond1;

            timestamp = timestamp - timestamp1;
            timestamp1 = timestamp / 1000;

            microsecond = (Int32)(timestamp % 1000);
            timestamp = timestamp / 1000;
            millisecond = (Int32)(timestamp % 1000);
            timestamp = timestamp / 1000;
            second = (Int32)(timestamp % 60);
            timestamp = timestamp / 60;
            minute = (Int32)(timestamp % 60);
            timestamp = timestamp / 60;
            hour = (Int32)(timestamp % 24);

            result = hour.ToString("D2") + ":" + minute.ToString("D2") + ":" + second.ToString("D2") + ":" + millisecond.ToString("D3") + microsecond.ToString("D3");
            if (testbox_display!=null)
             testbox_display.Text = result;

            if (testbox_display_timestamp != null)
                testbox_display_timestamp.Text = "0x" + timestamp1.ToString("x");

            return result;
        }

        private string cal_Calendar_ble_time_sum(string a, string b, TextBox testbox_display, TextBox testbox_display_timestamp)
        {
            Int32 hour = 0, minute = 0, second = 0, millisecond = 0, microsecond = 0, hour1 = 0, minute1 = 0, second1 = 0, millisecond1 = 0, microsecond1 = 0;
            UInt64 timestamp = 0, timestamp1 = 0;
            string result = "";

            if ((a.Length == 0) | (b.Length == 0))
            {
                MessageBox.Show("input error");
                return null;
            }
            List<string> lstArray = a.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (lstArray.Count > 1)
            {
                a = lstArray[1];
            }
            lstArray = b.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (lstArray.Count > 1)
            {
                b = lstArray[1];
            }
            hour = Convert.ToInt32(a.Substring(0, 2), 10);
            minute = Convert.ToInt32(a.Substring(3, 2), 10);
            second = Convert.ToInt32(a.Substring(6, 2), 10);
            millisecond = Convert.ToInt32(a.Substring(9, 3), 10);
            microsecond = Convert.ToInt32(a.Substring(12, 3), 10);
            timestamp = ((UInt64)hour * 3600 + (UInt64)minute * 60 + (UInt64)second) * 1000 * 1000 + (UInt64)millisecond * 1000 + (UInt64)microsecond;

            hour1 = Convert.ToInt32(b.Substring(0, 2), 10);
            minute1 = Convert.ToInt32(b.Substring(3, 2), 10);
            second1 = Convert.ToInt32(b.Substring(6, 2), 10);
            millisecond1 = Convert.ToInt32(b.Substring(9, 3), 10);
            microsecond1 = Convert.ToInt32(b.Substring(12, 3), 10);
            timestamp1 = ((UInt64)hour1 * 3600 + (UInt64)minute1 * 60 + (UInt64)second1) * 1000 * 1000 + (UInt64)millisecond1 * 1000 + (UInt64)microsecond1;

            timestamp = timestamp + timestamp1;
            timestamp1 = timestamp / 1000;

            microsecond = (Int32)(timestamp % 1000);
            timestamp = timestamp / 1000;
            millisecond = (Int32)(timestamp % 1000);
            timestamp = timestamp / 1000;
            second = (Int32)(timestamp % 60);
            timestamp = timestamp / 60;
            minute = (Int32)(timestamp % 60);
            timestamp = timestamp / 60;
            hour = (Int32)(timestamp % 24);

            result = hour.ToString("D2") + ":" + minute.ToString("D2") + ":" + second.ToString("D2") + ":" + millisecond.ToString("D3") + microsecond.ToString("D3");
            if (testbox_display != null)
                testbox_display.Text = result;

            if (testbox_display_timestamp != null)
                testbox_display_timestamp.Text = "0x" + timestamp1.ToString("x");

            return result;
        }

        private void cal_Calendar_blespeed_subtract(string a, string b, TextBox testbox_display)
        {
            Int32 packet = 0, hour1 = 0, minute1 = 0, second1 = 0, millisecond1 = 0, microsecond1 = 0;
            UInt64 timestamp1 = 0;
            double speed = 0.0000;
            string result = "";
            if ((a.Length == 0) | (b.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }

            packet = Convert.ToInt32(a, 10);

            hour1 = Convert.ToInt32(b.Substring(0, 2), 10);
            minute1 = Convert.ToInt32(b.Substring(3, 2), 10);
            second1 = Convert.ToInt32(b.Substring(6, 2), 10);
            millisecond1 = Convert.ToInt32(b.Substring(9, 3), 10);
            microsecond1 = Convert.ToInt32(b.Substring(12, 3), 10);
            timestamp1 = ((UInt64)hour1 * 3600 + (UInt64)minute1 * 60 + (UInt64)second1) * 1000 * 1000 + (UInt64)millisecond1 * 1000 + (UInt64)microsecond1;

            speed = (double)timestamp1 / 1000000;
            speed = (double)packet*20 / speed;
            speed = speed / 1000;

            result = speed.ToString("N") + "KByte/s";
            testbox_display.Text = result;
        }
        private void cal_timestamp_difference_subtract(string a, string b,TextBox testbox_display)
        {
            Int32 hour = 0, minute = 0, second = 0;
            UInt32 timestamp = 0, timestamp1 = 0;
            string result = "";
            if ((a.Length == 0) | (b.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }
            timestamp = Convert.ToUInt32(a.Substring(2, 8), 16);

            timestamp1 = Convert.ToUInt32(b.Substring(2, 8), 16);

            timestamp = timestamp - timestamp1;

            second = (Int32)(timestamp % 60);
            timestamp = timestamp / 60;
            minute = (Int32)(timestamp % 60);
            timestamp = timestamp / 60;
            hour = (Int32)(timestamp % 24);

            result = hour.ToString("D2") + ":" + minute.ToString("D2") + ":" + second.ToString("D2");
            testbox_display.Text = result;
        }

        private void cal_rtccounter_difference_subtract(string a, string b, TextBox testbox_display)
        {
            Int32 hour = 0, minute = 0, second = 0,millisecond = 0;
            UInt32 timestamp = 0, timestamp1 = 0;
            string result = "";
            if ((a.Length == 0) | (b.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }
            timestamp = Convert.ToUInt32(a.Substring(2, 8), 16);

            timestamp1 = Convert.ToUInt32(b.Substring(2, 8), 16);

            timestamp = timestamp - timestamp1;
            timestamp1 = timestamp;


            timestamp1 = (UInt32)(timestamp1 / 32.768);
            millisecond = (Int32)(timestamp % 1000);
            timestamp = timestamp / 1000;

            second = (Int32)(timestamp % 60);
            timestamp = timestamp / 60;
            minute = (Int32)(timestamp % 60);
            timestamp = timestamp / 60;
            hour = (Int32)(timestamp % 24);

            result = hour.ToString("D2") + ":" + minute.ToString("D2") + ":" + second.ToString("D2") + ":" + millisecond.ToString("D3");
            testbox_display.Text = result;
        }

        private void cal_rgb_subtract(int input,TextBox output)
        {
            Byte a, b, c;
            a = (Byte)(input >> 19);
            b = (Byte)(input >> 10);
            c = (Byte)(input >> 3);
            input = (a << 11) | (b << 5) | c;
            output.Text = "0x" + ((UInt32)input).ToString("X");
        }

        private _OFFECT_ cal_offect_subtract(_OFFECT_ offect)
        {
            int x = 0, y = 0;
            x = offect.out_w - offect.x * offect.num;
            y = offect.out_h - offect.y;
            offect.in_w = x/2;
            offect.in_h = y/2;
            if (x % 2 == 1)
            {
                offect.in_R_w = x / 2 + 1;
            }
            else
            {
                offect.in_R_w = x / 2;
            }
            offect.in_R_h = offect.out_h - offect.in_h - offect.y;
            offect.x1 = offect.out_w - offect.in_R_w;
            offect.y1 = offect.out_h - offect.in_R_h;
            return offect;
        }

        // 将文件转换为byte数组
        //param name="path"文件地址
        //返回值 转换后的byte数组
        public static byte[] File2Bytes(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new byte[0];
            }

            FileInfo fi = new FileInfo(path);
            byte[] buff = new byte[fi.Length];

            FileStream fs = fi.OpenRead();
            fs.Read(buff, 0, Convert.ToInt32(fs.Length));
            fs.Close();

            return buff;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void lock_button_Click(object sender, EventArgs e)
        {
            if (this.TopMost == true)
            {
                this.TopMost = false;
                pictureBox_lock.Image = Properties.Resources.unlock;
                pictureBox18.Image = Properties.Resources.unlock;
            }
            else
            {
                this.TopMost = true;
                pictureBox_lock.Image = Properties.Resources._lock;
                pictureBox18.Image = Properties.Resources._lock;
            }
        }
        private void pictureBoxinterface_Doing(bool min)
        {
            if (min == true)
            {
                pictureBox_interface_ismin = false;
                pictureBox_interface.Image = Properties.Resources.max;
                this.Width = 612;
                this.Height = 232;
            }
            else
            {
                pictureBox_interface_ismin = true;
                pictureBox_interface.Image = Properties.Resources.min;
                this.Width = 782;
                this.Height = 620;
            }
        }

        private void pictureBoxinterface_Click(object sender, EventArgs e)
        {
            if (pictureBox_interface_ismin == true)
            {
                pictureBoxinterface_Doing(true);
            }
            else
            {
                pictureBoxinterface_Doing(false);
            }
            //最大化窗口：
            //ApiCalls.ShowWindow(Form.ActiveForm.Handle,3); 
            //最小化窗口：
            //ApiCalls.ShowWindow(Form.ActiveForm.Handle,2);
            //恢复正常大小窗口：
            //ApiCalls.ShowWindow(Form.ActiveForm.Handle,1);
            this.WindowState = FormWindowState.Normal;
        }

        private void textBox8_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                cal_32768_subtract(textBox6.Text, textBox8.Text, textBox1.Text);
            }
        }

        private void textBox63_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                cal_overtrun_subtract(textBox63.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cal_32768_subtract(textBox6.Text, textBox8.Text, textBox1.Text);
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if ((textBox3.Text.Length != 0) && (textBox5.Text.Length == 0))
                    textBox5.Text = "0";
                if ((textBox5.Text.Length != 0) && (textBox3.Text.Length == 0))
                    textBox3.Text = "0";
                cal_32768_hex_subtract(textBox3.Text, textBox5.Text, textBox2.Text);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox3.Text.Length != 0) && (textBox5.Text.Length == 0))
                textBox5.Text = "0";
            if ((textBox5.Text.Length != 0) && (textBox3.Text.Length == 0))
                textBox3.Text = "0";

            cal_32768_hex_subtract(textBox3.Text, textBox5.Text, textBox2.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if ((textBox18.Text.Length == 0) || (textBox17.Text.Length == 0) || (textBox13.Text.Length == 0) || (textBox15.Text.Length == 0) || (textBox16.Text.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }
            cal_latency_hex_subtract(textBox18.Text, textBox17.Text,textBox13.Text, textBox15.Text, textBox16.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if ((textBox19.Text.Length == 0) || (textBox20.Text.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }
            cal_time_difference_subtract(textBox19.Text, textBox20.Text);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Int32 date = 0;
            if ((textBox21.Text.Length == 0) || (textBox22.Text.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }

            textBox21.Text=textBox21.Text.Trim();
            textBox22.Text = textBox22.Text.Trim();

            if (textBox25.Text.Length != 0)
            {
                date=Convert.ToInt32(textBox25.Text, 10);
            }

            if (textBox21.Text.Length > 13)
            {
                cal_Calendar_time_differenceble_subtract(textBox21.Text, textBox22.Text, textBox23, textBox56);
            }
            else
            {
                cal_Calendar_time_difference_subtract(textBox21.Text, textBox22.Text, date, textBox23, textBox56);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Int32 date = 0;
            if ((textBox29.Text.Length == 0) || (textBox28.Text.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }

            textBox29.Text = textBox29.Text.Trim();
            textBox28.Text = textBox28.Text.Trim();

            if (textBox26.Text.Length != 0)
            {
                date = Convert.ToInt32(textBox26.Text, 10);
            }

            if (textBox29.Text.Length > 13)
            {
                cal_Calendar_time_differenceble_subtract(textBox29.Text, textBox28.Text, textBox27, textBox57);
            }
            else
            {
                cal_Calendar_time_difference_subtract(textBox29.Text, textBox28.Text, date, textBox27, textBox57);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if ((textBox36.Text.Length == 0) || (textBox37.Text.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }

            cal_timestamp_difference_subtract(textBox37.Text, textBox36.Text, textBox35);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if ((textBox33.Text.Length == 0) || (textBox32.Text.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }

            cal_timestamp_difference_subtract(textBox33.Text, textBox32.Text, textBox31);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Int32 date = 0;
            if ((textBox39.Text.Length == 0) || (textBox38.Text.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }

            if (textBox30.Text.Length != 0)
            {
                date = Convert.ToInt32(textBox30.Text, 10);
            }

            if (textBox39.Text.Length > 13)
            {
                cal_Calendar_time_differenceble_subtract(textBox39.Text, textBox38.Text, textBox34, textBox60);
            }
            else
            {
                cal_Calendar_time_difference_subtract(textBox39.Text, textBox38.Text, date, textBox34, textBox60);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if ((textBox42.Text.Length == 0) || (textBox41.Text.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }

            cal_timestamp_difference_subtract(textBox42.Text, textBox41.Text, textBox40);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if ((textBox51.Text.Length == 0) || (textBox50.Text.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }

            cal_rtccounter_difference_subtract(textBox51.Text, textBox50.Text, textBox49);
        }

        private void bitmask_checkbox_Click(object sender, EventArgs e)
        {
            UInt32 bit_mask = 0;
            if (BIT_MARK0.CheckState == CheckState.Checked) bit_mask |= 0x01;
            if (BIT_MARK1.CheckState == CheckState.Checked) bit_mask |= 0x02;
            if (BIT_MARK2.CheckState == CheckState.Checked) bit_mask |= 0x04;
            if (BIT_MARK3.CheckState == CheckState.Checked) bit_mask |= 0x08;
            if (BIT_MARK4.CheckState == CheckState.Checked) bit_mask |= 0x10;
            if (BIT_MARK5.CheckState == CheckState.Checked) bit_mask |= 0x20;
            if (BIT_MARK6.CheckState == CheckState.Checked) bit_mask |= 0x40;
            if (BIT_MARK7.CheckState == CheckState.Checked) bit_mask |= 0x80;
            if (BIT_MARK8.CheckState == CheckState.Checked) bit_mask |= 0x100;
            if (BIT_MARK9.CheckState == CheckState.Checked) bit_mask |= 0x200;
            if (BIT_MARK10.CheckState == CheckState.Checked) bit_mask |= 0x400;
            if (BIT_MARK11.CheckState == CheckState.Checked) bit_mask |= 0x800;
            if (BIT_MARK12.CheckState == CheckState.Checked) bit_mask |= 0x1000;
            if (BIT_MARK13.CheckState == CheckState.Checked) bit_mask |= 0x2000;
            if (BIT_MARK14.CheckState == CheckState.Checked) bit_mask |= 0x4000;
            if (BIT_MARK15.CheckState == CheckState.Checked) bit_mask |= 0x8000;
            if (BIT_MARK16.CheckState == CheckState.Checked) bit_mask |= 0x10000;
            if (BIT_MARK17.CheckState == CheckState.Checked) bit_mask |= 0x20000;
            if (BIT_MARK18.CheckState == CheckState.Checked) bit_mask |= 0x40000;
            if (BIT_MARK19.CheckState == CheckState.Checked) bit_mask |= 0x80000;
            if (BIT_MARK20.CheckState == CheckState.Checked) bit_mask |= 0x100000;
            if (BIT_MARK21.CheckState == CheckState.Checked) bit_mask |= 0x200000;
            if (BIT_MARK22.CheckState == CheckState.Checked) bit_mask |= 0x400000;
            if (BIT_MARK23.CheckState == CheckState.Checked) bit_mask |= 0x800000;
            if (BIT_MARK24.CheckState == CheckState.Checked) bit_mask |= 0x1000000;
            if (BIT_MARK25.CheckState == CheckState.Checked) bit_mask |= 0x2000000;
            if (BIT_MARK26.CheckState == CheckState.Checked) bit_mask |= 0x4000000;
            if (BIT_MARK27.CheckState == CheckState.Checked) bit_mask |= 0x8000000;
            if (BIT_MARK28.CheckState == CheckState.Checked) bit_mask |= 0x10000000;
            if (BIT_MARK29.CheckState == CheckState.Checked) bit_mask |= 0x20000000;
            if (BIT_MARK30.CheckState == CheckState.Checked) bit_mask |= 0x40000000;
            if (BIT_MARK31.CheckState == CheckState.Checked) bit_mask |= 0x80000000;
            bit_mask_result.Text = "0x" + bit_mask.ToString("X8");
        }

        private void LED_input1_textBox_keydown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                byte input = Convert.ToByte(LED_input1_textBox.Text.Substring(2, 2), 16);
                UInt32 output = 0;

                byte temp = Convert.ToByte(textBox_GPIO0.Text, 10);
                if ((input & 0x01) == 0x01)
                    output |= (UInt32)(1 << temp);

                temp = Convert.ToByte(textBox_GPIO1.Text, 10);
                if ((input & 0x02) == 0x02)
                    output |= (UInt32)(1 << temp);

                temp = Convert.ToByte(textBox_GPIO2.Text, 10);
                if ((input & 0x04) == 0x04)
                    output |= (UInt32)(1 << temp);

                temp = Convert.ToByte(textBox_GPIO3.Text, 10);
                if ((input & 0x08) == 0x08)
                    output |= (UInt32)(1 << temp);

                temp = Convert.ToByte(textBox_GPIO4.Text, 10);
                if ((input & 0x10) == 0x10)
                    output |= (UInt32)(1 << temp);

                temp = Convert.ToByte(textBox_GPIO5.Text, 10);
                if ((input & 0x20) == 0x20)
                    output |= (UInt32)(1 << temp);

                temp = Convert.ToByte(textBox_GPIO6.Text, 10);
                if ((input & 0x40) == 0x40)
                    output |= (UInt32)(1 << temp);

                if (textBox_GPIO7.Text.Length != 0)
                {
                    temp = Convert.ToByte(textBox_GPIO7.Text, 10);
                    if ((input & 0x80) == 0x80)
                        output |= (UInt32)(1 << temp);
                }

                LED_result1_textBox.Text = "0x" + output.ToString("X8");
            }
        }

        private void bit_mask_result_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                UInt32 input = Convert.ToUInt32(bit_mask_result.Text.Substring(2, 8), 16);
                if ((input & 0x01) == 0x01) BIT_MARK0.Checked = true;
                else BIT_MARK0.Checked = false;
                if ((input & 0x02) == 0x02) BIT_MARK1.Checked = true;
                else BIT_MARK1.Checked = false;
                if ((input & 0x04) == 0x04) BIT_MARK2.Checked = true;
                else BIT_MARK2.Checked = false;
                if ((input & 0x08) == 0x08) BIT_MARK3.Checked = true;
                else BIT_MARK3.Checked = false;
                if ((input & 0x10) == 0x10) BIT_MARK4.Checked = true;
                else BIT_MARK4.Checked = false;
                if ((input & 0x20) == 0x20) BIT_MARK5.Checked = true;
                else BIT_MARK5.Checked = false;
                if ((input & 0x40) == 0x40) BIT_MARK6.Checked = true;
                else BIT_MARK6.Checked = false;
                if ((input & 0x80) == 0x80) BIT_MARK7.Checked = true;
                else BIT_MARK7.Checked = false;
                if ((input & 0x100) == 0x100) BIT_MARK8.Checked = true;
                else BIT_MARK8.Checked = false;
                if ((input & 0x200) == 0x200) BIT_MARK9.Checked = true;
                else BIT_MARK9.Checked = false;
                if ((input & 0x400) == 0x400) BIT_MARK10.Checked = true;
                else BIT_MARK10.Checked = false;
                if ((input & 0x800) == 0x800) BIT_MARK11.Checked = true;
                else BIT_MARK11.Checked = false;
                if ((input & 0x1000) == 0x1000) BIT_MARK12.Checked = true;
                else BIT_MARK12.Checked = false;
                if ((input & 0x2000) == 0x2000) BIT_MARK13.Checked = true;
                else BIT_MARK13.Checked = false;
                if ((input & 0x4000) == 0x4000) BIT_MARK14.Checked = true;
                else BIT_MARK14.Checked = false;
                if ((input & 0x8000) == 0x8000) BIT_MARK15.Checked = true;
                else BIT_MARK15.Checked = false;
                if ((input & 0x10000) == 0x10000) BIT_MARK16.Checked = true;
                else BIT_MARK16.Checked = false;
                if ((input & 0x20000) == 0x20000) BIT_MARK17.Checked = true;
                else BIT_MARK17.Checked = false;
                if ((input & 0x40000) == 0x40000) BIT_MARK18.Checked = true;
                else BIT_MARK18.Checked = false;
                if ((input & 0x80000) == 0x80000) BIT_MARK19.Checked = true;
                else BIT_MARK19.Checked = false;
                if ((input & 0x100000) == 0x100000) BIT_MARK20.Checked = true;
                else BIT_MARK20.Checked = false;
                if ((input & 0x200000) == 0x200000) BIT_MARK21.Checked = true;
                else BIT_MARK21.Checked = false;
                if ((input & 0x400000) == 0x400000) BIT_MARK22.Checked = true;
                else BIT_MARK22.Checked = false;
                if ((input & 0x800000) == 0x800000) BIT_MARK23.Checked = true;
                else BIT_MARK23.Checked = false;
                if ((input & 0x1000000) == 0x1000000) BIT_MARK24.Checked = true;
                else BIT_MARK24.Checked = false;
                if ((input & 0x2000000) == 0x2000000) BIT_MARK25.Checked = true;
                else BIT_MARK25.Checked = false;
                if ((input & 0x4000000) == 0x4000000) BIT_MARK26.Checked = true;
                else BIT_MARK26.Checked = false;
                if ((input & 0x8000000) == 0x8000000) BIT_MARK27.Checked = true;
                else BIT_MARK27.Checked = false;
                if ((input & 0x10000000) == 0x10000000) BIT_MARK28.Checked = true;
                else BIT_MARK28.Checked = false;
                if ((input & 0x20000000) == 0x20000000) BIT_MARK29.Checked = true;
                else BIT_MARK29.Checked = false;
                if ((input & 0x40000000) == 0x40000000) BIT_MARK30.Checked = true;
                else BIT_MARK30.Checked = false;
                if ((input & 0x80000000) == 0x80000000) BIT_MARK31.Checked = true;
                else BIT_MARK31.Checked = false;
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            BIT_MARK0.Checked = false;
            BIT_MARK1.Checked = false;
            BIT_MARK2.Checked = false;
            BIT_MARK3.Checked = false;
            BIT_MARK4.Checked = false;
            BIT_MARK5.Checked = false;
            BIT_MARK6.Checked = false;
            BIT_MARK7.Checked = false;
            BIT_MARK8.Checked = false;
            BIT_MARK9.Checked = false;
            BIT_MARK10.Checked = false;
            BIT_MARK11.Checked = false;
            BIT_MARK12.Checked = false;
            BIT_MARK13.Checked = false;
            BIT_MARK14.Checked = false;
            BIT_MARK15.Checked = false;
            BIT_MARK16.Checked = false;
            BIT_MARK17.Checked = false;
            BIT_MARK18.Checked = false;
            BIT_MARK19.Checked = false;
            BIT_MARK20.Checked = false;
            BIT_MARK21.Checked = false;
            BIT_MARK22.Checked = false;
            BIT_MARK23.Checked = false;
            BIT_MARK24.Checked = false;
            BIT_MARK25.Checked = false;
            BIT_MARK26.Checked = false;
            BIT_MARK27.Checked = false;
            BIT_MARK28.Checked = false;
            BIT_MARK29.Checked = false;
            BIT_MARK30.Checked = false;
            BIT_MARK31.Checked = false;
        }

        private void BIT_MARK_CheckStateChanged(object sender, EventArgs e)
        {
            bitmask_checkbox_Click(sender,e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string[] arr = new string[textBox10.Lines.Length];
            string result="";
            for (int i = 0; i < textBox10.Lines.Length; i++)
            {
                arr[i] = textBox10.Lines[i];
            }
            textBox11.AppendText("\r\n");
            if (checkBox1.Checked == false)
            {
                for (int i = 1; i < arr.Length; i++)
                {
                    if ((arr[i - 1].Length == 0) || (arr[i].Length == 0))
                        continue;
                    float Num = 0, Num1 = 0;
                    Num = float.Parse(arr[i - 1]);
                    Num1 = float.Parse(arr[i]);
                    if (Num1 > Num)
                        result = (Num1 - Num).ToString();
                    else
                        result = (Num - Num1).ToString();
                    textBox11.AppendText(result + "\r\n");
                }
            }
            else
            {
                for (int i = 1; i < arr.Length; i++)
                {
                    if ((arr[i - 1].Length == 0) || (arr[i].Length == 0))
                        continue;
                    UInt32 Num = 0, Num1 = 0;
                    Num = Convert.ToUInt32(arr[i - 1], 16);
                    Num1 = Convert.ToUInt32(arr[i], 16);;
                    if (Num1 > Num)
                        result = (Num1 - Num).ToString("X");
                    else
                        result = (Num - Num1).ToString("X");
                    textBox11.AppendText("0x"+result + "\r\n");
                }
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            int i = 0;
            string orgTxt1 = textBox10.Text.Trim();

            orgTxt1 = orgTxt1.Replace(" ", "").Replace("-", "");

            List<string> lstArray = orgTxt1.ToLower().Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();

            if (lstArray.Count <= 1) return;

            textBox11.Text = "\r\n";

            for (i = 1; i < lstArray.Count; i++)
            {
                if (lstArray[i].Length != 0)
                 textBox11.AppendText(cal_Calendar_time_difference_subtract(lstArray[i], lstArray[i - 1], 0,null, null) + "\r\n");
            }
        }

        private void button31_Click(object sender, EventArgs e)
        {
            int i = 0;
            UInt32 Num = 0;
            string orgTxt1 = textBox10.Text.Trim();

            orgTxt1 = orgTxt1.Replace(" ", "").Replace("-", "");

            List<string> lstArray = orgTxt1.ToLower().Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();

            if (lstArray.Count <= 1) return;

            for (i = 0; i < lstArray.Count; i++)
            {
                if (lstArray[i].Length != 0)
                {
                    if (checkBox1.Checked == false)
                    {
                        Num += Convert.ToUInt32(lstArray[i], 10);
                    }
                    else
                    {
                        Num += Convert.ToUInt32(lstArray[i], 16);
                    }
                }
            }
            if (checkBox1.Checked == false)
            {
                textBox11.Text = Num.ToString();
            }
            else
            {
                textBox11.Text = Num.ToString("X");
            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (data_direction == true)
            {
                data_direction = false;
                button4.Image = Properties.Resources.dowm;
            }
            else
            {
                data_direction = true;
                button4.Image = Properties.Resources.up;
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (textBox58.Text.Length != 0)
            {
                string str = textBox58.Text;
                cal_rgb_subtract(Convert.ToInt32(str, 10), textBox59);
            }
            else if (textBox55.Text.Length != 0)
            {
                string str = textBox55.Text.Remove(0, 2);
                cal_rgb_subtract(Convert.ToInt32(str, 16), textBox59);
            }
            else
            {
                MessageBox.Show("input error");
                return;
            }
           
        }

        private void cal_rgb_subtract(TextBox R, TextBox G, TextBox B, TextBox output)
        {
            //if (textBoxR.Text.Length == 0)
            //{
            //    textBoxR.Text = "0";
            //}
            //if (textBoxG.Text.Length == 0)
            //{
            //    textBoxG.Text = "0";
            //}
            //if (textBoxB.Text.Length == 0)
            //{
            //    textBoxB.Text = "0";
            //}
            //int temp = Convert.ToInt32(textBoxB.Text, 10) | (Convert.ToInt32(textBoxG.Text, 10) << 8) | (Convert.ToInt32(textBoxR.Text, 10) << 16);
            //cal_rgb_subtract(temp, textBox59);

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
            int temp = Convert.ToInt32(B.Text, 10) | (Convert.ToInt32(G.Text, 10) << 8) | (Convert.ToInt32(R.Text, 10) << 16);
            cal_rgb_subtract(temp, output);
        }

        private void buttonRGB_Click(object sender, EventArgs e)
        {
            cal_rgb_subtract(textBoxR, textBoxG, textBoxB, textBox59);
        }
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if ((TextBox)sender == textBoxR)
                    this.textBoxG.Focus();
                else if ((TextBox)sender == textBoxG)
                    this.textBoxB.Focus();
                else if ((TextBox)sender == textBoxB)
                {
                    cal_rgb_subtract(textBoxR, textBoxG, textBoxB, textBox59);
                }
            }
            else if (e.KeyChar == '\x1')
            {
                ((TextBox)sender).SelectAll();
                e.Handled = true;
            }
        }
        private void button22_Click(object sender, EventArgs e)
        {
            try
            {
                UInt32 base_data = 0;
                if (checkBox1.Checked)
                    base_data = Convert.ToUInt32(textBox10.Text, 16);
                else
                    base_data = Convert.ToUInt32(textBox10.Text, 10);
                
                UInt32 num = Convert.ToUInt32(textBox65.Text, 10);
                UInt32 difference = 0;
                if (textBox62.Text.Length != 0)
                    difference = Convert.ToUInt32(textBox62.Text, 10);
                if (difference == 0)
                {
                    difference = Convert.ToUInt32(textBox64.Text, 16);
                }
                UInt32 result = base_data;
                textBox11.Text = "";
                if (checkBox1.Checked)
                    textBox11.AppendText("0x" + result.ToString("X") + "\r\n");
                else
                    textBox11.AppendText(result.ToString() + "\r\n");
                
                for (UInt32 i = 1; i < num; i++)
                {
                    if (data_direction == false)
                    {
                        result = base_data - difference * i;
                    }
                    else
                    {
                        result = base_data + difference * i;
                    }
                    if (checkBox1.Checked)
                        textBox11.AppendText("0x"+result.ToString("X") + "\r\n");
                    else
                        textBox11.AppendText(result.ToString() + "\r\n");
                 }
            }
            catch
            {
                MessageBox.Show("invalid input");
                return;
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            textBox11.Text = "";
            textBox10.Text = "";
        }

        private void button28_Click(object sender, EventArgs e)
        {
            if ((textBox78.Text.Length == 0) || (textBox81.Text.Length == 0))
            {
                MessageBox.Show("input error");
                return;
            }

            cal_Calendar_blespeed_subtract(textBox78.Text, textBox81.Text, textBox80);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings1.Default.arr_data_type = comboBox_datatype.SelectedIndex;
            Settings1.Default.arr_font_type = comboBox_fonttype.SelectedIndex;
            Settings1.Default.arr_fun_sel = comboBox_mode.SelectedIndex;
            Settings1.Default.arr_source_file_text = source_file_textBox.Text;
            Settings1.Default.arr_extract_len = textBox_filesize.Text;

            Settings1.Default.source_copyfile_textBox = source_copyfile_textBox.Text;
            Settings1.Default.destination_file_textBox = destination_file_textBox.Text;
            Settings1.Default.destination_file_textBox_two = destination_file_textBox_two.Text;
            Settings1.Default.textBox_copy_destinationfileend = textBox_copy_destinationfileend.Text;
            Settings1.Default.source_copyfile_textBox_sync = source_copyfile_textBox_sync.Text;
            Settings1.Default.destination_file_textBox_sync = destination_file_textBox_sync.Text;
            Settings1.Default.destination_file_textBox_two_sync = destination_file_textBox_two_sync.Text;
            Settings1.Default.textBox_copy_destinationfileend_sync = textBox_copy_destinationfileend_sync.Text;
            Settings1.Default.textBox_key = textBox_key.Text;

            Settings1.Default.pictureBox_interface_ismin=!pictureBox_interface_ismin;

            Settings1.Default.Setting_textBox_section1 = textBox_section1.Text;
            Settings1.Default.Setting_textBox_section2 = textBox_section2.Text;
            Settings1.Default.Setting_textBox_section3 = textBox_section3.Text;
            Settings1.Default.Setting_textBox_section4 = textBox_section4.Text;
            Settings1.Default.Setting_textBox_section5 = textBox_section5.Text;
            Settings1.Default.Setting_source_copyfile_textBox_rename = source_copyfile_textBox_rename.Text;
            Settings1.Default.Setting_source_copyfile_prefix_textBox_rename = source_copyfile_prefix_textBox_rename.Text;
            Settings1.Default.Setting_source_copyfile_suffix_textBox_rename = source_copyfile_suffix_textBox_rename.Text;
            Settings1.Default.Setting_checkBox10 = checkBox10.Checked;
            Settings1.Default.Setting_checkBox_systemtime_rename = checkBox_systemtime_rename.Checked;
            Settings1.Default.Setting_checkBox6 = checkBox6.Checked;
            Settings1.Default.rename_mode_sel=comboBox7.SelectedIndex;
            Settings1.Default.Setting_checkBox_delete_srcfile=checkBox_delete_srcfile.Checked;
            Settings1.Default.Setting_textBoxSaveDir=textBoxSaveDir.Text;
            Settings1.Default.Save();
        }


        const int WM_SYSCOMMAND = 0x112;

        const int SC_CLOSE = 0xF060;

        const int SC_MINIMIZE = 0xF020;

        const int SC_MAXIMIZE = 0xF030;

        const int SC_RESTORE = 61728;
        //窗体按钮的拦截函数
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)
            {
                if (m.WParam.ToInt32() == SC_RESTORE)
                {
                    //log.Info("窗口还原！");
                }
                if (m.WParam.ToInt32() == SC_MINIMIZE)  //拦截最小化按钮
                {
                    //这里写操作代码
                    //log.Info("点击最小化按钮！");
                }
                if (m.WParam.ToInt32() == SC_MAXIMIZE)   //拦截窗体最大化按钮
                {
                    //log.Info("点击最大化按钮！");
                    //pictureBoxinterface_Click(null, null);
                    SC_MAX = true;
                    //BackgroundWorker work = new BackgroundWorker();

                    //work.DoWork += new DoWorkEventHandler(pictureBoxinterface_Click);
                    ////work.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SettingTrimWrite_Completed);

                    //work.RunWorkerAsync();
                }
                if (m.WParam.ToInt32() == SC_CLOSE)       //拦截窗体关闭按钮
                {
                    //log.Info("点击窗口关闭按钮！");
                }
            }

             base.WndProc(ref m);
        }

        private void button34_Click(object sender, EventArgs e)
        {
            double x0, y0, x1, y1,k;
            x0 = Convert.ToDouble(textBox_input_X0.Text);
            y0 = Convert.ToDouble(textBox_input_Y0.Text);
            x1 = Convert.ToDouble(textBox_input_X1.Text);
            if (comboBox5.SelectedIndex == 0)
            {
                y1 = Convert.ToDouble(textBox_input_Y1.Text);
                k = (y1 - y0) / (x1 - x0);
                textBox_output_formula.Text = "y=(" + y1.ToString() + ")+(" + k.ToString("f3") + ")*x -(" + (k * x1).ToString("f3") + ");";
            }
            else if (comboBox5.SelectedIndex == 1)
            {
                y1 = (x1 * y0) / x0;
                textBox_input_Y1.Text = y1.ToString("f3");
            }
            else if (comboBox5.SelectedIndex == 2)
            {
                y1 = x0 / (x0 + y0) * x1;
                textBox_input_Y1.Text = y1.ToString("f3");
            }
            
        }
        private void comboBox5_DropDownClosed(object sender, EventArgs e)
        {
            
        }

        private void button29_Click(object sender, EventArgs e)
        {
            textBox_input_X1.Text = textBox_input_X0.Text;
            textBox_input_Y1.Text = textBox_input_Y0.Text;
        }

        private void textBoxTrim_Leave(object sender, EventArgs e)
        {
            ((TextBox)sender).Text = ((TextBox)sender).Text.Trim();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.L && e.Control)
            {
                lock_button_Click(null,null);
            }
        }

        private void TabCrontrol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((TabCrontrol.SelectedIndex == 0) || (TabCrontrol.SelectedIndex == 1) || (TabCrontrol.SelectedIndex == 2) || (TabCrontrol.SelectedIndex == 3) || (TabCrontrol.SelectedIndex == 4))
            {
                pictureBoxinterface_Doing(true);
            }
            else
            {
                pictureBoxinterface_Doing(false);
            }
        }

        private void button39_Click(object sender, EventArgs e)
        {
            double x0, y0, k;
            x0 = Convert.ToDouble(textBox75.Text);
            y0 = Convert.ToDouble(textBox79.Text);
            k = x0 / y0 * 1000000;
            textBox76.Text = k.ToString("0.000");
        }
        private void textBox87_TextChanged(object sender, EventArgs e)
        {
            UInt32 x0, y0;
            x0 = Convert.ToUInt32(textBox87.Text);
            y0 = Convert.ToUInt32(textBox79.Text);
            if (x0 > y0) x0 = x0 - y0;
            else x0 = y0-x0;
            
        }

        private void button21_Click(object sender, EventArgs e)
        {
            uint init = Convert.ToUInt32(textBox_init.Text,16);
            byte []data = strToToHexByte(textBox_CRCInput.Text.Replace("0x","").Replace("0X", ""));
            uint crc= ll_crc24_generate(init,data, (int)(data.Length));
            textBox_Reault.Text = crc.ToString("x8");
        }

        private void button41_Click(object sender, EventArgs e)
        {
            Byte channel = Convert.ToByte(textBox_channel.Text);
            byte[] data = strToToHexByte(textBox_whitening.Text.Replace("0x", "").Replace("0X", ""));
            if(checkBox_ReverseBit_input.Checked==true)
            {
                for (uint i = 0; i < data.Length; i++)
                {
                    data[i] = reverseBits(data[i]);
                }
            }
            Byte ignore = Convert.ToByte(textBox_Ignore_fb.Text);
            if ((ignore< data.Length) && (ignore!=0))
            {
                bleWhiten(channel, data, ignore, (byte)(data.Length- ignore));
            }
            else
            {
                bleWhiten(channel, data, 0,(byte)(data.Length));
            }
            if (checkBox_ReverseBit_output.Checked == true)
            {
                for (uint i = 0; i < data.Length; i++)
                {
                    data[i] = reverseBits(data[i]);
                }
            }
            textBox_whitening_out.Text = "0x"+byteToHexStr(data);
        }
        private void button42_Click(object sender, EventArgs e)
        {
            byte[] data = strToToHexByte(textBox_whitening.Text.Replace("0x", "").Replace("0X", ""));
            for (uint i = 0; i < data.Length; i++)
            {
                data[i] = reverseBits(data[i]);
            }
            textBox_whitening_out.Text = "0x" + byteToHexStr(data);
        }
    }
}
