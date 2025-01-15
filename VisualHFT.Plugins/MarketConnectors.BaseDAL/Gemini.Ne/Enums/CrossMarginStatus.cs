using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Cross margin status
    /// </summary>
    [JsonConverter(typeof(EnumConverter))]
    public enum CrossMarginStatus
    {
        /// <summary>
        /// Effective
        /// </summary>
        [Map("EFFECTIVE")]
        Effective,
        /// <summary>
        /// Bankruptcy liquidation
        /// </summary>
        [Map("BANKRUPTCY")]
        BankruptcyLiquidation,
        /// <summary>
        /// Closing
        /// </summary>
        [Map("LIQUIDATION")]
        Closing,
        /// <summary>
        /// Repaying
        /// </summary>
        [Map("REPAY")]
        Repaying,
        /// <summary>
        /// Borrowing
        /// </summary>
        [Map("BORROW")]
        Borrowing
    }
}
