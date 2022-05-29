//软键盘，指定窗体，不获取光标
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace SparkFormEditor
{
    class HoverKeyboard : Form
    {

        public HoverKeyboard()
        {
            this.DoubleBuffered = true;
            this.SuspendLayout();

            int x = 0;
            int y = 0;
            //foreach (string line in new string[] {
            //    "1234567890", "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM "})
            //{
            //    foreach (char cur in line)
            //    {
            //        Button button = new Button();
            //        //button.TabIndex = 0;
            //        button.Location = new Point(x * 25, y * 25);
            //        button.Size = new Size(23, 23);
            //        button.Text = cur.ToString();
            //        button.Click += new EventHandler(Button_Click);
            //        Controls.Add(button);
            //        x++;
            //    }
            //    x = 0;
            //    y++;
            //}

            //ClientSize = new Size(25 * 10, 25 * 3);

            foreach (string line in new string[] {
                "1234567890", "qwertyuiop", "asdfghjkl", "zxcvbnm "})
            {
                foreach (char cur in line)
                {
                    Button button = new Button();
                    //button.TabIndex = 0;
                    button.Location = new Point(x * 45, y * 45);
                    button.Size = new Size(43, 43);
                    button.Text = cur.ToString();

                    if (button.Text == " ")
                    {
                        button.Text = "清空";
                        button.Size = new Size(125, 43);
                    }

                    button.Font = new Font("宋体", 20);
                    button.Click += new EventHandler(Button_Click);
                    Controls.Add(button);
                    x++;
                }
                x = 0;
                y++;
            }

            ////测试输入框是否有效
            //TextBox tb = new TextBox();
            //tb.Location = new Point(25, 25);
            //tb.Size = new Size(200, 60);
            //tb.Text = nameof(EntXmlModel.Text);
            //tb.BringToFront();
            ////tb.Dock = DockStyle.Fill;
            //Controls.Add(tb);

            ClientSize = new Size(45 * 9 + 43, 45 * 2 + 43 * 2);

            TopMost = true;

            //this.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.Load += new EventHandler(HoverKeyboard_Load);
            this.ResumeLayout(true);
        }

        void HoverKeyboard_Load(object sender, EventArgs e)
        {
            Application.AddMessageFilter(new PopupWindowHelperMessageFilter(this, _textboxr));
        }


        private Control _textboxr;

        public Control TextBox
        {
            get
            {
                return _textboxr;
            }
            set
            {
                _textboxr = value;
            }
        }

        void Button_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(((Button)sender).Text.Trim()) || ((Button)sender).Text == "清空")
            {
                //SendKeys.SendWait("{F5}");            //发送F5按键
                //SendKeys.SendWait("^s");              //发送 Ctrl + s 键
                //SendKeys.SendWait("%{F4}");           //发送 Alt + F4 键
                //按键 代码 
                //BACKSPACE {BACKSPACE}, {BS}, 或 {BKSP} 

                //空格，清空
                SendKeys.Send("^A");
                SendKeys.Send("{BACKSPACE}");
            }
            else
            {
                SendKeys.Send(((Button)sender).Text);
            }

            //((Button)sender).Focus();
            ((Button)sender).Select(); //选中最后一个操作的按钮
        }

        /// <summary>
        /// 指定窗体，不获取光标
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams ret = base.CreateParams;
                ret.Style = (int)Flags.WindowStyles.WS_THICKFRAME | (int)Flags.WindowStyles.WS_CHILD;
                ret.ExStyle |= (int)Flags.WindowStyles.WS_EX_NOACTIVATE | (int)Flags.WindowStyles.WS_EX_TOOLWINDOW;
                ret.X = this.Location.X;
                ret.Y = this.Location.Y;

                
                return ret;
            }
        }

    }
}
