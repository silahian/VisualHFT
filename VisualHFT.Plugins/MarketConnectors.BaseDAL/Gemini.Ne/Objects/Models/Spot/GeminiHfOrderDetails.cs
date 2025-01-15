using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Order info
    /// </summary>
    public record GeminiHfOrderDetails : GeminiOrder
    {
        /// <summary>
        /// Whether the order is active
        /// </summary>
        [JsonProperty("active")]
        public override bool? IsActive { get; set; }

        /// <summary>
        /// Is the order in the order book
        /// </summary>
        [JsonProperty("inOrderBook")]
        public bool InOrderBook { get; set; }
        /// <summary>
        /// Last update time
        /// </summary>
        [JsonProperty("lastUpdatedAt")]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// Quantity canceled
        /// </summary>
        [JsonProperty("cancelledSize")]
        public decimal? QuantityCanceled { get; set; }
        /// <summary>
        /// Quote quantity canceled
        /// </summary>
        [JsonProperty("cancelledFunds")]
        public decimal? QuoteQuantityCanceled { get; set; }
        /// <summary>
        /// Remaining quantity
        /// </summary>
        [JsonProperty("remainSize")]
        public decimal? QuantityRemaining { get; set; }
        /// <summary>
        /// Remaining quote quantity
        /// </summary>
        [JsonProperty("remainFunds")]
        public decimal? QuoteQuantityRemaining { get; set; }
    }
}
