using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;

using ExcelDataReader;
using System.IO;
using System.Linq;

namespace OJT___QR_Code_Generator
{
    public partial class Form1 : Form
    {
        private string _activeBin1 = string.Empty;
        private string _activeBin2 = string.Empty;

        private List<(string bin1, string bin2)> _batchPages = new List<(string, string)>();
        private int _batchPageIndex = 0;

        public Form1()
        {
            InitializeComponent();

            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            this.btnConvertToPdf.Click += new System.EventHandler(this.btnConvertToPdf_Click);
            this.btnPrintAll.Click += new System.EventHandler(this.btnPrintAll_Click);   // <-- add this line
            this.pnlPreview.Paint += new PaintEventHandler(this.pnlPreview_Paint);

            this.txtCustomWidth.TextChanged += (s, e) => pnlPreview.Invalidate();
            this.txtCustomHeight.TextChanged += (s, e) => pnlPreview.Invalidate();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CenterPanel()
        {
            panel4.Left = (this.ClientSize.Width - panel4.Width) / 2;
            panel4.Top = (this.ClientSize.Height - panel4.Height) / 2;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CenterPanel();
            this.Resize += (s, e) => CenterPanel();

            txtCustomWidth.Text = "3";
            txtCustomHeight.Text = "6";

            // Seed default data from the old hardcoded WarehouseData so the app works
            // before any file is uploaded. Uploading a file later will overwrite this.
            SharedWarehouseData.UploadedZones.Clear();
            foreach (var kvp in WarehouseData.myZones)
            {
                string zoneKey = kvp.Key.ToString();
                SharedWarehouseData.UploadedZones[zoneKey] = kvp.Value
                    .Select(bin => new BinEntry { BinLocation = bin, Material = "", MaterialDescription = "" })
                    .ToList();
            }

            var sortedZones = SharedWarehouseData.UploadedZones.Keys.ToList();
            sortedZones.Sort(new NaturalStringComparer());

            cmbBatch.DataSource = null;
            cmbBatch.Items.Clear();
            cmbBatch.DataSource = sortedZones;
        }

        private Size GetTargetPaperSizeInHundredths()
        {
            double widthInches = 3.2;
            double heightInches = 5.8;

            double.TryParse(txtCustomWidth.Text, out widthInches);
            double.TryParse(txtCustomHeight.Text, out heightInches);

            int w = (int)(widthInches * 100);
            int h = (int)(heightInches * 100);

            if (w <= 0) w = 320;
            if (h <= 0) h = 580;

            return new Size(w, h);
        }

        private void PrintBatchPageHandler(object sender, PrintPageEventArgs e)
        {
            if (_batchPageIndex >= _batchPages.Count)
            {
                e.HasMorePages = false;
                return;
            }

            // Temporarily swap in this page's bin values so the existing render path handles it
            string savedBin1 = _activeBin1;
            string savedBin2 = _activeBin2;

            _activeBin1 = _batchPages[_batchPageIndex].bin1;
            _activeBin2 = _batchPages[_batchPageIndex].bin2;

            PrintLabelsHandler(sender, e); // exact same rotation + RenderLabelLayout call as manual mode

            _activeBin1 = savedBin1;
            _activeBin2 = savedBin2;

            _batchPageIndex++;
            e.HasMorePages = _batchPageIndex < _batchPages.Count;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _activeBin1 = txtBinLocation1.Text.Trim();
            _activeBin2 = txtBinLocation2.Text.Trim();

            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2))
            {
                MessageBox.Show("Please enter at least one Bin Location value to generate.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            pnlPreview.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtBinLocation1.Clear();
            txtBinLocation2.Clear();
            _activeBin1 = string.Empty;
            _activeBin2 = string.Empty;
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
            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2))
            {
                MessageBox.Show("Please enter a Bin Location and click Generate before printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2))
            {
                MessageBox.Show("Please generate a Bin Location layout before converting to PDF.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDlg.Title = "Save Bin Labels as PDF";
                saveDlg.FileName = $"Bin_Labels_{_activeBin1.Replace("-", "_")}";

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

        // 🛠️ HONEYWELL COMPATIBLE HIGH-SCALE LAYOUT
        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            // Safeguard border cutoff zone on physical thermal heads (Honeywell standard unprintable margins)
            int margin = isPrinting ? 20 : 8;

            int safeX = margin;
            int safeY = margin;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);
            int halfHeight = safeHeight / 2;

            // Maintain your 75% wide partition strategy securely in safe zones
            int qrDividerX = safeX + (int)(safeWidth * 0.75);

            // 1. Draw frame paths
            int penThickness = isPrinting ? 4 : 2;
            using (Pen blackPen = new Pen(Color.Black, penThickness))
            {
                g.DrawRectangle(blackPen, safeX, safeY, safeWidth, safeHeight);
                g.DrawLine(blackPen, safeX, safeY + halfHeight, safeX + safeWidth, safeY + halfHeight);
                g.DrawLine(blackPen, qrDividerX, safeY, qrDividerX, safeY + safeHeight);
            }

            // 2. MAXIMUM AUTOFIT BIN TEXT: Automatically climbs to the tallest absolute size allowed by the height bounds.
            float maxFontCeiling = safeHeight * 0.32f;
            int textPadding = 10;
            int textBoxWidth = (qrDividerX - safeX) - textPadding;
            int textBoxHeight = halfHeight - textPadding;

            // Top Row Bin Code Text
            if (!string.IsNullOrEmpty(_activeBin1))
            {
                DrawTextAutofit(g, _activeBin1, "Arial", FontStyle.Bold, maxFontCeiling, safeX + (textPadding / 2), safeY + (textPadding / 2), textBoxWidth, textBoxHeight);
            }

            // Bottom Row Bin Code Text
            if (!string.IsNullOrEmpty(_activeBin2))
            {
                DrawTextAutofit(g, _activeBin2, "Arial", FontStyle.Bold, maxFontCeiling, safeX + (textPadding / 2), safeY + halfHeight + (textPadding / 2), textBoxWidth, textBoxHeight);
            }

            // 3. MAXIMIZED SYSTEM QR CODES: Scales cleanly to 95% of available inner box area height
            int qrSize = (int)(halfHeight * 0.95);
            int rightCompartmentWidth = (safeX + safeWidth) - qrDividerX;
            int qrX = qrDividerX + (rightCompartmentWidth - qrSize) / 2;

            // Top Row QR Code
            if (!string.IsNullOrEmpty(_activeBin1))
            {
                using (Bitmap qrImg = CreateQRCodeImage(_activeBin1))
                {
                    if (qrImg != null)
                    {
                        int qrY = safeY + (halfHeight - qrSize) / 2;
                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);
                    }
                }
            }

            // Bottom Row QR Code
            if (!string.IsNullOrEmpty(_activeBin2))
            {
                using (Bitmap qrImg = CreateQRCodeImage(_activeBin2))
                {
                    if (qrImg != null)
                    {
                        int qrY = safeY + halfHeight + (halfHeight - qrSize) / 2;
                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);
                    }
                }
            }
        }

