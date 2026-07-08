using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using QRCoder;
using System.Collections.Generic;
using System.Linq;

namespace OJT___QR_Code_Generator
{
    public partial class PartName_PartNumber2_BACKUP_ : Form
    {
        private string _activePartName1 = string.Empty;
        private string _activePartNumber1 = string.Empty;
        private string _activePartName2 = string.Empty;
        private string _activePartNumber2 = string.Empty;

        private List<(string partName1, string partNumber1, string partName2, string partNumber2)> _batchPages
            = new List<(string, string, string, string)>();
        private int _batchPageIndex = 0;

        // Set this to the EXACT printer name as it appears in Windows "Devices and Printers"
        // (e.g. "Honeywell PC42E-T (203 dpi) - DP"). Leave blank to use whatever the
        // system default printer is (useful while developing without the physical printer).
        private const string TargetPrinterName = "";

        public PartName_PartNumber2_BACKUP_()
        {
            InitializeComponent();

            this.Load += new EventHandler(this.PartName_PartNumber_Load);
            this.btnGenerate.Click += new EventHandler(this.btnGenerate_Click);
            this.btnClear.Click += new EventHandler(this.btnClear_Click);
            this.btnPrint.Click += new EventHandler(this.btnPrint_Click);
            this.btnConvertToPdf.Click += new EventHandler(this.btnConvertToPdf_Click); // "Convert to PDF" button
            this.btnPrintAll.Click += new EventHandler(this.btnPrintAll_Click);
            this.picPrintPreview.Paint += new PaintEventHandler(this.picPrintPreview_Paint);

            this.txtCustomWidth.TextChanged += (s, e) => picPrintPreview.Invalidate();
            this.numPaperHeight.TextChanged += (s, e) => picPrintPreview.Invalidate();
        }

        public class PartLabelData
        {
            public string PartName1 { get; set; }
            public string PartNumber1 { get; set; }
            public string PartName2 { get; set; }
            public string PartNumber2 { get; set; }

            // Add this to toggle between layouts
            public bool UseHorizontalLayout { get; set; }

            public string DisplayName => $"{PartNumber1} / {PartNumber2}";
        }

        private void PartName_PartNumber_Load(object sender, EventArgs e)
        {
            // TODO: confirm real default label size in mm/inches
            txtCustomWidth.Text = "3";
            numPaperHeight.Text = "6";

            // Populate the batch-match combobox with every zone found in the bin data.
            // Bin keys come in two formats: "1-1A-1" style (zones 1-17) and "18A-1" style
            // (zones 18-26). GetZoneFromBin reads the leading digits off the key so both
            // formats resolve correctly to their zone number.
            var zones = PartNumber_and_PartName_DATA.BinToParts.Keys
                .Select(GetZoneFromBin)
                .Where(z => !string.IsNullOrEmpty(z))
                .Distinct()
                .Select(int.Parse)
                .OrderBy(z => z)
                .Select(z => $"Zone {z}")
                .ToList();

            cmbBatchMatch.Items.Clear();
            cmbBatchMatch.Items.AddRange(zones.ToArray());
            if (cmbBatchMatch.Items.Count > 0)
                cmbBatchMatch.SelectedIndex = 0;
        }

        // Extracts the zone number from a bin key. "1-1A-1" -> "1", "18A-1" -> "18",
        // "26C-3" -> "26". Reads leading digit characters only.
        private string GetZoneFromBin(string bin)
        {
            int i = 0;
            while (i < bin.Length && char.IsDigit(bin[i])) i++;
            return bin.Substring(0, i);
        }

        private Size GetTargetPaperSizeInHundredths()
        {
            double widthInches = 3.0;
            double heightInches = 2.0;

            double.TryParse(txtCustomWidth.Text, out widthInches);
            double.TryParse(numPaperHeight.Text, out heightInches);

            int w = (int)(widthInches * 100);
            int h = (int)(heightInches * 100);

            if (w <= 0) w = 300;
            if (h <= 0) h = 200;

            return new Size(w, h);
        }

