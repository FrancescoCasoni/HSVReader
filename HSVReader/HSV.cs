using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;

namespace HSVReader
{
    [Serializable]
    public class HSV
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public double H { get; set; }

        public double S { get; set; }

        public double V { get; set; }

        public int RefValue { get; set; }
        public int Gain { get; set; }
        public bool IsBlack { get; set; }

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public HSV() { }

        public HSV(string arduinoHSV, int x, int y, int gain, int refValue)
        {
            string s = new string(arduinoHSV.Where(d => char.IsDigit(d)).ToArray());

            string a = s.Substring(0, 4).Insert(1, ",");
            string b = s.Substring(4, 4).Insert(1, ",");
            string c = s.Substring(8, 3).Insert(1, ",");
            H = Math.Round(double.Parse(a), 3);
            S = Math.Round(double.Parse(b), 3);
            V = Math.Round(double.Parse(c), 3);

            HSVToRGB(H * 360, S, V, out int R, out int G, out int B);
            this.R = R;
            this.G = G;
            this.B = B;

            X = x;
            Y = y;
            Gain = gain;
            RefValue = refValue;
        }

        public string getStringForExcel()
        {
            string h = H.ToString("N3");
            string s = S.ToString("N3");
            string v = V.ToString("N2");
            return "H: " + h + "\nS: " + s + "\nV: " + v;
        }

        public static void HSVToRGB(double hue, double saturation, double value, out int r, out int g, out int b)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
            {
                r = v; g = t; b = p;
            }
            else if (hi == 1)
            {
                r = q; g = v; b = p;
            }
            else if (hi == 2)
            {
                r = p; g = v; b = t;
            }
            else if (hi == 3)
            {
                r = p; g = q; b = v;
            }
            else if (hi == 4)
            {
                r = t; g = p; b = v;
            }
            else
            {
                r = v; g = p; b = q;
            }
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            hue *= 360;
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
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

     
    }
}
