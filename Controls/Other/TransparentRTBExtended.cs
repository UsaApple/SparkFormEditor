
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
    /// 透明的扩展输入框，打印和显示的时候用，无背景色，不会挡住上下线；但是字体会变粗
    /// </summary>
    internal partial class TransparentRTBExtended : RichTextBoxExtended
    {
        public TransparentRTBExtended()
        {
            //区域生成图表时，次数过多，会报错;创建窗口句柄时出错。
            if (!Comm._isCreatReportImages)
            {
                InitializeComponent();
            }
            else
            {
                this.components = new System.ComponentModel.Container();  //如果为nul会导致不能释放
            }

            this.DoubleBuffered = true;
        }

        //控件透明化
        override protected CreateParams CreateParams { get { CreateParams cp = base.CreateParams; cp.ExStyle |= 0x20; return cp; } }


        //private void TransparentRTB_TextChanged(object sender, EventArgs e)
        //{
        //    ////this.Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Regular);
        //    //this.Rtf = this.Rtf.Replace("fcharset0", "fcharset134");
        //}
        ////---------------------------------------背景透明，但是拖动滚动条会闪动--------------------------------------
    }
}
