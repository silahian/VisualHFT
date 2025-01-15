using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using Gemini.Net.Clients;
using Gemini.Net.Models;
using MarketConnectors.Gemini.Model;
using MarketConnectors.Gemini.UserControls;
using MarketConnectors.Gemini.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualHFT.Commons.Model;
using VisualHFT.Commons.PluginManager;
using VisualHFT.DataRetriever;
using VisualHFT.DataRetriever.DataParsers;
using VisualHFT.Enums;
using VisualHFT.Model;
using VisualHFT.PluginManager;
using VisualHFT.UserSettings;
using Websocket.Client;
using static MarketConnectors.Gemini.GeminiPlugin;
using OrderBook = VisualHFT.Model.OrderBook;
using Trade = VisualHFT.Model.Trade;

namespace MarketConnectors.Gemini
{
    public class GeminiPlugin : BasePluginDataRetriever
    { 
        private new bool _disposed = false; // to track whether the object has been disposed
        GeminiSubscription geminiSubscription = new GeminiSubscription();

        private Timer _heartbeatTimer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, VisualHFT.Model.OrderBook> _localOrderBooks = new Dictionary<string, VisualHFT.Model.OrderBook>();
        private Dictionary<string, VisualHFT.Model.Order> _localUserOrders = new Dictionary<string, VisualHFT.Model.Order>();
     
        private DataEventArgs tradeDataEvent = new DataEventArgs() { DataType = "Trades" }; //reusable object. So we avoid allocations
        private DataEventArgs marketDataEvent = new DataEventArgs() { DataType = "Market" };//reusable object. So we avoid allocations
        private DataEventArgs heartbeatDataEvent = new DataEventArgs() { DataType = "HeartBeats" };//reusable object. So we avoid allocations

        private PlugInSettings? _settings;
        public override string Name { get; set; } = "Gemini";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Connects to Gemini.";
        public override string Author { get ; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action? CloseSettingWindow { get; set; }

        private IDataParser _parser;
        JsonSerializerSettings? _parser_settings = null;
        WebsocketClient? _ws; 
        WebsocketClient? _userOrderEvents; 
         
        GeminiHttpClient geminiHttpClient;

        public GeminiPlugin()
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

            GeminiSubscription geminiSubscription= new GeminiSubscription();
            geminiSubscription.subscriptions = new List<Subscription>(); 

            geminiHttpClient = new GeminiHttpClient(); 
        }

        ~GeminiPlugin() 
        {
            Dispose(false);
        }


