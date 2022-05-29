using SparkFormEditor.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SparkFormEditor
{
    /// <summary>
    /// 表单帮助类
    /// </summary>
    class TemplateHelp
    {
        /// <summary>
        /// 创建xml(根据模板配置来，比如创建指定好的页数)
        /// </summary>
        /// <returns></returns>
        public XmlDocument CreateTemplate(string patientId, string patientName, string userID, string userDepartMent, XmlDocument templateXml, string templateName)
        {
            try
            {

                XmlNode tempRoot = templateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm));
                int creatPages = 1;
                if (!int.TryParse((tempRoot as XmlElement).GetAttribute(nameof(EntXmlModel.AutoCreatPagesCount)), out creatPages))
                {
                    creatPages = 1;
                }

                XmlDocument retXml = new XmlDocument();
                XmlDeclaration dec;
                XmlElement root;

                XmlElement Pages;
                XmlElement Records;

                dec = retXml.CreateXmlDeclaration("1.0", "utf-8", null);
                retXml.AppendChild(dec);
                root = retXml.CreateElement(nameof(EntXmlModel.NurseForm));

                //病人信息
                root.SetAttribute(nameof(EntXmlModel.UHID), patientId);
                root.SetAttribute(nameof(EntXmlModel.Name), patientName);

                string strDate = string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now);
                ////创建者信息
                //root.SetAttribute("CreatUserID", userIDPara);
                //root.SetAttribute("CreatUserDepartMent", userDepartMentPara);
                //root.SetAttribute("CreatDateTime", string.Format("{0:yyyy MM dd HH mm ss ffff}", DateTime.Now)); //创建时间到毫秒，也可以作为唯一号来确定这个表单

                root.SetAttribute(nameof(EntXmlModel.FormName), templateName); //"表单名"
                root.SetAttribute(nameof(EntXmlModel.FORM_TYPE), (tempRoot as XmlElement).GetAttribute(nameof(EntXmlModel.TemplateRule)));
                root.SetAttribute(nameof(EntXmlModel.Forms), creatPages.ToString());

                root.SetAttribute(nameof(EntXmlModel.ChartType), nameof(EntXmlModel.Record));
                root.SetAttribute(nameof(EntXmlModel.SequenceNo), "0");
                root.SetAttribute(nameof(EntXmlModel.CreatDate), strDate);
                root.SetAttribute(nameof(EntXmlModel.UpdateDate), strDate);
                root.SetAttribute(nameof(EntXmlModel.CreatUser), userID);
                root.SetAttribute(nameof(EntXmlModel.UpdateUser), userID);

                //  NurseForm/Forms ，NurseForm/Records
                Pages = retXml.CreateElement(nameof(EntXmlModel.Forms));
                Records = retXml.CreateElement(nameof(EntXmlModel.Records));

                root.AppendChild(Pages);
                root.AppendChild(Records);

                //自动创建指定页
                if (creatPages > 1)
                {
                    Comm.LogHelp.WriteDebug($"在TemplateHelp.CreateTemplate方法中，自动创建的页数：{creatPages} 堆栈信息：{Comm.GetStackTrace()}");
                    //插入新的节点
                    XmlElement newPage;

                    for (int i = 1; i <= creatPages; i++)
                    {
                        newPage = retXml.CreateElement(nameof(EntXmlModel.Form));

                        newPage.SetAttribute(nameof(EntXmlModel.SN), i.ToString());

                        Pages.AppendChild(newPage);
                    }
                }

                retXml.AppendChild(root);

                return retXml;
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        public XmlDocument CreateEmptyTemplate(string hisid, string namePara, string userIDPara, string userDepartMentPara, XmlDocument templateXml, string templateName)
        {
            //<?xml version="1.0" encoding="utf-8"?>
            //<NurseForm>
            //  <Pages />
            //  <Records />
            //</NurseForm>
            try
            {

                XmlNode tempRoot = templateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm));
                int creatPages = 1;
                if (!int.TryParse((tempRoot as XmlElement).GetAttribute(nameof(EntXmlModel.AutoCreatPagesCount)), out creatPages))
                {
                    creatPages = 1;
                }

                XmlDocument retXml = new XmlDocument();
                XmlDeclaration dec;
                XmlElement root;

                XmlElement Pages;
                XmlElement Records;

                dec = retXml.CreateXmlDeclaration("1.0", "utf-8", null);
                retXml.AppendChild(dec);
                root = retXml.CreateElement(nameof(EntXmlModel.NurseForm));

                //病人信息
                root.SetAttribute(nameof(EntXmlModel.HISID), hisid);
                root.SetAttribute(nameof(EntXmlModel.Name), namePara);

                string strDate = string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now);
                ////创建者信息
                //root.SetAttribute("CreatUserID", userIDPara);
                //root.SetAttribute("CreatUserDepartMent", userDepartMentPara);
                //root.SetAttribute("CreatDateTime", string.Format("{0:yyyy MM dd HH mm ss ffff}", DateTime.Now)); //创建时间到毫秒，也可以作为唯一号来确定这个表单

                root.SetAttribute(nameof(EntXmlModel.FormName), templateName); //"表单名"
                root.SetAttribute(nameof(EntXmlModel.FORM_TYPE), (tempRoot as XmlElement).GetAttribute(nameof(EntXmlModel.TemplateRule)));
                root.SetAttribute(nameof(EntXmlModel.Forms), creatPages.ToString());

                root.SetAttribute(nameof(EntXmlModel.ChartType), nameof(EntXmlModel.Record));
                root.SetAttribute(nameof(EntXmlModel.SequenceNo), "0");
                root.SetAttribute(nameof(EntXmlModel.CreatDate), strDate);
                root.SetAttribute(nameof(EntXmlModel.UpdateDate), strDate);
                root.SetAttribute(nameof(EntXmlModel.CreatUser), userIDPara);
                root.SetAttribute(nameof(EntXmlModel.UpdateUser), userIDPara);

                //  NurseForm/Forms ，NurseForm/Records
                Pages = retXml.CreateElement(nameof(EntXmlModel.Forms));
                Records = retXml.CreateElement(nameof(EntXmlModel.Records));

                root.AppendChild(Pages);
                root.AppendChild(Records);

                //自动创建指定页
                if (creatPages > 1)
                {
                    //插入新的节点
                    XmlElement newPage;

                    for (int i = 1; i <= creatPages; i++)
                    {
                        newPage = retXml.CreateElement(nameof(EntXmlModel.Form));

                        newPage.SetAttribute(nameof(EntXmlModel.SN), i.ToString());

                        Pages.AppendChild(newPage);
                    }
                }

                retXml.AppendChild(root);

                return retXml;
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 创建xml
        /// </summary>
        /// <returns></returns>
        public XmlDocument CreateTemplate(string uhidPara, string namePara, string userIDPara, string userDepartMentPara)
        {
            //<?xml version="1.0" encoding="utf-8"?>
            //<NurseForm>
            //  <Pages />
            //  <Records />
            //</NurseForm>
            try
            {
                XmlDocument retXml = new XmlDocument();
                XmlDeclaration dec;
                XmlElement root;

                XmlElement Pages;
                XmlElement Records;

                dec = retXml.CreateXmlDeclaration("1.0", "utf-8", null);
                retXml.AppendChild(dec);
                root = retXml.CreateElement(nameof(EntXmlModel.NurseForm));

                //病人信息
                root.SetAttribute(nameof(EntXmlModel.UHID), uhidPara);
                root.SetAttribute(nameof(EntXmlModel.Name), namePara);

                //创建者信息
                root.SetAttribute(nameof(EntXmlModel.CreatUserID), userIDPara);
                root.SetAttribute(nameof(EntXmlModel.CreatUserDepartMent), userDepartMentPara);

                root.SetAttribute(nameof(EntXmlModel.CreatDateTime), string.Format("{0:yyyy MM dd HH mm ss ffff}", DateTime.Now)); //创建时间到毫秒，也可以作为唯一号来确定这个表单

                //  NurseForm/Forms ，NurseForm/Records
                Pages = retXml.CreateElement(nameof(EntXmlModel.Forms));
                Records = retXml.CreateElement(nameof(EntXmlModel.Records));

                root.AppendChild(Pages);
                root.AppendChild(Records);

                retXml.AppendChild(root);

                return retXml;
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 给指定节点添加节点：在做末尾添加节点
        /// </summary>
        /// <param name="xmldoc"></param>
        public static XmlNode AddXmlRecordNode(XmlDocument xmldoc)
        {
            XmlNode refNode = null;
            XmlNode XmlNodeNurseForm = xmldoc.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            int index = XmlNodeNurseForm.SelectNodes(nameof(EntXmlModel.Record)).Count;
            if (index != 0)
            {
                refNode = XmlNodeNurseForm.SelectNodes(nameof(EntXmlModel.Record))[index - 1];
            }

            XmlNode parent = XmlNodeNurseForm; // xmldoc.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            XmlElement newNode = xmldoc.CreateElement(nameof(EntXmlModel.Record));
            parent.InsertAfter(newNode, refNode);

            return newNode;
        }

        /// <summary>
        /// 获取指定节点的值，先排除其子节点（默认会把子节点的值也获取过来的）
        /// </summary>
        /// <param name="xnPara"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XmlNode GetDelChildInnerText(XmlNode xnPara, string name)
        {
            XmlNode xnRet = null;

            if (xnPara.SelectSingleNode("//" + name) != null)
            {
                xnRet = xnPara.SelectSingleNode("//" + name).Clone();

                for (int j = xnRet.ChildNodes.Count - 1; j >= 0; j--)
                {
                    if (xnRet.ChildNodes[j].Name != @"#text")
                    {
                        xnRet.RemoveChild(xnRet.ChildNodes[j]); //删除所有子节点，但是节点值如果不为空，也认为是个子节点，但是name是@"#text"
                    }
                }
            }

            return xnRet;
        }

        /// <summary>
        /// 判断一个节点的某个属性是否存在;(xnTemplates as XmlElement).GetAttribute("arbName"),就算属性不存在，也是返回空。
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="attributesName"></param>
        /// <returns></returns>
        public static bool HaveAttribute(XmlNode xn, string attributesName)
        {
            //(xn as XmlElement).Attributes["Mode"]
            if ((xn as XmlElement).Attributes[attributesName] == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 删除某个节点的属性
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="attributesName"></param>
        /// <returns></returns>
        public static void DeleteAttribute(XmlNode xn, string attributesName)
        {
            XmlAttribute xa = (xn as XmlElement).Attributes[attributesName];
            if (xa != null)
            {
                xn.Attributes.Remove(xa);
            }
        }
    }
}
