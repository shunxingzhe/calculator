using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SYD_COPY_FILE
{
    public partial class Form1
    {
        public static string GetMD5HashCodeFromFile(string filePath)
        {
            try
            {
                using (FileStream fStream = new FileStream(filePath, System.IO.FileMode.Open))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(fStream);

                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        builder.Append(retVal[i].ToString("X2"));
                    }
                    return builder.ToString();
                };
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashCodeFromFile() fail,error:" + ex.Message);
            }
        }

        public static byte[] GetMD5HashBytesFromFile(string filePath)
        {
            try
            {
                using (FileStream fStream = new FileStream(filePath, System.IO.FileMode.Open))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    return md5.ComputeHash(fStream);
                };
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashBytesFromFile() fail,error:" + ex.Message);
            }
        }
    }


}
