using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ExcelDataReader;

namespace OJT___QR_Code_Generator
{
    public partial class Naming_Part_From : Form
    {
        // 1. Updated version of the list for batch pages:
        private List<((string name, string number) p1, (string name, string number)? p2)> _batchPages =
            new List<((string, string), (string, string)?)>();

        // Master dictionary to hold both Hardcoded + Excel Uploaded data, grouped by Zone
        private Dictionary<string, List<(string name, string number)>> _masterGroupedData =
            new Dictionary<string, List<(string, string)>>();

        // 2. Existing string variables:
        private string _activePartName = string.Empty;
        private string _activePartNumber = string.Empty;
        private string _activePartName2 = string.Empty;
        private string _activePartNumber2 = string.Empty;

        // 3. Index and constants:
        private int _batchPageIndex = 0;
        private const double DefaultLabelWidthInches = 2.755;
        private const double DefaultLabelHeightInches = 5;


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

            this.PrintAllButt.Click += new System.EventHandler(this.PrintAllButt_Click);
        }

        private void CenterPanel()
        {
            panel4.Left = (this.ClientSize.Width - panel4.Width) / 2;
            panel4.Top = (this.ClientSize.Height - panel4.Height) / 2;
        }

        private void Naming_Part_From_Load(object sender, EventArgs e)
        {
            CenterPanel();
            this.Resize += (s, e) => CenterPanel();

            txtCustomWidth.Text = DefaultLabelWidthInches.ToString();
            txtCustomHeight.Text = DefaultLabelHeightInches.ToString();

            // Seed default data from the hardcoded PartNumber_and_PartName_DATA
            _masterGroupedData.Clear();
            foreach (var kvp in PartNumber_and_PartName_DATA.BinToParts)
            {
                string group = GetZoneGroup(kvp.Key);
                if (!_masterGroupedData.ContainsKey(group))
                {
                    _masterGroupedData[group] = new List<(string, string)>();
                }

                foreach (var part in kvp.Value)
                {
                    // Assuming your part object has PartName and PartNumber properties
                    _masterGroupedData[group].Add((part.PartName, part.PartNumber));
                }
            }

            RefreshBatchDropdowns();
        }

        private void RefreshBatchDropdowns(List<string> newUploadZones = null)
        {
            // Update Old/Master Batch Dropdown
            List<string> allGroups = _masterGroupedData.Keys.ToList();
            allGroups.Sort((x, y) => GetGroupSortKey(x).CompareTo(GetGroupSortKey(y)));

            string previousBatchSelection = cmbBatch.SelectedItem?.ToString();
            cmbBatch.DataSource = null;
            cmbBatch.Items.Clear();
            cmbBatch.DataSource = allGroups;

            if (!string.IsNullOrEmpty(previousBatchSelection) && allGroups.Contains(previousBatchSelection))
                cmbBatch.SelectedItem = previousBatchSelection;
            else if (cmbBatch.Items.Count > 0)
                cmbBatch.SelectedIndex = 0;

            // Update New Batch Dropdown if applicable
            if (newUploadZones != null && newUploadZones.Count > 0)
            {
                newUploadZones.Sort((x, y) => GetGroupSortKey(x).CompareTo(GetGroupSortKey(y)));
                cmbNewBatch2.DataSource = null;
                cmbNewBatch2.Items.Clear();
                cmbNewBatch2.DataSource = newUploadZones;

                cmbNewBatch2.SelectedIndex = 0;
                cmbBatch.SelectedIndex = -1; // Deselect old batch to focus on the newly uploaded one
            }
        }

        /// <summary>
        /// Maps a raw bin location to the warehouse zone GROUP it belongs to.
        /// </summary>
        private static string GetZoneGroup(string binKey)
        {
            if (string.IsNullOrEmpty(binKey)) return "Unassigned";

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

            if (prefix == "ANX" || Regex.IsMatch(prefix, @"^ANX\d+$") ||
                Regex.IsMatch(prefix, @"^ANXL\d+$") || Regex.IsMatch(prefix, @"^ANXR\d+$"))
                return "ANX";

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
            if (whaMatch.Success) return "Zone WHA" + whaMatch.Groups[1].Value.PadLeft(2, '0');

            if (prefix == "WHC") return "WHC";

            return prefix;
        }

        private static int GetGroupSortKey(string group)
        {
            Match zoneMatch = Regex.Match(group, @"^Zone (\d+)$");
            if (zoneMatch.Success) return 1000 + int.Parse(zoneMatch.Groups[1].Value);

            Match whaMatch = Regex.Match(group, @"^Zone WHA(\d+)$");
            if (whaMatch.Success) return 5000 + int.Parse(whaMatch.Groups[1].Value);

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
            // Clear New Batch if Old Batch is selected
            if (cmbBatch.SelectedItem != null && cmbNewBatch2.Items.Count > 0)
            {
                cmbNewBatch2.SelectedIndex = -1;
            }
        }

