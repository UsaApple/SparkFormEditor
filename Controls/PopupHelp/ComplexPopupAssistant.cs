            //complex = new Popup(complexPopup = new ComplexPopup());  //(Popup) complex
            //complex.Resizable = true;

            //complexPopup.ButtonMore.Click += (_sender, _e) =>
            //{
            //    ComplexPopup cp = new ComplexPopup();
            //    cp.ButtonMore.Click +=
            //        (__sender, __e) => new Popup(new ComplexPopup()).Show(__sender as Button);
            //    new Popup(cp).Show(_sender as Button);
            //};

//complex.Show(sender as Button);

//complex.Close();

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SparkFormEditor
{
    internal partial class ComplexPopupAssistant : UserControl
    {
        //以后可以给每个文件夹设置，输入那个表单。那么双击的输入助理自动出现该表单的即可。但是又会输入多种表单啊。实际也是很复杂的。

        public EventHandler CallBackPageSelected = null;    //回调 需要绑定

        //和输入助理类似，但是又不一样。重新定义，便于以后个性化
        public static string _SpecialWord = "℃¤℉¤‰¤mg¤ml¤mmHg¤mmol/L¤次/分¤Ⅰ¤Ⅱ¤Ⅲ¤Ⅳ¤Ⅴ¤①¤②¤③¤④¤⑤"; //默认的特殊字符
        public static int _RecentUsedCountShow = 20;   //最近历史中的保存和加载显示的个数
        public static int _ItemWordCountShow = 6;     //toolStrip3 固定显示的文字数，完整的用tooltip显示

        private bool IsRecent = false;                   //只有目前显示的是最近历史的时候，双击后，需要将其显示为历史。都不显示也可以，切换在更新。

        /// <summary>
        /// 弹出的页码选择控件
        /// </summary>
        public ComplexPopupAssistant()
        {
            InitializeComponent();
            MinimumSize = Size;
            //MaximumSize = new Size(Size.Width * 2, Size.Height * 2);//最大的大小
            MaximumSize = new Size(SystemInformation.WorkingArea.Width, SystemInformation.WorkingArea.Height); //当前的屏幕除任务栏外的工作域大小
            DoubleBuffered = true;
            ResizeRedraw = true;


            this.menuStrip1.BackColor = Color.FromArgb(255, 58, 125, 224);
        }

        //private const int WM_GETTEXT = 0x000d;
        //private const int WM_COPY = 0x0301;
        //private const int WM_PASTE = 0x0302;
        //private const int WM_CONTEXTMENU = 0x007B;
        //private const int WM_RBUTTONDOWN = 0x0204;

        protected override void WndProc(ref Message m)
        {
            //if (m.Msg == WM_RBUTTONDOWN)
            //{
            //    //屏蔽TextBox默认右键菜单
            //    return;//WM_RBUTTONDOWN是为了不让出现鼠标菜单
            //}

            if ((Parent as Popup).ProcessResizing(ref m))
            {
                //this.Refresh();
                return;
            }
            base.WndProc(ref m);
        }

        ///// <summary>
        ///// 让焦点还在原来的窗口控件上，提高用户体验，操作性
        ///// </summary>
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        const int WS_EX_NOACTIVATE = 0x08000000;
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= WS_EX_NOACTIVATE;
        //        return cp;
        //    }
        //}

        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        //cp.Style = (int)0x40000000; //主控件焦点 (int)0x00040000 | (int)0x40000000

        //        cp.ExStyle |= NativeMethods.WS_EX_NOACTIVATE;

        //        //if (NonInteractive)
        //        //{
        //        //    //如果开启，那么不能上面的cp.Style = (int)0x40000000; 否则异常，创建句柄错误
        //        //    cp.ExStyle |= NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TOOLWINDOW;
        //        //}
        //        return cp;

        //        ////NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW |
        //        //////让焦点还在原来的窗口控件上，提高用户体验，操作性
        //    }
        //}

        private void ComplexPopupAssistant_Load(object sender, EventArgs e)
        {
            //LoadData();
        }

        public void LoadData()
        {
            //显示默认的：最近历史，如果为空，那么使用全部。

            if (GlobalVariable.XmlDocAssistant != null)
            {
                XmlNode xnApp = GlobalVariable.XmlDocAssistant.DocumentElement;
                //XmlNode xnApp = root.SelectSingleNode("settings[@name='" + GlobalVariable.APP_NAME + "']");
                XmlNode xnAssistant = xnApp.SelectSingleNode("strip[@key='" + "输入助理" + "']");
                XmlNode _NodeRecent = xnAssistant.SelectSingleNode("strip[@key='" + "最近历史" + "']");


                string strRecent = (_NodeRecent as XmlElement).GetAttribute("value");

                if (!string.IsNullOrEmpty(strRecent))
                {
                    //有最近历史的时候，保存历史。
                    LoadRecent();
                }
                else
                {
                    //否则显示所有的输入助理内容
                    LoadMain();
                }
            }

            //InitTypesMenu();

            //if (GlobalVariable.XmlDocAssistant != null)
            //{
            //    XmlNode root = GlobalVariable.XmlDocAssistant.FirstChild;
            //    XmlNode xnApp = root.SelectSingleNode("settings[@name='" + GlobalVariable.APP_NAME + "']");
            //    XmlNode xnAssistant = xnApp.SelectSingleNode("strip[@key='" + "输入助理" + "']");
            //    XmlNode xnSpecialWord = xnAssistant.SelectSingleNode("strip[@key='" + "特殊字符" + "']");
            //    string temp = (xnSpecialWord as XmlElement).GetAttribute("value");

            //    ToolStripMenuItem tsmi = null;
            //    string[] arr = temp.Split('¤');

            //    foreach (string str in arr)
            //    {
            //        tsmi = new ToolStripMenuItem(str);
            //        tsmi.Click += new EventHandler(tsi_Click);
            //        tsmi.ToolTipText = str;
            //        toolStripMenuItem特殊字符.DropDownItems.Add(tsmi);
            //    }

            //}


            ////滚动条设置到最底下(可以看到最后一页)     
            //if (this.VerticalScroll.Maximum >= this.menuStrip1.Height)
            //{
            //    this.VerticalScroll.Value = this.menuStrip1.Height;
            //}

            
        }

        /// <summary>
        /// 初始化，分类子菜单
        /// </summary>
        public void InitTypesMenu()
        {
            //if (xnPara.HasChildNodes)
            //{
            //    XmlElement xe = null;
            //    TreeNode tn = null;
            //    string text = "";
            //    foreach (XmlNode xn in xnPara.ChildNodes)
            //    {
            //        xe = (XmlElement)xn;
            //        tn = new TreeNode();

            //        //截取前20个文字：
            //        text = xe.GetAttribute("key");
            //        text = text.Length > _ItemWordCountShow * 2 ? text.Substring(0, _ItemWordCountShow * 2) : text;
            //        tn.Text = text;     // xe.GetAttribute("key");
            //        tn.ToolTipText = xe.GetAttribute("key");

            //        tn.Tag = xn;

            //        if (xe.GetAttribute("type") == "1")
            //        {
            //            tn.ImageIndex = 1;
            //        }
            //        else
            //        {
            //            //目录：设置图标后，还需再遍历
            //            tn.ImageIndex = 0;

            //            loadMain(xn, tn);
            //        }

            //        tn.SelectedImageIndex = tn.ImageIndex;

            //        if (tnPara == null)
            //        {
            //            this.treeViewVista1.Nodes.Add(tn);
            //        }
            //        else
            //        {
            //            tnPara.Nodes.Add(tn);
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 切换为显示特殊字符
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem特殊字符_Click(object sender, EventArgs e)
        {
            this.toolStrip3.Items.Clear();

            if (GlobalVariable.XmlDocAssistant != null)
            {
                XmlNode xnApp = GlobalVariable.XmlDocAssistant.DocumentElement;
                //XmlNode xnApp = root.SelectSingleNode("settings[@name='" + GlobalVariable.APP_NAME + "']");
                XmlNode xnAssistant = xnApp.SelectSingleNode("strip[@key='" + "输入助理" + "']");
                XmlNode xnSpecialWord = xnAssistant.SelectSingleNode("strip[@key='" + "特殊字符" + "']");
                string temp = (xnSpecialWord as XmlElement).GetAttribute("value");

                ToolStripMenuItem newTsl = null;
                string text = "";

                string[] arr = temp.Split('¤');

                foreach (string str in arr)
                {
                    //tsmi = new ToolStripMenuItem(str);
                    //tsmi.Click += new EventHandler(tsi_Click);
                    //tsmi.ToolTipText = str;
                    //toolStripMenuItem特殊字符.DropDownItems.Add(tsmi);

                    newTsl = new ToolStripMenuItem();
                    newTsl.AutoSize = false;
                    //newTsl.BackColor = Color.White;
                    newTsl.Paint += new PaintEventHandler(newTsl_Paint);
                    newTsl.Size = new System.Drawing.Size((this.Width - 24) / 3 - 12, 23);
                    newTsl.Font = new Font("宋体", 9);
                    newTsl.Margin = new Padding(5, 0, 5, 0);
                    newTsl.Padding = new Padding(5, 0, 5, 0);
                    newTsl.TextAlign = ContentAlignment.MiddleLeft;

                    //截取前20个文字：
                    text = str;
                    text = text.Length > _ItemWordCountShow * 2 ? text.Substring(0, _ItemWordCountShow * 2) : text;

                    newTsl.Text = text;
                    newTsl.ToolTipText = str;
                    newTsl.DoubleClickEnabled = true;
                    newTsl.DoubleClick += new System.EventHandler(this.tsi_Click);

                    this.toolStrip3.Items.Add(newTsl);

                }
            }
        }

        private void toolStripMenuItem筛选_Click(object sender, EventArgs e)
        {
            //InitFilter() 编辑器(_sender, _e) 调用出，调用执行，显示子菜单内容
        }

        private void toolStripMenuItem分类_Click(object sender, EventArgs e)
        {
            //InitTypesMenu();    //编辑器(_sender, _e) 调用出，调用执行，显示子菜单内容
        }

        private void toolStripMenuItem全部_Click(object sender, EventArgs e)
        {
            LoadMain();
        }

        private void toolStripMenuItem最近_Click(object sender, EventArgs e)
        {
            LoadRecent();
        }

        /// <summary>
        /// 回调方法，将文字写入到编辑器中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsi_Click(object sender, EventArgs e)
        {
            if (CallBackPageSelected != null)
            {
                //ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;

                SetRecent(((ToolStripMenuItem)sender).ToolTipText);

                CallBackPageSelected(sender, null);
            }
        }

        /// <summary>
        /// 切换为显示所有的输入助理项目
        /// </summary>
        private void LoadMain()
        {
            this.toolStrip3.Items.Clear();

            if (GlobalVariable.XmlDocAssistant != null)
            {
                XmlNode xnApp = GlobalVariable.XmlDocAssistant.DocumentElement;
                //XmlNode xnApp = root.SelectSingleNode("settings[@name='" + GlobalVariable.APP_NAME + "']");
                XmlNode xnAssistant = xnApp.SelectSingleNode("strip[@key='" + "输入助理" + "']");
                XmlNode _NodeMain = xnAssistant.SelectSingleNode("strip[@key='" + "配置内容" + "']");

                loadMainItems(_NodeMain);
            }
        }

        string _CompareFirstWord = "";
        /// <summary>
        /// 全部所有项目，或者某个节点下的所有项目
        /// </summary>
        /// <param name="xnPara"></param>
        /// <param name="tsmiPara"></param>
        private void loadMainItems(XmlNode xnPara)
        {
            ToolStripMenuItem newTsl;
            XmlElement xe = null;
            string text = "";
            //byte[] b;   //半角1，全角汉字2

            if (xnPara.HasChildNodes)
            {
                foreach (XmlNode xn in xnPara.ChildNodes)
                {
                    xe = (XmlElement)xn;

                    if (xe.GetAttribute("type") == "1")
                    {
                        text = xe.GetAttribute("key");

                        if (string.IsNullOrEmpty(_CompareFirstWord) || (!string.IsNullOrEmpty(text) && GetStringSpell.GetUpperChineseSpell(text.Substring(0, 1)) == _CompareFirstWord))
                        {

                            newTsl = new ToolStripMenuItem();
                            newTsl.AutoSize = false;
                            //newTsl.BackColor = Color.White;
                            newTsl.Paint += new PaintEventHandler(newTsl_Paint);
                            newTsl.Size = new System.Drawing.Size((this.Width - 24) / 3 - 12, 23);
                            newTsl.Font = this.Font;
                            newTsl.Margin = new Padding(5, 0, 5, 0);
                            newTsl.Padding = new Padding(5, 0, 5, 0);
                            newTsl.TextAlign = ContentAlignment.MiddleLeft;


                            newTsl.ToolTipText = text;

                            //b = Encoding.Default.GetBytes(text);//半角1，全角汉字2
                            text = text.Length > _ItemWordCountShow ? text.Substring(0, _ItemWordCountShow) : text;
                            newTsl.Text = text;
                            newTsl.DoubleClickEnabled = true;
                            newTsl.DoubleClick += new System.EventHandler(this.tsi_Click);

                            this.toolStrip3.Items.Add(newTsl);
                        }
                    }
                    else
                    {
                        loadMainItems(xn);
                    }
                }
            }
            else
            {
                //这里其实不会被执行到的，因为要么全部，要么是某个第一层文件夹的项目
                //if ((xnPara as XmlElement).GetAttribute("type") == "1")
                //{
                //    newTsl = new ToolStripMenuItem();
                //    newTsl.AutoSize = false;
                //    //newTsl.BackColor = Color.White;
                //    newTsl.Paint += new PaintEventHandler(newTsl_Paint);
                //    newTsl.Size = new System.Drawing.Size((this.Width - 24) / 3 - 12, 23);
                //    newTsl.Font = this.Font;
                //    newTsl.Margin = new Padding(5, 0, 5, 0);
                //    newTsl.Padding = new Padding(5, 0, 5, 0);
                //    newTsl.TextAlign = ContentAlignment.MiddleLeft;

                //    text = xe.GetAttribute("key");
                //    newTsl.ToolTipText = text;

                //    //b = Encoding.Default.GetBytes(text);//半角1，全角汉字2
                //    text = text.Length > _ItemWordCountShow ? text.Substring(0, _ItemWordCountShow) : text;
                //    newTsl.Text = text;
                //    newTsl.DoubleClickEnabled = true;
                //    newTsl.DoubleClick += new System.EventHandler(this.tsi_Click);

                //    this.toolStrip3.Items.Add(newTsl);
                //}
            }
        }

        /// <summary>
        /// 加载最近历史历史，方便常用的加快操作速度
        /// </summary>
        private void LoadRecent()
        {
            this.toolStrip3.Items.Clear();

            if (GlobalVariable.XmlDocAssistant != null)
            {
                XmlNode xnApp = GlobalVariable.XmlDocAssistant.DocumentElement;
                //XmlNode xnApp = root.SelectSingleNode("settings[@name='" + GlobalVariable.APP_NAME + "']");
                XmlNode xnAssistant = xnApp.SelectSingleNode("strip[@key='" + "输入助理" + "']");
                XmlNode _NodeRecent = xnAssistant.SelectSingleNode("strip[@key='" + "最近历史" + "']");

                string strRecent = (_NodeRecent as XmlElement).GetAttribute("value");

                if (!string.IsNullOrEmpty(strRecent))
                {
                    string[] arr = strRecent.Split('¤');

                    ToolStripMenuItem newTsl = null;
                    string text = "";

                    //for (int i = 0; i < arr.Length; i++)
                    for (int i = arr.Length - 1; i >= 0; i--)
                    {
                        newTsl = new ToolStripMenuItem();
                        newTsl.AutoSize = false;
                        //newTsl.BackColor = Color.White;
                        newTsl.Paint += new PaintEventHandler(newTsl_Paint);
                        newTsl.Size = new System.Drawing.Size((this.Width - 24) / 3 - 12, 23);
                        newTsl.Font = new Font("宋体", 9);
                        newTsl.Margin = new Padding(5, 0, 5, 0);
                        newTsl.Padding = new Padding(5, 0, 5, 0);
                        newTsl.TextAlign = ContentAlignment.MiddleLeft;

                        //截取前20个文字：
                        text = arr[i];
                        text = text.Length > _ItemWordCountShow * 2 ? text.Substring(0, _ItemWordCountShow * 2) : text;

                        newTsl.Text = text;
                        newTsl.ToolTipText = arr[i];
                        newTsl.DoubleClickEnabled = true;
                        newTsl.DoubleClick += new System.EventHandler(this.tsi_Click);

                        this.toolStrip3.Items.Add(newTsl);
                    }
                }
            }
        }

        /// <summary>
        /// 分割清晰
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newTsl_Paint(object sender, PaintEventArgs e)
        {
            ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;
            e.Graphics.DrawLine(Pens.LightGray, new Point(1, tsmi.Height - 10), new Point(1, tsmi.Height - 2));
            e.Graphics.DrawLine(Pens.LightGray, new Point(1, tsmi.Height - 2), new Point(tsmi.Width / 2, tsmi.Height - 2));
        }

        private void ComplexPopupAssistant_Scroll(object sender, ScrollEventArgs e)
        {
            //this.Refresh();
        }

        /// <summary>
        /// 保存、更新：最近历史
        /// </summary>
        private void SetRecent(string thisItemText)
        {
            //"¤"分割每个
            XmlNode xnApp = GlobalVariable.XmlDocAssistant.DocumentElement;
            //XmlNode xnApp = root.SelectSingleNode("settings[@name='" + GlobalVariable.APP_NAME + "']");
            XmlNode xnAssistant = xnApp.SelectSingleNode("strip[@key='" + "输入助理" + "']");
            XmlNode _NodeRecent = xnAssistant.SelectSingleNode("strip[@key='" + "最近历史" + "']");
            string currentValue = (_NodeRecent as XmlElement).GetAttribute("value");
            string[] arr = new string[0];
            string newValue = "";

            if (!string.IsNullOrEmpty(currentValue))
            {
                arr = currentValue.Split('¤');
            }

            //可能已经存在，那么移动最前(和关注病人一样：最新的放在最后的)
            List<string> list = new List<string>();
            list = arr.ToList();
            if (list.Contains(thisItemText))
            {
                list.Remove(thisItemText);
                list.Add(thisItemText);
            }
            else
            {
                list.Add(thisItemText);
            }

            //防止使用一段时间后，内容太多，也都是无效的内容。所以最多10个，理论上够用了。
            if (list.Count > _RecentUsedCountShow)
            {
                //将第一个移除，后面的都向前已一位。
                //List<string> list = new List<string>();
                //list = arr.ToList();
                list.RemoveAt(0);
            }

            arr = list.ToArray();

            newValue = string.Join("¤", arr); //如果数组长度为0，那么字符串为空""
            (_NodeRecent as XmlElement).SetAttribute("value", newValue);

            if (IsRecent)
            {
                //2.最近历史:下拉菜单方式加载最近几个（_RecentUsedCountShow）保存时控制限制个数即可。
                //_NodeRecent = _NodeInputAssistant.SelectSingleNode("strip[@key='" + "最近历史" + "']");
                //只有点击的是最近历史才要更新菜单，否则会一闪。
                LoadRecent();
            }
        }

        /// <summary>
        /// 根据选择的文件夹 类型，或者首字母，来初始化显示对应的输入助理项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PopChildCallBack(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                //string[] arr = (string[])sender;
                ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;

                if (tsmi.Text == "0")
                {
                    this.toolStrip3.Items.Clear();
                    loadMainItems((XmlNode)tsmi.Tag);
                }
                else
                {
                    this.toolStrip3.Items.Clear();

                    //首字母是置顶字母的的标签

                    XmlNode xnApp = GlobalVariable.XmlDocAssistant.DocumentElement;
                    //XmlNode xnApp = root.SelectSingleNode("settings[@name='" + GlobalVariable.APP_NAME + "']");
                    XmlNode xnAssistant = xnApp.SelectSingleNode("strip[@key='" + "输入助理" + "']");
                    XmlNode _NodeMain = xnAssistant.SelectSingleNode("strip[@key='" + "配置内容" + "']");

                    _CompareFirstWord = tsmi.ToolTipText;

                    loadMainItems(_NodeMain);

                    _CompareFirstWord = "";
                }
            }
        }
    }
}
