using OxyPlot.Series;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.View;

namespace VisualHFT.Studies
{
    public class LOBImbalanceStudy: IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private OrderBook _orderBook; //to hold last market data tick
        private string _symbol = null;
        private int _providerId = -1;
        private AggregatedCollection<BaseStudyModel> _rollingValues;//to maintain rolling window of study's values
        private AggregationLevel _aggregationLevel;

        // Event declaration
        public event EventHandler<decimal> OnAlertTriggered;
        public event EventHandler<BaseStudyModel> OnCalculated;
        public event EventHandler<BaseStudyModel> OnRollingAdded;
        public event EventHandler<BaseStudyModel> OnRollingUpdated;
        public event EventHandler<int> OnRollingRemoved;

        public LOBImbalanceStudy(string symbol, int providerId, AggregationLevel aggregationLevel, int rollingWindowSize = 50)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new Exception("Symbol cannot be null or empty.");

            EventAggregator.Instance.OnOrderBookDataReceived += LIMITORDERBOOK_OnDataReceived;
            _symbol = symbol;
            _providerId = providerId;
            _aggregationLevel = aggregationLevel;
            _rollingValues = new AggregatedCollection<BaseStudyModel>(aggregationLevel, rollingWindowSize, x => x.Timestamp, AggregateData);
            _rollingValues.OnRemoved += _rollingValues_OnRemoved;            
        }
        ~LOBImbalanceStudy()
        {
            Dispose(false);
        }
        private static void AggregateData(BaseStudyModel existing, BaseStudyModel newItem)
        {
            // Update the existing bucket with the new values
            existing.Timestamp = newItem.Timestamp;
            existing.Value = newItem.Value;
        }
        public IReadOnlyList<BaseStudyModel> Data => _rollingValues.AsReadOnly();
        public AggregationLevel AggregationLevel
        {
            get => _aggregationLevel;
            set => _aggregationLevel = value;
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

            if (!_orderBook.LoadData(e.Asks, e.Bids))
                return; //if nothing to update, then exit
            
            if (_orderBook.MidPrice != 0)
                CalculateStudy();
        }
        private void CalculateStudy()
        {
            var newItem = new BaseStudyModel() { 
                Value = (decimal)_orderBook.ImbalanceValue, 
                ValueFormatted = _orderBook.ImbalanceValue.ToString("N1"),
                Timestamp = DateTime.Now, 
                MarketMidPrice = (decimal)_orderBook.MidPrice 
            };
            bool addSuccess = _rollingValues.Add(newItem);
            if (addSuccess)
                OnRollingAdded?.Invoke(this, newItem);
            else
                OnRollingUpdated?.Invoke(this, newItem);


            OnCalculated?.Invoke(this, newItem);
        }
        private void _rollingValues_OnRemoved(object sender, int e)
        {
            OnRollingRemoved?.Invoke(this, e);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _providerId = 0;
                    _symbol = "";

                    // Dispose managed resources here
                    HelperCommon.LIMITORDERBOOK.OnDataReceived -= LIMITORDERBOOK_OnDataReceived;
                    _orderBook = null;

                    _rollingValues.OnRemoved -= _rollingValues_OnRemoved;
                    _rollingValues.Clear();
                    _rollingValues = null;
                }
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
