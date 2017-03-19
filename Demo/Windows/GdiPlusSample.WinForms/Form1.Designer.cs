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
            this.lstFontList = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lstFontSizes = new System.Windows.Forms.ListBox();
            this.txtInputChar = new System.Windows.Forms.TextBox();
            this.chkFillBackground = new System.Windows.Forms.CheckBox();
            this.chkBorder = new System.Windows.Forms.CheckBox();
            this.cmbPositionTech = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.lstHintList = new System.Windows.Forms.ListBox();
            this.cmdMeasureTextSpan = new System.Windows.Forms.Button();
            this.chkShowSampleTextBox = new System.Windows.Forms.CheckBox();
            this.sampleTextBox1 = new SampleWinForms.SampleTextBox();
            this.SuspendLayout();
            // 
            // lstFontList
            // 
            this.lstFontList.FormattingEnabled = true;
            this.lstFontList.Location = new System.Drawing.Point(693, 39);
            this.lstFontList.Name = "lstFontList";
            this.lstFontList.Size = new System.Drawing.Size(121, 121);
            this.lstFontList.TabIndex = 29;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(690, 184);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "Size in Points";
            // 
            // lstFontSizes
            // 
            this.lstFontSizes.FormattingEnabled = true;
            this.lstFontSizes.Location = new System.Drawing.Point(693, 200);
            this.lstFontSizes.Name = "lstFontSizes";
            this.lstFontSizes.Size = new System.Drawing.Size(121, 212);
            this.lstFontSizes.TabIndex = 27;
            // 
            // txtInputChar
            // 
            this.txtInputChar.Location = new System.Drawing.Point(12, 29);
            this.txtInputChar.Name = "txtInputChar";
            this.txtInputChar.Size = new System.Drawing.Size(101, 20);
            this.txtInputChar.TabIndex = 26;
            this.txtInputChar.Text = "I";
            // 
            // chkFillBackground
            // 
            this.chkFillBackground.AutoSize = true;
            this.chkFillBackground.Location = new System.Drawing.Point(822, 65);
            this.chkFillBackground.Name = "chkFillBackground";
            this.chkFillBackground.Size = new System.Drawing.Size(101, 17);
            this.chkFillBackground.TabIndex = 31;
            this.chkFillBackground.Text = "Fill BackGround";
            this.chkFillBackground.UseVisualStyleBackColor = true;
            // 
            // chkBorder
            // 
            this.chkBorder.AutoSize = true;
            this.chkBorder.Location = new System.Drawing.Point(822, 42);
            this.chkBorder.Name = "chkBorder";
            this.chkBorder.Size = new System.Drawing.Size(57, 17);
            this.chkBorder.TabIndex = 30;
            this.chkBorder.Text = "Border";
            this.chkBorder.UseVisualStyleBackColor = true;
            // 
            // cmbPositionTech
            // 
            this.cmbPositionTech.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPositionTech.FormattingEnabled = true;
            this.cmbPositionTech.Location = new System.Drawing.Point(451, 38);
            this.cmbPositionTech.Name = "cmbPositionTech";
            this.cmbPositionTech.Size = new System.Drawing.Size(224, 21);
            this.cmbPositionTech.TabIndex = 32;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(119, 29);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(81, 37);
            this.button1.TabIndex = 34;
            this.button1.Text = "Render!";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // lstHintList
            // 
            this.lstHintList.FormattingEnabled = true;
            this.lstHintList.Location = new System.Drawing.Point(451, 65);
            this.lstHintList.Name = "lstHintList";
            this.lstHintList.Size = new System.Drawing.Size(224, 69);
            this.lstHintList.TabIndex = 36;
            // 
            // cmdMeasureTextSpan
            // 
            this.cmdMeasureTextSpan.Location = new System.Drawing.Point(206, 31);
            this.cmdMeasureTextSpan.Name = "cmdMeasureTextSpan";
            this.cmdMeasureTextSpan.Size = new System.Drawing.Size(153, 37);
            this.cmdMeasureTextSpan.TabIndex = 37;
            this.cmdMeasureTextSpan.Text = "MeasureTextSpan";
            this.cmdMeasureTextSpan.UseVisualStyleBackColor = true;
            this.cmdMeasureTextSpan.Click += new System.EventHandler(this.cmdMeasureTextSpan_Click);
            // 
            // chkShowSampleTextBox
            // 
            this.chkShowSampleTextBox.AutoSize = true;
            this.chkShowSampleTextBox.Location = new System.Drawing.Point(12, 90);
            this.chkShowSampleTextBox.Name = "chkShowSampleTextBox";
            this.chkShowSampleTextBox.Size = new System.Drawing.Size(133, 17);
            this.chkShowSampleTextBox.TabIndex = 38;
            this.chkShowSampleTextBox.Text = "Show Sample TextBox";
            this.chkShowSampleTextBox.UseVisualStyleBackColor = true;
            this.chkShowSampleTextBox.CheckedChanged += new System.EventHandler(this.chkShowSampleTextBox_CheckedChanged);
            // 
            // sampleTextBox1
            // 
            this.sampleTextBox1.BackColor = System.Drawing.Color.Gray;
            this.sampleTextBox1.Location = new System.Drawing.Point(12, 113);
            this.sampleTextBox1.Name = "sampleTextBox1";
            this.sampleTextBox1.Size = new System.Drawing.Size(433, 388);
            this.sampleTextBox1.TabIndex = 39;
            this.sampleTextBox1.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(935, 753);
            this.Controls.Add(this.sampleTextBox1);
            this.Controls.Add(this.chkShowSampleTextBox);
            this.Controls.Add(this.cmdMeasureTextSpan);
            this.Controls.Add(this.lstHintList);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cmbPositionTech);
            this.Controls.Add(this.chkFillBackground);
            this.Controls.Add(this.chkBorder);
            this.Controls.Add(this.lstFontList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstFontSizes);
            this.Controls.Add(this.txtInputChar);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstFontList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lstFontSizes;
        private System.Windows.Forms.TextBox txtInputChar;
        private System.Windows.Forms.CheckBox chkFillBackground;
        private System.Windows.Forms.CheckBox chkBorder;
        private System.Windows.Forms.ComboBox cmbPositionTech;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox lstHintList;
        private System.Windows.Forms.Button cmdMeasureTextSpan;
        private System.Windows.Forms.CheckBox chkShowSampleTextBox;
        private SampleTextBox sampleTextBox1;
    }
}