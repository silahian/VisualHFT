using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class StopOrderStatusConverter : BaseConverter<StopOrderStatus>
    {
        public StopOrderStatusConverter() : this(true) { }
        public StopOrderStatusConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<StopOrderStatus, string>> Mapping => new List<KeyValuePair<StopOrderStatus, string>>
        {
            new KeyValuePair<StopOrderStatus, string>(StopOrderStatus.New, "NEW"),
            new KeyValuePair<StopOrderStatus, string>(StopOrderStatus.Triggered, "TRIGGERED")
        };
    }
}
