using System;
using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures.Socket
{
    /// <summary>
    /// Wallet update
    /// </summary>
    public record GeminiStreamFuturesWalletUpdate
    {
        /// <summary>
        /// Asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Wallet balance
        /// </summary>
        [JsonProperty("walletBalance")]
        public decimal WalletBalance { get; set; }
        /// <summary>
        /// Available balance
        /// </summary>
        [JsonProperty("availableBalance")]
        public decimal AvailableBalance { get; set; }
        /// <summary>
        /// Hold balance
        /// </summary>
        [JsonProperty("holdBalance")]
        public decimal HoldBalance { get; set; }
        /// <summary>
        /// Isolated order margin
        /// </summary>
        [JsonProperty("isolatedOrderMargin")]
        public decimal IsolatedOrderMargin { get; set; }
        /// <summary>
        /// Isolated pos margin
        /// </summary>
        [JsonProperty("isolatedPosMargin")]
        public decimal IsolatedPosMargin { get; set; }
        /// <summary>
        /// Isolated unrealized profit and loss
        /// </summary>
        [JsonProperty("isolatedUnPnl")]
        public decimal IsolatedUnrealizedPnl { get; set; }
        /// <summary>
        /// Isolated funding fee margin
        /// </summary>
        [JsonProperty("isolatedFundingFeeMargin")]
        public decimal IsolatedFundingFeeMargin { get; set; }
        /// <summary>
        /// Cross order margin
        /// </summary>
        [JsonProperty("crossOrderMargin")]
        public decimal CrossOrderMargin { get; set; }
        /// <summary>
        /// Cross position margin
        /// </summary>
        [JsonProperty("crossPosMargin")]
        public decimal CrossPositionMargin { get; set; }
        /// <summary>
        /// Cross unrealized profit and loss
        /// </summary>
        [JsonProperty("crossUnPnl")]
        public decimal CrossUnrealizedPnl { get; set; }
        /// <summary>
        /// Equity
        /// </summary>
        [JsonProperty("equity")]
        public decimal Equity { get; set; }
        /// <summary>
        /// Total cross margin
        /// </summary>
        [JsonProperty("totalCrossMargin")]
        public decimal TotalCrossMargin { get; set; }
        /// <summary>
        /// Version
        /// </summary>
        [JsonProperty("version")]
        public long Version { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }


}
