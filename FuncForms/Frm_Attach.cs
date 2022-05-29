using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text.RegularExpressions;
using SparkFormEditor.Model;

namespace SparkFormEditor
{
    internal partial class Frm_Attach : Form
    {
        XmlNode _XnTemplate = null; //附表模板：可用编辑器配置后，拷贝到表单模板的附表区域
        XmlNode _XnData = null;     //数据
        XmlDocument _RecruitXML = null;

        public double RetValue = 0; //返回值

        public Frm_Attach()
        {
            InitializeComponent();
        }

        public Frm_Attach(XmlNode xnTemplate, XmlNode xnData, XmlDocument recruitXML)
            : this()
        {
            _XnTemplate = xnTemplate;

            _XnData = xnData;

            _RecruitXML = recruitXML;

            if (!string.IsNullOrEmpty((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Size)).Trim()))
            {
                //this.Size = Comm.getSize((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();
                //this.ClientSize = Comm.getSize((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();
                //System.Windows.Forms.SystemInformation.VirtualScreen 包含系统任务栏
                //Screen.PrimaryScreen.WorkingArea.Size 不包含系统任务栏
                Size sz = Comm.getSize((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();
                if (sz.Width > Screen.PrimaryScreen.WorkingArea.Size.Width)
                {
                    sz = new Size(Screen.PrimaryScreen.WorkingArea.Size.Width, sz.Height);
                }

                if (sz.Height > Screen.PrimaryScreen.WorkingArea.Size.Height)
                {
                    sz = new Size(sz.Width, Screen.PrimaryScreen.WorkingArea.Size.Height);
                }

                this.Size = sz;
            }
        }

        public double SumResult { get { return RetValue; } }
        public XmlNode FormData { get { return _XnData; } }

        /// <summary>
        /// 加载附表模板和数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttachForm_Load(object sender, EventArgs e)
        {
            try
            {
                #region 加载模板
                if (_XnTemplate != null)
                {
                    //窗体标题和大小设置
                    this.Text = (_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Text));

                    if (!string.IsNullOrEmpty((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Size)).Trim()))
                    {
                        //this.Size = Comm.getSize((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();
                        //this.ClientSize = Comm.getSize((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();

                        //防止附表内容过多，需要出现滚动条，否则不能显示下
                        Size sz = Comm.getSize((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();
                        if (sz.Width > this.Width || sz.Height > this.Height)
                        {
                            this.pictureBoxBackGround.Dock = DockStyle.None;

                            if (sz.Height > this.Height)
                            {
                                this.pictureBoxBackGround.Size = new Size(this.panel1.Width - panel1.Margin.Left - panel1.Margin.Right - 20, sz.Height);
                            }

                            if (sz.Width > this.Width)
                            {
                                this.pictureBoxBackGround.Size = new Size(sz.Width, this.panel1.Height - panel1.Margin.Top - panel1.Margin.Bottom - 20);
                            }
                        }
                    }

                    Bitmap bmpBack = new Bitmap(pictureBoxBackGround.Width, pictureBoxBackGround.Height);
                    Graphics.FromImage(bmpBack).Clear(Color.White);
                    pictureBoxBackGround.Image = (Bitmap)bmpBack.Clone();

                    Graphics g = Graphics.FromImage(this.pictureBoxBackGround.Image);
                    //消除锯齿，平滑
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    //g.PixelOffsetMode = PixelOffsetMode.HighQuality; //高像素偏移质量   
                    //g.CompositingQuality = CompositingQuality.HighQuality;

                    //动态加载控件项目：暂时只支持：String,勾选框，输入框，下拉框即可
                    //类似表单
                    XmlNode xnlEach;
                    Font eachFont = new Font("宋体", 9F, FontStyle.Regular);
                    Pen eachPen = new Pen(Color.Black, 1);
                    SolidBrush sBrrsh;
                    int x = 0;
                    int y = 0;
                    int a = 0;
                    int b = 0; //坐标临时值
                    bool tempBool;
                    int tempInt;
                    string tempString;
                    HorizontalAlignment inputBoxAlignment;

                    RichTextBoxExtended rtbeNew;
                    CheckBoxExtended chkbNew;
                    ComboBoxExtended combNew;

                    //顺序读取详细配置信息的每个节点，进行设置
                    for (int i = 0; i < _XnTemplate.ChildNodes.Count; i++)
                    {
                        //如果是注释那么跳过
                        if (_XnTemplate.ChildNodes[i].Name == @"#comment" || _XnTemplate.ChildNodes[i].Name == @"#text")
                        {
                            continue;
                        }

                        xnlEach = _XnTemplate.ChildNodes[i];

                        if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Alignment)).ToLower() == "left")
                        {
                            inputBoxAlignment = HorizontalAlignment.Left;
                        }
                        else if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Alignment)).ToLower() == "right")
                        {
                            inputBoxAlignment = HorizontalAlignment.Right;
                        }
                        else
                        {
                            inputBoxAlignment = HorizontalAlignment.Center;//默认居中
                        }

