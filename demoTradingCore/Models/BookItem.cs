using System;

namespace demoTradingCore.Models
{
    public class BookItem
    {
        bool IsBid = false;
        double Price = 0;
        double Size = 0;
        DateTime ServerTimeStamp;
        DateTime LocalTimeStamp;
        long EntryID = 0;
        double MinSize = 0;
        int ProviderID = 0;
        bool IsTradeable = true;
        string LayerName = "";
        eINCREMENTALTYPE IncrementalType = eINCREMENTALTYPE.NEWITEM;
    }
}
