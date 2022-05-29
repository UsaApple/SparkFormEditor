using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SparkFormEditor
{
    internal partial class TransparentLabel : UserControl
    {
        public TransparentLabel()
        {
            InitializeComponent();

            //双缓存开启，另外Form的DoubleBuffer的属性为True
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams para = base.CreateParams;
        //        para.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT 透明支持
        //        return para;
        //    }
        //}

        //protected override void OnPaintBackground(PaintEventArgs e) //不画背景
        //{
        //    //base.OnPaintBackground(e);
        //}

        //override protected CreateParams CreateParams { get { CreateParams cp = base.CreateParams; cp.ExStyle |= 0x20; return cp; } }   

        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    e.Graphics.DrawString(Text, Font, new SolidBrush(this.ForeColor), new System.Drawing.PointF(1, 1)); //自己绘制文本
        //    //base.OnPaint(e);
        //}

        /// <summary>
        /// 绘制方法
        /// </summary>
        public void Draw()
        {
            //DrawString(Text, Font, new SolidBrush(this.ForeColor), new System.Drawing.PointF(1, 1)); //自己绘制文本

            this.BackgroundImage = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(this.BackgroundImage);
            //请注意这个透明并不是真正的透明，而是用父控件的当前位置的颜色填充PictureBox内的相应位置的颜色。 
            g.Clear(Color.White); //g.Clear(Color.White); //g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.HighQuality;
            //g.DrawString(Text, Font, new SolidBrush(this.ForeColor), new System.Drawing.PointF(3, 3)); //自己绘制文本
            //g.DrawString(Text, Font, new SolidBrush(this.ForeColor), new System.Drawing.PointF(3, 4)); //自己绘制文本
            Brush sBrrsh = new SolidBrush(this.ForeColor);
            Brush sBrrsh2 = new SolidBrush(Color.DarkRed);

            ////纯色箭头
            Point[] points = new Point[] { new Point(1, 1), new Point(16, 7), new Point(1, 13), new Point(4, 7) };
            g.FillPolygon(sBrrsh, points);

            ////仿Word的段落箭头
            //Point[] points = new Point[] { new Point(1, 1), new Point(16, 7), new Point(4, 7) };
            //g.FillPolygon(sBrrsh, points);

            //Point[] points2 = new Point[] { new Point(16, 7), new Point(1, 13), new Point(4, 7) };
            //g.FillPolygon(sBrrsh2, points2);

            g.Dispose();
            sBrrsh.Dispose();
            sBrrsh2.Dispose();

            ControlTrans(this, this.BackgroundImage); 
        }

        private unsafe static GraphicsPath subGraphicsPath(Image img)
        {

            //unsafe
            //{

            if (img == null) return null;

            // 建立GraphicsPath, 给我们的位图路径计算使用   
            GraphicsPath g = new GraphicsPath(FillMode.Alternate);

            Bitmap bitmap = new Bitmap(img);

            int width = bitmap.Width;
            int height = bitmap.Height;
            //BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* p = (byte*)bmData.Scan0;
            int offset = bmData.Stride - width * 3;
            int p0, p1, p2;         // 记录左上角0，0座标的颜色值  
            p0 = p[0];
            p1 = p[1];
            p2 = p[2];

            int start = -1;
            // 行座标 ( Y col )   
            for (int Y = 0; Y < height; Y++)
            {
                // 列座标 ( X row )   
                for (int X = 0; X < width; X++)
                {
                    if (start == -1 && (p[0] != p0 || p[1] != p1 || p[2] != p2))     //如果 之前的点没有不透明 且 不透明   
                    {
                        start = X;                            //记录这个点  
                    }
                    else if (start > -1 && (p[0] == p0 && p[1] == p1 && p[2] == p2))      //如果 之前的点是不透明 且 透明  
                    {
                        g.AddRectangle(new Rectangle(start, Y, X - start, 1));    //添加之前的矩形到  
                        start = -1;
                    }

                    if (X == width - 1 && start > -1)        //如果 之前的点是不透明 且 是最后一个点  
                    {
                        g.AddRectangle(new Rectangle(start, Y, X - start + 1, 1));      //添加之前的矩形到  
                        start = -1;
                    }
                    //if (p[0] != p0 || p[1] != p1 || p[2] != p2)  
                    //    g.AddRectangle(new Rectangle(X, Y, 1, 1));  
                    p += 3;                                   //下一个内存地址  
                }
                p += offset;
            } bitmap.UnlockBits(bmData);
            bitmap.Dispose();

            //}

            // 返回计算出来的不透明图片路径   
            return g;
        }

        /// <summary>  
        /// 调用此函数后使图片透明  
        /// </summary>  
        /// <param name="control">需要处理的控件</param>  
        /// <param name="img">控件的背景或图片，如PictureBox.Image  
        ///   或PictureBox.BackgroundImage</param>  
        public static void ControlTrans(Control control, Image img)
        {
            GraphicsPath g;
            g = subGraphicsPath(img);
            if (g == null)
                return;
            control.Region = new Region(g);
        }

    }
}
