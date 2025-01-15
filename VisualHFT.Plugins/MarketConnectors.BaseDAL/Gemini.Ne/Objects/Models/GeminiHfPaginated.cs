using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models
{
    /// <summary>
    /// Page with results
    /// </summary>
    /// <typeparam name="T">Type of the items in the book</typeparam>
    public record GeminiHfPaginated<T>
    {
        /// <summary>
        /// The last result id
        /// </summary>
        [JsonProperty("lastId")]
        public long LastId { get; set; }
        /// <summary>
        /// The items on this page
        /// </summary>
        [JsonProperty("items")]
        public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    }
}
