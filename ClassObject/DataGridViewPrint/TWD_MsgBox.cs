/* 新表单
 * *************************************************************
 TRT 2013年6月28日 -2013年8月8日
 * 
 * 
 * datagridview打印类 - 消息
 * 
 *  
 * *************************************************************
*/
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NurseForm.Editor.Ctrl
{
    /// <summary>
    /// Simple class to simplify access to MessageBoxes of different kinds
    /// WARNING: By no means complete!!!
    /// </summary>
    internal static class MsgBox
    {
        //---------------------------------------------------------------------
        /// <summary>
        /// Displays a Confirm type message box
        /// </summary>
        /// <param name="sMsg">The message you want to display</param>
        public static bool Confirm(string sMsg)
        {
            return Confirm("Confirm", sMsg);
        }
        public static bool Confirm(string sTitle, string sMsg)
        {
            DialogResult ret = MessageBox.Show(sMsg, sTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return (ret == DialogResult.Yes);
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Displays a Error type message box
        /// </summary>
        /// <param name="sMsg">The message you want to display</param>
        public static void Error(string sMsg)
        {
            Error("Error", sMsg);
        }
        public static void Error(string sTitle, string sMsg)
        {
            MessageBox.Show(sMsg, sTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        //---------------------------------------------------------------------
        /// <summary>
        /// Displays a Warning type message box
        /// </summary>
        /// <param name="sMsg">The message you want to display</param>
        public static void Warning(string sMsg)
        {
            Warning("", sMsg);
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Displays a Warning type message box
        /// </summary>
        /// <param name="sCaption">Name of Application or Class or Method</param>
        /// <param name="sMsg">The message you want to display</param>
        public static void Warning(string sCaption, string sMsg)
        {
            if (sCaption.Length == 0)
                sCaption = "Warning";
            MessageBox.Show(sMsg, sCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


        //---------------------------------------------------------------------
        /// <summary>
        /// Displays a Information type message box
        /// </summary>
        /// <param name="sMsg">The message you want to display</param>
        public static void Info(string sMsg)
        {
            Info("", sMsg);
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Displays a Information type message box
        /// </summary>
        /// <param name="sCaption">Name of Application or Class or Method</param>
        /// <param name="sMsg">The message you want to display</param>
        public static void Info(string sCaption, string sMsg)
        {
            if (sCaption.Length == 0)
                sCaption = "Information";
            MessageBox.Show(sMsg, sCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
