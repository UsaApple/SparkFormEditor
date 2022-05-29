using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparkFormEditor.Model
{
    /// <summary>
    /// XML实体,用来定义模板的元素,使用nameof来获取名称
    /// </summary>
    public class EntXmlModel
    {
        internal static string SN { get; set; }
        internal static string Records { get; set; }
        internal static string Record { get; set; }
        internal static string NurseForm { get; set; }
        internal static string Design { get; set; }
        internal static string Forms { get; set; }
        internal static string Form { get; set; }
        internal static string Lable { get; set; }
        internal static string BindingValue { get; set; }
        internal static string Text { get; set; }
        internal static string LocationBegin { get; set; }
        internal static string LocationEnd { get; set; }
        internal static string MenuHelpString { get; set; }
        internal static string Total { get; set; }
        internal static string Gap { get; set; }
        internal static string DateFormat { get; set; }
        internal static string RankColumnText { get; set; }
        internal static string GroupColumnNum { get; set; }
        internal static string FormRows { get; set; }
        internal static string ID { get; set; }
        internal static string PrintMode { get; set; }
        internal static string FirstRowIndent { get; set; }
        internal static string HelpString { get; set; }
        internal static string TemplateRule { get; set; }
        internal static string UHID { get; set; }
        internal static string NurseFormLevel { get; set; }
        internal static string Name { get; set; }
        internal static string IsSignMessage { get; set; }
        internal static string IsSignIgnoreLevel { get; set; }
        internal static string IsDoubleSign { get; set; }
        internal static string Table { get; set; }
        internal static string FingerPrintSign { get; set; }
        internal static string MaxID { get; set; }
        internal static string OtherTableColumns { get; set; }
        internal static string VALIDATE { get; set; }
        internal static string UserName { get; set; }
        internal static string UserID { get; set; }
        internal static string ROW { get; set; }
        internal static string AutoCreatPagesCount { get; set; }
        internal static string FormName { get; set; }
        internal static string FORM_TYPE { get; set; }
        internal static string ChartType { get; set; }
        internal static string SequenceNo { get; set; }
        internal static string CreatDate { get; set; }
        internal static string UpdateDate { get; set; }
        internal static string CreatUser { get; set; }
        internal static string UpdateUser { get; set; }
        internal static string HISID { get; set; }
        internal static string CreatUserID { get; set; }
        internal static string CreatUserDepartMent { get; set; }
        internal static string CreatDateTime { get; set; }
        internal static string PageNoStart { get; set; }
        internal static string PageEndCrossLine { get; set; }
        internal static string Page { get; set; }
        internal static string Mode { get; set; }
        internal static string FirstPageNo { get; set; }
        internal static string IMAGE { get; set; }
        internal static string Image { get; set; }
        internal static string FingerPrintSignImages { get; set; }
        internal static string Value { get; set; }
        internal static string Default { get; set; }
        /// <summary>
        /// 是否自动加载默认值
        /// </summary>
        internal static string AutoLoadDefault { get; set; }


        internal static string CheckBox { get; set; }
        internal static string Script { get; set; }
        internal static string PagePrintStatus { get; set; }


        internal static string Width { get; set; }
        internal static string Score { get; set; }
        internal static string Color { get; set; }
        internal static string Size { get; set; }

        internal static string ComboBox { get; set; }
        internal static string Items { get; set; }
        internal static string Style { get; set; }
        internal static string ShowValue { get; set; }
        internal static string MaxLength { get; set; }
        internal static string Repeat { get; set; }

        internal static string bold { get; set; }
        internal static string italic { get; set; }

        internal static string BackColor { get; set; }
        internal static string Origin { get; set; }
        internal static string IsSpaceChart { get; set; }
        internal static string SignSaveNeedLogin { get; set; }
        internal static string SaveAutoSign { get; set; }
        internal static string IsSaveAutoSignLastRow { get; set; }
        internal static string MergeUpdate { get; set; }
        internal static string Column { get; set; }

        internal static string Alignment { get; set; }

        internal static string LineType { get; set; }


        internal static string Multiline { get; set; }


        internal static string CheckAlign { get; set; }
        internal static string Attach { get; set; }

        internal static string SortValue { get; set; }

        internal static string Done { get; set; }
        internal static string Type { get; set; }

        internal static string SaveSynchronize { get; set; }

        internal static string TemplateStyleID { get; set; }
        internal static string Event { get; set; }


        internal static string Sort { get; set; }
        internal static string IsTable { get; set; }

        internal static string NurseRoot { get; set; }


        internal static string DefaultTemperatureType { get; set; }
        internal static string SynchronizeTemperatureRecord { get; set; }


        internal static string FirstRowMustDate { get; set; }
        internal static string SaveAutoSort { get; set; }
        internal static string AutoSortMode { get; set; }
        internal static string SumSetting { get; set; }
        internal static string GraphicsUnit { get; set; }
        internal static string Format { get; set; }
        internal static string PageEndCrossLinePen { get; set; }
        internal static string CheckRegex { get; set; }

        internal static string ReadOnly { get; set; }

        internal static string TransparentInputBox { get; set; }
        internal static string ThreeState { get; set; }
        internal static string Vertical { get; set; }

        internal static string RowHeight { get; set; }
        internal static string SumGroup { get; set; }

        internal static string SumGroupExtend { get; set; }

        internal static string RowForeColor { get; set; }
        internal static string Group { get; set; }

        internal static string VectorName { get; set; }
        internal static string Vector { get; set; }
        internal static string PicID { get; set; }
        internal static string PenVector { get; set; }
        internal static string VectorBackGround { get; set; }
        internal static string BorderUserConfig { get; set; }
        internal static string BorderDenoteDiction { get; set; }
        internal static string TabIndex { get; set; }
        internal static string DownInsertRow { get; set; }

        internal static string CommonRtfs { get; set; }
        internal static string CommonRtf { get; set; }
        internal static string GetName(params string[] name)
        {
            if (name.Length > 1)
            {
                return string.Join("/", name);
            }
            else if (name.Length == 1)
            {
                return name[0] ?? "";
            }
            else
            {
                return "";
            }
        }

        internal static string GetNameAt(string id, string value, string sign, params string[] name)
        {
            string str = GetName(name);
            return $"{str}[@{id}{sign}'{value}']";
        }


        #region 新加属性
        /// <summary>
        /// 备注1:列名称1|备注2:列名称2$描述信息|分隔符号
        /// </summary>
        internal static string GroupSum { get; set; }
        /// <summary>
        /// 体温单字段1,体温单字段2|表单字段1,表单字段2
        /// </summary>
        internal static string TcColsMapping { get; set; }
        #endregion
    }
}
