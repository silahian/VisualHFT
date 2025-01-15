using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using VisualHFT.Helpers;
using VisualHFT.Model;
using System.Windows.Threading;
using Prism.Mvvm;
using System.Windows.Input;
using System.Windows.Data;
using HelperCommon = VisualHFT.Commons.Helpers.HelperCommon;
using VisualHFT.Enums;

namespace VisualHFT.ViewModel
{
    public class vmPosition : BindableBase, IDisposable
    {

        private string _selectedSymbol;
        private string _selectedStrategy;
        private DateTime _selectedDate;
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        private ObservableCollection<VisualHFT.Model.Order> _allOrders;
        private ObservableCollection<PositionManager> _positionsManager;
        private readonly object _lockOrders = new object();
        private readonly object _lockPosMgr = new object();
        private bool _disposed = false; // to track whether the object has been disposed
        private volatile bool _isDispatcherActionRunning = false;

        public ICollectionView OrdersView { get; }
        public ICommand FilterCommand { get; }
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public vmPosition(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            PositionsManager = new ObservableCollection<PositionManager>();
            FilterCommand = new RelayCommand<string>(OnFilterChanged);
            this.SelectedDate = HelperTimeProvider.Now; //new DateTime(2022, 10, 6); 
            lock (_lockOrders)
            {
                _allOrders = new ObservableCollection<VisualHFT.Model.Order>();
                OrdersView = CollectionViewSource.GetDefaultView(_allOrders);
                OrdersView.SortDescriptions.Add(new SortDescription("CreationTimeStamp", ListSortDirection.Descending));
                SelectedFilter = "Working";
                ApplyFilter();
            }

            HelperCommon.EXECUTEDORDERS.OnInitialLoad += EXECUTEDORDERS_OnInitialLoad;
            HelperCommon.EXECUTEDORDERS.OnDataReceived += EXECUTEDORDERS_OnDataReceived;
            HelperCommon.EXECUTEDORDERS.OnDataUpdated += EXECUTEDORDERS_OnDataUpdated;
            HelperOrderBook.Instance.Subscribe(LIMITORDERBOOK_OnDataReceived);

            HelperTimeProvider.OnSetFixedTime += HelperTimeProvider_OnSetFixedTime;
        }


        ~vmPosition()
        {
            Dispose(false);
        }
        private void HelperTimeProvider_OnSetFixedTime(object? sender, EventArgs e)
        {
            if (_selectedDate != HelperTimeProvider.Now.Date)
                SelectedDate = HelperTimeProvider.Now.Date;
        }
        private void EXECUTEDORDERS_OnInitialLoad(object sender, IEnumerable<VisualHFT.Model.Order> e)
        {
            if (_allOrders.Count == 0 && !e.Any()) return;

            ReloadOrders(e.ToList());
        }

