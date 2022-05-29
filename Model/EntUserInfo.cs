using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SparkFormEditor.Model
{
    /// <summary>
    /// 用户登录信息实体
    /// </summary>
    public class EntUserInfoEx : Foundation.UserObject
    {
        protected EntUserInfoEx() { }

        public EntUserInfoEx(Foundation.UserObject userObject)
        {
            if (userObject != null)
            {
                this.UserCode = userObject.UserCode;
                this.UserName = userObject.UserName;
                this.DeptCode = userObject.DeptCode;
                this.DeptName = userObject.DeptName;
                this.WardCode = userObject.WardCode;
                this.WardName = userObject.WardName;
                this.OrganCode = userObject.OrganCode;
                this.OrganName = userObject.OrganName;
                this.Password = userObject.Password;
                this.PositionCode = userObject.PositionCode;
                this.PositionName = userObject.PositionName;
                this.TitleCode = userObject.TitleCode;
                this.TitleName = userObject.TitleName;
                this.FullData = userObject.FullData;
            }
        }

        public EntUserInfoEx(DataRow dr)
        {
            UserCode = dr["USERCODE"].ToString();
            UserName = dr["USERNAME"].ToString();
            Password = dr["PASSWORD"].ToString();
            PositionName = dr["POSITION_NAME"].ToString();
            TitleName = dr["TITLE_NAME"].ToString();
            OrganCode = dr["ORGAN_CODE"].ToString();
            OrganName = dr["ORGAN_NAME"].ToString();
            DeptCode = dr["DEPT_CODE"].ToString();
            DeptName = dr["DEPT_NAME"].ToString();
            WardCode = dr["WARD_CODE"].ToString();
            WardName = dr["WARD_NAME"].ToString();
        }

        /// <summary>
        /// 医生还是护士判断，有护就表示护士，其他都当成医生
        /// </summary>
        public string Type
        {
            get { return (!string.IsNullOrEmpty(base.TitleName) && base.TitleName.IndexOf("护") >= 0) ? "护士" : "医生"; }
        }
    }
}
