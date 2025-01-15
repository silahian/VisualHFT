using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot.Socket
{
    /// <summary>
    /// Margin order done event
    /// </summary>
    public record GeminiMarginOrderDoneUpdate
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
        /// Order done reason
        /// </summary>
        [JsonProperty("reason")]
        [JsonConverter(typeof(EnumConverter))]
        public MarginOrderDoneReason Reason { get; set; }
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
