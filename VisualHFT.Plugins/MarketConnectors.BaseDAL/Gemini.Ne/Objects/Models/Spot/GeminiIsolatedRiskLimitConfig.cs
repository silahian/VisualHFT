using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Isolated margin risk limit and asset configuration
    /// </summary>
    public record GeminiIsolatedRiskLimitConfig
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Base asset max borrow quantity
        /// </summary>
        [JsonProperty("baseMaxBorrowAmount")]
        public decimal BaseBorrowMaxQuantity { get; set; }
        /// <summary>
        /// Base asset max buy quantity
        /// </summary>
        [JsonProperty("baseMaxBuyAmount")]
        public decimal BaseBuyMaxQuantity { get; set; }
        /// <summary>
        /// Base asset max hold quantity
        /// </summary>
        [JsonProperty("baseMaxHoldAmount")]
        public decimal BaseHoldMaxQuantity { get; set; }
        /// <summary>
        /// Base asset precision
        /// </summary>
        [JsonProperty("basePrecision")]
        public int BasePrecision { get; set; }
        /// <summary>
        /// Base asset min borrow quantity
        /// </summary>
        [JsonProperty("baseBorrowMinAmount")]
        public decimal? BaseBorrowMinQuantity { get; set; }
        /// <summary>
        /// Base asset minimum unit for borrowing
        /// </summary>
        [JsonProperty("baseBorrowMinUnit")]
        public decimal? BaseBorrowMinUnit { get; set; }
        /// <summary>
        /// Base asset borrow is enabled
        /// </summary>
        [JsonProperty("baseBorrowEnabled")]
        public bool BaseBorrowEnabled { get; set; }
        /// <summary>
        /// Quote asset max borrow quantity
        /// </summary>
        [JsonProperty("quoteMaxBorrowAmount")]
        public decimal QuoteBorrowMaxQuantity { get; set; }
        /// <summary>
        /// Quote asset max buy quantity
        /// </summary>
        [JsonProperty("quoteMaxBuyAmount")]
        public decimal QuoteBuyMaxQuantity { get; set; }
        /// <summary>
        /// Quote asset max hold quantity
        /// </summary>
        [JsonProperty("quoteMaxHoldAmount")]
        public decimal QuoteHoldMaxQuantity { get; set; }
        /// <summary>
        /// Quote asset precision
        /// </summary>
        [JsonProperty("quotePrecision")]
        public int QuotePrecision { get; set; }
        /// <summary>
        /// Quote asset min borrow quantity
        /// </summary>
        [JsonProperty("quoteBorrowMinAmount")]
        public decimal? QuoteBorrowMinQuantity { get; set; }
        /// <summary>
        /// Quote asset minimum unit for borrowing
        /// </summary>
        [JsonProperty("quoteBorrowMinUnit")]
        public decimal? QuoteBorrowMinUnit { get; set; }
        /// <summary>
        /// Quote asset borrow is enabled
        /// </summary>
        [JsonProperty("quoteBorrowEnabled")]
        public bool QuoteBorrowEnabled { get; set; }
    }
}
