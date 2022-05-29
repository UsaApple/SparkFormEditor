using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Data;

namespace SparkFormEditor
{

    /// <summary>
    /// 全角、半角转换
    /// </summary>
    internal class StrHalfFull
    {
        /// 转全角的函数
        ///任意字符串
        ///全角字符串
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///
        public static string ToSBC_Full(string input)
        {
            // 半角转全角：
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }

        // 转半角的函数
        //任意字符串
        //半角字符串
        //全角空格为12288，半角空格为32
        //其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        /// <summary>
        /// 转半角的函数
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToDBC_Half(String input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

    }

    /// <summary>
    /// 取汉字首字母
    /// </summary>
    internal class GetStringSpell
    {
        /// <summary>
        /// 提取汉字首字母
        /// </summary>
        /// <param name="strText">需要转换的字</param>
        /// <returns>转换结果</returns>
        public static string GetChineseSpell(string strText)
        {
            int len = strText.Length;
            string myStr = "";
            for (int i = 0; i < len; i++)
            {
                myStr += getSpell(strText.Substring(i, 1));
            }
            return myStr;
        }

        /// <summary>
        /// 把提取的字母变成大写
        /// </summary>
        /// <param name="strText">需要转换的字符串</param>
        /// <returns>转换结果</returns>
        public static string GetLowerChineseSpell(string strText)
        {
            return GetChineseSpell(strText).ToLower();
        }

        /// <summary>
        /// 把提取的字母变成大写
        /// </summary>
        /// <param name="strText">需要转换的字符串</param>
        /// <returns>转换结果</returns>
        public static string GetUpperChineseSpell(string strText)
        {
            return GetChineseSpell(strText).ToUpper();
        }

        /// <summary>
        /// 获取单个汉字的首拼音
        /// </summary>
        /// <param name="myChar">需要转换的字符</param>
        /// <returns>转换结果</returns>
        public static string getSpell(string myChar)
        {
            byte[] arrCN = System.Text.Encoding.Default.GetBytes(myChar);
            if (arrCN.Length > 1)
            {
                int area = (short)arrCN[0];
                int pos = (short)arrCN[1];
                int code = (area << 8) + pos;
                int[] areacode = { 45217, 45253, 45761, 46318, 46826, 47010, 47297, 47614, 48119, 48119, 49062, 49324, 49896, 50371, 50614, 50622, 50906, 51387, 51446, 52218, 52698, 52698, 52698, 52980, 53689, 54481 };
                for (int i = 0; i < 26; i++)
                {
                    int max = 55290;
                    if (i != 25) max = areacode[i + 1];
                    if (areacode[i] <= code && code < max)
                    {
                        return System.Text.Encoding.Default.GetString(new byte[] { (byte)(65 + i) });
                    }
                }
                return "_";
            }
            else return myChar;
        }

    }

    /// <summary>
    /// 字符串和数字转换类
    /// </summary>
    internal class StringNumber
    {
        /// <summary>
        ///根据索引index取对应属性的int值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Point getSplitValue(string value)
        {
            try
            {
                return new Point(int.Parse(value.Split(',')[0].Trim()), int.Parse(value.Split(',')[1].Trim()));
            }
            catch (Exception ex)
            {
                throw ex;
                //return 0;
            }

        }

        /// <summary>
        ///根据索引index取对应属性的int值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Size getSize(string value)
        {
            try
            {
                return new Size(int.Parse(value.Split(',')[0].Trim()), int.Parse(value.Split(',')[1].Trim()));
            }
            catch (Exception ex)
            {
                throw ex;
                //return 0;
            }

        }

