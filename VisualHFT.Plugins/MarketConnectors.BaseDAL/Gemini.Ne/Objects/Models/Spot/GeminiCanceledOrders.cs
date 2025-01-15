using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Ids of cancelled orders
    /// </summary>
    public record GeminiCanceledOrders
    {
        /// <summary>
        /// List of canceled order ids
        /// </summary>
        public IEnumerable<string> CancelledOrderIds { get; set; } = Array.Empty<string>();
    }
}
