/* 新表单
 * *************************************************************
 TRT 2013年6月28日 -2013年8月8日
 * 
 * 
 * 打印dgv
 * 
 *  
 * *************************************************************
*/
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;

// This module contains code ...
//      Adapted from "DataGridView Printing by Selecting Columns and Rows", By Afrasiab Cheraghi
//      See http://www.codeproject.com/csharp/PrintDataGridView.asp

namespace NurseForm.Editor.Ctrl
{
    #region PrintDGV
    class PrintDGV
    {
        private  StringFormat HdrFormat;  // For column header
        private  StringFormat StrFormat;  // Holds content of a TextBox Cell to write by DrawString
        private  StringFormat StrFormatComboBox; // Holds content of a Boolean Cell to write by DrawImage
        private  Button CellButton;       // Holds the Contents of Button Cell
        private  CheckBox CellCheckBox;   // Holds the Contents of CheckBox Cell 
        private  ComboBox CellComboBox;   // Holds the Contents of ComboBox Cell

        private  int TotalWidth;          // Summation of Columns widths
        private  int RowPos;              // Position of currently printing row 
        private  bool NewPage;            // Indicates if a new page reached
        private  int PageNo;              // Number of pages to print
        private  ArrayList ColumnLefts = new ArrayList();  // Left Coordinate of Columns
        private  ArrayList ColumnWidths = new ArrayList(); // Width of Columns
        private  ArrayList ColumnTypes = new ArrayList();  // DataType of Columns
        private  int CellHeight;          // Height of DataGrid Cell
        private  int RowsPerPage;         // Number of Rows per Page
        private  PrintDocument printDoc = new PrintDocument();  // PrintDocumnet Object used for printing

        private  string PrintTitle = "";  // Header of pages
        private  DataGridView dgv;        // Holds DataGridView Object to print its contents
        private  List<string> SelectedColumns = new List<string>();   // The Columns Selected by user to print.
        private  List<string> AvailableColumns = new List<string>();  // All Columns avaiable in DataGrid 
        private  bool PrintAllRows = true;   // True = print all rows,  False = print selected rows    
        private  bool FitToPageWidth = true; // True = Fits selected columns to page width ,  False = Print columns as showed    
        private  int HeaderHeight = 0;

        private  bool mbPrintDate = false;
        private  string myPrintType = "";

        public PrintDGV()
        {
            // Showing the Print Preview Page
            printDoc.BeginPrint += new System.Drawing.Printing.PrintEventHandler(PrintDoc_BeginPrint);
            printDoc.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(PrintDoc_PrintPage);
        }

        public bool Print_DataGridView(DataGridView dgv1, string sTitle, bool selectCol, string myPrintTypePara)
        {
            bool bRet = false;
            bool is1Page2 = false;

            try
            {
                myPrintType = myPrintTypePara;

                //取得打印对象：DataGridView
                if (selectCol)
                {
                    //自定义打印
                    dgv = dgv1;

                    AvailableColumns.Clear();
                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        if (!col.Visible)
                            continue;

                        AvailableColumns.Add(col.HeaderText);
                    }

                    // Showing the PrintOption Form
                    PrintOptions dlgPrintOptions = new PrintOptions(AvailableColumns);
                    dlgPrintOptions.PrintTitle = sTitle;
                    if (DialogResult.OK != dlgPrintOptions.ShowDialog())
                        return bRet;

                    PrintTitle = dlgPrintOptions.PrintTitle;
                    PrintAllRows = dlgPrintOptions.PrintAllRows;
                    FitToPageWidth = dlgPrintOptions.FitToPageWidth;
                    SelectedColumns = dlgPrintOptions.GetSelectedColumns();
                    is1Page2 = dlgPrintOptions.Get1Page2();
                }
                else
                {
                    //列宽自适应，但是不弹出选择设置对话框
                    FitToPageWidth = true;
                    SelectedColumns.Clear();
                    dgv = dgv1;
                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        if (!col.Visible)
                            continue;

                        SelectedColumns.Add(col.HeaderText);
                    }

                    PrintAllRows = true;
                    PrintTitle = "";
                }

