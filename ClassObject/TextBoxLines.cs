using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SparkFormEditor
{
    /// <summary>
    /// 多行输入框中，关于光标所在行的相关特殊处理类
    /// </summary>
    internal class TextBoxLines
    {
        #region 调用示例
        //private void txtCmdInput_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Enter)
        //    {
        //        e.SuppressKeyPress = true;    / /回车事件已经处理完不再响应了
        //        string str = GetCmd(txtCmdInput);
        //        MoveCmdToLast(txtCmdInput, str);
        //    }
        //} 
        #endregion 调用示例

        /// <summary>
        /// 把所选中的命令移动到最下一行然后显示在文本框中
        /// </summary>
        /// <param name="txtCmdInput"></param>
        /// <param name="selCmd"></param>
        public static void MoveCmdToLast(TextBox txtCmdInput, string selCmd)
        {
            String txtStr = txtCmdInput.Text;
            int start = txtStr.IndexOf(selCmd);

            //把选中行或光标所在行的命令送到最后一行且光标移到第一行
            if (selCmd != null && selCmd.Length > 0 && selCmd != "\r\n")
            {
                string strLeft = txtStr.Remove(start, selCmd.Length);

                //处理剩下的字符串，注意把开头结尾的"\r\n"找到删掉

                while (strLeft != null && strLeft.Length > 0 && (strLeft[strLeft.Length - 1] == '\r' || strLeft[strLeft.Length - 1] == '\n'))
                {
                    strLeft = strLeft.Remove(strLeft.Length - 1, 1);
                }

                while (strLeft != null && strLeft.Length > 0 && strLeft[0] == '\r')
                {
                    strLeft = strLeft.Remove(0, 2);
                }

                //处理你取出的当前行的字符串若有"\r\n"注意把它去掉
                while (selCmd != "" && selCmd.Length > 0 &&
                       (selCmd[selCmd.Length - 1] == '\r'
                       || selCmd[selCmd.Length - 1] == '\n'))
                {
                    selCmd = selCmd.Remove(selCmd.Length - 1, 1);
                }

                string strNew = strLeft + "\r\n" + selCmd;
                //最后前面留一行空格且把鼠标定位到此
                txtCmdInput.Text = "\r\n" + strNew;
            }
            else
            {
                MessageBox.Show("请您不要发送空命令，谢谢合作！", "温馨提示：");
            }
        }

        /// <summary>
        /// 取控件里鼠标所在行或光标所选择的命令，发送的命令暂时写到文件中
        /// </summary>
        /// <param name="txtCmdInput"></param>
        /// <returns></returns>
        public static string GetCmd(TextBox txtCmdInput)
        {
            string txtStr = txtCmdInput.Text;
            string selStr = txtCmdInput.SelectedText;
            string selCmd = null;
            int start = 0;
            if (selStr != null && selStr.Length > 0)
            {

                int selBegin = txtStr.IndexOf(selStr);
                int selEnd = selBegin + selStr.Length - 1;
                string subPreStr = txtStr.Substring(0, selBegin);
                string subLastStr = txtStr.Substring(selEnd + 1);
                string preleft = null;
                string lastleft = null;
                if (subPreStr.Length > 0 && subPreStr[subPreStr.Length - 1] != '\n')
                {
                    int nindex = subPreStr.LastIndexOf("\n");
                    preleft = subPreStr.Substring(nindex + 1);
                }
                if (subLastStr.Length > 0 && subLastStr[0] != '\r')
                {
                    int rindex = subLastStr.IndexOf("\r");
                    if (rindex != -1)
                    {
                        lastleft = subLastStr.Substring(0, rindex + 2);
                    }
                    else lastleft = null;
                }
                else if (subLastStr != null && subLastStr.Length > 0 && subLastStr[0] == '\r')
                {
                    lastleft = "\r\n";
                }
                selStr = preleft + selStr + lastleft;
                start = txtStr.IndexOf(selStr);
                selCmd = selStr;
            }
            else
            {
                //取光标所在行的字符串包括末尾的换行回车符"\r\n"
                //string strCmdText = txtCmdInput.Text;
                int curInx = txtCmdInput.SelectionStart;       //光标所在位置索引
                string tmp = txtStr.Substring(0, curInx);  //开始到光标处的子串
                int n = tmp.LastIndexOf('\n');             //找光标所在行的开头索引start + 1
                start = n + 1;
                tmp = txtStr.Substring(curInx);//当前光标所在位置到最后的子串
                int end = tmp.IndexOf('\n'); //找该行的末尾索引包括"\r\n"
                string curRowText = null;
                if (end > 0)
                {
                    curRowText = txtStr.Substring(start, curInx - start + end + 1);
                }
                else
                {
                    curRowText = txtStr.Substring(start);
                }
                selCmd = curRowText;
            }

            ////MoveCmdToLast(txtStr,selCmd);
            ////把光标所在行的命令写入文件中
            //FileStream fs = new FileStream("D:\\file.txt", FileMode.Append, FileAccess.Write);
            //StreamWriter sw = new StreamWriter(fs);
            //sw.Flush();
            //sw.Write(selCmd);
            //sw.Flush();
            //sw.Close();

            return selCmd;
        }

        /// <summary>
        /// 选中光标所在行
        /// </summary>
        /// <param name="rTxtMenu"></param>
        /// <param name="lineStr"></param>
        public static void SelectCurrentLine(TextBox rTxtMenu, string lineStr)
        {
            //rTxtMenu.SelectionBackColor = Color.Red;
            //取得光标当前位置
            int currentPos = rTxtMenu.SelectionStart;

            //取得当前行号
            int row = rTxtMenu.GetLineFromCharIndex(currentPos);
            //取得全部文本
            //rTxtMenu.SelectAll();
            //string allStr = rTxtMenu.SelectedText;
            string allStr = rTxtMenu.Text;

            //替换间隔符
            string tempStr = allStr.Replace("\r\n", "¤");//Textbox是\r\n ,RichTextBox是\n
            //tempStr = tempStr.Replace("\t", "");
            //tempStr = tempStr.Replace("\n", "");
            //取得当前行
            string[] strArr = tempStr.Split("¤".ToCharArray()); //"\n".ToCharArray()
            //int flag = 0;
            //string resultStr = "";
            //foreach (string str in strArr)
            //{
            //    if (flag == row) { resultStr = str; break; }
            //    flag++;
            //}

            //if (row < rTxtMenu.Lines.Length)
            //{
            //    resultStr = rTxtMenu.Lines[row];
            //}

            //lineStr = rTxtMenu.Lines[row];


            //确定有效起始位置 
            if (row < rTxtMenu.Lines.Length - 1)
            {
                //rTxtMenu.GetFirstCharIndexOfCurrentLine
                currentPos = allStr.IndexOf("\r\n" + lineStr + "\r\n"); //第一个符合当前行文字的行，可能会错乱，如果其他行包含了当前行的内容
            }
            else
            {
                currentPos = allStr.IndexOf("\r\n" + lineStr);//最后一行，没有回行
            }

            //byte[] lineStrByte = Encoding.Default.GetBytes(lineStr);//半角1，全角汉字2

            if (currentPos < 0)
            {
                currentPos = 0;
            }

            if (row == 0)
            {
                //currentPos++;
            }
            else
            {
                currentPos++;
                currentPos++;
            }

            rTxtMenu.Select(currentPos, lineStr.Length);

        }
    }

}
