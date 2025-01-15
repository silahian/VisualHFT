using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// API key info
    /// </summary>
    public record GeminiApiKey
    {
        /// <summary>
        /// Remark
        /// </summary>
        [JsonProperty("remark")]
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// Api key
        /// </summary>
        [JsonProperty("apiKey")]
        public string Apikey { get; set; } = string.Empty;
        /// <summary>
        /// Version of the API key
        /// </summary>
        [JsonProperty("apiVersion")]
        public int ApiKeyVersion { get; set; }
        /// <summary>
        /// Permissions, comma seperated
        /// </summary>
        [JsonProperty("permission")]
        public string Permissions { get; set; } = string.Empty;
        /// <summary>
        /// Creation time
        /// </summary>
        [JsonProperty("createdAt"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// User id
        /// </summary>
        [JsonProperty("uid")]
        public long UserId { get; set; }
        /// <summary>
        /// Is master account
        /// </summary>
        [JsonProperty("isMaster")]
        public bool IsMaster { get; set; }
    }
}
