using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace SparkFormEditor
{
    /// <summary>
    /// 遗传图谱的封装控件
    /// </summary>
    internal partial class GeneticMapControl : PictureBox
    {
        public string _Data = "";

        public GeneticMapControl()
        {
            InitializeComponent();
        }

        public string GetSaveData()
        {
            return _Data;
        }

        public void ResetClear()
        {
            _Data = "";
            this.BackgroundImage = new Bitmap(this.Width, this.Height);

        }
    }
}
