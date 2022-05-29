using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data;

namespace SparkFormEditor
{
    /// <summary>
    /// CSV文件生成类
    /// </summary>
    internal class CSVHelp
    {
        /// <summary>
        /// 将DataGridView，导出为CSV文件
        /// 1.表单病人列表导出的csv才是真正的csv文件，是以逗号,分割的。
        /// 2.这个方法导出的csv也可以用记事本打开，但是是以【\t】即：“	”分割的。如果后缀改成csv打开的话，都合并到一列了。
        /// 所以后缀名还只能是xls
        /// </summary>
        /// <param name="dGV"></param>
        /// <param name="pFilePath"></param>
        public void ToCsV(DataGridView dGV, string pFilePath)
        {
            string stOutput = "";
            // Export titles:
            string sHeaders = "";

            //表格头部
            for (int j = 0; j < dGV.Columns.Count; j++)
            {
                if (dGV.Columns[j].Visible)
                {
                    sHeaders = sHeaders.ToString() + Convert.ToString(dGV.Columns[j].HeaderText) + "\t";
                }
            }

            stOutput += sHeaders + "\r\n";
            
            //遍历，每行每列进行拼接
            for (int i = 0; i < dGV.RowCount; i++)
            {
                if (dGV.Rows[i].Visible && !dGV.Rows[i].IsNewRow)
                {
                    string stLine = "";
                    for (int j = 0; j < dGV.Rows[i].Cells.Count; j++)
                    {
                        if (dGV.Columns[j].Visible)
                        {
                            stLine = stLine.ToString() + Convert.ToString(dGV.Rows[i].Cells[j].Value) + "\t";
                        }
                    }

                    stOutput += stLine + "\r\n";
                }
            }

            //文件设置
            Encoding utf16 = Encoding.GetEncoding("GB2312");
            byte[] output = utf16.GetBytes(stOutput);
            FileStream fs = new FileStream(pFilePath, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(output, 0, output.Length); //write the encoded file
            bw.Flush();
            bw.Close();
            fs.Close();
           
        }

        public void DataTableToCsV(DataTable dt, string pFilePath)
        {
            string stOutput = "";
            // Export titles:
            string sHeaders = "";

            //表格头部
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                sHeaders = sHeaders.ToString() + Convert.ToString(dt.Columns[j].ColumnName) + "\t";
            }

            stOutput += sHeaders + "\r\n";

            //遍历，每行每列进行拼接
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                string stLine = "";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    stLine = stLine.ToString() + Convert.ToString(dt.Rows[i][j].ToString()) + "\t";
                }

                stOutput += stLine + "\r\n";

            }

            //文件设置
            Encoding utf16 = Encoding.GetEncoding("GB2312");
            byte[] output = utf16.GetBytes(stOutput);
            FileStream fs = new FileStream(pFilePath, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(output, 0, output.Length); //write the encoded file
            bw.Flush();
            bw.Close();
            fs.Close();

        }

    }
}
