using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitStamp.Net.Models
{
    public class Data
    {
        public string channel { get; set; }
    }

    public class BitStampSubscriptions
    {
        public string @event { get; set; } = "bts:subscribe";
        public Data data { get; set; }
    }
    public class BitStampTradeData
    {
        public int id { get; set; }
        public long timestamp { get; set; }
        public decimal amount { get; set; }
        public string amount_str { get; set; }
        public decimal price { get; set; }
        public string price_str { get; set; }
        public int type { get; set; }
        public string microtimestamp { get; set; }
        public long buy_order_id { get; set; }
        public long sell_order_id { get; set; }
    }

    public class BitStampTrade
    {
        public BitStampTradeData data { get; set; }
        public string channel { get; set; }
        public string @event { get; set; }
    }
    public class InitialResponse
    {
        public string timestamp { get; set; }
        public string microtimestamp { get; set; }
        public List<List<string>> bids { get; set; }
        public List<List<string>> asks { get; set; }
    }
    public class BitStampOrderBookData
    {
        public string timestamp { get; set; }
        public string microtimestamp { get; set; }
        public List<List<string>> bids { get; set; }
        public List<List<string>> asks { get; set; }
    }

    public class BitStampOrderBook
    {
        public BitStampOrderBookData data { get; set; }
        public string channel { get; set; }
        public string @event { get; set; }
    }

}
