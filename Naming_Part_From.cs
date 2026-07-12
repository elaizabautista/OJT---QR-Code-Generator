using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
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
        private const double DefaultLabelWidthInches = 2.755;
        private const double DefaultLabelHeightInches = 5;


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

            // Use a HashSet to ensure we only get unique GROUP names.
            // GetZoneGroup rolls related sub-bins together (A1..A8 -> "Zone A",
            // B5/B5R1..B5R13 -> "Zone B5", WHA01..WHA15 stay individual, etc.)
            // instead of listing every raw prefix separately.
            HashSet<string> uniqueGroups = new HashSet<string>();

            foreach (string binKey in PartNumber_and_PartName_DATA.BinToParts.Keys)
            {
                uniqueGroups.Add(GetZoneGroup(binKey));
            }

            // Sort using the defined warehouse-group order (numeric zones first,
            // then lettered zones, then the named areas, then WHA01-15, then WHC)
            List<string> sortedGroups = new List<string>(uniqueGroups);
            sortedGroups.Sort((x, y) => GetGroupSortKey(x).CompareTo(GetGroupSortKey(y)));

            foreach (string group in sortedGroups)
            {
                cmbBatch.Items.Add(group);
            }
        }

        /// <summary>
        /// Maps a raw bin location (e.g. "A3-B-1", "B5R7-AC2", "ANX5", "WHA01-B-1")
        /// to the warehouse zone GROUP it belongs to (e.g. "Zone A", "Zone B5", "ANX",
        /// "Zone WHA01"). This is the single source of truth for grouping, used by
        /// both the dropdown population (Naming_Part_From_Load) and the print filter
        /// (PrintAllButt_Click), so they can never fall out of sync with each other.
        /// </summary>
        private static string GetZoneGroup(string binKey)
        {
            string prefix = binKey.Contains("-") ? binKey.Split('-')[0] : binKey;

            // Pure numeric zones, including lettered sub-areas (18A, 19B, 26C, 27C...)
            Match numMatch = Regex.Match(prefix, @"^(\d+)[A-C]?$");
            if (numMatch.Success)
                return "Zone " + numMatch.Groups[1].Value;

            if (Regex.IsMatch(prefix, @"^A[1-8]$"))
                return "Zone A";

            // B5 and its B5R1..B5R13 sub-rooms are their own group
            if (prefix == "B5" || Regex.IsMatch(prefix, @"^B5R\d+$"))
                return "Zone B5";

            if (Regex.IsMatch(prefix, @"^B[1-8]$"))
                return "Zone B";

            if (Regex.IsMatch(prefix, @"^C[1-7]$"))
                return "Zone C";

            if (Regex.IsMatch(prefix, @"^D[1-6]$"))
                return "Zone D";

            if (Regex.IsMatch(prefix, @"^E[3-6]$"))
                return "Zone E";

            // "B-STOCK" has no further suffix, so its prefix is just "B"
            if (prefix == "B")
                return "B-STOCK";

            if (prefix == "5TH BLDG" || prefix == "5THBLDG")
                return "5TH BLDG";

            if (prefix == "ANX" || Regex.IsMatch(prefix, @"^ANX\d+$") ||
                Regex.IsMatch(prefix, @"^ANXL\d+$") || Regex.IsMatch(prefix, @"^ANXR\d+$"))
                return "ANX";

            if (prefix == "CHEMROOM")
                return "CHEM";

            if (prefix == "CNPYA")
                return "CNPYA";

            if (prefix == "CNPYB")
                return "CNPYB";

            if (Regex.IsMatch(prefix, @"^CNPYR\d+$"))
                return "CNPYR";

            if (Regex.IsMatch(prefix, @"^COPPERAREA\d+$"))
                return "COPPER";

            if (prefix == "FREONRACK")
                return "FREON";

            if (Regex.IsMatch(prefix, @"^SCR\d+$"))
                return "SCR";

            if (Regex.IsMatch(prefix, @"^STRPNGP\d+$"))
                return "STRPNG";

            if (prefix == "WHA")
                return "WHA";

            Match whaMatch = Regex.Match(prefix, @"^WHA(\d+)$");
            if (whaMatch.Success)
                return "Zone WHA" + whaMatch.Groups[1].Value.PadLeft(2, '0');

            if (prefix == "WHC")
                return "WHC";

            // Fallback: anything unrecognized keeps its own raw prefix as its group,
            // so new/unexpected bin naming never silently disappears from the dropdown.
            return prefix;
        }

        /// <summary>
        /// Defines the dropdown display order: Zone 1-27, Zone A-E, B-STOCK, 5TH BLDG,
        /// ANX, Zone B5, CHEM, CNPYA, CNPYB, CNPYR, COPPER, FREON, SCR, STRPNG,
        /// WHA, Zone WHA01-15, WHC. Unrecognized groups sort to the very end.
        /// </summary>
        private static int GetGroupSortKey(string group)
        {
            Match zoneMatch = Regex.Match(group, @"^Zone (\d+)$");
            if (zoneMatch.Success)
                return 1000 + int.Parse(zoneMatch.Groups[1].Value);

            Match whaMatch = Regex.Match(group, @"^Zone WHA(\d+)$");
            if (whaMatch.Success)
                return 5000 + int.Parse(whaMatch.Groups[1].Value);

            switch (group)
            {
                case "Zone A": return 2000;
                case "Zone B": return 2001;
                case "Zone C": return 2002;
                case "Zone D": return 2003;
                case "Zone E": return 2004;
                case "B-STOCK": return 3000;
                case "5TH BLDG": return 3001;
                case "ANX": return 3002;
                case "Zone B5": return 3003;
                case "CHEM": return 3004;
                case "CNPYA": return 3005;
                case "CNPYB": return 3006;
                case "CNPYR": return 3007;
                case "COPPER": return 3008;
                case "FREON": return 3009;
                case "SCR": return 3010;
                case "STRPNG": return 3011;
                case "WHA": return 4000;
                case "WHC": return 6000;
                default: return 9999;
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

            string selectedGroup = cmbBatch.SelectedItem.ToString();

            // 2. Collect parts directly from PartNumber_and_PartName_DATA
            var zoneParts = new List<(string name, string number)>();

            foreach (var kvp in PartNumber_and_PartName_DATA.BinToParts)
            {
                string binLocation = kvp.Key;

                // Use the SAME grouping helper that built the dropdown, so a bin only
                // matches when it belongs to the exact group the user selected
                // (e.g. selecting "Zone A" now correctly pulls A1..A8, and "Zone B5"
                // pulls B5 + B5R1..B5R13, instead of each sub-bin being its own entry).
                if (GetZoneGroup(binLocation) == selectedGroup)
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

                // ROOT-CAUSE FIX: PrintPreviewDialog calls pd.Print() twice internally —
                // once to build the preview, once for the real print job. BeginPrint fires
                // at the start of both, so this guarantees the real print pass also starts
                // at page 0 instead of inheriting the exhausted index from the preview pass.
                pd.BeginPrint += (s, ev) => { _batchPageIndex = 0; };

                pd.PrintPage += new PrintPageEventHandler(PrintBatchPageHandler);

                // Show Preview
                using (PrintPreviewDialog previewDlg = new PrintPreviewDialog())
                {
                    previewDlg.WindowState = FormWindowState.Maximized;
                    previewDlg.Document = pd;
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

            var pageData = _batchPages[_batchPageIndex];
            _activePartName = pageData.p1.name;
            _activePartNumber = pageData.p1.number;
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
