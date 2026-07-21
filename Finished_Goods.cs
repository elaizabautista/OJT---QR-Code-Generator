using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Xml.Linq;

using ExcelDataReader;
using System.IO;
using System.Linq;

namespace OJT___QR_Code_Generator
{
    public partial class Finished_Goods : Form
    {
        private string _activeNumber1 = string.Empty;
        private string _activeNumber2 = string.Empty;

        // Class-level dictionary holding merged zones (Hardcoded + Uploaded)
        private Dictionary<string, List<string>> _uploadedZones = new Dictionary<string, List<string>>();
        private int _batchCurrentIndex = 0;

        public Finished_Goods()
        {
            InitializeComponent();

            this.btnGenerate.Click += new EventHandler(this.btnGenerate_Click);
            this.btnClear.Click += new EventHandler(this.btnClear_Click);
            this.btnPrint.Click += new EventHandler(this.btnPrint_Click);
            this.btnConvertToPdf.Click += new EventHandler(this.btnConvertToPdf_Click);
            this.btnPrintAll.Click += new EventHandler(this.btnPrintAll_Click);
            this.pnlPreview.Paint += new PaintEventHandler(this.pnlPreview_Paint);
            this.cmbBatch.SelectedIndexChanged += new EventHandler(this.cmbBatch_SelectedIndexChanged);

            this.txtCustomWidth.TextChanged += (s, e) => pnlPreview.Invalidate();
            this.txtCustomHeight.TextChanged += (s, e) => pnlPreview.Invalidate();
        }

        private void CenterPanel()
        {
            panel4.Left = (this.ClientSize.Width - panel4.Width) / 2;
            panel4.Top = (this.ClientSize.Height - panel4.Height) / 2;
        }

        private void Finished_Goods_Load(object sender, EventArgs e)
        {
            CenterPanel();
            this.Resize += (s, e) => CenterPanel();

            txtCustomWidth.Text = "4";
            txtCustomHeight.Text = "6";

            // 1. Seed local dictionary with hardcoded FGData if available
            _uploadedZones.Clear();
            if (FGData.ZoneToBins != null)
            {
                foreach (var kvp in FGData.ZoneToBins)
                {
                    _uploadedZones[kvp.Key] = new List<string>(kvp.Value);
                }
            }

            // 2. Bind initial list
            RefreshBatchDropdowns(new List<string>());
        }

