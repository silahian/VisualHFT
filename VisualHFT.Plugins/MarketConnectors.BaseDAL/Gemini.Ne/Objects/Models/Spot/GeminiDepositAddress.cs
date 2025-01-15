using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Deposit address
    /// </summary>
    public record GeminiDepositAddress
    {
        /// <summary>
        /// The address
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// A memo for the address
        /// </summary>
        public string Memo { get; set; } = string.Empty;

        /// <summary>
        /// The chain of the address
        /// </summary>
        [JsonProperty("chain")]
        public string Network { get; set; } = string.Empty;

        /// <summary>
        /// The token contract address
        /// </summary>
        public string ContractAddress { get; set; } = string.Empty;
    }
}
