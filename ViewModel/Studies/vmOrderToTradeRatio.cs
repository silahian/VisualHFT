using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VisualHFT.Studies;
using VisualHFT.Model;
using Prism.Mvvm;
using System.Linq;
using VisualHFT.Helpers;
using QuickFix.Fields;
using System.Windows.Threading;
using System;
using System.Windows;
using System.Collections.Generic;

namespace VisualHFT.ViewModels
{
    public class vmOrderToTradeRatio : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private OrderToTradeRatioStudy _ottStudy;
        private IReadOnlyList<BaseStudyModel> _chartData;
        private ObservableCollection<VisualHFT.ViewModel.Model.Provider> _providers;
        private ObservableCollection<string> _symbols;
        private VisualHFT.ViewModel.Model.Provider _selectedProvider;
        private string _selectedSymbol;
        private AggregationLevel _aggregationLevelSelection;
        private int _MAX_ITEMS = 500;
        private UIUpdater uiUpdater;
        public vmOrderToTradeRatio()
        {
            ChartData = new List<BaseStudyModel>();
            _symbols = new ObservableCollection<string>(HelperCommon.ALLSYMBOLS.ToList());
            _providers = HelperCommon.PROVIDERS.CreateObservableCollection();
            RaisePropertyChanged(nameof(Providers));
            RaisePropertyChanged(nameof(Symbols));

            HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperCommon.ALLSYMBOLS.CollectionChanged += ALLSYMBOLS_CollectionChanged;


            
            AggregationLevels = new ObservableCollection<Tuple<string, AggregationLevel>>();
            foreach (AggregationLevel level in Enum.GetValues(typeof(AggregationLevel)))
            {
                AggregationLevels.Add(new Tuple<string, AggregationLevel>(HelperCommon.GetEnumDescription(level), level));
            }
            AggregationLevelSelection = AggregationLevel.Automatic;

            uiUpdater = new UIUpdater(uiUpdaterAction);
        }
        ~vmOrderToTradeRatio()
        {
            Dispose(false);
        }
        public IReadOnlyList<BaseStudyModel> ChartData
        {
            get 
            {
                return _chartData; 
            }
            set
            {
                SetProperty(ref _chartData, value);
            }            
        }
        public ObservableCollection<VisualHFT.ViewModel.Model.Provider> Providers { get => _providers; set => _providers = value; }
        public ObservableCollection<string> Symbols { get => _symbols; set => _symbols = value; }
        public VisualHFT.ViewModel.Model.Provider SelectedProvider
        {
            get => _selectedProvider;
            set => SetProperty(ref _selectedProvider, value, onChanged: () => Clear());
        }
        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set => SetProperty(ref _selectedSymbol, value, onChanged: () => Clear());

        }
        public AggregationLevel AggregationLevelSelection
        {
            get => _aggregationLevelSelection;
            set => SetProperty(ref _aggregationLevelSelection, value, onChanged: () => Clear());
        }
        public ObservableCollection<Tuple<string, AggregationLevel>> AggregationLevels { get; set; }

        private void uiUpdaterAction()
        {
            RaisePropertyChanged(nameof(ChartData));
        }
        private void ALLSYMBOLS_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _symbols = new ObservableCollection<string>(HelperCommon.ALLSYMBOLS.ToList());
            RaisePropertyChanged(nameof(Symbols));
        }
        private void PROVIDERS_OnDataReceived(object sender, VisualHFT.ViewModel.Model.Provider e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                _providers.Add(e);
                if (_selectedProvider == null && e.Status == eSESSIONSTATUS.BOTH_CONNECTED) //default provider must be the first who's Active
                    SelectedProvider = e;
            }));
        }
        private void _ottStudy_OnRollingAdded(object sender, BaseStudyModel e)
        {
            _chartData = _ottStudy.Data;
        }
        private void Clear()
        {
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider == null)
                return;

            if (_ottStudy != null)
                _ottStudy.Dispose();
            _ottStudy = null;
            _ottStudy = new OrderToTradeRatioStudy(_selectedSymbol, _selectedProvider.ProviderCode, _aggregationLevelSelection, _MAX_ITEMS);
            _ottStudy.OnRollingAdded += _ottStudy_OnRollingAdded;
            RaisePropertyChanged("ChartData");
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    uiUpdater.Dispose();
                    if (_ottStudy != null)
                    {
                        _ottStudy.OnRollingAdded -= _ottStudy_OnRollingAdded; ;
                        _ottStudy.Dispose();
                    }
                    HelperCommon.PROVIDERS.OnDataReceived -= PROVIDERS_OnDataReceived;
                    HelperCommon.ALLSYMBOLS.CollectionChanged -= ALLSYMBOLS_CollectionChanged;
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
