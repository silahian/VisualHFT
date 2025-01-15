using CryptoExchange.Net.Attributes;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Withdrawal fee deduction type
    /// </summary>
    public enum FeeDeductType
    {
        /// <summary>
        /// Deduct the fee from the withdrawal amount
        /// </summary>
        [Map("INTERNAL")]
        Internal,
        /// <summary>
        /// Deduct the fee from main account
        /// </summary>
        [Map("EXTERNAL")]
        External
    }
}
