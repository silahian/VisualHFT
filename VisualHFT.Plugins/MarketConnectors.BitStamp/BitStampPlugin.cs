using BitStamp.Net.Clients;
using BitStamp.Net.Models;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using MarketConnectors.BitStamp;
using MarketConnectors.BitStamp.Model;
using MarketConnectors.BitStamp.UserControls;
using MarketConnectors.BitStamp.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Media.Converters;
using VisualHFT.Commons.Model;
using VisualHFT.Commons.PluginManager;
using VisualHFT.DataRetriever;
using VisualHFT.DataRetriever.DataParsers;
using VisualHFT.Enums;
using VisualHFT.Model;
using VisualHFT.PluginManager;
using VisualHFT.UserSettings;
using Websocket.Client;
using OrderBook = VisualHFT.Model.OrderBook;
using Trade = VisualHFT.Model.Trade;

namespace MarketConnectors.Gemini
{
    public class BitStampPlugin : BasePluginDataRetriever
    { 
        private new bool _disposed = false; // to track whether the object has been disposed
        

        private Timer _heartbeatTimer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, VisualHFT.Model.OrderBook> _localOrderBooks = new Dictionary<string, VisualHFT.Model.OrderBook>();

        private DataEventArgs tradeDataEvent = new DataEventArgs() { DataType = "Trades" }; //reusable object. So we avoid allocations
        private DataEventArgs marketDataEvent = new DataEventArgs() { DataType = "Market" };//reusable object. So we avoid allocations
        private DataEventArgs heartbeatDataEvent = new DataEventArgs() { DataType = "HeartBeats" };//reusable object. So we avoid allocations



        private PlugInSettings? _settings;
        public override string Name { get; set; } = "BitStamp";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Connects to BitStamp.";
        public override string Author { get ; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action? CloseSettingWindow { get; set; }

        private IDataParser _parser;
        JsonSerializerSettings? _parser_settings = null;
        WebsocketClient? _ws;

        BitStampHttpClient bitstampHttpClient;
        public BitStampPlugin()
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
            bitstampHttpClient = new BitStampHttpClient(); 
        }

        ~BitStampPlugin() 
        {
            Dispose(false);
        }


