using System;
using System.Threading.Tasks;
using VisualHFT.Commons.PluginManager;
using VisualHFT.Enums;
using VisualHFT.Model;
using VisualHFT.PluginManager;
using VisualHFT.Studies.MarketResilience.Model;
using VisualHFT.Studies.MarketResilience.UserControls;
using VisualHFT.Studies.MarketResilience.ViewModel;
using VisualHFT.UserSettings;

namespace VisualHFT.Studies
{
    public class MarketResilienceBiasStudy : BasePluginStudy
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private PlugInSettings _settings;
        private MarketResilienceStudy _MARKETRESILIENCE;


        // Event declaration
        public override event EventHandler<decimal> OnAlertTriggered;

        public override string Name { get; set; } = "Market Resiliecence Bias Study Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Market Resiliecence Bias Study Plugin.";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action CloseSettingWindow { get; set; }
        public override string TileTitle { get; set; } = "MRB";
        public override string TileToolTip { get; set; } = "<b>Market Resilience Bias</b> (MRB) is a real-time metric that quantifies the directional tendency of the market following a large trade. <br/> It provides insights into the prevailing sentiment among market participants, enhancing traders' understanding of market dynamics.<br/><br/>" +
                "The <b>MRB</b> score is derived from the behavior of the Limit Order Book (LOB) post-trade:<br/>" +
                "1. <b>Volume Addition Rate:</b> Analyzes the rate at which volume is added to the bid and ask sides of the LOB after a trade.<br/>" +
                "2. <b>Directional Inclination:</b> Determines whether the market is leaning towards a bullish or bearish stance based on the volume addition rate.<br/>" +
                "<br/>" +
                "The <b>MRB</b> score indicates the market's bias, with a value of 1 representing a bullish sentiment (sentiment up) and -1 representing a bearish sentiment (sentiment down). A zero (0) value represent unknown bias.";

        public MarketResilienceBiasStudy()
        {
            _MARKETRESILIENCE = new MarketResilienceStudy();
        }

        ~MarketResilienceBiasStudy()
        {
            Dispose(false);
        }


        public override async Task StartAsync()
        {
            await base.StartAsync();//call the base first

            _MARKETRESILIENCE.Settings = _settings;
            _MARKETRESILIENCE.OnTradeRecovered += _MARKETRESILIENCE_OnTradeRecovered;
            await _MARKETRESILIENCE.StartAsync();

            log.Info($"{this.Name} Plugin has successfully started.");
            Status = ePluginStatus.STARTED;
        }

        public override async Task StopAsync()
        {
            Status = ePluginStatus.STOPPING;
            log.Info($"{this.Name} is stopping.");

            _MARKETRESILIENCE.OnTradeRecovered -= _MARKETRESILIENCE_OnTradeRecovered;

            await base.StopAsync();
        }

        private void _MARKETRESILIENCE_OnTradeRecovered(object? sender, (BaseStudyModel model, eLOBSIDE recoverySide, int providerID, string symbol) e)
        {
            //in order to have a strong bias, the "Market Resilience" study must be near to zero.
            // So, to fiand the bias direction, we must have MR < 0.3 and check on wich side the recovery didn't happen.

            if (e.model == null)
                return;
            if (_settings.Provider.ProviderID != e.providerID || _settings.Symbol != e.symbol)
                return;

            int _valueBias = 0; //unkonw
            string _valueFormatted = "-"; //unknown
            string _valueColor = "White";

            if (e.model.Value <= 0.3m)
            {
                _valueBias = e.recoverySide == eLOBSIDE.ASK ? 1 : -1;
                _valueFormatted = _valueBias == 1 ? "↑" : "↓";
                _valueColor = _valueBias == 1 ? "Green" : "Red";
            }

            var newItem = new BaseStudyModel()
            {
                Value = _valueBias,
                ValueFormatted = _valueFormatted,
                ValueColor = _valueColor,
                Timestamp = HelperTimeProvider.Now,
                MarketMidPrice = e.model.MarketMidPrice
            };

            AddCalculation(newItem);
        }
        protected override void onDataAggregation(BaseStudyModel existing, BaseStudyModel newItem, int counterAggreated)
        {
            //Aggregation: last
            existing.Value = newItem.Value;
            existing.ValueFormatted = newItem.ValueFormatted;
            existing.MarketMidPrice = newItem.MarketMidPrice;
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if (disposing)
                {
                    _MARKETRESILIENCE.Dispose();
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
                AggregationLevel = AggregationLevel.Ms500
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

                //reset 
                _MARKETRESILIENCE.Settings = _settings;
                _MARKETRESILIENCE.ResetPreTradeState();

            };
            // Display the view, perhaps in a dialog or a new window.
            view.DataContext = viewModel;
            return view;
        }

    }
}
