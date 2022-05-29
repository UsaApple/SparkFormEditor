using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SparkFormEditor
{
    /// <summary>
    /// 验证帮助类
    /// </summary>
    class CheckHelp
    {
        /// <summary>
        /// 用户验证
        /// </summary>
        /// <param name="user"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool CheckCurrentLoginUser(string user, SparkFormEditor parent)
        {
            bool ret = false;

            Frm_SignLogin slf = new Frm_SignLogin(user, false, parent);
            DialogResult res = slf.ShowDialog();

            if (res == DialogResult.OK)
            {
                ret = true;
            }
            else
            {
                ret = false;
            }

            return ret;
        }

        public static bool ChecCurrentLoginUser2(string user, string tipInfo, SparkFormEditor parent)
        {
            bool ret = false;

            var slf = new Frm_SignLoginEx(user, false, parent, tipInfo);
            DialogResult res = slf.ShowDialog();

            if (res == DialogResult.OK)
            {
                ret = true;
            }
            else
            {
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// 是否为数字
        /// </summary>
        /// <param name="strNumber"></param>
        /// <returns></returns>
        public static bool IsNumber(string strNumber)
        {
            Regex regex = new Regex("[^0-9]");
            return !regex.IsMatch(strNumber); //非数字，再取反，就一定是数字了，但是012也认为正确
        }
    }
}
