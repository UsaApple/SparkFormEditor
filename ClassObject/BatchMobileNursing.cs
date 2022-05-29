using SparkFormEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SparkFormEditor.MyClass
{
    /// <summary>
    /// 批量操作表单数据
    /// </summary>
    internal class BatchMobileNursing
    {
        /// <summary>
        /// 新建表单
        /// </summary>
        /// <returns></returns>
        public XmlDocument CreateTemplate(string patientId, string patientName, string userID, string userDepartMent, XmlDocument templateXml, string templateName)
        {
            TemplateHelp mx = new TemplateHelp();
            return mx.CreateTemplate(patientId, patientName, userID, userDepartMent, templateXml, templateName);
        }

        #region 批量接口页数计算

        public int GetPageTotal(XmlDocument xdc, string pagecount)
        {
            try
            {
                XmlNodeList xnl = xdc.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records), nameof(EntXmlModel.Record))); //"NurseForm/Records/Record"
                XmlNodeList xnlPages = xdc.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form))); //EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form))

                //null不行的，如果没有Record节点，并不会为null，而是xnl.Count为0:
                if ((xnl == null || xnl.Count == 0) && (xnlPages == null || xnlPages.Count == 0))
                {
                    //log.Error("没有读到Record或者Page子节点");
                    return -1; //至少有Record或者Page，当然也可能两者都有
                }
                else
                {
                    //表格样式：有限判断表格，因为有些表单是表格和非表格共存都有的
                    int a = 0;
                    if (xnl != null && xnl.Count != 0)
                    {
                        if (!int.TryParse(pagecount, out a) || a == 0) //每页总行数
                        {
                            //log.Error("没有传入每页的总行数，应该是非表格样式。");
                            a = 0;
                        }
                        else
                        {
                            //传入每页总行数的时候
                            int count = xnl.Count;

                            a = count / a + (count % a > 0 ? 1 : 0);
                        }
                    }

                    //勾选样式部分，可能和表格共存，所以去最大值，但是不可能都没有
                    int b = 0;
                    if (xnlPages != null && xnlPages.Count != 0)
                    {
                        b = xnlPages.Count;
                    }
                    else
                    {
                        b = 0;
                    }

                    if (a == 0 && b == 0)
                    {
                        //log.Error("没有读到Record或者Page子节点");
                        return -1;
                    }

                    return a > b ? a : b; //至少有一个不为0的时候，只大的那个值
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //log.Error("getPageTotal_Error", ex);
                //return -1;
            }
        }

        /// <summary>
        /// 支持第一页和第二页以后表格行数不一样的表格
        /// </summary>
        /// <param name="xdc"></param>
        /// <param name="xmlTemplate"></param>
        /// <returns></returns>
        public int GetPageTotalNew(XmlNode xdc, XmlDocument xmlTemplate)
        {
            try
            {
                XmlNodeList xnl = xdc.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.Records), nameof(EntXmlModel.Record)));// "Records/Record"
                XmlNodeList xnlPages = xdc.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form))); // "Forms/Form"

                //null不行的，如果没有Record节点，并不会为null，而是xnl.Count为0:
                if ((xnl == null || xnl.Count == 0) && (xnlPages == null || xnlPages.Count == 0))
                {
                    //log.Error("没有读到Record或者Page子节点");
                    return -1; //至少有Record或者Page，当然也可能两者都有
                }
                else
                {
                    //表格样式：有限判断表格，因为有些表单是表格和非表格共存都有的
                    int a = 0;
                    if (xnl != null && xnl.Count != 0)
                    {
                        a = GetPageCount_HaveTable(xmlTemplate, xnl.Count);
                    }

                    //勾选样式部分，可能和表格共存，所以去最大值，但是不可能都没有
                    int b = 0;
                    if (xnlPages != null && xnlPages.Count != 0)
                    {
                        b = xnlPages.Count;
                    }
                    else
                    {
                        b = 0;
                    }

                    if (a == 0 && b == 0)
                    {
                        //log.Error("没有读到Record或者Page子节点");
                        return -1;
                    }

                    return a > b ? a : b; //至少有一个不为0的时候，只大的那个值
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                return -1;
            }
        }

        /// <summary>
        /// 特殊表格：第一页和第二页以后行数不一样的情况。获取第一页的行数
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="currentPageRows"></param>
        /// <returns></returns>
        public int GetPageRowsFristPage(XmlDocument xmlDoc)
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
                node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), "1", "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));//"NurseForm/Design/Form[@ID='1']"
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
        public int GetPageRowsSecondPage(XmlDocument xmlDoc)
        {
            int ret = 50;
            XmlNode node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));

            if (xmlDoc.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form))).Count == 1)
            {
                node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));
            }
            else
            {
                node_FirstPage = xmlDoc.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), "1", "!=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form))); //其实只能是第二页"NurseForm/Design/Form[@ID!='1']"
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
        public int GetPageCount_HaveTable(XmlDocument xmlDoc, int recordCount)
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
                }
            }
            else
            {
                if (recordCount > firstPageRows)
                {
                    pageCount = (recordCount + pageRows - firstPageRows) / pageRows;

                    if ((recordCount + pageRows - firstPageRows) % pageRows > 0)
                    //if (recordCount % pageRows > 0)
                    {
                        pageCount++;
                    }
                }
                else
                {
                    //小于等于 第一页的行数，那么肯定是第一页
                    pageCount = 1;
                }
            }

            return pageCount;
        }

        #endregion

        /// <summary>
        /// 录入数据的时候的处理方法（支持表中中的一行，或者非表格的一页）
        /// </summary>
        /// <param name="chartXml">当前的表单xml数据</param>
        /// <param name="thisRecordpara">需要插入的改行数据record：体温@38¤脉搏@122  如果没有就传入Null，如果是勾选框，选中True，未选中False</param>
        /// <param name="thisPageValuePara">本页数据内容  如果没有没有表格数据就传入Null或者空，必须指定页数：页数@n ，其他名字可能需要转义配置成表单中的名字对应；非表格的数据</param>
        /// <param name="userInforPara">插入数据的用户信息：string[] userInforPara = new string[] { _UserID, _UserName, _UserDepartMent, _UserInpatientArea, _User_Degree };
        /// paitentInforPara：new string[] { PatientID，姓名 }</param>
        /// otherTableColumns：如果为空，表示一个表格。如果不为空，表示另外的表格的所有列，那么需要判断基于这些列的最晚的非空白行
        /// 列名1¤列名2 需要对左右不对称的表格，分别输入，那么右边的输入行的时候，往往不是在最后一个节点往后添加，而是从后往前找到第一个空白行开始</param>
        /// <returns></returns>
        public XmlNode GetChartXmlAddRecord(XmlNode chartXml, string thisRecordpara, string thisPageValuePara,
            string[] userInforPara, string[] paitentInforPara, List<SparkFormEditor.RowForeColors> rowForeColorsList, XmlDocument xmlDocTemplate, string templateName)
        {
            string[] valueArr;

            string[] thisRecord;
            string[] thisPageValue;

            int maxId = 0;

            if (chartXml == null)
            {
                //如果是新建，那就没有该xml，则就需要新建空白的含有根节点的xml文件；并写入病人姓名和创建用户的工号等信息
                //MyXml mx = new MyXml();
                chartXml = CreateTemplate(paitentInforPara[0], paitentInforPara[1], userInforPara[0], userInforPara[2], xmlDocTemplate, templateName).DocumentElement;

                //这里，如果thisRecordpara和thisPageValuePara都为空，那么就是一场情况，必须有值
            }

            //表格部分
            //XmlNode recordsNode = chartXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            XmlNode recordsNode = chartXml.SelectSingleNode(nameof(EntXmlModel.Records));

            XmlNode newRecordNode = null;
            bool isSigned = false;

            //手摸签名控制判断（文字的内容去掉，在后者加入工号，打开表单就会自动显示）
            bool fingerPrintSign = false;
            if (!bool.TryParse((xmlDocTemplate.SelectSingleNode(nameof(EntXmlModel.NurseForm)) as XmlElement).GetAttribute(nameof(EntXmlModel.FingerPrintSign)).Trim(), out fingerPrintSign))
            {
                fingerPrintSign = false;
            }

            //添加数据：表格部分
            if (recordsNode != null && thisRecordpara != null && thisRecordpara != "")
            {
                thisRecord = thisRecordpara.Split('¤');

                //先设置好当前最大索引号，每次保存的时候，也要将最大索引号保存                             
                if (!string.IsNullOrEmpty((recordsNode as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID))))
                {
                    maxId = int.Parse((recordsNode as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID)));
                }
                else
                {
                    maxId = 0;
                }

                //maxId++;

                string thisRowTimeStr = "";  


                XmlNode nodeTable = null;   //自动调整段落列用
                XmlNode nodeColumn = null;
                string processStr = "";
                string[] arr;
                int adjustRows = 1;         //如果不调整，那么一行还是一行。但是如果接口中进行调整行，那么一行可能变成多行。如果有多个调整段落列的列，就取最大值
                int adjustMaxRows = 1;      //可能有多个调整段落列
                XmlNode paragraphsLastRow = null;
                List<XmlNode> newRecordNodeList = new List<XmlNode>();  //处理左右分开的复合表格，右侧的数据不会始终在最后位置添加行。而是根据实际情况在最后一个“局部空白行”位置添加

                string rowHeaderIndent = "";  //调整段落列的行头是否有缩进
                string otherTableColumns = ""; //合并表格的列，要单元输入行。可能要利用现有的空白行（针对自己的列来说）
                if (xmlDocTemplate != null)
                {
                    rowHeaderIndent = (xmlDocTemplate.SelectSingleNode(nameof(EntXmlModel.NurseForm)) as XmlElement).GetAttribute(nameof(EntXmlModel.FirstRowIndent));
                }

                nodeTable = xmlDocTemplate.SelectSingleNode("//" + nameof(EntXmlModel.Table));
                if (xmlDocTemplate != null && nodeTable != null)
                {
                    otherTableColumns = (nodeTable as XmlElement).GetAttribute(nameof(EntXmlModel.OtherTableColumns));
                }

                //需要对左右不对称的表格，分别输入，那么右边的输入行的时候，往往不是在最后一个节点往后添加，而是从后往前找到第一个空白行开始
                if (!string.IsNullOrEmpty(otherTableColumns))
                {
                    string[] arrOtherTableColumns = otherTableColumns.Split(',');

                    bool isOtherTable = true;
                    //并且传入的数据的列，都是定义的另外表格的列
                    for (int k = 0; k < thisRecord.Length; k++)
                    {
                        if (thisRecord[k] != null && thisRecord[k] != "") //&& thisRecord[k] != "姓名" && thisRecord[k] != "床号" && thisRecord[k] != "年龄" && thisRecord[k] != "性别"
                        {
                            valueArr = thisRecord[k].Split('@');

                            if (!System.Collections.ArrayList.Adapter(arrOtherTableColumns).Contains(valueArr[0]))
                            {
                                isOtherTable = false;
                                break;
                            }
                        }
                    }

                    //传入的数据的列，都是另外表格的列
                    if (isOtherTable)
                    {
                        bool isEmptyRecord = true;

                        //从最后一个行节点，往前遍历。根据参数传入的列
                        for (int i = recordsNode.ChildNodes.Count - 1; i >= 0; i--)
                        {
                            //isEmptyRecord = true;

                            for (int j = 0; j < arrOtherTableColumns.Length; j++)
                            {
                                if (!string.IsNullOrEmpty(((recordsNode.ChildNodes[i] as XmlElement).GetAttribute(arrOtherTableColumns[j]).Split('¤')[0])))
                                {
                                    isEmptyRecord = false;
                                    break;  //不是空的，跳出该行的判断，下面会继续跳出外层循环
                                }
                            }

                            if (!isEmptyRecord)
                            {
                                break; //遇到第一个
                            }
                            else
                            {
                                //newRecordNodeList.Add(recordsNode.ChildNodes[i]);  

                                newRecordNodeList.Insert(0, recordsNode.ChildNodes[i]);//加入到节点集合中，但是这样每次行编号会累加重新计算(后面的节点)
                            }
                        }
                    }
                }

                //不是左右复合表格的时候
                if (newRecordNodeList.Count == 0)
                {
                    #region 判断是否存在该节点，如果存在该节点就是更新：“日期”、“时间”
                    if (xmlDocTemplate != null)
                    {
                        nodeTable = xmlDocTemplate.SelectSingleNode("//" + nameof(EntXmlModel.Table));

                        //是否开启了，识别更新合并。原来的接口只能每次在最后插入数据，不能根据主键更新行
                        bool MergeUpdate = false;
                        if (!bool.TryParse((nodeTable as XmlElement).GetAttribute(nameof(EntXmlModel.MergeUpdate)).Trim(), out MergeUpdate))
                        {
                            //MergeUpdate = false;
                            MergeUpdate = true; //默认改为true
                        }

                        if (nodeTable != null && nodeTable.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.Name), "日期", "=", nameof(EntXmlModel.Column))) != null && MergeUpdate) //至少必须有日期 //"Column[@Name='日期']"
                        {
                            string[] arrDateTime = new string[] { "", "" };
                            for (int k = 0; k < thisRecord.Length; k++)
                            {
                                if (thisRecord[k] != null && thisRecord[k] != "")
                                {
                                    valueArr = thisRecord[k].Split('@');

                                    if (valueArr[0] == "日期")
                                    {
                                        arrDateTime[0] = valueArr[1];
                                    }

                                    if (valueArr[0] == "时间")
                                    {
                                        arrDateTime[1] = valueArr[1];
                                    }
                                }
                            }

                            //如果日期和时间，至少有一个有值，那么进行判断：是否已经存在该时间点数据（行的唯一标识主键）
                            if (!string.IsNullOrEmpty(arrDateTime[0]) || !string.IsNullOrEmpty(arrDateTime[1]))
                            {
                                string[] arrDateTimeRecord;
                                //从最后一个行节点，往前遍历。根据参数传入的列
                                for (int i = recordsNode.ChildNodes.Count - 1; i >= 0; i--)
                                {
                                    //if ((recordsNode.ChildNodes[i] as XmlElement).GetAttribute("日期") == arrDateTime[0] && (recordsNode.ChildNodes[i] as XmlElement).GetAttribute("时间") == arrDateTime[1])
                                    arrDateTimeRecord = GetRowDateTime(recordsNode, (recordsNode.ChildNodes[i] as XmlElement).GetAttribute(nameof(EntXmlModel.ID)), nodeTable);
                                    if (arrDateTimeRecord[0] == arrDateTime[0] && arrDateTimeRecord[1] == arrDateTime[1])
                                    {
                                        newRecordNodeList.Insert(0, recordsNode.ChildNodes[i]);//加入到节点集合中，但是这样每次行编号会累加重新计算(后面的节点)
                                    }
                                    else
                                    {
                                        if (newRecordNodeList.Count != 0)
                                        {
                                            break; //如果已经找到，那么就跳出。否则还是继续往后找
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion 判断是否存在该节点，如果存在该节点就是更新：“日期”、“时间”

                    //在最后位置插入，新行
                    if (newRecordNodeList.Count == 0) //newRecordNode == null
                    {
                        newRecordNode = chartXml.OwnerDocument.CreateElement(nameof(EntXmlModel.Record));
                        recordsNode.InsertAfter(newRecordNode, recordsNode.LastChild);

                        newRecordNodeList.Add(newRecordNode);
                    }
                }


                //设置表格内容
                for (int k = 0; k < thisRecord.Length; k++)
                {
                    if (thisRecord[k] != null && thisRecord[k] != "") //&& thisRecord[k] != "姓名" && thisRecord[k] != "床号" && thisRecord[k] != "年龄" && thisRecord[k] != "性别"
                    {
                        valueArr = thisRecord[k].Split('@');

                        //防止更新为空。当然可以让客户端在更新的时候，空的不要传入即可。
                        if (valueArr[1].Trim() == "")
                        {
                            continue; 
                        }

                        newRecordNode = newRecordNodeList[0]; //重置为第一个添加的行节点

                        if ((valueArr[0] == "签名" || valueArr[0] == "记录者") && valueArr[1].Trim() != "")
                        {
                            isSigned = true; //行签名，权限控制处理
                        }

                        if (valueArr[0] == "时间")  //根据时间，在保存的时候，根据配置项，设置行文字颜色
                        {
                            thisRowTimeStr = valueArr[1];
                        }

                        //----------------------------------------接口保存数据的时候自动调整段落列判断
                        //<Column Name="调整段落列" RankColumnText="True"  XmlDocument xmlDocTemplate = new XmlDocument();
                        nodeTable = xmlDocTemplate.SelectSingleNode("//" + nameof(EntXmlModel.Table));
                        if (xmlDocTemplate != null && nodeTable != null)
                        {
                            nodeColumn = nodeTable.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.Name), valueArr[0], "=", nameof(EntXmlModel.Column))); //"Column[@Name='" + valueArr[0] + "']"
                            if (nodeColumn != null && (nodeColumn as XmlElement).GetAttribute(nameof(EntXmlModel.RankColumnText)).ToLower() == "true")
                            {
                                processStr = WordWrap.lineStringAnalys(valueArr[1], Comm.getSize((nodeColumn as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize().Width + 3, Comm.getFont((nodeColumn as XmlElement).GetAttribute(nameof(EntXmlModel.Width))), "");
                                arr = processStr.Split(new string[] { "¤" }, StringSplitOptions.RemoveEmptyEntries);

                                adjustRows = arr.Length;

                                adjustMaxRows = adjustMaxRows > adjustRows ? adjustMaxRows : adjustRows;

                                if (adjustRows > 1)
                                {
                                    //调整行后：
                                    for (int a = 0; a < adjustRows; a++)
                                    {
                                        if (a < newRecordNodeList.Count)
                                        {
                                            newRecordNode = newRecordNodeList[a];
                                        }
                                        else
                                        {
                                            //在最后位置插入，新行
                                            newRecordNode = chartXml.OwnerDocument.CreateElement(nameof(EntXmlModel.Record));
                                            recordsNode.InsertAfter(newRecordNode, recordsNode.LastChild); //recordsNode.InsertAfter(newRecordNode, newRecordNodeList[newRecordNodeList.Count - 1]); //插在最后一行其实不对的，应该是当前处理节点的后面
                                            newRecordNodeList.Add(newRecordNode);
                                        }

                                        //记录下，段落的最后一行。如果是多个段落，记录下最下的那一行
                                        if (a + 1 == adjustMaxRows)
                                        {
                                            paragraphsLastRow = newRecordNode;
                                        }

                                        //手摸签名也是签一个段落的最后一行，但这里是调整段落列那一列，不是签名列。所以这里不能处理

                                        (newRecordNode as XmlElement).SetAttribute(valueArr[0], arr[a] + "¤False¤False¤" + userInforPara[0] + "¤False¤False¤False¤¤Black¤");
                                    }

                                    continue; //不要往下执行了，下面是只有一行的情况。处理下一列
                                }
                            }
                        }

                        //如果是手摸签名，并且是签名列，并且输入框内容为用户的名字。那么将内容存为空、并记录下工号，打开表单显示为图片。
                        //要避免的问题：如果是调整段落列，那就不能签名显示在该段落第一行，而是在该段落最后一行。
                        //保证【调整段落的列】传参时在【签名列】之前
                        if ((valueArr[0].StartsWith("签名") || valueArr[0].StartsWith("记录者")) && valueArr[1].Trim() == userInforPara[1])
                        {
                            //手摸签名的时候：不显示姓名的文字内容，空白；表单会显示图片的
                            if (fingerPrintSign)
                            {
                                if (paragraphsLastRow != null)
                                {
                                    (paragraphsLastRow as XmlElement).SetAttribute(valueArr[0], "¤False¤False¤" + userInforPara[0] + "¤False¤False¤False¤¤Black¤");
                                }
                                else
                                {
                                    //手摸签名的，文字内容为空、以便显示图片，记录下工号
                                    (newRecordNode as XmlElement).SetAttribute(valueArr[0], "¤False¤False¤" + userInforPara[0] + "¤False¤False¤False¤¤Black¤");
                                }
                            }
                            else
                            {
                                if (paragraphsLastRow != null)
                                {
                                    (paragraphsLastRow as XmlElement).SetAttribute(valueArr[0], valueArr[1] + "¤False¤False¤" + userInforPara[0] + "¤False¤False¤False¤¤Black¤");
                                }
                                else
                                {
                                    //手摸签名的，文字内容为空、以便显示图片，记录下工号
                                    (newRecordNode as XmlElement).SetAttribute(valueArr[0], valueArr[1] + "¤False¤False¤" + userInforPara[0] + "¤False¤False¤False¤¤Black¤");
                                }
                            }
                        }
                        else
                        {
                            //其他项目列（非签名），即使一个段落多行，也始终显示在第一行。只有签名显示在一个段落最后一行
                            (newRecordNode as XmlElement).SetAttribute(valueArr[0], valueArr[1] + "¤False¤False¤" + userInforPara[0] + "¤False¤False¤False¤¤Black¤");
                        }
                    }
                }

                //添加的节点，有可能是多个段落行的，一起设置行节点属性：签名，文字颜色，编号等
                for (int a = 0; a < newRecordNodeList.Count; a++)
                {
                    maxId++;
                    SetReXcord(recordsNode, newRecordNodeList[a], maxId, thisRowTimeStr, rowForeColorsList, isSigned, userInforPara);
                }
            }

            //添加数据：非表格部分，比如评估单等 //格式：Text 0¤单红线 1¤双红线 2¤权限 3¤Rtf 4¤修订历史 5¤日期格式 6
            if (thisPageValuePara != null && thisPageValuePara != "")
            {
                thisPageValue = thisPageValuePara.Split('¤');

                //非表格部分要指定页数：
                string strPage = "";

                for (int k = 0; k < thisPageValue.Length; k++)
                {
                    if (thisPageValue[k] != null && thisPageValue[k] != "") //&& thisRecord[k] != "姓名" && thisRecord[k] != "床号" && thisRecord[k] != "年龄" && thisRecord[k] != "性别"
                    {
                        valueArr = thisPageValue[k].Split('@');

                        if (valueArr[0] == "页数")
                        {
                            strPage = valueArr[1].Trim();
                        }
                    }
                }

                if (strPage != "")
                {
                    //取到本页节点，有就是更新，没有就是插入
                    //XmlNode thisPage = chartXml.SelectSingleNode("NurseForm/Forms/Form[@SN='" + strPage + "']");
                    XmlNode thisPage = chartXml.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), strPage, "=", nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));

                    if (thisPage != null)
                    {
                        //不用添加页节点
                    }
                    else
                    {
                        //先添加页页结点
                        //<?xml version="1.0" encoding="utf-8"?>
                        //<NurseForm>
                        //    <Pages>
                        //     <Page SN="1"/>
                        //    </Forms>
                        //    <Records />
                        //</NurseForm>

                        //XmlNode pages = chartXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms)));
                        XmlNode pages = chartXml.SelectSingleNode(nameof(EntXmlModel.Forms));
                        //插入新的节点
                        XmlElement newPage;
                        newPage = chartXml.OwnerDocument.CreateElement(nameof(EntXmlModel.Form));
                        newPage.SetAttribute(nameof(EntXmlModel.SN), strPage);

                        pages.AppendChild(newPage);

                        //thisPage = chartXml.SelectSingleNode("NurseForm/Forms/Form[@SN='" + strPage + "']");
                        thisPage = chartXml.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), strPage, "=", nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));//"Forms/Form[@SN='" + strPage + "']"
                    }

                    //插入数据
                    for (int k = 0; k < thisPageValue.Length; k++)
                    {
                        if (thisPageValue[k] != null && thisPageValue[k] != "") //&& thisRecord[k] != "姓名" && thisRecord[k] != "床号" && thisRecord[k] != "年龄" && thisRecord[k] != "性别"
                        {
                            valueArr = thisPageValue[k].Split('@');
                            (thisPage as XmlElement).SetAttribute(valueArr[0], valueArr[1]); //这样也能支持床号等 + "¤False¤False¤¤¤¤"



                            //基本信息体现历史(转科、签床)；调整段落列的；除非把客户端逻辑移过来，或者每天打开客户端操作一下并保存

                        }
                    }

                    DateTime serverDate = DateTime.Now;

                    //更新时间
                    (thisPage as XmlElement).SetAttribute(nameof(EntXmlModel.VALIDATE), string.Format("{0:yyyy-MM-dd HH:mm}", serverDate));

                    //更新用户
                    if ((thisPage as XmlElement).GetAttribute(nameof(EntXmlModel.UserName)) == "")
                    {
                        (thisPage as XmlElement).SetAttribute(nameof(EntXmlModel.UserName), userInforPara[0]);
                    }
                }
            }

            return chartXml;
        }

        # region 获取数据行的时间
        /// <summary>
        /// 扩展用方法：
        /// 根据规律，找到指定行对应的时间
        /// 因为书写规范上是，每一页如果和上一次行的日期一样就为空的。
        /// </summary>
        /// <param name="recordsNode"></param>
        /// <param name="rowXmlID"></param>
        /// <param name="nodeTable"></param>
        /// <returns></returns>
        private string[] GetRowDateTime(XmlNode recordsNode, string rowXmlID, XmlNode nodeTable)
        {
            //string dt = "¤"; //用小太阳"¤"来分割日期和之间进行返回
            string date = "";
            string time = "";


            ////如果异常情况下，rowXmlID为空，那么cellNode就为null，就会报错
            XmlNode cellNode = recordsNode.SelectSingleNode("//" + EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), rowXmlID, "=", nameof(EntXmlModel.Record)));  //"Record[@ID='" + rowXmlID + "']"

            date = (cellNode as XmlElement).GetAttribute("日期").Split('¤')[0].Trim();
            time = (cellNode as XmlElement).GetAttribute("时间").Split('¤')[0].Trim();

            //是否包含时间
            bool isHaveTimeCol = true;
            //if (nodeTable.SelectSingleNode("时间") != null)
            if (nodeTable.SelectSingleNode("Column[@Name='时间']") != null)
            {
                isHaveTimeCol = true;
            }
            else
            {
                //time = "12:00";
                time = "-"; //空格，最后返回的地方去掉
                isHaveTimeCol = false;
            }

            //如果为空，那么循环往上取值
            while ((date == "" || time == ""))
            {
                cellNode = cellNode.PreviousSibling; //如果当前节点已经是最前面的节点，那么返回null

                if (cellNode != null)
                {
                    if (date == "")
                    {
                        date = (cellNode as XmlElement).GetAttribute("日期").Split('¤')[0].Trim();
                    }

                    if (time == "")
                    {
                        time = (cellNode as XmlElement).GetAttribute("时间").Split('¤')[0].Trim();
                    }

                    if (isHaveTimeCol)
                    {
                        if (date != "" && time != "")
                        {
                            break; //都不会空，那么跳出循环；表示已经取到值了
                        }
                    }
                    else
                    {
                        if (date != "")
                        {
                            break; //只有日期的时候，日期不为空，那么跳出循环；表示已经取到值了
                        }
                    }
                }
                else
                {
                    ////到了第一个节点，还是没有找到
                    //if (_TableInfor.CellByRowNo_ColName(0, "日期") != null)
                    //{
                    //    //ShowInforMsg("表格日期或者时间书写不规范，无法找到段落的对应日期或时间。");
                    //}

                    break;//没有找到合法的日期和时间，跳出
                }
            }

            //XmlNode tempNode = cellNode.PreviousSibling; //如果当前节点已经是最前面的节点，那么返回null

            //return date + "¤" + time;
            return new string[] { date.Trim(), time.Trim().TrimStart('-') };
        }
        # endregion 获取行的时间

        /// <summary>
        /// 设置节点的基本属性：行颜色，编号，签名等
        /// </summary>
        /// <param name="recordsNode">行的总节点</param>
        /// <param name="newRecordNode">行节点</param>
        /// <param name="maxId">最大编号</param>
        /// <param name="thisRowTimeStr"></param>
        /// <param name="rowForeColorsList"></param>
        /// <param name="isSigned"></param>
        /// <param name="userInforPara"></param>
        private void SetReXcord(XmlNode recordsNode, XmlNode newRecordNode, int maxId, string thisRowTimeStr, List<SparkFormEditor.RowForeColors> rowForeColorsList, bool isSigned, string[] userInforPara)
        {
            //更新编号
            (newRecordNode as XmlElement).SetAttribute(nameof(EntXmlModel.ID), maxId.ToString());
            (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), maxId.ToString()); //最大索引编号自增数

            //行信息：
            //根据设置，依据每行数据的颜色进行指定颜色
            if (rowForeColorsList.Count > 0 && thisRowTimeStr != "")
            {
                DateTime thisRowTime = DateTime.Now;
                SparkFormEditor.RowForeColors rfcEach;

                thisRowTimeStr = thisRowTimeStr == "24:00" ? "23:59" : thisRowTimeStr;

                if (DateTime.TryParse("2000-01-01 " + thisRowTimeStr, out thisRowTime))
                {
                    //是规范的日期格式，才做处理
                    for (int jj = 0; jj < rowForeColorsList.Count; jj++)
                    {
                        rfcEach = rowForeColorsList[jj];

                        if (rfcEach.DtForm.CompareTo(thisRowTime) <= 0 && rfcEach.DtTo.CompareTo(thisRowTime) >= 0)
                        {
                            (newRecordNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), "Black¤None¤" + rfcEach.ForeColor.Name);      //行信息
                            break;
                        }
                    }
                }
            }

            //设置签名信息
            //如果没有填写签名，则不要，不然其他人就不能修改了
            if (isSigned)
            {
                //UserID="20123¤赵丽¤护士长"
                (newRecordNode as XmlElement).SetAttribute(nameof(EntXmlModel.UserID), userInforPara[0] + "¤" + userInforPara[1] + "¤" + userInforPara[4]);
            }
        }



    }
}
