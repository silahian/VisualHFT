using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Margin accounts info
    /// </summary>
    public record GeminiIsolatedMarginAccountsInfo
    {
        /// <summary>
        /// The total balance of the isolated margin account (in the specified coin)
        /// </summary>
        public decimal TotalConversionBalance { get; set; }
        /// <summary>
        /// Total liabilities of the isolated margin account (in the specified coin)
        /// </summary>
        public decimal LiabilityConversionBalance { get; set; }
        /// <summary>
        /// Account list
        /// </summary>
        public IEnumerable<GeminiIsolatedMarginAccount> Assets { get; set; } = Array.Empty<GeminiIsolatedMarginAccount>();
    }

    /// <summary>
    /// Isolated margin account info
    /// </summary>
    public record GeminiIsolatedMarginAccount
    {
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Position status
        /// </summary>
        [JsonConverter(typeof(EnumConverter))]
        public IsolatedMargingAccountStatus Status { get; set; }
        /// <summary>
        /// Debt ratio
        /// </summary>
        public decimal DebtRatio { get; set; }
        /// <summary>
        /// Base asset info
        /// </summary>
        public GeminiIsolatedMarginAccountAsset BaseAsset { get; set; } = null!;
        /// <summary>
        /// Quote asset info
        /// </summary>
        public GeminiIsolatedMarginAccountAsset QuoteAsset { get; set; } = null!;
    }

    /// <summary>
    /// Isolate margin account asset info
    /// </summary>
    public record GeminiIsolatedMarginAccountAsset
    {
        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get; set; } = string.Empty;
        /// <summary>
        /// Total balance
        /// </summary>
        public decimal TotalBalance { get; set; }
        /// <summary>
        /// Frozen balance
        /// </summary>
        public decimal HoldBalance { get; set; }
        /// <summary>
        /// Available balance
        /// </summary>
        public decimal AvailableBalance { get; set; }
        /// <summary>
        /// Liability
        /// </summary>
        public decimal Liability { get; set; }
        /// <summary>
        /// Interset
        /// </summary>
        public decimal Interest { get; set; }
        /// <summary>
        /// Borrowable quantity
        /// </summary>
        [JsonProperty("borrowableAmount")]
        public decimal BorrowableQuantity { get; set; }
    }
}
