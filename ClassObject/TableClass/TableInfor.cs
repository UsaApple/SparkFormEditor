using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SparkFormEditor
{
    ///<summary>
    ///表格对象类
    ///</summary>
    internal class TableInfor : IDisposable
    {
        public int ColumnsCount = 0;        //不分栏的情况下的行数和列数
        public int RowsCount = 0;           //行数(每页一样的表格的行数，或者是不一样行数的时候，当前页的表格行数)
        public int RowsCountFirstPage = 0;  //第一页行数（有些表格，第一页的行数少一些，因为要写一些备注）
        public int RowsCountSecondPage = 0;  //第一页行数（有些表格，第一页的行数少一些，因为要写一些备注）

        public int PageRowsHeight = 0;      //行高
        public int PageRowsWidth = 0;       //行宽

        public CellInfor[,] Cells;

        public ColumnInfor[] Columns;

        public RowInfor[] Rows;

        public Point Location;

        public Point RtbeDiff;               //cellDiffLoation单元格与上、左、右的边界定义

        public int GroupColumnNum = 1;             //表格的分栏，一般为1，但是也有为2的，就像word的分栏，左右对称；在列少哦情况下，两页合并到了一起。

        public CellInfor CurrentCell = null; //当前单元格


        public int MaxId = 0;

        public bool Vertical = false;        //横向表格还是纵向表格

        //SumGroup="入量列@实入¤途径列@途径¤出量列@出量1,出量2"
        public string SumGroup = "";         //总结小结，配置参数，如果不要配置，那么就是空。
        public string SumGroupExtend = "";   //分组合计的扩展             //SumGroupExtend="全天总结§{0}全天总结§-07:00§06:59¤白班小结§{0}白班小结§07:00§19:59¤夜班小结§{0}夜班小结§-19:00§06:59¤临时小结§{1}小时小结§-07:00§06:59"

        //构造方法：行列和所有单元格
        public TableInfor(int rows, int cols)
        {
            RowsCount = rows;
            ColumnsCount = cols;

            Cells = new CellInfor[rows, cols];  //int[,] b = new int[8, 8]; 

            Columns = new ColumnInfor[cols];

            Rows = new RowInfor[rows];
        }

        /// <summary>
        /// 获取表格的行递增序号
        /// </summary>
        /// <returns></returns>
        public int AddRowGetMaxID()
        {
            int num = this.MaxId + 1;
            this.MaxId = num;
            return num;
            //return ++MaxId; //表格xml中每条数据的唯一号
        }

        /// <summary>
        /// 根据行号和列名获取单元格信息
        /// </summary>
        /// <param name="row">行号</param>
        /// <param name="colName">列名</param>
        /// <returns>单元格</returns>
        public CellInfor CellByRowNo_ColName(int row, string colName)
        {
            CellInfor retCell = null;

            int col = -1;

            for (int i = 0; i < Columns.Length; i++)
            {
                if (Columns[i].ColName == colName)
                {
                    col = i;
                    break;
                }
            }

            if (col != -1)
            {
                retCell = Cells[row, col];
            }
            else
            {
                retCell = null;
            }

            return retCell;
        }

        /// <summary>
        /// 表格中是否存在制定列明的列
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public bool ContainColName(string colName)
        {
            bool ret = false;

            for (int i = 0; i < Columns.Length; i++)
            {
                if (Columns[i].ColName == colName)
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        //释放表格资源，否则在打印所有页和生成区域图片的时候可能会报错。
        public void Dispose()
        {
            //Dispose(true);
            //GC.SuppressFinalize(true);
            //GC.SuppressFinalize(this);

            if (Columns != null)
            {
                for (int i = 0; i < Columns.Length; i++)
                {
                    //if (Columns[i].RTBE != null && !Columns[i].RTBE.IsDisposed)
                    //{
                    Columns[i].RTBE.Dispose();
                    Columns[i] = null;
                    //}
                    //Columns[i].RTBE.Dispose(true);
                }

                //for (int i = Columns.Length - 1; i >= 0; i--)
                //{
                //    if (Columns[i].RTBE != null && !Columns[i].RTBE.IsDisposed)
                //    {
                //        Columns[i].RTBE.Dispose();
                //    }

                //    Columns[i] = null;
                //}
            }

            Cells = null;
            Columns = null;
            Rows = null;

            CurrentCell = null;

            //this = null;

            //GC.SuppressFinalize(this);
        }

        ////是否回收完毕 
        //bool _disposed; 
        //public void Dispose() 
        //{ 
        //    Dispose(true); 
        //    GC.SuppressFinalize(this); 
        //} 

        ////~DisposableClass() 
        ////{ 
        ////Dispose(false); 
        ////} 
        ////这里的参数表示示是否需要释放那些实现IDisposable接口的托管对象 
        //protected virtual void Dispose(bool disposing) 
        //{ 
        //if(_disposed) return; //如果已经被回收，就中断执行 
        //if(disposing) 
        //{ 
        ////TODO:释放那些实现IDisposable接口的托管对象 
        //} 
        ////TODO:释放非托管资源，设置对象为null 
        //_disposed = true; 
        //} 

    }
}
