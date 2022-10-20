using demoTradingCore.Models;
using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WatsonWebsocket;

namespace demoTradingCore
{



    internal class Program
    {

        static Dictionary<eEXCHANGE, Exchange> _EXCHANGES = new Dictionary<eEXCHANGE, Exchange>();
        static List<string> _SYMBOLS = new List<string>() { "BTC-USD" };
        static Dictionary<string, string> _SYMBOLS_EXCH_TO_NORMALIZED = new Dictionary<string, string>();
        static WatsonWsServer _SERVER_WS;
        static System.Timers.Timer _TIMER = new System.Timers.Timer();

        static async Task Main(string[] args)
        {
            await InitializeWS();
            await InitializeBinance();
            await InitializeCoinbase();

            _TIMER.Elapsed += _TIMER_Elapsed;
            _TIMER.Interval = 500;            
            _TIMER.Enabled = true;

            Console.WriteLine("\n\nPress ENTER to shutdown.");
            Console.ReadLine();
        }

        private static void _TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _TIMER.Stop();
            //foreach symbol
            //foreach provider
            jsonMarkets toSend = new jsonMarkets();
            toSend.type = "Market";
            toSend.dataObj = new List<jsonMarket>();

            foreach(var symbol in _SYMBOLS)
            {
                foreach(var exch in _EXCHANGES)
                {
                    toSend.dataObj.AddRange(exch.Value.GetSnapshots().dataObj);
                }
            }
            SendMarketData_toWS(toSend);

            _TIMER.Start();
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


        static void SnapshotUpdates(eEXCHANGE exchange, ExchangeOrderBook ob, int depth)
        {            
            if (!_EXCHANGES.ContainsKey(exchange))
                _EXCHANGES.Add(exchange, new Exchange(exchange, depth));
            _EXCHANGES[exchange].UpdateSnapshot(ob, depth);            
        }

        static void SendMarketData_toWS(jsonMarkets toSend)
        {
            string _json = Newtonsoft.Json.JsonConvert.SerializeObject(toSend);
            foreach (var cli in _SERVER_WS.ListClients())
            {
                if (_SERVER_WS.IsClientConnected(cli))
                {
                    _SERVER_WS.SendAsync(cli, _json);
                }                
            }
        }



        #region webserver callbacks
        static void ClientConnected(object sender, ClientConnectedEventArgs args)
        {
            Console.WriteLine("Client connected: " + args.IpPort);
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
