// Last version at http://www.dotnetskin.net
// Created by PanWen 2005
//================================= 
//DotNetSkin
//=================================
// You may include the source code, modified source code, assembly
// within your own projects for either personal or commercial use 
// with the only one restriction:
// don't change the name library "DotNetSkin.SkinControls".

using System;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SparkFormEditor
{
    internal enum State
	{
		Normal = 1,
		MouseOver  = 2,
		MouseDown = 3,
		Disable = 4,
		Default = 5
	}

	/// <summary>
	/// skinButton
	/// </summary>
	internal class SkinButton:Button
	{

		private State state=State.Normal;

		public SkinButton()
		{
			try
			{
				this.SetStyle(ControlStyles.DoubleBuffer, true);
				this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				this.SetStyle(ControlStyles.UserPaint, true);
				this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
				this.SetStyle(ControlStyles.StandardDoubleClick, false);
				this.SetStyle(ControlStyles.Selectable, true);
				this.ResizeRedraw = true;
			}
			catch{}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			state = State.MouseOver;
			this.Invalidate();
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			state = State.Normal;
			this.Invalidate();
			base.OnMouseLeave(e);
		}

		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;

			state = State.MouseDown;
			this.Invalidate();
			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
				state = State.Normal;
			this.Invalidate();
			base.OnMouseUp(e);
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{	
            //if (SkinImage.button.img==null) 
            //{
            //    base.OnPaint(e);
            //    return;
            //}

			int i = (int)state;
			if (this.Focused && state != State.MouseDown) 	i = 5;
			if (!this.Enabled) i = 4;
			Rectangle rc = this.ClientRectangle;
			Graphics g = e.Graphics;

			base.InvokePaintBackground(this, new PaintEventArgs(e.Graphics, base.ClientRectangle));
			
            //SkinDraw.DrawRect2(g,SkinImage.button,rc,i);

			Image img = null;
			Size txts,imgs;

			txts = Size.Empty;
			imgs = Size.Empty;

			if ( this.Image != null ) 
			{
				img = this.Image;
			}
			else if ( this.ImageList != null && this.ImageIndex != -1)
			{
				img = this.ImageList.Images[this.ImageIndex];
			}

			if (img != null)
			{
				imgs.Width = img.Width;
				imgs.Height = img.Height;
			} 

			StringFormat format1;
			using (format1 = new StringFormat())
			{
				format1.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show;
				SizeF ef1 = g.MeasureString(this.Text, this.Font, new SizeF((float) rc.Width, (float) rc.Height), format1);
				txts = Size.Ceiling(ef1);
			}

			rc.Inflate(-4,-4);
			if (imgs.Width*imgs.Height != 0)
			{
				Rectangle imgr = rc;
				imgr = SkinDraw.HAlignWithin(imgs,imgr,this.ImageAlign);
				imgr = SkinDraw.VAlignWithin(imgs,imgr,this.ImageAlign);
				if (!this.Enabled)
				{
					ControlPaint.DrawImageDisabled(g,img, imgr.Left, imgr.Top, this.BackColor);
				}
				else
				{
					g.DrawImage(img,imgr.Left, imgr.Top, img.Width, img.Height);
				}
			}

			Rectangle txtr = rc;
			txtr = SkinDraw.HAlignWithin(txts,txtr,this.TextAlign);
			txtr = SkinDraw.VAlignWithin(txts,txtr,this.TextAlign);

			Brush brush2;
			format1 = new StringFormat();
			format1.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show;

			if (this.RightToLeft == RightToLeft.Yes)
			{
				format1.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
			brush2 = new SolidBrush(this.ForeColor);
			g.DrawString(this.Text, this.Font, brush2, (RectangleF) txtr, format1);
			brush2.Dispose();

		}
	}
	
	internal class SkinCheckBox: CheckBox
	{
		private State state = State.Normal;

		public SkinCheckBox()
		{
            //this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            //this.BackColor = System.Drawing.Color.Transparent;
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			state = State.MouseOver;
			this.Invalidate();
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			state = State.Normal;
			this.Invalidate();
			base.OnMouseLeave(e);
		}

		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;

			state = State.MouseDown;
			this.Invalidate();
			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
				state = State.Normal;
			this.Invalidate();
			base.OnMouseUp(e);
		}
		
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{	
			if (SkinImage.checkbox.img==null) 
			{
				base.OnPaint(e);
				return;
			}

            if (this.CheckState != CheckState.Indeterminate)
            {
                base.OnPaint(e);
            }
            else
            {

                //或者四种状态的图片
                int i = (int)state;
                if (!this.Enabled) i = 4;
                if (this.CheckState == CheckState.Checked) i += 4;
                if (this.CheckState == CheckState.Indeterminate) i += 8;

                Rectangle rc = this.ClientRectangle;
                Rectangle r1 = rc; ;
                Graphics g = e.Graphics;
                base.OnPaint(e);

                int cw = SystemInformation.MenuCheckSize.Width;

                if (this.CheckAlign == ContentAlignment.MiddleLeft)
                {
                    r1 = Rectangle.FromLTRB(0, (r1.Height - cw) / 2, 0 + cw, (r1.Height + cw) / 2);
                }
                else
                {
                    r1 = Rectangle.FromLTRB(r1.Right - cw + 2, (r1.Height - cw) / 2, r1.Right + 2, (r1.Height + cw) / 2);
                }

                SkinDraw.DrawRect1(g, SkinImage.checkbox, r1, i);
            }
		}
	}

	internal class SkinRadioButton:RadioButton
	{
		private State state=State.Normal;

		public SkinRadioButton()
		{
			this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			this.BackColor = System.Drawing.Color.Transparent;
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			state = State.MouseOver;
			this.Invalidate();
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			state = State.Normal;
			this.Invalidate();
			base.OnMouseLeave(e);
		}

		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;

			state = State.MouseDown;
			this.Invalidate();
			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
				state = State.Normal;
			this.Invalidate();
			base.OnMouseUp(e);
		}
		
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{	
            //if (SkinImage.radiobutton.img==null) 
            //{
            //    base.OnPaint(e);
            //    return;
            //}

			int i = (int)state;
			if (!this.Enabled) i = 4;
			if (this.Checked) i+=4;

			Rectangle rc = this.ClientRectangle;
			Rectangle r1 = rc;;
			Graphics g = e.Graphics;
			base.OnPaint(e);
			
			int cw = SystemInformation.MenuCheckSize.Width ;

			if (this.CheckAlign == ContentAlignment.MiddleLeft)
			{
				r1=Rectangle.FromLTRB(0,(r1.Height-cw)/2,0+cw,(r1.Height+cw)/2);
			} 
			else
			{
				r1=Rectangle.FromLTRB(r1.Right-cw-1,(r1.Height-cw)/2,r1.Right,(r1.Height+cw)/2);
			}
			
            //SkinDraw.DrawRect1(g,SkinImage.radiobutton,r1,i);
		}
	}
}
