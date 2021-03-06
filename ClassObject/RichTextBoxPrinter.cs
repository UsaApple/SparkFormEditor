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
    /// http://support.microsoft.com/default.aspx?scid=kb;en-us;812425
    /// The RichTextBox control does not provide any method to print the content of the RichTextBox. 
    /// You can extend the RichTextBox class to use EM_FORMATRANGE message 
    /// to send the content of a RichTextBox control to an output device such as printer.
    /// </summary>
    internal class RichTextBoxPrinter
    {
        //Convert the unit used by the .NET framework (1/100 inch) 
        //and the unit used by Win32 API calls (twips 1/1440 inch)
        private static double anInch = Comm.AnInch;
        private Image img;
        internal TransparentRTB TRTBTransparent = new TransparentRTB();
        public RichTextBox TRTB = new RichTextBox();//如果为透明，那么表格中的输入框，如果有上下标，就会阴影，看不清楚。

        // Render the contents of the RichTextBox for printing
        //	Return the last character printed + 1 (printing start from this point for next page)
        public int Print(IntPtr richTextBoxHandle, int charFrom, int charTo, PrintPageEventArgs e)
        {
            //Calculate the area to render and print
            Struct.STRUCT_RECT rectToPrint;
            rectToPrint.Top = (int)(e.MarginBounds.Top * anInch * GlobalVariable.ScaleTransHeight);
            rectToPrint.Bottom = (int)(e.MarginBounds.Bottom * anInch * GlobalVariable.ScaleTransHeight);
            rectToPrint.Left = (int)(e.MarginBounds.Left * anInch * GlobalVariable.ScaleTransWidth);
            rectToPrint.Right = (int)(e.MarginBounds.Right * anInch * GlobalVariable.ScaleTransWidth);

            //Calculate the size of the page
            Struct.STRUCT_RECT rectPage;
            rectPage.Top = (int)(e.PageBounds.Top * anInch * GlobalVariable.ScaleTransHeight);
            rectPage.Bottom = (int)(e.PageBounds.Bottom * anInch * GlobalVariable.ScaleTransHeight);
            rectPage.Left = (int)(e.PageBounds.Left * anInch * GlobalVariable.ScaleTransWidth);
            rectPage.Right = (int)(e.PageBounds.Right * anInch * GlobalVariable.ScaleTransWidth);

            IntPtr hdc = e.Graphics.GetHdc();

            //SetTextCharacterExtra(hdc, 0); //字间距

            Struct.STRUCT_FORMATRANGE fmtRange;
            fmtRange.chrg.cpMax = charTo;				//Indicate character from to character to 
            fmtRange.chrg.cpMin = charFrom;
            fmtRange.hdc = hdc;                    //Use the same DC for measuring and rendering
            fmtRange.hdcTarget = hdc;              //Point at printer hDC
            fmtRange.rc = rectToPrint;             //Indicate the area on page to print
            fmtRange.rcPage = rectPage;            //Indicate size of page

            int res = 0;

            int wparam = 1;

            //Get the pointer to the FORMATRANGE structure in memory
            IntPtr lparam = IntPtr.Zero;
            lparam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmtRange));
            Marshal.StructureToPtr(fmtRange, lparam, false);

            //Send the rendered data for printing 
            res = Win32Api.SendMessage(richTextBoxHandle, Win32Api.EM_FORMATRANGE, wparam, lparam);

            //Free the block of memory allocated
            Marshal.FreeCoTaskMem(lparam);

            //Release the device context handle obtained by a previous call
            e.Graphics.ReleaseHdc(hdc);

            // Release and cached info
            Win32Api.SendMessage(richTextBoxHandle, Win32Api.EM_FORMATRANGE, 0, (IntPtr)0);

            //Return last + 1 character printer
            return res;
        }

        public Image Print(RichTextBox ctl, int width, int height)
        {
            Image img = new Bitmap(width, height);
            float scale;

            using (Graphics g = Graphics.FromImage(img))
            {
                // --- Begin code addition D_Kondrad

                // HorizontalResolution is measured in pix/inch         
                scale = (float)(width * 100) / img.HorizontalResolution;
                width = (int)scale;

                // VerticalResolution is measured in pix/inch
                scale = (float)(height * 100) / img.VerticalResolution;
                height = (int)scale;

                Rectangle marginBounds = new Rectangle(0, 0, width, height);
                Rectangle pageBounds = new Rectangle(0, 0, width, height);
                PrintPageEventArgs args = new PrintPageEventArgs(g, marginBounds, pageBounds, null);

                Print(ctl.Handle, 0, ctl.Text.Length, args);
            }

            return img;
        }

        /// <summary>
        /// 非表格多行的时候
        /// </summary>
        /// <param name="ctl"></param>
        /// <param name="g"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void PrintUseG(RichTextBox ctl, Graphics g, int width, int height)
        {

            //如果多行的非表格的输入框文字的长了，可能会挡住紧靠右侧的线。
            TRTBTransparent.LanguageOption = RichTextBoxLanguageOptions.AutoFont;
            TRTBTransparent.Rtf = ctl.Rtf;

            FontStyle replystyle = FontStyle.Regular;
            for (int i = 0; i < TRTBTransparent.Text.Length; i++)
            {
                TRTBTransparent.Select(i, 1);

                //if (TRTB.SelectedText == "®")
                //{
                //    continue;
                //}

                replystyle = FontStyle.Regular;

                if (TRTBTransparent.SelectionFont.Bold)
                {
                    replystyle = FontStyle.Bold;
                }

                if (TRTBTransparent.SelectionFont.Italic)
                {
                    replystyle = replystyle | FontStyle.Italic;
                }

                if (TRTBTransparent.SelectionFont.Strikeout)
                {
                    replystyle = replystyle | FontStyle.Strikeout;
                }

                if (TRTBTransparent.SelectionFont.Underline)
                {
                    replystyle = replystyle | FontStyle.Underline;
                }

                TRTBTransparent.SelectionFont = new Font(TRTBTransparent.SelectionFont.FontFamily.Name, TRTBTransparent.SelectionFont.Size - Comm.GetPrintFontGay(TRTBTransparent.SelectionFont.Size), replystyle);//不然打印的字体的宽度，会超过输入框中显示的跨度（文字要多的时候就能发现）
            }
            ////----------------------------过长超出


            img = new Bitmap(width, height);  //Image img = new Bitmap(width, height);
            float scale;

            // HorizontalResolution is measured in pix/inch         
            scale = (float)(width * 100) / img.HorizontalResolution; //dpi
            width = (int)scale;

            // VerticalResolution is measured in pix/inch
            scale = (float)(height * 100) / img.VerticalResolution;
            height = (int)scale;

            Rectangle marginBounds = new Rectangle(ctl.Location.X, ctl.Location.Y, width, height);

            if (ctl is RichTextBoxExtended && (ctl as RichTextBoxExtended).OwnerTableHaveMultilineCell)
            {
                //打印时
                //表格中单元格回行的时候：
                Rectangle rect = RichTextBoxExtended.SetVerticalCenter(ctl);
                marginBounds = new Rectangle(ctl.Location.X, ctl.Location.Y + TRTBTransparent.ClientRectangle.Top + rect.Top, width, height);
            }

            Rectangle pageBounds = new Rectangle(ctl.Location.X, ctl.Location.Y, width, height);
            PrintPageEventArgs args = new PrintPageEventArgs(g, marginBounds, pageBounds, null);

            Print(TRTBTransparent.Handle, 0, ctl.Text.Length, args);
        }

        public void PrintUseG_Location00(RichTextBoxExtended ctl, Graphics g, int width, int height)
        {
            if (Comm._isCreatReportImages)
            {
                try
                {
                    TRTB.Font = ctl.Font;
                }
                catch
                {
                    RichTextBox.CheckForIllegalCrossThreadCalls = false;
                    TRTB.Font = ctl.Font;
                }
            }
            else
            {
                TRTB.Font = ctl.Font;
            }

            //行头缩进
            //SetVerticalCenter();

            TRTB.Rtf = ctl.Rtf;
            TRTB.Size = new Size(TRTB.Width + Consts.PRINT_CELL_DIFF, TRTB.Height); //+2防止打印正好,但是光标出来绘制的时候,边界遮挡

            width = width + Consts.PRINT_CELL_DIFF;

            if (ctl._IsTable)
            {
                //gap = Recruit3Const.PRINT_FONT_DIFF;
                //进行自动换行处理
                //TRTB.Multiline = true;
            }

            Image img = new Bitmap(width, height);
            float scale;

            // HorizontalResolution is measured in pix/inch         
            scale = (float)(width * 100) / img.HorizontalResolution;
            width = (int)scale;

            // VerticalResolution is measured in pix/inch
            scale = (float)(height * 100) / img.VerticalResolution;
            height = (int)scale;

            Rectangle marginBounds = new Rectangle(0, 0, width, height);

            if (ctl.OwnerTableHaveMultilineCell)
            {
                //光标离开显示的时候
                //表格中单元格回行的时候：
                Rectangle rect = RichTextBoxExtended.SetVerticalCenter(ctl);
                marginBounds = new Rectangle(0, TRTB.ClientRectangle.Top + rect.Top, width, height);
            }


            Rectangle pageBounds = new Rectangle(0, 0, width, height);
            PrintPageEventArgs args = new PrintPageEventArgs(g, marginBounds, pageBounds, null);

            Print(TRTB.Handle, 0, TRTB.Text.Length, args);
        }

    }
}

