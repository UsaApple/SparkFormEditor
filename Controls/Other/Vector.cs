using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using SparkFormEditor.Model;

namespace SparkFormEditor
{
    /// <summary>
    /// 躯体感觉控件 (人体图)
    /// </summary>
    internal partial class Vector : PictureBox
    {

        #region 全局变量
        string _stritem = string.Empty;                             //图标配置大字段：Items="图标一@30,30@Line|Red|0.5||[0,0][29,0]|true,8,4§Line|Red|0.……
        public Image_Table _imagetable;                             //保存所有内容
        Ct_Images _ctimages;
        Ct_ImagesCom _ctimagecom;
        List<Ct_ImagesCom> _listctimagecom;
        List<Ct_ImagesCom> _listctimagecomsome;

        Dictionary<string, Ct_Images> _ctimageitem;                 //保存图标的字典
        List<Bitmap> _listpic = new List<Bitmap>();                 //保存图标的位图
        private string _strdata;                                    //数据 
        SparkFormEditor _nurseFormEditor;                               //该变量是传过来的，用于表单主界面判断是否修改过

        Point _ptTemClick = new Point();                            //鼠标点击当前位置，添加图标

        //#region 控件命名  删除根据名字删除
        ////（支持同一个区域图标存在多个，但是数据库中的名字都一样，只是坐标不一样）
        ////private int _ctname = 0;
        ////string ctDelete = string.Empty;
        //#endregion

        /// <summary>
        /// 保存身体感觉的配置信息
        /// </summary>
        internal class Image_Table
        {
            public string CtName = string.Empty;                //节点的name  可有可无
            public string CtImageName = string.Empty;           //背景图片名称
            public PointF CtPointF;                             //控件的坐标
            public SizeF CtSizeF;                               //控件的大小
            public Dictionary<string, Ct_Images> CtImageItem;   //图标集合
        }

        ///保存单个图标的配置信息
        internal class Ct_Images
        {
            //具体Items="图标一@size@Line|颜色|粗细|虚实线|[10,10][10,100]§Line|颜色|粗细|虚实线|[10,10][10,100]¤图标二@……" 
            public string ImgName = string.Empty;              //图标名字
            public SizeF ImgSizeF;                             //图标的大小
            public List<Ct_ImagesCom> ListCom;                 //图标有哪些东西组成的
            public List<Ct_ImagesCom> ListComSome;             //小图标样式

        }
        /// <summary>
        /// 每条线的画法
        /// </summary>
        internal class Ct_ImagesCom
        {
            public string ImgType = string.Empty;                //图标的类型  line value
            public string ImgValue = string.Empty;               //如果是文字的话  那么是字符
            public Color ImgColor ;                              //图标的颜色
            public float ImgWidth = 1;                           //粗细
            public string ImgDash = string.Empty;                //线的类型

            public PointF ImgSPointF;                            //起始坐标
            public PointF ImgEPointF;                            //结束坐标

            public bool IsVertical = false;                      //false 表示横向平铺 反正true 纵向
            public int TitleNum = 0;                             //为平铺条数 为0表示不平铺
            public int TitleDistance = 0;                        // 每行距离
        }
        #endregion

