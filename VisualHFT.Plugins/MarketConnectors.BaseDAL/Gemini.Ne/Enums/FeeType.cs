using CryptoExchange.Net.Attributes;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Type of fee paid
    /// </summary>
    public enum FeeType
    {
        /// <summary>
        /// Maker fee rate
        /// </summary>
        [Map("maker")]
        MakerFee,
        /// <summary>
        /// Taker fee rate
        /// </summary>
        [Map("takerFee")]
        TakerFee
    }
}
