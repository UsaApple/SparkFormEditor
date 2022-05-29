using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Xml;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Net;
using System.Xml.XPath;
using SparkFormEditor.Model;
using System.Threading.Tasks;
using SparkFormEditor.Foundation;

namespace SparkFormEditor
{
    public partial class SparkFormEditor
    {
        Rectangle _LastDrawRectangle = Rectangle.Empty;

        # region 绘制表格的基础线
        /// <summary>
        /// 绘制表格线（表格的基础线，不包含自定义线和表格数据）
        /// </summary>
        private void DrawTableBaseLines(Graphics g, TableInfor tableInforPara)
        {
            Pen pen = new Pen(Color.Black, 1);

            int fieldIndex = 0;

            for (int field = 0; field < tableInforPara.GroupColumnNum; field++)
            {
                fieldIndex = tableInforPara.RowsCount * field;

                if (!tableInforPara.Vertical)
                {
                    //表格右边竖线
                    g.DrawLine(pen, new Point(tableInforPara.Location.X + tableInforPara.PageRowsWidth * (field + 1), tableInforPara.Location.Y), new Point(tableInforPara.Location.X + tableInforPara.PageRowsWidth * (field + 1), tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.RowsCount));

                    //最下面的横线
                    g.DrawLine(pen, new Point(tableInforPara.Location.X + tableInforPara.PageRowsWidth * field, tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.RowsCount), new Point(tableInforPara.Location.X + tableInforPara.PageRowsWidth * field + tableInforPara.PageRowsWidth, tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.RowsCount));
                }
                else
                {
                    //表格右边竖线
                    g.DrawLine(pen, new Point(tableInforPara.Location.X + tableInforPara.Cells[0, 0].CellSize.Width * tableInforPara.RowsCount, tableInforPara.Location.Y),
                        new Point(tableInforPara.Location.X + tableInforPara.Cells[0, 0].CellSize.Width * tableInforPara.RowsCount, tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.ColumnsCount));

                    //最下面的横线
                    g.DrawLine(pen, new Point(tableInforPara.Location.X, tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.ColumnsCount),
                        new Point(tableInforPara.Location.X + tableInforPara.Cells[0, 0].CellSize.Width * tableInforPara.RowsCount, tableInforPara.Location.Y + tableInforPara.PageRowsHeight * tableInforPara.ColumnsCount));
                }


                for (int i = 0; i < tableInforPara.RowsCount; i++)
                {
                    for (int j = 0; j < tableInforPara.ColumnsCount; j++)
                    {
                        //没有隐藏下边线，也不是合并单元格的时候
                        if (!tableInforPara.Cells[i + fieldIndex, j].IsMergedNotShow && !tableInforPara.Cells[i + fieldIndex, j].NotTopmLine)
                        {
                            //如果不是被合并的单元格，也不是隐藏上边先的，那么：上线和左先都要绘制
                            DrawCellLineTopAndBottom(i + fieldIndex, j, g, tableInforPara);
                        }
                        else
                        {
                            if (tableInforPara.Cells[i + fieldIndex, j].NotTopmLine)
                            {
                                DrawCellLeftLine(i + fieldIndex, j, g, tableInforPara);
                            }
                            else if (tableInforPara.Cells[i + fieldIndex, j].IsMergedNotShow)
                            {
                                //判断是左右合并，还是上下合并
                                if (tableInforPara.Cells[i + fieldIndex, j].MergedRight)
                                {
                                    DrawCellTopLine(i + fieldIndex, j, g, tableInforPara);
                                }
                                else
                                {
                                    DrawCellLeftLine(i + fieldIndex, j, g, tableInforPara);
                                }
                            }
                        }
                    }

                    ////提示合计行：行头提示
                    //DrawSumRowAttention(tableInforPara, i, g);

                }
            }
        }
        # endregion 绘制表格的基础线

        # region 重绘方法:全局刷新
        /// <summary>
        /// 重新绘制：在删除添加行的时候
        /// </summary>
        private void Redraw()
        {

            //_IsLoading = true;
            //_IsNotNeedCellsUpdate = true;
            //Application.DoEvents();
            //Application.DoEvents(); //防止过快，导致出错


            //先绘制底图 
            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);

            if (_BmpBack != null)
            {
                pictureBoxBackGround.Image = (Bitmap)_BmpBack.Clone(); //重新绘制所有，所以将背景重置为只有固定背景的备份

                g = Graphics.FromImage(pictureBoxBackGround.Image);//上面图片重新绑定了，需要再次取得其GDi对象
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (_TableType && _TableInfor != null) // 根据解析本页的数据，形成本页的表格，进行绘制。这里还是默认的表格线。
                {
                    //绘制自己定义的表格的所有边框线：难点在于行、单元个的编辑后的线样式，数据保存到xml后再解析呈现
                    DrawTableBaseLines(g, _TableInfor);
                }
            }
            else
            {
                this.DrawEditorChart(g, false, false, false);
            }


            DrawTableData_Lines(g, false, _TableInfor, _CurrentPage); //绘制表格数据和自定义线

            pictureBoxBackGround.Refresh();

            g.Dispose();

            //_IsLoading = false;
            //_IsNotNeedCellsUpdate = false;
        }

        /// <summary>
        /// 刷新行：（目前这个方法是行文字变色的时候用，普通的行刷新没有用到）
        /// 普通的行刷新不用再多处理以下：
        /// 1.行文字颜色改变的时候（设置：已经是富文本的单元格的 颜色也要强行修改成行文字颜色）
        /// 2.表格对象tableInforPara也要更新rtf，否则光标在点如，还是老的颜色
        /// 
        /// 行刷新还存在的问题：如果是纵向合并的，那么刷新范围还是本身的一行，那么下面合并的就还是老的样式，没有刷新到。
        /// </summary>
        /// <param name="tableInforPara"></param>
        /// <param name="row"></param>
        /// <param name="isSetRowForeColor">普通的行刷新为False，行文字变色是为True</param>
        /// <param name="cellNode">当为行文字变得时候，如果是富文本还需要更新数据</param>
        private void RedrawRow(TableInfor tableInforPara, int row, bool isSetRowForeColor, XmlNode cellNode)
        {

            //先绘制底图
            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Point orgLocation;
            if (_TableType && tableInforPara != null)
            {
                _isCreatImages = true;

                int fieldIndex = 0;

                HorizontalAlignment temp;
                string showText = "";
                SolidBrush sBrrsh = new SolidBrush(Color.Red); //绘制修订的提示

                string value = string.Empty;
                string rowInforPara = string.Empty;

                for (int field = 0; field < tableInforPara.GroupColumnNum; field++)
                {
                    fieldIndex = tableInforPara.RowsCount * field;

                    for (int i = 0; i < tableInforPara.RowsCount; i++)
                    {
                        if (row == i)
                        {
                            //需要刷新的行
                        }
                        else
                        {
                            //不需要打印的
                            continue;
                        }

                        for (int j = 0; j < tableInforPara.ColumnsCount; j++)
                        {
                            try
                            {
                                if (j == _CurrentCellIndex.Y)
                                {
                                    continue; //本身所在的单元格（时间列）不用重绘，不然会有重影
                                }


                                //如果上一行，存在上下标等字体，会导致字体变小，产生异常。
                                //u静点,遵医嘱留置胃管，【上下标】的文字是第一个文字，就会出现，这种情况
                                tableInforPara.Columns[j].RTBE.ResetDefault();
                                tableInforPara.Columns[j].RTBE.SelectionFont = tableInforPara.Columns[j].RTBE.Font;
                                //如果上一行，存在上下标等字体，会导致字体变小，产生异常。

                                //纵向合并的,是多行的，如果先赋值，再设置多行，初始化显示只能显示第一行
                                if (!tableInforPara.Columns[j].RTBE.Multiline && tableInforPara.Cells[i + fieldIndex, j].IsMerged() && tableInforPara.Cells[i + fieldIndex, j].ColSpan == 0)
                                {
                                    tableInforPara.Columns[j].RTBE.Multiline = true;
                                }

                                //绘制单元格内容：优先取Rtf的值
                                if (tableInforPara.Cells[i + fieldIndex, j].Rtf != "" && tableInforPara.Cells[i + fieldIndex, j].IsRtf_OR_Text)
                                {
                                    tableInforPara.Columns[j].RTBE.Rtf = tableInforPara.Cells[i + fieldIndex, j].Rtf;

                                    //如果是行文字变色设置的时候，即使是富文本的单元格，也要整体变色
                                    if (isSetRowForeColor)
                                    {
                                        //即使是富文本，在修改行颜色的时候，整行文字颜色自动修改。如果是纵向合并的单元格，会局部不刷新的问题
                                        if (!string.IsNullOrEmpty(tableInforPara.Rows[i + fieldIndex].RowForeColor))
                                        {
                                            tableInforPara.Columns[j].RTBE.ForeColor = Color.FromName(tableInforPara.Rows[i + fieldIndex].RowForeColor);
                                            tableInforPara.Columns[j].RTBE.SelectAll();
                                            tableInforPara.Columns[j].RTBE.SelectionColor = tableInforPara.Columns[j].RTBE.ForeColor;

                                            tableInforPara.Cells[row, j].Rtf = tableInforPara.Columns[j].RTBE.Rtf; //否则，如果不切换重新载入本页，再次进入单元格的话还是原来的颜色
                                        }

                                        //并要保存到xml的Record中，否则下次打开还是原来的rtf，
                                        if (cellNode != null)
                                        {
                                            tableInforPara.Columns[j].RTBE._IsRtf = true;
                                            value = GetCellSaveValue(tableInforPara.Columns[j].RTBE, tableInforPara.Cells[row, j], ref rowInforPara);
                                            (cellNode as XmlElement).SetAttribute(tableInforPara.Columns[j].RTBE.Name, value); //单元格信息
                                        }
                                    }
                                }
                                else
                                {
                                    tableInforPara.Columns[j].RTBE.Text = tableInforPara.Cells[i + fieldIndex, j].Text;

                                    //整行文字颜色自动修改
                                    if (!string.IsNullOrEmpty(tableInforPara.Rows[i + fieldIndex].RowForeColor))
                                    {
                                        tableInforPara.Columns[j].RTBE.ForeColor = Color.FromName(tableInforPara.Rows[i + fieldIndex].RowForeColor);
                                        tableInforPara.Columns[j].RTBE.SelectAll();
                                        tableInforPara.Columns[j].RTBE.SelectionColor = tableInforPara.Columns[j].RTBE.ForeColor;

                                    }
                                    else
                                    {
                                        tableInforPara.Columns[j].RTBE.ForeColor = tableInforPara.Columns[j].RTBE.DefaultForeColor;
                                        tableInforPara.Columns[j].RTBE.SelectAll();
                                        tableInforPara.Columns[j].RTBE.SelectionColor = tableInforPara.Columns[j].RTBE.ForeColor;
                                    }

                                    tableInforPara.Columns[j].RTBE.SetAlignment();
                                }

                                //if (tableInforPara.Columns[j].RTBE.Name == "日期")
                                if (tableInforPara.Columns[j].RTBE.Name.StartsWith("日期"))
                                {
                                    tableInforPara.Columns[j].RTBE._YMD = tableInforPara.Cells[i + fieldIndex, j].DateFormat;
                                    showText = tableInforPara.Columns[j].RTBE.GetShowDate();
                                    if (tableInforPara.Columns[j].RTBE.Text != showText)
                                    {
                                        temp = tableInforPara.Columns[j].RTBE.SelectionAlignment;

                                        tableInforPara.Columns[j].RTBE.Text = showText;

                                        tableInforPara.Columns[j].RTBE.SelectionAlignment = temp;
                                    }
                                }

                                tableInforPara.Columns[j].RTBE.Location = tableInforPara.Cells[i + fieldIndex, j].RtbeLocation();
                                tableInforPara.Columns[j].RTBE._HaveSingleLine = tableInforPara.Cells[i + fieldIndex, j].SingleLine;
                                tableInforPara.Columns[j].RTBE._HaveDoubleLine = tableInforPara.Cells[i + fieldIndex, j].DoubleLine;

                                //绘制行和单元格的修改后的边框线样式：
                                //单元格线： 如果为空"",那么认为是白线，就会挡住
                                if (tableInforPara.Cells[i + fieldIndex, j].CellLineColor != Color.Black)
                                {
                                    if (!tableInforPara.Cells[i + fieldIndex, j].IsMergedNotShow)
                                    {
                                        DrawCellLine(i + fieldIndex, j, tableInforPara.Cells[i + fieldIndex, j].CellLineColor, g, tableInforPara);

                                        if (tableInforPara.Cells[i + fieldIndex, j].NotTopmLine)
                                        {
                                            HideCellBottomLine(i + fieldIndex, j, g, tableInforPara);
                                        }
                                    }
                                }

                                //上面先绘制线，再绘制输入框内容
                                if (!tableInforPara.Cells[i + fieldIndex, j].IsMergedNotShow)
                                {
                                    //可能是合并的单元格，所以用单元格的宽度来作为输入框大小
                                    if (tableInforPara.Cells[i + fieldIndex, j].IsMerged())
                                    {
                                        //（合并单元格的时候），输入框的大小变成单元格大小-边距后，再打印
                                        tableInforPara.Columns[j].RTBE.Size = new Size(tableInforPara.Cells[i + fieldIndex, j].CellSize.Width - tableInforPara.RtbeDiff.X * 2, tableInforPara.Cells[i + fieldIndex, j].CellSize.Height - tableInforPara.RtbeDiff.Y * 2);

                                        //如果是纵向合并的单元格，会变成默认的总是居左                
                                        if (!tableInforPara.Columns[j].RTBE.Multiline && tableInforPara.Cells[i + fieldIndex, j].ColSpan == 0)
                                        {
                                            //纵向合并的
                                            tableInforPara.Columns[j].RTBE.Multiline = true;
                                            tableInforPara.Columns[j].RTBE.Is_RowSpanMutiline = true;
                                        }
                                    }
                                    else
                                    {
                                        //重置输入框的大小，为：没有合并的标准大小
                                        tableInforPara.Columns[j].RTBE.ResetSize();
                                    }

                                    orgLocation = new Point(tableInforPara.Cells[i + fieldIndex, j].Loaction.X + tableInforPara.RtbeDiff.X, tableInforPara.Cells[i + fieldIndex, j].Loaction.Y + tableInforPara.RtbeDiff.Y);

                                    //打印单元格文字内容
                                    printRtbeExtend(tableInforPara.Columns[j].RTBE, g, orgLocation);

                                    if (!_isCreatImages)
                                    {
                                        _isCreatImages = true;
                                    }
                                }

                                //最后绘制对角线，防止对角线被输入框挡住
                                if (tableInforPara.Cells[i + fieldIndex, j].DiagonalLineZ)
                                {
                                    DrawLineZN(i + fieldIndex, j, g, "z", tableInforPara);
                                }
                                else if (tableInforPara.Cells[i + fieldIndex, j].DiagonalLineN)
                                {
                                    DrawLineZN(i + fieldIndex, j, g, "n", tableInforPara);
                                }

                                //if (tableInforPara.Cells[i + fieldIndex, j].EditHistory != "")
                                //{
                                //    //绘制修订的提示：
                                //    //g.FillPolygon(sBrrsh, new Point[]{new Point(220,10),new Point(300,10),new Point(230,100)});
                                //    g.FillPolygon(sBrrsh, new Point[] { new Point(tableInforPara.Cells[i + fieldIndex, j].Loaction.X + _TableInfor.Cells[i + fieldIndex, j].CellSize.Width, _TableInfor.Cells[i + fieldIndex, j].Loaction.Y + _TableInfor.Cells[i + fieldIndex, j].CellSize.Height - locDiff), 
                                //    new Point(_TableInfor.Cells[i + fieldIndex, j].Loaction.X + _TableInfor.Cells[i + fieldIndex, j].CellSize.Width - locDiff, _TableInfor.Cells[i + fieldIndex, j].Loaction.Y + _TableInfor.Cells[i + fieldIndex, j].CellSize.Height),
                                //    new Point(_TableInfor.Cells[i + fieldIndex, j].Loaction.X + _TableInfor.Cells[i + fieldIndex, j].CellSize.Width, _TableInfor.Cells[i + fieldIndex, j].Loaction.Y + _TableInfor.Cells[i + fieldIndex, j].CellSize.Height) });
                                //}

                            }
                            catch (Exception ex)
                            {
                                Comm.LogHelp.WriteErr(ex);
                                throw ex;
                            }
                        }

                        //后绘制行的自定义线：双红线,这样单元格设置的边框线会被行的自定义线给覆盖掉上下的线
                        if (tableInforPara.Rows[i + fieldIndex].RowLineColor != Color.Black)
                        {
                            DrawRowLine(i + fieldIndex, 0, tableInforPara.Rows[i + fieldIndex].RowLineColor, g, tableInforPara);
                        }

                        if (tableInforPara.Rows[i + fieldIndex].CustomLineType != LineTypes.None)
                        {
                            DrawCustomRowLine(i + fieldIndex, 0, tableInforPara.Rows[i + fieldIndex].CustomLineType, g, tableInforPara, tableInforPara.Rows[i + fieldIndex].CustomLineColor);
                        }

                        //刷新区域，横向和纵向表格判断
                        RectangleF rf = new RectangleF(tableInforPara.Rows[i + fieldIndex].Loaction, tableInforPara.Rows[i + fieldIndex].RowSize);
                        if (tableInforPara.Vertical)
                        {
                            try
                            {
                                //rf = new Rectangle(tableInforPara.Cells[0, _CurrentCellIndex.Y].Loaction,
                                rf = new Rectangle(tableInforPara.Rows[i + fieldIndex].Loaction,
                                    new Size(tableInforPara.Cells[0, 0].CellSize.Width, tableInforPara.Rows[0].RowSize.Height * this._TableInfor.ColumnsCount));
                            }
                            catch (Exception ex)
                            {
                                Comm.LogHelp.WriteErr(ex);
                                throw ex;
                            }
                        }

                        rf.Inflate(2f, 2f);//扩大区域，结构放大
                        pictureBoxBackGround.Invalidate(new Region(rf));

                        pictureBoxBackGround.Update();
                    }
                }

                _isCreatImages = false;
            }


        }

