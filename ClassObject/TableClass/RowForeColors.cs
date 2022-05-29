using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SparkFormEditor
{
    /// <summary>
    /// 每行，根据时间段范围，确定字体的颜色
    /// </summary>
    internal class RowForeColors
    {
        public DateTime DtForm;
        public DateTime DtTo;

        public Color ForeColor = Color.Black;

        //构造方法：行列和所有单元格
        public RowForeColors(DateTime fromPara, DateTime toPara, Color colorPara)
        {
            DtForm = fromPara;
            DtTo = toPara;
            ForeColor = colorPara;
        }
    }
}
