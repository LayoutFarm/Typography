namespace SampleWinForms
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
            this.button1 = new System.Windows.Forms.Button();
            this.txtInputChar = new System.Windows.Forms.TextBox();
            this.cmdBuildMsdfTexture = new System.Windows.Forms.Button();
            this.glyphContourAnalysisOptionsUserControl1 = new TypographyTest.WinForms.GlyphContourAnalysisOptionsUserControl();
            this.glyphRenderOptionsUserControl1 = new TypographyTest.WinForms.GlyphRenderOptionsUserControl();
            this.openFontOptions1 = new TypographyTest.WinForms.BasicFontOptionsUserControl();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(436, 11);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(81, 37);
            this.button1.TabIndex = 0;
            this.button1.Text = "Render!";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // txtInputChar
            // 
            this.txtInputChar.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(222)));
            this.txtInputChar.Location = new System.Drawing.Point(0, -2);
            this.txtInputChar.Name = "txtInputChar";
            this.txtInputChar.Size = new System.Drawing.Size(168, 21);
            this.txtInputChar.TabIndex = 1;
            this.txtInputChar.Text = "I";
            // 
            // cmdBuildMsdfTexture
            // 
            this.cmdBuildMsdfTexture.Location = new System.Drawing.Point(1260, 27);
            this.cmdBuildMsdfTexture.Name = "cmdBuildMsdfTexture";
            this.cmdBuildMsdfTexture.Size = new System.Drawing.Size(121, 28);
            this.cmdBuildMsdfTexture.TabIndex = 22;
            this.cmdBuildMsdfTexture.Text = "Make MsdfTexture";
            this.cmdBuildMsdfTexture.UseVisualStyleBackColor = true;
            this.cmdBuildMsdfTexture.Click += new System.EventHandler(this.cmdBuildMsdfTexture_Click);
            // 
            // glyphContourAnalysisOptionsUserControl1
            // 
            this.glyphContourAnalysisOptionsUserControl1.BackColor = System.Drawing.Color.White;
            this.glyphContourAnalysisOptionsUserControl1.Location = new System.Drawing.Point(772, 164);
            this.glyphContourAnalysisOptionsUserControl1.Name = "glyphContourAnalysisOptionsUserControl1";
            this.glyphContourAnalysisOptionsUserControl1.Size = new System.Drawing.Size(488, 607);
            this.glyphContourAnalysisOptionsUserControl1.TabIndex = 69;
            // 
            // glyphRenderOptionsUserControl1
            // 
            this.glyphRenderOptionsUserControl1.BackColor = System.Drawing.Color.White;
            this.glyphRenderOptionsUserControl1.Location = new System.Drawing.Point(772, 12);
            this.glyphRenderOptionsUserControl1.Name = "glyphRenderOptionsUserControl1";
            this.glyphRenderOptionsUserControl1.Size = new System.Drawing.Size(261, 146);
            this.glyphRenderOptionsUserControl1.TabIndex = 68;
            // 
            // openFontOptions1
            // 
            this.openFontOptions1.BackColor = System.Drawing.Color.White;
            this.openFontOptions1.Location = new System.Drawing.Point(567, 12);
            this.openFontOptions1.Name = "openFontOptions1";
            this.openFontOptions1.Size = new System.Drawing.Size(199, 428);
            this.openFontOptions1.TabIndex = 67;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1393, 857);
            this.Controls.Add(this.glyphContourAnalysisOptionsUserControl1);
            this.Controls.Add(this.glyphRenderOptionsUserControl1);
            this.Controls.Add(this.openFontOptions1);
            this.Controls.Add(this.cmdBuildMsdfTexture);
            this.Controls.Add(this.txtInputChar);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtInputChar;
        private System.Windows.Forms.Button cmdBuildMsdfTexture;
        private TypographyTest.WinForms.BasicFontOptionsUserControl openFontOptions1;
        private TypographyTest.WinForms.GlyphRenderOptionsUserControl glyphRenderOptionsUserControl1;
        private TypographyTest.WinForms.GlyphContourAnalysisOptionsUserControl glyphContourAnalysisOptionsUserControl1;
    }
}

