
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualHFT.DataRetriever.DataParsers;
using VisualHFT.Helpers;
using VisualHFT.Model;
using WebSocket4Net;

namespace VisualHFT.DataRetriever
{
    public class WebsocketData
    {
        public string type { get; set; }
        public string data { get; set; }
    }

    public class WebSocketDataRetriever : IDataRetriever
    {
        private IDataParser _parser;
        private WebSocket _webSocket;
        private string WEBSOCKET_URL = System.Configuration.ConfigurationManager.AppSettings["WSorderBook"];
        private const int INITIAL_DELAY = 5000;  // Initial delay of 5 seconds
        private const int MAX_DELAY = 30000;  // Max delay of 30 seconds

        public event EventHandler<DataEventArgs> OnDataReceived;
        JsonSerializerSettings settings = null;

        public WebSocketDataRetriever(IDataParser parser)
        {
            _parser = parser;
            settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new CustomDateConverter() },
                DateParseHandling = DateParseHandling.None,
                DateFormatString = "yyyy.MM.dd-HH.mm.ss.ffffff"
            };
            InitializeWebSocket();
        }

        private void InitializeWebSocket()
        {
            _webSocket = new WebSocket(WEBSOCKET_URL);
            _webSocket.Opened += WebSocket_Opened;
            _webSocket.Closed += WebSocket_Closed;
            _webSocket.Error += WebSocket_Error;
            _webSocket.MessageReceived += WebSocket_MessageReceived;
        }

        public void Start()
        {
            _webSocket.Open();
        }

        private void WebSocket_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("WebSocket connection opened.");
        }

        private void WebSocket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("WebSocket connection closed. Attempting to reconnect...");
            HandleReconnection();
        }

        private void WebSocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine($"WebSocket error: {e.Exception.Message}");
            HandleReconnection();
        }

        private void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var message = e.Message;
            var dataReceived = _parser.Parse<WebsocketData>(message);
            string dataType = dataReceived.type;
            object modelObj = null;

            // Determine the type of data received (e.g., market data, providers, active orders, etc.)
            if (dataType == "Market")
            {
                modelObj = _parser.Parse<IEnumerable<OrderBook>>(dataReceived.data, settings);
            }
            else if (dataType == "ActiveOrders")
            {
                modelObj = _parser.Parse<List<Model.OrderVM>>(dataReceived.data, settings);
            }
            else if (dataType == "Strategies")
            {
                modelObj = _parser.Parse<List<StrategyVM>>(dataReceived.data, settings);
            }
            else if (dataType == "Exposures")
            {
                modelObj = _parser.Parse<List<Exposure>>(dataReceived.data, settings);
            }
            else if (dataType == "HeartBeats")
            {
                modelObj = _parser.Parse<List<ProviderEx>>(dataReceived.data, settings);
            }
            else if (dataType == "Trades")
            {
                modelObj = _parser.Parse<List<Trade>>(dataReceived.data, settings);
            }
            else
            {
                Console.WriteLine("Websocket data retriever :" + dataType + " error: NOT RECOGNIZED.");
            }


            OnDataReceived?.Invoke(this, new DataEventArgs { DataType = dataType, RawData = message, ParsedModel = modelObj });
        }

        private async void HandleReconnection()
        {
            int delay = INITIAL_DELAY;
            while (true)
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    return;
                }

                Console.WriteLine("Attempting to reconnect...");
                try
                {
                    _webSocket.Open();
                    await Task.Delay(MAX_DELAY); // Give it some time to attempt the connection
                }
                catch
                {
                    Console.WriteLine("Failed to reconnect. Retrying...");
                    await Task.Delay(delay);
                    delay = Math.Min(delay * 2, MAX_DELAY);
                }
            }
        }

        public void Stop()
        {
            _webSocket.Close();
        }
    }
}

