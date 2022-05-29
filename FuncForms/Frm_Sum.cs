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
    ///<summary>
    /// 表格汇总合计
    ///</summary>
    internal partial class Frm_Sum : Form
    {
        //返回调用界面的设置参数：
        private DateTime _dtFrom;
        private DateTime _dtTo;
        private List<string> _columnsList = new List<string>();
        private string _sumWord = "";

        public string SumWord { get { return _sumWord; } }
        public DateTime DtFrom { get { return _dtFrom; } }
        public DateTime DtTo { get { return _dtTo; } }
        public List<string> ColumnsList { get { return _columnsList; } }

        /// <summary>
        /// 0:合计、并且统计次数，1：只统计次数
        /// </summary>
        public int SumMode
        {
            get
            {
                return 0;
            }
        }

        public Frm_Sum()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="dtFrom"></param>
        /// <param name="dtTo"></param>
        /// <param name="columns"></param>
        /// <param name="defauleCol"></param>
        /// <param name="rowSumType"></param>
        public Frm_Sum(DateTime dtFrom, DateTime dtTo, List<string> columns, string defauleCol, string rowSumType, string sumSetting, string sumedWord)
        {
            InitializeComponent();

            if (rowSumType != "")
            {
                //this.Text += " 【当前行已经进行过合计】：" + rowSumType;
                //this.Text = "[已合计]:" + rowSumType;

                string[] arr = rowSumType.Split('¤');

                this.Text = "已" + arr[0];

                if (arr.Length >= 3)
                {
                    //去除秒部分
                    if (DateHelp.IsDate(arr[1]))
                    {
                        this.Text += ":" + string.Format("{0:yyyy-MM-dd HH:mm}", DateTime.Parse(arr[1]));
                    }

                    if (DateHelp.IsDate(arr[2]))
                    {
                        this.Text += "~" + string.Format("{0:yyyy-MM-dd HH:mm}", DateTime.Parse(arr[2]));
                    }
                }

                //this.label1.ForeColor = Color.DarkBlue;
                this.label1.Text += " [ √ ]";
                //dateTimePickerFrom.CalendarForeColor = dateTimePickerTo.CalendarForeColor = Color.Blue;
            }

            dateTimePickerFrom.Value = dtFrom;
            dateTimePickerTo.Value = dtTo;

            checkedListBoxColumns.Items.Clear();

            var arrs = defauleCol.Split('|');

            int defaultIndex = -1;
            List<int> listColsIndex = new List<int>();
            for (int i = 0; i < columns.Count; i++)
            {
                checkedListBoxColumns.Items.Add(columns[i]);

                if (defauleCol == columns[i])
                {
                    defaultIndex = i;
                }
                if (arrs != null && arrs.Contains(columns[i]))
                {
                    listColsIndex.Add(i);
                }
            }

            if (defauleCol != "" && defaultIndex != -1)
            {
                checkedListBoxColumns.SetItemChecked(defaultIndex, true);
            }
            else
            {
                //选择全部
                if (defauleCol == "¤")
                {
                    for (int i = 0; i < checkedListBoxColumns.Items.Count; i++)
                    {
                        checkedListBoxColumns.SetItemChecked(i, true);
                    }
                }

            }
            if (listColsIndex.Any())
            {
                foreach (var item in listColsIndex)
                {
                    checkedListBoxColumns.SetItemChecked(item, true);
                }
            }

            //小结文字
            //SumSetting="行线@BottomLine,Red¤结文字列@项目¤结文字集合@12小时小结|12小时总结|24小时总结¤过滤@mg|Mg¤合计列@出量1|出量2" 
            string sumWordList = Comm.GetPropertyByName(sumSetting, "结文字集合").Trim();
            if (!string.IsNullOrEmpty(sumWordList))
            {
                string[] arr = sumWordList.Split('|');

                //for (int i = 0; i < arr.Length; i++)
                //{
                comboBoxSumWord.Items.AddRange(arr);
                //}

                if (!string.IsNullOrEmpty(sumedWord))
                {
                    comboBoxSumWord.Text = sumedWord;//已合计过的行，上次合计填写的合计固定文字
                }
            }
            else
            {
                comboBoxSumWord.Visible = false;
            }
        }

        /// <summary>
        /// 取消按钮按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 确定按钮按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            //设置返回结果
            _dtFrom = dateTimePickerFrom.Value;
            _dtTo = dateTimePickerTo.Value;

            _sumWord = this.comboBoxSumWord.Text.Trim();

            _columnsList.Clear();
            for (int i = 0; i < checkedListBoxColumns.CheckedItems.Count; i++)
            {
                _columnsList.Add(checkedListBoxColumns.CheckedItems[i].ToString());
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
