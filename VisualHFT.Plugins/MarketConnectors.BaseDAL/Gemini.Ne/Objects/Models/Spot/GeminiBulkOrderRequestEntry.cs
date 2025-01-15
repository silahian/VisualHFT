using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// The order model to be sent via bulk order endpoint
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record GeminiBulkOrderRequestEntry
    {
        /// <summary>
        /// The client order id
        /// </summary>
        [JsonProperty("clientOid")]
        public string ClientOrderId { get; set; } = string.Empty;
        /// <summary>
        /// The side of the order
        /// </summary>
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }
        /// <summary>
        /// The price of the order
        /// </summary>
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
        public NewOrderType Type { get; set; }

        /// <summary>
        /// Remark for the order
        /// </summary>
        public string? Remark { get; set; }
        /// <summary>
        /// The stop condition
        /// </summary>
        public StopCondition? Stop { get; set; }
        /// <summary>
        /// The stop price
        /// </summary>
        public decimal? StopPrice { get; set; }
        /// <summary>
        /// The self trade prevention type
        /// </summary>
        [JsonProperty("stp"), JsonConverter(typeof(SelfTradePreventionConverter))]
        public SelfTradePrevention? SelfTradePrevention { get; set; }
        /// <summary>
        /// Trade type
        /// </summary>
        [JsonConverter(typeof(TradeTypeConverter))]
        public TradeType? TradeType { get; set; }
        /// <summary>
        /// The time in force of the order
        /// </summary>
        [JsonConverter(typeof(TimeInForceConverter))]
        public TimeInForce? TimeInForce { get; set; }
        /// <summary>
        /// Time after which the order is canceled
        /// </summary>
        public int? CancelAfter { get; set; }
        /// <summary>
        /// Whether the order is post only
        /// </summary>
        public bool? PostOnly { get; set; }
        /// <summary>
        /// Whether the order is hidden
        /// </summary>
        public bool? Hidden { get; set; }
        /// <summary>
        /// Whether it is an iceberg order
        /// </summary>
        public bool? Iceberg { get; set; }
        /// <summary>
        /// The max visible size of the iceberg
        /// </summary>
        [JsonProperty("visibleSize")]
        public decimal? VisibleIcebergSize { get; set; }
    }
}
