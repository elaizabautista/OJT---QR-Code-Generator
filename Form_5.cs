using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using QRCoder;

namespace OJT___QR_Code_Generator
{
    public partial class Form_5 : Form
    {
        // Left Column Bins
        private string _activeBin1 = string.Empty;
        private string _activeBin2 = string.Empty;
        private string _activeBin3 = string.Empty;

        // Right Column Bins
        private string _activeBin4 = string.Empty;
        private string _activeBin5 = string.Empty;
        private string _activeBin6 = string.Empty;

        // Each batch page now holds SIX bin values (three per column)
        private List<(string bin1, string bin2, string bin3, string bin4, string bin5, string bin6)> _batchPages = new List<(string, string, string, string, string, string)>();
        private int _batchPageIndex = 0;

        public Form_5()
        {
            InitializeComponent();
            this.pnlPreview.Paint += new PaintEventHandler(this.pnlPreview_Paint);
        }

        private void Form_5_Load(object sender, EventArgs e)
        {
            txtCustomWidth.Text = "3";
            txtCustomHeight.Text = "6";

            // Populate the batch dropdown directly from Form5Data's zone/building keys
            // (e.g. "5THBLDG") instead of the old hard-coded "Zone 1".."Zone 26" list.
            cmbBatch.Items.Clear();
            foreach (string zoneKey in Form5Data.ZoneToMaterials.Keys)
            {
                cmbBatch.Items.Add(zoneKey);
            }
        }

        // --- Invalidation Handlers ---
        private void txtBinLocation1_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtBinLocation2_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void textBox1_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtBinLocation4_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtBinLocation5_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtBinLocation6_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }

