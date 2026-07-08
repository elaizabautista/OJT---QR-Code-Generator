using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using QRCoder;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OJT___QR_Code_Generator
{
    public partial class PartName_PartNumber : Form
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

        public PartName_PartNumber()
        {
            InitializeComponent();

            this.Load += new EventHandler(this.PartName_PartNumber_Load);
            this.btnGenerate.Click += new EventHandler(this.btnGenerate_Click);
            this.btnClear.Click += new EventHandler(this.btnClear_Click);
            this.btnPrint.Click += new EventHandler(this.btnPrint_Click);
            this.btnConvertToPdf.Click += new EventHandler(this.btnConvertToPdf_Click); // "Convert to PDF" button
            this.btnPrintAll.Click += new EventHandler(this.btnPrintAll_Click);
            this.picPrintPreview.Paint += new PaintEventHandler(this.picPrintPreview_Paint);

            // txtCustomWidth / numPaperHeight are TextBoxes -> TextChanged.
            // numMargin / numQrSizePercent / numNameHeightPercent are NumericUpDowns -> ValueChanged.
            this.txtCustomWidth.TextChanged += (s, e) => picPrintPreview.Invalidate();
            this.numPaperHeight.TextChanged += (s, e) => picPrintPreview.Invalidate();
            this.numMargin.ValueChanged += (s, e) => picPrintPreview.Invalidate();
            this.numQrSizePercent.ValueChanged += (s, e) => picPrintPreview.Invalidate();
            this.numNameHeightPercent.ValueChanged += (s, e) => picPrintPreview.Invalidate();
        }

        private int GetMarginSize()
        {
            return (int)numMargin.Value;
        }

        private float GetQrSizePercent()
        {
            float percent = (float)numQrSizePercent.Value;
            if (percent <= 0 || percent > 100) percent = 32f;
            return percent / 100f;
        }

        private float GetNameBarHeightPercent()
        {
            float percent = (float)numNameHeightPercent.Value;
            if (percent <= 0 || percent > 100) percent = 40f;
            return percent / 100f;
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
            // Measured from actual Honeywell label stock (see reference photos):
            // ~3.8-4.0" wide, ~2.0" tall for the two-row (2-label) page.
            txtCustomWidth.Text = "4";
            numPaperHeight.Text = "2";

            // Defaults for the layout-tuning fields (independent of paper size above).
            // NumericUpDown controls must be set to a value already within their
            // Minimum/Maximum range as configured in the designer, or this will throw.
            numMargin.Value = 8;
            numQrSizePercent.Value = 32;
            numNameHeightPercent.Value = 40;

            // Populate the batch-match combobox with every distinct zone found in the
            // bin data. GetZoneFromBin below handles the many different bin-key formats
            // (numeric zones, letter+digit zones, named racks/annexes, etc.).
            var zones = PartNumber_and_PartName_DATA.BinToParts.Keys
                .Select(GetZoneFromBin)
                .Where(z => !string.IsNullOrEmpty(z))
                .Distinct()
                .ToList();

            // Sort so that numeric-leading zones sort numerically (2 before 10) while
            // still keeping alphabetic zone codes (A1, SCR3, WHA-CP05...) in a sensible order.
            zones.Sort(NaturalZoneCompare);

            cmbBatchMatch.Items.Clear();
            cmbBatchMatch.Items.AddRange(zones.ToArray());
            if (cmbBatchMatch.Items.Count > 0)
                cmbBatchMatch.SelectedIndex = 0;
        }

        // Ordered list of (pattern, capture group) rules used to pull a "zone" identifier
        // out of a bin key. Order matters -- more specific patterns are checked first so
        // they don't get swallowed by a more general rule further down the list.
        private static readonly (Regex Pattern, int Group)[] ZonePatterns = new[]
        {
            // Compound named zones where the zone name itself contains a hyphen.
            (new Regex(@"^(WHA-CP\d+)-", RegexOptions.IgnoreCase), 1),
            (new Regex(@"^(WHA-FM-?\d+)-", RegexOptions.IgnoreCase), 1),
            (new Regex(@"^(WHC-\d+)-", RegexOptions.IgnoreCase), 1),

            // B5 "rack" sub-zones, e.g. B5R1-AC1, B5R13-D2 (kept distinct from plain "B5").
            (new Regex(@"^(B5R\d+)-", RegexOptions.IgnoreCase), 1),

            // All Annex variants (ANX, ANX1, ANXL3-1, ANXR1-B, etc.) grouped as one zone.
            (new Regex(@"^(ANX)", RegexOptions.IgnoreCase), 1),

            // Prefix+number zones followed by a dash, e.g. SCR3-B-2, WHA02-1A-1, CNPYR1-B-1.
            (new Regex(@"^(SCR\d+)-", RegexOptions.IgnoreCase), 1),
            (new Regex(@"^(STRPNGP)\d*", RegexOptions.IgnoreCase), 1),
            (new Regex(@"^(WHA\d+)-", RegexOptions.IgnoreCase), 1),
            (new Regex(@"^(CNPYR\d+)-", RegexOptions.IgnoreCase), 1),
            (new Regex(@"^(COPPERAREA)\d*", RegexOptions.IgnoreCase), 1),
            (new Regex(@"^(FREONRACK)", RegexOptions.IgnoreCase), 1),

            // Letter+digit zones followed by a row letter, e.g. A1-A-1, B5-A-1, D6-E-2, E3-A-1.
            (new Regex(@"^([A-Z]\d+)-[A-Z]-", RegexOptions.IgnoreCase), 1),

            // Numeric zones 1-17, which have a middle row/column code, e.g. 1-1A-1, 9-10C-5.
            (new Regex(@"^(\d+)-\d+[A-Z]-", RegexOptions.IgnoreCase), 1),

            // Numeric zones 18-27, which have no middle segment, e.g. 18A-1, 27C-6.
            (new Regex(@"^(\d+)[A-Z]-\d+$", RegexOptions.IgnoreCase), 1),
        };

        // Extracts a "zone" identifier from a bin key using the pattern rules above.
        // Falls back to the text before the first dash (or the whole key, for single-token
        // bins like ANX1, STRPNGP1, COPPERAREA3, FREONRACK) if nothing else matches.
        private string GetZoneFromBin(string bin)
        {
            if (string.IsNullOrWhiteSpace(bin)) return string.Empty;

            foreach (var (pattern, group) in ZonePatterns)
            {
                var match = pattern.Match(bin);
                if (match.Success)
                    return match.Groups[group].Value.ToUpperInvariant();
            }

            int dashIndex = bin.IndexOf('-');
            return (dashIndex > 0 ? bin.Substring(0, dashIndex) : bin).ToUpperInvariant();
        }

        // Compares zone codes so that leading numeric runs sort numerically (e.g. "2" before
        // "10") while non-numeric zone codes (A1, SCR3, WHA-CP05...) still sort sensibly.
        private static int NaturalZoneCompare(string a, string b)
        {
            int ai = 0, bi = 0;
            while (ai < a.Length && bi < b.Length)
            {
                if (char.IsDigit(a[ai]) && char.IsDigit(b[bi]))
                {
                    int aStart = ai, bStart = bi;
                    while (ai < a.Length && char.IsDigit(a[ai])) ai++;
                    while (bi < b.Length && char.IsDigit(b[bi])) bi++;

                    string aNum = a.Substring(aStart, ai - aStart).TrimStart('0');
                    string bNum = b.Substring(bStart, bi - bStart).TrimStart('0');

                    if (aNum.Length != bNum.Length) return aNum.Length.CompareTo(bNum.Length);
                    int numCmp = string.CompareOrdinal(aNum, bNum);
                    if (numCmp != 0) return numCmp;
                }
                else
                {
                    int cmp = a[ai].CompareTo(b[bi]);
                    if (cmp != 0) return cmp;
                    ai++; bi++;
                }
            }
            return (a.Length - ai).CompareTo(b.Length - bi);
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
        // The paper is portrait-shaped (narrow width, taller height, since it's a vertical
        // roll feed), but the label DESIGN inside that page stays landscape (two side-by-side
        // columns per row, QR codes upright, text left-to-right). We always render straight
        // into PageBounds -- no conditional 90-degree rotation -- so Print Preview and the
        // physical printout match exactly.
        private void PrintLabelsHandler(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            RenderLabelLayout(g, e.PageBounds.Width, e.PageBounds.Height, isPrinting: true);
        }

        // Renders one batch-print page at a time (2 parts per page, matching the label's
        // 2-row layout), advancing through _batchPages until all pages are printed.
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
            int margin = GetMarginSize();
            int drawingWidth = totalWidth - (margin * 2);
            int drawingHeight = totalHeight - (margin * 2);

            using (Pen borderPen = new Pen(Color.Black, 3))
            {
                g.DrawRectangle(borderPen, margin, margin, drawingWidth, drawingHeight);
            }

            int labelHeight = drawingHeight / 2;

            int qrSectionWidth = (int)(drawingWidth * GetQrSizePercent());
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

                    int nameBarHeight = (int)(labelHeight * GetNameBarHeightPercent());
                    int numBarHeight = labelHeight - nameBarHeight;

                    g.FillRectangle(blackBrush, margin, yPos, textSectionWidth, nameBarHeight);

                    DrawTextCentered(g, name, "Arial", FontStyle.Bold, nameBarHeight * 0.5f,
                                     margin, yPos, textSectionWidth, nameBarHeight, true);

                    DrawTextCentered(g, num, "Arial", FontStyle.Bold, numBarHeight * 0.6f,
                                     margin, yPos + nameBarHeight, textSectionWidth, numBarHeight, false);

                    if (!string.IsNullOrEmpty(num))
                    {
                        using (Bitmap qrImg = CreateQRCodeImage(num))
                        {
                            if (qrImg != null)
                            {
                                int qrPadding = 2;
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
        // text fits inside BOTH the available width and height. Passing the width into
        // MeasureString makes it measure as it would word-wrap, so multi-word names wrap
        // on spaces instead of fragmenting letter-by-letter.
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

            string zoneCode = cmbBatchMatch.SelectedItem.ToString().Trim();

            // Gather every non-vacant part belonging to the selected zone
            var zoneParts = new List<(string PartName, string PartNumber)>();
            foreach (var kvp in PartNumber_and_PartName_DATA.BinToParts)
            {
                if (GetZoneFromBin(kvp.Key) != zoneCode) continue;

                foreach (var part in kvp.Value)
                {
                    if (!string.IsNullOrWhiteSpace(part.PartNumber) && part.PartNumber != "VACANT")
                        zoneParts.Add((part.PartName, part.PartNumber));
                }
            }

            if (zoneParts.Count == 0)
            {
                MessageBox.Show($"No parts found for Zone {zoneCode}.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
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