using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Cross margin risk limit and asset configuration
    /// </summary>
    public record GeminiCrossRiskLimitConfig
    {
        /// <summary>
        /// Asset name
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
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
        /// Max hold quantity
        /// </summary>
        [JsonProperty("holdMaxAmount")]
        public decimal HoldMaxQuantity { get; set; }
        /// <summary>
        /// Borrow coefficient
        /// </summary>
        [JsonProperty("borrowCoefficient")]
        public decimal BorrowCoefficient { get; set; }
        /// <summary>
        /// Margin coefficient
        /// </summary>
        [JsonProperty("marginCoefficient")]
        public decimal MarginCoefficient { get; set; }
        /// <summary>
        /// Asset precision
        /// </summary>
        [JsonProperty("precision")]
        public int Precision { get; set; }
        /// <summary>
        /// Min borrow quantity
        /// </summary>
        [JsonProperty("borrowMinAmount")]
        public decimal? BorrowMinQuantity { get; set; }
        /// <summary>
        /// Minimum unit for borrowing
        /// </summary>
        [JsonProperty("borrowMinUnit")]
        public decimal? BorrowMinUnit { get; set; }
        /// <summary>
        /// Borrow is enabled
        /// </summary>
        [JsonProperty("borrowEnabled")]
        public bool BorrowEnabled { get; set; }
    }
}
