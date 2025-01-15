using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Transaction info
    /// </summary>
    public record GeminiTransactionVolume
    {
        /// <summary>
        /// Transaction volume in last 24h
        /// </summary>
        [JsonProperty("turnoverOf24h")]
        public decimal Turnover { get; set; }
    }
}
