using System;

using System.Collections.Generic;

using System.ComponentModel;

using System.Data;

using System.Drawing;

using System.Drawing.Drawing2D;

using System.Drawing.Printing;

using System.Text;

using System.Windows.Forms;

using QRCoder;



namespace OJT___QR_Code_Generator

{

    public partial class Form_5 : Form

    {

        // Bin Location 1 -> txtBinLocation1, Bin Location 2 -> txtBinLocation2,

        // Bin Location 3 -> textBox1 (per the control names already in the designer)

        private string _activeBin1 = string.Empty;

        private string _activeBin2 = string.Empty;

        private string _activeBin3 = string.Empty;



        // Each batch page holds THREE bin values (one per stacked QR section)

        private List<(string bin1, string bin2, string bin3)> _batchPages = new List<(string, string, string)>();

        private int _batchPageIndex = 0;



        public Form_5()

        {

            InitializeComponent();



            // pnlPreview isn't in the stub list you sent, so wiring its Paint event here.

            // If your preview panel has a different name, swap "pnlPreview" below to match.

            this.pnlPreview.Paint += new PaintEventHandler(this.pnlPreview_Paint);

        }



        private void Form_5_Load(object sender, EventArgs e)

        {

            // Same default sizing mechanism as the Bin Location form — 3 x 6 default,

            // still fully editable via the Width/Height textboxes.

            txtCustomWidth.Text = "3";

            txtCustomHeight.Text = "6";



            cmbBatch.Items.Clear();

            for (int i = 1; i <= 26; i++)

            {

                cmbBatch.Items.Add("Zone " + i);

            }

        }



        private void txtBinLocation1_TextChanged(object sender, EventArgs e)

        {

            pnlPreview.Invalidate();

        }



        private void txtBinLocation2_TextChanged(object sender, EventArgs e)

        {

            pnlPreview.Invalidate();

        }



        // Bin Location 3

        private void textBox1_TextChanged(object sender, EventArgs e)

        {

            pnlPreview.Invalidate();

        }



        private void txtCustomWidth_TextChanged(object sender, EventArgs e)

        {

            pnlPreview.Invalidate();

        }



        private void txtCustomHeight_TextChanged(object sender, EventArgs e)

        {

            pnlPreview.Invalidate();

        }



        private void cmbBatch_SelectedIndexChanged(object sender, EventArgs e)

        {

            // Selecting a zone just remembers which one is picked —

            // the actual batch printing happens in PrintAllButt_Click below.

        }



        private void btnGenerate_Click(object sender, EventArgs e)

        {

            _activeBin1 = txtBinLocation1.Text.Trim();

            _activeBin2 = txtBinLocation2.Text.Trim();

            _activeBin3 = textBox1.Text.Trim();



            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) && string.IsNullOrEmpty(_activeBin3))

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

            _activeBin1 = string.Empty;

            _activeBin2 = string.Empty;

            _activeBin3 = string.Empty;

