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

namespace VisualHFT.ViewModel
{
    public class vmPosition : BindableBase
    {

        private string _selectedSymbol;
        private string _selectedStrategy;
        private DateTime _selectedDate;
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        BackgroundWorker bwLoadPositions = new BackgroundWorker();
        ObservableCollection<Exposure> _exposures;
        ObservableCollection<PositionEx> _positions;
        ObservableCollection<ExecutionVM> _executions;
        ObservableCollection<OrderVM> _activeOrders;
        object _lockActiveOrders = new object();

        public vmPosition(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            this._dialogs = dialogs;
            _positions = new ObservableCollection<PositionEx>();
            _exposures = new ObservableCollection<Exposure>();
            _activeOrders = new ObservableCollection<OrderVM>();
            RaisePropertyChanged(nameof(Exposures));
            RaisePropertyChanged(nameof(ActiveOrders));


            HelperCommon.CLOSEDPOSITIONS.OnDataReceived += CLOSEDPOSITIONS_OnDataReceived;
			HelperCommon.CLOSEDPOSITIONS.OnInitialLoad += CLOSEDPOSITIONS_OnInitialLoad;
            

            HelperCommon.EXPOSURES.OnDataReceived += EXPOSURES_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataReceived += ACTIVEORDERS_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataRemoved += ACTIVEORDERS_OnDataRemoved;
            this.SelectedDate = DateTime.Now;
        }


        private void ACTIVEORDERS_OnDataRemoved(object sender, OrderVM e)
        {
            if (e == null || string.IsNullOrEmpty(_selectedStrategy))
                return;
            var existingItem = _activeOrders.Where(x => x.ClOrdId == e.ClOrdId).FirstOrDefault();
            if (existingItem != null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => {
                    lock (_lockActiveOrders)
                        _activeOrders.Remove(existingItem);
                }));                
            }
        }

        private void ACTIVEORDERS_OnDataReceived(object sender, OrderVM e)
        {
            if (e == null || string.IsNullOrEmpty(_selectedStrategy))
                return;
            if (e != null)
            {
                lock (_lockActiveOrders)
                {
                    var existingItem = _activeOrders.Where(x => x.ClOrdId == e.ClOrdId).FirstOrDefault();
                    if (existingItem == null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            _activeOrders.Add(e);
                        }));

                    }
                    else
                    {
                        existingItem.Update(e);
                    }
                }
            }
        }

        private void EXPOSURES_OnDataReceived(object sender, Exposure e)
        {
            if (e == null || string.IsNullOrEmpty(_selectedStrategy))
                return;
            var existingItem = _exposures.Where(x => x.Symbol == e.Symbol && x.StrategyName == _selectedStrategy).FirstOrDefault();
            if (existingItem == null)
                _exposures.Add(e);
            else
            {
                //UPPDATE
                if (existingItem.SizeExposed != e.SizeExposed)
                    existingItem.SizeExposed = e.SizeExposed;
                if (existingItem.UnrealizedPL != e.UnrealizedPL)
                    existingItem.UnrealizedPL = e.UnrealizedPL;
            }
        }

        private void CLOSEDPOSITIONS_OnInitialLoad(object sender, IEnumerable<PositionEx> e)
        {
            ReloadPositions(_selectedSymbol, _selectedStrategy);
        }

        private void CLOSEDPOSITIONS_OnDataReceived(object sender, IEnumerable<PositionEx> e)
        {
            foreach(var pos in e)
            {
                if ((string.IsNullOrEmpty(_selectedSymbol) || pos.Symbol == _selectedSymbol) && (string.IsNullOrEmpty(_selectedStrategy) || pos.StrategyCode == _selectedStrategy))
                {
                    if (_positions == null)
                        _positions = new ObservableCollection<PositionEx>();
                    _positions.Insert(0, pos);
                    foreach (var exec in pos.AllExecutions.Where(x => x.Status == ePOSITIONSTATUS.FILLED || x.Status == ePOSITIONSTATUS.PARTIALFILLED))
                        _executions.Insert(0, exec);
                }                
            }
		}

        public ObservableCollection<Exposure> Exposures => _exposures;
        public ObservableCollection<PositionEx> Positions => _positions;
        public ObservableCollection<ExecutionVM> Executions => _executions;
        public ObservableCollection<OrderVM> ActiveOrders => _activeOrders;

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


        private void ReloadPositions(string symbol, string strategyCode)
        {
            if (!bwLoadPositions.WorkerSupportsCancellation)
            {
                bwLoadPositions.WorkerSupportsCancellation = true; //use it to know if it was already setup
                bwLoadPositions.DoWork += (s, args) =>
                {
                    try
                    {
                        if (symbol == "-- All symbols --")
                            symbol = "";
                        var closedPositions = HelperCommon.CLOSEDPOSITIONS.Positions
                            .Where(x =>
                                    (string.IsNullOrEmpty(_selectedSymbol) || x.Symbol == _selectedSymbol)
                                    &&
                                    (string.IsNullOrEmpty(_selectedStrategy) || x.StrategyCode == _selectedStrategy)
                                    && x.CreationTimeStamp.Date == _selectedDate.Date
                             ).ToList();                        
                        _positions = new ObservableCollection<PositionEx>(closedPositions.OrderByDescending(x => x.CloseTimeStamp));
                        //EXECUTIONS
                        #region Executions
                        var open = _positions.SelectMany(x => x.OpenExecutions).Where(x => (ePOSITIONSTATUS)x.Status == ePOSITIONSTATUS.FILLED || (ePOSITIONSTATUS)x.Status == ePOSITIONSTATUS.PARTIALFILLED);
                        var close = _positions.SelectMany(x => x.CloseExecutions).Where(x => (ePOSITIONSTATUS)x.Status == ePOSITIONSTATUS.FILLED || (ePOSITIONSTATUS)x.Status == ePOSITIONSTATUS.PARTIALFILLED);
                        List<ExecutionVM> allExecution = new List<ExecutionVM>();
                        allExecution.AddRange(open);
                        allExecution.AddRange(close);
                        _executions = new ObservableCollection<ExecutionVM>(allExecution.OrderByDescending(x => x.LocalTimeStamp));
                        #endregion
                    }
                    catch (Exception ex) 
                    { 
                        Console.Write(ex.ToString());
                    }
                };
                bwLoadPositions.RunWorkerCompleted += (s, args) =>
                {
                    RaisePropertyChanged(nameof(Exposures));
                    RaisePropertyChanged(nameof(ActiveOrders));
                    RaisePropertyChanged(nameof(Positions));
                    RaisePropertyChanged(nameof(Executions));
                };
            }
            if (!bwLoadPositions.IsBusy)
                bwLoadPositions.RunWorkerAsync();
        }
    }
}
