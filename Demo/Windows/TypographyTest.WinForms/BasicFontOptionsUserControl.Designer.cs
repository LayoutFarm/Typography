namespace TypographyTest.WinForms
{
    partial class BasicFontOptionsUserControl
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
            this.cmbScriptLangs = new System.Windows.Forms.ComboBox();
            this.lstFontList = new System.Windows.Forms.ListBox();
            this.lstFontSizes = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbPositionTech = new System.Windows.Forms.ComboBox();
            this.cmbRenderChoices = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cmbScriptLangs
            // 
            this.cmbScriptLangs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScriptLangs.FormattingEnabled = true;
            this.cmbScriptLangs.Location = new System.Drawing.Point(15, 7);
            this.cmbScriptLangs.Name = "cmbScriptLangs";
            this.cmbScriptLangs.Size = new System.Drawing.Size(170, 21);
            this.cmbScriptLangs.TabIndex = 69;
            // 
            // lstFontList
            // 
            this.lstFontList.FormattingEnabled = true;
            this.lstFontList.Location = new System.Drawing.Point(15, 34);
            this.lstFontList.Name = "lstFontList";
            this.lstFontList.Size = new System.Drawing.Size(170, 121);
            this.lstFontList.TabIndex = 68;
            // 
            // lstFontSizes
            // 
            this.lstFontSizes.FormattingEnabled = true;
            this.lstFontSizes.Location = new System.Drawing.Point(15, 174);
            this.lstFontSizes.Name = "lstFontSizes";
            this.lstFontSizes.Size = new System.Drawing.Size(170, 199);
            this.lstFontSizes.TabIndex = 67;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 158);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 70;
            this.label2.Text = "Size in Points";
            // 
            // cmbPositionTech
            // 
            this.cmbPositionTech.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPositionTech.FormattingEnabled = true;
            this.cmbPositionTech.Location = new System.Drawing.Point(15, 379);
            this.cmbPositionTech.Name = "cmbPositionTech";
            this.cmbPositionTech.Size = new System.Drawing.Size(170, 21);
            this.cmbPositionTech.TabIndex = 71;
            // 
            // cmbRenderChoices
            // 
            this.cmbRenderChoices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRenderChoices.FormattingEnabled = true;
            this.cmbRenderChoices.Location = new System.Drawing.Point(15, 406);
            this.cmbRenderChoices.Name = "cmbRenderChoices";
            this.cmbRenderChoices.Size = new System.Drawing.Size(170, 21);
            this.cmbRenderChoices.TabIndex = 72;
            // 
            // BasicFontOptionsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmbRenderChoices);
            this.Controls.Add(this.cmbPositionTech);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbScriptLangs);
            this.Controls.Add(this.lstFontList);
            this.Controls.Add(this.lstFontSizes);
            this.Name = "BasicFontOptionsUserControl";
            this.Size = new System.Drawing.Size(196, 443);
            this.Load += new System.EventHandler(this.OpenFontOptions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbScriptLangs;
        private System.Windows.Forms.ListBox lstFontList;
        private System.Windows.Forms.ListBox lstFontSizes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbPositionTech;
        private System.Windows.Forms.ComboBox cmbRenderChoices;
    }
}
