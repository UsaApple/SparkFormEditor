using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using SparkFormEditor.Model;

namespace SparkFormEditor
{
    /// <summary>
    /// 表格处理类：行数、页数计算
    /// </summary>
    internal class TableClass
    {

        /// <summary>
        /// 特殊表格：第一页和第二页以后行数不一样的情况。获取第一页的行数
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public static int GetPageRowsFristPage(XmlDocument xmlDoc)
        {
            int ret = 50;
            XmlNode node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));

            //只有一页模板的，那么每页的行数都一样
            if (xmlDoc.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form))).Count == 1)
            {
                node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));
            }
            else
            {
                node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), "1", "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));
                if (node_FirstPage == null)
                {
                    node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));
                }
            }

            //第一页行数（特例的表格，第一页的行数可能会少几行）
            if (!int.TryParse((node_FirstPage.SelectSingleNode(nameof(EntXmlModel.Table)) as XmlElement).GetAttribute(nameof(EntXmlModel.FormRows)), out ret))
            {
                ret = 50; //第一页的表格行数，默认和第一页一样
            }

            return ret;
        }

        /// <summary>
        /// 获取第二页开始表格行数
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public static int GetPageRowsSecondPage(XmlDocument xmlDoc)
        {
            int ret = 50;
            XmlNode node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));

            if (xmlDoc.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form))).Count == 1)
            {
                node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));
            }
            else
            {
                node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), "1", "!=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form))); //其实只能是第二页，取不是第一页的模板页节点的表格节点来判断
                if (node_FirstPage == null)
                {
                    node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));
                }
            }

            //第一页行数（特例的表格，第一页的行数可能会少几行）
            if (!int.TryParse((node_FirstPage.SelectSingleNode(nameof(EntXmlModel.Table)) as XmlElement).GetAttribute(nameof(EntXmlModel.FormRows)), out ret))
            {
                ret = 50; //第一页的表格行数，默认和第一页一样
            }

            return ret;
        }

        /// <summary>
        /// 根据模板和数据行数，得到页数
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public static int GetPageCount_HaveTable(XmlDocument xmlDoc, int recordCount)
        {
            int pageCount = 1;
            int rowCount = 0;

            pageCount = GetPageCount_HaveTable(xmlDoc, recordCount, ref rowCount);

            return pageCount;
        }

        /// <summary>
        /// 根据模板和记录行数，
        /// 得到页数，并返回行数
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public static int GetPageCount_HaveTable(XmlDocument xmlDoc, int recordCount, ref int rowCount)
        {
            int pageCount = 1;

            int fileds = 1;
            XmlNode xn = xmlDoc.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form), nameof(EntXmlModel.Table)));

            if (!int.TryParse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.GroupColumnNum)), out fileds))
            {
                fileds = 1; // 所有表格的分栏数肯定一直
            }

            int pageRows = GetPageRowsSecondPage(xmlDoc) * fileds;
            int firstPageRows = GetPageRowsFristPage(xmlDoc) * fileds;

            if (pageRows == firstPageRows)
            {
                pageCount = recordCount / pageRows;

                if (recordCount % pageRows > 0)
                {
                    pageCount++;

                    rowCount = recordCount % pageRows;
                }
                else
                {
                    rowCount = pageRows; //如果整除，那说明在最后一行
                }
            }
            else
            {
                if (recordCount > firstPageRows)
                {
                    pageCount = (recordCount + pageRows - firstPageRows) / pageRows;

                    if ((recordCount + pageRows - firstPageRows) % pageRows > 0)
                    {
                        pageCount++;   //a = count / a + (count % a > 0 ? 1 : 0);

                        rowCount = (recordCount + pageRows - firstPageRows) % pageRows;
                    }
                    else
                    {
                        rowCount = pageRows; //如果整除，那说明在最后一行
                    }
                }
                else
                {
                    //小于等于 第一页的行数，那么肯定是第一页
                    pageCount = 1;

                    rowCount = recordCount;
                }
            }

            return pageCount;
        }

    }
}
