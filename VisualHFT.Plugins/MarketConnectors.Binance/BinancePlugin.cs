using Binance.Net;
using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using MarketConnectors.Binance.Model;
using MarketConnectors.Binance.UserControls;
using MarketConnectors.Binance.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VisualHFT.Commons.PluginManager;
using VisualHFT.DataRetriever;
using VisualHFT.PluginManager;
using VisualHFT.UserSettings;

namespace MarketConnectors.Binance
{
    public class BinancePlugin : BasePluginDataRetriever
    {
        private bool _disposed = false; // to track whether the object has been disposed

        private PlugInSettings _settings;
        private BinanceSocketClient _socketClient;
        private BinanceRestClient _restClient;
        private Dictionary<string, VisualHFT.Model.OrderBook> _localOrderBooks = new Dictionary<string, VisualHFT.Model.OrderBook>();
        private Dictionary<string, Queue<IBinanceEventOrderBook>> _eventBuffers = new Dictionary<string, Queue<IBinanceEventOrderBook>>();
        private Dictionary<string, Queue<IBinanceTrade>> _tradesBuffers = new Dictionary<string, Queue<IBinanceTrade>>();
        private CancellationTokenSource _ctDeltas;
        private Dictionary<string, CancellationTokenSource> _ct;
        private Dictionary<string, CancellationTokenSource> _ct_trades;
        private object _lock_eventBuffers = new object();
        private object _lock_tradeBuffers = new object();

        private Dictionary<string, long> _localOrderBooks_LastUpdate = new Dictionary<string, long>();
        private Timer _heartbeatTimer;
        private CallResult<UpdateSubscription> deltaSubscription;
        private CallResult<UpdateSubscription> tradesSubscription;


        public override string Name { get; set; } = "Binance.US Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Connects to Binance websockets.";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action CloseSettingWindow { get; set; }

