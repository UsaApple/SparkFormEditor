using SparkFormEditor.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

namespace SparkFormEditor
{
    /// <summary>
    /// 公共方法类-部分对外开放
    /// </summary>
    public class Comm
    {
        internal static bool _SettingRowForeColor { get; set; }

        internal static bool _isCreatReportImages { get; set; }//判断打开方

        internal static bool _EnterNotShowMenu { get; set; }

        /// <summary>
        /// 调整段落列空白的间隙设置，方式超出
        /// </summary>
        internal static int _AdjustParagraphsDiff = 2;

        /// <summary>
        /// 输入框的隐藏菜单
        /// </summary>
        internal static ArrayList _listInputBoxHideItems = new ArrayList();

        //调整段落列控制
        internal static string _AdjustParagraphsNotLine = "数字@True¤词语@体温§脉搏§出院";         //该属性设置在服务端，用来控制调整段落行的方式，遇到特殊关联词语或者符号，不拆成两行


        internal static double AnInch = 14.4;  //14.4电脑屏幕尺寸  屏幕尺寸和分辨率没有必然的关系。目前是常量，不用配置

        internal static string _Nurse_ExePath { get { return Application.StartupPath; } }
        internal static string _Nurse_ConfigPath { get { return Path.Combine(Application.StartupPath, "Config"); } }
        internal static string _Nurse_TemplatePath { get { return Path.Combine(_Nurse_ConfigPath, "Template"); } }
        internal static string _Nurse_ImagePath { get { return Path.Combine(_Nurse_ConfigPath, nameof(EntXmlModel.Image)); } }

        private static Lib.Foundation.LogManager _logManager = new Lib.Foundation.LogManager()
        {
            LogNode = "NURSE"
        };

        public static Lib.Foundation.LogManager LogHelp
        {
            get
            {
                return _logManager;
            }
        }

        /// <summary>
        /// 日志
        /// </summary>

        #region 消息框
        /// <summary>
        /// 显示错误消息
        /// </summary>
        /// <param name="message">消息内容</param>
        internal static void ShowErrorMessage(string message)
        {
            ShowErrorMessage(message, "错误");
        }

