using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Threading;
using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.Studies;
using VisualHFT.UserSettings;
using Prism.Commands;
using VisualHFT.ViewModels;
using System.Windows.Media;

namespace VisualHFT.ViewModel.Studies
{
    public class MetricTileViewModel: BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private string _tile_id;
        private eTILES_TYPE _type;
        private string _value;
        private string _title;
        private string _tooltip;
        private ObservableCollection<ProviderEx> _providers;
        private ObservableCollection<string> _symbols;
        private ProviderEx _selectedProvider;
        private string _selectedSymbol;
        private bool _areSymbolsLoaded = false;
        private bool _areProvidersLoaded = false;
        private UIUpdater uiUpdater;

        //*********************************************************
        //*********************************************************
        //Studies
        private LOBImbalanceStudy _lobStudy;
        private VPINStudy _vpinStudy;
        private TradeToOrderRatioStudy _ttoStudy;
        private OrderToTradeRatioStudy _ottStudy;
        private MarketResilienceStudy _marketResilienceStudy;
        private MarketResilienceBiasStudy _marketResilienceBiasStudy;
        //*********************************************************
        //*********************************************************


        private bool _isSettingsOpen;
        private TileSettings _settings;
        private SolidColorBrush _valueColor = Brushes.White;

        public MetricTileViewModel(eTILES_TYPE type, string tile_id)
        {
            _tile_id = tile_id;
            _type = type;
            
            SaveSettingsCommand = new RelayCommand(param => SaveSetting(), param => CanSaveSetting());
            OpenChartCommand = new RelayCommand(OpenChartClick);

            _symbols = new ObservableCollection<string>(HelperCommon.ALLSYMBOLS.ToList());
            _providers = new ObservableCollection<ProviderEx>(HelperCommon.PROVIDERS.Select(x => x.Value).ToList());
            RaisePropertyChanged(nameof(Providers));
            RaisePropertyChanged(nameof(Symbols));

            HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperCommon.ALLSYMBOLS.CollectionChanged += ALLSYMBOLS_CollectionChanged;

            uiUpdater = new UIUpdater(uiUpdaterAction);
        }
        ~MetricTileViewModel(){Dispose(false);}

