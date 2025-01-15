using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Sub user api key info
    /// </summary>
    public record GeminiSubUserKeyEdited
    {
        /// <summary>
        /// The sub user name
        /// </summary>
        [JsonProperty("subName")]
        public string SubName { get; set; } = string.Empty;
        /// <summary>
        /// The API key
        /// </summary>
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; } = string.Empty;
        /// <summary>
        /// Permissions
        /// </summary>
        [JsonProperty("permission")]
        public string? Permissions { get; set; }
        /// <summary>
        /// IP whitelist
        /// </summary>
        [JsonProperty("ipWhitelist")]
        public string? IpWhitelist { get; set; }       
    }
}
