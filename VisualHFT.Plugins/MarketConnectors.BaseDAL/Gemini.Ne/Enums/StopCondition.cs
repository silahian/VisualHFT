using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Stop condition
    /// </summary>
    [JsonConverter(typeof(EnumConverter))]
    public enum StopCondition
    {
        /// <summary>
        /// No stop condition
        /// </summary>
        [Map("")]
        None,
        /// <summary>
        /// Loss condition, triggers when the last trade price changes to a value at or below the stopPrice.
        /// </summary>
        [Map("loss", "down")]
        Loss,
        /// <summary>
        /// Entry condition, triggers when the last trade price changes to a value at or above the stopPrice.
        /// </summary>
        [Map("entry", "up")]
        Entry
    }
}