        /// <summary>重绘（部分区域进行刷新，提高效率）
        /// 根据指定的部分区域进行刷新，降低界面闪烁和性能
        /// </summary>
        /// <param name="rtbe"></param>
        private void Redraw(TransparentRTBExtended rtbe)
        {
            try
            {
                if (isClickAdjusting)
                {
                    return; // 将多行数据线准备到第一格的时候，无需刷新
                }

                //先绘制底图
                Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                _isCreatImages = true;
                //Point orgLocation = new Point(-1, -1);
                Point orgLocation = new Point(rtbe.Location.X, rtbe.Location.Y - 1); //rtbe.Location.Y - 1
                printRtbeExtend(rtbe, g, orgLocation);
                _isCreatImages = false;

                //有对角线，也要绘制对角线，位置会被挡住
                if (_TableInfor.CurrentCell.DiagonalLineN)
                {
                    DrawLineZN(_TableInfor.CurrentCell.RowIndex, _TableInfor.CurrentCell.ColIndex, g, "n", _TableInfor);
                }
                else if (_TableInfor.CurrentCell.DiagonalLineZ)
                {
                    DrawLineZN(_TableInfor.CurrentCell.RowIndex, _TableInfor.CurrentCell.ColIndex, g, "z", _TableInfor);
                }

                //双红线可能就被挡住了。
                if (_TableInfor.Rows[_CurrentCellIndex.X].CustomLineType != LineTypes.None)
                {
                    //Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);
                    //g.SmoothingMode = SmoothingMode.AntiAlias;

                    DrawCustomRowLine(_CurrentCellIndex.X, 0, _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType, g, _TableInfor, _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor);

                    //g.Dispose();

                    RectangleF rfCell = _TableInfor.CurrentCell.Rect;

                    //rf.Inflate(100f, 100f);
                    rfCell.Inflate(2f, 2f);//扩大区域，结构放大
                    pictureBoxBackGround.Invalidate(new Region(rfCell));

                    pictureBoxBackGround.Update();
                    g.Dispose();
                    return;
                }

                RectangleF rf = new RectangleF(rtbe.Location, rtbe.Size);

                //rf.Inflate(100f, 100f);
                rf.Inflate(2f, 2f);//扩大区域，结构放大
                pictureBoxBackGround.Invalidate(new Region(rf));

                pictureBoxBackGround.Update();

                g.Dispose();
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }
        # endregion 重绘方法:全局刷新

        # region 重置单元格背景，防止重叠
        /// <summary>
        /// 重置单元格背景，防止重叠
        /// </summary>
        /// <param name="rtbe"></param>
        private void ReSetCellBackGround(TransparentRTBExtended rtbe)
        {
            //先绘制底图
            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);

            Bitmap bmpRtb = new Bitmap(rtbe.Width + Consts.PRINT_CELL_DIFF, rtbe.Size.Height + 1); //+2防止打印正好,但是光标出来绘制的时候,边界遮挡
            Graphics.FromImage(bmpRtb).Clear(this.pictureBoxBackGround.BackColor);

            g.DrawImage(bmpRtb, rtbe.Location.X, rtbe.Location.Y, bmpRtb.Width, bmpRtb.Height);

            //有对角线，也要绘制对角线，位置会被挡住
            if (_TableInfor.CurrentCell.DiagonalLineN)
            {
                DrawLineZN(_TableInfor.CurrentCell.RowIndex, _TableInfor.CurrentCell.ColIndex, g, "n", _TableInfor);
            }
            else if (_TableInfor.CurrentCell.DiagonalLineZ)
            {
                DrawLineZN(_TableInfor.CurrentCell.RowIndex, _TableInfor.CurrentCell.ColIndex, g, "z", _TableInfor);
            }

            RectangleF rf = new RectangleF(rtbe.Location, new Size(rtbe.Width, rtbe.Size.Height + 1)); // - 2不会遮挡掉单元格线
            //rf.Inflate(100f, 100f);
            rf.Inflate(3f, 3f);//扩大区域，结构放大
            pictureBoxBackGround.Invalidate(new Region(rf));

            pictureBoxBackGround.Update();
        }
        # endregion 重置单元格背景，防止重叠

        # region 单元格的输入框隐藏的时候，重绘到画布
        /// <summary>
        /// 显示状态变为隐藏的时候，绘制输入框内容到底板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellsRedraw_VisibleChanged(object sender, EventArgs e)
        {
            if (!_IsNotNeedCellsUpdate)
            {
                TransparentRTBExtended rtbe = (TransparentRTBExtended)sender; //RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;

                if (!rtbe.Visible && !rtbe.IsDisposed)
                {
                    Redraw(rtbe);

                    ////双红线可能就被挡住了。
                    //if (_TableInfor.Rows[_CurrentCellIndex.X].CustomLineType != LineTypes.None)
                    //{
                    //    Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);
                    //    g.SmoothingMode = SmoothingMode.AntiAlias;

                    //    DrawCustomRowLine(_CurrentCellIndex.X, 0, _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType, g, _TableInfor, _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor);

                    //    g.Dispose();
                    //}
                }
            }
        }
        #endregion 单元格的输入框隐藏的时候，重绘到画布

