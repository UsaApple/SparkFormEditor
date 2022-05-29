#region License LGPL 3
// Copyright © Łukasz Świątkowski 2007–2010.
// http://www.lukesw.net/
//
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using VS = System.Windows.Forms.VisualStyles;

/*
<li>Base class for custom tooltips.</li>
<li>Office-2007-like tooltip class.</li>
*/
namespace SparkFormEditor
{
    //内部有弹出菜单的话，这个窗体就会被隐藏了。一般的下拉框也是这样，只有用专门的控件PopupComboBox
    //只能内部按钮继续嵌套：
    //complex = new Popup(complexPopup = new ComplexPopup());
    //complex.Resizable = true;
    //complexPopup.ButtonMore.Click += (_sender, _e) =>
    //{
    //    ComplexPopup cp = new ComplexPopup();
    //    cp.ButtonMore.Click +=
    //        (__sender, __e) => new Popup(new ComplexPopup()).Show(__sender as Button);
    //    new Popup(cp).Show(_sender as Button);
    //};


    /// <summary>
    /// Represents a pop-up window.
    /// </summary>
    [CLSCompliant(true), ToolboxItem(false)]
    internal partial class Popup : ToolStripDropDown
    {
        #region " Fields & Properties "

        /// <summary>
        /// Gets the content of the pop-up.
        /// </summary>
        public Control Content { get; private set; }

        /// <summary>
        /// Determines which animation to use while showing the pop-up window.
        /// </summary>
        public PopupAnimations ShowingAnimation { get; set; }

        /// <summary>
        /// Determines which animation to use while hiding the pop-up window.
        /// </summary>
        public PopupAnimations HidingAnimation { get; set; }

        /// <summary>
        /// Determines the duration of the animation.
        /// </summary>
        public int AnimationDuration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content should receive the focus after the pop-up has been opened.
        /// </summary>
        /// <value><c>true</c> if the content should be focused after the pop-up has been opened; otherwise, <c>false</c>.</value>
        /// <remarks>If the FocusOnOpen property is set to <c>false</c>, then pop-up cannot use the fade effect.</remarks>
        public bool FocusOnOpen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pressing the alt key should close the pop-up.
        /// </summary>
        /// <value><c>true</c> if pressing the alt key does not close the pop-up; otherwise, <c>false</c>.</value>
        public bool AcceptAlt { get; set; }

        private ToolStripControlHost _host;
        private Control _opener;
        private Popup _ownerPopup;
        private Popup _childPopup;
        private bool _resizableTop;
        private bool _resizableLeft;

        private bool _isChildPopupOpened;
        private bool _resizable;
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="PopupControl.Popup" /> is resizable.
        /// </summary>
        /// <value><c>true</c> if resizable; otherwise, <c>false</c>.</value>
        public bool Resizable
        {
            get { return _resizable && !_isChildPopupOpened; }
            set { _resizable = value; }
        }

        private bool _nonInteractive = false; //非激活，没有焦点
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="PopupControl.Popup"></see> acts like a transparent windows (so it cannot be clicked).
        /// </summary>
        /// <value>
        /// <c>true</c> if the popup is noninteractive; otherwise, <c>false</c>.</value>
        public bool NonInteractive
        {
            get { return _nonInteractive; }
            set
            {
                if (value != _nonInteractive)
                {
                    _nonInteractive = value;
                    if (IsHandleCreated) RecreateHandle();
                }
            }
        }

        #region 使窗口有鼠标穿透功能

        //[DllImport("user32", EntryPoint = "GetWindowLong")]
        //public static extern int GetWindowLong(
        //    IntPtr hwnd, int nIndex);
        //[DllImport("user32.dll")]
        //public static extern int SetWindowLong(
        //    IntPtr hwnd, int nIndex, int dwNewLong);

        //public const int GWL_EXSTYLE = -20;
        //public const int WS_EX_TRANSPARENT = 0x00000020;
        //public const int WS_EX_LAYERED = 0x00080000;

        ///// <summary>
        ///// 使窗口有鼠标穿透功能
        ///// </summary>
        //private void CanPenetrate()
        //{
        //    int intExTemp = GetWindowLong(this.Handle, GWL_EXSTYLE);
        //    int oldGWLEx = SetWindowLong(this.Handle, GWL_EXSTYLE, WS_EX_TRANSPARENT | WS_EX_LAYERED);
        //    //int oldGWLEx = Win32.SetWindowLong(this.Handle, Win32.GWL_EXSTYLE, Win32.WS_EX_TRANSPARENT);
        //}
        #endregion

