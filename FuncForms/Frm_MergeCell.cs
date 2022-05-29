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
    /// 合并单元格的参数设置界面
    ///</summary>
    internal partial class Frm_MergeCell : Form
    {
        private bool _type = false;
        private int _count = 0;

        int _maxRow = 1;
        int _maxCol = 1;

        public Frm_MergeCell()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 实例化构造方法
        /// </summary>
        /// <param name="maxRow">能合并的最大行数</param>
        /// <param name="maxCol">能合并的最大列数</param>
        /// <param name="currentCellX">当前行</param>
        /// <param name="currentCellY">当前列</param>
        public Frm_MergeCell(int maxRow, int maxCol, int currentCellX, int currentCellY)
        {
            InitializeComponent();

            _maxRow = maxRow;
            _maxCol = maxCol;

            numericUpDownRight.Maximum = maxCol - currentCellY - 1;
            numericUpDownDown.Maximum = maxRow - currentCellX - 1;

            //"表格共：" + maxRow.ToString() + "行， " + maxCol.ToString() + "列， 选中了：第" + (currentCellX + 1).ToString() + "行，第" + (currentCellY + 1).ToString() + "列。"
            //                    + "\r\n可设置的最大合并数为 向下：" + (maxRow - currentCellX - 1).ToString() + "，向右：" + (maxCol - currentCellY - 1).ToString();

        }

        /// <summary>
        /// 选择是横向/纵向 切换后界面上的显示对应变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButtonRight_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonRight.Checked)
            {
                numericUpDownRight.Enabled = true;
                numericUpDownDown.Enabled = false;
            }
            else
            {
                numericUpDownRight.Enabled = false;
                numericUpDownDown.Enabled = true;
            }
        }

        /// <summary>
        /// 选择是横向/纵向 切换后界面上的显示对应变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButtonDown_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonDown.Checked)
            {

                numericUpDownRight.Enabled = false;
                numericUpDownDown.Enabled = true;
            }
            else
            {
                numericUpDownRight.Enabled = true;
                numericUpDownDown.Enabled = false;
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (radioButtonRight.Checked)
            {
                _type = true;
                _count = (int)numericUpDownRight.Value;
            }
            else
            {
                _type = false;
                _count = (int)numericUpDownDown.Value;
            }

            this.DialogResult = DialogResult.OK; 
            this.Close();
        }

        public bool MergeType { get { return _type; } }
        public int MergeCount { get { return _count; } }

        private void numericUpDownDown_Click(object sender, EventArgs e)
        {
            //radioButtonDown.Checked = true;
        }
    }
}
