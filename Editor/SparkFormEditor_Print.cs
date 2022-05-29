using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Xml;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Net;
using System.Xml.XPath;
using SparkFormEditor.Model;
using System.Threading.Tasks;

namespace SparkFormEditor
{
    public partial class SparkFormEditor
    {
        #region 打印所有页参数定义
        internal int _startPage = 0;
        internal int _endPage = 0;
        internal int printAllTWD_Mode = 0; //默认为0打印所有页，1打印奇数页，2打印偶数页

        private CoolPrintPreviewDialog MyPrintDg3;
        private int printAll_PageIndex = 0; //物理页码（打印所有页的时候）和——CurrentPage一致
        private int _PrintAllStartPage = 0; //显示页码的计算的基数（打印所有页的时候）。表单数据根节点属性startPage = 0; //手动修改的页码，以后每页这个这个来累计，默认为0 。打印所有页，表单中显示的页码的基数
        private XmlNode _PrintAll_PageNode;
        private LabelExtended _PrintAll_LabelExtended;
        private RichTextBoxExtended _PrintAll_RichTextBoxExtended;
        private TransparentRTBExtended _PrintAll_TransparentRTBExtended;
        private CheckBoxExtended _PrintAll_CheckBoxExtended;
        private ComboBoxExtended _PrintAll_ComboBoxExtended;
        private TableInfor _PrintAll_TableInfor = null;
        private CoolPrintPreviewDialog MyPrintDg = null;    //区域生成图片会报错
        private int _CurrentPagePrintStartRow = -1;
        #endregion//打印所有页参数定义

        #region 区域调用打印
        private bool ReportPrint = false; //如果设置true，那么就不生成图片，而是进行打印
        private PrintDocument ReportPrintDocument = null;
        #endregion 区域调用打印

        #region 打印：工具栏，打印当前页

        /// <summary>
        /// 非表格样式，记录一下续打的选定位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoOnPrintLocation_Click(object sender, EventArgs e)
        {
            Control col = (Control)sender;

            _GoOnPrintLocation = col.Location;  //记录下非表格的续打位置
        }

        /// <summary>
        /// 打印 打印当前页 工具栏按钮
        /// </summary>
        /// <param name="type"></param>
        private void printCurrentPage(string type)
        {
            try
            {
                if (!toolStripPrintEnable)
                {
                    return;
                }

                if (this.pictureBoxBackGround.Controls.Contains(this.lblShowSearch))
                {
                    this.pictureBoxBackGround.Controls.Remove(this.lblShowSearch); //先删除这个搜索提示控件
                }

                this.pictureBoxBackGround.Focus();

                if (!PrintCheck())
                {
                    return;
                }

                int maxNoEmptyRows = GetTableRows();        //根据表格数据，得到当前的行数
                int pages = GetPageCount(maxNoEmptyRows);   //计算得到页数

                GlobalVariable.PrintPageCancelEventArgs printPage = null;
                if (this.BeforePrintPage != null)
                {
                    printPage = new GlobalVariable.PrintPageCancelEventArgs(
                        type == "续打" ? _CurrentCellIndex.X : int.Parse(_CurrentPage),
                        pages,
                        _PagePrintStatus,
                        _TemplateName, _TemplateID, _patientInfo,
                        type == "续打" ? PrintPageType.Continue : PrintPageType.Current
                        );
                    this.BeforePrintPage(this, printPage);
                }
                if (printPage != null && printPage.Cancel == true)
                {
                    return;
                }

                _IsLoading = true;

                //判断是否为续打
                if (type == "续打")
                {
                    if (!_TableType || _TableInfor.CurrentCell == null)
                    {
                        #region 以前，非表格的不能续打
                        ////重置标记位
                        //_IsLoading = false;
                        //toolStripToolBarWord.Enabled = true;

                        //string msg = "请先选择开始行后，再进行续打。";
                        //MessageBox.Show(msg, "提示消息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        #endregion 以前，非表格的不能续打

                        //如果是非表格样式的表单，进行续打的话，是从选择的控件开始往下打印。
                        printPreviewCurrentPageChart(-1); //(0) 要求第一行也要可以续打。原来是第一行即使点击续打也是完整打印的
                    }
                    else
                    {
                        ShowSearchRow(); //显示手指位置
                        this.pictureBoxBackGround.Refresh(); //DrawRowSignShow(true);//this.pictureBoxBackGround.Refresh();

                        //从光标所在行位置开始续打
                        printPreviewCurrentPageChart(_CurrentCellIndex.X); //, _CurrentCellIndex.Y

                        _TableInfor.CurrentCell = null; //要求第一行也要可以续打 可用来解决：选择某行后进行续打前，是否点击了画布（隐藏单元格输入框，在saveop时不会将CurrentCell设为null，下次再点击续打就走上面分支，会打印满足坐标的输入框等项目了）
                    }

                    //this.pictureBoxBackGround.Refresh(); //DrawRowSignShow(true);//this.pictureBoxBackGround.Refresh();
                }
                else
                {
                    this.pictureBoxBackGround.Refresh(); //DrawRowSignShow(true);//this.pictureBoxBackGround.Refresh();

                    _GoOnPrintLocation = new Point(-1, -1);

                    //判断是否已经打印，续打，不提醒
                    if (_CurrentPage != "")
                    {
                        ArrayList list = ArrayList.Adapter(_PagePrintStatus.Split('¤'));
                        if (list.Contains(_CurrentPage))
                        {
                            if (MessageBox.Show("该页已经被打印过，确定需要再次打印吗？", "打印确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {

                            }
                            else
                            {
                                //重置标记位
                                _IsLoading = false;
                                //toolStripToolBarWord.Enabled = true;
                                return;
                            }
                        }
                    }

                    //从第一行开始打印，就是全部打印
                    printPreviewCurrentPageChart(-1); //要求第一行也要可以续打
                }

                #region 判断打印状况
                ArrayList listPrint = ArrayList.Adapter(_PagePrintStatus.Split('¤'));
                string printStaus = "未打印";
                if (listPrint.Contains(_CurrentPage))
                {
                    //已经打印
                    printStaus = "已打印";
                }
                else
                {
                    //未打印
                }
                if (MyPrintDg.IsSucessPrint)
                {
                    if (this.AfterPrintPage != null)
                    {
                        this.AfterPrintPage(this, new GlobalVariable.PrintPageEventArgs(
                            type == "续打" ? _CurrentCellIndex.X : int.Parse(_CurrentPage),
                            pages,
                            _PagePrintStatus,
                            _TemplateName, _TemplateID, _patientInfo,
                            type == "续打" ? PrintPageType.Continue : PrintPageType.Current
                            ));
                    }
                }
                if (CallBackUpdateDocumentDetailInfor != null)
                {
                    CallBackUpdateDocumentDetailInfor(new string[] { printStaus, null }, null);
                }
                #endregion 判断打印状况

                //重置标记位
                _IsLoading = false;
                //toolStripToolBarWord.Enabled = true;
            }
            catch (Exception ex)
            {
                _IsLoading = false;
                //toolStripToolBarWord.Enabled = true;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }
        # endregion 打印：工具栏，打印当前页

        # region 打印：所有页
        /// <summary>
        /// 执行打印所有页
        /// </summary>
        private void printAllPage()
        {
            if (!toolStripPrintEnable)
            {
                return;
            }

            if (_RecruitXML == null || _CurrentPage == "")
            {
                ShowInforMsg("请先打开表单，才可以打印所有页。");
                return;
            }


            this.pictureBoxBackGround.Focus();
            this.pictureBoxBackGround.Refresh(); //rawRowSignShow(true);//this.pictureBoxBackGround.Refresh();

            //string nodePageText = _node_Page.InnerXml; //备份好节点页，防止打印后产生干扰
            XmlNode nodePageCurrent = _node_Page.Clone();

            if (!PrintCheck())
            {
                return;
            }

            int maxNoEmptyRows = GetTableRows();        //根据表格数据，得到当前的行数
            int pages = GetPageCount(maxNoEmptyRows);   //计算得到页数

            GlobalVariable.PrintPageCancelEventArgs printPage = null;
            if (this.BeforePrintPage != null)
            {
                printPage = new GlobalVariable.PrintPageCancelEventArgs(
                    0,
                    pages,
                    _PagePrintStatus,
                    _TemplateName, _TemplateID, _patientInfo,
                    PrintPageType.All
                    );
                this.BeforePrintPage(this, printPage);
            }
            if (printPage != null && printPage.Cancel == true)
            {
                return;
            }

            try
            {
                _IsLoading = true;
                //if (_CurrentTemplateNameTreeNode == null || _CurrentTemplateNameTreeNode.Nodes.Count == 0)
                //{
                //    ShowInforMsg("请先打开表单，才可以打印所有页。");
                //    _IsLoading = false;
                //    return;
                //}

                Frm_SetPrint wd = new Frm_SetPrint(1, this.uc_Pager1.NMax); //打印起始页选择

                DialogResult res = wd.ShowDialog();

                int startPage = 1;
                int endPage = 1;

                if (res == DialogResult.OK)
                {
                    startPage = wd.Value1;
                    endPage = wd.Value2;

                    printAll_PageIndex = startPage;

                    if (startPage > endPage)
                    {
                        ShowInforMsg("选择的打印开始页数，必须小于等于结束页数。");
                        return;
                    }
                }
                else
                {
                    wd.Dispose();
                    _IsLoading = false;
                    return;
                }

                _MaxPageForPrintAll = this.uc_Pager1.NMax;// _CurrentTemplateNameTreeNode.Nodes.Count; //总页数，根据这个物理页数，计算出手动修改后应该的页码

                //_PrintAll_TableInfor = null; //每次开始打印的时候，设置为null，表示要进行一次加载表格

                MyPrintDg3 = new CoolPrintPreviewDialog();
                _startPage = startPage;     //设置起始页
                _endPage = endPage;
                MyPrintDg3._Twd = this; //设置奇偶页打印
                MyPrintDg3._TwdPrintStatus = true;
                MyPrintDg3.DoAfterPrinted += new EventHandler(DoAfterPrinted);//打印后，反调更新页的打印状态

                PrintDocument printDocument3 = new PrintDocument();
                printDocument3.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument3_PrintPage);

                //设置文档名
                printDocument3.DocumentName = "表单打印所有页";//设置完后可在打印对话框及队列中显示（默认显示document）

                var page = GetPageSettings("表单打印所有页");

                //要先设置横纵，然后设置纸张大小
                printDocument3.PrinterSettings.DefaultPageSettings.Landscape = _PrintType;  //预览界面的效果
                printDocument3.DefaultPageSettings.Landscape = _PrintType;                  //横向True，还是纵向false默认打印 

                printDocument3.PrinterSettings = page.PrinterSettings;
                printDocument3.DefaultPageSettings = page;


                MyPrintDg3.Document = printDocument3;
                MyPrintDg3.WindowState = FormWindowState.Maximized;

                //MyPrintDg.Refresh();

                if (MyPrintDg3.ShowDialog() == DialogResult.OK)
                {
                    MyPrintDg3.Show();
                }

                //重置一些设置参数，防止打印后产程干扰
                _startPage = 0;
                _endPage = 0;
                //_node_Page.InnerXml = nodePageText;
                _node_Page = nodePageCurrent;
                _IsLoading = false;

                _isCreatImages = false;

                MyPrintDg3._TwdPrintStatus = false;
                MyPrintDg3.DoAfterPrinted -= new EventHandler(DoAfterPrinted);//打印后，反调更新页的打印状态

                #region 判断打印状况
                ArrayList listPrint = ArrayList.Adapter(_PagePrintStatus.Split('¤'));
                string printStaus = "未打印";
                if (listPrint.Contains(_CurrentPage))
                {
                    //已经打印
                    printStaus = "已打印";
                }
                else
                {
                    //未打印
                }
                if (MyPrintDg3.IsSucessPrint)
                {
                    if (this.AfterPrintPage != null)
                    {
                        this.AfterPrintPage(this, new GlobalVariable.PrintPageEventArgs(
                            0,
                            pages,
                            _PagePrintStatus,
                            _TemplateName, _TemplateID, _patientInfo,
                             PrintPageType.All
                            ));
                    }
                }
                if (CallBackUpdateDocumentDetailInfor != null)
                {
                    CallBackUpdateDocumentDetailInfor(new string[] { printStaus, null }, null);
                }
                #endregion 判断打印状况
            }
            catch (Exception ex)
            {
                _isCreatImages = false;
                _IsLoading = false;
                //_node_Page.InnerXml = nodePageText;
                _node_Page = nodePageCurrent;
                Comm.LogHelp.WriteErr(ex);
                //throw ex;
            }

        }

        /// <summary>
        /// 获取手动修改的页号索引
        /// </summary>
        private void SetPageNoStartForPrintAll()
        {
            //页号手动修改处理
            XmlNode root = _RecruitXML.SelectSingleNode("//" + nameof(EntXmlModel.NurseForm));
            string start = (root as XmlElement).GetAttribute(nameof(EntXmlModel.PageNoStart)); //默认起始页号为空，代表0
            if (start != "" && start != "0")
            {
                if (CheckHelp.IsNumber(start)) //老版本保存的肯定是数字，整体修改所有页码
                {
                    int a = int.Parse(start);

                    _PrintAllStartPage = a;  //打印所有页，表单中显示的页码的基数

                    //需要显示的总页码更新显示：
                    _MaxPageForPrintAllShow = _PrintAllStartPage + _MaxPageForPrintAll;
                }
                else
                {
                    _PrintAllStartPage = int.Parse(PageNo.GetValue(start, printAll_PageIndex)) - printAll_PageIndex;

                    //需要显示的总页码更新显示：
                    _MaxPageForPrintAllShow = int.Parse(PageNo.GetValue(start, _MaxPageForPrintAll));
                }
            }
            else
            {
                _PrintAllStartPage = 0;

                //需要显示的总页码更新显示：
                _MaxPageForPrintAllShow = _MaxPageForPrintAll; //实际的总页数
            }
        }

        private void printDocument3_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; //防锯齿    

                SetPrintScaleTransform(e.Graphics);

                System.GC.Collect();

                if (printAll_PageIndex <= _endPage)
                {
                    #region 奇偶页打印判断
                    bool needPrintThisPage = true;

                    if (printAllTWD_Mode == 1)
                    {
                        //只打印奇数页
                        if (printAll_PageIndex % 2 == 1)
                        {
                            needPrintThisPage = true;
                        }
                        else
                        {
                            needPrintThisPage = false;
                        }
                    }
                    else if (printAllTWD_Mode == 2)
                    {
                        //只打印偶数页
                        if (printAll_PageIndex % 2 == 0)
                        {
                            needPrintThisPage = true;
                        }
                        else
                        {
                            needPrintThisPage = false;
                        }
                    }
                    else
                    {
                        //所有页，奇偶页都要顺序的一起打印
                    }

                    if (!needPrintThisPage)
                    {
                        printAll_PageIndex++;
                    }
                    #endregion 奇偶页打印判断

                    //模板文件：会做备份的，防止和界面上的混乱
                    _node_Page = GetTemplatePageNode(_TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design))), printAll_PageIndex);

