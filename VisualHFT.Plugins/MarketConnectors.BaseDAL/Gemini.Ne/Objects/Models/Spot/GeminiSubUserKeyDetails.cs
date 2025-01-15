using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Sub user api key info
    /// </summary>
    public record GeminiSubUserKeyDetails
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
        /// The secret value
        /// </summary>
        [JsonProperty("apiSecret")]
        public string Secret { get; set; } = string.Empty;
        /// <summary>
        /// The passphrase/password
        /// </summary>
        [JsonProperty("passphrase")]
        public string Passphrase { get; set; } = string.Empty;
        /// <summary>
        /// Remark
        /// </summary>
        [JsonProperty("remark")]
        public string? Remark { get; set; }
        /// <summary>
        /// Permissions
        /// </summary>
        [JsonProperty("permission")]
        public string Permissions { get; set; } = string.Empty;
        /// <summary>
        /// IP whitelist
        /// </summary>
        [JsonProperty("ipWhitelist")]
        public string IpWhitelist { get; set; } = string.Empty;
        /// <summary>
        /// Key creation time
        /// </summary>
        [JsonProperty("createdAt"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreateTime { get; set; }
    }
}
