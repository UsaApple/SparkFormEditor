using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SparkFormEditor
{
    /// <summary>
    /// 打印富文本输入框的内容的类
    /// </summary>
    internal class RichTextBoxToPicture
    {
        private static int m_nFirstCharOnPage;
        private static RichTextBox RTB;
        public static TransparentRTB TRTB = new TransparentRTB();

        public static int FormatRange(bool measureOnly, ref TransparentRTB RTB, ref Bitmap b, int charFrom, int charTo)
        {
            // Specify which characters to print
            Struct.STRUCT_CHARRANGE cr;
            cr.cpMin = charFrom;
            cr.cpMax = charTo;

            // Specify the area inside page margins
            Struct.STRUCT_RECT rc;
            rc.Top = HundredthInchToTwips(0);
            rc.Bottom = HundredthInchToTwips(b.Height);
            rc.Left = HundredthInchToTwips(0);
            rc.Right = HundredthInchToTwips(b.Width);

            // Specify the page area
            Struct.STRUCT_RECT rcPage;
            rcPage.Top = HundredthInchToTwips(0);
            rcPage.Bottom = HundredthInchToTwips(b.Height);
            rcPage.Left = HundredthInchToTwips(0);
            rcPage.Right = HundredthInchToTwips(b.Width);

            // Get device context of output device
            Graphics g = Graphics.FromImage(b);
            IntPtr hdc = g.GetHdc();

            // Fill in the FORMATRANGE struct
            Struct.STRUCT_FORMATRANGE fr;
            fr.chrg = cr;
            fr.hdc = hdc;
            fr.hdcTarget = hdc;
            fr.rc = rc;
            fr.rcPage = rcPage;

            // Non-Zero wParam means render, Zero means measure
            Int32 wParam = (measureOnly ? 0 : 1);

            // Allocate memory for the FORMATRANGE struct and
            // copy the contents of our struct to this memory
            IntPtr lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fr));
            Marshal.StructureToPtr(fr, lParam, false);

            // Send the actual Win32 message
            int res = Win32Api.SendMessage(RTB.Handle, Win32Api.EM_FORMATRANGE, wParam, lParam);

            // Free allocated memory
            Marshal.FreeCoTaskMem(lParam);

            // and release the device context
            g.ReleaseHdc(hdc);

            g.Dispose();

            return res;
        }

        public int FormatRange(bool measureOnly, RichTextBox RTB, PictureBox pb, int charFrom, int charTo, Graphics g, int pTop, int pBottom, int pLeft, int pRight)
        {
            // Specify which characters to print
            Struct.STRUCT_CHARRANGE cr;
            cr.cpMin = charFrom;
            cr.cpMax = charTo;

            // Specify the area inside page margins
            Struct.STRUCT_RECT rc;
            rc.Top = HundredthInchToTwips(pTop + 2);
            rc.Bottom = HundredthInchToTwips(pBottom + 2);
            rc.Left = HundredthInchToTwips(pLeft);
            rc.Right = HundredthInchToTwips(pRight);

            // Specify the page area
            Struct.STRUCT_RECT rcPage;
            rcPage.Top = HundredthInchToTwips(pTop + 2);
            rcPage.Bottom = HundredthInchToTwips(pBottom + 2);
            rcPage.Left = HundredthInchToTwips(pLeft);
            rcPage.Right = HundredthInchToTwips(pRight);

            // Get device context of output device
            //Graphics g = pb.CreateGraphics();
            IntPtr hdc = g.GetHdc();

            // Fill in the FORMATRANGE struct
            Struct.STRUCT_FORMATRANGE fr;
            fr.chrg = cr;
            fr.hdc = hdc;
            fr.hdcTarget = hdc;
            fr.rc = rc;
            fr.rcPage = rcPage;

            // Non-Zero wParam means render, Zero means measure
            Int32 wParam = (measureOnly ? 0 : 1);

            // Allocate memory for the FORMATRANGE struct and
            // copy the contents of our struct to this memory
            IntPtr lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fr));
            Marshal.StructureToPtr(fr, lParam, false);

            // Send the actual Win32 message
            int res = Win32Api.SendMessage(RTB.Handle, Win32Api.EM_FORMATRANGE, wParam, lParam);

            // Free allocated memory
            Marshal.FreeCoTaskMem(lParam);

            // and release the device context
            g.ReleaseHdc(hdc);

            //g.Dispose();

            return res;
        }

        public static int FormatRange(bool measureOnly, RichTextBox RTB, Bitmap b, int charFrom, int charTo, Graphics g, int pTop, int pBottom, int pLeft, int pRight)
        {
            //TransparentRTB RTB = new TransparentRTB();
            ////RTB.BackColor = Color.Transparent;//会报错
            //RTB.Font = RTBPara.Font;
            //RTB.Rtf = RTBPara.Rtf;
            //RTB.Height = RTBPara.Height;
            //RTB.Width = RTBPara.Width + 2;

            // Specify which characters to print
            Struct.STRUCT_CHARRANGE cr;
            cr.cpMin = charFrom;
            cr.cpMax = charTo;


            // Specify the area inside page margins
            Struct.STRUCT_RECT rc;
            rc.Top = HundredthInchToTwips(pTop);
            rc.Bottom = HundredthInchToTwips(pBottom);
            rc.Left = HundredthInchToTwips(pLeft);
            rc.Right = HundredthInchToTwips(pRight);

            // Specify the page area
            Struct.STRUCT_RECT rcPage;
            rcPage.Top = HundredthInchToTwips(pTop);
            rcPage.Bottom = HundredthInchToTwips(pBottom);
            rcPage.Left = HundredthInchToTwips(pLeft);
            rcPage.Right = HundredthInchToTwips(pRight);

            // Get device context of output device
            //Graphics g = Graphics.FromImage(b);
            IntPtr hdc = g.GetHdc();

            //SetTextCharacterExtra(hdc, -1); //字间距


            // Fill in the FORMATRANGE struct
            Struct.STRUCT_FORMATRANGE fr;
            fr.chrg = cr;
            fr.hdc = hdc;
            fr.hdcTarget = hdc;
            fr.rc = rc;
            fr.rcPage = rcPage;


            // Non-Zero wParam means render, Zero means measure
            Int32 wParam = (measureOnly ? 0 : 1);

            // Allocate memory for the FORMATRANGE struct and
            // copy the contents of our struct to this memory
            IntPtr lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fr));
            Marshal.StructureToPtr(fr, lParam, false);

            // Send the actual Win32 message
            int res = Win32Api.SendMessage(RTB.Handle, Win32Api.EM_FORMATRANGE, wParam, lParam);

            // Free allocated memory
            Marshal.FreeCoTaskMem(lParam);

            // and release the device context
            g.ReleaseHdc(hdc);

            return res;
        }

        /// <summary>
        /// Free cached data from rich edit control after printing
        /// </summary>
        public static void FormatRangeDone(ref RichTextBox RTB)
        {
            IntPtr lParam = new IntPtr(0);
            Win32Api.SendMessage(RTB.Handle, Win32Api.EM_FORMATRANGE, 0, lParam);
        }

        public static void FormatRangeDone(ref TransparentRTB RTB)
        {
            IntPtr lParam = new IntPtr(0);
            Win32Api.SendMessage(RTB.Handle, Win32Api.EM_FORMATRANGE, 0, lParam);
        }

        public static void RTFtoBitmap(RichTextBox RTBPara, Bitmap b, Graphics g, int pTop, int pBottom, int pLeft, int pRight)
        {
            RTB = new RichTextBox();
            //RTB.Multiline = true;
            //RTB.WordWrap = true;
            //RTB.BackColor = Color.Transparent;//会报错
            //RTB.Font = new Font(RTBPara.Font, FontStyle.Regular);
            //RTB.ForeColor = RTBPara.ForeColor;
            RTB.Rtf = RTBPara.Rtf;
            RTB.Height = RTBPara.Height;
            RTB.Width = RTBPara.Width + 2;

            //RTB.SelectAll();
            //RTB.Multiline = true;
            //RTB.WordWrap = true;
            //RTB.Update();

            m_nFirstCharOnPage = 0;


            //上面，当输入框中文字过多显示显示不全会报错
            m_nFirstCharOnPage = FormatRange(false, RTB, b, m_nFirstCharOnPage, RTB.Text.Length, g, pTop, pBottom, pLeft, pRight);
            if (m_nFirstCharOnPage != RTB.Text.Length)
            {
                pLeft = pLeft - 3;
                pRight = pRight + 2;
                RTB.Width = RTBPara.Width + 5;
                b = new Bitmap(RTBPara.Width, RTBPara.Height);
                //RTB.Multiline = true;
                //RTB.WordWrap = true;

                m_nFirstCharOnPage = FormatRange(false, RTB, b, 0, Encoding.Default.GetBytes(RTB.Text).Length, g, pTop, pBottom, pLeft, pRight);
            }

            FormatRangeDone(ref RTB);

            RTB.Dispose();

        }

        /// <summary>
        /// 表格打印的时候，绘制每个单元格
        /// </summary>
        /// <param name="RTBPara"></param>
        /// <param name="b"></param>
        /// <param name="g"></param>
        /// <param name="pTop"></param>
        /// <param name="pBottom"></param>
        /// <param name="pLeft"></param>
        /// <param name="pRight"></param>
        public static void RTFtoBitmapTransparentRTB(RichTextBoxExtended RTBPara, Bitmap b, Graphics g, int pTop, int pBottom, int pLeft, int pRight)
        {
            //TransparentRTB TRTB = new TransparentRTB();

            if (RTBPara.OwnerTableHaveMultilineCell)
            {
                //打印时
                //表格中单元格回行的时候：
                Rectangle rect = RichTextBoxExtended.SetVerticalCenter(RTBPara);
                //marginBounds = new Rectangle(ctl.Location.X, ctl.Location.Y + TRTBTransparent.ClientRectangle.Top + rect.Top, width, height);
                //配置成单行的，
                pTop = pTop + RTBPara.ClientRectangle.Top + rect.Top;
            }

            TRTB.Font = RTBPara.Font;
            TRTB.Rtf = RTBPara.Rtf;
            //TRTB.Rtf = RTBPara.Rtf.Replace("fcharset0", "fcharset134"); 
            TRTB.Height = RTBPara.Height - 2;
            //TRTB.Width = RTBPara.Width + 2; //会偏右
            TRTB.Width = RTBPara.Width;

            float gap = Comm.GetPrintFontGay(RTBPara.Font.Size);

            if (RTBPara.SelectionFont != null)
            {
                gap = Comm.GetPrintFontGay(RTBPara.SelectionFont.Size);
            }

            //int bitLength = Encoding.Default.GetBytes(TRTB.Text).Length;  //半角一个字符为1

            if (TRTB.Text.Length > 20)
            {
                Font fme = RTBPara.Font;
                if (RTBPara.SelectionFont != null)
                {
                    fme = RTBPara.SelectionFont;
                }

                float f = TRTB.Text.Length * fme.Size / 9 / 200;

                if (fme.Size <= 9f)
                {
                    if (f > 0.1f)
                    {
                        f = 0.1f;
                    }
                    else
                    {
                        //gap = gap + f; //每1个字，10000左右
                    }
                }

                gap = gap + f;
            }


            if (RTBPara._IsTable)
            {
                //gap = Recruit3Const.PRINT_FONT_DIFF;
                //进行自动换行处理
                //TRTB.Multiline = true;            
            }
            else
            {
                gap = 0;
                TRTB.Height = RTBPara.Height - 1;
                pTop = pTop - 1;
            }

            //TRTB.SelectAll();
            //TRTB.SelectionFont = new Font(RTBPara.Font.FontFamily.Name, RTBPara.Font.Size - 0.25f);//不然打印的字体的宽度，会超过输入框中显示的跨度（文字要多的时候就能发现）
            FontStyle replystyle = FontStyle.Regular;
            for (int i = 0; i < TRTB.Text.Length; i++)
            {
                TRTB.Select(i, 1);

                //if (TRTB.SelectedText == "®")
                //{
                //    continue;
                //}

                replystyle = FontStyle.Regular;

                if (TRTB.SelectionFont.Bold)
                {
                    replystyle = FontStyle.Bold;
                }

                if (TRTB.SelectionFont.Italic)
                {
                    replystyle = replystyle | FontStyle.Italic;
                }

                if (TRTB.SelectionFont.Strikeout)
                {
                    replystyle = replystyle | FontStyle.Strikeout;
                }

                if (TRTB.SelectionFont.Underline)
                {
                    replystyle = replystyle | FontStyle.Underline;
                }


                //TRTB.SelectionFont = new Font(TRTB.SelectionFont.FontFamily.Name, TRTB.SelectionFont.Size - 0.28f, replystyle);//不然打印的字体的宽度，会超过输入框中显示的跨度（文字要多的时候就能发现）
                TRTB.SelectionFont = new Font(TRTB.SelectionFont.FontFamily.Name, (TRTB.SelectionFont.Size - gap) * GlobalVariable.ScaleTransWidth, replystyle);//不然打印的字体的宽度，会超过输入框中显示的跨度（文字要多的时候就能发现）
            }

            //b = new Bitmap(TRTB.Width - 2, TRTB.Height);


            m_nFirstCharOnPage = 0;

            pTop = (Int32)(pTop * GlobalVariable.ScaleTransHeight);
            pBottom = (Int32)(pBottom * GlobalVariable.ScaleTransHeight);
            pLeft = (Int32)(pLeft * GlobalVariable.ScaleTransWidth);
            pRight = (Int32)(pRight * GlobalVariable.ScaleTransWidth);

            //当输入框中文字过多显示显示不全会报错
            m_nFirstCharOnPage = FormatRange(false, TRTB, b, m_nFirstCharOnPage, TRTB.Text.Length, g, pTop, pBottom, pLeft, pRight); // - (int)(RTBPara.Font.Size / 2)
            if (m_nFirstCharOnPage != TRTB.Text.Length)
            {
                //pLeft = pLeft - 3;
                //pRight = pRight + 2;
                //TRTB.Width = RTBPara.Width + 5;
                //b = new Bitmap(RTBPara.Width, RTBPara.Height);

                m_nFirstCharOnPage = FormatRange(false, TRTB, b, 0, Encoding.Default.GetBytes(TRTB.Text).Length, g, pTop, pBottom, pLeft, pRight);
            }

            FormatRangeDone(ref TRTB);

            //TRTB.Dispose();

            //

        }

        public static void RTFtoBitmapTransparentRTB_Multiline(ref RichTextBoxExtended RTB, ref Bitmap b)
        {
            TRTB.Font = RTB.Font;
            TRTB.Rtf = RTB.Rtf.Replace("fcharset0", "fcharset134"); ;
            TRTB.Height = RTB.Height - 2;
            TRTB.Width = RTB.Width + 2;

            m_nFirstCharOnPage = 0;

            do
            {
                m_nFirstCharOnPage = FormatRange(false, ref TRTB, ref b, m_nFirstCharOnPage, TRTB.Text.Length);
                if (m_nFirstCharOnPage != TRTB.Text.Length)
                {
                    b = new Bitmap(b.Width, b.Height + 1);
                    m_nFirstCharOnPage = 0;
                }
            }
            while (m_nFirstCharOnPage < TRTB.Text.Length);

            FormatRangeDone(ref TRTB);

        }

        /// <summary>
        /// Convert between 1/100 inch (unit used by the .NET framework)
        /// and twips (1/1440 inch, used by Win32 API calls)
        /// </summary>
        /// <param >Value in 1/100 inch</param>
        /// <returns>Value in twips</returns>
        private static Int32 HundredthInchToTwips(int n)
        {
            return (Int32)(n * Comm.AnInch);
        }

    }
}
