namespace SparkFormEditor
{
    partial class Frm_MergeCell
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.radioButtonRight = new System.Windows.Forms.RadioButton();
            this.radioButtonDown = new System.Windows.Forms.RadioButton();
            this.numericUpDownRight = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownDown = new System.Windows.Forms.NumericUpDown();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancle = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDown)).BeginInit();
            this.SuspendLayout();
            // 
            // radioButtonRight
            // 
            this.radioButtonRight.AutoSize = true;
            this.radioButtonRight.Location = new System.Drawing.Point(16, 26);
            this.radioButtonRight.Name = "radioButtonRight";
            this.radioButtonRight.Size = new System.Drawing.Size(71, 16);
            this.radioButtonRight.TabIndex = 0;
            this.radioButtonRight.TabStop = true;
            this.radioButtonRight.Text = "向右合并";
            this.radioButtonRight.UseVisualStyleBackColor = true;
            this.radioButtonRight.CheckedChanged += new System.EventHandler(this.radioButtonRight_CheckedChanged);
            // 
            // radioButtonDown
            // 
            this.radioButtonDown.AutoSize = true;
            this.radioButtonDown.Location = new System.Drawing.Point(16, 69);
            this.radioButtonDown.Name = "radioButtonDown";
            this.radioButtonDown.Size = new System.Drawing.Size(71, 16);
            this.radioButtonDown.TabIndex = 1;
            this.radioButtonDown.TabStop = true;
            this.radioButtonDown.Text = "向下合并";
            this.radioButtonDown.UseVisualStyleBackColor = true;
            this.radioButtonDown.CheckedChanged += new System.EventHandler(this.radioButtonDown_CheckedChanged);
            // 
            // numericUpDownRight
            // 
            this.numericUpDownRight.Location = new System.Drawing.Point(108, 24);
            this.numericUpDownRight.Name = "numericUpDownRight";
            this.numericUpDownRight.Size = new System.Drawing.Size(55, 21);
            this.numericUpDownRight.TabIndex = 2;
            this.numericUpDownRight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // numericUpDownDown
            // 
            this.numericUpDownDown.Location = new System.Drawing.Point(108, 67);
            this.numericUpDownDown.Name = "numericUpDownDown";
            this.numericUpDownDown.Size = new System.Drawing.Size(55, 21);
            this.numericUpDownDown.TabIndex = 3;
            this.numericUpDownDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDownDown.Click += new System.EventHandler(this.numericUpDownDown_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(198, 23);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(102, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "确定";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancle
            // 
            this.buttonCancle.Location = new System.Drawing.Point(198, 64);
            this.buttonCancle.Name = "buttonCancle";
            this.buttonCancle.Size = new System.Drawing.Size(102, 23);
            this.buttonCancle.TabIndex = 5;
            this.buttonCancle.Text = "取消";
            this.buttonCancle.UseVisualStyleBackColor = true;
            this.buttonCancle.Click += new System.EventHandler(this.buttonCancle_Click);
            // 
            // MergeCellForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(334, 121);
            this.Controls.Add(this.buttonCancle);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.numericUpDownDown);
            this.Controls.Add(this.numericUpDownRight);
            this.Controls.Add(this.radioButtonDown);
            this.Controls.Add(this.radioButtonRight);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MergeCellForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "合并单元格";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonRight;
        private System.Windows.Forms.RadioButton radioButtonDown;
        private System.Windows.Forms.NumericUpDown numericUpDownRight;
        private System.Windows.Forms.NumericUpDown numericUpDownDown;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancle;
    }
}