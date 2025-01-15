using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Risk limit info
    /// </summary>
    public record GeminiRiskLimitIsolatedMargin
    {
        /// <summary>
        /// The Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// Max borrow quantity
        /// </summary>
        [JsonProperty("baseMaxBorrowAmount")]
        public decimal BaseMaxBorrowQuantity { get; set; }

        /// <summary>
        /// Max borrow quantity
        /// </summary>
        [JsonProperty("quoteMaxBorrowAmount")]
        public decimal QuoteMaxBorrowQuantity { get; set; }

        /// <summary>
        /// BaseMax buy quantity
        /// </summary>
        [JsonProperty("baseMaxBuyAmount")]
        public decimal BaseMaxBuyQuantity { get; set; }

        /// <summary>
        /// Quote Max buy quantity
        /// </summary>
        [JsonProperty("quoteMaxBuyAmount")]
        public decimal QuoteMaxBuyQuantity { get; set; }

        /// <summary>
        /// Base Precision
        /// </summary>
        ///         
        [JsonProperty("basePrecision")]
        public int BasePrecision { get; set; }

        /// <summary>
        /// Quote Precision
        /// </summary>
        ///         
        [JsonProperty("quotePrecision")]
        public int QuotePrecision { get; set; }
    }
}
