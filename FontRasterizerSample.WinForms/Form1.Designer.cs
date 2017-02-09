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
            this.chkTrueTypeHint = new System.Windows.Forms.CheckBox();
            this.chkShowControlPoints = new System.Windows.Forms.CheckBox();
            this.chkShowTess = new System.Windows.Forms.CheckBox();
            this.chkShowGrid = new System.Windows.Forms.CheckBox();
            this.txtGridSize = new System.Windows.Forms.TextBox();
            this.chkYGridFitting = new System.Windows.Forms.CheckBox();
            this.chkVerticalHinting = new System.Windows.Forms.CheckBox();
            this.chkDrawBone = new System.Windows.Forms.CheckBox();
            this.chkXGridFitting = new System.Windows.Forms.CheckBox();
            this.chkLcdTechnique = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(186, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(81, 37);
            this.button1.TabIndex = 0;
            this.button1.Text = "Render!";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtInputChar
            // 
            this.txtInputChar.Location = new System.Drawing.Point(79, 7);
            this.txtInputChar.Name = "txtInputChar";
            this.txtInputChar.Size = new System.Drawing.Size(101, 20);
            this.txtInputChar.TabIndex = 1;
            this.txtInputChar.Text = "I";
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
            this.chkBorder.Location = new System.Drawing.Point(491, 39);
            this.chkBorder.Name = "chkBorder";
            this.chkBorder.Size = new System.Drawing.Size(57, 17);
            this.chkBorder.TabIndex = 3;
            this.chkBorder.Text = "Border";
            this.chkBorder.UseVisualStyleBackColor = true;
            // 
            // chkFillBackground
            // 
            this.chkFillBackground.AutoSize = true;
            this.chkFillBackground.Location = new System.Drawing.Point(491, 62);
            this.chkFillBackground.Name = "chkFillBackground";
            this.chkFillBackground.Size = new System.Drawing.Size(101, 17);
            this.chkFillBackground.TabIndex = 4;
            this.chkFillBackground.Text = "Fill BackGround";
            this.chkFillBackground.UseVisualStyleBackColor = true;
            this.chkFillBackground.CheckedChanged += new System.EventHandler(this.chkFillBackground_CheckedChanged);
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
            this.chkKern.Location = new System.Drawing.Point(491, 85);
            this.chkKern.Name = "chkKern";
            this.chkKern.Size = new System.Drawing.Size(85, 17);
            this.chkKern.TabIndex = 10;
            this.chkKern.Text = "Do Kerning1";
            this.chkKern.UseVisualStyleBackColor = true;
            this.chkKern.CheckedChanged += new System.EventHandler(this.chkKern_CheckedChanged);
            // 
            // chkTrueTypeHint
            // 
            this.chkTrueTypeHint.AutoSize = true;
            this.chkTrueTypeHint.Location = new System.Drawing.Point(491, 108);
            this.chkTrueTypeHint.Name = "chkTrueTypeHint";
            this.chkTrueTypeHint.Size = new System.Drawing.Size(125, 17);
            this.chkTrueTypeHint.TabIndex = 11;
            this.chkTrueTypeHint.Text = "Do TrueType Hinting";
            this.chkTrueTypeHint.UseVisualStyleBackColor = true;
            this.chkTrueTypeHint.CheckedChanged += new System.EventHandler(this.chkTrueTypeHint_CheckedChanged);
            // 
            // chkShowControlPoints
            // 
            this.chkShowControlPoints.AutoSize = true;
            this.chkShowControlPoints.Checked = true;
            this.chkShowControlPoints.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowControlPoints.Location = new System.Drawing.Point(495, 161);
            this.chkShowControlPoints.Name = "chkShowControlPoints";
            this.chkShowControlPoints.Size = new System.Drawing.Size(121, 17);
            this.chkShowControlPoints.TabIndex = 12;
            this.chkShowControlPoints.Text = "Show Control Points";
            this.chkShowControlPoints.UseVisualStyleBackColor = true;
            // 
            // chkShowTess
            // 
            this.chkShowTess.AutoSize = true;
            this.chkShowTess.Location = new System.Drawing.Point(494, 197);
            this.chkShowTess.Name = "chkShowTess";
            this.chkShowTess.Size = new System.Drawing.Size(110, 17);
            this.chkShowTess.TabIndex = 13;
            this.chkShowTess.Text = "Show Tesselation";
            this.chkShowTess.UseVisualStyleBackColor = true;
            this.chkShowTess.CheckedChanged += new System.EventHandler(this.chkShowTess_CheckedChanged);
            // 
            // chkShowGrid
            // 
            this.chkShowGrid.AutoSize = true;
            this.chkShowGrid.Location = new System.Drawing.Point(495, 243);
            this.chkShowGrid.Name = "chkShowGrid";
            this.chkShowGrid.Size = new System.Drawing.Size(75, 17);
            this.chkShowGrid.TabIndex = 14;
            this.chkShowGrid.Text = "Show Grid";
            this.chkShowGrid.UseVisualStyleBackColor = true;
            this.chkShowGrid.CheckedChanged += new System.EventHandler(this.chkShowGrid_CheckedChanged);
            // 
            // txtGridSize
            // 
            this.txtGridSize.Location = new System.Drawing.Point(565, 243);
            this.txtGridSize.Name = "txtGridSize";
            this.txtGridSize.Size = new System.Drawing.Size(51, 20);
            this.txtGridSize.TabIndex = 15;
            this.txtGridSize.Text = "5";
            // 
            // chkYGridFitting
            // 
            this.chkYGridFitting.AutoSize = true;
            this.chkYGridFitting.Checked = true;
            this.chkYGridFitting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkYGridFitting.Location = new System.Drawing.Point(495, 269);
            this.chkYGridFitting.Name = "chkYGridFitting";
            this.chkYGridFitting.Size = new System.Drawing.Size(111, 17);
            this.chkYGridFitting.TabIndex = 16;
            this.chkYGridFitting.Text = "Y Grid Auto Fitting";
            this.chkYGridFitting.UseVisualStyleBackColor = true;
            this.chkYGridFitting.CheckedChanged += new System.EventHandler(this.chkYGridFitting_CheckedChanged);
            // 
            // chkVerticalHinting
            // 
            this.chkVerticalHinting.AutoSize = true;
            this.chkVerticalHinting.Location = new System.Drawing.Point(510, 131);
            this.chkVerticalHinting.Name = "chkVerticalHinting";
            this.chkVerticalHinting.Size = new System.Drawing.Size(123, 17);
            this.chkVerticalHinting.TabIndex = 17;
            this.chkVerticalHinting.Text = "Agg\'s VerticalHinting";
            this.chkVerticalHinting.UseVisualStyleBackColor = true;
            this.chkVerticalHinting.CheckedChanged += new System.EventHandler(this.chkVerticalHinting_CheckedChanged);
            // 
            // chkDrawBone
            // 
            this.chkDrawBone.AutoSize = true;
            this.chkDrawBone.Checked = true;
            this.chkDrawBone.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawBone.Location = new System.Drawing.Point(495, 220);
            this.chkDrawBone.Name = "chkDrawBone";
            this.chkDrawBone.Size = new System.Drawing.Size(76, 17);
            this.chkDrawBone.TabIndex = 19;
            this.chkDrawBone.Text = "DrawBone";
            this.chkDrawBone.UseVisualStyleBackColor = true;
            this.chkDrawBone.CheckedChanged += new System.EventHandler(this.chkDrawBone_CheckedChanged);
            // 
            // chkXGridFitting
            // 
            this.chkXGridFitting.AutoSize = true;
            this.chkXGridFitting.Location = new System.Drawing.Point(495, 292);
            this.chkXGridFitting.Name = "chkXGridFitting";
            this.chkXGridFitting.Size = new System.Drawing.Size(111, 17);
            this.chkXGridFitting.TabIndex = 20;
            this.chkXGridFitting.Text = "X Grid Auto Fitting";
            this.chkXGridFitting.UseVisualStyleBackColor = true;
            this.chkXGridFitting.CheckedChanged += new System.EventHandler(this.chkXGridFitting_CheckedChanged);
            // 
            // chkLcdTechnique
            // 
            this.chkLcdTechnique.AutoSize = true;
            this.chkLcdTechnique.Location = new System.Drawing.Point(495, 315);
            this.chkLcdTechnique.Name = "chkLcdTechnique";
            this.chkLcdTechnique.Size = new System.Drawing.Size(95, 17);
            this.chkLcdTechnique.TabIndex = 21;
            this.chkLcdTechnique.Text = "LcdTechnique";
            this.chkLcdTechnique.UseVisualStyleBackColor = true;
            this.chkLcdTechnique.CheckedChanged += new System.EventHandler(this.chkLcdTechnique_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 427);
            this.Controls.Add(this.chkLcdTechnique);
            this.Controls.Add(this.chkXGridFitting);
            this.Controls.Add(this.chkDrawBone);
            this.Controls.Add(this.chkVerticalHinting);
            this.Controls.Add(this.chkYGridFitting);
            this.Controls.Add(this.txtGridSize);
            this.Controls.Add(this.chkShowGrid);
            this.Controls.Add(this.chkShowTess);
            this.Controls.Add(this.chkShowControlPoints);
            this.Controls.Add(this.chkTrueTypeHint);
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
        private System.Windows.Forms.CheckBox chkTrueTypeHint;
        private System.Windows.Forms.CheckBox chkShowControlPoints;
        private System.Windows.Forms.CheckBox chkShowTess;
        private System.Windows.Forms.CheckBox chkShowGrid;
        private System.Windows.Forms.TextBox txtGridSize;
        private System.Windows.Forms.CheckBox chkYGridFitting;
        private System.Windows.Forms.CheckBox chkVerticalHinting;
        private System.Windows.Forms.CheckBox chkDrawBone;
        private System.Windows.Forms.CheckBox chkXGridFitting;
        private System.Windows.Forms.CheckBox chkLcdTechnique;
    }
}

