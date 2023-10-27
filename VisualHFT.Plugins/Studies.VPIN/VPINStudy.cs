using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VisualHFT.Commons.PluginManager;
using VisualHFT.Commons.Pools;
using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.PluginManager;
using VisualHFT.Studies.VPIN.Model;
using VisualHFT.Studies.VPIN.UserControls;
using VisualHFT.Studies.VPIN.ViewModel;
using VisualHFT.UserSettings;

namespace VisualHFT.Studies
{
    /// <summary>
    /// The VPIN (Volume-Synchronized Probability of Informed Trading) value is a measure of the imbalance between buy and sell volumes in a given bucket. It's calculated as the absolute difference between buy and sell volumes divided by the total volume (buy + sell) for that bucket.
    /// 
    /// Given this definition, the range of VPIN values is between 0 and 1:
    ///     0: This indicates a perfect balance between buy and sell volumes in the bucket. In other words, the number of buy trades is equal to the number of sell trades.
    ///     1: This indicates a complete imbalance, meaning all the trades in the bucket are either all buys or all sells.
    /// Most of the time, the VPIN value will be somewhere between these two extremes, indicating some level of imbalance between buy and sell trades. The closer the VPIN value is to 1, the greater the imbalance, and vice versa.
    /// </summary>
    public class VPINStudy : BasePluginStudy
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private PlugInSettings _settings;
        private OrderBook _orderBook; //to hold last market data tick

        //variables for calculation
        private readonly decimal _bucketVolumeSize; // The volume size of each bucket
        private decimal _currentBuyVolume = 0; // Current volume of buy trades in the bucket
        private decimal _currentSellVolume = 0; // Current volume of sell trades in the bucket
        private decimal _lastMarketMidPrice = 0; //keep track of market price
        private decimal _lastVPIN = 0;
        private object _locker = new object();

        private const decimal VPIN_THRESHOLD = 0.7M; // ALERT Example threshold

        private DateTime _currentBucketStartTime;
        private DateTime _currentBucketEndTime;

        // Event declaration
        public override event EventHandler<decimal> OnAlertTriggered;
        public override event EventHandler<BaseStudyModel> OnCalculated;
        public override event EventHandler<ErrorEventArgs> OnError;


        public override string Name { get; set; } = "VPIN Study Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Calculates VPIN.";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action CloseSettingWindow { get; set; }
        public override string TileTitle { get; set; } = "VPIN";
        public override string TileToolTip { get; set; } = "The <b>VPIN</b> (Volume - Synchronized Probability of Informed Trading) value is a measure of the imbalance between buy and sell volumes in a given bucket.<br/><br/>" +
                "It's calculated as the absolute difference between buy and sell volumes divided by the total volume (buy + sell) for that bucket.<br/>";
        public decimal BucketVolumeSize => _bucketVolumeSize;


        public VPINStudy()
        {

            HelperOrderBook.Instance.Subscribe(LIMITORDERBOOK_OnDataReceived);
            HelperTrade.Instance.Subscribe(TRADES_OnDataReceived);


            CalculateStudy(true); //initial value
        }
        ~VPINStudy()
        {
            Dispose(false);
        }



