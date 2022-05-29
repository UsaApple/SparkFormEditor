using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml;
using System.Collections;
using SparkFormEditor.Model;
using System.Linq;

//// 非数字键, 放弃该输入
// if (!char.IsDigit(e.KeyChar))
// {
//     e.Handled = true;
//     return;
// }

//RichTextBox 中英文混输时，字体样式不同的解决方式：
//RichTextBox默认情况下会根据用户输入的字符来自动设置字体样式，要想让输入保持制定的格式需要设置它的LanguageOption属性。
//设置代码为：this.RichTextBox1.LanguageOption = RichTextBoxLanguageOptions.UIFonts;

namespace SparkFormEditor
{
    /// <summary>
    /// 自定义输入框
    /// 打印和显示的时候用，无背景色，不会挡住上下线；但是字体会变粗
    /// </summary>
    [ToolboxItem(false)]
    public partial class RichTextBoxExtended : RichTextBox
    {

        //当前输入框，是否经过文字特殊编辑，关系到保存Text还是rtf
        public bool _IsRtf = false; // false为text true为Rtf
        public bool _SeniorRtfMode = true; //输入框可以对应上标和下标的高级Rtf格式
        public string _Default = ""; //默认值
        public string _Score = ""; //评分表达式

        //单横线
        public bool _HaveSingleLine = false;

        //双横线
        public bool _HaveDoubleLine = false;

        public bool _IsTable = false; //标识是否输入表格的输入框

        public Size _DefaultSize
        {
            get;
            set;
        }//编辑大小
        public Size _DefaultCellSize;//原始大小

        public bool _AdjustParagraphs = false;

        public string Sum = ""; //非表格输入框的合计公式

        private bool Is_SelectItem = false; //用输入框来实现选择框

        public int MenuSelectItemShowMode = 0; //默认为0，如果是0，每次光标进入单元格会显示独立的选择项菜单，右击显示完整菜单。 如果为1：光标进入不显示，右击显示独立的选择项菜单，按下Ctrl显示完整菜单。2：光标进入也不显示，但是右击的时候是蒋选择项菜单提到第一层。

        //public Font DefaultFont; //输入框默认字体，用来判断是否是富文本，还是纯文本；遍历每个字来判断，如果_IsText_Rtf已经为True那么就不需要判断了。
        public HorizontalAlignment _InputBoxAlignment;
        public Color DefaultForeColor;

        public string CreateUser = "";  //创建用户

        public string OldRtf = ""; //是否需要保存用
        public string OldText = ""; //值改变用 ,目前用于行文字根据时间自动变色。但是自动赋默认值的时候，之后要重置OldText为空，否则不会触发。

        public bool Is_RowSpanMutiline = false; //如果是纵向合并的单元格，会变成总是居左

        public bool IsReadOnly = false; // 不可编辑的输入框，如表格中的签名列

        public MouseButtons _MouseButton = MouseButtons.Left;

        //修订，留痕修改
        public string _EditHistory = "";

        public string _AttachForm = "";

        # region //日期输入框/单元格，string.Format("{0:yyyy-MM-dd}", DateTime.Now);
        public string _YMD = ""; //默认为：完整年月日yyyy-MM-dd，月日M-dd，日d .TrimStart('0')
        public string _YMD_Default = "";
        public string _YMD_Old = "";
        public string _FullDateText = "";
        public MonthCalendar _Calendar = null;  //如果是日期输入框的话，可以用日历来选择日期

        public bool _IsSetValue = false;    //是不是赋值的时候

        public bool _IsPrintAll = false;    //防止打印所有页太慢，跳过不需要的处理

        public int _MaxLength = 99999;      //调正段落列的时候，需要解除最大文字数，否则如果拷贝几行的内容，就被截断了

        public bool _CanAutoPositionByTime = false;                 //是否可以自动根据时间调整行位置（选择行自动排序）
        public event EventHandler AutoPositionByTime_MainForm;      //调用主体:调整方法

        public event EventHandler ShowInforMsg_MainForm;            //调用主体:显示提示消息
        public event EventHandler ShowParagraphContent_MainForm;    //调用主体:显示光标所在行的所有内容

        public bool _EnableSUM_MainForm = false;
        public event EventHandler SUM_MainForm;                     //调用主体:小结/总结

        public event EventHandler CopyCellsEvent;                   //复制单元格

        public string _CheckRegex = "";   //CheckRegex="^([6-9][0-9]|1[0-2][0-9])/(1[2-9][0-9]|2[0-9][0-9]|300)$"不能为空，^([6-9][0-9]|1[0-1][0-9])/(1[2-9][0-9]|2[0-9][0-9]|300)|^$ 可以为空

        public string GroupName = "";  //列的分组：点击某一列，显示组内项目，可以编辑

        //单元格多行
        public bool MultilineCell = false; //单元格是默认单行，多行的话会带来很多无法控制的问题。但是如果不是调整段罗列等列，医院确实需要的话，可以开启
        public bool OwnerTableHaveMultilineCell = false;  //（合并单元格的时候，配置为多行的时候）只要表格中含有一个是多行的单元格，那么其他不回行的，也要纵向居中的。

        public string _HelpString = "";  //右击菜单中的特殊文字，保存下来。以便查看段落时也能加载菜单

