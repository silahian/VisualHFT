using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot.Socket
{
    /// <summary>
    /// Base record for a stream update
    /// </summary>
    public record GeminiStreamOrderBaseUpdate
    {
        /// <summary>
        /// The symbol of the update
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The timestamp of the event
        /// </summary>
        [JsonProperty("ts"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The type of the update
        /// </summary>
        [JsonProperty("type"), JsonConverter(typeof(MatchUpdateTypeConverter))]
        public MatchUpdateType? UpdateType { get; set; }
        /// <summary>
        /// The side of the order
        /// </summary>
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }
        /// <summary>
        /// The order id
        /// </summary>
        public string OrderId { get; set; } = string.Empty;
        /// <summary>
        /// The type of the order
        /// </summary>
        [JsonConverter(typeof(OrderTypeConverter))]
        public OrderType OrderType { get; set; }
        /// <summary>
        /// The price of the order
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// The client order id
        /// </summary>
        [JsonProperty("clientOid")]
        public string? ClientOrderid { get; set; }
        /// <summary>
        /// Order status
        /// </summary>
        [JsonConverter(typeof(ExtendedOrderStatusConverter))]
        public ExtendedOrderStatus? Status { get; set; }
        /// <summary>
        /// Order time
        /// </summary>
        [JsonProperty("orderTime"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime? OrderTime { get; set; }
        /// <summary>
        /// Origin quantity
        /// </summary>
        [JsonProperty("originSize")]
        public decimal OriginalQuantity { get; set; }
        /// <summary>
        /// Origin value
        /// </summary>
        [JsonProperty("originFunds")]
        public decimal OriginalValue { get; set; }
    }

    /// <summary>
    /// New order update
    /// </summary>
    public record GeminiStreamOrderNewUpdate : GeminiStreamOrderBaseUpdate
    {
    }
    
    /// <summary>
    /// Order update
    /// </summary>
    public record GeminiStreamOrderUpdate : GeminiStreamOrderBaseUpdate
    {
        /// <summary>
        /// The quantity of the order
        /// </summary>
        [JsonProperty("size")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Quantity before the update
        /// </summary>
        [JsonProperty("oldSize")]
        public decimal? OldQuantity { get; set; }
        /// <summary>
        /// Quantity filled
        /// </summary>
        [JsonProperty("filledSize")]
        public decimal QuantityFilled { get; set; }
        /// <summary>
        /// Quantity remaining
        /// </summary>
        [JsonProperty("remainSize")]
        public decimal QuantityRemaining { get; set; }
        /// <summary>
        /// Quantity remaining
        /// </summary>
        [JsonProperty("remainFunds")]
        public decimal? QuoteQuantityRemaining { get; set; }
        /// <summary>
        /// Quantity canceled
        /// </summary>
        [JsonProperty("canceledSize")]
        public decimal QuantityCanceled { get; set; }
        /// <summary>
        /// Value canceled
        /// </summary>
        [JsonProperty("canceledFunds")]
        public decimal ValueCanceled { get; set; }

    }

    /// <summary>
    /// Stream order update (match)
    /// </summary>
    public record GeminiStreamOrderMatchUpdate : GeminiStreamOrderUpdate
    {
        /// <summary>
        /// The trade id
        /// </summary>
        public string TradeId { get; set; } = string.Empty;
        /// <summary>
        /// The price of the match
        /// </summary>
        public decimal MatchPrice { get; set; }
        /// <summary>
        /// The quantity of the match
        /// </summary>
        [JsonProperty("matchSize")]
        public decimal MatchQuantity { get; set; }
        /// <summary>
        /// The liquidity
        /// </summary>
        [JsonConverter(typeof(LiquidityTypeConverter))]
        public LiquidityType Liquidity { get; set; }
        /// <summary>
        /// Type of fee paid
        /// </summary>
        [JsonProperty("feeType"), JsonConverter(typeof(EnumConverter))]
        public FeeType FeeType { get; set; }
    }
}
