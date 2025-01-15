using Kucoin.Net;
using Kucoin.Net.Clients;
using Kucoin.Net.Objects.Models;
using Kucoin.Net.Enums;

using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using MarketConnectors.KuCoin.Model;
using MarketConnectors.KuCoin.UserControls;
using MarketConnectors.KuCoin.ViewModel;
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
using Kucoin.Net.Objects.Models.Spot;
using Kucoin.Net.Objects.Models.Spot.Socket;
using System.Data;
using CryptoExchange.Net.CommonObjects;
using Kucoin.Net.Objects;
using System.Runtime.CompilerServices;

namespace MarketConnectors.KuCoin
{
    public class KuCoinPlugin : BasePluginDataRetriever
    {
        private bool _disposed = false; // to track whether the object has been disposed

        private PlugInSettings _settings;
        private KucoinSocketClient _socketClient;
        private KucoinRestClient _restClient;
        private Dictionary<string, VisualHFT.Model.OrderBook> _localOrderBooks = new Dictionary<string, VisualHFT.Model.OrderBook>();

        private Dictionary<string, HelperCustomQueue<Tuple<DateTime, string, KucoinStreamOrderBookChanged>>> _eventBuffers =
            new Dictionary<string, HelperCustomQueue<Tuple<DateTime, string, KucoinStreamOrderBookChanged>>>();

        private Dictionary<string, HelperCustomQueue<Tuple<string, KucoinTrade>>> _tradesBuffers =
            new Dictionary<string, HelperCustomQueue<Tuple<string, KucoinTrade>>>();


        private Dictionary<string, VisualHFT.Model.Order> _localUserOrders = new Dictionary<string, VisualHFT.Model.Order>();


        private int pingFailedAttempts = 0;
        private System.Timers.Timer _timerPing;
        private CallResult<UpdateSubscription> deltaSubscription;
        private CallResult<UpdateSubscription> tradesSubscription;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly CustomObjectPool<VisualHFT.Model.Trade> tradePool = new CustomObjectPool<VisualHFT.Model.Trade>();//pool of Trade objects


        public override string Name { get; set; } = "KuCoin Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Connects to KuCoin websockets.";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action CloseSettingWindow { get; set; }

        public KuCoinPlugin()
        {
            SetReconnectionAction(InternalStartAsync);
            log.Info($"{this.Name} has been loaded.");
        }
        ~KuCoinPlugin()
        {
            Dispose(false);
        }

        public override async Task StartAsync()
        {

            await base.StartAsync();//call the base first
             _socketClient = new KucoinSocketClient(options => 
             { 
                  if (!string.IsNullOrEmpty(_settings.ApiKey)  && !string.IsNullOrEmpty(_settings.ApiSecret) && !string.IsNullOrEmpty(_settings.APIPassPhrase))
                 {
                     options.ApiCredentials = new Kucoin.Net.Objects.KucoinApiCredentials(_settings.ApiKey, _settings.ApiSecret, _settings.APIPassPhrase);
                 }

                 options.Environment = KucoinEnvironment.Live;
            });

              
            _restClient = new KucoinRestClient(options =>
            {
                if (!string.IsNullOrEmpty(_settings.ApiKey) && !string.IsNullOrEmpty(_settings.ApiSecret) && !string.IsNullOrEmpty(_settings.APIPassPhrase))
                {
                    options.ApiCredentials = new Kucoin.Net.Objects.KucoinApiCredentials(_settings.ApiKey, _settings.ApiSecret, _settings.APIPassPhrase);
                }

                options.Environment = KucoinEnvironment.Live;
            });

            var account=await _restClient.SpotApi.Account.GetDepositsAsync();
            


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
                _eventBuffers.Add(symbol, new HelperCustomQueue<Tuple<DateTime, string, KucoinStreamOrderBookChanged>>($"<Tuple<DateTime, string, KucoinStreamOrderBookChanged>>_{this.Name.Replace(" Plugin", "")}", eventBuffers_onReadAction, eventBuffers_onErrorAction));
                _tradesBuffers.Add(symbol, new HelperCustomQueue<Tuple<string, KucoinTrade>>($"<Tuple<DateTime, string, KucoinTrade>>_{this.Name.Replace(" Plugin", "")}", tradesBuffers_onReadAction, tradesBuffers_onErrorAction));
            }

