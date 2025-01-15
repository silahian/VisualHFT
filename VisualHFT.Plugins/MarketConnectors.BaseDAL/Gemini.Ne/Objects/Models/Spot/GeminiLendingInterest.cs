using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Lending interest info
    /// </summary>
    public record GeminiLendingInterest
    {
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Market interest rate
        /// </summary>
        [JsonProperty("marketInterestRate")]
        public decimal InterestRate { get; set; }
    }
}
