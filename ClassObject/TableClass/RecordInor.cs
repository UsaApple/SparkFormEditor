using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SparkFormEditor
{
    //表单更新到体温单
    //一个List<RecordInor> 来存储每次修改过后的行，在切换表单的时候清空该List，保存的时候提交该List并清空重置。
    /// <summary>
    /// 表单修改过后的行信息
    /// </summary>
    internal class RecordInor
    {
        //同步小结总结的出入量的SynchronizeTemperature  (SynchronizeTemperature="总入量@入量¤总出量@出量¤尿量@小便¤静脉@静脉")
        //同步体温等生命体征SynchronizeTemperatureRecord="体温,脉搏,呼吸"
        //DefaultTemperatureType="Y"
        //public string DefaultType = ""; //体温默认类型：口表、腋表、肛表、耳表
        public string ID = null;        //关联表格的ID，判断该行数据，时候已经记录在缓存的List<RecordInor>中
        public string Date = null;      //日期
        public string Time = null;      //时间

        //其他项目
        public Dictionary<string, string> DicItems = new Dictionary<string, string>();   //键值对：体温 37.5

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <param name="time"></param>
        public RecordInor(string id, string date, string time)
        {
            this.ID = id;
            this.Date = date;
            this.Time = time;
        }

        public void AddItem(string key, string value)
        {
            if (DicItems.ContainsKey(key))   //DicItems[key] != null
            {
                DicItems[key] = value;
            }
            else
            {
                DicItems.Add(key, value);
            }
        }

        public string GetItem(string key)
        {
            if (DicItems.ContainsKey(key))   //DicItems[key] != null
            {
                return DicItems[key];
            }
            else
            {
                return null;
            }
        }
    }

}
