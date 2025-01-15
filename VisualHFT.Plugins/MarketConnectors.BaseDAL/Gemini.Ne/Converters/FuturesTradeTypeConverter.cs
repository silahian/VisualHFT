using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class FuturesTradeTypeConverter : BaseConverter<FuturesTradeType>
    {
        public FuturesTradeTypeConverter() : this(true) { }

        public FuturesTradeTypeConverter(bool quotes) : base(quotes)
        {
        }

        protected override List<KeyValuePair<FuturesTradeType, string>> Mapping => new List<KeyValuePair<FuturesTradeType, string>>
        {
            new KeyValuePair<FuturesTradeType, string>(FuturesTradeType.Trade, "trade"),
            new KeyValuePair<FuturesTradeType, string>(FuturesTradeType.Liquidation, "liquidation"),
            new KeyValuePair<FuturesTradeType, string>(FuturesTradeType.ADL, "ADL"),
            new KeyValuePair<FuturesTradeType, string>(FuturesTradeType.Settlement, "settlement")
        };
    }
}
