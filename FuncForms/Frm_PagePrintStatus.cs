using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace SparkFormEditor
{
    /// <summary>
    /// 打印页状态，查看/修改
    /// </summary>
    internal partial class Frm_PagePrintStatus : Form
    {
        string _PagePrintStatus = "";
        int _MaxPage = 1;

        //返回调用界面的设置参数：
        private string _New_PagePrintStatus;

        public string New_PagePrintStatus { get { return _New_PagePrintStatus; } }

        public Frm_PagePrintStatus()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 实例化构造方法
        /// </summary>
        /// <param name="PagePrintStatusPara"></param>
        /// <param name="maxPagePara"></param>
        public Frm_PagePrintStatus(string PagePrintStatusPara, int maxPagePara)
        {
            InitializeComponent();

            _PagePrintStatus = PagePrintStatusPara;

            _MaxPage = maxPagePara;

            InitPages();

            this.MinimumSize = this.Size;
        }

        /// <summary>
        /// 根据页状态，显示列表
        /// </summary>
        private void InitPages()
        {
            try
            {
                checkedListBox1.Items.Clear();

                ArrayList list = ArrayList.Adapter(_PagePrintStatus.Split('¤'));
                string page = "";
                for (int i = 0; i < _MaxPage; i++)
                {
                    page = (i + 1).ToString();
                    checkedListBox1.Items.Add(page);

                    //包含的数据都为已经打印过的
                    if (list.Contains(page))
                    {
                        checkedListBox1.SetItemChecked(i, true);  //设置为选中
                    }
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, checkBox1.Checked);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _New_PagePrintStatus = "";

            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
                if (_New_PagePrintStatus == "")
                {
                    _New_PagePrintStatus = checkedListBox1.CheckedItems[i].ToString();
                }
                else
                {
                    _New_PagePrintStatus += "¤" + checkedListBox1.CheckedItems[i].ToString();
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
