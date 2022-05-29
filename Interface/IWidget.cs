using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SparkFormEditor
{
    internal interface IWidget
    {
        /// <summary>
        /// 设置皮肤，以便减轻视觉疲劳
        /// </summary>
        Color WidgetSkinColor { set; }
        /// <summary>
        /// 设置或获取个性化配置
        /// </summary>
        string WidgetConfig { set; get; }
        /// <summary>
        /// 设置用户所属单位
        /// </summary>
        string WidgetUnit { set; }
        /// <summary>
        /// 设置用户名
        /// </summary>
        string WidgetUsername { set; }
        /// <summary>
        /// 设置用户密码
        /// </summary>
        string WidgetPassword { set; }
        /// <summary>
        /// 设置所属科室列表
        /// </summary>
        string[] WidgetOffices { set; }

        /// <summary>
        /// 各种工具箱触发的“应用”事件
        /// </summary>
        event EventHandler WidgetApply;
    }
}
