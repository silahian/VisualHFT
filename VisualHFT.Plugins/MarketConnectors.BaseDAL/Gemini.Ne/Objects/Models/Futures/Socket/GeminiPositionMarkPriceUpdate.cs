using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures.Socket
{
    /// <summary>
    /// Position change caused by mark price
    /// </summary>
    public record GeminiPositionMarkPriceUpdate
    {
        /// <summary>
        /// Mark price
        /// </summary>
        public decimal MarkPrice { get; set; }
        /// <summary>
        /// Mark value
        /// </summary>
        public decimal MarkValue { get; set; }
        /// <summary>
        /// Maintenance margin
        /// </summary>
        [JsonProperty("maintMargin")]
        public decimal MaintenanceMargin { get; set; }
        /// <summary>
        /// Real leverage
        /// </summary>
        public decimal RealLeverage { get; set; }
        /// <summary>
        /// Unrealized profit and loss
        /// </summary>
        [JsonProperty("unrealisedPnl")]
        public decimal UnrealizedPnl { get; set; }
        /// <summary>
        /// Unrealized ROE
        /// </summary>
        [JsonProperty("unrealisedRoePcnt")]
        public decimal UnrealizedRoePercentage { get; set; }
        /// <summary>
        /// Unrealized profit and loss percentage
        /// </summary>
        [JsonProperty("unrealisedPnlPcnt")]
        public decimal UnrealizedPnlPercentage { get; set; }
        /// <summary>
        /// Adl ranking percentile
        /// </summary>
        [JsonProperty("delevPercentage")]
        public decimal DeleveragePercentage { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("currentTimestamp"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Currency used to clear and settle the trades
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleAsset { get; set; } = string.Empty;
    }
}
