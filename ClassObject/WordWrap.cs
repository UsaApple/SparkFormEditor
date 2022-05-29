using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace SparkFormEditor
{
    ///<summary>
    ///调整段落列，回行的算法
    ///</summary>
    internal class WordWrap
    {
        /// <summary>
        /// 将文字字段换行，调整段落列的每行
        /// </summary>
        /// <param name="_str"></param>
        /// <param name="_size"></param>
        /// <param name="_font"></param>
        /// <param name="_rowHearStr"></param>
        /// <returns></returns>
        public static string lineStringAnalys(string _str, int _size, Font _font, string _rowHearStr)
        {
            string aaa = StrHalfFull.ToDBC_Half(_str);
            string NewStr = _str;

            Font messureFont = new Font(_font.FontFamily.Name, _font.Size - Comm.GetPrintFontGay(_font.Size));

            int gap = Comm._AdjustParagraphsDiff;     //2;       //每行空隙调整，尝试后的最佳值为：2。但是说行尾多处尾巴，那么可以把这个行尾间隙余白继续增加调试。

            int rowHearStrWidth = TextRenderer.MeasureText(_rowHearStr, messureFont).Width;
            if (rowHearStrWidth > 0)
            {
                rowHearStrWidth -= gap / 2;
            }

            //如果小于需要调整的长度，那么就返回；无需处理
            if (TextRenderer.MeasureText(_str, messureFont).Width + gap + rowHearStrWidth < (_size))
                return _str; // 小于回行的文字宽度，那么就返回 

            int p1 = 0, p2 = 0;
            int count;
            string nextRowFirst = "";
            int rowsCount = 0;

            //根据服务端配置属性_AdjustParagraphsNotLine，根据要求进行调整换行
            bool isNumberAdjust = false;
            if (!bool.TryParse(Comm.GetPropertyByName(Comm._AdjustParagraphsNotLine, "数字"), out isNumberAdjust))
            {
                isNumberAdjust = false;
            }

            int intLength = 0;

            string[] arrWords = Comm.GetPropertyByName(Comm._AdjustParagraphsNotLine, "词语").Split('§');
            List<string> listWords = new List<string>(arrWords);
            if (arrWords.Length == 1 && arrWords[0].Trim() == "")
            {
                listWords.Clear();
            }

            int maxWordLength = 0;
            for (int j = 0; j < arrWords.Length; j++)
            {
                if (arrWords[j].Length > maxWordLength)
                {
                    maxWordLength = arrWords[j].Length;
                }
            }

            //根据配置属性_AdjustParagraphsNotLine，根据要求进行调整换行
            for (int i = 0; i < _str.Length; i++) // Check char by char the string 
            {
                p2++;//去下一个文字

                try
                {
                    if (p1 + p2 > _str.Length)
                    {
                        break;
                    }

                    //满足下一行
                    if (TextRenderer.MeasureText(_str.Substring(p1, p2), messureFont).Width + gap + rowHearStrWidth >= (_size))
                    {
                        count = 0;

                        # region 空格分割的英语单词不拆分行
                        ////英语单词是以空格拆分单词的，所以可以根据空格来判断。不将同一个单词破坏到两行中
                        //int y = i;                   
                        //while (_str.Substring(y, 1) != " ") // 寻找最后一个字符：如果空格强行回行：因为单词不拆分开的处理look for the last word
                        //{
                        //    y--;
                        //    count++;
                        //    if (y == 0 || y <= p1) // 如果没有找到空格，那就是没有拆分开单词
                        //    {
                        //        count = 0;
                        //        break;
                        //    }
                        //}
                        # endregion 空格分割的英语单词不拆分行

                        //数字处理
                        if (isNumberAdjust)
                        {
                            //如果这行的最后一个文字是数据，并且下一个文字也是数字；那么就认为是一个连续的数字
                            if (CheckHelp.IsNumber(_str.Substring(i, 1)) && i < _str.Length - 1 && CheckHelp.IsNumber(_str.Substring(i + 1, 1)))
                            {
                                intLength = 0;
                                //取得往前几个都是数字
                                for (int j = 0; j < p2; j++)
                                {
                                    if (CheckHelp.IsNumber(_str.Substring(i - j, 1)))
                                    {
                                        intLength++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                //防止一行都是数字的时候，出现异常
                                if (intLength == p2)
                                {
                                    intLength = 0;
                                }

                                count = intLength;
                            }
                        }

                        //固定词语，遇到正好换行，不拆分的处理
                        if (listWords.Count > 0 && count == 0)
                        {
                            intLength = 0;
                            int thiswordLength = 0;
                            //1234567890ABC1234512345678901234567890abcd体温12345678901234567890ABCD脉搏
                            //病情一切正常，明日出院。此处省略N字（这里换行了，下一段落开始）一个月会随访
                            for (int j = 0; j < listWords.Count; j++)
                            {
                                if (p1 + rowsCount + p2 + maxWordLength <= NewStr.Length)
                                {
                                    intLength = NewStr.Substring(p1 + rowsCount, p2 + maxWordLength).IndexOf(listWords[j]);
                                }
                                else
                                {
                                    intLength = NewStr.Substring(p1 + rowsCount, NewStr.Length - (p1 + rowsCount)).IndexOf(listWords[j]);
                                }

                                thiswordLength = listWords[j].Length;

                                //-1表示没有找到,可能是上次回行的值在行头
                                if (intLength != -1 && intLength + thiswordLength > p2)
                                {
                                    break;
                                }
                            }

                            //30,28
                            if (intLength + thiswordLength <= p2)
                            {
                                intLength = 0; //下面会将count置为0 包含指定词语，但是不再最后位置
                            }
                            else
                            {
                                if (intLength <= p2 && intLength + thiswordLength > p2) // p2 + 1
                                {
                                    intLength = p2 - intLength;
                                }
                                else
                                {
                                    intLength = 0; //无法处理的情况，所以不处理
                                }
                            }

                            if (intLength != -1)
                            {
                                if (count < intLength)
                                {
                                    count = intLength;
                                }
                            }
                        }

                        //下一个如果是标点符号，那么还是显示在本行
                        if (i < _str.Length - 1 && count == 0)
                        {
                            //全角转换成半角
                            nextRowFirst = StrHalfFull.ToDBC_Half(_str.Substring(i + 1, 1)); //、,。;“” //一二三123456四五，六七八九ABC

                            if (nextRowFirst == "、" || nextRowFirst == "," || nextRowFirst == ";" || nextRowFirst == "。" || nextRowFirst == "." || nextRowFirst == "“" || nextRowFirst == "”")
                            {
                                count = -1;
                            }
                        }

                        p2 -= count;
                        NewStr = NewStr.Insert(p1 + p2 + rowsCount, "¤"); //NewStr = NewStr.Insert(p1 + p2, "\n");

                        //p1 += p2 + 1; //因为加上了一个:"¤"分割符号，所以这里也要+1；但是会引起上面判断时，后移了一个文字
                        p1 += p2; //所以这里不加1，但是用rowsCount记录下已经添加的"¤"分割符号行数

                        p2 = 0; //重置为零，循环里面用来记录：本行的文字个数
                        i -= count;

                        rowHearStrWidth = 0;//下一行，肯定不是第一行，不用两个空格的缩进了。

                        rowsCount++; //默认为0，每次加1，得到当前是处理第几行

                        //gap = ((int)messureFont.Size) / 2 - 1;
                    }
                }
                catch (Exception ex)
                {
                    string msg = "调整段落列计算换行时出现异常：\r\n" + _str + " | " + _size.ToString() + " | " + _font.ToString() + " | " + _rowHearStr + " | " + gap.ToString() + " | " + (p1 + p2 + rowsCount).ToString();
                    Comm.LogHelp.WriteErr(msg);
                    Comm.LogHelp.WriteErr("异常信息:{0}", ex);
                    throw ex;
                }

            }
            return NewStr;
        }
    }

}
