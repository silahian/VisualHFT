using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot.Socket
{
    /// <summary>
    /// Best offer info
    /// </summary>
    public record GeminiStreamBestOffers
    {
        /// <summary>
        /// Data timestamp
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The current best ask
        /// </summary>
        [JsonProperty("asks")]
        public GeminiStreamOrderBookEntry BestAsk { get; set; } = null!;
        /// <summary>
        /// The current best bid
        /// </summary>
        [JsonProperty("bids")]
        public GeminiStreamOrderBookEntry BestBid { get; set; } = null!;
    }
}