            await InitializeSnapshotsAsync();
            await InitializeOpenOrders();
            await InitializeTradesAsync();
            await InitializeDeltasAsync();
            await InitializePingTimerAsync();
            await InitializeUserPrivateOrders();
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
                                
                                KucoinTrade trade1 = new KucoinTrade();
                                trade1.Sequence = trade.Data.Sequence;
                                trade1.Price = trade.Data.Price;
                                trade1.Timestamp= trade.Data.Timestamp;
                                trade1.Quantity = trade.Data.Quantity;

                                string _normalizedSymbol = "(null)";
                                if (trade != null && trade.Data != null)
                                {
                                    _normalizedSymbol = GetNormalizedSymbol(trade.Data.Symbol);
                                }
                                _traderQueueRef.Add(
                                       new Tuple<string, KucoinTrade>(_normalizedSymbol, trade1));

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

        private async Task InitializeOpenOrders()
        {

            var orders = await _restClient.SpotApi.CommonSpotClient.GetOpenOrdersAsync();
            if (orders != null)
            {
                foreach (Order item in orders.Data)
                {
                    VisualHFT.Model.Order localuserOrder;
                    if (!this._localUserOrders.ContainsKey(item.Id))
                    {
                        localuserOrder = new VisualHFT.Model.Order();
                        localuserOrder.ClOrdId = item.Id;
                        localuserOrder.Currency = GetNormalizedSymbol(item.Symbol);
                        localuserOrder.CreationTimeStamp = item.Timestamp;
                        localuserOrder.OrderID = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        //localuserOrder.OrderID = long.Parse(item.OrderId);
                        localuserOrder.QuoteServerTimeStamp = item.Timestamp;
                        localuserOrder.ProviderId = _settings!.Provider.ProviderID;
                        localuserOrder.ProviderName = _settings.Provider.ProviderName;
                        localuserOrder.CreationTimeStamp = item.Timestamp;
                        localuserOrder.Quantity = (double)item.Quantity;
                        localuserOrder.PricePlaced = (double)item.Price;
                        localuserOrder.Symbol = GetNormalizedSymbol(item.Symbol);
                        localuserOrder.TimeInForce = eORDERTIMEINFORCE.GTC;
                        /*
                        if (!string.IsNullOrEmpty(item.behaviour))
                        {
                            if (item.behavior.ToLower().Equals("immediate-or-cancel"))
                            {
                                localuserOrder.TimeInForce = eORDERTIMEINFORCE.IOC;
                            }   
                            else if (item.behavior.ToLower().Equals("fill-or-kill"))
                            {
                                localuserOrder.TimeInForce = eORDERTIMEINFORCE.FOK;
                            }
                            else if (item.behavior.ToLower().Equals("maker-or-cancel"))
                            {
                                localuserOrder.TimeInForce = eORDERTIMEINFORCE.MOK;
                            }
                        }
                        */
                        this._localUserOrders.Add(item.Id, localuserOrder);
                    }
                    else
                    {
                        localuserOrder = this._localUserOrders[item.Id];
                    }


                    if (item.Type == CommonOrderType.Market)
                    {
                        localuserOrder.OrderType = eORDERTYPE.MARKET;
                    }
                    else if (item.Type == CommonOrderType.Limit)
                    {
                        localuserOrder.OrderType = eORDERTYPE.LIMIT;

                    }
                    else
                    {
                        localuserOrder.OrderType = eORDERTYPE.PEGGED;
                    }


                    if (item.Side == CommonOrderSide.Buy)
                    {
                        localuserOrder.QuoteLocalTimeStamp = DateTime.Now;
                        localuserOrder.PricePlaced = (double)item.Price;
                        localuserOrder.BestBid = (double)item.Price;
                        localuserOrder.Side = eORDERSIDE.Buy;
                    }
                    if (item.Side == CommonOrderSide.Sell)
                    {
                        localuserOrder.Side = eORDERSIDE.Sell;
                        localuserOrder.BestAsk = (double)item.Price;
                        localuserOrder.QuoteLocalTimeStamp = DateTime.Now;
                        localuserOrder.Quantity = (double)item.Quantity;
                    }
                    localuserOrder.GetAvgPrice = (double)item.Price;
                    localuserOrder.LastUpdated = DateTime.Now;
                    localuserOrder.FilledPercentage = Math.Round((100 / localuserOrder.Quantity) * localuserOrder.FilledQuantity, 2);
                    RaiseOnDataReceived(localuserOrder);

                }
            }
        }

