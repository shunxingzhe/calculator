using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.International.Converters.PinYinConverter;
using System.Text.RegularExpressions;


//string转byte[]:
//byte[] byteArray = System.Text.Encoding.Default.GetBytes ( str );
//byte[]转string：
//string str = System.Text.Encoding.Default.GetString ( byteArray );
//string转ASCII byte[]:
//byte[] byteArray = System.Text.Encoding.ASCII.GetBytes ( str );
//ASCII byte[]转string:
//string str = System.Text.Encoding.ASCII.GetString ( byteArray );
//string filePath = "C:\\1.txt";
//string str = "获取文件的全路径：" + Path.GetFullPath(filePath); //-->C:1.txt
//str = "获取文件所在的目录：" + Path.GetDirectoryName(filePath); //-->C:
//str = "获取文件的名称含有后缀：" + Path.GetFileName(filePath); //-->1.txt
//str = "获取文件的名称没有后缀：" + Path.GetFileNameWithoutExtension(filePath); //-->1
//str = "获取路径的后缀扩展名称：" + Path.GetExtension(filePath); //-->.txt
//str = "获取路径的根目录：" + Path.GetPathRoot(filePath); //-->C:\

namespace SYD_COPY_FILE
{
    public partial class Form1
    {
        public string byteToHexStr(byte[] bytes)
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
        public StringBuilder byteToHexStrBuilder(byte[] bytes)
        {
            var returnStr = new System.Text.StringBuilder("");
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr.Append(bytes[i].ToString("X2"));
                }
            }
            return returnStr;
        }
        public string byteToHexStr(byte[] bytes, int index, int count)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = index; i < count; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
        /*
         * 字符串转16进制数据
         * */
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
        public byte[] strToToHexByteAddZero(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2+1];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            returnBytes[returnBytes.Length - 1] = 0;
            return returnBytes;
        }

        public string StringToHexString(string s, Encoding encode)
        {
            byte[] b = encode.GetBytes(s);//按照指定编码将string编程字节数组
            string result = string.Empty;
            for (int i = 0; i < b.Length; i++)//逐字节变为16进制字符，以%隔开
            {
                result += "%" + Convert.ToString(b[i], 16);
            }
            return result;
        }
        //588416352  "X8"    588416352-->0x23128560
        private string StringToHexString(string s, string f)
        {
            UInt64 data = Convert.ToUInt64(s);
            return "0x"+data.ToString(f);
        }
        //0B 0A 09 08
        public string HexStringToString(string hs, Encoding encode)
        {
            //以%分割字符串，并去掉空字符
            string[] chars = hs.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] b = new byte[chars.Length];
            //逐个字符变为16进制字节数据
            for (int i = 0; i < chars.Length; i++)
            {
                b[i] = Convert.ToByte(chars[i], 16);
            }
            //按照指定编码将字节数组变为字符串
            return encode.GetString(b);
        }
        ////0x23128560-->588416352
        private string HexStringToString(string hs)
        {
            hs= hs.Replace("0X", "").Replace("0x", "");
            UInt64 data = Convert.ToUInt64(hs, 16);
            return data.ToString();
        }

        public static string byteToHexText(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    if ((i % 16 == 0) && (i > 0))
                    {
                        returnStr += "\r\n";
                    }
                    returnStr += "0x" + bytes[i].ToString("X2") + ",";
                }
            }
            return returnStr;
        }

        public static UInt32 getUInt32FromString(string str,int index)
        {
            string str1 = str.Substring(index + 6, 2) + str.Substring(index + 4, 2) + str.Substring(index + 2, 2) + str.Substring(index, 2);
            return  Convert.ToUInt32(str1, 16);
        }
        public static UInt16 getUInt16FromString(string str, int index)
        {
            string str1 =  str.Substring(index + 2, 2) + str.Substring(index, 2);
            return Convert.ToUInt16(str1, 16);
        }
        public static string getStringFromUInt32(UInt32 data)
        {
            return ((Byte)data & 0xff).ToString("X2") + ((Byte)(data >> 8) & 0xff).ToString("X2") + ((Byte)(data >> 16) & 0xff).ToString("X2") + ((Byte)(data >> 24) & 0xff).ToString("X2"); 
        }
        public static string getStringFromUInt16(UInt16 data)
        {
            return ((Byte)data & 0xff).ToString("X2") + ((Byte)(data >> 8) & 0xff).ToString("X2") ;
        }


        //开头匹配一个字母或数字+匹配两个十进制数字+匹配一个字母或数字+匹配两个相同格式的的（-加数字）+已字母或数字结尾
        //如：1111-111-1119
        //string pattern = @"^[a-zA-Z0-9]\d{2}[a-zA-Z0-9](-\d{3}){2}[A-Za-z0-9]$";
        //string pattern = @"^[a-zA-Z0-9]\d{2}$"; //开头以字母或数字，然后后面加两个数字字符
        //string pattern = @"^[a-zA-Z0-9]*$"; //匹配所有字符都在字母和数字之间
        //string pattern = @"^[a-z0-9]*$"; //匹配所有字符都在小写字母和数字之间
        //string pattern = @"^[A-Z][0-9]*$"; //以大写字母开头，后面的都是数字
        //string pattern = @"^\d{3}-\d{2}$";//匹配 333-22 格式,三个数字加-加两个数字
        //只有数字
        public static bool IsOnlyNumber(string value)
        {
            System.Text.RegularExpressions.Regex g = new System.Text.RegularExpressions.Regex(@"^[0-9]*$");
            return g.IsMatch(value);
        }

        //只有字母
        public static bool IsOnlyWord(string value)
        {
            Regex r = new Regex(@"^[a-zA-Z]*$");
            return r.Match(value).Success;
        }

        //只有数字和字母
        public static bool IsNumberAndWord(string value)
        {
            Regex r = new Regex(@"^[a-zA-Z0-9]*$");
            return r.Match(value).Success;
        }
        //去掉字符串里除汉字、英文字母、数字、空格之外的字符
        public static string RemoveHiddenCharacter(string input)
        {
            string inputReplaced = null;
            Regex regex = new Regex(@"([^\u4e00-\u9fa5 -~\s].*?)");
            inputReplaced = regex.Replace(input, "");

            return inputReplaced;
        }
    }
    public class PinYinConverterHelp
    {
        public static PingYinModel GetTotalPingYin(string str)
        {
            var chs = str.ToCharArray();
            //记录每个汉字的全拼
            Dictionary<int, List<string>> totalPingYins = new Dictionary<int, List<string>>();
            for (int i = 0; i < chs.Length; i++)
            {
                var pinyins = new List<string>();
                var ch = chs[i];
                //是否是有效的汉字
                if (ChineseChar.IsValidChar(ch))
                {
                    ChineseChar cc = new ChineseChar(ch);
                    pinyins = cc.Pinyins.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

                    //去除声调，转小写
                    pinyins = pinyins.ConvertAll(p => Regex.Replace(p, @"\d", "").ToLower());
                    //去重
                    pinyins = pinyins.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
                }
                else
                {
                    pinyins.Add(ch.ToString());
                }

                if (pinyins.Any())
                {
                    totalPingYins[i] = pinyins;
                }
            }
            PingYinModel result = new PingYinModel();
            foreach (var pinyins in totalPingYins)
            {
                var items = pinyins.Value;
                if (result.TotalPingYin.Count <= 0)
                {
                    result.TotalPingYin = items;
                    result.FirstPingYin = items.ConvertAll(p => p.Substring(0, 1)).Distinct().ToList();
                }
                else
                {
                    //全拼循环匹配
                    var newTotalPingYins = new List<string>();
                    foreach (var totalPingYin in result.TotalPingYin)
                    {
                        newTotalPingYins.AddRange(items.Select(item => totalPingYin + item));
                    }
                    newTotalPingYins = newTotalPingYins.Distinct().ToList();
                    result.TotalPingYin = newTotalPingYins;

                    //首字母循环匹配
                    var newFirstPingYins = new List<string>();
                    foreach (var firstPingYin in result.FirstPingYin)
                    {
                        newFirstPingYins.AddRange(items.Select(item => firstPingYin + item.Substring(0, 1)));
                    }
                    newFirstPingYins = newFirstPingYins.Distinct().ToList();
                    result.FirstPingYin = newFirstPingYins;
                }
            }
            return result;
        }
    }
    public class PingYinModel
    {
        public PingYinModel()
        {
            TotalPingYin = new List<string>();
            FirstPingYin = new List<string>();
        }

        //全拼
        public List<string> TotalPingYin { get; set; }

        //首拼
        public List<string> FirstPingYin { get; set; }
    }
}
