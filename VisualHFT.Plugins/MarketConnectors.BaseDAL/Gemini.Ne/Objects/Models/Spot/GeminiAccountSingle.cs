using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Account info
    /// </summary>
    public record GeminiAccountSingle
    {
        /// <summary>
        /// The asset of the account
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// The total balance of the account
        /// </summary>
        [JsonProperty("balance")]
        public decimal Total { get; set; }
        /// <summary>
        /// the available quantity in the account
        /// </summary>
        public decimal Available { get; set; }
        /// <summary>
        /// The quantity in hold of the account
        /// </summary>
        public decimal Holds { get; set; }
    }
}
