using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Trade info
    /// </summary>
    public record GeminiFuturesTrade
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
        /// Trade id
        /// </summary>
        [JsonProperty("tradeId")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Taker order id
        /// </summary>
        public string TakerOrderId { get; set; } = string.Empty;
        /// <summary>
        /// Maker order id
        /// </summary>
        public string MakerOrderId { get; set; } = string.Empty;
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("ts"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }
}
