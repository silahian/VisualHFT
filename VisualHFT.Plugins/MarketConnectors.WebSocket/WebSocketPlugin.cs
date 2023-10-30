using MarketConnectors.WebSocket.Model;
using MarketConnectors.WebSocket.UserControls;
using MarketConnectors.WebSocket.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualHFT.Commons.PluginManager;
using VisualHFT.DataRetriever;
using VisualHFT.DataRetriever.DataParsers;
using VisualHFT.Model;
using VisualHFT.UserSettings;

namespace MarketConnectors.WebSocket
{
    public class WebSocketPlugin : BasePluginDataRetriever
    {
        public class WebsocketData
        {
            public required string type { get; set; }
            public required string data { get; set; }
        }
        
        private new bool _disposed = false; // to track whether the object has been disposed

        private Timer _heartbeatTimer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private DataEventArgs tradeDataEvent = new DataEventArgs() { DataType = "Trades" }; //reusable object. So we avoid allocations
        private DataEventArgs marketDataEvent = new DataEventArgs() { DataType = "Market" };//reusable object. So we avoid allocations
        private DataEventArgs heartbeatDataEvent = new DataEventArgs() { DataType = "HeartBeats" };//reusable object. So we avoid allocations

        private PlugInSettings? _settings;
        public override string Name { get; set; } = "WebSocket Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Connects to custom websocket.";
        public override string Author { get ; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action? CloseSettingWindow { get; set; }

        private IDataParser _parser;
        JsonSerializerSettings? _parser_settings = null;
        ClientWebSocket? _ws;

        public WebSocketPlugin()
        {
            _parser = new JsonParser();
            _parser_settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new CustomDateConverter() },
                DateParseHandling = DateParseHandling.None,
                DateFormatString = "yyyy.MM.dd-HH.mm.ss.ffffff"
            };
            // Initialize the timer
            _heartbeatTimer = new Timer(CheckConnectionStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // Check every 5 seconds
        }
        ~WebSocketPlugin() 
        {
            Dispose(false);
        }
        public override async Task StartAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    CancellationTokenSource source = new CancellationTokenSource();                    
                    using (_ws = new ClientWebSocket())
                    {
                        await _ws.ConnectAsync(new UriBuilder("ws", _settings.HostName, _settings.Port).Uri, CancellationToken.None);
                        await base.StartAsync();
                        byte[] buffer = new byte[1024 * 1024];
                        while (_ws.State == WebSocketState.Open)
                        {
                            var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                            }
                            else
                            {
                                HandleMessage(buffer, result.Count);
                            }
                        }
                    }
                });                
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error($"{this.Name} WebSocket has been closed. " + ex.Message);
                if (failedAttempts == 0)
                    RaiseOnError(new VisualHFT.PluginManager.ErrorEventArgs() { IsCritical = true, PluginName = Name, Exception = ex });
                await HandleConnectionLost();
                throw;
            }
        }
        public override async Task StopAsync()
        {
            //reset models
            RaiseOnDataReceived(new DataEventArgs() { DataType = "Market", ParsedModel = new List<VisualHFT.Model.OrderBook>(), RawData = "" });
            RaiseOnDataReceived(new DataEventArgs() { DataType = "HeartBeats", ParsedModel = new List<VisualHFT.Model.Provider>() { ToHeartBeatModel(false) }, RawData = "" });

            if(_ws != null && _ws.State == WebSocketState.Open)
                await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

            await base.StopAsync();
        }
        private void HandleMessage(byte[] buffer, int count)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, count);
            var dataReceived = _parser.Parse<WebsocketData>(message);
            string dataType = dataReceived.type;

            // Determine the type of data received (e.g., market data, providers, active orders, etc.)
            if (dataType == "Market")
            {
                marketDataEvent.ParsedModel = _parser.Parse<IEnumerable<OrderBook>>(dataReceived.data, _parser_settings);
                RaiseOnDataReceived(marketDataEvent);
            }
            else if (dataType == "ActiveOrders")
            {
                //ParsedModel = _parser.Parse<List<VisualHFT.Model.Order>>(dataReceived.data, _parser_settings);
            }
            else if (dataType == "Strategies")
            {
                //ParsedModel = _parser.Parse<List<StrategyVM>>(dataReceived.data, _parser_settings);
            }
            else if (dataType == "Exposures")
            {
                // ParsedModel = _parser.Parse<List<Exposure>>(dataReceived.data, _parser_settings);
            }
            else if (dataType == "HeartBeats")
            {
                heartbeatDataEvent.ParsedModel = _parser.Parse<List<VisualHFT.Model.Provider>>(dataReceived.data, _parser_settings);
                RaiseOnDataReceived(heartbeatDataEvent);
            }
            else if (dataType == "Trades")
            {
                tradeDataEvent.ParsedModel = _parser.Parse<List<Trade>>(dataReceived.data, _parser_settings);
                RaiseOnDataReceived(tradeDataEvent);
            }
            else
            {
                if (log.IsWarnEnabled)
                    log.Warn("Websocket data retriever :" + dataType + " error: NOT RECOGNIZED.");
            }
            
        }
        private void CheckConnectionStatus(object state)
        {
            bool isConnected = _ws != null && _ws.State == WebSocketState.Open;
            heartbeatDataEvent.ParsedModel = new List<VisualHFT.Model.Provider>() { ToHeartBeatModel(isConnected) };
            RaiseOnDataReceived(heartbeatDataEvent);
        }
        private VisualHFT.Model.Provider ToHeartBeatModel(bool isConnected = true)
        {
            return new VisualHFT.Model.Provider()
            {
                ProviderCode = _settings.ProviderId,
                ProviderID = _settings.ProviderId,
                ProviderName = _settings.ProviderName,
                Status = isConnected ? eSESSIONSTATUS.BOTH_CONNECTED : eSESSIONSTATUS.BOTH_DISCONNECTED,
                Plugin = this
            };
        }
        public override object GetUISettings()
        {
            PluginSettingsView view = new PluginSettingsView();
            PluginSettingsViewModel viewModel = new PluginSettingsViewModel(CloseSettingWindow);
            viewModel.HostName = _settings.HostName;
            viewModel.Port = _settings.Port;
            viewModel.ProviderId = _settings.ProviderId;
            viewModel.ProviderName = _settings.ProviderName;
            viewModel.UpdateSettingsFromUI = () =>
            {
                _settings.HostName = viewModel.HostName;
                _settings.Port = viewModel.Port;
                _settings.ProviderId = viewModel.ProviderId;
                _settings.ProviderName = viewModel.ProviderName;
                SaveSettings();

                // Start the HandleConnectionLost task without awaiting it
                //run this because it will allow to reconnect with the new values
                Task.Run(HandleConnectionLost);

            };
            // Display the view, perhaps in a dialog or a new window.
            view.DataContext = viewModel;
            return view;
        }
        protected override void InitializeDefaultSettings()
        {
            _settings = new PlugInSettings()
            {
                HostName = "localhost",
                Port = 6900,
                ProviderId = 3, //must be unique
                ProviderName = "WebSocket"
            };
            SaveToUserSettings(_settings);
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
                _settings.Provider = new VisualHFT.Model.Provider() { ProviderID = 3, ProviderName = "WebSocket" };
            }
        }
        protected override void SaveSettings()
        {
            SaveToUserSettings(_settings);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposed)
            {
                if (disposing)
                {        
                    _ws?.Dispose();
                    _heartbeatTimer?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}