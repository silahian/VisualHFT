using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models
{
    /// <summary>
    /// Stop order update
    /// </summary>
    public record GeminiStreamStopOrderUpdateBase
    {
        /// <summary>
        /// Order side
        /// </summary>
        [JsonProperty("side")]
        public OrderSide OrderSide { get; set; }

        /// <summary>
        /// Creation time
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("createdAt")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// Order id
        /// </summary>
        [JsonProperty("orderId")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Order price
        /// </summary>
        public decimal? OrderPrice { get; set; }
        /// <summary>
        /// Order type
        /// </summary>
        [JsonConverter(typeof(OrderTypeConverter))]
        public OrderType OrderType { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonProperty("size")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Stop
        /// </summary>
        public StopCondition Stop { get; set; }
        /// <summary>
        /// Stop price
        /// </summary>
        public decimal StopPrice { get; set; }
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Trade type
        /// </summary>
        [JsonConverter(typeof(TradeTypeConverter))]
        public TradeType TradeType { get; set; }
        /// <summary>
        /// Trigger was success
        /// </summary>
        public bool TriggerSuccess { get; set; }
        /// <summary>
        /// Update timestamp
        /// </summary>
        [JsonProperty("ts"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Update type
        /// </summary>
        public StopOrderEvent Type { get; set; }

        /// <summary>
        /// Margin mode
        /// </summary>
        [JsonConverter(typeof(EnumConverter))]
        [JsonProperty("marginMode")]
        public FuturesMarginMode? MarginMode { get; set; }
    }
}
