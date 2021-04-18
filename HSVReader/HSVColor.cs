using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSVReader
{
    public class HSVColor
    {
        float H;
        float S;
        float V;

        public HSVColor()
        {

        }

        public bool equals(HSVColor c)
        {
            string h = c.H.ToString("0.0000").Remove(5);
            string s = c.S.ToString("0.0000").Remove(5);
            string v = c.V.ToString("0.0000").Remove(5);

            string h1 = H.ToString("0.0000").Remove(5);
            string s1 = S.ToString("0.0000").Remove(5);
            string v1 = V.ToString("0.0000").Remove(5);

            bool ok = h == h1 && s == s1 && v == v1;
            if (!ok)
            {
                Console.WriteLine(h + " " + h1 + " " + s + " " + s1 + " " + v + " " + v1);
                h = c.H.ToString("0.0000").Remove(4);
                s = c.S.ToString("0.0000").Remove(4);
                v = c.V.ToString("0.0000").Remove(4);

                h1 = H.ToString("0.0000").Remove(4);
                s1 = S.ToString("0.0000").Remove(4);
                v1 = V.ToString("0.0000").Remove(4);
            }
            ok = h == h1 && s == s1 && v == v1;
            if (!ok)
            {
                Console.WriteLine(h + " " + h1 + " " + s + " " + s1 + " " + v + " " + v1);
            }
            return ok;
        }

        public static HSVColor fromRGBColor(Color color)
        {
            float r = from255To0_1(color.R);
            float g = from255To0_1(color.G);
            float b = from255To0_1(color.B);

            float m = getMinimum(r, g, b);
            float v = getValue(r, g, b);
            float c = getChroma(v, m);

            return new HSVColor()
            {
                H = color.GetHue() / 360F,
                S = getSaturation(c, v),
                V = getValue(r, g, b)
            };
        }

        public static HSVColor manualFromRGBColor(Color color)
        {
            float r = from255To0_1(color.R);
            float g = from255To0_1(color.G);
            float b = from255To0_1(color.B);

            float m = getMinimum(r, g, b);
            float v = getValue(r, g, b);
            float c = getChroma(v, m);
            float s = getSaturation(c, v);
            float h = getHue(r, g, b, v, c);

            return new HSVColor()
            {
                H = h,
                S = s,
                V = v
            };
        }

        // RGB from 0-255 to 0-1
        private static float from255To0_1(float value)
        {
            return value / 255.0F;
        }

        // Value (or Maximum) of HSV
        private static float getValue(float red, float green, float blue)
        {
            return Math.Max(Math.Max(red, green), blue);
        }

        // Minimum of HSV
        private static float getMinimum(float red, float green, float blue)
        {
            return Math.Min(Math.Min(red, green), blue);
        }

        // Chroma of HSV
        private static float getChroma(float V, float m)
        {
            return V - m;
        }

        // Hue of HSV
        private static float getHue(float R, float G, float B, float V, float C)
        {
            if (C == 0) return 0.0F;

            float hue = 0.0F;

            if (R == V)
            {
                hue = G - B;
            }
            else if (G == V)
            {
                hue = 2.0F * C + B - R;
            }
            else if (B == V)
            {
                hue = 4.0F * C + R - G;
            }

            hue = hue / (6.0F * C);

            if (hue == 1.0F)
            {
                hue = 0.0F;
            }
            else if (hue < 0.0F)
            {
                hue = hue + 1.0F;
            }
            return hue;
        }

        // Saturation of HSV
        private static float getSaturation(float C, float V)
        {
            if (V == 0F) return 0F;
            else return C / V;
        }

        private static int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

    }
}
