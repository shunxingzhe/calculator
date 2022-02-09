/*
 * 由SharpDevelop创建。
 * 用户： Administrator
 * 日期: 2018-09-19
 * 时间: 18:25
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SYD_COPY_FILE
{
	/// <summary>
	/// Description of FrmInputDialog.
	/// </summary>
	public partial class FrmInputDialog : Form
	{
		public delegate void TextEventHandler(string strText, string strText1);

        public TextEventHandler TextHandler;
		
		public FrmInputDialog(string x, string y)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent(x,y);
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		private void btnOk_Click(object sender, EventArgs e)
        {
			if (null != TextHandler)
			{
				TextHandler.Invoke(txtString.Text,textBox1.Text);
				DialogResult = DialogResult.OK;
			}
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void txtString_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Keys.Enter == (Keys)e.KeyChar)
            {
            	if (null != TextHandler)
				{
					TextHandler.Invoke(txtString.Text, textBox1.Text);
					DialogResult = DialogResult.OK;
	            }
            }
        }
	}
}
