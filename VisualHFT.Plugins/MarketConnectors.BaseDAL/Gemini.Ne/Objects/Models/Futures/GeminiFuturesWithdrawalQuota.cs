using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Withdrawal quota
    /// </summary>
    public record GeminiFuturesWithdrawalQuota
    {
        /// <summary>
        /// The asset the quota is for
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// The remaining quantity which can be withdrawn
        /// </summary>
        [JsonProperty("remainAmount")]
        public decimal QuantityRemaining { get; set; }
        /// <summary>
        /// 24h withdrawal limit
        /// </summary>
        [JsonProperty("limitAmount")]
        public decimal LimitQuantity { get; set; }
        /// <summary>
        /// 24h withdrawal limit
        /// </summary>
        [JsonProperty("usedAmount")]
        public decimal UsedQuantity { get; set; }
        /// <summary>
        /// The current quantity available for withdrawal
        /// </summary>
        [JsonProperty("availableAmount")]
        public decimal QuantityAvailable { get; set; }
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
    }
}