        private void RefreshBatchDropdowns(List<string> newlyUploadedZones)
        {
            // Populate New Batch if there are newly uploaded zones
            if (newlyUploadedZones != null && newlyUploadedZones.Count > 0)
            {
                var newBatchSource = newlyUploadedZones
                    .OrderBy(z => z)
                    .Select(z => new KeyValuePair<string, string[]>(z, _uploadedZones[z].ToArray()))
                    .ToList();

                cmbNewBatch3.DataSource = null;
                cmbNewBatch3.DisplayMember = "Key";
                cmbNewBatch3.DataSource = newBatchSource;
            }

            // Populate Old Batch with everything
            var allZonesSource = _uploadedZones.Keys
                .OrderBy(k => k)
                .Select(z => new KeyValuePair<string, string[]>(z, _uploadedZones[z].ToArray()))
                .ToList();

            string previousBatchSelection = cmbBatch.SelectedItem != null ? ((KeyValuePair<string, string[]>)cmbBatch.SelectedItem).Key : null;

            cmbBatch.DataSource = null;
            cmbBatch.DisplayMember = "Key";
            cmbBatch.DataSource = allZonesSource;

            // Restore previous selection if possible
            if (!string.IsNullOrEmpty(previousBatchSelection))
            {
                var match = allZonesSource.FirstOrDefault(x => x.Key == previousBatchSelection);
                if (match.Key != null) cmbBatch.SelectedItem = match;
            }
            else if (cmbBatch.Items.Count > 0)
            {
                cmbBatch.SelectedIndex = 0;
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
            _activeNumber1 = txtFinishedGoodsLocation1.Text.Trim();
            _activeNumber2 = txtFinishedGoodsLocation2.Text.Trim();

            if (string.IsNullOrEmpty(_activeNumber1) || string.IsNullOrEmpty(_activeNumber2))
            {
                MessageBox.Show("Please enter both codes to generate.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            pnlPreview.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFinishedGoodsLocation1.Clear();
            txtFinishedGoodsLocation2.Clear();
            _activeNumber1 = string.Empty;
            _activeNumber2 = string.Empty;
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

            RenderFinishedGoodsLabel(g, totalWidth, totalHeight, isPrinting: false);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activeNumber1) || string.IsNullOrEmpty(_activeNumber2))
            {
                MessageBox.Show("Please enter both codes and click Generate before printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Landscape = true;
                pd.DefaultPageSettings.PaperSize = new PaperSize("FinishedGoodsSticker", paperSize.Width, paperSize.Height);
                pd.PrintPage += new PrintPageEventHandler(PrintFinishedGoodsHandler);

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
            if (string.IsNullOrEmpty(_activeNumber1) || string.IsNullOrEmpty(_activeNumber2))
            {
                MessageBox.Show("Please generate a Finished Goods label before converting to PDF.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDlg.Title = "Save Finished Goods Label as PDF";
                saveDlg.FileName = $"FinishedGoods_{_activeNumber1.Replace("-", "_")}_{_activeNumber2.Replace("-", "_")}";

                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    using (PrintDocument pd = new PrintDocument())
                    {
                        pd.DefaultPageSettings.Landscape = true;
                        pd.DefaultPageSettings.PaperSize = new PaperSize("FinishedGoodsSticker", paperSize.Width, paperSize.Height);
                        pd.OriginAtMargins = false;

                        pd.PrintPage += new PrintPageEventHandler(PrintFinishedGoodsHandler);
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
            ComboBox activeCombo = cmbBatch.SelectedItem != null ? cmbBatch : cmbNewBatch3;

            if (activeCombo.SelectedItem is KeyValuePair<string, string[]> selectedPair)
            {
                string[] locations = selectedPair.Value;

                if (locations != null && locations.Length > 0)
                {
                    _batchCurrentIndex = 0;

                    using (PrintDocument pd = new PrintDocument())
                    {
                        Size paperSize = GetTargetPaperSizeInHundredths();
                        pd.DefaultPageSettings.Landscape = true;
                        pd.DefaultPageSettings.PaperSize = new PaperSize("FinishedGoodsSticker", paperSize.Width, paperSize.Height);

                        pd.BeginPrint += (s, ev) => { _batchCurrentIndex = 0; };

                        pd.PrintPage += (s, ev) =>
                        {
                            if (_batchCurrentIndex >= locations.Length)
                            {
                                ev.HasMorePages = false;
                                return;
                            }

                            _activeNumber1 = locations[_batchCurrentIndex];
                            _activeNumber2 = (_batchCurrentIndex + 1 < locations.Length) ? locations[_batchCurrentIndex + 1] : "N/A";

                            RenderFinishedGoodsLabel(ev.Graphics, ev.PageBounds.Width, ev.PageBounds.Height, true);

                            _batchCurrentIndex += 2;
                            ev.HasMorePages = (_batchCurrentIndex < locations.Length);
                        };

                        using (PrintPreviewDialog previewDlg = new PrintPreviewDialog())
                        {
                            previewDlg.Document = pd;
                            previewDlg.WindowState = FormWindowState.Maximized;
                            previewDlg.ShowDialog();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Selected batch contains no data.", "Empty Batch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a valid batch to print.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrintFinishedGoodsHandler(object sender, PrintPageEventArgs e)
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

            RenderFinishedGoodsLabel(g, printableWidth, printableHeight, isPrinting: true);
        }

        private void RenderFinishedGoodsLabel(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            int margin = isPrinting ? 14 : 4;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);

            if (string.IsNullOrEmpty(_activeNumber1) || string.IsNullOrEmpty(_activeNumber2))
                return;

            int rowHeight = safeHeight / 2;

            RenderCodeRow(g, _activeNumber1, margin, margin, safeWidth, rowHeight);
            RenderCodeRow(g, _activeNumber2, margin, margin + rowHeight, safeWidth, rowHeight);

            using (Pen dividerPen = new Pen(Color.Black, isPrinting ? 2f : 1f))
            {
                int dividerY = margin + rowHeight;
                g.DrawLine(dividerPen, margin, dividerY, margin + safeWidth, dividerY);
            }

            using (Pen borderPen = new Pen(Color.Black, isPrinting ? 3f : 2f))
            {
                g.DrawRectangle(borderPen, margin, margin, safeWidth - 1, safeHeight - 1);
            }
        }

        private void RenderCodeRow(Graphics g, string codeValue, int rowX, int rowY, int rowWidth, int rowHeight)
        {
            if (string.IsNullOrEmpty(codeValue))
                return;

            int innerPad = 10;
            int safeX = rowX + innerPad;
            int safeY = rowY + innerPad;
            int safeWidth = rowWidth - (innerPad * 2);
            int safeHeight = rowHeight - (innerPad * 2);

            int qrSize = (int)(safeHeight * 0.99f);
            int qrX = safeX + safeWidth - qrSize;
            int qrY = safeY + (safeHeight - qrSize) / 2;

            using (Bitmap qrImg = CreateQRCodeImage(codeValue))
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

            int textGap = (int)(safeHeight * 0.03f);
            int textWidth = qrX - safeX - textGap;

            if (textWidth > 0)
            {
                DrawTextAutofit(g, codeValue, "Arial", FontStyle.Bold, safeHeight, safeX, safeY, textWidth, safeHeight);
            }
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
                            byte[] qrCodeBytes = qrCode.GetGraphic(20, Color.Black, Color.White, drawQuietZones: false);
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
                            return qrCode.GetGraphic(20, Color.Black, Color.White, drawQuietZones: false);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        // --- EXCEL UPLOAD AND MAPPING LOGIC ---

        private int GetColumnIndex(DataColumnCollection columns, params string[] possibleNames)
        {
            // 1. Exact match first
            for (int i = 0; i < columns.Count; i++)
            {
                string colName = columns[i].ColumnName.Trim();
                foreach (string name in possibleNames)
                {
                    if (string.Equals(colName, name, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }

            // 2. Partial match (excluding status/YN columns)
            for (int i = 0; i < columns.Count; i++)
            {
                string colName = columns[i].ColumnName.Replace(" ", "").ToLower();
                if (colName.Contains("status") || colName.Contains("active") || colName.Contains("flag") || colName == "yn" || colName == "y/n")
                    continue;

                foreach (string possibleName in possibleNames)
                {
                    string cleanPossible = possibleName.Replace(" ", "").ToLower();
                    if (colName.Contains(cleanPossible))
                        return i;
                }
            }
            return -1;
        }

        private int GetZoneColumnIndex(DataTable dt, int codeCol)
        {
            int zoneCol = GetColumnIndex(dt.Columns, "Zone", "Zone Name", "Area", "Rack Zone", "Zone Header");
            if (zoneCol != -1 && zoneCol != codeCol)
                return zoneCol;
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

                            if (dt.Columns.Count == 0)
                            {
                                MessageBox.Show("The Excel table contains no columns.", "Empty File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // 1. Detect columns via standard names
                            int codeCol = GetColumnIndex(dt.Columns, "Location", "Code", "FG Code", "Bin", "Item", "Finished Goods", "Cell");
                            int zoneCol = GetZoneColumnIndex(dt, codeCol);

                            // 2. Default Column 0 as Zone, Column 1 as Code if names were not detected
                            if (zoneCol == -1 && dt.Columns.Count > 0)
                            {
                                zoneCol = 0; // Default Column 0 (1st Column)
                            }

                            if (codeCol == -1 || codeCol == zoneCol)
                            {
                                codeCol = (dt.Columns.Count > 1 && zoneCol == 0) ? 1 : 0; // Default Column 1 (2nd Column)
                            }

                            int totalCodes = 0;
                            string lastSeenZone = string.Empty;
                            List<string> currentUploadZones = new List<string>();

                            // 3. Process first header row if Excel file had no title row (e.g., Row 1 is already Zone 1 | 1-1A-1)
                            if (codeCol < dt.Columns.Count && zoneCol < dt.Columns.Count)
                            {
                                string headerZone = dt.Columns[zoneCol].ColumnName.Trim();
                                string headerCode = dt.Columns[codeCol].ColumnName.Trim();

                                string[] headerKeywords = new string[] { "location", "code", "fg", "bin", "item", "cell", "finished goods", "column" };
                                bool isHeaderTitle = headerKeywords.Any(k => headerCode.ToLower().Contains(k));

                                if (!isHeaderTitle && !string.IsNullOrWhiteSpace(headerCode))
                                {
                                    string zone = string.IsNullOrWhiteSpace(headerZone) ? "Unassigned FG" : headerZone;
                                    lastSeenZone = zone;

                                    if (!_uploadedZones.ContainsKey(zone))
                                        _uploadedZones[zone] = new List<string>();

                                    if (!currentUploadZones.Contains(zone))
                                        currentUploadZones.Add(zone);

                                    if (!_uploadedZones[zone].Contains(headerCode))
                                    {
                                        _uploadedZones[zone].Add(headerCode);
                                        totalCodes++;
                                    }
                                }
                            }

                            // 4. Iterate data rows and group into zones
                            foreach (DataRow row in dt.Rows)
                            {
                                string code = codeCol != -1 && codeCol < dt.Columns.Count ? row[codeCol]?.ToString().Trim() : string.Empty;

                                string zoneCell = string.Empty;
                                if (zoneCol != -1 && zoneCol < dt.Columns.Count)
                                {
                                    zoneCell = row[zoneCol]?.ToString().Trim();
                                }

                                if (!string.IsNullOrWhiteSpace(zoneCell))
                                {
                                    lastSeenZone = zoneCell;
                                }

                                if (string.IsNullOrWhiteSpace(code))
                                    continue;

                                string zone = string.IsNullOrWhiteSpace(lastSeenZone) ? "Unassigned FG" : lastSeenZone;

                                if (!_uploadedZones.ContainsKey(zone))
                                    _uploadedZones[zone] = new List<string>();

                                if (!currentUploadZones.Contains(zone))
                                    currentUploadZones.Add(zone);

                                if (!_uploadedZones[zone].Contains(code))
                                {
                                    _uploadedZones[zone].Add(code);
                                    totalCodes++;
                                }
                            }

                            // 5. Update dropdowns
                            RefreshBatchDropdowns(currentUploadZones);

                            if (cmbNewBatch3.Items.Count > 0)
                            {
                                cmbNewBatch3.SelectedIndex = 0;
                                cmbBatch.SelectedIndex = -1;
                            }

                            MessageBox.Show($"Successfully loaded {totalCodes} Finished Goods codes across {currentUploadZones.Count} zone(s).",
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

        private void cmbBatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBatch.SelectedItem != null && cmbNewBatch3.Items.Count > 0)
            {
                cmbNewBatch3.SelectedIndex = -1;
            }
        }

        private void cmbNewBatch3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbNewBatch3.SelectedItem != null && cmbBatch.Items.Count > 0)
            {
                cmbBatch.SelectedIndex = -1;
            }
        }
    }
}