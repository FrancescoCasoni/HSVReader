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
            HSVSerializer.init();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());          
        }
    }
}
