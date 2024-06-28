using Bitfinex.Net;
using Bitfinex.Net.Clients;
using Bitfinex.Net.Objects.Models;
using Bitfinex.Net.Enums;

using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using MarketConnectors.Bitfinex.Model;
using MarketConnectors.Bitfinex.UserControls;
using MarketConnectors.Bitfinex.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VisualHFT.Commons.PluginManager;
using VisualHFT.UserSettings;
using VisualHFT.Commons.Pools;
using VisualHFT.Commons.Model;
using VisualHFT.Commons.Helpers;
using CryptoExchange.Net.Objects.Sockets;
using VisualHFT.Enums;
using VisualHFT.PluginManager;

namespace MarketConnectors.Bitfinex
{
    public class BitfinexPlugin : BasePluginDataRetriever
    {
        private bool _disposed = false; // to track whether the object has been disposed

        private PlugInSettings _settings;
        private BitfinexSocketClient _socketClient;
        private BitfinexRestClient _restClient;
        private Dictionary<string, VisualHFT.Model.OrderBook> _localOrderBooks = new Dictionary<string, VisualHFT.Model.OrderBook>();
        private Dictionary<string, HelperCustomQueue<Tuple<DateTime, string, BitfinexOrderBookEntry>>> _eventBuffers =
            new Dictionary<string, HelperCustomQueue<Tuple<DateTime, string, BitfinexOrderBookEntry>>>();

        private Dictionary<string, HelperCustomQueue<Tuple<string, BitfinexTradeSimple>>> _tradesBuffers =
            new Dictionary<string, HelperCustomQueue<Tuple<string, BitfinexTradeSimple>>>();

        private int pingFailedAttempts = 0;
        private System.Timers.Timer _timerPing;
        private CallResult<UpdateSubscription> deltaSubscription;
        private CallResult<UpdateSubscription> tradesSubscription;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly CustomObjectPool<VisualHFT.Model.Trade> tradePool = new CustomObjectPool<VisualHFT.Model.Trade>();//pool of Trade objects


        public override string Name { get; set; } = "Bitfinex Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Connects to Bitfinex websockets.";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action CloseSettingWindow { get; set; }

        public BitfinexPlugin()
        {
            SetReconnectionAction(InternalStartAsync);
            log.Info($"{this.Name} has been loaded.");
        }
        ~BitfinexPlugin()
        {
            Dispose(false);
        }

