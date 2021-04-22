using IronOcr;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace HSVReader
{
    public partial class Watch : Form
    {

        private readonly IronTesseract Ocr;
        private Timer t;

        public Watch()
        {
            InitializeComponent();
            ControlExtension.Draggable(panel2, true);

            Ocr = new IronTesseract();

            t = new Timer();
            t.Interval = 1000; 
            t.Tick += new EventHandler(timer_Tick);
            t.Start();

            readAndShow();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            readAndShow();
        }

        private void readAndShow()
        {
            try
            {
                OcrResult res = Ocr.Read(getScreenImage());
                string ocrText = res.Text;


                HSV hsv = new HSV(ocrText, 0, 0, 0, 0);

                button1.ForeColor = Color.LimeGreen;

                panel1.BackColor = Color.FromArgb(hsv.R, hsv.G, hsv.B);
            }
            catch (Exception)
            {
                panel1.BackColor = Color.Black;

                button1.ForeColor = Color.OrangeRed;

                return;
            }
        }

        private Bitmap getScreenImage()
        {
            //Create a new bitmap.
            var screen = new Bitmap(window.Width, window.Height, PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(screen);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(panel2.Left + window.Left, panel2.Top + window.Top, 0, 0, window.Size, CopyPixelOperation.SourceCopy);

            //panel2.BackgroundImage = screen;
            //screen.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) +@"\ciao.JPG");
            return screen;
        }
    }
}
