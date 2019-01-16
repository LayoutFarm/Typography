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
            this.glyphNameListUserControl1 = new TypographyTest.WinForms.GlyphNameListUserControl();
            this.glyphContourAnalysisOptionsUserControl1 = new TypographyTest.WinForms.GlyphContourAnalysisOptionsUserControl();
            this.glyphRenderOptionsUserControl1 = new TypographyTest.WinForms.GlyphRenderOptionsUserControl();
            this.openFontOptions1 = new TypographyTest.WinForms.BasicFontOptionsUserControl();
            this.cmdMeasureString = new System.Windows.Forms.Button();
            this.lblStringSize = new System.Windows.Forms.Label();
            this.cmdTestFontAtlas = new System.Windows.Forms.Button();
            this.txtSampleChars = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(349, 11);
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
            this.txtInputChar.Text = "a";
            // 
            // cmdBuildMsdfTexture
            // 
            this.cmdBuildMsdfTexture.Location = new System.Drawing.Point(1266, 445);
            this.cmdBuildMsdfTexture.Name = "cmdBuildMsdfTexture";
            this.cmdBuildMsdfTexture.Size = new System.Drawing.Size(121, 28);
            this.cmdBuildMsdfTexture.TabIndex = 22;
            this.cmdBuildMsdfTexture.Text = "Make MsdfTexture";
            this.cmdBuildMsdfTexture.UseVisualStyleBackColor = true;
            this.cmdBuildMsdfTexture.Click += new System.EventHandler(this.cmdBuildMsdfTexture_Click);
            // 
            // glyphNameListUserControl1
            // 
            this.glyphNameListUserControl1.Location = new System.Drawing.Point(607, 446);
            this.glyphNameListUserControl1.Name = "glyphNameListUserControl1";
            this.glyphNameListUserControl1.Size = new System.Drawing.Size(159, 385);
            this.glyphNameListUserControl1.TabIndex = 70;
            this.glyphNameListUserControl1.Typeface = null;
            // 
            // glyphContourAnalysisOptionsUserControl1
            // 
            this.glyphContourAnalysisOptionsUserControl1.BackColor = System.Drawing.Color.White;
            this.glyphContourAnalysisOptionsUserControl1.Location = new System.Drawing.Point(772, 446);
            this.glyphContourAnalysisOptionsUserControl1.Name = "glyphContourAnalysisOptionsUserControl1";
            this.glyphContourAnalysisOptionsUserControl1.Size = new System.Drawing.Size(488, 302);
            this.glyphContourAnalysisOptionsUserControl1.TabIndex = 69;
            // 
            // glyphRenderOptionsUserControl1
            // 
            this.glyphRenderOptionsUserControl1.BackColor = System.Drawing.Color.White;
            this.glyphRenderOptionsUserControl1.Location = new System.Drawing.Point(1126, 12);
            this.glyphRenderOptionsUserControl1.Name = "glyphRenderOptionsUserControl1";
            this.glyphRenderOptionsUserControl1.Size = new System.Drawing.Size(261, 146);
            this.glyphRenderOptionsUserControl1.TabIndex = 68;
            // 
            // openFontOptions1
            // 
            this.openFontOptions1.BackColor = System.Drawing.Color.White;
            this.openFontOptions1.Location = new System.Drawing.Point(567, 12);
            this.openFontOptions1.Name = "openFontOptions1";
            this.openFontOptions1.Size = new System.Drawing.Size(553, 428);
            this.openFontOptions1.TabIndex = 67;
            // 
            // cmdMeasureString
            // 
            this.cmdMeasureString.Location = new System.Drawing.Point(436, 11);
            this.cmdMeasureString.Name = "cmdMeasureString";
            this.cmdMeasureString.Size = new System.Drawing.Size(125, 37);
            this.cmdMeasureString.TabIndex = 71;
            this.cmdMeasureString.Text = "Measure String";
            this.cmdMeasureString.UseVisualStyleBackColor = true;
            this.cmdMeasureString.Click += new System.EventHandler(this.cmdMeasureString_Click);
            // 
            // lblStringSize
            // 
            this.lblStringSize.AutoSize = true;
            this.lblStringSize.Location = new System.Drawing.Point(433, 51);
            this.lblStringSize.Name = "lblStringSize";
            this.lblStringSize.Size = new System.Drawing.Size(77, 13);
            this.lblStringSize.TabIndex = 72;
            this.lblStringSize.Text = "measure_size=";
            // 
            // cmdTestFontAtlas
            // 
            this.cmdTestFontAtlas.Location = new System.Drawing.Point(1266, 537);
            this.cmdTestFontAtlas.Name = "cmdTestFontAtlas";
            this.cmdTestFontAtlas.Size = new System.Drawing.Size(121, 28);
            this.cmdTestFontAtlas.TabIndex = 73;
            this.cmdTestFontAtlas.Text = "TestFontAtlas";
            this.cmdTestFontAtlas.UseVisualStyleBackColor = true;
            this.cmdTestFontAtlas.Click += new System.EventHandler(this.cmdTestFontAtlas_Click);
            // 
            // txtSampleChars
            // 
            this.txtSampleChars.Location = new System.Drawing.Point(1266, 511);
            this.txtSampleChars.Name = "txtSampleChars";
            this.txtSampleChars.Size = new System.Drawing.Size(119, 20);
            this.txtSampleChars.TabIndex = 74;
            this.txtSampleChars.Text = "sample!";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1126, 174);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(133, 37);
            this.button2.TabIndex = 75;
            this.button2.Text = "TestReadWoff";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(1126, 217);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(133, 37);
            this.button3.TabIndex = 76;
            this.button3.Text = "TestReadWoff2";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1393, 857);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.txtSampleChars);
            this.Controls.Add(this.cmdTestFontAtlas);
            this.Controls.Add(this.lblStringSize);
            this.Controls.Add(this.cmdMeasureString);
            this.Controls.Add(this.glyphNameListUserControl1);
            this.Controls.Add(this.glyphContourAnalysisOptionsUserControl1);
            this.Controls.Add(this.glyphRenderOptionsUserControl1);
            this.Controls.Add(this.openFontOptions1);
            this.Controls.Add(this.cmdBuildMsdfTexture);
            this.Controls.Add(this.txtInputChar);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
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
        private TypographyTest.WinForms.GlyphNameListUserControl glyphNameListUserControl1;
        private System.Windows.Forms.Button cmdMeasureString;
        private System.Windows.Forms.Label lblStringSize;
        private System.Windows.Forms.Button cmdTestFontAtlas;
        private System.Windows.Forms.TextBox txtSampleChars;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}

