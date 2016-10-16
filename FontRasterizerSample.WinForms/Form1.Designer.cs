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
            this.label1 = new System.Windows.Forms.Label();
            this.chkBorder = new System.Windows.Forms.CheckBox();
            this.chkFillBackground = new System.Windows.Forms.CheckBox();
            this.cmbRenderChoices = new System.Windows.Forms.ComboBox();
            this.lstFontSizes = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkKern = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(129, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(81, 37);
            this.button1.TabIndex = 0;
            this.button1.Text = "Render!";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtInputChar
            // 
            this.txtInputChar.Location = new System.Drawing.Point(12, 29);
            this.txtInputChar.Name = "txtInputChar";
            this.txtInputChar.Size = new System.Drawing.Size(101, 20);
            this.txtInputChar.TabIndex = 1;
            this.txtInputChar.Text = "B";
            this.txtInputChar.TextChanged += new System.EventHandler(this.txtInputChar_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Only 1 char";
            // 
            // chkBorder
            // 
            this.chkBorder.AutoSize = true;
            this.chkBorder.Location = new System.Drawing.Point(237, 13);
            this.chkBorder.Name = "chkBorder";
            this.chkBorder.Size = new System.Drawing.Size(57, 17);
            this.chkBorder.TabIndex = 3;
            this.chkBorder.Text = "Border";
            this.chkBorder.UseVisualStyleBackColor = true;
            // 
            // chkFillBackground
            // 
            this.chkFillBackground.AutoSize = true;
            this.chkFillBackground.Checked = true;
            this.chkFillBackground.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFillBackground.Location = new System.Drawing.Point(237, 36);
            this.chkFillBackground.Name = "chkFillBackground";
            this.chkFillBackground.Size = new System.Drawing.Size(101, 17);
            this.chkFillBackground.TabIndex = 4;
            this.chkFillBackground.Text = "Fill BackGround";
            this.chkFillBackground.UseVisualStyleBackColor = true;
            // 
            // cmbRenderChoices
            // 
            this.cmbRenderChoices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRenderChoices.FormattingEnabled = true;
            this.cmbRenderChoices.Location = new System.Drawing.Point(380, 12);
            this.cmbRenderChoices.Name = "cmbRenderChoices";
            this.cmbRenderChoices.Size = new System.Drawing.Size(224, 21);
            this.cmbRenderChoices.TabIndex = 7;
            // 
            // lstFontSizes
            // 
            this.lstFontSizes.FormattingEnabled = true;
            this.lstFontSizes.Location = new System.Drawing.Point(653, 26);
            this.lstFontSizes.Name = "lstFontSizes";
            this.lstFontSizes.Size = new System.Drawing.Size(100, 212);
            this.lstFontSizes.TabIndex = 8;
            this.lstFontSizes.SelectedIndexChanged += new System.EventHandler(this.lstFontSizes_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(650, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Size in Points";
            // 
            // chkKern
            // 
            this.chkKern.AutoSize = true;
            this.chkKern.Checked = true;
            this.chkKern.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkKern.Location = new System.Drawing.Point(237, 59);
            this.chkKern.Name = "chkKern";
            this.chkKern.Size = new System.Drawing.Size(85, 17);
            this.chkKern.TabIndex = 10;
            this.chkKern.Text = "Do Kerning1";
            this.chkKern.UseVisualStyleBackColor = true;
            this.chkKern.CheckedChanged += new System.EventHandler(this.chkKern_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 427);
            this.Controls.Add(this.chkKern);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstFontSizes);
            this.Controls.Add(this.cmbRenderChoices);
            this.Controls.Add(this.chkFillBackground);
            this.Controls.Add(this.chkBorder);
            this.Controls.Add(this.label1);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkBorder;
        private System.Windows.Forms.CheckBox chkFillBackground;
        private System.Windows.Forms.ComboBox cmbRenderChoices;
        private System.Windows.Forms.ListBox lstFontSizes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkKern;
    }
}

