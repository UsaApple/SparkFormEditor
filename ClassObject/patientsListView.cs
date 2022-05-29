using SparkFormEditor.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace SparkFormEditor
{
    ///<summary>
    ///取得病人列表数据，以及病人列表Dt列的初始化
    ///</summary>
    internal class patientsListView
    {
        /// <summary>
        /// 初始化列
        /// </summary>
        /// <returns></returns>
        public static DataTable GetInitDT(EntPatientInfoEx infoEx)
        {
            DataTable dt = infoEx.PatientOtherInfo.Table.Copy();
            dt.Columns.Add("床号");
            dt.Columns.Add("姓名");
            dt.Columns.Add("性别");
            dt.Columns.Add("年龄");
            dt.Columns.Add("科别");
            dt.Columns.Add("住院号");
            dt.Columns.Add("病区");
            dt.Columns.Add("入区日期");
            dt.Columns.Add("入院日期");
            //dt.Columns.Add("护理等级");
            dt.Columns.Add("表单名");   //表单模板名
            dt.Columns.Add("总页数");   //表单总页数，初始化列表的时候不用先取得所有病人的大字段后再对xml计算得到页数，性能很低
            dt.Columns.Add("表单类型"); //0：医生，1：护士，2：共享 。 默认作为共享吧
            dt.Columns.Add("DONE");     //是否归档

            ////补上：责任护士，主治医师，费别（新农合/城镇居民/现金），病情，入院诊断（诊断赞不处理）
            //dt.Columns.Add("责任护士");
            //dt.Columns.Add("主治医师");
            //dt.Columns.Add("费别");
            //dt.Columns.Add("病情");
            //dt.Columns.Add("入院门诊诊断");
            //dt.Columns.Add("入院时间");
            //dt.Columns.Add("心电监护");

            dt.Columns.Add("HISID");  

            return dt;
        }

        /// <summary>
        /// 获取指定项的最新的基本信息，比如：床号
        /// </summary>
        /// <param name="patientsLisDT"></param>
        /// <param name="thispatientUHID"></param>
        /// <param name="basicName"></param>
        /// <returns></returns>
        public static string GetPatientsInforFromName(DataTable patientsLisDT, string thispatientUHID, string basicName)
        {
            string retValue = "";

            if (patientsLisDT != null)
            {
                foreach (DataRow drChild in patientsLisDT.Rows)
                {
                    if (drChild["住院号"].ToString() == thispatientUHID)
                    {
                        if (patientsLisDT.Columns.Contains(basicName))
                        {
                            if (drChild[basicName] != null)
                            {
                                retValue = drChild[basicName].ToString();
                            }
                        }

                        break;
                    }
                }
            }

            return retValue;
        }

    }
}
