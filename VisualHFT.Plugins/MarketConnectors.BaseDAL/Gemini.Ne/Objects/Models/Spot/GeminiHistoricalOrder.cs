using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Historical order info
    /// </summary>
    public record GeminiHistoricalOrder
    {
        /// <summary>
        /// The id of the order
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// The symbol of the order
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The price of the order
        /// </summary>
        [JsonProperty("dealPrice")]
        public decimal Price { get; set; }
        /// <summary>
        /// The value of the order
        /// </summary>
        [JsonProperty("dealValue")]
        public decimal QuoteQuantity { get; set; }
        /// <summary>
        /// The quantity of the order
        /// </summary>
        [JsonProperty("amount")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// The fee of the order
        /// </summary>
        public decimal Fee { get; set; }
        /// <summary>
        /// The side of the order
        /// </summary>        
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }
        /// <summary>
        /// The time the order was created
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("createdAt")]
        public DateTime CreateTime { get; set; }
    }
}
