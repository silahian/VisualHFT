using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures.Socket
{
    /// <summary>
    /// Match info
    /// </summary>
    public record GeminiStreamFuturesMatch: GeminiStreamMatchBase
    {
        /// <summary>
        /// Gets time of the trade match
        /// </summary>
        [JsonProperty("ts"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Marer user id
        /// </summary>
        public string MakerUserId { get; set; } = string.Empty;
        /// <summary>
        /// Taker user id
        /// </summary>
        public string TakerUserId { get; set; } = string.Empty;
    }
}
