using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Migration status
    /// </summary>
    public record GeminiMigrateStatus
    {
        /// <summary>
        /// Status of migration
        /// </summary>
        [JsonProperty("status")]
        public MigrateStatus Status { get; set; }
    }
}
