using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Universal transfer
    /// </summary>
    public record GeminiUniversalTransfer
    {
        /// <summary>
        /// Orrder id
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; } = string.Empty;
    }
}
