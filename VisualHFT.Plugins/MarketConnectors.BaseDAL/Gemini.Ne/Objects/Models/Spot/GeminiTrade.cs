using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Trade info
    /// </summary>
    public record GeminiTrade
    {
        /// <summary>
        /// The sequence number of the trade
        /// </summary>
        public long Sequence { get; set; }
        /// <summary>
        /// The price of the trade
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// The quantity of the trade
        /// </summary>
        [JsonProperty("size")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// The side of the trade
        /// </summary>
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }
        /// <summary>
        /// The timestamp of the trade
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter)), JsonProperty("time")]
        public DateTime Timestamp { get; set; }
    }
}
