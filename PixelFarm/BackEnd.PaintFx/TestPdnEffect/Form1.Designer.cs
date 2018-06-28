namespace TestPdnEffect
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.trkZoom = new System.Windows.Forms.TrackBar();
            this.chkKeepBackground = new System.Windows.Forms.CheckBox();
            this.chkTile = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.rollControl1 = new PaintDotNet.Effects.RollControl();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(28, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(130, 45);
            this.button1.TabIndex = 0;
            this.button1.Text = "Render!";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(173, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(375, 522);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(554, 12);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(378, 522);
            this.pictureBox2.TabIndex = 2;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.trkZoom);
            this.groupBox1.Controls.Add(this.chkKeepBackground);
            this.groupBox1.Controls.Add(this.chkTile);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.rollControl1);
            this.groupBox1.Location = new System.Drawing.Point(28, 570);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(979, 210);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "RotateAndZoom";
            // 
            // trkZoom
            // 
            this.trkZoom.Location = new System.Drawing.Point(487, 19);
            this.trkZoom.Maximum = 1024;
            this.trkZoom.Name = "trkZoom";
            this.trkZoom.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trkZoom.Size = new System.Drawing.Size(45, 186);
            this.trkZoom.TabIndex = 14;
            this.trkZoom.TickFrequency = 64;
            this.trkZoom.Value = 512;
            // 
            // chkKeepBackground
            // 
            this.chkKeepBackground.AutoSize = true;
            this.chkKeepBackground.Location = new System.Drawing.Point(207, 42);
            this.chkKeepBackground.Name = "chkKeepBackground";
            this.chkKeepBackground.Size = new System.Drawing.Size(112, 17);
            this.chkKeepBackground.TabIndex = 13;
            this.chkKeepBackground.Text = "Keep Background";
            this.chkKeepBackground.UseVisualStyleBackColor = true;
            // 
            // chkTile
            // 
            this.chkTile.AutoSize = true;
            this.chkTile.Location = new System.Drawing.Point(207, 19);
            this.chkTile.Name = "chkTile";
            this.chkTile.Size = new System.Drawing.Size(43, 17);
            this.chkTile.TabIndex = 12;
            this.chkTile.Text = "Tile";
            this.chkTile.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(335, 19);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(130, 45);
            this.button2.TabIndex = 11;
            this.button2.Text = "Render!";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(207, 69);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(258, 136);
            this.textBox1.TabIndex = 10;
            // 
            // rollControl1
            // 
            this.rollControl1.Angle = 0D;
            this.rollControl1.Location = new System.Drawing.Point(33, 19);
            this.rollControl1.Name = "rollControl1";
            this.rollControl1.RollAmount = 0D;
            this.rollControl1.RollDirection = 0D;
            this.rollControl1.Size = new System.Drawing.Size(168, 144);
            this.rollControl1.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1019, 879);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkZoom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TrackBar trkZoom;
        private System.Windows.Forms.CheckBox chkKeepBackground;
        private System.Windows.Forms.CheckBox chkTile;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private PaintDotNet.Effects.RollControl rollControl1;
    }
}

