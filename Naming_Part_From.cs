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

        // Fixed default label dimensions (measured from the physical label): 4.375" x 1.625"
        private const double DefaultLabelWidthInches = 4.375;
        private const double DefaultLabelHeightInches = 1.625;

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

            // NOTE: cmbBatch's SelectedIndexChanged is already wired to cmbBatch_SelectedIndexChanged_1
            // by the Designer (from double-clicking the control) — no manual subscription needed here.

            // NOTE: btnPrintAll must exist as a Button control in the Designer, wired to btnPrintAll_Click.
            this.PrintAllButt.Click += new System.EventHandler(this.btnPrintAll_Click);
        }

        private void Naming_Part_From_Load(object sender, EventArgs e)
        {
            txtCustomWidth.Text = DefaultLabelWidthInches.ToString();
            txtCustomHeight.Text = DefaultLabelHeightInches.ToString();

            // Populate the dropdown with "Zone 1" through "Zone 26"
            cmbBatch.Items.Clear();
            for (int i = 1; i <= 26; i++)
            {
                cmbBatch.Items.Add("Zone " + i);
            }
        }

        // Selecting a zone just remembers which one is picked — the actual printing
        // happens in bulk when "Print All" is clicked (see btnPrintAll_Click below).
        private void cmbBatch_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // Intentionally left without per-item lookup now that cmbBatch shows Zones, not individual bins.
        }

        // Prints one label per bin in the selected zone, each showing that bin's Part Name/Number + QR.
        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            if (cmbBatch.SelectedItem == null)
            {
                MessageBox.Show("Please select a Zone from the dropdown first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selected = cmbBatch.SelectedItem.ToString();
            int zoneNum;
            if (!int.TryParse(selected.Replace("Zone ", "").Trim(), out zoneNum) || !WarehouseData.Zones.ContainsKey(zoneNum))
            {
                MessageBox.Show("Selected zone has no bin data.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] bins = WarehouseData.Zones[zoneNum];

            _batchPages.Clear();
            foreach (string bin in bins)
            {
                var part = PartNumber_and_PartName_DATA.GetFirstPart(bin);
                if (part.HasValue)
                {
                    _batchPages.Add((part.Value.PartName, part.Value.PartNumber));
                }
                // Bins with no matching Part Name/Number (like "5-4C-1") are silently skipped.
            }

            if (_batchPages.Count == 0)
            {
                MessageBox.Show($"No Part Name/Number data found for any bin in Zone {zoneNum}.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
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

        // 🛠️ WAREHOUSE-STYLE PART LABEL: black header bar (Part Name) on top,
        // large bold Part Number below it on the left, ONE maximized QR on the right
        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            // Safeguard border cutoff zone on physical thermal heads (Honeywell standard unprintable margins)
            int margin = isPrinting ? 20 : 8;

            int safeX = margin;
            int safeY = margin;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);

            // Left column ~65% width for text, right column ~35% width for the QR (bigger than before)
            int qrDividerX = safeX + (int)(safeWidth * 0.65);
            int leftColumnWidth = qrDividerX - safeX;

            // Header bar (Part Name) takes the top ~32% of the left column's height
            int headerHeight = (int)(safeHeight * 0.32);
            int headerBottom = safeY + headerHeight;

            // 1. Border only around the LEFT (text) column — left, top, and bottom edges.
            //    The QR compartment (right side) gets no box lines, just the divider.
            int penThickness = isPrinting ? 4 : 2;
            using (Pen blackPen = new Pen(Color.Black, penThickness))
            {
                g.DrawLine(blackPen, safeX, safeY, safeX, safeY + safeHeight);                 // left edge
                g.DrawLine(blackPen, safeX, safeY, qrDividerX, safeY);                          // top edge (text column only)
                g.DrawLine(blackPen, safeX, safeY + safeHeight, qrDividerX, safeY + safeHeight); // bottom edge (text column only)
                g.DrawLine(blackPen, qrDividerX, safeY, qrDividerX, safeY + safeHeight);         // divider between text and QR
            }

            // 2. Black header bar with the Part Name in white bold text
            using (SolidBrush headerBrush = new SolidBrush(Color.Black))
            {
                g.FillRectangle(headerBrush, safeX, safeY, leftColumnWidth, headerHeight);
            }

            int headerPadding = 8;
            if (!string.IsNullOrEmpty(_activePartName))
            {
                DrawTextAutofit(g, _activePartName, "Arial", FontStyle.Bold, headerHeight * 0.6f,
                    safeX + headerPadding, safeY + (headerPadding / 2),
                    leftColumnWidth - (headerPadding * 2), headerHeight - headerPadding,
                    Brushes.White);
            }

            // 3. Large bold Part Number filling the remaining left column space below the header
            int numberAreaTop = headerBottom;
            int numberAreaHeight = (safeY + safeHeight) - numberAreaTop;
            int textPadding = 10;

            if (!string.IsNullOrEmpty(_activePartNumber))
            {
                float maxFontCeiling = numberAreaHeight * 0.75f;
                DrawTextAutofit(g, _activePartNumber, "Arial", FontStyle.Bold, maxFontCeiling,
                    safeX + (textPadding / 2), numberAreaTop + (textPadding / 2),
                    leftColumnWidth - textPadding, numberAreaHeight - textPadding,
                    Brushes.Black);
            }

            // 4. QR code centered in the right compartment (not maximized — normal padded size),
            //    encoding the Part Number
            int rightCompartmentWidth = (safeX + safeWidth) - qrDividerX;
            int qrPadding = isPrinting ? 12 : 6;
            int qrSize = Math.Min(safeHeight, rightCompartmentWidth) - (qrPadding * 2);

            string qrPayload = $"{_activePartNumber}";

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

        private void btnGenerate_Click_1(object sender, EventArgs e)
        {

        }
    }
}