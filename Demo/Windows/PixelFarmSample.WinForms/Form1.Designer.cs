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
            this.chkBorder = new System.Windows.Forms.CheckBox();
            this.chkFillBackground = new System.Windows.Forms.CheckBox();
            this.cmbRenderChoices = new System.Windows.Forms.ComboBox();
            this.lstFontSizes = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkShowControlPoints = new System.Windows.Forms.CheckBox();
            this.chkShowTess = new System.Windows.Forms.CheckBox();
            this.chkShowGrid = new System.Windows.Forms.CheckBox();
            this.txtGridSize = new System.Windows.Forms.TextBox();
            this.chkYGridFitting = new System.Windows.Forms.CheckBox();
            this.chkDrawBone = new System.Windows.Forms.CheckBox();
            this.chkXGridFitting = new System.Windows.Forms.CheckBox();
            this.chkLcdTechnique = new System.Windows.Forms.CheckBox();
            this.cmdBuildMsdfTexture = new System.Windows.Forms.Button();
            this.cmbPositionTech = new System.Windows.Forms.ComboBox();
            this.lstFontList = new System.Windows.Forms.ListBox();
            this.chkGsubEnableLigature = new System.Windows.Forms.CheckBox();
            this.lstHintList = new System.Windows.Forms.ListBox();
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
            // 
            // txtInputChar
            // 
            this.txtInputChar.Location = new System.Drawing.Point(12, 11);
            this.txtInputChar.Name = "txtInputChar";
            this.txtInputChar.Size = new System.Drawing.Size(168, 20);
            this.txtInputChar.TabIndex = 1;
            this.txtInputChar.Text = "I";
            // 
            // chkBorder
            // 
            this.chkBorder.AutoSize = true;
            this.chkBorder.Location = new System.Drawing.Point(937, 41);
            this.chkBorder.Name = "chkBorder";
            this.chkBorder.Size = new System.Drawing.Size(57, 17);
            this.chkBorder.TabIndex = 3;
            this.chkBorder.Text = "Border";
            this.chkBorder.UseVisualStyleBackColor = true;
            // 
            // chkFillBackground
            // 
            this.chkFillBackground.AutoSize = true;
            this.chkFillBackground.Location = new System.Drawing.Point(937, 64);
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
            this.cmbRenderChoices.Location = new System.Drawing.Point(523, 12);
            this.cmbRenderChoices.Name = "cmbRenderChoices";
            this.cmbRenderChoices.Size = new System.Drawing.Size(224, 21);
            this.cmbRenderChoices.TabIndex = 7;
            // 
            // lstFontSizes
            // 
            this.lstFontSizes.FormattingEnabled = true;
            this.lstFontSizes.Location = new System.Drawing.Point(800, 162);
            this.lstFontSizes.Name = "lstFontSizes";
            this.lstFontSizes.Size = new System.Drawing.Size(121, 212);
            this.lstFontSizes.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(797, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Size in Points";
            // 
            // chkShowControlPoints
            // 
            this.chkShowControlPoints.AutoSize = true;
            this.chkShowControlPoints.Checked = true;
            this.chkShowControlPoints.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowControlPoints.Location = new System.Drawing.Point(937, 86);
            this.chkShowControlPoints.Name = "chkShowControlPoints";
            this.chkShowControlPoints.Size = new System.Drawing.Size(121, 17);
            this.chkShowControlPoints.TabIndex = 12;
            this.chkShowControlPoints.Text = "Show Control Points";
            this.chkShowControlPoints.UseVisualStyleBackColor = true;
            // 
            // chkShowTess
            // 
            this.chkShowTess.AutoSize = true;
            this.chkShowTess.Location = new System.Drawing.Point(936, 122);
            this.chkShowTess.Name = "chkShowTess";
            this.chkShowTess.Size = new System.Drawing.Size(110, 17);
            this.chkShowTess.TabIndex = 13;
            this.chkShowTess.Text = "Show Tesselation";
            this.chkShowTess.UseVisualStyleBackColor = true;
            // 
            // chkShowGrid
            // 
            this.chkShowGrid.AutoSize = true;
            this.chkShowGrid.Location = new System.Drawing.Point(937, 168);
            this.chkShowGrid.Name = "chkShowGrid";
            this.chkShowGrid.Size = new System.Drawing.Size(75, 17);
            this.chkShowGrid.TabIndex = 14;
            this.chkShowGrid.Text = "Show Grid";
            this.chkShowGrid.UseVisualStyleBackColor = true;
            // 
            // txtGridSize
            // 
            this.txtGridSize.Location = new System.Drawing.Point(1007, 168);
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
            this.chkYGridFitting.Location = new System.Drawing.Point(939, 220);
            this.chkYGridFitting.Name = "chkYGridFitting";
            this.chkYGridFitting.Size = new System.Drawing.Size(111, 17);
            this.chkYGridFitting.TabIndex = 16;
            this.chkYGridFitting.Text = "Y Grid Auto Fitting";
            this.chkYGridFitting.UseVisualStyleBackColor = true;
            // 
            // chkDrawBone
            // 
            this.chkDrawBone.AutoSize = true;
            this.chkDrawBone.Checked = true;
            this.chkDrawBone.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawBone.Location = new System.Drawing.Point(937, 145);
            this.chkDrawBone.Name = "chkDrawBone";
            this.chkDrawBone.Size = new System.Drawing.Size(76, 17);
            this.chkDrawBone.TabIndex = 19;
            this.chkDrawBone.Text = "DrawBone";
            this.chkDrawBone.UseVisualStyleBackColor = true;
            // 
            // chkXGridFitting
            // 
            this.chkXGridFitting.AutoSize = true;
            this.chkXGridFitting.Location = new System.Drawing.Point(939, 243);
            this.chkXGridFitting.Name = "chkXGridFitting";
            this.chkXGridFitting.Size = new System.Drawing.Size(111, 17);
            this.chkXGridFitting.TabIndex = 20;
            this.chkXGridFitting.Text = "X Grid Auto Fitting";
            this.chkXGridFitting.UseVisualStyleBackColor = true;
            // 
            // chkLcdTechnique
            // 
            this.chkLcdTechnique.AutoSize = true;
            this.chkLcdTechnique.Location = new System.Drawing.Point(939, 266);
            this.chkLcdTechnique.Name = "chkLcdTechnique";
            this.chkLcdTechnique.Size = new System.Drawing.Size(95, 17);
            this.chkLcdTechnique.TabIndex = 21;
            this.chkLcdTechnique.Text = "LcdTechnique";
            this.chkLcdTechnique.UseVisualStyleBackColor = true;
            // 
            // cmdBuildMsdfTexture
            // 
            this.cmdBuildMsdfTexture.Location = new System.Drawing.Point(800, 379);
            this.cmdBuildMsdfTexture.Name = "cmdBuildMsdfTexture";
            this.cmdBuildMsdfTexture.Size = new System.Drawing.Size(121, 37);
            this.cmdBuildMsdfTexture.TabIndex = 22;
            this.cmdBuildMsdfTexture.Text = "Make MsdfTexture";
            this.cmdBuildMsdfTexture.UseVisualStyleBackColor = true;
            this.cmdBuildMsdfTexture.Click += new System.EventHandler(this.cmdBuildMsdfTexture_Click);
            // 
            // cmbPositionTech
            // 
            this.cmbPositionTech.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPositionTech.FormattingEnabled = true;
            this.cmbPositionTech.Location = new System.Drawing.Point(523, 39);
            this.cmbPositionTech.Name = "cmbPositionTech";
            this.cmbPositionTech.Size = new System.Drawing.Size(224, 21);
            this.cmbPositionTech.TabIndex = 23;
            // 
            // lstFontList
            // 
            this.lstFontList.FormattingEnabled = true;
            this.lstFontList.Location = new System.Drawing.Point(800, 12);
            this.lstFontList.Name = "lstFontList";
            this.lstFontList.Size = new System.Drawing.Size(121, 121);
            this.lstFontList.TabIndex = 25;
            // 
            // chkGsubEnableLigature
            // 
            this.chkGsubEnableLigature.AutoSize = true;
            this.chkGsubEnableLigature.Checked = true;
            this.chkGsubEnableLigature.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGsubEnableLigature.Location = new System.Drawing.Point(938, 14);
            this.chkGsubEnableLigature.Name = "chkGsubEnableLigature";
            this.chkGsubEnableLigature.Size = new System.Drawing.Size(136, 17);
            this.chkGsubEnableLigature.TabIndex = 26;
            this.chkGsubEnableLigature.Text = "GSUB: Enable Ligature";
            this.chkGsubEnableLigature.UseVisualStyleBackColor = true;
            // 
            // lstHintList
            // 
            this.lstHintList.FormattingEnabled = true;
            this.lstHintList.Location = new System.Drawing.Point(523, 71);
            this.lstHintList.Name = "lstHintList";
            this.lstHintList.Size = new System.Drawing.Size(224, 69);
            this.lstHintList.TabIndex = 27;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1086, 526);
            this.Controls.Add(this.lstHintList);
            this.Controls.Add(this.chkGsubEnableLigature);
            this.Controls.Add(this.lstFontList);
            this.Controls.Add(this.cmbPositionTech);
            this.Controls.Add(this.cmdBuildMsdfTexture);
            this.Controls.Add(this.chkLcdTechnique);
            this.Controls.Add(this.chkXGridFitting);
            this.Controls.Add(this.chkDrawBone);
            this.Controls.Add(this.chkYGridFitting);
            this.Controls.Add(this.txtGridSize);
            this.Controls.Add(this.chkShowGrid);
            this.Controls.Add(this.chkShowTess);
            this.Controls.Add(this.chkShowControlPoints);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstFontSizes);
            this.Controls.Add(this.cmbRenderChoices);
            this.Controls.Add(this.chkFillBackground);
            this.Controls.Add(this.chkBorder);
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
        private System.Windows.Forms.CheckBox chkBorder;
        private System.Windows.Forms.CheckBox chkFillBackground;
        private System.Windows.Forms.ComboBox cmbRenderChoices;
        private System.Windows.Forms.ListBox lstFontSizes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkShowControlPoints;
        private System.Windows.Forms.CheckBox chkShowTess;
        private System.Windows.Forms.CheckBox chkShowGrid;
        private System.Windows.Forms.TextBox txtGridSize;
        private System.Windows.Forms.CheckBox chkYGridFitting;
        private System.Windows.Forms.CheckBox chkDrawBone;
        private System.Windows.Forms.CheckBox chkXGridFitting;
        private System.Windows.Forms.CheckBox chkLcdTechnique;
        private System.Windows.Forms.Button cmdBuildMsdfTexture;
        private System.Windows.Forms.ComboBox cmbPositionTech;
       
        private System.Windows.Forms.ListBox lstFontList;
        private System.Windows.Forms.CheckBox chkGsubEnableLigature;
        private System.Windows.Forms.ListBox lstHintList;
    }
}

