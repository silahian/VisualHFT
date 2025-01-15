using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Max open size
    /// </summary>
    public record GeminiMaxOpenSize
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Max buy size
        /// </summary>
        [JsonProperty("maxBuyOpenSize")]
        public long MaxBuyOpenSize { get; set; }

        /// <summary>
        /// Max sell size
        /// </summary>
        [JsonProperty("maxSellOpenSize")]
        public long MaxSellOpenSize { get; set; }
    }
}
