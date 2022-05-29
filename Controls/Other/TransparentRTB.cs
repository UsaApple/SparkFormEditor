using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SparkFormEditor
{
    /// <summary>
    /// 透明的输入框，打印和显示的时候用，无背景色，不会挡住上下线；但是字体会变粗
    /// </summary>
    internal partial class TransparentRTB : RichTextBoxExtended
    {
        public TransparentRTB()
        {
            InitializeComponent();
            this.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
            this.DoubleBuffered = true;
        }

        override protected CreateParams CreateParams { get { CreateParams cp = base.CreateParams; cp.ExStyle |= 0x20; return cp; } }    
        //override protected void OnPaintBackground(PaintEventArgs e) 
        //{
        //    ////双下划线绘制
        //    //Graphics g = this.CreateGraphics();
        //    //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        //    //g.DrawLine(new Pen(Color.Red, 2), new Point(5, 10), new Point(this.Width - 10, 10));
        //}

        ////---------------------------------------背景透明，但是拖动滚动条会闪动--------打印，字体会编出，上下标居左会重叠------------------------------
        ////[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        //[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        //static extern IntPtr LoadLibrary(string lpFileName);

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        //this.Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Regular);
        //        CreateParams prams = base.CreateParams;
        //        if (LoadLibrary("msftedit.dll") != IntPtr.Zero)
        //        {
        //            prams.ExStyle |= 0x020; // transparent
        //            prams.ClassName = "RICHEDIT50W";
        //        }
        //        return prams;
        //    }
        //}

        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    ////双下划线绘制
        //    //Graphics g = this.CreateGraphics();
        //    //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        //    //g.DrawLine(new Pen(Color.Red, 2), new Point(5, 10), new Point(this.Width - 10, 10));
        //}

        //private void TransparentRTB_TextChanged(object sender, EventArgs e)
        //{
        //    ////this.Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Regular);
        //    //this.Rtf = this.Rtf.Replace("fcharset0", "fcharset134");
        //}
        ////---------------------------------------背景透明，但是拖动滚动条会闪动--------------------------------------
    }
}