                    //"NurseForm/Forms/Form[@SN='" + printAll_PageIndex.ToString() + "']"
                    //数据节点：
                    _PrintAll_PageNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), printAll_PageIndex.ToString(), "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));

                    //页号手动修改处理
                    SetPageNoStartForPrintAll();
                    //页号手动修改处理


                    //载入表单模板
                    DrawEditorChart(e.Graphics, false, true, true); //非表格项目，这里就取值后打印(根据上面的数据节点_PrintAll_PageNode)

                    //添加的图片要再遍历本页数据进行绘制；因为根据模板是无法知道有没有添加的图片的
                    Bitmap bmp;
                    string[] arr;
                    Point p;
                    Size sz;
                    if (_PrintAll_PageNode != null)
                    {
                        foreach (XmlAttribute xe in _PrintAll_PageNode.Attributes)
                        {
                            if (xe.Name.StartsWith(nameof(EntXmlModel.IMAGE)) && !string.IsNullOrEmpty(xe.Value))
                            {
                                //是图片:属性并是名字 ¤ 坐标¤二进制
                                arr = xe.Value.Split('¤');

                                sz = StringNumber.getSize(arr[2]);
                                bmp = new Bitmap(sz.Width, sz.Height);

                                bmp = (Bitmap)FileHelp.ConvertStringToImage(arr[4]);
                                p = StringNumber.getSplitValue(arr[1]); //位置

                                e.Graphics.DrawImage(bmp, p.X, p.Y, sz.Width, sz.Height);

                                //图片是否需要边框线
                                if (arr[3] == "")
                                {
                                    //e.Graphics.
                                }
                                else
                                {
                                    e.Graphics.DrawRectangle(new Pen(Color.Black), p.X, p.Y, sz.Width, sz.Height);
                                }
                            }
                        }
                    }

                    //载入本页数据
                    LoadTable(_PrintAll_TableInfor, true); //打印所有页的时候，另作控制，给打印所有页用的_TableInfor赋值

                    //绘制表格的基础线
                    if (_TableType && _PrintAll_TableInfor != null) // 根据解析本页的数据，形成本页的表格，进行绘制。这里还是默认的表格线。
                    {
                        //绘制表格的基础线
                        DrawTableBaseLines(e.Graphics, _PrintAll_TableInfor);

                        //绘制表格数据和线
                        DrawTableData_Lines(e.Graphics, true, _PrintAll_TableInfor, printAll_PageIndex.ToString());
                    }

                    if (_PrintAll_TableInfor != null)
                    {
                        _PrintAll_TableInfor.Dispose();
                        //_PrintAll_TableInfor = null;
                    }

