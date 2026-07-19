namespace OJT___QR_Code_Generator
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            groupBox1 = new GroupBox();
            label2 = new Label();
            label3 = new Label();
            btnPrintAll = new Button();
            txtBinLocation1 = new TextBox();
            label8 = new Label();
            txtBinLocation2 = new TextBox();
            cmbBatch = new ComboBox();
            btnGenerate = new Button();
            txtCustomHeight = new TextBox();
            btnClear = new Button();
            txtCustomWidth = new TextBox();
            btnPrint = new Button();
            label7 = new Label();
            label5 = new Label();
            label6 = new Label();
            btnConvertToPdf = new Button();
            pnlPreview = new Panel();
            label4 = new Label();
            label1 = new Label();
            panel2 = new Panel();
            label9 = new Label();
            panel3 = new Panel();
            label10 = new Label();
            panel4 = new Panel();
            Uploadbutt = new Button();
            panel1.SuspendLayout();
            groupBox1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            panel4.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.LightSteelBlue;
            panel1.Controls.Add(Uploadbutt);
            panel1.Controls.Add(groupBox1);
            panel1.Controls.Add(btnConvertToPdf);
            panel1.Controls.Add(pnlPreview);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(48, 162);
            panel1.Name = "panel1";
            panel1.Size = new Size(1043, 516);
            panel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(btnPrintAll);
            groupBox1.Controls.Add(txtBinLocation1);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(txtBinLocation2);
            groupBox1.Controls.Add(cmbBatch);
            groupBox1.Controls.Add(btnGenerate);
            groupBox1.Controls.Add(txtCustomHeight);
            groupBox1.Controls.Add(btnClear);
            groupBox1.Controls.Add(txtCustomWidth);
            groupBox1.Controls.Add(btnPrint);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label6);
            groupBox1.Location = new Point(48, 77);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(387, 419);
            groupBox1.TabIndex = 21;
            groupBox1.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label2.Location = new Point(33, 19);
            label2.Name = "label2";
            label2.Size = new Size(114, 21);
            label2.TabIndex = 1;
            label2.Text = "Bin Location 1:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(33, 81);
            label3.Name = "label3";
            label3.Size = new Size(117, 21);
            label3.TabIndex = 2;
            label3.Text = "Bin Location 2:";
            // 
            // btnPrintAll
            // 
            btnPrintAll.BackColor = Color.LightSkyBlue;
            btnPrintAll.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPrintAll.Location = new Point(36, 357);
            btnPrintAll.Name = "btnPrintAll";
            btnPrintAll.Size = new Size(151, 48);
            btnPrintAll.TabIndex = 19;
            btnPrintAll.Text = "Print All";
            btnPrintAll.UseVisualStyleBackColor = false;
            btnPrintAll.Click += btnPrintAll_Click;
            // 
            // txtBinLocation1
            // 
            txtBinLocation1.Location = new Point(36, 43);
            txtBinLocation1.Name = "txtBinLocation1";
            txtBinLocation1.Size = new Size(298, 23);
            txtBinLocation1.TabIndex = 3;
            txtBinLocation1.TextChanged += txtBinLocation1_TextChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label8.Location = new Point(33, 137);
            label8.Name = "label8";
            label8.Size = new Size(55, 21);
            label8.TabIndex = 18;
            label8.Text = "Batch:";
            // 
            // txtBinLocation2
            // 
            txtBinLocation2.Location = new Point(36, 105);
            txtBinLocation2.Name = "txtBinLocation2";
            txtBinLocation2.Size = new Size(298, 23);
            txtBinLocation2.TabIndex = 4;
            // 
            // cmbBatch
            // 
            cmbBatch.FormattingEnabled = true;
            cmbBatch.Location = new Point(36, 161);
            cmbBatch.Name = "cmbBatch";
            cmbBatch.Size = new Size(298, 23);
            cmbBatch.TabIndex = 17;
            cmbBatch.SelectedIndexChanged += cmbBatch_SelectedIndexChanged;
            // 
            // btnGenerate
            // 
            btnGenerate.BackColor = Color.MediumBlue;
            btnGenerate.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnGenerate.ForeColor = SystemColors.ControlLightLight;
            btnGenerate.Location = new Point(36, 303);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(151, 48);
            btnGenerate.TabIndex = 7;
            btnGenerate.Text = "Generate";
            btnGenerate.UseVisualStyleBackColor = false;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // txtCustomHeight
            // 
            txtCustomHeight.Location = new Point(249, 249);
            txtCustomHeight.Name = "txtCustomHeight";
            txtCustomHeight.Size = new Size(50, 23);
            txtCustomHeight.TabIndex = 16;
            // 
            // btnClear
            // 
            btnClear.BackColor = Color.Brown;
            btnClear.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnClear.ForeColor = SystemColors.ControlLightLight;
            btnClear.Location = new Point(203, 303);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(151, 48);
            btnClear.TabIndex = 8;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = false;
            btnClear.Click += btnClear_Click;
            // 
            // txtCustomWidth
            // 
            txtCustomWidth.Location = new Point(128, 249);
            txtCustomWidth.Name = "txtCustomWidth";
            txtCustomWidth.Size = new Size(51, 23);
            txtCustomWidth.TabIndex = 15;
            // 
            // btnPrint
            // 
            btnPrint.BackColor = Color.LightSkyBlue;
            btnPrint.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPrint.Location = new Point(203, 357);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(151, 48);
            btnPrint.TabIndex = 9;
            btnPrint.Text = "Print";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += btnPrint_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(197, 254);
            label7.Name = "label7";
            label7.Size = new Size(46, 15);
            label7.TabIndex = 14;
            label7.Text = "Height:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label5.Location = new Point(33, 207);
            label5.Name = "label5";
            label5.Size = new Size(89, 21);
            label5.TabIndex = 12;
            label5.Text = "Paper Size:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(80, 254);
            label6.Name = "label6";
            label6.Size = new Size(42, 15);
            label6.TabIndex = 13;
            label6.Text = "Width:";
            // 
            // btnConvertToPdf
            // 
            btnConvertToPdf.BackColor = Color.AliceBlue;
            btnConvertToPdf.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnConvertToPdf.Location = new Point(658, 457);
            btnConvertToPdf.Name = "btnConvertToPdf";
            btnConvertToPdf.Size = new Size(165, 39);
            btnConvertToPdf.TabIndex = 10;
            btnConvertToPdf.Text = "Convert to PDF";
            btnConvertToPdf.UseVisualStyleBackColor = false;
            btnConvertToPdf.Click += btnConvertToPdf_Click;
            // 
            // pnlPreview
            // 
            pnlPreview.Location = new Point(474, 77);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Size = new Size(536, 351);
            pnlPreview.TabIndex = 6;
            pnlPreview.Paint += pnlPreview_Paint;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label4.Location = new Point(474, 53);
            label4.Name = "label4";
            label4.Size = new Size(109, 21);
            label4.TabIndex = 5;
            label4.Text = "Print Preview:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(18, 14);
            label1.Name = "label1";
            label1.Size = new Size(447, 37);
            label1.TabIndex = 0;
            label1.Text = "QR Code Generator | Bin Location";
            // 
            // panel2
            // 
            panel2.BackColor = Color.RoyalBlue;
            panel2.Controls.Add(label9);
            panel2.Location = new Point(-1, 79);
            panel2.Name = "panel2";
            panel2.Size = new Size(1147, 60);
            panel2.TabIndex = 20;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label9.ForeColor = SystemColors.ButtonHighlight;
            label9.Location = new Point(16, 23);
            label9.Name = "label9";
            label9.Size = new Size(117, 25);
            label9.TabIndex = 19;
            label9.Text = "Bin Location";
            // 
            // panel3
            // 
            panel3.BackColor = Color.MediumBlue;
            panel3.Controls.Add(label10);
            panel3.Location = new Point(-1, 0);
            panel3.Name = "panel3";
            panel3.Size = new Size(1147, 90);
            panel3.TabIndex = 19;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label10.ForeColor = SystemColors.ButtonHighlight;
            label10.Location = new Point(16, 33);
            label10.Name = "label10";
            label10.Size = new Size(264, 30);
            label10.TabIndex = 20;
            label10.Text = "Panasonic | QR Generator";
            // 
            // panel4
            // 
            panel4.BackColor = SystemColors.ActiveBorder;
            panel4.Controls.Add(panel3);
            panel4.Controls.Add(panel2);
            panel4.Controls.Add(panel1);
            panel4.Location = new Point(69, 41);
            panel4.Name = "panel4";
            panel4.Size = new Size(1137, 713);
            panel4.TabIndex = 22;
            // 
            // Uploadbutt
            // 
            Uploadbutt.BackColor = Color.LightBlue;
            Uploadbutt.Location = new Point(921, 467);
            Uploadbutt.Name = "Uploadbutt";
            Uploadbutt.Size = new Size(89, 23);
            Uploadbutt.TabIndex = 22;
            Uploadbutt.Text = "Upload";
            Uploadbutt.UseVisualStyleBackColor = false;
            Uploadbutt.Click += Uploadbutt_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Snow;
            ClientSize = new Size(1279, 832);
            Controls.Add(panel4);
            Name = "Form1";
            Text = "QR Code UI";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel4.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        public Panel panel1;
        public Panel pnlPreview;
        public Label label4;
        public TextBox txtBinLocation2;
        public TextBox txtBinLocation1;
        public Label label3;
        public Label label2;
        public Label label1;
        public Button btnPrint;
        public Button btnClear;
        public Button btnGenerate;
        public Button btnConvertToPdf;
        public Label label5;
        public TextBox txtCustomHeight;
        public TextBox txtCustomWidth;
        public Label label7;
        public Label label6;
        public Label label8;
        private ComboBox cmbBatch;
        public Button btnPrintAll;
        private GroupBox groupBox1;
        private Panel panel2;
        private Label label9;
        private Panel panel3;
        private Label label10;
        private Panel panel4;
        private Button Uploadbutt;
    }
}
