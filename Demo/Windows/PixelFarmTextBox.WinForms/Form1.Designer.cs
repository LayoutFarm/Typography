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
            this.glyphRenderOptionsUserControl1 = new TypographyTest.WinForms.GlyphRenderOptionsUserControl();
            this.sampleTextBox1 = new SampleWinForms.SampleTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // basicFontOptionsUserControl1
            // 
            this.basicFontOptionsUserControl1.Location = new System.Drawing.Point(561, 12);
            this.basicFontOptionsUserControl1.Name = "basicFontOptionsUserControl1";
            this.basicFontOptionsUserControl1.Size = new System.Drawing.Size(196, 443);
            this.basicFontOptionsUserControl1.TabIndex = 0;
            // 
            // glyphRenderOptionsUserControl1
            // 
            this.glyphRenderOptionsUserControl1.BackColor = System.Drawing.Color.White;
            this.glyphRenderOptionsUserControl1.Location = new System.Drawing.Point(569, 448);
            this.glyphRenderOptionsUserControl1.Name = "glyphRenderOptionsUserControl1";
            this.glyphRenderOptionsUserControl1.Size = new System.Drawing.Size(261, 146);
            this.glyphRenderOptionsUserControl1.TabIndex = 69;
            // 
            // sampleTextBox1
            // 
            this.sampleTextBox1.BackColor = System.Drawing.Color.Gray;
            this.sampleTextBox1.Location = new System.Drawing.Point(12, 12);
            this.sampleTextBox1.Name = "sampleTextBox1";
            this.sampleTextBox1.Size = new System.Drawing.Size(551, 595);
            this.sampleTextBox1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 680);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(154, 56);
            this.button1.TabIndex = 70;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(172, 680);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(154, 56);
            this.button2.TabIndex = 71;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 748);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}

