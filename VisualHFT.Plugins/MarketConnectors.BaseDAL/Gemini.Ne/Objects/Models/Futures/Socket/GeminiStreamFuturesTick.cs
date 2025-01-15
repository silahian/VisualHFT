using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures.Socket
{
    /// <summary>
    /// Tick info
    /// </summary>
    public record GeminiStreamFuturesTick
    {
        /// <summary>
        /// Sequence number
        /// </summary>
        public long Sequence { get; set; }
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;        
        /// <summary>
        /// Best bid quantity
        /// </summary>
        [JsonProperty("bestBidSize")]
        public decimal BestBidQuantity { get; set; }
        /// <summary>
        /// Best bid price
        /// </summary>
        public decimal BestBidPrice { get; set; }
        /// <summary>
        /// Best ask quantity
        /// </summary>
        [JsonProperty("bestAskSize")]
        public decimal BestAskQuantity { get; set; }
        /// <summary>
        /// Best ask price
        /// </summary>
        public decimal BestAskPrice { get; set; }
        /// <summary>
        /// Filled time
        /// </summary>
        [JsonProperty("ts"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }
}
