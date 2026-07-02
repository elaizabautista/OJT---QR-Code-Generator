using QRCoder;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace OJT___QR_Code_Generator
{
    public partial class Naming_Part_From : Form
    {
        // NOTE: renamed from _activeBin1 / _activeBin2 to reflect Part Name / Part Number usage
        private string _activePartName = string.Empty;
        private string _activePartNumber = string.Empty;

        // NOTE: batch pages now hold (partName, partNumber) pairs instead of bin pairs.
        // If WarehouseData.Zones still returns bin-location strings, you'll want to point
        // this at a part-number data source instead (see btnPrintAll_Click below).
        private List<(string partName, string partNumber)> _batchPages = new List<(string, string)>();
        private int _batchPageIndex = 0;

        public Naming_Part_From()
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

        private void Naming_Part_From_Load(object sender, EventArgs e)
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

        private void PrintBatchPageHandler(object sender, PrintPageEventArgs e)
        {
            if (_batchPageIndex >= _batchPages.Count)
            {
                e.HasMorePages = false;
                return;
            }

            // Temporarily swap in this page's part values so the existing render path handles it
            string savedName = _activePartName;
            string savedNumber = _activePartNumber;

            _activePartName = _batchPages[_batchPageIndex].partName;
            _activePartNumber = _batchPages[_batchPageIndex].partNumber;

            PrintLabelsHandler(sender, e); // exact same rotation + RenderLabelLayout call as manual mode

            _activePartName = savedName;
            _activePartNumber = savedNumber;

            _batchPageIndex++;
            e.HasMorePages = _batchPageIndex < _batchPages.Count;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _activePartName = txtBinLocation1.Text.Trim();
            _activePartNumber = txtBinLocation2.Text.Trim();

            if (string.IsNullOrEmpty(_activePartName) && string.IsNullOrEmpty(_activePartNumber))
            {
                MessageBox.Show("Please enter Part Name and/or Part Number to generate.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            pnlPreview.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtBinLocation1.Clear();
            txtBinLocation2.Clear();
            _activePartName = string.Empty;
            _activePartNumber = string.Empty;
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
            if (string.IsNullOrEmpty(_activePartName) && string.IsNullOrEmpty(_activePartNumber))
            {
                MessageBox.Show("Please enter a Part Name/Number and click Generate before printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (string.IsNullOrEmpty(_activePartName) && string.IsNullOrEmpty(_activePartNumber))
            {
                MessageBox.Show("Please generate a Part Name/Number layout before converting to PDF.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDlg.Title = "Save Part Label as PDF";
                saveDlg.FileName = $"Part_Label_{_activePartNumber.Replace("-", "_")}";

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

        // 🛠️ SINGLE-ROW PART LABEL: Part Name (top) / Part Number (bottom) on left, ONE shared QR on right
        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            // Safeguard border cutoff zone on physical thermal heads (Honeywell standard unprintable margins)
            int margin = isPrinting ? 20 : 8;

            int safeX = margin;
            int safeY = margin;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);
            int halfHeight = safeHeight / 2;

            // Maintain the 75% wide text partition / 25% QR partition
            int qrDividerX = safeX + (int)(safeWidth * 0.75);

            // 1. Draw frame: outer box, horizontal divider (Name|Number) only across the text column,
            //    vertical divider separating text column from the QR column
            int penThickness = isPrinting ? 4 : 2;
            using (Pen blackPen = new Pen(Color.Black, penThickness))
            {
                g.DrawRectangle(blackPen, safeX, safeY, safeWidth, safeHeight);
                g.DrawLine(blackPen, safeX, safeY + halfHeight, qrDividerX, safeY + halfHeight);
                g.DrawLine(blackPen, qrDividerX, safeY, qrDividerX, safeY + safeHeight);
            }

            // 2. MAXIMUM AUTOFIT TEXT: Part Name on top half, Part Number on bottom half
            float maxFontCeiling = safeHeight * 0.32f;
            int textPadding = 10;
            int textBoxWidth = (qrDividerX - safeX) - textPadding;
            int textBoxHeight = halfHeight - textPadding;

            if (!string.IsNullOrEmpty(_activePartName))
            {
                DrawTextAutofit(g, _activePartName, "Arial", FontStyle.Bold, maxFontCeiling,
                    safeX + (textPadding / 2), safeY + (textPadding / 2), textBoxWidth, textBoxHeight);
            }

            if (!string.IsNullOrEmpty(_activePartNumber))
            {
                DrawTextAutofit(g, _activePartNumber, "Arial", FontStyle.Bold, maxFontCeiling,
                    safeX + (textPadding / 2), safeY + halfHeight + (textPadding / 2), textBoxWidth, textBoxHeight);
            }

            // 3. ONE QR code spanning the full label height, encoding the part number
            //    (falls back to part name if number is blank)
            int rightCompartmentWidth = (safeX + safeWidth) - qrDividerX;
            int qrSize = (int)(Math.Min(safeHeight, rightCompartmentWidth) * 0.9);

            string qrPayload = $"{_activePartName} - {_activePartNumber}";

            if (!string.IsNullOrEmpty(qrPayload))
            {
                using (Bitmap qrImg = CreateQRCodeImage(qrPayload))
                {
                    if (qrImg != null)
                    {
                        int qrX = qrDividerX + (rightCompartmentWidth - qrSize) / 2;
                        int qrY = safeY + (safeHeight - qrSize) / 2;
                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);
                    }
                }
            }
        }

        // Adaptive font crunching ruleset to ensure maximum enlargement without bounds breaches
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

        

        private void cmbBatch_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
