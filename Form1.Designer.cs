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
            btnPrintAll = new Button();
            label8 = new Label();
            cmbBatch = new ComboBox();
            txtCustomHeight = new TextBox();
            txtCustomWidth = new TextBox();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            btnConvertToPdf = new Button();
            btnPrint = new Button();
            btnClear = new Button();
            btnGenerate = new Button();
            pnlPreview = new Panel();
            label4 = new Label();
            txtBinLocation2 = new TextBox();
            txtBinLocation1 = new TextBox();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            GenerateNameNNumberButt = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.LightSteelBlue;
            panel1.Controls.Add(GenerateNameNNumberButt);
            panel1.Controls.Add(btnPrintAll);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(cmbBatch);
            panel1.Controls.Add(txtCustomHeight);
            panel1.Controls.Add(txtCustomWidth);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(btnConvertToPdf);
            panel1.Controls.Add(btnPrint);
            panel1.Controls.Add(btnClear);
            panel1.Controls.Add(btnGenerate);
            panel1.Controls.Add(pnlPreview);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(txtBinLocation2);
            panel1.Controls.Add(txtBinLocation1);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(53, 25);
            panel1.Name = "panel1";
            panel1.Size = new Size(1043, 514);
            panel1.TabIndex = 0;
            // 
            // btnPrintAll
            // 
            btnPrintAll.BackColor = Color.AliceBlue;
            btnPrintAll.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPrintAll.Location = new Point(64, 423);
            btnPrintAll.Name = "btnPrintAll";
            btnPrintAll.Size = new Size(151, 48);
            btnPrintAll.TabIndex = 19;
            btnPrintAll.Text = "Print All";
            btnPrintAll.UseVisualStyleBackColor = false;
            btnPrintAll.Click += btnPrintAll_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label8.Location = new Point(61, 203);
            label8.Name = "label8";
            label8.Size = new Size(55, 21);
            label8.TabIndex = 18;
            label8.Text = "Batch:";
            // 
            // cmbBatch
            // 
            cmbBatch.FormattingEnabled = true;
            cmbBatch.Location = new Point(64, 227);
            cmbBatch.Name = "cmbBatch";
            cmbBatch.Size = new Size(298, 23);
            cmbBatch.TabIndex = 17;
            cmbBatch.SelectedIndexChanged += cmbBatch_SelectedIndexChanged;
            // 
            // txtCustomHeight
            // 
            txtCustomHeight.Location = new Point(277, 315);
            txtCustomHeight.Name = "txtCustomHeight";
            txtCustomHeight.Size = new Size(50, 23);
            txtCustomHeight.TabIndex = 16;
            // 
            // txtCustomWidth
            // 
            txtCustomWidth.Location = new Point(156, 315);
            txtCustomWidth.Name = "txtCustomWidth";
            txtCustomWidth.Size = new Size(51, 23);
            txtCustomWidth.TabIndex = 15;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(225, 320);
            label7.Name = "label7";
            label7.Size = new Size(46, 15);
            label7.TabIndex = 14;
            label7.Text = "Height:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(108, 320);
            label6.Name = "label6";
            label6.Size = new Size(42, 15);
            label6.TabIndex = 13;
            label6.Text = "Width:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label5.Location = new Point(61, 273);
            label5.Name = "label5";
            label5.Size = new Size(89, 21);
            label5.TabIndex = 12;
            label5.Text = "Paper Size:";
            // 
            // btnConvertToPdf
            // 
            btnConvertToPdf.BackColor = Color.AliceBlue;
            btnConvertToPdf.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnConvertToPdf.Location = new Point(446, 432);
            btnConvertToPdf.Name = "btnConvertToPdf";
            btnConvertToPdf.Size = new Size(165, 39);
            btnConvertToPdf.TabIndex = 10;
            btnConvertToPdf.Text = "Convert to PDF";
            btnConvertToPdf.UseVisualStyleBackColor = false;
            btnConvertToPdf.Click += btnConvertToPdf_Click;
            // 
            // btnPrint
            // 
            btnPrint.BackColor = Color.AliceBlue;
            btnPrint.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPrint.Location = new Point(231, 423);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(151, 48);
            btnPrint.TabIndex = 9;
            btnPrint.Text = "Print";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += btnPrint_Click;
            // 
            // btnClear
            // 
            btnClear.BackColor = Color.AliceBlue;
            btnClear.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnClear.Location = new Point(231, 369);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(151, 48);
            btnClear.TabIndex = 8;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = false;
            btnClear.Click += btnClear_Click;
            // 
            // btnGenerate
            // 
            btnGenerate.BackColor = Color.AliceBlue;
            btnGenerate.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnGenerate.Location = new Point(64, 369);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(151, 48);
            btnGenerate.TabIndex = 7;
            btnGenerate.Text = "Generate";
            btnGenerate.UseVisualStyleBackColor = false;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // pnlPreview
            // 
            pnlPreview.Location = new Point(446, 134);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Size = new Size(549, 292);
            pnlPreview.TabIndex = 6;
            pnlPreview.Paint += pnlPreview_Paint;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label4.Location = new Point(446, 107);
            label4.Name = "label4";
            label4.Size = new Size(109, 21);
            label4.TabIndex = 5;
            label4.Text = "Print Preview:";
            // 
            // txtBinLocation2
            // 
            txtBinLocation2.Location = new Point(64, 171);
            txtBinLocation2.Name = "txtBinLocation2";
            txtBinLocation2.Size = new Size(298, 23);
            txtBinLocation2.TabIndex = 4;
            // 
            // txtBinLocation1
            // 
            txtBinLocation1.Location = new Point(64, 109);
            txtBinLocation1.Name = "txtBinLocation1";
            txtBinLocation1.Size = new Size(298, 23);
            txtBinLocation1.TabIndex = 3;
            txtBinLocation1.TextChanged += txtBinLocation1_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(61, 147);
            label3.Name = "label3";
            label3.Size = new Size(108, 21);
            label3.TabIndex = 2;
            label3.Text = "Part Number:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label2.Location = new Point(64, 85);
            label2.Name = "label2";
            label2.Size = new Size(90, 21);
            label2.TabIndex = 1;
            label2.Text = "Part Name:";
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
            // GenerateNameNNumberButt
            // 
            GenerateNameNNumberButt.BackColor = Color.AliceBlue;
            GenerateNameNNumberButt.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            GenerateNameNNumberButt.Location = new Point(830, 432);
            GenerateNameNNumberButt.Name = "GenerateNameNNumberButt";
            GenerateNameNNumberButt.Size = new Size(165, 39);
            GenerateNameNNumberButt.TabIndex = 20;
            GenerateNameNNumberButt.Text = "Generate Item";
            GenerateNameNNumberButt.UseVisualStyleBackColor = false;
            GenerateNameNNumberButt.Click += GenerateNameNNumberButt_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.LightSlateGray;
            ClientSize = new Size(1139, 587);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "QR Code UI";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
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
        public Button GenerateNameNNumberButt;
    }
}
