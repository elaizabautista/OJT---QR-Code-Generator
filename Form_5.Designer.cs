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
            label4 = new Label();
            groupBox1 = new GroupBox();
            Location5 = new TextBox();
            label2 = new Label();
            txtBinLocation6 = new TextBox();
            btnGenerate = new Button();
            label10 = new Label();
            btnClear = new Button();
            txtBinLocation4 = new TextBox();
            btnPrint = new Button();
            label11 = new Label();
            label6 = new Label();
            label12 = new Label();
            label7 = new Label();
            textBox1 = new TextBox();
            label3 = new Label();
            label9 = new Label();
            txtCustomWidth = new TextBox();
            label8 = new Label();
            txtBinLocation1 = new TextBox();
            txtCustomHeight = new TextBox();
            cmbBatch = new ComboBox();
            txtBinLocation2 = new TextBox();
            btnConvertToPdf = new Button();
            pnlPreview = new Panel();
            label1 = new Label();
            panel2 = new Panel();
            label13 = new Label();
            panel3 = new Panel();
            label14 = new Label();
            panel1.SuspendLayout();
            groupBox1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // PrintAllButt
            // 
            PrintAllButt.BackColor = Color.LightSkyBlue;
            PrintAllButt.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            PrintAllButt.Location = new Point(186, 380);
            PrintAllButt.Name = "PrintAllButt";
            PrintAllButt.Size = new Size(161, 39);
            PrintAllButt.TabIndex = 21;
            PrintAllButt.Text = "Print All";
            PrintAllButt.UseVisualStyleBackColor = false;
            PrintAllButt.Click += PrintAllButt_Click;
            // 
            // panel1
            // 
            panel1.BackColor = Color.LightSteelBlue;
            panel1.Controls.Add(label4);
            panel1.Controls.Add(groupBox1);
            panel1.Controls.Add(btnConvertToPdf);
            panel1.Controls.Add(pnlPreview);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(58, 176);
            panel1.Name = "panel1";
            panel1.Size = new Size(1043, 562);
            panel1.TabIndex = 2;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label4.Location = new Point(510, 101);
            label4.Name = "label4";
            label4.Size = new Size(109, 21);
            label4.TabIndex = 5;
            label4.Text = "Print Preview:";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(Location5);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(txtBinLocation6);
            groupBox1.Controls.Add(btnGenerate);
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(btnClear);
            groupBox1.Controls.Add(txtBinLocation4);
            groupBox1.Controls.Add(btnPrint);
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(label12);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(txtCustomWidth);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(txtBinLocation1);
            groupBox1.Controls.Add(PrintAllButt);
            groupBox1.Controls.Add(txtCustomHeight);
            groupBox1.Controls.Add(cmbBatch);
            groupBox1.Controls.Add(txtBinLocation2);
            groupBox1.Location = new Point(65, 101);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(364, 445);
            groupBox1.TabIndex = 34;
            groupBox1.TabStop = false;
            // 
            // Location5
            // 
            Location5.Location = new Point(186, 110);
            Location5.Name = "Location5";
            Location5.Size = new Size(138, 23);
            Location5.TabIndex = 33;
            Location5.TextChanged += Location5_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label2.Location = new Point(19, 19);
            label2.Name = "label2";
            label2.Size = new Size(114, 21);
            label2.TabIndex = 19;
            label2.Text = "Bin Location 1:";
            // 
            // txtBinLocation6
            // 
            txtBinLocation6.Location = new Point(186, 176);
            txtBinLocation6.Name = "txtBinLocation6";
            txtBinLocation6.Size = new Size(138, 23);
            txtBinLocation6.TabIndex = 32;
            // 
            // btnGenerate
            // 
            btnGenerate.BackColor = Color.LimeGreen;
            btnGenerate.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnGenerate.Location = new Point(19, 326);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(161, 48);
            btnGenerate.TabIndex = 7;
            btnGenerate.Text = "Generate";
            btnGenerate.UseVisualStyleBackColor = false;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label10.Location = new Point(186, 152);
            label10.Name = "label10";
            label10.Size = new Size(117, 21);
            label10.TabIndex = 31;
            label10.Text = "Bin Location 6:";
            // 
            // btnClear
            // 
            btnClear.BackColor = Color.IndianRed;
            btnClear.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnClear.Location = new Point(186, 326);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(161, 48);
            btnClear.TabIndex = 8;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = false;
            btnClear.Click += btnClear_Click;
            // 
            // txtBinLocation4
            // 
            txtBinLocation4.Location = new Point(186, 48);
            txtBinLocation4.Name = "txtBinLocation4";
            txtBinLocation4.Size = new Size(138, 23);
            txtBinLocation4.TabIndex = 29;
            // 
            // btnPrint
            // 
            btnPrint.BackColor = Color.LightSkyBlue;
            btnPrint.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPrint.Location = new Point(19, 380);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(161, 39);
            btnPrint.TabIndex = 9;
            btnPrint.Text = "Print";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += btnPrint_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label11.Location = new Point(186, 86);
            label11.Name = "label11";
            label11.Size = new Size(117, 21);
            label11.TabIndex = 28;
            label11.Text = "Bin Location 5:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(22, 283);
            label6.Name = "label6";
            label6.Size = new Size(42, 15);
            label6.TabIndex = 13;
            label6.Text = "Width:";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label12.Location = new Point(186, 24);
            label12.Name = "label12";
            label12.Size = new Size(117, 21);
            label12.TabIndex = 27;
            label12.Text = "Bin Location 4:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(151, 283);
            label7.Name = "label7";
            label7.Size = new Size(46, 15);
            label7.TabIndex = 14;
            label7.Text = "Height:";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(22, 171);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(138, 23);
            textBox1.TabIndex = 26;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(19, 81);
            label3.Name = "label3";
            label3.Size = new Size(117, 21);
            label3.TabIndex = 20;
            label3.Text = "Bin Location 2:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label9.Location = new Point(19, 147);
            label9.Name = "label9";
            label9.Size = new Size(117, 21);
            label9.TabIndex = 25;
            label9.Text = "Bin Location 3:";
            // 
            // txtCustomWidth
            // 
            txtCustomWidth.Location = new Point(70, 278);
            txtCustomWidth.Name = "txtCustomWidth";
            txtCustomWidth.Size = new Size(51, 23);
            txtCustomWidth.TabIndex = 15;
            txtCustomWidth.TextChanged += txtCustomWidth_TextChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label8.Location = new Point(22, 209);
            label8.Name = "label8";
            label8.Size = new Size(55, 21);
            label8.TabIndex = 24;
            label8.Text = "Batch:";
            // 
            // txtBinLocation1
            // 
            txtBinLocation1.Location = new Point(22, 43);
            txtBinLocation1.Name = "txtBinLocation1";
            txtBinLocation1.Size = new Size(138, 23);
            txtBinLocation1.TabIndex = 21;
            txtBinLocation1.TextChanged += txtBinLocation1_TextChanged;
            // 
            // txtCustomHeight
            // 
            txtCustomHeight.Location = new Point(203, 278);
            txtCustomHeight.Name = "txtCustomHeight";
            txtCustomHeight.Size = new Size(51, 23);
            txtCustomHeight.TabIndex = 16;
            txtCustomHeight.TextChanged += txtCustomHeight_TextChanged;
            // 
            // cmbBatch
            // 
            cmbBatch.FormattingEnabled = true;
            cmbBatch.Location = new Point(25, 233);
            cmbBatch.Name = "cmbBatch";
            cmbBatch.Size = new Size(299, 23);
            cmbBatch.TabIndex = 23;
            cmbBatch.Text = " __Select__";
            cmbBatch.SelectedIndexChanged += cmbBatch_SelectedIndexChanged;
            // 
            // txtBinLocation2
            // 
            txtBinLocation2.Location = new Point(22, 105);
            txtBinLocation2.Name = "txtBinLocation2";
            txtBinLocation2.Size = new Size(138, 23);
            txtBinLocation2.TabIndex = 22;
            txtBinLocation2.TextChanged += txtBinLocation2_TextChanged;
            // 
            // btnConvertToPdf
            // 
            btnConvertToPdf.BackColor = Color.AliceBlue;
            btnConvertToPdf.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnConvertToPdf.Location = new Point(660, 457);
            btnConvertToPdf.Name = "btnConvertToPdf";
            btnConvertToPdf.Size = new Size(165, 39);
            btnConvertToPdf.TabIndex = 10;
            btnConvertToPdf.Text = "Convert to PDF";
            btnConvertToPdf.UseVisualStyleBackColor = false;
            btnConvertToPdf.Click += btnConvertToPdf_Click;
            // 
            // pnlPreview
            // 
            pnlPreview.Location = new Point(510, 144);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Size = new Size(466, 292);
            pnlPreview.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(65, 35);
            label1.Name = "label1";
            label1.Size = new Size(265, 37);
            label1.TabIndex = 0;
            label1.Text = "QR Code Generator";
            // 
            // panel2
            // 
            panel2.BackColor = Color.RoyalBlue;
            panel2.Controls.Add(label13);
            panel2.Location = new Point(2, 77);
            panel2.Name = "panel2";
            panel2.Size = new Size(1150, 60);
            panel2.TabIndex = 34;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label13.ForeColor = SystemColors.ButtonHighlight;
            label13.Location = new Point(18, 14);
            label13.Name = "label13";
            label13.Size = new Size(88, 25);
            label13.TabIndex = 19;
            label13.Text = "Floor Bin";
            // 
            // panel3
            // 
            panel3.BackColor = Color.MediumBlue;
            panel3.Controls.Add(label14);
            panel3.Location = new Point(2, 1);
            panel3.Name = "panel3";
            panel3.Size = new Size(1150, 87);
            panel3.TabIndex = 33;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label14.ForeColor = SystemColors.ButtonHighlight;
            label14.Location = new Point(16, 25);
            label14.Name = "label14";
            label14.Size = new Size(264, 30);
            label14.TabIndex = 20;
            label14.Text = "Panasonic | QR Generator";
            // 
            // Form_5
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.LightSlateGray;
            ClientSize = new Size(1150, 750);
            Controls.Add(panel2);
            Controls.Add(panel3);
            Controls.Add(panel1);
            Name = "Form_5";
            Text = "Form_5";
            Load += Form_5_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public Button PrintAllButt;
        public Panel panel1;
        public TextBox txtCustomHeight;
        public TextBox txtCustomWidth;
        public Label label7;
        public Label label6;
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
        public TextBox txtBinLocation4;
        public Label label11;
        public Label label12;
        public TextBox Location5;
        private Panel panel2;
        private Label label13;
        private Panel panel3;
        private Label label14;
        private GroupBox groupBox1;
    }
}