        #region 从模板配置文件，绘制模板内容，显示表单到界面
        /// <summary>
        /// 绘制底图、加载控件
        /// </summary>
        /// <param name="g"></param>
        /// <param name="gDisposeFlg"></param>
        /// <param name="addControlFlg">打印当前页的时候，不需要加载控件</param>
        /// <param name="isPrintlAll">打印所有页的时候，不需要加载控件，但是要设置参数，然后取值后打印</param>
        private void DrawEditorChart(Graphics g, bool gDisposeFlg, bool addControlFlg, bool isPrintlAll) //打印全部页可以扩展参数
        {
            //消除锯齿，平滑
            //g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.TextRenderingHint = TextRenderingHint.AntiAlias;

            //g.PixelOffsetMode = PixelOffsetMode.HighQuality; //高像素偏移质量   
            //g.CompositingQuality = CompositingQuality.HighQuality;
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            XmlNode xnlEach;
            Font eachFont = new Font("宋体", 9F, FontStyle.Regular);
            Pen eachPen = new Pen(Color.Black, 1);
            SolidBrush sBrrsh;
            int x = 0;
            int y = 0;
            int a = 0;
            int b = 0; //坐标临时值
            bool tempBool;
            int tempInt;
            string tempString;
            Bitmap tempBitmap = null;

            VectorControl tempVectorControl = null; //新版的矢量图
            Vector tempVector;                      //简洁版的矢量图，方便操作
            LabelExtended lblNew;
            RichTextBoxExtended rtbeNew;
            TransparentRTBExtended rtbeNewTransparent;
            CheckBoxExtended chkbNew;
            ComboBoxExtended combNew;
            PictureBox picNew;
            TableInfor tableInforNew;

            XmlNode xnlEachCol;


            Size tempSize;
            string[] arr;
            HorizontalAlignment inputBoxAlignment;

            string[] arrRtbeValue;

            Point cellDiffLoation = new Point(0, 0); //输入框的边界不能紧贴边界线，需要有一定的间距

            string[] arrEachItem;
            string[] arrEachItemTime;
            RowForeColors rfc;

            if (_node_Page == null || _node_Page.ChildNodes.Count == 0)
            {
                return;
            }

            //--↓↓↓全局属性在这里读取设置到全局变量，如果在双击事件中的读取的话，界面显示没有问题。但是生成所有页图片等就加载不了。

            //if (!bool.TryParse((_TemplateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm)) as XmlElement).GetAttribute(nameof(EntXmlModel.FingerPrintSign)).Trim(), out _FingerPrintSign))
            //{
            //    _FingerPrintSign = false;
            //}

            //读取根节点属性信息：如果含有<Table标签，那么就是含有Table表格的表单，否则就不含有表格，即：非表格样式。
            //和一般的String等标签一样的定义。这样所有的表单类型，就统一化处理了。
            if (_node_Page.SelectSingleNode(nameof(EntXmlModel.Table)) == null)
            {
                _TableType = false;
                _OwnerTableHaveMultilineCell = false;
            }
            else
            {
                _TableType = true;

                _OwnerTableHaveMultilineCell = false; //最初设置为false，根据下面有一列为true，就为True

                //每页的行数
                if (!int.TryParse((_node_Page.SelectSingleNode(nameof(EntXmlModel.Table)) as XmlElement).GetAttribute(nameof(EntXmlModel.FormRows)), out _PageRows))
                {
                    _PageRows = 50; //表格样式的每页行数
                }

                //只有一页模板的，那么每页的行数都一样
                _PageRowsFristPage = TableClass.GetPageRowsFristPage(_TemplateXml); //其实打开每一页后，已经用不到第一页行数。只有在计算行数的时候用到：单机版，生成所有页图片接口，移动接口
                _PageRowsSecondPage = TableClass.GetPageRowsSecondPage(_TemplateXml);


                //表格的第一行的日期和时间不能为空
                if (!string.IsNullOrEmpty((_node_Page.SelectSingleNode(nameof(EntXmlModel.Table)) as XmlElement).GetAttribute(nameof(EntXmlModel.FirstRowMustDate))))
                {
                    if (!bool.TryParse((_node_Page.SelectSingleNode(nameof(EntXmlModel.Table)) as XmlElement).GetAttribute(nameof(EntXmlModel.FirstRowMustDate)), out _FirstRowMustDate))
                    {
                        _FirstRowMustDate = false;
                    }
                }
                else
                {
                    _FirstRowMustDate = true;
                }

                //保存时排序：
                if (!bool.TryParse((_node_Page.SelectSingleNode(nameof(EntXmlModel.Table)) as XmlElement).GetAttribute(nameof(EntXmlModel.SaveAutoSort)).Trim(), out _SaveAutoSort))
                {
                    _SaveAutoSort = false;
                }

                //排序时，日期规范要求AutoSortMode：默认空表示同一页相同日期不填，1表示同一页即时日期相同，但是时间不一样，也填写日期
                _AutoSortMode = (_node_Page.SelectSingleNode(nameof(EntXmlModel.Table)) as XmlElement).GetAttribute(nameof(EntXmlModel.AutoSortMode)).Trim();

                _SumSetting = (_node_Page.SelectSingleNode(nameof(EntXmlModel.Table)) as XmlElement).GetAttribute(nameof(EntXmlModel.SumSetting)).Trim();
            }

            if (_TableType)
            {
                //页尾斜线
                //画笔定义：_PageEndCrossLinePen
                if (!string.IsNullOrEmpty((_TemplateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm)) as XmlElement).GetAttribute(nameof(EntXmlModel.PageEndCrossLinePen)).Trim()))
                {
                    arr = (_TemplateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm)) as XmlElement).GetAttribute(nameof(EntXmlModel.PageEndCrossLinePen)).Trim().Split(',');
                    _PageEndCrossLinePen = new Pen(Color.FromName(arr[0].Trim()), float.Parse(arr[1]));
                }
                else
                {
                    _PageEndCrossLinePen = new Pen(Color.Red, 1);
                }
            }

            ////toolStripStatusLabelType.Text = "医生专用表单";
            ////toolStripStatusLabelType.Text = "护士专用表单";
            ////toolStripStatusLabelType.Text = "医生/护士公用表单";
            ////toolStripStatusLabelType.Text = "权限提示：XXX";

            if (addControlFlg)
            {
                if (CallBackUpdateDocumentDetailInfor != null)
                {
                    if (_TemplateRight == "0")
                    {
                        CallBackUpdateDocumentDetailInfor(new string[] { null, "医生表单" }, null);
                    }
                    else if (_TemplateRight == "1")
                    {
                        CallBackUpdateDocumentDetailInfor(new string[] { null, "护士表单" }, null);
                    }
                    else
                    {
                        CallBackUpdateDocumentDetailInfor(new string[] { null, "医生/护士公用表单" }, null);
                    }
                }
            }

            //int tabindex = 0;

            //顺序读取详细配置信息的每个节点，进行设置
            for (int i = 0; i < _node_Page.ChildNodes.Count; i++)
            {
                //如果是注释那么跳过
                if (_node_Page.ChildNodes[i].Name == @"#comment" || _node_Page.ChildNodes[i].Name == @"#text")
                {
                    continue;
                }

                xnlEach = _node_Page.ChildNodes[i];

                if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Alignment)).ToLower() == "left")
                {
                    inputBoxAlignment = HorizontalAlignment.Left;
                }
                else if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Alignment)).ToLower() == "right")
                {
                    inputBoxAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    inputBoxAlignment = HorizontalAlignment.Center;//默认居中
                }

                switch (xnlEach.Name)
                {
                    case "Rect":
                        #region Rect                        
                        //设置外框参数
                        x = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size)), 0);//边框宽
                        y = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size)), 1);//边框高

                        a = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0);//边框距左边缘空白距离
                        b = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1);//边框距上边缘空白距离

                        //判断是否间距
                        if (b <= _SpaceTarget.Y)
                        {
                            _OriginPoint = _OriginPointBack;
                        }
                        else
                        {
                            if (_Space != 0)
                            {
                                _OriginPoint = new Point(_OriginPointBack.X, _OriginPointBack.Y + _Space);
                            }
                        }

                        //绘制参数：颜色，和字体大小
                        //绘制外框
                        {
                            Color color = ColorTranslator.FromHtml((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                            g.DrawRectangle(new Pen(color,
                                Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)), 0)), new Rectangle(a + _OriginPoint.X, b + _OriginPoint.Y, x, y));
                        }
                        //g.DrawRectangle(new Pen(Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color))),
                        //    getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)), 0)), new Rectangle(a + _OriginPoint.X, b + _OriginPoint.Y, x, y));

                        break;
                    #endregion
                    case "Line":
                        #region Line
                        //画线的话，坐标是线宽的中心点，不是上左边界点。如果画宽度为1的线，基本可以忽略：中心点和上边界点的误差，编辑器支持斜线的时候发现的。
                        {
                            Color color = ColorTranslator.FromHtml((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                            eachPen = new Pen(color, float.Parse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width))));
                        }
                        //eachPen = new Pen(Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color))), float.Parse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width))));
                        a = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0);//边框距左边缘空白距离
                        b = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1);//边框距上边缘空白距离

                        x = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationEnd)), 0);//边框距左边缘空白距离
                        y = Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationEnd)), 1);//边框距上边缘空白距离

                        //判断是否间距
                        if (y <= _SpaceTarget.Y)
                        {
                            _OriginPoint = _OriginPointBack;
                        }
                        else
                        {
                            if (_Space != 0)
                            {
                                _OriginPoint = new Point(_OriginPointBack.X, _OriginPointBack.Y + _Space);
                            }
                        }

                        tempString = (xnlEach as XmlElement).GetAttribute("LineType").Trim();
                        if (tempString != "")
                        {
                            if (tempString.ToLower() == "dash")
                            {
                                eachPen.DashStyle = DashStyle.Dash;
                            }
                            else if (tempString.ToLower() == "dashdot")
                            {
                                eachPen.DashStyle = DashStyle.DashDot;
                            }
                            else if (tempString.ToLower() == "dot")
                            {
                                eachPen.DashStyle = DashStyle.Dot;
                            }
                            else if (tempString.ToLower() == "solid")
                            {
                                eachPen.DashStyle = DashStyle.Solid;
                            }
                            else if (tempString.ToLower() == "dashdotdot")
                            {
                                eachPen.DashStyle = DashStyle.DashDotDot;
                            }
                            else
                            {
                                eachPen.DashStyle = DashStyle.Custom;
                            }
                        }

                        if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Repeat)) == "")//大多时候不需要重复绘制
                        {
                            g.DrawLine(eachPen,
                                new Point(a + _OriginPoint.X, b + _OriginPoint.Y),
                                new Point(x + _OriginPoint.X, y + _OriginPoint.Y));
                        }
                        else//重复绘制
                        {
                            string repeatStr = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Repeat));
                            int count = int.Parse(repeatStr.Split(',')[2].Trim());
                            float repeatAdd = float.Parse(repeatStr.Split(',')[1].Trim());

                            if (repeatStr.Split(',')[0].Trim() == "East")//水平重复
                            {
                                for (int j = 0; j <= count; j++)
                                {
                                    g.DrawLine(eachPen,
                                            new Point(a + _OriginPoint.X + (int)(repeatAdd * j), b + _OriginPoint.Y),
                                            new Point(x + _OriginPoint.X + (int)(repeatAdd * j), y + _OriginPoint.Y));
                                }
                            }
                            else//纵向重复
                            {
                                for (int j = 0; j <= count; j++)
                                {
                                    g.DrawLine(eachPen,
                                           new Point(a + _OriginPoint.X, b + _OriginPoint.Y + (int)(repeatAdd * j)),
                                            new Point(x + _OriginPoint.X, y + _OriginPoint.Y + (int)(repeatAdd * j)));
                                }
                            }
                        }

                        break;
                    #endregion
                    case nameof(EntXmlModel.Lable):
                        #region Lable
                        eachFont = Comm.getFont((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)));

                        if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).Trim().ToLower().Contains(nameof(EntXmlModel.bold)) &&
                            (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).Trim().ToLower().Contains(nameof(EntXmlModel.italic)))
                        {
                            eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size, FontStyle.Bold | FontStyle.Italic);
                        }
                        else if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).Trim().ToLower().Contains(nameof(EntXmlModel.bold)))
                        {
                            eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size, FontStyle.Bold);
                        }
                        else if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).Trim().ToLower().Contains(nameof(EntXmlModel.italic)))
                        {
                            eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size, FontStyle.Italic);
                        }
                        else
                        {
                            eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size, FontStyle.Regular);
                        }

                        //界面显示和绘制时，和打印时，font的unit默认是point，同样是绘制，结果字符串宽度和高度不一样。只能用过GraphicsUnit.World和GraphicsUnit.Pixel来强行设置
                        //尤其文字多了，就会很明显看出来。如果超出边界线，那就更不太好了。
                        if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)).Length > 15
                             || (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.GraphicsUnit)).Trim().ToLower() == "pixel") //不了不让以后的修改和整体变动产生影响，先这样处理吧。不超出就好
                        {
                            //最好的彻底的方式，还是在编辑器和表单一起处理uhit，就100%一致了。
                            //if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.GraphicsUnit)).Trim().ToLower() == "pixel" || (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.GraphicsUnit)).Trim().ToLower() == "world")
                            //{
                            eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size + 3.1f, eachFont.Style, GraphicsUnit.Pixel);
                            //eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size, eachFont.Style, GraphicsUnit.Pixel);
                            //}
                        }
                        {
                            Color color = ColorTranslator.FromHtml((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                            sBrrsh = new SolidBrush(color);
                        }
                        //sBrrsh = new SolidBrush(Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color))));

                        //判断是否间距
                        if (Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) <= _SpaceTarget.Y)
                        {
                            _OriginPoint = _OriginPointBack;
                        }
                        else
                        {
                            if (_Space != 0)
                            {
                                _OriginPoint = new Point(_OriginPointBack.X, _OriginPointBack.Y + _Space);
                            }
                        }

                        if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Repeat)) == "")//大多时候不需要重复绘制
                        {
                            //private Font ValueFont = new Font("宋体", 16, GraphicsUnit.Pixel);
                            //此时绘制的文本其大小就是大约在18*13个像素（通过measureString函数，得到的字符size，会在高宽分别加上1/6），因为GraphicsUnit 属性设置为Pixel。
                            //就是因为字体的 GraphicsUnit.Pixel如果不指定，那么在界面绘制和打印的时候，得到的默认值不一样，效果就不一样了
                            //GraphicsUnit unit GraphicsUnit.World和GraphicsUnit.Pixel就一样了
                            //eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size, eachFont.Style, GraphicsUnit.Pixel);//GraphicsUnit.Point默认的，打印和界面不一样
                            //为了不修改大小，那么：不同字体的相差值不一样，大概是3左右
                            //eachFont = new Font(eachFont.FontFamily.Name, eachFont.Size + 3, eachFont.Style, GraphicsUnit.Pixel);

                            g.DrawString((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)),
                                        eachFont,
                                        sBrrsh,
                                        new PointF(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y));

                            //打印的时候和界面显示的会不一样。比如一个长的字符串右边一条竖线，界面上没有超出。打印的时候就会超出了。
                            //这个正好也能解释之前迷茫很久的，输入框中的文字显示的时候，和打印绘制的时候宽度不一样了的问题了。现在终于搞清楚了，是字体的uhit设置问题！！！！！！
                            //MessageBox.Show(g.MeasureString((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)), eachFont).ToSize().ToString()); //调试看看：界面绘制的和打印绘制的确实不一样。打印的要长，要高一些。
                        }
                        else//重复绘制
                        {
                            string repeatStr = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Repeat));
                            int count = int.Parse(repeatStr.Split(',')[2].Trim());
                            int repeatAdd = int.Parse(repeatStr.Split(',')[1].Trim());
                            if (repeatStr.Split(',')[0].Trim() == "East")//水平重复
                            {
                                for (int j = 0; j <= count; j++)
                                {
                                    g.DrawString((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)),
                                                eachFont,
                                                sBrrsh,
                                                new PointF(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X + repeatAdd * j, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y));
                                }
                            }
                            else//纵向重复
                            {
                                for (int j = 0; j <= count; j++)
                                {
                                    g.DrawString((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)),
                                                eachFont,
                                                sBrrsh,
                                                new PointF(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y + repeatAdd * j));
                                }
                            }
                        }

                        break;
                    #endregion
                    case nameof(EntXmlModel.BindingValue): //动态标签显示文字
                        #region BindingValue

                        if (!addControlFlg)
                        {
                            continue; //不是添加控件，那么只需要绘制静态背景
                        }

                        eachFont = Comm.getFont((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)));

                        if (isPrintlAll)
                        {
                            if (_PrintAll_LabelExtended == null)
                            {
                                _PrintAll_LabelExtended = new LabelExtended();
                            }

                            lblNew = _PrintAll_LabelExtended;
                            lblNew._IsPrintAll = true;
                        }
                        else
                        {
                            lblNew = new LabelExtended();
                        }

                        lblNew.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                        lblNew.Location = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y);
                        lblNew.Text = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default));
                        lblNew.BackColor = Color.Transparent;
                        lblNew.Font = eachFont;
                        //lblNew.ForeColor = Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));


                        //if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)) != "")
                        //{
                        //    lblNew.ForeColor = Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)).Trim());
                        //}
                        if (string.IsNullOrEmpty((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color))))
                            lblNew.ForeColor = Color.Black;
                        else
                            lblNew.ForeColor = ColorTranslator.FromHtml((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));


                        lblNew.Default = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default));
                        lblNew._YMD = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.DateFormat));  //应付有写表单的入院日期，需要显示完整的时分秒的,或者生日等需要根据置顶格式来显示的时候

                        if (!isPrintlAll)
                        {
                            //光标设置，默认是0表示空，即不设置，那么不设置，根据模板中的顺序（编辑器保存的时候按照坐标自动排序）
                            SetTabIndex(xnlEach, lblNew);

                            lblNew.MouseLeave += new System.EventHandler(this.lblBasicInfor_MouseLeave);
                            lblNew.MouseEnter += new System.EventHandler(this.lblBasicInfor_MouseEnter);
                            lblNew.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lblBasicInfor_MouseDoubleClick);

                            lblNew.Click += new System.EventHandler(this.GoOnPrintLocation_Click);

                            //除页号以外，都是基本信息
                            if (lblNew.Name != "_PageNo") //床号也要可以修改：某科室写了4页，这个科室写后5页……
                            {
                                _BaseInforLblnameArr.Add(lblNew); //把所有的基本信息名，都保存下来，方便以后更新调用
                            }
                        }

                        if (lblNew.Name != "_PageNo")
                        {
                            //不是“页码”的标签，居左
                            lblNew.AutoSize = true; //如果为空，那么size就为0，双击事件也无法响应了
                            lblNew.TextAlign = System.Drawing.ContentAlignment.TopLeft;
                        }
                        else
                        {
                            //页码标签，特殊处理，让其居中。这样效果比较好。
                            if (!isPrintlAll)
                            {
                                _PageNo = lblNew;
                            }

                            lblNew.AutoSize = false;
                            lblNew.TextAlign = System.Drawing.ContentAlignment.TopCenter;

                            //默认为第一页
                            lblNew.Text = "1";
                            lblNew.Width = 25; //最多三位数字的页码，如果还宽度小，显示不完整，那么将页码的字体变小一点就可以了
                            //虽然这样固定宽度不太好，但是可以在同一模板中，页码是一位数字和三位数字的时候都居中。

                            //Format属性，支持页码的样式：最大页/总页数也可以加进来。
                            if (!string.IsNullOrEmpty((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Format)))) //_PageNoFormat
                            {
                                _PageNoFormat = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Format));
                                lblNew.AutoSize = true;
                            }
                            else
                            {
                                _PageNoFormat = "";
                            }
                        }

                        //如果配置了标签大小，在标签LabelExtended控件的内部事件中ResetMySize()，自动进行回航。打印所有页的时候，下面要再处理
                        //体温单做法：为了实现转科、转病区、转床的时候回行，坐标位置同时向上移动；那么要记录下原始的坐标位置。表单中就不移动坐标了。
                        //在打印（当前页和所有页）的时候根据坐标上下居中
                        //如果还要设置是不是手动修改，那么加上¤
                        lblNew.Tag = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size));
                        lblNew._Size = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size));
                        lblNew._Location = lblNew.Location;

                        if (isPrintlAll)
                        {
                            //打印所有页的时候，就直接绘制：
                            if (lblNew.Name == "_PageNo")
                            {
                                //页号手动修改处理
                                if (_PrintAllStartPage == 0)
                                {
                                    //lblNew.Text = printAll_PageIndex.ToString();

                                    if (string.IsNullOrEmpty(_PageNoFormat))
                                    {
                                        lblNew.Text = printAll_PageIndex.ToString();
                                    }
                                    else
                                    {
                                        lblNew.Text = String.Format(_PageNoFormat, printAll_PageIndex.ToString(), _MaxPageForPrintAllShow.ToString());
                                    }
                                }
                                else
                                {
                                    //lblNew.Text = (printAll_PageIndex + _PrintAllStartPage).ToString();
                                    if (string.IsNullOrEmpty(_PageNoFormat))
                                    {
                                        lblNew.Text = (printAll_PageIndex + _PrintAllStartPage).ToString();
                                    }
                                    else
                                    {
                                        lblNew.Text = String.Format(_PageNoFormat, (printAll_PageIndex + _PrintAllStartPage).ToString(), _MaxPageForPrintAllShow.ToString());
                                    }
                                }
                            }
                            else
                            {
                                if (_PrintAll_PageNode != null)
                                {
                                    //其他，如：科室等信息:
                                    //但是如果需要更新的话，为了不要打开某页保存后再打印所有，这里也可以做好更新处理后再打印
                                    lblNew.Text = ((_PrintAll_PageNode as XmlElement).GetAttribute(lblNew.Name)).Split('¤')[0];
                                }

                                //如果为空，取最新的打印显示
                                if (lblNew.Text == "")
                                {
                                    lblNew.Text = patientsListView.GetPatientsInforFromName(_PatientsInforDT, this._patientInfo.PatientId, lblNew.Name);
                                }
                            }

                            //根据模板配置的Size大小，一般不配置大小，表示：不自动换行。
                            Size newS = Comm.getSize(lblNew.Tag.ToString().TrimEnd('¤')).ToSize();

                            //调整坐标位置，纵向居中
                            //lblNew.Location = lblNew._Location; //恢复模板设置的坐标位置
                            Point locationAdjust = GetVariableVerticalCenterLocation(lblNew);

                            if (newS.Width > 0 && newS.Height > 0)
                            {
                                //Size adjustSize = g.MeasureString(lblNew.Text, lblNew.Font).ToSize();
                                //如果模板中配置了大小，那么打印就自动回行的，肯定居左
                                g.DrawString(lblNew.Text, new Font(lblNew.Font.Name, lblNew.Font.Size - Comm.GetPrintFontGay(lblNew.Font.Size)), new SolidBrush(lblNew.ForeColor), new RectangleF(lblNew.Location.X, locationAdjust.Y, newS.Width + 4, newS.Height));
                            }
                            else
                            {
                                if (lblNew.TextAlign == System.Drawing.ContentAlignment.MiddleCenter || lblNew.TextAlign == System.Drawing.ContentAlignment.TopCenter)
                                {
                                    //居中
                                    g.DrawString(lblNew.Text, lblNew.Font, new SolidBrush(lblNew.ForeColor), lblNew.Location.X + lblNew.Size.Width / 2 - (int)g.MeasureString(lblNew.Text, lblNew.Font).Width / 2, locationAdjust.Y);
                                }
                                else
                                {
                                    //一般都是居左
                                    g.DrawString(lblNew.Text, lblNew.Font, new SolidBrush(lblNew.ForeColor), lblNew.Location.X, locationAdjust.Y);
                                }
                            }
                        }
                        else
                        {
                            //界面打开，就直接添加该控件
                            this.pictureBoxBackGround.Controls.Add(lblNew);
                        }

                        break;
                    #endregion
                    case nameof(EntXmlModel.Text): //输入框 和透明输入框一样
                        #region Text

                        if (!addControlFlg)
                        {
                            continue; //不是添加控件，那么只需要绘制静态背景
                        }

                        eachFont = Comm.getFont((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)));

                        if (isPrintlAll)
                        {
                            if (_PrintAll_RichTextBoxExtended == null || _PrintAll_RichTextBoxExtended.IsDisposed)
                            {
                                _PrintAll_RichTextBoxExtended = new RichTextBoxExtended();
                            }

                            rtbeNew = _PrintAll_RichTextBoxExtended;
                        }
                        else
                        {
                            rtbeNew = new RichTextBoxExtended();
                        }

                        rtbeNew._InputBoxAlignment = inputBoxAlignment;
                        rtbeNew.SetAlignment();

                        rtbeNew.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                        rtbeNew.Location = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y);
                        //rtbeNew.ForeColor = Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                        rtbeNew.ForeColor = ColorTranslator.FromHtml((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                        rtbeNew.DefaultForeColor = rtbeNew.ForeColor;
                        rtbeNew.Font = eachFont;
                        rtbeNew.Size = Comm.getSize((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();

                        rtbeNew._AttachForm = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Attach)).Trim();

                        rtbeNew._CheckRegex = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.CheckRegex));  //验证表达式

                        if (rtbeNew.Name.StartsWith("日期") || rtbeNew.Name.Contains("日期") || rtbeNew.Name.Contains("时间"))
                        {
                            rtbeNew._YMD = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.DateFormat)); //默认的日期格式
                            rtbeNew._YMD_Default = rtbeNew._YMD;
                        }

                        //先初始化，在设置是否多行，不然会重置
                        if (!isPrintlAll)
                        {
                            initRtbe(rtbeNew);

                            //光标设置，默认是0表示空，即不设置，那么不设置，根据模板中的顺序（编辑器保存的时候按照坐标自动排序）
                            SetTabIndex(xnlEach, rtbeNew);
                        }

                        if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Multiline)), out tempBool))
                        {
                            tempBool = false;
                        }
                        rtbeNew.Multiline = tempBool;
                        rtbeNew.WordWrap = false; //要程序控制换行，如果自动换行，换行文字位置在打印和输入框中显示会不一样

                        //打印所有页的时候，不用设置下面的，会影响性能
                        if (!isPrintlAll)
                        {
                            //initRtbe(rtbeNew);

                            tempBool = false;
                            if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.ReadOnly)), out tempBool))
                            {
                                tempBool = false;
                            }
                            rtbeNew.ReadOnly = tempBool;
                            rtbeNew.IsReadOnly = tempBool;

                            rtbeNew._Default = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)); //rtbeNew.Default = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default));
                            rtbeNew.Score = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Score));

                            bool autoLoadDefault = false;
                            bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.AutoLoadDefault)), out autoLoadDefault);
                            if (autoLoadDefault)
                            {
                                rtbeNew.Text = rtbeNew._Default;
                            }

                            //放在这里，打印所有页的时候，位置为偏下
                            //if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Multiline)), out tempBool))
                            //{
                            //    tempBool = false;
                            //}
                            //rtbeNew.Multiline = tempBool;
                            //rtbeNew.WordWrap = false;

                            rtbeNew.Leave += new EventHandler(Rtbe_LeaveSaveDataToXml);

                            rtbeNew.Click += new System.EventHandler(this.GoOnPrintLocation_Click);

                            rtbeNew.SetSelectItems((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.MenuHelpString))); //右击菜单的选择项

                            //rtbeNew.WordWrap = rtbeNew.Multiline;//自动换行  自动换行的时候，打印的换行会有问题
                            if (rtbeNew.Multiline)
                            {
                                rtbeNew.TextChanged += new System.EventHandler(this.CellsRtbeWordWrapAutoLine_TextChanged);
                            }

                            rtbeNew.SetSelectWords(GetSelectWord((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.HelpString)))); //右击菜单的插入文字

                            rtbeNew.Sum = ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Total))); //非表格的合计公式

                            if (!int.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.MaxLength)), out tempInt))
                            {
                                tempInt = 50;
                            }

                            rtbeNew.MaxLength = tempInt;
                            rtbeNew._MaxLength = rtbeNew.MaxLength;

                            _TableHeaderList.Add(rtbeNew); //暂且将所有的输入框都加入到表头集合中

                            //rtbeNew.EnabledChanged +=new EventHandler(rtbeBackColor_EnabledChanged);                     
                            //rtbeNew.ReadOnly = true;
                            //rtbeNew.BackColor = Color.White;//SystemColors.Control; //防止没有权限，无效的时候有 背景变成灰色
                        }

                        if (isPrintlAll && _PrintAll_PageNode != null)
                        {
                            ////----------勾选框大小，影响到绘制的效果
                            //this.Controls.Add(rtbeNew); //这样空间大会自适应大小
                            //this.Controls.Remove(rtbeNew);
                            ////----------勾选框大小，影响到绘制的效果

                            arrRtbeValue = ((_PrintAll_PageNode as XmlElement).GetAttribute(rtbeNew.Name)).Split('¤');
                            SetPrintRtbeValue(arrRtbeValue, rtbeNew, g);

                            //rtbeNew.Dispose
                        }
                        else
                        {
                            this.pictureBoxBackGround.Controls.Add(rtbeNew);
                        }

                        break;
                    #endregion
                    case nameof(EntXmlModel.TransparentInputBox): //透明输入框
                        #region TransparentInputBox                        
                        if (!addControlFlg)
                        {
                            continue; //不是添加控件，那么只需要绘制静态背景
                        }

                        eachFont = Comm.getFont((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)));

                        if (isPrintlAll)
                        {
                            if (_PrintAll_TransparentRTBExtended == null)
                            {
                                _PrintAll_TransparentRTBExtended = new TransparentRTBExtended();
                            }

                            rtbeNewTransparent = _PrintAll_TransparentRTBExtended;
                        }
                        else
                        {
                            rtbeNewTransparent = new TransparentRTBExtended();
                        }

                        rtbeNewTransparent._InputBoxAlignment = inputBoxAlignment;
                        rtbeNewTransparent.SetAlignment();

                        rtbeNewTransparent.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                        rtbeNewTransparent.Location = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y);
                        //rtbeNewTransparent.ForeColor = Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                        rtbeNewTransparent.ForeColor = ColorTranslator.FromHtml((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                        rtbeNewTransparent.DefaultForeColor = rtbeNewTransparent.ForeColor;
                        rtbeNewTransparent.Font = eachFont;
                        rtbeNewTransparent.Size = Comm.getSize((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();

                        rtbeNewTransparent._AttachForm = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Attach)).Trim();

                        rtbeNewTransparent._CheckRegex = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.CheckRegex));  //验证表达式

                        if (rtbeNewTransparent.Name.StartsWith("日期"))
                        {
                            rtbeNewTransparent._YMD = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.DateFormat)); //默认的日期格式
                            rtbeNewTransparent._YMD_Default = rtbeNewTransparent._YMD;
                        }

                        //先初始化，在设置是否多行，不然会重置
                        if (!isPrintlAll)
                        {
                            initRtbe(rtbeNewTransparent);

                            //光标设置，默认是0表示空，即不设置，那么不设置，根据模板中的顺序（编辑器保存的时候按照坐标自动排序）
                            SetTabIndex(xnlEach, rtbeNewTransparent);
                        }

                        if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Multiline)), out tempBool))
                        {
                            tempBool = false;
                        }
                        rtbeNewTransparent.Multiline = tempBool;
                        rtbeNewTransparent.WordWrap = false;

                        //打印所有页的时候，不用设置下面的，会影响性能
                        if (!isPrintlAll)
                        {
                            //initRtbe(rtbeNewTransparent);

                            tempBool = false;
                            if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.ReadOnly)), out tempBool))
                            {
                                tempBool = false;
                            }
                            rtbeNewTransparent.ReadOnly = tempBool;
                            rtbeNewTransparent.IsReadOnly = tempBool;

                            rtbeNewTransparent._Default = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default));   //rtbeNewTransparent.Default = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default));
                            rtbeNewTransparent.Score = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Score));

                            //放在这里，打印所有页的时候，位置为偏下
                            //if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Multiline)), out tempBool))
                            //{
                            //    tempBool = false;
                            //}
                            //rtbeNewTransparent.Multiline = tempBool;
                            //rtbeNewTransparent.WordWrap = false;

                            rtbeNewTransparent.Leave += new EventHandler(Rtbe_LeaveSaveDataToXml);

                            rtbeNewTransparent.Click += new System.EventHandler(this.GoOnPrintLocation_Click);

                            rtbeNewTransparent.SetSelectItems((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.MenuHelpString))); //右击菜单的选择项

                            if (rtbeNewTransparent.Multiline)
                            {
                                rtbeNewTransparent.TextChanged += new System.EventHandler(this.CellsRtbeWordWrapAutoLine_TextChanged);
                            }

                            //rtbeNewTransparent.WordWrap = rtbeNewTransparent.Multiline;//自动换行

                            rtbeNewTransparent.SetSelectWords(GetSelectWord((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.HelpString)))); //右击菜单的插入文字

                            rtbeNewTransparent.Sum = ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Total))); //非表格的合计公式

                            if (!int.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.MaxLength)), out tempInt))
                            {
                                tempInt = 50;
                            }

                            rtbeNewTransparent.MaxLength = tempInt;
                            rtbeNewTransparent._MaxLength = rtbeNewTransparent.MaxLength;

                            _TableHeaderList.Add(rtbeNewTransparent); //暂且将所有的输入框都加入到表头集合中
                        }

                        if (isPrintlAll && _PrintAll_PageNode != null)
                        {
                            ////----------勾选框大小，影响到绘制的效果
                            //this.Controls.Add(rtbeNewTransparent); //这样空间大会自适应大小
                            //this.Controls.Remove(rtbeNewTransparent);
                            ////----------勾选框大小，影响到绘制的效果

                            arrRtbeValue = ((_PrintAll_PageNode as XmlElement).GetAttribute(rtbeNewTransparent.Name)).Split('¤');
                            SetPrintRtbeValue(arrRtbeValue, rtbeNewTransparent, g);
                        }
                        else
                        {
                            this.pictureBoxBackGround.Controls.Add(rtbeNewTransparent);
                        }

                        break;
                    #endregion
                    case nameof(EntXmlModel.CheckBox): //勾选框
                        #region CheckBox                        
                        if (!addControlFlg)
                        {
                            continue; //不是添加控件，那么只需要绘制静态背景
                        }

                        if (isPrintlAll)
                        {
                            if (_PrintAll_CheckBoxExtended == null)
                            {
                                _PrintAll_CheckBoxExtended = new CheckBoxExtended();
                            }

                            chkbNew = _PrintAll_CheckBoxExtended;
                        }
                        else
                        {
                            chkbNew = new CheckBoxExtended();

                            //光标设置，默认是0表示空，即不设置，那么不设置，根据模板中的顺序（编辑器保存的时候按照坐标自动排序）
                            SetTabIndex(xnlEach, chkbNew);
                        }

                        eachFont = Comm.getFont((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)));

                        //if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)), out tempBool))
                        //{
                        //    tempBool = false;
                        //}
                        //chkbNew.Default = tempBool;

                        chkbNew.Score = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Score));

                        //文字和框的位置样式:左右位置交换
                        tempString = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.CheckAlign));
                        if (string.IsNullOrEmpty(tempString) || tempString.ToLower() == "left")
                        {
                            chkbNew.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
                        }
                        else
                        {
                            chkbNew.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
                        }

                        chkbNew.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                        chkbNew.Text = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.ShowValue)); //chkbNew.Name;

                        //勾选框三种状态：
                        if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.ThreeState)), out tempBool))
                        {
                            tempBool = false;
                        }

                        chkbNew.ThreeState = tempBool;

                        if (chkbNew.ThreeState)
                        {
                            //<CheckBox ID="00189" Name="过敏史有" ThreeState="True" ShowValue="有" Default="Indeterminate"
                            //<Action Name="过敏史有" Event="CheckedStateChanged">过敏史有.CheckState == Indeterminate Then Warn(ThreeState:叉选)</Action>
                            //评分时也要注意，叉应该算作未选中
                            //三种状态的默认值设定
                            if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)).Trim().ToLower() == CheckState.Checked.ToString().ToLower())
                            {
                                chkbNew.CheckState = CheckState.Checked;
                                chkbNew.DefaultCheckState = chkbNew.CheckState;
                            }
                            else if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)).Trim().ToLower() == CheckState.Indeterminate.ToString().ToLower())
                            {
                                chkbNew.CheckState = CheckState.Indeterminate;
                                chkbNew.DefaultCheckState = chkbNew.CheckState;
                            }
                            else if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)).Trim().ToLower() == CheckState.Unchecked.ToString().ToLower())
                            {
                                chkbNew.CheckState = CheckState.Unchecked;
                                chkbNew.DefaultCheckState = chkbNew.CheckState;
                            }
                            else
                            {
                                //默认为空
                                if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)), out tempBool))
                                {
                                    tempBool = false;
                                }

                                chkbNew.Default = tempBool;

                                if (tempBool)
                                {
                                    chkbNew.CheckState = CheckState.Checked;
                                    chkbNew.DefaultCheckState = chkbNew.CheckState;
                                }
                                else
                                {
                                    chkbNew.CheckState = CheckState.Unchecked; //默认为选中状态。默认值设置，和取值显示和打印相反。如果取值显示，遇到false，那么就是打叉。
                                    chkbNew.DefaultCheckState = chkbNew.CheckState;
                                }
                            }
                        }
                        else
                        {
                            if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)), out tempBool))
                            {
                                tempBool = false;
                            }

                            chkbNew.Default = tempBool;
                        }


                        //如果显示值为空，那么将名字作为显示值
                        //if (chkbNew.Text == "")
                        //{
                        //    chkbNew.Text = chkbNew.Name;  //有时需要显示值就是为空的情况。
                        //}

                        chkbNew.Location = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y);
                        chkbNew.ForeColor = Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                        chkbNew.Font = eachFont;
                        chkbNew.AutoSize = true;

                        //打印所有页的时候，不用设置下面的，会影响性能
                        if (!isPrintlAll)
                        {
                            chkbNew.CheckedChanged += new System.EventHandler(this.Script_CheckedChanged);
                            chkbNew.Click += new System.EventHandler(this.GoOnPrintLocation_Click);

                            //<Action Name="过敏史有" Event="CheckedChanged">过敏史有.CheckState == Indeterminate Then Warn(ThreeState:叉选)</Action>
                            //三种状态的右击菜单菜单事件
                            if (chkbNew.ThreeState)
                            {
                                SetSkinCheckBoxMenu(chkbNew);
                            }

                            this.pictureBoxBackGround.Controls.Add(chkbNew); //大小会变
                        }
                        else
                        {
                            if (_PrintAll_PageNode != null)
                            {
                                //----------勾选框大小，影响到绘制的效果
                                //chkbNew.Size = new Size(chkbNew.Size.Width, (int)chkbNew.Font.Size * 2);
                                this.Controls.Add(chkbNew); //这样空间大会自适应大小
                                this.Controls.Remove(chkbNew);
                                //----------勾选框大小，影响到绘制的效果

                                arrRtbeValue = ((_PrintAll_PageNode as XmlElement).GetAttribute(chkbNew.Name)).Split('¤');

                                try
                                {
                                    //如果目前是打叉Indeterminate，在Checked设置为true，还是打叉的状态。设置为flase为Unchecked状态。
                                    LoadSkinCheckBox(chkbNew, arrRtbeValue);

                                    //chkbNew.Checked = bool.Parse(arrRtbeValue[0]);
                                }
                                catch
                                {
                                    chkbNew.Checked = false;
                                }

                                GDiHelp.DrawCheckBox(chkbNew, g);
                            }
                        }

                        break;
                    #endregion
                    case nameof(EntXmlModel.ComboBox): //下拉框
                        #region ComboBox

                        if (!addControlFlg)
                        {
                            continue; //不是添加控件，那么只需要绘制静态背景
                        }

                        if (isPrintlAll)
                        {
                            if (_PrintAll_ComboBoxExtended == null)
                            {
                                //防止每次都实例化，打印所有页的速度会很慢
                                _PrintAll_ComboBoxExtended = new ComboBoxExtended();
                            }

                            combNew = _PrintAll_ComboBoxExtended;
                        }
                        else
                        {
                            combNew = new ComboBoxExtended();

                            //光标设置，默认是0表示空，即不设置，那么不设置，根据模板中的顺序（编辑器保存的时候按照坐标自动排序）
                            SetTabIndex(xnlEach, combNew);
                        }

                        //combNew.Items.Clear();
                        if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Items)) != "")
                        {
                            combNew.Items.AddRange((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Items)).ToString().Split(','));
                        }

                        combNew.Tag = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Items)); //动态脚本执行，可能用到，比如选项清空后再还原

                        //下拉样式
                        if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).ToLower() != "dropdown")
                        {
                            combNew.DropDownStyle = ComboBoxStyle.DropDownList;
                        }
                        else
                        {
                            combNew.DropDownStyle = ComboBoxStyle.DropDown;
                        }

                        if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size)) != "")
                        {
                            combNew.Size = Comm.getSize((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();

                            //下拉宽度和高度
                            //combNew.DropDownWidth = combNew.Size.Width;
                            //combNew.DropDownHeight = combNew.Size.Height;
                        }

                        eachFont = Comm.getFont((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Width)));

                        combNew.Default = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default));
                        combNew.Score = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Score));

                        combNew.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                        combNew.Location = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y);
                        //combNew.ForeColor = Color.FromName((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                        combNew.ForeColor = ColorTranslator.FromHtml((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                        combNew.Font = eachFont;

                        //打印所有页的时候，不用设置下面的，会影响性能
                        if (!isPrintlAll)
                        {
                            combNew.TextChanged += new System.EventHandler(this.Script_TextChanged); //dropdown样式，输入/选择项改变的时候，都会触发

                            combNew.SelectedValueChanged += new System.EventHandler(this.Script_SelectedValueChanged); //dropdownlist样式，选择项改变的时候，上面的不会触发

                            combNew.Click += new System.EventHandler(this.GoOnPrintLocation_Click);

                            this.pictureBoxBackGround.Controls.Add(combNew);
                        }
                        else
                        {
                            if (_PrintAll_PageNode != null)
                            {
                                arrRtbeValue = ((_PrintAll_PageNode as XmlElement).GetAttribute(combNew.Name)).Split('¤');
                                combNew.Text = arrRtbeValue[0];

                                GDiHelp.DrawComboBox(combNew, g);
                            }
                        }

                        break;
                    #endregion
                    case nameof(EntXmlModel.Table): //行，一般的勾选输入样式的表格，因为是固定的，也可以用线和输入框来表示，所以这里的行是表格
                        #region Table                       
                        if (!addControlFlg)
                        {
                            continue; //不是添加控件，那么只需要绘制静态背景
                        }

                        if (_TableType)
                        {
                            //配置表格区域，表头用String或者InputBox来配置（固定表头和动态表头）

                            _PageRowsWidth = 0;
                            tableInforNew = new TableInfor(_PageRows, xnlEach.SelectNodes(nameof(EntXmlModel.Column)).Count);
                            tableInforNew.RowsCountFirstPage = _PageRowsFristPage;  //第一行表格的行数
                            tableInforNew.RowsCountSecondPage = _PageRowsSecondPage;

                            if (isPrintlAll)
                            {
                                //打印所有页，每页的table表格对象
                                _PrintAll_TableInfor = tableInforNew;
                            }
                            else
                            {
                                //界面上当前页的table表格对象
                                _TableInfor = tableInforNew;
                            }

                            int colIndex = 0;
                            int colWitdh = 0; //相对坐标的位置，一般为0，0；方便整体移动

                            Size verticalSize = new Size(0, 0);

                            _TableLocation = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y);
                            tableInforNew.Location = _TableLocation;   //赋值给变量对象，而不是全局变量控制，防止打印多页的时候，如果每页表格不一样就会混乱                      

                            cellDiffLoation = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Gap)), 0), Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Gap)), 1));
                            tableInforNew.RtbeDiff = cellDiffLoation;

                            //横向表格还是纵向表格
                            if (!bool.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Vertical)), out tempBool))
                            {
                                tempBool = false;
                            }
                            tableInforNew.Vertical = tempBool;

                            tempInt = 1;//表格分栏默认为1
                            if (!int.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.GroupColumnNum)), out tempInt))
                            {
                                tempInt = 1;
                            }
                            tableInforNew.GroupColumnNum = tempInt;

                            //表格分栏
                            if (tableInforNew.GroupColumnNum > 1)
                            {
                                tableInforNew.Cells = new CellInfor[tableInforNew.RowsCount * tableInforNew.GroupColumnNum, tableInforNew.ColumnsCount];
                                tableInforNew.Rows = new RowInfor[tableInforNew.RowsCount * tableInforNew.GroupColumnNum];
                            }

                            //默认行高设置
                            if (!int.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.RowHeight)), out _PageRowsHeight))
                            {
                                _PageRowsHeight = 25;
                            }

                            tableInforNew.PageRowsHeight = _PageRowsHeight;

                            tableInforNew.SumGroup = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.SumGroup)); //总结小结的属性 SumGroup="入量列@实入¤途径列@途径¤出量列@出量1,出量2"

                            //因为分组合计的逻辑很复杂，每家医院还有差异，现在上面的属性配置应很复杂了。所以再增加一个属性来控制实现：结类型，结事件范围，结文字的自定义控制。
                            //扩展小结总结的种类，弹出框，自定义结文字和时间范围。
                            //多个类型用¤分割,-表示前一天，没有-就是系统日期。
                            //{0}是日期格式，根据SumGroup中的“结文字日期格式”来格式化开始时间对应的日期，{1}多少小时，开始时间是结束时间间隔多少小时。
                            //结类型文字@结文字格式串,-07:00,06:59
                            //SumGroupExtend="全天总结§{0}全天总结§-07:00§06:59¤白班小结§{0}白班小结§07:00§19:59¤夜班小结§{0}夜班小结§-07:00§19:59¤临时小结§{1}小时小结§-07:00§19:59"
                            tableInforNew.SumGroupExtend = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.SumGroupExtend));

                            ////边距间隔距离和行高最小值验证
                            //if (cellDiffLoation.X < 4 || cellDiffLoation.Y < 6)
                            //{
                            //    string warning = "表格属性：gap的最小值为4，6；否则在编辑双红线等情况下会产生遮挡，产生异常。";
                            //    ShowInforMsg(warning);
                            //    MessageBox.Show(warning, "配置错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            //}

                            if (!isPrintlAll)
                            {
                                //行时段颜色
                                //RowForeColor="7:00~19:00@Blue¤19:01~6:59@Red" -> "7:00~19:00@Blue¤19:01~23:59@Red¤00:00~6:59@Red"
                                //表格式护理记录单，要求某个时段用蓝色，某个时段用红色字体书写
                                _RowForeColorsList.Clear();
                                _RowForeColor = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.RowForeColor));
                                if (!string.IsNullOrEmpty(_RowForeColor))
                                {
                                    arr = _RowForeColor.Split('¤');
                                    for (int j = 0; j < arr.Length; j++)
                                    {
                                        arrEachItem = arr[j].Trim().Split('@');

                                        arrEachItemTime = arrEachItem[0].Split('~');

                                        rfc = new RowForeColors(DateTime.Parse("2017-07-01 " + arrEachItemTime[0]), DateTime.Parse("2017-07-01 " + arrEachItemTime[1]), Color.FromName(arrEachItem[1]));

                                        _RowForeColorsList.Add(rfc);
                                    }
                                }

                                ////光标设置，默认是0表示空，即不设置，那么不设置，根据模板中的顺序（编辑器保存的时候按照坐标自动排序）
                                //if ((xnlEach as XmlElement).GetAttribute("MyTabIndex").Trim() != ""
                                //    && (xnlEach as XmlElement).GetAttribute("MyTabIndex").Trim() != "0")
                                //    if (!int.TryParse((xnlEach as XmlElement).GetAttribute("MyTabIndex").Trim(), out tabindex))
                                //    {
                                //        tabindex = 0;
                                //    }
                                //combNew.TabIndex = tabindex;
                            }


                            //开始初始化所有列：遍历各列
                            for (int j = 0; j < xnlEach.ChildNodes.Count; j++)
                            {
                                //如果是注释那么跳过
                                if (xnlEach.ChildNodes[j].Name == @"#comment" || xnlEach.ChildNodes[j].Name == @"#text")
                                {
                                    continue;
                                }

                                xnlEachCol = xnlEach.ChildNodes[j];

                                if ((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Alignment)).ToLower() == "left")
                                {
                                    inputBoxAlignment = HorizontalAlignment.Left;
                                }
                                else if ((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Alignment)).ToLower() == "right")
                                {
                                    inputBoxAlignment = HorizontalAlignment.Right;
                                }
                                else
                                {
                                    inputBoxAlignment = HorizontalAlignment.Center;//默认居中
                                }

                                //初始化好所有参数：全局变量的设置，并初始化好每列一个控件 _IsTable为True
                                rtbeNewTransparent = new TransparentRTBExtended();
                                rtbeNewTransparent._IsTable = true;

                                rtbeNewTransparent._IsPrintAll = isPrintlAll;

                                eachFont = Comm.getFont((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Width)));
                                rtbeNewTransparent._InputBoxAlignment = inputBoxAlignment;

                                //if (!isPrintlAll || !_isCreatReportImages)
                                //{
                                rtbeNewTransparent.SetAlignment();  //区域生成图表时，次数过多，会报错;创建窗口句柄时出错。
                                //}
                                //else
                                //{
                                //    rtbeNewTransparent.SetAlignmentForReport();
                                //}

                                rtbeNewTransparent.Name = (xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Name));

                                //rtbeNewTransparent.ForeColor = Color.FromName((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                                rtbeNewTransparent.ForeColor = ColorTranslator.FromHtml((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Color)));
                                rtbeNewTransparent.DefaultForeColor = rtbeNewTransparent.ForeColor;
                                rtbeNewTransparent.Font = eachFont;

                                rtbeNewTransparent._AttachForm = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Attach)).Trim();

                                rtbeNewTransparent.GroupName = (xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Group)).Trim();
                                if (!string.IsNullOrEmpty(rtbeNewTransparent.GroupName))
                                {
                                    if (!isPrintlAll)
                                    {
                                        rtbeNewTransparent.Click += new EventHandler(rtbeNewTransparentEditRow_Click);
                                    }
                                }

                                rtbeNewTransparent._CheckRegex = (xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.CheckRegex));  //验证表达式

                                if (!tableInforNew.Vertical)
                                {
                                    rtbeNewTransparent.Size = Comm.getSize((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();
                                }
                                else
                                {
                                    //纵向的表格，各列宽度一样，所以只要取第一列的宽度就行了
                                    if (verticalSize.Width == 0)
                                    {
                                        verticalSize = Comm.getSize((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();
                                    }

                                    rtbeNewTransparent.Size = Comm.getSize((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Size))).ToSize();
                                }

                                rtbeNewTransparent._DefaultSize = rtbeNewTransparent.Size;

                                //rtbeNewTransparent.Multiline = false;//表格的单元格是不允许多行的
                                //表格单元格多行处理
                                if (!bool.TryParse((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Multiline)), out tempBool))
                                {
                                    tempBool = false;
                                }
                                rtbeNewTransparent.Multiline = tempBool;
                                rtbeNewTransparent.WordWrap = false;// rtbeNewTransparent.Multiline;//自动换行
                                rtbeNewTransparent.MultilineCell = tempBool;

                                if (rtbeNewTransparent.MultilineCell || _OwnerTableHaveMultilineCell)
                                {
                                    _OwnerTableHaveMultilineCell = true;
                                    rtbeNewTransparent.OwnerTableHaveMultilineCell = true;
                                    //rtbeNewTransparent.MultilineCell = true;
                                }

                                //只要表格中含有一个是多行的单元格，那么其他不回行的，也要居中的。
                                if (_OwnerTableHaveMultilineCell)
                                {
                                    for (int index = 0; index < tableInforNew.Columns.Length; index++)
                                    {
                                        if (tableInforNew.Columns[index] != null && tableInforNew.Columns[index].RTBE != null)
                                        {
                                            tableInforNew.Columns[index].RTBE.OwnerTableHaveMultilineCell = true;
                                            //tableInforNew.Columns[index].RTBE.MultilineCell = true;
                                        }
                                    }
                                }

                                //if (rtbeNewTransparent.MultilineCell)
                                //{
                                //    //rtbeNewTransparent.SelectAll();
                                //    //RichTextBoxExtended.SetLineSpace(rtbeNewTransparent, 2); //设置较小的行间距
                                //    rtbeNewTransparent.SetVerticalCenter();
                                //}

                                rtbeNewTransparent._Default = (xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Default));    //rtbeNewTransparent.Default = (xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Default));

                                if (rtbeNewTransparent.Name.StartsWith("日期") || rtbeNewTransparent.Name.Contains("日期时间"))
                                {
                                    rtbeNewTransparent._YMD = (xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.DateFormat)); //默认的日期格式
                                    rtbeNewTransparent._YMD_Default = rtbeNewTransparent._YMD;
                                }

                                //打印所有页的时候，不用设置下面的，会影响性能
                                if (!isPrintlAll)
                                {
                                    initRtbe(rtbeNewTransparent);

                                    rtbeNewTransparent.Score = (xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Score));

                                    rtbeNewTransparent.Sum = ((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Total))); //表格中的某一行中几列单元格的的合计公式

                                    ////验证提醒：行高和单元格大小的设定
                                    //if (_PageRowsHeight < cellDiffLoation.Y * 2 + rtbeNewTransparent.Size.Height)
                                    //{
                                    //    string warning = "表格列属性：" + rtbeNewTransparent.Name + "的Size加上行的gap大于行高；在编辑双红线等情况下会产生遮挡，产生异常。";
                                    //    ShowInforMsg(warning);
                                    //    MessageBox.Show(warning,"配置错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    //}

                                    //if (!bool.TryParse((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.Multiline)), out tempBool))
                                    //{
                                    //    tempBool = false;
                                    //}
                                    //rtbeNewTransparent.Multiline = tempBool;
                                    //rtbeNewTransparent.WordWrap = rtbeNewTransparent.Multiline;//自动换行

                                    rtbeNewTransparent.SetSelectWords(GetSelectWord((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.HelpString)))); //右击菜单的插入文字

                                    rtbeNewTransparent.SetSelectItems((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.MenuHelpString))); //右击菜单的选择项

                                    if (!int.TryParse((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.MaxLength)), out tempInt))
                                    {
                                        tempInt = 100;
                                    }
                                    rtbeNewTransparent.MaxLength = tempInt;
                                    rtbeNewTransparent._MaxLength = rtbeNewTransparent.MaxLength;

                                    tempBool = false;
                                    if (!bool.TryParse((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.ReadOnly)), out tempBool))
                                    {
                                        tempBool = false;
                                    }
                                    //rtbeNewTransparent.ReadOnly = tempBool;
                                    rtbeNewTransparent.IsReadOnly = tempBool;

                                    tempBool = false;
                                    if (!bool.TryParse((xnlEachCol as XmlElement).GetAttribute(nameof(EntXmlModel.RankColumnText)), out tempBool))
                                    {
                                        tempBool = false;
                                    }

                                    if (tempBool) //设置了自动调整段落列属性开启
                                    {
                                        rtbeNewTransparent._AdjustParagraphs = true;
                                        rtbeNewTransparent.TextChanged += new System.EventHandler(this.CellsRtbeAdjustParagraphs_TextChanged);

                                        rtbeNewTransparent.ShowParagraphContent_MainForm += new EventHandler(rtbe_ShowParagraphContent_MainForm); //查看本段落的所有文字内容
                                    }
                                    else
                                    {
                                        rtbeNewTransparent._AdjustParagraphs = false;
                                    }

                                    //总结小结
                                    if (!string.IsNullOrEmpty(tableInforNew.SumGroup))
                                    {
                                        rtbeNewTransparent._EnableSUM_MainForm = true;
                                        rtbeNewTransparent.SUM_MainForm += new EventHandler(rtbe_ShowSumMainForm); //右击菜单（小结/总结），回调主窗体方法进行合计小结总结。

                                        LoadXmlDocSumGroup();

                                    }
                                    else
                                    {
                                        rtbeNewTransparent._EnableSUM_MainForm = false;
                                    }

                                    //复制单元格事件回调方法
                                    rtbeNewTransparent.CopyCellsEvent += new EventHandler(CopyCellsEvent);

                                    rtbeNewTransparent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CellsRtbeSetFocus_KeyDown);//箭头切换光标的事件处理

                                    //隐藏的时候来重绘
                                    rtbeNewTransparent.VisibleChanged += new System.EventHandler(this.CellsRedraw_VisibleChanged); //来绘制
                                    //Leave来保存 数据
                                    rtbeNewTransparent.Leave += new EventHandler(Cells_LeaveSaveData);

                                    //表格的签名列，设置为不能手动修改
                                    if (rtbeNewTransparent.Name == "签名") //表格的签名设定为不能手动修改，只能有修订历史清空去除签名
                                    {
                                        rtbeNewTransparent.IsReadOnly = true;
                                        rtbeNewTransparent.ReadOnly = true;
                                    }

                                    _IsNotNeedCellsUpdate = true;
                                    rtbeNewTransparent.Visible = false;//只有在需要的时候，才显示到指定位置
                                    _IsNotNeedCellsUpdate = false;

                                    this.pictureBoxBackGround.Controls.Add(rtbeNewTransparent);
                                }

                                //设置表格信息：
                                tableInforNew.Columns[colIndex] = new ColumnInfor();
                                tableInforNew.Columns[colIndex].RTBE = rtbeNewTransparent;
                                tableInforNew.Columns[colIndex].RTBE.Location = new Point(colWitdh + _TableLocation.X + cellDiffLoation.X, 0); //横坐标x是可以利用的

                                tableInforNew.Columns[colIndex].ColLocationX = colWitdh + _TableLocation.X;
                                tableInforNew.Columns[colIndex].ColWidth = rtbeNewTransparent.Width; //单元格输入框大小
                                tableInforNew.Columns[colIndex].ColIndex = colIndex;
                                tableInforNew.Columns[colIndex].ColName = rtbeNewTransparent.Name;
                                //tableInforNew.Columns[colIndex].ColDefault = rtbeNewTransparent._Default;
                                _PageRowsWidth += rtbeNewTransparent.Width + cellDiffLoation.X * 2; ;//记录行的总宽度     

                                tableInforNew.Columns[colIndex].GroupName = rtbeNewTransparent.GroupName;

                                //单元格信息
                                for (int row = 0; row < tableInforNew.RowsCount; row++)
                                {
                                    tableInforNew.Cells[row, colIndex] = new CellInfor(row, colIndex);
                                    tableInforNew.Cells[row, colIndex].CellName = rtbeNewTransparent.Name;

                                    if (!tableInforNew.Vertical)
                                    {
                                        //横向表格
                                        tableInforNew.Cells[row, colIndex].Loaction = new Point(colWitdh + tableInforNew.Location.X, _PageRowsHeight * row + tableInforNew.Location.Y);

                                        ////改变多行输入框的位置，如果这里设置，会改变单元格的线错开
                                        //if (tableInforNew.Columns[colIndex].RTBE.MultilineCell)
                                        //{
                                        //    tableInforNew.Cells[row, colIndex].Loaction = new Point(tableInforNew.Cells[row, colIndex].Loaction.X, tableInforNew.Cells[row, colIndex].Loaction.Y - 12);
                                        //}
                                    }
                                    else
                                    {
                                        //纵向表格
                                        tableInforNew.Cells[row, colIndex].Loaction = new Point((rtbeNewTransparent.Size.Width + cellDiffLoation.X * 2) * row + tableInforNew.Location.X, colIndex * _PageRowsHeight + tableInforNew.Location.Y);
                                    }

                                    tableInforNew.Cells[row, colIndex].CellSize = new Size(rtbeNewTransparent.Size.Width + cellDiffLoation.X * 2, _PageRowsHeight);
                                    tableInforNew.Cells[row, colIndex].CellSizeOrg = tableInforNew.Cells[row, colIndex].CellSize;
                                    tableInforNew.Cells[row, colIndex].Rect = new Rectangle(tableInforNew.Cells[row, colIndex].Loaction, tableInforNew.Cells[row, colIndex].CellSize);
                                    tableInforNew.Cells[row, colIndex].DiffLoaction = cellDiffLoation;

                                    tableInforNew.Cells[row, colIndex].Text = ""; //tableInforNew.Cells[row, colIndex].Text = rtbeNewTransparent.Default;   //调试的时候，可以开启 ；默认值，现在设计为光标进入，如果为空，就赋值默认值

                                    tableInforNew.Cells[row, colIndex].Rtf = rtbeNewTransparent.Rtf;
                                    tableInforNew.Cells[row, colIndex].IsRtf_OR_Text = rtbeNewTransparent._IsRtf;

                                    if (tableInforNew.Rows[row] == null)    //只有此一次为null的时候需要实例化
                                    {
                                        tableInforNew.Rows[row] = new RowInfor(row);
                                        tableInforNew.Rows[row].Loaction = tableInforNew.Cells[row, 0].Loaction;

                                    }
                                    else
                                    {
                                        tableInforNew.Rows[row].Loaction = tableInforNew.Cells[row, 0].Loaction;
                                    }

                                }

                                rtbeNewTransparent._DefaultCellSize = tableInforNew.Cells[0, colIndex].CellSize;

                                colIndex++;
                                colWitdh += rtbeNewTransparent.Width + cellDiffLoation.X * 2; //这个是用来计算每次各列单元格的位置
                            }

                            if (!isPrintlAll)
                            {
                                //判断是否可以自动根据时间，自动调整行位置
                                //_IsCheckedAutoPositionByTime = true;

                                bool isHaveYMD = false;
                                bool isHaveHM = false;
                                bool isHaveAdjust = false;
                                for (int index = 0; index < _TableInfor.Columns.Length; index++)
                                {
                                    if (_TableInfor.Columns[index].ColName == "日期")
                                    {
                                        isHaveYMD = true;
                                    }

                                    if (_TableInfor.Columns[index].ColName == "时间")
                                    {
                                        isHaveHM = true;
                                    }

                                    if (_TableInfor.Columns[index].RTBE._AdjustParagraphs)
                                    {
                                        isHaveAdjust = true;
                                    }
                                }

                                if (isHaveYMD && isHaveHM && !isHaveAdjust)
                                {
                                    for (int index = 0; index < _TableInfor.Columns.Length; index++)
                                    {
                                        _TableInfor.Columns[index].RTBE._CanAutoPositionByTime = true;
                                        _TableInfor.Columns[index].RTBE.AutoPositionByTime_MainForm += new EventHandler(rtbe_AutoPositionByTimeMainForm);

                                    }
                                }
                            }

                            if (!tableInforNew.Vertical)
                            {
                                tableInforNew.PageRowsWidth = _PageRowsWidth;
                            }
                            else
                            {
                                tableInforNew.PageRowsWidth = tableInforNew.Cells[0, 0].CellSize.Width * _PageRows;
                            }

                            //表格分栏的情况下，需要特殊处理后面几栏，一般为右边跟着第二栏
                            if (tableInforNew.GroupColumnNum > 1)
                            {
                                for (int field = 1; field < tableInforNew.GroupColumnNum; field++)
                                {
                                    //单元格信息
                                    for (int row = 0; row < tableInforNew.RowsCount; row++)
                                    {
                                        for (int col = 0; col < tableInforNew.ColumnsCount; col++)
                                        {
                                            tableInforNew.Cells[row + field * tableInforNew.RowsCount, col] = new CellInfor(row + field * tableInforNew.RowsCount, col);

                                            tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].Field = field; //标识该单元格属于那一栏

                                            tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].CellName = tableInforNew.Cells[row, col].CellName;
                                            tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].CellSize = tableInforNew.Cells[row, col].CellSize;
                                            tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].CellSizeOrg = tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].CellSize;
                                            tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].Loaction = new Point(tableInforNew.Cells[row, col].Loaction.X + field * _PageRowsWidth, tableInforNew.Cells[row, col].Loaction.Y);
                                            tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].Rect = new Rectangle(tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].Loaction,
                                                tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].CellSize);

                                            tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].DiffLoaction = cellDiffLoation;

                                            tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].Text = tableInforNew.Cells[row, col].Text;
                                            tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].Rtf = tableInforNew.Cells[row, col].Rtf;
                                            tableInforNew.Cells[row + field * tableInforNew.RowsCount, col].IsRtf_OR_Text = tableInforNew.Cells[row, col].IsRtf_OR_Text;

                                            if (tableInforNew.Rows[row + field * tableInforNew.RowsCount] == null)//只有此一次为null的时候需要实例化
                                            {
                                                tableInforNew.Rows[row + field * tableInforNew.RowsCount] = new RowInfor(row + field * tableInforNew.RowsCount);
                                                tableInforNew.Rows[row + field * tableInforNew.RowsCount].Loaction = tableInforNew.Cells[row + field * tableInforNew.RowsCount, 0].Loaction;

                                            }
                                            else
                                            {
                                                //tableInforNew.Rows[row + field * _TableInfor.RowsCount].Loaction = _TableInfor.Cells[row + field * _TableInfor.RowsCount, 0].Loaction;
                                            }
                                        }
                                    }
                                }
                            }

                            //设置隔行的大小
                            for (int row = 0; row < tableInforNew.RowsCount * tableInforNew.GroupColumnNum; row++)
                            {
                                tableInforNew.Rows[row].RowSize = new Size(colWitdh, _PageRowsHeight); //各行的大小
                            }


                        }
                        else
                        {
                            ShowInforMsg("当前表单的配置类型为：非表格式、属性TableType=False，无法加载表格专有配置标签：GridRow 。");
                        }


                        break;
                    #endregion
                    case "Image": //图片
                        #region Image                       
                        tempString = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)).Trim();

                        tempBitmap = null;          //先设置为null
                        tempVectorControl = null;   //先设置为null，下面判断为非null的时候，就是矢量图
                        bool isVector = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Vector)).ToBool();
                        //logo图片nameof(EntXmlModel.Default)不为空，矢量图nameof(EntXmlModel.VectorName)不为空。
                        if (tempString != "" && !isVector)
                        {//if 静态图片
                            try
                            {
                                if (RunMode)
                                {
                                    # region 服务端处理代码
                                    //System.Drawing.Image img = System.Drawing.Image.FromFile(_Recruit_XMlPath_Image + @"\" + tempString);
                                    //System.IO.MemoryStream mstream = new System.IO.MemoryStream();
                                    //System.Drawing.Bitmap t = new System.Drawing.Bitmap(img);
                                    //t.Save(mstream, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    //mstream.ToArray();
                                    # endregion 服务端处理代码

                                    //if (File.Exists(_Recruit_XMlPath_Image + @"\" + tempString)) 
                                    //tempBitmap = new Bitmap(_Recruit_XMlPath_Image + @"\" + tempString);

                                    //如果内容中已经有的话，有限从内存中获取logo图片
                                    if (_ServerImages.ContainsKey(tempString))
                                    {
                                        tempBitmap = _ServerImages[tempString];
                                    }
                                    else
                                    {
                                        tempBitmap = (Bitmap)GetServerImage(tempString);

                                        if (!_ServerImages.ContainsKey(tempString))
                                        {
                                            _ServerImages.Add(tempString, tempBitmap);
                                        }
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        if (Path.IsPathRooted(tempString))
                                        {
                                            tempString = Path.GetFileName(tempString);
                                        }
                                        tempBitmap = new Bitmap(Comm._Nurse_ImagePath + @"\" + tempString);
                                    }
                                    catch (Exception ex)
                                    {
                                        Comm.LogHelp.WriteErr("加载图片时异常，可能不存在在图片：" + Comm._Nurse_ImagePath + @"\" + tempString + "\r\n" + ex.StackTrace);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ShowInforMsg("加载图片出错，请确认配置：" + xnlEach.OuterXml);
                                Comm.LogHelp.WriteErr(ex);
                            }
                        }//if 静态图片
                        else if (isVector) //(xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.VectorName)).Trim() != "" &&
                        {//else 矢量图
                            # region 矢量图的时候

                            //显示只有底图的矢量图片，如果有数据，在载入本页数据的时候，再替换成有图层的图片
                            if (RunMode)
                            {//if RunMode
                                //try
                                //{
                                //    tempString = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.VectorName)).Trim();
                                //    DataTable dtVector = GetBackGround_Predefined(tempString);
                                //    if (dtVector != null && dtVector.Rows.Count > 0)
                                //    {
                                //        //含有标题，和矢量图信息 ：两列
                                //        //第三个字段为v_predefined_title 为null的表示底图，不为null的表示预定义区域的数据记录。
                                //        if (dtVector.Columns.Contains("v_predefined_title") && dtVector.Columns.Contains("v_figure"))
                                //        {
                                //            tempVectorControl = new VectorControl(); //先实例化控件
                                //            tempVectorControl._VectorName = tempString;

                                //            //矢量图画笔设置，粗细颜色：PenVector="SteelBlue,1F"
                                //            if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.PenVector)).Trim() == "")
                                //            {
                                //                tempVectorControl._PenVector = new Pen(Color.SteelBlue, 2.5f);  //底图画笔大小 默认
                                //            }
                                //            else
                                //            {
                                //                string[] penArr = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.PenVector)).Trim().Split(',');
                                //                tempVectorControl._PenVector = new Pen(Color.FromName(penArr[0].Trim()), float.Parse(penArr[1].Trim()));
                                //            }

                                //            tempVectorControl.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                                //            //tempVectorControl._VectorName = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.VectorName)).Trim();  // "人体正面示意图ABC";


                                //            //排序
                                //            dtVector.DefaultView.Sort = string.Format("v_user ASC");
                                //            dtVector = dtVector.DefaultView.ToTable(); //dtVector.DefaultView.ToTable().Copy();

                                //            SetPredefined(tempVectorControl, dtVector); //更新矢量图的自定义数组变量

                                //            # region 改用，调用共用方法SetPredefined来实现
                                //            //if (dtVector.Rows.Count == 1) //至少有一条数据配置矢量底图
                                //            //{
                                //            //    tempVectorControl._VectorConfig = dtVector.Rows[0]["v_figure"].ToString();
                                //            //    tempVectorControl._ArrPredefinedTitle = null;
                                //            //    tempVectorControl._ArrPredefinedBody = null;
                                //            //}
                                //            //else
                                //            //{
                                //            //    tempVectorControl._ArrPredefinedTitle = new string[dtVector.Rows.Count - 1]; //私有的自定义区域，开头加星号*
                                //            //    tempVectorControl._ArrPredefinedBody = new string[dtVector.Rows.Count - 1];
                                //            //    int indexPredefined = 0;

                                //            //    for (int k = 0; k < dtVector.Rows.Count; k++)
                                //            //    {
                                //            //        if (string.IsNullOrEmpty(dtVector.Rows[k]["v_predefined_title"].ToString()))
                                //            //        {
                                //            //            //底图的时候，底图肯定有，预定义区域可能没有，就为null
                                //            //            tempVectorControl._VectorConfig = dtVector.Rows[k]["v_figure"].ToString();
                                //            //        }
                                //            //        else
                                //            //        {
                                //            //            //预定义区域的时候
                                //            //            tempVectorControl._ArrPredefinedTitle[indexPredefined] = dtVector.Rows[k]["v_predefined_title"].ToString();
                                //            //            tempVectorControl._ArrPredefinedBody[indexPredefined] = dtVector.Rows[k]["v_figure"].ToString();

                                //            //            indexPredefined++;
                                //            //        }
                                //            //    }
                                //            //}
                                //            # endregion 改用，调用共用方法SetPredefined来实现

                                //            if (!isPrintlAll) // --没有数据，也能绘制默认的底图
                                //            {
                                //                //_CurrentEditVector = tempVectorControl;
                                //                tempBitmap = (Bitmap)InitVector(false, tempVectorControl, true); //默认图片，只要底图，不要图层信息
                                //            }
                                //        }
                                //        else
                                //        {
                                //            ShowInforMsg("1.请确认矢量图数据表中的【表结构】配置是否正确，Table：VECTOR_LIBRARY", true);
                                //        }
                                //    }
                                //    else
                                //    {
                                //        ShowInforMsg("2.请确认矢量图数据表中的配置【数据】是否正确，Table：VECTOR_LIBRARY", true);
                                //    }
                                //}
                                //catch (Exception ex)
                                //{
                                //    Comm.Logger.WriteErr(ex);
                                //    MessageBox.Show("3.请确认矢量图数据表中的配置【数据】是否正确，Table：VECTOR_LIBRARY");
                                //}
                            }//if RunMode
                            else
                            {//else RunMode
                                //单机版的时候 & 生成区域图片的时候(runmode也是False)
                                tempVectorControl = new VectorControl(); //先实例化控件

                                //矢量图画笔设置，粗细颜色：PenVector="SteelBlue,1F"
                                //if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.PenVector)).Trim() == "")
                                //{
                                //    tempVectorControl._PenVector = new Pen(Color.SteelBlue, 2.5f);  //底图画笔大小 默认
                                //}
                                //else
                                //{
                                //    string[] penArr = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.PenVector)).Trim().Split(',');
                                //    tempVectorControl._PenVector = new Pen(Color.FromName(penArr[0].Trim()), float.Parse(penArr[1].Trim()));
                                //}

                                tempVectorControl.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));
                                //tempVectorControl._VectorName = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.VectorName)).Trim();  // "人体正面示意图ABC";
                                if (tempVectorControl.VectorImageInfo == null)
                                {
                                    tempVectorControl.VectorImageInfo = new VectorImageInfo();
                                }

                                tempVectorControl.VectorImageInfo.OriginalName = tempString;
                                //tempVectorControl.VectorImageInfo.PicId = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.PicID));

                                //方便扩展，单机版和生成区域图片的默认底片信息，从属性获取
                                //if (string.IsNullOrEmpty((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.VectorBackGround))))
                                //{
                                //    tempVectorControl._VectorConfig = "(300,300)¤" +
                                //                    "人体正面示意图ABC[10,10]|右下对角线[10,30][30,30][30,40][10,40]|三角形[10,50][30,60][10,70]¤" + //有字就填充阴影
                                //                    "[100,74,40,0]" +
                                //                    "[74,115,74,115,19,133,19,133]" +
                                //                    "[19,133,5,150,5,150,25,161]" +
                                //                    "[25,161,25,161,67,148,67,148]" +
                                //                    "[67,148,67,148, 67,246,67,246]" +
                                //                    "[124,114,124,114,181,133,181,133]" +
                                //                    "[181,133,195,150,195,150,175,161]" +
                                //                    "[175,161,175,161,133,148,133,148]" +
                                //                    "[133,148,133,148,133,246,133,246]";
                                //}
                                //else
                                //{
                                //    tempVectorControl._VectorConfig = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.VectorBackGround));
                                //}

                                //tempVectorControl._ImageBorder = "";

                                //tempVectorControl._ArrPredefinedTitle = null;
                                //tempVectorControl._ArrPredefinedBody = null;

                                if (!isPrintlAll)    //生成区域的所有页图片，下面不用执行
                                {
                                    //_CurrentEditVector = tempVectorControl;
                                    tempBitmap = (Bitmap)InitVector(false, tempVectorControl, true);
                                }
                            }//else RunMode

                            if (tempVectorControl == null)
                            {
                                continue;
                            }

                            # endregion 矢量图的时候
                        }//else 矢量图

                        if (tempBitmap == null && tempVectorControl == null)
                        {
                            continue;
                        }

                        //打印的时候
                        if (!addControlFlg || isPrintlAll)
                        {
                            //静态logo图、矢量图，在遍历的时候不绘制的。
                            if (tempVectorControl != null) //是矢量图的时候
                            {
                                if (isPrintlAll)
                                {
                                    //先获取数据设置好
                                    if (_PrintAll_PageNode != null && !string.IsNullOrEmpty((_PrintAll_PageNode as XmlElement).GetAttribute(tempVectorControl.Name)))
                                    {

                                        //arrRtbeValue = ((_PrintAll_PageNode as XmlElement).GetAttribute(tempVectorControl.Name)).Split('§');
                                        //tempVectorControl._VectorConfig = arrRtbeValue[0];
                                        //tempVectorControl._ImageBorder = arrRtbeValue[1];
                                        tempVectorControl.VectorImageInfo.PicId = ((_PrintAll_PageNode as XmlElement).GetAttribute(tempVectorControl.Name));
                                        tempBitmap = (Bitmap)InitVector(true, tempVectorControl, true); //获取好图片，下面再绘制
                                    }
                                    else
                                    {
                                        //tempBitmap = (Bitmap)InitVector(false, tempVectorControl); //
                                        if (tempBitmap == null)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    continue; //矢量图打印当前页和生成当前页图片的时候，在遍历的时候绘制；这里跳过
                                }
                            }

                            //不是添加控件，那么只需要绘制静态背景
                            //打印所有页和生成图片的时候
                            tempString = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size)).Trim();
                            if (tempString == "")
                            {
                                tempSize = tempBitmap.Size;
                            }
                            else
                            {
                                tempSize = Comm.getSize(tempString).ToSize();
                            }

                            g.DrawImage(tempBitmap,
                                        Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y,
                                        tempSize.Width, tempSize.Height);

                            if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).ToLower().Contains("fixedsingle"))
                            {
                                g.DrawRectangle(new Pen(Color.Black, 0.4f),
                                            Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y,
                                            tempSize.Width, tempSize.Height);
                            }
                        }
                        else
                        {
                            if (tempVectorControl == null) //静态logo图片的时候
                            {
                                //静态图
                                picNew = new PictureBox();

                                picNew.BackgroundImage = tempBitmap;

                                picNew.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));

                                picNew.Location = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y);

                                tempString = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size)).Trim();
                                if (tempString == "")
                                {
                                    picNew.Size = picNew.BackgroundImage.Size;
                                }
                                else
                                {
                                    picNew.Size = Comm.getSize(tempString).ToSize();
                                }

                                tempString = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Style)).Trim();

                                if (tempString == "")
                                {
                                    picNew.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
                                    picNew.BorderStyle = System.Windows.Forms.BorderStyle.None;
                                }
                                else
                                {
                                    arr = tempString.Split(',');

                                    if (arr[0].ToLower() == "zoom")
                                    {
                                        picNew.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
                                    }
                                    else
                                    {
                                        picNew.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                                    }

                                    if (arr[1].ToLower() == "fixedsingle")
                                    {
                                        picNew.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                                    }
                                    else
                                    {
                                        picNew.BorderStyle = System.Windows.Forms.BorderStyle.None;
                                    }
                                }

                                picNew.Click += new System.EventHandler(this.GoOnPrintLocation_Click);

                                this.pictureBoxBackGround.Controls.Add(picNew);
                            }
                            else
                            {
                                //矢量图的时候
                                //tempVectorControl.UpdateVector(tempBitmap);
                                tempVectorControl.BackgroundImage = tempBitmap; //上面已经指定了图片
                                tempVectorControl.Tag = "yes"; //复制只有底图的数据，以备重置的时候还原
                                //tempVectorControl._ResetImage = (Image)tempVectorControl.BackgroundImage.Clone();
                                tempVectorControl.BackgroundImageLayout = ImageLayout.Zoom;

                                //tempVectorControl._BorderUserConfig = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.BorderUserConfig));       //颜色
                                //tempVectorControl._BorderDenoteDiction = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.BorderDenoteDiction)); //用语

                                tempVectorControl.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name));

                                tempVectorControl.Location = new Point(Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 0) + _OriginPoint.X, Comm.getSplitValue((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)), 1) + _OriginPoint.Y);

                                tempString = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Size)).Trim();
                                if (tempString == "")
                                {
                                    tempVectorControl.Size = tempVectorControl.Image.Size;
                                }
                                else
                                {
                                    tempVectorControl.Size = Comm.getSize(tempString).ToSize();
                                }

                                tempVectorControl.DoubleClick += new EventHandler(VectorControl_DoubleClick);  //双击打开编辑窗口的事件

                                tempVectorControl.Click += new System.EventHandler(this.GoOnPrintLocation_Click);

                                this.pictureBoxBackGround.Controls.Add(tempVectorControl);

                                //Clipboard.SetImage(tempVectorControl.BackgroundImage); //调试贴到Excel中查看
                            }
                        }

                        break;
                    #endregion
                    case nameof(EntXmlModel.Vector)://躯体感受
                        #region Vector                       
                        if (!addControlFlg)
                        {
                            continue; //不是添加控件，那么只需要绘制静态背景
                        }

                        tempString = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Default)).Trim();
                        tempBitmap = null;
                        //logo图片nameof(EntXmlModel.Default)不为空，矢量图nameof(EntXmlModel.VectorName)不为空。  //一下到吗是拷贝的

                        if (tempString != "")
                        {
                            try
                            {
                                if (RunMode)
                                {
                                    //如果内容中已经有的话，有限从内存中获取logo图片
                                    if (_ServerImages.ContainsKey(tempString))
                                    {
                                        tempBitmap = _ServerImages[tempString];
                                    }
                                    else
                                    {
                                        tempBitmap = (Bitmap)GetServerImage(tempString);

                                        if (!_ServerImages.ContainsKey(tempString))
                                        {
                                            _ServerImages.Add(tempString, tempBitmap);
                                        }
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        tempBitmap = new Bitmap(Comm._Nurse_ImagePath + @"\" + tempString);
                                    }
                                    catch (Exception ex)
                                    {
                                        Comm.LogHelp.WriteErr("加载图片时异常，可能不存在在图片：" + Comm._Nurse_ImagePath + @"\" + tempString + "\r\n" + ex.StackTrace);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ShowInforMsg("加载图片出错，请确认配置：" + xnlEach.OuterXml);
                                Comm.LogHelp.WriteErr(ex);
                            }
                        }

                        //实例化矢量图控件
                        tempVector = new Vector(xnlEach, tempBitmap, this);

                        tempVector.Name = (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.Name)).Trim();
                        tempVector.Location = Comm.getPointInt((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.LocationBegin)));

                        if (!isPrintlAll)
                        {
                            tempVector.Click += new System.EventHandler(this.GoOnPrintLocation_Click);

                            //添加控件
                            this.pictureBoxBackGround.Controls.Add(tempVector);
                        }
                        else
                        {
                            //打印所有页的时候，直接绘制
                            //arrRtbeValue = ((_PrintAll_PageNode as XmlElement).GetAttribute(chkbNew.Name)).Split('¤');
                            if (_PrintAll_PageNode != null)
                            {
                                tempVector.LoadData(((_PrintAll_PageNode as XmlElement).GetAttribute(tempVector.Name)));
                            }

                            g.DrawImage(tempVector.GetDataToPic(), tempVector.Location);

                        }

                        break;
                    #endregion
                    default:
                        //Comm.Logger.WriteErr();
                        ShowInforMsg("模板配置文件中遇到无法识别的表单标签：" + xnlEach.OuterXml);
                        break;
                }
            }

            //绘制表格线之前，将底图保存备份，以便刷新时再使用
            //初始化后，将背景保存到bmpBack
            if (addControlFlg)
            {
                if (isPrintlAll)
                {
                    //打印所有页，认为所有页的表格配置都是一样的
                    //continue;
                }
                else
                {
                    _BmpBack = (Bitmap)pictureBoxBackGround.Image.Clone(); //打印的时候不需要备份
                }
            }
            else
            {
                //打印当前页/刷新页的时候，绘制表格的基础线
                if (_TableType && _TableInfor != null) // 根据解析本页的数据，形成本页的表格，进行绘制。这里还是默认的表格线。
                {
                    //绘制表格的基础线
                    DrawTableBaseLines(g, _TableInfor);
                }
            }

            if (gDisposeFlg)
            {
                g.Dispose();
            }

            //释放句柄
            if (Comm._isCreatReportImages)
            {
                //if (tempVectorControl != null && !tempVectorControl.IsDisposed) //新版的矢量图
                //{
                //    tempVectorControl.Dispose();
                //}

                //if (tempVector != null && !tempVector.IsDisposed) //简洁版的矢量图，方便操作，
                //{
                //    tempVector.Dispose();
                //}

                //if (tempGeneticMap != null && !tempGeneticMap.IsDisposed) //遗传图谱
                //{
                //    tempGeneticMap.Dispose();
                //}

                //if (lblNew != null && !lblNew.IsDisposed)
                //{
                //    lblNew.Dispose();
                //}

                //lblNew = null;
                //rtbeNew = null;
                //rtbeNewTransparent = null;
                //chkbNew = null;
                //combNew = null;
                //picNew = null;
                //tableInforNew = null;
            }

            tempString = null;
            xnlEachCol = null;
            arr = null;
            arrRtbeValue = null;
            arrEachItem = null;
            arrEachItemTime = null;

            //tempBitmap = null;
            //tempVectorControl = null;  
            //tempVector = null;                      //简洁版的矢量图，方便操作，
            //tempGeneticMap = null;       //遗传图谱

            //lblNew = null;
            //rtbeNew = null;
            //rtbeNewTransparent = null;
            //chkbNew = null;
            //combNew = null;
            //picNew = null;
            //tableInforNew = null;
        }

        /// <summary>
        /// 设置Tab键索引
        /// </summary>
        /// <param name="xnlEach"></param>
        /// <param name="cl"></param>
        private void SetTabIndex(XmlNode xnlEach, Control cl)
        {
            //光标设置，默认是0表示空，即不设置，那么不设置，根据模板中的顺序（编辑器保存的时候按照坐标自动排序）
            if ((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.TabIndex)).Trim() != ""
                && (xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.TabIndex)).Trim() != "0")
            {
                int tabindex = 0;
                if (int.TryParse((xnlEach as XmlElement).GetAttribute(nameof(EntXmlModel.TabIndex)).Trim(), out tabindex))
                {
                    //tabindex = 0;
                    cl.TabIndex = tabindex;
                }
            }
        }

        # endregion 从模板配置文件，加载模板内容，显示表单到界面

        /// <summary>
        /// 绘制选择区域
        /// </summary>
        private void DrawRectangle()
        {
            Rectangle rectSelected = _MouseRect;

            ////如果是表格，那么要将选择矩形框自动要单元格边界
            //复制多个单元格，那么：1.首先是选择区域的样式，2.二维数组存储后，进行粘贴
            if (_TableType && _TableInfor != null)
            {
                //矩形框的四个点，去计算所在单元格的边界点
                rectSelected = MouseRectAutoCell(rectSelected);
            }


            Rectangle rect = this.RectangleToScreen(rectSelected);


            //ControlPaint.FillReversibleRectangle(new Rectangle(rect.X + pictureBoxBackGround.Left + this.tabControl1.Left + this.toolStripContainer1.ContentPanel.Left + this.tabPage1.Left + 5,
            //    rect.Y + pictureBoxBackGround.Top + this.tabControl1.Top + this.toolStripContainer1.ContentPanel.Top + this.tabPage1.Top + 4, rect.Width, rect.Height), SystemColors.ControlLight);


            Rectangle currentDrawRectangle = new Rectangle(rect.X + pictureBoxBackGround.Left + 5,
                rect.Y + pictureBoxBackGround.Top + 4, rect.Width, rect.Height);


            //选择的矩形边框
            //ControlPaint.DrawReversibleFrame(currentDrawRectangle, Color.DarkBlue, FrameStyle.Dashed);


            Rectangle currentDrawRectangleDrawFocus = new Rectangle(rectSelected.X + 1,
                                                                    rectSelected.Y + 1,
                                                                    rectSelected.Width - 3,
                                                                    rectSelected.Height - 3);

            Graphics g = this.pictureBoxBackGround.CreateGraphics();
            g.SmoothingMode = SmoothingMode.AntiAlias;

            #region 直接在画布上绘制
            if (currentDrawRectangleDrawFocus.Location.X > 1 && currentDrawRectangleDrawFocus.Location.Y > 1)
            {
                //复制提醒标记✚
                ControlPaint.DrawStringDisabled(
                    g, "复制", new Font("宋体", 9),
                    Color.Gold, new Rectangle(currentDrawRectangleDrawFocus.Location, new Size(32, 32)), StringFormat.GenericDefault);

                ////不透明啊，不行。合并单元格用这个效果就很赞了。
                //ControlPaint.DrawContainerGrabHandle(this.pictureBoxBackGround.CreateGraphics(), currentDrawRectangleDrawFocus);
                //比上面的刷新样要好
                //ControlPaint.DrawFocusRectangle(this.pictureBoxBackGround.CreateGraphics(), currentDrawRectangleDrawFocus); //,Color.Red, Color.LightBlue

                //这个边框效果，是目前最好的
                ControlPaint.DrawBorder(g, currentDrawRectangleDrawFocus, Color.Red, ButtonBorderStyle.Dashed);

                g.Dispose();  //这样绘制会导致刷新闪烁


                //ControlPaint.DrawGrabHandle(this.pictureBoxBackGround.CreateGraphics(), currentDrawRectangleDrawFocus, true, false);
                //ControlPaint.Light(Color.LightBlue);
            }
            #endregion 直接在画布上绘制

            _LastDrawRectangle = currentDrawRectangleDrawFocus;// currentDrawRectangle;



            if (_PictureBoxSelectArea != null && !_PictureBoxSelectArea.IsDisposed)
            {
                _PictureBoxSelectArea.Location = currentDrawRectangleDrawFocus.Location;
                _PictureBoxSelectArea.Size = currentDrawRectangleDrawFocus.Size;

                if (!this.pictureBoxBackGround.Controls.Contains(_PictureBoxSelectArea))
                {
                    this.pictureBoxBackGround.Controls.Add(_PictureBoxSelectArea);
                }

                this.pictureBoxBackGround.BringToFront();
            }
        }

        #region 根据内容进行绘制输入框
        private void SetPrintRtbeValue(string[] arrRtbeValue, RichTextBoxExtended rtbeNew, Graphics g)
        {
            //arrRtbeValue = ((_PrintAll_PageNode as XmlElement).GetAttribute(rtbeNew.Name)).Split('¤');
            bool tempBool = false;

            if (arrRtbeValue.Length > 1)
            {
                if (!string.IsNullOrEmpty(arrRtbeValue[4]))
                {
                    //如果存在富文本，那么就会有值；否则为空
                    rtbeNew._IsRtf = true;
                    rtbeNew.Rtf = arrRtbeValue[4];
                }
                else
                {
                    rtbeNew._IsRtf = false;
                    rtbeNew.Text = arrRtbeValue[0];

                    if (rtbeNew._InputBoxAlignment != HorizontalAlignment.Left)
                    {
                        rtbeNew.SetAlignment();
                    }
                }


                //手膜签名：公共的rtf数据设置过来
                if (string.IsNullOrEmpty(arrRtbeValue[0].Trim()) && string.IsNullOrEmpty(arrRtbeValue[4].Trim()) && !string.IsNullOrEmpty(arrRtbeValue[3]) && (rtbeNew.Name.StartsWith("签名") || rtbeNew.Name.StartsWith("记录者")))
                {
                    //先从数据文件中根据用户id是否存储了签名的手摸图片
                    XmlNode fingerPrintSignImages = _RecruitXML.SelectSingleNode("//" + EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.FingerPrintSignImages)));

                    //数据文件中没有手摸图片节点，就先添加
                    if (fingerPrintSignImages == null)
                    {
                        //插入节点
                        XmlElement images;
                        images = _RecruitXML.CreateElement(nameof(EntXmlModel.FingerPrintSignImages));
                        _RecruitXML.SelectSingleNode("//" + nameof(EntXmlModel.NurseForm)).AppendChild(images);

                        fingerPrintSignImages = images;
                    }

                    //先判断数据中有没有手膜签名图片
                    XmlNode imgRtfNode = fingerPrintSignImages.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), arrRtbeValue[3], "=", nameof(EntXmlModel.Image)));  //"Image[@ID='" + arrRtbeValue[3] + "']"
                    if (imgRtfNode != null)
                    {
                        rtbeNew._IsRtf = true;
                        rtbeNew.Rtf = (imgRtfNode as XmlElement).GetAttribute(nameof(EntXmlModel.Value));
                    }
                }

                //双红线
                if (!bool.TryParse(arrRtbeValue[1], out tempBool))
                {
                    tempBool = false;
                }
                rtbeNew._HaveSingleLine = tempBool;

                if (!bool.TryParse(arrRtbeValue[2], out tempBool))
                {
                    tempBool = false;
                }
                rtbeNew._HaveDoubleLine = tempBool;

                //大于6表示有日期格式
                if (arrRtbeValue.Length > 6)
                {
                    if (rtbeNew.Name.StartsWith("日期"))
                    {
                        rtbeNew._IsSetValue = true;

                        if (arrRtbeValue[0] != "")
                        {
                            rtbeNew._YMD = arrRtbeValue[6];
                        }
                        else
                        {
                            rtbeNew._YMD = rtbeNew._YMD_Default;
                        }

                        rtbeNew._FullDateText = arrRtbeValue[0];

                        string showText = rtbeNew.GetShowDate();

                        if (showText != rtbeNew.Text)
                        {
                            HorizontalAlignment temp = rtbeNew.SelectionAlignment;
                            rtbeNew.Text = showText;
                            rtbeNew.SelectionAlignment = temp;
                        }

                        rtbeNew._IsSetValue = false;
                    }
                }

                _isCreatImages = false;
                printRtbeExtend(rtbeNew, g, rtbeNew.Location);
                _isCreatImages = true;
            }
            else
            {
                //格式不全的异常数据
                rtbeNew.Text = arrRtbeValue[0];

                if (rtbeNew._InputBoxAlignment != HorizontalAlignment.Left)
                {
                    rtbeNew.SetAlignment();
                }

                _isCreatImages = false;
                printRtbeExtend(rtbeNew, g, rtbeNew.Location);
                _isCreatImages = true;
            }
        }
        # endregion 根据内容进行绘制输入框

        # region 绘制单元格边框线操作
        /// <summary>
        /// 绘制单元格的上下左右 四条边线
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colorPara"></param>
        /// <param name="g"></param>
        /// <param name="tableInforPara"></param>
        private void DrawCellLine(int x, int y, Color colorPara, Graphics g, TableInfor tableInforPara)
        {
            Rectangle rect = tableInforPara.Cells[x, y].Rect;

            g.DrawRectangle(new Pen(colorPara, 1), rect);
        }

        private void DrawCellLineTopAndBottom(int x, int y, Graphics g, TableInfor tableInforPara)
        {
            DrawCellLeftLine(x, y, g, tableInforPara);
            DrawCellTopLine(x, y, g, tableInforPara);
        }

        private void HideCellTopLine(int x, int y)
        {
            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);

            Rectangle rect = _TableInfor.Cells[x, y].Rect;

            //g.DrawLine(new Pen(pictureBoxBackGround.BackColor, 1), new Point(rect.Location.X, rect.Location.Y + _TableInfor.Cells[x, y].CellSize.Height), new Point(rect.Location.X + _TableInfor.Cells[x, y].CellSize.Width, rect.Location.Y + _TableInfor.Cells[x, y].CellSize.Height));
            HideCellTopLine(x, y, g);

            //rf.Inflate(100f, 100f);
            rect.Inflate(2, 2);//扩大区域，结构放大
            pictureBoxBackGround.Invalidate(rect);

            pictureBoxBackGround.Update();


            ShowCellRTBE(x, y);//不然设置线条后，输入框会异常清空
        }

        private void HideCellTopLine(int x, int y, Graphics g)
        {
            Rectangle rect = _TableInfor.Cells[x, y].Rect;

            //g.DrawLine(new Pen(pictureBoxBackGround.BackColor, 1), new Point(rect.Location.X, rect.Location.Y + _TableInfor.Cells[x, y].CellSize.Height), new Point(rect.Location.X + _TableInfor.Cells[x, y].CellSize.Width, rect.Location.Y + _TableInfor.Cells[x, y].CellSize.Height));
            g.DrawLine(new Pen(pictureBoxBackGround.BackColor, 2f), new PointF(rect.Location.X + 0.5f, rect.Location.Y), new PointF(rect.Location.X - 0.5f + _TableInfor.Cells[x, y].CellSize.Width, rect.Location.Y));
        }


        /// <summary>
        /// 隐藏左侧边线
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void HideCellLeftLine(int row, int col)
        {
            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);

            Rectangle rect = _TableInfor.Cells[row, col].Rect;

            //g.DrawLine(new Pen(pictureBoxBackGround.BackColor, 1), new Point(rect.Location.X, rect.Location.Y + 1), new Point(rect.Location.X, rect.Location.Y + _TableInfor.Cells[row, col].CellSize.Height - 1));
            drawHideCellLeftLine(row, col, g);

            //rf.Inflate(100f, 100f);
            rect.Inflate(2, 2);//扩大区域，结构放大
            pictureBoxBackGround.Invalidate(rect);

            pictureBoxBackGround.Update();
        }

        private void drawHideCellLeftLine(int row, int col, Graphics g)
        {
            Rectangle rect = _TableInfor.Cells[row, col].Rect;
            g.DrawLine(new Pen(pictureBoxBackGround.BackColor, 1), new Point(rect.Location.X, rect.Location.Y + 1), new Point(rect.Location.X, rect.Location.Y + _TableInfor.Cells[row, col].CellSize.Height - 1));
        }

        /// <summary>
        /// 显示左侧边线
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void ShowCellLeftLine(int row, int col)
        {
            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);

            Rectangle rect = _TableInfor.Cells[row, col].Rect;

            //g.DrawLine(new Pen(_TableInfor.Cells[row, col].CellLineColor, 1), new Point(rect.Location.X, rect.Location.Y + 1), new Point(rect.Location.X, rect.Location.Y + _TableInfor.Cells[row, col].CellSize.Height - 1));
            DrawCellLeftLine(row, col, g, _TableInfor);

            //rf.Inflate(100f, 100f);
            rect.Inflate(2, 2);//扩大区域，结构放大
            pictureBoxBackGround.Invalidate(rect);

            pictureBoxBackGround.Update();
        }

        /// <summary>
        /// 绘制每个单元格的上、左 边线
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="g"></param>
        /// <param name="tableInforPara"></param>
        private void DrawCellLeftLine(int row, int col, Graphics g, TableInfor tableInforPara)
        {
            Rectangle rect = tableInforPara.Cells[row, col].Rect;

            g.DrawLine(new Pen(tableInforPara.Cells[row, col].CellLineColor, 1), new Point(rect.Location.X, rect.Location.Y), new Point(rect.Location.X, rect.Location.Y + tableInforPara.Cells[row, col].CellSize.Height));
        }

        /// <summary>
        /// 隐藏侧边线
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void HideCellBottomLine(int row, int col)
        {
            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);

            Rectangle rect = _TableInfor.Cells[row, col].Rect;

            //g.DrawLine(new Pen(pictureBoxBackGround.BackColor, 1), new Point(rect.Location.X + 1, rect.Location.Y), new Point(rect.Location.X + _TableInfor.Cells[row, col].CellSize.Width - 1, rect.Location.Y));
            HideCellBottomLine(row, col, g, _TableInfor);

            //rf.Inflate(100f, 100f);
            rect.Inflate(2, 2);//扩大区域，结构放大
            pictureBoxBackGround.Invalidate(rect);

            pictureBoxBackGround.Update();
        }

        private void HideCellBottomLine(int row, int col, Graphics g, TableInfor tableInforPara)
        {
            Rectangle rect = tableInforPara.Cells[row, col].Rect;
            g.DrawLine(new Pen(pictureBoxBackGround.BackColor, 1), new PointF(rect.Location.X + 0.5f, rect.Location.Y), new PointF(rect.Location.X + tableInforPara.Cells[row, col].CellSize.Width - 0.5f, rect.Location.Y));
        }

        /// <summary>
        /// 显示上侧边线
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void ShowCellTopLine(int row, int col)
        {
            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);

            Rectangle rect = _TableInfor.Cells[row, col].Rect;

            //这里颜色，要去改单元格边框线的颜色
            //g.DrawLine(new Pen(_TableInfor.Cells[row, col].CellLineColor, 1), new Point(rect.Location.X + 1, rect.Location.Y), new Point(rect.Location.X + _TableInfor.Cells[row, col].CellSize.Width - 1, rect.Location.Y));
            DrawCellTopLine(row, col, g, _TableInfor);

            //rf.Inflate(100f, 100f);
            rect.Inflate(2, 2);//扩大区域，结构放大
            pictureBoxBackGround.Invalidate(rect);

            pictureBoxBackGround.Update();

            ShowCellRTBE(row, col);//不然设置线条后，输入框会异常清空
        }

        /// <summary>
        /// 绘制单元格的上边框线
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="g"></param>
        /// <param name="tableInforPara"></param>
        private void DrawCellTopLine(int row, int col, Graphics g, TableInfor tableInforPara)
        {
            Rectangle rect = tableInforPara.Cells[row, col].Rect;
            g.DrawLine(new Pen(tableInforPara.Cells[row, col].CellLineColor, 1), new Point(rect.Location.X, rect.Location.Y), new Point(rect.Location.X + tableInforPara.Cells[row, col].CellSize.Width, rect.Location.Y));
        }

        #region 设置单元格边框线操作
        /// <summary>
        /// 设置单元格上下左右 四条边框线颜色
        /// </summary>
        private void SetCellLine(int x, int y, Color colorPara)
        {
            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);

            DrawCellLine(x, y, colorPara, g, _TableInfor);

            Rectangle rect = _TableInfor.Cells[x, y].Rect;
            //g.DrawRectangle(new Pen(colorPara, 1), rect);

            _TableInfor.Cells[x, y].CellLineColor = colorPara; //将单元格边框线设置，保存到table信息中

            rect.Inflate(2, 2);//扩大区域，结构放大 //rf.Inflate(100f, 100f);
            pictureBoxBackGround.Invalidate(rect);

            pictureBoxBackGround.Update();

            ShowCellRTBE(x, y);//重新显示单元格输入框，不然设置线条后，输入框会异常清空
        }
        # endregion 设置单元格边框线操作

        /// <summary>
        /// 设置单元格的对角线
        /// </summary>
        private void SetCellLineZN(int x, int y, string typePara, bool showRtbe)
        {
            //_CurrentCellIndex

            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);

            Rectangle rect = _TableInfor.Cells[x, y].Rect;

            DrawLineZN(x, y, g, typePara, _TableInfor);
            //rf.Inflate(100f, 100f);
            rect.Inflate(2, 2);//扩大区域，结构放大
            pictureBoxBackGround.Invalidate(rect);

            pictureBoxBackGround.Update();

            if (showRtbe)
            {
                ShowCellRTBE(x, y);//不然设置线条后，输入框会异常清空
            }
        }

        /// <summary>
        /// 绘制单元格的对角线
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="g"></param>
        /// <param name="typePara"></param>
        /// <param name="tableInforPara"></param>
        private void DrawLineZN(int x, int y, Graphics g, string typePara, TableInfor tableInforPara)
        {
            //Rectangle rect = _TableInfor.CurrentCell.Rect;
            Rectangle rect = tableInforPara.Cells[x, y].Rect;

            if (typePara == "z")
            {
                g.DrawLine(new Pen(Color.Red, 1), new Point(rect.Location.X, rect.Location.Y + tableInforPara.Cells[x, y].CellSize.Height), new Point(rect.Location.X + tableInforPara.Cells[x, y].CellSize.Width, rect.Location.Y));
            }
            else
            {
                g.DrawLine(new Pen(Color.Red, 1), rect.Location, new Point(rect.Location.X + tableInforPara.Cells[x, y].CellSize.Width, rect.Location.Y + tableInforPara.Cells[x, y].CellSize.Height));
            }
        }

        /// <summary>
        /// 设置行边框线颜色
        /// </summary>
        private void SetRowLine(int row, int col, Color colorPara)
        {
            //_CurrentCellIndex

            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);

            Rectangle rect = new Rectangle(_TableInfor.Cells[row, 0].Loaction, _TableInfor.Rows[row].RowSize);

            //g.DrawRectangle(new Pen(colorPara, 1), rect);

            DrawRowLine(row, col, colorPara, g, _TableInfor);

            rect.Inflate(2, 2);//扩大区域，结构放大
            pictureBoxBackGround.Invalidate(rect);

            pictureBoxBackGround.Update();

            ShowCellRTBE(row, col);
        }

        /// <summary>
        /// 绘制表格的行线
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="colorPara"></param>
        /// <param name="g"></param>
        /// <param name="tableInforPara"></param>
        private void DrawRowLine(int row, int col, Color colorPara, Graphics g, TableInfor tableInforPara)
        {
            Rectangle rect = new Rectangle(tableInforPara.Cells[row, 0].Loaction, tableInforPara.Rows[row].RowSize);

            g.DrawRectangle(new Pen(colorPara, 1), rect);
        }

        /// <summary>
        /// 设置双红线
        /// </summary>
        private void SetCustomRowLine(int row, int col, LineTypes lineType)
        {
            SetCustomRowLine(row, col, lineType, GlobalVariable._RowLineType_Color_Selected);
        }

        private void SetCustomRowLine(int row, int col, LineTypes lineType, Color thisColor)
        {
            Graphics g = Graphics.FromImage(pictureBoxBackGround.Image);

            Rectangle rect = new Rectangle(_TableInfor.Cells[row, 0].Loaction, _TableInfor.Rows[row].RowSize);

            DrawCustomRowLine(row, col, lineType, g, _TableInfor, thisColor);

            rect.Inflate(2, 2);//扩大区域，结构放大
            pictureBoxBackGround.Invalidate(rect);

            pictureBoxBackGround.Update();

            ShowCellRTBE(row, col);
        }

        /// <summary>
        /// 绘制行的自定义线
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="lineType"></param>
        /// <param name="g"></param>
        /// <param name="tableInforPara"></param>
        /// <param name="colorPara">双红线颜色</param>
        private void DrawCustomRowLine(int row, int col, LineTypes lineType, Graphics g, TableInfor tableInforPara, Color colorPara)
        {
            Rectangle rect = new Rectangle(tableInforPara.Cells[row, 0].Loaction, tableInforPara.Rows[row].RowSize);

            int lineWidth = 1;
            Color lineColor = Color.Red;

            //根据配置指定双红线样式：颜色、宽度 _RowLineType_Color _RowLineType_Width
            lineWidth = _RowLineType_Width;
            lineColor = colorPara;  // _RowLineType_Color;

            //绘制各种双红线:
            if (lineType == LineTypes.RectLine)
            {
                //四周红线
                g.DrawRectangle(new Pen(lineColor, lineWidth), rect);
            }
            else if (lineType == LineTypes.RectDoubleLine)
            {
                //四周双红线
                g.DrawRectangle(new Pen(lineColor, lineWidth), rect);
                g.DrawRectangle(new Pen(lineColor, lineWidth), new Rectangle(new Point(rect.Location.X + lineWidth + 1, rect.Location.Y + lineWidth + 1),
                    new Size(rect.Size.Width - (lineWidth + 1) * 2, rect.Size.Height - (lineWidth + 1) * 2)));//往内测2像素,上个框本身宽度为1

                //防止单元格线挡住双红线中间,再绘制一条背景色的线
                g.DrawRectangle(new Pen(pictureBoxBackGround.BackColor, lineWidth), new Rectangle(new Point(rect.Location.X + lineWidth, rect.Location.Y + lineWidth),
                    new Size(rect.Size.Width - (lineWidth + 1), rect.Size.Height - (lineWidth + 1))));//往内测2像素,上个框本身宽度为1
            }
            else if (lineType == LineTypes.TopBottomLine)
            {
                //上下红线
                g.DrawLine(new Pen(lineColor, lineWidth), rect.Location, new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y));

                g.DrawLine(new Pen(lineColor, lineWidth), new Point(rect.Location.X, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height),
                    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height));
            }
            else if (lineType == LineTypes.TopBottomDoubleLine)
            {
                //上下双红线
                g.DrawLine(new Pen(lineColor, lineWidth), rect.Location, new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y));

                g.DrawLine(new Pen(lineColor, lineWidth), new Point(rect.Location.X, rect.Location.Y + lineWidth + 1), new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y + lineWidth + 1));

                g.DrawLine(new Pen(lineColor, lineWidth), new Point(rect.Location.X, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height),
                    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height));

                g.DrawLine(new Pen(lineColor, lineWidth), new Point(rect.Location.X, rect.Location.Y - lineWidth - 1 + tableInforPara.Rows[row].RowSize.Height),
                    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y - lineWidth - 1 + tableInforPara.Rows[row].RowSize.Height));

                //防止单元格线挡住双红线中间,再绘制一条背景色的线
                g.DrawLine(new Pen(pictureBoxBackGround.BackColor, lineWidth), new Point(rect.Location.X, rect.Location.Y + lineWidth), new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y + lineWidth));
                g.DrawLine(new Pen(pictureBoxBackGround.BackColor, lineWidth), new Point(rect.Location.X, rect.Location.Y - lineWidth + tableInforPara.Rows[row].RowSize.Height),
                    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y - lineWidth + tableInforPara.Rows[row].RowSize.Height));
            }
            else if (lineType == LineTypes.BottomLine)
            {
                //下红线
                g.DrawLine(new Pen(lineColor, lineWidth), new Point(rect.Location.X, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height),
                    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height));
            }
            else if (lineType == LineTypes.BottomDoubleLine)
            {
                //下双红线
                g.DrawLine(new Pen(lineColor, lineWidth), new Point(rect.Location.X, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height),
                    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height));

                g.DrawLine(new Pen(lineColor, lineWidth), new Point(rect.Location.X, rect.Location.Y - lineWidth - 1 + tableInforPara.Rows[row].RowSize.Height),
                    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y - lineWidth - 1 + tableInforPara.Rows[row].RowSize.Height));

                //防止单元格线挡住双红线中间,再绘制一条背景色的线
                g.DrawLine(new Pen(pictureBoxBackGround.BackColor, lineWidth), new Point(rect.Location.X, rect.Location.Y - lineWidth + tableInforPara.Rows[row].RowSize.Height),
                    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y - lineWidth + tableInforPara.Rows[row].RowSize.Height));
            }
            else if (lineType == LineTypes.TopLine)
            {
                //上红线
                g.DrawLine(new Pen(lineColor, lineWidth), rect.Location, new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y));

                //g.DrawLine(new Pen(lineColor, lineWidth), new Point(rect.Location.X, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height),
                //    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height));
            }
            else if (lineType == LineTypes.TopDoubleLine)
            {
                //上双红线
                g.DrawLine(new Pen(lineColor, lineWidth), rect.Location, new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y));

                g.DrawLine(new Pen(lineColor, lineWidth), new Point(rect.Location.X, rect.Location.Y + lineWidth + 1), new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y + lineWidth + 1));

                //g.DrawLine(new Pen(lineColor, lineWidth), new Point(rect.Location.X, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height),
                //    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y + tableInforPara.Rows[row].RowSize.Height));

                //g.DrawLine(new Pen(lineColor, lineWidth), new Point(rect.Location.X, rect.Location.Y - lineWidth - 1 + tableInforPara.Rows[row].RowSize.Height),
                //    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y - lineWidth - 1 + tableInforPara.Rows[row].RowSize.Height));

                //防止单元格线挡住双红线中间,再绘制一条背景色的线
                g.DrawLine(new Pen(pictureBoxBackGround.BackColor, lineWidth), new Point(rect.Location.X, rect.Location.Y + lineWidth), new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y + lineWidth));
                //g.DrawLine(new Pen(pictureBoxBackGround.BackColor, lineWidth), new Point(rect.Location.X, rect.Location.Y - lineWidth + tableInforPara.Rows[row].RowSize.Height),
                //    new Point(rect.Location.X + tableInforPara.Rows[row].RowSize.Width, rect.Location.Y - lineWidth + tableInforPara.Rows[row].RowSize.Height));
            }
            else if (lineType == LineTypes.None)
            {
                //不用处理
            }
        }

        # endregion 绘制单元格边框线操作
    }
}
