using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExcelDataReader;
using QRCoder;

namespace OJT___QR_Code_Generator
{
    public partial class Form_5 : Form
    {
        // Active bins for the 2x2 layout (4 total)
        private string _activeBin1 = string.Empty; // Top-left
        private string _activeBin2 = string.Empty; // Top-right
        private string _activeBin3 = string.Empty; // Bottom-left
        private string _activeBin4 = string.Empty; // Bottom-right

        // Each batch page holds FOUR bin values (one per grid cell)
        private List<(string bin1, string bin2, string bin3, string bin4)> _batchPages = new List<(string bin1, string bin2, string bin3, string bin4)>();
        private int _batchPageIndex = 0;

        // Stores parsed zones and bin lists from uploaded Excel files
        private Dictionary<string, List<string>> _uploadedZones = new Dictionary<string, List<string>>();

        public Form_5()
        {
            InitializeComponent();
            this.pnlPreview.Paint += new PaintEventHandler(this.pnlPreview_Paint);
        }

        private void CenterPanel()
        {
            // Safety check to prevent negative placement if form gets too small
            if (this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
            {
                panel4.Left = (this.ClientSize.Width - panel4.Width) / 2;
                panel4.Top = (this.ClientSize.Height - panel4.Height) / 2;
            }
        }

        private void Form_5_Load(object sender, EventArgs e)
        {
            CenterPanel();
            this.Resize += (s, ev) => CenterPanel();

            txtCustomWidth.Text = "3";
            txtCustomHeight.Text = "6";

            // Populate the batch dropdown directly from Form5Data's zone/building keys
            cmbBatch.Items.Clear();
            foreach (string zoneKey in Form5Data.ZoneToMaterials.Keys)
            {
                cmbBatch.Items.Add(zoneKey);
            }
        }

        // --- Invalidation Handlers ---
        private void txtBinLocation1_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtBinLocation2_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void textBox1_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtBinLocation4_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtBinLocation5_TextChanged(object sender, EventArgs e) { /* unused in 2x2 layout */ }
        private void txtBinLocation6_TextChanged(object sender, EventArgs e) { /* unused in 2x2 layout */ }

        private void txtCustomWidth_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }
        private void txtCustomHeight_TextChanged(object sender, EventArgs e) { pnlPreview.Invalidate(); }

