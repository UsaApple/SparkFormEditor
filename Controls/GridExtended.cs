/* 新表单
 * *************************************************************
 无锡 曼荼罗 谈俊琪 2013年6月28日 -2013年8月8日
 * 
 * 
 * 暂时废弃，老的实现方式的表格
 * 
 *  
 * *************************************************************
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;

//新表单 自定义行控件 by 谈俊琪  2013年
namespace Mandala.Recruit3
{
    public partial class GridExtended : UserControl
    {
        //XmlNode _NodeSetting;

        public GridExtended()
        {
            InitializeComponent();
        }

        public GridExtended(Size rowSize, int _PageRows)
        {
            InitializeComponent();

            this.BorderStyle = BorderStyle.None;

            this.Size = new Size(rowSize.Width, rowSize.Height);

            //this.BackColor = Color.Black; //调式用，凸显颜色
            //this.BorderStyle = BorderStyle.FixedSingle; //调试用
        }

        //控件透明化
        //override protected CreateParams CreateParams { get { CreateParams cp = base.CreateParams; cp.ExStyle |= 0x20; return cp; } }

        //override protected void OnPaintBackground(PaintEventArgs e)
        //{
        //    //base.OnPaintBackground(e);//最后执行父类的绘制时间
        //}

        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    //base.OnPaint(e);
        //}

    }
}
