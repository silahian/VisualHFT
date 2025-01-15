using CryptoExchange.Net.Attributes;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Isolated margin account status
    /// </summary>
    public enum IsolatedMargingAccountStatus
    {
        /// <summary>
        /// Existing liabilities
        /// </summary>
        [Map("DEBT")]
        Debt,
        /// <summary>
        /// No liabilities
        /// </summary>
        [Map("CLEAR")]
        Clear,
        /// <summary>
        /// Bankruptcy (after position enters a negative balance
        /// </summary>
        [Map("BANKRUPTCY")]
        Bankruptcy,
        /// <summary>
        /// Existing borrowings
        /// </summary>
        [Map("IN_BORROW")]
        InBorrow,
        /// <summary>
        /// Existing repayments
        /// </summary>
        [Map("IN_REPAY")]
        InRepay,
        /// <summary>
        /// Under liquidation
        /// </summary>
        [Map("IN_LIQUIDATION")]
        InLiquidation,
        /// <summary>
        /// Under auto-renewal assets
        /// </summary>
        [Map("IN_AUTO_RENEW ")]
        InAutoRenew
    }
}
