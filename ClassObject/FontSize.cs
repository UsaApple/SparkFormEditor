using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SparkFormEditor
{
    ///<summary>
    ///取得系统的字体类型和大小
    ///</summary>
    internal class FontSize
    {
        private static List<FontSize> allFontSize = null;
        public static List<FontSize> All
        {
            get
            {
                if (allFontSize == null)
                {
                    allFontSize = new List<FontSize>();
                    allFontSize.Add(new FontSize(8, 1));
                    allFontSize.Add(new FontSize(9, 2));
                    allFontSize.Add(new FontSize(10, 3));
                    allFontSize.Add(new FontSize(12, 4));
                    allFontSize.Add(new FontSize(14, 5));
                    allFontSize.Add(new FontSize(18, 6));
                    allFontSize.Add(new FontSize(24, 7));
                    allFontSize.Add(new FontSize(36, 8));
                    allFontSize.Add(new FontSize(48, 9));
                }

                return allFontSize;
            }
        }
        public static FontSize Find(int value)
        {
            if (value < 1)
            {
                return All[0];
            }

            if (value > 7)
            {
                return All[6];
            }

            return All[value - 1];
        }

        private FontSize(int display, int value)
        {
            displaySize = display;
            valueSize = value;
        }

        private int valueSize;
        public int Value
        {
            get
            {
                return valueSize;
            }
        }

        private int displaySize;
        public int Display
        {
            get
            {
                return displaySize;
            }
        }

        public override string ToString()
        {
            return displaySize.ToString();
        }
    }
}
