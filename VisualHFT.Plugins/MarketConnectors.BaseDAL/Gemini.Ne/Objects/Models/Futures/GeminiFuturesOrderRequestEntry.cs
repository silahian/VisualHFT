using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Futures order request
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public record GeminiFuturesOrderRequestEntry
    {
        /// <summary>
        /// Client order id
        /// </summary>
        [JsonProperty("clientOid")]
        public string? ClientOrderId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Leverage
        /// </summary>
        [JsonProperty("leverage")]
        [JsonConverter(typeof(DecimalStringWriterConverter))]
        public decimal? Leverage { get; set; }
        /// <summary>
        /// Amount of contracts to buy or sell
        /// </summary>
        [JsonProperty("size")]
        public int? Quantity { get; set; }
        /// <summary>
        /// Limit price
        /// </summary>
        [JsonProperty("price")]
        [JsonConverter(typeof(DecimalStringWriterConverter))]
        public decimal? Price { get; set; }
        /// <summary>
        /// Time in force
        /// </summary>
        [JsonConverter(typeof(TimeInForceConverter))]
        [JsonProperty("timeInForce")]
        public TimeInForce? TimeInForce { get; set; }
        /// <summary>
        /// Order side
        /// </summary>
        [JsonConverter(typeof(OrderSideConverter))]
        [JsonProperty("side")]
        public OrderSide Side { get; set; }
        /// <summary>
        /// Order type
        /// </summary>
        [JsonConverter(typeof(NewOrderTypeConverter))]
        [JsonProperty("type")]
        public NewOrderType? OrderType { get; set; }
        /// <summary>
        /// Remark
        /// </summary>
        [JsonProperty("remark")]
        public string? Remark { get; set; }
        /// <summary>
        /// Stop type
        /// </summary>
        [JsonProperty("stop")]
        [JsonConverter(typeof(StopTypeConverter))]
        public StopType? Stop { get; set; }
        /// <summary>
        /// Stop price type
        /// </summary>
        [JsonProperty("stopPriceType")]
        [JsonConverter(typeof(StopPriceTypeConverter))]
        public StopPriceType? StopPriceType { get; set; }
        /// <summary>
        /// Stop price
        /// </summary>
        [JsonProperty("stopPrice")]
        [JsonConverter(typeof(DecimalStringWriterConverter))]
        public decimal? StopPrice { get; set; }
        /// <summary>
        /// Reduce only
        /// </summary>
        [JsonProperty("reduceOnly")]
        public bool? ReduceOnly { get; set; }
        /// <summary>
        /// Close order
        /// </summary>
        [JsonProperty("closeOrder")]
        public bool? CloseOrder { get; set; }
        /// <summary>
        /// Force hold
        /// </summary>
        [JsonProperty("forceHold")]
        public bool? ForceHold { get; set; }
        /// <summary>
        /// Post only
        /// </summary>
        [JsonProperty("postOnly")]
        public bool? PostOnly { get; set; }
        /// <summary>
        /// Is hidden
        /// </summary>
        [JsonProperty("hidden")]
        public bool? Hidden { get; set; }
        /// <summary>
        /// Is iceberg order
        /// </summary>
        [JsonProperty("iceberg")]
        public bool? Iceberg { get; set; }
        /// <summary>
        /// Visible size
        /// </summary>
        [JsonProperty("visibleSize")]
        public bool? VisibleSize { get; set; }
        /// <summary>
        /// Self trade prevention type
        /// </summary>
        [JsonProperty("stp")]
        [JsonConverter(typeof(EnumConverter))]
        public SelfTradePrevention? SelfTradePrevention { get; set; }
        /// <summary>
        /// Margin mode
        /// </summary>
        [JsonProperty("marginMode")]
        [JsonConverter(typeof(EnumConverter))]
        public FuturesMarginMode? MarginMode { get; set; }
    }
}
