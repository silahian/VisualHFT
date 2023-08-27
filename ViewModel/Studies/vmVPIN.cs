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

namespace VisualHFT.ViewModels
{
    public class vmVPIN : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private VPINStudy _vpinStudy;
        private ObservableCollection<VPIN> _vpinChartData;
        private ObservableCollection<ProviderEx> _providers;
        private ObservableCollection<string> _symbols;
        private ProviderEx _selectedProvider;
        private string _selectedSymbol;
        private decimal _bucketVolumeSize = 1;
        private const decimal VPIN_THRESHOLD = 0.8M; // Example threshold

        public vmVPIN()
        {
            VPINChartData = new ObservableCollection<VPIN>();
            _symbols = new ObservableCollection<string>(HelperCommon.ALLSYMBOLS.ToList());
            RaisePropertyChanged(nameof(Symbols));

            HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperCommon.ALLSYMBOLS.CollectionChanged += ALLSYMBOLS_CollectionChanged;
        }
        ~vmVPIN()
        {
            Dispose(false);
        }
        public ObservableCollection<VPIN> VPINChartData
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
        public decimal BucketVolumeSize
        {
            get => _bucketVolumeSize;
            set => SetProperty(ref _bucketVolumeSize, value, onChanged: () => Clear());
        }

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
        private void _vpinStudy_VPINRollingRemoved(object sender, int e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                _vpinChartData.RemoveAt(e);
            }));
        }
        private void _vpinStudy_VPINRollingAdded(object sender, VPIN e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                _vpinChartData.Add(e);
            }));
        }
        private void _vpinStudy_VPINRollingUpdated(object sender, VPIN e)
        {
            //being updated inside the Study class
        }
        private void Clear()
        {
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider == null)
                return;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                _vpinChartData.Clear();
            }));
            if (_vpinStudy != null) 
                _vpinStudy.Dispose();
            _vpinStudy = null;
            _vpinStudy = new VPINStudy(_selectedSymbol, _selectedProvider.ProviderCode, _bucketVolumeSize);
            _vpinStudy.VPINRollingAdded += _vpinStudy_VPINRollingAdded;
            _vpinStudy.VPINRollingUpdated += _vpinStudy_VPINRollingUpdated;
            _vpinStudy.VPINRollingRemoved += _vpinStudy_VPINRollingRemoved;
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_vpinStudy != null)
                    {
                        _vpinStudy.VPINRollingAdded -= _vpinStudy_VPINRollingAdded;
                        _vpinStudy.VPINRollingUpdated -= _vpinStudy_VPINRollingUpdated;
                        _vpinStudy.VPINRollingRemoved -= _vpinStudy_VPINRollingRemoved;
                        _vpinStudy.Dispose();
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
