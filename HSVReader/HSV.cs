using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HSVReader
{
    public class HSV
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public double H { get; set; }

        public double S { get; set; }

        public double V { get; set; }

        public HSV() { }

        public HSV(string arduinoHSV)
        {
            H = Math.Round(double.Parse(arduinoHSV.Substring(3, 5).Replace('.', ',')), 3);
            S = Math.Round(double.Parse(arduinoHSV.Substring(14, 5).Replace('.', ',')), 3);
            V = Math.Round(double.Parse(arduinoHSV.Substring(25, 4).Replace('.', ',')), 3);
        }

        public string getString()
        {
            return "H: " + H + "\nS: " + S + "\nV: " + V;
        }
    }
}
