using System;
using Gemini.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// The order model in bulk order creation response
    /// </summary>
    public record GeminiHfOrder
    {
        /// <summary>
        /// The id of the order
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; } = string.Empty;
        /// <summary>
        /// Order timestamp
        /// </summary>
        [JsonProperty("orderTime"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// Trade timestamp
        /// </summary>
        [JsonProperty("matchTime"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime? TradeTime { get; set; }
        /// <summary>
        /// The quantity of the order
        /// </summary>
        [JsonProperty("originSize")]
        public decimal? Quantity { get; set; }
        /// <summary>
        /// The quote quantity of the order
        /// </summary>
        [JsonProperty("originFunds")]
        public decimal? QuoteQuantity { get; set; }
        /// <summary>
        /// The quantity of the order which was filled
        /// </summary>
        [JsonProperty("dealSize")]
        public decimal? QuantityFilled { get; set; }
        /// <summary>
        /// The quote quantity of the order which was filled
        /// </summary>
        [JsonProperty("dealFunds")]
        public decimal? QuoteQuantityFilled { get; set; }
        /// <summary>
        /// The quantity of the order which is still open
        /// </summary>
        [JsonProperty("remainSize")]
        public decimal? QuantityRemaining { get; set; }
        /// <summary>
        /// The quote quantity of the order which is still open
        /// </summary>
        [JsonProperty("remainFunds")]
        public decimal? QuoteQuantityRemaining { get; set; }
        /// <summary>
        /// The quantity of the order which was canceled
        /// </summary>
        [JsonProperty("canceledSize")]
        public decimal? QuantityCanceled { get; set; }
        /// <summary>
        /// The quote quantity of the order which was canceled
        /// </summary>
        [JsonProperty("canceledFunds")]
        public decimal? QuoteQuantityCanceled { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [JsonConverter(typeof(EnumConverter))]
        [JsonProperty("status")]
        public OrderStatus Status { get; set; }
    }

    /// <summary>
    /// The order model in bulk order creation response
    /// </summary>
    public record GeminiHfBulkOrderResponse : GeminiHfOrder
    {
        /// <summary>
        /// Successful or not
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
