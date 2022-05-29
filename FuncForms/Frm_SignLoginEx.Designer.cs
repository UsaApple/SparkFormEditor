namespace SparkFormEditor
{
    partial class Frm_SignLoginEx
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
            this.labTip = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labTip
            // 
            this.labTip.ForeColor = System.Drawing.Color.Red;
            this.labTip.Location = new System.Drawing.Point(13, 112);
            this.labTip.Name = "labTip";
            this.labTip.Size = new System.Drawing.Size(214, 31);
            this.labTip.TabIndex = 5;
            // 
            // Frm_SignLoginEx
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(238, 150);
            this.Controls.Add(this.labTip);
            this.Name = "Frm_SignLoginEx";
            this.Controls.SetChildIndex(this.labTip, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labTip;
    }
}