using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models
{
    /// <summary>
    /// Trade info
    /// </summary>
    public record GeminiTradeBase
    {
        /// <summary>
        /// The symbol the fill is for
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The side of the fill
        /// </summary>
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }
        /// <summary>
        /// The price of the fill
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// The quantity of the fill
        /// </summary>
        [JsonProperty("size")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// The quantity of fee of the fill
        /// </summary>
        public decimal Fee { get; set; }
        /// <summary>
        /// The price of the fee
        /// </summary>
        [JsonProperty("feeRate")]
        public decimal FeePrice { get; set; }
        /// <summary>
        /// The asset of the fee
        /// </summary>
        [JsonProperty("feeCurrency")]
        public string FeeAsset { get; set; } = string.Empty;

        /// <summary>
        /// The time the fill was created
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("createdAt")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The id of the trade
        /// </summary>
        [JsonProperty("tradeId")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// The id of the order
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Maker or taker
        /// </summary>
        [JsonConverter(typeof(LiquidityTypeConverter))]
        public LiquidityType Liquidity { get; set; }
    }
}
