using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot.Socket
{
    /// <summary>
    /// Margin order update
    /// </summary>
    public record GeminiMarginOrderUpdate
    {
        /// <summary>
        /// Asset name
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Order id
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; } = string.Empty;
        /// <summary>
        /// Daily interest rate
        /// </summary>
        [JsonProperty("dailyIntRate")]
        public decimal DailyInterestRate { get; set; }
        /// <summary>
        /// Term in days
        /// </summary>
        [JsonProperty("term")]
        public int Term { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonProperty("size")]
        public int Quantity { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonProperty("lentSize")]
        public decimal? LentQuantity { get; set; }
        /// <summary>
        /// Lend
        /// </summary>
        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("ts")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }
}
