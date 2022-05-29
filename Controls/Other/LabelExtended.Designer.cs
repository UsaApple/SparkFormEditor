namespace SparkFormEditor
{
    partial class LabelExtended
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();

                _Default = null; //默认值
                _Score = null; //评分表达式
                _YMD = null;   //入院日期的格式，可能需要时分秒
                _Size = null;   //配置的大小，以便回行处理的
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // LabelExtended
            // 
            this.TextChanged += new System.EventHandler(this.LabelExtended_TextChanged);
            this.SizeChanged += new System.EventHandler(this.LabelExtended_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
