﻿using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;

namespace demoTradingCore.Models
{

    public class OrderBook
    {
        private Dictionary<decimal, Extension.ExchangeOrderPrice> _asks;
        private Dictionary<decimal, Extension.ExchangeOrderPrice> _bids;
        private long _lastSequenceId = 0;
        private int _depthOfBook;
        public OrderBook(int depth)
        {
            _depthOfBook = depth;
            _asks = new Dictionary<decimal, Extension.ExchangeOrderPrice>();
            _bids = new Dictionary<decimal, Extension.ExchangeOrderPrice>();
        }

        public void UpdateSnapshot(ExchangeOrderBook ob)
        {
            _lastSequenceId = ob.SequenceId;
            lock (_asks)
                _asks = ob.Asks.ToArray().Select(x => new KeyValuePair<decimal, Extension.ExchangeOrderPrice>(x.Key, new Extension.ExchangeOrderPrice()
                {
                    Amount = x.Value.Amount,
                    LocalTimestamp = DateTime.Now,
                    Price = x.Value.Price,
                    ServerTimestamp = ob.LastUpdatedUtc.ToLocalTime(),
                })).ToDictionary(x => x.Key, x => x.Value);
            lock(_bids)
                _bids = ob.Bids.ToArray().Select(x => new KeyValuePair<decimal, Extension.ExchangeOrderPrice>(x.Key, new Extension.ExchangeOrderPrice()
                {
                    Amount = x.Value.Amount,
                    LocalTimestamp = DateTime.Now,
                    Price = x.Value.Price,
                    ServerTimestamp = ob.LastUpdatedUtc.ToLocalTime()
                })).ToDictionary(x => x.Key, x => x.Value);
        }

        public IEnumerable<Extension.ExchangeOrderPrice> GetAsks()
        {
            lock (_asks)
                return _asks.Values.ToList();
        }
        public IEnumerable<Extension.ExchangeOrderPrice> GetBids()
        {
            lock (_bids)
                return _bids.Values.ToList();
        }
        public IEnumerable<Extension.ExchangeOrderPrice> GetTopOfBook()
        {
            lock (_asks)
            {
                lock(_bids)
                {
                    var b = _bids.OrderBy(x => x.Value.Price).LastOrDefault().Value;
                    var a = _asks.OrderBy(x => x.Value.Price).FirstOrDefault().Value;
                    if (a == null || b == null)
                        return null;
                    return new List<Extension.ExchangeOrderPrice>() { b, a };
                }
            }
        }
    }
}
