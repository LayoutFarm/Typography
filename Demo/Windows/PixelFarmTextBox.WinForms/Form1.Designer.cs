namespace PixelFarmTextBox.WinForms
{
    partial class Form1
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
            this.basicFontOptionsUserControl1 = new TypographyTest.WinForms.BasicFontOptionsUserControl();
            this.sampleTextBox1 = new SampleWinForms.SampleTextBox();
            this.glyphRenderOptionsUserControl1 = new TypographyTest.WinForms.GlyphRenderOptionsUserControl();
            this.SuspendLayout();
            // 
            // basicFontOptionsUserControl1
            // 
            this.basicFontOptionsUserControl1.Location = new System.Drawing.Point(561, 12);
            this.basicFontOptionsUserControl1.Name = "basicFontOptionsUserControl1";
            this.basicFontOptionsUserControl1.Size = new System.Drawing.Size(196, 443);
            this.basicFontOptionsUserControl1.TabIndex = 0;
            // 
            // sampleTextBox1
            // 
            this.sampleTextBox1.BackColor = System.Drawing.Color.Gray;
            this.sampleTextBox1.Location = new System.Drawing.Point(12, 12);
            this.sampleTextBox1.Name = "sampleTextBox1";
            this.sampleTextBox1.Size = new System.Drawing.Size(551, 595);
            this.sampleTextBox1.TabIndex = 1;
            // 
            // glyphRenderOptionsUserControl1
            // 
            this.glyphRenderOptionsUserControl1.BackColor = System.Drawing.Color.White;
            this.glyphRenderOptionsUserControl1.Location = new System.Drawing.Point(569, 448);
            this.glyphRenderOptionsUserControl1.Name = "glyphRenderOptionsUserControl1";
            this.glyphRenderOptionsUserControl1.Size = new System.Drawing.Size(261, 146);
            this.glyphRenderOptionsUserControl1.TabIndex = 69;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 619);
            this.Controls.Add(this.glyphRenderOptionsUserControl1);
            this.Controls.Add(this.sampleTextBox1);
            this.Controls.Add(this.basicFontOptionsUserControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private TypographyTest.WinForms.BasicFontOptionsUserControl basicFontOptionsUserControl1;
        private SampleWinForms.SampleTextBox sampleTextBox1;
        private TypographyTest.WinForms.GlyphRenderOptionsUserControl glyphRenderOptionsUserControl1;
    }
}

