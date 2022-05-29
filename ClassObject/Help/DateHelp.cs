using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SparkFormEditor
{
    ///<summary>
    ///日期、周的算法类
    ///</summary>
    internal class DateHelp
    {
        //定义变量  
        public DateTime currentDateTime;        //当前时间
        public DateTime lastDateTime;           //昨天时间
        public DateTime nextDateTime;           //明天时间

        public int currentDayOfWeek;            //今天星期几

        public DateTime currentStartWeek;       //本周周一
        public DateTime currentEndWeek;         //本周周日
        public DateTime currentStartMonth;      //本月月初
        public DateTime currentEndMonth;        //本月月末
        public DateTime currentStartQuarter;    //本季度初
        public DateTime currentEndQuarter;      //本季度末
        public DateTime currentStartYear;       //今年年初
        public DateTime currentEndYear;         //今年年末

        public DateTime lastStartWeek;          //上周周一
        public DateTime lastEndWeek;            //上周周日
        public DateTime lastStartMonth;         //上月月初
        public DateTime lastEndMonth;           //上月月末
        public DateTime lastStartQuarter;       //上季度初
        public DateTime lastEndQuarter;         //上季度末
        public DateTime lastStartYear;          //去年年初
        public DateTime lastEndYear;            //去年年末

        public DateTime nextStartWeek;          //下周周一
        public DateTime nextEndWeek;            //下周周日
        public DateTime nextStartMonth;         //下月月初
        public DateTime nextEndMonth;           //下月月末
        public DateTime nextStartQuarter;       //下季度初
        public DateTime nextEndQuarter;         //下季度末
        public DateTime nextStartYear;          //明年年初
        public DateTime nextEndYear;            //明年年末

        /// <summary>
        /// 实例化构造方法
        /// </summary>
        public DateHelp() : this(DateTime.Now)
        {

        }

        public DateHelp(DateTime dt)
        {
            //计算变量 
            currentDateTime = DateTime.Now;                 //当前时间
            lastDateTime = DateTime.Now.AddDays(-1);        //昨天时间
            nextDateTime = DateTime.Now.AddDays(1);         //明天时间

            currentDayOfWeek = Convert.ToInt32(currentDateTime.DayOfWeek.ToString("d"));    //今天星期几
            currentDayOfWeek = Convert.ToInt32(lastDateTime.DayOfWeek.ToString("d"));       //昨天星期几
            currentDayOfWeek = Convert.ToInt32(nextDateTime.DayOfWeek.ToString("d"));       //明天星期几

            //currentStartWeek = currentDateTime.AddDays(1 - ((currentDayOfWeek == 0) ? 7 : currentDayOfWeek));   //本周周一
            //currentEndWeek = currentStartWeek.AddDays(6);                                                       //本周周日 
            currentStartWeek = currentDateTime.AddDays(2 - ((currentDayOfWeek == 0) ? 7 : currentDayOfWeek));   //本周周一
            currentEndWeek = currentStartWeek.AddDays(6);                                                       //本周周日    
            lastStartWeek = currentStartWeek.AddDays(-7);       //上周周一
            lastEndWeek = currentEndWeek.AddDays(-7);           //上周周日
            nextStartWeek = currentStartWeek.AddDays(7);        //下周周一
            nextEndWeek = currentEndWeek.AddDays(7);            //下周周日

            currentStartMonth = currentDateTime.AddDays(1 - currentDateTime.Day);   //本月月初
            currentEndMonth = currentStartMonth.AddMonths(1).AddDays(-1);           //本月月末
            lastStartMonth = currentStartMonth.AddMonths(-1);       //上月月初
            lastEndMonth = currentStartMonth.AddDays(-1);           //上月月末
            nextStartMonth = currentEndMonth.AddDays(1);            //下月月初
            nextEndMonth = nextStartMonth.AddMonths(1).AddDays(-1); //下月月末

            currentStartQuarter = currentDateTime.AddMonths(0 - (currentDateTime.Month - 1) % 3).AddDays(1 - currentDateTime.Day);  //本季度初
            currentEndQuarter = currentStartQuarter.AddMonths(3).AddDays(-1);                                                      //本季度末
            lastStartQuarter = currentStartQuarter.AddMonths(-3);           //上季度初
            lastEndQuarter = currentStartQuarter.AddDays(-1);               //上季度末
            nextStartQuarter = currentEndQuarter.AddDays(1);                //下季度初
            nextEndQuarter = nextStartQuarter.AddMonths(3).AddDays(-1);     //下季度末

            //年度运算
            currentStartYear = new DateTime(currentDateTime.Year, 1, 1);  //今年年初
            currentEndYear = new DateTime(currentDateTime.Year, 12, 31);  //今年年末
            lastStartYear = currentStartYear.AddYears(-1);  //去年年初
            lastEndYear = currentEndYear.AddYears(-1);      //去年年末
            nextStartYear = currentStartYear.AddYears(1);   //明年年初
            nextEndYear = currentEndYear.AddYears(1);       //明年年末
        }


        /// <summary>
        /// 日期转换成汉字
        /// </summary>
        /// <param name="datePara"></param>
        /// <returns></returns>
        public static string date2CHN(string datePara)
        {

            string[] c = new string[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            string[] cc = new string[] { "零", "十", "二十", "三十", "四十", "五十", "六十", "七十", "八十", "九十" };
            string input = datePara;
            string retValue = "";

            if (input.Length == 1)
            {
                string temp = input.Substring(0, 1);

                switch (temp)
                {
                    case "1": retValue = c[1]; break;
                    case "2": retValue = c[2]; break;
                    case "3": retValue = c[3]; break;
                    case "4": retValue = c[4]; break;
                    case "5": retValue = c[5]; break;
                    case "6": retValue = c[6]; break;
                    case "7": retValue = c[7]; break;
                    case "8": retValue = c[8]; break;
                    case "9": retValue = c[9]; break;
                    case "0": retValue = c[0]; break;
                    default: retValue = temp; break;
                }
            }
            else
            {
                //两位的时候
                string temp = input.Substring(1, 1);

                switch (temp)
                {
                    case "1": retValue = c[1]; break;
                    case "2": retValue = c[2]; break;
                    case "3": retValue = c[3]; break;
                    case "4": retValue = c[4]; break;
                    case "5": retValue = c[5]; break;
                    case "6": retValue = c[6]; break;
                    case "7": retValue = c[7]; break;
                    case "8": retValue = c[8]; break;
                    case "9": retValue = c[9]; break;
                    case "0": retValue = c[0]; break;
                    default: retValue = temp; break;
                }

                if (retValue == "零")
                {
                    retValue = "";
                }

                temp = input.Substring(0, 1);

                switch (temp)
                {
                    case "1": retValue = cc[1] + retValue; break;
                    case "2": retValue = cc[2] + retValue; break;
                    case "3": retValue = cc[3] + retValue; break;
                    case "4": retValue = cc[4] + retValue; break;
                    case "5": retValue = cc[5] + retValue; break;
                    case "6": retValue = cc[6] + retValue; break;
                    case "7": retValue = cc[7] + retValue; break;
                    case "8": retValue = cc[8] + retValue; break;
                    case "9": retValue = cc[9] + retValue; break;
                    case "0": retValue = cc[0] + retValue; break;
                    default: retValue += temp; break;
                }
            }

            return retValue;

        }


        /// <summary>
        /// 返回星期几字符串函数
        /// </summary>
        /// <param name="myDateTime"></param>
        /// <returns></returns>
        public string GetWeekDayName(DateTime myDateTime)
        {
            string week = "";
            //获取当前日期是星期几
            string dt = myDateTime.DayOfWeek.ToString();
            //根据取得的星期英文单词返回汉字
            switch (dt)
            {
                case "Monday":
                    week = "星期一";
                    break;
                case "Tuesday":
                    week = "星期二";
                    break;
                case "Wednesday":
                    week = "星期三";
                    break;
                case "Thursday":
                    week = "星期四";
                    break;
                case "Friday":
                    week = "星期五";
                    break;
                case "Saturday":
                    week = "星期六";
                    break;
                case "Sunday":
                    week = "星期日";
                    break;
            }
            return week;
        }

        #region 字符日期验证
        /// <summary>   
        /// 是否为日期型字符串   
        /// </summary>   
        /// <param name="strSource">日期字符串(2008-05-08)</param>   
        /// <returns></returns>   
        public static bool IsDate(string strSource)
        {

            //不支持年月日的 return Regex.IsMatch(StrSource, @"((^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(10|12|0?[13578])([-\/\._])(3[01]|[12][0-9]|0?[1-9])$)|(^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(11|0?[469])([-\/\._])(30|[12][0-9]|0?[1-9])$)|(^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(0?2)([-\/\._])(2[0-8]|1[0-9]|0?[1-9])$)|(^([2468][048]00)([-\/\._])(0?2)([-\/\._])(29)$)|(^([3579][26]00)([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][0][48])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][0][48])([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][2468][048])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][2468][048])([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][13579][26])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][13579][26])([-\/\._])(0?2)([-\/\._])(29)$))");

            DateTime dtConvert = DateTime.Now;
            if (DateTime.TryParse(strSource, out dtConvert))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>   
        /// 是否为时间型字符串   
        /// </summary>   
        /// <param name="strSource">时间字符串(15:00)</param>   
        /// <returns></returns>   
        public static bool IsTime(string strSource)
        {
            //return Regex.IsMatch(StrSource, @"^((20|21|22|23|[0-1]?\d):[0-5]?\d:[0-5]?\d)$");  //时间字符串(15:00:00)

            //return Regex.IsMatch(StrSource, @"^((20|21|22|23|[0-1]?\d):[0-5]?\d)$");    //时间字符串(15:00)

            //护理部要求写入24:00
            return Regex.IsMatch(strSource, @"^((20|21|22|23|[0-1]?\d):[0-5]?\d)$") || strSource.Trim() == "24:00";    //时间字符串(15:00)
        }

        /// <summary>   
        /// 是否为日期+时间型字符串   
        /// </summary>   
        /// <param name="strSource"></param>   
        /// <returns></returns>   
        public static bool IsDateTime(string strSource)
        {
            DateTime dt = DateTime.Now;
            if (DateTime.TryParse(strSource, out dt))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion 字符日期验证
    }

}
