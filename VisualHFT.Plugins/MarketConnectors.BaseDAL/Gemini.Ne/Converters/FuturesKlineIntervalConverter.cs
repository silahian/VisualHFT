using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class FuturesKlineIntervalConverter : BaseConverter<FuturesKlineInterval>
    {
        public FuturesKlineIntervalConverter() : this(true) { }
        public FuturesKlineIntervalConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<FuturesKlineInterval, string>> Mapping => new List<KeyValuePair<FuturesKlineInterval, string>>
        {
            new KeyValuePair<FuturesKlineInterval, string>(FuturesKlineInterval.OneMinute, "1"),
            new KeyValuePair<FuturesKlineInterval, string>(FuturesKlineInterval.FiveMinutes, "5"),
            new KeyValuePair<FuturesKlineInterval, string>(FuturesKlineInterval.FifteenMinutes, "15"),
            new KeyValuePair<FuturesKlineInterval, string>(FuturesKlineInterval.ThirtyMinutes, "30"),
            new KeyValuePair<FuturesKlineInterval, string>(FuturesKlineInterval.OneHour, "60"),
            new KeyValuePair<FuturesKlineInterval, string>(FuturesKlineInterval.TwoHours, "120"),
            new KeyValuePair<FuturesKlineInterval, string>(FuturesKlineInterval.FourHours, "240"),
            new KeyValuePair<FuturesKlineInterval, string>(FuturesKlineInterval.EightHours, "480"),
            new KeyValuePair<FuturesKlineInterval, string>(FuturesKlineInterval.TwelveHours, "720"),
            new KeyValuePair<FuturesKlineInterval, string>(FuturesKlineInterval.OneDay, "1440"),
            new KeyValuePair<FuturesKlineInterval, string>(FuturesKlineInterval.OneWeek, "10080"),
        };
    }
}
