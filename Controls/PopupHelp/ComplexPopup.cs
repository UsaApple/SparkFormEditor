            //complex = new Popup(complexPopup = new ComplexPopup());
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
//using PopupControl;

namespace SparkFormEditor
{
    internal partial class ComplexPopup : UserControl
    {

        public EventHandler CallBackPageSelected = null;    //回调跳转页、打开指定页时间，需要绑定
        public int _PageCount = 0;

        /// <summary>
        /// 弹出的页码选择控件
        /// </summary>
        public ComplexPopup()
        {
            InitializeComponent();
            MinimumSize = Size;
            //MaximumSize = new Size(Size.Width * 2, Size.Height * 2);//最大的大小
            MaximumSize = new Size(SystemInformation.WorkingArea.Width, SystemInformation.WorkingArea.Height); //当前的屏幕除任务栏外的工作域大小
            DoubleBuffered = true;
            ResizeRedraw = true;
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

        private void ComplexPopup_Load(object sender, EventArgs e)
        {
            //和窗体一样，只有第一次显示会执行。以后隐藏后再显示，不会执行本方法

            //if (this.panel1.VerticalScroll.Maximum >= this.toolStrip1.Height)
            //{
            //    this.panel1.VerticalScroll.Value = this.toolStrip1.Height;
            //}
            //else
            //{
            //    this.panel1.VerticalScroll.Value = this.VerticalScroll.Maximum;
            //}
        }

        /// <summary>
        /// 加载具体的页数项目
        /// </summary>
        /// <param name="pageCount"></param>
        public void LoadPageInfor(int pageCount)
        {
            if (_PageCount != pageCount)
            {
                _PageCount = pageCount;
                this.toolStrip1.Items.Clear();

                //初始化和页数改省改变的时候，都要调用该方法
                //加载临时数据：100页
                ToolStripMenuItem tsi;
                for (int i = 1; i <= pageCount; i++)
                {
                    tsi = new ToolStripMenuItem();
                    tsi.Text = i.ToString();
                    tsi.AutoSize = false; //设置大小，保证每一行的个数一样
                    tsi.Size = new Size(28, 22);
                    tsi.BackColor = Color.White;
                    tsi.Click += new EventHandler(tsi_Click);

                    this.toolStrip1.Items.Add(tsi);

                    
                }

                //this.panel1.Refresh();
                //SetBottomScrolll();
            }

            //滚动条设置到最底下(可以看到最后一页)     
            //this.menuStrip1.Items[this.menuStrip1.Items.Count - 1].Select();    //不会影响滚动条

            //this.panel1.VerticalScroll.Maximum 一直是100啊
            //if (this.panel1.VerticalScroll.Maximum >= this.toolStrip1.Height)
            //{
            //    this.panel1.VerticalScroll.Value = this.toolStrip1.Height;
            //}
            //else
            //{
            //    this.panel1.VerticalScroll.Value = this.VerticalScroll.Maximum;
            //}


        }

        public void SetBottomScrolll()
        {
            if (this.panel1.VerticalScroll.Maximum >= this.toolStrip1.Height)
            {
                this.panel1.VerticalScroll.Value = this.toolStrip1.Height;
            }
            else
            {
                this.panel1.VerticalScroll.Value = this.panel1.VerticalScroll.Maximum;
            }
        }


        private void tsi_Click(object sender, EventArgs e)
        {
            if (CallBackPageSelected != null)
            {
                CallBackPageSelected(sender, null);
            }
        }

        private void ComplexPopup_VisibleChanged(object sender, EventArgs e)
        {

        }

        private void panel1_VisibleChanged(object sender, EventArgs e)
        {
            //SetBottomScrolll();
        }

        private void toolStrip1_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            //SetBottomScrolll();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            //if (_PageCount != pageCount)
            //{
            //    SetBottomScrolll();
            //}
        }
    }

}
