using Gemini.Net.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Gemini.Net.Enums;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// The order model to be sent via bulk order endpoint
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public record GeminiHfBulkOrderRequestEntry
    {
        /// <summary>
        /// The symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The client order id
        /// </summary>
        [JsonProperty("clientOid")]
        public string ClientOrderId { get; set; } = string.Empty;
        /// <summary>
        /// The side of the order
        /// </summary>
        [JsonConverter(typeof(OrderSideConverter))]
        [JsonProperty("side")]
        public OrderSide Side { get; set; }
        /// <summary>
        /// The price of the order
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }
        /// <summary>
        /// The quantity of the order
        /// </summary>
        [JsonProperty("size")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// The type of the order
        /// </summary>
        [JsonConverter(typeof(NewOrderTypeConverter))]
        [JsonProperty("type")]
        public NewOrderType Type { get; set; }

        /// <summary>
        /// Remark for the order
        /// </summary>
        [JsonProperty("remark")]
        public string? Remark { get; set; }
        /// <summary>
        /// Tags for the order
        /// </summary>
        [JsonProperty("tags")]
        public string? Tags { get; set; }
        /// <summary>
        /// The self trade prevention type
        /// </summary>
        [JsonProperty("stp"), JsonConverter(typeof(SelfTradePreventionConverter))]
        public SelfTradePrevention? SelfTradePrevention { get; set; }
        /// <summary>
        /// The time in force of the order
        /// </summary>
        [JsonConverter(typeof(TimeInForceConverter))]
        [JsonProperty("timeInForce")]
        public TimeInForce? TimeInForce { get; set; }
        /// <summary>
        /// Timespan in seconds after which the order is canceled
        /// </summary>
        [JsonProperty("cancelAfter")]
        public int? CancelAfter { get; set; }
        /// <summary>
        /// Whether the order is post only
        /// </summary>
        [JsonProperty("postOnly")]
        public bool? PostOnly { get; set; }
        /// <summary>
        /// Whether the order is hidden
        /// </summary>
        [JsonProperty("hidden")]
        public bool? Hidden { get; set; }
        /// <summary>
        /// Whether it is an iceberg order
        /// </summary>
        [JsonProperty("iceberg")]
        public bool? Iceberg { get; set; }
        /// <summary>
        /// The max visible size of the iceberg
        /// </summary>
        [JsonProperty("visibleSize")]
        public decimal? VisibleIcebergSize { get; set; }
    }
}
