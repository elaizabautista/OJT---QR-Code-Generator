using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
using QRCoder;

namespace OJT___QR_Code_Generator
{
    public partial class Form1 : Form
    {
        private string _activeBin1 = string.Empty;
        private string _activeBin2 = string.Empty;

        public Form1()
        {
            InitializeComponent();

            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            this.btnConvertToPdf.Click += new System.EventHandler(this.btnConvertToPdf_Click);
            this.pnlPreview.Paint += new PaintEventHandler(this.pnlPreview_Paint);

            this.txtCustomWidth.TextChanged += (s, e) => pnlPreview.Invalidate();
            this.txtCustomHeight.TextChanged += (s, e) => pnlPreview.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtCustomWidth.Text = "3";
            txtCustomHeight.Text = "6";
        }

        private Size GetTargetPaperSizeInHundredths()
        {
            double widthInches = 3.0;
            double heightInches = 6.0;

            double.TryParse(txtCustomWidth.Text, out widthInches);
            double.TryParse(txtCustomHeight.Text, out heightInches);

            int w = (int)(widthInches * 100);
            int h = (int)(heightInches * 100);

            if (w <= 0) w = 300;
            if (h <= 0) h = 600;

            return new Size(w, h);
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _activeBin1 = txtBinLocation1.Text.Trim();
            _activeBin2 = txtBinLocation2.Text.Trim();

            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2))
            {
                MessageBox.Show("Please enter at least one Bin Location value to generate.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            pnlPreview.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtBinLocation1.Clear();
            txtBinLocation2.Clear();
            _activeBin1 = string.Empty;
            _activeBin2 = string.Empty;
            pnlPreview.Invalidate();
        }

        private void pnlPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Size paperSize = GetTargetPaperSizeInHundredths();
            int totalWidth = pnlPreview.Width;
            int totalHeight = pnlPreview.Height;

            float targetRatio = (float)paperSize.Width / paperSize.Height;
            float currentRatio = (float)totalWidth / totalHeight;

            if (currentRatio > targetRatio)
            {
                totalWidth = (int)(totalHeight * targetRatio);
            }
            else
            {
                totalHeight = (int)(totalWidth / targetRatio);
            }

            if (totalWidth < totalHeight)
            {
                int temp = totalWidth;
                totalWidth = totalHeight;
                totalHeight = temp;
            }

            RenderLabelLayout(g, totalWidth, totalHeight, isPrinting: false);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2))
            {
                MessageBox.Show("Please enter a Bin Location and click Generate before printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Landscape = (paperSize.Width > paperSize.Height) || (paperSize.Height > paperSize.Width);
                pd.DefaultPageSettings.PaperSize = new PaperSize("CustomSticker", paperSize.Width, paperSize.Height);
                pd.PrintPage += new PrintPageEventHandler(PrintLabelsHandler);

                using (PrintPreviewDialog previewDlg = new PrintPreviewDialog())
                {
                    previewDlg.Document = pd;
                    previewDlg.WindowState = FormWindowState.Maximized;
                    previewDlg.ShowDialog();
                }
            }
        }

        private void btnConvertToPdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2))
            {
                MessageBox.Show("Please generate a Bin Location layout before converting to PDF.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDlg.Title = "Save Bin Labels as PDF";
                saveDlg.FileName = $"Bin_Labels_{_activeBin1.Replace("-", "_")}";

                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    using (PrintDocument pd = new PrintDocument())
                    {
                        pd.DefaultPageSettings.Landscape = (paperSize.Width > paperSize.Height) || (paperSize.Height > paperSize.Width);
                        pd.DefaultPageSettings.PaperSize = new PaperSize("CustomSticker", paperSize.Width, paperSize.Height);
                        pd.OriginAtMargins = false;

                        pd.PrintPage += new PrintPageEventHandler(PrintLabelsHandler);
                        pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                        pd.PrinterSettings.PrintToFile = true;
                        pd.PrinterSettings.PrintFileName = saveDlg.FileName;

                        try
                        {
                            pd.Print();
                            MessageBox.Show("PDF File saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Could not save PDF file: {ex.Message}", "PDF Printing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void PrintLabelsHandler(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int printableWidth = e.PageBounds.Width;
            int printableHeight = e.PageBounds.Height;

            if (printableWidth < printableHeight)
            {
                g.TranslateTransform(printableWidth, 0);
                g.RotateTransform(90);

                int temp = printableWidth;
                printableWidth = printableHeight;
                printableHeight = temp;
            }

            RenderLabelLayout(g, printableWidth, printableHeight, isPrinting: true);
        }

        // 🛠️ MAXIMUM SCALE LAYOUT: Shifter divider right, pushes text and QR to the absolute limit
        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            int halfHeight = totalHeight / 2;

            // Shift the partition line further right to 75% to give the text box maximum stretching room
            int qrDividerX = (int)(totalWidth * 0.75);

            // 1. Draw layout grid lines
            int penThickness = isPrinting ? 4 : 2;
            using (Pen blackPen = new Pen(Color.Black, penThickness))
            {
                g.DrawRectangle(blackPen, 0, 0, totalWidth - 1, totalHeight - 1);
                g.DrawLine(blackPen, 0, halfHeight, totalWidth, halfHeight);
                g.DrawLine(blackPen, qrDividerX, 0, qrDividerX, totalHeight);
            }

            // 2. MAXIMUM BIN TEXT SIZE: Increased ratio to 0.26f with a 75% wide box means giant text that won't clip
            float fontSize = totalHeight * 0.26f;
            using (Font labelFont = new Font("Arial", fontSize, FontStyle.Bold))
            {
                // Top Row Bin Code
                if (!string.IsNullOrEmpty(_activeBin1))
                {
                    DrawTextCentered(g, _activeBin1, labelFont, 0, 0, qrDividerX, halfHeight);
                }

                // Bottom Row Bin Code
                if (!string.IsNullOrEmpty(_activeBin2))
                {
                    DrawTextCentered(g, _activeBin2, labelFont, 0, halfHeight, qrDividerX, halfHeight);
                }
            }

            // 3. MAXIMUM QR CODE SIZE: Pushed to 96% of the box height with a tight 2% border buffer
            int qrSize = (int)(halfHeight * 0.96);
            int qrX = qrDividerX + ((totalWidth - qrDividerX) - qrSize) / 2;

            // Top Row QR Code
            if (!string.IsNullOrEmpty(_activeBin1))
            {
                using (Bitmap qrImg = CreateQRCodeImage(_activeBin1))
                {
                    if (qrImg != null)
                    {
                        // Tight vertical centering
                        int qrY = (halfHeight - qrSize) / 2;
                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);
                    }
                }
            }

            // Bottom Row QR Code
            if (!string.IsNullOrEmpty(_activeBin2))
            {
                using (Bitmap qrImg = CreateQRCodeImage(_activeBin2))
                {
                    if (qrImg != null)
                    {
                        // Tight vertical centering
                        int qrY = halfHeight + (halfHeight - qrSize) / 2;
                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);
                    }
                }
            }
        }

        private void DrawTextCentered(Graphics g, string text, Font font, int x, int y, int width, int height)
        {
            SizeF textSize = g.MeasureString(text, font);
            float posX = x + (width - textSize.Width) / 2;
            float posY = y + (height - textSize.Height) / 2;
            g.DrawString(text, font, Brushes.Black, posX, posY);
        }

        private Bitmap CreateQRCodeImage(string text)
        {
            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
                    {
                        try
                        {
                            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                            {
                                byte[] qrCodeBytes = qrCode.GetGraphic(4);
                                using (var ms = new System.IO.MemoryStream(qrCodeBytes))
                                {
                                    return new Bitmap(ms);
                                }
                            }
                        }
                        catch
                        {
                            using (QRCode qrCode = new QRCode(qrCodeData))
                            {
                                return qrCode.GetGraphic(4, Color.Black, Color.White, true);
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}