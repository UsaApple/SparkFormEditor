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
    /// 小结总结的时间范围设定
    /// 选择小结总结的时间范围（刚入院，或者外出等时候，小结和总结不能是直到上一次小结总结，会多出几天的数据）
    ///</summary>
    internal partial class Frm_SumTime : Form
    {
        //返回调用界面的设置参数：
        private DateTime _dtFrom;
        private DateTime _dtTo;
        private string _strWord;

        public DateTime DtFrom { get { return _dtFrom; } }
        public DateTime DtTo { get { return _dtTo; } }
        public string StrWord { get { return _strWord; } }

        private string _sumGroupExtend = null;
        private string _dateFormat = null;

        public Frm_SumTime()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="sumGroupExtend"></param>
        /// <param name="dateFormat"></param>
        public Frm_SumTime(string sumGroupExtend, string dateFormat)
        {
            InitializeComponent();

            //dateTimePickerFrom.Value = dtFrom;
            //dateTimePickerTo.Value = dtTo;

            //SumGroupExtend="全天总结§{0}全天总结§-07:00§06:59¤白班小结§{0}白班小结§07:00§19:59¤夜班小结§{0}夜班小结§-19:00§06:59¤临时小结§{1}小时小结§-07:00§06:59"
            _sumGroupExtend = sumGroupExtend;
            _dateFormat = dateFormat;
        }

        private void SumTimeForm_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_sumGroupExtend))
            {
                string[] arr1 = _sumGroupExtend.Split('¤');
                string[] arr2 = null;

                //绑定结类型下拉框
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("col1");
                dataTable.Columns.Add("col2");

                for (int i = 0; i < arr1.Length; i++)
                {
                    arr2 = arr1[i].Split('§');

                    dataTable.Rows.Add(new string[] { arr2[0], arr1[i] });
                }

                comboBox1.DataSource = dataTable;            
                comboBox1.DisplayMember = "col1";
                comboBox1.ValueMember = "col2";
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
            if (dateTimePickerFrom.Value.CompareTo(dateTimePickerTo.Value) > 0)
            {
                MessageBox.Show("开始时间应该早于结束时间。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }


            //设置返回结果
            _dtFrom = dateTimePickerFrom.Value;
            _dtTo = dateTimePickerTo.Value;

            _strWord = this.richTextBox1.Text.Trim();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 结类型下拉框选择改变后，根据配置设置时间和结文字，当然还可以手动修改的。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue != null) //初始化时是datarow，点击时是string
            {
                //"全天总结§{0}全天总结§-07:00§06:59¤白班小结§{0}白班小结§07:00§19:59¤夜班小结§{0}夜班小结§-19:00§06:59¤临时小结§{1}小时小结§-07:00§06:59"
                object setting = "";

                if (comboBox1.SelectedValue is DataRowView)
                {
                    setting = ((comboBox1.SelectedValue as DataRowView).Row as DataRow).ItemArray[1];
                }
                else
                {
                    setting = comboBox1.SelectedValue;
                }

                //为空，不用处理
                if(setting == null || string.IsNullOrEmpty(setting.ToString()))
                {
                    return;
                }

                string[] arr = setting.ToString().Split('§');

                //先设置日期，然后设置结文字。
                if (arr[2] != null && !string.IsNullOrEmpty(arr[2].ToString()))
                {
                    string time = arr[2].ToString().Trim();
                    int preDay = 0;
                    if (time.StartsWith("-")) //系统日期前一天
                    {
                        preDay = -1;
                        time = time.TrimStart('-');
                    }
                    else
                    {
                        preDay = 0;
                    }

                    this.dateTimePickerFrom.Value = DateTime.Parse(string.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(preDay)) + " " + time);
                }

                if (arr[3] != null && !string.IsNullOrEmpty(arr[3].ToString()))
                {
                    string time = arr[3].ToString().Trim();
                    int preDay = 0;
                    if (time.StartsWith("-")) //系统日期前一天
                    {
                        preDay = -1;
                        time = time.TrimStart('-');
                    }
                    else
                    {
                        preDay = 0;
                    }

                    this.dateTimePickerTo.Value = DateTime.Parse(string.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(preDay)) + " " + time);
                }

                string ymdStr = string.Format("{0:" + _dateFormat + "}", this.dateTimePickerFrom.Value);

                TimeSpan ts = this.dateTimePickerTo.Value.Subtract(this.dateTimePickerFrom.Value);
                this.richTextBox1.Text = arr[1].Replace("{0}", ymdStr).Replace("{1}", ((int)ts.TotalHours + 1).ToString());
            }
        }

        private void dateTimePickerFrom_ValueChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue != null) //初始化时是datarow，点击时是string
            {
                //"全天总结§{0}全天总结§-07:00§06:59¤白班小结§{0}白班小结§07:00§19:59¤夜班小结§{0}夜班小结§-19:00§06:59¤临时小结§{1}小时小结§-07:00§06:59"
                object setting = "";

                if (comboBox1.SelectedValue is DataRowView)
                {
                    setting = ((comboBox1.SelectedValue as DataRowView).Row as DataRow).ItemArray[1];
                }
                else
                {
                    setting = comboBox1.SelectedValue;
                }

                //为空，不用处理
                if (setting == null || string.IsNullOrEmpty(setting.ToString()))
                {
                    return;
                }

                string[] arr = setting.ToString().Split('§');
                string ymdStr = string.Format("{0:" + _dateFormat + "}", this.dateTimePickerFrom.Value);

                TimeSpan ts = this.dateTimePickerTo.Value.Subtract(this.dateTimePickerFrom.Value);
                this.richTextBox1.Text = arr[1].Replace("{0}", ymdStr).Replace("{1}", ((int)ts.TotalHours + 1).ToString());
            }
        }

        private void dateTimePickerTo_ValueChanged(object sender, EventArgs e)
        {

            if (comboBox1.SelectedValue != null) //初始化时是datarow，点击时是string
            {
                //"全天总结§{0}全天总结§-07:00§06:59¤白班小结§{0}白班小结§07:00§19:59¤夜班小结§{0}夜班小结§-19:00§06:59¤临时小结§{1}小时小结§-07:00§06:59"
                object setting = "";

                if (comboBox1.SelectedValue is DataRowView)
                {
                    setting = ((comboBox1.SelectedValue as DataRowView).Row as DataRow).ItemArray[1];
                }
                else
                {
                    setting = comboBox1.SelectedValue;
                }

                //为空，不用处理
                if (setting == null || string.IsNullOrEmpty(setting.ToString()))
                {
                    return;
                }

                string[] arr = setting.ToString().Split('§');
                string ymdStr = string.Format("{0:" + _dateFormat + "}", this.dateTimePickerFrom.Value);

                TimeSpan ts = this.dateTimePickerTo.Value.Subtract(this.dateTimePickerFrom.Value);
                this.richTextBox1.Text = arr[1].Replace("{0}", ymdStr).Replace("{1}", ((int)ts.TotalHours + 1).ToString());
            }
        }


    }
}
