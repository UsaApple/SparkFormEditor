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
    /// 自定义勾选框
    /// </summary>
    internal partial class CheckBoxExtended : SkinCheckBox   //CheckBoxThreeState  //SkinCheckBox
    {
        public bool _Default = false; //默认值
        public CheckState _DefaultCheckState = CheckState.Unchecked;

        public string _Score = ""; //评分表达式

        public string CreateUser = "";

        public CheckBoxExtended()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 默认值读取、设置
        /// </summary>
        public bool Default
        {
            get { return _Default; }
            set 
            { 
                _Default = value;
                this.Checked = value;
            }
        }

        public CheckState DefaultCheckState
        {
            get { return _DefaultCheckState; }
            set
            {
                _DefaultCheckState = value;
                this.CheckState = value;
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

        /// <summary>
        /// 重置
        /// </summary>
        public void ResetDefault()
        {
            this.Checked = _Default;

            if (this.ThreeState)
            {
                this.CheckState = _DefaultCheckState;
            }

            this.CreateUser = "";
        }

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
                //光标迁移到下一个控件，和Tab键一样的效果
                //SendKeys.Send("\t");
                SendKeys.Send("{TAB}"); //Tab
                //SendKeys.Send("^ +{TAB}"); //Shift + Tab
            }

            return false; //如果想要焦点保持在原控件则返回true
        }
    }
}
