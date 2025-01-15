using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures.Socket
{
    /// <summary>
    /// Contract announcement
    /// </summary>
    public record GeminiContractAnnouncement
    {
        /// <summary>
        /// Event type
        /// </summary>
        public string Event { get; set; } = string.Empty;
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Funding rate
        /// </summary>
        public decimal FundingRate { get; set; }
        /// <summary>
        /// Funding time
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime FundingTime { get; set; }
        /// <summary>
        /// Event time
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }
}
