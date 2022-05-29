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
using System.Text;
using System.Windows.Forms;
using System.Xml;
//using PopupControl;

namespace SparkFormEditor
{
    internal partial class ComplexPopupAssistantChild : UserControl
    {
        //以后可以给每个文件夹设置，输入那个表单。那么双击的输入助理自动出现该表单的即可。但是又会输入多种表单啊。实际也是很复杂的。

        public EventHandler CallBackEvent = null;    //回调 需要绑定
        private int _Flg = 0;

        /// <summary>
        /// 弹出的页码选择控件
        /// </summary>
        public ComplexPopupAssistantChild()
        {
            InitializeComponent();
            //MinimumSize = Size;
            //MaximumSize = new Size(Size.Width * 2, Size.Height * 2);//最大的大小
            MaximumSize = new Size(SystemInformation.WorkingArea.Width, SystemInformation.WorkingArea.Height); //当前的屏幕除任务栏外的工作域大小
            DoubleBuffered = true;
            ResizeRedraw = true;

            this.BackColor = Color.FromArgb(255, 58, 125, 224);
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

        private void ComplexPopupAssistantChild_Load(object sender, EventArgs e)
        {
            //LoadData();
        }

        /// <summary>
        /// 初始化内容：
        /// 0：分类，文件夹
        /// 1：首字母
        /// </summary>
        /// <param name="flg"></param>
        public void LoadData(int flg)
        {
            _Flg = flg;
            this.toolStrip1.Items.Clear();

            if (GlobalVariable.XmlDocAssistant != null)
            {
                //XmlNode root = GlobalVariable.XmlDocAssistant.FirstChild;
                //XmlNode xnApp = root.SelectSingleNode("settings[@name='" + GlobalVariable.APP_NAME + "']");

                XmlNode xnApp = GlobalVariable.XmlDocAssistant.FirstChild;

                XmlNode xnAssistant = xnApp.SelectSingleNode("strip[@key='" + "输入助理" + "']");
                //XmlNode _NodeRecent = xnAssistant.SelectSingleNode("strip[@key='" + "最近历史" + "']");
                XmlNode _NodeMain = xnAssistant.SelectSingleNode("strip[@key='" + "配置内容" + "']");

                if (flg == 0)
                {
                    loadMainItems(_NodeMain);
                }
                else if (flg == 1)
                {
                    loadMainItemsFirstWord(_NodeMain);
                }
            }
        }

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

                    if (xe.GetAttribute("type") == "0")
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

                        text = xe.GetAttribute("key");
                        newTsl.ToolTipText = text;

                        newTsl.Tag = xn;

                        //b = Encoding.Default.GetBytes(text);//半角1，全角汉字2
                        text = text.Length > ComplexPopupAssistant._ItemWordCountShow ? text.Substring(0, ComplexPopupAssistant._ItemWordCountShow) : text;
                        newTsl.Text = text;
                        //newTsl.DoubleClickEnabled = true;
                        newTsl.Click += new System.EventHandler(this.tsi_Click);

                        this.toolStrip1.Items.Add(newTsl);
                    }
                }
            }
        }

        /// <summary>
        /// 全部所有项目，或者某个节点下的所有项目
        /// </summary>
        /// <param name="xnPara"></param>
        /// <param name="tsmiPara"></param>
        private void loadMainItemsFirstWord(XmlNode xnPara)
        {
            //得到所有type为1的名字：
            _ListWords.Clear();
            GetAllWord(xnPara);

            List<string> listItems = new List<string>();

            for (int i = 0; i < _ListWords.Count; i++)
            {
                if (!string.IsNullOrEmpty(_ListWords[i]))
                {
                    _ListWords[i] = GetStringSpell.GetUpperChineseSpell(_ListWords[i].Substring(0, 1));

                    if (!listItems.Contains(_ListWords[i]))
                    {
                        listItems.Add(_ListWords[i]);
                    }
                }
            }

            listItems.Sort();

            ToolStripMenuItem newTsl;
            string text = "";

            for (int i = 0; i < listItems.Count; i++)
            {
                newTsl = new ToolStripMenuItem();
                newTsl.AutoSize = false;
                //newTsl.BackColor = Color.White;
                newTsl.Paint += new PaintEventHandler(newTsl_Paint);
                newTsl.Size = new System.Drawing.Size(22, 23);
                newTsl.Font = this.Font;
                newTsl.Margin = new Padding(5, 0, 5, 0);
                newTsl.Padding = new Padding(5, 0, 5, 0);
                newTsl.TextAlign = ContentAlignment.MiddleLeft;

                text = listItems[i];
                newTsl.ToolTipText = text;

                //b = Encoding.Default.GetBytes(text);//半角1，全角汉字2
                text = text.Length > ComplexPopupAssistant._ItemWordCountShow ? text.Substring(0, ComplexPopupAssistant._ItemWordCountShow) : text;
                newTsl.Text = text;
                //newTsl.DoubleClickEnabled = true;
                newTsl.Click += new System.EventHandler(this.tsi_Click);

                this.toolStrip1.Items.Add(newTsl);
            }
        }

        /// <summary>
        /// 获取所有节点名字
        /// </summary>
        /// <param name="xnPara"></param>
        List<string> _ListWords = new List<string>();
        private void GetAllWord(XmlNode xnPara)
        {
            XmlElement xe = null;

            if (xnPara.HasChildNodes)
            {
                foreach (XmlNode xn in xnPara.ChildNodes)
                {
                    xe = (XmlElement)xn;

                    if (xe.GetAttribute("type") == "1")
                    {
                        _ListWords.Add(xe.GetAttribute("key"));
                    }
                    else
                    {
                        //_ListWords.Add(xe.GetAttribute("key"));

                        GetAllWord(xn);
                    }
                }
            }
        }

        private void tsi_Click(object sender, EventArgs e)
        {

            if (CallBackEvent != null)
            {
                ToolStripMenuItem aa = new ToolStripMenuItem();
                aa.Tag = ((ToolStripMenuItem)sender).Tag;
                aa.Text = _Flg.ToString();
                aa.ToolTipText = ((ToolStripMenuItem)sender).ToolTipText;

                CallBackEvent(aa, null);
            }

            (Parent as Popup).Close(); //关闭
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
            e.Graphics.DrawLine(Pens.LightGray, new Point(1, tsmi.Height - 2), new Point(tsmi.Width / 2, tsmi.Height - 2)); //Pens.LightGray
        }

    }
}