        public override async Task StartAsync()
        {
            await base.StartAsync();//call the base first 

            await InitializeSnapshotAsync();

            var symbols = GetAllNormalizedSymbols();

            geminiSubscription.subscriptions = new List<Subscription>();
            geminiSubscription.subscriptions.Add(new Subscription()
            {
                symbols = GetAllNonNormalizedSymbols()
            });

            if (!string.IsNullOrEmpty(_settings.ApiKey) && !string.IsNullOrEmpty(_settings.ApiSecret))
            {
                await Task.Run(async () =>
                {
                    await StartUserOrderEvents();
                });
            }

            try
            { 
                await Task.Run(async () =>
                {
                    var exitEvent = new ManualResetEvent(false);
                    var url = new Uri(_settings.WebSocketHostName);
                    using (_ws = new WebsocketClient(url))
                    { 
                        //"X-GEMINI-APIKEY": 
                        _ws.ReconnectTimeout = TimeSpan.FromSeconds(30);
                        _ws.ReconnectionHappened.Subscribe(info =>
                        {
                            if (info.Type == ReconnectionType.Error)
                            {
                                RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.CONNECTED));
                                Status = ePluginStatus.STOPPED_FAILED;
                            }
                            else if(info.Type == ReconnectionType.Initial)
                            {
                                foreach (var symbol in GetAllNonNormalizedSymbols())
                                {
                                    var normalizedSymbol = GetNormalizedSymbol(symbol);
                                    if (!_localOrderBooks.ContainsKey(normalizedSymbol))
                                    {
                                        _localOrderBooks.Add(normalizedSymbol, null);
                                    }
                                }
                            }
                        });
                        _ws.DisconnectionHappened.Subscribe(disconnected =>
                        {
                            RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.DISCONNECTED));
                            Status = ePluginStatus.STOPPED_FAILED;

                        });
                        _ws.MessageReceived.Subscribe(async msg =>
                        {
                            string data = msg.ToString();
                            HandleMessage(data,DateTime.Now);
                            RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.CONNECTED));
                        });
                        try
                        {
                            await _ws.Start();
                            log.Info($"Plugin has successfully started.");
                            RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.CONNECTED));
                            string jsonToSubscribe = JsonConvert.SerializeObject(geminiSubscription);
                            _ws.Send(jsonToSubscribe);
                            Status = ePluginStatus.STARTED;
                        }
                        catch(Exception ex) 
                        {
                            var _error = ex.Message;
                            log.Error(_error, ex);
                            await HandleConnectionLost(_error, ex);
                        } 
                        exitEvent.WaitOne();
                    }
                });
            }
            catch (Exception ex)
            {
                var _error = ex.Message;
                log.Error(_error, ex);
                await HandleConnectionLost(_error, ex);
            }
            CancellationTokenSource source = new CancellationTokenSource();
        }
        public static string SHA384Sign(string message, string key)
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
            {
                var signed = new HMACSHA384(Encoding.UTF8.GetBytes(key)).ComputeHash(stream)
                    .Aggregate(new StringBuilder(), (sb, b) => sb.AppendFormat("{0:x2}", b), (sb) => sb.ToString());
                return signed;
            }
        } 
        private string CreateSignature(string b64)
        { 
            using (var hmac = new HMACSHA384(Encoding.UTF8.GetBytes(_settings.ApiSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(b64));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        private async Task StartUserOrderEvents()
        {
            try
            { 
                var payload = new
                {
                    request = "/v1/order/events",
                    nonce = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 5
                };

                string payloadstring = JsonConvert.SerializeObject(payload);
                var encodedPayload = Encoding.UTF8.GetBytes(payloadstring);
                var b64 = Convert.ToBase64String(encodedPayload);
                var signature = CreateSignature(b64);

                var factory = new Func<ClientWebSocket>(() =>
                {
                    var client = new ClientWebSocket
                    {
                        Options =
                    {
                        KeepAliveInterval = TimeSpan.FromSeconds(30),
                    }
                    };
                    client.Options.SetRequestHeader("X-GEMINI-APIKEY", _settings.ApiKey);
                    client.Options.SetRequestHeader("X-GEMINI-PAYLOAD", b64);
                    client.Options.SetRequestHeader("X-GEMINI-SIGNATURE", signature);

                    return client;
                });

                var exitEvent = new ManualResetEvent(false);
                _userOrderEvents = new WebsocketClient(new Uri(_settings.WebSocketHostName), factory);
                    _userOrderEvents.ReconnectTimeout = TimeSpan.FromSeconds(30);
                _userOrderEvents.ReconnectionHappened.Subscribe(info =>
                {


                });
                _userOrderEvents.DisconnectionHappened.Subscribe(disconnected =>
                {

                });
                _userOrderEvents.MessageReceived.Subscribe(async msg =>
                {
                    string data = msg.ToString(); 
                    HandleUserOrderMessage(data);
                });
                try
                {

                    await _userOrderEvents.Start();

                    log.Info($"Plugin has successfully started.");
                }
                catch (Exception ex)
                {
                    var _error = ex.Message;
                    log.Error(_error, ex);
                } 
            }

            catch (Exception ex)
            {
                var _error = ex.Message;

            }
            CancellationTokenSource source = new CancellationTokenSource();
        }

        public async Task InitializeSnapshotAsync()
        {
            foreach (var symbol in GetAllNonNormalizedSymbols())
            {
                var normalizedSymbol = GetNormalizedSymbol(symbol);
                if (!_localOrderBooks.ContainsKey(normalizedSymbol))
                {
                    _localOrderBooks.Add(normalizedSymbol, null);
                }
                var response = await geminiHttpClient.InitializeSnapshotAsync(symbol);
                if (response != null)
                { 
                    _localOrderBooks[normalizedSymbol] = ToOrderBookModel(response, normalizedSymbol);
                }

            }
        }

        public override async Task StopAsync()
        {
            Status = ePluginStatus.STOPPING;
            log.Info($"{this.Name} is stopping."); 

            if (_ws != null && !_ws.IsRunning)
                await _ws.Stop(WebSocketCloseStatus.NormalClosure, "Manual CLosing");
            //reset models
            RaiseOnDataReceived(new List<VisualHFT.Model.OrderBook>());
            RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.DISCONNECTED));

            await base.StopAsync();
        }

        private VisualHFT.Model.OrderBook ToOrderBookModel(InitialResponse data, string symbol)
        {
            var identifiedPriceDecimalPlaces = RecognizeDecimalPlacesAutomatically(data.asks.Select(x => x.price));

            var lob = new VisualHFT.Model.OrderBook(symbol, identifiedPriceDecimalPlaces, _settings.DepthLevels);
            lob.ProviderID = _settings.Provider.ProviderID;
            lob.ProviderName = _settings.Provider.ProviderName;
            lob.SizeDecimalPlaces = RecognizeDecimalPlacesAutomatically(data.asks.Select(x => x.amount));

            var _asks = new List<VisualHFT.Model.BookItem>();
            var _bids = new List<VisualHFT.Model.BookItem>();
            data.asks.ToList().ForEach(x =>
            {
                _asks.Add(new VisualHFT.Model.BookItem()
                {
                    IsBid = false,
                    Price = (double)x.price,
                    Size = (double)x.amount,
                    LocalTimeStamp = DateTime.Now,
                    ServerTimeStamp = DateTime.Now,
                    Symbol = lob.Symbol,
                    PriceDecimalPlaces = lob.PriceDecimalPlaces,
                    SizeDecimalPlaces = lob.SizeDecimalPlaces,
                    ProviderID = lob.ProviderID,
                });
            });
            data.bids.ToList().ForEach(x =>
            {
                _bids.Add(new VisualHFT.Model.BookItem()
                {
                    IsBid = true,
                    Price = (double)x.price,
                    Size = (double)x.amount,
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

        private void HandleUserOrderMessage(string data)
        {
            string message = data;
            if (!string.IsNullOrEmpty(message) && message.Length > 2)
            {
                JToken token=JToken.Parse(message);
                if(token is JObject)
                {
                    dynamic dataType = JsonConvert.DeserializeObject<dynamic>(message);

                    if (dataType.type == "initial")
                    {
                        ProcessUserOrderData(message);
                    }
                    else if (dataType.type == "subscription_ack")
                    {

                    }
                    else if (dataType.type == "heartbeat")
                    {


                    }
                    else
                    {
                        string ss = message;
                    }
                }
                else if(token is JArray)
                {
                    ProcessUserOrderData(message);
                } 
                
            }
        } 
        private void ProcessUserOrderData(string message)
        {
            List<UserOrderData> _dataType = JsonConvert.DeserializeObject<List<UserOrderData>>(message);

            if (_dataType != null && _dataType.Count > 0)
            {
                foreach (var item in _dataType)
                {

                    VisualHFT.Model.Order localuserOrder;
                    if (!this._localUserOrders.ContainsKey(item.client_order_id))
                    {
                        localuserOrder = new VisualHFT.Model.Order();
                        localuserOrder.ClOrdId = item.client_order_id;
                        localuserOrder.Currency = GetNormalizedSymbol(item.symbol);
                        localuserOrder.OrderID = item.order_id;
                        localuserOrder.ProviderId = _settings!.Provider.ProviderID;
                        localuserOrder.ProviderName = _settings.Provider.ProviderName;
                        localuserOrder.CreationTimeStamp = DateTimeOffset.FromUnixTimeSeconds(item.timestamp).DateTime;
                        localuserOrder.Quantity = item.original_amount;
                        localuserOrder.PricePlaced = item.price;
                        localuserOrder.Symbol = GetNormalizedSymbol(item.symbol);
                        localuserOrder.FilledQuantity = item.executed_amount;
                        localuserOrder.TimeInForce = eORDERTIMEINFORCE.GTC;

                        if (!string.IsNullOrEmpty(item.behavior))
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
                        this._localUserOrders.Add(item.client_order_id, localuserOrder);
                    }
                    else
                    {

                        localuserOrder = this._localUserOrders[item.client_order_id];
                    }
                    localuserOrder.OrderType = eORDERTYPE.LIMIT;

                    localuserOrder.Quantity = item.original_amount;
                    localuserOrder.GetAvgPrice = item.price;
                    if (item.order_type.ToLower().Equals("limit"))
                    {
                        localuserOrder.OrderType = eORDERTYPE.LIMIT;
                    }
                    else if (item.order_type.ToLower().Equals("exchange limit"))
                    {
                        localuserOrder.OrderType = eORDERTYPE.PEGGED;
                    }
                    else if (item.order_type.ToLower().Equals("market buy"))
                    {
                        localuserOrder.OrderType = eORDERTYPE.MARKET;
                    }

                    if (item.side.ToLower().Equals("sell"))
                    {
                        localuserOrder.BestAsk = item.price;
                        localuserOrder.Side = eORDERSIDE.Sell;

                    }
                    else if (item.side.ToLower().Equals("buy"))
                    {
                        localuserOrder.QuoteLocalTimeStamp = DateTime.Now;
                        localuserOrder.QuoteServerTimeStamp = DateTimeOffset.FromUnixTimeSeconds(item.timestamp).DateTime;
                        localuserOrder.PricePlaced = item.price;
                        localuserOrder.BestBid = item.price;
                        localuserOrder.Side = eORDERSIDE.Buy;

                    }

                    if (item.type.ToLower().Equals("accepted"))
                    {
                        localuserOrder.Status = eORDERSTATUS.NEW;
                    }
                    else if (item.type.ToLower().Equals("fill"))
                    {
                        localuserOrder.GetAvgPrice = item.avg_execution_price;
                        localuserOrder.FilledQuantity = item.executed_amount;
                        localuserOrder.Status = eORDERSTATUS.PARTIALFILLED;
                    }
                    else if (item.type.ToLower().Equals("closed"))
                    {
                        localuserOrder.Status = eORDERSTATUS.FILLED;
                        if (item.is_cancelled)
                        {
                            localuserOrder.Status = eORDERSTATUS.CANCELED;

                        }
                    }
                    else if (item.type.ToLower().Equals("rejected"))
                    {
                        localuserOrder.Status = eORDERSTATUS.REJECTED;
                    }
                    else if (item.type.ToLower().Equals("cancelled"))
                    {
                        localuserOrder.Status = eORDERSTATUS.CANCELED;
                    }
                    else if (item.type.ToLower().Equals("cancel_rejected"))
                    {
                        localuserOrder.Status = eORDERSTATUS.CANCELED; 
                    }
                    if (!string.IsNullOrEmpty(item.behavior))
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
                    localuserOrder.LastUpdated = DateTime.Now;
                    localuserOrder.FilledPercentage = Math.Round((100 / localuserOrder.Quantity) * localuserOrder.FilledQuantity, 2);
                    RaiseOnDataReceived(localuserOrder);

                }
            }
        }
        private void HandleMessage(string marketData,DateTime serverTime)
        {
            string message = marketData; 
            dynamic type=JsonConvert.DeserializeObject<dynamic>(message);
            if (type.type == "l2_updates")
            {
                List<OrderBook> events = new List<OrderBook>();
                GeminiResponseInitial dataReceived = _parser.Parse<GeminiResponseInitial>(message);
                string symbol = GetNormalizedSymbol(dataReceived.symbol);

                if (!_localOrderBooks.ContainsKey(symbol))
                    return;

                var local_lob = _localOrderBooks[symbol];

                if (local_lob == null)
                {
                    local_lob = new OrderBook();
                }
                OrderBook book = new VisualHFT.Model.OrderBook();
                book.ProviderID = _settings.Provider.ProviderID;
                book.ProviderName = _settings.Provider.ProviderName;
                book.Symbol = symbol;


                foreach (var item in dataReceived.changes)
                {
                    if (item[0].ToLower().Equals("buy"))
                    {
                        BookItem bookItem = new BookItem();
                        bookItem.Symbol = symbol;
                        bookItem.ProviderID = _settings.Provider.ProviderID;

                        bookItem.IsBid = false;
                        bookItem.Price = double.Parse(item[1]);
                        bookItem.Size =double.Parse(item[2]);
                        bookItem.LocalTimeStamp = DateTime.Now;
                        bookItem.ServerTimeStamp = serverTime;
                        book.Asks.Add(bookItem);

                        local_lob.AddOrUpdateLevel(new DeltaBookItem()
                        {
                            MDUpdateAction = eMDUpdateAction.None,
                            Price = bookItem.Price,
                            Size = bookItem.Size,
                            IsBid = true,
                            LocalTimeStamp = DateTime.Now,
                            ServerTimeStamp = bookItem.ServerTimeStamp,
                            Symbol = symbol
                        });
                    }
                    else if (item[0].ToLower().Equals("sell"))
                    {
                        book.Symbol = symbol;
                        BookItem bookItem = new BookItem();
                        bookItem.Symbol = symbol;
                        bookItem.IsBid = true;
                        bookItem.Price = double.Parse(item[1]);
                        bookItem.Size = double.Parse(item[2]);
                        bookItem.LocalTimeStamp = DateTime.Now;
                        bookItem.ServerTimeStamp = serverTime;
                        bookItem.ProviderID = _settings.Provider.ProviderID;
                        book.Bids.Add(bookItem);

                        local_lob.AddOrUpdateLevel(new DeltaBookItem()
                        {
                            MDUpdateAction = eMDUpdateAction.None,
                            Price = bookItem.Price,
                            Size = bookItem.Size,
                            IsBid = false,
                            LocalTimeStamp = DateTime.Now,
                            ServerTimeStamp = bookItem.ServerTimeStamp,
                            Symbol = symbol
                        });
                    }
                }

                //marketDataEvent.ParsedModel = _parser.Parse<IEnumerable<OrderBook>>(dataReceived.events, _parser_settings);
                var identifiedPriceDecimalPlaces = RecognizeDecimalPlacesAutomatically(book.Asks.Where(x => x.Price.HasValue).Select(x => x.Price.Value));
                var identifiedSizeDecimalPlaces = RecognizeDecimalPlacesAutomatically(book.Asks.Where(x => x.Size.HasValue).Select(x => x.Size.Value));
                book.SizeDecimalPlaces = identifiedSizeDecimalPlaces;
                book.PriceDecimalPlaces = identifiedPriceDecimalPlaces;
                foreach (var bookItem in book.Asks)
                {
                    bookItem.SizeDecimalPlaces = identifiedSizeDecimalPlaces;
                    bookItem.PriceDecimalPlaces = identifiedPriceDecimalPlaces;
                }
                foreach (var bookItem in book.Bids)
                {
                    bookItem.SizeDecimalPlaces = identifiedSizeDecimalPlaces;
                    bookItem.PriceDecimalPlaces = identifiedPriceDecimalPlaces;
                }
                events.Add(book);

                marketDataEvent.ParsedModel = events;
                RaiseOnDataReceived(local_lob);

                if (dataReceived.trades != null && dataReceived.trades.Count > 0)
                {
                    List<Trade> trades = new List<VisualHFT.Model.Trade>();
                    foreach (var item in dataReceived.trades)
                    {
                        Trade trade = new Trade();
                        trade.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(item.timestamp).DateTime;
                        trade.Price = item.price;
                        trade.Size = item.quantity;
                        trade.Symbol = symbol;
                        trade.ProviderId = _settings.Provider.ProviderID;
                        trade.ProviderName = _settings.Provider.ProviderName;
                        if (item.side.ToLower().Equals("buy"))
                        {
                            trade.IsBuy = true;
                        }
                        else if(item.side.ToLower().Equals("sell"))
                        {
                            trade.IsBuy = false;
                        }
                        trades.Add(trade);
                    }
                    tradeDataEvent.ParsedModel = trades;
                    RaiseOnDataReceived(tradeDataEvent);
                }
            }
            else if (type.type == "heartbeat")
            {
                RaiseOnDataReceived(heartbeatDataEvent);

            }
            else if (type.type == "trade")
            {
                List<Trade> trades = new List<VisualHFT.Model.Trade>();

                var item = JsonConvert.DeserializeObject<GeminiResponseTrade>(message);

                string symbol = GetNormalizedSymbol(item.symbol);
                Trade trade = new Trade();
                trade.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(item.timestamp).DateTime;
                trade.Price = item.price;
                trade.Size = item.quantity;
                trade.Symbol = symbol;
                trade.ProviderId = _settings.Provider.ProviderID;
                trade.ProviderName = _settings.Provider.ProviderName;
                if (item.side.ToLower().Equals("buy"))
                {
                    trade.IsBuy = true;
                }
                trades.Add(trade);
                tradeDataEvent.ParsedModel = trades;
                RaiseOnDataReceived(tradeDataEvent);
            }
        }
        private void CheckConnectionStatus(object state)
        {
            bool isConnected = _ws != null && _ws.IsRunning;
            if (isConnected)
            {
                RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.CONNECTED));
            }
            else
            {
                RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.DISCONNECTED));

            }
        }



        public override object GetUISettings()
        {
            PluginSettingsView view = new PluginSettingsView();
            PluginSettingsViewModel viewModel = new PluginSettingsViewModel(CloseSettingWindow);
            viewModel.ApiSecret = _settings.ApiSecret;
            viewModel.ApiKey = _settings.ApiKey; 
            viewModel.ProviderId = _settings.Provider.ProviderID;
            viewModel.ProviderName = _settings.Provider.ProviderName;
            viewModel.Symbols = _settings.Symbols;

            viewModel.UpdateSettingsFromUI = () =>
            {
                _settings.ApiSecret = viewModel.ApiSecret;
                _settings.ApiKey = viewModel.ApiKey; 
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
        protected override void InitializeDefaultSettings()
        {  /*
             *   //https://api.gemini.com/v1/book/:symbol
            //wss://api.gemini.com/v1/marketdata?heartbeat=true
             * 
             */

            _settings = new PlugInSettings()
            {
                ApiKey = "",
                ApiSecret = "",
                HostName = "https://api.gemini.com/v1/book/",
                WebSocketHostName= "wss://api.gemini.com/v2/marketdata?heartbeat=true",
                Provider = new VisualHFT.Model.Provider() { ProviderID = 5, ProviderName = "Gemini" },
                Symbols = new List<string>() { "BTCUSD(BTC/USD)", "ETHUSD(ETH/USD)" } // Add more symbols as needed
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
                _settings.Provider = new VisualHFT.Model.Provider() { ProviderID = 2, ProviderName = "Bitfinex" };
            }
            ParseSymbols(string.Join(',', _settings.Symbols.ToArray())); //Utilize normalization function
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