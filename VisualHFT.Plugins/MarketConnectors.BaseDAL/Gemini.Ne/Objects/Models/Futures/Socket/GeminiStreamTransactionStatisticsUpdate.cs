using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures.Socket
{
    /// <summary>
    /// 24 Hour statistics update
    /// </summary>
    public record GeminiStreamTransactionStatisticsUpdate
    {
        /// <summary>
        /// Volume
        /// </summary>
        public decimal Volume { get; set; }
        /// <summary>
        /// Turnover
        /// </summary>
        public decimal Turnover { get; set; }
        /// <summary>
        /// Last price
        /// </summary>
        public decimal LastPrice { get; set; }
        /// <summary>
        /// Price change percentage
        /// </summary>
        [JsonProperty("priceChgPct")]
        public decimal PriceChangePercentage { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("ts"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }
}