                if (is1Page2)
                {
                    DataGridView mDGV = new DataGridView();
                    mDGV.Rows.Clear();

                    foreach (DataGridViewColumn xe in dgv.Columns)
                    {
                        if (xe.Visible)
                        {
                            mDGV.Columns.Add((DataGridViewColumn)xe.Clone());
                        }
                    }

                    //第二列重复
                    DataGridViewColumn dgvc;
                    foreach (DataGridViewColumn xe in dgv.Columns)
                    {
                        if (xe.Visible)
                        {
                            dgvc = new DataGridViewColumn();
                            dgvc = (DataGridViewColumn)xe.Clone();
                            dgvc.Name = xe.Name + "1";
                            dgvc.HeaderText = xe.HeaderText;//xe.Name; 用名字的话，血压记录单 等 ，如果名字和显示文字不一样就会漏掉不打印
                            mDGV.Columns.Add(dgvc);
                        }
                    }

                    //最后要不要空白行:这个打印dgv如果有添加行，就会最后有一个空行
                    mDGV.AllowUserToAddRows = false;

                    int rowIndex = 0;
                    int allCount = 0;
                    bool isLeftRight = true;
                    foreach (DataGridViewRow dr in dgv.Rows)
                    {
                        mDGV.Rows.Add();

                        if (isLeftRight)
                        {
                            foreach (DataGridViewColumn xe in dgv.Columns)
                            {
                                if (xe.Visible)
                                {
                                    mDGV.Rows[rowIndex].Cells[xe.Name].Value = dr.Cells[xe.Name].Value.ToString();
                                }
                            }
                        }
                        else
                        {

                            foreach (DataGridViewColumn xe in dgv.Columns)
                            {
                                if (xe.Visible)
                                {
                                    mDGV.Rows[rowIndex].Cells[xe.Name + "1"].Value = dr.Cells[xe.Name].Value.ToString();
                                }
                            }

                            rowIndex++;
                        }

                        isLeftRight = !isLeftRight;
                        allCount++;
                    }

                    if (allCount % 2 == 0)
                    {

                    }
                    else
                    {
                        foreach (DataGridViewColumn xe in dgv.Columns)
                        {
                            if (xe.Visible)
                            {
                                mDGV.Rows[rowIndex].Cells[xe.Name + "1"].Value = "　";
                            }
                        }
                    }

                    dgv = mDGV;

                    //选择列：
                    int count = SelectedColumns.Count;
                    for (int i = 0; i < count; i++)
                    {
                        SelectedColumns.Add(SelectedColumns[i] + "1");
                    }
                }

                RowsPerPage = 0;

                PrintPreviewDialog mdlgPrintPreview = new PrintPreviewDialog();

                //边距 左右余白
                PageSettings page = new PageSettings();
                page.Margins.Left = 30;
                //page.Margins.Right = 10;
                page.Margins.Top = 80;
                page.Margins.Bottom = 100;

                printDoc.DefaultPageSettings = page;
                
                //预览对话框最大化
                PrintPreviewDialogEx.MakePrintPreviewDialogMaximized(mdlgPrintPreview);
                         
                mdlgPrintPreview.Document = printDoc;
                //DoGetPrinterAndSettingsFromUser(mdlgPrintPreview.Document.PrinterSettings);

