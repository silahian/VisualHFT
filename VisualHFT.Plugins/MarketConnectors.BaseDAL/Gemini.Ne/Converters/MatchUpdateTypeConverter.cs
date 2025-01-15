using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class MatchUpdateTypeConverter : BaseConverter<MatchUpdateType>
    {
        public MatchUpdateTypeConverter() : this(true) { }
        public MatchUpdateTypeConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<MatchUpdateType, string>> Mapping => new List<KeyValuePair<MatchUpdateType, string>>
        {
            new KeyValuePair<MatchUpdateType, string>(MatchUpdateType.Received, "received"),
            new KeyValuePair<MatchUpdateType, string>(MatchUpdateType.Match, "match"),
            new KeyValuePair<MatchUpdateType, string>(MatchUpdateType.Open, "open"),
            new KeyValuePair<MatchUpdateType, string>(MatchUpdateType.Canceled, "canceled"),
            new KeyValuePair<MatchUpdateType, string>(MatchUpdateType.Filled, "filled"),
            new KeyValuePair<MatchUpdateType, string>(MatchUpdateType.Update, "update"),
        };
    }
}