        /// <summary>
        /// 构造函数  
        /// </summary>
        /// <param name="XmlSet">配置文件节点</param>
        /// <param name="StrData">打开时根据数据展现内容</param>
        /// <param name="nurseFormEditor">传过来的表单主窗体对象 控制修改提醒保存的</param>
        public Vector(XmlNode XmlSet, Image BackImg, SparkFormEditor nurseFormEditor)
        {
            InitializeComponent();

            _nurseFormEditor = nurseFormEditor;
            this.SizeMode = PictureBoxSizeMode.StretchImage;
            this.BorderStyle = BorderStyle.FixedSingle;

            try
            {
                _imagetable = new Image_Table();
                _ctimageitem = new Dictionary<string, Ct_Images>();
                //初始化参数 把数据保存到Image_Table 
                LoadXml(XmlSet);
                _imagetable.CtImageItem = _ctimageitem;

                //控件大小
                this.Size = new Size((int)_imagetable.CtSizeF.Width, (int)_imagetable.CtSizeF.Height);
                this.Image = BackImg;
                //加载菜单
                LoadMouseMenu();

                tsmiDelete.Click += new EventHandler(tsmiDelete_Click);

                this.MouseDown += new MouseEventHandler(Vector_MouseDown);

                #region 透明前提工作
                //双缓存开启，另外Form的DoubleBuffer的属性为True
                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.DoubleBuffer, true);
                this.SetStyle(ControlStyles.UserPaint, true);
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                #endregion

            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex.ToString());
                throw ex;
            }
        }



        /// <summary>
        /// 人体示意图="图标一@坐标¤图标二@坐标¤图标三@坐标"
        /// 初始化数据
        /// 外部调用方法  传入数据  初始化界面
        /// 传入空字符串 表示清空控件数据
        /// </summary>
        public void LoadData(string _strdata)
        {
            try
            {
                //先清空界面数据,不然会叠加。
                ResetClear();       // this.Controls.Clear();

                //再根据数据添加：区域图标
                if (!_strdata.Equals(""))
                {
                    string[] strData = _strdata.Split('¤');
                    foreach (string str in strData)
                    {
                        AddPic(str.Split('@')[0], Comm.getPoint(str.Split('@')[1]));
                    }
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 清空界面
        /// 重置和每次加载前，先清空
        /// </summary>
        public void ResetClear()
        {
            this.Controls.Clear();
        }

        /// <summary>
        /// 界面上添加控件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="point"></param>
        private void AddPic(string name, PointF point)
        {
            try
            {
            
                if (_ctimageitem != null && _ctimageitem.Count != 0)
                {
                    foreach (KeyValuePair<string, Ct_Images> dickey in _ctimageitem)
                    {
                        if (dickey.Key.Equals(name))
                        {
                            PictureBox picbox = new PictureBox();
                            picbox.Cursor = Cursors.Hand;
                            picbox.Image = new Bitmap((int)dickey.Value.ImgSizeF.Width, (int)dickey.Value.ImgSizeF.Height);

                            point = new PointF(point.X - picbox.Image.Width / 2, point.Y - picbox.Image.Height / 2);  //鼠标点击位置为中心，添加图标

                            //调用方法绘制
                            picbox.BackColor = Color.Transparent;
                            Graphics g = Graphics.FromImage(picbox.Image);
                            g.SmoothingMode = SmoothingMode.AntiAlias;

                            SetMenuPic(g, dickey.Value, false);


                            //透明处理  需要做几个工作 
                            //1  picbox.BackColor = Color.Transparent; 
                            //2  g.SmoothingMode = SmoothingMode.AntiAlias; 不加会出现虚线  不知道原理  这属性是用在画弧线和斜线时消除锯齿的 
                            //3 初始化方法中开启双缓存
                            //ControlTrans(picbox, picbox.Image);

                            //保存数据需要用到的
                            picbox.Tag = dickey.Value.ImgName;
                            picbox.Show();

                            picbox.Size = picbox.Image.Size;
                            picbox.Left = (int)point.X + picbox.Size.Width / 2 > this.Width ? (int)point.X - picbox.Size.Width / 2 : (int)point.X ;
                            picbox.Top = (int)point.Y + picbox.Size.Height / 2 > this.Height ? (int)point.Y - picbox.Size.Height / 2 : (int)point.Y;

                            picbox.ContextMenuStrip = cmsDelete;

                            // 为删除做准备  防止名字重复  方便后面删除
                            //picbox.Name = _ctname.ToString();
                            //_ctname++;

                            picbox.MouseEnter += new EventHandler(picbox_MouseEnter);

                            //控件拖动
                            picbox.MouseDown += new MouseEventHandler(picbox_MouseDown);
                            picbox.MouseMove += new MouseEventHandler(picbox_MouseMove);
                            this.Controls.Add(picbox);

                            //跳出  避免多次循环
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex.ToString());
                throw ex;
            }
        }

        #region 外部调用方法  可返回位图和具体数据
        /// <summary>
        /// 外部调用方法  
        /// 返回当前数据  拼接字符串，保存到数据库
        /// </summary>
        /// <returns></returns>
        public string GetSaveData()
        {
            try
            {
                _strdata = "";
                foreach (Control pic in this.Controls)
                {
                    if (pic is PictureBox)
                    {
                        if (_strdata.Equals(""))
                        {
                            _strdata = pic.Tag.ToString() + "@" + (pic.Left + pic.Width / 2).ToString() + "," + (pic.Top + pic.Height / 2).ToString();
                        }
                        else //不是空的话加分太阳分隔符¤
                        {
                            // - picbox.Image.Width / 2, point.Y - picbox.Image.Height / 2
                            _strdata = _strdata + "¤" + pic.Tag.ToString() + "@" + (pic.Left + pic.Width / 2).ToString() + "," + (pic.Top + pic.Height / 2).ToString();
                        }
                    }
                }

                return _strdata;
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 外部调用方法  返回bitmap 位图
        /// ps： 该方法需要改进  直接绘制内容效果更好
        /// </summary>
        /// <returns></returns>
        public Bitmap GetDataToPic()
        {
            try
            {
                Bitmap formBitmap = new Bitmap(this.Width, this.Height);
                this.DrawToBitmap(formBitmap, new Rectangle(0, 0, this.Width, this.Height));

                return formBitmap;
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex.ToString());
                throw ex;
            }

        }
        #endregion

        #region 鼠标事件
        //这里的变量主要控制控件移动其他的地方不用  所以  就放在鼠标事件一起 
        bool IsMoving = false;
        ///控件当前位置（移动前）
        Point PCtrlLastCoordinate = new Point(0, 0);
        Point PCursorOffset = new Point(0, 0);
        ///光标当前位置（移动前）
        Point PCursorLastCoordinate = new Point(0, 0);

        /// <summary>
        /// 开始拖动控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void picbox_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox pic = (PictureBox)sender;
            if (e.Button == MouseButtons.Left)
            {
                if (this.IsMoving)
                {
                    Point pCursor = Cursor.Position;
                    PCursorOffset.X = pCursor.X - PCursorLastCoordinate.X;
                    PCursorOffset.Y = pCursor.Y - PCursorLastCoordinate.Y;
                    if (PCtrlLastCoordinate.X + PCursorOffset.X > -pic.Width / 2 && (PCtrlLastCoordinate.X + PCursorOffset.X + pic.Width / 2) < this.Size.Width)
                    {
                        pic.Left = PCtrlLastCoordinate.X + PCursorOffset.X;
                    }

                    if (PCtrlLastCoordinate.Y + PCursorOffset.Y > -pic.Height / 2 && (PCtrlLastCoordinate.Y + PCursorOffset.Y + pic.Height / 2) < this.Size.Height)
                    {
                        pic.Top = PCtrlLastCoordinate.Y + PCursorOffset.Y;
                    }

                    _nurseFormEditor.IsNeedSave = true;
                }
            }
        }

        /// <summary>
        /// 记录控件拖动前为止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void picbox_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox pic = (PictureBox)sender;
            if (e.Button == MouseButtons.Left)
            {
                IsMoving = true;
                PCtrlLastCoordinate.X = pic.Left;
                PCtrlLastCoordinate.Y = pic.Top;
                PCursorLastCoordinate.X = Cursor.Position.X;
                PCursorLastCoordinate.Y = Cursor.Position.Y;
            }
        }

        /// <summary>
        /// 鼠标进入 保存控件的名字  方便删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void picbox_MouseEnter(object sender, EventArgs e)
        {
            PictureBox pic = (PictureBox)sender;
            //ctDelete = pic.Name;
        }

        /// <summary>
        /// 右键添加对应的控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tsi_ClickToAddPic(object sender, EventArgs e)
        {
            ToolStripItem tsi = (ToolStripItem)sender;
            AddPic(tsi.Text, _ptTemClick);
            _nurseFormEditor.IsNeedSave = true;
        }


        /// <summary>
        /// 删除
        /// 右击选择的区域图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tsmiDelete_Click(object sender, EventArgs e)
        {
            //this.Controls.Find(ctDelete, true)[0].Dispose();

            cmsDelete.SourceControl.Dispose();

            _nurseFormEditor.IsNeedSave = true;
        }

        /// <summary>
        /// 添加之前获取当前的位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Vector_MouseDown(object sender, MouseEventArgs e)
        {
            //获取当前位置  第一个为错误方法！  可能导致错乱
            //_ptTemClick = this.PointToClient(Control.MousePosition);
            _ptTemClick = new Point(e.X, e.Y);
        }

        #endregion

        /// <summary>
        /// 加载鼠标事件
        /// 1 右键单击  单击空白处显示可以添加的控件图标  
        /// 单击图标可以删除该图标 这个在AddPic()方法中处理
        /// 2  在控件图片上左击 选中  可移动位置  也在AddPic()方法中处理  
        /// 这里主要加载 右键菜单栏
        /// </summary>
        private void LoadMouseMenu()
        {
            try
            {
                Graphics g;
                ToolStripItem tsi;

                foreach (KeyValuePair<string, Ct_Images> key in _imagetable.CtImageItem)
                {
                    tsi = new ToolStripMenuItem(key.Key);
                    tsi.Text = key.Key;
                    //单击添加事件
                    tsi.Click += new EventHandler(tsi_ClickToAddPic);
                    tsi.Image = new Bitmap(12, 12);

                    //加载菜单小图标
                    g = Graphics.FromImage(tsi.Image);
                    SetMenuPic(g, key.Value, true);
                    cmsAddPic.Items.Add(tsi);

                    g.Dispose();
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex.ToString());
                throw ex;
            }
        }

        #region 图标绘制方法

        /// <summary>
        /// 右键菜单或者图标
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ctimage"></param>
        /// <param name="Thumbnails"> 菜单的缩略图还是其绘制控件图标</param>
        private void SetMenuPic(Graphics g, Ct_Images ctimage, bool thumbnails)
        {
            if (thumbnails)
            {
                if (ctimage.ListComSome != null && ctimage.ListComSome.Count != 0)
                {
                    SetMenuPic(g, ctimage.ListComSome);
                }
            }
            else
            {
                if (ctimage.ListCom != null && ctimage.ListCom.Count != 0)
                {
                    SetMenuPic(g, ctimage.ListCom);
                }
            }
        }

        /// <summary>
        /// 给定数据画图
        /// </summary>
        /// <param name="g"></param>
        /// <param name="listCom"></param>
        private void SetMenuPic(Graphics g, List<Ct_ImagesCom> listCom)
        {
            Pen p;
            foreach (Ct_ImagesCom ct_ic in listCom)
            {
                if (ct_ic.ImgType.ToLower() == "line")
                {
                    p = new Pen(ct_ic.ImgColor, ct_ic.ImgWidth);

                    if (ct_ic.ImgDash.Equals("dash"))
                    {
                        p.DashStyle = DashStyle.Dash;
                    }
                    else
                    {
                        p.DashStyle = DashStyle.Solid;
                    }

                    g.DrawLine(p, ct_ic.ImgSPointF, ct_ic.ImgEPointF);

                    //这次当参数是0的时候不平铺
                    for (int i = 2; i <= ct_ic.TitleNum; i++)
                    {
                        //false 表示横向向下平铺 反正true 纵向向右
                        if (!ct_ic.IsVertical)
                        {
                            g.DrawLine(p, new PointF(ct_ic.ImgSPointF.X + (i - 1) * ct_ic.TitleDistance, ct_ic.ImgSPointF.Y), new PointF(ct_ic.ImgEPointF.X + (i - 1) * ct_ic.TitleDistance, ct_ic.ImgEPointF.Y));
                        }
                        else
                        {
                            g.DrawLine(p, new PointF(ct_ic.ImgSPointF.X, ct_ic.ImgSPointF.Y + (i - 1) * ct_ic.TitleDistance), new PointF(ct_ic.ImgEPointF.X, ct_ic.ImgEPointF.Y + (i - 1) * ct_ic.TitleDistance));
                        }
                    }

                    p.Dispose();

                }
                else if (ct_ic.ImgType.ToLower() == "string") //留个  可能是字符
                {
                    p = new Pen(ct_ic.ImgColor);
                    g.DrawString(ct_ic.ImgValue, Comm.getFont(ct_ic.ImgDash), p.Brush, ct_ic.ImgSPointF);
                    p.Dispose();

                }
                else if (ct_ic.ImgType.ToLower() == "ellipse")
                {
                    p = new Pen(ct_ic.ImgColor);
                    g.FillEllipse(p.Brush, ct_ic.ImgSPointF.X, ct_ic.ImgSPointF.Y, ct_ic.ImgEPointF.X, ct_ic.ImgEPointF.Y);
                    p.Dispose();
                }
            }

            g.Dispose();
        }
        #endregion

        #region xml配置数据读取
        /// <summary>
        /// 读取xml的配置信息保存到数据结构中
        /// </summary>
        /// <param name="XmlSet"></param>
        private void LoadXml(XmlNode XmlSet)
        {
            try
            {
                _imagetable.CtName = (XmlSet as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                _imagetable.CtImageName = (XmlSet as XmlElement).GetAttribute(nameof(EntXmlModel.Default));
                _imagetable.CtPointF = Comm.getPoint((XmlSet as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)));
                _imagetable.CtSizeF = Comm.getSize((XmlSet as XmlElement).GetAttribute(nameof(EntXmlModel.Size)));
                _stritem = (XmlSet as XmlElement).GetAttribute(nameof(EntXmlModel.Items));

                string[] strchar = _stritem.Split('¤');
                string[] strstyle1;
                string[] strstyle2;
                string strtem;
                foreach (string strstyle in strchar)
                {
                    //配置为空则跳出
                    if (strstyle.Equals(""))
                    {
                        break;
                    }

                    //图标一@size@Line|颜色|粗细|虚实线|[10,10][10,100]§Line|颜色|粗细|虚实线|[10,10][10,100]
                    _ctimages = new Ct_Images();
                    strstyle1 = strstyle.Split('@');
                    _ctimages.ImgName = strstyle1[0];
                    _ctimages.ImgSizeF = Comm.getSize(strstyle1[1]);

                    strtem = strstyle1[2];//Line|颜色|粗细|虚实线|[10,10][10,100]§Line|颜色|粗细|虚实线|[10,10][10,100]
                    //开始取得组成部分
                    strstyle2 = strtem.Split('§');
                    _listctimagecom = new List<Ct_ImagesCom>();
                    _listctimagecomsome = new List<Ct_ImagesCom>();

                    GetPicInfo(strstyle2, true);
                    if (strstyle1.Length > 3)
                    {
                        GetPicInfo(strstyle1[3].Split('§'), false);
                    }
                    _ctimages.ListCom = _listctimagecom;
                    _ctimages.ListComSome = _listctimagecomsome;
                    _ctimageitem.Add(_ctimages.ImgName, _ctimages);

                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 取得配置图标的信息  保存到对应的数据结构中
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="Thumbnails"></param>
        private void GetPicInfo(string[] strValue, bool IsThumbnails)
        {

            try
            {
                string[] strstyle2 = strValue;

                string[] arrStr;
                foreach (string str in strstyle2)
                {
                    if (str.Trim().Equals(""))
                    {
                        break;
                    }

                    _ctimagecom = new Ct_ImagesCom();
                    arrStr = str.Split('|');
                    _ctimagecom.ImgType = arrStr[0];

                    //这个是线条
                    if (_ctimagecom.ImgType.ToLower().Equals("line"))
                    {
                        //Line|Red|0.5|dash|[0,15][29,15]
                        _ctimagecom.ImgColor = Color.FromName(arrStr[1]);

                        //_ctimagecom.ImgWidth = arrStr[2];
                        if (!float.TryParse(arrStr[2], out _ctimagecom.ImgWidth))
                        {
                            _ctimagecom.ImgWidth = 1;
                        }

                        _ctimagecom.ImgDash = arrStr[3].ToLower();
                        _ctimagecom.ImgSPointF = Comm.getPoint(arrStr[4].Split(']')[0].Replace("[", "").Replace("]", ""));
                        _ctimagecom.ImgEPointF = Comm.getPoint(arrStr[4].Split(']')[1].Replace("[", "").Replace("]", ""));

                        if (!bool.TryParse(arrStr[5].Split(',')[0], out _ctimagecom.IsVertical))
                        {
                            _ctimagecom.IsVertical = false;
                        }

                        // _ctimagecom.IsVertical = arrStr[5].Split(',')[0].ToLower() == "false" ? false : true;
                        _ctimagecom.TitleNum = int.Parse(arrStr[5].Split(',')[1]);
                        _ctimagecom.TitleDistance = int.Parse(arrStr[5].Split(',')[2]);

                    }
                    //字符
                    else if (_ctimagecom.ImgType.ToLower().Equals("string"))
                    {
                        //Value|Red|宋体,0.5|value|0,15
                        _ctimagecom.ImgColor = Color.FromName(arrStr[1]);
                        _ctimagecom.ImgDash = arrStr[2];
                        _ctimagecom.ImgValue = arrStr[3];
                        _ctimagecom.ImgSPointF = Comm.getPoint(arrStr[4]);

                    }
                    //圆圈
                    else if (_ctimagecom.ImgType.ToLower().Equals("ellipse"))
                    {   //------------------------------位置  大小
                        // ¤图标五@30,30@Ellipse|Red|0.5|dash|[20,20][10,10]
                        _ctimagecom.ImgColor = _ctimagecom.ImgColor = Color.FromName(arrStr[1]);

                        //  _ctimagecom.ImgWidth = arrStr[2];
                        if (!float.TryParse(arrStr[2], out _ctimagecom.ImgWidth))
                        {
                            _ctimagecom.ImgWidth = 1;
                        }
                       
                        _ctimagecom.ImgDash = arrStr[3].ToLower();
                        //当是圆形的时候  我们采用 开始位置记录位置
                        _ctimagecom.ImgSPointF = Comm.getPoint(arrStr[4].Split(']')[0].Replace("[", "").Replace("]", ""));
                        //当时圆形的时候  我们采用结束位置记录圆形的高宽
                        _ctimagecom.ImgEPointF = Comm.getPoint(arrStr[4].Split(']')[1].Replace("[", "").Replace("]", ""));

                    }

                    //大图标还是小图标
                    if (IsThumbnails)
                    {
                        _listctimagecom.Add(_ctimagecom);
                    }
                    else
                    {
                        _listctimagecomsome.Add(_ctimagecom);
                    }
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex.ToString());
                throw ex;
            }
        }
        #endregion


        #region 透明处理
        ///// <summary>
        ///// 透明处理  
        /////  需要做两个工作 
        /////  picbox.BackColor = Color.Transparent; 
        /////  g.SmoothingMode = SmoothingMode.AntiAlias; 不加会出现虚线  不知道原理  这属性是用在画弧线和斜线时消除锯齿的 
        ///// </summary>
        ///// <param name="img"></param>
        ///// <returns></returns>
        //private unsafe static GraphicsPath subGraphicsPath(Image img)
        //{
        //    if (img == null) return null;

        //    // 建立GraphicsPath, 给我们的位图路径计算使用   
        //    GraphicsPath g = new GraphicsPath(FillMode.Alternate);

        //    Bitmap bitmap = new Bitmap(img);

        //    int width = bitmap.Width;
        //    int height = bitmap.Height;
        //    //BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
        //    BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        //    byte* p = (byte*)bmData.Scan0;
        //    int offset = bmData.Stride - width * 3;
        //    int p0, p1, p2;         // 记录左上角0，0座标的颜色值  
        //    p0 = p[0];
        //    p1 = p[1];
        //    p2 = p[2];

        //    int start = -1;
        //    // 行座标 ( Y col )   
        //    for (int Y = 0; Y < height; Y++)
        //    {
        //        // 列座标 ( X row )   
        //        for (int X = 0; X < width; X++)
        //        {
        //            if (start == -1 && (p[0] != p0 || p[1] != p1 || p[2] != p2))     //如果 之前的点没有不透明 且 不透明   
        //            {
        //                start = X;                            //记录这个点  
        //            }
        //            else if (start > -1 && (p[0] == p0 && p[1] == p1 && p[2] == p2))      //如果 之前的点是不透明 且 透明  
        //            {
        //                g.AddRectangle(new Rectangle(start, Y, X - start, 1));    //添加之前的矩形到  
        //                start = -1;
        //            }

        //            if (X == width - 1 && start > -1)        //如果 之前的点是不透明 且 是最后一个点  
        //            {
        //                g.AddRectangle(new Rectangle(start, Y, X - start + 1, 1));      //添加之前的矩形到  
        //                start = -1;
        //            }
        //            //if (p[0] != p0 || p[1] != p1 || p[2] != p2)  
        //            //    g.AddRectangle(new Rectangle(X, Y, 1, 1));  
        //            p += 3;                                   //下一个内存地址  
        //        }
        //        p += offset;
        //    }

        //    bitmap.UnlockBits(bmData);
        //    bitmap.Dispose();

        //    return g;
        //}

        ///// <summary>  
        ///// 调用此函数后使图片透明  
        ///// </summary>  
        ///// <param name="control">需要处理的控件</param>  
        ///// <param name="img">控件的背景或图片，如PictureBox.Image  
        /////   或PictureBox.BackgroundImage</param>  
        //public static void ControlTrans(Control control, Image img)
        //{
        //    GraphicsPath g;
        //    g = subGraphicsPath(img);

        //    if (g == null)
        //    {
        //        return;
        //    }

        //    control.Region = new Region(g);
        //}
        #endregion
    }
}
