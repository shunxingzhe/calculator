using System;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using COSXML.Model.Bucket;
using COSXML.CosException;
using COSXML.Model.Service;
using COSXML.Model.Tag;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using COSXML.Transfer;

namespace SYD_COPY_FILE
{
    public partial class Form1
    {

        #region define 
        private Bitmap Bitmap_Bucket;
        private Bitmap Bitmap_Folder;
        private Bitmap Bitmap_Zip;
        private Bitmap Bitmap_File;
        public enum dataGridViewType
        {
            FILE = 0,
            ZIP = 1,
            BUCKET = 2,
        }
        //初始化 CosXmlConfig 
        //static string appid = "1304918232";//设置腾讯云账户的账户标识 APPID
        static string region = "ap-nanjing"; //设置一个默认的存储桶地域
        public string nextMarker;
        CosXmlConfig config = new CosXmlConfig.Builder()
          .IsHttps(true)  //设置默认 HTTPS 请求
          .SetRegion(region)  //设置一个默认的存储桶地域
          .SetDebugLog(true)  //显示日志
          .Build();  //创建 CosXmlConfig 对象

        static string secretId = "AKIDELCn7AxWt0sSlnZ4IHzRcW9WcRzHH0Sd"; //"云 API 密钥 SecretId";
        static string secretKey = "wbz6xWpqc7LIyOXP0KNAgvta091cMp2y"; //"云 API 密钥 SecretKey";
        static long durationSecond = 600;  //每次请求签名有效时长，单位为秒
        QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(secretId, secretKey, durationSecond);
        CosXml cosXml;
        public class ItemCosCol
        {
            public Bitmap imageColumn { get; set; }
            public string FileName { get; set; }
            public string ModifyTime { get; set; }
        }
        public List<ItemCosCol> ListCosCol = new List<ItemCosCol>();
        string bucket = null;
        string folder_out= null;
        string file_in = null;

