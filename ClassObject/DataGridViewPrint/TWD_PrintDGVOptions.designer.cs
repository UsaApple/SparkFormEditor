namespace NurseForm.Editor.Ctrl
{
    partial class PrintOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintOptions));
            this.rdoSelectedRows = new System.Windows.Forms.RadioButton();
            this.ctlPrintAllRowsRBTN = new System.Windows.Forms.RadioButton();
            this.ctlPrintToFitPageWidthCHK = new System.Windows.Forms.CheckBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.ctlPrintTitleTBX = new System.Windows.Forms.TextBox();
            this.gboxRowsToPrint = new System.Windows.Forms.GroupBox();
            this.lblColumnsToPrint = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.ctlColumnsToPrintCHKLBX = new System.Windows.Forms.CheckedListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.checkBox1Page2 = new System.Windows.Forms.CheckBox();
            this.gboxRowsToPrint.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rdoSelectedRows
            // 
            this.rdoSelectedRows.AutoSize = true;
            this.rdoSelectedRows.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoSelectedRows.Location = new System.Drawing.Point(91, 18);
            this.rdoSelectedRows.Name = "rdoSelectedRows";
            this.rdoSelectedRows.Size = new System.Drawing.Size(77, 17);
            this.rdoSelectedRows.TabIndex = 1;
            this.rdoSelectedRows.TabStop = true;
            this.rdoSelectedRows.Text = "选择的行";
            this.rdoSelectedRows.UseVisualStyleBackColor = true;
            // 
            // ctlPrintAllRowsRBTN
            // 
            this.ctlPrintAllRowsRBTN.AutoSize = true;
            this.ctlPrintAllRowsRBTN.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ctlPrintAllRowsRBTN.Location = new System.Drawing.Point(9, 18);
            this.ctlPrintAllRowsRBTN.Name = "ctlPrintAllRowsRBTN";
            this.ctlPrintAllRowsRBTN.Size = new System.Drawing.Size(51, 17);
            this.ctlPrintAllRowsRBTN.TabIndex = 0;
            this.ctlPrintAllRowsRBTN.TabStop = true;
            this.ctlPrintAllRowsRBTN.Text = "全部";
            this.ctlPrintAllRowsRBTN.UseVisualStyleBackColor = true;
            // 
            // ctlPrintToFitPageWidthCHK
            // 
            this.ctlPrintToFitPageWidthCHK.AutoSize = true;
            this.ctlPrintToFitPageWidthCHK.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ctlPrintToFitPageWidthCHK.Checked = true;
            this.ctlPrintToFitPageWidthCHK.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ctlPrintToFitPageWidthCHK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ctlPrintToFitPageWidthCHK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ctlPrintToFitPageWidthCHK.Location = new System.Drawing.Point(195, 95);
            this.ctlPrintToFitPageWidthCHK.Name = "ctlPrintToFitPageWidthCHK";
            this.ctlPrintToFitPageWidthCHK.Size = new System.Drawing.Size(149, 18);
            this.ctlPrintToFitPageWidthCHK.TabIndex = 21;
            this.ctlPrintToFitPageWidthCHK.Text = "列宽根据纸宽自适应";
            this.ctlPrintToFitPageWidthCHK.UseVisualStyleBackColor = true;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(192, 159);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(59, 13);
            this.lblTitle.TabIndex = 20;
            this.lblTitle.Text = "打印标题";
            // 
            // ctlPrintTitleTBX
            // 
            this.ctlPrintTitleTBX.AcceptsReturn = true;
            this.ctlPrintTitleTBX.Location = new System.Drawing.Point(193, 185);
            this.ctlPrintTitleTBX.Multiline = true;
            this.ctlPrintTitleTBX.Name = "ctlPrintTitleTBX";
            this.ctlPrintTitleTBX.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ctlPrintTitleTBX.Size = new System.Drawing.Size(185, 38);
            this.ctlPrintTitleTBX.TabIndex = 19;
            // 
            // gboxRowsToPrint
            // 
            this.gboxRowsToPrint.Controls.Add(this.rdoSelectedRows);
            this.gboxRowsToPrint.Controls.Add(this.ctlPrintAllRowsRBTN);
            this.gboxRowsToPrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gboxRowsToPrint.Location = new System.Drawing.Point(193, 42);
            this.gboxRowsToPrint.Name = "gboxRowsToPrint";
            this.gboxRowsToPrint.Size = new System.Drawing.Size(173, 39);
            this.gboxRowsToPrint.TabIndex = 18;
            this.gboxRowsToPrint.TabStop = false;
            this.gboxRowsToPrint.Text = "要打印的行";
            this.gboxRowsToPrint.Visible = false;
            // 
            // lblColumnsToPrint
            // 
            this.lblColumnsToPrint.AutoSize = true;
            this.lblColumnsToPrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblColumnsToPrint.Location = new System.Drawing.Point(8, 8);
            this.lblColumnsToPrint.Name = "lblColumnsToPrint";
            this.lblColumnsToPrint.Size = new System.Drawing.Size(85, 13);
            this.lblColumnsToPrint.TabIndex = 17;
            this.lblColumnsToPrint.Text = "要打印的列：";
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.SystemColors.Control;
            this.btnOK.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.btnOK.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnOK.Image = ((System.Drawing.Image)(resources.GetObject("btnOK.Image")));
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnOK.Location = new System.Drawing.Point(194, 242);
            this.btnOK.Name = "btnOK";
            this.btnOK.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnOK.Size = new System.Drawing.Size(56, 23);
            this.btnOK.TabIndex = 14;
            this.btnOK.Text = "&确定";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.SystemColors.Control;
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.btnCancel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.Location = new System.Drawing.Point(313, 242);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnCancel.Size = new System.Drawing.Size(56, 23);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "&取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ctlColumnsToPrintCHKLBX
            // 
            this.ctlColumnsToPrintCHKLBX.CheckOnClick = true;
            this.ctlColumnsToPrintCHKLBX.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ctlColumnsToPrintCHKLBX.FormattingEnabled = true;
            this.ctlColumnsToPrintCHKLBX.Location = new System.Drawing.Point(8, 26);
            this.ctlColumnsToPrintCHKLBX.Name = "ctlColumnsToPrintCHKLBX";
            this.ctlColumnsToPrintCHKLBX.Size = new System.Drawing.Size(170, 244);
            this.ctlColumnsToPrintCHKLBX.TabIndex = 13;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(193, 29);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(173, 52);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "选择";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(82, 18);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(59, 16);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "全不选";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.Click += new System.EventHandler(this.radioButton2_Click);
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(11, 17);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(47, 16);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "全选";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.Click += new System.EventHandler(this.radioButton1_Click);
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // checkBox1Page2
            // 
            this.checkBox1Page2.AutoSize = true;
            this.checkBox1Page2.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox1Page2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBox1Page2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox1Page2.Location = new System.Drawing.Point(195, 125);
            this.checkBox1Page2.Name = "checkBox1Page2";
            this.checkBox1Page2.Size = new System.Drawing.Size(149, 18);
            this.checkBox1Page2.TabIndex = 22;
            this.checkBox1Page2.Text = "一页合并两列后打印";
            this.checkBox1Page2.UseVisualStyleBackColor = true;
            // 
            // PrintOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 277);
            this.Controls.Add(this.checkBox1Page2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ctlPrintToFitPageWidthCHK);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.ctlPrintTitleTBX);
            this.Controls.Add(this.gboxRowsToPrint);
            this.Controls.Add(this.lblColumnsToPrint);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.ctlColumnsToPrintCHKLBX);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PrintOptions";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "自定义打印设置";
            this.Load += new System.EventHandler(this.OnLoadForm);
            this.gboxRowsToPrint.ResumeLayout(false);
            this.gboxRowsToPrint.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.RadioButton rdoSelectedRows;
        internal System.Windows.Forms.RadioButton ctlPrintAllRowsRBTN;
        internal System.Windows.Forms.CheckBox ctlPrintToFitPageWidthCHK;
        internal System.Windows.Forms.Label lblTitle;
        internal System.Windows.Forms.TextBox ctlPrintTitleTBX;
        internal System.Windows.Forms.GroupBox gboxRowsToPrint;
        internal System.Windows.Forms.Label lblColumnsToPrint;
        protected System.Windows.Forms.Button btnOK;
        protected System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.CheckedListBox ctlColumnsToPrintCHKLBX;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        internal System.Windows.Forms.CheckBox checkBox1Page2;

    }
}