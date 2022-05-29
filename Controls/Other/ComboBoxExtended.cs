using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace SparkFormEditor
{
    /// <summary>
    /// 自定义下拉框控件
    /// </summary>
    internal partial class ComboBoxExtended : ComboBox
    {
        public string _Default = ""; //默认值
        public string _Score = ""; //评分表达式

        public string CreateUser = "";

        public ComboBoxExtended()
        {
            InitializeComponent();

            //this.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
        }

        /// <summary>
        /// 默认值读取、设置
        /// </summary>
        public string Default
        {
            get { return _Default; }
            set
            {
                _Default = value;
                this.Text = value;
            }
        }

        /// <summary>
        /// 评分表达式
        /// </summary>
        public string Score
        {
            get { return _Score; }
            set
            {
                _Score = value;
            }
        }

        public void ResetDefault()
        {
            this.Text = _Default;

            this.CreateUser = "";
        }

        //private void ComboBoxExtended_KeyDown(object sender, KeyEventArgs e)
        //{
        //    //if (e.KeyCode == Keys.Enter)
        //    //{
        //    //    //base.OnKeyDown(e);
        //    //    SendKeys.Send("{TAB}"); //Tab 光标迁移到下一个控件上
        //    //}
        //}

        /// <summary>
        /// 重写捕获按键处理：
        /// 光标迁移和复制粘贴
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                //如果展开下拉项目，选择后按下回车，那么需要选择该项目的;DropDownList还是依然还是可以的
                //KeyEventArgs e = new KeyEventArgs(keyData);
                //base.OnKeyDown(e);
                //SendKeys.Send("{ENTER}");
                //this.RefreshItems();
                //this.OnMouseClick(null);

                //光标迁移到下一个控件，和Tab键一样的效果
                //SendKeys.Send("\t");
                SendKeys.Send("{TAB}"); //Tab
                //SendKeys.Send("^ +{TAB}"); //Shift + Tab
            }

            return false; //如果想要焦点保持在原控件则返回true
        }

        ////解决居中问题
        //private void ComboBoxExtended_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    string s = this.Items[e.Index].ToString();
        //    SizeF ss = e.Graphics.MeasureString(s, e.Font);

        //    float l = (float)(e.Bounds.Width - ss.Width) / 2;
        //    if (l < 0) l = 0f;
        //    float t = (float)(e.Bounds.Height - ss.Height) / 2;
        //    if (t < 0) t = 0f;
        //    t = t + this.ItemHeight * e.Index;
        //    e.DrawBackground();
        //    e.DrawFocusRectangle();
        //    e.Graphics.DrawString(s, e.Font, new SolidBrush(e.ForeColor), l, t);
        //}
    }
}
