namespace OJT___QR_Code_Generator
{
    partial class Finished_Goods
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
            txtFinishedGoodsLocation2 = new TextBox();
            txtFinishedGoodsLocation1 = new TextBox();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            panel1 = new Panel();
            groupBox1 = new GroupBox();
            panel2 = new Panel();
            label11 = new Label();
            panel3 = new Panel();
            label12 = new Label();
            panel1.SuspendLayout();
            groupBox1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // btnPrintAll
            // 
            btnPrintAll.BackColor = Color.LightSkyBlue;
            btnPrintAll.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPrintAll.Location = new Point(23, 392);
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
            label8.Location = new Point(20, 172);
            label8.Name = "label8";
            label8.Size = new Size(55, 21);
            label8.TabIndex = 18;
            label8.Text = "Batch:";
            // 
            // cmbBatch
            // 
            cmbBatch.FormattingEnabled = true;
            cmbBatch.Location = new Point(23, 196);
            cmbBatch.Name = "cmbBatch";
            cmbBatch.Size = new Size(298, 23);
            cmbBatch.TabIndex = 17;
            // 
            // txtCustomHeight
            // 
            txtCustomHeight.Location = new Point(236, 284);
            txtCustomHeight.Name = "txtCustomHeight";
            txtCustomHeight.Size = new Size(50, 23);
            txtCustomHeight.TabIndex = 16;
            // 
            // txtCustomWidth
            // 
            txtCustomWidth.Location = new Point(115, 284);
            txtCustomWidth.Name = "txtCustomWidth";
            txtCustomWidth.Size = new Size(51, 23);
            txtCustomWidth.TabIndex = 15;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(184, 289);
            label7.Name = "label7";
            label7.Size = new Size(46, 15);
            label7.TabIndex = 14;
            label7.Text = "Height:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(67, 289);
            label6.Name = "label6";
            label6.Size = new Size(42, 15);
            label6.TabIndex = 13;
            label6.Text = "Width:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label5.Location = new Point(20, 242);
            label5.Name = "label5";
            label5.Size = new Size(89, 21);
            label5.TabIndex = 12;
            label5.Text = "Paper Size:";
            // 
            // btnConvertToPdf
            // 
            btnConvertToPdf.BackColor = Color.AliceBlue;
            btnConvertToPdf.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnConvertToPdf.Location = new Point(679, 441);
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
            btnPrint.Location = new Point(190, 392);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(151, 48);
            btnPrint.TabIndex = 9;
            btnPrint.Text = "Print";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += btnPrint_Click;
            // 
            // btnClear
            // 
            btnClear.BackColor = Color.Brown;
            btnClear.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnClear.ForeColor = SystemColors.ControlLightLight;
            btnClear.Location = new Point(190, 338);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(151, 48);
            btnClear.TabIndex = 8;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = false;
            btnClear.Click += btnClear_Click;
            // 
            // btnGenerate
            // 
            btnGenerate.BackColor = Color.MediumBlue;
            btnGenerate.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnGenerate.ForeColor = SystemColors.ControlLightLight;
            btnGenerate.Location = new Point(23, 338);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(151, 48);
            btnGenerate.TabIndex = 7;
            btnGenerate.Text = "Generate";
            btnGenerate.UseVisualStyleBackColor = false;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // pnlPreview
            // 
            pnlPreview.Location = new Point(522, 143);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Size = new Size(473, 292);
            pnlPreview.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label4.Location = new Point(522, 119);
            label4.Name = "label4";
            label4.Size = new Size(109, 21);
            label4.TabIndex = 5;
            label4.Text = "Print Preview:";
            // 
            // txtFinishedGoodsLocation2
            // 
            txtFinishedGoodsLocation2.Location = new Point(23, 140);
            txtFinishedGoodsLocation2.Name = "txtFinishedGoodsLocation2";
            txtFinishedGoodsLocation2.Size = new Size(298, 23);
            txtFinishedGoodsLocation2.TabIndex = 4;
            // 
            // txtFinishedGoodsLocation1
            // 
            txtFinishedGoodsLocation1.Location = new Point(23, 78);
            txtFinishedGoodsLocation1.Name = "txtFinishedGoodsLocation1";
            txtFinishedGoodsLocation1.Size = new Size(298, 23);
            txtFinishedGoodsLocation1.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(20, 116);
            label3.Name = "label3";
            label3.Size = new Size(139, 21);
            label3.TabIndex = 2;
            label3.Text = "Finished Goods 2:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label2.Location = new Point(23, 54);
            label2.Name = "label2";
            label2.Size = new Size(136, 21);
            label2.TabIndex = 1;
            label2.Text = "Finished Goods 1:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(3, 36);
            label1.Name = "label1";
            label1.Size = new Size(483, 37);
            label1.TabIndex = 0;
            label1.Text = "QR Code Generator | Finished Goods";
            // 
            // panel1
            // 
            panel1.BackColor = Color.LightSteelBlue;
            panel1.Controls.Add(label4);
            panel1.Controls.Add(groupBox1);
            panel1.Controls.Add(btnConvertToPdf);
            panel1.Controls.Add(pnlPreview);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(27, 156);
            panel1.Name = "panel1";
            panel1.Size = new Size(1046, 592);
            panel1.TabIndex = 1;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnPrintAll);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(cmbBatch);
            groupBox1.Controls.Add(txtFinishedGoodsLocation1);
            groupBox1.Controls.Add(txtCustomHeight);
            groupBox1.Controls.Add(txtFinishedGoodsLocation2);
            groupBox1.Controls.Add(txtCustomWidth);
            groupBox1.Controls.Add(btnGenerate);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(btnClear);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(btnPrint);
            groupBox1.Controls.Add(label5);
            groupBox1.Location = new Point(66, 107);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(361, 463);
            groupBox1.TabIndex = 20;
            groupBox1.TabStop = false;
            // 
            // panel2
            // 
            panel2.BackColor = Color.RoyalBlue;
            panel2.Controls.Add(label11);
            panel2.Location = new Point(1, 76);
            panel2.Name = "panel2";
            panel2.Size = new Size(1150, 60);
            panel2.TabIndex = 30;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label11.ForeColor = SystemColors.ButtonHighlight;
            label11.Location = new Point(18, 14);
            label11.Name = "label11";
            label11.Size = new Size(142, 25);
            label11.TabIndex = 19;
            label11.Text = "Finished Goods";
            // 
            // panel3
            // 
            panel3.BackColor = Color.MediumBlue;
            panel3.Controls.Add(label12);
            panel3.Location = new Point(1, 0);
            panel3.Name = "panel3";
            panel3.Size = new Size(1150, 87);
            panel3.TabIndex = 29;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label12.ForeColor = SystemColors.ButtonHighlight;
            label12.Location = new Point(16, 25);
            label12.Name = "label12";
            label12.Size = new Size(264, 30);
            label12.TabIndex = 20;
            label12.Text = "Panasonic | QR Generator";
            // 
            // Finished_Goods
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.LightSlateGray;
            ClientSize = new Size(1105, 793);
            Controls.Add(panel2);
            Controls.Add(panel3);
            Controls.Add(panel1);
            Name = "Finished_Goods";
            Text = "Finished_Goods";
            Load += Finished_Goods_Load;
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
        public Button btnPrintAll;
        public Label label8;
        private ComboBox cmbBatch;
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
        public TextBox txtFinishedGoodsLocation2;
        public TextBox txtFinishedGoodsLocation1;
        public Label label3;
        public Label label2;
        public Label label1;
        public Panel panel1;
        private Panel panel2;
        private Label label11;
        private Panel panel3;
        private Label label12;
        private GroupBox groupBox1;
    }
}