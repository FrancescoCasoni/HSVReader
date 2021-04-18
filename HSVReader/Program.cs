using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HSVReader
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

        //    List<Color> colors = new List<Color>();
        //    for (int r = 0; r < 256; r += 1)
        //    {
        //        colors.Add(Color.FromArgb(r, 0, 0));
        //        for (int g = 0; g < 256; g += 1)
        //        {
        //            colors.Add(Color.FromArgb(r, g, 0));
        //            for (int b = 0; b < 256; b += 1)
        //            {
        //                colors.Add(Color.FromArgb(r, g, b));
        //            }
        //        }
        //    }

        //    List<Color> ok = new List<Color>();
        //    foreach (Color color in colors)
        //    {
        //        HSVColor auto = HSVColor.fromRGBColor(color);
        //        HSVColor manual = HSVColor.manualFromRGBColor(color);

        //        if (auto.equals(manual))
        //        {
        //            ok.Add(color);
        //        }
        //        else
        //        {

        //        }
        //    }
        }
    }
}
