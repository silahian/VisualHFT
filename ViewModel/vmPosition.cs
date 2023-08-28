using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using VisualHFT.Helpers;
using VisualHFT.Model;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Prism.Mvvm;
using System.Windows.Input;
using System.Windows.Controls;
//using GalaSoft.MvvmLight.Command;
using System.Windows.Data;
using GalaSoft.MvvmLight.Command;
using QuickFix.Fields;
using System.Threading;

namespace VisualHFT.ViewModel
{
    public class vmPosition : BindableBase, IDisposable
    {

        private string _selectedSymbol;
        private string _selectedStrategy;
        private DateTime _selectedDate;
        private Dictionary<string, Func<string, string, bool>> _dialogs;        
        private ObservableCollection<OrderVM> _allOrders;
        private ObservableCollection<PositionManager> _positionsManager;
        private readonly object _locker = new object();
        private bool _disposed = false; // to track whether the object has been disposed

        public ICollectionView OrdersView { get; }
        public ICommand FilterCommand { get; }


        public vmPosition(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            /*if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;*/


            this._dialogs = dialogs;
            PositionsManager = new ObservableCollection<PositionManager>();            
            FilterCommand = new RelayCommand<string>(OnFilterChanged);
            this.SelectedDate = DateTime.Now; //new DateTime(2022, 10, 6); 

            lock (_locker)
            {
                _allOrders = new ObservableCollection<OrderVM>();
                OrdersView = CollectionViewSource.GetDefaultView(_allOrders);
                OrdersView.SortDescriptions.Add(new SortDescription("CreationTimeStamp", ListSortDirection.Descending));
            }


            HelperCommon.EXECUTEDORDERS.OnInitialLoad += EXECUTEDORDERS_OnInitialLoad;
            HelperCommon.EXECUTEDORDERS.OnDataReceived += EXECUTEDORDERS_OnDataReceived;
            HelperCommon.EXPOSURES.OnDataReceived += EXPOSURES_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataReceived += ACTIVEORDERS_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataRemoved += ACTIVEORDERS_OnDataRemoved;
        }
        ~vmPosition()
        {
            Dispose(false);
        }

        private void ACTIVEORDERS_OnDataRemoved(object sender, OrderVM e)
        {
            if (e == null || string.IsNullOrEmpty(_selectedStrategy))
                return;
            
            OrderVM existingItem = null;
            lock (_locker)
                existingItem = _allOrders.Where(x => x.ClOrdId == e.ClOrdId).FirstOrDefault();
            if (existingItem != null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => {
                    lock (_locker)
                        _allOrders.Remove(existingItem);
                }));                
            }

        }
        private void ACTIVEORDERS_OnDataReceived(object sender, OrderVM e)
        {
            if (e == null || string.IsNullOrEmpty(_selectedStrategy))
                return;
            if (e != null)
            {
                OrderVM existingItem = null;

                lock (_locker)
                    existingItem = _allOrders.Where(x => x.ClOrdId == e.ClOrdId).FirstOrDefault();
                if (existingItem == null)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                    {
                        lock (_locker)
                            _allOrders.Add(e);
                    }));
                }
                else
                {
                    existingItem.Update(e);
                }
            }
        }
        private void EXPOSURES_OnDataReceived(object sender, Exposure e)
        {
            if (e == null || string.IsNullOrEmpty(_selectedStrategy))
                return;
            /*var existingItem = _exposures.Where(x => x.Symbol == e.Symbol && x.StrategyName == _selectedStrategy).FirstOrDefault();
            if (existingItem == null)
                _exposures.Add(e);
            else
            {
                //UPPDATE
                if (existingItem.SizeExposed != e.SizeExposed)
                    existingItem.SizeExposed = e.SizeExposed;
                if (existingItem.UnrealizedPL != e.UnrealizedPL)
                    existingItem.UnrealizedPL = e.UnrealizedPL;
            }*/
        }
        private void EXECUTEDORDERS_OnInitialLoad(object sender, IEnumerable<OrderVM> e)
        {
            if (_allOrders.Count == 0 && !e.Any() ) return;

            ReloadOrders(e.ToList());
        }
        private void EXECUTEDORDERS_OnDataReceived(object sender, IEnumerable<OrderVM> e)
        {
            lock (_locker)
            {
                foreach (var ord in e)
                    _allOrders.Add(ord);
            }
        }


        public ObservableCollection<OrderVM> AllOrders
        {
            get => _allOrders;
            set
            {
                lock (_locker) 
                    SetProperty(ref _allOrders, value);
            }
        }
        public ObservableCollection<PositionManager> PositionsManager
        {
            get => _positionsManager;
            set
            {
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
        private void ReloadOrders(List<OrderVM> orders)
        {
            string symbol = _selectedSymbol;
            if (_selectedSymbol == "-- All symbols --")
                symbol = "";
            var grpSymbols = orders.GroupBy(x => x.Symbol).ToList();
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => {
                //POSITION MANAGER
                _positionsManager.Clear();
                foreach (var grp in grpSymbols)
                {
                    var existing_item = _positionsManager.Where(x => x.Symbol == grp.Key).FirstOrDefault();
                    if (existing_item == null)
                        _positionsManager.Add(new PositionManager(grp.ToList(), PositionManagerCalculationMethod.FIFO));
                    else
                        existing_item = new PositionManager(grp.ToList(), PositionManagerCalculationMethod.FIFO);
                }
                lock (_locker)
                {
                    _allOrders.Clear();
                    _allOrders.AddRange(orders);
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
                        var order = (OrderVM)o;
                        return new[] { "NONE", "SENT", "NEW", "PARTIALFILLED", "CANCELEDSENT", "REPLACESENT", "REPLACED" }.Contains(order.Status.ToString())
                               && order.CreationTimeStamp.Date == _selectedDate.Date;
                    };
                    break;
                case "Filled":
                    OrdersView.Filter = o =>
                    {
                        var order = (OrderVM)o;
                        return order.Status.ToString() == "FILLED"
                               && order.CreationTimeStamp.Date == _selectedDate.Date;
                    };
                    break;
                case "Cancelled":
                    OrdersView.Filter = o =>
                    {
                        var order = (OrderVM)o;
                        return new[] { "CANCELED", "REJECTED" }.Contains(order.Status.ToString())
                               && order.CreationTimeStamp.Date == _selectedDate.Date;
                    };
                    break;
                case "All":
                    OrdersView.Filter = o => ((OrderVM)o).CreationTimeStamp.Date == _selectedDate.Date;
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
                    HelperCommon.EXPOSURES.OnDataReceived -= EXPOSURES_OnDataReceived;
                    HelperCommon.ACTIVEORDERS.OnDataReceived -= ACTIVEORDERS_OnDataReceived;
                    HelperCommon.ACTIVEORDERS.OnDataRemoved -= ACTIVEORDERS_OnDataRemoved;
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
