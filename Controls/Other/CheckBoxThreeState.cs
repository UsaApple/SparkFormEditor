/**
 * 
 * 
 * 
 * 
 * 新表单增加一个勾选按钮 ，点击之后有 、  、 三个选项,老表单的选择框是三项，新表单是两项，产科分娩记录单要求不能有空选项，需要用到 。
 * 
 * 
 * 绘制难以控制，大小和位置。所以还是采用：SkinCheckBox
 * 
 * 
 * 
 * 
 * */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace SparkFormEditor
{
    internal partial class CheckBoxThreeState : CheckBox
    {
        public CheckBoxThreeState()
        {
            InitializeComponent();

            //this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            //this.BackColor = System.Drawing.Color.Transparent;
            //this.ThreeState = true; //勾选框本身就是可以设置为三种状态，只不过第三种状态是灰色无效的样式。重写就可以了
        }

        //private State state = State.Normal;

        ////public CheckBoxThreeState()
        ////{
        ////    this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        ////    this.BackColor = System.Drawing.Color.Transparent;
        ////}

        //protected override void OnMouseEnter(EventArgs e)
        //{
        //    state = State.MouseOver;
        //    this.Invalidate();
        //    base.OnMouseEnter(e);
        //}

        //protected override void OnMouseLeave(EventArgs e)
        //{
        //    state = State.Normal;
        //    this.Invalidate();
        //    base.OnMouseLeave(e);
        //}

        //protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        //{
        //    if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;

        //    state = State.MouseDown;
        //    this.Invalidate();
        //    base.OnMouseDown(e);
        //}

        //protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        //{
        //    if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
        //        state = State.Normal;
        //    this.Invalidate();
        //    base.OnMouseUp(e);
        //}

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.CheckState == CheckState.Indeterminate)
            {

                //第三种状态的，重绘。一般绘制×✘
                Graphics g = e.Graphics;

                //绘制第三种状态 ：×
                DrawRectIndeterminate(g);
                //g.Dispose();  //不能释放，否则会报错

            }
            //else
            //{
            //    base.OnPaint(e);
            //}

            //int cw = SystemInformation.MenuCheckSize.Width ;

            //if (this.CheckAlign == ContentAlignment.MiddleLeft)
            //{
            //    r1 = Rectangle.FromLTRB(0,(r1.Height-cw)/2,0+cw,(r1.Height+cw)/2);
            //} 
            //else
            //{
            //    r1 = Rectangle.FromLTRB(r1.Right-cw-1,(r1.Height-cw)/2,r1.Right,(r1.Height+cw)/2);
            //}

            //SkinDraw.DrawRect1(g,SkinImage.checkbox,r1,i);
            //base.OnPaint(e);

        }

        public void DrawRectIndeterminate(Graphics g)
        {
            Rectangle rc = this.ClientRectangle;
            //Rectangle r1 = rc;
            Point rectPoint = Point.Empty;

            int cw = SystemInformation.MenuCheckSize.Width; //勾选框的的大小

            if (this.CheckAlign == ContentAlignment.MiddleLeft)
            {
                rc = Rectangle.FromLTRB(0, (rc.Height - cw) / 2, 0 + cw, (rc.Height + cw) / 2);

                rectPoint = new Point(rc.X - 2, rc.Y + 2);
            }
            else
            {
                rc = Rectangle.FromLTRB(rc.Right - cw - 1, (rc.Height - cw) / 2, rc.Right, (rc.Height + cw) / 2);

                rectPoint = new Point(rc.X + 2, rc.Y + 2);
            }

            //////清空矩形区域
            //g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(2, 5, 9, 9));
            //g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), rc);

            //g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(rc.X, rc.Y, 9, 9));

            int x1 = rc.Left;
            int y1 = rc.Top;
            Rectangle r2 = new Rectangle(x1, y1, rc.Width, rc.Height);

            g.FillRectangle(new SolidBrush(this.BackColor), rc);

            // Set format of string.
            StringFormat drawFormat = new StringFormat();
            drawFormat.Alignment = StringAlignment.Center;


            g.DrawString("×", new Font(Font.FontFamily.Name, 9f, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(rectPoint.X, rectPoint.Y, rc.Width, rc.Height), drawFormat);

            //g.DrawString("×", new Font("宋体", 9f, FontStyle.Bold), new SolidBrush(Color.Black), rectPoint);

        }

        //private void CheckBoxThreeState_Paint(object sender, PaintEventArgs e)
        //{
        //    if (this.CheckState == CheckState.Indeterminate)
        //    {
        //        //第三种状态的，重绘。一般绘制×✘
        //        Graphics g = e.Graphics;

        //        //绘制第三种状态 ：×
        //        DrawRectIndeterminate(g);
        //        //g.Dispose();  //不能释放，否则会报错
        //    }
        //}

        //internal static void DrawRect1(Graphics g, ImageObject obj, Rectangle r, int index)
        //{
        //    if (obj.img == null) return;
        //    Rectangle r1, r2;
        //    int x = (index - 1) * obj.Width;
        //    int y = 0;
        //    int x1 = r.Left;
        //    int y1 = r.Top;
        //    r1 = new Rectangle(x, y, obj.Width, obj.Height);
        //    r2 = new Rectangle(x1, y1, r.Width, r.Height);
        //    g.DrawImage(obj.img, r2, r1, GraphicsUnit.Pixel);
        //}
    }

    //public enum State
    //{
    //    Normal = 1,
    //    MouseOver = 2,
    //    MouseDown = 3,
    //    Disable = 4,
    //    Default = 5
    //}
}
