using SparkFormEditor.Model;
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
    internal partial class FormEditRow : Form
    {
        private bool _IsLoading = true;
        static Point _LastLocation = Point.Empty;

        public EventHandler _CallBackEvent = null;       //修改后，更新到主界面
        public EventHandler _CallBackEnterEvent = null;  //选中单元格提示手形提示

        public FormEditRow()
        {
            //_IsLoading = true;

            InitializeComponent();

            this.DoubleBuffered = true;
            //TopMost = true;

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;

            //tableLayoutPanel1.Resize += new EventHandler(tableLayoutPanel1_Resize);
            //resize.FirstResize(tableLayoutPanel1);

            //按下Alt键  //Ctrl+
            //if ((Control.ModifierKeys & Keys.Alt) != 0)

            //按下Ctrl + Alt键
            //if ((Control.ModifierKeys & Keys.Control) == Keys.Control && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)

            //_IsLoading = false;
        }

        /// <summary>
        /// 根据参数初始化，界面（表格列分组）
        /// </summary>
        public void InitTableItems(XmlNode xnTable, string key, string groupName, string[] itemsHeaderArr, string[] itemsColumnArr, string[] itemsValueArr)
        {
            _IsLoading = true;

            this.Text = key + " - 编辑";
            this.labelRowKey.Text = key;
            this.labelGroupName.Text = groupName;

            this.tableLayoutPanel1.Dispose();
            this.tableLayoutPanel1 = new TableLayoutPanel();

            //固定两列
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.97059F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.02941F));

            this.tableLayoutPanel1.RowCount = itemsColumnArr.Length;

            Label lblNew;
            RichTextBoxExtended rtbNew;
            XmlNode xnCol = null;
            bool tempBool = false;
            for(int i = 0; i< itemsColumnArr.Length; i++)
            {
                rtbNew = new RichTextBoxExtended();
                rtbNew.BorderStyle = System.Windows.Forms.BorderStyle.None;
                rtbNew.Dock = System.Windows.Forms.DockStyle.Fill;
                rtbNew.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                rtbNew.Location = new System.Drawing.Point(199, 4);
                rtbNew.Multiline = false;
                rtbNew.Name = itemsColumnArr[i];
                rtbNew.Size = new System.Drawing.Size(300, 103);
                rtbNew.TabIndex = 1;
                rtbNew.Text = itemsValueArr[i];
                rtbNew.TextChanged += new System.EventHandler(this.richTextBoxItem_TextChanged);
                rtbNew.Enter += new System.EventHandler(this.richTextBoxItem_Enter);

                if (xnTable != null)
                {
                    xnCol = xnTable.SelectSingleNode("Column[@Name='" + rtbNew.Name + "']");
                    if (xnCol != null)
                    {
                        tempBool = false;
                        if (!bool.TryParse((xnCol as XmlElement).GetAttribute(nameof(EntXmlModel.ReadOnly)), out tempBool))
                        {
                            tempBool = false;
                        }
                        //rtbeNewTransparent.ReadOnly = tempBool;
                        rtbNew.ReadOnly = rtbNew.IsReadOnly = tempBool;

                        if (!tempBool)
                        {
                            //特殊字符
                            rtbNew.SetSelectWords((xnCol as XmlElement).GetAttribute(nameof(EntXmlModel.HelpString))); //设置特殊字符菜单:→¤√¤←¤§  会导致每次打开窗体，赋值现有内容
                            //选择项
                            rtbNew.SetSelectItems((xnCol as XmlElement).GetAttribute(nameof(EntXmlModel.MenuHelpString)));
                        }
                    }
                }

                lblNew = new Label();
                lblNew.AutoSize = true;
                lblNew.BackColor = System.Drawing.SystemColors.Control;
                lblNew.Dock = System.Windows.Forms.DockStyle.Fill;
                lblNew.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                lblNew.Location = new System.Drawing.Point(4, 1);
                lblNew.Size = new System.Drawing.Size(188, 109);
                lblNew.TabIndex = 0;
                lblNew.Text = itemsHeaderArr[i];
                lblNew.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

                this.tableLayoutPanel1.Controls.Add(lblNew, 0, i);
                this.tableLayoutPanel1.Controls.Add(rtbNew, 1, i);
                //this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100 / itemsColumnArr.Length));
                this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10));
            }

            this.panelMain.AutoScroll = true;
            this.panelMain.Controls.Add(this.tableLayoutPanel1);
            this.tableLayoutPanel1.Location = new Point(0, 0);
            //this.tableLayoutPanel1.Dock = DockStyle.None;
            //this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)
            //            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            //this.tableLayoutPanel1.Refresh();

            int newHeight = 70 + 26 * this.tableLayoutPanel1.RowCount;
            if (newHeight < Screen.PrimaryScreen.WorkingArea.Size.Height)
            {
                this.ClientSize = new Size(this.ClientSize.Width, newHeight);
                this.tableLayoutPanel1.Dock = DockStyle.Fill;
            }
            else
            {
                this.ClientSize = new Size(this.ClientSize.Width, Screen.PrimaryScreen.WorkingArea.Size.Height);

                this.tableLayoutPanel1.Size = new Size(this.ClientSize.Width, 30 * this.tableLayoutPanel1.RowCount);
            }

            _IsLoading = false;
        }



        #region 输入状态控制
        /// <summary>
        /// 指定窗体，不获取光标(窗口过大，位置靠下就看不到了，即使计算屏幕高度)
        /// </summary>
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams ret = base.CreateParams;
        //        ////ret.Style = (int)Flags.WindowStyles.WS_THICKFRAME | (int)Flags.WindowStyles.WS_CHILD;
        //        //ret.Style = (int)Flags.WindowStyles.WS_THICKFRAME; // | (int)Flags.WindowStyles.WS_CHILD
        //        //ret.ExStyle |= (int)Flags.WindowStyles.WS_EX_NOACTIVATE | (int)Flags.WindowStyles.WS_EX_TOOLWINDOW;
        //        //ret.X = this.Location.X;
        //        //ret.Y = this.Location.Y;

        //        if (_LastLocation != Point.Empty)
        //        {
        //            ret.X = _LastLocation.X;
        //            ret.Y = _LastLocation.Y;  //
        //        }

        //        return ret;
        //    }
        //}

        private void FormEditRow_Load(object sender, EventArgs e)
        {
            //实时关闭窗口
            //Application.AddMessageFilter(new PopupWindowHelperMessageFilter(this, _textboxr));

            if (_LastLocation != Point.Empty)
            {
                //ret.X = _LastLocation.X;
                //ret.Y = _LastLocation.Y;
                this.Location = _LastLocation;
            }
        }

        private Control _textboxr;

        public Control TextBox
        {
            get
            {
                return _textboxr;
            }
            set
            {
                _textboxr = value;
                //richTextBox1.Text = value.Text;
                //richTextBox1.Focus();
                //richTextBox1.Select(richTextBox1.Text.Length, 0); //光标置于最后索引位置
            }
        }
        #endregion 输入状态控制



        private void richTextBoxItem_TextChanged(object sender, EventArgs e)
        {
            RichTextBox thisRtb = (RichTextBox)sender;
            //thisRtb.Select(thisRtb.Text.Length, 0); //光标置于最后索引位置

            if (!_IsLoading)
            {
                int selectIndex = thisRtb.SelectionStart;

                if (_CallBackEvent != null)
                {
                    //选中对应单元格，赋值
                    _CallBackEvent(thisRtb, null);

                    if (!thisRtb.Focused)
                    {
                        this.Focus();
                        thisRtb.Select(selectIndex, 0);
                    }
                }
            }
        }

        private void richTextBoxItem_Enter(object sender, EventArgs e)
        {
            if (!_IsLoading)
            {
                RichTextBox thisRtb = (RichTextBox)sender;

                if (_CallBackEnterEvent != null)
                {
                    _CallBackEnterEvent(thisRtb, null);
                }

                //if (TextBox != null)
                //{
                //    //TextBox.Text = rtbe.Text.Trim();

                //    //if (_CallBackEvent != null)
                //    //{
                //    //    //选中对应单元格，赋值
                //    //    _CallBackEvent(thisRtb, null);
                //    //}

                //    //int selectIndex = thisRtb.SelectionStart;

                //    //if (!thisRtb.Focused)
                //    //{
                //    //    this.Focus();
                //    //    thisRtb.Select(selectIndex, 0);
                //    //}
                //}
            }
        }


        private void panelBottom_Click(object sender, EventArgs e)
        {
            //this.Close();
        }

        private void FormEditRow_Shown(object sender, EventArgs e)
        {
            if (TextBox != null)
            {
                Control[] ct = this.Controls.Find(TextBox.Name, true);

                if (ct != null && ct.Length > 0)
                {
                    ct[0].Focus();

                    RichTextBox thisRtb = (RichTextBox)ct[0];
                    thisRtb.Select(thisRtb.Text.Length, 0); //光标置于最后索引位置
                }
            }
        }

        private void FormEditRow_FormClosed(object sender, FormClosedEventArgs e)
        {
            _LastLocation = this.Location;
        }


    }
}
