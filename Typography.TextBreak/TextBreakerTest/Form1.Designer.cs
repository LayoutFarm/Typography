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
            this.cmdBidiTest = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.chkUseUnicodeRangeBreaker = new System.Windows.Forms.CheckBox();
            this.cmbSurrogatePairBreakOptions = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
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
            this.cmdReadDict.Size = new System.Drawing.Size(143, 35);
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
            this.cmdIcuTest.Size = new System.Drawing.Size(143, 35);
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
            // cmdBidiTest
            // 
            this.cmdBidiTest.Location = new System.Drawing.Point(652, 399);
            this.cmdBidiTest.Name = "cmdBidiTest";
            this.cmdBidiTest.Size = new System.Drawing.Size(130, 35);
            this.cmdBidiTest.TabIndex = 7;
            this.cmdBidiTest.Text = "Test Bidi";
            this.cmdBidiTest.UseVisualStyleBackColor = true;
            this.cmdBidiTest.Click += new System.EventHandler(this.cmdBidiTest_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(652, 440);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(130, 35);
            this.button1.TabIndex = 8;
            this.button1.Text = "Test Bidi";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // chkUseUnicodeRangeBreaker
            // 
            this.chkUseUnicodeRangeBreaker.AutoSize = true;
            this.chkUseUnicodeRangeBreaker.Location = new System.Drawing.Point(13, 21);
            this.chkUseUnicodeRangeBreaker.Name = "chkUseUnicodeRangeBreaker";
            this.chkUseUnicodeRangeBreaker.Size = new System.Drawing.Size(157, 17);
            this.chkUseUnicodeRangeBreaker.TabIndex = 9;
            this.chkUseUnicodeRangeBreaker.Text = "Use UnicodeRangeBreaker";
            this.chkUseUnicodeRangeBreaker.UseVisualStyleBackColor = true;
            // 
            // cmbSurrogatePairBreakOptions
            // 
            this.cmbSurrogatePairBreakOptions.FormattingEnabled = true;
            this.cmbSurrogatePairBreakOptions.Location = new System.Drawing.Point(178, 44);
            this.cmbSurrogatePairBreakOptions.Name = "cmbSurrogatePairBreakOptions";
            this.cmbSurrogatePairBreakOptions.Size = new System.Drawing.Size(248, 21);
            this.cmbSurrogatePairBreakOptions.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(178, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "SurrogatePairBreakOptions";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 507);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbSurrogatePairBreakOptions);
            this.Controls.Add(this.chkUseUnicodeRangeBreaker);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cmdBidiTest);
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
        private System.Windows.Forms.Button cmdBidiTest;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox chkUseUnicodeRangeBreaker;
        private System.Windows.Forms.ComboBox cmbSurrogatePairBreakOptions;
        private System.Windows.Forms.Label label1;
    }
}

