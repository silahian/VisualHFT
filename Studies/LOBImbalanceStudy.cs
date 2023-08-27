using OxyPlot.Series;
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
using VisualHFT.View;
using static NetMQ.NetMQSelector;

namespace VisualHFT.Studies
{
    public class LOBImbalanceStudy: IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private OrderBook _orderBook; //to hold last market data tick
        private string _symbol = null;
        private int _providerId = -1;
        private AggregatedCollection<LOBImbalance> _rollingValues;//to maintain rolling window of study's values

        // Event declaration
        public event EventHandler<decimal> OnAlertTriggered;
        public event EventHandler<LOBImbalance> OnCalculated;
        public event EventHandler<LOBImbalance> OnRollingAdded;
        public event EventHandler<LOBImbalance> OnRollingUpdated;
        public event EventHandler<int> OnRollingRemoved;

        public LOBImbalanceStudy(string symbol, int providerId, AggregationLevel aggregationLevel, int rollingWindowSize = 50)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new Exception("Symbol cannot be null or empty.");

            HelperCommon.LIMITORDERBOOK.OnDataReceived += LIMITORDERBOOK_OnDataReceived;
            _symbol = symbol;
            _providerId = providerId;

            _rollingValues = new AggregatedCollection<LOBImbalance>(aggregationLevel, rollingWindowSize, x => x.Timestamp, AggregateData);
            _rollingValues.OnRemoved += _rollingValues_OnRemoved;
        }
        ~LOBImbalanceStudy()
        {
            Dispose(false);
        }
        private static void AggregateData(LOBImbalance existing, LOBImbalance newItem)
        {
            // Update the existing bucket with the new values
            existing.Timestamp = newItem.Timestamp;
            existing.Value = newItem.Value;
        }
        public IReadOnlyList<LOBImbalance> Data => _rollingValues.AsReadOnly();
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
            
            
            CalculateStudy();
        }
        private void CalculateStudy()
        {
            var newItem = new LOBImbalance() { Value = (decimal)_orderBook.ImbalanceValue, Timestamp = DateTime.Now, MarketMidPrice = (decimal)_orderBook.MidPrice };
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
