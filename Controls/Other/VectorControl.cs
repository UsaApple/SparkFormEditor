using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SparkFormEditor.Foundation;

namespace SparkFormEditor
{
    internal partial class VectorControl : PictureBox
    {
        public VectorImageInfo VectorImageInfo { get; set; }

        internal string PicId
        {
            get
            {
                return VectorImageInfo != null ? VectorImageInfo.PicId : "";
            }
        }

        public VectorControl()
        {
            InitializeComponent();
        }

        public string GetSaveStr()
        {
            return this.PicId;
            //return _VectorConfig + "§" + _ImageBorder;
        }
    }
}