        /// <summary>
        /// 显示错误消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">消息标题，默认为‘错误’</param>
        internal static void ShowErrorMessage(string message, string title)
        {
            MessageBox.Show(message, string.IsNullOrEmpty(title) ? "错误" : title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        /// <summary>
        /// Format格式显示错误消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="args">Format的参数</param>
        internal static void ShowErrorMessage(string message, params object[] args)
        {
            ShowErrorMessage(string.Format(message, args));
        }

        /// <summary>
        /// Format格式显示错误消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">消息标题</param>
        /// <param name="args">Format的参数</param>
        internal static void ShowErrorMessage(string message, string title, params object[] args)
        {
            ShowErrorMessage(string.Format(message, args), title);
        }


        /// <summary>
        /// 显示提示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        internal static void ShowInfoMessage(string message)
        {
            ShowInfoMessage(message, "提示");
        }

        /// <summary>
        /// 显示提示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        internal static void ShowInfoMessage(string message, string title)
        {
            MessageBox.Show(message, string.IsNullOrEmpty(title) ? "提示" : title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Format格式显示提示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="args">Format的参数</param>
        internal static void ShowInfoMessage(string message, params object[] args)
        {
            ShowInfoMessage(string.Format(message, args));
        }

        /// <summary>
        /// Format格式显示提示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title"></param>
        /// <param name="args">Format的参数</param>
        internal static void ShowInfoMessage(string message, string title, params object[] args)
        {
            ShowInfoMessage(string.Format(message, args), title);
        }


        /// <summary>
        /// 显示警告消息
        /// </summary>
        /// <param name="message">消息内容</param>
        internal static void ShowWarningMessage(string message)
        {
            ShowWarningMessage(message, "警告");
        }

        /// <summary>
        /// 显示警告消息
        /// </summary>
        /// <param name="message">消息内容</param>
        internal static void ShowWarningMessage(string message, string title)
        {
            MessageBox.Show(message, string.IsNullOrEmpty(title) ? "警告" : title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


        /// <summary>
        /// Format格式显示提示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="args">Format的参数</param>
        internal static void ShowWarningMessage(string message, params object[] args)
        {
            ShowWarningMessage(string.Format(message, args));
        }

        /// <summary>
        /// Format格式显示提示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="args">Format的参数</param>
        internal static void ShowWarningMessage(string message, string title, params object[] args)
        {
            ShowWarningMessage(string.Format(message, args), title);
        }


        /// <summary>
        /// buttonNum = 3 三个按钮 返回 Yes,No,Cancel
        /// buttonNum = 2 两个按钮 返回 Yes,No
        /// </summary>
        /// <param name="message"></param>
        /// <param name="buttonNum"></param>
        /// <returns></returns>
        internal static DialogResult ShowQuestionMessage(string message, int buttonNum = 3)
        {
            return ShowQuestionMessage(message, "询问", buttonNum);
        }

        /// <summary>
        /// buttonNum = 3 三个按钮 返回 Yes,No,Cancel
        /// buttonNum = 2 两个按钮 返回 Yes,No
        /// </summary>
        /// <param name="message"></param>
        /// <param name="buttonNum"></param>
        /// <returns></returns>
        internal static DialogResult ShowQuestionMessage(string message, string title, int buttonNum = 3)
        {
            MessageBoxButtons btn = MessageBoxButtons.YesNoCancel;
            if (buttonNum == 2) btn = MessageBoxButtons.YesNo;
            return MessageBox.Show(message, string.IsNullOrEmpty(title) ? "询问" : title, btn, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region 其他-编辑器用
        /// <summary>
        ///根据配置文件中的颜色描述的文字，得到程序用的字体
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static Font getFont(string value)
        {
            if (value != "")
            {
                //this.rtb1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                //return new Font(value.Split(',')[0].Trim(), float.Parse(value.Split(',')[1].Trim()), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                return new Font(value.Split(',')[0].Trim(), float.Parse(value.Split(',')[1].Trim()));   //, FontStyle.Regular, GraphicsUnit.World, byte.Parse("134")
            }
            else
            {
                return new Font("宋体", 9F, FontStyle.Regular);
            }
        }

        /// <summary>
        ///根据配置文件中的颜色描述的位置，得到point坐标
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static PointF getPoint(string value)
        {
            if (value != "")
            {
                return new PointF(float.Parse(value.Split(',')[0].Trim()), float.Parse(value.Split(',')[1].Trim()));
            }
            else
            {
                return new PointF(0, 0);
            }
        }

        internal static Point getPointInt(string value)
        {
            if (value != "")
            {
                return new Point(int.Parse(value.Split(',')[0].Trim()), int.Parse(value.Split(',')[1].Trim()));
            }
            else
            {
                return new Point(0, 0);
            }
        }

        /// <summary>
        /// 返回大小size
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static SizeF getSize(string value)
        {
            if (value != "")
            {
                return new SizeF(float.Parse(value.Split(',')[0].Trim()), float.Parse(value.Split(',')[1].Trim()));
            }
            else
            {
                return new SizeF(0, 0);
            }

        }

        /// <summary>
        ///根据索引index取对应属性的int值
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static int getSplitValue(string value, int index)
        {
            try
            {
                return int.Parse(value.Split(',')[index].Trim());
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// 根据数字内容判断是否能进行时间格式化
        /// </summary>
        /// <param name="inuputValue"></param>
        /// <returns></returns>
        internal static string GetNumberToTimeStr(string inuputValue, string dateFormat_hm = "H:mm")
        {
            string ret = inuputValue;

            //【主诉】表格式表单时间栏里面的“冒号”无法去掉
            //【现病史】由于”涉及时间，输入数字自动调整为时间格式”的限制，
            //今若在表单时间列输入文字后，无法删掉冒号（”：“），严重影响日常护理书写。
            //目前医院时间列不一定仅仅只输入时间格式，很多时候表单第二列（时间列）跟第三列进行合并，填写非时间格式的内容。
            //如，死亡的病人，需要合并后书写”告知病危“，但目前程序会自动生成“：”号，删不掉。
            //而此类病人往往是比较严重、紧急的病人，护士就无法及时进行打印，影响非常严重。
            if (!CheckHelp.IsNumber(inuputValue))
            {
                return ret;
            }

            if ((inuputValue.Length == 3 || inuputValue.Length == 4) && !inuputValue.Contains(":"))
            {
                if (inuputValue.Length == 3)
                {
                    //三位的肯定是第一位是时间，后两位是分
                    ret = inuputValue.Insert(1, ":");

                    //根据格式串，判断是否要签名补零
                    if (dateFormat_hm.Contains("HH"))
                    {
                        ret = "0" + ret;
                    }
                }
                else
                {
                    ret = inuputValue.Insert(2, ":");
                }
            }

            return ret;
        }

        #endregion

        #region 判断是否为空行 -编辑器用

        /// <summary>
        /// xml界面数据判断是否为空数据
        /// </summary>
        /// <param name="xn"></param>
        /// <returns></returns>
        internal static bool isEmptyRecord(XmlNode xn)
        {
            bool tetFlg = true;

            int columnCount = xn.Attributes.Count;
            string[] arrEach = null;

            for (int i = 0; i < columnCount; i++)
            {
                //xe.Name == nameof(EntXmlModel.ROW) || xe.Name == nameof(EntXmlModel.Total) 如果有值，说明也是被设置过了，就不是空行了。
                if (xn.Attributes[i].Name != nameof(EntXmlModel.VALIDATE) && xn.Attributes[i].Name != nameof(EntXmlModel.ID) && xn.Attributes[i].Name != nameof(EntXmlModel.Total) && xn.Attributes[i].Name != nameof(EntXmlModel.UserID)) // && xn.Attributes[i].Name != nameof(EntXmlModel.SN)
                {
                    arrEach = xn.Attributes[i].InnerText.Split('¤');
                    //ROW="Black¤None¤Blue" UserID有值表示已经签名，但是都为空也认为空白行进行删除
                    if (xn.Attributes[i].Name == nameof(EntXmlModel.ROW))
                    {
                        if (arrEach[0].Trim() != "Black" || arrEach[1].Trim() != "None")
                        {
                            tetFlg = false;
                            break;
                        }
                    }
                    else if (arrEach[0].Trim() != "")
                    {
                        tetFlg = false;
                        break;
                    }
                    //else if (arrEach.Length > 4 && arrEach[4].Trim() != "")//会导致false没有隐藏的，空行也没有删除掉。点击调整段落列后，下面空行会有这些信息
                    else if (arrEach.Length > 4 && arrEach[4].Trim() != "" && arrEach[4].Trim().ToLower() != "false") //如果隐藏最后一行空白行的上线后，那么也会删除掉
                    {
                        //如果隐藏了边线，那么也认为不是空白行了。即下面的方法： && arrEach[4].Trim().ToLower() == "true"
                        tetFlg = false;
                        break;
                    }
                }

            }

            return tetFlg;
        }

        /// <summary>
        /// 段落的空行（除去日期时间列判断）
        /// </summary>
        /// <param name="xn"></param>
        /// <returns></returns>
        internal static bool isEmptyRecordParagraph(XmlNode xn)
        {
            bool tetFlg = true;

            int columnCount = xn.Attributes.Count;
            string[] arrEach = null;

            for (int i = 0; i < columnCount; i++)
            {
                //xe.Name == nameof(EntXmlModel.ROW) || xe.Name == nameof(EntXmlModel.Total) 如果有值，说明也是被设置过了，就不是空行了。
                if (xn.Attributes[i].Name != "日期" && xn.Attributes[i].Name != "时间" &&
                    xn.Attributes[i].Name != nameof(EntXmlModel.VALIDATE) && xn.Attributes[i].Name != nameof(EntXmlModel.ID) && xn.Attributes[i].Name != nameof(EntXmlModel.Total) && xn.Attributes[i].Name != nameof(EntXmlModel.UserID)) // && xn.Attributes[i].Name != nameof(EntXmlModel.SN)
                {
                    arrEach = xn.Attributes[i].InnerText.Split('¤');
                    //ROW="Black¤None¤Blue" UserID有值表示已经签名，但是都为空也认为空白行进行删除
                    if (xn.Attributes[i].Name == nameof(EntXmlModel.ROW))
                    {
                        if (xn.Attributes[i].InnerText.Split('¤')[0].Trim() != "Black" || xn.Attributes[i].InnerText.Split('¤')[1].Trim() != "None")
                        {
                            tetFlg = false;
                            break;
                        }
                    }
                    else if (arrEach[0].Trim() != "")
                    {
                        tetFlg = false;
                        break;
                    }
                    //else if (arrEach.Length > 4 && arrEach[4].Trim() != "")//会导致false没有隐藏的，空行也没有删除掉。点击调整段落列后，下面空行会有这些信息
                    else if (arrEach.Length > 4 && arrEach[4].Trim() != "" && arrEach[4].Trim().ToLower() != "false") //如果隐藏最后一行空白行的上线后，那么也会删除掉
                    {
                        //如果隐藏了边线，那么也认为不是空白行了。即下面的方法： && arrEach[4].Trim().ToLower() == "true"
                        tetFlg = false;
                        break;
                    }
                    //else if (xn.Attributes[i].InnerText.Split('¤')[0].Trim() != "" || xn.Attributes[i].InnerText.Split('¤')[4].Trim().ToLower() == "true")
                    //{
                    //    tetFlg = false;
                    //    break;
                    //}
                }

            }

            return tetFlg;
        }

        /// <summary>
        /// 签名的空行（除去签名列判断）
        /// </summary>
        /// <param name="xn"></param>
        /// <returns></returns>
        internal static bool isEmptyRecordSign(XmlNode xn)
        {
            bool tetFlg = true;

            int columnCount = xn.Attributes.Count;
            string[] arrEach = null;

            for (int i = 0; i < columnCount; i++)
            {
                //xe.Name == nameof(EntXmlModel.ROW) || xe.Name == nameof(EntXmlModel.Total) 如果有值，说明也是被设置过了，就不是空行了。
                if (xn.Attributes[i].Name != "签名" && xn.Attributes[i].Name != "日期" && xn.Attributes[i].Name != "时间" &&
                    xn.Attributes[i].Name != nameof(EntXmlModel.VALIDATE) && xn.Attributes[i].Name != nameof(EntXmlModel.ID) && xn.Attributes[i].Name != nameof(EntXmlModel.Total) && xn.Attributes[i].Name != nameof(EntXmlModel.UserID)) // && xn.Attributes[i].Name != nameof(EntXmlModel.SN)
                {
                    arrEach = xn.Attributes[i].InnerText.Split('¤');
                    //ROW="Black¤None¤Blue" UserID有值表示已经签名，但是都为空也认为空白行进行删除
                    if (xn.Attributes[i].Name == nameof(EntXmlModel.ROW))
                    {
                        if (xn.Attributes[i].InnerText.Split('¤')[0].Trim() != "Black" || xn.Attributes[i].InnerText.Split('¤')[1].Trim() != "None")
                        {
                            tetFlg = false;
                            break;
                        }
                    }
                    else if (arrEach[0].Trim() != "")
                    {
                        tetFlg = false;
                        break;
                    }
                    //else if (arrEach.Length > 4 && arrEach[4].Trim() != "")//会导致false没有隐藏的，空行也没有删除掉。点击调整段落列后，下面空行会有这些信息
                    else if (arrEach.Length > 4 && arrEach[4].Trim() != "" && arrEach[4].Trim().ToLower() != "false") //如果隐藏最后一行空白行的上线后，那么也会删除掉
                    {
                        //如果隐藏了边线，那么也认为不是空白行了。即下面的方法： && arrEach[4].Trim().ToLower() == "true"
                        tetFlg = false;
                        break;
                    }
                    ////else if (xn.Attributes[i].InnerText.Split('¤')[0].Trim() != "" && xn.Attributes[i].InnerText.Split('¤')[0].Trim().ToLower() == "true")//“查看段落内容”编辑表单，签名会跳转到第一行
                    //else if (xn.Attributes[i].InnerText.Split('¤')[0].Trim() != "" || xn.Attributes[i].InnerText.Split('¤')[4].Trim().ToLower() == "true")
                    //{
                    //    tetFlg = false;
                    //    break;
                    //}
                }

            }

            return tetFlg;
        }

        /// <summary>
        /// 根据字体大小，得到一个合理的差值
        /// 比如：9号字体是0.28-3，字体不一样，差值也不一样的
        /// </summary>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        internal static float GetPrintFontGay(float fontSize) //GetPRINT_FONT_DIFF
        {
            return fontSize / 30 - 0.02f;
        }
        #endregion 判断是否为空行

        #region 实体操作-对外开放
        /// <summary>
        /// 自定义实体转为Dict
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="entList"></param>
        /// <returns></returns>
        public static Dictionary<string, object> EntToDict<T>(T entList) where T : class
        {
            try
            {
                if (entList != null)
                {
                    // 创建对象
                    var dict = new Dictionary<string, object>();
                    // 获取属性
                    PropertyInfo[] allProps = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance); //| BindingFlags.Instance
                                                                                                                    // 添加数据
                    for (int i = 0; i < allProps.Length; i++)
                    {
                        //if (allProps[i].PropertyType.IsClass)
                        //{
                        //    continue;
                        //}
                        var value = allProps[i].GetValue(entList, null);
                        if (allProps[i].PropertyType.IsEnum)
                        {
                            dict.Add(allProps[i].Name, Convert.ToInt32(value));
                        }
                        else
                        {
                            dict.Add(allProps[i].Name, value);
                        }
                    }
                    return dict;
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
            }
            return null;
        }

        public static bool DataRowToEnt<T>(DataRow dr, T ent) where T : class, new()
        {
            try
            {
                if (dr == null) return false;
                ent = ent ?? new T();
                Type type = ent.GetType();
                PropertyInfo[] Properties = type.GetProperties(BindingFlags.Instance /*| BindingFlags.DeclaredOnly*/ | BindingFlags.Public);
                string fieldName = null;
                foreach (PropertyInfo pi in Properties)
                {
                    fieldName = pi.Name;
                    object fieldValue = null;
                    if (!dr.Table.Columns.Contains(fieldName))
                    {
                        fieldName = pi.Name;
                        if (!dr.Table.Columns.Contains(fieldName)) { continue; }
                    }

                    fieldValue = dr[fieldName];
                    SetPropertyValue(ent, pi, fieldValue);
                }
                return true;
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
            }
            return false;
        }

        /// <summary>
        /// 设置反射属性的值
        /// </summary>
        /// <param name="obj">一个实例对象</param>
        /// <param name="pi">反射属性</param>
        /// <param name="value">属性值</param>
        private static void SetPropertyValue(object obj, PropertyInfo pi, object value)
        {
            MethodInfo mi = pi.GetSetMethod();
            if (mi != null)
            {
                object oAry = null;
                if (value != DBNull.Value)
                {
                    if (pi.PropertyType.IsEnum)
                    {
                        pi.SetValue(obj, System.Enum.Parse(pi.PropertyType, value.ToString()), null);
                    }
                    else
                    {
                        switch (pi.PropertyType.FullName)
                        {
                            case "System.String":
                                //如果系统中时间使用字符串处理
                                if (string.Equals(value.GetType().FullName, "System.DateTime"))
                                {
                                    oAry = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                                    break;
                                }

                                oAry = System.Convert.ToString(value);
                                break;
                            case "System.DateTime":
                                oAry = (DateTime)value;
                                break;
                            case "System.Int32":
                                oAry = System.Convert.ToInt32(value);
                                break;
                            case "System.Int64":
                                oAry = System.Convert.ToInt64(value);
                                break;
                            case "System.Int16":
                                oAry = System.Convert.ToInt16(value);
                                break;
                            case "System.Single":
                                oAry = System.Convert.ToSingle(value);
                                break;
                            case "System.Double":
                                oAry = System.Convert.ToDouble(value);
                                break;
                            case "System.Decimal":
                                oAry = System.Convert.ToDecimal(value);
                                break;
                            case "System.Boolean":
                                oAry = System.Convert.ToBoolean(value);
                                break;
                            case "System.Byte[]":
                                oAry = value;
                                break;
                            default:
                                oAry = value;
                                break;

                        };
                        mi.Invoke(obj, new object[] { oAry });
                    }
                }
                else
                {
                    if (pi.PropertyType.IsEnum) { return; }
                    switch (pi.PropertyType.FullName)
                    {
                        case "System.String":
                            oAry = null;
                            break;
                        case "System.Byte[]":
                            oAry = null;
                            break;
                        case "System.Decimal":
                            oAry = Convert.ToDecimal(0);
                            break;
                        case "System.Double":
                            oAry = Convert.ToDouble(0);
                            break;
                        default:
                            if (!pi.PropertyType.IsValueType)
                            {
                                oAry = null;
                            }
                            break;

                    };
                    if (oAry != null)
                    {
                        mi.Invoke(obj, new object[] { oAry });
                    }
                }
            }
        }
        #endregion

        #region 文件操作
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="sFullName"></param>
        /// <returns></returns>
        internal static long GetFileSize(string sFullName)
        {
            long lSize = 0;
            if (File.Exists(sFullName))
                lSize = new FileInfo(sFullName).Length;
            return lSize;
        }
        #endregion

        #region 其他
        /// <summary>
        /// 从一个大字符串中，获取指定项的设定值
        /// </summary>
        /// <param name="RemainTemperatureStylePara"></param>
        /// <param name="thisName"></param>
        /// <returns></returns>
        internal static string GetPropertyByName(string RemainTemperatureStylePara, string thisName)
        {
            string RetValue = "";
            string[] eachArr = RemainTemperatureStylePara.Split('¤');
            string[] eachArrNameValue;

            for (int i = 0; i < eachArr.Length; i++)
            {
                eachArrNameValue = eachArr[i].Split('@');
                if (eachArrNameValue[0] == thisName)
                {
                    RetValue = eachArrNameValue[1];
                    break;
                }
            }

            eachArr = null;
            eachArrNameValue = null;

            return RetValue;
        }
        #endregion

        #region OS操作
        //获取本机的IP
        public static string GetLocalIP()
        {
            #region 多个网卡的话，可能就会获取错了
            string strHostName = Dns.GetHostName(); //得到本机的主机名

            IPHostEntry ipEntry = Dns.GetHostByName(strHostName); //取得本机IP

            string strAddr = ipEntry.AddressList[ipEntry.AddressList.Length - 1].ToString();
            return (strAddr);
            #endregion 多个网卡的话，可能就会获取错了
        }
        #endregion


        public static string GetStackTrace()
        {
            StackTrace trace = new StackTrace();
            if (trace.FrameCount > 0)
            {
                //获取是哪个类来调用的
                return string.Join(Environment.NewLine, trace.GetFrames().Select(a => a.GetMethod().ToString()));
            }
            return "";
        }
    }
}
