using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot.Socket
{
    /// <summary>
    /// Position status update
    /// </summary>
    public record GeminiMarginPositionStatusUpdate
    {
        /// <summary>
        /// Event type
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(EnumConverter))]
        public MarginEventType TotalDebt { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
