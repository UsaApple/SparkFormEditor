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
using System.Linq;
using System.Xml.Linq;
using SparkFormEditor.Foundation;
using SparkFormEditor.CA;

namespace SparkFormEditor
{
    public partial class SparkFormEditor
    {
        #region 表格行操作：添加、删除行

        /// <summary>
        /// 删除行
        /// 如果纵向合并的行，被在上面插入新行后，导致正好挤到页尾删除报错，只能在上线删除行。恢复正常后，再操作。
        /// </summary>
        private void TableDeleteRow()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                //不存在表格，或者没有选择当前行的时候，不能进行删除操作
                if (!_TableType || _TableInfor.CurrentCell == null)
                {
                    string msg = "请先选择需要删除的行。";
                    ShowInforMsg(msg);
                    MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ShowSearchRow();

                //已经被其他人签名的行，不能删除行
                if (_DSS.IntensiveCareSignedReadOnly)
                {
                    if (_TableInfor.Rows[_CurrentCellIndex.X].UserID != "")
                    {
                        if (GlobalVariable.LoginUserInfo.UserCode != _TableInfor.Rows[_CurrentCellIndex.X].UserID)
                        {
                            if (GetNurseLevel(GlobalVariable.LoginUserInfo.TitleName) > GetNurseLevel(_TableInfor.Rows[_CurrentCellIndex.X].NurseFormLevel))
                            {
                                //rightOk = true;
                            }
                            else
                            {
                                string msg = "选择行已被其他人签名，不能删除。";
                                ShowInforMsg(msg);
                                MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            //string msg = "选择行已被其他人签名，不能删除。";
                            //ShowInforMsg(msg);
                            //MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            //return;
                        }
                    }
                }

                GlobalVariable.DelRowCancelEventArg delRowArg = null;
                if (this.BeforeDelRow != null)
                {
                    delRowArg = new GlobalVariable.DelRowCancelEventArg(
                             _CurrentCellIndex.X + 1,
                             _TemplateName,
                             _TemplateID,
                             _patientInfo);
                    this.BeforeDelRow(this, delRowArg);
                }
                if (delRowArg != null && delRowArg.Cancel == true)
                {
                    return;
                }

                if (MessageBox.Show("确定要删除第 " + (_CurrentCellIndex.X + 1).ToString() + " 行吗？", "删除行确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {

                    //先判断:被删除的行的单元格中:有没有纵向合并单元格
                    //如果是合并的主对象单元格:下移到下一个单元格,并 - 1；如果是被合并掉的单元格，那么主对象的RowSpan - 1
                    //修改表格对象_TableInfor的同时，修改xml数据。因为单元格保存到xml是在光标离开单元格是触发的。
                    string value = "";
                    string[] arr;
                    XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                    int nodeId;
                    XmlNode cellNode;
                    string mergeCellInfor = "";
                    string merge = "";

                    # region 处理纵向合并的单元格
                    //-------------如果有纵向合并单元格，那么需要更新设置
                    for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                    {
                        if (_TableInfor.Cells[_CurrentCellIndex.X, j].IsMerged() && _TableInfor.Cells[_CurrentCellIndex.X, j].RowSpan > 0)
                        {
                            //如果该行下面还有行：如果纵向合并的行，被在上面插入行，在删除的时候就会异常
                            if (_CurrentCellIndex.X < _TableInfor.GroupColumnNum * _TableInfor.RowsCount - 1) //如果是纵向合并多行，那么将下一行设置为合并的第一行，合并行数减1
                            {
                                _TableInfor.Cells[_CurrentCellIndex.X + 1, j].IsMergedNotShow = false;
                                _TableInfor.Cells[_CurrentCellIndex.X + 1, j].RowSpan = _TableInfor.Cells[_CurrentCellIndex.X, j].RowSpan - 1;

                                _TableInfor.Cells[_CurrentCellIndex.X + 1, j].CellSize = new Size(_TableInfor.Cells[_CurrentCellIndex.X, j].CellSize.Width,
                                                                                        _TableInfor.Cells[_CurrentCellIndex.X, j].CellSize.Height - _TableInfor.PageRowsHeight);
                                _TableInfor.Cells[_CurrentCellIndex.X + 1, j].Rect = new Rectangle(_TableInfor.Cells[_CurrentCellIndex.X + 1, j].Loaction, _TableInfor.Cells[_CurrentCellIndex.X + 1, j].CellSize);

                                //更新到xml中，
                                nodeId = _TableInfor.Rows[_CurrentCellIndex.X + 1].ID; //先取得删除行的合并单元格信息
                                //"Record[@ID='" + nodeId.ToString() + "']"
                                cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));
                                mergeCellInfor = (cellNode as XmlElement).GetAttribute(_TableInfor.Columns[j].RTBE.Name);
                                merge = "C," + _TableInfor.Cells[_CurrentCellIndex.X + 1, j].RowSpan.ToString() + ",False";

                            } //纵向合并后 删除会超出索引。反正和合并的已经在显示的时候不处理了。
                            else
                            {
                                //更新到xml中，
                                //nodeId = _TableInfor.Rows[_CurrentCellIndex.X + 1].ID; //先取得删除行的合并单元格信息
                                //"Record[@ID='" + _TableInfor.Rows[_CurrentCellIndex.X].ID + "']"
                                cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString(), "=", nameof(EntXmlModel.Record)));
                                cellNode = cellNode.NextSibling;

                                if (cellNode != null)
                                {
                                    nodeId = int.Parse((cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));
                                    mergeCellInfor = (cellNode as XmlElement).GetAttribute(_TableInfor.Columns[j].RTBE.Name);
                                    merge = "C," + (_TableInfor.Cells[_CurrentCellIndex.X, j].RowSpan - 1).ToString() + ",False";
                                }

                                //但是这个时候，删除行后，界面不会立刻刷新成正确样式，需要重新打开这一页。不会把立刻把下一页需要显示的行拿过来，在进行合并。
                                //只有在重新打开这一页，载入也信息的时候，才会更新
                            }

                            ////更新到xml中，
                            //nodeId = _TableInfor.Rows[_CurrentCellIndex.X + 1].ID; //先取得删除行的合并单元格信息
                            //cellNode = recordsNode.SelectSingleNode("Record[@ID='" + nodeId.ToString() + "']");
                            //mergeCellInfor = (cellNode as XmlElement).GetAttribute(_TableInfor.Columns[j].RTBE.Name);
                            //merge = "C," + _TableInfor.Cells[_CurrentCellIndex.X + 1, j].RowSpan.ToString() + ",False";

                            if (cellNode != null)
                            {
                                if (string.IsNullOrEmpty(mergeCellInfor))
                                {
                                    value = "¤False¤False¤" + GlobalVariable.LoginUserInfo.UserCode + "¤False¤False¤False¤" + merge + "¤Black¤";
                                }
                                else
                                {
                                    arr = mergeCellInfor.Split('¤');

                                    //如：R,2,False ; R,"",True
                                    arr[7] = merge;

                                    value = "";

                                    //再次拼接
                                    for (int i = 0; i < arr.Length; i++)
                                    {
                                        if (i == 0)
                                        {
                                            value = arr[i];
                                        }
                                        else
                                        {
                                            value += "¤" + arr[i];
                                        }

                                    }
                                }

                                (cellNode as XmlElement).SetAttribute(_TableInfor.Columns[j].RTBE.Name, value);      //合并单元格
                            }
                        }
                        else if (_TableInfor.Cells[_CurrentCellIndex.X, j].IsMergedNotShow)
                        {
                            //网上遍历，找到第一个IsMerged()的单元格，RowSpan-1 ：防止被合并的行删除后，还往下合并其他行。
                            for (int i = _CurrentCellIndex.X; i >= 0; i--)
                            {
                                if (_TableInfor.Cells[i, j].IsMerged() && _TableInfor.Cells[i, j].RowSpan > 0)
                                {
                                    //界面上表格信息修改
                                    _TableInfor.Cells[i, j].RowSpan = _TableInfor.Cells[i, j].RowSpan - 1;

                                    _TableInfor.Cells[i, j].CellSize = new Size(_TableInfor.Cells[i, j].CellSize.Width, _TableInfor.Cells[i, j].CellSize.Height - _TableInfor.PageRowsHeight);
                                    _TableInfor.Cells[i, j].Rect = new Rectangle(_TableInfor.Cells[i, j].Loaction, _TableInfor.Cells[i, j].CellSize);

                                    //xml数据更新
                                    //更新到xml中，
                                    nodeId = _TableInfor.Rows[i].ID; //先取得删除行的合并单元格信息
                                    cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));

                                    mergeCellInfor = (cellNode as XmlElement).GetAttribute(_TableInfor.Columns[j].RTBE.Name);

                                    merge = "C," + _TableInfor.Cells[i, j].RowSpan.ToString() + ",False";

                                    if (string.IsNullOrEmpty(mergeCellInfor))
                                    {
                                        value = "¤False¤False¤" + GlobalVariable.LoginUserInfo.UserCode + "¤False¤False¤False¤" + merge + "¤Black¤";
                                    }
                                    else
                                    {
                                        arr = mergeCellInfor.Split('¤');

                                        //如：R,2,False ; R,"",True
                                        arr[7] = merge;

                                        value = "";

                                        //再次拼接
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

                                    (cellNode as XmlElement).SetAttribute(_TableInfor.Columns[j].RTBE.Name, value);     //合并单元格

                                    break;
                                }
                            }
                        }
                    }
                    //-------------如果有纵向合并单元格，那么需要更新设置
                    # endregion 处理纵向合并的单元格

