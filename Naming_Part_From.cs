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
        // Strip 1
        private string _activePartName = string.Empty;
        private string _activePartNumber = string.Empty;

        // Strip 2
        private string _activePartName2 = string.Empty;
        private string _activePartNumber2 = string.Empty;

        private List<(string partName, string partNumber)> _batchPages = new List<(string, string)>();
        private int _batchPageIndex = 0;

        // Swapped default label dimensions: 5.375" width x 1.625" height
        private const double DefaultLabelWidthInches = 6.375;
        private const double DefaultLabelHeightInches = 2.625;

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

            this.PrintAllButt.Click += new System.EventHandler(this.btnPrintAll_Click);
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

        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            if (cmbBatch.SelectedItem == null)
            {
                MessageBox.Show("Please select a Zone from the dropdown first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selected = cmbBatch.SelectedItem.ToString();
            int zoneNum;
            if (!int.TryParse(selected.Replace("Zone ", "").Trim(), out zoneNum) || !WarehouseData.myZones.ContainsKey(zoneNum))
            {
                MessageBox.Show("Selected zone has no bin data.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] bins = WarehouseData.myZones[zoneNum];

            _batchPages.Clear();
            foreach (string bin in bins)
            {
                var part = PartNumber_and_PartName_DATA.GetFirstPart(bin);
                if (part.HasValue)
                {
                    _batchPages.Add((part.Value.PartName, part.Value.PartNumber));
                }
            }

            if (_batchPages.Count == 0)
            {
                MessageBox.Show($"No Part Name/Number data found for any bin in Zone {zoneNum}.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
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

            string savedName = _activePartName;
            string savedNumber = _activePartNumber;
            string savedName2 = _activePartName2;
            string savedNumber2 = _activePartNumber2;

            _activePartName = _batchPages[_batchPageIndex].partName;
            _activePartNumber = _batchPages[_batchPageIndex].partNumber;
            _activePartName2 = string.Empty;
            _activePartNumber2 = string.Empty;

            PrintLabelsHandler(sender, e);

            _activePartName = savedName;
            _activePartNumber = savedNumber;
            _activePartName2 = savedName2;
            _activePartNumber2 = savedNumber2;

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

            int gap = isPrinting ? 10 : 5;
            int stripHeight = (safeHeight - gap) / 2;

            // Strip 1: top half
            RenderStrip(g, _activePartName, _activePartNumber,
                safeX, safeY, safeWidth, stripHeight, isPrinting);

            // Strip 2: bottom half
            int strip2Top = safeY + stripHeight + gap;
            RenderStrip(g, _activePartName2, _activePartNumber2,
                safeX, strip2Top, safeWidth, stripHeight, isPrinting);
        }

        private void RenderStrip(Graphics g, string partName, string partNumber,
            int stripX, int stripY, int stripWidth, int stripHeight, bool isPrinting)
        {
            if (string.IsNullOrEmpty(partName) && string.IsNullOrEmpty(partNumber)) return;

            int leftColWidth = (int)(stripWidth * 0.55);
            int rightColX = stripX + leftColWidth;
            int rightColWidth = stripWidth - leftColWidth;

            int headerHeight = (int)(stripHeight * 0.55);
            int numberAreaTop = stripY + headerHeight;
            int numberAreaHeight = stripHeight - headerHeight;

            // 1. Solid black header bar block
            using (SolidBrush headerBrush = new SolidBrush(Color.Black))
            {
                g.FillRectangle(headerBrush, stripX, stripY, leftColWidth, headerHeight);
            }

            // 2. Targeted border box around ONLY the lower part number text area
            using (Pen bodyPen = new Pen(Color.Black, isPrinting ? 2f : 1.5f))
            {
                g.DrawRectangle(bodyPen, stripX, numberAreaTop, leftColWidth, numberAreaHeight);
            }

            // 3. Render Header Text (Part Name)
            int headerPadding = 6;
            if (!string.IsNullOrEmpty(partName))
            {
                DrawTextAutofit(g, partName, "Arial", FontStyle.Bold, headerHeight * 0.7f,
                    stripX + headerPadding, stripY + (headerPadding / 2),
                    leftColWidth - (headerPadding * 2), headerHeight - headerPadding,
                    Brushes.White);
            }

            // 4. Render Body Text (Part Number)
            int textPadding = 6;
            if (!string.IsNullOrEmpty(partNumber))
            {
                DrawTextAutofit(g, partNumber, "Arial", FontStyle.Bold, numberAreaHeight * 0.7f,
                    stripX + textPadding, numberAreaTop + (textPadding / 2),
                    leftColWidth - (textPadding * 2), numberAreaHeight - textPadding,
                    Brushes.Black);
            }

            // 5. Render QR Code (Borderless and Maximized)
            int qrSize = Math.Min(rightColWidth, stripHeight);
            string qrPayload = !string.IsNullOrEmpty(partNumber) ? partNumber : partName;

            if (!string.IsNullOrEmpty(qrPayload) && qrSize > 0)
            {
                using (Bitmap qrImg = CreateQRCodeImage(qrPayload))
                {
                    if (qrImg != null)
                    {
                        int qrX = rightColX + (rightColWidth - qrSize) / 2;
                        int qrY = stripY + (stripHeight - qrSize) / 2;

                        InterpolationMode originalMode = g.InterpolationMode;
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;

                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);

                        g.InterpolationMode = originalMode;
                    }
                }
            }
        }

        private void DrawTextAutofit(Graphics g, string text, string fontFamily, FontStyle style, float maxFontSize, int x, int y, int maxWidth, int maxHeight, Brush brush = null)
        {
            if (brush == null) brush = Brushes.Black;

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

        private void PrintAllButt_Click(object sender, EventArgs e)
        {

        }
    }
}