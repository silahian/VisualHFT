using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using VisualHFT.Commons.PluginManager;
using VisualHFT.Commons.Pools;
using VisualHFT.Enums;
using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.PluginManager;
using VisualHFT.Studies.MarketRatios.Model;
using VisualHFT.Studies.MarketRatios.UserControls;
using VisualHFT.Studies.MarketRatios.ViewModel;
using VisualHFT.UserSettings;

namespace VisualHFT.Studies
{
    /// <summary>
    /// The Order to Trade Ratio (OTR) is the inverse of TTO. 
    /// It is calculated by dividing the number of orders placed by the number of trades executed. 
    /// This ratio is often used by regulators to identify potentially manipulative or disruptive trading behavior. 
    /// 
    /// A high OTR may indicate that a trader is placing a large number of orders but executing very few, which could be a sign of market manipulation tactics like layering or spoofing.
    /// 
    /// </summary>
    public class OrderToTradeRatioStudy : BasePluginStudy
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private PlugInSettings _settings;

        private decimal total_L2OrderSize_Ini = 0;
        private decimal total_L2OrderSize_End = 0;
        private decimal totalExecutedTradeSize = 0;
        private decimal _lastMarketMidPrice = 0; //keep track of market price


        // Event declaration
        public override event EventHandler<decimal> OnAlertTriggered;

        public override string Name { get; set; } = "Order To Trade Ratio Study Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Volume - Order To Trade Ratio.";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action CloseSettingWindow { get; set; }
        public override string TileTitle { get; set; } = "OTT";
        public override string TileToolTip { get; set; } = "The <b>OTT</b> (Volume - Order To Trade Ratio) is a key metric used to evaluate trading behavior. <br/> It measures the number of orders placed relative to the number of trades executed. This ratio is often <b>monitored by regulatory bodies</b> to identify potentially manipulative or disruptive trading activities.<br/><br/>" +
                "<b>OTT</b> is calculation as follows: <i>OTT Ratio  = Number of Orders Placed / Number of Executed Trades</i><br/>";

        public OrderToTradeRatioStudy()
        {
        }
        ~OrderToTradeRatioStudy()
        {
            Dispose(false);
        }

        public override async Task StartAsync()
        {
            await base.StartAsync();//call the base first

            HelperOrderBook.Instance.Subscribe(LIMITORDERBOOK_OnDataReceived);
            HelperTrade.Instance.Subscribe(TRADES_OnDataReceived);

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

        private void LIMITORDERBOOK_OnDataReceived(OrderBook e)
        {
            if (e == null)
                return;
            if (_settings.Provider.ProviderID != e.ProviderID || _settings.Symbol != e.Symbol)
                return;

            _lastMarketMidPrice = (decimal)e.MidPrice;
            var currentOrderSize = e.Asks.Where(x => x.Size.HasValue).Sum(a => (decimal)a.Size) + e.Bids.Where(x => x.Size.HasValue).Sum(b => (decimal)b.Size);  // Sum of all order sizes
            if (total_L2OrderSize_Ini == 0)
                total_L2OrderSize_Ini = currentOrderSize;
            total_L2OrderSize_End = currentOrderSize;

            DoCalculation();
        }
        private void TRADES_OnDataReceived(Trade e)
        {
            if (_settings.Provider.ProviderID != e.ProviderId || _settings.Symbol != e.Symbol)
                return;
            if (!e.IsBuy.HasValue) //we do not know what it is
                return;

            if (e.IsBuy.Value)
                totalExecutedTradeSize += e.Size;  // Add the size of the executed trade
            else
                totalExecutedTradeSize -= e.Size;  // Subtract the size of the executed trade

            DoCalculation();
        }

        private void ResetCalculations()
        {
            total_L2OrderSize_Ini = 0;
            total_L2OrderSize_End = 0;
            totalExecutedTradeSize = 0;
        }
        protected void DoCalculation()
        {
            if (Status != VisualHFT.PluginManager.ePluginStatus.STARTED) return;

            var newItem = new BaseStudyModel();
            decimal t2oRatio = 0;
            decimal delta = total_L2OrderSize_End - total_L2OrderSize_Ini;

            if (totalExecutedTradeSize == 0)
                t2oRatio = 0; // Avoid division by zero
            else
                t2oRatio = delta / totalExecutedTradeSize;


            // Trigger any events or updates based on the new T2O ratio
            newItem.Value = t2oRatio;
            newItem.ValueFormatted = t2oRatio.ToString("N1");
            newItem.MarketMidPrice = _lastMarketMidPrice;
            newItem.Timestamp = HelperTimeProvider.Now;

            AddCalculation(newItem);
        }

        protected override void onDataAggregation(BaseStudyModel existing, BaseStudyModel newItem, int counterAggreated)
        {
            //Aggregation: last
            existing.Value = newItem.Value;
            existing.ValueFormatted = newItem.ValueFormatted;
            existing.MarketMidPrice = newItem.MarketMidPrice;

            base.onDataAggregation(existing, newItem, counterAggreated);
        }

        protected override void onDataAdded()
        {
            ResetCalculations();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    // Dispose managed resources here
                    HelperOrderBook.Instance.Unsubscribe(LIMITORDERBOOK_OnDataReceived);
                    HelperTrade.Instance.Unsubscribe(TRADES_OnDataReceived);
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
                Symbol = "",
                Provider = new Provider(),
                AggregationLevel = AggregationLevel.Ms100
            };
            SaveToUserSettings(_settings);
        }
        public override object GetUISettings()
        {
            PluginSettingsView view = new PluginSettingsView();
            PluginSettingsViewModel viewModel = new PluginSettingsViewModel(CloseSettingWindow);
            viewModel.SelectedSymbol = _settings.Symbol;
            viewModel.SelectedProviderID = _settings.Provider.ProviderID;
            viewModel.AggregationLevelSelection = _settings.AggregationLevel;

            viewModel.UpdateSettingsFromUI = () =>
            {
                _settings.Symbol = viewModel.SelectedSymbol;
                _settings.Provider = viewModel.SelectedProvider;
                _settings.AggregationLevel = viewModel.AggregationLevelSelection;

                SaveSettings();
                //run this because it will allow to restart with the new values
                Task.Run(async () => await HandleRestart($"{this.Name} is starting (from reloading settings).", null, true));

            };
            // Display the view, perhaps in a dialog or a new window.
            view.DataContext = viewModel;
            return view;
        }

    }
}