        // Adaptive font crunching ruleset to ensure maximum enlargement on both 6" and 5.4" lengths without bounds breaches
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
                {
                    using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
                    {
                        try
                        {
                            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                            {
                                byte[] qrCodeBytes = qrCode.GetGraphic(4);
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
                                return qrCode.GetGraphic(4, Color.Black, Color.White, true);
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

        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            if (cmbBatch.SelectedItem == null)
            {
                MessageBox.Show("Please select a Zone from the Batch dropdown.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedZone = cmbBatch.SelectedItem.ToString();

            if (!SharedWarehouseData.UploadedZones.ContainsKey(selectedZone) ||
                SharedWarehouseData.UploadedZones[selectedZone].Count == 0)
            {
                MessageBox.Show("Selected zone has no bin location data. Please upload an Excel file first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<BinEntry> entries = SharedWarehouseData.UploadedZones[selectedZone];

            _batchPages.Clear();
            for (int i = 0; i < entries.Count; i += 2)
            {
                string bin1 = entries[i].BinLocation;
                string bin2 = (i + 1 < entries.Count) ? entries[i + 1].BinLocation : string.Empty;
                _batchPages.Add((bin1, bin2));
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Landscape = false;
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

        private void cmbBatch_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtBinLocation1_TextChanged(object sender, EventArgs e)
        {

        }

        // Zone is now read directly from a column (with forward-fill for merged-style cells),
        // NOT derived from the Bin Location text.
        // Helper to find the correct column index regardless of header name variations
        private int GetColumnIndex(DataColumnCollection columns, params string[] possibleNames)
        {
            foreach (string name in possibleNames)
            {
                if (columns.Contains(name))
                    return columns.IndexOf(name);
            }
            return -1;
        }

        private int GetZoneColumnIndex(DataTable dt, int binCol, int matCol, int descCol)
        {
            // 1. Try an explicit "Zone" header first (in case some files do label it)
            int zoneCol = GetColumnIndex(dt.Columns, "Zone", "Zone Name", "Area");
            if (zoneCol != -1) return zoneCol;

            // 2. Fallback: if column 0 exists and isn't already used for Bin/Material/Description,
            //    treat it as the (unlabeled) Zone column — matches your real file's layout.
            if (dt.Columns.Count > 0 && 0 != binCol && 0 != matCol && 0 != descCol)
                return 0;

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

                            int binCol = GetColumnIndex(dt.Columns, "Bin Location", "Bin(FINAL)", "Bin");
                            int matCol = GetColumnIndex(dt.Columns, "Material", "Material Code");
                            int descCol = GetColumnIndex(dt.Columns, "Material Description", "Description");

                            if (binCol == -1 || matCol == -1)
                            {
                                var headers = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
                                MessageBox.Show($"Could not confidently match required columns (Bin Location and Material).\n\nHeaders found:\n{string.Join(", ", headers)}",
                                    "Header Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            int zoneCol = GetZoneColumnIndex(dt, binCol, matCol, descCol);
                            if (zoneCol == -1)
                            {
                                MessageBox.Show("Could not find a Zone column in this file.", "Header Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            SharedWarehouseData.UploadedZones.Clear();
                            int totalBins = 0;
                            string lastSeenZone = string.Empty;

                            foreach (DataRow row in dt.Rows)
                            {
                                string binLoc = row[binCol]?.ToString().Trim();
                                string material = row[matCol]?.ToString().Trim();
                                string description = descCol != -1 ? row[descCol]?.ToString().Trim() : string.Empty;

                                // Forward-fill: if this row has a zone value, remember it;
                                // otherwise reuse the last zone seen above it (merged-cell behavior).
                                string zoneCell = row[zoneCol]?.ToString().Trim();
                                if (!string.IsNullOrWhiteSpace(zoneCell))
                                    lastSeenZone = zoneCell;

                                if (string.IsNullOrWhiteSpace(binLoc) || string.IsNullOrWhiteSpace(material))
                                    continue;

                                string zone = string.IsNullOrWhiteSpace(lastSeenZone) ? "Unassigned" : lastSeenZone;

                                if (!SharedWarehouseData.UploadedZones.ContainsKey(zone))
                                    SharedWarehouseData.UploadedZones[zone] = new List<BinEntry>();

                                SharedWarehouseData.UploadedZones[zone].Add(new BinEntry
                                {
                                    BinLocation = binLoc,
                                    Material = material,
                                    MaterialDescription = description
                                });
                                totalBins++;
                            }

                            var sortedZones = SharedWarehouseData.UploadedZones.Keys.ToList();
                            sortedZones.Sort(new NaturalStringComparer());

                            cmbBatch.DataSource = null;
                            cmbBatch.Items.Clear();
                            cmbBatch.DataSource = sortedZones;

                            MessageBox.Show($"Successfully loaded {totalBins} bins across {sortedZones.Count} zones.",
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


    }
}