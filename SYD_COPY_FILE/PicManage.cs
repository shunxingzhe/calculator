using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing.Drawing2D;

namespace SYD_COPY_FILE
{
    class PicManage
    {

        public static bool SplitPic(string fromFile,
            int startX, int startY, int width, int height, string fileSaveUrl)
        {
            using (FileStream sFileStream = new FileStream(fromFile, FileMode.Open))
            {
                Image initImage = Image.FromStream(sFileStream, true);

                int quality = 100; //图片质量
                Image destImage = null;
                bool ret = SplitPic(initImage, startX, startY, width, height, ref destImage);

                //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                ImageCodecInfo ici = GetCodeInfo(fileSaveUrl);
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
                destImage.Save(fileSaveUrl, ici, ep);
            }

            return true;
        }

        public static ImageCodecInfo GetCodeInfo(string filePath)
        {
            string fileType = Path.GetExtension(filePath).ToLower();

            ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo i in icis)
            {
                if (fileType == ".jpg" && i.MimeType == "image/jpeg")
                    return i;
                if (fileType == ".png" && i.MimeType == "image/png")
                    return i;
                if (fileType == ".bmp" && i.MimeType == "image/bmp")
                    return i;
            }
            return null;
        }
        public static bool SplitPic(Image initImage,
          int startX, int startY, int width, int height, ref Image destImage)
        {
            if (startX >= initImage.Width || startY >= initImage.Height)
                return false;

            //计算最大宽度 长度
           int width2 = Math.Min(width, initImage.Width - startX);
           int height2 = Math.Min(height, initImage.Height - startY);

            int destHeight;
            int destWidth = 0;
            //if (destWidth == 0)
            //{
            //    destWidth = width2;
            //    destHeight = height2;
            //}
            //else
            //{
            //    destHeight = (height2 * destWidth) / width2;
            //}
            destWidth = width2;
            destHeight = height2;

            width = width2;
            height = height2;

            //裁剪对象
            System.Drawing.Image pickedImage = null;
            System.Drawing.Graphics pickedG = null;

            //定位
            Rectangle fromR = new Rectangle(0, 0, 0, 0);//原图裁剪定位
            Rectangle toR = new Rectangle(0, 0, 0, 0);//目标定位

            //裁剪对象实例化
            pickedImage = new System.Drawing.Bitmap(destWidth, destHeight);
            pickedG = System.Drawing.Graphics.FromImage(pickedImage);

            //裁剪源定位
            fromR.X = startX;
            fromR.Y = startY;
            fromR.Width = width;
            fromR.Height = height;

            //裁剪目标定位
            toR.X = 0;
            toR.Y = 0;
            toR.Width = destWidth;
            toR.Height = destHeight;

            //设置质量
            pickedG.InterpolationMode = InterpolationMode.HighQualityBicubic;
            pickedG.SmoothingMode =  SmoothingMode.HighQuality;
            //裁剪
            pickedG.DrawImage(initImage, toR, fromR, System.Drawing.GraphicsUnit.Pixel);

            destImage = pickedImage;
            return true;
        }

        public static bool ZoomAuto(Image initImage, ref Image newImage,
            Double newWidth, Double newHeight)
        {

            //生成新图
            //新建一个bmp图片
            newImage = new System.Drawing.Bitmap((int)newWidth, (int)newHeight);
            //新建一个画板
            System.Drawing.Graphics newG = System.Drawing.Graphics.FromImage(newImage);

            //设置质量
            newG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            newG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //置背景色
            newG.Clear(Color.White);
            //画图
            newG.DrawImage(initImage, new System.Drawing.Rectangle(0, 0, newImage.Width, newImage.Height), new System.Drawing.Rectangle(0, 0, initImage.Width, initImage.Height), System.Drawing.GraphicsUnit.Pixel);

            //释放资源
            newG.Dispose();
            return true;
        }

        public static Image GetFromFile(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            int byteLength = (int)fileStream.Length;
            byte[] fileBytes = new byte[byteLength];
            fileStream.Read(fileBytes, 0, byteLength);

            //文件流关閉,文件解除锁定
            fileStream.Close();

            return Image.FromStream(new MemoryStream(fileBytes));
        }
    }


    class ZoomRate
    {
        //每次缩放比例
        int _zoomRatePerAction = 10;

        //合计缩放比例
        double _totalRate = 1;

        int _width;
        int _height;

        public void SetSize(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public double GetRate()
        {
            return _totalRate;
        }

        public void SetRate(int rate)
        {
            _zoomRatePerAction = rate;
        }
        public void Reset()
        {
            _totalRate = 1;
        }

        public void GetNextZoomIn(ref int widthNew, ref int heightNew)
        {
            _totalRate -= (_totalRate * _zoomRatePerAction) / 100;
            widthNew = (int)(_width * _totalRate);
            heightNew = (int)(_height * _totalRate);
        }

        public void GetNextZoomOut(ref int widthNew, ref int heightNew)
        {
            _totalRate += (_totalRate * _zoomRatePerAction) / 100;

            widthNew = (int)(_width * _totalRate);
            heightNew = (int)(_height * _totalRate);
        }

    }

}
