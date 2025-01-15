using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Lending asset
    /// </summary>
    public record GeminiLendingAsset
    {
        /// <summary>
        /// Asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Is purchasing enabled
        /// </summary>
        [JsonProperty("purchaseEnable")]
        public bool PurchaseEnabled { get; set; }
        /// <summary>
        /// Is redeeming enabled
        /// </summary>
        [JsonProperty("redeemEnable")]
        public bool RedeemEnabled { get; set; }
        /// <summary>
        /// Increment precision for subscription and redemption
        /// </summary>
        [JsonProperty("increment")]
        public decimal Increment { get; set; }
        /// <summary>
        /// Minimal purchase quantity
        /// </summary>
        [JsonProperty("minPurchaseSize")]
        public decimal MinPurchaseQuantity { get; set; }
        /// <summary>
        /// Minimal interest rate
        /// </summary>
        [JsonProperty("minInterestRate")]
        public decimal MinInterestRate { get; set; }
        /// <summary>
        /// Max interest rate
        /// </summary>
        [JsonProperty("maxInterestRate")]
        public decimal MaxInterestRate { get; set; }
        /// <summary>
        /// Interest precision
        /// </summary>
        [JsonProperty("interestIncrement")]
        public decimal InterestIncrement { get; set; }
        /// <summary>
        /// Max purchase quantity
        /// </summary>
        [JsonProperty("maxPurchaseSize")]
        public decimal MaxPurchaseQuantity { get; set; }
        /// <summary>
        /// Latest market annualized interest rate
        /// </summary>
        [JsonProperty("marketInterestRate")]
        public decimal MarketInterestRate { get; set; }
        /// <summary>
        /// Is Auto-Subscribe enabled
        /// </summary>
        [JsonProperty("autoPurchaseEnable")]
        public bool AutoSubscribeEnabled { get; set; }
    }
}
