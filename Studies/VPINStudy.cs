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

namespace VisualHFT.Studies
{
    /// <summary>
    /// The VPIN (Volume-Synchronized Probability of Informed Trading) value is a measure of the imbalance between buy and sell volumes in a given bucket. It's calculated as the absolute difference between buy and sell volumes divided by the total volume (buy + sell) for that bucket.
    /// 
    /// Given this definition, the range of VPIN values is between 0 and 1:
    ///     0: This indicates a perfect balance between buy and sell volumes in the bucket. In other words, the number of buy trades is equal to the number of sell trades.
    ///     1: This indicates a complete imbalance, meaning all the trades in the bucket are either all buys or all sells.
    /// Most of the time, the VPIN value will be somewhere between these two extremes, indicating some level of imbalance between buy and sell trades. The closer the VPIN value is to 1, the greater the imbalance, and vice versa.
    /// </summary>
    public class VPINStudy: IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private OrderBook _orderBook; //to hold last market data tick
        private string _symbol = null;
        private int _providerId = -1;
        //variables for calculation
        private readonly decimal _bucketVolumeSize; // The volume size of each bucket
        private decimal _currentBuyVolume = 0; // Current volume of buy trades in the bucket
        private decimal _currentSellVolume = 0; // Current volume of sell trades in the bucket
        private decimal _lastMarketMidPrice = 0; //keep track of market price

        private readonly int _rollingWindowSize; // Number of buckets to consider for rolling calculation
        private const decimal VPIN_THRESHOLD = 0.7M; // ALERT Example threshold


        private DateTime _currentBucketStartTime;
        private DateTime _currentBucketEndTime;
        private List<VPIN> _rollingVPINValues;//to maintain rolling window of VPIN values

        // Event declaration
        public event EventHandler<decimal> VPINAlertTriggered;
        public event EventHandler<VPIN> VPINCalculated;
        public event EventHandler<VPIN> VPINRollingAdded;
        public event EventHandler<VPIN> VPINRollingUpdated;
        public event EventHandler<int> VPINRollingRemoved;

        public VPINStudy(string symbol, int providerId, decimal bucketVolumeSize, int rollingWindowSize = 50)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new Exception("Symbol cannot be null or empty.");

            HelperCommon.LIMITORDERBOOK.OnDataReceived += LIMITORDERBOOK_OnDataReceived;
            HelperCommon.TRADES.OnDataReceived += TRADES_OnDataReceived;
            _symbol = symbol;
            _providerId = providerId;

            _bucketVolumeSize = bucketVolumeSize;
            _rollingWindowSize = rollingWindowSize;
            _rollingVPINValues = new List<VPIN>();

            CalculateVPINForBucket(true); //initial value
            
        }
        ~VPINStudy()
        {
            Dispose(false);
        }



        public IReadOnlyList<VPIN> VpinData => _rollingVPINValues.AsReadOnly();
        private void TRADES_OnDataReceived(object sender, Trade e)
        {
            if (e == null)
                return;
            if (_providerId != e.ProviderId || _symbol == "-- All symbols --" || _symbol != e.Symbol)
                return;

            // Set the start time for the current bucket if it's a new bucket
            if (_currentBuyVolume == 0 && _currentSellVolume == 0)
            {
                _currentBucketStartTime = e.Timestamp;
            }

            if (e.IsBuy)
                _currentBuyVolume += e.Size;
            else
                _currentSellVolume += e.Size;

            // Check if the bucket is filled
            if (_currentBuyVolume + _currentSellVolume >= _bucketVolumeSize)
            {
                _currentBucketStartTime = e.Timestamp;
                _currentBucketEndTime = e.Timestamp; // Set the end time for the current bucket
                CalculateVPINForBucket(true);
                ResetBucket();
            }
            else
            {
                _currentBucketEndTime = e.Timestamp; // Set the end time for the current bucket
                CalculateVPINForBucket(false);
            }
        }
        private void LIMITORDERBOOK_OnDataReceived(object sender, OrderBook e)
        {
            //Thread.Sleep(1000000000);
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
        }
        private void CalculateVPINForBucket(bool isNewBucket)
        {
            decimal vpin = 0;
            if ((_currentBuyVolume + _currentSellVolume) > 0)
                vpin = (decimal)Math.Abs(_currentBuyVolume - _currentSellVolume) / (_currentBuyVolume + _currentSellVolume);

            if (isNewBucket)
            {
                // Check against threshold and trigger alert
                if (vpin > VPIN_THRESHOLD)
                    OnVPINAlertTriggered(vpin);

                // Add to rolling window and remove oldest if size exceeded
                var newItem = new VPIN() { Value = vpin, TimestampIni = _currentBucketStartTime, TimestampEnd = _currentBucketEndTime, MarketMidPrice = _lastMarketMidPrice };
                _rollingVPINValues.Add(newItem);
                OnVPINRollingAdded(newItem);

                if (_rollingVPINValues.Count > _rollingWindowSize)
                {
                    OnVPINRollingRemoved(0);
                    _rollingVPINValues.RemoveAt(0);
                }
            }
            else if (_rollingVPINValues.Any())
            {
                var lastNode = _rollingVPINValues.Last();
                lastNode.Value = vpin;
                lastNode.TimestampIni = _currentBucketStartTime;
                lastNode.TimestampEnd = _currentBucketEndTime;
                lastNode.MarketMidPrice = _lastMarketMidPrice;
                OnVPINRollingUpdated(lastNode);
            }

            if (_rollingVPINValues.Any())
                OnVPINCalculated(_rollingVPINValues.Last());
        }
        private void ResetBucket()
        {
            _currentBuyVolume = 0;
            _currentSellVolume = 0;
        }
        // Event invoker method
        protected virtual void OnVPINAlertTriggered(decimal vpinValue)
        {
            VPINAlertTriggered?.Invoke(this, vpinValue);
        }
        protected virtual void OnVPINCalculated(VPIN vpin)
        {
            VPINCalculated?.Invoke(this, vpin);
        }
        protected virtual void OnVPINRollingAdded(VPIN vpin)
        {
            VPINRollingAdded?.Invoke(this, vpin);
        }
        protected void OnVPINRollingRemoved(int index)
        {
            VPINRollingRemoved?.Invoke(this, index);
        }
        protected void OnVPINRollingUpdated(VPIN vpin)
        {
            VPINRollingUpdated?.Invoke(this, vpin);
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
                    _rollingVPINValues.Clear();
                    _rollingVPINValues = null;
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