        /// <summary>
        /// 获取对应格式的日期字符串
        /// </summary>
        /// <returns></returns>
        public string GetShowDate()
        {
            string ret = "";
            string dateFormat = "";

            if (_YMD == "")
            {
                //如果为空用默认值
                dateFormat = "yyyy-MM-dd";
            }
            else
            {
                //用配置文件设置的
                dateFormat = _YMD;
            }

            try
            {
                //根据日期内容进行判断
                if (_FullDateText.Trim() == "")
                {
                    ret = "";
                }
                else
                {
                    //-12 1200也认为是正确的，这样光标离开后，显示的错误日期不是原来的了 返回9-30了
                    if (DateHelp.IsDate(_FullDateText))
                    {
                        //ret = string.Format("{0:" + dateFormat + "}", DateTime.Parse(_FullDateText + " 12:00")).TrimStart('0'); 

                        if (dateFormat == "d")
                        {

                            if (IsDateYMDChinese(_YMD_Default))
                            {
                                ret = DateTime.Parse(_FullDateText + " 12:00").Day.ToString() + "日";
                            }
                            else
                            {
                                ret = DateTime.Parse(_FullDateText + " 12:00").Day.ToString();
                            }
                        }
                        else
                        {
                            ret = string.Format("{0:" + dateFormat + "}", DateTime.Parse(_FullDateText + " 12:00"));  //不能去掉前面的0
                        }
                    }
                    else
                    {
                        ret = _FullDateText;
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                //如果给规范日期，那么返回原来的输入框内容，并且记录到日志中         
                Comm.LogHelp.WriteErr("日期输入框，获取完整日期时出现错误：" + this.Name + "@" + this.Text);
                Comm.LogHelp.WriteErr(ex);
                return _FullDateText;
            }
        }
        # endregion //日期输入框/单元格


        private string _SpecialWords = "";      //特殊字符
        private string _SelectItems = "";       //选择项
        private XmlNode _XnKnowledg = null;     //知识库

        public RichTextBoxExtended()
        {
            //区域生成图表时，次数过多，会报错;创建窗口句柄时出错。
            if (!Comm._isCreatReportImages)
            {
                InitializeComponent();

                this.DoubleBuffered = true;
                base.SetStyle(ControlStyles.DoubleBuffer, true);

                //// 上下标隐藏
                //toolStripSeparator1.Visible = false;
                //toolStripMenuItemUp.Visible = false;
                //toolStripMenuItemDown.Visible = false;

                //this.toolStripMenuItemSelectItem.Visible = false;
                //toolStripMenuItemSelectItem.Tag = "false";
            }
            else
            {
                this.components = new System.ComponentModel.Container();  //如果为nul会导致不能释放

                //导致区域生成图片的时候，不触发设值
                this.TextChanged += new System.EventHandler(this.RichTextBoxExtended_TextChanged);
            }



            this.LanguageOption = RichTextBoxLanguageOptions.AutoFont;

            ////this.ImeMode = ImeMode.Inherit;
            //this.ImeMode = ImeMode.HangulFull;//在vs中使用非ms中文输入法会自动切换全角,每次输入都要切换,很烦人
            this.ImeMode = ImeMode.OnHalf;
            //this.ImeMode = ImeMode.Hangul;

            this.AutoWordSelection = true;

            //this.MouseWheel += new MouseEventHandler(richTextBox_MouseWheel);  //滚轮事件
        }

        /// <summary>
        /// 隐藏配置的，不要的一级菜单项
        /// InputBoxHideItems="复制,重做"
        /// </summary>
        public void HideMenuItems(ArrayList inputBoxHideItemsList)
        {
            if (inputBoxHideItemsList != null && inputBoxHideItemsList.Count > 0)
            {
                foreach (ToolStripItem tsmi in this.linecmenu.Items)
                {
                    //ToolStripSeparator 的.Text为空
                    if (inputBoxHideItemsList.Contains(tsmi.Text))
                    {
                        tsmi.Visible = false;
                    }
                }
            }
        }

        ///// <summary>
        ///// 滚轮事件
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void richTextBox_MouseWheel(object sender, MouseEventArgs e) 
        //{
        //    MessageBox.Show("richTextBox_MouseWheel");
        //    //showLineIndex(); 
        //}

        /// <summary>
        /// 保存当前的Rtf内容，便于后期判断
        /// </summary>
        public void SetOldRtf()
        {

            OldRtf = this.Rtf;

            //if (this.Text == "")
            //{
            //    OldRtf = "";
            //}
        }

        /// <summary>
        /// 重置大小
        /// </summary>
        public void ResetSize()
        {
            this.Size = _DefaultSize;
            this._IsRtf = false;
        }

        //override protected CreateParams CreateParams { get { CreateParams cp = base.CreateParams; cp.ExStyle |= 0x20; return cp; } }    
        //override protected void OnPaintBackground(PaintEventArgs e) 
        //{
        //    //自己的代码
        //    //双下划线绘制
        //    Graphics g = this.CreateGraphics();
        //    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        //    g.DrawLine(new Pen(Color.Red, 2), new Point(5, 10), new Point(this.Width - 10, 10));
        //    base.OnPaintBackground(e);//最后执行父类的绘制时间
        //}

        //protected override void OnPaint(PaintEventArgs e) 
        //{
        //    //双下划线绘制
        //    Graphics g = this.CreateGraphics();
        //    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        //    g.DrawLine(new Pen(Color.Red, 2), new Point(5, 10), new Point(this.Width - 10, 10));
        //}

        /// <summary>
        /// 默认值读取、设置
        /// </summary>
        public string Default
        {
            get { return _Default; }
            set
            {
                _Default = value;
                this.Text = value; //直接复制，会将居中等变成居右。现在外部通过_Default来设定
                //SetAlignment()
            }
        }

        /// <summary>
        /// 默认值是否自动加载，还是点击加载
        /// </summary>
        //public bool AutoLoadDefault { get; set; }

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

        public void ResetDefault()
        {
            _HaveSingleLine = false;
            _HaveDoubleLine = false;

            //this.Text = _Default; //显示默认值会干扰的
            this.Text = "";

            if (this.Name.StartsWith("日期"))
            {
                this._YMD = this._YMD_Default;
            }

            this.CreateUser = "";

            SetOldRtf();

            OldText = "";

            _IsRtf = false;  //不然，如果上页是富文本，新建一页这个输入框还是富文本了。

            _EditHistory = "";

            this.SetAlignment();
        }

        public void SetAlignment()
        {
            this.SelectAll();

            this.SelectionFont = this.Font;

            //对其方式设定
            if (_InputBoxAlignment == HorizontalAlignment.Center)
            {
                this.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            }
            else if (_InputBoxAlignment == HorizontalAlignment.Left)
            {
                this.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            }
            else if (_InputBoxAlignment == HorizontalAlignment.Right)
            {
                this.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Right;
            }
        }

        //区域生成图片使用
        public void SetAlignmentForReport()
        {
            //this.SelectAll();

            //this.SelectionFont = this.Font;

            //对其方式设定
            if (_InputBoxAlignment == HorizontalAlignment.Center)
            {
                this.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            }
            else if (_InputBoxAlignment == HorizontalAlignment.Left)
            {
                this.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            }
            else if (_InputBoxAlignment == HorizontalAlignment.Right)
            {
                this.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Right;
            }
        }

        #region ////-----------------------------------显示提示----------------
        //public ToolTip tt = new ToolTip();
        //private void rtb_MouseEnter(object sender, EventArgs e)
        //{
        //    showToolTip();
        //}

        //public void showToolTip()
        //{
        //    if (this._EditHistory != "")
        //    {
        //        //鼠标放上去，显示内容提示
        //        tt.ShowAlways = true;
        //        tt.Show(this._EditHistory.Replace("§", "\r\n"), this);
        //    }
        //}

        //private void rtb_MouseLeave(object sender, EventArgs e)
        //{
        //    tt.Hide(this);
        //}

        //public void hideToolTip()
        //{
        //    tt.Hide(this);
        //}
        #endregion ////-----------------------------------显示提示----------------

        //行，单元格等操作，可以放入右击菜单来处理，但是菜单数量已经很多了。
        //rtbeNew.TextChanged += new System.EventHandler(Script_TextChanged_ME);
        //public event EventHandler Script_TextChanged; //调用主体设置绑定的事件来处理
        //private void Script_TextChanged_ME(object sender, EventArgs e)
        //{
        //    Script_TextChanged(sender,e);
        //}


        #region 重绘

        //        //RichTextBox屏蔽的重绘方法，那么利用api接口来重绘; 
        //        //导致某些输入法有问题 -- 不能选择字
        //        private const int WM_PAINT = 0xF; //0x000F;
        //        //const int WM_KEYDOWN = &H102;
        //        const int WM_CHAR = 0x100; //258
        //        protected override void WndProc(ref Message m)
        //        {
        //            //int WM_CHAR = 0x0102;
        //            //if (m.Msg == WM_CHAR)
        //            //{
        //            //    base.WndProc(ref m);
        //            //    return;
        //            //}
        //            //else
        //            //{
        //            //    //base.WndProc(ref m);
        //            //}

        //            //WM_KEYDOWN WM_IME_CHAR 
        //            /*
        // * WM_KEYUP/DOWN/CHAR HIWORD(lParam) flags
        // */
        ////#define KF_EXTENDED       0x0100
        ////#define KF_DLGMODE        0x0800
        ////#define KF_MENUMODE       0x1000
        ////#define KF_ALTDOWN        0x2000
        ////#define KF_REPEAT         0x4000
        ////#define KF_UP             0x8000

        //            //if (m.Msg != WM_PAINT)
        //            //{
        //            //    base.WndProc(ref m);
        //            //    return;
        //            //}

        //            switch (m.Msg)
        //            {
        //                //case WM_CHAR:  //256

        //                //    //base.WndProc(ref m);
        //                //    return;

        //                case WM_PAINT:    //这里是可以实时刷新，但是会将输入法的提示选字显示不出来

        //                    //m = {msg=0xf (WM_PAINT) hwnd=0x400ac0 wparam=0x0 lparam=0x0 result=0x0}
        //                    base.WndProc(ref m);

        //                    ////双下划线绘制 RichTextBoxExtended_Leave中处理
        //                    //if (this.Text.Trim() != "" && (_HaveSingleLine || _HaveDoubleLine))
        //                    //{
        //                    //    Graphics g = this.CreateGraphics();
        //                    //    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        //                    //    PrintCustomLines(g, true, new Point(0, 0), Color.Red);
        //                    //}

        //                    return;
        //            }

        //            base.WndProc(ref m);

        //        }

        /// <summary>
        /// 绘制文字的下双红线
        /// </summary>
        public void PrintCusLines()
        {
            //双下划线绘制 RichTextBoxExtended_Leave中处理
            if (this.Text.Trim() != "" && (_HaveSingleLine || _HaveDoubleLine))
            {
                //如果选中文字的话，离开也不会刷新的
                if (this.SelectionLength > 0)
                {
                    this.Select(0, 0);
                    this.Refresh();

                    if (this.Parent != null && !this._IsTable)
                    {
                        this.Parent.Refresh(); //否则选择文字情况下，就会空白，需要刷新才能显示
                        //this.Parent.Invalidate(new Rectangle(this.Location, this.Size), true);
                    }
                }

                Graphics g = this.CreateGraphics();
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                PrintCustomLines(g, true, new Point(0, 0), Color.Red);
            }
        }

        /// <summary>
        /// 绘制自定义的线：单横线、双横线
        /// </summary>
        /// <param name="g"></param>
        /// <param name="IsSelfOnPaint"></param>
        public void PrintCustomLines(Graphics g, bool IsSelfOnPaint, Point orgLocation, Color colorPara) //IsSelfOnPaint为false的时候才用到orgLocation（行中的输入框位置，并不是rtbe的位置）
        {
            if (this.Text.Trim() == "")
            {
                //return;
                if (_HaveSingleLine || _HaveDoubleLine)
                {
                    colorPara = Color.White;
                }
                else
                {
                    return;
                }
            }

            if (colorPara == null)
            {
                colorPara = Color.Red;
            }

            PointF pStart = new PointF(0, 0);
            PointF pEnd = new PointF(0, 0);
            int txtToTop = this.Margin.Top; // 文字与上边框的距离
            int txtToLeft = this.Margin.Left; // 文字与左边框的距离

            //g.MeasureString界面展示，和打印对应的参数不一样，所以同样的文字，宽度也不一样

            //SizeF strSize = g.MeasureString(this.Text, this.Font); //文字大小，如果含有字体不一样的多个文字，那么可以用遍历来判断

            Font messureFont = new Font(this.Font.FontFamily.Name, this.Font.Size - Comm.GetPrintFontGay(this.Font.Size));
            SizeF strSize = TextRenderer.MeasureText(this.Text, messureFont);

            if (colorPara == Color.White)
            {
                strSize = g.MeasureString("双红线", this.Font);     //比TextRenderer的宽度和高度都大；汉字和数字字母不一样，得到的高度不一样
            }
            else
            {
                strSize = new SizeF(strSize.Width, g.MeasureString("双红线", this.Font).Height);

                ////字间距的影响
                //strSize = new SizeF(strSize.Width - this.Text.Length , strSize.Height);
            }

            strSize = new SizeF(strSize.Width - this.Margin.Left, strSize.Height - this.Margin.Top);
            float lineHeight = txtToTop + strSize.ToSize().Height;

            if (this.SelectionAlignment == HorizontalAlignment.Left)
            {
                pStart = new PointF(-2, lineHeight);
                pEnd = new PointF(strSize.Width - txtToLeft, lineHeight);
            }
            else if (this.SelectionAlignment == HorizontalAlignment.Right)
            {
                pStart = new PointF(this.Width - strSize.Width - 2, lineHeight);
                pEnd = new PointF(this.Width, lineHeight);
            }
            else
            {
                pStart = new PointF(this.Width / 2 - txtToLeft / 2 - strSize.Width / 2, lineHeight);
                pEnd = new PointF(this.Width / 2 - txtToLeft / 2 + strSize.Width / 2, lineHeight);
            }

            //与相对位置进行计算，累加
            if (IsSelfOnPaint)
            {
                //控件自绘
                //相对位置为0，0
            }
            else
            {
                //打印，生成图片，要相对的控件位置：
                PointF relativePoinf = new PointF(0, 0);
                relativePoinf = new PointF(orgLocation.X - txtToLeft, orgLocation.Y - txtToTop);

                pStart = new PointF(pStart.X + orgLocation.X, pStart.Y + orgLocation.Y);
                pEnd = new PointF(pEnd.X + orgLocation.X, pEnd.Y + orgLocation.Y);
            }

            if (_HaveSingleLine)
            {
                g.DrawLine(new Pen(colorPara, 1), pStart, pEnd);
            }

            if (_HaveDoubleLine)
            {
                g.DrawLine(new Pen(colorPara, 1), pStart, pEnd);
                g.DrawLine(new Pen(colorPara, 1), new PointF(pStart.X, pStart.Y + 2), new PointF(pEnd.X, pStart.Y + 2));
                //g.DrawLine(new Pen(colorPara, 1), new PointF(pStart.X, pStart.Y + 4), new PointF(pEnd.X, pStart.Y + 4));
            }

        }



        #endregion 


        /// <summary>
        /// 文字内容发生改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RichTextBoxExtended_TextChanged(object sender, EventArgs e)
        {
            if (!_IsChangeAndLeaveEnable)//和上下标会干扰
            {
                return;
            }

            //表格单元格，多行的支持，并且纵向居中
            if (OwnerTableHaveMultilineCell)  //MultilineCell || 
            {
                //int linesCount = 1;

                ////0行的时候，当做一行的来处理
                //if (this.Lines.Length != 0)
                //{
                //    linesCount = this.Lines.Length;
                //}

                //int top = (this.Height - 6) / linesCount / 2 - (int)Math.Ceiling(this.Font.Size);
                SetVerticalCenter(this);
            }
            //else
            //{
            //    //如果表格中含有多行的，这里也要处理的
            //    if (OwnerTableHaveMultilineCell)
            //    {
            //        SetVerticalCenter(this);
            //    }
            //}

            //日期：
            if (this.Name.StartsWith("日期"))
            {
                //如果是完整的年月日，就修改_FullDateText
                try
                {
                    //DateTime.Parse("04月03日" + " 12:00") 正常
                    //DateTime.Parse("04-03日" + " 12:00") 异常

                    //this.Text = StrHalfFull.ToDBC_Half(this.Text); //修改后，光标会显示到最前面，混乱。

                    DateTime etme = DateTime.Parse(this.Text.Trim() + " 12:00");

                    if (!_IsSetValue)  //_IsSetValue 赋值
                    {
                        _FullDateText = this.Text;
                    }

                }
                catch
                {
                    if (!_IsSetValue)  //_IsSetValue 赋值
                    {
                        _FullDateText = this.Text; //赋值和验证，会冲突；空会变成默认值
                    }
                }

                //如果值空为，那么dateFormat格式为默认的配置格式
                if (string.IsNullOrEmpty(this.Text))
                {
                    this._YMD = this._YMD_Default;
                }
            }

            if (_IsPrintAll || this.Name == "") //防止打印所有页太慢，跳过不需要的处理
            {
                return;
            }

        }

        /// <summary>
        /// 自动换行，控件自带的自动换行，会导致打印和显示不一致。
        /// 避免多行的输入框，打印的时候换行和界面显示不一致，打印就所见不即所得了。
        /// 1、在RichTextBox中，换行符只以"\n"表示。
        /// 2、如果给RichTextBox赋值，"\r\n"、"\n"、"\r"都将被转换成"\n"，由于"\n\r"不是"\r\n"组合，所以被当成2个"\n"对待。 
        /// 3、如果粘贴多行内容，那么是从粘贴后的最后一行开始调整的。
        /// </summary>
        private string _PreStr = "";
        public void WordWrapAutoLine_TextChanged()
        {
            try
            {
                Font messureFont = new Font(this.Font.FontFamily.Name, this.Font.Size - Comm.GetPrintFontGay(this.Font.Size));

                int gap = 8; // (int)this.Font.Size

                //为了性能更好，要先判断光标当前行的文字宽度进行判断
                int rowIndex = this.GetLineFromCharIndex(this.SelectionStart);//获得光标所在行的索引值
                string strCurrentLine = "";

                if (rowIndex < this.Lines.Length)
                {
                    strCurrentLine = this.Lines[rowIndex]; // 获取当前行的文字

                    #region //避免修改后，当前行往后全部变成一行。还是存在缺陷，但是至少普通情况下的可以不合并了
                    if (string.IsNullOrEmpty(strCurrentLine) ||    //末尾回车的新行
                        (TextRenderer.MeasureText(strCurrentLine, messureFont).Width + gap < this.Width &&
                        TextRenderer.MeasureText(GetPreTextChangeLineValue(_PreStr, rowIndex), messureFont).Width + gap < this.Width))
                    {
                        _PreStr = this.Text;
                        return;
                    }
                    #endregion //避免修改后，当前行往后全部变成一行。还是存在缺陷，但是至少普通情况下的可以不合并了

                    //将后面的文字的回车符号全部去掉，然后重新处理
                    //从本行开始，以及后面所有的文字重新排列行

                    int index = this.SelectionStart;

                    string oldStr = this.Text;
                    string str = "";
                    string preLineStr = "";

                    for (int i = 0; i < this.Lines.Length; i++)
                    {
                        strCurrentLine = this.Lines[i]; // 获取当前行的文字

                        if (i < rowIndex)
                        {
                            preLineStr += strCurrentLine + "\n"; // rowReline "\n"; //之前的回行，强制加上回行符号，不再改变。是\n还是\r\n需要判断取原来的
                        }
                        else
                        {
                            //strCurrentLine = this.Lines[i]; // 获取当前行的文字
                            strCurrentLine = strCurrentLine.Replace("\r\n", ""); //\r\n是程序换行，\n是手动换行，手动换行不去掉。和word一致。
                            strCurrentLine = strCurrentLine.Replace("\n", "");
                            str += strCurrentLine;
                        }
                    }

                    string NewStr = str;

                    //循环该字符串：如果不包含回车符号的连续文字达到换行长度，就自动换行
                    int p1 = 0, p2 = 0;
                    for (int i = 0; i < str.Length; i++)
                    {
                        p2++;

                        str = NewStr;  //if (p1 + p2 > NewStr.Length) //一行一个字的时候就能看出来错误了

                        if (p1 + p2 > str.Length)
                        {
                            break;
                        }

                        if (!str.Substring(p1, p2).Contains("\n") &&
                            (p1 + p2 + 1 > str.Length || (p1 + p2 + 1 <= str.Length && str.Substring(p1 + p2, 1) != "\n"))) //本行不包含换行符号，并且紧跟后面不是换行符
                        {
                            if (TextRenderer.MeasureText(str.Substring(p1, p2), messureFont).Width + gap >= (this.Width))
                            {
                                string bbbb = str.Substring(p1, p2);

                                int count = 0;
                                if (i < str.Length - 1)
                                {
                                    //全角转换成半角
                                    string nextRowFirst = StrHalfFull.ToDBC_Half(str.Substring(i + 1, 1)); //、,。;“” //一二三123456四五，六七八九ABC

                                    if (nextRowFirst == "、" || nextRowFirst == "," || nextRowFirst == ";" || nextRowFirst == "。")
                                    {
                                        count = -1;
                                    }
                                }

                                p2 -= count;

                                //程序换行
                                NewStr = NewStr.Insert(p1 + p2, "\n"); //NewStr = NewStr.Insert(p1 + p2, "\n")  //即使这里拼接\r\n，复制给Text后，又变成了\n。

                                p1 += p2 + 1;

                                p2 = 0; //重置为零，循环里面用来记录：本行的文字个数
                                i -= count;
                            }
                        }
                        else
                        {
                            p1 += p2 + 1;
                            p2 = 0;
                        }
                    }

                    //如果末位是回行符号，要去掉
                    if (preLineStr.EndsWith("\r\n"))
                    {
                        preLineStr = preLineStr.Substring(0, preLineStr.Length - 2);
                    }

                    if (preLineStr.EndsWith("\n"))
                    {
                        preLineStr = preLineStr.Substring(0, preLineStr.Length - 1);
                    }

                    //如果末位是回行符号，要去掉
                    if (NewStr.StartsWith("\r\n") || NewStr.StartsWith("\n"))
                    {

                    }
                    else
                    {
                        if (preLineStr != "" && (!preLineStr.EndsWith("\r\n") || preLineStr.EndsWith("\n")))
                        {
                            NewStr = "\n" + NewStr;
                        }
                    }

                    this.Invalidate(); //防止界面刷新
                    this.Text = preLineStr + NewStr;

                    if (_PreStr == "")
                    {
                        _PreStr = oldStr;
                    }

                    //使得：回行后，光标所在位置，就在当前输入/粘帖的文字的后面：
                    if (oldStr.Length < this.Text.Length)
                    {
                        //在行头删除，需要移动上一行;但是增加输入文字，导致换行的时候就会有问题
                        if (oldStr.Replace("\n", "").Length == this.Text.Replace("\n", "").Length)
                        {
                            if (oldStr.Length < this.Text.Length)
                            {
                                //输入文字
                                this.Select(index, 0);

                                int rowIndexNew = this.GetLineFromCharIndex(this.SelectionStart);

                                if (rowIndexNew != rowIndex)
                                {
                                    this.Select(index + this.Text.Length - oldStr.Length, 0);
                                }
                            }
                            else
                            {
                                //删除文字的情况下
                                this.Select(index, 0);
                            }
                        }
                        else
                        {
                            this.Select(index + this.Text.Length - oldStr.Length, 0);
                        }
                    }
                    else
                    {
                        if (_PreStr.Replace("\n", "") != this.Text.Replace("\n", "") && this.Text.Length > _PreStr.Length)
                        {
                            this.Select(index, 0);

                            int rowIndexNew = this.GetLineFromCharIndex(this.SelectionStart);

                            if (rowIndexNew != rowIndex && this.Text.Substring(index, 1) != "\n")
                            {
                                this.Select(index + rowIndexNew - rowIndex, 0);
                            }
                        }
                        else
                        {
                            this.Select(index, 0);
                        }
                    }

                    _PreStr = this.Text;
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                //throw ex;
            }
        }

        /// <summary>
        /// 获取修改内容前，指定行原来的文本内容
        /// </summary>
        /// <param name="allText"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetPreTextChangeLineValue(string allText, int index)
        {
            string ret = "";
            string[] arr = allText.Split(new string[] { "\n" }, StringSplitOptions.None);

            if (index < arr.Length)
            {
                ret = arr[index];
            }

            return ret;
        }

        #region 行间距

        /// <summary>
        /// 设置行距
        /// </summary>
        /// <param name="ctl">控件</param>
        /// <param name="dyLineSpacing">间距</param>
        public static void SetLineSpace(Control ctl, int dyLineSpacing)
        {
            Struct.PARAFORMAT2 fmt = new Struct.PARAFORMAT2();
            fmt.cbSize = Marshal.SizeOf(fmt);
            fmt.bLineSpacingRule = 4;// bLineSpacingRule;
            fmt.dyLineSpacing = dyLineSpacing;
            fmt.dwMask = Consts.PFM_LINESPACING;
            try
            {
                Win32Api.SendMessage(new HandleRef(ctl, ctl.Handle), Consts.EM_SETPARAFORMAT, 0, ref fmt);
            }
            catch
            {

            }
        }

        #region 设置合理的边距，实现垂直居中



        //垂直居中
        public Struct.STRUCT_RECT RichTextBoxMargin
        {
            get
            {
                Struct.STRUCT_RECT rect = new Struct.STRUCT_RECT();
                Win32Api.SendMessage(this.Handle, Consts.EM_GETRECT, IntPtr.Zero, ref rect);
                rect.Left += 1;
                rect.Top += 1;
                rect.Right = 1 + this.ClientSize.Width - rect.Right;
                rect.Bottom = this.ClientSize.Height - rect.Bottom;
                return rect;
            }
            set
            {
                Struct.STRUCT_RECT rect;
                rect.Left = this.ClientRectangle.Left + value.Left;
                rect.Top = this.ClientRectangle.Top + value.Top;
                rect.Right = this.ClientRectangle.Right - value.Right;
                rect.Bottom = this.ClientRectangle.Bottom - value.Bottom;

                Win32Api.SendMessage(this.Handle, Consts.EM_SETRECT, IntPtr.Zero, ref rect);
            }
        }

        public static void SetMargin(RichTextBox rtb, Struct.STRUCT_RECT rect)
        {
            rect.Left = rtb.ClientRectangle.Left + rect.Left;
            rect.Top = rtb.ClientRectangle.Top + rect.Top;
            rect.Right = rtb.ClientRectangle.Right - rect.Right;
            rect.Bottom = rtb.ClientRectangle.Bottom - rect.Bottom;

            Win32Api.SendMessage(rtb.Handle, Consts.EM_SETRECT, IntPtr.Zero, ref rect);
        }

        ////设置
        //private void button1_Click(object sender, EventArgs e)
        //{
        //    Rect rect;
        //    rect.Left = 50;
        //    rect.Top = 50;
        //    rect.Right = 50;
        //    rect.Bottom = 50;
        //    RichTextBoxMargin = rect;
        //}

        /// <summary>
        /// 打印和textchanged的时候进行赋值。
        /// </summary>
        public void SetVerticalCenter(int topPara)
        {
            //默认边距是3
            Struct.STRUCT_RECT rect;
            rect.Left = 3;      //数值过大，就无效
            rect.Top = topPara; // 5;
            rect.Right = 3;
            rect.Bottom = 3;

            RichTextBoxMargin = rect;
        }

        /// <summary>
        /// 最外层封装的，提供调用的。
        /// 即使不用换行的单元格也要居中的啊。
        /// </summary>
        /// <param name="rtb"></param>
        public static Rectangle SetVerticalCenter(RichTextBox rtb)
        {
            int linesCount = 1;

            //0行的时候，当做一行的来处理
            if (rtb.Lines.Length != 0)
            {
                linesCount = rtb.Lines.Length;
            }

            int top = (rtb.Height - 0) / linesCount / 2 - (int)Math.Ceiling(rtb.Font.Size);

            Struct.STRUCT_RECT rect;
            rect.Left = 0;
            rect.Top = 0 + top;
            rect.Right = 0;
            rect.Bottom = 0;

            SetMargin(rtb, rect);

            return new Rectangle(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
        #endregion 设置合理的边距，实现垂直居中

        /// <summary>
        /// 自动换行后，如果出现垂直滚动条后，自动调整行间距
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        bool _IsProcessing = false;
        private int spacePre = 0;
        private void RichTextBoxExtended_VScroll(object sender, EventArgs e)
        {
            if (!_IsProcessing && !_IsTable) // && !_IsTable
            {
                _IsProcessing = true;

                int spaceThis = ((int)this.Font.Size + 1) * 20 + 2;

                if (spacePre == spaceThis) return;

                spacePre = spaceThis;

                //richTextBox2.ScrollBars = RichTextBoxScrollBars.None;

                int index = this.SelectionStart;
                this.SelectAll();

                //现将上面几行的行间距缩小
                //直接舍掉小数  比如float是4.7   转换成int  后是4  而不是5   要四舍五入的话转换前先加上0.5  比如   int  i ; double j = 4.7;     i = (int)(j+0.5);
                SetLineSpace(this, spaceThis); //((int)richTextBox1.Font.Size) * 20 * ((int)ud.Value);

                this.Select(index, 0);

                //再缩小本行的行间距
                SetLineSpace(this, spaceThis); //((int)richTextBox1.Font.Size) * 20 * ((int)ud.Value);

                //MessageBox.Show("请确认：输入框文字太多，可能显示不下，程序自动缩短了行间距。");
            }

            _IsProcessing = false;
        }

        # endregion 行间距


        //设置剪切
        private void menuItem6_Click(object sender, System.EventArgs e)
        {
            this.Cut();
        }

        //设置复制
        private void menuItem7_Click(object sender, System.EventArgs e)
        {
            this.Copy();
        }

        //设置粘贴
        private void menuItem8_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string str = Clipboard.GetText();

                    //先判断是否含有html标记
                    if (str.Contains(@"</") || str.Contains(@"&nbsp;"))
                    {
                        int oldMaxLength = this.MaxLength;
                        if (this._IsTable && this._AdjustParagraphs && !this.Multiline)
                        {
                            this.MaxLength = 999999;
                        }

                        //如果是多行输入框 / 调整段落列的时候，由于程序换行和调整段落行是以文字计算，上下标还是没有了，但是没有html标记符号了。虽然破坏了上小标，但是也没有换行，很奇怪
                        //含有html标记的时候
                        //&nbsp;生化全套,肿瘤指标女 (2017-08-29 00:00:00 ):红细胞计数 :4.36x10^<SUP>12</SUP>/L 、血红蛋白 :126g/L 、 红细胞比积 :0.363L/L 、 红细胞平均体积 :83.3fL 、 平均血红蛋白量 :28.9pg 、 平均血红蛋白浓度 :347g/L ;

                        str = str.Replace("&nbsp;", " "); //替换html的空格
                        //如果直接在本输入框中，每次粘贴之前的内容就没有了
                        RichTextBoxExtended rtbTemp = new RichTextBoxExtended();
                        rtbTemp._IsChangeAndLeaveEnable = false;
                        rtbTemp.Font = this.Font;
                        rtbTemp.AddHTML(str); //这样多行输入框的自动换行有问题。

                        rtbTemp.SelectAll();
                        rtbTemp.SelectionColor = this.ForeColor;
                        rtbTemp.SelectionFont = this.Font;

                        rtbTemp._IsChangeAndLeaveEnable = true;//防止干扰

                        //if (this.Multiline)
                        //{
                        this.SelectedText = rtbTemp.Text; //
                        //}
                        //else
                        //{
                        //    this.SelectedRtf = rtbTemp.Rtf;  //只有单行能粘贴近上下标样式等，所以干脆去掉，统一效果。这样已经去掉html标签了。
                        //}

                        //如果贴rtf需要。this._IsRtf = true;  

                        if (this._IsTable && this._AdjustParagraphs && !this.Multiline)
                        {
                            this.MaxLength = oldMaxLength;
                        }

                        return;
                    }

                    if (this.Multiline)
                    {
                        string strOld = str;
                        //str = str.Replace("\r\n", "\n");
                        //str = str.Replace("\n", "");    //移除换行符，因为单行输入框只能粘帖进复制的第一行，所以要进行处理

                        //由于部分有网管系统，C盘是不能进行更改操作，在管理员权限下，电脑里面复制粘贴没有问题，但是切换到科室用户，复制粘贴经常会出现复制不上，
                        //所以我希望知道我们的新表单运行的时候在C盘什么文件夹内产生操作，这样可以让信息科给此文件夹给予权限！

                        Clipboard.SetText(str); //, TextDataFormat.UnicodeText 

                        //粘帖问题的调试日志
                        //Comm.Logger.WriteInformation("粘帖内容：" + str); //加上这个会偶尔粘帖不上？！
                        System.Threading.Thread.Sleep(50);  //不加上这个会偶尔粘帖不上？！
                        //Application.DoEvents();
                        //Application.DoEvents(); 

                        this.Paste();

                        Clipboard.SetText(strOld); //, TextDataFormat.UnicodeText

                        //if (!this._IsRtf)   //防止当前输入框不是rtf模式，拷贝进来rtf当时显示格式，再次打开就不是了
                        //{
                        //    int index = this.SelectionStart;    //记录修改时光标的位置

                        //    //2&#xA;时间&#xA;999&#xA; 回车是转移符号的再次转义
                        //    toolStripMenuItemClearFormat_Click(null, null);//清除格式

                        //    //清除格式后会全选的
                        //    this.Select(index, 0);
                        //}
                    }
                    else
                    {
                        //如果输入框为但行，那么如果复制的文字是换行的，先进行取消换行，否则只能拷贝一行
                        if (Clipboard.ContainsText())
                        {
                            int oldMaxLength = this.MaxLength;
                            if (this._IsTable && this._AdjustParagraphs)
                            {
                                this.MaxLength = 999999;
                            }

                            //string str = Clipboard.GetText();
                            string strOld = str;
                            str = str.Replace("\r\n", "");
                            str = str.Replace("\n", "");    //移除换行符，因为单行输入框只能粘帖进复制的第一行，所以要进行处理                     

                            //由于部分有网管系统，C盘是不能进行更改操作，在管理员权限下，电脑里面复制粘贴没有问题，但是切换到科室用户，复制粘贴经常会出现复制不上，
                            //所以我希望知道我们的新表单运行的时候在C盘什么文件夹内产生操作，这样可以让信息科给此文件夹给予权限！

                            Clipboard.SetText(str); //, TextDataFormat.UnicodeText 

                            //粘帖问题的调试日志
                            //Comm.Logger.WriteInformation("粘帖内容：" + str); //加上这个会偶尔粘帖不上？！
                            System.Threading.Thread.Sleep(50);  //不加上这个会偶尔粘帖不上？！
                            //Application.DoEvents();
                            //Application.DoEvents(); 

                            this.Paste();

                            Clipboard.SetText(strOld); //, TextDataFormat.UnicodeText

                            if (this._IsTable && this._AdjustParagraphs)
                            {
                                this.MaxLength = oldMaxLength;
                            }
                        }
                        else
                        {
                            //粘帖问题的调试日志
                            Comm.LogHelp.WriteInformation("粘帖时发现，没有可粘帖的文字内容。");
                            //this.Paste(); //电脑可能复制不进去
                        }
                    }
                }
                else
                {
                    //如果是复制的单元格:不包含文本的
                    //IDataObject ido =  Clipboard.GetDataObject();
                    if (_IsTable && CopyCellsEvent != null)
                    {
                        CopyCellsEvent(this, null);
                    }

                    this.Paste();
                    Comm.LogHelp.WriteInformation("不能拷贝非文字内容到输入框中：" + this.Name);
                    //MessageBox.Show("输入框只能粘帖文字内容。");
                }
            }
            catch (Exception ex)
            {
                this.Paste();
                Comm.LogHelp.WriteErr(ex);
                //MessageBox.Show("粘帖出现异常。");
            }
        }

        //全选
        private void menuItem11_Click(object sender, System.EventArgs e)
        {
            this.Focus();
            this.SelectAll();
        }

        //清空
        private void menuItem12_Click(object sender, System.EventArgs e)
        {
            this.Clear();
            //this.Text = "";
            this.Focus();
        }

        private void rtb1_Enter(object sender, EventArgs e)
        {

            //SetHalfShape(this.Handle); //设置为半角输入法
            //IMEClass.SetHalfShape(this.Handle);

            //日期：设置格式化菜单
            if (this.Name.StartsWith("日期"))
            {
                _YMD_Old = _YMD;//修改了格式，也要提示保存

                if (_FullDateText != this.Text) //这样如果是rtf，可以保留部分的文字样式
                {
                    HorizontalAlignment temp = this.SelectionAlignment;
                    this.Text = _FullDateText; //显示完整值
                    this.SelectionAlignment = temp;
                }
            }

            SetOldRtf();

            OldText = this.Text;

            if (MenuSelectItemShowMode == 0) //默认是0，光标进入需要显示。如果为1，那么不显示。
            {
                if (Is_SelectItem && !Comm._SettingRowForeColor && !this.ReadOnly && !Comm._EnterNotShowMenu)//是否含有选择项，无法通过Visable来判断
                {

                }
            }

            //IMEClass.SetHalfShape(this.Handle); //Tab键的不行，还是全角

            //解决Tab键切换，还是全角的问题 （都是在Enter中处理）
            if (this.ImeMode != ImeMode.Hangul) //ImeMode.Hangul
            {
                this.ImeMode = ImeMode.Hangul;
            }

            if (this._HaveDoubleLine || this._HaveSingleLine)
            {
                this.Refresh(); //不然光标进去，最后一个字符后面，不会消失红线； 不适合透明输入框，否则会丢失
            }

            _PreStr = this.Text;
        }

        /// <summary>
        /// 设置菜单项，隐藏
        /// </summary>
        /// <param name="vPara"></param>
        private void SetStripVisable(bool vPara)
        {
            for (int i = 0; i < this.linecmenu.Items.Count; i++)
            {
                ////隐藏后再显示的时候，防止出乱是否要显示：特殊字符，知识库，选择项：设置为True要先判断Tag
                if (vPara == false || this.linecmenu.Items[i].Tag == null)
                {
                    this.linecmenu.Items[i].Visible = vPara;
                }
                else if (vPara && this.linecmenu.Items[i].Tag != null)
                {
                    if (this.linecmenu.Items[i].Tag.ToString() == "false")
                    {
                        this.linecmenu.Items[i].Visible = false;
                    }
                }
                else
                {
                    this.linecmenu.Items[i].Visible = vPara;
                }
            }
        }

        //protected override OnPaint(PaintEventArgs e)
        //{
        //    //自己的代码
        //  base.OnPaint(e);       
        //}

        //特殊字符 & 知识库
        private void menuItemAddWord_Click(object sender, System.EventArgs e)
        {
            if (this.ReadOnly)
            {
                ShowInforMsg_MainForm("只读对象，无法写入。", null);
                return; //如果只读，则无效
            }

            ToolStripItem me = (ToolStripItem)sender;

            HorizontalAlignment temp = this.SelectionAlignment;

            this.SelectedText = "";//替换掉选择的文字

            int index = this.SelectionStart;    //记录修改时光标的位置

            this.Focus();
            string insert = me.Tag.ToString();
            this.Text = this.Text.Insert(index, insert);
            this.Select(index + insert.Length, 0);

            this.SelectionAlignment = temp;  //不然会默认居左的，要保留之前的对齐方式

            //恢复光标处设置
            //this.RichTextBox.Select(index, 0);     //返回修改的位置
        }


        //<KNOWLEDG Name="Knowledg" Notes="护理知识库 改善防止名字过长，屏幕全屏等现象，分为显示缩略名和实际内容两部分">
        //  <Item Text="医学用词" Value="医学用词1234567890"></Item>
        //  <Floder Text="一级文件夹主菜单" Value="">
        //      <Item Text="一级子1" Value="一级子1 1234567890"></Item>
        //      <Item Text="一级子2" Value="一级子2 1234567890"></Item>
        //         <Floder Text="二级文件夹主菜单" Value="">
        //            <Item Text="二级子1" Value="二级子1 1234567890"></Item>
        //            <Item Text="二级子2" Value="二级子2 1234567890"></Item>
        //        </Floder>
        //  </Floder>
        //</KNOWLEDG>
        /// <summary>
        /// 设置知识库
        /// </summary>
        /// <param name="wordPara"></param>
        public void SetKnowledg(XmlNode nodePara, ToolStripMenuItem tsiPara)
        {
            _XnKnowledg = nodePara;
        }

        /// <summary>
        /// 设置特殊字符的字菜单：︴表示分割线
        /// </summary>
        /// <param name="wordPara"></param>
        public void SetSelectWords(string wordPara)
        {
            _SpecialWords = wordPara;//
        }

        //选择项点击，添加文字，不能包含一样的文字
        private void menuItemAddItem_Click(object sender, System.EventArgs e)
        {
            if (this.ReadOnly)
            {
                ShowInforMsg_MainForm("只读对象，无法写入。", null);
                return; //如果只读，则无效
            }

            ToolStripItem me = (ToolStripItem)sender;

            HorizontalAlignment temp = this.SelectionAlignment;

            //分隔符号，用空格分开，是不是操作性更好，以后可扩展
            string separator = "";

            this.SelectedText = "";//替换掉选择的文字

            //防止重复选择项，所以替换掉
            if (this.Text == me.Tag.ToString())
            {
                return;
            }

            if (this.Text.Contains(me.Tag.ToString() + separator))
            {
                return;
            }

            if (this.Text.Contains(separator + me.Tag.ToString()))
            {
                return;
            }

            this.Focus();

            int index = this.SelectionStart;    //记录修改时光标的位置

            string insert = "";
            if (string.IsNullOrEmpty(this.Text))
            {
                insert = me.Tag.ToString();
                this.Text = this.Text.Insert(index, insert);
            }
            else
            {
                insert = separator + me.Tag.ToString();
                this.Text = this.Text.Insert(index, insert);
            }

            this.Select(index + insert.Length, 0);

            this.SelectionAlignment = temp;  //不然会默认居左的，要保留之前的对齐方式
        }

        /// <summary>
        /// 设置选择项目子菜单（可标统代替勾选框）：︴表示分组
        /// </summary>
        /// <param name="wordPara"></param>
        public void SetSelectItems(string wordPara)
        {
            _SelectItems = wordPara;

            if (string.IsNullOrEmpty(wordPara))
            {
                Is_SelectItem = false;
            }
            else
            {
                Is_SelectItem = true;
            }
        }

        /// <summary>
        /// 初始化选择项菜单
        /// </summary>
        /// <param name="wordPara"></param>
        private void InitSelectItems(string wordPara)
        {
            //子项A¤A1¤A2§子项B¤B11¤B22§子项C¤C11¤C22 
            //添加菜单
            ToolStripMenuItem addTSTGroup;
            ToolStripItem addTST;

            //独立菜单，光标进入就显示的。不在整个右击菜单中显示，那样选择麻烦，层级多一级。
            //这个单独的菜单和下拉框效果类似
            ToolStripMenuItem addTSTGroupMenu;
            ToolStripItem addTSTMenu;
            string showName = "";
            string showValue = ""; //用@符号分割，显示文字和输入的文字不一样

            //M1:，如果以这个开头，表示设置了模式
            //默认为0，如果是0，每次光标进入单元格会显示独立的选择项菜单，右击显示完整菜单。 如果为1：光标进入不显示，右击显示独立的选择项菜单，按下Ctrl显示完整菜单。2：光标进入也不显示，但是右击的时候是蒋选择项菜单提到第一层。
            if (wordPara.StartsWith("M1:"))
            {
                //只有右键的时候，才显示选择项独立菜单，光标进入不显示；并且当按下Ctrl+右击才会显示完整的菜单
                MenuSelectItemShowMode = 1; //光标进入不弹出菜单，
                wordPara = wordPara.Substring(3, wordPara.Length - 3);
            }
            else if (wordPara.StartsWith("M2:"))
            {
                //光标进入也不显示，但是右击的时候是蒋选择项菜单提到第一层。右击就是显示完整的菜单。等于就是独立的选择项菜单不会显示了
                MenuSelectItemShowMode = 2; //光标进入不弹出菜单，
                wordPara = wordPara.Substring(3, wordPara.Length - 3);

                if (MenuSelectItemShowMode == 2)
                {
                    this.toolStripMenuItemSelectItem.Visible = false; //多一层的就不要显示了

                    if (wordPara.Trim() != "")
                    {
                        ToolStripSeparator sp1 = new ToolStripSeparator();
                        sp1.Name = "M2分割线";
                        sp1.Tag = sp1.Name;
                        sp1.Visible = true;
                        this.linecmenu.Items.Add(sp1);                    //隐藏原来的选择项层次项，并且添加和上面其他菜单项的分割线
                    }
                }
            }

            if (wordPara.Trim() != "")
            {
                string[] eachArrGroup = wordPara.Split('§');
                string[] eachArr;

                int countGroup = eachArrGroup.Length;

                contextMenuStripSelectItem.Items.Clear();
                toolStripMenuItemSelectItem.DropDownItems.Clear();

                for (int i = 0; i < eachArrGroup.Length; i++)
                {
                    eachArr = eachArrGroup[i].Split('¤');

                    showName = eachArr[0];
                    showValue = showName;

                    ////如果想要知识库一样，显示的内容和选择后填入的内容不一样。那么用@符号分割，显示名@填入内容，为空或者没有@符号，就都用一样的-和原来逻辑一直
                    //SetTextValue(showName, ref showName, ref showValue);

                    addTSTGroup = new ToolStripMenuItem();
                    addTSTGroup.Name = showName;
                    addTSTGroup.Text = showName;
                    addTSTGroup.Tag = showValue;        //showName;

                    addTSTGroupMenu = new ToolStripMenuItem();
                    addTSTGroupMenu.Name = showName;
                    addTSTGroupMenu.Text = showName;
                    addTSTGroupMenu.Tag = showValue;    //showName;

                    int start = 1;
                    if (countGroup == 1)
                    {
                        start = 0; //没有分组的时候
                    }

                    for (int j = start; j < eachArr.Length; j++)
                    {
                        showName = eachArr[j];
                        showValue = showName;

                        if (string.IsNullOrEmpty(showName))
                        {
                            continue;
                        }

                        if (showName == "圈R")//后期调查发现，设置模板xml的文件编码类型和xml的编码类型就可以解决，不会乱码了。这个也就没有必要了
                        {
                            showName = "®"; //这个特殊字符必须特殊处理，否则每次都是问号？？
                        }

                        //如果想要知识库一样，显示的内容和选择后填入的内容不一样。那么用@符号分割，显示名@填入内容，为空或者没有@符号，就都用一样的-和原来逻辑一直
                        SetTextValue(showName, ref showName, ref showValue);

                        addTST = new ToolStripMenuItem();
                        addTST.Name = showName;
                        addTST.Size = new System.Drawing.Size(100, 22);
                        addTST.Text = showName;
                        addTST.Tag = showValue;

                        addTST.Click += new System.EventHandler(this.menuItemAddItem_Click);

                        addTSTMenu = new ToolStripMenuItem();
                        addTSTMenu.Name = showName;
                        addTSTMenu.AutoSize = true;
                        addTSTMenu.Text = showName;
                        addTSTMenu.Tag = showValue;

                        addTSTMenu.Click += new System.EventHandler(this.menuItemAddItem_Click);

                        //选项菜单分组
                        if (countGroup > 1)
                        {
                            addTSTGroup.DropDownItems.Add(addTST);

                            addTSTGroupMenu.DropDownItems.Add(addTSTMenu);
                        }
                        else
                        {
                            //this.toolStripMenuItemSelectItem.DropDownItems.Add(addTST);  //完整右击菜单中的选择项的子项
                            if (MenuSelectItemShowMode == 2)
                            {
                                //直接显示到菜单根，第一层。如果选择项不是太多，可以这么干
                                this.linecmenu.Items.Add(addTST);
                                //this.toolStripMenuItemSelectItem.Visible = false; //多一层的就不要显示了
                            }
                            else
                            {
                                //显示到第一次的“选择项”中，虽然层次清晰，但是操作的时候，确实要多点一下。
                                this.toolStripMenuItemSelectItem.DropDownItems.Add(addTST);  //完整右击菜单中的选择项的子项
                            }

                            //独立的选择菜单，效果更好
                            contextMenuStripSelectItem.Items.Add(addTSTMenu);             //单独的选择项右击菜单
                        }
                    }

                    //分组的选项菜单
                    if (addTSTGroup.DropDownItems.Count > 0 && countGroup > 1)
                    {
                        if (MenuSelectItemShowMode == 2)
                        {
                            this.linecmenu.Items.Add(addTSTGroup);
                            //this.toolStripMenuItemSelectItem.Visible = false;
                        }
                        else
                        {
                            this.toolStripMenuItemSelectItem.DropDownItems.Add(addTSTGroup);  //完整右击菜单中的选择项的子项
                        }

                        contextMenuStripSelectItem.Items.Add(addTSTGroupMenu);            //单独的选择项右击菜单
                    }
                }

                if (MenuSelectItemShowMode == 2)
                {
                    this.toolStripMenuItemSelectItem.Visible = false; //如果一个项目都没有，那么隐藏起来。
                    toolStripMenuItemSelectItem.Tag = "false"; //隐藏后再显示的时候，防止出乱，是否要显示：特殊字符，知识库，选择项
                }
                else
                {
                    this.toolStripMenuItemSelectItem.Visible = true;
                    toolStripMenuItemSelectItem.Tag = null;
                }

                Is_SelectItem = true;
            }
            else
            {
                Is_SelectItem = false;
                this.toolStripMenuItemSelectItem.Visible = false; //如果一个项目都没有，那么隐藏起来。
                toolStripMenuItemSelectItem.Tag = "false"; //隐藏后再显示的时候，防止出乱，是否要显示：特殊字符，知识库，选择项
            }
        }

        private void InitSelectWords(string wordPara)
        {
            menuItem13.DropDownItems.Clear();

            _HelpString = wordPara;
            //HelpString="root○¤√¤∨¤∑︴1¤2¤︴¤3¤4"
            //按照︴分割线分，每组前面如果有root，表示此组放入菜单根中，否则加入到menuItem13特殊字符中
            //--------------------------------------------new_begin---------------------------------------
            if (wordPara.Trim() != "")
            {
                var arr = wordPara.Split('︴');
                int tssIndex = -1;
                //加入菜单根目录中
                foreach (var str in arr.Where(a => a.StartsWith("root", StringComparison.OrdinalIgnoreCase)))
                {
                    tssIndex++;
                    string strTemp = str.Substring(4);
                    var items = strTemp.Split('¤').Select(b => new ToolStripMenuItem(b, null, menuItemAddWord_Click)
                    {
                        Name = b,
                        Size = new System.Drawing.Size(100, 22),
                        Tag = b,
                    });
                    this.linecmenu.Items.AddRange(items.ToArray());
                    this.linecmenu.Items.Add(new ToolStripSeparator() { Name = "tss_" + tssIndex });
                }
                this.linecmenu.Items.RemoveByKey("tss_" + tssIndex);

                //加入到menuItem13特殊字符
                foreach (var str in arr.Where(a => !a.StartsWith("root", StringComparison.OrdinalIgnoreCase)))
                {
                    tssIndex++;
                    var items = str.Split('¤').Select(b => new ToolStripMenuItem(b, null, menuItemAddWord_Click)
                    {
                        Name = b,
                        Size = new System.Drawing.Size(100, 22),
                        Tag = b,
                    });
                    this.menuItem13.DropDownItems.AddRange(items.ToArray());
                    this.menuItem13.DropDownItems.Add(new ToolStripSeparator() { Name = "tss_" + tssIndex });
                }
                this.menuItem13.DropDownItems.RemoveByKey("tss_" + tssIndex);
            }
            if (this.menuItem13.DropDownItems.Count == 0)
            {
                this.menuItem13.Visible = false;
                this.menuItem13.Tag = "false";
            }
            //--------------------------------------------new-end-----------------------------------------

            //--------------------------------------------old-begin---------------------------------------
            //添加特殊字符，菜单
            //ToolStripItem addTST;
            //string showName;
            //if (wordPara.Trim() != "")
            //{
            //    string[] eachArr = wordPara.Split('¤');

            //    for (int i = 0; i < eachArr.Length; i++)
            //    {
            //        showName = eachArr[i];

            //        ////ToolStripSeparator toolStripSeparator1;  ︴，如果字符太多，可以配置分割线来区分显示
            //        if (showName != "︴")
            //        {
            //            if (showName == "圈R")
            //            {
            //                showName = "®"; //这个特殊字符必须特殊处理，否则每次都是问号？？
            //            }

            //            addTST = new ToolStripMenuItem();
            //            addTST.Name = showName;
            //            addTST.Size = new System.Drawing.Size(100, 22);
            //            addTST.Text = showName;
            //            addTST.Tag = showName;

            //            addTST.Click += new System.EventHandler(this.menuItemAddWord_Click);
            //            this.menuItem13.DropDownItems.Add(addTST);
            //        }
            //        else
            //        {
            //            ToolStripSeparator tss = new ToolStripSeparator();
            //            this.menuItem13.DropDownItems.Add(tss);
            //        }
            //    }
            //}
            //else
            //{
            //    this.menuItem13.Visible = false; //如果一个项目都没有，那么隐藏起来。
            //    menuItem13.Tag = "false";
            //}
            //--------------------------------------------old-end-----------------------------------------
        }

        private void InitKnowledg(XmlNode nodePara, ToolStripMenuItem tsiPara)
        {
            toolStripMenuItemKnowledg.DropDownItems.Clear();

            //添加特殊字符，菜单
            ToolStripMenuItem addTST;

            if (nodePara != null)
            {
                //<strip key="配置内容">
                //  <strip key="少多" value="" type="0">
                //    <strip key="1" value="1的代表意思是第一种" type="1" />
                //    <strip key="2" value="第二种" type="1" />
                //    <strip key="3" value="第三种" type="1" />
                //    <strip key="未命名" value="未命名" type="1" />
                //    <strip key="未命名" value="未命名" type="1" />
                //    <strip key="未命名" value="未命名" type="1" />
                //</strip>

                foreach (XmlNode xn in nodePara.ChildNodes)
                {
                    //如果是注释那么跳过
                    if (xn.Name == @"#comment" || xn.Name == @"#text")
                    {
                        continue;
                    }

                    //输入项
                    //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) != "子菜单")
                    if ((xn as XmlElement).GetAttribute("type") != "0")
                    {
                        addTST = new ToolStripMenuItem();
                        addTST.Name = (xn as XmlElement).GetAttribute("key"); //(xn as XmlElement).GetAttribute(nameof(EntXmlModel.Text));
                        addTST.Size = new System.Drawing.Size(100, 22);
                        addTST.Text = addTST.Name;
                        addTST.Tag = (xn as XmlElement).GetAttribute("value");//(xn as XmlElement).GetAttribute(nameof(EntXmlModel.Value));
                        addTST.ToolTipText = addTST.Tag.ToString();
                        addTST.Image = global::SparkFormEditor.Properties.Resources.特殊字符;
                        addTST.Click += new System.EventHandler(this.menuItemAddWord_Click);
                    }
                    else
                    {
                        //文件夹
                        addTST = new ToolStripMenuItem();
                        addTST.Name = (xn as XmlElement).GetAttribute("key");

                        //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Text)) == "圈R") //ⓧ ®
                        //{
                        //    addTST.Name = "®"; //这个特殊字符必须特殊处理，否则每次都是问号？？
                        //}

                        addTST.Size = new System.Drawing.Size(100, 22);
                        addTST.Text = addTST.Name;
                        //addTST.Tag = (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Value));
                        addTST.Image = global::SparkFormEditor.Properties.Resources.文件夹;

                        InitKnowledg(xn, addTST);
                    }

                    if (tsiPara == null)
                    {
                        toolStripMenuItemKnowledg.DropDownItems.Add(addTST);
                    }
                    else
                    {
                        tsiPara.DropDownItems.Add(addTST);
                    }
                }
            }
            else
            {
                //if (isRoot) // 首次调用 每个窗口实例化知识库，可能会降低性能，可以放入到主窗体加载一次，赋值过来
                //{
                toolStripMenuItemKnowledg.Visible = false;
                toolStripMenuItemKnowledg.Tag = "false";
                //}
            }
        }

        /// <summary>
        /// 初始化菜单
        /// </summary>
        private void InitMenus()
        {
            if (this.linecmenu == null || this.linecmenu.IsDisposed)
            {
                this.linecmenu = new ContextMenuStrip();// System.Windows.Forms.ContextMenuStrip(this.components);
                this.contextMenuStripSelectItem = new ContextMenuStrip();

                this.toolStripMenuItemInfor = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripMenuItemParagraphContent = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripMenuItemSumSmall = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripMenuItemSumBig = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
                this.tsmiShowCalendar = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripSeparatorCalendar = new System.Windows.Forms.ToolStripSeparator();
                this.tsmiDate_YMD = new System.Windows.Forms.ToolStripMenuItem();
                this.tsmiDate_MD = new System.Windows.Forms.ToolStripMenuItem();
                this.tsmiDate_D = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripSeparatorDate = new System.Windows.Forms.ToolStripSeparator();
                this.menuItem6 = new System.Windows.Forms.ToolStripMenuItem();
                this.menuItem7 = new System.Windows.Forms.ToolStripMenuItem();
                this.menuItem8 = new System.Windows.Forms.ToolStripMenuItem();
                this.menuItem11 = new System.Windows.Forms.ToolStripMenuItem();
                this.menuItem12 = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
                this.menuItem13 = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripMenuItemKnowledg = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripMenuItemSelectItem = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
                this.toolStripMenuItemUp = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripMenuItemDown = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
                this.toolStripMenuItemUndo = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripMenuItemRedo = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
                this.toolStripMenuItemClearFormat = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripMenuItemAutoPositionByTime = new System.Windows.Forms.ToolStripMenuItem();

                // 
                // linecmenu
                // 
                this.linecmenu.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
                this.linecmenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripMenuItemInfor,
                this.toolStripMenuItemParagraphContent,
                this.toolStripMenuItemAutoPositionByTime,
                this.toolStripMenuItemSumSmall,
                this.toolStripMenuItemSumBig,
                this.toolStripSeparator5,
                this.tsmiShowCalendar,
                this.toolStripSeparatorCalendar,
                this.tsmiDate_YMD,
                this.tsmiDate_MD,
                this.tsmiDate_D,
                this.toolStripSeparatorDate,
                this.menuItem6,
                this.menuItem7,
                this.menuItem8,
                this.menuItem11,
                this.menuItem12,
                this.toolStripSeparator4,
                this.menuItem13,
                this.toolStripMenuItemKnowledg,
                this.toolStripMenuItemSelectItem,
                this.toolStripSeparator1,
                this.toolStripMenuItemUp,
                this.toolStripMenuItemDown,
                this.toolStripSeparator2,
                this.toolStripMenuItemUndo,
                this.toolStripMenuItemRedo,
                this.toolStripSeparator3,
                this.toolStripMenuItemClearFormat});
                this.linecmenu.Name = "linecmenu";
                this.linecmenu.Size = new System.Drawing.Size(161, 530);

                // 
                // toolStripMenuItemInfor
                // 
                this.toolStripMenuItemInfor.Enabled = false;
                this.toolStripMenuItemInfor.ForeColor = System.Drawing.Color.DarkBlue;
                this.toolStripMenuItemInfor.Image = global::SparkFormEditor.Properties.Resources.editPen;
                this.toolStripMenuItemInfor.Name = "toolStripMenuItemInfor";
                this.toolStripMenuItemInfor.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemInfor.Text = "正在编辑：";
                // 
                // toolStripMenuItemParagraphContent
                // 
                this.toolStripMenuItemParagraphContent.Image = global::SparkFormEditor.Properties.Resources._8;
                this.toolStripMenuItemParagraphContent.Name = "toolStripMenuItemParagraphContent";
                this.toolStripMenuItemParagraphContent.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemParagraphContent.Text = "调整段落文字";
                this.toolStripMenuItemParagraphContent.Click += new System.EventHandler(this.ShowParagraphContent);
                // 
                // toolStripMenuItemSumSmall
                // 
                this.toolStripMenuItemSumSmall.Name = "toolStripMenuItemSumSmall";
                this.toolStripMenuItemSumSmall.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemSumSmall.Text = "小结";
                this.toolStripMenuItemSumSmall.Click += new System.EventHandler(this.ShowSumMainForm);
                // 
                // toolStripMenuItemSumBig
                // 
                this.toolStripMenuItemSumBig.Name = "toolStripMenuItemSumBig";
                this.toolStripMenuItemSumBig.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemSumBig.Text = "总结";
                this.toolStripMenuItemSumBig.Click += new System.EventHandler(this.ShowSumMainForm);
                // 
                // toolStripSeparator5
                // 
                this.toolStripSeparator5.Name = "toolStripSeparator5";
                this.toolStripSeparator5.Size = new System.Drawing.Size(157, 6);
                // 
                // tsmiShowCalendar
                // 
                this.tsmiShowCalendar.ForeColor = System.Drawing.SystemColors.ControlText;
                this.tsmiShowCalendar.Name = "tsmiShowCalendar";
                this.tsmiShowCalendar.Size = new System.Drawing.Size(160, 22);
                this.tsmiShowCalendar.Text = "日历";
                this.tsmiShowCalendar.Click += new System.EventHandler(this.tsmiShowCalendar_Click);
                // 
                // toolStripSeparatorCalendar
                // 
                this.toolStripSeparatorCalendar.Name = "toolStripSeparatorCalendar";
                this.toolStripSeparatorCalendar.Size = new System.Drawing.Size(157, 6);
                // 
                // tsmiDate_YMD
                // 
                this.tsmiDate_YMD.Name = "tsmiDate_YMD";
                this.tsmiDate_YMD.Size = new System.Drawing.Size(160, 22);
                this.tsmiDate_YMD.Text = "年月日";
                this.tsmiDate_YMD.Click += new System.EventHandler(this.tsmiDate_Click);
                // 
                // tsmiDate_MD
                // 
                this.tsmiDate_MD.Name = "tsmiDate_MD";
                this.tsmiDate_MD.Size = new System.Drawing.Size(160, 22);
                this.tsmiDate_MD.Text = "月日";
                this.tsmiDate_MD.Click += new System.EventHandler(this.tsmiDate_Click);
                // 
                // tsmiDate_D
                // 
                this.tsmiDate_D.Name = "tsmiDate_D";
                this.tsmiDate_D.Size = new System.Drawing.Size(160, 22);
                this.tsmiDate_D.Text = "日";
                this.tsmiDate_D.Click += new System.EventHandler(this.tsmiDate_Click);
                // 
                // toolStripSeparatorDate
                // 
                this.toolStripSeparatorDate.Name = "toolStripSeparatorDate";
                this.toolStripSeparatorDate.Size = new System.Drawing.Size(157, 6);
                //// 
                //// menuItem6
                //// 
                //this.menuItem6.Name = "menuItem6";
                //this.menuItem6.Size = new System.Drawing.Size(160, 22);
                //this.menuItem6.Text = "剪切";
                //this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
                //// 
                //// menuItem7
                //// 
                //this.menuItem7.Name = "menuItem7";
                //this.menuItem7.Size = new System.Drawing.Size(160, 22);
                //this.menuItem7.Text = "复制";
                //this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
                //// 
                //// menuItem8
                //// 
                //this.menuItem8.Name = "menuItem8";
                //this.menuItem8.Size = new System.Drawing.Size(160, 22);
                //this.menuItem8.Text = "粘贴";
                //this.menuItem8.Click += new System.EventHandler(this.menuItem8_Click);
                //// 
                //// menuItem11
                //// 
                //this.menuItem11.Name = "menuItem11";
                //this.menuItem11.Size = new System.Drawing.Size(160, 22);
                //this.menuItem11.Text = "全选";
                //this.menuItem11.Click += new System.EventHandler(this.menuItem11_Click);
                //// 
                //// menuItem12
                //// 
                this.menuItem12.Name = "menuItem12";
                this.menuItem12.Size = new System.Drawing.Size(160, 22);
                this.menuItem12.Text = "清空";
                this.menuItem12.Click += new System.EventHandler(this.menuItem12_Click);
                // 
                // toolStripSeparator4
                // 
                this.toolStripSeparator4.Name = "toolStripSeparator4";
                this.toolStripSeparator4.Size = new System.Drawing.Size(157, 6);
                // 
                // menuItem13
                // 
                this.menuItem13.Image = global::SparkFormEditor.Properties.Resources.特殊字符;
                this.menuItem13.Name = "menuItem13";
                this.menuItem13.Size = new System.Drawing.Size(160, 22);
                this.menuItem13.Text = "特殊字符";
                // 
                // toolStripMenuItemKnowledg
                // 
                //this.toolStripMenuItemKnowledg.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemKnowledg.Image")));
                this.toolStripMenuItemKnowledg.Name = "toolStripMenuItemKnowledg";
                this.toolStripMenuItemKnowledg.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemKnowledg.Text = "知识库";
                // 
                // toolStripMenuItemSelectItem
                // 
                this.toolStripMenuItemSelectItem.Name = "toolStripMenuItemSelectItem";
                this.toolStripMenuItemSelectItem.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemSelectItem.Text = "选择项";
                // 
                // toolStripSeparator1
                // 
                this.toolStripSeparator1.Name = "toolStripSeparator1";
                this.toolStripSeparator1.Size = new System.Drawing.Size(157, 6);
                // 
                // toolStripMenuItemUp
                // 
                this.toolStripMenuItemUp.Name = "toolStripMenuItemUp";
                this.toolStripMenuItemUp.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemUp.Text = "上标";
                this.toolStripMenuItemUp.Click += new System.EventHandler(this.toolStripMenuItemUp_Click);
                // 
                // toolStripMenuItemDown
                // 
                this.toolStripMenuItemDown.Name = "toolStripMenuItemDown";
                this.toolStripMenuItemDown.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemDown.Text = "下标";
                this.toolStripMenuItemDown.Click += new System.EventHandler(this.toolStripMenuItemDown_Click);
                // 
                // toolStripSeparator2
                // 
                this.toolStripSeparator2.Name = "toolStripSeparator2";
                this.toolStripSeparator2.Size = new System.Drawing.Size(157, 6);
                // 
                // toolStripMenuItemUndo
                // 
                //this.toolStripMenuItemUndo.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemUndo.Image")));
                this.toolStripMenuItemUndo.Name = "toolStripMenuItemUndo";
                this.toolStripMenuItemUndo.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemUndo.Text = "撤销";
                this.toolStripMenuItemUndo.Click += new System.EventHandler(this.toolStripMenuItemUndo_Click);
                // 
                // toolStripMenuItemRedo
                // 
                //this.toolStripMenuItemRedo.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemRedo.Image")));
                this.toolStripMenuItemRedo.Name = "toolStripMenuItemRedo";
                this.toolStripMenuItemRedo.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemRedo.Text = "重做";
                this.toolStripMenuItemRedo.Click += new System.EventHandler(this.toolStripMenuItemRedo_Click);
                // 
                // toolStripSeparator3
                // 
                this.toolStripSeparator3.Name = "toolStripSeparator3";
                this.toolStripSeparator3.Size = new System.Drawing.Size(157, 6);
                // 
                // toolStripMenuItemClearFormat
                // 
                //this.toolStripMenuItemClearFormat.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemClearFormat.Image")));
                this.toolStripMenuItemClearFormat.Name = "toolStripMenuItemClearFormat";
                this.toolStripMenuItemClearFormat.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemClearFormat.Text = "清除格式";
                this.toolStripMenuItemClearFormat.Click += new System.EventHandler(this.toolStripMenuItemClearFormat_Click);

                // 
                // toolStripMenuItemAutoPositionByTime
                // 
                this.toolStripMenuItemAutoPositionByTime.Name = "toolStripMenuItemAutoPositionByTime";
                this.toolStripMenuItemAutoPositionByTime.Size = new System.Drawing.Size(160, 22);
                this.toolStripMenuItemAutoPositionByTime.Text = "自动调整行位置";
                this.toolStripMenuItemAutoPositionByTime.ToolTipText = "根据日期时间的顺序，调整行的位置。";
                this.toolStripMenuItemAutoPositionByTime.Click += new System.EventHandler(this.AutoPositionByTimemMainForm);

                // 上下标隐藏
                toolStripSeparator1.Visible = false;
                toolStripMenuItemUp.Visible = false;
                toolStripMenuItemDown.Visible = false;

                this.toolStripMenuItemSelectItem.Visible = false;
                toolStripMenuItemSelectItem.Tag = "false";

                //初始化数据，子菜单
                InitSelectItems(_SelectItems);

                InitSelectWords(_SpecialWords);

                //如果选择项和特殊符号都没有，则隐藏此线
                if ((toolStripMenuItemSelectItem.Tag ?? "").ToString() == "false" && (menuItem13.Tag ?? "").ToString() == "false")
                {
                    toolStripSeparator2.Visible = false;
                    toolStripSeparator2.Tag = "false";
                }
                InitKnowledg(_XnKnowledg, null);
            }
        }

