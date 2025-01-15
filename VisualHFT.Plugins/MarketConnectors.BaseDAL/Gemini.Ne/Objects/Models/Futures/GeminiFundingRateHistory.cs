using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Funding rate history
    /// </summary>
    public record GeminiFundingRateHistory
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Funding rate
        /// </summary>
        [JsonProperty("fundingRate")]
        public decimal FundingRate { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timepoint")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }
}
