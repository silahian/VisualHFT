using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures.Socket
{
    /// <summary>
    /// Futures stop order update
    /// </summary>
    public record GeminiStreamFuturesStopOrderUpdate: GeminiStreamStopOrderUpdateBase
    {
        /// <summary>
        /// Stop price type
        /// </summary>
        [JsonConverter(typeof(StopPriceTypeConverter))]
        public StopPriceType StopPriceType { get; set; }

        /// <summary>
        /// Error info if there was an error with the order
        /// </summary>
        public string? Error { get; set; }
    }
}
