using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;

namespace SparkFormEditor
{
    /// <summary>
    /// 备份功能类
    /// 本地数据备份，每次保存时，本地做一个临时备份
    /// </summary>
    internal class BackUpHelp
    {
        public static int canDays = 7; //历史文件保留天数
        static string folderPath = Comm._Nurse_ExePath + @"\BackUp";

        /// <summary>
        /// 添加一个本地备份
        /// 表单在保存和删除页的时候要调用
        /// </summary>
        /// <param name="xmlData"></param>
        /// <param name="name"></param>
        /// <param name="patientId"></param>
        /// <param name="templateName"></param>
        public static void SaveBack(XmlDocument xmlData, string name, string patientId, string templateName)
        {
            //病人id-病人姓名-模板名称-日期
            string filePath = Path.Combine(folderPath, string.Format("{0}-{1}-{2}-{3}.xml", patientId, name, templateName, string.Format("{0:yyyy-MM-dd HH&mm&ss&ffff}", DateTime.Now)));

            //遍历文件夹，删除超过期限的日志文件
            DirectoryInfo dir = new DirectoryInfo(folderPath);

            //如果不存在目录，那么创建
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            try
            {
                //保存到本地：
                xmlData.Save(filePath);
            }
            catch (Exception ex)
            {
                //保存失败
                Comm.LogHelp.WriteErr("本地备份时遇到异常：" + patientId + templateName + "\r\n" + xmlData.InnerXml);
                Comm.LogHelp.WriteErr(ex);
            }
        }

        /// <summary>
        /// 删除本地过期的文件
        /// 根据名字日期是否过期来判断
        /// </summary>
        public static void DelOldBackUp()
        {
            try
            {


                DateTime dtNow = DateTime.Now;
                TimeSpan ts;

                //遍历文件夹，删除超过期限的日志文件
                DirectoryInfo dir = new DirectoryInfo(folderPath);

                //如果不存在目录，那么创建
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileDate = "";
                string[] arr;
                foreach (FileInfo dChild in dir.GetFiles("*.xml")) //删除超时的日志文件
                {
                    //2018-07-24 01&54&07&6063
                    arr = dChild.Name.Split('-');
                    fileDate = string.Concat(arr[arr.Length -3], "-", arr[arr.Length - 2], "-", arr[arr.Length - 1]);
                    fileDate = fileDate.Substring(0, 10);
                    //arr[arr.Length - 1].Replace(@".xml", "").Split(' ')[0] + " " + string.Format("{0:HH:mm}", dtNow);

                    try
                    {
                        if (DateTime.TryParse(fileDate, out DateTime fileDt))
                        {
                            ts = dtNow.Subtract(fileDt);
                            if (ts.TotalDays > canDays)
                            {
                                dChild.Delete();
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                fileDate = null;
                arr = null;
            }
            catch (Exception)
            {
              
            }
        }

    }


}
