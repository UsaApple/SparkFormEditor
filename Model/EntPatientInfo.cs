using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SparkFormEditor.Model
{
    /// <summary>
    /// 病人信息实体
    /// </summary>
    public class EntPatientInfoEx : Foundation.EntPatientInfo
    {
        private Dictionary<string, object> _dict;
        private DataRow _patientOtherInfo;

        internal Dictionary<string, object> Dict
        {
            get
            {
                if (_dict == null || _dict.Count == 0)
                {
                    _dict = Comm.EntToDict(this);
                    DrToDict();
                }
                return _dict;
            }
        }

        /// <summary>
        /// 病人额外信息的DataRow数据，用来赋值表单上的动态标签
        /// </summary>
        public DataRow PatientOtherInfo
        {
            get
            {
                return this._patientOtherInfo;
            }
            set
            {
                this._patientOtherInfo = value;
                if (_dict != null && _dict.Any() && value != null)
                {
                    DrToDict();
                }
            }
        }

        private void DrToDict()
        {
            if (_patientOtherInfo != null)
            {
                if (_dict == null) _dict = new Dictionary<string, object>();
                foreach (DataColumn col in _patientOtherInfo.Table.Columns)
                {
                    if (_dict.ContainsKey(col.ColumnName))
                    {
                        _dict[col.ColumnName] = _patientOtherInfo[col.ColumnName];
                    }
                    else
                    {
                        _dict.Add(col.ColumnName, _patientOtherInfo[col.ColumnName]);
                    }
                }
            }
        }

        private void SetDictValue(string name, object value)
        {
            if (_dict != null && _dict.ContainsKey(name))
            {
                _dict[name] = value;
            }
        }
    }
}
