using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Transferable Account info
    /// </summary>
    public record GeminiTransferableAccount
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
        /// The available balance of the account
        /// </summary>
        public decimal Available { get; set; }
        /// <summary>
        /// The quantity of balance that's in hold
        /// </summary>
        public decimal Holds { get; set; }
        /// <summary>
        /// The quantity of transferable balance
        /// </summary>
        public decimal Transferable { get; set; }
    }
}
