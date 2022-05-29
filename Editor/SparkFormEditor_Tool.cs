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
using System.Linq;

namespace SparkFormEditor
{
    public partial class SparkFormEditor
    {
        #region 工具栏单击事件
        private void toolStripButtonBold_Click(object sender, EventArgs e)
        {
            _IsLoading = true;

            //ToolStripButton thisItem = (ToolStripButton)sender;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                //thisItem.Checked = !thisItem.Checked;
                //_IsBold = !_IsBold;
                Font thisFont = GetFontDetails(((RichTextBoxExtended)_Need_Assistant_Control)); //获取选择内容的字体

                ChangeFontStyle(FontStyle.Bold, !thisFont.Bold);
                ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = true;

                thisFont.Dispose();
            }

            _IsLoading = false;
        }

        private void toolStripToolBarWord_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButtonItalic_Click(object sender, EventArgs e)
        {
            //if (_IsLoading) return;
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                Font thisFont = GetFontDetails(((RichTextBoxExtended)_Need_Assistant_Control)); //获取选择内容的字体

                ChangeFontStyle(FontStyle.Italic, !thisFont.Italic);
                ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = true;

                thisFont.Dispose();
            }



            _IsLoading = false;
        }

        private void toolStripButtonUnderline_Click(object sender, EventArgs e)
        {
            //if (_IsLoading) return;
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                Font thisFont = GetFontDetails(((RichTextBoxExtended)_Need_Assistant_Control)); //获取选择内容的字体

                ChangeFontStyle(FontStyle.Underline, !thisFont.Underline);
                ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = true;

                thisFont.Dispose();
            }



            _IsLoading = false;
        }

        private void toolStripButtonColor_Click(object sender, EventArgs e)
        {
            toolStripButtonColor.ShowDropDown();
        }

        private void toolStripButton文字颜色_Click(object sender, EventArgs e)
        {
            Color thisColor = Color.Black;

            if (sender != null)
            {
                string strColor = ((ToolStripMenuItem)sender).Text;

                if (strColor == "红色")
                {
                    thisColor = Color.Red;
                }
                else if (strColor == "绿色")
                {
                    thisColor = Color.Green;
                }
                else if (strColor == "蓝色")
                {
                    thisColor = Color.Blue;
                }
            }


            toolStripButtonColorBlack_Click(sender.ToString(), null);

        }

        private void toolStripButtonUp_Click(object sender, EventArgs e)
        {
            //if (_IsLoading) return;
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                //Font thisFont = GetFontDetails(((RichTextBoxExtended)_Need_Assistant_Control)); //获取选择内容的字体
                ToolStripMenuItem tsi = new ToolStripMenuItem();
                tsi.Checked = ((RichTextBoxExtended)_Need_Assistant_Control).IsSuperScript();

                ((RichTextBoxExtended)_Need_Assistant_Control).toolStripMenuItemUp_Click(tsi, null);
                ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = true;

                tsi.Dispose();
            }

            _IsLoading = false;
        }

        private void toolStripButtonDown_Click(object sender, EventArgs e)
        {
            //if (_IsLoading) return;
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                ToolStripMenuItem tsi = new ToolStripMenuItem();
                tsi.Checked = ((RichTextBoxExtended)_Need_Assistant_Control).IsSubScript();

                ((RichTextBoxExtended)_Need_Assistant_Control).toolStripMenuItemDown_Click(tsi, null);
                ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = true;

                tsi.Dispose();
            }


            _IsLoading = false;
        }

        private void toolStripButtonLeft_Click(object sender, EventArgs e)
        {
            //if (_IsLoading) return;
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                ((RichTextBoxExtended)_Need_Assistant_Control).SelectionAlignment = HorizontalAlignment.Left;

                ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = true;

                //toolStripButtonCenter.Checked = false;
                //toolStripButtonRight.Checked = false;
            }

            _IsLoading = false;
        }

        private void toolStripButtonCenter_Click(object sender, EventArgs e)
        {
            //if (_IsLoading) return;
            _IsLoading = true;

            ToolStripButton thisItem = (ToolStripButton)sender;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                ((RichTextBoxExtended)_Need_Assistant_Control).SelectionAlignment = HorizontalAlignment.Center;

                ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = true;

                //thisItem.Checked = true;
                //toolStripButtonLeft.Checked = false;
                //toolStripButtonRight.Checked = false;
            }

            _IsLoading = false;
        }

        private void toolStripButtonRight_Click(object sender, EventArgs e)
        {
            //if (_IsLoading) return;
            _IsLoading = true;

            ToolStripButton thisItem = (ToolStripButton)sender;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                ((RichTextBoxExtended)_Need_Assistant_Control).SelectionAlignment = HorizontalAlignment.Right;

                ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = true;

                //thisItem.Checked = true;
                //toolStripButtonLeft.Checked = false;
                //toolStripButtonCenter.Checked = false;
            }

            _IsLoading = false;
        }

        private void toolStripButtonStrikeout_Click(object sender, EventArgs e)
        {
            //if (_IsLoading) return;
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                Font thisFont = GetFontDetails(((RichTextBoxExtended)_Need_Assistant_Control)); //获取选择内容的字体

                ChangeFontStyle(FontStyle.Strikeout, !thisFont.Strikeout);
                ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = true;

                thisFont.Dispose();
            }


            _IsLoading = false;
        }

        private void toolStripButtonOneLine_Click(object sender, EventArgs e)
        {
            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                int index = ((RichTextBoxExtended)_Need_Assistant_Control).SelectionStart;    //记录修改时光标的位置
                ((RichTextBoxExtended)_Need_Assistant_Control).SelectAll();//操作后可以及时刷新界面

                //ToolStripButton thisItem = (ToolStripButton)sender;
                //thisItem.Checked = !thisItem.Checked;

                ((RichTextBoxExtended)_Need_Assistant_Control)._HaveSingleLine = !((RichTextBoxExtended)_Need_Assistant_Control)._HaveSingleLine;

                ((RichTextBoxExtended)_Need_Assistant_Control).Select(index, 0);//方便连续操作

                AddToDoubleList((RichTextBoxExtended)_Need_Assistant_Control);

                IsNeedSave = true;
                _CurrentCellNeedSave = true;
            }
        }

        private void toolStripButtonDoubleLine_Click(object sender, EventArgs e)
        {
            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                int index = ((RichTextBoxExtended)_Need_Assistant_Control).SelectionStart;    //记录修改时光标的位置
                ((RichTextBoxExtended)_Need_Assistant_Control).SelectAll();//操作后可以及时刷新界面

                //ToolStripButton thisItem = (ToolStripButton)sender;
                //thisItem.Checked = !thisItem.Checked;

                ((RichTextBoxExtended)_Need_Assistant_Control)._HaveDoubleLine = !((RichTextBoxExtended)_Need_Assistant_Control)._HaveDoubleLine;

                ((RichTextBoxExtended)_Need_Assistant_Control).Select(index, 0);

                AddToDoubleList((RichTextBoxExtended)_Need_Assistant_Control);

                IsNeedSave = true;
                _CurrentCellNeedSave = true;
            }
        }

        private void toolStripButtonClearFormat_Click(object sender, EventArgs e)
        {
            //if (_IsLoading) return;
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                ((RichTextBoxExtended)_Need_Assistant_Control).toolStripMenuItemClearFormat_Click(null, null);
                ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = false;

                IsNeedSave = true;
                _CurrentCellNeedSave = true;
            }


            _IsLoading = false;
        }

        private void toolStripButtonDefaultRowLine_Click(object sender, EventArgs e)
        {
            ToolStripSplitButton me = (ToolStripSplitButton)sender;
            if (!me.ButtonPressed)
            {
                return; //如果是点击下拉子菜单的时候，那么不要出发本身按钮的事件。
            }
            SetDefaultRowLine();
        }

        /// <summary>
        /// 行边框线设置
        /// 单元格边框线设置在单元格的信息，小太阳分割的索引4中
        /// 行线保存在ROW="Red¤None¤Blue¤Red"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemRow_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (!_TableType || _TableInfor.CurrentCell == null)
            {
                ShowInforMsg("请先选择行后再操作。");
                return;
            }

            ShowSearch(new Point(_TableInfor.Rows[_CurrentCellIndex.X].Loaction.X + 1, _TableInfor.Rows[_CurrentCellIndex.X].Loaction.Y + 3));//显示手指位置

            if (sender != null)
            {
                //toolStripButtonLine.HideDropDown();
                if ((sender as ToolStripSplitButton) != null)
                    (sender as ToolStripSplitButton).HideDropDown();
                //(sender as ToolStripDropDownItem).HideDropDown();
            }

            if (e != null)
            {
                if (MessageBox.Show("确定要对第 " + (_CurrentCellIndex.X + 1).ToString() + " 行：【" + e.ClickedItem.Text + "】操作吗？", "行边框线设置", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {

                }
                else
                {

                    return;
                }
            }
            else
            {
                DialogResult dialogResult = Comm.ShowQuestionMessage(string.Format("是否对第{0}行进行操作？", _CurrentCellIndex.X + 1), 2);
                if (dialogResult == DialogResult.No)
                    return;

            }

            string opName = "";
            if (e != null && e.ClickedItem != null)
            {
                opName = e.ClickedItem.Name;
            }
            else
            {
                opName = sender.ToString();
            }

            switch (opName) //switch (e.ClickedItem.Name)
            {

                //行的边框线：//OW="Black¤None¤Blue¤Red" 行信息：行线颜色，双红线类型，行文字颜色，双红线颜色
                case "toolStripMenuItemRowLineRed": //"行边框色：红":
                    SetRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, Color.Red);
                    _TableInfor.Rows[_CurrentCellIndex.X].RowLineColor = Color.Red;
                    break;
                case "toolStripMenuItemRowLineGreen": //"行边框色：绿":
                    SetRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, Color.Green);
                    _TableInfor.Rows[_CurrentCellIndex.X].RowLineColor = Color.Green;
                    break;
                case "toolStripMenuItemRowLineBlue": //"行边框色：蓝":
                    SetRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, Color.Blue);
                    _TableInfor.Rows[_CurrentCellIndex.X].RowLineColor = Color.Blue;
                    break;
                case "toolStripMenuItemRowLineBlack": //"行边框色：黑":
                    SetRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, Color.Black);
                    _TableInfor.Rows[_CurrentCellIndex.X].RowLineColor = Color.Black;
                    break;
                case "toolStripMenuItemRowLineDefault": //"行边框色：默认":
                    SetRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, Color.Black);
                    _TableInfor.Rows[_CurrentCellIndex.X].RowLineColor = Color.Black;
                    break;

                //双红线
                case "toolStripMenuItemRectLine": //"行四周红线":
                    SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.RectLine);
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.RectLine;
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = GlobalVariable._RowLineType_Color_Selected;
                    break;
                case "ToolStripMenuItemRectDoubleLine": //"行四周双红线":
                    SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.RectDoubleLine);
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.RectDoubleLine;
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = GlobalVariable._RowLineType_Color_Selected;
                    break;
                case "ToolStripMenuItemTopBottomLine": //"上下红线":
                    SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.TopBottomLine);
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.TopBottomLine;
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = GlobalVariable._RowLineType_Color_Selected;
                    break;
                case "ToolStripMenuItemTopBottomDoubleLine": //"上下双红线":
                    SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.TopBottomDoubleLine);
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.TopBottomDoubleLine;
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = GlobalVariable._RowLineType_Color_Selected;
                    break;
                case "ToolStripMenuItemBottomLine": //"下红线":
                    SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.BottomLine);
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.BottomLine;
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = GlobalVariable._RowLineType_Color_Selected;
                    break;
                case "ToolStripMenuItemBottomDoubleLine": //"下双红线":
                    SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.BottomDoubleLine);
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.BottomDoubleLine;
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = GlobalVariable._RowLineType_Color_Selected;
                    break;

                case "ToolStripMenuItemTopLine": //"上红线":
                    SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.TopLine);
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.TopLine;
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = GlobalVariable._RowLineType_Color_Selected;
                    break;
                case "ToolStripMenuItemTopDoubleLine": //"上双红线":
                    SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.TopDoubleLine);
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.TopDoubleLine;
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = GlobalVariable._RowLineType_Color_Selected;
                    break;

                case "ToolStripMenuItemBottomLineDefault": //"默认，清空":             
                    //SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.None);

                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.None;
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = GlobalVariable._RowLineType_Color_Selected;

                    //this.pictureBoxBackGround.Focus();
                    SaveToOp();

                    Redraw();
                    //RedrawRow(_TableInfor, _CurrentCellIndex.X, false, null);

                    ShowCellRTBE(_CurrentCellIndex.X, _CurrentCellIndex.Y);

                    break;

                case "toolStripButtonDefaultRowLine"://"下红线":
                    SetCustomRowLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, LineTypes.BottomLine);
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineType = LineTypes.BottomLine;
                    _TableInfor.Rows[_CurrentCellIndex.X].CustomLineColor = Variable.ToolStrip_LineColor.color;
                    break;
            }

            IsNeedSave = true;
            _CurrentCellNeedSave = true;
        }

        private void toolStripButtonDefaultRowLine_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //switch (e.ClickedItem.Name)
            //{
            //    //红色
            //    case "ToolStripMenuItemRed":
            //        GlobalVariable._RowLineType_Color_Selected = Color.Red;
            //        break;
            //    case "ToolStripMenuItemGreen":
            //        GlobalVariable._RowLineType_Color_Selected = Color.Green;
            //        break;
            //    case "ToolStripMenuItemRedBlue":
            //        GlobalVariable._RowLineType_Color_Selected = Color.Blue;
            //        break;
            //    case "ToolStripMenuItemBlack":
            //        GlobalVariable._RowLineType_Color_Selected = Color.Black;
            //        break;
            //}

            //SetRowLineMenuText(); //SetRowLineType();
        }

        private void toolStripButtonDefaultRowLine_DropDownOpening(object sender, EventArgs e)
        {

            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;
            colorDialog.FullOpen = true;
            colorDialog.ShowHelp = true;
            //colorDialog.Color = Color.Black;
            DialogResult dialogResult = colorDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                Variable.ToolStrip_LineColor.color = colorDialog.Color;
                SetRowLineMenuText();
            }
        }

        private void toolStripButtonRedo_Click(object sender, EventArgs e)
        {
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                ((RichTextBoxExtended)_Need_Assistant_Control).Redo();
            }

            _IsLoading = false;
        }

        private void toolStripButtonUndo_Click(object sender, EventArgs e)
        {
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                ((RichTextBoxExtended)_Need_Assistant_Control).Undo();
            }

            _IsLoading = false;
        }

        private void toolStripButtonCopy_Click(object sender, EventArgs e)
        {
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                ((RichTextBoxExtended)_Need_Assistant_Control).Copy();
            }

            _IsLoading = false;
        }

        private void toolStripButtonCut_Click(object sender, EventArgs e)
        {
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                ((RichTextBoxExtended)_Need_Assistant_Control).Cut();
            }

            _IsLoading = false;
        }

        private void toolStripButtonPaste_Click(object sender, EventArgs e)
        {
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                ((RichTextBoxExtended)_Need_Assistant_Control).Paste();
            }

            _IsLoading = false;
        }

        private void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            _IsLoading = true;

            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                ((RichTextBoxExtended)_Need_Assistant_Control).Clear();
            }

            _IsLoading = false;
        }

        //插入体温数据
        private void toolStripButtonTcData_Click(object sender, EventArgs e)
        {
            if (_TableType && _TableInfor != null &&
                _TableInfor.CurrentCell != null && this._patientInfo != null &&
                !this._TemplateID.IsNullOrEmpty())
            {
                var tcColsMapping = _TemplateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm)).GetAttributeValue(nameof(EntXmlModel.TcColsMapping));
                if (tcColsMapping.IsNullOrEmpty())
                {
                    Comm.ShowErrorMessage("未配置体温单字段映射,无法使用此功能,请联系管理员!");
                    return;
                }
                var arrs = tcColsMapping.Split('|');
                if (arrs.Length != 2) return;
                var tcCols = arrs[0].Split(',');
                var formCols = arrs[1].Split(',');
                if (tcCols.Length != formCols.Length) return;
                var args = new GlobalVariable.InsertTcDataEventArgs(this._patientInfo.PatientId, this._TemplateID);
                var ret = InsertTcData?.Invoke(this, args);
                if (ret == true && args.Data != null)
                {
                    //体温数据
                    //选中的数据
                    int index = -1;
                    int rowIndex = _TableInfor.CurrentCell.RowIndex;
                    foreach (var item in formCols)
                    {
                        index++;
                        string tcName = tcCols[index];
                        if (item.IsNullOrEmpty() || tcName.IsNullOrEmpty() || !args.Data.Table.HasColumn(tcName)) continue;
                        if (_TableInfor.ContainColName(item))
                        {
                            string value = $"{args.Data[tcName]}";
                            if (value.IsNullOrEmpty()) continue;
                            SetCellValue(rowIndex, item, value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 分类合计
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonSumGroup_Click(object sender, EventArgs e)
        {
            //插入一行,合并行(排除日期时间和签名)
            try
            {
                if (!_TableType) return;
                if (_TableType && _TableInfor.CurrentCell != null && _TableInfor.Vertical)
                {
                    MessageBox.Show("纵向表格不支持此功能。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                //不能合计的场合，没有日期列
                if (_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "日期") == null)
                {
                    ShowInforMsg("不包含【日期】列的表格，不能根据时间范围进行合计。");
                    return;
                }
                //&& (rtbe != null && rtbe._IsTable)
                if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
                {
                    RichTextBoxExtended rtbe = (RichTextBoxExtended)_Need_Assistant_Control;
                    if (!rtbe._IsTable) return;


                    // SumForm() 构造方法参数： DateTime dtFrom, List<string> columns, string defauleCol
                    //先计算，看看能否取到上次的合计数据，传入设置界面，自动选定时间范围
                    DateTime dtTo = GetServerDate();
                    DateTime dtFrom = dtTo.AddDays(-1);

                    //获取GroupSum的值
                    string groupMemo = "";
                    string groupSign = ",";
                    var groupSum = _TemplateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm)).GetAttributeValue(nameof(EntXmlModel.GroupSum));
                    List<Tuple<string, string>> listCols = new List<Tuple<string, string>>();
                    if (!groupSum.IsNullOrEmpty())
                    {
                        var groups = groupSum.Split('$');
                        if (groups.Length > 1)
                        {
                            var items = groups[1].Split('|');
                            if (items.Length >= 1)
                            {
                                groupMemo = items[0];
                            }
                            if (items.Length > 1)
                            {
                                groupSign = items[1];
                            }
                        }
                        var arrs = groups[0].Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        listCols = arrs.Select(a =>
                        {
                            string memo = "", col = "";
                            var split = a.Split(':');
                            if (split.Length >= 1)
                            {
                                memo = split[0];
                            }
                            if (split.Length > 1)
                            {
                                col = split[1];
                            }
                            if (col == "日期" || col == "时间" || col.Contains("签名"))
                            {
                                return null;
                            }
                            return Tuple.Create(memo, col);
                        }).Where(a => a != null).ToList();
                    }

                    string[] arr;




                    //"行线@BottomLine,Red¤结文字列@项目¤结文字集合@12小时小结|12小时总结|24小时总结¤过滤@mg|Mg";
                    //_SumSetting = "分类汇总@出量=入量|入量=入量¤结文字集合@24小时总结";
                    //string sumGroup = Comm.GetPropertyByName(_SumSetting, "分类汇总").Trim(); //分类汇总的时候，只要选择行，不需要制定某特定单元格


                    //根据护士的日常规律，自动设置时间范围：
                    //TODO：0.最优先的日期自动设置算法,是：取到上次的汇总时间往后开始；但是问题是上一次汇总的那个跳数据未必填写时间。

                    //1.如果当前行有日期时间，就用当前行的日期时间 //_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "日期");
                    //2.上一行有无日期时间
                    //3.系统默认的日期时间


                    //根据当前行位置，得到行最近的日期时间
                    XmlNode records = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                    //当前行节点
                    XmlNode thisNode = records.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString(), "=", nameof(EntXmlModel.Record)));
                    //获取当前行的日期
                    string dt = GetRowDateTime((thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                    try
                    {
                        arr = dt.Split('¤');

                        //护理部要求写入24:00
                        arr[1] = arr[1] == "24:00" ? "23:59" : arr[1];

                        if (arr[1].Trim() == "")
                        {
                            arr[1] = string.Format("{0:HH:mm}", GetServerDate());
                        }

                        dtTo = DateTime.Parse(arr[0] + " " + arr[1]);
                        dtFrom = dtTo.AddDays(-1);
                    }
                    catch
                    {
                        //无法获取正常的日期时间，就用设定的系统默认日期时间
                    }

                    //已经合计过的日期时间，最优先显示
                    arr = (thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.Total)).Split('¤');
                    if (arr.Length >= 3)
                    {
                        DateTime dt1 = GetServerDate();
                        DateTime dt2 = dt1;

                        if (DateTime.TryParse(arr[1], out dt1))
                        {
                            dtFrom = dt1.AddMinutes(-1);
                        }

                        if (DateTime.TryParse(arr[2], out dt2))
                        {
                            dtTo = dt2;
                        }
                    }

                    //指定的合计列，那么就不用光标选中的了 ¤合计列@出量1|出量2" 
                    //string sumColumns = string.Join("|", listCols.Select(a => a.Item2)); //Comm.GetPropertyByName(_SumSetting, "合计列").Trim();
                    //设置需要的列，那么全部选中
                    // arr = sumColumns.Split('|');
                    //显示配置的合计列的集合，并且是包含在表格中的。配置错的不在表格列中的名字，过滤掉
                    var listAllCols = _TableInfor.Columns.Select(a => a.ColName)
                        .Where(colName => colName != "日期" && colName != "时间" && !colName.Contains("签名")).ToList();
                    //defauleCol = "¤";  //表示全部选中

                    XmlNode recordsNode = records;

                    //更新xml：设计行的合计状态，以及显示该行是否已经是合计行。
                    string nodeId = _TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].ID.ToString();
                    XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId, "=", nameof(EntXmlModel.Record)));

                    //string sumWordCol = Comm.GetPropertyByName(_SumSetting, "结文字列").Trim();
                    //string sumedWord = "";     //已经合计过的行，结文字也传过去
                    //if (!string.IsNullOrEmpty(sumWordCol) && _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, sumWordCol) != null)
                    //{
                    //    sumedWord = (cellNode as XmlElement).GetAttribute(sumWordCol).Split('¤')[0];
                    //}

                    //设置列，传入设置界面，让用户选择
                    List<string> columns = listCols.Select(a => a.Item2).ToList();
                    var cellTotal = (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.Total));
                    var defaultCols = string.Join("|", listCols.Select(a => a.Item2));
                    //合计界面窗口
                    Frm_Sum sf = new Frm_Sum(dtFrom.AddMinutes(1), dtTo, listAllCols, defaultCols, cellTotal, "", "");

                    DialogResult res = sf.ShowDialog();

                    if (res == DialogResult.OK)
                    {
                        //获取返回的设置结果：
                        dtFrom = sf.DtFrom;
                        dtTo = sf.DtTo;
                        List<string> columnsList = sf.ColumnsList;

                        #region 分类合计汇总
                        //最初的合计，每次只能对一个单元格合计，虽然可以对多列合计。但是如果是出入量，就要合计两次。
                        //分类汇总@出量1=出量1,出量2,出量3|出量3=出量3
                        //string sumGroup = Comm.getPropertyByName(_SumSetting, "分类汇总").Trim();
                        SumMulti[] arrSumGroup = null;
                        arrSumGroup = columnsList.Select(a => new SumMulti()
                        {
                            Sum = 0,
                            SumItems = null,
                            SumName = a,
                            Memo = (listCols.FirstOrDefault(b => b.Item2 == a)?.Item1) ?? a,
                        }).ToArray();
                        #endregion 分类合计汇总

                        //根据合计条件和表单数据进行合计
                        if (recordsNode != null && recordsNode.ChildNodes.Count > 0)
                        {
                            XmlNode xn = null;
                            string eachValue = "";
                            double eachValueInt = 0;
                            string recordDt = "";
                            DateTime recordDtTime;
                            string tempDateTime = "";

                            //从当前行所在的节点，往前推
                            xn = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString(), "=", nameof(EntXmlModel.Record)));
                            bool isFirst = true;
                            while (xn != null)
                            {
                                if (isFirst) isFirst = false; else xn = xn.PreviousSibling;
                                //不为空，且日期时间在范围内的数据；并且为非合计、小计行(nameof(EntXmlModel.Total)非空，表示需要统计的明细数据)
                                if (xn != null && !Comm.isEmptyRecord(xn))
                                {
                                    /**************************ljf*2018-9-31*********************************/
                                    //因为之前合计过，但是又取消合计了，Total属性的标记一直存在，所以增加一次判断，有标记，但是合计的值是空的，不排除这个情况
                                    bool isTotal = (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) == "";
                                    /**************************************************************/

                                    //取到该条数据的对应的时间
                                    recordDt = GetRowDateTime((xn as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                                    if (recordDt != "¤") //防止异常情况下取不到这条数据的日期时间
                                    {
                                        #region 获取时间
                                        arr = recordDt.Split('¤');

                                        //防止只有日期，没有时间的表格
                                        if (arr[1].Trim() == "")
                                        {
                                            arr[1] = "00:00";
                                        }

                                        //护理部要求写入24:00
                                        arr[1] = arr[1] == "24:00" ? "23:59" : arr[1];

                                        //可能是界面上刚输入的，不合法的日期的行数据
                                        tempDateTime = arr[0] + " " + arr[1];
                                        if (!DateHelp.IsDateTime(tempDateTime))
                                        {
                                            //跳过错误数据
                                            ShowInforMsg("在列合计的时候，发现某行日期时间的格式不正确，忽略了该行明细数据：" + tempDateTime, true);
                                            continue;
                                        }
                                        else
                                        {
                                            recordDtTime = DateTime.Parse(tempDateTime);
                                        }
                                        #endregion

                                        //如果遍历到时间下限之前的数据，那么跳过
                                        if (recordDtTime.CompareTo(dtFrom) < 0 || recordDtTime.CompareTo(dtTo) > 0)
                                        {
                                            break;
                                        }

                                        //获取每行，每列的数据。如果需要过滤的，可以再这里过滤
                                        string fileter = Comm.GetPropertyByName(_SumSetting, "过滤").Trim();

                                        //在循环：返回的统计的各列，进行合计
                                        for (int j = 0; j < columnsList.Count; j++)
                                        {
                                            //判断该单元格是否被合并和空的跳过
                                            if (xn.GetAttributeValue(columnsList[j]) == "" ||
                                                xn.GetAttributeValue(columnsList[j]).Split('¤')[7].Contains("True"))
                                            {
                                                continue;
                                            }

                                            //过滤指定的数据内容的单元格
                                            if (!string.IsNullOrEmpty(fileter))
                                            {
                                                //需要过滤掉mg的，只要统计ml的
                                                if (Regex.IsMatch((xn as XmlElement).GetAttribute(columnsList[j]).Split('¤')[0], fileter))
                                                {
                                                    continue;
                                                }
                                            }

                                            eachValue = (xn as XmlElement).GetAttribute(columnsList[j]).Split('¤')[0]; // 取文本Text
                                            eachValueInt = StringNumber.getFirstNum(eachValue.Trim());
                                            if (eachValueInt != -1)
                                            {
                                                arrSumGroup.FirstOrDefault(a => a.SumName == columnsList[j])
                                                        .Sum += eachValueInt;
                                            }
                                        }
                                    }
                                }

                            }

                            //是合计，统计次数
                            if (sf.SumMode == 0)
                            {
                                int rowIndex = _TableInfor.CurrentCell.RowIndex + 1;
                                //向下插入行
                                if (TableInsertRowToDown(null) == false)
                                {
                                    return;
                                }

                                //日期,时间,签名
                                var b1 = _TableInfor.ContainColName("日期");
                                var b2 = _TableInfor.ContainColName("时间");
                                var b3 = _TableInfor.ContainColName("签名");
                                int count = _TableInfor.ColumnsCount - b1.IIF(1, 0) - b2.IIF(1, 0) - b3.IIF(1, 0) - 1;
                                int colIndex = b1.IIF(1, 0) + b2.IIF(1, 0);

                                MergeCellToRight(colIndex, rowIndex, count);

                                var cell = _TableInfor.Cells[rowIndex, colIndex];
                                string sumText = groupMemo + string.Join(groupSign, arrSumGroup.Select(a => $"{a.Memo}：{a.Sum}"));

                                SetCellValue(rowIndex, cell.CellName, sumText);

                                nodeId = _TableInfor.Rows[rowIndex].ID.ToString();
                                cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId, "=", nameof(EntXmlModel.Record)));
                                //将该行设置为：合计行；这样以后统计的时候跳过该行
                                string totalValue = "合计¤" + string.Format("{0:yyyy-MM-dd HH:mm:ss}", dtFrom) + "¤" + string.Format("{0:yyyy-MM-dd HH:mm:ss}", dtTo);
                                (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.Total), totalValue);
                                _TableInfor.Rows[rowIndex].SumType = totalValue;
                                rtbe = (RichTextBoxExtended)_Need_Assistant_Control;
                                if (rtbe != null)
                                {
                                    rtbe.SelectionAlignment = HorizontalAlignment.Center;
                                }
                                //插入日期时间
                                //dtTo
                                if (b1)
                                {
                                    SetCellValue(rowIndex, "日期", dtTo.ToString("yyyy-MM-dd"));
                                }
                                if (b2)
                                {
                                    SetCellValue(rowIndex, "时间", dtTo.ToString("HH:mm"));
                                }
                            }

                            //提示合计行：行头提示
                            if (!string.IsNullOrEmpty(_DSS.SumRowAttention))
                            {
                                DrawSumRowAttention(_TableInfor, _TableInfor.CurrentCell.RowIndex, null);
                                this.pictureBoxBackGround.Refresh();
                            }
                        }
                    }

                }
                else
                {
                    MessageBox.Show("请先选择填写合计结果的行。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Comm.ShowErrorMessage(ex.Message);
                Comm.LogHelp.WriteErr(ex);
            }
        }

        /// <summary>
        /// 合计
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonSum_Click(object sender, EventArgs e)
        {
            try
            {
                //Sum(""); //进行合计

                if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
                {

                    // SumForm() 构造方法参数： DateTime dtFrom, List<string> columns, string defauleCol
                    //先计算，看看能否取到上次的合计数据，传入设置界面，自动选定时间范围
                    DateTime dtFrom = GetServerDate().AddDays(-1);
                    DateTime dtTo = GetServerDate();
                    string[] arr;

                    //设置列，传入设置界面，让用户选择
                    List<string> columns = new List<string>();
                    //如果没有合并单元格，那么将光标所在列作为默认列
                    string defauleCol = "";

                    RichTextBoxExtended rtbe = (RichTextBoxExtended)_Need_Assistant_Control;

                    string sumGroup = Comm.GetPropertyByName(_SumSetting, "分类汇总").Trim(); //分类汇总的时候，只要选择行，不需要制定某特定单元格

                    //没有权限，不能修改的输入框，不进行赋值；不是表格，也不能进行合计
                    if (rtbe.ReadOnly)
                    {
                        if (string.IsNullOrEmpty(sumGroup))
                        {
                            return;
                        }
                    }

                    if (rtbe._IsTable && _TableInfor.CurrentCell != null)
                    {
                        //不能合计的场合，没有日期列
                        if (_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "日期") == null)
                        {
                            ShowInforMsg("不包含【日期】列的表格，不能根据时间范围进行合计。");
                            return;
                        }

                        //指示手指的位置
                        ShowSearch(_Need_Assistant_Control);

                        //根据护士的日常规律，自动设置时间范围：
                        //TODO：0.最优先的日期自动设置算法,是：取到上次的汇总时间往后开始；但是问题是上一次汇总的那个跳数据未必填写时间。

                        //1.如果当前行有日期时间，就用当前行的日期时间 //_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "日期");
                        //2.上一行有无日期时间
                        //3.系统默认的日期时间


                        //根据当前行位置，得到行最近的日期时间
                        XmlNode records = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));

                        XmlNode thisNode = records.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString(), "=", nameof(EntXmlModel.Record)));
                        string dt = GetRowDateTime((thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                        try
                        {
                            arr = dt.Split('¤');

                            //护理部要求写入24:00
                            arr[1] = arr[1] == "24:00" ? "23:59" : arr[1];

                            if (arr[1].Trim() == "")
                            {
                                arr[1] = string.Format("{0:HH:mm}", GetServerDate());
                            }

                            dtTo = DateTime.Parse(arr[0] + " " + arr[1]);
                            dtFrom = dtTo.AddDays(-1);
                        }
                        catch
                        {
                            //无法获取正常的日期时间，就用设定的系统默认日期时间
                        }

                        //已经合计过的日期时间，最优先显示
                        arr = (thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.Total)).Split('¤');
                        if (arr.Length >= 3)
                        {
                            DateTime dt1 = GetServerDate();
                            DateTime dt2 = GetServerDate();

                            if (DateTime.TryParse(arr[1], out dt1))
                            {
                                dtFrom = dt1.AddMinutes(-1);
                            }

                            if (DateTime.TryParse(arr[2], out dt2))
                            {
                                dtTo = dt2;
                            }
                        }

                        //日期，时间，签名列不进行合计列
                        if (rtbe.Name != "日期" && rtbe.Name != "时间" && !rtbe.Name.Contains("签名"))
                        {
                            defauleCol = rtbe.Name;
                        }

                        //指定的合计列，那么就不用光标选中的了 ¤合计列@出量1|出量2" 
                        string sumColumns = Comm.GetPropertyByName(_SumSetting, "合计列").Trim();
                        string colName = "";
                        if (!string.IsNullOrEmpty(sumColumns))
                        {
                            //设置需要的列，那么全部选中
                            arr = sumColumns.Split('|');

                            ArrayList arrList = new ArrayList();

                            //显示配置的合计列的集合，并且是包含在表格中的。配置错的不在表格列中的名字，过滤掉
                            for (int i = 0; i < _TableInfor.Columns.Length; i++)
                            {
                                colName = _TableInfor.Columns[i].ColName;

                                if (colName != "日期" && colName != "时间" && !colName.Contains("签名"))
                                {
                                    arrList.Add(colName);
                                }
                            }

                            for (int i = 0; i < arr.Length; i++)
                            {
                                colName = arr[i];

                                if (colName != "日期" && colName != "时间" && !colName.Contains("签名")
                                     && arrList.Contains(colName))
                                {
                                    columns.Add(colName);
                                }
                            }

                            defauleCol = "¤";  //表示全部选中
                        }
                        else
                        {
                            for (int i = 0; i < _TableInfor.Columns.Length; i++)
                            {
                                colName = _TableInfor.Columns[i].ColName;

                                if (colName != "日期" && colName != "时间" && !colName.Contains("签名"))
                                {
                                    columns.Add(colName);
                                }
                            }
                        }

                        XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));

                        //更新xml：设计行的合计状态，以及显示该行是否已经是合计行。
                        string nodeId = _TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].ID.ToString();
                        XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId, "=", nameof(EntXmlModel.Record)));

                        string sumWordCol = Comm.GetPropertyByName(_SumSetting, "结文字列").Trim();
                        string sumedWord = "";     //已经合计过的行，结文字也传过去
                        if (!string.IsNullOrEmpty(sumWordCol) && _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, sumWordCol) != null)
                        {
                            sumedWord = (cellNode as XmlElement).GetAttribute(sumWordCol).Split('¤')[0];
                        }

                        //合计界面窗口
                        Frm_Sum sf = new Frm_Sum(dtFrom.AddMinutes(1), dtTo, columns, defauleCol, (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.Total)), _SumSetting, sumedWord);

                        DialogResult res = sf.ShowDialog();

                        if (res == DialogResult.OK)
                        {
                            //获取返回的设置结果：
                            dtFrom = sf.DtFrom;
                            dtTo = sf.DtTo;
                            List<string> columnsList = sf.ColumnsList;
                            double sum = 0;

                            #region 分类合计汇总
                            //最初的合计，每次只能对一个单元格合计，虽然可以对多列合计。但是如果是出入量，就要合计两次。
                            //分类汇总@出量1=出量1,出量2,出量3|出量3=出量3
                            //string sumGroup = Comm.getPropertyByName(_SumSetting, "分类汇总").Trim();
                            SumMulti[] arrSumGroup = null;

                            if (!string.IsNullOrEmpty(sumGroup))
                            {
                                arr = sumGroup.Split('|');
                                int groupCount = arr.Length;
                                string[] arrSumItem;

                                arrSumGroup = new SumMulti[groupCount]; //存放每种分类的合计结果

                                for (int i = 0; i < arr.Length; i++)
                                {
                                    arrSumItem = arr[i].Split('=');

                                    arrSumGroup[i] = new SumMulti();
                                    arrSumGroup[i].SumName = arrSumItem[0].Trim();
                                    arrSumGroup[i].Sum = 0; //初始化为0；
                                    arrSumGroup[i].SumItems = ArrayList.Adapter(arrSumItem[1].Split(','));
                                }
                            }
                            #endregion 分类合计汇总

                            //根据合计条件和表单数据进行合计
                            if (recordsNode != null && recordsNode.ChildNodes.Count > 0)
                            {
                                XmlNode xn = null;
                                string eachValue = "";
                                double eachValueInt = 0;
                                string recordDt = "";
                                DateTime recordDtTime;
                                string tempDateTime = "";

                                //从当前行所在的节点，往前推
                                xn = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString(), "=", nameof(EntXmlModel.Record)));

                                //记录哪些项目统计了几次，即：测量了几次。  //需要在合计的时候统计某项目测量了几次
                                int[] arrInfor = new int[columnsList.Count];
                                for (int a = 0; a < arrInfor.Length; a++)
                                {
                                    arrInfor[a] = 0; //默认为测量了0次，以后合计时没遇到一次，加1.
                                }

                                while (xn != null)
                                {
                                    //xn = recordsNode.ChildNodes[i];
                                    xn = xn.PreviousSibling;

                                    //不为空，且日期时间在范围内的数据；并且为非合计、小计行(nameof(EntXmlModel.Total)非空，表示需要统计的明细数据)
                                    if (xn != null && !Comm.isEmptyRecord(xn))
                                    {
                                        /**************************ljf*2018-9-31*********************************/
                                        //因为之前合计过，但是又取消合计了，Total属性的标记一直存在，所以增加一次判断，有标记，但是合计的值是空的，不排除这个情况
                                        bool isTotal = (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Total)) == "";
                                        if (!isTotal)
                                        {
                                            //选鼠标选中的列和合计选择的列一致，则需要统计。（不管内容是不是为空
                                            //选择鼠标选中的列和合计选了的列不一致，则，如何内容不为空，则不合计
                                            if (!columnsList.Contains(rtbe.Name))
                                            {
                                                if ((xn as XmlElement).GetAttribute(rtbe.Name).Split('¤')[0] != "")
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        /**************************************************************/
                                        //取到该条数据的对应的时间
                                        recordDt = GetRowDateTime((xn as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                                        if (recordDt != "¤") //防止异常情况下取不到这条数据的日期时间
                                        {
                                            arr = recordDt.Split('¤');

                                            //防止只有日期，没有时间的表格
                                            if (arr[1].Trim() == "")
                                            {
                                                arr[1] = "00:00";
                                            }

                                            //护理部要求写入24:00
                                            arr[1] = arr[1] == "24:00" ? "23:59" : arr[1];

                                            //可能是界面上刚输入的，不合法的日期的行数据
                                            tempDateTime = arr[0] + " " + arr[1];
                                            if (!DateHelp.IsDateTime(tempDateTime))
                                            {
                                                //跳过错误数据
                                                ShowInforMsg("在列合计的时候，发现某行日期时间的格式不正确，忽略了该行明细数据：" + tempDateTime, true);
                                                continue;
                                            }
                                            else
                                            {
                                                recordDtTime = DateTime.Parse(tempDateTime);
                                            }

                                            //判断时间，范围内的数据进行统计，早于范围下限后就跳出
                                            if (recordDtTime.CompareTo(dtFrom) >= 0 && recordDtTime.CompareTo(dtTo) <= 0)
                                            {
                                                //去每行，每列的数据。如果需要过滤的，可以再这里过滤
                                                string fileter = Comm.GetPropertyByName(_SumSetting, "过滤").Trim();

                                                //在循环：返回的统计的各列，进行合计
                                                for (int j = 0; j < columnsList.Count; j++)
                                                {
                                                    //判断该单元格是否被合并和空的跳过
                                                    if ((xn as XmlElement).GetAttribute(columnsList[j]) == "" || (xn as XmlElement).GetAttribute(columnsList[j]).Split('¤')[7].Contains("True"))
                                                    {
                                                        continue;

                                                    }

                                                    //过滤指定的数据内容的单元格
                                                    if (!string.IsNullOrEmpty(fileter))
                                                    {
                                                        //需要过滤掉mg的，只要统计ml的
                                                        if (Regex.IsMatch((xn as XmlElement).GetAttribute(columnsList[j]).Split('¤')[0], fileter))
                                                        {
                                                            continue;
                                                        }
                                                    }

                                                    eachValue = (xn as XmlElement).GetAttribute(columnsList[j]).Split('¤')[0]; // 取文本Text
                                                    eachValueInt = StringNumber.getFirstNum(eachValue.Trim());

                                                    if (eachValueInt != -1)
                                                    {
                                                        //总合计
                                                        sum += eachValueInt;

                                                        //分类汇总合计
                                                        if (arrSumGroup != null)
                                                        {
                                                            for (int index = 0; index < arrSumGroup.Length; index++)
                                                            {
                                                                if (arrSumGroup[index].SumItems.Contains(columnsList[j]))
                                                                {
                                                                    arrSumGroup[index].Sum += eachValueInt;
                                                                    //break; //不要跳出，万一以后一列要合计在多个单元格呢，一般不会出现
                                                                }
                                                            }
                                                        }

                                                        //-1和0都表示没有测量的数据，之外的数据都认为是测量数据，那么统计测量次数。
                                                        if (eachValueInt != 0)
                                                        {
                                                            arrInfor[j]++;
                                                        }
                                                    }

                                                    ////只要有内容，非空，就认为是测量数据，那么统计测量次数。
                                                    //if (!string.IsNullOrEmpty(eachValue.Trim()))
                                                    //{
                                                    //    arrInfor[j]++;
                                                    //}
                                                }
                                            }
                                            else if (recordDtTime.CompareTo(dtFrom) < 0)
                                            {
                                                //如果遍历到时间下限之前的数据，那么跳过
                                                break;
                                            }
                                        }
                                    }
                                }

                                //是合计，统计次数
                                if (sf.SumMode == 0)
                                {
                                    ////---------合计结果设置--------- 总合计
                                    //HorizontalAlignment temp = rtbe.SelectionAlignment;
                                    ////将合计的结果显示到合计输入框上
                                    //rtbe.Text = sum.ToString();
                                    //rtbe.SelectionAlignment = temp;

                                    if (arrSumGroup != null)
                                    {
                                        //分类汇总合计的时候
                                        for (int index = 0; index < arrSumGroup.Length; index++)
                                        {
                                            SetCellValue(_CurrentCellIndex.X, arrSumGroup[index].SumName, arrSumGroup[index].Sum.ToString());
                                        }
                                    }
                                    else
                                    {
                                        //默认的总合计的时候
                                        //---------合计结果设置--------- 总合计
                                        HorizontalAlignment temp = rtbe.SelectionAlignment;
                                        //将合计的结果显示到合计输入框上
                                        rtbe.Text = sum.ToString();
                                        rtbe.SelectionAlignment = temp;
                                    }

                                    //将该行设置为：合计行；这样以后统计的时候跳过该行
                                    (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.Total), "合计¤" + string.Format("{0:yyyy-MM-dd HH:mm:ss}", dtFrom) + "¤" + string.Format("{0:yyyy-MM-dd HH:mm:ss}", dtTo));
                                    _TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].SumType = (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.Total));

                                    //---------合计结果设置---------

                                    //合计行的行线设置
                                    string rowLine = Comm.GetPropertyByName(_SumSetting, "行线").Trim();
                                    if (!string.IsNullOrEmpty(rowLine))
                                    {
                                        SetSumRowLine(rowLine);

                                        //行线颜色 保存到xml数据中
                                        arr = rowLine.Split(',');
                                        (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), "Black¤" + arr[0] + "¤" + _TableInfor.Rows[_CurrentCellIndex.X].RowForeColor + "¤" + arr[1]);
                                        IsNeedSave = true; //需要提示保存

                                    }

                                    //合计文字：
                                    //string sumWordCol = Comm.getPropertyByName(_SumSetting, "结文字列").Trim(); //构造方法中也要传过去
                                    if (!string.IsNullOrEmpty(sumWordCol))
                                    {
                                        //选中该单元格，赋值
                                        //if (_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "sumWordCol") == null)
                                        SetCellValue(_CurrentCellIndex.X, sumWordCol, sf.SumWord);
                                    }
                                }

                                //提示合计行：行头提示
                                if (!string.IsNullOrEmpty(_DSS.SumRowAttention))
                                {
                                    DrawSumRowAttention(_TableInfor, _TableInfor.CurrentCell.RowIndex, null);
                                    this.pictureBoxBackGround.Refresh();
                                }

                                #region //---------测量次数提示：『出量2』：0次、『出量3』：2次、『出量4』：0次、『出量总』：3次。
                                StringBuilder sb = new StringBuilder();
                                //sb.Append("次数：");
                                for (int j = 0; j < columnsList.Count; j++)
                                {
                                    sb.Append("『");
                                    sb.Append(columnsList[j]);
                                    sb.Append("』：");
                                    sb.Append(arrInfor[j].ToString());
                                    sb.Append("次");

                                    if (j == columnsList.Count - 1)
                                    {
                                        sb.Append("。");
                                    }
                                    else
                                    {
                                        sb.Append("、");
                                    }
                                }

                                ShowInforMsg(sb.ToString());

                                //仅此处统计的时候，才弹出消息框提示
                                if (sf.SumMode == 1)
                                {
                                    MessageBox.Show(sb.ToString(), "次数统计：", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }

                                #endregion //---------测量次数提示---------
                            }
                        }
                    }
                    else
                    {
                        //非表格，无法进行统计
                        ShowInforMsg("不是表格，或者没有选中单元格；无法进行统计。");
                    }
                }
                else
                {
                    MessageBox.Show("请先选择填写合计结果的输入框。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Comm.ShowErrorMessage(ex.Message);
                Comm.LogHelp.WriteErr(ex);

            }
        }

        private void toolStripButtonCreatePage_Click(object sender, EventArgs e)
        {
            AddNewPage();
            uc_Pager1.Bind();
        }

        //保存方法
        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            bool blFlag = SaveData() == 1;
            if (blFlag == false) return;
            if (_strInsertOrUpdate == "insert")
            {
                //if (MenuEditEvent != null)
                //{
                //    MenuEditEvent(this, new MenuEditEventArgs()
                //    {
                //        EditType = EnumMenuEditType.Add,
                //        Id = strFormID,
                //        Name = strFormType,
                //        Node = this._tn,
                //    });
                //}
            }
        }

        private void toolStripButtonDelPage_Click(object sender, EventArgs e)
        {
            int current = this.uc_Pager1.PageCurrent;
            GlobalVariable.DelPageCancelEventArg delPageArg = null;
            if (this.BeforeDelPage != null)
            {
                delPageArg = new GlobalVariable.DelPageCancelEventArg(
                    current,
                    _TemplateName,
                    _TemplateID,
                    _patientInfo
                    );
                this.BeforeDelPage(this, delPageArg);
            }
            if (delPageArg != null && delPageArg.Cancel == true)
            {
                return;
            }

            ////删除整条记录返回0,删除1页返回1
            int iType = -1;
            bool isDelMenu = false;
            bool blFlag = RemovePage(ref iType) > 0;
            if (iType == 0)
            {
                isDelMenu = true;
                string strFormID = this._TemplateID;
                string strFormType = this._TemplateName;
                this._TemplateID = string.Empty;
                this._TemplateName = string.Empty;

                //删除页，如果没有数据了，则同步树菜单
                //MenuEdit(null, new MenuEditEventArgs()
                //{
                //    EditType = EnumMenuEditType.Del,
                //    Node = _patentTreeNode,
                //    EntMenuInfo = new EntMenuInfo()
                //    {
                //        Id = e.TemplateId,
                //        Name = e.TemplateName,
                //        Sn = _entNurseForm.Sn,
                //    }
                //});

                //if (MenuEditEvent != null)
                //{
                //    MenuEditEvent(this, new MenuEditEventArgs()
                //    {
                //        EditType = EnumMenuEditType.Del,
                //        Id = strFormID,
                //        Name = strFormType,
                //        Node = this._tn,
                //    });
                //}
            }
            else if (iType >= 1)
            {
                int iPageGo = int.Parse((this.uc_Pager1.lblPageCount.Text)) - iType;
                if (iPageGo == 0) iPageGo = 1;
                this.uc_Pager1.ReSetAttribute(iPageGo);
                this.LoadFromDetail(iPageGo.ToString());
                this.uc_Pager1.Bind();
            }
            if (this.AfterDelPage != null)
            {
                this.AfterDelPage(this, new GlobalVariable.DelPageEventArg(
                    current,
                    _TemplateName,
                    _TemplateID,
                    _patientInfo)
                { IsDelMenu = isDelMenu });
            }
        }

        private void toolStripButtonDelAllPage_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsNeedSave)
                {
                    Comm.ShowWarningMessage(string.Format("当前打开的{0}还未保存，请先执行保存操作", this._TemplateName));
                    return;
                }

                if (Comm.ShowQuestionMessage("是否确认删除护理表单？", 2) == DialogResult.Yes)
                {
                    //int ret = DeleteNursingRecord(this._patientInfo.PatientId);

                    if (this.DeleteAllPage != null)
                    {
                        if (CheckHelp.ChecCurrentLoginUser2(GlobalVariable.LoginUserInfo.UserCode, "删除全部页时，需要输入密码进行验证，防止误删除。", this) == false)
                        {
                            return;
                        }
                        bool isSucess = DeleteAllPage(this, new GlobalVariable.DelFormEventArg(this._TemplateName, this._TemplateID, this._patientInfo));

                        if (isSucess)
                        {
                            //外部删除成功后，清空界面，刷新页数
                            ClearControls();
                            _TableInfor = null;
                            _RowForeColorsList.Clear();
                            _RecruitXML = null;
                            _RecruitXML_Org = null;
                            _BaseInforLblnameArr.Clear();
                            _BaseArrForPrintAllPageUpdate.Clear();
                            _TableHeaderList.Clear();
                            _ListDoubleLineInputBox.Clear();
                            _SynchronizeTemperatureRecordListRecordInor.Clear();
                            _isNeedSave = false;
                            _ErrorList.Clear();
                            //Bitmap bmp = new Bitmap(pictureBoxBackGround.Width, pictureBoxBackGround.Height);
                            //Graphics.FromImage(bmp).Clear(Color.White);
                            //pictureBoxBackGround.Image = (Bitmap)bmp.Clone();
                            //bmp.Dispose();
                            this.uc_Pager1.SetInit();
                            //全部删除不要加载数据
                            //LoadData(this._patientInfo, null, this._TemplateXml, this._TemplateID, this._TemplateName);
                            //LoadFromDetail(null);
                            Comm.ShowInfoMessage("护理表单删除成功");
                            this.LoadData(this._patientInfo, null, null, "", "");
                            this.LoadFromDetail(null);
                            return;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                return;
            }
            //string strFormID = this._TemplateID;
            //string strFormType = this._TemplateName;
            //this._TemplateID = string.Empty;
            //this._TemplateName = string.Empty;
            //if (blFlag == false) return;
            //if (MenuEditEvent != null)
            //{
            //    MenuEditEvent(this, new MenuEditEventArgs()
            //    {
            //        EditType = EnumMenuEditType.Del,
            //        Id = strFormID,
            //        Name = strFormType,
            //        Node = this._tn,
            //    });
            //}
        }

        private void toolStripButtonPrintPage_Click(object sender, EventArgs e)
        {
            PrintCurrentPage(null);
        }

        private void toolStripButtonPrintAllPage_Click(object sender, EventArgs e)
        {
            PrintAllPage();
        }

        private void toolStripButtonContinuePrint_Click(object sender, EventArgs e)
        {
            PrintCurrentPage("续打"); //续打
        }

        private void toolStripButtonColorBlack_Click(object sender, EventArgs e)
        {
            Color thisColor = Color.Black;

            if (sender is string)
            {
                string strColor = sender.ToString();

                if (strColor == "红色")
                {
                    thisColor = Color.Red;
                }
                else if (strColor == "绿色")
                {
                    thisColor = Color.Green;
                }
                else if (strColor == "蓝色")
                {
                    thisColor = Color.Blue;
                }
            }

            //if (_IsLoading) return;
            _IsLoading = true;
            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                if ((Control.ModifierKeys & Keys.Control) != 0)
                {
                    //如果是表格的单元格，按下Ctrl设置整行的颜色
                    if (_TableType && _TableInfor.CurrentCell != null)
                    {
                        SetCurrentRowForeColor(((RichTextBoxExtended)_Need_Assistant_Control), thisColor);
                    }
                }
                else
                {
                    //单个输入框、单元格，设置选中文字的颜色
                    ((RichTextBoxExtended)_Need_Assistant_Control).SelectionColor = thisColor;
                    ((RichTextBoxExtended)_Need_Assistant_Control)._IsRtf = true;
                }
            }


            _IsLoading = false;
        }

        /// <summary>
        /// 合并拆分单元格操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonMergeCell_Click(object sender, EventArgs e)
        {
            RichTextBoxExtended rtbe = null;
            if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed)
            {
                rtbe = (RichTextBoxExtended)_Need_Assistant_Control;
            }

            if (_TableType && _TableInfor.CurrentCell != null && (rtbe != null && rtbe._IsTable))
            {
                if (_TableInfor.Vertical)
                {
                    MessageBox.Show("纵向表格不支持合并单元格。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ShowSearch(_Need_Assistant_Control); // 显示指示图标

                string[] arr; //拆分单元格属性值的时候用
                XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                string nodeId = _TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].ID.ToString();
                XmlNode cellNode = recordsNode.SelectSingleNode("Record[@ID='" + nodeId + "']");
                string value = "";

                if (_TableInfor.CurrentCell.IsMerged())
                {
                    //已经合并过，那么就和Excel一样，只能先拆分
                    if (MessageBox.Show("确定要拆分已经合并的单元格吗？", "该单元格是已经合并的", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        bool type = true;//True:向右合并单元格，False:向下合并单元格
                        int count = 0;
                        if (_TableInfor.CurrentCell.ColSpan > 0)
                        {
                            type = true;
                            count = _TableInfor.CurrentCell.ColSpan;
                        }
                        else
                        {
                            type = false;
                            count = _TableInfor.CurrentCell.RowSpan;
                        }

                        int row = _TableInfor.CurrentCell.RowIndex;
                        int col = _TableInfor.CurrentCell.ColIndex;

                        //_TableInfor.Columns[col].RTBE.ResetSize();//恢复拆分前的输入框大小，会取消_IsRtf，
                        _TableInfor.Columns[col].RTBE.Size = _TableInfor.Columns[col].RTBE._DefaultSize;

                        _TableInfor.CurrentCell.ColSpan = 0;
                        _TableInfor.CurrentCell.RowSpan = 0;
                        _TableInfor.CurrentCell.CellSize = _TableInfor.Columns[col].RTBE._DefaultCellSize;
                        _TableInfor.CurrentCell.Rect = new Rectangle(_TableInfor.CurrentCell.Loaction, _TableInfor.CurrentCell.CellSize);

                        ShowCellRTBE(_TableInfor.CurrentCell.RowIndex, _TableInfor.CurrentCell.ColIndex);

                        //SaveCellToXML(_TableInfor.Columns[col].RTBE); //如果该单元格为富文本，那么下次打开会变成纯文本

                        //2.再设定，被合并的单元格的属性： _TableInfor.Cells[i, j].IsMergedNotShow
                        for (int i = 1; i <= count; i++)
                        {
                            if (type)
                            {
                                _TableInfor.Cells[row, col + i].IsMergedNotShow = false;

                                ShowCellLeftLine(row, col + i); //显示左侧边线

                                //-------------更新xml，取消该单元格已经被隐藏
                                value = (cellNode as XmlElement).GetAttribute(_TableInfor.Cells[row, col + i].CellName);
                                if (value == "")
                                {
                                    //0:数据值Text,1、2：单红线、双红线，3：创建用户，4：有无单元格底线，5、6：有无正反对角线，7：合并单元格信息，8：单元格线颜色，9：Rtf
                                    value = "¤¤¤¤¤¤¤¤¤";
                                }
                                else
                                {
                                    arr = value.Split('¤');
                                    arr[7] = "";

                                    value = "";
                                    for (int k = 0; k < arr.Length; k++)
                                    {
                                        if (k == 0)
                                        {
                                            value = arr[k];
                                        }
                                        else
                                        {
                                            value += "¤" + arr[k];
                                        }
                                    }
                                }

                                //更新属性：
                                (cellNode as XmlElement).SetAttribute(_TableInfor.Cells[row, col + i].CellName, value);
                                //-------------更新xml，设置该单元格已经被隐藏


                                //被拆分的单元格可能又数据，需要更新刷新，不光光标点击进入才会显示出来
                                ShowCellRTBE(row, col + i);
                                Redraw(_TableInfor.Columns[col + i].RTBE);
                            }
                            else
                            {
                                _TableInfor.Cells[row + i, col].IsMergedNotShow = false;
                                //_TableInfor.Cells[row, col + i].MergedRight = false;
                                //_TableInfor.Cells[row, col + i].MergedCell = new Point(row, col);
                                //_TableInfor.Cells[row, col + i].MergedCount = count;    //（1，2 ，右，3）

                                ShowCellTopLine(row + i, col); //显示上侧边线


                                //-------------更新xml，取消该单元格已经被隐藏
                                nodeId = _TableInfor.Rows[row + i].ID.ToString();
                                cellNode = recordsNode.SelectSingleNode("Record[@ID='" + nodeId + "']");

                                value = (cellNode as XmlElement).GetAttribute(_TableInfor.Cells[row + i, col].CellName);

                                if (value == "")
                                {
                                    //0:数据值Text,1、2：单红线、双红线，3：创建用户，4：有无单元格底线，5、6：有无正反对角线，7：合并单元格信息，8：单元格线颜色，9：Rtf ，10：修订
                                    value = "¤¤¤¤¤¤¤¤¤";  //颜色就是“”空，不是黑色，不绘制了。
                                }
                                else
                                {
                                    arr = value.Split('¤');
                                    arr[7] = "";

                                    value = "";
                                    for (int k = 0; k < arr.Length; k++)
                                    {
                                        if (k == 0)
                                        {
                                            value = arr[k];
                                        }
                                        else
                                        {
                                            value += "¤" + arr[k];
                                        }
                                    }
                                }

                                //更新属性：
                                (cellNode as XmlElement).SetAttribute(_TableInfor.Cells[row + i, col].CellName, value);
                                //-------------更新xml，设置该单元格已经被隐藏

                                //被拆分的单元格可能又数据，需要更新刷新，不光光标点击进入才会显示出来
                                ShowCellRTBE(row + i, col);
                                Redraw(_TableInfor.Columns[col].RTBE);
                            }
                        }

                        ShowCellRTBE(row, col); //重新将光标置回，显示拆分对象单元格。

                        _CurrentCellNeedSave = true;

                        string msg = "对" + _TableInfor.CurrentCell.CellName + "的第" + (_TableInfor.CurrentCell.RowIndex + 1).ToString() + "行，进行了拆分单元格操作。";
                        ShowInforMsg(msg);

                    }
                }
                else
                {
                    Frm_MergeCell mcf = new Frm_MergeCell(_TableInfor.RowsCount, _TableInfor.ColumnsCount, _CurrentCellIndex.X, _CurrentCellIndex.Y);

                    DialogResult res = mcf.ShowDialog();

                    if (res == DialogResult.OK)
                    {
                        if (mcf.MergeCount == 0)
                        {
                            ShowInforMsg("合并列数或者行数如果为0，没有进行合并。");
                            return;
                        }

                        bool type = mcf.MergeType;//True:向右合并单元格，False:向下合并单元格
                        int count = mcf.MergeCount;

                        MergeCell(type, count);
                    }
                }
            }
        }

        # region 表单：另存为
        /// <summary>
        /// 另存为、表单数据另存到本地
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripSaveAs_Click(object sender, EventArgs e)
        {
            SaveAs();
        }
        #endregion 表单：另存为

        #region 行操作（插入，删除）
        //向上插入行
        private void tBnt_InsertRowUp_Click(object sender, EventArgs e)
        {
            TableInsertRowToUp(sender);
        }

        //向下插入行
        private void tBnt_InsertRowDwon_Click(object sender, EventArgs e)
        {
            TableInsertRowToDown(sender);
        }

        //删除行
        private void tBnt_DelRow_Click(object sender, EventArgs e)
        {
            TableDeleteRow();
        }
        #endregion

        #region 单元格线操作
        private void toolStripMenuItemCell_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (!_TableType || _TableInfor.CurrentCell == null)
            {
                return;
            }

            ShowSearch(_Need_Assistant_Control); // 显示指示图标

            //toolStripButtonLine.HideDropDown(); //不然会挡住弹出的确定消息哦
            //(sender as ToolStripDropDownItem).HideDropDown();
            toolStripButtonLine.HideDropDown();

            if (MessageBox.Show("确定要对该单元格进行：【" + e.ClickedItem.Text + "】操作吗？", "行边框线设置", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {

            }
            else
            {
                return;
            }

            _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].NotTopmLine = false; //先设置为不隐藏上边线

            switch (e.ClickedItem.Name)
            {
                //单元格的边框线
                case "toolStripMenuItemCellLineRed": //"单元格边框色：红色":
                    SetCellLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, Color.Red);
                    break;

                case "toolStripMenuItemCellLineGreen": //"单元格边框色：绿":
                    SetCellLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, Color.Green);
                    break;

                case "toolStripMenuItemCellLineBlue": //"单元格边框色：蓝":
                    SetCellLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, Color.Blue);
                    break;

                case "toolStripMenuItemCellLineBlack": //"单元格边框色：黑":
                    SetCellLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, Color.Black);
                    break;

                case "toolStripMenuItemCellLineDefault": //"单元格边框色：默认":
                    SetCellLine(_CurrentCellIndex.X, _CurrentCellIndex.Y, Color.Black);
                    break;

                case "toolStripMenuItemCellLineZ": //"单元格正对角线：默认":
                    SetCellLineZN(_CurrentCellIndex.X, _CurrentCellIndex.Y, "z", true);
                    _TableInfor.CurrentCell.DiagonalLineZ = true;
                    break;

                case "toolStripMenuItemCellLineN": //"单元格反对角线":
                    SetCellLineZN(_CurrentCellIndex.X, _CurrentCellIndex.Y, "n", true);
                    _TableInfor.CurrentCell.DiagonalLineN = true;
                    break;

                case "toolStripMenuItemCellHideTopLine": //"隐藏单元格上边框线":
                    HideCellTopLine(_CurrentCellIndex.X, _CurrentCellIndex.Y);
                    _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].NotTopmLine = true;//_TableInfor.CurrentCell.NotBottomLine = true;
                    break;
            }

            IsNeedSave = true;
            _CurrentCellNeedSave = true;
        }

        #endregion 单元格线操作


        #endregion

        #region 私有
        private void MergeCell(bool type, int count)
        {
            string[] arr; //拆分单元格属性值的时候用
            XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            string nodeId = _TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].ID.ToString();
            XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId, "=", nameof(EntXmlModel.Record)));
            string value = "";

            //bool type = mcf.MergeType;//True:向右合并单元格，False:向下合并单元格
            //int count = mcf.MergeCount;

            //根据选择设定的参数，开始进行合并单元格处理
            //1.首先设置，元单元格的属性：
            if (type)
            {
                _TableInfor.CurrentCell.ColSpan = count;

                //判断，是否超过最大值
                if (_TableInfor.CurrentCell.ColIndex + count >= _TableInfor.ColumnsCount)
                {
                    _TableInfor.CurrentCell.ColSpan = 0;
                    ShowInforMsg("设置的合并列数 " + count.ToString() + " 超过了当前单元格右侧的最大列数" + (_TableInfor.ColumnsCount - _TableInfor.CurrentCell.ColIndex - 1) + "，请重新设置。");
                    return;
                }
            }
            else
            {
                //纵向合并
                _TableInfor.CurrentCell.RowSpan = count;

                //判断，是否超过最大值
                if (_TableInfor.CurrentCell.RowIndex + count >= _TableInfor.RowsCount)
                {
                    _TableInfor.CurrentCell.RowSpan = 0;
                    ShowInforMsg("设置的合并行数 " + count.ToString() + " 超过了当前单元格下面的最大行数" + (_TableInfor.RowsCount - _TableInfor.CurrentCell.RowIndex - 1) + "，请重新设置。");
                    return;
                }
            }

            Size szNew = _TableInfor.CurrentCell.CellSize;
            int row = _TableInfor.CurrentCell.RowIndex;
            int col = _TableInfor.CurrentCell.ColIndex;

            for (int i = 1; i <= count; i++)
            {
                if (type)
                {
                    szNew = new Size(szNew.Width + _TableInfor.Cells[row, col + i].CellSize.Width, szNew.Height);
                }
                else
                {
                    szNew = new Size(szNew.Width, szNew.Height + _TableInfor.Cells[row + i, col].CellSize.Height);
                }
            }

            _TableInfor.CurrentCell.CellSize = szNew;
            _TableInfor.CurrentCell.Rect = new Rectangle(_TableInfor.CurrentCell.Loaction, _TableInfor.CurrentCell.CellSize);

            ShowCellRTBE(_TableInfor.CurrentCell.RowIndex, _TableInfor.CurrentCell.ColIndex);

            ////合并后，单元格内容不能完全显示
            //_TableInfor.Columns[_TableInfor.CurrentCell.ColIndex].RTBE.Focus();
            //_TableInfor.Columns[_TableInfor.CurrentCell.ColIndex].RTBE.Update();



            //2.再设定，被合并的单元格的属性： _TableInfor.Cells[i, j].IsMergedNotShow
            for (int i = 1; i <= count; i++)
            {
                if (type)
                {
                    _TableInfor.Cells[row, col + i].IsMergedNotShow = true;
                    _TableInfor.Cells[row, col + i].MergedRight = true;
                    _TableInfor.Cells[row, col + i].MergedCell = new Point(row, col);
                    _TableInfor.Cells[row, col + i].MergedCount = count;    //（1，2 ，右，3）

                    HideCellLeftLine(row, col + i); //隐藏左侧边线

                    //-------------更新xml，设置该单元格已经被隐藏
                    value = (cellNode as XmlElement).GetAttribute(_TableInfor.Cells[row, col + i].CellName);
                    if (value == "")
                    {
                        //0:数据值Text,1、2：单红线、双红线，3：创建用户，4：有无单元格底线，5、6：有无正反对角线，7：合并单元格信息，8：单元格线颜色，9：Rtf
                        value = "¤¤¤¤¤¤¤,,True¤¤";
                    }
                    else
                    {
                        arr = value.Split('¤');
                        arr[7] = ",,True";

                        value = "";
                        for (int k = 0; k < arr.Length; k++)
                        {
                            if (k == 0)
                            {
                                value = arr[k];
                            }
                            else
                            {
                                value += "¤" + arr[k];
                            }
                        }
                    }

                    //更新属性：
                    (cellNode as XmlElement).SetAttribute(_TableInfor.Cells[row, col + i].CellName, value);
                    //-------------更新xml，设置该单元格已经被隐藏
                }
                else
                {
                    _TableInfor.Cells[row + i, col].IsMergedNotShow = true;
                    _TableInfor.Cells[row + i, col].MergedRight = false;
                    _TableInfor.Cells[row + i, col].MergedCell = new Point(row, col);
                    _TableInfor.Cells[row + i, col].MergedCount = count;    //（1，2 ，右，3）

                    HideCellTopLine(row + i, col); //隐藏上侧边线

                    //-------------更新xml，设置该单元格已经被隐藏
                    nodeId = _TableInfor.Rows[row + i].ID.ToString();
                    cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId, "=", nameof(EntXmlModel.Record)));

                    value = (cellNode as XmlElement).GetAttribute(_TableInfor.Cells[row + i, col].CellName);

                    if (value == "")
                    {
                        //0:数据值Text,1、2：单红线、双红线，3：创建用户，4：有无单元格底线，5、6：有无正反对角线，7：合并单元格信息，8：单元格线颜色，9：Rtf
                        value = "¤¤¤¤¤¤¤,,True¤¤";
                    }
                    else
                    {
                        arr = value.Split('¤');
                        arr[7] = ",,True";

                        value = "";
                        for (int k = 0; k < arr.Length; k++)
                        {
                            if (k == 0)
                            {
                                value = arr[k];
                            }
                            else
                            {
                                value += "¤" + arr[k];
                            }
                        }
                    }

                    //更新属性：
                    (cellNode as XmlElement).SetAttribute(_TableInfor.Cells[row + i, col].CellName, value);
                    //-------------更新xml，设置该单元格已经被隐藏
                }
            }

            _CurrentCellNeedSave = true;

            string msg = "对" + _TableInfor.CurrentCell.CellName + "的第" + (_TableInfor.CurrentCell.RowIndex + 1).ToString() + "行，进行了合并单元格操作：";
            if (type)
            {
                msg += "向右合并了" + count.ToString() + "格。";
            }
            else
            {
                msg += "向下合并了" + count.ToString() + "格。";
            }

            ShowInforMsg(msg);

            //(本身九尾多行的输入框)合并后，单元格内容不能完全显示 
            if (_TableInfor.Columns[_TableInfor.CurrentCell.ColIndex].RTBE.OwnerTableHaveMultilineCell) //因为会居中，纵向位置变了。所以要刷新
            {
                _TableInfor.Columns[_TableInfor.CurrentCell.ColIndex].RTBE.Focus();
                _TableInfor.Columns[_TableInfor.CurrentCell.ColIndex].RTBE.Refresh();
            }
        }


        private void MergeCellToRight(int colIndex, int rowIndex, int count)
        {
            string[] arr; //拆分单元格属性值的时候用
            XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            string nodeId = _TableInfor.Rows[rowIndex].ID.ToString();
            XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId, "=", nameof(EntXmlModel.Record)));
            string value = "";
            var currentCell = _TableInfor.Cells[rowIndex, colIndex];
            //bool type = mcf.MergeType;//True:向右合并单元格，False:向下合并单元格
            //int count = mcf.MergeCount;

            //根据选择设定的参数，开始进行合并单元格处理
            //1.首先设置，元单元格的属性：
            currentCell.ColSpan = count;

            //判断，是否超过最大值
            if (colIndex + count >= _TableInfor.ColumnsCount)
            {
                currentCell.ColSpan = 0;
                ShowInforMsg("设置的合并列数 " + count.ToString() + " 超过了当前单元格右侧的最大列数" + (_TableInfor.ColumnsCount - currentCell.ColIndex - 1) + "，请重新设置。");
                return;
            }

            Size szNew = currentCell.CellSize;
            int row = rowIndex;// _TableInfor.CurrentCell.RowIndex;
            int col = colIndex;

            for (int i = 1; i <= count; i++)
            {
                szNew = new Size(szNew.Width + _TableInfor.Cells[row, col + i].CellSize.Width, szNew.Height);
            }

            currentCell.CellSize = szNew;
            currentCell.Rect = new Rectangle(currentCell.Loaction, currentCell.CellSize);

            ShowCellRTBE(currentCell.RowIndex, currentCell.ColIndex);


            //2.再设定，被合并的单元格的属性： _TableInfor.Cells[i, j].IsMergedNotShow
            for (int i = 1; i <= count; i++)
            {
                _TableInfor.Cells[row, col + i].IsMergedNotShow = true;
                _TableInfor.Cells[row, col + i].MergedRight = true;
                _TableInfor.Cells[row, col + i].MergedCell = new Point(row, col);
                _TableInfor.Cells[row, col + i].MergedCount = count;    //（1，2 ，右，3）

                HideCellLeftLine(row, col + i); //隐藏左侧边线

                //-------------更新xml，设置该单元格已经被隐藏
                value = (cellNode as XmlElement).GetAttribute(_TableInfor.Cells[row, col + i].CellName);
                if (value == "")
                {
                    //0:数据值Text,1、2：单红线、双红线，3：创建用户，4：有无单元格底线，5、6：有无正反对角线，7：合并单元格信息，8：单元格线颜色，9：Rtf
                    value = "¤¤¤¤¤¤¤,,True¤¤";
                }
                else
                {
                    arr = value.Split('¤');
                    arr[7] = ",,True";

                    value = "";
                    for (int k = 0; k < arr.Length; k++)
                    {
                        if (k == 0)
                        {
                            value = arr[k];
                        }
                        else
                        {
                            value += "¤" + arr[k];
                        }
                    }
                }

                    //更新属性：
                    (cellNode as XmlElement).SetAttribute(_TableInfor.Cells[row, col + i].CellName, value);
                //-------------更新xml，设置该单元格已经被隐藏
            }

            _CurrentCellNeedSave = true;

            string msg = "对" + currentCell.CellName + "的第" + (currentCell.RowIndex + 1).ToString() + "行，进行了合并单元格操作：";
            msg += "向右合并了" + count.ToString() + "格。";

            ShowInforMsg(msg);

            //(本身九尾多行的输入框)合并后，单元格内容不能完全显示 
            if (_TableInfor.Columns[currentCell.ColIndex].RTBE.OwnerTableHaveMultilineCell) //因为会居中，纵向位置变了。所以要刷新
            {
                _TableInfor.Columns[currentCell.ColIndex].RTBE.Focus();
                _TableInfor.Columns[currentCell.ColIndex].RTBE.Refresh();
            }
        }

        /// <summary>
        /// 更新工具栏图标和菜单文字
        /// </summary>
        private void SetRowLineMenuText()
        {
            //更新工具栏图标和菜单文字
            //1.默认行线图标的图像
            toolStripButtonDefaultRowLine.Image = new Bitmap(toolStripButtonDefaultRowLine.Image.Width, toolStripButtonDefaultRowLine.Image.Height); //重新设置图标图片为空白
            Graphics g = Graphics.FromImage(toolStripButtonDefaultRowLine.Image);   //this.pictureBoxBackGround.CreateGraphics(); 刷新会丢失的创建方法
            g.SmoothingMode = SmoothingMode.HighQuality;
            int width = 2; //这里的图标上的线的宽度固定为2，不然有可能显示不清楚
            //g.DrawLine(new Pen(GlobalVariable._RowLineType_Color_Selected, 2), new Point(1, (toolStripButtonDefaultRowLine.Image.Height - width) / 2), new Point(toolStripButtonDefaultRowLine.Image.Width - 1, (toolStripButtonDefaultRowLine.Image.Height - width) / 2));
            g.DrawLine(new Pen(Variable.ToolStrip_LineColor.color, 2), new Point(1, (toolStripButtonDefaultRowLine.Image.Height - width) / 2), new Point(toolStripButtonDefaultRowLine.Image.Width - 1, (toolStripButtonDefaultRowLine.Image.Height - width) / 2));
            g.Dispose();
        }

        /// <summary>
        /// 默认的行线操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetDefaultRowLine()
        {
            ToolStripMenuItem tsi = null;

            //工具栏，行线的默认类型指定
            string DefaultRowLine = "BottomLine";

            switch (DefaultRowLine)
            {
                //e.ClickedItem.Name
                case "None":
                    tsi = ToolStripMenuItemBottomLineDefault;
                    //tsi = new ToolStripMenuItem();
                    //tsi.Name = "ToolStripMenuItemBottomLineDefault";
                    //tsi.Text = "行线：默认";
                    break;

                case "RectLine":
                    tsi = toolStripMenuItemRectLine;
                    //tsi = new ToolStripMenuItem();
                    //tsi.Name = "toolStripMenuItemRectLine";
                    //tsi.Text = "行线：四周红线";
                    break;
                case "RectDoubleLine":
                    tsi = ToolStripMenuItemRectDoubleLine;
                    //tsi = new ToolStripMenuItem();
                    //tsi.Name = "ToolStripMenuItemRectDoubleLine";
                    //tsi.Text = "行线：四周双红线";
                    break;

                case "BottomLine":
                    tsi = ToolStripMenuItemBottomLine;
                    //tsi = new ToolStripMenuItem();
                    //tsi.Name = "ToolStripMenuItemRectDoubleLine";
                    //tsi.Text = "行线：四周双红线";
                    break;
                case "BottomDoubleLine":
                    tsi = ToolStripMenuItemBottomDoubleLine;
                    break;

                case "TopBottomLine":
                    tsi = ToolStripMenuItemTopBottomLine;
                    break;
                case "TopBottomDoubleLine":
                    tsi = ToolStripMenuItemTopBottomDoubleLine;
                    break;

                case "TopLine":
                    tsi = ToolStripMenuItemTopLine;
                    break;
                case "TopDoubleLine":
                    tsi = ToolStripMenuItemTopDoubleLine;
                    break;

                default:
                    tsi = ToolStripMenuItemBottomLine;
                    break;
            }

            if (tsi != ToolStripMenuItemBottomLine)
            {
                ToolStripItemClickedEventArgs eLine = new ToolStripItemClickedEventArgs(tsi);

                toolStripMenuItemRow_DropDownItemClicked(null, eLine);

                DefaultRowLine = null;
            }
            else
            {
                toolStripMenuItemRow_DropDownItemClicked("toolStripButtonDefaultRowLine", null);

                DefaultRowLine = null;
            }
        }

        /// <summary>
        /// 操作后，要更新双红线的输入框集合
        /// </summary>
        /// <param name="rtbePara"></param>
        private void AddToDoubleList(RichTextBoxExtended rtbePara)
        {
            if (rtbePara._HaveSingleLine || rtbePara._HaveDoubleLine)
            {
                if (!_ListDoubleLineInputBox.Contains(rtbePara))
                {
                    _ListDoubleLineInputBox.Add(rtbePara);
                }
            }
            else
            {
                if (_ListDoubleLineInputBox.Contains(rtbePara))
                {
                    _ListDoubleLineInputBox.Remove(rtbePara);
                }
            }
        }

        private void AddNewPage()
        {
            if (this._patientInfo == null)
            {
                Comm.ShowWarningMessage("没有病人信息，无法新建页！");
                return;
            }

            if (string.IsNullOrEmpty(this._TemplateID) || this._TemplateXml == null)
            {
                Comm.ShowWarningMessage("请先加载表单后才能创建页！");
                return;
            }
            this.pictureBoxBackGround.Focus();
            SaveToOp(); //防止直接修改后，直接点击创建其他病人的表单，而表格填补的空白行已经删除，会报错

            //if (_IsCreated)
            if (IsNeedSave)
            {
                string strMsg = "已经是新建页，必须保存后才能继续新建页。";
                Comm.ShowWarningMessage(strMsg);
                return;
            }

            GlobalVariable.NewPageCancelEventArg newPageArg = null;
            if (this.BeforeNewPage != null)
            {
                newPageArg = new GlobalVariable.NewPageCancelEventArg(this.uc_Pager1.PageCount, this._TemplateName, this._TemplateID, this._patientInfo);
                this.BeforeNewPage(this, newPageArg);
            }

            if (newPageArg != null && newPageArg.Cancel == true)
            {
                return;
            }

            _IsCreating = true;

            //打开新建的第一页：
            //treeViewPatients_NodeMouseDoubleClick(this.treeViewPatients.SelectedNode, null);
            int tempMaxInt = -1;
            int.TryParse(this.uc_Pager1.lblPageCount.Text, out tempMaxInt);
            int iPageGo = tempMaxInt + 1;
            //连续创建N页
            var xmlTemp = _TemplateXml.SelectSingleNode("/" + nameof(EntXmlModel.NurseForm));
            if (!int.TryParse((xmlTemp as XmlElement).GetAttribute(nameof(EntXmlModel.AutoCreatPagesCount)), out int autoCreatPagesCount))
            {
                autoCreatPagesCount = 1;
            }

            //如果有多页的情况，生成多页的Form，否则下次打开会丢失页

            if (autoCreatPagesCount > 1)
            {
                Comm.LogHelp.WriteDebug($"在AddNewPage中，自动创建页AutoCreatPagesCount={autoCreatPagesCount} 堆栈信息：{Comm.GetStackTrace()}");
                XmlNode pages = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms)));
                for (int i = 0; i < autoCreatPagesCount; i++)
                {
                    XmlNode thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), (iPageGo + i).ToString(), "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));
                    if (thisPage == null)
                    {
                        //插入新的节点
                        XmlElement newPage;
                        newPage = _RecruitXML.CreateElement(nameof(EntXmlModel.Form));
                        newPage.SetAttribute(nameof(EntXmlModel.SN), (iPageGo + i).ToString());
                        pages.AppendChild(newPage);
                    }
                }
            }


            //int maxPage = 0;
            //if (!int.TryParse(this.uc_Pager1.lblPageCount.Text, out maxPage))
            //{
            //    maxPage = this.uc_Pager1.PageCurrent;
            //}
            //if (maxPage > this.uc_Pager1.PageCurrent)
            //{
            //    for (int i = 1; i <= maxPage - this.uc_Pager1.PageCurrent; i++)
            //    {
            //        //插入新的节点
            //        XmlElement newPage;
            //        newPage = _RecruitXML.CreateElement(nameof(EntXmlModel.Form));

            //        newPage.SetAttribute(nameof(EntXmlModel.SN), (this.uc_Pager1.PageCurrent + i).ToString());

            //        pages.AppendChild(newPage);
            //    }
            //}


            if (tempMaxInt == -1)
            {
                LoadFromDetail(null);
            }
            else
            {
                //this.formPager1.ReSetAttribute(iPageGo);
                this.uc_Pager1.SetMaxCount(tempMaxInt + autoCreatPagesCount);
                this.uc_Pager1.Bind();
                LoadFromDetail(iPageGo.ToString());
            }

            if (this.AfterNewPage != null)
            {
                this.AfterNewPage(this, new GlobalVariable.NewPageEventArg(iPageGo, this._TemplateName, this._TemplateID, this._patientInfo));
            }
            //_IsCreating = false; //因为是线程处理的，这里重置可能有问题
            IsNeedSave = true;
            _IsCreated = true;
        }


        #region 删除当前页

        #region 老的方法
        //删除表单页
        //private int RemovePage(ref int iType)
        //{
        //    try
        //    {
        //        SaveToOp();

        //        if (this.uc_Pager1.PageCurrent != 0 && _RecruitXML != null)
        //        {
        //            if ((Comm.ShowQuestionMessage("确定要删除本页数据吗？", 2)) == DialogResult.Yes)
        //            {
        //                int autoCreatPagesCount = 1;
        //                var xmlTemp = _TemplateXml.SelectSingleNode("/" + nameof(EntXmlModel.NurseForm));
        //                if (!int.TryParse((xmlTemp as XmlElement).GetAttribute(nameof(EntXmlModel.AutoCreatPagesCount)), out autoCreatPagesCount))
        //                {
        //                    autoCreatPagesCount = 1;
        //                }

        //                //如果删除的是第一页，并且只有一页，那么删除该表单
        //                int iPageCount = _RecruitXML.SelectSingleNode("//" + EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms))).ChildNodes.Count;
        //                //if ((autoCreatPagesCount == 1 && this.uc_Pager1.PageCurrent == 1 && iPageCount == 1) ||
        //                //    (autoCreatPagesCount > 1 && this.uc_Pager1.PageCurrent <= autoCreatPagesCount && iPageCount <= autoCreatPagesCount))
        //                //{
        //                //    delPageStart = 1;
        //                //    int ret = DeleteNursingRecord(this._patientInfo.PatientId);
        //                //    if (ret > 0)
        //                //    {
        //                //        //Comm.ShowInfoMessage("护理表单删除成功");
        //                //        iType = 1;
        //                //        //return 1;
        //                //    }
        //                //    else
        //                //    {
        //                //        //Comm.ShowErrorMessage("护理表单删除时发生异常，请查看日志");
        //                //        //return -1;
        //                //    }
        //                //}

        //                int thisSn = this.uc_Pager1.PageCurrent;
        //                int eachSn = 0;
        //                //1.删除<Page ： 要更新后面的SN
        //                if (_RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms))) != null)
        //                {
        //                    iType = autoCreatPagesCount;
        //                    //删除节点需要考虑 多页模板，多页模板时，删除需要删除当前页包含的多页
        //                    //删除页节点
        //                    int delPageStart = this.uc_Pager1.PageCurrent;
        //                    if (autoCreatPagesCount > 1)
        //                    {
        //                        int mod = thisSn % autoCreatPagesCount;
        //                        //删除起始页 =（当前页/多页 - mod??1）*多页 +1
        //                        delPageStart = ((thisSn / autoCreatPagesCount) - (mod == 0 ? 1 : 0)) * autoCreatPagesCount + 1;
        //                    }

        //                    for (int i = delPageStart; i < delPageStart + autoCreatPagesCount; i++)
        //                    {
        //                        XmlNode thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), i.ToString(), "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));
        //                        if (thisPage != null)
        //                        {
        //                            _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms))).RemoveChild(thisPage);
        //                        }
        //                    }

        //                    //更新删除页后面的页号
        //                    if (_RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms))).ChildNodes.Count > 0)
        //                    {
        //                        foreach (XmlNode xn in _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms))).ChildNodes)
        //                        {
        //                            eachSn = int.Parse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.SN)));
        //                            if (eachSn > delPageStart)
        //                            {
        //                                (xn as XmlElement).SetAttribute(nameof(EntXmlModel.SN), (eachSn - autoCreatPagesCount).ToString());
        //                            }
        //                        }
        //                    }
        //                }

        //                //2.删除：<Record
        //                if (_TableType && _TableInfor != null)
        //                {
        //                    XmlNode records = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
        //                    XmlNode record;
        //                    int maxId = 0;

        //                    //删除本页的各行
        //                    for (int row = 0; row < _TableInfor.RowsCount * _TableInfor.GroupColumnNum; row++)
        //                    {
        //                        record = records.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[row].ID.ToString(), "=", nameof(EntXmlModel.Record)));
        //                        if (record != null)
        //                        {
        //                            //判断ID,再删除
        //                            if ((record as XmlElement).GetAttribute(nameof(EntXmlModel.ID)) == (records as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID)))
        //                            {
        //                                //更新最大的编号唯一号
        //                                maxId = int.Parse((records as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID)));
        //                                maxId--;
        //                                (records as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), maxId.ToString());
        //                            }

        //                            records.RemoveChild(record);
        //                        }
        //                    }

        //                    if (records.ChildNodes.Count == 0)
        //                    {
        //                        (records as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), "");
        //                    }
        //                }


        //                ////删除完毕，刷新界面和书节点：删除最后一个节点
        //                //_CurrentTemplateNameTreeNode.Nodes.RemoveAt(_CurrentTemplateNameTreeNode.Nodes.Count - 1);

        //                //防止之前正好创建页等，还没有保存，那么就会提示要保存的。实际上这里不需要提示
        //                IsNeedSave = false;
        //                _RecruitXML_Org.InnerXml = _RecruitXML.InnerXml;//保存后，更新备份比较文件。
        //                //SaveToDB();

        //                //bool blResult = SaveFormToDB();
        //                //iType = 1;
        //                return iType;
        //                //return blResult ? 1 : -1;
        //                ////删除后，还有页，那么打开最后一页


        //                //if (_CurrentTemplateNameTreeNode.Nodes.Count > 0)
        //                //{
        //                //    if (_IsCreated)
        //                //    {
        //                //        _IsCreated = false; //删除刚创建的没有保存的新页，就不能在添加新页了
        //                //    }

        //                //    if (_IsCreating)
        //                //    {
        //                //        _IsCreating = false;
        //                //    }

        //                //    if (_IsCreatedTemplate)
        //                //    {
        //                //        _IsCreatedTemplate = false;
        //                //    }

        //                //    //刷新界面
        //                //    TreeNode nodeChildPage = _CurrentTemplateNameTreeNode.LastNode;
        //                //    //打开新建的第一页： 一共多少页等都不变不刷新了
        //                //    treeViewPatients_NodeMouseDoubleClick(nodeChildPage, null);
        //                //}
        //                //else
        //                //{
        //                //    //创建模板的节点后，默认有第一页，所以要添加页节点
        //                //    //添加页码节点
        //                //    TreeNode nodeChildPage = new TreeNode();
        //                //    nodeChildPage.Text = "1";
        //                //    nodeChildPage.Tag = "3"; //nodeChildPage.Tag = nodeChild;
        //                //    nodeChildPage.ImageIndex = 2;
        //                //    _CurrentTemplateNameTreeNode.Nodes.Add(nodeChildPage);

        //                //    _CurrentTemplateNameTreeNode.Expand();

        //                //    //打开新建的第一页：
        //                //    treeViewPatients_NodeMouseDoubleClick(nodeChildPage, null);

        //                //    ShowInforMsg("本表单已经删除了最后一页，程序自动新建了空白页。");
        //                //}

        //                //this.treeViewPatients.SelectedNode = _CurrentTemplateNameTreeNode.LastNode;
        //            }
        //            else
        //                return -1;
        //        }
        //        else
        //            return -1;
        //    }
        //    catch (Exception ex)
        //    {
        //        Comm.Logger.WriteErr(ex);
        //        return -1;
        //    }
        //}
        #endregion

        private int RemovePage(ref int iType)
        {
            try
            {
                this.SaveToOp();
                if ((this.uc_Pager1.PageCurrent != 0) && (this._RecruitXML != null))
                {
                    if (Comm.ShowQuestionMessage("确定要删除本页数据吗？", 2) != DialogResult.Yes)
                    {
                        return -1;
                    }
                    int result = 1;
                    if (!int.TryParse((this._TemplateXml.SelectSingleNode("/NurseForm") as XmlElement).GetAttribute("AutoCreatPagesCount"), out result))
                    {
                        result = 1;
                    }
                    string[] name = new string[] { "NurseForm", "Forms" };
                    int count = this._RecruitXML.SelectSingleNode("//" + EntXmlModel.GetName(name)).ChildNodes.Count;
                    int pageCurrent = this.uc_Pager1.PageCurrent;
                    int num4 = 0;
                    string[] textArray2 = new string[] { "NurseForm", "Forms" };
                    if (this._RecruitXML.SelectSingleNode(EntXmlModel.GetName(textArray2)) != null)
                    {
                        iType = result;
                        int num5 = this.uc_Pager1.PageCurrent;
                        if (result > 1)
                        {
                            int num6 = pageCurrent % result;
                            num5 = (((pageCurrent / result) - ((num6 == 0) ? 1 : 0)) * result) + 1;
                        }
                        for (int i = num5; i < (num5 + result); i++)
                        {
                            string[] textArray3 = new string[] { "NurseForm", "Forms", "Form" };
                            XmlNode oldChild = this._RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt("SN", i.ToString(), "=", textArray3));
                            if (oldChild != null)
                            {
                                string[] textArray4 = new string[] { "NurseForm", "Forms" };
                                this._RecruitXML.SelectSingleNode(EntXmlModel.GetName(textArray4)).RemoveChild(oldChild);
                            }
                        }
                        string[] textArray5 = new string[] { "NurseForm", "Forms" };
                        if (this._RecruitXML.SelectSingleNode(EntXmlModel.GetName(textArray5)).ChildNodes.Count > 0)
                        {
                            string[] textArray6 = new string[] { "NurseForm", "Forms" };
                            foreach (XmlNode node3 in this._RecruitXML.SelectSingleNode(EntXmlModel.GetName(textArray6)).ChildNodes)
                            {
                                num4 = int.Parse((node3 as XmlElement).GetAttribute("SN"));
                                if (num4 > num5)
                                {
                                    int num8 = num4 - result;
                                    (node3 as XmlElement).SetAttribute("SN", num8.ToString());
                                }
                            }
                        }
                    }
                    if (this._TableType && (this._TableInfor != null))
                    {
                        string[] textArray7 = new string[] { "NurseForm", "Records" };
                        XmlNode node4 = this._RecruitXML.SelectSingleNode(EntXmlModel.GetName(textArray7));
                        int num9 = 0;
                        for (int j = 0; j < (this._TableInfor.RowsCount * this._TableInfor.GroupColumnNum); j++)
                        {
                            string[] textArray8 = new string[] { "Record" };
                            XmlNode node5 = node4.SelectSingleNode(EntXmlModel.GetNameAt("ID", this._TableInfor.Rows[j].ID.ToString(), "=", textArray8));
                            if (node5 != null)
                            {
                                if ((node5 as XmlElement).GetAttribute("ID") == (node4 as XmlElement).GetAttribute("MaxID"))
                                {
                                    num9 = int.Parse((node4 as XmlElement).GetAttribute("MaxID")) - 1;
                                    (node4 as XmlElement).SetAttribute("MaxID", num9.ToString());
                                }
                                node4.RemoveChild(node5);
                            }
                        }
                        if (node4.ChildNodes.Count == 0)
                        {
                            (node4 as XmlElement).SetAttribute("MaxID", "");
                        }
                    }
                    this._isNeedSave = false;
                    this._RecruitXML_Org.InnerXml = this._RecruitXML.InnerXml;
                    return iType;
                }
                return -1;
            }
            catch (Exception exception)
            {
                Comm.LogHelp.WriteErr(exception);
                return -1;
            }
        }
        #endregion
        #endregion


        #region 新菜单方法
        /// <summary>
        /// 病历解封申请
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripButtonDelockApply_Click(object sender, EventArgs e)
        {
            ApplyNurseFormDelock?.Invoke(_patientInfo);
        }
        #endregion
    }
}
