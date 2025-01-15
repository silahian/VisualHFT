using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Repayment order info
    /// </summary>
    public record GeminiRepayOrderV3
    {
        /// <summary>
        /// Order id
        /// </summary>
        [JsonProperty("orderNo")]
        public string OrderId { get; set; } = string.Empty;
        /// <summary>
        /// Isolated margin symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string? Symbol { get; set; }
        /// <summary>
        /// Asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Iniated quantity
        /// </summary>
        [JsonProperty("size")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Principal to be paid
        /// </summary>
        [JsonProperty("principal")]
        public decimal Principal { get; set; }
        /// <summary>
        /// Interest to be paid
        /// </summary>
        [JsonProperty("interest")]
        public decimal Interest { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
        /// <summary>
        /// Time of repayment
        /// </summary>
        [JsonProperty("createdTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }
}
