using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SparkFormEditor
{
    ///<summary>
    ///单元格对象类
    ///</summary>
    internal class CellInfor
    {
        public int Field = 0; //该单元格输入哪一栏

        public string CellName;
        public int RowIndex;
        public int ColIndex;

        //合并单元格
        public int RowSpan = 0;                 //纵向合并单元个数  IsMerged()来判断，本单元格是否是合并单元格
        public int ColSpan = 0;                 //横向合并单元格数

        //被合并的单元格
        public bool IsMergedNotShow = false;    //是不是被合并的单元，如果是被合并的，那就不用显示。
        public bool MergedRight = true;
        public Point MergedCell;                //被合并的单元格位置--留着扩展用，暂时没有实际作用，只是记录下来
        public int MergedCount;                 //被合并的数--留着扩展用，暂时没有实际作用，只是记录下来

        //位置大小
        public Point Loaction;
        public Size CellSize;
        public Size CellSizeOrg;                 //防止合并后再还原的时候恢复，高度可以除法来计算，但是高度不行的
                                                 //public Size CellSizeBack;
        public Rectangle Rect = Rectangle.Empty; //该单元格的区域位置

        public Point DiffLoaction;               //单元格中输入框和上下左右边界线的间隔距离

        //单元格（输入框）属性：
        public bool IsRtf_OR_Text;

        public bool SingleLine;
        public bool DoubleLine;                 //文字下面的红线

        public bool DiagonalLineZ = false;      //正对角线：/   Z、N的中间那一笔，正好是象形的表示了线的走向
        public bool DiagonalLineN = false;      //对角线：\   如果是合并单元格，还需要根据RowSpan、ColSpan来计算起点位置

        public bool NotTopmLine = false;        //如果设置成没有，那么重绘上画布的背景颜色，那就没有了

        public string Text;
        public string Rtf;
        public string DateFormat = "";

        public string CreateUser = "";

        public string EditHistory = "";

        //四条边框线颜色
        public Color CellLineColor = Color.Black;


        //实例化方法
        public CellInfor(int row, int col)
        {
            RowIndex = row;
            ColIndex = col;
        }

        /// <summary>
        /// 坐标
        /// </summary>
        /// <returns></returns>
        public Point RtbeLocation()
        {
            return new Point(Loaction.X + DiffLoaction.X, Loaction.Y + DiffLoaction.Y);
        }

        /// <summary>
        /// 是否为合并单元格
        /// </summary>
        /// <returns></returns>
        public bool IsMerged()
        {
            if (RowSpan == 0 && ColSpan == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
