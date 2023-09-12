using QuickFix.Fields;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using VisualHFT.Helpers;
using VisualHFT.Model;
using static NetMQ.NetMQSelector;

namespace VisualHFT.Studies
{
    /// <summary>
    /// Trade To Order Ratio Study
    /// 
    /// **Description:** The T2O ratio measures the proportion of executed trades to placed orders in the market.
    /// **Calculation:** It's calculated by dividing the number of executed trades by the total number of orders (including unexecuted ones) in a given time frame.
    /// **Usefulness:** A high T2O ratio may indicate strong demand or supply at certain price levels, while a low ratio may suggest indecision or lack of conviction in the market.
    /// </summary>
    public class TradeToOrderRatioStudy : IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private OrderBook _orderBook; //to hold last market data tick
        private string _symbol = null;
        private int _providerId = -1;
        //variables for calculation
        private decimal _lastMarketMidPrice = 0; //keep track of market price

        private readonly int _rollingWindowSize; // Number of buckets to consider for rolling calculation
        private const decimal VALUE_THRESHOLD = 0.7M; // ALERT Example threshold


        private AggregatedCollection<BaseStudyModel> _rollingValues;//to maintain rolling window of study's values
        private decimal total_L2OrderSize_Ini=0;
        private decimal total_L2OrderSize_End = 0;
        private decimal totalExecutedTradeSize=0;
        private AggregationLevel _aggregationLevel;


        // Event declaration
        public event EventHandler<decimal> OnAlertTriggered;
        public event EventHandler<BaseStudyModel> OnCalculated;
        public event EventHandler<BaseStudyModel> OnRollingAdded;
        public event EventHandler<BaseStudyModel> OnRollingUpdated;
        public event EventHandler<int> OnRollingRemoved;

        public TradeToOrderRatioStudy(string symbol, int providerId, AggregationLevel aggregationLevel, int rollingWindowSize = 50)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new Exception("Symbol cannot be null or empty.");

            HelperCommon.LIMITORDERBOOK.OnDataReceived += LIMITORDERBOOK_OnDataReceived;
            HelperCommon.TRADES.OnDataReceived += TRADES_OnDataReceived;
            _symbol = symbol;
            _providerId = providerId;
            _aggregationLevel = aggregationLevel;

            _rollingValues = new AggregatedCollection<BaseStudyModel>(aggregationLevel, rollingWindowSize, x => x.Timestamp, AggregateData);
            _rollingValues.OnRemoved += _rollingValues_OnRemoved;
            CalculateT2ORatio(); //initial value
            
        }

        private void _rollingValues_OnRemoved(object sender, int e)
        {
            OnRollingRemoved?.Invoke(this, e);
        }

        ~TradeToOrderRatioStudy()
        {
            Dispose(false);
        }

        public AggregationLevel AggregationLevel
        {
            get => _aggregationLevel;
            set => _aggregationLevel = value;
        }

        private static void AggregateData(BaseStudyModel existing, BaseStudyModel newItem)
        {
            // Update the existing bucket with the new values
            existing.Timestamp = newItem.Timestamp;
            existing.Value = newItem.Value;
        }


        public IReadOnlyList<BaseStudyModel> Data => _rollingValues.AsReadOnly();
        private void TRADES_OnDataReceived(object sender, Trade e)
        {
            if (e == null || e.ProviderId != _providerId || e.Symbol != _symbol)
                return;

            if (e.IsBuy)
                totalExecutedTradeSize += e.Size;  // Add the size of the executed trade
            else
                totalExecutedTradeSize -= e.Size;  // Subtract the size of the executed trade

            CalculateT2ORatio();
        }

        private void LIMITORDERBOOK_OnDataReceived(object sender, OrderBook e)
        {
            if (e == null)
                return;
            if (_providerId != e.ProviderID || _symbol == "-- All symbols --" || _symbol != e.Symbol)
                return;
            if (_orderBook == null)
            {
                _orderBook = new OrderBook(_symbol, e.DecimalPlaces);
            }

            if (!_orderBook.LoadData(e.Asks?.ToList(), e.Bids?.ToList()))
                return; //if nothing to update, then exit

            _lastMarketMidPrice = (decimal)_orderBook.MidPrice;
            var currentOrderSize = e.Asks.Where(x => x.Size.HasValue).Sum(a => (decimal)a.Size) + e.Bids.Where(x => x.Size.HasValue).Sum(b => (decimal)b.Size);  // Sum of all order sizes
            if (total_L2OrderSize_Ini == 0)
                total_L2OrderSize_Ini = currentOrderSize;
            total_L2OrderSize_End = currentOrderSize;

            CalculateT2ORatio();
        }
        private void CalculateT2ORatio()
        {
            decimal t2oRatio = 0;
            decimal delta = total_L2OrderSize_End - total_L2OrderSize_Ini;

            if (delta == 0)
                t2oRatio = 0;  // Avoid division by zero
            else 
                t2oRatio = totalExecutedTradeSize / delta;

            // Trigger any events or updates based on the new T2O ratio
            var newItem = new BaseStudyModel() {
                Value = t2oRatio,
                ValueFormatted = Math.Min(t2oRatio, 0.99m).ToString("N1"),
                MarketMidPrice = _lastMarketMidPrice, 
                Timestamp = DateTime.Now, 
                
            };

            bool addSuccess = _rollingValues.Add(newItem);
            if (addSuccess)
            {
                OnRollingAdded?.Invoke(this, newItem);
                totalExecutedTradeSize = 0; //reset
                total_L2OrderSize_Ini = 0; //reset
                total_L2OrderSize_End = 0; //reset
            }
            else
                OnRollingUpdated?.Invoke(this, newItem);

            OnCalculated?.Invoke(this, newItem);


        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                    HelperCommon.LIMITORDERBOOK.OnDataReceived -= LIMITORDERBOOK_OnDataReceived;
                    HelperCommon.TRADES.OnDataReceived -= TRADES_OnDataReceived;
                    _orderBook = null;

                    if (_rollingValues != null)
                    {
                        _rollingValues.OnRemoved -= _rollingValues_OnRemoved;
                        _rollingValues.Clear();
                        _rollingValues.Dispose();
                    }
                    _rollingValues = null;
                }
                // Dispose unmanaged resources here, if any
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
