using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using VisualHFT.Helpers;
using VisualHFT.Model;

namespace VisualHFT.DataTradeRetriever
{
    public class MSSQLServerTradesRetriever : IDataTradeRetriever, IDisposable
    {
        private const int POLLING_INTERVAL = 5000; // Interval for polling the database
        private long? _LAST_POSITION_ID = null;
        private List<PositionEx> _positions;
        private List<OrderVM> _orders;
        private DateTime? _sessionDate = null;
        private readonly System.Timers.Timer _timer;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource(); // Added cancellation token source
        private readonly HFTEntities _DB = null;
        private readonly object _lock = new object();
        private bool _disposed = false; // to track whether the object has been disposed


        public event EventHandler<IEnumerable<OrderVM>> OnInitialLoad;
        public event EventHandler<IEnumerable<OrderVM>> OnDataReceived;
        protected virtual void RaiseOnInitialLoad(IEnumerable<OrderVM> ord) => OnInitialLoad?.Invoke(this, ord);
        protected virtual void RaiseOnDataReceived(IEnumerable<OrderVM> ord) => OnDataReceived?.Invoke(this, ord);


        public MSSQLServerTradesRetriever()
        {
            _positions = new List<PositionEx>();
            _orders =  new List<OrderVM>();
            _timer = new System.Timers.Timer(POLLING_INTERVAL);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            _timer_Elapsed(null, null);

            _DB = new HFTEntities();
            _DB.Database.CommandTimeout = 6000;
            _DB.Configuration.ValidateOnSaveEnabled = false;
            _DB.Configuration.AutoDetectChangesEnabled = false;
            _DB.Configuration.LazyLoadingEnabled = false;
        }
        ~MSSQLServerTradesRetriever()
        {
            Dispose(false);
        }
        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop(); // Stop the timer while the operation is running
            if (_cancellationTokenSource.IsCancellationRequested) return; // Check for cancellation


            var res = await GetPositionsAsync();
            if (res != null && res.Any())
            {
                foreach (var p in res)
                {
                    if (!p.PipsPnLInCurrency.HasValue || p.PipsPnLInCurrency == 0)
                    {
                        p.PipsPnLInCurrency = (p.GetCloseQuantity * p.GetCloseAvgPrice) - (p.GetOpenQuantity * p.GetOpenAvgPrice);
                        if (p.Side == ePOSITIONSIDE.Sell)
                        {
                            p.PipsPnLInCurrency *= -1;
                        }
                    }
                    if (!HelperCommon.ALLSYMBOLS.Contains(p.Symbol))
                    {
                        //this collection needs to be updated in the UI thread
                        Application.Current.Dispatcher.Invoke(() => HelperCommon.ALLSYMBOLS.Add(p.Symbol));
                    }
                    _positions.Add(p);
                }

                if (this.Orders == null || !this.Orders.Any())
                {
                    _orders = res.Select(x => x.GetOrders()).SelectMany(x => x).ToList();
                    RaiseOnInitialLoad(this.Orders);
                }
                else
                {
                    var ordersToNotify = new List<OrderVM>();
                    foreach (var p in res)
                    {
                        _orders.InsertRange(0, p.GetOrders());
                        ordersToNotify.AddRange(p.GetOrders());
                    }
                    RaiseOnDataReceived(ordersToNotify);
                }
            }
            _timer.Start(); // Restart the timer once the operation is complete
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
                    _LAST_POSITION_ID = null;
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
        private async Task<IEnumerable<PositionEx>> GetPositionsAsync()
        {
            if (!SessionDate.HasValue || _cancellationTokenSource.IsCancellationRequested) return null;

            if (!SessionDate.HasValue)
                return null;
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        var targetDate = SessionDate.Value.Date;
                        var simpleCheckOfNewRecords = _DB.Positions
                            .Where(x => DbFunctions.TruncateTime(x.CreationTimeStamp) == targetDate &&
                                        (!_LAST_POSITION_ID.HasValue || x.ID > _LAST_POSITION_ID.Value))
                            .ToList();

                        if (!simpleCheckOfNewRecords.Any()) { return null; }

                        var allProviders = _DB.Providers.ToList();
                        var result = _DB.Positions.Include("OpenExecutions").Include("CloseExecutions").Where(x => DbFunctions.TruncateTime(x.CreationTimeStamp) == targetDate && (!_LAST_POSITION_ID.HasValue || x.ID > _LAST_POSITION_ID.Value)).ToList();
                        if (result.Any())
                        {
                            _LAST_POSITION_ID = result.Max(x => x.ID);

                            var ret = result.Select(x => new PositionEx(x)).ToList(); //convert to our model
                                                                                      //find provider's name
                            ret.ForEach(x =>
                            {
                                x.CloseProviderName = allProviders.Where(p => p.ProviderCode == x.CloseProviderId).DefaultIfEmpty(new Provider()).FirstOrDefault().ProviderName;
                                x.OpenProviderName = allProviders.Where(p => p.ProviderCode == x.OpenProviderId).DefaultIfEmpty(new Provider()).FirstOrDefault().ProviderName;

                                x.CloseExecutions.ForEach(ex => ex.ProviderName = x.CloseProviderName);
                                x.OpenExecutions.ForEach(ex => ex.ProviderName = x.OpenProviderName);
                            });
                            return ret;
                        }
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        return null;
                    }
                }
            });

            //return null;
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer.Elapsed -= _timer_Elapsed;
                    _timer?.Stop();
                    _timer?.Dispose();
                    _cancellationTokenSource?.Cancel(); // Cancel any ongoing operations
                    _cancellationTokenSource?.Dispose();
                    _DB.Dispose();
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
