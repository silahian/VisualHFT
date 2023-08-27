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


namespace VisualHFT.ViewModels
{
    public class vmLOBImbalances : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private LOBImbalanceStudy _lobStudy;
        private ObservableCollection<LOBImbalance> _chartData;
        private ObservableCollection<ProviderEx> _providers;
        private ObservableCollection<string> _symbols;
        private ProviderEx _selectedProvider;
        private string _selectedSymbol;
        private AggregationLevel _aggregationLevelSelection;
        private int _MAX_ITEMS = 500;

        public vmLOBImbalances()
        {
            ChartData = new ObservableCollection<LOBImbalance>();
            _symbols = new ObservableCollection<string>(HelperCommon.ALLSYMBOLS.ToList());
            RaisePropertyChanged(nameof(Symbols));

            HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperCommon.ALLSYMBOLS.CollectionChanged += ALLSYMBOLS_CollectionChanged;


            
            AggregationLevels = new ObservableCollection<Tuple<string, AggregationLevel>>();
            foreach (AggregationLevel level in Enum.GetValues(typeof(AggregationLevel)))
            {
                AggregationLevels.Add(new Tuple<string, AggregationLevel>(HelperCommon.GetEnumDescription(level), level));
            }
            AggregationLevelSelection = AggregationLevel.Automatic;

        }
        ~vmLOBImbalances()
        {
            Dispose(false);
        }
        public ObservableCollection<LOBImbalance> ChartData
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
        public ObservableCollection<ProviderEx> Providers => _providers;
        public ObservableCollection<string> Symbols => _symbols;
        public ProviderEx SelectedProvider
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


        private void ALLSYMBOLS_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _symbols = new ObservableCollection<string>(HelperCommon.ALLSYMBOLS.ToList());
            RaisePropertyChanged(nameof(Symbols));
        }
        private void PROVIDERS_OnDataReceived(object sender, ProviderEx e)
        {
            if (_providers == null)
            {
                _providers = new ObservableCollection<ProviderEx>();
                RaisePropertyChanged(nameof(Providers));
            }
            if (!_providers.Any(x => x.ProviderName == e.ProviderName))
            {
                var cleanProvider = new ProviderEx();
                cleanProvider.ProviderName = e.ProviderName;
                cleanProvider.ProviderCode = e.ProviderCode;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    _providers.Add(cleanProvider);
                }));

                if (_selectedProvider == null && e.Status == eSESSIONSTATUS.BOTH_CONNECTED) //default provider must be the first who's Active
                    SelectedProvider = cleanProvider;
            }
        }
        private void _lobStudy_OnRollingRemoved(object sender, int e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (_chartData.Any())
                    _chartData.RemoveAt(e);
            }));
        }
        private void _lobStudy_OnRollingAdded(object sender, LOBImbalance e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                _chartData.Add(e);
            }));
        }
        private void _lobStudy_OnRollingUpdated(object sender, LOBImbalance e)
        {
            //being updated inside the Study class
        }
        private void Clear()
        {
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider == null)
                return;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                _chartData.Clear();
            }));
            RaisePropertyChanged("ChartData");
            if (_lobStudy != null) 
                _lobStudy.Dispose();
            _lobStudy = null;
            _lobStudy = new LOBImbalanceStudy(_selectedSymbol, _selectedProvider.ProviderCode, _aggregationLevelSelection, _MAX_ITEMS);
            _lobStudy.OnRollingAdded += _lobStudy_OnRollingAdded;
            _lobStudy.OnRollingUpdated += _lobStudy_OnRollingUpdated;
            _lobStudy.OnRollingRemoved += _lobStudy_OnRollingRemoved;
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_lobStudy != null)
                    {
                        _lobStudy.OnRollingAdded -= _lobStudy_OnRollingAdded; ;
                        _lobStudy.OnRollingUpdated -= _lobStudy_OnRollingUpdated;
                        _lobStudy.OnRollingRemoved -= _lobStudy_OnRollingRemoved;
                        _lobStudy.Dispose();
                    }
                }
                HelperCommon.PROVIDERS.OnDataReceived -= PROVIDERS_OnDataReceived;
                HelperCommon.ALLSYMBOLS.CollectionChanged -= ALLSYMBOLS_CollectionChanged;

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
