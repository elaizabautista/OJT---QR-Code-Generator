using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using QRCoder;

namespace OJT___QR_Code_Generator
{
    public partial class Floor_Bin : Form
    {
        private string _activeNumber = string.Empty;

        // Holds the list of codes queued up for a "Print All" batch job,
        // plus the index of which one is currently being printed
        private List<string> _batchQueue = new List<string>();
        private int _batchIndex = 0;

        public Floor_Bin()
        {
            InitializeComponent();

            this.btnGenerate.Click += new EventHandler(this.btnGenerate_Click);
            this.btnClear.Click += new EventHandler(this.btnClear_Click);
            this.btnPrint.Click += new EventHandler(this.btnPrint_Click);
            this.btnConvertToPdf.Click += new EventHandler(this.btnConvertToPdf_Click);
            this.btnPrintAll.Click += new EventHandler(this.btnPrintAll_Click);
            this.pnlPreview.Paint += new PaintEventHandler(this.pnlPreview_Paint);

            this.txtCustomWidth.TextChanged += (s, e) => pnlPreview.Invalidate();
            this.txtCustomHeight.TextChanged += (s, e) => pnlPreview.Invalidate();

            // Connect the ComboBox change event
            this.cmbBatch.SelectedIndexChanged += new EventHandler(this.cmbWhaZones_SelectedIndexChanged);
        }

        private void Floor_Bin_Load(object sender, EventArgs e)
        {
            txtCustomWidth.Text = "4";
            txtCustomHeight.Text = "6";

            // Populate the ComboBox with options WHA01 to WHA15
            cmbBatch.Items.Clear();
            for (int i = 1; i <= 15; i++)
            {
                cmbBatch.Items.Add($"WHA{i:D2}");
            }
        }

        private void cmbWhaZones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBatch.SelectedItem == null) return;

            string selectedZonePrefix = cmbBatch.SelectedItem.ToString(); // e.g., "WHA01"

            // Query the WHA_Data dictionary for any matches starting with our prefix
            foreach (var key in WHA_Data.BinToPartMapping.Keys)
            {
                if (key.StartsWith(selectedZonePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    // Update textbox with the associated value/part number found
                    txtNumber.Text = WHA_Data.BinToPartMapping[key];

                    // Automatically trigger label preview updating
                    _activeNumber = txtNumber.Text.Trim();
                    pnlPreview.Invalidate();
                    break;
                }
            }
        }

