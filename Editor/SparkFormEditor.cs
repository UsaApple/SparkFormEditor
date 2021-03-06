using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Xml;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Net;
using System.Xml.XPath;
using SparkFormEditor.Model;
using System.Threading.Tasks;
using System.Linq;
using SparkFormEditor.CA;

namespace SparkFormEditor
{
    ///<summary>
    ///新表单：主界面
    ///</summary>
    public partial class SparkFormEditor : UserControl
    {
        # region 全局变量定义
        protected EditorTypeEnum _editorTypeEnum = EditorTypeEnum.护士;
        protected FormTypeEnum _formTypeEnum = FormTypeEnum.住院;

        /// <summary>
        /// 表单运行模式：单机版为False，否则True
        /// </summary>
        private bool RunMode = false;
        private bool _Preview = false;                              //CA签名记录查询的时候预览
        private string m_formSettingsVerifySignID = "";              //上一次签名的工号，历史信息

        private bool toolStripPrintEnable = true;                   //toolStripPrint toolStripButtonPrintAll toolStripButton续打是否有权限打印
        private DataTable _PatientsInforDT = null;      //病人列表
        private XmlDocument _TemplateManageXml = null;         //当前表单模板的xml配置的管理文件

        private bool xmlMode = true;                                  //模板管理文件配置方式：true节点名，false为Name属性来制定，科室/病区中有括号等特殊符号

        /// <summary>
        /// 当前表单模板的xml配置文件
        /// </summary>
        private XmlDocument _TemplateXml;
        private XmlNode _node_Page;                            //表单模板中本页的配置节点
        private XmlNode _node_Script;                          //表单模板中配置的联动脚本



        private bool _OwnerTableHaveMultilineCell = false;
        private bool _TableType = false;                      //表单样式：true表格，false非表格
        private int _PageRows = 50;                           //表格样式的每页行数
        private int _PageRowsFristPage = 50;                  //表格样式的第一页的行数
        private int _PageRowsSecondPage = 50;                  //表格样式的第一页的行数
        private int _PageRowsHeight = 25;                     //表格样式的行高
        private int _PageRowsWidth = 0;                       //表格样式的行宽度，要根据配置累加计算得到
        private Point _TableLocation = new Point(0, 0);       //表格的坐标位置
        private string _NurseLevel = "";                      //该表单对应的医生/护士，权限等级

        //存储当前表格的所有数据信息（输入框信息）
        private TableInfor _TableInfor = null;                        //行、列、单元格
        private Point _CurrentCellIndex = new Point(-1, -1);  //记录下当前操作的单元格

        private string _CurrentTemplateXml_Path = "";   //当前表单模板的xml配置文件的路径
        private Point _OriginPoint = new Point(0, 0);
        private bool _PrintType = false;                       //打印类型：false为纵向，True为横向
        private Bitmap _BmpBack = null;                        //表单 背景图
        private Control _Need_Assistant_Control = null;
        private Control _PageNo = null;

        private static XmlNode _node_Knowledg = null;          //知识库节点，目前也计划配置到输入助理同一个个性化配置文件中的，另外一个节点。
        private XmlNode _node_Assistant = null;

        //======当前操作病人、表单、页数信息
        private string _CurrentChartYear = "";                 //每个病人所有的表单都在一个年度表中
        private string _PreShowedPage = "";                    //本页显示前的页数


        /// <summary>
        /// 模板ID
        /// </summary>
        private string _TemplateID = "";
        private string _TemplateRight = "2";                   //表单权限：0医生，1护士，2共享表单
        private string _TemplateUpdate = "";                  //更新表单时，再次进行排他处理，防止被秒级同时打开的重复覆盖数据

        private TreeNode _CurrentPatientTreeNode = null;

        private static string _HelpString = "";                  //全局的输入框右击菜单配置
        private static string _RowHeaderIndent = "";                     //调整段落列 行头缩进
        private string _PatientInforOrder = "";

        //RowForeColor="7:00~19:00@Blue¤19:01~6:59@Red" -> 7:00~19:00@Blue¤19:01~23:59@Red¤00:00~6:59@Red
        //表格式护理记录单，要求某个时段用蓝色，某个时段用红色字体书写
        private string _RowForeColor = "";
        private List<RowForeColors> _RowForeColorsList = new List<RowForeColors>();

        //生成图片的时候 -- 按上述处理，位置会错乱 位置会不对，很奇怪
        private bool _isCreatImages = false;                            //True是生成图片，Flase为打印的场合       

        private PictureBox _picTransEffect = null;                              //过渡效果,请等待提示

        private string _StrXmlData = "";                                //数据字符串，如果改格式已经不正确，那么load成xml文档就会报错，暗门导出的就是空文件了。如果文件为空就导出这个。
        /// <summary>
        /// 表单数据对象XMl文件
        /// </summary>
        private XmlDocument _RecruitXML = null;
        private XmlDocument _RecruitXML_Org = null;                     //表单数据对象XMl文件，修改前源文件  ； Xml复制：_RecruitXML_Org[nameof(EntXmlModel.NurseForm)].InnerXml = _RecruitXML[nameof(EntXmlModel.NurseForm)].InnerXml;   
        private static string _RecruitXMLPATH_Data = null;               //表单数据文件的本地客户端路径

        private bool _IsCreating = false;                               //表示正在添加页：正在添加的时候，更新空的基本信息，无需提示。
        private bool _IsCreated = false;                                //创建表单，或者点击新建页，或者调整段落列增加到下一页，都会促发这个属性为True
        private bool _IsCreatedTemplate = false;                        //是否为创建表单，并且还没有保存。用来控制是insert还是update
        private string _CreatTemplatePara = "";                         //自动创建表单时候，需要传值的参数，自动带入现实到创建页中
        private TreeNode _CreatTreeNode = null;                         //在没有保存的时候，记录下来后，可以删除，避免界面混乱
        private Dictionary<string, string> _CurrentBasicInfor = new Dictionary<string, string>();
        //切换页和表单，关闭的时候，提示是否需要保存。
        private bool _CurrentCellNeedSave = false;                      //当前单元格在添加行线等情况下，是否需要进行保存
        private bool _IsNotNeedReloadTemplate = false;                  //先做第一条件判断：同一病人，同一模板名称，并且所属的模板页一致；就不需要更新模板样式(重新加载和绘制地图，一样只要重置控件默认值)
        private bool _IsNotNeedReloadData = false;                      //判断上一次的uhid和模板一致，那么就不需要再次加载数据

        //日期时间格式化标准 yyyy-MM-dd HH:mm，方便以后扩展成属性定制格式样式
        private string DateFormat_YMD = "yyyy-MM-dd";
        private static string DateFormat_HM = "HH:mm";                    //hh:mm 12小时，HH24小时制，一个H是不会补零的（H:mm）。服务端统一配置的。dateFormat可以每个输入框或者列进行设置。时间列的格式是服务端统一设置的。
        private List<string> _ErrorList = new List<string>();           //输入框错误内容：ID名字：msg
        private DataServiceSetting _DSS;

        //键盘输入，智能提示，补全
        AutocompleteMenu autocompleteMenuForWrite;
        //存储本模板页中，配置的病人基本信息
        private List<LabelExtended> _BaseInforLblnameArr = new List<LabelExtended>();
        private List<LabelExtended> _BaseArrForPrintAllPageUpdate = new List<LabelExtended>();
        private string _SignTag = "→";          // 体现转科、转床、转病区的标记符号
        private List<RichTextBoxExtended> _TableHeaderList = new List<RichTextBoxExtended>();

        //-------------------锁定表单的信息：标记，对应表单的uhid，表单名，年度----------------------
        //True：这个是被其他人锁定了的意思；False表示被自己锁定了。只要打开表单，这条记录肯定被锁住了。
        internal bool _IsLocked = false;          //每次打开表单，需要判断，该表单是不是已经被其他人打开锁定，那么就不能编辑了。
        //-------------------锁定表单的信息：标记，对应表单的uhid，表单名，年度----------------------

        //private MessageBalloon m_mb;           //修订历史的气泡提示：会招致内存泄露，频繁操作，再现率不高，可能和内部线程释放有关系
        private string _LocalIP = "";            //本机IP地址，锁定表单的时候记录用
        private Dictionary<string, Bitmap> _ServerImages = new Dictionary<string, Bitmap>(); //图片名字，图片内容
        private bool _PrintAddLog = false;       //暗门开启写打印日志的标记，Ctrl + A
        private int _DragDrawRectMode = 0;       //暗门开启拖拽是绘制矩形框标记，Ctrl + 0，关闭；Ctrl + 1，临时会消失；Ctrl + 2，一直显示的，累加的

        //复制多个单元格：按住Ctrl ，然后点击单元格表示进行选择单元格，进行多个复制等操作，这个进入不显示输入框
        private bool _IS_CTRL_KEYDOWN = false;  //按下Ctrl生效，释放就设置为false
        private bool _IS_SHIFT_KEYDOWN = false; //按下Ctrl生效，释放就设置为false
        private List<string> _SelectedCells = new List<string>();
        //复制多个单元格：

        //右击工具栏，定制显示那些项目的菜单

        //存储所有的绘制双红线的输入框，必要时要全部刷新
        private List<RichTextBoxExtended> _ListDoubleLineInputBox = new List<RichTextBoxExtended>();

        //行线，双红线自定义样式
        private string _RowLineType = "Red,1";
        private Color _RowLineType_Color = Color.Red;           //配置的双红线，默认颜色
        //private Color _RowLineType_Color_Selected = Color.Red;  //菜单选择的默认双红线颜色
        private int _RowLineType_Width = 1;

        private bool _SignNeedConfirmMsg = true;    //双击签名是否需要确认消息框，有些ICU的表单所有列都签名，不需要弹出
        private bool _FirstRowMustDate = true;      //表格的第一行，如果有日期和时间列，那么都不能为空。默认为True开始改属性
        private bool _SignIgnoreLevel = false;      //双签名是否忽略用户等级。

        //手摸签名的开关
        private bool _FingerPrintSign = false;
        private Dictionary<string, Image> _FingerPrintSignImages = new Dictionary<string, Image>();
        private Pen _PageEndCrossLinePen = new Pen(Color.Red, 1);               //页尾斜线的画笔定义：颜色和粗细
        private Point _GoOnPrintLocation = new Point(-1, -1);                   //非表格，续打开始位置。往上，往左的不再打印。
        private bool _NurseSubmit_Enable = true;                                 //当前病人是否有效无效编辑
        /// <summary>
        /// 转科科别权限判断
        /// </summary>
        private string _NurseType = "";

        //护士A写了记录后未关闭程序，护士B继续填写后不修改签名，会存在签名护士与实际填写表单内容护士不一致的情况，存在责任管理风险
        //在每次签名时弹出用户身份验证或在保存的时候弹出用户身份验证否则无法保存。
        private bool _SignNeedLogin = false;            //每次签名，需要进行登录验证
        private bool _SaveNeedLogin = false;
        private XmlDocument _xmlDocSumGroup = null;
        string _CellValueDefault = "¤False¤False¤¤False¤False¤False¤¤Black¤"; //单元格内容，后面拼接时的丰富信息，一般创建的时候只要默认的这个就行了
        private bool _NotNeedCheckForSave = false;              //在某些切换页操作的时候，不需要进行保存确认的操作
        private static int _LocalBackSaveDays = 7;       //本地表单xml备份文件历史保留天数，过期的会登录就删除
        private string _SaveAutoSign = "";                      //保存时自动签名 SaveAutoSign="Start" End 是一个段落的开始行签名，还是一个段落的最后一行签名。一是开始行签名的

        //和调整段落列后，签名调整到最后一行是矛盾的。SetParagraphSign(
        private bool _SaveAutoSignLastRow = false;               //最后一行需要签名，哪怕段落没有结束，下页还继续写。
        private bool _IsSignLastRow = false;                     //签名的时候，标识是调整段落列后的，最后一页签名

        private bool _NeedSaveSuccessMsg = true;                //保存成功是否需要提示消息，一般的保存都需要提示，但是CA签名自动保存的话不需要提示的。
        private bool _IsCASignAutoSave = false;                 //如果CA签名时保存，并开启保存启动保存，那么签名输入框会再次签名混乱。


        //以前静态方法，调用打印类输入框类打印，在服务端或者多线程会报错
        private RichTextBoxPrinter _RichTextBoxPrinter = new RichTextBoxPrinter();

        //关闭主程序时，触发表单关闭的:如果验证通过就保存，如果没有验证通过就退出，不提示消息卡住导致不能关闭
        private bool _LordClosing = false;  //保存时需要登陆验证的不行的，不会自动保存

        private bool _SaveAutoSort = false; //SaveAutoSort="True" SaveAutoSort="True" AutoSortMode="1"
        private string _AutoSortMode = "";

        private string _SumSetting = "";  // "行线@BottomLine,Red¤结文字列@项目¤结文字集合@12小时小结|12小时总结|24小时总结¤过滤@mg|Mg";

        private Color _SelectRowBackColor = Color.Empty;



        //页码扩展，_PageNo标签的Format属性扩展0：页码，1：总页码 。 <Variable Name="_PageNo" Format="{0}/{1}"
        private string _PageNoFormat = "";
        private int _MaxPageForPrintAll = 0; //最后一页的物理页数
        private int _MaxPageForPrintAllShow = 0; //最后一页得到的要显示总页数（可能手动修改）

        //private bool _IsPrintingDraw = false;       //区分绘制的是有，是打印呢，界面绘制。

        //保存表单的时候，更新体温单，记录下修改的数据行即信息
        private List<RecordInor> _SynchronizeTemperatureRecordListRecordInor = new List<RecordInor>();
        private ArrayList _SynchronizeTemperatureRecordItems = new ArrayList();
        private string _DefaultTemperatureType = "";
        private EntPatientInfoEx _patientInfo;
        #endregion 全局变量定义

        #region internal
        internal string _CurrentPage = "";
        private bool _isNeedSave;
        internal bool IsNeedSave
        {
            get => _isNeedSave && this._IsLocked == false;
            set => _isNeedSave = value;
        }
        internal EventHandler CallBackUpdateDocumentPaging = null;    //回调主工程，更新页码
        internal EventHandler CallBackAddOutPutMsg = null;            //回调主工程，提示消息
        internal EventHandler CallBackUpdateDocumentDetailInfor = null;//更新分页控件的打印、签名、权限信息，进行提示用户
        internal EventHandler CallBackDeleteBook = null;                  //删除文书后，回调主窗体关闭标签页，和移除病人列表中的节点
        internal bool toolStripSaveEnabled = true;                   //toolStripSave.Enabled   是否有保存的权限（医生不能保存护士专有文书、归档等情况下不能保存）
        /// <summary>
        /// 模板名称
        /// </summary>
        internal string _TemplateName = "";
        internal TreeNode _CurrentTemplateNameTreeNode = null;           //当前打开的病人，对应的父节点；并非选择的节点（有可能单击没有打开呢）
        internal List<GlobalVariable.Template> TemplateList = new List<GlobalVariable.Template>();
        #endregion

        #region 对外开放
        /// <summary>
        /// 内部保存后，输入结果由外部保存数据库，并告诉内部成功失败，返回值true（成功）或false（失败）
        /// </summary>
        [Obsolete("请使用BeforeSave和AfterSave事件")]
        public event GlobalVariable.SaveEventHandler Save;

        /// <summary>
        /// 获取外部时间事件
        /// </summary>
        public event GlobalVariable.GetDateTime GetDateTime;

        /// <summary>
        /// 内部保存之前的事件
        /// </summary>
        public event GlobalVariable.BeforeSaveEventHandler BeforeSave;
        /// <summary>
        /// 内部保存后，输入结果由外部保存数据库，并告诉内部成功失败，返回值true（成功）或false（失败）
        /// </summary>
        public event GlobalVariable.AfterSaveEventHandler AfterSave;

        /// <summary>
        /// 打印之前的事件
        /// </summary>
        public event GlobalVariable.BeforePrintPageEventHandler BeforePrintPage;
        /// <summary>
        /// 打印完成的事件
        /// </summary>
        public event GlobalVariable.AfterPrintPageEventHandler AfterPrintPage;

        /// <summary>
        /// 删除页之前的事件
        /// </summary>
        public event GlobalVariable.BeforeDelPageEventHandler BeforeDelPage;
        /// <summary>
        /// 删除页之后的事件
        /// </summary>
        public event GlobalVariable.AfterDelPageEventHandler AfterDelPage;

        /// <summary>
        /// 删除全部页，也就是删除此表单内容
        /// </summary>
        public event GlobalVariable.DelFormEventHandler DeleteAllPage;

        /// <summary>
        /// 插入行之前的事件
        /// </summary>
        public event GlobalVariable.BeforeInsertRowEventHandler BeforeInsertRow;
        /// <summary>
        /// 插入行之后的事件
        /// </summary>
        public event GlobalVariable.AfterInsertRowEventHandler AfterInsertRow;

        /// <summary>
        /// 新建页之前的事件
        /// </summary>
        public event GlobalVariable.BeforeNewPageEventHandler BeforeNewPage;
        /// <summary>
        /// 新建页之后的事件
        /// </summary>
        public event GlobalVariable.AfterNewPageEventHandler AfterNewPage;

        /// <summary>
        /// 删除行之前的事件
        /// </summary>
        public event GlobalVariable.BeforeDelRowEventHandler BeforeDelRow;
        /// <summary>
        /// 删除行之后的事件
        /// </summary>
        public event GlobalVariable.AfterDelRowEventHandler AfterDelRow;

        /// <summary>
        /// 创建新表单
        /// </summary>
        public event GlobalVariable.NewFormEventHandler CreateNewForm;

        /// <summary>
        /// 用户验证事件
        /// </summary>
        public event GlobalVariable.LoginVerifyHandler LoginVerify;

        /// <summary>
        /// 解封申请
        /// </summary>
        public event GlobalVariable.ApplyNurseFormDelockHandler ApplyNurseFormDelock;

        /// <summary>
        /// 插入体温单数据
        /// </summary>
        public event GlobalVariable.InsertTcDataEventHandler InsertTcData;
        #endregion

        #region 构造方法
        /// <summary>
        /// 
        /// </summary>
        public SparkFormEditor()
        {
            InitializeComponent();

            //_FingerPrintSign = CaProvider.Default.IsStartESignature();
            if (_LocalIP == "")
            {
                _LocalIP = Comm.GetLocalIP();
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                toolStripButton1.Visible = true;
            }
            //画布拖动操作
            this.pictureBoxBackGround.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WorkArea_MouseMove);
            this.pictureBoxBackGround.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WorkArea_MouseDown);
            this.pictureBoxBackGround.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WorkArea_MouseUp);

            //if (_borderDragger == null || _borderDragger.IsDisposed)
            //{
            //    _borderDragger = new BorderDragger();
            //}
            //_borderDragger.FloorImageDone += new EventHandler(borderDragger_FloorImageDone);            //应用
            //_borderDragger.FloorPredefinedDone += new EventHandler(borderDragger_FloorPredefinedDone);  //复用

            _BaseArrForPrintAllPageUpdate.Add(new LabelExtended());

            //工具栏菜单事件注册
            this.toolStripButtonDelockApply.Click += ToolStripButtonDelockApply_Click;

            this.toolStripButton1.Click += (sender, e) =>
            {
                this.GetReportImage();
            };
        }



        Label _labelInfor = null; //权限提示
        private void InitRightInforLabel(string msgPara)
        {
            //要么不显示，显示后每一页都一样。因为归档和转科等权限每页都是一样的。
            if (_labelInfor == null)
            {
                _labelInfor = new Label();
                _labelInfor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
                _labelInfor.AutoSize = true;
                _labelInfor.BackColor = System.Drawing.Color.White;
                _labelInfor.ForeColor = Color.Red;
                _labelInfor.Location = new System.Drawing.Point(6, 6);
                _labelInfor.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                _labelInfor.Name = "labelInfor";
                _labelInfor.Size = new System.Drawing.Size(29, 12);
                _labelInfor.TabIndex = 3;
                _labelInfor.Font = new Font("宋体", 9);
            }

            if (!this.panelMain.Controls.Contains(_labelInfor))
            {
                this.panelMain.Controls.Add(_labelInfor);
            }

            _labelInfor.Location = new System.Drawing.Point(6, 6);//切换表单可能会位置错乱
            _labelInfor.Visible = true;
            _labelInfor.BringToFront();

            _labelInfor.Text = msgPara;

            ////切换文书和重置的时候，隐藏该控件即可
            //if (this.panelMain.Controls.Contains(_labelInfor))
            //{
            //    this.panelMain.Controls.Remove(_labelInfor);
            //}
        }

        #endregion 构造方法

        #region 清空、重置
        /// <summary>清空
        /// 关闭后再打开的时候，
        /// 清空表单界面，重置所有的控制变量和参数。
        /// </summary>
        internal void ClearMe()
        {
            _SynchronizeTemperatureRecordListRecordInor.Clear();

            //切换文书和重置的时候，隐藏该控件即可
            if (this.panelMain.Controls.Contains(_labelInfor))
            {
                this.panelMain.Controls.Remove(_labelInfor);
            }
            panelMain.VerticalScroll.Value = 0;
            panelMain.HorizontalScroll.Value = 0;
            //清空界面
            this.pictureBoxBackGround.Controls.Clear();

            if (this.pictureBoxBackGround.Image != null)
            {
                Graphics.FromImage(this.pictureBoxBackGround.Image).Clear(Color.White);
            }

            pictureBoxFrozen.Size = new Size(pictureBoxBackGround.Width, Frozen_Default_Height);

            //清空参数：当前表单设置的。重新打开也要清空
            ClearCurrentTemplate();

            _PrintAddLog = false;
            _DragDrawRectMode = 0;

            preRowIndex = -1;
            preColIndex = -1;

            MouseDownOldPoint = new PointF(-1.0F, -1.0F);
            MouseDownNewPoint = new PointF(-1.0F, -1.0F);

            _VectorList.Clear();

            _TableInfor = null;
            _TableType = false;
            _OwnerTableHaveMultilineCell = false;

            //_RowLineType_Color_Selected = _RowLineType_Color; //主窗体进行设置

            //锁定
            _IsLocked = false;

            _TemplateUpdate = "";

            IsNeedSave = false;
            _IsCreating = false;
            _IsCreated = false;
            _IsCreatedTemplate = false;

            _CurrentBasicInfor.Clear();

            _node_Page = null;

            _PreCellRowIndex = new Point(-1, -1);

            _ServerImages.Clear();

            if (RunMode)
            {
                BackUpHelp.DelOldBackUp();
            }

            SetAllControlDisabled(false);

            if (this.treeViewPatients != null && this.treeViewPatients.Nodes.Count > 0)
            {
                this.treeViewPatients.CollapseAll();
            }

            _CurrentPatientTreeNode = null;
            _CurrentTemplateNameTreeNode = null;    //当前打开的病人，对应的父节点

            _GoOnPrintLocation = new Point(-1, -1);
        }

        /// <summary>
        /// 清空表单设置，以便重新打开
        /// 清空关于当前表单的参数
        /// </summary>
        private void ClearCurrentTemplate()
        {
            //切换文书和重置的时候，隐藏该控件即可
            if (this.panelMain.Controls.Contains(_labelInfor))
            {
                this.panelMain.Controls.Remove(_labelInfor);
            }

            _CurrentChartYear = "";
            _CurrentPage = "";
            _PreShowedPage = "";                    //本页显示前的页数
            _TemplateName = "";
            _patientInfo = null;
            //toolStripStatusLabelCurrentPatient.Text = "";

            if (CallBackUpdateDocumentDetailInfor != null)
            {
                CallBackUpdateDocumentDetailInfor(new string[] { "", "" }, null);
            }

            _NurseSubmit_Enable = true;
            _NurseType = "";
        }
        # endregion 清空、重置

        # region 初始化表单表单基础设置信息
        /// <summary>
        /// 实例化时，初始化一些参数基本设置
        /// </summary>
        private void InitializeNurseEditor()
        {
            _LordClosing = false;

            this.DoubleBuffered = true; //开启双缓存，界面显示流畅

            //this.WindowState = FormWindowState.Maximized;

            //窗体标题和状态栏文字

            # region 加载服务端属性配置：执行Form1_Load时调用
            _DSS = new DataServiceSetting(RunMode); //            

            Comm._AdjustParagraphsNotLine = _DSS.AdjustParagraphsStyle;
            _PatientInforOrder = _DSS.PatientInforOrder;
            Comm._AdjustParagraphsDiff = _DSS.AdjustParagraphsDiff;
            _LocalBackSaveDays = _DSS.LocalBackSaveDays;

            //实例化和关闭的时候，删除历史备份
            BackUpHelp.DelOldBackUp();

            if (string.IsNullOrEmpty(_DSS.InputBoxHideItems))
            {
                Comm._listInputBoxHideItems.Clear();
            }
            else
            {
                Comm._listInputBoxHideItems = ArrayList.Adapter(_DSS.InputBoxHideItems.Replace(" ", "").Split(','));  //输入框隐藏菜单
            }

            DateFormat_HM = _DSS.DateFormat_HM;

            //行线，双红线自定义样式
            SetRowLineType();

            # endregion 加载服务端属性配置：执行Form1_Load时调用

            ////取得病人列表
            //getPatients(); //_PatientsInforDT全局变量保存

            ////初始化病人列表
            //loadTreePatient();

            this.ContextMenuStrip = null;

            //样式加载，取消掉样式
            ToolStripManager.Renderer = null;
            _ServerImages.Clear();

            //----------------------------------------临时调试代码-----------------------------------------


            //string[] arr = "a == b and ".Split(new string[] { "==", "!=", "<=", "<", ">=", ">" }, StringSplitOptions.RemoveEmptyEntries);


            //string[] arr = "a < b  And  c<=d  And e> f  And  g >= h".Split(new string[] { "<", "<=", ">", ">=" }, StringSplitOptions.RemoveEmptyEntries);
            //string[] arr = "a <= b".Split(new string[] { "<", "<=", ">", ">=" }, StringSplitOptions.RemoveEmptyEntries);
            //int aaa = arr.Length;

            //string[] arr1 = "a <= b < c".Split(new string[] { "<=", "<", ">=", ">" }, StringSplitOptions.RemoveEmptyEntries); //字符串和分割符号，都是按照从左往右的次序分割，不会重复分割
            //int aaa1 = arr1.Length;

            //string[] arr1 = "abcefg".Split('¤');     //长度为：1
            //string[] arr2 = "abc¤efg".Split('¤');   //长度为：2
            //string[] arr3 = "¤efg".Split('¤');      //长度为：2
            //string[] arr4 = "abc¤".Split('¤');      //长度为：2
            //string[] arr5 = "¤".Split('¤');         //长度为：2

            //string[] arr6 = "¤efg".Split(new string[] { "¤" }, StringSplitOptions.RemoveEmptyEntries);     //长度为：1，因为去掉空的了。


            //int a = 1;
            //System.Diagnostics.Debug.Assert(a == 10); //使用断言来帮助你写出更健壮的程序， 比如你想在程序运行中确定一个变量(a)的值是不是你期待的值10

            //string msg = "";
            //string outPutSum = "";
            //SumGroupObject.SumByGroup(ref msg);

            //int a = 2, b = 3;
            //int c  = a > b ? a : b;

            //(?<=\[)[^]]*?(?=\])

            //MatchCollection matchs = Regex.Matches(exeStr, @"(?<=\[)[^]]*?(?=\])");
            //foreach (Match m in matchs)
            //{
            //    //m.Value
            //    //matchs.to
            //}

            //MatchCollection matchs1 = Regex.Matches("", @"(?<=\[)[^]]*?(?=\])");
            //foreach (Match m in matchs1)
            //{
            //    //m.Value
            //    //matchs.to
            //}

            //MatchCollection matchs2 = Regex.Matches("(20 - [积极因子分1] +  - [积极因子分x])*2", @"(?<=\[)[^]]*?(?=\])");
            //foreach (Match m in matchs1)
            //{
            //    //m.Value
            //    //matchs.to
            //}

            //string[] arrNames = new string[matchs2.Count];

            //for (int i = 0; i < arrNames.Length; i++)
            //{
            //    arrNames[i] = matchs2[i].Value;
            //}

            //MyGDi mg = new MyGDi();
            //mg.DrawDeskTop();
            //System.Threading.Thread.Sleep(500); // System.Threading.Thread.Sleep(new TimeSpan(0, 0, 2));



            //DateTime dt111 = GetServerDate();

            //int aaaa = "1234561234".IndexOf("1");  //0


            ////错误，警告图标
            //MessageBox.Show("消息内容", "错误提醒", MessageBoxButtons.OK, MessageBoxIcon.Error);  //MessageBoxIcon.Error

            //bool ret = Evaluator.EvaluateToBool("8>9&&9>8");
            //int c = a > b ? a : b;


            //int a = dt1.CompareTo(dt2);
            //TimeSpan ts = dt1.Subtract(dt2);

            //string aa = PassWordClass.UHIDDecoded("WFSP");//1-> WFSP

            //Regex regex = new Regex("[^0-9]");
            //bool aa =  !regex.IsMatch("012");
            //bool a1 = !regex.IsMatch("12");
            //bool a2 = !regex.IsMatch("012a");

            //Regex reg = new Regex("^[0-9]+$");
            //bool bb = reg.IsMatch("012");
            //bool cc = reg.IsMatch("12");
            //bool dd = reg.IsMatch("12a");

            //string str = "asd体温fqw体温er";
            //int i = str.IndexOf("体温2");

            //----------------------------------------临时调试代码-----------------------------------------
        }

        /// <summary>
        /// 读取服务端属性，设置双红线属性样式：颜色，宽度
        /// </summary>
        private void SetRowLineType()
        {
            _RowLineType = _DSS.RowLineType;
            if (!string.IsNullOrEmpty(_RowLineType) && _RowLineType.Contains(","))
            {
                _RowLineType_Color = Color.FromName(_RowLineType.Split(',')[0]);

                //_RowLineType_Color_Selected = _RowLineType_Color; //主窗体设置了，以后手动选择的优先。不要打开一个标签就改变

                if (!int.TryParse(_RowLineType.Split(',')[1], out _RowLineType_Width))
                {
                    _RowLineType_Width = 1;
                }

                //不是默认红色的时候
                if (_RowLineType_Color != Color.Red)
                {
                    //更新工具栏图标和菜单文字
                    //SetRowLineMenuText();
                }
            }
        }

        #endregion 初始化表单表单基础设置信息

        #region 载入智能输入内容
        /// <summary>
        /// 设置智能输入，类似百度输入框
        /// </summary>
        private void SetWriteHelp()
        {
            try
            {
                //autocompleteMenuForWrite = new AutocompleteMenu();

                //this.autocompleteMenuForWrite.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                //this.autocompleteMenuForWrite.ImageList = null;

                ////this.autocompleteMenu1.AutoPopup = true;
                //this.autocompleteMenuForWrite.MinFragmentLength = 1;   //用来指定至少多少个字数，才会出现提示
                //this.autocompleteMenuForWrite.LeftPadding = 1;         //左边距：文字距离只能弹出框左边界的边距。并不是弹出框的位置
                //this.autocompleteMenuForWrite.TargetControlWrapper = null;

            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 智能输入加载内容，
        /// 遍历子节点
        /// </summary>
        /// <param name="node_Output"></param>
        /// <param name="listAll"></param>
        private void SetWriteHelpItems(XmlNode node_Output, List<string> listAll)
        {
            //<strip key="谷氨酰转肽酶IU/L" type="1">
            string eachValue = "";

            for (int i = 0; i < node_Output.ChildNodes.Count; i++)
            {
                //如果是注释那么跳过
                if (node_Output.ChildNodes[i].Name == @"#comment" || node_Output.ChildNodes[i].Name == @"#text")
                {
                    continue;
                }

                if ((node_Output.ChildNodes[i] as XmlElement).GetAttribute("type") == "1")
                {
                    eachValue = (node_Output.ChildNodes[i] as XmlElement).GetAttribute("key");
                    listAll.Add(eachValue);
                }
                else
                {
                    if (node_Output.ChildNodes[i].HasChildNodes)
                    {
                        SetWriteHelpItems(node_Output.ChildNodes[i], listAll);
                    }
                }
            }
        }

        # endregion 载入智能输入内容

        # region 权限设置，各控件有效/无效
        /// <summary>表单设置为有效/无效 控制方法
        /// 将表单设置为只读状态:True只读
        /// </summary>
        internal void SetAllControlDisabled(bool statePara)
        {
            ////工具栏的打印按钮始终为有效，其他的按钮都根据参数指定设置
            //foreach (ToolStripItem cl in this.toolStrip1.Items)
            //{
            //    if (cl.Text != "打印")
            //    {
            //        cl.Enabled = !statePara;
            //    }
            //}

            //菜单栏里面的修改数据的按钮进行设置
            toolStripSaveEnabled = !statePara;
            toolStripButtonCreatePage.Enabled = !statePara;
            toolStripButtonSave.Enabled = !statePara;
            tBnt_InsertRowUp.Enabled = !statePara;
            tBnt_InsertRowDown.Enabled = !statePara;
            tBnt_DelRow.Enabled = !statePara;
            toolStripButtonDelPage.Enabled = !statePara;
            toolStripButtonDelAllPage.Enabled = !statePara;
            toolStripToolBarWord.Enabled = !statePara;
            //ToolStripMenuItemSave.Enabled = !statePara;
            //ToolStripMenuItemScore.Enabled = !statePara;
            //ToolStripMenuItemDeletePage.Enabled = !statePara;


            //界面上的所有控件，不能修改的状态
            foreach (Control control in this.pictureBoxBackGround.Controls)
            {
                if (control.Name == "lblShowSearch")
                {
                    //是搜索图标图片，跳过   
                    //this.pictureBoxBackGround.Controls.Remove(this.lblShowSearch); //先移除这个搜索提示控件
                    continue;
                }

                if (control is RichTextBoxExtended)
                {
                    if (!statePara)
                    {
                        ((RichTextBoxExtended)control).ReadOnly = ((RichTextBoxExtended)control).IsReadOnly;
                    }
                    else
                    {
                        ((RichTextBoxExtended)control).ReadOnly = statePara;
                    }
                }
                else if (control is TransparentRTBExtended)  //TransparentRTBExtended是继承RichTextBoxExtended的 ,所以这里注释掉也可以
                {
                    if (!statePara)
                    {
                        ((TransparentRTBExtended)control).ReadOnly = ((TransparentRTBExtended)control).IsReadOnly;
                    }
                    else
                    {
                        ((TransparentRTBExtended)control).ReadOnly = statePara;
                    }
                }
                else
                {
                    control.Enabled = !statePara;
                    //没有指定的控件
                    //ShowInforMsg("在控件进行无效设置的时候，遇到无法识别的类型：" + control.ToString()); //Label：System.Windows.Forms.Label, Text: 调试科别名字
                }
            }
        }
        # endregion 权限设置，各控件有效/无效

        # region 获取外部日期 

        /// <summary>
        /// 获取外部日期
        /// </summary>
        /// <returns></returns>
        private DateTime GetServerDate()
        {
            if (GetDateTime != null)
            {
                return GetDateTime();
            }
            else
            {
                return DateTime.Now;
            }
        }

        # endregion 获取外部日期

        # region 病人基本信息更新
        /// <summary>
        /// 病人基本信息更新：线程更新
        /// </summary>
        private void UpdateThispageBaseInforUseThread()
        {
            //需要另起线程执行，否则速度会很慢
            System.Threading.Thread threadUpdateThispageBaseInfor = new System.Threading.Thread(new System.Threading.ThreadStart(UpdateThispageBaseInforThread)); //new System.Threading.Thread(new System.Threading.ThreadStart(UpdatePatientsLisDTThread));
            threadUpdateThispageBaseInfor.IsBackground = true; //当主线程退出的时候，IsBackground=FALSE的线程还会继续执行下去，直到线程执行结束。只有IsBackground=TRUE的线程才会随着主线程的退出而退出。

            //Thread的参数委托，有2种，一种是无参的，另一种是一个object参数的，如：new Thread(b).Start(new int[]{x,y,w,h);
            threadUpdateThispageBaseInfor.Start();
        }

        /// <summary>
        /// 更新基本信息的线程方法
        /// </summary>
        private void UpdateThispageBaseInforThread()
        {
            try
            {
                //UpdateThispageBaseInfor();
                this.Invoke(new MethodInvoker(delegate { UpdateThispageBaseInfor(_BaseInforLblnameArr, _CurrentPage, false); })); //防止查询统计验证界面，打开表单线程报错：从不是创建控件 的线程访问它。

                //更新所有页的基本信息。遇到某字段修改后，每页都要打开保存一下，否则打印所有页的内容为老的。
                this.Invoke(new MethodInvoker(delegate { UpdateAllPagesBaseInfor(); }));

            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 更新所有页的抬头基本信息标签的数据内容
        /// </summary>
        private void UpdateAllPagesBaseInfor()
        {
            //更新所有页的基本信息。遇到某字段修改后，每页都要打开保存一下，否则打印所有页的内容为老的。
            if (!_IsCreating && _DSS.UpdateAllPagesBaseInfor)
            {
                //基本信息保存在 非表格部分
                XmlNode nodePage = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms)));
                XmlNode thisPage;
                if (nodePage != null)
                {
                    int allPagesCount = nodePage.ChildNodes.Count;

                    for (int page = 1; page <= allPagesCount; page++)
                    {
                        //if (page.ToString() == _CurrentPage)
                        //{
                        //    //当前页其实也没有更新到xml数据中，而且界面上更新了，提示要保存。
                        //    continue; //当前页不需要再刷新保存了，界面打开的时候已经更新了。
                        //}

                        thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), page.ToString(), "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));//"NurseForm/Forms/Form[@SN='" + page.ToString() + "']"

                        //存在本页数据
                        if (thisPage != null && _BaseArrForPrintAllPageUpdate.Count == 1)
                        {
                            //编辑本页数据xml节点的所有属性，找到控件进行赋值，遍历所有节点属性
                            foreach (XmlAttribute xe in thisPage.Attributes)
                            {
                                //因为非表格样式的数据都存在里面，所以只取属性名在本页标签名字中有的。
                                if (IsBaseInfor(xe.Name))
                                {
                                    if (!xe.Value.Contains("¤")) //手动修改的不要再更新
                                    {
                                        _BaseArrForPrintAllPageUpdate[0].Name = xe.Name;
                                        _BaseArrForPrintAllPageUpdate[0].Text = xe.Value;
                                        _BaseArrForPrintAllPageUpdate[0].Tag = xe.Value;

                                        UpdateThispageBaseInfor(_BaseArrForPrintAllPageUpdate, page.ToString(), true); //根据基本信息刷新逻辑去判断

                                        //如果已经被刷新了内容
                                        if (_BaseArrForPrintAllPageUpdate[0].Text != xe.Value)
                                        {
                                            xe.Value = _BaseArrForPrintAllPageUpdate[0].Text;
                                        }

                                        //_IsNeedSave = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判断某名字的控件是否为标签基本信息项目
        /// </summary>
        /// <param name="namePara"></param>
        /// <returns></returns>
        private bool IsBaseInfor(string namePara)
        {
            bool ret = false;
            for (int i = 0; i < _BaseInforLblnameArr.Count; i++)
            {
                if (_BaseInforLblnameArr[i].Name == namePara)
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        /// <summary>
        /// 病人基本信息更新：1.如果为空--显示最新的； 2.服务端属性：每次更新，体现箭头历史进行设置显示；如果有改动，那么也要提示保存。
        /// 更新一页基本信息，如果其他也是每页也要更新的情况下，也是条用改方法，传入各业页码来更新。只不过isUpdateAllPagesBaseInforFlg为true，baseInforLblPara每次都为1
        /// </summary>
        private void UpdateThispageBaseInfor(List<LabelExtended> baseInforLblPara, string currentPage, bool isUpdateAllPagesBaseInforFlg)
        {

            bool toolStripSaveEnabledOrg = toolStripSaveEnabled; //不知道当前是有效还是无效

            try
            {

                toolStripSaveEnabled = false;   //严谨一些，线程更新基本信息的时候，不允许保存，防止数据不全

                //等待基本信息获取后，在更新本页的病人基本信息
                while (_IsThreadUpdatePatientsLisDTInfor)
                {
                    System.Threading.Thread.Sleep(50);
                }

                _IsThreadUpdatePatientsLisDTInfor = false;

                if (RunMode && !_IsNotNeedReloadTemplate && !_IsNotNeedReloadData)
                {
                    //获取外部数据
                    GetExternalData();
                }

                _CurrentBasicInfor.Clear();

                //病人基本信息:
                string name = "";
                string dtColumnName = "";
                LabelExtended lblTemp;
                XmlNode xn;
                string[] arrEach;   //本来一个名字就是一个项目，现在可能，诊断1+诊断2，连在一起显示。诊断1__诊断2

                string emptyShowStr = "--";  //"     " 两个全角空格，看不见的，操作不方便

                //根据初始化时存储好的，动态进行赋值----------------------病人基本信息--------------------------

                for (int i = 0; i < baseInforLblPara.Count; i++)
                {
                    lblTemp = baseInforLblPara[i];
                    name = lblTemp.Name;

                    //手动修改后的，跳过：不再做任何更新处理
                    if (lblTemp.Tag != null && lblTemp.Tag.ToString().Contains("¤")) // 床号="新A新A新A¤"  手动修改的
                    {
                        //手动修改过的，如果为空，也要变成空格，不然就没法再次双击修改了
                        if (lblTemp.Text == "")
                        {
                            lblTemp.Text = emptyShowStr;// "     ";
                        }

                        continue;
                    }

                    string strPatientInfo = _patientInfo.Dict.ContainsKey(name) ? _patientInfo.Dict[name].ToString() : string.Empty;

                    //为空则强行更新，不用考虑其他的配置。如果不为空，那么要不等于现在获取的值，而且满足配置的是否每页/最后一页 刷新
                    if (lblTemp.Text.Trim() == "" || lblTemp.Text.Trim() == emptyShowStr)
                    {
                        //刷新的内容不为空，并且和界面内容部一样的时候才要更新到界面，并提示内容改变要保存
                        if (strPatientInfo != "" && lblTemp.Text != strPatientInfo)
                        {
                            lblTemp.Text = strPatientInfo;

                            if (!_IsCreating) //不是正在添加页
                            {
                                //_IsNeedSave = true;
                                Comm.LogHelp.WriteInformation(string.Format("{0}中的{1}更新成了{2}", _TemplateName, name, strPatientInfo));
                            }
                        }

                    }

                    if (lblTemp != null)
                    {
                        if (!_CurrentBasicInfor.ContainsKey(lblTemp.Name)) //可能同个标签名，存在多个地方。
                        {
                            _CurrentBasicInfor.Add(lblTemp.Name, lblTemp.Text);
                        }
                        else
                        {
                            Comm.LogHelp.WriteErr(_TemplateName + " 中有多个Variable标签一样的" + lblTemp.Name);
                        }
                    }
                }



                //恢复保存按钮的有效和无效
                toolStripSaveEnabled = toolStripSaveEnabledOrg;


                if (!isUpdateAllPagesBaseInforFlg)
                {
                    //打开某页表单，添加页/创建表单的时候，执行该脚本
                    //如果脚本修改了界面上的内容，会提示保存。但是没有和基本信息一样，提示是哪个项目修改了。因为不知道脚本是修改了状态还是修改了数据。
                    Script_TemplateLoad();// 初始化的脚本，一般是根据基本信息，控制项目的默认值设定
                }

                //因为本方法是线程更新的，所以是否创建的控制基本信息刷新提示的标记，要这这个方法最后重置
                if (_IsCreating)
                {
                    _IsCreating = false;
                }

                arrEach = null;
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);

                //恢复保存按钮的有效和无效
                toolStripSaveEnabled = toolStripSaveEnabledOrg;

                throw ex;
            }
        }
        # endregion 病人基本信息更新

        # region 获取是否需要刷新该项目的基本信息
        /// <summary>
        /// 根据服务端配置的属性IntensiveCare.AlwaysRefreshGlobalInformation；判断Name比如床号等，要不要刷新
        /// </summary>
        /// <param name="settingPara"></param>
        /// <param name="namePara"></param>
        /// <returns></returns>
        private bool IsBaseInforNeedReflashByName(string settingPara, string namePara, int currentPagePara, int maxPagePara)
        {
            bool ret = false;

            //"床号,True|病区,False|入院主要诊断,True|科别,True"
            //诊断,AllPage ---> 
            //true:最后一页更新，AllPage:所有页更新,False：只有界面为空才更新（新建页的情况）。这样来解决老表单更新开启时，之前页都更新的错乱问题
            string[] arr = settingPara.Split('|');
            string[] arrEach;
            bool temp = false;

            for (int i = 0; i < arr.Length; i++)
            {
                arrEach = arr[i].Split(',');
                if (arrEach[0].Trim() == namePara)
                {
                    if (arrEach[1].Trim().ToLower() == "allpage")
                    {
                        //如果所有页更新，那么不需要判断页数
                        ret = true;
                    }
                    else
                    {
                        //不是所有页更新，那就是最后一页，那么还需要再根据页数来判断
                        if (!bool.TryParse(arrEach[1].Trim(), out temp))
                        {
                            temp = false;
                        }

                        //不是最后一页，那么也设置为Flase，不需要更新
                        if (temp && currentPagePara < maxPagePara)
                        {
                            temp = false;
                        }

                        ret = temp;
                    }
                }
            }

            arr = null;
            arrEach = null;

            return ret;
        }
        # endregion 获取是否需要刷新该项目的基本信息

        # region 刷新病人：完整的基本信息（所有）

        /// <summary>
        /// 线程，启动线程：获取病人基本信息对象
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="uhid"></param>
        private bool _IsThreadUpdatePatientsLisDTInfor = false;
        private void UpdatePatientsLisDTInforThread(DataTable dt, string uhid)
        {
            //需要另起线程执行，否则速度会很慢
            System.Threading.Thread threadUpdatePatientsLisDT = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(UpdatePatientsLisDTThread)); //new System.Threading.Thread(new System.Threading.ThreadStart(UpdatePatientsLisDTThread));
            threadUpdatePatientsLisDT.IsBackground = true;
            //Thread的参数委托，有2种，一种是无参的，另一种是一个object参数的，如：new Thread(b).Start(new int[]{x,y,w,h);
            threadUpdatePatientsLisDT.Start(new object[] { dt, uhid });
        }

        /// <summary>
        /// 带参数的线程更新病人基本信息对象
        /// </summary>
        /// <param name="args"></param>
        private void UpdatePatientsLisDTThread(object args)
        {
            try
            {
                _IsThreadUpdatePatientsLisDTInfor = true;
                //Comm.Logger.WriteInformation(this._patientInfo.Name + " 获取Root开始" + string.Format("{0:yyyy-MM-dd HH:mm:ss:ffff}", GetServerDate()));

                object[] arr = (object[])args;
                DataTable dt = (DataTable)arr[0];
                string uhidPara = arr[1].ToString();

                this.Invoke(new MethodInvoker(delegate { UpdatePatientsLisDTInfor(dt, uhidPara); }));

                //Comm.Logger.WriteInformation(this._patientInfo.Name + " 获取Root结束" + string.Format("{0:yyyy-MM-dd HH:mm:ss:ffff}", GetServerDate()));
                _IsThreadUpdatePatientsLisDTInfor = false;
            }
            catch (Exception ex)
            {
                _IsThreadUpdatePatientsLisDTInfor = false;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 初始化时病人列表数据不完全,这里打开某个病人的时候进行补充,更新到_patientsLisDT中:
        /// ：科别,住院号,病区,入区日期,入院日期,类别
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="uhid"></param>
        private XmlDocument xmlEmrRoot;
        private void UpdatePatientsLisDTInfor(DataTable dt, string uhid)
        {
            try
            {
                xmlEmrRoot = null;

                if (xmlEmrRoot == null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }
        # endregion 刷新病人：完整的基本信息（所有）

        # region 创建表单
        /// <summary>创建表单 调用方法
        /// 添加病人列表中的模板节点；创建表单的节点
        /// </summary>
        /// <returns></returns>
        private TreeNode AdddTemplateNode(string TemplateNamePara)
        {
            TreeNode retTN = null;

            //存在选中节点的时候
            if (this.treeViewPatients.SelectedNode != null)
            {
                //添加模板节点：
                TreeNode nodeChildTemplate = new TreeNode();
                nodeChildTemplate.Text = TemplateNamePara;
                nodeChildTemplate.Tag = "2";    //模板类型，暂且，默认创建的都为共享的
                nodeChildTemplate.ImageIndex = 3;
                this.treeViewPatients.SelectedNode.Nodes.Add(nodeChildTemplate);

                //创建模板的节点后，默认有第一页，所以要添加页节点
                ////添加页码节点
                TreeNode nodeChildPage = new TreeNode();
                nodeChildPage.Text = "1";
                nodeChildPage.Tag = "3";
                nodeChildPage.ImageIndex = 2;
                nodeChildTemplate.Nodes.Add(nodeChildPage);

                #region 连续创建N页
                XmlNode tempRoot = GetTemplateXml(TemplateNamePara).SelectSingleNode(nameof(EntXmlModel.NurseForm));
                int creatPages = 1;
                if (!int.TryParse((tempRoot as XmlElement).GetAttribute(nameof(EntXmlModel.AutoCreatPagesCount)), out creatPages))
                {
                    creatPages = 1;
                }

                //自动创建指定页
                if (creatPages > 1)
                {
                    //插入新的节点  //XmlElement newPage;

                    TreeNode nodeChildPageMore;

                    for (int i = 2; i <= creatPages; i++)
                    {
                        //添加页码节点
                        nodeChildPageMore = new TreeNode();
                        nodeChildPageMore.Text = i.ToString();
                        nodeChildPageMore.Tag = "3";
                        nodeChildPageMore.ImageIndex = 2;
                        nodeChildTemplate.Nodes.Add(nodeChildPageMore);
                    }
                }

                #endregion 连续创建N页

                nodeChildTemplate.Expand();

                retTN = nodeChildTemplate;

                _CreatTreeNode = retTN;

                this.treeViewPatients.SelectedNode = nodeChildPage;

                _IsCreating = true;
                _IsCreatedTemplate = true;

                //打开新建的第一页：
                treeViewPatients_NodeMouseDoubleClick(nodeChildPage, null);
            }

            return retTN;
        }
        # endregion 创建表单

        # region 新建页
        /// <summary>调整段落列的时候，也会调用添加下一页
        /// 添加病人列表中的页节点：添加页
        /// </summary>
        /// <returns></returns>
        private TreeNode AdddPageNode()
        {
            if (this.uc_Pager1.PageCount <= int.Parse(_CurrentPage))
            {
                this.uc_Pager1.SetMaxCount(int.Parse(_CurrentPage) + 1);
            }
            _IsCreated = true; //设置标记
            return null;
            //TreeNode retTN = null;

            //if (_CurrentPatientTreeNode != null && _CurrentTemplateNameTreeNode != null)
            //{
            //    //病人列表中如果没有（当前页对应的下一页）下一页，那么就需要添加
            //    if (_CurrentTemplateNameTreeNode.Nodes.Count <= int.Parse(_CurrentPage))
            //    {
            //        //添加页码节点
            //        TreeNode nodeChildPage = new TreeNode();
            //        nodeChildPage.Text = (int.Parse(_CurrentPage) + 1).ToString();
            //        nodeChildPage.Tag = "3"; //nodeChildPage.Tag = nodeChild;
            //        nodeChildPage.ImageIndex = 2;

            //        _CurrentTemplateNameTreeNode.Nodes.Add(nodeChildPage);

            //        retTN = nodeChildPage;

            //        _CreatTreeNode = nodeChildPage;

            //        _IsCreated = true; //设置标记

            //        if (CallBackUpdateDocumentPaging != null)
            //        {
            //            //0当前页码，1当前页码
            //            int[] arr = new int[] { _CurrentTemplateNameTreeNode.Nodes.Count, int.Parse(_CurrentPage) };
            //            CallBackUpdateDocumentPaging(arr, null);
            //        }
            //    }
            //}

            //return retTN;
        }

        /// <summary>
        /// 连续添加
        /// </summary>
        /// <returns></returns>
        private TreeNode AdddPageNodeN()
        {
            this.uc_Pager1.SetMaxCount(int.Parse(_CurrentPage) + 1);
            _IsCreated = true; //设置标记

            //TreeNode retTN = null;

            //if (_CurrentPatientTreeNode != null && _CurrentTemplateNameTreeNode != null)
            //{
            //    //病人列表中如果没有（当前页对应的下一页）下一页，那么就需要添加
            //    //if (_CurrentTemplateNameTreeNode.Nodes.Count <= int.Parse(_CurrentPage))
            //    //{
            //    //添加页码节点
            //    TreeNode nodeChildPage = new TreeNode();
            //    nodeChildPage.Text = (_CurrentTemplateNameTreeNode.Nodes.Count + 1).ToString();
            //    nodeChildPage.Tag = "3"; //nodeChildPage.Tag = nodeChild;
            //    nodeChildPage.ImageIndex = 2;

            //    _CurrentTemplateNameTreeNode.Nodes.Add(nodeChildPage);

            //    retTN = nodeChildPage;

            //  _CreatTreeNode = nodeChildPage;

            //   _IsCreated = true; //设置标记
            //    //}
            //}

            return null;
        }
        # endregion 新建页

        # region 点击新建页的时，添加节点
        /// <summary>
        /// 添加病人列表中的页节点：添加页/新建页
        /// 
        /// </summary>
        /// <returns></returns>
        private TreeNode AdddPageNode_ForClick(int nowMaxPage)
        {
            this.uc_Pager1.PageCount = nowMaxPage + 1;
            this.uc_Pager1.SetMaxCount(nowMaxPage + 1);
            //TreeNode retTN = null;
            //病人列表中如果没有下一页，那么就需要添加
            //if (_CurrentPatientTreeNode != null && _CurrentTemplateNameTreeNode != null)
            //{
            //    //添加页码节点
            //    TreeNode nodeChildPage = new TreeNode();
            //    nodeChildPage.Text = (nowMaxPage + 1).ToString();
            //    nodeChildPage.Tag = "3";
            //    nodeChildPage.ImageIndex = 2;

            //    _CurrentTemplateNameTreeNode.Nodes.Add(nodeChildPage);

            //    retTN = nodeChildPage;

            //    _CreatTreeNode = nodeChildPage;
            //}
            return null;
            //return retTN;
        }
        # endregion 点击新建页的时，添加节点

        # region 获取表单的模板文件
        /// <summary>
        /// 获取表单的模板文件
        /// </summary>
        /// <param name="tempName"></param>
        private XmlDocument GetTemplateXml(string tempName)
        {
            string errMsg = @"无法取到服务端的:Recruit3XML\Template\" + tempName + ".xml";
            XmlDocument retXml;

            if (RunMode)
            {
                retXml = new XmlDocument();
                string xmlText = GetTemplateXmlServer(tempName);    // "";

                if (string.IsNullOrEmpty(xmlText))
                {
                    //ShowInforMsg(errMsg);
                    Comm.LogHelp.WriteErr(errMsg);
                    MessageBox.Show(errMsg, "配置错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                retXml.InnerXml = xmlText;
            }
            else
            {
                _CurrentTemplateXml_Path = FileHelp.GetFilePathFromRootDirAndFileName(Comm._Nurse_TemplatePath, tempName);

                if (_CurrentTemplateXml_Path == "")
                {
                    ShowInforMsg("找不到表单模板文件：" + tempName);
                    this.Cursor = Cursors.Default;
                    //return;//找不到指定的模板配置文件，跳出返回
                }

                //根据该节点对应的模板，首先加载界面模板，然后加载 数据
                retXml = new XmlDocument();
                retXml.Load(_CurrentTemplateXml_Path);
            }

            return retXml;
        }

        private string GetTemplateXmlServer(string tempName)
        {
            string xmlText = "";
            //GlobalVariable._serviceLava.ReadOutXmlFile(@"\Nurse\Template\" + tempName + ".xml", ref xmlText);

            return xmlText;
        }

        /// <summary>
        /// Log图片等
        /// </summary>
        /// <param name="imgName"></param>
        /// <returns></returns>
        private Image GetServerImage(string imgName)
        {
            Image img = null;

            //(@"\Recruit3XML\Image\logo.jpg");       
            //img = byteArrayToImage(GlobalVariable._serviceLava.GetServerImage(@"Nurse\Image\" + imgName));

            if (img == null)
            {
                ShowInforMsg("服务器上找不到模板中指定的图片：" + imgName);
            }

            return img;
        }

        # endregion 获取表单的模板文件

        # region 加载【创建表单】的子菜单
        /// <summary>
        /// 遍历初始化模板菜单的所有项，包括字菜单
        /// </summary>
        /// <param name="xnTemplates"></param>
        /// <param name="tsmi"></param>
        private void SetTemplateFromXmlNode(XmlNode xnTemplates, ToolStripMenuItem tsmi, ToolStripSplitButton ownerToolStripSplitButton)
        {
            ToolStripMenuItem tsi;
            ToolStripMenuItem tsiFolder;
            string name = "";

            foreach (XmlNode xn in xnTemplates.ChildNodes)
            {
                //如果是注释那么跳过
                if (xn.Name == @"#comment" || xn.Name == @"#text")
                {
                    continue; //注释和节点值也认为是子节点的。唉
                }

                if (xmlMode)
                {
                    name = xn.Name;
                }
                else
                {
                    name = (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                }

                //0：模板
                if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Type)).Trim() == "0")
                {

                    tsi = new ToolStripMenuItem();
                    tsi.Name = name;
                    tsi.Text = tsi.Name; //showName; // 

                    if (ownerToolStripSplitButton.Name == "toolStripTemplate")
                    {
                        tsi.Click += new System.EventHandler(this.TemplateMenu_Click);
                    }
                    else
                    {
                        //tsi.Click += new System.EventHandler(this.BatchMenu_Click);
                    }

                    if (tsmi == null)
                    {
                        ownerToolStripSplitButton.DropDownItems.Add(tsi);//toolStripTemplate.DropDownItems.Add(tsi);
                    }
                    else
                    {
                        tsmi.DropDownItems.Add(tsi);
                    }
                }
                else
                {
                    tsiFolder = new ToolStripMenuItem();
                    tsiFolder.Name = name;
                    tsiFolder.Text = tsiFolder.Name;

                    if (tsmi == null)
                    {
                        ownerToolStripSplitButton.DropDownItems.Add(tsiFolder); //toolStripTemplate.DropDownItems.Add(tsiFolder);
                    }
                    else
                    {
                        tsmi.DropDownItems.Add(tsiFolder);
                    }

                    SetTemplateFromXmlNode(xn, tsiFolder, ownerToolStripSplitButton);
                }
            }
        }
        # endregion 加载【创建表单】的子菜单

        # region 创建表单
        /// <summary>
        /// 点击模板菜单事件：创建表单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TemplateMenu_Click(object sender, EventArgs e)
        {
            this.pictureBoxBackGround.Focus();
            SaveToOp(); //防止直接修改后，直接点击创建其他病人的表单，而表格填补的空白行已经删除，会报错

            if (!this.pictureBoxBackGround.Enabled) // && this.toolStripSaveEnabled新建后又取消，双后在双击，这是填补的空白行没有了，在输入就报为异常:索引的单元格，找不到它所对应的XML节点
            {
                this.pictureBoxBackGround.Enabled = true;
            }

            //如果需要保存，提示是否需要先保存，再执行后续操作，如果硬要取消，那么充值
            CheckForSave();

            ToolStripMenuItem owner = (ToolStripMenuItem)sender;

            //验证，是否选中了某病人进行创建
            if (this.treeViewPatients.SelectedNode != null)
            {
                //是否选中了某个病人节点
                if (this.treeViewPatients.SelectedNode.Level == 0)
                {
                    //选中的是第一层节点:病人节点
                }
                else
                {
                    //或者不报错，根据2，3选择对应的病人节点，然后再做处理。
                    if (this.treeViewPatients.SelectedNode.Level == 2)
                    {
                        this.treeViewPatients.SelectedNode = this.treeViewPatients.SelectedNode.Parent.Parent; //选中当前病人节点，判断该病人有无该表单
                    }
                    else
                    {
                        this.treeViewPatients.SelectedNode = this.treeViewPatients.SelectedNode.Parent;
                    }
                }

                bool isHavaMe = false;
                //存在行验证：分两步：本地病人列表中查看有没有该病人表单，如果没有，那么再刷新数据库查看有没有
                //本地又分两步：列表树（可能被筛选过滤），然后再从病人列表DT信息中再判断一次
                foreach (TreeNode tn in this.treeViewPatients.SelectedNode.Nodes)
                {
                    if (tn.Text == owner.Text)
                    {
                        isHavaMe = true;

                        break;
                    }
                }

                //本地判断2：病人列表DT信息中再判断一次
                if (_PatientsInforDT != null && !isHavaMe)
                {
                    if (this.treeViewPatients.SelectedNode != null && this.treeViewPatients.SelectedNode.Tag != null)
                    {
                        string uhid = ((DataRow)this.treeViewPatients.SelectedNode.Tag)["UHID"].ToString();
                        foreach (DataRow drChild in _PatientsInforDT.Rows)
                        {
                            if (drChild["UHID"].ToString() == uhid && drChild["表单名"].ToString() == owner.Text)
                            {
                                isHavaMe = true;

                                break;
                            }
                        }
                    }
                }

                //如果已经被创建了
                if (isHavaMe)
                {
                    string errmsg = this.treeViewPatients.SelectedNode.Text + " 已经创建了指定的表单：" + owner.Text + "，请刷新病人列表。";
                    ShowInforMsg(errmsg);
                    MessageBox.Show(errmsg, "提示消息", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    ShowInforMsg("请刷新病人列表，确认该表单已经被创建了。");

                    //就不自动刷新了，让用户手动刷新吧。因为是在本地客户端判断，就发现有该表单了。
                    ////插入数据失败：已经存在，需要刷新病人列表，有可能页码等已经不正确了。
                    //toolStripButtonReflash_Click(null, null);
                }
                //如果没有被创建，那么这里才开始执行创建该表单
                if (!isHavaMe)
                {
                    //_IsCreatedTemplate = true; //不能再下面赋值，因为里面是多线程的，可能走到，还没有设置为True
                    AdddTemplateNode(owner.Text);
                    IsNeedSave = true;
                    _IsCreated = true;
                    //_IsCreatedTemplate = true;
                    _TemplateUpdate = "";
                }
            }
            else
            {
                ShowInforMsg("请先选择需要创建表单的病人。");
            }
        }
        # endregion 创建表单

        # region 新建页

        /// <summary>
        /// 添加新的一页
        /// (点击菜单新建页，添加新的一页)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripAddNextPage_Click(object sender, EventArgs e)
        {
            this.pictureBoxBackGround.Focus();
            SaveToOp(); //防止直接修改后，直接点击创建其他病人的表单，而表格填补的空白行已经删除，会报错

            if (_IsCreated)
            {
                string msg = "当前页是新建页，需要保存后；才能继续添加页。";
                ShowInforMsg(msg);
                MessageBox.Show(msg, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //验证，分两步：本地病人列表中查看有没有该病人表单，如果没有，那么再刷新数据库查看有没有
            if (this.treeViewPatients.SelectedNode != null)
            {
                //如果选中了其他节点，切换为选中当前表单页号节点
                if (this.treeViewPatients.SelectedNode.Level <= 1)
                {
                    //this.treeViewPatients.SelectedNode = _CurrentTemplateNameTreeNode;
                    //选中之前打开表单的节点
                    SetOpenPageTreeNodeSelected();
                }

                //选中了模板或者页，才能创建；如果是页，根据父节点得到模板名称
                if (this.treeViewPatients.SelectedNode.Level > 0)
                {
                    if (this.treeViewPatients.SelectedNode.Level == 2)
                    {
                        //如果选择的是页节点，那么取对应的父节点。
                        this.treeViewPatients.SelectedNode = this.treeViewPatients.SelectedNode.Parent;
                    }

                    //创建页的时候，必须已经打开了该病人的表单，否则是否是表格式的表单，会导致当前页最后的空白数据清空，导致翻页异常。
                    if (((this._patientInfo.PatientId != ((DataRow)this.treeViewPatients.SelectedNode.Parent.Tag)["UHID"].ToString() ||
                        _TemplateName != this.treeViewPatients.SelectedNode.Text)) && this.treeViewPatients.SelectedNode.Nodes.Count != 0) //防止新建的表格式，没添加行，就保存，再次打开没有页，但是表单xml已经有了。
                    {
                        MessageBox.Show("请先选择某表单后，再进行添加页操作。", "提示消息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //上面的限制还不够，如果最后一页没有写满页到最后一行，必须打开最后一页才能创建，否则也异常，再次打开新建页没有的
                    if (this.treeViewPatients.SelectedNode.Nodes.Count == 0)
                    {
                        //由于创建后没有保存，所以没有任何页的情况，刚打开表单界面，没有打开其他表单，要可以新建页
                    }
                    else
                    {
                        if (_CurrentPage != this.treeViewPatients.SelectedNode.Nodes.Count.ToString()) //用模板下的子节点数来判断，在筛选的情况下，可能会错误
                        {
                            MessageBox.Show("请打开最后一页后，再进行新建页操作。", "提示消息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    _CurrentPatientTreeNode = this.treeViewPatients.SelectedNode.Parent;
                    _CurrentTemplateNameTreeNode = this.treeViewPatients.SelectedNode;

                    this.treeViewPatients.SelectedNode = AdddPageNode_ForClick(_CurrentTemplateNameTreeNode.Nodes.Count);

                    _IsCreating = true;

                    //打开新建的第一页：
                    treeViewPatients_NodeMouseDoubleClick(this.treeViewPatients.SelectedNode, null);

                    //_IsCreating = false; //因为是线程处理的，这里重置可能有问题
                    IsNeedSave = true;
                    _IsCreated = true;
                }
                else
                {
                    MessageBox.Show("请先选择某表单后，再进行添加页操作。", "提示消息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        # endregion 新建页

        # region 获取当前表单总页数  根据模板节点的子节点在筛选情况下就错误了
        /// <summary>
        /// 暂时废弃的方法，不用
        /// 获取当前打开的表单的总页数
        /// 保存和添加页的时候，必须要取得正确的总页数，否则会数据混乱。
        /// </summary>
        /// <returns>目前的总页数，即：最大页数</returns>
        private int GetCurrentTemplateMaxPage()
        {
            int ret = 1; //默认为1，任何表单至少有一页

            //暂时：过滤的时候，当前打开的节点不过滤来解决页码问题


            return ret;
        }

        /// <summary>
        /// 在创建表单、新建页和删除页情况下，更新病人信息dt；防止筛选时还是用老的数据来筛选过滤
        /// </summary>
        private void UpdatePatientsInforDT(string uhidPara, string tempNamePara, int PagesPara)
        {
            //更新逻辑：1.如果已经存在该行，直接更新，没有的话，在该uhid的末尾行添加
            //2.同时，某个病人还没创建任何表单，创建表单的话，直接在该行上更新，不需要插入行
            if (_PatientsInforDT != null)
            {
                //先判断，该表单是否已经存在dt中了
                string strFilter = "UHID='" + uhidPara + "' AND 表单名='" + tempNamePara + "'";
                DataRow[] drArrEachPatient = _PatientsInforDT.Select(strFilter);  //_PatientsInforDT.Copy()

                if (drArrEachPatient != null && drArrEachPatient.Length == 1)
                {
                    if (PagesPara > 0)
                    {
                        //更新表单页数
                        drArrEachPatient[0]["总页数"] = PagesPara.ToString();
                    }
                    else
                    {
                        //删除表单的情况下,清空
                        drArrEachPatient[0]["表单名"] = "";
                        drArrEachPatient[0]["总页数"] = "";
                    }
                }
                else
                {
                    //dt中不存在该病人的该表单名这行的情况下：要么插入行，要么该病人没有任何表单的话，直接利用初始行信息的行（每个病人至少一行）

                    //在判断，该病人是否一张表单都么有，那么也不用插入行，直接更新就行
                    strFilter = "UHID='" + uhidPara + "'";
                    drArrEachPatient = _PatientsInforDT.Select(strFilter);

                    //该病人还没有任何表单，那么就利用病人信息行
                    if (drArrEachPatient != null && drArrEachPatient.Length == 1 && string.IsNullOrEmpty(drArrEachPatient[0]["表单名"].ToString()))
                    {
                        drArrEachPatient[0]["表单名"] = tempNamePara;
                        drArrEachPatient[0]["总页数"] = PagesPara.ToString();
                        drArrEachPatient[0]["表单类型"] = _TemplateRight;
                    }
                    else
                    {
                        //需要在该病人uhid最后一行位置添加行
                        //foreach (DataRow drChild in _PatientsInforDT.Rows)
                        DataRow drChild;
                        DataRow drNew;
                        for (int i = _PatientsInforDT.Rows.Count - 1; i >= 0; i--)
                        {
                            drChild = _PatientsInforDT.Rows[i];
                            if (drChild["UHID"].ToString() == uhidPara)
                            {
                                drNew = _PatientsInforDT.NewRow();

                                drNew["UHID"] = uhidPara;
                                drNew["表单名"] = tempNamePara;
                                drNew["总页数"] = PagesPara.ToString();
                                drNew["表单类型"] = _TemplateRight;

                                _PatientsInforDT.Rows.InsertAt(drNew, i + 1);

                                break;
                            }
                        }
                    }
                }
            }

        }

        # endregion 获取当前表单总页数  根据模板节点的子节点在筛选情况下就错误了

        # region 显示提示：手指指示目标单元格或者输入框
        private void ShowSearch(Control cl)
        {
            return;
            //if (_Need_Assistant_Control != null && !cl.IsDisposed)
            //{
            //    Point showLocation = cl.Location;

            //    ShowSearch(showLocation);
            //}
        }

        private void ShowSearch(Point clPoint)
        {
            return;
            Point showLocation = clPoint;

            //指示手指的位置
            if (this.lblShowSearch == null || this.lblShowSearch.IsDisposed)
            {
                //保存，打印前，会删除掉该控件
                this.lblShowSearch = new TransparentLabel();
                //if (_SelectRowBackColor != Color.Empty)
                //{
                //    this.lblShowSearch.BackColor = _SelectRowBackColor;
                //}
                //else
                //{
                this.lblShowSearch.BackColor = System.Drawing.Color.Transparent; //this.pictureBoxBackGround.BackColor;
                //this.lblShowSearch.Parent = this.pictureBoxBackGround;
                //}
                //this.lblShowSearch.BackgroundImage = new Bitmap(lblShowSearch.Width, lblShowSearch.Height);
                //Graphics g = Graphics.FromImage(lblShowSearch.BackgroundImage);
                ////请注意这个透明并不是真正的透明，而是用父控件的当前位置的颜色填充PictureBox内的相应位置的颜色。 
                //g.Clear(Color.Transparent);
                //g.Dispose();


                //this.lblShowSearch.DimmedColor = System.Drawing.Color.Red;
                //this.lblShowSearch.ForeColor = System.Drawing.Color.Red;
                this.lblShowSearch.Name = "lblShowSearch";
                //this.lblShowSearch.Opacity = 100;
                //this.lblShowSearch.Radius = 1;
                this.lblShowSearch.Click += new System.EventHandler(this.lblShowSearch_Click);
            }

            if (!this.pictureBoxBackGround.Controls.Contains(this.lblShowSearch))
            {
                this.pictureBoxBackGround.Controls.Add(this.lblShowSearch);
            }

            //this.lblShowSearch.Text = "▲◥";
            this.lblShowSearch.Text = "→"; //↑↓←→
            this.lblShowSearch.Size = new Size(22, 22);
            this.lblShowSearch.ForeColor = System.Drawing.Color.OrangeRed;

            //this.lblShowSearch.BackgroundImage = new Bitmap(lblShowSearch.Width, lblShowSearch.Height);
            //Graphics g = Graphics.FromImage(lblShowSearch.BackgroundImage);
            ////请注意这个透明并不是真正的透明，而是用父控件的当前位置的颜色填充PictureBox内的相应位置的颜色。 
            //g.Clear(Color.Transparent); //g.Clear(Color.White); //g.Clear(Color.Transparent);
            //g.Dispose();
            this.lblShowSearch.Font = new Font("宋体", 10, FontStyle.Bold);
            this.lblShowSearch.Draw();

            this.lblShowSearch.BackColor = System.Drawing.Color.Transparent; //this.pictureBoxBackGround.BackColor;


            lblShowSearch.Visible = true;
            lblShowSearch.BringToFront();

            lblShowSearch.Location = new Point(showLocation.X - lblShowSearch.Size.Width - 3, showLocation.Y);
            //this.lblShowSearch.Refresh();

            DrawRowSignShow(true); //不然，如果现实提示合计单元格，立刻删除行等操作，又提示行，原来单元格那里就会有阴影。但是续打的时候，又会现实行标记，导致异常
        }

        private void lblShowSearch_Click(object sender, EventArgs e)
        {

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                _Need_Assistant_Control.Focus();
            }
            else
            {
                this.panelMain.Focus();
            }


            lblShowSearch.Hide();
            //this.pictureBoxBackGround.Refresh();
        }
        # endregion 显示提示：手指指示目标单元格或者输入框

        # region 显示消息内容：显示，日志可选
        /// <summary>
        /// 默认将消息内容保存到业务日志中
        /// </summary>
        /// <param name="msgPara"></param>
        private void ShowInforMsg(string msgPara)
        {
            if (CallBackAddOutPutMsg != null)
            {
                CallBackAddOutPutMsg(msgPara, null);
            }
        }


        /// <summary>
        /// 显示提示消息
        /// </summary>
        /// <param name="msgPara"></param>
        /// <param name="isErrorOrBusiness">加入到异常日志True，还是业务日志 false</param>
        private void ShowInforMsg(string msgPara, bool isErrorOrBusiness)
        {
            ShowInforMsg(msgPara);

            if (isErrorOrBusiness)
            {
                Comm.LogHelp.WriteErr(msgPara);
            }
            else
            {
                Comm.LogHelp.WriteInformation(msgPara);
            }
        }

        /// <summary>
        /// 显示消息时，强行展开【输出】面板
        /// </summary>
        /// <param name="msgPara"></param>
        private void ShowInforMsgExpand(string msgPara)
        {
            ShowInforMsg(msgPara);
        }
        #endregion 显示消息内容

        #region 插入选择的图片

        #region 属性
        private const int Band = 5;
        private const int MinWidth = 10;
        private const int MinHeight = 10;
        private EnumMousePointPosition m_MousePointPosition;
        private Point p, p1;
        #endregion

        private void MyMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            p.X = e.X;
            p.Y = e.Y;
            p1.X = e.X;
            p1.Y = e.Y;

            PictureBox me = ((PictureBox)sender);
            me.BringToFront();

            # region 边框线和图像缩放类型
            if (me.BorderStyle == BorderStyle.FixedSingle)
            {
                边框线ToolStripMenuItem.Checked = true;
            }
            else
            {
                边框线ToolStripMenuItem.Checked = false;
            }


            toolStripTextBoxSize.Text = me.Width.ToString() + " " + me.Height.ToString();

            # endregion 边框线和图像缩放类型

            //_IsNeedSave = true;
        }
        /// <summary>
        /// 鼠标离开事件需要改进
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyMouseLeave(object sender, EventArgs e)
        {
            Control s = (Control)sender;

            #region 保存 数据
            //XmlNodeList nodes = doc.GetElementsByTagName(s.Name);
            //XmlElement xn;
            //if (nodes.Count != 1)
            //{
            //    xn = doc.CreateElement(s.Name);
            //}
            //else
            //{
            //    xn = (XmlElement)doc.GetElementsByTagName(s.Name)[0];
            //}
            //xn.SetAttribute("Top", s.Top.ToString());
            //xn.SetAttribute("Left", s.Left.ToString());
            //xn.SetAttribute(nameof(EntXmlModel.Width), s.Width.ToString());
            //xn.SetAttribute("Height", s.Height.ToString());


            //XmlNodeList xnl = doc.GetElementsByTagName(this.Name);
            //XmlElement xnp;
            //if (xnl.Count < 1)
            //{
            //    xnp = doc.CreateElement(this.Name);
            //}
            //else
            //{
            //    xnp = (XmlElement)xnl[0];
            //}
            //xnp.AppendChild((XmlNode)xn);
            //doc.DocumentElement.AppendChild((XmlNode)xnp);
            //doc.Save(xmlDocPath);
            #endregion 保存 数据

            m_MousePointPosition = EnumMousePointPosition.MouseSizeNone;
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// 移动鼠标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Control lCtrl = (sender as Control);

            //拖动范围限制
            int gap = 4;
            if (lCtrl.Left + e.X - p.X < 0 - gap || lCtrl.Left + e.X - p.X > this.pictureBoxBackGround.Width - gap * 2
                || lCtrl.Top + e.Y - p.Y < 0 - gap || lCtrl.Top + e.Y - p.Y > this.pictureBoxBackGround.Height - gap * 2)
            {
                this.Cursor = Cursors.SizeAll;      //'四方向   

                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (e.X == p.X && e.Y == p.Y)
                {
                    //没有改变属性，不需要进行保存
                }
                else
                {
                    IsNeedSave = true;
                }

                switch (m_MousePointPosition)
                {
                    case EnumMousePointPosition.MouseDrag:  //拖动位置

                        lCtrl.Left = lCtrl.Left + e.X - p.X;
                        lCtrl.Top = lCtrl.Top + e.Y - p.Y;

                        break;
                    case EnumMousePointPosition.MouseSizeBottom:
                        lCtrl.Height = lCtrl.Height + e.Y - p1.Y;
                        p1.X = e.X;
                        p1.Y = e.Y; //'记录光标拖动的当前点   
                        break;
                    case EnumMousePointPosition.MouseSizeBottomRight:
                        lCtrl.Width = lCtrl.Width + e.X - p1.X;
                        lCtrl.Height = lCtrl.Height + e.Y - p1.Y;
                        p1.X = e.X;
                        p1.Y = e.Y; //'记录光标拖动的当前点   
                        break;
                    case EnumMousePointPosition.MouseSizeRight:
                        lCtrl.Width = lCtrl.Width + e.X - p1.X;
                        //       lCtrl.Height = lCtrl.Height + e.Y - p1.Y;   
                        p1.X = e.X;
                        p1.Y = e.Y; //'记录光标拖动的当前点   
                        break;
                    case EnumMousePointPosition.MouseSizeTop:
                        lCtrl.Top = lCtrl.Top + (e.Y - p.Y);
                        lCtrl.Height = lCtrl.Height - (e.Y - p.Y);
                        break;
                    case EnumMousePointPosition.MouseSizeLeft:
                        lCtrl.Left = lCtrl.Left + e.X - p.X;
                        lCtrl.Width = lCtrl.Width - (e.X - p.X);
                        break;
                    case EnumMousePointPosition.MouseSizeBottomLeft:
                        lCtrl.Left = lCtrl.Left + e.X - p.X;
                        lCtrl.Width = lCtrl.Width - (e.X - p.X);
                        lCtrl.Height = lCtrl.Height + e.Y - p1.Y;
                        p1.X = e.X;
                        p1.Y = e.Y; //'记录光标拖动的当前点   
                        break;
                    case EnumMousePointPosition.MouseSizeTopRight:
                        lCtrl.Top = lCtrl.Top + (e.Y - p.Y);
                        lCtrl.Width = lCtrl.Width + (e.X - p1.X);
                        lCtrl.Height = lCtrl.Height - (e.Y - p.Y);
                        p1.X = e.X;
                        p1.Y = e.Y; //'记录光标拖动的当前点   
                        break;
                    case EnumMousePointPosition.MouseSizeTopLeft:
                        lCtrl.Left = lCtrl.Left + e.X - p.X;
                        lCtrl.Top = lCtrl.Top + (e.Y - p.Y);
                        lCtrl.Width = lCtrl.Width - (e.X - p.X);
                        lCtrl.Height = lCtrl.Height - (e.Y - p.Y);
                        break;
                    default:
                        break;
                }
                if (lCtrl.Width < MinWidth) lCtrl.Width = MinWidth;
                if (lCtrl.Height < MinHeight) lCtrl.Height = MinHeight;

            }
            else
            {
                m_MousePointPosition = MousePointPosition(lCtrl.Size, e);   //'判断光标的位置状态   
                switch (m_MousePointPosition)   //'改变光标   
                {
                    case EnumMousePointPosition.MouseSizeNone:
                        this.Cursor = Cursors.Arrow;        //'箭头   
                        break;
                    case EnumMousePointPosition.MouseDrag:
                        this.Cursor = Cursors.SizeAll;      //'四方向   
                        break;
                    case EnumMousePointPosition.MouseSizeBottom:
                        this.Cursor = Cursors.SizeNS;       //'南北   
                        break;
                    case EnumMousePointPosition.MouseSizeTop:
                        this.Cursor = Cursors.SizeNS;       //'南北   
                        break;
                    case EnumMousePointPosition.MouseSizeLeft:
                        this.Cursor = Cursors.SizeWE;       //'东西   
                        break;
                    case EnumMousePointPosition.MouseSizeRight:
                        this.Cursor = Cursors.SizeWE;       //'东西   
                        break;
                    case EnumMousePointPosition.MouseSizeBottomLeft:
                        this.Cursor = Cursors.SizeNESW;     //'东北到南西   
                        break;
                    case EnumMousePointPosition.MouseSizeBottomRight:
                        this.Cursor = Cursors.SizeNWSE;     //'东南到西北   
                        break;
                    case EnumMousePointPosition.MouseSizeTopLeft:
                        this.Cursor = Cursors.SizeNWSE;     //'东南到西北   
                        break;
                    case EnumMousePointPosition.MouseSizeTopRight:
                        this.Cursor = Cursors.SizeNESW;     //'东北到南西   
                        break;
                    default:
                        break;
                }
            }

        }

        private EnumMousePointPosition MousePointPosition(Size size, System.Windows.Forms.MouseEventArgs e)
        {

            if ((e.X >= -1 * Band) | (e.X <= size.Width) | (e.Y >= -1 * Band) | (e.Y <= size.Height))
            {
                if (e.X < Band)
                {
                    if (e.Y < Band) { return EnumMousePointPosition.MouseSizeTopLeft; }
                    else
                    {
                        if (e.Y > -1 * Band + size.Height)
                        { return EnumMousePointPosition.MouseSizeBottomLeft; }
                        else
                        { return EnumMousePointPosition.MouseSizeLeft; }
                    }
                }
                else
                {
                    if (e.X > -1 * Band + size.Width)
                    {
                        if (e.Y < Band)
                        { return EnumMousePointPosition.MouseSizeTopRight; }
                        else
                        {
                            if (e.Y > -1 * Band + size.Height)
                            { return EnumMousePointPosition.MouseSizeBottomRight; }
                            else
                            { return EnumMousePointPosition.MouseSizeRight; }
                        }
                    }
                    else
                    {
                        if (e.Y < Band)
                        { return EnumMousePointPosition.MouseSizeTop; }
                        else
                        {
                            if (e.Y > -1 * Band + size.Height)
                            { return EnumMousePointPosition.MouseSizeBottom; }
                            else
                            { return EnumMousePointPosition.MouseDrag; }
                        }
                    }
                }
            }
            else
            { return EnumMousePointPosition.MouseSizeNone; }
        }
        # region //----------图片的操作菜单-------

        ///// <summary>
        ///// 根据菜单数据库设置的大小进行设置图片大小
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        private void contextMenuStripForImage_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            try
            {
                string inputValue = toolStripTextBoxSize.Text.Trim();
                inputValue = inputValue.Replace(" ", "¤"); //防止输入多个空格
                inputValue = inputValue.Replace("　", "¤"); //防止全角空格

                if (inputValue.Split('¤').Length < 2)
                {
                    //不合法
                }
                if (inputValue.Split('¤').Length == 2)
                {
                    //正常
                }
                else
                {
                    //将连续的两个变成一个
                    inputValue = inputValue.Replace("¤¤", "¤");
                }

                if (inputValue != "" && inputValue.Contains("¤"))
                {
                    PictureBox me = (PictureBox)contextMenuStripForImage.SourceControl;

                    string[] arr = inputValue.Split('¤');
                    double width = StringNumber.getFirstNum(arr[0]);
                    double heitht = StringNumber.getFirstNum(arr[1]);

                    me.Size = new Size((int)width, (int)heitht);

                    IsNeedSave = true;

                    arr = null;
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                MessageBox.Show("输入的长宽格式不正确，请确认长和宽是否用空格分开的。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //throw ex;
            }
        }

        /// <summary>
        /// 删除图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            PictureBox me = (PictureBox)contextMenuStripForImage.SourceControl;

            string[] arr = me.Tag.ToString().Split('¤');

            this.Controls.Remove(me);
            me.Dispose();

            //删除的话，先要更新属性（最后保存时，已经没有该图片了）；其他编辑不用实时更新xml，最后保存的时候更新xml就好了
            if (_RecruitXML != null && arr.Length > 1)  //arr.Length > 1表示不是新添加的，如果是新添加的，直接删除就行了
            {
                XmlNode thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), _CurrentPage, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));//"NurseForm/Forms/Form[@SN='" + _CurrentPage + "']"

                //判断xml中是否已经存在本页的Page数据
                if (thisPage != null)
                {
                    (thisPage as XmlElement).SetAttribute(arr[0], "");

                    if ((thisPage as XmlElement).Attributes[arr[0]] != null)
                    {
                        (thisPage as XmlElement).Attributes.Remove((thisPage as XmlElement).Attributes[arr[0]]); //删除xml节点的指定属性
                    }
                }
            }

            IsNeedSave = true;

            arr = null;
        }

        /// <summary>
        /// 图片边框线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 边框线ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            边框线ToolStripMenuItem.Checked = !边框线ToolStripMenuItem.Checked;
            PictureBox me = (PictureBox)contextMenuStripForImage.SourceControl;

            string[] arr = me.Tag.ToString().Split('¤');

            if (边框线ToolStripMenuItem.Checked)
            {
                me.BorderStyle = BorderStyle.FixedSingle;
            }
            else
            {
                me.BorderStyle = BorderStyle.None;
            }

            IsNeedSave = true;
        }

        #endregion

        #endregion

        #region 验证提醒是否需要进行保存
        /// <summary>
        /// 构造方法，不需要关注保存是否成功的情况下调用
        /// </summary>
        /// <returns></returns>
        private bool CheckForSave()
        {
            bool saveState = true;
            return CheckForSave(ref saveState);
        }

        /// <summary>
        /// 验证是否需要保存：
        /// 创建表单、切换表单页、关闭窗体 的时候，如果需要保存，提示保存
        /// </summary>
        /// <returns></returns>
        private bool CheckForSave(ref bool saveState)
        {
            bool ret = false;    //不需要保存为False，只有进行了保存,且成功了才为True
            saveState = true;    //不需要保存，也为True；False表示进行了保存，且保存失败。其他情况都为True

            if (IsNeedSave && !_IsLocked && toolStripSaveEnabled && _TemplateName != "") //_TemplateName == "" 打开界面后后第一次操作，不需要验证
            {
                //Msg_Confirm_CheckForSave = "[{0}]_[{1}] 已经被修改，是否需要保存？" PaperWord中的确认消息有方括号的
                string msg = "数据已经被修改，是否需要保存？"; //数据已经创建或者修改，是否需要保存？
                if (_IsCreated)
                {
                    msg = "数据已经被创建，是否需要保存？";
                }

                if (_LordClosing || MessageBox.Show(msg, "保存确认：" + _TemplateName + "," + this._patientInfo.PatientName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //保存
                    bool saveRet = SaveToDetail(true); //点击保存的时候，验证失败不用回滚xml数据，但是如果新建页等，数据不回滚就会是不合理的数据，再一页成功保存后，就有错误的格式的数据了（比如前一页日期格式错误）
                    ret = saveRet;
                    saveState = saveRet;

                    if (!saveRet)
                    {
                        //切换页，新建页的时候，需要回滚内存数据。切换表单时不用回滚，因为本身也没有保存到db，会重新获取其他表单数据
                        //放弃修改，用原来的数据替换:否则当前页输入错误日期，新建下一页后，提示保存失败，其实数据还是在的，新页保存成功后，前页的错误数据也就在了
                        //切换页的时候，同样会有这个问题
                        if (_RecruitXML != null && _RecruitXML_Org != null)
                        {
                            _RecruitXML.InnerXml = _RecruitXML_Org.InnerXml;
                        }

                        //重置标记位，如果是新建，那么删除新建的节点
                        IsNeedSave = false; //因为在提示后进行保存的失败，等于已经放弃，所以也要重置

                        //新建节点删除和标记位重置---------------
                        if (_CreatTreeNode != null && _IsCreated)
                        {
                            _CreatTreeNode.Remove(); //新建页就是页节点，创建表单，就是表单节点。
                        }

                        if (_IsCreatedTemplate) //可能由于调整段落列后，存在几页的节点
                        {
                            if (_CurrentTemplateNameTreeNode != null)
                            {
                                _CurrentTemplateNameTreeNode.Remove(); //所有页的删除
                            }
                        }

                        _IsCreated = false;
                        _IsCreatedTemplate = false;
                    }
                }
                else
                {
                    Comm.LogHelp.WriteInformation(GlobalVariable.LoginUserInfo.UserCode + "在提示保存后，取消了保存：" + this._patientInfo.PatientId + "，" + this._patientInfo.PatientName + "，" + _TemplateName);

                    //放弃修改，用原来的数据替换
                    if (_RecruitXML != null)
                    {
                        _RecruitXML.InnerXml = _RecruitXML_Org.InnerXml;
                    }

                    IsNeedSave = false; //因为已经放弃，所以也要重置

                    //新建节点删除和标记位重置---------------
                    if (_CreatTreeNode != null && _IsCreated)
                    {
                        _CreatTreeNode.Remove(); //新建页就是页节点，创建表单，就是表单节点。
                    }

                    if (_IsCreatedTemplate) //可能由于调整段落列后，存在几页的节点
                    {
                        if (_CurrentTemplateNameTreeNode != null)
                        {
                            _CurrentTemplateNameTreeNode.Remove(); //所有页的删除
                        }
                    }

                    _IsCreated = false;
                    _IsCreatedTemplate = false;

                    ret = false;
                }

            }

            return ret;
        }
        # endregion 验证提醒是否需要进行保存

        # region 双击表单，打开表单
        private static bool _IsLoading = false;
        private static bool _IsNotNeedCellsUpdate = false; //visable属性改变后，是否许需要单元格 !_IsNotNeedCellsUpdate 判断的地方

        /// <summary>
        /// 设置页尾斜线
        /// </summary>
        private void SetToolStripPageEndCrossLine()
        {
            if (GlobalVariable.ToolStripMenuItemPageEndCrossLine != null)
            {
                if (_TableType && _TableInfor != null)
                {
                    //页尾斜线（表格的页尾斜线的操作菜单控制）
                    GlobalVariable.ToolStripMenuItemPageEndCrossLine.Enabled = true;

                    if (_RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))) != null)
                    {
                        if ((_RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))) as XmlElement).GetAttribute(nameof(EntXmlModel.PageEndCrossLine)) == "")
                        {
                            GlobalVariable.ToolStripMenuItemPageEndCrossLine.Checked = false;
                        }
                        else
                        {
                            GlobalVariable.ToolStripMenuItemPageEndCrossLine.Checked = true;

                            //int pages = _CurrentTemplateNameTreeNode.Nodes.Count;
                            //if (pages == 0)
                            //{
                            //    pages = 1; //不可能为0，至少为1.
                            //}

                            //if (pages.ToString() == _CurrentPage)
                            //{
                            //    GlobalVariable.ToolStripMenuItemPageEndCrossLine.Enabled = true;
                            //}
                            //else
                            //{
                            //    GlobalVariable.ToolStripMenuItemPageEndCrossLine.Enabled = false;
                            //}
                        }

                        int pages = _CurrentTemplateNameTreeNode.Nodes.Count;
                        if (pages == 0)
                        {
                            pages = 1; //不可能为0，至少为1.
                        }

                        if (pages.ToString() == _CurrentPage)
                        {
                            GlobalVariable.ToolStripMenuItemPageEndCrossLine.Enabled = true;
                        }
                        else
                        {
                            GlobalVariable.ToolStripMenuItemPageEndCrossLine.Enabled = false;
                        }
                    }
                    else
                    {
                        GlobalVariable.ToolStripMenuItemPageEndCrossLine.Checked = false;
                    }
                }
                else
                {
                    GlobalVariable.ToolStripMenuItemPageEndCrossLine.Checked = false;
                    GlobalVariable.ToolStripMenuItemPageEndCrossLine.Enabled = false;
                }
            }
        }

        /// <summary>
        /// 护理归档权限，归档后临时权限获取
        /// 在归档的前提下判断这个方法
        /// patientUHID
        /// _userName
        /// GetNurseArchive(patientUHID, _userName)
        /// </summary>
        private string GetNurseArchive(string uhid, string userId, ref string done)
        {
            if (!RunMode)
            {
                return "编辑";
            }

            return "0"; //如果没有配置，那么既然是在归档后来查询的，就认为没有任何权限
        }

        /// <summary>
        /// 创建表单的时候，设置传递参数
        /// </summary>
        private void LoadCreatTemplatePara()
        {
            if (!string.IsNullOrEmpty(_CreatTemplatePara))
            {
                string[] arrPara = _CreatTemplatePara.Split('¤');
                Control[] cl = this.pictureBoxBackGround.Controls.Find(arrPara[1], false);
                if (cl != null && cl.Length > 0)
                {
                    if (cl[0].Text != arrPara[0])
                    {
                        if (cl[0] is RichTextBoxExtended)
                        {
                            HorizontalAlignment temp = ((RichTextBoxExtended)cl[0]).SelectionAlignment;
                            ((RichTextBoxExtended)cl[0]).Text = arrPara[0]; //12分¤压疮评分
                            ((RichTextBoxExtended)cl[0]).SelectionAlignment = temp;
                        }
                        else
                        {
                            cl[0].Text = arrPara[0]; //12分¤压疮评分
                        }

                        IsNeedSave = true; //如果修改了值，那么要提示已经修改需要保存
                    }
                }
                else
                {
                    ShowInforMsg("找不到被创建表单是的传参控件：" + arrPara[1], true);
                }

                _CreatTemplatePara = "";
                arrPara = null;
            }
        }

        /// <summary>
        /// 获取外部数据
        /// </summary>
        private void GetExternalData()
        {

        }

        #region 显示非表格的双红线
        private void ShowDoubleLineInputboxs()
        {
            if (_ListDoubleLineInputBox != null && _ListDoubleLineInputBox.Count > 0)
            {
                for (int i = 0; i < _ListDoubleLineInputBox.Count; i++)
                {
                    if (!_ListDoubleLineInputBox[i].Focused)
                    {
                        _ListDoubleLineInputBox[i].Refresh();
                        _ListDoubleLineInputBox[i].PrintCusLines();
                    }
                }
            }
        }

        /// <summary>
        /// 重绘的时候，双红线需要重新绘制，否则无法显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxBackGround_Paint(object sender, PaintEventArgs e)
        {
            ShowDoubleLineInputboxs();
        }
        #endregion 显示双红线

        /// <summary>
        /// 验证数据的UHID是否一致
        /// </summary>
        /// <param name="uhidPara">uhid唯一号</param>
        /// <param name="xmlData">表单数据</param>
        /// <returns></returns>
        private bool CheckDataUHID(string uhidPara, XmlDocument xmlData)
        {
            bool ret = true;

            //<?xml version="1.0" encoding="utf-8"?>
            //    <NurseForm UHID="2[宋春雨]389375566671.875" 
            XmlNode root = xmlData.SelectSingleNode(nameof(EntXmlModel.NurseForm));
            if (root != null)
            {
                if ((root as XmlElement).GetAttribute(nameof(EntXmlModel.UHID)) == "")
                {
                    ret = false;
                    string msg = "当前表单数据存在异常，UHID为空。";
                    ShowInforMsg(msg, true);
                    Comm.LogHelp.WriteErr(uhidPara + " | " + xmlData.InnerXml);
                }
                else if ((root as XmlElement).GetAttribute(nameof(EntXmlModel.UHID)) != uhidPara)
                {
                    ret = false;
                    string msg = "当前表单数据存在异常，和病人的UHID不一致，请联系管理员。";
                    ShowInforMsg(msg, true);
                    Comm.LogHelp.WriteErr(uhidPara + " | " + xmlData.InnerXml);

                }
                else
                {
                    ret = true;
                }
            }
            else
            {
                ret = false;
                Comm.LogHelp.WriteErr("表单Xml数据的格式，创建异常，不存在根节点NurseForm：" + uhidPara + " | " + xmlData.InnerXml);
            }

            return ret;
        }

        /// <summary>
        /// 过渡特效 提示
        /// </summary>
        /// <param name="msg"></param>
        private void ShowEffect(string message)
        {
            //-----------------------------------------加载特效提示：正在加载-------------
            //_picTransEffect.Image = (Bitmap)this.pictureBoxBackGround.Image.Clone();
            if (this._picTransEffect.Size.Width != 0 && this._picTransEffect.Size.Height != 0)
            {
                _picTransEffect.Image = new Bitmap(this._picTransEffect.Size.Width, this._picTransEffect.Size.Height);
                _picTransEffect.BackgroundImage = Properties.Resources.翻页;
                _picTransEffect.BackgroundImageLayout = ImageLayout.Stretch;
                _picTransEffect.Visible = true;
                //Graphics.FromImage(_picTransEffect.Image).DrawString("正在加载数据，请稍候…", new Font("宋体", 11), Brushes.Black, new Point(pictureBoxBackGround.Width / 3, -pictureBoxBackGround.Top + Screen.PrimaryScreen.WorkingArea.Height / 4));
                Graphics.FromImage(_picTransEffect.Image).DrawString(message, new Font("宋体", 11), Brushes.Black, new Point(pictureBoxBackGround.Width / 3, -pictureBoxBackGround.Top + Screen.PrimaryScreen.WorkingArea.Height / 4));

                _picTransEffect.BringToFront();
                _picTransEffect.Refresh();
            }
            //-----------------------------------------加载特效提示：正在加载-------------
        }
        # endregion 双击表单，打开表单

        # region 当前页对应的模板样式ID
        /// <summary>
        /// 得到当前页对应的模板样式ID：每页的模板样式可能都不一样
        /// 参数：_TemplateXml.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design)))，和当前页数
        /// </summary>
        /// <param name="node"></param>
        /// <param name=nameof(EntXmlModel.Form)></param>
        /// <returns></returns>
        private XmlNode GetTemplatePageNode(XmlNode node, int page)
        {
            XmlNode retNode = node.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), page.ToString(), "=", nameof(EntXmlModel.Form))); //"Page[@ID='" + page.ToString() + "']"   //NurseForm/Design/

            //<Design Mode="repeat"> 新建页，按模板页循环来
            string mode = (node as XmlElement).GetAttribute(nameof(EntXmlModel.Mode)).ToLower();

            var xmlTemp = node.ParentNode.SelectSingleNode("/" + nameof(EntXmlModel.NurseForm));
            int createPage = 1;
            if (!int.TryParse((xmlTemp as XmlElement).GetAttribute(nameof(EntXmlModel.AutoCreatPagesCount)), out createPage))
            {
                createPage = 1;
            }

            if (retNode != null)
            {
                //存在当页对应的样式的就返回
                return retNode;
            }
            else if (mode == "repeat" || createPage > 1)
            {
                //例如正反两页的，新建第三页的是有，还是循环12,12页的模板
                int index = 0;

                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    //如果是注释那么跳过
                    if (node.ChildNodes[i].Name == @"#comment" || node.ChildNodes[i].Name == @"#text")
                    {
                        continue; //注释和节点值也认为是子节点的。唉
                    }


                    index++; //默认的顺序编号ID
                }

                int count = page % index;

                if (count == 0)
                {
                    count = index;
                }

                retNode = node.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), count.ToString(), "=", nameof(EntXmlModel.Form)));//"Page[@ID='" + count.ToString() + "']"

                return retNode;
            }
            else
            {

                int temp = 1;
                List<int> listID = new List<int>();
                int index = 1;

                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    //如果是注释那么跳过
                    if (node.ChildNodes[i].Name == @"#comment" || node.ChildNodes[i].Name == @"#text")
                    {
                        continue; //注释和节点值也认为是子节点的。唉
                    }

                    if (!int.TryParse((node.ChildNodes[i] as XmlElement).GetAttribute(nameof(EntXmlModel.ID)), out temp))
                    {
                        temp = index; //如果没有设置ID编号，那么就以节点的上下顺序来作为编号
                        (node.ChildNodes[i] as XmlElement).SetAttribute(nameof(EntXmlModel.ID), temp.ToString()); //重新设置不合法的编号              
                    }

                    listID.Add(temp);

                    index++; //默认的顺序编号ID
                }

                listID.Sort(); //编号进行排序

                for (int i = 0; i < listID.Count; i++)
                {
                    if (page < listID[i])
                    {
                        if (i > 0)
                        {
                            retNode = node.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), listID[i - 1].ToString(), "=", nameof(EntXmlModel.Form))); //"Page[@ID='" + listID[i - 1].ToString() + "']"
                        }
                        else
                        {
                            retNode = node.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), listID[i].ToString(), "=", nameof(EntXmlModel.Form)));//"Page[@ID='" + listID[i].ToString() + "']"
                        }

                        break;
                    }

                    retNode = node.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), listID[i].ToString(), "=", nameof(EntXmlModel.Form)));//"Page[@ID='" + listID[i].ToString() + "']"
                }

                return retNode;
            }
        }
        # endregion 当前页对应的模板样式ID

        # region 载入本页数据
        /// <summary>
        /// 载入本页表单的数据
        /// </summary>
        private void LoadThisPageData(TableInfor tableInforPara)
        {
            try
            {
                if (_RecruitXML != null)
                {
                    //页号手动修改处理
                    XmlNode root = _RecruitXML.SelectSingleNode(nameof(EntXmlModel.NurseForm));
                    string start = (root as XmlElement).GetAttribute(nameof(EntXmlModel.PageNoStart)); //默认起始页号为空，代表0


                    //提出护理记录单在装订时是连接在首次护理记录单后，护理记录单的页码将以“3”作为起始页。现在护理记录单的起始页以‘1’开始。
                    if (!string.IsNullOrEmpty(_TemplateXml.DocumentElement.GetAttribute(nameof(EntXmlModel.FirstPageNo))))
                    {
                        //1@1    
                        //1@3
                        //因为表单的创建可能是在批量的时候，也可能在客户端新建。所以在配置该属性后，如果发现PageNoStart为空，那么就复制
                        if (string.IsNullOrEmpty(start))
                        {
                            start = "1@" + _TemplateXml.DocumentElement.GetAttribute(nameof(EntXmlModel.FirstPageNo));
                            (root as XmlElement).SetAttribute(nameof(EntXmlModel.PageNoStart), start);
                            IsNeedSave = true;
                        }
                    }

                    if (start != "" && start != "0")
                    {
                        if (CheckHelp.IsNumber(start)) //老版本保存的肯定是数字，整体修改所有页码
                        {
                            int a = int.Parse(start);
                            int b = int.Parse(_CurrentPage);  //表单里面显示的页码，和实际的页码不一定一样。比如：危重和普通转来转去。就会导致页码中间断掉。实际是2页，但是第一页需要显示1，第二页显示6……

                            string showValue = (b + a).ToString();

                            if (string.IsNullOrEmpty(_PageNoFormat))
                            {
                                //前面已经赋值：showValue = (b + a).ToString();
                            }
                            else
                            {
                                //ca签名查询预览的时候，_CurrentTemplateNameTreeNode为null，不需要处理。通过设置最大页数即可
                                if (_CurrentTemplateNameTreeNode != null)
                                {
                                    //总页码
                                    _MaxPageForPrintAll = _CurrentTemplateNameTreeNode.Nodes.Count; //总页数，根据这个物理页数，计算出手动修改后应该的页码
                                }

                                _MaxPageForPrintAllShow = _MaxPageForPrintAll + a;
                                showValue = String.Format(_PageNoFormat, showValue, _MaxPageForPrintAllShow.ToString());
                            }

                            //最终设置页码：
                            _PageNo.Text = showValue;   //(b + a).ToString();
                        }
                        else
                        {
                            string showValue = PageNo.GetValue(start, int.Parse(_CurrentPage));

                            if (string.IsNullOrEmpty(_PageNoFormat))
                            {
                                //前面已经赋值：showValue = (b + a).ToString();
                            }
                            else
                            {
                                //ca签名查询预览的时候，_CurrentTemplateNameTreeNode为null，不需要处理。通过设置最大页数即可
                                if (_CurrentTemplateNameTreeNode != null)
                                {
                                    //总页码
                                    _MaxPageForPrintAll = _CurrentTemplateNameTreeNode.Nodes.Count; //总页数，根据这个物理页数，计算出手动修改后应该的页码
                                }

                                _MaxPageForPrintAllShow = int.Parse(PageNo.GetValue(start, _MaxPageForPrintAll));
                                showValue = String.Format(_PageNoFormat, showValue, _MaxPageForPrintAllShow.ToString());
                            }

                            //最终设置页码：
                            _PageNo.Text = showValue;
                        }
                    }
                    else
                    {
                        ////ca签名查询预览的时候，_CurrentTemplateNameTreeNode为null，不需要处理。通过设置最大页数即可
                        //if (_CurrentTemplateNameTreeNode != null)
                        //{
                        //    //需要显示的总页码更新显示：
                        //    _MaxPageForPrintAll = _CurrentTemplateNameTreeNode.Nodes.Count; //总页数，根据这个物理页数，计算出手动修改后应该的页码
                        //}

                        //_MaxPageForPrintAllShow = _MaxPageForPrintAll;                  //没有手动修改页码，那么逻辑总页码等于实际的总页数
                        _MaxPageForPrintAllShow = this.uc_Pager1.PageCount;


                        //在这个处理之前，已经赋值为页码数字，所以要判断格式串，重新赋值
                        if (string.IsNullOrEmpty(_PageNoFormat))
                        {
                            //前面已经赋值：_PageNo.Text = _CurrentPage;
                        }
                        else
                        {
                            _PageNo.Text = String.Format(_PageNoFormat, _CurrentPage, _MaxPageForPrintAllShow.ToString());
                        }
                    }

                    //非表格部分
                    XmlNode thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), _CurrentPage, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form))); //"NurseForm/Forms/Form[@SN='" + _CurrentPage + "']"

                    string[] arrRtbeValue; //用来拆分数据

                    //存在本页数据
                    if (thisPage != null)
                    {
                        Control[] ct;
                        PictureBox picNew;
                        bool tempBool = false;
                        string showText = "";
                        try
                        {
                            #region 非表格属性赋值
                            //编辑本页数据xml节点的所有属性，找到控件进行赋值，遍历所有节点属性
                            foreach (XmlAttribute xe in thisPage.Attributes)
                            {
                                //手动添加的静态图片，以IMAGE开头的属性名保存数据
                                if (xe.Name.StartsWith(nameof(EntXmlModel.IMAGE)) && !string.IsNullOrEmpty(xe.Value))
                                {
                                    //是图片:属性并是名字 ¤ 坐标¤二进制
                                    picNew = new PictureBox();
                                    picNew.Tag = xe.Value;

                                    string[] arr = xe.Value.Split('¤');

                                    picNew.BackgroundImage = FileHelp.ConvertStringToImage(arr[4]);
                                    picNew.BackgroundImageLayout = ImageLayout.Stretch;
                                    picNew.Location = StringNumber.getSplitValue(arr[1]);
                                    picNew.Name = arr[0];

                                    //图片是否需要边框线
                                    if (arr[3] == "")
                                    {
                                        picNew.BorderStyle = System.Windows.Forms.BorderStyle.None;
                                    }
                                    else
                                    {
                                        picNew.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                                    }

                                    picNew.Size = StringNumber.getSize(arr[2]);

                                    picNew.ContextMenuStrip = this.contextMenuStripForImage;
                                    picNew.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MyMouseDown);
                                    picNew.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MyMouseMove);
                                    picNew.MouseLeave += new System.EventHandler(this.MyMouseLeave);

                                    this.pictureBoxBackGround.Controls.Add(picNew);

                                    arr = null;
                                }
                                else
                                {
                                    ct = this.pictureBoxBackGround.Controls.Find(xe.Name, false);
                                    if (ct.Length > 1 && ct[0] is TransparentRTBExtended)
                                    {
                                        ct = ct.Where(a => !(a is TransparentRTBExtended)).ToArray();
                                    }
                                    if (ct != null && ct.Length > 0)
                                    {
                                        if (ct[0] is RichTextBoxExtended)
                                        {
                                            #region RichTextBoxExtended                                        
                                            ((RichTextBoxExtended)ct[0]).SetAlignment(); //默认的对齐方式

                                            //ct[0].Text = xe.Value;
                                            //排序后：Text 0¤单红线 1¤双红线 2¤权限 3¤Rtf 4
                                            //retValue = rtbe.Text + "¤" + rtbe._HaveSingleLine.ToString() + "¤" + rtbe._HaveDoubleLine.ToString() + "¤" + CreateUser + "¤" + rtbe.Rtf;
                                            arrRtbeValue = xe.Value.Split('¤');

                                            if (arrRtbeValue.Length > 1)
                                            {
                                                if (!string.IsNullOrEmpty(arrRtbeValue[4]))
                                                {
                                                    //如果存在富文本，那么就会有值；否则为空
                                                    ((RichTextBoxExtended)ct[0])._IsRtf = true;
                                                    ((RichTextBoxExtended)ct[0]).Rtf = arrRtbeValue[4];
                                                }
                                                else
                                                {
                                                    ((RichTextBoxExtended)ct[0])._IsRtf = false;

                                                    HorizontalAlignment temp = ((RichTextBoxExtended)ct[0]).SelectionAlignment;
                                                    ((RichTextBoxExtended)ct[0]).Text = arrRtbeValue[0];
                                                    ((RichTextBoxExtended)ct[0]).SelectionAlignment = temp;
                                                }

                                                //手膜签名：公共的rtf数据设置过来
                                                if (string.IsNullOrEmpty(arrRtbeValue[0].Trim()) && string.IsNullOrEmpty(arrRtbeValue[4].Trim()) && !string.IsNullOrEmpty(arrRtbeValue[3]) && (xe.Name.StartsWith("签名") || xe.Name.StartsWith("记录者")))
                                                {
                                                    //先从数据文件中根据用户id是否存储了签名的手摸图片
                                                    XmlNode fingerPrintSignImages = _RecruitXML.SelectSingleNode("//" + EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.FingerPrintSignImages))); //NurseForm/FingerPrintSignImages

                                                    //数据文件中没有手摸图片节点，就先添加
                                                    if (fingerPrintSignImages == null)
                                                    {
                                                        //插入节点
                                                        XmlElement images;
                                                        images = _RecruitXML.CreateElement(nameof(EntXmlModel.FingerPrintSignImages));
                                                        _RecruitXML.SelectSingleNode("//" + nameof(EntXmlModel.NurseForm)).AppendChild(images);

                                                        fingerPrintSignImages = images;
                                                    }

                                                    //先判断数据中有没有手膜签名图片
                                                    XmlNode imgRtfNode = fingerPrintSignImages.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), arrRtbeValue[3], "=", nameof(EntXmlModel.Image)));//"Image[@ID='" + arrRtbeValue[3] + "']"
                                                    if (imgRtfNode != null)
                                                    {
                                                        ((RichTextBoxExtended)ct[0])._IsRtf = true;
                                                        ((RichTextBoxExtended)ct[0]).Rtf = (imgRtfNode as XmlElement).GetAttribute(nameof(EntXmlModel.Value));
                                                    }
                                                }

                                                //双红线
                                                if (!bool.TryParse(arrRtbeValue[1], out tempBool))
                                                {
                                                    tempBool = false;
                                                }
                                                ((RichTextBoxExtended)ct[0])._HaveSingleLine = tempBool;

                                                if (!bool.TryParse(arrRtbeValue[2], out tempBool))
                                                {
                                                    tempBool = false;
                                                }
                                                ((RichTextBoxExtended)ct[0])._HaveDoubleLine = tempBool;

                                                //((RichTextBoxExtended)ct[0]).PrintCusLines(); //这里绘制，刷新后，会无效

                                                //后来输入框的重绘事件钩子WndProc WM_PAINT会导致很多输入法输入汉字不提示汉字选择的界面
                                                if (((RichTextBoxExtended)ct[0])._HaveSingleLine || ((RichTextBoxExtended)ct[0])._HaveDoubleLine)
                                                {
                                                    _ListDoubleLineInputBox.Add((RichTextBoxExtended)ct[0]);
                                                }

                                                ((RichTextBoxExtended)ct[0]).CreateUser = arrRtbeValue[3];

                                                //大于5表示有修订历史
                                                if (arrRtbeValue.Length > 5)
                                                {
                                                    ((RichTextBoxExtended)ct[0])._EditHistory = arrRtbeValue[5];
                                                }

                                                //大于6表示有日期格式
                                                if (arrRtbeValue.Length > 6)
                                                {
                                                    if (ct[0].Name.StartsWith("日期"))
                                                    {
                                                        ((RichTextBoxExtended)ct[0])._IsSetValue = true;

                                                        if (arrRtbeValue[0] != "")
                                                        {
                                                            ((RichTextBoxExtended)ct[0])._YMD = arrRtbeValue[6];
                                                        }
                                                        else
                                                        {
                                                            ((RichTextBoxExtended)ct[0])._YMD = ((RichTextBoxExtended)ct[0])._YMD_Default;
                                                        }

                                                        ((RichTextBoxExtended)ct[0])._FullDateText = arrRtbeValue[0];

                                                        showText = ((RichTextBoxExtended)ct[0]).GetShowDate();

                                                        if (showText != ((RichTextBoxExtended)ct[0]).Text)
                                                        {
                                                            HorizontalAlignment temp = ((RichTextBoxExtended)ct[0]).SelectionAlignment;
                                                            ((RichTextBoxExtended)ct[0]).Text = showText;
                                                            ((RichTextBoxExtended)ct[0]).SelectionAlignment = temp;
                                                        }

                                                        ((RichTextBoxExtended)ct[0])._IsSetValue = false;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                HorizontalAlignment temp = ((RichTextBoxExtended)ct[0]).SelectionAlignment;
                                                //异常  情况下；或者，的情况
                                                ct[0].Text = xe.Value;
                                                ((RichTextBoxExtended)ct[0]).SelectionAlignment = temp;
                                                //ShowInforMsg("数据可能存在异常：" + _CurrentPage + "页的，项目：" + xe.Name);
                                            }
                                            //((RichTextBoxExtended)ct[0]).OldText = ((RichTextBoxExtended)ct[0]).Text; //打开表单的脚本，如果值一样也会提示保存
                                            #endregion
                                        }
                                        else if (ct[0] is CheckBoxExtended)
                                        {
                                            #region CheckBoxExtended
                                            arrRtbeValue = xe.Value.Split('¤');

                                            try
                                            {
                                                LoadSkinCheckBox(((CheckBoxExtended)ct[0]), arrRtbeValue);

                                                //((CheckBoxExtended)ct[0]).Checked = bool.Parse(arrRtbeValue[0]);
                                            }
                                            catch (Exception ex)
                                            {
                                                string msgErr = "勾选框设置的时候发现，上次保存的值不是布尔值，请确认是否存在同名的其他非勾选框控件：" + ct[0].Name;
                                                Comm.LogHelp.WriteErr(msgErr);
                                                MessageBox.Show(msgErr, "数据错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                throw ex;
                                            }

                                            if (arrRtbeValue.Length > 1)
                                            {
                                                ((CheckBoxExtended)ct[0]).CreateUser = arrRtbeValue[1];
                                            }
                                            #endregion
                                        }
                                        else if (ct[0] is ComboBoxExtended)
                                        {
                                            #region ComboBoxExtended                                       
                                            arrRtbeValue = xe.Value.Split('¤');

                                            ((ComboBoxExtended)ct[0]).Text = arrRtbeValue[0];

                                            if (arrRtbeValue.Length > 1)
                                            {
                                                ((ComboBoxExtended)ct[0]).CreateUser = arrRtbeValue[1];
                                            }
                                            #endregion
                                        }
                                        else if (ct[0] is LabelExtended)
                                        {
                                            #region LabelExtended                                       
                                            if (ct[0] != _PageNo)
                                            {
                                                arrRtbeValue = xe.Value.Split('¤');

                                                ((LabelExtended)ct[0]).Text = arrRtbeValue[0];
                                                //基本信息只有文字，如果有¤，表示是手动强行修改的，不会再自动更新的
                                                if (arrRtbeValue.Length > 1)
                                                {
                                                    ((LabelExtended)ct[0]).Tag = ((LabelExtended)ct[0]).Tag.ToString() + "¤";
                                                    //((LabelExtended)ct[0]).Tag = ((LabelExtended)ct[0]).Text + "¤";
                                                }

                                                //调整坐标位置，纵向居中
                                                ((LabelExtended)ct[0]).Location = GetVariableVerticalCenterLocation(((LabelExtended)ct[0]));
                                            }
                                            #endregion
                                        }
                                        else if (ct[0] is VectorControl)
                                        {
                                            #region VectorControl                                       
                                            if (!string.IsNullOrEmpty(xe.Value))
                                            {
                                                //arrRtbeValue = xe.Value.Split('§');
                                                ((VectorControl)ct[0]).VectorImageInfo.PicId = xe.Value;
                                                //((VectorControl)ct[0])._VectorConfig = arrRtbeValue[0];
                                                //((VectorControl)ct[0])._ImageBorder = arrRtbeValue[1];

                                                InitVector(true, ((VectorControl)ct[0]), true);
                                            }
                                            #endregion
                                        }
                                        else if (ct[0] is Vector)
                                        {
                                            #region Vector
                                            ((Vector)ct[0]).LoadData(xe.Value);
                                            #endregion
                                        }

                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowInforMsg(ex.ToString(), true);
                            throw ex;
                        }
                        ct = null;
                        picNew = null;
                        showText = null;
                        #endregion
                    }

                    thisPage = null;
                    arrRtbeValue = null;
                    root = null;
                    start = null;

                    //2.表格部分 解析xml后，对_TableInfor进行赋值设置
                    LoadTable(tableInforPara, false);

                }
                else
                {
                    ////
                    //if (_TableType && tableInforPara != null) // 根据解析本页的数据，形成本页的表格，进行绘制。这里还是默认的表格线。
                    //{
                    //    //绘制自己定义的表格的所有边框线：难点在于行、单元个的编辑后的线样式，数据保存到xml后再解析呈现
                    //    DrawTableBaseLines(g);
                    //}
                }

            }
            catch (Exception ex)
            {
                ShowInforMsg(ex.StackTrace, true);
                throw ex;
            }
        }
        # endregion 载入本页数据

        # region 重置表单界面上的数据内容
        /// <summary>所有控件恢复默认值
        /// 重设默认值
        /// </summary>
        private void ReSetDefaultValue()
        {
            ArrayList delControlList = new ArrayList();

            try
            {
                if (this.pictureBoxBackGround.Controls.Count > 0)
                {
                    //界面上删除所有的页数据：比如添加的图片
                    foreach (Control control in this.pictureBoxBackGround.Controls)
                    {
                        if (control.Name == "lblShowSearch")
                        {
                            //是搜索图标图片，跳过   
                            //this.pictureBoxBackGround.Controls.Remove(this.lblShowSearch); //先移除这个搜索提示控件
                            continue;
                        }

                        if (control is RichTextBoxExtended)
                        {
                            ((RichTextBoxExtended)control).ResetDefault();
                            //((RichTextBoxExtended)control).Text = ((RichTextBoxExtended)control)._Default; //输入框创建页的时候，直接显示默认值，如果已经创建好页，需要点击进入才能有默认值
                        }
                        else if (control is TransparentRTBExtended)  //TransparentRTBExtended是继承RichTextBoxExtended的 ,所以这里注释掉也可以
                        {
                            ((TransparentRTBExtended)control).ResetDefault();
                        }
                        else if (control is CheckBoxExtended)
                        {
                            ((CheckBoxExtended)control).ResetDefault(); //默认是否勾上
                        }
                        else if (control is ComboBoxExtended)
                        {
                            ((ComboBoxExtended)control).ResetDefault();
                        }
                        else if (control is LabelExtended)
                        {
                            ((LabelExtended)control).ResetDefault();

                            if (control.Name != "_PageNo")
                            {
                                control.Tag = ((LabelExtended)control)._Size;                           //control.Tag = null; //如果是床号等基本信息，那么每页的手动修改不一定一样
                                ((LabelExtended)control).Location = ((LabelExtended)control)._Location; //恢复模板设置的坐标位置
                            }
                            else
                            {
                                //清空tag，否则手动修改一页后，继续新建页，始终有tag，都会认为每一页手动修改过的，保存到PageNoStart="1@3,6@500,7@101"中
                                control.Tag = ((LabelExtended)control)._Size;
                            }
                        }
                        else if (control is Vector)
                        {
                            //精简矢量图
                            ((Vector)control).ResetClear();  //清空所有子控件（图标）
                        }
                        else if (control is GeneticMapControl)
                        {
                            //遗传图谱
                            ((GeneticMapControl)control).ResetClear();  //清空数据
                        }
                        else if (control is PictureBox)
                        {
                            ////((PictureBox)control).ResetDefault();
                            if (((PictureBox)control).Tag != null)
                            {
                                if (control is VectorControl)
                                {
                                    ((VectorControl)control).BackgroundImage = (Image)((VectorControl)control).VectorImageInfo.OriginalIamge; //重置为只有底图的
                                }
                                else
                                {
                                    //control.Dispose(); //这里删除不掉，因为在遍历循环中
                                    delControlList.Add(control);
                                }
                            }
                        }
                        else if (control is MonthCalendar)
                        {
                            //日历控件跳过
                            control.Dispose();  //释放掉，提高系统性能
                        }
                        else
                        {
                            //没有指定的控件
                            ShowInforMsg("在控件进行赋默认值的时候，遇到无法识别的类型：" + control.ToString()); //Label：System.Windows.Forms.Label, Text: 调试科别名字
                        }
                    }
                }

                for (int i = delControlList.Count - 1; i >= 0; i--)
                {
                    ((Control)delControlList[i]).Dispose();
                }

                # region //表格控制对象_TableInfor也需要重置。
                //先重置后，再赋值；否则不保存再次更新此页，修改的地方还是残留着

                if (_TableType && _TableInfor != null)
                {
                    ReSetTableInfor();
                }

                # endregion //表格控制对象_TableInfor也需要重置。

                pictureBoxBackGround.Image = (Bitmap)_BmpBack.Clone();

            }
            catch (Exception ex)
            {
                ShowInforMsg(ex.StackTrace, true);
                throw ex;
            }
        }

        /// <summary>
        /// 重置表格
        /// </summary>
        private void ReSetTableInfor()
        {
            if (_TableType && _TableInfor != null)
            {
                int fieldIndex = 0;

                for (int field = 0; field < _TableInfor.GroupColumnNum; field++)
                {
                    fieldIndex = _TableInfor.RowsCount * field;

                    for (int i = 0; i < _TableInfor.RowsCount; i++)
                    {
                        for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                        {
                            RestCell(_TableInfor, _TableInfor.Cells[i + fieldIndex, j]); //RestCell(_TableInfor.Cells[i + fieldIndex, j]);
                        }

                        _TableInfor.Rows[i + fieldIndex].UserID = "";
                        _TableInfor.Rows[i + fieldIndex].UserName = "";
                        _TableInfor.Rows[i + fieldIndex].NurseFormLevel = "";

                        _TableInfor.Rows[i + fieldIndex].RowForeColor = "";
                        _TableInfor.Rows[i + fieldIndex].RowLineColor = Color.Black;
                        _TableInfor.Rows[i + fieldIndex].CustomLineType = LineTypes.None;
                        _TableInfor.Rows[i + fieldIndex].CustomLineColor = _RowLineType_Color;

                        _TableInfor.Rows[i + fieldIndex].SumType = "";
                    }
                }
            }

        }

        /// <summary>
        /// 添加页的时候，所有的输入框显示默认值；已经保存过的页，如果为空，需要光标点入才能显示
        /// </summary>
        private void InputBoxShowDefaultAtAddPage()
        {
            return;
            if (this.pictureBoxBackGround.Controls.Count > 0)
            {
                //界面上删除所有的页数据：比如添加的图片
                foreach (Control control in this.pictureBoxBackGround.Controls)
                {
                    if (control.Name == "lblShowSearch")
                    {
                        //是搜索图标图片，跳过   
                        continue;
                    }

                    if (control is RichTextBoxExtended)
                    {
                        if (((RichTextBoxExtended)control)._Default != "" && ((RichTextBoxExtended)control).Visible && !((RichTextBoxExtended)control)._IsTable)
                        {
                            HorizontalAlignment temp = ((RichTextBoxExtended)control).SelectionAlignment;

                            ((RichTextBoxExtended)control).Text = ((RichTextBoxExtended)control)._Default; //输入框创建页的时候，直接显示默认值，如果已经创建好页，需要点击进入才能有默认值

                            if (((RichTextBoxExtended)control).Multiline)
                            {
                                ((RichTextBoxExtended)control).WordWrapAutoLine_TextChanged(); //比如表头，纵向的，赋值的时候就要调整行
                            }

                            ((RichTextBoxExtended)control).SelectAll();
                            ((RichTextBoxExtended)control).SelectionAlignment = temp;
                        }
                    }
                }
            }
        }
        # endregion 重置表单界面上的数据内容

        #region 勾选框 三种状态
        ContextMenuStrip contextMenuStripSkinCheckBox = null;

        private void toolStripMenuItemChecked_Click(object sender, EventArgs e)
        {
            //ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;
            CheckBox cb = (CheckBox)contextMenuStripSkinCheckBox.SourceControl;
            cb.CheckState = CheckState.Checked;
        }

        private void toolStripMenuItemIndeterminate_Click(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)contextMenuStripSkinCheckBox.SourceControl;
            cb.CheckState = CheckState.Indeterminate;
        }

        private void toolStripMenuItemUnChecked_Click(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)contextMenuStripSkinCheckBox.SourceControl;
            cb.CheckState = CheckState.Unchecked;
        }

        private void SetSkinCheckBoxMenu(CheckBoxExtended chkbNew)
        {
            //三种状态的右击菜单菜单事件
            if (chkbNew.ThreeState)
            {
                chkbNew.CheckStateChanged += new System.EventHandler(this.Script_CheckedStateChanged); //也要触发值改变，提示保存

                if (contextMenuStripSkinCheckBox == null)
                {
                    contextMenuStripSkinCheckBox = new ContextMenuStrip();

                    ToolStripMenuItem toolStripMenuItemChecked = new ToolStripMenuItem();
                    toolStripMenuItemChecked.Name = "toolStripMenuItemChecked";
                    toolStripMenuItemChecked.Size = new System.Drawing.Size(152, 22);
                    toolStripMenuItemChecked.Text = "勾";
                    toolStripMenuItemChecked.Click += new System.EventHandler(this.toolStripMenuItemChecked_Click);

                    ToolStripMenuItem toolStripMenuItemUnChecked = new ToolStripMenuItem();
                    toolStripMenuItemUnChecked.Name = "toolStripMenuItemUnChecked";
                    toolStripMenuItemUnChecked.Size = new System.Drawing.Size(152, 22);
                    toolStripMenuItemUnChecked.Text = "空";
                    toolStripMenuItemUnChecked.Click += new System.EventHandler(this.toolStripMenuItemUnChecked_Click);

                    ToolStripMenuItem toolStripMenuItemIndeterminate = new ToolStripMenuItem();
                    toolStripMenuItemIndeterminate.Name = "toolStripMenuItemIndeterminate";
                    toolStripMenuItemIndeterminate.Size = new System.Drawing.Size(152, 22);
                    toolStripMenuItemIndeterminate.Text = "叉";
                    toolStripMenuItemIndeterminate.Click += new System.EventHandler(this.toolStripMenuItemIndeterminate_Click);

                    contextMenuStripSkinCheckBox.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                    toolStripMenuItemChecked,
                                    toolStripMenuItemIndeterminate,
                                    toolStripMenuItemUnChecked});
                }

                chkbNew.ContextMenuStrip = contextMenuStripSkinCheckBox;
            }
        }

        /// <summary>
        /// 三种状态的勾选框，赋值
        /// </summary>
        private void LoadSkinCheckBox(CheckBoxExtended chkbNew, string[] arrRtbeValue)
        {
            //如果目前是打叉Indeterminate，在Checked设置为true，还是打叉的状态。设置为flase为Unchecked状态。
            if (chkbNew.ThreeState) //此处和载入数据一样的逻辑
            {
                bool tempBool = false;

                if (arrRtbeValue.Length > 2)
                {
                    if (arrRtbeValue[2] == "Indeterminate")
                    {
                        //过敏史无="False¤" 过敏史有="False¤Mandalat¤Indeterminate"
                        chkbNew.CheckState = CheckState.Indeterminate; //选中：假False
                    }
                    else if (arrRtbeValue[2] == "Checked")
                    {
                        chkbNew.CheckState = CheckState.Checked; //选中：真
                    }
                    else if (arrRtbeValue[2] == "Unchecked")
                    {
                        chkbNew.CheckState = CheckState.Unchecked; //默认：未选
                    }
                    else
                    {
                        //如果目前是打叉Indeterminate，在Checked设置为true，还是打叉的状态。设置为flase为Unchecked状态。
                        if (!bool.TryParse(arrRtbeValue[0], out tempBool))
                        {
                            tempBool = false;
                        }

                        if (tempBool)
                        {
                            chkbNew.CheckState = CheckState.Checked;
                        }
                        else
                        {
                            chkbNew.CheckState = CheckState.Indeterminate;
                        }

                        //chkbNew.CheckState = chkbNew._DefaultCheckState;
                    }
                }
                else
                {
                    //如果数据没有保存（修改模板过程中）
                    //如果目前是打叉Indeterminate，在Checked设置为true，还是打叉的状态。设置为flase为Unchecked状态。
                    if (!bool.TryParse(arrRtbeValue[0], out tempBool))
                    {
                        tempBool = false;
                    }

                    chkbNew.Checked = tempBool;

                    if (tempBool)
                    {
                        chkbNew.CheckState = CheckState.Checked;
                    }
                    else
                    {
                        chkbNew.CheckState = CheckState.Indeterminate;
                    }

                    //chkbNew.Checked = tempBool;
                    //chkbNew.CheckState = chkbNew._DefaultCheckState; //不能用默认的状态，否则之前修改模板保存的数据，变成未选了，应该是打叉

                }
            }
            else
            {
                chkbNew.Checked = bool.Parse(arrRtbeValue[0]);
            }
        }

        #endregion 勾选框 三种状态

        # region 多行输入框，程序控制自动换行，手动的换行有问题：和打印不一致
        /// <summary> 
        /// 多行输入框，自动换行处理。输入框自带的换行，回合打印的时候起冲突
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellsRtbeWordWrapAutoLine_TextChanged(object sender, EventArgs e)
        {
            if (_IsLoading)
            {
                return;
            }

            RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;

            if (isAdjusting)
            {
                return;
            }
            else
            {
                isAdjusting = true;
            }

            rtbe.WordWrapAutoLine_TextChanged(); //英文或者数字只走一次，但是如果中文输入法：连个子，会先走一遍，然后每个字格走一遍。

            isAdjusting = false;
        }
        # endregion 多行输入框，程序控制自动换行，手动的换行有问题：和打印不一致

        # region 内容改变，自动进行调整段落列
        /// <summary>
        /// 输入框内容改变，自动进行调整段落列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool _IsTextChangedAdjust = false; //用来标记是否正在由于文本发生改变触发调整中（如果调整还会再次本事件）
        private void CellsRtbeAdjustParagraphs_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_IsLoading || _IsNotNeedCellsUpdate || _TableInfor.CurrentCell == null || isAdjusting)
                {
                    return;
                }

                //选择了不自动换行调整
                if (!GlobalVariable.InputAutoAdjustParagraphs)
                {
                    return;
                }

                RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;

                //可用属性控制，是否在输入的时候就自动进行调整；还是写入后再点击按钮进行手动调整
                _IsTextChangedAdjust = true;

                RankColumnText(rtbe, false);

                _IsTextChangedAdjust = false;
            }
            catch (Exception ex)
            {
                _IsTextChangedAdjust = false;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }
        # endregion 内容改变，自动进行调整段落列

        # region 查看段落内容
        /// <summary>
        /// 输入框控件，回调主窗体的查看段落内容事件(光标所在行开始的，所有的该段落的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rtbe_ShowParagraphContent_MainForm(object sender, EventArgs e)
        {
            try
            {
                if (_TableInfor.CurrentCell == null)
                {
                    return;
                }

                //签名权限控制
                //如果开启：签名后只读的开关
                bool rightOk = true;
                if (_DSS.IntensiveCareSignedReadOnly)
                {
                    if (_TableInfor.Rows[_CurrentCellIndex.X].UserID != "")
                    {
                        if (GlobalVariable.LoginUserInfo.UserCode != _TableInfor.Rows[_CurrentCellIndex.X].UserID)
                        {
                            //rightOk = !_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.ReadOnly;
                            //rightOk = false;  
                            //高级的护士长等，还是需要可以调整的
                            if (GetNurseLevel(GlobalVariable.LoginUserInfo.TitleName) > GetNurseLevel(_TableInfor.Rows[_CurrentCellIndex.X].NurseFormLevel))
                            {
                                rightOk = true;
                            }
                            else
                            {
                                rightOk = false;
                            }

                            //MessageBox.Show("没有修改权限，无法进行调整段落列。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            //return;
                        }
                        else
                        {
                            rightOk = true;
                        }
                    }
                }

                this.Cursor = Cursors.WaitCursor;

                string content = _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Text; //默认为当前行内容

                //根据后台XMl数据，获取到该段落的内容
                XmlNode node = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                int nodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID;
                XmlNode cellNode = node.SelectSingleNode("Record[@ID='" + nodeId.ToString() + "']");

                if (cellNode != null)
                {
                    //获取本段落，下一行开始的所有段落内容
                    content += GetParagraphContent();

                    Frm_ParagraphContent pcf = new Frm_ParagraphContent(content, rightOk);
                    pcf.LoadSelectWord(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE._HelpString);
                    DialogResult res = pcf.ShowDialog();

                    if (res == DialogResult.OK)
                    {
                        try
                        {
                            if (!_TableType || _TableInfor.CurrentCell == null || _Need_Assistant_Control == null || _Need_Assistant_Control.IsDisposed)
                            {
                                //string msg = "请先选择：需要进行调整段落的单元格。";
                                //MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            RichTextBoxExtended rtbe = (RichTextBoxExtended)_Need_Assistant_Control;

                            if (rtbe._AdjustParagraphs)
                            {
                                string currentNodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString();


                                //性能速度的影响：经过调试发现，输入框的当前的输入法是英文-快，中文半角慢一些，中文全角很慢
                                //IMEClass.SetHalfShape(rtbe.Handle); //这个效率比较低，改用下面的设置输入法半角
                                //解决Tab键切换，还是全角的问题 （都是在Enter中处理）
                                if (this.ImeMode != ImeMode.Hangul)
                                {
                                    this.ImeMode = ImeMode.Hangul;
                                }

                                if (_IsLoading || _IsNotNeedCellsUpdate || _TableInfor.CurrentCell == null)
                                {
                                    return;
                                }

                                if (isAdjusting)
                                {
                                    return;
                                }
                                else
                                {
                                    isAdjusting = true;
                                }

                                this.SuspendLayout();
                                this.pictureBoxBackGround.SuspendLayout();
                                _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.SuspendLayout();

                                //有可能输入框中的内容，已经和表格信息单元格中的修改后不一样了，所以更新一下
                                _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].Text = rtbe.Text;

                                int x = _CurrentCellIndex.X, y = _CurrentCellIndex.Y;

                                this.pictureBoxBackGround.SuspendLayout();
                                _IsNotNeedCellsUpdate = true;//不更新cell，不自动保存，不刷新单元格
                                isClickAdjusting = true; // 控制没有必要的刷新
                                bool isNotSameDateTimeToPreRow = false;

                                bool isHaveTimeCol = true;
                                if (_TableInfor.CellByRowNo_ColName(x, "时间") == null)
                                {
                                    isHaveTimeCol = false;
                                }

                                bool isHaveDateCol = true;
                                if (_TableInfor.CellByRowNo_ColName(x, "日期") == null)
                                {
                                    isHaveDateCol = false;
                                }

                                #region 将这一段落的数据内容清空
                                //之前点击工具栏进行调整的时候，不对调整多余行清空（调整前行多，调整后行少，并且跨页的情况下）。
                                //这样会导致多出了行，其实应该变成空白行，更为合理，保存再去掉空白行，更加方便。
                                //应该在确定调整段落后，现将原来的段落行的内容清空比较合理。
                                //将同一段落全部设置为空，否则后面跨页的的多余行的数据还会存在。
                                ClearParagraphContent();

                                #endregion 将这一段落的数据内容清空

                                //首先，将光标行及往后行的文字都合并到一起，然后清空，再根据修改的段落内容进行调整段落行
                                for (int i = x + 1; i < _TableInfor.RowsCount * _TableInfor.GroupColumnNum; i++)
                                {
                                    //判断是否本段落
                                    isNotSameDateTimeToPreRow = NotSameDateTimeToPreRow(i);

                                    if (((isHaveTimeCol && (!isHaveDateCol && string.IsNullOrEmpty(_TableInfor.CellByRowNo_ColName(i, "时间").Text))) || !isNotSameDateTimeToPreRow)
                                         || ((isHaveDateCol && !isHaveTimeCol && string.IsNullOrEmpty(_TableInfor.CellByRowNo_ColName(i, "日期").Text)) || !isNotSameDateTimeToPreRow)
                                        || (!isHaveTimeCol && !isHaveDateCol && _TableInfor.Cells[i, _CurrentCellIndex.Y].Text != ""))
                                    {
                                        //_TableInfor.Cells[x, y].Text += _TableInfor.Cells[i, _CurrentCellIndex.Y].Text.Trim();

                                        //更新下面的那个输入框，要先清空
                                        _TableInfor.Cells[i, _CurrentCellIndex.Y].Text = "";
                                        _TableInfor.Cells[i, _CurrentCellIndex.Y].Rtf = "";
                                        _TableInfor.Cells[i, _CurrentCellIndex.Y].IsRtf_OR_Text = false;

                                        ShowCellRTBE(i, _CurrentCellIndex.Y); //这里切换单元格的时候，其实不需要进行刷新的：用isClickAdjusting来控制

                                        _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.OldRtf = null;    //触发保存，防止内容上移合并调整后，后面空的单元格的老数据还在

                                        SaveCellToXML(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE);
                                    }
                                    else
                                    {
                                        break; //仅仅把本页的进行调整
                                    }
                                }

                                isClickAdjusting = false;
                                _IsNotNeedCellsUpdate = false;
                                this.pictureBoxBackGround.ResumeLayout();

                                ShowCellRTBE(x, y); //重置目标单元格为选中状态

                                //更新内容，然后调整
                                rtbe.Text = pcf.Content;

                                isAdjusting = false;
                                RankColumnText(rtbe, true); //最后指定调整段落行计算的处理

                                this.ResumeLayout();
                                this.pictureBoxBackGround.ResumeLayout();
                                _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.ResumeLayout();

                                //ShowInforMsg(string.Format("{0:yyyy-MM-dd HH:mm:ss:ffff}", GetServerDate()));


                                //调整段落列后，签名保持在最后一行
                                SetParagraphSign(currentNodeId);  // string currentNodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString();


                                //因为弹出窗口的调整段落列，可能导致页数变少
                                #region 根据行数计算正确的页数，防止保存去除空白行或者调整段罗列后，导致最后有空白页n页
                                //if (_TableType && _TableInfor != null)  // && _AutoSortMode == "2" && _SaveAutoSort有些表格本来就是有空行的，不能去除空行；而且最后一页可能就是空页进行保存的。
                                //{
                                //    XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));

                                //    int rowCount = 0;
                                //    int rightPageCount = TableClass.GetPageCount_HaveTable(_TemplateXml, recordsNode.ChildNodes.Count, ref rowCount);
                                //    if (rightPageCount < pages)
                                //    {
                                //        ShowInforMsg("保存时遇到最后有空页，删除了空白页。", false);
                                //        pages = rightPageCount;
                                //        string page = "";
                                //        for (int i = _CurrentTemplateNameTreeNode.Nodes.Count - 1; i >= 0; i--)
                                //        {
                                //            page = _CurrentTemplateNameTreeNode.Nodes[i].Text;

                                //            if (int.Parse(page) > rightPageCount)
                                //            {
                                //                _CurrentTemplateNameTreeNode.Nodes[i].Remove();
                                //            }
                                //            else if (int.Parse(page) == rightPageCount)
                                //            {
                                //                //选中
                                //                //this.treeViewPatients.SelectedNode = _CurrentTemplateNameTreeNode.Nodes[i]; //切换表单离开保存的时候就会有问题了。
                                //            }
                                //        }

                                //        if (_NeedSaveSuccessMsg && !_LordClosing)
                                //        {
                                //            //打开最后一页，并移除多余的节点
                                //            if (int.Parse(_CurrentPage) > rightPageCount)
                                //            {
                                //                _CurrentPage = rightPageCount.ToString();

                                //                //打开刷新：
                                //                //重新按照排序后的数据显示----------------------------------------------------------------
                                //                //当前页，重新载入表格，并重绘，选中新位置的行
                                //                ReSetTableInfor();

                                //                LoadTable(_TableInfor, false);

                                //                _IsLoading = true;
                                //                Redraw();
                                //                _IsLoading = false;
                                //                //重新按照排序后的数据显示----------------------------------------------------------------
                                //            }
                                //        }
                                //    }
                                //}
                                #endregion 根据行数计算正确的页数，防止保存去除空白行或者调整段罗列后，导致最后有空白页n页
                            }
                            else
                            {
                                ShowInforMsg("该列没有设置为调整段落列，不能进行调整段落。");
                            }
                        }
                        catch (Exception ex)
                        {
                            isClickAdjusting = false;
                            _IsNotNeedCellsUpdate = false;

                            this.ResumeLayout();
                            this.pictureBoxBackGround.ResumeLayout();
                            _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.ResumeLayout();

                            Comm.LogHelp.WriteErr(ex);
                            throw ex;
                        }
                    }
                }
                else
                {
                    ShowInforMsg("查看段落内容时，发现存在异常，无法定位数据所在行。", false);
                }

                this.Cursor = Cursors.Default;

            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取本段落内容
        /// 从当前单元格所在行的，下一行开始，到本段落末尾
        /// </summary>
        /// <param name="xnPara">当前行对应的xml数据的节点</param>
        /// <param name="nodeId">当前行ID</param>
        /// <param name="nodeId">调整段列的列名</param>
        /// <returns></returns>
        private string GetParagraphContent()
        {
            string content = "";

            string columnName = _TableInfor.Columns[_CurrentCellIndex.Y].ColName;

            //根据后台XMl数据，获取到该段落的内容
            XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            int nodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID;
            //"Record[@ID='" + nodeId.ToString() + "']"
            XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));

            string currentDateTime = GetRowDateTime(nodeId.ToString());
            string eachDateTime = "";
            cellNode = cellNode.NextSibling;    //如果当前节点已经是最前面的节点，那么返回null

            while (cellNode != null)
            {
                eachDateTime = GetRowDateTime((cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                if (currentDateTime == eachDateTime)
                {
                    content += (cellNode as XmlElement).GetAttribute(columnName).Split('¤')[0];

                    cellNode = cellNode.NextSibling;    //如果当前节点已经是最前面的节点，那么返回null
                }
                else
                {
                    cellNode = null;
                    break;
                }

                //防止内容过长，尤其没有日期和时间列的表格，所以超过指定字数就停止
                if (content.Length > 6000)
                {
                    break;
                }
            }

            return content;
        }

        /// <summary>
        /// 清空段落内容
        /// </summary>
        private void ClearParagraphContent()
        {
            //string content = "";
            StringBuilder sb = new StringBuilder();

            string columnName = _TableInfor.Columns[_CurrentCellIndex.Y].ColName;

            //根据后台XMl数据，获取到该段落的内容
            XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            int nodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID;
            XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));

            string currentDateTime = GetRowDateTime(nodeId.ToString());
            string eachDateTime = "";
            cellNode = cellNode.NextSibling;    //如果当前节点已经是最前面的节点，那么返回null

            while (cellNode != null)
            {
                eachDateTime = GetRowDateTime((cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                if (currentDateTime == eachDateTime)
                {
                    //content += (cellNode as XmlElement).GetAttribute(columnName).Split('¤')[0];
                    sb.Append((cellNode as XmlElement).GetAttribute(columnName).Split('¤')[0]);

                    if (!string.IsNullOrEmpty((cellNode as XmlElement).GetAttribute(columnName).Split('¤')[0].Trim()))
                    {
                        (cellNode as XmlElement).SetAttribute(columnName, "");
                    }

                    cellNode = cellNode.NextSibling;    //如果当前节点已经是最前面的节点，那么返回null
                }
                else
                {
                    cellNode = null;
                    break;
                }

                //防止内容过长，尤其没有日期和时间列的表格，所以超过指定字数就停止
                if (sb.ToString().Length > 6000)
                {
                    break;
                }
            }

            //return sb.ToString();
        }

        # endregion 查看段落内容

        # region 调整段落列，主处理方法

        private void RankColumnText(RichTextBoxExtended rtbe, bool notMergeNextPageContent)
        {
            //构造方法，需要跳转到新页
            RankColumnText(rtbe, notMergeNextPageContent, true);
        }

        bool isAdjusting = false;       //标记是否正在调整段落列
        bool isClickAdjusting = false;  //标记是否正在点击按钮多连续多行进行调整，合并过程中，无需刷新

        XmlNode _LastNodeAdjustParagraphs = null;

        /// <summary>
        /// 调整段落列方法
        /// 
        /// </summary>
        /// <param name="rtbe"></param>
        /// <param name="mergeNextPageContent">一般为False，只有在弹出调整段落界面调用的时候不用合并原来内容，否则重复了</param>
        /// <param name="needGoToNextPage">如果调整到下一页，是否需要跳转打开新页</param>
        private bool RankColumnText(RichTextBoxExtended rtbe, bool notMergeNextPageContent, bool needGoToNextPage)
        {
            _LastNodeAdjustParagraphs = null;
            //XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            //_LastNodeAdjustParagraphs = recordsNode.SelectSingleNode("Record[@ID='" + _TableInfor.Rows[_CurrentCellIndex.X + 1].ID.ToString() + "']");

            bool retCreatPage = false;

            //如果在调整段落列过程中，不要再次死循环触发调整
            if (isAdjusting)
            {
                return false;
            }
            else
            {
                isAdjusting = true;
            }

            //TODO:先判断：是增加还是删除文字。如果是增加-换行处理，如果是删除-下面行的文字跟过来
            //这步不做了。这种效果刷新太频繁，界面体验效果会不好
            int currentRow = _CurrentCellIndex.X;

            string rowHearStr = _RowHeaderIndent;  // "　　"; //段落的首行，要行首空两个字符的空白



            string thisRowDate = "";
            bool isHaveDateCol = true;
            if (_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "日期") != null)
            {
                thisRowDate = _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "日期").Text;
                isHaveDateCol = true;
            }
            else
            {
                thisRowDate = string.Format("{0:yyyy-MM-dd}", GetServerDate());  //或者去本页的输入框名字为日期的值
                isHaveDateCol = false;
            }

            string thisRowTime = "";
            bool isHaveTimeCol = true;
            if (_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间") == null)
            {
                thisRowTime = "12:00";
                isHaveTimeCol = false;
            }
            else
            {
                thisRowTime = _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间").Text; //调整段落列，必须要有时间，不然报错

                //护理部要求写入24:00
                //thisRowTime = thisRowTime == "24:00" ? "23:59" : thisRowTime;
            }

            //判断是不是调整段落的第一行，所以第一行根据配置，可能要有两个空格的缩进CellByRowNo_ColName
            //如果不为空，还需要判断和上一行的时间，是否一样。如果一样，那么还是一个段落
            bool isNotSameDateTimeToPreRow = NotSameDateTimeToPreRow(_CurrentCellIndex.X);

            //如果不为空，还需要判断和上一行的时间，是否一样。如果一样，那么还是一个段落
            if (!string.IsNullOrEmpty(thisRowTime) && isNotSameDateTimeToPreRow)  //翻页的时候，严谨的判断：时间为空或者日期时间都相同，都认为一个段落
            {
                rowHearStr = _RowHeaderIndent; //如果是本页第一行，那么可能是上一页，某段落的行，但是也显示时间，所以还要继续判断
            }
            else
            {
                rowHearStr = "";
            }

            //是不是已经有了开头缩进的空格，比如手动敲的空格。医务人员手动的始终应该尊总，优先……
            string halfIndent = StrHalfFull.ToDBC_Half(_RowHeaderIndent);//全角转换成半角
            if (rtbe.Text.StartsWith(_RowHeaderIndent) || rtbe.Text.StartsWith(halfIndent))
            {
                rowHearStr = "";
            }

            if (_IsTextChangedAdjust) //如果是手动修改输入框内容发生改变触发的
            {
                //光标在最后位置；只要是在单元格最后位置就自动调整，否则不自动，手动按钮调整
                if (rtbe.SelectionStart != rtbe.Text.Length || rtbe.Text.Length < rtbe.OldText.Length) //如果文字变少，那么就是删除。变多是输入
                {
                    #region 原来的处理方式，只有光标在单元格最后，才会调整
                    //如果是调整好的，修改其中某行，那么不需要调整，修改后手动点击按钮调整；否则光标迁移到下一行，打字错位
                    isAdjusting = false;
                    return false;
                    #endregion

                    #region 实时刷新 调整段落行（删除，修改，增加）。刷新效果和，跨页删除的时候还有问题。
                    ////（输入文字，删除文字）：获取当前行开始的所有段落内容后，再进行调整
                    //Point currentCell = _CurrentCellIndex;
                    //int focusIndex = rtbe.SelectionStart;
                    //string content = rtbe.Text;
                    //content += GetParagraphContent();

                    ////----------清空下面的段落----------------
                    //this.pictureBoxBackGround.SuspendLayout();
                    //_IsNotNeedCellsUpdate = true;   //不更新cell，不自动保存，不刷新单元格
                    //isClickAdjusting = true;        // 控制没有必要的刷新

                    //if (_TableInfor.CellByRowNo_ColName(currentCell.X, "时间") == null)
                    //{
                    //    isHaveTimeCol = false;
                    //}

                    //if (_TableInfor.CellByRowNo_ColName(currentCell.X, "日期") == null)
                    //{
                    //    isHaveDateCol = false;
                    //}

                    //string rowInforPara = "";
                    //string value = "";
                    ////根据后台XMl数据，获取到该段落的内容
                    //XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                    //int nodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID;
                    //XmlNode cellNode = recordsNode.SelectSingleNode("Record[@ID='" + nodeId.ToString() + "']");

                    //_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Text = "";

                    ////首先，将光标行及往后行的文字都合并到一起，然后清空，再根据修改的段落内容进行调整段落行
                    //for (int i = currentCell.X + 1; i < _TableInfor.RowsCount * _TableInfor.GroupColumnNum; i++)
                    //{
                    //    //判断是否本段落
                    //    isNotSameDateTimeToPreRow = NotSameDateTimeToPreRow(i);

                    //    if (((isHaveTimeCol && (!isHaveDateCol && string.IsNullOrEmpty(_TableInfor.CellByRowNo_ColName(i, "时间").Text))) || !isNotSameDateTimeToPreRow)
                    //         || ((isHaveDateCol && !isHaveTimeCol && string.IsNullOrEmpty(_TableInfor.CellByRowNo_ColName(i, "日期").Text)) || !isNotSameDateTimeToPreRow)
                    //        || (!isHaveTimeCol && !isHaveDateCol && _TableInfor.Cells[i, _CurrentCellIndex.Y].Text != ""))
                    //    {
                    //        //更新下面的那个输入框，要先清空
                    //        _TableInfor.Cells[i, currentCell.Y].Text = "";
                    //        _TableInfor.Cells[i, currentCell.Y].Rtf = "";
                    //        _TableInfor.Cells[i, currentCell.Y].IsRtf_OR_Text = false;

                    //        //this.pictureBoxBackGround.SuspendLayout();
                    //        _IsLoading = true; //防止更新文字工具栏的粗体等状态

                    //        nodeId = _TableInfor.Rows[i].ID;
                    //        cellNode = recordsNode.SelectSingleNode("Record[@ID='" + nodeId.ToString() + "']");
                    //        value = GetCellSaveValue(_TableInfor.Columns[currentCell.Y].RTBE, _TableInfor.Cells[i, currentCell.Y], ref rowInforPara);
                    //        (cellNode as XmlElement).SetAttribute(_TableInfor.Columns[currentCell.Y].RTBE.Name, value); //单元格信息

                    //        _TableInfor.Columns[currentCell.Y].RTBE.Location = _TableInfor.Cells[i, currentCell.Y].RtbeLocation();
                    //        ReSetCellBackGround(_TableInfor.Columns[currentCell.Y].RTBE); //防止删除的时候，为空了，不重置为空白，还显示最后的文字
                    //        RedrawRow(_TableInfor, i, true, cellNode);  //整个刷新太慢 Redraw(); //刷新整行颜色

                    //        _IsLoading = false;
                    //        //this.pictureBoxBackGround.ResumeLayout();
                    //    }
                    //    else
                    //    {
                    //        break; //仅仅把本页的进行调整
                    //    }
                    //}

                    //isClickAdjusting = false;
                    //_IsNotNeedCellsUpdate = false;
                    //this.pictureBoxBackGround.ResumeLayout();
                    ////----------清空下面的段落----------------

                    ////调整
                    ////更新内容，然后调整
                    //ShowCellRTBE(currentCell.X, currentCell.Y);
                    //rtbe.Text = content;
                    //isAdjusting = false;
                    //_IsTextChangedAdjust = false;
                    //RankColumnText(rtbe, true); //最后指定调整段落行计算的处理

                    //ShowCellRTBE(currentCell.X, currentCell.Y);

                    //if (focusIndex - rowHearStr.Length >= 0)
                    //{
                    //    rtbe.Select(focusIndex - rowHearStr.Length, 0);
                    //}
                    //else
                    //{
                    //    rtbe.Select(focusIndex, 0);
                    //}

                    //isAdjusting = false;
                    //return;
                    #endregion 实时刷新
                }
            }

            string inputStr = rtbe.Text;
            string processStr = WordWrap.lineStringAnalys(inputStr, rtbe.Width + rtbe.Margin.Right, rtbe.Font, rowHearStr);
            string[] arr = processStr.Split(new string[] { "¤" }, StringSplitOptions.RemoveEmptyEntries); //;(new string[] { @"\r\n" }, StringSplitOptions.RemoveEmptyEntries); //一二三123456，7四五，六七八九ABCD，

            if (arr.Length > 1) //超过一行
            {
                //--现在有一个问题：如果选择的调整的开始行（点击按钮调整或者弹出编辑窗口录入后调整都一样）的第一个字为上下标等小字体（并保存后），那么调整后，该行都变成小字体了。
                //重置的话，会导致，整行颜色设置的话，需要重新置入才会变成对应的颜色，不然还保持原来的颜色，如pink等。现在即使修改颜色还是原来的颜色
                //同样清除格式完，也变成了默认颜色，需要再次点进，才会变成行文字颜色。
                //rtbe._IsRtf = false; 
                //rtbe.SetAlignment();
                //if (!string.IsNullOrEmpty(_TableInfor.Rows[_CurrentCellIndex.X].RowForeColor))
                //{
                //    rtbe.SelectionColor = Color.FromName(_TableInfor.Rows[_CurrentCellIndex.X].RowForeColor);//行文字颜色;// rtbe.DefaultForeColor;
                //}
                //else
                //{
                //    rtbe.SelectionColor = rtbe.DefaultForeColor;
                //}

                //string currentNodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString();

                rtbe.SetAlignment();
                rtbe.SetSubScript(false);//第一个字是上下标，调整后，第一行都变成上小标了
                rtbe.SetSuperScript(false);

                //--虽然调整段落的开始行一般不会是上下标的字体开头，

                //设置当前行的内容
                rtbe.Select(0, arr[0].Length);
                rtbe.Rtf = rtbe.SelectedRtf; //该行显示一行的内容
                rtbe.Select(0, 0);
                rtbe.SelectedText = rowHearStr;//补上开头空格（有些表单可能配置的不需要缩进）

                //Color rowForeColor = rtbe.ForeColor;//记录下这行的行文字颜色：这样的话会有问题，如果行文字颜色没有指定，单元格颜色不一样就都变成目前这个单元格的颜色了。
                Color rowForeColor = Color.FromName(_TableInfor.Rows[_CurrentCellIndex.X].RowForeColor);

                string rowUserId = _TableInfor.Rows[_CurrentCellIndex.X].UserID;
                string rowUserName = _TableInfor.Rows[_CurrentCellIndex.X].UserName;
                string rowNurseLevel = _TableInfor.Rows[_CurrentCellIndex.X].NurseFormLevel;

                string newRowInfor = "";
                XmlNode recordsNode = null;
                XmlNode cellNode;

                //下页
                int nextPageRowIndex = 0;
                //只有当下页有数据，且属于一个段落，才为True有数据
                bool isHaveNextPageRecord = false; //是否存在下页的数据，如果有数据，但是和当前段落不属于一个段落也当作不存在数据
                XmlNode lastNode = null;
                string[] valueArr;
                string value = "";
                TreeNode retTN = null; //调整段落列后，是否添加了下一页的节点

                #region 签名内容，在修改调整段落列后，签名显示到最后一行
                //string signText = "";
                //recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                //cellNode = recordsNode.SelectSingleNode("Record[@ID='" + _TableInfor.Rows[_CurrentCellIndex.X + 1].ID.ToString() + "']");
                //if (!string.IsNullOrEmpty((cellNode as XmlElement).GetAttribute("签名").Split('¤')[0]))
                //{
                //    signText = (cellNode as XmlElement).GetAttribute("签名");
                //}
                #endregion 签名内容，在修改调整段落列后，签名显示到最后一行

                bool gotoNextPage = false;
                int showedIndex = 0;

                for (int i = 1; i < arr.Length; i++)
                {
                    //在下面增加一空白行，光标置入：
                    if (_CurrentCellIndex.X < _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
                    {
                        //如果是按下ctrl间，点击工具栏调整，那么不往下合并
                        if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                        //if(_IS_SHIFT_KEYDOWN
                        {
                            //MessageBox.Show("Pressed " + Keys.Shift);

                            //向下插入空白行
                            tBnt_InsertRowDwon_Click(null, null);
                            _TableInfor.Cells[_CurrentCellIndex.X + 1, _CurrentCellIndex.Y].Text = arr[i];
                        }
                        else
                        {
                            //判断目标行的下一行是否是调整行，要不要进行插入行操作：时间不为空，且和当前行不一样，那就要插入行；否则不用。如果没有时间，就按照日期判断
                            //if ((isHaveTimeCol && !string.IsNullOrEmpty(_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X + 1, "时间").Text) && NotSameDateTimeToPreRow(_CurrentCellIndex.X + 1))
                            //    || (!isHaveTimeCol && !string.IsNullOrEmpty(_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X + 1, "日期").Text) && NotSameDateTimeToPreRow(_CurrentCellIndex.X + 1))) //没有日期和时间的就不行了
                            if ((isHaveTimeCol && !string.IsNullOrEmpty(_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X + 1, "时间").Text) && NotSameDateTimeToPreRow(_CurrentCellIndex.X + 1))
                                    || (isHaveDateCol && !string.IsNullOrEmpty(_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X + 1, "日期").Text) && NotSameDateTimeToPreRow(_CurrentCellIndex.X + 1))
                                    || (!isHaveTimeCol && !isHaveDateCol && _TableInfor.Cells[_CurrentCellIndex.X + 1, _CurrentCellIndex.Y].Text != ""))
                            {
                                //向下插入空白行
                                tBnt_InsertRowDwon_Click(null, null);
                                _TableInfor.Cells[_CurrentCellIndex.X + 1, _CurrentCellIndex.Y].Text = arr[i];


                            }
                            else
                            {

                                //如果是已经是调整行，那么就不用插入了，直接利用
                                _TableInfor.Cells[_CurrentCellIndex.X + 1, _CurrentCellIndex.Y].Text = arr[i] + _TableInfor.Cells[_CurrentCellIndex.X + 1, _CurrentCellIndex.Y].Text;
                                _TableInfor.Cells[_CurrentCellIndex.X + 1, _CurrentCellIndex.Y].Rtf = ""; //要丢弃格式，不然增加的文字无效，rtf优先赋值了。

                            }
                        }

                        //如果是行的颜色根据时间变色，那么也要设置成一样。
                        _TableInfor.Rows[_CurrentCellIndex.X + 1].RowForeColor = rowForeColor.Name;//统一段落的时间是一样的，所以，行的文字颜色也是应该一样的

                        //同段落的签名都是一样的，而且对于护士来说只要签最后一行
                        _TableInfor.Rows[_CurrentCellIndex.X + 1].UserID = rowUserId;
                        _TableInfor.Rows[_CurrentCellIndex.X + 1].UserName = rowUserName;
                        _TableInfor.Rows[_CurrentCellIndex.X + 1].NurseFormLevel = rowNurseLevel;

                        //更新保存的行颜色
                        //行线颜色 更新
                        //更新到xml中，
                        recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                        int nodeId = _TableInfor.Rows[_CurrentCellIndex.X + 1].ID;
                        cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record))); //"Record[@ID='" + nodeId.ToString() + "']"

                        newRowInfor = "";
                        newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X + 1].RowLineColor.Name + "¤";
                        newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X + 1].CustomLineType.ToString() + "¤";
                        newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X + 1].RowForeColor + "¤";
                        newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X + 1].CustomLineColor.Name;
                        (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), newRowInfor);      //行信息：行线颜色，双红线类型，行文字颜色，双红线颜色

                        #region 签名内容，在修改调整段落列后，签名显示到最后一行
                        ////signText =  _TableInfor.Cells[_CurrentCellIndex.X + 1, _CurrentCellIndex.Y].Text
                        ////if (_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间") == null)
                        //if (!string.IsNullOrEmpty((cellNode as XmlElement).GetAttribute("签名").Split('¤')[0]))
                        //{
                        //    //-- Redraw(TransparentRTBExtended rtbe)
                        //    signText = (cellNode as XmlElement).GetAttribute("签名");
                        //}
                        #endregion 签名内容，在修改调整段落列后，签名显示到最后一行

                        //防止手动直接中文输入的时候，会在下一行显示两行中文

                        //Application.DoEvents();
                        //Application.DoEvents();

                        //光标置入下一行：
                        //会导致没有保存数据
                        Comm._SettingRowForeColor = true;
                        ShowCellRTBE(_CurrentCellIndex.X + 1, _CurrentCellIndex.Y);
                        Comm._SettingRowForeColor = false;

                        //为了触发leave事件进行保存
                        _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.OldRtf = ""; //输入助理换行或者手动第一个字换行后设置后，因为认为没有改变不会触发保存


                        //防止全选 光标人性化设置，可以立刻再继续输入
                        _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Select(arr[i].Length, 0);

                        //_LastNodeAdjustParagraphs = recordsNode.SelectSingleNode("Record[@ID='" + _TableInfor.Rows[_CurrentCellIndex.X].ID + "']");
                        _LastNodeAdjustParagraphs = cellNode;

                        showedIndex++;
                    }
                    else
                    {
                        gotoNextPage = true;

                        //换页：下一页继续处理：1.有可能存在下页数据，有可能还没有下页的任何数据
                        //显示到下一页。问题在于，每页第一行都要显示日期时间，但是不要行缩进两个空格。
                        //1.如果下一页还没有效数据（非空白），直接末尾插入节点
                        //2.如果下一页已经存在数据，那么要判断，下一页的开始行的日期和时间是否和当前段落的日期时间一致，如果一致认为是同一段落；那么不需要行头缩进
                        if (recordsNode == null)
                        {
                            recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                        }

                        //第一次进入这个分支的时候才需要做判断，以后每次按照第一次的判断，进行处理就可以了
                        if (nextPageRowIndex == 0 || isHaveNextPageRecord) // 如果要到下一页多行，不是每行都是同一段落的
                        {
                            //会导致没有保存数据
                            this.pictureBoxBackGround.Focus();//上面的ShowCellRTBE后，要保存数据 必须要有这一行代码，否则就可能报错，下面的数据没有保存，作为空节点删除，然后找不到节点。
                            _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Visible = false; //隐藏，防止光标还在，继续输入就出问题了。

                            //病人列表中如果没有下一页，那么就需要添加
                            if (needGoToNextPage)
                            {
                                //如果是刚创建的表单，还没有保存后，直接调整段落行，产生新页，那么page节点也是没有的。
                                //if (_IsCreated)
                                //{                                //
                                SaveThisPageDataToXml();
                                //}

                                retTN = AdddPageNode(); //只能增加一页：而且，是在最后一页编辑导致调整段落列添加页的时候。

                                //TODO：如果是从中间页进行修改，那么还需要计算后面的空白行，否则会导致计算增加的页不准。多增加页。
                                //目前是只增加一页，但是如果特殊情况下，调整段罗列后增加了多页，还是要添加实际的页数的。
                                //自动调整段落后，须新建到最后一页
                                //int reHaveRows = arr.Length - showedIndex - _PageRowsSecondPage;
                                if (this.uc_Pager1.PageCount == int.Parse(_CurrentPage) + 1) //只有在最后一页编辑的时候，才处理。以前的不计算调整页数，始终为一页的也是这样的。
                                {
                                    int reHaveRows = arr.Length - i - _PageRowsSecondPage;
                                    int addPages = 1;
                                    if (reHaveRows > 0)
                                    {
                                        //一页还不够，还需要增加
                                        int otherPages = reHaveRows / _PageRowsSecondPage;
                                        if (reHaveRows % _PageRowsSecondPage > 0)
                                        {
                                            otherPages++; //不满一页，也至少有一页了
                                        }

                                        addPages = addPages + otherPages;

                                        for (int aa = 0; aa < otherPages; aa++)
                                        {
                                            retTN = AdddPageNodeN(); //连续增加多个n个其他页，更加人性化

                                            //page节点的基本信息也需要更新，否则每次打开，还会再提示更细基本信息后的保存。                                        
                                        }
                                    }

                                    //补上基本信息：                             
                                    for (int aa = 0; aa < addPages - 1; aa++)
                                    {
                                        AddPageInfor(aa + int.Parse(_CurrentPage) + 1);
                                    }

                                    //从最早，到现在的调整段落列，自动产生的新页，如果含有默认值的控件，都没有赋值默认值。但是连续创建n页的是有的默认值的：AutoCreatPagesCount
                                }
                                this.uc_Pager1.Bind();
                            }


                            retCreatPage = true;

                            RemoveEmptyRows();  //删除程序补充的末尾的空白行

                            //根据xml，取上一页最后一行的日期和时间
                            XmlNode thisNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString(), "=", nameof(EntXmlModel.Record)));
                            //"Record[@ID='" + _TableInfor.Rows[_CurrentCellIndex.X].ID + "']"
                            if (lastNode == null)
                            {
                                //只有第一次需要赋值为：本页最后一行对应的节点,如果下页同段落赋值后，就为下页那个节点
                                lastNode = thisNode;
                            }

                            thisNode = lastNode.NextSibling; //取得下一条数据。    

                            string thisRowDateTime = GetRowDateTime(_TableInfor.Rows[currentRow].ID.ToString()); //开始调整行
                            thisRowDate = thisRowDateTime.Split('¤')[0];    //_TableInfor.CellByRowNo_ColName(rowIndex, "日期").Text; //如果为空
                            thisRowTime = thisRowDateTime.Split('¤')[1];    //_TableInfor.CellByRowNo_ColName(rowIndex, "时间").Text;

                            if (thisNode != null)
                            {
                                if (((thisNode as XmlElement).GetAttribute("日期").Split('¤')[0].Trim() == "" && (thisNode as XmlElement).GetAttribute("时间").Split('¤')[0].Trim() == "")
                                    || ((thisNode as XmlElement).GetAttribute("日期").Split('¤')[0].Trim() == thisRowDate && (thisNode as XmlElement).GetAttribute("时间").Split('¤')[0].Trim() == thisRowTime)
                                    || ((!isHaveDateCol || (thisNode as XmlElement).GetAttribute("日期").Split('¤')[0].Trim() == thisRowDate) && (!isHaveTimeCol || (thisNode as XmlElement).GetAttribute("时间").Split('¤')[0].Trim() == thisRowTime)))
                                {
                                    isHaveNextPageRecord = true; //下页的第一行和当前页的最后段落，属于同一段落
                                }
                                else
                                {
                                    isHaveNextPageRecord = false; //下页的首行，和目前行不属于一个段落，也和没有数据一样，直接插入新节点就可以了
                                }
                            }
                            else
                            {
                                //不存在后页的数据，那么也认为
                                isHaveNextPageRecord = false;
                            }
                        }

                        //注意：如果以后：每次换页都强行插入行，那么将上面的变量：isHaveNextPageRecord写死设定为为False就可以了，不判断，写死即可。
                        //但是在弹出界面输入框，修改好整个段落一起提交的时候，不能始终添加行
                        if (isHaveNextPageRecord && notMergeNextPageContent) //  && notMergeNextPageContent如果是手动调整增加到下一行，那么合并的话，内容和一行一行凭借反而错乱。干脆插入行
                        {
                            //更新，但是可能只是第一行先更新，第二行开始就是要插入了
                            lastNode = lastNode.NextSibling;

                            valueArr = (lastNode as XmlElement).GetAttribute(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Name).Split('¤');

                            if (notMergeNextPageContent)
                            {
                                //弹出界面整体修改输入调整时
                                valueArr[0] = arr[i];
                            }
                            else
                            {
                                //直接输入时，点击菜单按钮调整时
                                valueArr[0] = arr[i] + valueArr[0];
                            }

                            //单元格的复合信息（内容和格式等全部信息）
                            for (int m = 0; m < valueArr.Length; m++)
                            {
                                if (m == 0)
                                {
                                    value = valueArr[m];
                                }
                                else
                                {
                                    value += "¤" + valueArr[m];
                                }

                            }

                            (lastNode as XmlElement).SetAttribute(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Name, value);

                            valueArr = (lastNode as XmlElement).GetAttribute(nameof(EntXmlModel.ROW)).Split('¤');

                            //valueArr[2] = rowForeColor.Name;
                            (lastNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), valueArr[0] + "¤" + valueArr[1] + "¤" + rowForeColor.Name);      //行信息

                        }
                        else
                        {
                            //在当前节点后面插入新的节点
                            //添加对应的xml空白新节点
                            //-------------更新到xml中，在对应位置添加对应的节点
                            XmlElement newNode = _RecruitXML.CreateElement(nameof(EntXmlModel.Record));
                            int temp = _TableInfor.AddRowGetMaxID();
                            (newNode as XmlElement).SetAttribute(nameof(EntXmlModel.ID), temp.ToString());

                            (newNode as XmlElement).SetAttribute(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Name, arr[i] + "¤False¤False¤¤False¤False¤False¤¤Black¤");
                            (newNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), "Black¤None¤" + rowForeColor.Name);      //行信息
                            (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), temp.ToString());

                            recordsNode.InsertAfter(newNode, lastNode);

                            lastNode = newNode;
                            //-------------更新到xml中，在对应位置添加对应的节点
                        }

                        //ROW="Black¤None¤Blue"
                        newRowInfor = (lastNode as XmlElement).GetAttribute(nameof(EntXmlModel.ROW));
                        (lastNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), newRowInfor);      //行信息

                        //同一段落签名保留一致
                        (lastNode as XmlElement).SetAttribute(nameof(EntXmlModel.UserID), rowUserId + "¤" + rowUserName + "¤" + rowNurseLevel);

                        //换页后的首行，需要设置本段落的日期和时间
                        if (nextPageRowIndex == 0)
                        {
                            (lastNode as XmlElement).SetAttribute("日期", thisRowDate + "¤False¤False¤¤False¤False¤False¤¤Black¤");

                            if (isHaveTimeCol)
                            {
                                (lastNode as XmlElement).SetAttribute("时间", thisRowTime + "¤False¤False¤¤False¤False¤False¤¤Black¤");
                            }
                        }

                        _LastNodeAdjustParagraphs = lastNode;

                        nextPageRowIndex++;
                    }
                }

                //虽然不用保存，到下一页保存，但是这一页如果要求每页的最后一个段落行要签名。那么还是要签名
                //SaveAutoSign="End" SaveAutoSignLastRow=EndAll:表示每页最后一行需要签名
                //保存时自动签名
                if (gotoNextPage && _SaveAutoSignLastRow)
                {
                    if (!_IsCASignAutoSave)
                    {
                        _IsSignLastRow = true;
                        SaveAutoSign();
                        _IsSignLastRow = false;
                    }
                }

                ////调整段落列后，签名保持在最后一行
                //SetParagraphSign(currentNodeId);  // string currentNodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString();

                if (nextPageRowIndex == 0)
                {
                    //都在本页进行调整的段落
                    ShowInforMsg("进行自动段落列调整：\r\n" + processStr.Replace("¤", "\r\n"));
                }
                else
                {
                    ShowInforMsg("进行自动段落列调整：\r\n" + processStr.Replace("¤", "\r\n") + "\r\n其中最后" + nextPageRowIndex.ToString() + "行调整到了下一页。");
                }

                //如果调整段落列后，添加了新页，那么提示是否需要打开新页
                if (retTN != null)
                {
                    //retCreatPage = true;

                    if (needGoToNextPage)
                    {
                        if (MessageBox.Show("调整段落列后产生了新页，需要打开新页吗？", "打开新页" + _TemplateName + "," + this._patientInfo.PatientName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            this.treeViewPatients.SelectedNode = retTN;

                            _NotNeedCheckForSave = true; //切换过程中不需要提示保存等

                            _IsCreating = true; //这样才会和新建页一样，自动创建的页有默认值

                            treeViewPatients_NodeMouseDoubleClick(retTN, null);

                            _NotNeedCheckForSave = false;
                            _IsCreating = false;
                        }
                    }
                }
            }
            else
            {
                ////如果调整行，光标没有置入，在下面输入几行，单后工具栏调整段落，保存后刷新，还是一开始的行。
                //SaveCellToXML(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE);
            }

            isAdjusting = false;

            return retCreatPage; //是否调整后，产生到了新页的下一页
        }

        /// <summary>
        /// 调整段落列，添加多页的时候，那么需要判断是否增加页
        /// </summary>
        /// <param name=nameof(EntXmlModel.Forms)></param>
        private void AddPageInfor(int creatPages)
        {

            XmlNode thisPageNode = null;
            XmlNode xnEach = null;  //模板节点
            XmlNode thisPageData;
            string strName = "";

            //for (int i = 2; i <= creatPages; i++)
            //{
            int i = creatPages;

            //获取，当前模板页配置节点
            thisPageNode = GetTemplatePageNode(_TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design))), i);

            if (thisPageNode != null)
            {
                XmlNode pages = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms)));
                thisPageData = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), i.ToString(), "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));
                strName = "";

                if (thisPageData == null) //如果还没有该页节点
                {
                    //插入新的节点
                    XmlElement newPage;
                    newPage = _RecruitXML.CreateElement(nameof(EntXmlModel.Form));

                    newPage.SetAttribute(nameof(EntXmlModel.SN), i.ToString());

                    pages.AppendChild(newPage);

                    thisPageData = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), i.ToString(), "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));
                }

                for (int j = 0; j < thisPageNode.ChildNodes.Count; j++)
                {
                    //如果是注释那么跳过
                    if (thisPageNode.ChildNodes[j].Name == @"#comment" || thisPageNode.ChildNodes[j].Name == @"#text")
                    {
                        continue;
                    }

                    xnEach = thisPageNode.ChildNodes[j];

                    strName = (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));

                    if (xnEach.Name == nameof(EntXmlModel.BindingValue))
                    {
                        //不等于页号
                        if ((xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) != "_PageNo")
                        {
                            if (_CurrentBasicInfor != null && _CurrentBasicInfor.ContainsKey(strName))
                            {
                                (thisPageData as XmlElement).SetAttribute(strName, _CurrentBasicInfor[strName]);
                            }
                        }
                    }
                    else if (xnEach.Name.EndsWith(nameof(EntXmlModel.Text)) || xnEach.Name == nameof(EntXmlModel.ComboBox))
                    {
                        //bool autoLoadDefault = false;
                        //bool.TryParse((xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.AutoLoadDefault)), out autoLoadDefault);
                        //if (autoLoadDefault)
                        (thisPageData as XmlElement).SetAttribute(strName, (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)));
                    }
                    else if (xnEach.Name == nameof(EntXmlModel.CheckBox))
                    {
                        (thisPageData as XmlElement).SetAttribute(strName, (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)));
                    }
                    //else if (xnEach.Name == nameof(EntXmlModel.ComboBox))
                    //{
                    //    (thisPageData as XmlElement).SetAttribute(strName, (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)));
                    //}

                }

            }
            //}
        }

        # endregion 调整段落列，主处理方法

        #region 签名内容，在修改调整段落列后，签名显示到最后一行
        /// <summary>
        /// 签名内容，在修改调整段落列后，签名显示到最后一行
        /// </summary>
        /// <returns></returns>
        private void SetParagraphSign(string rowIdPara)
        {
            //如果没有签名列，那么不需要处理
            if (_TableInfor.CellByRowNo_ColName(0, "签名") == null)
            {
                return;
            }

            //TODO:测试：CA签名、跨页，下面有其他段落，都没有签名等情况，都需进行测试
            //TODO:::如果以后有问题，可以加一个属性（ParagraphSignEnd="False"），默认为True，设置成False的话，在调整段落列的时候就执行签名置尾的处理

            //最后一行的数据保存到内存xml中
            SaveToOp();

            //先做验证，判断要不要调整签名
            XmlNode xnSigned = null;
            XmlNode xnLast = null;

            //根据后台XMl数据，获取到该段落的内容
            XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            //int nodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID;
            XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), rowIdPara, "=", nameof(EntXmlModel.Record))); //"Record[@ID='" + rowIdPara + "']"

            //极端情况，如果前后，将多行的段落内容全部清空，那么签名应该显示到第一行
            xnLast = cellNode;

            string signUserId = ""; //如果签名，那么还需要签名航的用户信息
            string signText = "";
            if (!string.IsNullOrEmpty((cellNode as XmlElement).GetAttribute("签名").Split('¤')[0]))
            {
                signText = (cellNode as XmlElement).GetAttribute("签名");
                xnSigned = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)), "=", nameof(EntXmlModel.Record)));

                if (!string.IsNullOrEmpty((cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.UserID))))
                {
                    signUserId = (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.UserID));
                }
            }

            if (!Comm.isEmptyRecordSign(cellNode)) //还要除签名和日期时间 ，如果只有签名有内容，那么也要算空行，将签名移动上面去。或者已经是该段落第一行（整个段落为空）
            {
                xnLast = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)), "=", nameof(EntXmlModel.Record)));
            }

            string currentDateTime = GetRowDateTime(rowIdPara);
            string eachDateTime = "";
            cellNode = cellNode.NextSibling;    //如果当前节点已经是最前面的节点，那么返回null

            bool otherNeedRedraw = false;
            int rowCount = 0;

            bool needRemoveSign = true;

            while (cellNode != null)
            {
                eachDateTime = GetRowDateTime((cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                if (currentDateTime == eachDateTime)
                {
                    //记录下同一段落最后一个签名行
                    if (!string.IsNullOrEmpty((cellNode as XmlElement).GetAttribute("签名").Split('¤')[0]))
                    {
                        //之前可能已经有签名，一个段落多次签名，合并成一次（用最后一次）
                        if (xnSigned != null && (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)) != (xnSigned as XmlElement).GetAttribute(nameof(EntXmlModel.ID)))
                        {
                            needRemoveSign = true;

                            if (_SaveAutoSignLastRow)
                            {
                                rowCount = 0;

                                //知道一个xml节点，何获取它在父节点中的位置
                                TableClass.GetPageCount_HaveTable(_TemplateXml, GetNodeIndex(xnSigned) + 1, ref rowCount);

                                if (rowCount == _TableInfor.RowsCount * _TableInfor.GroupColumnNum)
                                {
                                    //最后一行，跳过，不清空
                                    needRemoveSign = false;
                                }
                            }

                            if (needRemoveSign)
                            {
                                (xnSigned as XmlElement).RemoveAttribute("签名");
                                //重绘，清空显示的签名
                                otherNeedRedraw = true;
                            }
                        }


                        //xnSigned = cellNode; 
                        xnSigned = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)), "=", nameof(EntXmlModel.Record)));
                        signText = (cellNode as XmlElement).GetAttribute("签名");

                        if (!string.IsNullOrEmpty((cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.UserID))))
                        {
                            signUserId = (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.UserID));
                        }

                        //xnSignedArr.Add(xnSigned);
                    }

                    //记录最后一个非空的段落行，（最后是页末尾的最后一个段落可能有空白行）
                    if (!Comm.isEmptyRecordSign(cellNode)) //还要除签名的非空
                    {
                        //xnLast = cellNode; //引用类型的
                        //记录下本段落的最后一行，下面要赋值签名。也就是将签名移动到最后一行
                        //"Record[@ID='" + (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)) + "']"
                        xnLast = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)), "=", nameof(EntXmlModel.Record)));
                    }

                    //调整段落，导致行变少后，后面的行要清除签名ID
                    //签名后只能修改段落的脚本时发现，签名后，再重新调整段落的话，如果行变多了，那么多出的行也是原来段落的签名（第一行开始调整），但是要是少了行，后面的行还是保持签名的
                    //上面代码清理签名xnSigned是在后一次循环后才会被清除的。所以也需要用isEmptyRecordSign来判断。
                    if (Comm.isEmptyRecordSign(cellNode))
                    {
                        //UserID="Mandalat¤华佗再世¤主治医师"
                        //清空签名
                        (cellNode as XmlElement).RemoveAttribute(nameof(EntXmlModel.UserID));
                    }
                    else
                    {
                        //如果行变少，签名一般是在最后一行啊。可能还是没有找到呢
                        if (xnSigned != null && !string.IsNullOrEmpty(signUserId))
                        {
                            //增加可能签名有的时候还没有
                            if (string.IsNullOrEmpty((cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.UserID))))
                            {
                                //清空签名
                                (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.UserID), signUserId);
                            }
                        }
                    }

                    cellNode = cellNode.NextSibling;    //如果当前节点已经是最前面的节点，那么返回null
                }
                else
                {
                    //日期时间不是一个段落，跳出循环
                    cellNode = null;
                    break;
                }

                ////防止内容过长，尤其没有日期和时间列的表格，所以超过指定字数就停止
                //if (content.Length > 6000)
                //{
                //    break;
                //}
            }

            if (xnLast != null && xnSigned != null && !string.IsNullOrEmpty(signText.Split('¤')[0])
                && (xnSigned as XmlElement).GetAttribute(nameof(EntXmlModel.ID)) != (xnLast as XmlElement).GetAttribute(nameof(EntXmlModel.ID)))
            {
                //将签名设置到本段落最后一行
                (xnLast as XmlElement).SetAttribute("签名", signText);

                //清空签名
                //(cellNode as XmlElement).SetAttribute("签名", "");

                needRemoveSign = true;

                if (_SaveAutoSignLastRow)
                {
                    rowCount = 0;

                    //知道一个xml节点，何获取它在父节点中的位置
                    TableClass.GetPageCount_HaveTable(_TemplateXml, GetNodeIndex(xnSigned) + 1, ref rowCount);

                    if (rowCount == _TableInfor.RowsCount * _TableInfor.GroupColumnNum)
                    {
                        //最后一行，跳过，不清空
                        needRemoveSign = false;
                    }
                }

                if (needRemoveSign)
                {
                    (xnSigned as XmlElement).RemoveAttribute("签名");
                }

                Comm.LogHelp.WriteInformation(GlobalVariable.LoginUserInfo.UserCode + _TemplateName + "进行了调整段落列签名置尾处理");

                //刷新
                //重新按照排序后的数据显示
                //当前页，重新载入表格，并重绘，选中新位置的行
                ReSetTableInfor();

                LoadTable(_TableInfor, false);

                _IsLoading = true;
                Redraw();
                _IsLoading = false;
            }
            else
            {
                if (otherNeedRedraw)
                {
                    //删除多次签名的情况下，刷新

                    //刷新
                    //重新按照排序后的数据显示
                    //当前页，重新载入表格，并重绘，选中新位置的行
                    ReSetTableInfor();

                    LoadTable(_TableInfor, false);

                    _IsLoading = true;
                    Redraw();
                    _IsLoading = false;
                }
            }

        }
        #endregion 签名内容，在修改调整段落列后，签名显示到最后一行

        #region 获取行的时间
        /// <summary>
        /// 扩展用方法：
        /// 根据规律，找到指定行对应的时间
        /// 在调整段落列的时候，需要进行判断
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        private string GetRowDateTime(string rowXmlID)
        {
            //string dt = "¤"; //用小太阳"¤"来分割日期和之间进行返回
            string date = "";
            string time = "";

            //每个表格的第一行，必须有日期和时间，
            //根据mxl中的日期和时间去网上遍历寻找
            XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), rowXmlID, "=", nameof(EntXmlModel.Record))); //如果异常情况下，rowXmlID为空，那么cellNode就为null，就会报错

            date = (cellNode as XmlElement).GetAttribute("日期").Split('¤')[0].Trim();
            time = (cellNode as XmlElement).GetAttribute("时间").Split('¤')[0].Trim();

            //是否包含时间
            bool isHaveTimeCol = true;
            if (_TableInfor.CellByRowNo_ColName(0, "时间") != null)
            {
                isHaveTimeCol = true;
            }
            else
            {
                time = "12:00";
                isHaveTimeCol = false;
            }

            ////处理日期和时间两列合并的：根据第一列来判断；本来就不建议使用日期和时间两列合并的
            //if (date == "" && !string.IsNullOrEmpty(_TableInfor.Columns[0].RTBE._YMD_Default))
            //{
            //    if (_TableInfor.Columns[0].RTBE._YMD_Default.Contains("yyyy") && _TableInfor.Columns[0].RTBE._YMD_Default.Contains("mm"))
            //    {
            //        date = (cellNode as XmlElement).GetAttribute(_TableInfor.Columns[0].ColName).Split('¤')[0].Trim();

            //        if (date.Split(' ').Length == 2)
            //        {
            //            time = date.Split(' ')[1];
            //            date = date.Split(' ')[0];
            //        }
            //    }
            //}
            string[] arr;

            //如果为空，那么循环往上取值
            while ((date == "" || time == ""))
            {
                cellNode = cellNode.PreviousSibling; //如果当前节点已经是最前面的节点，那么返回null

                if (cellNode != null)
                {
                    if (date == "")
                    {
                        date = (cellNode as XmlElement).GetAttribute("日期").Split('¤')[0].Trim();

                        ////处理日期和时间两列合并的：根据第一列来判断；本来就不建议使用日期和时间两列合并的
                        //if (date == "" && !string.IsNullOrEmpty(_TableInfor.Columns[0].RTBE._YMD_Default))
                        //{
                        //    if (_TableInfor.Columns[0].RTBE._YMD_Default.Contains("yyyy") && _TableInfor.Columns[0].RTBE._YMD_Default.Contains("mm"))
                        //    {
                        //        date = (cellNode as XmlElement).GetAttribute(_TableInfor.Columns[0].ColName).Split('¤')[0].Trim();

                        //        if (date.Split(' ').Length == 2)
                        //        {
                        //            time = date.Split(' ')[1];
                        //            date = date.Split(' ')[0];
                        //        }
                        //    }
                        //}

                        //if (date != "") //本身那条数据会被忽略，其实也是符合实际的。但是合计窗口的开始事件和结束事件还是需要重新设置
                        //{
                        //    
                        //    //如果是合并单元格，并且不是日期格式，那么跳过去上一个。
                        //    //日期="2016-11-19¤False¤False¤¤False¤False¤False¤¤Black¤" 索引7位置
                        //    //日期="引流量合并,合计中断¤False¤False¤Mandalat¤False¤False¤False¤R,1,False¤Black¤¤¤MM-dd" 
                        //    arr = (cellNode as XmlElement).GetAttribute("日期").Split('¤');
                        //    if (arr.Length > 7 && !string.IsNullOrEmpty(arr[7]))
                        //    {
                        //        if (!MyDate.IsDate(arr[0]) && !MyDate.IsDateTime(arr[0]))
                        //        {
                        //            date = "";
                        //            //continue;
                        //        }
                        //    }
                        //}
                    }

                    if (time == "")
                    {
                        time = (cellNode as XmlElement).GetAttribute("时间").Split('¤')[0].Trim();
                    }

                    if (isHaveTimeCol)
                    {
                        if (date != "" && time != "")
                        {
                            break; //都不会空，那么跳出循环；表示已经取到值了
                        }
                    }
                    else
                    {
                        if (date != "")
                        {
                            break; //都不会空，那么跳出循环；表示已经取到值了
                        }
                    }
                }
                else
                {
                    //到了第一个节点，还是没有找到
                    if (_TableInfor.CellByRowNo_ColName(0, "日期") != null)
                    {
                        ShowInforMsg("表格日期或者时间书写不规范，无法找到段落的对应日期或时间。");
                    }

                    break;//没有找到合法的日期和时间，跳出
                }
            }

            //XmlNode tempNode = cellNode.PreviousSibling; //如果当前节点已经是最前面的节点，那么返回null

            return date + "¤" + time;
        }
        # endregion 获取行的时间

        # region 当前行和上以上是否时间相同（同段落）
        private bool NotSameDateTimeToPreRow(int rowIndex)
        {
            bool isNotSameDateTimeToPreRow = false;
            string preRowDateTime = "";

            string thisRowDateTime = GetRowDateTime(_TableInfor.Rows[rowIndex].ID.ToString());

            string thisRowDate = thisRowDateTime.Split('¤')[0];    //_TableInfor.CellByRowNo_ColName(rowIndex, "日期").Text; //如果为空
            string thisRowTime = thisRowDateTime.Split('¤')[1];    //_TableInfor.CellByRowNo_ColName(rowIndex, "时间").Text;

            //护理部要求写入24:00
            //thisRowTime = thisRowTime == "24:00" ? "23:59" : thisRowTime;

            if (rowIndex > 0)
            {
                preRowDateTime = GetRowDateTime(_TableInfor.Rows[rowIndex - 1].ID.ToString());
            }
            else
            {
                //根据当前行的xml，取上一页最后一行的日期和时间
                XmlNode records = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                XmlNode thisNode = records.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[rowIndex].ID.ToString(), "=", nameof(EntXmlModel.Record)));
                //"Record[@ID='" + _TableInfor.Rows[rowIndex].ID + "']"
                if (thisNode != null)
                {
                    thisNode = thisNode.PreviousSibling;    //如果当前节点已经是最前面的节点，那么返回null

                    if (thisNode != null)
                    {
                        preRowDateTime = GetRowDateTime((thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));
                    }
                    else
                    {
                        //第一页的第一行，默认认为和前一行（不存在前一行）是不相同的日期时间
                        return true;
                    }
                }
            }

            string[] arrDT = preRowDateTime.Split('¤');
            if (arrDT[0] != thisRowDate || arrDT[1] != thisRowTime)
            {
                isNotSameDateTimeToPreRow = true;
            }
            else
            {
                isNotSameDateTimeToPreRow = false;
            }

            return isNotSameDateTimeToPreRow;
        }

        /// <summary>
        /// 和上一行比较日期时间是否一致
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        private bool NotSameDateTimeToNextRow(int rowIndex)
        {
            bool isNotSameDateTimeToPreRow = false;
            string preRowDateTime = "";

            string thisRowDateTime = GetRowDateTime(_TableInfor.Rows[rowIndex].ID.ToString());

            string thisRowDate = thisRowDateTime.Split('¤')[0];    //_TableInfor.CellByRowNo_ColName(rowIndex, "日期").Text; //如果为空
            string thisRowTime = thisRowDateTime.Split('¤')[1];    //_TableInfor.CellByRowNo_ColName(rowIndex, "时间").Text;

            //护理部要求写入24:00
            //thisRowTime = thisRowTime == "24:00" ? "23:59" : thisRowTime;

            if (rowIndex + 1 <= _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
            {
                preRowDateTime = GetRowDateTime(_TableInfor.Rows[rowIndex + 1].ID.ToString());
            }
            else
            {
                //根据当前行的xml，取上一页最后一行的日期和时间
                XmlNode records = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                XmlNode thisNode = records.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[rowIndex].ID.ToString(), "=", nameof(EntXmlModel.Record)));
                //"Record[@ID='" + _TableInfor.Rows[rowIndex].ID + "']"

                if (thisNode != null)
                {
                    thisNode = thisNode.NextSibling;    //如果当前节点已经是最前面的节点，那么返回null

                    if (thisNode != null)
                    {
                        preRowDateTime = GetRowDateTime((thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));
                    }
                    else
                    {
                        //第一页的第一行，默认认为和前一行（不存在前一行）是不相同的日期时间
                        return true;
                    }
                }
            }

            string[] arrDT = preRowDateTime.Split('¤');
            if (arrDT[0] != thisRowDate || arrDT[1] != thisRowTime)
            {
                isNotSameDateTimeToPreRow = true;
            }
            else
            {
                isNotSameDateTimeToPreRow = false;
            }

            return isNotSameDateTimeToPreRow;
        }

        private bool NotSameDateTimeToPreRow(string rowId)
        {
            bool isNotSameDateTimeToPreRow = false;
            string preRowDateTime = "";

            string thisRowDateTime = GetRowDateTime(rowId);

            string thisRowDate = thisRowDateTime.Split('¤')[0];    //_TableInfor.CellByRowNo_ColName(rowIndex, "日期").Text; //如果为空
            string thisRowTime = thisRowDateTime.Split('¤')[1];    //_TableInfor.CellByRowNo_ColName(rowIndex, "时间").Text;


            //if (rowIndex > 0)
            //{
            //    preRowDateTime = GetRowDateTime(_TableInfor.Rows[rowIndex - 1].ID.ToString());
            //}
            //else
            //{
            //根据当前行的xml，取上一页最后一行的日期和时间
            XmlNode records = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            XmlNode thisNode = records.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), rowId, "=", nameof(EntXmlModel.Record)));

            if (thisNode != null)
            {
                thisNode = thisNode.PreviousSibling;    //如果当前节点已经是最前面的节点，那么返回null

                if (thisNode != null)
                {
                    preRowDateTime = GetRowDateTime((thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));
                }
                else
                {
                    //第一页的第一行，默认认为和前一行（不存在前一行）是不相同的日期时间
                    return true;
                }
            }
            //}

            string[] arrDT = preRowDateTime.Split('¤');
            if (arrDT[0] != thisRowDate || arrDT[1] != thisRowTime)
            {
                isNotSameDateTimeToPreRow = true;
            }
            else
            {
                isNotSameDateTimeToPreRow = false;
            }

            return isNotSameDateTimeToPreRow;
        }
        # endregion 当前行和上以上是否时间相同（同段落）

        # region 非表格输入框，光标离开保存
        /// <summary>
        /// 非表格输入框，光标离开保存 数据到XML
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rtbe_LeaveSaveDataToXml(object sender, EventArgs e)
        {
            RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;

            if (rtbe.Rtf != rtbe.OldRtf)
            {
                if (rtbe.Name.StartsWith("日期") && rtbe._FullDateText == rtbe.OldText)
                {
                    //日期时间的格式是简化格式，光标进入会变成完整格式，但是如果没有修改-只是光标进入呢
                    if (rtbe._YMD_Old != rtbe._YMD)
                    {
                        _CurrentCellNeedSave = true;
                        IsNeedSave = true;
                    }
                }
                else
                {
                    IsNeedSave = true;
                }
            }
            else
            {
                //_IsNeedSave = false; //不需要设置，因为之前可能已经是True需要提示保存确认对话框了。
            }
        }
        # endregion 非表格输入框，光标离开保存

        # region 输入框内容验证（非表格）
        /// <summary>
        /// 输入框内容验证（非表格）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RtbeLeave_Validating(object sender, EventArgs e)
        {
            RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;

            //表格输入框不再这里验证，在Cells_LeaveSaveData中进行验证
            if (rtbe._IsTable)
            {
                return;
            }

            string errMsg = "";

            if (rtbe.Name.StartsWith("时间"))
            {
                errMsg = "输入的时间格式不正确，有效格式为：HH:mm";

                //全角转换成半角
                rtbe.Text = StrHalfFull.ToDBC_Half(rtbe.Text);  //这里时间可以处理，但是日期不行，因为会影响格式化


                //直接输入数字，格式化成时间的处理
                NumberToTime(rtbe);

                //时间
                if (!DateHelp.IsTime(rtbe.Text) && rtbe.Text.Trim() != "")
                {
                    ShowSearch(_Need_Assistant_Control); // 显示指示图标

                    if (!_ErrorList.Contains(rtbe.Name + "：" + errMsg))
                    {
                        _ErrorList.Add(rtbe.Name + "：" + errMsg);
                    }

                    MessageBox.Show(errMsg, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                else
                {
                    _ErrorList.Remove(rtbe.Name + "：" + errMsg);
                }
            }
            else if (rtbe.Name.StartsWith("日期"))
            {
                errMsg = "输入的日期格式不正确，有效格式为：yyyy-MM-dd";

                //全角转换成半角
                //rtbe.Text = StrHalfFull.ToDBC_Half(rtbe.Text);

                //日期
                if (!DateHelp.IsDate(rtbe.Text) && !DateHelp.IsDate(rtbe._FullDateText) && rtbe.Text.Trim() != "")
                {
                    ShowSearch(_Need_Assistant_Control); // 显示指示图标

                    if (!_ErrorList.Contains(rtbe.Name + "：" + errMsg))
                    {
                        _ErrorList.Add(rtbe.Name + "：" + errMsg);
                    }

                    MessageBox.Show(errMsg, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                else
                {
                    _ErrorList.Remove(rtbe.Name + "：" + errMsg);
                }

            }
            else if (rtbe.Name.Contains("日期") || rtbe.Name.Contains("时间"))
            {
                //<InputBox ID="00203" Name="记录时间" PointStart="553,1050" DateFormat="yyyy-MM-dd HH:mm" 
                //日期时间 时分一起的
                if (rtbe._YMD_Default != "" && rtbe._YMD_Default.Contains("yyyy") && rtbe._YMD_Default.Contains("mm"))
                {
                    DateTime temp;
                    errMsg = "输入的日期时间格式不正确，有效格式为：" + rtbe._YMD_Default;


                    //全角转换成半角
                    //rtbe.Text = StrHalfFull.ToDBC_Half(rtbe.Text);  //这里时间可以处理，但是日期不行，因为会影响格式化

                    if (!DateTime.TryParse(rtbe.Text, out temp) && rtbe.Text.Trim() != "")
                    {
                        ShowSearch(_Need_Assistant_Control); // 显示指示图标

                        if (!_ErrorList.Contains(rtbe.Name + "：" + errMsg))
                        {
                            _ErrorList.Add(rtbe.Name + "：" + errMsg);
                        }

                        MessageBox.Show(errMsg, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }
                    else
                    {
                        _ErrorList.Remove(rtbe.Name + "：" + errMsg);
                    }
                }
            }

            if (rtbe.Name.Contains("日期") || rtbe.Name.Contains("时间"))
            //if (rtbe.Name == "日期" || rtbe.Name == "时间")
            {
                if (DateHelp.IsDateTime(rtbe.Text))
                {
                    string StrMessageValue = rtbe.Name + "：日期时间不合理，不能晚于规定时间。";
                    DateTime dtRow = DateTime.Parse(rtbe.Text);
                    TimeSpan ts = dtRow.Subtract(GetServerDate());
                    if (ts.TotalMinutes > _DSS.InputDateTimeValidate)
                    {
                        if (!_ErrorList.Contains(StrMessageValue))
                        {
                            _ErrorList.Add(StrMessageValue);
                        }

                        ShowSearch(_Need_Assistant_Control); // 显示指示图标

                        MessageBox.Show(StrMessageValue, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        _ErrorList.Remove(StrMessageValue);
                    }
                }
            }

            //每个输入框配置的正则表达式验证规则，判断是否满足格式规定
            if (!string.IsNullOrEmpty(rtbe._CheckRegex))
            {
                errMsg = "输入的数据格式不正确。";

                if (Regex.IsMatch(rtbe.Text.Trim(), rtbe._CheckRegex)) // || string.IsNullOrEmpty(rtbe.Text.Trim())
                {
                    _ErrorList.Remove(rtbe.Name + "：" + errMsg);
                }
                else
                {
                    ShowSearch(_Need_Assistant_Control); // 显示指示图标

                    if (!_ErrorList.Contains(rtbe.Name + "：" + errMsg))
                    {
                        _ErrorList.Add(rtbe.Name + "：" + errMsg);
                    }

                    MessageBox.Show(errMsg, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

            }


            //_ErrorRtbe = null;
        }
        # endregion 输入框内容验证（非表格）

        # region 根据时间，设置行文字颜色
        /// <summary>
        /// 根据时间，设置行文字颜色，并刷新界面。
        /// 光标置入，设置默认值也要执行
        /// </summary>
        /// <param name="rtbe"></param>
        private void SetCurrentRowForeColor(RichTextBoxExtended rtbe)
        {
            //行文字颜色自动变色
            RowForeColors rfc;
            string newRowInfor = "";

            //更新到xml中，一遍数据更新后统计
            XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            int nodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID;
            XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));
            //"Record[@ID='" + nodeId.ToString() + "']"

            string name = rtbe.Name;
            string value = "";
            string rowInforPara = "";

            //护理部要求写入24:00
            string strTimeHHmm = rtbe.Text == "24:00" ? "23:59" : rtbe.Text;
            DateTime thisRowTime = DateTime.Parse("2017-07-01 " + strTimeHHmm);

            for (int i = 0; i < _RowForeColorsList.Count; i++)
            {
                rfc = _RowForeColorsList[i];

                if (rfc.DtForm.CompareTo(thisRowTime) <= 0 && rfc.DtTo.CompareTo(thisRowTime) >= 0)
                {
                    if (rfc.ForeColor != rtbe.ForeColor || rfc.ForeColor.Name != _TableInfor.Rows[_CurrentCellIndex.X].RowForeColor)
                    {
                        IsNeedSave = true;
                        Comm._SettingRowForeColor = true;

                        rtbe.ForeColor = rfc.ForeColor; //配置的默认字体颜色已经在保存在输入框的DefaultForeColor中了，其他行可以在恢复的。

                        _TableInfor.Rows[_CurrentCellIndex.X].RowForeColor = rfc.ForeColor.Name;//设置行的文字颜色属性

                        //更新保存的行颜色
                        //行线颜色 更新
                        newRowInfor = "";
                        newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].RowLineColor.Name + "¤";
                        newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType.ToString() + "¤";
                        newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].RowForeColor + "¤";
                        newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor.Name;

                        (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), newRowInfor);      //行信息

                        rtbe.SelectAll();
                        rtbe.SelectionColor = rfc.ForeColor; //并且，这一行的所有列都要设置指定的颜色

                        //如果时间是富文本，也要保存
                        if (_TableInfor.Columns[_CurrentCellIndex.Y].RTBE._IsRtf)
                        {
                            value = GetCellSaveValue(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE, _TableInfor.CurrentCell, ref rowInforPara);
                            (cellNode as XmlElement).SetAttribute(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Name, value); //单元格信息
                        }

                        int x = _CurrentCellIndex.X;
                        int y = _CurrentCellIndex.Y;

                        _TableInfor.CurrentCell.Text = _TableInfor.Columns[y].RTBE.Text;
                        _TableInfor.CurrentCell.Rtf = _TableInfor.Columns[y].RTBE.Rtf;

                        this.pictureBoxBackGround.SuspendLayout();
                        _IsLoading = true; //防止更新文字工具栏的粗体等状态

                        RedrawRow(_TableInfor, x, true, cellNode);  //整个刷新太慢 Redraw(); //刷新整行颜色

                        _IsLoading = false;
                        this.pictureBoxBackGround.ResumeLayout();

                        # region 整行设置行文字颜色 原来这种方式会刷新
                        //_IsLoading = true;
                        //_IsNotNeedCellsUpdate = true; //不然处理过快 override void WndProc(ref Message m)会报错

                        ////整行都要变色
                        //for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                        //{
                        //    if (y != j)
                        //    {
                        //        //跳过合并的单元格，而且不跳过，下面代码没有设置到单元格，RTBE取值赋值会有产生数据混乱
                        //        if (!_TableInfor.Cells[x, j].IsMergedNotShow)
                        //        {
                        //            ShowCellRTBE(x, j);

                        //            _TableInfor.Columns[j].RTBE.SelectAll();
                        //            _TableInfor.Columns[j].RTBE.SelectionColor = rfc.ForeColor; //设置的时候，即使已经是rtf的也要重置颜色。但是再次打开显示的时候如果修改成rtf，就要以修改后的为准

                        //            _TableInfor.CurrentCell.Text = _TableInfor.Columns[j].RTBE.Text;
                        //            _TableInfor.CurrentCell.Rtf = _TableInfor.Columns[j].RTBE.Rtf;

                        //            //按照日期格式再显示
                        //            if (_TableInfor.Columns[j].RTBE.Name.StartsWith("日期"))
                        //            {
                        //                _TableInfor.Columns[j].RTBE.LeaveShowDate();
                        //            }

                        //            Redraw(_TableInfor.Columns[j].RTBE);

                        //            //如果之前已经是Rtf不同颜色的文字，因为已经强行替换颜色，所以这里需要保存
                        //            if (_TableInfor.Columns[j].RTBE._IsRtf)
                        //            {
                        //                value = GetCellSaveValue(_TableInfor.Columns[j].RTBE, _TableInfor.CurrentCell, ref rowInforPara);
                        //                (cellNode as XmlElement).SetAttribute(_TableInfor.Columns[j].RTBE.Name, value); //单元格信息
                        //            }
                        //        }

                        //    }
                        //}

                        //_IsLoading = false;
                        //_IsNotNeedCellsUpdate = false;
                        # endregion 整行设置行文字颜色

                        Comm._SettingRowForeColor = false; //取消行文字颜色设置状态标记
                    }
                }
            }
        }

        private string GetRowForeColor(string timePara)
        {
            string retColorName = "";

            //行文字颜色自动变色
            RowForeColors rfc;


            //护理部要求写入24:00
            string strTimeHHmm = timePara == "24:00" ? "23:59" : timePara;
            DateTime thisRowTime = DateTime.Parse("2017-07-01 " + strTimeHHmm);

            for (int i = 0; i < _RowForeColorsList.Count; i++)
            {
                rfc = _RowForeColorsList[i];

                if (rfc.DtForm.CompareTo(thisRowTime) <= 0 && rfc.DtTo.CompareTo(thisRowTime) >= 0)
                {
                    retColorName = rfc.ForeColor.Name;
                }
            }

            return retColorName;
        }

        /// <summary>
        ///要求只有危重病人在规定时间内要变红，这个护士需要一个一个格子把颜色改回来
        /// 以前只能选某个单元格的文字设置颜色，或者根据时间设置整行颜色。
        /// 现在这个方法实现根据手动选择的颜色来设置行文字颜色。按下Ctrl+设置单元格文字颜色来触发。
        /// </summary>
        /// <param name="rtbe"></param>
        private void SetCurrentRowForeColor(RichTextBoxExtended rtbe, Color colorPara)
        {
            //行文字颜色手动变色
            string newRowInfor = "";

            //更新到xml中，一遍数据更新后统计
            XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            int nodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID;
            XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));
            string value = "";
            string rowInforPara = "";

            //if (colorPara.Name != _TableInfor.Rows[_CurrentCellIndex.X].RowForeColor)
            //{
            IsNeedSave = true;
            Comm._SettingRowForeColor = true;

            rtbe.ForeColor = colorPara; //配置的默认字体颜色已经在保存在输入框的DefaultForeColor中了，其他行可以在恢复的。

            _TableInfor.Rows[_CurrentCellIndex.X].RowForeColor = colorPara.Name;//设置行的文字颜色属性

            //更新保存的行颜色
            //行线颜色 更新
            newRowInfor = "";
            newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].RowLineColor.Name + "¤";
            newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType.ToString() + "¤";
            newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].RowForeColor + "¤";
            newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor.Name;

            (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), newRowInfor);      //行信息

            rtbe.SelectAll();
            rtbe.SelectionColor = colorPara; //并且，这一行的所有列都要设置指定的颜色

            //如果时间是富文本，也要保存
            if (_TableInfor.Columns[_CurrentCellIndex.Y].RTBE._IsRtf)
            {
                value = GetCellSaveValue(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE, _TableInfor.CurrentCell, ref rowInforPara);
                (cellNode as XmlElement).SetAttribute(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Name, value); //单元格信息
            }

            int x = _CurrentCellIndex.X;
            int y = _CurrentCellIndex.Y;

            _TableInfor.CurrentCell.Text = _TableInfor.Columns[y].RTBE.Text;
            _TableInfor.CurrentCell.Rtf = _TableInfor.Columns[y].RTBE.Rtf;

            this.pictureBoxBackGround.SuspendLayout();
            _IsLoading = true; //防止更新文字工具栏的粗体等状态

            RedrawRow(_TableInfor, x, true, cellNode);  //整个刷新太慢 Redraw(); //刷新整行颜色

            _IsLoading = false;
            this.pictureBoxBackGround.ResumeLayout();

            Comm._SettingRowForeColor = false; //取消行文字颜色设置状态标记
            //}
        }
        # endregion 根据时间，设置行文字颜色

        # region 更新错误消息中的行号提示：添加行，删除行的时候进行更新

        /// <summary>
        /// 删除添加行的时候，更新错误消息集中的消息的行号
        /// 如果添加行将末尾行，移动到了下一页，也要排除
        /// </summary>
        /// <param name="isDelRow">True为删除行，False为添加行</param>
        /// <param name="CurrentRow"></param>
        private void UpdateErrorList(bool isDelRow, int CurrentRow)
        {
            if (_ErrorList != null && _ErrorList.Count > 0)
            {
                CurrentRow++;

                if (isDelRow)
                {
                    for (int i = _ErrorList.Count - 1; i >= 0; i--)
                    {
                        if (_ErrorList[i].Contains("第" + CurrentRow.ToString() + "行"))
                        {
                            _ErrorList.RemoveAt(i);
                        }
                    }

                    int indexNo = 0;
                    for (int i = _ErrorList.Count - 1; i >= 0; i--)
                    {
                        indexNo = _ErrorList[i].IndexOf("第");
                        if (_ErrorList[i].Length > indexNo + 1) //不是最后一个字符
                        {
                            if (int.TryParse(_ErrorList[i].Substring(indexNo + 1, 1), out indexNo))
                            {
                                //“第”后面是数字，那么肯定是行号
                                if (indexNo > CurrentRow)
                                {
                                    _ErrorList[i] = _ErrorList[i].Replace("第" + indexNo.ToString() + "行", "第" + (indexNo - 1).ToString() + "行");
                                }
                            }
                        }
                    }
                }
                else
                {
                    int indexNo = 0;

                    for (int i = _ErrorList.Count - 1; i >= 0; i--)
                    {
                        indexNo = _ErrorList[i].IndexOf("第");
                        if (_ErrorList[i].Length > indexNo + 1) //不是最后一个字符
                        {
                            if (int.TryParse(_ErrorList[i].Substring(indexNo + 1, 1), out indexNo))
                            {
                                //“第”后面是数字，那么肯定是行号
                                if (indexNo > CurrentRow)
                                {
                                    _ErrorList[i] = _ErrorList[i].Replace("第" + indexNo.ToString() + "行", "第" + (indexNo + 1).ToString() + "行");
                                }
                            }
                        }
                    }
                }

                //移到下一页的数据，如果跳过那也保存进去了，存在了异常不合理的数据了；这样也不好，，纠结
            }
        }

        # endregion 更新错误消息中的行号提示：添加行，删除行的时候进行更新

        #region xml排序
        //XML的内容：
        //<?xml version="1.0" encoding="utf-8"?>
        //<UserPattern>
        //  <Tag name="本帮菜" Tag_Weight="4.833333" IDF="1.819544" />
        //  <Tag name="沪上红烧第一" Tag_Weight="1.666667" IDF="2.281942" />
        //  <Tag name="商务宴请" Tag_Weight="2.833333" IDF="2.051493" />
        //  <Tag name="素食" Tag_Weight="1.116667" IDF="2.455867" />
        //  <Tag name="书卷气" Tag_Weight="0.3333333" IDF="2.980912" />
        //  <Tag name="安静" Tag_Weight="0.5" IDF="2.804821" />
        //  <Tag name="酒店" Tag_Weight="0.6166667" IDF="2.71374" />
        //  <Tag name="经济型" Tag_Weight="0.2" IDF="3.202761" />
        //  <Tag name="电脑" Tag_Weight="0.2" IDF="3.202761" />
        //  <Tag name="订票" Tag_Weight="0.2" IDF="3.202761" />
        //  <Tag name="卫浴" Tag_Weight="0.45" IDF="2.850578" />
        //  <Tag name="旅游" Tag_Weight="0.75" IDF="2.628729" />
        //  <Tag name="会展" Tag_Weight="0.25" IDF="3.105851" />
        //</UserPattern>

        ////根据属性Tag_Weight排序，代码如下：
        //private void SortXMLDoc_Test(string address)
        //{
        //    XmlDocument dom = new XmlDocument();
        //    dom.Load(address);
        //    XPathDocument pathdoc = new XPathDocument(address);
        //    //XmlNodeList nodes = dom.SelectSingleNode("UserPattern").ChildNodes;

        //    XmlNode xn = dom.SelectSingleNode("UserPattern");

        //    XPathNavigator nav = pathdoc.CreateNavigator();
        //    string xpath = String.Format("/UserPattern/Tag");
        //    XPathExpression exp = nav.Compile(xpath);
        //    exp.AddSort("@Tag_Weight",XmlSortOrder.Descending,XmlCaseOrder.None,"",XmlDataType.Text);
        //    XPathNodeIterator nodeIter = nav.Select(exp);

        //    xn.RemoveAll();

        //    while (nodeIter.MoveNext())
        //    {
        //        XmlElement xe = dom.createElement_x("Tag");
        //        xe.SetAttribute("name", nodeIter.Current.GetAttribute("name", ""));
        //        xe.SetAttribute("Tag_Weight", nodeIter.Current.GetAttribute("Tag_Weight", ""));
        //        xe.SetAttribute("IDF", nodeIter.Current.GetAttribute("IDF", ""));
        //        xn.AppendChild(xe);
        //    }
        //    dom.Save(address);
        //}

        //根据属性Tag_Weight排序，代码如下：
        private void SortXMLDoc(XmlDocument xmldocPara)
        {
            try
            {
                string noSortXml = xmldocPara.InnerXml;

                int allRcordsCount = xmldocPara.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))).ChildNodes.Count;

                if (_AutoSortMode == "2")
                {
                    //中间有空白行，去删除
                    RemoveEmptyRowsAll(_RecruitXML);   //所有空行。（如果是调整段落的行，那么日期时间有内容的话，其实也应该认为空白行。。。。）；下面排序、规范化日期后再移动一次空行

                    //只有日期时间，如果是上一行日期时间一样，那么也认为是空行。 调整段落后，可能导致空行的，跨页的时候，还可能有日期和时间
                }
                else
                {
                    RemoveEmptyRows(_RecruitXML);   //末尾空行
                }

                int noEmptyRcordsCount = xmldocPara.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))).ChildNodes.Count;
                int removedEmptyRecordCount = allRcordsCount - noEmptyRcordsCount;

                XmlNode xnRoot = xmldocPara.SelectSingleNode(nameof(EntXmlModel.NurseForm));
                XmlNode xnRecords = xnRoot.SelectSingleNode(nameof(EntXmlModel.Records));

                if (int.TryParse((xnRecords as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID)), out _TableInfor.MaxId))
                {

                }

                if (xnRecords != null && xnRecords.ChildNodes.Count > 0)
                {
                    //SortValue：日期+时间+上下顺序NO好（调整段罗列的行顺序，只有一行那么也从1开始）
                    //遍历整个xml，将日期时间和顺序号，写入属性SortValue中，下面开始排序
                    string date = (xnRoot.ChildNodes[0] as XmlElement).GetAttribute("日期").Trim();
                    string time = (xnRoot.ChildNodes[0] as XmlElement).GetAttribute("时间").Trim();
                    int index = 0;  //"00001"

                    DateTime dateCheck = GetServerDate();

                    //序号要前面补零的
                    //方法1：Console.WriteLine(i.ToString("D5"));
                    //方法2：Console.WriteLine(i.ToString().PadLeft(5,'0'));//推荐
                    //方法3：Console.WriteLine(i.ToString("00000")); 

                    ////如果第一行数据没有日期那么跳出：
                    //if (string.IsNullOrEmpty((xnRoot.ChildNodes[0] as XmlElement).GetAttribute("日期").Trim()))
                    //{
                    //    ShowInforMsg("无法自动排序：第一行必须填写正确的日期内容，不能为空。", true);
                    //    return;
                    //}

                    //if (_LordClosing || MessageBox.Show("是否需要自动排序？", "补录时，请填写日期和时间", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    //{

                    string dateEach = "1900-01-01";
                    string timeEach = "00:00";
                    CellInfor thisCell;
                    string value = "";

                    for (int i = 0; i < xnRecords.ChildNodes.Count; i++)
                    {
                        ////空行跳过：最下面的空白行如果设置的日期，那就不好了。但是前面的空白还是要处理的
                        //if (isEmptyRecord(xnRecords.ChildNodes[i]))
                        //{
                        //    continue;
                        //}

                        dateEach = (xnRecords.ChildNodes[i] as XmlElement).GetAttribute("日期").Split('¤')[0].Trim();
                        timeEach = (xnRecords.ChildNodes[i] as XmlElement).GetAttribute("时间").Split('¤')[0].Trim();

                        //如果这样日期为空，那么去前面的，如果时间也为空，也取前面的
                        if (string.IsNullOrEmpty(dateEach))
                        {
                            dateEach = date;
                        }
                        else
                        {
                            //如果日期格式不正确，主要是整行小结时-合并单元格的时候。就不用按错误内容排序，用上一行的日期
                            if (!DateTime.TryParse(dateEach + " 00:01", out dateCheck))
                            {
                                dateEach = date;
                            }
                        }

                        if (string.IsNullOrEmpty(timeEach))
                        {
                            timeEach = time;
                        }

                        //如果日期时间相同，序号累加；否则从1开始
                        if (dateEach == date && timeEach == time)
                        {

                        }
                        else
                        {
                            index = 0;
                        }

                        index++;

                        //日期和时间值保存到最近上一次的值
                        //if (!string.IsNullOrEmpty(dateEach))
                        if (dateEach != date)
                        {
                            date = dateEach;
                        }

                        //if (!string.IsNullOrEmpty(timeEach))
                        if (time != timeEach)
                        {
                            time = timeEach;
                        }

                        //设置排序属性：日期时间序号
                        //(xnRecords.ChildNodes[i] as XmlElement).SetAttribute(nameof(EntXmlModel.SortValue), dateEach + " " + timeEach + " " + index.ToString("D8"));
                        //如果补录的日期时间和现有数据一致，那么索引号就有多个1，2……，那么就不是接尾。所以加上i的索引号
                        (xnRecords.ChildNodes[i] as XmlElement).SetAttribute(nameof(EntXmlModel.SortValue), dateEach + " " + timeEach + " " + i.ToString("D8"));

                        //防止补录的是某一天最早时间的&日期没有填写，那样第一排序后，日期就没有。再次保存排序就变成上一行（前一天的）日期重新排序了
                        if (!string.IsNullOrEmpty(dateEach) && string.IsNullOrEmpty((xnRecords.ChildNodes[i] as XmlElement).GetAttribute("日期").Split('¤')[0].Trim()))
                        {
                            //日期="2017-05-07¤False¤False¤Mandalat¤False¤False¤False¤¤Black¤¤¤MM-dd"
                            //如果第一页第一行，之中是无法补上的（如果在这里处理的话）；但日期为空的始终排序后在最前位置了
                            ////string value = dateEach + "¤False¤False¤" + "" + "¤False¤False¤False¤¤Black¤¤¤";  //格式串为空，就显示为完整的年月日了
                            ////(xnRecords.ChildNodes[i] as XmlElement).SetAttribute("日期", value);

                            //直接写入一个数值，没有其他小太阳分割的，也不会报错，但是不太好
                            //(xnRecords.ChildNodes[i] as XmlElement).SetAttribute("日期", dateEach);

                            ////if (arrRtbeValue.Length == 1)
                            ////{arrRtbeValue = (xe.Value + "¤False¤False¤¤False¤False¤False¤¤Black¤").Split('¤');
                            ////thisCell.DateFormat = tableInforPara.Columns[thisCell.ColIndex].RTBE._YMD_Default

                            //补日期和清空日期都会导致，合并单元格失效：因为格式没有了；如果整体用空，采用下面的默认串。
                            thisCell = _TableInfor.CellByRowNo_ColName(0, "日期");

                            if (thisCell != null)
                            {
                                value = dateEach + "¤False¤False¤¤False¤False¤False¤¤Black¤¤¤" + _TableInfor.Columns[thisCell.ColIndex].RTBE._YMD_Default;  //格式串为空，就显示为完整的年月日了
                                (xnRecords.ChildNodes[i] as XmlElement).SetAttribute("日期", value);
                            }
                        }
                    }

                    XPathNavigator nav = xmldocPara.CreateNavigator();
                    string xpath = String.Format("/" + EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records), nameof(EntXmlModel.Record)));//NurseForm/Records/Record
                    XPathExpression exp = nav.Compile(xpath);
                    //exp.AddSort("@SortValue", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Text);
                    exp.AddSort("@" + nameof(EntXmlModel.SortValue), XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Text);  //SortValue：日期+时间+上下顺序NO好（调整段罗列的行顺序）
                    XPathNodeIterator nodeIter = nav.Select(exp);


                    StringBuilder sorted = new StringBuilder();

                    while (nodeIter.MoveNext())
                    {
                        //sorted.Append(nodeIter.Current.Value + "\r\n");  // + "\n"
                        sorted.Append(nodeIter.Current.OuterXml);
                    }

                    //移除后重新赋值
                    //xnRecords.RemoveAll();//只要移除子节点
                    for (int i = xnRecords.ChildNodes.Count - 1; i >= 0; i--)
                    {
                        //移除子节点
                        xnRecords.RemoveChild(xnRecords.ChildNodes[i]);
                    }

                    xnRecords.InnerXml = sorted.ToString();


                    //删除排序属性：SortValue，防止很多地方
                    for (int i = 0; i < xnRecords.ChildNodes.Count; i++)
                    {
                        //移除指定属性：SortValue
                        (xnRecords.ChildNodes[i] as XmlElement).RemoveAttribute(nameof(EntXmlModel.SortValue));
                    }

                    //补上空数据
                    XmlNode recordNodeNew = null;
                    for (int i = 0; i < removedEmptyRecordCount; i++)
                    {
                        recordNodeNew = TemplateHelp.AddXmlRecordNode(_RecruitXML);
                        int temp = _TableInfor.AddRowGetMaxID();
                        (recordNodeNew as XmlElement).SetAttribute(nameof(EntXmlModel.ID), temp.ToString());

                        //XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                        (xnRecords as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), temp.ToString());
                    }

                    #region 排序后，日期格式自动化修正

                    //规则：
                    //1.每页首行，必须有日期和时间
                    //2.每页，相同日期时间，只能出现一次（日期只有一次，时间在日期不一样的情况下，可多次出现。）即：每页中：相同日期时间，只能出现一次
                    //3.如果第一行日期为空，那么久无法处理了：(xnRoot.ChildNodes[0] as XmlElement).GetAttribute("日期").Trim()

                    //实现处理判断的逻辑条件
                    //从排序后的第一行开始，判断是否为某页第一行，是否在对应页中于上行的日期时间一致。
                    //1.所属的日期和时间
                    //2.存储显示的日期时间文字串
                    //3.是不是某页的第一行
                    //4.是不是和本页上一行的日期时间相同
                    if (xnRecords.ChildNodes.Count > 0) // && _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间") != null 记录大于一行，并且含有时间
                    {
                        //单元格的用户id等信息也被复制过去了，行颜色没有再次确定
                        //ROW="Black¤None¤Blue¤Red"  //修改第三个行文字颜色，行线等不用继承
                        //日期="2017-05-07¤False¤False¤Mandalat¤False¤False¤False¤¤Black¤¤¤MM-dd"  //修改第四个用户id变成空，当前用户也不行 

                        string saveRowFontColor = "";
                        string[] arrEach;

                        string saveDate = (xnRecords.ChildNodes[0] as XmlElement).GetAttribute("日期");
                        string saveTime = (xnRecords.ChildNodes[0] as XmlElement).GetAttribute("时间");
                        string ownDate = (xnRecords.ChildNodes[0] as XmlElement).GetAttribute("日期").Split('¤')[0].Trim();
                        string ownTime = (xnRecords.ChildNodes[0] as XmlElement).GetAttribute("时间").Split('¤')[0].Trim();

                        string ownDatePre = ownDate;
                        string ownTimePre = ownTime;

                        int rowCount = 0; //改行记录在表格中所处的行号；每页第一行为1
                        //int recordsCount = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))).ChildNodes.Count; // 行数

                        for (int i = 1; i < xnRecords.ChildNodes.Count; i++)
                        {
                            //计算该行记录，在表格中的行号。rowCount为1的时候，表示是某一页的第一行
                            TableClass.GetPageCount_HaveTable(_TemplateXml, i + 1, ref rowCount);  //索引1的节点是第二个节点，第二行

                            //saveDate = (xnRecords.ChildNodes[i] as XmlElement).GetAttribute("日期");
                            //saveTime = (xnRecords.ChildNodes[i] as XmlElement).GetAttribute("时间");


                            dateEach = (xnRecords.ChildNodes[i] as XmlElement).GetAttribute("日期").Split('¤')[0].Trim();
                            timeEach = (xnRecords.ChildNodes[i] as XmlElement).GetAttribute("时间").Split('¤')[0].Trim();

                            if (!string.IsNullOrEmpty(dateEach))
                            {
                                saveDate = (xnRecords.ChildNodes[i] as XmlElement).GetAttribute("日期");
                                ownDate = dateEach;
                            }

                            if (!string.IsNullOrEmpty(timeEach))
                            {
                                saveTime = (xnRecords.ChildNodes[i] as XmlElement).GetAttribute("时间");
                                ownTime = timeEach;

                                //记录上一行的行文字颜色，这样在手动手机行颜色等时候，会有问题。还是要根据日期时间重新计算
                                if (!string.IsNullOrEmpty((xnRecords.ChildNodes[i] as XmlElement).GetAttribute(nameof(EntXmlModel.ROW))))
                                {
                                    arrEach = (xnRecords.ChildNodes[i] as XmlElement).GetAttribute(nameof(EntXmlModel.ROW)).Split('¤');

                                    if (arrEach.Length >= 3)
                                    {
                                        if (!string.IsNullOrEmpty(arrEach[2]))
                                        {
                                            saveRowFontColor = arrEach[2];
                                        }
                                    }
                                }
                            }

                            //GetRowDateTime((xnRecords.ChildNodes[i] as XmlElement).GetAttribute(nameof(EntXmlModel.ID))) 
                            if (rowCount == 1)
                            {
                                //如果删除空白行，那么会导致，后面补的新的空行补上日期就变成有效数据
                                if (i >= xnRecords.ChildNodes.Count - removedEmptyRecordCount) //_AutoSortMode == "2" && 
                                {
                                    //不用处理
                                    //如果是补的空行，那么在插入行等操作下，都可能导致跨页下一页了，补上日期，就会导致数据显示不对
                                }
                                else
                                {

                                    //第一页，始终都要日期和时间
                                    if (string.IsNullOrEmpty(dateEach))
                                    {
                                        (xnRecords.ChildNodes[i] as XmlElement).SetAttribute("日期", saveDate);
                                    }

                                    if (string.IsNullOrEmpty(timeEach))
                                    {
                                        (xnRecords.ChildNodes[i] as XmlElement).SetAttribute("时间", saveTime);
                                    }

                                    //需要再次判断行文字的颜色nameof(EntXmlModel.ROW)  ROW="Black¤None¤¤Red"
                                    //ROW="Black¤None¤Blue¤Red"  //修改第三个行文字颜色，行线等不用继承
                                    //日期="2017-05-07¤False¤False¤Mandalat¤False¤False¤False¤¤Black¤¤¤MM-dd"  //修改第四个用户id变成空，当前用户也不行 

                                    //根据时间重新计算：GetRowForeColor(时间)
                                    if (!string.IsNullOrEmpty(ownTime) && DateHelp.IsTime(ownTime))
                                    {
                                        saveRowFontColor = GetRowForeColor(ownTime);
                                    }

                                    //利用上一行的颜色，可能会有问题
                                    if (!string.IsNullOrEmpty(saveRowFontColor)) //某页第一行，可能是原来的空行排序后变成第一行，自动补录的日期时间，就没有行文字颜色，那么需要补上
                                    {
                                        if (!string.IsNullOrEmpty((xnRecords.ChildNodes[i] as XmlElement).GetAttribute(nameof(EntXmlModel.ROW))))
                                        {
                                            arrEach = (xnRecords.ChildNodes[i] as XmlElement).GetAttribute(nameof(EntXmlModel.ROW)).Split('¤');

                                            if (arrEach.Length >= 3)
                                            {
                                                if (string.IsNullOrEmpty(arrEach[2]))
                                                {
                                                    arrEach[2] = saveRowFontColor;

                                                    (xnRecords.ChildNodes[i] as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), string.Join("¤", arrEach));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //根据这一行压根没有这个属性，空行排序到第一行显示了日期时间导致的
                                            (xnRecords.ChildNodes[i] as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), "Black¤None¤" + saveRowFontColor);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //日期规范：AutoSortMode
                                //默认空和0，表示：同一页中，只要日期和上一行一样就为空，不填写，哪怕遇到时间不一样的其他段落
                                //1表示，同段落日期只显示第一行，但是不同段落都要显示日期，哪怕日期相同
                                if (_AutoSortMode == "" || _AutoSortMode == "0")
                                {
                                    //同一页，如果日期相同，则不填，只填时间。
                                    if (ownDate == ownDatePre)
                                    {
                                        (xnRecords.ChildNodes[i] as XmlElement).SetAttribute("日期", ""); //会导致包含日期时间的合并的单元格丢失
                                    }
                                    else
                                    {
                                        (xnRecords.ChildNodes[i] as XmlElement).SetAttribute("日期", saveDate);
                                    }

                                    if (ownDate + ownTime == ownDatePre + ownTimePre)
                                    {
                                        (xnRecords.ChildNodes[i] as XmlElement).SetAttribute("时间", "");
                                    }
                                    else
                                    {
                                        (xnRecords.ChildNodes[i] as XmlElement).SetAttribute("时间", saveTime);
                                    }
                                }
                                else
                                {
                                    //每个段落都有日期和时间，尽管同一页，日期一样、时间不一样。
                                    if (ownDate + ownTime == ownDatePre + ownTimePre)
                                    {
                                        (xnRecords.ChildNodes[i] as XmlElement).SetAttribute("日期", "");
                                        (xnRecords.ChildNodes[i] as XmlElement).SetAttribute("时间", "");
                                    }
                                    else
                                    {
                                        (xnRecords.ChildNodes[i] as XmlElement).SetAttribute("日期", saveDate);
                                        (xnRecords.ChildNodes[i] as XmlElement).SetAttribute("时间", saveTime);
                                    }
                                }
                            }

                            ownDatePre = ownDate;
                            ownTimePre = ownTime;
                        }

                    }

                    #endregion 排序后，日期格式自动化修正

                    ////（如果是调整段落的行，那么日期时间有内容的话，其实也应该认为空白行。。。。） 中间的空行，保存时，是不会移除的。
                    //if (_AutoSortMode == "2")
                    //{
                    //    allRcordsCount = xmldocPara.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))).ChildNodes.Count;

                    //    //中间有空白行，去删除
                    //    RemoveEmptyRowsAll(_RecruitXML);   //规范化日期后再移除一次空行

                    //    noEmptyRcordsCount = xmldocPara.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))).ChildNodes.Count;
                    //    removedEmptyRecordCount = allRcordsCount - noEmptyRcordsCount;
                    //    for (int i = 0; i < removedEmptyRecordCount; i++)
                    //    {
                    //        recordNodeNew = MyXml.AddXmlRecordNode(_RecruitXML);
                    //        int tempId = _TableInfor.AddRowGetMaxID();
                    //        (recordNodeNew as XmlElement).SetAttribute(nameof(EntXmlModel.ID), tempId.ToString());
                    //        (xnRecords as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), tempId.ToString());
                    //    }
                    //}

                    //重新按照排序后的数据显示----------------------------------------------------------------
                    //当前页，重新载入表格，并重绘，选中新位置的行
                    ReSetTableInfor();

                    LoadTable(_TableInfor, false);

                    _IsLoading = true;
                    Redraw();
                    _IsLoading = false;
                    //重新按照排序后的数据显示----------------------------------------------------------------

                    if (noSortXml == xmldocPara.InnerXml)
                    {
                        ShowInforMsg(GlobalVariable.LoginUserInfo.UserCode + "无需排序：" + this._patientInfo.PatientId + " | " + _TemplateName, false);
                    }
                    else
                    {
                        ShowInforMsg(GlobalVariable.LoginUserInfo.UserCode + " 排序：" + this._patientInfo.PatientId + " | " + _TemplateName, false);
                    }
                    //}
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        #endregion xml排序

        # region 获取表单数据内容
        /// <summary>
        /// 从数据库，获取指定表单内容；并且判断要不要锁住该记录（lockPara：0-只检索，1-更新成自己占有，-1清除占有）
        /// </summary>
        /// <param name="yearPara">年度表的年份</param>
        /// <param name="uhidPara">病人唯一号</param>
        /// <param name="templateNamePara">表单名</param>
        /// <param name="lockPara">检索标识：-1，0，1</param>
        private void GetTemplate(string yearPara, string uhidPara, string templateNamePara, int lockPara)
        {
            string strData = GlobalVariable.NurseFormValue;
            //GlobalVariable.GetNurseRecruitData(uhidPara, this._patientInfo.Id, templateNamePara, ref statusArr);

            if (string.IsNullOrEmpty(strData))
            {
                IsNeedSave = true;
                _IsCreatedTemplate = true;

                ////没有检索到，那么创建一个空的xml数据文件
                TemplateHelp mx = new TemplateHelp();
                _RecruitXML = mx.CreateTemplate(this._patientInfo.PatientId, this._patientInfo.PatientName, GlobalVariable.LoginUserInfo.UserCode, GlobalVariable.LoginUserInfo.DeptName, _TemplateXml, _TemplateName);

                int creatPages = 1;

                #region 自动创建n页 自动创建后打开第一页比较好
                //自动创建n页的处理…… 创建打开表单的时候处理。这样还能先打开第一页。也符合业务逻辑。
                XmlNode tempRoot = _TemplateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm));

                if (!int.TryParse((tempRoot as XmlElement).GetAttribute(nameof(EntXmlModel.AutoCreatPagesCount)), out creatPages))
                {
                    creatPages = 1;
                }

                //自动创建指定页:填写好默认的数据和抬头基本信息；这样每次开大创建的其他页，如果不做修改就不会提示保存了
                if (creatPages > 1)
                {
                    XmlNode thisPageNode = null;
                    XmlNode xnEach = null;  //模板节点
                    XmlNode thisPageData;
                    string strName = "";

                    for (int i = 2; i <= creatPages; i++)
                    {
                        //获取，当前模板页配置节点
                        thisPageNode = GetTemplatePageNode(_TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design))), i);

                        if (thisPageNode != null)
                        {
                            //"NurseForm/Forms/Form[@SN='" + i.ToString() + "']"
                            thisPageData = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), i.ToString(), "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));  //数据页节点
                            strName = "";

                            if (thisPageData == null)
                            {
                                continue;//CreateXML中已经添加了页节点
                            }

                            for (int j = 0; j < thisPageNode.ChildNodes.Count; j++)
                            {
                                //如果是注释那么跳过
                                if (thisPageNode.ChildNodes[j].Name == @"#comment" || thisPageNode.ChildNodes[j].Name == @"#text")
                                {
                                    continue;
                                }

                                xnEach = thisPageNode.ChildNodes[j];

                                strName = (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));

                                if (xnEach.Name == nameof(EntXmlModel.BindingValue))
                                {
                                    //不等于页号
                                    if ((xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) != "_PageNo")
                                    {
                                        if (_CurrentBasicInfor != null && _CurrentBasicInfor.ContainsKey(strName))
                                        {
                                            (thisPageData as XmlElement).SetAttribute(strName, _CurrentBasicInfor[strName]);
                                        }
                                    }
                                }
                                else if (xnEach.Name.EndsWith(nameof(EntXmlModel.Text)) || xnEach.Name == nameof(EntXmlModel.ComboBox))
                                {
                                    //bool autoLoadDefault = false;
                                    //bool.TryParse((xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.AutoLoadDefault)), out autoLoadDefault);
                                    //if (autoLoadDefault)
                                    (thisPageData as XmlElement).SetAttribute(strName, (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)));
                                }
                                else if (xnEach.Name == nameof(EntXmlModel.CheckBox))
                                {
                                    (thisPageData as XmlElement).SetAttribute(strName, (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)));
                                }
                                //else if (xnEach.Name == nameof(EntXmlModel.CheckBox))
                                //{
                                //    (thisPageData as XmlElement).SetAttribute(strName, (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)));
                                //}

                            }

                        }
                    }

                    Comm.LogHelp.WriteInformation(_TemplateName + "：在连续创建N页时，对每页数据都进行了默认初始化。");
                }
                #endregion end自动创建n页

                _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.FORM_TYPE), _TemplateRight);
                _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Forms), creatPages.ToString());

                //_RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.FORM_TYPE), _TemplateRight);
                //_RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Forms), "1");
            }
            else
            {
                _RecruitXML = new XmlDocument();
                _RecruitXML.LoadXml(strData);

                //statusArr更新归档和转科病区 nameof(EntXmlModel.Done) nameof(EntXmlModel.Type)
                _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Done), GlobalVariable.Done);
                _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Type), GlobalVariable.Type);
            }
        }

        private string GetTemplate(string yearPara, string uhidPara, string templateNamePara)
        {
            //string[] statusArr = null;

            //string strData = GlobalVariable.GetNurseRecruitData(uhidPara, this._patientInfo.Id, templateNamePara, ref statusArr);

            return GlobalVariable.NurseFormValue;
        }
        # endregion 获取表单数据内容

        #region 同步更新其他表单，评估单的得分更新到护理记录单中
        /// <summary>
        /// 同步更新其他表单
        /// 调用批量接口来更新保存，当然以后根据需要也可以获取段字段解析更新后，再保存。
        /// </summary>
        private void SaveSynchronize()
        {
            //string saveSynchronize = _TemplateXml.DocumentElement.GetAttribute("NeedSaveSynchronize"); //哪些评估单需要开始
            //服务端根节点，存放那个病区，需要那些护理记录单。一般只要一个。但是icu一般会有两个：一般护理记录单，危重护理记录单。
            bool NeedSaveSynchronize = false;
            if (!string.IsNullOrEmpty(_TemplateXml.DocumentElement.GetAttribute(nameof(EntXmlModel.SaveSynchronize))))
            {
                NeedSaveSynchronize = true;
            }

            if (!NeedSaveSynchronize)
            {
                //本表单没有开启，那么就不用处理了。如果开始了，继续判断本病区是应该更新到一般护理记录还是危重护理记录单。
                return;
            }

            string saveSynchronize = "";


            XmlDocument doc = new XmlDocument();
            doc.Load(Application.StartupPath + @"\\Recruit3XML\\DataService.cfg");

            saveSynchronize = GetSaveSynchronize(doc.DocumentElement.SelectSingleNode("//生命体征共享"), GlobalVariable.LoginUserInfo.WardName);

            if (!string.IsNullOrEmpty(saveSynchronize))
            {
                Frm_SelectConfirm scf = new Frm_SelectConfirm(saveSynchronize);
                DialogResult drt = scf.ShowDialog();

                //表单名只要弹出确认消息窗体后，就完毕，现在赋值为客户端的详细配置
                saveSynchronize = _TemplateXml.DocumentElement.GetAttribute(nameof(EntXmlModel.SaveSynchronize));

                //调用批量接口的4个值。
                string date = null;
                string time = null;
                string score = null;
                string sign = null;

                //配置的控件名
                //评分
                string scoreSetting = Comm.GetPropertyByName(saveSynchronize, "评分");
                string[] arrscore = scoreSetting.Split(',');

                //评分格式串
                string signFormatSetting = Comm.GetPropertyByName(saveSynchronize, "评分格式串");
                //string[] arrsign = scoreSetting.Split(',');

                //日期
                string dateSetting = Comm.GetPropertyByName(saveSynchronize, "日期");
                string[] arrdate = dateSetting.Split(',');

                //时间
                string timeSetting = Comm.GetPropertyByName(saveSynchronize, "时间");
                string[] arrtime = timeSetting.Split(',');

                //签名：如果为空或者null，那就不要签名了。
                string signSetting = Comm.GetPropertyByName(saveSynchronize, "签名");
                string[] arrsign = signSetting.Split(',');

                //索引号
                string indexSetting = Comm.GetPropertyByName(saveSynchronize, "索引号");
                int maxIndex = 9;
                if (!int.TryParse(indexSetting, out maxIndex))
                {
                    maxIndex = 9; //默认为9
                }

                //¤扩展格式串参数@{1}=导管分类,{2}=评估详情X
                string exFormatPara = Comm.GetPropertyByName(saveSynchronize, "扩展格式串参数");
                string[] arrEx = exFormatPara.Split(',');//{1}=导管分类
                string[] arrExEach = null;

                Control[] ct = null;

                //if (MessageBox.Show("是否需要同步更新【记录单】？", "确认消息", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                if (drt == DialogResult.OK)
                {
                    string updateRecuritName = scf._SelectedName; //选择的需要同步更新的表单名字。

                    #region 取值，组成接口参数
                    //SaveSynchronize="表单名@一般护理记录单,危重护理记录单¤日期@评估时间X,日期¤时间@评估时间X,时间¤评分@得分X,病情观察及措施¤评分格式串@压疮高危评分：{0}分¤索引号@7¤签名@系统用户,签名";
                    if (_TableType && _TableInfor != null)
                    {
                        if (_TableInfor.CellByRowNo_ColName(0, "日期") != null)
                        {
                            return;
                        }

                        //表格的时候：必须还有“日期”列
                        //取当前页，最后一个的非空行的内容

                        bool isEmptyRow = false;
                        for (int rowIndex = _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1; rowIndex >= 0; rowIndex--)
                        {
                            isEmptyRow = IsEmptyRow(rowIndex);

                            //表格式的评估单中的评分列不能为空。
                            if (!isEmptyRow && !string.IsNullOrEmpty(_TableInfor.CellByRowNo_ColName(rowIndex, arrscore[0].Trim()).Text))
                            {
                                //根据这一行的内容，设置参数进行更新
                                date = _TableInfor.CellByRowNo_ColName(rowIndex, arrdate[0].Trim()).Text.Trim();
                                if (date.Contains(" "))
                                {
                                    date = date.Split(' ')[0];
                                }

                                if (_TableInfor.CellByRowNo_ColName(rowIndex, arrtime[0].Trim()) == null || string.IsNullOrEmpty(arrtime[0]))
                                {
                                    //不含时间列的话，那么去系统时间。
                                    time = string.Format("{0:HH:mm}", GetServerDate());
                                }
                                else
                                {
                                    time = _TableInfor.CellByRowNo_ColName(rowIndex, arrtime[0].Trim()).Text.Trim();

                                    if (time.Contains(" "))
                                    {
                                        time = time.Split(' ')[1];
                                    }
                                }

                                score = _TableInfor.CellByRowNo_ColName(rowIndex, arrscore[0].Trim()).Text;

                                if (signFormatSetting.Contains("{0}"))
                                {
                                    //扩展多参数的时候
                                    if (!string.IsNullOrEmpty(exFormatPara))
                                    {
                                        string[] arrPara = new string[arrEx.Length + 1];
                                        arrPara[0] = score;

                                        for (int i = 0; i < arrEx.Length; i++)
                                        {
                                            arrExEach = arrEx[i].Split('=');

                                            if (_TableInfor.CellByRowNo_ColName(rowIndex, arrExEach[1].Trim()) != null)
                                            {
                                                arrPara[i + 1] = _TableInfor.CellByRowNo_ColName(rowIndex, arrExEach[1].Trim()).Text;
                                            }
                                            else
                                            {
                                                arrPara[i + 1] = "";
                                            }
                                        }

                                        score = string.Format(signFormatSetting, arrPara);

                                        arrPara = null;
                                    }
                                    else
                                    {
                                        score = string.Format(signFormatSetting, score);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        //非表格的时候

                        //日期和评分，要么同时有X，要么同时没X。
                        if (arrdate[0].Trim().Contains("X"))
                        {
                            string clName = "";
                            for (int index = maxIndex; index >= 0; index--)
                            {
                                clName = arrdate[0].Trim().Replace("X", index.ToString());

                                ct = this.pictureBoxBackGround.Controls.Find(clName, false);

                                if (ct != null && ct.Length > 0)
                                {
                                    //日期
                                    date = ct[0].Text.Trim();

                                    if (date.Contains(" "))
                                    {
                                        date = date.Split(' ')[0];
                                    }

                                    if (!string.IsNullOrEmpty(date)) //日期不为空时，取这个索引号的
                                    {
                                        //时间
                                        clName = arrtime[0].Trim().Replace("X", index.ToString());
                                        ct = this.pictureBoxBackGround.Controls.Find(clName, false);
                                        if (ct != null && ct.Length > 0)
                                        {
                                            time = ct[0].Text.Trim();

                                            if (time.Contains(" "))
                                            {
                                                time = time.Split(' ')[1];
                                            }

                                            if (string.IsNullOrEmpty(time))
                                            {
                                                time = string.Format("{0:HH:mm}", GetServerDate());
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(time))
                                            {
                                                time = string.Format("{0:HH:mm}", GetServerDate());
                                            }
                                        }

                                        //评分
                                        clName = arrscore[0].Trim().Replace("X", index.ToString());
                                        ct = this.pictureBoxBackGround.Controls.Find(clName, false);
                                        if (ct != null && ct.Length > 0)
                                        {
                                            score = ct[0].Text.Trim();

                                            if (signFormatSetting.Contains("{0}"))
                                            {
                                                //score = string.Format(signFormatSetting, score);

                                                //扩展多参数的时候
                                                if (!string.IsNullOrEmpty(exFormatPara))
                                                {
                                                    string[] arrPara = new string[arrEx.Length + 1];
                                                    arrPara[0] = score;

                                                    for (int i = 0; i < arrEx.Length; i++)
                                                    {
                                                        arrExEach = arrEx[i].Split('=');

                                                        clName = arrExEach[1].Trim().Replace("X", index.ToString());
                                                        ct = this.pictureBoxBackGround.Controls.Find(clName, false);
                                                        if (ct != null && ct.Length > 0)
                                                        {
                                                            arrPara[i + 1] = ct[0].Text.Trim();
                                                        }
                                                        else
                                                        {
                                                            arrPara[i + 1] = "";
                                                        }
                                                    }

                                                    score = string.Format(signFormatSetting, arrPara);

                                                    arrPara = null;
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        score = string.Format(signFormatSetting, score); //如果配置了{1}，但是扩展参数中没有定义就会异常的
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        string errmsg = "格式串中参数个数和扩展格式串参数中定义的参数不一致！";
                                                        Comm.LogHelp.WriteErr(errmsg);
                                                        MessageBox.Show(errmsg);
                                                        throw ex;
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //日期
                            ct = this.pictureBoxBackGround.Controls.Find(arrdate[0].Trim(), false);

                            if (ct != null && ct.Length > 0)
                            {
                                date = ct[0].Text.Trim();

                                if (date.Contains(" "))
                                {
                                    date = date.Split(' ')[0];
                                }
                            }

                            //时间
                            ct = this.pictureBoxBackGround.Controls.Find(arrtime[0].Trim(), false);

                            if (ct != null && ct.Length > 0)
                            {
                                time = ct[0].Text.Trim();

                                if (time.Contains(" "))
                                {
                                    time = time.Split(' ')[1];
                                }

                                if (string.IsNullOrEmpty(time))
                                {
                                    time = string.Format("{0:HH:mm}", GetServerDate());
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(time))
                                {
                                    time = string.Format("{0:HH:mm}", GetServerDate());
                                }
                            }

                            //评分
                            ct = this.pictureBoxBackGround.Controls.Find(arrscore[0].Trim(), false);

                            if (ct != null && ct.Length > 0)
                            {
                                score = ct[0].Text.Trim();

                                if (signFormatSetting.Contains("{0}"))
                                {
                                    //score = string.Format(signFormatSetting, score);

                                    //扩展多参数的时候
                                    if (!string.IsNullOrEmpty(exFormatPara))
                                    {
                                        string[] arrPara = new string[arrEx.Length + 1];
                                        arrPara[0] = score;

                                        for (int i = 0; i < arrEx.Length; i++)
                                        {
                                            arrExEach = arrEx[i].Split('=');
                                            ct = this.pictureBoxBackGround.Controls.Find(arrExEach[1].Trim(), false);

                                            if (ct != null && ct.Length > 0)
                                            {
                                                arrPara[i + 1] = ct[0].Text.Trim();
                                            }
                                            else
                                            {
                                                arrPara[i + 1] = "";
                                            }
                                        }

                                        score = string.Format(signFormatSetting, arrPara);

                                        arrPara = null;
                                    }
                                    else
                                    {
                                        score = string.Format(signFormatSetting, score);
                                    }
                                }
                            }
                        }

                        //string cc = string.Format("{0}-{1}-{2}", new string[] {"aa", "b", "c", "d"});
                        // string bb = string.Format("{0}-{1}-{2}", "aa", "b", "c", "d"); //参数只能多，不能少
                    }
                    #endregion 取值，组成接口参数


                    //调用批量接口，保存到表单
                    if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(score))
                    {
                        //获取模板，得到模板的权限
                        XmlDocument xmlDocTemplate = GetTemplateXml(updateRecuritName);
                        string templateRight = xmlDocTemplate.DocumentElement.GetAttribute(nameof(EntXmlModel.TemplateRule));
                        StringBuilder recordValues = new StringBuilder();
                        recordValues.Append(arrdate[1].Trim() + "@" + date);
                        recordValues.Append("¤");
                        recordValues.Append(arrtime[1].Trim() + "@" + time);
                        recordValues.Append("¤");
                        recordValues.Append(arrscore[1].Trim() + "@" + score);
                        if (!string.IsNullOrEmpty(signSetting))
                        {
                            //需要签名
                            sign = GlobalVariable.LoginUserInfo.UserName;

                            recordValues.Append("¤");
                            recordValues.Append(arrsign[1].Trim() + "@" + sign);
                        }


                        string[][] myArray = new string[1][];
                        //string[] arrayEach = new string[9];
                        //arrayEach[0] = GetDateValidated_YearFromUhid(this._patientInfo.Id);
                        //arrayEach[1] = this._patientInfo.Id;
                        //arrayEach[2] = updateRecuritName;
                        //arrayEach[3] = templateRight;
                        //arrayEach[4] = ""; //每页的行数，现在不用传值了。
                        //arrayEach[5] = recordValues.ToString();
                        //arrayEach[6] = "";
                        //arrayEach[7] = GlobalVariable.LoginUserInfo.UserCode + "¤" + GlobalVariable.LoginUserInfo.UserName + "¤" + GlobalVariable.LoginUserInfo.DeptName + "¤" + GlobalVariable.LoginUserInfo.WardName + "¤" + GlobalVariable.LoginUserInfo.TitleName;
                        //arrayEach[8] = this._patientInfo.Id + "¤" + patientsListView.GetPatientsInforFromName(_PatientsInforDT, this._patientInfo.Id, "姓名");

                        string[] arrayEach = new string[6];
                        arrayEach[0] = this._patientInfo.PatientId;
                        arrayEach[1] = updateRecuritName;
                        arrayEach[2] = recordValues.ToString(); //表格行的数据，如：日期@2016-06-21¤时间@13:00¤体温@38¤脉搏@122
                        arrayEach[3] = ""; //非表格的页数据：页数据内容，如果没有就传入Null或者空。如果传入，那么久必须指定页数：页数@n 。比如：页数@1¤签名@王医生¤内容@数值
                        arrayEach[4] = GlobalVariable.LoginUserInfo.UserCode + "¤" + GlobalVariable.LoginUserInfo.UserName + "¤" + GlobalVariable.LoginUserInfo.DeptName + "¤" + GlobalVariable.LoginUserInfo.WardName + "¤" + GlobalVariable.LoginUserInfo.TitleName;
                        arrayEach[5] = this._patientInfo.PatientId + "¤" + patientsListView.GetPatientsInforFromName(_PatientsInforDT, this._patientInfo.PatientId, "姓名") + "¤" + this._patientInfo.PatientId;

                        //批量接口，只提交一个病人的数据
                        myArray[0] = arrayEach;

                        //调用批量接口，进行同步更新保存。如果失败，提示信息，如果成功不提示。
                        //bool ret = LibraryWj.GlobalMethods.SaveBatchRecruit(myArray);
                        string Errmsg = "";
                        bool ret = false;// GlobalVariable._serviceLava.SaveBatchRecruit(myArray, ref Errmsg);

                        if (!ret)
                        {
                            Comm.LogHelp.WriteErr(_TemplateName + ",同步更新表单：" + updateRecuritName + "，失败！\r\n" + Errmsg);

                            MessageBox.Show("同步更新：[" + updateRecuritName + "]失败！" + Errmsg, "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            Comm.LogHelp.WriteInformation(_TemplateName + ",同步更新表单：" + updateRecuritName + "\r\n" + recordValues.ToString());
                        }

                        recordValues = null;
                        myArray = null;
                    }
                    else
                    {

                        Comm.LogHelp.WriteErr(_TemplateName + ",同步更新表单：" + updateRecuritName + "，由于日期和评分为空，无需同步到护理计划单！");
                        MessageBox.Show("日期或评分为空，所以不需要更新：[" + updateRecuritName + "]，请确认数据填写情况。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }


                }

                date = null;
                time = null;
                score = null;
                sign = null;

                //配置的控件名
                //评分
                scoreSetting = null;
                arrscore = null;

                //评分格式串
                signFormatSetting = null;
                //string[] arrsign = scoreSetting.Split(',');

                //日期
                dateSetting = null;
                arrdate = null;

                //时间
                timeSetting = null;
                arrtime = null;

                //签名：如果为空或者null，那就不要签名了。
                signSetting = null;
                arrsign = null;

                //索引号
                indexSetting = null;

                exFormatPara = null;
                arrEx = null;//{1}=导管分类
                arrExEach = null;
            }
            else
            {
                Comm.LogHelp.WriteErr("服务端：生命体征共享节点没有定义病区：" + GlobalVariable.LoginUserInfo.WardName + "的SaveSynchronize属性，指定护理记录单。");
            }
        }

        /// <summary>
        /// 获取所在病区的同步表单的配置
        /// 主要是icu病区和病房的的危重病人，可能阶段性使用危重记录单，病情好转后又会使用一般护理记录单
        /// </summary>
        /// <param name="xnSynchronizeRecruit"></param>
        /// LibraryWj.GlobalVariable.DataServiceNode.SelectSingleNode("//生命体征共享")
        /// <param name="userInpatientArea"></param>
        /// <returns></returns>
        private string GetSaveSynchronize(XmlNode xnSynchronizeRecruit, string userInpatientArea)
        {
            //<!-- 每个病区需要可以单独配置，如果不配置则取默认的，因为ICU病区和ICU病房可能使用危重记录单，或者两种同时使用 -->
            //<生命体征共享 SynchronizeRecruit="护理记录单¤体温,脉搏,呼吸,血压¤只读" Notes="本节点的定义的默认的配置">
            // <病区 Name="ICU病区" SynchronizeRecruit="护理记录单一危重¤体温,脉搏,呼吸,血压¤只读" Notes="ICU病区特定的配置,一般都是危重记录单"/>
            // <病区 Name="内分泌一科病区护理组" SynchronizeRecruit="护理记录单一,护理记录单一危重¤体温,脉搏,呼吸,血压¤只读" Notes="病区内有ICU病房，一般记录单和危重记录单都会使用的"/>
            //</生命体征共享>
            string ret = "";

            if (xnSynchronizeRecruit != null)
            {
                XmlNode xn = xnSynchronizeRecruit.SelectSingleNode("病区[@Name='" + userInpatientArea + "']");

                if (xn != null)
                {
                    //ret = (xn as XmlElement).GetAttribute("SynchronizeRecruit");
                    ret = (xn as XmlElement).GetAttribute(nameof(EntXmlModel.SaveSynchronize));
                }
                else
                {
                    //ret = (xnSynchronizeRecruit as XmlElement).GetAttribute("SynchronizeRecruit");
                    ret = (xnSynchronizeRecruit as XmlElement).GetAttribute(nameof(EntXmlModel.SaveSynchronize));
                }
            }

            return ret;
        }

        #endregion 同步更新其他表单，评估单的得分更新到护理记录单中

        # region 线程异步在本地备份表单xml
        private void ThreadAddBack()
        {
            try
            {
                //备份表单：
                BackUpHelp.SaveBack(_RecruitXML, this._patientInfo.PatientName, this._patientInfo.PatientId, _TemplateName);
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }
        # endregion 线程异步在本地备份表单xml

        # region 保存本页表单的数据，为提交数据库做好准备
        /// <summary>
        /// 保存本页表单的数据，到xml中，为提交数据库做好准备
        /// </summary>
        private void SaveThisPageDataToXml()
        {
            //如果是新建的表单，那么对象xml还没有创建，所以需要将null实例化为标准格式的xml文件
            if (_RecruitXML == null)
            {
                //创建表单数据的xml文件
                TemplateHelp mx = new TemplateHelp();
                _RecruitXML = mx.CreateTemplate(this._patientInfo.PatientId, this._patientInfo.PatientName, GlobalVariable.LoginUserInfo.UserCode, GlobalVariable.LoginUserInfo.DeptName);
            }

            XmlNode pages = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms)));

            //"NurseForm/Forms/Form[@SN='" + _CurrentPage + "']"
            XmlNode thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), _CurrentPage, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));

            //判断xml中是否已经存在本页的Page数据
            if (thisPage == null)
            {
                //<?xml version="1.0" encoding="utf-8"?>
                //<NurseForm>
                //    <Pages>
                //     <Page SN="1"/>
                //    </Forms>
                //    <Records />
                //</NurseForm>

                //插入新的节点
                XmlElement newPage;
                newPage = _RecruitXML.CreateElement(nameof(EntXmlModel.Form));

                newPage.SetAttribute(nameof(EntXmlModel.SN), _CurrentPage);

                pages.AppendChild(newPage);
                thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), _CurrentPage, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));
            }
            else
            {
                //更新
            }
            //如果是Table的话，插入行或者删除行，可能会导致多页或者少页，判断实际页和_RecruitXML的Forms.Form是否一致，如果不一致的话，新增或删除
            if (_TableType && _TableInfor != null)
            {
                //如果有多页的情况，生成多页的Form，否则下次打开会丢失页
                int maxPage = 0;
                if (!int.TryParse(this.uc_Pager1.lblPageCount.Text, out maxPage))
                {
                    maxPage = this.uc_Pager1.PageCurrent;
                }
                if (maxPage > this.uc_Pager1.PageCurrent)
                {
                    Comm.LogHelp.WriteDebug($"在SaveThisPageDataToXml方法中，自动创建的页数：{maxPage} 堆栈信息：{Comm.GetStackTrace()}");

                    for (int i = 1; i <= maxPage - this.uc_Pager1.PageCurrent; i++)
                    {
                        //这里算法有问题，每次切换页，会生成很多很多
                        var node2 = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), (this.uc_Pager1.PageCurrent + i).ToString(), "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));
                        if (node2 == null)
                        {
                            //插入新的节点
                            XmlElement newPage;
                            newPage = _RecruitXML.CreateElement(nameof(EntXmlModel.Form));

                            newPage.SetAttribute(nameof(EntXmlModel.SN), (this.uc_Pager1.PageCurrent + i).ToString());

                            pages.AppendChild(newPage);
                        }
                    }
                }
            }


            DateTime serverDate;
            serverDate = GetServerDate();

            //更新时间
            (thisPage as XmlElement).SetAttribute(nameof(EntXmlModel.VALIDATE), string.Format("{0:yyyy-MM-dd HH:mm}", serverDate));

            //更新用户
            if ((thisPage as XmlElement).GetAttribute(nameof(EntXmlModel.UserName)) == "")
            {
                (thisPage as XmlElement).SetAttribute(nameof(EntXmlModel.UserName), GlobalVariable.LoginUserInfo.UserCode);
            }

            //遍历pictureBoxBackGround画布上所有控件，进行保存
            if (this.pictureBoxBackGround.Controls.Count > 0)
            {

                string ctrlName = "";
                try
                {
                    #region foreach control
                    //界面上删除所有的以前无效的辅助线图标mark
                    foreach (Control control in this.pictureBoxBackGround.Controls)
                    {
                        ctrlName = control.Name;
                        if (control.Name == "lblShowSearch")
                        {
                            //是指示用的：搜索图标图片，跳过   
                            continue;
                        }

                        //开始保存
                        if (control is RichTextBoxExtended)
                        {
                            if (!((RichTextBoxExtended)control)._IsTable)
                            {
                                (thisPage as XmlElement).SetAttribute(((RichTextBoxExtended)control).Name, GetRtbeSaveValue((RichTextBoxExtended)control));
                            }
                        }
                        else if (control is CheckBoxExtended)
                        {
                            //(thisPage as XmlElement).SetAttribute(((CheckBoxExtended)control).Name, ((CheckBoxExtended)control).Checked.ToString());
                            (thisPage as XmlElement).SetAttribute(((CheckBoxExtended)control).Name, GetChkSaveValue(((CheckBoxExtended)control)));
                        }
                        else if (control is ComboBoxExtended)
                        {
                            //(thisPage as XmlElement).SetAttribute(((ComboBoxExtended)control).Name, ((ComboBoxExtended)control).Text);
                            (thisPage as XmlElement).SetAttribute(((ComboBoxExtended)control).Name, GetCombSaveValue(((ComboBoxExtended)control)));
                        }
                        else if (control is Vector)
                        {
                            (thisPage as XmlElement).SetAttribute(((Vector)control).Name, ((Vector)control).GetSaveData()); //获取矢量图信息，保存矢量图  
                        }
                        else if (control is GeneticMapControl)
                        {
                            (thisPage as XmlElement).SetAttribute(((GeneticMapControl)control).Name, ((GeneticMapControl)control).GetSaveData()); //获取矢量图信息，保存矢量图  遗传图谱
                        }
                        else if (control is PictureBox)
                        {
                            //判断是否为添加的图片，如果不是固定显示的log图片，那么需要保存到本页数据中
                            if (((PictureBox)control).Tag != null)  //null是logo图片  && ((PictureBox)control).Tag.ToString().EndsWith(".jpg")
                            {
                                if (control is VectorControl)
                                {
                                    //保存矢量图：底图和图层信息，中间用§进行分割。
                                    (thisPage as XmlElement).SetAttribute(control.Name, ((VectorControl)control).GetSaveStr());   //矢量图
                                }
                                else
                                {
                                    //保存添加的静态图片：
                                    string line = "";

                                    if (((PictureBox)control).BorderStyle == BorderStyle.None)
                                    {
                                        line = "";
                                    }
                                    else
                                    {
                                        line = "Line";
                                    }

                                    string size = control.Size.Width.ToString() + "," + control.Size.Height.ToString();

                                    if (!((PictureBox)control).Tag.ToString().Contains("¤"))
                                    {
                                        //新添加到本页的图片
                                        Image img = ((PictureBox)control).BackgroundImage;
                                        //Bitmap bmp = new Bitmap(((PictureBox)control).Tag.ToString());

                                        #region 图片压缩
                                        //-----------------图片压缩成图片控件当前的大小比例进行压缩-------------------
                                        //Bitmap bmpNew = new Bitmap(control.Size.Width, control.Size.Height);
                                        //Graphics grap = Graphics.FromImage(bmpNew);
                                        //grap.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                                        //grap.DrawImage(img, new Rectangle(0, 0, control.Size.Width, control.Size.Height));

                                        //Image bmpNew = GetReducedImage(img, control.Size.Width, control.Size.Height);
                                        //-----------------图片压缩成图片控件当前的大小比例进行压缩-------------------
                                        #endregion 图片压缩

                                        //名字要作为属性名来保存的，如果uhid含有非法字符就会报错
                                        //string name = nameof(EntXmlModel.IMAGE) + Path.GetFileNameWithoutExtension(((PictureBox)control).Tag.ToString()).Replace("~", "").Replace(" ", "");
                                        //string name = nameof(EntXmlModel.IMAGE) + Path.GetFileNameWithoutExtension(((PictureBox)control).Tag.ToString()).Split('~')[1].Replace(" ", "");

                                        string name = ((PictureBox)control).Tag.ToString();

                                        string value = name + "¤" + ((PictureBox)control).Location.X.ToString() + "," + ((PictureBox)control).Location.Y.ToString() + "¤"
                                            + size + "¤" + line + "¤"
                                            + FileHelp.ConvertImageToString(img) + "¤" + GlobalVariable.LoginUserInfo.UserCode;

                                        //以标识性开头表示是图片，下次显示、删除操作都要根据这个来判断
                                        (thisPage as XmlElement).SetAttribute(name, value);

                                        ((PictureBox)control).Tag = value;
                                    }
                                    else
                                    {
                                        //打开的本页之前添加的图片:名字，位置，大小，是否边框线，图片二进制数据
                                        string[] arr = ((PictureBox)control).Tag.ToString().Split('¤');

                                        string creatUser = "";
                                        if (arr.Length > 5)
                                        {
                                            creatUser = arr[5];
                                        }

                                        (thisPage as XmlElement).SetAttribute(arr[0],
                                            arr[0] + "¤" + ((PictureBox)control).Location.X.ToString() + "," + ((PictureBox)control).Location.Y.ToString() + "¤" + size + "¤" + line + "¤" + arr[4] + "¤" + creatUser);
                                    }
                                }
                            }
                        }
                        else if (control is LabelExtended)
                        {
                            if (control.Name == "_PageNo")
                            {
                                if (control.Tag != null && control.Tag.ToString().Contains("¤"))
                                {
                                    try
                                    {
                                        string pageEdit = ((LabelExtended)control).Text.Trim();
                                        //如果手动修改页数，那么需要根据这个来得到第一页应该显示的页数，以后所有页根据这个累加差值
                                        if (control.Tag.ToString().Contains("¤")) //手动修改的标签的Tag中会加入小太阳符号 
                                        {
                                            #region 老的处理方法 所有页都会跟着变。现在只要当前页和之后页跟着变 
                                            //int a = int.Parse(pageEdit);
                                            //int b = int.Parse(_CurrentPage);

                                            //XmlNode root = _RecruitXML.SelectSingleNode(nameof(EntXmlModel.NurseForm));
                                            //(root as XmlElement).SetAttribute(nameof(EntXmlModel.PageNoStart), (a - b).ToString()); //默认起始页号为空，代表0
                                            #endregion 老的处理方法 所有页都会跟着变。现在只要当前页和之后页跟着变

                                            if (!string.IsNullOrEmpty(_PageNoFormat))
                                            {
                                                if (PageNo.IsCurrentPageLeft(_PageNoFormat))
                                                {
                                                    pageEdit = ((int)StringNumber.getFirstNum(pageEdit)).ToString();
                                                }
                                                else
                                                {
                                                    pageEdit = ((int)StringNumber.getLastNum(pageEdit)).ToString();
                                                }
                                            }

                                            //将手动修改的页码，保存到表单数据xml的根节点属性：PageNoStart中。
                                            PageNo.SaveValue(_RecruitXML, _CurrentPage, pageEdit);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ShowInforMsg("手动修改页数，保存出错：" + ex.StackTrace, true);
                                    }
                                }

                                continue; //页码修改，不要保存界面上的页码值，而是保存一个变量信息。下次打开再重新计算所有页。
                            }

                            //Label的Tag中存放了坐标¤
                            //需要判断是否为手动修改的，并记录下来，下次打开的时候，如果是手动修改的，那么不要自动化新；以手动修改的为准
                            if (control.Tag != null && control.Tag.ToString().Contains("¤"))
                            {
                                (thisPage as XmlElement).SetAttribute(((LabelExtended)control).Name, ((LabelExtended)control).Text.Trim() + "¤");
                            }
                            else
                            {
                                (thisPage as XmlElement).SetAttribute(((LabelExtended)control).Name, ((LabelExtended)control).Text.Trim());
                            }
                        }
                        else if (control is MonthCalendar)
                        {
                            //日历控件跳过
                        }
                        //else
                        //{
                        //    //没有指定的控件
                        //    ShowInforMsg("在控件进行保存的时候，遇到无法识别的类型：" + control.ToString()); //Label：System.Windows.Forms.Label, Text: 调试科别名字
                        //}
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Comm.LogHelp.WriteErr("保存表单循环控件错误了，控件的名称位：{0}，错误信息：{1}", ctrlName, ex);
                    throw ex;
                }

            }

            //保存当前页目前的样式ID _node_Page ;方便以后扩展，比如以后更新新的样式修改，老数据还要不变
            (thisPage as XmlElement).SetAttribute(nameof(EntXmlModel.TemplateStyleID), (_node_Page as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

            //保存表格数据
            if (_TableType && _TableInfor != null)
            {
                XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                //先更新最大唯一号
                //
                int maxId = GetTableRows();
                (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), _TableInfor.MaxId.ToString());
            }

        }
        # endregion 保存本页表单的数据，为提交数据库做好准备

        # region 保存前，移除初始化添加的空行
        /// <summary>
        /// 删除空白行
        /// 如果不指定参数，那么就是全局变量的数据文件
        /// </summary>
        private void RemoveEmptyRows()
        {
            RemoveEmptyRows(_RecruitXML);
        }

        /// <summary>
        /// 删除空白行（末尾的）
        /// 将指定的xml数据文件末尾的空白行删除掉。（原来的空白行，是补充添加填满界面所有行的，或者添加后产生的无效数据）
        /// </summary>
        /// <param name="xmlDataPara"></param>
        private void RemoveEmptyRows(XmlDocument xmlDataPara)
        {
            XmlNode recordsNode = xmlDataPara.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            if (recordsNode != null && recordsNode.ChildNodes.Count > 0)
            {
                XmlNode xn = null;
                int maxId = 0;
                for (int i = recordsNode.ChildNodes.Count - 1; i >= 0; i--)
                {
                    xn = recordsNode.ChildNodes[i];

                    if (Comm.isEmptyRecord(xn))
                    {
                        //在修改表单同步更新到体温单的时候，删除行在添加行，可能id已经不是同一行了。不减少id号，可能反而稳定
                        //判断ID,再删除
                        if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.ID)) == (recordsNode as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID)) && xn.Equals(recordsNode.LastChild))
                        {
                            //更新最大的编号唯一号
                            maxId = int.Parse((recordsNode as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID)));
                            maxId--;
                            (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), maxId.ToString());

                            //_TableInfor.MaxId = maxId; //这里不用更新表格对象的最大ID，因为下面还要还原的
                        }

                        //删除该节点
                        recordsNode.RemoveChild(xn);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 删除所有空白行
        /// 保存排序时，清除空白行。
        /// </summary>
        /// <param name="xmlDataPara"></param>
        private void RemoveEmptyRowsAll(XmlDocument xmlDataPara)
        {
            XmlNode recordsNode = xmlDataPara.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            if (recordsNode != null && recordsNode.ChildNodes.Count > 0)
            {
                XmlNode xn = null;
                //int maxId = 0;
                for (int i = recordsNode.ChildNodes.Count - 1; i >= 0; i--)
                {
                    xn = recordsNode.ChildNodes[i];

                    if (Comm.isEmptyRecord(xn))
                    {
                        ////判断ID,再删除
                        //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.ID)) == (recordsNode as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID)))
                        //{
                        //    //更新最大的编号唯一号
                        //    maxId = int.Parse((recordsNode as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID)));
                        //    maxId--;
                        //    (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), maxId.ToString());

                        //    //_TableInfor.MaxId = maxId; //这里不用更新表格对象的最大ID，因为下面还要还原的
                        //}

                        ////删除该节点
                        //recordsNode.RemoveChild(xn);

                        RemoveNodeRecord(recordsNode, xn);
                    }
                    else
                    {
                        //break;

                        //如果只有日期和时间；并且日期和时间和上一行一样，也认为是空行
                        //eachDateTime = GetRowDateTime((cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));
                        //NotSameDateTimeToPreRow(
                        if (Comm.isEmptyRecordParagraph(xn) && !NotSameDateTimeToPreRow((xn as XmlElement).GetAttribute(nameof(EntXmlModel.ID))))
                        {
                            RemoveNodeRecord(recordsNode, xn);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 删除指定的数据节点
        /// </summary>
        /// <param name="recordsNode"></param>
        /// <param name="thisNode"></param>
        private void RemoveNodeRecord(XmlNode recordsNode, XmlNode xn)
        {
            //在修改表单同步更新到体温单的时候，删除行在添加行，可能id已经不是同一行了。不减少id号，可能反而稳定
            //判断ID,再删除
            if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.ID)) == (recordsNode as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID)) && xn.Equals(recordsNode.LastChild))
            {
                int maxId = 0;
                //更新最大的编号唯一号
                maxId = int.Parse((recordsNode as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID)));
                maxId--;
                (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), maxId.ToString());

                //_TableInfor.MaxId = maxId; //这里不用更新表格对象的最大ID，因为下面还要还原的
            }

            //删除该节点
            recordsNode.RemoveChild(xn);
        }
        # endregion 保存前，移除初始化添加的空行

        # region 获取非表格中输入框的保存的文本
        /// <summary>
        /// 返回该输入框需要保存的文本
        /// Text 0¤单红线 1¤双红线 2¤权限 3¤Rtf 4¤修订历史 5¤日期格式 6
        /// </summary>
        /// <returns></returns>
        private string GetRtbeSaveValue(RichTextBoxExtended rtbe)
        {
            string retValue = "";

            string CreateUser = rtbe.CreateUser;
            if (string.IsNullOrEmpty(CreateUser))
            {
                //如果输入框为空，那么不要设置【创建用户】
                if (string.IsNullOrEmpty(rtbe.Text))
                {
                    CreateUser = ""; //不设置
                }
                else
                {
                    CreateUser = GlobalVariable.LoginUserInfo.UserCode; //权限创建用户
                }
            }

            //删除清空手摸签名图片的时候，不要记录CreateUser，否则下次打开还会记录
            //可以利用判断rtf是否有图片来判断，可能更好：没有图片就清空: rtbe.Rtf.IndexOf(@"{\pict\") > -1  && !rtbe.IsContainImg()
            if (string.IsNullOrEmpty(rtbe.Text)
                && (_FingerPrintSign && (rtbe.Name.StartsWith("签名") || rtbe.Name.StartsWith("记录者")) && !string.IsNullOrEmpty(rtbe.CreateUser))
                && !rtbe.IsContainImg()) //如果这里处理的话，那么会导致光标离开的时候，赋值后OldRtf和现在的一样
            {
                rtbe._IsRtf = false;
                rtbe.toolStripMenuItemClearFormat_Click(null, null);
                rtbe.CreateUser = "";
                CreateUser = ""; //不设置
            }

            string myText = rtbe.Text;

            if (rtbe.Name.StartsWith("日期") && rtbe._FullDateText != "")
            {
                myText = rtbe._FullDateText;
            }


            if (rtbe._IsRtf)
            {
                //是高级富文本的时候，需要
                //Rtf，Text，单红线，双红线，权限（创建用户），
                //如果是表格，还有：单元格边框线信息，行线信息，合并单元格信息，调整段落列信息

                //排序后：Text 0¤单红线 1¤双红线 2¤权限 3¤Rtf 4

                retValue = myText + "¤" + rtbe._HaveSingleLine.ToString() + "¤" + rtbe._HaveDoubleLine.ToString() + "¤" + CreateUser + "¤" + rtbe.Rtf + "¤" + rtbe._EditHistory;

                //手摸签名的图片rtf不要每次保存
                if (_FingerPrintSign && string.IsNullOrEmpty(rtbe.Text.Trim())) //没有在图片签名后，继续输入文字的话。如果添加文字的，那么就是不通用的Rtf了，不保存
                {
                    retValue = myText + "¤" + rtbe._HaveSingleLine.ToString() + "¤" + rtbe._HaveDoubleLine.ToString() + "¤" + CreateUser + "¤¤" + rtbe._EditHistory;
                }
            }
            else
            {
                retValue = myText + "¤" + rtbe._HaveSingleLine.ToString() + "¤" + rtbe._HaveDoubleLine.ToString() + "¤" + CreateUser + "¤" + "" + "¤" + rtbe._EditHistory;
            }

            if (rtbe.Name.StartsWith("日期"))
            {
                retValue += "¤" + rtbe._YMD;
            }

            return retValue;
        }
        # endregion 获取非表格中输入框的保存的文本

        # region 获取非表格的非输入框，如勾选框等的保存文本
        private string GetChkSaveValue(CheckBoxExtended chk)
        {
            string retValue = "";

            string CreateUser = chk.CreateUser;
            if (string.IsNullOrEmpty(CreateUser))
            {
                //如果选择框还是为默认值
                if (chk.Default == chk.Checked)
                {
                    CreateUser = ""; //不设置
                }
                else
                {
                    CreateUser = GlobalVariable.LoginUserInfo.UserCode; //权限创建用户
                }
            }

            //拼接：是否选中 + 用户id + 三种状态中的哪一种，有可能是第三种状态，其实也是未选中
            //if (chk.ThreeState && chk.CheckState == CheckState.Indeterminate)
            //{
            //    //True¤Mandalat¤Indeterminate  || 其实是未选中状态
            //    //if(chk.CheckState
            //    retValue = "False¤" + CreateUser + "¤" + chk.CheckState.ToString();
            //}
            if (chk.ThreeState) // && chk.CheckState == CheckState.Indeterminate
            {
                //True¤Mandalat¤Indeterminate  || 其实是未选中状态 应该改成False
                if (chk.CheckState == CheckState.Indeterminate)
                {
                    //打叉的时候，Checked为True，但其实逻辑上是False的意思
                    retValue = "False¤" + CreateUser + "¤" + chk.CheckState.ToString();
                }
                else
                {
                    retValue = chk.Checked.ToString() + "¤" + CreateUser + "¤" + chk.CheckState.ToString();
                }
            }
            else
            {
                retValue = chk.Checked.ToString() + "¤" + CreateUser;
            }

            return retValue;
        }

        private string GetCombSaveValue(ComboBoxExtended cbe)
        {
            string retValue = "";

            string CreateUser = cbe.CreateUser;
            if (string.IsNullOrEmpty(CreateUser))
            {
                //如果选择框还是为默认值
                if (cbe.Default == cbe.Text)
                {
                    CreateUser = ""; //不设置
                }
                else
                {
                    CreateUser = GlobalVariable.LoginUserInfo.UserCode; //权限创建用户
                }
            }



            retValue = cbe.Text.ToString() + "¤" + CreateUser;

            return retValue;
        }
        # endregion 获取非表格的非输入框，如勾选框等的保存文本

        # region 全局的特殊字符，合并到当前输入框
        /// <summary>
        /// 将全局的输入框配置的可选择文字和每个输入框的合并
        /// </summary>
        /// <param name="thisRtbValue">每个输入框配置的选择文字的内容</param>
        private string GetSelectWord(string thisRtbValue)
        {
            string retValue = "";

            if (thisRtbValue != "")
            {
                if (_HelpString == "")
                {
                    retValue = thisRtbValue;
                }
                else
                {
                    //ToolStripSeparator toolStripSeparator1;  ︴
                    retValue = _HelpString + "︴" + thisRtbValue;
                }
            }
            else
            {
                retValue = _HelpString;
            }

            return retValue;
        }
        # endregion 全局的特殊字符，合并到当前输入框

        # region 初始化：输入框共通部分处理
        private void initRtbe(RichTextBoxExtended rtbe)
        {

            rtbe._DefaultSize = rtbe.Size;

            //rtbe.AlignmentHorizontalRB = System.Windows.Forms.HorizontalAlignment.Center;//设置成居中
            rtbe.AutoWordSelection = true;
            rtbe.BorderStyle = System.Windows.Forms.BorderStyle.None;
            //rtbe.Multiline = false;  //表格单元格多行处理 
            rtbe.WordWrap = false;
            rtbe.ImeMode = ImeMode.On;
            rtbe.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;

            //单元格自动换行:注册事件-自动调整行高，并使下面的控件保持相同间距
            if (_IsSpaceChart)
            {
                rtbe.ContentsResized += new System.Windows.Forms.ContentsResizedEventHandler(this.RTBE_ContentsResized);
            }

            //知识库
            rtbe.SetKnowledg(_node_Knowledg, null);

            rtbe.ShowInforMsg_MainForm += new EventHandler(rtbe_ShowInforMsg_MainForm);

            //事件处理：
            rtbe.MouseLeave += new EventHandler(this.ClearToolTip_MouseLeave); ;                    //隐藏气泡提示

            if (!rtbe._IsTable) //表格的排除，不支持脚本
            {
                rtbe.TextChanged += new System.EventHandler(this.Script_TextChanged);               //执行脚本

                rtbe.Enter += new EventHandler(InputBox_EnterSetDefaulyValue);                      //非表格，赋默认值
            }
            else
            {
                //将单元格输入框隐藏，可以选择区域
                rtbe.MouseMove += new MouseEventHandler(this.Cells_MouseMoveHideForCopySelect);
                rtbe.MouseDown += new MouseEventHandler(this.Cells_MouseDown);
                rtbe.MouseUp += new MouseEventHandler(this.Cells_MouseUp);
            }

            //双击签名
            rtbe.DoubleClick += new System.EventHandler(this.Cells_DoubleClickSetDefaulyValue);

            rtbe.Enter += new EventHandler(SetAssistantControl_Enter);                          //光标进入后，设置为输入法对象
            rtbe.SelectionChanged += new EventHandler(UpdateToolBarFontStyle_SelectionChanged); //选择文字改变后，将选中文字的样式更新到工具栏

            //rtbe.Validating += new System.ComponentModel.CancelEventHandler(this.Rtbe_Validating);
            rtbe.Leave += new System.EventHandler(this.RtbeLeave_Validating);

            rtbe.MouseWheel += new MouseEventHandler(richTextBox_MouseWheel);  //滚轮事件：光标在单元格内，也可以移动滚动条

            //rtbe.KeyDown +=new KeyEventHandler(rtbe_KeyDown);                  //非表格输入框，按下回车和Tab键一样的效果
        }

        ///// <summary>
        ///// 非表格输入框，按下回车和Tab键一样的效果
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void rtbe_KeyDown(object sender, KeyEventArgs e)
        //{
        //    --
        //}

        /// <summary>
        /// 滚轮事件
        /// 光标在单元格内，也可以移动滚动条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void richTextBox_MouseWheel(object sender, MouseEventArgs e)
        {
            //MessageBox.Show("richTextBox_MouseWheel");
            if ((Control.ModifierKeys & Keys.Control) != Keys.Control) //多行输入框，按下Ctrl的时候+滚轮，是对输入框内容进行缩放
            {
                int newValue = this.panelMain.VerticalScroll.Value - e.Delta;

                if (e.Delta > 0)  //e.Delta 为120
                {
                    if (newValue >= this.panelMain.VerticalScroll.Minimum)
                    {
                        this.panelMain.VerticalScroll.Value = newValue;
                    }
                    else
                    {
                        this.panelMain.VerticalScroll.Value = this.panelMain.VerticalScroll.Minimum; //可能不能滚到置顶
                        this.panelMain.Refresh();   //可能不能滚到置顶
                        //this.panelMain.VerticalScroll.Value = 0;

                        //this.panelMain.VerticalScroll.Value = newValue; //报错
                    }
                }
                else
                {
                    if (newValue <= this.panelMain.VerticalScroll.Maximum)
                    {
                        this.panelMain.VerticalScroll.Value = newValue;
                    }
                    else
                    {
                        this.panelMain.VerticalScroll.Value = this.panelMain.VerticalScroll.Maximum;
                    }
                }

                if (!_IsLoading)
                {
                    //this.pictureBoxBackGround.Update(); //会来不及及时刷新
                    DrawRowSignShow(false);
                }
            }
        }

        /// <summary>
        /// 输入框控件，回调主窗体的显示提示消息事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rtbe_ShowInforMsg_MainForm(object sender, EventArgs e)
        {
            if (sender != null)
            {
                ShowInforMsg(sender.ToString());
            }
        }

        # endregion 初始化：输入框共通部分处理

        # region 基本信息，鼠标移上去显示内容
        //防止基本信息重叠后看不清，提示
        private ToolTip ttLabel = new ToolTip();
        private void lblBasicInfor_MouseEnter(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(((Label)sender).Text.Trim().Replace("　", "")))
            //{
            //    ////鼠标放上去，显示内容提示
            //    //ttLabel.ShowAlways = true;
            //    //ttLabel.Show(((Label)sender).Text, ((Label)sender));
            //}
            //else
            //{
            //鼠标放上去，显示内容提示
            ttLabel.ShowAlways = true;
            ttLabel.Show(((Label)sender).Text, ((Label)sender));
            //}
        }

        private void lblBasicInfor_MouseLeave(object sender, EventArgs e)
        {
            ttLabel.Hide(((Label)sender));
        }
        # endregion 基本信息，鼠标移上去显示内容

        # region 基本信息双击：手动修改内容
        /// <summary>
        /// 双击：手动修改内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblBasicInfor_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Label lb = (Label)sender;

            string para1 = lb.Name;
            string para2 = lb.Text;

            if (para1 == "_PageNo" && !string.IsNullOrEmpty(_PageNoFormat))
            {
                if (PageNo.IsCurrentPageLeft(_PageNoFormat))
                {
                    para2 = ((int)StringNumber.getFirstNum(para2)).ToString();
                }
                else
                {
                    para2 = ((int)StringNumber.getLastNum(para2)).ToString();
                }
            }

            //EditBaseInforForm editBIForm = new EditBaseInforForm(lb.Name, lb.Text, "");
            Frm_EditBaseInfo editBIForm = new Frm_EditBaseInfo(para1, para2, "");
            DialogResult res = editBIForm.ShowDialog();

            if (res == DialogResult.OK)
            {
                //根据修改后的值进行设置显示
                string newValue = editBIForm.NewText;

                if (newValue.Trim() == "")
                {
                    MessageBox.Show("基本信息不能为空！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (para1 == "_PageNo" && !string.IsNullOrEmpty(_PageNoFormat))
                {
                    //页号手动修改处理
                    XmlNode root = _RecruitXML.SelectSingleNode(nameof(EntXmlModel.NurseForm));
                    string start = (root as XmlElement).GetAttribute(nameof(EntXmlModel.PageNoStart)); //默认起始页号为空，代表0

                    //重新根据配置的页码格式串，设置页码内容
                    if (_CurrentPage == this._CurrentTemplateNameTreeNode.Nodes.Count.ToString())
                    {
                        //如果是最后一页，还要修改最大页数：_MaxPageForPrintAllShow，请显示
                        //即使不是最后一页，如果手动修改的页码，是目前最后的，那么也会影响最大页码的
                        _MaxPageForPrintAllShow = int.Parse(newValue);
                    }
                    else if (PageNo.IsMaxSetPage(start, _CurrentPage))
                    {
                        _MaxPageForPrintAll = this._CurrentTemplateNameTreeNode.Nodes.Count;
                        //_MaxPageForPrintAllShow = int.Parse(PageNo.GetValue(start, _MaxPageForPrintAll)); //不能还用老的
                        //将手动修改的页码，保存到表单数据xml的根节点属性：PageNoStart中。
                        PageNo.SaveValue(_RecruitXML, _CurrentPage, newValue);
                        root = _RecruitXML.SelectSingleNode(nameof(EntXmlModel.NurseForm));
                        start = (root as XmlElement).GetAttribute(nameof(EntXmlModel.PageNoStart)); //默认起始页号为空，代表0

                        _MaxPageForPrintAllShow = int.Parse(PageNo.GetValue(start, _MaxPageForPrintAll));

                    }
                    else
                    {
                        //newValue = String.Format(_PageNoFormat, newValue, _MaxPageForPrintAllShow.ToString());
                    }

                    newValue = String.Format(_PageNoFormat, newValue, _MaxPageForPrintAllShow.ToString());
                }

                if (newValue != lb.Text)
                {
                    IsNeedSave = true;

                    lb.Text = newValue;

                    if (lb.Tag.ToString().EndsWith("¤"))
                    {
                        //有的话就不用加了
                    }
                    else
                    {
                        lb.Tag = lb.Tag.ToString() + "¤";
                    }

                    //重新设置坐标
                    if (lb is LabelExtended)
                    {
                        ((LabelExtended)lb).Location = ((LabelExtended)lb)._Location; //恢复模板设置的坐标位置
                        lb.Location = GetVariableVerticalCenterLocation((LabelExtended)lb);
                    }
                }
            }
        }
        # endregion 基本信息双击：手动修改内容

        # region 点击面板和画布，选中滚动条的panel，便于鼠标拖动滚动条
        private void panelMain_Click(object sender, EventArgs e)
        {
            panelMain.Focus();

            SetOpenPageTreeNodeSelected();
        }
        # endregion 点击面板和画布，选中滚动条的panel，便于鼠标拖动滚动条

        # region 点击画布
        /// <summary>
        /// 点击画布
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxBackGround_Click(object sender, EventArgs e)
        {
            //panelMain.Focus();
            //_Need_Assistant_Control = null;
            if (lblShowSearch.Visible)
            {
                lblShowSearch.Hide();
                //this.pictureBoxBackGround.Refresh();
            }

            if (this.lblShowEditRowCell != null && !this.lblShowEditRowCell.IsDisposed && this.lblShowEditRowCell.Visible)
            {
                this.lblShowEditRowCell.Hide();
                DrawRowSignShow(true);
            }

            SetOpenPageTreeNodeSelected();

            //取消上次显示的突出单元格
            ClearRowSignShow();

            _GoOnPrintLocation = new Point(-1, -1);

        }
        # endregion 点击画布

        #region 画布拖动操作 复制内容 鼠标写字
        bool _MouseIsDown = false;
        Rectangle _MouseRect;
        Rectangle _WorkArea;
        //List<Control> SelectedCtrl = new List<Control>();

        //表格
        CellInfor[,] _ArrCell = null;

        void WorkArea_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (_PictureBoxSelectArea != null && !_PictureBoxSelectArea.IsDisposed)
                {
                    _PictureBoxSelectArea.Dispose();
                }


                //--------------------------------------鼠标写字-------------------
                _MouseDownDraw = false;
                //--------------------------------------鼠标写字-------------------

                if (!_MouseIsDown) //还没有按shift后拖拽，不要处理：只有按下shift，然后按下鼠标才会为True，如果抬起图标没有不为真，那么就不用刷新和加入粘帖板了
                {
                    return;
                }

                this.pictureBoxBackGround.Capture = false;
                Cursor.Clip = Rectangle.Empty;

                if (_MouseIsDown && !_IS_SHIFT_KEYDOWN) //if (MouseIsDown && !_IS_SHIFT_KEYDOWN) 如果按下了shift后，又释放了，这时MouseIsDown还为true
                {
                    _MouseIsDown = false;
                    this.pictureBoxBackGround.Refresh();
                    return;
                }

                _MouseIsDown = false;

                #region//--获取选择的控件，遍历
                //防止从右下，往左上拖动
                if (_MouseRect.Width < 0 || _MouseRect.Height < 0)
                {
                    if (_MouseRect.Width < 0)
                    {
                        _MouseRect = new Rectangle(_MouseRect.X + _MouseRect.Width, _MouseRect.Y, Math.Abs(_MouseRect.Width), _MouseRect.Height);
                    }

                    if (_MouseRect.Height < 0)
                    {
                        _MouseRect = new Rectangle(_MouseRect.X, _MouseRect.Y + _MouseRect.Height, _MouseRect.Width, Math.Abs(_MouseRect.Height));
                    }
                }

                Control interSectCtl = null;
                string text = "";

                //非表格输入框
                foreach (Control ctl in pictureBoxBackGround.Controls)
                {
                    if (ctl.Bounds.IntersectsWith(_MouseRect))
                    {
                        interSectCtl = ctl;
                        if (interSectCtl is RichTextBoxExtended)
                        {
                            RichTextBoxExtended picBox = (RichTextBoxExtended)ctl;
                            if (!picBox._IsTable)
                            {
                                text += picBox.Text;

                            }
                        }

                        continue;
                    }
                }

                //得到选中了多少行，多少列
                int row = -1;
                int col = -1;

                int row1 = -1;
                int col1 = -1;
                int row2 = -1;  //不能只记录最后一行，如果是分栏的情况下就出错了。所以要记录总行数
                int col2 = -1;
                int rowAll = 0;

                if (_TableType && _TableInfor != null)
                {
                    int fieldIndex = 0;
                    for (int field = 0; field < _TableInfor.GroupColumnNum; field++)
                    {
                        fieldIndex = _TableInfor.RowsCount * field;

                        for (int i = 0; i < _TableInfor.RowsCount; i++)
                        {
                            for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                            {
                                if (_TableInfor.Cells[i + fieldIndex, j].Rect.IntersectsWith(_MouseRect))
                                {
                                    //如果是分栏的，是很难控制。因为第一栏的开始列不一定为0，第二栏一定为0。这样就复杂的

                                    if (row1 < 0)
                                    {
                                        row1 = i + fieldIndex; //第一行，
                                    }

                                    if (i + fieldIndex > row2) //这样第二栏的一行，会认为多行了
                                    {
                                        rowAll++;
                                    }

                                    row2 = i + fieldIndex;  //最后一行 。分栏的表格就计算错了

                                    if (col1 < 0)
                                    {
                                        col1 = j; //第一列，
                                    }

                                    if (j > col2) // 防止分栏的，两侧的列不一样
                                    {
                                        col2 = j;  //最后一列
                                    }

                                }
                            }
                        }
                    }
                }

                row = rowAll;// row = row2 - row1;
                col = col2 - col1;

                if (_TableType && _TableInfor != null)
                {
                    if (_TableInfor.GroupColumnNum > 1 && row2 - row1 + 1 > rowAll && rowAll != 0)
                    {
                        ////如果是分栏的，是很难控制。因为第一栏的开始列不一定为0，第二栏一定为0
                        MessageBox.Show("分栏的表格在复制单元格时，请按照行顺序操作，不能直接跨栏。");
                        this.pictureBoxBackGround.Refresh();
                        return;
                    }
                }

                if (row >= 0 && col >= 0 && row1 >= 0 && col1 >= 0)
                {
                    //row2++;// row++;
                    col++;
                    //_ArrCell = new CellInfor[row, col];
                    _ArrCell = new CellInfor[rowAll, col];

                    //ShowInforMsg("复制表格：" + row.ToString() + "行" + col.ToString() + "列");
                }
                else
                {
                    _ArrCell = null;
                }

                if (_TableType && _TableInfor != null)
                {
                    int copyRow = -1;
                    int fieldIndex = 0;
                    CellInfor ciEach = null;
                    for (int field = 0; field < _TableInfor.GroupColumnNum; field++)
                    {
                        fieldIndex = _TableInfor.RowsCount * field;

                        for (int i = 0; i < _TableInfor.RowsCount; i++)
                        {
                            for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                            {
                                //if (_TableInfor.Cells[i + fieldIndex, j].Rect.Contains(e.X, e.Y) && !_TableInfor.Cells[i + fieldIndex, j].IsMergedNotShow)
                                if (_TableInfor.Cells[i + fieldIndex, j].Rect.IntersectsWith(_MouseRect))
                                {
                                    text += _TableInfor.Cells[i + fieldIndex, j].Text;

                                    if (_ArrCell != null)
                                    {
                                        ////不分栏的时候
                                        //_ArrCell[i + fieldIndex - row1,j - col1] = _TableInfor.Cells[i + fieldIndex, j];

                                        if (copyRow != i + fieldIndex - row1)
                                        {
                                            copyRow++;
                                        }

                                        //新建页的时候，会将表格cell重置，那么引用类型的cell也就空了。
                                        //_ArrCell[copyRow, j - col1] = _TableInfor.Cells[i + fieldIndex, j];
                                        if (_TableInfor.Cells[i + fieldIndex, j] != null)
                                        {
                                            ciEach = _TableInfor.Cells[i + fieldIndex, j];
                                            _ArrCell[copyRow, j - col1] = new CellInfor(ciEach.RowIndex, ciEach.ColIndex);
                                            CopyCell(_ArrCell[copyRow, j - col1], ciEach);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (text != "")
                {
                    if (_ArrCell != null)
                    {
                        //新建页的时候，会将表格cell重置，那么引用类型的cell也就空了。
                        //_ArrCell.CopyTo(_ArrCell, 0); //只支持一维数组。可以总类自带的copycell方法


                        Clipboard.SetDataObject(_ArrCell);

                        text = "复制表格：" + rowAll.ToString() + "行" + col.ToString() + "列\r\n" + text;
                    }
                    else
                    {
                        Clipboard.SetText(text);//TextDataFormat.UnicodeText
                    }

                    ShowInforMsg("Shift选择区的内容，复制到粘帖板：\r\n" + text);
                }

                //Graphics g = this.pictureBoxBackGround.CreateGraphics();
                //g.DrawRectangle(Pens.Black, MouseRect);
                //g.Dispose();

                if (_DragDrawRectMode == 0)
                {
                    this.pictureBoxBackGround.Refresh();
                }
                else if (_DragDrawRectMode == 1)
                {
                    this.pictureBoxBackGround.Refresh();
                    DrawLastRectangular(); //临时显示的
                }
                else if (_DragDrawRectMode == 2)
                {
                    DrawLastRectangular();
                    this.pictureBoxBackGround.Refresh();
                }
                else
                {
                    this.pictureBoxBackGround.Refresh();
                }

                ////防止上次的干扰，要在上次绘制的矩形框基础上，放到一定比例使其工作区域无效，然后刷新。用Refresh()效率最低；可以百度：Refresh()和Invalidate(),Update()的作用和区别
                //if(MouseRect != Rectangle.Empty)
                //{
                //    MouseRect.Inflate(new Size(2, 2));
                //    this.pictureBoxBackGround.Invalidate(); // 其工作与去无效,但是不会刷新子控件，导致输入框内容清楚，要刷新才能显示
                //    this.pictureBoxBackGround.Update();
                //}

                #endregion//--获取选择的控件，遍历
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        //将单元格输入框隐藏，可以选择区域
        private void Cells_MouseMoveHideForCopySelect(object sender, MouseEventArgs e)
        {
            if (_IS_SHIFT_KEYDOWN && e.Button == MouseButtons.Left && _IsCopyCellMouseDown)
            {
                ((Control)sender).Hide();

                //this.pictureBoxBackGround.pe
                MouseEventArgs ee = new MouseEventArgs(MouseButtons.Left, 1, e.X + ((Control)sender).Left, e.Y + ((Control)sender).Top, 1);
                WorkArea_MouseDown(null, ee);

                _IsCopyCellMouseDown = false;
            }
        }

        bool _IsCopyCellMouseDown = false;
        private void Cells_MouseDown(object sender, MouseEventArgs e)
        {
            _IsCopyCellMouseDown = true;
        }

        private void Cells_MouseUp(object sender, MouseEventArgs e)
        {
            _IsCopyCellMouseDown = false;
        }

        /// <summary>
        /// 复制单元格：CellInfor[,] _ArrCell
        /// </summary>
        private void CopyCellsEvent(object sender, EventArgs e)
        {
            if (_ArrCell != null && _TableType && _TableInfor != null)
            {
                //ShowSearch(_Need_Assistant_Control); // 显示指示图标

                Point current = _CurrentCellIndex;
                //MessageBox.Show(_ArrCell.Length.ToString()); // i * j
                if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed && _Need_Assistant_Control is RichTextBoxExtended)
                {
                    for (int i = 0; i < _ArrCell.GetLength(0); i++)
                    {
                        for (int j = 0; j < _ArrCell.GetLength(1); j++)
                        {
                            //还在表格有效区域
                            if (current.X + i < _TableInfor.GroupColumnNum * _TableInfor.RowsCount &&
                                current.Y + j < _TableInfor.ColumnsCount)
                            {
                                if (_ArrCell[i, j] == null)
                                {
                                    continue;
                                }

                                if (_ArrCell[i, j].Text.Trim() == "")
                                {
                                    continue;
                                }

                                //不显示右击菜单
                                //_SettingRowForeColor = true; //会导致只能一列，和刷新有关
                                Comm._EnterNotShowMenu = true;
                                _IS_CTRL_KEYDOWN = false; //防止某些人：粘贴后，先释放Ctrl再释放V，就会导致触发Ctrl的赋值，更新粘贴板

                                ShowCellRTBE(current.X + i, current.Y + j);

                                Comm._EnterNotShowMenu = false;
                                //_SettingRowForeColor = false;

                                if (!((RichTextBoxExtended)_Need_Assistant_Control).ReadOnly && !((RichTextBoxExtended)_Need_Assistant_Control).Name.StartsWith("签名"))
                                {
                                    if (_ArrCell[i, j].IsRtf_OR_Text)
                                    {
                                        _Need_Assistant_Control.Text = _ArrCell[i, j].Text;
                                        ((RichTextBoxExtended)_Need_Assistant_Control).Rtf = _ArrCell[i, j].Rtf;
                                        ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = true;

                                    }
                                    else
                                    {
                                        _Need_Assistant_Control.Text = _ArrCell[i, j].Text;
                                    }
                                }
                            }
                            else
                            {
                                //不能break，因为可能是第一行的列超出，下面还是复制。
                            }
                        }
                    }
                }
            }
        }

        MyOpaqueLayer _PictureBoxSelectArea = null;
        /// <summary>
        /// 
        /// 和点击画布有干扰 Alt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkArea_MouseDown(object sender, MouseEventArgs e)
        {



            //--------------------------------------鼠标写字-------------------
            _MouseDownDraw = true;
            MouseDownOldPoint.X = e.X;
            MouseDownOldPoint.Y = e.Y;
            //--------------------------------------鼠标写字-------------------

            if (!_IS_SHIFT_KEYDOWN)
            {
                return;
            }
            else
            {
                //这样的话，按下的开始点在表格之外，还是达不到自动单元格区域的效果；或者拖动点在表格之外，也一样。
                if (_TableType && _TableInfor != null)
                {
                    if (!GetTableRect(_TableInfor).Contains(e.Location))
                    {
                        return;
                    }
                }


                //选中区域变成蓝色激活状态，绘制不出来。只能用个图片来实现了。
                if (_PictureBoxSelectArea == null || _PictureBoxSelectArea.IsDisposed)
                {
                    _PictureBoxSelectArea = new MyOpaqueLayer(168, false);
                    _PictureBoxSelectArea.BackColor = Color.SkyBlue;
                    _PictureBoxSelectArea.Visible = true;
                    _PictureBoxSelectArea.BringToFront();

                    //不直接在画布上绘制，在半透明图层上绘制
                    //_PictureBoxSelectArea.SizeChanged +=new EventHandler(_PictureBoxSelectArea_SizeChanged);
                }
            }

            _MouseRect = new Rectangle(0, 0, 0, 0);
            _WorkArea = new Rectangle(this.pictureBoxBackGround.ClientRectangle.X + this.pictureBoxBackGround.Left + 5,
                this.pictureBoxBackGround.ClientRectangle.Y + this.pictureBoxBackGround.Top + 4, this.pictureBoxBackGround.ClientRectangle.Width, this.pictureBoxBackGround.ClientRectangle.Height);

            ////取消选中处理
            //for (int i = 0; i < SelectedCtrl.Count; i++)
            //{
            //}

            DrawRectangle();
            _MouseIsDown = true;
            DrawStart(e.Location);

        }

        /// <summary>
        /// 获取表格的外边框
        /// </summary>
        /// <param name="tableInforPara"></param>
        /// <returns></returns>
        private Rectangle GetTableRect(TableInfor tableInforPara)
        {
            if (!tableInforPara.Vertical)
            {
                ////表格右边竖线
                //g.DrawLine(pen, new Point(tableInforPara.Location.X + tableInforPara.PageRowsWidth * (field + 1), tableInforPara.Location.Y), 
                //                  new Point(tableInforPara.Location.X + tableInforPara.PageRowsWidth * (field + 1), tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.RowsCount));

                ////最下面的横线
                //g.DrawLine(pen, new Point(tableInforPara.Location.X + tableInforPara.PageRowsWidth * field, tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.RowsCount), new Point(tableInforPara.Location.X + tableInforPara.PageRowsWidth * field + tableInforPara.PageRowsWidth, tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.RowsCount));

                return new Rectangle(tableInforPara.Location.X, tableInforPara.Location.Y, tableInforPara.PageRowsWidth * tableInforPara.GroupColumnNum, tableInforPara.PageRowsHeight * tableInforPara.RowsCount);
            }
            else
            {
                ////表格右边竖线
                //g.DrawLine(pen, new Point(tableInforPara.Location.X + tableInforPara.Cells[0, 0].CellSize.Width * tableInforPara.RowsCount, tableInforPara.Location.Y),
                //                new Point(tableInforPara.Location.X + tableInforPara.Cells[0, 0].CellSize.Width * tableInforPara.RowsCount, tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.ColumnsCount));

                ////最下面的横线
                //g.DrawLine(pen, new Point(tableInforPara.Location.X, tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.ColumnsCount),
                //    new Point(tableInforPara.Location.X + tableInforPara.Cells[0, 0].CellSize.Width * tableInforPara.RowsCount, tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.ColumnsCount));

                return new Rectangle(tableInforPara.Location.X, tableInforPara.Location.Y, tableInforPara.Cells[0, 0].CellSize.Width * tableInforPara.RowsCount, tableInforPara.PageRowsHeight * tableInforPara.ColumnsCount);
            }
        }

        private void WorkArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (_MouseIsDown)
            {
                //选择区域的时候，自动移动滚动条
                if (e.X > this.panelMain.ClientRectangle.Width && this.panelMain.HorizontalScroll.Visible)
                {
                    if (this.panelMain.HorizontalScroll.Value + 5 < this.panelMain.HorizontalScroll.Maximum)
                    {
                        this.panelMain.HorizontalScroll.Value = this.panelMain.HorizontalScroll.Value + 5;
                        //this.panelMain.HorizontalScroll.Value = e.X; //this.panelMain.HorizontalScroll.Maximum;// e.X;
                        //this.pictureBoxBackGround.Refresh();
                    }
                    else
                    {
                        this.panelMain.HorizontalScroll.Value = this.panelMain.HorizontalScroll.Maximum;
                    }

                    //ResizeToRectangle(e.Location);//矩形框，可能来不及刷新过来
                }

                if (e.Y > this.panelMain.ClientRectangle.Height && this.panelMain.VerticalScroll.Visible)
                {
                    if (this.panelMain.VerticalScroll.Value + 5 < this.panelMain.VerticalScroll.Maximum)
                    {
                        this.panelMain.VerticalScroll.Value = this.panelMain.VerticalScroll.Value + 5;
                        //this.panelMain.HorizontalScroll.Value = e.X; //this.panelMain.HorizontalScroll.Maximum;// e.X;
                        //this.pictureBoxBackGround.Refresh();
                    }
                    else
                    {
                        this.panelMain.VerticalScroll.Value = this.panelMain.VerticalScroll.Maximum;
                    }

                    //ResizeToRectangle(e.Location);//矩形框，可能来不及刷新过来
                }

                //这样的话，按下的开始点在表格之外，还是达不到自动单元格区域的效果；或者拖动点在表格之外，也一样。
                if (_TableType && _TableInfor != null)
                {
                    Rectangle rectTable = GetTableRect(_TableInfor);
                    if (!rectTable.Contains(e.Location))
                    {
                        this.pictureBoxBackGround.Refresh();

                        //矩形框，可能来不及刷新过来。需要绘制一下。而且如果，x和y有一个超出边界，改变还要合理的调整
                        if (_PictureBoxSelectArea != null && !_PictureBoxSelectArea.IsDisposed)
                        {

                            if (e.Location.X < rectTable.Location.X + rectTable.Width)
                            {
                                ResizeToRectangle(new Point(e.Location.X, _PictureBoxSelectArea.Location.Y + _PictureBoxSelectArea.Height));//矩形框，可能来不及刷新过来
                            }
                            else
                            {
                                ResizeToRectangle(new Point(_PictureBoxSelectArea.Location.X + _PictureBoxSelectArea.Width, e.Location.Y));//矩形框，可能来不及刷新过来
                            }

                            //ResizeToRectangle(new Point(_PictureBoxSelectArea.Location.X + _PictureBoxSelectArea.Width, _PictureBoxSelectArea.Location.Y + _PictureBoxSelectArea.Height));//矩形框，可能来不及刷新过来
                        }
                        else
                        {
                            ResizeToRectangle(e.Location);
                        }

                        return;
                    }
                }

                ResizeToRectangle(e.Location);
            }

            if (!_IS_SHIFT_KEYDOWN)
            {
                if (_DragDrawRectMode == 1 || _DragDrawRectMode == 2)
                {
                    //--------------------------------------鼠标写字-------------------
                    MouseDownNewPoint.X = e.X;
                    MouseDownNewPoint.Y = e.Y;
                    MouseDownDrawLine();
                    //--------------------------------------鼠标写字-------------------
                }
            }

        }
        private void ResizeToRectangle(Point p)
        {
            //DrawRectangle();  //历史矩形阴影
            //_LastDrawRectangle.Inflate(2, 2);//扩大区域，结构放大
            //this.pictureBoxBackGround.Invalidate(new Region(_LastDrawRectangle));




            //this.pictureBoxBackGround.Invalidate(_LastDrawRectangle);
            //this.pictureBoxBackGround.Update();

            //this.Refresh();
            //this.pictureBoxBackGround.Capture = false;
            //Cursor.Clip = Rectangle.Empty;
            //ControlPaint.DrawReversibleFrame(_LastDrawRectangle, Color, FrameStyle.Dashed);
            //this.panelMain.Refresh();

            _MouseRect.Width = p.X - _MouseRect.Left;
            _MouseRect.Height = p.Y - _MouseRect.Top;

            if (_TableType && _TableInfor != null)
            {
                //矩形框的四个点，去计算所在单元格的边界点
                Rectangle rectSelected = MouseRectAutoCell(_MouseRect);
                Rectangle rect = this.RectangleToScreen(rectSelected);
                Rectangle currentDrawRectangleDrawFocus = new Rectangle(rectSelected.X + 1,
                                            rectSelected.Y + 1,
                                            rectSelected.Width - 3,
                                            rectSelected.Height - 3);


                if (currentDrawRectangleDrawFocus.Equals(_LastDrawRectangle))
                {
                    //return; //防止闪烁
                }
                else
                {
                    this.pictureBoxBackGround.Refresh(); //但是会闪烁
                    //this.pictureBoxBackGround.Invalidate(_LastDrawRectangle);
                    //this.pictureBoxBackGround.Update();
                }


            }
            else
            {
                this.pictureBoxBackGround.Refresh(); //但是会闪烁
            }

            //this.pictureBoxBackGround.Refresh(); //但是会闪烁


            DrawRectangle();

            //this.pictureBoxBackGround.Update();
        }




        /// <summary>
        /// 如果是表格，那么要将选择q区域的矩形框自动要单元格边界
        /// </summary>
        private Rectangle MouseRectAutoCell(Rectangle rectPara)
        {
            Rectangle rect = rectPara;// Rectangle.Empty;

            //防止从右下，往左上拖动
            if (rect.Width < 0 || rect.Height < 0)
            {
                if (rect.Width < 0)
                {
                    rect = new Rectangle(rect.X + rect.Width, rect.Y, Math.Abs(rect.Width), rect.Height);
                }

                if (rect.Height < 0)
                {
                    rect = new Rectangle(rect.X, rect.Y + rect.Height, rect.Width, Math.Abs(rect.Height));
                }
            }

            //if (rectPara.Width < 0)
            //{
            //    rect = new Rectangle(rect.Location.X + rect.Width, rect.Y, -rect.Width, rect.Height);
            //}

            //if (rectPara.Height < 0)
            //{
            //    rect = new Rectangle(rect.Location.X, rect.Y + rect.Height, rect.Width, -rect.Height);
            //}


            //矩形框的四个点，去计算所在单元格的边界点
            Point p1 = rect.Location;  //这样的话，按下的开始点在表格之外，还是达不到自动单元格区域的效果；或者拖动点在表格之外，也一样。 鼠标按下的时候做限制
            Point p2 = new Point(p1.X + rect.Width, p1.Y);
            Point p3 = new Point(p1.X + rect.Width, p1.Y + rect.Height);
            Point p4 = new Point(p1.X, p1.Y + rect.Height);

            CellInfor ci = null;

            ci = GetCellWithPoint(p1);
            if (ci != null)
            {
                p1 = ci.Loaction;
            }

            ci = GetCellWithPoint(p2);
            if (ci != null)
            {
                p2 = new Point(ci.Loaction.X + ci.Rect.Width, ci.Loaction.Y);
            }

            ci = GetCellWithPoint(p3);
            if (ci != null)
            {
                p3 = new Point(ci.Loaction.X + ci.Rect.Width, ci.Loaction.Y + ci.Rect.Height);
            }

            ci = GetCellWithPoint(p4);
            if (ci != null)
            {
                p4 = new Point(ci.Loaction.X, ci.Loaction.Y + ci.Rect.Height);
            }

            //return new Rectangle(p1, new Size(p3.X - p1.X, p3.Y - p1.Y));

            return new Rectangle(new Point(p1.X + 3, p1.Y + 2), new Size(p3.X - p1.X - 3, p3.Y - p1.Y - 2));
        }

        /// <summary>
        /// 根据坐标，得到所在的单元格
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private CellInfor GetCellWithPoint(Point p)
        {
            if (_TableType && _TableInfor != null)
            {
                int fieldIndex = 0;
                for (int field = 0; field < _TableInfor.GroupColumnNum; field++)
                {
                    fieldIndex = _TableInfor.RowsCount * field;

                    for (int i = 0; i < _TableInfor.RowsCount; i++)
                    {
                        for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                        {
                            //if (_TableInfor.Cells[i + fieldIndex, j].Rect.Contains(e.X, e.Y) && !_TableInfor.Cells[i + fieldIndex, j].IsMergedNotShow)
                            //if (_TableInfor.Cells[i + fieldIndex, j].Rect.IntersectsWith(MouseRect))
                            if (_TableInfor.Cells[i + fieldIndex, j].Rect.Contains(p))
                            {
                                return _TableInfor.Cells[i + fieldIndex, j];
                            }
                        }
                    }
                }
            }

            return null;
        }

        private void DrawStart(Point StartPoint)
        {
            //this.Capture = true;
            this.pictureBoxBackGround.Capture = true;

            //这是设置鼠标筐选时鼠标的移动区域 和控件对鼠标的捕获
            Cursor.Clip = this.RectangleToScreen(_WorkArea);  //this.RectangleToScreen(this.panel1.ClientRectangle);

            _MouseRect = new Rectangle(StartPoint.X, StartPoint.Y, 0, 0);
        }

        /// <summary>
        /// 绘制拖选的矩形框
        /// </summary>
        private void DrawLastRectangular()
        {
            if (_DragDrawRectMode == 0)
            {
                return;
            }

            //Graphics g = this.pictureBoxBackGround.CreateGraphics();           //不重绘，一旦刷新就没有了
            //Graphics g = Graphics.FromImage(this.pictureBoxBackGround.Image);  //刷新，不需要重绘制
            Graphics g = this.pictureBoxBackGround.CreateGraphics();  //默认
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_DragDrawRectMode == 1)
            {
                g = this.pictureBoxBackGround.CreateGraphics();
            }
            else if (_DragDrawRectMode == 2)
            {
                g = Graphics.FromImage(this.pictureBoxBackGround.Image);
            }
            else
            {
                g.Dispose();
                return;
            }

            g.DrawRectangle(Pens.Red, _MouseRect);
            //Pen p = new Pen(Brushes.Red, 1.5f);
            //p.DashStyle = DashStyle.DashDotDot;
            //g.DrawRectangle(Pens.Red, MouseRect);
            g.Dispose();
        }

        //鼠标写字的全局变量
        private PointF MouseDownOldPoint;
        private PointF MouseDownNewPoint;
        private bool _MouseDownDraw = false;

        /// <summary>
        /// 绘制两点的线
        /// </summary>
        private void MouseDownDrawLine()
        {
            if (_MouseDownDraw == true)
            {
                Graphics g = this.pictureBoxBackGround.CreateGraphics();

                if (_DragDrawRectMode == 1)
                {
                    g = this.pictureBoxBackGround.CreateGraphics();
                }
                else if (_DragDrawRectMode == 2)
                {
                    g = Graphics.FromImage(this.pictureBoxBackGround.Image);
                }

                g.SmoothingMode = SmoothingMode.AntiAlias;
                Pen redPen = new Pen(Color.Red, 2);
                g.DrawLine(redPen, MouseDownOldPoint, MouseDownNewPoint);
                MouseDownOldPoint.X = MouseDownNewPoint.X;
                MouseDownOldPoint.Y = MouseDownNewPoint.Y;
                redPen.Dispose();

                if (_DragDrawRectMode == 1)
                {
                    ////this.pictureBoxBackGround.Refresh();
                    //DrawLastRectangular(); //临时显示的
                }
                else if (_DragDrawRectMode == 2)
                {
                    //DrawLastRectangular();
                    this.pictureBoxBackGround.Refresh();
                }
            }
        }

        #endregion 画布拖动操作 复制内容 鼠标写字

        # region 点击画布等时候，将当前页对应的节点选中
        //选择打开页的节点为选中状态
        private void SetOpenPageTreeNodeSelected()
        {
            //try
            //{
            //    //防止又点了其他节点，所以还是选中打开页的节点
            //    if (_CurrentTemplateNameTreeNode != null && _CurrentPage != "0" && _CurrentPage != "")
            //    {
            //        //if (_CurrentTemplateNameTreeNode.Nodes.Count >= int.Parse(_CurrentPage)) //筛选的话，个数判断就不对了
            //        //{
            //        //如果是刷新的话，刚才的选中节点已经被移除了；所以选择不上了。
            //        //this.treeViewPatients.SelectedNode = _CurrentTemplateNameTreeNode.Nodes[int.Parse(_CurrentPage) - 1]; //切换倒序排序就会有问题
            //        for (int i = 0; i < _CurrentTemplateNameTreeNode.Nodes.Count; i++)
            //        {
            //            if (_CurrentTemplateNameTreeNode.Nodes[i].Text == _CurrentPage)
            //            {
            //                this.treeViewPatients.SelectedNode = _CurrentTemplateNameTreeNode.Nodes[i];
            //            }
            //        }
            //        //}
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Comm.Logger.WriteErr(ex);
            //    throw ex;
            //}
        }
        # endregion 点击画布等时候，将当前页对应的节点选中

        # region 提示签名内容
        /// <summary>
        /// 工具栏的标签上显示：谁签名
        /// </summary>
        /// <param name="name"></param>
        private void SetSignName(string id, string name)
        {
            if (id != "")
            {
                ////    toolStripStatusLabelType.Text = "当前行：" + id + " | " + name;
                //toolStripStatusLabelType.Text = "当前行：" + id;

                if (CallBackUpdateDocumentDetailInfor != null)
                {
                    CallBackUpdateDocumentDetailInfor(new string[] { null, "该行：" + id + " | " + name }, null);
                }
            }
            else
            {
                //toolStripStatusLabelType.Text = "当前行：未签名";

                if (CallBackUpdateDocumentDetailInfor != null)
                {
                    CallBackUpdateDocumentDetailInfor(new string[] { null, "该行：未签名" }, null);
                }
            }
        }
        # endregion 提示签名内容

        # region 将同一段落的行，设置成与上一行相同的行文字的颜色
        private void SetRowForeColor()
        {
            //如果含有时间列，当前行没有行文字颜色，那么继承上一行的行文字颜色
            //------------有些护理表单，不会文字换行，但是会有一个段落多行，只填写第一行日期时间，颜色要一样
            //if (_CurrentCellIndex.X > 0 && _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].Text.Trim() == "" && _RowForeColorsList.Count > 0 && _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间") != null && string.IsNullOrEmpty(_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间").Text)
            //    && IsEmptyRow(_CurrentCellIndex.X))
            if (_CurrentCellIndex.X > 0 && _RowForeColorsList.Count > 0 && _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间") != null && string.IsNullOrEmpty(_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间").Text)
                    && IsEmptyRow(_CurrentCellIndex.X))
            {
                //如果当前行的行文字颜色为空
                if (string.IsNullOrEmpty(_TableInfor.Rows[_CurrentCellIndex.X].RowForeColor) && !string.IsNullOrEmpty(_TableInfor.Rows[_CurrentCellIndex.X - 1].RowForeColor))  //
                {
                    //取得上一行的行文字颜色，设置到当前行
                    _TableInfor.Rows[_CurrentCellIndex.X].RowForeColor = _TableInfor.Rows[_CurrentCellIndex.X - 1].RowForeColor;
                    _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.ForeColor = Color.FromName(_TableInfor.Rows[_CurrentCellIndex.X - 1].RowForeColor);

                    //更新保存的行颜色
                    XmlNode cellNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))).SelectSingleNode("Record[@ID='" + _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString() + "']");
                    //行线颜色 更新
                    string newRowInfor = "";
                    newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].RowLineColor.Name + "¤";
                    newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType.ToString() + "¤";
                    newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].RowForeColor + "¤";
                    newRowInfor += _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor.Name;

                    (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), newRowInfor);      //将行信息保存到Xml数据文件中
                }
            }
            //------------有些护理表单，不会文字换行，但是会有一个段落多行，只填写第一行日期时间，颜色要一样
        }
        # endregion 将同一段落的行，设置成与上一行相同的行文字的颜色

        # region 获取当前用户权限等级
        private int GetNurseLevel(string User_DegreePara)
        {
            string[] arrLevels = _NurseLevel.Split('¤');
            int ret = -1;

            for (int i = 0; i < arrLevels.Length; i++)
            {
                if (arrLevels[i] == User_DegreePara)
                {
                    ret = i;
                    break;
                }
            }

            return ret;
        }
        # endregion 获取当前用户权限等级

        # region 非表格，光标进入，设置默认值
        private void InputBox_EnterSetDefaulyValue(object sender, EventArgs e)
        {
            try
            {
                if ((_DSS.DefaultDateEnterOrDoubleClick && e != null)
                     || (!_DSS.DefaultDateEnterOrDoubleClick && e == null))
                {

                    RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;
                    if (rtbe.Text.Trim() == "" && !rtbe.ReadOnly)
                    {
                        switch (rtbe.Name)
                        {
                            case "签名":
                                //
                                break;

                            case "日期":


                                //单击，有的时候容易误点就生成了默认值
                                if (rtbe._YMD_Default.Contains("yyyy"))
                                {
                                    rtbe.Text = string.Format("{0:" + rtbe._YMD_Default + "}", GetServerDate());
                                }
                                else
                                {
                                    rtbe.Text = string.Format("{0:yyyy-MM-dd}", GetServerDate());
                                }

                                break;

                            case "时间":
                                //单击，有的时候容易误点就生成了默认值
                                rtbe.Text = string.Format("{0:" + DateFormat_HM + "}", GetServerDate());

                                break;

                            default:
                                //补充上，以日期和时间开头的控件名，也是日期和时间
                                if (rtbe.Name.StartsWith("日期"))
                                {
                                    //rtbe.Text = string.Format("{0:yyyy-MM-dd}", GetServerDate());

                                    if (rtbe._YMD_Default.Contains("yyyy"))
                                    {
                                        rtbe.Text = string.Format("{0:" + rtbe._YMD_Default + "}", GetServerDate());
                                    }
                                    else
                                    {
                                        rtbe.Text = string.Format("{0:yyyy-MM-dd}", GetServerDate());
                                    }
                                }
                                else if (rtbe.Name.StartsWith("时间"))
                                {
                                    rtbe.Text = string.Format("{0:" + DateFormat_HM + "}", GetServerDate());
                                }
                                else if (rtbe._YMD_Default != "")
                                {
                                    try
                                    {
                                        rtbe.Text = string.Format("{0:" + rtbe._YMD_Default + "}", GetServerDate());
                                    }
                                    catch
                                    {
                                        ShowInforMsg("在对输入框：" + rtbe.Name + "，按照指定格式：" + rtbe._YMD_Default + "设定时出现异常，请确认格式是否合理。", true);
                                    }
                                }
                                else if (rtbe._Default != "") //光标进入默认值
                                {
                                    if (!_IsLoading)
                                    {
                                        rtbe.Text = rtbe._Default;
                                    }
                                }

                                break;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowInforMsg(ex.StackTrace, true);
                throw ex;
            }
        }
        # endregion 非表格，光标进入，设置默认值

        #region 评分
        /// <summary>
        /// 评分
        /// 某一张表单的评分，将评分结果自动写入评分输入框。
        /// 但是实际中，一般利用合计就能满足要求了。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItemScore_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                //进行遍历，根据配置进行评分，累加，并且找到：评分的目标写入控件
                Control scoreCl = null;
                double scoreAll = 0;
                string[] eachControl;
                string[] eachArr;
                string scoreScript = ""; //Score="目标一@5¤xx@2"

                StringBuilder msg = new StringBuilder(); //消息

                if (this.pictureBoxBackGround.Controls.Count > 0)
                {
                    //界面上删除所有的以前无效的辅助线图标mark
                    foreach (Control control in this.pictureBoxBackGround.Controls)
                    {
                        if (control.Name == "评分")
                        {
                            scoreCl = control;      //找到显示的目标        
                            continue;
                        }
                        else if (control.Name == "lblShowSearch")
                        {
                            //是索索图标图片，跳过   
                            continue;
                        }

                        if (control is RichTextBoxExtended)
                        {
                            scoreScript = ((RichTextBoxExtended)control).Score;
                            if (!string.IsNullOrEmpty(scoreScript))
                            {
                                eachControl = scoreScript.Split('¤');
                                for (int i = 0; i < eachControl.Length; i++)
                                {
                                    eachArr = eachControl[i].Split('@');

                                    if (Regex.IsMatch(((RichTextBoxExtended)control).Text, eachArr[0].Trim()))
                                    {
                                        scoreAll += double.Parse(eachArr[1].Trim());
                                        msg.Append("项目：" + control.Name + "，评分：" + eachArr[1].Trim() + "\r\n");

                                        break;//找到第一个满足评分的项目就跳出
                                    }
                                }
                            }
                        }
                        else if (control is TransparentRTBExtended)  //TransparentRTBExtended是继承RichTextBoxExtended的 ,所以这里注释掉也可以
                        {
                            scoreScript = ((TransparentRTBExtended)control).Score;
                            if (!string.IsNullOrEmpty(scoreScript))
                            {
                                eachControl = scoreScript.Split('¤');
                                for (int i = 0; i < eachControl.Length; i++)
                                {
                                    eachArr = eachControl[i].Split('@');

                                    if (Regex.IsMatch(((TransparentRTBExtended)control).Text, eachArr[0].Trim()))
                                    {
                                        scoreAll += double.Parse(eachArr[1].Trim());
                                        msg.Append("项目：" + control.Name + "，评分：" + eachArr[1].Trim() + "\r\n");

                                        break;//找到第一个满足评分的项目就跳出
                                    }
                                }
                            }
                        }
                        else if (control is CheckBoxExtended)
                        {
                            scoreScript = ((CheckBoxExtended)control).Score;
                            if (!string.IsNullOrEmpty(scoreScript))
                            {
                                eachControl = scoreScript.Split('¤');
                                for (int i = 0; i < eachControl.Length; i++)
                                {
                                    eachArr = eachControl[i].Split('@');

                                    if (((CheckBoxExtended)control).Checked.ToString().ToLower() == eachArr[0].ToLower().Trim())
                                    {
                                        scoreAll += double.Parse(eachArr[1].Trim());
                                        msg.Append("项目：" + control.Name + "，评分：" + eachArr[1].Trim() + "\r\n");
                                        break;//找到第一个满足评分的项目就跳出
                                    }
                                }
                            }
                        }
                        else if (control is ComboBoxExtended)
                        {
                            scoreScript = ((ComboBoxExtended)control).Score;
                            if (!string.IsNullOrEmpty(scoreScript))
                            {
                                eachControl = scoreScript.Split('¤');
                                for (int i = 0; i < eachControl.Length; i++)
                                {
                                    eachArr = eachControl[i].Split('@');

                                    if (Regex.IsMatch(((ComboBoxExtended)control).Text, eachArr[0].Trim()))
                                    {
                                        scoreAll += double.Parse(eachArr[1].Trim());
                                        msg.Append("项目：" + control.Name + "，评分：" + eachArr[1].Trim() + "\r\n");
                                        break;//找到第一个满足评分的项目就跳出
                                    }
                                }
                            }
                        }
                        else if (control is LabelExtended)
                        {

                        }
                        else if (control is PictureBox)
                        {
                            //图片，暂时无需评分
                        }
                        else if (control is MonthCalendar)
                        {
                            //图片，暂时无需评分
                        }
                        else
                        {
                            //没有指定的控件
                            ShowInforMsg("在评分的时候，遇到无法识别的类型：" + control.ToString()); //Label：System.Windows.Forms.Label, Text: 调试科别名字
                        }
                    }
                }

                if (scoreCl != null)
                {
                    HorizontalAlignment temp = ((RichTextBoxExtended)scoreCl).SelectionAlignment;

                    scoreCl.Text = scoreAll.ToString();

                    ((RichTextBoxExtended)scoreCl).SelectionAlignment = temp;
                }

                if (scoreAll == 0)
                {
                    ShowInforMsg("本次评分的总分为：" + scoreAll.ToString() + " 分。");
                }
                else
                {
                    ShowInforMsg("本次评分的总分为：" + scoreAll.ToString() + " 分，以下为得分明细：\r\n" + msg.ToString());
                }

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                ShowInforMsg(ex.StackTrace, true);
                throw ex;
            }
        }

        #endregion 评分

        # region 执行脚本，实现界面上联动效果类似js

        //<Action Name="体温过高" Event="CheckedChanged">checkBox1.Checked Then textBox6.Text = 6666 And checkBox2.Checked = True And textBox7.Text = 777 And comboBox1.Items = 11,22,33 And comboBox1.Text = 33 </Action>
        //<Action Name="体温过高" Event="CheckedChanged">!checkBox1.Checked Then textBox6.Text = 取消 And checkBox2.Checked = False And textBox7.Text = textBox6.Text  And comboBox1.Items = comboBox1.Tag  And comboBox1.SelectedIndex = 0 And txt1.Visible = False</Action>

        /// <summary>
        /// 脚本中的参数化替换
        /// </summary>
        /// <param name="script">脚本</param>
        /// <returns></returns>
        private string ScriptPara(string script)
        {
            if (script.Contains("%")) //参数化的用%来标识  //不能为空，如果为空，脚本会执行错误，需要将空变成''
            {
                string retScript = script;

                //先进行：用户名替换，用户等级的替换：
                retScript = retScript.Replace("%UserName%", GlobalVariable.LoginUserInfo.UserName);
                retScript = retScript.Replace("%UserDegree%", GlobalVariable.LoginUserInfo.TitleName);
                retScript = retScript.Replace("%CurrentPage%", _CurrentPage); //指定那些页，可以执行该脚本


                //表格参数替换
                if (_TableType && _TableInfor != null &&
                    (retScript.Contains("上一行") || retScript.Contains("下一行") || retScript.Contains("当前行")))
                {
                    //表格选中了某行，存在处理对象当前行
                    if (_CurrentCellIndex.X != -1 && _TableInfor.CurrentCell != null)
                    {
                        //<!-- 段落中间不能插入行，防止段落修改错误-->
                        //<Action Name="出量名称" IsTable="True" Event="UpInsertRow">%上一行.日期时间% == %当前行.日期时间% Then  Error(不能进行删除操作。)</Action>
                        //<Action Name="出量名称" IsTable="True" Event=nameof(EntXmlModel.DownInsertRow)>%下一行.日期时间% == %当前行.日期时间% Then  Error(不能进行删除操作。)</Action>
                        //根据rowid和
                        XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                        XmlNode xn = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString(), "=", nameof(EntXmlModel.Record)));
                        //"Record[@ID='" + _TableInfor.Rows[_CurrentCellIndex.X].ID + "']"
                        string recordDt = "";
                        string sum = "";

                        if (retScript.Contains("当前行"))
                        {
                            recordDt = GetRowDateTime((xn as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));
                            retScript = retScript.Replace("%当前行.日期时间%", recordDt);

                            sum = (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total));
                            if (string.IsNullOrEmpty(sum))
                            {
                                sum = "''";
                            }
                            retScript = retScript.Replace("%当前行.Sum%", sum);
                        }

                        if (retScript.Contains("上一行") && xn.PreviousSibling != null)
                        {
                            recordDt = GetRowDateTime((xn.PreviousSibling as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));
                            retScript = retScript.Replace("%上一行.日期时间%", recordDt);

                            sum = (xn.PreviousSibling as XmlElement).GetAttribute(nameof(EntXmlModel.Total));
                            if (string.IsNullOrEmpty(sum))
                            {
                                sum = "''";
                            }
                            retScript = retScript.Replace("%上一行.Sum%", sum);
                        }

                        if (retScript.Contains("下一行") && xn.NextSibling != null)
                        {
                            recordDt = GetRowDateTime((xn.NextSibling as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));
                            retScript = retScript.Replace("%下一行.日期时间%", recordDt);

                            sum = (xn.NextSibling as XmlElement).GetAttribute(nameof(EntXmlModel.Total));
                            if (string.IsNullOrEmpty(sum))
                            {
                                sum = "''";
                            }
                            retScript = retScript.Replace("%下一行.Sum%", sum);
                        }
                    }
                }


                //其他页数据替换：比如第二页的数据要根据第一页的数据来判断显示：必须填写项目等
                if (retScript.Contains("%P")) //如果还包含百分号，那么就是指定页的数据，来判断的了。合计的时候，去指定页数据也是类似逻辑
                {
                    //%页数.项目名%， %P1.性别% (大写字母P表示页码的意思，后面紧跟页码数字，如：1,2,10,99 ……)
                    //<Action Name="脱水" Event="TemplateLoad">%CurrentPage% == 2  And %P1.时间% == 12:45 Then Warn(只有指定的第2页可以执行该脚本)</Action>
                    if (_RecruitXML != null)
                    {
                        //可能还会有多个%P这样的参数配置条件
                        int count = retScript.Split(new string[] { "%P" }, StringSplitOptions.RemoveEmptyEntries).Length; //这样如果%P开头就会去空后，个数不对了。所以不用减1

                        if (!retScript.StartsWith("%P"))
                        {
                            count--;
                        }

                        string settingStr = "";
                        string page = ""; // settingStr.Split('.')[0].TrimStart('P').Trim();
                        string name = ""; // settingStr.Split('.')[1].Trim();
                        XmlNode thisPage = null;

                        int start = 0, end = 0; //截取字符串用的索引

                        for (int i = 0; i < count; i++)
                        {
                            //3 == 2  And %P1.时间% == 12:45 Then Warn(只有指定的第2页可以执行该脚本) 
                            start = retScript.IndexOf("%P");
                            end = retScript.IndexOf("%", start + 1) + 1; //从本次循环开始的%P开始查找下一个百分号（）+1跳过开头的百分号）

                            if (start < end)
                            {
                                settingStr = retScript.Substring(start, end - start); //去过百分号后的表达式条件

                                page = settingStr.Split('.')[0].TrimStart('%').TrimStart('P').Trim();
                                name = settingStr.Split('.')[1].TrimEnd('%').Trim();

                                thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), page, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));

                                if (thisPage != null && !string.IsNullOrEmpty((thisPage as XmlElement).GetAttribute(name).Split('¤')[0].Trim()))
                                {
                                    retScript = retScript.Replace(settingStr, (thisPage as XmlElement).GetAttribute(name).Split('¤')[0]); //替换
                                }
                                else
                                {
                                    //不存在本页数据的话，为了不影响其他的判断，设置为空。
                                    retScript = retScript.Replace(settingStr, "''"); //替换为空
                                }
                            }
                        }
                    }
                }

                return retScript;
            }
            else
            {
                return script;
            }
        }

        private void Script_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_IsLoading)
                {
                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                if (_node_Script != null && _node_Script.ChildNodes.Count > 0)
                {
                    CheckBoxExtended chk = (CheckBoxExtended)sender;

                    foreach (XmlNode xn in _node_Script.ChildNodes)
                    {
                        //如果是注释那么跳过
                        if (xn.Name == @"#comment" || xn.Name == @"#text")
                        {
                            continue;
                        }

                        //找到当前控件，对应的脚本，多个事件合并共用
                        //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == chk.Name && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)) == "CheckedChanged")
                        if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == chk.Name && ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains("CheckedChanged"))
                        {
                            Evaluator.EvaluateRunString(this, ScriptPara(xn.InnerText.Trim()));
                        }
                    }

                }

                IsNeedSave = true; //值改变，本身就要提示保存

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                ShowInforMsg("执行脚本发生异常：\r\n" + ex.StackTrace, true);
                throw ex;
            }
        }

        private void Script_CheckedStateChanged(object sender, EventArgs e)
        {
            //_IsNeedSave = true; //值改变，本身就要提示保存

            try
            {
                if (_IsLoading)
                {
                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                if (_node_Script != null && _node_Script.ChildNodes.Count > 0)
                {
                    CheckBoxExtended chk = (CheckBoxExtended)sender;

                    foreach (XmlNode xn in _node_Script.ChildNodes)
                    {
                        //如果是注释那么跳过
                        if (xn.Name == @"#comment" || xn.Name == @"#text")
                        {
                            continue;
                        }

                        //找到当前控件，对应的脚本，多个事件合并共用
                        //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == chk.Name && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)) == "CheckedChanged")
                        if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == chk.Name && ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains("CheckedStateChanged"))
                        {
                            Evaluator.EvaluateRunString(this, ScriptPara(xn.InnerText.Trim()));
                        }
                    }

                }

                IsNeedSave = true; //值改变，本身就要提示保存

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                ShowInforMsg("执行脚本发生异常：\r\n" + ex.StackTrace, true);
                throw ex;
            }
        }

        //隐藏气泡提斯
        private void ClearToolTip_MouseLeave(object sender, EventArgs e)
        {
            //try
            //{
            //    if (m_mb != null && !m_mb.Disposed())
            //    {
            //        m_mb.Hide();
            //    }
            //}
            //catch(Exception ex)
            //{
            //    Comm.Logger.WriteErr(ex);
            //}
        }

        /// <summary>
        /// 下拉框（dropdown样式）和输入框内容改变，触发执行脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Script_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_IsLoading)
                {
                    return;
                }

                if (sender is RichTextBoxExtended)
                {
                    RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;

                    if (rtbe.OldRtf == rtbe.Rtf || (rtbe.Text == rtbe.OldText) || (rtbe.Name.StartsWith("日期") && rtbe._FullDateText == rtbe.OldText)) //日期时间的格式是简化格式，光标进入会变成完整格式，但是如果没有修改-只是光标进入呢
                    {
                        return; //内容没有改变，跳出，不触发脚本
                    }
                }

                if (_node_Script != null && _node_Script.ChildNodes.Count > 0)
                {
                    Control ctl = (Control)sender;

                    foreach (XmlNode xn in _node_Script.ChildNodes)
                    {
                        //如果是注释那么跳过
                        if (xn.Name == @"#comment" || xn.Name == @"#text")
                        {
                            continue;
                        }

                        //找到当前控件，对应的脚本，多个事件合并共用
                        //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == ctl.Name && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)) == "TextChanged")
                        if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == ctl.Name && ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains("TextChanged"))
                        {
                            Evaluator.EvaluateRunString(this, ScriptPara(xn.InnerText.Trim()));

                            //_IsNeedSave = true; // 需要保存提示
                        }
                    }
                }

                IsNeedSave = true; // 需要保存提示
            }
            catch (Exception ex)
            {
                ShowInforMsg("执行脚本发生异常：\r\n" + ex.StackTrace, true);
                throw ex;
            }
        }

        /// <summary>
        /// 下拉框（dropdownlist样式）选择项改变，触发执行脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Script_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBoxExtended cbe = (ComboBoxExtended)sender;

            if (cbe.DropDownStyle != ComboBoxStyle.DropDown)
            {
                Script_TextChanged(sender, e);
            }
        }

        /// <summary>
        /// 初始化事，脚本联动
        /// 一般是根据标签内容，触发执行脚本，设定相关输入框的默认值
        /// 如男性病人，写错月经史的控制 就能实现了。
        /// <Action Name="性别" Event="Load">性别.Text=男 Then  男性部分的控件.Text = --</Action>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Script_TemplateLoad()
        {
            try
            {
                Control[] ct;

                //初始化脚本更新的内容，即使再手动修改保存，下次打开还是会被脚本覆盖。初始化脚本最优先
                //一般往往用于标签，但是有些可能不是基于标签来判断，而是基于之前页的数据来判断的。
                if (_node_Script != null && _node_Script.ChildNodes.Count > 0)
                {
                    foreach (XmlNode xn in _node_Script.ChildNodes)
                    {
                        //如果是注释那么跳过
                        if (xn.Name == @"#comment" || xn.Name == @"#text")
                        {
                            continue;
                        }

                        //找到当前控件，对应的脚本，多个事件可合并共用:是初始化脚本的时候，再先判断是否标签，不是标签再判断控件。没有控件则跳过
                        if (ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains("TemplateLoad"))
                        {
                            ct = this.pictureBoxBackGround.Controls.Find((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)), false);

                            if (ct == null || ct.Length == 0)
                            {
                                continue;  //找不到控件，不执行
                            }

                            if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == ct[0].Name)
                            {
                                //因为是线程启动，就会导致值改变，提示：需要保存
                                Evaluator.EvaluateRunString(this, ScriptPara(xn.InnerText.Trim()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //this.Cursor = Cursors.Default;
                ShowInforMsg("Script_TemplateLoad执行脚本发生异常：\r\n" + ex.StackTrace, true);
                throw ex;
            }
        }

        #endregion

        #region 生成所有页图片

        /// <summary>
        /// 暗门调用：参数为null，因为暗门的时候，已经打开了表单，所有的数据都有了。
        /// </summary>
        /// <returns></returns>
        protected int GetReportImage()
        {
            return GetReportImage(null, null, null);
        }
        private string _pathReport = "";                     //生成图片的绝对路径
        private string _drawTitleHospitalName = "";         //绘制图片，外部接口使用的名字

        ////区域生成图表时，次数过多，会报错;创建窗口句柄时出错。
        //句柄不断增加，导致异常
        private void ClearReport()
        {

            //区域报表调用生成图片后，每次执行后，调用本类的dispose()方法进行释放

            //this.pictureBoxBackGround.Dispose();
            //System.GC.Collect();

            //原因是楼主这段代码有问题，没有完全释放控件
            // foreach (Control cl in ptbMap.Controls)
            // {
            //   ptbMap.Controls.Remove(cl);
            //   cl.Dispose();
            // }

            //正确的释放控件写法是：
            // Panel parentControl = panel_Bottom_Customer;//你的容器控件                      
            // while (parentControl.Controls.Count > 0)
            // {
            //    if (parentControl.Controls[0] != null)                 
            //        parentControl.Controls[0].Dispose();                 
            // }

            //ClearMe();


            if (_PrintAll_TableInfor != null)
            {
                _PrintAll_TableInfor.Dispose();
                _PrintAll_TableInfor = null;
            }

            if (_BaseArrForPrintAllPageUpdate != null)
            {
                for (int i = 0; i < _BaseArrForPrintAllPageUpdate.Count; i++)
                {
                    _BaseArrForPrintAllPageUpdate[i].Dispose();
                }
            }

            if (_TableInfor != null)
            {
                _TableInfor.Dispose();
                //_PrintAll_TableInfor = null;
            }

            if (_TableHeaderList != null)
            {
                for (int i = 0; i < _TableHeaderList.Count; i++)
                {
                    _TableHeaderList[i].Dispose();
                }
            }

            if (_ListDoubleLineInputBox != null)
            {
                for (int i = 0; i < _ListDoubleLineInputBox.Count; i++)
                {
                    _ListDoubleLineInputBox[i].Dispose();
                }
            }

            //RichTextBoxExtended _PrintAll_RichTextBoxExtended;
            //TransparentRTBExtended _PrintAll_TransparentRTBExtended;
            if (_PrintAll_RichTextBoxExtended != null && !_PrintAll_RichTextBoxExtended.IsDisposed)
            {
                _PrintAll_RichTextBoxExtended.Dispose();
            }

            if (_PrintAll_TransparentRTBExtended != null && !_PrintAll_TransparentRTBExtended.IsDisposed)
            {
                _PrintAll_TransparentRTBExtended.Dispose();
            }

            if (_rtbTempForFont != null && !_rtbTempForFont.IsDisposed)
            {
                _rtbTempForFont.Dispose();
            }

            if (_RichTextBoxPrinter.TRTB != null && !_RichTextBoxPrinter.TRTB.IsDisposed)
            {
                _RichTextBoxPrinter.TRTB.Dispose();
            }

            if (_RichTextBoxPrinter.TRTBTransparent != null && !_RichTextBoxPrinter.TRTBTransparent.IsDisposed)
            {
                _RichTextBoxPrinter.TRTBTransparent.Dispose();
                _RichTextBoxPrinter = null;
            }

            //if (RtfToPicture.TRTB != null && !RtfToPicture.TRTB.IsDisposed)
            //{
            //    RtfToPicture.TRTB.Dispose();   //服务端调用就会报错         
            //}

            if (lblShowSearch != null && !lblShowSearch.IsDisposed)
            {
                lblShowSearch.Dispose();
            }

            //if (_borderDragger != null && !_borderDragger.IsDisposed)
            //{
            //    _borderDragger.Dispose();
            //}

            _VectorList.Clear();
            _RowForeColorsList.Clear();

            //_borderDragger
            //foreach (Control cl in this.Controls)
            //{
            //    if (cl != null && !cl.IsDisposed)
            //    {
            //        cl.Dispose();
            //    }
            //}

            ////释放句柄
            //this.Dispose(true); 
            //GC.SuppressFinalize(this);
            //this.Dispose(true);
            //base.Dispose();

            //this.Dispose(true); 
            //System.GC.Collect();

            //实在不行，调用方，可以监控句柄进行释放
        }

        /// <summary>
        /// 生成图片构造方法：
        /// 不指定起始页数：默认打印所有页
        /// </summary>
        /// <param name="xmlReport"></param>
        /// <param name="xmlTemplate"></param>
        /// <param name="dicPatient"></param>
        /// <returns>当前表单的总页数</returns>
        protected int GetReportImage(XmlDocument xmlReport, XmlDocument xmlTemplate, Dictionary<string, string> dicPatient, Action<Image> actionImage = null)
        {
            return GetReportImage(xmlReport, xmlTemplate, dicPatient, -9999, -9999, actionImage);
        }

        /// 调用生成表单图片的接口：
        /// 如果为空，就代表只有一家医院，或者用区域的默认样式，即：不需要医院名字作为文件夹，直接dll目录下的Recruit3XML配置文件。
        /// 传入：1.两个xml，表单数据xml和表单模板xml； 2.路径、图片文件名规则（用Dictionary<string, string>来传入）；
        /// 这样就能正常获取显示图片。
        /// \Recruit3XML\DataService.cfg 涉及到样式的配置，目前只有双红线的颜色和宽度设置
        /// <param name="xmlReport">表单数据xml文件</param>
        /// <param name="xmlTemplate">表单模板的配置文件xml</param>
        /// <param name="dicPatient">其他设定信息，如：["UHID"]、 ["姓名"]、["表单名"]、["总页数"] 、["路径"]、文件名规则 ["图片名"] </param> 
        /// <returns>当前表单的总页数（图片张数,生成所有页的时候）</returns>
        private int GetReportImage(XmlDocument xmlReport, XmlDocument xmlTemplate, Dictionary<string, string> dicPatient, int startPage, int endPage, Action<Image> actionImage = null)
        {
            int pageCount = 0;
            string picName = "";

            try
            {
                Comm._isCreatReportImages = true;
                this.Cursor = Cursors.WaitCursor;
                if (_DSS == null)
                {
                    _DSS = new DataServiceSetting(RunMode);
                }
                //参数设置：
                if (xmlReport != null) //为null的时候表示是暗门调用，不用赋值
                {
                    _RecruitXML = xmlReport;
                }

                if (xmlTemplate != null)
                {
                    _TemplateXml = xmlTemplate;
                }

                if (dicPatient != null)
                {
                    //设置当前病人uhid
                    //this._patientInfo.PatientId = dicPatient["UHID"];
                    //this._patientInfo.PatientName = dicPatient["姓名"];
                    _TemplateName = dicPatient["表单名"];

                    //_drawTitleHospitalName = dicPatient["医院名"];

                    //设置生成图片的路径
                    _pathReport = dicPatient["路径"];

                    picName = dicPatient["图片名"];

                    //设置临时病人信息列表
                    _PatientsInforDT = patientsListView.GetInitDT(this._patientInfo); //初始化列
                    //_PatientsInforDT.Rows.Add();
                    //_PatientsInforDT.Rows[0]["UHID"] = dicPatient["UHID"];
                    _PatientsInforDT.Rows[0]["表单名"] = dicPatient["表单名"];
                    _PatientsInforDT.Rows[0]["姓名"] = dicPatient["姓名"];
                    _PatientsInforDT.Rows[0]["总页数"] = dicPatient["总页数"];

                    //暂时不传入，认为表单数据文件中已经有了各页的数据信息：
                    # region 暂时不要的，但可扩展的信息
                    if (dicPatient.ContainsKey("床号"))
                    {
                        _PatientsInforDT.Rows[0]["床号"] = dicPatient["床号"];
                    }
                    else
                    {
                        _PatientsInforDT.Rows[0]["床号"] = "";
                    }

                    if (dicPatient.ContainsKey("性别"))
                    {
                        _PatientsInforDT.Rows[0]["性别"] = dicPatient["性别"];
                    }
                    else
                    {
                        _PatientsInforDT.Rows[0]["性别"] = "";
                    }

                    if (dicPatient.ContainsKey("年龄"))
                    {
                        _PatientsInforDT.Rows[0]["年龄"] = dicPatient["年龄"];
                    }
                    else
                    {
                        _PatientsInforDT.Rows[0]["年龄"] = "";
                    }

                    if (dicPatient.ContainsKey("科别"))
                    {
                        _PatientsInforDT.Rows[0]["科别"] = dicPatient["科别"];
                    }
                    else
                    {
                        _PatientsInforDT.Rows[0]["科别"] = "";
                    }

                    if (dicPatient.ContainsKey("住院号"))
                    {
                        _PatientsInforDT.Rows[0]["住院号"] = dicPatient["住院号"];
                    }
                    else
                    {
                        _PatientsInforDT.Rows[0]["住院号"] = "";
                    }

                    if (dicPatient.ContainsKey("病区"))
                    {
                        _PatientsInforDT.Rows[0]["病区"] = dicPatient["病区"];
                    }
                    else
                    {
                        _PatientsInforDT.Rows[0]["病区"] = "";
                    }

                    if (dicPatient.ContainsKey("入区日期"))
                    {
                        _PatientsInforDT.Rows[0]["入区日期"] = dicPatient["入区日期"];
                    }
                    else
                    {
                        _PatientsInforDT.Rows[0]["入区日期"] = "";
                    }

                    if (dicPatient.ContainsKey("入院日期"))
                    {
                        _PatientsInforDT.Rows[0]["入院日期"] = dicPatient["入院日期"];
                    }
                    else
                    {
                        _PatientsInforDT.Rows[0]["入院日期"] = "";
                    }

                    # endregion 暂时不要的，但可扩展的信息

                    //页数
                    if (!int.TryParse(dicPatient["总页数"], out pageCount))
                    {
                        //pageCount没有传入的时候，或者传入错误的时候
                        //根据数据和模板来计算页数
                        try
                        {
                            //int pages = _RecruitXML.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form))).Count; //页数
                            //XmlNode xn = _TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form), nameof(EntXmlModel.Table)));
                            //if (xn != null)
                            //{
                            //    int records = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))).ChildNodes.Count; // 行数
                            //    pageCount = TableClass.GetPageCount_HaveTable(_TemplateXml, records);
                            //}
                            //else
                            //{
                            //    //不存在表格的时候，就以页数据的节点数来作为总页数
                            //    pageCount = pages;
                            //}
                            int a = _RecruitXML.SelectNodes(EntXmlModel.GetName("/", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form))).Count;              // 非表格的页数
                            int b = _RecruitXML.SelectSingleNode(EntXmlModel.GetName("/", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))).ChildNodes.Count; // 表格的行数

                            XmlNode xn = _TemplateXml.SelectSingleNode(EntXmlModel.GetName("/", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form), nameof(EntXmlModel.Table)));
                            if (xn != null)
                            {
                                pageCount = TableClass.GetPageCount_HaveTable(_TemplateXml, b);
                            }
                            else
                            {
                                pageCount = a;
                            }
                            if (pageCount == 0) pageCount = 1;
                            _MaxPageForPrintAll = pageCount;
                        }
                        catch (Exception ex)
                        {
                            Comm.LogHelp.WriteErr("调用接口时，没有传入总页数；根据模板和数据计算页数时出现异常，一般为模板配置错误：FormRows。");
                            throw ex;
                        }
                    }

                    if (pageCount == 0)
                    {
                        Comm.LogHelp.WriteErr("传入参数【总页数】为：0，或者没有传入，无法生成图片。");
                        Comm._isCreatReportImages = false;
                        this.Cursor = Cursors.Default;
                        return 0;
                    }

                }
                else
                {
                    //暗门调用的时候：
                    if (_RecruitXML == null || _CurrentPage == "")
                    {
                        ShowInforMsg("请先打开表单，才可以利用暗门生成所有页图片。");
                        Comm._isCreatReportImages = false;
                        this.Cursor = Cursors.Default;
                        return 0;
                    }

                    if (this._patientInfo != null)
                    {
                        pageCount = this.uc_Pager1.NMax;
                    }
                    else
                    {
                        MessageBox.Show("请先选打开指定的表单，再使用暗门生成所有页图片。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Comm._isCreatReportImages = false;
                        this.Cursor = Cursors.Default;
                        return -1;
                    }

                    _MaxPageForPrintAll = pageCount;

                    picName = this._patientInfo.PatientId + _TemplateName + @".{0}.jpg";

                    FolderBrowserDialog myDialog = new FolderBrowserDialog();
                    myDialog.ShowNewFolderButton = true;

                    myDialog.Description = @"请先选择图片保存的目录：";

                    if (myDialog.ShowDialog() == DialogResult.OK)
                    {
                        _pathReport = myDialog.SelectedPath;

                    }
                    else
                    {
                        Comm._isCreatReportImages = false;
                        this.Cursor = Cursors.Default;
                        return -1;
                    }
                }

                //防止，区域调用的时候，为空，其他处理转int会报错
                if (_CurrentPage == "")
                {
                    _CurrentPage = "1";
                }

                //生成所有页的时候，外部方法不传入起始页数，表单内部构造方法传入-9999
                if (startPage == -9999 && endPage == -9999)
                {
                    startPage = 1;
                    endPage = pageCount;
                }

                if (startPage < 1 || endPage < 1 || startPage > pageCount || startPage > pageCount)
                {
                    Comm._isCreatReportImages = false;
                    this.Cursor = Cursors.Default;
                    Comm.LogHelp.WriteErr("指定的起始页数必须为大于0的整数，并且小于等于当前表单最大页数。");
                    return -1;
                }

                if (ReportPrint) //区域调用生成图片，比如pdf后，还有可能要打印的。如果用图片打印就不清楚了。
                {
                    //区域打印：

                    printAll_PageIndex = startPage;
                    _startPage = startPage;     //设置起始页
                    _endPage = endPage;

                    ReportPrintDocument = new PrintDocument();
                    ReportPrintDocument.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument3_PrintPage);

                    //外部调用的时候，通过绑定打印文档来调用打印所有页：其余的打印窗口、打印纸张、打印机等在调用方自己根据情况来设置
                    //1.将变量ReportPrint设置为True；
                    //2.依然调用GetReportImage(…… 方法；
                    //3.调用方自己使用PrintPreviewDialog，.Document设置为ReportPrintDocument
                    //4.打印完成后，调用ClearReport();清理处理。

                }
                else
                {
                    //绘制表单所有页
                    DrawImageAllPages(picName, pageCount, startPage, endPage, actionImage);

                    Comm._isCreatReportImages = false;

                    ClearReport();
                }

                ////绘制表单所有页
                //DrawImageAllPages(picName, pageCount, startPage, endPage);
                //_isCreatReportImages = false;
                //ClearReport();


                this.Cursor = Cursors.Default;

                //Comm.Logger.WriteInformation("成功生成【" + this._patientInfo.Id + " | " + this._patientInfo.Name + " | " + _TemplateName + "】表单图片，共：" + pageCount.ToString() + "张。");
            }
            catch (Exception ex)
            {
                Comm._isCreatReportImages = false;
                this.Cursor = Cursors.Default;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }

            return pageCount;
        }

        /// <summary>
        /// 生成所有页图片的时候，绘制每页图片
        /// </summary>
        /// <param name="picName">文件名规则名</param>
        /// <param name="pageCount">总页数</param>
        private void DrawImageAllPages(string picName, int pageCount, int startPage, int endPage, Action<Image> actionImage = null)
        {
            XmlNode nodePageCurrent; //备份好节点页，防止打印后产生干扰，下面会对_node_Page重指定，进行加载某页模板的
            if (_node_Page != null)
            {
                nodePageCurrent = _node_Page.Clone();
            }
            else
            {
                nodePageCurrent = null;
            }

            try
            {

                //初始化：
                //表单大小读取、设定 //<NurseForm BackColor="Red" Size="700,1000">
                SizeF chartSize = Comm.getSize((_TemplateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm)) as XmlElement).GetAttribute(nameof(EntXmlModel.Size)));

                //生成图片
                Image ni;
                Graphics g = null;

                //关键质量控制 
                //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff 
                ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo ici = null;
                foreach (ImageCodecInfo i in icis)
                {
                    if (
                        //i.MimeType == "image/jpeg" ||
                        i.MimeType == "image/bmp" //||
                                                  //i.MimeType == "image/png" //||
                                                  //i.MimeType == "image/gif"
                        )
                    {
                        ici = i;
                        break;
                    }
                }

                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);

                //for (int i = 1; i <= pageCount; i++)      //原来的生成所有页
                for (int i = startPage; i <= endPage; i++)  //可以生成指定页的范围。
                {
                    printAll_PageIndex = i;

                    ni = new Bitmap((int)chartSize.Width, (int)chartSize.Height);
                    g = Graphics.FromImage(ni);

                    g.Clear(System.Drawing.Color.White);// 白色背景色

                    g.SmoothingMode = SmoothingMode.HighQuality;//消除锯齿，平滑
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality; //高像素偏移质量   
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    //模板文件：会做备份的，防止和界面上的混乱
                    _node_Page = GetTemplatePageNode(_TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design))), i);

                    //数据节点："NurseForm/Forms/Form[@SN='" + i.ToString() + "']"
                    _PrintAll_PageNode = _RecruitXML.SelectSingleNode("//" + EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), i.ToString(), "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));

                    //页号手动修改处理
                    SetPageNoStartForPrintAll();
                    //页号手动修改处理

                    //载入表单模板
                    DrawEditorChart(g, false, true, true); //非表格项目，这里就取值后打印(根据上面的数据节点_PrintAll_PageNode)

                    //添加的图片要再遍历本页数据进行绘制；因为根据模板是无法知道有没有添加的图片的
                    Bitmap bmp;
                    string[] arr;
                    Point p;
                    Size sz;
                    if (_PrintAll_PageNode != null)
                    {
                        foreach (XmlAttribute xe in _PrintAll_PageNode.Attributes)
                        {
                            if (xe.Name.StartsWith(nameof(EntXmlModel.IMAGE)) && !string.IsNullOrEmpty(xe.Value))
                            {
                                //是图片:属性并是名字 ¤ 坐标¤二进制
                                arr = xe.Value.Split('¤');

                                sz = StringNumber.getSize(arr[2]);
                                bmp = new Bitmap(sz.Width, sz.Height);

                                bmp = (Bitmap)FileHelp.ConvertStringToImage(arr[4]);
                                p = StringNumber.getSplitValue(arr[1]); //位置

                                g.DrawImage(bmp, p.X, p.Y, sz.Width, sz.Height);

                                //图片是否需要边框线
                                if (arr[3] == "")
                                {
                                    //g.
                                }
                                else
                                {
                                    g.DrawRectangle(new Pen(Color.Black), p.X, p.Y, sz.Width, sz.Height);
                                }
                            }
                        }
                    }

                    //载入本页数据
                    LoadTable(_PrintAll_TableInfor, true); //打印所有页的时候，另作控制，给打印所有页用的_TableInfor赋值

                    //绘制表格的基础线
                    if (_TableType && _PrintAll_TableInfor != null) // 根据解析本页的数据，形成本页的表格，进行绘制。这里还是默认的表格线。
                    {
                        //绘制表格的基础线
                        DrawTableBaseLines(g, _PrintAll_TableInfor);

                        //绘制表格数据和线
                        DrawTableData_Lines(g, true, _PrintAll_TableInfor, i.ToString());
                    }

                    //如果已经存在，那么先删除
                    if (File.Exists(_pathReport + @"\" + string.Format(picName, (i - 1).ToString())))
                    {
                        File.Delete(_pathReport + @"\" + string.Format(picName, (i - 1).ToString()));
                    }

                    if (actionImage == null)
                    {
                        ni.Save(_pathReport + @"\" + string.Format(picName, (i - 1).ToString()), ici, ep);

                    }
                    else
                    {
                        //Image aaa = null;
                        //using (MemoryStream ms = new MemoryStream())
                        //{
                        //    ni.Save(ms, ImageFormat.Jpeg);
                        //    Image.FromStream(ms);
                        //};
                        actionImage(ni);
                    }

                    if (_PrintAll_TableInfor != null)
                    {
                        _PrintAll_TableInfor.Dispose();
                        _PrintAll_TableInfor = null;
                    }

                    //if (_TableInfor != null)
                    //{
                    //    _TableInfor.Dispose();
                    //    _TableInfor = null;
                    //    //_PrintAll_TableInfor = null;
                    //}

                    //if (_PrintAll_RichTextBoxExtended != null && !_PrintAll_RichTextBoxExtended.IsDisposed)
                    //{
                    //    _PrintAll_RichTextBoxExtended.Dispose();
                    //}

                    //if (_PrintAll_TransparentRTBExtended != null && !_PrintAll_TransparentRTBExtended.IsDisposed)
                    //{
                    //    _PrintAll_TransparentRTBExtended.Dispose();
                    //}
                }

                _node_Page = nodePageCurrent;
            }
            catch (Exception ex)
            {
                _node_Page = nodePageCurrent;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }
        #endregion 生成所有页图片

        #region 取得字体更新到工具栏

        /// <summary>
        /// 将选择文字的字体类型更新到工具栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateToolBarFontStyle_SelectionChanged(object sender, EventArgs e)
        {
            if (_IsLoading)
            {
                return; //加载的时候不用触发
            }

            _IsLoading = true;


            _IsLoading = false;
        }

        //Used for looping
        private RichTextBox _rtbTempForFont = new RichTextBox();
        /// <summary>
        /// 取得选择文字的字体信息
        /// </summary>		
        /// 
        private Font GetFontDetails(RichTextBoxExtended rtbe)
        {
            //This method should handle cases that occur when multiple fonts/styles are selected

            int rtb1start = rtbe.SelectionStart;
            int len = rtbe.SelectionLength;
            int rtbTempStart = 0;

            if (len <= 1)
            {
                // Return the selection or default font
                if (rtbe.SelectionFont != null)
                    return rtbe.SelectionFont;
                else
                    return rtbe.Font;
            }

            // Step through the selected text one char at a time	
            // after setting defaults from first char
            _rtbTempForFont.Rtf = rtbe.SelectedRtf;

            //Turn everything on so we can turn it off one by one
            FontStyle replystyle =
                FontStyle.Bold | FontStyle.Italic | FontStyle.Strikeout | FontStyle.Underline;

            // Set reply font, size and style to that of first char in selection.
            _rtbTempForFont.Select(rtbTempStart, 1);
            string replyfont = _rtbTempForFont.SelectionFont.Name;
            float replyfontsize = _rtbTempForFont.SelectionFont.Size;
            replystyle = replystyle & _rtbTempForFont.SelectionFont.Style;

            // Search the rest of the selection
            for (int i = 1; i < len; ++i)
            {
                _rtbTempForFont.Select(rtbTempStart + i, 1);

                // Check reply for different style
                replystyle = replystyle & _rtbTempForFont.SelectionFont.Style;

                // Check font
                if (replyfont != _rtbTempForFont.SelectionFont.FontFamily.Name)
                    replyfont = "";

                // Check font size
                if (replyfontsize != _rtbTempForFont.SelectionFont.Size)
                    replyfontsize = (float)0.0;
            }

            // Now set font and size if more than one font or font size was selected
            if (replyfont == "")
                replyfont = _rtbTempForFont.Font.FontFamily.Name;

            if (replyfontsize == 0.0)
                replyfontsize = _rtbTempForFont.Font.Size;

            // generate reply font
            Font reply
                = new Font(replyfont, replyfontsize, replystyle);

            return reply;
        }
        #endregion

        #region 改变字体样式 Change font style
        /// <summary>
        ///     Change the richtextbox style for the current selection
        /// </summary>
        private void ChangeFontStyle(FontStyle style, bool add)
        {
            RichTextBox rtb1 = (RichTextBoxExtended)_Need_Assistant_Control;

            HorizontalAlignment temp = rtb1.SelectionAlignment;


            // throw error if style isn't: bold, italic, strikeout or underline
            if (style != FontStyle.Bold
                && style != FontStyle.Italic
                && style != FontStyle.Strikeout
                && style != FontStyle.Underline)
                throw new System.InvalidProgramException("Invalid style parameter to ChangeFontStyle");

            int rtb1start = rtb1.SelectionStart;
            int len = rtb1.SelectionLength;
            int rtbTempStart = 0;

            if (len <= 1 && rtb1.SelectionFont != null)
            {
                //选中一个文字的时候
                if (add)
                    rtb1.SelectionFont = new Font(rtb1.SelectionFont, rtb1.SelectionFont.Style | style);
                else
                    rtb1.SelectionFont = new Font(rtb1.SelectionFont, rtb1.SelectionFont.Style & ~style);


                rtb1.Select(rtb1start, len);//恢复选中
                return;
            }

            // Step through the selected text one char at a time	
            _rtbTempForFont.Rtf = rtb1.SelectedRtf;
            _rtbTempForFont.SelectionAlignment = temp;
            for (int i = 0; i < len; ++i)
            {
                _rtbTempForFont.Select(rtbTempStart + i, 1);

                //add or remove style 
                if (add)
                    _rtbTempForFont.SelectionFont = new Font(_rtbTempForFont.SelectionFont, _rtbTempForFont.SelectionFont.Style | style);
                else
                    _rtbTempForFont.SelectionFont = new Font(_rtbTempForFont.SelectionFont, _rtbTempForFont.SelectionFont.Style & ~style);
            }

            _rtbTempForFont.Select(rtbTempStart, len);
            _rtbTempForFont.SelectionAlignment = temp;

            rtb1.SelectedRtf = _rtbTempForFont.SelectedRtf;
            if (rtb1.SelectionAlignment != temp)
            {
                rtb1.SelectionAlignment = temp;  //不然会默认居左的，要保留之前的对齐方式
            }
            rtb1.Select(rtb1start, len);



            return;
        }
        #endregion

        # region 合计

        /// <summary>
        /// 根据时间，自动调整行位置
        /// 必须含有：日期、时间列，并且没有调整段落列的表格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rtbe_AutoPositionByTimeMainForm(object sender, EventArgs e)
        {
            try
            {
                //记录下当下行位置
                Point currentCell = _CurrentCellIndex;
                //CellInfor cellYMD = _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "日期");
                //CellInfor cellHM = _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间");

                SaveToOp();

                string nodeId = _TableInfor.Rows[currentCell.X].ID.ToString();
                XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));  //当前的所有数据
                XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId, "=", nameof(EntXmlModel.Record))); //当前行，在xml数据中的节点
                DateTime currentRowDatetime = GetServerDate();

                string dateFormat = (cellNode as XmlElement).GetAttribute("日期").Split('¤')[0];
                string hm = (cellNode as XmlElement).GetAttribute("时间").Split('¤')[0];

                // 护理部要求写入24:00
                hm = hm == "24:00" ? "23:59" : hm;
                string thisRowTimeStr = "";


                if (dateFormat != "" && hm != "")
                {
                    //选择行的日期和时间要为非空才能判断
                    if (DateTime.TryParse(dateFormat + " " + hm, out currentRowDatetime))
                    {
                        //1.移动当前节点                           
                        DateTime nodeDateTime = GetServerDate();
                        bool Isfind = false;

                        //根据合计条件和表单数据进行合计
                        if (recordsNode != null && recordsNode.ChildNodes.Count > 0)
                        {
                            XmlNode xn = null;

                            //从当前行所在的节点，往前推
                            xn = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId, "=", nameof(EntXmlModel.Record)));

                            while (xn != null)
                            {
                                xn = xn.PreviousSibling; //去上一个节点
                                if (xn != null)
                                {
                                    //护理部要求写入24:00
                                    thisRowTimeStr = (xn as XmlElement).GetAttribute("时间").Split('¤')[0] == "24:00" ? "23:59" : (xn as XmlElement).GetAttribute("时间").Split('¤')[0];

                                    if (DateTime.TryParse((xn as XmlElement).GetAttribute("日期").Split('¤')[0] + " " + thisRowTimeStr, out nodeDateTime))
                                    {
                                        if (currentRowDatetime.CompareTo(nodeDateTime) >= 0)
                                        {
                                            Isfind = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            //如果：找到的节点是当前节点的上一个，那么还是当前页，当前行，那么不需要进行调整
                            if (xn == cellNode.PreviousSibling)
                            {
                                ShowInforMsg("当前行不需要进行调整。");
                                return;
                            }

                            if (!Isfind)
                            {
                                //移动到第一行
                                xn = null;
                            }


                            //XmlNode newNode = cellNode.Clone();

                            recordsNode.InsertAfter(cellNode.Clone(), xn);  //移动节点，如果xn是null那么就插入到最前面，成为第一个节点了
                            recordsNode.RemoveChild(cellNode);              //删除原来的节点


                            //2.获取到移动后位置的页数，重新打开

                            int indexAutoPosition = recordsNode.ChildNodes.Count - 1;
                            //2.1得到移动后，该行对应节点在xml中的索引位置，以便计算所在页数和行数
                            for (int i = recordsNode.ChildNodes.Count - 1; i >= 0; i--)
                            {
                                if ((recordsNode.ChildNodes[i] as XmlElement).GetAttribute(nameof(EntXmlModel.ID)) == nodeId)
                                {
                                    indexAutoPosition = i;
                                    break;
                                }
                            }

                            int rowCount = 0;
                            int pageCount = TableClass.GetPageCount_HaveTable(_TemplateXml, indexAutoPosition + 1, ref rowCount); //移动后节点所在的页数

                            if (pageCount >= int.Parse(_CurrentPage))
                            {
                                //如果还是当前页，当前行，那么不需要进行调整
                                if (pageCount == int.Parse(_CurrentPage) && rowCount - 1 == currentCell.X)
                                {
                                    ShowInforMsg("当前行不需要进行调整。");
                                    return;
                                }


                                //当前页，重新载入表格，并重绘，选中新位置的行
                                ReSetTableInfor();

                                LoadTable(_TableInfor, false);

                                _IsLoading = true;
                                Redraw();
                                _IsLoading = false;

                                ShowCellRTBE(rowCount - 1, currentCell.Y);
                            }
                            else
                            {
                                //不在当前页，打开那一页，并选中那一行
                                for (int j = 0; j < _CurrentTemplateNameTreeNode.Nodes.Count; j++)
                                {
                                    if (pageCount.ToString() == _CurrentTemplateNameTreeNode.Nodes[j].Text)
                                    {
                                        this.treeViewPatients.SelectedNode = _CurrentTemplateNameTreeNode.Nodes[j];

                                        _NotNeedCheckForSave = true; //切换过程中不需要提示保存等

                                        treeViewPatients_NodeMouseDoubleClick(_CurrentTemplateNameTreeNode.Nodes[j], null);

                                        _NotNeedCheckForSave = false;

                                        //选中移动后所在表格中的行
                                        ShowCellRTBE(rowCount - 1, currentCell.Y);
                                        break;
                                    }
                                }
                            }

                            IsNeedSave = true;
                        }
                    }
                    else
                    {
                        ShowInforMsg("日期时间格式不正确，不能自动调整行位置！");
                    }
                }
                else
                {
                    ShowInforMsg("日期或时间为空，不能自动调整行位置！");
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        # region 小结总结
        /// <summary>
        /// 载入总结小结的配置文件
        /// 途径分组设定
        /// </summary>
        private void LoadXmlDocSumGroup()
        {
            string errMsg = "找不到小结和总结的配置文件：SumGroupSetting.xml";

            if (RunMode)
            {
                _xmlDocSumGroup = new XmlDocument();

                string xmlText = "";
                //GlobalVariable._serviceLava.ReadOutXmlFile(@"\Nurse\SumGroupSetting.xml", ref xmlText);

                if (!string.IsNullOrEmpty(xmlText))
                {
                    _xmlDocSumGroup.InnerXml = xmlText;
                }
                else
                {
                    _xmlDocSumGroup = null;
                    ShowInforMsg(errMsg, true);
                }
            }
            else
            {
                _xmlDocSumGroup = new XmlDocument();

                if (File.Exists(Comm._Nurse_ConfigPath + @"\SumGroupSetting.xml"))
                {
                    _xmlDocSumGroup.Load(Comm._Nurse_ConfigPath + @"\SumGroupSetting.xml");
                }
                else
                {
                    _xmlDocSumGroup = null;
                    ShowInforMsg(errMsg, true);
                }
            }
        }

        /// <summary>
        /// 根据输入行的名称，得到所属的类型名称(所属分组)
        /// </summary>
        /// <returns></returns>
        private string GetGroupTypeName(string tuJin, ref string sort)
        {
            try
            {
                //<NurseFormSetting>
                //<!-- 
                //类型节点的Name：小结/总结类型的名称
                //Sort：为总结的类型行的上下的显示顺序
                //途径名称的Name：途径名称
                //-->
                //  <类型 Name="输入" Sort="A" Notes="">
                //     <途径名称 Name="iH" Notes=""/>
                //     <途径名称 Name="im" Notes=""/>
                //     <途径名称 Name="iv" Notes=""/>
                //  </类型>

                string ret = tuJin; //""如果分组中找不到,那么就以途径本身作为类型名
                sort = "";

                if (_xmlDocSumGroup != null)
                {
                    XmlNode xn = _xmlDocSumGroup.SelectSingleNode("NurseFormSetting/类型/途径名称[@Name='" + tuJin + "']");

                    if (xn != null)
                    {
                        ret = (xn.ParentNode as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                        sort = (xn.ParentNode as XmlElement).GetAttribute(nameof(EntXmlModel.Sort));
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        #region 只能处理入量名称回行的获取途径的方式
        /// <summary>
        /// 一个时间点，可能输入一个途径一个入量，也可能一个途径两个入量，也可能两个路径两个入量。
        /// 而且，虽然限制路径不回行，但是名称是可能回行的，因为文字会很多个字数。
        /// 
        /// 所以，如果本行的入量不为空的时候，网上遍历，去所属时间相同的行，第一个途径非空的作为本行的途径。
        /// </summary>
        /// <returns></returns>
        private string GetTuJin(XmlNode recordsNode, XmlNode xnThis, string inputTypeColumn, string inputValueColumn)
        {
            //if (string.IsNullOrEmpty(tujin) && !string.IsNullOrEmpty(eachValue分组合计的某一项值))

            string ret = "";

            string recordDt = GetRowDateTime((xnThis as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));
            string recordDtPre = null;

            while (xnThis != null)
            {
                xnThis = xnThis.PreviousSibling; //取上一个节点

                if (xnThis != null)
                {
                    recordDtPre = GetRowDateTime((xnThis as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                    if (recordDtPre == recordDt)
                    {
                        if ((xnThis as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) != "")
                        {
                            break; //continue; //遇到合计行，多行的名称肯定结束了。不要把上一次小结的文字又作为了途径名。当然上一次小结行的算到的时间也是小结之前的日期时间。也一般不会不在范围内了。除非小结后总结。
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty((xnThis as XmlElement).GetAttribute(inputTypeColumn).Split('¤')[0].Trim()))
                            {
                                //获取当前节点中的途径的文字内容
                                ret = (xnThis as XmlElement).GetAttribute(inputTypeColumn).Split('¤')[0].Trim();
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        //行所属的时间点不一样了，那就要跳出了。还是没有统计到的途径，就是书写不规范了。
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return ret;
        }
        #endregion 只能处理入量名称回行的获取途径的方式

        #region 途径多行的处理：合计前合并，合计后再拆分到行
        /// <summary>
        /// 一个时间点，可能输入一个途径一个入量，也可能一个途径两个入量，也可能两个路径两个入量。
        /// 而且，虽然限制路径不回行，但是名称是可能回行的，因为文字会很多个字数。
        /// 
        /// 所以，如果本行的入量不为空的时候，网上遍历，去所属时间相同的行，第一个途径非空的作为本行的途径
        /// 
        /// 
        /// 如果书写不规范，填写了途径但是乜有写数字；那么按照现在的规则就会变成根据相邻行的途径名字拼接，影响相邻行的分组合计了。
        /// </summary>
        /// <returns></returns>
        private string GetTuJinTop(XmlNode recordsNode, XmlNode xnThis, string inputTypeColumn, string inputValueColumn)
        {

            #region 过期注释，因为复杂，过程中的理解
            //if (string.IsNullOrEmpty(tujin) && !string.IsNullOrEmpty(eachValue分组合计的某一项值))
            //_TuJinMultilineTop

            //如果数值不为空，那么往下找，合并得到多行的途径名
            //如果数值为空，那么

            //如果途径有内容，但是么有数值，那么就表示途径回行了。
            //多行空途径，如果数值为空，那么给他赋值唯一号，不然会被唯一名，然后同名分组合计，就会合并成一行到dt中没有了。也不能赋空，那么分组合计后肯定也没有了。
            //在得到的分组合计后的dt中还是需要将这个多文字的途径按照顺序多行显示的啊。而且，也不能排序，如果有xml的再次分组配置，将sort设置为空。
            //if ((xnThis as XmlElement).GetAttribute(inputValueColumn).Trim() == "") // && (xnThis as XmlElement).GetAttribute(inputTypeColumn).Trim() != "" 有可能入量名称的行数更多，那么途径可能是为空的
            //{
            //    return "去零" + Guid.NewGuid().ToString(); //但是这样分组合计后，数值会为0。不能guid，不然多行的途径，就不能实现分组合计咯
            //    //应该为完整的途径名称+本行的途径名称。这样即使中间由于名称多一行产生的空行也能被合并掉。
            //    //并且防止灌肠后大便的【大便】和大便混起来。
            //}
            #endregion 过期注释，因为复杂，过程中的理解

            //---------------------------------------------------------------------------------
            //不该合并合组的不能合并，该有的行，还要有，分组显示几行
            //☆☆★★ 最好的办法还是dt中的途径名称将多行的途径拼接起来，然后写入表单的时候再利用调整段落列的算法来处理。
            //这样只要在合计之前合并一下，合计后拆分一下即可。
            //---------------------------------------------------------------------------------

            string ret = "";
            string recordDt = GetRowDateTime((xnThis as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));
            string recordDt2 = null;

            //如果数值为空，那么就返回途径为空。因为：要么输入不规范，要么就是有途径回行了。分组合计的时候排除掉
            if ((xnThis as XmlElement).GetAttribute(inputValueColumn).Split('¤')[0].Trim() == "")
            {
                return "";
            }
            else
            {
                //从当前节点往下遍历
                string lasNodeId = (xnThis as XmlElement).GetAttribute(nameof(EntXmlModel.ID));
                XmlNode xnLast = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), lasNodeId, "=", nameof(EntXmlModel.Record))); //防止引用类型，改变节点

                ret = (xnLast as XmlElement).GetAttribute(inputTypeColumn).Split('¤')[0].Trim();
                xnLast = xnLast.NextSibling; //取下一个节点

                while (xnLast != null)
                {
                    recordDt2 = GetRowDateTime((xnLast as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                    if (!Comm.isEmptyRecord(xnLast) && recordDt2 == recordDt && (xnLast as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) == "" && (xnLast as XmlElement).GetAttribute(inputValueColumn).Split('¤')[0].Trim() == "")
                    {
                        ret = ret + (xnLast as XmlElement).GetAttribute(inputTypeColumn).Split('¤')[0].Trim();

                        xnLast = xnLast.NextSibling; //取下一个节点
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 如果数值是写在一个多行途径的最后一行，而且也可能有空行。因为其名称的行数更多。
        /// </summary>
        /// <param name="recordsNode"></param>
        /// <param name="xnThis"></param>
        /// <param name="inputTypeColumn"></param>
        /// <param name="inputValueColumn"></param>
        /// <returns></returns>
        private string GetTuJinEnd(XmlNode recordsNode, XmlNode xnThis, string inputTypeColumn, string inputValueColumn)
        {
            string ret = "";
            string recordDt = GetRowDateTime((xnThis as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));
            string recordDt2 = null;

            //如果数值为空，那么就返回途径为空。因为：要么输入不规范，要么就是有途径回行了。分组合计的时候排除掉
            if ((xnThis as XmlElement).GetAttribute(inputValueColumn).Split('¤')[0].Trim() == "")
            {
                return "";
            }
            else
            {
                //从当前节点往下遍历
                string lasNodeId = (xnThis as XmlElement).GetAttribute(nameof(EntXmlModel.ID));
                XmlNode xnLast = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), lasNodeId, "=", nameof(EntXmlModel.Record))); //防止引用类型，改变节点

                ret = (xnLast as XmlElement).GetAttribute(inputTypeColumn).Split('¤')[0].Trim();
                xnLast = xnLast.PreviousSibling; //取上一个节点

                while (xnLast != null)
                {
                    recordDt2 = GetRowDateTime((xnLast as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                    //有可能为空的，比如：入量名称的行数大于入量途径的行数
                    if (recordDt2 == recordDt && (xnLast as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) == "" && (xnLast as XmlElement).GetAttribute(inputValueColumn).Split('¤')[0].Trim() == "")
                    {
                        ret = (xnLast as XmlElement).GetAttribute(inputTypeColumn).Split('¤')[0].Trim() + ret;

                        xnLast = xnLast.PreviousSibling; //取上一个节点
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 合计后的数据，一个途径是原来的多行合并的，现在需要拆分开。
        /// result outputValueColumn outputTypeColumn 
        /// _TuJinMultilineTop回行类型。
        /// </summary>
        /// <param name="inputTypeColumn"></param>
        /// <param name="inputValueColumn"></param>
        private DataTable GetTuJinDtTop(DataTable result, string inputTypeColumn, string inputValueColumn, string outputValueColumn, string outputTypeColumn)
        {
            //如果outputTypeColumn为空，那么就不需要考虑出量的途径列了
            DataTable dtOut = null;
            CellInfor cellDate = null;
            //_TableInfor.Columns[cellDate.ColIndex].RTBE.Font
            string processStr = null;
            string[] arr = null;
            string inputStr = null;

            //将double类型的列，设置为string。
            //之前没有分组合计，数值是typeof(double)类型的。但是插入行的话，还是都赋值空为好
            //for (int k = 0; k < result.Columns.Count; k++)
            //{
            //    result.Columns[k].DataType = typeof(string);//列有数据后，就不能在更改类型了。
            //}
            DataTable dtString = new DataTable();
            for (int k = 0; k < result.Columns.Count; k++)
            {
                dtString.Columns.Add(result.Columns[k].ColumnName);
            }
            for (int i = 0; i < result.Rows.Count; i++)
            {
                dtString.Rows.Add();

                for (int k = 0; k < result.Columns.Count; k++)
                {
                    dtString.Rows[i][k] = result.Rows[i][k].ToString();
                }
            }
            result = dtString;


            if (!string.IsNullOrEmpty(outputValueColumn) && !string.IsNullOrEmpty(outputTypeColumn))
            {
                //存在出量分组的时候
                dtOut = new DataTable();
                dtOut.Columns.Add(outputValueColumn);
                dtOut.Columns.Add(outputTypeColumn);

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    //如果两者都为空，表示这一行是出量分组的空行，一般是入量的行数多，在上次合并dt的时候的空行。就不要那古来了
                    if ((result.Rows[i][outputValueColumn] != null && !string.IsNullOrEmpty(result.Rows[i][outputValueColumn].ToString()))
                        || (result.Rows[i][outputTypeColumn] != null && !string.IsNullOrEmpty(result.Rows[i][outputTypeColumn].ToString())))
                    {
                        dtOut.Rows.Add();
                        dtOut.Rows[dtOut.Rows.Count - 1][outputValueColumn] = result.Rows[i][outputValueColumn].ToString();
                        dtOut.Rows[dtOut.Rows.Count - 1][outputTypeColumn] = result.Rows[i][outputTypeColumn].ToString();
                    }
                }

                //出量途径分行
                //入量途径分行
                cellDate = _TableInfor.CellByRowNo_ColName(0, outputTypeColumn);
                processStr = null;
                arr = null;
                inputStr = null;
                //object tempSum = "";
                for (int i = 0; i < dtOut.Rows.Count; i++)
                {
                    inputStr = dtOut.Rows[i][outputTypeColumn].ToString();

                    if (!string.IsNullOrEmpty(inputStr))
                    {
                        processStr = WordWrap.lineStringAnalys(inputStr, _TableInfor.Columns[cellDate.ColIndex].RTBE.Width + _TableInfor.Columns[cellDate.ColIndex].RTBE.Margin.Right, _TableInfor.Columns[cellDate.ColIndex].RTBE.Font, "");
                        arr = processStr.Split(new string[] { "¤" }, StringSplitOptions.RemoveEmptyEntries);

                        if (arr.Length > 1)
                        {
                            //增加行，并将内容分别写入
                            dtOut.Rows[i][outputTypeColumn] = arr[0];
                            //tempSum = dtOut.Rows[i]["入量"]; //如果写在第一行，那么不需要移动

                            for (int j = 1; j < arr.Length; j++)
                            {
                                DataRow drNew = dtOut.NewRow();
                                for (int k = 0; k < dtOut.Columns.Count; k++)
                                {
                                    //if(result.Columns[k].DataType.FullName == "string"
                                    drNew[k] = "";
                                }

                                drNew[outputTypeColumn] = arr[j];

                                dtOut.Rows.InsertAt(drNew, i + 1);

                            }
                        }
                    }
                }
            }

            //将主dt中的出量分组的两列清空内容。
            for (int i = 0; i < result.Rows.Count; i++)
            {
                result.Rows[i][outputValueColumn] = "";
                result.Rows[i][outputTypeColumn] = "";
            }

            //将主dt中的出量分组的两列清空内容后，最后的空行要删除
            bool isEmptyRecord = true;
            for (int i = result.Rows.Count - 1; i >= 0; i--)
            {
                isEmptyRecord = true;

                for (int j = 0; j < result.Columns.Count; j++)
                {
                    if (result.Rows[i][j] != null && !string.IsNullOrEmpty(result.Rows[i][j].ToString()))
                    {
                        //有一个不为空，那么这条数据就不是空。
                        isEmptyRecord = false;
                        break;
                    }

                }

                if (isEmptyRecord)
                {
                    result.Rows.RemoveAt(i);
                }
                else
                {
                    break;//如果末尾有空行删除的话，那么遇到第一个非空行也就可以退出了。
                }
            }

            //入量途径分行
            cellDate = _TableInfor.CellByRowNo_ColName(0, inputTypeColumn);
            processStr = null;
            arr = null;
            inputStr = null;
            //object tempSum = "";
            for (int i = 0; i < result.Rows.Count; i++)
            {
                inputStr = result.Rows[i]["类型"].ToString();

                if (!string.IsNullOrEmpty(inputStr))
                {
                    processStr = WordWrap.lineStringAnalys(inputStr, _TableInfor.Columns[cellDate.ColIndex].RTBE.Width + _TableInfor.Columns[cellDate.ColIndex].RTBE.Margin.Right, _TableInfor.Columns[cellDate.ColIndex].RTBE.Font, "");
                    arr = processStr.Split(new string[] { "¤" }, StringSplitOptions.RemoveEmptyEntries);

                    if (arr.Length > 1)
                    {
                        //增加行，并将内容分别写入
                        result.Rows[i]["类型"] = arr[0];
                        //tempSum = result.Rows[i]["入量"]; //如果写在第一行，那么不需要移动

                        for (int j = 1; j < arr.Length; j++)
                        {
                            DataRow drNew = result.NewRow();
                            for (int k = 0; k < result.Columns.Count; k++)
                            {
                                //if(result.Columns[k].DataType.FullName == "string"
                                drNew[k] = "";
                            }

                            drNew["类型"] = arr[j];

                            result.Rows.InsertAt(drNew, i + 1);

                        }
                    }
                }
            }


            //如果入量dt不为null，那么将出量分行后的合并到dt中
            if (dtOut != null)
            {
                int index = 0;
                for (int i = 0; i < dtOut.Rows.Count; i++)
                {
                    if (i >= result.Rows.Count)
                    {
                        result.Rows.Add();
                    }

                    result.Rows[index][outputValueColumn] = dtOut.Rows[i][outputValueColumn].ToString();
                    result.Rows[index][outputTypeColumn] = dtOut.Rows[i][outputTypeColumn].ToString();

                    index++;
                }
            }

            cellDate = null;
            processStr = null;
            arr = null;
            inputStr = null;

            return result;
        }

        private DataTable GetTuJinDtEnd(DataTable result, string inputTypeColumn, string inputValueColumn, string outputValueColumn, string outputTypeColumn)
        {
            //如果outputTypeColumn为空，那么就不需要考虑出量的途径列了
            DataTable dtOut = null;
            CellInfor cellDate = null;
            //_TableInfor.Columns[cellDate.ColIndex].RTBE.Font
            string processStr = null;
            string[] arr = null;
            string inputStr = null;
            object tempSum = null;

            //将double类型的列，设置为string。
            //之前没有分组合计，数值是typeof(double)类型的。但是插入行的话，还是都赋值空为好
            //for (int k = 0; k < result.Columns.Count; k++)
            //{
            //    result.Columns[k].DataType = typeof(string);//列有数据后，就不能在更改类型了。
            //}
            DataTable dtString = new DataTable();
            for (int k = 0; k < result.Columns.Count; k++)
            {
                dtString.Columns.Add(result.Columns[k].ColumnName);
            }
            for (int i = 0; i < result.Rows.Count; i++)
            {
                dtString.Rows.Add();

                for (int k = 0; k < result.Columns.Count; k++)
                {
                    dtString.Rows[i][k] = result.Rows[i][k].ToString();
                }
            }
            result = dtString;


            if (!string.IsNullOrEmpty(outputValueColumn) && !string.IsNullOrEmpty(outputTypeColumn))
            {
                //存在出量分组的时候
                dtOut = new DataTable();
                dtOut.Columns.Add(outputValueColumn);
                dtOut.Columns.Add(outputTypeColumn);

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    //如果两者都为空，表示这一行是出量分组的空行，一般是入量的行数多，在上次合并dt的时候的空行。就不要那古来了
                    if ((result.Rows[i][outputValueColumn] != null && !string.IsNullOrEmpty(result.Rows[i][outputValueColumn].ToString()))
                        || (result.Rows[i][outputTypeColumn] != null && !string.IsNullOrEmpty(result.Rows[i][outputTypeColumn].ToString())))
                    {
                        dtOut.Rows.Add();
                        dtOut.Rows[dtOut.Rows.Count - 1][outputValueColumn] = result.Rows[i][outputValueColumn].ToString();
                        dtOut.Rows[dtOut.Rows.Count - 1][outputTypeColumn] = result.Rows[i][outputTypeColumn].ToString();
                    }
                }

                //出量途径分行
                //入量途径分行
                cellDate = _TableInfor.CellByRowNo_ColName(0, outputTypeColumn);
                processStr = null;
                arr = null;
                inputStr = null;
                tempSum = null;
                for (int i = 0; i < dtOut.Rows.Count; i++)
                {
                    inputStr = dtOut.Rows[i][outputTypeColumn].ToString();

                    if (!string.IsNullOrEmpty(inputStr))
                    {
                        processStr = WordWrap.lineStringAnalys(inputStr, _TableInfor.Columns[cellDate.ColIndex].RTBE.Width + _TableInfor.Columns[cellDate.ColIndex].RTBE.Margin.Right, _TableInfor.Columns[cellDate.ColIndex].RTBE.Font, "");
                        arr = processStr.Split(new string[] { "¤" }, StringSplitOptions.RemoveEmptyEntries);

                        if (arr.Length > 1)
                        {
                            //增加行，并将内容分别写入
                            dtOut.Rows[i][outputTypeColumn] = arr[0];
                            tempSum = dtOut.Rows[i][outputValueColumn]; //如果写在第一行，那么不需要移动
                            dtOut.Rows[i][outputValueColumn] = "";

                            for (int j = 1; j < arr.Length; j++)
                            {
                                DataRow drNew = dtOut.NewRow();
                                for (int k = 0; k < dtOut.Columns.Count; k++)
                                {
                                    //if(result.Columns[k].DataType.FullName == "string"
                                    drNew[k] = "";
                                }

                                if (j == arr.Length - 1)
                                {
                                    drNew[outputValueColumn] = tempSum;
                                }

                                drNew[outputTypeColumn] = arr[j];

                                dtOut.Rows.InsertAt(drNew, i + 1);

                            }
                        }
                    }
                }
            }

            //将主dt中的出量分组的两列清空内容。
            for (int i = 0; i < result.Rows.Count; i++)
            {
                result.Rows[i][outputValueColumn] = "";
                result.Rows[i][outputTypeColumn] = "";
            }

            //将主dt中的出量分组的两列清空内容后，最后的空行要删除
            bool isEmptyRecord = true;
            for (int i = result.Rows.Count - 1; i >= 0; i--)
            {
                isEmptyRecord = true;

                for (int j = 0; j < result.Columns.Count; j++)
                {
                    if (result.Rows[i][j] != null && !string.IsNullOrEmpty(result.Rows[i][j].ToString()))
                    {
                        //有一个不为空，那么这条数据就不是空。
                        isEmptyRecord = false;
                        break;
                    }

                }

                if (isEmptyRecord)
                {
                    result.Rows.RemoveAt(i);
                }
                else
                {
                    break;//如果末尾有空行删除的话，那么遇到第一个非空行也就可以退出了。
                }
            }

            //入量途径分行
            cellDate = _TableInfor.CellByRowNo_ColName(0, inputTypeColumn);
            processStr = null;
            arr = null;
            inputStr = null;
            tempSum = null;
            for (int i = 0; i < result.Rows.Count; i++)
            {
                inputStr = result.Rows[i]["类型"].ToString();

                if (!string.IsNullOrEmpty(inputStr))
                {
                    processStr = WordWrap.lineStringAnalys(inputStr, _TableInfor.Columns[cellDate.ColIndex].RTBE.Width + _TableInfor.Columns[cellDate.ColIndex].RTBE.Margin.Right, _TableInfor.Columns[cellDate.ColIndex].RTBE.Font, "");
                    arr = processStr.Split(new string[] { "¤" }, StringSplitOptions.RemoveEmptyEntries);

                    if (arr.Length > 1)
                    {
                        //增加行，并将内容分别写入
                        result.Rows[i]["类型"] = arr[0];
                        tempSum = result.Rows[i]["入量"]; //如果写在第一行，那么不需要移动
                        result.Rows[i]["入量"] = "";

                        for (int j = 1; j < arr.Length; j++)
                        {
                            DataRow drNew = result.NewRow();
                            for (int k = 0; k < result.Columns.Count; k++)
                            {
                                //if(result.Columns[k].DataType.FullName == "string"
                                drNew[k] = "";
                            }

                            drNew["类型"] = arr[j];

                            if (j == arr.Length - 1)
                            {
                                drNew["入量"] = tempSum;
                            }

                            result.Rows.InsertAt(drNew, i + 1);

                        }
                    }
                }
            }


            //如果入量dt不为null，那么将出量分行后的合并到dt中
            if (dtOut != null)
            {
                int index = 0;
                for (int i = 0; i < dtOut.Rows.Count; i++)
                {
                    if (i >= result.Rows.Count)
                    {
                        result.Rows.Add();
                    }

                    result.Rows[index][outputValueColumn] = dtOut.Rows[i][outputValueColumn].ToString();
                    result.Rows[index][outputTypeColumn] = dtOut.Rows[i][outputTypeColumn].ToString();

                    index++;
                }
            }

            cellDate = null;
            processStr = null;
            arr = null;
            inputStr = null;

            return result;

        }

        #endregion 途径多行的处理：合计前合并，合计后再拆分到行

        /// <summary>
        /// 根据设置：mode，inputTypeColumn  columnsList 为分组合计组装好数据。 sumType，recordsNode，dt
        /// 分组合计的时候，如果途径名是空，那么这行数据就被过滤了
        /// </summary>
        /// ,bool isExtend, DateTime dtFrom ,DateTime dtTo 根据扩展弹出的界面，选择结类型，结时间范围来合计时间范围内的数据。
        /// <returns></returns>
        private DataTable GetDateForGroup(ref string mode, string inputTypeColumn, string inputValueColumn, List<string> columnsList, string sumType, XmlNode recordsNode, DataTable dt, SumGroup sg
            , bool isExtend, DateTime dtFrom, DateTime dtTo)
        {
            XmlNode xn = null;
            string eachValue = "";
            double eachValueInt = 0;
            object[] arrNewRow = dt.NewRow().ItemArray;
            string sort = "";
            string tujin = "";

            //从当前行所在的节点，往前推
            xn = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString(), "=", nameof(EntXmlModel.Record)));

            bool isGoDoIt = false;
            DateTime recordDtTime = GetServerDate();
            string recordDt = "";
            string[] arr = null;
            string tempDateTime = null;

            while (xn != null)
            {
                xn = xn.PreviousSibling; //去上一个节点

                isGoDoIt = false;

                if (isExtend)
                {
                    if (xn != null && !Comm.isEmptyRecord(xn) && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) == "")
                    {
                        //根据时间范围来判断：直到没有下一个节点，或者节点的日期已经的时间已经小于选择的开始时间、就结束
                        //取到该条数据的对应的时间
                        recordDt = GetRowDateTime((xn as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                        if (recordDt != "¤") //防止异常情况下取不到这条数据的日期时间
                        {
                            arr = recordDt.Split('¤');

                            //防止只有日期，没有时间的表格
                            if (arr[1].Trim() == "")
                            {
                                arr[1] = "00:00";
                            }

                            //护理部要求写入24:00
                            arr[1] = arr[1] == "24:00" ? "23:59" : arr[1];

                            //可能是界面上刚输入的，不合法的日期的行数据
                            tempDateTime = arr[0] + " " + arr[1];
                            if (!DateHelp.IsDateTime(tempDateTime))
                            {
                                //跳过错误数据
                                ShowInforMsg("在列合计的时候，发现某行日期时间的格式不正确，忽略了该行明细数据：" + tempDateTime, true);
                                continue;
                            }
                            else
                            {
                                recordDtTime = DateTime.Parse(tempDateTime);
                            }

                            //判断时间，范围内的数据进行统计，早于范围下限后就跳出
                            if (recordDtTime.CompareTo(dtFrom) >= 0 && recordDtTime.CompareTo(dtTo) <= 0)
                            {
                                isGoDoIt = true;
                            }
                            else if (recordDtTime.CompareTo(dtFrom) < 0)
                            {
                                //如果遍历到时间下限之前的数据，那么跳过
                                //break;

                                isGoDoIt = false;
                            }
                            else
                            {
                                continue; //跳过。上面一行大于开始时间，跳过，继续往前判断。
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (xn != null)
                        {
                            //遇到空行，合计行 ，跳过
                            continue;
                        }
                        else
                        {
                            //没有下一个节点，那么就跳出
                            break;
                        }
                    }
                }
                else
                {
                    //遇到满足条件的小结或者总结就结束
                    isGoDoIt = (xn != null && ((sumType == "小结" && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) == "") || (sumType == "总结" && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) != "总结")));
                }

                ////不为空，并且为非合计、小计行(nameof(EntXmlModel.Total)非空，表示需要统计的明细数据)
                ////小结：遇到上一次小结或总结就结束
                ////总结：遇到上一次总结为止，遇到小结不结束
                //if (xn != null &&
                //    ((sumType == "小结" && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) == "") || (sumType == "总结" && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) != "总结"))) // (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) == ""
                if (isGoDoIt)
                {
                    //如果是总结，还会有小计的行出现，也跳过。空行也跳过
                    if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) != "" || Comm.isEmptyRecord(xn))
                    {
                        continue;
                    }

                    sort = "";
                    tujin = "";

                    //string inputStr = null;

                    for (int j = 0; j < columnsList.Count; j++)
                    {
                        //判断该单元格是否为空，或者被合并。
                        if ((xn as XmlElement).GetAttribute(columnsList[j]) == "" || (xn as XmlElement).GetAttribute(columnsList[j]).Split('¤')[7].Contains("True"))
                        {
                            //continue;
                            eachValue = "0";
                            eachValueInt = 0; // 不能跳过，因为出量可能有值

                            //inputStr = null;
                        }
                        else
                        {
                            eachValue = (xn as XmlElement).GetAttribute(columnsList[j]).Split('¤')[0]; // 取文本Text
                            //inputStr = eachValue;
                            eachValueInt = StringNumber.getFirstNum(eachValue.Trim());
                        }

                        //第一个是入量，后面是出量
                        if (j == 0) //columnsList[j] == inputValueColumn && 
                        {
                            //-------------------------小结总结的时候，入量名称可能多行（入量途径绝不能多行）
                            //入量对应：入量途径，入量名称，入量值。但是入量名称中的文字可能一行写不下，就会回行：变成一行或者两行
                            //如下：（人血白蛋白是一个名词，变成两行了。那对于途径，如果当前行是行，那么去相同时间的上一行（日期时间相同的上一行））
                            //静脉__人血白
                            //______蛋白____100  
                            //
                            //-----------------------如果当前的途径为空，但是数值有内容的话，就应该去上一行的途径作为本行的途径（递归）
                            //极端特殊情况下，一个时间点，会有多个途径，挂了多袋水。是个时间点，可能有多个途径。（这就要求途径必须不能回行）通过列宽和字体来调整。

                            //如果入量途径或者入量的途径（即分组合计的名称列，也多行的话，那逻辑就更复杂了）：只能通过数值位置来判断是否那几行是一个名称。
                            //有些是数字卸载一个名称的第一行；有些卸载最后一行。


                            //入量数值列：设定类型，排序，途径,入量值
                            //tujin = (xn as XmlElement).GetAttribute(inputTypeColumn).Split('¤')[0]; //根据配置的【途径列】取到途径内容

                            #region 如果名称多行，数值卸载尾行，那么数值的行的途径需要网上遍历
                            ////但是这种方式，还是没有办法解决途径，即分组的文字回行的情况。分组会错
                            ////tujin = (xn as XmlElement).GetAttribute(inputTypeColumn).Split('¤')[0]; //根据配置的【途径列】取到途径内容
                            //if (string.IsNullOrEmpty(tujin) && !string.IsNullOrEmpty(eachValue))
                            //{
                            //    //名称要回行，或者一个途径多个入量名称。又或者一个时间点，多个途径。都是有可能的。
                            //    tujin = GetTuJin(recordsNode, xn, inputTypeColumn, inputValueColumn);
                            //}
                            #endregion 如果名称多行，数值卸载尾行，那么数值的行的途径需要网上遍历
                            if (_TuJinMultilineTop == 0)
                            {
                                tujin = (xn as XmlElement).GetAttribute(inputTypeColumn).Split('¤')[0]; //根据配置的【途径列】取到途径内容
                                if (string.IsNullOrEmpty(tujin) && !string.IsNullOrEmpty(eachValue))
                                {
                                    //名称要回行，或者一个途径多个入量名称。又或者一个时间点，多个途径。都是有可能的。
                                    tujin = GetTuJin(recordsNode, xn, inputTypeColumn, inputValueColumn);
                                }
                            }
                            else if (_TuJinMultilineTop == 1) //start
                            {
                                tujin = GetTuJinTop(recordsNode, xn, inputTypeColumn, inputValueColumn);
                            }
                            else if (_TuJinMultilineTop == 2) //end
                            {
                                tujin = GetTuJinEnd(recordsNode, xn, inputTypeColumn, inputValueColumn);
                            }
                            else
                            {
                                tujin = (xn as XmlElement).GetAttribute(inputTypeColumn).Split('¤')[0]; //根据配置的【途径列】取到途径内容
                            }

                            //获取可配置的再次分组和排序
                            arrNewRow[0] = GetGroupTypeName(tujin, ref sort); //可能还要根据途径分组来分组，也许有些医院其实没有途径，直接写名称了吧。
                            arrNewRow[1] = sort;
                            arrNewRow[2] = tujin;


                            //如果没有途径，不能为空，不然会合计时会过滤的
                            if (mode == "不分组")
                            {
                                arrNewRow[0] = "不分组";
                                arrNewRow[2] = "不分组";
                            }
                            else if (mode == "总结不分组")
                            {
                                if (sumType == "总结")
                                {
                                    mode = "不分组";
                                    arrNewRow[0] = "不分组";
                                    arrNewRow[2] = "不分组";
                                }
                            }
                            else if (mode == "小结不分组")
                            {
                                if (sumType == "小结")
                                {
                                    mode = "不分组";
                                    arrNewRow[0] = "不分组";
                                    arrNewRow[2] = "不分组";
                                }
                            }
                        }

                        //-1表示没有数据
                        if (eachValueInt != -1)
                        {
                            arrNewRow[j + 3] = eachValueInt;
                        }
                        else
                        {
                            arrNewRow[j + 3] = 0;
                        }
                    }

                    //sg.AddData(dt, new object[] { "输入", "B", "im", 10 }); //将数据插入小结总结对象中
                    sg.AddData(dt, arrNewRow);
                }
                else
                {
                    break; //条件结束，跳出，不再处理
                }
            }

            return dt;
        }

        /// <summary>
        /// 自动从上次小结总结到目前为止的小结，从上次总结到目前为止的总结。
        /// 入量要分类汇总
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private int _TuJinMultilineTop = 0;
        private void rtbe_ShowSumMainForm(object sender, EventArgs e)
        {
            try
            {
                if (_TableInfor.CurrentCell == null)
                {
                    return;
                }

                //string aaa = Guid.NewGuid().ToString();//5086c561-4ea3-4a62-98bf-9e1dca5cc900

                this.Cursor = Cursors.WaitCursor;

                string sumType = sender.ToString();  //判别是小结还是总结

                //获取配置的指定信息，SumGroup="入量列@实入¤途径列@途径¤出量列@出量1,出量2¤行线@BottomLine,Red¤结文字列@出量3"
                //SumGroup="入量列@实入实入¤途径列@入量途径¤出量列@出量1,出量2,出量3¤行线@BottomLine,Red¤结文字列@项目"
                string inputValueColumn = Comm.GetPropertyByName(_TableInfor.SumGroup, "入量列"); //输入的数值
                string inputTypeColumn = Comm.GetPropertyByName(_TableInfor.SumGroup, "途径列");  //输入的途径
                string outputColumns = Comm.GetPropertyByName(_TableInfor.SumGroup, "出量列");    //数量的数值
                string[] outputColumnArr = outputColumns.Split(',');
                string lineSetting = Comm.GetPropertyByName(_TableInfor.SumGroup, "行线");  //ROW="Black¤BottomLine¤¤Red" //行信息：行线颜色，双红线类型，行文字颜色，双红线颜色
                string wordSum = Comm.GetPropertyByName(_TableInfor.SumGroup, "结文字列");
                string mode = Comm.GetPropertyByName(_TableInfor.SumGroup, "模式");  //默认是分组的，如果是：模式@不分组，那么所有的分组行会合成一行进行小结和总结（固定文字“小结”，“总结”和合计值）

                string multilineMode = Comm.GetPropertyByName(_TableInfor.SumGroup, "多行"); //如果途径是多行的是多行的时候，数字是写在第一行还是末尾最后一行。
                if (multilineMode.ToLower() == "end")
                {
                    //途径回行，是多行的时候，数字写在最后一行（也是类似段落的意思）贵阳是写在最后一行
                    _TuJinMultilineTop = 2;
                }
                else if (multilineMode.ToLower() == "start")
                {
                    //途径回行，是多行的时候，数字写在第一行。
                    _TuJinMultilineTop = 1;
                }
                else
                {
                    _TuJinMultilineTop = 0; //默认不配置该项，那么认为途径是单行的
                }


                //合计行的多行，首尾红线。
                bool startEndLine = false;
                if (lineSetting.StartsWith("首尾"))
                {
                    startEndLine = true;
                }

                //要求增加出量的分组合计：¤出量途径值列@其他出量¤出量途径列@其他出量名称
                string outputValueColumn = Comm.GetPropertyByName(_TableInfor.SumGroup, "出量途径值列"); //输入的数值
                string outputTypeColumn = Comm.GetPropertyByName(_TableInfor.SumGroup, "出量途径列");  //输入的途径

                //需要合计的列：输入一个，输出n个
                List<string> columnsList = new List<string>();
                columnsList.Add(inputValueColumn);
                columnsList.AddRange(outputColumnArr);

                string msg = "";
                SumGroup sg = new SumGroup();  //合计处理对象类
                DataTable dt = sg.InitDataTable(outputColumnArr);

                XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));  //当前的所有数据

                //更新xml：设计行的合计状态，以及显示该行是否已经是合计行。
                string nodeId = _TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].ID.ToString();
                XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId, "=", nameof(EntXmlModel.Record))); //当前行，在xml数据中的节点
                XmlNode currentCellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId, "=", nameof(EntXmlModel.Record))); //保存下，当前行的节点，以便处理下一页的时候用

                //弹出结类型，选择时间范围和结文字
                bool isExtend = false; //当为true的时候，合计的数据范围根据日期来判断，小结文字也根据返回的结文字
                string extendSumWord = "";
                DateTime dtFrom = GetServerDate();
                DateTime dtTo = GetServerDate();
                if (!string.IsNullOrEmpty(_TableInfor.SumGroupExtend))
                {
                    Frm_SumTime stf = new Frm_SumTime(_TableInfor.SumGroupExtend, Comm.GetPropertyByName(_TableInfor.SumGroup, "结文字日期格式"));
                    DialogResult drl = stf.ShowDialog();

                    if (drl == DialogResult.OK)
                    {
                        isExtend = true;

                        //取得界面选择的时间范围和结文字
                        extendSumWord = stf.StrWord;
                        dtFrom = stf.DtFrom;
                        dtTo = stf.DtTo;
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;
                        isExtend = false;
                        return;
                    }
                }

                //根据合计条件和表单数据进行合计
                if (recordsNode != null && recordsNode.ChildNodes.Count > 0)
                {
                    //为进行分组合计，准备好数据，以及便于分组合计的数据。第一次入量
                    dt = GetDateForGroup(ref mode, inputTypeColumn, inputValueColumn, columnsList, sumType, recordsNode, dt, sg, isExtend, dtFrom, dtTo);

                    #region 老的代码，只有入量分组合计时，没有独立出来一个方法
                    //XmlNode xn = null;
                    //string eachValue = "";
                    //double eachValueInt = 0;
                    //object[] arrNewRow = dt.NewRow().ItemArray;
                    //string sort = "";
                    //string tujin = "";

                    ////从当前行所在的节点，往前推
                    //xn = recordsNode.SelectSingleNode("Record[@ID='" + _TableInfor.Rows[_CurrentCellIndex.X].ID + "']");

                    //while (xn != null)
                    //{
                    //    xn = xn.PreviousSibling; //去上一个节点

                    //    //不为空，并且为非合计、小计行(nameof(EntXmlModel.Total)非空，表示需要统计的明细数据)
                    //    //小结：遇到上一次小结或总结就结束
                    //    //总结：遇到上一次总结为止，遇到小结不结束
                    //    if (xn != null &&
                    //        ((sumType == "小结" && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) == "") || (sumType == "总结" && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) != "总结"))) // (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) == ""
                    //    {
                    //        //如果是总结，还会有小计的行出现，也跳过。空行也跳过
                    //        if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) != "" || isEmptyRecord(xn))
                    //        {
                    //            continue;
                    //        }

                    //        sort = "";
                    //        tujin = "";

                    //        for (int j = 0; j < columnsList.Count; j++)
                    //        {
                    //            //判断该单元格是否为空，或者被合并。
                    //            if ((xn as XmlElement).GetAttribute(columnsList[j]) == "" || (xn as XmlElement).GetAttribute(columnsList[j]).Split('¤')[7].Contains("True"))
                    //            {
                    //                //continue;
                    //                eachValue = "0";
                    //                eachValueInt = 0; // 不能跳过，因为出量可能有值
                    //            }
                    //            else
                    //            {
                    //                eachValue = (xn as XmlElement).GetAttribute(columnsList[j]).Split('¤')[0]; // 取文本Text
                    //                eachValueInt = StringNumber.getFirstNum(eachValue.Trim());
                    //            }

                    //            if (j == 0) //columnsList[j] == inputValueColumn && 
                    //            {
                    //                //入量数值列：设定类型，排序，途径,入量值
                    //                tujin = (xn as XmlElement).GetAttribute(inputTypeColumn).Split('¤')[0]; //根据配置的【途径列】取到途径内容
                    //                arrNewRow[0] = GetGroupTypeName(tujin, ref sort);
                    //                arrNewRow[1] = sort;
                    //                arrNewRow[2] = tujin;

                    //                //如果没有途径，不能为空，不然会合计时会过滤的
                    //                if (mode == "不分组")
                    //                {
                    //                    arrNewRow[0] = "不分组";
                    //                    arrNewRow[2] = "不分组";
                    //                }
                    //            }

                    //            //-1表示没有数据
                    //            if (eachValueInt != -1)
                    //            {
                    //                arrNewRow[j + 3] = eachValueInt;
                    //            }
                    //            else
                    //            {
                    //                arrNewRow[j + 3] = 0;
                    //            }
                    //        }

                    //        //sg.AddData(dt, new object[] { "输入", "B", "im", 10 }); //将数据插入小结总结对象中
                    //        sg.AddData(dt, arrNewRow);
                    //    }
                    //    else
                    //    {
                    //        break; //条件结束，跳出，不再处理
                    //    }
                    //}
                    #endregion 老的代码，只有入量分组合计时，没有独立出来一个方法

                    if (dt.Rows.Count == 0)
                    {
                        //没有原始数据，无需统计
                        ShowInforMsg("找不到需要进行" + sumType + "的数据，请确认选择行位置。", false);
                        this.Cursor = Cursors.Default;
                        return;
                    }

                    //进行分组小结、总结：分组合计的时候会过滤掉空途径的数据；所以如果“不分组”那么就没有入量了：所以上面dt处理的时候将“途径”写成固定的“不分组”
                    DataTable result = sg.SumByGroup(dt, ref msg);

                    //出量的分组合计。当然，入量和出量，还可能各有n列只要总计，不需要合计：比如：备用量，尿量等，已经明确类型的。【依然使用出量列来表示不需要分组合计的，只要总计的列】
                    //增加属性：¤出量途径值列@出量2¤出量途径列@出量1
                    //          ¤出量途径值列@其他出量¤出量途径列@其他出量名称
                    if (!string.IsNullOrEmpty(outputValueColumn) && !string.IsNullOrEmpty(outputTypeColumn))
                    {
                        //重新设置分组参数，数据准备，和分组。
                        //分组后有数据，那么和原来的dt合并，并且要增加两列：出量途径值列 和 出量途径列。或者在outputColumnArr的两个列名，也可以。下面方法是通用的
                        sg = new SumGroup();  //合计处理对象类
                        dt = sg.InitDataTable(null);

                        columnsList = new List<string>();
                        columnsList.Add(outputValueColumn);

                        //第一次的入量分组合计,如下：
                        //dt = GetDateForGroup(mode, inputTypeColumn, columnsList, sumType, recordsNode, dt, sg);

                        //出量分组合计：
                        dt = GetDateForGroup(ref mode, outputTypeColumn, outputValueColumn, columnsList, sumType, recordsNode, dt, sg, isExtend, dtFrom, dtTo);

                        if (dt.Rows.Count > 0)
                        {
                            DataTable resultOutput = sg.SumByGroup(dt, ref msg);

                            outputColumns = Comm.GetPropertyByName(_TableInfor.SumGroup, "出量列") + "," + outputValueColumn + "," + outputTypeColumn;
                            outputColumnArr = outputColumns.Split(','); //将出入途径和出量数值列，也加入出量列中，便于下面统一把合计结果写入界面。

                            //合并数据到result中。resultOutput中的：resultOutput.Rows[index]["入量"].ToString() resultOutput.Rows[index]["类型"].ToString()
                            result.Columns.Add(outputValueColumn);
                            result.Columns.Add(outputTypeColumn);

                            if (resultOutput != null && resultOutput.Rows.Count > 0)
                            {
                                //如果出量没有填写的话，就会导致没有出量名称和出量数值列，下面处理可能就会报错了：不包含出量列：列“出量名称”不属于表 。
                                //outputColumns = Comm.getPropertyByName(_TableInfor.SumGroup, "出量列") + "," + outputValueColumn + "," + outputTypeColumn;
                                //outputColumnArr = outputColumns.Split(','); //将出入途径和出量数值列，也加入出量列中，便于下面统一把合计结果写入界面。

                                ////合并数据到result中。resultOutput中的：resultOutput.Rows[index]["入量"].ToString() resultOutput.Rows[index]["类型"].ToString()
                                //result.Columns.Add(outputValueColumn);
                                //result.Columns.Add(outputTypeColumn);

                                int index = 0;
                                for (int i = 0; i < resultOutput.Rows.Count; i++)
                                {
                                    if (i >= result.Rows.Count)
                                    {
                                        result.Rows.Add();
                                    }

                                    result.Rows[index][outputValueColumn] = resultOutput.Rows[i]["入量"].ToString();
                                    result.Rows[index][outputTypeColumn] = resultOutput.Rows[i]["类型"].ToString();

                                    index++;
                                }
                            }
                        }
                    }

                    int currentRowIndex = _CurrentCellIndex.X;
                    string[] arr;
                    string rowForeColor = _TableInfor.Rows[currentRowIndex].RowForeColor;
                    bool isNeedAddPageNode = false;
                    int nextPageIndex = 0; //添加的小结总结行，第一次处理到下一页(下一页的第一行)：日期时间要处理

                    bool firstRowSum = false;

                    //根据得到的统计结果，写入界面上。第一行，要在指定位置列（wordSum），上写“小结/总结”
                    if (result != null && result.Rows.Count > 0)
                    {
                        #region 不分组、合计和分组的处理
                        //GetDateForGroup中已经分组合计，或者不分组总计了。但是只有一种模式。不会两种都有。
                        //到这里，如果是不分组，只有一行数据了。
                        if (mode == "不分组") //入量不需要分组合计，只要总计。那么把分组合计后的再次合计就行。形成一行
                        {

                            //【类型】置为空，【入量】再次合计；出量本来就在第一行，不用处理（出量不分组的情况下）。所以将值合计到第一行就行
                            //后来出量也分组的时候，将不分组的合计也在分组算法总进行总合计了。但是要将类型设置为空
                            if (result.Rows.Count == 1)
                            {
                                result.Rows[0]["类型"] = ""; //只有总的合计就要将不分组设置为空。但是有可能有些医院要“总量”。至少需要合计也需要分类小计的时候，是写成总量
                            }
                            else
                            {
                                //其实如果配置成不分组，返回的dt已经是一行了。不会走到这里了
                                double sumNoGroup = 0;
                                double temp = 0;
                                for (int i = result.Rows.Count - 1; i >= 0; i--)
                                {
                                    if (result.Rows[i]["入量"] != null && !string.IsNullOrEmpty(result.Rows[i]["入量"].ToString()))
                                    {
                                        if (!double.TryParse(result.Rows[i]["入量"].ToString(), out temp))
                                        {
                                            temp = 0;
                                        }

                                        sumNoGroup += temp;

                                        if (i > 0)
                                        {
                                            result.Rows.Remove(result.Rows[i]); //移除该行
                                        }
                                    }
                                }

                                //设置总的合计（只有一行了）
                                result.Rows[0]["类型"] = "";
                                result.Rows[0]["入量"] = sumNoGroup.ToString();
                            }
                        }
                        else if (mode == "合计和分组") //不分组的合计显示在第一行，分组的合计显示在下面
                        {
                            //在分类汇总的基础上，第一行增加一行总的合计。
                            //【类型】置为空，【入量】再次合计；出量本来就在第一行，不用处理。所以将值合计到第一行就行
                            if (result.Rows.Count > 1) //如果只有一个途径的时候，分组合计后就是一行，那么就不会添加合计行了。也对，但是名称是途径，不是“总量”
                            {
                                double sumNoGroup1 = 0;
                                double sumNoGroup2 = 0;
                                double temp = 0;
                                DataRow drNew = result.NewRow();
                                drNew.ItemArray = result.Rows[0].ItemArray;
                                drNew["类型"] = "总量";

                                if (result.Columns.Contains(outputTypeColumn))
                                {
                                    drNew[outputTypeColumn] = "总量";
                                }

                                //遍历：除：类型、排序、途径和出量途径名（outputTypeColumn）不要合计.
                                //或者：第一行都是合计，除了分组合计的outputTypeColumn和入量再次合计（总计）一下进行。
                                for (int i = result.Rows.Count - 1; i >= 0; i--)
                                {

                                    if (result.Rows[i]["入量"] != null && !string.IsNullOrEmpty(result.Rows[i]["入量"].ToString()))
                                    {
                                        if (!double.TryParse(result.Rows[i]["入量"].ToString(), out temp))
                                        {
                                            temp = 0;
                                        }

                                        sumNoGroup1 += temp;
                                    }

                                    if (result.Columns.Contains(outputValueColumn))
                                    {
                                        if (result.Rows[i][outputValueColumn] != null && !string.IsNullOrEmpty(result.Rows[i][outputValueColumn].ToString()))
                                        {
                                            if (!double.TryParse(result.Rows[i][outputValueColumn].ToString(), out temp))
                                            {
                                                temp = 0;
                                            }

                                            sumNoGroup2 += temp;
                                        }
                                    }
                                }

                                drNew["入量"] = sumNoGroup1.ToString();

                                if (result.Columns.Contains(outputValueColumn))
                                {
                                    drNew[outputValueColumn] = sumNoGroup2.ToString();
                                }

                                //标记该行是总的合计行
                                firstRowSum = true; //第一行是否为总的合计行

                                #region 在第一行是总合计行的时候，防止出量有专门的出量列，一二两行都是合计就重复了
                                //如果出量列中，独立出来的“大便”“尿”。不能让这样的出量，开始两行都是合计行。
                                for (int i = 0; i < outputColumnArr.Length; i++)
                                {
                                    if (result.Columns.Contains(outputColumnArr[i]) && outputColumnArr[i] != outputValueColumn && outputColumnArr[i] != outputTypeColumn)
                                    {
                                        //typeof(double)
                                        //result.Rows[i][outputColumnArr[i]] = "";
                                        result.Rows[0][outputColumnArr[i]] = 0;
                                    }
                                }
                                #endregion 在第一行是总合计行的时候，防止出量有专门的出量列，一二两行都是合计就重复了

                                //添加总的合计行
                                result.Rows.InsertAt(drNew, 0);



                            }
                        }
                        #endregion 不分组、合计和分组的处理


                        #region 将出入量，同步到体温单中
                        //TODO:出量不分组的输入空，和输入0，都变成了空。也是不合适的。【只能要求护士自己严重特别舒适为0的危重病人了（一般是尿为0是有问题的）】

                        if (result.Rows.Count > 0)
                        {
                            //体温单中的输入框，如果是自定义输入框，那就不能同步了。只能是明确配置的“大便”“出量”等输入框。而且是一天7个输入框。
                            //将出入量，同步到体温单中。根据小结总结的出入量结果，调用批量体温单的接口传入数据
                            //先判断列名中是否含有，那么取第一行。“入量”【入量列】,列名中，一般有：出量总，入量总，（尿量，大便）可选 等。
                            //行的名称，也可能是某几种入量或者出量的名字，也会对应体温单中的出入量输入框。
                            //如果列明中没有，那么判断“类型”【途径列】和【出量途径列】，根据行的名称，去对应的出入量值。
                            //体温单中的出量输入框名字@表单中的列明/合计行的出入量名。
                            //比如体温单中有一页有7个的“出量”输入框，要同步护理记录单中每次总结的入量汇总列：“使用量”的值。就配置为：出量@其他出量。
                            //SynchronizeTemperature="出量@其他出量¤入量@使用量¤尿量@尿¤大便@大便"

                            bool NeedSynchronizeTemperature = false;
                            if (!string.IsNullOrEmpty(_TemplateXml.DocumentElement.GetAttribute("SynchronizeTemperature")))
                            {
                                NeedSynchronizeTemperature = true;
                            }

                            if (NeedSynchronizeTemperature)
                            {
                                string[] arrT = _TemplateXml.DocumentElement.GetAttribute("SynchronizeTemperature").Split('¤');
                                string[] arrT2 = null;                          //拆分每个配置项

                                //体温@37¤脉搏@120¤呼吸@36¤血压@120/70
                                //string[] arrSave = new string[arrT.Length];     //存储好调用保存接口的参数
                                StringBuilder sb = new StringBuilder();

                                //特别需要注意的是【入量总】在记录单中的列名可能是“使用量”，但是在汇总的dt中固定叫做：“入量”--> inputValueColumn
                                //SynchronizeTemperature="出量@其他出量¤入量@使用量¤尿量@尿¤大便@大便"
                                //SynchronizeTemperature="总入量@入量途径¤总出量@出量¤尿量@小便¤静脉@静脉"
                                for (int index = 0; index < arrT.Length; index++)
                                {
                                    arrT2 = arrT[index].Split('@');

                                    if (index != 0)
                                    {
                                        sb.Append("¤");
                                    }

                                    //根据配置的要更新到体温单的“项目名”在dt中得到数据“值/内容”
                                    sb.Append(arrT2[0]);    //项目名
                                    sb.Append("@");
                                    sb.Append(GetSynchronizeTemperatureItemValue(result, arrT2[1], firstRowSum, inputValueColumn, outputValueColumn, outputTypeColumn));         //值（内容）

                                }

                                #region 同步体温单的时候更新UserName，这样表单签名后，体温单也要根据这个来控制不能修改。
                                //同步到体温单的时候，本来“UserName”是没有传参保存的。导致体温单中的该项为空。
                                //把表单行的签名ID传过去即可
                                //体温等项目输入的时候，这里可能还没有签名
                                //recordInor.AddItem("UserName", _TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].UserID);

                                //XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                                //XmlNode cellNode = recordsNode.SelectSingleNode("Record[@ID='" + recordInor.ID + "']");
                                //string UserName = "";
                                //if (cellNode != null)
                                //{
                                //    // UserID="Mandalat¤华佗再世¤主治医师"
                                //    UserName = (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.UserID)).Split('¤')[0];
                                //}

                                if (!string.IsNullOrEmpty(sb.ToString()))
                                {
                                    sb.Append("¤");
                                    sb.Append("UserName");    //行签名信息
                                    sb.Append("@");
                                    sb.Append(GlobalVariable.LoginUserInfo.UserCode);      //行签名信息
                                }
                                else
                                {
                                    sb.Append("UserName");    //行签名信息
                                    sb.Append("@");
                                    sb.Append(GlobalVariable.LoginUserInfo.UserCode);      //行签名信息
                                }

                                //UserName = null;

                                #endregion 同步体温单的时候更新UserName，这样表单签名后，体温单也要根据这个来控制不能修改。

                                //调用批量接口，同步更新体温单中。
                                string[][] myArray = new string[1][];
                                myArray[0] = new string[9];

                                //myArray[0][0] = this._patientInfo.Id;
                                //myArray[0][1] = string.Format("{0:yyyy-MM-dd}", dtFrom) + " " + "16:00"; //开始日期 + 14：00
                                //myArray[0][2] = sb.ToString();
                                //myArray[0][3] = "";
                                //myArray[0][4] = string.Format("{0:yyyy-MM-dd HH:mm}", getRuYuanDate(this._patientInfo.Id, _PatientsInforDT));

                                //准备好调用接口的参数设置，进行保存数据
                                //arrayEach[0] = UHID：病人唯一标识号
                                //arrayEach[1] = 日期时间（格式化为：yyyy-MM-dd HH:mm）
                                //arrayEach[2] = 体温单数据（用¤分割每一个测量项目，用@符号将项目名和项目值连接起来。如：体温@38¤脉搏@122¤呼吸@36，有多少项目就传多少项目。）
                                //arrayEach[3] = 体温单事件（如果只是提交数据，这里设置空即可。格式为：体温单事件的文字内容。用↑和↓开头来指定上下事件，默认为上事件。）↑转科、↓体温不升
                                //arrayEach[4] = 病人的入院/入区日期时间（格式化为：yyyy-MM-dd HH:mm），如果是创建体温单，那么需要根据这个时间来生成入院事件章;
                                //arrayEach[5] = 病人姓名（如：张三）
                                //arrayEach[6] = 病人HISID（HIS标识号）
                                //arrayEach[7] = 护士用户信息：（工号 + "¤" + _姓名 + "¤" + 科室 + "¤" + 病区 + "¤" + 级别）


                                myArray[0][0] = this._patientInfo.PatientId;
                                myArray[0][1] = string.Format("{0:yyyy-MM-dd}", dtFrom) + " " + "16:01"; //开始日期 + 14：00
                                myArray[0][2] = AddNurseRecordStyleColumns(sb.ToString());
                                myArray[0][3] = "";
                                myArray[0][4] = string.Format("{0:yyyy-MM-dd HH:mm}", getRuYuanDate(this._patientInfo.PatientId, _PatientsInforDT));

                                //dr = TiWenDan.GetBaseInfor(patientsDataTable, sortedDT.Rows[j]["UHID"].ToString());
                                myArray[0][5] = this._patientInfo.PatientName;
                                myArray[0][6] = this._patientInfo.PatientId;
                                //arrayEach[7] = GlobalVariable.UserArgs.UserUserName;
                                //因为可能在服务端也要更新表单，那么将表单需要的用户详细信息也传入
                                myArray[0][7] = GlobalVariable.LoginUserInfo.UserName + "¤" + GlobalVariable.LoginUserInfo.UserCode + "¤" + GlobalVariable.LoginUserInfo.DeptName + "¤" + GlobalVariable.UserDivision + "¤" + GlobalVariable.LoginUserInfo.TitleName;

                                //同步表单名：如果有两种记录单，那么需要手动选择了。
                                myArray[0][8] = "";

                                if (RunMode)
                                {
                                    string errorMsg = "";
                                    //tds = initDataService(tds);
                                    //bool operateState = tds.doInsertTempChartNew1(myArray, true, ref errorMsg);
                                    bool operateState = false;// GlobalVariable._serviceLava.SaveBatchTemperature(myArray, false, ref errorMsg);

                                    if (!operateState)
                                    {
                                        string msgTempChart = "同步出入量时遇到异常：" + errorMsg;
                                        Comm.LogHelp.WriteErr(msgTempChart);
                                        MessageBox.Show(msgTempChart);
                                    }
                                }
                            }
                        }
                        #endregion 将出入量，同步到体温单中


                        #region 将分组合计中的合并多行后的途径名称，拆分到多行

                        if (_TuJinMultilineTop == 1 || _TuJinMultilineTop == 2)
                        {
                            //先准备好入量和入量途径对应的列的宽度和字体 _TableInfor.Columns[_CurrentCellIndex.Y].RTBE
                            //string processStr = WordWrap.lineStringAnalys(inputStr, rtbe.Width + rtbe.Margin.Right, rtbe.Font, "");
                            //string[] arr = processStr.Split(new string[] { "¤" }, StringSplitOptions.RemoveEmptyEntries); 
                            //CellInfor cellDate = _TableInfor.CellByRowNo_ColName(0, "日期"); //inputTypeColumn 和 outputTypeColumn
                            //_TableInfor.Columns[cellDate.ColIndex].RTBE

                            //如果途径可能为多行的情况，合计后dt中的数据还是没有分行的。
                            //就需要使用调整段罗列的算法来分行了。
                            //"类型" “outputTypeColumn”
                            if (_TuJinMultilineTop == 1) //start
                            {
                                //datatable也不是直接的引用修改的
                                result = GetTuJinDtTop(result, inputTypeColumn, inputValueColumn, outputValueColumn, outputTypeColumn);
                            }
                            else if (_TuJinMultilineTop == 2) //end
                            {
                                result = GetTuJinDtEnd(result, inputTypeColumn, inputValueColumn, outputValueColumn, outputTypeColumn);
                            }
                        }

                        #endregion 将分组合计中的合并多行后的途径名称，拆分到多行



                        IsNeedSave = true;
                        Comm._SettingRowForeColor = true; //防止弹出选择项菜单

                        //this.panelMain.Enabled = false;  //防止滚动条闪动:滚动条是不动了，但是也导致数据不能保存
                        //this.panelMain.Visible = false;
                        //this.pictureBoxBackGround.SuspendLayout();
                        //this.pictureBoxBackGround.Visible = false;
                        //int currentValue = this.panelMain.VerticalScroll.Value; //防止滚动条抖动  表格下面不要再放输入框即可。或者在坐标00放置一个size11的输入框(但是会引起最下面行小结时跨页等也会闪动)。表格下面的控件移动表格上面。
                        //this.panelMain.AutoScroll = false; //会导致滚动条置顶的
                        //显示单元格的时候：panelMain.Focus(); //使滚动条panel获取焦点，可以滚动滚动条。而且调用显示单元格的时候，和点击画布时一样。滚动条不会抖动了。

                        for (int index = 0; index < result.Rows.Count; index++)
                        {

                            if (index == 0) //第一行肯定在本页的
                            {
                                //设置行线的颜色
                                if (startEndLine)
                                {
                                    if (index == result.Rows.Count - 1)
                                    {
                                        SetSumRowLine(lineSetting.Replace("首尾", "TopBottomLine"));
                                    }
                                    else
                                    {
                                        SetSumRowLine(lineSetting.Replace("首尾", "TopLine"));
                                    }
                                }
                                else
                                {
                                    SetSumRowLine(lineSetting);  //只有处理行还在本页需要绘制线，不然就保存到xml中就可以了
                                }

                                ////"小结"、"总结"写入指定列。
                                //SetCellValue(currentRowIndex, wordSum, sumType); //只有第一行要写
                                #region "小结"、"总结"写入指定列。
                                //¤小结文字串@{0}12小时小结¤总结文字串@{0}24小时总结¤结文字日期格式@Mdd
                                //7月15日12小时白班小结   7月15日24小时总结
                                string showSumType = null;
                                string temp = null;

                                if (isExtend)
                                {
                                    showSumType = extendSumWord;
                                }
                                else
                                {
                                    if (sumType == "小结")
                                    {
                                        temp = Comm.GetPropertyByName(_TableInfor.SumGroup, "小结文字串");
                                        if (!string.IsNullOrEmpty(temp))
                                        {
                                            showSumType = temp;
                                        }
                                    }
                                    else if (sumType == "总结")
                                    {
                                        temp = Comm.GetPropertyByName(_TableInfor.SumGroup, "总结文字串");
                                        if (!string.IsNullOrEmpty(temp))
                                        {
                                            showSumType = temp;
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(showSumType))
                                    {
                                        temp = Comm.GetPropertyByName(_TableInfor.SumGroup, "结文字日期格式");

                                        if (string.IsNullOrEmpty(temp))
                                        {
                                            temp = "Mdd";
                                        }

                                        //¤小结文字串@{0}12小时小结¤总结文字串@{0}24小时总结¤结文字日期格式@Mdd
                                        if (showSumType.Contains("{0}"))
                                        {
                                            //string dateStr = string.Format("{0:" + temp + "}", GetServerDate()); //string.Format("{0:Mdd}", GetServerDate());
                                            //showSumType = string.Format(showSumType, dateStr);

                                            //临床中，一般今天是记录前一天的出入量。一般晚上进行小结，后天早上7点做总结（总结时不要小结了）
                                            //小结是当前，总结是前一天
                                            string dateStr = string.Format("{0:" + temp + "}", GetServerDate()); //string.Format("{0:Mdd}", GetServerDate());

                                            if (sumType == "总结")
                                            {
                                                dateStr = string.Format("{0:" + temp + "}", GetServerDate().AddDays(-1));
                                            }

                                            showSumType = string.Format(showSumType, dateStr);
                                            dateStr = null;
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(showSumType))
                                {
                                    SetCellValue(currentRowIndex, wordSum, showSumType); //只有第一行要写

                                    showSumType = null;
                                }
                                else
                                {
                                    //最早的时候，结文字是固定的：小结 或 总结
                                    SetCellValue(currentRowIndex, wordSum, sumType); //只有第一行要写
                                }

                                #region 出量和入量的固定文字
                                //没有配置第一行是总合计，“合计和分组”的时候，这两个属性不能配置，否则会把对应的分类名称覆盖。
                                //但是复杂的时候会有三项：24小时总结，入量总，出量总；这三项的。¤入量文字列@入量总¤出量文字列@出量总
                                temp = Comm.GetPropertyByName(_TableInfor.SumGroup, "入量文字列");
                                if (!string.IsNullOrEmpty(temp))
                                {
                                    result.Rows[index]["类型"] = temp;
                                }

                                temp = Comm.GetPropertyByName(_TableInfor.SumGroup, "出量文字列");
                                if (!string.IsNullOrEmpty(temp))
                                {
                                    if (result.Columns.Contains(outputTypeColumn))
                                    {
                                        result.Rows[index][outputTypeColumn] = temp;
                                    }
                                }

                                #endregion 出量和入量的固定文字

                                #endregion "小结"、"总结"写入指定列。

                                //入量 不管在不在本页都要(除非合计需要第一行是总的合计行)
                                if (!string.IsNullOrEmpty(inputValueColumn))
                                {
                                    SetCellValue(currentRowIndex, inputValueColumn, result.Rows[index]["入量"].ToString());
                                }

                                if (!string.IsNullOrEmpty(inputTypeColumn))
                                {
                                    SetCellValue(currentRowIndex, inputTypeColumn, result.Rows[index]["类型"].ToString());
                                }

                                //出量写入对应列。
                                for (int i = 0; i < outputColumnArr.Length; i++)
                                {
                                    //不为空，且不为零
                                    if (!string.IsNullOrEmpty(outputColumnArr[i]))
                                    {
                                        //这样的话，出量的分组合计，如果是0，也变成空了。
                                        if (outputColumnArr[i] == outputValueColumn)
                                        {
                                            SetCellValue(currentRowIndex, outputColumnArr[i], result.Rows[index][outputColumnArr[i]].ToString());
                                        }
                                        else
                                        {
                                            if (result.Rows[index][outputColumnArr[i]].ToString() != "0" && result.Rows[index][outputColumnArr[i]].ToString() != "0.0")
                                            {
                                                SetCellValue(currentRowIndex, outputColumnArr[i], result.Rows[index][outputColumnArr[i]].ToString());
                                            }
                                        }
                                    }
                                }

                                ////记录行已经小结或者总结过
                                //(cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.Total), sumType);//nameof(EntXmlModel.Total), "合计"

                                rowForeColor = _TableInfor.Rows[currentRowIndex].RowForeColor;
                            }
                            else
                            {
                                if (currentRowIndex < _TableInfor.RowsCount * _TableInfor.GroupColumnNum)
                                {
                                    //防止光标还置于原来的行的单元格内，影响行线绘制
                                    ShowCellRTBE(currentRowIndex, _CurrentCellIndex.Y);

                                    //设置行线的颜色
                                    if (startEndLine && index == result.Rows.Count - 1)
                                    {
                                        if (startEndLine)
                                        {
                                            SetSumRowLine(lineSetting.Replace("首尾", "BottomLine"));
                                        }
                                    }
                                    else
                                    {
                                        SetSumRowLine(lineSetting);  //只有处理行还在本页需要绘制线，不然就保存到xml中就可以了
                                    }

                                    //入量 不管在不在本页都要
                                    SetCellValue(currentRowIndex, inputValueColumn, result.Rows[index]["入量"].ToString());
                                    SetCellValue(currentRowIndex, inputTypeColumn, result.Rows[index]["类型"].ToString());

                                    //出量写入对应列。
                                    for (int i = 0; i < outputColumnArr.Length; i++)
                                    {
                                        if (!string.IsNullOrEmpty(outputColumnArr[i]))
                                        {
                                            if (outputColumnArr[i] == outputValueColumn)
                                            {
                                                SetCellValue(currentRowIndex, outputColumnArr[i], result.Rows[index][outputColumnArr[i]].ToString());
                                            }
                                            else
                                            {
                                                if (result.Rows[index][outputColumnArr[i]].ToString() != "0" && result.Rows[index][outputColumnArr[i]].ToString() != "0.0")
                                                {
                                                    SetCellValue(currentRowIndex, outputColumnArr[i], result.Rows[index][outputColumnArr[i]].ToString());
                                                }
                                            }
                                        }
                                    }

                                    ////记录行已经小结或者总结过
                                    //(cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.Total), sumType);//nameof(EntXmlModel.Total), "合计"
                                }
                                else
                                {
                                    //上面是小结总结行还在当前页，走早这里，表示已经处理到下一页了。也可能没有下一页的节点
                                    //1.如果添加页后，再删除行时也去除了最后一页的节点，没有了后面一页，但是空白节点还存在，再次合计跨页，可能不会添加页节点。

                                    //bool isNextSameSum = (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) != sumType && !isEmptyRecord(cellNode);
                                    //为null肯定要添加，或者这个节点不是当前的小结或总结
                                    if (cellNode == null)
                                    {
                                        //下一页，没有该节点，就创建
                                        XmlElement newNode = _RecruitXML.CreateElement(nameof(EntXmlModel.Record));
                                        int temp = _TableInfor.AddRowGetMaxID();
                                        (newNode as XmlElement).SetAttribute(nameof(EntXmlModel.ID), temp.ToString());
                                        (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), temp.ToString());

                                        recordsNode.InsertAfter(newNode, recordsNode.LastChild);

                                        cellNode = recordsNode.LastChild;

                                        isNeedAddPageNode = true;
                                    }
                                    else if ((cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) != sumType && !Comm.isEmptyRecord(cellNode))
                                    {
                                        //下一页，不是当前小结总结类型的行，不能覆盖，在插入节点
                                        XmlElement newNode = _RecruitXML.CreateElement(nameof(EntXmlModel.Record));
                                        int temp = _TableInfor.AddRowGetMaxID();
                                        (newNode as XmlElement).SetAttribute(nameof(EntXmlModel.ID), temp.ToString());
                                        (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), temp.ToString());

                                        recordsNode.InsertBefore(newNode, cellNode);

                                        cellNode = newNode;

                                        //isNeedAddPageNode = true; //不需要，因为可能反而多出一页
                                    }

                                    //将数据写入xml的节点中
                                    //1.行线颜色 ¤行线@BottomLine,Red¤
                                    arr = lineSetting.Split(',');
                                    //(cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), "Black¤" + arr[0] + "¤" + rowForeColor + "¤" + arr[1]);

                                    if (startEndLine && index == result.Rows.Count - 1)
                                    {
                                        (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), "Black¤" + "BottomLine" + "¤" + rowForeColor + "¤" + arr[1]);
                                    }
                                    else
                                    {
                                        (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), "Black¤" + arr[0] + "¤" + rowForeColor + "¤" + arr[1]);
                                    }

                                    //入量和出量的合计值
                                    //"¤False¤False¤¤False¤False¤False¤¤Black¤"
                                    (cellNode as XmlElement).SetAttribute(inputValueColumn, result.Rows[index]["入量"].ToString() + _CellValueDefault);
                                    (cellNode as XmlElement).SetAttribute(inputTypeColumn, result.Rows[index]["类型"].ToString() + _CellValueDefault);


                                    //出量写入对应列。
                                    for (int i = 0; i < outputColumnArr.Length; i++)
                                    {
                                        if (!string.IsNullOrEmpty(outputColumnArr[i]))
                                        {
                                            if (result.Rows[index][outputColumnArr[i]].ToString() != "0" && result.Rows[index][outputColumnArr[i]].ToString() != "0.0")
                                            {
                                                (cellNode as XmlElement).SetAttribute(outputColumnArr[i], result.Rows[index][outputColumnArr[i]].ToString() + _CellValueDefault);
                                            }
                                        }
                                    }

                                    //TODO：如果含有日期和时间，用第一行的日期和时间来设定
                                    if (nextPageIndex == 0)
                                    {
                                        if ((currentCellNode as XmlElement).GetAttribute("日期") != "")
                                        {
                                            (cellNode as XmlElement).SetAttribute("日期", (currentCellNode as XmlElement).GetAttribute("日期"));
                                        }

                                        if ((currentCellNode as XmlElement).GetAttribute("时间") != "")
                                        {
                                            (cellNode as XmlElement).SetAttribute("时间", (currentCellNode as XmlElement).GetAttribute("时间"));
                                        }
                                    }

                                    nextPageIndex++;
                                }
                            }


                            //记录行已经小结或者总结过
                            (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.Total), sumType);//nameof(EntXmlModel.Total), "合计"

                            //为下一行做准备-----------------------------
                            //注意：添加合计行的时候，不插入新行，那样修改的时候确实更好，但是如果再次小结总结呢？ 
                            //那么：是小结总结的行直接覆盖，不然，如果下面行不是空行，就插入行。上面第一行，肯定在 当前行位置上处理
                            //向下插入空白行
                            if (index < result.Rows.Count - 1) //已经是最后一行就不用处理了
                            {
                                if (currentRowIndex < _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
                                {
                                    if (cellNode.NextSibling != null && (cellNode.NextSibling as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) != sumType && !Comm.isEmptyRecord(cellNode.NextSibling))
                                    {
                                        tBnt_InsertRowDwon_Click(null, null);
                                    }
                                }
                            }

                            cellNode = cellNode.NextSibling;

                            currentRowIndex++;
                        }

                        //_SettingRowForeColor = false;
                        //_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Select(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Text.Length, 0); //光标置于单元格最后
                        ShowCellRTBE(_CurrentCellIndex.X, _CurrentCellIndex.Y);

                        Comm._SettingRowForeColor = false;

                        if (isNeedAddPageNode) // 小结 总结到了下一行，而且之前没有页节点，那么需要添加
                        {
                            //病人列表中如果没有下一页，那么就需要添加
                            AdddPageNode();

                            RemoveEmptyRows();  //删除程序补充的末尾的空白行
                        }

                        //this.panelMain.AutoScroll = true;
                        //this.pictureBoxBackGround.Visible = true;
                        //this.panelMain.VerticalScroll.Value = currentValue;
                        //this.pictureBoxBackGround.ResumeLayout();
                        //this.panelMain.Enabled = true;  //防止滚动条闪动

                    }
                }

                //总结 小结时如果发现某行为分组等，要提示消息给用户
                if (!string.IsNullOrEmpty(msg))
                {
                    ShowInforMsg(sumType + "的时候，遇到无法识别的途径：" + msg, false);
                }

                this.Cursor = Cursors.Default;

            }
            catch (Exception ex)
            {
                Comm._SettingRowForeColor = false;
                this.Cursor = Cursors.Default;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        #region 将出入量，同步到体温单中

        //体温单和表单的批量接口，在更新数据的时候，有一个逻辑不一样：体温单是所有项目都要传入，哪怕是空。而表单是空就不用传入了
        //那么批量体温界面，自然是空的项目也传入了。但是表单中调用出入量小结同步和体温等生命体征数据同步更新的时候，是不会将所有项目作为参数传入的。那么就有问题了.覆盖成空了。
        //所以要将NurseRecordStyle.xml中的列明补齐。
        private string AddNurseRecordStyleColumns(string valuePara)
        {

            string xmlPathNurseRecordStyle = Application.StartupPath + @"\TWD\NurseRecordStyle.xml";

            if (File.Exists(xmlPathNurseRecordStyle))
            {
                XmlDocument xmlRecordStyle = new XmlDocument();
                xmlRecordStyle.Load(xmlPathNurseRecordStyle);

                string[] arrNowCols = valuePara.Split('¤');
                for (int i = 0; i < arrNowCols.Length; i++)
                {
                    arrNowCols[i] = arrNowCols[i].Split('@')[0];
                }

                ArrayList arrList = ArrayList.Adapter(arrNowCols);

                foreach (XmlElement xe in xmlRecordStyle.DocumentElement.ChildNodes[0].ChildNodes)
                {
                    if (!arrList.Contains(xe.GetAttribute(nameof(EntXmlModel.Name))))
                    {
                        if (valuePara == "")
                        {
                            valuePara = xe.GetAttribute(nameof(EntXmlModel.Name)) + "@";
                        }
                        else
                        {
                            valuePara = valuePara + "¤" + xe.GetAttribute(nameof(EntXmlModel.Name)) + "@";
                        }
                    }
                }

                arrNowCols = null;
                xmlRecordStyle = null;
            }



            return valuePara;
        }

        /// <summary>
        /// 从合计dt中，得到体温单中出入量项目的值
        /// </summary>
        /// <param name="dtPara"></param>
        /// <param name="itemNamePara"></param>
        /// <returns></returns>
        private string GetSynchronizeTemperatureItemValue(DataTable result, string itemNamePara, bool firstRowSumPara, string inputValueColumn, string outputValueColumn, string outputTypeColumn)
        {
            string ret = "";

            //特别需要注意的是【入量总】在记录单中的列名可能是“使用量”，但是在汇总的dt中固定叫做：“入量”<--> inputValueColumn
            if (itemNamePara == inputValueColumn)
            {
                itemNamePara = "入量";
            }

            //for (int index = 0; index < result.Columns.Count; index++)
            //{
            //    if(

            //}

            //先判断列名中有没有当前项目，有是列的项目，那么就去这一列的值。还要分第一行是否合计行，第一行是合计行，直接取第一行。否则还要合计一下。
            if (result.Columns.Contains(itemNamePara))
            {
                if (firstRowSumPara)
                {
                    return result.Rows[0][itemNamePara].ToString();
                }
                else
                {
                    //大便次数，大便的量。大便的次数是0的话，是问了病人有没有大便，但是病人实际没有大便。护理记录单中记录了几次大便，那么大便次数就是几次。
                    //大便的总量就是每一次的合计值。大便的次数还有灌肠的，写法又不一样。（分子分母），还有人工肛门的。
                    //对于量来说，0就是空。但是也可能确实是观察了，没有啊。
                    //TODO：：那么可以再判断一次合计范围内的数据，填写了0，那么还是0。否则变成空。（但是判断比较复杂啊）
                    //分组合计的，是0还是0，空也是零。但是出量没有分组的，0也变成空了。
                    double sum = 0;
                    double temp = 0;
                    for (int i = result.Rows.Count - 1; i >= 0; i--)
                    {
                        if (!double.TryParse(result.Rows[i][itemNamePara].ToString(), out temp))
                        {
                            temp = 0;
                        }

                        sum += temp;
                    }

                    if (sum == 0)
                    {
                        return "";
                    }
                    else
                    {
                        return sum.ToString();
                    }
                }
            }
            else
            {
                //遍历行查找
                int startIndex = 0;
                if (firstRowSumPara)
                {
                    startIndex = 1;
                }
                else
                {
                    startIndex = 0;
                }

                for (int index = startIndex; index < result.Rows.Count; index++)
                {
                    //入量："类型"-->“入量” ； 出量：outputValueColumn --> outputTypeColumn
                    if (result.Rows[index]["类型"].ToString() == itemNamePara)
                    {
                        ret = result.Rows[index]["入量"].ToString();
                        break;
                    }
                    else if (result.Columns.Contains(outputTypeColumn) && result.Rows[index][outputTypeColumn].ToString() == itemNamePara)
                    {
                        //列“出量名称”不属于表 。(出量没有填写的时候)
                        ret = result.Rows[index][outputValueColumn].ToString();
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 为了纠正，生成入院事件张时间不准确的问题
        /// </summary>
        /// <param name="patientUHIDPara"></param>
        /// <param name="dtPatientsInof"></param>
        /// <returns></returns>
        private DateTime getRuYuanDate(string patientUHIDPara, DataTable dtPatientsInof)
        {
            DateTime dTime = GetServerDate();

            foreach (DataRow drChild in dtPatientsInof.Rows)
            {
                if (drChild["UHID"].ToString() == patientUHIDPara)
                {
                    if (drChild["入区日期"].ToString() != "")
                    {
                        dTime = DateTime.Parse(drChild["入区日期"].ToString());
                    }
                    else if (drChild["入院日期"].ToString() != "")
                    {
                        dTime = DateTime.Parse(drChild["入院日期"].ToString());
                    }
                    else
                    {
                        dTime = GetServerDate();
                    }

                    break;
                }

            }

            return dTime;
        }

        ///// <summary>
        ///// 实例化服务端；调用批量体温单接口用
        ///// </summary>
        ///// <param name="tds"></param>
        ///// <returns></returns>
        //private Towers.DataService initDataService(Towers.DataService tds)
        //{
        //    if (tds == null && RunMode)
        //    {
        //        string url = LibraryWj.GlobalVariable.Url;
        //        if (url.Contains("GlobalService"))
        //        {
        //            url += @"/DataService.asmx";
        //        }
        //        else
        //        {
        //            url += @"/GlobalService/DataService.asmx";
        //        }

        //        tds = new Towers.DataService(url);  //DataService ds = new DataService(@"http://localhost:3137/GlobalService/DataService.asmx");

        //        //iis访问超时 默认是300秒
        //        //异常类型：WebException
        //        //异常消息：操作超时
        //        tds.Timeout = 1000 * 60 * 15;
        //    }

        //    return tds;
        //}

        #endregion 将出入量，同步到体温单中

        /// <summary>
        /// 设置小结、总结行的行线
        /// </summary>
        /// <param name="lineSetting"></param>
        private void SetSumRowLine(string lineSetting)
        {

            //设置行线的颜色
            if (!string.IsNullOrEmpty(lineSetting) && lineSetting.Split(',').Length > 1)
            {
                string[] arr = lineSetting.Split(',');
                Color rowColor = Color.FromName(arr[1]);

                switch (arr[0])
                {
                    //双红线
                    case "RectLine": //"行四周红线":
                        //SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.RectLine);
                        SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.RectLine, rowColor);
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.RectLine;
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = rowColor;
                        break;
                    case "RectDoubleLine": //"行四周双红线":
                        SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.RectDoubleLine, rowColor);
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.RectDoubleLine;
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = rowColor;
                        break;
                    case "TopBottomLine": //"上下红线":
                        SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.TopBottomLine, rowColor);
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.TopBottomLine;
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = rowColor;
                        break;
                    case "TopBottomDoubleLine": //"上下双红线":
                        SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.TopBottomDoubleLine, rowColor);
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.TopBottomDoubleLine;
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = rowColor;
                        break;
                    case "BottomLine": //"下红线":
                        SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.BottomLine, rowColor);
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.BottomLine;
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = rowColor;
                        break;
                    case "BottomDoubleLine": //"下双红线":
                        SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.BottomDoubleLine, rowColor);
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.BottomDoubleLine;
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = rowColor;
                        break;

                    case "TopLine": //"上红线":
                        SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.TopLine, rowColor);
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.TopLine;
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = rowColor;
                        break;
                    case "TopDoubleLine": //"上双红线":
                        SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.TopDoubleLine, rowColor);
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.TopDoubleLine;
                        _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = rowColor;
                        break;
                }
            }

        }

        /// <summary>
        /// 单元格设值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        private void SetCellValue(int row, string columnName, string value)
        {
            CellInfor cell = _TableInfor.CellByRowNo_ColName(row, columnName);

            if (cell != null)
            {
                ShowCellRTBE(row, cell.ColIndex);

                HorizontalAlignment temp = _TableInfor.Columns[cell.ColIndex].RTBE.SelectionAlignment;

                _TableInfor.Columns[cell.ColIndex].RTBE.Text = value;

                _TableInfor.Columns[cell.ColIndex].RTBE.SelectAll();
                _TableInfor.Columns[cell.ColIndex].RTBE.SelectionAlignment = temp;
            }
        }

        # endregion 小结总结

        /// <summary>
        /// 返回高级的，带有范围的评分值判断。表达之中含有特殊符号～就表示是范围判断评分的。前后必须是数字或者空
        /// ～表示有范围，如果前后有值，表示大于等于 和小于等于前后两个数。
        /// 如果有一个空，表示只大于等于或者只小于等于一个数。
        /// 
        /// 如果范围中没有找到匹配的，那么跳过。
        /// </summary>
        /// <param name="scoreScript"></param>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        private double GetSumScoreScope(string scoreScript, double thisValue)
        {
            //Score="38～38.4@2¤38.5～@3"
            double ret = 0;

            if (thisValue == -1)
            {
                return 0; //如果基值没有，那么就跳过，没法和范围比较的
            }

            string[] eachControl = scoreScript.Split('¤');
            string[] eachArr;
            string[] eachArrNumber;
            double d1 = -999999;  // -999999表示没有配置该值
            double d2 = 999999;
            for (int j = 0; j < eachControl.Length; j++)
            {
                eachArr = eachControl[j].Split('@');
                eachArrNumber = eachArr[0].Split('～');

                eachArrNumber[0] = eachArrNumber[0].Trim(); //去空格
                eachArrNumber[1] = eachArrNumber[1].Trim();

                if (!string.IsNullOrEmpty(eachArrNumber[0]))
                {
                    if (!double.TryParse(eachArrNumber[0], out d1))
                    {
                        d1 = 0; //0表示配置的数值格式配置错误
                        MessageBox.Show("评分中数值格式配置错误，不是有效数字：" + scoreScript);
                    }
                }
                else
                {
                    d1 = -999999;
                }

                if (!string.IsNullOrEmpty(eachArrNumber[1]))
                {
                    if (!double.TryParse(eachArrNumber[1], out d2))
                    {
                        d2 = 0;
                        MessageBox.Show("评分中数值格式配置错误，不是有效数字：" + scoreScript);
                    }
                }
                else
                {
                    d2 = 999999; //不配置的时候，只要小于指定的d1，小于最大数999999就行了
                }


                if (thisValue >= d1 && thisValue <= d2)
                {
                    ret = double.Parse(eachArr[1].Trim());
                    break;
                }
            }

            return ret;
        }


        /// <summary>
        /// 合计算法
        /// </summary>
        /// <param name="exeStrPara">表达式</param>
        /// <returns></returns>
        private string Sum(string exeStrPara)
        {
            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                RichTextBoxExtended rtbe = ((RichTextBoxExtended)_Need_Assistant_Control);

                #region -- 预警评分
                //条件判断值
                //                1分 2分 1分 0分 1分 2分 3分
                //舒张压（mmHg）  <70 71-80 81-100 101-199 >=200 
                //心率（次/分）   <40 41-50 51-100 101-110 111-129 >=130
                //呼吸频率(次/分) <9 9-14 15-20 21-29 >=30
                //体温（C）       <35 35-38.4 >=38.5 
                //AVPU评分 
                //MEWS合计总分

                //1、 舒张压是百分比输入：举例90/150只用分子数值做判断
                //2、 AVPU评分文本框输入，护士自己手动输入分值
                //3、 空白部分不需要考虑，根据已有条件判断及可

                //TODO:用脚本来实现，判断分支过多，导致脚本很多，性能很慢。所以还是通过扩展合计来做。
                //Ω∫ ≈表示有评分范围，就优先取评分范围对应的值，没有范围就去当前文字中的第一段数字，文字也没有数字，就认为是0，或者跳过这个被合计项。（因为原来文字内就会有文字）
                //评分进行扩展用全角的～符号分割最小值和最大值进行解析，满足范围的返回对应值。原来是匹配一直，返回@后面设定的值。
                //基本的评分配置：Score="√@3"
                //扩展后的评分配置：体温列的配置：Score="～34.9@1¤35～38.4@2¤38.5～@3" Score="～70@1¤71～80@2¤81～100@1¤101～199@1"
                //合计的SUM配置Sum="≈体温+脉搏+实际数值列"
                #endregion -- 预警评分

                bool _SumScoreFirst = false; //因为合计项中还有可能个别是没有评分的，直接取值的。所以，只能说是优先取评分，，，，


                //没有权限，不能修改的输入框，不进行赋值
                if (rtbe.ReadOnly)
                {
                    return "";
                }

                double sumAll = 0;
                string exeStr = "";

                //如果参数不为空，那么取参数的公式
                if (!string.IsNullOrEmpty(exeStrPara))
                {
                    exeStr = exeStrPara;
                }
                else
                {
                    //如果为空，取输入框配置的公式
                    exeStr = rtbe.Sum;
                }

                //用约等于来表示高级扩展后的有范围的评分设定。也符合约等于的实际意思。有才……
                if (exeStr.StartsWith("≈"))
                {
                    _SumScoreFirst = true;
                    exeStr = exeStr.TrimStart('≈');
                }

                //获取不到合计值的时候，再根据评分来合计
                string scoreScript = "";
                string[] eachControl;
                string[] eachArr;

                HorizontalAlignment temp;

                //如果是非表格,那么根据输入框的配置属性:Sum="出量1:出量4"或者Sum="出量1+出量4" 来进行自动计算。
                if (!rtbe._IsTable)
                {
                    //合计公式不为空，那么表示设置了合计公式
                    if (!string.IsNullOrEmpty(exeStr))
                    {
                        //预产期优先计算，因为格式和其他公式不一样，否则会报错；没法计算的。
                        if (exeStr == "预产期" || exeStr == "孕周")
                        {
                            //孕产期计算方式：末次月经的月份数+9或者-3，日期数+7，若超过月底日期，则顺延至下月。
                            //例如：末次月经是2017-01-21，则孕产期为2017-10-28
                            //-----------------------------------------------------------------------------------------------------------------------------
                            //孕周计算方式：
                            //1.今日离末次月经若不到28周，则按天数算。例如今天是2017-01-21，末次月经是2016-07-21，计算出的间隔天数为184天，那么孕周为26+2周
                            //2.若今日离末次月经超过28周，则按预产期满40周计算，从预产期往前算至今日。
                            //例如：今天为2017-01-21，预产期为2017-01-31，预产期-今天=10天，则孕周为（40周-10天）即38+4周。

                            MessageBox.Show("TODO:根据末次月经来计算得到，更合并后再处理。", "提示消息");

                            return "";
                        }

                        ////如果是直接的表达式
                        ////将输入框的数值，结合公式代替后计算
                        if (exeStr.StartsWith("∑"))
                        {
                            exeStr = exeStr.TrimStart('∑');
                            //sumAll = Evaluator.EvaluateToInteger(exeStr);

                            //Sum="∑[%P1.积极因子分4%]+(20 - [积极因子分1])*2"
                            sumAll = Caculate_ExeculateStr(exeStr, false); //上面的只能纯表达式，现在可以将输入框名字的值代替计算

                            if (string.IsNullOrEmpty(exeStrPara))
                            {
                                temp = rtbe.SelectionAlignment;

                                //将合计的结果显示到合计输入框上
                                rtbe.Text = sumAll.ToString();

                                rtbe.SelectionAlignment = temp;
                            }

                            return sumAll.ToString();
                        }

                        Control[] ct;
                        double valueEach = 0;


                        //目前支持和excel一样的公式：A1:AN, A1+A2+AN……
                        if (exeStr.Contains(":"))
                        {
                            string[] arr = exeStr.Split(':');
                            int index = StringNumber.getLastNum(arr[0]);
                            string controlName = arr[0].Trim().Replace(index.ToString(), ""); //arr[0].Replace(index.ToString(), ""); +或：两侧有空格就不能计算了

                            int indexEnd = StringNumber.getLastNum(arr[1]);

                            string checkedstr = "";

                            for (int i = index; i <= indexEnd; i++)
                            {
                                ct = this.pictureBoxBackGround.Controls.Find(controlName + i.ToString(), false);

                                if (ct != null && ct.Length > 0)
                                {
                                    if (!string.IsNullOrEmpty(ct[0].Text.Trim()) || ct[0] is CheckBoxExtended) //有些勾选框的显示值没有，就不会评分了。用全家空格代替吧
                                    {
                                        valueEach = StringNumber.getFirstNum(ct[0].Text.Trim());

                                        if (ct[0] is CheckBoxExtended)
                                        {
                                            //输入框和下拉框，一般是根据文字内容来合计的
                                            valueEach = -1; //勾选框肯定是根据选中不选中来配置，输入框等如果数值没有，还是不用评分来，不然如果为空，就用评分，会错乱
                                        }

                                        if (valueEach != -1 && !_SumScoreFirst) //取到数据的时候
                                        {
                                            sumAll += valueEach;
                                        }
                                        else
                                        {
                                            //如果为空，那么在判断表单的评分设定：
                                            scoreScript = "";
                                            if (ct[0] is CheckBoxExtended)
                                            {
                                                scoreScript = ((CheckBoxExtended)ct[0]).Score;

                                                if (!string.IsNullOrEmpty(scoreScript))
                                                {
                                                    eachControl = scoreScript.Split('¤');
                                                    for (int j = 0; j < eachControl.Length; j++)
                                                    {
                                                        eachArr = eachControl[j].Split('@');

                                                        //跨页的，如：Sum="∑[%P1.积极因子分4%]+(20 - [积极因子分1])*2"
                                                        //有可能是三种状态的勾选框，如果打叉了，Checked也为True，但逻辑上是未选的意思。原来未选是表示没有人工选择，还是默认值。
                                                        if (((CheckBoxExtended)ct[0]).ThreeState)
                                                        {
                                                            if (((CheckBoxExtended)ct[0]).CheckState == CheckState.Checked)
                                                            {
                                                                //只有选中为True，其他都为False
                                                                checkedstr = "true";
                                                            }
                                                            else
                                                            {
                                                                checkedstr = "false"; //默认未选，可以做空，跳过也符合逻辑
                                                            }
                                                        }
                                                        else
                                                        {
                                                            checkedstr = ((CheckBoxExtended)ct[0]).Checked.ToString().ToLower();
                                                        }

                                                        //if (((CheckBoxExtended)ct[0]).Checked.ToString().ToLower() == eachArr[0].ToLower().Trim())
                                                        if (checkedstr == eachArr[0].ToLower().Trim())
                                                        {
                                                            sumAll += double.Parse(eachArr[1].Trim());
                                                            break;//找到第一个满足评分的项目就跳出
                                                        }
                                                    }
                                                }
                                            }
                                            else if (ct[0] is RichTextBoxExtended)
                                            {
                                                scoreScript = ((RichTextBoxExtended)ct[0]).Score;

                                                if (scoreScript.Contains("～"))
                                                {
                                                    sumAll += GetSumScoreScope(scoreScript, valueEach); //正好目前的血压也是取分子，即：舒张压，第一段数字
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(scoreScript))
                                                    {
                                                        eachControl = scoreScript.Split('¤');
                                                        for (int j = 0; j < eachControl.Length; j++)
                                                        {
                                                            eachArr = eachControl[j].Split('@');

                                                            if (((RichTextBoxExtended)ct[0]).Text == eachArr[0].ToLower().Trim())
                                                            {
                                                                sumAll += double.Parse(eachArr[1].Trim());
                                                                break;//找到第一个满足评分的项目就跳出
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (ct[0] is ComboBoxExtended)
                                            {
                                                scoreScript = ((ComboBoxExtended)ct[0]).Score;

                                                if (scoreScript.Contains("～"))
                                                {
                                                    sumAll += GetSumScoreScope(scoreScript, valueEach); //正好目前的血压也是取分子，即：舒张压，第一段数字
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(scoreScript))
                                                    {
                                                        eachControl = scoreScript.Split('¤');
                                                        for (int j = 0; j < eachControl.Length; j++)
                                                        {
                                                            eachArr = eachControl[j].Split('@');

                                                            if (((ComboBoxExtended)ct[0]).Text == eachArr[0].ToLower().Trim())
                                                            {
                                                                sumAll += double.Parse(eachArr[1].Trim());
                                                                break;//找到第一个满足评分的项目就跳出
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            //如果该被合计的子项没有配置评分，那么就去实际值，如果实际值中没有数值那么好用0，不累计该项
                                            if (string.IsNullOrEmpty(scoreScript))
                                            {
                                                if (valueEach != -1)
                                                {
                                                    sumAll += valueEach;
                                                }
                                            }

                                            //评分处理结束
                                        }
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(exeStrPara))
                            {
                                temp = rtbe.SelectionAlignment;
                                //将合计的结果显示到合计输入框上
                                rtbe.Text = sumAll.ToString();
                                rtbe.SelectionAlignment = temp;
                            }
                        }
                        else if (exeStr.Contains("+"))
                        {
                            string[] arr = exeStr.Split('+');

                            for (int i = 0; i < arr.Length; i++)
                            {
                                ct = this.pictureBoxBackGround.Controls.Find(arr[i].Trim(), false);

                                if (ct != null && ct.Length > 0)
                                {
                                    if (!string.IsNullOrEmpty(ct[0].Text.Trim()) || ct[0] is CheckBoxExtended)//有些勾选框的显示值没有，就不会评分了。用全家空格代替吧
                                    {
                                        valueEach = StringNumber.getFirstNum(ct[0].Text.Trim());

                                        if (ct[0] is CheckBoxExtended)
                                        {
                                            //输入框和下拉框，一般是根据文字内容来合计的
                                            valueEach = -1; //勾选框肯定是根据选中不选中来配置，输入框等如果数值没有，还是不用评分来，不然如果为空，就用评分，会错乱
                                        }

                                        if (valueEach != -1 && !_SumScoreFirst)
                                        {
                                            sumAll += valueEach;
                                        }
                                        else
                                        {
                                            //如果为空，那么在判断表单的评分设定：
                                            scoreScript = "";
                                            if (ct[0] is CheckBoxExtended)
                                            {
                                                scoreScript = ((CheckBoxExtended)ct[0]).Score;

                                                if (!string.IsNullOrEmpty(scoreScript))
                                                {
                                                    eachControl = scoreScript.Split('¤');
                                                    for (int j = 0; j < eachControl.Length; j++)
                                                    {
                                                        eachArr = eachControl[j].Split('@');

                                                        if (((CheckBoxExtended)ct[0]).Checked.ToString().ToLower() == eachArr[0].ToLower().Trim())
                                                        {
                                                            sumAll += double.Parse(eachArr[1].Trim());
                                                            break;//找到第一个满足评分的项目就跳出
                                                        }
                                                    }
                                                }
                                            }
                                            else if (ct[0] is RichTextBoxExtended)
                                            {
                                                scoreScript = ((RichTextBoxExtended)ct[0]).Score;

                                                if (scoreScript.Contains("～"))
                                                {
                                                    sumAll += GetSumScoreScope(scoreScript, valueEach); //正好目前的血压也是取分子，即：舒张压，第一段数字
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(scoreScript))
                                                    {
                                                        eachControl = scoreScript.Split('¤');
                                                        for (int j = 0; j < eachControl.Length; j++)
                                                        {
                                                            eachArr = eachControl[j].Split('@');

                                                            if (((RichTextBoxExtended)ct[0]).Text == eachArr[0].ToLower().Trim())
                                                            {
                                                                sumAll += double.Parse(eachArr[1].Trim());
                                                                break;//找到第一个满足评分的项目就跳出
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (ct[0] is ComboBoxExtended)
                                            {
                                                scoreScript = ((ComboBoxExtended)ct[0]).Score;

                                                if (scoreScript.Contains("～"))
                                                {
                                                    sumAll += GetSumScoreScope(scoreScript, valueEach); //正好目前的血压也是取分子，即：舒张压，第一段数字
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(scoreScript))
                                                    {
                                                        eachControl = scoreScript.Split('¤');
                                                        for (int j = 0; j < eachControl.Length; j++)
                                                        {
                                                            eachArr = eachControl[j].Split('@');

                                                            if (((ComboBoxExtended)ct[0]).Text == eachArr[0].ToLower().Trim())
                                                            {
                                                                sumAll += double.Parse(eachArr[1].Trim());
                                                                break;//找到第一个满足评分的项目就跳出
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            //如果该被合计的子项没有配置评分，那么就去实际值，如果实际值中没有数值那么好用0，不累计该项
                                            if (string.IsNullOrEmpty(scoreScript))
                                            {
                                                if (valueEach != -1)
                                                {
                                                    sumAll += valueEach;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //写入值
                            if (string.IsNullOrEmpty(exeStrPara))
                            {
                                temp = rtbe.SelectionAlignment;
                                //将合计的结果显示到合计输入框上
                                rtbe.Text = sumAll.ToString();
                                rtbe.SelectionAlignment = temp;
                            }
                        }
                    }
                    else
                    {
                        //
                        ShowInforMsg("该项目没有配置合计功能，无法进行合计汇总：" + rtbe.Name);
                    }
                }
                else
                {
                    //表格式的合计：分为列合计和行合计两种（多行的合计，采用点击合计按钮，选择时间范围来计算）

                    //1.0 一行中某几列的单元格的合计，统计
                    if (!string.IsNullOrEmpty(exeStr))
                    {
                        CellInfor thisCell;
                        double valueEach = 0;

                        ////如果是直接的表达式
                        ////将输入框的数值，结合公式代替后计算
                        if (exeStr.StartsWith("∑"))
                        {
                            exeStr = exeStr.TrimStart('∑');
                            //sumAll = Evaluator.EvaluateToInteger(exeStr);

                            sumAll = Caculate_ExeculateStr(exeStr, true); //上面的只能纯表达式，现在可以将输入框名字的值代替计算

                            if (string.IsNullOrEmpty(exeStrPara))
                            {
                                temp = rtbe.SelectionAlignment;

                                //将合计的结果显示到合计输入框上
                                rtbe.Text = sumAll.ToString();

                                rtbe.SelectionAlignment = temp;
                            }

                            return sumAll.ToString();
                        }

                        //目前支持和excel一样的公式：A1:AN, A1+A2+AN……
                        if (exeStr.Contains(":"))
                        {
                            string[] arr = exeStr.Split(':');
                            int index = StringNumber.getLastNum(arr[0]);
                            string controlName = arr[0].Replace(index.ToString(), "");

                            int indexEnd = StringNumber.getLastNum(arr[1]);

                            for (int i = index; i <= indexEnd; i++)
                            {
                                thisCell = _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, controlName + i.ToString());

                                if (thisCell != null && !thisCell.IsMergedNotShow)
                                {
                                    if (!string.IsNullOrEmpty(thisCell.Text.Trim()))
                                    {
                                        valueEach = StringNumber.getFirstNum(thisCell.Text.Trim());

                                        if (valueEach != -1 && !_SumScoreFirst)
                                        {
                                            sumAll += valueEach;
                                        }
                                        else
                                        {
                                            //如果为空，那么在判断表单的评分设定：
                                            scoreScript = _TableInfor.Columns[thisCell.ColIndex].RTBE.Score; //((RichTextBoxExtended)control).Score;

                                            if (scoreScript.Contains("～"))
                                            {
                                                sumAll += GetSumScoreScope(scoreScript, valueEach); //正好目前的血压也是取分子，即：舒张压，第一段数字
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(scoreScript))
                                                {
                                                    eachControl = scoreScript.Split('¤');
                                                    for (int j = 0; j < eachControl.Length; j++)
                                                    {
                                                        eachArr = eachControl[j].Split('@');

                                                        if (Regex.IsMatch(thisCell.Text.Trim(), eachArr[0].Trim()))
                                                        {
                                                            valueEach = int.Parse(eachArr[1].Trim());
                                                            sumAll += valueEach;
                                                            break;//找到第一个满足评分的项目就跳出
                                                        }
                                                    }
                                                }
                                            }

                                            //如果该被合计的子项没有配置评分，那么就去实际值，如果实际值中没有数值那么好用0，不累计该项
                                            if (string.IsNullOrEmpty(scoreScript))
                                            {
                                                if (valueEach != -1)
                                                {
                                                    sumAll += valueEach;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(exeStrPara))
                            {
                                temp = rtbe.SelectionAlignment;

                                //将合计的结果显示到合计输入框上
                                rtbe.Text = sumAll.ToString();

                                rtbe.SelectionAlignment = temp;
                            }

                        }
                        else if (exeStr.Contains("+"))
                        {
                            string[] arr = exeStr.Split('+');

                            for (int i = 0; i < arr.Length; i++)
                            {
                                thisCell = _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, arr[i]);

                                if (thisCell != null)
                                {
                                    if (!string.IsNullOrEmpty(thisCell.Text.Trim()))
                                    {
                                        valueEach = StringNumber.getFirstNum(thisCell.Text.Trim());

                                        if (valueEach != -1 && !_SumScoreFirst)
                                        {
                                            sumAll += valueEach;
                                        }
                                        else
                                        {
                                            //如果为空，那么在判断表单的评分设定：
                                            scoreScript = _TableInfor.Columns[thisCell.ColIndex].RTBE.Score; //((RichTextBoxExtended)control).Score;

                                            if (scoreScript.Contains("～"))
                                            {
                                                sumAll += GetSumScoreScope(scoreScript, valueEach); //正好目前的血压也是取分子，即：舒张压，第一段数字
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(scoreScript))
                                                {
                                                    eachControl = scoreScript.Split('¤');
                                                    for (int j = 0; j < eachControl.Length; j++)
                                                    {
                                                        eachArr = eachControl[j].Split('@');

                                                        if (Regex.IsMatch(thisCell.Text.Trim(), eachArr[0].Trim()))
                                                        {
                                                            valueEach = int.Parse(eachArr[1].Trim());
                                                            sumAll += valueEach;

                                                            break;//找到第一个满足评分的项目就跳出
                                                        }
                                                    }
                                                }
                                            }

                                            //如果该被合计的子项没有配置评分，那么就去实际值，如果实际值中没有数值那么好用0，不累计该项
                                            if (string.IsNullOrEmpty(scoreScript))
                                            {
                                                if (valueEach != -1)
                                                {
                                                    sumAll += valueEach;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(exeStrPara))
                            {
                                temp = rtbe.SelectionAlignment;

                                //将合计的结果显示到合计输入框上
                                rtbe.Text = sumAll.ToString();

                                rtbe.SelectionAlignment = temp;
                            }

                        }
                    }
                    else
                    {
                        //ShowInforMsg("该项目没有配置合计功能，无法进行合计汇总：" + rtbe.Name);
                        //2.0各行中指定列的统计（可以几列）：a，根据时间段范围内的（不是小计合计的：SumType 空：原始数据 、 非空： 合计），b.上次小结结束
                        //复杂的合计，项目名是填写的，统计的项目分类也是手动填写的，比如：尿量、大便、呕吐量都为手写的随机行，有日期时间，
                        //统计的时候，统计项目也为手动填写，可能最后两行n行为不同的统计。这样的统计需要根据关键字进行分组统计，而且也要根据时间段
                    }
                }

                return sumAll.ToString();
            }

            return "";
        }

        /// <summary>
        /// 计算直接的表达式结果的扩展，
        /// 处理有些项目是输入框的名字来取值
        /// </summary>
        /// <param name="exeStr"></param>
        /// <returns></returns>
        private int Caculate_ExeculateStr(string exeStr, bool isTable)
        {
            int ret = 0;

            Control[] ct;

            string finalExeStr = exeStr;

            //MatchCollection matchs2 = Regex.Matches("(20 - [积极因子分1] +  - [积极因子分x])*2", @"(?<=\[)[^]]*?(?=\])");
            MatchCollection matchsExeStr = Regex.Matches(exeStr, @"(?<=\[)[^]]*?(?=\])"); //中括号[]把动态控件名包起来，已区分。用这个正则表达式得到所有的控件。 开始符号和结束符号是不一样的，所以能分割出来
            string eachValue = "";
            double valueEach = 0;

            CellInfor thisCell;

            if (matchsExeStr.Count > 0)
            {
                for (int i = 0; i < matchsExeStr.Count; i++)
                {
                    if (!isTable)
                    {
                        //非表格的时候
                        if (matchsExeStr[i].Value.StartsWith("%P"))  //Sum="∑[%P1.积极因子分4%]+(20 - [积极因子分1])*2"
                        {
                            //取指定页数据，进行合计
                            eachValue = ScriptPara(matchsExeStr[i].Value);

                            if (eachValue == "''") //如果找不到指定页，那么返回'',如果数据为空，则无法进行计算
                            {
                                eachValue = "";
                            }
                        }
                        else
                        {
                            //取本页数据，进行合计
                            //根据找到名字的输入框的内容来处理
                            ct = this.pictureBoxBackGround.Controls.Find(matchsExeStr[i].Value, false);

                            if (ct != null && ct.Length > 0)
                            {
                                eachValue = ct[0].Text.Trim();
                            }
                            else
                            {
                                eachValue = "";
                                ShowInforMsg("公式合计的时候，找不到控件：" + matchsExeStr[i].Value);

                                return 0;
                            }
                        }
                    }
                    else
                    {
                        //表格的时候，根据当前行列名来处理
                        thisCell = _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, matchsExeStr[i].Value);

                        if (thisCell != null)
                        {
                            eachValue = thisCell.Text.Trim();
                        }
                        else
                        {
                            eachValue = "";
                            ShowInforMsg("公式合计的时候，找不到列：" + matchsExeStr[i].Value);

                            return 0;
                        }
                    }

                    //根据内容，进行替换，计算
                    if (!string.IsNullOrEmpty(eachValue))
                    {
                        valueEach = StringNumber.getFirstNum(eachValue);

                        if (valueEach != -1)
                        {
                            finalExeStr = finalExeStr.Replace("[" + matchsExeStr[i].Value + "]", valueEach.ToString()); //将输入框名字，替换成输入框的值
                        }
                        else
                        {
                            //返回-1，表示字符串中没有数字，当作0来计算。在sum合计的时候，是跳过了。
                            finalExeStr = finalExeStr.Replace("[" + matchsExeStr[i].Value + "]", "0"); //将输入框名字，替换成输入框的值

                            ShowInforMsg("项目：『" + matchsExeStr[i].Value + "』中没有数字，不能正常计算，被当作：0来计算了。", false);
                        }
                    }
                    else
                    {
                        //finalExeStr = finalExeStr.Replace("[" + matchsExeStr[i].Value + "]", "0"); //为空不处理
                        string errMsg = "计算公式：" + finalExeStr + "中，『" + matchsExeStr[i].Value + "』的内容为空，不能进行计算。";
                        ShowInforMsg(errMsg, false);

                        return 0;
                    }
                }
            }

            ret = Evaluator.EvaluateToInteger(finalExeStr);

            return ret;
        }
        # endregion 合计

        # region 调整页位置
        /// <summary>
        /// 将第N页移动到M页
        /// 只使用与非表格样式，补填的一页不希望在最后位置，而是在指定的中间的某位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 调整页位置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SaveToOp();

                if (IsNeedSave)
                {
                    MessageBox.Show("请先保存本页数据后，再调整页位置。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //已经打开某表单
                //修改页码，不移动节点位置
                if (!_TableType && _CurrentPage != "" && _RecruitXML != null && _CurrentTemplateNameTreeNode != null)
                {
                    Frm_EditPageOrder epof = new Frm_EditPageOrder(_CurrentTemplateNameTreeNode.Nodes.Count, int.Parse(_CurrentPage));

                    DialogResult res = epof.ShowDialog();

                    int currentPage = int.Parse(_CurrentPage);

                    if (res == DialogResult.OK)
                    {
                        //1.修改<Page ： 要更新后面的SN
                        if (_RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms))) != null)
                        {
                            //修改页码
                            if (currentPage > epof.ID)
                            {
                                XmlNode thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), _CurrentPage, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));

                                int eachSn = 1;
                                //更新删除页后面的页号
                                if (_RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms))).ChildNodes.Count > 0)
                                {
                                    foreach (XmlNode xn in _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms))).ChildNodes)
                                    {
                                        eachSn = int.Parse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.SN)));
                                        if (eachSn >= epof.ID && eachSn <= currentPage)
                                        {
                                            (xn as XmlElement).SetAttribute(nameof(EntXmlModel.SN), (eachSn + 1).ToString());
                                        }
                                    }
                                }

                                if (thisPage != null)
                                {
                                    (thisPage as XmlElement).SetAttribute(nameof(EntXmlModel.SN), epof.ID.ToString());

                                    //为了更好，可以移动xml节点位置
                                }
                            }
                            else
                            {
                                XmlNode thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), _CurrentPage, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));

                                int eachSn = 1;
                                //更新删除页后面的页号
                                if (_RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms))).ChildNodes.Count > 0)
                                {
                                    foreach (XmlNode xn in _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms))).ChildNodes)
                                    {
                                        eachSn = int.Parse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.SN)));
                                        if (eachSn >= currentPage && eachSn <= epof.ID)
                                        {
                                            (xn as XmlElement).SetAttribute(nameof(EntXmlModel.SN), (eachSn - 1).ToString());
                                        }
                                    }
                                }

                                if (thisPage != null)
                                {
                                    (thisPage as XmlElement).SetAttribute(nameof(EntXmlModel.SN), epof.ID.ToString());

                                    //为了更好，可以移动xml节点位置
                                }
                            }

                            //2.继续根据移动后的位置打开本页
                            TreeNode tn = _CurrentTemplateNameTreeNode.Nodes[epof.ID - 1];

                            if (tn != null)
                            {
                                this.treeViewPatients.SelectedNode = tn;

                                //打开新建的第一页： 一共多少页等都不变不刷新了
                                treeViewPatients_NodeMouseDoubleClick(tn, null);
                            }

                            IsNeedSave = true;
                        }

                    }
                }
                else
                {
                    ShowInforMsg("无法进行调整页位置，非表格样式的表单才能执行此操作。");
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }
        # endregion 调整页位置

        # region 冻结图片、鼠标下拉拖动
        /// <summary>
        /// 按键的X坐标
        /// </summary>        
        private int Point_Y;

        private Image Frozen_Image = null;

        private int Frozen_Default_Height = 3;

        private bool DoubleClickMode = false;

        /// <summary>
        /// 鼠标左键按下，记录位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxFrozen_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.pictureBoxFrozen.Height <= Frozen_Default_Height)
            {
                //获取当前表单图片
                Frozen_Image = (Image)GetChartImage().Clone();
            }

            if (e.Button == MouseButtons.Left)
            {
                Point_Y = System.Windows.Forms.Control.MousePosition.Y;

                //拖动水平滚动条后，可能错位
                pictureBoxFrozen.Location = new Point(pictureBoxBackGround.Location.X + this.panelMain.Left, pictureBoxFrozen.Location.Y);
            }
        }

        /// <summary>
        /// 移动鼠标，控件往下变大
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxFrozen_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = System.Windows.Forms.Control.MousePosition;

                //高度最小为2，否则下次不能再看到了并拖动了
                if (this.pictureBoxFrozen.Height + (mousePos.Y - Point_Y) >= Frozen_Default_Height)
                {
                    this.pictureBoxFrozen.Height = this.pictureBoxFrozen.Height + (mousePos.Y - Point_Y);
                    Point_Y = mousePos.Y;

                    pictureBoxFrozen_MouseEnter(null, null); //更新下面的底分界线（拖动的边界）

                    this.pictureBoxFrozen.Refresh();
                }
            }
        }

        /// <summary>
        /// 绘制最底下的分界线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxFrozen_MouseEnter(object sender, EventArgs e)
        {
            //pictureBoxFrozen.Image = (Image)pictureBoxBackGround.Image.Clone(); 
            //pictureBoxFrozen.Image = GetChartImage();

            //更新冻结图像
            if (Frozen_Image != null)
            {
                pictureBoxFrozen.Image = (Image)Frozen_Image.Clone();
            }
            else
            {
                pictureBoxFrozen.Image = (Image)pictureBoxBackGround.Image.Clone();
            }

            //Graphics g = Graphics.FromImage(this.pictureBoxFrozen.Image);
            using (Graphics g = Graphics.FromImage(this.pictureBoxFrozen.Image))  //这种使用using的方式，在使用完成之后，自动回收，释放资源
            {

                Pen p = new Pen(Color.Gold, 2f);
                p.DashStyle = DashStyle.DashDot;

                g.DrawLine(p, new Point(0, this.pictureBoxFrozen.Height - 2), new Point(this.pictureBoxFrozen.Width, this.pictureBoxFrozen.Height - 2));

                p.Dispose();
                g.Dispose();
            }
        }

        /// <summary>
        /// 去除分界线（会导致分不清）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxFrozen_MouseLeave(object sender, EventArgs e)
        {
            if (pictureBoxFrozen.Height <= Frozen_Default_Height)
            {
                pictureBoxFrozen.Image = (Image)pictureBoxBackGround.Image.Clone(); //更新冻结图像
            }
        }

        /// <summary>
        /// 恢复默认大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxFrozen_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //自动到表格位置和默认隐藏位置进行切换
            if (!DoubleClickMode)
            {
                if (_TableType && _TableInfor != null)
                {
                    pictureBoxFrozen.Size = new Size(pictureBoxBackGround.Width, _TableLocation.Y + 1);
                }
                else
                {
                    pictureBoxFrozen.Size = new Size(pictureBoxBackGround.Width, Frozen_Default_Height);
                }
            }
            else
            {
                pictureBoxFrozen.Size = new Size(pictureBoxBackGround.Width, Frozen_Default_Height);
            }

            pictureBoxFrozen_MouseEnter(null, null); //更新下面的底分界线（拖动的边界）
            pictureBoxFrozen.Refresh();

            DoubleClickMode = !DoubleClickMode; //取反，下次双击效果交替
        }

        /// <summary>
        /// 显示冻结图片
        /// </summary>
        /// <param name="modePara">默认 / 自动显示到表格位置</param>
        private void ShowDefault(bool modePara)
        {

        }

        /// <summary>
        /// 拖动表单水品滚动条的时候，也跟着动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxBackGround_LocationChanged(object sender, EventArgs e)
        {
            //拖动水平滚动条后，可能错位
            pictureBoxFrozen.Location = new Point(pictureBoxBackGround.Location.X + this.panelMain.Left, pictureBoxFrozen.Location.Y);

            //if (!_IsLoading)
            //{
            //    DrawRowSignShow();
            //}
        }

        private void toolStripMenuItemHide_Click(object sender, EventArgs e)
        {
            pictureBoxFrozen.Size = new Size(pictureBoxBackGround.Width, Frozen_Default_Height);
        }

        private void toolStripMenuItemAuto_Click(object sender, EventArgs e)
        {
            if (_TableType && _TableInfor != null)
            {
                pictureBoxFrozen.Size = new Size(pictureBoxBackGround.Width, _TableLocation.Y + 1);
            }
            else
            {
                pictureBoxFrozen.Size = new Size(pictureBoxBackGround.Width, Frozen_Default_Height);
            }
        }

        # endregion 冻结图片、鼠标下拉拖动

        # region 新版矢量图 调用方法

        //private BorderDragger _borderDragger = new BorderDragger();
        ////private const string _strName = "矢量图的名称";
        private string _strImagePath;

        VectorControl _CurrentEditVector; //可能有多个矢量图，全局变量用来控制当前编辑的。双击编辑的时候，设置该全局变量

        //从数据库获取对应名的矢量图的：底图和预定义区域信息
        /// <summary>
        /// 矢量图的连接串
        /// </summary>
        string _ConnVector = "";
        //存放所有的矢量图配置信息
        private static ArrayList _VectorList = new ArrayList();

        /// <summary>
        /// 从数据库，根据矢量图名称，获取对应的底图、预定义内容
        /// </summary>
        /// <param name="vectorName"></param>
        /// <returns></returns>
        private DataTable GetBackGround_Predefined_FromDB(string vectorName)
        {
            //<!--矢量图背景图信息也是存在数据库中的：[mandala].[dbo].[VECTOR_LIBRARY] -->
            //<VectorLibrary ConnectionString="Mandala" />
            //XmlNode xnVectorLibrary = GlobalVariable.XmlNurseConfig.DocumentElement.SelectSingleNode("VectorLibrary");
            string conn = "";
            //if (xnVectorLibrary != null)
            //{
            //    conn = (xnVectorLibrary as XmlElement).GetAttribute("ConnectionString");
            //}
            if (string.IsNullOrEmpty(conn)) return null;
            _ConnVector = conn;

            DataTable retDt = new DataTable();

            //从数据库获取该表单数据：
            DataSet dSet = new DataSet(); //返回结果集

            string[] arrParam = new string[] { "" };
            Object[] arrValue = new Object[] { "" };
            arrParam = new string[] { "{v_vector_name}" };
            arrValue = new string[] { vectorName };
            string msg = "";
            int resultCount = -1;//GlobalVariable._serviceLava.UniversalSqlForOutsides(conn, _SqlSelect, arrParam, arrValue, ref dSet, ref msg);

            if (resultCount > 0)
            {
                if (dSet != null && dSet.Tables.Count > 0)
                {
                    retDt = dSet.Tables[0];

                    //进行过滤，只要公共（v_user为空或Null）和用户自己的（v_user为工号）
                    for (int i = retDt.Rows.Count - 1; i >= 0; i--)
                    {
                        if (retDt.Rows[i]["v_user"] == null || string.IsNullOrEmpty(retDt.Rows[i]["v_user"].ToString()))
                        {
                            continue;
                        }
                        else
                        {
                            if (retDt.Rows[i]["v_user"].ToString() == GlobalVariable.LoginUserInfo.UserCode)
                            {
                                if (retDt.Rows[i]["v_predefined_title"] != null && !string.IsNullOrEmpty(retDt.Rows[i]["v_predefined_title"].ToString()))
                                {
                                    retDt.Rows[i]["v_predefined_title"] = "*" + retDt.Rows[i]["v_predefined_title"].ToString(); //用户自定义的，程序显示的时候用星号开头
                                }

                                continue;
                            }
                            else
                            {
                                retDt.Rows.RemoveAt(i); //其他用户的删除
                            }
                        }

                    }

                    #region //更新内存中的矢量图配置数据
                    bool isFind = false;
                    for (int i = 0; i < _VectorList.Count; i++)
                    {
                        Struct.VectorInfor vi = (Struct.VectorInfor)_VectorList[i];
                        if (vi.VectorName == vectorName)
                        {
                            vi.VectorSettingDT = retDt;
                            isFind = true;
                            break;
                        }
                    }

                    //没有找到更新的话，就添加
                    if (!isFind)
                    {
                        Struct.VectorInfor vi = new Struct.VectorInfor();
                        vi.VectorName = vectorName;
                        vi.VectorSettingDT = retDt;
                        _VectorList.Add(vi);
                    }
                    #endregion //更新内存中的矢量图配置数据
                }
            }

            return retDt;
        }

        /// <summary>
        /// 根据矢量图名称，获取对应的底图、预定义内容
        /// </summary>
        /// <param name="vectorName"></param>
        /// <returns></returns>
        private DataTable GetBackGround_Predefined(string vectorName)
        {
            DataTable retDt = new DataTable();


            #region //获取矢量图配置信息

            bool isFind = false;
            for (int i = 0; i < _VectorList.Count; i++)
            {
                Struct.VectorInfor vi = (Struct.VectorInfor)_VectorList[i];
                if (vi.VectorName == vectorName)
                {
                    retDt = vi.VectorSettingDT; //vi.VectorSettingDT.Copy();
                    isFind = true;
                    break;
                }
            }

            //内存中没有找到的话，就从数据库检索
            if (!isFind)
            {
                retDt = GetBackGround_Predefined_FromDB(vectorName);
            }

            #endregion //获取矢量图配置信息

            //retDt.DefaultView.Sort = string.Format("v_user ASC");
            //retDt = retDt.DefaultView.ToTable(); //dtVector.DefaultView.ToTable().Copy();

            return retDt;
        }


        //打开编辑矢量图
        //每次先生成只有背景的图片
        private void EditVector(VectorControl thisVector)
        {
            //InitVector(false, thisVector, false);

            //_CurrentEditVector = thisVector;

            //if (!File.Exists(_strImagePath))
            //{
            //    //MessageBox
            //    return;
            //}

            ////以往的标注
            ////string strOldChart = thisVector._ImageBorder;

            //FileStream fs = new FileStream(_strImagePath, FileMode.Open, FileAccess.Read);
            //Image img = System.Drawing.Image.FromStream(fs);


            //_borderDragger.BorderAct(img, strOldChart, GlobalVariable.LoginUserInfo.UserCode); //_strImagePath
            ////_borderDragger.BorderAct(System.Drawing.Image.FromFile(_strImagePath), strOldChart, GlobalVariable.LoginUserInfo.UserCode); //这样读取会造成进程锁住文件，无法删除/替换

            ////如果复用界面操作后，已经将开头的星好过滤，再显示就没有了  _borderDragger.BorderSetPredefined(thisVector._ArrPredefinedTitle, thisVector._ArrPredefinedBody);
            ////string[] arrTitle = new string[thisVector._ArrPredefinedTitle.Length];
            ////thisVector._ArrPredefinedTitle.CopyTo(arrTitle, 0);
            ////_borderDragger.BorderSetPredefined(arrTitle, thisVector._ArrPredefinedBody);
            //_borderDragger.BorderSetPredefined(thisVector._ArrPredefinedTitle, thisVector._ArrPredefinedBody);

            //_borderDragger.ShowDialog();

            //fs.Close(); //关闭文件流
        }

        /// <summary>
        /// 双击遗传图谱，打开编辑窗体，进行修改
        /// 单机版不能运行的
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GeneticMap_DoubleClick(object sender, EventArgs e)
        {
            //GeneticMapControl me = (GeneticMapControl)sender;

            //Rings.VectorImageInfo imageInfo = new Rings.VectorImageInfo();
            //imageInfo.ImageJsonData = me.GetSaveData();

            //imageInfo.ImageFile = "";   // @"C:\Users\tanjunqi\Desktop\11.jpg"; //elControl.getAttribute("src", 0).ToString();
            //imageInfo.ImageSize = me.Size;

            //Rings.FormDrawingBoard formBoard;  // = new Rings.FormDrawingBoard(imageInfo);

            //if (!string.IsNullOrEmpty(imageInfo.ImageJsonData))
            //{
            //    formBoard = new Rings.FormDrawingBoard(imageInfo);
            //}
            //else
            //{
            //    formBoard = new Rings.FormDrawingBoard();
            //}

            ////Rings.DrawOption();


            //formBoard.Dispose();
        }

        /// <summary>
        /// 双击矢量图，打开编辑窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VectorControl_DoubleClick(object sender, EventArgs e)
        {
            VectorControl thisVector = (VectorControl)sender;
            if (GlobalVariable.Vector != null)
            {
                thisVector.VectorImageInfo = GlobalVariable.Vector.OpenVectorForm(
                    thisVector.VectorImageInfo,
                    _patientInfo.PatientId,
                    _TemplateName,
                    _TemplateID
                    );
                if (thisVector.VectorImageInfo != null && thisVector.VectorImageInfo.FinalImage != null)
                {
                    thisVector.BackgroundImage = thisVector.VectorImageInfo.FinalImage;
                }
            }
            //EditVector(thisVector); //先生成只有背景的图片
        }

        /// <summary>
        /// 初始化：矢量图控件。
        /// 为查看图片和编辑矢量图做好准备
        /// </summary>
        /// <param name="imageBorderFlg">是否绘制图层信息</param>
        /// <param name="thisVector">是否绘制图层信息</param>
        /// <param name="updateToOwnerFlg">是否更新背景图到控件</param>
        /// <returns></returns>
        private Image InitVector(bool imageBorderFlg, VectorControl thisVector, bool updateToOwnerFlg)
        {
            Image retImage = null;
            if (thisVector != null && thisVector.VectorImageInfo != null)
            {
                if (!string.IsNullOrEmpty(thisVector.VectorImageInfo.OriginalName) && thisVector.VectorImageInfo.OriginalIamge == null)
                {//初始化OriginalIamge
                    string tempString = "";
                    try
                    {
                        tempString = thisVector.VectorImageInfo.OriginalName;
                        if (Path.IsPathRooted(tempString))
                        {
                            tempString = Path.GetFileName(tempString);
                        }
                        thisVector.VectorImageInfo.OriginalIamge = Image.FromFile(Comm._Nurse_ImagePath + @"\" + tempString);
                    }
                    catch (Exception ex)
                    {
                        Comm.LogHelp.WriteErr("加载图片时异常，可能不存在在图片：" + Comm._Nurse_ImagePath + @"\" + tempString + "\r\n" + ex.StackTrace);
                    }
                }

                if (thisVector.VectorImageInfo.FinalImage == null && !string.IsNullOrEmpty(thisVector.VectorImageInfo.PicId))
                {//第一次加载有数据
                    if (GlobalVariable.Vector != null)
                    {
                        thisVector.VectorImageInfo = GlobalVariable.Vector.SelectImage(thisVector.VectorImageInfo.PicId, _patientInfo.PatientId, _TemplateName, _TemplateID, thisVector.VectorImageInfo.OriginalName);
                    }
                }
                if (thisVector.VectorImageInfo.FinalImage != null)
                {//有最终图的
                    retImage = thisVector.VectorImageInfo.FinalImage;
                }
                else if (thisVector.VectorImageInfo.OriginalIamge != null)
                {//有原始图的
                    retImage = thisVector.VectorImageInfo.OriginalIamge;
                }
            }
            if (updateToOwnerFlg)
            {
                thisVector.BackgroundImage = retImage;
            }

            return retImage;

            #region 老的
            /*
            try
            {
                //底图画笔 
                _borderDragger.SetPenVector(thisVector._PenVector.Color, thisVector._PenVector.Width);

                //矢量图的配置，TrackerColor为跟踪块的颜色，FloorColors为标注可选的边框颜色，TrackerSize为跟踪块的大小，有效值8~20。
                if (string.IsNullOrEmpty(thisVector._BorderUserConfig))
                {
                    _borderDragger.BorderUserConfig = "TrackerColor=SkyBlue;TrackerSize=11;FloorColors=Coral,Gold,SkyBlue,Green";
                    //_borderDragger.BorderUserConfig = "TrackerColor=Red;TrackerSize=11;FloorColors=Coral,Gold,SkyBlue,Green";
                }
                else
                {
                    _borderDragger.BorderUserConfig = thisVector._BorderUserConfig;
                }

                _borderDragger.BorderName = thisVector.Name;// _strName;

                if (string.IsNullOrEmpty(thisVector._BorderDenoteDiction))
                {
                    _borderDragger.BorderDenoteDiction = "矢量图的标注常用语|用语1|用语2|用语3";   //常用用语和填充颜色，可以以后再模板中配置传入扩展
                }
                else
                {
                    _borderDragger.BorderDenoteDiction = thisVector._BorderDenoteDiction;
                }

                string strBeginTime = string.Format("{0:yyyyMMdd HHmmssffff}", GetServerDate());

                //矢量图临时图片的目录
                //string strDirectory = Directory.GetCurrentDirectory() + "\\OccasionalFigures\\";
                string strDirectory = Comm._Nurse_ImagePath;


                if (!Directory.Exists(strDirectory))
                    Directory.CreateDirectory(strDirectory); //矢量图Dll内部还是Directory.GetCurrentDirectory() + "\\OccasionalFigures\\"这个目录


                // 矢量图临时图片的路径
                //_strImagePath = Directory.GetCurrentDirectory() + "\\OccasionalFigures\\_" + strBeginTime + "." + thisVector.Name + ".png";
                _strImagePath = strDirectory + "_" + strBeginTime + "." + thisVector.Name + ".png";

                #region 调试的固定参数代码
                /// 矢量图底图定义：图片长宽¤文本绘制|多边形示例绘制¤底图绘制
                //string strConfig = "(300,200)¤" +
                //    "肺示意图ABC[10,10]|右下对角线[10,30][30,30][30,40][10,40]|三角形[10,50][30,60][10,70]¤" + //有字就填充阴影
                //    "[137,20,152,76,152,76,105,78]" +
                //    "[105,78,111,50,111,50,101,42]" +
                //    "[101,42,90,34,75,35,65,42]" +
                //    "[65,42,54,50,36,132,33,156]" +
                //    "[33,156,31,180,108,195,112,156]" +
                //    "[112,156,112,125, 112,125,100,103]" +
                //    "[100,103,134,84,174,82,202,103]" +
                //    "[166,20,149,78,149,78,198,80]" +
                //    "[198,80,192,50,192,50,214,34]" +
                //    "[192,50,214,34,230,30,239,43]" +
                //    "[239,43,250,50,272,129,271,157]" +
                //    "[271,157,273,179,196,195,192,157]" +
                //    "[192,157,192,124,192,124,202,103]";

                //string strConfig = "(300,300)¤" +
                //    "人体正面示意图ABC[10,10]|右下对角线[10,30][30,30][30,40][10,40]|三角形[10,50][30,60][10,70]¤" + //有字就填充阴影
                //    "[100,74,40,0]" +
                //    "[74,115,74,115,19,133,19,133]" +
                //    "[19,133,5,150,5,150,25,161]" +
                //    "[25,161,25,161,67,148,67,148]" +
                //    "[67,148,67,148, 67,246,67,246]" +
                //    "[124,114,124,114,181,133,181,133]" +
                //    "[181,133,195,150,195,150,175,161]" +
                //    "[175,161,175,161,133,148,133,148]" +
                //    "[133,148,133,148,133,246,133,246]";
                #endregion 调试的固定参数代码

                string strConfig = thisVector._VectorConfig;

                // 以往的标注
                string strOldChart = "";

                if (imageBorderFlg)
                {
                    //strOldChart = "LayerStyle=2;Date=2016-11-11;User=Linys;Remark=第一层标注说明;Points=[20,20][50,20][50,50][20,50];Visible=true;IsLine=true;FloorColor=Coral;SeamInterval=16;SeamLength=8|LayerStyle=1;Date=2009-11-11;User=Linys;Remark=第二层标注说明;Points=[60,60][100,60][100,100][60,100];Visible=true;IsLine=false;FloorColor=Gold";
                    strOldChart = thisVector._ImageBorder;
                }

                /// 在硬盘上产生底图图片，如果成功，将形成_strImagePath指向的文件。这里如果传入了第三个参数获取全部图层的图片，再次编辑的时候，还要传入第三个参数空进行编辑
                _borderDragger.BorderDrawVector(strConfig, _strImagePath, strOldChart); //_borderDragger.BorderDrawVector(strConfig, _strImagePath, "") 第三个参数空,就是空的背景底图 在背景图上显示图层等矢量图数据： 以往的标注


                if (!File.Exists(_strImagePath))
                {
                    string errMsg = "无法产生图形，请检查原因。";
                    Comm.Logger.WriteErr(errMsg);
                    MessageBox.Show(errMsg, "矢量图", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return null;
                }

                if (updateToOwnerFlg)
                {
                    FileStream fs = new FileStream(_strImagePath, FileMode.Open, FileAccess.Read);
                    Image img = System.Drawing.Image.FromStream(fs);
                    fs.Close();

                    thisVector.BackgroundImage = img;
                    //thisVector.BackgroundImage = Image.FromFile(_strImagePath); 
                }

                return (Image)thisVector.BackgroundImage.Clone();
            }
            catch (Exception ex)
            {
                Comm.Logger.WriteErr(ex);

                throw ex;
            }*/
            #endregion
        }


        /// <summary>
        /// 在【矢量图】工具栏：按“应用”按钮触发的事件
        /// 在调用的表单这里，需要进行保存设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void borderDragger_FloorImageDone(object sender, EventArgs e)
        {
            try
            {
                string[] strXes = sender.ToString().Split('¤');

                if (strXes.Length != 2)
                    return; // 异常格式，返回，不处理

                //新图片文件的路径
                string strPath = strXes[0];

                if (File.Exists(strPath))
                {
                    FileStream fs = new FileStream(strPath, FileMode.Open, FileAccess.Read);
                    Image img = System.Drawing.Image.FromStream(fs);
                    fs.Close();

                    _CurrentEditVector.BackgroundImage = img;
                    //_CurrentEditVector.BackgroundImage = Image.FromFile(strPath); 

                    //_CurrentEditVector._ImageBorder = strXes[1];

                    IsNeedSave = true; //需要进行保存
                }

                //MessageBox.Show("这是用户绘制的标注：\r\n" + strXes[1], "矢量图", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// 在【矢量图】工具栏：按“复用”，在复用按钮弹出的窗口中，按“新建预定义区域+私有”、“新建预定义区域+公共”、“删除预定义区域”触发的事件
        /// 将选择的图层设置为预定义,保存到数据库。
        /// CREATE TABLE [dbo].[VECTOR_LIBRARY](
        ///[sn] [int] IDENTITY(1,1) NOT NULL,
        ///[v_vector_name] [nvarchar](50) NOT NULL,
        ///[v_predefined_title] [nvarchar](50) NULL,
        ///[v_offices] [nvarchar](200) NULL,
        ///[v_user] [nvarchar](50) NULL,
        ///[v_remark] [nvarchar](50) NULL,
        ///[v_figure] [nvarchar](max) NULL
        ///) 
        private void borderDragger_FloorPredefinedDone(object sender, EventArgs e)
        {
            //try
            //{
            //    //如果找不到当前操作的矢量图对象，那么异常。要获取名字，并且添加删除后，要更新对象属性的预定义区域信息
            //    if (_CurrentEditVector == null || _CurrentEditVector.IsDisposed)
            //    {
            //        return;
            //    }

            //    string msg = "";
            //    string[] strXes = sender.ToString().Split('¤');

            //    DataTable dtVector = null;

            //    //12¤第二层标注说明[60,60][100,60][100,100][60,100]¤0  ； 图层名 内容 0私有 2共有
            //    if (strXes.Length == 1)
            //    {
            //        //MessageBox.Show("表示要删除外部的预定义区的存储。", "矢量图", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            //        //XmlNode xnVectorLibrary = GlobalVariable.XmlNurseConfig.DocumentElement.SelectSingleNode("VectorLibrary");
            //        string conn = "";
            //        //if (xnVectorLibrary != null)
            //        //{
            //        //    conn = (xnVectorLibrary as XmlElement).GetAttribute("ConnectionString");
            //        //}
            //        if (string.IsNullOrEmpty(conn)) return;
            //        string[] arrParam = new string[] { "", "" };
            //        object[] arrValue = new object[] { "", "" };

            //        arrParam = new string[] { "{v_vector_name}", "{v_predefined_title}" };
            //        arrValue = new object[] { _CurrentEditVector.Name, strXes[0] };
            //        string errMsg = "";
            //        int result = -1;// GlobalVariable._serviceLava.UniversalSqlForOutsidesNoQuery(conn, _SqlDelete_Predefined, arrParam, arrValue, ref errMsg);

            //        if (result <= 0)
            //        {
            //            msg = "删除预定义区域的时候，出现异常：" + result.ToString();
            //        }
            //        else
            //        {
            //            msg = "成功删除预定义区域。";

            //            //更新对象中的预定义，否则要下次加载，才能更新为db中的
            //            if (_CurrentEditVector._ArrPredefinedTitle != null)
            //            {

            //                for (int i = 0; i < _CurrentEditVector._ArrPredefinedTitle.Length; i++)
            //                {
            //                    if (_CurrentEditVector._ArrPredefinedTitle[i] == strXes[0] || _CurrentEditVector._ArrPredefinedTitle[i] == "*" + strXes[0]) // 添加删除的时候，如果title同名，就会有问题
            //                    {
            //                        _CurrentEditVector._ArrPredefinedTitle = DeleteString(_CurrentEditVector._ArrPredefinedTitle, strXes[0]); //删除数据中的某一项
            //                        _CurrentEditVector._ArrPredefinedBody = DeleteString(_CurrentEditVector._ArrPredefinedBody, i);

            //                        //更新缓存中用户自定义区域信息
            //                        dtVector = GetBackGround_Predefined(_CurrentEditVector._VectorName);
            //                        if (dtVector != null && dtVector.Rows.Count > 0)
            //                        {
            //                            for (int j = 0; j < dtVector.Rows.Count; j++)
            //                            {
            //                                if (dtVector.Rows[j]["v_predefined_title"] != null &&
            //                                    (dtVector.Rows[j]["v_predefined_title"].ToString() == strXes[0] || dtVector.Rows[j]["v_predefined_title"].ToString() == "*" + strXes[0]))
            //                                {
            //                                    dtVector.Rows.RemoveAt(j);

            //                                    //SetPredefined(_CurrentEditVector, dtVector); //更新矢量图的自定义数组变量
            //                                    break;
            //                                }
            //                            }
            //                        }

            //                        break;
            //                    }
            //                }


            //            }
            //        }
            //    }
            //    else if (strXes.Length == 3)
            //    {
            //        //XmlNode xnVectorLibrary = GlobalVariable.XmlNurseConfig.DocumentElement.SelectSingleNode("VectorLibrary");
            //        string conn = "";
            //        //if (xnVectorLibrary != null)
            //        //{
            //        //    conn = (xnVectorLibrary as XmlElement).GetAttribute("ConnectionString");
            //        //}
            //        if (string.IsNullOrEmpty(conn)) return;
            //        //string _SqlInsert_Predefined = "INSERT INTO VECTOR_LIBRARY (v_vector_name,v_predefined_title,v_user,v_figure) values(?,?,?,?)"; 
            //        string[] arrParam = new string[] { "", "", "", "" };
            //        object[] arrValue = new object[] { "", "", "", "" };

            //        arrParam = new string[] { "{v_vector_name}", "{v_predefined_title}", "{v_user}", "{v_figure}" };

            //        if (strXes[2] == "2")
            //        {
            //            //MessageBox.Show("这表示预定义区为公共：\r\n" + strXes[1], "矢量图", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            //            arrValue = new object[] { _CurrentEditVector.Name, strXes[0], "", strXes[1] };

            //        }
            //        else
            //        {
            //            //MessageBox.Show("这表示预定义区为私有：\r\n" + strXes[1], "矢量图", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            //            arrValue = new object[] { _CurrentEditVector.Name, strXes[0], GlobalVariable.LoginUserInfo.UserCode, strXes[1] };
            //        }

            //        string errMsg = "";
            //        int result = -1;// GlobalVariable._serviceLava.UniversalSqlForOutsidesNoQuery(conn, _SqlInsert_Predefined, arrParam, arrValue, ref errMsg);

            //        if (result <= 0)
            //        {
            //            msg = "添加预定义区域的时候，出现异常：" + result.ToString();
            //        }
            //        else
            //        {
            //            msg = "成功添加预定义区域。";

            //            //更新对象中的预定义，否则要下次加载才能更新为db中的
            //            if (_CurrentEditVector._ArrPredefinedTitle != null)
            //            {

            //                string[] arr1 = new string[_CurrentEditVector._ArrPredefinedTitle.Length + 1];
            //                string[] arr2 = new string[_CurrentEditVector._ArrPredefinedBody.Length + 1];

            //                for (int i = 0; i < _CurrentEditVector._ArrPredefinedTitle.Length; i++)
            //                {
            //                    arr1[i] = _CurrentEditVector._ArrPredefinedTitle[i];
            //                    arr2[i] = _CurrentEditVector._ArrPredefinedBody[i];
            //                }

            //                if (strXes[2] == "2")
            //                {
            //                    arr1[arr1.Length - 1] = strXes[0];
            //                }
            //                else
            //                {
            //                    arr1[arr1.Length - 1] = "*" + strXes[0];
            //                }

            //                arr2[arr1.Length - 1] = strXes[1];

            //                _CurrentEditVector._ArrPredefinedTitle = arr1;
            //                _CurrentEditVector._ArrPredefinedBody = arr2;

            //                //更新缓存中用户自定义区域信息
            //                dtVector = GetBackGround_Predefined(_CurrentEditVector._VectorName);
            //                if (dtVector != null)
            //                {
            //                    dtVector.Rows.Add();
            //                    dtVector.Rows[dtVector.Rows.Count - 1]["v_predefined_title"] = arr1[arr1.Length - 1];
            //                    dtVector.Rows[dtVector.Rows.Count - 1]["v_figure"] = arr2[arr1.Length - 1];

            //                    if (strXes[2] == "2")
            //                    {
            //                        dtVector.Rows[dtVector.Rows.Count - 1]["v_user"] = "";
            //                    }
            //                    else
            //                    {
            //                        dtVector.Rows[dtVector.Rows.Count - 1]["v_user"] = GlobalVariable.LoginUserInfo.UserCode;
            //                    }

            //                    //SetPredefined(_CurrentEditVector, dtVector); //更新矢量图的自定义数组变量

            //                    //排序
            //                    dtVector.DefaultView.Sort = string.Format("v_user ASC");
            //                    dtVector = dtVector.DefaultView.ToTable(); //dtVector.DefaultView.ToTable().Copy();
            //                }
            //            }
            //        }


            //    }

            //    if (dtVector != null)
            //    {
            //        //更新矢量图控件：自定义区域的变量和自定义菜单
            //        SetPredefined(_CurrentEditVector, dtVector); //更新矢量图的自定义数组变量

            //        _borderDragger.BorderSetPredefined(_CurrentEditVector._ArrPredefinedTitle, _CurrentEditVector._ArrPredefinedBody); //更新自定义区域的菜单
            //    }

            //    if (!string.IsNullOrEmpty(msg))
            //    {
            //        ShowInforMsg(msg, false);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Comm.Logger.WriteErr(ex);
            //    throw ex;
            //}
        }

        private void SetPredefined(VectorControl tempVectorControl, DataTable dtVector)
        {
            //if (dtVector.Rows.Count == 1) //至少有一条数据配置矢量底图
            //{
            //    tempVectorControl.VectorImageInfo.PicId = dtVector.Rows[0]["v_figure"].ToString();
            //    //tempVectorControl._ArrPredefinedTitle = null;
            //    //tempVectorControl._ArrPredefinedBody = null;
            //}
            //else
            //{
            //    //tempVectorControl._ArrPredefinedTitle = new string[dtVector.Rows.Count - 1]; //私有的自定义区域，开头加星号*
            //    //tempVectorControl._ArrPredefinedBody = new string[dtVector.Rows.Count - 1];
            //    int indexPredefined = 0;

            //    for (int k = 0; k < dtVector.Rows.Count; k++)
            //    {
            //        if (string.IsNullOrEmpty(dtVector.Rows[k]["v_predefined_title"].ToString()))
            //        {
            //            //底图的时候，底图肯定有，预定义区域可能没有，就为null
            //            tempVectorControl._VectorConfig = dtVector.Rows[k]["v_figure"].ToString();
            //        }
            //        else
            //        {
            //            //预定义区域的时候
            //            tempVectorControl._ArrPredefinedTitle[indexPredefined] = dtVector.Rows[k]["v_predefined_title"].ToString();
            //            tempVectorControl._ArrPredefinedBody[indexPredefined] = dtVector.Rows[k]["v_figure"].ToString();

            //            indexPredefined++;
            //        }
            //    }
            //}
        }


        /// <summary>
        /// 删除用户自定义区域的时候，删除存储变量的数组
        /// </summary>
        /// <param name="list"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string[] DeleteString(string[] list, string key)
        {
            List<string> l = new List<string>();
            foreach (string s in list)
            {
                if (s != key) l.Add(s);
            }

            return l.ToArray();
        }

        private string[] DeleteString(string[] list, int key)
        {
            List<string> l = new List<string>();
            int index = 0;
            foreach (string s in list)
            {
                if (index != key) l.Add(s);

                index++;
            }

            return l.ToArray();
        }


        #endregion  新版矢量图 调用方法 -------------------------------------------

        #region 其他

        /// <summary>
        /// 执行保存时是插入还是更新，外部属性节点更新用
        /// </summary>
        private string _strInsertOrUpdate = string.Empty;

        private bool _IsLoaded = false;

        private void JoinTemplateAndXml()
        {
            string strData = GlobalVariable.NurseFormValue;
            if (string.IsNullOrEmpty(strData))
            {
                IsNeedSave = true;
                _IsCreatedTemplate = true;

                ////没有检索到，那么创建一个空的xml数据文件
                TemplateHelp mx = new TemplateHelp();
                _RecruitXML = mx.CreateEmptyTemplate(this._patientInfo.PatientId, this._patientInfo.PatientName, GlobalVariable.LoginUserInfo.UserCode, GlobalVariable.LoginUserInfo.DeptName, _TemplateXml, _TemplateName);

                int creatPages = 1;

                _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.FORM_TYPE), _TemplateRight);
                _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Forms), creatPages.ToString());
            }
            else
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strData);
                //xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + strData);
                string bookNodeName = "//" + nameof(EntXmlModel.NurseForm);

                strData = xmlDoc.SelectSingleNode(bookNodeName).OuterXml;

                _RecruitXML = new XmlDocument();
                _RecruitXML.LoadXml(strData);

                ////statusArr更新归档和转科病区 nameof(EntXmlModel.Done) nameof(EntXmlModel.Type)
                _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Done), "");
                _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Type), "");
            }

        }

        private bool _IsSaveingNow = false;


        private bool SaveToDetail(bool needCheckUser)
        {
            try
            {
                bool ret = true;

                this.pictureBoxBackGround.Focus();

                if (_IsLocked)
                {
                    if (!_LordClosing)
                    {
                        Comm.ShowErrorMessage("该表单已经被锁住，不能进行保存。");
                    }
                    return false;
                }

                if (_SaveNeedLogin && needCheckUser)    //保存登录密码验证
                {
                    if (_LordClosing)
                    {
                        return false; //没有通过验证
                    }

                    if (CheckHelp.CheckCurrentLoginUser(GlobalVariable.LoginUserInfo.UserCode, this))
                    {
                        //通过验证
                    }
                    else
                    {
                        return false; //没有通过验证
                    }
                }

                _IsSaveingNow = true;

                SaveToOp();   //保存前处理，防止光标还在单元格，而且内容已经修改

                if (!this.toolStripSaveEnabled)
                {
                    Comm.ShowErrorMessage(Consts.Msg_NoRightSave);
                    return false; //如果，医生或者，护理状态无效的时候，不要进行保存，直接跳出
                }

                //先移除这个搜索提示控件
                if (this.pictureBoxBackGround.Controls.Contains(this.lblShowSearch))
                {
                    this.pictureBoxBackGround.Controls.Remove(this.lblShowSearch);
                }

                //是否存在不合法的输入数据，有错误不能保存，跳出。
                if (_ErrorList.Count > 0)
                {
                    string errMsg = "";
                    for (int i = 0; i < _ErrorList.Count; i++)
                    {
                        errMsg += "\r\n" + _ErrorList[i];
                    }

                    _IsSaveingNow = false;

                    Comm.LogHelp.WriteErr("保存失败，因为存在不合理的数据：" + errMsg);
                    ShowInforMsgExpand(errMsg);

                    if (!_LordClosing)
                    {
                        Comm.ShowErrorMessage("保存失败，因为存在不合理的数据，请先修改后再保存。", "保存失败");
                    }
                    return false; //如果切换其他页，提示保存的时候选择了【是】，但是发现错误不能报错，需要做处理
                }
                else
                {

                    //在删除行的情况下，表格的第一行可能还是空的（光标没有进入，没有上面错误信息）
                    if (_TableType && _TableInfor != null)  //如果含有日期列
                    {
                        if (_FirstRowMustDate && ((_TableInfor.CellByRowNo_ColName(0, "日期") != null && _TableInfor.CellByRowNo_ColName(0, "日期").Text.Trim() == "")
                            || (_TableInfor.CellByRowNo_ColName(0, "时间") != null && _TableInfor.CellByRowNo_ColName(0, "时间").Text.Trim() == "")))
                        {

                            string errMsg = "每页的第1行中的日期和时间不能为空。";
                            Comm.LogHelp.WriteErr("保存失败，因为存在不合理的数据：" + errMsg);
                            ShowInforMsgExpand(errMsg);

                            if (!_LordClosing)
                            {
                                Comm.ShowErrorMessage(errMsg, "保存失败");
                            }

                            return false; //如果切换其他页，提示保存的时候选择了【是】，但是发现错误不能报错，需要做处理
                        }
                    }
                }

                //保存脚本验证：保存前
                #region 保存事件的脚本验证
                if (_node_Script != null && _node_Script.ChildNodes.Count > 0)
                {
                    StringBuilder messageList = new StringBuilder(); //一张表单多个人填写，有40个必填项，一个一个提示太麻烦。一起提示消息

                    ////为了提高速度，一次性遍历所有控件后，将控件存入字典，以后从字典中获取。
                    //Dictionary<string, Control> controlsDic = new Dictionary<string, Control>();
                    //foreach (Control clEach in this.pictureBoxBackGround.Controls)
                    //{
                    //    if (!controlsDic.ContainsKey(clEach.Name))
                    //    {
                    //        controlsDic.Add(clEach.Name, clEach);
                    //    }
                    //}

                    //Evaluator.controlsDic = controlsDic;

                    this.Cursor = Cursors.WaitCursor;

                    Control[] cl = null;    //new Control[1];
                    bool isTableScript = false;
                    bool contineEmptyRow = true;

                    foreach (XmlNode xn in _node_Script.ChildNodes)
                    {
                        //如果是注释那么跳过
                        if (xn.Name == @"#comment" || xn.Name == @"#text")
                        {
                            continue;
                        }

                        if (ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains("Save"))
                        {
                            isTableScript = false;
                            if (!bool.TryParse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.IsTable)), out isTableScript))
                            {
                                isTableScript = false;
                            }

                            if (!isTableScript)
                            {

                                //性能比较慢：
                                cl = this.pictureBoxBackGround.Controls.Find((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)), false);
                                //if (controlsDic.ContainsKey((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name))))
                                //{
                                //    //cl = new Control[1];
                                //    cl[0] = controlsDic[(xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name))];
                                //}
                                //else
                                //{
                                //    cl = null;
                                //}

                                if (cl == null || cl.Length == 0)
                                {
                                    continue;
                                }

                                //找到当前控件，对应的脚本，多个事件合并共用
                                //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == ctl.Name && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)) == "Save")
                                if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == cl[0].Name)
                                {
                                    //不是输入框，或非表格的输入框，执行这里。排除表格的单元格输入框的时候
                                    if (!(cl[0] is RichTextBoxExtended) || (cl[0] is RichTextBoxExtended && !((RichTextBoxExtended)cl[0])._IsTable && !isTableScript))
                                    {
                                        Evaluator.messageList = ""; //先清空消息，防止干扰

                                        //if (!Evaluator.EvaluateRunString(this, ScriptPara(xn.InnerText.Trim())))
                                        if (!Evaluator.EvaluateRunString(this.pictureBoxBackGround, ScriptPara(xn.InnerText.Trim())))
                                        {
                                            _IsSaveingNow = false;
                                            //Evaluator.controlsDic.Clear();
                                            this.Cursor = Cursors.Default;

                                            //焦点设置：
                                            if (cl != null && cl.Length > 0)
                                            {
                                                cl[0].Focus();

                                                //指示手指的位置
                                                //ShowSearch(cl[0]);
                                            }

                                            return false;
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(Evaluator.messageList))
                                            {
                                                if (string.IsNullOrEmpty(messageList.ToString()))
                                                {
                                                    messageList.Append(Evaluator.messageList);
                                                }
                                                else
                                                {
                                                    messageList.Append("\r\n");
                                                    messageList.Append(Evaluator.messageList);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else    //表格脚本
                            {
                                if (_TableType && _TableInfor != null)
                                {
                                    ////表格的表单，在保存的时候脚本，没有当前行的概念。那么遍历本页的所有行进行循环进行判断。（从最后一个非空行开始判断）
                                    ////if (_CurrentCellIndex.X != -1 && _TableInfor.CurrentCell != null)

                                    //if (!Evaluator.EvaluateRunString(_TableInfor, xn.InnerText.Trim()))
                                    //{
                                    //    _IsSaveingNow = false;
                                    //    return false;
                                    //}

                                    //<!--保存的时候，表格的行除日期、时间和签名外不能都为空，否则提示不能保存。-->
                                    //<Action Name="" IsTable="True" Event="Save">出量1.Text == '' And 出量2.Text == '' And 出量3.Text == '' And 出量4.Text == '' And 入量.Text == '' Then Error(空行，或者只有日期、时间或签名的行。)</Action>

                                    //<!--保存的时候，除日期、时间、调整段落的列和签名外的一列有输入内容，但是签名为空，那么提示要签名。Save事件的表格脚本，会遍历本页所有行进行判断。其他的针对当前行进行处理-->
                                    //<Action Name="" IsTable="True" Event="Save">出量1.Text != '' And 签名.Text == '' Then Error(有行需要进行签名)</Action>  //如果开启了保存自动签名，那么不需要配置这样的提示签名的验证了。或者改成Warn
                                    //<Action Name="" IsTable="True" Event="Save">出量2.Text != '' And 签名.Text == '' Then Error(有行需要进行签名)</Action>
                                    //<Action Name="" IsTable="True" Event="Save">出量3.Text != '' And 签名.Text == '' Then Error(有行需要进行签名)</Action>
                                    //<Action Name="" IsTable="True" Event="Save">出量4.Text != '' And 签名.Text == '' Then Error(有行需要进行签名)</Action>
                                    contineEmptyRow = true;
                                    for (int i = _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1; i >= 0; i--)
                                    {
                                        if (contineEmptyRow && IsEmptyRow(i))
                                        {
                                            continue;//跳过末尾的空白航
                                        }
                                        else
                                        {
                                            contineEmptyRow = false; //遇到第一行非空白行，就不要跳过了
                                        }

                                        _TableInfor.CurrentCell = _TableInfor.Cells[i, 0];
                                        _CurrentCellIndex = new Point(i, 0);  //记录下当前操作的单元格

                                        if (!Evaluator.EvaluateRunString(_TableInfor, xn.InnerText.Trim()))
                                        {

                                            ShowSearchRow(); //指示行

                                            this.Cursor = Cursors.Default;
                                            _TableInfor.CurrentCell = null;
                                            _CurrentCellIndex = new Point(-1, -1);  //记录下当前操作的单元格
                                            _IsSaveingNow = false;
                                            return false;
                                        }
                                    }

                                    _TableInfor.CurrentCell = null;
                                    _CurrentCellIndex = new Point(-1, -1);  //记录下当前操作的单元格
                                }
                            }
                        }
                    }

                    this.Cursor = Cursors.Default;

                    if (!string.IsNullOrEmpty(messageList.ToString()))
                    {
                        Comm.ShowInfoMessage(messageList.ToString());
                    }
                }
                #endregion 保存事件的脚本验证

                //保存时自动签名
                if (!_IsCASignAutoSave)
                {
                    SaveAutoSign();
                }

                //日期时间的早晚，顺序验证
                if (_TableType && _TableInfor != null && _TableInfor.CellByRowNo_ColName(0, "日期") != null)  //如果含有日期列
                {
                    DateTime dtPreRow = DateTime.Parse("1900-01-01 12:00");
                    DateTime dtCurRow = dtPreRow;
                    int fieldIndex = 0;
                    string errMsg = "";

                    //string time = "";
                    //bool isHaveTime = false;

                    //if (_TableInfor.CellByRowNo_ColName(0, "时间") != null)
                    //{
                    //    isHaveTime = true;
                    //}


                    //考虑到表格分栏的情况：
                    for (int field = 0; field < _TableInfor.GroupColumnNum; field++)
                    {
                        fieldIndex = _TableInfor.RowsCount * field;

                        for (int i = 0; i < _TableInfor.RowsCount; i++)
                        {
                            //if (isHaveTime)
                            //{
                            //    time = _TableInfor.CellByRowNo_ColName(i + fieldIndex, "时间").Text; //但是日期可能会空，又要去上一行的有效日期，，效率
                            //}

                            if (!DateTime.TryParse(_TableInfor.CellByRowNo_ColName(i + fieldIndex, "日期").Text, out dtCurRow))
                            {
                                //日期格式不正确的情况下，不做处理。理论上应该都已经是验证过正确格式的了
                            }
                            else
                            {
                                //当前行日期有效的情况下，进行比较
                                if (dtPreRow.Year != 1900) //第一行有效行日期以后才要
                                {
                                    if (dtCurRow.CompareTo(dtPreRow) < 0)
                                    {
                                        _IsSaveingNow = false;

                                        errMsg = "第" + (i + fieldIndex + 1).ToString() + "行的日期，早于前面的行的日期。";
                                        Comm.LogHelp.WriteErr(errMsg);
                                        ShowInforMsgExpand(errMsg);

                                        if (!_LordClosing)
                                        {
                                            Comm.ShowInfoMessage(errMsg);
                                        }

                                        break; //return false; //如果切换其他页，提示保存的时候选择了【是】，但是发现错误不能报错，需要做处理,不然就离开了

                                    }
                                }

                                dtPreRow = dtCurRow;
                            }
                        }
                    }
                }

                if (_SaveAutoSort)
                {
                    //if (MessageBox.Show("确定要进行排序吗？", "自动排序", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    if (_TableType && _TableInfor != null && _TableInfor.CellByRowNo_ColName(0, "日期") != null)  //如果含有日期列
                    {
                        //先判断数据是否错乱，需要按照日期和时间进行排序

                        //如果要更加理想化，那么需要对配需后的数据：1.每页首行日期（补日期），2.段落行的日期（同一页要去日期，只保留每页同段落的第一行）

                        if (_LordClosing || Comm.ShowQuestionMessage("是否需要自动排序？", "补录时，请填写日期和时间", 2) == DialogResult.Yes)
                        {
                            SortXMLDoc(_RecruitXML);
                        }
                    }
                }

                //如果是表格样式的表单，最好还是做当前页都不是空白行的验证检查，否则保存后，这一页还是没有。
                if (!string.IsNullOrEmpty(_TemplateName))
                {

                    this.Cursor = Cursors.WaitCursor;
                    toolStripSaveEnabled = false;

                    //将数据保存到内存变量的xml中
                    SaveThisPageDataToXml();


                    #region // -----------------善后处理：提交数据库保存的xml要删除最后面的空白节点
                    //如果xml最后的行是空行，那就是程序一开始为了方便处理加上的，要先删除后再保存；同时，如果删除行的ID等于MaxID，那么MaxID也要减1

                    //防止保存后，不重新打开还继续操作，按么空白节点还是要存在的，所以先做好备份
                    string xmlInnerXml = _RecruitXML.InnerXml;

                    RemoveEmptyRows(); //删除末尾的空白行后再保存

                    #endregion // -----------------善后处理善后处理


                    //保存前，还应该和体温单等一样，做本地备份，这个单机版的保存，可以扩展成整合后的本地备份  //保存到本地：保存的数据要删除后面的空白节点                      
                    ret = SaveFormToDB();

                    if (ret)
                    {
                        int sequenceNo = 0;
                        if (!int.TryParse(_RecruitXML.DocumentElement.GetAttribute(nameof(EntXmlModel.SequenceNo)), out sequenceNo))
                        {
                            sequenceNo = 0;
                        }

                        _RecruitXML.InnerXml = xmlInnerXml;

                        _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.SequenceNo), sequenceNo.ToString());
                    }
                    else
                    {

                        _RecruitXML.InnerXml = xmlInnerXml;             //还原没有删除空白节点前的xml数据，否则继续操作会报错（前面页有空白行的话），后面页的行，顶到前面页去了

                    }
                }
                else
                {
                    //不需要进行保存，或者没有打开表单就返回
                    toolStripSaveEnabled = true;
                    this.Cursor = Cursors.Default;
                    _IsSaveingNow = false;
                    return false;
                }

                _IsSaveingNow = false;
                this.Cursor = Cursors.Default;
                toolStripSaveEnabled = true;

                //如果保存成功，才需要重置：需要保存，是创建新页，新创建表单
                if (ret)
                {
                    IsNeedSave = false;
                    _IsCreated = false;
                    _IsCreatedTemplate = false;

                    //保存脚本验证：成功保存后
                    //保存事件的脚本验证
                    SavedScript("Saved");

                    SavedScript("SavedChanged"); //和上次保存的值不一样的情况下。 用_RecruitXML_Org来比较保存前的数据
                }

                _RecruitXML_Org.InnerXml = _RecruitXML.InnerXml;//保存后，更新备份比较文件。

                return ret;
            }
            catch (Exception ex)
            {
                _IsSaveingNow = false;
                this.Cursor = Cursors.Default;
                toolStripSaveEnabled = true;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        private bool SaveFormToDB()
        {

            //设置总页数和权限：[FORM_TYPE],[Pages]
            int pages = this.uc_Pager1.PageCount;
            if (pages == 0)
            {
                pages = 1; //不可能为0，至少为1.
            }

            _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.FORM_TYPE), _TemplateRight);
            _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Forms), pages.ToString());

            if (string.IsNullOrEmpty(_RecruitXML.DocumentElement.GetAttribute(nameof(EntXmlModel.FormName))))
            {
                _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.FormName), _TemplateName); //服务端保存时是会设置的，但是新建保存，客户端第二次保存就是为空了。
            }

            if (string.IsNullOrEmpty(_RecruitXML.DocumentElement.GetAttribute(nameof(EntXmlModel.Name))))
            {
                _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Name), this._patientInfo.PatientName); //新建的名字可能为空
            }

            string msg = "";
            //string[] array1 = new string[] { this._patientInfo.Id, this._patientInfo.Id, _RecruitXML.DocumentElement.OuterXml, GlobalVariable.UserArgs.UserUserName, patientsListView.GetPatientsInforFromName(_PatientsInforDT, this._patientInfo.Id, "姓名"), _TemplateName };
            string userName = GlobalVariable.LoginUserInfo.UserName;
            string[] array1 = new string[] { string.Empty, this._patientInfo.PatientId, _RecruitXML.DocumentElement.OuterXml, userName, this._patientInfo.PatientName, _TemplateName };

            ////string[] array2 = new string[] { "2[七病区]379700039375", str2 };
            //string[][] myarray = new string[1][]; ////    string[][] myarray = new string[2][];
            //myarray[0] = array1;

            //更新最大唯一号
            //保存表格数据
            if (_TableType && _TableInfor != null)
            {
                XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                int maxId = GetTableRows();
                (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), maxId.ToString());
            }

            //使用以下方法可以准确的记录代码运行的耗时。
            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start(); //  开始监视代码运行时间

            int sequenceNo = 0;
            if (!int.TryParse(_RecruitXML.DocumentElement.GetAttribute(nameof(EntXmlModel.SequenceNo)), out sequenceNo))
            {
                sequenceNo = 0;
            }


            string value = "";
            if (_strInsertOrUpdate == "insert")
            {
                value = RecruitNewToString_Insert();
            }
            else
            {
                value = RecruitNewToString_Update();
            }

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            GlobalVariable.SaveCancelEventArgs saveArgs = null;
            //前面的准备工作已完成，开始保存了，在此处触发保存前事件
            if (this.BeforeSave != null)
            {
                saveArgs = new GlobalVariable.SaveCancelEventArgs(value,
                    this._TemplateName,
                    this._TemplateID,
                    this._patientInfo);
                this.BeforeSave(this, saveArgs);
            }
            if (saveArgs != null && saveArgs.Cancel == true)
            {
                return false;
            }

            //bool operateState = GlobalVariable._serviceLava.doInsertRecruitNew(array1, ref msg, ref sequenceNo);
            bool operateState = false;// this.doInsertRecruitNew(array1, ref msg, ref sequenceNo);

            if (this.Save != null)
            {
                //保存时，触发矢量图的保存事件
                if (GlobalVariable.Vector != null)
                {
                    var vectorList = this.pictureBoxBackGround.Controls.Cast<Control>().Where(a => a is VectorControl && (a as VectorControl).Tag != null);
                    bool isVectorSaveFlag = true;
                    if (vectorList != null && vectorList.Any())
                    {
                        foreach (VectorControl v in vectorList)
                        {
                            if (string.IsNullOrEmpty(v.PicId) || v.VectorImageInfo.ActionByte == null || v.VectorImageInfo.ActionByte.Length == 0)
                                continue;
                            isVectorSaveFlag = GlobalVariable.Vector.SaveVectorImage(v.VectorImageInfo, _patientInfo.PatientId, _TemplateName, _TemplateID);
                            if (!isVectorSaveFlag)
                            {
                                msg = string.Format("保存矢量图失败：patientid={0} , image name={1} , picid={2}", _patientInfo.PatientId, v.Name, v.PicId);
                                break;
                            }
                        }
                    }
                    if (!isVectorSaveFlag)
                    {
                        ShowInforMsg(msg, true);
                        //Comm.ShowErrorMessage(msg);
                        return false;
                    }
                }
                operateState = this.Save(this, new GlobalVariable.SaveEventArgs(
                    value,
                    this._TemplateName,
                    this._TemplateID,
                    this._patientInfo));
            }

            if (this.AfterSave != null)
            {
                //保存时，触发矢量图的保存事件
                if (GlobalVariable.Vector != null)
                {
                    var vectorList = this.pictureBoxBackGround.Controls.Cast<Control>().Where(a => a is VectorControl && (a as VectorControl).Tag != null);
                    bool isVectorSaveFlag = true;
                    if (vectorList != null && vectorList.Any())
                    {
                        foreach (VectorControl v in vectorList)
                        {
                            if (string.IsNullOrEmpty(v.PicId) || v.VectorImageInfo.ActionByte == null || v.VectorImageInfo.ActionByte.Length == 0)
                                continue;
                            isVectorSaveFlag = GlobalVariable.Vector.SaveVectorImage(v.VectorImageInfo, _patientInfo.PatientId, _TemplateName, _TemplateID);
                            if (!isVectorSaveFlag)
                            {
                                msg = string.Format("保存矢量图失败：patientid={0} , image name={1} , picid={2}", _patientInfo.PatientId, v.Name, v.PicId);
                                break;
                            }
                        }
                    }
                    if (!isVectorSaveFlag)
                    {
                        ShowInforMsg(msg, true);
                        //Comm.ShowErrorMessage(msg);
                        return false;
                    }
                }
                operateState = this.AfterSave(this, new GlobalVariable.SaveEventArgs(
                    value,
                    this._TemplateName,
                    this._TemplateID,
                    this._patientInfo));
            }

            //成功
            if (operateState)
            {

                _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.SequenceNo), sequenceNo.ToString());

                ////数据修改后，更新病人列表中的信息
                //UpdatePatientsInforDT(this._patientInfo.Id, _TemplateName, pages);

                //异步备份表单到硬盘中
                Task.Factory.StartNew(() =>
                {
                    ThreadAddBack();
                });

                //触发拆分二维表,可扩展

                msg = _TemplateName + "已经成功保存到数据库。";

                ShowInforMsg(this._patientInfo.PatientName + msg, false);


                //自动同步更新到其他表单，比如评估单的得分更新到一般护理记录单中。
                //确认消息：是否需要同步更新【记录单】？
                SaveSynchronize();

                if (_NeedSaveSuccessMsg && GlobalVariable.NeedMdiChildSaveSuccessMsg)
                {
                    Comm.ShowInfoMessage(msg);
                }

                //_LordClosing = false;

                return true;
            }
            else
            {
                //异常：
                ShowInforMsg(msg, true);
                Comm.ShowErrorMessage(msg);

                return false;
            }
        }

        private string RecruitNewToString_Insert()
        {
            //准备好病人信息参数：
            string strDate = string.Format("{0:yyyyMMddHHmmssffff}", GetServerDate());

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<NurseRoot></NurseRoot>"); //xml申明先不要了，以后根据需要可以随时添加

            XmlNode xnRoot = xmlDoc.SelectSingleNode(nameof(EntXmlModel.NurseRoot));

            //添加当前文书节点：产程图"Partogram" NursePartogramChart
            string bookNodeName = nameof(EntXmlModel.NurseForm);
            XmlElement xeBook = xmlDoc.CreateElement(bookNodeName); //xmlDoc.CreateElement(nameof(EntXmlModel.NurseForm));
            xnRoot.AppendChild(xeBook);
            xeBook.SetAttribute(nameof(EntXmlModel.FormName), this._TemplateName);


            //xeBook.InnerXml = paraArr[2]; 这样是将文书的根节点作为其子节点添加进来了；会将传入的节点作为子节点添加。而不是覆盖。而OuterXml又是只读的
            //先移除这个节点，再把传入的字符串转成节点添加进来。
            xmlDoc.DocumentElement.RemoveChild(xeBook);

            XmlDocument xmlDocTemp = new XmlDocument();
            xmlDocTemp.LoadXml(_RecruitXML.DocumentElement.OuterXml);
            //xnRoot.AppendChild(xnRoot.OwnerDocument.ImportNode(xmlDocTemp.DocumentElement, true)); //深度拷贝xmlnode
            xeBook = xnRoot.AppendChild(xnRoot.OwnerDocument.ImportNode(xmlDocTemp.DocumentElement, true)) as XmlElement; //深度拷贝xmlnode

            //必要的文书特性和更新信息设置：
            xeBook.SetAttribute(nameof(EntXmlModel.UHID), _patientInfo.PatientId);
            xeBook.SetAttribute(nameof(EntXmlModel.FormName), this._TemplateName);
            xeBook.SetAttribute(nameof(EntXmlModel.ChartType), nameof(EntXmlModel.Record));
            //xeBook.SetAttribute(nameof(EntXmlModel.SequenceNo), paraArr[6]);
            xeBook.SetAttribute(nameof(EntXmlModel.SequenceNo), "1");
            xeBook.SetAttribute(nameof(EntXmlModel.CreatDate), strDate);
            xeBook.SetAttribute(nameof(EntXmlModel.UpdateDate), strDate);
            xeBook.SetAttribute(nameof(EntXmlModel.CreatUser), GlobalVariable.LoginUserInfo.UserCode);
            xeBook.SetAttribute(nameof(EntXmlModel.UpdateUser), GlobalVariable.LoginUserInfo.UserCode);

            return xnRoot.OuterXml;
        }

        private string RecruitNewToString_Update()
        {
            return string.Format("<{1}><{2}>{0}</{2}></{1}>", _RecruitXML.DocumentElement.InnerXml, nameof(EntXmlModel.NurseRoot), nameof(EntXmlModel.NurseForm));
        }

        private int DeleteNursingRecord(string strHisID)
        {
            try
            {
                //string strSQL = "DELETE FROM " + Consts.strTrtTableName + " WHERE PATIENT_ID={PATIENT_ID} AND FORM_ID={FORM_ID}";
                //Dictionary<string, object> dic = new Dictionary<string, object>();
                //dic.Add("PATIENT_ID", strHisID);
                //dic.Add("FORM_ID", _TemplateID);

                //ResponseOfInt responseOfInt = WcfRwClient.RwManager.ExecuteNonQuery(new DbInputDict() { ConnectName = Consts.trt, Sql = strSQL, Dict = dic });
                //ResponseOfInt responseOfInt = WcfRwClient.RwManager.ExecuteNonQuery(Consts.trt, strSQL, dic, CommandType.Text, null);
                return 1;// 
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (_Preview)
                {
                    return; //预览的时候不要往下梳理
                }

                _IsLoaded = true;



                InitializeNurseEditor(); //初始化一些基本的设置

                ////加载模板菜单
                //SetTemplate();


                panelMain.Focus();
                //toolStripStatusLabelCurrentPatient.Text = "选择打开已有的表单进行查看、修改，或者选择病人进行新建表单后再操作。";

                _picTransEffect = new PictureBox();
                this.panelMain.Controls.Add(_picTransEffect);

                //初始化智能助理
                SetWriteHelp();
                //清空样式
                ToolStripManager.Renderer = null;

                //表格当前行被金色
                string colorDSS = _DSS.SelectRowBackColor;
                if (string.IsNullOrEmpty(colorDSS))
                {
                    _SelectRowBackColor = Color.Empty;
                }
                else
                {
                    if (string.IsNullOrEmpty(colorDSS.Trim()))
                    {
                        _SelectRowBackColor = Color.Empty;
                    }
                    else
                    {
                        _SelectRowBackColor = Color.FromName(colorDSS.Trim());
                    }
                }

                ////如果不存在表单导入配置文件，那么工具栏按钮隐藏
                //if (File.Exists(_Recruit_XMlPath + @"\表单导入共享.ini"))
                //{
                //    this.toolStripButtonImportRecruit.Visible = true;
                //}
                //else
                //{
                //    this.toolStripButtonImportRecruit.Visible = false;
                //}

            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 光标进入的时候，设置智能输入匹配提示，和文字工具栏信息，合计处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetAssistantControl_Enter(object sender, EventArgs e)
        {
            _IsCopyCellMouseDown = false;

            if (Comm._SettingRowForeColor)//时间改变，自动对行的文字颜色改变的时候，不用刷新文字工具栏
            {
                return;
            }

            _Need_Assistant_Control = (Control)sender;

            if (!isAdjusting)
            {
                SetAutocompleteToAssistant_Control();  //自动自能匹配显示输入助理的字典文字
            }

            try
            {
                //强行触发更新字体工具栏信息
                ((RichTextBoxExtended)sender).SelectAll();

                bool temp = _IsLoading;
                _IsLoading = false;

                UpdateToolBarFontStyle_SelectionChanged(sender, null);

                _IsLoading = temp;
                _CurrentCellNeedSave = false;


                //进行合计:表格的合计在showcell中做，这里赋值后还是会被清空的
                if (((RichTextBoxExtended)sender).Text == "" && !((RichTextBoxExtended)sender)._IsTable && ((RichTextBoxExtended)sender).Sum.Trim() != "")
                {
                    Sum("");
                }

                //如果表格，还是应该在showcell中做，否则还是会被清空的
                //光标进入脚本：尤其是表格的时候，用来判断当前行的数据，形成特殊的合计列的汇总。利用评分和合计都是无法完成的，因为里面含有数字，而且是一个范围判断。
                #region 双击事件/Enter事件的脚本验证 可以执行表格脚本：判断赋值
                if (_node_Script != null && _node_Script.ChildNodes.Count > 0)
                {
                    RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;

                    if (string.IsNullOrEmpty(rtbe.Text.Trim()))
                    {
                        foreach (XmlNode xn in _node_Script.ChildNodes)
                        {
                            //如果是注释那么跳过
                            if (xn.Name == @"#comment" || xn.Name == @"#text")
                            {
                                continue;
                            }

                            //找到当前控件，对应的脚本，多个事件合并共用
                            //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == ctl.Name && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)) == "DoubleClick")
                            if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == rtbe.Name && ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains("Enter")) //DoubleClick
                            {
                                bool isTableScript = false;
                                if (!bool.TryParse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.IsTable)), out isTableScript))
                                {
                                    isTableScript = false;
                                }

                                //非表格的双击，执行这里
                                if (!rtbe._IsTable && !isTableScript)
                                {
                                    if (!Evaluator.EvaluateRunString(this, ScriptPara(xn.InnerText.Trim())))
                                    {
                                        return;
                                    }
                                }
                                //else if (rtbe._IsTable && isTableScript)    //表格脚本执行
                                //{
                                //    if (_TableType && _TableInfor != null)
                                //    {
                                //        //表格选中了某行，存在处理对象当前行
                                //        if (_CurrentCellIndex.X != -1 && _TableInfor.CurrentCell != null)
                                //        {
                                //            this.Cursor = Cursors.WaitCursor;

                                //            //签名.Text != '' Then Error(已经签名了哦)
                                //            //日期.Text != '' Then 签名.Text = ''     

                                //            //但是如果签了一次，再签的话；需要光标离开才能更新当前单元格啊,要触发一次保存
                                //            Cells_LeaveSaveData(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE, null); //触发光标离开事件，进行保存 数据

                                //            if (!Evaluator.EvaluateRunString(_TableInfor, ScriptPara(xn.InnerText.Trim())))
                                //            {
                                //                this.Cursor = Cursors.Default;

                                //                return;
                                //            }

                                //            this.Cursor = Cursors.Default;
                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                }
                #endregion 双击事件的脚本验证


                AddCtrlCell(((RichTextBoxExtended)sender).Text);

            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }



        /// <summary>
        /// 行头提示合计行，一眼看清楚
        /// </summary>
        /// <param name="tableInforPara"></param>
        /// <param name="i"></param>
        /// <param name="g"></param>
        private void DrawSumRowAttention(TableInfor tableInforPara, int i, Graphics g)
        {
            //提示合计行：行头提示
            if (!string.IsNullOrEmpty(_DSS.SumRowAttention)) //打印的时候不要绘制 && !_IsPrintingDraw
            {
                string[] arr = _DSS.SumRowAttention.Split(','); //标记符号，颜色，字体大小

                if (!string.IsNullOrEmpty(tableInforPara.Rows[i].SumType) && arr.Length >= 3)
                {
                    bool needDispose = false;
                    if (g == null)
                    {
                        //点击工具栏，进行合计操作后，更新界面
                        needDispose = true;
                        g = Graphics.FromImage(pictureBoxBackGround.Image);//上面图片重新绑定了，需要再次取得其GDi对象
                                                                           //g = this.pictureBoxBackGround.CreateGraphics();
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                    }

                    //SumRowAttention="∑,Red,9"
                    int size = int.Parse(arr[2].Trim());

                    Font fontSumRowAttention = new Font("宋体", float.Parse(arr[2].Trim()));
                    SolidBrush sb = new SolidBrush(Color.FromName(arr[1].Trim()));
                    Point pp = tableInforPara.Rows[i].Loaction;

                    if (!tableInforPara.Vertical)
                    {
                        pp = new Point(pp.X - size * 2, pp.Y + size);
                    }
                    else
                    {
                        pp = new Point(pp.X + size, pp.Y - size * 2);
                    }

                    g.DrawString(arr[0], fontSumRowAttention, sb, pp);

                    fontSumRowAttention.Dispose();
                    sb.Dispose();

                    if (needDispose)
                    {
                        g.Dispose();
                    }
                }
            }
        }
        private int GetNodeIndex(XmlNode xn)
        {
            int ret = -1;

            for (int i = 0; i < xn.ParentNode.ChildNodes.Count; i++)
            {
                if (xn.ParentNode.ChildNodes[i] == xn)
                {
                    return i;
                }
            }

            return ret;
        }

        /// <summary>
        /// 获取rtf字体颜色
        /// </summary>
        /// <param name="rtfPara"></param>
        /// <returns></returns>
        private Color GetRtfColor(string rtfPara)
        {
            RichTextBox rtb = new RichTextBox();
            if (!string.IsNullOrEmpty(rtfPara))
            {
                rtb.Clear();
                rtb.Rtf = rtfPara;
                rtb.SelectAll();
                return rtb.SelectionColor;
            }

            return Color.Black; //默认值
        }

        private void SavedScript(string typeName)
        {
            if (_node_Script != null && _node_Script.ChildNodes.Count > 0)
            {
                Control[] cl;
                XmlNode thisPage = _RecruitXML_Org.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), _CurrentPage, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));

                foreach (XmlNode xn in _node_Script.ChildNodes)
                {
                    //如果是注释那么跳过
                    if (xn.Name == @"#comment" || xn.Name == @"#text")
                    {
                        continue;
                    }

                    if (ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains(typeName)) //Contains("Saved")
                    {

                        cl = this.pictureBoxBackGround.Controls.Find((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)), false);

                        if (cl == null || cl.Length == 0)
                        {
                            continue;
                        }

                        if (typeName == "SavedChanged")
                        {

                            if (thisPage == null || (thisPage as XmlElement).GetAttribute((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name))).Split('¤')[0].Trim() != cl[0].Text.Trim())
                            {
                                //新建页还没有保存过，或者和上次保存的值不一样，就执行：往下处理

                            }
                            else
                            {
                                continue;
                            }
                        }

                        //找到当前控件，对应的脚本，多个事件合并共用
                        if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == cl[0].Name)
                        {
                            bool isTableScript = false;
                            if (!bool.TryParse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.IsTable)), out isTableScript))
                            {
                                isTableScript = false;
                            }

                            //不是输入框，或非表格的输入框，执行这里
                            if (!(cl[0] is RichTextBoxExtended) || (cl[0] is RichTextBoxExtended && !((RichTextBoxExtended)cl[0])._IsTable && !isTableScript))
                            {
                                bool isOKPara = true;
                                if (!Evaluator.EvaluateRunString(this, ScriptPara(xn.InnerText.Trim()), ref isOKPara))  //返回值是是否错误消息，中断退出。引用参数才是条件是否满足
                                {
                                    return;
                                }

                                //if(isOKPara) //条件满足的时候，再判断是否要创建表单的脚本
                                //{
                                //    //ScriptOwnMethod(xn.InnerText.Trim());  //改在脚本类中处理调用ScriptOwnMethod方法。
                                //}
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 保存时自动签名的处理
        /// 只处理本页
        /// </summary>
        private void SaveAutoSign()
        {

            if (_TableType && _TableInfor != null)
            {
                //没有日期列的跳出
                if (_TableInfor.CellByRowNo_ColName(0, "日期") == null)
                {
                    return;
                }

                //没有日期列的跳出
                if (_TableInfor.CellByRowNo_ColName(0, "签名") == null)
                {
                    return;
                }

                bool isEmptyRow = true;                 //判断是否为空行
                bool isNotSameDateTimeToPreRow = false; //判断于上一行日期时间是否一样
                CellInfor cellSign = null;

                if (_SaveAutoSign == "start")
                {
                    //一行一行的遍历，判断某段落没有签名，那么就签名
                    for (int rowIndex = _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1; rowIndex >= 0; rowIndex--)
                    {
                        isEmptyRow = IsEmptyRow(rowIndex);

                        //如果为非空行，那么判断：是否需要签名
                        if (!isEmptyRow)
                        {
                            //判断是否本段落
                            isNotSameDateTimeToPreRow = NotSameDateTimeToPreRow(rowIndex);

                            if (isNotSameDateTimeToPreRow)//和上一行时间不一样，那么就要签名了
                            {
                                //还没有签名的话
                                cellSign = _TableInfor.CellByRowNo_ColName(rowIndex, "签名");
                                if (string.IsNullOrEmpty(cellSign.Text.Trim()))
                                {
                                    ShowCellRTBE(rowIndex, cellSign.ColIndex); //选中需要签名的单元格

                                    Cells_DoubleClickSetDefaulyValue(_TableInfor.Columns[cellSign.ColIndex].RTBE, null);

                                    //Cells_LeaveSaveData(_TableInfor.Columns[cellSign.ColIndex].RTBE, null); 
                                    SaveCellToXML(_TableInfor.Columns[cellSign.ColIndex].RTBE);

                                    //_TableInfor.Columns[cellSign.ColIndex].RTBE.Visible = false; //不然切换其他页，会再提示需要保存
                                }
                                else
                                {
                                    //遇到一行已经签名的，就不用再遍历判断了
                                    //break;
                                }
                            }
                        }
                    }

                    this.pictureBoxBackGround.Focus();
                    SaveToOp();   //保存前处理，防止光标还在单元格，而且内容已经修改

                    IsNeedSave = false;  //保存最后一样的时候也会被清掉
                }
                else if (_SaveAutoSign == "end")
                {
                    //一行一行的遍历，判断某段落没有签名，那么就签名
                    for (int rowIndex = _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1; rowIndex >= 0; rowIndex--)
                    {
                        isEmptyRow = IsEmptyRow(rowIndex);

                        //如果为非空行，那么判断：是否需要签名
                        if (!isEmptyRow)
                        {
                            //判断是否本段落
                            if (rowIndex < _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1 && IsEmptyRow(rowIndex + 1))
                            {
                                isNotSameDateTimeToPreRow = true;  //如果当前行为非空白行，并且下一行是空白行。那么认为是一个段落的最后一行，需要签名
                            }
                            else
                            {
                                isNotSameDateTimeToPreRow = NotSameDateTimeToNextRow(rowIndex); //+1,和下一行比较，如果下面空行，那么都是一样的
                            }

                            //是最后一行也要签名，但是这种做法会导致和签名在最后一行，调整到下一页后。也有签名了，效果不好，还是保存时签名好
                            //但是这里，只处理本页
                            if (!isNotSameDateTimeToPreRow && _SaveAutoSignLastRow)
                            {
                                if (rowIndex == _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
                                {
                                    isNotSameDateTimeToPreRow = true;
                                }
                            }

                            if (isNotSameDateTimeToPreRow)//和上一行时间不一样，那么就要签名了
                            {
                                //还没有签名的话：保存时，会加本页所有行（段落）的自动签名。但是如果正好签名被纵向合并，或者整行合并，就会异常currentcell为null导致
                                cellSign = _TableInfor.CellByRowNo_ColName(rowIndex, "签名");
                                if (cellSign != null && string.IsNullOrEmpty(cellSign.Text.Trim()) && !cellSign.IsMergedNotShow) //并且不是被合并的
                                {
                                    ShowCellRTBE(rowIndex, cellSign.ColIndex); //选中需要签名的单元格:如果签名单元格被合并隐藏，那么：_TableInfor.CurrentCell为null，下面保存的时候就会报错

                                    if (_TableInfor.CurrentCell == null)
                                    {
                                        Comm.LogHelp.WriteErr("自动签名时遇到异常：_TableInfor.CurrentCell为null，无法保存移动签名");
                                    }

                                    Cells_DoubleClickSetDefaulyValue(_TableInfor.Columns[cellSign.ColIndex].RTBE, null);

                                    //Cells_LeaveSaveData(_TableInfor.Columns[cellSign.ColIndex].RTBE, null); 
                                    SaveCellToXML(_TableInfor.Columns[cellSign.ColIndex].RTBE);

                                }
                                else
                                {
                                    //遇到一行已经签名的，就不用再遍历判断了
                                    //break;  //如果在中间插入行，就会不会再签名了。
                                }
                            }

                        }
                    }

                    if (_IsSignLastRow && _SaveAutoSignLastRow)
                    {
                        //this.pictureBoxBackGround.Focus();
                        //SaveToOp();   //保存前处理，防止光标还在单元格，而且内容已经修改

                        IsNeedSave = true; //如果调整段落后，进行了这个处理变成不要保存就不提示了
                    }
                    else
                    {
                        this.pictureBoxBackGround.Focus();
                        SaveToOp();   //保存前处理，防止光标还在单元格，而且内容已经修改

                        IsNeedSave = false;
                    }

                    //_IsNeedSave = false;
                }
            }
            else
            {
                //非表格的，只要属性为非空就认为有效
                if (!string.IsNullOrEmpty(_SaveAutoSign))
                {
                    //非表格的，找到“签名”输入框进行签名。如果签名不处理，因为很难判断某个签名对应的区域已经填写了数据
                    Control[] ct = this.pictureBoxBackGround.Controls.Find("签名", false);
                    if (ct == null || ct.Length == 0)
                    {
                        //找不到控件，不执行
                        ct = this.pictureBoxBackGround.Controls.Find("记录者", false);
                    }

                    if (ct != null && ct.Length >= 0)
                    {
                        for (int i = 0; i < ct.Length; i++)
                        {
                            if (ct[i] is RichTextBoxExtended)
                            {
                                if (string.IsNullOrEmpty(((RichTextBoxExtended)ct[i]).Text) && !((RichTextBoxExtended)ct[i]).IsContainImg() && !((RichTextBoxExtended)ct[i])._IsTable)
                                {
                                    Cells_DoubleClickSetDefaulyValue(ct[i], null);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        //private void GetTemplate_New(string yearPara, string uhidPara, string templateNamePara, int lockPara)
        //{

        //    //string[] statusArr = new string[] { "", "" };
        //    ////string strData = GlobalVariable.GetNurseRecruitData(uhidPara, this._patientInfo.Id, templateNamePara, ref statusArr);
        //    //string strSQL = "select nm_done,nm_type,nm_record from Nursing_2017 where nm_uhid='{0}' and um_hisid='{1}'";
        //    ////2[血液内科]537463274755.419
        //    ////000521217400
        //    //strSQL = string.Format(strSQL, "2[血液内科]537463274755.419", "000521217400");
        //    ////ResponseOfDataTable dtbl = WcfRwClient.RwManager.ExecuteQuery(Consts.trt, strSQL);
        //    //ResponseOfDataTable dtbl = WcfRwClient.RwManager.ExecuteQuery(new DbInput() { ConnectName = Consts.trt, Sql = strSQL });
        //    //if (dtbl.IsSuccess == false) return;

        //    string strData = string.Empty;
        //    strData = GlobalVariable.NurseFormValue;
        //    //if (dtbl.Count == 1)
        //    //{
        //    //strData = dtbl.ResponseContent.Rows[0]["nm_record"].ToString();
        //    ////归档状态
        //    //statusArr[0] = dtbl.ResponseContent.Rows[0]["nm_done"].ToString();
        //    ////转科转病区标记
        //    //statusArr[1] = dtbl.ResponseContent.Rows[0]["nm_type"].ToString();

        //    ////TRT.Framework.Library.ZipDeflate.DecompressString
        //    ////先用现有的解压方法，然后改成自己的
        //    //strData = ZipUtil.UnZip(strData);

        //    //XmlDocument xmlDoc = new XmlDocument();
        //    //xmlDoc.LoadXml(strData);
        //    //string bookNodeName = nameof(EntXmlModel.NurseForm);
        //    //XmlNode xnBook = xmlDoc.DocumentElement.SelectSingleNode(bookNodeName + "[@FormName='一般护理记录单']") as XmlElement;
        //    //strData = xnBook.OuterXml;
        //    //}
        //    if (string.IsNullOrEmpty(strData))
        //    {
        //        IsNeedSave = true;
        //        _IsCreatedTemplate = true;

        //        ////没有检索到，那么创建一个空的xml数据文件
        //        TemplateHelp mx = new TemplateHelp();
        //        _RecruitXML = mx.CreateTemplate(this._patientInfo.PatientId, this._patientInfo.PatientName, GlobalVariable.LoginUserInfo.UserCode, GlobalVariable.LoginUserInfo.DeptName, _TemplateXml, _TemplateName);

        //        int creatPages = 1;

        //        #region 自动创建n页 自动创建后打开第一页比较好
        //        //自动创建n页的处理…… 创建打开表单的时候处理。这样还能先打开第一页。也符合业务逻辑。
        //        XmlNode tempRoot = _TemplateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm));

        //        if (!int.TryParse((tempRoot as XmlElement).GetAttribute(nameof(EntXmlModel.AutoCreatPagesCount)), out creatPages))
        //        {
        //            creatPages = 1;
        //        }

        //        //自动创建指定页:填写好默认的数据和抬头基本信息；这样每次开大创建的其他页，如果不做修改就不会提示保存了
        //        if (creatPages > 1)
        //        {
        //            XmlNode thisPageNode = null;
        //            XmlNode xnEach = null;  //模板节点
        //            XmlNode thisPageData;
        //            string strName = "";

        //            for (int i = 2; i <= creatPages; i++)
        //            {
        //                //获取，当前模板页配置节点
        //                thisPageNode = GetTemplatePageNode(_TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design))), i);

        //                if (thisPageNode != null)
        //                {

        //                    thisPageData = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), i.ToString(), "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));  //数据页节点
        //                    strName = "";

        //                    if (thisPageData == null)
        //                    {
        //                        continue;//CreateXML中已经添加了页节点
        //                    }

        //                    for (int j = 0; j < thisPageNode.ChildNodes.Count; j++)
        //                    {
        //                        //如果是注释那么跳过
        //                        if (thisPageNode.ChildNodes[j].Name == @"#comment" || thisPageNode.ChildNodes[j].Name == @"#text")
        //                        {
        //                            continue;
        //                        }

        //                        xnEach = thisPageNode.ChildNodes[j];

        //                        strName = (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));

        //                        if (xnEach.Name == nameof(EntXmlModel.BindingValue))
        //                        {
        //                            //不等于页号
        //                            if ((xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) != "_PageNo")
        //                            {
        //                                if (_CurrentBasicInfor != null && _CurrentBasicInfor.ContainsKey(strName))
        //                                {
        //                                    (thisPageData as XmlElement).SetAttribute(strName, _CurrentBasicInfor[strName]);
        //                                }
        //                            }
        //                        }
        //                        else if (xnEach.Name.EndsWith(nameof(EntXmlModel.Text)) || xnEach.Name == nameof(EntXmlModel.ComboBox))
        //                        {
        //                            //bool autoLoadDefault = false;
        //                            //bool.TryParse((xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.AutoLoadDefault)), out autoLoadDefault);
        //                            //if (autoLoadDefault)
        //                            (thisPageData as XmlElement).SetAttribute(strName, (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)));
        //                        }
        //                        else if (xnEach.Name == nameof(EntXmlModel.CheckBox))
        //                        {
        //                            (thisPageData as XmlElement).SetAttribute(strName, (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)));
        //                        }
        //                        //else if (xnEach.Name == nameof(EntXmlModel.CheckBox))
        //                        //{
        //                        //    (thisPageData as XmlElement).SetAttribute(strName, (xnEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)));
        //                        //}

        //                    }

        //                }
        //            }

        //            Comm.Logger.WriteInformation(_TemplateName + "：在连续创建N页时，对每页数据都进行了默认初始化。");
        //        }
        //        #endregion end自动创建n页

        //        _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.FORM_TYPE), _TemplateRight);
        //        _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Forms), creatPages.ToString());

        //        //_RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.FORM_TYPE), _TemplateRight);
        //        //_RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Forms), "1");
        //    }
        //    else
        //    {
        //        _RecruitXML = new XmlDocument();
        //        _RecruitXML.LoadXml(strData);

        //        //statusArr更新归档和转科病区 nameof(EntXmlModel.Done) nameof(EntXmlModel.Type)
        //        _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Done), GlobalVariable.Done); //statusArr[0]
        //        _RecruitXML.DocumentElement.SetAttribute(nameof(EntXmlModel.Type), GlobalVariable.Type); //statusArr[1]
        //    }
        //}

        /// <summary>
        /// 打印前验证，PrintCheck为true开启，表示打印要进行和保存一样的验证，有错误的不能打印。
        /// </summary>
        /// <returns></returns>
        private bool PrintCheck()
        {
            if (_DSS == null)
            {
                _DSS = new DataServiceSetting(RunMode);
            }

            if (_DSS.PrintCheck)
            {

                //是否存在不合法的输入数据，有错误不能保存，跳出。
                if (_ErrorList.Count > 0)
                {
                    string errMsg = "";
                    for (int i = 0; i < _ErrorList.Count; i++)
                    {
                        errMsg += "\r\n" + _ErrorList[i];
                    }

                    _IsSaveingNow = false;

                    Comm.LogHelp.WriteErr("打印失败，因为存在不合理的数据：" + errMsg);
                    ShowInforMsgExpand(errMsg);

                    if (!_LordClosing)
                    {
                        MessageBox.Show("打印失败，因为存在不合理的数据，请先修改后再打印和保存。", "无法打印", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    return false; //如果切换其他页，提示保存的时候选择了【是】，但是发现错误不能报错，需要做处理
                }

            }

            return true;

        }

        /// <summary>
        /// 将绘制的图片保存到本地
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sfd_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _IsLoading = true;

                Image ni = GetChartImage();

                //关键质量控制 
                //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff 
                ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo ici = null;
                foreach (ImageCodecInfo i in icis)
                {
                    if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                    {
                        ici = i;
                    }
                }
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);

                //保存体温单图片 
                ni.Save(sfd.FileName, ici, ep);

                _IsLoading = false;
            }
            catch (Exception ex)
            {
                _IsLoading = false;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 保存背景图片，目前仅仅为调试查看绘制情况
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sfdBG_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Image ni = _BmpBack;

                //关键质量控制 
                //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff 
                ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo ici = null;
                foreach (ImageCodecInfo i in icis)
                {
                    if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                    {
                        ici = i;
                    }
                }
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);

                //保存体温单图片 
                ni.Save(sfdBG.FileName, ici, ep);

            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        //将病人列表导出为csv文件查看
        private void sfdCSV_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                if (_PatientsInforDT != null)
                {
                    //表头
                    for (int i = 0; i < _PatientsInforDT.Columns.Count; i++)
                    {
                        if (i == 0)
                        {
                            sb.Append(_PatientsInforDT.Columns[i].ColumnName);
                        }
                        else
                        {
                            sb.Append("," + _PatientsInforDT.Columns[i].ColumnName);
                        }

                        //判断末尾
                        if (i == _PatientsInforDT.Columns.Count - 1)
                        {
                            sb.Append("\r\n");
                        }
                    }

                    //数据
                    for (int r = 0; r < _PatientsInforDT.Rows.Count; r++)
                    {
                        for (int i = 0; i < _PatientsInforDT.Columns.Count; i++)
                        {
                            if (i == 0)
                            {
                                sb.Append(_PatientsInforDT.Rows[r][i].ToString());
                            }
                            else
                            {
                                sb.Append("," + _PatientsInforDT.Rows[r][i].ToString());
                            }

                            //判断末尾
                            if (i == _PatientsInforDT.Columns.Count - 1)
                            {
                                sb.Append("\r\n");
                            }
                        }
                    }

                    string content = sb.ToString();
                    //string dir = Directory.GetCurrentDirectory();
                    //string fullName = Path.Combine(dir, fileName);
                    string fullName = sfdCSV.FileName;
                    if (File.Exists(fullName))
                    {
                        File.Delete(fullName);
                    }

                    using (FileStream fs = new FileStream(fullName, FileMode.CreateNew, FileAccess.Write))
                    {
                        StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                        sw.Flush();
                        sw.Write(content);
                        sw.Flush();
                        sw.Close();
                    }

                    ShowInforMsg("病人列表信息如下：\r\n" + content);
                }

            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>得到表单当前页的图像 Image
        /// 得表单的当前打开页的图像 Image
        /// </summary>
        /// <returns></returns>
        private Image GetChartImage()
        {
            try
            {
                _IsLoading = true;

                _CurrentPagePrintStartRow = -1; //0;
                _GoOnPrintLocation = new Point(-1, -1); //生成图片的时候，不需要续打

                Image ni;
                Graphics g = null;
                ni = new Bitmap(this.pictureBoxBackGround.Width, this.pictureBoxBackGround.Height);
                Graphics.FromImage(ni).Clear(pictureBoxBackGround.BackColor); //不然为透明背景，这里用底板一样的背景
                g = Graphics.FromImage(ni);

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//消除锯齿，平滑
                                                                                   //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                                                                                   //g.PixelOffsetMode = PixelOffsetMode.HighQuality; //高像素偏移质量   
                                                                                   //g.CompositingQuality = CompositingQuality.HighQuality;
                                                                                   //g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                //生成图片的时候 -- 按上述处理，位置会错乱 位置会不对，很奇怪
                _isCreatImages = true;

                //将体温单信息绘制到图片上，并释放g
                drawToImage(g, true, false);

                _isCreatImages = false;

                _IsLoading = false;

                return ni;
            }
            catch (Exception ex)
            {
                _IsLoading = false;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 在操作行显示手指提示
        /// 操作行，位置突出显示（一个手指指向行头）
        /// </summary>
        private void ShowSearchRow()
        {
            int x = 0;
            int y = 0;

            //考虑到分栏的情况，为了让显示效果更好
            if (_TableInfor.GroupColumnNum > 1 && _TableInfor.CurrentCell.Field > 0)
            {
                x = _TableInfor.Rows[_CurrentCellIndex.X - _TableInfor.CurrentCell.Field * _TableInfor.RowsCount].Loaction.X + 1 + _TableInfor.CurrentCell.Field * _TableInfor.PageRowsWidth;
                y = _TableInfor.Rows[_CurrentCellIndex.X - _TableInfor.CurrentCell.Field * _TableInfor.RowsCount].Loaction.Y + 3;
            }
            else
            {
                x = _TableInfor.Rows[_CurrentCellIndex.X].Loaction.X + 1;
                y = _TableInfor.Rows[_CurrentCellIndex.X].Loaction.Y + 3;
            }

            ShowSearch(new Point(x, y));//显示手指位置
        }

        /// <summary>
        /// 使输入框和智能输入关联和卸载
        /// </summary>
        private void SetAutocompleteToAssistant_Control()
        {
            if (_Preview)
            {
                return; //预览的时候不要往下梳理
            }

            if (GlobalVariable.InputAutocomplete)
            {
                //MaximumSize如果能合理设置大小，那么可以防止在最下面的输入，匹配的项目又很多的时候，防止遮挡住输入框。
                int height = this.panelMain.Height - _Need_Assistant_Control.Location.Y - pictureBoxBackGround.Location.Y - 2;
                this.autocompleteMenuForWrite.MaximumSize = new Size(this.autocompleteMenuForWrite.MaximumSize.Width, height);

                //输入智能提示，与输入框绑定
                this.autocompleteMenuForWrite.SetAutocompleteMenu(_Need_Assistant_Control, this.autocompleteMenuForWrite);
            }
            else
            {
                //this.autocompleteMenuForWrite
                if (AutocompleteMenu.WrapperByControls.ContainsKey(_Need_Assistant_Control))
                {
                    AutocompleteMenu.WrapperByControls.Remove(_Need_Assistant_Control);
                }
            }
        }

        /// <summary>
        /// 按下Ctrl的时候，多选几个输入框，选择复制内容
        /// </summary>
        /// <param name="text"></param>
        private void AddCtrlCell(string text)
        {
            if (_IS_CTRL_KEYDOWN)
            {
                _SelectedCells.Add(text);
            }
        }

        /// <summary>
        /// 绘制页尾斜线
        /// </summary>
        /// <param name="currentPagePara"></param>
        /// <param name="PageEndCrossLinePara">页号，开始行，结束行</param>
        /// <param name="g"></param>
        /// <param name="tableInforPara"></param>
        private void DrawPageEndCrossLine(string currentPagePara, string PageEndCrossLinePara, Graphics g, TableInfor tableInforPara)
        {
            try
            {
                if (!string.IsNullOrEmpty(PageEndCrossLinePara))
                {
                    string[] arr = PageEndCrossLinePara.Split(',');
                    if (arr.Length == 3 && currentPagePara == arr[0].Trim())
                    {
                        int startRow = int.Parse(arr[1].Trim()) - 1;
                        int endRow = int.Parse(arr[2].Trim()) - 1;

                        CellInfor endCell = tableInforPara.Cells[endRow, tableInforPara.ColumnsCount - 1];
                        //--Pen _PageEndCrossLinePen = new Pen(Color.Red, 1);
                        g.DrawLine(_PageEndCrossLinePen,
                            tableInforPara.Cells[startRow, 0].Loaction, new Point(endCell.Loaction.X + endCell.Rect.Width, endCell.Loaction.Y + endCell.Rect.Height));
                    }
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                MessageBox.Show("绘制页尾斜线，出现异常！", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        ///输入数字，格式化成时间
        /// </summary>
        /// <param name="rtbe"></param>
        private void NumberToTime(RichTextBox rtbe)
        {
            //直接输入数字的处理
            rtbe.Text = Comm.GetNumberToTimeStr(rtbe.Text, DateFormat_HM);
        }

        private void toolStripButtonCreateForm_Click(object sender, EventArgs e)
        {

        }






        /// <summary>
        /// 根据当时的界面大小，设置空白画布的大小
        /// </summary>
        private void SetPreviewSize(Size sizePara)
        {
            pictureBoxBackGround.Size = sizePara;
        }

        #endregion
    }
}