using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Data;

namespace SparkFormEditor
{
    ///<summary>
    ///小结/总结 类型分组合计处理类
    ///</summary>
    internal class SumGroup
    {
        //如果是分组小结，会过滤掉途径为空的数据。不要分组，那么途径就是空，这里根据途径类型合计的时候，会过滤掉途径为空的数据
        //string _Mode = "";   //Comm.getPropertyByName(_TableInfor.SumGroup, "模式");  //默认是分组的，如果是：模式@不分组，那么所有的分组行会合成一行进行小结和总结（固定文字“小结”，“总结”和合计值）

        /// <summary>
        /// 按照结构初始化表格，一遍处理合计
        /// </summary>
        /// <returns></returns>
        public DataTable InitDataTable(string[] outPutColumns)
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[] 
                    { new DataColumn("类型", typeof(string)),
                      new DataColumn("排序", typeof(string)),
                      new DataColumn("途径", typeof(string)),
                      new DataColumn("入量", typeof(double))      //入量要分组计算
                            });    //出量不要分组计算new DataColumn("出量", typeof(double))}

            if (outPutColumns != null && outPutColumns.Length > 0)
            {
                for (int i = 0; i < outPutColumns.Length; i++)
                {
                    if (!string.IsNullOrEmpty(outPutColumns[i]))
                    {
                        dt.Columns.Add(new DataColumn(outPutColumns[i], typeof(double)));
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="arr"></param>
        public void AddData(DataTable dt, object[] arr)
        {
            //dt.Rows.Add(new object[] { "输入", "B", "im", 10 });

            DataRow drNew = dt.NewRow();
            drNew.ItemArray = arr;
            dt.Rows.InsertAt(drNew, 0);
            
        }

        private bool NeedSort(DataTable dt)
        {
            bool ret = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["排序"] != null && !string.IsNullOrEmpty(dt.Rows[i]["排序"].ToString()))
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        //类型、途径、数值
        //输入、im、100
        //输入、iv、100

        /// <summary>
        /// 总结小结的方法
        /// </summary>
        /// <param name="dt">数据</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public DataTable SumByGroup(DataTable dt, ref string msg)
        {
            try
            {
                //DataTable dt = new DataTable();
                //dt.Columns.AddRange(new DataColumn[] 
                //    { new DataColumn("类型", typeof(string)),
                //      new DataColumn("排序", typeof(string)),
                //      new DataColumn("途径", typeof(string)),
                //      new DataColumn("入量", typeof(double))      //入量要分组计算
                //            });    //出量不要分组计算new DataColumn("出量", typeof(double))}

                //dt.Rows.Add(new object[] { "输入", "B", "im", 10 });
                //dt.Rows.Add(new object[] { "冲入", "A", "冲入", 20 });
                //dt.Rows.Add(new object[] { "输入", "B", "im", 30 });
                //dt.Rows.Add(new object[] { "输入", "B", "im", 40 });
                //dt.Rows.Add(new object[] { "冲入", "A", "洗胃", 80 });
                //dt.Rows.Add(new object[] { "", "A", "洗胃", 80 });

                if(dt.Rows.Count == 0)
                {
                    //没有原始数据，无需统计，而且下面统计也会报错
                    return dt;
                }


                //空类型的过滤，并且提示：
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    if (dt.Rows[i]["类型"] == null || string.IsNullOrEmpty(dt.Rows[i]["类型"].ToString()))
                    {
                        //有的一个时间点的记录，可能没有填写出入量。但是如有如有数值，则必须有途径
                        if (dt.Rows[i]["入量"] != null && !string.IsNullOrEmpty(dt.Rows[i]["入量"].ToString()))
                        {
                            Comm.LogHelp.WriteInformation("小结/总结的时候，遇到无法识别的途径（空）：" + dt.Rows[i]["途径"].ToString());

                            if (string.IsNullOrEmpty(msg))
                            {
                                msg = dt.Rows[i]["途径"].ToString();
                            }
                            else
                            {
                                msg += "、" + dt.Rows[i]["途径"].ToString();
                            }

                            //dt.Rows.Remove(dt.Rows[i]);
                        }
                    }
                }


                //排序
                //如果都为空，那么不排序。否则打乱顺序的
                if (NeedSort(dt))
                {
                    dt.DefaultView.Sort = string.Format("排序 ASC");
                    dt = dt.DefaultView.ToTable();
                }

                DataTable dtResult = dt.Clone();
                DataTable dtTypes = dt.DefaultView.ToTable(true, "类型"); //根据列：类型，重复的取唯一行  //ToTable(bool distinct, params string[] columnNames); 途径空的还会保留的
                DataRow[] rows;
                DataTable temp;
                DataRow dr;

                for (int i = 0; i < dtTypes.Rows.Count; i++)
                {
                    //过滤掉，可能不存在分类类型的，写错的途径。这里就排除了空途径的行
                    if (dtTypes.Rows[i]["类型"] != null && !string.IsNullOrEmpty(dtTypes.Rows[i]["类型"].ToString()))
                    {
                        rows = dt.Select("类型='" + dtTypes.Rows[i]["类型"] + "'"); //rows = dt.Select("类型='" + dtTypes.Rows[i][0] + "' and 数量='" + dtTypes.Rows[i][1] + "'");
                        //temp用来存储筛选出来的数据
                        temp = dtResult.Clone();

                        foreach (DataRow row in rows)
                        {
                            temp.Rows.Add(row.ItemArray);
                        }

                        dr = dtResult.NewRow();
                        dr["类型"] = dtTypes.Rows[i][0].ToString();
                        dr["入量"] = temp.Compute("sum(入量)", "");  //汇总对应类型的合计值
                        dtResult.Rows.Add(dr);
                    }
                }

                ////出量汇总
                //outPutSum = dt.Compute("sum(出量)", "").ToString();  //.TrimEnd('0').TrimEnd('.')
                if (dtResult.Columns.Count > 4)
                {
                    if(dtResult.Rows.Count == 0) //如果传入的数据行数不为0，之前没有入量的，这里出量需要增加行
                    {
                        dtResult.Rows.Add();
                    }

                    for (int i = 4; i < dtResult.Columns.Count; i++)
                    {
                        dtResult.Rows[0][i] = dt.Compute("sum(" + dt.Columns[i].ColumnName + ")", "").ToString();

                        ////出量如果为零，那么现实为空
                        //if (dtResult.Rows[0][i] != null && dtResult.Rows[0][i].ToString() == "0")
                        //{
                        //    dtResult.Rows[0][i] = "";
                        //}
                    }
                }

                return dtResult;
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }
    }

}