        private void cmbNewBatch2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear Old Batch if New Batch is selected
            if (cmbNewBatch2.SelectedItem != null && cmbBatch.Items.Count > 0)
            {
                cmbBatch.SelectedIndex = -1;
            }
        }

        private void PrintAllButt_Click(object sender, EventArgs e)
        {
            // 1. Validation - Find Active Dropdown
            ComboBox activeCombo = cmbBatch.SelectedItem != null ? cmbBatch : cmbNewBatch2;

            if (activeCombo.SelectedItem == null)
            {
                MessageBox.Show("Please select a Zone from the Batch or New Batch dropdown.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedGroup = activeCombo.SelectedItem.ToString();

            // 2. Collect parts directly from our Master Dictionary
            if (!_masterGroupedData.ContainsKey(selectedGroup) || _masterGroupedData[selectedGroup].Count == 0)
            {
                MessageBox.Show("Selected zone has no part data.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var zoneParts = _masterGroupedData[selectedGroup];

            // 3. Clear and Populate _batchPages with pairs of parts
            _batchPages.Clear();

            for (int i = 0; i < zoneParts.Count; i += 2)
            {
                var p1 = zoneParts[i];

                (string name, string number)? p2 = null;
                if (i + 1 < zoneParts.Count)
                {
                    p2 = zoneParts[i + 1];
                }

                _batchPages.Add((p1, p2));
            }

            // 4. Setup Print Document
            Size paperSize = GetTargetPaperSizeInHundredths();
            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.PaperSize = new PaperSize("CustomSticker", paperSize.Width, paperSize.Height);

                // Start indexing at 0
                pd.BeginPrint += (s, ev) => { _batchPageIndex = 0; };
                pd.PrintPage += new PrintPageEventHandler(PrintBatchPageHandler);

                using (PrintPreviewDialog previewDlg = new PrintPreviewDialog())
                {
                    previewDlg.WindowState = FormWindowState.Maximized;
                    previewDlg.Document = pd;
                    previewDlg.ShowDialog();
                }
            }
        }

        private int GetColumnIndex(DataColumnCollection columns, params string[] possibleNames)
        {
            foreach (string name in possibleNames)
            {
                if (columns.Contains(name))
                    return columns.IndexOf(name);
            }
            return -1;
        }

        private void Uploadbutt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                openFileDialog.Title = "Select the Master Excel File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                        using (var stream = File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                            });

                            if (result.Tables.Count == 0)
                            {
                                MessageBox.Show("The Excel workbook is empty.", "Empty File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            DataTable dt = result.Tables[0];

                            int nameCol = GetColumnIndex(dt.Columns, "Part Name", "Name", "Material Description", "Description", "Desc", "MaterialDesc", "Mat Desc");
                            int numCol = GetColumnIndex(dt.Columns, "Part Number", "Number", "Material Code", "Material", "PartNo", "Part No", "MaterialNo");
                            int zoneCol = GetColumnIndex(dt.Columns, "Zone", "Area", "Zone Name", "Group", "Column0");
                            int binCol = GetColumnIndex(dt.Columns, "Bin Location", "Bin(FINAL)", "Bin");

                            if (nameCol == -1 && numCol == -1)
                            {
                                var headers = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
                                MessageBox.Show($"Could not find a valid Material/Part Number or Description column.\n\nHeaders found:\n{string.Join(", ", headers)}",
                                    "Header Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            // CLEAR OLD / DEFAULT DATA BEFORE LOADING NEW EXCEL DATA
                            _masterGroupedData.Clear();

                            int totalParts = 0;
                            string lastSeenGroup = string.Empty;
                            List<string> currentUploadZones = new List<string>();

                            foreach (DataRow row in dt.Rows)
                            {
                                string partName = nameCol != -1 ? (row[nameCol]?.ToString().Trim() ?? string.Empty) : string.Empty;
                                string partNumber = numCol != -1 ? (row[numCol]?.ToString().Trim() ?? string.Empty) : string.Empty;

                                if (string.IsNullOrWhiteSpace(partName) && string.IsNullOrWhiteSpace(partNumber))
                                    continue;

                                string group = string.Empty;

                                if (zoneCol != -1)
                                {
                                    string rawZone = row[zoneCol]?.ToString().Trim();
                                    if (!string.IsNullOrWhiteSpace(rawZone))
                                    {
                                        group = GetZoneGroup(rawZone);
                                    }
                                }

                                if (string.IsNullOrEmpty(group) && binCol != -1 && zoneCol == -1)
                                {
                                    string rawBin = row[binCol]?.ToString().Trim();
                                    if (!string.IsNullOrWhiteSpace(rawBin))
                                    {
                                        group = GetZoneGroup(rawBin);
                                    }
                                }

                                if (string.IsNullOrEmpty(group))
                                {
                                    group = !string.IsNullOrEmpty(lastSeenGroup) ? lastSeenGroup : "Unassigned";
                                }

                                lastSeenGroup = group;

                                if (!_masterGroupedData.ContainsKey(group))
                                    _masterGroupedData[group] = new List<(string, string)>();

                                if (!currentUploadZones.Contains(group))
                                    currentUploadZones.Add(group);

                                _masterGroupedData[group].Add((partName, partNumber));
                                totalParts++;
                            }

                            RefreshBatchDropdowns(currentUploadZones);

                            MessageBox.Show($"Successfully loaded {totalParts} parts across {currentUploadZones.Count} zones.",
                                "Upload Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("The file is currently in use by another program. Please close Excel and try again.", "File Lock Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading file: {ex.Message}", "Excel Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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

            Size paperSize = GetTargetPaperSizeInHundredths();

            if (e.PageBounds.Width < e.PageBounds.Height)
            {
                g.TranslateTransform(e.PageBounds.Width, 0);
                g.RotateTransform(90f);
                RenderLabelLayout(g, e.PageBounds.Height, e.PageBounds.Width, isPrinting: true);
            }
            else
            {
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

            using (Pen borderPen = new Pen(Color.Black, isPrinting ? 4f : 2f))
            {
                g.DrawRectangle(borderPen, safeX, safeY, safeWidth, safeHeight);
            }

            int innerPadding = 5;
            int drawX = safeX + innerPadding;
            int drawY = safeY + innerPadding;
            int drawWidth = safeWidth - (innerPadding * 2);
            int drawHeight = safeHeight - (innerPadding * 2);

            int gap = isPrinting ? 10 : 5;
            int stripHeight = (drawHeight - gap) / 2;

            RenderStrip(g, _activePartName, _activePartNumber, drawX, drawY, drawWidth, stripHeight, isPrinting);

            int strip2Top = drawY + stripHeight + gap;
            RenderStrip(g, _activePartName2, _activePartNumber2, drawX, strip2Top, drawWidth, stripHeight, isPrinting);

            int lineY = drawY + stripHeight + (gap / 2);
            using (Pen divPen = new Pen(Color.Black, isPrinting ? 2f : 1f))
            {
                divPen.DashStyle = DashStyle.Dash;
                g.DrawLine(divPen, drawX, lineY, drawX + drawWidth, lineY);
            }
        }

        private void RenderStrip(Graphics g, string partName, string partNumber, int stripX, int stripY, int stripWidth, int stripHeight, bool isPrinting)
        {
            if (string.IsNullOrEmpty(partName) && string.IsNullOrEmpty(partNumber)) return;

            int leftColWidth = (int)(stripWidth * 0.75);
            int rightColX = stripX + leftColWidth;
            int rightColWidth = stripWidth - leftColWidth;

            int headerHeight = (int)(stripHeight * 0.40);
            int numberAreaTop = stripY + headerHeight;
            int numberAreaHeight = stripHeight - headerHeight;

            g.FillRectangle(Brushes.Black, stripX, stripY, leftColWidth, headerHeight);

            using (Pen bodyPen = new Pen(Color.Black, isPrinting ? 2f : 1.5f))
            {
                g.DrawRectangle(bodyPen, stripX, numberAreaTop, leftColWidth, numberAreaHeight);
            }

            DrawTextAutofit(g, partName, "Arial", FontStyle.Bold, headerHeight * 0.7f,
                            stripX + 6, stripY + 3, leftColWidth - 12, headerHeight - 6, Brushes.White);

            DrawTextAutofit(g, partNumber, "Arial", FontStyle.Bold, numberAreaHeight * 0.8f,
                            stripX + 6, numberAreaTop + 3, leftColWidth - 12, numberAreaHeight - 6, Brushes.Black);

            int qrSize = Math.Min(rightColWidth - 10, stripHeight - 10);
            string qrPayload = !string.IsNullOrEmpty(partNumber) ? partNumber : partName;

            if (!string.IsNullOrEmpty(qrPayload) && qrSize > 0)
            {
                using (Bitmap qrImg = CreateQRCodeImage(qrPayload))
                {
                    if (qrImg != null)
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.DrawImage(qrImg, rightColX + (rightColWidth - qrSize) / 2, stripY + (stripHeight - qrSize) / 2, qrSize, qrSize);
                    }
                }
            }
        }

        private void DrawTextAutofit(Graphics g, string text, string fontFamily, FontStyle style, float maxFontSize, int x, int y, int maxWidth, int maxHeight, Brush brush)
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

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

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }
}