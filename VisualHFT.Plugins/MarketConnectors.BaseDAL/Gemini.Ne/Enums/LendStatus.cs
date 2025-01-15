using CryptoExchange.Net.Attributes;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Lend order status
    /// </summary>
    public enum LendStatus
    {
        /// <summary>
        /// Filled
        /// </summary>
        [Map("FILLED")]
        Filled,
        /// <summary>
        /// Canceled
        /// </summary>
        [Map("CANCELED")]
        Canceled
    }
}
