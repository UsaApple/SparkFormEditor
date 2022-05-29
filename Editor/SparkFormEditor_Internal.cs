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
using SparkFormEditor.Foundation;

namespace SparkFormEditor
{
    public partial class SparkFormEditor
    {
        /// <summary>
        /// 新建页
        /// </summary>
        /// <returns></returns>
        internal int AddPageRecruit()
        {
            this.treeViewPatients.SelectedNode = _CurrentTemplateNameTreeNode;

            toolStripAddNextPage_Click(null, null);

            return this._CurrentTemplateNameTreeNode.Nodes.Count;
        }

        /// <summary>双击病人列表节点
        /// 加载模板样式和数据
        /// 打开病人的表单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void treeViewPatients_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //右击双击的话，刷新数据再打开。或者继续打开当前页就刷新
            bool reflashdata = false;//true强行刷数据

            try
            {
                _GoOnPrintLocation = new Point(-1, -1); //续打的位置进行重置

                bool isMsgShowed = false;

                //为防止切换太快，上次的加减锁的线程还在处理，导致混乱
                while (_IsThreadUpdatePatientsLisDTInfor) // 锁定表单和更新基本信息的线程标记
                {
                    if (!isMsgShowed)
                    {
                        ShowInforMsg("切换表单过快，等稍候再操作……");
                        isMsgShowed = true;
                    }

                    System.Threading.Thread.Sleep(20);
                }

                this.Cursor = Cursors.WaitCursor;

                _IsLoading = true;
                _IsNotNeedCellsUpdate = true;


                TreeNode tn = null;

                if (sender is TreeView)
                {
                    //病人列表打开的时候
                    tn = ((TreeView)sender).SelectedNode;

                    //肯定不是创建表单或者新建页，重置标记位：防止更新基本信息的提示不显示
                    if (_IsCreating)
                    {
                        _IsCreating = false;
                    }
                }
                else if (sender is TreeNode)
                {
                    //创建的时候，暗门导入的时候 ：
                    //sender为null，调用该方法之前，先处理好模板名和页数
                    //tn = AdddPageNode(); //AdddTemplateNode(;同时添加第一页
                    tn = (TreeNode)sender;
                }
                else
                {
                    //直接打开指定页，sender is string
                    //tn = this.treeViewPatients.Nodes
                    int openPage = 1;
                    if (sender == null || string.IsNullOrEmpty(sender.ToString()))
                    {
                        //如果是空，那么表示默认打开最后一页。但是创建的连续n页的时候，要打开第一页比较符合实际需要
                        if (_IsCreatedTemplate)
                        {
                            //新建表单的时候，如果是连续创建n页，那么打开第一页比较合适。
                            openPage = 1;
                            this.uc_Pager1.PageCurrent = openPage;
                            this.uc_Pager1.PageCount = openPage;
                            this.uc_Pager1.Bind();
                        }
                        else
                        {
                            //openPage = this.treeViewPatients.Nodes[0].Nodes[0].Nodes.Count;

                            //重新计算页数：（更加严谨，但是好像也没有必要）
                            #region 重新计算页数：--------
                            int pages = 0;
                            int a = _RecruitXML.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form))).Count;              // 非表格的页数
                            int b = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))).ChildNodes.Count; // 表格的行数

                            openPage = a;
                            this.uc_Pager1.PageCurrent = openPage;
                            this.uc_Pager1.PageCount = a;
                            this.uc_Pager1.Bind();

                            XmlNode xn = _TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form), nameof(EntXmlModel.Table)));

                            if (xn != null)
                            {
                                pages = TableClass.GetPageCount_HaveTable(_TemplateXml, b);
                            }
                            else
                            {
                                pages = a;
                            }

                            if (openPage < pages)
                            {
                                Comm.LogHelp.WriteErr("总页数和计算的页数不一致：" + this.treeViewPatients.Nodes[0].Text);
                            }
                            #endregion 重新计算页数：--------
                        }
                    }
                    else
                    {
                        if (int.TryParse(sender.ToString(), out openPage))
                        {
                            //
                        }
                        else
                        {
                            openPage = 1;
                        }
                    }

                    if (this.treeViewPatients.Nodes.Count == 1)
                    {
                        //if (this.treeViewPatients.Nodes[0].Nodes.Count < openPage)
                        //{
                        //    //补充节点
                        //}

                        //删除页后，必须更新主窗体的页码。都会走到该方法的。所以不用担心索引超出范围。
                        //tn = this.treeViewPatients.Nodes[0].Nodes[0].Nodes[openPage - 1];
                        tn = this.treeViewPatients.Nodes[0].Nodes[0].Nodes[0];
                    }
                    else
                    {
                        string noPatientMsg = "病人列表中没有数据。";
                        Comm.LogHelp.WriteErr(noPatientMsg);
                        MessageBox.Show(noPatientMsg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }
                }

                //if (tn.Tag.ToString() == "3") //如果为页数，才需要打开该表单，否则需要先新建表单后默认有第一页
                #region
                //if (tn.Level == 2) //如果为页数，才需要打开该表单，否则需要先新建表单后默认有第一页
                //{

                if (!this.pictureBoxBackGround.Enabled) // && this.toolStripSaveEnabled新建后又取消，双后在双击，这是填补的空白行没有了，在输入就报为异常:索引的单元格，找不到它所对应的XML节点
                {
                    this.pictureBoxBackGround.Enabled = true;
                }

                _PreCellRowIndex = new Point(-1, -1);

                preRowIndex = -1;
                preColIndex = -1;

                //间距变量
                _SpaceTarget = new Point(-1, -1);
                _Space = 0;

                this.pictureBoxBackGround.Focus();

                //防止光标在单元格内修改数据后，立刻重复打开本页，该输入框会显示不空，光标置入为空，因为没有刷新
                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;

                //打开界面后后第一次操作，不需要
                if (_TemplateName != "")
                {
                    SaveToOp();
                }

                _IsLoading = true;
                _IsNotNeedCellsUpdate = true;

                if (!_NotNeedCheckForSave)
                {
                    //如果需要保存，提示是否需要先保存，再执行后续操作，如果硬要取消，那么重置
                    CheckForSave();

                    _IsLoading = true; //上面提醒保存的时候，可能又变成false了。比如：保存时自动签名
                }


                //防止调整段落添加页后，直接打开下页，但又取消保存的情况下，新加的页节点又不删除，会报错
                if (tn.Parent == null || tn.Parent.Parent == null) //新建页，新建模板
                {
                    this.Cursor = Cursors.Default;
                    _IsLoading = false;
                    _IsNotNeedCellsUpdate = false;

                    IsNeedSave = false;
                    _IsCreated = false;
                    _IsCreatedTemplate = false;

                    ClearCurrentTemplate(); //清除当前表单的设置标记

                    this.pictureBoxBackGround.Enabled = false; //设置为无效，防止继续操作。需要切换表单或者关闭才能重新打开
                    toolStripSaveEnabled = false;
                    //this.ToolStripMenuItemSave.Enabled = false;
                    //this.ToolStripMenuItemDeletePage.Enabled = false;

                    string msg001 = "新建页在没保存的情况下继续打开本页，又取消提示的保存；属于异常操作，请重新打开表单后再操作。";
                    ShowInforMsg(msg001, false);

                    if (CallBackUpdateDocumentPaging != null)
                    {
                        if (_CurrentPage != "") //这样的话，会和表单一样弹出消息提示：新建页后没保存的消息。但是页码还是老的
                        {
                            //0当前页码，1当前页码
                            int[] arr = new int[] { _CurrentTemplateNameTreeNode.Nodes.Count, int.Parse(_CurrentPage) }; //_CurrentPage为空异常
                            CallBackUpdateDocumentPaging(arr, null);
                        }
                        else
                        {
                            //小结产生新页，点击新页，提示是否保存，如果选择取消，那么页码是增加后的页码，但是数据还是前一页的。
                            int[] arr = new int[] { _CurrentTemplateNameTreeNode.Nodes.Count, _CurrentTemplateNameTreeNode.Nodes.Count }; //_CurrentPage为空异常
                            CallBackUpdateDocumentPaging(arr, null);
                        }
                    }

                    MessageBox.Show(msg001, "提示消息", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                _CurrentPatientTreeNode = tn.Parent.Parent; //记录下目前节点
                _CurrentTemplateNameTreeNode = tn.Parent;

                _ListDoubleLineInputBox.Clear();    //清空双红线输入框集合
                _ErrorList.Clear();                 //清空错误内容




                _IsNotNeedReloadData = false;

                //记录下，当前打开页的信息：病人，模板名称，页数，并要判断：
                //如果只有第一页的配置（各业一样）、和前一次打开的页数都一样，那么就不需要加载了，只要界面控件的值设置为Defalut；
                _IsNotNeedReloadTemplate = false;//先做第一条件判断：同一病人，统一模板名称：当前的uhid和表单名称和上一次的进行比较
                if (this._patientInfo.PatientId == ((DataRow)tn.Parent.Parent.Tag)["UHID"].ToString() && _TemplateName == tn.Parent.Text)
                {
                    _IsNotNeedReloadTemplate = true; //先判断病人名字和表单，如果不一样，肯定需要重新加载；下面再判断页ID

                    _IsNotNeedReloadData = true;

                    //手动双击，当前页，那么也要刷新数据。
                    if (e != null && tn != null && _PreShowedPage == tn.Text)
                    {
                        reflashdata = true;
                    }
                }
                else
                {
                    //如果调整段落列的时候，跨页也清空就不能保存了
                    _SynchronizeTemperatureRecordListRecordInor.Clear();            //如果跨页，肯定是调整段落列等情况，那么不保存。体温等肯定是输入的。

                    _IsNotNeedReloadTemplate = false;

                    IsNeedSave = false;
                    _IsCreated = false;

                    if (!_IsCreating) //创建新表单的时候，这里不要清空标记，因为更新基本信息后，要生成自动创建N页的每页的默认数据的 AutoCreatPagesCount
                    {
                        _IsCreatedTemplate = false;
                    }
                }

                //_CurrentPage = tn.Text;
                _CurrentPage = this.uc_Pager1.PageCurrent.ToString();
                //this._patientInfo.Id = ((DataRow)tn.Parent.Parent.Tag)["UHID"].ToString();
                //this._patientInfo.Id = ((DataRow)tn.Parent.Parent.Tag)["HISID"].ToString();
                //_CurrentChartYear = UhidClass.GetDateValidated_YearFromUhid(((DataRow)tn.Parent.Parent.Tag)["UHID"].ToString());
                _CurrentChartYear = "2017";
                //执行本方法前，就获取数据，没有则创建的时候，已经没有名字。这里赋值已经来不及了。保存时再更新吧。
                this._patientInfo.PatientName = ((DataRow)tn.Parent.Parent.Tag)["姓名"].ToString(); //tn.Parent.Parent.Text; //((DataRow)tn.Parent.Parent.Tag)["姓名"].ToString();
                _TemplateName = tn.Parent.Text;

                if (CallBackUpdateDocumentPaging != null)
                {
                    //0当前页码，1当前页码
                    int[] arr = new int[] { _CurrentTemplateNameTreeNode.Nodes.Count, int.Parse(_CurrentPage) };
                    CallBackUpdateDocumentPaging(arr, null);
                }

                //---------------------------------------------------------------初始化表单基本参数：画布大小等-------------                   
                //_TemplateXml = GetTemplateXml(tn.Parent.Text); //获取模板和数据都在一开始初始化病人列表的时候，获取好

                XmlNode node_NurseForm = null; //<NurseForm BackColor="Red" Size="700,1000">

                try
                {
                    node_NurseForm = _TemplateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm)); //<NurseForm BackColor="Red" Size="700,1000">

                    //表单大小读取、设定
                    SizeF chartSize = Comm.getSize((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.Size)));
                    pictureBoxBackGround.Size = chartSize.ToSize();
                    panelShade.Size = new Size(pictureBoxBackGround.Size.Width, pictureBoxBackGround.Size.Height - 1);

                    pictureBoxBackGround.BackColor = Color.FromName((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.BackColor)).Trim());

                    //this.pictureBoxBackGround.Refresh();
                    //this.panelShade.Refresh();
                }
                catch //(Exception ex)
                {
                    ShowInforMsg("服务端找不到表单：" + tn.Parent.Text + "，或者表单大小没有设置，请检查：NurseForm->Size。");
                    return;
                }

                //表单元素，整体的相对坐标，控制整体移动，默认为0，0
                if ((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.Origin)).Trim() != "")
                {
                    Size temp = Comm.getSize((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.Origin))).ToSize();
                    _OriginPoint = new Point(temp.Width, temp.Height);
                }
                else
                {
                    _OriginPoint = new Point(0, 0);
                }

                _OriginPointBack = _OriginPoint; //备份的坐标偏移量也设置好

                if (!bool.TryParse((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.IsSpaceChart)).Trim(), out _IsSpaceChart))
                {
                    _IsSpaceChart = false;
                }

                _NurseLevel = (node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.NurseFormLevel)).Trim();

                _HelpString = (node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.HelpString));

                _RowHeaderIndent = (node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.FirstRowIndent));

                _TemplateRight = (node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.TemplateRule));
                if (_TemplateRight != "0" && _TemplateRight != "1" && _TemplateRight != "2")
                {
                    _TemplateRight = "2";
                }

                if ((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.PrintMode)) == "横向")
                {
                    _PrintType = true;   //true为横向打印
                }
                else
                {
                    _PrintType = false;  //false为默认纵向打印
                }

                //签名时，是否需要提示对话框消息_SignNeedConfirmMsg，默认空表示True，需要提示
                if (!string.IsNullOrEmpty((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.IsSignMessage))))
                {
                    if (!bool.TryParse((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.IsSignMessage)), out _SignNeedConfirmMsg))
                    {
                        _SignNeedConfirmMsg = false;
                    }
                }
                else
                {
                    _SignNeedConfirmMsg = true;
                }

                //双签名，默认需要权限等级判断，如果不需要等级判断，都可以互相签名
                if (!bool.TryParse((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.IsSignIgnoreLevel)), out _SignIgnoreLevel))
                {
                    _SignIgnoreLevel = false;
                }

                //每次签名、保存的时候，需要进行登录验证。SignSaveNeedLogin="签名@True¤保存@True" 
                //签名登录验证
                if (!bool.TryParse(Comm.GetPropertyByName((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.SignSaveNeedLogin)), "签名"), out _SignNeedLogin))
                {
                    _SignNeedLogin = false;
                }

                //保存验证
                if (!bool.TryParse(Comm.GetPropertyByName((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.SignSaveNeedLogin)), "保存"), out _SaveNeedLogin))
                {
                    _SaveNeedLogin = false;
                }

                //保存时自动签名_SaveAutoSign
                if ((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.SaveAutoSign)).ToLower() == "start")
                {
                    _SaveAutoSign = "start"; //不需要保存时自动签名
                }
                else if ((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.SaveAutoSign)).ToLower() == "end")
                {
                    _SaveAutoSign = "end"; //不需要保存时自动签名
                }
                else
                {
                    _SaveAutoSign = ""; //不需要保存时自动签名
                }

                //每页最后一行，是否需要签名
                if (!bool.TryParse((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.IsSaveAutoSignLastRow)), out _SaveAutoSignLastRow))
                {
                    _SaveAutoSignLastRow = false;
                }

                #region 同步体温单数据 生命体征数据
                _SynchronizeTemperatureRecordItems.Clear();
                if (!string.IsNullOrEmpty(_TemplateXml.DocumentElement.GetAttribute(nameof(EntXmlModel.SynchronizeTemperatureRecord))))
                {
                    string[] arrSynchronizeTemperature = _TemplateXml.DocumentElement.GetAttribute(nameof(EntXmlModel.SynchronizeTemperatureRecord)).Split(',');

                    for (int k = 0; k < arrSynchronizeTemperature.Length; k++)
                    {
                        _SynchronizeTemperatureRecordItems.Add(arrSynchronizeTemperature[k]);
                    }

                    arrSynchronizeTemperature = null;
                }

                //表单中是不输入体温类型的，那么就不知道是口表还是腋表肛表耳表
                _DefaultTemperatureType = _TemplateXml.DocumentElement.GetAttribute(nameof(EntXmlModel.DefaultTemperatureType));

                #endregion 同步体温单数据 生命体征数据

                //取得该页对应的模板配置样式Page的No来指定：如果不存在本页对应编号ID的模板，那么用最接近于页码的样式id作为本页样式。          
                //_node_Page = _TemplateXml.SelectSingleNode("NurseForm/Design/Form[@ID='" + _CurrentPage + "']"); //<NurseForm BackColor="Red" Size="700,1000">
                _node_Page = GetTemplatePageNode(_TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design))), int.Parse(_CurrentPage));

                _node_Script = _TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Script)));

                if (_TemplateXml.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form))).Count == 1)
                {
                    //如果只有第一页的配置（各业一样）或者 和前一次打开的页数和模板名称都一样，那么就不需要加载了，只要界面控件的值设置为Defalut；
                    //_IsNotNeedReloadTemplate = _IsNotNeedReloadTemplate && true; //只有一页的话，那么需要，(上一次的病人和模板名称一样)--方法上面开始的时候已经判断
                }
                else if (_TemplateXml.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form))).Count >= 1)
                {
                    //_PreShowedPage为空表示第一次打开，那么必须重新加载样式；或者与上次打开的同一病人同一表单的页的模板不属于同一模板页
                    _IsNotNeedReloadTemplate = _IsNotNeedReloadTemplate && (_PreShowedPage != "" && _node_Page == GetTemplatePageNode(_TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design))), int.Parse(_PreShowedPage))); // 如果多页模板，是否与上次打开的页的模板页一致
                }
                else
                {
                    _IsNotNeedReloadTemplate = false;
                    ShowInforMsg("该模板文件中没有样式定义，而且至少一个，需要从<Form ID=“1”开始 ：" + _CurrentTemplateXml_Path, true);
                }

                if (_node_Page == null)//只有第一页的模板，所有页都一样
                {
                    _node_Page = _TemplateXml.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.SN), "1", "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));

                    //防止没有写样式的ID编号
                    if (_node_Page == null)
                    {
                        _node_Page = _TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));
                    }
                }

                if (_node_Page == null)//还为null空，就跳出
                {
                    ShowInforMsg("该模板文件中没有样式定义，而且至少一个，需要从<Form ID=“1”开始  ：" + _CurrentTemplateXml_Path, true);
                    return;
                }

                //-----------------------------------------加载特效提示：正在加载-------------
                _picTransEffect.Top = 0; //这样即使滚动条在下面也能看到特效过渡图片

                if (this.pictureBoxBackGround.Size.Width < this.panelMain.Width)
                {
                    _picTransEffect.Size = new Size(this.pictureBoxBackGround.Size.Width + 8, this.panelMain.Size.Height);
                }
                else
                {
                    _picTransEffect.Size = new Size(this.panelMain.Size.Width, this.panelMain.Size.Height);
                }

                //初始化冻结图的大小，防止切换的时候，过大挡住过渡图
                pictureBoxFrozen.Size = new Size(pictureBoxBackGround.Width, Frozen_Default_Height);

                //-----------------------------------------加载特效提示：正在加载-------------
                _picTransEffect.Image = (Bitmap)this.pictureBoxBackGround.Image.Clone();
                _picTransEffect.BackgroundImageLayout = ImageLayout.Stretch;
                _picTransEffect.Visible = true;
                Graphics.FromImage(_picTransEffect.Image).DrawString("开始切换表单，请等待…… ", new Font("宋体", 11), Brushes.Black, new Point(pictureBoxBackGround.Width / 3, -pictureBoxBackGround.Top + Screen.PrimaryScreen.WorkingArea.Height / 4));

                _picTransEffect.BringToFront();
                _picTransEffect.Refresh();

                //-----------------------------------------加载特效提示：正在加载-------------

                //如果再次当即当前表单的，当前病人的同一页，那也加载模板，可以将表格控制对象完全充值 ；目前是通过遍历所有单元格进行RestCell方法重置的
                if (_IsNotNeedReloadTemplate)
                {
                    //无需重新加载，新添加空白页：那么只要将控件设置为默认值，老页：重新加载数据
                    ReSetDefaultValue();

                    //现在控制的还不够理想，如果是切换病人，有些情况其实无需要加载模板的。 
                    //右击双击的话，刷新数据再打开。或者继续打开当前页就刷新
                    if (reflashdata)
                    {
                        //GetTemplate(_CurrentChartYear.ToString(), this._patientInfo.Id, _TemplateName, 0); //取到表单，并判断其是否被锁住，没有锁住还要将其锁定
                        GetTemplate(_CurrentChartYear.ToString(), this._patientInfo.PatientId, _TemplateName, 0);
                        reflashdata = false;
                    }
                }
                else
                {
                    //判断是否要重新读取表单数据加载：需要加载模板页的时候不一定要重新加载表单数据
                    if (!_IsNotNeedReloadData) //如果只是模板页样式不一样，那么就不需要重新获取载入数据，只要刷新样式
                    {
                        //-----------------------------------------加载特效提示：正在加载-------------
                        ShowEffect("获取最新数据，请稍候…… ");
                        //-----------------------------------------加载特效提示：正在加载-------------

                        //更新病人列表中的基本信息：姓名，床号，科室等……
                        if (RunMode)
                        {
                            UpdatePatientsLisDTInforThread(_PatientsInforDT, this._patientInfo.PatientId);  //线程更新
                        }

                        //做好打开时的备份，为保存前容量大小验证和放弃修改恢复xml而不用刷新数据库做好备份。
                        _RecruitXML_Org = new XmlDocument();
                        _RecruitXML_Org.InnerXml = _RecruitXML.InnerXml;   //备份xml，如果不行有问题，就用string保存，之后再转xml都可以的。

                        //调试：
                        //_RecruitXML.Save(_Recruit_XMlPath + @"\Current.xml");

                        //打印页状态
                        XmlNode nodeXMLRoot = _RecruitXML.SelectSingleNode(nameof(EntXmlModel.NurseForm));
                        _PagePrintStatus = (nodeXMLRoot as XmlElement).GetAttribute(nameof(EntXmlModel.PagePrintStatus));
                    }
                    else
                    {
                        //右击双击的话，刷新数据再打开。或者继续打开当前页就刷新
                        if (reflashdata)
                        {
                            GetTemplate(_CurrentChartYear.ToString(), this._patientInfo.PatientId, _TemplateName, 0); //取到表单，并判断其是否被锁住，没有锁住还要将其锁定

                            reflashdata = false;
                        }
                    }

                    //背景图保存到缓存中，方便刷新调用（无需再绘制，快速）
                    _BmpBack = new Bitmap(pictureBoxBackGround.Width, pictureBoxBackGround.Height);
                    Graphics.FromImage(_BmpBack).Clear(Color.White);
                    pictureBoxBackGround.Image = (Bitmap)_BmpBack.Clone();


                    _TableInfor = null; //每次加载模板页，清空表格对象

                    //如果重新加载模板页，那么要先清空已经加载的控件的          
                    //foreach (Control cl in this.pictureBoxBackGround.Controls) //不一定都能释放掉
                    for (int i = this.pictureBoxBackGround.Controls.Count - 1; i >= 0; i--)
                    {
                        if (this.pictureBoxBackGround.Controls[i] != this.lblShowSearch)
                        {
                            if (this.pictureBoxBackGround.Controls[i] != null && !this.pictureBoxBackGround.Controls[i].IsDisposed)
                            {
                                this.pictureBoxBackGround.Controls[i].Dispose(); //这样才能清楚句柄
                            }
                        }
                    }

                    this.pictureBoxBackGround.Controls.Clear();//清空所有控件     //并没有释放内存，必须dispose  
                    System.GC.Collect();

                    _BaseInforLblnameArr.Clear();
                    _TableHeaderList.Clear();

                    //-----------------------------------------加载特效提示：正在加载-------------
                    ShowEffect("正在加载模板，请稍候…");
                    //-----------------------------------------加载特效提示：正在加载-------------

                    //载入表单模板
                    DrawEditorChart(Graphics.FromImage(this.pictureBoxBackGround.Image), true, true, false);


                    //表单能否编辑，设定：锁定&权限
                    if (_IsLocked)
                    {
                        //被其他人锁定，设置为无效
                        SetAllControlDisabled(true);
                    }
                    else
                    {
                        //如果被自己锁定，再判断表单和自己的权限是否匹配
                        if (GlobalVariable.LoginUserInfo.UserCode.ToLower() != "guest")
                        {
                            //权限判断：
                            if (_TemplateRight == "0")
                            {

                                if (!GlobalVariable.IsDoctor)
                                {
                                    SetAllControlDisabled(true);
                                }
                                else
                                {
                                    SetAllControlDisabled(false);
                                }
                            }
                            else if (_TemplateRight == "1")
                            {

                                if (GlobalVariable.IsDoctor)
                                {
                                    SetAllControlDisabled(true);
                                }
                                else
                                {
                                    SetAllControlDisabled(false);
                                }
                            }
                            else
                            {

                                SetAllControlDisabled(false);
                            }
                        }
                    }

                    //护理 有效无效编辑控制 (上面的逻辑，每次打开表单，都会根据以前的逻辑重新设置有效无效，所以下面不要重置True，直接增加判断即可)
                    if (toolStripSaveEnabled && !_NurseSubmit_Enable && _TemplateRight != "0")  // && _TemplateRight != "0" 是否要排除医生表单
                    {

                        //toolStripStatusLabelType.Text = "无效状态、不能编辑";
                        if (CallBackUpdateDocumentDetailInfor != null)
                        {
                            CallBackUpdateDocumentDetailInfor(new string[] { null, "无效状态、不能编辑" }, null);
                        }

                        SetAllControlDisabled(true);
                    }

                    //this.toolStripButtonPrintAll.Enabled = true;
                    //this.toolStripPrint.Enabled = true;
                    //this.toolStripButton续打.Enabled = true;
                    toolStripPrintEnable = true;


                    //转科科别权限判断----------------------要区分是医生还是护理来判断比较合理。
                    if (RunMode && _DSS.NurseDepartmentRight && toolStripSaveEnabled) //如果已经无效，那么不需要判断了
                    {
                        //bool nurseSubmit_Enable = !toolStripSaveEnabled;
                        //string nurseType = GetNurseType(this._patientInfo.Id, _CurrentChartYear);//这里每次都要检索判断，效率太低。只有切换病人的时候需要

                        string currentType = GlobalVariable.UserDivision;
                        if (GlobalVariable.IsDoctor)
                        {
                            currentType = GlobalVariable.LoginUserInfo.DeptName;
                        }

                        if (!string.IsNullOrEmpty(_NurseType) && _NurseType != currentType) //GlobalVariable.UserDepartMent  GlobalVariable.UserDivision
                        {
                            //SetAllControlDisabled(true); //无权限

                            //无纸化后将没有任何纸质文书，这样会存在部分医疗文书在多科室书写流转时遇到障碍。比如医院这边的“药物过敏试验记录单”，
                            //目前流转流程是：1、临床科室电脑上新建“药物过敏试验记录单”，临床科室护士在电脑上填写单子的上半部分，打印出来随病人进入手术室，
                            //2、手术室护士用笔填写单子的下半部分，手术做完后，随病人再回到临床科室，3、最后找病人或家属在“药物过敏试验记录单”上手工签字。
                            //TODO:指定的一些科室，就有不在本科室病人的编辑权限
                            //比如服务端根节点属性值设置:RecruitEspecialRight="A科室@药物过敏试验记录单,手术交接单¤B科室@手术安全核查表" ：表示A科室拥有此类医疗文书的书写权限，B科室拥有其他医疗文书写权限。
                            bool isHaveEspecialRight = false;
                            if (!string.IsNullOrEmpty(_DSS.RecruitEspecialRight))
                            {
                                string arrDepartment = Comm.GetPropertyByName(_DSS.RecruitEspecialRight, GlobalVariable.LoginUserInfo.DeptName);
                                if (!string.IsNullOrEmpty(arrDepartment))
                                {
                                    string[] strArr = arrDepartment.Split(',');
                                    ArrayList arrList = ArrayList.Adapter(strArr);

                                    if (arrList.Contains(_TemplateName))
                                    {
                                        isHaveEspecialRight = true;
                                    }

                                    strArr = null;
                                    arrList = null;
                                }

                                arrDepartment = null;
                            }

                            if (isHaveEspecialRight)
                            {
                                SetAllControlDisabled(false);
                            }
                            else
                            {
                                if (GlobalVariable.IsHaveUserDivisionConfine(_NurseType))
                                {
                                    SetAllControlDisabled(false);
                                }
                                else
                                {
                                    SetAllControlDisabled(true); //无权限

                                    if (!GlobalVariable.IsDoctor)
                                    {
                                        InitRightInforLabel("已转科/病区，不能编辑。");
                                    }
                                    else
                                    {
                                        InitRightInforLabel("已转科，不能编辑。");
                                    }
                                }
                            }
                        }
                        else
                        {
                            SetAllControlDisabled(false);
                        }

                        if (!GlobalVariable.IsDoctor)
                        {
                            ShowInforMsg("用户病区：" + GlobalVariable.UserDivision + " ，病区：" + _NurseType, false);//Comm.LogHelp.WriteInformation();
                        }
                        else
                        {
                            ShowInforMsg("用户科室：" + GlobalVariable.LoginUserInfo.DeptName + " ，科室：" + _NurseType, false);
                        }
                    }

                    //归档判断
                    if (RunMode) // && toolStripSaveEnabled如果已经无效，那么不需要判断了
                    {
                        //if (((DataRow)tn.Parent.Parent.Tag).Table.Columns.Contains("DONE")) //暂时还没有设值，所有等于没有判断
                        //{
                        //    if (((DataRow)tn.Parent.Parent.Tag)["DONE"] != null && ((DataRow)tn.Parent.Parent.Tag)["DONE"].ToString() == "1")
                        //    {
                        //        toolStripStatusLabelType.Text = "已归档、不能编辑";
                        //        SetAllControlDisabled(true);
                        //    }
                        //}

                        //string done = "";
                        //string doneTempRtght = GetNurseArchive(this._patientInfo.Id, GlobalVariable.LoginUserInfo.UserCode, ref done);
                        string done = _RecruitXML.DocumentElement.GetAttribute(nameof(EntXmlModel.Done)).Trim();

                        if (done == "1")
                        {
                            //this.toolStripButtonPrintAll.Enabled = false;
                            //this.toolStripPrint.Enabled = false;
                            //this.toolStripButton续打.Enabled = false;
                            toolStripPrintEnable = false;

                            //归档的情况下，再判断是否具有：归档后临时权限。GetNurseArchive(patientUHID, _userName)
                            //0(编辑),13(浏览),17(打印)
                            string doneTempRtght = GetNurseArchive(this._patientInfo.PatientId, GlobalVariable.LoginUserInfo.UserCode, ref done);

                            //直接护理表和临时权限表用内联可能没有数据，也查不出是否归档了。所以用外连接
                            //错：sqlDoneAndTemp="select a.DONE, a.DATE_VALIDATED, b.ALLOWED_TYPE from NURSE_YYYY a left join OCCASION b on a.uhid = b.uhid where  a.uhid={UHID} and b.ApplicantUserName={ApplicantUserName}"
                            //对：sqlDoneAndTemp="select a.DONE, a.DATE_VALIDATED, b.ALLOWED_TYPE from NURSE_YYYY a left join OCCASION b on a.uhid = b.uhid and b.ApplicantUserName={ApplicantUserName}"  where  a.uhid={UHID}


                            if (doneTempRtght == "编辑")
                            {
                                //保存按钮，不用处理
                                SetAllControlDisabled(false);

                                toolStripPrintEnable = true;
                                //this.toolStripButtonPrintAll.Enabled = true;
                                //this.toolStripPrint.Enabled = true;
                                //this.toolStripButton续打.Enabled = true;

                                InitRightInforLabel("已归档，具有编辑权限。");
                            }
                            else if (doneTempRtght == "浏览")
                            {
                                SetAllControlDisabled(true);

                                toolStripPrintEnable = false;
                                //this.toolStripButtonPrintAll.Enabled = false;
                                //this.toolStripPrint.Enabled = false;
                                //this.toolStripButton续打.Enabled = false;

                                InitRightInforLabel("已归档，具有浏览权限。");
                            }
                            else if (doneTempRtght == "打印")
                            {
                                SetAllControlDisabled(true);

                                toolStripPrintEnable = true;
                                //this.toolStripButtonPrintAll.Enabled = true;
                                //this.toolStripPrint.Enabled = true;
                                //this.toolStripButton续打.Enabled = true;

                                InitRightInforLabel("已归档，具有打印权限。");
                            }
                            else //if (doneTempRtght == "") 空或者其他，表是归档，没有：浏览，编辑，和打印的权限
                            {
                                SetAllControlDisabled(true);

                                toolStripPrintEnable = false;
                                //this.toolStripButtonPrintAll.Enabled = false;
                                //this.toolStripPrint.Enabled = false;
                                //this.toolStripButton续打.Enabled = false;

                                string msg = "已经归档，没有浏览权限。";
                                InitRightInforLabel(msg);
                                Comm.LogHelp.WriteInformation(msg + this._patientInfo.PatientId + "," + GlobalVariable.LoginUserInfo.UserCode);

                                if (CallBackUpdateDocumentDetailInfor != null)
                                {
                                    CallBackUpdateDocumentDetailInfor(new string[] { null, "已经归档，没有浏览权限" }, null);
                                }

                                this.pictureBoxBackGround.Controls.Clear();

                                MessageBox.Show(msg);



                                //防止切换其他页
                                this._patientInfo.PatientId = "";
                                _TemplateName = "";
                                return;
                                //}
                            }

                        }
                        else
                        {
                            //未归档的时候：
                            //不用处理
                        }
                    }
                }

                #region 表单页信息提示：
                string opeationg = string.Format("【{0}】->【{1}】->【第{2}页】", this._patientInfo.PatientName, _TemplateName, _CurrentPage);
                //判断打印状况：
                ArrayList list = ArrayList.Adapter(_PagePrintStatus.Split('¤'));
                string printStaus = "未打印";
                if (list.Contains(_CurrentPage))
                {
                    //已经打印
                    opeationg += " ❏ "; //☺☻☼☑ ◈ ◇⿻ ✐ ✔ ❏ ❐ ▟ ✉
                    printStaus = "已打印";
                }
                else
                {
                    //未打印
                }

                ShowInforMsg("打开表单:" + opeationg, false);
                //toolStripStatusLabelCurrentPatient.Text = opeationg; //状态栏的提示：病人-表单-页数

                if (CallBackUpdateDocumentDetailInfor != null)
                {
                    CallBackUpdateDocumentDetailInfor(new string[] { printStaus, null }, null);
                }

                #endregion 表单页信息提示：

                #region 冻结图片
                pictureBoxFrozen.Visible = true;
                if (!this.Controls.Contains(this.pictureBoxFrozen))
                {
                    this.Controls.Add(this.pictureBoxFrozen); //this.tabPage2.Controls.Add(this.pictureBoxFrozen);
                }

                pictureBoxFrozen.Location = new Point(pictureBoxBackGround.Location.X, pictureBoxBackGround.Location.Y);
                pictureBoxFrozen.Size = new Size(pictureBoxBackGround.Width, Frozen_Default_Height);
                pictureBoxFrozen.Image = pictureBoxBackGround.Image;
                pictureBoxFrozen.BringToFront();  //置于顶层显示
                #endregion 冻结图片


                //正在加载数据，请稍候

                //初始化模板完成后的处理，关闭加载提示
                if (_picTransEffect != null && !_picTransEffect.IsDisposed)
                {
                    _picTransEffect.Hide();
                    //_picTransEffect.Invoke(new MethodInvoker(delegate { _picTransEffect.Hide(); }));
                }

                //如果配置了页码，那么就更新页号
                if (_PageNo != null)
                {
                    _PageNo.Text = _CurrentPage;
                }

                this.panelMain.Focus();

                //输入框默认值设置
                if (_IsCreating)
                {
                    InputBoxShowDefaultAtAddPage(); //新建表单和新建页的时候，输入框默认值直接显示
                }

                //从数据库取数据，可以在上面加载模板之前就另起线程进程加载，到这里在判断，取好了最好，没有取好就等待
                LoadThisPageData(_TableInfor);

                //病人基本信息更新：1.如果为空--显示最新的； 2.服务端属性：每次更新，体现箭头历史进行设置显示；如果有改动，那么也要提示保存。
                if (RunMode)
                {
                    UpdateThispageBaseInforUseThread(); //UpdateThispageBaseInfor();
                }
                else
                {
                    UpdateThispageBaseInfor(_BaseInforLblnameArr, _CurrentPage, false);
                }

                //自动创建表单的时候，还需要传参过来的 
                LoadCreatTemplatePara();

                Graphics g = Graphics.FromImage(this.pictureBoxBackGround.Image);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                //绘制表格的基础线
                if (_TableType && _TableInfor != null) //根据解析本页的数据，形成本页的表格，进行绘制。这里还是默认的表格线。
                {
                    //绘制表格的基础线
                    DrawTableBaseLines(g, _TableInfor);

                    //绘制表格数据和线
                    DrawTableData_Lines(g, false, _TableInfor, _CurrentPage);
                }

                g.Dispose(); //释放
                this.pictureBoxBackGround.Refresh(); //防止再次点击的时候，画面没有刷新

                //_IsLoading = false; //先置为Flalse，否则下面的刷新双红线就不会显示
                ShowDoubleLineInputboxs();

                _PreShowedPage = _CurrentPage;//处理前，记录下：上次显示的页数

                //验证数据的UHID是否一致
                CheckDataUHID(this._patientInfo.PatientId, _RecruitXML);

                //设置页尾斜线
                SetToolStripPageEndCrossLine();
                #endregion

                this.ResumeLayout();
                this.Cursor = Cursors.Default;
                reflashdata = false;
                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;
                //this.ResumeLayout();
            }
            catch (Exception ex)
            {
                //this.ResumeLayout();
                reflashdata = false;
                _CreatTemplatePara = "";
                this.Cursor = Cursors.Default;
                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        internal void LoadData()
        {
            this.treeViewPatients.Visible = false;

            panelShade.Location = new Point(7, 10);
            pictureBoxBackGround.Location = new Point(1, 1);

            panelShade.Size = new Size(panelMain.Size.Width - 10, panelMain.Size.Height - 10);
            pictureBoxBackGround.Size = panelShade.Size;
            this.pictureBoxBackGround.BringToFront();
            pictureBoxBackGround.Visible = true;
            panelShade.Visible = true;
            //pictureBoxBackGround.Refresh();//防止打开过程中背景闪烁
            //pictureBoxBackGround.Image = new Bitmap(1, 1); //初始化默认图片大小
            Bitmap bmp = new Bitmap(pictureBoxBackGround.Width, pictureBoxBackGround.Height);
            Graphics.FromImage(bmp).Clear(Color.White);
            pictureBoxBackGround.Image = (Bitmap)bmp.Clone();
            bmp.Dispose();

            //pictureBoxBackGround.Refresh();

            //ClearMe();
            Form1_Load(null, null);

            this._TemplateName = "";

            //this.treeViewPatients.BringToFront();
        }

        internal void LoadData(EntPatientInfoEx patientInfo, string patientFormData, XmlDocument templateValue, string templateID, string templateName)
        {
            if (string.IsNullOrEmpty(patientFormData))
            {
                _strInsertOrUpdate = "insert";
            }
            else
            {
                _strInsertOrUpdate = "update";
            }

            panelShade.Location = new Point(7, 10);
            pictureBoxBackGround.Location = new Point(1, 1);
            panelShade.Size = new Size(panelMain.Size.Width - 10, panelMain.Size.Height - 10);
            pictureBoxBackGround.Size = panelShade.Size;
            this.pictureBoxBackGround.BringToFront();
            pictureBoxBackGround.Visible = true;
            panelShade.Visible = true;
            Bitmap bmp = new Bitmap(pictureBoxBackGround.Width, pictureBoxBackGround.Height);
            Graphics.FromImage(bmp).Clear(Color.White);
            pictureBoxBackGround.Image = (Bitmap)bmp.Clone();
            bmp.Dispose();
            Form1_Load(null, null);
            //取得单个病人信息
            this._patientInfo = patientInfo;
            this._TemplateID = templateID;
            this._TemplateName = templateName;
            _TemplateXml = templateValue; //GetTemplateXmlFromDB();
            GlobalVariable.NurseFormValue = patientFormData;
            if (!string.IsNullOrEmpty(this._TemplateID))
                JoinTemplateAndXml();
        }

        internal void LoadData(EntPatientInfoEx patientInfo)
        {
            this._patientInfo = patientInfo;
        }

        /// <summary>
        /// 打开表单明细
        /// </summary>
        internal void LoadFromDetail(string indexPage)
        {
            bool reflashdata = false;//true强行刷数据

            try
            {
                _GoOnPrintLocation = new Point(-1, -1); //续打的位置进行重置

                bool isMsgShowed = false;

                //为防止切换太快，上次的加减锁的线程还在处理，导致混乱
                while (_IsThreadUpdatePatientsLisDTInfor) // 锁定表单和更新基本信息的线程标记
                {
                    if (!isMsgShowed)
                    {
                        //ShowInforMsg("切换动作太快，处理中，稍候再操作");
                        //isMsgShowed = true;
                        Comm.ShowWarningMessage("切换动作太快，处理中，稍候再操作");
                    }

                    System.Threading.Thread.Sleep(20);
                }

                this.Cursor = Cursors.WaitCursor;

                _IsLoading = true;
                _IsNotNeedCellsUpdate = true;

                TreeNode tn = null;

                int openPage = 1;
                if (indexPage == null || string.IsNullOrEmpty(indexPage.ToString()))
                {
                    //如果是空，那么表示默认打开最后一页。但是创建的连续n页的时候，要打开第一页比较符合实际需要
                    if (_IsCreatedTemplate)
                    {
                        //新建表单的时候，如果是连续创建n页，那么打开第一页比较合适。
                        int a = _RecruitXML.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form))).Count;
                        if (a < 1) a = 1;
                        openPage = 1;
                        this.uc_Pager1.PageCurrent = openPage;
                        this.uc_Pager1.PageCount = a;
                        this.uc_Pager1.SetMaxCount(a);
                        this.uc_Pager1.Bind();
                    }
                    else
                    {
                        //重新计算页数：（更加严谨，但是好像也没有必要）
                        #region 重新计算页数：--------
                        int pages = 0;
                        int a = _RecruitXML.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Forms), nameof(EntXmlModel.Form))).Count;              // 非表格的页数
                        int b = _RecruitXML.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Records))).ChildNodes.Count; // 表格的行数

                        openPage = a;
                        this.uc_Pager1.PageCurrent = openPage;
                        this.uc_Pager1.PageCount = a;
                        this.uc_Pager1.SetMaxCount(openPage);
                        this.uc_Pager1.Bind();

                        XmlNode xn = _TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form), nameof(EntXmlModel.Table)));

                        if (xn != null)
                        {
                            pages = TableClass.GetPageCount_HaveTable(_TemplateXml, b);
                        }
                        else
                        {
                            pages = a;
                        }

                        if (openPage < pages)
                        {
                            Comm.LogHelp.WriteErr("总页数和计算的页数不一致：" + this.treeViewPatients.Nodes[0].Text);
                        }
                        #endregion 重新计算页数：--------
                    }
                }
                else
                {
                    if (int.TryParse(indexPage.ToString(), out openPage))
                    {
                        this.uc_Pager1.PageCurrent = openPage;
                        //this.formPager1.PageCount = openPage;
                        //this.formPager1.SetMaxCount(openPage);
                        //this.formPager1.Bind();
                    }
                    else
                    {
                        openPage = 1;
                        this.uc_Pager1.PageCurrent = openPage;
                        //this.formPager1.PageCount = openPage;
                        //this.formPager1.SetMaxCount(openPage);
                        //this.formPager1.Bind();
                    }
                }

                //if (this.treeViewPatients.Nodes.Count == 1)
                //{
                //    //if (this.treeViewPatients.Nodes[0].Nodes.Count < openPage)
                //    //{
                //    //    //补充节点
                //    //}

                //    //删除页后，必须更新主窗体的页码。都会走到该方法的。所以不用担心索引超出范围。
                //    //tn = this.treeViewPatients.Nodes[0].Nodes[0].Nodes[openPage - 1];
                //    tn = this.treeViewPatients.Nodes[0].Nodes[0].Nodes[0];
                //}
                //else
                //{
                //    string noPatientMsg = "病人列表中没有数据。";
                //    Comm.LogHelp.WriteErr(noPatientMsg);
                //    MessageBox.Show(noPatientMsg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //    return;
                //}
                //}

                //if (tn.Tag.ToString() == "3") //如果为页数，才需要打开该表单，否则需要先新建表单后默认有第一页
                #region
                //if (tn.Level == 2) //如果为页数，才需要打开该表单，否则需要先新建表单后默认有第一页
                //{

                if (!this.pictureBoxBackGround.Enabled) // && this.toolStripSaveEnabled新建后又取消，双后在双击，这是填补的空白行没有了，在输入就报为异常:索引的单元格，找不到它所对应的XML节点
                {
                    this.pictureBoxBackGround.Enabled = true;
                }

                _PreCellRowIndex = new Point(-1, -1);

                preRowIndex = -1;
                preColIndex = -1;

                //间距变量
                _SpaceTarget = new Point(-1, -1);
                _Space = 0;

                this.pictureBoxBackGround.Focus();

                //防止光标在单元格内修改数据后，立刻重复打开本页，该输入框会显示不空，光标置入为空，因为没有刷新
                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;

                //打开界面后后第一次操作，不需要
                if (_TemplateName != "")
                {
                    SaveToOp();
                }

                _IsLoading = true;
                _IsNotNeedCellsUpdate = true;

                //if (!_NotNeedCheckForSave)
                //{
                //    //如果需要保存，提示是否需要先保存，再执行后续操作，如果硬要取消，那么重置
                //    CheckForSave();

                //    _IsLoading = true; //上面提醒保存的时候，可能又变成false了。比如：保存时自动签名
                //}


                ////防止调整段落添加页后，直接打开下页，但又取消保存的情况下，新加的页节点又不删除，会报错
                //if (tn.Parent == null || tn.Parent.Parent == null) //新建页，新建模板
                //{
                //    this.Cursor = Cursors.Default;
                //    _IsLoading = false;
                //    _IsNotNeedCellsUpdate = false;

                //    _IsNeedSave = false;
                //    _IsCreated = false;
                //    _IsCreatedTemplate = false;

                //    ClearCurrentTemplate(); //清除当前表单的设置标记

                //    this.pictureBoxBackGround.Enabled = false; //设置为无效，防止继续操作。需要切换表单或者关闭才能重新打开
                //    toolStripSaveEnabled = false;
                //    //this.ToolStripMenuItemSave.Enabled = false;
                //    //this.ToolStripMenuItemDeletePage.Enabled = false;

                //    string msg001 = "新建页在没保存的情况下继续打开本页，又取消提示的保存；属于异常操作，请重新打开表单后再操作。";
                //    ShowInforMsg(msg001, false);

                //    if (CallBackUpdateDocumentPaging != null)
                //    {
                //        if (_CurrentPage != "") //这样的话，会和表单一样弹出消息提示：新建页后没保存的消息。但是页码还是老的
                //        {
                //            //0当前页码，1当前页码
                //            int[] arr = new int[] { _CurrentTemplateNameTreeNode.Nodes.Count, int.Parse(_CurrentPage) }; //_CurrentPage为空异常
                //            CallBackUpdateDocumentPaging(arr, null);
                //        }
                //        else
                //        {
                //            //小结产生新页，点击新页，提示是否保存，如果选择取消，那么页码是增加后的页码，但是数据还是前一页的。
                //            int[] arr = new int[] { _CurrentTemplateNameTreeNode.Nodes.Count, _CurrentTemplateNameTreeNode.Nodes.Count }; //_CurrentPage为空异常
                //            CallBackUpdateDocumentPaging(arr, null);
                //        }
                //    }

                //    MessageBox.Show(msg001, "提示消息", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //    return;
                //}

                //_CurrentPatientTreeNode = tn.Parent.Parent; //记录下目前节点
                //_CurrentTemplateNameTreeNode = tn.Parent;

                _ListDoubleLineInputBox.Clear();    //清空双红线输入框集合
                _ErrorList.Clear();                 //清空错误内容


                ////如果调整段落列的时候，跨页也清空就不能保存了
                //_SynchronizeTemperatureRecordListRecordInor.Clear();            //如果跨页，肯定是调整段落列等情况，那么不保存。体温等肯定是输入的。



                //_IsNotNeedReloadData = false;

                ////记录下，当前打开页的信息：病人，模板名称，页数，并要判断：
                ////如果只有第一页的配置（各业一样）、和前一次打开的页数都一样，那么就不需要加载了，只要界面控件的值设置为Defalut；
                //_IsNotNeedReloadTemplate = false;//先做第一条件判断：同一病人，统一模板名称：当前的uhid和表单名称和上一次的进行比较
                //if (this._patientInfo.Id == ((DataRow)tn.Parent.Parent.Tag)["UHID"].ToString() && _TemplateName == tn.Parent.Text)
                //{
                //    _IsNotNeedReloadTemplate = true; //先判断病人名字和表单，如果不一样，肯定需要重新加载；下面再判断页ID

                //    _IsNotNeedReloadData = true;

                //    //手动双击，当前页，那么也要刷新数据。
                //    //if (e != null && tn != null && _PreShowedPage == tn.Text)
                //    //{
                //    //    reflashdata = true;
                //    //}
                //}
                //else
                //{
                //    //如果调整段落列的时候，跨页也清空就不能保存了
                //    _SynchronizeTemperatureRecordListRecordInor.Clear();            //如果跨页，肯定是调整段落列等情况，那么不保存。体温等肯定是输入的。

                //    _IsNotNeedReloadTemplate = false;

                //    _IsNeedSave = false;
                //    _IsCreated = false;

                //    if (!_IsCreating) //创建新表单的时候，这里不要清空标记，因为更新基本信息后，要生成自动创建N页的每页的默认数据的 AutoCreatPagesCount
                //    {
                //        _IsCreatedTemplate = false;
                //    }
                //}

                //_CurrentPage = tn.Text;


                //_CurrentPage = this.formPager1.PageCurrent.ToString();
                _CurrentPage = openPage.ToString();


                //this._patientInfo.Id = ((DataRow)tn.Parent.Parent.Tag)["UHID"].ToString();
                //this._patientInfo.Id = ((DataRow)tn.Parent.Parent.Tag)["HISID"].ToString();
                //_CurrentChartYear = UhidClass.GetDateValidated_YearFromUhid(((DataRow)tn.Parent.Parent.Tag)["UHID"].ToString());
                //this._patientInfo.Id = "2[血液内科]537463274755.419";
                //this._patientInfo.Id = "000521217400";
                //_CurrentChartYear = "2017";
                //执行本方法前，就获取数据，没有则创建的时候，已经没有名字。这里赋值已经来不及了。保存时再更新吧。
                //this._patientInfo.Name = ((DataRow)tn.Parent.Parent.Tag)["姓名"].ToString(); //tn.Parent.Parent.Text; //((DataRow)tn.Parent.Parent.Tag)["姓名"].ToString();
                //_TemplateName = tn.Parent.Text;

                if (CallBackUpdateDocumentPaging != null)
                {
                    //0当前页码，1当前页码
                    int[] arr = new int[] { _CurrentTemplateNameTreeNode.Nodes.Count, int.Parse(_CurrentPage) };
                    CallBackUpdateDocumentPaging(arr, null);
                }

                //---------------------------------------------------------------初始化表单基本参数：画布大小等-------------                   
                //_TemplateXml = GetTemplateXml(tn.Parent.Text); //获取模板和数据都在一开始初始化病人列表的时候，获取好

                XmlNode node_NurseForm = null; //<NurseForm BackColor="Red" Size="700,1000">

                try
                {
                    node_NurseForm = _TemplateXml.SelectSingleNode(nameof(EntXmlModel.NurseForm)); //<NurseForm BackColor="Red" Size="700,1000">

                    //表单大小读取、设定
                    SizeF chartSize = Comm.getSize((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.Size)));
                    pictureBoxBackGround.Size = chartSize.ToSize();
                    panelShade.Size = new Size(pictureBoxBackGround.Size.Width, pictureBoxBackGround.Size.Height - 1);

                    pictureBoxBackGround.BackColor = Color.FromName((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.BackColor)).Trim());

                    //this.pictureBoxBackGround.Refresh();
                    //this.panelShade.Refresh();
                }
                catch //(Exception ex)
                {
                    ShowInforMsg("找不到表单：" + tn.Parent.Text + "，或者表单大小没有设置，请检查：NurseForm->Size。");
                    return;
                }

                //表单元素，整体的相对坐标，控制整体移动，默认为0，0
                if ((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.Origin)).Trim() != "")
                {
                    Size temp = Comm.getSize((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.Origin))).ToSize();
                    _OriginPoint = new Point(temp.Width, temp.Height);
                }
                else
                {
                    _OriginPoint = new Point(0, 0);
                }

                _OriginPointBack = _OriginPoint; //备份的坐标偏移量也设置好

                if (!bool.TryParse((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.IsSpaceChart)).Trim(), out _IsSpaceChart))
                {
                    _IsSpaceChart = false;
                }

                _NurseLevel = (node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.NurseFormLevel)).Trim();

                _HelpString = (node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.HelpString));

                _RowHeaderIndent = (node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.FirstRowIndent));

                _TemplateRight = (node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.TemplateRule));
                if (_TemplateRight != "0" && _TemplateRight != "1" && _TemplateRight != "2")
                {
                    _TemplateRight = "2";
                }

                if ((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.PrintMode)) == "横向")
                {
                    _PrintType = true;   //true为横向打印
                }
                else
                {
                    _PrintType = false;  //false为默认纵向打印
                }

                //签名时，是否需要提示对话框消息_SignNeedConfirmMsg，默认空表示True，需要提示
                if (!string.IsNullOrEmpty((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.IsSignMessage))))
                {
                    if (!bool.TryParse((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.IsSignMessage)), out _SignNeedConfirmMsg))
                    {
                        _SignNeedConfirmMsg = false;
                    }
                }
                else
                {
                    _SignNeedConfirmMsg = true;
                }

                //双签名，默认需要权限等级判断，如果不需要等级判断，都可以互相签名
                if (!bool.TryParse((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.IsSignIgnoreLevel)), out _SignIgnoreLevel))
                {
                    _SignIgnoreLevel = false;
                }

                //每次签名、保存的时候，需要进行登录验证。SignSaveNeedLogin="签名@True¤保存@True" 
                //签名登录验证
                if (!bool.TryParse(Comm.GetPropertyByName((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.SignSaveNeedLogin)), "签名"), out _SignNeedLogin))
                {
                    _SignNeedLogin = false;
                }

                //保存验证
                if (!bool.TryParse(Comm.GetPropertyByName((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.SignSaveNeedLogin)), "保存"), out _SaveNeedLogin))
                {
                    _SaveNeedLogin = false;
                }

                //保存时自动签名_SaveAutoSign
                if ((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.SaveAutoSign)).ToLower() == "start")
                {
                    _SaveAutoSign = "start"; //不需要保存时自动签名
                }
                else if ((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.SaveAutoSign)).ToLower() == "end")
                {
                    _SaveAutoSign = "end"; //不需要保存时自动签名
                }
                else
                {
                    _SaveAutoSign = ""; //不需要保存时自动签名
                }

                //每页最后一行，是否需要签名
                if (!bool.TryParse((node_NurseForm as XmlElement).GetAttribute(nameof(EntXmlModel.IsSaveAutoSignLastRow)), out _SaveAutoSignLastRow))
                {
                    _SaveAutoSignLastRow = false;
                }

                #region 同步体温单数据 生命体征数据
                _SynchronizeTemperatureRecordItems.Clear();
                if (!string.IsNullOrEmpty(_TemplateXml.DocumentElement.GetAttribute(nameof(EntXmlModel.SynchronizeTemperatureRecord))))
                {
                    string[] arrSynchronizeTemperature = _TemplateXml.DocumentElement.GetAttribute(nameof(EntXmlModel.SynchronizeTemperatureRecord)).Split(',');

                    for (int k = 0; k < arrSynchronizeTemperature.Length; k++)
                    {
                        _SynchronizeTemperatureRecordItems.Add(arrSynchronizeTemperature[k]);
                    }

                    arrSynchronizeTemperature = null;
                }

                //表单中是不输入体温类型的，那么就不知道是口表还是腋表肛表耳表
                _DefaultTemperatureType = _TemplateXml.DocumentElement.GetAttribute(nameof(EntXmlModel.DefaultTemperatureType));

                #endregion 同步体温单数据 生命体征数据

                //取得该页对应的模板配置样式Page的No来指定：如果不存在本页对应编号ID的模板，那么用最接近于页码的样式id作为本页样式。          
                //_node_Page = _TemplateXml.SelectSingleNode("NurseForm/Design/Form[@ID='" + _CurrentPage + "']"); //<NurseForm BackColor="Red" Size="700,1000">
                _node_Page = GetTemplatePageNode(_TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design))), int.Parse(_CurrentPage));

                _node_Script = _TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Script)));

                if (_TemplateXml.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form))).Count == 1)
                {
                    //如果只有第一页的配置（各业一样）或者 和前一次打开的页数和模板名称都一样，那么就不需要加载了，只要界面控件的值设置为Defalut；
                    //_IsNotNeedReloadTemplate = _IsNotNeedReloadTemplate && true; //只有一页的话，那么需要，(上一次的病人和模板名称一样)--方法上面开始的时候已经判断
                }
                else if (_TemplateXml.SelectNodes(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form))).Count >= 1)
                {
                    //_PreShowedPage为空表示第一次打开，那么必须重新加载样式；或者与上次打开的同一病人同一表单的页的模板不属于同一模板页
                    _IsNotNeedReloadTemplate = _IsNotNeedReloadTemplate && (_PreShowedPage != "" && _node_Page == GetTemplatePageNode(_TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design))), int.Parse(_PreShowedPage))); // 如果多页模板，是否与上次打开的页的模板页一致
                }
                else
                {
                    _IsNotNeedReloadTemplate = false;
                    ShowInforMsg("该模板文件中没有样式定义，而且至少一个，需要从<Form ID=“1”开始 ：" + _CurrentTemplateXml_Path, true);
                }

                if (_node_Page == null)//只有第一页的模板，所有页都一样
                {
                    //"NurseForm/Design/Form[@ID='1']"
                    _node_Page = _TemplateXml.SelectSingleNode(EntXmlModel.GetNameAt(nameof(EntXmlModel.ID), "1", "=", nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));

                    //防止没有写样式的ID编号
                    if (_node_Page == null)
                    {
                        _node_Page = _TemplateXml.SelectSingleNode(EntXmlModel.GetName(nameof(EntXmlModel.NurseForm), nameof(EntXmlModel.Design), nameof(EntXmlModel.Form)));
                    }
                }

                if (_node_Page == null)//还为null空，就跳出
                {
                    ShowInforMsg("该模板文件中没有样式定义，而且至少一个，需要从<Form ID=“1”开始  ：" + _CurrentTemplateXml_Path, true);
                    return;
                }

                //-----------------------------------------加载特效提示：正在加载-------------
                _picTransEffect.Top = 0; //这样即使滚动条在下面也能看到特效过渡图片

                if (this.pictureBoxBackGround.Size.Width < this.panelMain.Width)
                {
                    _picTransEffect.Size = new Size(this.pictureBoxBackGround.Size.Width + 8, this.panelMain.Size.Height);
                }
                else
                {
                    _picTransEffect.Size = new Size(this.panelMain.Size.Width, this.panelMain.Size.Height);
                }

                //初始化冻结图的大小，防止切换的时候，过大挡住过渡图
                pictureBoxFrozen.Size = new Size(pictureBoxBackGround.Width, Frozen_Default_Height);

                //-----------------------------------------加载特效提示：正在加载-------------
                _picTransEffect.Image = (Bitmap)this.pictureBoxBackGround.Image.Clone();
                _picTransEffect.BackgroundImageLayout = ImageLayout.Stretch;
                _picTransEffect.Visible = true;
                Graphics.FromImage(_picTransEffect.Image).DrawString("开始切换表单，请等待…… ", new Font("宋体", 11), Brushes.Black, new Point(pictureBoxBackGround.Width / 3, -pictureBoxBackGround.Top + Screen.PrimaryScreen.WorkingArea.Height / 4));

                _picTransEffect.BringToFront();
                _picTransEffect.Refresh();

                //-----------------------------------------加载特效提示：正在加载-------------

                //如果再次当即当前表单的，当前病人的同一页，那也加载模板，可以将表格控制对象完全充值 ；目前是通过遍历所有单元格进行RestCell方法重置的
                if (_IsNotNeedReloadTemplate)
                {
                    //无需重新加载，新添加空白页：那么只要将控件设置为默认值，老页：重新加载数据
                    ReSetDefaultValue();

                    //现在控制的还不够理想，如果是切换病人，有些情况其实无需要加载模板的。

                    //右击双击的话，刷新数据再打开。或者继续打开当前页就刷新
                    if (reflashdata)
                    {
                        //GetTemplate(_CurrentChartYear.ToString(), this._patientInfo.Id, _TemplateName, 0); //取到表单，并判断其是否被锁住，没有锁住还要将其锁定
                        GetTemplate(_CurrentChartYear.ToString(), this._patientInfo.PatientId, _TemplateName, 0);
                        reflashdata = false;
                    }
                }
                else
                {
                    //判断是否要重新读取表单数据加载：需要加载模板页的时候不一定要重新加载表单数据
                    if (!_IsNotNeedReloadData) //如果只是模板页样式不一样，那么就不需要重新获取载入数据，只要刷新样式
                    {
                        //-----------------------------------------加载特效提示：正在加载-------------
                        ShowEffect("获取最新数据，请稍候…… ");
                        //-----------------------------------------加载特效提示：正在加载-------------

                        //更新病人列表中的基本信息：姓名，床号，科室等……
                        if (RunMode)
                        {
                            //1.获取数据显示到表单（基本信息）
                            //UpdatePatientsLisDTInfor(_PatientsInforDT, this._patientInfo.Id);      
                            UpdatePatientsLisDTInforThread(_PatientsInforDT, this._patientInfo.PatientId);  //线程更新：（服务端可配置成刷新HIS获取的）

                        }


                        if (RunMode)
                        {
                            ////切换表单的时候需要重新减锁前表单，加锁后表单
                            ////如果_LockTemplateName不为空，表示自己上次有锁定的表单，要先减锁
                            //UnlockRecord();

                            ////关闭定时更新锁定的定时刷新器：关闭表单和重新打开表单的时候（需要获取表单数据的时候）
                            // _RecordLockTimer.Stop();

                            //GetTemplate(_CurrentChartYear.ToString(), this._patientInfo.Id, _TemplateName, 1); //取到表单，并判断其是否被锁住，没有锁住还要将其锁定

                        }
                        else
                        {
                            //string filePath = _RecruitXMLPATH_Data + @"\" + _TemplateName + this._patientInfo.Id + @".xml";//_RecruitXMLPATH_Data + @"\" + _TemplateName + @".xml"

                            //if (File.Exists(filePath))
                            //{
                            //    _RecruitXML = new XmlDocument();
                            //    _RecruitXML.Load(filePath);
                            //}
                            //else
                            //{
                            //    //_RecruitXML = MyXml.CreateXML(this._patientInfo.Id, this._patientInfo.Name, GlobalVariable.LoginUserInfo.UserCode, GlobalVariable.LoginUserInfo.DeptName);
                            //    MyXml mx = new MyXml();
                            //    _RecruitXML = mx.CreateXML(this._patientInfo.Id, this._patientInfo.Name, GlobalVariable.LoginUserInfo.UserCode, GlobalVariable.LoginUserInfo.DeptName, _TemplateXml, _TemplateName);
                            //}
                        }

                        //if (_RecruitXML == null) //获取数据失败，那么防止在之前打开其他人的表单数据上修改保存错乱
                        //{
                        //    //清空界面
                        //    this.pictureBoxBackGround.Controls.Clear(); //并没有释放内存，必须dispose

                        //    if (this.pictureBoxBackGround.Image != null)
                        //    {
                        //        Graphics.FromImage(this.pictureBoxBackGround.Image).Clear(Color.White);
                        //    }

                        //    ClearCurrentTemplate();//清空参数，以便下次双击可以重新检索数据

                        //    _IsLocked = false;
                        //    _RecordLockTimer.Stop();
                        //    _LockYear = "";
                        //    _LockUHID = "";
                        //    _LockTemplateName = "";
                        //    _LockUserId = "";

                        //    toolStripSaveEnabled = false; //不让进行保存，方式数据错乱和覆盖
                        //    this.ToolStripMenuItemSave.Enabled = false;
                        //    this.ToolStripMenuItemDeletePage.Enabled = false;

                        //    return;
                        //}

                        //做好打开时的备份，为保存前容量大小验证和放弃修改恢复xml而不用刷新数据库做好备份。
                        _RecruitXML_Org = new XmlDocument();
                        _RecruitXML_Org.InnerXml = _RecruitXML.InnerXml;   //备份xml，如果不行有问题，就用string保存，之后再转xml都可以的。

                        //调试：
                        //_RecruitXML.Save(_Recruit_XMlPath + @"\Current.xml");

                        //打印页状态
                        XmlNode nodeXMLRoot = _RecruitXML.SelectSingleNode(nameof(EntXmlModel.NurseForm));
                        _PagePrintStatus = (nodeXMLRoot as XmlElement).GetAttribute(nameof(EntXmlModel.PagePrintStatus));
                    }
                    else
                    {
                        //右击双击的话，刷新数据再打开。或者继续打开当前页就刷新
                        if (reflashdata)
                        {
                            GetTemplate(_CurrentChartYear.ToString(), this._patientInfo.PatientId, _TemplateName, 0); //取到表单，并判断其是否被锁住，没有锁住还要将其锁定

                            reflashdata = false;
                        }
                    }

                    //背景图保存到缓存中，方便刷新调用（无需再绘制，快速）
                    _BmpBack = new Bitmap(pictureBoxBackGround.Width, pictureBoxBackGround.Height);
                    Graphics.FromImage(_BmpBack).Clear(Color.White);
                    pictureBoxBackGround.Image = (Bitmap)_BmpBack.Clone();


                    _TableInfor = null; //每次加载模板页，清空表格对象

                    //如果重新加载模板页，那么要先清空已经加载的控件的         
                    //foreach (Control cl in this.pictureBoxBackGround.Controls) //不一定都能释放掉
                    for (int i = this.pictureBoxBackGround.Controls.Count - 1; i >= 0; i--)
                    {
                        if (this.pictureBoxBackGround.Controls[i] != this.lblShowSearch)
                        {
                            if (this.pictureBoxBackGround.Controls[i] != null && !this.pictureBoxBackGround.Controls[i].IsDisposed)
                            {
                                this.pictureBoxBackGround.Controls[i].Dispose(); //这样才能清楚句柄
                            }
                        }
                    }

                    this.pictureBoxBackGround.Controls.Clear();//清空所有控件     //并没有释放内存，必须dispose  
                    System.GC.Collect();

                    _BaseInforLblnameArr.Clear();
                    _TableHeaderList.Clear();

                    //-----------------------------------------加载特效提示：正在加载-------------
                    ShowEffect("正在加载模板，请稍候…");
                    //-----------------------------------------加载特效提示：正在加载-------------

                    //载入表单模板
                    DrawEditorChart(Graphics.FromImage(this.pictureBoxBackGround.Image), true, true, false);


                    //表单能否编辑，设定：锁定&权限
                    if (_IsLocked)
                    {
                        //被其他人锁定，设置为无效
                        SetAllControlDisabled(true);
                    }
                    else
                    {
                        //如果被自己锁定，再判断表单和自己的权限是否匹配
                        if (GlobalVariable.LoginUserInfo.UserCode.ToLower() != "guest")
                        {
                            //权限判断：
                            if (_TemplateRight == "0")
                            {

                                if (!GlobalVariable.IsDoctor)
                                {
                                    SetAllControlDisabled(true);
                                }
                                else
                                {
                                    SetAllControlDisabled(false);
                                }
                            }
                            else if (_TemplateRight == "1")
                            {

                                if (GlobalVariable.IsDoctor)
                                {
                                    SetAllControlDisabled(true);
                                }
                                else
                                {
                                    SetAllControlDisabled(false);
                                }
                            }
                            else
                            {

                                SetAllControlDisabled(false);
                            }
                        }
                    }

                    //护理 有效无效编辑控制 (上面的逻辑，每次打开表单，都会根据以前的逻辑重新设置有效无效，所以下面不要重置True，直接增加判断即可)
                    if (toolStripSaveEnabled && !_NurseSubmit_Enable && _TemplateRight != "0")  // && _TemplateRight != "0" 是否要排除医生表单
                    {
                        //toolStripStatusLabelType.Text = "无效状态、不能编辑";
                        if (CallBackUpdateDocumentDetailInfor != null)
                        {
                            CallBackUpdateDocumentDetailInfor(new string[] { null, "无效状态、不能编辑" }, null);
                        }

                        SetAllControlDisabled(true);
                    }

                    //this.toolStripButtonPrintAll.Enabled = true;
                    //this.toolStripPrint.Enabled = true;
                    //this.toolStripButton续打.Enabled = true;
                    toolStripPrintEnable = true;

                    //转科科别权限判断----------------------要区分是医生还是护理来判断比较合理。
                    if (RunMode && _DSS.NurseDepartmentRight && toolStripSaveEnabled) //如果已经无效，那么不需要判断了
                    {
                        //bool nurseSubmit_Enable = !toolStripSaveEnabled;
                        //string nurseType = GetNurseType(this._patientInfo.Id, _CurrentChartYear);//这里每次都要检索判断，效率太低。只有切换病人的时候需要

                        string currentType = GlobalVariable.UserDivision;
                        if (GlobalVariable.IsDoctor)
                        {
                            currentType = GlobalVariable.LoginUserInfo.DeptName;
                        }

                        if (!string.IsNullOrEmpty(_NurseType) && _NurseType != currentType) //GlobalVariable.UserDepartMent  GlobalVariable.UserDivision
                        {
                            //SetAllControlDisabled(true); //无权限

                            //无纸化后将没有任何纸质文书，这样会存在部分医疗文书在多科室书写流转时遇到障碍。比如医院这边的“药物过敏试验记录单”，
                            //目前流转流程是：1、临床科室电脑上新建“药物过敏试验记录单”，临床科室护士在电脑上填写单子的上半部分，打印出来随病人进入手术室，
                            //2、手术室护士用笔填写单子的下半部分，手术做完后，随病人再回到临床科室，3、最后找病人或家属在“药物过敏试验记录单”上手工签字。
                            //TODO:指定的一些科室，就有不在本科室病人的编辑权限
                            //比如服务端根节点属性值设置:RecruitEspecialRight="A科室@药物过敏试验记录单,手术交接单¤B科室@手术安全核查表" ：表示A科室拥有此类医疗文书的书写权限，B科室拥有其他医疗文书写权限。
                            bool isHaveEspecialRight = false;
                            if (!string.IsNullOrEmpty(_DSS.RecruitEspecialRight))
                            {
                                string arrDepartment = Comm.GetPropertyByName(_DSS.RecruitEspecialRight, GlobalVariable.LoginUserInfo.DeptName);
                                if (!string.IsNullOrEmpty(arrDepartment))
                                {
                                    string[] strArr = arrDepartment.Split(',');
                                    ArrayList arrList = ArrayList.Adapter(strArr);

                                    if (arrList.Contains(_TemplateName))
                                    {
                                        isHaveEspecialRight = true;
                                    }

                                    strArr = null;
                                    arrList = null;
                                }

                                arrDepartment = null;
                            }

                            if (isHaveEspecialRight)
                            {
                                SetAllControlDisabled(false);
                            }
                            else
                            {
                                if (GlobalVariable.IsHaveUserDivisionConfine(_NurseType))
                                {
                                    SetAllControlDisabled(false);
                                }
                                else
                                {
                                    SetAllControlDisabled(true); //无权限

                                    if (!GlobalVariable.IsDoctor)
                                    {
                                        InitRightInforLabel("已转科/病区，不能编辑。");
                                    }
                                    else
                                    {
                                        InitRightInforLabel("已转科，不能编辑。");
                                    }
                                }
                            }
                        }
                        else
                        {
                            SetAllControlDisabled(false);
                        }

                        if (!GlobalVariable.IsDoctor)
                        {
                            ShowInforMsg("用户病区：" + GlobalVariable.UserDivision + " ，病区：" + _NurseType, false);//Comm.LogHelp.WriteInformation();
                        }
                        else
                        {
                            ShowInforMsg("用户科室：" + GlobalVariable.LoginUserInfo.DeptName + " ，科室：" + _NurseType, false);
                        }
                    }

                    //归档判断
                    if (RunMode)
                    {
                        string done = GlobalVariable.Done;

                        if (done == "1")
                        {
                            //if (GlobalVariable.isBrowseDoneEmr) //True：归档后不能浏览，才需要判断浏览权限
                            //{
                            //this.toolStripButtonPrintAll.Enabled = false;
                            //this.toolStripPrint.Enabled = false;
                            //this.toolStripButton续打.Enabled = false;
                            toolStripPrintEnable = false;

                            //归档的情况下，再判断是否具有：归档后临时权限。GetNurseArchive(patientUHID, _userName)
                            //0(编辑),13(浏览),17(打印)
                            string doneTempRtght = GetNurseArchive(this._patientInfo.PatientId, GlobalVariable.LoginUserInfo.UserCode, ref done);

                            //直接护理表和临时权限表用内联可能没有数据，也查不出是否归档了。所以用外连接
                            //错：sqlDoneAndTemp="select a.DONE, a.DATE_VALIDATED, b.ALLOWED_TYPE from NURSE_YYYY a left join OCCASION b on a.uhid = b.uhid where  a.uhid={UHID} and b.ApplicantUserName={ApplicantUserName}"
                            //对：sqlDoneAndTemp="select a.DONE, a.DATE_VALIDATED, b.ALLOWED_TYPE from NURSE_YYYY a left join OCCASION b on a.uhid = b.uhid and b.ApplicantUserName={ApplicantUserName}"  where  a.uhid={UHID}


                            if (doneTempRtght == "编辑")
                            {
                                //保存按钮，不用处理
                                SetAllControlDisabled(false);

                                toolStripPrintEnable = true;
                                //this.toolStripButtonPrintAll.Enabled = true;
                                //this.toolStripPrint.Enabled = true;
                                //this.toolStripButton续打.Enabled = true;

                                InitRightInforLabel("已归档，具有编辑权限。");
                            }
                            else if (doneTempRtght == "浏览")
                            {
                                SetAllControlDisabled(true);

                                toolStripPrintEnable = false;
                                //this.toolStripButtonPrintAll.Enabled = false;
                                //this.toolStripPrint.Enabled = false;
                                //this.toolStripButton续打.Enabled = false;

                                InitRightInforLabel("已归档，具有浏览权限。");
                            }
                            else if (doneTempRtght == "打印")
                            {
                                SetAllControlDisabled(true);

                                toolStripPrintEnable = true;
                                //this.toolStripButtonPrintAll.Enabled = true;
                                //this.toolStripPrint.Enabled = true;
                                //this.toolStripButton续打.Enabled = true;

                                InitRightInforLabel("已归档，具有打印权限。");
                            }
                            else //if (doneTempRtght == "") 空或者其他，表是归档，没有：浏览，编辑，和打印的权限
                            {
                                SetAllControlDisabled(true);

                                toolStripPrintEnable = false;
                                //this.toolStripButtonPrintAll.Enabled = false;
                                //this.toolStripPrint.Enabled = false;
                                //this.toolStripButton续打.Enabled = false;

                                string msg = "已经归档，没有浏览权限。";
                                InitRightInforLabel(msg);
                                Comm.LogHelp.WriteInformation(msg + this._patientInfo.PatientId + "," + GlobalVariable.LoginUserInfo.UserCode);

                                if (CallBackUpdateDocumentDetailInfor != null)
                                {
                                    CallBackUpdateDocumentDetailInfor(new string[] { null, "已经归档，没有浏览权限" }, null);
                                }

                                this.pictureBoxBackGround.Controls.Clear();

                                MessageBox.Show(msg);



                                //防止切换其他页
                                this._patientInfo.PatientId = "";
                                _TemplateName = "";
                                return;
                                //}
                            }
                            //}
                            //else
                            //{
                            //    SetAllControlDisabled(true);

                            //    if (CallBackUpdateDocumentDetailInfor != null)
                            //    {
                            //        CallBackUpdateDocumentDetailInfor(new string[] { null, "已归档、不能编辑" }, null);
                            //    }

                            //    InitRightInforLabel("已归档，不能编辑。");

                            //    toolStripPrintEnable = false;
                            //    //this.toolStripButtonPrintAll.Enabled = false;
                            //    //this.toolStripPrint.Enabled = false;
                            //    //this.toolStripButton续打.Enabled = false;
                            //}
                        }
                        else
                        {
                            //未归档的时候：
                            //不用处理
                        }
                    }
                }

                #region 表单页信息提示：
                string opeationg = string.Format("【{0}】->【{1}】->【第{2}页】", this._patientInfo.PatientName, _TemplateName, _CurrentPage);
                //判断打印状况：
                ArrayList list = ArrayList.Adapter(_PagePrintStatus.Split('¤'));
                string printStaus = "未打印";
                if (list.Contains(_CurrentPage))
                {
                    //已经打印
                    opeationg += " ❏ "; //☺☻☼☑ ◈ ◇⿻ ✐ ✔ ❏ ❐ ▟ ✉
                    printStaus = "已打印";
                }
                else
                {
                    //未打印
                }

                ShowInforMsg("打开表单:" + opeationg, false);
                //toolStripStatusLabelCurrentPatient.Text = opeationg; //状态栏的提示：病人-表单-页数

                if (CallBackUpdateDocumentDetailInfor != null)
                {
                    CallBackUpdateDocumentDetailInfor(new string[] { printStaus, null }, null);
                }

                #endregion 表单页信息提示：

                #region 冻结图片
                pictureBoxFrozen.Visible = false;
                if (!this.Controls.Contains(this.pictureBoxFrozen))
                {
                    this.Controls.Add(this.pictureBoxFrozen); //this.tabPage2.Controls.Add(this.pictureBoxFrozen);
                }

                pictureBoxFrozen.Location = new Point(pictureBoxBackGround.Location.X, pictureBoxBackGround.Location.Y);
                pictureBoxFrozen.Size = new Size(pictureBoxBackGround.Width, Frozen_Default_Height);
                pictureBoxFrozen.Image = pictureBoxBackGround.Image;
                pictureBoxFrozen.BringToFront();  //置于顶层显示
                #endregion 冻结图片


                //-----------------------------------------加载特效提示：正在加载-------------
                ShowEffect("正在加载数据，请稍候…");
                //-----------------------------------------加载特效提示：正在加载-------------

                //初始化模板完成后的处理，关闭加载提示
                if (_picTransEffect != null && !_picTransEffect.IsDisposed)
                {
                    _picTransEffect.Hide();
                    //_picTransEffect.Invoke(new MethodInvoker(delegate { _picTransEffect.Hide(); }));
                }

                //如果配置了页码，那么就更新页号
                if (_PageNo != null)
                {
                    _PageNo.Text = _CurrentPage;
                }

                this.panelMain.Focus();

                //输入框默认值设置
                if (_IsCreating)
                {
                    InputBoxShowDefaultAtAddPage(); //新建表单和新建页的时候，输入框默认值直接显示
                }

                //从数据库取数据，可以在上面加载模板之前就另起线程进程加载，到这里在判断，取好了最好，没有取好就等待
                LoadThisPageData(_TableInfor);

                //病人基本信息更新：1.如果为空--显示最新的； 2.服务端属性：每次更新，体现箭头历史进行设置显示；如果有改动，那么也要提示保存。
                if (RunMode)
                {
                    UpdateThispageBaseInforUseThread(); //UpdateThispageBaseInfor();
                }
                else
                {
                    UpdateThispageBaseInfor(_BaseInforLblnameArr, _CurrentPage, false);
                }

                //自动创建表单的时候，还需要传参过来的 
                LoadCreatTemplatePara();

                Graphics g = Graphics.FromImage(this.pictureBoxBackGround.Image);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                //绘制表格的基础线
                if (_TableType && _TableInfor != null) //根据解析本页的数据，形成本页的表格，进行绘制。这里还是默认的表格线。
                {
                    //绘制表格的基础线
                    DrawTableBaseLines(g, _TableInfor);

                    //绘制表格数据和线
                    DrawTableData_Lines(g, false, _TableInfor, _CurrentPage);
                }

                g.Dispose(); //释放
                this.pictureBoxBackGround.Refresh(); //防止再次点击的时候，画面没有刷新

                //_IsLoading = false; //先置为Flalse，否则下面的刷新双红线就不会显示
                ShowDoubleLineInputboxs();

                #region 绘制合计行提醒符号
                ////在DrawTableData_Lines方法绘制的话，每一页都有那个符号了
                //if (_TableType && _TableInfor != null)
                //{
                //    int fieldIndex = 0;
                //    for (int field = 0; field < _TableInfor.GroupColumnNum; field++)
                //    {
                //        fieldIndex = _TableInfor.RowsCount * field;

                //        for (int i = 0; i < _TableInfor.RowsCount; i++)
                //        {
                //            DrawSumRowAttention(_TableInfor, i + fieldIndex, null);
                //        }
                //    }
                //}
                #endregion 绘制合计行提醒符号

                _PreShowedPage = _CurrentPage;//处理前，记录下：上次显示的页数

                //验证数据的UHID是否一致
                //CheckDataUHID(this._patientInfo.Id, _RecruitXML);

                //设置页尾斜线
                SetToolStripPageEndCrossLine();
                //}
                #endregion

                this.ResumeLayout();
                this.Cursor = Cursors.Default;
                reflashdata = false;
                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;
                //this.ResumeLayout();
            }
            catch (Exception ex)
            {
                //this.ResumeLayout();
                reflashdata = false;
                _CreatTemplatePara = "";
                this.Cursor = Cursors.Default;
                _IsLoading = false;
                _IsNotNeedCellsUpdate = false;
                Comm.LogHelp.WriteErr(ex);
            }
        }

        internal int SaveData()
        {

            bool ret = SaveToDetail(true);

            this.pictureBoxBackGround.Refresh();

            if (ret)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }


        /// <summary>
        /// 打印 打印当前页 工具栏按钮
        /// </summary>
        /// <param name="type"></param>
        internal void PrintCurrentPage(string type)
        {
            printCurrentPage(type);
        }

        /// <summary>
        /// 执行打印所有页
        /// </summary>
        internal void PrintAllPage()
        {
            printAllPage();
        }

        internal void SaveForOp()
        {
            this.pictureBoxBackGround.Focus();
            SaveToOp();
        }

        /// <summary>
        /// 插入特殊字符
        /// </summary>
        /// <param name="strInsert"></param>
        internal void AddWord(string strInsert)
        {
            //赋值到指定的输入框中
            if ((_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed))
            {
                if (_Need_Assistant_Control is RichTextBoxExtended)
                {
                    //赋值到指定的输入框中
                    if ((_Need_Assistant_Control != null && !((Control)_Need_Assistant_Control).IsDisposed))
                    {
                        //HorizontalAlignment temp = ((RichTextBoxExtended)_Need_Assistant_Control).SelectionAlignment;

                        //if (((RichTextBoxExtended)_Need_Assistant_Control).ReadOnly)
                        //{
                        //    ShowInforMsg("只读对象，无法写入。");
                        //    return; //如果只读，则无效
                        //}

                        //((RichTextBoxExtended)_Need_Assistant_Control).SelectedText = "";//替换掉选择的文字

                        //int index = ((RichTextBoxExtended)_Need_Assistant_Control).SelectionStart;    //记录修改时光标的位置
                        //((RichTextBoxExtended)_Need_Assistant_Control).Focus();
                        //((RichTextBoxExtended)_Need_Assistant_Control).Text = ((RichTextBoxExtended)_Need_Assistant_Control).Text.Insert(index, strInsert);
                        //((RichTextBoxExtended)_Need_Assistant_Control).Select(index + strInsert.Length, 0);

                        //((RichTextBoxExtended)_Need_Assistant_Control).SelectionAlignment = temp;

                        HorizontalAlignment temp = ((RichTextBoxExtended)_Need_Assistant_Control).SelectionAlignment;

                        ((RichTextBoxExtended)_Need_Assistant_Control).SelectedText = "";//替换掉选择的文字

                        int index = ((RichTextBoxExtended)_Need_Assistant_Control).SelectionStart;    //记录修改时光标的位置

                        ((RichTextBoxExtended)_Need_Assistant_Control).Focus();
                        string insert = strInsert;
                        ((RichTextBoxExtended)_Need_Assistant_Control).Text = ((RichTextBoxExtended)_Need_Assistant_Control).Text.Insert(index, insert);
                        ((RichTextBoxExtended)_Need_Assistant_Control).Select(index + insert.Length, 0);

                        ((RichTextBoxExtended)_Need_Assistant_Control).SelectionAlignment = temp;  //不然会默认居左的，要保留之前的对齐方式

                        _Need_Assistant_Control.Focus();

                        //ShowSearch(_Need_Assistant_Control); // 显示指示图标
                    }
                }
            }
        }

        internal void SaveAs()
        {
            //this.pictureBoxBackGround.Focus();

            if (_TemplateName == "" || _RecruitXML == null)
            {
                MessageBox.Show("没有打开任何表单，无法进行另存为。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((sfdSaveAsXml.ShowDialog() == DialogResult.OK) && (sfdSaveAsXml.FileName.Length > 0))
            {
                try
                {
                    string filePath = sfdSaveAsXml.FileName;
                    this.pictureBoxBackGround.Focus();
                    //_IsLoading = true;
                    _IsSaveingNow = true;

                    if (_RecruitXML == null || string.IsNullOrEmpty(_RecruitXML.InnerXml) || _RecruitXML.SelectSingleNode(nameof(EntXmlModel.NurseForm)) == null)   //XmlDocument aaa = new XmlDocument(); .InnerXml为空
                    {
                        //set FORM='<?xml version="1.0" encoding="utf-8"?><NurseForm UHID="2[孙彦隽]343381320171" Name="01,伍群昊,男,4个月" CreatUserID="4794" CreatUserDepartMent="胸外监护病区护理组" CreatDateTime="2017 07 01 14 34 24 4243"><Pages><Page SN="1"'
                        //where FORM_NAME='一般护理记录单'
                        //格式破坏的xml，原来导出来就是空了。现在用_StrXmlData数据库获取的字符串导出为文本文件
                        filePath += ".txt";
                        using (StreamWriter w = File.AppendText(filePath))
                        {
                            w.Write(_StrXmlData);
                            w.Flush();
                            w.Close();
                        }
                    }
                    else
                    {
                        SaveToOp();

                        //执行和保存一样的操作，将数据更新到Xml中
                        SaveThisPageDataToXml();

                        if (_RecruitXML != null)
                        {
                            _RecruitXML.Save(filePath);
                        }
                    }

                    _IsSaveingNow = false;
                }
                catch (Exception ex)
                {
                    _IsSaveingNow = false;
                    Comm.LogHelp.WriteErr(ex);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 脚本表达式，现有的执行语句功能简单，不能含有业务，如：弹出消息和传入this的控件赋值等操作。不设计业务
        /// 如果是业务方法，比如创建表单等，在这里扩展
        /// </summary>
        /// <param name="strScript"></param>
        internal void ScriptOwnMethod(string strScript)
        {
            //如果是创建某表单:：评分.Text 小于 12 Then Creat(神经内科护理记录单)

            //<Action Name="评分" Event="SavedChanged">评分.Text &lt; 12 Then Creat(神经内科护理记录单¤评分¤压疮评分)</Action>
            if (strScript.Contains("Creat("))
            {
                int a = 0;
                int b = 0;

                //获取消息内容
                //a = xn.InnerText.Trim().IndexOf("(");
                //b = xn.InnerText.Trim().IndexOf(")");
                a = strScript.IndexOf("(");
                b = strScript.IndexOf(")");

                if (a < b && b - a - 1 > 0)
                {
                    string templateNameAndPara = strScript.Substring(a + 1, b - a - 1);
                    string[] arr = templateNameAndPara.Split('¤');
                    string templateName = arr[0]; //需要创建的表单名¤本页取值输入框名¤创建页传值输入框名

                    //寻找创建表单菜单中的指定项目：(和创建菜单的可创建表单一致，否则不创建)
                    //ToolStripItem[] tsArr = toolStripTemplate.DropDownItems.Find(templateName, true);//可创建的表单集合

                    if (!GlobalVariable._ListCreatDoc.Contains(templateName))
                    {
                        //找不到，不处理
                    }
                    else
                    {
                        if (MessageBox.Show("需要创建：《" + templateName + "》吗？", "确认消息", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            //if (arr.Length >= 3)
                            //{
                            //    Control[] cl = this.pictureBoxBackGround.Controls.Find(arr[1], false);
                            //    if (cl != null && cl.Length > 0)
                            //    {
                            //        _CreatTemplatePara = cl[0].Text.Trim() + "¤" + arr[2]; //12分¤压疮评分
                            //    }
                            //    else
                            //    {
                            //        _CreatTemplatePara = "";
                            //        ShowInforMsg("找不到创建表单是的传参控件：" + arr[1], true);
                            //    }
                            //}
                            //else
                            //{
                            //    _CreatTemplatePara = "";
                            //}

                            ////TemplateMenu_Click(tsArr[0], null);
                            //SetDefaultOpenUHID(this._patientInfo.Id, templateName);

                            //TODO:回调主窗体方法，创建该表单
                            if (GlobalVariable._CallBackCreatBook != null)
                            {
                                GlobalVariable._CallBackCreatBook(templateName, null);
                            }
                        }
                    }
                }
            }
        }

        internal void InitTemplate(List<GlobalVariable.Template> list)
        {
            TemplateList = list;
            InitTemplate();
        }

        internal void InitTemplate()
        {
            toolStripButtonCreateForm.DropDownItems.Clear();
            if (this.TemplateList != null && this.TemplateList.Count > 0)
            {
                var list1 = this.TemplateList.Where(a => a.Memo.IsNullOrEmpty())
                                       .Select(a => new GlobalVariable.Template(a.ID, a.Name)).OrderBy(a => a.Name).ToList();
                var list2 = this.TemplateList.Where(a => !a.Memo.IsNullOrEmpty()).Select(a => a.Memo).OrderBy(a => a).Distinct()
                                         .Select(a => new GlobalVariable.Template("", a)).ToList();
                foreach (var item in list2)
                {
                    var list3 = this.TemplateList.Where(a => a.Memo == item.Name)
                                    .Select(a => new GlobalVariable.Template(a.ID, a.Name)).OrderBy(a => a.Name).ToList();
                    item.Child.AddRange(list3);
                }
                list2 = list2.Concat(list1).ToList();
                Init(toolStripButtonCreateForm, list2);
            }
            else
            {
                toolStripButtonCreateForm.DropDownItems.Clear();
            }
        }


        private void Init(ToolStripDropDownItem menu, List<GlobalVariable.Template> list)
        {
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    ToolStripMenuItem tool = new ToolStripMenuItem(item.Name)
                    {
                        Tag = item,
                    };
                    if (item.Child != null && item.Child.Count > 0)
                    {
                        Init(tool, item.Child);
                    }
                    else
                    {
                        tool.Click += Template_Click;
                    }
                    menu.DropDownItems.Add(tool);
                }
            }
        }

        private void Template_Click(object sender, EventArgs e)
        {
            var menu = sender as ToolStripItem;
            if (menu != null && menu.Tag is GlobalVariable.Template temp && !temp.ID.IsNullOrEmpty())
            {
                bool isNew = true;
                if (IsNeedSave)
                {
                    var ret = Comm.ShowQuestionMessage(string.Format("当前表单发生改变，是否保存后再创建?{0}按\"是\"保存后新建{0}按\"否\"不保存直接新建{0}按\"取消\"则返回操作界面）", Environment.NewLine), "提示", 3);
                    if (ret == DialogResult.Yes)
                    {
                        isNew = SaveData() == 1;
                    }
                    else if (ret == DialogResult.No)
                    {
                        isNew = true;
                    }
                    else
                    {
                        isNew = false;
                    }
                }
                if (isNew)
                {
                    if (this.CreateNewForm != null)
                    {
                        //var temp = menu.Tag as GlobalVariable.Template;
                        this.CreateNewForm(this, new GlobalVariable.NewFormEventArg(this._TemplateName, this._TemplateID, temp.Name, temp.ID, this._patientInfo));
                    }
                }
            }


        }


        internal EntUserInfoEx Login(string userid, string password)
        {
            if (LoginVerify != null)
            {
                return LoginVerify(userid, password);
            }
            return null;
        }


        internal void ClearControls()
        {
            //如果重新加载模板页，那么要先清空已经加载的控件的         
            //foreach (Control cl in this.pictureBoxBackGround.Controls) //不一定都能释放掉
            for (int i = this.pictureBoxBackGround.Controls.Count - 1; i >= 0; i--)
            {
                if (this.pictureBoxBackGround.Controls[i] != this.lblShowSearch)
                {
                    if (this.pictureBoxBackGround.Controls[i] != null && !this.pictureBoxBackGround.Controls[i].IsDisposed)
                    {
                        this.pictureBoxBackGround.Controls[i].Dispose();
                    }
                }
            }

            this.pictureBoxBackGround.Controls.Clear();//清空所有控件     //并没有释放内存，必须dispose  
            ClearMe();
            System.GC.Collect();
        }
    }
}
