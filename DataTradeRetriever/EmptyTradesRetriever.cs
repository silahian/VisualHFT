using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using VisualHFT.Helpers;
using VisualHFT.Model;
using QuickFix.Fields;
using QuickFix.DataDictionary;
using System.Windows.Shapes;


namespace VisualHFT.DataTradeRetriever
{
    public class EmptyTradesRetriever : IDataTradeRetriever, IDisposable
    {
        private List<PositionEx> _positions;
        private List<OrderVM> _orders;
        int _providerId;
        string _providerName;
        DateTime? _sessionDate = null;

        private bool _disposed = false;

        public event EventHandler<IEnumerable<OrderVM>> OnInitialLoad;
        public event EventHandler<IEnumerable<OrderVM>> OnDataReceived;
        protected virtual void RaiseOnInitialLoad(IEnumerable<OrderVM> ord) => OnInitialLoad?.Invoke(this, ord);
        protected virtual void RaiseOnDataReceived(IEnumerable<OrderVM> ord) => OnDataReceived?.Invoke(this, ord);
        public EmptyTradesRetriever()
        {
            _positions = new List<PositionEx>();
            _orders = new List<OrderVM>();
        }
        ~EmptyTradesRetriever()
        {
            Dispose(false);
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
                    RaiseOnInitialLoad(this.Orders);
                }
            }
        }
        public ReadOnlyCollection<OrderVM> Orders
        {
            get { return _orders.AsReadOnly(); }
        }
        public ReadOnlyCollection<PositionEx> Positions
        {
            get { return _positions.AsReadOnly(); }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {}
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
