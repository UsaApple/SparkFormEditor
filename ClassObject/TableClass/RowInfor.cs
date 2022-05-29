using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SparkFormEditor
{
    ///<summary>
    ///行对象类
    ///</summary>
    internal class RowInfor
    {
        public int RowIndex;

        //位置大小
        public Point Loaction;
        public Size RowSize;

        //四条边框线颜色
        public Color RowLineColor = Color.Black;
        public string SumType = "";                         //空表示明细行普通行，非空，表示具体的小计、合计行。
        public LineTypes CustomLineType = LineTypes.None;   //行的双红线样式
        public Color CustomLineColor = Color.Red;           //行的双红线颜色
        public string RowForeColor = "";                    //根据时间自动变色行的颜色，没有则为空
        public int ID = -1;                                 //对应xml数据中的节点的索引值，是唯一确定这行数据对应xml中那个节点的。跟节点属性中MaxID每次加1
        public string UserID = "";                          //该行的签名用户ID
        public string UserName = "";                        //该行的签名用户ID
        public string NurseFormLevel = "";                      //权限等级

        //public bool IsSumRow = false;                       //合计行，根据SumRowAttention="∑,Red" 在行头提示该行是合计行。根据SumType来判断是否合计行也行

        /// <summary>
        /// 构造方法：
        /// </summary>
        /// <param name="row">行号</param>
        public RowInfor(int row)
        {
            RowIndex = row;
        }
    }
}
