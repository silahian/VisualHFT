using demoTradingCore.Models;
using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using WatsonWebsocket;


namespace demoTradingCore
{



    internal class Program
    {

        static Dictionary<eEXCHANGE, Exchange> _EXCHANGES = new Dictionary<eEXCHANGE, Exchange>();
        static List<string> _SYMBOLS = new List<string>() { "BTC-USD" };
        static Dictionary<string, string> _SYMBOLS_EXCH_TO_NORMALIZED = new Dictionary<string, string>();
        static WatsonWsServer _SERVER_WS;
        static IEnumerable<string> allWSClients = null;
        static Strategy _STRATEGY = null;

        static async Task Main(string[] args)
        {
            await InitializeWS();
            await InitializeBinance();
            await InitializeCoinbase();
            await InitializeStrategy();

            Console.WriteLine("\n\nPress ENTER to shutdown.");
            Console.ReadLine();
        }

        static async Task InitializeWS()
        {
            Console.Write("Initializing Websocket server...");
            _SERVER_WS = new WatsonWsServer("localhost", 6900, false);
            _SERVER_WS.ClientConnected += ClientConnected;
            _SERVER_WS.ClientDisconnected += ClientDisconnected;
            _SERVER_WS.MessageReceived += MessageReceived;
            await _SERVER_WS.StartAsync();
            Console.WriteLine("OK");
        }
        static async Task InitializeBinance()
        {
            Console.Write("Initializing Binance...");
            _EXCHANGES.Add(eEXCHANGE.BINANCE, new Exchange(eEXCHANGE.BINANCE, 5));

            var exchangeAPI = await ExchangeAPI.GetExchangeAPIAsync<ExchangeBinanceUSAPI>();
            var lstNormalized = new List<string>();
            foreach (var symbol in _SYMBOLS)
            {
                var norm = await exchangeAPI.GlobalMarketSymbolToExchangeMarketSymbolAsync(symbol);
                lstNormalized.Add(norm);
                if (!_SYMBOLS_EXCH_TO_NORMALIZED.ContainsKey(norm))
                    _SYMBOLS_EXCH_TO_NORMALIZED.Add(norm, symbol);
            }
            await exchangeAPI.GetFullOrderBookWebSocketAsync(book => 
                { 
                    book.MarketSymbol = _SYMBOLS_EXCH_TO_NORMALIZED[book.MarketSymbol];
                    SnapshotUpdates(eEXCHANGE.BINANCE, book, 5);                 
                }, 5, lstNormalized.ToArray());
            Console.WriteLine("OK");
        }
        static async Task InitializeCoinbase()
        {
            Console.Write("Initializing Coinbase...");
            _EXCHANGES.Add(eEXCHANGE.COINBASE, new Exchange(eEXCHANGE.COINBASE, 5));

            var exchangeAPI = await ExchangeAPI.GetExchangeAPIAsync<ExchangeCoinbaseAPI>();
            var lstNormalized = new List<string>();
            foreach (var symbol in _SYMBOLS)
            {
                var norm = await exchangeAPI.GlobalMarketSymbolToExchangeMarketSymbolAsync(symbol);
                lstNormalized.Add(norm);
                if (!_SYMBOLS_EXCH_TO_NORMALIZED.ContainsKey(norm))
                    _SYMBOLS_EXCH_TO_NORMALIZED.Add(norm, symbol);
            }
            await exchangeAPI.GetFullOrderBookWebSocketAsync(book => 
                {
                    book.MarketSymbol = _SYMBOLS_EXCH_TO_NORMALIZED[book.MarketSymbol];
                    SnapshotUpdates(eEXCHANGE.COINBASE, book, 5); 
                }, 5, lstNormalized.ToArray());
            Console.WriteLine("OK");
        }
        static async Task InitializeStrategy()
        {
            Console.Write("Initializing Strategy...");
            _STRATEGY = new Strategy(_EXCHANGES.Select(x => x.Value).ToList(), _SYMBOLS.First());
            _STRATEGY.OnStrategyExposure += _STRATEGY_OnStrategyExposure; ;
            Console.WriteLine("OK");
        }

        private static void _STRATEGY_OnStrategyExposure(object sender, StrategyExposureEventArgs e)
        {
            //send heart beat with all strategies:
            // in this demo we only have one strategy running
            Json_Exposure json_Exp = new Json_Exposure() { StrategyName = _STRATEGY.GetStrategyName(), SizeExposed = e.SizeExposed, Symbol = e.Symbol, UnrealizedPL = e.UnrealizedPL };
            JsonExposures toSendExposure = new JsonExposures();
            toSendExposure.dataObj = new List<Json_Exposure>() { json_Exp };

            Send_toWS(toSendExposure);

        }


        static void SnapshotUpdates(eEXCHANGE exchange, ExchangeOrderBook ob, int depth)
        {            
            if (!_EXCHANGES.ContainsKey(exchange))
                _EXCHANGES.Add(exchange, new Exchange(exchange, depth));
            _EXCHANGES[exchange].UpdateSnapshot(ob, depth);
            _STRATEGY.UpdateSnapshot(ob);

            jsonMarkets toSend = new jsonMarkets();
            toSend.type = "Market";
            toSend.dataObj = _EXCHANGES[exchange].GetSnapshots().dataObj;
            SendMarketData_toWS(toSend);

            //send heart beat with all strategies:
            // in this demo we only have one strategy running
            Json_Strategy json_Strategy = new Json_Strategy() { StrategyCode = _STRATEGY.GetStrategyName() };
            jsonStrategies toSendStrategy = new jsonStrategies();
            toSendStrategy.dataObj = new List<Json_Strategy>() { json_Strategy };
            Send_toWS(toSendStrategy);
        }

        static void SendMarketData_toWS(jsonMarkets toSend)
        {            
            if (allWSClients == null || !allWSClients.Any())
                return;
            var msg = Newtonsoft.Json.JsonConvert.SerializeObject(toSend);
            foreach (var cli in allWSClients)
            {
                bool result = _SERVER_WS.SendAsync(cli, msg).Result;
            }
        }
        static void Send_toWS(Json_BaseData toSend)
        {
            if (allWSClients == null || !allWSClients.Any())
                return;
            var msg = Newtonsoft.Json.JsonConvert.SerializeObject(toSend);
            foreach (var cli in allWSClients)
            {
                bool result = _SERVER_WS.SendAsync(cli, msg).Result;
            }
        }

        #region webserver callbacks
        static void ClientConnected(object sender, ClientConnectedEventArgs args)
        {
            Console.WriteLine("Client connected: " + args.IpPort);
            allWSClients = _SERVER_WS.ListClients();            
        }

        static void ClientDisconnected(object sender, ClientDisconnectedEventArgs args)
        {
            Console.WriteLine("Client disconnected: " + args.IpPort);
        }

        static void MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            Console.WriteLine("Message received from " + args.IpPort + ": " + Encoding.UTF8.GetString(args.Data.ToArray()));
        }
        #endregion
    }
}
