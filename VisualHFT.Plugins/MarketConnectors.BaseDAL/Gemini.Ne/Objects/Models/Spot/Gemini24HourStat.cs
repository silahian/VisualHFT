using System;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// 24 hours stats
    /// </summary>
    public record Gemini24HourStat
    {
        /// <summary>
        /// The symbol the stat is for
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The highest price in the last 24 hours
        /// </summary>
        [JsonProperty("high")]
        public decimal? HighPrice { get; set; }
        /// <summary>
        /// The lowest price in the last 24 hours
        /// </summary>
        [JsonProperty("low")]
        public decimal? LowPrice { get; set; }
        /// <summary>
        /// The volume of the past 24 hours
        /// </summary>
        [JsonProperty("vol")]
        public decimal? Volume { get; set; }
        /// <summary>
        /// The value of the volume in the past 24 hours
        /// </summary>
        [JsonProperty("volValue")]
        public decimal? QuoteVolume { get; set; }
        /// <summary>
        /// The last trade price
        /// </summary>
        [JsonProperty("last")]
        public decimal? LastPrice { get; set; }
        /// <summary>
        /// The best ask price
        /// </summary>
        [JsonProperty("buy")]
        public decimal? BestAskPrice { get; set; }
        /// <summary>
        /// The best bid price
        /// </summary>
        [JsonProperty("sell")]
        public decimal? BestBidPrice { get; set; }
        /// <summary>
        /// The price change since 24 hours ago
        /// </summary>
        public decimal? ChangePrice { get; set; }
        /// <summary>
        /// The percentage change since 24 hours ago
        /// </summary>
        [JsonProperty("changeRate")]
        public decimal? ChangePercentage { get; set; }
        /// <summary>
        /// The timestamp of the data
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter)), JsonProperty("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The average trade price in the last 24 hours
        /// </summary>
        public decimal? AveragePrice { get; set; }

        /// <summary>
        /// Basic Taker Fee
        /// </summary>
        public decimal? TakerFeeRate { get; set; }
        /// <summary>
        /// Basic Maker Fee
        /// </summary>
        public decimal? MakerFeeRate { get; set; }
        /// <summary>
        /// Taker Fee Coefficient
        /// </summary>
        public decimal? TakerCoefficient { get; set; }
        /// <summary>
        /// Maker Fee Coefficient
        /// </summary>
        public decimal? MakerCoefficient { get; set; }
    }
}
