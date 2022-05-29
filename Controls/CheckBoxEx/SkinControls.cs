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
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SparkFormEditor
{

	internal class ImageObject
	{
		public Bitmap img;
		public int Width;
		public int Height;
		public int Count;
		public Rectangle lr;

		public ImageObject(string str,int count,Rectangle r)
		{
            if (img == null)
            {
                Count = count;
                lr = r;
                img = GetResBitmap(str);
                if (img != null)
                {
                    Width = img.Width / Count;
                    Height = img.Height;
                }

                //在背景图上绘制，勾和叉 √×
                Graphics g = Graphics.FromImage(img);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                //g.DrawString("√", new Font("宋体", 9f, FontStyle.Bold), new SolidBrush(Color.Black), new Point();

                ////13*13的图标
                //g.DrawString("√", new Font("宋体", 11f, FontStyle.Bold), new SolidBrush(Color.Black), new Point(52 - 2, 2));
                //g.DrawString("√", new Font("宋体", 11f, FontStyle.Bold), new SolidBrush(Color.Black), new Point(65 - 2, 2));
                //g.DrawString("√", new Font("宋体", 11f, FontStyle.Bold), new SolidBrush(Color.Black), new Point(78 - 2, 2));
                //g.DrawString("√", new Font("宋体", 11f, FontStyle.Bold), new SolidBrush(Color.Black), new Point(91 - 2, 2));

                g.DrawString("×", new Font("宋体", 11f, FontStyle.Bold), new SolidBrush(Color.Black), new Point(104 - 1, 2));
                g.DrawString("×", new Font("宋体", 11f, FontStyle.Bold), new SolidBrush(Color.Black), new Point(117 - 1, 2));
                g.DrawString("×", new Font("宋体", 11f, FontStyle.Bold), new SolidBrush(Color.Black), new Point(130 - 1, 2));
                g.DrawString("×", new Font("宋体", 11f, FontStyle.Bold), new SolidBrush(Color.Black), new Point(143 - 1, 2));

                g.Dispose(); //释放
            }

		}

		~ImageObject()
		{
			if (img!=null) img.Dispose();
		}

		protected Stream FindStream(string str)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string [] resNames = assembly.GetManifestResourceNames();
			foreach( string s in resNames)
			{
				if (s == str)
				{
					return assembly.GetManifestResourceStream(s);
				}
			}
			return null;
		}

		protected Bitmap GetResBitmap(string str)
		{
			Stream sm;
			sm = FindStream(str);
			if ( sm == null ) return null;
			return new Bitmap(sm);
		}
	}

    internal enum Schemes
	{
		MacOs,
		Xp,
		Plex
	}

	internal class SkinImage:Component
	{

		//public static ImageObject button;
		public static ImageObject checkbox;
        //public static ImageObject radiobutton;

	
		private Schemes scheme = Schemes.MacOs;

		public SkinImage()
		{
		}

		static SkinImage()
		{
            Xp1skin();
		}

        //protected static void Macskin()
        //{
        //    button = new ImageObject("DotNetSkin.SkinControls.mac_button.png",5,Rectangle.FromLTRB(14,11,14,11));
        //    checkbox = new ImageObject("DotNetSkin.SkinControls.mac_checkbox.png",12,new Rectangle(0,0,0,0));
        //    radiobutton = new ImageObject("DotNetSkin.SkinControls.mac_radiobutton.png",8,new Rectangle(0,0,0,0));
        //}

		protected static void Xp1skin()
		{
            //button = new ImageObject("DotNetSkin.SkinControls.xp1_button.png",5,Rectangle.FromLTRB(8,9,8,9));
            //checkbox = new ImageObject("DotNetSkin.SkinControls.xp1_checkbox.png",12,new Rectangle(0,0,0,0));
            //radiobutton = new ImageObject("DotNetSkin.SkinControls.xp1_radiobutton.png",8,new Rectangle(0,0,0,0));
            //SparkFormEditor.Controls.CheckBoxEx.xp1_checkbox.png
            checkbox = new ImageObject("SparkFormEditor.Controls.CheckBoxEx.xp1_checkbox.png", 12, new Rectangle(0, 0, 0, 0));
		}

        //protected static void Plexskin()
        //{
        //    button = new ImageObject("DotNetSkin.SkinControls.Plex_button.png",5,Rectangle.FromLTRB(8,9,8,9));
        //    checkbox = new ImageObject("DotNetSkin.SkinControls.Plex_checkbox.png",12,new Rectangle(0,0,0,0));
        //    radiobutton = new ImageObject("DotNetSkin.SkinControls.Plex_radiobutton.png",8,new Rectangle(0,0,0,0));
        //}

		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
			}
			base.Dispose( disposing );
		}

		public Schemes Scheme
		{
			get
			{
				return scheme;
			}
			set
			{
				scheme = value;
                try
                {
                //    switch (scheme)
                //    {
                //        case Schemes.MacOs:
                //            Macskin();
                //            break;
						//case Schemes.Xp:
							Xp1skin();
                    //        break;
                    //    case Schemes.Plex:
                    //        Plexskin();
                    //        break;
                    //}
				}
				catch (Exception)
				{
					return;
				}
			}
		}

	}

	internal class SkinDraw
	{
		public static ContentAlignment anyRight = ContentAlignment.BottomRight | (ContentAlignment.MiddleRight | ContentAlignment.TopRight);
		public static ContentAlignment anyTop = ContentAlignment.TopRight | (ContentAlignment.TopCenter | ContentAlignment.TopLeft);
		public static ContentAlignment anyBottom = ContentAlignment.BottomRight | (ContentAlignment.BottomCenter | ContentAlignment.BottomLeft);
		public static ContentAlignment anyCenter = ContentAlignment.BottomCenter | (ContentAlignment.MiddleCenter | ContentAlignment.TopCenter);
		public static ContentAlignment anyMiddle = ContentAlignment.MiddleRight | (ContentAlignment.MiddleCenter | ContentAlignment.MiddleLeft);

		internal static void DrawRect1(Graphics g,ImageObject obj,Rectangle r,int index)
		{
			if (obj.img == null) return;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			Rectangle r1,r2;
			int x = (index-1)*obj.Width;
			int y = 0;
			int x1 = r.Left;
			int y1 = r.Top;
			r1 = new Rectangle(x,y, obj.Width,obj.Height);
			r2 = new Rectangle(x1,y1 + 1, r.Width - 3,r.Height - 3);
			g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);
		}

		internal static void DrawRect2(Graphics g,ImageObject obj,Rectangle r,int index)
		{
			if (obj.img == null) return;
			Rectangle r1,r2;
			int x = (index-1)*obj.Width;
			int y = 0;//obj.r.Top;
			int x1 = r.Left;
			int y1 = r.Top;

			if (r.Height > obj.Height && r.Width <= obj.Width)
			{
				r1 = new Rectangle(x,y, obj.Width,obj.lr.Top);
				r2 = new Rectangle(x1,y1, r.Width,obj.lr.Top);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);

				r1 = new Rectangle(x,y+obj.lr.Top, obj.Width,obj.Height-obj.lr.Top-obj.lr.Bottom);
				r2 = new Rectangle(x1,y1+obj.lr.Top, r.Width, r.Height-obj.lr.Top-obj.lr.Bottom);
				if ((obj.lr.Top+obj.lr.Bottom) == 0) r1.Height = r1.Height-1;
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);

				r1 = new Rectangle(x,y+obj.Height-obj.lr.Bottom, obj.Width,obj.lr.Bottom);
				r2 = new Rectangle(x1,y1+r.Height-obj.lr.Bottom, r.Width,obj.lr.Bottom);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);
			}
			else if (r.Height <= obj.Height && r.Width > obj.Width)
			{
				r1 = new Rectangle(x,y,obj.lr.Left,obj.Height);
				r2 = new Rectangle(x1,y1,obj.lr.Left,r.Height);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);
				r1 = new Rectangle(x+obj.lr.Left,y, obj.Width-obj.lr.Left-obj.lr.Right,obj.Height);
				r2 = new Rectangle(x1+obj.lr.Left,y1, r.Width-obj.lr.Left-obj.lr.Right,r.Height);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);
				r1 = new Rectangle(x+obj.Width-obj.lr.Right,y, obj.lr.Right,obj.Height);
				r2 = new Rectangle(x1+r.Width-obj.lr.Right,y1,obj.lr.Right,r.Height);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);
			} 
			else if (r.Height <= obj.Height && r.Width <= obj.Width)
			{
				r1 = new Rectangle((index-1)*obj.Width,0,obj.Width,obj.Height-1);
				//r1.Offset(obj.r.Left,obj.r.Top);
				g.DrawImage(obj.img,new Rectangle(x1,y1,r.Width,r.Height),r1,GraphicsUnit.Pixel);
			} 
			else if (r.Height > obj.Height && r.Width > obj.Width)
			{
				//top-left
				r1 = new Rectangle(x,y,obj.lr.Left,obj.lr.Top);
				r2 = new Rectangle(x1,y1,obj.lr.Left,obj.lr.Top);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);
				
				//top-bottom
				r1 = new Rectangle(x,y+obj.Height-obj.lr.Bottom,obj.lr.Left,obj.lr.Bottom);
				r2 = new Rectangle(x1,y1+r.Height-obj.lr.Bottom,obj.lr.Left,obj.lr.Bottom);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);

				//left
				r1 = new Rectangle(x,y+obj.lr.Top,obj.lr.Left,obj.Height-obj.lr.Top-obj.lr.Bottom);
				r2 = new Rectangle(x1,y1+obj.lr.Top,obj.lr.Left,r.Height-obj.lr.Top-obj.lr.Bottom);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);

				//top
				r1 = new Rectangle(x+obj.lr.Left,y,
					obj.Width-obj.lr.Left-obj.lr.Right,obj.lr.Top);
				r2 = new Rectangle(x1+obj.lr.Left,y1,
					r.Width-obj.lr.Left-obj.lr.Right,obj.lr.Top);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);

				//right-top
				r1 = new Rectangle(x+obj.Width-obj.lr.Right,y,obj.lr.Right,obj.lr.Top);
				r2 = new Rectangle(x1+r.Width-obj.lr.Right,y1,obj.lr.Right,obj.lr.Top);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);

				//Right
				r1 = new Rectangle(x+obj.Width-obj.lr.Right,y+obj.lr.Top, 
					obj.lr.Right,obj.Height-obj.lr.Top-obj.lr.Bottom);
				r2 = new Rectangle(x1+r.Width-obj.lr.Right,y1+obj.lr.Top, 
					obj.lr.Right,r.Height-obj.lr.Top-obj.lr.Bottom);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);

				//right-bottom
				r1 = new Rectangle(x+obj.Width-obj.lr.Right,y+obj.Height-obj.lr.Bottom,
					obj.lr.Right,obj.lr.Bottom);
				r2 = new Rectangle(x1+r.Width-obj.lr.Right,y1+r.Height-obj.lr.Bottom,
					obj.lr.Right,obj.lr.Bottom);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);

				//bottom
				r1 = new Rectangle(x+obj.lr.Left,y+obj.Height-obj.lr.Bottom,
					obj.Width-obj.lr.Left-obj.lr.Right,obj.lr.Bottom);
				r2 = new Rectangle(x1+obj.lr.Left,y1+r.Height-obj.lr.Bottom,
					r.Width-obj.lr.Left-obj.lr.Right,obj.lr.Bottom);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);

				//Center
				r1 = new Rectangle(x+obj.lr.Left,y+obj.lr.Top,
					obj.Width-obj.lr.Left-obj.lr.Right,obj.Height-obj.lr.Top-obj.lr.Bottom);
				//r1 = Rectangle.FromLTRB(x+obj.lr.Left,y+obj.lr.Top,
				//	x+obj.width-obj.lr.Right,y+obj.height-obj.lr.Bottom);
				r2 = new Rectangle(x1+obj.lr.Left,y1+obj.lr.Top,
					r.Width-obj.lr.Left-obj.lr.Right,r.Height-obj.lr.Top-obj.lr.Bottom);
				g.DrawImage(obj.img,r2,r1,GraphicsUnit.Pixel);
			}
			
		}

		internal static Rectangle HAlignWithin(Size alignThis, Rectangle withinThis, ContentAlignment align)
		{
			if ((align & anyRight) != (ContentAlignment) 0)
			{
				withinThis.X += (withinThis.Width - alignThis.Width);
			}
			else if ((align & anyCenter) != ((ContentAlignment) 0))
			{
				withinThis.X += ((withinThis.Width - alignThis.Width+1) / 2);
			}
			withinThis.Width = alignThis.Width;
			return withinThis;
		}

		internal static  Rectangle VAlignWithin(Size alignThis, Rectangle withinThis, ContentAlignment align)
		{
			if ((align & anyBottom) != ((ContentAlignment) 0))
			{
				withinThis.Y += (withinThis.Height - alignThis.Height);
			}
			else if ((align & anyMiddle) != ((ContentAlignment) 0))
			{
				withinThis.Y += ((withinThis.Height - alignThis.Height+1) / 2);
			}
			withinThis.Height = alignThis.Height;
			return withinThis;
		}
	}
}
