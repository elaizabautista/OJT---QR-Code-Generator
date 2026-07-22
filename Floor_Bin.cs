using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExcelDataReader;
using QRCoder;

namespace OJT___QR_Code_Generator
{
    public partial class Floor_Bin : Form
    {
        private string _activeNumber = string.Empty;

        // Holds the list of codes queued up for a "Print All" batch job,
        // plus the index of which one is currently being printed
        private List<string> _batchQueue = new List<string>();
        private int _batchIndex = 0;

        // Stores parsed zones and bin lists from uploaded Excel files
        private Dictionary<string, List<string>> _uploadedZones = new Dictionary<string, List<string>>();

        public Floor_Bin()
        {
            InitializeComponent();

            this.btnGenerate.Click += new EventHandler(this.btnGenerate_Click);
            this.btnClear.Click += new EventHandler(this.btnClear_Click);
            this.btnPrint.Click += new EventHandler(this.btnPrint_Click);
            this.btnConvertToPdf.Click += new EventHandler(this.btnConvertToPdf_Click);
            this.btnPrintAll.Click += new EventHandler(this.btnPrintAll_Click);
            this.pnlPreview.Paint += new PaintEventHandler(this.pnlPreview_Paint);

            this.txtCustomWidth.TextChanged += (s, e) => pnlPreview.Invalidate();
            this.txtCustomHeight.TextChanged += (s, e) => pnlPreview.Invalidate();

            // Connect the ComboBox & Upload change events
            this.cmbBatch.SelectedIndexChanged += new EventHandler(this.cmbWhaZones_SelectedIndexChanged);
            this.cmbNewBatch4.SelectedIndexChanged += new EventHandler(this.cmbNewBatch4_SelectedIndexChanged);
            this.Uploadbutt.Click += new EventHandler(this.Uploadbutt_Click);
        }

        private void CenterPanel()
        {
            panel4.Left = (this.ClientSize.Width - panel4.Width) / 2;
            panel4.Top = (this.ClientSize.Height - panel4.Height) / 2;
        }

        private void Floor_Bin_Load(object sender, EventArgs e)
        {
            CenterPanel();
            this.Resize += (s, e) => CenterPanel();

            txtCustomWidth.Text = "4";
            txtCustomHeight.Text = "6";

            // Populate default WarehouseData zones
            cmbBatch.Items.Clear();

            List<object> sortedZones = WarehouseData.myZones.Keys.ToList();
            sortedZones.Sort((x, y) => GetZoneSortKey(x.ToString()).CompareTo(GetZoneSortKey(y.ToString())));

            foreach (object zone in sortedZones)
            {
                cmbBatch.Items.Add(zone.ToString());
            }
        }

        private static int GetZoneSortKey(string zone)
        {
            if (zone.Length == 5 && zone.StartsWith("WHA") && int.TryParse(zone.Substring(3), out int n))
                return n;

            switch (zone)
            {
                case "WHA-CP": return 100;
                case "WHA-FM": return 101;
                case "WHC": return 102;
                default: return 999;
            }
        }

        private void cmbWhaZones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBatch.SelectedItem == null) return;

            cmbNewBatch4.SelectedIndexChanged -= cmbNewBatch4_SelectedIndexChanged;
            cmbNewBatch4.SelectedIndex = -1;
            cmbNewBatch4.SelectedIndexChanged += cmbNewBatch4_SelectedIndexChanged;

            string selectedZone = cmbBatch.SelectedItem.ToString();