        private void cmbBatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBatch.SelectedItem != null)
            {
                // Reset uploaded batch dropdown selection to avoid conflict
                cmbNewBatch5.SelectedIndexChanged -= cmbNewBatch5_SelectedIndexChanged;
                cmbNewBatch5.SelectedIndex = -1;
                cmbNewBatch5.SelectedIndexChanged += cmbNewBatch5_SelectedIndexChanged;

                LoadSelectedZoneIntoTextBoxes(cmbBatch.SelectedItem.ToString(), isUploaded: false);
            }
        }

        private void cmbNewBatch5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbNewBatch5.SelectedItem != null)
            {
                // Reset standard batch dropdown selection to avoid conflict
                cmbBatch.SelectedIndexChanged -= cmbBatch_SelectedIndexChanged;
                cmbBatch.SelectedIndex = -1;
                cmbBatch.SelectedIndexChanged += cmbBatch_SelectedIndexChanged;

                LoadSelectedZoneIntoTextBoxes(cmbNewBatch5.SelectedItem.ToString(), isUploaded: true);
            }
        }

        private void LoadSelectedZoneIntoTextBoxes(string zoneKey, bool isUploaded)
        {
            List<string> materials = null;
            if (isUploaded)
            {
                if (_uploadedZones.TryGetValue(zoneKey, out List<string> list))
                    materials = list;
            }
            else
            {
                if (Form5Data.ZoneToMaterials.TryGetValue(zoneKey, out string[] array))
                    materials = array.ToList();
            }

            if (materials != null)
            {
                // Automatically populate the 4 text boxes for the 2x2 grid preview
                txtBinLocation1.Text = materials.Count > 0 ? materials[0] : string.Empty;
                txtBinLocation2.Text = materials.Count > 1 ? materials[1] : string.Empty;
                textBox1.Text = materials.Count > 2 ? materials[2] : string.Empty;
                txtBinLocation4.Text = materials.Count > 3 ? materials[3] : string.Empty;

                // Update active bins and refresh the preview panel
                _activeBin1 = txtBinLocation1.Text.Trim();
                _activeBin2 = txtBinLocation2.Text.Trim();
                _activeBin3 = textBox1.Text.Trim();
                _activeBin4 = txtBinLocation4.Text.Trim();

                pnlPreview.Invalidate();
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _activeBin1 = txtBinLocation1.Text.Trim();
            _activeBin2 = txtBinLocation2.Text.Trim();
            _activeBin3 = textBox1.Text.Trim();
            _activeBin4 = txtBinLocation4.Text.Trim();

            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) &&
                string.IsNullOrEmpty(_activeBin3) && string.IsNullOrEmpty(_activeBin4))
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
            textBox1.Clear();
            txtBinLocation4.Clear();

            cmbBatch.SelectedIndex = -1;
            cmbNewBatch5.SelectedIndex = -1;

            _activeBin1 = string.Empty;
            _activeBin2 = string.Empty;
            _activeBin3 = string.Empty;
            _activeBin4 = string.Empty;

            pnlPreview.Invalidate();
        }

        private void pnlPreview_Paint(object sender, PaintEventArgs e)
        {
            int totalWidth = pnlPreview.Width;
            int totalHeight = pnlPreview.Height;

            // Prevent crash if the panel is minimized or essentially invisible
            if (totalWidth <= 0 || totalHeight <= 0) return;

            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Size paperSize = GetTargetPaperSizeInHundredths();

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
            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) &&
                string.IsNullOrEmpty(_activeBin3) && string.IsNullOrEmpty(_activeBin4))
            {
                MessageBox.Show("Please enter a Bin Location and click Generate before printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void PrintAllButt_Click(object sender, EventArgs e)
        {
            string selectedZone = null;
            List<string> materials = null;

            // Priority check for uploaded batch dropdown
            if (cmbNewBatch5.SelectedItem != null)
            {
                selectedZone = cmbNewBatch5.SelectedItem.ToString();
                if (_uploadedZones.TryGetValue(selectedZone, out List<string> list))
                {
                    materials = list;
                }
            }
            // Check default warehouse batch dropdown
            else if (cmbBatch.SelectedItem != null)
            {
                selectedZone = cmbBatch.SelectedItem.ToString();
                if (Form5Data.ZoneToMaterials.TryGetValue(selectedZone, out string[] array))
                {
                    materials = array.ToList();
                }
            }

            if (string.IsNullOrEmpty(selectedZone) || materials == null || materials.Count == 0)
            {
                MessageBox.Show("Please select a zone from either the Batch or Uploaded Batch dropdown before printing all.", "Batch Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _batchPages.Clear();

            // Loop steps by 4 now (one page = 4 bins in a 2x2 grid)
            for (int i = 0; i < materials.Count; i += 4)
            {
                string b1 = materials[i];
                string b2 = (i + 1 < materials.Count) ? materials[i + 1] : string.Empty;
                string b3 = (i + 2 < materials.Count) ? materials[i + 2] : string.Empty;
                string b4 = (i + 3 < materials.Count) ? materials[i + 3] : string.Empty;

                _batchPages.Add((b1, b2, b3, b4));
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

        private void btnConvertToPdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) &&
                string.IsNullOrEmpty(_activeBin3) && string.IsNullOrEmpty(_activeBin4))
            {
                MessageBox.Show("Please generate a Bin Location layout before converting to PDF.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDlg.Title = "Save Bin Labels as PDF";
                saveDlg.FileName = "Bin_Labels_Export.pdf";

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

        private Size GetTargetPaperSizeInHundredths()
        {
            double widthInches = 3.0;
            double heightInches = 6.0;

            double.TryParse(txtCustomWidth.Text, out widthInches);
            double.TryParse(txtCustomHeight.Text, out heightInches);

            int w = (int)(widthInches * 100);
            int h = (int)(heightInches * 100);

            if (w <= 0) w = 300;
            if (h <= 0) h = 600;

            return new Size(w, h);
        }

        private void PrintLabelsHandler(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int printableWidth = e.PageBounds.Width;
            int printableHeight = e.PageBounds.Height;

            RenderLabelLayout(g, printableWidth, printableHeight, isPrinting: true);
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
            string savedBin3 = _activeBin3;
            string savedBin4 = _activeBin4;

            _activeBin1 = _batchPages[_batchPageIndex].bin1;
            _activeBin2 = _batchPages[_batchPageIndex].bin2;
            _activeBin3 = _batchPages[_batchPageIndex].bin3;
            _activeBin4 = _batchPages[_batchPageIndex].bin4;

            PrintLabelsHandler(sender, e);

            _activeBin1 = savedBin1;
            _activeBin2 = savedBin2;
            _activeBin3 = savedBin3;
            _activeBin4 = savedBin4;

            _batchPageIndex++;
            e.HasMorePages = _batchPageIndex < _batchPages.Count;
        }

        // 2x2 LAYOUT: 2 columns, 2 rows, vertical divider ONLY (no horizontal line)
        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            int margin = isPrinting ? 20 : 8;
            int safeX = margin;
            int safeY = margin;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);

            // Safety guard to prevent dividing by zero or drawing out of bounds
            if (safeWidth <= 0 || safeHeight <= 0) return;

            // Vertical divider only — rows are separated by space, not a drawn line
            int dividerThickness = 2;
            int columnWidth = (safeWidth - dividerThickness) / 2;

            // Safety guard for column width
            if (columnWidth <= 0) return;

            int rowHeight = safeHeight / 2;
            int rightColumnX = safeX + columnWidth + dividerThickness;
            int bottomRowY = safeY + rowHeight;

            // Draw ONLY the vertical divider line (splits left/right columns)
            using (Pen dividerPen = new Pen(Color.Black, dividerThickness))
            {
                g.DrawLine(dividerPen, safeX + columnWidth, safeY, safeX + columnWidth, safeY + safeHeight);
            }

            string[] bins = { _activeBin1, _activeBin2, _activeBin3, _activeBin4 };

            for (int i = 0; i < 4; i++)
            {
                string bin = bins[i];
                if (string.IsNullOrEmpty(bin)) continue;

                int rowIndex = i / 2;
                int colIndex = i % 2;

                int currentX = (colIndex == 0) ? safeX : rightColumnX;
                int rowTop = (rowIndex == 0) ? safeY : bottomRowY;
                int rowBottom = rowTop + rowHeight;

                int qrPadding = isPrinting ? 6 : 3;
                int qrSize = columnWidth - (qrPadding * 2);

                if (qrSize <= 0) continue;

                int gapBetweenQrAndText = isPrinting ? 4 : 2;
                int textBlockHeight = (int)(rowHeight * 0.18f);
                int contentHeight = qrSize + gapBetweenQrAndText + textBlockHeight;

                int marginFromDivider = isPrinting ? 20 : 10;

                int qrY;
                if (rowIndex == 0)
                {
                    // Top row: anchor the block's BOTTOM near the divider
                    int contentBottom = rowBottom - marginFromDivider;
                    qrY = contentBottom - contentHeight;
                    if (qrY < rowTop) qrY = rowTop; // safety clamp
                }
                else
                {
                    // Bottom row: anchor the block's TOP near the divider
                    qrY = rowTop + marginFromDivider;
                    if (qrY + contentHeight > rowBottom) qrY = rowBottom - contentHeight; // safety clamp
                }

                int qrX = currentX + (columnWidth - qrSize) / 2;

                using (Bitmap qrImg = CreateQRCodeImage(bin))
                {
                    if (qrImg != null)
                    {
                        var oldInterpolation = g.InterpolationMode;
                        var oldSmoothing = g.SmoothingMode;
                        var oldPixelOffset = g.PixelOffsetMode;

                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.SmoothingMode = SmoothingMode.None;
                        g.PixelOffsetMode = PixelOffsetMode.Half;

                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);

                        g.InterpolationMode = oldInterpolation;
                        g.SmoothingMode = oldSmoothing;
                        g.PixelOffsetMode = oldPixelOffset;
                    }
                }

                int textTop = qrY + qrSize + gapBetweenQrAndText;
                int textPadding = 6;
                DrawTextAutofit(g, bin, "Arial", FontStyle.Bold, textBlockHeight * 0.9f,
                    currentX + textPadding, textTop, columnWidth - (textPadding * 2), textBlockHeight);
            }
        }

        private void DrawTextAutofit(Graphics g, string text, string fontFamily, FontStyle style, float maxFontSize, int x, int y, int maxWidth, int maxHeight, Brush brush = null)
        {
            // Safety guards for text rendering
            if (string.IsNullOrEmpty(text) || maxWidth <= 0 || maxHeight <= 0) return;
            if (brush == null) brush = Brushes.Black;

            // Ensure the font size never drops below a valid metric, preventing an ArgumentException crash
            if (maxFontSize < 1f) maxFontSize = 1f;

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
                                byte[] qrCodeBytes = qrCode.GetGraphic(4);
                                using (var ms = new MemoryStream(qrCodeBytes))
                                {
                                    using (var tempBmp = Image.FromStream(ms))
                                    {
                                        return new Bitmap(tempBmp);
                                    }
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

        private void Uploadbutt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Excel Files|*.xls;*.xlsx", ValidateNames = true })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        DataTable dt = ReadExcelToDataTable(ofd.FileName);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            _uploadedZones.Clear();
                            cmbNewBatch5.Items.Clear();

                            string currentZone = null;
                            List<string> currentBins = null;

                            // Walk down the DataTable row by row
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                DataRow row = dt.Rows[i];
                                string zoneVal = row.ItemArray.Length > 0 ? row[0]?.ToString()?.Trim() : null;
                                string binVal = row.ItemArray.Length > 1 ? row[1]?.ToString()?.Trim() : null;

                                // Non-empty cell in Column A marks the start of a new zone
                                if (!string.IsNullOrWhiteSpace(zoneVal))
                                {
                                    if (currentZone != null && currentBins != null)
                                    {
                                        AddZoneToDictionary(currentZone, currentBins);
                                    }

                                    currentZone = zoneVal;
                                    currentBins = new List<string>();
                                }

                                // Collect Column B values for the active zone
                                if (currentZone != null && !string.IsNullOrWhiteSpace(binVal))
                                {
                                    currentBins.Add(binVal);
                                }
                            }

                            // Save the final zone after the loop completes
                            if (currentZone != null && currentBins != null)
                            {
                                AddZoneToDictionary(currentZone, currentBins);
                            }

                            // Populate the ComboBox with zones in top-to-bottom sheet order
                            if (_uploadedZones.Count > 0)
                            {
                                foreach (string zoneName in _uploadedZones.Keys)
                                {
                                    cmbNewBatch5.Items.Add(zoneName);
                                }

                                cmbNewBatch5.SelectedIndex = 0;
                                MessageBox.Show($"Excel uploaded successfully! Found {_uploadedZones.Count} zone(s).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("No valid zones or bin data found in the spreadsheet.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("The selected Excel file is empty.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading Excel file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void AddZoneToDictionary(string zoneName, List<string> bins)
        {
            string uniqueKey = zoneName;
            int counter = 1;
            while (_uploadedZones.ContainsKey(uniqueKey))
            {
                uniqueKey = $"{zoneName} ({counter++})";
            }
            _uploadedZones[uniqueKey] = bins;
        }

        private DataTable ReadExcelToDataTable(string filePath)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                    });
                    return result.Tables[0];
                }
            }
        }

        private int GetColumnIndex(DataTable dt, params string[] possibleHeaders)
        {
            if (dt.Rows.Count == 0) return -1;
            DataRow headerRow = dt.Rows[0];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string cellValue = headerRow[i]?.ToString()?.Trim().ToUpper();
                if (string.IsNullOrEmpty(cellValue)) continue;

                if (IsYnOrFlagColumn(cellValue)) continue;

                foreach (string expected in possibleHeaders)
                {
                    if (cellValue.Contains(expected))
                        return i;
                }
            }
            return -1;
        }

        private int GetZoneColumnIndex(DataTable dt)
        {
            if (dt.Rows.Count == 0) return -1;
            DataRow headerRow = dt.Rows[0];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string cellValue = headerRow[i]?.ToString()?.Trim().ToUpper();
                if (string.IsNullOrEmpty(cellValue)) continue;

                if (IsYnOrFlagColumn(cellValue)) continue;

                if (cellValue.Contains("ZONE") || cellValue.Contains("BUILDING") || cellValue.Contains("AREA"))
                    return i;
            }
            return -1;
        }

        private bool IsYnOrFlagColumn(string headerText)
        {
            return headerText.Contains("Y/N") ||
                   headerText.Contains("YN") ||
                   headerText.Contains("FLAG") ||
                   headerText.Contains("INDICATOR");
        }

        private void Location5_TextChanged(object sender, EventArgs e)
        {
            // Unused
        }
    }
}