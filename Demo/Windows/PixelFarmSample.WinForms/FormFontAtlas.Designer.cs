namespace SampleWinForms
{
    partial class FormFontAtlas
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
            this.txtSampleChars = new System.Windows.Forms.TextBox();
            this.cmdBuildAtlasFromText = new System.Windows.Forms.Button();
            this.picOutput = new System.Windows.Forms.PictureBox();
            this.lblOutput = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdBuildFromSelectedScriptLangs = new System.Windows.Forms.Button();
            this.txtSelectedFontSize = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbTextureKind = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.picOutput)).BeginInit();
            this.SuspendLayout();
            // 
            // txtSampleChars
            // 
            this.txtSampleChars.Location = new System.Drawing.Point(17, 683);
            this.txtSampleChars.Name = "txtSampleChars";
            this.txtSampleChars.Size = new System.Drawing.Size(119, 20);
            this.txtSampleChars.TabIndex = 75;
            this.txtSampleChars.Text = "sample!";
            // 
            // cmdBuildAtlasFromText
            // 
            this.cmdBuildAtlasFromText.Location = new System.Drawing.Point(142, 681);
            this.cmdBuildAtlasFromText.Name = "cmdBuildAtlasFromText";
            this.cmdBuildAtlasFromText.Size = new System.Drawing.Size(111, 23);
            this.cmdBuildAtlasFromText.TabIndex = 76;
            this.cmdBuildAtlasFromText.Text = "BuildAtlas from Text";
            this.cmdBuildAtlasFromText.UseVisualStyleBackColor = true;
            this.cmdBuildAtlasFromText.Click += new System.EventHandler(this.cmdBuildAtlasFromText_Click);
            // 
            // picOutput
            // 
            this.picOutput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.picOutput.Location = new System.Drawing.Point(331, 65);
            this.picOutput.Name = "picOutput";
            this.picOutput.Size = new System.Drawing.Size(820, 600);
            this.picOutput.TabIndex = 77;
            this.picOutput.TabStop = false;
            // 
            // lblOutput
            // 
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(328, 49);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(39, 13);
            this.lblOutput.TabIndex = 78;
            this.lblOutput.Text = "Output";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(13, 65);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(312, 600);
            this.flowLayoutPanel1.TabIndex = 79;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 80;
            this.label1.Text = "ScriptLangs";
            // 
            // cmdBuildFromSelectedScriptLangs
            // 
            this.cmdBuildFromSelectedScriptLangs.Location = new System.Drawing.Point(331, 10);
            this.cmdBuildFromSelectedScriptLangs.Name = "cmdBuildFromSelectedScriptLangs";
            this.cmdBuildFromSelectedScriptLangs.Size = new System.Drawing.Size(157, 36);
            this.cmdBuildFromSelectedScriptLangs.TabIndex = 81;
            this.cmdBuildFromSelectedScriptLangs.Text = "BuildAtlas from selected ScriptLangs";
            this.cmdBuildFromSelectedScriptLangs.UseVisualStyleBackColor = true;
            this.cmdBuildFromSelectedScriptLangs.Click += new System.EventHandler(this.cmdBuildFromSelectedScriptLangs_Click);
            // 
            // txtSelectedFontSize
            // 
            this.txtSelectedFontSize.Location = new System.Drawing.Point(108, 24);
            this.txtSelectedFontSize.Name = "txtSelectedFontSize";
            this.txtSelectedFontSize.Size = new System.Drawing.Size(54, 20);
            this.txtSelectedFontSize.TabIndex = 82;
            this.txtSelectedFontSize.Text = "24";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 83;
            this.label2.Text = "Font Size (points)";
            // 
            // cmbTextureKind
            // 
            this.cmbTextureKind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTextureKind.FormattingEnabled = true;
            this.cmbTextureKind.Location = new System.Drawing.Point(168, 25);
            this.cmbTextureKind.Name = "cmbTextureKind";
            this.cmbTextureKind.Size = new System.Drawing.Size(157, 21);
            this.cmbTextureKind.TabIndex = 84;
            // 
            // FormFontAtlas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1163, 727);
            this.Controls.Add(this.cmbTextureKind);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtSelectedFontSize);
            this.Controls.Add(this.cmdBuildFromSelectedScriptLangs);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.picOutput);
            this.Controls.Add(this.cmdBuildAtlasFromText);
            this.Controls.Add(this.txtSampleChars);
            this.Name = "FormFontAtlas";
            this.Text = "FormFontAtlas";
            this.Load += new System.EventHandler(this.FormFontAtlas_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picOutput)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSampleChars;
        private System.Windows.Forms.Button cmdBuildAtlasFromText;
        private System.Windows.Forms.PictureBox picOutput;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdBuildFromSelectedScriptLangs;
        private System.Windows.Forms.TextBox txtSelectedFontSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbTextureKind;
    }
}