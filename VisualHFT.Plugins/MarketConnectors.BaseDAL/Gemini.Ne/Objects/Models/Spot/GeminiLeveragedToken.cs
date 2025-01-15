using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Leveraged token info
    /// </summary>
    public record GeminiLeveragedToken
    {
        /// <summary>
        /// Asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Net worth
        /// </summary>
        [JsonProperty("netAsset")]
        public decimal NetWorth { get; set; }
        /// <summary>
        /// Target leverage
        /// </summary>
        [JsonProperty("targetLeverage")]
        public string TargetLeverage { get; set; } = string.Empty;
        /// <summary>
        /// Actual leverage
        /// </summary>
        [JsonProperty("actualLeverage")]
        public decimal ActualLeverage { get; set; }
        /// <summary>
        /// Assets under management
        /// </summary>
        [JsonProperty("assetsUnderManagement")]
        public string AssetsUnderManagement { get; set; } = string.Empty;
        /// <summary>
        /// Basket info
        /// </summary>
        [JsonProperty("basket")]
        public string Basket { get; set; } = string.Empty;
    }
}
