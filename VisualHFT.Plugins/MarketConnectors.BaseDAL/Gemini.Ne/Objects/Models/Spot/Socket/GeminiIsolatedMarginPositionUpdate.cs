using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gemini.Net.Objects.Models.Spot.Socket
{
    /// <summary>
    /// Isolated margin position update
    /// </summary>
    public record GeminiIsolatedMarginPositionUpdate
    {
        /// <summary>
        /// Tag
        /// </summary>
        [JsonProperty("tag")]
        public string Tag { get; set; } = string.Empty;
        /// <summary>
        /// Accumelated principal
        /// </summary>
        [JsonProperty("accumulatedPrincipal")]
        public decimal AccumelatedPrincipal { get; set; }
        /// <summary>
        /// Data timestamp
        /// </summary>
        [JsonProperty("timestamp"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Position status
        /// </summary>
        [JsonProperty("status"), JsonConverter(typeof(EnumConverter))]
        public IsolatedPositionStatus Status { get; set; }
        /// <summary>
        /// Changed assets
        /// </summary>
        [JsonProperty("changeAssets")]
        public Dictionary<string, GeminiIsolatedMarginAsset> ChangedAssets { get; set; } = new Dictionary<string, GeminiIsolatedMarginAsset>();

    }

    /// <summary>
    /// Isolated margin asset info
    /// </summary>
    public record GeminiIsolatedMarginAsset
    {
        /// <summary>
        /// Total asset
        /// </summary>
        [JsonProperty("total")]
        public decimal Total { get; set; }
        /// <summary>
        /// Frozen asset
        /// </summary>
        [JsonProperty("hold")]
        public decimal Hold { get; set; }
        /// <summary>
        /// Liability principal
        /// </summary>
        [JsonProperty("liabilityPrincipal")]
        public decimal LiabilityPrincipal { get; set; }
        /// <summary>
        /// Liability interest
        /// </summary>
        [JsonProperty("liabilityInterest")]
        public decimal LiabilityInterest { get; set; }
    }
}
