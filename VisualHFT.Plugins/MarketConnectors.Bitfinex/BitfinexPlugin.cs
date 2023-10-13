using Bitfinex.Net;
using Bitfinex.Net.Clients;
using Bitfinex.Net.Objects.Models;
using Bitfinex.Net.Enums;

using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using MarketConnectors.Bitfinex.Model;
using MarketConnectors.Bitfinex.UserControls;
using MarketConnectors.Bitfinex.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VisualHFT.Commons.PluginManager;
using VisualHFT.DataRetriever;
using VisualHFT.DataTradeRetriever;
using VisualHFT.UserSettings;


namespace MarketConnectors.Bitfinex
{
    public class BitfinexPlugin : BasePluginDataRetriever
    {
        private bool _disposed = false; // to track whether the object has been disposed

        private PlugInSettings _settings;
        private BitfinexSocketClient _socketClient;
        private BitfinexRestClient _restClient;
        private Dictionary<string, VisualHFT.Model.OrderBook> _localOrderBooks = new Dictionary<string, VisualHFT.Model.OrderBook>();
        private Dictionary<string, Queue<Tuple<DateTime, BitfinexOrderBookEntry>>> _eventBuffers = new Dictionary<string, Queue<Tuple<DateTime, BitfinexOrderBookEntry>>>();
        private Dictionary<string, Queue<BitfinexTradeSimple>> _tradesBuffers = new Dictionary<string, Queue<BitfinexTradeSimple>>();
        private CancellationTokenSource _ctDeltas;
        private Dictionary<string, CancellationTokenSource> _ct;
        private Dictionary<string, CancellationTokenSource> _ct_trades;
        private object _lock_eventBuffers = new object();
        private object _lock_tradeBuffers = new object();

        private Timer _heartbeatTimer;
        private CallResult<UpdateSubscription> deltaSubscription;
        private CallResult<UpdateSubscription> tradesSubscription;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public override string Name { get; set; } = "Bitfinex Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Connects to Bitfinex websockets.";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action CloseSettingWindow { get; set; }

