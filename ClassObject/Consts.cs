
using System;
using System.Collections.Generic;
using System.Text;

namespace SparkFormEditor
{
    /// <summary>
    /// 常量类
    /// </summary>
    internal class Consts
    {
        #region Win32 Apis const

        // Constants from the Platform SDK.
        public const int EM_FORMATRANGE = 1081;

        public const int EM_GETCHARFORMAT = WM_USER + 58;
        public const int EM_SETCHARFORMAT = WM_USER + 68;

        public const int EM_SETEVENTMASK = 1073;
        public const int EM_SETTYPOGRAPHYOPTIONS = 1226;
        public const int WM_SETREDRAW = 11;
        public const int TO_ADVANCEDTYPOGRAPHY = 1;


        // Defines for EM_SETCHARFORMAT/EM_GETCHARFORMAT
        public const Int32 SCF_SELECTION = 0x0001;
        public const Int32 SCF_WORD = 0x0002;
        public const Int32 SCF_ALL = 0x0004;

        public const int LF_FACESIZE = 32;

        // Defines for STRUCT_CHARFORMAT member dwMask
        public const UInt32 CFM_BOLD = 0x00000001;
        public const UInt32 CFM_ITALIC = 0x00000002;
        public const UInt32 CFM_UNDERLINE = 0x00000004;
        public const UInt32 CFM_STRIKEOUT = 0x00000008;
        public const UInt32 CFM_PROTECTED = 0x00000010;
        public const UInt32 CFM_LINK = 0x00000020;
        public const UInt32 CFM_SIZE = 0x80000000;
        public const UInt32 CFM_COLOR = 0x40000000;
        public const UInt32 CFM_FACE = 0x20000000;
        public const UInt32 CFM_OFFSET = 0x10000000;
        public const UInt32 CFM_CHARSET = 0x08000000;
        public const UInt32 CFM_SUBSCRIPT = CFE_SUBSCRIPT | CFE_SUPERSCRIPT;
        public const UInt32 CFM_SUPERSCRIPT = CFM_SUBSCRIPT;

        // Defines for STRUCT_CHARFORMAT member dwEffects
        public const UInt32 CFE_BOLD = 0x00000001;
        public const UInt32 CFE_ITALIC = 0x00000002;
        public const UInt32 CFE_UNDERLINE = 0x00000004;
        public const UInt32 CFE_STRIKEOUT = 0x00000008;
        public const UInt32 CFE_PROTECTED = 0x00000010;
        public const UInt32 CFE_LINK = 0x00000020;
        public const UInt32 CFE_AUTOCOLOR = 0x40000000;
        public const UInt32 CFE_SUBSCRIPT = 0x00010000;		/* Superscript and subscript are */
        public const UInt32 CFE_SUPERSCRIPT = 0x00020000;		/*  mutually exclusive			 */

        public const byte CFU_UNDERLINENONE = 0x00;
        public const byte CFU_UNDERLINE = 0x01;
        public const byte CFU_UNDERLINEWORD = 0x02; /* (*) displayed as ordinary underline	*/
        public const byte CFU_UNDERLINEDOUBLE = 0x03; /* (*) displayed as ordinary underline	*/
        public const byte CFU_UNDERLINEDOTTED = 0x04;
        public const byte CFU_UNDERLINEDASH = 0x05;
        public const byte CFU_UNDERLINEDASHDOT = 0x06;
        public const byte CFU_UNDERLINEDASHDOTDOT = 0x07;
        public const byte CFU_UNDERLINEWAVE = 0x08;
        public const byte CFU_UNDERLINETHICK = 0x09;
        public const byte CFU_UNDERLINEHAIRLINE = 0x0A; /* (*) displayed as ordinary underline	*/

        public const int CFM_SMALLCAPS = 0x0040;			/* (*)	*/
        public const int CFM_ALLCAPS = 0x0080;			/* Displayed by 3.0	*/
        public const int CFM_HIDDEN = 0x0100;			/* Hidden by 3.0 */
        public const int CFM_OUTLINE = 0x0200;			/* (*)	*/
        public const int CFM_SHADOW = 0x0400;			/* (*)	*/
        public const int CFM_EMBOSS = 0x0800;			/* (*)	*/
        public const int CFM_IMPRINT = 0x1000;			/* (*)	*/
        public const int CFM_DISABLED = 0x2000;
        public const int CFM_REVISED = 0x4000;

