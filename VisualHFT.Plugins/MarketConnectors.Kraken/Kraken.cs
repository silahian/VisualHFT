using MarketConnectors.Kraken.Model;
using MarketConnectors.Kraken.UserControls;
using MarketConnectors.Kraken.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VisualHFT.Commons.PluginManager;
using VisualHFT.DataRetriever;
using VisualHFT.UserSettings;

namespace MarketConnectors.Kraken
{
    public class Kraken : BasePluginDataRetriever
    {
        private PlugInSettings? _settings;
        private Action? _closeSettingWindow;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Kraken));

        private DataEventArgs heartbeatDataEvent = new DataEventArgs() { DataType = "HeartBeats" };//reusable object. So we avoid allocations

        public override string Name { get; set; } = "Kraken Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Connects to TBD.";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings
        {
            get
            {
                ISetting? settings = _settings;

                if (settings == null)
                {
                    throw new InvalidOperationException();
                }

                return settings;
            }
            set => _settings = (PlugInSettings)value;
        }

        public override Action CloseSettingWindow
        {
            get
            {
                Action? closeSettingWindow = _closeSettingWindow;

                if (closeSettingWindow == null)
                {
                    throw new InvalidOperationException();
                }

                return closeSettingWindow;
            }
            set => _closeSettingWindow = value;
        }

        public Kraken()
        {
            log.Debug("Create instance");
        }

        ~Kraken()
        {
            Dispose(false);
        }

        public override async Task StartAsync()
        {
            await base.StartAsync();

            heartbeatDataEvent.ParsedModel = new List<VisualHFT.Model.Provider>() { ToHeartBeatModel(true) };
            RaiseOnDataReceived(heartbeatDataEvent);
        }

        public override async Task StopAsync()
        {
            heartbeatDataEvent.ParsedModel = new List<VisualHFT.Model.Provider>() { ToHeartBeatModel(false) };
            //reset models
            RaiseOnDataReceived(heartbeatDataEvent);

            await base.StopAsync();
        }

        protected override void LoadSettings()
        {
            _settings = LoadFromUserSettings<PlugInSettings>();
            if (_settings == null)
            {
                InitializeDefaultSettings();
            }
            if (_settings!.Provider == null) //To prevent back compability with older setting formats
            {
                _settings.Provider = CreateProvider();
            }
            ParseSymbols(string.Join(',', _settings.Symbols.ToArray())); //Utilize normalization function
        }

        protected override void SaveSettings()
        {
            SaveToUserSettings(Settings);
        }

        protected override void InitializeDefaultSettings()
        {
            _settings = new PlugInSettings()
            {
                ApiKey = "",
                ApiSecret = "",
                DepthLevels = 10,
                UpdateIntervalMs = 100,
                Provider = CreateProvider(),
                Symbols = new List<string>() { "???(BTC/USD)", "???(ETH/USD)" } // Add more symbols as needed
            };
            SaveToUserSettings(_settings);
        }

        public override object GetUISettings()
        {
            PluginSettingsView view = new PluginSettingsView();
            PluginSettingsViewModel viewModel = new PluginSettingsViewModel(CloseSettingWindow);
            PlugInSettings settings = (PlugInSettings)Settings;
            viewModel.ApiSecret = settings.ApiSecret;
            viewModel.ApiKey = settings.ApiKey;
            viewModel.UpdateIntervalMs = settings.UpdateIntervalMs;
            viewModel.DepthLevels = settings.DepthLevels;
            viewModel.ProviderId = settings.Provider.ProviderID;
            viewModel.ProviderName = settings.Provider.ProviderName;
            viewModel.Symbols = settings.Symbols;
            viewModel.UpdateSettingsFromUI = () =>
            {
                settings.ApiSecret = viewModel.ApiSecret;
                settings.ApiKey = viewModel.ApiKey;
                settings.UpdateIntervalMs = viewModel.UpdateIntervalMs;
                settings.DepthLevels = viewModel.DepthLevels;
                settings.Provider = new VisualHFT.Model.Provider() { ProviderID = viewModel.ProviderId, ProviderName = viewModel.ProviderName };
                settings.Symbols = viewModel.Symbols;
                SaveSettings();
                ParseSymbols(string.Join(',', settings.Symbols.ToArray()));

                // Start the HandleConnectionLost task without awaiting it
                //run this because it will allow to reconnect with the new values
                Task.Run(HandleConnectionLost);
            };
            // Display the view, perhaps in a dialog or a new window.
            view.DataContext = viewModel;
            return view;
        }

        private VisualHFT.Model.Provider ToHeartBeatModel(bool isConnected)
        {
            PlugInSettings settings = (PlugInSettings)Settings;
            return new VisualHFT.Model.Provider()
            {
                ProviderCode = settings.Provider.ProviderID,
                ProviderID = settings.Provider.ProviderID,
                ProviderName = settings.Provider.ProviderName,
                Status = isConnected ? eSESSIONSTATUS.BOTH_CONNECTED : eSESSIONSTATUS.BOTH_DISCONNECTED,
                Plugin = this
            };
        }

        private static VisualHFT.Model.Provider CreateProvider()
        {
            return new VisualHFT.Model.Provider() { ProviderID = 5, ProviderName = "Kraken" };
        }
    }
}