            if (WarehouseData.myZones.TryGetValue(selectedZone, out string[] binsInZone) && binsInZone.Length > 0)
            {
                txtNumber.Text = binsInZone[0];
                _activeNumber = txtNumber.Text.Trim();
                pnlPreview.Invalidate();
            }
        }

        private void cmbNewBatch4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbNewBatch4.SelectedItem == null) return;

            cmbBatch.SelectedIndexChanged -= cmbWhaZones_SelectedIndexChanged;
            cmbBatch.SelectedIndex = -1;
            cmbBatch.SelectedIndexChanged += cmbWhaZones_SelectedIndexChanged;

            string selectedZone = cmbNewBatch4.SelectedItem.ToString();

            if (_uploadedZones.TryGetValue(selectedZone, out List<string> binsInZone) && binsInZone.Count > 0)
            {
                txtNumber.Text = binsInZone[0];
                _activeNumber = txtNumber.Text.Trim();
                pnlPreview.Invalidate();
            }
        }

        private void Uploadbutt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|All Files (*.*)|*.*";
                openFileDialog.Title = "Select Excel File to Upload";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        DataTable dt = ReadExcelToDataTable(openFileDialog.FileName);
                        if (dt == null || dt.Rows.Count == 0)
                        {
                            MessageBox.Show("The selected Excel file is empty or could not be read.", "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        int codeColIdx = GetColumnIndex(dt.Columns, "Bin Location", "Storage Bin", "Location", "Code", "Bin", "Floor Bin");
                        if (codeColIdx == -1)
                        {
                            MessageBox.Show("Could not locate a valid Bin Location / Code column in the Excel file.", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        int zoneColIdx = GetZoneColumnIndex(dt, codeColIdx);

                        _uploadedZones.Clear();
                        cmbNewBatch4.Items.Clear();

                        string lastSeenZone = string.Empty;

                        foreach (DataRow row in dt.Rows)
                        {
                            string code = (codeColIdx >= 0 && row[codeColIdx] != DBNull.Value) ? row[codeColIdx].ToString().Trim() : string.Empty;
                            string zoneCell = (zoneColIdx >= 0 && zoneColIdx < dt.Columns.Count && row[zoneColIdx] != DBNull.Value) ? row[zoneColIdx].ToString().Trim() : string.Empty;

                            if (!string.IsNullOrEmpty(zoneCell))
                            {
                                lastSeenZone = zoneCell;
                            }

                            if (string.IsNullOrEmpty(code)) continue;

                            string zone = string.IsNullOrEmpty(lastSeenZone) ? "Unassigned" : lastSeenZone;

                            if (!_uploadedZones.ContainsKey(zone))
                            {
                                _uploadedZones[zone] = new List<string>();
                                cmbNewBatch4.Items.Add(zone);
                            }

                            if (!_uploadedZones[zone].Contains(code))
                            {
                                _uploadedZones[zone].Add(code);
                            }
                        }

                        if (cmbNewBatch4.Items.Count > 0)
                        {
                            cmbNewBatch4.SelectedIndex = 0;
                            MessageBox.Show($"Successfully uploaded and loaded {cmbNewBatch4.Items.Count} zone(s) from Excel file!", "Upload Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No valid bin records found in the file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading Excel file: {ex.Message}", "Upload Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #region Excel Parsing Helpers

        private DataTable ReadExcelToDataTable(string filePath)
        {
            // Register encoding provider to fix "No data is available for encoding 1252" error
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });

                    if (result.Tables.Count > 0)
                        return result.Tables[0];
                }
            }
            return null;
        }

        private bool IsYnOrFlagColumn(string colName)
        {
            if (string.IsNullOrWhiteSpace(colName)) return false;

            string clean = colName.ToLower().Trim();
            string[] ignoreKeywords = new string[] { "y/n", "(y/n)", "yes/no", "status", "active", "flag", "indicator", "required" };

            return ignoreKeywords.Any(kw => clean.Contains(kw));
        }

        private int GetColumnIndex(DataColumnCollection columns, params string[] possibleNames)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                string colName = columns[i].ColumnName.Trim();
                if (IsYnOrFlagColumn(colName)) continue;

                foreach (string name in possibleNames)
                {
                    if (string.Equals(colName, name, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }

            for (int i = 0; i < columns.Count; i++)
            {
                string colName = columns[i].ColumnName.Replace(" ", "").ToLower();
                if (IsYnOrFlagColumn(columns[i].ColumnName)) continue;

                foreach (string possibleName in possibleNames)
                {
                    string cleanPossible = possibleName.Replace(" ", "").ToLower();
                    if (colName.Contains(cleanPossible))
                        return i;
                }
            }

            return -1;
        }

        private int GetZoneColumnIndex(DataTable dt, int codeColIdx)
        {
            int zoneCol = GetColumnIndex(dt.Columns, "Zone", "Zone Name", "Area", "Rack Zone", "Zone Header", "Storage Section", "Storage Type");
            if (zoneCol != -1 && zoneCol != codeColIdx) return zoneCol;

            if (codeColIdx == 1 && dt.Columns.Count > 0)
            {
                return 0;
            }

            return -1;
        }

        #endregion

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
            _activeNumber = txtNumber.Text.Trim();

            if (string.IsNullOrEmpty(_activeNumber))
            {
                MessageBox.Show("Please enter a Number to generate.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            pnlPreview.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtNumber.Clear();
            cmbBatch.SelectedIndex = -1;
            cmbNewBatch4.SelectedIndex = -1;
            _activeNumber = string.Empty;
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

            RenderFloorBinLabel(g, totalWidth, totalHeight, isPrinting: false);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activeNumber))
            {
                MessageBox.Show("Please enter a Number and click Generate before printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Landscape = true;
                pd.DefaultPageSettings.PaperSize = new PaperSize("FloorBinSticker", paperSize.Width, paperSize.Height);
                pd.PrintPage += new PrintPageEventHandler(PrintFloorBinHandler);

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
            if (string.IsNullOrEmpty(_activeNumber))
            {
                MessageBox.Show("Please generate a Floor Bin label before converting to PDF.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDlg.Title = "Save Floor Bin Label as PDF";
                saveDlg.FileName = $"FloorBin_{_activeNumber.Replace("-", "_")}";

                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    using (PrintDocument pd = new PrintDocument())
                    {
                        pd.DefaultPageSettings.Landscape = true;
                        pd.DefaultPageSettings.PaperSize = new PaperSize("FloorBinSticker", paperSize.Width, paperSize.Height);
                        pd.OriginAtMargins = false;

                        pd.PrintPage += new PrintPageEventHandler(PrintFloorBinHandler);
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
            string selectedZone = null;
            List<string> binsToPrint = null;

            if (cmbNewBatch4.SelectedItem != null)
            {
                selectedZone = cmbNewBatch4.SelectedItem.ToString();
                if (_uploadedZones.TryGetValue(selectedZone, out List<string> list))
                {
                    binsToPrint = list;
                }
            }
            else if (cmbBatch.SelectedItem != null)
            {
                selectedZone = cmbBatch.SelectedItem.ToString();
                if (WarehouseData.myZones.TryGetValue(selectedZone, out string[] array))
                {
                    binsToPrint = array.ToList();
                }
            }

            if (string.IsNullOrEmpty(selectedZone) || binsToPrint == null || binsToPrint.Count == 0)
            {
                MessageBox.Show("Please select a zone from either the Batch or Uploaded Batch dropdown before printing all.", "Batch Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _batchQueue = binsToPrint;
            _batchIndex = 0;

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Landscape = true;
                pd.DefaultPageSettings.PaperSize = new PaperSize("FloorBinSticker", paperSize.Width, paperSize.Height);

                pd.BeginPrint += (s, ev) => { _batchIndex = 0; };
                pd.PrintPage += new PrintPageEventHandler(PrintAllFloorBinHandler);

                using (PrintPreviewDialog previewDlg = new PrintPreviewDialog())
                {
                    previewDlg.Document = pd;
                    previewDlg.WindowState = FormWindowState.Maximized;
                    previewDlg.ShowDialog();
                }
            }
        }

        private void PrintFloorBinHandler(object sender, PrintPageEventArgs e)
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

            RenderFloorBinLabel(g, printableWidth, printableHeight, isPrinting: true);
        }

        private void PrintAllFloorBinHandler(object sender, PrintPageEventArgs e)
        {
            if (_batchIndex >= _batchQueue.Count)
            {
                e.HasMorePages = false;
                return;
            }

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

            string previousActiveNumber = _activeNumber;
            _activeNumber = _batchQueue[_batchIndex];

            RenderFloorBinLabel(g, printableWidth, printableHeight, isPrinting: true);

            _activeNumber = previousActiveNumber;

            _batchIndex++;
            e.HasMorePages = _batchIndex < _batchQueue.Count;
        }

        private void RenderFloorBinLabel(Graphics g, int totalWidth, int totalHeight, bool isPrinting)
        {
            int margin = isPrinting ? 10 : 5;
            int safeX = margin;
            int safeY = margin;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);

            if (string.IsNullOrEmpty(_activeNumber))
            {
                DrawBorder(g, safeX, safeY, safeWidth, safeHeight, isPrinting);
                return;
            }

            int gap = 4;
            int textHeight = (int)(safeHeight * 0.27f);
            float maxFontCeiling = textHeight;
            int qrSizeByHeight = safeHeight - gap - textHeight;
            int qrSizeByWidth = safeWidth;
            int qrSize = (int)(Math.Min(qrSizeByHeight, qrSizeByWidth) * 1.22f);

            int moveTextUpPixels = 40;
            int contentHeight = qrSize + gap + textHeight;
            int blockStartY = safeY + Math.Max(0, (safeHeight - contentHeight) / 2);

            int qrX = safeX + (safeWidth - qrSize) / 2;
            int qrY = blockStartY - 15;

            using (Bitmap qrImg = CreateQRCodeImage(_activeNumber))
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

            int textY = qrY + qrSize + gap;
            textY -= moveTextUpPixels;

            DrawTextAutofit(g, _activeNumber, "Arial", FontStyle.Bold, maxFontCeiling, safeX, textY, safeWidth, textHeight);
            DrawBorder(g, safeX, safeY, safeWidth, safeHeight, isPrinting);
        }

        private void DrawBorder(Graphics g, int safeX, int safeY, int safeWidth, int safeHeight, bool isPrinting)
        {
            int borderThickness = isPrinting ? 3 : 2;
            using (SolidBrush borderBrush = new SolidBrush(Color.Black))
            {
                g.FillRectangle(borderBrush, safeX, safeY, safeWidth, borderThickness);
                g.FillRectangle(borderBrush, safeX, safeY + safeHeight - borderThickness, safeWidth, borderThickness);
                g.FillRectangle(borderBrush, safeX, safeY, borderThickness, safeHeight);
                g.FillRectangle(borderBrush, safeX + safeWidth - borderThickness, safeY, borderThickness, safeHeight);
            }
        }

        private void DrawTextAutofit(Graphics g, string text, string fontFamily, FontStyle style, float maxFontSize, int x, int y, int maxWidth, int maxHeight)
        {
            StringFormat format = StringFormat.GenericTypographic;

            float currentSize = maxFontSize;
            Font testFont = new Font(fontFamily, currentSize, style);
            SizeF size = g.MeasureString(text, testFont, int.MaxValue, format);

            while ((size.Width > maxWidth || size.Height > maxHeight) && currentSize > 8f)
            {
                currentSize -= 1f;
                testFont.Dispose();
                testFont = new Font(fontFamily, currentSize, style);
                size = g.MeasureString(text, testFont, int.MaxValue, format);
            }

            using (testFont)
            {
                float posX = x + (maxWidth - size.Width) / 2;
                float posY = y + (maxHeight - size.Height) / 2;
                g.DrawString(text, testFont, Brushes.Black, posX, posY, format);
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
                            byte[] qrCodeBytes = qrCode.GetGraphic(20);
                            using (var ms = new MemoryStream(qrCodeBytes))
                            {
                                return new Bitmap(ms);
                            }
                        }
                    }
                    catch
                    {
                        using (QRCode qrCode = new QRCode(qrCodeData))
                        {
                            return qrCode.GetGraphic(20, Color.Black, Color.White, true);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}