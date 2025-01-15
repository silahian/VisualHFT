using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Futures.Socket
{
    /// <summary>
    /// Order book change
    /// </summary>
    public record GeminiFuturesOrderBookChange
    {
        /// <summary>
        /// Sequence number
        /// </summary>
        public long Sequence { get; set; }
        [JsonProperty("change")]
        internal string Change { get; set; } = string.Empty;
        /// <summary>
        /// Price
        /// </summary>
        public decimal Price => decimal.Parse(Change.Split(',')[0]);
        /// <summary>
        /// Side
        /// </summary>
        public OrderSide Side => string.Equals(Change.Split(',')[1], "sell", StringComparison.Ordinal) ? OrderSide.Sell : OrderSide.Buy;
        /// <summary>
        /// Quantity
        /// </summary>
        public decimal Quantity => decimal.Parse(Change.Split(',')[2]);
    }
}
