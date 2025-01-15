using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Hold info
    /// </summary>
    public record GeminiHold
    {
        /// <summary>
        /// The asset of the hold
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// The quantity of the hold
        /// </summary>
        [JsonProperty("holdAmount")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// The type the hold is for
        /// </summary>
        public string BizType { get; set; } = string.Empty;
        /// <summary>
        /// The order id of the hold
        /// </summary>
        public string OrderId { get; set; } = string.Empty;
        /// <summary>
        /// The time the hold was created
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("createdAt")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// The time the hold was last updated
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("updatedAt")]
        public DateTime UpdateTime { get; set; }
    }
}
