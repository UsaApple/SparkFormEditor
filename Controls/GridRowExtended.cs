/* 新表单
 * *************************************************************
 无锡 曼荼罗 谈俊琪 2013年6月28日 -2013年8月8日
 * 
 * 
 * 暂时废弃，老的实现方式的表格 行控件
 * 
 *  
 * *************************************************************
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Collections;

//新表单 自定义行控件 by 谈俊琪  2013年
namespace Mandala.Recruit3
{
    //[Serializable]
    public partial class GridRowExtended : UserControl
    {
        Recruit3Chart _Owner;

        //XmlNode _NodeSetting;
        private Point _Location = new Point(1,1);
        private Size _AllSize = new Size(0, 0);

        XmlNode xnlEach;
        RichTextBoxExtended rebeNew;
        TransparentRTBExtended rtbeNewTransparent;

        Font eachFont = new Font("宋体", 8F, FontStyle.Regular);
        Pen eachPen = new Pen(Color.Black, 1);

        int tempInt;
        HorizontalAlignment inputBoxAlignment;

        int _Rowindex = 0;

        public GridRowExtended()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public GridRowExtended(XmlNode nodeSetting, Recruit3Chart Owner ,int Rowindex)
        {
            InitializeComponent();

            _Owner = Owner;

            _Rowindex = Rowindex;

            this.BorderStyle = BorderStyle.None;

            //_NodeSetting = nodeSetting;

            this.SuspendLayout();

            InitGridRowColumns(nodeSetting);

            this.ResumeLayout();

            this.Size = _AllSize;

           
        }

        //控件透明化
        //override protected CreateParams CreateParams { get { CreateParams cp = base.CreateParams; cp.ExStyle |= 0x20; return cp; } }

        //override protected void OnPaintBackground(PaintEventArgs e)
        //{
        //    //base.OnPaintBackground(e);//最后执行父类的绘制时间
        //}

        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    //base.OnPaint(e);
        //}

        private void InitGridRowColumns(XmlNode nodeRowSetting)
        {
            //根据配置文件来进行初始化：本行
            //this.BorderStyle = BorderStyle.FixedSingle; //调试用

            int index = 1;
            //顺序读取详细配置信息的每个节点，进行设置
            for (int i = 0; i < nodeRowSetting.ChildNodes.Count; i++)
            {
                //如果是注释那么跳过
                if (nodeRowSetting.ChildNodes[i].Name == @"#comment" || nodeRowSetting.ChildNodes[i].Name == @"#text")
                {
                    continue;
                }

                 xnlEach = nodeRowSetting.ChildNodes[i];

                //属性包含了下拉框中的所有属性：
                //<GridRow Name="表格" Size="120,25" PointStart="85,140" Width="宋体,9" Style="">  
                //<Column Name="日期" Size="150,25" ReadOnly="True" Color="Red" Width="宋体,9" Alignment="Left" MaxLength="20" BackGround="Transparent"/>
                //   ……
                //</GridRow>

                 if ((xnlEach as XmlElement).GetAttribute("BackGround").ToLower() == "transparent")
                 {
                     rtbeNewTransparent = new TransparentRTBExtended();
                     rtbeNewTransparent.SuspendLayout();
                     IntitColumn(xnlEach, rtbeNewTransparent);
                     this.Controls.Add(rtbeNewTransparent);
                     rtbeNewTransparent.ResumeLayout();
                 }
                 else
                 {
                     rebeNew = new RichTextBoxExtended();
                     //rebeNew = CreatRTBE(index);
                     index++;
                     rebeNew.SuspendLayout();
                     IntitColumn(xnlEach, rebeNew);
                     this.Controls.Add(rebeNew);
                     rebeNew.ResumeLayout();
                 }

                 //if (_Owner != null)
                 //{
                 //    _Owner.ShowInforMsg("单元格：" + i.ToString() + "____" + string.Format("{0:yyyy-MM-dd HH:mm:ss:ffff}", DateTime.Now)); //调试，查看加载时间
                 //}
            }

        }

        //private RichTextBoxExtended CreatRTBE(int index)
        //{
        //    RichTextBoxExtended ret;

        //    if (index <= 20)
        //    {
        //        ret = (RichTextBoxExtended)ControlFromName("r" + index.ToString());
        //    }
        //    else
        //    {
        //        ret = new RichTextBoxExtended();
        //    }

        //    return ret;
        //}

        ///// <summary>根据名字取得控件（不是动态添加的）
        /////根据名字取得控件 -- 不是动态添加的控件
        ///// </summary>
        //private Control ControlFromName(string name)
        //{
        //    if (this.Controls.ContainsKey(name))
        //    {
        //        object o = this.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
        //        return ((Control)o);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        private void IntitColumn(XmlNode xnlEach, RichTextBoxExtended rtbeNew)
        {

            if ((xnlEach as XmlElement).GetAttribute("Alignment").ToLower() == "left")
            {
                inputBoxAlignment = HorizontalAlignment.Left;
            }
            else if ((xnlEach as XmlElement).GetAttribute("Alignment").ToLower() == "right")
            {
                inputBoxAlignment = HorizontalAlignment.Right;
            }
            else
            {
                inputBoxAlignment = HorizontalAlignment.Center;//默认居中
            }

            eachFont = Recruit3Chart.getFont((xnlEach as XmlElement).GetAttribute("Width"));

            rtbeNew._InputBoxAlignment = inputBoxAlignment;
            rtbeNew.SetAlignment();

            rtbeNew.Name = (xnlEach as XmlElement).GetAttribute("Name");
            
            rtbeNew.ForeColor = Color.FromName((xnlEach as XmlElement).GetAttribute("Color"));
            rtbeNew.Font = eachFont;
            rtbeNew.Size = Recruit3Chart.getSize((xnlEach as XmlElement).GetAttribute("Size")).ToSize();

            //rtbeNew.BackColor = Color.Blue; //调式用，凸显颜色
            //this.BackColor = Color.DarkGray;//调式用，凸显颜色

            //位置要累加
            rtbeNew.Location = new Point(_Location.X, _Location.Y); //横线坐标位置位置累加：上个控件的宽度 再+1

            _Location = new Point(_Location.X + rtbeNew.Size.Width + 1, _Location.Y);

            if (rtbeNew.Size.Height > _AllSize.Height)
            {
                _AllSize = new Size(_Location.X, rtbeNew.Size.Height + 1);
            }

            rtbeNew.Default = (xnlEach as XmlElement).GetAttribute("Default") + _Rowindex.ToString();
            rtbeNew.Score = (xnlEach as XmlElement).GetAttribute("Score");

            //知识库
            rtbeNew.SetKnowledg(Recruit3Chart._node_Knowledg,null);

            rtbeNew.SetSelectWords(Recruit3Chart.GetSelectWord((xnlEach as XmlElement).GetAttribute("InputBoxSelectWord"))); //右击菜单的插入文字

            if (!int.TryParse((xnlEach as XmlElement).GetAttribute("MaxLength"), out tempInt))
            {
                tempInt = 20;
            }

            rtbeNew.MaxLength = tempInt;

            //initRtbe(rtbeNew);
            rtbeNew.AutoWordSelection = true;
            rtbeNew.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtbeNew.Multiline = false;
            rtbeNew.WordWrap = false;
            rtbeNew.ImeMode = ImeMode.On;
            rtbeNew.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;

            //事件处理：
            rtbeNew.TextChanged += new System.EventHandler(Script_TextChanged_ME);               //执行脚本
            rtbeNew.Enter += new EventHandler(SetAssistantControl_Enter_ME);                          //光标进入后，设置为输入法对象
            rtbeNew.SelectionChanged += new EventHandler(UpdateToolBarFontStyle_SelectionChanged_ME); //选择文字改变后，将选中文字的样式更新到工具栏
        }

        public event EventHandler Script_TextChanged; //调用主体通用事件来处理
        public event EventHandler SetAssistantControl_Enter;
        public event EventHandler UpdateToolBarFontStyle_SelectionChanged;

        private void Script_TextChanged_ME(object sender, EventArgs e)
        {
            Script_TextChanged(sender,e);
        }

        private void SetAssistantControl_Enter_ME(object sender, EventArgs e)
        {
            SetAssistantControl_Enter(sender, e);
        }

        private void UpdateToolBarFontStyle_SelectionChanged_ME(object sender, EventArgs e)
        {
            UpdateToolBarFontStyle_SelectionChanged(sender, e);
        }


        ////克隆分为深度克隆和浅度克隆
        ////深度克隆:会克隆当前实例的所有所有成员.
        ////浅度克隆:只会克隆当前实例的所有值类型的.
        //public object Clone()
        //{
        //    try
        //    {
        //        return MemberwiseClone();//浅度克隆
        //        //return this.Clone();
        //    }
        //    catch (Exception ex)
        //    {
        //        Recruit3Log.AddLog(ex);
        //        throw ex;
        //    }
        //}

        //public GridRowExtended Clone()
        //{
        //    MemoryStream ms = new MemoryStream();
        //    BinaryFormatter bf = new BinaryFormatter();

        //    bf.Serialize(ms, this);

        //    ms.Seek(0, SeekOrigin.Begin);

        //    return (GridRowExtended)bf.Deserialize(ms);
        //}

        //public GridRowExtended DeepClone()
        //{
        //    MemoryStream ms = new MemoryStream();
        //    BinaryFormatter bf = new BinaryFormatter();
        //    bf.Serialize(ms, this);
        //    ms.Position = 0;
        //    return bf.Deserialize(ms) as GridRowExtended;
        //}

        //这个方法就可以实现克隆了
        //这个方法是利用序列化和反序列化来实现克隆 比较方便但是类必须用[Serializable]标记可以序列化  
        ////另外一种方式:
        //public Person Clone()
        //{

        //   Person temp = new Person();
        //   temp.Name = this.Name;
        //   temp.Age = this.Age;
        //   temp.Address.Province = this.Address.Province;
        //   temp.Address.City = this.Address.City;
        //   return temp;
        // }

        /// <summary>    
        /// 克隆对象，并返回一个已克隆对象的引用    
        /// </summary>    
        /// <returns>引用新的克隆对象</returns>     
        //public object Clone()
        //{
        //    //首先我们建立指定类型的一个实例         
        //    object newObject = Activator.CreateInstance(this.GetType());
        //    //我们取得新的类型实例的字段数组。         
        //    FieldInfo[] fields = newObject.GetType().GetFields();
        //    int i = 0;
        //    foreach (FieldInfo fi in this.GetType().GetFields())
        //    {
        //        //我们判断字段是否支持ICloneable接口。             
        //        Type ICloneType = fi.FieldType.
        //        GetInterface("ICloneable", true);
        //        if (ICloneType != null)
        //        {
        //            //取得对象的Icloneable接口。                 
        //            ICloneable IClone = (ICloneable)fi.GetValue(this);
        //            //我们使用克隆方法给字段设定新值。                
        //            fields[i].SetValue(newObject, IClone.Clone());
        //        }
        //        else
        //        {
        //            // 如果该字段部支持Icloneable接口，直接设置即可。                 
        //            fields[i].SetValue(newObject, fi.GetValue(this));
        //        }
        //        //现在我们检查该对象是否支持IEnumerable接口，如果支持，             
        //        //我们还需要枚举其所有项并检查他们是否支持IList 或 IDictionary 接口。            
        //        Type IEnumerableType = fi.FieldType.GetInterface("IEnumerable", true);
        //        if (IEnumerableType != null)
        //        {
        //            //取得该字段的IEnumerable接口                
        //            IEnumerable IEnum = (IEnumerable)fi.GetValue(this);
        //            //这个版本支持IList 或 IDictionary 接口来迭代集合。                
        //            Type IListType = fields[i].FieldType.GetInterface("IList", true);
        //            Type IDicType = fields[i].FieldType.GetInterface("IDictionary", true);
        //            int j = 0;
        //            if (IListType != null)
        //            {
        //                //取得IList接口。                     
        //                IList list = (IList)fields[i].GetValue(newObject);
        //                foreach (object obj in IEnum)
        //                {
        //                    //查看当前项是否支持支持ICloneable 接口。                         
        //                    ICloneType = obj.GetType().
        //                    GetInterface("ICloneable", true);
        //                    if (ICloneType != null)
        //                    {
        //                        //如果支持ICloneable 接口，    
        //                        //我们用它李设置列表中的对象的克隆    
        //                        ICloneable clone = (ICloneable)obj;
        //                        list[j] = clone.Clone();
        //                    }
        //                    //注意：如果列表中的项不支持ICloneable接口，那么                      
        //                    //在克隆列表的项将与原列表对应项相同                      
        //                    //（只要该类型是引用类型）                        
        //                    j++;
        //                }
        //            }
        //            else if (IDicType != null)
        //            {
        //                //取得IDictionary 接口                    
        //                IDictionary dic = (IDictionary)fields[i].
        //                GetValue(newObject);
        //                j = 0;
        //                foreach (DictionaryEntry de in IEnum)
        //                {
        //                    //查看当前项是否支持支持ICloneable 接口。                         
        //                    ICloneType = de.Value.GetType().
        //                    GetInterface("ICloneable", true);
        //                    if (ICloneType != null)
        //                    {
        //                        ICloneable clone = (ICloneable)de.Value;
        //                        dic[de.Key] = clone.Clone();
        //                    }
        //                    j++;
        //                }
        //            }
        //        }
        //        i++;
        //    }
        //    return newObject;
        //}

        ///// <summary>
        ///// BaseObject类是一个用来继承的抽象类。 
        ///// 每一个由此类继承而来的类将自动支持克隆方法。
        ///// 该类实现了Icloneable接口，并且每个从该对象继承而来的对象都将同样地
        ///// 支持Icloneable接口。 
        ///// </summary> 
        //public abstract class BaseObject : ICloneable
        //{
        //    /// <summary>    
        //    /// 克隆对象，并返回一个已克隆对象的引用    
        //    /// </summary>    
        //    /// <returns>引用新的克隆对象</returns>     
        //    public object Clone()
        //    {
        //        //首先我们建立指定类型的一个实例         
        //        object newObject = Activator.CreateInstance(this.GetType());
        //        //我们取得新的类型实例的字段数组。         
        //        FieldInfo[] fields = newObject.GetType().GetFields();
        //        int i = 0;
        //        foreach (FieldInfo fi in this.GetType().GetFields())
        //        {
        //            //我们判断字段是否支持ICloneable接口。             
        //            Type ICloneType = fi.FieldType.
        //            GetInterface("ICloneable", true);
        //            if (ICloneType != null)
        //            {
        //                //取得对象的Icloneable接口。                 
        //                ICloneable IClone = (ICloneable)fi.GetValue(this);
        //                //我们使用克隆方法给字段设定新值。                
        //                fields[i].SetValue(newObject, IClone.Clone());
        //            }
        //            else
        //            {
        //                // 如果该字段部支持Icloneable接口，直接设置即可。                 
        //                fields[i].SetValue(newObject, fi.GetValue(this));
        //            }
        //            //现在我们检查该对象是否支持IEnumerable接口，如果支持，             
        //            //我们还需要枚举其所有项并检查他们是否支持IList 或 IDictionary 接口。            
        //            Type IEnumerableType = fi.FieldType.GetInterface("IEnumerable", true);
        //            if (IEnumerableType != null)
        //            {
        //                //取得该字段的IEnumerable接口                
        //                IEnumerable IEnum = (IEnumerable)fi.GetValue(this);
        //                //这个版本支持IList 或 IDictionary 接口来迭代集合。                
        //                Type IListType = fields[i].FieldType.GetInterface("IList", true);
        //                Type IDicType = fields[i].FieldType.GetInterface("IDictionary", true);
        //                int j = 0;
        //                if (IListType != null)
        //                {
        //                    //取得IList接口。                     
        //                    IList list = (IList)fields[i].GetValue(newObject);
        //                    foreach (object obj in IEnum)
        //                    {
        //                        //查看当前项是否支持支持ICloneable 接口。                         
        //                        ICloneType = obj.GetType().
        //                        GetInterface("ICloneable", true);
        //                        if (ICloneType != null)
        //                        {
        //                            //如果支持ICloneable 接口，    
        //                            //我们用它李设置列表中的对象的克隆    
        //                            ICloneable clone = (ICloneable)obj;
        //                            list[j] = clone.Clone();
        //                        }
        //                        //注意：如果列表中的项不支持ICloneable接口，那么                      
        //                        //在克隆列表的项将与原列表对应项相同                      
        //                        //（只要该类型是引用类型）                        
        //                        j++;
        //                    }
        //                }
        //                else if (IDicType != null)
        //                {
        //                    //取得IDictionary 接口                    
        //                    IDictionary dic = (IDictionary)fields[i].
        //                    GetValue(newObject);
        //                    j = 0;
        //                    foreach (DictionaryEntry de in IEnum)
        //                    {
        //                        //查看当前项是否支持支持ICloneable 接口。                         
        //                        ICloneType = de.Value.GetType().
        //                        GetInterface("ICloneable", true);
        //                        if (ICloneType != null)
        //                        {
        //                            ICloneable clone = (ICloneable)de.Value;
        //                            dic[de.Key] = clone.Clone();
        //                        }
        //                        j++;
        //                    }
        //                }
        //            }
        //            i++;
        //        }
        //        return newObject;
        //    }
        //}

    }



}
