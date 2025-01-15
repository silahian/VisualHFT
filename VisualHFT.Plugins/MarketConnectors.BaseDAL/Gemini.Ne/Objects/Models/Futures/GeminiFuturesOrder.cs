using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Futures order info
    /// </summary>
    public record GeminiFuturesOrder: GeminiOrderBase
    {
        /// <summary>
        /// Value of the order
        /// </summary>
        [JsonProperty("value")]
        public decimal QuoteQantity { get; set; }
        /// <summary>
        /// Filled value
        /// </summary>
        [JsonProperty("dealValue")]
        public decimal? ExecutedValue { get; set; }
        /// <summary>
        /// Filled quantity
        /// </summary>
        [JsonProperty("dealSize")]
        public decimal? ExecutedQuantity { get; set; }
        /// <summary>
        /// Filled value
        /// </summary>
        [JsonProperty("filledValue")]
        public decimal QuoteQuantityFilled { get; set; }
        /// <summary>
        /// Filled quantity
        /// </summary>
        [JsonProperty("filledSize")]
        public decimal QuantityFilled { get; set; }
        /// <summary>
        /// The type of the stop order
        /// </summary>
        [JsonProperty("stop")]
        public StopCondition? StopOrderType { get; set; }
        /// <summary>
        /// Stop price type
        /// </summary>
        [JsonConverter(typeof(StopPriceTypeConverter))]
        public StopPriceType? StopPriceType { get; set; }
        /// <summary>
        /// Leverage
        /// </summary>
        public decimal Leverage { get; set; }
        /// <summary>
        /// Force hold
        /// </summary>
        public bool ForceHold { get; set; }
        /// <summary>
        /// Close order
        /// </summary>
        public bool CloseOrder { get; set; }
        /// <summary>
        /// Reduce only
        /// </summary>
        public bool ReduceOnly { get; set; }
        /// <summary>
        /// Settle asset
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleAsset { get; set; } = string.Empty;
        /// <summary>
        /// The time the order was last updated
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("updatedAt")]
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// Order create time
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("orderTime")]
        public DateTime? OrderTime { get; set; }
        /// <summary>
        /// End time
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("endAt")]
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [JsonConverter(typeof(EnumConverter))]
        public OrderStatus Status { get; set; }
        /// <summary>
        /// Tags
        /// </summary>
        [JsonProperty("tags")]
        public string? Tags { get; set; }
        /// <summary>
        /// Margin mode
        /// </summary>
        [JsonConverter(typeof(EnumConverter))]
        public FuturesMarginMode? MarginMode { get; set; }
        /// <summary>
        /// Average fill price
        /// </summary>
        [JsonProperty("avgDealPrice")]
        public decimal? AveragePrice { get; set; }
    }
}
