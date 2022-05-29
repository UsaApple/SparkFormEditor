/* 新表单
 * *************************************************************
 TRT 2013年6月28日 -2013年8月8日
 * 
 * 
 * 打印DGV，自定义选择界面（需要打印的列和一些指定的打印方式）
 * 
 *  
 * *************************************************************
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NurseForm.Editor.Ctrl
{
    internal partial class PrintOptions : Form
    {
        #region Constructors
        public PrintOptions()
        {
            InitializeComponent();
        }
        
        public PrintOptions(List<string> availableFields)
        {
            InitializeComponent();

            foreach (string column in availableFields)
            {
                ctlColumnsToPrintCHKLBX.Items.Add(column, true);
            }

            radioButton1.Checked = false;
        }
        #endregion //Constructors

        #region Event Handlers
        private void OnLoadForm(object sender, EventArgs e)
        {
            // Initialize some controls
            ctlPrintAllRowsRBTN.Checked = true;
            ctlPrintToFitPageWidthCHK.Checked = true;
            checkBox1Page2.Checked = false;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //如果设置了一页打印原来两页的内容，那么将dgv进行合并
            //if (this.checkBox1Page2.Checked)
            //{
            //    //一页打印原来的两页，进行合并


            //}

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        #endregion //Event Handlers

        public bool Get1Page2()
        {
            return checkBox1Page2.Checked;
        }

        public List<string> GetSelectedColumns()
        {
            List<string> list = new List<string>();
            foreach (object item in ctlColumnsToPrintCHKLBX.CheckedItems)
            {
                list.Add(item.ToString());
            }
            return list;
        }

        #region Properties
        public string PrintTitle
        {
            get { return ctlPrintTitleTBX.Text; }
            set { ctlPrintTitleTBX.Text = value; }
        }

        public bool PrintAllRows
        {
            get { return ctlPrintAllRowsRBTN.Checked; }
        }

        public bool FitToPageWidth
        {
            get { return ctlPrintToFitPageWidthCHK.Checked; }
        }
        #endregion //Properties

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton2.Checked = false;

            CheckBox[] check = new CheckBox[this.ctlColumnsToPrintCHKLBX.Items.Count];
            for (int i = 0; i < this.ctlColumnsToPrintCHKLBX.Items.Count; i++)
            {

                this.ctlColumnsToPrintCHKLBX.SetItemChecked(i, true);

            }
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            radioButton2.Checked = true;
            radioButton1.Checked = false;

            CheckBox[] check = new CheckBox[this.ctlColumnsToPrintCHKLBX.Items.Count];
            for (int i = 0; i < this.ctlColumnsToPrintCHKLBX.Items.Count; i++)
            {

                this.ctlColumnsToPrintCHKLBX.SetItemChecked(i, false);

            }
        }

        //private void btnHelp_Click(object sender, EventArgs e)
        //{

        //}

    }
}