using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures.Socket
{
    /// <summary>
    /// Futures order update
    /// </summary>
    public record GeminiStreamFuturesOrderUpdate
    {
        /// <summary>
        /// Order id
        /// </summary>
        public string OrderId { get; set; } = string.Empty;
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The type of the update
        /// </summary>
        [JsonProperty("type"), JsonConverter(typeof(MatchUpdateTypeConverter))]
        public MatchUpdateType UpdateType { get; set; }
        /// <summary>
        /// Order status
        /// </summary>
        [JsonConverter(typeof(ExtendedOrderStatusConverter))]
        public ExtendedOrderStatus Status { get; set; }
        /// <summary>
        /// Match quantity (for match update types)
        /// </summary>
        [JsonProperty("matchSize")]
        public decimal? MatchQuantity { get; set; }
        /// <summary>
        /// Match price (for match update types)
        /// </summary>
        public decimal? MatchPrice { get; set; }
        /// <summary>
        /// Order type
        /// </summary>
        [JsonConverter(typeof(OrderTypeConverter))]
        public OrderType OrderType { get; set; }
        /// <summary>
        /// Order side
        /// </summary>
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }
        /// <summary>
        /// Price
        /// </summary>
        public decimal? Price { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonProperty("size")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Remaining quantity
        /// </summary>
        [JsonProperty("remainSize")]
        public decimal QuantityRemaining { get; set; }
        /// <summary>
        /// Filled quantity
        /// </summary>
        [JsonProperty("filledSize")]
        public decimal QuantityFilled { get; set; }
        /// <summary>
        /// Canceled quantity
        /// </summary>
        [JsonProperty("canceledSize")]
        public decimal QuantityCanceled { get; set; }
        /// <summary>
        /// Trade id
        /// </summary>
        public string? TradeId { get; set; }
        /// <summary>
        /// Client order id
        /// </summary>
        [JsonProperty("clientOid")]
        public string? ClientOrderId { get; set; }
        /// <summary>
        /// Order timestamp
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// Quantity before the update
        /// </summary>
        [JsonProperty("oldSize")]
        public decimal? OldQuantity { get; set; }
        /// <summary>
        /// Trade direction
        /// </summary>
        [JsonConverter(typeof(LiquidityTypeConverter))]
        public LiquidityType? Liquidity { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("ts")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Trade direction
        /// </summary>
        [JsonConverter(typeof(EnumConverter))]
        [JsonProperty("feeType")]
        public FeeType? FeeType { get; set; }

        /// <summary>
        /// Margin mode
        /// </summary>
        [JsonConverter(typeof(EnumConverter))]
        [JsonProperty("marginMode")]
        public FuturesMarginMode? MarginMode { get; set; }
    }
}
