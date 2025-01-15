using System;
using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// The response for bulk order creation
    /// </summary>
    public record GeminiBulkOrderResponse
    {
        /// <summary>
        /// List of orders
        /// </summary>
        [JsonProperty("data")]
        public IEnumerable<GeminiBulkOrderResponseEntry> Orders { get; set; } = default!;
    }

    /// <summary>
    /// The order model in bulk order creation response
    /// </summary>
    public record GeminiBulkOrderResponseEntry
    {
        /// <summary>
        /// The id of the order
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// The client order id
        /// </summary>
        [JsonProperty("clientOid")]
        public string ClientOrderId { get; set; } = string.Empty;
        /// <summary>
        /// the symbol of the order
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The type of the order
        /// </summary>
        [JsonConverter(typeof(OrderTypeConverter))]
        public OrderType Type { get; set; }
        /// <summary>
        /// The side of the order
        /// </summary>
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }
        /// <summary>
        /// The price of the order
        /// </summary>
        public decimal? Price { get; set; }
        /// <summary>
        /// The quantity of the order
        /// </summary>
        [JsonProperty("size")]
        public decimal? Quantity { get; set; }
        /// <summary>
        /// The funds of the order
        /// </summary>
        [JsonProperty("funds")]
        public decimal? QuoteQuantity { get; set; }
        /// <summary>
        /// The self trade prevention type
        /// </summary>
        [JsonProperty("stp"), JsonConverter(typeof(SelfTradePreventionConverter))]
        public SelfTradePrevention? SelfTradePrevention { get; set; }
        /// <summary>
        /// The stop condition
        /// </summary>
        public StopCondition? Stop { get; set; }
        /// <summary>
        /// The stop price
        /// </summary>
        public decimal? StopPrice { get; set; }

        /// <summary>
        /// The time in force of the order
        /// </summary>
        [JsonConverter(typeof(TimeInForceConverter))]
        public TimeInForce? TimeInForce { get; set; }
        /// <summary>
        /// Time after which the order is canceled
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime CancelAfter { get; set; }
        /// <summary>
        /// Whether the order is post only
        /// </summary>
        public bool PostOnly { get; set; }
        /// <summary>
        /// Whether the order is hidden
        /// </summary>
        public bool Hidden { get; set; }
        /// <summary>
        /// Whether it is an iceberg order
        /// </summary>
        public bool Iceberg { get; set; }
        /// <summary>
        /// The max visible size of the iceberg
        /// </summary>
        [JsonProperty("visibleSize")]
        public decimal? VisibleIcebergSize { get; set; }
        /// <summary>
        /// The source of the order
        /// </summary>
        public string Channel { get; set; } = string.Empty;
        /// <summary>
        /// Status
        /// </summary>
        [JsonConverter(typeof(BulkOrderCreationStatusConverter))]
        public BulkOrderCreationStatus Status { get; set; }
        /// <summary>
        /// The cause of failure
        /// </summary>
        public string? FailMsg { get; set; }
    }
}
