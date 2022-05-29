using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SparkFormEditor
{
    ///<summary>
    /// 调整页顺序
    ///</summary>
    internal partial class Frm_EditPageOrder : Form
    {
        //返回调用界面的设置参数：
        private int _id = 1;
        public int ID { get { return _id; } }

        int _MaxPage = 1;
        int _CurrentPage = 1;

        public Frm_EditPageOrder()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 构造方法
        /// 传入最大页数和当前页数
        /// </summary>
        /// <param name="maxPage"></param>
        /// <param name="currentPage"></param>
        public Frm_EditPageOrder(int maxPage,int currentPage)
        {
            InitializeComponent();

            _MaxPage = maxPage;
            _CurrentPage = currentPage;

            numericUpDown1.Value = currentPage;
            numericUpDown1.Enabled = false;

            numericUpDown2.Value = currentPage;
            numericUpDown2.Minimum = 1;
            numericUpDown2.Maximum = maxPage;

            this.Text += " ： 1 ～ " + _MaxPage.ToString();
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 确定按钮按下：根据设定的页号，重新设定xml数据中的页号SN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            //验证          
            if (numericUpDown2.Value > _MaxPage)
            {
                MessageBox.Show("修改后的页号不能大于最大页数。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numericUpDown2.Value == _CurrentPage)
            {
                MessageBox.Show("修改后的页号不能是原页号。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //设置返回结果 
            _id = (int)numericUpDown2.Value;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonOK_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void numericUpDown2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                buttonOK_Click(null, null);
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown2.Value = (int)numericUpDown2.Value;
        }

        private void numericUpDown2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.')
                e.Handled = true;  //只能输入整数
        }
    }
}
