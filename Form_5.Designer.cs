namespace OJT___QR_Code_Generator
{
    partial class Form_5
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
            PrintAllButt = new Button();
            panel1 = new Panel();
            txtBinLocation6 = new TextBox();
            label10 = new Label();
            txtBinLocation4 = new TextBox();
            label11 = new Label();
            label12 = new Label();
            textBox1 = new TextBox();
            label9 = new Label();
            label8 = new Label();
            cmbBatch = new ComboBox();
            txtBinLocation2 = new TextBox();
            txtCustomHeight = new TextBox();
            txtBinLocation1 = new TextBox();
            txtCustomWidth = new TextBox();
            label3 = new Label();
            label7 = new Label();
            label2 = new Label();
            label6 = new Label();
            label5 = new Label();
            btnConvertToPdf = new Button();
            btnPrint = new Button();
            btnClear = new Button();
            btnGenerate = new Button();
            pnlPreview = new Panel();
            label4 = new Label();
            label1 = new Label();
            textBox2 = new TextBox();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // PrintAllButt
            // 
            PrintAllButt.BackColor = Color.LightSkyBlue;
            PrintAllButt.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            PrintAllButt.Location = new Point(213, 431);
            PrintAllButt.Name = "PrintAllButt";
            PrintAllButt.Size = new Size(161, 39);
            PrintAllButt.TabIndex = 21;
            PrintAllButt.Text = "Print All";
            PrintAllButt.UseVisualStyleBackColor = false;
            PrintAllButt.Click += PrintAllButt_Click;
            // 
            // panel1
            // 
            panel1.BackColor = Color.RosyBrown;
            panel1.Controls.Add(textBox2);
            panel1.Controls.Add(txtBinLocation6);
            panel1.Controls.Add(label10);
            panel1.Controls.Add(txtBinLocation4);
            panel1.Controls.Add(label11);
            panel1.Controls.Add(label12);
            panel1.Controls.Add(textBox1);
            panel1.Controls.Add(label9);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(PrintAllButt);
            panel1.Controls.Add(cmbBatch);
            panel1.Controls.Add(txtBinLocation2);
            panel1.Controls.Add(txtCustomHeight);
            panel1.Controls.Add(txtBinLocation1);
            panel1.Controls.Add(txtCustomWidth);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(btnConvertToPdf);
            panel1.Controls.Add(btnPrint);
            panel1.Controls.Add(btnClear);
            panel1.Controls.Add(btnGenerate);
            panel1.Controls.Add(pnlPreview);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(91, 13);
            panel1.Name = "panel1";
            panel1.Size = new Size(1043, 514);
            panel1.TabIndex = 2;
            // 
            // txtBinLocation6
            // 
            txtBinLocation6.Location = new Point(239, 248);
            txtBinLocation6.Name = "txtBinLocation6";
            txtBinLocation6.Size = new Size(158, 23);
            txtBinLocation6.TabIndex = 32;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label10.Location = new Point(236, 224);
            label10.Name = "label10";
            label10.Size = new Size(117, 21);
            label10.TabIndex = 31;
            label10.Text = "Bin Location 6:";
            // 
            // txtBinLocation4
            // 
            txtBinLocation4.Location = new Point(239, 120);
            txtBinLocation4.Name = "txtBinLocation4";
            txtBinLocation4.Size = new Size(158, 23);
            txtBinLocation4.TabIndex = 29;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label11.Location = new Point(236, 158);
            label11.Name = "label11";
            label11.Size = new Size(117, 21);
            label11.TabIndex = 28;
            label11.Text = "Bin Location 5:";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label12.Location = new Point(236, 96);
            label12.Name = "label12";
            label12.Size = new Size(117, 21);
            label12.TabIndex = 27;
            label12.Text = "Bin Location 4:";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(49, 248);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(158, 23);
            textBox1.TabIndex = 26;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label9.Location = new Point(46, 224);
            label9.Name = "label9";
            label9.Size = new Size(117, 21);
            label9.TabIndex = 25;
            label9.Text = "Bin Location 3:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label8.Location = new Point(49, 298);
            label8.Name = "label8";
            label8.Size = new Size(55, 21);
            label8.TabIndex = 24;
            label8.Text = "Batch:";
            // 
            // cmbBatch
            // 
            cmbBatch.FormattingEnabled = true;
            cmbBatch.Location = new Point(52, 322);
            cmbBatch.Name = "cmbBatch";
            cmbBatch.Size = new Size(298, 23);
            cmbBatch.TabIndex = 23;
            cmbBatch.SelectedIndexChanged += cmbBatch_SelectedIndexChanged;
            // 
            // txtBinLocation2
            // 
            txtBinLocation2.Location = new Point(49, 182);
            txtBinLocation2.Name = "txtBinLocation2";
            txtBinLocation2.Size = new Size(158, 23);
            txtBinLocation2.TabIndex = 22;
            txtBinLocation2.TextChanged += txtBinLocation2_TextChanged;
            // 
            // txtCustomHeight
            // 
            txtCustomHeight.Location = new Point(941, 99);
            txtCustomHeight.Name = "txtCustomHeight";
            txtCustomHeight.Size = new Size(51, 23);
            txtCustomHeight.TabIndex = 16;
            txtCustomHeight.TextChanged += txtCustomHeight_TextChanged;
            // 
            // txtBinLocation1
            // 
            txtBinLocation1.Location = new Point(49, 120);
            txtBinLocation1.Name = "txtBinLocation1";
            txtBinLocation1.Size = new Size(158, 23);
            txtBinLocation1.TabIndex = 21;
            txtBinLocation1.TextChanged += txtBinLocation1_TextChanged;
            // 
            // txtCustomWidth
            // 
            txtCustomWidth.Location = new Point(941, 68);
            txtCustomWidth.Name = "txtCustomWidth";
            txtCustomWidth.Size = new Size(51, 23);
            txtCustomWidth.TabIndex = 15;
            txtCustomWidth.TextChanged += txtCustomWidth_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(46, 158);
            label3.Name = "label3";
            label3.Size = new Size(117, 21);
            label3.TabIndex = 20;
            label3.Text = "Bin Location 2:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(889, 102);
            label7.Name = "label7";
            label7.Size = new Size(46, 15);
            label7.TabIndex = 14;
            label7.Text = "Height:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label2.Location = new Point(46, 96);
            label2.Name = "label2";
            label2.Size = new Size(114, 21);
            label2.TabIndex = 19;
            label2.Text = "Bin Location 1:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(893, 73);
            label6.Name = "label6";
            label6.Size = new Size(42, 15);
            label6.TabIndex = 13;
            label6.Text = "Width:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label5.Location = new Point(884, 35);
            label5.Name = "label5";
            label5.Size = new Size(89, 21);
            label5.TabIndex = 12;
            label5.Text = "Paper Size:";
            // 
            // btnConvertToPdf
            // 
            btnConvertToPdf.BackColor = Color.AliceBlue;
            btnConvertToPdf.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnConvertToPdf.Location = new Point(594, 455);
            btnConvertToPdf.Name = "btnConvertToPdf";
            btnConvertToPdf.Size = new Size(165, 39);
            btnConvertToPdf.TabIndex = 10;
            btnConvertToPdf.Text = "Convert to PDF";
            btnConvertToPdf.UseVisualStyleBackColor = false;
            btnConvertToPdf.Click += btnConvertToPdf_Click;
            // 
            // btnPrint
            // 
            btnPrint.BackColor = Color.LightSkyBlue;
            btnPrint.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPrint.Location = new Point(46, 431);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(161, 39);
            btnPrint.TabIndex = 9;
            btnPrint.Text = "Print";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += btnPrint_Click;
            // 
            // btnClear
            // 
            btnClear.BackColor = Color.IndianRed;
            btnClear.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnClear.Location = new Point(213, 377);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(161, 48);
            btnClear.TabIndex = 8;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = false;
            btnClear.Click += btnClear_Click;
            // 
            // btnGenerate
            // 
            btnGenerate.BackColor = Color.LimeGreen;
            btnGenerate.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnGenerate.Location = new Point(46, 377);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(161, 48);
            btnGenerate.TabIndex = 7;
            btnGenerate.Text = "Generate";
            btnGenerate.UseVisualStyleBackColor = false;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // pnlPreview
            // 
            pnlPreview.Location = new Point(510, 144);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Size = new Size(328, 292);
            pnlPreview.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label4.Location = new Point(510, 120);
            label4.Name = "label4";
            label4.Size = new Size(109, 21);
            label4.TabIndex = 5;
            label4.Text = "Print Preview:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(353, 25);
            label1.Name = "label1";
            label1.Size = new Size(358, 50);
            label1.TabIndex = 0;
            label1.Text = "QR Code Generator";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(239, 182);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(158, 23);
            textBox2.TabIndex = 33;
            // 
            // Form_5
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.LightSlateGray;
            ClientSize = new Size(1224, 561);
            Controls.Add(panel1);
            Name = "Form_5";
            Text = "Form_5";
            Load += Form_5_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public Button PrintAllButt;
        public Panel panel1;
        public TextBox txtCustomHeight;
        public TextBox txtCustomWidth;
        public Label label7;
        public Label label6;
        public Label label5;
        public Button btnConvertToPdf;
        public Button btnPrint;
        public Button btnClear;
        public Button btnGenerate;
        public Panel pnlPreview;
        public Label label4;
        public TextBox textBox1;
        public Label label9;
        public Label label8;
        private ComboBox cmbBatch;
        public TextBox txtBinLocation2;
        public TextBox txtBinLocation1;
        public Label label3;
        public Label label2;
        public Label label1;
        public TextBox txtBinLocation6;
        public Label label10;
        public TextBox textBox3;
        public TextBox txtBinLocation4;
        public Label label11;
        public Label label12;
        public TextBox textBox2;
    }
}