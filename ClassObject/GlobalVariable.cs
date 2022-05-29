using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using SparkFormEditor.Model;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using SparkFormEditor;
using SparkFormEditor.Foundation;
using System.Diagnostics;

namespace SparkFormEditor
{
    public class GlobalVariable
    {
        #region 非public
        private static DataTable _patientInfoList = null;
        private static bool _InputAutocomplete = false;
        private static bool _InputAutoAdjustParagraphs = true;
        private static bool _NeedMdiChildSaveSuccessMsg = true;
        private static ToolStripMenuItem _ToolStripMenuItemPageEndCrossLine = null;

        /// <summary>
        /// 病人信息列表，可用于扩展批量录入功能
        /// </summary>
        internal static DataTable PatientInfoList
        {
            set { _patientInfoList = value; }
            get { return _patientInfoList; }
        }

        /// <summary>
        /// 智能输入匹配
        /// </summary>
        internal static bool InputAutocomplete
        {
            set { _InputAutocomplete = value; }
            get { return _InputAutocomplete; }
        }

        /// <summary>
        /// 菜单选择的默认双红线颜色
        /// </summary>
        internal static Color _RowLineType_Color_Selected = Color.Red;

        private static bool _PrintEffect = false;
        /// <summary>
        /// 打印样式，勾选框是自己绘制还是调用控件本身的绘制方法。默认False自己绘制
        /// </summary>
        internal static bool PrintEffect
        {
            set { _PrintEffect = value; }
            get { return _PrintEffect; }
        }

        /// <summary>
        /// 回调主窗体方法，创建该表单
        /// </summary>
        internal static EventHandler _CallBackCreatBook = null;

        /// <summary>
        /// 能创建的文书集合，权限判断。本来是初始化在菜单中的。
        /// </summary>
        internal static ArrayList _ListCreatDoc = new ArrayList();

        /// <summary>
        /// 自动调整段罗列
        /// </summary>
        internal static bool InputAutoAdjustParagraphs
        {
            set { _InputAutoAdjustParagraphs = value; }
            get { return _InputAutoAdjustParagraphs; }
        }

        /// <summary>
        /// 输入助理
        /// </summary>
        internal static XmlDocument XmlDocAssistant
        {
            get; set;
        }

        /// <summary>
        /// 判断此用户是否有权限编辑表单
        /// </summary>
        /// <param name="userDivision"></param>
        /// <returns></returns>
        internal static bool IsHaveUserDivisionConfine(string userDivision)
        {
            return true;
        }

        /// <summary>
        /// 判断表单是否归档
        /// </summary>
        internal static string Done { get; set; }

        /// <summary>
        /// 表单的权限
        /// </summary>
        internal static string Type { get; set; }

        /// <summary>
        /// 护理表单内容，外部传入
        /// </summary>
        internal static string NurseFormValue { get; set; }

        /// <summary>
        /// 关闭主窗体时的保存，不需要提示保存成功
        /// </summary>
        internal static bool NeedMdiChildSaveSuccessMsg
        {
            set { _NeedMdiChildSaveSuccessMsg = value; }
            get { return _NeedMdiChildSaveSuccessMsg; }
        }

        /// <summary>
        /// 判断是否是医生
        /// </summary>
        internal static bool IsDoctor
        {
            get { return LoginUserInfo.Type == "医生"; }
        }

        /// <summary>
        /// 页尾斜线
        /// </summary>
        internal static ToolStripMenuItem ToolStripMenuItemPageEndCrossLine
        {
            set { _ToolStripMenuItemPageEndCrossLine = value; }
            get { return _ToolStripMenuItemPageEndCrossLine; }
        }


        internal static float ScaleTransWidth { get; set; } = 1;
        internal static float ScaleTransHeight { get; set; } = 1;
        #endregion

        #region public
        /// <summary>
        /// 纸张的大小，单位英寸的100倍，和模板设置的大小比较
        /// </summary>
        public static Size PrintPageSize = Size.Empty;
        /// <summary>
        /// 根据打印纸张的名称切换纸张，默认A4
        /// </summary>

        public static string PrintPageSizeSwitchByName = string.Empty;

