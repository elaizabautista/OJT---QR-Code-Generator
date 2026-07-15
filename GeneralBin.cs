using QRCoder;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OJT___QR_Code_Generator
{
    public partial class GeneralForm : Form
    {
        // ── Constants ─────────────────────────────────────────────────────────
        private const int LabelsPerPage = 12;
        private const int Columns = 2;
        private const int Rows = 6;

        // A4 Landscape in 1/100 inch units (297 mm × 210 mm)
        private const int A4WidthHundredths = 1169;
        private const int A4HeightHundredths = 827;

        // Margin / gap in inches
        private const double MarginLeftIn = 1.0;
        private const double MarginRightIn = 1.0;
        private const double MarginTopIn = 0.5;
        private const double MarginBottomIn = 0.5;
        private const double CenterGapIn = 0.5;

        // ── State ─────────────────────────────────────────────────────────────
        // Flat list built from the 12 manual text boxes OR from BinToParts
        private List<(string Name, string Number)> _allLabels =
            new List<(string, string)>();

        // Page counter reset on every BeginPrint
        private int _pageIndex = 0;

        // ── Constructor ───────────────────────────────────────────────────────
        public GeneralForm()
        {
            InitializeComponent();
            WireEvents();
            PopulateBatchDropdown();

            // Default mode: Labels With Data
            rdoWithData.Checked = true;

            // Default size
            txtWidth.Text = "8.27";
            txtHeight.Text = "11.69";
        }

        // ── Event wiring ──────────────────────────────────────────────────────
        private void WireEvents()
        {
            btnGenerate.Click += btnGenerate_Click;
            btnClear.Click += btnClear_Click;
            btnPrint.Click += btnPrint_Click;
            btnPrintAll.Click += btnPrintAll_Click;
            btnConvertToPdf.Click += btnConvertToPdf_Click;

            pnlPreview.Paint += pnlPreview_Paint;

            // Refresh preview whenever mode changes
            rdoBlank.CheckedChanged += (s, e) => pnlPreview.Invalidate();
            rdoWithData.CheckedChanged += (s, e) => pnlPreview.Invalidate();
        }

        // ── Batch dropdown ────────────────────────────────────────────────────
        private void PopulateBatchDropdown()
        {
            cmbBatch.Items.Clear();
            cmbBatch.Items.Add("__Select__");

            var unique = new HashSet<string>();
            foreach (string key in PartNumber_and_PartName_DATA.BinToParts.Keys)
                unique.Add(GetZoneGroup(key));

            var sorted = new List<string>(unique);
            sorted.Sort((x, y) => GetGroupSortKey(x).CompareTo(GetGroupSortKey(y)));

            foreach (string g in sorted)
                cmbBatch.Items.Add(g);

            cmbBatch.SelectedIndex = 0;
        }

        // ── Button handlers ───────────────────────────────────────────────────

        /// <summary>Generate preview from the 12 manual text boxes.</summary>
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _allLabels = CollectManualEntries();
            pnlPreview.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearAllTextBoxes();
            _allLabels.Clear();
            pnlPreview.Invalidate();
        }

        /// <summary>Print from the 12 manual text boxes.</summary>
        private void btnPrint_Click(object sender, EventArgs e)
        {
            _allLabels = CollectManualEntries();

            if (_allLabels.Count == 0 && !rdoBlank.Checked)
            {
                MessageBox.Show("Please enter at least one Part Name or Part Number, or switch to Blank Template mode.",
                    "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (PrintDocument pd = BuildPrintDocument())
            using (PrintPreviewDialog dlg = new PrintPreviewDialog())
            {
                dlg.Document = pd;
                dlg.WindowState = FormWindowState.Maximized;
                dlg.ShowDialog();
            }
        }

        /// <summary>Print All: loads every label from BinToParts for the selected zone.</summary>
        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            if (!LoadZoneLabels()) return;

            using (PrintDocument pd = BuildPrintDocument())
            using (PrintPreviewDialog dlg = new PrintPreviewDialog())
            {
                dlg.Document = pd;
                dlg.WindowState = FormWindowState.Maximized;
                dlg.ShowDialog();
            }
        }

        private void btnConvertToPdf_Click(object sender, EventArgs e)
        {
            // Decide data source: zone batch if selected, otherwise manual boxes
            if (cmbBatch.SelectedItem != null &&
                cmbBatch.SelectedItem.ToString() != "__Select__")
            {
                if (!LoadZoneLabels()) return;
            }
            else
            {
                _allLabels = CollectManualEntries();
            }

            if (_allLabels.Count == 0 && !rdoBlank.Checked)
            {
                MessageBox.Show("No data to export. Enter labels or select a zone, or switch to Blank Template mode.",
                    "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDlg.Title = "Save General Bin Labels as PDF";
                saveDlg.FileName = "GeneralBin_Labels";

                if (saveDlg.ShowDialog() != DialogResult.OK) return;

                using (PrintDocument pd = BuildPrintDocument())
                {
                    pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                    pd.PrinterSettings.PrintToFile = true;
                    pd.PrinterSettings.PrintFileName = saveDlg.FileName;

                    try
                    {
                        pd.Print();
                        MessageBox.Show("PDF saved successfully!",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not save PDF: {ex.Message}",
                            "PDF Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ── Preview panel paint ───────────────────────────────────────────────
        private void pnlPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int pw = pnlPreview.Width;
            int ph = pnlPreview.Height;

            // Maintain A4 Landscape aspect ratio inside the panel
            float targetRatio = (float)A4WidthHundredths / A4HeightHundredths;
            float currentRatio = (float)pw / ph;

            if (currentRatio > targetRatio)
                pw = (int)(ph * targetRatio);
            else
                ph = (int)(pw / targetRatio);

            // Use whatever is already loaded in _allLabels for the preview
            RenderA4Page(g, pw, ph, startIndex: 0, isPrinting: false);
        }

        // ── PrintDocument factory ─────────────────────────────────────────────
        private PrintDocument BuildPrintDocument(string printerName = null)
        {
            var pd = new PrintDocument();

            if (!string.IsNullOrEmpty(printerName))
                pd.PrinterSettings.PrinterName = printerName;

            PaperSize a4 = null;
            foreach (PaperSize ps in pd.PrinterSettings.PaperSizes)
            {
                if (ps.PaperName.IndexOf("A4", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    a4 = ps;
                    break;
                }
            }

            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.PaperSize = a4 ??
                new PaperSize("A4", A4WidthHundredths, A4HeightHundredths);

            pd.BeginPrint += (s, ev) => { _pageIndex = 0; };
            pd.PrintPage += PrintA4PageHandler;
            return pd;
        }

        private void PrintA4PageHandler(object sender, PrintPageEventArgs e)
        {
            int startIndex = _pageIndex * LabelsPerPage;
            if (rdoBlank.Checked)
            {
                if (_pageIndex > 0) { e.HasMorePages = false; return; }
                startIndex = 0;
            }
            else if (startIndex >= _allLabels.Count)
            {
                e.HasMorePages = false;
                return;
            }

            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Check the ACTUAL bounds reported at print time — don't assume either way
            if (e.PageBounds.Width < e.PageBounds.Height)
            {
                g.TranslateTransform(e.PageBounds.Width, 0);
                g.RotateTransform(90f);
                RenderA4Page(g, e.PageBounds.Height, e.PageBounds.Width, startIndex, isPrinting: true);
            }
            else
            {
                RenderA4Page(g, e.PageBounds.Width, e.PageBounds.Height, startIndex, isPrinting: true);
            }

            _pageIndex++;
            e.HasMorePages = rdoBlank.Checked ? false : (_pageIndex * LabelsPerPage) < _allLabels.Count;
        }
        // ── Page rendering engine ─────────────────────────────────────────────

        /// <summary>
        /// Renders one full A4 Landscape page of 12 label slots.
        /// Depending on the selected mode, each slot is either blank or filled.
        /// A 0.3" vertical gap is inserted between consecutive labels in each column.
        /// The whole grid is centered within the printable area to compensate for
        /// integer-division rounding in labelHeight/colWidth.
        /// </summary>
        private void RenderA4Page(Graphics g, int pageWidth, int pageHeight,
                                   int startIndex, bool isPrinting)
        {
            // Scale inch-based margins to device pixels
            double sx = (double)pageWidth / A4WidthHundredths;
            double sy = (double)pageHeight / A4HeightHundredths;

            int marginLeft = (int)(MarginLeftIn * 100 * sx);
            int marginRight = (int)(MarginRightIn * 100 * sx);
            int marginTop = (int)(MarginTopIn * 100 * sy);
            int marginBottom = (int)(MarginBottomIn * 100 * sy);
            int centerGap = (int)(CenterGapIn * 100 * sx);

            int areaLeft = marginLeft;
            int areaTop = marginTop;
            int areaWidth = pageWidth - marginLeft - marginRight;
            int areaHeight = pageHeight - marginTop - marginBottom;

            int colWidth = (areaWidth - centerGap) / 2;

            // Vertical gap between labels (0.3 inch), scaled to device pixels
            const double LabelGapIn = 0.3;
            int labelGap = (int)(LabelGapIn * 100 * sy);

            int totalGapHeight = labelGap * (Rows - 1);
            int labelHeight = (areaHeight - totalGapHeight) / Rows;

            // ── NEW: center the grid within the printable area ──
            // Integer division above always leaves a small remainder (a few px)
            // that was previously pushed entirely to the right/bottom of the
            // printable area. We compute that leftover and split it evenly so
            // the grid sits dead-center between the margins.
            int contentWidth = colWidth * 2 + centerGap;
            int contentHeight = labelHeight * Rows + totalGapHeight;

            int horizontalOffset = (areaWidth - contentWidth) / 2;
            int verticalOffset = (areaHeight - contentHeight) / 2;

            int gridLeft = areaLeft + horizontalOffset;
            int gridTop = areaTop + verticalOffset;

            bool isBlank = rdoBlank.Checked;

            // Outer page border: removed
            // Column separator line: removed

            // ── Draw each of the 12 label slots ──
            for (int slot = 0; slot < LabelsPerPage; slot++)
            {
                int row = slot % Rows;   // 0–5  (fill top-to-bottom in each column)
                int col = slot / Rows;   // 0 or 1

                // NEW: positions now originate from gridLeft/gridTop instead of areaLeft/areaTop
                int labelX = gridLeft + col * (colWidth + centerGap);
                int labelY = gridTop + row * (labelHeight + labelGap);

                if (isBlank)
                {
                    DrawBlankSlot(g, labelX, labelY, colWidth, labelHeight, isPrinting);
                }
                else
                {
                    int dataIndex = startIndex + slot;
                    string name = (dataIndex < _allLabels.Count) ? _allLabels[dataIndex].Name : string.Empty;
                    string number = (dataIndex < _allLabels.Count) ? _allLabels[dataIndex].Number : string.Empty;

                    RenderStrip(g, name, number, labelX, labelY, colWidth, labelHeight, isPrinting);
                }

                // Dashed cut guide between rows: removed (0.3" gap already separates labels)
            }
        }

        // ── Blank slot renderer ───────────────────────────────────────────────

        /// <summary>
        /// Draws an empty label slot as a plain bordered rectangle only.
        /// No placeholder text, no QR box, no inner guides.
        /// </summary>
        private void DrawBlankSlot(Graphics g, int x, int y, int width, int height, bool isPrinting)
        {
            // Outer cell border only
            using (Pen border = new Pen(Color.Black, isPrinting ? 1.5f : 1f))
            {
                g.DrawRectangle(border, x, y, width - 1, height - 1);
            }
        }

        // ── Label renderer (identical logic to Naming_Part_From) ──────────────

        private void RenderStrip(Graphics g, string partName, string partNumber,
                                  int stripX, int stripY, int stripWidth, int stripHeight,
                                  bool isPrinting)
        {
            if (string.IsNullOrEmpty(partName) && string.IsNullOrEmpty(partNumber)) return;

            // Outer cell border
            using (Pen border = new Pen(Color.Black, isPrinting ? 2f : 1f))
            {
                g.DrawRectangle(border, stripX, stripY, stripWidth - 1, stripHeight - 1);
            }

            int pad = isPrinting ? 4 : 2;
            int innerX = stripX + pad;
            int innerY = stripY + pad;
            int innerW = stripWidth - pad * 2;
            int innerH = stripHeight - pad * 2;

            // 75 % text | 25 % QR
            int leftColWidth = (int)(innerW * 0.75);
            int rightColX = innerX + leftColWidth;
            int rightColWidth = innerW - leftColWidth;

            // 40 % header | 60 % body
            int headerHeight = (int)(innerH * 0.40);
            int numberAreaTop = innerY + headerHeight;
            int numberAreaHeight = innerH - headerHeight;

            // Part Name background (black)
            g.FillRectangle(Brushes.Black, innerX, innerY, leftColWidth, headerHeight);

            // Part Number border
            using (Pen bodyPen = new Pen(Color.Black, isPrinting ? 1.5f : 1f))
            {
                g.DrawRectangle(bodyPen, innerX, numberAreaTop, leftColWidth, numberAreaHeight);
            }

            // Part Name text (white)
            DrawTextAutofit(g, partName, "Arial", FontStyle.Bold, headerHeight * 0.7f,
                            innerX + 4, innerY + 2, leftColWidth - 8, headerHeight - 4, Brushes.White);

            // Part Number text (black)
            DrawTextAutofit(g, partNumber, "Arial", FontStyle.Bold, numberAreaHeight * 0.8f,
                            innerX + 4, numberAreaTop + 2, leftColWidth - 8, numberAreaHeight - 4, Brushes.Black);

            // QR Code
            int qrSize = Math.Min(rightColWidth - 6, innerH - 6);
            string qrPayload = !string.IsNullOrEmpty(partNumber) ? partNumber : partName;

            if (!string.IsNullOrEmpty(qrPayload) && qrSize > 0)
            {
                using (Bitmap qrImg = CreateQRCodeImage(qrPayload))
                {
                    if (qrImg != null)
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        int qrX = rightColX + (rightColWidth - qrSize) / 2;
                        int qrY = innerY + (innerH - qrSize) / 2;
                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);
                    }
                }
            }
        }

        // ── Text helper ───────────────────────────────────────────────────────

        private void DrawTextAutofit(Graphics g, string text, string fontFamily,
                                      FontStyle style, float maxFontSize,
                                      int x, int y, int maxWidth, int maxHeight, Brush brush)
        {
            if (string.IsNullOrEmpty(text)) return;

            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            float currentSize = Math.Max(maxFontSize, 4f);
            Font testFont = new Font(fontFamily, currentSize, style);
            SizeF size = g.MeasureString(text, testFont);

            while ((size.Width > maxWidth || size.Height > maxHeight) && currentSize > 3f)
            {
                currentSize -= 0.5f;
                testFont.Dispose();
                testFont = new Font(fontFamily, currentSize, style);
                size = g.MeasureString(text, testFont);
            }

            using (testFont)
            {
                float posX = (float)Math.Round(x + (maxWidth - size.Width) / 2);
                float posY = (float)Math.Round(y + (maxHeight - size.Height) / 2);
                g.DrawString(text, testFont, brush, posX, posY);
            }
        }

        // ── QR helper ─────────────────────────────────────────────────────────

        private Bitmap CreateQRCodeImage(string text)
        {
            try
            {
                using (QRCodeGenerator qrGen = new QRCodeGenerator())
                using (QRCodeData qrData = qrGen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
                {
                    try
                    {
                        using (PngByteQRCode qrCode = new PngByteQRCode(qrData))
                        {
                            byte[] bytes = qrCode.GetGraphic(4, Color.Black, Color.White, drawQuietZones: false);
                            using (var ms = new System.IO.MemoryStream(bytes))
                                return new Bitmap(ms);
                        }
                    }
                    catch
                    {
                        using (QRCode qrCode = new QRCode(qrData))
                            return qrCode.GetGraphic(4, Color.Black, Color.White, drawQuietZones: false);
                    }
                }
            }
            catch { return null; }
        }

        // ── Data helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Reads the 12 manual Part Name / Part Number text boxes
        /// and returns only the pairs that have at least one non-empty field.
        /// </summary>
        private List<(string Name, string Number)> CollectManualEntries()
        {
            var list = new List<(string, string)>();

            var names = new[] { txtPartName1,  txtPartName2,  txtPartName3,
                                   txtPartName4,  txtPartName5,  txtPartName6,
                                   txtPartName7,  txtPartName8,  txtPartName9,
                                   txtPartName10, txtPartName11, txtPartName12 };

            var numbers = new[] { txtPartNumber1,  txtPartNumber2,  txtPartNumber3,
                                   txtPartNumber4,  txtPartNumber5,  txtPartNumber6,
                                   txtPartNumber7,  txtPartNumber8,  txtPartNumber9,
                                   txtPartNumber10, txtPartNumber11, txtPartNumber12 };

            for (int i = 0; i < 12; i++)
            {
                string n = names[i].Text.Trim();
                string no = numbers[i].Text.Trim();
                if (!string.IsNullOrEmpty(n) || !string.IsNullOrEmpty(no))
                    list.Add((n, no));
            }

            return list;
        }

        /// <summary>
        /// Loads all labels from BinToParts for the zone selected in cmbBatch.
        /// Returns false with a warning when nothing is found.
        /// </summary>
        private bool LoadZoneLabels()
        {
            if (cmbBatch.SelectedItem == null ||
                cmbBatch.SelectedItem.ToString() == "__Select__")
            {
                MessageBox.Show("Please select a Zone from the Batch dropdown.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string selectedGroup = cmbBatch.SelectedItem.ToString();
            _allLabels.Clear();

            foreach (var kvp in PartNumber_and_PartName_DATA.BinToParts)
            {
                if (GetZoneGroup(kvp.Key) == selectedGroup)
                {
                    foreach (var part in kvp.Value)
                        _allLabels.Add((part.PartName, part.PartNumber));
                }
            }

            if (_allLabels.Count == 0)
            {
                MessageBox.Show("Selected zone has no bin data.",
                    "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ClearAllTextBoxes()
        {
            var controls = new[]
            {
                txtPartName1,  txtPartNumber1,
                txtPartName2,  txtPartNumber2,
                txtPartName3,  txtPartNumber3,
                txtPartName4,  txtPartNumber4,
                txtPartName5,  txtPartNumber5,
                txtPartName6,  txtPartNumber6,
                txtPartName7,  txtPartNumber7,
                txtPartName8,  txtPartNumber8,
                txtPartName9,  txtPartNumber9,
                txtPartName10, txtPartNumber10,
                txtPartName11, txtPartNumber11,
                txtPartName12, txtPartNumber12
            };

            foreach (var tb in controls)
                tb.Clear();
        }

        // ── Zone grouping (shared with Naming_Part_From, kept in sync) ────────

        private static string GetZoneGroup(string binKey)
        {
            string prefix = binKey.Contains("-") ? binKey.Split('-')[0] : binKey;

            Match numMatch = Regex.Match(prefix, @"^(\d+)[A-C]?$");
            if (numMatch.Success) return "Zone " + numMatch.Groups[1].Value;

            if (Regex.IsMatch(prefix, @"^A[1-8]$")) return "Zone A";
            if (prefix == "B5" || Regex.IsMatch(prefix, @"^B5R\d+$")) return "Zone B5";
            if (Regex.IsMatch(prefix, @"^B[1-8]$")) return "Zone B";
            if (Regex.IsMatch(prefix, @"^C[1-7]$")) return "Zone C";
            if (Regex.IsMatch(prefix, @"^D[1-6]$")) return "Zone D";
            if (Regex.IsMatch(prefix, @"^E[3-6]$")) return "Zone E";
            if (prefix == "B") return "B-STOCK";
            if (prefix == "5TH BLDG" || prefix == "5THBLDG") return "5TH BLDG";
            if (prefix == "ANX"
                || Regex.IsMatch(prefix, @"^ANX\d+$")
                || Regex.IsMatch(prefix, @"^ANXL\d+$")
                || Regex.IsMatch(prefix, @"^ANXR\d+$")) return "ANX";
            if (prefix == "CHEMROOM") return "CHEM";
            if (prefix == "CNPYA") return "CNPYA";
            if (prefix == "CNPYB") return "CNPYB";
            if (Regex.IsMatch(prefix, @"^CNPYR\d+$")) return "CNPYR";
            if (Regex.IsMatch(prefix, @"^COPPERAREA\d+$")) return "COPPER";
            if (prefix == "FREONRACK") return "FREON";
            if (Regex.IsMatch(prefix, @"^SCR\d+$")) return "SCR";
            if (Regex.IsMatch(prefix, @"^STRPNGP\d+$")) return "STRPNG";
            if (prefix == "WHA") return "WHA";

            Match whaMatch = Regex.Match(prefix, @"^WHA(\d+)$");
            if (whaMatch.Success)
                return "Zone WHA" + whaMatch.Groups[1].Value.PadLeft(2, '0');

            if (prefix == "WHC") return "WHC";

            return prefix;
        }

        private static int GetGroupSortKey(string group)
        {
            Match zm = Regex.Match(group, @"^Zone (\d+)$");
            if (zm.Success) return 1000 + int.Parse(zm.Groups[1].Value);

            Match wm = Regex.Match(group, @"^Zone WHA(\d+)$");
            if (wm.Success) return 5000 + int.Parse(wm.Groups[1].Value);

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

        private void GeneralForm_Load(object sender, EventArgs e)
        {

        }
    }
}