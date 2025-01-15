using System;

namespace demoTradingCore.Models.Extension
{
    public class ExchangeOrderPrice
    {
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public DateTime ServerTimestamp { get; set; }
        public DateTime LocalTimestamp { get; set; }
    }
}
