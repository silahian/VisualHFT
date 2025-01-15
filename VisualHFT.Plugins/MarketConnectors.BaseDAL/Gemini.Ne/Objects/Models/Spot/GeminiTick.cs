using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Tick info
    /// </summary>
    public record GeminiTick
    {
        /// <summary>
        /// The sequence of the tick
        /// </summary>
        public long Sequence { get; set; }
        /// <summary>
        /// The price of the last trade
        /// </summary>
        [JsonProperty("price")]
        public decimal? LastPrice { get; set; }
        /// <summary>
        /// The quantity of the last trade
        /// </summary>
        [JsonProperty("size")]
        public decimal? LastQuantity { get; set; }
        /// <summary>
        /// The best ask price
        /// </summary>
        [JsonProperty("bestAsk")]
        public decimal? BestAskPrice { get; set; }
        /// <summary>
        /// The quantity of the best ask price
        /// </summary>
        [JsonProperty("bestAskSize")]
        public decimal? BestAskQuantity { get; set; }
        /// <summary>
        /// The best bid price
        /// </summary>
        [JsonProperty("bestBid")]
        public decimal? BestBidPrice { get; set; }
        /// <summary>
        /// The quantity of the best bid
        /// </summary>
        [JsonProperty("bestBidSize")]
        public decimal? BestBidQuantity { get; set; }
        /// <summary>
        /// The timestamp of the data
        /// </summary>
        [JsonProperty("time"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }
}
