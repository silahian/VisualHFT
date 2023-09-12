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
        private decimal _lastVPIN = 0;

        private readonly int _rollingWindowSize; // Number of buckets to consider for rolling calculation
        private const decimal VPIN_THRESHOLD = 0.7M; // ALERT Example threshold


        private DateTime _currentBucketStartTime;
        private DateTime _currentBucketEndTime;
        private AggregatedCollection<BaseStudyModel> _rollingValues;//to maintain rolling window of study's values
        private AggregationLevel _aggregationLevel;

        // Event declaration
        public event EventHandler<decimal> OnAlertTriggered;
        public event EventHandler<BaseStudyModel> OnCalculated;
        public event EventHandler<BaseStudyModel> OnRollingAdded;
        public event EventHandler<BaseStudyModel> OnRollingUpdated;
        public event EventHandler<int> OnRollingRemoved;


        public VPINStudy(string symbol, int providerId, AggregationLevel aggregationLevel, decimal bucketVolumeSize, int rollingWindowSize = 50)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new Exception("Symbol cannot be null or empty.");

            HelperCommon.LIMITORDERBOOK.OnDataReceived += LIMITORDERBOOK_OnDataReceived;
            HelperCommon.TRADES.OnDataReceived += TRADES_OnDataReceived;
            _symbol = symbol;
            _providerId = providerId;
            _aggregationLevel = aggregationLevel;

            _bucketVolumeSize = bucketVolumeSize;
            _rollingWindowSize = rollingWindowSize;

            _rollingValues = new AggregatedCollection<BaseStudyModel>(_aggregationLevel, rollingWindowSize, x => x.Timestamp, AggregateData);
            _rollingValues.OnRemoved += _rollingValues_OnRemoved;

            CalculateStudy(true); //initial value
            
        }
        ~VPINStudy()
        {
            Dispose(false);
        }
        private void _rollingValues_OnRemoved(object sender, int e)
        {
            OnRollingRemoved?.Invoke(this, e);
        }

        private static void AggregateData(BaseStudyModel existing, BaseStudyModel newItem)
        {
            // Update the existing bucket with the new values
            existing.Timestamp = newItem.Timestamp;
            existing.Value = newItem.Value;
        }
        public IReadOnlyList<BaseStudyModel> VpinData => _rollingValues.AsReadOnly();
        public AggregationLevel AggregationLevel
        {
            get => _aggregationLevel;
            set => _aggregationLevel = value;
        }
        public decimal BucketVolumeSize => _bucketVolumeSize;

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
                CalculateStudy(true);
                ResetBucket();
            }
            else
            {
                _currentBucketEndTime = e.Timestamp; // Set the end time for the current bucket
                CalculateStudy(false);
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

            if (_lastMarketMidPrice != 0)
                CalculateStudy(false);
        }
        private void CalculateStudy(bool isNewBucket)
        {
            decimal vpin = 0;
            if ((_currentBuyVolume + _currentSellVolume) > 0)
                vpin = (decimal)Math.Abs(_currentBuyVolume - _currentSellVolume) / (_currentBuyVolume + _currentSellVolume);
            if (isNewBucket)
            {
                // Check against threshold and trigger alert
                if (vpin > VPIN_THRESHOLD)
                    OnAlertTriggered?.Invoke(this, vpin);
                _lastVPIN = vpin;
            }



            // Add to rolling window and remove oldest if size exceeded
            var newItem = new BaseStudyModel()
            {
                Value = _lastVPIN,
                ValueFormatted = _lastVPIN.ToString("N1"),
                Timestamp = DateTime.Now,
                MarketMidPrice = _lastMarketMidPrice
            };
            bool addSuccess = _rollingValues.Add(newItem);
            if (addSuccess)
                OnRollingAdded?.Invoke(this, newItem);
            else
                OnRollingUpdated?.Invoke(this, newItem);

            OnCalculated?.Invoke(this, newItem);
        }
        private void ResetBucket()
        {
            _currentBuyVolume = 0;
            _currentSellVolume = 0;
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
                    _rollingValues.OnRemoved -= _rollingValues_OnRemoved;
                    _rollingValues.Clear();
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
