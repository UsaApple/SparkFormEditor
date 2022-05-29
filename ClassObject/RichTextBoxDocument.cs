using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices;

namespace Mandala.Recruit3
{
    class RichTextBoxDocument : PrintDocument
    {
        //---------------------------------------------------------------------------
        #region ** fields

        RichTextBox _rtb;
        static int _firstChar;
        int _currentPage;
        int _pageCount;

        // special tags for headers/footers
        const string PAGE = "[page]";
        const string PAGES = "[pages]";

        #endregion

        //---------------------------------------------------------------------------
        #region ** ctor

        public RichTextBoxDocument(RichTextBox rtb)
        {
            // store a reference to the RichTextBox to be rendered
            _rtb = rtb;

            // initialize header and footer
            Header = Footer = string.Empty;
            HeaderFont = FooterFont = new Font("Verdana", 9, FontStyle.Bold);
        }

        #endregion

        //---------------------------------------------------------------------------
        #region ** object model

        public string Header { get; set; }
        public string Footer { get; set; }
        public Font HeaderFont { get; set; }
        public Font FooterFont { get; set; }

        #endregion

        //---------------------------------------------------------------------------
        #region ** overrides

        // start printing the document
        protected override void OnBeginPrint(PrintEventArgs e)
        {
            // we haven't printed anything yet
            _firstChar = 0;
            _currentPage = 0;

            // check whether we need a page count
            _pageCount = Header.IndexOf(PAGES) > -1 || Footer.IndexOf(PAGES) > -1
                ? -1
                : 0;

            // fire event as usual
            base.OnBeginPrint(e);
        }

        // render a page into the PrintDocument
        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            // get a page count if that is required
            if (_pageCount < 0)
            {
                _pageCount = GetPageCount(e);
            }

            // update current page
            _currentPage++;

            // render text
            FORMATRANGE fmt = GetFormatRange(e, _firstChar);
            int nextChar = FormatRange(_rtb, true, ref fmt);
            e.Graphics.ReleaseHdc(fmt.hdc);

            //// render header
            //if (!string.IsNullOrEmpty(Header))
            //{
            //    var rc = e.MarginBounds;
            //    rc.Y = 0;
            //    rc.Height = e.MarginBounds.Top;
            //    RenderHeaderFooter(e, Header, HeaderFont, rc);
            //    e.Graphics.DrawLine(Pens.Black, rc.X, rc.Bottom, rc.Right, rc.Bottom);
            //}

            //// render footer
            //if (!string.IsNullOrEmpty(Footer))
            //{
            //    var rc = e.MarginBounds;
            //    rc.Y = rc.Bottom;
            //    rc.Height = e.PageBounds.Bottom - rc.Y;
            //    RenderHeaderFooter(e, Footer, FooterFont, rc);
            //    e.Graphics.DrawLine(Pens.Black, rc.X, rc.Y, rc.Right, rc.Y);
            //}

            // check whether we're done
            //e.HasMorePages = nextChar > _firstChar && nextChar < _rtb.TextLength;

            // save start char for next time
            //_firstChar = nextChar;

            // fire event as usual
            base.OnPrintPage(e);
        }

        public static void DrawRtb(Graphics g, RichTextBox rtb, int pTop, int pBottom, int pLeft, int pRight)
        {
            // render text
            FORMATRANGE fmt = GetFormatRange(g, _firstChar, pTop, pBottom, pLeft, pRight);
            int nextChar = FormatRange(rtb, true, ref fmt);
            g.ReleaseHdc(fmt.hdc);


        }

        static FORMATRANGE GetFormatRange(Graphics g, int firstChar, int pTop, int pBottom, int pLeft, int pRight)
        {
            // get page rectangle in twips
            var rc = new Rectangle();
            rc.X = (int)(pTop * 14.4 + .5);
            rc.Y = (int)(pBottom * 14.4 + .5);
            rc.Width = (int)(pLeft * 14.4 + .5);
            rc.Height = (int)(pRight * 14.40 + .5);

            // set up FORMATRANGE structure
            var hdc = g.GetHdc();
            var fmt = new FORMATRANGE();
            fmt.hdc = fmt.hdcTarget = hdc;
            fmt.rc.SetRect(rc);
            fmt.rcPage = fmt.rc;
            fmt.cpMin = firstChar;
            fmt.cpMax = -1;

            // done
            return fmt;
        }

        #endregion

        //---------------------------------------------------------------------------
        #region ** render headers and footers