        /// <summary>
        /// 登录用户信息
        /// </summary>
        public static EntUserInfoEx LoginUserInfo { get; set; }

        /// <summary>
        /// 登陆系统用户，默认或者选择后的对象病区（多病病区可以切换的）
        /// LoginUserInfo中的病区为默认病区
        /// </summary>
        public static string UserDivision
        {
            get; set;
        }

        //默认打印机名称
        public static string DefaultPrintName
        {
            get; set;
        }


        #endregion

        #region 事件
        /// <summary>
        /// 保存事件委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        [Obsolete("请使用BeforeSaveEventHandler和AfterSaveEventHandler事件")]
        public delegate bool SaveEventHandler(object sender, SaveEventArgs e);

        /// <summary>
        /// 内部保存之前的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void BeforeSaveEventHandler(object sender, SaveCancelEventArgs e);

        /// <summary>
        /// 内部保存完成之后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate bool AfterSaveEventHandler(object sender, SaveEventArgs e);

        //Before After TreeViewCancelEventArgs CancelEventArgs 
        /// <summary>
        /// 打印页之前的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate void BeforePrintPageEventHandler(object sender, PrintPageCancelEventArgs e);

        /// <summary>
        /// 打印页完成之后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void AfterPrintPageEventHandler(object sender, PrintPageEventArgs e);

        /// <summary>
        /// 新建页之前的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate void BeforeNewPageEventHandler(object sender, NewPageCancelEventArg e);

        /// <summary>
        /// 新建页完成之后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void AfterNewPageEventHandler(object sender, NewPageEventArg e);

        /// <summary>
        /// ，
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate void BeforeDelPageEventHandler(object sender, DelPageCancelEventArg e);

        /// <summary>
        /// 删除文书完成之后的事件，成功返回True，否则返回False
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void AfterDelPageEventHandler(object sender, DelPageEventArg e);


        /// <summary>
        /// 由外部删除数据库此病人的表单数据，成功返回True，否则返回False
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate bool DelFormEventHandler(object sender, DelFormEventArg e);

        /// <summary>
        /// 插入行之前的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate void BeforeInsertRowEventHandler(object sender, InsertRowCancelEventArg e);

        /// <summary>
        /// 插入行完成之后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void AfterInsertRowEventHandler(object sender, InsertRowEventArg e);

        /// <summary>
        /// 删除行之前的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate void BeforeDelRowEventHandler(object sender, DelRowCancelEventArg e);

        /// <summary>
        /// 删除行完成之后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void AfterDelRowEventHandler(object sender, DelRowEventArg e);

        /// <summary>
        /// 新建表单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void NewFormEventHandler(object sender, NewFormEventArg e);

        /// <summary>
        /// 获取服务器时间
        /// </summary>
        /// <returns></returns>
        public delegate DateTime GetDateTime();

        /// <summary>
        /// 双签名用户密码验证事件，成功返回用户信息，失败返回null
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public delegate EntUserInfoEx LoginVerifyHandler(string userid, string password);

        public delegate void ApplyNurseFormDelockHandler(EntPatientInfoEx patientInfo);

        /// <summary>
        /// 插入体温单数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate bool InsertTcDataEventHandler(object sender, InsertTcDataEventArgs e);
        #endregion

        #region 事件参数
        public class InsertTcDataEventArgs : EventArgs
        {
            public InsertTcDataEventArgs(string patientID, string formId)
            {
                PatientID = patientID;
                FormId = formId;
            }

            public DataRow Data { get; set; }

            public string PatientID { get; }

            public string FormId { get; }
        }


        /// <summary>
        /// 保存事件的返回参数
        /// </summary>
        public class SaveEventArgs : EventArgs
        {
            public SaveEventArgs(string value, string tempName, string tempId, EntPatientInfoEx patientInfo)
            {
                this.Value = value;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
            }
            /// <summary>
            /// 保存的内容
            /// </summary>
            public string Value { get; private set; }
            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }
        }

