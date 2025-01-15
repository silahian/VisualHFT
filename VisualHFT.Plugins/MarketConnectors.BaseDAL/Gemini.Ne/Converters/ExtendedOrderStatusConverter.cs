using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class ExtendedOrderStatusConverter : BaseConverter<ExtendedOrderStatus>
    {
        public ExtendedOrderStatusConverter() : this(true) { }
        public ExtendedOrderStatusConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<ExtendedOrderStatus, string>> Mapping => new List<KeyValuePair<ExtendedOrderStatus, string>>
        {
            new KeyValuePair<ExtendedOrderStatus, string>(ExtendedOrderStatus.New, "new"),
            new KeyValuePair<ExtendedOrderStatus, string>(ExtendedOrderStatus.Match, "match"),
            new KeyValuePair<ExtendedOrderStatus, string>(ExtendedOrderStatus.Open, "open"),
            new KeyValuePair<ExtendedOrderStatus, string>(ExtendedOrderStatus.Done, "done"),
        };
    }
}
