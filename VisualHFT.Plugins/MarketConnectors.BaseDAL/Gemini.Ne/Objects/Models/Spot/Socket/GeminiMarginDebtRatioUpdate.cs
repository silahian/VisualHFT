using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Models.Spot.Socket
{
    /// <summary>
    /// Debt ratio update
    /// </summary>
    public record GeminiMarginDebtRatioUpdate
    {
        /// <summary>
        /// Debt ratio
        /// </summary>
        [JsonProperty("debtRatio")]
        public decimal DebtRatio { get; set; }
        /// <summary>
        /// Total debt in BTC
        /// </summary>
        [JsonProperty("totalDebt")]
        public decimal TotalDebt { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Debt list
        /// </summary>
        [JsonProperty("debtList")]
        public Dictionary<string, decimal> Debts { get; set; } = new Dictionary<string, decimal>();
    }
}