        private Size GetTargetPaperSizeInHundredths()
        {
            double widthInches = 4.0;
            double heightInches = 6.0;

            double.TryParse(txtCustomWidth.Text, out widthInches);
            double.TryParse(txtCustomHeight.Text, out heightInches);

            int w = (int)(widthInches * 100);
            int h = (int)(heightInches * 100);

            if (w <= 0) w = 400;
            if (h <= 0) h = 600;

            return new Size(w, h);
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _activeNumber = txtNumber.Text.Trim();

            if (string.IsNullOrEmpty(_activeNumber))
            {
                MessageBox.Show("Please enter a Number to generate.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            pnlPreview.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtNumber.Clear();
            cmbBatch.SelectedIndex = -1; // Reset combobox selection
            _activeNumber = string.Empty;
            pnlPreview.Invalidate();
        }

        private void pnlPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Size paperSize = GetTargetPaperSizeInHundredths();

            int paperWidth = Math.Max(paperSize.Width, paperSize.Height);
            int paperHeight = Math.Min(paperSize.Width, paperSize.Height);

            int totalWidth = pnlPreview.Width;
            int totalHeight = pnlPreview.Height;

            float targetRatio = (float)paperWidth / paperHeight;
            float currentRatio = (float)totalWidth / totalHeight;

            if (currentRatio > targetRatio)
                totalWidth = (int)(totalHeight * targetRatio);
            else
                totalHeight = (int)(totalWidth / targetRatio);

            int offsetX = (pnlPreview.Width - totalWidth) / 2;
            int offsetY = (pnlPreview.Height - totalHeight) / 2;

            g.TranslateTransform(offsetX, offsetY);

            RenderFloorBinLabel(g, totalWidth, totalHeight, isPrinting: false);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activeNumber))
            {
                MessageBox.Show("Please enter a Number and click Generate before printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Landscape = true;
                pd.DefaultPageSettings.PaperSize = new PaperSize("FloorBinSticker", paperSize.Width, paperSize.Height);
                pd.PrintPage += new PrintPageEventHandler(PrintFloorBinHandler);

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
            if (string.IsNullOrEmpty(_activeNumber))
            {
                MessageBox.Show("Please generate a Floor Bin label before converting to PDF.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDlg.Title = "Save Floor Bin Label as PDF";
                saveDlg.FileName = $"FloorBin_{_activeNumber.Replace("-", "_")}";

                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    using (PrintDocument pd = new PrintDocument())
                    {
                        pd.DefaultPageSettings.Landscape = true;
                        pd.DefaultPageSettings.PaperSize = new PaperSize("FloorBinSticker", paperSize.Width, paperSize.Height);
                        pd.OriginAtMargins = false;

                        pd.PrintPage += new PrintPageEventHandler(PrintFloorBinHandler);
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

        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            if (cmbBatch.SelectedItem == null)
            {
                MessageBox.Show("Please select a WHA zone from the Batch dropdown before printing all.", "Batch Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedZonePrefix = cmbBatch.SelectedItem.ToString(); // e.g., "WHA01"

            // Gather every bin code belonging to the selected zone
            _batchQueue = WHA_Data.BinToPartMapping
                .Where(kvp => kvp.Key.StartsWith(selectedZonePrefix, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Value)
                .ToList();

            if (_batchQueue.Count == 0)
            {
                MessageBox.Show($"No bin codes found for {selectedZonePrefix}.", "Batch Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _batchIndex = 0;

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Landscape = true;
                pd.DefaultPageSettings.PaperSize = new PaperSize("FloorBinSticker", paperSize.Width, paperSize.Height);
                pd.PrintPage += new PrintPageEventHandler(PrintAllFloorBinHandler);

                using (PrintPreviewDialog previewDlg = new PrintPreviewDialog())
                {
                    previewDlg.Document = pd;
                    previewDlg.WindowState = FormWindowState.Maximized;
                    previewDlg.ShowDialog();
                }
            }
        }

        private void PrintFloorBinHandler(object sender, PrintPageEventArgs e)
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

            RenderFloorBinLabel(g, printableWidth, printableHeight, isPrinting: true);
        }

        // Multi-page handler for "Print All" - prints one label per page,
        // advancing through _batchQueue and setting HasMorePages until done
        private void PrintAllFloorBinHandler(object sender, PrintPageEventArgs e)
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

            // Temporarily swap the active number to whichever bin code is next in the queue,
            // reusing the exact same render method as single-label printing
            string previousActiveNumber = _activeNumber;
            _activeNumber = _batchQueue[_batchIndex];

            RenderFloorBinLabel(g, printableWidth, printableHeight, isPrinting: true);

            _activeNumber = previousActiveNumber;

            _batchIndex++;
            e.HasMorePages = _batchIndex < _batchQueue.Count;
        }

        private void RenderFloorBinLabel(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            // Bigger margin = more white space outside the border, protects against
            // print/cut misalignment cutting into the border or content
            int margin = isPrinting ? 14 : 7;

            int safeX = margin;
            int safeY = margin;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);

            // Border around the whole label - the only line drawn, no divider
            int penThickness = isPrinting ? 4 : 2;
            using (Pen borderPen = new Pen(Color.Black, penThickness))
            {
                g.DrawRectangle(borderPen, safeX, safeY, safeWidth, safeHeight);
            }

            if (string.IsNullOrEmpty(_activeNumber))
                return;

            int gap = (int)(safeHeight * 0.04f);

            int textHeight = (int)(safeHeight * 0.19f);
            float maxFontCeiling = textHeight;

            int qrSize = (int)((safeHeight - gap - textHeight) * 0.90f);
            qrSize = Math.Min(qrSize, safeWidth);

            int moveTextUpPixels = 10;

            int contentHeight = qrSize + gap + textHeight;
            int blockStartY = safeY + (safeHeight - contentHeight) / 2;

            int qrX = safeX + (safeWidth - qrSize) / 2;
            int qrY = blockStartY;

            using (Bitmap qrImg = CreateQRCodeImage(_activeNumber))
            {
                if (qrImg != null)
                {
                    var oldInterpolation = g.InterpolationMode;
                    var oldPixelOffset = g.PixelOffsetMode;

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                    g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);

                    g.InterpolationMode = oldInterpolation;
                    g.PixelOffsetMode = oldPixelOffset;
                }
            }

            int textY = qrY + qrSize + gap;
            textY -= moveTextUpPixels;

            DrawTextAutofit(g, _activeNumber, "Arial", FontStyle.Bold, maxFontCeiling, safeX, textY, safeWidth, textHeight);
        }

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
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
                {
                    try
                    {
                        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                        {
                            byte[] qrCodeBytes = qrCode.GetGraphic(20);
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
                            return qrCode.GetGraphic(20, Color.Black, Color.White, true);
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