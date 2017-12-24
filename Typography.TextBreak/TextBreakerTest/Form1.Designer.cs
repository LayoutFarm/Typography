namespace TextBreakerTest
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.cmdReadDict = new System.Windows.Forms.Button();
            this.cmdManagedBreaker = new System.Windows.Forms.Button();
            this.cmdIcuTest = new System.Windows.Forms.Button();
            this.cmdPerformace1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 279);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(265, 212);
            this.listBox1.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(13, 85);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(264, 188);
            this.textBox1.TabIndex = 2;
            // 
            // cmdReadDict
            // 
            this.cmdReadDict.Location = new System.Drawing.Point(283, 456);
            this.cmdReadDict.Name = "cmdReadDict";
            this.cmdReadDict.Size = new System.Drawing.Size(130, 35);
            this.cmdReadDict.TabIndex = 3;
            this.cmdReadDict.Text = "ReadDict not complete!";
            this.cmdReadDict.UseVisualStyleBackColor = true;
            this.cmdReadDict.Click += new System.EventHandler(this.cmdReadDict_Click);
            // 
            // cmdManagedBreaker
            // 
            this.cmdManagedBreaker.Location = new System.Drawing.Point(13, 44);
            this.cmdManagedBreaker.Name = "cmdManagedBreaker";
            this.cmdManagedBreaker.Size = new System.Drawing.Size(130, 35);
            this.cmdManagedBreaker.TabIndex = 4;
            this.cmdManagedBreaker.Text = "Managed";
            this.cmdManagedBreaker.UseVisualStyleBackColor = true;
            this.cmdManagedBreaker.Click += new System.EventHandler(this.cmdManaged_Click);
            // 
            // cmdIcuTest
            // 
            this.cmdIcuTest.Location = new System.Drawing.Point(283, 399);
            this.cmdIcuTest.Name = "cmdIcuTest";
            this.cmdIcuTest.Size = new System.Drawing.Size(130, 35);
            this.cmdIcuTest.TabIndex = 5;
            this.cmdIcuTest.Text = "Icu";
            this.cmdIcuTest.UseVisualStyleBackColor = true;
            this.cmdIcuTest.Click += new System.EventHandler(this.cmdIcuTest_Click);
            // 
            // cmdPerformace1
            // 
            this.cmdPerformace1.Location = new System.Drawing.Point(431, 85);
            this.cmdPerformace1.Name = "cmdPerformace1";
            this.cmdPerformace1.Size = new System.Drawing.Size(130, 35);
            this.cmdPerformace1.TabIndex = 6;
            this.cmdPerformace1.Text = "Compare Perf";
            this.cmdPerformace1.UseVisualStyleBackColor = true;
            this.cmdPerformace1.Click += new System.EventHandler(this.cmdPerformace1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 507);
            this.Controls.Add(this.cmdPerformace1);
            this.Controls.Add(this.cmdIcuTest);
            this.Controls.Add(this.cmdManagedBreaker);
            this.Controls.Add(this.cmdReadDict);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.listBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button cmdReadDict;
        private System.Windows.Forms.Button cmdManagedBreaker;
        private System.Windows.Forms.Button cmdIcuTest;
        private System.Windows.Forms.Button cmdPerformace1;
    }
}