        /// <summary>
        /// Gets or sets a minimum size of the pop-up.
        /// </summary>
        /// <returns>An ordered pair of type <see cref="T:System.Drawing.Size" /> representing the width and height of a rectangle.</returns>
        public new Size MinimumSize { get; set; }

        /// <summary>
        /// Gets or sets a maximum size of the pop-up.
        /// </summary>
        /// <returns>An ordered pair of type <see cref="T:System.Drawing.Size" /> representing the width and height of a rectangle.</returns>
        public new Size MaximumSize { get; set; }

        /// <summary>
        /// Gets parameters of a new window.
        /// </summary>
        /// <returns>An object of type <see cref="T:System.Windows.Forms.CreateParams" /> used when creating a new window.</returns>
        protected override CreateParams CreateParams
        {
            //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style = 0x40000000 | 0x02000000; //主控件焦点 (int)0x00040000 | (int)0x40000000 //会导致拖动后，没有刷新出来(没有0x02000000的话)很流畅

                cp.ExStyle |= Consts.WS_EX_NOACTIVATE;

                if (NonInteractive)
                {
                    //如果开启，那么不能上面的cp.Style = (int)0x40000000; 否则异常，创建句柄错误
                    cp.ExStyle |= Consts.WS_EX_TRANSPARENT | Consts.WS_EX_LAYERED | Consts.WS_EX_TOOLWINDOW;
                }

                return cp;

                ////NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW |
                //////让焦点还在原来的窗口控件上，提高用户体验，操作性
            }

            //get
            //{
            //    CreateParams ret = base.CreateParams;
            //    ret.Style = (int)0x00040000 | (int)0x40000000;
            //    ret.ExStyle |= NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW;
            //    ret.X = this.Location.X;
            //    ret.Y = this.Location.Y;

            //    return ret;
            //}
        }

        #endregion

        #region " Constructors "

        /// <summary>
        /// Initializes a new instance of the <see cref="PopupControl.Popup"/> class.
        /// </summary>
        /// <param name="content">The content of the pop-up.</param>
        /// <remarks>
        /// Pop-up will be disposed immediately after disposion of the content control.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="content" /> is <code>null</code>.</exception>
        public Popup(Control content)
        {
            //this.BackColor = Color.Blue;

            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            Content = content;
            FocusOnOpen = true;
            AcceptAlt = true;
            ShowingAnimation = PopupAnimations.SystemDefault;
            HidingAnimation = PopupAnimations.None;
            AnimationDuration = 100;
            InitializeComponent();
            AutoSize = false;
            DoubleBuffered = true;
            ResizeRedraw = true;
            _host = new ToolStripControlHost(content);
            Padding = Margin = _host.Padding = _host.Margin = Padding.Empty;
            if (NativeMethods.IsRunningOnMono) content.Margin = Padding.Empty;
            MinimumSize = content.MinimumSize;
            content.MinimumSize = content.Size;
            MaximumSize = content.MaximumSize;
            content.MaximumSize = content.Size;
            Size = content.Size;
            if (NativeMethods.IsRunningOnMono) _host.Size = content.Size;
            TabStop = content.TabStop = true;
            content.Location = Point.Empty;
            Items.Add(_host);
            content.Disposed += (sender, e) =>
            {
                content = null;
                Dispose(true);
            };
            content.RegionChanged += (sender, e) => UpdateRegion();
            content.Paint += (sender, e) => PaintSizeGrip(e);
            UpdateRegion();

            //窗口鼠标穿透效果：透明区域可以透过，但是界面上也不能点击了。
            //CanPenetrate();


        }

        #endregion

