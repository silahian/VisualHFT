using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class StopTypeConverter : BaseConverter<StopType>
    {
        public StopTypeConverter() : this(true) { }
        public StopTypeConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<StopType, string>> Mapping => new List<KeyValuePair<StopType, string>>
        {
            new KeyValuePair<StopType, string>(StopType.Up, "up"),
            new KeyValuePair<StopType, string>(StopType.Down, "down"),
        };
    }
}
