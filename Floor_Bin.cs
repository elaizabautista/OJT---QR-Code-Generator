using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
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

            // Connect the ComboBox change event
            this.cmbBatch.SelectedIndexChanged += new EventHandler(this.cmbWhaZones_SelectedIndexChanged);
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

            // Populate the ComboBox directly from WarehouseData, instead of hardcoding
            // zone names here. This automatically includes every zone that exists
            // in the data and stays correct if more zones get added to WarehouseData
            // later without touching this form again.
            cmbBatch.Items.Clear();

            List<object> sortedZones = WarehouseData.myZones.Keys.ToList();
            sortedZones.Sort((x, y) => GetZoneSortKey(x.ToString()).CompareTo(GetZoneSortKey(y.ToString())));

            foreach (object zone in sortedZones)
            {
                cmbBatch.Items.Add(zone.ToString());
            }
        }

        // Keeps WHA01..WHA15 in numeric order, followed by WHA-CP, WHA-FM, then WHC.
        private static int GetZoneSortKey(string zone)
        {
            if (zone.Length == 5 && zone.StartsWith("WHA") && int.TryParse(zone.Substring(3), out int n))
                return n; // WHA01..WHA15 -> 1..15

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

            string selectedZone = cmbBatch.SelectedItem.ToString(); // e.g., "WHA01" or "WHA-CP"

            // Direct dictionary lookup - no more scanning every key with StartsWith.
            // Preview the first bin code in the zone so the user has something to look at
            // right after picking a zone; Print All still prints every code in the zone.
            if (WarehouseData.myZones.TryGetValue(selectedZone, out string[] binsInZone) &&
                binsInZone.Length > 0)
            {
                txtNumber.Text = binsInZone[0];
                _activeNumber = txtNumber.Text.Trim();
                pnlPreview.Invalidate();
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
            cmbBatch.SelectedIndex = -1; // Reset combobox selection
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
            if (cmbBatch.SelectedItem == null)
            {
                MessageBox.Show("Please select a WHA zone from the Batch dropdown before printing all.", "Batch Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedZone = cmbBatch.SelectedItem.ToString(); // e.g., "WHA01" or "WHA-CP"

            // Direct dictionary lookup instead of scanning every key with StartsWith.
            if (!WarehouseData.myZones.TryGetValue(selectedZone, out string[] binsInZone) ||
                binsInZone.Length == 0)
            {
                MessageBox.Show($"No bin codes found for {selectedZone}.", "Batch Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _batchQueue = binsInZone.ToList();
            _batchIndex = 0;

            Size paperSize = GetTargetPaperSizeInHundredths();

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DefaultPageSettings.Landscape = true;
                pd.DefaultPageSettings.PaperSize = new PaperSize("FloorBinSticker", paperSize.Width, paperSize.Height);

                // ROOT-CAUSE FIX: PrintPreviewDialog internally calls pd.Print() TWICE —
                // once to build the on-screen preview, and again for the real print job
                // when the user clicks Print inside the preview window. BeginPrint fires
                // at the start of BOTH passes, so this guarantees _batchIndex is reset to 0
                // before the real print pass too, instead of inheriting the exhausted index
                // left behind by the preview pass (which was causing an out-of-range crash).
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

        // Multi-page handler for "Print All" - prints one label per page,
        // advancing through _batchQueue and setting HasMorePages until done
        private void PrintAllFloorBinHandler(object sender, PrintPageEventArgs e)
        {
            // SAFETY GUARD: prevents ArgumentOutOfRangeException on _batchQueue[_batchIndex]
            // below if this handler is ever invoked with an exhausted index.
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

            // Temporarily swap the active number to whichever bin code is next in the queue,
            // reusing the exact same render method as single-label printing
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

            int moveTextUpPixels = 30;
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
            int borderThickness = isPrinting ? 3 : 2; // thinner, as requested
            using (SolidBrush borderBrush = new SolidBrush(Color.Black))
            {
                g.FillRectangle(borderBrush, safeX, safeY, safeWidth, borderThickness); // top
                g.FillRectangle(borderBrush, safeX, safeY + safeHeight - borderThickness, safeWidth, borderThickness); // bottom
                g.FillRectangle(borderBrush, safeX, safeY, borderThickness, safeHeight); // left
                g.FillRectangle(borderBrush, safeX + safeWidth - borderThickness, safeY, borderThickness, safeHeight); // right
            }
        }

        private void DrawTextAutofit(Graphics g, string text, string fontFamily, FontStyle style, float maxFontSize, int x, int y, int maxWidth, int maxHeight)
        {
            StringFormat format = StringFormat.GenericTypographic; // add this

            float currentSize = maxFontSize;
            Font testFont = new Font(fontFamily, currentSize, style);
            SizeF size = g.MeasureString(text, testFont, int.MaxValue, format); // pass format

            while ((size.Width > maxWidth || size.Height > maxHeight) && currentSize > 8f)
            {
                currentSize -= 1f;
                testFont.Dispose();
                testFont = new Font(fontFamily, currentSize, style);
                size = g.MeasureString(text, testFont, int.MaxValue, format); // pass format
            }

            using (testFont)
            {
                float posX = x + (maxWidth - size.Width) / 2;
                float posY = y + (maxHeight - size.Height) / 2;
                g.DrawString(text, testFont, Brushes.Black, posX, posY, format); // pass format
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