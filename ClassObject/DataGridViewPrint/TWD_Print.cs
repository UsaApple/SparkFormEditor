/*
 * *************************************************************
 TRT 2011年11月11日
 * 
 * 
 * 
 * 
 * 
 * 
 *新生儿体温单和产程图   dataGridView自定义打印方法    
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
using System.IO;
using System.Xml;

// This module contains two classes
// #1:  'TWD.DataGridView_Print': Adapted from "The DataGridViewPrinter Class", By Salan Al-Ani. 
//                        See http://www.codeproject.com/csharp/datagridviewprinter.asp 
// #2:  'TWD.Print': My own TWD.Print "wrapper"

namespace NurseForm.Editor.Ctrl
{
    #region DataGridView_Print

    /// <summary>
    /// This is a renamed, refactored and slightly modified version of DataGridViewPrinter
    /// originally created by Salan Al-Ani and downloaded from CodeProject (around Mar 10,2007)
    /// </summary>
    class DataGridView_Print
    {
        private string mNameOfClient = "Unknown";

        // The PrintDocument to be used for printing.
        PrintDocument mPrintDoc;
        PrintDialog mPrintDlg;
        Margins mPageMargins = new Margins(0, 0, 0, 0);
        Margins mCellMargins = new Margins(2, 2, 2, 2);

        private DataGridView mTheDataGridView; // The DataGridView Control which will be printed
        private PrintDocument mThePrintDocument; // The PrintDocument to be used for printing
        private bool mIsCenterOnPage; // Determine if the report will be printed in the Top-Center of the page

        private bool mIsWithTitle; // Determine if the page contain title text
        private string mTheTitleText; // The title text to be printed in each page (if IsWithTitle is set to true)
        private Font mTheTitleFont; // The font to be used with the title text (if IsWithTitle is set to true)
        private Color mTheTitleColor; // The color to be used with the title text (if IsWithTitle is set to true)

        private bool mIsWithPaging; // Determine if paging is used
        private bool mIsWithColHeaders = true; // Determines if header is printed
        private int mPageNumber; //was static

        private int mCurrentRow; // A static parameter that keep track on which Row (in the DataGridView control) that should be printed
        private float mCurrentY; // A parameter that keep track on the y coordinate of the page, so the next object to be printed will start from this y coordinate

        private int mPageWidth;
        private int mPageHeight;

        private float mColHeaderHeight;
        private List<float> mRowHeight;
        private List<float> mColumnWidthPadded;
        private float mTheDataGridViewWidth;
        private float mColumnWidthMin = 0.0f;   //in in 1/100th inches
        private float mColumnWidthMax = 300.0f;   //in in 1/100th inches

        // Maintain a generic list to hold start/stop points for the column printing
        // This will be used for wrapping in situations where the DataGridView will not fit on a single page
        private List<int[]> mColumnRange;
        private List<float> mColumnRangeWidth;
        private int mColumnRangeCur;
        public string myPrintType = "";

        //*****************************************************************************
        // Properties
        //*****************************************************************************
        /// <summary>
        /// This is used to set PrintDocument.Name (not the printed Title)
        /// </summary>
        public string NameOfClient
        {
            get { return mNameOfClient; }
            set { mNameOfClient = value; }
        }

        /// <summary>
        /// Max column width in inches
        /// </summary>
        public float ColumnWidthMax
        {
            get { return mColumnWidthMax/100f; }
            set { mColumnWidthMax = value*100f; }
        }

        /// <summary>
        /// Min column width in inches
        /// </summary>
        public float ColumnWidthMin
        {
            get { return mColumnWidthMin / 100f; }
            set { mColumnWidthMin = value * 100f; }
        }

        public Margins PageMargins
        {
            get { return mPageMargins; }
            set { mPageMargins = value; }
        }

        public string PageTitle
        {
            get { return mTheTitleText; }
            set { mTheTitleText = value; }
        }

        public Font PageTitleFont
        {
            get { return mTheTitleFont; }
            set { mTheTitleFont = value; }
        }

        public Color PageTitleColor
        {
            get { return mTheTitleColor; }
            set { mTheTitleColor = value; }
        }

        public Margins CellMargins
        {
            get { return mCellMargins; }
            set { mCellMargins = value; }
        }

        public bool ShowPageNumbers
        {
            get { return mIsWithPaging; }
            set { mIsWithPaging = value; }
        }

        public bool HorizontallyCenterOnPage
        {
            get { return mIsCenterOnPage; }
            set { mIsCenterOnPage = value; }
        }

        //*****************************************************************************
        // The class constructor
        //*****************************************************************************
        public DataGridView_Print(DataGridView aDataGridView,string myPrintTypePara)
        {

            myPrintType = myPrintTypePara;

            mTheDataGridView = aDataGridView;

            //bool mbCenterOnPage = true; //MsgBox.Confirm("Center on the page?");
            //bool mTitleShow = true;
            //string mTitleText = "Customers";
            //Font mTitleFont = new Font("Tahoma", 18, FontStyle.Bold, GraphicsUnit.Point);
            //Color mTitleColor = Color.Black;
            //bool mPagingEnabled = true;

            mIsCenterOnPage = true;
            mIsWithTitle = true;
            mTheTitleText = "Customers";
            mTheTitleFont = new Font("Tahoma", 12, FontStyle.Bold, GraphicsUnit.Point);
            mTheTitleColor = Color.Black;
            mIsWithPaging = true;

            DoInit();
        }

        public DataGridView_Print(bool isMe)
        {

        }

        public DataGridView_Print(DataGridView aDataGridView
                                    , PrintDocument aPrintDocument
                                    , bool CenterOnPage
                                    , bool WithTitle
                                    , string aTitleText
                                    , Font aTitleFont
                                    , Color aTitleColor
                                    , bool WithPaging)
        {
            mTheDataGridView = aDataGridView;
            mThePrintDocument = aPrintDocument;
            mIsCenterOnPage = CenterOnPage;
            mIsWithTitle = WithTitle;
            mTheTitleText = aTitleText;
            mTheTitleFont = aTitleFont;
            mTheTitleColor = aTitleColor;
            mIsWithPaging = WithPaging;

            DoInit();
        }

        //*****************************************************************************
        // The class constructor helper
        //*****************************************************************************
        private void DoInit()
        {
            if (mPrintDoc == null)
            {
                this.mPrintDoc = new PrintDocument();
                this.mPrintDoc.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.OnPrintPage);

                //打印机事件 弹出选择打印机
                this.mPrintDoc.BeginPrint += new System.Drawing.Printing.PrintEventHandler(this.BeginPrint);
            }

            mThePrintDocument = this.mPrintDoc;

            mPageNumber = 0;

            mRowHeight = new List<float>();
            mColumnWidthPadded = new List<float>();

            mColumnRange = new List<int[]>();
            mColumnRangeWidth = new List<float>();

            // Calculating the PageWidth and the PageHeight
            if (!mThePrintDocument.DefaultPageSettings.Landscape)
            {
                mPageWidth = mThePrintDocument.DefaultPageSettings.PaperSize.Width;
                mPageHeight = mThePrintDocument.DefaultPageSettings.PaperSize.Height;
            }
            else
            {
                mPageHeight = mThePrintDocument.DefaultPageSettings.PaperSize.Width;
                mPageWidth = mThePrintDocument.DefaultPageSettings.PaperSize.Height;
            }

            //--- Calculate the page margins
            //this.mPageMargins = mThePrintDocument.DefaultPageSettings.Margins;

            // Set current row to be printed is the first row in the DataGridView control
            mCurrentRow = 0;
        }

        //弹出选择打印机
        private void BeginPrint(object sender, PrintEventArgs e)
        {            
            //也可以把一些打印的参数放在此处设置 
            if (e.PrintAction == PrintAction.PrintToPrinter)
            {

                //FormsetupDialog设置的效果一样   多了页面设置的方式
                PrintDialog psd = new PrintDialog();
                psd.Document = this.mPrintDoc;
                if (psd.ShowDialog() != DialogResult.OK)
                {
                    e.Cancel = true;
                }
                else
                {
                    //设定打印纸张类型
                    //TiWenDan.setPaperSizeRawKind("体温单表格", mPrintDoc.DefaultPageSettings.PaperSize.RawKind.ToString());
                }
            }
        }

        //*****************************************************************************
        // The function that calculate the height of each row (including the header row), 
        // the width of each column (according to the longest text in all its cells 
        // including the header cell), and the whole DataGridView width
        //*****************************************************************************
        private void Calculate(Graphics g)
        {
            if (mPageNumber == 0) // Just calculate once
            {
                mColHeaderHeight = 0;

                mTheDataGridViewWidth = 0;

                //----------------------------------------------------------------
                // loop through all the COLUMNS
                //----------------------------------------------------------------
                bool bSaveRowHeight = true;

                for (int iCol = 0; iCol < mTheDataGridView.Columns.Count; iCol++)
                {
                    if (mTheDataGridView.Columns[iCol].Visible == false)
                        continue;

                    float colWidth = 0;

                    //----------------------------------------------------------------
                    // first do calc for the HEADER
                    //----------------------------------------------------------------
                    if(this.mIsWithColHeaders)
                    {
                        Font hdrFont;
                        SizeF hdrSize = new SizeF();
                        // get the header font
                        hdrFont = mTheDataGridView.ColumnHeadersDefaultCellStyle.Font;
                        if (hdrFont == null) // If there is no special HeaderFont style, then use the default DataGridView font style
                            hdrFont = mTheDataGridView.DefaultCellStyle.Font;

                        //--- get the header size
                        hdrSize = g.MeasureString(mTheDataGridView.Columns[iCol].HeaderText, hdrFont);

                        hdrSize.Width += this.mCellMargins.Left + this.mCellMargins.Right;
                        hdrSize.Height += this.mCellMargins.Top + this.mCellMargins.Bottom;
                        //--- the column should be at least as wide as the header (incl padding)
                        colWidth = hdrSize.Width;
                        if (mColHeaderHeight < hdrSize.Height)
                            mColHeaderHeight = hdrSize.Height;
                    }

                    //----------------------------------------------------------------
                    // then do calc for the ROWS
                    //----------------------------------------------------------------
                    //--- loop through all the ROWS in the current COLUMN
                    for (int jRow = 0; jRow < mTheDataGridView.Rows.Count; jRow++)
                    {
                        Font rowFont;
                        SizeF cellSize;// = new SizeF();

                        //--- get the CELL font
                        rowFont = mTheDataGridView.Rows[jRow].DefaultCellStyle.Font;
                        if (rowFont == null) // If the there is no special font style of the CurrentRow, then use the default one associated with the DataGridView control
                            rowFont = mTheDataGridView.DefaultCellStyle.Font;

                        //--- set the height of the row 行高设置
                        //    (for now, only use the first column)
                        if (bSaveRowHeight)
                        {
                            cellSize = g.MeasureString("Anything", rowFont);
                            float rowHt = cellSize.Height
                                        + this.mCellMargins.Top + this.mCellMargins.Bottom;
                            //mRowHeight.Add(rowHt);
                            mRowHeight.Add(mTheDataGridView.Rows[jRow].Height);//23
                           
                        }
                        //mRowHeight.Add(mTheDataGridView.Rows[jRow].Height);

                        //--- set the width of the column
                        string cellValue = mTheDataGridView[iCol,jRow].EditedFormattedValue.ToString();
                        //cellValue = mTheDataGridView.Rows[jRow].Cells[iCol].EditedFormattedValue.ToString();
                        cellSize = g.MeasureString(cellValue, rowFont);
                        if (colWidth < cellSize.Width)
                            colWidth = cellSize.Width;
                    }
                    //--- now save the width of the column
                    //if (mTheDataGridView.Columns[iCol].Visible) 
                    //    mTheDataGridViewWidth += colWidth;

                    colWidth += this.mCellMargins.Left + this.mCellMargins.Right;

                    //--- test for min
                    if (colWidth < mColumnWidthMin && mColumnWidthMin > 0)
                        colWidth = mColumnWidthMin;

                    //--- test for max
                    if (mColumnWidthMax > 0)
                    {
                        if (colWidth > mColumnWidthMax)
                            colWidth = mColumnWidthMax;
                    }
                    else
                    {
                        float maxPrintableWidth = this.DoGetPrintableWidth()
                                                - this.mTheDataGridViewWidth - 1;
                        if (colWidth > maxPrintableWidth)
                            colWidth = maxPrintableWidth;
                    }

                    mTheDataGridViewWidth += colWidth;

                    mColumnWidthPadded.Add(colWidth);

                    if (mRowHeight.Count > 0)
                    {
                        // assuming all cells in row are same height
                        bSaveRowHeight = false; 
                    }
                }

                //----------------------------------------------------------------
                // Define the start/stop column points based on the page width 
                // and the DataGridView Width. We will use this to determine the 
                // columns which are drawn on each page and how wrapping will be handled
                // By default, the wrapping will occurr such that the maximum number 
                // of columns for a page will be determine
                //----------------------------------------------------------------
                int col;
                int colStart = 0;
                for (col = 0; col < mTheDataGridView.Columns.Count; col++)
                {
                    if (mTheDataGridView.Columns[col].Visible)
                    {
                        colStart = col;
                        break;
                    }
                }

                int colEnd = mTheDataGridView.Columns.Count;
                for (col = mTheDataGridView.Columns.Count - 1; col >= 0; col--)
                {
                    if (mTheDataGridView.Columns[col].Visible)
                    {
                        colEnd = col + 1;
                        break;
                    }
                }

                float pageWidthUsed = 0f;
                float pagePrintableWidth = this.DoGetPrintableWidth();

                for (col = 0; col < mTheDataGridView.Columns.Count; col++)
                {
                    if (mTheDataGridView.Columns[col].Visible)
                    {
                        pageWidthUsed += mColumnWidthPadded[col];
                        // If the width is bigger than the page area, 
                        // then define a new column print range
                        if (pageWidthUsed > pagePrintableWidth)
                        {
                            pageWidthUsed -= mColumnWidthPadded[col];
                            mColumnRange.Add(new int[] { colStart, colEnd });
                            mColumnRangeWidth.Add(pageWidthUsed);
                            colStart = col;
                            pageWidthUsed = mColumnWidthPadded[col];
                        }
                    }
                    // Our end point is actually one index above the current index
                    colEnd = col + 1;
                }

                // Add the last set of columns
                mColumnRange.Add(new int[] { colStart, colEnd });
                mColumnRangeWidth.Add(pageWidthUsed);
                mColumnRangeCur = 0;
            }

#if !true
            {
                int n;
                string sMsg = "";
                for (n = 0; n < this.mColumnRangeWidth.Count; n++)
                {
                    sMsg = String.Format("n={0}\t Width={1}", n, this.mColumnRangeWidth[n]);
                    Debug.WriteLine(sMsg);
                }
                for (n = 0; n < this.mColumnWidthPadded.Count; n++)
                {
                    sMsg = String.Format("n={0}\t Width={1}", n, this.mColumnWidthPadded[n]);
                    Debug.WriteLine(sMsg);
                }
            }
#endif
        }

        //*****************************************************************************
        // Helper function
        //*****************************************************************************
        private int DoGetPrintableWidth()
        {
            int pagePrintWidth = this.mPageWidth 
                               - this.mPageMargins.Left 
                               - this.mPageMargins.Right;
            return pagePrintWidth;
        }

        //*****************************************************************************
        // Helper function
        //*****************************************************************************
        private int DoGetPrintableHeight()
        {
            int pagePrintHeight = this.mPageHeight 
                                - this.mPageMargins.Top 
                                - this.mPageMargins.Bottom;
            return pagePrintHeight;
        }

        //*****************************************************************************
        // The funtion that prints the page number
        //*****************************************************************************
        private void DrawPageNumber(Graphics g)
        {
            //mCurrentY = (float)this.mPageMargins.Top;
            float pagePrintWidth = this.DoGetPrintableWidth();

            //-------------------------------------------------------------
            // Printing the page number (if isWithPaging is set to true)
            //-------------------------------------------------------------
            if (mIsWithPaging)
            {
                mPageNumber++;
                string pageString = "第 " + mPageNumber.ToString() + " 页";

                StringFormat pageStringFormat = new StringFormat();
                pageStringFormat.Trimming = StringTrimming.Word;
                pageStringFormat.FormatFlags = StringFormatFlags.NoWrap
                                             | StringFormatFlags.LineLimit
                                             | StringFormatFlags.NoClip;
                //FormstringFormat.Alignment = StringAlignment.Far;
                pageStringFormat.Alignment = StringAlignment.Center;

                Font pageStringFont = new Font("宋体", 9
                                              , FontStyle.Regular
                                              , GraphicsUnit.Point);
                float pagePageStringHeight = g.MeasureString(pageString, pageStringFont).Height;
                float pageNumberX = (float)this.mPageMargins.Left;
                float pageNumberY = 0;
                bool bPrintAtBottom = true;
                if (bPrintAtBottom)
                {
                    pageNumberY = this.mPageHeight - this.mPageMargins.Bottom;
                    pageNumberY += pagePageStringHeight;
                }
                else
                {
                    pageNumberY = this.mPageMargins.Top;
                    pageNumberY -= pagePageStringHeight;
                }
                RectangleF pagePrintArea = new RectangleF(pageNumberX, pageNumberY,
                                                          pagePrintWidth, pagePageStringHeight);

                g.DrawString(pageString, pageStringFont, new SolidBrush(Color.DarkGray)
                                       , pagePrintArea, pageStringFormat);

                //mCurrentY += g.MeasureString(pageString, pageStringFont).Height;
            }
        }

        //*****************************************************************************
        // The funtion that prints the title
        //*****************************************************************************
        private void DrawPageTitle(Graphics g)
        {
            float pagePrintWidth = this.DoGetPrintableWidth();
            if (mIsWithTitle)
            {
                StringFormat titleFormat = new StringFormat();
                titleFormat.Trimming = StringTrimming.Word;
                titleFormat.FormatFlags = StringFormatFlags.NoWrap
                                        | StringFormatFlags.LineLimit
                                        | StringFormatFlags.NoClip;

                float titleHt = g.MeasureString(mTheTitleText, mTheTitleFont).Height;

                float titleX = (float)this.mPageMargins.Left;
                float titleY = this.mPageMargins.Top
                             - titleHt * 1.8f;
                if (mIsCenterOnPage)
                {
                    titleFormat.Alignment = StringAlignment.Center; //HCenter
                    //titleFormat.LineAlignment = StringAlignment.Near;
                }
                else
                {
                    titleFormat.Alignment = StringAlignment.Near;   //Left
                    //titleFormat.LineAlignment = StringAlignment.Near;
                }

                RectangleF titleRectangle = new RectangleF(titleX, titleY, 
                                                           pagePrintWidth, titleHt);

                g.DrawString(mTheTitleText, mTheTitleFont, new SolidBrush(mTheTitleColor)
                                            , titleRectangle, titleFormat);

            }
        }
        

        //*****************************************************************************
        // The funtion that prints the title, page number, and the header row
        //*****************************************************************************
        private void DrawHeader(Graphics g)
        {
            mCurrentY = (float)this.mPageMargins.Top;
            float pagePrintWidth = this.DoGetPrintableWidth();

            //-------------------------------------------------------------
            // Printing the page number (if isWithPaging is set to true)
            //-------------------------------------------------------------
#if false
            if (mIsWithPaging)
            {
                mPageNumber++;
                string pageString = "Page " + mPageNumber.ToString();

                StringFormat pageStringFormat = new StringFormat();
                pageStringFormat.Trimming = StringTrimming.Word;
                pageStringFormat.FormatFlags = StringFormatFlags.NoWrap
                                             | StringFormatFlags.LineLimit
                                             | StringFormatFlags.NoClip;
                pageStringFormat.Alignment = StringAlignment.Far;

                Font pageStringFont = new Font("Tahoma", 8
                                              , FontStyle.Regular
                                              , GraphicsUnit.Point);
                float pagePageStringHeight = g.MeasureString(pageString, pageStringFont).Height;
                RectangleF pagePrintArea = new RectangleF((float)this.mPageMargins.Left, mCurrentY,
                                                          pagePrintWidth, pagePageStringHeight);

                g.DrawString(pageString, pageStringFont, new SolidBrush(Color.Black), pagePrintArea, pageStringFormat);

                //mCurrentY += g.MeasureString(pageString, pageStringFont).Height;
            }

            //-------------------------------------------------------------
            // Printing the title (if mIsWithTitle is set to true)
            //-------------------------------------------------------------
            if (mIsWithTitle)
            {
                StringFormat titleFormat = new StringFormat();
                titleFormat.Trimming = StringTrimming.Word;
                titleFormat.FormatFlags = StringFormatFlags.NoWrap
                                        | StringFormatFlags.LineLimit
                                        | StringFormatFlags.NoClip;
                if (mIsCenterOnPage)
                    titleFormat.Alignment = StringAlignment.Center;
                else
                    titleFormat.Alignment = StringAlignment.Near;

                float titleHt = g.MeasureString(mTheTitleText, mTheTitleFont).Height;
                RectangleF titleRectangle 
                    = new RectangleF((float)this.mPageMargins.Left, mCurrentY,
                                     pagePrintWidth, titleHt);

                g.DrawString(mTheTitleText, mTheTitleFont, new SolidBrush(mTheTitleColor)
                                            , titleRectangle, titleFormat);

                mCurrentY += g.MeasureString(mTheTitleText, mTheTitleFont).Height;
            }
#endif

            //-------------------------------------------------------------
            // printing the header itself
            //-------------------------------------------------------------
            if(mIsWithColHeaders)
            {
                // --- Setting the HeaderFore style
                Color headerForeColor = mTheDataGridView.ColumnHeadersDefaultCellStyle.ForeColor;
                if (headerForeColor.IsEmpty) // If there is no special HeaderFore style, then use the default DataGridView style
                    headerForeColor = mTheDataGridView.DefaultCellStyle.ForeColor;
                SolidBrush headerForeBrush = new SolidBrush(headerForeColor);

                // --- Setting the HeaderBack style
                Color headerBackColor = mTheDataGridView.ColumnHeadersDefaultCellStyle.BackColor;
                if (headerBackColor.IsEmpty) // If there is no special HeaderBack style, then use the default DataGridView style
                    headerBackColor = mTheDataGridView.DefaultCellStyle.BackColor;
                SolidBrush headerBackBrush = new SolidBrush(headerBackColor);

                // --- Setting the LinePen that will be used to draw lines and rectangles (derived from the GridColor property of the DataGridView control)
                Pen theLinePen = new Pen(mTheDataGridView.GridColor, 1);

                // --- Setting the HeaderFont style
                Font headerFont = mTheDataGridView.ColumnHeadersDefaultCellStyle.Font;
                if (headerFont == null) // If there is no special HeaderFont style, then use the default DataGridView font style
                    headerFont = mTheDataGridView.DefaultCellStyle.Font;

                //-------------------------------------------------------------
                // Calculating and drawing the HeaderBounds        
                //-------------------------------------------------------------
                // --- Calculating the starting x coordinate that the printing process 
                //     will start from
                float currentX = (float)this.mPageMargins.Left;
                float colRangeWidth = mColumnRangeWidth[mColumnRangeCur];
                if (mIsCenterOnPage)
                {
                    currentX += (pagePrintWidth - colRangeWidth) / 2.0F;
                }

                //--- fill the background of the header area
                RectangleF headerBounds = new RectangleF(currentX, mCurrentY,
                                            colRangeWidth, mColHeaderHeight);
                //headerBounds.Inflate(this.mCellMargins.Left, this.mCellMargins.Top);
                g.FillRectangle(headerBackBrush, headerBounds);

                //--- Set the format that will be used to print each cell of the header row
                StringFormat cellFormat = new StringFormat();
                cellFormat.Trimming = StringTrimming.Word;
                cellFormat.FormatFlags = StringFormatFlags.NoWrap
                                       | StringFormatFlags.LineLimit
                                       | StringFormatFlags.NoClip;

                //--- Print each visible cell of the header row
                RectangleF cellBounds;
                float columnWidth;
                for (int iCol = (int)mColumnRange[mColumnRangeCur].GetValue(0);
                         iCol < (int)mColumnRange[mColumnRangeCur].GetValue(1);
                         iCol++)
                {
                    if (!mTheDataGridView.Columns[iCol].Visible)
                        continue; // If the column is not visible then ignore this iteration

                    columnWidth = mColumnWidthPadded[iCol];

                    //--- Check the CurrentCell alignment and apply it to the CellFormat
                    if (mTheDataGridView.ColumnHeadersDefaultCellStyle.Alignment.ToString().Contains("Right"))
                        cellFormat.Alignment = StringAlignment.Far;
                    else if (mTheDataGridView.ColumnHeadersDefaultCellStyle.Alignment.ToString().Contains("Center"))
                        cellFormat.Alignment = StringAlignment.Center;
                    else
                        cellFormat.Alignment = StringAlignment.Near;

                    //--- set the bounding rectangle for the header cell
                    cellBounds = new RectangleF(currentX, mCurrentY,
                                                columnWidth, mColHeaderHeight);
                    cellBounds.Offset(this.mCellMargins.Left, this.mCellMargins.Top);

                    // Printing the cell text
                    g.DrawString(mTheDataGridView.Columns[iCol].HeaderText, headerFont
                                        , headerForeBrush, cellBounds, cellFormat);

                    // Drawing the cell bounds
                    //   only if the HeaderBorderStyle is not None
                    if (mTheDataGridView.RowHeadersBorderStyle != DataGridViewHeaderBorderStyle.None)
                    {
                        g.DrawRectangle(theLinePen, currentX, mCurrentY,
                                                    columnWidth, mColHeaderHeight);
                    }

                    currentX += columnWidth;
                }

                mCurrentY += mColHeaderHeight;
            }
        }

        //*****************************************************************************
        // The function that print a bunch of rows that fit in one page
        // When it returns true, meaning that there are more rows still not printed, 
        //   so another PagePrint action is required
        // When it returns false, meaning that all rows are printed (the mCurrentRow 
        //   parameter reaches the last row of the DataGridView control) and no further 
        //   PagePrint action is required
        //*****************************************************************************
        private bool DrawRows(Graphics g)
        {
            //--- Set the LinePen that will be used to draw lines and rectangles 
            //    (derived from the GridColor property of the DataGridView control)
            Pen theLinePen = new Pen(mTheDataGridView.GridColor, 1);

            //--- Set the style parameters that will be used to print each cell
            Font rowFont;
            Color rowForeColor;
            Color rowBackColor;
            SolidBrush rowForeBrush;
            SolidBrush rowBackBrush;
            SolidBrush rowAlternatingBackBrush;

            //--- Set the format that will be used to print each cell
            StringFormat cellFormat = new StringFormat();
            cellFormat.Trimming = StringTrimming.Word;
            cellFormat.Trimming = StringTrimming.EllipsisCharacter;
            cellFormat.FormatFlags = StringFormatFlags.NoWrap 
                                   | StringFormatFlags.LineLimit;
                                       //| StringFormatFlags.LineLimit
                                       //| StringFormatFlags.NoClip;

            //--- Print each visible cell
            RectangleF rowBounds;
            float currentX;
            float columnWidth;

            bool bUpdateColumnCount = true;
            int nColsPrinted = 0;

            while (mCurrentRow < mTheDataGridView.Rows.Count)
            {
                float rowHeight = mRowHeight[mCurrentRow];

                if (mTheDataGridView.Rows[mCurrentRow].Visible) // Print the cells of the CurrentRow only if that row is visible
                {
                    //--- Set the row font style
                    rowFont = mTheDataGridView.Rows[mCurrentRow].DefaultCellStyle.Font;
                    if (rowFont == null) // If the there is no special font style of the CurrentRow, then use the default one associated with the DataGridView control
                        rowFont = mTheDataGridView.DefaultCellStyle.Font;

                    //--- Set the RowFore style
                    rowForeColor = mTheDataGridView.Rows[mCurrentRow].DefaultCellStyle.ForeColor;
                    //      If the there is no special RowFore style of the CurrentRow, 
                    //      then use the default one associated with the DataGridView control
                    if (rowForeColor.IsEmpty) 
                        rowForeColor = mTheDataGridView.DefaultCellStyle.ForeColor;
                    rowForeBrush = new SolidBrush(rowForeColor);

                    //--- Set the RowBack (for even rows) and the RowAlternatingBack (for odd rows) styles
                    rowBackColor = mTheDataGridView.Rows[mCurrentRow].DefaultCellStyle.BackColor;
                    //      If the there is no special RowBack style of the CurrentRow, 
                    //      then use the default one associated with the DataGridView control
                    if (rowBackColor.IsEmpty) 
                    {
                        rowBackBrush = new SolidBrush(mTheDataGridView.DefaultCellStyle.BackColor);
                        rowAlternatingBackBrush = new SolidBrush(mTheDataGridView.AlternatingRowsDefaultCellStyle.BackColor);
                    }
                    else 
                    {
                        // If the there is a special RowBack style of the CurrentRow, 
                        // then use it for both the RowBack and the RowAlternatingBack styles
                        rowBackBrush = new SolidBrush(rowBackColor);
                        rowAlternatingBackBrush = new SolidBrush(rowBackColor);
                    }

                    //--- Calculate the starting X coordinate that the printing process 
                    //    will start from
                    currentX = (float)this.mPageMargins.Left;
                    float pagePrintWidth = this.DoGetPrintableWidth();
                    float colRangeWidth = mColumnRangeWidth[mColumnRangeCur];
                    if (mIsCenterOnPage)
                        currentX += (pagePrintWidth - colRangeWidth) / 2.0F;

                    //--- Calculate the entire CurrentRow bounds                
                    rowBounds = new RectangleF(currentX, mCurrentY, 
                                               colRangeWidth, rowHeight);

                    //--- Fill the back of the CurrentRow
                    if (mCurrentRow % 2 == 0)
                        g.FillRectangle(rowBackBrush, rowBounds);
                    else
                        g.FillRectangle(rowAlternatingBackBrush, rowBounds);

                    //--- Print each visible cell of the CurrentRow                
                    int colStart = (int)mColumnRange[mColumnRangeCur].GetValue(0);
                    int colEnd = (int)mColumnRange[mColumnRangeCur].GetValue(1);
                    for (int iCol = colStart; iCol < colEnd; iCol++)
                    {
                        // If the cell is belong to invisible column, 
                        // then ignore this iteration
                        if (!mTheDataGridView.Columns[iCol].Visible)
                            continue;

                        Type cellType = mTheDataGridView.Columns[iCol].CellType;
                        bool bCellIsAnImage 
                            = (cellType.ToString() == "System.Windows.Forms.DataGridViewImageCell");
                        
                        // Adjust the height of the row if there is an image
                        if (bCellIsAnImage)     //REWORK
                        {
                            if (mTheDataGridView.Rows[this.mCurrentRow].Cells[iCol].Value != null)
                            {
                                Image mTempImage = (Image)mTheDataGridView.Rows[mCurrentRow].Cells[iCol].Value;
                                if (mTempImage.Height > rowHeight)
                                    rowHeight = mTempImage.Height;
                            }
                        }

                        // Check the CurrentCell alignment and apply it to the CellFormat
                        if (mTheDataGridView.Columns[iCol].DefaultCellStyle.Alignment.ToString().Contains("Right"))
                            cellFormat.Alignment = StringAlignment.Far;
                        else if (mTheDataGridView.Columns[iCol].DefaultCellStyle.Alignment.ToString().Contains("Center"))
                            cellFormat.Alignment = StringAlignment.Center;
                        else
                            cellFormat.Alignment = StringAlignment.Near;

                        columnWidth = mColumnWidthPadded[iCol];
                        RectangleF cellBounds = new RectangleF(currentX, mCurrentY, 
                                                               columnWidth, rowHeight);
                        
                        //Printing the cell text OR draw the image
                        if (bCellIsAnImage)
                        {
                            // Only try drawing the image if one is assigned to the cell
                            if (mTheDataGridView.Rows[mCurrentRow].Cells[iCol].Value != null)
                            {
                                Image img = (Image)mTheDataGridView.Rows[mCurrentRow].Cells[iCol].Value;
                                g.DrawImage(img, (float)currentX, (float)mCurrentY, 
                                                 (float)columnWidth, (float)rowHeight);
                            }
                        }
                        else
                        {
                            // There was no image, so just draw the text
                            //cellBounds.Inflate(-mCellMargins.Left, -mCellMargins.Top);   //REWORK
                            cellBounds.Offset(mCellMargins.Left, mCellMargins.Top);   //REWORK
                            string cellValue = mTheDataGridView.Rows[mCurrentRow].Cells[iCol].EditedFormattedValue.ToString();
                            g.DrawString(cellValue, rowFont, rowForeBrush, cellBounds, cellFormat);
                        }

                        //--- Draw the cell's bounds
                        if (mTheDataGridView.CellBorderStyle != DataGridViewCellBorderStyle.None) // Draw the cell border only if the CellBorderStyle is not None
                            g.DrawRectangle(theLinePen, currentX, mCurrentY, 
                                                        columnWidth, rowHeight);

                        currentX += columnWidth;
                        if(bUpdateColumnCount)
                            nColsPrinted++;
                    }
                    
                    bUpdateColumnCount = false;
                    mCurrentY += rowHeight;

                    // Checking if the CurrentY is exceeds the page boundries
                    // If so then exit the function and returning true 
                    // meaning another PagePrint action is required
                    //if ((int)mCurrentY > (mPageHeight - this.mPageMargins.Top - this.mPageMargins.Bottom))
                    if ((int)this.mCurrentY > this.DoGetPrintableHeight())
                    {
                        mCurrentRow++;
                        return true;
                    }
                }
                mCurrentRow++;
            }

            //if (true)
            //{
            //    string sMsg = String.Format("DrawRows:DGV.Rows={0}; Row={1}", mTheDataGridView.Rows.Count, this.mCurrentRow);
            //    Debug.WriteLine(sMsg);
            //}

            mCurrentRow = 0;
            // Continue to print the next group of columns
            mColumnRangeCur ++; //= nColsPrinted;

            if (mColumnRangeCur >= mColumnRange.Count) // Which means all columns are printed
            {
                mColumnRangeCur = 0;
                return false;
            }
            else
                return true;
        }

        //*****************************************************************************
        // The method that calls all other functions
        //*****************************************************************************
        private bool DrawDataGridView(Graphics g)
        {
            bool bContinue;
            try
            {
                Calculate(g);
                //batchForm.DrawReportHeader(g, myPrintType);
                DrawHeader(g);
                DrawPageTitle(g);
                DrawPageNumber(g);
                bContinue = DrawRows(g);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Operation failed: " + ex.Message.ToString(), Application.ProductName + " - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                bContinue = false;
            }
            return bContinue;
        }

        //*****************************************************************************
        //*****************************************************************************
        public bool SetupThePrinting(bool bPreview)
        {
            if (mPrintDlg == null)
            {
                mPrintDlg = new PrintDialog();
                //set the default
                mPrintDlg.AllowCurrentPage = false;
                mPrintDlg.AllowPrintToFile = false;
                mPrintDlg.AllowSelection = false;
                mPrintDlg.AllowSomePages = false;
                mPrintDlg.PrintToFile = false;
                mPrintDlg.ShowHelp = false;
                mPrintDlg.ShowNetwork = false;
                mPrintDlg.UseEXDialog = true;
                //mPrintDlg.PrinterSettings.
            }

            //--- now get the options from the user
            //if (DialogResult.OK == mPrintDlg.ShowDialog())
            //{
                mPrintDoc.DocumentName = this.mNameOfClient + ":" + this.PageTitle;
                PrinterAndPaper pp = new PrinterAndPaper();
                pp.setDefaultPrinterName(mPrintDlg.PrinterSettings, "Record");
                mPrintDoc.PrinterSettings = mPrintDlg.PrinterSettings;
                mPrintDoc.DefaultPageSettings = mPrintDlg.PrinterSettings.DefaultPageSettings;

                //获取上次设定的纸张类型 如果设定的打印机没有该纸张类型，那么设置无效。还是该打印机默认的纸张类型
                //mPrintDoc.DefaultPageSettings.PaperSize.RawKind = TiWenDan.getPaperSizeRawKind("表单", mPrintDoc.PrinterSettings.PrinterName); //这样直接指定，会赋值无效

                PaperSize ps = mPrintDoc.DefaultPageSettings.PaperSize;
                ps.RawKind = pp.getPaperSizeRawKind("表单表格", mPrintDlg.PrinterSettings.PrinterName);
                mPrintDoc.DefaultPageSettings.PaperSize = ps;
                
                if(this.mPageMargins.Left!=0 && this.mPageMargins.Top != 0)
                    mPrintDoc.DefaultPageSettings.Margins = mPageMargins;

                if (true)
                {
                    // Calculating the PageWidth and the PageHeight
                    if (!mPrintDoc.DefaultPageSettings.Landscape)
                    {
                        mPageWidth = mPrintDoc.DefaultPageSettings.PaperSize.Width;
                        mPageHeight = mPrintDoc.DefaultPageSettings.PaperSize.Height;
                    }
                    else
                    {
                        mPageHeight = mPrintDoc.DefaultPageSettings.PaperSize.Width;
                        mPageWidth = mPrintDoc.DefaultPageSettings.PaperSize.Height;
                    }

                    //--- Calculate the page margins边缘设置 边距空白
                    this.mPageMargins.Left = 50;
                    this.mPageMargins.Right = 50;
                    this.mPageMargins.Top = 80;
                    this.mPageMargins.Bottom = 50;
                }

                if (bPreview)
                {
                    PrintPreviewDialog previewDlg = new PrintPreviewDialog();

                    previewDlg.Document = this.mThePrintDocument;

                    PrintPreviewDialogEx.MakePrintPreviewDialogMaximized(previewDlg);

                    previewDlg.ShowDialog();
                }
                else
                {
                    this.mThePrintDocument.Print();
                }

                return true;
            //}
            //else
            //{
            //    return false;
            //}
        }

        private void OnPrintPage(object sender, PrintPageEventArgs e)
        {
            bool more = this.DrawDataGridView(e.Graphics);
            if (more == true)
                e.HasMorePages = true;
            else
            {
                this.mPageNumber = 0;    //reset it
                this.mCurrentRow = 0;
                this.mColumnRangeCur = 0;
                this.DoInit();
            }
        }

    }
    #endregion //DataGridView_Print
    
    #region TWD.Print Wrapper
    class Print
    {
        #region Enums
        public enum HowToPrintDGV
        {
            Everything,
            SelectedColumns,
            FillColumns
        }
        #endregion //Enums

        #region Member Variables (Private)
        //PrintDocument mPrintDoc;
        //PrintDialog mPrintDlg;

        private DataGridView mDGV;
        private bool mbPrint;
        public string myPrintType = "";
        //public static string myFirstRecordDate = "";
        private string msTitle = "";
        private bool mbPrintSetup;
        private bool mbPrintPreview;
        private HowToPrintDGV mPrintHowDGV = HowToPrintDGV.Everything;
        #endregion //Member Variables (Private)

        #region Properties (Public)
        /// <summary>
        /// Set or Get the DataGridView [to be] printed
        /// </summary>
        public DataGridView PrintWhichDGV
        {
            get { return mDGV; }
            set { mDGV = value; }
        }

        public HowToPrintDGV PrintHowDGV
        {
            get { return mPrintHowDGV; }
            set { mPrintHowDGV = value; }
        }
        private string msPrintJobName = "TWD.Print";

        /// <summary>
        /// Name of the print job displayed by Windows, Drivers, or the Printer itself, etc.
        /// </summary>
        public string PrintJobName
        {
            get { return msPrintJobName; }
            set { msPrintJobName = value; }
        }

        /// <summary>
        /// Determines if the Print Setup Dialog is displayed before printing or previewing
        /// </summary>
        public bool PrintSetup
        {
            get { return mbPrintSetup; }
            set { mbPrintSetup = value; }
        }

        /// <summary>
        /// Determines if Previewing occurs before printing
        /// </summary>
        public bool PrintPreview
        {
            get { return mbPrintPreview; }
            set { mbPrintPreview = value; }
        }

        /// <summary>
        /// Name of the print job displayed by Windows, Drivers, or the Printer itself, etc.
        /// </summary>
        public string PrintTitle
        {
            get { return msTitle; }
            set { msTitle = value; }
        }

        ////myPrintType 打印的是什么，"抄录单"
        //public string MyPrintType
        //{
        //    get { return myPrintType; }
        //    set { myPrintType = value; }
        //}

        ////表格的第一行日期
        //public string MyFirstRecordDate
        //{
        //    get 
        //    {
        //        //string myDate = "";
        //        //if (mDGV != null && mDGV.Rows.Count > 0 && mDGV.Columns.Contains("日期") && mDGV.Columns.Contains("时间"))
        //        //{
        //        //    myDate = mDGV.Rows[0].Cells["日期"].Value.ToString() + " " + mDGV.Rows[0].Cells["时间"].Value.ToString();
        //        //}

        //        return myFirstRecordDate; 
        //    }
        //    set { myFirstRecordDate = value; }
        //}
        #endregion //Properties (Public)

        #region Constructors
        /// <summary>
        /// Constructor (mainly for using PrintControl)
        /// </summary>
        public Print()
        {
        }

        /// <summary>
        /// Constructor for printing contents of a DataGridView
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="bSetup"></param>
        /// <param name="bPreview"></param>
        /// <param name="bPrint"></param>
        public Print(DataGridView dgv, bool bSetup, bool bPreview, bool bPrint)
        {
            this.mDGV = new DataGridView();
            this.mDGV.Rows.Clear();

            foreach (DataGridViewColumn xe in dgv.Columns)
            {
                if (xe.Visible)
                {
                    this.mDGV.Columns.Add((DataGridViewColumn)xe.Clone());
                }
            }

            //最后要不要空白行:这个打印dgv如果有添加行，就会最后有一个空行
            this.mDGV.AllowUserToAddRows = false;

            int rowIndex = 0;
            foreach (DataGridViewRow dr in dgv.Rows)
            {
                //如果不是自定义，dgv有列隐藏的话，就会报索引溢出
                if ((dr.Visible && !dr.IsNewRow) && ((dgv.MultiSelect && dr.Selected) || !dgv.MultiSelect))
                {
                    this.mDGV.Rows.Add();

                    foreach (DataGridViewColumn xe in dgv.Columns)
                    {
                        if (xe.Visible)
                        {
                            this.mDGV.Rows[rowIndex].Cells[xe.Name].Value = dr.Cells[xe.Name].Value;
                        }
                    }

                    rowIndex++;
                }
            }


            this.mbPrintSetup = bSetup;
            this.mbPrintPreview = bPreview;
            this.mbPrint = bPrint;

            System.GC.Collect();        
        }
        #endregion //Constructors

        #region Methods (Public)
        /// <summary>
        /// Prints the contents of the DataGridView 
        /// </summary>
        /// <returns></returns>
        public bool DoPrintDGV()
        {
            bool bRet = false;
            string sMsg = "";
            string sEOL = Environment.NewLine;
            if (this.mDGV != null)
            {
                switch (this.mPrintHowDGV)
                {
                    default:
                    case HowToPrintDGV.Everything:

                        DataGridView_Print dgvPrint = new DataGridView_Print(this.mDGV, myPrintType);
                        dgvPrint.NameOfClient = this.PrintJobName;
                        dgvPrint.PageTitle = this.PrintTitle;
                        if (dgvPrint != null)
                        {
                            bRet = dgvPrint.SetupThePrinting(this.mbPrintPreview);
                        }
                        break;

                    case HowToPrintDGV.SelectedColumns:
                        // http://www.codeproject.com/csharp/PrintDataGridView.asp
                        PrintDGV pdgv = new PrintDGV();
                        bRet = pdgv.Print_DataGridView(this.mDGV, this.PrintTitle, true, myPrintType);
                        //bRet = PrintDGV.Print_DataGridView(this.mDGV, this.PrintTitle, true, myPrintType);
                        break;
                    case HowToPrintDGV.FillColumns:
                        //http://www.codeproject.com/csharp/PrintDataGridView.asp
                        PrintDGV pdgv2 = new PrintDGV();
                        bRet = pdgv2.Print_DataGridView(this.mDGV, this.PrintTitle, false, myPrintType);
                        //bRet = PrintDGV.Print_DataGridView(this.mDGV, this.PrintTitle, false, myPrintType);
                        break;
                }
            }
            else
            {
                sMsg = "需要初始化表格，" + sEOL;
                MsgBox.Error(sMsg);
                //Debug.WriteLine(sMsg);
            }

            sMsg += String.Format("TWD.Print successful? {0} ", bRet.ToString());
            //Debug.WriteLine(sMsg);
            return bRet;
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

            if (printDlg != null)
            {
                printDlg.PrinterSettings = printDocSettings;

                //--- now get the options from the user
                if (DialogResult.OK != printDlg.ShowDialog())
                {
                    return false;
                }
                else
                {
                    //--- update the settings
                    printDocSettings = printDlg.PrinterSettings;
                    return true;
                }
            }

            printDlg.Dispose();

            return false;
        }

        #endregion //Methods (Public)

    }
    #endregion //TWD.Print Wrapper
}
