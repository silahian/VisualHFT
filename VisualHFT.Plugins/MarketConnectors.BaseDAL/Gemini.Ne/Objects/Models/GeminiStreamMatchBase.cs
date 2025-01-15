using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models
{
    /// <summary>
    /// Match info
    /// </summary>
    public record GeminiStreamMatchBase
    {
        /// <summary>
        /// The sequence of the match
        /// </summary>
        public long Sequence { get; set; }
        /// <summary>
        /// The symbol the match is for
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The side of the match
        /// </summary>
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }
        /// <summary>
        /// The quantity
        /// </summary>
        [JsonProperty("size")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// The price of the match
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// The taker order id
        /// </summary>
        public string TakerOrderId { get; set; } = string.Empty;
        /// <summary>
        /// The maker order id
        /// </summary>
        public string MakerOrderId { get; set; } = string.Empty;
        /// <summary>
        /// The id of the trade
        /// </summary>
        [JsonProperty("tradeId")]
        public string Id { get; set; } = string.Empty;

    }
}
