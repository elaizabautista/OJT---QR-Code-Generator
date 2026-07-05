using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using QRCoder;

namespace OJT___QR_Code_Generator
{
    public partial class Finished_Goods : Form
    {
        private string _activeNumber1 = string.Empty;
        private string _activeNumber2 = string.Empty;

        public Finished_Goods()
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
        }

        private void Finished_Goods_Load(object sender, EventArgs e)
        {
            txtCustomWidth.Text = "4";
            txtCustomHeight.Text = "6";
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
            // NOTE: rename txtNumber1 / txtNumber2 to whatever your two
            // designer textboxes are actually called (e.g. txtBinLocation1 / txtBinLocation2)
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
            MessageBox.Show("Batch printing for Finished Goods isn't available yet - the data source hasn't been set up. Use manual mode (Generate + Print) for now.", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            int margin = 0;
            int safeWidth = totalWidth - (margin * 2);
            int safeHeight = totalHeight - (margin * 2);

            if (string.IsNullOrEmpty(_activeNumber1) || string.IsNullOrEmpty(_activeNumber2))
                return;

            int rowHeight = safeHeight / 2;

            // Row 1 (top) - first code
            RenderCodeRow(g, _activeNumber1, margin, margin, safeWidth, rowHeight);

            // Row 2 (bottom) - second code
            RenderCodeRow(g, _activeNumber2, margin, margin + rowHeight, safeWidth, rowHeight);

            // Thin divider line between the two rows
            using (Pen dividerPen = new Pen(Color.Black, isPrinting ? 2f : 1f))
            {
                int dividerY = margin + rowHeight;
                g.DrawLine(dividerPen, margin, dividerY, margin + safeWidth, dividerY);
            }
        }

        private void RenderCodeRow(Graphics g, string codeValue, int rowX, int rowY, int rowWidth, int rowHeight)
        {
            if (string.IsNullOrEmpty(codeValue))
                return;

            // Tiny inner pad just so the QR doesn't touch the very edge of the sheet
            int innerPad = 2;

            int safeX = rowX + innerPad;
            int safeY = rowY + innerPad;
            int safeWidth = rowWidth - (innerPad * 2);
            int safeHeight = rowHeight - (innerPad * 2);

            // QR maximized against the row height, sitting on the right
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

            // Text fills the remaining width to the left of the QR
            int textGap = (int)(safeHeight * 0.03f);
            int textWidth = qrX - safeX - textGap;

            if (textWidth > 0)
            {
                DrawTextAutofit(g, codeValue, "Arial", FontStyle.Bold, safeHeight, safeX, safeY, textWidth, safeHeight);
            }
        }

        private void RenderQRBlock(Graphics g, string codeValue, int blockX, int blockY, int blockWidth, int blockHeight)
        {
            if (string.IsNullOrEmpty(codeValue))
                return;

            // Small inner pad so the QR doesn't butt right up against the
            // cut/fold line between the two halves
            int innerPad = 4;

            int safeX = blockX + innerPad;
            int safeY = blockY + innerPad;
            int safeWidth = blockWidth - (innerPad * 2);
            int safeHeight = blockHeight - (innerPad * 2);

            float qrHeightPercentage = 0.85f;
            float textHeightPercentage = 0.22f;

            int gap = (int)(safeHeight * 0.02f);

            // Maximize QR - capped by whichever dimension is tighter in this half
            int qrSize = (int)(safeHeight * qrHeightPercentage);
            qrSize = Math.Min(qrSize, (int)(safeWidth * 0.98f));

            int textHeight = (int)(safeHeight * textHeightPercentage);

            int contentHeight = qrSize + gap + textHeight;
            int blockStartY = safeY + (safeHeight - contentHeight) / 2;

            int qrX = safeX + (safeWidth - qrSize) / 2;
            int qrY = blockStartY;

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

            int textY = qrY + qrSize + gap;

            DrawTextAutofit(g, codeValue, "Arial", FontStyle.Bold, textHeight, safeX, textY, safeWidth, textHeight);
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
                            // drawQuietZones: false removes the built-in white margin baked into
                            // the bitmap, so the QR modules fill the entire image with no dead space
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
    }
}