                        switch (xnlEach.Name)
                        {
                            case "Rect":
                                //设置外框参数
                                x = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size)), 0);//边框宽
                                y = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size)), 1);//边框高

                                a = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0);//边框距左边缘空白距离
                                b = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1);//边框距上边缘空白距离


                                //绘制参数：颜色，和字体大小
                                //绘制外框
                                g.DrawRectangle(new Pen(Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color))),
                                    Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)), 0)), new Rectangle(a, b, x, y));

                                break;
                            case "Line":
                                eachPen = new Pen(Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color))), float.Parse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width))));
                                a = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0);//边框距左边缘空白距离
                                b = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1);//边框距上边缘空白距离

                                x = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationEnd)), 0);//边框距左边缘空白距离
                                y = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationEnd)), 1);//边框距上边缘空白距离

                                tempString = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LineType)).Trim();
                                if (tempString != "")
                                {
                                    if (tempString.ToLower() == "dash")
                                    {
                                        eachPen.DashStyle = DashStyle.Dash;
                                    }
                                    else if (tempString.ToLower() == "dashdot")
                                    {
                                        eachPen.DashStyle = DashStyle.DashDot;
                                    }
                                    else if (tempString.ToLower() == "dot")
                                    {
                                        eachPen.DashStyle = DashStyle.Dot;
                                    }
                                    else if (tempString.ToLower() == "solid")
                                    {
                                        eachPen.DashStyle = DashStyle.Solid;
                                    }
                                    else if (tempString.ToLower() == "dashdotdot")
                                    {
                                        eachPen.DashStyle = DashStyle.DashDotDot;
                                    }
                                    else
                                    {
                                        eachPen.DashStyle = DashStyle.Custom;
                                    }
                                }

                                if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Repeat)) == "")//大多时候不需要重复绘制
                                {
                                    g.DrawLine(eachPen,
                                        new Point(a, b),
                                        new Point(x, y));
                                }
                                else//重复绘制
                                {
                                    string repeatStr = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Repeat));
                                    int count = int.Parse(repeatStr.Split(',')[2].Trim());
                                    float repeatAdd = float.Parse(repeatStr.Split(',')[1].Trim());

                                    if (repeatStr.Split(',')[0].Trim() == "East")//水平重复
                                    {
                                        for (int j = 0; j <= count; j++)
                                        {
                                            g.DrawLine(eachPen,
                                                    new Point(a + (int)(repeatAdd * j), b),
                                                    new Point(x + (int)(repeatAdd * j), y));
                                        }
                                    }
                                    else//纵向重复
                                    {
                                        for (int j = 0; j <= count; j++)
                                        {
                                            g.DrawLine(eachPen,
                                                   new Point(a, b + (int)(repeatAdd * j)),
                                                    new Point(x, y + (int)(repeatAdd * j)));
                                        }
                                    }
                                }

                                break;
                            case nameof(EntXmlModel.Lable):
                                eachFont = Comm.getFont((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)));

                                if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).Trim().ToLower().Contains(nameof(EntXmlModel.bold)) &&
                                    (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).Trim().ToLower().Contains(nameof(EntXmlModel.italic)))
                                {
                                    eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size, FontStyle.Bold | FontStyle.Italic);
                                }
                                else if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).Trim().ToLower().Contains(nameof(EntXmlModel.bold)))
                                {
                                    eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size, FontStyle.Bold);
                                }
                                else if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).Trim().ToLower().Contains(nameof(EntXmlModel.italic)))
                                {
                                    eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size, FontStyle.Italic);
                                }
                                else
                                {
                                    eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size, FontStyle.Regular);
                                }

                                sBrrsh = new SolidBrush(Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color))));

                                if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Repeat)) == "")//大多时候不需要重复绘制
                                {
                                    g.DrawString((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)),
                                                eachFont,
                                                sBrrsh,
                                                new PointF(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0), Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1)));
                                }
                                else//重复绘制
                                {
                                    string repeatStr = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Repeat));
                                    int count = int.Parse(repeatStr.Split(',')[2].Trim());
                                    int repeatAdd = int.Parse(repeatStr.Split(',')[1].Trim());
                                    if (repeatStr.Split(',')[0].Trim() == "East")//水平重复
                                    {
                                        for (int j = 0; j <= count; j++)
                                        {
                                            g.DrawString((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)),
                                                        eachFont,
                                                        sBrrsh,
                                                        new PointF(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + repeatAdd * j, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1)));
                                        }
                                    }
                                    else//纵向重复
                                    {
                                        for (int j = 0; j <= count; j++)
                                        {
                                            g.DrawString((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)),
                                                        eachFont,
                                                        sBrrsh,
                                                        new PointF(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0), Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + repeatAdd * j));
                                        }
                                    }
                                }

                                break;
                            case nameof(EntXmlModel.Text): //输入框 和透明输入框一样

                                eachFont = Comm.getFont((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)));

                                rtbeNew = new RichTextBoxExtended();

                                rtbeNew._InputBoxAlignment = inputBoxAlignment;
                                rtbeNew.SetAlignment();

                                rtbeNew.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                                rtbeNew.Location = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0), Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1));
                                rtbeNew.ForeColor = Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                                rtbeNew.DefaultForeColor = rtbeNew.ForeColor;
                                rtbeNew.Font = eachFont;
                                rtbeNew.Size = Comm.getSize((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();


                                if (rtbeNew.Name.StartsWith("日期") || rtbeNew.Name.Contains("日期") || rtbeNew.Name.Contains("时间"))
                                {
                                    rtbeNew._YMD = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.DateFormat)); //默认的日期格式
                                    rtbeNew._YMD_Default = rtbeNew._YMD;
                                }

                                if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Multiline)), out tempBool))
                                {
                                    tempBool = false;
                                }
                                rtbeNew.Multiline = tempBool;
                                rtbeNew.WordWrap = false; //要程序控制换行，如果自动换行，换行文字位置在打印和输入框中显示会不一样

                                rtbeNew._Default = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)); //rtbeNew.Default = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default));
                                rtbeNew.Score = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Score));
                                rtbeNew.SetSelectItems((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.MenuHelpString))); //右击菜单的选择项

                                if (!int.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.MaxLength)), out tempInt))
                                {
                                    tempInt = 50;
                                }

                                rtbeNew.MaxLength = tempInt;
                                rtbeNew._MaxLength = rtbeNew.MaxLength;

                                this.pictureBoxBackGround.Controls.Add(rtbeNew);

                                break;
                            case nameof(EntXmlModel.CheckBox): //勾选框
                                chkbNew = new CheckBoxExtended();
                                //chkbNew.BackColor = Color.White; //将底板pic的背景也设置为白色就不会有灰色区域了
                                eachFont = Comm.getFont((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)));

                                if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)), out tempBool))
                                {
                                    tempBool = false;
                                }
                                chkbNew.Default = tempBool;

                                chkbNew.Score = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Score));

                                //文字和框的位置样式:左右位置交换
                                tempString = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.CheckAlign));
                                if (string.IsNullOrEmpty(tempString) || tempString.ToLower() == "left")
                                {
                                    chkbNew.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
                                }
                                else
                                {
                                    chkbNew.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
                                }

                                chkbNew.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                                chkbNew.Text = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.ShowValue)); //chkbNew.Name;

                                chkbNew.Location = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0), Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1));
                                chkbNew.ForeColor = Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                                chkbNew.Font = eachFont;
                                chkbNew.AutoSize = true;

                                this.pictureBoxBackGround.Controls.Add(chkbNew);

                                break;
                            case nameof(EntXmlModel.ComboBox): //下拉框

                                combNew = new ComboBoxExtended();

                                if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Items)) != "")
                                {
                                    combNew.Items.AddRange((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Items)).ToString().Split(','));
                                }

                                combNew.Tag = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Items)); //动态脚本执行，可能用到，比如选项清空后再还原

                                //下拉样式
                                if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).ToLower() != "dropdown")
                                {
                                    combNew.DropDownStyle = ComboBoxStyle.DropDownList;
                                }
                                else
                                {
                                    combNew.DropDownStyle = ComboBoxStyle.DropDown;
                                }

                                if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size)) != "")
                                {
                                    combNew.Size = Comm.getSize((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();

                                    //下拉宽度和高度
                                    //combNew.DropDownWidth = combNew.Size.Width;
                                    //combNew.DropDownHeight = combNew.Size.Height;
                                }

                                eachFont = Comm.getFont((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)));

                                combNew.Default = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default));
                                combNew.Score = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Score));

                                combNew.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                                combNew.Location = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0), Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1));
                                combNew.ForeColor = Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                                combNew.Font = eachFont;

                                this.pictureBoxBackGround.Controls.Add(combNew);
                                break;
                        }
                    }
                }
                #endregion 加载模板


                //加载数据
                //"Attach/Form[@Name='" + (_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Name)).Trim() + "']"
                XmlNode xnThisForm = _XnData.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.Name), (_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Name)).Trim(), "=", nameof(EntXmlModel.Attach), nameof(EntXmlModel.Form))); //Attach/Form
                if (xnThisForm != null)
                {
                    //类似表单,载入数据
                    Control[] ct;

                    //编辑本页数据xml节点的所有属性，找到控件进行赋值，遍历所有节点属性
                    foreach (XmlAttribute xe in xnThisForm.Attributes)
                    {
                        ct = this.pictureBoxBackGround.Controls.Find(xe.Name, false);

                        if (ct != null && ct.Length > 0)
                        {

                            if (ct[0] is RichTextBoxExtended)
                            {
                                ((RichTextBoxExtended)ct[0]).SetAlignment(); //默认的对齐方式

                                HorizontalAlignment temp = ((RichTextBoxExtended)ct[0]).SelectionAlignment;
                                ((RichTextBoxExtended)ct[0]).Text = xe.Value;
                                ((RichTextBoxExtended)ct[0]).SelectionAlignment = temp;
                            }
                            else if (ct[0] is CheckBoxExtended)
                            {
                                try
                                {
                                    ((CheckBoxExtended)ct[0]).Checked = bool.Parse(xe.Value);
                                }
                                catch (Exception ex)
                                {
                                    string msgErr = "勾选框设置的时候发现，上次保存的值不是布尔值，请确认是否存在同名的其他非勾选框控件：" + ct[0].Name;
                                    Comm.LogHelp.WriteErr(msgErr + Environment.NewLine + "异常信息:" + ex.ToString());
                                    MessageBox.Show(msgErr, "数据错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    //throw ex;
                                }
                            }
                            else if (ct[0] is ComboBoxExtended)
                            {
                                ((ComboBoxExtended)ct[0]).Text = xe.Value;
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

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            //保存数据，以便下次打开再现。
            SaveData();

            //类似表单的合计功能，赋值给RetValue
            if (!string.IsNullOrEmpty((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Score))))
            {
                double score = 0;
                if (!double.TryParse((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Score)), out score))
                {
                    score = 0;
                }

                RetValue = GetSum();

                if (score != 0)
                {
                    //RetValue = 100 - RetValue; //如果设置的不是0，那么就表示总分减法
                    RetValue = score - RetValue; //上面始终是100去减法不合适
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        private void SaveData()
        {
            try
            {
                //如果还没有数据，那么创建一个新的节点 XmlNode currtAttach = thisPage.SelectSingleNode("Attach/Form[@Name='" + rtbe._AttachForm + "']");
                if (_XnData.SelectSingleNode(nameof(EntXmlModel.Attach)) == null)
                {
                    XmlNode ndAttach = _RecruitXML.CreateElement(nameof(EntXmlModel.Attach));
                    _XnData.AppendChild(ndAttach);
                }
                //"Attach/Form[@Name='" + (_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Name)).Trim() + "']"
                if (_XnData.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.Name), (_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Name)).Trim(), "=", nameof(EntXmlModel.Attach), nameof(EntXmlModel.Form))) == null)   //Attach / Form
                {
                    XmlNode ndFrom = _RecruitXML.CreateElement(nameof(EntXmlModel.Form));
                    (ndFrom as XmlElement).SetAttribute(nameof(EntXmlModel.Name), (_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Name)).Trim());
                    _XnData.SelectSingleNode(nameof(EntXmlModel.Attach)).AppendChild(ndFrom);
                }

                XmlNode xnThisForm = _XnData.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.Name), (_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Name)).Trim(), "=", nameof(EntXmlModel.Attach), nameof(EntXmlModel.Form)));

                //遍历pictureBoxBackGround画布上所有控件，进行保存
                if (this.pictureBoxBackGround.Controls.Count > 0)
                {
                    //界面上删除所有的以前无效的辅助线图标mark
                    foreach (Control control in this.pictureBoxBackGround.Controls)
                    {

                        //保存控件数据：
                        if (control is RichTextBoxExtended)
                        {
                            (xnThisForm as XmlElement).SetAttribute(((RichTextBoxExtended)control).Name, ((RichTextBoxExtended)control).Text);
                        }
                        else if (control is CheckBoxExtended)
                        {
                            (xnThisForm as XmlElement).SetAttribute(((CheckBoxExtended)control).Name, ((CheckBoxExtended)control).Checked.ToString());
                        }
                        else if (control is ComboBoxExtended)
                        {
                            (xnThisForm as XmlElement).SetAttribute(((ComboBoxExtended)control).Name, ((ComboBoxExtended)control).Text);
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

        /// <summary>
        /// 返回合计
        /// </summary>
        /// <returns></returns>
        private double GetSum()
        {
            try
            {
                double sumAll = 0;
                string exeStr = (_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Total));

                //获取不到合计值的时候，再根据评分来合计
                string scoreScript = "";
                string[] eachControl;
                string[] eachArr;

                //合计公式不为空，那么表示设置了合计公式
                if (!string.IsNullOrEmpty(exeStr))
                {

                    ////如果是直接的表达式
                    ////将输入框的数值，结合公式代替后计算
                    if (exeStr.StartsWith("∑"))
                    {
                        exeStr = exeStr.TrimStart('∑');
                        //sumAll = Evaluator.EvaluateToInteger(exeStr);

                        sumAll = Caculate_ExeculateStr(exeStr, false); //上面的只能纯表达式，现在可以将输入框名字的值代替计算

                        return sumAll;
                    }

                    Control[] ct;
                    double valueEach = 0;


                    //目前支持和excel一样的公式：A1:AN, A1+A2+AN……
                    if (exeStr.Contains(":"))
                    {
                        string[] arr = exeStr.Split(':');
                        int index = StringNumber.getLastNum(arr[0]);
                        string controlName = arr[0].Replace(index.ToString(), "");

                        int indexEnd = StringNumber.getLastNum(arr[1]);

                        for (int i = index; i <= indexEnd; i++)
                        {
                            ct = this.pictureBoxBackGround.Controls.Find(controlName + i.ToString(), false);

                            if (ct != null && ct.Length > 0)
                            {
                                if (!string.IsNullOrEmpty(ct[0].Text.Trim()))
                                {
                                    valueEach = StringNumber.getFirstNum(ct[0].Text.Trim());

                                    if (ct[0] is CheckBoxExtended)
                                    {
                                        //输入框和下拉框，一般是根据文字内容来合计的
                                        valueEach = -1; //勾选框肯定是根据选中不选中来配置，输入框等如果数值没有，还是不用评分来，不然如果为空，就用评分，会错乱
                                    }

                                    if (valueEach != -1) //取到数据的时候
                                    {
                                        sumAll += valueEach;
                                    }
                                    else
                                    {
                                        //如果为空，那么在判断表单的评分设定：
                                        scoreScript = "";
                                        if (ct[0] is CheckBoxExtended)
                                        {
                                            scoreScript = ((CheckBoxExtended)ct[0]).Score;

                                            if (!string.IsNullOrEmpty(scoreScript))
                                            {
                                                eachControl = scoreScript.Split('¤');
                                                for (int j = 0; j < eachControl.Length; j++)
                                                {
                                                    eachArr = eachControl[j].Split('@');

                                                    if (((CheckBoxExtended)ct[0]).Checked.ToString().ToLower() == eachArr[0].ToLower().Trim())
                                                    {
                                                        sumAll += double.Parse(eachArr[1].Trim());
                                                        break;//找到第一个满足评分的项目就跳出
                                                    }
                                                }
                                            }
                                        }
                                        else if (ct[0] is RichTextBoxExtended)
                                        {
                                            scoreScript = ((RichTextBoxExtended)ct[0]).Score;

                                            if (!string.IsNullOrEmpty(scoreScript))
                                            {
                                                eachControl = scoreScript.Split('¤');
                                                for (int j = 0; j < eachControl.Length; j++)
                                                {
                                                    eachArr = eachControl[j].Split('@');

                                                    if (((RichTextBoxExtended)ct[0]).Text == eachArr[0].ToLower().Trim())
                                                    {
                                                        sumAll += double.Parse(eachArr[1].Trim());
                                                        break;//找到第一个满足评分的项目就跳出
                                                    }
                                                }
                                            }
                                        }
                                        else if (ct[0] is ComboBoxExtended)
                                        {
                                            scoreScript = ((ComboBoxExtended)ct[0]).Score;

                                            if (!string.IsNullOrEmpty(scoreScript))
                                            {
                                                eachControl = scoreScript.Split('¤');
                                                for (int j = 0; j < eachControl.Length; j++)
                                                {
                                                    eachArr = eachControl[j].Split('@');

                                                    if (((ComboBoxExtended)ct[0]).Text == eachArr[0].ToLower().Trim())
                                                    {
                                                        sumAll += double.Parse(eachArr[1].Trim());
                                                        break;//找到第一个满足评分的项目就跳出
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }


                        return sumAll;

                    }
                    else  //if (exeStr.Contains("+"))
                    {
                        string[] arr = exeStr.Split('+');

                        for (int i = 0; i < arr.Length; i++)
                        {
                            ct = this.pictureBoxBackGround.Controls.Find(arr[i], false);

                            if (ct != null && ct.Length > 0)
                            {
                                if (!string.IsNullOrEmpty(ct[0].Text.Trim()))
                                {
                                    valueEach = StringNumber.getFirstNum(ct[0].Text.Trim());

                                    if (ct[0] is CheckBoxExtended)
                                    {
                                        //输入框和下拉框，一般是根据文字内容来合计的
                                        valueEach = -1; //勾选框肯定是根据选中不选中来配置，输入框等如果数值没有，还是不用评分来，不然如果为空，就用评分，会错乱
                                    }

                                    if (valueEach != -1)
                                    {
                                        sumAll += valueEach;
                                    }
                                    else
                                    {
                                        //如果为空，那么在判断表单的评分设定：
                                        scoreScript = "";
                                        if (ct[0] is CheckBoxExtended)
                                        {
                                            scoreScript = ((CheckBoxExtended)ct[0]).Score;

                                            if (!string.IsNullOrEmpty(scoreScript))
                                            {
                                                eachControl = scoreScript.Split('¤');
                                                for (int j = 0; j < eachControl.Length; j++)
                                                {
                                                    eachArr = eachControl[j].Split('@');

                                                    if (((CheckBoxExtended)ct[0]).Checked.ToString().ToLower() == eachArr[0].ToLower().Trim())
                                                    {
                                                        sumAll += double.Parse(eachArr[1].Trim());
                                                        break;//找到第一个满足评分的项目就跳出
                                                    }
                                                }
                                            }
                                        }
                                        else if (ct[0] is RichTextBoxExtended)
                                        {
                                            scoreScript = ((RichTextBoxExtended)ct[0]).Score;

                                            if (!string.IsNullOrEmpty(scoreScript))
                                            {
                                                eachControl = scoreScript.Split('¤');
                                                for (int j = 0; j < eachControl.Length; j++)
                                                {
                                                    eachArr = eachControl[j].Split('@');

                                                    if (((RichTextBoxExtended)ct[0]).Text == eachArr[0].ToLower().Trim())
                                                    {
                                                        sumAll += double.Parse(eachArr[1].Trim());
                                                        break;//找到第一个满足评分的项目就跳出
                                                    }
                                                }
                                            }
                                        }
                                        else if (ct[0] is ComboBoxExtended)
                                        {
                                            scoreScript = ((ComboBoxExtended)ct[0]).Score;

                                            if (!string.IsNullOrEmpty(scoreScript))
                                            {
                                                eachControl = scoreScript.Split('¤');
                                                for (int j = 0; j < eachControl.Length; j++)
                                                {
                                                    eachArr = eachControl[j].Split('@');

                                                    if (((ComboBoxExtended)ct[0]).Text == eachArr[0].ToLower().Trim())
                                                    {
                                                        sumAll += double.Parse(eachArr[1].Trim());
                                                        break;//找到第一个满足评分的项目就跳出
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //写入值
                        return sumAll;

                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 计算直接的表达式结果的扩展，
        /// 处理有些项目是输入框的名字来取值
        /// </summary>
        /// <param name="exeStr"></param>
        /// <param name="isTable"></param>
        /// <returns></returns>
        private int Caculate_ExeculateStr(string exeStr, bool isTable)
        {
            int ret = 0;

            Control[] ct;

            string finalExeStr = exeStr;

            //MatchCollection matchs2 = Regex.Matches("(20 - [积极因子分1] +  - [积极因子分x])*2", @"(?<=\[)[^]]*?(?=\])");
            MatchCollection matchsExeStr = Regex.Matches(exeStr, @"(?<=\[)[^]]*?(?=\])"); //中括号[]把动态控件名包起来，已区分。用这个正则表达式得到所有的控件。 开始符号和结束符号是不一样的，所以能分割出来
            string eachValue = "";
            double valueEach = 0;

            if (matchsExeStr.Count > 0)
            {
                for (int i = 0; i < matchsExeStr.Count; i++)
                {

                    //非表格的时候
                    if (matchsExeStr[i].Value.StartsWith("%P"))
                    {
                        if (eachValue == "''") //如果找不到指定页，那么返回'',如果数据为空，则无法进行计算
                        {
                            eachValue = "";
                        }
                    }
                    else
                    {
                        //取本页数据，进行合计
                        //根据找到名字的输入框的内容来处理
                        ct = this.pictureBoxBackGround.Controls.Find(matchsExeStr[i].Value, false);

                        if (ct != null && ct.Length > 0)
                        {
                            eachValue = ct[0].Text.Trim();
                        }
                        else
                        {
                            eachValue = "";

                            return 0;
                        }
                    }

                    //根据内容，进行替换，计算
                    if (!string.IsNullOrEmpty(eachValue))
                    {
                        valueEach = StringNumber.getFirstNum(eachValue);

                        if (valueEach != -1)
                        {
                            finalExeStr = finalExeStr.Replace("[" + matchsExeStr[i].Value + "]", valueEach.ToString()); //将输入框名字，替换成输入框的值
                        }
                        else
                        {
                            //返回-1，表示字符串中没有数字，当作0来计算。在sum合计的时候，是跳过了。
                            finalExeStr = finalExeStr.Replace("[" + matchsExeStr[i].Value + "]", "0"); //将输入框名字，替换成输入框的值

                            //ShowInforMsg("项目：『" + matchsExeStr[i].Value + "』中没有数字，不能正常计算，被当作：0来计算了。", false);
                        }
                    }
                    else
                    {
                        //finalExeStr = finalExeStr.Replace("[" + matchsExeStr[i].Value + "]", "0"); //为空不处理
                        string errMsg = "计算公式：" + finalExeStr + "中，『" + matchsExeStr[i].Value + "』的内容为空，不能进行计算。";
                        //ShowInforMsg(errMsg, false);

                        return 0;
                    }
                }
            }

            ret = Evaluator.EvaluateToInteger(finalExeStr);

            return ret;
        }

        private void AttachForm_Shown(object sender, EventArgs e)
        {
            //if (_XnTemplate != null)
            //{
            //    if (!string.IsNullOrEmpty((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Size)).Trim()))
            //    {
            //        //防止附表内容过多，需要出现滚动条，否则不能显示下
            //        Size sz = Comm.getSize((_XnTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();
            //        if (sz.Width > this.Width || sz.Height > this.Height)
            //        {
            //            this.pictureBoxBackGround.Dock = DockStyle.None;

            //            this.pictureBoxBackGround.Size = new Size(this.panel1.Width - panel1.Left - panel1.Right, sz.Height);
            //        }
            //    }
            //}
        }
    }
}