        /// <summary>
        /// 将选择项功能进行扩展：显示文字，和选择后输入的文字可以不一样。
        /// </summary>
        /// <param name="setTextPara"></param>
        /// <param name="textPara"></param>
        /// <param name="valuePara"></param>
        private void SetTextValue(string setTextPara, ref string textPara, ref string valuePara)
        {
            //如果想要知识库一样，显示的内容和选择后填入的内容不一样。那么用@符号分割，显示名@填入内容，为空或者没有@符号，就都用一样的-和原来逻辑一直
            string[] eachArrTextValue = setTextPara.Split('@');

            if (eachArrTextValue.Length > 1)
            {
                textPara = eachArrTextValue[0].Trim();

                valuePara = "";//防止内容中又@符号，那么后面的全部作为内容。更加符号实施人员的配置逻辑
                for (int index = 1; index < eachArrTextValue.Length; index++)
                {
                    valuePara += eachArrTextValue[index];
                }
            }
        }


        /// <summary>
        /// 特殊的人性化操作处理：在tab键移动光标的时候，到了最后一个菜单，可以出来，防止在里面循环
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStripSelectItem_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //Tab键，遍历完左右子菜单是否有选中的，然后再选中当前输入框，然后按Tab移向其他输入框
            if (e.KeyCode == Keys.Tab && contextMenuStripSelectItem.Items[contextMenuStripSelectItem.Items.Count - 1].Selected)
            {
                contextMenuStripSelectItem.Hide();
            }

