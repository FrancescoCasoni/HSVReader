namespace HSVReader
{
    public class HSV
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string H { get; set; }
        public string S { get; set; }
        public string V { get; set; }

        public HSV() { }

        public HSV(string arduinoHSV)
        {
            H = arduinoHSV.Substring(3, 5);
            S = arduinoHSV.Substring(14, 5);
            V = arduinoHSV.Substring(25, 4) ;
        }

        public string getString()
        {
            return "H: " + H + "\nS: " + S + "\nV: " + V;
        }
    }
}
