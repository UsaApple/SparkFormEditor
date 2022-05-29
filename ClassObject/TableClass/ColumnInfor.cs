using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SparkFormEditor
{
    ///<summary>
    ///列对象类
    ///</summary>
    internal class ColumnInfor
    {
        public string ColName;
        public int ColIndex;

        public int ColWidth;

        public int ColLocationX;

        //列、属性：首先将每列的一个输入框设置保存下来，以便备用
        public TransparentRTBExtended RTBE; //全部使用透明的，反正一列一个，一共没有多少个。方便处理

        public string GroupName;  //列的分组：点击某一列，显示组内项目，可以编辑

        /// <summary>
        /// 构造方法
        /// </summary>
        public ColumnInfor()
        {
            ColName = "";
        }
    }
}
