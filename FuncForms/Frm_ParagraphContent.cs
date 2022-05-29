using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SparkFormEditor
{
    ///<summary>
    ///显示同一段落的内容，方便复制段落的全部文字
    ///</summary>
    internal partial class Frm_ParagraphContent : Form
    {
        //返回调用界面的设置参数：
        private string _Content = "";
        public string Content { get { return _Content; } }

        public Frm_ParagraphContent()
        {
            InitializeComponent();
        }

        public Frm_ParagraphContent(string contentPara, bool rightOk)
        {
            InitializeComponent();

            textBox1.LanguageOption = RichTextBoxLanguageOptions.AutoFont; //否则：字母字体会变成粗一点

            textBox1.Text = contentPara;

            SetRowLineSpacing();  //行间距

            this.buttonOK.Enabled = rightOk;
        }

        //特殊字符菜单加载显示
        public void LoadSelectWord(string helpStringPara)
        {
            //rtbeNew.SetSelectWords(GetSelectWord((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.HelpString)))); //右击菜单的插入文字
            textBox1.SetSelectWords(helpStringPara);
        }

        /// <summary>
        /// 输入法全半角
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_Enter(object sender, EventArgs e)
        {
            //解决Tab键切换，还是全角的问题 （都是在Enter中处理）
            if (this.textBox1.ImeMode != ImeMode.Hangul) //ImeMode.Hangul
            {
                this.textBox1.ImeMode = ImeMode.Hangul;
            }
        }

        /// <summary>
        /// 根据修改后的内容，再次调整段落列，写入到表单中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            _Content = this.textBox1.Text.Replace("\n", ""); //防止强行回车后，后面的内容不会重新调整

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        //字体太小看不清楚。Ctrl + 滚轮，还可以缩放
        #region 行间距
        private void SetRowLineSpacing()
        {
            //PFM_NUMBERING 成员 wNumbering 才起作用，项目符号，默认用PFN_BULLET
            //2 使用阿拉伯数字 (1, 2, 3, ...). 
            //3 使用小写字母 (a, b, c, ...). 
            //4 使用大写字母 (A, B, C, ...). 
            //5 使用小写罗马数字 (i, ii, iii, ...). 
            //6 使用大写罗马数字 (I, II, III, ...). 
            //7 自定义，字符见成员 wNumberingStart. 
            //PFM_OFFSET 成员 dxOffset 才起作用，缩进，单位twips
            //PFM_STARTINDENT 成员 dxStartIndent 才起作用，首行缩进
            //PFM_SPACEAFTER 成员 dySpaceAfter 才起作用，段间距
            //PFM_LINESPACING 成员 dyLineSpacing 才起作用，行间距

            Struct.PARAFORMAT2 fmt = new Struct.PARAFORMAT2();
            fmt.cbSize = Marshal.SizeOf(fmt);
            fmt.bLineSpacingRule = 1;  //几倍行距，取值0－5，具体含义看帮助
            fmt.dyLineSpacing = 30;//((int)richTextBox1.Font.Size) * 20 * ((int)ud.Value);
            fmt.dwMask = Consts.PFM_LINESPACING;
            Win32Api.SendMessage(new HandleRef(this.textBox1, this.textBox1.Handle), Consts.EM_SETPARAFORMAT, 0, ref fmt);
        }

        #endregion 行间距
        
    }
}