                    //if (printAll_PageIndex == _endPage)
                    if (printAll_PageIndex == _endPage
                        || (printAllTWD_Mode != 0 && printAll_PageIndex + 1 >= _endPage))
                    {
                        printAll_PageIndex = _startPage;
                        e.HasMorePages = false;
                        e.Graphics.Dispose();
                    }
                    else
                    {
                        printAll_PageIndex++;
                        e.HasMorePages = true;
                    }
                }
            }
            catch (Exception ex)
            {
                printAllTWD_Mode = 0;
                Comm.LogHelp.WriteErr(ex); //Comm.Logger.WriteErr(ex)
                throw ex;
            }
        }

        #endregion 打印：所有页

        #region 打印方法

        /// <summary>打印预览
        /// 外部调用方法：
        /// 预览
        /// </summary>
        private void printPreviewCurrentPageChart(int startRow)
        {
            try
            {
                if (_node_Page == null || _node_Page.ChildNodes.Count == 0)
                {
                    ShowInforMsg("还没有打开任何表单，无法进行打印。");
                    return;
                }

                this.Cursor = Cursors.WaitCursor;
                //_IsPrintingDraw = true;
                _CurrentPagePrintStartRow = startRow;

                MyPrintDg = new CoolPrintPreviewDialog();
                //MyPrintDg._Twd = this;
                MyPrintDg._TwdPrintStatus = true; //打印后更新打印状态
                MyPrintDg.DoAfterPrinted += new EventHandler(DoAfterPrinted);//打印后，反调更新页的打印状态
                _startPage = int.Parse(_CurrentPage);
                _endPage = _startPage;

                PrintDocument printDocument1 = new PrintDocument(); //PrintDocument必须这里定义，否则多线程打印机会内存溢出
                printDocument1.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument1_PrintPage);

                var page = GetPageSettings("表单打印");

                //设置文档名
                printDocument1.DocumentName = "表单打印";//设置完后可在打印对话框及队列中显示（默认显示document）

                //printDocument1.DefaultPageSettings.Landscape 

                //要先设置横纵，然后设置纸张大小
                printDocument1.PrinterSettings.DefaultPageSettings.Landscape = _PrintType; //预览界面的效果
                printDocument1.DefaultPageSettings.Landscape = _PrintType; //横向True，还是纵向false默认打印 

                printDocument1.PrinterSettings = page.PrinterSettings;
                printDocument1.DefaultPageSettings = page;


                MyPrintDg._PrintAddLog = _PrintAddLog;
                if (_PrintAddLog)
                {
                    //Comm.Logger.WriteInformation(_TemplateName + "：表单界面设置时，横/纵向：" + printDocument1.DefaultPageSettings.Landscape.ToString() + " , 纸张RawKind："
                    //    + printDocument1.DefaultPageSettings.PaperSize.RawKind.ToString());
                    Comm.LogHelp.WriteInformation(_TemplateName + "：表单界面设置时，横/纵向：" + _PrintType.ToString() + " , 纸张RawKind："
                        + printDocument1.DefaultPageSettings.PaperSize.RawKind.ToString() + " , 纸张类型：" + printDocument1.DefaultPageSettings.PaperSize.ToString());
                }

                //printDocument1.PrinterSettings.DefaultPageSettings.PaperSize = ps;


                MyPrintDg.Document = printDocument1;

                MyPrintDg.Refresh();

                MyPrintDg.WindowState = FormWindowState.Maximized;

                if (MyPrintDg.ShowDialog() == DialogResult.OK)
                {
                    MyPrintDg.Show();
                }

                MyPrintDg._TwdPrintStatus = false;
                MyPrintDg.DoAfterPrinted -= new EventHandler(DoAfterPrinted);//打印后，反调更新页的打印状态
                _startPage = 0;
                _endPage = 0;

                this.Cursor = Cursors.Default;
                _CurrentPagePrintStartRow = -1;

                //_IsPrintingDraw = false;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                //_IsPrintingDraw = false;
                _CurrentPagePrintStartRow = -1;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }


        private PageSettings GetPageSettings(string name)
        {
            //设置纸张大小（可以不设置取，取默认设置）
            PageSettings page = new PageSettings();
            page.Margins.Left = 0;
            page.Margins.Right = 0;
            page.Margins.Top = 0;
            page.Margins.Bottom = 0;

            //双面打印，即使打印机更新驱动后，支持双面打印，也勾选了。但是还是需要手动翻纸：并打印完后再次按一下打印机的确定按钮，打印剩余的一半。
            //page.PrinterSettings.CanDuplex //获取指示打印机是否支持双面打印的值。
            //if (page.PrinterSettings.CanDuplex == true)
            //{
            //    //-1打印机默认的双面打印设置，1单面打印，2双面垂直打印，3 双面水平打印
            //    if (_PrintType)
            //    {
            //        page.PrinterSettings.Duplex = Duplex.Horizontal;
            //        printDocument3.PrinterSettings.DefaultPageSettings.PrinterSettings.Duplex = Duplex.Horizontal;
            //    }
            //    else
            //    {
            //        page.PrinterSettings.Duplex = Duplex.Vertical;
            //        printDocument3.PrinterSettings.DefaultPageSettings.PrinterSettings.Duplex = Duplex.Vertical;
            //    }
            //}


            if (GlobalVariable.PrintPageSize == Size.Empty)
            {
                if (this._PrintType)
                {
                    GlobalVariable.PrintPageSize.Width = this.pictureBoxBackGround.Size.Height;
                    GlobalVariable.PrintPageSize.Height = this.pictureBoxBackGround.Size.Width;
                }
                else
                {
                    GlobalVariable.PrintPageSize.Width = this.pictureBoxBackGround.Size.Width;
                    GlobalVariable.PrintPageSize.Height = this.pictureBoxBackGround.Size.Height;
                }
            }

            int leftGap = 0;
            int topGap = 0;
            if (this._PrintType)
            {
                leftGap = (this.pictureBoxBackGround.Size.Width - GlobalVariable.PrintPageSize.Height) / 2;
                topGap = (this.pictureBoxBackGround.Size.Height - GlobalVariable.PrintPageSize.Width) / 2;
                //ps = new PaperSize(name ?? "表单打印", GlobalVariable.PrintPageSize.Height, GlobalVariable.PrintPageSize.Width);
                //ps = new PaperSize("表单打印所有页", GlobalVariable.PrintPageSize.Height, GlobalVariable.PrintPageSize.Width);
            }
            else
            {
                leftGap = (this.pictureBoxBackGround.Size.Width - GlobalVariable.PrintPageSize.Width) / 2;
                topGap = (this.pictureBoxBackGround.Size.Height - GlobalVariable.PrintPageSize.Height) / 2;
                //ps = new PaperSize(name ?? "表单打印", GlobalVariable.PrintPageSize.Width, GlobalVariable.PrintPageSize.Height);
                //ps = new PaperSize("表单打印所有页", GlobalVariable.PrintPageSize.Width, GlobalVariable.PrintPageSize.Height);
            }
            if (leftGap > 0)
                page.Margins.Left = page.Margins.Right = leftGap;
            if (topGap > 0)
                page.Margins.Top = page.Margins.Bottom = topGap;
            //ps = new PaperSize("表单打印", this.pictureBoxBackGround.Size.Height, this.pictureBoxBackGround.Size.Width);


            //关于打印机纸张设定，在弹出的打印机选择和设置对话框中，点击首选项或者属性，都是第一个默认打印机设置的纸张类型。
            //除非是可以在打印机上右击首选项还是系统中打印机设置的默认纸张
            //而且，第一个默认打印机的纸张类型中可能不包含第二个打印机的纸张类型。但是表单需要用第二个非默认打印机打印的话。类型设置也就无效了。
            //纸张类型设定，客户端设定-默认
            SettingPrintPaper pap = new SettingPrintPaper();
            pap.SetDefaultPrinterName(page.PrinterSettings, GlobalVariable.DefaultPrintName);//nameof(EntXmlModel.Record)

            PaperSize ps = new PaperSize();
            //纸张类型设定，客户端设定-默认
            var rawkind = pap.GetPaperSizeRawKind(GlobalVariable.PrintPageSizeSwitchByName, page.PrinterSettings.PrinterName);
            if (!string.IsNullOrEmpty(rawkind))
            {
                ps.RawKind = Convert.ToInt32(rawkind);
            }
            //PaperSize ps = new PaperSize(name ?? "表单打印", GlobalVariable.PrintPageSize.Width, GlobalVariable.PrintPageSize.Height);
            page.PaperSize = ps;

            //默认设置横纵
            page.Landscape = _PrintType;

            page.PrinterSettings.DefaultPageSettings.Landscape = _PrintType; //横向、纵向打印设置
            return page;
        }

        /// <summary>打印和预览调用的绘制事件
        /// 打印和预览调用的绘制事件
        /// 打印当前页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            //打印的时候和界面显示的会不一样。比如一个长的字符串右边一条竖线，界面上没有超出。打印的时候就会超出了。
            //e.Graphics.TextRenderingHint = TextRenderingHint.SystemDefault
            //e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            SetPrintScaleTransform(g);
            //e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            //e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality; //高像素偏移质量   
            //e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            //e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            ////e.Graphics.RotateTransform(-270);//旋转角度绘制           
            ////e.Graphics.PageScale = 2.0f;// 缩放打印比例，默认是1.0

            this.pictureBoxBackGround.SuspendLayout();

            //ShowInforMsg(string.Format("{0:yyyy-MM-dd HH:mm:ss:ffff}", GetServerDate()));

            //将表单信息都绘制到打印中
            drawToImage(g, false, true);

            //_CurrentPagePrintStartRow = -1;//会导致预览是好的，设计打印又是全部打印了

            this.pictureBoxBackGround.ResumeLayout();
        }



        private void SetPrintScaleTransform(Graphics g)
        {
            int pageWidth = 0;
            int pageHeight = 0;
            if (GlobalVariable.PrintPageSize == Size.Empty)
            {
                if (this._PrintType)
                {
                    GlobalVariable.PrintPageSize.Width = this.pictureBoxBackGround.Size.Height;
                    GlobalVariable.PrintPageSize.Height = this.pictureBoxBackGround.Size.Width;
                }
                else
                {
                    GlobalVariable.PrintPageSize.Width = this.pictureBoxBackGround.Size.Width;
                    GlobalVariable.PrintPageSize.Height = this.pictureBoxBackGround.Size.Height;
                }
            }
            if (this._PrintType)
            {
                pageWidth = GlobalVariable.PrintPageSize.Height;
                pageHeight = GlobalVariable.PrintPageSize.Width;
            }
            else
            {
                pageWidth = GlobalVariable.PrintPageSize.Width;
                pageHeight = GlobalVariable.PrintPageSize.Height;
            }
            //1128,798
            //模板的大小，设置为A4，但是打印需要根据PaperSize大小来判断，如果模板大小>纸张大小，需要同比缩小
            if (pageWidth < this.pictureBoxBackGround.Size.Width &&
                 pageHeight < this.pictureBoxBackGround.Size.Height)
            {
                var xs_width = 1.0f * pageWidth / this.pictureBoxBackGround.Size.Width;
                var xs_height = 1.0f * pageHeight / this.pictureBoxBackGround.Size.Height;

                GlobalVariable.ScaleTransWidth = xs_width;
                GlobalVariable.ScaleTransHeight = xs_height;

                //e.Graphics.TranslateTransform(-1.0f * pageWidth * (1 - xs_width), -1.0f * pageHeight * (1 - xs_height));
                //e.Graphics.TranslateClip(pageWidth * (1 - xs_width), pageHeight * (1 - xs_height));
                //if (this._PrintType)
                //{
                //    e.Graphics.TranslateTransform(pageWidth, pageHeight);
                //    e.Graphics.RotateTransform(180);
                //}
                g.ScaleTransform(xs_width, xs_height);
            }
            else
            {
                GlobalVariable.ScaleTransWidth = 1;
                GlobalVariable.ScaleTransHeight = 1;
            }
            Comm.LogHelp.WriteDebug($"打印系数：{GlobalVariable.ScaleTransWidth},{GlobalVariable.ScaleTransHeight}");
        }

        /// <summary>
        /// 在进行打印等操作前，防止光标还在单元格内，修改的数据不能保存，所以这里保存一下
        /// </summary>
        private void SaveToOp()
        {
            if (_TableType && _TableInfor != null)//有表格的时候，修改后光标没有离开，就直接点击打印等，会无法更新，所以要处理一下
            {
                //先隐藏界面上已经显示的表格的输入框
                for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                {
                    if (_TableInfor.Columns[j].RTBE.Visible)
                    {
                        Cells_LeaveSaveData(_TableInfor.Columns[j].RTBE, null); //触发光标离开事件，进行保存 数据
                        _TableInfor.Columns[j].RTBE.Visible = false;

                        _TableInfor.CurrentCell = null;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 根据当前显示页面，重新绘制：生成图片和打印都可以调用
        /// </summary>
        /// <param name="g"></param>
        /// <param name="gDisposeFlg"></param>
        /// <param name="isPrintOrCreatImages"></param>
        private void drawToImage(Graphics g, bool gDisposeFlg, bool isPrintOrCreatImages)
        {
            try
            {
                SaveToOp();

                Bitmap bmpBackTemp;
                //int pTop, pBottom, pLeft, pRight;
                byte[] b;
                //int moreWidth = 0;
                RichTextBoxExtended rtbe;
                LabelExtended lbl;
                //RichTextBoxExtended rtbeTemp;
                Point orgLocation = new Point(-1, -1);
                //int lineCount = 1;

                //Graphics gRtb;//生成图片的时候比较怪异，位置会错乱，不能和打印一样的绘制
                //Bitmap bmpRtb;

                if (this.pictureBoxBackGround.Controls.Count > 0 && _CurrentPagePrintStartRow == -1) //要求第一行也要可以续打  && _CurrentPagePrintStartRow == 0
                {
                    //foreach (Control control in this.pictureBoxBackGround.Controls)
                    Control control;
                    for (int i = this.pictureBoxBackGround.Controls.Count - 1; i >= 0; i--)
                    {
                        control = this.pictureBoxBackGround.Controls[i]; //遍历的时候，是根据界面上的层次顺序来获取顺序的

                        if (control.Location.Y < _GoOnPrintLocation.Y || (control.Location.Y == _GoOnPrintLocation.Y && control.Location.X < _GoOnPrintLocation.X))
                        {
                            continue;   //非表格的，续打位置以上的跳过，输入框等没有问题，勾选框如果在下面每次都会打印的
                        }

                        if (!control.Visible || (control is RichTextBoxExtended && ((RichTextBoxExtended)control)._IsTable))
                        {
                            continue;   //隐藏的控件，不需要打印
                        }
                        if (control.Text.IndexOf("尿管") >= 0)
                        {

                        }
                        if (control is CheckBoxExtended)
                        {
                            if (!GlobalVariable.PrintEffect)
                            {
                                GDiHelp.DrawCheckBox((CheckBoxExtended)control, g);
                            }
                            else
                            {
                                //调用控件自身的绘制方法，打印效果和界面一样，但是放大会模糊
                                bmpBackTemp = new Bitmap(control.Width, control.Height);//直接调用控件的绘制方法，但是方法不清晰
                                ((CheckBoxExtended)control).DrawToBitmap(bmpBackTemp, new Rectangle(new Point(0, 0), control.Size));
                                g.DrawImage(bmpBackTemp, control.Location.X, control.Location.Y, control.Size.Width, control.Size.Height);
                            }
                        }
                        else if (control is ComboBoxExtended)
                        {
                            if (!GlobalVariable.PrintEffect)
                            {
                                GDiHelp.DrawComboBox((ComboBoxExtended)control, g);
                            }
                            else
                            {
                                //调用控件自身的绘制方法，打印效果和界面一样，但是放大会模糊
                                bmpBackTemp = new Bitmap(control.Width, control.Height);//直接调用控件的绘制方法，但是方法不清晰
                                ((ComboBoxExtended)control).DrawToBitmap(bmpBackTemp, new Rectangle(new Point(0, 0), control.Size));
                                g.DrawImage(bmpBackTemp, control.Location.X, control.Location.Y, control.Size.Width, control.Size.Height);

                                //Clipboard.SetImage((Bitmap)bmpBackTemp.Clone()); //调试：保存到粘帖板，进行查看
                            }
                        }
                        else if (control is RichTextBoxExtended)
                        {
                            rtbe = (RichTextBoxExtended)control;

                            if (rtbe.Visible && !rtbe._IsTable)
                            {

                                //如果是空，跳出循环  printRtbeExtend中处理
                                //if (rtbe.Text.Trim() == "" && !rtbe.Name.StartsWith("签名") && !rtbe.Name.StartsWith("记录者")) continue;  //手膜签名只有图片，Text为空格。

                                orgLocation = new Point(-1, -1); //字会偏上  orgLocation = new Point(rtbe.Location.X, rtbe.Location.Y + 2);
                                printRtbeExtend(rtbe, g, orgLocation);
                            }
                        }
                        else if (control is LabelExtended)//文本显示内容
                        {
                            //非表格续打的时候，页码跳过。但是选中页码(坐标一样)，还是要打印的
                            if (_GoOnPrintLocation != new Point(-1, -1) && _PageNo == control && _GoOnPrintLocation != control.Location)
                            {
                                continue;
                            }

                            lbl = (LabelExtended)control;

                            //住院号="9527001&#xA;1234567890ABCD1234567890&#xA;AB
                            //如果是换行的，在打印当前页和打印所有页的时候，还要根据坐标上下调整位置比较好。
                            Point locationAdjust = lbl.Location; //在界面赋值的时候调整位置，切换界面的时候重置默认配置坐标。只要在打印所有页和界面赋值的时候计算纵坐标就行

                            //如果标签配置了Size，表示自动换行
                            Size newS = Comm.getSize(lbl.Tag.ToString().TrimEnd('¤')).ToSize();

                            if (newS.Width > 0 && newS.Height > 0)
                            {
                                //相对当位置，上下调整居中的纵坐标
                                //locationAdjust = new Point(lbl.Location.X, lbl.Location.Y - lbl.Size.Height / 2);

                                //自动回行的，肯定居左
                                g.DrawString(lbl.Text, new Font(lbl.Font.Name, lbl.Font.Size - Comm.GetPrintFontGay(lbl.Font.Size)), new SolidBrush(lbl.ForeColor), new RectangleF(locationAdjust.X, locationAdjust.Y, newS.Width + 2, newS.Height));
                                //g.DrawString(lbl.Text, new Font(lbl.Font.Name,lbl.Font.Size - Recruit3Const.GetPRINT_FONT_DIFF(lbl.Font.Size)), new SolidBrush(lbl.ForeColor), new RectangleF(lbl.Location.X, lbl.Location.Y, newS.Width + 2, newS.Height));
                            }
                            else
                            {
                                ////相对当位置，上下调整居中的纵坐标：根据回行符计算行数：&#xA; 
                                //if (lbl.Text.Contains("\n")) //"张三\n姓名回行\n测试一下"
                                //{
                                //    lineCount = lbl.Text.Length - lbl.Text.Replace("\n", "").Length + 1; //得到回行符的个数 + 1 = 行数

                                //    locationAdjust = new Point(lbl.Location.X, locationAdjust.Y - (int)(lbl.Font.Size * lineCount / 2));
                                //}

                                //判断该标签是否居中
                                if (lbl.TextAlign == System.Drawing.ContentAlignment.MiddleCenter
                                    || lbl.TextAlign == System.Drawing.ContentAlignment.TopCenter)
                                {
                                    g.DrawString(lbl.Text, lbl.Font, new SolidBrush(lbl.ForeColor), lbl.Location.X + lbl.Size.Width / 2 - (int)g.MeasureString(lbl.Text, lbl.Font).Width / 2, locationAdjust.Y);
                                }
                                else
                                {
                                    g.DrawString(lbl.Text, lbl.Font, new SolidBrush(lbl.ForeColor), lbl.Location.X, locationAdjust.Y);
                                }
                            }

                        }
                        else if (control is Vector)
                        {
                            g.DrawImage(((Vector)control).GetDataToPic(), ((Vector)control)._imagetable.CtPointF);
                        }
                        else if (control is GeneticMapControl)
                        {
                            g.DrawImage(((GeneticMapControl)control).BackgroundImage, ((GeneticMapControl)control).Location);

                            if (((PictureBox)control).BorderStyle == BorderStyle.FixedSingle)
                            {
                                g.DrawRectangle(new Pen(Color.Black), control.Location.X, control.Location.Y, control.Width, control.Height);
                                //g.DrawRectangle(new Pen(Color.Black), control.Location);
                            }
                        }
                        else if (control is PictureBox)//文本显示内容
                        {
                            if (control.Tag != null || (control is VectorControl)) //非静态logo图片和矢量图的时候，这里遍历的时候才绘制
                            {
                                //防止图像绘制后界面上显示失真，效果不一致：.BackgroundImageLayout = ImageLayout.Stretch;
                                //需要合理的调整图片的长宽和真实的长宽比例一直，否则界面显示Stretch和打印效果就会有差异
                                g.DrawImage(((PictureBox)control).BackgroundImage, control.Location.X, control.Location.Y, control.Size.Width, control.Size.Height);
                                //g.DrawImage(((PictureBox)control).BackgroundImage, control.Location.X, control.Location.Y, ((PictureBox)control).BackgroundImage.Width, ((PictureBox)control).BackgroundImage.Height);

                                if (((PictureBox)control).BorderStyle == BorderStyle.FixedSingle)
                                {
                                    g.DrawRectangle(new Pen(Color.Black), control.Location.X, control.Location.Y, control.Width, control.Height);
                                }
                            }
                        }
                        else if (control is MonthCalendar)
                        {
                            //日历控件跳过
                        }
                    }
                }

                //this.pictureBoxBackGround.Controls.Count > 0 && _CurrentPagePrintStartRow == 0 只有字符串等不是控件的就会有问题，预览的时候
                if (_GoOnPrintLocation == new Point(-1, -1) && _CurrentPagePrintStartRow == -1) //要求第一行也要可以续打  && _CurrentPagePrintStartRow == 0
                {
                    //重新绘制体温单背景
                    //只画底图,不需要释放g,不需要添加控件(因为是打印)
                    DrawEditorChart(g, false, false, false);
                }

                //不含有表格，或者表格的坐标在续打位置以上的时候（无需打印）
                if (_TableInfor == null || (_TableInfor.Location.Y < _GoOnPrintLocation.Y || (_TableInfor.Location.Y == _GoOnPrintLocation.Y && _TableInfor.Location.X < _GoOnPrintLocation.X)))
                {
                    //continue;
                }
                else
                {
                    //打印：表格数据:数据和单元格以及行的自定义线
                    DrawTableData_Lines(g, isPrintOrCreatImages, _TableInfor, _CurrentPage);   //DrawTableBaseLine在InitChart中绘制了
                }

                //根据参数判断，要不要释放g
                if (gDisposeFlg)
                {
                    g.Dispose();
                }

                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;
                //_CurrentPagePrintStartRow = -1;
            }
            catch (Exception ex)
            {
                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;
                _CurrentPagePrintStartRow = -1;

                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 计算：在标签自动换行或者手动换行后，纵向位置居中的坐标
        /// </summary>
        /// <param name="lbl"></param>
        /// <returns></returns>
        private Point GetVariableVerticalCenterLocation(LabelExtended lbl)
        {
            Point locationAdjust = lbl.Location;
            int lineCount = 1;

            //如果标签配置了Size，表示自动换行
            Size newS = Comm.getSize(lbl.Tag.ToString().TrimEnd('¤')).ToSize();

            if (newS.Width > 0 && newS.Height > 0)
            {
                ////而且设置了Size要自动换行的标签，界面上本来就预留了空间，本来就是要往下撑的。
                ////自动回行的，始终没有完美的解决方案，在一行，两行，效果会有偏差。错位。 
                ////相对当位置，上下调整居中的纵坐标
                ////不能直接获取设置的标签的高度，如果行数不满，比如只写了一行或者两行，那么就居中，不是居中的效果了
                //Graphics g = lbl.CreateGraphics(); //g.MeasureString(lbl.Text, lbl.Font).ToSize();
                ////Size adjustSize = TextRenderer.MeasureText(lbl.Text, lbl.Font);
                //try
                //{
                //    Size adjustSize = lbl.MeasureStringExtended(g, lbl.Text, lbl.Font, lbl.Width).ToSize();
                //    Size adjustSizeOneLine = lbl.MeasureStringExtended(g, "T", lbl.Font, lbl.Width).ToSize(); //一个字母肯定是一行的
                //    //Size adjustSize = g.MeasureString(lbl.Text, lbl.Font).ToSize();
                //    //Size adjustSizeOneLine = g.MeasureString("T", lbl.Font).ToSize(); //一个字母肯定是一行的

                //    locationAdjust = new Point(lbl.Location.X, lbl.Location.Y - (adjustSize.Height - adjustSizeOneLine.Height) / 2);
                //}
                //catch (Exception ex)
                //{
                //    ////索引和长度必须引用该字符串内的位置。
                //    //如果计算高度的方法异常，那么设置为设置的最大高度
                //    locationAdjust = lbl.Location;
                //    Comm.Logger.WriteErr(ex);
                //}

                //g.Dispose();
                ////locationAdjust = new Point(lbl.Location.X, lbl.Location.Y - adjustSize.Height + (int)lbl.Font.Size + 4 / 2);
            }
            else
            {
                //相对当位置，上下调整居中的纵坐标：根据回行符计算行数：&#xA; 
                if (lbl.Text.Contains("\n")) //"张三\n姓名回行\n测试一下"
                {
                    lineCount = lbl.Text.Length - lbl.Text.Replace("\n", "").Length; //得到回行符的个数 + 1 = 行数

                    locationAdjust = new Point(lbl.Location.X, lbl.Location.Y - (((int)(lbl.Font.Size) + 5) * lineCount / 2)); // 4是lable行间距
                }
            }

            return locationAdjust;
        }

        /// <summary>
        /// 绘制表格的数据和所有的自定义线
        /// isPrint打印的时候，不用绘制图片，直接用g绘制，放大也清楚
        /// </summary>
        private void DrawTableData_Lines(Graphics g, bool isPrint, TableInfor tableInforPara, string pagePara)
        {
            Point orgLocation;
            if (_TableType && tableInforPara != null)
            {
                _isCreatImages = true;

                int fieldIndex = 0;

                HorizontalAlignment temp;
                string showText = "";
                SolidBrush sBrrsh = new SolidBrush(Color.Red); //绘制修订的提示
                //int locDiff = 5;

                bool tempIsLoading = false;
                //bool tempIsPrintAll = false; 

                for (int field = 0; field < tableInforPara.GroupColumnNum; field++)
                {
                    fieldIndex = tableInforPara.RowsCount * field;

                    for (int i = 0; i < tableInforPara.RowsCount; i++)
                    {
                        if (i + fieldIndex >= _CurrentPagePrintStartRow)
                        {
                            //需要打印的范围
                        }
                        else
                        {
                            //不需要打印的
                            continue;
                        }

                        if (!isPrint)
                        {
                            //提示合计行：行头提示
                            DrawSumRowAttention(tableInforPara, i, null);
                        }

                        for (int j = 0; j < tableInforPara.ColumnsCount; j++)
                        {
                            if (isPrint)
                            {
                                //防止事件处理，打印所有页的时候会明显变慢
                                tempIsLoading = _IsLoading;
                                //tempIsPrintAll = tableInforPara.Columns[j].RTBE._IsPrintAll; //先备份

                                _IsLoading = true;
                                //tableInforPara.Columns[j].RTBE._IsPrintAll = true;
                            }

                            try
                            {
                                //如果上一行，存在上下标等字体，会导致字体变小，产生异常。
                                //u静点,遵医嘱留置胃管，【上下标】的文字是第一个文字，就会出现，这种情况
                                string cellText = tableInforPara.Cells[i + fieldIndex, j].Text;
                                tableInforPara.Columns[j].RTBE.ResetDefault();
                                tableInforPara.Columns[j].RTBE.SelectionFont = tableInforPara.Columns[j].RTBE.Font;
                                //如果上一行，存在上下标等字体，会导致字体变小，产生异常。

                                //纵向合并的,是多行的，如果先赋值，再设置多行，初始化显示只能显示第一行
                                if (!tableInforPara.Columns[j].RTBE.Multiline && tableInforPara.Cells[i + fieldIndex, j].IsMerged() && tableInforPara.Cells[i + fieldIndex, j].ColSpan == 0)
                                {
                                    tableInforPara.Columns[j].RTBE.Multiline = true;
                                }

                                //绘制单元格内容：优先取Rtf的值
                                if (tableInforPara.Cells[i + fieldIndex, j].Rtf != "" && tableInforPara.Cells[i + fieldIndex, j].IsRtf_OR_Text)
                                {
                                    //如果是Rtf的不进行行文字颜色更新，以本身的进行（当然如果仅仅是上下标的，其实还应该设置颜色的）
                                    tableInforPara.Columns[j].RTBE.Rtf = tableInforPara.Cells[i + fieldIndex, j].Rtf;
                                }
                                else
                                {
                                    tableInforPara.Columns[j].RTBE.Text = tableInforPara.Cells[i + fieldIndex, j].Text;

                                    //整行文字颜色自动修改
                                    if (!string.IsNullOrEmpty(tableInforPara.Rows[i + fieldIndex].RowForeColor))
                                    {
                                        tableInforPara.Columns[j].RTBE.ForeColor = Color.FromName(tableInforPara.Rows[i + fieldIndex].RowForeColor);
                                        tableInforPara.Columns[j].RTBE.SelectAll();
                                        tableInforPara.Columns[j].RTBE.SelectionColor = tableInforPara.Columns[j].RTBE.ForeColor;

                                    }
                                    else
                                    {
                                        tableInforPara.Columns[j].RTBE.ForeColor = tableInforPara.Columns[j].RTBE.DefaultForeColor;
                                        tableInforPara.Columns[j].RTBE.SelectAll();
                                        tableInforPara.Columns[j].RTBE.SelectionColor = tableInforPara.Columns[j].RTBE.ForeColor;
                                    }

                                    tableInforPara.Columns[j].RTBE.SetAlignment();
                                }

                                //if (tableInforPara.Columns[j].RTBE.Name == "日期")
                                if (tableInforPara.Columns[j].RTBE.Name.StartsWith("日期"))
                                {
                                    tableInforPara.Columns[j].RTBE._YMD = tableInforPara.Cells[i + fieldIndex, j].DateFormat;
                                    showText = tableInforPara.Columns[j].RTBE.GetShowDate();
                                    if (tableInforPara.Columns[j].RTBE.Text != showText)
                                    {
                                        temp = tableInforPara.Columns[j].RTBE.SelectionAlignment;

                                        tableInforPara.Columns[j].RTBE.Text = showText;

                                        tableInforPara.Columns[j].RTBE.SelectionAlignment = temp;
                                    }
                                }

                                tableInforPara.Columns[j].RTBE.Location = tableInforPara.Cells[i + fieldIndex, j].RtbeLocation();
                                tableInforPara.Columns[j].RTBE._HaveSingleLine = tableInforPara.Cells[i + fieldIndex, j].SingleLine;
                                tableInforPara.Columns[j].RTBE._HaveDoubleLine = tableInforPara.Cells[i + fieldIndex, j].DoubleLine;

                                //绘制行和单元格的修改后的边框线样式：
                                //单元格线： 如果为空"",那么认为是白线，就会挡住
                                if (tableInforPara.Cells[i + fieldIndex, j].CellLineColor != Color.Black)
                                {
                                    if (!tableInforPara.Cells[i + fieldIndex, j].IsMergedNotShow)
                                    {
                                        DrawCellLine(i + fieldIndex, j, tableInforPara.Cells[i + fieldIndex, j].CellLineColor, g, tableInforPara);

                                        if (tableInforPara.Cells[i + fieldIndex, j].NotTopmLine)
                                        {
                                            HideCellBottomLine(i + fieldIndex, j, g, tableInforPara);
                                        }
                                    }
                                }

                                //上面先绘制线，再绘制输入框内容
                                if (!tableInforPara.Cells[i + fieldIndex, j].IsMergedNotShow)
                                {
                                    if (isPrint)
                                    {
                                        if (_isCreatImages)
                                        {
                                            _isCreatImages = false;
                                        }
                                    }

                                    //可能是合并的单元格，所以用单元格的宽度来作为输入框大小
                                    if (tableInforPara.Cells[i + fieldIndex, j].IsMerged())
                                    {
                                        tableInforPara.Columns[j].RTBE.Size = new Size(tableInforPara.Cells[i + fieldIndex, j].CellSize.Width - tableInforPara.RtbeDiff.X * 2, tableInforPara.Cells[i + fieldIndex, j].CellSize.Height - tableInforPara.RtbeDiff.Y * 2);

                                        //如果是纵向合并的单元格，会变成默认的总是居左                
                                        if (!tableInforPara.Columns[j].RTBE.Multiline && tableInforPara.Cells[i + fieldIndex, j].ColSpan == 0)
                                        {
                                            //纵向合并的
                                            tableInforPara.Columns[j].RTBE.Multiline = true;
                                            tableInforPara.Columns[j].RTBE.Is_RowSpanMutiline = true;
                                        }
                                    }
                                    else
                                    {
                                        tableInforPara.Columns[j].RTBE.ResetSize();
                                    }

                                    orgLocation = new Point(tableInforPara.Cells[i + fieldIndex, j].Loaction.X + tableInforPara.RtbeDiff.X, tableInforPara.Cells[i + fieldIndex, j].Loaction.Y + tableInforPara.RtbeDiff.Y);

                                    //如果是红色字体，那么变成黑色斜体打印 PrintRedInsteadOfItalic
                                    if (_DSS.PrintRedInsteadOfItalic && isPrint)  //开启红色变黑色斜体，并且是打印的时候，才处理
                                    {
                                        //--------整个单元格的字为红色，才能处理
                                        if (tableInforPara.Columns[j].RTBE.SelectionColor == Color.Red)
                                        {
                                            //如果配置的字体在系统中没有Width="楷体_GB2312,9"，那么SelectionFont为null，就会报错(文字中含有汉字和数字的时候才会出现)
                                            tableInforPara.Columns[j].RTBE.SelectAll();
                                            tableInforPara.Columns[j].RTBE.SelectionColor = Color.Black;
                                            if (tableInforPara.Columns[j].RTBE.SelectionFont != null)
                                            {
                                                tableInforPara.Columns[j].RTBE.SelectionFont = new Font(tableInforPara.Columns[j].RTBE.SelectionFont.Name,
                                                                                            tableInforPara.Columns[j].RTBE.SelectionFont.Size, tableInforPara.Columns[j].RTBE.SelectionFont.Style | FontStyle.Italic);
                                            }
                                            else
                                            {
                                                tableInforPara.Columns[j].RTBE.SelectionFont = new Font(tableInforPara.Columns[j].RTBE.Font.Name,
                                                                                            tableInforPara.Columns[j].RTBE.Font.Size, tableInforPara.Columns[j].RTBE.Font.Style | FontStyle.Italic);
                                            }
                                        }
                                        //--------整个单元格的字为红色，才能处理

                                        ////--------单元格中个别文字为红色也处理：
                                        ////if (tableInforPara.Cells[i + fieldIndex, j].IsRtf_OR_Text) //行文字没法判断
                                        //for (int start = 0; start < tableInforPara.Columns[j].RTBE.Text.Length; start++)
                                        //{
                                        //    tableInforPara.Columns[j].RTBE.Select(start, 1);
                                        //    if (tableInforPara.Columns[j].RTBE.SelectionColor == Color.Red)
                                        //    {
                                        //        tableInforPara.Columns[j].RTBE.SelectionColor = Color.Black;
                                        //        tableInforPara.Columns[j].RTBE.SelectionFont = new Font(tableInforPara.Columns[j].RTBE.SelectionFont.Name,
                                        //                                                    tableInforPara.Columns[j].RTBE.SelectionFont.Size, tableInforPara.Columns[j].RTBE.SelectionFont.Style | FontStyle.Italic);
                                        //    }
                                        //}
                                        ////--------单元格中个别文字为红色也处理：
                                    }

                                    //if (_TableInfor.Columns[j].RTBE.MultilineCell)
                                    //{
                                    //    //rtbeNewTransparent.SelectAll();
                                    //    //RichTextBoxExtended.SetLineSpace(rtbeNewTransparent, 2); //设置较小的行间距
                                    //    //tableInforPara.Columns[j].RTBE.SetVerticalCenter();

                                    //    //如果是多行的单元格，要保持垂直居中
                                    //    //if (rtbe.MultilineCell)
                                    //    //{
                                    //    //    //if(rtbe.Lines.Length == 
                                    //    orgLocation.Y += (int)((_TableInfor.Columns[j].RTBE.Height - _TableInfor.Columns[j].RTBE.Font.Size * _TableInfor.Columns[j].RTBE.Lines.Length) / 2);
                                    //    //}
                                    //}

                                    //打印单元格文字内容
                                    printRtbeExtend(tableInforPara.Columns[j].RTBE, g, orgLocation);

                                    if (!_isCreatImages)
                                    {
                                        _isCreatImages = true;
                                    }
                                }

                                //最后绘制对角线，防止对角线被输入框挡住
                                if (tableInforPara.Cells[i + fieldIndex, j].DiagonalLineZ)
                                {
                                    DrawLineZN(i + fieldIndex, j, g, "z", tableInforPara);
                                }
                                else if (tableInforPara.Cells[i + fieldIndex, j].DiagonalLineN)
                                {
                                    DrawLineZN(i + fieldIndex, j, g, "n", tableInforPara);
                                }

                                //if (tableInforPara.Cells[i + fieldIndex, j].EditHistory != "")
                                //{
                                //    //绘制修订的提示：
                                //    //g.FillPolygon(sBrrsh, new Point[]{new Point(220,10),new Point(300,10),new Point(230,100)});
                                //    g.FillPolygon(sBrrsh, new Point[] { new Point(tableInforPara.Cells[i + fieldIndex, j].Loaction.X + _TableInfor.Cells[i + fieldIndex, j].CellSize.Width, _TableInfor.Cells[i + fieldIndex, j].Loaction.Y + _TableInfor.Cells[i + fieldIndex, j].CellSize.Height - locDiff), 
                                //    new Point(_TableInfor.Cells[i + fieldIndex, j].Loaction.X + _TableInfor.Cells[i + fieldIndex, j].CellSize.Width - locDiff, _TableInfor.Cells[i + fieldIndex, j].Loaction.Y + _TableInfor.Cells[i + fieldIndex, j].CellSize.Height),
                                //    new Point(_TableInfor.Cells[i + fieldIndex, j].Loaction.X + _TableInfor.Cells[i + fieldIndex, j].CellSize.Width, _TableInfor.Cells[i + fieldIndex, j].Loaction.Y + _TableInfor.Cells[i + fieldIndex, j].CellSize.Height) });
                                //}

                                if (isPrint)
                                {
                                    //恢复 防止事件处理，打印所有页的时候会明显变慢
                                    _IsLoading = tempIsLoading;
                                    //tableInforPara.Columns[j].RTBE._IsPrintAll = tempIsPrintAll; //恢复
                                }
                            }
                            catch (Exception ex)
                            {
                                if (isPrint)
                                {
                                    //恢复 防止事件处理，打印所有页的时候会明显变慢
                                    _IsLoading = tempIsLoading;
                                    //tableInforPara.Columns[j].RTBE._IsPrintAll = tempIsPrintAll; //恢复
                                }

                                Comm.LogHelp.WriteErr(ex);
                                throw ex;
                            }
                        }

                        //后绘制行的自定义线：双红线,这样单元格设置的边框线会被行的自定义线给覆盖掉上下的线
                        if (tableInforPara.Rows[i + fieldIndex].RowLineColor != Color.Black)
                        {
                            DrawRowLine(i + fieldIndex, 0, tableInforPara.Rows[i + fieldIndex].RowLineColor, g, tableInforPara);
                        }

                        if (tableInforPara.Rows[i + fieldIndex].CustomLineType != LineTypes.None)
                        {
                            DrawCustomRowLine(i + fieldIndex, 0, tableInforPara.Rows[i + fieldIndex].CustomLineType, g, tableInforPara, tableInforPara.Rows[i + fieldIndex].CustomLineColor);
                        }
                    }
                }

                _isCreatImages = false;

                //for (int i = 0; i < _TableInfor.RowsCount; i++)
                //{
                //    for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                //    {
                //        //绘制行和单元格的修改后的边框线样式：
                //        //单元格：
                //        if (_TableInfor.Cells[i, j].CellLineColor != Color.Black)
                //        {
                //            SetCellLine(i, j, _TableInfor.Cells[i, j].CellLineColor);
                //        }
                //    }
                //}


                //页尾斜线
                DrawPageEndCrossLine(pagePara, (_RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))) as XmlElement)?.GetAttribute("PageEndCrossLine"), g, tableInforPara);


                sBrrsh.Dispose();
            }
        }

        /// <summary>打印输入框
        /// orgLocation为rtbe的父控件的坐标位置
        /// </summary>
        /// <param name="rtbe"></param>
        /// <param name="g"></param>
        /// <param name="orgLocation"></param>
        private void printRtbeExtend(RichTextBoxExtended rtbe, Graphics g, Point orgLocation)
        {
            Bitmap bmpBackTemp;
            int pTop, pBottom, pLeft, pRight;
            byte[] b;
            int moreWidth = 0;
            Bitmap bmpRtb;
            Graphics gRtb;

            //非表格判断
            if (orgLocation.X == -1)
            {
                orgLocation = rtbe.Location; //不是表格的时候，还是用本身的位置
            }

            //如果是空，跳出循环
            //if (rtbe.Text.Trim() == "") return;


            //如果是打印,开启了图片签名打印,打印需要打印图片
            if (rtbe.Name.StartsWith("签名") || rtbe.Name.StartsWith("记录者"))
            {

            }

            //如果为空那么不需要打印。但是如果是手膜签名的时候虽然有图片，但是还是为空的。所以还是需要绘制的
            if ((rtbe != null && rtbe.Text != string.Empty) || (_FingerPrintSign && (rtbe.Name.StartsWith("签名") || rtbe.Name.StartsWith("记录者"))) || rtbe.IsContainImg()) // && !string.IsNullOrEmpty(rtbe.CreateUser)
            //if (rtbe != null && rtbe.Text != string.Empty) ////if (rtbe.Text.Trim() == "" && !rtbe.Name.StartsWith("签名") && !rtbe.Name.StartsWith("记录者")) continue;  //手膜签名只有图片，Text为空格。
            {
                //判断如果文字过多，那么需要特殊处理
                moreWidth = rtbe.Width;
                b = Encoding.Default.GetBytes(rtbe.Text);
                if (((int)b.Length * rtbe.Font.Size * 0.7) > rtbe.Width && !rtbe.Multiline)
                {
                    moreWidth = (int)(b.Length * rtbe.Font.Size * 0.7);
                }

                //图片背景色
                bmpBackTemp = new Bitmap(moreWidth - 2, rtbe.Height - 2);

                int pageWidth = pictureBoxBackGround.Width;
                int pageHeight = pictureBoxBackGround.Height;
                if (GlobalVariable.PrintPageSize != Size.Empty)
                {
                    //pageWidth = GlobalVariable.PrintPageSize.Width;
                    //pageHeight = GlobalVariable.PrintPageSize.Height;
                }
                pTop = orgLocation.Y - 2; // 
                pBottom = pageHeight - pTop - rtbe.Height - 2;  // pictureBoxBackGround.Height - pTop - rtbe.Height - 2;
                pLeft = orgLocation.X - 1; //-1 避免打印出来的效果偏右
                pRight = orgLocation.X + moreWidth + 1;


                //1.单元格显示（直接在背景图上绘制图片） 2.绘制区域调用图片。 和打印的时候效果会有差异，需要判断分别绘制
                if (!_isCreatImages && !Comm._isCreatReportImages)
                {
                    //打印的时候
                    pTop = orgLocation.Y + 1; // 如果是表格，那么调整一下
                    pBottom = pageHeight - pTop - rtbe.Height + 1;

                    if (rtbe.Multiline)
                    {
                        ////新方法，支持打印多行的
                        //RichTextBoxDocument doc = new RichTextBoxDocument(rtbe);
                        //doc.printDraw(g, orgLocation.X, orgLocation.Y, rtbe.Size.Width, rtbe.Size.Height);

                        //再一个新方法：
                        //RichTextBoxPrinter.PrintUseG(rtbe, g, rtbe.Width + 2, rtbe.Height); 
                        _RichTextBoxPrinter.PrintUseG(rtbe, g, rtbe.Width + 2, rtbe.Height); //不 rtbe.Width + 2，可能会回行
                    }
                    else
                    {
                        //RichTextBoxPrinter.PrintUseG(rtbe, g, rtbe.Width + 2, rtbe.Height); //不 + 2，可能会回行

                        ////绘制输入框本身的内容：文字样式等
                        RichTextBoxToPicture.RTFtoBitmapTransparentRTB(rtbe, bmpBackTemp, g, pTop, pBottom, pLeft - 1, pRight - 2);
                    }

                    //绘制自定义线 打印方式 最后绘线，否则多行的，可能被挡住
                    if (rtbe._HaveDoubleLine || rtbe._HaveSingleLine)
                    {
                        rtbe.PrintCustomLines(g, false, orgLocation, Color.Red);
                    }
                }
                else
                {
                    //表格界面初始化等时候
                    //1.绘制输入框文字
                    bmpRtb = new Bitmap(rtbe.Width + 2, rtbe.Height); //+ 2防止打印正好,但是光标出来绘制的时候,边界遮挡
                    gRtb = Graphics.FromImage(bmpRtb);
                    gRtb.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//消除锯齿，平滑

                    //新方法，支持打印多行的      //如果为透明输入框打印，那么表格中的输入框，如果有上下标，就会阴影，看不清楚。                   
                    //try
                    //{
                    //RichTextBoxPrinter.PrintUseG_Location00(rtbe, gRtb, moreWidth + 2, rtbe.Height);//moreWidth + 2否则可能打印生成图片会回行
                    _RichTextBoxPrinter.PrintUseG_Location00(rtbe, gRtb, moreWidth + 2, rtbe.Height);


                    //如果是多行的单元格，要保持垂直居中
                    //if (rtbe.MultilineCell)
                    //{
                    //if(rtbe.Lines.Length == 
                    //    orgLocation.Y += (int)((rtbe.Height - rtbe.Font.Size * rtbe.Lines.Length) / 2); //单元格显示的时候
                    //}

                    g.DrawImage(bmpRtb, orgLocation.X, orgLocation.Y - 1, bmpRtb.Width, bmpRtb.Height); //orgLocation.Y - 1误差调整

                    //调试，生成的图片：防止图片过小，被截断下部分（多行的时候）
                    //bmpRtb.Save(Application.StartupPath + "\\cell.jpg");
                    bmpRtb.Dispose();

                    //2.再绘制双红线
                    if (rtbe._HaveSingleLine || rtbe._HaveDoubleLine)
                    {
                        bmpRtb = new Bitmap(rtbe.Width, rtbe.Height); //现在是打印单元格区域，完全打印 new Bitmap(rtbe.Width // moreWidth, rtbe.Height);
                        gRtb = Graphics.FromImage(bmpRtb);

                        gRtb.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//消除锯齿，平滑
                        rtbe.PrintCustomLines(gRtb, true, orgLocation, Color.Red); //先绘制图片，然后再绘制到目标图片上
                                                                                   //g.DrawImage(bmpRtb, orgLocation.X * 96 / 100, orgLocation.Y * 96 / 100, bmpRtb.Width, bmpRtb.Height);

                        g.DrawImage(bmpRtb, orgLocation.X, orgLocation.Y + 1, bmpRtb.Width, bmpRtb.Height); //orgLocation.Y + 1误差调整   
                        //bmpRtb.Save(@"d:\1.png", ImageFormat.Png);
                        //bmpRtb.Dispose();
                    }

                    gRtb.Dispose();
                }

                # region 注释掉之前的老代码
                //if (drawMode == 0)
                //{
                //    //绘制自定义线 打印方式
                //    rtbe.PrintCustomLines(g, false, orgLocation);
                //}
                //else if (drawMode == 1)
                //{
                //    //生成图片的时候 -- 按上述处理，位置会错乱 位置会不对，很奇怪 14.4和15.6等屏幕尺寸不一致导致的
                //    bmpRtb = new Bitmap(rtbe.Width, rtbe.Height);
                //    gRtb = Graphics.FromImage(bmpRtb);
                //    rtbe.PrintCustomLines(gRtb, true, orgLocation); //先绘制图片，然后再绘制到目标图片上
                //    g.DrawImage(bmpRtb, orgLocation.X * 96 / 100, orgLocation.Y * 96 / 100, bmpRtb.Width, bmpRtb.Height);

                //    bmpRtb.Dispose();
                //}
                //else if (drawMode == 2)
                //{
                //    //生成图片的时候 -- 按上述处理，位置会错乱 位置会不对，很奇怪 14.4和15.6等屏幕尺寸不一致导致的
                //    bmpRtb = new Bitmap(rtbe.Width, rtbe.Height);
                //    gRtb = Graphics.FromImage(bmpRtb);
                //    rtbe.PrintCustomLines(gRtb, true, orgLocation); //先绘制图片，然后再绘制到目标图片上
                //    g.DrawImage(bmpRtb, orgLocation.X, orgLocation.Y, bmpRtb.Width, bmpRtb.Height);

                //    bmpRtb.Dispose();
                //}

                //if (rtbe.Multiline)
                //{
                //    ////新方法，支持打印多行的
                //    //RichTextBoxDocument doc = new RichTextBoxDocument(rtbe);
                //    //doc.printDraw(g, orgLocation.X, orgLocation.Y, rtbe.Size.Width, rtbe.Size.Height);

                //    //再一个新方法：
                //    RichTextBoxPrinter.PrintUseG(rtbe, g, rtbe.Width, rtbe.Height);
                //}
                //else
                //{
                //    //绘制输入框本身的内容：文字样式等
                //    RtfToPicture.RTFtoBitmapTransparentRTB(rtbe, bmpBackTemp, g, pTop, pBottom, pLeft, pRight);
                //}
                # endregion 注释掉之前的老代码

                //释放临时图片
                bmpBackTemp.Dispose();
            }

        }

        #endregion

        # region 打印页状态
        /// <summary>
        /// 查看打印页状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Frm_PagePrintStatus _PagePrintStatusForm = null;
        string _PagePrintStatus = "";
        private void toolStripMenuItemPagePrintStatus_Click(object sender, EventArgs e)
        {
            try
            {
                if (_CurrentTemplateNameTreeNode != null && _CurrentPage != "" && _TemplateName != "")
                {
                    //已经关闭，或者尚未打开过
                    //if (_PagePrintStatusForm == null || _PagePrintStatusForm.IsDisposed)
                    //{
                    _PagePrintStatusForm = new Frm_PagePrintStatus(_PagePrintStatus, _CurrentTemplateNameTreeNode.Nodes.Count);

                    DialogResult res = _PagePrintStatusForm.ShowDialog();

                    if (res == DialogResult.OK)
                    {
                        //获取返回的设置结果：
                        string state = _PagePrintStatusForm.New_PagePrintStatus;

                        //如果不一样，表示做了修改，需要提示保存
                        if (state != _PagePrintStatus)
                        {
                            _PagePrintStatus = state;

                            XmlNode nodeXMLRoot = _RecruitXML.SelectSingleNode(nameof(EntXmlModel.NurseForm));
                            (nodeXMLRoot as XmlElement).SetAttribute(nameof(EntXmlModel.PagePrintStatus), _PagePrintStatus);
                            IsNeedSave = true;
                        }
                    }
                    //}
                    //else
                    //{
                    //    _PagePrintStatusForm.Show();
                    //    _PagePrintStatusForm.BringToFront(); //窗口最前面显示
                    //    _PagePrintStatusForm.Activate();
                    //}
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 打印页后，更新打印状态
        /// 1.保存已打印页
        /// 2.打印后自动保存数据
        /// </summary>
        //private bool _needSaveMsgFlg = true;
        private void DoAfterPrinted(object sender, EventArgs e)
        {
            try
            {
                bool savePrint = false; //保存页信息是否有变化
                int startPage = _startPage;
                int endPage = _endPage;
                //可能是打印多页的时候，点击的打印当前页，那么需要通过打印界面的所在相对页数和绝对页数进行计算
                //相对页数是从0～n      
                if (sender != null)
                {
                    int current = 0;
                    if (!int.TryParse(sender.ToString(), out current))
                    {
                        current = 0;
                    }

                    startPage = startPage + current;
                    endPage = startPage;
                }

                string oldPagePrintStatus = _PagePrintStatus;

                for (int i = startPage; i <= endPage; i++) //可满足打印全部页
                {
                    //打印后的奇数偶数页设置保存打印页状态
                    if (printAllTWD_Mode == 1 && i % 2 != 1)
                    {
                        continue;
                    }
                    else if (printAllTWD_Mode == 2 && i % 2 != 0)
                    {
                        continue;
                    }

                    XmlNode nodeXMLRoot = _RecruitXML.SelectSingleNode(nameof(EntXmlModel.NurseForm));
                    ArrayList list = ArrayList.Adapter(_PagePrintStatus.Split('¤'));
                    if (list.Contains(i.ToString()))
                    {

                    }
                    else
                    {
                        savePrint = true;

                        if (_PagePrintStatus == "")
                        {
                            _PagePrintStatus = i.ToString();
                        }
                        else
                        {
                            _PagePrintStatus += "¤" + i.ToString();
                        }

                        (nodeXMLRoot as XmlElement).SetAttribute(nameof(EntXmlModel.PagePrintStatus), _PagePrintStatus);
                    }
                }

                //打印的页已经打印过，而且不需要进行保存。那么不进行保存。
                if (oldPagePrintStatus == _PagePrintStatus && !IsNeedSave)
                {
                    return; //跳出，不需要进行保存，浪费性能，没有意义的操作
                }

                //如果打印了，之前没有打印的页，需要打印后自动保存到数据库
                //实现打印后自动保存
                if (toolStripSaveEnabled) //_IsNeedSave 但是还有打印页数的保存的
                {
                    _NeedSaveSuccessMsg = false; //打印时自动保存，不管是只是状态，还是数据都不提示消息。
                    SaveToDetail(IsNeedSave);   //如果只是保存打印状态，数据本身没有修改_IsNeedSave为false，那么不需要验证
                    _NeedSaveSuccessMsg = true;
                }
            }
            catch (Exception ex)
            {
                _NeedSaveSuccessMsg = true;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        # endregion 打印页状态
    }
}
