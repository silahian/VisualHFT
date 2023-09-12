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
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Wpf;
using System.Collections.Generic;

namespace VisualHFT.ViewModels
{
    public class vmVPIN : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private VPINStudy _vpinStudy;
        private IReadOnlyList<BaseStudyModel> _vpinChartData;
        private ObservableCollection<ProviderEx> _providers;
        private ObservableCollection<string> _symbols;
        private ProviderEx _selectedProvider;
        private string _selectedSymbol;
        private decimal _bucketVolumeSize = 1;
        private AggregationLevel _aggregationLevelSelection;
        private int _MAX_ITEMS = 500;
        private const decimal VPIN_THRESHOLD = 0.8M; // Example threshold
        private UIUpdater uiUpdater;

        public vmVPIN()
        {
            VPINChartData = new List<BaseStudyModel>();
            _symbols = new ObservableCollection<string>(HelperCommon.ALLSYMBOLS.ToList());
            _providers = new ObservableCollection<ProviderEx>(HelperCommon.PROVIDERS.Select(x => x.Value).ToList());
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
        ~vmVPIN()
        {
            Dispose(false);
        }
        public IReadOnlyList<BaseStudyModel> VPINChartData
        {
            get 
            {
                return _vpinChartData; 
            }
            set
            {
                SetProperty(ref _vpinChartData, value);
            }            
        }
        public ObservableCollection<ProviderEx> Providers { get => _providers; set => _providers = value; }
        public ObservableCollection<string> Symbols { get => _symbols; set => _symbols = value; }
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
        public decimal BucketVolumeSize
        {
            get => _bucketVolumeSize;
            set => SetProperty(ref _bucketVolumeSize, value, onChanged: () => Clear());
        }

        private void uiUpdaterAction()
        {
            RaisePropertyChanged(nameof(VPINChartData));
        }
        private void ALLSYMBOLS_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _symbols = new ObservableCollection<string>(HelperCommon.ALLSYMBOLS.ToList());
            RaisePropertyChanged(nameof(Symbols));
        }
        private void PROVIDERS_OnDataReceived(object sender, ProviderEx e)
        {
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

        private void _vpinStudy_OnRollingAdded(object sender, BaseStudyModel e)
        {
            _vpinChartData = _vpinStudy.VpinData;
        }

        private void Clear()
        {
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider == null)
                return;

            if (_vpinStudy != null) 
                _vpinStudy.Dispose();
            _vpinStudy = null;
            _vpinStudy = new VPINStudy(_selectedSymbol, _selectedProvider.ProviderCode, _aggregationLevelSelection, _bucketVolumeSize, _MAX_ITEMS);
            _vpinStudy.OnRollingAdded += _vpinStudy_OnRollingAdded;

            RaisePropertyChanged("VPINChartData");
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    uiUpdater.Dispose();
                    if (_vpinStudy != null)
                    {
                        _vpinStudy.OnRollingAdded -= _vpinStudy_OnRollingAdded;
                        _vpinStudy.Dispose();
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
