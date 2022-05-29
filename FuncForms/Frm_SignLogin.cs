using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SparkFormEditor
{
    ///<summary>
    /// 双签名的时候，在本窗口验证工号密码，进行双签名，或者删除全部页时的验证
    ///</summary>
    internal partial class Frm_SignLogin : Form
    {
        protected SparkFormEditor _parent = null;

        //返回调用界面的设置参数：
        private string _Name = "";
        public string NameText { get { return _Name; } }

        private string _MEDIC_DEGREE = "";
        public string MEDIC_DEGREE { get { return _MEDIC_DEGREE; } }

        private string _ID = "";
        public string NameID { get { return _ID; } }

        public Frm_SignLogin()
        {
            InitializeComponent();
        }

        public Frm_SignLogin(string id, SparkFormEditor parent)
        {
            InitializeComponent();
            this.textBoxID.Text = id;
            _parent = parent;
        }

        /// <summary>
        /// 构造方法：工号不可以修改
        /// </summary>
        /// <param name="id"></param>
        /// <param name="idEnable"></param>
        public Frm_SignLogin(string id, bool idEnable, SparkFormEditor parent)
        {
            InitializeComponent();

            this.textBoxID.Text = id;
            this.textBoxID.Enabled = idEnable;
            _parent = parent;
        }

        private void buttonCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 确定按钮按下：根据工号，密码，去数据库验证，如果成功，就只是签名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (_parent != null)
            {
                var info = _parent.Login(this.textBoxID.Text, this.textBoxPassWord.Text);
                if (info != null)
                {
                    _ID = this.textBoxID.Text;
                    _Name = info.UserName;
                    _MEDIC_DEGREE = info.TitleName;
                    this.DialogResult = DialogResult.OK;
                }
            }
        }

        private void buttonOK_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyData == Keys.Enter)
            //{
            //    buttonOK_Click(null, null);
            //}
        }

        private void textBoxPassWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                buttonOK_Click(null, null);
            }
        }
    }
}