                //if (mdlgPrintPreview.ShowDialog() == DialogResult.OK)
                //{
                //    try
                //    {
                //        printDoc.Print();
                //        printDoc.BeginPrint -= new System.Drawing.Printing.PrintEventHandler(PrintDoc_BeginPrint);
                //        printDoc.PrintPage -= new System.Drawing.Printing.PrintPageEventHandler(PrintDoc_PrintPage);
                //        bRet = true;
                //    }
                //    catch
                //    {   //停止打印
                //        printDoc.PrintController.OnEndPrint(printDoc, new System.Drawing.Printing.PrintEventArgs());
                //    }
                //}
                //else
                //{
                //    printDoc.BeginPrint -= new System.Drawing.Printing.PrintEventHandler(PrintDoc_BeginPrint);
                //    printDoc.PrintPage -= new System.Drawing.Printing.PrintPageEventHandler(PrintDoc_PrintPage);
                //    return bRet;
                //}

                if (mdlgPrintPreview.ShowDialog() != DialogResult.OK)
                {
                    printDoc.BeginPrint -= new System.Drawing.Printing.PrintEventHandler(PrintDoc_BeginPrint);
                    printDoc.PrintPage -= new System.Drawing.Printing.PrintPageEventHandler(PrintDoc_PrintPage);
                    return bRet;
                }

                //Printing the Documnet
                //printDoc.Print();
                //printDoc.BeginPrint -= new System.Drawing.Printing.PrintEventHandler(PrintDoc_BeginPrint);
                //printDoc.PrintPage -= new System.Drawing.Printing.PrintPageEventHandler(PrintDoc_PrintPage);
                bRet = true;
            }
            catch (Exception ex)
            {
                Comm.Logger.WriteErr(ex);
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw ex;
            }
            finally
            {

            }

