using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SparkFormEditor.Model;
using System.Xml;
using SparkFormEditor.Foundation;

namespace SparkFormEditor
{
    public partial class SparkPaperRecord : SparkFormEditor, IWidget, ISealedBook, IVectorManaged
    {
        //表单的"FormType"，医生护士的权限，本来是在二维表的。现在也放到根节点中。这个创建表单的时候来通过模板设定。（客户端，或者批量新建的时候）
        public SparkPaperRecord()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.BackColor = Color.White;//防止打开的时候背景色闪动
            this.uc_Pager1.EventFlip += new SparkFormEditor.EventFlipHandler(formPager1_EventFlip);
        }

        public delegate void FilpHandle();


        /// <summary>
        /// 加载表单数据
        /// </summary>
        /// <param name="patientInfo">病人基本信息</param>
        /// <param name="patientFormData">病人表单数据，没有则null</param>
        /// <param name="templateValue">模板数据</param>
        /// <param name="templateID">模板id</param>
        /// <param name="templateName">模板名称</param>
        public void Load(EntPatientInfoEx patientInfo, string patientFormData, XmlDocument templateValue, string templateID, string templateName)
        {
            try
            {
                if (!string.IsNullOrEmpty(this._TemplateName)) this.ClearMe();
                this.LoadData(patientInfo, patientFormData, templateValue, templateID, templateName);
                this.LoadFromDetail(null);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 加载表单数据
        /// </summary>
        /// <param name="patientDR">病人基本信息</param>
        /// <param name="patientFormData">病人表单数据，没有则null</param>
        /// <param name="templateValue">模板数据</param>
        /// <param name="templateID">模板id</param>
        /// <param name="templateName">模板名称</param>
        public void Load(DataRow patientDR, string patientFormData, XmlDocument templateValue, string templateID, string templateName)
        {
            try
            {
                ClearControls(); //清楚界面上的全部控件
                EntPatientInfoEx patientInfo = null;
                if (Comm.DataRowToEnt(patientDR, patientInfo))
                {
                    this.LoadData(patientInfo, patientFormData, templateValue, templateID, templateName);
                    this.LoadFromDetail(null);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void Load(EntPatientInfoEx patientInfo)
        {
            LoadData(patientInfo);
        }

        internal void Load()
        {
            try
            {
                this.LoadData();
                this.treeViewPatients_NodeMouseDoubleClick(null, null);
                this.uc_Pager1.EventFlip += new SparkFormEditor.EventFlipHandler(formPager1_EventFlip);

            }
            catch (Exception ex)
            {
            }
        }

        bool formPager1_EventFlip(int iPage)
        {
            this.pictureBoxBackGround.Focus();
            if (IsNeedSave)
            {
                Comm.ShowWarningMessage(string.Format("当前打开的{0}还未保存，请先执行保存操作", _TemplateName));
                return false;
            }


            //this.LoadFromDetail(this.formPager1.PageCurrent.ToString());
            this.LoadFromDetail(iPage.ToString());
            return true;
        }

        /// <summary>
        /// 获取文书的当前总页数
        /// </summary>
        public int PagesCount
        {
            get
            {
                //return 100;
                return this._CurrentTemplateNameTreeNode.Nodes.Count;
            }
        }

        int _SequenceNo = 0;
        /// <summary>
        /// 防止覆盖，排他更新用的序列号
        /// </summary>
        public int SequenceNo
        {
            get
            {
                return _SequenceNo;
            }

            set
            {
                _SequenceNo = value;
            }
        }

        /// <summary>
        /// 判断文书是否已经修改，需要提示保存
        /// </summary>
        public bool BookNeedSave
        {
            get
            {
                //return false;//_IsNeedSave
                return this.IsNeedSave && this.toolStripSaveEnabled; //没有编辑权限的话，肯定不要提示的
            }

            //set;
        }

        public Color WidgetSkinColor
        {
            set
            {
            }
        }
        public string WidgetConfig
        {
            set { }
            get { return ""; }
        }
        public string WidgetUnit
        {
            set
            {
            }
        }
        public string WidgetUsername
        {
            set
            {
            }
        }
        public string WidgetPassword
        {
            set
            {
            }
        }
        public string[] WidgetOffices
        {
            set
            {
            }
        }

        public event EventHandler WidgetApply;
        public void BookGoTo(string strDirection)
        {
        }
        public void BookUndo()
        {
        }
        public void BookRedo()
        {
        }
        public void BookCopy()
        {
        }
        public void BookPaste()
        {
        }
        public bool BookCheckAndVerify(string strSomeParameters)
        {
            return true;
        }
        public void BookSign()
        {
        }
        public void BookFontColor(string strColor)
        {
        }
        public void BookPrint(string strDirection)
        {
            this.PrintCurrentPage(null);
            //PrintCurrentPage("续打"); //表示续打
        }
        public void BookPrintAll(string strDirection)
        {
            this.PrintAllPage();
        }
        public int BookSave()
        {
            //return this.SaveRecruit();
            return this.SaveData();
        }

        /// <summary>
        /// 新建页
        /// </summary>
        public int BookNewPage()
        {
            return this.AddPageRecruit();
        }

        /// <summary>
        /// 另存为
        /// </summary>
        public void BookSaveAs()
        {
            //保存到本地
            this.SaveAs();
        }

        public void BookAddParagraph(string strTemplate)
        {
        }
        public void BookAssistantEntry(string strContent)
        {
            this.AddWord(strContent);
        }

        public event EventHandler BookIsDirty;
        public event EventHandler BookCanSaveNow;

        /// <summary>
        /// 初始化模板列表
        /// </summary>
        /// <param name="list"></param>
        public void InitTemplate(List<GlobalVariable.Template> list)
        {
            base.InitTemplate(list);
        }

        /// <summary>
        /// 设置表单只读
        /// </summary>
        public void SetReadOnly()
        {
            this._IsLocked = true;
            base.SetAllControlDisabled(true);
        }

        #region IVectorManaged
        public void VectorInit(IVector vector)
        {
            GlobalVariable.Vector = vector;
        }
        #endregion

        public void SetType(EditorTypeEnum editorTypeEnum = EditorTypeEnum.护士, FormTypeEnum formTypeEnum = FormTypeEnum.住院)
        {
            _editorTypeEnum = editorTypeEnum;
            _formTypeEnum = formTypeEnum;
        }

        #region pdf
        public bool GetReportImage(out List<Image> imageList, XmlDocument xmlReport, XmlDocument xmlTemplate, Dictionary<string, string> dicPatient)
        {
            _IsLocked = true;
            imageList = null;
            var tempImageList = new List<Image>();
            var ret = base.GetReportImage(xmlReport, xmlTemplate, dicPatient, image =>
               {
                   tempImageList.Add(image);
               });
            if (ret > 0)
            {
                imageList = tempImageList;
                return true;
            }
            return false;
        }
        #endregion

    }
}