        /// <summary>
        /// 解析一个字符串，取得其左边开头的第一个连续数字
        /// </summary>
        /// <param name="srtPara"></param>
        /// <returns></returns>
        public static double getFirstNum(string srtPara)
        {
            string[] arrValue = { "" };
            int index = 0;
            string iString = "";

            for (int i = 0; i < srtPara.Length; i++)
            {
                iString = srtPara.Substring(i, 1);
                if ("0123456789.".Contains(iString)) //.支持小数
                {
                    if (iString == "." && arrValue[index].Contains("."))
                    {
                        //下一个连续数字
                        index++;
                    }
                    else
                    {
                        arrValue[index] += iString;
                    }
                }
                else
                {
                    //负数
                    if (iString == "-")
                    {
                        if (arrValue[index] == "")
                        {
                            //负号，只能是第一个位置
                            arrValue[index] += iString;
                        }
                        else
                        {
                            //下一个连续数字
                            index++;
                        }
                    }
                    else
                    {
                        //原来只考虑正数的时候
                        if (arrValue[index] != "")
                        {
                            //下一个连续数字
                            index++;
                        }
                    }
                }

                if (index > 0) break;
            }

            if (arrValue[0] == "" || arrValue[0] == "-")
            {
                return -1;
            }
            else
            {
                try
                {
                    return double.Parse(arrValue[0]);  //1、Math.Round(0.333333,2);//按照四舍五入的国际标准2、    double dbdata=0.335333;    string str1=String.Format("{0:F}",dbdata);//默认为保留两位
                }
                catch (Exception ex)
                {
                    MessageBox.Show("数值过大或者不合法，请重新输入：" + arrValue[0]);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 解析一个字符串，取得其右边开头的第一个连续数字
        /// </summary>
        /// <param name="srtPara"></param>
        /// <returns></returns>
        public static int getLastNum(string srtPara)
        {
            string[] arrValue = { "" };
            int index = 0;
            string iString = "";

            for (int i = srtPara.Length - 1; i >= 0; i--)
            {
                iString = srtPara.Substring(i, 1);
                if ("0123456789.".Contains(iString))
                {
                    //arrValue[index] += iString;
                    arrValue[index] = iString + arrValue[index];
                }
                else
                {
                    if (arrValue[index] != "")
                    {
                        index++;
                    }
                }

                if (index > 0) break;
            }

            if (arrValue[0] == "")
            {
                return -1;
            }
            else
            {
                return int.Parse(arrValue[0]);
            }
        }

    }


    /// <summary>
    /// 字符串压缩和解压
    /// (字符串长度，可以压缩7倍)
    /// SparkFormEditor.StringZipUtil
    /// </summary>
    internal class StringZipUtil
    {
        ///// <summary>  
        ///// 解压  
        ///// </summary>  
        ///// <param name=nameof(EntXmlModel.Value)></param>  
        ///// <returns></returns>  
        //public static DataSet GetDatasetByString(string Value)
        //{
        //    DataSet ds = new DataSet();
        //    string CC = GZipDecompressString(Value);
        //    System.IO.StringReader Sr = new StringReader(CC);
        //    ds.ReadXml(Sr);
        //    return ds;
        //}

        ///// <summary>  
        ///// 根据DATASET压缩字符串  
        ///// </summary>  
        ///// <param name="ds"></param>  
        ///// <returns></returns>  
        //public static string GetStringByDataset(string ds)
        //{
        //    return GZipCompressString(ds);
        //}

        /// <summary>  
        /// 获取压缩后的字符串
        /// 将传入字符串以GZip算法压缩后，返回Base64编码字符  
        /// </summary>  
        /// <param name="rawString">需要压缩的字符串</param>  
        /// <returns>压缩后的Base64编码的字符串</returns>  
        public static string GZipCompressString(string rawString)
        {
            if (string.IsNullOrEmpty(rawString) || rawString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] rawData = System.Text.Encoding.Unicode.GetBytes(rawString.ToString());
                byte[] zippedData = Compress(rawData);
                return (string)(Convert.ToBase64String(zippedData));
            }
        }

        /// <summary>  
        /// GZip压缩  
        /// </summary>  
        /// <param name="rawData"></param>  
        /// <returns></returns>  
        static byte[] Compress(byte[] rawData)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }


        /// <summary>  
        /// 获取解压后的字符串
        /// 将传入的二进制字符串资料以GZip算法解压缩  
        /// </summary>  
        /// <param name="zippedString">经GZip压缩后的二进制字符串</param>  
        /// <returns>原始未压缩字符串</returns>  
        public static string GZipDecompressString(string zippedString)
        {
            if (string.IsNullOrEmpty(zippedString) || zippedString.Length == 0)
            {
                //为空
                return "";
            }
            else if (zippedString.StartsWith("<?xml"))
            {
                //是xml标准格式，未压缩的过的，那就不用解压缩了
                return zippedString;
            }
            else
            {
                byte[] zippedData = Convert.FromBase64String(zippedString.ToString());
                //return (string)(System.Text.Encoding.UTF8.GetString(Decompress(zippedData)));
                return (string)(System.Text.Encoding.Unicode.GetString(Decompress(zippedData)));
            }
        }


        /// <summary>  
        /// ZIP解压  
        /// </summary>  
        /// <param name="zippedData"></param>  
        /// <returns></returns>  
        public static byte[] Decompress(byte[] zippedData)
        {
            MemoryStream ms = new MemoryStream(zippedData);
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Decompress);
            MemoryStream outBuffer = new MemoryStream();
            byte[] block = new byte[1024];
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            return outBuffer.ToArray();
        }