                    //-------------更新到xml中，删除本行对应的节点             
                    nodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID;
                    cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));
                    recordsNode.RemoveChild(cellNode); //将对应节点删除
                    //-------------更新到xml中，删除对应的节点

                    _IsLoading = true;
                    _IsNotNeedCellsUpdate = true;
                    Point loaction;

                    //先隐藏所有的表格的输入框
                    for (int col = 0; col < _TableInfor.ColumnsCount; col++)
                    {
                        if (_TableInfor.Columns[col].RTBE.Visible)
                        {
                            _TableInfor.Columns[col].RTBE.Visible = false;
                        }
                    }

                    //从选择行开始，将下一行的数据上移
                    XmlNode recordNode;
                    int recordIndex;

                    for (int i = _CurrentCellIndex.X; i < _TableInfor.RowsCount * _TableInfor.GroupColumnNum; i++)
                    {
                        for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                        {
                            loaction = _TableInfor.Cells[i, j].Loaction;

                            if (i < _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
                            {
                                CopyCell(_TableInfor.Cells[i, j], _TableInfor.Cells[i + 1, j]);

                                //_TableInfor.Rows[i].RowForeColor = _TableInfor.Rows[i + 1].RowForeColor;
                            }
                            else
                            {
                                //如果已经是最后一行，重置该行的所有单元格CellInfor信息，和该行RowInfor信息
                                RestCell(_TableInfor, _TableInfor.Cells[i, j]); //RestCell(_TableInfor.Cells[i, j]);

                            }

                        }

                        //行信息修改
                        _TableInfor.Rows[i].Loaction = _TableInfor.Cells[i, 0].Loaction;
                        _TableInfor.Rows[i].RowIndex = i;

                        if (i < _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
                        {
                            CopyRow(_TableInfor.Rows[i], _TableInfor.Rows[i + 1]);
                        }
                        else
                        {
                            //最后一行重置，
                            RestRow(_TableInfor.Rows[i]);
                        }

                        //最后一行时；判断xml数据中有无下一行，如果有拿过来显示
                        if (i == _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
                        {
                            recordIndex = _TableInfor.RowsCount * _TableInfor.GroupColumnNum * (int.Parse(_CurrentPage) - 1) + i;

                            //存在后面的数据，就把后面页的数据拿过来，填补到最后一行；否则新建一个新的节点
                            if (recordIndex < recordsNode.ChildNodes.Count)
                            {
                                recordNode = recordsNode.ChildNodes[recordIndex];
                                XmlNode2Row(_TableInfor, i, recordNode);
                            }
                            else
                            {
                                //添加对应的xml空白新节点
                                //-------------更新到xml中，在对应位置添加对应的节点
                                nodeId = _TableInfor.Rows[_TableInfor.RowsCount * _TableInfor.GroupColumnNum - 2].ID;//最后第二行
                                cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));
                                XmlElement newNode = _RecruitXML.CreateElement(nameof(EntXmlModel.Record));
                                int temp = _TableInfor.AddRowGetMaxID();
                                (newNode as XmlElement).SetAttribute(nameof(EntXmlModel.ID), temp.ToString());

                                //设置表格实体中行的ID，在表格单元格失去焦点保存 数据的时候，根据这个ID更新xml中的数据
                                _TableInfor.Rows[_TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1].ID = temp;

                                (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), temp.ToString());

                                recordsNode.InsertAfter(newNode, cellNode);
                                //-------------更新到xml中，在对应位置添加对应的节点
                            }
                        }
                    }

                    Redraw();

                    _IsLoading = false;
                    _IsNotNeedCellsUpdate = false;
                    _TableInfor.CurrentCell = null;

                    IsNeedSave = true;

                    UpdateErrorList(true, _CurrentCellIndex.X);

                    #region 判断：病人列表中的页节点，是否需要添加页码

                    //删除最后一页的任意行，不会影响页码，最多做后一页变成空页
                    //if (_CurrentPage != this.formPager1.PageCount.ToString())//_CurrentTemplateNameTreeNode.Nodes.Count.ToString())
                    //{
                    int maxNoEmptyRows = GetTableRows();        //根据表格数据，得到当前的行数
                    int pages = GetPageCount(maxNoEmptyRows);   //计算得到页数

                    //如果页数发生改变，在行操作的时候，只可能引起一页的变化
                    if (pages < this.uc_Pager1.PageCount)//_CurrentTemplateNameTreeNode.Nodes.Count)
                    {
                        this.uc_Pager1.PageCount = pages;
                        this.uc_Pager1.SetMaxCount(pages);
                        this.uc_Pager1.Bind();
                        //删除页码。现在有个弊端，如果删除行导致少了最后一个页码，那么取消保存的话，该页码不能恢复显示出来了。需要新建页
                        //foreach (TreeNode tn in _CurrentTemplateNameTreeNode.Nodes)
                        //{
                        //    if (tn.Text == _CurrentTemplateNameTreeNode.Nodes.Count.ToString())
                        //    {
                        //        _CurrentTemplateNameTreeNode.Nodes.Remove(tn);
                        //    }
                        //}
                    }
                    //}
                    if (this.AfterDelRow != null)
                    {
                        this.AfterDelRow(this, new GlobalVariable.DelRowEventArg
                             (_CurrentCellIndex.X + 1,
                             _TemplateName,
                             _TemplateID,
                             _patientInfo
                            ));
                    }
                    #endregion 判断：病人列表中的页节点，是否需要添加页码
                }

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;
                Comm.LogHelp.WriteErr(ex);
                _TableInfor.CurrentCell = null;
                throw ex;
            }
            finally
            {
                this.Cursor = Cursors.Default;
                //_TableInfor.CurrentCell = null;
                lblShowSearch.Hide();
                //this.pictureBoxBackGround.Refresh();
            }
        }

        /// <summary>
        /// 向下插入行
        /// </summary>
        /// <param name="sender"></param>
        private bool TableInsertRowToDown(object sender, int? mergeFisrtCol = null, int? mergeColCount = null)
        {
            try
            {
                if (!_TableType || _TableInfor.CurrentCell == null)
                {
                    string msg = "请先选择需要进行添加的行的位置。";
                    ShowInforMsg(msg);
                    MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                int x = 0;
                int y = 0;

                //考虑到分栏的情况，为了让显示效果更好
                if (_TableInfor.GroupColumnNum > 1 && _TableInfor.CurrentCell.Field > 0)
                {
                    x = _TableInfor.Rows[_CurrentCellIndex.X - _TableInfor.CurrentCell.Field * _TableInfor.RowsCount].Loaction.X + 1 + _TableInfor.CurrentCell.Field * _TableInfor.PageRowsWidth;
                    y = _TableInfor.Rows[_CurrentCellIndex.X - _TableInfor.CurrentCell.Field * _TableInfor.RowsCount].Loaction.Y + 3;
                }
                else
                {
                    x = _TableInfor.Rows[_CurrentCellIndex.X].Loaction.X + 1;
                    y = _TableInfor.Rows[_CurrentCellIndex.X].Loaction.Y + 3;
                }

                ShowSearch(new Point(x, y));//显示手指位置

                this.Cursor = Cursors.WaitCursor;

                if (_CurrentCellIndex.X == _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
                {
                    string msg = "当前位置已经是最底行，不能在下面再添加行，请先添加页。";
                    ShowInforMsg(msg);
                    MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return false;
                }

                //因为是向下插入行，所以判断当前行的下一行是否是否为合并的，那么线拆分，不然处理会乱。
                for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                {
                    if ((_TableInfor.Cells[_CurrentCellIndex.X, j].IsMerged() || _TableInfor.Cells[_CurrentCellIndex.X, j].IsMergedNotShow)
                         && (_TableInfor.Cells[_CurrentCellIndex.X, j].RowSpan > 0 || !_TableInfor.Cells[_CurrentCellIndex.X, j].MergedRight))
                    {
                        string msg = "目标位置有纵向合并的单元格，为避免表格混乱，请先拆分单元格后再插入行。";
                        ShowInforMsg(msg);
                        MessageBox.Show(msg, "插入行警告，操作不能正常进行。", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        return false;
                    }
                }

                GlobalVariable.InsertRowCancelEventArg insertArg = null;
                if (this.BeforeInsertRow != null)
                {
                    insertArg = new GlobalVariable.InsertRowCancelEventArg(
                        _CurrentCellIndex.X + 1,
                        this._TemplateName,
                        this._TemplateID,
                        this._patientInfo,
                        InsertRowType.Down);
                    this.BeforeInsertRow(this, insertArg);
                };
                if (insertArg != null && insertArg.Cancel == true)
                {
                    return false;
                }


                #region DownInsertRow 脚本
                //用户操作的时候sender不为null，如果是调整段落或小结总结的时候程序自动插入行就是null那么不要验证了。
                //如果要在已经小结和总结的基础上进行插入补数据，那么开始排序。或者删除合计行，然后在补加行数据
                if (sender != null && _node_Script != null && _node_Script.ChildNodes.Count > 0)  //if (_node_Script != null && _node_Script.ChildNodes.Count > 0)  发现下面报null异常
                {
                    foreach (XmlNode xn in _node_Script.ChildNodes)
                    {
                        //如果是注释那么跳过
                        if (xn.Name == @"#comment" || xn.Name == @"#text")
                        {
                            continue;
                        }

                        //找到当前控件，对应的脚本，多个事件合并共用
                        //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == ctl.Name && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)) == "DoubleClick")
                        if (ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains(nameof(EntXmlModel.DownInsertRow)))
                        {
                            bool isTableScript = false;
                            if (!bool.TryParse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.IsTable)), out isTableScript))
                            {
                                isTableScript = false;
                            }

                            if (isTableScript)    //表格脚本执行
                            {
                                if (_TableType && _TableInfor != null)
                                {
                                    //表格选中了某行，存在处理对象当前行
                                    if (_CurrentCellIndex.X != -1 && _TableInfor.CurrentCell != null)
                                    {
                                        this.Cursor = Cursors.WaitCursor;

                                        //,如果一个段落的话前面是签在最后一行。只能根据_TableInfor.Rows[i].UserID 来判断是否签名
                                        if (!Evaluator.EvaluateRunString(_TableInfor, ScriptPara(xn.InnerText.Trim())))
                                        {
                                            this.Cursor = Cursors.Default;

                                            return false;
                                        }

                                        this.Cursor = Cursors.Default;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion DownInsertRow

                //sender == null的时候，是自动调整段落列，就不需要确认消息框了。
                if (sender == null || MessageBox.Show("确定要在第 " + (_CurrentCellIndex.X + 1).ToString() + " 行的往下位置添加行吗？", "向下插入行确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SaveToOp();

                    _IsLoading = true;
                    _IsNotNeedCellsUpdate = true;
                    Point loaction;

                    for (int col = 0; col < _TableInfor.ColumnsCount; col++)
                    {
                        if (_TableInfor.Columns[col].RTBE.Visible)
                        {
                            _TableInfor.Columns[col].RTBE.Visible = false;
                        }
                    }

                    //从选择行开始，将下一行的数据上移
                    for (int i = _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1; i > _CurrentCellIndex.X + 1; i--)
                    {
                        for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                        {
                            loaction = _TableInfor.Cells[i, j].Loaction;

                            CopyCell(_TableInfor.Cells[i, j], _TableInfor.Cells[i - 1, j]);

                        }

                        //行信息修改
                        _TableInfor.Rows[i].Loaction = _TableInfor.Cells[i, 0].Loaction;
                        _TableInfor.Rows[i].RowIndex = i;

                        CopyRow(_TableInfor.Rows[i], _TableInfor.Rows[i - 1]);
                    }

                    //将添加的行的单元格重置，因为现在还是原来行的单元格的信息
                    for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                    {
                        loaction = _TableInfor.Cells[_CurrentCellIndex.X + 1, j].Loaction;

                        RestCell(_TableInfor, _TableInfor.Cells[_CurrentCellIndex.X + 1, j]); //RestCell(_TableInfor.Cells[_CurrentCellIndex.X + 1, j]);
                    }

                    //行：
                    //重置该行：
                    RestRow(_TableInfor.Rows[_CurrentCellIndex.X + 1]);

                    //-------------更新到xml中，在对应位置添加对应的节点
                    XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                    int nodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID;
                    XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));
                    XmlElement newNode = _RecruitXML.CreateElement(nameof(EntXmlModel.Record));
                    int temp = _TableInfor.AddRowGetMaxID();
                    (newNode as XmlElement).SetAttribute(nameof(EntXmlModel.ID), temp.ToString());

                    //设置表格实体中行的ID，在表格单元格失去焦点保存 数据的时候，根据这个ID更新xml中的数据
                    _TableInfor.Rows[_CurrentCellIndex.X + 1].ID = temp;

                    int maxId = GetTableRows() + 1;
                    (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), maxId.ToString());

                    XmlNode thisRowNode = recordsNode.InsertAfter(newNode, cellNode);
                    //-------------更新到xml中，在对应位置添加对应的节点

                    //如果插入行有上一个节点，那么默认集成上一行的签名信息
                    if (thisRowNode != null && thisRowNode.NextSibling != null)
                    {
                        string userId = (thisRowNode.NextSibling as XmlElement).GetAttribute(nameof(EntXmlModel.UserID));
                        string[] arrUserID = null;

                        if (!string.IsNullOrEmpty(userId) && userId == (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.UserID)))
                        {
                            //防止两个段落中间，另外一个用户要插入行添加段落，那么
                            if (string.IsNullOrEmpty((cellNode as XmlElement).GetAttribute("签名").Split('¤')[0]))
                            {
                                (thisRowNode as XmlElement).SetAttribute(nameof(EntXmlModel.UserID), userId);
                                //_TableInfor.Rows[_CurrentCellIndex.X + 1].UserID = userId.Split('¤')[0];

                                arrUserID = userId.Split('¤');
                                _TableInfor.Rows[_CurrentCellIndex.X + 1].UserID = arrUserID[0];

                                if (arrUserID.Length > 1)
                                {
                                    _TableInfor.Rows[_CurrentCellIndex.X + 1].UserName = arrUserID[1];
                                }
                            }
                        }
                    }


                    Redraw();

                    _IsLoading = false;
                    _IsNotNeedCellsUpdate = false;

                    _TableInfor.CurrentCell = null;

                    IsNeedSave = true;

                    UpdateErrorList(false, _CurrentCellIndex.X);

                    #region 判断：病人列表中的页节点，是否需要添加页码

                    int maxNoEmptyRows = GetTableRows();        //根据表格数据，得到当前的行数
                    int pages = GetPageCount(maxNoEmptyRows);   //计算得到页数

                    //如果页数发生改变，在行操作的时候，只可能引起一页的变化
                    if (pages > this.uc_Pager1.PageCount)//_CurrentTemplateNameTreeNode.Nodes.Count)
                    {
                        //添加页码
                        AdddPageNode_ForClick(this.uc_Pager1.PageCount);
                        this.uc_Pager1.Bind();
                        _IsCreating = true;
                    }

                    if (this.AfterInsertRow != null)
                    {
                        this.AfterInsertRow(this, new GlobalVariable.InsertRowEventArg(
                            _CurrentCellIndex.X + 1,
                            this._TemplateName,
                            this._TemplateID,
                            this._patientInfo,
                            InsertRowType.Down));
                    };
                    #endregion 判断：病人列表中的页节点，是否需要添加页码
                }

                this.Cursor = Cursors.Default;
                return true;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;
                ShowInforMsg(ex.StackTrace);
                return false;
                throw ex;
            }
            finally
            {
                this.Cursor = Cursors.Default;

                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;

                this.Cursor = Cursors.Default;
                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;

                lblShowSearch.Hide();
                //this.pictureBoxBackGround.Refresh();
            }
        }

        /// <summary>
        /// 向上插入行
        /// </summary>
        /// <param name="sender"></param>
        private void TableInsertRowToUp(object sender)
        {
            try
            {
                if (!_TableType || _TableInfor.CurrentCell == null)
                {
                    string msg = "请先选择需要进行添加的行的位置。";
                    ShowInforMsg(msg);
                    MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //因为是向下插入行，所以判断当前行的下一行是否是否为合并的，那么线拆分，不然处理会乱。
                for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                {
                    if ((_TableInfor.Cells[_CurrentCellIndex.X, j].IsMerged() || _TableInfor.Cells[_CurrentCellIndex.X, j].IsMergedNotShow)
                         && (_TableInfor.Cells[_CurrentCellIndex.X, j].RowSpan > 0 || !_TableInfor.Cells[_CurrentCellIndex.X, j].MergedRight))
                    {
                        string msg = "目标位置有纵向合并的单元格，为避免表格混乱，请先拆分单元格后再插入行。";
                        ShowInforMsg(msg);
                        MessageBox.Show(msg, "插入行警告，操作不能正常进行", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        return;
                    }
                }
                GlobalVariable.InsertRowCancelEventArg insertArg = null;
                if (this.BeforeInsertRow != null)
                {
                    insertArg = new GlobalVariable.InsertRowCancelEventArg(
                        _CurrentCellIndex.X + 1,
                        this._TemplateName,
                        this._TemplateID,
                        this._patientInfo,
                        InsertRowType.Up);
                    this.BeforeInsertRow(this, insertArg);
                };
                if (insertArg != null && insertArg.Cancel == true)
                {
                    return;
                }

                #region UpInsertRow 脚本
                //用户操作的时候sender不为null，如果是调整段落或小结总结的时候程序自动插入行就是null那么不要验证了。
                //如果要在已经小结和总结的基础上进行插入补数据，那么开始排序。或者删除合计行，然后在补加行数据
                if (sender != null && _node_Script != null && _node_Script.ChildNodes.Count > 0)  //if (_node_Script != null && _node_Script.ChildNodes.Count > 0) 发现下面报null异常
                {

                    foreach (XmlNode xn in _node_Script.ChildNodes)
                    {
                        //如果是注释那么跳过
                        if (xn.Name == @"#comment" || xn.Name == @"#text")
                        {
                            continue;
                        }

                        //找到当前控件，对应的脚本，多个事件合并共用
                        //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == ctl.Name && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)) == "DoubleClick")
                        if (ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains("UpInsertRow"))
                        {
                            bool isTableScript = false;
                            if (!bool.TryParse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.IsTable)), out isTableScript))
                            {
                                isTableScript = false;
                            }

                            if (isTableScript)    //表格脚本执行
                            {
                                if (_TableType && _TableInfor != null)
                                {
                                    //表格选中了某行，存在处理对象当前行
                                    if (_CurrentCellIndex.X != -1 && _TableInfor.CurrentCell != null)
                                    {
                                        this.Cursor = Cursors.WaitCursor;

                                        //,如果一个段落的话前面是签在最后一行。只能根据_TableInfor.Rows[i].UserID 来判断是否签名
                                        if (!Evaluator.EvaluateRunString(_TableInfor, ScriptPara(xn.InnerText.Trim())))
                                        {
                                            this.Cursor = Cursors.Default;

                                            return;
                                        }

                                        this.Cursor = Cursors.Default;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion UpInsertRow

                this.Cursor = Cursors.WaitCursor;

                int x = 0;
                int y = 0;

                //考虑到分栏的情况，为了让显示效果更好
                if (_TableInfor.GroupColumnNum > 1 && _TableInfor.CurrentCell.Field > 0)
                {
                    x = _TableInfor.Rows[_CurrentCellIndex.X - _TableInfor.CurrentCell.Field * _TableInfor.RowsCount].Loaction.X + 1 + _TableInfor.CurrentCell.Field * _TableInfor.PageRowsWidth;
                    y = _TableInfor.Rows[_CurrentCellIndex.X - _TableInfor.CurrentCell.Field * _TableInfor.RowsCount].Loaction.Y + 3;
                }
                else
                {
                    x = _TableInfor.Rows[_CurrentCellIndex.X].Loaction.X + 1;
                    y = _TableInfor.Rows[_CurrentCellIndex.X].Loaction.Y + 3;
                }

                ShowSearch(new Point(x, y));//显示手指位置

                if (MessageBox.Show("确定要在第 " + (_CurrentCellIndex.X + 1).ToString() + " 行的往上位置添加行吗？", "向上插入行确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SaveToOp();

                    _IsLoading = true;
                    _IsNotNeedCellsUpdate = true;
                    Point loaction;

                    for (int col = 0; col < _TableInfor.ColumnsCount; col++)
                    {
                        if (_TableInfor.Columns[col].RTBE.Visible)
                        {
                            _TableInfor.Columns[col].RTBE.Visible = false;
                        }
                    }

                    //从选择行开始，将下一行的数据上移
                    for (int i = _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1; i > _CurrentCellIndex.X; i--)
                    {
                        for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                        {
                            loaction = _TableInfor.Cells[i, j].Loaction;

                            CopyCell(_TableInfor.Cells[i, j], _TableInfor.Cells[i - 1, j]);

                            _TableInfor.Rows[i].RowForeColor = _TableInfor.Rows[i - 1].RowForeColor;
                        }

                        //行信息修改
                        _TableInfor.Rows[i].Loaction = _TableInfor.Cells[i, 0].Loaction;
                        _TableInfor.Rows[i].RowIndex = i;

                        CopyRow(_TableInfor.Rows[i], _TableInfor.Rows[i - 1]);
                    }

                    //将添加的行的单元格重置，因为现在还是原来行的单元格的信息
                    for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                    {
                        loaction = _TableInfor.Cells[_CurrentCellIndex.X, j].Loaction;


                        RestCell(_TableInfor, _TableInfor.Cells[_CurrentCellIndex.X, j]); //RestCell(_TableInfor.Cells[_CurrentCellIndex.X, j]);

                    }

                    //行：
                    //添加的空白行，行重置，
                    RestRow(_TableInfor.Rows[_CurrentCellIndex.X]);

                    //-------------更新到xml中，在对应位置添加对应的节点
                    XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                    int nodeId = 0;
                    XmlNode cellNode = null;
                    XmlNode thisRowNode = null;

                    if (_CurrentCellIndex.X == _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
                    {
                        nodeId = _TableInfor.Rows[_TableInfor.RowsCount * _TableInfor.GroupColumnNum - 2].ID;

                        cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));
                        XmlElement newNode = _RecruitXML.CreateElement(nameof(EntXmlModel.Record));
                        int temp = _TableInfor.AddRowGetMaxID();
                        (newNode as XmlElement).SetAttribute(nameof(EntXmlModel.ID), temp.ToString());

                        //设置表格实体中行的ID，在表格单元格失去焦点保存 数据的时候，根据这个ID更新xml中的数据
                        _TableInfor.Rows[_CurrentCellIndex.X].ID = temp;

                        int maxId = GetTableRows() + 1;
                        (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), maxId.ToString());

                        //在当前行的下行往上插入
                        thisRowNode = recordsNode.InsertAfter(newNode, cellNode);
                    }
                    else
                    {
                        nodeId = _TableInfor.Rows[_CurrentCellIndex.X + 1].ID;

                        cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));
                        XmlElement newNode = _RecruitXML.CreateElement(nameof(EntXmlModel.Record));
                        int temp = _TableInfor.AddRowGetMaxID();
                        (newNode as XmlElement).SetAttribute(nameof(EntXmlModel.ID), temp.ToString());

                        //设置表格实体中行的ID，在表格单元格失去焦点保存 数据的时候，根据这个ID更新xml中的数据
                        _TableInfor.Rows[_CurrentCellIndex.X].ID = temp;

                        int maxId = GetTableRows() + 1;
                        (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), maxId.ToString());

                        //在当前行的下行往上插入
                        thisRowNode = recordsNode.InsertBefore(newNode, cellNode);
                    }
                    //-------------更新到xml中，在对应位置添加对应的节点

                    //如果插入行有上一个节点，那么默认集成上一行的签名信息
                    //如果插入行有上一个节点，那么默认集成上一行的签名信息
                    if (thisRowNode != null && thisRowNode.PreviousSibling != null)
                    {
                        string[] arrUserID = null;
                        string userId = (thisRowNode.PreviousSibling as XmlElement).GetAttribute(nameof(EntXmlModel.UserID));
                        if (!string.IsNullOrEmpty(userId) && userId == (cellNode as XmlElement).GetAttribute(nameof(EntXmlModel.UserID)))
                        {
                            //防止两个段落中间，另外一个用户要插入行添加段落，那么
                            if (string.IsNullOrEmpty((thisRowNode.PreviousSibling as XmlElement).GetAttribute("签名").Split('¤')[0]))
                            {
                                (thisRowNode as XmlElement).SetAttribute(nameof(EntXmlModel.UserID), userId);
                                arrUserID = userId.Split('¤');
                                _TableInfor.Rows[_CurrentCellIndex.X].UserID = arrUserID[0];

                                if (arrUserID.Length > 1)
                                {
                                    _TableInfor.Rows[_CurrentCellIndex.X].UserName = arrUserID[1];
                                }
                            }
                        }
                    }

                    Redraw();

                    _IsLoading = false;
                    _IsNotNeedCellsUpdate = false;

                    _TableInfor.CurrentCell = null;

                    IsNeedSave = true;

                    UpdateErrorList(false, _CurrentCellIndex.X - 1); //添加删除好行，要更新错误消息提示中的行号等信息

                    #region 判断：病人列表中的页节点，是否需要添加页码

                    int maxNoEmptyRows = GetTableRows();        //根据表格数据，得到当前的行数
                    int pages = GetPageCount(maxNoEmptyRows);   //计算得到页数

                    //如果页数发生改变，在行操作的时候，只可能引起一页的变化
                    if (pages > this.uc_Pager1.PageCount)//_CurrentTemplateNameTreeNode.Nodes.Count)
                    {
                        //添加页码
                        AdddPageNode_ForClick(this.uc_Pager1.PageCount);
                        this.uc_Pager1.Bind();
                        _IsCreating = true;
                    }
                    if (this.AfterInsertRow != null)
                    {
                        this.AfterInsertRow(this, new GlobalVariable.InsertRowEventArg(
                            _CurrentCellIndex.X + 1,
                        this._TemplateName,
                        this._TemplateID,
                        this._patientInfo,
                        InsertRowType.Up));
                    };
                    #endregion 判断：病人列表中的页节点，是否需要添加页码
                }

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;
                ShowInforMsg(ex.StackTrace);
                throw ex;
            }
            finally
            {
                this.Cursor = Cursors.Default;
                lblShowSearch.Hide();
                //this.pictureBoxBackGround.Refresh();
            }
        }

        /// <summary>
        /// 获取表格的行数（排除末尾的红行）
        /// 用来计算表格实际是多少页
        /// </summary>
        /// <returns></returns>
        private int GetTableRows()
        {
            XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            int maxNoEmptyRows = recordsNode.ChildNodes.Count;
            if (recordsNode != null && recordsNode.ChildNodes.Count > 0)
            {
                XmlNode xn = null;

                for (int i = recordsNode.ChildNodes.Count - 1; i >= 0; i--)
                {
                    xn = recordsNode.ChildNodes[i];

                    if (Comm.isEmptyRecord(xn))
                    {
                        maxNoEmptyRows--;
                    }
                    else
                    {
                        return maxNoEmptyRows;
                        //break; //从后往前，遇到第一个非空白行，就跳出。得到总行数
                    }
                }
            }

            return maxNoEmptyRows;
        }

        /// <summary>
        /// 获取表单数据的行数，计算表格的页数
        /// </summary>
        /// <returns></returns>
        private int GetPageCount(int rows)
        {
            int pages = 0;

            XmlNode xn = _TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form), nameof(EntXmlModel.Table)));
            //int pageRows = 1;

            if (xn != null)
            {

                pages = TableClass.GetPageCount_HaveTable(_TemplateXml, rows);

                //int fileds = 1;

                //if (!int.TryParse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.GroupColumnNum)), out fileds))
                //{
                //    fileds = 1;
                //}

                //pageRows = int.Parse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.FormRows))) * fileds;
                //Forms = rows / pageRows;

                //if (rows % pageRows > 0)
                //{
                //    pages++;
                //}
            }
            else
            {
                //插入行，删除行，模板中肯定有表格的配置
                ShowInforMsg("操作表格行的时候出现异常，程序没有找到表单模板中的表格配置。");

                pages = this.uc_Pager1.PageCount;//_CurrentTemplateNameTreeNode.Nodes.Count;
            }


            return pages;
        }

        #endregion 表格行操作

        # region 双击单元格，赋默认值
        /// <summary>
        /// 双击单元格进行默认值自动填写：比如签名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cells_DoubleClickSetDefaulyValue(object sender, EventArgs e)
        {
            if (_IsLocked) return;
            RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;

            HorizontalAlignment tempHa = rtbe.SelectionAlignment;

            if (rtbe._MouseButton == MouseButtons.Right)
            {

            }
            else
            {
                #region 双击事件/Enter事件的脚本验证 可以执行表格脚本：判断赋值
                if (_node_Script != null && _node_Script.ChildNodes.Count > 0)
                {
                    foreach (XmlNode xn in _node_Script.ChildNodes)
                    {
                        //如果是注释那么跳过
                        if (xn.Name == @"#comment" || xn.Name == @"#text")
                        {
                            continue;
                        }

                        //找到当前控件，对应的脚本，多个事件合并共用
                        //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == ctl.Name && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)) == "DoubleClick")
                        if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == rtbe.Name && ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains("DoubleClick"))
                        {
                            bool isTableScript = false;
                            if (!bool.TryParse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.IsTable)), out isTableScript))
                            {
                                isTableScript = false;
                            }

                            //非表格的双击，执行这里
                            if (!rtbe._IsTable && !isTableScript)
                            {
                                if (!Evaluator.EvaluateRunString(this, ScriptPara(xn.InnerText.Trim())))
                                {
                                    return;
                                }
                            }
                            else if (rtbe._IsTable && isTableScript)    //表格脚本执行
                            {
                                if (_TableType && _TableInfor != null)
                                {
                                    //表格选中了某行，存在处理对象当前行
                                    if (_CurrentCellIndex.X != -1 && _TableInfor.CurrentCell != null)
                                    {
                                        this.Cursor = Cursors.WaitCursor;

                                        //签名.Text != '' Then Error(已经签名了哦)
                                        //日期.Text != '' Then 签名.Text = ''     

                                        //但是如果签了一次，再签的话；需要光标离开才能更新当前单元格啊,要触发一次保存
                                        Cells_LeaveSaveData(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE, null); //触发光标离开事件，进行保存 数据

                                        if (!Evaluator.EvaluateRunString(_TableInfor, ScriptPara(xn.InnerText.Trim())))
                                        {
                                            this.Cursor = Cursors.Default;

                                            return;
                                        }

                                        this.Cursor = Cursors.Default;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion 双击事件的脚本验证

                string VerifySign = "";
                string newID = "";
                string name = "";
                string level = "";

                if (rtbe._IsTable)
                {
                    switch (rtbe.Name)
                    {
                        //签名设置行的权限
                        case "记录者":
                        case "签名":

                            //可以在设定整个行的签名状态  User _TableInfor.CurrentCell.Text = GlobalVariable.LoginUserInfo.UserCode;
                            ShowSearchRow();//显示手指位置

                            if (!SignEnable())
                            {
                                return;
                            }

                            if (!_SignNeedConfirmMsg || MessageBox.Show("确定要对第 " + (_CurrentCellIndex.X + 1).ToString() + " 行进行签名吗？", "签名确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                //这里要先做控制，如果已经签名了，那么只有高级别的，护士长再对护士内容进行签名
                                if (_TableInfor.Rows[_CurrentCellIndex.X].UserID != "" && rtbe.Text.Trim() != "")
                                {
                                    //已经有了签名，要判断，是不是护士长审核签名，否则不能进行签名
                                    //MEDIC_DEGREE;nurselevel:表单权限:NurseFormLevel="实习护士¤护士¤护士长" 职位由低到高。

                                    # region 备用注释
                                    //select * from MEDIC_BASE

                                    //--update MEDIC_BASE
                                    //--set MEDIC_DEGREE='护士长'

                                    //int index = _NurseLevel.IndexOf("a");
                                    # endregion 备用注释

                                    if (_NurseLevel != "" || _SignIgnoreLevel)
                                    {
                                        //nameof(EntXmlModel.UserID), UserID + "¤" + UserName + "¤" + User_Degree
                                        if (GetNurseLevel(GlobalVariable.LoginUserInfo.TitleName) > GetNurseLevel(_TableInfor.Rows[_CurrentCellIndex.X].NurseFormLevel) || _SignIgnoreLevel)
                                        {
                                            //权限等级大于，上次签名者：比如护士长给小护士进行确认签名
                                            VerifySign = "";
                                        }
                                        else
                                        {
                                            //如果登录用户权限一样，那么弹出登录验证，获取可以二次签名的用户
                                            //审核签名，直接进行工号、密码验证

                                            Frm_SignLogin slf = new Frm_SignLogin(GlobalVariable.LoginUserInfo.UserCode, this);
                                            DialogResult res = slf.ShowDialog();

                                            if (res == DialogResult.OK)
                                            {
                                                //根据修改后的值进行设置显示
                                                newID = slf.NameID;
                                                name = slf.NameText;
                                                level = slf.MEDIC_DEGREE;

                                                m_formSettingsVerifySignID = newID;

                                                //防止同名同姓的人，实施配置的时候会在后面加点的
                                                if (name.Contains("."))
                                                {
                                                    name = name.Trim().TrimEnd('.');
                                                }

                                                VerifySign = name;

                                                if (VerifySign == GlobalVariable.LoginUserInfo.UserName || GetNurseLevel(level) <= GetNurseLevel(_TableInfor.Rows[_CurrentCellIndex.X].NurseFormLevel))
                                                {
                                                    MessageBox.Show("该行已经被签名了，只有上级管理者才能审核签名。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                //权限等级小于等于，上次签名者
                                                //MessageBox.Show("该行已经被签名了，只有上级管理者才能审核签名。");  //MessageBox.Show("该行已经被签名了，只有护士长才能审核签名。");
                                                //ShowInforMsg("该行已经被签名了，只有签名者本人才能直接修改，他人需用修订历史来修改。");
                                                return;
                                            }

                                            ////权限等级小于等于，上次签名者
                                            ////MessageBox.Show("该行已经被签名了，只有上级管理者才能审核签名。");  //MessageBox.Show("该行已经被签名了，只有护士长才能审核签名。");
                                            ShowInforMsg("该行已经被签名了，只有签名者本人才能直接修改，他人需用修订历史来修改。");
                                            //return;
                                        }
                                    }
                                    else
                                    {
                                        ShowInforMsg("该表单没有配置：NurseFormLevel医务人员权限等级，不能进行双签名操作。", false);
                                    }
                                }
                                else
                                {
                                    //某行，首次签名时候

                                    #region 同一段落禁止重复单签名
                                    ////该行（空行）没有签名的时候，但是上一行是同段落，且已经签名
                                    ////防止在已经签名的段落的下面一行空行继续签名，导致签名覆盖
                                    //if (_CurrentCellIndex.X > 0 && _TableInfor.CellByRowNo_ColName(0, "签名") != null && rtbe.Text.Trim() == "")
                                    //{
                                    //    //往上遍历、寻找该行的同一段落，都设置签名UserID=GlobalVariable.LoginUserInfo.UserCode；
                                    //    for (int i = _CurrentCellIndex.X; i >= 1; i--)
                                    //    {
                                    //        if (!NotSameDateTimeToPreRow(i))
                                    //        {
                                    //            //如果上一行所属一个段落（日期时间相同），已经被签名
                                    //            //if (_TableInfor.Rows[i - 1].UserID != "" && _TableInfor.Rows[i - 1].NurseFormLevel != "")
                                    //            if (_TableInfor.CellByRowNo_ColName(i - 1, "签名").Text.Trim() != "")
                                    //            {
                                    //                ShowSearchRow();//显示手指位置

                                    //                MessageBox.Show("该段落已签名，请确认该行位置，或者是否为空白行。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    //                return;

                                    //            }
                                    //            else
                                    //            {
                                    //                //不能跳过，可能中间还有未签名的行，或者有空行的话，就又限制不住了  //break;                                                   
                                    //            }
                                    //        }
                                    //        else
                                    //        {
                                    //            break;
                                    //        }
                                    //    }
                                    //}
                                    #endregion 同一段落禁止重复单签名

                                    #region 禁止空白行签名
                                    if (IsEmptyRow(_CurrentCellIndex.X))
                                    {
                                        ShowSearchRow();//显示手指位置

                                        MessageBox.Show("选择行为空行，不能签名。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return;
                                    }
                                    #endregion 禁止空白行签名

                                    //每次签名验证：输入当前工号的密码
                                    if (_SignNeedLogin)
                                    {
                                        if (CheckHelp.CheckCurrentLoginUser(GlobalVariable.LoginUserInfo.UserCode, this))
                                        {
                                            //通过验证
                                        }
                                        else
                                        {
                                            return; //没有通过验证
                                        }
                                    }
                                }

                                if (rtbe.Text.Trim() != "" && rtbe.Text != GlobalVariable.LoginUserInfo.UserName)
                                {
                                    //rtbe.Text = GlobalVariable.LoginUserInfo.UserName + "/" + rtbe.Text; //这里显示累计签名的用户名信息
                                    rtbe.Text = GetDoubleSignString(rtbe.Text, GlobalVariable.LoginUserInfo.UserName);
                                }
                                else if (VerifySign != "")
                                {
                                    //rtbe.Text = VerifySign + "/" + rtbe.Text;
                                    rtbe.Text = GetDoubleSignString(rtbe.Text, VerifySign);
                                }
                                else
                                {
                                    rtbe.Text = GlobalVariable.LoginUserInfo.UserName; //这里显示签名的用户名
                                }

                                //将签名信息设置到整个段落，而不是仅仅该行，保存到整个段落和Xml数据中
                                if (VerifySign != "")
                                {
                                    SaveSign(newID, name, level);
                                }
                                else
                                {
                                    SaveSign(GlobalVariable.LoginUserInfo.UserCode, GlobalVariable.LoginUserInfo.UserName, GlobalVariable.LoginUserInfo.TitleName);
                                }

                                //TODO:调试手膜签名
                                if (_FingerPrintSign)
                                {
                                    //手膜签名
                                    FingerPrintSign(rtbe, GlobalVariable.LoginUserInfo.UserCode, true);
                                }
                            }

                            break;

                        default:
                            //cellInfor.Text = _TableInfor.Columns[cellInfor.ColIndex].RTBE.Default;
                            if (rtbe.Name.StartsWith("签名"))
                            {
                                if (!SignEnable())
                                {
                                    return;
                                }

                                if (!_SignNeedConfirmMsg || MessageBox.Show("确定要对第 " + (_CurrentCellIndex.X + 1).ToString() + " 行进行签名吗？", "签名确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    //这里要先做控制，如果已经签名了，那么只有高级别的，护士长再对护士内容进行签名
                                    if (rtbe.Text.Trim() == "")
                                    {
                                        rtbe.Text = GlobalVariable.LoginUserInfo.UserName;
                                    }
                                    else
                                    {
                                        //如果已经有值了，名字是以前面开头，不正好是签名两个字，
                                        //rtbe.Text = GlobalVariable.LoginUserInfo.UserName + "/" + rtbe.Text;
                                        rtbe.Text = GetDoubleSignString(rtbe.Text, GlobalVariable.LoginUserInfo.UserName);
                                    }


                                    //TODO:调试手膜签名
                                    if (_FingerPrintSign)
                                    {
                                        //手膜签名
                                        FingerPrintSign(rtbe, GlobalVariable.LoginUserInfo.UserCode, true);
                                    }
                                }
                            }
                            else if (rtbe.Name.Contains("日期") || rtbe.Name.Contains("时间"))
                            {
                                //日期时间
                                if (!_DSS.DefaultDateEnterOrDoubleClick)
                                {
                                    if (string.IsNullOrEmpty(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Text.Trim()) &&
                                         !_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.ReadOnly)
                                    {
                                        //先走输入框内置的双击事件，打开日历
                                        Cells_EnterSetDefaulyValue(_TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y]);  //设置默认值，比如时间和工号

                                        //还要加载才行的
                                        _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Text = _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].Text;
                                    }
                                }
                            }
                            else
                            {
                                //双击弹出输入助理：
                                //complexPopupAssistant.LoadData(); //更新数据
                                //complex.Show(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE);

                            }

                            break;
                    }
                }
                else
                {
                    //非表格的，双击填入签名
                    if ((rtbe.Name.StartsWith("签名") || rtbe.Name.StartsWith("记录者"))) //!rtbe.ReadOnly &&  只读的有要双击可以签名
                    {
                        if (!SignEnable())
                        {
                            return;
                        }

                        if (!_SignNeedConfirmMsg || MessageBox.Show("确定要进行签名吗？", "签名确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            if (rtbe.Text.Trim() == "")
                            {
                                rtbe.Text = GlobalVariable.LoginUserInfo.UserName;
                            }
                            else
                            {
                                if (_NurseLevel != "")
                                {
                                    //审核签名，直接进行工号、密码验证
                                    Frm_SignLogin slf = new Frm_SignLogin(m_formSettingsVerifySignID, this);
                                    DialogResult res = slf.ShowDialog();

                                    if (res == DialogResult.OK)
                                    {
                                        //根据修改后的值进行设置显示
                                        newID = slf.NameID;
                                        name = slf.NameText;
                                        level = slf.MEDIC_DEGREE;

                                        m_formSettingsVerifySignID = newID;

                                        VerifySign = name;

                                        if (rtbe.Text != GlobalVariable.LoginUserInfo.UserName && GetNurseLevel(level) > 0) //非表格的不做等级判断，但是必须不是最低级的，就能确认审核签名
                                        {
                                            //rtbe.Text = VerifySign + "/" + rtbe.Text;
                                            rtbe.Text = GetDoubleSignString(rtbe.Text, VerifySign);
                                        }
                                    }
                                }
                            }

                            //TODO:调试手膜签名
                            if (_FingerPrintSign)
                            {
                                //手膜签名
                                FingerPrintSign(rtbe, GlobalVariable.LoginUserInfo.UserCode, true);
                            }
                        }
                    }
                    else if (rtbe.Name.Contains("日期") || rtbe.Name.Contains("时间"))
                    {

                        if (!_DSS.DefaultDateEnterOrDoubleClick)
                        {
                            if (string.IsNullOrEmpty(rtbe.Text.Trim()) && !rtbe.ReadOnly)
                            {
                                InputBox_EnterSetDefaulyValue(rtbe, null);  //设置默认值，比如时间和工号
                            }
                        }
                    }
                    else
                    {
                        //双击弹出输入助理：
                        //complexPopupAssistant.LoadData(); //更新数据
                        //complex.Show(rtbe);

                        ////不然弹出后，输入框看不到闪烁的焦点了
                        //if (_Need_Assistant_Control != null && !_Need_Assistant_Control.IsDisposed)
                        //{
                        //    _Need_Assistant_Control.Focus();

                        //    if (_Need_Assistant_Control is RichTextBoxExtended)
                        //    {
                        //        (_Need_Assistant_Control as RichTextBoxExtended).DoubleClickSelecting();
                        //    }
                        //}
                    }
                }
            }

            //双击打开附表：
            if (!string.IsNullOrEmpty(rtbe._AttachForm))
            {
                #region 如果是新建的表单，或者新建页，那么先添加好节点
                if (_RecruitXML == null)
                {
                    //创建表单数据的xml文件
                    TemplateHelp mx = new TemplateHelp();
                    _RecruitXML = mx.CreateTemplate(this._patientInfo.PatientId, this._patientInfo.PatientName, GlobalVariable.LoginUserInfo.UserCode, GlobalVariable.LoginUserInfo.DeptName);
                }

                XmlNode pages = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms)));

                XmlNode thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), _CurrentPage, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));

                //判断xml中是否已经存在本页的Page数据
                if (thisPage == null)
                {
                    //插入新的节点
                    XmlElement newPage;
                    newPage = _RecruitXML.CreateElement(nameof(EntXmlModel.Form));

                    newPage.SetAttribute(nameof(EntXmlModel.SN), _CurrentPage);

                    pages.AppendChild(newPage);

                    thisPage = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), _CurrentPage, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form)));
                }
                #endregion 如果是新建的表单，或者新建页，那么先添加好节点
                XmlNode attachTemplate = _TemplateXml.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.Name), rtbe._AttachForm, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Attach), nameof(EntXmlModel.Form)));
                //"Attach/Form[@Name='" + rtbe._AttachForm + "']"
                XmlNode currtAttach = thisPage.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.Name), rtbe._AttachForm, "=", nameof(EntXmlModel.Attach), nameof(EntXmlModel.Form)));
                string oldFormData = thisPage.OuterXml;

                if (attachTemplate != null)
                {
                    Frm_Attach af = new Frm_Attach(attachTemplate, thisPage, _RecruitXML);
                    DialogResult res = af.ShowDialog();

                    //属性Score说明 0：各项目累加的加法；大于0：基于这个数值作为被减数的减法；空：不返回合计，只是附表数据
                    if (res == DialogResult.OK)
                    {
                        //是否需要合计，需要的话，填入返回的合计结果
                        if (!string.IsNullOrEmpty((attachTemplate as XmlElement).GetAttribute(nameof(EntXmlModel.Score)).Trim()))
                        {
                            //合计结果:
                            rtbe.Text = af.SumResult.ToString();
                        }

                        //附表数据也要保存，这样可以追踪历史
                        if (oldFormData != af.FormData.OuterXml)
                        {
                            IsNeedSave = true;     //currtAttach = af.FormData;
                        }
                    }
                }
                else
                {
                    string errMsg = "找不到附表的模板配置信息：" + EntXmlModel.GetNameAt(nameof(EntXmlModel.Name), rtbe._AttachForm, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Attach), nameof(EntXmlModel.Form));
                    Comm.LogHelp.WriteErr(errMsg);
                    MessageBox.Show(errMsg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            rtbe.SelectionAlignment = tempHa;

            if ((rtbe.Name.StartsWith("签名") || rtbe.Name.StartsWith("记录者")))
            {
                rtbe.Select(rtbe.Text.Length, 0);
            }
            else
            {
                //双击光标位置
                rtbe.DoubleClickSelecting();
            }
        }

        /// <summary>
        /// 获取双签名的内容：
        /// 根据服务端属性OverturnSignature，默认为false，默认为：上级/下级 
        /// true表示：下级/上级
        /// </summary>
        /// <param name="pretest"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        private string GetDoubleSignString(string pretest, string currentUser)
        {
            string ret = "";

            if (!_DSS.OverturnSignature) //默认
            {
                ret = currentUser + "/" + pretest;
            }
            else
            {
                ret = pretest + "/" + currentUser;
            }

            return ret;
        }

        /// <summary>
        /// 是否可以签名：
        /// 如果是ukey登录的，那么如果拔掉ukey就不应该能再签名，需要重新插入ukey并重启电子病例。
        /// </summary>
        /// <returns></returns>
        private bool SignEnable()
        {
            bool ret = true;
            _FingerPrintSign = CaProvider.Default.IsStartESignature();
            return ret;
        }

        RTB_InsertImg rtbImg = new RTB_InsertImg();
        /// <summary>
        /// 手膜签名：不是现实名字，而是现实签名图片
        /// 手摸：只显示图片
        /// CA：名字后面，再显示图片
        /// </summary>
        /// <param name="rtbe"></param>
        /// <param name="userId"></param>
        /// <param name="isDoSignOrPrint">是签名调用，还是打印绘制。签名调用的时候，要设计数据设置</param>
        private void FingerPrintSign(RichTextBoxExtended rtbe, string userId, bool isDoSignOrPrint)
        {
            //<CA IsDisplayImage="True" IsDisplaySingleImage="True" IsImageFromDB="False" ImageSize="70,18" IsOnMedicRecordSavedJudge="True">

            // XmlNode xnSql = GlobalVariable.XmlNurseConfig.DocumentElement.SelectSingleNode("//NewRecruit3/Sqls/Item[@Key='" + "表单列表" + "']");
            // XmlNode xnSql = GlobalVariable.XmlNurseConfig.DocumentElement.SelectSingleNode("FingerPrintSign");
            //FingerPrintSign：节点配置
            //<FingerPrintSign IsEnable="True" IsImageFromDB="False" ImageSize="70,20">
            //    <Sqls>
            //      <Item Key="getMeidcSignInfo">
            //        <!--这个需要做一个与his表的视图,查询出图片字段-->
            //        select MEDIC_ID,MEDIC_NAME, SignImage from   MEDIC_BASEView where USERNAME={0}
            //      </Item>
            //    </Sqls>
            //</FingerPrintSign>

            try
            {

                if (_FingerPrintSign)
                {
                    if (string.IsNullOrEmpty(userId))
                    {
                        MessageBox.Show("工号不能为空。");
                        return;
                    }

                    Image tempBitmap = new Bitmap(1, 1);  //rtbe.Width, rtbe.Height


                    //先判断缓存中有没有
                    if (_FingerPrintSignImages != null && _FingerPrintSignImages.ContainsKey(userId))
                    {
                        //tempBitmap = Image.FromFile(@"D:\yqljf\Desktop\ceshi.png");
                        tempBitmap = (Image)_FingerPrintSignImages[userId].Clone();
                    }
                    else
                    {
                        //如果表单数据文件中还没有图片，那么从服务端读取

                        tempBitmap = CaProvider.Default.GetUserImage(userId);
                        //string path = "";

                        //path = GetSignImagePath(userId);

                        //if (File.Exists(path))
                        //{
                        //    //tempBitmap = (Image)Image.FromFile(path).Clone(); //重新生成覆盖会冲突 //new Bitmap(path); 
                        //    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                        //    Image img = System.Drawing.Image.FromStream(fs);
                        //    fs.Close();

                        //    tempBitmap = img;
                        //}
                        //else //存在本地目录的图片
                        //{
                        //    string errMsg = "无法获取手膜签名的图片：" + userId + " ，" + path;
                        //    Comm.LogHelp.WriteErr(errMsg);
                        //    MessageBox.Show(errMsg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        //    return;
                        //}

                    }
                    if (tempBitmap == null) return;


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


                    if (!_FingerPrintSignImages.ContainsKey(userId) && tempBitmap != null)
                    {
                        _FingerPrintSignImages.Add(userId, (Image)tempBitmap.Clone());    //加入本地缓存，防止其他表单也要从服务端获取
                    }


                    int x = rtbe.Width, y = rtbe.Height;

                    //if (tempBitmap.Width <= rtbe.Width && tempBitmap.Height <= rtbe.Height)
                    //{
                    //    x = tempBitmap.Width;
                    //    y = tempBitmap.Height;
                    //}
                    //else
                    //{
                    //    if (tempBitmap.Width / rtbe.Width >= tempBitmap.Height / rtbe.Height) // 图片的宽度比例为最大，高度空白的图片
                    //    {
                    //        x = rtbe.Width;
                    //        y = rtbe.Width * tempBitmap.Height / tempBitmap.Width;
                    //    }
                    //    else
                    //    {
                    //        y = rtbe.Height;
                    //        x = rtbe.Height * tempBitmap.Width / tempBitmap.Height;
                    //    }
                    //}

                    Image bitmapFinger = null; //new Bitmap(rtbe.Width, rtbe.Height);

                    bitmapFinger = new Bitmap(x, y);

                    //防止清空等情况，边界重叠显示
                    x = x - 1;
                    y = y - 1;

                    Graphics.FromImage(bitmapFinger).Clear(Color.White); //不然透明的png图片，会显示不清楚的
                    Graphics g = Graphics.FromImage(bitmapFinger);

                    g.DrawImage(tempBitmap, 1, 1, x, y);    ////防止清空等情况，边界重叠显示

                    g.Dispose();

                    if (isDoSignOrPrint)
                    {
                        rtbe.CreateUser = userId; //手膜签名的时候，这里强行设置签名输入框的用户ID（不然，保存的时候，空值就会不保存用户id，下次无法再现）
                        rtbe.Text = ""; //全角空格，清空文字的名字签名
                        rtbe._IsRtf = true;

                        //保存数据，同一个签名图片只保存一次
                    }

                    rtbe.Select(rtbe.Text.Length, 0); //在开始0位置插入

                    //RTB_InsertImg.InsertImage(rtbe, (Image)bitmapFinger.Clone());
                    //RTB_InsertImg rtbImg = new RTB_InsertImg();
                    rtbImg.InsertImage(rtbe, (Image)bitmapFinger.Clone());

                    rtbe.Select(0, 0); //最后选中光标到开始位置

                    if (string.IsNullOrEmpty(rtbe.Text.Trim())) //没有在图片签名后，继续输入文字的话。如果添加文字的，那么就是不通用的Rtf了，不保存
                    {
                        SaveFingerPrintSignImageRtfToXml(userId, rtbe); //保存到该表单中，以后打开不用获取
                    }
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 调用CA接口，获取图片。
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private string GetSignImagePath(string userId)
        {
            return null;
            //try
            //{
            //    //return HealthCA.CaBLL.GetSignImagePath(userId);  //这个方法可能取不到图片，返回的路进是服务端的路径

            //    //imgPath = FingerPrintSignHelper.GetImagePath(GlobalVariable.UserName);
            //    return FingerPrintSignHelper.GetImagePath(userId);
            //}
            //catch (Exception ex)
            //{
            //    Comm.Logger.WriteErr("调用LibraryWj.FingerPrintSignHelper获取签名图片出现异常：" + ex.StackTrace);
            //    throw ex;
            //}
        }

        /// <summary>
        /// 手膜签名的RTF存储到公用节点下，节省存储数据的容量
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="rtbe"></param>
        private void SaveFingerPrintSignImageRtfToXml(string userId, RichTextBoxExtended rtbe)
        {
            try
            {
                //--
                //<Pages>……</Forms>
                //<FingerPrintSignImages>
                //  <Image ID="工号" Value="图片二进制图数据" />
                //</FingerPrintSignImages>

                XmlNode fingerPrintSignImages = _RecruitXML.SelectSingleNode("//" + EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.FingerPrintSignImages)));
                if (fingerPrintSignImages != null)
                {
                    XmlNode thisNodeImage = fingerPrintSignImages.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), userId, "=", nameof(EntXmlModel.Image)));

                    if (thisNodeImage != null) // && !string.IsNullOrEmpty((thisNodeImages as XmlElement).GetAttribute(nameof(EntXmlModel.Value)))
                    {

                    }
                    else
                    {
                        //插入节点
                        XmlElement image;
                        image = _RecruitXML.CreateElement(nameof(EntXmlModel.Image));
                        image.SetAttribute(nameof(EntXmlModel.ID), userId);
                        fingerPrintSignImages.AppendChild(image);

                        thisNodeImage = fingerPrintSignImages.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), userId, "=", nameof(EntXmlModel.Image)));
                    }

                    //Rtf保存
                    (thisNodeImage as XmlElement).SetAttribute(nameof(EntXmlModel.Value), rtbe.Rtf);
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 判断指定行是否为空白行
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        private bool IsEmptyRow(int rowIndex)
        {
            bool ret = true;

            if (rowIndex <= _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
            {
                for (int i = 0; i < _TableInfor.ColumnsCount; i++)
                {
                    if (!string.IsNullOrEmpty(_TableInfor.Cells[rowIndex, i].Text.Trim()))
                    {
                        ret = false;
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 根据当前行签名，设置整个段落的签名属性控制
        /// </summary>
        /// <param name=nameof(EntXmlModel.UserID)></param>
        /// <param name="UserName"></param>
        /// <param name="User_Degree"></param>
        private void SaveSign(string UserID, string UserName, string User_Degree)
        {


            //--------------当前页表格设置----------------------------
            //先设置当前行
            _TableInfor.Rows[_CurrentCellIndex.X].UserID = UserID;
            _TableInfor.Rows[_CurrentCellIndex.X].UserName = UserName;
            _TableInfor.Rows[_CurrentCellIndex.X].NurseFormLevel = User_Degree;

            SetSignName(UserID, UserName);

            //更新Xml，后台Xml数据节点的属性设置：
            XmlNode records = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));

            XmlNode thisNode = records.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString(), "=", nameof(EntXmlModel.Record)));

            (thisNode as XmlElement).SetAttribute(nameof(EntXmlModel.UserID), UserID + "¤" + UserName + "¤" + User_Degree);

            //上面对当前行的签名处理完毕
            //没有日期或者时间的，就不要整个段落签名了，否则最后一次签名有效了
            if (_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "日期") == null && _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间") == null)
            {
                //ShowInforMsg("不包含【日期、时间】列的表格，无法判断段落行，所以不需要对段落所有行进行签名。");
                //this.Cursor = Cursors.Default;
                return;
            }

            //判断上面的行是否同一段落，是的话，也设定一下
            if (_CurrentCellIndex.X > 0)
            {
                //往上遍历、寻找该行的同一段落，都设置签名UserID=GlobalVariable.LoginUserInfo.UserCode；
                for (int i = _CurrentCellIndex.X; i >= 1; i--)
                {
                    //所属一个段落（日期时间相同）
                    if (!NotSameDateTimeToPreRow(i))
                    {
                        //if (_TableInfor.Rows[i - 1].UserID == "") 这样审核签名，就设置不上了
                        //{
                        _TableInfor.Rows[i - 1].UserID = UserID;
                        _TableInfor.Rows[i - 1].UserName = UserName;       //这里设置当前表格，下面循环再更新数据文件
                        _TableInfor.Rows[i - 1].NurseFormLevel = User_Degree;
                        //}
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //--------------当前页表格设置----------------------------

            string preRowDateTime = GetRowDateTime((thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

            while (thisNode != null)
            {
                thisNode = thisNode.PreviousSibling;    //如果当前节点已经是最前面的节点，那么返回null

                if (thisNode != null && preRowDateTime == GetRowDateTime((thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID))))
                {
                    //if ((thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.UserID)) == "") 这样审核签名，就设置不上了
                    //{
                    (thisNode as XmlElement).SetAttribute(nameof(EntXmlModel.UserID), UserID + "¤" + UserName + "¤" + User_Degree);
                    //}
                }
                else
                {
                    break;
                }
            }

        }

        # endregion 双击单元格，赋默认值

        # region 显示指定的单元格
        Point _PreCellRowIndex = new Point(-1, -1);

        /// <summary>
        /// 根据索引号，显示对应的输入框，其他的输入框全部隐藏
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void ShowCellRTBE(int i, int j)
        {
            //if (_Preview)
            //{
            //    return; //预览的时候不要往下梳理
            //}

            //DrawRowSignShow(); //所在行，显示选中

            _GoOnPrintLocation = new Point(-1, -1);

            if (_TableInfor.Cells[i, j].IsMergedNotShow)
            {
                return; //被合并掉的单元格掉过
            }

            _IsLoading = true;//不然字体会变成默认，因为触发了工具栏字体更新事件

            //提示：所在签名信息
            SetSignName(_TableInfor.Rows[i].UserID, _TableInfor.Rows[i].UserName);


            //如果开启：签名后只读的开关
            if (_DSS.IntensiveCareSignedReadOnly)
            {
                if (_TableInfor.Rows[i].UserID != "")
                {
                    if (GlobalVariable.LoginUserInfo.UserCode != _TableInfor.Rows[i].UserID)
                    {
                        _TableInfor.Columns[j].RTBE.ReadOnly = true;
                    }
                    else
                    {
                        if (_TableInfor.Columns[j].RTBE.Name != "签名")
                        {
                            //_TableInfor.Columns[j].RTBE.ReadOnly = false;
                            _TableInfor.Columns[j].RTBE.ReadOnly = _TableInfor.Columns[j].RTBE.IsReadOnly;
                        }
                    }
                }
                else
                {
                    if (_TableInfor.Columns[j].RTBE.Name != "签名")
                    {
                        //_TableInfor.Columns[j].RTBE.ReadOnly = false;
                        _TableInfor.Columns[j].RTBE.ReadOnly = _TableInfor.Columns[j].RTBE.IsReadOnly;
                    }
                }
            }
            else
            {
                _TableInfor.Columns[j].RTBE.ReadOnly = _TableInfor.Columns[j].RTBE.IsReadOnly || this._IsLocked;
            }

            //调整段落列和小结总结，多行调用此方法的时候，滚动条可能会动啊
            //程序控制连续设值调用本方法的时候，会导致滚动条落到
            panelMain.Focus(); //使滚动条panel获取焦点，可以滚动滚动条。而且调用显示单元格的时候，和点击画布时一样。滚动条不会抖动了。
            //this.pictureBoxBackGround.Focus();

            HorizontalAlignment temp;
            //隐藏所有列的输入框
            for (int col = 0; col < _TableInfor.ColumnsCount; col++)
            {
                if (_TableInfor.Columns[col].RTBE.Visible)
                {
                    //如果是纵向合并的单元格，会变成默认的总是居左：20170906目前纵向合并的，如果对齐修改，在本列光标移动，在离开就会变成默认对齐方式
                    if (_TableInfor.Columns[col].RTBE.Is_RowSpanMutiline)
                    {
                        if (_PreCellRowIndex.Y == j && _PreCellRowIndex.X != -1 && _TableInfor.Columns[j].RTBE.Multiline && _TableInfor.Cells[_PreCellRowIndex.X, j].ColSpan == 0) //上一个单元格
                        {
                            temp = _TableInfor.Columns[j].RTBE.SelectionAlignment;

                            _TableInfor.Columns[j].RTBE.SelectAll();
                            _TableInfor.Columns[j].RTBE.SelectionAlignment = temp;
                        }
                    }

                    //无意中测试发现(时间列有问题，光标移进移出后变成rtf富文本了)，如果是居中的单元格，在合并纵向合并后，直接点击。画布，对齐没有问题，如果点击其他单元格，就会出问题了。
                    //_TableInfor.Columns[col].RTBE.SelectionAlignment = _TableInfor.Columns[col].RTBE._InputBoxAlignment;

                    _TableInfor.Columns[col].RTBE.Visible = false; //隐藏，触发更新
                }
            }

            _CurrentCellIndex = new Point(i, j);

            _TableInfor.Columns[j].RTBE.Location = _TableInfor.Cells[i, j].RtbeLocation();
            _TableInfor.Columns[j].RTBE.CreateUser = _TableInfor.Cells[i, j].CreateUser;
            _TableInfor.Columns[j].RTBE._EditHistory = _TableInfor.Cells[i, j].EditHistory;

            _TableInfor.Columns[j].RTBE.SetAlignment(); // 可能赋值为Text，所以要设置默认的对齐方式

            _TableInfor.Columns[j].RTBE._IsSetValue = true;


            //如果含有时间列，当前行没有行文字颜色，那么继承上一行的行文字颜色
            //有些护理表单，不会文字换行，但是会有一个段落多行，只填写第一行日期时间，颜色要一样
            SetRowForeColor();

            //前面设置
            _TableInfor.Columns[j].RTBE.Multiline = _TableInfor.Columns[j].RTBE.MultilineCell; //单行的单元格，在合并单元格后，首次打开界面，是多行的。

            //大小设定，可能是合并单元格的
            if (_TableInfor.Cells[i, j].IsMerged())
            {
                if (!_TableInfor.Columns[j].RTBE.Multiline && _TableInfor.Cells[i, j].ColSpan == 0) //ColSpan不为0就表示横向合并的，为0那就是纵向合并的了
                {
                    //纵向合并的
                    _TableInfor.Columns[j].RTBE.Multiline = true;
                    _TableInfor.Columns[j].RTBE.Is_RowSpanMutiline = true;

                    //无意中测试发现(时间列有问题，光标移进移出后变成rtf富文本了)，如果是居中的单元格，在合并纵向合并后，直接点击。画布，对齐没有问题，如果点击其他单元格，就会出问题了。
                    //_TableInfor.Columns[j].RTBE.SelectionAlignment = _TableInfor.Columns[j].RTBE._InputBoxAlignment;
                }
                //else if(_TableInfor.Cells[i, j].ColSpan != 0)
                //{
                //    _TableInfor.Columns[j].RTBE.Multiline = _TableInfor.Columns[j].RTBE.MultilineCell; //单行的单元格，在合并单元格后，首次打开界面，是多行的。
                //}

                //大小设置：
                if (_TableInfor.Cells[i, j].ColSpan != 0)
                {
                    //横向合并
                    _TableInfor.Columns[j].RTBE.Size =
                        new Size(_TableInfor.Cells[i, j].Rect.Size.Width - _TableInfor.Cells[i, j].DiffLoaction.X * 2,
                                _TableInfor.Columns[j].RTBE._DefaultSize.Height);
                }
                else
                {
                    //纵向合并的
                    _TableInfor.Columns[j].RTBE.Size =
                        new Size(_TableInfor.Columns[j].RTBE._DefaultSize.Width,
                                _TableInfor.Cells[i, j].Rect.Size.Height - _TableInfor.Cells[i, j].DiffLoaction.Y * 2);

                    //_TableInfor.Columns[j].RTBE.Refresh();
                    //_TableInfor.Columns[j].RTBE.Focus();
                }

                //已经合并的单元格，不做格式验证，也不做Maxlength限制
                _TableInfor.Columns[j].RTBE.MaxLength = 99999;
            }
            else
            {
                //_TableInfor.Columns[j].RTBE.Multiline = _TableInfor.Columns[j].RTBE.MultilineCell; //单行的单元格，在合并单元格后，首次打开界面，是多行的。

                if (!_TableInfor.Columns[j].RTBE._DefaultSize.Equals(_TableInfor.Columns[j].RTBE.Size))
                {
                    _TableInfor.Columns[j].RTBE.ResetSize();        //恢复单元格输入框的默认大小
                    _TableInfor.Columns[j].RTBE.Multiline = false;
                }

                _TableInfor.Columns[j].RTBE.MaxLength = _TableInfor.Columns[j].RTBE._MaxLength;
            }

            //单元格是多行的，肯定还是多行
            if (_TableInfor.Columns[j].RTBE.MultilineCell)
            {
                if (!_TableInfor.Columns[j].RTBE.Multiline)
                {
                    _TableInfor.Columns[j].RTBE.Multiline = true; //不管合并还是怎么操作，永远都是多行的了。处理处：加载表格，输入框按下Enter，显示单元格
                }
            }

            //处理：设置自动显示默认值，并且不是手膜签名开启下的签名单元格的时候（签名的可能为True）
            if (_TableInfor.Cells[i, j].Text.Trim() == ""
                && !(_TableInfor.Cells[i, j].Rtf.IndexOf(@"{\pict\") > -1) // 不含有图片，因为如果只有图片，Text为半角空格，Trim后为空了。
                && !(_FingerPrintSign && (_TableInfor.Cells[i, j].CellName.StartsWith("签名") || _TableInfor.Cells[i, j].CellName.StartsWith("记录者")) && !string.IsNullOrEmpty(_TableInfor.Cells[i, j].CreateUser))) //如果这里处理的话，那么会导致光标离开的时候，赋值后OldRtf和现在的一样
            {
                //Cells_EnterSetDefaulyValue(_TableInfor.Cells[i, j]);  //设置默认值，比如时间和工号

                //如果是纵向合并的单元格，会变成默认的总是居左
                if (_TableInfor.Columns[j].RTBE.Is_RowSpanMutiline)
                {
                    temp = _TableInfor.Columns[j].RTBE.SelectionAlignment;
                    _TableInfor.Columns[j].RTBE.Text = _TableInfor.Cells[i, j].Text;
                    //_TableInfor.Columns[j].RTBE.OldRtf = ""; //不然根据时间，行的文字变色的时候，有可能会事件干扰导致OldRtf和当前的默认值一样
                    _TableInfor.Columns[j].RTBE.SelectionAlignment = temp;
                }

                //if (_TableInfor.Columns[j].RTBE.Name == "日期")
                if (_TableInfor.Columns[j].RTBE.Name.StartsWith("日期"))
                {
                    _TableInfor.Columns[j].RTBE._YMD = _TableInfor.Columns[j].RTBE._YMD_Default;
                    _TableInfor.Columns[j].RTBE._FullDateText = "";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_TableInfor.Cells[i, j].Rtf))
                {
                    _TableInfor.Columns[j].RTBE.Rtf = _TableInfor.Cells[i, j].Rtf; //可能是不满整个的年月日
                }
                else
                {
                    //如果上一行，存在上下标等字体，会导致字体变小，产生异常。
                    //u静点,遵医嘱留置胃管，【上下标】的文字是第一个文字，就会出现，这种情况
                    _TableInfor.Columns[j].RTBE.ResetDefault(); //历史也会清空
                    _TableInfor.Columns[j].RTBE.SelectionFont = _TableInfor.Columns[j].RTBE.Font;
                    //如果上一行，存在上下标等字体，会导致字体变小，产生异常。

                    _TableInfor.Columns[j].RTBE.CreateUser = _TableInfor.Cells[i, j].CreateUser;    //重新补上
                    _TableInfor.Columns[j].RTBE._EditHistory = _TableInfor.Cells[i, j].EditHistory; //重新补上，上面一行ResetDefault修改一个问题导致的

                    temp = _TableInfor.Columns[j].RTBE.SelectionAlignment;
                    _TableInfor.Columns[j].RTBE.Text = _TableInfor.Cells[i, j].Text;

                    _TableInfor.Columns[j].RTBE.SelectAll();    //要全选，否则，纵向合并的单元格，光标进入再离开防止，就变成居左了
                    _TableInfor.Columns[j].RTBE.SelectionAlignment = temp;
                }

                //手膜签名：公共的rtf数据设置过来
                if (_TableInfor.Cells[i, j].IsRtf_OR_Text && string.IsNullOrEmpty(_TableInfor.Cells[i, j].Rtf) && _FingerPrintSign && (_TableInfor.Columns[j].RTBE.Name.StartsWith("签名") || _TableInfor.Columns[j].RTBE.Name.StartsWith("记录者"))
                     && !string.IsNullOrEmpty(_TableInfor.Cells[i, j].CreateUser))
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
                    XmlNode imgRtfNode = fingerPrintSignImages.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Cells[i, j].CreateUser, "=", nameof(EntXmlModel.Image)));
                    if (imgRtfNode != null)
                    {
                        _TableInfor.Columns[j].RTBE.Rtf = (imgRtfNode as XmlElement).GetAttribute(nameof(EntXmlModel.Value));
                    }
                }

                //if (_TableInfor.Columns[j].RTBE.Name == "日期")
                if (_TableInfor.Columns[j].RTBE.Name.StartsWith("日期"))
                {
                    if (_TableInfor.Cells[i, j].Text != "")
                    {
                        _TableInfor.Columns[j].RTBE._YMD = _TableInfor.Cells[i, j].DateFormat;
                    }
                    else
                    {
                        _TableInfor.Columns[j].RTBE._YMD = _TableInfor.Columns[j].RTBE._YMD_Default;
                    }

                    _TableInfor.Columns[j].RTBE._FullDateText = _TableInfor.Cells[i, j].Text;

                    string showText = _TableInfor.Columns[j].RTBE.GetShowDate();

                    if (showText != _TableInfor.Columns[j].RTBE.Text)
                    {
                        temp = _TableInfor.Columns[j].RTBE.SelectionAlignment;
                        _TableInfor.Columns[j].RTBE.Text = showText;
                        _TableInfor.Columns[j].RTBE.SelectionAlignment = temp;
                    }
                }
            }

            _TableInfor.Columns[j].RTBE._IsRtf = _TableInfor.Cells[i, j].IsRtf_OR_Text;

            _TableInfor.Columns[j].RTBE._HaveSingleLine = _TableInfor.Cells[i, j].SingleLine;
            _TableInfor.Columns[j].RTBE._HaveDoubleLine = _TableInfor.Cells[i, j].DoubleLine;

            _TableInfor.Columns[j].RTBE.SelectAll();


            _TableInfor.CurrentCell = _TableInfor.Cells[i, j]; //保留下当前单元格，方便之后需要处理

            ReSetCellBackGround(_TableInfor.Columns[j].RTBE); //清空输入框下，之前绘制的东西，重置为背景颜色，等于没有了。不然透明的会重叠

            //DrawRowSignShow(true); //所在行，显示选中。 最后绘制的话，会导致合并后的单元格的文字被虚线挡住。但是这里话不会显示

            //根据时间自动进行行的文字颜色改变
            //整行文字颜色自动修改
            if (!_TableInfor.Cells[i, j].IsRtf_OR_Text)
            {
                if (!string.IsNullOrEmpty(_TableInfor.Rows[i].RowForeColor)) // !_TableInfor.Cells[i, j].IsRtf_OR_Text
                {
                    _TableInfor.Columns[j].RTBE.SelectAll();
                    _TableInfor.Columns[j].RTBE.ForeColor = Color.FromName(_TableInfor.Rows[i].RowForeColor);
                    _TableInfor.Columns[j].RTBE.SelectionColor = Color.FromName(_TableInfor.Rows[i].RowForeColor);
                }
                else
                {
                    _TableInfor.Columns[j].RTBE.SelectAll();
                    _TableInfor.Columns[j].RTBE.SelectionColor = _TableInfor.Columns[j].RTBE.DefaultForeColor;
                    _TableInfor.Columns[j].RTBE.ForeColor = _TableInfor.Columns[j].RTBE.DefaultForeColor;
                }
            }

            #region PreEnter 脚本
            if (_node_Script != null && _node_Script.ChildNodes.Count > 0)  //if (_node_Script != null && _node_Script.ChildNodes.Count > 0)  发现下面报null异常
            {
                //RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;
                RichTextBoxExtended rtbe = _TableInfor.Columns[j].RTBE;

                //if (string.IsNullOrEmpty(rtbe.Text.Trim())) //只有空才能执行
                //{
                foreach (XmlNode xn in _node_Script.ChildNodes)
                {
                    //如果是注释那么跳过
                    if (xn.Name == @"#comment" || xn.Name == @"#text")
                    {
                        continue;
                    }

                    //找到当前控件，对应的脚本，多个事件合并共用
                    //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == ctl.Name && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)) == "DoubleClick")
                    if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == rtbe.Name && ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains("PreEnter")) //DoubleClick
                    {
                        bool isTableScript = false;
                        if (!bool.TryParse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.IsTable)), out isTableScript))
                        {
                            isTableScript = false;
                        }

                        if (rtbe._IsTable && isTableScript)    //表格脚本执行
                        {
                            if (_TableType && _TableInfor != null)
                            {
                                //表格选中了某行，存在处理对象当前行
                                if (_CurrentCellIndex.X != -1 && _TableInfor.CurrentCell != null)
                                {
                                    this.Cursor = Cursors.WaitCursor;

                                    //,如果一个段落的话前面是签在最后一行。只能根据_TableInfor.Rows[i].UserID 来判断是否签名
                                    if (!Evaluator.EvaluateRunString(_TableInfor, ScriptPara(xn.InnerText.Trim())))
                                    {
                                        this.Cursor = Cursors.Default;
                                        Redraw(_TableInfor.Columns[j].RTBE);
                                        return;
                                    }

                                    this.Cursor = Cursors.Default;
                                }
                            }
                        }
                    }
                }
                //}
            }
            #endregion PreEnter

            _TableInfor.Columns[j].RTBE.Visible = true;
            _TableInfor.Columns[j].RTBE.Focus();


            //等于空，就会被重置格式
            if (_TableInfor.Cells[i, j].Text.Trim() == ""
                && !(_TableInfor.Cells[i, j].Rtf.IndexOf(@"{\pict\") > -1) // 不含有图片，因为如果只有图片，Text为半角空格，Trim后为空了。
                && !(_FingerPrintSign && (_TableInfor.Cells[i, j].CellName.StartsWith("签名") || _TableInfor.Cells[i, j].CellName.StartsWith("记录者")) && !string.IsNullOrEmpty(_TableInfor.Cells[i, j].CreateUser))) //
            {
                if (!Comm._SettingRowForeColor) //根据时间显示字颜色的时候，不需要设置默认值。会触发的干扰
                {
                    if (_DSS.DefaultDateEnterOrDoubleClick)
                    {
                        Cells_EnterSetDefaulyValue(_TableInfor.Cells[i, j]);  //设置默认值，比如时间和工号
                    }
                }

                temp = _TableInfor.Columns[j].RTBE.SelectionAlignment;

                //_TableInfor.Columns[j].RTBE._IsSetValue = false; //不然值设置不上
                _TableInfor.Columns[j].RTBE.Text = _TableInfor.Cells[i, j].Text;

                //设置默认值时间后，行文字颜色也要变。
                if (_TableInfor.Cells[i, j].CellName == "时间" && !string.IsNullOrEmpty(_TableInfor.Cells[i, j].Text.Trim()))
                {

                    SetCurrentRowForeColor(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE); //设置行文字颜色，并刷新界面

                    ReSetCellBackGround(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE);    //不然，之前可能会有重影
                }

                if (_TableInfor.Cells[i, j].CellName.StartsWith("日期"))
                {
                    _TableInfor.Columns[j].RTBE._FullDateText = _TableInfor.Cells[i, j].Text; //全日期的值也赋值上
                }

                //_TableInfor.Columns[j].RTBE._IsSetValue = true;

                //因为上面已经置入光标了，修改text后，不会再出发更新oldrtf了，所以这里将oldrtf设置为空，表示是text为空的时候进行设置了默认值
                _TableInfor.Columns[j].RTBE.OldRtf = ""; //不然根据时间，行的文字变色的时候，有可能会事件干扰导致OldRtf和当前的默认值一样
                _TableInfor.Columns[j].RTBE.OldText = "";
                _TableInfor.Columns[j].RTBE.SelectionAlignment = temp;

                if (!string.IsNullOrEmpty(_TableInfor.Rows[i].RowForeColor)) // !_TableInfor.Cells[i, j].IsRtf_OR_Text
                {
                    _TableInfor.Columns[j].RTBE.SelectAll();
                    _TableInfor.Columns[j].RTBE.ForeColor = Color.FromName(_TableInfor.Rows[i].RowForeColor);
                    _TableInfor.Columns[j].RTBE.SelectionColor = Color.FromName(_TableInfor.Rows[i].RowForeColor);
                }
                else
                {
                    _TableInfor.Columns[j].RTBE.SelectAll();
                    _TableInfor.Columns[j].RTBE.SelectionColor = _TableInfor.Columns[j].RTBE.DefaultForeColor;
                    _TableInfor.Columns[j].RTBE.ForeColor = _TableInfor.Columns[j].RTBE.DefaultForeColor;
                }

                _TableInfor.Columns[j].RTBE._IsRtf = false; //节省容量，变成普通文本
            }

            //进行合计:表格的合计在showcell中做
            if (!Comm._SettingRowForeColor && _TableInfor.Columns[j].RTBE.Text == "")
            {
                Sum("");

                //---以上是进行合计：

                _TableInfor.Columns[j].RTBE.OldText = "";
                _TableInfor.Columns[j].RTBE._IsRtf = false; //节省容量，变成普通文本
            }

            _PreCellRowIndex = new Point(i, j);

            _TableInfor.Columns[j].RTBE._IsSetValue = false;

            _IsLoading = false;

            DrawRowSignShow(true); //所在行，显示选中。最后绘制的话，会导致合并后的单元格的文字被虚线挡住




            #region 双击事件/Enter事件的脚本验证 可以执行表格脚本：判断赋值
            //光标进入脚本：尤其是表格的时候，用来判断当前行的数据，形成特殊的合计列的汇总。利用评分和合计都是无法完成的，因为里面含有数字，而且是一个范围判断。
            //预警评分
            //条件判断值
            //                分 2分 1分 0分 1分 2分 3分
            //舒张压（mmHg）  <70 71-80 81-100 101-199 >=200 
            //心率（次/分）   <40 41-50 51-100 101-110 111-129 >=130
            //呼吸频率(次/分) <9 9-14 15-20 21-29 >=30
            //体温（C）       <35 35-38.4 >=38.5 
            //AVPU评分 
            //MEWS合计总分

            //1、 舒张压是百分比输入：举例90/150只用分子数值做判断
            //2、 AVPU评分文本框输入，护士自己手动输入分值
            //3、 空白部分不需要考虑，根据已有条件判断及可

            //TODO:用脚本来实现，判断分支过多，导致脚本很多，性能很慢。所以还是通过扩展合计来做。
            //∫ 表示有评分范围，就优先取评分范围对应的值，没有范围就去当前文字中的第一段数字，文字也没有数字，就认为是0，或者跳过这个被合计项。（因为原来文字内就会有文字）
            //评分进行扩展用全角的～符号分割最小值和最大值进行解析，满足范围的返回对应值。原来是匹配一直，返回@后面设定的值。
            //基本的评分配置：Score="√@3"
            //扩展后的评分配置：Score="38～38.4@2¤38.5～@3"

            if (_node_Script != null && _node_Script.ChildNodes.Count > 0 && _Need_Assistant_Control != null)  //if (_node_Script != null && _node_Script.ChildNodes.Count > 0)发现下面报null异常
            {
                //RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;
                RichTextBoxExtended rtbe = ((RichTextBoxExtended)_Need_Assistant_Control);

                //if (string.IsNullOrEmpty(rtbe.Text.Trim()))
                //{
                foreach (XmlNode xn in _node_Script.ChildNodes)
                {
                    //如果是注释那么跳过
                    if (xn.Name == @"#comment" || xn.Name == @"#text")
                    {
                        continue;
                    }

                    //找到当前控件，对应的脚本，多个事件合并共用
                    //if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == ctl.Name && (xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)) == "DoubleClick")
                    if ((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Name)) == rtbe.Name && ArrayList.Adapter((xn as XmlElement).GetAttribute(nameof(EntXmlModel.Event)).Split(',')).Contains("Enter")) //DoubleClick
                    {
                        bool isTableScript = false;
                        if (!bool.TryParse((xn as XmlElement).GetAttribute(nameof(EntXmlModel.IsTable)), out isTableScript))
                        {
                            isTableScript = false;
                        }

                        ////非表格的双击，执行这里
                        //if (!rtbe._IsTable && !isTableScript)
                        //{
                        //    if (!Evaluator.EvaluateRunString(this, ScriptPara(xn.InnerText.Trim())))
                        //    {
                        //        return;
                        //    }
                        //}
                        //else 
                        if (rtbe._IsTable && isTableScript)    //表格脚本执行
                        {
                            if (_TableType && _TableInfor != null)
                            {
                                //表格选中了某行，存在处理对象当前行
                                if (_CurrentCellIndex.X != -1 && _TableInfor.CurrentCell != null)
                                {
                                    this.Cursor = Cursors.WaitCursor;

                                    //签名.Text != '' Then Error(已经签名了哦)
                                    //日期.Text != '' Then 签名.Text = ''     

                                    //但是如果签了一次，再签的话；需要光标离开才能更新当前单元格啊,要触发一次保存
                                    Cells_LeaveSaveData(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE, null); //触发光标离开事件，进行保存 数据

                                    if (!Evaluator.EvaluateRunString(_TableInfor, ScriptPara(xn.InnerText.Trim())))
                                    {
                                        this.Cursor = Cursors.Default;

                                        return;
                                    }

                                    this.Cursor = Cursors.Default;
                                }
                            }
                        }
                    }
                }
                //}
            }
            #endregion 双击事件的脚本验证


        }
        # endregion 显示指定的单元格

        # region 单元格自动换行、自动调整行高，并使下面的控件保持相同间距
        private int _Space = 0;
        private Point _SpaceTarget = new Point(-1, -1);
        Point _OriginPointBack;
        private bool _IsSpaceChart = false;

        /// <summary>
        /// 自动调整行高，并使下面的控件保持相同间距
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RTBE_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;
            int newHeight = e.NewRectangle.Height + rtbe.Margin.Top + rtbe.Margin.Bottom; //调试发现还有2的误差，应该是上下的边界导致的
            int diffHeitht = newHeight - rtbe.Height;

            if (diffHeitht <= 0 && newHeight <= rtbe._DefaultSize.Height) //if (diffHeitht <= 0 && rtb.Text == "")
            {
                diffHeitht = rtbe._DefaultSize.Height - rtbe.Height; //重新设置偏差高度
                rtbe.Height = rtbe._DefaultSize.Height;
            }
            else
            {
                rtbe.Height = newHeight;
            }

            if (_IsLoading)
            {
                return; //载入的时候，一次性刷新，不要每次都刷新；而且有多个触发，也不能控制
            }

            //如果不是单元格，那么下面的控件要保持相同间距
            if (!rtbe._IsTable)
            {
                //移动下面控件，保持相对的固定间距
                foreach (Control cl in this.pictureBoxBackGround.Controls)
                {
                    if (cl == rtbe)
                    {
                        //当前控件本身，不做处理，跳过
                    }
                    else
                    {
                        if (cl.Location.Y > rtbe.Location.Y)
                        {
                            cl.Location = new Point(cl.Location.X, cl.Location.Y + diffHeitht);
                        }
                    }
                }

                //按照间距，重绘背景：线、固定文字等底板；如果有2个及以上这样的情况，现在还没法处理。。。。
                //1.设置重绘变量
                _SpaceTarget = rtbe.Location;
                _Space = rtbe.Height - rtbe._DefaultSize.Height;

                //2.重新绘制底板
                _BmpBack = new Bitmap(pictureBoxBackGround.Width, pictureBoxBackGround.Height);
                Graphics.FromImage(_BmpBack).Clear(Color.White);
                pictureBoxBackGround.Image = (Bitmap)_BmpBack.Clone();

                _OriginPointBack = _OriginPoint;

                _OriginPoint = new Point(_OriginPoint.X, _OriginPoint.Y + _Space);

                DrawEditorChart(Graphics.FromImage(this.pictureBoxBackGround.Image), true, false, false);
                pictureBoxBackGround.Refresh();

                _OriginPoint = _OriginPointBack;
            }
        }
        # endregion 单元格自动换行、自动输入框高度

        # region 表格基础类和方法
        /// <summary>
        /// 复制单元格：将cell2复制到cell1
        /// </summary>
        /// <param name="cell1">目标单元格</param>
        /// <param name="cell2">源单元格</param>
        private void CopyCell(CellInfor cell1, CellInfor cell2)
        {
            cell1.CellName = cell2.CellName;
            cell1.CellSize = cell2.CellSize;

            cell1.Rect = new Rectangle(cell1.Loaction, cell1.CellSize);

            cell1.DiffLoaction = cell2.DiffLoaction;

            cell1.Text = cell2.Text;
            cell1.Rtf = cell2.Rtf;
            cell1.IsRtf_OR_Text = cell2.IsRtf_OR_Text;

            //双红线
            cell1.SingleLine = cell2.SingleLine;
            cell1.DoubleLine = cell2.DoubleLine;

            cell1.DiagonalLineZ = cell2.DiagonalLineZ; //对角线：/   Z、N的中间那一笔，正好是象形的表示了线的走向
            cell1.DiagonalLineN = cell2.DiagonalLineN; //对角线：\   如果是合并单元格，还需要根据RowSpan、ColSpan来计算起点位置

            cell1.NotTopmLine = cell2.NotTopmLine; //如果设置成没有，那么重绘上画布的背景颜色，那就没有了

            //合并单元格
            cell1.RowSpan = cell2.RowSpan;
            cell1.ColSpan = cell2.ColSpan;

            //被合并的单元格
            cell1.IsMergedNotShow = cell2.IsMergedNotShow;
            cell1.MergedRight = cell2.MergedRight;
            cell1.MergedCell = cell2.MergedCell;
            cell1.MergedCount = cell2.MergedCount;

            cell1.EditHistory = cell2.EditHistory;

            cell1.DateFormat = cell2.DateFormat;

            //四条边框线颜色
            cell1.CellLineColor = cell2.CellLineColor;
        }

        /// <summary>
        /// 复制行
        /// </summary>
        /// <param name="row1">目标行</param>
        /// <param name="row2">对象行</param>
        private void CopyRow(RowInfor row1, RowInfor row2)
        {
            row1.RowForeColor = row2.RowForeColor;
            row1.RowLineColor = row2.RowLineColor;
            row1.CustomLineType = row2.CustomLineType;
            row1.CustomLineColor = row2.CustomLineColor;
            row1.ID = row2.ID;
            row1.UserID = row2.UserID;
            row1.UserName = row2.UserName;
            row1.NurseFormLevel = row2.NurseFormLevel;
            row1.SumType = row2.SumType;

            //row1.IsSumRow = row2.IsSumRow;
        }

        /// <summary>
        /// 重置行
        /// </summary>
        /// <param name="row1">对象行</param>
        private void RestRow(RowInfor row1)
        {
            row1.RowForeColor = "";
            row1.RowLineColor = Color.Black; ;
            row1.CustomLineType = LineTypes.None;
            row1.CustomLineColor = Color.Red;
            row1.ID = -1;
            row1.UserID = "";
            row1.UserName = "";
            row1.NurseFormLevel = "";
            row1.SumType = "";
            //row1.IsSumRow = false;
        }

        private void RestCell(TableInfor tableInforPara, CellInfor cell1)
        {

            cell1.Text = "";
            cell1.Rtf = "";

            cell1.EditHistory = "";

            if (cell1.IsMerged())
            {
                //合并单元格，重置大小
                cell1.CellSize = cell1.CellSizeOrg;
            }

            //合并单元格
            cell1.RowSpan = 0;//向后合并单元个数  IsMerged()来判断，本单元格是否是合并单元格
            cell1.ColSpan = 0;//向下合并单元格数

            cell1.IsMergedNotShow = false; //是不是被合并的单元，如果是被合并的，那就不用显示。
            cell1.MergedRight = true;
            //cell1.MergedCell = null;  //被合并的单元格位置
            cell1.MergedCount = 0; //被合并的数

            //位置大小
            cell1.Rect = new Rectangle(cell1.Loaction, cell1.CellSize);

            //单元格（输入框）属性：
            cell1.IsRtf_OR_Text = false; //非Rtf的纯文字

            cell1.SingleLine = false;
            cell1.DoubleLine = false; //文字下面的红线

            cell1.DiagonalLineZ = false; //对角线：/   Z、N的中间那一笔，正好是象形的表示了线的走向
            cell1.DiagonalLineN = false; //对角线：\   如果是合并单元格，还需要根据RowSpan、ColSpan来计算起点位置

            cell1.NotTopmLine = false; //如果设置成没有，那么重绘上画布的背景颜色，那就没有了

            //cell的对象有可能是_TableInfor，也有可能是打印所有页的对象，或者生成区域图片的
            cell1.DateFormat = tableInforPara.Columns[cell1.ColIndex].RTBE._YMD_Default; //cell1.DateFormat = _TableInfor.Columns[cell1.ColIndex].RTBE._YMD_Default;

            //四条边框线颜色
            cell1.CellLineColor = Color.Black;
        }

        /// <summary>
        /// 根据双红线名字，得到线类型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private LineTypes GetLineTypeFromName(string name)
        {
            LineTypes ret = LineTypes.None;

            switch (name)
            {
                case "None":
                    ret = LineTypes.None;
                    break;

                case "RectLine":
                    ret = LineTypes.RectLine;
                    break;
                case "RectDoubleLine":
                    ret = LineTypes.RectDoubleLine;
                    break;

                case "BottomLine":
                    ret = LineTypes.BottomLine;
                    break;
                case "BottomDoubleLine":
                    ret = LineTypes.BottomDoubleLine;
                    break;

                case "TopLine":
                    ret = LineTypes.TopLine;
                    break;
                case "TopDoubleLine":
                    ret = LineTypes.TopDoubleLine;
                    break;

                case "TopBottomLine":
                    ret = LineTypes.TopBottomLine;
                    break;
                case "TopBottomDoubleLine":
                    ret = LineTypes.TopBottomDoubleLine;
                    break;
            }

            return ret;
        }

        # endregion 表格基础类和方法

        # region 加载表格
        /// <summary>
        /// 加载表格数据
        /// </summary>
        /// <param name="tableInforPara"></param>
        /// <param name="isPrintAll"></param>
        private void LoadTable(TableInfor tableInforPara, bool isPrintAll)
        {
            //2.表格部分 解析xml后，对_TableInfor进行赋值设置
            if (_TableType && tableInforPara != null) // 根据解析本页的数据，形成本页的表格，进行绘制。这里还是默认的表格线。
            {
                //表格部分
                XmlNode recordsNode = _RecruitXML.SelectSingleNode("//" + EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));

                if (recordsNode != null)
                {
                    # region 唯一号索引号
                    //先设置好当前最大索引号，每次保存的时候，也要将最大索引号保存
                    //这里的索引号，按照最大值获取，不能按照行数获取
                    //插入行时，最大值可能时19，但是实际行数小于19，第一次加载的时候，MaxId应该=id的最大值
                    int maxId = 0;
                    if (recordsNode.ChildNodes.Count > 0)
                    {
                        maxId = recordsNode.ChildNodes.OfType<XmlNode>().Max(a => a.Attributes[nameof(EntXmlModel.ID)] != null ? a.Attributes[nameof(EntXmlModel.ID)].Value.ToInt() : 0);
                    }
                    //if (!string.IsNullOrEmpty((recordsNode as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID))))
                    //{
                    //    maxId = int.Parse((recordsNode as XmlElement).GetAttribute(nameof(EntXmlModel.MaxID)));
                    //}
                    //else
                    //{
                    //    maxId = 0;
                    //}

                    tableInforPara.MaxId = maxId;

                    # endregion 唯一号索引号

                    //TODO：注释也认为是子节点，会影响分页，如果存在这样的问题，可以先遍历删除 .Name == @"#comment"

                    XmlNode recordNode;

                    //int pageRowsFristPage = TableClass.GetPageRowsFristPage(_TemplateXml); //其实打开每一页后，已经用不到第一页行数。只有在计算行数的时候用到：单机版，生成所有页图片接口，移动接口
                    //int pageRowsSecondPage = TableClass.GetPageRowsSecondPage(_TemplateXml);

                    //根据当前页数，取得数据（xml中的数据已经是顺序的了）
                    int pageRows = 0;
                    int startIndex = 0;

                    pageRows = tableInforPara.RowsCount * tableInforPara.GroupColumnNum; //当前页行数

                    //if (pageRowsFristPage == pageRowsSecondPage)
                    if (tableInforPara.RowsCountFirstPage == tableInforPara.RowsCountSecondPage)
                    {
                        //表格的每页的行数一样的时候
                        startIndex = pageRows * (int.Parse(_CurrentPage) - 1);

                        if (isPrintAll)
                        {
                            startIndex = pageRows * (printAll_PageIndex - 1);
                        }
                    }
                    else
                    {
                        //表格每页行数不一样的时候
                        int page = int.Parse(_CurrentPage);

                        if (isPrintAll)
                        {
                            page = printAll_PageIndex;
                        }

                        if (page == 1)
                        {
                            startIndex = 0;
                        }
                        else
                        {
                            startIndex = tableInforPara.RowsCountFirstPage * tableInforPara.GroupColumnNum +
                                (page - 2) * tableInforPara.RowsCountSecondPage * tableInforPara.GroupColumnNum;
                        }

                        //startIndex = pageRows * (int.Parse(_CurrentPage) - 1);

                        //if (isPrintAll)
                        //{
                        //    startIndex = pageRows * (printAll_PageIndex - 1);
                        //}
                    }

                    #region 全部空白行保存后，再打开显示时，行往前移的的问题
                    // (已接收) 表单内的表格数据会往前错行
                    //如果前一页下面有空白行，后面一页的表格全部空行保存后（保存时，删除了底端的所有空白行）
                    //那么，直接打开最后一页的填写行后，保存再打开就显示到前面页去了。
                    if (startIndex > recordsNode.ChildNodes.Count)
                    {
                        int all = startIndex - recordsNode.ChildNodes.Count;
                        XmlNode recordNodeNew;

                        for (int i = 0; i < all; i++)
                        {
                            recordNodeNew = TemplateHelp.AddXmlRecordNode(_RecruitXML);
                            int temp = _TableInfor.AddRowGetMaxID();
                            (recordNodeNew as XmlElement).SetAttribute(nameof(EntXmlModel.ID), temp.ToString());

                            //XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                            (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), temp.ToString());
                        }

                        Comm.LogHelp.WriteInformation("添加了特殊情况下，需要补加的空白行。");
                    }
                    #endregion 全部空白行保存后，再打开显示时，行往前移的的问题

                    int thisPageRowIndex = 0;

                    for (int i = startIndex; i < startIndex + pageRows; i++)
                    {
                        if (i < recordsNode.ChildNodes.Count)
                        {
                            recordNode = recordsNode.ChildNodes[i];

                            //将xml数据显示到表格中
                            XmlNode2Row(tableInforPara, thisPageRowIndex, recordNode);

                        }
                        else
                        {
                            //添加本页的末尾的没有数据行的空白行
                            //xml中添加空白节点，如果保存的时候，还是空白的会把最后的空白行删除掉的
                            //xml节点的位置（最末尾添加即可）、 id号设定 -> 并与tableInforPara中的设置绑定
                            if (!isPrintAll)
                            {
                                AddNewRecordNodeForRow(thisPageRowIndex);
                            }
                        }

                        thisPageRowIndex++;
                    }
                }  //存在Records节点
            }      //该表单是表格样式
        }
        # endregion 加载表格

        # region 解析数据节点，设置表格的行
        /// <summary>
        /// 解析xml中的节点数据，设置到表格中，准备显示表格数据
        /// </summary>
        /// <param name="thisPageRowIndex"></param>
        /// <param name="recordNode"></param>
        private void XmlNode2Row(TableInfor tableInforPara, int thisPageRowIndex, XmlNode recordNode)
        {
            CellInfor thisCell;
            CellInfor mergedCell;
            string[] arrRtbeValue;
            bool tempBool = false;
            string[] arrTemp;

            if ((recordNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)) == "")
            {
                //对没有id的异常数据进行修补；有ID属性，但是值为空的情况;但是更多的情况是没有这个属性
                string newID = tableInforPara.AddRowGetMaxID().ToString();
                (recordNode as XmlElement).SetAttribute(nameof(EntXmlModel.ID), newID);

                tableInforPara.Rows[thisPageRowIndex].ID = int.Parse(newID);
                IsNeedSave = true;

                Comm.LogHelp.WriteInformation("发现没有ID的异常RecordNode数据，已经进行修补。");
            }

            tableInforPara.Rows[thisPageRowIndex].SumType = ""; //先要置空，否则没有sum属性不会被重置的啊

            //根据节点内容，设置_TableInfor中的Row和这行的Cells
            //遍历节点所有属性：
            foreach (XmlAttribute xe in recordNode.Attributes)
            {
                if (xe.Name == nameof(EntXmlModel.ID))
                {
                    //行数据唯一号
                    tableInforPara.Rows[thisPageRowIndex].ID = int.Parse(xe.Value);
                }
                else if (xe.Name == nameof(EntXmlModel.VALIDATE))
                {
                    //操作时间
                }
                else if (xe.Name == nameof(EntXmlModel.UserID))
                {
                    arrTemp = xe.Value.Split('¤');

                    if (arrTemp.Length > 1)
                    {
                        tableInforPara.Rows[thisPageRowIndex].UserID = arrTemp[0];
                        tableInforPara.Rows[thisPageRowIndex].UserName = arrTemp[1];

                        if (arrTemp.Length > 2)
                        {
                            tableInforPara.Rows[thisPageRowIndex].NurseFormLevel = arrTemp[2];
                        }
                        else
                        {
                            tableInforPara.Rows[thisPageRowIndex].NurseFormLevel = "";
                        }
                    }
                    else
                    {
                        //行段落的签名用户ID
                        tableInforPara.Rows[thisPageRowIndex].UserID = xe.Value;
                        tableInforPara.Rows[thisPageRowIndex].UserName = "";
                        tableInforPara.Rows[thisPageRowIndex].NurseFormLevel = "";
                    }
                }
                else if (xe.Name == nameof(EntXmlModel.Total))
                {
                    //行的合计状态
                    //该行，是合计行还是一般行。统计的时候，要跳过合计行/小计行。
                    if (xe.Value != "")
                    {
                        tableInforPara.Rows[thisPageRowIndex].SumType = xe.Value;
                    }
                    else
                    {
                        tableInforPara.Rows[thisPageRowIndex].SumType = ""; //如果没有sum属性，那么自然就走不进来了。
                    }
                }
                else if (xe.Name == nameof(EntXmlModel.ROW))
                {
                    //行信息

                    arrRtbeValue = xe.Value.Split('¤');

                    if (xe.Name == nameof(EntXmlModel.ROW))
                    {
                        //10. 11. 12行信息：行的线颜色，和双红线种类，行的文字颜色
                        tableInforPara.Rows[thisPageRowIndex].RowLineColor = Color.FromName(arrRtbeValue[0]);           //行线颜色：（行边框线的颜色，非双红线）
                        tableInforPara.Rows[thisPageRowIndex].CustomLineType = GetLineTypeFromName(arrRtbeValue[1]);    //双红线类型
                        tableInforPara.Rows[thisPageRowIndex].RowForeColor = arrRtbeValue[2];                           //行文字颜色（指定时间的红色/蓝色自动切换）

                        //双红线颜色也要定制
                        if (arrRtbeValue.Length >= 4 && arrRtbeValue[3] != "")
                        {
                            //tableInforPara.Rows[thisPageRowIndex].CustomLineColor = Color.FromName(arrRtbeValue[3]);
                            Color color = Color.FromName(arrRtbeValue[3]);
                            if (!color.IsKnownColor)
                                color = ColorTranslator.FromHtml("#" + arrRtbeValue[3]);
                            tableInforPara.Rows[thisPageRowIndex].CustomLineColor = color;
                        }
                        else
                        {
                            tableInforPara.Rows[thisPageRowIndex].CustomLineColor = _RowLineType_Color; //已录入的数据，如果之前没有指定颜色，那么用默认的，而不是选择的（会变）
                        }
                    }
                }
                else
                {
                    thisCell = tableInforPara.CellByRowNo_ColName(thisPageRowIndex, xe.Name);

                    if (thisCell != null)
                    {
                        if (string.IsNullOrEmpty(xe.Value))
                        {
                            RestCell(tableInforPara, thisCell); //RestCell(thisCell);//一般走不到这里，因为不保存的话，就没有该属性；但是这个分支还是处理一下，防止以后可能走到
                            continue;
                        }

                        //排序后：Text 0¤单红线 1¤双红线 2¤权限 3¤Rtf 4 
                        //比一般输入框，还多出：对角线，合并单元格，单元格线；以及行线，行的字体颜色
                        arrRtbeValue = xe.Value.Split('¤');

                        //防止只有数值插入的错误数据：长度为1。
                        if (arrRtbeValue.Length == 1)
                        {
                            arrRtbeValue = (xe.Value + "¤False¤False¤¤False¤False¤False¤¤Black¤").Split('¤');
                            Comm.LogHelp.WriteErr("发现异常的表格数据项目：【" + xe.Name + "】的值为：" + xe.Value + "，缺少：" + "¤False¤False¤¤False¤False¤False¤¤Black¤");
                        }

                        if (!string.IsNullOrEmpty(arrRtbeValue[9])) //Rtf富文本
                        {
                            //如果存在富文本，那么就会有值；否则为空
                            thisCell.IsRtf_OR_Text = true;
                            thisCell.Rtf = arrRtbeValue[9];
                            thisCell.Text = arrRtbeValue[0];

                            //利用保存到公用区域，并设置唯一编号：CR1、CR2 固定的字母CR开头表示公用的Rtf，后面跟数字编号，唯一区分。
                            if (thisCell.CellName.StartsWith("签名") || thisCell.CellName.StartsWith("记录者"))
                            {
                                if (thisCell.Rtf.StartsWith("CR"))
                                {
                                    //"NurseForm/CommonRtfs/CommonRtf" + "[@ID='" + thisCell.Rtf + "']"
                                    XmlNode commonRtfNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), thisCell.Rtf, "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.CommonRtfs), nameof(EntXmlModel.CommonRtf)));
                                    if (commonRtfNode != null)
                                    {
                                        arrRtbeValue[9] = (commonRtfNode as XmlElement).GetAttribute(nameof(EntXmlModel.Value));
                                        thisCell.Rtf = arrRtbeValue[9];
                                    }
                                    else
                                    {
                                        thisCell.IsRtf_OR_Text = false;
                                        thisCell.Text = arrRtbeValue[0];
                                        thisCell.Rtf = "";

                                        ShowInforMsg("找不到共用的富文本：" + thisCell.Rtf, true);
                                    }
                                }
                            }
                        }
                        else
                        {
                            thisCell.IsRtf_OR_Text = false;
                            thisCell.Text = arrRtbeValue[0];
                            thisCell.Rtf = "";  //强制为空，因为初始化表格的时候，会走ResetDefault()方法；赋值成表示空的rtf的非空文本
                        }

                        //手膜签名：公共的rtf数据设置过来
                        if (string.IsNullOrEmpty(arrRtbeValue[0].Trim()) && string.IsNullOrEmpty(arrRtbeValue[9].Trim()) && !string.IsNullOrEmpty(arrRtbeValue[3]) && (xe.Name.StartsWith("签名") || xe.Name.StartsWith("记录者")))
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
                            else
                            {
                                //先判断数据中有没有手膜签名图片
                                XmlNode imgRtfNode = fingerPrintSignImages.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), arrRtbeValue[3], "=", nameof(EntXmlModel.Image)));
                                //如果第一次签名是批量数据界面签名的，那么数据文件中没有对应工号的rtf数据，需要重新获取
                                //if (imgRtfNode == null)
                                //{
                                //    //如果还没有手膜签名的图片rtf保存到表单数据xml中，那么是取不到的。需要添加图片
                                //    //手膜签名
                                //    FingerPrintSign(tableInforPara.Columns[thisCell.ColIndex].RTBE, arrRtbeValue[3], true);

                                //    imgRtfNode = fingerPrintSignImages.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), arrRtbeValue[3], "=", nameof(EntXmlModel.Image)));

                                //    if (_drawTitleHospitalName == "") //不是外部接口调用，而是界面执行的时候，要进行保存
                                //    {
                                //        IsNeedSave = true;
                                //        ShowInforMsg("加载了签名图片：" + arrRtbeValue[3]);
                                //    }
                                //}

                                if (imgRtfNode != null)
                                {
                                    thisCell.IsRtf_OR_Text = true;
                                    thisCell.Rtf = (imgRtfNode as XmlElement).GetAttribute(nameof(EntXmlModel.Value));
                                    thisCell.Text = arrRtbeValue[0];
                                }
                            }
                        }

                        //双红线
                        if (!bool.TryParse(arrRtbeValue[1], out tempBool))
                        {
                            tempBool = false;
                        }
                        thisCell.SingleLine = tempBool;

                        if (!bool.TryParse(arrRtbeValue[2], out tempBool))
                        {
                            tempBool = false;
                        }
                        thisCell.DoubleLine = tempBool;

                        thisCell.CreateUser = arrRtbeValue[3];

                        //Text 0¤单红线 1¤双红线 2¤权限 3¤Rtf 4
                        //如果是表格，还有：有无下底线，对角线，合并单元格，单元格线颜色；以及 行的线种类和行的文字颜色。

                        //4.是否隐藏上边框线，底线
                        if (!bool.TryParse(arrRtbeValue[4], out tempBool))
                        {
                            tempBool = false;
                        }
                        thisCell.NotTopmLine = tempBool;

                        //5.是否正对角线
                        if (!bool.TryParse(arrRtbeValue[5], out tempBool))
                        {
                            tempBool = false;
                        }
                        thisCell.DiagonalLineZ = tempBool;

                        //6.是否反对角线
                        if (!bool.TryParse(arrRtbeValue[6], out tempBool))
                        {
                            tempBool = false;
                        }
                        thisCell.DiagonalLineN = tempBool;

                        //8.单元格边框线
                        if (arrRtbeValue[8] == "")
                        {
                            thisCell.CellLineColor = Color.Black; //被合并的单元格，下次再打开的时候，如果是空，那么就是白色了。
                        }
                        else
                        {
                            thisCell.CellLineColor = Color.FromName(arrRtbeValue[8]);
                        }

                        //如果还是未知的颜色，那么默认为黑色边框线
                        if (!thisCell.CellLineColor.IsKnownColor)
                        {
                            thisCell.CellLineColor = Color.Black;
                        }

                        //10.修订内容  //大于10表示有修订历史
                        if (arrRtbeValue.Length > 10)
                        {
                            thisCell.EditHistory = arrRtbeValue[10];
                        }

                        //11.日期格式
                        if (arrRtbeValue.Length > 11)
                        {
                            //if (thisCell.CellName == "日期")
                            if (thisCell.CellName.StartsWith("日期"))
                            {
                                thisCell.DateFormat = arrRtbeValue[11];
                            }
                        }
                        else
                        {
                            //if (thisCell.CellName == "日期")
                            if (thisCell.CellName.StartsWith("日期"))
                            {
                                thisCell.DateFormat = tableInforPara.Columns[thisCell.ColIndex].RTBE._YMD_Default;
                            }
                        }

                        //7.合并单元格， 如：R,2,False ; R,"",True 
                        if (string.IsNullOrEmpty(arrRtbeValue[7]))
                        {
                            //空就表示：没有合并单元格

                            //thisCell.MergedRight = true;
                            //thisCell.RowSpan = 0;
                            //thisCell.ColSpan = 0;
                        }
                        else
                        {
                            arrRtbeValue = arrRtbeValue[7].Split(',');
                            if (!bool.TryParse(arrRtbeValue[2], out tempBool))
                            {
                                tempBool = false;
                            }

                            if (tempBool)
                            {
                                //被合并的子单元格：不处理，在主合并单元格中就处理掉
                            }
                            else
                            {
                                //合并单元格，主对象格子。
                                int count = int.Parse(arrRtbeValue[1]);
                                if (arrRtbeValue[0] == "R")
                                {
                                    //横向合并
                                    thisCell.ColSpan = count;

                                    for (int i = 1; i <= count; i++)
                                    {
                                        if (thisCell.ColIndex + i < tableInforPara.ColumnsCount)
                                        {
                                            mergedCell = tableInforPara.Cells[thisCell.RowIndex, thisCell.ColIndex + i];

                                            mergedCell.IsMergedNotShow = true;//被合并的单元格
                                            mergedCell.MergedRight = true;

                                            mergedCell.MergedCell = new Point(thisCell.RowIndex, thisCell.ColIndex); //合并的源头记录的主单元格
                                        }
                                    }
                                }
                                else
                                {
                                    //纵向合并：C
                                    thisCell.RowSpan = count;

                                    for (int i = 1; i <= count; i++)
                                    {
                                        if (thisCell.RowIndex + i < tableInforPara.RowsCount * tableInforPara.GroupColumnNum)
                                        {
                                            mergedCell = tableInforPara.Cells[thisCell.RowIndex + i, thisCell.ColIndex];

                                            mergedCell.IsMergedNotShow = true;//被合并的单元格
                                            mergedCell.MergedRight = false;

                                            mergedCell.MergedCell = new Point(thisCell.RowIndex, thisCell.ColIndex); //合并的源头记录的主单元格
                                        }
                                    }


                                }

                                //---------------------------------------------重新计算单元格大小
                                //Size szNew = thisCell.CellSize; //重复打开，会不断累加
                                Size szNew = tableInforPara.Columns[thisCell.ColIndex].RTBE._DefaultCellSize;
                                int row = thisCell.RowIndex;
                                int col = thisCell.ColIndex;

                                for (int i = 1; i <= count; i++)
                                {
                                    if (arrRtbeValue[0] == "R")
                                    {
                                        //横向合并
                                        szNew = new Size(szNew.Width + tableInforPara.Cells[row, col + i].CellSize.Width, szNew.Height);
                                    }
                                    else
                                    {
                                        //纵向合并单元格，再在上面插入行，导致跨页会报错
                                        //如果是纵向合并的，由于插入行的时候，正好合并的几行的最后一行到下一页，就会报错（超过最大行了）
                                        int mergeRows = row + i;
                                        if (row + i >= tableInforPara.RowsCount)
                                        {
                                            //mergeRows = tableInforPara.RowsCount - 1;
                                            //会导致竖线过长，点击后输入框过高，索引报错；但是现在这个方法里面修改，也只会每次打开的时候刷新。实时插入/删除行不会刷新的
                                        }
                                        else
                                        {
                                            szNew = new Size(szNew.Width, szNew.Height + tableInforPara.Cells[mergeRows, col].CellSize.Height);
                                        }
                                    }
                                }

                                thisCell.CellSize = szNew;

                                thisCell.Rect = new Rectangle(thisCell.Loaction, thisCell.CellSize);
                                //---------------------------------------------重新计算单元格大小

                            }
                        } //合并单元格设置结束
                    }
                }
            }

            arrRtbeValue = null;
            arrTemp = null;
        }
        # endregion 解析数据节点，设置表格的行

        # region 添加节点，和表格行绑定
        /// <summary>
        /// 在xml最后位置添加Record节点，并和当前页表格绑定。
        /// </summary>
        /// <param name="thisPageRowIndex"></param>
        private void AddNewRecordNodeForRow(int thisPageRowIndex)
        {
            XmlNode recordNode = TemplateHelp.AddXmlRecordNode(_RecruitXML);
            int temp = _TableInfor.AddRowGetMaxID();
            (recordNode as XmlElement).SetAttribute(nameof(EntXmlModel.ID), temp.ToString());

            //设置表格实体中行的ID，在表格单元格失去焦点保存 数据的时候，根据这个ID更新xml中的数据
            _TableInfor.Rows[thisPageRowIndex].ID = temp;
            _TableInfor.Rows[thisPageRowIndex].UserID = "";
            _TableInfor.Rows[thisPageRowIndex].UserName = "";
            _TableInfor.Rows[thisPageRowIndex].NurseFormLevel = "";

            XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            (recordsNode as XmlElement).SetAttribute(nameof(EntXmlModel.MaxID), temp.ToString());

            //如果是新建页，第一行的日期时间为空，则填写上默认的日期和时间
            //添加的空行是第一行，那么就要赋默认日期时间了
            if (_IsCreating && thisPageRowIndex == 0)
            {
                if (_TableType && _TableInfor != null)
                {
                    DateTime serverDate;

                    serverDate = GetServerDate();

                    //创建表单和新建页的时候，第一行的日期设置默认值
                    //设置新建页的第一行默认日期
                    CellInfor cellDate = _TableInfor.CellByRowNo_ColName(0, "日期");
                    if (cellDate != null)
                    {
                        //日期="2017-10-28¤False¤False¤Mandalat¤False¤False¤False¤¤Black¤¤¤MM-dd"  新建页的日期能自动格式化，创建表单时第一页不行
                        cellDate.Text = string.Format("{0:yyyy-MM-dd}", serverDate);
                        cellDate.DateFormat = _TableInfor.Columns[cellDate.ColIndex].RTBE._YMD;
                        (recordNode as XmlElement).SetAttribute("日期", string.Format("{0:yyyy-MM-dd}", serverDate) + "¤False¤False¤¤False¤False¤False¤¤Black¤");
                    }
                }
            }
        }
        # endregion 添加节点，和表格行绑定

        # region 表格中，按键切换光标
        /// <summary>
        /// 表格中单元格输入框，按键切换光标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private bool Is_SHIFT = false;
        private void CellsRtbeSetFocus_KeyDown(object sender, KeyEventArgs e)
        {
            //当前RichTextBox
            RichTextBoxExtended rtbe = (RichTextBoxExtended)sender;

            try
            {
                ////如果单元格是多行的，那么就会跳到下一个单元格，然后再下一个单元格回车了
                //if (rtbe.MultilineCell)
                //{
                //    return;  //只要处理回车键
                //}

                //左右键不处理，在输入框内部移动光标；Enter要非多行单元格才有效  _CurrentCellIndex = new Point(i, j);
                //if (_CurrentCellIndex.X != -1 && e.KeyCode == Keys.Tab || (e.KeyCode == Keys.Enter && _TableInfor.CurrentCell.RowSpan == 0)) 
                if (_CurrentCellIndex.X != -1 && e.KeyCode == Keys.Tab ||
                    ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)))   //  && !rtbe.Multiline && _TableInfor.CurrentCell.RowSpan == 0 //多行数据丢失
                {
                    //防止和输入智能提示补全的回车选择键入，产生干扰
                    if (e.KeyCode == Keys.Enter && autocompleteMenuForWrite != null && autocompleteMenuForWrite.Host.Visible)
                    {
                        return;
                    }

                    if ((e.Shift && e.KeyCode == Keys.Tab) || (e.KeyCode == Keys.Left && (rtbe.SelectionStart == 0 || rtbe.SelectionLength == rtbe.Text.Length)))
                    {
                        if (_CurrentCellIndex.Y > 0)
                        {
                            if (_TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y - 1].IsMergedNotShow)
                            {
                                //如果是被合并掉的单元格，那么跳过
                                ShowCellRTBE(_TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y - 1].MergedCell.X, _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y - 1].MergedCell.Y);
                            }
                            else
                            {
                                ShowCellRTBE(_CurrentCellIndex.X, _CurrentCellIndex.Y - 1);
                            }
                        }
                        else
                        {
                            if (_CurrentCellIndex.X > 0)
                            {
                                if (_TableInfor.Cells[_CurrentCellIndex.X - 1, _TableInfor.ColumnsCount - 1].IsMergedNotShow)
                                {
                                    //如果是被合并掉的单元格，那么跳过
                                    ShowCellRTBE(_TableInfor.Cells[_CurrentCellIndex.X - 1, _TableInfor.ColumnsCount - 1].MergedCell.X, _TableInfor.Cells[_CurrentCellIndex.X - 1, _TableInfor.ColumnsCount - 1].MergedCell.Y);
                                }
                                else
                                {
                                    ShowCellRTBE(_CurrentCellIndex.X - 1, _TableInfor.ColumnsCount - 1);
                                }
                            }
                            else
                            {
                                //base.OnKeyDown(e);
                                //SendKeys.Send("^ +{TAB}");
                            }
                        }
                    }
                    else if (e.KeyCode == Keys.Tab || (e.KeyCode == Keys.Right && (rtbe.SelectionStart == rtbe.Text.Length || rtbe.SelectionLength == rtbe.Text.Length)))
                    {
                        if (_CurrentCellIndex.Y + 1 < _TableInfor.ColumnsCount)
                        {
                            if (_TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y + 1].IsMergedNotShow && _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].ColSpan > 0)
                            {
                                //如果是被合并掉的单元格，那么跳过
                                ShowCellRTBE(_CurrentCellIndex.X, _CurrentCellIndex.Y + 1 + _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].ColSpan);
                            }
                            else if (_TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y + 1].IsMergedNotShow) //下一个是纵向合并的情况下，
                            {
                                //如果是被合并掉的单元格，那么跳过
                                ShowCellRTBE(_TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y + 1].MergedCell.X, _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y + 1].MergedCell.Y);
                            }
                            else
                            {
                                ShowCellRTBE(_CurrentCellIndex.X, _CurrentCellIndex.Y + 1);
                            }
                        }
                        else
                        {
                            //if (_CurrentCellIndex.X < _TableInfor.RowsCount - 1)
                            if (_CurrentCellIndex.X < _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
                            {
                                if (_TableInfor.Cells[_CurrentCellIndex.X + 1, 0].IsMergedNotShow && _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].ColSpan > 0)
                                {
                                    //如果是被合并掉的单元格，那么跳过
                                    ShowCellRTBE(_CurrentCellIndex.X, _CurrentCellIndex.Y + 1 + _TableInfor.Cells[_CurrentCellIndex.X + 1, 0].ColSpan);
                                }
                                else
                                {
                                    ShowCellRTBE(_CurrentCellIndex.X + 1, 0);
                                }
                            }
                        }
                    }
                    else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
                    {
                        //单元格自动换行
                        if (((RichTextBoxExtended)sender).Multiline)  // || ((RichTextBoxExtended)sender).MultilineCell
                        {
                            if (((RichTextBoxExtended)sender).SelectedText.Length != ((RichTextBoxExtended)sender).Text.Length) //全选时
                            {
                                if (e.KeyCode == Keys.Enter)
                                {
                                    return; //如果全选情况下，回车键会把数据全部清空
                                }
                            }
                        }

                        int rowIndex = ((RichTextBoxExtended)sender).GetLineFromCharIndex(((RichTextBoxExtended)sender).SelectionStart);//获得光标所在行的索引值

                        if (rowIndex == ((RichTextBoxExtended)sender).Lines.Length - 1 || ((RichTextBoxExtended)sender).SelectedText.Length == ((RichTextBoxExtended)sender).Text.Length)
                        {
                            if (_CurrentCellIndex.X < _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
                            {
                                //垂直合并的处理
                                //if (_TableInfor.Cells[_CurrentCellIndex.X + 1, _CurrentCellIndex.Y].IsMergedNotShow)
                                if (_TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].RowSpan > 0)
                                {
                                    if (_CurrentCellIndex.X + 1 + _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].RowSpan < _TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1)
                                    {
                                        ShowCellRTBE(_CurrentCellIndex.X + 1 + _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].RowSpan, _CurrentCellIndex.Y); //合并的回车时走这里
                                    }
                                    else
                                    {
                                        ShowCellRTBE(0, _CurrentCellIndex.Y);
                                    }
                                }
                                else if (_TableInfor.Cells[_CurrentCellIndex.X + 1, _CurrentCellIndex.Y].IsMergedNotShow)
                                {
                                    //水平合并的处理
                                    ShowCellRTBE(_TableInfor.Cells[_CurrentCellIndex.X + 1, _CurrentCellIndex.Y].MergedCell.X, _TableInfor.Cells[_CurrentCellIndex.X + 1, _CurrentCellIndex.Y].MergedCell.Y);
                                }
                                else
                                {
                                    if (!((RichTextBoxExtended)sender).MultilineCell)
                                    {
                                        ShowCellRTBE(_CurrentCellIndex.X + 1, _CurrentCellIndex.Y); //本身就是多行的单元格，回车键走车里，数据会清空。所以分开处理
                                    }
                                    else
                                    {
                                        ShowCellRTBE(_CurrentCellIndex.X + 1, _CurrentCellIndex.Y); //本身就是多行的单元格，回车键走车里,数据清空
                                        e.Handled = true;
                                    }
                                }

                            }
                            else
                            {
                                if (e.KeyCode == Keys.Down) //回车键不循环，因为有些护士医生可能输入后按回车，不希望迁至第一行。如果要循环用上下键切换
                                {
                                    ShowCellRTBE(0, _CurrentCellIndex.Y);
                                }
                            }
                        }
                    }
                    else if (e.KeyCode == Keys.Up)
                    {
                        int rowIndex = ((RichTextBoxExtended)sender).GetLineFromCharIndex(((RichTextBoxExtended)sender).SelectionStart);//获得光标所在行的索引值

                        if (rowIndex == 0)
                        {
                            //输入框空间的ProcessCmdKey中(keyData == Keys.Up || keyData == Keys.Down) 注释掉，就不会全选
                            if (_CurrentCellIndex.X > 0)
                            {
                                if (_TableInfor.Cells[_CurrentCellIndex.X - 1, _CurrentCellIndex.Y].IsMergedNotShow)
                                {
                                    ShowCellRTBE(_TableInfor.Cells[_CurrentCellIndex.X - 1, _CurrentCellIndex.Y].MergedCell.X, _TableInfor.Cells[_CurrentCellIndex.X - 1, _CurrentCellIndex.Y].MergedCell.Y);
                                }
                                else
                                {
                                    ShowCellRTBE(_CurrentCellIndex.X - 1, _CurrentCellIndex.Y);
                                }
                            }
                            else
                            {
                                ShowCellRTBE(_TableInfor.RowsCount * _TableInfor.GroupColumnNum - 1, _CurrentCellIndex.Y);
                            }
                        }
                    }

                }
                //else
                //{
                //    base.OnKeyDown(e);
                //}

                //if (e.Shift && e.KeyCode == Keys.Tab)
                //{
                //    rebe.Focus();
                //    e.Handled = true;              
                //}
                //else
                //{
                //    base.OnKeyDown(e);
                //}
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }
        # endregion 表格中，按键切换光标

        # region 将单元格保存到Xml
        private bool SaveCellToXML(TransparentRTBExtended rtbe)
        {
            if (rtbe.OldRtf != rtbe.Rtf && !(rtbe.Text == "" && rtbe.OldRtf == ""))
            {
                if (rtbe.Name.StartsWith("日期") && rtbe._FullDateText == rtbe.OldText)
                {
                    //日期时间的格式是简化格式，光标进入会变成完整格式，但是如果没有修改-只是光标进入呢

                    #region 日期单元格文字颜色手动修改设置，或者合并整行设置颜色
                    //如果是日期列单元格修改文字颜色（通常是合并整行），那么可能设置不上，下次打开就没有了
                    //新表单合并及自动生成日期问题
                    //if (rtbe.OldRtf != rtbe.Rtf && rtbe._IsRtf) //这样的话，只要日期是rtf的，每次光标进入离开都会提示保存了。
                    if (rtbe._IsRtf && GetRtfColor(rtbe.OldRtf) != GetRtfColor(rtbe.Rtf))
                    {
                        IsNeedSave = true;             //需要提示保存
                        _CurrentCellNeedSave = true;    //就算文本不需要进行保存的场合，也可能添加的行线等信息需要保存呢
                    }
                    #endregion 日期单元格文字颜色手动修改设置，或者合并整行设置颜色
                }
                else
                {
                    IsNeedSave = true;             //需要提示保存
                    _CurrentCellNeedSave = true;    //就算文本不需要进行保存的场合，也可能添加的行线等信息需要保存呢
                }
            }
            else
            {
                if (_CurrentCellNeedSave && !IsNeedSave)
                {
                    IsNeedSave = true; //需要提示保存
                }
            }

            //等于日期的时候，修改dateFormat完整的年月日格式，这里不会触发需要保存的
            //if (rtbe.Name == "日期")
            if (rtbe.Name.StartsWith("日期"))
            {
                //// 日期时间的格式是简化格式，光标进入会变成完整格式，但是如果没有修改-只是光标进入呢
                //if (rtbe._FullDateText == rtbe.OldText)
                //{
                //    _CurrentCellNeedSave = false; //放在这里处理，会导致已经需要保存的也会变成不需要保存了
                //    _IsNeedSave = false;
                //}
                //// 日期时间的格式是简化格式，光标进入会变成完整格式，但是如果没有修改

                if (rtbe._YMD_Old != rtbe._YMD)
                {
                    _CurrentCellNeedSave = true;
                    IsNeedSave = true;
                }
            }

            //此时，如果是日期，可能不是完整的年月日，是对应格式的字符串，下面会再处理的
            _TableInfor.CurrentCell.Text = rtbe.Text; //文字也要赋值，否则清空后，调整段落行/打印的时候，这个单元格还是有值的。

            //如果是合并到倒数最后一行（再往下没有空白行，也一样），那么这里就会在保存的时候异常。只要被合并行下面也有值，聚会报null异常：SaveAutoSign()导致上一行赋值异常: _TableInfor.CurrentCell为null

            _TableInfor.CurrentCell.Rtf = rtbe.Rtf;   //有rtf也有可能是纯文本的。    
            _TableInfor.CurrentCell.EditHistory = rtbe._EditHistory;

            //if (_TableInfor.CurrentCell.CellName == "日期")
            if (_TableInfor.CurrentCell.CellName.StartsWith("日期"))
            {
                _TableInfor.CurrentCell.DateFormat = rtbe._YMD;
                _TableInfor.CurrentCell.Text = rtbe._FullDateText;  //可能不是完整的年月日，这里不赋值也没有关系，因为9828行验证通过也会将完整日期复制过去的

                if (rtbe.Text.Trim() == "")
                {
                    _TableInfor.CurrentCell.Text = "";
                }
            }

            _TableInfor.CurrentCell.IsRtf_OR_Text = rtbe._IsRtf;

            _TableInfor.CurrentCell.SingleLine = rtbe._HaveSingleLine;
            _TableInfor.CurrentCell.DoubleLine = rtbe._HaveDoubleLine;

            //更新到xml中，以便数据更新后统计
            XmlNode recordsNode = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
            int nodeId = _TableInfor.Rows[_CurrentCellIndex.X].ID;
            XmlNode cellNode = recordsNode.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), nodeId.ToString(), "=", nameof(EntXmlModel.Record)));

            string name = rtbe.Name;
            string value = "";
            string rowInforPara = "";
            //string[] eachArr; //分割数组

            if (cellNode != null)
            {
                //if (cellNode != null && rtbe.OldRtf != rtbe.Rtf ) //进行赋默认值的时候，控制不好，就会导致判断异常
                if (_CurrentCellNeedSave) //(rtbe.OldRtf != rtbe.Rtf && !(rtbe.Text == "" && rtbe.OldRtf == ""))
                {
                    value = GetCellSaveValue(rtbe, _TableInfor.CurrentCell, ref rowInforPara);

                    (cellNode as XmlElement).SetAttribute(name, value);              //单元格信息
                    (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.ROW), rowInforPara);      //行信息

                    //nameof(EntXmlModel.VALIDATE)
                    (cellNode as XmlElement).SetAttribute(nameof(EntXmlModel.VALIDATE), string.Format("{0:yyyy-MM-dd HH:mm}", GetServerDate()));      //最后更新时间

                    ////"DATETIME" 存在多种情况:1.调整的段落中日期和时间都为空; 日期时间都有; 只有时间// 不管怎么样第一行必须有完整的日期和时间
                    //if (_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "日期") != null && _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间") != null)
                    //{
                    //    //如本行没日期或者时间,遍历XMl中往上的节点,取到第一个满足的作为自己的日期或者时间，如果无法确定时间，那就报错提醒。
                    //    //
                    //    //string dt = GetRowDateTime(_TableInfor.Rows[_CurrentCellIndex.X]);
                    //    //if (MyDate.IsDateTime(dt))
                    //    //{
                    //    //    (cellNode as XmlElement).SetAttribute("DATETIME", dt);      //日期时间,这个可以和日期时间显示的规律,
                    //    //}
                    //    //else
                    //    //{
                    //    //    MessageBox.Show("请填写日期、时间，当前行无法确定日期时间。");
                    //    //}
                    //}

                }
            }
            else
            {
                //异常情况，正常是肯定有对应的节点的
                string msg = _CurrentCellIndex.ToString() + "为索引的单元格，找不到它所对应的XML节点；数据无法保存，请尝试重新打开后再操作。";
                ShowInforMsg(msg, true);
                Comm.LogHelp.WriteErr("\r\n单元格所在行索引，无法确定Xml数据中的ID：" + _RecruitXML.InnerXml);

                MessageBox.Show(msg, "提示消息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            bool tempRet = _CurrentCellNeedSave;

            _CurrentCellNeedSave = false;

            return tempRet;
        }
        #endregion 将单元格保存到Xml

        #region 表格输入框：光标离开，保存 数据

        /// <summary>
        /// 表格单元格数据验证 & 保存数据到Xml便于实时统计
        /// 表格输入框：光标离开，保存 数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cells_LeaveSaveData(object sender, EventArgs e)
        {
            //说明：消息中必须包含：第N行，否则，添加行，删除行后，索引行的位置就改变了，就会混乱
            try
            {
                if (_IsLocked) return;
                if (!_IsNotNeedCellsUpdate && _TableInfor.CurrentCell != null) //!_IsLoading && 会导致有些事情舒服不能保存的
                {
                    TransparentRTBExtended rtbe = (TransparentRTBExtended)sender;

                    //rtbe._IsSetValue = true;
                    ////全角转换成半角
                    //rtbe.Text = StrHalfFull.ToDBC_Half(rtbe.Text);  //会导致日期保存的格式不正确。保存的不是完整的日期，以后无法判断了。
                    //rtbe._IsSetValue = false;

                    //日期和时间，如果是全角，变成半角再处理
                    if (rtbe.Name.StartsWith("时间") || rtbe.Name.StartsWith("日期"))
                    {

                        if (rtbe.Name.StartsWith("时间"))
                        {
                            //无意中测试发现(时间列有问题，光标移进移出后变成rtf富文本了)，如果是居中的单元格，在合并纵向合并后，直接点击。画布，对齐没有问题，如果点击其他单元格，就会出问题了。
                            HorizontalAlignment temp = rtbe.SelectionAlignment;

                            //全角转换成半角
                            rtbe.Text = StrHalfFull.ToDBC_Half(rtbe.Text);  //会导致日期保存的格式不正确。

                            //直接输入数字，格式化成时间的处理
                            NumberToTime(rtbe);

                            rtbe.SelectionAlignment = temp;
                        }
                    }

                    bool changed = SaveCellToXML(rtbe); // 将数据保存到Xml

                    #region 判断是否需要更新到体温单中

                    if (changed && _SynchronizeTemperatureRecordItems.Contains(rtbe.Name))
                    {
                        string nodeId = _TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].ID.ToString();
                        //XmlNode cellNode = recordsNode.SelectSingleNode("Record[@ID='" + nodeId + "']"); //当前行，在xml数据中的节点
                        string dateTimeStr = GetRowDateTime(nodeId);

                        bool exist = false;
                        RecordInor recordInor = null; //bool isExist = false;
                        for (int k = 0; k < _SynchronizeTemperatureRecordListRecordInor.Count; k++)
                        {
                            if (((RecordInor)_SynchronizeTemperatureRecordListRecordInor[k]).ID == nodeId)
                            {
                                exist = true;
                                recordInor = (RecordInor)_SynchronizeTemperatureRecordListRecordInor[k];
                                break;
                            }
                        }

                        if (recordInor == null)
                        {
                            //exist = false;
                            string[] arr = dateTimeStr.Split('¤');
                            recordInor = new RecordInor(nodeId, arr[0].Trim(), arr[1].Trim()); //id,日期,时间
                        }
                        else
                        {
                            //1.有可能，录入后，再修改了日期和时间呢
                            //2.删除行，这个也要删除
                        }

                        if (rtbe.Name == "体温" && !string.IsNullOrEmpty(_DefaultTemperatureType))
                        {
                            if (!string.IsNullOrEmpty(rtbe.Text.Trim())) //如果为空，那么不需要拼接Y，否则体温数据格式异常了。
                            {
                                if (!rtbe.Text.Trim().EndsWith(_DefaultTemperatureType))
                                {
                                    recordInor.AddItem(rtbe.Name, rtbe.Text.Trim() + _DefaultTemperatureType);
                                }
                                else
                                {
                                    recordInor.AddItem(rtbe.Name, rtbe.Text.Trim());
                                }
                            }
                            else
                            {
                                //更新成空了。
                                recordInor.AddItem(rtbe.Name, rtbe.Text.Trim());
                            }
                        }
                        else
                        {
                            recordInor.AddItem(rtbe.Name, rtbe.Text.Trim());
                        }

                        if (!exist)
                        {
                            _SynchronizeTemperatureRecordListRecordInor.Add(recordInor);
                        }
                    }

                    #endregion 判断是否需要更新到体温单中

                    if (_IsSaveingNow)
                    {
                        return;
                    }

                    if (_TableInfor.CurrentCell.IsMerged()) //已经合并的单元格，不做格式验证，也不做Maxlength限制
                    {
                        return;
                    }

                    bool isOk = true;
                    string errMsg = "";

                    //验证输入框内容格式，是否合理
                    //if (rtbe.Name == "时间")
                    if (rtbe.Name.StartsWith("时间"))
                    {
                        //验证
                        //第一行，日期和时间都不能为空
                        if (_CurrentCellIndex.X == 0 && _FirstRowMustDate)
                        {
                            errMsg = "每页的第1行中的日期和时间不能为空。";

                            if (rtbe.Text.Trim() == "")
                            {
                                if (_Need_Assistant_Control != null)
                                {
                                    ShowSearch(_Need_Assistant_Control); // 显示指示图标
                                }

                                isOk = false;

                                if (!_ErrorList.Contains(errMsg))
                                {
                                    _ErrorList.Add(errMsg);
                                }

                                MessageBox.Show(errMsg, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                _ErrorList.Remove(errMsg);
                            }
                        }

                        errMsg = "第" + (_CurrentCellIndex.X + 1).ToString() + "行中输入的时间格式不正确，有效格式为：HH:mm";

                        if (!DateHelp.IsTime(rtbe.Text) && rtbe.Text.Trim() != "")
                        {
                            if (_Need_Assistant_Control != null)
                            {
                                ShowSearch(_Need_Assistant_Control); // 显示指示图标
                            }

                            isOk = false;

                            if (!_ErrorList.Contains(rtbe.Name + "第" + (_CurrentCellIndex.X + 1).ToString() + "行：" + errMsg))
                            {
                                _ErrorList.Add(rtbe.Name + "第" + (_CurrentCellIndex.X + 1).ToString() + "行：" + errMsg);
                            }

                            MessageBox.Show(errMsg, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            //rtbe.Text = string.Format();
                            _ErrorList.Remove(rtbe.Name + "第" + (_CurrentCellIndex.X + 1).ToString() + "行：" + errMsg);
                        }

                        //不为空的时候，进行标准格式化
                        if (isOk && !string.IsNullOrEmpty(rtbe.Text.Trim()))
                        {
                            //护理部要求写入24:00
                            if (rtbe.Text != "24:00")
                            {
                                string formatText = string.Format("{0:" + DateFormat_HM + "}", DateTime.Parse("2017-7-15 " + rtbe.Text));
                                if (formatText != rtbe.Text)
                                {
                                    _IsSaveingNow = true;
                                    rtbe.Text = formatText;     //会再次触发本事件，用_IsSaveingNow来跳出，不处理，下面强行保存单元格信息
                                    SaveCellToXML(rtbe);        // 将数据保存到Xml
                                    _IsSaveingNow = false;
                                }
                            }
                        }

                    }
                    else if (rtbe.Name.StartsWith("日期"))   //else if (rtbe.Name == "日期")
                    {
                        //验证
                        //第一行，日期和时间都不能为空
                        if (_CurrentCellIndex.X == 0 && _FirstRowMustDate)
                        {
                            //选择弹出日历框的时候，也会触发，不用保存。但是如果点击其他地方也会不验证

                            errMsg = "每页的第1行中的日期和时间不能为空。";

                            if (rtbe.Text.Trim() == "")
                            {
                                if (_Need_Assistant_Control != null)
                                {
                                    ShowSearch(_Need_Assistant_Control); // 显示指示图标
                                }

                                isOk = false;

                                if (!_ErrorList.Contains(rtbe.Name + "第" + (_CurrentCellIndex.X + 1).ToString() + "行：" + errMsg))
                                {
                                    _ErrorList.Add(rtbe.Name + "第" + (_CurrentCellIndex.X + 1).ToString() + "行：" + errMsg);
                                }

                                MessageBox.Show(errMsg, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                //return;
                            }
                            else
                            {
                                //rtbe.Text = string.Format();
                                _ErrorList.Remove(rtbe.Name + "第" + (_CurrentCellIndex.X + 1).ToString() + "行：" + errMsg);
                            }
                        }
                        //else
                        //{

                        errMsg = "第" + (_CurrentCellIndex.X + 1).ToString() + "行中输入的日期格式不正确，有效格式为：yyyy-MM-dd";

                        if (!DateHelp.IsDate(rtbe.Text) && rtbe.Text.Trim() != "" && !DateHelp.IsDate(rtbe._FullDateText))
                        {
                            ShowSearch(_Need_Assistant_Control); // 显示指示图标

                            isOk = false;

                            if (!_ErrorList.Contains(rtbe.Name + "第" + (_CurrentCellIndex.X + 1).ToString() + "行：" + errMsg))
                            {
                                _ErrorList.Add(rtbe.Name + "第" + (_CurrentCellIndex.X + 1).ToString() + "行：" + errMsg);
                            }

                            MessageBox.Show(errMsg, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            _TableInfor.CurrentCell.Text = rtbe.Text; //显示错误值，不然就会上次完整的日期
                        }
                        else
                        {
                            _TableInfor.CurrentCell.Text = rtbe._FullDateText; //设置为完整日期

                            _ErrorList.Remove(rtbe.Name + "第" + (_CurrentCellIndex.X + 1).ToString() + "行：" + errMsg);
                        }
                        //}
                    }
                    else if (rtbe.Name.Contains("日期") || rtbe.Name.Contains("时间"))
                    {
                        //<InputBox ID="00203" Name="记录时间" PointStart="553,1050" DateFormat="yyyy-MM-dd HH:mm" 
                        //日期时间 时分一起的
                        if (rtbe._YMD_Default != "" && rtbe._YMD_Default.Contains("yyyy") && rtbe._YMD_Default.Contains("mm"))
                        {
                            DateTime temp;
                            errMsg = "第" + (_CurrentCellIndex.X + 1).ToString() + "行中输入的日期时间格式不正确，有效格式为：" + rtbe._YMD_Default;
                            if (!DateTime.TryParse(rtbe.Text, out temp) && rtbe.Text.Trim() != "")
                            {
                                ShowSearch(_Need_Assistant_Control); // 显示指示图标

                                if (!_ErrorList.Contains(rtbe.Name + "：" + errMsg))
                                {
                                    _ErrorList.Add(rtbe.Name + "：" + errMsg);
                                }

                                MessageBox.Show(errMsg, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return;
                            }
                            else
                            {
                                _ErrorList.Remove(rtbe.Name + "：" + errMsg);
                            }
                        }
                    }

                    //录入的时间有效范围验证，输入的时间不得晚于规定的超过系统时间多少范围内。
                    //if (rtbe.Name.Contains("日期") || rtbe.Name.Contains("时间"))                     //if (rtbe.Name == "日期" || rtbe.Name == "时间")
                    if (rtbe.Name == "日期" || rtbe.Name == "时间")
                    {
                        if (_TableInfor != null)
                        {
                            if (_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "日期") != null)
                            {
                                string cellValue = "";
                                if (rtbe.Name == "日期")
                                {
                                    cellValue = rtbe._FullDateText.Trim();
                                }
                                else
                                {
                                    //cellValue = _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "日期").Text.Trim();
                                    XmlNode records = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));
                                    XmlNode thisNode = records.SelectSingleNode("Record[@ID='" + _TableInfor.Rows[_CurrentCellIndex.X].ID + "']");
                                    cellValue = GetRowDateTime((thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID))).Split('¤')[0];
                                }

                                if (_TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间") != null)
                                {
                                    if (rtbe.Name == "时间")
                                    {
                                        cellValue = cellValue + " " + rtbe.Text.Trim();
                                    }
                                    else
                                    {
                                        cellValue = cellValue + " " + _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间").Text.Trim();
                                    }
                                }

                                if (DateHelp.IsDateTime(cellValue))
                                {
                                    string StrMessageValue = "第" + (_CurrentCellIndex.X + 1).ToString() + "行日期时间不合理，不能晚于规定时间。";
                                    DateTime dtRow = DateTime.Parse(cellValue);
                                    TimeSpan ts = dtRow.Subtract(GetServerDate());
                                    if (ts.TotalMinutes > _DSS.InputDateTimeValidate)
                                    {
                                        if (!_ErrorList.Contains(StrMessageValue))
                                        {
                                            _ErrorList.Add(StrMessageValue);
                                        }

                                        ShowSearch(_Need_Assistant_Control); // 显示指示图标

                                        MessageBox.Show(StrMessageValue, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                    else
                                    {
                                        _ErrorList.Remove(StrMessageValue);
                                    }
                                }
                            }
                            else
                            {
                                //如果表格将日期和时间两列合并在一列，叫做日期时间列，那么要这里处理
                                //TODO:将这一列命名为为：“日期”即可
                                //要么去本控件内容进行判断，但是合并的还好。如果不是合并的就不好办了，不知道是哪两列的组合啊。
                                //同样非单元格的，也不处理了RtbeLeave_Validating。
                                //如果硬要处理，判断其dateFormat属性是完整的就进行验证也是可以的。
                            }
                        }
                    }

                    //每个输入框配置的正则表达式验证规则，判断是否满足格式规定
                    if (!string.IsNullOrEmpty(rtbe._CheckRegex))
                    {
                        errMsg = "第" + (_CurrentCellIndex.X + 1).ToString() + "行，" + rtbe.Name + "输入的数据格式不正确。";

                        if (Regex.IsMatch(rtbe.Text.Trim(), rtbe._CheckRegex)) //|| string.IsNullOrEmpty(rtbe.Text.Trim())
                        {
                            _ErrorList.Remove(errMsg);
                        }
                        else
                        {
                            isOk = false;

                            ShowSearch(_Need_Assistant_Control); // 显示指示图标

                            if (!_ErrorList.Contains(rtbe.Name + "：" + errMsg))
                            {
                                _ErrorList.Add(errMsg);
                            }

                            MessageBox.Show(errMsg, "数据格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }

                    }

                    //根据时间，进行行文字颜色自动改变
                    if (rtbe.Name == "时间" && isOk)
                    {
                        //if (!string.IsNullOrEmpty(rtbe.Text.Trim()) && (rtbe.OldRtf != rtbe.Rtf && !(rtbe.Text == "" && rtbe.OldRtf == "")) && _RowForeColorsList.Count > 0) // _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, "时间").Text != rtbe.Text && 
                        if (!string.IsNullOrEmpty(rtbe.Text.Trim()) && rtbe.OldText != rtbe.Text && _RowForeColorsList.Count > 0)
                        {
                            SetCurrentRowForeColor(rtbe); //设置行文字颜色，并刷新界面
                        }
                    }
                }
                else
                {
                    if (!_IsNotNeedCellsUpdate)
                    {
                        if (_TableInfor != null && _TableInfor.CurrentCell == null)
                        {
                            ShowInforMsg("单元格光标离开的时候，无法判断单元格索引位置。");
                            _TableInfor.CurrentCell = _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }
        # endregion 表格输入框：光标离开，保存 数据

        # region 获取表格中每个单元格的保存的文本
        /// <summary>
        /// 获取表格中每个单元格的保存的文本
        /// Text 0¤单红线 1¤双红线 2¤权限 3¤Rtf 4
        /// 如果是表格，还有：有无下底线，对角线，合并单元格，单元格线颜色；以及 行的线种类和行的文字颜色。
        /// </summary>
        /// <param name="rtbe"></param>
        /// <returns></returns>
        private string GetCellSaveValue(RichTextBoxExtended rtbe, CellInfor cell, ref string RowInforPara)
        {
            //string retValue = "";
            StringBuilder retValue = new StringBuilder();

            string CreateUser = rtbe.CreateUser;
            if (string.IsNullOrEmpty(CreateUser))
            {
                //如果输入框为空，那么不要设置创建用户
                if (string.IsNullOrEmpty(rtbe.Text))
                {
                    CreateUser = ""; //不设置
                }
                else
                {
                    CreateUser = GlobalVariable.LoginUserInfo.UserCode; //权限创建用户
                }
            }

            //删除清空手摸签名图片的时候，不要记录CreateUser，否则下次打开还会有
            //如果有图片的话，Text为一个半角空格，而不是直接为空。 判断是否含有图片
            //清除行签名
            if (string.IsNullOrEmpty(rtbe.Text)
                && (_FingerPrintSign && (rtbe.Name.StartsWith("签名") || rtbe.Name.StartsWith("记录者")) && !string.IsNullOrEmpty(rtbe.CreateUser))
                && !rtbe.IsContainImg()) //如果这里处理的话，那么会导致光标离开的时候，赋值后OldRtf和现在的一样
            {
                rtbe._IsRtf = false;
                rtbe.toolStripMenuItemClearFormat_Click(null, null);
                rtbe.CreateUser = "";
                CreateUser = ""; //不设置
            }

            //retValue = rtbe.Text + "¤" + rtbe._HaveSingleLine.ToString() + "¤" + rtbe._HaveDoubleLine.ToString() + "¤" + CreateUser;
            //0.数据值
            //if (rtbe.Name == "日期")
            if (rtbe.Name.StartsWith("日期"))
            {
                retValue.Append(rtbe._FullDateText); // 0
                retValue.Append("¤");
            }
            else
            {
                retValue.Append(rtbe.Text); // 0
                retValue.Append("¤");
            }

            //1.单红线、2.双红线
            retValue.Append(rtbe._HaveSingleLine.ToString());//1
            retValue.Append("¤");
            retValue.Append(rtbe._HaveDoubleLine.ToString());//2
            retValue.Append("¤");

            //3.创建用户，可以扩展于：修改权限控制
            retValue.Append(CreateUser);//3
            retValue.Append("¤");

            _TableInfor.CurrentCell.CreateUser = CreateUser;//更新到表格信息中

            //单元格信息：有无下底线，对角线正和反，合并单元格，单元格线颜色
            bool tempBool = false;

            //4.有无单元格下底线
            tempBool = cell.NotTopmLine;//4
            retValue.Append(tempBool.ToString());
            retValue.Append("¤");

            //5.6.有无正、反对角线
            tempBool = cell.DiagonalLineZ;//5
            retValue.Append(tempBool.ToString());
            retValue.Append("¤");

            tempBool = cell.DiagonalLineN;//6
            retValue.Append(tempBool.ToString());
            retValue.Append("¤");

            //7. 合并单元格信息：三个分项：横/纵、合并数、是否被合并不用显示的
            //合并单元格 R/C+合并数字 + 是否被合并不显示
            //如：R,2,False ; R,"",True  //但是删除行等添加行的时候，这个数字（合并了多少格）也要更着变。
            if (cell.IsMergedNotShow)
            {
                //其实程序走不到这里，因为被合并的单元格，就不会再触发保存事件了
                retValue.Append(",,True¤");  //retValue.Append("¤"); //如果是被合并，那么该分项属性为空
            }
            else
            {
                if (cell.IsMerged())
                {
                    if (cell.ColSpan > 0)
                    {
                        retValue.Append("R," + cell.ColSpan.ToString());
                    }
                    else
                    {
                        retValue.Append("C," + cell.RowSpan.ToString());
                    }

                    retValue.Append(",False¤");
                }
                else
                {
                    retValue.Append("¤"); //如果没有合并，那么该分项属性为空
                }
            }

            //8.单元格线颜色
            retValue.Append(cell.CellLineColor.Name);
            retValue.Append("¤");

            //9. 富文本信息Rtf最后保存，因为可能很长
            if (rtbe._IsRtf)
            {
                //是高级富文本的时候，需要
                //Rtf，Text，单红线，双红线，权限（创建用户），
                //如果是表格，还有：单元格边框线信息，行线信息，合并单元格信息，信息

                //手膜签名和CA签名是冲突的。如果是手膜签名的表单，那么不能进行CA签名，否则签名单元格打开再显示会错乱
                //手摸签名的图片rtf不要每次保存
                if (_FingerPrintSign && string.IsNullOrEmpty(rtbe.Text.Trim())) //没有在图片签名后，继续输入文字的话。如果添加文字的，那么就是不通用的Rtf了，不保存
                {
                    retValue.Append("");
                }
                else
                {
                    //retValue.Append(rtbe.Rtf);

                    //签名输入框的如果显示图片是Rtf模式的时候，那么一张表单中很多单元格数据是一样的。就可以保存到共同的Rtf区域、并设置唯一编号，减少数据容量。加载的时候根据唯一编号从共用区域加载Rtf。
                    if (rtbe.Name.StartsWith("签名") || rtbe.Name.StartsWith("记录者"))
                    {
                        //1.保存到公用区域，并设置唯一编号：CR1、CR2…… 固定的字母CR开头表示公用的Rtf，后面跟数字编号，唯一区分。
                        //string id = CAHistoryForm.SaveCASignRtfToXml(_RecruitXML, rtbe.Rtf);

                        //2.这里保存唯一编号即可
                        //retValue.Append(id);

                    }
                    else
                    {
                        retValue.Append(rtbe.Rtf);
                    }
                }
            }
            else
            {
                retValue.Append("");
            }

            retValue.Append("¤");
            retValue.Append(cell.EditHistory);

            //if (rtbe.Name == "日期")
            if (rtbe.Name.StartsWith("日期"))
            {
                retValue.Append("¤");
                retValue.Append(rtbe._YMD);
            }

            //10. 11. 12行信息：行的线颜色，和双红线种类，行的文字颜色
            //行线颜色
            RowInforPara += _TableInfor.Rows[cell.RowIndex].RowLineColor.Name + "¤";
            RowInforPara += _TableInfor.Rows[cell.RowIndex].CustomLineType.ToString() + "¤";
            RowInforPara += _TableInfor.Rows[cell.RowIndex].RowForeColor + "¤";
            RowInforPara += _TableInfor.Rows[cell.RowIndex].CustomLineColor.Name;

            return retValue.ToString();
        }
        # endregion 获取表格中每个单元格的保存的文本

        # region 点击画布：根据位置，显示单元格
        /// <summary>点击画布，显示对应单元格的输入框
        /// 点击画布，根据鼠标位置显示单元格的输入框，如果不是单元格位置，那么将底板有焦点，以便可以拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxBackGround_MouseClick(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Middle)
            {
                return; //滚轮中键按下，不处理。防止滚轮滚动的时候按下，进入了其他单元格。
            }

            panelMain.Focus(); //使滚动条panel获取焦点，可以滚动滚动条

            _Need_Assistant_Control = null;

            if (_TableInfor == null) return;

            if (_IS_SHIFT_KEYDOWN)
            {
                return; //如果是按下Shift拖选，那么跳过
            }

            int fieldIndex = 0;

            //表格分栏
            for (int field = 0; field < _TableInfor.GroupColumnNum; field++)
            {
                fieldIndex = _TableInfor.RowsCount * field;

                for (int i = 0; i < _TableInfor.RowsCount; i++)
                {
                    for (int j = 0; j < _TableInfor.ColumnsCount; j++)
                    {
                        if (_TableInfor.Cells[i + fieldIndex, j].Rect.Contains(e.X, e.Y) && !_TableInfor.Cells[i + fieldIndex, j].IsMergedNotShow)
                        {
                            ShowCellRTBE(i + fieldIndex, j);

                            //如果右击，直接要显示菜单啊
                            if (e.Button == MouseButtons.Right)
                            {
                                if (_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed && _Need_Assistant_Control is RichTextBoxExtended)
                                {
                                    (_Need_Assistant_Control as RichTextBoxExtended).RightMouseDown(e);
                                }
                            }

                            //如果按下Ctrl 显示分组
                            if ((Control.ModifierKeys & Keys.Control) != 0)
                            {
                                //Cells_LeaveSaveData(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE, null); //触发光标离开事件

                                textBoxFormEditRow_Enter(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE, null);
                            }

                            return;
                        }
                    }
                }
            }

            //执行到这里，说明没有找到，即：表格区域外。这样如果还是点击了刚才那个单元格的输入框，就不会触发进入事件
            for (int col = 0; col < _TableInfor.ColumnsCount; col++)
            {
                if (_TableInfor.Columns[col].RTBE.Visible)
                {
                    //如果是纵向合并的单元格，会变成默认的总是居左
                    if (_TableInfor.Columns[col].RTBE.Is_RowSpanMutiline)
                    {
                        //如果是合并的多行的，设置了对齐方式，那么也会被重置。
                        if (!_TableInfor.Columns[col].RTBE._IsRtf) //如果是手动设置了对齐方式的rtf，不重置对齐方式
                        {
                            _TableInfor.Columns[col].RTBE.SelectAll();
                            _TableInfor.Columns[col].RTBE.SelectionAlignment = _TableInfor.Columns[col].RTBE._InputBoxAlignment;
                        }
                        //else
                        //{
                        //    HorizontalAlignment temp = _TableInfor.Columns[col].RTBE.SelectionAlignment;
                        //    _TableInfor.Columns[col].RTBE.SelectionAlignment = temp;
                        //}
                    }

                    _TableInfor.Columns[col].RTBE.Visible = false; //隐藏，触发更新
                }
            }
        }

        /// <summary>
        /// 标记选择的当前行，单元格所在行
        /// 选中单元格所在行的的突出显示（所在行突出显示、光标所在行突出显示）
        /// 选择行虚线边框显示
        /// </summary>
        int preRowIndex = -1;
        int preColIndex = -1;
        Rectangle _RectRow = Rectangle.Empty;
        Rectangle _RectCol = Rectangle.Empty;
        private void DrawRowSignShow(bool needReflash)
        {
            //如果拖动滚动条，会闪烁
            if (_TableType && _TableInfor != null)
            {
                if (_TableInfor.CurrentCell != null && _TableInfor.Columns[_TableInfor.CurrentCell.ColIndex].RTBE.Visible)
                {

                    Point p = this._TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].Loaction;

                    Point pcol = this._TableInfor.Cells[0, _TableInfor.CurrentCell.ColIndex].Loaction; //普通表格
                    //如果是分栏的表格
                    //考虑到分栏的情况，为了让显示效果更好
                    if (_TableInfor.GroupColumnNum > 1 && _TableInfor.CurrentCell.Field > 0)
                    {
                        //x = _TableInfor.Rows[_CurrentCellIndex.X - _TableInfor.CurrentCell.Field * _TableInfor.RowsCount].Loaction.X + 1 + _TableInfor.CurrentCell.Field * _TableInfor.PageRowsWidth;
                        //y = _TableInfor.Rows[_CurrentCellIndex.X - _TableInfor.CurrentCell.Field * _TableInfor.RowsCount].Loaction.Y + 3;

                        pcol = this._TableInfor.Cells[0 + _TableInfor.CurrentCell.Field * _TableInfor.RowsCount, _TableInfor.CurrentCell.ColIndex].Loaction; //支持分栏的多栏的表格和一栏的普通表格

                    }


                    //传统的一般的横向的表格
                    Size s = new Size(this._TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].RowSize.Width, this._TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].RowSize.Height);
                    Rectangle rect = new Rectangle(p, s);

                    //如果第一行合并的话，那么下面的行都是按照第一行合并的来处理了。CellSizeOrg
                    //Rectangle rectCol = new Rectangle(pcol, new Size(this._TableInfor.Cells[0, _TableInfor.CurrentCell.ColIndex].CellSize.Width, this._TableInfor.Rows[0].RowSize.Height * this._TableInfor.RowsCount));
                    Rectangle rectCol = new Rectangle(pcol, new Size(this._TableInfor.Cells[0, _TableInfor.CurrentCell.ColIndex].CellSizeOrg.Width, this._TableInfor.Rows[0].RowSize.Height * this._TableInfor.RowsCount));

                    if (_TableInfor.Vertical)
                    {

                        rectCol = new Rectangle(pcol, new Size(this._TableInfor.Cells[0, _TableInfor.CurrentCell.ColIndex].CellSize.Width * this._TableInfor.RowsCount, this._TableInfor.Rows[0].RowSize.Height));

                        //纵向表格
                        s = new Size(this._TableInfor.Cells[0, 0].CellSize.Width, this._TableInfor.Rows[0].RowSize.Height * this._TableInfor.ColumnsCount);
                        rect = new Rectangle(p, s);

                    }

                    this.pictureBoxBackGround.SuspendLayout();

                    ClearRowSignShow();

                    Graphics g = this.pictureBoxBackGround.CreateGraphics();
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    //Pen rowPen = new Pen(Color.LightGoldenrodYellow, 2);
                    /*Pen rowPen = new Pen(Color.LightYellow, 2); */ //Color.LightYellow
                    Pen rowPen = new Pen(ColorTranslator.FromHtml("#007ACC"));
                    Pen colPen = new Pen(Color.LightGoldenrodYellow, 2);
                    //rowPen.DashStyle = DashStyle.Dot;
                    colPen.DashStyle = DashStyle.Dot;

                    if (_TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].IsMerged() && !_TableInfor.Vertical)
                    {
                        if (_TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].ColSpan == 0)
                        {
                            //纵向合并
                            //纵向合并的
                            rect = new Rectangle(p.X, p.Y, this._TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].RowSize.Width,
                                this._TableInfor.Rows[_TableInfor.CurrentCell.RowIndex].RowSize.Height * (_TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].RowSpan + 1));
                        }
                        else
                        {
                            //水平合并列
                            int width = this._TableInfor.Cells[0, _TableInfor.CurrentCell.ColIndex].CellSizeOrg.Width;
                            for (int aa = 1; aa <= _TableInfor.Cells[_CurrentCellIndex.X, _CurrentCellIndex.Y].ColSpan; aa++)
                            {
                                width += this._TableInfor.Cells[0, _TableInfor.CurrentCell.ColIndex + aa].CellSizeOrg.Width;
                            }

                            rectCol = new Rectangle(pcol, new Size(width, this._TableInfor.Rows[0].RowSize.Height * this._TableInfor.RowsCount));
                        }
                    }
                    //else
                    //{
                    //    //这样的话，会导致虚线和单元格重叠
                    //    g.DrawRectangle(rowPen, rect);      //行
                    //    g.DrawRectangle(colPen, rectCol);   //列
                    //}

                    g.DrawRectangle(rowPen, rect);      //行
                    //g.DrawRectangle(colPen, rectCol);   //列


                    //ReSetCellBackGround(_TableInfor.Columns[_CurrentCellIndex.X].RTBE);//电盘重置，防止虚线干扰
                    //g.FillRectangle(Brushes.White, _TableInfor.CurrentCell.Loaction.X, _TableInfor.CurrentCell.Loaction.Y,
                    //    _TableInfor.CurrentCell.CellSize.Width, _TableInfor.CurrentCell.CellSize.Height);

                    //行背景色
                    if (_SelectRowBackColor != Color.Empty)
                    {
                        //单元格内容不能绘制，不会选择单元格个状态显示有异常
                        //g.FillRectangle(bBgWhite, _TableInfor.CurrentCell.Rect);
                        //g.FillRectangle(bBg, rect);
                        //g.FillRectangle(bBg, rectCol);

                        //透明背景System.Drawing.Color.FromArgb(50, Color.SkyBlue)
                        Brush bBg = new SolidBrush(Color.FromArgb(80, _SelectRowBackColor));
                        //Brush bBgWhite = new SolidBrush(Color.White); 

                        Region rg = new Region(rect);
                        rg.Exclude(new Region(rectCol)); //不相交的部分

                        Region rg2 = new Region(rectCol);
                        rg2.Exclude(new Region(rect)); //不相交的部分

                        #region 如果是合并的单元格，那么要出去掉合并的单元格。这样选中行的颜色突出才更合理
                        //int i = _CurrentCellIndex.X;
                        //int j = _CurrentCellIndex.Y;
                        //if (_TableInfor.Cells[i, j].IsMerged() && !_TableInfor.Vertical)
                        //{
                        //    Point pcolMerge = Point.Empty;
                        //    Rectangle rectColMerge = Rectangle.Empty;

                        //    Point pcolMergedRow = Point.Empty;
                        //    Rectangle rectColMergedRow = Rectangle.Empty;

                        //    if (_TableInfor.Cells[i, j].ColSpan == 0) //ColSpan不为0就表示横向合并的，为0那就是纵向合并的了
                        //    {
                        //        //纵向合并的
                        //        for (int aa = 1; aa <= _TableInfor.Cells[i, j].RowSpan; aa++)
                        //        {

                        //            ////合并行，都设置背景色比较合适
                        //            //pcolMergedRow = this._TableInfor.Cells[0, _TableInfor.CurrentCell.ColIndex + aa].Loaction; //普通表格
                        //            //rectColMergedRow = new Rectangle(pcolMergedRow, new Size(this._TableInfor.Cells[0, _TableInfor.CurrentCell.ColIndex + aa].CellSizeOrg.Width, this._TableInfor.Rows[i].RowSize.Height * this._TableInfor.RowsCount));
                        //            //rg2.Union(rectColMergedRow); 

                        //            //合并单元格，整个格子为白色，选中状态的效果样式最合适
                        //            pcolMerge = this._TableInfor.Cells[i + aa, _TableInfor.CurrentCell.ColIndex].Loaction; //普通表格
                        //            rectColMerge = new Rectangle(pcolMerge, new Size(this._TableInfor.Cells[i + aa, _TableInfor.CurrentCell.ColIndex].CellSizeOrg.Width, this._TableInfor.Rows[i].RowSize.Height));
                        //            rg2.Exclude(new Region(rectColMerge)); //不相交的部分（除去相交部分的区域）
                        //        }
                        //    }
                        //    else
                        //    {
                        //        //横向合并的
                        //        for (int aa = 1; aa <= _TableInfor.Cells[i, j].ColSpan; aa++)
                        //        {
                        //            ////合并行，都设置背景色比较合适
                        //            //pcolMergedRow = this._TableInfor.Cells[0, _TableInfor.CurrentCell.ColIndex + aa].Loaction; //普通表格
                        //            //rectColMergedRow = new Rectangle(pcolMergedRow, new Size(this._TableInfor.Cells[0, _TableInfor.CurrentCell.ColIndex + aa].CellSizeOrg.Width, this._TableInfor.Rows[i].RowSize.Height * this._TableInfor.RowsCount));
                        //            //rg.Union(rectColMergedRow); 


                        //            pcolMerge = this._TableInfor.Cells[i, _TableInfor.CurrentCell.ColIndex + aa].Loaction; //普通表格
                        //            rectColMerge = new Rectangle(pcolMerge, new Size(this._TableInfor.Cells[i, _TableInfor.CurrentCell.ColIndex + aa].CellSizeOrg.Width, this._TableInfor.Rows[i].RowSize.Height));
                        //            rg.Exclude(new Region(rectColMerge)); //不相交的部分
                        //        }
                        //    }
                        //}
                        #endregion 如果是合并的单元格，那么要出去掉合并的单元格。这样选中行的颜色突出才更合理

                        //rg.Intersect(rg2);//交集
                        rg.Union(rg2); //并集  .Xor( 并集，且 除去交集

                        g.FillRegion(bBg, rg);

                        bBg.Dispose();

                        //////绘制了虚线框，会挡住了输入框。所以刷新一下，这样就没有遮挡了。但是水平合并的又会第一次是空白。
                        //if (_TableInfor.Cells[i, j].IsMerged())  //&& !_TableInfor.Vertical
                        //{
                        //    //_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Focus();
                        //    _TableInfor.Columns[j].RTBE.Refresh();//会导致打开第一个单击的时候，空白
                        //    //_TableInfor.Columns[j].RTBE.Focus();
                        //    //ShowCellRTBE(i, j);
                        //}
                    }

                    _RectRow = rect;
                    _RectCol = rectCol;

                    preRowIndex = _TableInfor.CurrentCell.RowIndex;
                    preColIndex = _TableInfor.CurrentCell.ColIndex;

                    //释放绘图对象
                    rowPen.Dispose();
                    //sb.Dispose();
                    g.Dispose();

                    this.pictureBoxBackGround.ResumeLayout();

                }
            }
        }

        /// <summary>
        /// 清空选择行的标记
        /// </summary>
        private void ClearRowSignShow()
        {
            //取消上次显示的突出单元格
            if (preRowIndex != -1)
            {
                //没有解决闪烁的问题，反而导致，拖动滚动条后，单元格内容在多次绘制背景后背挡住，看不到了。
                //if (_RectRow != Rectangle.Empty)
                //{
                //    //Region r;

                //    ////rf.Inflate(100f, 100f);//扩大区域，结构放大
                //    //_RectRow.Inflate(2, 2);

                //    //r = new Region(_RectRow);
                //    //this.pictureBoxBackGround.Invalidate(r);

                //    //_RectCol.Inflate(2, 2);

                //    //r = new Region(_RectCol);

                //    Region rg = new Region(_RectRow);
                //    rg.Exclude(new Region(_RectCol)); //不相交的部分

                //    Region rg2 = new Region(_RectCol);
                //    rg2.Exclude(new Region(_RectRow)); //不相交的部分

                //    //rg.Intersect(rg2);//交集
                //    rg.Union(rg2); //并集

                //    this.pictureBoxBackGround.Invalidate(rg);

                //    this.pictureBoxBackGround.Update();
                //}
                //else
                //{
                this.pictureBoxBackGround.Refresh();
                //}
            }
            else
            {
                //如果Ctrl + 1的手绘开启后，不选择一行后，下次再画时，之前的不会消失
                if (_DragDrawRectMode == 1)
                {
                    this.pictureBoxBackGround.Refresh();
                }
            }
        }

        # endregion 点击画布：根据位置，显示单元格

        #region 触发显示行编辑界面
        Form HK;
        private void textBoxFormEditRow_Enter(object sender, EventArgs e)
        {

            RichTextBoxExtended thisCl = (RichTextBoxExtended)sender;
            //ShowEditRowCell(thisCl.Location);//显示手指位置
            //thisCl.SelectAll(); //突出显示

            //if ((Control.ModifierKeys & Keys.Alt) != 0)
            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                //ShowEditRowCell(thisCl.Location);//显示手指位置，如果不是分组编辑的，那么不需要提示

                if (HK == null || HK.IsDisposed)
                {
                    string currentCellGroupName = _TableInfor.Columns[_CurrentCellIndex.Y].GroupName;

                    if (!string.IsNullOrEmpty(currentCellGroupName))
                    {
                        ShowEditRowCell(thisCl.Location);//显示手指位置，如果不是分组编辑的，那么不需要提示

                        HK = new FormEditRow();

                        // //点击选项右击菜单，界面会关闭
                        ((FormEditRow)HK).TextBox = thisCl; //会实时关闭，而且显示的时候，光标显示在指定位置
                        //((FormEditRow)HK).TextBox = this.pictureBoxBackGround;

                        List<string> listGroup = new List<string>();
                        for (int i = 0; i < _TableInfor.Columns.Length; i++)
                        {
                            if (_TableInfor.Columns[i].GroupName == currentCellGroupName)
                            {
                                listGroup.Add(_TableInfor.Columns[i].ColName);
                            }
                        }

                        //根据行列，取好数据，传入给分组编辑界面
                        string[] itemsHeaderArr = listGroup.ToArray();                  //显示名
                        string[] itemsColumnArr = itemsHeaderArr;                       //列名
                        string[] itemsValueArr = new string[itemsHeaderArr.Length];     //该行的对应列数据
                        //XmlNode nodeTable = null

                        string strKeyDate = ""; //一般取“日期”“时间” ，如果没有日期时间，取第一列

                        //根据当前行位置，得到行最近的日期时间
                        XmlNode records = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records)));

                        XmlNode thisNode = records.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), _TableInfor.Rows[_CurrentCellIndex.X].ID.ToString(), "=", nameof(EntXmlModel.Record)));
                        //string dt = GetRowDateTime((thisNode as XmlElement).GetAttribute(nameof(EntXmlModel.ID)));

                        if (_TableInfor.CellByRowNo_ColName(0, "日期") != null && _TableInfor.CellByRowNo_ColName(0, "时间") != null)
                        {
                            strKeyDate = GetRowDateTime(_TableInfor.Rows[_CurrentCellIndex.X].ID.ToString()).Replace("¤", " ");
                        }
                        else
                        {
                            if (_TableInfor.CellByRowNo_ColName(0, "日期") != null)
                            {
                                strKeyDate = (thisNode as XmlElement).GetAttribute("日期").Split('¤')[0];
                            }
                            else if (_TableInfor.CellByRowNo_ColName(0, "时间") != null)
                            {
                                strKeyDate = (thisNode as XmlElement).GetAttribute("时间").Split('¤')[0];
                            }
                            else
                            {
                                //默认取第一列的
                                if (_TableInfor.Columns[0].ColName != "循环系统T")
                                {
                                    strKeyDate = (thisNode as XmlElement).GetAttribute(_TableInfor.Columns[0].ColName).Split('¤')[0];
                                }
                                else
                                {
                                    strKeyDate = "";
                                }
                            }
                        }

                        //Cells_LeaveSaveData(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE, null); //触发光标离开事件
                        //for (int col = 0; col < _TableInfor.ColumnsCount; col++)
                        //{
                        //    if (_TableInfor.Columns[col].RTBE.Visible)
                        //    {
                        //        //如果是纵向合并的单元格，会变成默认的总是居左
                        //        if (_TableInfor.Columns[col].RTBE.Is_RowSpanMutiline)
                        //        {
                        //            _TableInfor.Columns[col].RTBE.SelectAll();
                        //            _TableInfor.Columns[col].RTBE.SelectionAlignment = _TableInfor.Columns[col].RTBE._InputBoxAlignment;
                        //        }
                        //        _TableInfor.Columns[col].RTBE.Visible = false; //隐藏，触发更新
                        //    }
                        //}

                        //分组的各列数据
                        for (int i = 0; i < itemsHeaderArr.Length; i++)
                        {
                            if (itemsColumnArr[i] == _TableInfor.Columns[_CurrentCellIndex.Y].ColName)
                            {
                                //如果是当前编辑单元格，可能光标置入的时候进行了合计或者默认值。所以取控件的值，因为xml数据中还没有保存
                                itemsValueArr[i] = _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Text.Trim();
                            }
                            else
                            {
                                itemsValueArr[i] = (thisNode as XmlElement).GetAttribute(itemsColumnArr[i]).Split('¤')[0];
                            }
                        }

                        //如果存在分组
                        if (listGroup.Count > 0)
                        {
                            //((FormEditRow)HK).InitTableItems("2017-01-21 13:30", "循环系统分组", new string[] { "列名显示1", "列名显示2", "列名显示3", "列名显示4", "列名显示5" }, new string[] { "列名1", "列名2", "列名3", "列名4", "列名5" }, new string[] { "AAA", "BCD", "OOOXXX", "BCD", "OOOXXX" });
                            ((FormEditRow)HK).InitTableItems(_node_Page.SelectSingleNode(nameof(EntXmlModel.Table)), strKeyDate, currentCellGroupName, itemsHeaderArr, itemsColumnArr, itemsValueArr);
                            ((FormEditRow)HK)._CallBackEvent = FormEditRow_CallBack;
                            ((FormEditRow)HK)._CallBackEnterEvent = FormEditRowEnter_CallBack;

                            //HK.Location = this.PointToScreen(new Point(thisCl.Left + 23, thisCl.Bottom)); //设置位置
                            HK.Location = thisCl.Parent.PointToScreen(new Point(thisCl.Left, thisCl.Bottom));
                            HK.FormClosed += new FormClosedEventHandler(HK_FormClosed);
                            //HK.Show();
                            HK.ShowDialog(); //点击选项右击菜单，界面会关闭

                            //先时候，表单元格格已经丢失，输入框不存在丢失了


                            ////防止关闭后，清除右击菜单
                            //this.pictureBoxBackGround.Focus();
                        }
                    }
                }
            }
        }


        private void FormEditRowEnter_CallBack(object sender, EventArgs e)
        {
            RichTextBox rtbe = (RichTextBox)sender;
            CellInfor thisCell = _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, rtbe.Name);
            if (thisCell != null)
            {
                if (_CurrentCellIndex.Y != thisCell.ColIndex) // || !_TableInfor.Columns[thisCell.ColIndex].RTBE.Visible || _TableInfor.CurrentCell == null
                {

                    Comm._SettingRowForeColor = true;
                    ShowCellRTBE(thisCell.RowIndex, thisCell.ColIndex);
                    Comm._SettingRowForeColor = false; //防止弹出右击菜单等，但是合计的话到了列组编辑将失效

                    //_TableInfor.Columns[thisCell.ColIndex].RTBE.Focus();
                }

                ShowEditRowCell(_TableInfor.Columns[thisCell.ColIndex].RTBE.Location);//显示手指位置
                DrawRowSignShow(true); //防止阴影、残留UI
            }
        }

        /// <summary>
        /// 修改后直接更新到界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormEditRow_CallBack(object sender, EventArgs e)
        {
            //修改后，返回修改单元格
            RichTextBox rtbe = (RichTextBox)sender;
            CellInfor thisCell = _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, rtbe.Name);
            if (thisCell != null)
            {
                //Cells_LeaveSaveData(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE, null); //触发光标离开事件，进行保存 数据

                ////int preIndexY = _CurrentCellIndex.Y;
                if (_CurrentCellIndex.Y != thisCell.ColIndex)   //|| !_TableInfor.Columns[thisCell.ColIndex].RTBE.Visible || _TableInfor.CurrentCell == null
                {
                    Comm._SettingRowForeColor = true;
                    ShowCellRTBE(thisCell.RowIndex, thisCell.ColIndex);
                    Comm._SettingRowForeColor = false; //防止弹出右击菜单等，但是合计的话到了列组编辑将失效
                }

                //ShowCellRTBE(thisCell.RowIndex, thisCell.ColIndex);

                //_TableInfor.Columns[thisCell.ColIndex].RTBE.Focus();

                HorizontalAlignment temp = _TableInfor.Columns[thisCell.ColIndex].RTBE.SelectionAlignment;
                //将合计的结果显示到合计输入框上
                _TableInfor.Columns[thisCell.ColIndex].RTBE.Text = rtbe.Text.Trim();
                _TableInfor.Columns[thisCell.ColIndex].RTBE.SelectionAlignment = temp;

                _TableInfor.Columns[thisCell.ColIndex].RTBE.Select(_TableInfor.Columns[thisCell.ColIndex].RTBE.Text.Length, 0);

                Cells_LeaveSaveData(_TableInfor.Columns[thisCell.ColIndex].RTBE, null); //触发光标离开事件，进行保存 数据

                //if (preIndexY != thisCell.ColIndex)
                //{
                //保存一下，否则再点击单元格来回切换数据会串
                //Cells_LeaveSaveData(_TableInfor.Columns[thisCell.ColIndex].RTBE, null); //触发光标离开事件，进行保存 数据
                //}
            }
            else
            {
                string errMsg = "行组编辑，更新失败。";
                Comm.LogHelp.WriteErr(errMsg);
                MessageBox.Show(errMsg);
            }

            //RichTextBox rtbe = (RichTextBox)sender;
            //CellInfor thisCell = _TableInfor.CellByRowNo_ColName(_CurrentCellIndex.X, rtbe.Name);
            //if (thisCell != null)
            //{
            //    if (_CurrentCellIndex.Y != thisCell.ColIndex)
            //    {
            //        ShowCellRTBE(thisCell.RowIndex, thisCell.ColIndex);
            //    }

            //    //HorizontalAlignment temp = _TableInfor.Columns[thisCell.ColIndex].RTBE.SelectionAlignment;
            //    ////将合计的结果显示到合计输入框上
            //    //_TableInfor.Columns[thisCell.ColIndex].RTBE.Text = rtbe.Text.Trim();
            //    //_TableInfor.Columns[thisCell.ColIndex].RTBE.SelectionAlignment = temp;
            //}
        }

        //BorderColorLabel lblShowEditRowCell = null;
        TransparentLabel lblShowEditRowCell = null;
        private void ShowEditRowCell(Point clPoint)
        {
            Point showLocation = clPoint;

            ////指示手指的位置
            //if (this.lblShowEditRowCell == null || this.lblShowEditRowCell.IsDisposed)
            //{
            //    //保存，打印前，会删除掉该控件
            //    this.lblShowEditRowCell = new Label();
            //    //this.lblShowEditRowCell.BackColor = Color.Transparent;
            //    this.lblShowEditRowCell.Text = "☞";
            //    //this.lblShowEditRowCell.Caption = "☞";
            //    this.lblShowEditRowCell.Size = new Size(18, 12);
            //    //this.lblShowEditRowCell.DimmedColor = System.Drawing.Color.Red;
            //    this.lblShowEditRowCell.ForeColor = System.Drawing.Color.Red;
            //    this.lblShowEditRowCell.Name = "lblShowEditRowCell";
            //    //this.lblShowEditRowCell.Opacity = 100;
            //    //this.lblShowEditRowCell.Radius = 1;
            //    this.lblShowEditRowCell.Click += new System.EventHandler(this.lblShowEditRowCell_Click);
            //}

            if (this.lblShowEditRowCell == null || this.lblShowEditRowCell.IsDisposed)
            {
                //保存，打印前，会删除掉该控件
                this.lblShowEditRowCell = new TransparentLabel();
                //if (_SelectRowBackColor != Color.Empty)
                //{
                //    this.lblShowEditRowCell.BackColor = _SelectRowBackColor;
                //}
                //else
                //{
                this.lblShowEditRowCell.BackColor = System.Drawing.Color.Transparent; //this.pictureBoxBackGround.BackColor;
                //this.lblShowEditRowCell.Parent = this.pictureBoxBackGround;
                //}
                //this.lblShowEditRowCell.BackgroundImage = new Bitmap(lblShowEditRowCell.Width, lblShowEditRowCell.Height);
                //Graphics g = Graphics.FromImage(lblShowEditRowCell.BackgroundImage);
                ////请注意这个透明并不是真正的透明，而是用父控件的当前位置的颜色填充PictureBox内的相应位置的颜色。 
                //g.Clear(Color.Transparent);
                //g.Dispose();


                //this.lblShowEditRowCell.DimmedColor = System.Drawing.Color.Red;
                //this.lblShowEditRowCell.ForeColor = System.Drawing.Color.Red;
                this.lblShowEditRowCell.Name = "lblShowEditRowCell";
                //this.lblShowEditRowCell.Opacity = 100;
                //this.lblShowEditRowCell.Radius = 1;
                this.lblShowEditRowCell.Click += new System.EventHandler(this.lblShowEditRowCell_Click);
            }

            this.lblShowEditRowCell.Size = new Size(22, 22);
            this.lblShowEditRowCell.ForeColor = System.Drawing.Color.OrangeRed;

            //this.lblShowEditRowCell_Click.BackgroundImage = new Bitmap(lblShowSearch.Width, lblShowSearch.Height);
            //Graphics g = Graphics.FromImage(lblShowSearch.BackgroundImage);
            ////请注意这个透明并不是真正的透明，而是用父控件的当前位置的颜色填充PictureBox内的相应位置的颜色。 
            //g.Clear(Color.Transparent); //g.Clear(Color.White); //g.Clear(Color.Transparent);
            //g.Dispose();
            this.lblShowEditRowCell.Font = new Font("宋体", 10, FontStyle.Bold);
            this.lblShowEditRowCell.Draw();
            this.lblShowEditRowCell.BackColor = System.Drawing.Color.Transparent; //this.pictureBoxBackGround.BackColor;



            if (!this.pictureBoxBackGround.Controls.Contains(this.lblShowEditRowCell))
            {
                this.pictureBoxBackGround.Controls.Add(this.lblShowEditRowCell);
            }

            this.lblShowEditRowCell.BackColor = this.pictureBoxBackGround.BackColor;

            lblShowEditRowCell.Visible = true;
            lblShowEditRowCell.BringToFront();
            lblShowEditRowCell.Focus();

            lblShowEditRowCell.Location = new Point(showLocation.X - lblShowSearch.Size.Width, showLocation.Y);
            lblShowEditRowCell.Refresh();
        }

        private void lblShowEditRowCell_Click(object sender, EventArgs e)
        {
            if (this.pictureBoxBackGround.Controls.Contains(this.lblShowEditRowCell))
            {
                this.pictureBoxBackGround.Controls.Remove(this.lblShowEditRowCell);
            }

            lblShowEditRowCell.Dispose();
            DrawRowSignShow(true);
            //this.pictureBoxBackGround.Refresh();
            //lblShowEditRowCell.Hide();
        }

        void HK_FormClosed(object sender, FormClosedEventArgs e)
        {

            //HK.Dispose();            //防止关闭时，界面闪烁一次
            HK = null;

            //DrawRowSignShow();

            if (_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Visible)
            {
                //防止关闭后，清除右击菜单
                //_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.
                //Cells_LeaveSaveData(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE, null); //触发光标离开事件，进行保存 数据
                _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Visible = false;
                //ShowCellRTBE(_CurrentCellIndex.X, _CurrentCellIndex.Y);
            }

            ShowCellRTBE(_CurrentCellIndex.X, _CurrentCellIndex.Y);
            //_TableInfor.CurrentCell = null;
            ////_Need_Assistant_Control = null;

            if (this.lblShowEditRowCell != null && !this.lblShowEditRowCell.IsDisposed)
            {
                //lblShowEditRowCell.Focus();
                //lblShowEditRowCell.Dispose();

                lblShowEditRowCell.Hide();

                if (this.pictureBoxBackGround.Controls.Contains(this.lblShowEditRowCell))
                {
                    this.pictureBoxBackGround.Controls.Remove(this.lblShowEditRowCell);
                    //this.pictureBoxBackGround.Refresh();
                }

                DrawRowSignShow(true);
                //lblShowEditRowCell.Dispose();
            }
        }

        private void rtbeNewTransparentEditRow_Click(object sender, EventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                //if (textBox1.Focused)
                //{
                //    textBox1.Focus();
                //}
                //else
                //{
                //Cells_LeaveSaveData(_TableInfor.Columns[_CurrentCellIndex.Y].RTBE, null); //触发光标离开事件，进行保存 数据
                textBoxFormEditRow_Enter(sender, e);
                //}
            }
        }
        #endregion 触发显示行编辑界面

        #region 单元格光标进入，设置默认值
        /// <summary>
        /// 光标进入单元格，设置默认值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool Cells_EnterSetDefaulyValue(CellInfor cellInfor)
        {
            bool isSet = false;

            //没有权限，不能修改的输入框，不进行赋值
            if (_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.ReadOnly)
            {
                return isSet;
            }

            if (cellInfor.Text.Trim() == "")
            {
                switch (cellInfor.CellName)
                {
                    case "签名":
                        //isSet = true;
                        break;

                    case "日期":
                        //单击，有的时候容易误点就生成了默认值
                        if (_TableInfor.Columns[_CurrentCellIndex.Y].RTBE._YMD_Default.Contains("yyyy"))
                        {
                            cellInfor.Text = string.Format("{0:" + _TableInfor.Columns[_CurrentCellIndex.Y].RTBE._YMD_Default + "}", GetServerDate());
                        }
                        else
                        {
                            cellInfor.Text = string.Format("{0:yyyy-MM-dd}", GetServerDate());
                        }

                        isSet = true;

                        break;

                    case "时间":
                        //单击，有的时候容易误点就生成了默认值
                        cellInfor.Text = string.Format("{0:" + DateFormat_HM + "}", GetServerDate());
                        isSet = true;

                        break;

                    default:

                        if (cellInfor.CellName.Contains("日期时间"))
                        {

                            if (_TableInfor.Columns[_CurrentCellIndex.Y].RTBE._YMD_Default != "")
                            {
                                cellInfor.Text = string.Format("{0:" + _TableInfor.Columns[_CurrentCellIndex.Y].RTBE._YMD_Default + "}", GetServerDate());
                            }
                            else
                            {
                                //暂时不设置，因为验证的时候，也是根据表达式来验证的
                                //cellInfor.Text = string.Format("{0:yyyy-MM-dd " + DateFormat_HM + "}", GetServerDate()); //{0:yyyy-MM-dd HH:mm}
                            }

                            isSet = true;
                        }
                        else if (cellInfor.CellName.StartsWith("日期"))
                        {
                            //cellInfor.Text = string.Format("{0:yyyy-MM-dd}", GetServerDate()); //{0:yyyy-MM-dd HH:mm}

                            if (_TableInfor.Columns[_CurrentCellIndex.Y].RTBE._YMD_Default.Contains("yyyy"))
                            {
                                cellInfor.Text = string.Format("{0:" + _TableInfor.Columns[_CurrentCellIndex.Y].RTBE._YMD_Default + "}", GetServerDate());
                            }
                            else
                            {
                                cellInfor.Text = string.Format("{0:yyyy-MM-dd}", GetServerDate());
                            }

                            isSet = true;
                        }
                        else if (cellInfor.CellName.StartsWith("时间"))
                        {
                            cellInfor.Text = string.Format("{0:" + DateFormat_HM + "}", GetServerDate());
                            isSet = true;
                        }
                        else if (_TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Default != "") //光标进入默认值
                        {
                            //单元格默认值
                            cellInfor.Text = _TableInfor.Columns[_CurrentCellIndex.Y].RTBE.Default;
                            isSet = true;
                        }
                        else
                        {
                            isSet = false;
                        }

                        break;

                }
            }

            return isSet;
        }
        # endregion 光标进入，设置默认值
    }
}
