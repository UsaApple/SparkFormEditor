using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Collections;
using System.Security.Cryptography;

namespace SparkFormEditor
{
    /// <summary>
    /// SUM合计界面，进行分类合计
    /// 比如：出入量，一次合计，得到两个分类的合计结果，提高工作效率
    /// </summary>
    class SumMulti
    {
        public string Memo = "";//备注信息
        public string SumName = ""; //合计结果填入列的名字
        public double Sum = 0;      //合计结果
        public ArrayList SumItems = new ArrayList();  //需要合计的列s，列的集合
    }

    /// <summary>
    /// 加密类
    /// </summary>
    class PassWordClass
    {

        /// <summary>
        /// 加密过的密码
        /// WFSP -> 1
        /// </summary>
        /// <param name="PrimaryString"></param>
        /// <returns></returns>
        public static string UHIDDecoded(string PrimaryString)
        {
            if (PrimaryString.Trim() == "")
                return "";

            string FinalString = "";
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] SeniorBytes = utf8.GetBytes(PrimaryString);
            byte[] JuniorBytes = new byte[] { };
            byte[] StratoBytes = new byte[] { };
            bool Found = false;
            int Count = 0;
            int EncryptInteger = 0;
            int EncryptTail = 0;
            int Jesus = 0;

            if (SeniorBytes.Length == 0)
                return "";

            EncryptInteger = (int)SeniorBytes[0];
            EncryptTail = (int)SeniorBytes[SeniorBytes.Length - 1];
            JuniorBytes = UHIDMatrix(EncryptInteger, SeniorBytes.Length);
            StratoBytes = new byte[SeniorBytes.Length];

            for (int i = 1; i < SeniorBytes.Length - 2; i++)
            {
                Found = false;

                for (int j = 0; j < JuniorBytes.Length; j++)
                {
                    Count = (int)JuniorBytes[j] * 2 + j + 1;

                    if (i == Count)
                    {
                        Found = true;
                        break;
                    }
                }

                if (Found)
                    continue;

                Jesus = ((int)SeniorBytes[i] - 65) * 26 + ((int)SeniorBytes[i + 1] - 65);
                Jesus = Jesus - EncryptTail - 19;
                StratoBytes[i] = (byte)(Jesus);
                i++;
            }

            Count = 0;

            for (int i = 0; i < StratoBytes.Length; i++)
            {
                if (StratoBytes[i] == 0)
                    Count++;
            }

            if (Count != 0)
            {
                JuniorBytes = new byte[StratoBytes.Length - Count];
                Count = 0;

                for (int i = 0; i < StratoBytes.Length; i++)
                {
                    if (StratoBytes[i] == 0)
                        Count++;
                    else
                        JuniorBytes[i - Count] = StratoBytes[i];
                }
            }

            FinalString = utf8.GetString(JuniorBytes);

            return FinalString;
        }

        /// <summary>
        /// 加密算法
        /// 
        /// 
        /// </summary>
        /// <param name="PrimaryInteger"></param>
        /// <param name="Capacity"></param>
        /// <returns></returns>
        private static byte[] UHIDMatrix(int PrimaryInteger, int Capacity)
        {
            byte[] FinalBytes = new byte[] { };
            byte[] MinorBytes = new byte[] { };
            int CapacityLength = Capacity / 13;
            int Count = 0;
            int Number = 0;

            switch (PrimaryInteger)
            {
                case 65:
                case 74:
                case 79:
                case 82:
                    MinorBytes = new byte[] { 1 };
                    break;
                case 67:
                case 73:
                case 77:
                case 84:
                    MinorBytes = new byte[] { 3, 6 };
                    break;
                case 68:
                case 72:
                case 76:
                case 85:
                    MinorBytes = new byte[] { 2, 4, 9 };
                    break;
                case 66:
                case 69:
                case 81:
                case 83:
                    MinorBytes = new byte[] { 1, 2, 6, 12 };
                    break;
                case 71:
                case 78:
                case 86:
                case 88:
                    MinorBytes = new byte[] { 1, 3, 5, 7, 9 };
                    break;
                case 70:
                case 75:
                case 80:
                case 89:
                    MinorBytes = new byte[] { 3, 5, 7, 8, 9, 10 };
                    break;
                case 87:
                case 90:
                    MinorBytes = new byte[] { 1, 3, 5, 8, 10, 11, 12 };
                    break;
            }

            if (CapacityLength == 0)
                return MinorBytes;

            FinalBytes = new byte[MinorBytes.Length * (CapacityLength + 1)];

            for (int i = 0; i < FinalBytes.Length; i++)
            {
                Count = i / MinorBytes.Length;
                Number = (i + MinorBytes.Length) % MinorBytes.Length;
                FinalBytes[i] = (byte)(Count * 13 + (int)MinorBytes[Number]);
            }

            return FinalBytes;
        }

