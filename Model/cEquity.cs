using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualHFT.Model
{
    public class cEquity
    {
        public DateTime Date { get; set; }
        public decimal Equity { get; set; }
        public decimal Commission { get; set; }
        public decimal VolumeQty { get; set; }
        public static cEquity GetMinBetween2Points(cEquity fromElement, cEquity toElement, List<cEquity> aList)
        {
            if (fromElement == null || toElement == null)
                return null;
            var aPeriod = (from x in aList where x.Date >= fromElement.Date && x.Date <= toElement.Date select x).ToList();
            if (aPeriod != null && aPeriod.Count > 0)
                return aPeriod.OrderBy(x => x.Equity).FirstOrDefault();
            else
                return null;
        }
    }
    public class cDrawDown
    {
        public DateTime Date { get; set; }
        public decimal DrawDownAmmount { get; set; }
        public decimal DrawDownPerc { get; set; }
        public int DrawDownHours { get; set; }
    }
}
