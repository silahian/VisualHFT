using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Sub user info
    /// </summary>
    public record GeminiSubUser
    {
        /// <summary>
        /// The sub user id
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; } = string.Empty;
        /// <summary>
        /// The uid
        /// </summary>
        [JsonProperty("uid")]
        public string Uid { get; set; } = string.Empty;
        /// <summary>
        /// The sub user name
        /// </summary>
        [JsonProperty("subName")]
        public string SubName { get; set; } = string.Empty;
        /// <summary>
        /// Status, 2: enabled, 3: frozen
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }
        /// <summary>
        /// Account type
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; set; }
        /// <summary>
        /// Remarks for this sub user
        /// </summary>
        [JsonProperty("remarks")]
        public string? Remarks { get; set; }
        /// <summary>
        /// Permissions
        /// </summary>
        public string Access { get; set; } = string.Empty;
        /// <summary>
        /// Key creation time
        /// </summary>
        [JsonProperty("createdAt"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreateTime { get; set; }
    }
}
