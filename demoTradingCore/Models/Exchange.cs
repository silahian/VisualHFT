using ExchangeSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace demoTradingCore.Models
{
    public class Exchange
    {
        private Dictionary<string, OrderBook> _orderBooks { get; set; }
        private object _orderBooksLCK = new object();
        private eEXCHANGE _exchange;
        private DateTime? _lastUpdated = DateTime.MinValue;

        public Exchange(eEXCHANGE exchange, int depth)
        {
            _exchange = exchange;
            _orderBooks = new Dictionary<string, OrderBook>();
        }
        public void UpdateSnapshot(ExchangeOrderBook ob, int depth)
        {
            lock (_orderBooksLCK)
            {
                if (!_orderBooks.ContainsKey(ob.MarketSymbol))
                    _orderBooks.Add(ob.MarketSymbol, new OrderBook(depth));
                _orderBooks[ob.MarketSymbol].UpdateSnapshot(ob);
                _lastUpdated = DateTime.Now;
            }

        }
        public IEnumerable<Extension.ExchangeOrderPrice> GetTopOfBook(string symbol)
        {
            lock (_orderBooksLCK)
            {
                if (_orderBooks.ContainsKey(symbol))
                    return _orderBooks[symbol].GetTopOfBook();
                else
                    return null;
            }
        }
        public jsonMarkets GetSnapshots()
        {
            jsonMarkets ret = new jsonMarkets();
            ret.dataObj = new List<jsonMarket>();
            lock (_orderBooksLCK)
            {
                foreach (var symbol in _orderBooks.Keys)
                {
                    if (_orderBooks.ContainsKey(symbol))
                    {
                        var _asks = _orderBooks[symbol].GetAsks();
                        var _bids = _orderBooks[symbol].GetBids();
                        if (_asks.Any() || _bids.Any())
                        {
                            jsonMarket m = new jsonMarket();
                            m.Symbol = symbol;
                            m.ProviderId = (int)_exchange;
                            m.ProviderName = _exchange.ToString();
                            m.ProviderStatus = 2; //conected
                            m.SymbolMultiplier = 1;
                            m.DecimalPlaces = CalculateDecimalPlaces(_asks);
                            m.Bids = _bids.Select(x => new jsonBookItem()
                            {
                                DecimalPlaces = m.DecimalPlaces,
                                EntryID = (int)x.Price * (10 * m.DecimalPlaces),
                                IsBid = true,
                                LayerName = "",
                                LocalTimeStamp = x.LocalTimestamp,
                                Price = x.Price,
                                ProviderID = m.ProviderId,
                                ServerTimeStamp = x.ServerTimestamp,
                                Size = x.Amount,
                                Symbol = m.Symbol
                            }).ToList();
                            m.Asks = _asks.Select(x => new jsonBookItem()
                            {
                                DecimalPlaces = m.DecimalPlaces,
                                EntryID = (int)x.Price * (10 * m.DecimalPlaces),
                                IsBid = false,
                                LayerName = "",
                                LocalTimeStamp = x.LocalTimestamp,
                                Price = x.Price,
                                ProviderID = m.ProviderId,
                                ServerTimeStamp = x.ServerTimestamp,
                                Size = x.Amount,
                                Symbol = m.Symbol
                            }).ToList();
                            ret.dataObj.Add(m);
                        }

                    }
                }
            }
            
            return ret;
        }
        private int CalculateDecimalPlaces(IEnumerable<Extension.ExchangeOrderPrice> _prices)
        {
            if (_prices != null &&  _prices.Any())
            {
                var priceSample = _prices
                    .Where(x =>                     
                                x.Price.ToString().IndexOf(".") > -1 //has decimal
                                && Convert.ToInt32(x.Price.ToString().Split('.')[1]) > 0 // The decimal part must be > 0
                    ) 
                    .FirstOrDefault();
                if (priceSample != null)
                {
                    string strFirst = priceSample.Price.ToString();
                    int decimalPlaces = strFirst.Length - strFirst.IndexOf('.') - 1;

                    return Math.Max(decimalPlaces, 2);
                }
                
            }
            return 2; //default
        }
        public string ExchangeName
        { get { return _exchange.ToString(); } }
        public DateTime? LastUpdated { get { return _lastUpdated; } }
    }
}
