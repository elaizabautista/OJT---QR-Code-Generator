namespace OJT___QR_Code_Generator
{
    partial class Main_Menu
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
            label1 = new Label();
            label2 = new Label();
            btnBinLocation = new Button();
            label3 = new Label();
            label4 = new Label();
            btnNamingPart = new Button();
            btnFloorBin = new Button();
            btnFinishedGoods = new Button();
            btnForm5 = new Button();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(28, 30);
            label1.Name = "label1";
            label1.Size = new Size(358, 50);
            label1.TabIndex = 1;
            label1.Text = "QR Code Generator";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(227, 80);
            label2.Name = "label2";
            label2.Size = new Size(156, 37);
            label2.TabIndex = 2;
            label2.Text = "Main menu";
            // 
            // btnBinLocation
            // 
            btnBinLocation.Location = new Point(240, 154);
            btnBinLocation.Name = "btnBinLocation";
            btnBinLocation.Size = new Size(173, 36);
            btnBinLocation.TabIndex = 3;
            btnBinLocation.Text = "Click here";
            btnBinLocation.UseVisualStyleBackColor = true;
            btnBinLocation.Click += btnBinLocation_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F);
            label3.Location = new Point(28, 169);
            label3.Name = "label3";
            label3.Size = new Size(95, 21);
            label3.TabIndex = 8;
            label3.Text = "Bin Location";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 12F);
            label4.Location = new Point(28, 207);
            label4.Name = "label4";
            label4.Size = new Size(206, 21);
            label4.TabIndex = 9;
            label4.Text = "Part Number and Part Name";
            // 
            // btnNamingPart
            // 
            btnNamingPart.Location = new Point(240, 192);
            btnNamingPart.Name = "btnNamingPart";
            btnNamingPart.Size = new Size(173, 36);
            btnNamingPart.TabIndex = 10;
            btnNamingPart.Text = "Click here";
            btnNamingPart.UseVisualStyleBackColor = true;
            btnNamingPart.Click += btnNamingPart_Click;
            // 
            // btnFloorBin
            // 
            btnFloorBin.Location = new Point(240, 230);
            btnFloorBin.Name = "btnFloorBin";
            btnFloorBin.Size = new Size(173, 36);
            btnFloorBin.TabIndex = 11;
            btnFloorBin.Text = "Click here";
            btnFloorBin.UseVisualStyleBackColor = true;
            btnFloorBin.Click += btnFloorBin_Click;
            // 
            // btnFinishedGoods
            // 
            btnFinishedGoods.Location = new Point(240, 269);
            btnFinishedGoods.Name = "btnFinishedGoods";
            btnFinishedGoods.Size = new Size(173, 36);
            btnFinishedGoods.TabIndex = 12;
            btnFinishedGoods.Text = "Click here";
            btnFinishedGoods.UseVisualStyleBackColor = true;
            btnFinishedGoods.Click += btnFinishedGoods_Click;
            // 
            // btnForm5
            // 
            btnForm5.Location = new Point(240, 308);
            btnForm5.Name = "btnForm5";
            btnForm5.Size = new Size(173, 36);
            btnForm5.TabIndex = 13;
            btnForm5.Text = "Click here";
            btnForm5.UseVisualStyleBackColor = true;
            btnForm5.Click += btnForm5_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 12F);
            label5.Location = new Point(28, 245);
            label5.Name = "label5";
            label5.Size = new Size(72, 21);
            label5.TabIndex = 14;
            label5.Text = "Floor Bin";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 12F);
            label6.Location = new Point(28, 284);
            label6.Name = "label6";
            label6.Size = new Size(117, 21);
            label6.TabIndex = 15;
            label6.Text = "Finished Goods";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 12F);
            label7.Location = new Point(28, 323);
            label7.Name = "label7";
            label7.Size = new Size(60, 21);
            label7.TabIndex = 16;
            label7.Text = "Form 5";
            // 
            // Main_Menu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(425, 369);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(btnForm5);
            Controls.Add(btnFinishedGoods);
            Controls.Add(btnFloorBin);
            Controls.Add(btnNamingPart);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(btnBinLocation);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "Main_Menu";
            Text = "Main_Menu";
            Load += Main_Menu_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public Label label1;
        public Label label2;
        private Button btnBinLocation;
        private Label label3;
        private Label label4;
        private Button btnNamingPart;
        private Button btnFloorBin;
        private Button btnFinishedGoods;
        private Button btnForm5;
        private Label label5;
        private Label label6;
        private Label label7;
    }
}