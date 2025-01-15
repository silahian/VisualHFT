using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Cancel after setting
    /// </summary>
    public record GeminiCancelAfter
    {
        /// <summary>
        /// Current time
        /// </summary>
        [JsonProperty("currentTime")]
        public DateTime CurrentTime { get; set; }
        /// <summary>
        /// Trigger time
        /// </summary>
        [JsonProperty("triggerTime")]
        public DateTime TriggerTime { get; set; }
    }

    /// <summary>
    /// Cancel after status
    /// </summary>
    public record GeminiCancelAfterStatus : GeminiCancelAfter
    {
        /// <summary>
        /// Timeout in seconds
        /// </summary>
        [JsonProperty("timeout")]
        public int Timeout { get; set; }
        /// <summary>
        /// Symbols, comma separated
        /// </summary>
        [JsonProperty("symbols")]
        public string Symbols { get; set; } = string.Empty;
    }
}