        private void uiUpdaterAction()
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                // This means the method is being called from a non-UI thread
                throw new InvalidOperationException("uiUpdaterAction is being accessed from a non-UI thread");
            }


            RaisePropertyChanged(nameof(Value));
            RaisePropertyChanged(nameof(ValueColor));
        }
        private void _lobStudy_OnRollingAdded(object sender, BaseStudyModel e)
        {
            _value = e.ValueFormatted;
        }
        private void _vpinStudy_OnRollingAdded(object sender, BaseStudyModel e)
        {
            _value = e.ValueFormatted;
        }
        private void _ttoStudy_OnRollingAdded(object sender, BaseStudyModel e)
        {            
            _value = e.ValueFormatted;
        }
        private void _ottStudy_OnRollingAdded(object sender, BaseStudyModel e)
        {
            _value = e.ValueFormatted;
        }
        private void _marketResilienceStudy_OnRollingAdded(object sender, BaseStudyModel e)
        {
            _value = e.ValueFormatted;
        }
        private void _marketResilienceBiasStudy_OnRollingAdded(object sender, BaseStudyModel e)
        {
            _value = e.ValueFormatted;
            if (e.ValueColor != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _valueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(e.ValueColor));
                });

            }

        }


        private void ALLSYMBOLS_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _symbols = new ObservableCollection<string>(HelperCommon.ALLSYMBOLS.ToList());
            RaisePropertyChanged(nameof(Symbols));
            _areSymbolsLoaded = true;
            if (_settings == null && _areProvidersLoaded && _areSymbolsLoaded)
            {
                LoadSetting();
            }
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
                    _areProvidersLoaded = true;
                }));

                if (_selectedProvider == null && e.Status == eSESSIONSTATUS.BOTH_CONNECTED) //default provider must be the first who's Active
                    SelectedProvider = cleanProvider;               
            }
            if (_settings == null && _areProvidersLoaded && _areSymbolsLoaded)
                LoadSetting();

        }
        public ICommand SaveSettingsCommand { get; set; }
        public ICommand OpenChartCommand { get; private set; }
        public ObservableCollection<ProviderEx> Providers { get => _providers; set => _providers = value; } 
        public ObservableCollection<string> Symbols { get => _symbols; set => _symbols = value; }
        public ProviderEx SelectedProvider
        {
            get => _selectedProvider;
            set => SetProperty(ref _selectedProvider, value);
        }
        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set => SetProperty(ref _selectedSymbol, value);

        }
        public string Value { get => _value; set => SetProperty(ref _value, value); }
        public SolidColorBrush ValueColor { get => _valueColor; set => SetProperty(ref _valueColor, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Tooltip { get => _tooltip; set => SetProperty(ref _tooltip, value); }
        public bool IsSettingsOpen { get => _isSettingsOpen; set => SetProperty(ref _isSettingsOpen, value); }
        private void OpenChartClick(object obj)
        {
            if (_lobStudy != null)
            {
                var winModel = new vmLOBImbalances();
                winModel.SelectedProvider = _selectedProvider;
                winModel.SelectedSymbol = _selectedSymbol;
                winModel.Symbols = _symbols;
                winModel.Providers = _providers;
                winModel.AggregationLevelSelection = _lobStudy.AggregationLevel;
                var frmChart = new View.LOBImbalances();
                frmChart.DataContext = winModel;
                frmChart.Show();
            }
            else if (_vpinStudy != null)
            {
                var winModel = new vmVPIN();
                winModel.SelectedProvider = _selectedProvider;
                winModel.SelectedSymbol = _selectedSymbol;
                winModel.Symbols = _symbols;
                winModel.Providers = _providers;
                winModel.AggregationLevelSelection = _vpinStudy.AggregationLevel;
                winModel.BucketVolumeSize = _vpinStudy.BucketVolumeSize;
                var frmChart = new View.VPIN();
                frmChart.DataContext = winModel;
                frmChart.Show();
            }
            else if (_ttoStudy != null)
            {
                var winModel = new vmTradeToOrderRatio();
                winModel.SelectedProvider = _selectedProvider;
                winModel.SelectedSymbol = _selectedSymbol;
                winModel.Symbols = _symbols;
                winModel.Providers = _providers;
                winModel.AggregationLevelSelection = _ttoStudy.AggregationLevel;
                var frmChart = new View.TradeToOrderRatio();
                frmChart.DataContext = winModel;
                frmChart.Show();
            }
            else if (_ottStudy != null)
            {
                var winModel = new vmOrderToTradeRatio();
                winModel.SelectedProvider = _selectedProvider;
                winModel.SelectedSymbol = _selectedSymbol;
                winModel.Symbols = _symbols;
                winModel.Providers = _providers;
                winModel.AggregationLevelSelection = _ottStudy.AggregationLevel;
                var frmChart = new View.OrderToTradeRatio();
                frmChart.DataContext = winModel;
                frmChart.Show();
            }
            else if (_marketResilienceStudy != null)
            {
                var winModel = new vmMarketResilience();
                winModel.SelectedProvider = _selectedProvider;
                winModel.SelectedSymbol = _selectedSymbol;
                winModel.Symbols = _symbols;
                winModel.Providers = _providers;
                winModel.AggregationLevelSelection = _marketResilienceStudy.AggregationLevel;
                var frmChart = new View.MarketResilience();
                frmChart.DataContext = winModel;
                frmChart.Show();
            }
            else if (_marketResilienceBiasStudy != null)
            {
                var winModel = new vmMarketResilienceBias();
                winModel.SelectedProvider = _selectedProvider;
                winModel.SelectedSymbol = _selectedSymbol;
                winModel.Symbols = _symbols;
                winModel.Providers = _providers;
                winModel.AggregationLevelSelection = _marketResilienceBiasStudy.AggregationLevel;
                var frmChart = new View.MarketResilienceBias();
                frmChart.DataContext = winModel;
                frmChart.Show();
            }
            else
            {
                throw new NotImplementedException();
            }

        }


        private void LoadSetting()
        {
            _settings = SettingsManager.Instance.GetSetting<TileSettings>(SettingKey.TILE_STUDY, _tile_id);
            if (_settings != null)
            {
                _selectedProvider = _providers.Where(x => x.ProviderCode == _settings.ProviderID).FirstOrDefault();
                _selectedSymbol = _settings.Symbol;
                if (string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider == null)
                {
                    _settings = null;
                    //IsSettingsOpen = true;
                    return;
                }
                RaisePropertyChanged(nameof(SelectedProvider));
                RaisePropertyChanged(nameof(SelectedSymbol));
                IsSettingsOpen = false;
                Value = "..";

                if (_type == eTILES_TYPE.STUDY_LOB_IMBALANCE)
                {
                    if (_lobStudy != null)
                        _lobStudy.Dispose();
                    _lobStudy = new LOBImbalanceStudy(_selectedSymbol, _selectedProvider.ProviderCode, AggregationLevel.Ms500, 1);
                    _lobStudy.OnRollingAdded += _lobStudy_OnRollingAdded;
                    
                }
                else if (_type == eTILES_TYPE.STUDY_VPIN)
                {
                    if (_vpinStudy != null)
                        _vpinStudy.Dispose();
                    _vpinStudy = new VPINStudy(_selectedSymbol, _selectedProvider.ProviderCode, AggregationLevel.Ms500, 1);
                    _vpinStudy.OnRollingAdded += _vpinStudy_OnRollingAdded;
                }
                else if (_type == eTILES_TYPE.STUDY_TTO)
                {
                    if (_ttoStudy != null)
                        _ttoStudy.Dispose();
                    _ttoStudy = new TradeToOrderRatioStudy(_selectedSymbol, _selectedProvider.ProviderCode, AggregationLevel.Ms500, 1);
                    _ttoStudy.OnRollingAdded += _ttoStudy_OnRollingAdded;
                }
                else if (_type == eTILES_TYPE.STUDY_OTT)
                {
                    if (_ottStudy != null)
                        _ottStudy.Dispose();
                    _ottStudy = new OrderToTradeRatioStudy(_selectedSymbol, _selectedProvider.ProviderCode, AggregationLevel.Ms500, 1);
                    _ottStudy.OnRollingAdded += _ottStudy_OnRollingAdded;
                }
                else if (_type == eTILES_TYPE.STUDY_MARKETRESILIENCE)
                {
                    if (_marketResilienceStudy != null)
                        _marketResilienceStudy.Dispose();
                    _marketResilienceStudy = new MarketResilienceStudy(_selectedSymbol, _selectedProvider.ProviderCode, AggregationLevel.Ms500);
                    _marketResilienceStudy.OnRollingAdded += _marketResilienceStudy_OnRollingAdded;
                }
                else if (_type == eTILES_TYPE.STUDY_MARKETRESILIENCEBIAS)
                {
                    if (_marketResilienceBiasStudy != null)
                        _marketResilienceBiasStudy.Dispose();
                    _marketResilienceBiasStudy = new MarketResilienceBiasStudy(_selectedSymbol, _selectedProvider.ProviderCode, AggregationLevel.Ms500);
                    _marketResilienceBiasStudy.OnRollingAdded += _marketResilienceBiasStudy_OnRollingAdded;
                }
            }
            else //NO SETTINGS AVILABLE
                SaveInitialSettings();
        }
        private void SaveInitialSettings()
        {
            _selectedSymbol = Symbols.First();
            _selectedProvider = _providers.SkipWhile(x => x.Status == eSESSIONSTATUS.BOTH_DISCONNECTED).FirstOrDefault();
            SaveSetting();
        }
        private void SaveSetting()
        {
            IsSettingsOpen = false;
            SettingsManager.Instance.SetSetting(SettingKey.TILE_STUDY, _tile_id, new TileSettings()
            {
                Symbol = _selectedSymbol,
                ProviderID = _selectedProvider.ProviderCode
            });
            LoadSetting();
        }

        private bool CanSaveSetting()
        {
            // Implement your logic here to determine if the SaveSetting method can be executed
            // For now, let's assume it can always execute
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_lobStudy != null)
                    {
                        _lobStudy.OnRollingAdded -= _lobStudy_OnRollingAdded;
                        _lobStudy.Dispose();
                    }
                    if (_vpinStudy != null)
                    {
                        _vpinStudy.OnRollingAdded -= _vpinStudy_OnRollingAdded;
                        _vpinStudy.Dispose();
                    }
                    if (_ttoStudy != null)
                    {
                        _ttoStudy.OnRollingAdded -= _ttoStudy_OnRollingAdded;
                        _ttoStudy.Dispose();
                    }
                    if (_ottStudy != null)
                    {
                        _ottStudy.OnRollingAdded -= _ottStudy_OnRollingAdded;
                        _ottStudy.Dispose();
                    }
                    if (_marketResilienceStudy != null)
                    {
                        _marketResilienceStudy.OnRollingAdded -= _marketResilienceStudy_OnRollingAdded;
                        _marketResilienceStudy.Dispose();
                    }
                    if (_marketResilienceBiasStudy != null)
                    {
                        _marketResilienceBiasStudy.OnRollingAdded -= _marketResilienceBiasStudy_OnRollingAdded;
                        _marketResilienceBiasStudy.Dispose();
                    }
                    HelperCommon.PROVIDERS.OnDataReceived -= PROVIDERS_OnDataReceived;
                    HelperCommon.ALLSYMBOLS.CollectionChanged -= ALLSYMBOLS_CollectionChanged;
                    uiUpdater.Dispose();
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
