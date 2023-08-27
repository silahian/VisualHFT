using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.Model;

namespace VisualHFT.Helpers
{

    public class HelperAggregatedPlotCollection : AggregatedCollection<PlotInfoPriceChart>
    {
        public HelperAggregatedPlotCollection(IEnumerable<PlotInfoPriceChart> items, AggregationLevel level, int maxItems)
            : base(items, level, maxItems, item => item.Date, AggregateData)
        {
        }

        public HelperAggregatedPlotCollection(AggregationLevel level, int maxItems)
            : base(level, maxItems, item => item.Date, AggregateData)
        {
        }

        private static void AggregateData(PlotInfoPriceChart existing, PlotInfoPriceChart newItem)
        {
            // Update the existing bucket with the new values
            existing.Volume = newItem.Volume; // (existingBucket.Volume + plotInfo.Volume) / 2;
            existing.MidPrice = newItem.MidPrice; // (existingBucket.MidPrice + plotInfo.MidPrice) / 2;
            existing.BidPrice = newItem.BidPrice; // (existingBucket.BidPrice + plotInfo.BidPrice) / 2;
            existing.AskPrice = newItem.AskPrice; // (existingBucket.AskPrice + plotInfo.AskPrice) / 2;
            existing.BuyActiveOrder = newItem.BuyActiveOrder; // (existingBucket.BuyActiveOrder + plotInfo.BuyActiveOrder) / 2;
            existing.SellActiveOrder = newItem.SellActiveOrder; // (existingBucket.SellActiveOrder + plotInfo.SellActiveOrder) / 2;
            existing.AskOrders = newItem.AskOrders?.ToList();
            existing.BidOrders = newItem.BidOrders?.ToList();
        }


        public double GetAvgOfMidPrice()
        {
            return this.ToList().Average(x =>  x.MidPrice);
        }
        public double GetMaxOfMidPrice()
        {
            return this.ToList().Max(x => x.MidPrice);
        }
        public double GetMinOfMidPrice()
        {
            return this.ToList().Min(x => x.MidPrice);
        }

        public double GetMaxOfPrices()
        {
            return this.ToList().Max(x => x.AskPrice);
        }
        public double GetMinOfPrices()
        {
            return this.ToList().Min(x => x.BidPrice);
        }

    }

}
