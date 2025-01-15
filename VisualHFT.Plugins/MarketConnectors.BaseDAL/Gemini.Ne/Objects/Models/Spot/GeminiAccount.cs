using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Account info
    /// </summary>
    public record GeminiAccount
    {
        /// <summary>
        /// The id of the account
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// The asset of the account
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// The type of the account
        /// </summary>
        [JsonConverter(typeof(AccountTypeConverter))]
        public AccountType Type { get; set; }
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
    }
}
