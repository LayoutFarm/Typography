namespace SampleWinForms
{
    partial class FormTestTrimableFeature
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
            this.cmdTestReloadGlyphs = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmdTestReloadGlyphs
            // 
            this.cmdTestReloadGlyphs.Location = new System.Drawing.Point(13, 92);
            this.cmdTestReloadGlyphs.Name = "cmdTestReloadGlyphs";
            this.cmdTestReloadGlyphs.Size = new System.Drawing.Size(133, 36);
            this.cmdTestReloadGlyphs.TabIndex = 80;
            this.cmdTestReloadGlyphs.Text = "Test Trimable Features";
            this.cmdTestReloadGlyphs.UseVisualStyleBackColor = true;
            this.cmdTestReloadGlyphs.Click += new System.EventHandler(this.cmdTestReloadGlyphs_Click);
            // 
            // FormTestTrimableFeature
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 581);
            this.Controls.Add(this.cmdTestReloadGlyphs);
            this.Name = "FormTestTrimableFeature";
            this.Text = "FormTestTrimableFeature";
            this.Load += new System.EventHandler(this.FormTestTrimableFeature_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdTestReloadGlyphs;
    }
}