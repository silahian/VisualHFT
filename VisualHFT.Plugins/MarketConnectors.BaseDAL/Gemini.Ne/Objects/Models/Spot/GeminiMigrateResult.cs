using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Migration result
    /// </summary>
    public record GeminiMigrateResult
    {
        /// <summary>
        /// Ids of user accounts successfully migrated
        /// </summary>
        [JsonProperty("successUsers")]
        public IEnumerable<string> SuccessUsers { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Ids of user accounts failed to migrate
        /// </summary>
        [JsonProperty("failUsers")]
        public Dictionary<string, string> FailedUsers { get; set; } = new Dictionary<string, string>();
    }
}
