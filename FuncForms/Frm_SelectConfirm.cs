using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SparkFormEditor
{
    internal partial class Frm_SelectConfirm : Form
    {
        private string _SaveSynchronize = null;
        public string _SelectedName = null;

        public Frm_SelectConfirm()
        {
            InitializeComponent();
        }

        public Frm_SelectConfirm(string saveSynchronize)
            : this()
        {
            _SaveSynchronize = saveSynchronize;

            //SaveSynchronize="表单名@一般护理记录单,危重护理记录单¤
            string names = Comm.GetPropertyByName(saveSynchronize, "表单名");
            string[] arr = names.Split(',');

            if (arr.Length == 2)
            {
                label1.Text = arr[0].Trim();
                label2.Text = arr[1].Trim();
            }
            else if (arr.Length == 1)
            {
                label1.Text = arr[0].Trim();
                label2.Visible = false;
                label1.Location = new Point(label1.Location.X, label1.Location.Y + 20);
            }
            else
            {
                label1.Text = "尚未配置表单名，请联系管理员";
                label1.Text = "按照正确方式配置表单名";
            }
        }

        private void label_MouseEnter(object sender, EventArgs e)
        {
            ((Label)sender).BorderStyle = BorderStyle.Fixed3D;
            ((Label)sender).ForeColor = Color.Red;
            this.Cursor = Cursors.Hand;
            ((Label)sender).Focus();
        }

        private void label_MouseLeave(object sender, EventArgs e)
        {
            ((Label)sender).BorderStyle = BorderStyle.None;
            ((Label)sender).ForeColor = Color.Black;
            this.Cursor = Cursors.Default;
        }

        private void SelectConfirmForm_MouseMove(object sender, MouseEventArgs e)
        {
            label1.BorderStyle = BorderStyle.None;
            label2.BorderStyle = BorderStyle.None;
            label取消.BorderStyle = BorderStyle.None;

            label1.ForeColor = Color.Black;
            label2.ForeColor = Color.Black;
            label取消.ForeColor = Color.Black;
        }

        private void label_MouseMove(object sender, MouseEventArgs e)
        {
            ((Label)sender).BorderStyle = BorderStyle.Fixed3D;
            this.Cursor = Cursors.Hand;
            ((Label)sender).Focus();
        }

        private void label取消_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
        }

        private void label_Click(object sender, EventArgs e)
        {
            _SelectedName = ((Label)sender).Text;
            this.DialogResult = DialogResult.OK;
        }
    }
}
