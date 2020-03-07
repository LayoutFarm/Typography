namespace SampleWinForms
{
    partial class UIFontScriptOpt
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.chkSelected = new System.Windows.Forms.CheckBox();
            this.chkDoFilter = new System.Windows.Forms.CheckBox();
            this.cmbHintTech = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // chkSelected
            // 
            this.chkSelected.AutoSize = true;
            this.chkSelected.Location = new System.Drawing.Point(6, 9);
            this.chkSelected.Name = "chkSelected";
            this.chkSelected.Size = new System.Drawing.Size(15, 14);
            this.chkSelected.TabIndex = 1;
            this.chkSelected.UseVisualStyleBackColor = true;
            // 
            // chkDoFilter
            // 
            this.chkDoFilter.AutoSize = true;
            this.chkDoFilter.Location = new System.Drawing.Point(6, 29);
            this.chkDoFilter.Name = "chkDoFilter";
            this.chkDoFilter.Size = new System.Drawing.Size(65, 17);
            this.chkDoFilter.TabIndex = 2;
            this.chkDoFilter.Text = "Do Filter";
            this.chkDoFilter.UseVisualStyleBackColor = true;
            // 
            // cmbHintTech
            // 
            this.cmbHintTech.FormattingEnabled = true;
            this.cmbHintTech.Location = new System.Drawing.Point(77, 29);
            this.cmbHintTech.Name = "cmbHintTech";
            this.cmbHintTech.Size = new System.Drawing.Size(171, 21);
            this.cmbHintTech.TabIndex = 3;
            // 
            // CustomUIFontScriptOpt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmbHintTech);
            this.Controls.Add(this.chkDoFilter);
            this.Controls.Add(this.chkSelected);
            this.Controls.Add(this.label1);
            this.Name = "CustomUIFontScriptOpt";
            this.Size = new System.Drawing.Size(275, 64);
            this.Load += new System.EventHandler(this.CustomUIFontScriptOpt_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkSelected;
        private System.Windows.Forms.CheckBox chkDoFilter;
        private System.Windows.Forms.ComboBox cmbHintTech;
    }
}
