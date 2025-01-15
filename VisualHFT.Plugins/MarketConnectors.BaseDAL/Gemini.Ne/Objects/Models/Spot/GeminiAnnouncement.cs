using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Announcement
    /// </summary>
    public record GeminiAnnouncement
    {
        /// <summary>
        /// Announcement id
        /// </summary>
        [JsonProperty("annId")]
        public long AnnouncementId { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        [JsonProperty("annTitle")]
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Description
        /// </summary>
        [JsonProperty("annDesc")]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Create timestamp
        /// </summary>
        [JsonProperty("cTime")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// Language
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; } = string.Empty;
        /// <summary>
        /// Url
        /// </summary>
        [JsonProperty("annUrl")]
        public string Url { get; set; } = string.Empty;
        /// <summary>
        /// Announcement types
        /// </summary>
        [JsonProperty("annType")]
        public IEnumerable<string> Types { get; set; } = Array.Empty<string>();
    }


}
