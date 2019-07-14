namespace Test_WinForm_TessGlyph
{
    partial class FormTess
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
            this.cmdDrawGlyph = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkFlipY = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.txtDivCurveRecursiveLimit = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDivAngleTolerenceEpsilon = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtIncrementalTessStep = new System.Windows.Forms.TextBox();
            this.rdoSubdivCureveFlattener = new System.Windows.Forms.RadioButton();
            this.rdoSimpleIncCurveFlattener = new System.Windows.Forms.RadioButton();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.rdoTessPoly2Tri = new System.Windows.Forms.RadioButton();
            this.rdoTessSGI = new System.Windows.Forms.RadioButton();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdDrawGlyph
            // 
            this.cmdDrawGlyph.Location = new System.Drawing.Point(539, 13);
            this.cmdDrawGlyph.Name = "cmdDrawGlyph";
            this.cmdDrawGlyph.Size = new System.Drawing.Size(144, 39);
            this.cmdDrawGlyph.TabIndex = 0;
            this.cmdDrawGlyph.Text = "DrawGlyph";
            this.cmdDrawGlyph.UseVisualStyleBackColor = true;
            this.cmdDrawGlyph.Click += new System.EventHandler(this.cmdDrawGlyph_Click);
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(7, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(526, 447);
            this.panel1.TabIndex = 1;
            // 
            // chkFlipY
            // 
            this.chkFlipY.AutoSize = true;
            this.chkFlipY.Location = new System.Drawing.Point(540, 59);
            this.chkFlipY.Name = "chkFlipY";
            this.chkFlipY.Size = new System.Drawing.Size(49, 17);
            this.chkFlipY.TabIndex = 2;
            this.chkFlipY.Text = "FlipY";
            this.chkFlipY.UseVisualStyleBackColor = true;
            this.chkFlipY.CheckedChanged += new System.EventHandler(this.chkInvert_CheckedChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(537, 112);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(537, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Single Char:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.txtDivCurveRecursiveLimit);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.txtDivAngleTolerenceEpsilon);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.txtIncrementalTessStep);
            this.panel2.Controls.Add(this.rdoSubdivCureveFlattener);
            this.panel2.Controls.Add(this.rdoSimpleIncCurveFlattener);
            this.panel2.Location = new System.Drawing.Point(543, 138);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(213, 201);
            this.panel2.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 136);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "RecursiveLimit";
            // 
            // txtDivCurveRecursiveLimit
            // 
            this.txtDivCurveRecursiveLimit.Location = new System.Drawing.Point(137, 136);
            this.txtDivCurveRecursiveLimit.Name = "txtDivCurveRecursiveLimit";
            this.txtDivCurveRecursiveLimit.Size = new System.Drawing.Size(73, 20);
            this.txtDivCurveRecursiveLimit.TabIndex = 20;
            this.txtDivCurveRecursiveLimit.Text = "32";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(124, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "AngleTolerence (degree)";
            // 
            // txtDivAngleTolerenceEpsilon
            // 
            this.txtDivAngleTolerenceEpsilon.Location = new System.Drawing.Point(137, 107);
            this.txtDivAngleTolerenceEpsilon.Name = "txtDivAngleTolerenceEpsilon";
            this.txtDivAngleTolerenceEpsilon.Size = new System.Drawing.Size(73, 20);
            this.txtDivAngleTolerenceEpsilon.TabIndex = 18;
            this.txtDivAngleTolerenceEpsilon.Text = "0.01";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Curve Flattener ...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "steps";
            // 
            // txtIncrementalTessStep
            // 
            this.txtIncrementalTessStep.Location = new System.Drawing.Point(45, 49);
            this.txtIncrementalTessStep.Name = "txtIncrementalTessStep";
            this.txtIncrementalTessStep.Size = new System.Drawing.Size(91, 20);
            this.txtIncrementalTessStep.TabIndex = 15;
            this.txtIncrementalTessStep.Text = "7";
            // 
            // rdoSubdivCureveFlattener
            // 
            this.rdoSubdivCureveFlattener.AutoSize = true;
            this.rdoSubdivCureveFlattener.Location = new System.Drawing.Point(11, 84);
            this.rdoSubdivCureveFlattener.Name = "rdoSubdivCureveFlattener";
            this.rdoSubdivCureveFlattener.Size = new System.Drawing.Size(93, 17);
            this.rdoSubdivCureveFlattener.TabIndex = 14;
            this.rdoSubdivCureveFlattener.TabStop = true;
            this.rdoSubdivCureveFlattener.Text = "2. SubDivision";
            this.rdoSubdivCureveFlattener.UseVisualStyleBackColor = true;
            // 
            // rdoSimpleIncCurveFlattener
            // 
            this.rdoSimpleIncCurveFlattener.AutoSize = true;
            this.rdoSimpleIncCurveFlattener.Location = new System.Drawing.Point(10, 26);
            this.rdoSimpleIncCurveFlattener.Name = "rdoSimpleIncCurveFlattener";
            this.rdoSimpleIncCurveFlattener.Size = new System.Drawing.Size(136, 17);
            this.rdoSimpleIncCurveFlattener.TabIndex = 13;
            this.rdoSimpleIncCurveFlattener.TabStop = true;
            this.rdoSimpleIncCurveFlattener.Text = "1. Incremental Flattener";
            this.rdoSimpleIncCurveFlattener.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.rdoTessPoly2Tri);
            this.panel3.Controls.Add(this.rdoTessSGI);
            this.panel3.Location = new System.Drawing.Point(543, 359);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(213, 100);
            this.panel3.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(69, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Tess Method";
            // 
            // rdoTessPoly2Tri
            // 
            this.rdoTessPoly2Tri.AutoSize = true;
            this.rdoTessPoly2Tri.Location = new System.Drawing.Point(11, 50);
            this.rdoTessPoly2Tri.Name = "rdoTessPoly2Tri";
            this.rdoTessPoly2Tri.Size = new System.Drawing.Size(145, 17);
            this.rdoTessPoly2Tri.TabIndex = 23;
            this.rdoTessPoly2Tri.Text = "2. Poly2Tri (Triangulation)";
            this.rdoTessPoly2Tri.UseVisualStyleBackColor = true;
            // 
            // rdoTessSGI
            // 
            this.rdoTessSGI.AutoSize = true;
            this.rdoTessSGI.Checked = true;
            this.rdoTessSGI.Location = new System.Drawing.Point(11, 27);
            this.rdoTessSGI.Name = "rdoTessSGI";
            this.rdoTessSGI.Size = new System.Drawing.Size(81, 17);
            this.rdoTessSGI.TabIndex = 22;
            this.rdoTessSGI.TabStop = true;
            this.rdoTessSGI.Text = "1. SGI Tess";
            this.rdoTessSGI.UseVisualStyleBackColor = true;
            // 
            // FormTess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 472);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.chkFlipY);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cmdDrawGlyph);
            this.Name = "FormTess";
            this.Text = "FormTess";
            this.Load += new System.EventHandler(this.FormTess_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdDrawGlyph;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox chkFlipY;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtDivCurveRecursiveLimit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDivAngleTolerenceEpsilon;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtIncrementalTessStep;
        private System.Windows.Forms.RadioButton rdoSubdivCureveFlattener;
        private System.Windows.Forms.RadioButton rdoSimpleIncCurveFlattener;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.RadioButton rdoTessPoly2Tri;
        private System.Windows.Forms.RadioButton rdoTessSGI;
        private System.Windows.Forms.Label label6;
    }
}

