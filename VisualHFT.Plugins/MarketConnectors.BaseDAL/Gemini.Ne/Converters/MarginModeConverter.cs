using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;
using System.Collections.Generic;

namespace Gemini.Net.Converters
{
    internal class MarginModeConverter : BaseConverter<MarginMode>
    {
        public MarginModeConverter() : this(true) { }
        public MarginModeConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<MarginMode, string>> Mapping => new List<KeyValuePair<MarginMode, string>>
        {
            new KeyValuePair<MarginMode, string>(MarginMode.CrossMode, "cross"),
            new KeyValuePair<MarginMode, string>(MarginMode.IsolatedMode, "isolated"),
        };
    }
}