            //回车键 和tab键，如果没有选中任何子菜单的情况下
            if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Enter)   //Capture ContainsFocus 
            {
                //判断有没有被选中的子菜单
                bool isItemSelected = false;
                foreach (ToolStripMenuItem ts in contextMenuStripSelectItem.Items)
                {
                    if (ts.Selected)
                    {
                        isItemSelected = true;
                        break;
                    }
                }

                //如果没有选中任何子菜单，就隐藏，一遍光标切换到下一个单元格
                if (!isItemSelected)
                {
                    contextMenuStripSelectItem.Hide();
                }
            }
        }

        /// <summary>
        /// 以变通的方式，使得每次打开选择菜单的时候，不选择任何子菜单。
        /// 不然操作几次后，可能就会被选中第一个子菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStripSelectItem_VisibleChanged(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem ts in contextMenuStripSelectItem.Items)
            {
                if (ts.Selected)
                {
                    ts.Enabled = false;
                    ts.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 撤销
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemUndo_Click(object sender, EventArgs e)
        {
            this.Undo();

            //防止还一样，继续
        }

        /// <summary>
        /// 重做
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemRedo_Click(object sender, EventArgs e)
        {
            this.Redo();
        }


        /// <summary>
        /// 清除格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void toolStripMenuItemClearFormat_Click(object sender, EventArgs e)
        {
            _HaveSingleLine = false;
            _HaveDoubleLine = false;

            string txt = this.Text;
            this.Clear();
            this.Text = txt;
            this.SetAlignment();

            _IsRtf = false; //标记设置
        }

        /// <summary>
        /// 是否含有图片
        /// </summary>
        /// <returns></returns>
        public bool IsContainImg()
        {
            if (this.Rtf.IndexOf(@"{\pict\") > -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 下标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void toolStripMenuItemDown_Click(object sender, EventArgs e)
        {
            _IsChangeAndLeaveEnable = false;

            //默认选中光标之前那个字符
            if (this.SelectionLength == 0 && this.Text.Length > 0 && this.SelectionStart != 0)
            {
                this.Select(this.SelectionStart - 1, 1);
            }

            //if (toolStripMenuItemDown.Checked) //工具栏调用的时候，e为null
            if ((e != null && toolStripMenuItemDown.Checked) || (e == null && ((ToolStripMenuItem)sender).Checked))
            {
                this.SetSubScript(false);
            }
            else
            {
                this.SetSubScript(true);
                this.SetSuperScript(false);

                this.SelectionFont = new Font("宋体", this.SelectionFont.Size, this.SelectionFont.Style);
            }

            #region 自己写的方法，上下标
            ////if (toolStripMenuItemDown.Checked)
            //if ((e != null && toolStripMenuItemDown.Checked ) || (e == null && ((ToolStripButton)sender).Checked)) //工具栏调用的时候，e为null
            //{
            //    this.SelectionCharOffset = 0; //恢复上下位置
            //    this.SelectionFont = new Font(this.SelectionFont.FontFamily.Name, this.SelectionFont.Size + 2); //恢复字体
            //}
            //else
            //{
            //    OffsetRichText(_DownSet);
            //}
            #endregion 自己写的方法，上下标

            _IsChangeAndLeaveEnable = true;
        }

        /// <summary>
        /// 上标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void toolStripMenuItemUp_Click(object sender, EventArgs e)
        {
            _IsChangeAndLeaveEnable = false;

            //默认选中光标之前那个字符
            if (this.SelectionLength == 0 && this.Text.Length > 0 && this.SelectionStart != 0)
            {
                this.Select(this.SelectionStart - 1, 1);
            }

            if ((e != null && toolStripMenuItemUp.Checked) || (e == null && ((ToolStripMenuItem)sender).Checked))
            //if (toolStripMenuItemUp.Checked)
            {
                this.SetSuperScript(false);  //恢复取消上标
            }
            else
            {
                this.SetSuperScript(true);//设为上标
                this.SetSubScript(false); //取消下标

                this.SelectionFont = new Font("宋体", this.SelectionFont.Size, this.SelectionFont.Style);
            }

            #region 自己写的方法，上下标
            ////if (toolStripMenuItemUp.Checked)
            //if ((e != null && toolStripMenuItemUp.Checked) || (e == null && ((ToolStripButton)sender).Checked)) //工具栏调用的时候，e为null
            //{
            //    this.SelectionCharOffset = 0;
            //    this.SelectionFont = new Font(this.SelectionFont.FontFamily.Name, this.SelectionFont.Size + 2);
            //}
            //else
            //{
            //    OffsetRichText(_UpSet);
            //}
            #endregion 自己写的方法，上下标

            _IsChangeAndLeaveEnable = true;
        }

        #region 自己写的方法，上下标
        public bool _IsChangeAndLeaveEnable = true;
        //int _UpSet = 4;
        //int _DownSet = -2;

        //richTextBox1.Text =  "H2SO4";
        //OffsetRichText(richTextBox1,1,1,2);
        //OffsetRichText(richTextBox1, 4, 1, -2);
        ////设置上下标
        //private void OffsetRichText(int iOffset)
        //{
        //    Font richFont1 = new Font("宋体", this.Font.Size - 2);
        //    this.SelectionFont = richFont1;
        //    this.SelectionCharOffset = iOffset;
        //}
        #endregion 自己写的方法，上下标

        /// <summary>
        /// 当前输入框名
        /// </summary>
        private void setStripName()
        {
            try
            {
                string name = this.Name; // Substring(0, this.Name.Length - 3);

                if (name.StartsWith("CustomTile"))
                {
                    name = "自定义标题内容";
                }
                else if (name.StartsWith("自定义"))
                {
                    name = "自定义内容";
                }

                toolStripMenuItemInfor.Text = "正在编辑：" + name;
            }
            catch
            {

            }
        }

        /// <summary>
        /// 公外部调用，执行右击操作
        /// （点击画布的时候）
        /// </summary>
        public void RightMouseDown(MouseEventArgs e)
        {
            if (MenuSelectItemShowMode == 1)
            {
                RichTextBoxExtended_MouseDown(this, e); //这样 并不会触发右击菜单的

                if (this.ContextMenuStrip != null)
                {
                    this.ContextMenuStrip.Show(this, new Point(e.Location.X - this.Location.X, e.Location.Y - this.Location.Y)); //是的右击画布，能直接显示出来
                }
            }
        }

        /// <summary>
        /// 双击后，屏蔽全选，设置为当前位置
        /// </summary>
        public void DoubleClickSelecting()
        {
            if (_Calendar != null && !_Calendar.IsDisposed && _Calendar.Visible)
            {
                //会导致双击日期单元格的时候，不会弹出日历控件的
            }
            else
            {
                if (_IndexDoubleClickSelecting <= this.Text.Length)
                {
                    this.Focus();
                    this.Select(_IndexDoubleClickSelecting, 0);
                }
            }
        }

        /// <summary>
        /// 鼠标点击按下，对菜单等进行处理设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public int _IndexDoubleClickSelecting = 0; //双击会全选的，记录下双击是的光标位置
        private void RichTextBoxExtended_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks != 2)
            {
                _IndexDoubleClickSelecting = this.SelectionStart;
            }

            if (e.Button == MouseButtons.Right)
            {

                _MouseButton = MouseButtons.Right;

                InitMenus();

                SetStripVisable(true); //设置菜单显示，下面再判断哪些需要隐藏

                setStripName();//当前输入框名

                //防止光标不在，没有选中的时候，没有意义的设置
                if (this.Focused)
                {
                    this.ContextMenuStrip = this.linecmenu;
                }
                else
                {
                    this.Focus();
                    this.SelectAll();
                    this.ContextMenuStrip = this.linecmenu;
                }

                //显示独立的菜单：选择项，方便操作
                if (MenuSelectItemShowMode == 1)
                {
                    if ((Control.ModifierKeys & Keys.Control) != 0)  //if ((Control.ModifierKeys & Keys.Control) != Keys.Control)
                    {
                        //按下了Ctrl，那么还是显示默认的全部菜单
                    }
                    else
                    {
                        this.ContextMenuStrip = this.contextMenuStripSelectItem; //模式1 M1：右击时也只显示选择项菜单，只有按下Ctrl才显示全部的右击菜单
                    }
                }

                //签名：只能手动：双击单元格进行签名
                menuItem12.Enabled = !this.ReadOnly;
                //if (this.Name == "签名" && !this.ReadOnly)
                //{
                //    menuItem12.Enabled = true; //签名列，“清空”菜单无效
                //    toolStripMenuItemClearFormat.Enabled = true;
                //}
                //else
                //{
                //    menuItem12.Enabled = true;
                //    toolStripMenuItemClearFormat.Enabled = true;
                //}

                //显示段落内容
                if (this._AdjustParagraphs)
                {
                    toolStripMenuItemParagraphContent.Visible = true;
                }
                else
                {
                    toolStripMenuItemParagraphContent.Visible = false;//不是调整段落列列，“段落内容”菜单隐藏
                }

                //是否开启，并配置了自动小结、总结功能
                if (!_EnableSUM_MainForm)
                {
                    toolStripMenuItemSumSmall.Visible = false;
                    toolStripMenuItemSumBig.Visible = false;
                }
                else
                {
                    toolStripMenuItemSumSmall.Visible = true;
                    toolStripMenuItemSumBig.Visible = true; //属性设在表格上，点击任何一列都可以进行进行：小结/总结
                }

                //if (_IsTable && (this.Name == "日期" || this.Name == "时间"))
                if (_CanAutoPositionByTime) //是否可以根据时间，自动调整位置 （表格行，有日期时间，并且没有调整段落列）
                {
                    toolStripMenuItemAutoPositionByTime.Visible = true;
                }
                else
                {
                    toolStripMenuItemAutoPositionByTime.Visible = false;
                }

                //日期：设置格式化菜单
                if (this.Name.StartsWith("日期"))
                {
                    this.tsmiShowCalendar.Visible = true;
                    this.toolStripSeparatorCalendar.Visible = true;
                    this.tsmiDate_YMD.Visible = true;
                    this.tsmiDate_MD.Visible = true;
                    this.tsmiDate_D.Visible = true;
                    this.toolStripSeparatorDate.Visible = true;

                    if (_YMD == "" || _YMD.Contains("yyyy"))
                    {
                        this.tsmiDate_YMD.Checked = true;
                        this.tsmiDate_MD.Checked = false;
                        this.tsmiDate_D.Checked = false;
                    }
                    else if (_YMD.Contains("M"))
                    {
                        this.tsmiDate_YMD.Checked = false;
                        this.tsmiDate_MD.Checked = true;
                        this.tsmiDate_D.Checked = false;
                    }
                    else
                    {
                        this.tsmiDate_YMD.Checked = false;
                        this.tsmiDate_MD.Checked = false;
                        this.tsmiDate_D.Checked = true;
                    }
                }
                else
                {
                    this.tsmiShowCalendar.Visible = false;
                    this.toolStripSeparatorCalendar.Visible = false;
                    this.tsmiDate_YMD.Visible = false;
                    this.tsmiDate_MD.Visible = false;
                    this.tsmiDate_D.Visible = false;
                    this.toolStripSeparatorDate.Visible = false;
                }


                //上下标状态显示
                if (_SeniorRtfMode)  //不是上下错开，即不是呼吸，并且是上下标高级rtf模式的输入框
                {
                    //如果没有选中，那么默认是光标前一个字符进行设置上下标，但是又不能一直选中，因为插入特殊字符会覆盖
                    int index = this.SelectionStart;
                    bool notSelected = false;
                    if (this.SelectionLength == 0 && this.Text.Length > 0
                        && index != 0)
                    {
                        notSelected = true;
                        this.Select(index - 1, 1);
                    }

                    //toolStripSeparator1.Visible = true;
                    //toolStripMenuItemUp.Visible = true;
                    //toolStripMenuItemDown.Visible = true;
                    toolStripSeparator1.Visible = false;
                    toolStripMenuItemUp.Visible = false;
                    toolStripMenuItemDown.Visible = false;

                    toolStripMenuItemUp.Enabled = true;
                    toolStripMenuItemDown.Enabled = true;

                    if (this.SelectionCharOffset > 0)
                    {
                        toolStripMenuItemUp.Checked = true;
                        toolStripMenuItemDown.Checked = false;
                    }
                    else if (this.SelectionCharOffset < 0)
                    {
                        toolStripMenuItemUp.Checked = false;
                        toolStripMenuItemDown.Checked = true;
                    }
                    else
                    {
                        toolStripMenuItemUp.Checked = false;
                        toolStripMenuItemDown.Checked = false;
                    }

                    //恢复选择-如果右击之前没有选择，那么还是没有选择
                    if (notSelected)
                    {
                        this.Select(index, 0);
                    }
                }
                else
                {
                    toolStripSeparator1.Visible = false;
                    toolStripMenuItemUp.Visible = false;
                    toolStripMenuItemDown.Visible = false;
                }

                this.menuItem6.Visible = false;
                this.menuItem7.Visible = false;
                this.menuItem8.Visible = false;
                this.menuItem11.Visible = false;
                this.menuItem12.Visible = false;
                this.toolStripSeparatorDate.Visible = false;
                this.toolStripSeparator4.Visible = false;

                this.toolStripMenuItemUndo.Visible = false;
                this.toolStripMenuItemRedo.Visible = false;
                this.toolStripSeparator3.Visible = false;
                this.toolStripMenuItemClearFormat.Visible = false;
            }
            else
            {
                _MouseButton = MouseButtons.Left;
            }

            //隐藏配置的，不需要的菜单项
            HideMenuItems(Comm._listInputBoxHideItems);
        }


        /// <summary>
        /// 调整段落文字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ShowParagraphContent(object sender, EventArgs e)
        {
            if (ShowParagraphContent_MainForm != null)
            {
                ShowParagraphContent_MainForm(sender, e); //回调表单主窗体的方法
            }
        }

        /// <summary>
        /// 调用主窗体的小结 总结
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ShowSumMainForm(object sender, EventArgs e)
        {
            if (SUM_MainForm != null)
            {
                SUM_MainForm(((ToolStripMenuItem)sender).Text, e); //回调表单主窗体的方法
            }
        }

        /// 调用主窗体的自动调整位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AutoPositionByTimemMainForm(object sender, EventArgs e)
        {
            if (AutoPositionByTime_MainForm != null)
            {
                AutoPositionByTime_MainForm(sender, e); //回调表单主窗体的方法
            }
        }

        /// <summary>
        /// 打开日历
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void tsmiShowCalendar_Click(object sender, EventArgs e)
        {
            //打开日期选择控件
            ShowCalendar();
        }

        /// <summary>
        /// 日期格式选择改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void tsmiDate_Click(object sender, EventArgs e)
        {
            _IsSetValue = true;

            ToolStripMenuItem me = (ToolStripMenuItem)sender;

            //根据选择的菜单，对日期格式化设置
            if (me.Name.EndsWith(nameof(EntXmlModel.DateFormat)))
            {
                //区分：yyyy年MM月dd日 ，yyyy-MM-dd
                if (IsDateYMDChinese(_YMD_Default))
                {
                    _YMD = "yyyy年MM月dd日";
                }
                else
                {
                    _YMD = "yyyy-MM-dd";
                }
            }
            else if (me.Name.EndsWith("MD"))
            {
                if (_YMD_Default.Contains("MM"))
                {
                    if (_YMD_Default.Contains("dd"))
                    {
                        if (IsDateYMDChinese(_YMD_Default))
                        {
                            _YMD = "MM月dd日";
                        }
                        else
                        {
                            _YMD = "MM-dd";
                        }
                    }
                    else
                    {
                        if (IsDateYMDChinese(_YMD_Default))
                        {
                            _YMD = "MM月d日";
                        }
                        else
                        {
                            _YMD = "MM-d";
                        }
                    }
                }
                else
                {
                    if (_YMD_Default.Contains("dd"))
                    {
                        if (IsDateYMDChinese(_YMD_Default))
                        {
                            _YMD = "M月dd日";
                        }
                        else
                        {
                            _YMD = "M-dd";
                        }
                    }
                    else
                    {
                        if (IsDateYMDChinese(_YMD_Default))
                        {
                            _YMD = "M月d日";
                        }
                        else
                        {
                            _YMD = "M-d";
                        }
                    }
                }
            }
            else
            {
                if (_YMD_Default.Contains("dd"))
                {
                    //if (IsDateYMDChinese(_YMD_Default))
                    if (_YMD_Default.Contains("日"))
                    {
                        _YMD = "dd日";
                    }
                    else
                    {
                        _YMD = "dd";
                    }
                }
                else
                {
                    //if (IsDateYMDChinese(_YMD_Default))
                    if (_YMD_Default.Contains("日"))
                    {
                        _YMD = "d日";
                    }
                    else
                    {
                        _YMD = "d";
                    }
                }
            }

            HorizontalAlignment temp = this.SelectionAlignment;

            this.Text = GetShowDate();

            this.SelectionAlignment = temp;

            _IsSetValue = false;
        }

        /// <summary>
        /// 判断设置的日期格式串，是否中文
        /// yyyy年MM月dd日
        /// yyyy-MM-dd
        /// </summary>
        /// <param name="ymdPara"></param>
        /// <returns></returns>
        private bool IsDateYMDChinese(string ymdPara)
        {

            if (ymdPara.Contains("年") || ymdPara.Contains("月") || ymdPara.Contains("日"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 光标离开，设置成对应的格式
        /// </summary>
        public void LeaveShowDate()
        {
            RichTextBoxExtended_Leave(null, null);
        }

        private bool _NeedUpdateLeave = true;
        private void RichTextBoxExtended_Leave(object sender, EventArgs e)
        {
            if (this.Name.StartsWith("日期") && _NeedUpdateLeave) // && !this._IsTable
            {
                string showText = GetShowDate();

                if (showText != this.Text)
                {
                    _IsSetValue = true;
                    HorizontalAlignment temp = this.SelectionAlignment;
                    this.Text = showText;
                    this.SelectAll();
                    this.SelectionAlignment = temp;
                    _IsSetValue = false;
                }
            }

            ////如果有双红线，在全选的情况下光标离开，会不能显示
            //if (this._HaveSingleLine || this._HaveDoubleLine)
            //{
            //    this.Select(0, 0);
            //}

            //双下划线绘制
            PrintCusLines();
        }

        private void monthCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            try
            {
                this.Focus(); //先置入光标，因为现在已经失去焦点了；先置入，离开会立刻刷新格式的

                this._FullDateText = string.Format("{0:yyyy-MM-dd}", _Calendar.SelectionStart);
                this.Text = string.Format("{0:yyyy-MM-dd}", _Calendar.SelectionStart);

                //this._FullDateText = string.Format("{0:yyyy-M-dd}", _Calendar.SelectionStart);
                //this.Text = string.Format("{0:yyyy-M-dd}", _Calendar.SelectionStart);

                //_Calendar.Dispose();
                _Calendar.Hide();
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 关闭日历选择框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monthCalendar_Leave(object sender, EventArgs e)
        {
            try
            {
                //_Calendar.Dispose();
                _Calendar.Hide();

                if (!this.Focused)
                {
                    RichTextBoxExtended_Leave(null, null);
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 双击打开日历，也可以右键打开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RichTextBoxExtended_DoubleClick(object sender, EventArgs e)
        {


            if (!this.ReadOnly && !string.IsNullOrEmpty(this.Text)) //空的时候，复制系统时间，不弹出。
            {
                //打开日期选择控件
                ShowCalendar();
            }
        }

        /// <summary>
        /// 显示日历选择控件
        /// </summary>
        private void ShowCalendar()
        {
            try
            {
                if (this.Name.StartsWith("日期")) // && !this._IsTable
                {
                    if (_Calendar == null || _Calendar.IsDisposed)
                    {
                        _Calendar = new MonthCalendar();
                        _Calendar.Name = "新表单日期选择框monthCalendar";
                        this._Calendar.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.monthCalendar_DateSelected);
                        this._Calendar.Leave += new System.EventHandler(this.monthCalendar_Leave);
                        this.Parent.Controls.Add(_Calendar);
                    }
                    else
                    {
                        if (this.Parent.Controls.Contains(_Calendar))
                        {
                            this.Parent.Controls.Add(_Calendar);
                        }

                        _Calendar.Show();
                    }

                    _Calendar.TodayDate = DateTime.Now;

                    if (DateHelp.IsDate(this.Text))
                    {
                        //_Calendar.SelectionStart = DateTime.Parse(this.Text + " 12:00"); //如果日期那么就报错了
                        if (this.Text.Contains(" ") || this.Text.Contains(":"))
                        {
                            _Calendar.SelectionStart = DateTime.Parse(this.Text);
                        }
                        else
                        {
                            _Calendar.SelectionStart = DateTime.Parse(this.Text + " 12:00");
                        }

                        _Calendar.SelectionEnd = _Calendar.SelectionStart;
                    }
                    else
                    {
                        _Calendar.SelectionStart = DateTime.Now;
                        _Calendar.SelectionEnd = _Calendar.SelectionStart;
                    }

                    if (this.Parent != null)
                    {
                        _Calendar.BringToFront();

                        Point loc = this.Location;

                        if (this.Location.Y + _Calendar.Height > this.Parent.Height)
                        {
                            loc = new Point(this.Location.X, this.Parent.Height - _Calendar.Height);
                        }

                        if (this.Location.X + _Calendar.Width > this.Parent.Width)
                        {
                            loc = new Point(this.Parent.Width - _Calendar.Width, loc.Y);
                        }

                        _Calendar.Location = loc;

                        _NeedUpdateLeave = false;
                        _Calendar.Focus(); // 会导致输入框光标离开
                        _NeedUpdateLeave = true;
                        //_Calendar.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 按键处理，目前只处理Ctrl + Z
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RichTextBoxExtended_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers.CompareTo(Keys.Control) == 0 && e.KeyCode == Keys.Z)
            {
                this.Undo();
            }
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
            //走自己的粘帖方法：单行输入框，那么取消换行符
            //if (keyData == (Keys.Control & Keys.V))
            if (keyData == (Keys)Shortcut.CtrlV) // 快捷键 Ctrl+V 粘贴操作
            {
                menuItem8_Click(null, null); //走自己的复制粘贴方法：如果是单行输入框，那么执行取消换行符
                return true;//如果想要焦点保持在原控件则返回true，别且不会再执行系统的这个按键处理，不会执行两次

            }

            if (_IsTable)
            {
                if (keyData == Keys.Tab) // || keyData == Keys.Shift
                {
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    base.OnKeyDown(e); //让CellsRtbeSetFocus_KeyDown可以捕捉到Tab事件

                    return true;//如果想要焦点保持在原控件则返回true
                }
                else if (keyData == (Keys.Tab | Keys.Shift)) //  Shift + Tab
                {
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    base.OnKeyDown(e); //让CellsRtbeSetFocus_KeyDown可以捕捉到Tab事件

                    //如果想要焦点保持在原控件则返回true
                    return true;
                }
                //else if (keyData == Keys.Up || keyData == Keys.Down
                //    || (keyData == Keys.Left && this.SelectionStart == 0) || (keyData == Keys.Right && this.SelectionStart == this.Text.Length)) //   上下键  ,这里注释掉：切换单元格的时候，就不会不全选了
                //{
                //    KeyEventArgs e = new KeyEventArgs(keyData);
                //    base.OnKeyDown(e); //让CellsRtbeSetFocus_KeyDown可以捕捉到Tab事件

                //    //如果想要焦点保持在原控件则返回true
                //    return true;
                //}

                //else if (this.Multiline &&
                //    ((keyData == Keys.Up  || keyData == Keys.Down))) //   上下键  ,这里注释掉：切换单元格的时候，就不会不全选了
                //{

                //    int rowIndex = this.GetLineFromCharIndex(this.SelectionStart);//获得光标所在行的索引值

                //    if ((keyData == Keys.Up && rowIndex == 0) || (keyData == Keys.Down && rowIndex == 0))
                //    {
                //        KeyEventArgs e = new KeyEventArgs(keyData);
                //        base.OnKeyDown(e); //让CellsRtbeSetFocus_KeyDown可以捕捉到Tab事件

                //        //如果想要焦点保持在原控件则返回true
                //        return true;
                //    }
                //    else
                //    {
                //        return false;
                //    }
                //}
                else if (((keyData == Keys.Up || keyData == Keys.Down) && this.SelectedText.Length == this.Text.Length)
                    || (keyData == Keys.Left && this.SelectionStart == 0) || (keyData == Keys.Right && this.SelectionStart == this.Text.Length)) //   上下键  ,这里注释掉：切换单元格的时候，就不会不全选了
                {
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    base.OnKeyDown(e); //让CellsRtbeSetFocus_KeyDown可以捕捉到Tab事件

                    //如果想要焦点保持在原控件则返回true
                    return true;
                }
                else
                {
                    return false; // 这里如果为true，那么不能键入文字了
                                  //return base.ProcessCmdKey(ref msg, keyData);   //其他键按默认处理
                }
            }
            else
            {
                //非表格输入框，按下回车和Tab键一样的效果
                if (!this.Multiline)
                {
                    if (keyData == Keys.Enter)
                    {
                        //光标迁移到下一个控件，和Tab键一样的效果
                        //SendKeys.Send("\t");
                        SendKeys.Send("{TAB}"); //Tab
                                                //SendKeys.Send("^ +{TAB}"); //Shift + Tab
                    }
                }

                //if (keyData == Keys.Tab)
                //{
                //    IMEClass.SetHalfShape(this.Handle); //这样输入不了全角了
                //}

                //非表格的，不用处理
                //KeyEventArgs e = new KeyEventArgs(keyData);
                //base.OnKeyDown(e); 

                //base.ProcessCmdKey(ref msg, keyData);
                return false; //如果想要焦点保持在原控件则返回true
            }


            ////KeyEventArgs e = new KeyEventArgs(keyData);
            ////base.OnKeyDown(e); //让CellsRtbeSetFocus_KeyDown可以捕捉到Tab事件

            ////return false;
        }


        #region //Rich自带的上下标操作----------------------------
        // New, version 1.1
        public void SetSuperScript(bool bSet)
        {
            Struct.CHARFORMAT cf = this.CharFormat;

            if (bSet)
            {
                cf.dwMask |= Consts.CFM_SUPERSCRIPT;
                cf.dwEffects |= Consts.CFE_SUPERSCRIPT;
            }
            else
            {
                cf.dwEffects &= ~Consts.CFE_SUPERSCRIPT;
            }

            this.CharFormat = cf;
        }

        public void SetSubScript(bool bSet)
        {
            Struct.CHARFORMAT cf = this.CharFormat;

            if (bSet)
            {
                cf.dwMask |= Consts.CFM_SUBSCRIPT;
                cf.dwEffects |= Consts.CFE_SUBSCRIPT;
            }
            else
            {
                cf.dwEffects &= ~Consts.CFE_SUBSCRIPT;
            }

            this.CharFormat = cf;
        }

        public bool IsSuperScript()
        {
            Struct.CHARFORMAT cf = this.CharFormat;
            return ((cf.dwEffects & Consts.CFE_SUPERSCRIPT) == Consts.CFE_SUPERSCRIPT);
        }

        public bool IsSubScript()
        {
            Struct.CHARFORMAT cf = this.CharFormat;
            return ((cf.dwEffects & Consts.CFE_SUBSCRIPT) == Consts.CFE_SUBSCRIPT);
        }

        public Struct.CHARFORMAT CharFormat
        {
            get
            {
                Struct.CHARFORMAT cf = new Struct.CHARFORMAT();
                cf.cbSize = Marshal.SizeOf(cf);

                // Get the alignment.
                Win32Api.SendMessage(new HandleRef(this, Handle),
                    Consts.EM_GETCHARFORMAT,
                    Consts.SCF_SELECTION, ref cf);

                return cf;
            }

            set
            {
                Struct.CHARFORMAT cf = value;
                cf.cbSize = Marshal.SizeOf(cf);

                // Set the alignment.
                Win32Api.SendMessage(new HandleRef(this, Handle),
                    Consts.EM_SETCHARFORMAT,
                    Consts.SCF_SELECTION, ref cf);
            }
        }

        public Struct.CHARFORMAT DefaultCharFormat
        {
            get
            {
                Struct.CHARFORMAT cf = new Struct.CHARFORMAT();
                cf.cbSize = Marshal.SizeOf(cf);

                // Get the alignment.
                Win32Api.SendMessage(new HandleRef(this, Handle),
                    Consts.EM_GETCHARFORMAT,
                    Consts.SCF_ALL, ref cf);

                return cf;
            }

            set
            {
                Struct.CHARFORMAT cf = value;
                cf.cbSize = Marshal.SizeOf(cf);

                // Set the alignment.
                Win32Api.SendMessage(new HandleRef(this, Handle),
                    Consts.EM_SETCHARFORMAT,
                    Consts.SCF_ALL, ref cf);
            }
        }

        public Struct.PARAFORMAT2 ParaFormat
        {
            get
            {
                Struct.PARAFORMAT2 pf = new Struct.PARAFORMAT2();
                pf.cbSize = Marshal.SizeOf(pf);

                // Get the alignment.
                Win32Api.SendMessage(new HandleRef(this, Handle),
                    Consts.EM_GETPARAFORMAT,
                    Consts.SCF_SELECTION, ref pf);

                return pf;
            }

            set
            {
                Struct.PARAFORMAT2 pf = value;
                pf.cbSize = Marshal.SizeOf(pf);

                // Set the alignment.
                Win32Api.SendMessage(new HandleRef(this, Handle),
                    Consts.EM_SETPARAFORMAT,
                    Consts.SCF_SELECTION, ref pf);
            }
        }

        public Struct.PARAFORMAT2 DefaultParaFormat
        {
            get
            {
                Struct.PARAFORMAT2 pf = new Struct.PARAFORMAT2();
                pf.cbSize = Marshal.SizeOf(pf);

                // Get the alignment.
                Win32Api.SendMessage(new HandleRef(this, Handle),
                    Consts.EM_GETPARAFORMAT,
                    Consts.SCF_ALL, ref pf);

                return pf;
            }

            set
            {
                Struct.PARAFORMAT2 pf = value;
                pf.cbSize = Marshal.SizeOf(pf);

                // Set the alignment.
                Win32Api.SendMessage(new HandleRef(this, Handle),
                    Consts.EM_SETPARAFORMAT,
                    Consts.SCF_ALL, ref pf);
            }
        }

        #region COLORREF helper functions

        // convert COLORREF to Color
        private Color GetColor(int crColor)
        {
            byte r = (byte)(crColor);
            byte g = (byte)(crColor >> 8);
            byte b = (byte)(crColor >> 16);

            return Color.FromArgb(r, g, b);
        }

        // convert COLORREF to Color
        private int GetCOLORREF(int r, int g, int b)
        {
            int r2 = r;
            int g2 = (g << 8);
            int b2 = (b << 16);

            int result = r2 | g2 | b2;

            return result;
        }

        private int GetCOLORREF(Color color)
        {
            int r = color.R;
            int g = color.G;
            int b = color.B;

            return GetCOLORREF(r, g, b);
        }
        #endregion



        private int updating = 0;
        private int oldEventMask = 0;

        /// <summary>
        /// Maintains performance while updating.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is recommended to call this method before doing
        /// any major updates that you do not wish the user to
        /// see. Remember to call EndUpdate when you are finished
        /// with the update. Nested calls are supported.
        /// </para>
        /// <para>
        /// Calling this method will prevent redrawing. It will
        /// also setup the event mask of the underlying richedit
        /// control so that no events are sent.
        /// </para>
        /// </remarks>

        public void BeginUpdate()
        {
            // Deal with nested calls.
            ++updating;

            if (updating > 1)
                return;

            // Prevent the control from raising any events.
            oldEventMask = Win32Api.SendMessage(new HandleRef(this, Handle),
                Consts.EM_SETEVENTMASK, 0, 0);

            // Prevent the control from redrawing itself.
            Win32Api.SendMessage(new HandleRef(this, Handle),
                Consts.WM_SETREDRAW, 0, 0);
        }

        /// <summary>
        /// Resumes drawing and event handling.
        /// </summary>
        /// <remarks>
        /// This method should be called every time a call is made
        /// made to BeginUpdate. It resets the event mask to it's
        /// original value and enables redrawing of the control.
        /// </remarks>
        public void EndUpdate()
        {
            // Deal with nested calls.
            --updating;

            if (updating > 0)
                return;

            // Allow the control to redraw itself.
            Win32Api.SendMessage(new HandleRef(this, Handle),
                Consts.WM_SETREDRAW, 1, 0);

            // Allow the control to raise event messages.
            Win32Api.SendMessage(new HandleRef(this, Handle),
                Consts.EM_SETEVENTMASK, 0, oldEventMask);
        }

        /// <summary>
        /// Returns true when the control is performing some 
        /// internal updates, specially when is reading or writing
        /// HTML text
        /// </summary>
        public bool InternalUpdating
        {
            get
            {
                return (updating != 0);
            }
        }

        #endregion//Rich自带的上下标操作----------------------------

        #region 将Rtf转换为html "Get HTML text"

        // format states
        private enum ctformatStates
        {
            nctNone = 0, // none format applied
            nctNew = 1, // new format
            nctContinue = 2, // continue with previous format
            nctReset = 3 // reset format (close this tag)
        }

        private enum uMyREType
        {
            U_MYRE_TYPE_TAG,
            U_MYRE_TYPE_EMO,
            U_MYRE_TYPE_ENTITY,
        }

        private struct cMyREFormat
        {
            public uMyREType nType;
            public int nLen;
            public int nPos;
            public string strValue;
        }

        public string GetHTML(bool bHTML, bool bParaFormat)
        {
            //------------------------
            // working variables
            Struct.CHARFORMAT cf;
            Struct.PARAFORMAT2 pf;

            ctformatStates bold = ctformatStates.nctNone;
            ctformatStates bitalic = ctformatStates.nctNone;
            ctformatStates bstrikeout = ctformatStates.nctNone;
            ctformatStates bunderline = ctformatStates.nctNone;
            ctformatStates super = ctformatStates.nctNone;
            ctformatStates sub = ctformatStates.nctNone;

            ctformatStates bacenter = ctformatStates.nctNone;
            ctformatStates baleft = ctformatStates.nctNone;
            ctformatStates baright = ctformatStates.nctNone;
            ctformatStates bnumbering = ctformatStates.nctNone;

            string strFont = "";
            Int32 crFont = 0;
            Color color = new Color();
            int yHeight = 0;
            int i = 0;
            //-------------------------

            //-------------------------
            // to store formatting
            ArrayList colFormat = new ArrayList();

            cMyREFormat mfr;
            //-------------------------

            //-------------------------
            // ok, lets go
            int nStart, nEnd;
            string strHTML = "";

            this.HideSelection = true;
            this.BeginUpdate();

            nStart = this.SelectionStart;
            nEnd = this.SelectionLength;

            try
            {
                //--------------------------------
                // replace entities
                if (bHTML)
                {

                    char[] ch = { '&', '<', '>', '"', '\'' };
                    string[] strreplace = { "&amp;", "&lt;", "&gt;", "&quot;", "&apos;" };

                    for (i = 0; i < ch.Length; i++)
                    {
                        char[] ch2 = { ch[i] };

                        int n = this.Find(ch2, 0);
                        while (n != -1)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = n;
                            mfr.nLen = 1;
                            mfr.nType = uMyREType.U_MYRE_TYPE_ENTITY;
                            mfr.strValue = strreplace[i];

                            colFormat.Add(mfr);

                            n = this.Find(ch2, n + 1);
                        }
                    }
                }
                //--------------------------------

                string strT = "";

                int k = this.TextLength;
                char[] chtrim = { ' ', '\x0000' };

                //--------------------------------
                // this is an inefficient method to get text format
                // but RichTextBox doesn't provide another method to
                // get something like an array of charformat and paraformat
                //--------------------------------
                for (i = 0; i < k; i++)
                {
                    // select one character
                    this.Select(i, 1);
                    string strChar = this.SelectedText;

                    if (bHTML)
                    {
                        //-------------------------
                        // get format for this character
                        cf = this.CharFormat;
                        pf = this.ParaFormat;

                        string strfname = new string(cf.szFaceName);
                        strfname = strfname.Trim(chtrim);
                        //-------------------------


                        //-------------------------
                        // new font format ?
                        if ((strFont != strfname) || (crFont != cf.crTextColor) || (yHeight != cf.yHeight))
                        {
                            if (strFont != "")
                            {
                                // close previous <font> tag

                                mfr = new cMyREFormat();

                                mfr.nPos = i;
                                mfr.nLen = 0;
                                mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                                mfr.strValue = "</font>";

                                colFormat.Add(mfr);
                            }

                            //-------------------------
                            // save this for cache
                            strFont = strfname;
                            crFont = cf.crTextColor;
                            yHeight = cf.yHeight;
                            //-------------------------

                            //-------------------------
                            // font size should be translate to 
                            // html size (Approximately) 
                            //字体大小，但是9号字体 -》1，保存后就会变小，要10号-》2才正好
                            int fsize = yHeight / (20 * 5);

                            if (fsize == 1)
                            {
                                fsize = 2; //1号字体太小，变成2号的
                            }
                            //-------------------------

                            //-------------------------
                            // color object from COLORREF
                            color = GetColor(crFont);
                            //-------------------------

                            //-------------------------
                            // add <font> tag
                            mfr = new cMyREFormat();

                            string strcolor = string.Concat("#", (color.ToArgb() & 0x00FFFFFF).ToString("X6"));

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "<font face=\"" + strFont + "\" color=\"" + strcolor + "\" size=\"" + fsize + "\">";

                            colFormat.Add(mfr);
                            //-------------------------
                        }

                        //-------------------------
                        // are we in another line ?
                        if ((strChar == "\r") || (strChar == "\n"))
                        {
                            // yes?
                            // then, we need to reset paragraph format
                            // and character format
                            if (bParaFormat)
                            {
                                bnumbering = ctformatStates.nctNone;
                                baleft = ctformatStates.nctNone;
                                baright = ctformatStates.nctNone;
                                bacenter = ctformatStates.nctNone;
                            }

                            // close previous tags

                            // is italic? => close it
                            if (bitalic != ctformatStates.nctNone)
                            {
                                mfr = new cMyREFormat();

                                mfr.nPos = i;
                                mfr.nLen = 0;
                                mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                                mfr.strValue = "</i>";

                                colFormat.Add(mfr);

                                bitalic = ctformatStates.nctNone;
                            }

                            // is bold? => close it
                            if (bold != ctformatStates.nctNone)
                            {
                                mfr = new cMyREFormat();

                                mfr.nPos = i;
                                mfr.nLen = 0;
                                mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                                mfr.strValue = "</b>";

                                colFormat.Add(mfr);

                                bold = ctformatStates.nctNone;
                            }

                            // is underline? => close it
                            if (bunderline != ctformatStates.nctNone)
                            {
                                mfr = new cMyREFormat();

                                mfr.nPos = i;
                                mfr.nLen = 0;
                                mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                                mfr.strValue = "</u>";

                                colFormat.Add(mfr);

                                bunderline = ctformatStates.nctNone;
                            }

                            // is strikeout? => close it
                            if (bstrikeout != ctformatStates.nctNone)
                            {
                                mfr = new cMyREFormat();

                                mfr.nPos = i;
                                mfr.nLen = 0;
                                mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                                mfr.strValue = "</s>";

                                colFormat.Add(mfr);

                                bstrikeout = ctformatStates.nctNone;
                            }

                            // is super? => close it
                            if (super != ctformatStates.nctNone)
                            {
                                mfr = new cMyREFormat();

                                mfr.nPos = i;
                                mfr.nLen = 0;
                                mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                                mfr.strValue = "</sup>";

                                colFormat.Add(mfr);

                                super = ctformatStates.nctNone;
                            }

                            // is sub? => close it
                            if (sub != ctformatStates.nctNone)
                            {
                                mfr = new cMyREFormat();

                                mfr.nPos = i;
                                mfr.nLen = 0;
                                mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                                mfr.strValue = "</sub>";

                                colFormat.Add(mfr);

                                sub = ctformatStates.nctNone;
                            }
                        }

                        // now, process the paragraph format,
                        // managing states: none, new, continue {with previous}, reset
                        if (bParaFormat)
                        {
                            // align to center?
                            if (pf.wAlignment == Consts.PFA_CENTER)
                            {
                                if (bacenter == ctformatStates.nctNone)
                                    bacenter = ctformatStates.nctNew;
                                else
                                    bacenter = ctformatStates.nctContinue;
                            }
                            else
                            {
                                if (bacenter != ctformatStates.nctNone)
                                    bacenter = ctformatStates.nctReset;
                            }

                            if (bacenter == ctformatStates.nctNew)
                            {
                                mfr = new cMyREFormat();

                                mfr.nPos = i;
                                mfr.nLen = 0;
                                mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                                mfr.strValue = "<p align=\"center\">";

                                colFormat.Add(mfr);
                            }
                            else if (bacenter == ctformatStates.nctReset)
                                bacenter = ctformatStates.nctNone;
                            //---------------------

                            //---------------------
                            // align to left ?
                            if (pf.wAlignment == Consts.PFA_LEFT)
                            {
                                if (baleft == ctformatStates.nctNone)
                                    baleft = ctformatStates.nctNew;
                                else
                                    baleft = ctformatStates.nctContinue;
                            }
                            else
                            {
                                if (baleft != ctformatStates.nctNone)
                                    baleft = ctformatStates.nctReset;
                            }

                            if (baleft == ctformatStates.nctNew)
                            {
                                mfr = new cMyREFormat();

                                mfr.nPos = i;
                                mfr.nLen = 0;
                                mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                                mfr.strValue = "<p align=\"left\">";

                                colFormat.Add(mfr);
                            }
                            else if (baleft == ctformatStates.nctReset)
                                baleft = ctformatStates.nctNone;
                            //---------------------

                            //---------------------
                            // align to right ?
                            if (pf.wAlignment == Consts.PFA_RIGHT)
                            {
                                if (baright == ctformatStates.nctNone)
                                    baright = ctformatStates.nctNew;
                                else
                                    baright = ctformatStates.nctContinue;
                            }
                            else
                            {
                                if (baright != ctformatStates.nctNone)
                                    baright = ctformatStates.nctReset;
                            }

                            if (baright == ctformatStates.nctNew)
                            {
                                mfr = new cMyREFormat();

                                mfr.nPos = i;
                                mfr.nLen = 0;
                                mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                                mfr.strValue = "<p align=\"right\">";

                                colFormat.Add(mfr);
                            }
                            else if (baright == ctformatStates.nctReset)
                                baright = ctformatStates.nctNone;
                            //---------------------

                            //---------------------
                            // bullet ?
                            if (pf.wNumbering == Consts.PFN_BULLET)
                            {
                                if (bnumbering == ctformatStates.nctNone)
                                    bnumbering = ctformatStates.nctNew;
                                else
                                    bnumbering = ctformatStates.nctContinue;
                            }
                            else
                            {
                                if (bnumbering != ctformatStates.nctNone)
                                    bnumbering = ctformatStates.nctReset;
                            }

                            if (bnumbering == ctformatStates.nctNew)
                            {
                                mfr = new cMyREFormat();

                                mfr.nPos = i;
                                mfr.nLen = 0;
                                mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                                mfr.strValue = "<li>";

                                colFormat.Add(mfr);
                            }
                            else if (bnumbering == ctformatStates.nctReset)
                                bnumbering = ctformatStates.nctNone;
                            //---------------------
                        }

                        //---------------------
                        // bold ?
                        if ((cf.dwEffects & Consts.CFE_BOLD) == Consts.CFE_BOLD)
                        {
                            if (bold == ctformatStates.nctNone)
                                bold = ctformatStates.nctNew;
                            else
                                bold = ctformatStates.nctContinue;
                        }
                        else
                        {
                            if (bold != ctformatStates.nctNone)
                                bold = ctformatStates.nctReset;
                        }

                        if (bold == ctformatStates.nctNew)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "<b>";

                            colFormat.Add(mfr);
                        }
                        else if (bold == ctformatStates.nctReset)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "</b>";

                            colFormat.Add(mfr);

                            bold = ctformatStates.nctNone;
                        }
                        //---------------------

                        //---------------------
                        // Italic
                        if ((cf.dwEffects & Consts.CFE_ITALIC) == Consts.CFE_ITALIC)
                        {
                            if (bitalic == ctformatStates.nctNone)
                                bitalic = ctformatStates.nctNew;
                            else
                                bitalic = ctformatStates.nctContinue;
                        }
                        else
                        {
                            if (bitalic != ctformatStates.nctNone)
                                bitalic = ctformatStates.nctReset;
                        }

                        if (bitalic == ctformatStates.nctNew)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "<i>";

                            colFormat.Add(mfr);
                        }
                        else if (bitalic == ctformatStates.nctReset)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "</i>";

                            colFormat.Add(mfr);

                            bitalic = ctformatStates.nctNone;
                        }
                        //---------------------

                        //---------------------
                        // strikeout
                        if ((cf.dwEffects & Consts.CFM_STRIKEOUT) == Consts.CFM_STRIKEOUT)
                        {
                            if (bstrikeout == ctformatStates.nctNone)
                                bstrikeout = ctformatStates.nctNew;
                            else
                                bstrikeout = ctformatStates.nctContinue;
                        }
                        else
                        {
                            if (bstrikeout != ctformatStates.nctNone)
                                bstrikeout = ctformatStates.nctReset;
                        }

                        if (bstrikeout == ctformatStates.nctNew)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "<s>";

                            colFormat.Add(mfr);
                        }
                        else if (bstrikeout == ctformatStates.nctReset)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "</s>";

                            colFormat.Add(mfr);

                            bstrikeout = ctformatStates.nctNone;
                        }
                        //---------------------

                        //---------------------
                        // underline ?
                        if ((cf.dwEffects & Consts.CFE_UNDERLINE) == Consts.CFE_UNDERLINE)
                        {
                            if (bunderline == ctformatStates.nctNone)
                                bunderline = ctformatStates.nctNew;
                            else
                                bunderline = ctformatStates.nctContinue;
                        }
                        else
                        {
                            if (bunderline != ctformatStates.nctNone)
                                bunderline = ctformatStates.nctReset;
                        }

                        if (bunderline == ctformatStates.nctNew)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "<u>";

                            colFormat.Add(mfr);
                        }
                        else if (bunderline == ctformatStates.nctReset)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "</u>";

                            colFormat.Add(mfr);

                            bunderline = ctformatStates.nctNone;
                        }
                        //---------------------

                        //---------------------
                        // superscript ?
                        if ((cf.dwEffects & Consts.CFE_SUPERSCRIPT) == Consts.CFE_SUPERSCRIPT)
                        {
                            if (super == ctformatStates.nctNone)
                                super = ctformatStates.nctNew;
                            else
                                super = ctformatStates.nctContinue;
                        }
                        else
                        {
                            if (super != ctformatStates.nctNone)
                                super = ctformatStates.nctReset;
                        }

                        if (super == ctformatStates.nctNew)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "<sup>";

                            colFormat.Add(mfr);
                        }
                        else if (super == ctformatStates.nctReset)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "</sup>";

                            colFormat.Add(mfr);

                            super = ctformatStates.nctNone;
                        }
                        //---------------------

                        //---------------------
                        // subscript ?
                        if ((cf.dwEffects & Consts.CFE_SUBSCRIPT) == Consts.CFE_SUBSCRIPT)
                        {
                            if (sub == ctformatStates.nctNone)
                                sub = ctformatStates.nctNew;
                            else
                                sub = ctformatStates.nctContinue;
                        }
                        else
                        {
                            if (sub != ctformatStates.nctNone)
                                sub = ctformatStates.nctReset;
                        }

                        if (sub == ctformatStates.nctNew)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "<sub>";

                            colFormat.Add(mfr);
                        }
                        else if (sub == ctformatStates.nctReset)
                        {
                            mfr = new cMyREFormat();

                            mfr.nPos = i;
                            mfr.nLen = 0;
                            mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                            mfr.strValue = "</sub>";

                            colFormat.Add(mfr);

                            sub = ctformatStates.nctNone;
                        }
                        //---------------------
                    }

                    strT += strChar;
                }

                if (bHTML)
                {
                    // close pending tags
                    if (bold != ctformatStates.nctNone)
                    {
                        mfr = new cMyREFormat();

                        mfr.nPos = i;
                        mfr.nLen = 0;
                        mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                        mfr.strValue = "</b>";

                        colFormat.Add(mfr);
                        //strT += "</b>";
                    }

                    if (bitalic != ctformatStates.nctNone)
                    {
                        mfr = new cMyREFormat();

                        mfr.nPos = i;
                        mfr.nLen = 0;
                        mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                        mfr.strValue = "</i>";

                        colFormat.Add(mfr);
                        //strT += "</i>";
                    }

                    if (bstrikeout != ctformatStates.nctNone)
                    {
                        mfr = new cMyREFormat();

                        mfr.nPos = i;
                        mfr.nLen = 0;
                        mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                        mfr.strValue = "</s>";

                        colFormat.Add(mfr);

                        //strT += "</s>";
                    }

                    if (bunderline != ctformatStates.nctNone)
                    {
                        mfr = new cMyREFormat();

                        mfr.nPos = i;
                        mfr.nLen = 0;
                        mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                        mfr.strValue = "</u>";

                        colFormat.Add(mfr);

                        //strT += "</u>";
                    }

                    if (super != ctformatStates.nctNone)
                    {
                        mfr = new cMyREFormat();

                        mfr.nPos = i;
                        mfr.nLen = 0;
                        mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                        mfr.strValue = "</sup>";

                        colFormat.Add(mfr);
                        //strT += "</sup>";
                    }

                    if (sub != ctformatStates.nctNone)
                    {
                        mfr = new cMyREFormat();

                        mfr.nPos = i;
                        mfr.nLen = 0;
                        mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                        mfr.strValue = "</sub>";

                        colFormat.Add(mfr);
                        //strT += "</sub>";
                    }

                    if (strFont != "")
                    {
                        // close pending font format
                        mfr = new cMyREFormat();

                        mfr.nPos = i;
                        mfr.nLen = 0;
                        mfr.nType = uMyREType.U_MYRE_TYPE_TAG;
                        mfr.strValue = "</font>";

                        colFormat.Add(mfr);
                    }
                }

                //--------------------------
                // now, reorder the formatting array
                k = colFormat.Count;
                for (i = 0; i < k - 1; i++)
                {
                    for (int j = i + 1; j < k; j++)
                    {
                        mfr = (cMyREFormat)colFormat[i];
                        cMyREFormat mfr2 = (cMyREFormat)colFormat[j];

                        if (mfr2.nPos < mfr.nPos)
                        {
                            colFormat.RemoveAt(j);
                            colFormat.Insert(i, mfr2);
                            j--;
                        }
                        else if ((mfr2.nPos == mfr.nPos) && (mfr2.nLen < mfr.nLen))
                        {
                            colFormat.RemoveAt(j);
                            colFormat.Insert(i, mfr2);
                            j--;
                        }
                    }
                }
                //--------------------------


                //--------------------------
                // apply format by replacing and inserting HTML tags
                // stored in the Format Array
                int nAcum = 0;
                for (i = 0; i < k; i++)
                {
                    mfr = (cMyREFormat)colFormat[i];

                    strHTML += strT.Substring(nAcum, mfr.nPos - nAcum) + mfr.strValue;
                    nAcum = mfr.nPos + mfr.nLen;
                }

                if (nAcum < strT.Length)
                    strHTML += strT.Substring(nAcum);
                //--------------------------
            }
            catch
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                //--------------------------
                // finish, restore
                this.SelectionStart = nStart;
                this.SelectionLength = nEnd;

                this.EndUpdate();
                this.HideSelection = false;
                //--------------------------
            }

            return strHTML;
        }
        #endregion 将Rtf转换为html

        #region "Add HTML text" 将html转换为rtf
        public void AddHTML(string strHTML)
        {
            //this.SelectionFont = new Font(this.Font.FontFamily.Name, this.Font.Size, this.SelectionFont.Style);

            Struct.CHARFORMAT cf;
            Struct.PARAFORMAT2 pf;

            cf = this.DefaultCharFormat; // to apply character formatting
            pf = this.DefaultParaFormat; // to apply paragraph formatting

            char[] chtrim = { ' ', '\x0000' };

            this.HideSelection = true;
            this.BeginUpdate();

            try
            {
                // process text
                while (strHTML.Length > 0)
                {
                    string strData = strHTML;

                reinit:

                    // looking for start tags
                    int nStart = strHTML.IndexOf('<');
                    if (nStart >= 0)
                    {
                        if (nStart > 0)
                        {
                            // tag is not the first character, so
                            // we need to add text to control and continue
                            // looking for tags at the begining of the text
                            strData = strHTML.Substring(0, nStart);
                            strHTML = strHTML.Substring(nStart);
                        }
                        else
                        {
                            // ok, get tag value
                            int nEnd = strHTML.IndexOf('>', nStart);
                            if (nEnd > nStart)
                            {
                                if ((nEnd - nStart) > 0)
                                {
                                    string strTag = strHTML.Substring(nStart, nEnd - nStart + 1);
                                    strTag = strTag.ToLower();

                                    if (strTag == "<b>")
                                    {
                                        cf.dwMask |= Consts.CFM_WEIGHT | Consts.CFM_BOLD;
                                        cf.dwEffects |= Consts.CFE_BOLD;
                                        cf.wWeight = Consts.FW_BOLD;
                                    }
                                    else if (strTag == "<i>")
                                    {
                                        cf.dwMask |= Consts.CFM_ITALIC;
                                        cf.dwEffects |= Consts.CFE_ITALIC;
                                    }
                                    else if (strTag == "<u>")
                                    {
                                        cf.dwMask |= Consts.CFM_UNDERLINE | Consts.CFM_UNDERLINETYPE;
                                        cf.dwEffects |= Consts.CFE_UNDERLINE;
                                        cf.bUnderlineType = Consts.CFU_UNDERLINE;
                                    }
                                    else if (strTag == "<s>")
                                    {
                                        cf.dwMask |= Consts.CFM_STRIKEOUT;
                                        cf.dwEffects |= Consts.CFE_STRIKEOUT;
                                    }
                                    else if (strTag == "<sup>")
                                    {
                                        cf.dwMask |= Consts.CFM_SUPERSCRIPT;
                                        cf.dwEffects |= Consts.CFE_SUPERSCRIPT;
                                    }
                                    else if (strTag == "<sub>")
                                    {
                                        cf.dwMask |= Consts.CFM_SUBSCRIPT;
                                        cf.dwEffects |= Consts.CFE_SUBSCRIPT;
                                    }
                                    else if ((strTag.Length > 2) && (strTag.Substring(0, 2) == "<p"))
                                    {
                                        if (strTag.IndexOf("align=\"left\"") > 0)
                                        {
                                            pf.dwMask |= Consts.PFM_ALIGNMENT;
                                            pf.wAlignment = (short)Consts.PFA_LEFT;
                                        }
                                        else if (strTag.IndexOf("align=\"right\"") > 0)
                                        {
                                            pf.dwMask |= Consts.PFM_ALIGNMENT;
                                            pf.wAlignment = (short)Consts.PFA_RIGHT;
                                        }
                                        else if (strTag.IndexOf("align=\"center\"") > 0)
                                        {
                                            pf.dwMask |= Consts.PFM_ALIGNMENT;
                                            pf.wAlignment = (short)Consts.PFA_CENTER;
                                        }
                                    }
                                    else if ((strTag.Length > 5) && (strTag.Substring(0, 5) == "<font"))
                                    {
                                        string strFont = new string(cf.szFaceName);
                                        strFont = strFont.Trim(chtrim);
                                        int crFont = cf.crTextColor;
                                        int yHeight = cf.yHeight;

                                        int nFace = strTag.IndexOf("face=");
                                        if (nFace > 0)
                                        {
                                            int nFaceEnd = strTag.IndexOf('\"', nFace + 6);
                                            if (nFaceEnd > nFace)
                                                strFont = strTag.Substring(nFace + 6, nFaceEnd - nFace - 6);
                                        }

                                        int nSize = strTag.IndexOf("size=");
                                        if (nSize > 0)
                                        {
                                            int nSizeEnd = strTag.IndexOf('\"', nSize + 6);
                                            if (nSizeEnd > nSize)
                                            {
                                                yHeight = int.Parse(strTag.Substring(nSize + 6, nSizeEnd - nSize - 6));
                                                yHeight *= (20 * 5);
                                            }
                                        }

                                        int nColor = strTag.IndexOf("color=");
                                        if (nColor > 0)
                                        {
                                            int nColorEnd = strTag.IndexOf('\"', nColor + 7);
                                            if (nColorEnd > nColor)
                                            {
                                                if (strTag.Substring(nColor + 7, 1) == "#")
                                                {
                                                    string strCr = strTag.Substring(nColor + 8, nColorEnd - nColor - 8);
                                                    int nCr = Convert.ToInt32(strCr, 16);

                                                    Color color = Color.FromArgb(nCr);

                                                    crFont = GetCOLORREF(color);
                                                }
                                                else
                                                {
                                                    crFont = int.Parse(strTag.Substring(nColor + 7, nColorEnd - nColor - 7));
                                                }
                                            }
                                        }

                                        cf.szFaceName = new char[Consts.LF_FACESIZE];
                                        strFont.CopyTo(0, cf.szFaceName, 0, Math.Min(Consts.LF_FACESIZE - 1, strFont.Length));
                                        //cf.szFaceName = strFont.ToCharArray(0, Math.Min(strFont.Length, LF_FACESIZE));
                                        cf.crTextColor = crFont;
                                        cf.yHeight = yHeight; //cf.yHeight = 180;//cf.yHeight = yHeight;

                                        cf.dwMask |= Consts.CFM_COLOR | Consts.CFM_SIZE | Consts.CFM_FACE;
                                        cf.dwEffects &= ~Consts.CFE_AUTOCOLOR;
                                    }
                                    else if (strTag == "<li>")
                                    {
                                        if (pf.wNumbering != Consts.PFN_BULLET)
                                        {
                                            pf.dwMask |= Consts.PFM_NUMBERING;
                                            pf.wNumbering = (short)Consts.PFN_BULLET;
                                        }
                                    }
                                    else if (strTag == "</b>")
                                    {
                                        cf.dwEffects &= ~Consts.CFE_BOLD;
                                        cf.wWeight = Consts.FW_NORMAL;
                                    }
                                    else if (strTag == "</i>")
                                    {
                                        cf.dwEffects &= ~Consts.CFE_ITALIC;
                                    }
                                    else if (strTag == "</u>")
                                    {
                                        cf.dwEffects &= ~Consts.CFE_UNDERLINE;
                                    }
                                    else if (strTag == "</s>")
                                    {
                                        cf.dwEffects &= ~Consts.CFM_STRIKEOUT;
                                    }
                                    else if (strTag == "</sup>")
                                    {
                                        cf.dwEffects &= ~Consts.CFE_SUPERSCRIPT;
                                    }
                                    else if (strTag == "</sub>")
                                    {
                                        cf.dwEffects &= ~Consts.CFE_SUBSCRIPT;
                                    }
                                    else if (strTag == "</font>")
                                    {
                                    }
                                    else if (strTag == "</p>")
                                    {
                                    }
                                    else if (strTag == "</li>")
                                    {
                                    }

                                    //-------------------------------
                                    // now, remove tag from HTML
                                    int nStart2 = strHTML.IndexOf("<", nEnd + 1);
                                    if (nStart2 > 0)
                                    {
                                        // extract partial data
                                        strData = strHTML.Substring(nEnd + 1, nStart2 - nEnd - 1);
                                        strHTML = strHTML.Substring(nStart2);
                                    }
                                    else
                                    {
                                        // get remain text and finish
                                        if ((nEnd + 1) < strHTML.Length)
                                            strData = strHTML.Substring(nEnd + 1);
                                        else
                                            strData = "";

                                        strHTML = "";
                                    }
                                    //-------------------------------


                                    //-------------------------------
                                    // have we any continuos tag ?
                                    if (strData.Length > 0)
                                    {
                                        // yes, ok, goto to reinit
                                        if (strData[0] == '<')
                                            goto reinit;
                                    }
                                    //-------------------------------
                                }
                                else
                                {
                                    // we have not found any valid tag
                                    strHTML = "";
                                }
                            }
                            else
                            {
                                // we have not found any valid tag
                                strHTML = "";
                            }
                        }
                    }
                    else
                    {
                        // we have not found any tag
                        strHTML = "";
                    }

                    if (strData.Length > 0)
                    {
                        //-------------------------------
                        // replace entities
                        strData = strData.Replace("&amp;", "&");
                        strData = strData.Replace("&lt;", "<");
                        strData = strData.Replace("&gt;", ">");
                        strData = strData.Replace("&apos;", "'");
                        strData = strData.Replace("&quot;", "\"");
                        //-------------------------------

                        string strAux = strData; // use another copy

                        while (strAux.Length > 0)
                        {
                            //-----------------------
                            int nLen = strAux.Length;
                            //-----------------------

                            //-------------------------------
                            // now, add text to control
                            int nStartCache = this.SelectionStart;
                            string strt = strAux.Substring(0, nLen);

                            this.SelectedText = strt;
                            strAux = strAux.Remove(0, nLen);

                            this.SelectionStart = nStartCache;
                            this.SelectionLength = strt.Length;
                            //-------------------------------

                            //-------------------------------
                            // apply format
                            this.ParaFormat = pf;
                            this.CharFormat = cf;
                            //-------------------------------

                            //this.SelectionFont = new Font(this.Font.FontFamily.Name, this.Font.Size, this.SelectionFont.Style);//字体名和大小固定

                            // reposition to final
                            this.SelectionStart = this.TextLength + 1;
                            this.SelectionLength = 0;
                        }

                        // reposition to final
                        this.SelectionStart = this.TextLength + 1;
                        this.SelectionLength = 0;

                        //-------------------------------
                        // new paragraph requires to reset alignment
                        if ((strData.IndexOf("\r\n", 0) >= 0) || (strData.IndexOf("\n", 0) >= 0))
                        {
                            pf.dwMask = Consts.PFM_ALIGNMENT | Consts.PFM_NUMBERING;
                            pf.wAlignment = (short)Consts.PFA_LEFT;
                            pf.wNumbering = 0;
                        }
                        //-------------------------------
                    }
                } // while (strHTML.Length > 0)

                ////将全部字体设置为默认的字体大小
                //this.SelectAll();
                //this.SelectionFont = new Font(this.Font.FontFamily.Name, this.Font.Size, this.SelectionFont.Style);
                //this.SelectionFont = this.Font;//this.SelectionFont = this.Font;  //粗体也会去掉，不行
            }
            catch (Exception ex)
            {
                //一定要使用 Unicode 而不能采用 Ansi 
                //正确 ：
                // [StructLayout(LayoutKind.Sequential,  CharSet = CharSet.Unicode)] 
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // reposition to final
                this.SelectionStart = this.TextLength + 1;
                this.SelectionLength = 0;

                this.EndUpdate();
                this.HideSelection = false;
            }
        }
        #endregion 将html转换为rtf

    }
}
