using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSVReader
{
    public class HSVDB : DbContext
    {
        public DbSet<HSV> HSVTable { get; set; }
        public HSVDB()
        {

        }

        public void registerHSV(HSV hsv)
        {
            HSV old = HSVTable.Where(v => v.X == hsv.X && v.Y == hsv.Y && v.RefValue == hsv.RefValue && v.Gain == hsv.Gain).FirstOrDefault();

            if (old != null)
            {
                HSVTable.Remove(old);
            }

            HSVTable.Add(hsv);

            SaveChanges();
        }

        public List<HSV> getHSVTableFromVandGain(int Vref, int gain)
        {
            return HSVTable.Where(v => v.RefValue == Vref && v.Gain == gain).ToList();
        }
    }
}
