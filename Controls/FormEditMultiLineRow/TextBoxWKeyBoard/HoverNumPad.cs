using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace SparkFormEditor
{
    class HoverNumPad : Form
    {

        public HoverNumPad()
        {
            this.Load += new EventHandler(HoverKeyboard_Load);

            this.DoubleBuffered = true;
            this.SuspendLayout();
            int x = 0;
            int y = 0;
            foreach (string line in new string[] {
                //"789", "456", "123", "0."})
                "123", "456", "789", "0."})
            {
                foreach (char cur in line)
                {
                    Button button = new Button();
                    button.Location = new Point(x * 25, y * 25);
                    button.Size = new Size(23, 23);
                    button.Text = cur.ToString();
                    button.Click += new EventHandler(Button_Click);
                    Controls.Add(button);
                    x++;
                }
                x = 0;
                y++;
            }

            this.ClientSize = new Size(25 * 3, 25 * 4);
            TopMost = true;

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
            SendKeys.Send(((Button)sender).Text);

            ((Button)sender).Select(); //选中最后一个操作的按钮
        }

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
