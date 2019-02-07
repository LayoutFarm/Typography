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
            this.cmdTestMsdfGen = new System.Windows.Forms.Button();
            this.cmdTestMsdfGen2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmdTestMsdfGen
            // 
            this.cmdTestMsdfGen.Location = new System.Drawing.Point(12, 33);
            this.cmdTestMsdfGen.Name = "cmdTestMsdfGen";
            this.cmdTestMsdfGen.Size = new System.Drawing.Size(115, 44);
            this.cmdTestMsdfGen.TabIndex = 0;
            this.cmdTestMsdfGen.Text = "TestMsdfGen";
            this.cmdTestMsdfGen.UseVisualStyleBackColor = true;
            this.cmdTestMsdfGen.Click += new System.EventHandler(this.cmdTestMsdfGen_Click);
            // 
            // cmdTestMsdfGen2
            // 
            this.cmdTestMsdfGen2.Location = new System.Drawing.Point(12, 96);
            this.cmdTestMsdfGen2.Name = "cmdTestMsdfGen2";
            this.cmdTestMsdfGen2.Size = new System.Drawing.Size(115, 44);
            this.cmdTestMsdfGen2.TabIndex = 1;
            this.cmdTestMsdfGen2.Text = "TestMsdfGen2";
            this.cmdTestMsdfGen2.UseVisualStyleBackColor = true;
            this.cmdTestMsdfGen2.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormMsdfTest2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(631, 650);
            this.Controls.Add(this.cmdTestMsdfGen2);
            this.Controls.Add(this.cmdTestMsdfGen);
            this.Name = "FormMsdfTest2";
            this.Text = "FormMsdfTest2";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdTestMsdfGen;
        private System.Windows.Forms.Button cmdTestMsdfGen2;
    }
}