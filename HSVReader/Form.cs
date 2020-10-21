using IronOcr;
using System;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using OfficeOpenXml;
using System.IO;
using System.Collections.Generic;

namespace HSVReader
{
    public partial class Form : System.Windows.Forms.Form
    {
        private DataGridViewCell currentCell;
        private readonly AutoOcr Ocr;
        private readonly HSVDB DB;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Return)
            {
                performReadAndSave();

                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public Form()
        {
            InitializeComponent();

            Ocr = new AutoOcr();

            DB = new HSVDB();

            initTable();
        }

        private void initTable()
        {
            table.Rows.Add(15);

            foreach (DataGridViewRow row in table.Rows)
            {
                row.HeaderCell.Value = string.Format("{0}", 16 - row.Index);
            }

            DB.HSVTable.ForEachAsync(c => table.Rows[16 - c.Y].Cells[c.X - 1].Style.BackColor = ColorFromHSV(c.H * 360, c.S, c.V)).GetAwaiter().GetResult();
        }

        private void table_SelectionChanged(object sender, EventArgs e)
        {
            currentCell = table.CurrentCell;

            int X = currentCell.ColumnIndex + 1;
            int Y = 16 - currentCell.RowIndex;

            HSV hsv = DB.HSVTable.Where(v => v.X == X && v.Y == Y).FirstOrDefault();
            if (hsv == null)
            {
                label9.Text = "H: ";
                label8.Text = "S: ";
                label7.Text = "V: ";
                return;
            }
            label9.Text = "H: " + hsv.H;
            label8.Text = "S: " + hsv.S;
            label7.Text = "V: " + hsv.V;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private Bitmap getScreenImage()
        {
            //Create a new bitmap.
            var screen = new Bitmap(500, 50, PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(screen);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(10, 150, 0, 0, new Size(400, 50), CopyPixelOperation.SourceCopy);

            pictureBox1.Image = screen;

            return screen;
        }

        private void performReadAndSave()
        {
            var res = Ocr.Read(getScreenImage());
            string s = res.Text;
            HSV hsv;
            if (string.IsNullOrWhiteSpace(s) || string.IsNullOrEmpty(s) || s.Length < 29)
            {
                hsv = new HSV()
                {
                    H = -1,
                    S = -1,
                    V = -1,
                    X = currentCell.ColumnIndex + 1,
                    Y = 16 - currentCell.RowIndex
                };

                label1.Text = "H: " + hsv.H;
                label2.Text = "S: " + hsv.S;
                label3.Text = "V: " + hsv.V;
                label4.Text = "X: " + hsv.X;
                label5.Text = "Y: " + hsv.Y;

                label6.Text = hsv.X + "-" + hsv.Y + " NOT registered!";

                return;
            }
            else
            {
                hsv = new HSV(s)
                {
                    X = currentCell.ColumnIndex + 1,
                    Y = 16 - currentCell.RowIndex
                };
            }

            label1.Text = "H: " + hsv.H;
            label2.Text = "S: " + hsv.S;
            label3.Text = "V: " + hsv.V;
            label4.Text = "X: " + hsv.X;
            label5.Text = "Y: " + hsv.Y;

            DB.registerHSV(hsv);

            label6.Text = hsv.X + "-" + hsv.Y + " registered!";

            Color c = ColorFromHSV(hsv.H * 360, hsv.S, 0.8);

            table.Rows[currentCell.RowIndex].Cells[currentCell.ColumnIndex].Style.BackColor = c;
        }

        private void createExcelTable()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Worksheets.Add("Worksheet1");

                string[,] matrix = new string[16, 16];

                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        var hsv = DB.HSVTable.Where(v => v.X - 1 == x && v.Y - 1 == y).FirstOrDefault();

                        if (hsv != null) matrix[15 - y, x] = hsv.getString();
                        else matrix[15 - y, x] = "X";
                    }
                }


                string[] row1 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[0, x]).ToArray();
                string[] row2 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[1, x]).ToArray();
                string[] row3 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[2, x]).ToArray();
                string[] row4 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[3, x]).ToArray();
                string[] row5 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[4, x]).ToArray();
                string[] row6 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[5, x]).ToArray();
                string[] row7 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[6, x]).ToArray();
                string[] row8 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[7, x]).ToArray();
                string[] row9 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[8, x]).ToArray();
                string[] row10 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[9, x]).ToArray();
                string[] row11 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[10, x]).ToArray();
                string[] row12 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[11, x]).ToArray();
                string[] row13 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[12, x]).ToArray();
                string[] row14 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[13, x]).ToArray();
                string[] row15 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[14, x]).ToArray();
                string[] row16 = Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[15, x]).ToArray();

                var headerRow = new List<string[]>() { row1, row2, row3, row4, row5, row6, row7, row8, row9, row10, row11, row12, row13, row14, row15, row16 };

                // Determine the header range (e.g. A1:D1)
                string r1 = "A1:P1";
                string r2 = "A2:P2";
                string r3 = "A3:P3";
                string r4 = "A4:P4";
                string r5 = "A5:P5";
                string r6 = "A6:P6";
                string r7 = "A7:P7";
                string r8 = "A8:P8";
                string r9 = "A9:P9";
                string r10 = "A10:P10";
                string r11 = "A11:P11";
                string r12 = "A12:P12";
                string r13 = "A13:P13";
                string r14 = "A14:P14";
                string r15 = "A15:P15";
                string r16 = "A16:P16";

                // Target a worksheet
                var worksheet = excel.Workbook.Worksheets["Worksheet1"];
                worksheet.Cells.Style.WrapText = true;
                // Popular header row data
                worksheet.Cells[r1].LoadFromArrays(headerRow);
                //worksheet.Cells[r2].LoadFromArrays(row2);
                //worksheet.Cells[r3].LoadFromArrays(row3);
                //worksheet.Cells[r4].LoadFromArrays(row4);
                //worksheet.Cells[r5].LoadFromArrays(row5);
                //worksheet.Cells[r6].LoadFromArrays(row6);
                //worksheet.Cells[r7].LoadFromArrays(row7);
                //worksheet.Cells[r8].LoadFromArrays(row8);
                //worksheet.Cells[r9].LoadFromArrays(row9);
                //worksheet.Cells[r10].LoadFromArrays(row10);
                //worksheet.Cells[r11].LoadFromArrays(row11);
                //worksheet.Cells[r12].LoadFromArrays(row12);
                //worksheet.Cells[r13].LoadFromArrays(row13);
                //worksheet.Cells[r14].LoadFromArrays(row14);
                //worksheet.Cells[r15].LoadFromArrays(row15);
                //worksheet.Cells[r16].LoadFromArrays(row16);

                FileInfo excelFile = new FileInfo(@"C:\Users\fabri\Desktop\test.xlsx");
                excel.SaveAs(excelFile);


            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            createExcelTable();
        }
    }
}