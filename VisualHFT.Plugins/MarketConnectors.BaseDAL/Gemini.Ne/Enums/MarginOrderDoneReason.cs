using CryptoExchange.Net.Attributes;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Margin order done reason
    /// </summary>
    public enum MarginOrderDoneReason
    {
        /// <summary>
        /// Filled
        /// </summary>
        [Map("filled")]
        Filled,
        /// <summary>
        /// Cancelled
        /// </summary>
        [Map("canceled")]
        Canceled
    }
}
