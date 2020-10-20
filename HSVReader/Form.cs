using IronOcr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            DB.HSVTable.ForEachAsync(c => table.Rows[16 - c.Y].Cells[c.X + 1].Style.BackColor = Color.Red);
        }

        private void table_SelectionChanged(object sender, EventArgs e)
        {
            currentCell = table.CurrentCell;

            int X = currentCell.ColumnIndex + 1;
            int Y = 16 - currentCell.RowIndex;

            HSV hsv = DB.HSVTable.Where(v => v.X == X && v.Y == Y).FirstOrDefault();
            if (hsv == null) return;

            label9.Text = "H: " + hsv.H;
            label8.Text = "S: " + hsv.S;
            label7.Text = "V: " + hsv.V;

        }

        void hsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }

        private Bitmap getScreenImage()
        {
            //Create a new bitmap.
            var screen = new Bitmap(500, 50, PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(screen);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(10, 150, 0, 0, new Size(500, 50), CopyPixelOperation.SourceCopy);

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

            hsvToRgb(hsv.H * 360, hsv.S * 360, hsv.V * 360, out int R, out int G, out int B);

            table.Rows[currentCell.RowIndex].Cells[currentCell.ColumnIndex].Style.BackColor = Color.FromArgb(R, G, B);
        }
    }
}