        public override async Task StartAsync()
        {

            await base.StartAsync();//call the base first

            _socketClient = new BitfinexSocketClient(options =>
            {
                if (_settings.ApiKey != "" && _settings.ApiSecret != "")
                    options.ApiCredentials = new ApiCredentials(_settings.ApiKey, _settings.ApiSecret);
                options.Environment = BitfinexEnvironment.Live;
            });

            _restClient = new BitfinexRestClient(options =>
            {
                if (_settings.ApiKey != "" && _settings.ApiSecret != "")
                    options.ApiCredentials = new ApiCredentials(_settings.ApiKey, _settings.ApiSecret);
                options.Environment = BitfinexEnvironment.Live;
            });
            try
            {
                await InternalStartAsync();
                if (Status == ePluginStatus.STOPPED_FAILED) //check again here for failure
                    return;
                log.Info($"Plugin has successfully started.");
                RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.CONNECTED));
                Status = ePluginStatus.STARTED;


            }
            catch (Exception ex)
            {
                var _error = ex.Message;
                log.Error(_error, ex);
                await HandleConnectionLost(_error, ex);
            }
        }
        private async Task InternalStartAsync()
        {
            await ClearAsync();

            // Initialize event buffer for each symbol
            foreach (var symbol in GetAllNormalizedSymbols())
            {
                _eventBuffers.Add(symbol, new HelperCustomQueue<Tuple<DateTime, string, BitfinexOrderBookEntry>>(eventBuffers_onReadAction, eventBuffers_onErrorAction));
                _tradesBuffers.Add(symbol, new HelperCustomQueue<Tuple<string, BitfinexTradeSimple>>(tradesBuffers_onReadAction, tradesBuffers_onErrorAction));
            }

            await InitializeSnapshotsAsync();
            await InitializeTradesAsync();
            await InitializeDeltasAsync();
            await InitializePingTimerAsync();
        }
        public override async Task StopAsync()
        {
            Status = ePluginStatus.STOPPING;
            log.Info($"{this.Name} is stopping.");

            await ClearAsync();
            RaiseOnDataReceived(new List<VisualHFT.Model.OrderBook>());
            RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.DISCONNECTED));

            await base.StopAsync();
        }
        public async Task ClearAsync()
        {

            UnattachEventHandlers(deltaSubscription?.Data);
            UnattachEventHandlers(tradesSubscription?.Data);
            if (_socketClient != null)
                await _socketClient.UnsubscribeAllAsync();
            if (deltaSubscription != null && deltaSubscription.Data != null)
                await deltaSubscription.Data.CloseAsync();
            if (tradesSubscription != null && tradesSubscription.Data != null)
                await tradesSubscription.Data.CloseAsync();
            _timerPing?.Stop();
            _timerPing?.Dispose();

            foreach (var q in _eventBuffers)
                q.Value.Clear();
            _eventBuffers.Clear();

            foreach (var q in _tradesBuffers)
                q.Value.Clear();
            _tradesBuffers.Clear();


            //CLEAR LOB
            if (_localOrderBooks != null)
            {
                foreach (var lob in _localOrderBooks)
                {
                    lob.Value?.Dispose();
                }
                _localOrderBooks.Clear();
            }
        }

        private async Task InitializeTradesAsync()
        {
            foreach (var symbol in GetAllNonNormalizedSymbols())
            {
                var _normalizedSymbol = GetNormalizedSymbol(symbol);
                var _traderQueueRef = _tradesBuffers[_normalizedSymbol];

                log.Info($"{this.Name}: sending WS Trades Subscription {_normalizedSymbol} ");
                tradesSubscription = await _socketClient.SpotApi.SubscribeToTradeUpdatesAsync(
                    symbol,
                    trade =>
                    {
                        // Buffer the trades
                        if (trade.Data != null)
                        {
                            try
                            {
                                foreach (var item in trade.Data)
                                {
                                    item.Timestamp = trade.Timestamp; //not sure why these are different
                                    _traderQueueRef.Add(
                                        new Tuple<string, BitfinexTradeSimple>(_normalizedSymbol, item));
                                }
                            }
                            catch (Exception ex)
                            {

                                var _error = $"Will reconnect. Unhandled error while receiving trading data for {_normalizedSymbol}.";
                                log.Error(_error, ex);
                                Task.Run(async () => await HandleConnectionLost(_error, ex));
                            }
                        }
                    });
                if (tradesSubscription.Success)
                {
                    AttachEventHandlers(tradesSubscription.Data);
                }
                else
                {
                    var _error = $"Unsuccessful trades subscription for {_normalizedSymbol} error: {tradesSubscription.Error}";
                    throw new Exception(_error);
                }
            }
        }
        private async Task InitializeDeltasAsync()
        {
            foreach (var symbol in GetAllNonNormalizedSymbols())
            {
                var normalizedSymbol = GetNormalizedSymbol(symbol);
                log.Info($"{this.Name}: sending WS Trades Subscription {normalizedSymbol} ");
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
                            try
                            {
                                foreach (var item in data.Data)
                                {
                                    _eventBuffers[normalizedSymbol].Add(
                                        new Tuple<DateTime, string, BitfinexOrderBookEntry>(
                                            data.Timestamp.ToLocalTime(), normalizedSymbol, item));
                                }
                            }
                            catch (Exception ex)
                            {

                                var _error = $"Will reconnect. Unhandled error while receiving delta market data for {normalizedSymbol}.";
                                log.Error(_error, ex);
                                Task.Run(async () => await HandleConnectionLost(_error, ex));
                            }
                        }
                    }, null, new CancellationToken());
                if (deltaSubscription.Success)
                {
                    AttachEventHandlers(deltaSubscription.Data);
                }
                else
                {
                    var _error = $"Unsuccessful deltas subscription for {normalizedSymbol} error: {deltaSubscription.Error}";
                    throw new Exception(_error);
                }
            }
        }
        private async Task InitializeSnapshotsAsync()
        {
            foreach (var symbol in GetAllNonNormalizedSymbols())
            {
                var normalizedSymbol = GetNormalizedSymbol(symbol);
                if (!_localOrderBooks.ContainsKey(normalizedSymbol))
                {
                    _localOrderBooks.Add(normalizedSymbol, null);
                }
                log.Info($"{this.Name}: Getting snapshot {normalizedSymbol} level 2");

                // Fetch initial depth snapshot
                var depthSnapshot = await _restClient.SpotApi.ExchangeData.GetOrderBookAsync(symbol, Precision.PrecisionLevel0, _settings.DepthLevels);
                if (depthSnapshot.Success)
                {
                    _localOrderBooks[normalizedSymbol] = ToOrderBookModel(depthSnapshot.Data, normalizedSymbol);
                    log.Info($"{this.Name}: LOB {normalizedSymbol} level 2 Successfully loaded.");
                }
                else
                {
                    var _error = $"Unsuccessful snapshot request for {normalizedSymbol} error: {depthSnapshot.ResponseStatusCode} - {depthSnapshot.Error}";
                    throw new Exception(_error);
                }
            }
        }
        private async Task InitializePingTimerAsync()
        {
            _timerPing?.Stop();
            _timerPing?.Dispose();

            _timerPing = new System.Timers.Timer(3000); // Set the interval to 3000 milliseconds (3 seconds)
            _timerPing.Elapsed += async (sender, e) => await DoPingAsync();
            _timerPing.AutoReset = true;
            _timerPing.Enabled = true; // Start the timer
        }

        private void eventBuffers_onReadAction(Tuple<DateTime, string, BitfinexOrderBookEntry> eventData)
        {
            UpdateOrderBook(eventData.Item3, eventData.Item2, eventData.Item1);
        }
        private void eventBuffers_onErrorAction(Exception ex)
        {
            var _error = $"Will reconnect. Unhandled error in the Market Data Queue: {ex.Message}";

            log.Error(_error, ex);
            Task.Run(async () => await HandleConnectionLost(_error, ex));
        }
        private void tradesBuffers_onReadAction(Tuple<string, BitfinexTradeSimple> item)
        {
            var trade = tradePool.Get();
            trade.Price = item.Item2.Price;
            trade.Size = Math.Abs(item.Item2.Quantity);
            trade.Symbol = item.Item1;
            trade.Timestamp = item.Item2.Timestamp.ToLocalTime();
            trade.ProviderId = _settings.Provider.ProviderID;
            trade.ProviderName = _settings.Provider.ProviderName;
            trade.IsBuy = item.Item2.Quantity > 0;
            trade.MarketMidPrice = _localOrderBooks[item.Item1].MidPrice;

            RaiseOnDataReceived(trade);
            tradePool.Return(trade);
        }
        private void tradesBuffers_onErrorAction(Exception ex)
        {
            var _error = $"Will reconnect. Unhandled error in the Trades Queue: {ex.Message}";

            log.Error(_error, ex);
            Task.Run(async () => await HandleConnectionLost(_error, ex));
        }


        #region Websocket Deltas Callbacks
        private void AttachEventHandlers(UpdateSubscription data)
        {
            if (data == null)
                return;
            data.Exception += deltaSubscription_Exception;
            data.ConnectionLost += deltaSubscription_ConnectionLost;
            data.ConnectionClosed += deltaSubscription_ConnectionClosed;
            data.ConnectionRestored += deltaSubscription_ConnectionRestored;
            data.ActivityPaused += deltaSubscription_ActivityPaused;
            data.ActivityUnpaused += deltaSubscription_ActivityUnpaused;
        }
        private void UnattachEventHandlers(UpdateSubscription data)
        {
            if (data == null)
                return;

            data.Exception -= deltaSubscription_Exception;
            data.ConnectionLost -= deltaSubscription_ConnectionLost;
            data.ConnectionClosed -= deltaSubscription_ConnectionClosed;
            data.ConnectionRestored -= deltaSubscription_ConnectionRestored;
            data.ActivityPaused -= deltaSubscription_ActivityPaused;
            data.ActivityUnpaused -= deltaSubscription_ActivityUnpaused;
        }
        private void deltaSubscription_ActivityUnpaused()
        {
            //throw new NotImplementedException();
        }
        private void deltaSubscription_ActivityPaused()
        {
            //throw new NotImplementedException();
        }
        private void deltaSubscription_ConnectionRestored(TimeSpan obj)
        {
            //throw new NotImplementedException();
        }
        private void deltaSubscription_ConnectionClosed()
        {
            if (Status != ePluginStatus.STOPPING && Status != ePluginStatus.STOPPED) //avoid executing this if we are actually trying to disconnect.
                Task.Run(async () => await HandleConnectionLost("Websocket has been closed from the server (no informed reason)."));
        }
        private void deltaSubscription_ConnectionLost()
        {
            Task.Run(async () => await HandleConnectionLost("Websocket connection has been lost (no informed reason)."));
        }
        private void deltaSubscription_Exception(Exception obj)
        {
            string _error = $"Websocket error: {obj.Message}";
            log.Error(_error, obj);
            HelperNotificationManager.Instance.AddNotification(this.Name, _error, HelprNorificationManagerTypes.ERROR, HelprNorificationManagerCategories.PLUGINS);

            Task.Run(StopAsync);

            Status = ePluginStatus.STOPPED_FAILED;
            RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.DISCONNECTED_FAILED));
        }
        #endregion


        private void UpdateOrderBook(BitfinexOrderBookEntry lob_update, string symbol, DateTime ts)
        {
            if (!_localOrderBooks.ContainsKey(symbol))
                return;
            if (lob_update == null)
                return;

            var local_lob = _localOrderBooks[symbol];

            if (lob_update.Count > 0) //add or update level
            {
                bool isBid = lob_update.Quantity > 0;
                var delta = new DeltaBookItem()
                {
                    //EntryID = lob_update.Price.ToString(),
                    Price = (double)lob_update.Price,
                    Size = (double)Math.Abs(lob_update.Quantity),
                    IsBid = isBid,
                    LocalTimeStamp = DateTime.Now,
                    ServerTimeStamp = ts,
                    Symbol = local_lob.Symbol,
                    MDUpdateAction = eMDUpdateAction.None,
                };
                local_lob.AddOrUpdateLevel(delta);
            }
            else if (Math.Abs(lob_update.Quantity) == 1)
            {
                local_lob.DeleteLevel(new DeltaBookItem()
                {
                    IsBid = lob_update.Quantity == 1,
                    Price = (double)lob_update.Price
                });
            }

            RaiseOnDataReceived(local_lob);
        }
        private async Task DoPingAsync()
        {
            try
            {
                if (Status == ePluginStatus.STOPPED || Status == ePluginStatus.STOPPING || Status == ePluginStatus.STOPPED_FAILED)
                    return; //do not ping if any of these statues

                bool isConnected = _socketClient.CurrentConnections > 0;
                if (!isConnected)
                {
                    throw new Exception("The socket seems to be disconnected.");
                }


                DateTime ini = DateTime.Now;
                var result = await _restClient.SpotApi.ExchangeData.GetPlatformStatusAsync();
                if (result != null)
                {
                    var timeLapseInMicroseconds = DateTime.Now.Subtract(ini).TotalMicroseconds;


                    // Connection is healthy
                    pingFailedAttempts = 0; // Reset the failed attempts on a successful ping

                    RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.CONNECTED));
                }
                else
                {
                    // Consider the ping failed
                    throw new Exception("Ping failed, result was null.");
                }
            }
            catch (Exception ex)
            {

                if (++pingFailedAttempts >= 5) //5 attempts
                {
                    var _error = $"Will reconnect. Unhandled error in DoPingAsync. Initiating reconnection. {ex.Message}";

                    log.Error(_error, ex);

                    Task.Run(async () => await HandleConnectionLost(_error, ex));
                }
            }

        }
        private VisualHFT.Model.OrderBook ToOrderBookModel(BitfinexOrderBook data, string symbol)
        {
            var identifiedPriceDecimalPlaces = RecognizeDecimalPlacesAutomatically(data.Asks.Select(x => x.Price));

            var lob = new VisualHFT.Model.OrderBook(symbol, identifiedPriceDecimalPlaces, _settings.DepthLevels);
            lob.ProviderID = _settings.Provider.ProviderID;
            lob.ProviderName = _settings.Provider.ProviderName;
            lob.SizeDecimalPlaces = RecognizeDecimalPlacesAutomatically(data.Asks.Select(x => x.Quantity));

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
                    PriceDecimalPlaces = lob.PriceDecimalPlaces,
                    SizeDecimalPlaces = lob.SizeDecimalPlaces,
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
                    PriceDecimalPlaces = lob.PriceDecimalPlaces,
                    SizeDecimalPlaces = lob.SizeDecimalPlaces,
                    ProviderID = lob.ProviderID,
                });
            });

            lob.LoadData(
                _asks.OrderBy(x => x.Price).Take(_settings.DepthLevels),
                _bids.OrderByDescending(x => x.Price).Take(_settings.DepthLevels)
                );
            return lob;
        }


        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    UnattachEventHandlers(deltaSubscription?.Data);
                    UnattachEventHandlers(tradesSubscription?.Data);
                    _socketClient?.UnsubscribeAllAsync();
                    _socketClient?.Dispose();
                    _restClient?.Dispose();
                    _timerPing?.Dispose();

                    foreach (var q in _eventBuffers)
                        q.Value?.Dispose();
                    _eventBuffers.Clear();

                    foreach (var q in _tradesBuffers)
                        q.Value?.Dispose();
                    _tradesBuffers.Clear();


                    if (_localOrderBooks != null)
                    {
                        foreach (var lob in _localOrderBooks)
                        {
                            lob.Value?.Dispose();
                        }
                        _localOrderBooks.Clear();
                    }

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
                _settings.Provider = new VisualHFT.Model.Provider() { ProviderID = 2, ProviderName = "Bitfinex" };
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
                Provider = new VisualHFT.Model.Provider() { ProviderID = 2, ProviderName = "Bitfinex" },
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
            viewModel.ProviderId = _settings.Provider.ProviderID;
            viewModel.ProviderName = _settings.Provider.ProviderName;
            viewModel.Symbols = _settings.Symbols;
            viewModel.UpdateSettingsFromUI = () =>
            {
                _settings.ApiSecret = viewModel.ApiSecret;
                _settings.ApiKey = viewModel.ApiKey;
                _settings.DepthLevels = viewModel.DepthLevels;
                _settings.Provider = new VisualHFT.Model.Provider() { ProviderID = viewModel.ProviderId, ProviderName = viewModel.ProviderName };
                _settings.Symbols = viewModel.Symbols;
                SaveSettings();
                ParseSymbols(string.Join(',', _settings.Symbols.ToArray()));

                //run this because it will allow to reconnect with the new values
                RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.CONNECTING));
                Status = ePluginStatus.STARTING;
                Task.Run(async () => await HandleConnectionLost($"{this.Name} is starting (from reloading settings).", null, true));


            };
            // Display the view, perhaps in a dialog or a new window.
            view.DataContext = viewModel;
            return view;
        }
    }
}