            return bRet;
        }

        private void PrintDoc_BeginPrint(object sender,
                    System.Drawing.Printing.PrintEventArgs e)
        {
            try
            {
                // Formatting the Content of Text Cell to print
                HdrFormat = new StringFormat();
                HdrFormat.Alignment = StringAlignment.Center;
                HdrFormat.LineAlignment = StringAlignment.Center;
                HdrFormat.FormatFlags = StringFormatFlags.NoWrap;
                HdrFormat.Trimming = StringTrimming.EllipsisCharacter;

                // Formatting the Content of Text Cell to print
                StrFormat = new StringFormat();
                StrFormat.Alignment = StringAlignment.Near;
                StrFormat.LineAlignment = StringAlignment.Center;
                StrFormat.FormatFlags = StringFormatFlags.NoWrap;
                StrFormat.Trimming = StringTrimming.EllipsisCharacter;

                // Formatting the Content of Combo Cells to print
                StrFormatComboBox = new StringFormat();
                StrFormatComboBox.LineAlignment = StringAlignment.Center;
                StrFormatComboBox.FormatFlags = StringFormatFlags.NoWrap;
                StrFormatComboBox.Trimming = StringTrimming.EllipsisCharacter;

                ColumnLefts.Clear();
                ColumnWidths.Clear();
                ColumnTypes.Clear();
                CellHeight = 0;
                RowsPerPage = 0;

                // For various column types
                CellButton = new Button();
                CellCheckBox = new CheckBox();
                CellComboBox = new ComboBox();

                // Calculating Total Widths
                TotalWidth = 0;
                foreach (DataGridViewColumn GridCol in dgv.Columns)
                {
                    if (!GridCol.Visible) continue;
                    if (!SelectedColumns.Contains(GridCol.HeaderText)) continue;
                    TotalWidth += GridCol.Width;
                }
                PageNo = 1;
                NewPage = true;
                RowPos = 0;

                ////获取上次设定的纸张类型
                //printDoc.DefaultPageSettings.PaperSize.RawKind = TiWenDan.getPaperSizeRawKind("表单表格");

                //Application.DoEvents();//未处理 System.OutOfMemoryExceptionMessage: 未处理的“System.OutOfMemoryException”类型的异常出现在 mscorlib.dll 中。
                //Application.DoEvents();//发现某类打印机，可能会报内存溢出System.AccessViolationException: 尝试读取或写入受保护的内存。这通常指示其他内存已损坏。
                PrinterAndPaper pp = new PrinterAndPaper();

                //把一些打印的参数放在此处设置 ，打印时刻选择打印机
                if (e.PrintAction == PrintAction.PrintToPrinter)
                {
                    //FormsetupDialog设置的效果一样   多了页面设置的方式
                    PrintDialog psd = new PrintDialog();

                    pp.setDefaultPrinterName(psd.PrinterSettings, "Record");  //特殊打印机在setDefaultPrinterName指定名字就会报内存溢出：imageWrite

                    //printDoc.PrinterSettings = psd.PrinterSettings;
                    psd.PrinterSettings = printDoc.PrinterSettings;
                    psd.Document = printDoc;
                    //psd.Document = new PrintDocument(); //不打印，而只获取参数

                    if (psd.ShowDialog() != DialogResult.OK)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        //mdlgPrintPreview.Document = null;
                        PrinterSettings ps = new PrinterSettings();
                        ps.PrinterName = psd.PrinterSettings.PrinterName;//dlg.PrinterSettings.DefaultPageSettings
                        ps.MinimumPage = ps.FromPage = 1;
                        ps.MaximumPage = ps.ToPage = 1;

                        printDoc.PrinterSettings = ps;
                        printDoc.DefaultPageSettings.PrinterSettings = printDoc.PrinterSettings;
                        printDoc.DefaultPageSettings.PaperSize = psd.Document.DefaultPageSettings.PaperSize;

                        //e.Cancel = true;
                        //printDoc.BeginPrint -= new System.Drawing.Printing.PrintEventHandler(PrintDoc_BeginPrint);
                        //printDoc.Print();
                        ////printDoc.BeginPrint -= new System.Drawing.Printing.PrintEventHandler(PrintDoc_BeginPrint);
                        //printDoc.BeginPrint += new System.Drawing.Printing.PrintEventHandler(PrintDoc_BeginPrint);
                        ////printDoc.Print();
                        
                    }              
                }
                else
                {
                    //预览
                    //指定预览打印机的名称为取得的设置名称，不然就是默认打印机了。
                    //其实像表单打印，一开始的printDoc已经设置好了系统默认打印机，选项配置的，这里也就不用再预览的时候再次指定的必要了
                    pp.setDefaultPrinterName(printDoc.PrinterSettings, "Record"); //特殊打印机在setDefaultPrinterName指定名字就会报内存溢出：imageWrite

                    //获取上次设定的纸张类型
                    PaperSize ps = printDoc.DefaultPageSettings.PaperSize;
                    ps.RawKind = pp.getPaperSizeRawKind("表单表格", printDoc.PrinterSettings.PrinterName);
                    printDoc.DefaultPageSettings.PaperSize = ps;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                printDoc.BeginPrint -= new System.Drawing.Printing.PrintEventHandler(PrintDoc_BeginPrint);
                printDoc.PrintPage -= new System.Drawing.Printing.PrintPageEventHandler(PrintDoc_PrintPage);
                //printDoc.Print();
                Comm.Logger.WriteErr(ex);
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw ex;
            }
        }

        private  void PrintDoc_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            int tmpWidth, i;

            //边距 左右余白
            //int tmpTop = e.MarginBounds.Top;
            int tmpTop = 80;
            //int tmpLeft = e.MarginBounds.Left;
            int tmpLeft = 50;

            try
            {
                ////绘制标题信息
                //if (myPrintType == "抄录单")
                //{
                //    //DataGridView_Print dgvPrint = new DataGridView_Print(false);
                //    batchForm.DrawReportHeader(e.Graphics, myPrintType);
                //}

                // Before starting first page, it saves Width & Height of Headers and CoulmnType
                if (PageNo == 1)
                {
                    foreach (DataGridViewColumn GridCol in dgv.Columns)
                    {
                        if (!GridCol.Visible) continue;
                        // Skip if the current column not selected
                        if (!SelectedColumns.Contains(GridCol.HeaderText)) continue;

                        // Detemining whether the columns are fitted to page or not.
                        if (FitToPageWidth)
                            tmpWidth = (int)(Math.Floor((double)((double)GridCol.Width /
                                       (double)TotalWidth * (double)TotalWidth *
                                       ((double)e.MarginBounds.Width / (double)TotalWidth))));
                        else
                            tmpWidth = GridCol.Width;

                        HeaderHeight = (int)(e.Graphics.MeasureString(GridCol.HeaderText,
                                    GridCol.InheritedStyle.Font, tmpWidth).Height) + 11; //引发类型为“System.ExecutionEngineException”的异常。

                        // Save width & height of headres and ColumnType
                        ColumnLefts.Add(tmpLeft);
                        ColumnWidths.Add(tmpWidth);
                        ColumnTypes.Add(GridCol.GetType());
                        tmpLeft += tmpWidth;
                    }
                }

                //逐行打印
                while (RowPos <= dgv.Rows.Count - 1)        //最后有空白行
                //while (RowPos < dgv.Rows.Count - 1)
                {
                    DataGridViewRow GridRow = dgv.Rows[RowPos];
                    if (GridRow.IsNewRow || (!PrintAllRows && !GridRow.Selected))
                    {
                        RowPos++;
                        continue;
                    }

                    //行高设置
                    CellHeight = GridRow.Height;

                    if (tmpTop + CellHeight >= e.MarginBounds.Height + e.MarginBounds.Top)
                    {
                        DrawFooter(e, RowsPerPage);
                        NewPage = true;
                        PageNo++;
                        e.HasMorePages = true;
                        return;
                    }
                    else
                    {
                        if (NewPage)
                        {
                            // Draw Header
                            Font font = new Font(dgv.Font, FontStyle.Bold);
                            Brush brush = Brushes.Black;
                            int pageW = e.MarginBounds.Width;
                            SizeF size = e.Graphics.MeasureString(PrintTitle, font, pageW);
                            float X = e.MarginBounds.Left;
                            if (pageW > size.Width)
                                   X+= (pageW - size.Width)/2;
                            float Y = e.MarginBounds.Top - e.Graphics.MeasureString(PrintTitle, new Font(dgv.Font, FontStyle.Bold), e.MarginBounds.Width).Height - 13;
                            e.Graphics.DrawString(PrintTitle, font, brush, X, Y);
                            //e.Graphics.DrawString(PrintTitle, font, brush, X, Y, HdrFormat);

                            if (mbPrintDate)
                            {
                                String s = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToShortTimeString();

                                e.Graphics.DrawString(s, new Font(dgv.Font, FontStyle.Bold),
                                        Brushes.Black, e.MarginBounds.Left + (e.MarginBounds.Width -
                                        e.Graphics.MeasureString(s, new Font(dgv.Font,
                                        FontStyle.Bold), e.MarginBounds.Width).Width), e.MarginBounds.Top -
                                        e.Graphics.MeasureString(PrintTitle, new Font(new Font(dgv.Font,
                                        FontStyle.Bold), FontStyle.Bold), e.MarginBounds.Width).Height - 13);
                            }
                            // Draw Columns
                            tmpTop = e.MarginBounds.Top;
                            i = 0;
                            foreach (DataGridViewColumn GridCol in dgv.Columns)
                            {
                                if (!GridCol.Visible) 
                                    continue;

                                if (!SelectedColumns.Contains(GridCol.HeaderText))
                                    continue;

                                e.Graphics.FillRectangle(new SolidBrush(Color.LightGray),
                                    new Rectangle((int)ColumnLefts[i], tmpTop,
                                    (int)ColumnWidths[i], HeaderHeight));

                                e.Graphics.DrawRectangle(Pens.Black,
                                    new Rectangle((int)ColumnLefts[i], tmpTop,
                                    (int)ColumnWidths[i], HeaderHeight));

                                e.Graphics.DrawString(GridCol.HeaderText
                                                    , GridCol.InheritedStyle.Font
                                                    , new SolidBrush(GridCol.InheritedStyle.ForeColor)
                                                    , new RectangleF((int)ColumnLefts[i], tmpTop,(int)ColumnWidths[i], HeaderHeight)
                                                    , HdrFormat);
                                i++;
                            }
                            NewPage = false;
                            tmpTop += HeaderHeight;
                        }

                        // Draw Columns Contents
                        i = 0;
                        string txt = "";
                        foreach (DataGridViewCell Cel in GridRow.Cells)
                        {
                            if (!Cel.OwningColumn.Visible) continue;
                            if (!SelectedColumns.Contains(Cel.OwningColumn.HeaderText))
                                continue;

                            if (Cel == null || Cel.Value == null)
                            {
                                txt = "";
                                continue;
                            }
                            else
                            {
                                txt = Cel.Value.ToString();
                            }

                            // For the TextBox Column
                            if (((Type)ColumnTypes[i]).Name == "DataGridViewTextBoxColumn" ||
                                ((Type)ColumnTypes[i]).Name == "DataGridViewLinkColumn")
                            {
                                e.Graphics.DrawString(Cel.Value.ToString(), Cel.InheritedStyle.Font,
                                        new SolidBrush(Cel.InheritedStyle.ForeColor),
                                        new RectangleF((int)ColumnLefts[i], (float)tmpTop,
                                        (int)ColumnWidths[i], (float)CellHeight), StrFormat);
                            }
                            // For the Button Column
                            else if (((Type)ColumnTypes[i]).Name == "DataGridViewButtonColumn")
                            {
                                CellButton.Text = Cel.Value.ToString();
                                CellButton.Size = new Size((int)ColumnWidths[i], CellHeight);
                                Bitmap bmp = new Bitmap(CellButton.Width, CellButton.Height);
                                CellButton.DrawToBitmap(bmp, new Rectangle(0, 0,
                                        bmp.Width, bmp.Height));
                                e.Graphics.DrawImage(bmp, new Point((int)ColumnLefts[i], tmpTop));
                            }
                            // For the CheckBox Column
                            else if (((Type)ColumnTypes[i]).Name == "DataGridViewCheckBoxColumn")
                            {
                                CellCheckBox.Size = new Size(14, 14);
                                CellCheckBox.Checked = (bool)Cel.Value;
                                Bitmap bmp = new Bitmap((int)ColumnWidths[i], CellHeight);
                                Graphics tmpGraphics = Graphics.FromImage(bmp);
                                tmpGraphics.FillRectangle(Brushes.White, new Rectangle(0, 0,
                                        bmp.Width, bmp.Height));
                                CellCheckBox.DrawToBitmap(bmp,
                                        new Rectangle((int)((bmp.Width - CellCheckBox.Width) / 2),
                                        (int)((bmp.Height - CellCheckBox.Height) / 2),
                                        CellCheckBox.Width, CellCheckBox.Height));
                                e.Graphics.DrawImage(bmp, new Point((int)ColumnLefts[i], tmpTop));
                            }
                            // For the ComboBox Column
                            else if (((Type)ColumnTypes[i]).Name == "DataGridViewComboBoxColumn")
                            {
                                CellComboBox.Size = new Size((int)ColumnWidths[i], CellHeight);
                                Bitmap bmp = new Bitmap(CellComboBox.Width, CellComboBox.Height);
                                CellComboBox.DrawToBitmap(bmp, new Rectangle(0, 0,
                                        bmp.Width, bmp.Height));
                                e.Graphics.DrawImage(bmp, new Point((int)ColumnLefts[i], tmpTop));
                                e.Graphics.DrawString(Cel.Value.ToString(), Cel.InheritedStyle.Font,
                                        new SolidBrush(Cel.InheritedStyle.ForeColor),
                                        new RectangleF((int)ColumnLefts[i] + 1, tmpTop, (int)ColumnWidths[i]
                                        - 16, CellHeight), StrFormatComboBox);
                            }
                            // For the Image Column
                            else if (((Type)ColumnTypes[i]).Name == "DataGridViewImageColumn")
                            {
                                Rectangle CelSize = new Rectangle((int)ColumnLefts[i],
                                        tmpTop, (int)ColumnWidths[i], CellHeight);
                                Size ImgSize = ((Image)(Cel.FormattedValue)).Size;
                                e.Graphics.DrawImage((Image)Cel.FormattedValue,
                                        new Rectangle((int)ColumnLefts[i] + (int)((CelSize.Width - ImgSize.Width) / 2),
                                        tmpTop + (int)((CelSize.Height - ImgSize.Height) / 2),
                                        ((Image)(Cel.FormattedValue)).Width, ((Image)(Cel.FormattedValue)).Height));

                            }

                            // Drawing Cells Borders 
                            e.Graphics.DrawRectangle(Pens.Black, new Rectangle((int)ColumnLefts[i],
                                    tmpTop, (int)ColumnWidths[i], CellHeight));

                            i++;

                        }
                        tmpTop += CellHeight;
                    }

                    RowPos++;
                    // For the first page it calculates Rows per Page
                    if (PageNo == 1) RowsPerPage++;
                }

                if (RowsPerPage == 0) return;

                // Write Footer (Page Number)
                DrawFooter(e, RowsPerPage);

                e.HasMorePages = false;
            }
            catch (Exception ex)
            {
                Comm.Logger.WriteErr(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private  void DrawFooter(System.Drawing.Printing.PrintPageEventArgs e,
                    int RowsPerPage)
        {
            double cnt = 0;

            // Detemining rows number to print
            if (PrintAllRows)
            {
                if (dgv.Rows[dgv.Rows.Count - 1].IsNewRow)
                    cnt = dgv.Rows.Count - 2; // When the DataGridView doesn't allow adding rows
                else
                    cnt = dgv.Rows.Count - 1; // When the DataGridView allows adding rows
            }
            else
                cnt = dgv.SelectedRows.Count;


            //string PageNum = "第 " + PageNo.ToString() + " / " + Math.Ceiling((double)(cnt / RowsPerPage)).ToString() + " 页";
            string PageNum = "第 " + PageNo.ToString() + " 页";

            e.Graphics.DrawString(PageNum, dgv.Font, Brushes.Black,
                e.MarginBounds.Left + (e.MarginBounds.Width -
                e.Graphics.MeasureString(PageNum, dgv.Font,
                e.MarginBounds.Width).Width) / 2, e.MarginBounds.Top +
                e.MarginBounds.Height + 31);
        }


        /// <summary>
        /// Displays a PrintDialog and allows the user to choose a printer
        /// and various settings (such as orientation)
        /// </summary>
        /// <param name="printDocSettings">PrinterSettings settings to be used by the PrintDocument</param>
        /// <returns>'true' if user clicked 'Print' in the PrintDialog, 'false' otherwise</returns>
        public bool DoGetPrinterAndSettingsFromUser(PrinterSettings printDocSettings)
        {
            PrintDialog printDlg = new PrintDialog();

            ////把一些打印的参数放在此处设置 ，打印时刻选择打印机
            //TiWenDan.setDefaultPrinterName(printDlg.PrinterSettings, "Batch");
            //printDocSettings = printDlg.PrinterSettings;

            if (printDlg != null)
            {
                printDlg.PrinterSettings = printDocSettings;

                //不要确定对话框
                //--- now get the options from the user
                //if (DialogResult.OK != printDlg.ShowDialog())
                //{
                //    return false;
                //}
                //else
                //{
                //--- update the settings
                printDocSettings = printDlg.PrinterSettings;

                return true;
                //}
            }

            return false;

            //return true;
        }



    }
    #endregion

}
