using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SparkFormEditor
{
    internal partial class Frm_SignLoginEx : Frm_SignLogin
    {
        public Frm_SignLoginEx() : base()
        {
            InitializeComponent();
        }

        public Frm_SignLoginEx(string id, SparkFormEditor parent) : base(id, parent)
        {
            InitializeComponent();
        }

        /// <summary>
        /// 构造方法：工号不可以修改
        /// </summary>
        /// <param name="id"></param>
        /// <param name="idEnable"></param>
        public Frm_SignLoginEx(string id, bool idEnable, SparkFormEditor parent) : base(id, idEnable, parent)
        {
            InitializeComponent();
        }

        public Frm_SignLoginEx(string id, bool idEnable, SparkFormEditor parent, string tipInfo) : base(id, idEnable, parent)
        {
            InitializeComponent();
            labTip.Text = tipInfo;
        }

    }
}