        /// <summary>
        /// MD5
        /// </summary>
        /// <param name="myString"></param>
        /// <returns></returns>
        public static string GetMd5Str(string myString)
        {
            //MD5CryptoServiceProvider是MD5的实现类，可计算输入数据的MD5哈希值
            MD5 md5 = new MD5CryptoServiceProvider();
            //获取需加密的字符串对用的字符数组
            byte[] fromData = System.Text.Encoding.Unicode.GetBytes(myString);
            //ComputeHash方法计算指定字节数组的哈希值
            byte[] toData = md5.ComputeHash(fromData);
            //定义用于接收结果的字符串变量，并初始化
            string byteStr = null;
            //获取MD5哈希字符串
            for (int i = 0; i < toData.Length; i++)
            {
                //格式化成十六进制输出，每个占2位，保证byteStr的长度为32
                byteStr += toData[i].ToString("x2");
            }
            //返回结果
            return byteStr;
        }

    }

    /// <summary>
    /// 获取Dll目录
    /// </summary>
    class DllClass
    {
        /// <summary>得到DLL的路径
        /// 得到DLL的路径 DllPath(this.GetType());
        /// </summary>
        /// <param name="t">dll中的一个类</param>
        /// <returns>DLL的路径</returns>
        public static string DllPath(Type t)
        {
            string ret = System.IO.Path.GetDirectoryName(new Uri(t.Assembly.CodeBase).LocalPath);
            if (ret[ret.Length - 1] != '\\')
            {
                ret += '\\';
            }
            return ret;
        }
    }

    /// <summary>
    /// Dgv隔行变色类
    /// </summary>
    internal class DgvSet
    {

        public static void setRowColor(DataGridView dgv)
        {
            //隔行变色
            dgv.RowsDefaultCellStyle.BackColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue;

        }
    }

    //打印窗口最大化
    internal class PrintPreviewDialogEx
    {
        public static void MakePrintPreviewDialogMaximized(PrintPreviewDialog ppd)
        {
            System.Reflection.PropertyInfo[] pis = ppd.GetType().GetProperties();
            for (int i = 0; i < pis.Length; i++)
            {
                if (pis[i].Name == "WindowState")
                {
                    pis[i].SetValue(ppd, FormWindowState.Maximized, null);
                    break;
                }
            }
        }
    }

    //获取表单的参数
    internal class DataServiceSetting
    {
        //---------------------------------服务器端DataService.cfg 参数定义---------------------------------
        //”位置|线数|时间段”;位置：上边框(Top)、下边框(Bottom)、上下两边框(Both)、上下左右四边框(round)；
        //线数：单线(Single)、双线(Double);
        //时间段：时间段及颜色 “开始时间点，结束时间点，颜色” 默认全时间段为红色。
        //默认值：”上边框双红线”
        //IntensiveCare.DoubleRedLine="Round|Double|19:00,07:00,Blue" 

        //调整段落列的个性化要求：数字是不是不能分开行，固定词语是不是不能分开行的设定
        public string AdjustParagraphsStyle = "数字@True¤词语@体温§脉搏§出院";

        //默认false,如果这个属性设置为True,启用显示转科标记。即：显示"A→B"。
        //如果已有表单中的最后一页为空白内容，不体现A→B，直接显示为B科室，只有对于有内容的表单才体现A→B的显示方式。程序就是这么设计的。
        public bool IsOpenTransferSign = false;

        //每次打开表单要不要更新对应的基本信息；
        //IntensiveCare.AlwaysRefreshGlobalInformation="床号,True|病区,False|入院主要诊断,True|科别,True"
        //诊断,AllPage ---> true:最后一页更新，AllPage:所有页更新。这样来解决老表单更新开启时，之前页都更新的罪恶问题
        public string IntensiveCareAlwaysRefreshGlobalInformation = "";

        //"IntensiveCare.SignedReadOnly" 默认false,如果这个属性设置为True,则护理表单在签名后变为只读状态。
        public bool IntensiveCareSignedReadOnly = false;

        //病人列表中节点：床号，姓名……排序
        public string PatientInforOrder = "";

        //RecordLockInterval属性值（刷新周期：定时更新；锁定时，如果超过这个时间，也认为无效）：打开或保存(新建)操作时执行timer事件
        public int RecordLockInterval = 60;

        //工具栏，行线的默认类型指定
        public string DefaultRowLine = "BottomLine";

        //工具栏，行线的自定义设置：颜色，宽度
        public string RowLineType = "Red,1";

        //除掉种类的后缀符号
        public string ImprotTemperatureTrim = "";

        //表单自定义复制的类型：ctrl shift两种，可以两种并存
        public string CopyDefine = "";

        //单机版：不进行压缩
        public bool Recruit3ZipEnable = false;  //表单数据是否压缩：1.解决Oracle中特殊字符®等乱码；2.减少数据容量

        //public bool Recruit3FingerPrintSign = false; //其否启用表单手摸签名

        public string DateFormat_HM = "HH:mm"; //hh:mm 12小时，HH24小时制，一个H是不会补零的

        //红色文字，由于黑白打印机，所以改为：黑色斜体的字体，进行打印
        public bool PrintRedInsteadOfItalic = false;

        public int AdjustParagraphsDiff = 2; //调整段落列空白的间隙设置，方式超出

        //InputBoxHideItems="复制,重做"
        public string InputBoxHideItems = "";  //输入框中需要隐藏的菜单项目

        //本地表单xml备份文件历史保留天数，过期的会登录就删除
        public int LocalBackSaveDays = 7;

        public bool NurseDepartmentRight = false;  //his转科后，现在刷新病人列表后，就立刻能修改。

        //CA签名相关属性
        public int Recruit3CAImage = 0;                         //0:只显示文字（和双击签名一样），1：文字+图片标记， 2：只要图片
        public Size Recruit3CAImageSize = new Size(10, 10);     //签名图片的大小

        public bool Recruit3CACoverText = false;                //如果表格里面的签名有名字，点击CA签名的时候判断CA和这个签名的工号是否一致，不一致不让签名，一致的话，覆盖掉，只显示图片。

        public bool UpdateAllPagesBaseInfor = false;            //打开某页的时候，原来只更新本页的基本信息，用来控制是否同时刷新所有页         

        public string EditHistoryMode = "";                     //默认为空，表示不管等级，都可以修订。 1：上级对下级才能修订；2：上级、平级之间都能修订。

        public bool OverturnSignature = false;                  //表示医生三级签名时，签名的显示顺序。默认false：高/低

        public string SelectRowBackColor = "";                  //表格选择行、当前行的背景色突出显示 Gold，要么为空不绘制，要么为金色比较合适

        public string Recruit3PatientSign = "";                 //病人签名、模式硬件厂商、显示方式

        //输入的时间不得晚于规定时间。
        public int InputDateTimeValidate = 525600;              //不能录入晚于系统时间60分钟以后的数据。默认是525600，即一年之内的数据。

        public bool DefaultDateEnterOrDoubleClick = true;       //默认是true在光标进入的时候赋值日期，有些表单和护士，会误点击，所以false表示双击生成默认值

        public bool PrintCheck = false;       //打印是否和保存一样，做验证。如果存在不正确格式的，就不让打印。

        public string SumRowAttention = "";               //"∑,Red,9" 用来标识合计行，打印的时候不出现

         
        //TODO:指定的一些科室，就有不在本科室病人的编辑权限
        //比如服务端根节点属性值设置:RecruitEspecialRight="A科室@药物过敏试验记录单,手术交接单¤B科室@手术安全核查表" ：表示A科室拥有此类医疗文书的书写权限，B科室拥有其他医疗文书写权限。
        public string RecruitEspecialRight = null;

        /// <summary>
        /// 根据单机版，正式版，得到服务端配置信息DataService.cfg
        /// </summary>
        /// <param name="RunMode">True正式，False单击版</param>
        public DataServiceSetting(bool RunMode)
        {

            //单机版时，使用方法
            string xmlPath = "";
            //if (File.Exists(xmlPath))
            //{
            //    XmlDocument xml = new XmlDocument();
            //    xml.Load(xmlPath);
            //    XmlNode node_DataService = xml.SelectSingleNode("SERVICE_CONFIG");

            //    string tempStr = "";

            //    tempStr = (node_DataService as XmlElement).GetAttribute("AdjustParagraphsStyle");
            //    if (!string.IsNullOrEmpty(tempStr))
            //    {
            //        AdjustParagraphsStyle = tempStr;
            //    }
            //    else
            //    {
            //        AdjustParagraphsStyle = "";
            //    }

            //    //转科，签床历史标记
            //    if (!bool.TryParse((node_DataService as XmlElement).GetAttribute("IsOpenTransferSign"), out IsOpenTransferSign))
            //    {
            //        IsOpenTransferSign = false;
            //    }

            //    //基本信息刷新控制
            //    tempStr = (node_DataService as XmlElement).GetAttribute("IntensiveCare.AlwaysRefreshGlobalInformation");
            //    if (!string.IsNullOrEmpty(tempStr))
            //    {
            //        IntensiveCareAlwaysRefreshGlobalInformation = tempStr;
            //    }
            //    else
            //    {
            //        IntensiveCareAlwaysRefreshGlobalInformation = "";
            //    }

            //    //签名后只读
            //    if (!bool.TryParse((node_DataService as XmlElement).GetAttribute("IntensiveCare.SignedReadOnly"), out IntensiveCareSignedReadOnly))
            //    {
            //        IntensiveCareSignedReadOnly = false;
            //    }

            //    tempStr = (node_DataService as XmlElement).GetAttribute("PatientInforOrder");
            //    if (!string.IsNullOrEmpty(tempStr))
            //    {
            //        PatientInforOrder = tempStr;
            //    }
            //    else
            //    {
            //        PatientInforOrder = "床号,姓名,性别,年龄";
            //    }

            //    //锁定 定时器
            //    if (!int.TryParse((node_DataService as XmlElement).GetAttribute("RecordLockInterval"), out RecordLockInterval))
            //    {
            //        RecordLockInterval = 60;
            //    }

            //    //默认行线：DefaultRowLine = "BottomLine";
            //    tempStr = (node_DataService as XmlElement).GetAttribute("DefaultRowLine");
            //    if (!string.IsNullOrEmpty(tempStr))
            //    {
            //        DefaultRowLine = tempStr;
            //    }
            //    else
            //    {
            //        DefaultRowLine = "BottomLine";
            //    }

            //    tempStr = (node_DataService as XmlElement).GetAttribute("RowLineType");
            //    if (!string.IsNullOrEmpty(tempStr))
            //    {
            //        RowLineType = tempStr;
            //    }
            //    else
            //    {
            //        RowLineType = "Red,1";
            //    }

            //    tempStr = (node_DataService as XmlElement).GetAttribute("ImprotTemperatureTrim");
            //    if (!string.IsNullOrEmpty(tempStr))
            //    {
            //        ImprotTemperatureTrim = tempStr;
            //    }
            //    else
            //    {
            //        ImprotTemperatureTrim = "";
            //    }

            //    //自定义复制的模式 CopyDefine="Ctrl,Shift"
            //    tempStr = (node_DataService as XmlElement).GetAttribute("CopyDefine");
            //    if (!string.IsNullOrEmpty(tempStr))
            //    {
            //        CopyDefine = tempStr;
            //    }
            //    else
            //    {
            //        CopyDefine = "";
            //    }

            //    //压缩，但是单机版不能压缩，否则读取xml报错。
            //    if (!bool.TryParse((node_DataService as XmlElement).GetAttribute("Recruit3ZipEnable"), out Recruit3ZipEnable))
            //    {
            //        Recruit3ZipEnable = false;
            //    }

            //    //时间，要不要补零显示
            //    tempStr = (node_DataService as XmlElement).GetAttribute("DateFormat_HM");
            //    if (!string.IsNullOrEmpty(tempStr))
            //    {
            //        DateFormat_HM = tempStr;
            //    }
            //    else
            //    {
            //        DateFormat_HM = "H:mm";
            //    }

            //    if (!bool.TryParse((node_DataService as XmlElement).GetAttribute("PrintRedInsteadOfItalic"), out PrintRedInsteadOfItalic))
            //    {
            //        PrintRedInsteadOfItalic = false;
            //    }

            //    //调整段落列行长的间隙
            //    if (!int.TryParse((node_DataService as XmlElement).GetAttribute("AdjustParagraphsDiff"), out AdjustParagraphsDiff))
            //    {
            //        AdjustParagraphsDiff = 2;
            //    }

            //    InputBoxHideItems = (node_DataService as XmlElement).GetAttribute("InputBoxHideItems");

            //    //备份文件保留天数
            //    if (!int.TryParse((node_DataService as XmlElement).GetAttribute("Recruit3LocalBackSaveDays"), out LocalBackSaveDays))
            //    {
            //        LocalBackSaveDays = 7;
            //    }

            //    //护理权限
            //    if (!bool.TryParse((node_DataService as XmlElement).GetAttribute("NurseDepartmentRight"), out NurseDepartmentRight))
            //    {
            //        NurseDepartmentRight = false;
            //    }

            //    //CA签名 样式类型，图片大小
            //    if (!int.TryParse((node_DataService as XmlElement).GetAttribute("Recruit3CAImage"), out Recruit3CAImage))
            //    {
            //        Recruit3CAImage = 0;
            //    }

            //    if (!string.IsNullOrEmpty((node_DataService as XmlElement).GetAttribute("Recruit3CAImageSize")))
            //    {
            //        Recruit3CAImageSize = Comm.getSize((node_DataService as XmlElement).GetAttribute("Recruit3CAImageSize")).ToSize();
            //    }

            //    if (!bool.TryParse((node_DataService as XmlElement).GetAttribute("UpdateAllPagesBaseInfor"), out UpdateAllPagesBaseInfor))
            //    {
            //        UpdateAllPagesBaseInfor = false;
            //    }

            //    EditHistoryMode = (node_DataService as XmlElement).GetAttribute("EditHistoryMode");

            //    if (!bool.TryParse((node_DataService as XmlElement).GetAttribute("OverturnSignature"), out OverturnSignature))
            //    {
            //        OverturnSignature = false;
            //    }

            //    //CA签名覆盖自己的名字
            //    if (!bool.TryParse((node_DataService as XmlElement).GetAttribute("Recruit3CACoverText"), out Recruit3CACoverText))
            //    {
            //        Recruit3CACoverText = false;
            //    }

            //    SelectRowBackColor = (node_DataService as XmlElement).GetAttribute("SelectRowBackColor");

            //    Recruit3PatientSign = (node_DataService as XmlElement).GetAttribute("Recruit3PatientSign");

            //    if (!int.TryParse((node_DataService as XmlElement).GetAttribute("InputDateTimeValidate"), out InputDateTimeValidate))
            //    {
            //        InputDateTimeValidate = 525600;
            //    }

            //    if (!bool.TryParse((node_DataService as XmlElement).GetAttribute("DefaultDateEnterOrDoubleClick"), out DefaultDateEnterOrDoubleClick))
            //    {
            //        DefaultDateEnterOrDoubleClick = true;
            //    }

            //    //打印验证
            //    if (!bool.TryParse((node_DataService as XmlElement).GetAttribute("PrintCheck"), out PrintCheck))
            //    {
            //        PrintCheck = false;
            //    }

            //    SumRowAttention = (node_DataService as XmlElement).GetAttribute("SumRowAttention");

            //    RecruitEspecialRight = (node_DataService as XmlElement).GetAttribute("RecruitEspecialRight");
            //}

        }
    }

    /// <summary> 
    /// RichTextBox插入图片使用的静态类 
    /// </summary> 
    internal class RTB_InsertImg
    {
        public const int
        EmfToWmfBitsFlagsDefault = 0x00000000,
        EmfToWmfBitsFlagsEmbedEmf = 0x00000001,
        EmfToWmfBitsFlagsIncludePlaceable = 0x00000002,
        EmfToWmfBitsFlagsNoXORClip = 0x00000004;


        private struct RtfFontFamilyDef
        {
            public const string Unknown = @"\fnil";
            public const string Roman = @"\froman";
            public const string Swiss = @"\fswiss";
            public const string Modern = @"\fmodern";
            public const string Script = @"\fscript";
            public const string Decor = @"\fdecor";
            public const string Technical = @"\ftech";
            public const string BiDirect = @"\fbidi";
        }

        private const int MM_ISOTROPIC = 7;
        private const int MM_ANISOTROPIC = 8;
        private const int HMM_PER_INCH = 2540;
        private const int TWIPS_PER_INCH = 1440;

        private const string FF_UNKNOWN = "UNKNOWN";

        private const string RTF_HEADER = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033";
        private const string RTF_DOCUMENT_PRE = @"\viewkind4\uc1\pard\cf1\f0\fs20";
        private const string RTF_DOCUMENT_POST = @"\cf0\fs17}";
        private const string RTF_IMAGE_POST = @"}";

        private HybridDictionary rtfFontFamily;

        public RTB_InsertImg()
        {
            //rtfFontFamily = new HybridDictionary();
            //rtfFontFamily.Add(FontFamily.GenericMonospace.Name, RtfFontFamilyDef.Modern);
            //rtfFontFamily.Add(FontFamily.GenericSansSerif.Name, RtfFontFamilyDef.Swiss);
            //rtfFontFamily.Add(FontFamily.GenericSerif.Name, RtfFontFamilyDef.Roman);
            //rtfFontFamily.Add(FF_UNKNOWN, RtfFontFamilyDef.Unknown);

   
            rtfFontFamily = new HybridDictionary();
            rtfFontFamily.Add(FontFamily.GenericMonospace.Name, RtfFontFamilyDef.Modern);

            if (!rtfFontFamily.Contains(FontFamily.GenericSansSerif.Name))
            {
                rtfFontFamily.Add(FontFamily.GenericSansSerif.Name, RtfFontFamilyDef.Swiss);
            }

            if (!rtfFontFamily.Contains(FontFamily.GenericSerif.Name))
            {
                rtfFontFamily.Add(FontFamily.GenericSerif.Name, RtfFontFamilyDef.Roman);
            }

            if (!rtfFontFamily.Contains(FF_UNKNOWN))
            {
                rtfFontFamily.Add(FF_UNKNOWN, RtfFontFamilyDef.Unknown);
            }
        }


        //public RTB_InsertImg()
        //{

        //}

        private string GetFontTable(Font _font)
        {
            StringBuilder _fontTable = new StringBuilder();
            _fontTable.Append(@"{\fonttbl{\f0");
            _fontTable.Append(@"\");
            if (rtfFontFamily.Contains(_font.FontFamily.Name))
                _fontTable.Append(rtfFontFamily[_font.FontFamily.Name]);
            else
                _fontTable.Append(rtfFontFamily[FF_UNKNOWN]);
            _fontTable.Append(@"\fcharset0 ");
            _fontTable.Append(_font.Name);
            _fontTable.Append(@";}}");
            return _fontTable.ToString();
        }

        /// 
        /// 在RichTextBox当前光标处插入一副图像。 
        /// 
        /// 多格式文本框控件 
        /// 插入的图像 
        public void InsertImage(RichTextBox rtb, Image image)
        {
            StringBuilder _rtf = new StringBuilder();
            _rtf.Append(RTF_HEADER);
            _rtf.Append(GetFontTable(rtb.Font));
            _rtf.Append(GetImagePrefix(rtb, image));
            _rtf.Append(GetRtfImage(rtb, image));
            _rtf.Append(RTF_IMAGE_POST);
            rtb.SelectedRtf = _rtf.ToString();
        }

        private string GetImagePrefix(RichTextBox rtb, Image _image)
        {
            float xDpi;
            float yDpi;
            using (Graphics _graphics = rtb.CreateGraphics())
            {
                xDpi = _graphics.DpiX;
                yDpi = _graphics.DpiY;
            }

            StringBuilder _rtf = new StringBuilder();
            int picw = (int)Math.Round((_image.Width / xDpi) * HMM_PER_INCH);
            int pich = (int)Math.Round((_image.Height / yDpi) * HMM_PER_INCH);
            int picwgoal = (int)Math.Round((_image.Width / xDpi) * TWIPS_PER_INCH);
            int pichgoal = (int)Math.Round((_image.Height / yDpi) * TWIPS_PER_INCH);
            _rtf.Append(@"{\pict\wmetafile8");
            _rtf.Append(@"\picw");
            _rtf.Append(picw);
            _rtf.Append(@"\pich");
            _rtf.Append(pich);
            _rtf.Append(@"\picwgoal");
            _rtf.Append(picwgoal);
            _rtf.Append(@"\pichgoal");
            _rtf.Append(pichgoal);
            _rtf.Append(" ");
            return _rtf.ToString();
        }

        private string GetRtfImage(RichTextBox rtb, Image _image)
        {
            StringBuilder _rtf = null;
            MemoryStream _stream = null;
            Graphics _graphics = null;
            Metafile _metaFile = null;
            IntPtr _hdc;
            try
            {
                _rtf = new StringBuilder();
                _stream = new MemoryStream();
                using (_graphics = rtb.CreateGraphics())
                {
                    _hdc = _graphics.GetHdc();
                    _metaFile = new Metafile(_stream, _hdc);
                    _graphics.ReleaseHdc(_hdc);
                }
                using (_graphics = Graphics.FromImage(_metaFile))
                {
                    _graphics.DrawImage(_image, new Rectangle(0, 0, _image.Width, _image.Height));
                }
                IntPtr _hEmf = _metaFile.GetHenhmetafile();
                uint _bufferSize = Win32Api.GdipEmfToWmfBits(_hEmf, 0, null, MM_ANISOTROPIC, EmfToWmfBitsFlagsDefault);
                byte[] _buffer = new byte[_bufferSize];
                uint _convertedSize = Win32Api.GdipEmfToWmfBits(_hEmf, _bufferSize, _buffer, MM_ANISOTROPIC, EmfToWmfBitsFlagsDefault);
                for (int i = 0; i < _buffer.Length; ++i)
                {
                    _rtf.Append(String.Format("{0:X2}", _buffer[i]));
                }

                return _rtf.ToString();
            }
            finally
            {
                if (_graphics != null)
                    _graphics.Dispose();
                if (_metaFile != null)
                    _metaFile.Dispose();
                if (_stream != null)
                    _stream.Close();
            }
        }
    }
}
