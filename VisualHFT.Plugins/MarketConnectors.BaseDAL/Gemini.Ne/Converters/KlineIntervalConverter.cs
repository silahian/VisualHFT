using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class KlineIntervalConverter : BaseConverter<KlineInterval>
    {
        public KlineIntervalConverter() : this(true) { }
        public KlineIntervalConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<KlineInterval, string>> Mapping => new List<KeyValuePair<KlineInterval, string>>
        {
            new KeyValuePair<KlineInterval, string>(KlineInterval.OneMinute, "1min"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.ThreeMinutes, "3min"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.FiveMinutes, "5min"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.FifteenMinutes, "15min"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.ThirtyMinutes, "30min"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.OneHour, "1hour"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.TwoHours, "2hour"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.FourHours, "4hour"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.SixHours, "6hour"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.EightHours, "8hour"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.TwelveHours, "12hour"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.OneDay, "1day"),
            new KeyValuePair<KlineInterval, string>(KlineInterval.OneWeek, "1week"),
        };
    }
}