        public override async Task StartAsync()
        {
            await base.StartAsync();//call the base first 

            await InitializeSnapshotAsync();

            var symbols = GetAllNonNormalizedSymbols(); 
            List<string> channelsToSubscribe = new List<string>();

            foreach (var symbol in symbols)
            {
                
                channelsToSubscribe.Add("order_book_" + symbol);
                channelsToSubscribe.Add("live_trades_" + symbol);
            }  
            try
            { 
                await Task.Run(async () =>
                {
                    var exitEvent = new ManualResetEvent(false);
                    var url = new Uri("wss://ws.bitstamp.net");
                    using (_ws = new WebsocketClient(url))
                    { 
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
                            if(data.Contains("trade"))
                            {

                            }
                            HandleMessage(data,DateTime.Now);
                            RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.CONNECTED));
                        });
                        try
                        {
                            await _ws.Start();
                            log.Info($"Plugin has successfully started.");
                            RaiseOnDataReceived(GetProviderModel(eSESSIONSTATUS.CONNECTED));
                            foreach (var tosubscribe in channelsToSubscribe)
                            {
                                BitStampSubscriptions bitStampSubscriptions = new BitStampSubscriptions();
                                bitStampSubscriptions.data = new Data();
                                bitStampSubscriptions.data.channel = tosubscribe;                                
                                string jsonToSubscribe = JsonConvert.SerializeObject(bitStampSubscriptions);
                                _ws.Send(jsonToSubscribe);
                                Thread.Sleep(1000);
                            }
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

        public async Task InitializeSnapshotAsync()
        {
            foreach (var symbol in GetAllNonNormalizedSymbols())
            {
                var normalizedSymbol = GetNormalizedSymbol(symbol);
                if (!_localOrderBooks.ContainsKey(normalizedSymbol))
                {
                    _localOrderBooks.Add(normalizedSymbol, null);
                }
                var response = await bitstampHttpClient.InitializeSnapshotAsync(symbol);
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
            var identifiedPriceDecimalPlaces = RecognizeDecimalPlacesAutomatically(data.asks.Select(x => double.Parse(x[0])));

            var lob = new VisualHFT.Model.OrderBook(symbol, identifiedPriceDecimalPlaces, _settings.DepthLevels);
            lob.ProviderID = _settings.Provider.ProviderID;
            lob.ProviderName = _settings.Provider.ProviderName;
            lob.SizeDecimalPlaces = RecognizeDecimalPlacesAutomatically(data.asks.Select(x => double.Parse(x[1])));

            var _asks = new List<VisualHFT.Model.BookItem>();
            var _bids = new List<VisualHFT.Model.BookItem>();
            data.asks.ToList().ForEach(x =>
            {
                _asks.Add(new VisualHFT.Model.BookItem()
                {
                    IsBid = false,
                    Price = double.Parse(x[0]),
                    Size = double.Parse(x[1]),
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
                    Price = double.Parse(x[0]),
                    Size = double.Parse(x[1]),
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

        private void HandleMessage(string marketData, DateTime serverTime)
        {
            string message = marketData;


            dynamic data = JsonConvert.DeserializeObject<dynamic>(message);
            if (data.@event=="data")
            {
                BitStampOrderBook type = JsonConvert.DeserializeObject<BitStampOrderBook>(message);
                if (type.@event.ToLower().Equals("data"))
                {
                    List<OrderBook> events = new List<OrderBook>();

                    string symbol=string.Empty;
                    if(type.channel.Split('_').Length>3)
                    {
                        symbol=GetNormalizedSymbol(type.channel.Split('_')[3]);
                    }
                    else if (type.channel.Split('_').Length == 3)
                    {
                        symbol = GetNormalizedSymbol(type.channel.Split('_')[2]);
                    }

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
                    foreach (var item in type.data.bids)
                    {
                        BookItem bookItem = new BookItem();
                        bookItem.Symbol = symbol;
                        bookItem.ProviderID = _settings.Provider.ProviderID;
                        bookItem.IsBid = true;
                        bookItem.Price = double.Parse(item[0]);
                        bookItem.Size = double.Parse(item[1]);
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
                    foreach (var item in type.data.asks)
                    {
                        book.Symbol = symbol;
                        BookItem bookItem = new BookItem();
                        bookItem.Symbol = symbol;
                        bookItem.IsBid = false;
                        bookItem.Price = double.Parse(item[0]);
                        bookItem.Size = double.Parse(item[1]);
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

                    //if (dataReceived.trades != null && dataReceived.trades.Count > 0)
                    //{
                    //    List<Trade> trades = new List<VisualHFT.Model.Trade>();
                    //    foreach (var item in dataReceived.trades)
                    //    {
                    //        Trade trade = new Trade();
                    //        trade.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(item.timestamp).DateTime;
                    //        trade.Price = item.price;
                    //        trade.Size = item.quantity;
                    //        trade.Symbol = symbol;
                    //        trade.ProviderId = _settings.Provider.ProviderID;
                    //        trade.ProviderName = _settings.Provider.ProviderName;
                    //        if (item.side.ToLower().Equals("buy"))
                    //        {
                    //            trade.IsBuy = true;
                    //        }
                    //        else if(item.side.ToLower().Equals("sell"))
                    //        {
                    //            trade.IsBuy = false;
                    //        }
                    //        trades.Add(trade);
                    //    }
                    //    tradeDataEvent.ParsedModel = trades;
                    //    RaiseOnDataReceived(tradeDataEvent);
                    //}
                }
            }
            else if (data.@event=="heartbeat")
            {
                RaiseOnDataReceived(heartbeatDataEvent);

            }
            else if (data.@event=="trade")
            {
                
                BitStampTrade type = JsonConvert.DeserializeObject<BitStampTrade>(message);
                if (type != null && type.data != null)
                {
                    List<Trade> trades = new List<Trade>();
                    string symbol = string.Empty; 
                    if (type.channel.Split('_').Length > 3)
                    {
                        symbol = GetNormalizedSymbol(type.channel.Split('_')[3]);
                    }
                    else if (type.channel.Split('_').Length == 3)
                    {
                        symbol = GetNormalizedSymbol(type.channel.Split('_')[2]);
                    }
                    Trade trade = new Trade();
                    trade.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(type.data.timestamp).DateTime;
                    trade.Price = type.data.price;
                    trade.Size = type.data.amount;
                    trade.Symbol = symbol;
                    trade.ProviderId = _settings.Provider.ProviderID;
                    trade.ProviderName = _settings.Provider.ProviderName;
                    if (type.data.type==0) //0 means buy
                    {
                        trade.IsBuy = true;
                    }
                    else if(type.data.type==1) //1 means sell
                    {
                        trade.IsBuy = false;
                    }
                    trades.Add(trade);
                    tradeDataEvent.ParsedModel = trades;
                }
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
            viewModel.APIPassPhrase = _settings.APIPassPhrase;
            viewModel.ProviderId = _settings.Provider.ProviderID;
            viewModel.ProviderName = _settings.Provider.ProviderName;
            viewModel.Symbols = _settings.Symbols;

            viewModel.UpdateSettingsFromUI = () =>
            {
                _settings.ApiSecret = viewModel.ApiSecret;
                _settings.ApiKey = viewModel.ApiKey;
                _settings.APIPassPhrase = viewModel.APIPassPhrase;
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
                Provider = new VisualHFT.Model.Provider() { ProviderID = 6, ProviderName = "BitStamp" },
                Symbols = new List<string>() { "btcusd(BTC/USD)", "ethusd(ETH/USD)" } // Add more symbols as needed
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
                _settings.Provider = new VisualHFT.Model.Provider() { ProviderID = 6, ProviderName = "BitStamp" };
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