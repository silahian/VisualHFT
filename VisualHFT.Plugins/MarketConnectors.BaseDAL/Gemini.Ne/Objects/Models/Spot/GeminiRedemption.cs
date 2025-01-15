using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Redemption record
    /// </summary>
    public record GeminiRedemption
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
        /// Redeem order id
        /// </summary>
        [JsonProperty("redeemOrderNo")]
        public string RedeemOrderId { get; set; } = string.Empty;
        /// <summary>
        /// Redeem quantity
        /// </summary>
        [JsonProperty("redeemSize")]
        public decimal RedeemQuantity { get; set; }
        /// <summary>
        /// Redeemed quantity
        /// </summary>
        [JsonProperty("receiptSize")]
        public decimal ReceiptQuantity { get; set; }
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
