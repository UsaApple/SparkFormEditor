using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SparkFormEditor
{
    internal class SelectingEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
        public bool Cancel { get; set; }
        public int SelectedIndex { get; set; }
        public bool Handled { get; set; }
    }

    internal class SelectedEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
        public Control Control { get; set; }
    }

    internal class HoveredEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
    }


    internal class PaintItemEventArgs : PaintEventArgs
    {
        public RectangleF TextRect { get; internal set; }
        public StringFormat StringFormat { get; internal set; }
        public Font Font { get; internal set; }
        public bool IsSelected { get; internal set; }
        public bool IsHovered { get; internal set; }

        public PaintItemEventArgs(Graphics graphics, Rectangle clipRect):base(graphics, clipRect)
        {
        }
    }

    internal class WrapperNeededEventArgs : EventArgs
    {
        public Control TargetControl { get; private set; }
        public ITextBoxWrapper Wrapper { get; set; }

        public WrapperNeededEventArgs(Control targetControl)
        {
            this.TargetControl = targetControl;
        }
    }
}
