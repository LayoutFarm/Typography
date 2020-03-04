namespace SampleWinForms
{
    partial class FormMsdfTest2
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
            this.cmdGenMsdfGlyphAtlas = new System.Windows.Forms.Button();
            this.cmdTestMsdfGen2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lstFontFiles = new System.Windows.Forms.ListBox();
            this.txtFontSize = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtGlyphIndexStart = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtGlyphIndexStop = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // cmdGenMsdfGlyphAtlas
            // 
            this.cmdGenMsdfGlyphAtlas.Location = new System.Drawing.Point(236, 123);
            this.cmdGenMsdfGlyphAtlas.Name = "cmdGenMsdfGlyphAtlas";
            this.cmdGenMsdfGlyphAtlas.Size = new System.Drawing.Size(171, 44);
            this.cmdGenMsdfGlyphAtlas.TabIndex = 0;
            this.cmdGenMsdfGlyphAtlas.Text = "Gen Msdf Glyph Atlas";
            this.cmdGenMsdfGlyphAtlas.UseVisualStyleBackColor = true;
            this.cmdGenMsdfGlyphAtlas.Click += new System.EventHandler(this.cmdGenMsdfGlyphAtlas_Click);
            // 
            // cmdTestMsdfGen2
            // 
            this.cmdTestMsdfGen2.Location = new System.Drawing.Point(557, 42);
            this.cmdTestMsdfGen2.Name = "cmdTestMsdfGen2";
            this.cmdTestMsdfGen2.Size = new System.Drawing.Size(115, 44);
            this.cmdTestMsdfGen2.TabIndex = 1;
            this.cmdTestMsdfGen2.Text = "TestMsdfGen2";
            this.cmdTestMsdfGen2.UseVisualStyleBackColor = true;
            this.cmdTestMsdfGen2.Click += new System.EventHandler(this.button1_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(678, 42);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(115, 44);
            this.button1.TabIndex = 2;
            this.button1.Text = "TestMsdfGen3";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // lstFontFiles
            // 
            this.lstFontFiles.FormattingEnabled = true;
            this.lstFontFiles.Location = new System.Drawing.Point(16, 123);
            this.lstFontFiles.Name = "lstFontFiles";
            this.lstFontFiles.Size = new System.Drawing.Size(214, 342);
            this.lstFontFiles.TabIndex = 3;
            // 
            // txtFontSize
            // 
            this.txtFontSize.Location = new System.Drawing.Point(130, 20);
            this.txtFontSize.Name = "txtFontSize";
            this.txtFontSize.Size = new System.Drawing.Size(100, 20);
            this.txtFontSize.TabIndex = 4;
            this.txtFontSize.Text = "18";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Font Size (points)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "GlyphIndex Start";
            // 
            // txtGlyphIndexStart
            // 
            this.txtGlyphIndexStart.Location = new System.Drawing.Point(130, 55);
            this.txtGlyphIndexStart.Name = "txtGlyphIndexStart";
            this.txtGlyphIndexStart.Size = new System.Drawing.Size(100, 20);
            this.txtGlyphIndexStart.TabIndex = 7;
            this.txtGlyphIndexStart.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "GlyphIndex Stop";
            // 
            // txtGlyphIndexStop
            // 
            this.txtGlyphIndexStop.Location = new System.Drawing.Point(130, 82);
            this.txtGlyphIndexStop.Name = "txtGlyphIndexStop";
            this.txtGlyphIndexStop.Size = new System.Drawing.Size(100, 20);
            this.txtGlyphIndexStop.TabIndex = 9;
            this.txtGlyphIndexStop.Text = "0";
            // 
            // FormMsdfTest2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1182, 650);
            this.Controls.Add(this.txtGlyphIndexStop);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtGlyphIndexStart);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFontSize);
            this.Controls.Add(this.lstFontFiles);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cmdTestMsdfGen2);
            this.Controls.Add(this.cmdGenMsdfGlyphAtlas);
            this.Name = "FormMsdfTest2";
            this.Text = "FormMsdfTest2";
            this.Load += new System.EventHandler(this.FormMsdfTest2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdGenMsdfGlyphAtlas;
        private System.Windows.Forms.Button cmdTestMsdfGen2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox lstFontFiles;
        private System.Windows.Forms.TextBox txtFontSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtGlyphIndexStart;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtGlyphIndexStop;
    }
}