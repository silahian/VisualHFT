using CryptoExchange.Net.Attributes;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// OCO order status
    /// </summary>
    public enum OcoOrderStatus
    {
        /// <summary>
        /// New
        /// </summary>
        [Map("NEW")]
        New,
        /// <summary>
        /// Done
        /// </summary>
        [Map("DONE")]
        Done,
        /// <summary>
        /// Triggered
        /// </summary>
        [Map("TRIGGERED")]
        Triggered,
        /// <summary>
        /// Cancelled
        /// </summary>
        [Map("CANCELLED")]
        Canceled
    }
}