        public int ColumnIndexSelect = 0;
        public int RowIndexSelect = 0;
        #endregion
        public void cos_init()
        {
            cosXml = new CosXmlServer(config, cosCredentialProvider);

            Bitmap_Bucket = new Bitmap(global::SYD_COPY_FILE.Properties.Resources.Bucket);
            Bitmap_Folder = new Bitmap(global::SYD_COPY_FILE.Properties.Resources.Folder);
            Bitmap_File = new Bitmap(global::SYD_COPY_FILE.Properties.Resources.File);
            Bitmap_Zip = new Bitmap(global::SYD_COPY_FILE.Properties.Resources.zip);

            //for (int i = 0; i < 5; ++i)
            //{
            //    ItemCosCol item = new ItemCosCol();
            //    item.FileName = "Listitem1-" + i;
            //    item.ModifyTime = "Listitem2-" + i;
            //    ListCosCol.Add(item);
            //}
            dataGridViewCosBing(dataGridViewType.FILE);
        }
        public void dataGridViewCosBing(dataGridViewType type)
        {
            if(ListCosCol.Count>0)
            {
                dataGridViewCos.DataSource = null;
                dataGridViewCos.Rows.Clear();
                dataGridViewCos.DataSource = ListCosCol;
                //DataGridView的列name和对象成员的绑定
                dataGridViewCos.Columns["FileName"].DataPropertyName = "FileName";
                dataGridViewCos.Columns["ModifyTime"].DataPropertyName = "ModifyTime";
                if(type == dataGridViewType.BUCKET)
                {
                    dataGridViewCos.Columns["FileName"].HeaderText = "Bucket";
                    dataGridViewCos.Columns["ModifyTime"].HeaderText= "Region";
                }
                dataGridViewCos.Columns["imageColumn"].Width = 32;
                dataGridViewCos.Columns["imageColumn"].HeaderText = "Icon";
                dataGridViewCos.Columns["FileName"].Width = 405;
                dataGridViewCos.Columns["ModifyTime"].Width = 135;
            }
        }
        private void button_CosBuckets_Click(object sender, EventArgs e)
        {
            try
            {
                GetServiceRequest request = new GetServiceRequest();
                //执行请求
                GetServiceResult result = cosXml.GetService(request);
                //得到所有的 buckets
                List<ListAllMyBuckets.Bucket> allBuckets = result.listAllMyBuckets.buckets;
                ListCosCol.Clear();
                for (int i = 2; i < allBuckets.Count; i++)
                {
                    ItemCosCol item = new ItemCosCol();
                    item.FileName = allBuckets[i].name;
                    item.ModifyTime = allBuckets[i].location;

                    item.imageColumn = Bitmap_Bucket;

                    ListCosCol.Add(item);
                }
                dataGridViewCosBing(dataGridViewType.BUCKET);
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                //请求失败                
                MessageBox.Show("客户端请求失败:" + clientEx);
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                //请求失败
                string str = serverEx.GetInfo();
                MessageBox.Show("服务器端请求失败:" + str);
            }
        }
        private void button_commit_file_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = (Bitmap)dataGridViewCos.Rows[0].Cells[0].Value;
            if (bitmap == Bitmap_Zip)
            {
                if ((folder_out != null) || (file_in != null)) 
                {
                    string filename = Path.GetFileName(file_in);
                    File.Delete(file_in);
                    try
                    {
                        System.IO.Compression.ZipFile.CreateFromDirectory(folder_out, file_in); //压缩
                    }
                    catch (Exception ex)
                    {
                        //请求失败
                        MessageBox.Show("压缩失败" + ex);
                        return;
                    }
                    try
                    {
                        // 存储桶名称，此处填入格式必须为 bucketname-APPID, 其中 APPID 获取参考 https://console.cloud.tencent.com/developer
                        string key = filename; //对象键
                        string srcPath = file_in;//本地文件绝对路径

                        PutObjectRequest request = new PutObjectRequest(bucket, key, srcPath);
                        //设置进度回调
                        request.SetCosProgressCallback(delegate (long completed, long total)
                        {
                            label_schedule.Text=String.Format("{0:##.##}%", completed * 100.0 / total);
                        });
                        //执行请求
                        PutObjectResult result = cosXml.PutObject(request);
                        //对象的 eTag
                        string eTag = result.eTag;
                    }
                    catch (COSXML.CosException.CosClientException clientEx)
                    {
                        //请求失败
                        MessageBox.Show("CosClientException: " + clientEx);
                    }
                    catch (COSXML.CosException.CosServerException serverEx)
                    {
                        //请求失败
                        MessageBox.Show("CosServerException: " + serverEx.GetInfo());
                    }
                }
            }
        }
        private void dataGridViewCos_DragDrop(object sender, DragEventArgs e)
        {
            IDataObject dataObject = e.Data;

            if (dataObject == null) return;
            if ((folder_out == null) || (file_in == null)) return;

            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    string filename = Path.GetFileName(file);
                    File.Copy(@file, folder_out+"\\"+ filename, true);
                }
                Display_Folder(folder_out);
            }
        }
        private void Display_Folder(string folder_out)
        {
            List<string> zipfile = new List<string>();
            List<string> MtimeList = new List<string>();
            DirectorFileNameSize(folder_out, zipfile, MtimeList);//获取目录下的所有文件名
            ListCosCol.Clear();
            for (int i = 0; i < zipfile.Count; i++)
            {
                ItemCosCol item = new ItemCosCol();
                item.FileName = zipfile[i];
                item.ModifyTime = MtimeList[i];
                item.imageColumn = Bitmap_Zip;
                ListCosCol.Add(item);
            }
            dataGridViewCosBing(dataGridViewType.ZIP);
        }
        private void dataGridViewCos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            Bitmap bitmap = (Bitmap)dataGridViewCos. Rows[e.RowIndex].Cells[0].Value;
            string filename = (string)dataGridViewCos.Rows[e.RowIndex].Cells[1].Value; ;
            if (bitmap == Bitmap_Bucket)
            {
                bucket= (string)dataGridViewCos.Rows[e.RowIndex].Cells[1].Value;
                string region = (string)dataGridViewCos.Rows[e.RowIndex].Cells[2].Value;
                try
                {
                    //string bucket = "examplebucket-1250000000"; //格式：BucketName-APPID
                    GetBucketRequest request = new GetBucketRequest(bucket);
                    request.Region = region;
                    //执行请求
                    GetBucketResult result = cosXml.GetBucket(request);
                    //bucket的相关信息
                    ListBucket info = result.listBucket;
                    if (info.isTruncated)
                    {
                        // 数据被截断，记录下数据下标
                        this.nextMarker = info.nextMarker;
                    }
                    ListCosCol.Clear();
                    for (int i = 0; i < info.contentsList.Count; i++)
                    {
                        ItemCosCol item = new ItemCosCol();
                        item.FileName = info.contentsList[i].key;

                        DateTime samplingDate = Convert.ToDateTime(info.contentsList[i].lastModified);
                        item.ModifyTime = samplingDate.ToString("yyyy-MM-dd HH:mm:ss");

                        if (item.FileName[item.FileName.Length - 1] == '/') item.imageColumn = Bitmap_Folder;
                        else item.imageColumn = Bitmap_File;

                        ListCosCol.Add(item);
                    }
                    dataGridViewCosBing(dataGridViewType.FILE);
                }
                catch (COSXML.CosException.CosClientException clientEx)
                {
                    //请求失败
                    MessageBox.Show("CosClientException: " + clientEx);
                }
                catch (COSXML.CosException.CosServerException serverEx)
                {
                    //请求失败
                    MessageBox.Show("CosServerException: " + serverEx.GetInfo());
                }
            }
            else if (bitmap == Bitmap_File)
            {
                string mtime = (string)dataGridViewCos.Rows[e.RowIndex].Cells[2].Value;
                string ext = filename.Substring(filename.Length - 4, 4);
                string dir = ".\\temp";
                if (ext.ToLower() != ".zip")
                {
                    FolderBrowserDialog dialog = new FolderBrowserDialog();
                    dialog.Description = "选择目录";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        //获取选择的目录路径
                        dir = dialog.SelectedPath;
                    }
                    else return;
                }
                // 存储桶名称，此处填入格式必须为 bucketname-APPID, 其中 APPID 获取参考 https://console.cloud.tencent.com/developer
                //string bucket = "examplebucket-1250000000";
                //string key = "exampleobject"; //对象键
                string key = filename; //对象键
                //string localDir = System.IO.Path.GetTempPath();//本地文件夹
                string localDir = dir;//本地文件夹
                string localFileName = filename; //指定本地保存的文件名
                try
                {
                    GetObjectRequest request = new GetObjectRequest(bucket, key, localDir, localFileName);
                    //设置进度回调
                    request.SetCosProgressCallback(delegate (long completed, long total)
                    {
                        label_schedule.Text=String.Format("{0:##.##}%", completed * 100.0 / total);
                    });
                    //执行请求
                    GetObjectResult result = cosXml.GetObject(request);
                    //请求成功
                    //Console.WriteLine(result.GetResultInfo());
                }
                catch (COSXML.CosException.CosClientException clientEx)
                {
                    //请求失败
                    MessageBox.Show("CosClientException: " + clientEx);
                }
                catch (COSXML.CosException.CosServerException serverEx)
                {
                    //请求失败
                    MessageBox.Show("CosServerException: " + serverEx.GetInfo());
                }
                if (ext.ToLower() == ".zip")
                {
                    file_in = localDir + "\\" + localFileName;
                    folder_out = file_in.Replace(ext, "");
                    if (Directory.Exists(folder_out))
                    {
                        DirectoryInfo di = new DirectoryInfo(folder_out);
                        di.Delete(true);
                    }
                    try
                    {
                        System.IO.Compression.ZipFile.ExtractToDirectory(file_in, folder_out); //解压
                    }
                    catch (Exception ex)
                    {
                        //请求失败
                        MessageBox.Show("解压失败"+ ex);
                        return;
                    }
                    Display_Folder(folder_out);
                }
                else
                {
                    MessageBox.Show("下载成功");
                }
            }
        }
        private void contextMenuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            for (int i = 0; i < contextMenuStrip2.Items.Count; i++)
            {
                if (contextMenuStrip2.Items[i].Selected)
                {
                    if (contextMenuStrip2.Items[i].Text.Trim() == "删除")
                    {
                        Bitmap bitmap = (Bitmap)dataGridViewCos.Rows[RowIndexSelect].Cells[0].Value;
                        if (bitmap == Bitmap_Zip)
                        {
                            if (folder_out != null)
                            {
                                string filename = (string)dataGridViewCos.Rows[RowIndexSelect].Cells[1].Value; ;
                                File.Delete(folder_out + "\\" + filename);
                                Display_Folder(folder_out);
                            }
                        }
                    }
                }
            }
        }
        private void dataGridViewCos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ColumnIndexSelect = e.ColumnIndex;
            RowIndexSelect = e.RowIndex;
        }
    }
}

