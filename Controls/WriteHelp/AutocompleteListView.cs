using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SparkFormEditor
{
    /// <summary>
    /// 智能输入提示控件
    /// </summary>
    internal class AutocompleteListView : UserControl, IAutocompleteListView
    {
        private readonly ToolTip toolTip = new ToolTip();
        private int hoveredItemIndex = -1;
        private int oldItemCount;
        private int selectedItemIndex = -1;
        private IList<AutocompleteItem> visibleItems;

        /// <summary>
        /// Duration (ms) of tooltip showing
        /// </summary>
        public int ToolTipDuration { get; set; }

        /// <summary>
        /// Occurs when user selected item for inserting into text
        /// </summary>
        public event EventHandler ItemSelected;


        /// <summary>
        /// Occurs when current hovered item is changing
        /// </summary>
        public event EventHandler<HoveredEventArgs> ItemHovered;

        internal AutocompleteListView()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            base.Font = new Font("宋体", 9);
            ItemHeight = Font.Height + 2;
            VerticalScroll.SmallChange = ItemHeight;
            BackColor = Color.White; //背景颜色
            LeftPadding = 18;
            ToolTipDuration = 3000;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                toolTip.Dispose();
            }
            base.Dispose(disposing);
        }


        private int itemHeight;

        public int ItemHeight
        {
            get { return itemHeight; }
            set { 
                itemHeight = value;
                VerticalScroll.SmallChange = value;
                oldItemCount = -1;
                AdjustScroll();
            }
        }

        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                ItemHeight = Font.Height + 2;
            }
        }

        public int LeftPadding { get; set; }

        public ImageList ImageList { get; set; }

        public IList<AutocompleteItem> VisibleItems
        {
            get { return visibleItems; }
            set
            {
                visibleItems = value;
                SelectedItemIndex = -1;
                AdjustScroll();
                Invalidate();
            }
        }

        public int SelectedItemIndex
        {
            get { return selectedItemIndex; }
            set
            {
                AutocompleteItem item = null;
                if (value >= 0 && value < VisibleItems.Count)
                    item = VisibleItems[value];

                selectedItemIndex = value;

                OnItemHovered(new HoveredEventArgs() { Item = item });

                if (item != null)
                {
                    SetToolTip(item);
                    ScrollToSelected();
                }

                Invalidate();
            }
        }

        private void OnItemHovered(HoveredEventArgs e)
        {
            if (ItemHovered != null)
                ItemHovered(this, e);
        }

        private void AdjustScroll()
        {
            if (VisibleItems == null)
                return;

            //要提前到这里的。否则，如果两个输入框的只能匹配到的一样多，那么就高度一样了。就有余白了
            int needHeight = ItemHeight * VisibleItems.Count + 1;
            Height = Math.Min(needHeight, MaximumSize.Height);//防止设置MaximumSize后，有时会有空白，高度过大

            if (oldItemCount == VisibleItems.Count)
                return;

            //int needHeight = ItemHeight*VisibleItems.Count + 1;
            //Height = Math.Min(needHeight, MaximumSize.Height);//防止设置MaximumSize后，有时会有空白，高度过大

            //this.Size = new Size(this.Width, Height);//防止设置MaximumSize后，有时会有空白，高度过大

            AutoScrollMinSize = new Size(0, needHeight);
            oldItemCount = VisibleItems.Count;
        }


        private void ScrollToSelected()
        {
            int y = SelectedItemIndex*ItemHeight - VerticalScroll.Value;
            if (y < 0)
                VerticalScroll.Value = SelectedItemIndex*ItemHeight;
            if (y > ClientSize.Height - ItemHeight)
                VerticalScroll.Value = Math.Min(VerticalScroll.Maximum,
                                                SelectedItemIndex*ItemHeight - ClientSize.Height + ItemHeight);
            //some magic for update scrolls
            AutoScrollMinSize -= new Size(1, 0);
            AutoScrollMinSize += new Size(1, 0);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            bool rtl = RightToLeft == RightToLeft.Yes;
            AdjustScroll();
            int startI = VerticalScroll.Value/ItemHeight - 1;
            int finishI = (VerticalScroll.Value + ClientSize.Height)/ItemHeight + 1;
            startI = Math.Max(startI, 0);
            finishI = Math.Min(finishI, VisibleItems.Count);
            int y = 0;

            for (int i = startI; i < finishI; i++)
            {
                y = i*ItemHeight - VerticalScroll.Value;

                if (ImageList != null && VisibleItems[i].ImageIndex >= 0)
                    if (rtl)
                        e.Graphics.DrawImage(ImageList.Images[VisibleItems[i].ImageIndex], Width - 1 - LeftPadding, y);
                    else
                        e.Graphics.DrawImage(ImageList.Images[VisibleItems[i].ImageIndex], 1, y);

                var textRect = new Rectangle(LeftPadding, y, ClientSize.Width - 1 - LeftPadding, ItemHeight - 1);
                if (rtl)
                    textRect = new Rectangle(1, y, ClientSize.Width - 1 - LeftPadding, ItemHeight - 1);

                if (i == SelectedItemIndex)
                {
                    //Brush selectedBrush = new LinearGradientBrush(new Point(0, y - 3), new Point(0, y + ItemHeight),
                    //                                              Color.White, Color.Orange);

                    Brush selectedBrush = new LinearGradientBrush(new Point(0, y - 3), new Point(0, y + ItemHeight),
                                              Color.White, Color.LightSkyBlue); //智能提示选中项的背景颜色

                    e.Graphics.FillRectangle(selectedBrush, textRect);
                    e.Graphics.DrawRectangle(Pens.LightSkyBlue, textRect);
                }
                if (i == hoveredItemIndex)
                    e.Graphics.DrawRectangle(Pens.Red, textRect);

                var sf = new StringFormat();
                if (rtl)
                    sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                var args = new PaintItemEventArgs(e.Graphics, e.ClipRectangle)
                               {
                                   Font = Font,
                                   TextRect = new RectangleF(textRect.Location, textRect.Size),
                                   StringFormat = sf,
                                   IsSelected = i == SelectedItemIndex,
                                   IsHovered = i == hoveredItemIndex
                               };
                //call drawing
                VisibleItems[i].OnPaint(args);
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate(true);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                SelectedItemIndex = PointToItemIndex(e.Location);
                ScrollToSelected();
                Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            SelectedItemIndex = PointToItemIndex(e.Location);
            Invalidate();
            OnItemSelected();
        }

        private void OnItemSelected()
        {
            if (ItemSelected != null)
                ItemSelected(this, EventArgs.Empty);
        }


        private int PointToItemIndex(Point p)
        {
            return (p.Y + VerticalScroll.Value)/ItemHeight;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var host = Parent as AutocompleteMenuHost;
            if (host != null)
                if (host.Menu.ProcessKey((char) keyData, Keys.None))
                    return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void SelectItem(int itemIndex)
        {
            SelectedItemIndex = itemIndex;
            ScrollToSelected();
            Invalidate();
        }

        public void SetItems(List<AutocompleteItem> items)
        {
            VisibleItems = items;
            SelectedItemIndex = -1;
            AdjustScroll();
            Invalidate();
        }

        public void SetToolTip(AutocompleteItem autocompleteItem)
        {
            string title = autocompleteItem.ToolTipTitle;
            string text = autocompleteItem.ToolTipText;

            if (string.IsNullOrEmpty(title))
            {
                toolTip.ToolTipTitle = null;
                toolTip.SetToolTip(this, null);
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                toolTip.ToolTipTitle = null;
                toolTip.Show(title, this, Width + 3, 0, ToolTipDuration);
            }
            else
            {
                toolTip.ToolTipTitle = title;
                toolTip.Show(text, this, Width + 3, 0, ToolTipDuration);
            }
        }
    }
}