namespace MathLayout
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.exampleBox = new System.Windows.Forms.ComboBox();
            this.nextExampleBtn = new System.Windows.Forms.Button();
            this.opsAutogenBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(29, 637);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(136, 52);
            this.button1.TabIndex = 0;
            this.button1.Text = "Test Layout1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(29, 22);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1076, 609);
            this.panel1.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(29, 695);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(136, 52);
            this.button2.TabIndex = 2;
            this.button2.Text = "Read Font file";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(214, 695);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(199, 52);
            this.button3.TabIndex = 3;
            this.button3.Text = "Render a simple math string";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(587, 683);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(136, 52);
            this.button4.TabIndex = 4;
            this.button4.Text = "TestReader";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(445, 683);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(136, 52);
            this.button5.TabIndex = 5;
            this.button5.Text = "Dom Autogen";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // exampleBox
            // 
            this.exampleBox.FormattingEnabled = true;
            this.exampleBox.Location = new System.Drawing.Point(587, 637);
            this.exampleBox.Name = "exampleBox";
            this.exampleBox.Size = new System.Drawing.Size(293, 21);
            this.exampleBox.TabIndex = 7;
            // 
            // nextExampleBtn
            // 
            this.nextExampleBtn.Location = new System.Drawing.Point(886, 637);
            this.nextExampleBtn.Name = "nextExampleBtn";
            this.nextExampleBtn.Size = new System.Drawing.Size(124, 41);
            this.nextExampleBtn.TabIndex = 8;
            this.nextExampleBtn.Text = "Next Example";
            this.nextExampleBtn.UseVisualStyleBackColor = true;
            this.nextExampleBtn.Click += new System.EventHandler(this.nextExampleBtn_Click);
            // 
            // opsAutogenBtn
            // 
            this.opsAutogenBtn.Location = new System.Drawing.Point(730, 683);
            this.opsAutogenBtn.Name = "opsAutogenBtn";
            this.opsAutogenBtn.Size = new System.Drawing.Size(129, 54);
            this.opsAutogenBtn.TabIndex = 9;
            this.opsAutogenBtn.Text = "Operator Table Autogen";
            this.opsAutogenBtn.UseVisualStyleBackColor = true;
            this.opsAutogenBtn.Click += new System.EventHandler(this.opsAutogenBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1197, 749);
            this.Controls.Add(this.opsAutogenBtn);
            this.Controls.Add(this.nextExampleBtn);
            this.Controls.Add(this.exampleBox);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.ComboBox exampleBox;
        private System.Windows.Forms.Button nextExampleBtn;
        private System.Windows.Forms.Button opsAutogenBtn;
    }
}

