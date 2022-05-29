using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace SparkFormEditor
{
    /// <summary>
    /// 打印预览的窗体
    /// Represents a dialog containing a <see cref="CoolPrintPreviewControl"/> control
    /// used to preview and print <see cref="PrintDocument"/> objects.
    /// </summary>
    /// <remarks>
    /// This dialog is similar to the standard <see cref="PrintPreviewDialog"/>
    /// but provides additional options such printer and page setup buttons,
    /// a better UI based on the <see cref="ToolStrip"/> control, and built-in
    /// PDF export.
    /// </remarks>
    /// > internal 关键字是类型和类型的成员 访问修饰符。 只有在同一程序集的文件中,内部类型或成员才是可访问的
    public partial class CoolPrintPreviewDialog : Form
    {
        //--------------------------------------------------------------------
        #region ** fields
        internal bool IsSucessPrint = false;
        PrintDocument _doc;
        public bool _PrintAddLog = false;

        #endregion

        //--------------------------------------------------------------------
        #region ** ctor

        /// <summary>
        /// Initializes a new instance of a <see cref="CoolPrintPreviewDialog"/>.
        /// </summary>
        public CoolPrintPreviewDialog()
            : this(null)  //CoolPrintPreviewDialog(Control parentForm)
        {
            //InitializeComponent();

            //_doc = new PrintDocument();
            //_preview.Document = new PrintDocument();
            //_doc = null;
            //_preview.Document = null;
            IsSucessPrint = false;
            _TwdPrintStatus = false;
            DoAfterPrinted = null;
        }
        /// <summary>
        /// Initializes a new instance of a <see cref="CoolPrintPreviewDialog"/>.
        /// </summary>
        /// <param name="parentForm">Parent form that defines the initial size for this dialog.</param>
        public CoolPrintPreviewDialog(Control parentForm)
        {
            InitializeComponent();
            if (parentForm != null)
            {
                Size = parentForm.Size;
            }

            _TwdPrintStatus = false;
            DoAfterPrinted = null;
        }
        #endregion

        //--------------------------------------------------------------------
        #region ** object model

        /// <summary>
        /// Gets or sets the <see cref="PrintDocument"/> to preview.
        /// </summary>
        public PrintDocument Document
        {
            get { return _doc; }
            set
            {
                // unhook event handlers
                if (_doc != null)
                {
                    _doc.BeginPrint -= _doc_BeginPrint;
                    _doc.EndPrint -= _doc_EndPrint;
                }

                // save the value
                _doc = value;

                //_doc.DefaultPageSettings.Landscape = value.DefaultPageSettings.Landscape;
                //_preview.Document = _doc;   //
                //_preview.RefreshPreview();

                // hook up event handlers
                if (_doc != null)
                {
                    _doc.BeginPrint += _doc_BeginPrint;
                    _doc.EndPrint += _doc_EndPrint;
                }


                // don't assign document to preview until this form becomes visible
                if (Visible)
                {
                    _preview.Document = Document;
                    //_preview.RefreshPreview();
                }

                //this.Refresh();
            }
        }

        #endregion

        //--------------------------------------------------------------------
        #region ** overloads

        /// <summary>
        /// Overridden to assign document to preview control only after the 
        /// initial activation.
        /// </summary>
        /// <param name="e"><see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _preview.Document = Document;
        }
        /// <summary>
        /// Overridden to cancel any ongoing previews when closing form.
        /// </summary>
        /// <param name="e"><see cref="FormClosingEventArgs"/> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_preview.IsRendering && !e.Cancel)
            {
                _preview.Cancel();
            }

            //this.Dispose();
        }

        #endregion

        //--------------------------------------------------------------------
        #region ** main commands
        static PrintDialog dlg;
        static PrinterSettings ps;
        public bool _TwdPrintStatus = false;//打印后更新打印状态标记
        public event EventHandler DoAfterPrinted;
        void _btnPrint_Click(object sender, EventArgs e) //打印按钮
        {
            try
            {
                if (sender != null && _Twd != null)
                {
                    _Twd.printAllTWD_Mode = 0; //防止在点击
                    //_printAllTWD_Mode = 0; //记录打印页信息，奇数偶数页要分开，不能所有页了。
                }

                dlg = new PrintDialog();
                dlg.AllowSomePages = true;
                dlg.AllowSelection = true;
                //dlg.UseEXDialog = true;


                dlg.PrinterSettings = _preview.Document.PrinterSettings;  //this.Document.DefaultPageSettings.PrinterSettings.PrinterName
                dlg.Document = _preview.Document; //将TiWenDan代码中中打印设置的参数（纸张类型）在复制给打印属性框，如果这里不复制dlg.Document为null；但是一旦赋值后，那么dlg.Document.DefaultPageSettings.PrinterSettings和dlg.PrinterSettings一致的
                dlg.Document.DefaultPageSettings.Landscape = _preview.Document.PrinterSettings.DefaultPageSettings.Landscape;

                dlg.PrinterSettings.Copies = 1; //防止设置后继续保留上次的多份  //打印份数

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ps = new PrinterSettings();

                    ps.PrinterName = dlg.PrinterSettings.PrinterName;//dlg.PrinterSettings.DefaultPageSettings
                    //保存选择的打印机名称
                    Foundation.INIClass.WriteValue(Path.Combine(Application.StartupPath, "Config", "User.ini"), "NurseFrom", "PrintName", ps.PrinterName);
                    GlobalVariable.DefaultPrintName = ps.PrinterName;

                    ps.MinimumPage = ps.FromPage = 1;
                    ps.MaximumPage = ps.ToPage = _preview.PageCount;

                    ps.DefaultPageSettings.Landscape = _doc.PrinterSettings.DefaultPageSettings.Landscape; //横向、纵向打印保持，不然会默认纵向打印

                    _doc.PrinterSettings = ps;
                    _doc.DefaultPageSettings.PrinterSettings = _doc.PrinterSettings;
                    _doc.DefaultPageSettings.PaperSize = dlg.Document.DefaultPageSettings.PaperSize; //dlg.PrinterSettings.DefaultPageSettings
                    _doc.DefaultPageSettings.Landscape = dlg.Document.DefaultPageSettings.Landscape; //横向、纵向打印保持，不然会默认纵向打印

                    //纠正一些破打印机的横宽倒过来
                    //正常：横/纵向：True , 纸张RawKind：9 , 纸张类型：[PaperSize A4 Kind=A4 Height=1169 Width=827]
                    //不正常：横/纵向：True , 纸张RawKind：0 , 纸张类型：[PaperSize custom Kind=Custom Height=730 Width=1015]
                    if (_doc.DefaultPageSettings.Landscape)
                    {
                        if (_doc.DefaultPageSettings.PaperSize.Width > _doc.DefaultPageSettings.PaperSize.Height)
                        {
                            int temp = _doc.DefaultPageSettings.PaperSize.Width;
                            _doc.DefaultPageSettings.PaperSize.Width = _doc.DefaultPageSettings.PaperSize.Height;
                            _doc.DefaultPageSettings.PaperSize.Height = temp;
                        }
                    }

                    _preview._PrintAddLog = _PrintAddLog;
                    //if (_PrintAddLog)
                    //{
                    //    Comm.Logger.WriteInformation("预览打印时，横/纵向：" + _doc.DefaultPageSettings.Landscape.ToString() + " , 纸张RawKind："
                    //        + _doc.DefaultPageSettings.PaperSize.RawKind.ToString() + " , 纸张类型：" + _doc.DefaultPageSettings.PaperSize.ToString());
                    //}

                    _doc.PrinterSettings.Copies = dlg.PrinterSettings.Copies;   //打印份数

                    //双面打印
                    _doc.PrinterSettings.Duplex = dlg.PrinterSettings.Duplex;  //Duplex.Simplex为单面打印，Duplex.Horizontal水平翻页

                    _preview.Document = _doc;

                    _preview.Print();

                    //设定打印纸张类型
                    //TiWenDan.setPaperSizeRawKind(_preview.Document.DocumentName, _preview.Document.DefaultPageSettings.PaperSize.RawKind.ToString());

                    //设定打印标记
                    if (_TwdPrintStatus)
                    {
                        if (DoAfterPrinted != null)
                        {
                            DoAfterPrinted(null, null);
                        }
                        IsSucessPrint = true;
                    }

                    if (_Twd != null)
                    {
                        _Twd.printAllTWD_Mode = 0;
                    }

                    ////奇数页翻转方向
                    //e.Graphics.TranslateTransform(1169, 0);
                    //e.Graphics.TranslateTransform(PrintConstant.XMove, PrintConstant.YMove);
                    //e.Graphics.RotateTransform(-270);
                    //// 缩放打印比例
                    //e.Graphics.PageScale = PrintConstant.PageScale;
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr("堆栈信息：" + ex.StackTrace);
                throw ex;
            }
        }
        //void _btnPrint_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //using (PrintDialog dlg = new PrintDialog())
        //        //{

        //        dlg = new PrintDialog();
        //        dlg.AllowSomePages = true;
        //        dlg.AllowSelection = true;
        //        //dlg.UseEXDialog = true;
        //        dlg.PrinterSettings = _preview.Document.PrinterSettings;

        //        if (dlg.ShowDialog() == DialogResult.OK)
        //        {
        //            ps = new PrinterSettings();
        //            ps.PrinterName = dlg.PrinterSettings.PrinterName;
        //            ps.MinimumPage = ps.FromPage = 1;
        //            ps.MaximumPage = ps.ToPage = _preview.PageCount;

        //            _doc.PrinterSettings = ps;
        //            _doc.DefaultPageSettings.PrinterSettings = _doc.PrinterSettings;

        //            _preview.Document = _doc;

        //            _preview.Print(); //

        //            //设定打印标记
        //            if (_TwdPrintStatus)
        //            {
        //                if (DoAfterPrinted != null)
        //                {
        //                    DoAfterPrinted(null, null);
        //                }

        //            }
        //        }

        //        //dlg.Dispose();


        //        //}

        //    }
        //    catch (Exception ex)
        //    {
        //        TWDLog.AddLog("堆栈信息：" + ex.StackTrace);
        //        throw ex;
        //    }
        //}

        void _btnPageSetup_Click(object sender, EventArgs e)
        {
            using (var dlg = new PageSetupDialog())
            {
                dlg.Document = Document;
                // dlg.PageSettings.Landscape = this.Document.PrinterSettings.DefaultPageSettings.Landscape;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // to show new page Design
                    _preview.RefreshPreview();
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------
        //#region ** zoom

        void _btnZoom_ButtonClick(object sender, EventArgs e)
        {
            _preview.ZoomMode = _preview.ZoomMode == ZoomMode.PageWidth
                ? ZoomMode.FullPage
                : ZoomMode.PageWidth;
        }

        //--------------------------------------------------------------------
        #region ** page navigation

        void _btnFirst_Click(object sender, EventArgs e)
        {
            _preview.StartPage = 0;
        }
        void _btnPrev_Click(object sender, EventArgs e)
        {
            _preview.StartPage--;
        }
        void _btnNext_Click(object sender, EventArgs e)
        {
            _preview.StartPage++;
        }
        void _btnLast_Click(object sender, EventArgs e)
        {
            _preview.StartPage = _preview.PageCount - 1;
        }
        void _txtStartPage_Enter(object sender, EventArgs e)
        {
            _txtStartPage.SelectAll();
        }
        void _txtStartPage_Validating(object sender, CancelEventArgs e)
        {
            CommitPageNumber();
        }
        void _txtStartPage_KeyPress(object sender, KeyPressEventArgs e)
        {
            var c = e.KeyChar;
            if (c == (char)13)
            {
                CommitPageNumber();
                e.Handled = true;
            }
            else if (c > ' ' && !char.IsDigit(c))
            {
                e.Handled = true;
            }
        }
        void CommitPageNumber()
        {
            int page;
            if (int.TryParse(_txtStartPage.Text, out page))
            {
                _preview.StartPage = page - 1;
            }
        }
        void _preview_StartPageChanged(object sender, EventArgs e)
        {
            var page = _preview.StartPage + 1;
            _txtStartPage.Text = page.ToString();
        }
        private void _preview_PageCountChanged(object sender, EventArgs e)
        {
            this.Update();
            Application.DoEvents();
            _lblPageCount.Text = string.Format(" / {0}", _preview.PageCount);
        }

        #endregion

        //--------------------------------------------------------------------
        #region ** job control

        void _btnCancel_Click(object sender, EventArgs e)
        {
            if (_preview.IsRendering)
            {
                _preview.Cancel();
                this.Hide();
                //Close();
                //this.Dispose();
                //this.Refresh();
            }
            else
            {
                this.Hide();
                //_preview.Dispose();
                //Close();
                //this.Dispose();
            }
        }
        void _doc_BeginPrint(object sender, PrintEventArgs e)
        {
            _btnCancel.Text = "处理中，请稍候……取消(&C)";
            _btnPrint.Enabled = _btnPageSetup.Enabled = false;
        }
        void _doc_EndPrint(object sender, PrintEventArgs e)
        {
            _btnCancel.Text = "关闭(&C)";
            _btnPrint.Enabled = _btnPageSetup.Enabled = true;
            //this.Refresh();
        }

        #endregion


        /// <summary>打印当前页
        /// 很奇怪，会打印空白（而且用设置的默认打印机才会再现）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _btnCurrentPage_Click(object sender, EventArgs e)
        {

            try
            {
                dlg = new PrintDialog();
                dlg.AllowSomePages = false;
                dlg.AllowSelection = true;
                //dlg.UseEXDialog = true;
                dlg.PrinterSettings = _preview.Document.PrinterSettings;  //this.Document.DefaultPageSettings.PrinterSettings.PrinterName
                dlg.PrinterSettings.PrintRange = PrintRange.CurrentPage; //_preview.StartPage
                dlg.Document = _preview.Document; //将TiWenDan代码中中打印设置的参数（纸张类型）在复制给打印属性框，如果这里不复制dlg.Document为null；但是一旦赋值后，那么dlg.Document.DefaultPageSettings.PrinterSettings和dlg.PrinterSettings一致的
                dlg.Document.DefaultPageSettings.Landscape = _preview.Document.PrinterSettings.DefaultPageSettings.Landscape;

                //if (_PrintAddLog)
                //{
                //    Comm.Logger.WriteInformation("预览界面时，横/纵向：" + _doc.DefaultPageSettings.Landscape.ToString() + " , 纸张RawKind："
                //        + _doc.DefaultPageSettings.PaperSize.RawKind.ToString());
                //}

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ps = new PrinterSettings();
                    ps.PrinterName = dlg.PrinterSettings.PrinterName;//dlg.PrinterSettings.DefaultPageSettings
                    ps.PrintRange = PrintRange.CurrentPage; //打印当前页
                    //ps.MinimumPage = ps.FromPage = _preview.StartPage + 1;
                    //ps.MaximumPage = ps.ToPage = _preview.StartPage + 1;

                    ps.DefaultPageSettings.Landscape = _doc.PrinterSettings.DefaultPageSettings.Landscape; //横向、纵向打印保持，不然会默认纵向打印

                    _doc.PrinterSettings = ps;
                    _doc.DefaultPageSettings.PrinterSettings = _doc.PrinterSettings;
                    _doc.DefaultPageSettings.PaperSize = dlg.Document.DefaultPageSettings.PaperSize; //dlg.PrinterSettings.DefaultPageSettings
                    _doc.DefaultPageSettings.Landscape = dlg.Document.DefaultPageSettings.Landscape; //横向、纵向打印保持，不然会默认纵向打印

                    _doc.PrinterSettings.Copies = dlg.PrinterSettings.Copies;                        //打印份数

                    //双面打印
                    _doc.PrinterSettings.Duplex = dlg.PrinterSettings.Duplex;  //Duplex.Horizontal;

                    _preview.Document = _doc;

                    _preview.Print();

                    //设定打印纸张类型
                    //TiWenDan.setPaperSizeRawKind(_preview.Document.DocumentName, _preview.Document.DefaultPageSettings.PaperSize.RawKind.ToString());

                    //设定体温单页是否打印，即：打印标记
                    if (_TwdPrintStatus)
                    {
                        if (DoAfterPrinted != null)
                        {
                            DoAfterPrinted(_preview.StartPage.ToString(), null); //防止打印当前页的时候，把所有页都设置为已打印标记。
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr("堆栈信息：" + ex.StackTrace);
                throw ex;
            }
        }

        private void _btnAllPages_Click(object sender, EventArgs e)
        {
            using (var dlg = new PrintDialog())
            {
                // configure dialog
                dlg.AllowSomePages = true;
                dlg.AllowSelection = true;
                dlg.UseEXDialog = true;
                _preview.Document.PrinterSettings.PrintRange = PrintRange.AllPages;


                // show dialog
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // print selected page range
                    _preview.Print();
                }
            }
        }

        private void _toolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == _itemActualSize)
            {
                _preview.ZoomMode = ZoomMode.ActualSize;
            }
            else if (e.ClickedItem == _itemFullPage)
            {
                _preview.ZoomMode = ZoomMode.FullPage;
            }
            else if (e.ClickedItem == _itemPageWidth)
            {
                _preview.ZoomMode = ZoomMode.PageWidth;
            }
            else if (e.ClickedItem == _itemOnePages || e.ClickedItem == tsmiPages1 || e.ClickedItem == _HitemPage1)
            {
                _preview.ZoomMode = ZoomMode.OnePages;
            }
            else if (e.ClickedItem == _itemTwoPages || e.ClickedItem == tsmiPages2 || e.ClickedItem == _HitemPage2)
            {
                _preview.ZoomMode = ZoomMode.TwoPages;
            }
            else if (e.ClickedItem == _itemThreePages || e.ClickedItem == tsmiPages3 || e.ClickedItem == _HitemPage3)
            {
                _preview.ZoomMode = ZoomMode.ThreePages;
            }
            else if (e.ClickedItem == _itemFourPages || e.ClickedItem == tsmiPages4 || e.ClickedItem == _HitemPage4)
            {
                _preview.ZoomMode = ZoomMode.FourPages;
            }
            else if (e.ClickedItem == _itemFivePages || e.ClickedItem == tsmiPages5 || e.ClickedItem == _HitemPage5)
            {
                _preview.ZoomMode = ZoomMode.FivePages;
            }
            else if (e.ClickedItem == _itemSixPages || e.ClickedItem == tsmiPages6 || e.ClickedItem == _HitemPage6)
            {
                _preview.ZoomMode = ZoomMode.SixPages;
            }

            if (e.ClickedItem == _item10 || e.ClickedItem == _Hitem10 || e.ClickedItem == tsmiZoom10)
            {
                _preview.Zoom = .1;

            }
            else if (e.ClickedItem == _item100 || e.ClickedItem == _Hitem100 || e.ClickedItem == tsmiZoom100)
            {
                _preview.Zoom = 1;

            }
            else if (e.ClickedItem == _item150 || e.ClickedItem == _Hitem150 || e.ClickedItem == tsmiZoom150)
            {
                _preview.Zoom = 1.5;
                //_preview.Size = new Size((int)(this._preview.Size.Width * _preview.Zoom), (int)(this._preview.Size.Height * _preview.Zoom));

            }
            else if (e.ClickedItem == _item200 || e.ClickedItem == _Hitem200 || e.ClickedItem == tsmiZoom200)
            {
                _preview.Zoom = 2;
            }
            else if (e.ClickedItem == _item25 || e.ClickedItem == _Hitem25 || e.ClickedItem == tsmiZoom25)
            {
                _preview.Zoom = .25;
            }
            else if (e.ClickedItem == _item50 || e.ClickedItem == _Hitem50 || e.ClickedItem == tsmiZoom50)
            {
                _preview.Zoom = .5;
            }
            else if (e.ClickedItem == _item500 || e.ClickedItem == _Hitem500 || e.ClickedItem == tsmiZoom500)
            {
                _preview.Zoom = 5;
            }
            else if (e.ClickedItem == _item75 || e.ClickedItem == _Hitem75 || e.ClickedItem == tsmiZoom75)
            {
                _preview.Zoom = .75;
            }
        }

        private void tsmi6_Click(object sender, EventArgs e)
        {
            _btnCancel_Click(sender, e);
        }

        private void tsmi1_Click(object sender, EventArgs e)
        {
            _btnPageSetup_Click(sender, e);
        }

        private void tsmi2_Click(object sender, EventArgs e)
        {
            _btnCurrentPage_Click(sender, e);
        }

        private void tsmi3_Click(object sender, EventArgs e)
        {
            //_btnAllPages_Click(sender, e);
            _btnPrint_Click(null, null);
        }

        /// <summary>
        /// 当前的打印纸张类型名称和id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _txtStartPage_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show("当前纸张类型为：" + _doc.DefaultPageSettings.PaperSize.Kind.ToString() + " ，编号为：" + _doc.DefaultPageSettings.PaperSize.RawKind.ToString());
        }

        private void CoolPrintPreviewDialog_Load(object sender, EventArgs e)
        {
            //_preview.RefreshPreview();

            if (_Twd == null)
            {
                //打印当前页的时候不要显示打印奇偶页
                this.toolStripButton打印选项.Visible = false;
            }
            else
            {
                //如果是打印很多也，但是总页数，或者选择的总页数范围是1，那么只有一个奇数页可以打印，或者只有一个偶数页打印。
                //总归有一个会打印空页的
                if (_Twd._startPage == _Twd._endPage)
                {
                    if (_Twd._startPage % 2 == 0)
                    {
                        this.toolStripMenuItem打印奇数页.Enabled = false;
                        this.toolStripMenuItem打印偶数页.Enabled = true;
                    }
                    else
                    {
                        this.toolStripMenuItem打印奇数页.Enabled = true;
                        this.toolStripMenuItem打印偶数页.Enabled = false;
                    }
                }

            }
        }

        #region 奇数偶数 奇偶页打印

        public SparkFormEditor _Twd = null;
        //public int _printAllTWD_Mode = 0;

        private void toolStripButton打印选项_Click(object sender, EventArgs e)
        {
            toolStripButton打印选项.ShowDropDown();
        }

        private void toolStripMenuItem打印奇数页_Click(object sender, EventArgs e)
        {
            if (_Twd != null)
            {
                _Twd.printAllTWD_Mode = 1;
                //_printAllTWD_Mode = 1;
            }

            _btnPrint_Click(null, null);

            _Twd.printAllTWD_Mode = 0;
        }

        private void toolStripMenuItem打印偶数页_Click(object sender, EventArgs e)
        {
            if (_Twd != null)
            {
                _Twd.printAllTWD_Mode = 2;
                //_printAllTWD_Mode = 2;
            }

            _btnPrint_Click(null, null);

            _Twd.printAllTWD_Mode = 0;
        }

        #endregion 奇数偶数 奇偶页打印
    }
}
