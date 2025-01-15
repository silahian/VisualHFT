using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class StopPriceTypeConverter : BaseConverter<StopPriceType>
    {
        public StopPriceTypeConverter() : this(true) { }
        public StopPriceTypeConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<StopPriceType, string>> Mapping => new List<KeyValuePair<StopPriceType, string>>
        {
            new KeyValuePair<StopPriceType, string>(StopPriceType.IndexPrice, "IP"),
            new KeyValuePair<StopPriceType, string>(StopPriceType.MarkPrice, "MP"),
            new KeyValuePair<StopPriceType, string>(StopPriceType.TradePrice, "TP"),
        };
    }
}