        public BitfinexPlugin()
        {
            _ct = new Dictionary<string, CancellationTokenSource>();
            _ct_trades = new Dictionary<string, CancellationTokenSource>();
            _socketClient = new BitfinexSocketClient(options =>
            {
                if (_settings.ApiKey != "" && _settings.ApiSecret != "")
                    options.ApiCredentials = new ApiCredentials(_settings.ApiKey, _settings.ApiSecret);
                options.Environment = BitfinexEnvironment.Live;
                options.AutoReconnect = true;
            });

            _restClient = new BitfinexRestClient(options =>
            {
                if (_settings.ApiKey != "" && _settings.ApiSecret != "")
                    options.ApiCredentials = new ApiCredentials(_settings.ApiKey, _settings.ApiSecret);
                options.Environment = BitfinexEnvironment.Live;
            });
            // Initialize the timer
            _heartbeatTimer = new Timer(CheckConnectionStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // Check every 5 seconds
        }
        ~BitfinexPlugin()
        {
            Dispose(false);
        }

        public override async Task StartAsync()
        {
            _eventBuffers.Clear();
            _tradesBuffers.Clear();
            _ct.Clear();
            _ct_trades.Clear();

            foreach (var sym in GetAllNonNormalizedSymbols())
            {
                var symbol = GetNormalizedSymbol(sym);
                // Initialize event buffer for each symbol
                lock (_lock_eventBuffers)
                    _eventBuffers.Add(symbol, new Queue<Tuple<DateTime, BitfinexOrderBookEntry>>());
                lock (_lock_tradeBuffers)
                    _tradesBuffers.Add(symbol, new Queue<BitfinexTradeSimple>());

                _ct_trades.Add(symbol, null);
                _ct.Add(symbol, null);
            }

            InitializeTrades();
            InitializeDeltas();
            InitializeSnapshots();
            await base.StartAsync();
        }
        public override async Task StopAsync()
        {
            _ctDeltas.Cancel();
            foreach (var token in _ct.Values)
                token.Cancel();
            foreach (var token in _ct_trades.Values)
                token.Cancel();

            UnattachEventHandlers(tradesSubscription.Data);
            UnattachEventHandlers(deltaSubscription.Data);

            await deltaSubscription.Data.CloseAsync();
            await tradesSubscription.Data.CloseAsync();

            //reset models
            RaiseOnDataReceived(new DataEventArgs() { DataType = "Market", ParsedModel = new List<VisualHFT.Model.OrderBook>(), RawData = "" });
            RaiseOnDataReceived(new DataEventArgs() { DataType = "HeartBeats", ParsedModel = new List<VisualHFT.Model.Provider>() { ToHeartBeatModel(false) }, RawData = "" });

            await base.StopAsync();
        }
        private void InitializeTrades()
        {
            Task.Run(async () =>
            {
                try
                {
                    foreach (var symbol in GetAllNonNormalizedSymbols())
                    {
                        tradesSubscription = await _socketClient.SpotApi.SubscribeToTradeUpdatesAsync(
                            symbol,
                            trade =>
                            {
                                // Buffer the trades
                                if (trade.Data != null)
                                {
                                    lock (_lock_tradeBuffers)
                                    {
                                        foreach (var item in trade.Data)
                                            _tradesBuffers[GetNormalizedSymbol(symbol)].Enqueue(item);
                                    }

                                }
                            });
                        if (tradesSubscription.Success)
                        {
                            AttachEventHandlers(tradesSubscription.Data);
                            InitializeBufferProcessingTasks();
                        }
                        else
                        {
                            throw new Exception($"{this.Name} trades subscription for {symbol} error: {tradesSubscription.Error}");
                        }

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
                    foreach (var symbol in GetAllNonNormalizedSymbols())
                    {
                        deltaSubscription = await _socketClient.SpotApi.SubscribeToOrderBookUpdatesAsync(
                            symbol,
                            Precision.PrecisionLevel0,
                            Frequency.Realtime,
                            _settings.DepthLevels,
                            data =>
                            {
                                // Buffer the events
                                if (data.Data != null)
                                {
                                    var normalizedSymbol = GetNormalizedSymbol(symbol);
                                    lock (_lock_eventBuffers)
                                    {
                                        foreach (var item in data.Data)
                                        {
                                            if (_eventBuffers.ContainsKey(normalizedSymbol))
                                                _eventBuffers[normalizedSymbol].Enqueue(new Tuple<DateTime, BitfinexOrderBookEntry>(data.Timestamp.ToLocalTime(), item));
                                        }
                                    }
                                }
                            }, null, _ctDeltas.Token);
                        if (deltaSubscription.Success)
                        {
                            AttachEventHandlers(deltaSubscription.Data);
                        }
                        else
                        {
                            throw new Exception($"{this.Name} deltas subscription for {symbol} error: {deltaSubscription.Error}");
                        }

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
                try
                {
                    var normalizedSymbol = GetNormalizedSymbol(symbol);

                    // Fetch initial depth snapshot
                    var depthSnapshot = _restClient.SpotApi.ExchangeData.GetOrderBookAsync(symbol, Precision.PrecisionLevel0, _settings.DepthLevels).Result;
                    if (!_localOrderBooks.ContainsKey(normalizedSymbol))
                        _localOrderBooks.Add(normalizedSymbol, null);

                    _ct[normalizedSymbol] = new CancellationTokenSource();
                    if (depthSnapshot.Success)
                    {
                        _localOrderBooks[normalizedSymbol] = ToOrderBookModel(depthSnapshot.Data, normalizedSymbol);

                        //launch Task. in a new thread with _ct as cancellation
                        Task.Run(async () =>
                        {
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
                catch (Exception ex)
                {
                    RaiseOnError(new VisualHFT.PluginManager.ErrorEventArgs() { IsCritical = true, PluginName = Name, Exception = ex });
                }
            }
        }
        private async Task HandleConnectionLost()
        {
            // Close the connection on a background thread
            await StopAsync();
            // Wait 
            await Task.Delay(TimeSpan.FromSeconds(10));
            // Start the connection again
            await StartAsync();
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
                        var typeEvent = eventData.UpdateType;
                        _trades.Add(new VisualHFT.Model.Trade()
                        {
                            Price = eventData.Price,
                            Size = Math.Abs(eventData.Quantity),
                            Symbol = symbol,
                            Timestamp = eventData.Timestamp.ToLocalTime(),
                            ProviderId = _settings.ProviderId,
                            ProviderName = _settings.ProviderName,
                            IsBuy = eventData.Quantity > 0,
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
            List<Tuple<DateTime, BitfinexOrderBookEntry>> eventsToProcess = new List<Tuple<DateTime, BitfinexOrderBookEntry>>();
            lock (_lock_eventBuffers)
            {
                while (_eventBuffers[normalizedSymbol].Count > 0)
                    eventsToProcess.Add(_eventBuffers[normalizedSymbol].Dequeue());
            }

            foreach (var eventData in eventsToProcess)
            {
                try
                {
                    UpdateOrderBook(eventData.Item2, normalizedSymbol, eventData.Item1);
                }
                catch (Exception ex)
                { }

            }
        }
        private void UpdateOrderBook(BitfinexOrderBookEntry lob_update, string symbol, DateTime ts)
        {
            if (!_localOrderBooks.ContainsKey(symbol))
                return;
            if (lob_update == null)
                return;

            var local_lob = _localOrderBooks[symbol];
            var _bids = local_lob.Bids.ToList();
            var _asks = local_lob.Asks.ToList();


            if (lob_update.Count > 0) //add or update level
            {
                bool isBid = lob_update.Quantity > 0;
                if (isBid)
                {
                    var itemToUpdate = _bids.FirstOrDefault(x => x.Price == (double)lob_update.Price);
                    if (itemToUpdate != null)
                    {
                        itemToUpdate.Size = (double)Math.Abs(lob_update.Quantity);
                        itemToUpdate.LocalTimeStamp = DateTime.Now;
                        itemToUpdate.ServerTimeStamp = ts;
                    }
                    else
                    {
                        _bids.Add(new VisualHFT.Model.BookItem()
                        {
                            Price = (double)lob_update.Price,
                            Size = (double)Math.Abs(lob_update.Quantity),
                            LocalTimeStamp = DateTime.Now,
                            ServerTimeStamp = ts,
                            DecimalPlaces = local_lob.DecimalPlaces,
                            IsBid = isBid,
                            ProviderID = _settings.ProviderId,
                            Symbol = local_lob.Symbol
                        });
                    }
                }
                else
                {
                    var itemToUpdate = _asks.FirstOrDefault(x => x.Price == (double)lob_update.Price);
                    if (itemToUpdate != null)
                    {
                        itemToUpdate.Size = (double)Math.Abs(lob_update.Quantity);
                        itemToUpdate.LocalTimeStamp = DateTime.Now;
                        itemToUpdate.ServerTimeStamp = ts;
                    }
                    else
                    {
                        _asks.Add(new VisualHFT.Model.BookItem()
                        {
                            Price = (double)lob_update.Price,
                            Size = (double)Math.Abs(lob_update.Quantity),
                            LocalTimeStamp = DateTime.Now,
                            ServerTimeStamp = ts,
                            DecimalPlaces = local_lob.DecimalPlaces,
                            IsBid = isBid,
                            ProviderID = _settings.ProviderId,
                            Symbol = local_lob.Symbol
                        });
                    }

                }
            }
            else
            {
                if (lob_update.Quantity == 1) //remove from bids
                {
                    _bids.RemoveAll(x => x.Price == (double)lob_update.Price);
                }
                else if (lob_update.Quantity == -1) //remove from asks
                {
                    _asks.RemoveAll(x => x.Price == (double)lob_update.Price);
                }

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
        private VisualHFT.Model.OrderBook ToOrderBookModel(BitfinexOrderBook data, string symbol)
        {
            var lob = new VisualHFT.Model.OrderBook();
            lob.Symbol = symbol;
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
                DepthLevels = 25,
                ProviderId = 2, //must be unique
                ProviderName = "Bitfinex",
                Symbols = new List<string>() { "tBTCUSD(BTC/USD)", "tETHUSD(ETH/USD)" } // Add more symbols as needed
            };
            SaveToUserSettings(_settings);
        }
        public override object GetUISettings()
        {
            PluginSettingsView view = new PluginSettingsView();
            PluginSettingsViewModel viewModel = new PluginSettingsViewModel(CloseSettingWindow);
            viewModel.ApiSecret = _settings.ApiSecret;
            viewModel.ApiKey = _settings.ApiKey;
            viewModel.DepthLevels = _settings.DepthLevels;
            viewModel.ProviderId = _settings.ProviderId;
            viewModel.ProviderName = _settings.ProviderName;
            viewModel.Symbols = _settings.Symbols;
            viewModel.UpdateSettingsFromUI = () =>
            {
                _settings.ApiSecret = viewModel.ApiSecret;
                _settings.ApiKey = viewModel.ApiKey;
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