        private async Task UpdateUserOrder(KucoinStreamOrderNewUpdate item)
        {
            VisualHFT.Model.Order localuserOrder;
            if (!this._localUserOrders.ContainsKey(item.OrderId))
            {
                localuserOrder = new VisualHFT.Model.Order();
                localuserOrder.ClOrdId = item.OrderId;
                localuserOrder.Currency = GetNormalizedSymbol(item.Symbol);
                localuserOrder.CreationTimeStamp = item.OrderTime.Value;
                localuserOrder.OrderID = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                //localuserOrder.OrderID = long.Parse(item.OrderId);
                localuserOrder.QuoteServerTimeStamp = item.OrderTime.Value;
                localuserOrder.ProviderId = _settings!.Provider.ProviderID;
                localuserOrder.ProviderName = _settings.Provider.ProviderName;
                localuserOrder.CreationTimeStamp = item.OrderTime.Value;
                localuserOrder.Quantity = (double)item.OriginalQuantity;
                localuserOrder.PricePlaced = (double)item.Price;
                localuserOrder.Symbol = GetNormalizedSymbol(item.Symbol);
                localuserOrder.TimeInForce = eORDERTIMEINFORCE.GTC;
                /*
                if (!string.IsNullOrEmpty(item.behaviour))
                {
                    if (item.behavior.ToLower().Equals("immediate-or-cancel"))
                    {
                        localuserOrder.TimeInForce = eORDERTIMEINFORCE.IOC;
                    }   
                    else if (item.behavior.ToLower().Equals("fill-or-kill"))
                    {
                        localuserOrder.TimeInForce = eORDERTIMEINFORCE.FOK;
                    }
                    else if (item.behavior.ToLower().Equals("maker-or-cancel"))
                    {
                        localuserOrder.TimeInForce = eORDERTIMEINFORCE.MOK;
                    }
                }
                */
                this._localUserOrders.Add(item.OrderId, localuserOrder);
            }
            else
            {
                localuserOrder = this._localUserOrders[item.OrderId];
            }


            if (item.OrderType == OrderType.Market)
            {
                localuserOrder.OrderType = eORDERTYPE.MARKET;
            }
            else if (item.OrderType == OrderType.Limit)
            {
                localuserOrder.OrderType = eORDERTYPE.LIMIT;

            }
            else
            {
                localuserOrder.OrderType = eORDERTYPE.PEGGED;
            }


            if (item.Side == OrderSide.Buy)
            {
                localuserOrder.QuoteLocalTimeStamp = DateTime.Now;
                localuserOrder.PricePlaced = (double)item.Price;
                localuserOrder.BestBid = (double)item.Price;
                localuserOrder.Side = eORDERSIDE.Buy;
            }
            if (item.Side == OrderSide.Sell)
            {
                localuserOrder.Side = eORDERSIDE.Sell;
                localuserOrder.BestAsk = (double)item.Price;
                localuserOrder.QuoteLocalTimeStamp = DateTime.Now;
                localuserOrder.Quantity = (double)item.OriginalQuantity;
            }
            localuserOrder.GetAvgPrice = (double)item.Price;
            localuserOrder.LastUpdated = DateTime.Now;
            localuserOrder.FilledPercentage = Math.Round((100 / localuserOrder.Quantity) * localuserOrder.FilledQuantity, 2);
            RaiseOnDataReceived(localuserOrder);
        }
        private async Task UpdateUserOrder(KucoinStreamOrderUpdate item)
        {
            VisualHFT.Model.Order localuserOrder;
            if (!this._localUserOrders.ContainsKey(item.OrderId))
            {
                localuserOrder = new VisualHFT.Model.Order();
                localuserOrder.ClOrdId = item.OrderId;
                localuserOrder.Currency = GetNormalizedSymbol(item.Symbol);
                localuserOrder.CreationTimeStamp = item.OrderTime.Value;
                localuserOrder.OrderID = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                //localuserOrder.OrderID = long.Parse(item.OrderId);
                localuserOrder.QuoteServerTimeStamp = item.OrderTime.Value;
                localuserOrder.ProviderId = _settings!.Provider.ProviderID;
                localuserOrder.ProviderName = _settings.Provider.ProviderName;
                localuserOrder.CreationTimeStamp = item.OrderTime.Value;
                localuserOrder.Quantity = (double)item.OriginalQuantity;
                localuserOrder.PricePlaced = (double)item.Price;
                localuserOrder.Symbol = GetNormalizedSymbol(item.Symbol);
                localuserOrder.TimeInForce = eORDERTIMEINFORCE.GTC;
                /*
                if (!string.IsNullOrEmpty(item.behaviour))
                {
                    if (item.behavior.ToLower().Equals("immediate-or-cancel"))
                    {
                        localuserOrder.TimeInForce = eORDERTIMEINFORCE.IOC;
                    }   
                    else if (item.behavior.ToLower().Equals("fill-or-kill"))
                    {
                        localuserOrder.TimeInForce = eORDERTIMEINFORCE.FOK;
                    }
                    else if (item.behavior.ToLower().Equals("maker-or-cancel"))
                    {
                        localuserOrder.TimeInForce = eORDERTIMEINFORCE.MOK;
                    }
                }
                */
                this._localUserOrders.Add(item.OrderId, localuserOrder);
            }
            else
            {
                localuserOrder = this._localUserOrders[item.OrderId];
            }


            if (item.OrderType == OrderType.Market)
            {
                localuserOrder.OrderType = eORDERTYPE.MARKET;
            }
            else if (item.OrderType == OrderType.Limit)
            {
                localuserOrder.OrderType = eORDERTYPE.LIMIT;

            }
            else
            {
                localuserOrder.OrderType = eORDERTYPE.PEGGED;
            }


            if (item.Side == OrderSide.Buy)
            {
                localuserOrder.QuoteLocalTimeStamp = DateTime.Now;
                localuserOrder.PricePlaced = (double)item.Price;
                localuserOrder.BestBid = (double)item.Price;
                localuserOrder.Side = eORDERSIDE.Buy;
            }
            if (item.Side == OrderSide.Sell)
            {
                localuserOrder.Side = eORDERSIDE.Sell;
                localuserOrder.BestAsk = (double)item.Price;
                localuserOrder.QuoteLocalTimeStamp = DateTime.Now;
                localuserOrder.Quantity = (double)item.OriginalQuantity;
            }

            if (item.UpdateType == MatchUpdateType.Update)
            {
                if (item.Status == ExtendedOrderStatus.Open)
                {
                    localuserOrder.Status = eORDERSTATUS.NEW;
                }
                if (item.Status == ExtendedOrderStatus.Match)
                {
                    localuserOrder.Status = eORDERSTATUS.NEW;
                }
                if (item.Status == ExtendedOrderStatus.Done)
                {
                    localuserOrder.Status = eORDERSTATUS.CANCELED;
                }
            }

            if (item.UpdateType == MatchUpdateType.Filled)
            {
                localuserOrder.FilledQuantity = (double)item.QuantityFilled;
                localuserOrder.Status = eORDERSTATUS.FILLED;
            }
            if (item.UpdateType == MatchUpdateType.Canceled)
            {
                localuserOrder.Status = eORDERSTATUS.CANCELED;
            }

            localuserOrder.GetAvgPrice = (double)item.Price;
            localuserOrder.LastUpdated = DateTime.Now;
            localuserOrder.FilledPercentage = Math.Round((100 / localuserOrder.Quantity) * localuserOrder.FilledQuantity, 2);
            RaiseOnDataReceived(localuserOrder);
        }
        private async Task InitializeUserPrivateOrders()
        {  
            await _socketClient.SpotApi.SubscribeToOrderUpdatesAsync(neworder =>
            {
                log.Info(neworder.Data);
                if (neworder.Data != null)
                {
                    KucoinStreamOrderNewUpdate item = neworder.Data;
                    UpdateUserOrder(item);
                }
            }, onOrderData =>
            {
                if (onOrderData.Data != null)
                {

                    KucoinStreamOrderUpdate item = onOrderData.Data;

                    UpdateUserOrder(item);
                }
            },

            onTradeData =>
            {

            });


        }
        private async Task InitializeDeltasAsync()
        {

            foreach (var symbol in GetAllNonNormalizedSymbols())
            {
                var normalizedSymbol = GetNormalizedSymbol(symbol);
                log.Info($"{this.Name}: sending WS Trades Subscription {normalizedSymbol} ");
                deltaSubscription = await _socketClient.SpotApi.SubscribeToOrderBookUpdatesAsync(
                    symbol,
                    _settings.DepthLevels,
                    data =>
                    {  
                        // Buffer the events
                        if (data.Data != null)
                        {
                            try
                            { 
                                KucoinOrderBookEntry entry = new KucoinOrderBookEntry();
                                data.Timestamp = data.Timestamp;
                                if (Math.Abs(DateTime.Now.Subtract(data.Timestamp.ToLocalTime()).TotalSeconds) > 1)
                                {
                                    var _msg = $"Rates are coming late at {Math.Abs(DateTime.Now.Subtract(data.Timestamp.ToLocalTime()).TotalSeconds)} seconds.";
                                    log.Warn(_msg);
                                    HelperNotificationManager.Instance.AddNotification(this.Name, _msg, HelprNorificationManagerTypes.WARNING, HelprNorificationManagerCategories.PLUGINS);
                                }
                                _eventBuffers[normalizedSymbol].Add(
                                       new Tuple<DateTime, string, KucoinStreamOrderBookChanged>(
                                           data.Timestamp.ToLocalTime(), normalizedSymbol, data.Data));
                            }
                            catch (Exception ex)
                            {
                                string _normalizedSymbol = "(null)";
                                if (data != null && data.Data != null)
                                    _normalizedSymbol = GetNormalizedSymbol(data.Symbol);


                                var _error = $"Will reconnect. Unhandled error while receiving delta market data for {_normalizedSymbol}.";
                                log.Error(_error, ex);
                                Task.Run(async () => await HandleConnectionLost(_error, ex));
                            }
                        }
                    },new CancellationToken());
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
            try
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
                    var depthSnapshot = await _restClient.SpotApi.ExchangeData.GetAggregatedPartialOrderBookAsync(symbol, 20);//.ExchangeData.GetAggregatedFullOrderBookAsync(symbol);
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
            catch (Exception ex)
            {

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

        private void eventBuffers_onReadAction(Tuple<DateTime, string, KucoinStreamOrderBookChanged> eventData)
        {
            UpdateOrderBook(eventData.Item3, eventData.Item2, eventData.Item1);
        }
        private void eventBuffers_onErrorAction(Exception ex)
        {
            var _error = $"Will reconnect. Unhandled error in the Market Data Queue: {ex.Message}";

            log.Error(_error, ex);
            Task.Run(async () => await HandleConnectionLost(_error, ex));
        }
        private void tradesBuffers_onReadAction(Tuple<string, KucoinTrade> item)
        {
            var trade = tradePool.Get();
            trade.Price = item.Item2.Price;
            trade.Size = Math.Abs(item.Item2.Quantity);
            trade.Symbol = item.Item1;
            trade.Timestamp = item.Item2.Timestamp.ToLocalTime();
            trade.ProviderId = _settings.Provider.ProviderID;
            trade.ProviderName = _settings.Provider.ProviderName;
            trade.IsBuy = item.Item2.Side == OrderSide.Buy;
            trade.MarketMidPrice = _localOrderBooks[item.Item1]==null?0: _localOrderBooks[item.Item1].MidPrice;

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
         
        private void UpdateOrderBook(KucoinStreamOrderBookChanged lob_update, string symbol, DateTime ts)
        {
            if (!_localOrderBooks.ContainsKey(symbol))
                return;
            if (lob_update == null)
                return;

            var local_lob = _localOrderBooks[symbol];
            if (local_lob == null)
            {
                local_lob=new VisualHFT.Model.OrderBook();
            }
            foreach (var item in lob_update.Bids)
            {
                if (item.Quantity != 0)
                {
                    local_lob.AddOrUpdateLevel(new DeltaBookItem()
                    {
                        MDUpdateAction = eMDUpdateAction.None,
                        Price = (double)item.Price,
                        Size = (double)item.Quantity,
                        IsBid = true,
                        LocalTimeStamp = DateTime.Now,
                        ServerTimeStamp = ts,
                        Symbol = symbol
                    });
                }
                else
                    local_lob.DeleteLevel(new DeltaBookItem()
                    {
                        MDUpdateAction = eMDUpdateAction.Delete,
                        Price = (double)item.Price,
                        //Size = (double)item.Quantity,
                        IsBid = true,
                        LocalTimeStamp = DateTime.Now,
                        ServerTimeStamp = ts,
                        Symbol = symbol
                    });
            }
            foreach (var item in lob_update.Asks)
            {
                if (item.Quantity != 0)
                {
                    local_lob.AddOrUpdateLevel(new DeltaBookItem()
                    {
                        MDUpdateAction = eMDUpdateAction.None,
                        Price = (double)item.Price,
                        Size = (double)item.Quantity,
                        IsBid = false,
                        LocalTimeStamp = DateTime.Now,
                        ServerTimeStamp = ts,
                        Symbol = symbol
                    });
                }
                else
                    local_lob.DeleteLevel(new DeltaBookItem()
                    {
                        MDUpdateAction = eMDUpdateAction.Delete,
                        Price = (double)item.Price,
                        Size = (double)item.Quantity,
                        IsBid = false,
                        LocalTimeStamp = DateTime.Now,
                        ServerTimeStamp = ts,
                        Symbol = symbol
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
                var result = await _restClient.SpotApi.ExchangeData.GetServerTimeAsync();
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
        private VisualHFT.Model.OrderBook ToOrderBookModel(KucoinOrderBook data, string symbol)
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
            InitializeDefaultSettings();
            if (_settings.Provider == null) //To prevent back compability with older setting formats
            {
                _settings.Provider = new VisualHFT.Model.Provider() { ProviderID = 4, ProviderName = "KuCoin" };
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
                DepthLevels = 50,
                Provider = new VisualHFT.Model.Provider() { ProviderID = 4, ProviderName = "KuCoin" },
                Symbols = new List<string>() { "BTC-USDT(BTC/USD)" } // Add more symbols as needed
            };
            SaveToUserSettings(_settings);
        }
        public override object GetUISettings()
        {
            PluginSettingsView view = new PluginSettingsView();
            PluginSettingsViewModel viewModel = new PluginSettingsViewModel(CloseSettingWindow);
            viewModel.ApiSecret = _settings.ApiSecret;
            viewModel.ApiKey = _settings.ApiKey;
            viewModel.APIPassPhrase= _settings.APIPassPhrase;
            viewModel.DepthLevels = _settings.DepthLevels;
            viewModel.ProviderId = _settings.Provider.ProviderID;
            viewModel.ProviderName = _settings.Provider.ProviderName;
            viewModel.Symbols = _settings.Symbols;
            viewModel.UpdateSettingsFromUI = () =>
            {
                _settings.ApiSecret = viewModel.ApiSecret;
                _settings.ApiKey = viewModel.ApiKey;
                _settings.APIPassPhrase=viewModel.APIPassPhrase;

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
