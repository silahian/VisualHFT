using System;
using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Tick info
    /// </summary>
    public record GeminiTicks
    {
        /// <summary>
        /// The timestamp of the data
        /// </summary>
        [JsonProperty("time"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The ticker data
        /// </summary>
        [JsonProperty("ticker")]
        public IEnumerable<GeminiAllTick> Data { get; set; } = Array.Empty<GeminiAllTick>();
    }
}
