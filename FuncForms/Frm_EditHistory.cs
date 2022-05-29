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
    /// 留痕修改 保留修改历史 修订历史
    ///</summary>
    internal partial class Frm_EditHistory : Form
    {
        //返回调用界面的设置参数：
        private string _newText = "";

        public string NewText { get { return _newText; } }


        public Frm_EditHistory()
        {
            InitializeComponent();

            richTextBoxInput.Focus();
        }

        /// <summary>
        /// 实例化 构造方法
        /// </summary>
        /// <param name="selectedText">选中的需要修改的文字</param>
        /// <param name="historyTexts">历史信息</param>
        public Frm_EditHistory(string selectedText, string historyTexts)
        {
            InitializeComponent();

            richTextBoxInput.Focus();

            this.richTextBoxSelected.Text = selectedText;

            if (historyTexts.Trim() == "")
            {
                this.listBoxHistory.Items.Clear();
            }
            else
            {
                string[] arr = historyTexts.Split('§');

                for (int i = 0; i < arr.Length; i++)
                {
                    this.listBoxHistory.Items.Add(arr[i]);
                }
            }
        }

        /// <summary>
        /// 取消按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// OK确定按钮按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            //设置返回结果
            _newText = richTextBoxInput.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 光标进入，解决全角的问题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBoxInput_Enter(object sender, EventArgs e)
        {
            //解决Tab键切换，还是全角的问题 （都是在Enter中处理）
            if (this.richTextBoxInput.ImeMode != ImeMode.Hangul)
            {
                this.richTextBoxInput.ImeMode = ImeMode.Hangul;
            }
        }
    }
}
