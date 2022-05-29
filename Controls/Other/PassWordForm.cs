using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace SparkFormEditor
{
    internal partial class PassWordForm : Form
    {
        public PassWordForm()
        {
            InitializeComponent();
        }

        public static string oldPassWord = "";

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.ToLower() == "trt")
            {
                oldPassWord = textBox1.Text.ToLower();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
