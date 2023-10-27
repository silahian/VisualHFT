using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VisualHFT.Commons.PluginManager;
using VisualHFT.Commons.Pools;
using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.PluginManager;
using VisualHFT.Studies.LOBImbalance.Model;
using VisualHFT.Studies.LOBImbalance.UserControls;
using VisualHFT.Studies.LOBImbalance.ViewModel;
using VisualHFT.UserSettings;

namespace VisualHFT.Studies
{
    public class LOBImbalanceStudy : BasePluginStudy
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private PlugInSettings _settings;

        private OrderBook _orderBook; //to hold last market data tick

        // Event declaration
        public override event EventHandler<decimal> OnAlertTriggered;
        public override event EventHandler<BaseStudyModel> OnCalculated;
        public override event EventHandler<ErrorEventArgs> OnError;

        public override string Name { get; set; } = "LOB Imbalance Study Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Calculates Limit Order Book Imbalances.";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action CloseSettingWindow { get; set; }
        public override string TileTitle { get; set; } = "LOB Imbalance";
        public override string TileToolTip { get; set; } = "The <b>Limit Order Book Imbalance</b> represents the disparity between buy and sell orders at a specific price level.<br/><br/>" +
                "It highlights the difference in demand and supply in the order book, providing insights into potential price movements.<br/>" +
                "A significant imbalance can indicate a strong buying or selling interest at that price.";

        public LOBImbalanceStudy()
        {
            HelperOrderBook.Instance.Subscribe(LIMITORDERBOOK_OnDataReceived);
        }
        ~LOBImbalanceStudy()
        {
            Dispose(false);
        }


        private void LIMITORDERBOOK_OnDataReceived(OrderBook e)
        {
            if (e == null)
                return;
            if (_settings.Provider.ProviderID != e.ProviderID || _settings.Symbol != e.Symbol)
                return;
            _orderBook = e;
            if (_orderBook.MidPrice != 0)
                CalculateStudy();
        }
        private void CalculateStudy()
        {
            if (Status != VisualHFT.PluginManager.ePluginStatus.STARTED) return;

            var newItem = new BaseStudyModel();
            newItem.Value = (decimal)_orderBook.ImbalanceValue;
            newItem.ValueFormatted = _orderBook.ImbalanceValue.ToString("N1");
            newItem.Timestamp = DateTime.Now;
            newItem.MarketMidPrice = (decimal)_orderBook.MidPrice;
            OnCalculated?.Invoke(this, newItem);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                    HelperOrderBook.Instance.Unsubscribe(LIMITORDERBOOK_OnDataReceived);
                    _orderBook?.Dispose();
                    _orderBook = null;
                }
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
                Symbol = "",
                Provider = new Provider(),
                AggregationLevel = AggregationLevel.Automatic
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

            };
            // Display the view, perhaps in a dialog or a new window.
            view.DataContext = viewModel;
            return view;
        }

    }
}