            pnlPreview.Invalidate();

        }



        // FIX: previously this computed a correct portrait-fitted box (narrow width,

        // tall height, matching the 3x6 label) and then immediately swapped it back

        // to landscape, which squished the 3 stacked sections into thin strips.

        // The label is portrait, so the preview canvas must stay portrait.

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

                // Panel is relatively wider than the label -> constrain by height

                totalWidth = (int)(totalHeight * targetRatio);

            }

            else

            {

                // Panel is relatively taller than the label -> constrain by width

                totalHeight = (int)(totalWidth / targetRatio);

            }



            // NOTE: no forced swap here anymore — keep whatever orientation the

            // label's actual dimensions dictate (portrait 3x6 stays portrait).



            RenderLabelLayout(g, totalWidth, totalHeight, isPrinting: false);

        }



        private void btnPrint_Click(object sender, EventArgs e)

        {

            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) && string.IsNullOrEmpty(_activeBin3))

            {

                MessageBox.Show("Please enter a Bin Location and click Generate before printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            Size paperSize = GetTargetPaperSizeInHundredths();



            using (PrintDocument pd = new PrintDocument())

            {

                // FIX: was "(W > H) || (H > W)" which is true for almost any non-square

                // label, forcing Landscape=true even for a portrait 3x6 label.

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



        // Batch printing: groups the selected zone's bins into sets of THREE per label page

        private void PrintAllButt_Click(object sender, EventArgs e)

        {

            if (cmbBatch.SelectedItem == null)

            {

                MessageBox.Show("Please select a Zone from the Batch dropdown.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            string selected = cmbBatch.SelectedItem.ToString();

            int zoneNum;

            if (!int.TryParse(selected.Replace("Zone ", "").Trim(), out zoneNum) || !WarehouseData.Zones.ContainsKey(zoneNum))

            {

                MessageBox.Show("Selected zone has no bin location data.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            string[] bins = WarehouseData.Zones[zoneNum];

            if (bins == null || bins.Length == 0)

            {

                MessageBox.Show("Selected zone has no bin locations.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            _batchPages.Clear();

            for (int i = 0; i < bins.Length; i += 3)

            {

                string bin1 = bins[i];

                string bin2 = (i + 1 < bins.Length) ? bins[i + 1] : string.Empty;

                string bin3 = (i + 2 < bins.Length) ? bins[i + 2] : string.Empty;

                _batchPages.Add((bin1, bin2, bin3));

            }



            Size paperSize = GetTargetPaperSizeInHundredths();



            using (PrintDocument pd = new PrintDocument())

            {

                // FIX: same corrected Landscape logic as btnPrint_Click.

                pd.DefaultPageSettings.Landscape = paperSize.Width > paperSize.Height;

                pd.DefaultPageSettings.PaperSize = new PaperSize("CustomSticker", paperSize.Width, paperSize.Height);



                // Reset the page cursor every time this document starts a print/preview pass —

                // PrintPreviewDialog can trigger BeginPrint more than once (resize, zoom, view switch)

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

            if (string.IsNullOrEmpty(_activeBin1) && string.IsNullOrEmpty(_activeBin2) && string.IsNullOrEmpty(_activeBin3))

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

                        // FIX: same corrected Landscape logic.

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



        // Identical logic/defaults to the Bin Location form's paper-size handling

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



        // FIX: removed the "rotate 90 if width < height" block. The label is portrait

        // (narrower than tall) and RenderLabelLayout already stacks the 3 sections

        // top-to-bottom for exactly that shape. Rotating here — on top of the

        // Landscape flag fix above — was what turned the 3 stacked sections into

        // 3 side-by-side (parallel) columns on the physical printout.

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



            // Temporarily swap in this page's bin values so the existing render path handles it

            string savedBin1 = _activeBin1;

            string savedBin2 = _activeBin2;

            string savedBin3 = _activeBin3;



            _activeBin1 = _batchPages[_batchPageIndex].bin1;

            _activeBin2 = _batchPages[_batchPageIndex].bin2;

            _activeBin3 = _batchPages[_batchPageIndex].bin3;



            PrintLabelsHandler(sender, e); // exact same layout call as manual mode, no rotation



            _activeBin1 = savedBin1;

            _activeBin2 = savedBin2;

            _activeBin3 = savedBin3;



            _batchPageIndex++;

            e.HasMorePages = _batchPageIndex < _batchPages.Count;

        }



        // TRIPLE-QR STACKED LAYOUT: 3 stacked sections, each with its own QR on top

        // and its bin code text below it — matching the "[QR] / A-01-01" sketch.

        private void RenderLabelLayout(Graphics g, int totalWidth, int totalHeight, bool isPrinting)

        {

            // Safeguard border cutoff zone on physical thermal heads (Honeywell standard unprintable margins)

            int margin = isPrinting ? 20 : 8;



            int safeX = margin;

            int safeY = margin;

            int safeWidth = totalWidth - (margin * 2);

            int safeHeight = totalHeight - (margin * 2);

            int rowHeight = safeHeight / 3;





            string[] bins = { _activeBin1, _activeBin2, _activeBin3 };



            for (int i = 0; i < 3; i++)

            {

                string bin = bins[i];

                if (string.IsNullOrEmpty(bin)) continue;



                int rowTop = safeY + (rowHeight * i);



                // Each row: QR takes the top ~65%, bin text takes the bottom ~35%

                int qrAreaHeight = (int)(rowHeight * 0.65);

                int textAreaHeight = rowHeight - qrAreaHeight;



                int qrPadding = isPrinting ? 8 : 4;

                int qrSize = Math.Min(qrAreaHeight, safeWidth) - (qrPadding * 2);



                using (Bitmap qrImg = CreateQRCodeImage(bin))

                {

                    if (qrImg != null)

                    {

                        int qrX = safeX + (safeWidth - qrSize) / 2;

                        int qrY = rowTop + (qrAreaHeight - qrSize) / 2;

                        g.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);

                    }

                }



                int textTop = rowTop + qrAreaHeight;

                int textPadding = 6;

                DrawTextAutofit(g, bin, "Arial", FontStyle.Bold, textAreaHeight * 0.8f,

                    safeX + textPadding, textTop, safeWidth - (textPadding * 2), textAreaHeight);

            }

        }



        // Adaptive font crunching ruleset to ensure maximum enlargement without bounds breaches

        private void DrawTextAutofit(Graphics g, string text, string fontFamily, FontStyle style, float maxFontSize, int x, int y, int maxWidth, int maxHeight, Brush brush = null)

        {

            if (brush == null) brush = Brushes.Black;



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



        private void txtBinLocation4_TextChanged(object sender, EventArgs e)

        {



        }



        private void txtBinLocation5_TextChanged(object sender, EventArgs e)

        {



        }



        private void txtBinLocation6_TextChanged(object sender, EventArgs e)

        {



        }

    }

}