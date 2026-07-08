using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace OJT___QR_Code_Generator
{
    public partial class Naming_Part_From : Form
    {
        // 1. Keep only this updated version of the list:
        private List<((string name, string number) p1, (string name, string number)? p2)> _batchPages =
            new List<((string, string), (string, string)?)>();

        // 2. Keep your existing string variables:
        private string _activePartName = string.Empty;
        private string _activePartNumber = string.Empty;
        private string _activePartName2 = string.Empty;
        private string _activePartNumber2 = string.Empty;

        // 3. Keep the index and constants:
        private int _batchPageIndex = 0;
        private const double DefaultLabelWidthInches = 6.375;
        private const double DefaultLabelHeightInches = 2.625;


        public Naming_Part_From()
{
    InitializeComponent();

    // Existing event handlers
    this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
    this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
    this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
    this.btnConvertToPdf.Click += new System.EventHandler(this.btnConvertToPdf_Click);
    this.pnlPreview.Paint += new PaintEventHandler(this.pnlPreview_Paint);

    this.txtCustomWidth.TextChanged += (s, e) => pnlPreview.Invalidate();
    this.txtCustomHeight.TextChanged += (s, e) => pnlPreview.Invalidate();

    // ADD THIS LINE TO CONNECT YOUR PRINT ALL BUTTON
    this.PrintAllButt.Click += new System.EventHandler(this.PrintAllButt_Click); 
}

        private void Naming_Part_From_Load(object sender, EventArgs e)
        {
            txtCustomWidth.Text = DefaultLabelWidthInches.ToString();
            txtCustomHeight.Text = DefaultLabelHeightInches.ToString();

            cmbBatch.Items.Clear();
            for (int i = 1; i <= 26; i++)
            {
                cmbBatch.Items.Add("Zone " + i);
            }
        }

        private void cmbBatch_SelectedIndexChanged_1(object sender, EventArgs e)
        {
        }

        private void PrintAllButt_Click(object sender, EventArgs e)
        {
            // 1. Validation
            if (cmbBatch.SelectedItem == null)
            {
                MessageBox.Show("Please select a Zone from the dropdown.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selected = cmbBatch.SelectedItem.ToString();

            // Extract just the number from "Zone X"
            string zoneString = selected.Replace("Zone ", "").Trim();

            // 2. Collect parts directly from PartNumber_and_PartName_DATA
            var zoneParts = new List<(string name, string number)>();

            foreach (var kvp in PartNumber_and_PartName_DATA.BinToParts)
            {
                string binLocation = kvp.Key;

                // Check if the bin starts with the zone number and a dash (e.g., "1-" or "13-")
                // The dash is crucial so "Zone 1" doesn't accidentally grab "Zone 13" bins
                if (binLocation.StartsWith(zoneString + "-"))
                {
                    // Add all parts stored inside this bin to our print list
                    foreach (var part in kvp.Value)
                    {
                        zoneParts.Add((part.PartName, part.PartNumber));
                    }
                }
            }

            if (zoneParts.Count == 0)
            {
                MessageBox.Show("Selected zone has no bin data.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Clear and Populate _batchPages with pairs of parts
            _batchPages.Clear();

            for (int i = 0; i < zoneParts.Count; i += 2)
            {
                var p1 = zoneParts[i];

                // Get part 2 (if we haven't reached the end of the list)
                (string name, string number)? p2 = null;
                if (i + 1 < zoneParts.Count)
                {
                    p2 = zoneParts[i + 1];
                }

                // Add the pair to the batch
                _batchPages.Add((p1, p2));
            }

            // 4. Setup Print Document
            Size paperSize = GetTargetPaperSizeInHundredths();
            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.PaperSize = new PaperSize("CustomSticker", paperSize.Width, paperSize.Height);

                // Reset index before printing
                _batchPageIndex = 0;

                pd.PrintPage += new PrintPageEventHandler(PrintBatchPageHandler);

                // Show Preview
                using (PrintPreviewDialog previewDlg = new PrintPreviewDialog())
                {
                    previewDlg.Document = pd;
                    previewDlg.WindowState = FormWindowState.Maximized;
                    previewDlg.ShowDialog();
                }
            }
        }
        private Size GetTargetPaperSizeInHundredths()
        {
            double widthInches = DefaultLabelWidthInches;
            double heightInches = DefaultLabelHeightInches;

            double.TryParse(txtCustomWidth.Text, out widthInches);
            double.TryParse(txtCustomHeight.Text, out heightInches);

            int w = (int)(widthInches * 100);
            int h = (int)(heightInches * 100);

            if (w <= 0) w = (int)(DefaultLabelWidthInches * 100);
            if (h <= 0) h = (int)(DefaultLabelHeightInches * 100);

            return new Size(w, h);
        }

        private void PrintBatchPageHandler(object sender, PrintPageEventArgs e)
        {
            if (_batchPageIndex >= _batchPages.Count)
            {
                e.HasMorePages = false;
                return;
            }

            // Assign the pair to the active variables for rendering
            var pageData = _batchPages[_batchPageIndex];
            _activePartName = pageData.p1.name;
            _activePartNumber = pageData.p1.number;

            // Use null-coalescing to handle the case where a page has only one part
            _activePartName2 = pageData.p2?.name ?? string.Empty;
            _activePartNumber2 = pageData.p2?.number ?? string.Empty;

            PrintLabelsHandler(sender, e);

            _batchPageIndex++;
            e.HasMorePages = _batchPageIndex < _batchPages.Count;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _activePartName = txtBinLocation1.Text.Trim();
            _activePartNumber = txtBinLocation2.Text.Trim();
            _activePartName2 = PartNamebox.Text.Trim();
            _activePartNumber2 = PartNumberbox.Text.Trim();

            if (string.IsNullOrEmpty(_activePartName) && string.IsNullOrEmpty(_activePartNumber) &&
                string.IsNullOrEmpty(_activePartName2) && string.IsNullOrEmpty(_activePartNumber2))
            {
                MessageBox.Show("Please enter at least one Part Name/Part Number pair to generate.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            pnlPreview.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtBinLocation1.Clear();
            txtBinLocation2.Clear();
            PartNamebox.Clear();
            PartNumberbox.Clear();

            _activePartName = string.Empty;
            _activePartNumber = string.Empty;
            _activePartName2 = string.Empty;
            _activePartNumber2 = string.Empty;

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
            if (string.IsNullOrEmpty(_activePartName) && string.IsNullOrEmpty(_activePartNumber) &&
                string.IsNullOrEmpty(_activePartName2) && string.IsNullOrEmpty(_activePartNumber2))
            {
                MessageBox.Show("Please enter a Part Name/Number and click Generate before printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void btnConvertToPdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activePartName) && string.IsNullOrEmpty(_activePartNumber) &&
                string.IsNullOrEmpty(_activePartName2) && string.IsNullOrEmpty(_activePartNumber2))
            {
                MessageBox.Show("Please generate a Part Name/Number layout before converting to PDF.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDlg.Title = "Save Part Label as PDF";
                string fileNameSeed = !string.IsNullOrEmpty(_activePartNumber) ? _activePartNumber : _activePartNumber2;
                saveDlg.FileName = $"Part_Label_{fileNameSeed.Replace("-", "_")}";

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

        private void PrintLabelsHandler(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Fetch your custom dimensions defined in your text inputs
            Size paperSize = GetTargetPaperSizeInHundredths();

            // Check if the hardware or driver settings are forcing a portrait bounding layout
            if (e.PageBounds.Width < e.PageBounds.Height)
            {
                // 1. Pivot the graphics canvas coordinate space to align with landscape drawing
                g.TranslateTransform(e.PageBounds.Width, 0);
                g.RotateTransform(90f);

                // 2. Render out passing the inverted height and width bounds
                RenderLabelLayout(g, e.PageBounds.Height, e.PageBounds.Width, isPrinting: true);
            }
            else
            {
                // Otherwise, render cleanly inside the existing horizontal bounds
                RenderLabelLayout(g, e.PageBounds.Width, e.PageBounds.Height, isPrinting: true);
            }
        }

        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            int margin = isPrinting ? 20 : 8;
            int safeX = margin;
            int safeY = margin;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);

            // 1. Draw the outer frame border
            using (Pen borderPen = new Pen(Color.Black, isPrinting ? 4f : 2f))
            {
                g.DrawRectangle(borderPen, safeX, safeY, safeWidth, safeHeight);
            }

            // Adjust inner area
            int innerPadding = 5;
            int drawX = safeX + innerPadding;
            int drawY = safeY + innerPadding;
            int drawWidth = safeWidth - (innerPadding * 2);
            int drawHeight = safeHeight - (innerPadding * 2);

            int gap = isPrinting ? 10 : 5;
            int stripHeight = (drawHeight - gap) / 2;

            // 2. Render Strip 1 (Top)
            RenderStrip(g, _activePartName, _activePartNumber, drawX, drawY, drawWidth, stripHeight, isPrinting);

            // 3. Render Strip 2 (Bottom)
            int strip2Top = drawY + stripHeight + gap;
            RenderStrip(g, _activePartName2, _activePartNumber2, drawX, strip2Top, drawWidth, stripHeight, isPrinting);

            // 4. ADD DIVIDER: Draw a line between the two strips
            // This draws a horizontal line across the center gap
            int lineY = drawY + stripHeight + (gap / 2);
            using (Pen divPen = new Pen(Color.Black, isPrinting ? 2f : 1f))
            {
                divPen.DashStyle = DashStyle.Dash; // Optional: Makes it look like a cut line
                g.DrawLine(divPen, drawX, lineY, drawX + drawWidth, lineY);
            }
        }

        private void RenderStrip(Graphics g, string partName, string partNumber, int stripX, int stripY, int stripWidth, int stripHeight, bool isPrinting)
        {
            if (string.IsNullOrEmpty(partName) && string.IsNullOrEmpty(partNumber)) return;

            // Define columns: 75% for text, 25% for QR
            int leftColWidth = (int)(stripWidth * 0.75);
            int rightColX = stripX + leftColWidth;
            int rightColWidth = stripWidth - leftColWidth;

            // Adjust Ratio: 40% height for Name (Header), 60% for Number
            int headerHeight = (int)(stripHeight * 0.40);
            int numberAreaTop = stripY + headerHeight;
            int numberAreaHeight = stripHeight - headerHeight;

            // 1. Draw Text Backgrounds
            // Name (Header) Background
            g.FillRectangle(Brushes.Black, stripX, stripY, leftColWidth, headerHeight);

            // Number (Body) Background / Border
            using (Pen bodyPen = new Pen(Color.Black, isPrinting ? 2f : 1.5f))
            {
                g.DrawRectangle(bodyPen, stripX, numberAreaTop, leftColWidth, numberAreaHeight);
            }

            // 2. Draw Text (Autofit)
            // Part Name (White text on black background)
            DrawTextAutofit(g, partName, "Arial", FontStyle.Bold, headerHeight * 0.7f,
                            stripX + 6, stripY + 3, leftColWidth - 12, headerHeight - 6, Brushes.White);

            // Part Number (Larger: 80% of area, black text)
            DrawTextAutofit(g, partNumber, "Arial", FontStyle.Bold, numberAreaHeight * 0.8f,
                            stripX + 6, numberAreaTop + 3, leftColWidth - 12, numberAreaHeight - 6, Brushes.Black);

            // 3. Draw the QR Code
            int qrSize = Math.Min(rightColWidth - 10, stripHeight - 10);
            string qrPayload = !string.IsNullOrEmpty(partNumber) ? partNumber : partName;

            if (!string.IsNullOrEmpty(qrPayload) && qrSize > 0)
            {
                using (Bitmap qrImg = CreateQRCodeImage(qrPayload))
                {
                    if (qrImg != null)
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        // Center QR vertically in the strip
                        g.DrawImage(qrImg, rightColX + (rightColWidth - qrSize) / 2, stripY + (stripHeight - qrSize) / 2, qrSize, qrSize);
                    }
                }
            }
        }

        private void DrawTextAutofit(Graphics g, string text, string fontFamily, FontStyle style, float maxFontSize, int x, int y, int maxWidth, int maxHeight, Brush brush)
        {
            // Fix: Set high-quality rendering to prevent "curvy" text
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            float currentSize = maxFontSize;
            Font testFont = new Font(fontFamily, currentSize, style);
            SizeF size = g.MeasureString(text, testFont);

            // Shrink text until it fits
            while ((size.Width > maxWidth || size.Height > maxHeight) && currentSize > 8f)
            {
                currentSize -= 1f;
                testFont.Dispose();
                testFont = new Font(fontFamily, currentSize, style);
                size = g.MeasureString(text, testFont);
            }

            using (testFont)
            {
                // Fix: Use Math.Round to snap to whole pixels, preventing blurry/wavy edges
                float posX = (float)Math.Round(x + (maxWidth - size.Width) / 2);
                float posY = (float)Math.Round(y + (maxHeight - size.Height) / 2);

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
                                byte[] qrCodeBytes = qrCode.GetGraphic(4, Color.Black, Color.White, drawQuietZones: false);
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
                                return qrCode.GetGraphic(4, Color.Black, Color.White, drawQuietZones: false);
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

        private void ReturnButt_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.ShowDialog();
        }

        private void PartNamebox_TextChanged(object sender, EventArgs e)
        {
            _activePartName2 = PartNamebox.Text.Trim();
            pnlPreview.Invalidate();
        }

        private void PartNumberbox_TextChanged(object sender, EventArgs e)
        {
            _activePartNumber2 = PartNumberbox.Text.Trim();
            pnlPreview.Invalidate();
        }
    }
}