using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Withdrawal quota info
    /// </summary>
    public record GeminiWithdrawalQuota
    {
        /// <summary>
        /// The asset the quota is for
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// The max BTC value that can be withdrawn
        /// </summary>
        [JsonProperty("limitBTCAmount")]
        public decimal LimitBTCQuantity { get; set; }
        /// <summary>
        /// The used BTC value
        /// </summary>
        [JsonProperty("usedBTCAmount")]
        public decimal UsedBTCQuantity { get; set; }
        /// <summary>
        /// The remaining quantity which can be withdrawn
        /// </summary>
        [JsonProperty("remainAmount")]
        public decimal RemainingQuantity { get; set; }
        /// <summary>
        /// The current quantity available for withdrawal
        /// </summary>
        [JsonProperty("availableAmount")]
        public decimal AvailableQuantity { get; set; }
        /// <summary>
        /// The minimum fee for withdrawing
        /// </summary>
        public decimal WithdrawMinFee { get; set; }
        /// <summary>
        /// The minimum fee for an internal withdrawal
        /// </summary>
        public decimal InnerWithdrawMinFee { get; set; }
        /// <summary>
        /// The min quantity of a withdrawal
        /// </summary>
        [JsonProperty("withdrawMinSize")]
        public decimal WithdrawMinQuantity { get; set; }
        /// <summary>
        /// Whether withdrawing is enabled
        /// </summary>
        public bool IsWithdrawEnabled { get; set; }
        /// <summary>
        /// The precision of a withdrawal
        /// </summary>
        [JsonProperty("precision")]
        public int WithdrawPrecision { get; set; }
        /// <summary>
        /// The network
        /// </summary>
        [JsonProperty("chain")]
        public string Network { get; set; } = string.Empty;
        /// <summary>
        /// Withdrawal limit asset
        /// </summary>
        [JsonProperty("quotaCurrency")]
        public string QuotaAsset { get; set; } = string.Empty;
        /// <summary>
        /// The intraday available withdrawal amount
        /// </summary>
        [JsonProperty("limitQuotaCurrencyAmount")]
        public decimal LimitQuotaAssetQuantity { get; set; }
        /// <summary>
        /// The intraday used withdrawal amount
        /// </summary>
        [JsonProperty("usedQuotaCurrencyAmount")]
        public decimal UsedQuotaAssetQuantity { get; set; }
        /// <summary>
        /// Total locked amount
        /// </summary>
        [JsonProperty("lockedAmount")]
        public decimal LockedQuantity { get; set; }
        /// <summary>
        /// Reason
        /// </summary>
        [JsonProperty("reason")]
        public string? Reason { get; set; }
    }
}
