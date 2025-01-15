using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Lending result
    /// </summary>
    public record GeminiLendingResult
    {
        /// <summary>
        /// Order number
        /// </summary>
        [JsonProperty("orderNo")]
        public string OrderId { get; set; } = string.Empty;
    }
}
