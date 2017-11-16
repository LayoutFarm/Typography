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
            this.chkDrawCentroidBone = new System.Windows.Forms.CheckBox();
            this.chkXGridFitting = new System.Windows.Forms.CheckBox();
            this.chkLcdTechnique = new System.Windows.Forms.CheckBox();
            this.cmdBuildMsdfTexture = new System.Windows.Forms.Button();
            this.cmbPositionTech = new System.Windows.Forms.ComboBox();
            this.lstFontList = new System.Windows.Forms.ListBox();
            this.chkGsubEnableLigature = new System.Windows.Forms.CheckBox();
            this.lstHintList = new System.Windows.Forms.ListBox();
            this.chkShowSampleTextBox = new System.Windows.Forms.CheckBox();
            this.chkDrawGlyphBone = new System.Windows.Forms.CheckBox();
            this.chkDynamicOutline = new System.Windows.Forms.CheckBox();
            this.chkDrawTriangles = new System.Windows.Forms.CheckBox();
            this.chkDrawRegenerateOutline = new System.Windows.Forms.CheckBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.chkDrawLineHubConn = new System.Windows.Forms.CheckBox();
            this.chkDrawPerpendicularLine = new System.Windows.Forms.CheckBox();
            this.lstEdgeOffset = new System.Windows.Forms.ListBox();
            this.chkDrawGlyphPoint = new System.Windows.Forms.CheckBox();
            this.chkTestGridFit = new System.Windows.Forms.CheckBox();
            this.chkWriteFitOutputToConsole = new System.Windows.Forms.CheckBox();
            this.chkUseHorizontalFitAlign = new System.Windows.Forms.CheckBox();
            this.chkSetPrinterLayoutForLcdSubPix = new System.Windows.Forms.CheckBox();
            this.cmbScriptLangs = new System.Windows.Forms.ComboBox();
            this.sampleTextBox1 = new SampleWinForms.SampleTextBox();
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
            // chkBorder
            // 
            this.chkBorder.AutoSize = true;
            this.chkBorder.Location = new System.Drawing.Point(1170, 123);
            this.chkBorder.Name = "chkBorder";
            this.chkBorder.Size = new System.Drawing.Size(57, 17);
            this.chkBorder.TabIndex = 3;
            this.chkBorder.Text = "Border";
            this.chkBorder.UseVisualStyleBackColor = true;
            // 
            // chkFillBackground
            // 
            this.chkFillBackground.AutoSize = true;
            this.chkFillBackground.Location = new System.Drawing.Point(1170, 146);
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
            this.cmbRenderChoices.Location = new System.Drawing.Point(741, 12);
            this.cmbRenderChoices.Name = "cmbRenderChoices";
            this.cmbRenderChoices.Size = new System.Drawing.Size(224, 21);
            this.cmbRenderChoices.TabIndex = 7;
            // 
            // lstFontSizes
            // 
            this.lstFontSizes.FormattingEnabled = true;
            this.lstFontSizes.Location = new System.Drawing.Point(746, 167);
            this.lstFontSizes.Name = "lstFontSizes";
            this.lstFontSizes.Size = new System.Drawing.Size(121, 212);
            this.lstFontSizes.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(743, 151);
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
            this.chkShowControlPoints.Location = new System.Drawing.Point(1170, 168);
            this.chkShowControlPoints.Name = "chkShowControlPoints";
            this.chkShowControlPoints.Size = new System.Drawing.Size(121, 17);
            this.chkShowControlPoints.TabIndex = 12;
            this.chkShowControlPoints.Text = "Show Control Points";
            this.chkShowControlPoints.UseVisualStyleBackColor = true;
            // 
            // chkShowTess
            // 
            this.chkShowTess.AutoSize = true;
            this.chkShowTess.Location = new System.Drawing.Point(1029, 173);
            this.chkShowTess.Name = "chkShowTess";
            this.chkShowTess.Size = new System.Drawing.Size(110, 17);
            this.chkShowTess.TabIndex = 13;
            this.chkShowTess.Text = "Show Tesselation";
            this.chkShowTess.UseVisualStyleBackColor = true;
            // 
            // chkShowGrid
            // 
            this.chkShowGrid.AutoSize = true;
            this.chkShowGrid.Location = new System.Drawing.Point(883, 173);
            this.chkShowGrid.Name = "chkShowGrid";
            this.chkShowGrid.Size = new System.Drawing.Size(75, 17);
            this.chkShowGrid.TabIndex = 14;
            this.chkShowGrid.Text = "Show Grid";
            this.chkShowGrid.UseVisualStyleBackColor = true;
            // 
            // txtGridSize
            // 
            this.txtGridSize.Location = new System.Drawing.Point(953, 173);
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
            this.chkYGridFitting.Location = new System.Drawing.Point(885, 225);
            this.chkYGridFitting.Name = "chkYGridFitting";
            this.chkYGridFitting.Size = new System.Drawing.Size(111, 17);
            this.chkYGridFitting.TabIndex = 16;
            this.chkYGridFitting.Text = "Y Grid Auto Fitting";
            this.chkYGridFitting.UseVisualStyleBackColor = true;
            // 
            // chkDrawCentroidBone
            // 
            this.chkDrawCentroidBone.AutoSize = true;
            this.chkDrawCentroidBone.Location = new System.Drawing.Point(1045, 223);
            this.chkDrawCentroidBone.Name = "chkDrawCentroidBone";
            this.chkDrawCentroidBone.Size = new System.Drawing.Size(121, 17);
            this.chkDrawCentroidBone.TabIndex = 19;
            this.chkDrawCentroidBone.Text = "Draw Centroid Bone";
            this.chkDrawCentroidBone.UseVisualStyleBackColor = true;
            // 
            // chkXGridFitting
            // 
            this.chkXGridFitting.AutoSize = true;
            this.chkXGridFitting.Location = new System.Drawing.Point(885, 248);
            this.chkXGridFitting.Name = "chkXGridFitting";
            this.chkXGridFitting.Size = new System.Drawing.Size(111, 17);
            this.chkXGridFitting.TabIndex = 20;
            this.chkXGridFitting.Text = "X Grid Auto Fitting";
            this.chkXGridFitting.UseVisualStyleBackColor = true;
            // 
            // chkLcdTechnique
            // 
            this.chkLcdTechnique.AutoSize = true;
            this.chkLcdTechnique.Location = new System.Drawing.Point(885, 271);
            this.chkLcdTechnique.Name = "chkLcdTechnique";
            this.chkLcdTechnique.Size = new System.Drawing.Size(95, 17);
            this.chkLcdTechnique.TabIndex = 21;
            this.chkLcdTechnique.Text = "LcdTechnique";
            this.chkLcdTechnique.UseVisualStyleBackColor = true;
            // 
            // cmdBuildMsdfTexture
            // 
            this.cmdBuildMsdfTexture.Location = new System.Drawing.Point(1191, 453);
            this.cmdBuildMsdfTexture.Name = "cmdBuildMsdfTexture";
            this.cmdBuildMsdfTexture.Size = new System.Drawing.Size(121, 28);
            this.cmdBuildMsdfTexture.TabIndex = 22;
            this.cmdBuildMsdfTexture.Text = "Make MsdfTexture";
            this.cmdBuildMsdfTexture.UseVisualStyleBackColor = true;
            this.cmdBuildMsdfTexture.Click += new System.EventHandler(this.cmdBuildMsdfTexture_Click);
            // 
            // cmbPositionTech
            // 
            this.cmbPositionTech.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPositionTech.FormattingEnabled = true;
            this.cmbPositionTech.Location = new System.Drawing.Point(741, 39);
            this.cmbPositionTech.Name = "cmbPositionTech";
            this.cmbPositionTech.Size = new System.Drawing.Size(224, 21);
            this.cmbPositionTech.TabIndex = 23;
            // 
            // lstFontList
            // 
            this.lstFontList.FormattingEnabled = true;
            this.lstFontList.Location = new System.Drawing.Point(981, 12);
            this.lstFontList.Name = "lstFontList";
            this.lstFontList.Size = new System.Drawing.Size(139, 121);
            this.lstFontList.TabIndex = 25;
            // 
            // chkGsubEnableLigature
            // 
            this.chkGsubEnableLigature.AutoSize = true;
            this.chkGsubEnableLigature.Checked = true;
            this.chkGsubEnableLigature.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGsubEnableLigature.Location = new System.Drawing.Point(1171, 96);
            this.chkGsubEnableLigature.Name = "chkGsubEnableLigature";
            this.chkGsubEnableLigature.Size = new System.Drawing.Size(136, 17);
            this.chkGsubEnableLigature.TabIndex = 26;
            this.chkGsubEnableLigature.Text = "GSUB: Enable Ligature";
            this.chkGsubEnableLigature.UseVisualStyleBackColor = true;
            // 
            // lstHintList
            // 
            this.lstHintList.FormattingEnabled = true;
            this.lstHintList.Location = new System.Drawing.Point(741, 71);
            this.lstHintList.Name = "lstHintList";
            this.lstHintList.Size = new System.Drawing.Size(224, 69);
            this.lstHintList.TabIndex = 27;
            // 
            // chkShowSampleTextBox
            // 
            this.chkShowSampleTextBox.AutoSize = true;
            this.chkShowSampleTextBox.Location = new System.Drawing.Point(883, 146);
            this.chkShowSampleTextBox.Name = "chkShowSampleTextBox";
            this.chkShowSampleTextBox.Size = new System.Drawing.Size(133, 17);
            this.chkShowSampleTextBox.TabIndex = 39;
            this.chkShowSampleTextBox.Text = "Show Sample TextBox";
            this.chkShowSampleTextBox.UseVisualStyleBackColor = true;
            this.chkShowSampleTextBox.CheckedChanged += new System.EventHandler(this.chkShowSampleTextBox_CheckedChanged);
            // 
            // chkDrawGlyphBone
            // 
            this.chkDrawGlyphBone.AutoSize = true;
            this.chkDrawGlyphBone.Checked = true;
            this.chkDrawGlyphBone.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawGlyphBone.Location = new System.Drawing.Point(1045, 252);
            this.chkDrawGlyphBone.Name = "chkDrawGlyphBone";
            this.chkDrawGlyphBone.Size = new System.Drawing.Size(109, 17);
            this.chkDrawGlyphBone.TabIndex = 47;
            this.chkDrawGlyphBone.Text = "Draw Glyph Bone";
            this.chkDrawGlyphBone.UseVisualStyleBackColor = true;
            // 
            // chkDynamicOutline
            // 
            this.chkDynamicOutline.AutoSize = true;
            this.chkDynamicOutline.Location = new System.Drawing.Point(1024, 306);
            this.chkDynamicOutline.Name = "chkDynamicOutline";
            this.chkDynamicOutline.Size = new System.Drawing.Size(130, 17);
            this.chkDynamicOutline.TabIndex = 51;
            this.chkDynamicOutline.Text = "Show DynamicOutline";
            this.chkDynamicOutline.UseVisualStyleBackColor = true;
            // 
            // chkDrawTriangles
            // 
            this.chkDrawTriangles.AutoSize = true;
            this.chkDrawTriangles.Checked = true;
            this.chkDrawTriangles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawTriangles.Location = new System.Drawing.Point(1045, 196);
            this.chkDrawTriangles.Name = "chkDrawTriangles";
            this.chkDrawTriangles.Size = new System.Drawing.Size(94, 17);
            this.chkDrawTriangles.TabIndex = 54;
            this.chkDrawTriangles.Text = "DrawTriangles";
            this.chkDrawTriangles.UseVisualStyleBackColor = true;
            // 
            // chkDrawRegenerateOutline
            // 
            this.chkDrawRegenerateOutline.AutoSize = true;
            this.chkDrawRegenerateOutline.Location = new System.Drawing.Point(1044, 329);
            this.chkDrawRegenerateOutline.Name = "chkDrawRegenerateOutline";
            this.chkDrawRegenerateOutline.Size = new System.Drawing.Size(157, 17);
            this.chkDrawRegenerateOutline.TabIndex = 55;
            this.chkDrawRegenerateOutline.Text = "Draw Regenerated Outlines";
            this.chkDrawRegenerateOutline.UseVisualStyleBackColor = true;
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(746, 496);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(393, 349);
            this.treeView1.TabIndex = 56;
            // 
            // chkDrawLineHubConn
            // 
            this.chkDrawLineHubConn.AutoSize = true;
            this.chkDrawLineHubConn.Checked = true;
            this.chkDrawLineHubConn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawLineHubConn.Location = new System.Drawing.Point(1045, 275);
            this.chkDrawLineHubConn.Name = "chkDrawLineHubConn";
            this.chkDrawLineHubConn.Size = new System.Drawing.Size(122, 17);
            this.chkDrawLineHubConn.TabIndex = 57;
            this.chkDrawLineHubConn.Text = "Draw LineHub Conn";
            this.chkDrawLineHubConn.UseVisualStyleBackColor = true;
            // 
            // chkDrawPerpendicularLine
            // 
            this.chkDrawPerpendicularLine.AutoSize = true;
            this.chkDrawPerpendicularLine.Checked = true;
            this.chkDrawPerpendicularLine.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawPerpendicularLine.Location = new System.Drawing.Point(1170, 196);
            this.chkDrawPerpendicularLine.Name = "chkDrawPerpendicularLine";
            this.chkDrawPerpendicularLine.Size = new System.Drawing.Size(142, 17);
            this.chkDrawPerpendicularLine.TabIndex = 58;
            this.chkDrawPerpendicularLine.Text = "Draw Perpendicular Line";
            this.chkDrawPerpendicularLine.UseVisualStyleBackColor = true;
            // 
            // lstEdgeOffset
            // 
            this.lstEdgeOffset.FormattingEnabled = true;
            this.lstEdgeOffset.Location = new System.Drawing.Point(746, 386);
            this.lstEdgeOffset.Name = "lstEdgeOffset";
            this.lstEdgeOffset.Size = new System.Drawing.Size(120, 95);
            this.lstEdgeOffset.TabIndex = 60;
            // 
            // chkDrawGlyphPoint
            // 
            this.chkDrawGlyphPoint.AutoSize = true;
            this.chkDrawGlyphPoint.Checked = true;
            this.chkDrawGlyphPoint.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawGlyphPoint.Location = new System.Drawing.Point(1170, 225);
            this.chkDrawGlyphPoint.Name = "chkDrawGlyphPoint";
            this.chkDrawGlyphPoint.Size = new System.Drawing.Size(105, 17);
            this.chkDrawGlyphPoint.TabIndex = 61;
            this.chkDrawGlyphPoint.Text = "Draw GlyphPoint";
            this.chkDrawGlyphPoint.UseVisualStyleBackColor = true;
            // 
            // chkTestGridFit
            // 
            this.chkTestGridFit.AutoSize = true;
            this.chkTestGridFit.Checked = true;
            this.chkTestGridFit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTestGridFit.Location = new System.Drawing.Point(1170, 252);
            this.chkTestGridFit.Name = "chkTestGridFit";
            this.chkTestGridFit.Size = new System.Drawing.Size(80, 17);
            this.chkTestGridFit.TabIndex = 62;
            this.chkTestGridFit.Text = "Test GridFit";
            this.chkTestGridFit.UseVisualStyleBackColor = true;
            // 
            // chkWriteFitOutputToConsole
            // 
            this.chkWriteFitOutputToConsole.AutoSize = true;
            this.chkWriteFitOutputToConsole.Checked = true;
            this.chkWriteFitOutputToConsole.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWriteFitOutputToConsole.Location = new System.Drawing.Point(1187, 289);
            this.chkWriteFitOutputToConsole.Name = "chkWriteFitOutputToConsole";
            this.chkWriteFitOutputToConsole.Size = new System.Drawing.Size(150, 17);
            this.chkWriteFitOutputToConsole.TabIndex = 63;
            this.chkWriteFitOutputToConsole.Text = "WriteFit Output to Console";
            this.chkWriteFitOutputToConsole.UseVisualStyleBackColor = true;
            // 
            // chkUseHorizontalFitAlign
            // 
            this.chkUseHorizontalFitAlign.AutoSize = true;
            this.chkUseHorizontalFitAlign.Checked = true;
            this.chkUseHorizontalFitAlign.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseHorizontalFitAlign.Location = new System.Drawing.Point(1187, 312);
            this.chkUseHorizontalFitAlign.Name = "chkUseHorizontalFitAlign";
            this.chkUseHorizontalFitAlign.Size = new System.Drawing.Size(99, 17);
            this.chkUseHorizontalFitAlign.TabIndex = 64;
            this.chkUseHorizontalFitAlign.Text = "Use H_Fit Align";
            this.chkUseHorizontalFitAlign.UseVisualStyleBackColor = true;
            // 
            // chkSetPrinterLayoutForLcdSubPix
            // 
            this.chkSetPrinterLayoutForLcdSubPix.AutoSize = true;
            this.chkSetPrinterLayoutForLcdSubPix.Location = new System.Drawing.Point(885, 294);
            this.chkSetPrinterLayoutForLcdSubPix.Name = "chkSetPrinterLayoutForLcdSubPix";
            this.chkSetPrinterLayoutForLcdSubPix.Size = new System.Drawing.Size(130, 17);
            this.chkSetPrinterLayoutForLcdSubPix.TabIndex = 65;
            this.chkSetPrinterLayoutForLcdSubPix.Text = "Layout For LcdSubPix";
            this.chkSetPrinterLayoutForLcdSubPix.UseVisualStyleBackColor = true;
            // 
            // cmbScriptLangs
            // 
            this.cmbScriptLangs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScriptLangs.FormattingEnabled = true;
            this.cmbScriptLangs.Location = new System.Drawing.Point(1147, 12);
            this.cmbScriptLangs.Name = "cmbScriptLangs";
            this.cmbScriptLangs.Size = new System.Drawing.Size(200, 21);
            this.cmbScriptLangs.TabIndex = 66;
            // 
            // sampleTextBox1
            // 
            this.sampleTextBox1.BackColor = System.Drawing.Color.Silver;
            this.sampleTextBox1.Location = new System.Drawing.Point(12, 71);
            this.sampleTextBox1.Name = "sampleTextBox1";
            this.sampleTextBox1.Size = new System.Drawing.Size(505, 774);
            this.sampleTextBox1.TabIndex = 40;
            this.sampleTextBox1.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1393, 857);
            this.Controls.Add(this.cmbScriptLangs);
            this.Controls.Add(this.chkSetPrinterLayoutForLcdSubPix);
            this.Controls.Add(this.chkUseHorizontalFitAlign);
            this.Controls.Add(this.chkWriteFitOutputToConsole);
            this.Controls.Add(this.chkTestGridFit);
            this.Controls.Add(this.chkDrawGlyphPoint);
            this.Controls.Add(this.lstEdgeOffset);
            this.Controls.Add(this.chkDrawPerpendicularLine);
            this.Controls.Add(this.chkDrawLineHubConn);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.chkDrawRegenerateOutline);
            this.Controls.Add(this.chkDrawTriangles);
            this.Controls.Add(this.chkDynamicOutline);
            this.Controls.Add(this.chkDrawGlyphBone);
            this.Controls.Add(this.sampleTextBox1);
            this.Controls.Add(this.chkShowSampleTextBox);
            this.Controls.Add(this.lstHintList);
            this.Controls.Add(this.chkGsubEnableLigature);
            this.Controls.Add(this.lstFontList);
            this.Controls.Add(this.cmbPositionTech);
            this.Controls.Add(this.cmdBuildMsdfTexture);
            this.Controls.Add(this.chkLcdTechnique);
            this.Controls.Add(this.chkXGridFitting);
            this.Controls.Add(this.chkDrawCentroidBone);
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
        private System.Windows.Forms.CheckBox chkDrawCentroidBone;
        private System.Windows.Forms.CheckBox chkXGridFitting;
        private System.Windows.Forms.CheckBox chkLcdTechnique;
        private System.Windows.Forms.Button cmdBuildMsdfTexture;
        private System.Windows.Forms.ComboBox cmbPositionTech;
       
        private System.Windows.Forms.ListBox lstFontList;
        private System.Windows.Forms.CheckBox chkGsubEnableLigature;
        private System.Windows.Forms.ListBox lstHintList;
        private System.Windows.Forms.CheckBox chkShowSampleTextBox;
        private SampleTextBox sampleTextBox1;
        private System.Windows.Forms.CheckBox chkDrawGlyphBone;
        private System.Windows.Forms.CheckBox chkDynamicOutline;
        private System.Windows.Forms.CheckBox chkDrawTriangles;
        private System.Windows.Forms.CheckBox chkDrawRegenerateOutline;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.CheckBox chkDrawLineHubConn;
        private System.Windows.Forms.CheckBox chkDrawPerpendicularLine;
        private System.Windows.Forms.ListBox lstEdgeOffset;
        private System.Windows.Forms.CheckBox chkDrawGlyphPoint;
        private System.Windows.Forms.CheckBox chkTestGridFit;
        private System.Windows.Forms.CheckBox chkWriteFitOutputToConsole;
        private System.Windows.Forms.CheckBox chkUseHorizontalFitAlign;
        private System.Windows.Forms.CheckBox chkSetPrinterLayoutForLcdSubPix;
        private System.Windows.Forms.ComboBox cmbScriptLangs;
    }
}

