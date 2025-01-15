using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// User info
    /// </summary>
    public record GeminiUserInfo
    {
        /// <summary>
        /// User level
        /// </summary>
        [JsonProperty("level")]
        public int Level { get; set; }
        /// <summary>
        /// Max number of default open sub-accounts (according to level)
        /// </summary>
        [JsonProperty("subQuantity")]
        public decimal SubQuantity { get; set; }
        /// <summary>
        /// Max number of default open sub-accounts (according to level)
        /// </summary>
        [JsonProperty("maxDefaultSubQuantity")]
        public decimal MaxDefaultSubQuantity { get; set; }
        /// <summary>
        /// Max number of sub-accounts = maxDefaultSubQuantity + maxSpotSubQuantity
        /// </summary>
        [JsonProperty("maxSubQuantity")]
        public decimal MaxSubQuantity { get; set; }
        /// <summary>
        /// Number of sub-accounts with spot trading permissions enabled
        /// </summary>
        [JsonProperty("spotSubQuantity")]
        public decimal SpotSubQuantity { get; set; }
        /// <summary>
        /// Number of sub-accounts with margin trading permissions enabled
        /// </summary>
        [JsonProperty("marginSubQuantity")]
        public decimal MarginSubQuantity { get; set; }
        /// <summary>
        /// Number of sub-accounts with futures trading permissions enabled
        /// </summary>
        [JsonProperty("futuresSubQuantity")]
        public decimal FuturesSubQuantity { get; set; }
        /// <summary>
        /// Max number of sub-accounts with additional Spot trading permissions
        /// </summary>
        [JsonProperty("maxSpotSubQuantity")]
        public decimal MaxSpotSubQuantity { get; set; }
        /// <summary>
        /// Max number of sub-accounts with additional margin trading permissions
        /// </summary>
        [JsonProperty("maxMarginSubQuantity")]
        public decimal MaxMarginSubQuantity { get; set; }
        /// <summary>
        /// Max number of sub-accounts with additional futures trading permissions
        /// </summary>
        [JsonProperty("maxFuturesSubQuantity")]
        public decimal MaxFuturesSubQuantity { get; set; }
    }
}
