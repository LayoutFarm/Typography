namespace TypographyTest.WinForms
{
    partial class GlyphNameListUserControl
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.chkRenderGlyph = new System.Windows.Forms.CheckBox();
            this.txtHexUnicode = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(4, 4);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(147, 264);
            this.listBox1.TabIndex = 0;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(3, 274);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(147, 20);
            this.textBox1.TabIndex = 1;
            // 
            // chkRenderGlyph
            // 
            this.chkRenderGlyph.AutoSize = true;
            this.chkRenderGlyph.Checked = true;
            this.chkRenderGlyph.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRenderGlyph.Location = new System.Drawing.Point(3, 301);
            this.chkRenderGlyph.Name = "chkRenderGlyph";
            this.chkRenderGlyph.Size = new System.Drawing.Size(61, 17);
            this.chkRenderGlyph.TabIndex = 2;
            this.chkRenderGlyph.Text = "Render";
            this.chkRenderGlyph.UseVisualStyleBackColor = true;
            // 
            // txtHexUnicode
            // 
            this.txtHexUnicode.Location = new System.Drawing.Point(0, 324);
            this.txtHexUnicode.Name = "txtHexUnicode";
            this.txtHexUnicode.Size = new System.Drawing.Size(147, 20);
            this.txtHexUnicode.TabIndex = 3;
            // 
            // GlyphNameListUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtHexUnicode);
            this.Controls.Add(this.chkRenderGlyph);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.listBox1);
            this.Name = "GlyphNameListUserControl";
            this.Size = new System.Drawing.Size(156, 379);
            this.Load += new System.EventHandler(this.GlyphNameListUserControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox chkRenderGlyph;
        private System.Windows.Forms.TextBox txtHexUnicode;
    }
}