        public BinancePlugin()
        {
            _ct = new Dictionary<string, CancellationTokenSource>();
            _ct_trades = new Dictionary<string, CancellationTokenSource>();
            _socketClient = new BinanceSocketClient(options =>
            {
                if (_settings.ApiKey != ""  && _settings.ApiSecret != "")
                    options.ApiCredentials = new ApiCredentials(_settings.ApiKey, _settings.ApiSecret);
                options.Environment = BinanceEnvironment.Us;
                options.AutoReconnect = true;
            });

            _restClient = new BinanceRestClient(options =>
            {
                if (_settings.ApiKey != "" && _settings.ApiSecret != "")
                    options.ApiCredentials = new ApiCredentials(_settings.ApiKey, _settings.ApiSecret);
                options.Environment = BinanceEnvironment.Us;
            });
            // Initialize the timer
            _heartbeatTimer = new Timer(CheckConnectionStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // Check every 5 seconds
        }
        ~BinancePlugin()
        {
            Dispose(false);
        }

        public override void Start()
        {
            _eventBuffers.Clear();
            _tradesBuffers.Clear();
            _ct.Clear();
            _ct_trades.Clear();
            
            foreach (var sym in GetAllNonNormalizedSymbols())
            {
                string symbol = GetNormalizedSymbol(sym);

                // Initialize event buffer for each symbol
                lock (_lock_eventBuffers)
                    _eventBuffers.Add(symbol, new Queue<IBinanceEventOrderBook>());
                lock (_lock_tradeBuffers)
                    _tradesBuffers.Add(symbol, new Queue<IBinanceTrade>());

                _ct_trades.Add(symbol, null);
                _ct.Add(symbol, null);
            }

            InitializeTrades();
            InitializeDeltas();
            InitializeSnapshots();
            base.Start();
        }
        public override void Stop()
        {
            _ctDeltas.Cancel();
            foreach (var token in _ct.Values)
                token.Cancel();
            foreach (var token in _ct_trades.Values)
                token.Cancel();

            UnattachEventHandlers(tradesSubscription.Data);
            UnattachEventHandlers(deltaSubscription.Data);

            Task.Run(async () =>
            {
                await deltaSubscription.Data.CloseAsync();
                await tradesSubscription.Data.CloseAsync();
            });

            //reset models
            RaiseOnDataReceived(new DataEventArgs() { DataType = "Market", ParsedModel = new List<VisualHFT.Model.OrderBook>(), RawData = "" });
            RaiseOnDataReceived(new DataEventArgs() { DataType = "HeartBeats", ParsedModel = new List<VisualHFT.Model.Provider>() { ToHeartBeatModel(false) }, RawData = "" });

            base.Stop();
        }
        private void InitializeTrades()
        {
            Task.Run(async () =>
            {
                try
                {
                    tradesSubscription = await _socketClient.SpotApi.ExchangeData.SubscribeToTradeUpdatesAsync(
                        GetAllNonNormalizedSymbols(),
                        trade =>
                        {
                            // Buffer the trades
                            if (trade.Data != null)
                            {
                                lock (_lock_tradeBuffers)
                                    _tradesBuffers[GetNormalizedSymbol(trade.Data.Symbol)].Enqueue(trade.Data);
                            }
                        });
                    if (tradesSubscription.Success)
                    {
                        AttachEventHandlers(tradesSubscription.Data);
                        InitializeBufferProcessingTasks();
                    }
                    else
                    {
                        throw new Exception($"{this.Name} trades subscription error: {tradesSubscription.Error}");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnError(new VisualHFT.PluginManager.ErrorEventArgs() { IsCritical = true, PluginName = Name, Exception = ex });
                }
            });

        }
        private void InitializeBufferProcessingTasks()
        {
            //Initialize processes to consume buffer
            _ct_trades = new Dictionary<string, CancellationTokenSource>();
            foreach (var sym in GetAllNonNormalizedSymbols())
            {
                string symbol = GetNormalizedSymbol(sym);
                
                _ct_trades.Add(symbol, new CancellationTokenSource());
                //launch Task. in a new thread with _ct as cancellation
                Task.Run(async () =>
                {
                    while (!_ct_trades[symbol].IsCancellationRequested)
                    {
                        // Process buffered events
                        ProcessBufferedTrades(symbol);
                        await Task.Delay(1); // Prevents tight looping, adjust as needed
                    }
                });

            }

        }
        private void InitializeDeltas()
        {
            _ctDeltas = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    // *************** PARTIAL OR NOT???????
                    // Subscribe to updates of a symbol
                    //_socketClient.SpotApi.ExchangeData.SubscribeToOrderBookUpdatesAsync
                    //_socketClient.SpotApi.ExchangeData.SubscribeToPartialOrderBookUpdatesAsync (looks like this is snapshots)
                    deltaSubscription = await _socketClient.SpotApi.ExchangeData.SubscribeToOrderBookUpdatesAsync(
                        GetAllNonNormalizedSymbols(),
                        //DEPTH_LEVELS,
                        _settings.UpdateIntervalMs,
                        data =>
                        {
                            // Buffer the events
                            if (data.Data != null)
                            {
                                data.Data.EventTime = data.Timestamp;
                                if (Math.Abs(DateTime.Now.Subtract(data.Data.EventTime.ToLocalTime()).TotalSeconds) > 1)
                                {
                                    Console.WriteLine("Rates coming late?");
                                }
                                var normalizedSymbol = GetNormalizedSymbol(data.Data.Symbol);
                                lock (_lock_eventBuffers)
                                {
                                    if (_eventBuffers.ContainsKey(normalizedSymbol))
                                        _eventBuffers[normalizedSymbol].Enqueue(data.Data);
                                }
                            }
                        }, _ctDeltas.Token);
                    if (deltaSubscription.Success)
                    {
                        AttachEventHandlers(deltaSubscription.Data);
                    }
                    else
                    {
                        throw new Exception($"{this.Name} deltas subscription error: {deltaSubscription.Error}");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnError(new VisualHFT.PluginManager.ErrorEventArgs() { IsCritical = true, PluginName = Name, Exception = ex });
                }
            });
            // ***************

        }
        private void InitializeSnapshots()
        {
            foreach (var symbol in GetAllNonNormalizedSymbols())
            {
                var normalizedSymbol = GetNormalizedSymbol(symbol);
                // Fetch initial depth snapshot
                var depthSnapshot = _restClient.SpotApi.ExchangeData.GetOrderBookAsync(symbol, _settings.DepthLevels).Result;
                if (!_localOrderBooks.ContainsKey(normalizedSymbol))
                    _localOrderBooks.Add(normalizedSymbol, null);
                if (!_localOrderBooks_LastUpdate.ContainsKey(normalizedSymbol))
                    _localOrderBooks_LastUpdate.Add(normalizedSymbol, -1);

                _ct[normalizedSymbol] = new CancellationTokenSource();
                if (depthSnapshot.Success)
                {
                    _localOrderBooks[normalizedSymbol] = ToOrderBookModel(depthSnapshot.Data);
                    _localOrderBooks_LastUpdate[normalizedSymbol] = depthSnapshot.Data.LastUpdateId;

                    //launch Task. in a new thread with _ct as cancellation
                    Task.Run(async () => {
                        while (!_ct[normalizedSymbol].IsCancellationRequested)
                        {
                            // Process buffered events
                            ProcessBufferedEvents(normalizedSymbol);
                            await Task.Delay(1); // Prevents tight looping, adjust as needed
                        }
                    });                    
                }
                else
                {                    
                    string erroMsg = $"{this.Name} getting Snapshot error for {symbol}: {depthSnapshot.ResponseStatusCode} - {depthSnapshot.Error}";
                    RaiseOnError(new VisualHFT.PluginManager.ErrorEventArgs() { IsCritical = true, PluginName = Name, Exception = new Exception(erroMsg) });
                }
            }
        }
        private async Task HandleConnectionLost()
        {
            // Close the connection on a background thread
            await Task.Run(() => Stop());
            // Wait 
            await Task.Delay(TimeSpan.FromSeconds(10));
            // Start the connection again
            Start();
        }

