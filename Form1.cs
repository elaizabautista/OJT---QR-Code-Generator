using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
using QRCoder;
using System.Collections.Generic;

namespace OJT___QR_Code_Generator
{
    public partial class Form1 : Form
    {
        private string _activeBin1 = string.Empty;
        private string _activeBin2 = string.Empty;

        private List<(string bin1, string bin2)> _batchPages = new List<(string, string)>();
        private int _batchPageIndex = 0;

        public Form1()
        {
            InitializeComponent();

            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            this.btnConvertToPdf.Click += new System.EventHandler(this.btnConvertToPdf_Click);
            this.btnPrintAll.Click += new System.EventHandler(this.btnPrintAll_Click);   // <-- add this line
            this.pnlPreview.Paint += new PaintEventHandler(this.pnlPreview_Paint);

            this.txtCustomWidth.TextChanged += (s, e) => pnlPreview.Invalidate();
            this.txtCustomHeight.TextChanged += (s, e) => pnlPreview.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtCustomWidth.Text = "3";
            txtCustomHeight.Text = "6";

            for (int i = 1; i <= 26; i++)
            {
                cmbBatch.Items.Add("Zone " + i);
            }
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

        private void PrintBatchPageHandler(object sender, PrintPageEventArgs e)
        {
            if (_batchPageIndex >= _batchPages.Count)
            {
                e.HasMorePages = false;
                return;
            }

            // Temporarily swap in this page's bin values so the existing render path handles it
            string savedBin1 = _activeBin1;
            string savedBin2 = _activeBin2;

            _activeBin1 = _batchPages[_batchPageIndex].bin1;
            _activeBin2 = _batchPages[_batchPageIndex].bin2;

            PrintLabelsHandler(sender, e); // exact same rotation + RenderLabelLayout call as manual mode

            _activeBin1 = savedBin1;
            _activeBin2 = savedBin2;

            _batchPageIndex++;
            e.HasMorePages = _batchPageIndex < _batchPages.Count;
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

        // 🛠️ HONEYWELL COMPATIBLE HIGH-SCALE LAYOUT
        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            // Safeguard border cutoff zone on physical thermal heads (Honeywell standard unprintable margins)
            int margin = isPrinting ? 20 : 8;

            int safeX = margin;
            int safeY = margin;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);
            int halfHeight = safeHeight / 2;

            // Maintain your 75% wide partition strategy securely in safe zones
            int qrDividerX = safeX + (int)(safeWidth * 0.75);

            // 1. Draw frame paths
            int penThickness = isPrinting ? 4 : 2;
            using (Pen blackPen = new Pen(Color.Black, penThickness))
            {
                g.DrawRectangle(blackPen, safeX, safeY, safeWidth, safeHeight);
                g.DrawLine(blackPen, safeX, safeY + halfHeight, safeX + safeWidth, safeY + halfHeight);
                g.DrawLine(blackPen, qrDividerX, safeY, qrDividerX, safeY + safeHeight);
            }

            // 2. MAXIMUM AUTOFIT BIN TEXT: Automatically climbs to the tallest absolute size allowed by the height bounds.
            float maxFontCeiling = safeHeight * 0.32f;
            int textPadding = 10;
            int textBoxWidth = (qrDividerX - safeX) - textPadding;
            int textBoxHeight = halfHeight - textPadding;

            // Top Row Bin Code Text
            if (!string.IsNullOrEmpty(_activeBin1))
            {
                DrawTextAutofit(g, _activeBin1, "Arial", FontStyle.Bold, maxFontCeiling, safeX + (textPadding / 2), safeY + (textPadding / 2), textBoxWidth, textBoxHeight);
            }

            // Bottom Row Bin Code Text
            if (!string.IsNullOrEmpty(_activeBin2))
            {
                DrawTextAutofit(g, _activeBin2, "Arial", FontStyle.Bold, maxFontCeiling, safeX + (textPadding / 2), safeY + halfHeight + (textPadding / 2), textBoxWidth, textBoxHeight);
            }

            // 3. MAXIMIZED SYSTEM QR CODES: Scales cleanly to 95% of available inner box area height
            int qrSize = (int)(halfHeight * 0.95);
            int rightCompartmentWidth = (safeX + safeWidth) - qrDividerX;
            int qrX = qrDividerX + (rightCompartmentWidth - qrSize) / 2;

            // Top Row QR Code
            if (!string.IsNullOrEmpty(_activeBin1))
            {
                using (Bitmap qrImg = CreateQRCodeImage(_activeBin1))
                {
                    if (qrImg != null)
                    {
                        int qrY = safeY + (halfHeight - qrSize) / 2;
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
                        int qrY = safeY + halfHeight + (halfHeight - qrSize) / 2;
                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);
                    }
                }
            }
        }

        // Adaptive font crunching ruleset to ensure maximum enlargement on both 6" and 5.4" lengths without bounds breaches
        private void DrawTextAutofit(Graphics g, string text, string fontFamily, FontStyle style, float maxFontSize, int x, int y, int maxWidth, int maxHeight)
        {
            float currentSize = maxFontSize;
            Font testFont = new Font(fontFamily, currentSize, style);
            SizeF size = g.MeasureString(text, testFont);

            while ((size.Width > maxWidth || size.Height > maxHeight) && currentSize > 8f)
            {
                currentSize -= 1f;
                testFont.Dispose();
                testFont = new Font(fontFamily, currentSize, style);
                size = g.MeasureString(text, testFont);
            }

            using (testFont)
            {
                float posX = x + (maxWidth - size.Width) / 2;
                float posY = y + (maxHeight - size.Height) / 2;
                g.DrawString(text, testFont, Brushes.Black, posX, posY);
            }
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

        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            if (cmbBatch.SelectedItem == null)
            {
                MessageBox.Show("Please select a Zone from the Batch dropdown.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selected = cmbBatch.SelectedItem.ToString();
            int zoneNum;
            if (!int.TryParse(selected.Replace("Zone ", "").Trim(), out zoneNum) || !WarehouseData.Zones.ContainsKey(zoneNum))
            {
                MessageBox.Show("Selected zone has no bin location data.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] bins = WarehouseData.Zones[zoneNum];
            if (bins == null || bins.Length == 0)
            {
                MessageBox.Show("Selected zone has no bin locations.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _batchPages.Clear();
            for (int i = 0; i < bins.Length; i += 2)
            {
                string bin1 = bins[i];
                string bin2 = (i + 1 < bins.Length) ? bins[i + 1] : string.Empty;
                _batchPages.Add((bin1, bin2));
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Landscape = (paperSize.Width > paperSize.Height) || (paperSize.Height > paperSize.Width);
                pd.DefaultPageSettings.PaperSize = new PaperSize("CustomSticker", paperSize.Width, paperSize.Height);

                // Reset the page cursor every time this document starts a print/preview pass —
                // PrintPreviewDialog can trigger BeginPrint more than once (resize, zoom, view switch)
                pd.BeginPrint += (s, ea) => { _batchPageIndex = 0; };

                pd.PrintPage += new PrintPageEventHandler(PrintBatchPageHandler);

                using (PrintPreviewDialog previewDlg = new PrintPreviewDialog())
                {
                    previewDlg.Document = pd;
                    previewDlg.WindowState = FormWindowState.Maximized;
                    previewDlg.ShowDialog();
                }
            }

            pnlPreview.Invalidate();
        }

        private void GenerateNameNNumberButt_Click(object sender, EventArgs e)
        {
            Naming_Part_From namingForm = new Naming_Part_From();
            namingForm.ShowDialog();
        }
        private void cmbBatch_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtBinLocation1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}