using System.Windows.Forms;
namespace SparkFormEditor
{
    partial class RichTextBoxExtended
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.ContextMenuStrip linecmenu;
        private ContextMenuStrip contextMenuStripSelectItem;

        private ToolStripMenuItem toolStripMenuItemKnowledg;        //知识库
        private ToolStripMenuItem menuItem13;                       //特殊字符
        private ToolStripMenuItem toolStripMenuItemSelectItem;      //选择项

        private System.Windows.Forms.ToolStripMenuItem menuItem6;
        private System.Windows.Forms.ToolStripMenuItem menuItem7;
        private System.Windows.Forms.ToolStripMenuItem menuItem8;
        private System.Windows.Forms.ToolStripMenuItem menuItem11;
        private System.Windows.Forms.ToolStripMenuItem menuItem12;

        private ToolStripMenuItem toolStripMenuItemUndo;
        private ToolStripMenuItem toolStripMenuItemRedo;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem toolStripMenuItemInfor;

        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem toolStripMenuItemUp;
        private ToolStripMenuItem toolStripMenuItemDown;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem toolStripMenuItemClearFormat;

        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem tsmiDate_YMD;
        private ToolStripMenuItem tsmiDate_MD;
        private ToolStripMenuItem tsmiDate_D;
        private ToolStripSeparator toolStripSeparatorDate;
        private ToolStripMenuItem tsmiShowCalendar;
        private ToolStripSeparator toolStripSeparatorCalendar;
        private ToolStripMenuItem toolStripMenuItemParagraphContent;
        private ToolStripMenuItem toolStripMenuItemSumSmall;
        private ToolStripMenuItem toolStripMenuItemSumBig;
        private ToolStripMenuItem toolStripMenuItemAutoPositionByTime;


        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            this.TextChanged -= new System.EventHandler(this.RichTextBoxExtended_TextChanged);
            

            if (disposing && (components != null))
            {
                components.Dispose();

                _Default = null; //默认值
                _Score = null; //评分表达式
                Sum = null; //非表格输入框的合计公式
                CreateUser = null;  //创建用户
                OldRtf = null; //是否需要保存用
                OldText = null; //值改变用 ,目前用于行文字根据时间自动变色。但是自动赋默认值的时候，之后要重置OldText为空，否则不会触发。
                _EditHistory = null;
                _AttachForm = null;
                _YMD = null; //默认为：完整年月日yyyy-MM-dd，月日M-dd，日d .TrimStart('0')
                _YMD_Default = null;
                _YMD_Old = null;
                _FullDateText = null;
                _CheckRegex = null;
                GroupName = null;
                _HelpString = null;

                //System.GC.Collect();
            }
            base.Dispose(disposing);
        }


        //void ClearEvent(Control control, string eventname)
        // {
        // if (control == null) return;
        // if (string.IsNullOrEmpty(eventname)) return;
        // BindingFlags mPropertyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;
        // BindingFlags mFieldFlags = BindingFlags.Static | BindingFlags.NonPublic;
        // Type controlType = typeof(System.Windows.Forms.Control);
        // PropertyInfo propertyInfo = controlType.GetProperty("Events", mPropertyFlags);
        // EventHandlerList eventHandlerList = (EventHandlerList)propertyInfo.GetValue(control, null);
        // FieldInfo fieldInfo = (typeof(Control)).GetField(nameof(EntXmlModel.Event) + eventname, mFieldFlags);
        // Delegate d = eventHandlerList[fieldInfo.GetValue(control)];
        // if (d == null) return;
        // EventInfo eventInfo=controlType.GetEvent(eventname);
        // foreach (Delegate dx in d.GetInvocationList())
        // eventInfo.RemoveEventHandler(control, dx);
        // }
        //调用案例：
        //ClearEvent(button1,"Click");//就会清除button1对象的Click事件的所有挂接事件。 

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RichTextBoxExtended));

            // 
            // RichTextBoxExtended
            // 
            this.ContextMenuStrip = this.linecmenu;
            this.Size = new System.Drawing.Size(504, 224);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RichTextBoxExtended_KeyDown);
            this.VScroll += new System.EventHandler(this.RichTextBoxExtended_VScroll);
            this.Enter += new System.EventHandler(this.rtb1_Enter);
            this.DoubleClick += new System.EventHandler(this.RichTextBoxExtended_DoubleClick);
            this.Leave += new System.EventHandler(this.RichTextBoxExtended_Leave);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxExtended_MouseDown);
            this.TextChanged += new System.EventHandler(this.RichTextBoxExtended_TextChanged);
            this.ResumeLayout(false);

        }

        #endregion









    }
}
