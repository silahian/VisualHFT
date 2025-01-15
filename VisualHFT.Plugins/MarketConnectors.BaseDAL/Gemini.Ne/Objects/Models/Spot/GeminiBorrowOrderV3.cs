using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Borrow order info
    /// </summary>
    public record GeminiBorrowOrderV3
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
        /// Actual quantity
        /// </summary>
        [JsonProperty("actualSize")]
        public decimal ActualQuantity { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
        /// <summary>
        /// Time of borrowing
        /// </summary>
        [JsonProperty("createdTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }
}
