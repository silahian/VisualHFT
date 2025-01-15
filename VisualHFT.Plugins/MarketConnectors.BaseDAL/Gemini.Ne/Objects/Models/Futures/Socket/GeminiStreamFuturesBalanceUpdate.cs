using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures.Socket
{
    /// <summary>
    /// Balance update
    /// </summary>
    public record GeminiStreamFuturesBalanceUpdate
    {
        /// <summary>
        /// Available balance
        /// </summary>
        public decimal AvailableBalance { get; set; }
        /// <summary>
        /// Frozen balance
        /// </summary>
        public decimal HoldBalance { get; set; }
        /// <summary>
        /// Asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }
}
