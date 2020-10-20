using IronOcr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

            initTable();

            DB = new HSVDB();
        }

        private void initTable()
        {
            table.Rows.Add(15);

            foreach (DataGridViewRow row in table.Rows)
            {
                row.HeaderCell.Value = string.Format("{0}", 16 - row.Index);
            }
        }

        private void table_SelectionChanged(object sender, EventArgs e)
        {
            currentCell = table.CurrentCell;
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
                    H = "-1",
                    S = "-1",
                    V = "-1",
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
        }
    }
}
