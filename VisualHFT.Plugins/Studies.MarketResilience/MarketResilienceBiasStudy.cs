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

        public override string Name { get; set; } = "Market Resilience Bias";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Market Resilience Bias.";
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
            _MARKETRESILIENCE.OnCalculated += _MARKETRESILIENCE_OnCalculated;
            await _MARKETRESILIENCE.StartAsync();

            log.Info($"{this.Name} Plugin has successfully started.");
            Status = ePluginStatus.STARTED;
        }

        public override async Task StopAsync()
        {
            Status = ePluginStatus.STOPPING;
            log.Info($"{this.Name} is stopping.");

            _MARKETRESILIENCE.OnCalculated -= _MARKETRESILIENCE_OnCalculated;

            await base.StopAsync();
        }

        private void _MARKETRESILIENCE_OnCalculated(object? sender, BaseStudyModel model)
        {
            if (model == null)
                return;
            //No need to check the provider/symbol, since this event is subscribed with local settings


            eORDERSIDE _valueBias = eORDERSIDE.None; //unkonw
            string _valueFormatted = "-"; //unknown
            string _valueColor = "White";
            if (model.Tag == "Buy")
                _valueBias = eORDERSIDE.Buy;
            else if (model.Tag == "Sell")
                _valueBias = eORDERSIDE.Sell;

            if (_valueBias == eORDERSIDE.None)
                return;

            _valueFormatted = _valueBias == eORDERSIDE.Buy ? "↑" : "↓";
            _valueColor = _valueBias == eORDERSIDE.Buy ? "Green" : "Red";



            var newItem = new BaseStudyModel()
            {
                Value = _valueBias == eORDERSIDE.Buy? 1: -1,
                ValueFormatted = _valueFormatted,
                ValueColor = _valueColor,
                Timestamp = HelperTimeProvider.Now,
                MarketMidPrice = model.MarketMidPrice
            };

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



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if (disposing)
                {
                    _MARKETRESILIENCE.OnCalculated -= _MARKETRESILIENCE_OnCalculated;
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

                //run this because it will allow to restart with the new values
                Task.Run(async () => await HandleRestart($"{this.Name} is starting (from reloading settings).", null, true));


            };
            // Display the view, perhaps in a dialog or a new window.
            view.DataContext = viewModel;
            return view;
        }

    }
}
