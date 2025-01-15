using Gemini.Net.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Margin mode info
    /// </summary>
    public record GeminiMarginMode
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Margin mode
        /// </summary>
        [JsonPropertyName("marginMode")]
        public FuturesMarginMode MarginMode { get; set; }
    }


}
