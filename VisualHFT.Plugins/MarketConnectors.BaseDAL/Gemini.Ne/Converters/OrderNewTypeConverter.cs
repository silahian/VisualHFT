using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class NewOrderTypeConverter : BaseConverter<NewOrderType>
    {
        public NewOrderTypeConverter() : this(true) { }
        public NewOrderTypeConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<NewOrderType, string>> Mapping => new List<KeyValuePair<NewOrderType, string>>
        {
            new KeyValuePair<NewOrderType, string>(NewOrderType.Limit, "limit"),
            new KeyValuePair<NewOrderType, string>(NewOrderType.Market, "market")
        };
    }
}
