
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace SparkFormEditor
{
    ///<summary>
    /// 表单里面的标签：用来显示的姓名、住院号等
    ///</summary>
    internal partial class LabelExtended : Label
    {
        public string _Default = ""; //默认值
        public string _Score = ""; //评分表达式
        public string _YMD = "";   //入院日期的格式，可能需要时分秒

        public string _Size = "";   //配置的大小，以便回行处理的
        public Point _Location;     //配置的位置坐标
        public bool _IsPrintAll = false;

        //public string _PageNo = ""; //如果页码格式串比较复杂，那么text就不是数字了。存储在这里，当前页码。但是修改最后一页的话。最大页码也要跟着变的。

        public LabelExtended()
        {
            InitializeComponent();

            //bChanging = false;
            //this.UseCompatibleTextRendering = true; 
        }

        /// <summary>
        /// 默认值读取、设置
        /// </summary>
        public string Default
        {
            get { return _Default; }
            set
            {
                _Default = value;
                this.Text = value;
            }
        }

        /// <summary>
        /// 评分表达式
        /// </summary>
        public string Score
        {
            get { return _Score; }
            set
            {
                _Score = value;
            }
        }

        public void ResetDefault()
        {
            this.Text = _Default;
        }

        //大小改变的时候
        private void LabelExtended_SizeChanged(object sender, EventArgs e)
        {
            if (!_IsPrintAll)
            {
                ResetMySize(); //初始化显示的时候
            }
        }

        private void LabelExtended_TextChanged(object sender, EventArgs e)
        {
            if (!_IsPrintAll)
            {
                ResetMySize(); //界面直接修改的时候
            }
        }

        /// <summary>
        /// 根据配置的大小，自适应回行
        /// 取消默认的自动自适应大小，变成手动控制大小
        /// </summary>
        private void ResetMySize()
        {
            try
            {
                if (this.Tag != null && this.Name != "_PageNo")
                {
                    if (this.Tag.ToString().TrimEnd('¤').Contains(","))
                    {
                        //bChanging = true;
                        Size newS = Comm.getSize(this.Tag.ToString().TrimEnd('¤')).ToSize();

                        int witdh = TextRenderer.MeasureText(this.Text, this.Font).Width;

                        if (newS.Width > 0 && newS.Height > 0)
                        {
                            if (witdh > newS.Width || this.Width > newS.Width)
                            {
                                newS.Width = newS.Width - 4;
                                this.AutoSize = false;
                                this.Size = newS; //打印时也要处理

                                if (!bChanging)
                                {
                                    Bitmap bmp = new Bitmap(newS.Width, newS.Height);
                                    Graphics graphics = Graphics.FromImage(bmp);
                                    Size size = new Size();

                                    try
                                    {
                                        size = MeasureStringExtended(graphics, this.Text, this.Font, this.Width).ToSize();
                                        if (this.Height > size.Height)
                                        {
                                            this.Height = size.Height;
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        //如果计算高度的方法异常，那么设置为设置的最大高度
                                        this.Height = newS.Height;
                                        Comm.LogHelp.WriteErr(ex);
                                    }

                                    bmp.Dispose();
                                    graphics.Dispose();

                                    //string temp = this.Text;
                                    //this.Text = "";
                                    //this.Refresh();//有事打开会遮住下面，很奇怪的现象
                                    //this.Text = temp;
                                }
                            }
                            else
                            {
                                this.AutoSize = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        //自使用行高
        private bool bChanging = false;

        public SizeF MeasureStringExtended(Graphics g, string text, Font font, int desWidth)
        {
            string tempString = text;
            string workString = "";
            string outputstring = "";
            int npos = 1;
            int sp_pos = 0;
            int sp_pos1 = 0;
            int sp_pos2 = 0;
            int sp_pos3 = 0;
            bool bNeeded = false;
            int line = 0;
            int nWidth = 0;

            //get original size 
            SizeF size = g.MeasureString(text, font);

            if (size.Width > desWidth)
            {
                while (tempString.Length > 0)
                {
                    //Check for the last lane 
                    if (npos > tempString.Length)
                    {
                        outputstring = outputstring + tempString;
                        line++;
                        break;
                    }
                    workString = tempString.Substring(0, npos);
                    //get the current width 
                    nWidth = (int)g.MeasureString(workString, font).Width;
                    //check if we've got out of the destWidth 
                    if (nWidth > desWidth)
                    {
                        //try to find a space 

                        sp_pos1 = workString.LastIndexOf(" ");
                        sp_pos2 = workString.LastIndexOf(";");
                        sp_pos3 = workString.IndexOf("\r\n");
                        //sp_pos3 = workString.IndexOf("\n");
                        if (sp_pos3 > 0)
                        {
                            sp_pos = sp_pos3;
                            bNeeded = false;
                        }
                        else
                        {
                            if ((sp_pos2 > 0) && (sp_pos2 > sp_pos1))
                            {
                                sp_pos = sp_pos2;
                                bNeeded = true;
                            }
                            else if (sp_pos1 > 0)
                            {
                                sp_pos = sp_pos1;
                                bNeeded = true;
                            }
                            else
                            {
                                sp_pos = 0;
                                bNeeded = true;
                            }
                        }

                        if (sp_pos > 0)
                        {
                            //cut out the wrap lane we've found 
                            outputstring = outputstring + tempString.Substring(0, sp_pos + 1);
                            if (bNeeded) outputstring = outputstring + "\r\n";
                            //if (bNeeded) outputstring = outputstring + "\n";
                            tempString = tempString.Substring((sp_pos + 1), tempString.Length - (sp_pos + 1));
                            line++;
                            npos = 0;
                        }
                        else //no space 
                        {
                            ////outputstring = outputstring + tempString.Substring(0, npos + 1);
                            //if (npos + 1 <= tempString.Length) //不然会报错
                            //{
                            //    outputstring = outputstring + tempString.Substring(0, npos + 1);

                            //    if (bNeeded) outputstring = outputstring + "\r\n";

                            //    tempString = tempString.Substring(npos, tempString.Length - npos);
                            //    line++;
                            //    npos = 0;
                            //}
                            //else
                            //{
                            //    outputstring = outputstring + tempString;
                            //    tempString = "";

                            //    //if (bNeeded) 
                            //        outputstring = outputstring + "\r\n";

                            //    //tempString = tempString.Substring(npos, tempString.Length - npos);
                            //    line++;
                            //    npos = 0;
                            //}

                            //索引和长度必须引用该字符串内的位置。
                            //if (npos + 1 <= tempString.Length)
                            //{
                                outputstring = outputstring + tempString.Substring(0, npos + 1);
                            //}
                            //else
                            //{
                            //    outputstring = outputstring + tempString.Substring(0, npos);
                            //}

                            if (bNeeded) outputstring = outputstring + "\r\n";
                            //if (bNeeded) outputstring = outputstring + "\n";

                            tempString = tempString.Substring(npos, tempString.Length - npos);
                            line++;
                            npos = 0;
                        }
                    }
                    npos++;
                }
            }
            else
            {
                outputstring = text;
            }

            //SizeF returnSize = g.MeasureString(outputstring, font);

            SizeF returnSize = g.MeasureString(outputstring, new Font(font.Name,font.Size + 0.5f)); //可能高度还是不够会有遮挡
            returnSize = new SizeF(returnSize.Width, returnSize.Height + 2);
 
            return returnSize;
        }

        //protected override void OnTextChanged(EventArgs e)
        //{
        //    if (!bChanging)
        //    {
        //        Bitmap bmp = new Bitmap(175, 175);
        //        Graphics graphics = Graphics.FromImage(bmp);
        //        SizeF size = new SizeF();

        //        size = MeasureStringExtended(graphics, this.Text, this.Font, 175);
        //        this.Height = (int)size.Height;
        //        base.OnTextChanged(e);
        //    }
        //}
    }
}
