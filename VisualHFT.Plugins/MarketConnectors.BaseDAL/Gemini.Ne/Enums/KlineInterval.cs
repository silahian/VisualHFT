namespace Gemini.Net.Enums
{
    /// <summary>
    /// The time for each candlestick, the int value represent the time in seconds
    /// </summary>
    public enum KlineInterval
    {
        /// <summary>
        /// 1m
        /// </summary>
        OneMinute = 60,
        /// <summary>
        /// 3m
        /// </summary>
        ThreeMinutes = 60 * 3,
        /// <summary>
        /// 5m
        /// </summary>
        FiveMinutes = 60 * 5,
        /// <summary>
        /// 15m
        /// </summary>
        FifteenMinutes = 60 * 15,
        /// <summary>
        /// 30m
        /// </summary>
        ThirtyMinutes = 60 * 30,
        /// <summary>
        /// 1h
        /// </summary>
        OneHour = 60 * 60,
        /// <summary>
        /// 2h
        /// </summary>
        TwoHours = 60 * 60 * 2,
        /// <summary>
        /// 4h
        /// </summary>
        FourHours = 60 * 60 * 4,
        /// <summary>
        /// 6h
        /// </summary>
        SixHours = 60 * 60 * 6,
        /// <summary>
        /// 8h
        /// </summary>
        EightHours = 60 * 60 * 8,
        /// <summary>
        /// 12h
        /// </summary>
        TwelveHours = 60 * 60 * 12,
        /// <summary>
        /// 1d
        /// </summary>
        OneDay = 60 * 60 * 24,
        /// <summary>
        /// 1w
        /// </summary>
        OneWeek = 60 * 60 * 24 * 7,
        /// <summary>
        /// 1M
        /// </summary>
        OneMonth = 60 * 60 * 24 * 31
    }
}
