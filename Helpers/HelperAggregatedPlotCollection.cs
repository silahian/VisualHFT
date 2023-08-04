using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.Model;

namespace VisualHFT.Helpers
{
    public class HelperAggregatedPlotCollection
    {
        private readonly TimeSpan _aggregationSpan;
        private readonly List<PlotInfoPriceChart> _aggregatedData = new List<PlotInfoPriceChart>();
        private readonly object _lockObject = new object();
        private int _maxPoints = 0; // Maximum number of points

        public HelperAggregatedPlotCollection(IEnumerable<PlotInfoPriceChart> items, AggregationLevel level, int maxItems)
        {
            _aggregatedData = items.ToList();
            _maxPoints = maxItems;
            _aggregationSpan = GetAggregationSpan(level);
        }
        public HelperAggregatedPlotCollection(AggregationLevel level, int maxItems)
        {
            _maxPoints = maxItems;
            _aggregationSpan = GetAggregationSpan(level);
        }
        public void Add(PlotInfoPriceChart plotInfo)
        {

            lock (_lockObject)
            {
                if (_aggregationSpan == TimeSpan.Zero)
                {
                    _aggregatedData.Add(new PlotInfoPriceChart
                    {
                        Date = plotInfo.Date,
                        Volume = plotInfo.Volume,
                        MidPrice = plotInfo.MidPrice,
                        BidPrice = plotInfo.BidPrice,
                        AskPrice = plotInfo.AskPrice,
                        BuyActiveOrder = plotInfo.BuyActiveOrder,
                        SellActiveOrder = plotInfo.SellActiveOrder,
                        AskOrders = plotInfo.AskOrders,
                        BidOrders = plotInfo.BidOrders
                    });
                }
                else
                {
                    // Find the existing aggregation bucket for this time span
                    var bucketTime = new DateTime((plotInfo.Date.Ticks / _aggregationSpan.Ticks) * _aggregationSpan.Ticks);
                    var existingBucket = _aggregatedData.FirstOrDefault(p => p.Date == bucketTime);
                    if (existingBucket != null)
                    {
                        // Update the existing bucket with the new values
                        existingBucket.Volume = plotInfo.Volume; // (existingBucket.Volume + plotInfo.Volume) / 2;
                        existingBucket.MidPrice = plotInfo.MidPrice; // (existingBucket.MidPrice + plotInfo.MidPrice) / 2;
                        existingBucket.BidPrice = plotInfo.BidPrice; // (existingBucket.BidPrice + plotInfo.BidPrice) / 2;
                        existingBucket.AskPrice = plotInfo.AskPrice; // (existingBucket.AskPrice + plotInfo.AskPrice) / 2;
                        existingBucket.BuyActiveOrder = plotInfo.BuyActiveOrder; // (existingBucket.BuyActiveOrder + plotInfo.BuyActiveOrder) / 2;
                        existingBucket.SellActiveOrder = plotInfo.SellActiveOrder; // (existingBucket.SellActiveOrder + plotInfo.SellActiveOrder) / 2;
                        existingBucket.AskOrders = plotInfo.AskOrders?.ToList();
                        existingBucket.BidOrders = plotInfo.BidOrders?.ToList();
                    }
                    else
                    {
                        // Create a new bucket for this time span
                        _aggregatedData.Add(new PlotInfoPriceChart
                        {
                            Date = bucketTime,
                            Volume = plotInfo.Volume,
                            MidPrice = plotInfo.MidPrice,
                            BidPrice = plotInfo.BidPrice,
                            AskPrice = plotInfo.AskPrice,
                            BuyActiveOrder = plotInfo.BuyActiveOrder,
                            SellActiveOrder = plotInfo.SellActiveOrder,
                            AskOrders = plotInfo.AskOrders?.ToList(),
                            BidOrders = plotInfo.BidOrders?.ToList()
                        });

                        //CHECK MAX
                        if (_aggregatedData.Count > _maxPoints) { _aggregatedData.RemoveAt(0); }
                    }
                }
            }
        }

        public double GetAvgOfMidPrice()
        {
            lock (_lockObject)
            {
                return _aggregatedData.Average(x => x.MidPrice);
            }
        }
        public double GetMaxOfMidPrice()
        {
            lock (_lockObject)
            {
                return _aggregatedData.Max(x => x.MidPrice);
            }
        }
        public double GetMinOfMidPrice()
        {
            lock (_lockObject)
            {
                return _aggregatedData.Min(x => x.MidPrice);
            }
        }


        public int Count()
        {
            lock (_lockObject)
            {
                return _aggregatedData.Count;
            }
        }

        public IEnumerable<PlotInfoPriceChart> ToList()
        {
            lock (_lockObject)
            {
                return _aggregatedData.ToList();
            }
        }
        public PlotInfoPriceChart LastOrDefault() {
            lock (_lockObject)
                return _aggregatedData.LastOrDefault(); 
        
        }
        
        private TimeSpan GetAggregationSpan(AggregationLevel level)
        {
            switch (level)
            {
                case AggregationLevel.None: return TimeSpan.Zero;
                case AggregationLevel.Ms1:
                    return TimeSpan.FromMilliseconds(1);
                case AggregationLevel.Ms10:
                    return TimeSpan.FromMilliseconds(10);
                case AggregationLevel.Ms100:
                    return TimeSpan.FromMilliseconds(100);
                case AggregationLevel.Ms500:
                    return TimeSpan.FromMilliseconds(500);
                case AggregationLevel.S1:
                    return TimeSpan.FromSeconds(1);
                case AggregationLevel.S3:
                    return TimeSpan.FromSeconds(3);
                case AggregationLevel.S5:
                    return TimeSpan.FromSeconds(5);
                default:
                    throw new ArgumentException("Unsupported aggregation level", nameof(level));
            }
        }
    }

}