        private void EXECUTEDORDERS_OnDataReceived(object sender, IEnumerable<VisualHFT.Model.Order> e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                lock (_lockOrders)
                {
                    foreach (var ord in e)
                    {
                        _allOrders.Add(ord);
                    }
                }
            }));
        }

        private void EXECUTEDORDERS_OnDataUpdated(object? sender, Order e)
        {
            if (e.Status == eORDERSTATUS.FILLED || e.Status == eORDERSTATUS.PARTIALFILLED)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    lock (_lockPosMgr)
                    {
                        var posBySymbol = _positionsManager.Where(x => x.Symbol == e.Symbol).FirstOrDefault();
                        if (posBySymbol == null)
                        {
                            _positionsManager.Add(new PositionManager(new List<Order>() { e }, PositionManagerCalculationMethod.FIFO));
                            posBySymbol = _positionsManager.Where(x => x.Symbol == e.Symbol).FirstOrDefault();
                        }
                        posBySymbol.AddOrder(e);
                    }
                }));
            }

            if (_isDispatcherActionRunning)
                return;
            _isDispatcherActionRunning = true;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                OrdersView.Refresh();
                _isDispatcherActionRunning = false;
            }));
        }
        private void LIMITORDERBOOK_OnDataReceived(OrderBook e)
        {
            lock (_lockPosMgr)
            {
                if (_positionsManager.Count > 0)
                {
                    var posMgrBySymbol = _positionsManager.FirstOrDefault(x => x.Symbol == e.Symbol);
                    if (posMgrBySymbol != null)
                        posMgrBySymbol.UpdateLastMidPrice(e.MidPrice);
                }
            }
        }

        public ObservableCollection<VisualHFT.Model.Order> AllOrders
        {
            get
            {
                lock (_lockOrders)
                    return _allOrders;
            }
            set
            {
                lock (_lockOrders)
                    SetProperty(ref _allOrders, value);
            }
        }
        public ObservableCollection<PositionManager> PositionsManager
        {
            get
            {
                lock (_lockPosMgr)
                    return _positionsManager;
            }
            set
            {
                lock (_lockPosMgr)
                    SetProperty(ref _positionsManager, value);
            }
        }
        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set => SetProperty(ref _selectedSymbol, value, onChanged: () => ReloadOrders(HelperCommon.EXECUTEDORDERS.Orders.ToList()));

        }
        public string SelectedStrategy
        {
            get => _selectedStrategy;
            set => SetProperty(ref _selectedStrategy, value, onChanged: () => ReloadOrders(HelperCommon.EXECUTEDORDERS.Orders.ToList()));
        }
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value, onChanged: () => HelperCommon.EXECUTEDORDERS.SessionDate = _selectedDate);
        }
        private string _selectedFilter = "Working";
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                SetProperty(ref _selectedFilter, value);
                ApplyFilter();
            }
        }
        public bool IsAllFilterSelected => _selectedFilter == "All";
        private void OnFilterChanged(string filter)
        {
            SelectedFilter = filter;
        }
        private void ReloadOrders(List<VisualHFT.Model.Order> orders)
        {
            string symbol = _selectedSymbol;
            if (_selectedSymbol == "-- All symbols --")
                symbol = "";
            var grpSymbols = orders.GroupBy(x => x.Symbol).ToList();
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => {
                //POSITION MANAGER
                lock (_lockPosMgr)
                {
                    _positionsManager.Clear();
                    foreach (var grp in grpSymbols)
                    {
                        var existing_item = _positionsManager.Where(x => x.Symbol == grp.Key).FirstOrDefault();
                        if (existing_item == null)
                            _positionsManager.Add(new PositionManager(grp.ToList(), PositionManagerCalculationMethod.FIFO));
                        else
                            existing_item = new PositionManager(grp.ToList(), PositionManagerCalculationMethod.FIFO);
                    }

                }
                lock (_lockOrders)
                {
                    _allOrders.Clear();
                    orders.ForEach(x => _allOrders.Add(x));
                }
                SelectedFilter = "Working";
            }));

            RaisePropertyChanged(nameof(AllOrders));
        }
        private void ApplyFilter()
        {
            if (string.IsNullOrEmpty(SelectedFilter))
                return;
            switch (SelectedFilter)
            {
                case "Working":
                    OrdersView.Filter = o =>
                    {
                        var order = (VisualHFT.Model.Order)o;
                        return new[] { "NONE", "SENT", "NEW", "PARTIALFILLED", "CANCELEDSENT", "REPLACESENT", "REPLACED" }.Contains(order.Status.ToString())
                               && order.CreationTimeStamp.Date == _selectedDate.Date;
                    };
                    break;
                case "Filled":
                    OrdersView.Filter = o =>
                    {
                        var order = (VisualHFT.Model.Order)o;
                        return (order.Status.ToString() == "FILLED" || order.Status.ToString() == "PARTIALFILLED")
                               && order.CreationTimeStamp.Date == _selectedDate.Date;
                    };
                    break;
                case "Cancelled":
                    OrdersView.Filter = o =>
                    {
                        var order = (VisualHFT.Model.Order)o;
                        return new[] { "CANCELED", "REJECTED" }.Contains(order.Status.ToString())
                               && order.CreationTimeStamp.Date == _selectedDate.Date;
                    };
                    break;
                case "All":
                    OrdersView.Filter = o => ((VisualHFT.Model.Order)o).CreationTimeStamp.Date == _selectedDate.Date;
                    //OrdersView.Filter = null;
                    break;
            }
            // Ensure the sort description is still in place
            if (!OrdersView.SortDescriptions.Any(sd => sd.PropertyName == "CreationTimeStamp"))
            {
                OrdersView.SortDescriptions.Add(new SortDescription("CreationTimeStamp", ListSortDirection.Descending));
            }

            OrdersView.Refresh();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    HelperCommon.EXECUTEDORDERS.OnInitialLoad -= EXECUTEDORDERS_OnInitialLoad;
                    HelperCommon.EXECUTEDORDERS.OnDataReceived -= EXECUTEDORDERS_OnDataReceived;
                    HelperCommon.EXECUTEDORDERS.OnDataUpdated -= EXECUTEDORDERS_OnDataUpdated;
                    HelperOrderBook.Instance.Unsubscribe(LIMITORDERBOOK_OnDataReceived);

                    HelperTimeProvider.OnSetFixedTime -= HelperTimeProvider_OnSetFixedTime;
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