        public const int CFM_BACKCOLOR = 0x04000000;
        public const int CFM_LCID = 0x02000000;
        public const int CFM_UNDERLINETYPE = 0x00800000;		/* Many displayed by 3.0 */
        public const int CFM_WEIGHT = 0x00400000;
        public const int CFM_SPACING = 0x00200000;		/* Displayed by 3.0	*/
        public const int CFM_KERNING = 0x00100000;		/* (*)	*/
        public const int CFM_STYLE = 0x00080000;		/* (*)	*/
        public const int CFM_ANIMATION = 0x00040000;		/* (*)	*/
        public const int CFM_REVAUTHOR = 0x00008000;

        // Font Weights
        public const short FW_DONTCARE = 0;
        public const short FW_THIN = 100;
        public const short FW_EXTRALIGHT = 200;
        public const short FW_LIGHT = 300;
        public const short FW_NORMAL = 400;
        public const short FW_MEDIUM = 500;
        public const short FW_SEMIBOLD = 600;
        public const short FW_BOLD = 700;
        public const short FW_EXTRABOLD = 800;
        public const short FW_HEAVY = 900;

        public const short FW_ULTRALIGHT = FW_EXTRALIGHT;
        public const short FW_REGULAR = FW_NORMAL;
        public const short FW_DEMIBOLD = FW_SEMIBOLD;
        public const short FW_ULTRABOLD = FW_EXTRABOLD;
        public const short FW_BLACK = FW_HEAVY;

        // PARAFORMAT mask values
        public const UInt32 PFM_STARTINDENT = 0x00000001;
        public const UInt32 PFM_RIGHTINDENT = 0x00000002;
        public const UInt32 PFM_OFFSET = 0x00000004;
        public const UInt32 PFM_ALIGNMENT = 0x00000008;
        public const UInt32 PFM_TABSTOPS = 0x00000010;
        public const UInt32 PFM_NUMBERING = 0x00000020;
        public const UInt32 PFM_OFFSETINDENT = 0x80000000;

        // PARAFORMAT numbering options
        public const UInt16 PFN_BULLET = 0x0001;

        // PARAFORMAT alignment options
        public const UInt16 PFA_LEFT = 0x0001;
        public const UInt16 PFA_RIGHT = 0x0002;
        public const UInt16 PFA_CENTER = 0x0003;

        internal const int EM_GETRECT = 0x00b2;
        internal const int EM_SETRECT = 0x00b3;
        internal const int WM_NCHITTEST = 0x0084,
                           WM_NCACTIVATE = 0x0086,
                           WS_EX_TRANSPARENT = 0x00000020,
                           WS_EX_TOOLWINDOW = 0x00000080,
                           WS_EX_LAYERED = 0x00080000,
                           WS_EX_NOACTIVATE = 0x08000000,
                           HTTRANSPARENT = -1,
                           HTLEFT = 10,
                           HTRIGHT = 11,
                           HTTOP = 12,
                           HTTOPLEFT = 13,
                           HTTOPRIGHT = 14,
                           HTBOTTOM = 15,
                           HTBOTTOMLEFT = 16,
                           HTBOTTOMRIGHT = 17,
                           WM_PRINT = 0x0317,
                           WM_REFLECT = WM_USER + 0x1C00,
                           WM_COMMAND = 0x0111,
                           CBN_DROPDOWN = 7,
                           WM_GETMINMAXINFO = 0x0024;
        public const int WM_USER = 0x0400;
        public const int EM_GETPARAFORMAT = WM_USER + 61;
        public const int EM_SETPARAFORMAT = WM_USER + 71;
        public const long MAX_TAB_STOPS = 32;
        public const uint PFM_LINESPACING = 0x00000100;
        public const int IME_CMODE_FULLSHAPE = 0x0008;
        #endregion

        public static float PRINT_FONT_DIFF = 0.28f; //打印和输入框中文字宽度的误差调整 0.28f列很宽，或者调整单元格就会有问题;
        public static int PRINT_CELL_DIFF = 2; //+2防止打印正好,但是光标出来绘制的时候,边界遮挡 
        public static string Msg_Log_Error = "异常类型：{0}\r\n异常消息：{1}\r\n异常信息：{2}\r\n";
        public static string Msg_Confirm_CheckForSave = "[{0}]_[{1}] 已经被修改，是否需要保存？";
        public static string Msg_NoRightSave = "没有编辑权限，不能保存。";

    }
}
