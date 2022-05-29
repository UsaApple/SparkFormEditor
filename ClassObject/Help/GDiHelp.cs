//WiNDOWS的用户所使用的分辨率一般是96像素/英寸。
//一个像素的尺寸是和分辨率有关的，例如分辨率是96dpi的话,
//也就是每英寸里有96个像素,每像素就是1/96英寸。
//1英寸=2.54厘米； 1厘米=0.39370078740157英寸 
//96/ 25.4= 3.8，每毫米cm就是3.8像素
//16K 184*260 --> 699×988
//A3  297*420 --> 1128×1596
//A4  210*297 --> 798×1128
//A5  148*210 --> 562×798

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace SparkFormEditor
{
    ///<summary>
    ///绘制勾选框、下拉框等控件的自己的方法
    ///解决直接控件本身的绘制的不良效果：不清晰等
    ///</summary>
    internal class GDiHelp
    {
        //自己用GDI绘制勾选框，比想象中的复杂多了，唉
        public static void DrawCheckBox(CheckBoxExtended chkbe, Graphics g)
        {
            //1.很神奇的一个问题：宋体，12的字体，在单击版运行的时候chkbe.Size.Height为20，但结合就是25
            //2.经过测试发现，微软的勾选框，哪怕字体设置到50，勾选框还是固定的那个大小，差不多就是9好字体那么；垂直居中
            Font messureFont = new Font(chkbe.Font.FontFamily.Name, chkbe.Font.Size - Consts.PRINT_FONT_DIFF);
            int height = TextRenderer.MeasureText("W", messureFont).Height;

            Font messureFont9 = new Font(chkbe.Font.FontFamily.Name, 9 - Consts.PRINT_FONT_DIFF);
            int height9 = TextRenderer.MeasureText("W", messureFont9).Height; //（9，宋体）的字体高度

            int thisHeight = height9; //（9，宋体）的字体时的框高
            float diffRect = (height - height9) / 2;
            Font fontGou = new Font(chkbe.Font.FontFamily.Name, 9);  //绘制勾的字体

            string strChecked = "√";
            float chaDiffRight = 0;
            if (chkbe.CheckState == CheckState.Indeterminate)
            {
                strChecked = "×";
                fontGou = new Font(chkbe.Font.FontFamily.Name, 9.2f);

                if (chkbe.CheckAlign == System.Drawing.ContentAlignment.MiddleRight)
                {
                    chaDiffRight = 1.0f;
                }
            }

            //文字对其方式
            if (chkbe.CheckAlign == System.Drawing.ContentAlignment.MiddleLeft)
            {
                //居左
                g.DrawRectangle(new Pen(chkbe.ForeColor, 1), chkbe.Location.X, chkbe.Location.Y + diffRect, thisHeight, thisHeight);//绘制边框

                if (chkbe.Checked)
                {
                    //绘制勾
                    g.DrawString(strChecked, fontGou, new SolidBrush(chkbe.ForeColor), chkbe.Location.X - 2f, chkbe.Location.Y + diffRect + 0.5f);
                }

                try
                {
                    g.DrawString(chkbe.Text, chkbe.Font, new SolidBrush(chkbe.ForeColor), chkbe.Location.X + thisHeight + 2, chkbe.Location.Y);
                }
                catch
                {
                    //神奇的绘制问题异常，暂未查明具体原因。调试发现时字体的大小异常了。
                    //文字：+	护理Focus中，如果是分组打印的时候FontFamily	{Name = 当前上下文中不存在名称“name”}	System.Drawing.FontFamily
                    g.DrawString(chkbe.Text, new Font(chkbe.Font.Name,chkbe.Font.Size) , new SolidBrush(chkbe.ForeColor), chkbe.Location.X + thisHeight + 2, chkbe.Location.Y);
                }
            }
            else
            {
                byte[] b = Encoding.Default.GetBytes(chkbe.Text);
                float width = b.Length * chkbe.Font.Size * 0.73f + 2;

                //居右
                g.DrawRectangle(new Pen(chkbe.ForeColor, 1), chkbe.Location.X + width + thisHeight / 3, chkbe.Location.Y + diffRect, thisHeight, thisHeight);   //绘制边框

                if (chkbe.Checked)
                {
                    //绘制勾
                    g.DrawString(strChecked, fontGou, new SolidBrush(chkbe.ForeColor), chkbe.Location.X + width + thisHeight / 3 - chkbe.Font.Size / 4 + chaDiffRight, chkbe.Location.Y + diffRect + 0.5f);
                }

                //文字
                g.DrawString(chkbe.Text, chkbe.Font, new SolidBrush(chkbe.ForeColor), chkbe.Location.X, chkbe.Location.Y);
            }
        }

        /// <summary>
        /// 绘制下拉框
        /// </summary>
        /// <param name="cmbe"></param>
        /// <param name="g"></param>
        public static void DrawComboBox(ComboBoxExtended cmbe, Graphics g)
        {
            g.DrawString(cmbe.Text, cmbe.Font, new SolidBrush(cmbe.ForeColor), cmbe.Location.X, cmbe.Location.Y);
        }

        /// <summary>
        /// 绘制扇形饼图的方法，和扇形控件的原理和逻辑一致
        /// </summary>
        /// <param name="g"></param>
        private void DrawPie(Graphics g, Point startPoint, int[] arrValue, Color[] arrColor, string[] arrText)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle er = new Rectangle(startPoint.X, startPoint.Y, 230, 200); //位置，大小
            Rectangle br = er;
            //br.Offset(0, 30);

            int[] arrSum = new int[arrValue.Length];     //360圆弧的比例
            string[] arrProrate = new string[arrValue.Length]; //100百分比的比例

            //得到总值，一边计算比例
            int sumAll = 0;
            for (int i = 0; i < arrValue.Length; i++)
            {
                sumAll += arrValue[i];
            }

            //得到各自所占的比例
            for (int i = 0; i < arrValue.Length; i++)
            {
                arrSum[i] = 360 * arrValue[i] / sumAll;
                arrProrate[i] = Math.Round(100f * arrValue[i] / sumAll, 2).ToString() + "%";
            }

            int start = 0; //从0开始
            for (int i = 0; i < arrValue.Length; i++)
            {
                //绘制扇形区域
                g.FillPie(new SolidBrush(Color.FromArgb(200, arrColor[i])), er, start, arrSum[i]);          //指定的角度，转移多少度

                //绘制边框
                g.DrawPie(Pens.Goldenrod, er, start, arrSum[i]);

                start += arrSum[i];
            }

            //绘制文字
            start = 20; //从0开始
            for (int i = 0; i < arrValue.Length; i++)
            {
                //绘制矩形颜色提示
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, arrColor[i])), br.X + br.Width, start + i * 20, 20, 10);          //指定的角度，转移多少度

                //绘制文字提示
                g.DrawString(arrText[i], new Font("宋体", 9), Brushes.Black, new Point(br.X + br.Width + 25, start + i * 20));

                //各自数值
                g.DrawString(arrValue[i].ToString(), new Font("宋体", 9), Brushes.Black, new Point(br.X + br.Width - 20, start + i * 20));

                //各自数值
                g.DrawString(arrProrate[i].ToString(), new Font("宋体", 9), Brushes.Black, new Point(br.X + br.Width + 60, start + i * 20));

            }
        }

        /// <summary>
        /// 绘制扇形饼图2
        /// </summary>
        /// <param name="g"></param>
        private void DrawPie2(Graphics objGraphics)
        {
            // Create a Bitmap instance that's 468x60, and a Graphics instance
            const int width = 300, height = 300;
            //Bitmap objBitmap = new Bitmap(width, height);
            //Graphics objGraphics = Graphics.FromImage(objBitmap);
            //Graphics objGraphics = Graphics.FromHwnd(this.Handle);

            // Create a black background for the border
            //objGraphics.FillRectangle(new SolidBrush(Color.White), 0, 0, width, height);

            Draw3DPieChart(ref objGraphics);
            // Save the image to a file
            //	objBitmap.Save("test.jpg");
            //MessageBox.Show("Test");

            // clean up...
            objGraphics.Dispose();
            //	objBitmap.Dispose();
        }   // btnRun _Click

        // Draws a 3D pie chart where ever slice is 45 degrees in value
        void Draw3DPieChart(ref Graphics objGraphics)
        {
            int iLoop, iLoop2;
            // Create location and size of ellipse.
            int x = 50;
            int y = 120;
            int width = 200;
            int height = 100;
            // Create start and sweep angles.
            int startAngle = 0;
            int sweepAngle = 45;
            SolidBrush objBrush = new SolidBrush(Color.Aqua);

            Random rand = new Random();

            objGraphics.SmoothingMode = SmoothingMode.AntiAlias;

            //Loop through 180 back around to 135 degress so it gets drawn correctly.
            for (iLoop = 0; iLoop <= 315; iLoop += 45)
            {
                startAngle = (iLoop + 180) % 360;

                objBrush.Color = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
                // On degrees from 0 to 180 draw 10 Hatched brush slices to show depth
                if ((startAngle < 135) || (startAngle == 180))
                {
                    for (iLoop2 = 0; iLoop2 < 10; iLoop2++)
                        objGraphics.FillPie(new HatchBrush(HatchStyle.Percent50, objBrush.Color), x, y + iLoop2, width, height, startAngle, sweepAngle);

                }

                // Displace this pie slice from pie.
                if (startAngle == 135)
                {
                    // Show Depth
                    for (iLoop2 = 0; iLoop2 < 10; iLoop2++)
                        objGraphics.FillPie(new HatchBrush(HatchStyle.Percent50, objBrush.Color), x - 30, y + iLoop2 + 15, width, height, startAngle, sweepAngle);

                    objGraphics.FillPie(objBrush, x - 30, y + 15, width, height, startAngle, sweepAngle);
                }
                else // Draw normally
                    objGraphics.FillPie(objBrush, x, y, width, height, startAngle, sweepAngle);

            }
        }

        /// <summary>
        /// 绘制五角星
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="xc"></param>
        /// <param name="yc"></param>
        private void DrawStar(Graphics g, float r, float xc, float yc)
        {
            // r: determines the size of the star.
            // xc, yc: determine the location of the star.
            float sin36 = (float)Math.Sin(36.0 * Math.PI / 180.0);
            float sin72 = (float)Math.Sin(72.0 * Math.PI / 180.0);
            float cos36 = (float)Math.Cos(36.0 * Math.PI / 180.0);
            float cos72 = (float)Math.Cos(72.0 * Math.PI / 180.0);
            float r1 = r * cos72 / cos36;
            // Fill the star:
            PointF[] pts = new PointF[10];
            pts[0] = new PointF(xc, yc - r);
            pts[1] = new PointF(xc + r1 * sin36, yc - r1 * cos36);
            pts[2] = new PointF(xc + r * sin72, yc - r * cos72);
            pts[3] = new PointF(xc + r1 * sin72, yc + r1 * cos72);
            pts[4] = new PointF(xc + r * sin36, yc + r * cos36);
            pts[5] = new PointF(xc, yc + r1);
            pts[6] = new PointF(xc - r * sin36, yc + r * cos36);
            pts[7] = new PointF(xc - r1 * sin72, yc + r1 * cos72);
            pts[8] = new PointF(xc - r * sin72, yc - r * cos72);
            pts[9] = new PointF(xc - r1 * sin36, yc - r1 * cos36);
            g.FillPolygon(Brushes.White, pts);


            ////绘制五角星（按照美国国旗上的位置）
            //RectangleF blueBox = new RectangleF(x0, y0, 2 * width / 5, 7 * height / 13);
            //// Draw fifty stars in the blue box.
            //float offset = 1.Width / 40;
            //// Divide the blue box into a grid of 11 x 9 squares and place a star
            //// in every other square.
            //float dx = (blueBox.Width - 2 * offset) / 11;
            //float dy = (blueBox.Height - 2 * offset) / 9;
            //for (int j = 0; j < 9; j++)
            //{
            //    float yc = y0 + offset + j * dy + dy / 2;
            //    for (int i = 0; i < 11; i++)
            //    {
            //        float xc = x0 + offset + i * dx + dx / 2;
            //        if ((i + j) % 2 == 0)
            //        {
            //            DrawStar(g, this.Width / 55, xc, yc);
            //        }
            //    }
            //}
        }  

    }

    /// <summary>线类 - 用来表述绘制的线的特性
    /// 线类：两个端点，粗细，颜色
    /// </summary>
    public struct LineEx
    {
        public Point PointStart;
        public Point LocationEnd;
        public int Width;
        public Color LineColor;
    }
}
