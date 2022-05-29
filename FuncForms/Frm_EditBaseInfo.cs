using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SparkFormEditor
{
    /// <summary>
    /// 编辑表单抬头：病人接本信息；双击表单的基本信息，即可打开本界面进行修改
    /// </summary>
    internal partial class Frm_EditBaseInfo : Form
    {
        //返回调用界面的设置参数：
        private string _newText = "";

        public string NewText { get { return _newText; } }

        private string editName = "";

        public Frm_EditBaseInfo()
        {
            InitializeComponent();         
        }

        /// <summary>
        /// 实例化构造方法
        /// </summary>
        /// <param name="namePara">标签名</param>
        /// <param name="currentPara">当前内容</param>
        /// <param name="historyPara">修改后的内容，暂时无用，留作扩展</param>
        public Frm_EditBaseInfo(string namePara, string currentPara, string historyPara)
        {
            InitializeComponent();

            this.Text = "修改基本信息：" + namePara;

            editName = namePara;

            this.textBoxCurrent.Text = currentPara;

            this.MinimumSize = this.Size;

            //if (historyPara == "")
            //{
            //    this.textBoxHistory.Text = currentPara;
            //}
            //else
            //{
            //    this.textBoxHistory.Text = historyPara;
            //}

            修改后内容.SetSelectWords("→¤√¤←"); //设置特殊字符菜单:→¤√¤←¤§  会导致每次打开窗体，赋值现有内容
            修改后内容.Text = currentPara;
            修改后内容.Focus();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (editName == "_PageNo")
            {
                int temp = 0;
                if (!int.TryParse(修改后内容.Text, out temp))
                {
                    MessageBox.Show("页数只能为数字。", "提示信息");
                    return;
                }
            }


            //设置返回结果
            _newText = 修改后内容.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 如果诊断等文字很长，要进行修改。需要复制到输入框然后修改，这里进行双击，如果空白就把原来内容先填写上去。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 修改后内容_DoubleClick(object sender, EventArgs e)
        {
            //空的时候，双击才复制原来内容。防止误双击，内容被替换。
            if (string.IsNullOrEmpty(修改后内容.Text.Trim()))
            {
                修改后内容.Text = this.textBoxCurrent.Text;

                修改后内容.SelectionStart = 修改后内容.Text.Length;
            }
        }

    }
}
