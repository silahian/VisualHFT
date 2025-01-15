using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// New Borrow order
    /// </summary>
    public record GeminiNewBorrowOrder
    {
        /// <summary>
        /// The id of the new borrow order
        /// </summary>
        [JsonProperty("orderNo")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Actual borrowed quantity
        /// </summary>
        [JsonProperty("actualSize")]
        public decimal BorrowedQuantity { get; set; }
    }
}
