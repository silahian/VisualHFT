using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using VisualHFT.Helpers;
using System.IO;
using VisualHFT.Model;
using System.Windows.Data;
using System.Runtime.CompilerServices;

namespace VisualHFT.ViewModel
{
    public class vmPosition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _selectedSymbol;
        private string _selectedStrategy;
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        System.ComponentModel.BackgroundWorker bwLoadPositions = new System.ComponentModel.BackgroundWorker();
        ObservableCollection<Exposure> _exposures;
        ObservableCollection<PositionEx> _positions;
        ObservableCollection<ExecutionVM> _executions;
        ObservableCollection<OrderVM> _activeOrders;

        public vmPosition(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            this._dialogs = dialogs;
            _positions = new ObservableCollection<PositionEx>();
            _exposures = new ObservableCollection<Exposure>();
            _activeOrders = new ObservableCollection<OrderVM>();
            RaisePropertyChanged("Exposures");
            RaisePropertyChanged("ActiveOrders");


            HelperCommon.CLOSEDPOSITIONS.Positions.CollectionChanged += ClosedPositions_CollectionChanged;
			HelperCommon.CLOSEDPOSITIONS.OnInitialLoad += CLOSEDPOSITIONS_OnInitialLoad;
            //HelperCommon.OPENPOSITIONS.Positions.CollectionChanged += OpenedPositions_CollectionChanged;
            HelperCommon.EXPOSURES.OnDataReceived += EXPOSURES_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataReceived += ACTIVEORDERS_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataRemoved += ACTIVEORDERS_OnDataRemoved;
        }

        private void ACTIVEORDERS_OnDataRemoved(object sender, OrderVM e)
        {
            _activeOrders.Remove(e);
        }

        private void ACTIVEORDERS_OnDataReceived(object sender, OrderVM e)
        {
            _activeOrders.Add(e);
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

        private void ClosedPositions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
			{
				foreach (var pos in e.NewItems as List<PositionEx>)
				{
					if ((string.IsNullOrEmpty(_selectedSymbol) || pos.Symbol == _selectedSymbol) && (string.IsNullOrEmpty(_selectedStrategy) || pos.StrategyCode == _selectedStrategy))
					{
						if (_positions == null)
							_positions = new ObservableCollection<PositionEx>();
						_positions.Add(pos);
						foreach (var exec in pos.AllExecutions.Where(x => x.Status == ePOSITIONSTATUS.FILLED || x.Status == ePOSITIONSTATUS.PARTIALFILLED))
							_executions.Add(exec);
					}
				}
				RaisePropertyChanged("Positions");
				RaisePropertyChanged("Executions");
			}
		}

        public DateTime CurrentSession { get; set; }
        public ObservableCollection<Exposure> Exposures
        {
            get
            {
                return _exposures;
            }
        }
        public ObservableCollection<PositionEx> Positions
        {
            get
            {                
                return _positions;
            }
        }
        public ObservableCollection<ExecutionVM> Executions
        {
            get
            {
                return _executions;
            }
        }
        public ObservableCollection<OrderVM> ActiveOrders
        {
            get
            {
                return _activeOrders;
            }
        }

        public string SelectedSymbol
        {
            get { return _selectedSymbol; }
            set
            {
                if (_selectedSymbol != value)
                {
                    _selectedSymbol = value;                    
                    RaisePropertyChanged("SelectedSymbol");
                    ReloadPositions(_selectedSymbol, _selectedStrategy);
                }
            }
        }
        public string SelectedStrategy
        {
            get { return _selectedStrategy; }
            set
            {
                if (_selectedStrategy != value)
                {
                    _selectedStrategy = value;                    
                    RaisePropertyChanged("SelectedStrategy");
                    ReloadPositions(_selectedSymbol, _selectedStrategy);
                }
            }
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
                        var closedPositions = HelperCommon.CLOSEDPOSITIONS.Positions.Where(x => (string.IsNullOrEmpty(_selectedSymbol) || x.Symbol == _selectedSymbol) && (string.IsNullOrEmpty(_selectedStrategy) || x.StrategyCode == _selectedStrategy)).ToList();
                        var openPositions = HelperCommon.OPENPOSITIONS.Positions.Where(x => (string.IsNullOrEmpty(_selectedSymbol) || x.Symbol == _selectedSymbol) && (string.IsNullOrEmpty(_selectedStrategy) || x.StrategyCode == _selectedStrategy));
                        if (openPositions != null && openPositions.Any())
                            closedPositions.AddRange(openPositions);
                        _positions = new ObservableCollection<PositionEx>(closedPositions.OrderByDescending(x => x.CloseTimeStamp));
                        //EXECUTIONS
                        #region Executions
                        var open = _positions.SelectMany(x => x.OpenExecutions).Where(x => (ePOSITIONSTATUS)x.Status == ePOSITIONSTATUS.FILLED || (ePOSITIONSTATUS)x.Status == ePOSITIONSTATUS.PARTIALFILLED).Select(x => new ExecutionVM(x, _selectedSymbol));
                        var close = _positions.SelectMany(x => x.CloseExecutions).Where(x => (ePOSITIONSTATUS)x.Status == ePOSITIONSTATUS.FILLED || (ePOSITIONSTATUS)x.Status == ePOSITIONSTATUS.PARTIALFILLED).Select(x => new ExecutionVM(x, _selectedSymbol));
                        List<ExecutionVM> allExecution = new List<ExecutionVM>();
                        allExecution.AddRange(open);
                        allExecution.AddRange(close);
                        _executions = new ObservableCollection<ExecutionVM>(allExecution.OrderByDescending(x => x.LocalTimeStamp));
                        #endregion
                    }
                    catch (Exception ex) { }
                };
                bwLoadPositions.RunWorkerCompleted += (s, args) =>
                {
                    RaisePropertyChanged("Positions");
                    RaisePropertyChanged("Executions");
                };
            }
            if (!bwLoadPositions.IsBusy)
                bwLoadPositions.RunWorkerAsync();
        }
    }
}
