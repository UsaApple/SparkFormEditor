namespace NurseForm.Editor.Ctrl
{
    partial class SelectConfirmForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label取消 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(30, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(323, 34);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.MouseLeave += new System.EventHandler(this.label_MouseLeave);
            this.label1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label_MouseMove);
            this.label1.Click += new System.EventHandler(this.label_Click);
            this.label1.MouseEnter += new System.EventHandler(this.label_MouseEnter);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(30, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(323, 34);
            this.label2.TabIndex = 1;
            this.label2.Text = "label2";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.MouseLeave += new System.EventHandler(this.label_MouseLeave);
            this.label2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label_MouseMove);
            this.label2.Click += new System.EventHandler(this.label_Click);
            this.label2.MouseEnter += new System.EventHandler(this.label_MouseEnter);
            // 
            // label取消
            // 
            this.label取消.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label取消.Location = new System.Drawing.Point(30, 131);
            this.label取消.Name = "label取消";
            this.label取消.Size = new System.Drawing.Size(323, 34);
            this.label取消.TabIndex = 2;
            this.label取消.Text = "取消";
            this.label取消.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label取消.MouseLeave += new System.EventHandler(this.label_MouseLeave);
            this.label取消.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label_MouseMove);
            this.label取消.Click += new System.EventHandler(this.label取消_Click);
            this.label取消.MouseEnter += new System.EventHandler(this.label_MouseEnter);
            // 
            // SelectConfirmForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(393, 182);
            this.Controls.Add(this.label取消);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectConfirmForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "选择需要同步更新的文书";
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SelectConfirmForm_MouseMove);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label取消;

    }
}