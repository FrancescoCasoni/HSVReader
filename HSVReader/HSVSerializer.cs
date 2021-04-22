using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace HSVReader
{
    public class HSVSerializer
    {
        public static readonly string path = Path.Combine(Application.StartupPath, "hsv.values");
        public static readonly string xmlPath = Path.Combine(Application.StartupPath, "hsv.xml");
        public static List<HSV> getHSVTable() => deserilizeHSVs();


        public static void serilizeHSVs(List<HSV> list)
        {
            HSV[] array = list.ToArray();

            XmlSerializer s = new XmlSerializer(typeof(HSV[]));

            using (FileStream fs = new FileStream(xmlPath, FileMode.Create))
            {
                s.Serialize(fs, array);
            }
        }

        public static List<HSV> deserilizeHSVs()
        {
            List<HSV> list;
            XmlSerializer s = new XmlSerializer(typeof(HSV[]));
            using (FileStream fs = new FileStream(xmlPath, FileMode.OpenOrCreate))
            {
                HSV[] array = (HSV[])s.Deserialize(fs);

                list = array.ToList();
            }

            return list;
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
