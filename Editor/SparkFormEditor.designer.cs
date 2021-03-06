using System.Windows.Forms;
namespace SparkFormEditor
{
    partial class SparkFormEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            //ClearReport();

            if (disposing && (components != null))
            {


                components.Dispose();

                if (_TableInfor != null)
                {
                    _TableInfor.Dispose();
                }

                _StrXmlData = null;
                this._RecruitXML = null;
                this._RecruitXML_Org = null;

                if (this._PatientsInforDT != null)
                {
                    this._PatientsInforDT.Dispose();
                }

                //if (complexPopupAssistant != null && !complexPopupAssistant.IsDisposed)
                //{
                //    complexPopupAssistant.Dispose();
                //}

                autocompleteMenuForWrite = null;

                _TemplateManageXml = null;
                _TemplateXml = null;
                //_listInputBoxHideItems = null;
                _RowForeColorsList = null;

                if (_picTransEffect != null && !_picTransEffect.IsDisposed)
                {
                    _picTransEffect.Dispose();
                }

                _CurrentBasicInfor.Clear();
                _CreatTreeNode = null;
                if (_ErrorList != null) _ErrorList.Clear();

                _DSS = null;
                _ServerImages.Clear();
                _ListDoubleLineInputBox.Clear();
                _RichTextBoxPrinter = null;
                _SynchronizeTemperatureRecordListRecordInor = null;
                _SynchronizeTemperatureRecordItems = null;
                _SumSetting = null;
                _CellValueDefault = null;
                _FingerPrintSignImages = null;
                _ListDoubleLineInputBox = null;
                _LocalIP = null;
                _TableHeaderList = null;
                _BaseInforLblnameArr = null;
                _BaseArrForPrintAllPageUpdate = null;
                _CurrentPatientTreeNode = null;
                _CurrentTemplateNameTreeNode = null;

                _node_Knowledg = null;
                _node_Assistant = null;

                _BmpBack = null;
                _Need_Assistant_Control = null;
                _CurrentTemplateXml_Path = null;
                _NurseLevel = null;
                _node_Page = null;
                _node_Script = null;
                m_formSettingsVerifySignID = null;
                _CurrentTemplateXml_Path = null;

                _CurrentChartYear = null;                 //每个病人所有的表单都在一个年度表中
                _CurrentPage = null;
                _PreShowedPage = null;                   //本页显示前的页数
                _patientInfo = null;
                _TemplateName = null; 
                _TemplateRight = null;                   //表单权限：0医生，1护士，2共享表单
                _TemplateUpdate = null;                  //更新表单时，再次进行排他处理，防止被秒级同时打开的重复覆盖数据
                _RowForeColor = null;
             
                _CreatTemplatePara = null;
                DateFormat_YMD = null;
                _SignTag = null;

                _SelectedCells = null;

                _RowLineType = null;
                _NurseType = null;
                _xmlDocSumGroup = null;
                _SaveAutoSign = null;
                _AutoSortMode = null;
                _PageNoFormat = null;
                _DefaultTemperatureType = null;


                Control C = null;
                for (int i = this.pictureBoxBackGround.Controls.Count - 1; i >= 0; i--)
                {
                    if (this.pictureBoxBackGround.Controls[i] != null && !this.pictureBoxBackGround.Controls[i].IsDisposed)
                    {
                        //this.pictureBoxBackGround.Controls[i].Dispose(); //这样才能清楚句柄

                        C = this.pictureBoxBackGround.Controls[i];
                        pictureBoxBackGround.Controls.Remove(C);
                        C.Dispose();
                    }
                }

                System.GC.Collect();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SparkFormEditor));
            this.panel3 = new System.Windows.Forms.Panel();
            this.panelMain = new System.Windows.Forms.Panel();
            this.treeViewPatients = new System.Windows.Forms.TreeView();
            this.pictureBoxBackGround = new System.Windows.Forms.PictureBox();
            this.lblShowSearch = new SparkFormEditor.TransparentLabel();
            this.panelShade = new System.Windows.Forms.Panel();
            this.pnlToolSet = new System.Windows.Forms.Panel();
            this.toolStripToolBarWord = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonBold = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonItalic = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonUnderline = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonStrikeout = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonColor = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripButtonColorRed = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonColorGreen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonColorBlue = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonColorBlack = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorFont = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonUp = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparatorFormat = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonLeft = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonCenter = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRight = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparatorAlign = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonUndo = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRedo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonCopy = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonCut = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPaste = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonOneLine = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDoubleLine = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonClearFormat = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonDefaultRowLine = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripButtonLine = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripMenuItemCell = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCellLineRed = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCellLineGreen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCellLineBlue = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCellLineBlack = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCellLineDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemCellLineZ = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCellLineN = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemCellHideTopLine = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemRow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRowLineRed = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRowLineGreen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRowLineBlue = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRowLineBlack = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRowLineDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator22 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemRectLine = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemRectDoubleLine = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemTopBottomLine = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemTopBottomDoubleLine = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemBottomLine = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemBottomDoubleLine = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemTopLine = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemTopDoubleLine = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemBottomLineDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator26 = new System.Windows.Forms.ToolStripSeparator();
            this.关于行的其他操作toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.隐藏行上边框线ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.显示行上边框线toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.隐藏行下边框线ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator27 = new System.Windows.Forms.ToolStripSeparator();
            this.合并行ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.页尾斜线ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonMergeCell = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator24 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonSum = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator28 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonSumGroup = new System.Windows.Forms.ToolStripButton();
            this.pnl_tool1 = new System.Windows.Forms.Panel();
            this.toolStripFunc = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonCreateForm = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonCreatePage = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.tBnt_InsertRowUp = new System.Windows.Forms.ToolStripButton();
            this.tBnt_InsertRowDown = new System.Windows.Forms.ToolStripButton();
            this.tBnt_DelRow = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonDelPage = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDelAllPage = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonPrintPage = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPrintAllPage = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonContinuePrint = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonDelockApply = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.pnlPage = new System.Windows.Forms.Panel();
            this.uc_Pager1 = new SparkFormEditor.UC_Pager();
            this.contextMenuStripToolStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuStripFrozen = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemHide = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAuto = new System.Windows.Forms.ToolStripMenuItem();
            this.dateTimePickerSelectRYDate = new System.Windows.Forms.DateTimePicker();
            this.radioButtonNewHospital = new System.Windows.Forms.RadioButton();
            this.radioButtonheart = new System.Windows.Forms.RadioButton();
            this.radioButtonDisease2 = new System.Windows.Forms.RadioButton();
            this.radioButtonDisease1 = new System.Windows.Forms.RadioButton();
            this.radioButtonNurseLevel2 = new System.Windows.Forms.RadioButton();
            this.radioButtonNurseLevel1 = new System.Windows.Forms.RadioButton();
            this.radioButtonAllPatient = new System.Windows.Forms.RadioButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemReName = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemUp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDown = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripForImage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.删除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.边框线ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator21 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBoxSize = new System.Windows.Forms.ToolStripTextBox();
            this.sfd = new System.Windows.Forms.SaveFileDialog();
            this.sfdBG = new System.Windows.Forms.SaveFileDialog();
            this.sfdCSV = new System.Windows.Forms.SaveFileDialog();
            this.sfdHtml = new System.Windows.Forms.SaveFileDialog();
            this.sfdSaveAsXml = new System.Windows.Forms.SaveFileDialog();
            this.radioButtonSpecialNurse = new System.Windows.Forms.RadioButton();
            this.radioButtonNurseLevel3 = new System.Windows.Forms.RadioButton();
            this.pictureBoxFrozen = new System.Windows.Forms.PictureBox();
            this.toolStripButtonTcData = new System.Windows.Forms.ToolStripButton();
            this.panel3.SuspendLayout();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBackGround)).BeginInit();
            this.pictureBoxBackGround.SuspendLayout();
            this.pnlToolSet.SuspendLayout();
            this.toolStripToolBarWord.SuspendLayout();
            this.pnl_tool1.SuspendLayout();
            this.toolStripFunc.SuspendLayout();
            this.pnlPage.SuspendLayout();
            this.contextMenuStripFrozen.SuspendLayout();
            this.contextMenuStripForImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFrozen)).BeginInit();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Transparent;
            this.panel3.Controls.Add(this.panelMain);
            this.panel3.Controls.Add(this.pnlToolSet);
            this.panel3.Controls.Add(this.pnl_tool1);
            this.panel3.Controls.Add(this.pnlPage);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // panelMain
            // 
            resources.ApplyResources(this.panelMain, "panelMain");
            this.panelMain.BackColor = System.Drawing.SystemColors.Control;
            this.panelMain.Controls.Add(this.treeViewPatients);
            this.panelMain.Controls.Add(this.pictureBoxBackGround);
            this.panelMain.Controls.Add(this.panelShade);
            this.panelMain.Name = "panelMain";
            this.panelMain.Click += new System.EventHandler(this.panelMain_Click);
            // 
            // treeViewPatients
            // 
            this.treeViewPatients.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.treeViewPatients, "treeViewPatients");
            this.treeViewPatients.Name = "treeViewPatients";
            // 
            // pictureBoxBackGround
            // 
            this.pictureBoxBackGround.BackColor = System.Drawing.Color.White;
            this.pictureBoxBackGround.Controls.Add(this.lblShowSearch);
            resources.ApplyResources(this.pictureBoxBackGround, "pictureBoxBackGround");
            this.pictureBoxBackGround.Name = "pictureBoxBackGround";
            this.pictureBoxBackGround.TabStop = false;
            this.pictureBoxBackGround.LocationChanged += new System.EventHandler(this.pictureBoxBackGround_LocationChanged);
            this.pictureBoxBackGround.Click += new System.EventHandler(this.pictureBoxBackGround_Click);
            this.pictureBoxBackGround.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxBackGround_Paint);
            this.pictureBoxBackGround.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxBackGround_MouseClick);
            // 
            // lblShowSearch
            // 
            resources.ApplyResources(this.lblShowSearch, "lblShowSearch");
            this.lblShowSearch.BackColor = System.Drawing.Color.Transparent;
            this.lblShowSearch.ForeColor = System.Drawing.Color.Red;
            this.lblShowSearch.Name = "lblShowSearch";
            // 
            // panelShade
            // 
            this.panelShade.BackColor = System.Drawing.SystemColors.ControlDark;
            resources.ApplyResources(this.panelShade, "panelShade");
            this.panelShade.Name = "panelShade";
            // 
            // pnlToolSet
            // 
            this.pnlToolSet.Controls.Add(this.toolStripToolBarWord);
            resources.ApplyResources(this.pnlToolSet, "pnlToolSet");
            this.pnlToolSet.Name = "pnlToolSet";
            // 
            // toolStripToolBarWord
            // 
            this.toolStripToolBarWord.BackColor = System.Drawing.Color.White;
            this.toolStripToolBarWord.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripToolBarWord.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonBold,
            this.toolStripButtonItalic,
            this.toolStripButtonUnderline,
            this.toolStripButtonStrikeout,
            this.toolStripButtonColor,
            this.toolStripSeparatorFont,
            this.toolStripButtonUp,
            this.toolStripButtonDown,
            this.toolStripSeparatorFormat,
            this.toolStripButtonLeft,
            this.toolStripButtonCenter,
            this.toolStripButtonRight,
            this.toolStripSeparatorAlign,
            this.toolStripButtonUndo,
            this.toolStripButtonRedo,
            this.toolStripSeparator5,
            this.toolStripButtonCopy,
            this.toolStripButtonCut,
            this.toolStripButtonPaste,
            this.toolStripButtonClear,
            this.toolStripSeparator4,
            this.toolStripButtonOneLine,
            this.toolStripButtonDoubleLine,
            this.toolStripButtonClearFormat,
            this.toolStripSeparator10,
            this.toolStripButtonDefaultRowLine,
            this.toolStripButtonLine,
            this.toolStripButtonMergeCell,
            this.toolStripSeparator24,
            this.toolStripButtonSum,
            this.toolStripSeparator28,
            this.toolStripButtonSumGroup,
            this.toolStripButtonTcData});
            resources.ApplyResources(this.toolStripToolBarWord, "toolStripToolBarWord");
            this.toolStripToolBarWord.Name = "toolStripToolBarWord";
            this.toolStripToolBarWord.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripToolBarWord.Click += new System.EventHandler(this.toolStripToolBarWord_Click);
            // 
            // toolStripButtonBold
            // 
            this.toolStripButtonBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonBold, "toolStripButtonBold");
            this.toolStripButtonBold.Margin = new System.Windows.Forms.Padding(5, 1, 0, 2);
            this.toolStripButtonBold.Name = "toolStripButtonBold";
            this.toolStripButtonBold.Click += new System.EventHandler(this.toolStripButtonBold_Click);
            // 
            // toolStripButtonItalic
            // 
            this.toolStripButtonItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonItalic, "toolStripButtonItalic");
            this.toolStripButtonItalic.Name = "toolStripButtonItalic";
            this.toolStripButtonItalic.Click += new System.EventHandler(this.toolStripButtonItalic_Click);
            // 
            // toolStripButtonUnderline
            // 
            this.toolStripButtonUnderline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonUnderline, "toolStripButtonUnderline");
            this.toolStripButtonUnderline.Name = "toolStripButtonUnderline";
            this.toolStripButtonUnderline.Click += new System.EventHandler(this.toolStripButtonUnderline_Click);
            // 
            // toolStripButtonStrikeout
            // 
            this.toolStripButtonStrikeout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonStrikeout.Image = global::SparkFormEditor.Properties.Resources.fontStrickout;
            resources.ApplyResources(this.toolStripButtonStrikeout, "toolStripButtonStrikeout");
            this.toolStripButtonStrikeout.Name = "toolStripButtonStrikeout";
            this.toolStripButtonStrikeout.Click += new System.EventHandler(this.toolStripButtonStrikeout_Click);
            // 
            // toolStripButtonColor
            // 
            this.toolStripButtonColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonColor.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonColorRed,
            this.toolStripButtonColorGreen,
            this.toolStripButtonColorBlue,
            this.toolStripButtonColorBlack});
            resources.ApplyResources(this.toolStripButtonColor, "toolStripButtonColor");
            this.toolStripButtonColor.Name = "toolStripButtonColor";
            this.toolStripButtonColor.Click += new System.EventHandler(this.toolStripButtonColor_Click);
            // 
            // toolStripButtonColorRed
            // 
            this.toolStripButtonColorRed.ForeColor = System.Drawing.Color.Red;
            this.toolStripButtonColorRed.Name = "toolStripButtonColorRed";
            resources.ApplyResources(this.toolStripButtonColorRed, "toolStripButtonColorRed");
            this.toolStripButtonColorRed.Click += new System.EventHandler(this.toolStripButton文字颜色_Click);
            // 
            // toolStripButtonColorGreen
            // 
            this.toolStripButtonColorGreen.ForeColor = System.Drawing.Color.Green;
            this.toolStripButtonColorGreen.Name = "toolStripButtonColorGreen";
            resources.ApplyResources(this.toolStripButtonColorGreen, "toolStripButtonColorGreen");
            this.toolStripButtonColorGreen.Click += new System.EventHandler(this.toolStripButton文字颜色_Click);
            // 
            // toolStripButtonColorBlue
            // 
            this.toolStripButtonColorBlue.ForeColor = System.Drawing.Color.Blue;
            this.toolStripButtonColorBlue.Name = "toolStripButtonColorBlue";
            resources.ApplyResources(this.toolStripButtonColorBlue, "toolStripButtonColorBlue");
            this.toolStripButtonColorBlue.Click += new System.EventHandler(this.toolStripButton文字颜色_Click);
            // 
            // toolStripButtonColorBlack
            // 
            this.toolStripButtonColorBlack.Name = "toolStripButtonColorBlack";
            resources.ApplyResources(this.toolStripButtonColorBlack, "toolStripButtonColorBlack");
            this.toolStripButtonColorBlack.Click += new System.EventHandler(this.toolStripButton文字颜色_Click);
            // 
            // toolStripSeparatorFont
            // 
            this.toolStripSeparatorFont.Name = "toolStripSeparatorFont";
            resources.ApplyResources(this.toolStripSeparatorFont, "toolStripSeparatorFont");
            // 
            // toolStripButtonUp
            // 
            this.toolStripButtonUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonUp.Image = global::SparkFormEditor.Properties.Resources.fontUp;
            resources.ApplyResources(this.toolStripButtonUp, "toolStripButtonUp");
            this.toolStripButtonUp.Name = "toolStripButtonUp";
            this.toolStripButtonUp.Click += new System.EventHandler(this.toolStripButtonUp_Click);
            // 
            // toolStripButtonDown
            // 
            this.toolStripButtonDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonDown.Image = global::SparkFormEditor.Properties.Resources.fontDown;
            resources.ApplyResources(this.toolStripButtonDown, "toolStripButtonDown");
            this.toolStripButtonDown.Name = "toolStripButtonDown";
            this.toolStripButtonDown.Click += new System.EventHandler(this.toolStripButtonDown_Click);
            // 
            // toolStripSeparatorFormat
            // 
            this.toolStripSeparatorFormat.Name = "toolStripSeparatorFormat";
            resources.ApplyResources(this.toolStripSeparatorFormat, "toolStripSeparatorFormat");
            // 
            // toolStripButtonLeft
            // 
            this.toolStripButtonLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonLeft.Image = global::SparkFormEditor.Properties.Resources.left;
            resources.ApplyResources(this.toolStripButtonLeft, "toolStripButtonLeft");
            this.toolStripButtonLeft.Name = "toolStripButtonLeft";
            this.toolStripButtonLeft.Click += new System.EventHandler(this.toolStripButtonLeft_Click);
            // 
            // toolStripButtonCenter
            // 
            this.toolStripButtonCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonCenter.Image = global::SparkFormEditor.Properties.Resources.center;
            resources.ApplyResources(this.toolStripButtonCenter, "toolStripButtonCenter");
            this.toolStripButtonCenter.Name = "toolStripButtonCenter";
            this.toolStripButtonCenter.Click += new System.EventHandler(this.toolStripButtonCenter_Click);
            // 
            // toolStripButtonRight
            // 
            this.toolStripButtonRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonRight.Image = global::SparkFormEditor.Properties.Resources.right;
            resources.ApplyResources(this.toolStripButtonRight, "toolStripButtonRight");
            this.toolStripButtonRight.Name = "toolStripButtonRight";
            this.toolStripButtonRight.Click += new System.EventHandler(this.toolStripButtonRight_Click);
            // 
            // toolStripSeparatorAlign
            // 
            this.toolStripSeparatorAlign.Name = "toolStripSeparatorAlign";
            resources.ApplyResources(this.toolStripSeparatorAlign, "toolStripSeparatorAlign");
            // 
            // toolStripButtonUndo
            // 
            this.toolStripButtonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonUndo, "toolStripButtonUndo");
            this.toolStripButtonUndo.Name = "toolStripButtonUndo";
            this.toolStripButtonUndo.Click += new System.EventHandler(this.toolStripButtonUndo_Click);
            // 
            // toolStripButtonRedo
            // 
            this.toolStripButtonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonRedo, "toolStripButtonRedo");
            this.toolStripButtonRedo.Name = "toolStripButtonRedo";
            this.toolStripButtonRedo.Click += new System.EventHandler(this.toolStripButtonRedo_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // toolStripButtonCopy
            // 
            this.toolStripButtonCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonCopy, "toolStripButtonCopy");
            this.toolStripButtonCopy.Name = "toolStripButtonCopy";
            this.toolStripButtonCopy.Click += new System.EventHandler(this.toolStripButtonCopy_Click);
            // 
            // toolStripButtonCut
            // 
            this.toolStripButtonCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonCut, "toolStripButtonCut");
            this.toolStripButtonCut.Name = "toolStripButtonCut";
            this.toolStripButtonCut.Click += new System.EventHandler(this.toolStripButtonCut_Click);
            // 
            // toolStripButtonPaste
            // 
            this.toolStripButtonPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonPaste, "toolStripButtonPaste");
            this.toolStripButtonPaste.Name = "toolStripButtonPaste";
            this.toolStripButtonPaste.Click += new System.EventHandler(this.toolStripButtonPaste_Click);
            // 
            // toolStripButtonClear
            // 
            this.toolStripButtonClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonClear, "toolStripButtonClear");
            this.toolStripButtonClear.Name = "toolStripButtonClear";
            this.toolStripButtonClear.Click += new System.EventHandler(this.toolStripButtonClear_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // toolStripButtonOneLine
            // 
            this.toolStripButtonOneLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonOneLine, "toolStripButtonOneLine");
            this.toolStripButtonOneLine.Name = "toolStripButtonOneLine";
            this.toolStripButtonOneLine.Click += new System.EventHandler(this.toolStripButtonOneLine_Click);
            // 
            // toolStripButtonDoubleLine
            // 
            this.toolStripButtonDoubleLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonDoubleLine, "toolStripButtonDoubleLine");
            this.toolStripButtonDoubleLine.Name = "toolStripButtonDoubleLine";
            this.toolStripButtonDoubleLine.Click += new System.EventHandler(this.toolStripButtonDoubleLine_Click);
            // 
            // toolStripButtonClearFormat
            // 
            this.toolStripButtonClearFormat.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonClearFormat, "toolStripButtonClearFormat");
            this.toolStripButtonClearFormat.Name = "toolStripButtonClearFormat";
            this.toolStripButtonClearFormat.Click += new System.EventHandler(this.toolStripButtonClearFormat_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            resources.ApplyResources(this.toolStripSeparator10, "toolStripSeparator10");
            // 
            // toolStripButtonDefaultRowLine
            // 
            this.toolStripButtonDefaultRowLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonDefaultRowLine.Image = global::SparkFormEditor.Properties.Resources.redLine;
            resources.ApplyResources(this.toolStripButtonDefaultRowLine, "toolStripButtonDefaultRowLine");
            this.toolStripButtonDefaultRowLine.Name = "toolStripButtonDefaultRowLine";
            this.toolStripButtonDefaultRowLine.DropDownOpening += new System.EventHandler(this.toolStripButtonDefaultRowLine_DropDownOpening);
            this.toolStripButtonDefaultRowLine.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStripButtonDefaultRowLine_DropDownItemClicked);
            this.toolStripButtonDefaultRowLine.Click += new System.EventHandler(this.toolStripButtonDefaultRowLine_Click);
            // 
            // toolStripButtonLine
            // 
            this.toolStripButtonLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonLine.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemCell,
            this.toolStripSeparator2,
            this.toolStripMenuItemRow,
            this.toolStripSeparator26,
            this.关于行的其他操作toolStripMenuItem,
            this.页尾斜线ToolStripMenuItem});
            this.toolStripButtonLine.Image = global::SparkFormEditor.Properties.Resources.边框线;
            resources.ApplyResources(this.toolStripButtonLine, "toolStripButtonLine");
            this.toolStripButtonLine.Name = "toolStripButtonLine";
            // 
            // toolStripMenuItemCell
            // 
            this.toolStripMenuItemCell.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemCellLineRed,
            this.toolStripMenuItemCellLineGreen,
            this.toolStripMenuItemCellLineBlue,
            this.toolStripMenuItemCellLineBlack,
            this.toolStripMenuItemCellLineDefault,
            this.toolStripSeparator15,
            this.toolStripMenuItemCellLineZ,
            this.toolStripMenuItemCellLineN,
            this.toolStripSeparator19,
            this.toolStripMenuItemCellHideTopLine});
            this.toolStripMenuItemCell.Name = "toolStripMenuItemCell";
            resources.ApplyResources(this.toolStripMenuItemCell, "toolStripMenuItemCell");
            this.toolStripMenuItemCell.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStripMenuItemCell_DropDownItemClicked);
            // 
            // toolStripMenuItemCellLineRed
            // 
            this.toolStripMenuItemCellLineRed.Name = "toolStripMenuItemCellLineRed";
            resources.ApplyResources(this.toolStripMenuItemCellLineRed, "toolStripMenuItemCellLineRed");
            // 
            // toolStripMenuItemCellLineGreen
            // 
            this.toolStripMenuItemCellLineGreen.Name = "toolStripMenuItemCellLineGreen";
            resources.ApplyResources(this.toolStripMenuItemCellLineGreen, "toolStripMenuItemCellLineGreen");
            // 
            // toolStripMenuItemCellLineBlue
            // 
            this.toolStripMenuItemCellLineBlue.Name = "toolStripMenuItemCellLineBlue";
            resources.ApplyResources(this.toolStripMenuItemCellLineBlue, "toolStripMenuItemCellLineBlue");
            // 
            // toolStripMenuItemCellLineBlack
            // 
            this.toolStripMenuItemCellLineBlack.Name = "toolStripMenuItemCellLineBlack";
            resources.ApplyResources(this.toolStripMenuItemCellLineBlack, "toolStripMenuItemCellLineBlack");
            // 
            // toolStripMenuItemCellLineDefault
            // 
            this.toolStripMenuItemCellLineDefault.Name = "toolStripMenuItemCellLineDefault";
            resources.ApplyResources(this.toolStripMenuItemCellLineDefault, "toolStripMenuItemCellLineDefault");
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            resources.ApplyResources(this.toolStripSeparator15, "toolStripSeparator15");
            // 
            // toolStripMenuItemCellLineZ
            // 
            this.toolStripMenuItemCellLineZ.Name = "toolStripMenuItemCellLineZ";
            resources.ApplyResources(this.toolStripMenuItemCellLineZ, "toolStripMenuItemCellLineZ");
            // 
            // toolStripMenuItemCellLineN
            // 
            this.toolStripMenuItemCellLineN.Name = "toolStripMenuItemCellLineN";
            resources.ApplyResources(this.toolStripMenuItemCellLineN, "toolStripMenuItemCellLineN");
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            resources.ApplyResources(this.toolStripSeparator19, "toolStripSeparator19");
            // 
            // toolStripMenuItemCellHideTopLine
            // 
            this.toolStripMenuItemCellHideTopLine.Name = "toolStripMenuItemCellHideTopLine";
            resources.ApplyResources(this.toolStripMenuItemCellHideTopLine, "toolStripMenuItemCellHideTopLine");
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // toolStripMenuItemRow
            // 
            this.toolStripMenuItemRow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRowLineRed,
            this.toolStripMenuItemRowLineGreen,
            this.toolStripMenuItemRowLineBlue,
            this.toolStripMenuItemRowLineBlack,
            this.toolStripMenuItemRowLineDefault,
            this.toolStripSeparator22,
            this.toolStripMenuItemRectLine,
            this.ToolStripMenuItemRectDoubleLine,
            this.ToolStripMenuItemTopBottomLine,
            this.ToolStripMenuItemTopBottomDoubleLine,
            this.ToolStripMenuItemBottomLine,
            this.ToolStripMenuItemBottomDoubleLine,
            this.ToolStripMenuItemTopLine,
            this.ToolStripMenuItemTopDoubleLine,
            this.ToolStripMenuItemBottomLineDefault});
            this.toolStripMenuItemRow.Name = "toolStripMenuItemRow";
            resources.ApplyResources(this.toolStripMenuItemRow, "toolStripMenuItemRow");
            this.toolStripMenuItemRow.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStripMenuItemRow_DropDownItemClicked);
            // 
            // toolStripMenuItemRowLineRed
            // 
            this.toolStripMenuItemRowLineRed.Name = "toolStripMenuItemRowLineRed";
            resources.ApplyResources(this.toolStripMenuItemRowLineRed, "toolStripMenuItemRowLineRed");
            // 
            // toolStripMenuItemRowLineGreen
            // 
            this.toolStripMenuItemRowLineGreen.Name = "toolStripMenuItemRowLineGreen";
            resources.ApplyResources(this.toolStripMenuItemRowLineGreen, "toolStripMenuItemRowLineGreen");
            // 
            // toolStripMenuItemRowLineBlue
            // 
            this.toolStripMenuItemRowLineBlue.Name = "toolStripMenuItemRowLineBlue";
            resources.ApplyResources(this.toolStripMenuItemRowLineBlue, "toolStripMenuItemRowLineBlue");
            // 
            // toolStripMenuItemRowLineBlack
            // 
            this.toolStripMenuItemRowLineBlack.Name = "toolStripMenuItemRowLineBlack";
            resources.ApplyResources(this.toolStripMenuItemRowLineBlack, "toolStripMenuItemRowLineBlack");
            // 
            // toolStripMenuItemRowLineDefault
            // 
            this.toolStripMenuItemRowLineDefault.Name = "toolStripMenuItemRowLineDefault";
            resources.ApplyResources(this.toolStripMenuItemRowLineDefault, "toolStripMenuItemRowLineDefault");
            // 
            // toolStripSeparator22
            // 
            this.toolStripSeparator22.Name = "toolStripSeparator22";
            resources.ApplyResources(this.toolStripSeparator22, "toolStripSeparator22");
            // 
            // toolStripMenuItemRectLine
            // 
            this.toolStripMenuItemRectLine.Name = "toolStripMenuItemRectLine";
            resources.ApplyResources(this.toolStripMenuItemRectLine, "toolStripMenuItemRectLine");
            this.toolStripMenuItemRectLine.Tag = "行线";
            // 
            // ToolStripMenuItemRectDoubleLine
            // 
            this.ToolStripMenuItemRectDoubleLine.Name = "ToolStripMenuItemRectDoubleLine";
            resources.ApplyResources(this.ToolStripMenuItemRectDoubleLine, "ToolStripMenuItemRectDoubleLine");
            this.ToolStripMenuItemRectDoubleLine.Tag = "行线";
            // 
            // ToolStripMenuItemTopBottomLine
            // 
            this.ToolStripMenuItemTopBottomLine.Name = "ToolStripMenuItemTopBottomLine";
            resources.ApplyResources(this.ToolStripMenuItemTopBottomLine, "ToolStripMenuItemTopBottomLine");
            this.ToolStripMenuItemTopBottomLine.Tag = "行线";
            // 
            // ToolStripMenuItemTopBottomDoubleLine
            // 
            this.ToolStripMenuItemTopBottomDoubleLine.Name = "ToolStripMenuItemTopBottomDoubleLine";
            resources.ApplyResources(this.ToolStripMenuItemTopBottomDoubleLine, "ToolStripMenuItemTopBottomDoubleLine");
            this.ToolStripMenuItemTopBottomDoubleLine.Tag = "行线";
            // 
            // ToolStripMenuItemBottomLine
            // 
            this.ToolStripMenuItemBottomLine.Name = "ToolStripMenuItemBottomLine";
            resources.ApplyResources(this.ToolStripMenuItemBottomLine, "ToolStripMenuItemBottomLine");
            this.ToolStripMenuItemBottomLine.Tag = "行线";
            // 
            // ToolStripMenuItemBottomDoubleLine
            // 
            this.ToolStripMenuItemBottomDoubleLine.Name = "ToolStripMenuItemBottomDoubleLine";
            resources.ApplyResources(this.ToolStripMenuItemBottomDoubleLine, "ToolStripMenuItemBottomDoubleLine");
            this.ToolStripMenuItemBottomDoubleLine.Tag = "行线";
            // 
            // ToolStripMenuItemTopLine
            // 
            this.ToolStripMenuItemTopLine.Name = "ToolStripMenuItemTopLine";
            resources.ApplyResources(this.ToolStripMenuItemTopLine, "ToolStripMenuItemTopLine");
            this.ToolStripMenuItemTopLine.Tag = "行线";
            // 
            // ToolStripMenuItemTopDoubleLine
            // 
            this.ToolStripMenuItemTopDoubleLine.Name = "ToolStripMenuItemTopDoubleLine";
            resources.ApplyResources(this.ToolStripMenuItemTopDoubleLine, "ToolStripMenuItemTopDoubleLine");
            this.ToolStripMenuItemTopDoubleLine.Tag = "行线";
            // 
            // ToolStripMenuItemBottomLineDefault
            // 
            this.ToolStripMenuItemBottomLineDefault.Name = "ToolStripMenuItemBottomLineDefault";
            resources.ApplyResources(this.ToolStripMenuItemBottomLineDefault, "ToolStripMenuItemBottomLineDefault");
            this.ToolStripMenuItemBottomLineDefault.Tag = "行线";
            // 
            // toolStripSeparator26
            // 
            this.toolStripSeparator26.Name = "toolStripSeparator26";
            resources.ApplyResources(this.toolStripSeparator26, "toolStripSeparator26");
            // 
            // 关于行的其他操作toolStripMenuItem
            // 
            this.关于行的其他操作toolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.隐藏行上边框线ToolStripMenuItem,
            this.显示行上边框线toolStripMenuItem,
            this.隐藏行下边框线ToolStripMenuItem,
            this.toolStripSeparator27,
            this.合并行ToolStripMenuItem});
            this.关于行的其他操作toolStripMenuItem.Name = "关于行的其他操作toolStripMenuItem";
            resources.ApplyResources(this.关于行的其他操作toolStripMenuItem, "关于行的其他操作toolStripMenuItem");
            // 
            // 隐藏行上边框线ToolStripMenuItem
            // 
            this.隐藏行上边框线ToolStripMenuItem.Name = "隐藏行上边框线ToolStripMenuItem";
            resources.ApplyResources(this.隐藏行上边框线ToolStripMenuItem, "隐藏行上边框线ToolStripMenuItem");
            // 
            // 显示行上边框线toolStripMenuItem
            // 
            this.显示行上边框线toolStripMenuItem.Name = "显示行上边框线toolStripMenuItem";
            resources.ApplyResources(this.显示行上边框线toolStripMenuItem, "显示行上边框线toolStripMenuItem");
            // 
            // 隐藏行下边框线ToolStripMenuItem
            // 
            this.隐藏行下边框线ToolStripMenuItem.Name = "隐藏行下边框线ToolStripMenuItem";
            resources.ApplyResources(this.隐藏行下边框线ToolStripMenuItem, "隐藏行下边框线ToolStripMenuItem");
            // 
            // toolStripSeparator27
            // 
            this.toolStripSeparator27.Name = "toolStripSeparator27";
            resources.ApplyResources(this.toolStripSeparator27, "toolStripSeparator27");
            // 
            // 合并行ToolStripMenuItem
            // 
            this.合并行ToolStripMenuItem.Name = "合并行ToolStripMenuItem";
            resources.ApplyResources(this.合并行ToolStripMenuItem, "合并行ToolStripMenuItem");
            // 
            // 页尾斜线ToolStripMenuItem
            // 
            this.页尾斜线ToolStripMenuItem.Name = "页尾斜线ToolStripMenuItem";
            resources.ApplyResources(this.页尾斜线ToolStripMenuItem, "页尾斜线ToolStripMenuItem");
            // 
            // toolStripButtonMergeCell
            // 
            this.toolStripButtonMergeCell.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonMergeCell, "toolStripButtonMergeCell");
            this.toolStripButtonMergeCell.Name = "toolStripButtonMergeCell";
            this.toolStripButtonMergeCell.Click += new System.EventHandler(this.toolStripButtonMergeCell_Click);
            // 
            // toolStripSeparator24
            // 
            this.toolStripSeparator24.Name = "toolStripSeparator24";
            resources.ApplyResources(this.toolStripSeparator24, "toolStripSeparator24");
            // 
            // toolStripButtonSum
            // 
            this.toolStripButtonSum.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonSum, "toolStripButtonSum");
            this.toolStripButtonSum.Name = "toolStripButtonSum";
            this.toolStripButtonSum.Click += new System.EventHandler(this.toolStripButtonSum_Click);
            // 
            // toolStripSeparator28
            // 
            this.toolStripSeparator28.Name = "toolStripSeparator28";
            resources.ApplyResources(this.toolStripSeparator28, "toolStripSeparator28");
            // 
            // toolStripButtonSumGroup
            // 
            this.toolStripButtonSumGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonSumGroup, "toolStripButtonSumGroup");
            this.toolStripButtonSumGroup.Name = "toolStripButtonSumGroup";
            this.toolStripButtonSumGroup.Click += new System.EventHandler(this.toolStripButtonSumGroup_Click);
            // 
            // pnl_tool1
            // 
            this.pnl_tool1.Controls.Add(this.toolStripFunc);
            resources.ApplyResources(this.pnl_tool1, "pnl_tool1");
            this.pnl_tool1.Name = "pnl_tool1";
            // 
            // toolStripFunc
            // 
            this.toolStripFunc.BackColor = System.Drawing.Color.White;
            this.toolStripFunc.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripFunc.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonCreateForm,
            this.toolStripButtonCreatePage,
            this.toolStripSeparator7,
            this.toolStripButtonSave,
            this.toolStripSeparator8,
            this.tBnt_InsertRowUp,
            this.tBnt_InsertRowDown,
            this.tBnt_DelRow,
            this.toolStripSeparator9,
            this.toolStripButtonDelPage,
            this.toolStripButtonDelAllPage,
            this.toolStripSeparator6,
            this.toolStripButtonPrintPage,
            this.toolStripButtonPrintAllPage,
            this.toolStripButtonContinuePrint,
            this.toolStripSeparator11,
            this.toolStripButtonDelockApply,
            this.toolStripButton1});
            resources.ApplyResources(this.toolStripFunc, "toolStripFunc");
            this.toolStripFunc.Name = "toolStripFunc";
            this.toolStripFunc.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // toolStripButtonCreateForm
            // 
            this.toolStripButtonCreateForm.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem4});
            resources.ApplyResources(this.toolStripButtonCreateForm, "toolStripButtonCreateForm");
            this.toolStripButtonCreateForm.Name = "toolStripButtonCreateForm";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem5});
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            resources.ApplyResources(this.toolStripMenuItem4, "toolStripMenuItem4");
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            resources.ApplyResources(this.toolStripMenuItem5, "toolStripMenuItem5");
            // 
            // toolStripButtonCreatePage
            // 
            resources.ApplyResources(this.toolStripButtonCreatePage, "toolStripButtonCreatePage");
            this.toolStripButtonCreatePage.Name = "toolStripButtonCreatePage";
            this.toolStripButtonCreatePage.Click += new System.EventHandler(this.toolStripButtonCreatePage_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            // 
            // toolStripButtonSave
            // 
            resources.ApplyResources(this.toolStripButtonSave, "toolStripButtonSave");
            this.toolStripButtonSave.Name = "toolStripButtonSave";
            this.toolStripButtonSave.Click += new System.EventHandler(this.toolStripButtonSave_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            resources.ApplyResources(this.toolStripSeparator8, "toolStripSeparator8");
            // 
            // tBnt_InsertRowUp
            // 
            resources.ApplyResources(this.tBnt_InsertRowUp, "tBnt_InsertRowUp");
            this.tBnt_InsertRowUp.Name = "tBnt_InsertRowUp";
            this.tBnt_InsertRowUp.Click += new System.EventHandler(this.tBnt_InsertRowUp_Click);
            // 
            // tBnt_InsertRowDown
            // 
            resources.ApplyResources(this.tBnt_InsertRowDown, "tBnt_InsertRowDown");
            this.tBnt_InsertRowDown.Name = "tBnt_InsertRowDown";
            this.tBnt_InsertRowDown.Click += new System.EventHandler(this.tBnt_InsertRowDwon_Click);
            // 
            // tBnt_DelRow
            // 
            resources.ApplyResources(this.tBnt_DelRow, "tBnt_DelRow");
            this.tBnt_DelRow.Name = "tBnt_DelRow";
            this.tBnt_DelRow.Click += new System.EventHandler(this.tBnt_DelRow_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            resources.ApplyResources(this.toolStripSeparator9, "toolStripSeparator9");
            // 
            // toolStripButtonDelPage
            // 
            resources.ApplyResources(this.toolStripButtonDelPage, "toolStripButtonDelPage");
            this.toolStripButtonDelPage.Name = "toolStripButtonDelPage";
            this.toolStripButtonDelPage.Click += new System.EventHandler(this.toolStripButtonDelPage_Click);
            // 
            // toolStripButtonDelAllPage
            // 
            resources.ApplyResources(this.toolStripButtonDelAllPage, "toolStripButtonDelAllPage");
            this.toolStripButtonDelAllPage.Name = "toolStripButtonDelAllPage";
            this.toolStripButtonDelAllPage.Click += new System.EventHandler(this.toolStripButtonDelAllPage_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            // 
            // toolStripButtonPrintPage
            // 
            resources.ApplyResources(this.toolStripButtonPrintPage, "toolStripButtonPrintPage");
            this.toolStripButtonPrintPage.Name = "toolStripButtonPrintPage";
            this.toolStripButtonPrintPage.Click += new System.EventHandler(this.toolStripButtonPrintPage_Click);
            // 
            // toolStripButtonPrintAllPage
            // 
            resources.ApplyResources(this.toolStripButtonPrintAllPage, "toolStripButtonPrintAllPage");
            this.toolStripButtonPrintAllPage.Name = "toolStripButtonPrintAllPage";
            this.toolStripButtonPrintAllPage.Click += new System.EventHandler(this.toolStripButtonPrintAllPage_Click);
            // 
            // toolStripButtonContinuePrint
            // 
            resources.ApplyResources(this.toolStripButtonContinuePrint, "toolStripButtonContinuePrint");
            this.toolStripButtonContinuePrint.Name = "toolStripButtonContinuePrint";
            this.toolStripButtonContinuePrint.Click += new System.EventHandler(this.toolStripButtonContinuePrint_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            resources.ApplyResources(this.toolStripSeparator11, "toolStripSeparator11");
            // 
            // toolStripButtonDelockApply
            // 
            resources.ApplyResources(this.toolStripButtonDelockApply, "toolStripButtonDelockApply");
            this.toolStripButtonDelockApply.Name = "toolStripButtonDelockApply";
            // 
            // toolStripButton1
            // 
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.Name = "toolStripButton1";
            // 
            // pnlPage
            // 
            this.pnlPage.BackColor = System.Drawing.Color.Transparent;
            this.pnlPage.Controls.Add(this.uc_Pager1);
            resources.ApplyResources(this.pnlPage, "pnlPage");
            this.pnlPage.Name = "pnlPage";
            // 
            // uc_Pager1
            // 
            resources.ApplyResources(this.uc_Pager1, "uc_Pager1");
            this.uc_Pager1.Name = "uc_Pager1";
            this.uc_Pager1.NMax = 0;
            this.uc_Pager1.PageCount = 0;
            this.uc_Pager1.PageCurrent = 0;
            this.uc_Pager1.PageSize = 20;
            // 
            // contextMenuStripToolStrip
            // 
            resources.ApplyResources(this.contextMenuStripToolStrip, "contextMenuStripToolStrip");
            this.contextMenuStripToolStrip.Name = "contextMenuStripToolStrip";
            // 
            // contextMenuStripFrozen
            // 
            resources.ApplyResources(this.contextMenuStripFrozen, "contextMenuStripFrozen");
            this.contextMenuStripFrozen.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemHide,
            this.toolStripMenuItemAuto});
            this.contextMenuStripFrozen.Name = "contextMenuStripFrozen";
            this.contextMenuStripFrozen.ShowImageMargin = false;
            // 
            // toolStripMenuItemHide
            // 
            this.toolStripMenuItemHide.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItemHide.Name = "toolStripMenuItemHide";
            resources.ApplyResources(this.toolStripMenuItemHide, "toolStripMenuItemHide");
            this.toolStripMenuItemHide.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.toolStripMenuItemHide.Click += new System.EventHandler(this.toolStripMenuItemHide_Click);
            // 
            // toolStripMenuItemAuto
            // 
            this.toolStripMenuItemAuto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItemAuto.Name = "toolStripMenuItemAuto";
            resources.ApplyResources(this.toolStripMenuItemAuto, "toolStripMenuItemAuto");
            this.toolStripMenuItemAuto.Click += new System.EventHandler(this.toolStripMenuItemAuto_Click);
            // 
            // dateTimePickerSelectRYDate
            // 
            resources.ApplyResources(this.dateTimePickerSelectRYDate, "dateTimePickerSelectRYDate");
            this.dateTimePickerSelectRYDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerSelectRYDate.Name = "dateTimePickerSelectRYDate";
            // 
            // radioButtonNewHospital
            // 
            resources.ApplyResources(this.radioButtonNewHospital, "radioButtonNewHospital");
            this.radioButtonNewHospital.Name = "radioButtonNewHospital";
            this.radioButtonNewHospital.TabStop = true;
            this.radioButtonNewHospital.UseVisualStyleBackColor = true;
            // 
            // radioButtonheart
            // 
            resources.ApplyResources(this.radioButtonheart, "radioButtonheart");
            this.radioButtonheart.Name = "radioButtonheart";
            this.radioButtonheart.TabStop = true;
            this.radioButtonheart.UseVisualStyleBackColor = true;
            // 
            // radioButtonDisease2
            // 
            resources.ApplyResources(this.radioButtonDisease2, "radioButtonDisease2");
            this.radioButtonDisease2.Name = "radioButtonDisease2";
            this.radioButtonDisease2.TabStop = true;
            this.radioButtonDisease2.UseVisualStyleBackColor = true;
            // 
            // radioButtonDisease1
            // 
            resources.ApplyResources(this.radioButtonDisease1, "radioButtonDisease1");
            this.radioButtonDisease1.Name = "radioButtonDisease1";
            this.radioButtonDisease1.TabStop = true;
            this.radioButtonDisease1.UseVisualStyleBackColor = true;
            // 
            // radioButtonNurseLevel2
            // 
            resources.ApplyResources(this.radioButtonNurseLevel2, "radioButtonNurseLevel2");
            this.radioButtonNurseLevel2.Name = "radioButtonNurseLevel2";
            this.radioButtonNurseLevel2.TabStop = true;
            this.radioButtonNurseLevel2.UseVisualStyleBackColor = true;
            // 
            // radioButtonNurseLevel1
            // 
            resources.ApplyResources(this.radioButtonNurseLevel1, "radioButtonNurseLevel1");
            this.radioButtonNurseLevel1.Name = "radioButtonNurseLevel1";
            this.radioButtonNurseLevel1.TabStop = true;
            this.radioButtonNurseLevel1.UseVisualStyleBackColor = true;
            // 
            // radioButtonAllPatient
            // 
            resources.ApplyResources(this.radioButtonAllPatient, "radioButtonAllPatient");
            this.radioButtonAllPatient.Name = "radioButtonAllPatient";
            this.radioButtonAllPatient.TabStop = true;
            this.radioButtonAllPatient.UseVisualStyleBackColor = true;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // toolStripMenuItemAdd
            // 
            this.toolStripMenuItemAdd.Name = "toolStripMenuItemAdd";
            resources.ApplyResources(this.toolStripMenuItemAdd, "toolStripMenuItemAdd");
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            resources.ApplyResources(this.toolStripMenuItemDelete, "toolStripMenuItemDelete");
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // toolStripMenuItemReName
            // 
            this.toolStripMenuItemReName.Name = "toolStripMenuItemReName";
            resources.ApplyResources(this.toolStripMenuItemReName, "toolStripMenuItemReName");
            // 
            // toolStripMenuItemUp
            // 
            this.toolStripMenuItemUp.Name = "toolStripMenuItemUp";
            resources.ApplyResources(this.toolStripMenuItemUp, "toolStripMenuItemUp");
            // 
            // toolStripMenuItemDown
            // 
            this.toolStripMenuItemDown.Name = "toolStripMenuItemDown";
            resources.ApplyResources(this.toolStripMenuItemDown, "toolStripMenuItemDown");
            // 
            // contextMenuStripForImage
            // 
            this.contextMenuStripForImage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.删除ToolStripMenuItem,
            this.toolStripSeparator20,
            this.边框线ToolStripMenuItem,
            this.toolStripSeparator21,
            this.toolStripTextBoxSize});
            this.contextMenuStripForImage.Name = "contextMenuStripForImage";
            resources.ApplyResources(this.contextMenuStripForImage, "contextMenuStripForImage");
            this.contextMenuStripForImage.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.contextMenuStripForImage_Closing);
            // 
            // 删除ToolStripMenuItem
            // 
            this.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
            resources.ApplyResources(this.删除ToolStripMenuItem, "删除ToolStripMenuItem");
            this.删除ToolStripMenuItem.Click += new System.EventHandler(this.删除ToolStripMenuItem_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            resources.ApplyResources(this.toolStripSeparator20, "toolStripSeparator20");
            // 
            // 边框线ToolStripMenuItem
            // 
            this.边框线ToolStripMenuItem.Name = "边框线ToolStripMenuItem";
            resources.ApplyResources(this.边框线ToolStripMenuItem, "边框线ToolStripMenuItem");
            this.边框线ToolStripMenuItem.Click += new System.EventHandler(this.边框线ToolStripMenuItem_Click);
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            resources.ApplyResources(this.toolStripSeparator21, "toolStripSeparator21");
            // 
            // toolStripTextBoxSize
            // 
            this.toolStripTextBoxSize.Name = "toolStripTextBoxSize";
            resources.ApplyResources(this.toolStripTextBoxSize, "toolStripTextBoxSize");
            // 
            // sfd
            // 
            this.sfd.DefaultExt = "jpg";
            resources.ApplyResources(this.sfd, "sfd");
            this.sfd.FileOk += new System.ComponentModel.CancelEventHandler(this.sfd_FileOk);
            // 
            // sfdBG
            // 
            this.sfdBG.DefaultExt = "jpg";
            resources.ApplyResources(this.sfdBG, "sfdBG");
            this.sfdBG.FileOk += new System.ComponentModel.CancelEventHandler(this.sfdBG_FileOk);
            // 
            // sfdCSV
            // 
            this.sfdCSV.DefaultExt = "csv";
            resources.ApplyResources(this.sfdCSV, "sfdCSV");
            this.sfdCSV.FileOk += new System.ComponentModel.CancelEventHandler(this.sfdCSV_FileOk);
            // 
            // sfdHtml
            // 
            this.sfdHtml.DefaultExt = "htm";
            resources.ApplyResources(this.sfdHtml, "sfdHtml");
            // 
            // sfdSaveAsXml
            // 
            this.sfdSaveAsXml.DefaultExt = "xml";
            resources.ApplyResources(this.sfdSaveAsXml, "sfdSaveAsXml");
            // 
            // radioButtonSpecialNurse
            // 
            resources.ApplyResources(this.radioButtonSpecialNurse, "radioButtonSpecialNurse");
            this.radioButtonSpecialNurse.Name = "radioButtonSpecialNurse";
            this.radioButtonSpecialNurse.TabStop = true;
            this.radioButtonSpecialNurse.UseVisualStyleBackColor = true;
            // 
            // radioButtonNurseLevel3
            // 
            resources.ApplyResources(this.radioButtonNurseLevel3, "radioButtonNurseLevel3");
            this.radioButtonNurseLevel3.Name = "radioButtonNurseLevel3";
            this.radioButtonNurseLevel3.TabStop = true;
            this.radioButtonNurseLevel3.UseVisualStyleBackColor = true;
            // 
            // pictureBoxFrozen
            // 
            this.pictureBoxFrozen.BackColor = System.Drawing.Color.White;
            this.pictureBoxFrozen.ContextMenuStrip = this.contextMenuStripFrozen;
            this.pictureBoxFrozen.Cursor = System.Windows.Forms.Cursors.HSplit;
            resources.ApplyResources(this.pictureBoxFrozen, "pictureBoxFrozen");
            this.pictureBoxFrozen.Name = "pictureBoxFrozen";
            this.pictureBoxFrozen.TabStop = false;
            this.pictureBoxFrozen.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxFrozen_MouseDoubleClick);
            this.pictureBoxFrozen.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBoxFrozen_MouseDown);
            this.pictureBoxFrozen.MouseEnter += new System.EventHandler(this.pictureBoxFrozen_MouseEnter);
            this.pictureBoxFrozen.MouseLeave += new System.EventHandler(this.pictureBoxFrozen_MouseLeave);
            this.pictureBoxFrozen.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxFrozen_MouseMove);
            // 
            // toolStripButtonTcData
            // 
            this.toolStripButtonTcData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonTcData, "toolStripButtonTcData");
            this.toolStripButtonTcData.Name = "toolStripButtonTcData";
            this.toolStripButtonTcData.Click += new System.EventHandler(this.toolStripButtonTcData_Click);
            // 
            // UC_NurseFormEditor
            // 
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel3);
            resources.ApplyResources(this, "$this");
            this.Name = "UC_NurseFormEditor";
            this.panel3.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBackGround)).EndInit();
            this.pictureBoxBackGround.ResumeLayout(false);
            this.pictureBoxBackGround.PerformLayout();
            this.pnlToolSet.ResumeLayout(false);
            this.pnlToolSet.PerformLayout();
            this.toolStripToolBarWord.ResumeLayout(false);
            this.toolStripToolBarWord.PerformLayout();
            this.pnl_tool1.ResumeLayout(false);
            this.pnl_tool1.PerformLayout();
            this.toolStripFunc.ResumeLayout(false);
            this.toolStripFunc.PerformLayout();
            this.pnlPage.ResumeLayout(false);
            this.contextMenuStripFrozen.ResumeLayout(false);
            this.contextMenuStripForImage.ResumeLayout(false);
            this.contextMenuStripForImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFrozen)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        //private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSize;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAdd;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemReName;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUp;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDown;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripForImage;
        private System.Windows.Forms.ToolStripMenuItem 删除ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 边框线ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 布局ToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog sfd;
        private System.Windows.Forms.SaveFileDialog sfdBG;
        private System.Windows.Forms.SaveFileDialog sfdCSV;
        private System.Windows.Forms.SaveFileDialog sfdHtml;
        private System.Windows.Forms.SaveFileDialog sfdSaveAsXml;
        //private LoadingProgress loadingProgress1;
        private System.Windows.Forms.RadioButton radioButtonAllPatient;
        private System.Windows.Forms.RadioButton radioButtonNurseLevel2;
        private System.Windows.Forms.RadioButton radioButtonNurseLevel1;
        private System.Windows.Forms.RadioButton radioButtonSpecialNurse;
        private System.Windows.Forms.RadioButton radioButtonNurseLevel3;
        private System.Windows.Forms.RadioButton radioButtonNewHospital;
        private System.Windows.Forms.RadioButton radioButtonheart;
        private System.Windows.Forms.RadioButton radioButtonDisease2;
        private System.Windows.Forms.RadioButton radioButtonDisease1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSize;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator21;
        private System.Windows.Forms.DateTimePicker dateTimePickerSelectRYDate;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripToolStrip;
        private System.Windows.Forms.PictureBox pictureBoxFrozen;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFrozen;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHide;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAuto;
        private Panel pnlPage;
        internal SparkFormEditor.UC_Pager uc_Pager1;
        public Panel panelMain;
        public TreeView treeViewPatients;
        public PictureBox pictureBoxBackGround;
        private TransparentLabel lblShowSearch;
        public Panel panelShade;
        private Panel pnlToolSet;
        private ToolStrip toolStripToolBarWord;
        private ToolStripButton toolStripButtonBold;
        private ToolStripButton toolStripButtonItalic;
        private ToolStripButton toolStripButtonUnderline;
        private ToolStripButton toolStripButtonStrikeout;
        private ToolStripSplitButton toolStripButtonColor;
        private ToolStripMenuItem toolStripButtonColorRed;
        private ToolStripMenuItem toolStripButtonColorGreen;
        private ToolStripMenuItem toolStripButtonColorBlue;
        private ToolStripMenuItem toolStripButtonColorBlack;
        private ToolStripSeparator toolStripSeparatorFont;
        private ToolStripButton toolStripButtonUp;
        private ToolStripButton toolStripButtonDown;
        private ToolStripSeparator toolStripSeparatorFormat;
        private ToolStripButton toolStripButtonLeft;
        private ToolStripButton toolStripButtonCenter;
        private ToolStripButton toolStripButtonRight;
        private ToolStripSeparator toolStripSeparatorAlign;
        private ToolStripButton toolStripButtonOneLine;
        private ToolStripButton toolStripButtonDoubleLine;
        private ToolStripButton toolStripButtonClearFormat;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripSplitButton toolStripButtonDefaultRowLine;
        private ToolStripSplitButton toolStripButtonLine;
        private ToolStripMenuItem toolStripMenuItemCell;
        private ToolStripMenuItem toolStripMenuItemCellLineRed;
        private ToolStripMenuItem toolStripMenuItemCellLineGreen;
        private ToolStripMenuItem toolStripMenuItemCellLineBlue;
        private ToolStripMenuItem toolStripMenuItemCellLineBlack;
        private ToolStripMenuItem toolStripMenuItemCellLineDefault;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripMenuItem toolStripMenuItemCellLineZ;
        private ToolStripMenuItem toolStripMenuItemCellLineN;
        private ToolStripSeparator toolStripSeparator19;
        private ToolStripMenuItem toolStripMenuItemCellHideTopLine;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem toolStripMenuItemRow;
        private ToolStripMenuItem toolStripMenuItemRowLineRed;
        private ToolStripMenuItem toolStripMenuItemRowLineGreen;
        private ToolStripMenuItem toolStripMenuItemRowLineBlue;
        private ToolStripMenuItem toolStripMenuItemRowLineBlack;
        private ToolStripMenuItem toolStripMenuItemRowLineDefault;
        private ToolStripSeparator toolStripSeparator22;
        private ToolStripMenuItem toolStripMenuItemRectLine;
        private ToolStripMenuItem ToolStripMenuItemRectDoubleLine;
        private ToolStripMenuItem ToolStripMenuItemTopBottomLine;
        private ToolStripMenuItem ToolStripMenuItemTopBottomDoubleLine;
        private ToolStripMenuItem ToolStripMenuItemBottomLine;
        private ToolStripMenuItem ToolStripMenuItemBottomDoubleLine;
        private ToolStripMenuItem ToolStripMenuItemTopLine;
        private ToolStripMenuItem ToolStripMenuItemTopDoubleLine;
        private ToolStripMenuItem ToolStripMenuItemBottomLineDefault;
        private ToolStripSeparator toolStripSeparator26;
        private ToolStripMenuItem 关于行的其他操作toolStripMenuItem;
        private ToolStripMenuItem 隐藏行上边框线ToolStripMenuItem;
        private ToolStripMenuItem 显示行上边框线toolStripMenuItem;
        private ToolStripMenuItem 隐藏行下边框线ToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator27;
        private ToolStripMenuItem 合并行ToolStripMenuItem;
        private ToolStripMenuItem 页尾斜线ToolStripMenuItem;
        private ToolStripButton toolStripButtonMergeCell;
        private ToolStripSeparator toolStripSeparator24;
        public ToolStripButton toolStripButtonSum;
        private ToolStripSeparator toolStripSeparator28;
        private ToolStripButton toolStripButtonUndo;
        private ToolStripButton toolStripButtonRedo;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripButton toolStripButtonCopy;
        private ToolStripButton toolStripButtonCut;
        private ToolStripButton toolStripButtonPaste;
        private ToolStripButton toolStripButtonClear;
        private Panel pnl_tool1;
        private ToolStrip toolStripFunc;
        private ToolStripButton toolStripButtonSave;
        private ToolStripButton toolStripButtonPrintPage;
        private ToolStripButton toolStripButtonCreatePage;
        private ToolStripButton toolStripButtonDelAllPage;
        private ToolStripButton toolStripButtonDelPage;
        private ToolStripButton toolStripButtonPrintAllPage;
        private ToolStripButton toolStripButtonContinuePrint;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripButton tBnt_InsertRowUp;
        private ToolStripButton tBnt_InsertRowDown;
        private ToolStripButton tBnt_DelRow;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripDropDownButton toolStripButtonCreateForm;
        private ToolStripButton toolStripButtonDelockApply;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripMenuItem toolStripMenuItem5;
        private ToolStripButton toolStripButton1;
        public ToolStripButton toolStripButtonSumGroup;
        private ToolStripButton toolStripButtonTcData;
    }
}