        #region Websocket Deltas Callbacks
        private void AttachEventHandlers(UpdateSubscription data)
        {
            data.Exception += deltaSubscription_Exception;
            data.ConnectionLost += deltaSubscription_ConnectionLost;
            data.ConnectionClosed += deltaSubscription_ConnectionClosed;
            data.ConnectionRestored += deltaSubscription_ConnectionRestored;
            data.ActivityPaused += deltaSubscription_ActivityPaused;
            data.ActivityUnpaused += deltaSubscription_ActivityUnpaused;
        }
        private void UnattachEventHandlers(UpdateSubscription data)
        {
            data.Exception -= deltaSubscription_Exception;
            data.ConnectionLost -= deltaSubscription_ConnectionLost;
            data.ConnectionClosed -= deltaSubscription_ConnectionClosed;
            data.ConnectionRestored -= deltaSubscription_ConnectionRestored;
            data.ActivityPaused -= deltaSubscription_ActivityPaused;
            data.ActivityUnpaused -= deltaSubscription_ActivityUnpaused;
        }
        private void deltaSubscription_ActivityUnpaused()
        {
            throw new NotImplementedException();
        }
        private void deltaSubscription_ActivityPaused()
        {
            throw new NotImplementedException();
        }
        private void deltaSubscription_ConnectionRestored(TimeSpan obj)
        {
            throw new NotImplementedException();
        }
        private void deltaSubscription_ConnectionClosed()
        {
            // Start the HandleConnectionLost task without awaiting it
            _ = HandleConnectionLost();
        }
        private void deltaSubscription_ConnectionLost()
        {
            // Start the HandleConnectionLost task without awaiting it
            _ = HandleConnectionLost();
        }
        private void deltaSubscription_Exception(Exception obj)
        {
            throw new NotImplementedException();
        }
        #endregion
        private void ProcessBufferedTrades(string symbol)
        {
            lock (_lock_tradeBuffers)
            {                
                List<VisualHFT.Model.Trade> _trades = new List<VisualHFT.Model.Trade>();
                while (_tradesBuffers[symbol].Count > 0)
                {
                    var eventData = _tradesBuffers[symbol].Dequeue();
                    try
                    {
                        
                        _trades.Add(new VisualHFT.Model.Trade()
                        {
                            Price = eventData.Price,
                            Size = eventData.Quantity,
                            Symbol = symbol,
                            Timestamp = eventData.TradeTime.ToLocalTime(),
                            ProviderId = _settings.ProviderId,
                            ProviderName = _settings.ProviderName,
                            IsBuy = eventData.BuyerIsMaker                           
                        }); ;
                        
                    }
                    catch (Exception ex)
                    {

                    }
                }
                if (_trades.Any())
                    RaiseOnDataReceived(new DataEventArgs() { DataType = "Trades", ParsedModel = _trades, RawData = "" });
            }
        }
        private void ProcessBufferedEvents(string normalizedSymbol)
        {
            var lastUpdateId = _localOrderBooks_LastUpdate[normalizedSymbol];
            List<IBinanceEventOrderBook> eventsToProces = new List<IBinanceEventOrderBook>();
            lock (_lock_eventBuffers)
            {
                while (_eventBuffers[normalizedSymbol].Count > 0)
                    eventsToProces.Add(_eventBuffers[normalizedSymbol].Dequeue());
            }
            foreach(var eventData in eventsToProces)
            {
                if (eventData.LastUpdateId <= lastUpdateId) continue;
                try
                {
                    UpdateOrderBook(eventData, normalizedSymbol);
                }
                catch (Exception ex)
                {}
                lastUpdateId = eventData.LastUpdateId;
            }
            _localOrderBooks_LastUpdate[normalizedSymbol] = lastUpdateId;
        }
        private void UpdateOrderBook(IBinanceEventOrderBook lob_update, string normalizedSymbol)
        {
            if (!_localOrderBooks.ContainsKey(normalizedSymbol))
                return;

            var local_lob = _localOrderBooks[normalizedSymbol];
            var _bids = local_lob.Bids.ToList();
            var _asks = local_lob.Asks.ToList();
            DateTime ts = lob_update.EventTime.ToLocalTime();

            foreach (var item in lob_update.Bids)
            {
                var toUpdate = _bids.FirstOrDefault(x => x != null && x.Price == (double)item.Price);
                if (toUpdate == null && item.Quantity != 0)
                {
                    _bids.Add(new VisualHFT.Model.BookItem()
                    {
                        Price = (double)item.Price,
                        Size = (double)item.Quantity,
                        LocalTimeStamp = DateTime.Now,
                        ServerTimeStamp = ts,
                        DecimalPlaces = local_lob.DecimalPlaces,
                        IsBid = true,
                        ProviderID = _settings.ProviderId,
                        Symbol = local_lob.Symbol
                    });
                }
                else if (toUpdate != null && item.Quantity != 0)
                {
                    toUpdate.Size = (double)item.Quantity;
                    toUpdate.LocalTimeStamp = DateTime.Now;
                    toUpdate.ServerTimeStamp = ts;
                }
                else if (toUpdate != null && item.Quantity == 0)
                {
                    _bids.Remove(toUpdate);
                }
                else
                    continue;
            }
            foreach (var item in lob_update.Asks)
            {
                var toUpdate = _asks.FirstOrDefault(x => x != null && x.Price.Value == (double)item.Price);
                if (toUpdate == null && item.Quantity != 0)
                {
                    _asks.Add(new VisualHFT.Model.BookItem()
                    {
                        Price = (double)item.Price,
                        Size = (double)item.Quantity,
                        LocalTimeStamp = DateTime.Now,
                        ServerTimeStamp = ts,
                        DecimalPlaces = local_lob.DecimalPlaces,
                        IsBid = false,
                        ProviderID = _settings.ProviderId,
                        Symbol = local_lob.Symbol
                    });
                }
                else if (toUpdate != null && item.Quantity != 0)
                {
                    toUpdate.Size = (double)item.Quantity;
                    toUpdate.LocalTimeStamp = DateTime.Now;
                    toUpdate.ServerTimeStamp = ts;
                }
                else if (toUpdate != null && item.Quantity == 0)
                {
                    _asks.Remove(toUpdate);
                }
                else
                    continue;
            }
            
            local_lob.LoadData(
                _asks.OrderBy(x => x.Price).Take(_settings.DepthLevels), 
                _bids.OrderByDescending(x => x.Price).Take(_settings.DepthLevels)
            );

            RaiseOnDataReceived(new DataEventArgs() { DataType = "Market", ParsedModel = new List<VisualHFT.Model.OrderBook>() { local_lob }, RawData = "" });
        }
        private void CheckConnectionStatus(object state)
        {
            bool isConnected = _socketClient.CurrentConnections > 0;
            RaiseOnDataReceived(new DataEventArgs() { DataType = "HeartBeats", ParsedModel = new List<VisualHFT.Model.Provider>() { ToHeartBeatModel(isConnected) }, RawData = "" });
        }
        private VisualHFT.Model.OrderBook ToOrderBookModel(BinanceOrderBook data)
        {
            var lob = new VisualHFT.Model.OrderBook();
            lob.Symbol = GetNormalizedSymbol(data.Symbol);
            lob.SymbolMultiplier = 2; //???????
            lob.DecimalPlaces = 2; //?????????
            lob.ProviderID = _settings.ProviderId;
            lob.ProviderName = _settings.ProviderName;

            var _asks = new List<VisualHFT.Model.BookItem>();
            var _bids = new List<VisualHFT.Model.BookItem>();
            data.Asks.ToList().ForEach(x =>
            {
                _asks.Add(new VisualHFT.Model.BookItem()
                {
                    IsBid = false,
                    Price = (double)x.Price,
                    Size = (double)x.Quantity,
                    LocalTimeStamp = DateTime.Now,
                    ServerTimeStamp = DateTime.Now,
                    Symbol = lob.Symbol,
                    DecimalPlaces = lob.DecimalPlaces,
                    ProviderID = lob.ProviderID,
                });
            });
            data.Bids.ToList().ForEach(x =>
            {
                _bids.Add(new VisualHFT.Model.BookItem()
                {
                    IsBid = true,
                    Price = (double)x.Price,
                    Size = (double)x.Quantity,
                    LocalTimeStamp = DateTime.Now,
                    ServerTimeStamp = DateTime.Now,
                    Symbol = lob.Symbol,
                    DecimalPlaces = lob.DecimalPlaces,
                    ProviderID = lob.ProviderID,
                });
            });
            lob.LoadData(_asks, _bids);
            return lob;
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



        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposed)
            {
                if (disposing)
                {
                    _ctDeltas.Cancel();
                    foreach (var token in _ct.Values)
                        token.Cancel();
                    foreach (var token in _ct_trades.Values)
                        token.Cancel();

                    UnattachEventHandlers(tradesSubscription.Data);
                    UnattachEventHandlers(deltaSubscription.Data);

                    _localOrderBooks?.Clear();
                    _eventBuffers?.Clear();
                    _tradesBuffers?.Clear();
                    _ct?.Clear();
                    _ct_trades?.Clear();

                    _socketClient?.Dispose();
                    _restClient?.Dispose();
                    _heartbeatTimer?.Dispose();
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
            ParseSymbols(string.Join(',', _settings.Symbols.ToArray())); //Utilize normalization function
        }
        protected override void SaveSettings()
        {
            SaveToUserSettings(_settings);
        }

        protected override void InitializeDefaultSettings()
        {
            _settings = new PlugInSettings()
            {
                ApiKey = "",
                ApiSecret = "",
                DepthLevels = 10,
                UpdateIntervalMs = 100,
                ProviderId = 1,
                ProviderName = "Binance.US",
                Symbols = new List<string>() { "BTCUSDT(BTC/USD)", "ETHUSDT(ETH/USD)" } // Add more symbols as needed
            };
            SaveToUserSettings(_settings);
        }
        public override object GetUISettings()
        {
            PluginSettingsView view = new PluginSettingsView();
            PluginSettingsViewModel viewModel = new PluginSettingsViewModel(CloseSettingWindow);
            viewModel.ApiSecret = _settings.ApiSecret;
            viewModel.ApiKey = _settings.ApiKey;
            viewModel.UpdateIntervalMs = _settings.UpdateIntervalMs;
            viewModel.DepthLevels = _settings.DepthLevels;
            viewModel.ProviderId = _settings.ProviderId;
            viewModel.ProviderName = _settings.ProviderName;
            viewModel.Symbols = _settings.Symbols;
            viewModel.UpdateSettingsFromUI = () =>
            {
                _settings.ApiSecret = viewModel.ApiSecret;
                _settings.ApiKey = viewModel.ApiKey;
                _settings.UpdateIntervalMs = viewModel.UpdateIntervalMs;
                _settings.DepthLevels = viewModel.DepthLevels;
                _settings.ProviderId = viewModel.ProviderId;
                _settings.ProviderName = viewModel.ProviderName;
                _settings.Symbols = viewModel.Symbols;
                SaveSettings();
                ParseSymbols(string.Join(',', _settings.Symbols.ToArray()));
                HandleConnectionLost(); //run this because it will allow to reconnect with the new values
            };
            // Display the view, perhaps in a dialog or a new window.
            view.DataContext = viewModel;
            return view;
        }
    }
}
