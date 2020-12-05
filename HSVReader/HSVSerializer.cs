using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace HSVReader
{
    public class HSVSerializer
    {
        public static readonly string path = Path.Combine(Application.StartupPath, "hsv.values");
        public static List<HSV> getHSVTable() => deserilizeHSVs();


        public static void serilizeHSVs(List<HSV> list)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, list);
                fs.Close();
            }
        }

        public static List<HSV> deserilizeHSVs()
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (List<HSV>)formatter.Deserialize(fs);
            }
        }

        public static void registerHSV(HSV hsv)
        {
            List<HSV> HSVTable = getHSVTable();

            HSV old = HSVTable.Find(v => v.X == hsv.X && v.Y == hsv.Y && v.RefValue == hsv.RefValue && v.Gain == hsv.Gain && v.IsBlack == hsv.IsBlack);

            if (old != null) HSVTable.Remove(old);

            HSVTable.Add(hsv);

            serilizeHSVs(HSVTable);
        }

        public static void deleteHSV(int x, int y, int refValue, int gain, bool isBlack)
        {
            List<HSV> HSVTable = getHSVTable();
            HSV hsv = HSVTable.Find(v => v.X == x && v.Y == y && v.RefValue == refValue && v.Gain == gain && v.IsBlack == isBlack);

            if (hsv != null) HSVTable.Remove(hsv);

            serilizeHSVs(HSVTable);
        }

        public static List<HSV> getHSVTableFromValueGainBlack(int Vref, int gain, bool isBlack)
        {
            return getHSVTable().Where(v => v.RefValue == Vref && v.Gain == gain && v.IsBlack == isBlack).ToList();
        }
    }
}
