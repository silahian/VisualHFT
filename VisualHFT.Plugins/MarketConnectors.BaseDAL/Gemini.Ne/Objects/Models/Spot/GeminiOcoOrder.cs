using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// One Cancels Other order
    /// </summary>
    public record GeminiOcoOrder
    {
        /// <summary>
        /// Order id
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; } = string.Empty;
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Client order id
        /// </summary>
        [JsonProperty("clientOid")]
        public string? ClientOrderId { get; set; }
        /// <summary>
        /// Order time
        /// </summary>
        [JsonProperty("orderTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// Order status
        /// </summary>
        [JsonProperty("status")]
        [JsonConverter(typeof(EnumConverter))]
        public OcoOrderStatus Status { get; set; }
    }

    /// <summary>
    /// Oco order details
    /// </summary>
    public record GeminiOcoOrderDetails : GeminiOcoOrder
    {
        /// <summary>
        /// Orders
        /// </summary>
        [JsonProperty("orders")]
        public IEnumerable<GeminiOcoOrderInfo> Orders { get; set; } = Array.Empty<GeminiOcoOrderInfo>();
    }

    /// <summary>
    /// Oco stop order info
    /// </summary>
    public record GeminiOcoOrderInfo
    {
        /// <summary>
        /// Order id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Side
        /// </summary>
        [JsonProperty("side")]
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }
        /// <summary>
        /// Price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }
        /// <summary>
        /// Stop price
        /// </summary>
        [JsonProperty("stopPrice")]
        public decimal StopPrice { get; set; }
        /// <summary>
        /// Order quantity
        /// </summary>
        [JsonProperty("size")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
    }
}
