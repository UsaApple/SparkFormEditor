using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Drawing.Printing;
using System.IO;

namespace SparkFormEditor
{
    ///<summary>
    ///打印纸张类型设置
    ///</summary>
    internal class SettingPrintPaper
    {
        /// <summary>
        /// 实例化方法
        /// </summary>
        public SettingPrintPaper()
        {
        }

        //设置默认打印机
        //关于打印机纸张设定，在弹出的打印机选择和设置对话框中，点击首选项或者属性，都是第一个默认打印机设置的纸张类型。
        //除非是可以在打印机上右击首选项还是系统中打印机设置的默认纸张
        //而且，第一个默认打印机的纸张类型中可能不包含第二个打印机的纸张类型。但是表单需要用第二个非默认打印机打印的话。类型设置也就无效了。
        public string SetDefaultPrinterName(PrinterSettings ps, string printName)
        {
            try
            {
                if (!string.IsNullOrEmpty(printName))
                {
                    bool isHavaThePrinter = false;
                    foreach (string sPrint in PrinterSettings.InstalledPrinters)//获取所有打印机名称
                    {
                        if (sPrint == printName)
                        {
                            isHavaThePrinter = true;
                            break;
                        }
                    }

                    if (isHavaThePrinter)
                    {
                        ps.PrinterName = printName;
                    }
                }
            }
            catch (Exception ex)
            {
                Comm.ShowErrorMessage("设置默认打印机失败，错误信息:{0}", ex);
            }
            return null;
        }

        /// <summary>
        /// 获取打印机纸张大小，验证打印机是否有相应的纸张大小设置
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="printerName"></param>
        /// <returns></returns>
        public string GetPaperSizeRawKind(string pageName, string printerName)
        {
            //不设置默认打印机的纸张大小
            int sizeValue = 9;
            //未做验证，待扩展
            if (!string.IsNullOrEmpty(pageName))
            {
                return getPaperSizeRawKindFromPrinterDefault(printerName, pageName);
            }
            return "";
        }

        /// <summary>
        /// 取得打印机的默认设置纸张类型
        /// http://msdn.microsoft.com/zh-tw/library/system.drawing.printing.papersize.rawkind(v=vs.80).aspx
        /// </summary>
        /// <param name="printerName"></param>
        /// <returns></returns>
        private string getPaperSizeRawKindFromPrinterDefault(string printerName, string pageName)
        {
            try
            {
                System.Drawing.Printing.PrintDocument pd;
                foreach (string sPrint in PrinterSettings.InstalledPrinters)//获取所有打印机名称
                {
                    if (printerName == sPrint)
                    {
                        pd = new System.Drawing.Printing.PrintDocument();
                        pd.PrinterSettings.PrinterName = printerName;

                        //retValue = pd.DefaultPageSettings.PaperSize.RawKind;
                        foreach (PaperSize ps in pd.PrinterSettings.PaperSizes)
                        {
                            if (ps.PaperName.Equals(pageName, StringComparison.OrdinalIgnoreCase))
                            {
                                return ps.RawKind.ToString();
                            }
                        }
                        return "";
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                return "";
            }
        }



    }
}