        // Pins the print job to a specific printer if TargetPrinterName is set,
        // and safely falls back to the system default if that printer isn't
        // installed on this machine (e.g. while testing at home).
        private void ConfigurePrinter(PrintDocument pd)
        {
            if (string.IsNullOrEmpty(TargetPrinterName)) return;

            pd.PrinterSettings.PrinterName = TargetPrinterName;
            if (!pd.PrinterSettings.IsValid)
            {
                string fallback = new PrinterSettings().PrinterName;
                MessageBox.Show(
                    $"Printer \"{TargetPrinterName}\" was not found on this machine.\nFalling back to the default printer: {fallback}",
                    "Printer Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                pd.PrinterSettings.PrinterName = fallback;
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _activePartName1 = txtPartName1.Text.Trim();
            _activePartNumber1 = txtPartNumber1.Text.Trim();
            _activePartName2 = txtPartName2.Text.Trim();
            _activePartNumber2 = txtPartNumber2.Text.Trim();

            if (string.IsNullOrEmpty(_activePartName1) && string.IsNullOrEmpty(_activePartName2))
            {
                MessageBox.Show("Please enter at least one Part Name / Part Number pair to generate.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            picPrintPreview.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtPartName1.Clear();
            txtPartNumber1.Clear();
            txtPartName2.Clear();
            txtPartNumber2.Clear();

            _activePartName1 = string.Empty;
            _activePartNumber1 = string.Empty;
            _activePartName2 = string.Empty;
            _activePartNumber2 = string.Empty;

            picPrintPreview.Invalidate();
        }

        private void picPrintPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Panel border
            using (Pen borderPen = new Pen(Color.Black, 1))
            {
                g.DrawRectangle(borderPen, 0, 0, picPrintPreview.Width - 1, picPrintPreview.Height - 1);
            }

            Size paperSize = GetTargetPaperSizeInHundredths();
            int totalWidth = picPrintPreview.Width;
            int totalHeight = picPrintPreview.Height;

            float targetRatio = (float)paperSize.Width / paperSize.Height;
            float currentRatio = (float)totalWidth / totalHeight;

            if (currentRatio > targetRatio)
                totalWidth = (int)(totalHeight * targetRatio);
            else
                totalHeight = (int)(totalWidth / targetRatio);

            RenderLabelLayout(g, totalWidth, totalHeight, isPrinting: false);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            using (PrintDocument pd = new PrintDocument())
            {
                ConfigurePrinter(pd);

                // Use the same paper size the on-screen preview uses instead of a
                // hardcoded 450x260 -- keeps Print Preview and the design panel in sync.
                Size paperSize = GetTargetPaperSizeInHundredths();

                // IMPORTANT: do NOT also set Landscape = true here.
                // paperSize.Width > paperSize.Height already defines a landscape-shaped
                // page. Setting Landscape=true on top of that tells .NET to rotate an
                // already-rotated shape, which is what was producing the portrait
                // Print Preview you saw -- the two settings were fighting each other.
                pd.DefaultPageSettings.Landscape = false;
                pd.DefaultPageSettings.PaperSize = new PaperSize("CustomSticker", paperSize.Width, paperSize.Height);

                pd.PrintPage += new PrintPageEventHandler(PrintLabelsHandler);

                using (PrintPreviewDialog previewDlg = new PrintPreviewDialog())
                {
                    previewDlg.Document = pd;
                    previewDlg.ShowDialog();
                }
            }
        }

        private void btnConvertToPdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activePartName1) && string.IsNullOrEmpty(_activePartName2))
            {
                MessageBox.Show("Please generate a Part Name / Part Number layout before converting to PDF.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDlg.Title = "Save Part Labels as PDF";
                saveDlg.FileName = $"Part_Labels_{_activePartNumber1.Replace("-", "_")}";

                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    using (PrintDocument pd = new PrintDocument())
                    {
                        // Same fix as btnPrint_Click: paperSize.Width > Height already
                        // encodes landscape, so Landscape must stay false here too.
                        pd.DefaultPageSettings.Landscape = false;
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

        // Renders a single label (from _activePartName1/2, _activePartNumber1/2) to the printer page.
        //
        // ORIENTATION NOTE FOR THE HONEYWELL THERMAL PRINTER:
        // The paper is 3" wide x 6" tall (portrait-shaped, since it's a vertical roll feed),
        // but the label DESIGN inside that page is meant to stay landscape (two side-by-side
        // columns per row, QR codes upright, text reading left-to-right). Earlier code had a
        // "safety net" that rotated the whole drawing 90 degrees whenever PageBounds.Width 
        // PageBounds.Height -- but for this printer, width < height is the NORMAL, correct
        // page shape (narrow roll, tall feed), not a driver error. That rotation was firing
        // on every print and sideways-rotating an already-correct landscape design.
        // Fix: always render straight into PageBounds, matching the on-screen preview exactly.
        private void PrintLabelsHandler(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            RenderLabelLayout(g, e.PageBounds.Width, e.PageBounds.Height, isPrinting: true);
        }

        // Renders one batch-print page at a time (2 parts per page, matching the label's
        // 2-row layout), advancing through _batchPages until all pages are printed.
        // Uses the same straight (non-rotated) rendering as PrintLabelsHandler above.
        private void PrintBatchLabelsHandler(object sender, PrintPageEventArgs e)
        {
            if (_batchPageIndex >= _batchPages.Count)
            {
                e.HasMorePages = false;
                return;
            }

            var page = _batchPages[_batchPageIndex];
            _activePartName1 = page.partName1;
            _activePartNumber1 = page.partNumber1;
            _activePartName2 = page.partName2;
            _activePartNumber2 = page.partNumber2;

            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            RenderLabelLayout(g, e.PageBounds.Width, e.PageBounds.Height, isPrinting: true);

            _batchPageIndex++;
            e.HasMorePages = _batchPageIndex < _batchPages.Count;
        }

        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            int margin = 8;
            int drawingWidth = totalWidth - (margin * 2);
            int drawingHeight = totalHeight - (margin * 2);

            // 1. Draw Outer Border
            using (Pen borderPen = new Pen(Color.Black, 3))
            {
                g.DrawRectangle(borderPen, margin, margin, drawingWidth, drawingHeight);
            }

            int labelHeight = drawingHeight / 2;

            // QR column widened from 0.25 -> 0.32 of drawingWidth so the QR has enough
            // room to actually fill its box instead of being capped small by width.
            int qrSectionWidth = (int)(drawingWidth * 0.32);
            int textSectionWidth = drawingWidth - qrSectionWidth;

            using (Pen innerPen = new Pen(Color.Black, 2))
            using (Brush blackBrush = new SolidBrush(Color.Black))
            {
                for (int i = 0; i < 2; i++)
                {
                    int yPos = margin + (i * labelHeight);
                    string name = (i == 0) ? _activePartName1 : _activePartName2;
                    string num = (i == 0) ? _activePartNumber1 : _activePartNumber2;

                    if (i == 0) g.DrawLine(innerPen, margin, yPos + labelHeight, margin + drawingWidth, yPos + labelHeight);
                    g.DrawLine(innerPen, margin + textSectionWidth, yPos, margin + textSectionWidth, yPos + labelHeight);

                    // Layout split: 40% height for Name, 60% height for Number
                    int nameBarHeight = (int)(labelHeight * 0.4);
                    int numBarHeight = labelHeight - nameBarHeight;

                    // Black background for Part Name
                    g.FillRectangle(blackBrush, margin, yPos, textSectionWidth, nameBarHeight);

                    // Draw Part Name (smaller, autofit to its box)
                    DrawTextCentered(g, name, "Arial", FontStyle.Bold, nameBarHeight * 0.5f,
                                     margin, yPos, textSectionWidth, nameBarHeight, true);

                    // Draw Part Number (bigger, no background, autofit to its box)
                    DrawTextCentered(g, num, "Arial", FontStyle.Bold, numBarHeight * 0.6f,
                                     margin, yPos + nameBarHeight, textSectionWidth, numBarHeight, false);

                    // Draw QR Code (fit inside its box, no overlap, eats up available space)
                    if (!string.IsNullOrEmpty(num))
                    {
                        using (Bitmap qrImg = CreateQRCodeImage(num))
                        {
                            if (qrImg != null)
                            {
                                int qrPadding = 2;

                                // The QR must fit within BOTH the section's width and the
                                // label's height. Capping by the smaller of the two means
                                // it can never overflow past the divider or outer border.
                                int qrAvailableSize = Math.Min(qrSectionWidth, labelHeight) - (qrPadding * 2);

                                if (qrAvailableSize > 0)
                                {
                                    int qrX = margin + textSectionWidth + (qrSectionWidth - qrAvailableSize) / 2;
                                    int qrY = yPos + (labelHeight - qrAvailableSize) / 2;
                                    g.DrawImage(qrImg, qrX, qrY, qrAvailableSize, qrAvailableSize);
                                }

                            }
                        }
                    }
                }
            }
        }

        private void DrawText(Graphics g, string text, Font font, Brush brush, int x, int y, int w, int h)
        {
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, font, brush, new RectangleF(x, y, w, h), sf);
        }

        // Draws text centered in its box, autofitting the font size so the (word-wrapped)
        // text fits inside BOTH the available width and height. Previously this only sized
        // off height, which -- on a paper that's narrow (3") but tall (6") -- produced a
        // font far too wide for the column, forcing GDI+ to wrap it letter-by-letter.
        // Passing the width into MeasureString makes it measure as it would word-wrap,
        // so names like "PARTICULAR METAL PIECE" wrap on spaces instead of fragmenting.
        private void DrawTextCentered(Graphics g, string text, string font, FontStyle style, float maxSize, int x, int y, int w, int h, bool isWhite)
        {
            if (string.IsNullOrEmpty(text)) return;

            float fontSize = maxSize;
            Font testFont = new Font(font, fontSize, style);
            SizeF measured = g.MeasureString(text, testFont, w);

            while ((measured.Width > w || measured.Height > h) && fontSize > 6f)
            {
                testFont.Dispose();
                fontSize -= 0.5f;
                testFont = new Font(font, fontSize, style);
                measured = g.MeasureString(text, testFont, w);
            }

            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisWord
            };

            using (testFont)
            {
                g.DrawString(text, testFont, isWhite ? Brushes.White : Brushes.Black, new RectangleF(x, y, w, h), sf);
            }
        }

        private void DrawTextAutofit(Graphics g, string text, string fontFamily, FontStyle style, float maxFontSize, int x, int y, int maxWidth, int maxHeight)
        {
            float currentSize = maxFontSize;
            Font testFont = new Font(fontFamily, currentSize, style);
            SizeF size = g.MeasureString(text, testFont);

            while ((size.Width > maxWidth || size.Height > maxHeight) && currentSize > 6f)
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
                            byte[] qrCodeBytes = qrCode.GetGraphic(10); // higher module resolution, less blur when scaled up
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
                            return qrCode.GetGraphic(10, Color.Black, Color.White, true);
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
            if (cmbBatchMatch.SelectedItem == null)
            {
                MessageBox.Show("Please select a zone to batch print.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string zoneNumber = cmbBatchMatch.SelectedItem.ToString().Replace("Zone ", "").Trim();

            // Gather every non-vacant part belonging to the selected zone
            var zoneParts = new List<(string PartName, string PartNumber)>();
            foreach (var kvp in PartNumber_and_PartName_DATA.BinToParts)
            {
                if (GetZoneFromBin(kvp.Key) != zoneNumber) continue;

                foreach (var part in kvp.Value)
                {
                    if (!string.IsNullOrWhiteSpace(part.PartNumber) && part.PartNumber != "VACANT")
                        zoneParts.Add((part.PartName, part.PartNumber));
                }
            }

            if (zoneParts.Count == 0)
            {
                MessageBox.Show($"No parts found for Zone {zoneNumber}.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Pack 2 parts per page, matching the existing 2-row label layout
            _batchPages.Clear();
            for (int i = 0; i < zoneParts.Count; i += 2)
            {
                string name1 = zoneParts[i].PartName;
                string num1 = zoneParts[i].PartNumber;
                string name2 = (i + 1 < zoneParts.Count) ? zoneParts[i + 1].PartName : string.Empty;
                string num2 = (i + 1 < zoneParts.Count) ? zoneParts[i + 1].PartNumber : string.Empty;
                _batchPages.Add((name1, num1, name2, num2));
            }
            _batchPageIndex = 0;

            using (PrintDocument pd = new PrintDocument())
            {
                ConfigurePrinter(pd);

                Size paperSize = GetTargetPaperSizeInHundredths();
                pd.DefaultPageSettings.Landscape = false;
                pd.DefaultPageSettings.PaperSize = new PaperSize("CustomSticker", paperSize.Width, paperSize.Height);

                pd.PrintPage += new PrintPageEventHandler(PrintBatchLabelsHandler);

                using (PrintPreviewDialog previewDlg = new PrintPreviewDialog())
                {
                    previewDlg.Document = pd;
                    previewDlg.ShowDialog();
                }
            }
        }
    }
}