        /// <summary>
        /// 保存之前事件的返回参数
        /// </summary>
        public class SaveCancelEventArgs : CancelEventArgs
        {
            public SaveCancelEventArgs(string value, string tempName, string tempId, EntPatientInfoEx patientInfo)
            {
                this.Value = value;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
            }
            /// <summary>
            /// 保存的内容
            /// </summary>
            public string Value { get; private set; }
            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }
        }

        /// <summary>
        /// 打印页事件参数
        /// </summary>
        public class PrintPageEventArgs
        {
            public PrintPageEventArgs(int pageIndex, int pageCount, string pageStates, string tempName, string tempId, EntPatientInfoEx patientInfo, PrintPageType type)
            {
                this.PageIndex = pageIndex;
                this.PageCount = pageCount;
                this.PagePrintStatus = pageStates;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
                this.Type = type;
            }

            /// <summary>
            /// 打印方式
            /// </summary>
            public PrintPageType Type { get; private set; }

            /// <summary>
            /// 打印状态，¤分割，表示哪些页打印过了
            /// </summary>
            public string PagePrintStatus { get; private set; }

            /// <summary>
            /// 总共多少页
            /// </summary>
            public int PageCount { get; private set; }

            /// <summary>
            /// 打印的页码
            /// </summary>
            public int PageIndex { get; private set; }
            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }
        }

        /// <summary>
        /// 打印页之前事件参数
        /// </summary>
        public class PrintPageCancelEventArgs : CancelEventArgs
        {
            public PrintPageCancelEventArgs(int pageIndex, int pageCount, string pageStates, string tempName, string tempId, EntPatientInfoEx patientInfo, PrintPageType type)
            {
                this.PageIndex = pageIndex;
                this.PageCount = pageCount;
                this.PagePrintStatus = pageStates;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
                this.Type = type;
            }

            /// <summary>
            /// 打印方式
            /// </summary>
            public PrintPageType Type { get; private set; }

            /// <summary>
            /// 打印状态，¤分割，表示哪些页打印过了
            /// </summary>
            public string PagePrintStatus { get; private set; }

            /// <summary>
            /// 总共多少页
            /// </summary>
            public int PageCount { get; private set; }

            /// <summary>
            /// 打印的页码
            /// </summary>
            public int PageIndex { get; private set; }
            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }
        }

        /// <summary>
        /// 删除事件参数
        /// </summary>
        public class DelPageEventArg
        {
            public DelPageEventArg(int pageIndex, string tempName, string tempId, EntPatientInfoEx patientInfo)
            {
                this.PageIndex = pageIndex;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
            }

            /// <summary>
            /// 删除的页码
            /// </summary>
            public int PageIndex { get; private set; }

            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }

            /// <summary>
            /// 删除页时，如果是最后一页，则需要删除菜单
            /// </summary>
            public bool IsDelMenu { get; set; }
        }

        /// <summary>
        /// 删除页之前的事件参数
        /// </summary>
        public class DelPageCancelEventArg : CancelEventArgs
        {
            public DelPageCancelEventArg(int pageIndex, string tempName, string tempId, EntPatientInfoEx patientInfo)
            {
                this.PageIndex = pageIndex;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
            }

            /// <summary>
            /// 删除的页码
            /// </summary>
            public int PageIndex { get; private set; }

            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }
        }


        /// <summary>
        /// 删除全部表单事件参数
        /// </summary>
        public class DelFormEventArg
        {
            public DelFormEventArg(string tempName, string tempId, EntPatientInfoEx patientInfo)
            {
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
            }

            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }
        }

        /// <summary>
        /// 插入行事件参数
        /// </summary>
        public class InsertRowEventArg
        {
            public InsertRowEventArg(int rowIndex, string tempName, string tempId, EntPatientInfoEx patientInfo, InsertRowType type)
            {
                this.RowIndex = rowIndex;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
                this.Type = type;
            }
            /// <summary>
            /// 插入方式
            /// </summary>
            public InsertRowType Type { get; private set; }

            /// <summary>
            /// 插入的行号
            /// </summary>
            public int RowIndex { get; private set; }

            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }

        }

        /// <summary>
        /// 插入行之前的事件参数
        /// </summary>
        public class InsertRowCancelEventArg : CancelEventArgs
        {
            public InsertRowCancelEventArg(int rowIndex, string tempName, string tempId, EntPatientInfoEx patientInfo, InsertRowType type)
            {
                this.RowIndex = rowIndex;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
                this.Type = type;
            }
            /// <summary>
            /// 插入方式
            /// </summary>
            public InsertRowType Type { get; private set; }

            /// <summary>
            /// 插入的行号
            /// </summary>
            public int RowIndex { get; private set; }

            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }

        }

        /// <summary>
        /// 删除行事件参数
        /// </summary>
        public class DelRowEventArg
        {
            public DelRowEventArg(int rowIndex, string tempName, string tempId, EntPatientInfoEx patientInfo)
            {
                this.RowIndex = rowIndex;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
            }
            /// <summary>
            /// 插入的行号
            /// </summary>
            public int RowIndex { get; private set; }

            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }
        }

        /// <summary>
        /// 删除行之前的事件参数
        /// </summary>
        public class DelRowCancelEventArg : CancelEventArgs
        {
            public DelRowCancelEventArg(int rowIndex, string tempName, string tempId, EntPatientInfoEx patientInfo)
            {
                this.RowIndex = rowIndex;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
            }
            /// <summary>
            /// 插入的行号
            /// </summary>
            public int RowIndex { get; private set; }

            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }
        }


        /// <summary>
        /// 新建表单事件参数
        /// </summary>
        public class NewFormEventArg
        {
            public NewFormEventArg(string tempNameOld, string tempIdOld, string tempNameNew, string tempIdNew, EntPatientInfoEx patientInfo)
            {
                this.TemplateNameOld = tempNameOld;
                this.TemplateIdOld = tempIdOld;
                this.TemplateNameNew = tempNameNew;
                this.TemplateIdNew = tempIdNew;
                this.PatientInfo = patientInfo;
            }

            /// <summary>
            /// 老模板名称
            /// </summary>
            public string TemplateNameOld { get; private set; }
            /// <summary>
            /// 老模板ID
            /// </summary>
            public string TemplateIdOld { get; private set; }
            /// <summary>
            /// 新模板名称
            /// </summary>
            public string TemplateNameNew { get; private set; }
            /// <summary>
            /// 新模板ID
            /// </summary>
            public string TemplateIdNew { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }
        }

        /// <summary>
        /// 新建页参数
        /// </summary>
        public class NewPageEventArg
        {
            public NewPageEventArg(int pageIndex, string tempName, string tempId, EntPatientInfoEx patientInfo)
            {
                this.PageIndex = pageIndex;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
            }
            /// <summary>
            /// 插入的行号
            /// </summary>
            public int PageIndex { get; private set; }

            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }
        }

        /// <summary>
        /// 删除行之前的事件参数
        /// </summary>
        public class NewPageCancelEventArg : CancelEventArgs
        {
            public NewPageCancelEventArg(int pageIndex, string tempName, string tempId, EntPatientInfoEx patientInfo)
            {
                this.PageIndex = pageIndex;
                this.TemplateName = tempName;
                this.TemplateId = tempId;
                this.PatientInfo = patientInfo;
            }
            /// <summary>
            /// 插入的行号
            /// </summary>
            public int PageIndex { get; private set; }

            /// <summary>
            /// 模板名称
            /// </summary>
            public string TemplateName { get; private set; }
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TemplateId { get; private set; }
            /// <summary>
            /// 病人信息
            /// </summary>
            public EntPatientInfoEx PatientInfo { get; private set; }
        }


        #endregion

        #region 表单列表
        /// <summary>
        /// 表单列表
        /// </summary>
        public class Template
        {
            public Template(string id, string name)
            {
                this.ID = id;
                this.Name = name;
            }

            /// <summary>
            /// 模板ID，如果有子项，id可以不赋值
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// 模板名称，如果有子项则表示分组名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 分组名称
            /// </summary>
            public string Memo { get; set; }

            /// <summary>
            /// 子项
            /// </summary>
            public List<Template> Child { get; } = new List<Template>();
        }
        #endregion

        #region 矢量图接口
        /// <summary>
        /// 矢量图接口对象
        /// </summary>
        internal static IVector Vector { get; set; }
        #endregion



    }
}
