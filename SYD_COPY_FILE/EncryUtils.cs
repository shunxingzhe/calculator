using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SYD_COPY_FILE
{
    public partial class Form1
    {
        private static string key = "cd";
        private static string iv = "ib202011";   

        static public byte[] GetEncryData(byte[] src, int length,string external_key)
        {

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            if (external_key == null)
            {
                des.Key = Encoding.ASCII.GetBytes(key + "chengd");
            }
            else
            {
                des.Key = Encoding.ASCII.GetBytes(external_key + key);
            }
            des.IV = Encoding.ASCII.GetBytes(iv);

            ICryptoTransform encryptor = des.CreateEncryptor();
            return encryptor.TransformFinalBlock(src, 0, length);

        }
        static public byte[] GetDeEncryData(byte[] encrySrc, string external_key)
        {

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            if (external_key == null)
            {
                des.Key = Encoding.ASCII.GetBytes(key + "chengd");
            }
            else
            {
                des.Key = Encoding.ASCII.GetBytes(external_key + key);
            }
            des.IV = Encoding.ASCII.GetBytes(iv);

            ICryptoTransform decryptor = des.CreateDecryptor();

            byte[] buf=null;
            try
            {
                buf=decryptor.TransformFinalBlock(encrySrc, 0, encrySrc.Length);
            }
            catch (CryptographicException)
            {
                buf = null;
            }
            return buf;
        }
    }

}
