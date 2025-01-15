using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Client order id
    /// </summary>
    public record GeminiClientOrderId
    {
        /// <summary>
        /// The client id of the order
        /// </summary>
        [JsonProperty("clientOid")]
        public string ClientOrderId { get; set; } = string.Empty;
    }
}
