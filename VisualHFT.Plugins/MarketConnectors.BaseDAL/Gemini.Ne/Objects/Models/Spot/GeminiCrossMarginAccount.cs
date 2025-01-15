using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Margin account info
    /// </summary>
    public record GeminiCrossMarginAccount
    {
        /// <summary>
        /// Total Assets in Quote Currency
        /// </summary>
        [JsonProperty("totalLiabilityOfQuoteCurrency")]
        public decimal TotalLiabilityOfQuoteAsset { get; set; }
        /// <summary>
        /// Total Liability in Quote Currency
        /// </summary>
        [JsonProperty("totalAssetOfQuoteCurrency")]
        public decimal TotalAssetOfQuoteAsset { get; set; }
        /// <summary>
        /// debt ratio
        /// </summary>
        [JsonProperty("debtRatio")]
        public decimal DebtRatio { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty("status")]
        public CrossMarginStatus Status { get; set; }
        /// <summary>
        /// Accounts
        /// </summary>
        [JsonProperty("accounts")] // API docs incorrectly has this as 'assets'
        public IEnumerable<GeminiCrossMarginAccountAsset> Accounts { get; set; } = Array.Empty<GeminiCrossMarginAccountAsset>();
    }

    /// <summary>
    /// Margin account asset info
    /// </summary>
    public record GeminiCrossMarginAccountAsset
    {
        /// <summary>
        /// Asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Support borrow or not
        /// </summary>
        [JsonProperty("borrowEnabled")]
        public bool BorrowEnabled { get; set; }
        /// <summary>
        /// Support repay or not
        /// </summary>
        [JsonProperty("repayEnabled")]
        public bool RepayEnabled { get; set; }
        /// <summary>
        /// Support transfer or not
        /// </summary>
        [JsonProperty("transferEnabled")]
        public bool TransferEnabled { get; set; }
        /// <summary>
        /// Borrowed
        /// </summary>
        [JsonProperty("borrowed")]
        public decimal Borrowed { get; set; }
        /// <summary>
        /// Total Assets
        /// </summary>
        [JsonProperty("totalAsset")]
        public decimal TotalAsset { get; set; }
        /// <summary>
        /// Account available assets (total assets - frozen)
        /// </summary>
        [JsonProperty("available")]
        public decimal Available { get; set; }
        /// <summary>
        /// Account frozen assets
        /// </summary>
        [JsonProperty("hold")]
        public decimal Hold { get; set; }
        /// <summary>
        /// The user's remaining maximum loan amount
        /// </summary>
        [JsonProperty("maxBorrowSize")]
        public decimal MaxBorrowQuantity { get; set; }
    }
}
