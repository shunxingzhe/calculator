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

namespace SYD_COPY_FILE
{
    public partial class Form1
    {

        #region define 
        private Bitmap Bitmap_Bucket;
        private Bitmap Bitmap_Folder;
        private Bitmap Bitmap_File;
        public enum dataGridViewType
        {
            FILE = 0,
            FOLDER = 1,
            BUCKET = 2,
        }
        //初始化 CosXmlConfig 
        static string appid = "1304918232";//设置腾讯云账户的账户标识 APPID
        static string region = "ap-guangzhou"; //设置一个默认的存储桶地域
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
        #endregion
        public void cos_init()
        {
            cosXml = new CosXmlServer(config, cosCredentialProvider);

            Bitmap_Bucket = new Bitmap(global::SYD_COPY_FILE.Properties.Resources.Bucket);
            Bitmap_Folder = new Bitmap(global::SYD_COPY_FILE.Properties.Resources.Folder);
            Bitmap_File = new Bitmap(global::SYD_COPY_FILE.Properties.Resources.File);

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
                if (type== dataGridViewType.FILE)
                {
                    
                }
                else if (type == dataGridViewType.FOLDER)
                {
                    dataGridViewCos.Columns["FileName"].HeaderText = "FolderName";
                    //dataGridViewCos.Columns["ModifyTime"].HeaderText = "Region";
                }
                else if (type == dataGridViewType.BUCKET)
                {
                    dataGridViewCos.Columns["FileName"].HeaderText = "Bucket";
                    dataGridViewCos.Columns["ModifyTime"].HeaderText= "Region";
                }
                dataGridViewCos.Columns["imageColumn"].Width = 32;
                dataGridViewCos.Columns["imageColumn"].HeaderText = "Icon";
                dataGridViewCos.Columns["FileName"].Width = 300;
                dataGridViewCos.Columns["ModifyTime"].Width = 100;
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
        private void dataGridViewCos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            Bitmap bitmap = (Bitmap)dataGridViewCos. Rows[e.RowIndex].Cells[0].Value;
            if (bitmap == Bitmap_Bucket)
            {
                string bucket= (string)dataGridViewCos.Rows[e.RowIndex].Cells[1].Value;
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
}