        #region " Methods "

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.ToolStripItem.VisibleChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (NativeMethods.IsRunningOnMono) return; // in case of non-Windows
            if ((Visible && ShowingAnimation == PopupAnimations.None) || (!Visible && HidingAnimation == PopupAnimations.None))
            {
                return;
            }
            AnimationFlags flags = Visible ? AnimationFlags.Roll : AnimationFlags.Hide;
            PopupAnimations _flags = Visible ? ShowingAnimation : HidingAnimation;
            if (_flags == PopupAnimations.SystemDefault)
            {
                if (SystemInformation.IsMenuAnimationEnabled)
                {
                    if (SystemInformation.IsMenuFadeEnabled)
                    {
                        _flags = PopupAnimations.Blend;
                    }
                    else
                    {
                        _flags = PopupAnimations.Slide | (Visible ? PopupAnimations.TopToBottom : PopupAnimations.BottomToTop);
                    }
                }
                else
                {
                    _flags = PopupAnimations.None;
                }
            }
            if ((_flags & (PopupAnimations.Blend | PopupAnimations.Center | PopupAnimations.Roll | PopupAnimations.Slide)) == PopupAnimations.None)
            {
                return;
            }
            if (_resizableTop) // popup is “inverted”, so the animation must be
            {
                if ((_flags & PopupAnimations.BottomToTop) != PopupAnimations.None)
                {
                    _flags = (_flags & ~PopupAnimations.BottomToTop) | PopupAnimations.TopToBottom;
                }
                else if ((_flags & PopupAnimations.TopToBottom) != PopupAnimations.None)
                {
                    _flags = (_flags & ~PopupAnimations.TopToBottom) | PopupAnimations.BottomToTop;
                }
            }
            if (_resizableLeft) // popup is “inverted”, so the animation must be
            {
                if ((_flags & PopupAnimations.RightToLeft) != PopupAnimations.None)
                {
                    _flags = (_flags & ~PopupAnimations.RightToLeft) | PopupAnimations.LeftToRight;
                }
                else if ((_flags & PopupAnimations.LeftToRight) != PopupAnimations.None)
                {
                    _flags = (_flags & ~PopupAnimations.LeftToRight) | PopupAnimations.RightToLeft;
                }
            }
            flags = flags | (AnimationFlags.Mask & (AnimationFlags)(int)_flags);
            NativeMethods.SetTopMost(this);
            NativeMethods.AnimateWindow(this, AnimationDuration, flags);
        }
        

        /// <summary>
        /// Processes a dialog box key.
        /// </summary>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process.</param>
        /// <returns>
        /// true if the key was processed by the control; otherwise, false.
        /// </returns>
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (AcceptAlt && ((keyData & Keys.Alt) == Keys.Alt))
            {
                if ((keyData & Keys.F4) != Keys.F4)
                {
                    return false;
                }
                else
                {
                    Close();
                }
            }
            bool processed = base.ProcessDialogKey(keyData);
            if (!processed && (keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift)))
            {
                bool backward = (keyData & Keys.Shift) == Keys.Shift;
                Content.SelectNextControl(null, !backward, true, true, true);
            }
            return processed;
        }

        /// <summary>
        /// Updates the pop-up region.
        /// </summary>
        protected void UpdateRegion()
        {
            if (Region != null)
            {
                Region.Dispose();
                Region = null;
            }
            if (Content.Region != null)
            {
                Region = Content.Region.Clone();
            }
        }

        /// <summary>
        /// Shows the pop-up window below the specified control.
        /// </summary>
        /// <param name="control">The control below which the pop-up will be shown.</param>
        /// <remarks>
        /// When there is no space below the specified control, the pop-up control is shown above it.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="control"/> is <code>null</code>.</exception>
        public void Show(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            Show(control, control.ClientRectangle);

            //滚动条显示到最后一页
            if (Content is ComplexPopup)
            {
                (Content as ComplexPopup).SetBottomScrolll();
            }
        }

        /// <summary>
        /// Shows the pop-up window below the specified area.
        /// </summary>
        /// <param name="area">The area of desktop below which the pop-up will be shown.</param>
        /// <remarks>
        /// When there is no space below specified area, the pop-up control is shown above it.
        /// </remarks>
        public void Show(Rectangle area)
        {
            _resizableTop = _resizableLeft = false;
            Point location = new Point(area.Left, area.Top + area.Height);
            Rectangle screen = Screen.FromControl(this).WorkingArea;
            if (location.X + Size.Width > (screen.Left + screen.Width))
            {
                _resizableLeft = true;
                location.X = (screen.Left + screen.Width) - Size.Width;
            }
            if (location.Y + Size.Height > (screen.Top + screen.Height))
            {
                _resizableTop = true;
                location.Y -= Size.Height + area.Height;
            }
            //location = control.PointToClient(location);
            Show(location, ToolStripDropDownDirection.BelowRight);
        }

        /// <summary>
        /// Shows the pop-up window below the specified area of the specified control.
        /// </summary>
        /// <param name="control">The control used to compute screen location of specified area.</param>
        /// <param name="area">The area of control below which the pop-up will be shown.</param>
        /// <remarks>
        /// When there is no space below specified area, the pop-up control is shown above it.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="control"/> is <code>null</code>.</exception>
        public void Show(Control control, Rectangle area)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            SetOwnerItem(control);

            _resizableTop = _resizableLeft = false;
            Point location = control.PointToScreen(new Point(area.Left, area.Top + area.Height));
            Rectangle screen = Screen.FromControl(control).WorkingArea;
            if (location.X + Size.Width > (screen.Left + screen.Width))
            {
                _resizableLeft = true;
                location.X = (screen.Left + screen.Width) - Size.Width;
            }
            if (location.Y + Size.Height > (screen.Top + screen.Height))
            {
                _resizableTop = true;
                location.Y -= Size.Height + area.Height;
            }
            location = control.PointToClient(location);
            Show(control, location, ToolStripDropDownDirection.BelowRight);
        }

        private void SetOwnerItem(Control control)
        {
            if (control == null)
            {
                return;
            }
            if (control is Popup)
            {
                Popup popupControl = control as Popup;
                _ownerPopup = popupControl;
                _ownerPopup._childPopup = this;
                OwnerItem = popupControl.Items[0];
                return;
            }
            else if (_opener == null)
            {
                _opener = control;
            }
            if (control.Parent != null)
            {
                SetOwnerItem(control.Parent);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            if (Content != null)
            {
                Content.MinimumSize = Size;
                Content.MaximumSize = Size;
                Content.Size = Size;
                Content.Location = Point.Empty;
            }
            base.OnSizeChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Design" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.LayoutEventArgs" /> that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            if (!NativeMethods.IsRunningOnMono)
            {
                base.OnLayout(e);
                return;
            }
            Size suggestedSize = GetPreferredSize(Size.Empty);
            if (AutoSize && suggestedSize != Size)
            {
                Size = suggestedSize;
            }
            SetDisplayedItems();
            OnLayoutCompleted(EventArgs.Empty);
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.ToolStripDropDown.Opening" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnOpening(CancelEventArgs e)
        {
            if (Content.IsDisposed || Content.Disposing)
            {
                e.Cancel = true;
                return;
            }
            UpdateRegion();
            base.OnOpening(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.ToolStripDropDown.Opened" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnOpened(EventArgs e)
        {
            if (_ownerPopup != null)
            {
                _ownerPopup._isChildPopupOpened = true;
            }
            if (FocusOnOpen)
            {
                Content.Focus();
            }
            base.OnOpened(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.ToolStripDropDown.Closed"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripDropDownClosedEventArgs"/> that contains the event data.</param>
        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            _opener = null;
            if (_ownerPopup != null)
            {
                _ownerPopup._isChildPopupOpened = false;
            }
            base.OnClosed(e);
        }

        #endregion

        #region " Resizing Support "

        /// <summary>
        /// Processes Windows messages.
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message" /> to process.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            //if (m.Msg == NativeMethods.WM_PRINT && !Visible)
            //{
            //    Visible = true;
            //}
            if (InternalProcessResizing(ref m, false))
            {
                return;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Processes the resizing messages.
        /// </summary>
        /// <param name="m">The message.</param>
        /// <returns>true, if the WndProc method from the base class shouldn't be invoked.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool ProcessResizing(ref Message m)
        {
            return InternalProcessResizing(ref m, true);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private bool InternalProcessResizing(ref Message m, bool contentControl)
        {
            if (m.Msg == Consts.WM_NCACTIVATE && m.WParam != IntPtr.Zero && _childPopup != null && _childPopup.Visible)
            {
                _childPopup.Hide();
            }
            if (!Resizable && !NonInteractive)
            {
                return false;
            }
            if (m.Msg == Consts.WM_NCHITTEST)
            {
                return OnNcHitTest(ref m, contentControl);
            }
            else if (m.Msg == Consts.WM_GETMINMAXINFO)
            {
                return OnGetMinMaxInfo(ref m);
            }
            return false;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private bool OnGetMinMaxInfo(ref Message m)
        {
            Struct.MINMAXINFO minmax = (Struct.MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(Struct.MINMAXINFO));
            if (!MaximumSize.IsEmpty)
            {
                minmax.maxTrackSize = MaximumSize;
            }
            minmax.minTrackSize = MinimumSize;
            Marshal.StructureToPtr(minmax, m.LParam, false);
            return true;
        }

        private bool OnNcHitTest(ref Message m, bool contentControl)
        {
            if (NonInteractive)
            {
                m.Result = (IntPtr)Consts.HTTRANSPARENT;
                return true;
            }

            int x = Cursor.Position.X; // NativeMethods.LOWORD(m.LParam);
            int y = Cursor.Position.Y; // NativeMethods.HIWORD(m.LParam);
            Point clientLocation = PointToClient(new Point(x, y));

            GripBounds gripBouns = new GripBounds(contentControl ? Content.ClientRectangle : ClientRectangle);
            IntPtr transparent = new IntPtr(Consts.HTTRANSPARENT);

            if (_resizableTop)
            {
                if (_resizableLeft && gripBouns.TopLeft.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)Consts.HTTOPLEFT;
                    return true;
                }
                if (!_resizableLeft && gripBouns.TopRight.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)Consts.HTTOPRIGHT;
                    return true;
                }
                if (gripBouns.Top.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)Consts.HTTOP;
                    return true;
                }
            }
            else
            {
                if (_resizableLeft && gripBouns.BottomLeft.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)Consts.HTBOTTOMLEFT;
                    return true;
                }
                if (!_resizableLeft && gripBouns.BottomRight.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)Consts.HTBOTTOMRIGHT;
                    return true;
                }
                if (gripBouns.Bottom.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)Consts.HTBOTTOM;
                    return true;
                }
            }
            if (_resizableLeft && gripBouns.Left.Contains(clientLocation))
            {
                m.Result = contentControl ? transparent : (IntPtr)Consts.HTLEFT;
                return true;
            }
            if (!_resizableLeft && gripBouns.Right.Contains(clientLocation))
            {
                m.Result = contentControl ? transparent : (IntPtr)Consts.HTRIGHT;
                return true;
            }
            return false;
        }

        private VS.VisualStyleRenderer _sizeGripRenderer;
        /// <summary>
        /// Paints the sizing grip.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs" /> instance containing the event data.</param>
        public void PaintSizeGrip(PaintEventArgs e)
        {
            if (e == null || e.Graphics == null || !_resizable)
            {
                return;
            }
            Size clientSize = Content.ClientSize;
            using (Bitmap gripImage = new Bitmap(0x10, 0x10))
            {
                using (Graphics g = Graphics.FromImage(gripImage))
                {
                    if (Application.RenderWithVisualStyles)
                    {
                        if (_sizeGripRenderer == null)
                        {
                            _sizeGripRenderer = new VS.VisualStyleRenderer(VS.VisualStyleElement.Status.Gripper.Normal);
                        }
                        _sizeGripRenderer.DrawBackground(g, new Rectangle(0, 0, 0x10, 0x10));
                    }
                    else
                    {
                        ControlPaint.DrawSizeGrip(g, Content.BackColor, 0, 0, 0x10, 0x10);
                    }
                }



                GraphicsState gs = e.Graphics.Save();
                e.Graphics.ResetTransform();
                if (_resizableTop)
                {
                    if (_resizableLeft)
                    {
                        e.Graphics.RotateTransform(180);
                        e.Graphics.TranslateTransform(-clientSize.Width, -clientSize.Height);
                    }
                    else
                    {
                        e.Graphics.ScaleTransform(1, -1);
                        e.Graphics.TranslateTransform(0, -clientSize.Height);
                    }
                }
                else if (_resizableLeft)
                {
                    e.Graphics.ScaleTransform(-1, 1);
                    e.Graphics.TranslateTransform(-clientSize.Width, 0);
                }
                e.Graphics.DrawImage(gripImage, clientSize.Width - 0x10, clientSize.Height - 0x10 + 1, 0x10, 0x10);
                e.Graphics.Restore(gs);

                //e.Graphics.DrawRectangle(Pens.Blue, new Rectangle(0, 0, this.Width - 2, this.Height - 2));  //外边框线，不全，只有左和上
            }
        }

        #endregion
    }

}
