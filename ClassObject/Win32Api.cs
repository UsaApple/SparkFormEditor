using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Security;

namespace SparkFormEditor
{
    ///<summary>
    ///Api定义类
    ///</summary>
    internal class Win32Api
    {
        #region 常量
        public const Int32 WM_USER = 0x400;
        public const Int32 EM_FORMATRANGE = WM_USER + 57;
        #endregion
        #region 输入法操作
        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        public static extern bool ImmGetOpenStatus(IntPtr hIMC);

        [DllImport("imm32.dll")]
        public static extern bool ImmGetConversionStatus(IntPtr hIMC, ref int conversion, ref int sentence);

        [DllImport("imm32.dll")]
        public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("imm32.dll")]
        public static extern bool ImmSetConversionStatus(IntPtr hIMC, int conversion, int sentence);
        #endregion

        #region 字间距GDI API
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern int SetTextCharacterExtra(IntPtr hdc, int nCharExtra);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr handle);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

        [DllImport("gdiplus.dll")]
        public static extern uint GdipEmfToWmfBits(IntPtr _hEmf, uint _bufferSize, byte[] _buffer, int _mappingMode, int _flags);

        #endregion

        #region user32
        [DllImport("user32.dll")]
        public static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref Struct.PARAFORMAT2 lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, ref Struct.STRUCT_RECT lParam);


        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int SendMessage(HandleRef hWnd, int msg, int wParam, ref Struct.CHARFORMAT lp);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int AnimateWindow(HandleRef windowHandle, int time, AnimationFlags flags);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        // Use DllImportAttribute to inport the Win32 MessageBox
        // function.  Set the SetLastError flag to true to allow
        // the function to set the Win32 error.
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hwnd, String text, String caption, uint type);

        #endregion

        #region kernel32
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        #endregion

    }
}