        // render a header or a footer on the current page
        void RenderHeaderFooter(PrintPageEventArgs e, string text, Font font, Rectangle rc)
        {
            var parts = text.Split('\t');
            if (parts.Length > 0)
            {
                RenderPart(e, parts[0], font, rc, StringAlignment.Near);
            }
            if (parts.Length > 1)
            {
                RenderPart(e, parts[1], font, rc, StringAlignment.Center);
            }
            if (parts.Length > 2)
            {
                RenderPart(e, parts[2], font, rc, StringAlignment.Far);
            }
        }

        // render a part of a header or footer on the page
        void RenderPart(PrintPageEventArgs e, string text, Font font, Rectangle rc, StringAlignment align)
        {
            // replace wildcards
            text = text.Replace(PAGE, _currentPage.ToString());
            text = text.Replace(PAGES, _pageCount.ToString());

            // prepare string format
            StringFormat fmt = new StringFormat();
            fmt.Alignment = align;
            fmt.LineAlignment = StringAlignment.Center;

            // render footer
            e.Graphics.DrawString(text, font, Brushes.Black, rc, fmt);
        }

        #endregion

        //---------------------------------------------------------------------------
        #region ** implementation

        // build a FORMATRANGE structure with the proper page size and hdc
        // (the hdc must be released after the FORMATRANGE is used)
        FORMATRANGE GetFormatRange(PrintPageEventArgs e, int firstChar)
        {
            // get page rectangle in twips
            var rc = e.MarginBounds;
            rc.X = (int)(rc.X * 14.4 + .5);
            rc.Y = (int)(rc.Y * 14.4 + .5);
            rc.Width = (int)(rc.Width * 14.4 + .5);
            rc.Height = (int)(rc.Height * 14.40 + .5);

            // set up FORMATRANGE structure
            var hdc = e.Graphics.GetHdc();
            var fmt = new FORMATRANGE();
            fmt.hdc = fmt.hdcTarget = hdc;
            fmt.rc.SetRect(rc);
            fmt.rcPage = fmt.rc;
            fmt.cpMin = firstChar;
            fmt.cpMax = -1;

            // done
            return fmt;
        }

        // get a page count by using FormatRange to measure the content
        int GetPageCount(PrintPageEventArgs e)
        {
            int pageCount = 0;

            // count the pages using FormatRange
            FORMATRANGE fmt = GetFormatRange(e, 0);
            for (int firstChar = 0; firstChar < _rtb.TextLength; )
            {
                fmt.cpMin = firstChar;
                firstChar = FormatRange(_rtb, false, ref fmt);
                pageCount++;
            }
            e.Graphics.ReleaseHdc(fmt.hdc);

            // done
            return pageCount;
        }

        #endregion

        //---------------------------------------------------------------------------
        #region ** Win32 stuff

        // messages used by RichEd20.dll
        const int
            WM_USER = 0x400,
            EM_FORMATRANGE = WM_USER + 0x39;

        // Win32 RECT
        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left, top, right, bottom;
            public void SetRect(Rectangle rc)
            {
                left = rc.Left;
                top = rc.Top;
                right = rc.Right;
                bottom = rc.Bottom;
            }
        }

        // FORMATRANGE is used by RichEd20.dll to render RTF
        [StructLayout(LayoutKind.Sequential)]
        struct FORMATRANGE
        {
            public IntPtr hdc, hdcTarget;
            public RECT rc, rcPage;
            public int cpMin, cpMax;
        }

        // send the EM_FORMATRANGE message to the RichTextBox to render or measure
        // a range of the document into a target specified by a FORMATRANGE structure.
        static int FormatRange(RichTextBox rtb, bool render, ref FORMATRANGE fmt)
        {
            // render
            int nextChar = SendMessageFormatRange(
                rtb.Handle, 
                EM_FORMATRANGE, 
                render ? 1 : 0, 
                ref fmt);

            // reset
            SendMessage(rtb.Handle, EM_FORMATRANGE, 0, 0);

            // return next character to print
            return nextChar;
        }

        // two flavors of SendMessage
        [DllImport("USER32.DLL", CharSet = CharSet.Auto)]
        static extern int SendMessage(IntPtr hWnd, uint wMsg, int wParam, int lParam);

        [DllImport("USER32.DLL", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        static extern int SendMessageFormatRange(IntPtr hWnd, uint wMsg, int wParam, ref FORMATRANGE lParam);

        #endregion

    }
}
