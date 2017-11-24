namespace TypographyTest.WinForms
{
    partial class GlyphRenderOptionsUserControl
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
            this.lstHintList = new System.Windows.Forms.ListBox();
            this.chkGsubEnableLigature = new System.Windows.Forms.CheckBox();
            this.chkFillBackground = new System.Windows.Forms.CheckBox();
            this.chkBorder = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lstHintList
            // 
            this.lstHintList.FormattingEnabled = true;
            this.lstHintList.Location = new System.Drawing.Point(3, 3);
            this.lstHintList.Name = "lstHintList";
            this.lstHintList.Size = new System.Drawing.Size(224, 69);
            this.lstHintList.TabIndex = 30;
            // 
            // chkGsubEnableLigature
            // 
            this.chkGsubEnableLigature.AutoSize = true;
            this.chkGsubEnableLigature.Checked = true;
            this.chkGsubEnableLigature.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGsubEnableLigature.Location = new System.Drawing.Point(3, 78);
            this.chkGsubEnableLigature.Name = "chkGsubEnableLigature";
            this.chkGsubEnableLigature.Size = new System.Drawing.Size(136, 17);
            this.chkGsubEnableLigature.TabIndex = 31;
            this.chkGsubEnableLigature.Text = "GSUB: Enable Ligature";
            this.chkGsubEnableLigature.UseVisualStyleBackColor = true;
            // 
            // chkFillBackground
            // 
            this.chkFillBackground.AutoSize = true;
            this.chkFillBackground.Location = new System.Drawing.Point(3, 125);
            this.chkFillBackground.Name = "chkFillBackground";
            this.chkFillBackground.Size = new System.Drawing.Size(101, 17);
            this.chkFillBackground.TabIndex = 69;
            this.chkFillBackground.Text = "Fill BackGround";
            this.chkFillBackground.UseVisualStyleBackColor = true;
            // 
            // chkBorder
            // 
            this.chkBorder.AutoSize = true;
            this.chkBorder.Location = new System.Drawing.Point(3, 102);
            this.chkBorder.Name = "chkBorder";
            this.chkBorder.Size = new System.Drawing.Size(57, 17);
            this.chkBorder.TabIndex = 68;
            this.chkBorder.Text = "Border";
            this.chkBorder.UseVisualStyleBackColor = true;
            // 
            // GlyphRenderOptionsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkFillBackground);
            this.Controls.Add(this.chkBorder);
            this.Controls.Add(this.chkGsubEnableLigature);
            this.Controls.Add(this.lstHintList);
            this.Name = "GlyphRenderOptionsUserControl";
            this.Size = new System.Drawing.Size(236, 166);
            this.Load += new System.EventHandler(this.GlyphRenderOptionsUserControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstHintList;
        private System.Windows.Forms.CheckBox chkGsubEnableLigature;
        private System.Windows.Forms.CheckBox chkFillBackground;
        private System.Windows.Forms.CheckBox chkBorder;
    }
}
