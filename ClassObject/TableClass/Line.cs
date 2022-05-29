using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SparkFormEditor
{
    /// <summary>
    /// 绘制表格线的顺序：1.默认线，2.行线，3.单元格定义线
    /// </summary>
    internal class Line
    {
        public Point PointStart;    //开始位置
        public Point LocationEnd;
        public int Width = 1;       //默认为1
        public Color LineColor;     //颜色
    }
}
