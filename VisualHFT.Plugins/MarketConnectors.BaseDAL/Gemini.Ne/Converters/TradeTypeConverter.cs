using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class TradeTypeConverter : BaseConverter<TradeType>
    {
        public TradeTypeConverter() : this(true) { }

        public TradeTypeConverter(bool quotes) : base(quotes)
        {
        }

        protected override List<KeyValuePair<TradeType, string>> Mapping => new List<KeyValuePair<TradeType, string>>
        {
            new KeyValuePair<TradeType, string>(TradeType.IsolatedMarginTrade, "ISOLATED_MARGIN_TRADE"),
            new KeyValuePair<TradeType, string>(TradeType.MarginTrade, "MARGIN_TRADE"),
            new KeyValuePair<TradeType, string>(TradeType.SpotTrade, "TRADE")
        };
    }
}
