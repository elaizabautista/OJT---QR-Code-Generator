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
            this.btnPrintAll.Click += new System.EventHandler(this.btnPrintAll_Click);
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

            // Seed default data from the old hardcoded WarehouseData
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

            string savedBin1 = _activeBin1;
            string savedBin2 = _activeBin2;

            _activeBin1 = _batchPages[_batchPageIndex].bin1;
            _activeBin2 = _batchPages[_batchPageIndex].bin2;

            PrintLabelsHandler(sender, e);

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

        // HONEYWELL COMPATIBLE HIGH-SCALE LAYOUT
        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            int margin = isPrinting ? 20 : 8;

            int safeX = margin;
            int safeY = margin;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);
            int halfHeight = safeHeight / 2;

            int qrDividerX = safeX + (int)(safeWidth * 0.75);

            int penThickness = isPrinting ? 4 : 2;
            using (Pen blackPen = new Pen(Color.Black, penThickness))
            {
                g.DrawRectangle(blackPen, safeX, safeY, safeWidth, safeHeight);
                g.DrawLine(blackPen, safeX, safeY + halfHeight, safeX + safeWidth, safeY + halfHeight);
                g.DrawLine(blackPen, qrDividerX, safeY, qrDividerX, safeY + safeHeight);
            }

            float maxFontCeiling = safeHeight * 0.32f;
            int textPadding = 10;
            int textBoxWidth = (qrDividerX - safeX) - textPadding;
            int textBoxHeight = halfHeight - textPadding;

            if (!string.IsNullOrEmpty(_activeBin1))
            {
                DrawTextAutofit(g, _activeBin1, "Arial", FontStyle.Bold, maxFontCeiling, safeX + (textPadding / 2), safeY + (textPadding / 2), textBoxWidth, textBoxHeight);
            }

            if (!string.IsNullOrEmpty(_activeBin2))
            {
                DrawTextAutofit(g, _activeBin2, "Arial", FontStyle.Bold, maxFontCeiling, safeX + (textPadding / 2), safeY + halfHeight + (textPadding / 2), textBoxWidth, textBoxHeight);
            }

            int qrSize = (int)(halfHeight * 0.95);
            int rightCompartmentWidth = (safeX + safeWidth) - qrDividerX;
            int qrX = qrDividerX + (rightCompartmentWidth - qrSize) / 2;

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
            ComboBox activeCombo = cmbBatch.SelectedItem != null ? cmbBatch : cmbNewBatch;

            if (activeCombo.SelectedItem == null)
            {
                MessageBox.Show("Please select a Zone from the Batch or New Batch dropdown.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedZone = activeCombo.SelectedItem.ToString();

            if (!SharedWarehouseData.UploadedZones.ContainsKey(selectedZone) ||
                SharedWarehouseData.UploadedZones[selectedZone].Count == 0)
            {
                MessageBox.Show("Selected zone has no bin location data.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (cmbBatch.SelectedItem != null && cmbNewBatch.Items.Count > 0)
            {
                cmbNewBatch.SelectedIndex = -1;
            }
        }

        private void txtBinLocation1_TextChanged(object sender, EventArgs e)
        {
        }

        // --- UPDATED METHOD: Smarter auto-detection (Fuzzy Matching) ---
        private int GetColumnIndex(DataColumnCollection columns, params string[] possibleNames)
        {
            // Try exact match first
            foreach (string name in possibleNames)
            {
                if (columns.Contains(name))
                    return columns.IndexOf(name);
            }

            // Try fuzzy match (ignores uppercase/lowercase and spaces)
            for (int i = 0; i < columns.Count; i++)
            {
                string colName = columns[i].ColumnName.Replace(" ", "").ToLower();
                foreach (string possibleName in possibleNames)
                {
                    if (colName.Contains(possibleName.Replace(" ", "").ToLower()))
                        return i;
                }
            }
            return -1;
        }

        private int GetZoneColumnIndex(DataTable dt, int binCol, int matCol, int descCol)
        {
            int zoneCol = GetColumnIndex(dt.Columns, "Zone", "Zone Name", "Area");
            if (zoneCol != -1) return zoneCol;

            if (dt.Columns.Count > 0 && 0 != binCol && 0 != matCol && 0 != descCol)
                return 0;

            return -1;
        }

        // --- NEW METHOD: Dynamic mapping pop-up if headers can't be found ---
        private bool ShowColumnMappingDialog(List<string> headers, ref int binCol, ref int matCol, ref int descCol, ref int zoneCol)
        {
            Form mapForm = new Form
            {
                Width = 400,
                Height = 300,
                Text = "Map Excel Columns",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // Labels
            mapForm.Controls.Add(new Label { Text = "Bin Location:", Left = 20, Top = 23, Width = 100 });
            mapForm.Controls.Add(new Label { Text = "Material:", Left = 20, Top = 63, Width = 100 });
            mapForm.Controls.Add(new Label { Text = "Description:", Left = 20, Top = 103, Width = 100 });
            mapForm.Controls.Add(new Label { Text = "Zone (Optional):", Left = 20, Top = 143, Width = 100 });

            // ComboBoxes for mapping
            ComboBox cbBin = new ComboBox { Left = 130, Top = 20, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            ComboBox cbMat = new ComboBox { Left = 130, Top = 60, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            ComboBox cbDesc = new ComboBox { Left = 130, Top = 100, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            ComboBox cbZone = new ComboBox { Left = 130, Top = 140, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };

            // Add Excel headers to the dropdowns
            string[] headerArray = headers.ToArray();
            cbBin.Items.AddRange(headerArray);
            cbMat.Items.AddRange(headerArray);
            cbDesc.Items.AddRange(headerArray);

            // Add a blank option for Zone since it might not exist
            cbZone.Items.Add("-- No Zone Column --");
            cbZone.Items.AddRange(headerArray);

            // Auto-select if the system guessed correctly
            if (binCol != -1 && binCol < cbBin.Items.Count) cbBin.SelectedIndex = binCol;
            if (matCol != -1 && matCol < cbMat.Items.Count) cbMat.SelectedIndex = matCol;
            if (descCol != -1 && descCol < cbDesc.Items.Count) cbDesc.SelectedIndex = descCol;

            if (zoneCol != -1 && zoneCol < (cbZone.Items.Count - 1))
                cbZone.SelectedIndex = zoneCol + 1; // +1 because of the "-- No Zone Column --" at index 0
            else
                cbZone.SelectedIndex = 0;

            mapForm.Controls.AddRange(new Control[] { cbBin, cbMat, cbDesc, cbZone });

            Button btnOk = new Button { Text = "Confirm Mapping", Left = 130, Top = 200, Width = 120, DialogResult = DialogResult.OK };
            mapForm.Controls.Add(btnOk);
            mapForm.AcceptButton = btnOk;

            if (mapForm.ShowDialog() == DialogResult.OK)
            {
                binCol = cbBin.SelectedIndex;
                matCol = cbMat.SelectedIndex;
                descCol = cbDesc.SelectedIndex;

                // Adjust for the empty "-- No Zone Column --" option
                zoneCol = cbZone.SelectedIndex > 0 ? cbZone.SelectedIndex - 1 : -1;
                return true;
            }
            return false; // User cancelled
        }

        // --- UPDATED METHOD: Integrated Auto-detect & UI fallback ---
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

                            // Try to detect columns with fuzzy matching
                            int binCol = GetColumnIndex(dt.Columns, "Bin Location", "Bin(FINAL)", "Bin", "loc");
                            int matCol = GetColumnIndex(dt.Columns, "Material", "Material Code", "sku", "item");
                            int descCol = GetColumnIndex(dt.Columns, "Material Description", "Description", "desc");
                            int zoneCol = GetZoneColumnIndex(dt, binCol, matCol, descCol);

                            // If it missed ANY required columns, open the UI Mapping Form instead of crashing
                            if (binCol == -1 || matCol == -1 || zoneCol == -1)
                            {
                                var headers = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

                                MessageBox.Show("Some columns couldn't be detected automatically from this layout. Please map them manually.", "Manual Mapping Required", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                bool userMapped = ShowColumnMappingDialog(headers, ref binCol, ref matCol, ref descCol, ref zoneCol);

                                // If the user cancelled or left required dropdowns empty
                                if (!userMapped || binCol == -1 || matCol == -1)
                                {
                                    MessageBox.Show("Upload cancelled. 'Bin Location' and 'Material' columns are required to continue.", "Upload Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return; // Stop the upload process safely
                                }
                            }

                            int totalBins = 0;
                            string lastSeenZone = string.Empty;

                            List<string> currentUploadZones = new List<string>();

                            foreach (DataRow row in dt.Rows)
                            {
                                // Protect against mapped columns falling out of bounds or missing
                                string binLoc = binCol != -1 && binCol < dt.Columns.Count ? row[binCol]?.ToString().Trim() : string.Empty;
                                string material = matCol != -1 && matCol < dt.Columns.Count ? row[matCol]?.ToString().Trim() : string.Empty;
                                string description = descCol != -1 && descCol < dt.Columns.Count ? row[descCol]?.ToString().Trim() : string.Empty;

                                string zoneCell = string.Empty;
                                if (zoneCol != -1 && zoneCol < dt.Columns.Count)
                                {
                                    zoneCell = row[zoneCol]?.ToString().Trim();
                                }

                                if (!string.IsNullOrWhiteSpace(zoneCell))
                                    lastSeenZone = zoneCell;

                                if (string.IsNullOrWhiteSpace(binLoc) || string.IsNullOrWhiteSpace(material))
                                    continue;

                                string zone = string.IsNullOrWhiteSpace(lastSeenZone) ? "Unassigned" : lastSeenZone;

                                if (!SharedWarehouseData.UploadedZones.ContainsKey(zone))
                                    SharedWarehouseData.UploadedZones[zone] = new List<BinEntry>();

                                if (!currentUploadZones.Contains(zone))
                                    currentUploadZones.Add(zone);

                                SharedWarehouseData.UploadedZones[zone].Add(new BinEntry
                                {
                                    BinLocation = binLoc,
                                    Material = material,
                                    MaterialDescription = description
                                });
                                totalBins++;
                            }

                            currentUploadZones.Sort(new NaturalStringComparer());

                            cmbNewBatch.DataSource = null;
                            cmbNewBatch.Items.Clear();
                            cmbNewBatch.DataSource = currentUploadZones;

                            var allZones = SharedWarehouseData.UploadedZones.Keys.ToList();
                            allZones.Sort(new NaturalStringComparer());

                            string previousBatchSelection = cmbBatch.SelectedItem?.ToString();

                            cmbBatch.DataSource = null;
                            cmbBatch.Items.Clear();
                            cmbBatch.DataSource = allZones;

                            if (!string.IsNullOrEmpty(previousBatchSelection) && allZones.Contains(previousBatchSelection))
                            {
                                cmbBatch.SelectedItem = previousBatchSelection;
                            }
                            else if (cmbBatch.Items.Count > 0)
                            {
                                cmbBatch.SelectedIndex = 0;
                            }

                            if (cmbNewBatch.Items.Count > 0)
                            {
                                cmbNewBatch.SelectedIndex = 0;
                                cmbBatch.SelectedIndex = -1;
                            }

                            MessageBox.Show($"Successfully loaded {totalBins} bins across {currentUploadZones.Count} zones.",
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

        private void cmbNewBatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbNewBatch.SelectedItem != null && cmbBatch.Items.Count > 0)
            {
                cmbBatch.SelectedIndex = -1;
            }
        }
    }
}