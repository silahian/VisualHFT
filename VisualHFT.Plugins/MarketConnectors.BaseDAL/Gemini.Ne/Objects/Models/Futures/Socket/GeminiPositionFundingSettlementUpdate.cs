using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures.Socket
{
    /// <summary>
    /// Funding settlement update
    /// </summary>
    public record GeminiPositionFundingSettlementUpdate
    {
        /// <summary>
        /// Funding time
        /// </summary>
        [JsonProperty("fundingTime"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime FundTime { get; set; }
        /// <summary>
        /// Position size
        /// </summary>
        [JsonProperty("qty")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Settlement price
        /// </summary>
        public decimal MarkPrice { get; set; }
        /// <summary>
        /// Funding rate
        /// </summary>
        public decimal FundingRate { get; set; }
        /// <summary>
        /// Funding fee
        /// </summary>
        public decimal FundingFee { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("ts"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Asset used to clear and settle the trades
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleAsset { get; set; } = string.Empty;
    }
}