        private void txtCustomWidth_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtCustomHeight_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }

        private void cmbBatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Handled during print
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _activeBin1 = txtBinLocation1.Text.Trim();
            _activeBin2 = txtBinLocation2.Text.Trim();
            _activeBin3 = textBox1.Text.Trim();
            _activeBin4 = txtBinLocation4.Text.Trim();
            _activeBin5 = Location5.Text.Trim(); // ✅ Fixed: Added this line
            _activeBin6 = txtBinLocation6.Text.Trim();

            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) && string.IsNullOrEmpty(_activeBin3) &&
                string.IsNullOrEmpty(_activeBin4) && string.IsNullOrEmpty(_activeBin5) && string.IsNullOrEmpty(_activeBin6))
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
            textBox1.Clear();
            txtBinLocation4.Clear();
            Location5.Clear(); // ✅ Fixed: Added this line
            txtBinLocation6.Clear();

            _activeBin1 = string.Empty;
            _activeBin2 = string.Empty;
            _activeBin3 = string.Empty;
            _activeBin4 = string.Empty;
            _activeBin5 = string.Empty;
            _activeBin6 = string.Empty;

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

            RenderLabelLayout(g, totalWidth, totalHeight, isPrinting: false);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) && string.IsNullOrEmpty(_activeBin3) &&
                string.IsNullOrEmpty(_activeBin4) && string.IsNullOrEmpty(_activeBin5) && string.IsNullOrEmpty(_activeBin6))
            {
                MessageBox.Show("Please enter a Bin Location and click Generate before printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Landscape = paperSize.Width > paperSize.Height;
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

        private void PrintAllButt_Click(object sender, EventArgs e)
        {
            if (cmbBatch.SelectedItem == null)
            {
                MessageBox.Show("Please select a Zone from the Batch dropdown.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Key is now a string (e.g. "5THBLDG") straight from the dropdown,
            // matched directly against Form5Data.ZoneToMaterials — no int parsing needed.
            string selectedZone = cmbBatch.SelectedItem.ToString();

            if (!Form5Data.ZoneToMaterials.ContainsKey(selectedZone))
            {
                MessageBox.Show("Selected zone has no material data.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] materials = Form5Data.ZoneToMaterials[selectedZone];
            if (materials == null || materials.Length == 0)
            {
                MessageBox.Show("Selected zone has no materials.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _batchPages.Clear();

            // Loop steps by 6 instead of 3
            for (int i = 0; i < materials.Length; i += 6)
            {
                string b1 = materials[i];
                string b2 = (i + 1 < materials.Length) ? materials[i + 1] : string.Empty;
                string b3 = (i + 2 < materials.Length) ? materials[i + 2] : string.Empty;
                string b4 = (i + 3 < materials.Length) ? materials[i + 3] : string.Empty;
                string b5 = (i + 4 < materials.Length) ? materials[i + 4] : string.Empty;
                string b6 = (i + 5 < materials.Length) ? materials[i + 5] : string.Empty;

                _batchPages.Add((b1, b2, b3, b4, b5, b6));
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Landscape = paperSize.Width > paperSize.Height;
                pd.DefaultPageSettings.PaperSize = new PaperSize("CustomSticker", paperSize.Width, paperSize.Height);

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

        private void btnConvertToPdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) && string.IsNullOrEmpty(_activeBin3) &&
                string.IsNullOrEmpty(_activeBin4) && string.IsNullOrEmpty(_activeBin5) && string.IsNullOrEmpty(_activeBin6))
            {
                MessageBox.Show("Please generate a Bin Location layout before converting to PDF.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDlg.Title = "Save Bin Labels as PDF";
                saveDlg.FileName = $"Bin_Labels_Export";

                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    using (PrintDocument pd = new PrintDocument())
                    {
                        pd.DefaultPageSettings.Landscape = paperSize.Width > paperSize.Height;
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

        private void PrintLabelsHandler(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int printableWidth = e.PageBounds.Width;
            int printableHeight = e.PageBounds.Height;

            RenderLabelLayout(g, printableWidth, printableHeight, isPrinting: true);
        }

        private void PrintBatchPageHandler(object sender, PrintPageEventArgs e)
        {
            if (_batchPageIndex >= _batchPages.Count)
            {
                e.HasMorePages = false;
                return;
            }

            string savedBin1 = _activeBin1;
            string savedBin2 = _activeBin2;
            string savedBin3 = _activeBin3;
            string savedBin4 = _activeBin4;
            string savedBin5 = _activeBin5;
            string savedBin6 = _activeBin6;

            _activeBin1 = _batchPages[_batchPageIndex].bin1;
            _activeBin2 = _batchPages[_batchPageIndex].bin2;
            _activeBin3 = _batchPages[_batchPageIndex].bin3;
            _activeBin4 = _batchPages[_batchPageIndex].bin4;
            _activeBin5 = _batchPages[_batchPageIndex].bin5;
            _activeBin6 = _batchPages[_batchPageIndex].bin6;

            PrintLabelsHandler(sender, e);

            _activeBin1 = savedBin1;
            _activeBin2 = savedBin2;
            _activeBin3 = savedBin3;
            _activeBin4 = savedBin4;
            _activeBin5 = savedBin5;
            _activeBin6 = savedBin6;

            _batchPageIndex++;
            e.HasMorePages = _batchPageIndex < _batchPages.Count;
        }

        // SPLIT LAYOUT: 2 columns, 3 rows, with vertical divider
        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            int margin = isPrinting ? 20 : 8;
            int safeX = margin;
            int safeY = margin;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);

            // Set up divider and column widths
            int dividerWidth = 2; // Thickness of the center line
            int columnWidth = (safeWidth - dividerWidth) / 2;
            int rightColumnX = safeX + columnWidth + dividerWidth;
            int rowHeight = safeHeight / 3;

            // Draw the vertical divider line
            using (Pen dividerPen = new Pen(Color.Black, dividerWidth))
            {
                g.DrawLine(dividerPen, safeX + columnWidth, safeY, safeX + columnWidth, safeY + safeHeight);
            }

            string[] bins = { _activeBin1, _activeBin2, _activeBin3, _activeBin4, _activeBin5, _activeBin6 };

            for (int i = 0; i < 6; i++)
            {
                string bin = bins[i];
                if (string.IsNullOrEmpty(bin)) continue;

                // Determine row (0, 1, 2) and column (0 for left, 1 for right)
                int rowIndex = i % 3;
                int colIndex = i / 3;

                int currentX = (colIndex == 0) ? safeX : rightColumnX;
                int rowTop = safeY + (rowHeight * rowIndex);

                int qrAreaHeight = (int)(rowHeight * 0.65);
                int textAreaHeight = rowHeight - qrAreaHeight;

                int qrPadding = isPrinting ? 8 : 4;
                int qrSize = Math.Min(qrAreaHeight, columnWidth) - (qrPadding * 2);

                using (Bitmap qrImg = CreateQRCodeImage(bin))
                {
                    if (qrImg != null)
                    {
                        int qrX = currentX + (columnWidth - qrSize) / 2;
                        int qrY = rowTop + (qrAreaHeight - qrSize) / 2;
                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);
                    }
                }

                int textTop = rowTop + qrAreaHeight;
                int textPadding = 6;
                DrawTextAutofit(g, bin, "Arial", FontStyle.Bold, textAreaHeight * 0.8f,
                    currentX + textPadding, textTop, columnWidth - (textPadding * 2), textAreaHeight);
            }
        }

        private void DrawTextAutofit(Graphics g, string text, string fontFamily, FontStyle style, float maxFontSize, int x, int y, int maxWidth, int maxHeight, Brush brush = null)
        {
            if (brush == null) brush = Brushes.Black;

            float currentSize = maxFontSize;
            Font testFont = new Font(fontFamily, currentSize, style);
            SizeF size = g.MeasureString(text, testFont);

            // 1. Auto-fit the text to the maximum allowable bounds
            while ((size.Width > maxWidth || size.Height > maxHeight) && currentSize > 8f)
            {
                currentSize -= 1f;
                testFont.Dispose();
                testFont = new Font(fontFamily, currentSize, style);
                size = g.MeasureString(text, testFont);
            }

            // 2. Force the final fitted font size down by an additional 1 unit
            if (currentSize > 8f)
            {
                currentSize -= 1f;
                testFont.Dispose();
                testFont = new Font(fontFamily, currentSize, style);
                size = g.MeasureString(text, testFont); // Re-measure so the smaller text centers correctly
            }

            // 3. Draw the text
            using (testFont)
            {
                float posX = x + (maxWidth - size.Width) / 2;
                float posY = y + (maxHeight - size.Height) / 2;
                g.DrawString(text, testFont, brush, posX, posY);
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

        private void Location5_TextChanged(object sender, EventArgs e)
        {

        }
    }
}