using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Index info
    /// </summary>
    public record GeminiIndex: GeminiIndexBase
    {
        /// <summary>
        /// Component list
        /// </summary>
        public IEnumerable<GeminiDecomposionItem> DecomposionList { get; set; } = Array.Empty<GeminiDecomposionItem>();
    }

    /// <summary>
    /// Decomposion item
    /// </summary>
    public record GeminiDecomposionItem
    {
        /// <summary>
        /// Exchange
        /// </summary>
        public string Exchange { get; set; } = string.Empty;
        /// <summary>
        /// Price
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Weight
        /// </summary>
        public decimal Weight { get; set; }
    }
}
