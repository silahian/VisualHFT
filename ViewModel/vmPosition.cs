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

namespace VisualHFT.ViewModel
{
    public class vmPosition : BindableBase
    {

        private string _selectedSymbol;
        private string _selectedStrategy;
        private DateTime _selectedDate;
        private Dictionary<string, Func<string, string, bool>> _dialogs;        
        private List<PositionEx> _positions;
        private ObservableCollection<OrderVM> _allOrders;
        private ObservableCollection<PositionManager> _positionsManager;
        private object _locker = new object();

        public ICollectionView OrdersView { get; }
        public ICommand FilterCommand { get; }


        public vmPosition(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            /*if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;*/


            this._dialogs = dialogs;
            _positions = new List<PositionEx>();
            PositionsManager = new ObservableCollection<PositionManager>();

            HelperCommon.CLOSEDPOSITIONS.OnDataReceived += CLOSEDPOSITIONS_OnDataReceived;
            HelperCommon.CLOSEDPOSITIONS.OnInitialLoad += CLOSEDPOSITIONS_OnInitialLoad;
            HelperCommon.EXPOSURES.OnDataReceived += EXPOSURES_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataReceived += ACTIVEORDERS_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataRemoved += ACTIVEORDERS_OnDataRemoved;
            this.SelectedDate = DateTime.Now; //new DateTime(2022, 10, 6);

            FilterCommand = new RelayCommand<string>(OnFilterChanged);


            lock (_locker)
            {
                _allOrders = new ObservableCollection<OrderVM>();
                OrdersView = CollectionViewSource.GetDefaultView(_allOrders);
                OrdersView.SortDescriptions.Add(new SortDescription("CreationTimeStamp", ListSortDirection.Descending));
            }
            
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
        private void CLOSEDPOSITIONS_OnInitialLoad(object sender, IEnumerable<PositionEx> e)
        {

            if (_positions.Count == 0 && !e.Any()) return;	

            ReloadPositions(_selectedSymbol, _selectedStrategy);
        }
        private void CLOSEDPOSITIONS_OnDataReceived(object sender, IEnumerable<PositionEx> e)
        {
            foreach(var pos in e)
            {
                if ((string.IsNullOrEmpty(_selectedSymbol) || pos.Symbol == _selectedSymbol) && (string.IsNullOrEmpty(_selectedStrategy) || pos.StrategyCode == _selectedStrategy))
                {
                    if (_positions == null)
                        _positions = new List<PositionEx>();
                    _positions.Insert(0, pos);
                    lock (_locker)
                    {
                        foreach (var ord in pos.GetOrders())
                            _allOrders.Add(ord);
                    }
                }
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
            set => SetProperty(ref _selectedSymbol, value, onChanged: () => ReloadPositions(_selectedSymbol, _selectedStrategy));

        }
        public string SelectedStrategy
        {
            get => _selectedStrategy;
            set => SetProperty(ref _selectedStrategy, value, onChanged: () => ReloadPositions(_selectedSymbol, _selectedStrategy));
        }
        public DateTime SelectedDate
        {            
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value, onChanged: () => HelperCommon.CLOSEDPOSITIONS.SessionDate = _selectedDate);
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
        private void ReloadPositions(string symbol, string strategyCode)
        {
            if (symbol == "-- All symbols --")
                symbol = "";
            var closedPositions = HelperCommon.CLOSEDPOSITIONS.Positions
                .Where(x => x.CreationTimeStamp.Date == _selectedDate.Date).ToList();
            _positions = new List<PositionEx>(closedPositions.OrderByDescending(x => x.CloseTimeStamp));

            //ALL ORDERS
            // Update the ObservableCollection on the UI thread
            var source = closedPositions.Select(x => x.GetOrders()).SelectMany(x => x).OrderByDescending(x => x.CreationTimeStamp).ToList();
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (_locker)
                    _allOrders.Clear();
                AllOrders.AddRange(source);
            }));
            // Refresh the OrdersView after making changes to the underlying collection
            OrdersView.Refresh();
            SelectedFilter = "Working";
            ApplyFilter();

            //POSITION MANAGER
            var grpSymbols = source.GroupBy(x => x.Symbol).ToList();
            foreach (var grp in grpSymbols)
            {
                var existing_item = _positionsManager.Where(x => x.Symbol == grp.Key).FirstOrDefault();
                if (existing_item == null)
                    _positionsManager.Add(new PositionManager(grp.ToList(), PositionManagerCalculationMethod.FIFO));
                else
                    existing_item = new PositionManager(grp.ToList(), PositionManagerCalculationMethod.FIFO);
            }

        }
        private void ApplyFilter()
        {
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
    }
}
