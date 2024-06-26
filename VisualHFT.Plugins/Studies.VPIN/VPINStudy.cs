using System;
using System.Threading.Tasks;
using VisualHFT.Commons.PluginManager;
using VisualHFT.Enums;
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

        //variables for calculation
        private decimal _bucketVolumeSize; // The volume size of each bucket
        private decimal _currentBuyVolume = 0; // Current volume of buy trades in the bucket
        private decimal _currentSellVolume = 0; // Current volume of sell trades in the bucket
        private decimal _lastMarketMidPrice = 0; //keep track of market price
        private object _locker = new object();

        private BookItem _tobBid = new BookItem();
        private BookItem _tobAsk = new BookItem();

        private const decimal VPIN_THRESHOLD = 0.7M; // ALERT Example threshold

        private DateTime _currentBucketStartTime;
        private DateTime _currentBucketEndTime;

        // Event declaration
        public override event EventHandler<decimal> OnAlertTriggered;

        public override string Name { get; set; } = "VPIN Study Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Calculates VPIN.";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action CloseSettingWindow { get; set; }
        public override string TileTitle { get; set; } = "VPIN";
        public override string TileToolTip { get; set; } = "<b>Volume-Synchronized Probability of Informed Trading</b> (VPIN) is a real-time metric that measures the imbalance between buy and sell volumes, reflecting potential market risk or instability. <br/>VPIN is crucial for traders and analysts to gauge market sentiment and anticipate liquidity and volatility shifts.<br/><br/>" +
                "VPIN is calculated through the accumulation of trade volumes into fixed-size buckets. Each bucket captures a snapshot of trading activity, enabling ongoing analysis of market dynamics:<br/>" +
                "1. <b>Trade Classification:</b> Trades are categorized as buys or sells based on their relation to the market mid-price at execution.<br/>" +
                "2. <b>Volume Accumulation:</b> Buy and sell volumes are accumulated separately until reaching a pre-set bucket size.<br/>" +
                "3. <b>VPIN Calculation:</b> VPIN is the absolute difference between buy and sell volumes in a bucket, normalized to total volume, ranging from 0 (balanced trading) to 1 (high imbalance).<br/><br/>" +
                "To enhance real-time relevance, VPIN values are updated with 'Interim Updates' during the filling of each bucket, providing a more current view of market conditions. These updates offer a dynamic and timely insight into market liquidity and informed trading activity. VPIN serves as an early warning indicator of market turbulence, particularly valuable in high-frequency trading environments.";

        public decimal BucketVolumeSize => _bucketVolumeSize;

        public VPINStudy()
        {
        }
        ~VPINStudy()
        {
            Dispose(false);
        }

        public override async Task StartAsync()
        {
            await base.StartAsync();//call the base first

            HelperOrderBook.Instance.Subscribe(LIMITORDERBOOK_OnDataReceived);
            HelperTrade.Instance.Subscribe(TRADES_OnDataReceived);
            DoCalculation(true); //initial value

            log.Info($"{this.Name} Plugin has successfully started.");
            Status = ePluginStatus.STARTED;
        }

        public override async Task StopAsync()
        {
            Status = ePluginStatus.STOPPING;
            log.Info($"{this.Name} is stopping.");

            HelperOrderBook.Instance.Unsubscribe(LIMITORDERBOOK_OnDataReceived);
            HelperTrade.Instance.Unsubscribe(TRADES_OnDataReceived);

            await base.StopAsync();
        }


        private void TRADES_OnDataReceived(Trade e)
        {
            /*
             * ***************************************************************************************************
             * TRANSFORM the incoming object (decouple it)
             * DO NOT hold this call back, since other components depends on the speed of this specific call back.
             * DO NOT BLOCK
             * IDEALLY, USE QUEUES TO DECOUPLE
             * ***************************************************************************************************
             */

            if (e == null)
                return;
            if (_settings.Provider.ProviderID != e.ProviderId || _settings.Symbol != e.Symbol)
                return;
            if (!e.IsBuy.HasValue) //we do not know what it is
                return;
            lock (_locker)
            {
                // Set the start time for the current bucket if it's a new bucket
                if (_currentBuyVolume == 0 && _currentSellVolume == 0)
                {
                    _currentBucketStartTime = e.Timestamp;
                }

                if (e.IsBuy.Value)
                    _currentBuyVolume += e.Size;
                else
                    _currentSellVolume += e.Size;
                // Check if the bucket is filled
                if (_currentBuyVolume + _currentSellVolume >= _bucketVolumeSize)
                {
                    _currentBucketStartTime = e.Timestamp;
                    _currentBucketEndTime = e.Timestamp; // Set the end time for the current bucket
                    DoCalculation(true);
                }
                else
                {
                    _currentBucketEndTime = e.Timestamp; // Set the end time for the current bucket
                    DoCalculation(false);
                }
            }
        }
        private void LIMITORDERBOOK_OnDataReceived(OrderBook e)
        {
            /*
             * ***************************************************************************************************
             * TRANSFORM the incoming object (decouple it)
             * DO NOT hold this call back, since other components depends on the speed of this specific call back.
             * DO NOT BLOCK
               * IDEALLY, USE QUEUES TO DECOUPLE
             * ***************************************************************************************************
             */

            if (e == null)
                return;
            if (_settings.Provider.ProviderID != e.ProviderID || _settings.Symbol != e.Symbol)
                return;

            lock (_locker)
            {
                var incomingTobBid = e.GetTOB(true);
                var incomingTobAsk = e.GetTOB(false);
                if (incomingTobBid == null || incomingTobAsk == null
                    || (incomingTobBid.Equals(_tobBid) && incomingTobAsk.Equals(_tobAsk)) //if the top of the book has not changed, no need to continue
                    )
                    return;

                _tobBid.CopyFrom(incomingTobBid);
                _tobAsk.CopyFrom(incomingTobAsk);

                _lastMarketMidPrice = (decimal)e.MidPrice;
                if (_lastMarketMidPrice != 0)
                    DoCalculation(false);
            }
        }
        private void DoCalculation(bool isNewBucket)
        {
            if (Status != VisualHFT.PluginManager.ePluginStatus.STARTED) return;
            string valueColor = "White";
            if (_bucketVolumeSize == 0)
                _bucketVolumeSize = (decimal)_settings.BucketVolSize;

            decimal vpin = 0;
            if ((_currentBuyVolume + _currentSellVolume) > 0)
                vpin = (decimal)Math.Abs(_currentBuyVolume - _currentSellVolume) / (_currentBuyVolume + _currentSellVolume);

            if (isNewBucket)
            {
                valueColor = "Green";
                // Check against threshold and trigger alert
                if (vpin > VPIN_THRESHOLD)
                    OnAlertTriggered?.Invoke(this, vpin);
                ResetBucket();
            }
            // Add to rolling window and remove oldest if size exceeded
            var newItem = new BaseStudyModel();
            newItem.Value = vpin;
            newItem.ValueFormatted = vpin.ToString("N1");
            newItem.Timestamp = HelperTimeProvider.Now;
            newItem.MarketMidPrice = _lastMarketMidPrice;
            newItem.ValueColor = valueColor;
            AddCalculation(newItem);
        }
        private void ResetBucket()
        {
            _currentBuyVolume = 0;
            _currentSellVolume = 0;
        }

        protected override void onDataAggregation(BaseStudyModel existing, BaseStudyModel newItem, int counterAggreated)
        {
            //we want to average the aggregations
            existing.Value = ((existing.Value * (counterAggreated - 1)) + newItem.Value) / counterAggreated;
            existing.ValueFormatted = existing.Value.ToString("N1");
            existing.MarketMidPrice = newItem.MarketMidPrice;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    // Dispose managed resources here
                    HelperOrderBook.Instance.Unsubscribe(LIMITORDERBOOK_OnDataReceived);
                    HelperTrade.Instance.Unsubscribe(TRADES_OnDataReceived);
                    _tobAsk?.Dispose();
                    _tobBid?.Dispose();
                    base.Dispose();
                }

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
                AggregationLevel = AggregationLevel.Ms100
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
                _bucketVolumeSize = (decimal)_settings.BucketVolSize;
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
