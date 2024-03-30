using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Kraken.Net;
using Kraken.Net.Clients;
using MarketConnectors.Kraken.Model;
using MarketConnectors.Kraken.UserControls;
using MarketConnectors.Kraken.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VisualHFT.Commons.PluginManager;
using VisualHFT.DataRetriever;
using VisualHFT.UserSettings;

namespace MarketConnectors.Kraken
{
    public class KrakenPlugin : BasePluginDataRetriever
    {
        private PlugInSettings? _settings;
        KrakenSocketClient _socketClient;
        KrakenRestClient _restClient;
        private Action? _closeSettingWindow;

        /// <summary>
        /// Contains pair EXCHANGE_REST_SYMBOL: EXCHANGE_WEBSOCKET_SYMBOL
        /// - TBTCUSD: TBTC/USD
        /// - ETHUSDT: ETH/USD
        /// </summary>
        private Dictionary<string, string> _socketSymbols;

        private Timer _heartbeatTimer;
        private CallResult<UpdateSubscription>? _tradesSubscription;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(KrakenPlugin));

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


        public KrakenPlugin()
        {
            _socketSymbols = new Dictionary<string, string>();
            _restClient = new KrakenRestClient(options =>
            {
                var credentials = GetApiCredentials();
                if (credentials != null) { options.ApiCredentials = credentials; }
                options.Environment = KrakenEnvironment.Live;
            });
            _socketClient = new KrakenSocketClient(options =>
            {
                var credentials = GetApiCredentials();
                if (credentials != null) { options.ApiCredentials = credentials; }
                options.Environment = KrakenEnvironment.Live;
                options.AutoReconnect = true;
            });


            _heartbeatTimer = new Timer(CheckConnectionStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // Check every 5 seconds

            log.Debug("Create instance");
        }

        ~KrakenPlugin()
        {
            Dispose(false);
        }

        public override object GetUISettings()
        {
            PluginSettingsView view = new PluginSettingsView();
            PluginSettingsViewModel viewModel = new PluginSettingsViewModel(CloseSettingWindow);
            PlugInSettings settings = (PlugInSettings)Settings;
            viewModel.ApiSecret = settings.PrivateKey;
            viewModel.ApiKey = settings.ApiKey;
            viewModel.UpdateIntervalMs = settings.UpdateIntervalMs;
            viewModel.DepthLevels = settings.DepthLevels;
            viewModel.ProviderId = settings.Provider.ProviderID;
            viewModel.ProviderName = settings.Provider.ProviderName;
            viewModel.Symbols = settings.Symbols;
            viewModel.UpdateSettingsFromUI = () =>
            {
                settings.PrivateKey = viewModel.ApiSecret;
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


        public override async Task StartAsync()
        {
            await InitializeSocketSymbols();
            await InitializeTradesAsync();

            await base.StartAsync();
        }

        public override async Task StopAsync()
        {
            {
                // local scope
                var tradesSubscription = _tradesSubscription;
                if (tradesSubscription != null) { await tradesSubscription.Data.CloseAsync(); }
            }
            _tradesSubscription = null;

            await _socketClient.UnsubscribeAllAsync();

            //reset models
            heartbeatDataEvent.ParsedModel = new List<VisualHFT.Model.Provider>();
            RaiseOnDataReceived(heartbeatDataEvent);

            await base.StopAsync();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposed)
            {
                if (disposing)
                {
                    _heartbeatTimer.Dispose();
                    _restClient.Dispose();
                    _socketClient.Dispose();
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
                PrivateKey = "",
                DepthLevels = 10,
                UpdateIntervalMs = 100,
                Provider = CreateProvider(),
                Symbols = new List<string>() { "TBTCUSD(BTC/USD)", "ETHUSDT(ETH/USD)" } // Add more symbols as needed
            };
            SaveToUserSettings(_settings);
        }

        private void CheckConnectionStatus(object? state)
        {
            bool isConnected = _socketClient.CurrentConnections > 0;
            heartbeatDataEvent.ParsedModel = new List<VisualHFT.Model.Provider>() { ToHeartBeatModel(isConnected) };
            RaiseOnDataReceived(heartbeatDataEvent);
        }


        private ApiCredentials? GetApiCredentials()
        {
            var settings = _settings;
            if (settings != null)
            {
                var krakenApiKey = settings.ApiKey;
                var krakenPrivateKey = settings.PrivateKey;
                if (krakenApiKey != "" && krakenPrivateKey != "")
                {
                    return new ApiCredentials(krakenApiKey, krakenPrivateKey);
                }
            }
            return null;

        }

        private async Task InitializeSocketSymbols()
        {
            var symbols = GetAllNonNormalizedSymbols();

            var result = await _restClient.SpotApi.ExchangeData.GetSymbolsAsync(symbols);

            _socketSymbols.Clear();
            foreach (var item in result.Data)
            {
                _socketSymbols[item.Key] = item.Value.WebsocketName;
            }
        }

        private async Task InitializeTradesAsync()
        {
            var symbols = GetAllNonNormalizedSymbols();
            var websocketSymbols = symbols.Select(s => _socketSymbols[s]);
            try
            {
                /**
                 * ??? is not a valid Kraken websocket symbol.
                 * Should be [BaseAsset]/[QuoteAsset] in ISO 4217-A3 standardized names,
                 * e.g. ETH/XBTWebsocket names for pairs are returned in the GetSymbols
                 * method in the WebsocketName property.
                 */


                var tradesSubscription = await _socketClient.SpotApi.SubscribeToTradeUpdatesAsync(
                   websocketSymbols,
                   trade =>
                   {
                       // Buffer the trades
                       if (trade.Data != null)
                       {
                           try
                           {
                               //_tradesBuffers[GetNormalizedSymbol(trade.Data.Symbol)].Add(trade.Data);
                           }
                           catch (Exception ex)
                           {
                               RaiseOnError(new VisualHFT.PluginManager.ErrorEventArgs() { IsCritical = false, PluginName = Name, Exception = ex });
                               // Start the HandleConnectionLost task without awaiting it
                               Task.Run(HandleConnectionLost);
                           }
                       }
                   }
                );
                if (tradesSubscription.Success)
                {
                    _tradesSubscription = tradesSubscription;
                    //AttachEventHandlers(tradesSubscription.Data);
                    //InitializeBufferProcessingTasks();
                }
                else
                {
                    throw new Exception($"{this.Name} trades subscription error: {tradesSubscription.Error}");
                }
            }
            catch (Exception e)
            {
                //
                throw;
            }
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
            return new VisualHFT.Model.Provider() { ProviderID = 6, ProviderName = "Kraken" };
        }
    }
}
