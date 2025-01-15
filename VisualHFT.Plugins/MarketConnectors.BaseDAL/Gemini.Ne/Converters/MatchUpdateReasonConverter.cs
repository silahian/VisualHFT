using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class MatchUpdateReasonConverter : BaseConverter<MatchUpdateReason>
    {
        public MatchUpdateReasonConverter() : this(true) { }
        public MatchUpdateReasonConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<MatchUpdateReason, string>> Mapping => new List<KeyValuePair<MatchUpdateReason, string>>
        {
            new KeyValuePair<MatchUpdateReason, string>(MatchUpdateReason.Canceled, "canceled"),
            new KeyValuePair<MatchUpdateReason, string>(MatchUpdateReason.Filled, "filled")
        };
    }
}
