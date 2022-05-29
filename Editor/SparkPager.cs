using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace SparkFormEditor
{
    /// <summary>
    /// ����ί��
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    internal delegate int EventPagingHandler(EventPagingArg e);

    /// <summary>
    /// ��ҳ��ί��
    /// </summary>
    public delegate bool EventFlipHandler(int iPage);

    /// <summary>
    /// ��ҳ�ؼ�����
    /// </summary>
    internal partial class SparkPager : UserControl
    {
        public SparkPager()
        {
            InitializeComponent();
            this.Enabled = false;
        }
        public event EventPagingHandler EventPaging;

        /// <summary>
        /// ��ҳ���¼�
        /// </summary>
        public event EventFlipHandler EventFlip;
        /// <summary>
        /// ÿҳ��ʾ��¼��
        /// </summary>
        private int _pageSize = 20;
        /// <summary>
        /// ÿҳ��ʾ��¼��
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                _pageSize = value;
                GetPageCount();
            }
        }

        private int _nMax = 0;
        /// <summary>
        /// �ܼ�¼��
        /// </summary>
        public int NMax
        {
            get { return _nMax; }
            set
            {
                _nMax = value;
                //GetPageCount();
            }
        }

        private int _pageCount = 0;
        /// <summary>
        /// ҳ��=�ܼ�¼��/ÿҳ��ʾ��¼��
        /// </summary>
        public int PageCount
        {
            get { return _pageCount; }
            set { _pageCount = value; }
        }

        private int _pageCurrent = 0;
        /// <summary>
        /// ��ǰҳ��
        /// </summary>
        public int PageCurrent
        {
            get { return _pageCurrent; }
            set { _pageCurrent = value; }
        }

        public BindingNavigator ToolBar
        {
            get { return this.bindingNavigator; }
        }

        private void GetPageCount()
        {
            if (this.NMax > 0)
            {
                this.PageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(this.NMax) / Convert.ToDouble(this.PageSize)));
            }
            else
            {
                this.PageCount = 0;
            }
        }

        public void SetMaxCount(int iCount)
        {
            lblPageCount.Text = iCount.ToString();
        }

        /// <summary>
        /// ��ҳ�ؼ����ݰ󶨵ķ���
        /// </summary>
        public void Bind()
        {
            int temp;
            if (!int.TryParse(this.lblPageCount.Text, out temp))
            {
                return;
            }
            this.Enabled = true;
            //if (this.EventPaging != null)
            //{
            //    this.NMax = this.EventPaging(new EventPagingArg(this.PageCurrent));
            //}

            //if (this.PageCurrent > this.PageCount)
            //{
            //    this.PageCurrent = this.PageCount;
            //}
            //if (this.PageCount == 1)
            //{
            //    this.PageCurrent = 1;
            //}
            this.NMax = this.PageCurrent;

            //lblPageCount.Text = this.PageCount.ToString();
            //if (this.EventFlip == null) lblPageCount.Text = this.NMax.ToString();//�¼�δ��ע��ʱ��ֵ���ҳ��
            //this.lblMaxPage.Text = "��"+this.NMax.ToString()+"����¼";

            //if (this.EventFlip != null)
            //{
            //    bool flag = EventFlip();
            //    if (!flag)
            //        return;
            //}

            this.txtCurrentPage.Text = this.PageCurrent.ToString();


            if (this.PageCurrent == 1)
            {
                this.btnPrev.Enabled = false;
                this.btnFirst.Enabled = false;
            }
            else
            {
                btnPrev.Enabled = true;
                btnFirst.Enabled = true;
            }

            //if (this.PageCurrent == this.PageCount)
            if (this.PageCurrent == int.Parse(this.lblPageCount.Text))
            {
                this.btnLast.Enabled = false;
                this.btnNext.Enabled = false;
            }
            else
            {
                btnLast.Enabled = true;
                btnNext.Enabled = true;
            }

            if (this.NMax == 0)
            {
                btnNext.Enabled = false;
                btnLast.Enabled = false;
                btnFirst.Enabled = false;
                btnPrev.Enabled = false;
            }

        }


        public void SetInit()
        {
            txtCurrentPage.Text = "";
            lblPageCount.Text = "lblPageCount";
            this.Enabled = false;
            _pageCurrent = 0;
            _pageCount = 0;
            _nMax = 0;
        }

        public void ReSetAttribute(int iPageGo)
        {
            //lblPageCount.Text = this.PageCount.ToString();
            //txtCurrentPage.Text = this.PageCount.ToString();
            lblPageCount.Text = iPageGo.ToString();
            txtCurrentPage.Text = iPageGo.ToString();
            PageCurrent = iPageGo;
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            if (this.EventFlip != null)
            {
                bool flag = EventFlip(1);
                if (!flag)
                    return;
            }
            PageCurrent = 1;
            this.Bind();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (this.EventFlip != null)
            {
                bool flag = EventFlip(PageCurrent - 1);
                if (!flag)
                    return;
            }
            //PageCurrent -= 1;
            if (PageCurrent <= 0)
            {
                PageCurrent = 1;
            }
            this.Bind();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (this.EventFlip != null)
            {
                bool flag = EventFlip(PageCurrent + 1);
                if (!flag)
                    return;
            }
            //this.PageCurrent += 1;
            //if (PageCurrent > PageCount)
            //{
            //    PageCurrent = PageCount;
            //}
            if (PageCurrent > int.Parse(this.lblPageCount.Text))
            {
                PageCurrent = int.Parse(this.lblPageCount.Text);
            }
            this.Bind();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            if (this.EventFlip != null)
            {
                bool flag = EventFlip(int.Parse(this.lblPageCount.Text));
                if (!flag)
                    return;
            }
            //PageCurrent = PageCount;
            PageCurrent = int.Parse(this.lblPageCount.Text);
            this.Bind();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {

            if (this.txtCurrentPage.Text != null && txtCurrentPage.Text != "")
            {
                int iPageGo = -1;
                if (Int32.TryParse(txtCurrentPage.Text, out iPageGo))
                {
                    if (iPageGo <= 0)
                    {
                        Comm.ShowWarningMessage(string.Format("ҳ���������0"));
                        return;
                    }

                    if (iPageGo > int.Parse(this.lblPageCount.Text))
                    {
                        Comm.ShowWarningMessage(string.Format("��ǰ���ĵĵ�{0}ҳ���ݻ�δ�����������½�ҳ", iPageGo));
                        this.txtCurrentPage.Focus();
                        return;
                    }
                    //else if (iPageGo == int.Parse(this.lblPageCount.Text))
                    //{
                    //    return;
                    //}

                    if (this.EventFlip != null)
                    {
                        //bool flag = EventFlip(_pageCurrent);
                        bool flag = EventFlip(int.Parse(this.txtCurrentPage.Text));
                        if (!flag)
                            return;
                    }

                    this.Bind();
                }
                else
                {
                    Comm.ShowErrorMessage("�������ָ�ʽ����");
                }
            }
        }

        private void txtCurrentPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //this.Bind();
                btnGo_Click(null, null);
            }
        }

    }
    /// <summary>
    /// �Զ����¼����ݻ���
    /// </summary>
    internal class EventPagingArg : EventArgs
    {
        private int _intPageIndex;
        public EventPagingArg(int PageIndex)
        {
            _intPageIndex = PageIndex;
        }
    }
    /// <summary>
    /// ����Դ�ṩ
    /// </summary>
    internal class PageData
    {
        private int _PageSize = 10;
        private int _PageIndex = 1;
        private int _PageCount = 0;
        private int _TotalCount = 0;
        private string _TableName;//����
        private string _QueryFieldName = "*";//���ֶ�FieldStr
        private string _OrderStr = string.Empty; //����_SortStr
        private string _QueryCondition = string.Empty;//��ѯ������ RowFilter
        private string _PrimaryKey = string.Empty;//����
        private bool _isQueryTotalCounts = true;//�Ƿ��ѯ�ܵļ�¼����
        /// <summary>
        /// �Ƿ��ѯ�ܵļ�¼����
        /// </summary>
        public bool IsQueryTotalCounts
        {
            get { return _isQueryTotalCounts; }
            set { _isQueryTotalCounts = value; }
        }
        /// <summary>
        /// ��ʾҳ��
        /// </summary>
        public int PageSize
        {
            get
            {
                return _PageSize;

            }
            set
            {
                _PageSize = value;
            }
        }
        /// <summary>
        /// ��ǰҳ
        /// </summary>
        public int PageIndex
        {
            get
            {
                return _PageIndex;
            }
            set
            {
                _PageIndex = value;
            }
        }
        /// <summary>
        /// ��ҳ��
        /// </summary>
        public int PageCount
        {
            get
            {
                return _PageCount;
            }
        }
        /// <summary>
        /// �ܼ�¼��
        /// </summary>
        public int TotalCount
        {
            get
            {
                return _TotalCount;
            }
        }
        /// <summary>
        /// ������������ͼ
        /// </summary>
        public string TableName
        {
            get
            {
                return _TableName;
            }
            set
            {
                _TableName = value;
            }
        }
        /// <summary>
        /// ���ֶ�FieldStr
        /// </summary>
        public string QueryFieldName
        {
            get
            {
                return _QueryFieldName;
            }
            set
            {
                _QueryFieldName = value;
            }
        }
        /// <summary>
        /// �����ֶ�
        /// </summary>
        public string OrderStr
        {
            get
            {
                return _OrderStr;
            }
            set
            {
                _OrderStr = value;
            }
        }
        /// <summary>
        /// ��ѯ����
        /// </summary>
        public string QueryCondition
        {
            get
            {
                return _QueryCondition;
            }
            set
            {
                _QueryCondition = value;
            }
        }
        /// <summary>
        /// ����
        /// </summary>
        public string PrimaryKey
        {
            get
            {
                return _PrimaryKey;
            }
            set
            {
                _PrimaryKey = value;
            }
        }
        public DataSet QueryDataTable()
        {
            //SqlParameter[] parameters = {
            //        new SqlParameter("@Tables", SqlDbType.VarChar, 255),
            //        new SqlParameter("@PrimaryKey" , SqlDbType.VarChar , 255),	
            //        new SqlParameter("@Sort", SqlDbType.VarChar , 255 ),
            //        new SqlParameter("@CurrentPage", SqlDbType.Int),
            //        new SqlParameter("@PageSize", SqlDbType.Int),									
            //        new SqlParameter("@Fields", SqlDbType.VarChar, 255),
            //        new SqlParameter("@Filter", SqlDbType.VarChar,1000),
            //        new SqlParameter("@Group" ,SqlDbType.VarChar , 1000 )
            //        };
            //parameters[0].Value = _TableName;
            //parameters[1].Value = _PrimaryKey;
            //parameters[2].Value = _OrderStr;
            //parameters[3].Value = PageIndex;
            //parameters[4].Value = PageSize;
            //parameters[5].Value =_QueryFieldName;
            //parameters[6].Value = _QueryCondition;
            //parameters[7].Value = string.Empty;
            //DataSet ds = DbHelperSQL.RunProcedure("SP_Pagination", parameters, "dd");
            //if (_isQueryTotalCounts)
            //{
            //    _TotalCount = GetTotalCount();
            //}
            //if (_TotalCount == 0)
            //{
            //    _PageIndex = 0;
            //    _PageCount = 0;
            //}
            //else
            //{
            //    _PageCount = _TotalCount % _PageSize == 0 ? _TotalCount / _PageSize : _TotalCount / _PageSize + 1;
            //    if (_PageIndex > _PageCount)
            //    {
            //        _PageIndex = _PageCount;

            //        parameters[4].Value = _PageSize;

            //        ds = QueryDataTable();
            //    }
            //}
            //return ds;
            return null;
        }

        public int GetTotalCount()
        {
            //string strSql = " select count(1) from "+_TableName;
            //if (_QueryCondition != string.Empty)
            //{
            //    strSql +=" where " + _QueryCondition;
            //}
            //return int.Parse(DbHelperSQL.GetSingle(strSql).ToString());
            return 10;
        }
    }
}
