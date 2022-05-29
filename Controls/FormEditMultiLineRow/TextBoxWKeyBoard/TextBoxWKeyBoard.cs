using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace SparkFormEditor
{
    internal class TexBoxWKeyBoard : TextBox
    {
        public TexBoxWKeyBoard()
        {
            this.Click += new EventHandler(TexBoxWKeyBoard_Click);
            this.Enter += new EventHandler(TexBoxWKeyBoard_Enter);
            this.Leave += new EventHandler(TexBoxWKeyBoard_Leave);
            //this.KeyDown += new KeyEventHandler(SparkFormEditor_KeyDown);
            //this.KeyPress += new KeyPressEventHandler(SparkFormEditor_KeyPress);
        }

        Form HK;
        void TexBoxWKeyBoard_Leave(object sender, EventArgs e)
        {
            if (HK != null)
            {
                HK.Close();
                HK = null;
            }
        }

        void HK_FormClosed(object sender, FormClosedEventArgs e)
        {
            HK.Dispose();
            HK = null;
        }

        void TexBoxWKeyBoard_Enter(object sender, EventArgs e)
        {
            if (HK == null)
            {
                if (TextEntryMode == EntryMode.Standard)
                {
                    HK = new HoverKeyboard();
                    ((HoverKeyboard)HK).TextBox = this;
                }
                else
                {
                    HK = new HoverNumPad();
                    ((HoverNumPad)HK).TextBox = this;
                }
                HK.Location = this.Parent.PointToScreen(new Point(this.Left, this.Bottom));
                HK.FormClosed += new FormClosedEventHandler(HK_FormClosed);
                HK.Show();
            }

        }

        void TexBoxWKeyBoard_Click(object sender, EventArgs e)
        {
            TexBoxWKeyBoard_Enter(sender, e);
        }

        public enum EntryMode
        {
            Standard,
            Numeric
        }


        private EntryMode _TextEntryMode = EntryMode.Standard;

        public EntryMode TextEntryMode
        {
            get
            {
                return _TextEntryMode;
            }
            set
            {
                _TextEntryMode = value;
            }
        }

        #region numericMode
        //This is secion is based on Capturing numeric input in a TextBox by ddanbe on DaniWeb
        //http://www.daniweb.com/code/snippet217265.html
        //Its simple enough, but danny did a great job so I thought I would give him some credit.

        bool NumberEntered = false;

        ////Check if key entered is "numeric".
        //private bool CheckIfNumericKey(Keys K, bool isDecimalPoint)
        //{
        //    if (K == Keys.Back) //backspace?
        //        return true;
        //    else if (K == Keys.OemPeriod || K == Keys.Decimal)  //decimal point?
        //        return isDecimalPoint ? false : true;       //or: return !isDecimalPoint
        //    else if ((K >= Keys.D0) && (K <= Keys.D9))      //digit from top of keyboard?
        //        return true;
        //    else if ((K >= Keys.NumPad0) && (K <= Keys.NumPad9))    //digit from keypad?
        //        return true;
        //    else
        //        return false;   //no "numeric" key
        //}

        //private void SparkFormEditor_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (TextEntryMode == EntryMode.Numeric)
        //    {
        //        //Get our textbox.
        //        TextBox Tbx = (TextBox)sender;
        //        // Initialize the flag.
        //        NumberEntered = CheckIfNumericKey(e.KeyCode, Tbx.Text.Contains("."));
        //    }
        //}

        //// This event occurs after the KeyDown event and can be used to prevent
        //// characters from entering the control.
        //private void SparkFormEditor_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (TextEntryMode == EntryMode.Numeric)
        //    {
        //        // Check for the flag being set in the KeyDown event.
        //        if (NumberEntered == false)
        //        {
        //            // Stop the character from being entered into the control since it is non-numerical.
        //            e.Handled = true;
        //        }
        //    }

        //}
        #endregion
    }
}
