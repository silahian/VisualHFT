using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VisualHFT.Model;


namespace VisualHFT.DataTradeRetriever
{
    public class GenericTradesRetriever : IDataTradeRetriever, IDisposable
    {
        private List<VisualHFT.Model.Position> _positions;
        private List<VisualHFT.Model.Order> _orders;
        private object _locker = new object();

        int _providerId;
        string _providerName;
        DateTime? _sessionDate = null;

        private bool _disposed = false;

        public event EventHandler<IEnumerable<VisualHFT.Model.Order>> OnInitialLoad;
        public event EventHandler<IEnumerable<VisualHFT.Model.Order>> OnDataReceived;
        public event EventHandler<VisualHFT.Model.Execution> OnExecutionReceived;
        public event EventHandler<Order> OnDataUpdated;

        public GenericTradesRetriever()
        {
            _positions = new List<VisualHFT.Model.Position>();
            _orders = new List<VisualHFT.Model.Order>();

            HelperTimeProvider.OnSetFixedTime += HelperTimeProvider_OnSetFixedTime;
        }
        ~GenericTradesRetriever()
        {
            Dispose(false);
        }
        private void HelperTimeProvider_OnSetFixedTime(object? sender, EventArgs e)
        {
            if (_sessionDate != HelperTimeProvider.Now.Date)
                SessionDate = HelperTimeProvider.Now.Date;
        }

        public DateTime? SessionDate
        {
            get { return _sessionDate; }
            set
            {
                if (value != _sessionDate)
                {
                    _sessionDate = value;
                    _orders.Clear();
                    _positions.Clear();
                    OnInitialLoad?.Invoke(this, this.Orders);
                }
            }
        }
        public ReadOnlyCollection<VisualHFT.Model.Order> Orders
        {
            get { return _orders.AsReadOnly(); }
        }
        public ReadOnlyCollection<VisualHFT.Model.Position> Positions
        {
            get { return _positions.AsReadOnly(); }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {


                }
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void AddOrder(Order? order)
        {
            if (order == null)
                return;
            lock (_locker)
            {
                var existingOrder = _orders.FirstOrDefault(x => x.OrderID == order.OrderID);
                if (existingOrder == null)
                {
                    _orders.Add(order);
                    OnDataReceived?.Invoke(this, new List<Order> { order });
                }
                else
                {
                    existingOrder = order;
                    OnDataUpdated?.Invoke(this, order);
                }
            }

        }
    }
}
