using IronOcr;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HSVReader
{
    public partial class Form : System.Windows.Forms.Form
    {
        private DataGridViewCell currentCell;
        private readonly AutoOcr Ocr;
        private readonly HSVDB DB;
        private int Value;
        private int Gain;
        private bool ForcedV;
        private bool ForcedS;
        private bool ForcedH;

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

            WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.Manual;
            BringToFront();
            Left = 1920 - Width;
            Width += 7;
            Height += 7;
            Top = 0;

            Ocr = new AutoOcr();

            DB = new HSVDB();

            initTable();

            comboBoxGain.SelectedIndex = 1;
            comboBoxValue.SelectedIndex = 4;

            updateTable();
        }

        private void initTable()
        {
            table.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            table.Rows.Add(15);

            foreach (DataGridViewRow row in table.Rows)
            {
                row.HeaderCell.Value = string.Format("{0}", 16 - row.Index);
                row.Height = 52;
            }
            foreach (DataGridViewColumn col in table.Columns)
            {
                col.Width = 52;
            }

            table.RowHeadersDefaultCellStyle.Padding = Padding.Empty;

            table.Width = 17 * 52 - 2;
            table.Height = 16 * 52 + 21;


            pictureBox2.Height = table.Height;
            pictureBox2.Top = table.Top;
            pictureBox2.Left = table.Right;
            Bitmap b = new Bitmap(table.Width, table.Height);
            table.DrawToBitmap(b, new Rectangle(0, 0, b.Width, b.Height));
            b = b.Clone(new Rectangle(21, 0, 25, table.Height), b.PixelFormat);
            pictureBox2.Image = b;

            pictureBox3.Height = 21;
            pictureBox3.Width = table.Width + pictureBox2.Width;
            pictureBox3.Top = table.Bottom;
            pictureBox3.Left = table.Left;
            Bitmap c = new Bitmap(table.Width, table.Height);
            table.DrawToBitmap(c, new Rectangle(0, 0, c.Width, c.Height));
            c = c.Clone(new Rectangle(0, 2, table.Width, 18), c.PixelFormat);
            Bitmap d = new Bitmap(table.Width + pictureBox2.Width, 21);
            Graphics.FromImage(d).DrawImage(c, 0, 0);
            pictureBox3.Image = d;

            panel1.Height = table.Height + pictureBox3.Height;
        }

        private void updateTable()
        {
            foreach (DataGridViewRow row in table.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = Color.White;
                }
            }

            colorCells();

            label6.Text = "V: " + ((double)Value / 100).ToString("N2") + "    Gain: " + Gain + "X";

            table_SelectionChanged(null, null);
        }

        private void colorCells()
        {
            foreach (HSV cell in DB.getHSVTableFromVandGain(Value, Gain))
            {

                double h = ForcedH ? (double)numericUpDownH.Value : cell.H;
                double s = ForcedS ? (double)numericUpDownS.Value : cell.S;
                double v = ForcedV ? (double)numericUpDownV.Value : cell.V;

                table.Rows[16 - cell.Y].Cells[cell.X - 1].Style.BackColor = ColorFromHSV(h * 360, s, v);
            }
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
            var screen = new Bitmap(330, 50, PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(screen);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(2, 150, 0, 0, new Size(330, 50), CopyPixelOperation.SourceCopy);

            pictureBox1.Image = screen;

            return screen;
        }

        private void performReadAndSave()
        {
            var res = Ocr.Read(getScreenImage());
            string read = res.Text;
            HSV hsv;
            if (string.IsNullOrWhiteSpace(read) || string.IsNullOrEmpty(read) || read.Length < 29)
            {
                hsv = new HSV()
                {
                    H = -1,
                    S = -1,
                    V = -1,
                    X = currentCell.ColumnIndex + 1,
                    Y = 16 - currentCell.RowIndex
                };

                label1.Text = "H: " + hsv.H.ToString("N3");
                label2.Text = "S: " + hsv.S.ToString("N3");
                label3.Text = "V: " + hsv.V.ToString("N2");
                label4.Text = "X: " + hsv.X;
                label5.Text = "Y: " + hsv.Y;

                return;
            }
            else
            {
                try
                {
                    hsv = new HSV(read)
                    {
                        X = currentCell.ColumnIndex + 1,
                        Y = 16 - currentCell.RowIndex,
                        Gain = Gain,
                        RefValue = Value
                    };
                }
                catch (Exception)
                {
                    hsv = new HSV()
                    {
                        H = -1,
                        S = -1,
                        V = -1,
                        X = currentCell.ColumnIndex + 1,
                        Y = 16 - currentCell.RowIndex
                    };

                    label1.Text = "H: " + hsv.H.ToString("N3");
                    label2.Text = "S: " + hsv.S.ToString("N3");
                    label3.Text = "V: " + hsv.V.ToString("N2");
                    label4.Text = "X: " + hsv.X;
                    label5.Text = "Y: " + hsv.Y;

                    return;
                }

            }

            label1.Text = "H: " + hsv.H;
            label2.Text = "S: " + hsv.S;
            label3.Text = "V: " + hsv.V;
            label4.Text = "X: " + hsv.X;
            label5.Text = "Y: " + hsv.Y;

            DB.registerHSV(hsv);


            double h = ForcedH ? (double)numericUpDownH.Value : hsv.H;
            double s = ForcedS ? (double)numericUpDownS.Value : hsv.S;
            double v = ForcedV ? (double)numericUpDownV.Value : hsv.V;

            Color c = ColorFromHSV(h * 360, s, v);

            table.Rows[currentCell.RowIndex].Cells[currentCell.ColumnIndex].Style.BackColor = c;

            table_SelectionChanged(null, null);
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
                        var hsv = DB.getHSVTableFromVandGain(Value, Gain).Where(v => v.X - 1 == x && v.Y - 1 == y).FirstOrDefault();

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

                string range = "A1:P1";

                var worksheet = excel.Workbook.Worksheets["Worksheet1"];

                worksheet.Cells.Style.WrapText = true;

                worksheet.Cells[range].LoadFromArrays(headerRow);


                FileInfo excelFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Readings " + Gain + "xG " + Value + "V.xlsx");
                excel.SaveAs(excelFile);
            }
        }



        private void table_SelectionChanged(object sender, EventArgs e)
        {
            if (table.CurrentCell == null) return;

            currentCell = table.CurrentCell;
            //Color c = currentCell.Style.BackColor;
            //c = Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B);
            //table.ForeColor = c;
            table.ClearSelection();

            foreach (DataGridViewRow row in table.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = "";
                }
            }

            table.Rows[currentCell.RowIndex].Cells[currentCell.ColumnIndex].Value = "●";

            int X = currentCell.ColumnIndex + 1;
            int Y = 16 - currentCell.RowIndex;

            HSV hsv = DB.getHSVTableFromVandGain(Value, Gain).Where(v => v.X == X && v.Y == Y).FirstOrDefault();
            if (hsv == null)
            {
                label9.Text = "H: missing";
                label8.Text = "S: missing";
                label7.Text = "V: missing";
                label18.Text = "Col: " + X;
                label19.Text = "Row: " + Y;
                return;
            }
            label9.Text = "H: " + hsv.H.ToString("N3");
            label8.Text = "S: " + hsv.S.ToString("N3");
            label7.Text = "V: " + hsv.V.ToString("N2");
            label18.Text = "Col: " + hsv.X;
            label19.Text = "Row: " + hsv.Y;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            createExcelTable();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxGain.SelectedIndex)
            {
                default: Gain = 1; break;
                case 0: Gain = 1; break;
                case 1: Gain = 4; break;
                case 2: Gain = 16; break;
                case 3: Gain = 60; break;
            }

            updateTable();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxValue.SelectedIndex)
            {
                default: break;
                case 0: Value = 55; break;
                case 1: Value = 60; break;
                case 2: Value = 65; break;
                case 3: Value = 70; break;
                case 4: Value = 75; break;
                case 5: Value = 80; break;
                case 6: Value = 85; break;
                case 7: Value = 90; break;
                case 8: Value = 95; break;
                case 9: Value = 100; break;
            }

            updateTable();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel1.ClientRectangle,
                   Color.Gray, 0, ButtonBorderStyle.Solid,
                   Color.Gray, 0, ButtonBorderStyle.Solid,
                   Color.Black, 2, ButtonBorderStyle.Solid,
                   Color.Gray, 0, ButtonBorderStyle.Solid);
        }

        private void table_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, table.ClientRectangle,
                  Color.Black, 0, ButtonBorderStyle.Solid,
                  Color.Black, 0, ButtonBorderStyle.Solid,
                  Color.Black, 0, ButtonBorderStyle.Solid,
                  Color.Black, 0, ButtonBorderStyle.Solid);
        }

        private void buttonFH_Click(object sender, EventArgs e)
        {
            ForcedH = !ForcedH;
            buttonFH.Text = ForcedH ? "Remove" : "Apply";
            colorCells();
        }

        private void buttonFS_Click(object sender, EventArgs e)
        {
            ForcedS = !ForcedS;
            buttonFS.Text = ForcedS ? "Remove" : "Apply";
            colorCells();
        }

        private void buttonFV_Click(object sender, EventArgs e)
        {
            ForcedV = !ForcedV;
            buttonFV.Text = ForcedV ? "Remove" : "Apply";
            colorCells();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, pictureBox2.ClientRectangle,
                  Color.Gray, 0, ButtonBorderStyle.Solid,
                  Color.Gray, 0, ButtonBorderStyle.Solid,
                  Color.Black, 2, ButtonBorderStyle.Solid,
                  Color.Gray, 0, ButtonBorderStyle.Solid);
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, pictureBox3.ClientRectangle,
                 Color.Gray, 0, ButtonBorderStyle.Solid,
                 Color.Gray, 0, ButtonBorderStyle.Solid,
                 Color.Black, 2, ButtonBorderStyle.Solid,
                 Color.Black, 2, ButtonBorderStyle.Solid);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel2.ClientRectangle,
                Color.Gray, 0, ButtonBorderStyle.Solid,
                Color.Gray, 0, ButtonBorderStyle.Solid,
                Color.Black, 0, ButtonBorderStyle.Solid,
                Color.Black, 2, ButtonBorderStyle.Solid);
        }
    }
}