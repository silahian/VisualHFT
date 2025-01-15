using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Service status
    /// </summary>
    public record GeminiFuturesServiceStatus
    {
        /// <summary>
        /// Service status
        /// </summary>
        [JsonConverter(typeof(ServiceStatusConverter))]
        public ServiceStatus Status { get; set; }
        /// <summary>
        /// Info
        /// </summary>
        [JsonProperty("msg")]
        public string Message { get; set; } = string.Empty;
    }
}
