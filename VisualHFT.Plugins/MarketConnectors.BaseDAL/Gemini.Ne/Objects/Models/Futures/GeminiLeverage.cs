using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Leverage info
    /// </summary>
    public record GeminiLeverage
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Leverage
        /// </summary>
        [JsonPropertyName("leverage")]
        public decimal Leverage { get; set; }
    }


}
