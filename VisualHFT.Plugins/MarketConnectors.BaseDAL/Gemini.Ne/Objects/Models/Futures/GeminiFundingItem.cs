using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Funding info
    /// </summary>
    public record GeminiFundingItem
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Time point
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime TimePoint { get; set; }
        /// <summary>
        /// Funding rate
        /// </summary>
        public decimal FundingRate { get; set; }
        /// <summary>
        /// Mark price
        /// </summary>
        public decimal MarkPrice { get; set; }
        /// <summary>
        /// Position quantity
        /// </summary>
        [JsonProperty("positionQty")]
        public decimal PositionQuantity { get; set; }
        /// <summary>
        /// Position value at settlement period
        /// </summary>
        public decimal PositionCost { get; set; }
        /// <summary>
        /// Settled funding fees. A positive number means that the user received the funding fee, and vice versa.
        /// </summary>
        public decimal Funding { get; set; }
        /// <summary>
        /// Settlement asset
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleAsset { get; set; } = string.Empty;
    }
}
