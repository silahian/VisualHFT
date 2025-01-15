using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Leverage update
    /// </summary>
    public record GeminiLeverageUpdate
    {
        /// <summary>
        /// Leverage value
        /// </summary>
        [JsonProperty("leverage")]
        public decimal Leverage { get; set; }
    }
}
