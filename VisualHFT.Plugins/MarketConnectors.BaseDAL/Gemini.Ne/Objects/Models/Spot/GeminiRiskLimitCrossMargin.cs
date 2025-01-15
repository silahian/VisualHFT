using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Risk limit info
    /// </summary>
    public record GeminiRiskLimitCrossMargin
    {
        /// <summary>
        /// The asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;

        /// <summary>
        /// Max borrow quantity
        /// </summary>
        [JsonProperty("borrowMaxAmount")]
        public decimal BorrowMaxQuantity { get; set; }

        /// <summary>
        /// Max buy quantity
        /// </summary>
        [JsonProperty("buyMaxAmount")]
        public decimal BuyMaxQuantity { get; set; }

        /// <summary>
        /// Prevision
        /// </summary>
        public int Precision { get; set; }
    }
}
