using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Order id
    /// </summary>
    public record GeminiOrderId
    {
        /// <summary>
        /// The id of the order
        /// </summary>
        [JsonProperty("orderId")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// The client order id
        /// </summary>
        [JsonProperty("clientOid")]
        public string? ClientOrderId { get; set; }
    }
}
