using VisualHFT.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WebSocket4Net;
using System.Windows.Threading;
using System.Windows;

namespace VisualHFT.Helpers
{
    public class HelperWebsocketData
    {
        public string type { get; set; }
        public string data { get; set; }
    }
    public class HelperWebsocket
    {

        public event EventHandler<IEnumerable<Model.OrderBook>> OnDataReceived;
        protected virtual void RaiseOnDataReceived(IEnumerable<Model.OrderBook> orderBooks)
        {
            EventHandler<IEnumerable<Model.OrderBook>> _handler = OnDataReceived;
            if (_handler != null)
            {
                _handler(this, orderBooks);
            }
        }
        protected WebSocket _clientWS;
        protected string _webSocketUrl = "";
        protected bool _isChannelOpen;
        protected int _ChannelOpenTimeout = 5000; //5s
        
        protected Queue<string> _QUEUE = new Queue<string>();
        protected object LOCK_QUEUE = new object();
        protected object _LOCK_SYMBOLS = new object();

        ~HelperWebsocket()
        {
            Disconnect();
            if (_clientWS != null)
                _clientWS.Dispose();
        }
        public HelperWebsocket()
        {
            _webSocketUrl = System.Configuration.ConfigurationManager.AppSettings["WSorderBook"];
            //launch processing QUEUE thread
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                while(true)
                {
                    ProcessQueue();
                    Thread.Sleep(5);
                }
            }).Start();
        }
        public void Connect()
        {
            if (_isChannelOpen)
                return;
            if (string.IsNullOrEmpty(_webSocketUrl))
                throw new Exception("_webSocketUrl cannot be blank");
            try
            {
                _clientWS = new WebSocket(_webSocketUrl, "", WebSocketVersion.None);
                _clientWS.Opened += _clientWS_Opened;
                _clientWS.Error += _clientWS_Error;
                _clientWS.Closed += _clientWS_Closed;
                _clientWS.MessageReceived += _clientWS_MessageReceived;
                /*WAIT until open*/
                TryReconnect();
            }
            catch (Exception ex) {  }
        }

        protected void Disconnect()
        {
            if (_clientWS != null && !_isChannelOpen)
                _clientWS.Close();
        }
        protected void SendMessage(string msg)
        {
            _clientWS.Send(msg);
        }
        protected virtual void _clientWS_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            InsertMessageIntoQueue(e.Message);
        }

        protected virtual void _clientWS_Closed(object sender, EventArgs e)
        {
            _isChannelOpen = false;
            TryReconnect();
        }
        protected virtual void _clientWS_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            var socketException = e.Exception as System.Net.Sockets.SocketException;
            if (socketException != null && (socketException.NativeErrorCode == 10061 || socketException.NativeErrorCode == 10060))
            {                
                System.Threading.Thread.Sleep(30 * 1000);
                return;
            }
            throw e.Exception;
            //throw new NotImplementedException();
        }
        private void _clientWS_Opened(object sender, EventArgs e)
        {
            _isChannelOpen = true;
        }
        private void TryReconnect()
        {
            while(_isChannelOpen == false)
            {
                try
                {
                    if (_clientWS.State == WebSocketState.Closed || _clientWS.State == WebSocketState.None)
                        _clientWS.Open();                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                System.Threading.Thread.Sleep(5 * 1000);
            }
        }



        #region Queues
        private void InsertMessageIntoQueue(string message)
        {
            if (!string.IsNullOrEmpty(message))
                lock (LOCK_QUEUE)
                    _QUEUE.Enqueue(message);
        }
        private void ProcessQueue()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new CustomDateConverter() },
                DateParseHandling = DateParseHandling.None,
                DateFormatString = "yyyy.MM.dd-HH.mm.ss.ffffff"
            };
            string message = "";
            HelperWebsocketData dataReceived = null;

            lock (LOCK_QUEUE)
            {
                while (_QUEUE.Any())
                {
                    message = _QUEUE.Dequeue();
                    try
                    {
                        if (!string.IsNullOrEmpty(message))
                        {
                            if (_QUEUE.Count > 100)
                            {
                                Console.WriteLine("WS QUEUE is way behind: " + _QUEUE.Count);
                            }
                            dataReceived = Newtonsoft.Json.JsonConvert.DeserializeObject<HelperWebsocketData>(message);
                            if (dataReceived != null && !string.IsNullOrEmpty(dataReceived.data))
                            {
                                if (dataReceived.type == "Market")
                                {
                                    var orderBook = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<OrderBook>>(dataReceived.data, settings);
                                    if (orderBook != null && orderBook.Any())
                                    {
                                        var allProviders = orderBook.Select(x => new ProviderVM()
                                        {
                                            ProviderID = x.ProviderID,
                                            ProviderName = x.ProviderName,
                                            Status = x.ProviderStatus
                                        });
                                        var allSymbols = orderBook.Select(x => x.Symbol);

                                        ParseSymbols(allSymbols.Distinct());
                                        ParseProviders(allProviders);
                                        ParseOrderBook(orderBook);
                                    }

                                }
                                else if (dataReceived.type == "ActiveOrders")
                                {
                                    var activeOrders = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Model.OrderVM>>(dataReceived.data, settings);
                                    ParseActiveOrders(activeOrders);
                                }
                                else if (dataReceived.type == "Strategies")
                                {
                                    var activeStrategies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StrategyVM>>(dataReceived.data, settings);
                                    ParseActiveStrategies(activeStrategies);
                                }
                                else if (dataReceived.type == "Exposures")
                                {
                                    var exposures = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Exposure>>(dataReceived.data, settings);
                                    ParseExposures(exposures);
                                }
                                else if (dataReceived.type == "HeartBeats")
                                {
                                    var heartbeats = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProviderVM>>(dataReceived.data, settings);
                                    ParseHeartBeat(heartbeats);
                                }
                                else
                                {
                                    Console.WriteLine(dataReceived.type + " error: NOT RECOGNIZED.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(dataReceived.type + " error: " + ex.ToString());
                    }
                }
            }

        }
        #endregion
        #region Parsing Methods        
        private void ParseSymbols(IEnumerable<string> symbols)
        {
            lock (_LOCK_SYMBOLS)
            {
                if (HelperCommon.ALLSYMBOLS == null)
                    HelperCommon.ALLSYMBOLS = new System.Collections.ObjectModel.ObservableCollection<string>();
                if (Application.Current == null)
                    return;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    foreach (var s in symbols)
                    {
                        if (!HelperCommon.ALLSYMBOLS.Contains(s))
                        {
                            HelperCommon.ALLSYMBOLS.Add(s);
                        }
                    }
                }));
            }
        }
        private void ParseProviders(IEnumerable<ProviderVM> providers)
        {
            HelperCommon.PROVIDERS.UpdateData(providers);
        }
        private void ParseOrderBook(IEnumerable<OrderBook> orderBooks)
        {
            HelperCommon.LIMITORDERBOOK.UpdateData(orderBooks);
        }
        private void ParsePositions(IEnumerable<PositionEx> positions)
        {
            if (HelperCommon.CLOSEDPOSITIONS.LoadingType == ePOSITION_LOADING_TYPE.WEBSOCKETS)
                HelperCommon.CLOSEDPOSITIONS.LoadNewPositions(positions.ToList());
        }
        private void ParseExposures(IEnumerable<Exposure> exposures)
        {
            HelperCommon.EXPOSURES.UpdateData(exposures);
        }
        private void ParseActiveOrders(IEnumerable<OrderVM> activeOrders)
        {
            HelperCommon.ACTIVEORDERS.UpdateData(activeOrders.ToList());
        }
        private void ParseActiveStrategies(IEnumerable<StrategyVM> activeStrategies)
        {
            HelperCommon.ACTIVESTRATEGIES.UpdateData(activeStrategies.ToList());
        }
        private void ParseStrategyParams(string data)
        {
            HelperCommon.STRATEGYPARAMS.RaiseOnDataUpdateReceived(data);

        }
        private void ParseHeartBeat(IEnumerable<ProviderVM> providers)
        {
            HelperCommon.PROVIDERS.UpdateData(providers.ToList());
        }
        #endregion
    }
}
