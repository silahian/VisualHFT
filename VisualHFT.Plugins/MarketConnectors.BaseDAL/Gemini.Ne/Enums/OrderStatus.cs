using CryptoExchange.Net.Attributes;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Order status
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order is active
        /// </summary>
        [Map("active", "open")]
        Active,
        /// <summary>
        /// Order is done
        /// </summary>
        [Map("done")]
        Done
    }
}
