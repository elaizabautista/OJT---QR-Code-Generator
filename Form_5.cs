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
        // Active bins for the 2x2 layout (4 total)
        private string _activeBin1 = string.Empty; // Top-left
        private string _activeBin2 = string.Empty; // Top-right
        private string _activeBin3 = string.Empty; // Bottom-left
        private string _activeBin4 = string.Empty; // Bottom-right

        // Each batch page now holds FOUR bin values (one per grid cell)
        private List<(string bin1, string bin2, string bin3, string bin4)> _batchPages = new List<(string, string, string, string)>();
        private int _batchPageIndex = 0;

        public Form_5()
        {
            InitializeComponent();
            this.pnlPreview.Paint += new PaintEventHandler(this.pnlPreview_Paint);
        }

        private void CenterPanel()
        {
            panel4.Left = (this.ClientSize.Width - panel4.Width) / 2;
            panel4.Top = (this.ClientSize.Height - panel4.Height) / 2;
        }

        private void Form_5_Load(object sender, EventArgs e)
        {
            CenterPanel();
            this.Resize += (s, e) => CenterPanel();

            txtCustomWidth.Text = "3";
            txtCustomHeight.Text = "6";

            // Populate the batch dropdown directly from Form5Data's zone/building keys
            cmbBatch.Items.Clear();
            foreach (string zoneKey in Form5Data.ZoneToMaterials.Keys)
            {
                cmbBatch.Items.Add(zoneKey);
            }
        }

        // --- Invalidation Handlers ---
        // Note: txtBinLocation1/2/textBox1/txtBinLocation4 now map to the 4 grid cells.
        // Location5 / txtBinLocation6 are kept wired up (in case the designer still
        // references them) but are no longer part of the printed layout.
        private void txtBinLocation1_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtBinLocation2_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void textBox1_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtBinLocation4_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtBinLocation5_TextChanged(object sender, EventArgs e) { /* unused in 2x2 layout */ }
        private void txtBinLocation6_TextChanged(object sender, EventArgs e) { /* unused in 2x2 layout */ }

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

            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) &&
                string.IsNullOrEmpty(_activeBin3) && string.IsNullOrEmpty(_activeBin4))
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


            _activeBin1 = string.Empty;
            _activeBin2 = string.Empty;
            _activeBin3 = string.Empty;
            _activeBin4 = string.Empty;

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
            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) &&
                string.IsNullOrEmpty(_activeBin3) && string.IsNullOrEmpty(_activeBin4))
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

            // Loop steps by 4 now (one page = 4 bins in a 2x2 grid)
            for (int i = 0; i < materials.Length; i += 4)
            {
                string b1 = materials[i];
                string b2 = (i + 1 < materials.Length) ? materials[i + 1] : string.Empty;
                string b3 = (i + 2 < materials.Length) ? materials[i + 2] : string.Empty;
                string b4 = (i + 3 < materials.Length) ? materials[i + 3] : string.Empty;

                _batchPages.Add((b1, b2, b3, b4));
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
            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) &&
                string.IsNullOrEmpty(_activeBin3) && string.IsNullOrEmpty(_activeBin4))
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

            _activeBin1 = _batchPages[_batchPageIndex].bin1;
            _activeBin2 = _batchPages[_batchPageIndex].bin2;
            _activeBin3 = _batchPages[_batchPageIndex].bin3;
            _activeBin4 = _batchPages[_batchPageIndex].bin4;

            PrintLabelsHandler(sender, e);

            _activeBin1 = savedBin1;
            _activeBin2 = savedBin2;
            _activeBin3 = savedBin3;
            _activeBin4 = savedBin4;

            _batchPageIndex++;
            e.HasMorePages = _batchPageIndex < _batchPages.Count;
        }

        // 2x2 LAYOUT: 2 columns, 2 rows, vertical divider ONLY (no horizontal line)
        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            int margin = isPrinting ? 20 : 8;
            int safeX = margin;
            int safeY = margin;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);

            // Vertical divider only — rows are separated by space, not a drawn line
            int dividerThickness = 2;
            int columnWidth = (safeWidth - dividerThickness) / 2;
            int rowHeight = safeHeight / 2;
            int rightColumnX = safeX + columnWidth + dividerThickness;
            int bottomRowY = safeY + rowHeight;

            // Draw ONLY the vertical divider line (splits left/right columns)
            using (Pen dividerPen = new Pen(Color.Black, dividerThickness))
            {
                g.DrawLine(dividerPen, safeX + columnWidth, safeY, safeX + columnWidth, safeY + safeHeight);
            }

            string[] bins = { _activeBin1, _activeBin2, _activeBin3, _activeBin4 };

            for (int i = 0; i < 4; i++)
            {
                string bin = bins[i];
                if (string.IsNullOrEmpty(bin)) continue;

                int rowIndex = i / 2;
                int colIndex = i % 2;

                int currentX = (colIndex == 0) ? safeX : rightColumnX;
                int rowTop = (rowIndex == 0) ? safeY : bottomRowY;
                int rowBottom = rowTop + rowHeight;

                // QR sized against column width (the real constraint), with minimal padding
                // so it grows as large as possible
                int qrPadding = isPrinting ? 6 : 3;
                int qrSize = columnWidth - (qrPadding * 2);

                int gapBetweenQrAndText = isPrinting ? 4 : 2;
                int textBlockHeight = (int)(rowHeight * 0.18f);
                int contentHeight = qrSize + gapBetweenQrAndText + textBlockHeight;

                // Small fixed margin that pulls each block toward the center divider
                // instead of centering it in the full (tall) row
                int marginFromDivider = isPrinting ? 20 : 10;

                int qrY;
                if (rowIndex == 0)
                {
                    // Top row: anchor the block's BOTTOM near the divider
                    int contentBottom = rowBottom - marginFromDivider;
                    qrY = contentBottom - contentHeight + (contentHeight - (qrSize + gapBetweenQrAndText + textBlockHeight)); // no-op, kept for clarity
                    qrY = contentBottom - contentHeight;
                    if (qrY < rowTop) qrY = rowTop; // safety clamp
                }
                else
                {
                    // Bottom row: anchor the block's TOP near the divider
                    qrY = rowTop + marginFromDivider;
                    if (qrY + contentHeight > rowBottom) qrY = rowBottom - contentHeight; // safety clamp
                }

                int qrX = currentX + (columnWidth - qrSize) / 2;

                using (Bitmap qrImg = CreateQRCodeImage(bin))
                {
                    if (qrImg != null)
                    {
                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);
                    }
                }

                int textTop = qrY + qrSize + gapBetweenQrAndText;
                int textPadding = 6;
                DrawTextAutofit(g, bin, "Arial", FontStyle.Bold, textBlockHeight * 0.9f,
                    currentX + textPadding, textTop, columnWidth - (textPadding * 2), textBlockHeight);
            }
        }

        private void DrawTextAutofit(Graphics g, string text, string fontFamily, FontStyle style, float maxFontSize, int x, int y, int maxWidth, int maxHeight, Brush brush = null)
        {
            if (brush == null) brush = Brushes.Black;

            float currentSize = maxFontSize;
            Font testFont = new Font(fontFamily, currentSize, style);
            SizeF size = g.MeasureString(text, testFont);

            // Auto-fit the text to the maximum allowable bounds, shrinking only when necessary
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