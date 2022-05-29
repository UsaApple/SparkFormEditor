using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparkFormEditor
{
    public interface ISealedBook
    {

        /// <summary>
        /// 获取文书的当前总页数
        /// </summary>
        int PagesCount
        {
            get;
        }

        /// <summary>
        /// 防止覆盖，排他更新用的序列号
        /// </summary>
        int SequenceNo
        {
            get;
            set;
        }

        /// <summary>
        /// 上一页、下一页、到首页、到尾页的操作
        /// </summary>
        void BookGoTo(string strDirection);
        /// <summary>
        /// 撤销
        /// </summary>
        void BookUndo();
        /// <summary>
        /// 重复
        /// </summary>
        void BookRedo();
        /// <summary>
        /// 复制
        /// </summary>
        void BookCopy();
        /// <summary>
        /// 粘贴
        /// </summary>
        void BookPaste();
        /// <summary>
        /// 查对，返回是否符合的结果
        /// </summary>
        bool BookCheckAndVerify(string strSomeParameters);
        /// <summary>
        /// 签名
        /// </summary>
        void BookSign();
        /// <summary>
        /// 设置字体颜色
        /// </summary>
        void BookFontColor(string strColor);
        /// <summary>
        /// 打印
        /// </summary>
        void BookPrint(string strDirection);

        /// <summary>
        /// 打印
        /// </summary>
        void BookPrintAll(string strDirection);

        /// <summary>
        /// 保存
        /// </summary>
        int BookSave();

        /// <summary>
        /// 新建页
        /// </summary>
        int BookNewPage();

        /// <summary>
        /// 另存为 本地文件
        /// </summary>
        void BookSaveAs();

        /// <summary>
        /// 添加段落
        /// </summary>
        void BookAddParagraph(string strTemplate);

        /// <summary>
        /// 输入助理输入
        /// </summary>
        void BookAssistantEntry(string strContent);

        /// <summary>
        /// 判断文书是否已经修改，需要提示保存
        /// </summary>
        bool BookNeedSave
        {
            get;
            //set;
        }

        //---------------------------------下面两个现在用不到，以后用到在看---------------
        /// <summary>
        /// 已做了修改，需要提示保存
        /// </summary>
        event EventHandler BookIsDirty;

        /// <summary>
        /// 保存准备完毕，传出数据保存到服务端数据库
        /// </summary>
        event EventHandler BookCanSaveNow;
    }
}