        #region 压缩&&解压缩 -- 字符串
        /// <summary>压缩字符串</summary>
        /// <param name="value">要压缩的字符串</param>
        /// <returns>压缩后的字符串</returns>        
        public static string Zip(string value)
        {
            System.Text.Encoding encoding = System.Text.Encoding.Unicode;
            byte[] buffer = encoding.GetBytes(value);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.Compression.DeflateStream stream = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress, true);
            stream.Write(buffer, 0, buffer.Length);
            stream.Close();

            buffer = ms.ToArray();
            ms.Close();

            return Convert.ToBase64String(buffer); //将压缩后的byte[]转换为Base64String

        }

        /// <summary>解压缩字符串</summary>
        /// <param name="value">要解缩的字符串</param>
        /// <returns>解缩后的字符串</returns>
        public static string UnZip(string value)
        {
            byte[] buffer = Convert.FromBase64String(value);

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;
            System.IO.Compression.DeflateStream stream = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Decompress);
            stream.Flush();

            int nSize = 100 * 1024 * 1024; //字符串不会超过100M
            try
            {
                byte[] decompressBuffer = new byte[nSize];
                int nSizeIncept = stream.Read(decompressBuffer, 0, nSize);
                stream.Close();
                GC.Collect();
                return System.Text.Encoding.Unicode.GetString(decompressBuffer, 0, nSizeIncept);//转换为普通的字符串
            }
            catch (Exception e)
            {
                stream.Close();
                GC.Collect();
                return null;
            }

        }
        #endregion
    }

    /// <summary>
    /// 字符串压缩和解压
    /// </summary>
    internal class ZipUtil
    {
        #region 压缩&&解压缩 -- 字符串
        /// <summary>压缩字符串</summary>
        /// <param name="value">要压缩的字符串</param>
        /// <returns>压缩后的字符串</returns>        
        public static string Zip(string value)
        {
            System.Text.Encoding encoding = System.Text.Encoding.Unicode;
            byte[] buffer = encoding.GetBytes(value);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.Compression.DeflateStream stream = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress, true);
            stream.Write(buffer, 0, buffer.Length);
            stream.Close();

            buffer = ms.ToArray();
            ms.Close();

            return Convert.ToBase64String(buffer); //将压缩后的byte[]转换为Base64String

        }

        /// <summary>解压缩字符串</summary>
        /// <param name="value">要解缩的字符串</param>
        /// <returns>解缩后的字符串</returns>
        public static string UnZip(string value)
        {
            byte[] buffer = Convert.FromBase64String(value);

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;
            System.IO.Compression.DeflateStream stream = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Decompress);
            stream.Flush();

            int nSize = 100 * 1024 * 1024; //字符串不会超过100M
            try
            {
                byte[] decompressBuffer = new byte[nSize];
                int nSizeIncept = stream.Read(decompressBuffer, 0, nSize);
                stream.Close();
                GC.Collect();
                return System.Text.Encoding.Unicode.GetString(decompressBuffer, 0, nSizeIncept);//转换为普通的字符串
            }
            catch (Exception e)
            {
                stream.Close();
                GC.Collect();
                return null;
            }

        }
        #endregion
    }
}
