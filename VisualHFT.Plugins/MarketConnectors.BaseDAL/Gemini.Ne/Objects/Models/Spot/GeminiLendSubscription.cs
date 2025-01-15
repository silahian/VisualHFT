using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Lend subscription info
    /// </summary>
    public record GeminiLendSubscription
    {
        /// <summary>
        /// Asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Purchase order id
        /// </summary>
        [JsonProperty("purchaseOrderNo")]
        public string PurchaseOrderId { get; set; } = string.Empty;
        /// <summary>
        /// Purchase quantity
        /// </summary>
        [JsonProperty("purchaseSize")]
        public decimal PurchaseQuantity { get; set; }
        /// <summary>
        /// Executed amount
        /// </summary>
        [JsonProperty("matchSize")]
        public decimal QuantityExecuted { get; set; }
        /// <summary>
        /// Redeemed amount
        /// </summary>
        [JsonProperty("redeemSize")]
        public decimal QuantityRedeemed { get; set; }
        /// <summary>
        /// Target annualized interest rate
        /// </summary>
        [JsonProperty("interestRate")]
        public decimal InterestRate { get; set; }
        /// <summary>
        /// Total earnings
        /// </summary>
        [JsonProperty("incomeSize")]
        public decimal TotalEarnings { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
        /// <summary>
        /// Apply time
        /// </summary>
        [JsonProperty("applyTime")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime ApplyTime { get; set; }

    }
}