        private void TRADES_OnDataReceived(Trade e)
        {
            if (e == null)
                return;
            if (_settings.Provider.ProviderID != e.ProviderId || _settings.Symbol != e.Symbol)
                return;
            lock (_locker)
            {
                // Set the start time for the current bucket if it's a new bucket
                if (_currentBuyVolume == 0 && _currentSellVolume == 0)
                {
                    _currentBucketStartTime = e.Timestamp;
                }

                if (e.IsBuy)
                    _currentBuyVolume += e.Size;
                else
                    _currentSellVolume += e.Size;

                // Check if the bucket is filled
                if (_currentBuyVolume + _currentSellVolume >= _bucketVolumeSize)
                {
                    _currentBucketStartTime = e.Timestamp;
                    _currentBucketEndTime = e.Timestamp; // Set the end time for the current bucket
                    CalculateStudy(true);
                    ResetBucket();
                }
                else
                {
                    _currentBucketEndTime = e.Timestamp; // Set the end time for the current bucket
                    CalculateStudy(false);
                }
            }
        }
        private void LIMITORDERBOOK_OnDataReceived(OrderBook e)
        {
            if (e == null)
                return;
            if (_settings.Provider.ProviderID != e.ProviderID || _settings.Symbol != e.Symbol)
                return;

            lock (_locker)
            {
                if (_orderBook == null)
                {
                    _orderBook = new OrderBook(e.Symbol, e.DecimalPlaces);
                }

                if (!_orderBook.LoadData(e.Asks, e.Bids))
                    return; //if nothing to update, then exit
                _lastMarketMidPrice = (decimal)_orderBook.MidPrice;

                if (_lastMarketMidPrice != 0)
                    CalculateStudy(false);
            }
        }
        private void CalculateStudy(bool isNewBucket)
        {
            if (Status != VisualHFT.PluginManager.ePluginStatus.STARTED) return;
            decimal vpin = 0;
            if ((_currentBuyVolume + _currentSellVolume) > 0)
                vpin = (decimal)Math.Abs(_currentBuyVolume - _currentSellVolume) / (_currentBuyVolume + _currentSellVolume);
            if (isNewBucket)
            {
                // Check against threshold and trigger alert
                if (vpin > VPIN_THRESHOLD)
                    OnAlertTriggered?.Invoke(this, vpin);
                _lastVPIN = vpin;
            }
            // Add to rolling window and remove oldest if size exceeded
            var newItem = new BaseStudyModel();
            newItem.Value = _lastVPIN;
            newItem.ValueFormatted = _lastVPIN.ToString("N1");
            newItem.Timestamp = DateTime.Now;
            newItem.MarketMidPrice = _lastMarketMidPrice;
            OnCalculated?.Invoke(this, newItem);
        }
        private void ResetBucket()
        {
            _currentBuyVolume = 0;
            _currentSellVolume = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                    HelperOrderBook.Instance.Unsubscribe(LIMITORDERBOOK_OnDataReceived);
                    HelperTrade.Instance.Unsubscribe(TRADES_OnDataReceived);

                    _orderBook?.Dispose();
                    _orderBook = null;
                    
                }

                // Dispose unmanaged resources here, if any

                _disposed = true;
            }
        }

        protected override void LoadSettings()
        {
            _settings = LoadFromUserSettings<PlugInSettings>();
            if (_settings == null)
            {
                InitializeDefaultSettings();
            }
            if (_settings.Provider == null) //To prevent back compability with older setting formats
            {
                _settings.Provider = new Provider();
            }
        }

        protected override void SaveSettings()
        {
            SaveToUserSettings(_settings);
        }

        protected override void InitializeDefaultSettings()
        {
            _settings = new PlugInSettings()
            {
                BucketVolSize = 1,
                Symbol = "",
                Provider = new ViewModel.Model.Provider(),
                AggregationLevel = AggregationLevel.Automatic
            };
            SaveToUserSettings(_settings);
        }
        public override object GetUISettings()
        {
            PluginSettingsView view = new PluginSettingsView();
            PluginSettingsViewModel viewModel = new PluginSettingsViewModel(CloseSettingWindow);
            viewModel.BucketVolumeSize = _settings.BucketVolSize;
            viewModel.SelectedSymbol = _settings.Symbol;
            viewModel.SelectedProviderID = _settings.Provider.ProviderID;
            viewModel.AggregationLevelSelection = _settings.AggregationLevel;

            viewModel.UpdateSettingsFromUI = () =>
            {
                _settings.BucketVolSize = viewModel.BucketVolumeSize;
                _settings.Symbol = viewModel.SelectedSymbol;
                _settings.Provider = viewModel.SelectedProvider;
                _settings.AggregationLevel = viewModel.AggregationLevelSelection;

                SaveSettings();

                // Start the Reconnection 
                //  It will allow to reload with the new values
                Task.Run(() =>
                {
                    ResetBucket();
                });
            };
            // Display the view, perhaps in a dialog or a new window.
            view.DataContext = viewModel;
            return view;
        }

    }
}
