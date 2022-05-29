using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Xml;
using System.Collections;
using SparkFormEditor.Model;

namespace SparkFormEditor
{
    ///<summary>
    ///页码控制算法
    ///</summary>
    class PageNo
    {

        /// <summary>
        /// 根据当前数据，以及本次修改的物理页码和修改后的现实页码。
        /// 得到保存的内容，一遍重新打开可以计算。要防止重复修改导致数据重叠的错误。
        /// </summary>
        /// <param name="xmlData"></param>
        /// <param name="currentPage"></param>
        /// <param name="editPage"></param>
        /// <returns></returns>
        public static void SaveValue(XmlDocument xmlData, string currentPage, string editPage)
        {
            string retValue = "";
            XmlNode root = xmlData.SelectSingleNode(nameof(EntXmlModel.NurseForm));
            string nowValue = (root as XmlElement).GetAttribute(nameof(EntXmlModel.PageNoStart)); // 1@5,10@26…… 物理实际页码@修改后显示的页码内容

            if (CheckHelp.IsNumber(nowValue) || string.IsNullOrEmpty(nowValue))
            {
                //如果是数字，还是老的程序写入的老数据，根据这个数字作为基础，来整体调整显示的页码
                //直接覆盖原来修改的老数据就行了
                retValue = currentPage + "@" + editPage; 
            }
            else
            {
                string[] arr = nowValue.Split(',');
                bool isFind = false;

                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i].Split('@')[0] == currentPage)
                    {
                        //物理页码已经修改过，继续覆盖修改就行了
                        arr[i] = currentPage + "@" + editPage;
                        isFind = true;
                        break;
                    }
                }

                if (isFind)
                {
                    //已经找到，遍历整个数组在拼接
                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (i == 0)
                        {
                            sb.Append(arr[i]);
                        }
                        else
                        {
                            sb.Append("," + arr[i]);
                        }
                    }

                    retValue = sb.ToString();
                }
                else
                {
                    //没有找到直接，评价在最后
                    if (nowValue == "")
                    {
                        retValue = currentPage + "@" + editPage;
                    }
                    else
                    {
                        retValue = nowValue + "," + currentPage + "@" + editPage;
                    }
                }
            }

            (root as XmlElement).SetAttribute(nameof(EntXmlModel.PageNoStart), retValue);

            //return retValue;
        }

        public static string GetValue(string pageNoSetting, int currentPage)
        {
            string retValue = "";
            string[] arr = pageNoSetting.Split(',');
            string[] arrEach;

            ArrayList list = new ArrayList();
            pageClass pc;

            for (int i = 0; i < arr.Length; i++)
            {
                arrEach = arr[i].Split('@');

                pc = new pageClass();
                pc.CurrentPage = int.Parse(arrEach[0]);
                pc.ShowPageEdit = int.Parse(arrEach[1]);

                list.Add(pc);
            }

            //排序
            list.Sort(new PageClassComparer());

            //遍历判断，计算。从后往前遍历，找到第一个满足的，作为判断基准
            pc = new pageClass();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (currentPage >= ((pageClass)list[i]).CurrentPage)
                {
                    pc = (pageClass)list[i];

                    break; //必须跳出，找到最接近当前页码的一个作为基数来计算的。
                }
            }

            retValue = (pc.ShowPageEdit + currentPage - pc.CurrentPage).ToString(); //默认值的CurrentPage和ShowPageEdit要一样，不然如果上面循环找不到，抵消后还是原来的实际页码。

            return retValue;
        }

        /// <summary>
        /// 根据页码格式串判断，是否当前页显示在最左。
        /// 要根据这个来解析文字串中的当前页码（手动修改页码的时候）
        /// </summary>
        /// <param name="formatPara"></param>
        /// <returns></returns>
        public static bool IsCurrentPageLeft(string formatPara)
        {
            //<Variable ID="00025" Name="_PageNo" Format="{0}/{1}"
            //现在还有一个问题，就是删除页后，没有判断是否手动修改过该页页码，会残留。但是一般不会操作的。
            bool ret = true;

            if (formatPara.Contains("{1}") && formatPara.Contains("{0}"))
            {
                if (formatPara.IndexOf("{0}") > formatPara.IndexOf("{1}"))
                {
                    ret = false;
                }
            }

            return ret;
        }

        //如果手动修改最后一页，或者修改的页码是目前存储中最大的。那么都会影响总页数。
        public static bool IsMaxSetPage(string pageNoSetting, string currentPagePara)
        {
            bool ret = true;

            if (string.IsNullOrEmpty(pageNoSetting))
            {
                //如果第一次设置页码，而且不是最值当前的最后一页，执行到本方法，这里不处理的话，下面将空转int就会异常的。
                ret = true;
                return ret;
            }

            string[] arr = pageNoSetting.Split(',');
            string[] arrEach;

            for (int i = 0; i < arr.Length; i++)
            {
                arrEach = arr[i].Split('@');

                if (int.Parse(arrEach[0]) > int.Parse(currentPagePara))
                {
                    ret = false; //只要还有一个比现在的大，那么就不会影响总页数
                    break;
                }
            }

            return ret;
        }

        class pageClass
        {
            public int CurrentPage = 1;     //物理顺序的页码
            public int ShowPageEdit = 1;    //需要显示的页码

        }

        //排序类
        class PageClassComparer : IComparer
        {
            int IComparer.Compare(Object x, Object y)
            {
                //return  int.Compare(((pageClass)x).CurrentPage, ((pageClass)x).ShowPageEdit);
                //return ((new CaseInsensitiveComparer()).Compare(y, x));

                return ((pageClass)x).CurrentPage.CompareTo(((pageClass)y).CurrentPage);

            }

        }
    }

}
