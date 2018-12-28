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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lstFontStyle = new System.Windows.Forms.ListBox();
            this.lstFontNameList = new System.Windows.Forms.ListBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmbScriptLangs
            // 
            this.cmbScriptLangs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScriptLangs.FormattingEnabled = true;
            this.cmbScriptLangs.Location = new System.Drawing.Point(14, 3);
            this.cmbScriptLangs.Name = "cmbScriptLangs";
            this.cmbScriptLangs.Size = new System.Drawing.Size(141, 21);
            this.cmbScriptLangs.TabIndex = 69;
            // 
            // lstFontList
            // 
            this.lstFontList.FormattingEnabled = true;
            this.lstFontList.Location = new System.Drawing.Point(6, 6);
            this.lstFontList.Name = "lstFontList";
            this.lstFontList.Size = new System.Drawing.Size(172, 251);
            this.lstFontList.TabIndex = 68;
            // 
            // lstFontSizes
            // 
            this.lstFontSizes.FormattingEnabled = true;
            this.lstFontSizes.Location = new System.Drawing.Point(13, 43);
            this.lstFontSizes.Name = "lstFontSizes";
            this.lstFontSizes.Size = new System.Drawing.Size(142, 199);
            this.lstFontSizes.TabIndex = 67;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 70;
            this.label2.Text = "Size in Points";
            // 
            // cmbPositionTech
            // 
            this.cmbPositionTech.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPositionTech.FormattingEnabled = true;
            this.cmbPositionTech.Location = new System.Drawing.Point(13, 248);
            this.cmbPositionTech.Name = "cmbPositionTech";
            this.cmbPositionTech.Size = new System.Drawing.Size(142, 21);
            this.cmbPositionTech.TabIndex = 71;
            // 
            // cmbRenderChoices
            // 
            this.cmbRenderChoices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRenderChoices.FormattingEnabled = true;
            this.cmbRenderChoices.Location = new System.Drawing.Point(13, 275);
            this.cmbRenderChoices.Name = "cmbRenderChoices";
            this.cmbRenderChoices.Size = new System.Drawing.Size(142, 21);
            this.cmbRenderChoices.TabIndex = 72;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(161, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(380, 293);
            this.tabControl1.TabIndex = 73;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lstFontList);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(372, 267);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Actual Font Files";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lstFontStyle);
            this.tabPage2.Controls.Add(this.lstFontNameList);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(372, 267);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Fonts under Management";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lstFontStyle
            // 
            this.lstFontStyle.FormattingEnabled = true;
            this.lstFontStyle.Location = new System.Drawing.Point(178, 6);
            this.lstFontStyle.Name = "lstFontStyle";
            this.lstFontStyle.Size = new System.Drawing.Size(188, 251);
            this.lstFontStyle.TabIndex = 70;
            // 
            // lstFontNameList
            // 
            this.lstFontNameList.FormattingEnabled = true;
            this.lstFontNameList.Location = new System.Drawing.Point(3, 6);
            this.lstFontNameList.Name = "lstFontNameList";
            this.lstFontNameList.Size = new System.Drawing.Size(169, 251);
            this.lstFontNameList.TabIndex = 69;
            // 
            // BasicFontOptionsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.cmbRenderChoices);
            this.Controls.Add(this.cmbPositionTech);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbScriptLangs);
            this.Controls.Add(this.lstFontSizes);
            this.Name = "BasicFontOptionsUserControl";
            this.Size = new System.Drawing.Size(555, 299);
            this.Load += new System.EventHandler(this.OpenFontOptions_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
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
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox lstFontStyle;
        private System.Windows.Forms.ListBox lstFontNameList;
    }
}
