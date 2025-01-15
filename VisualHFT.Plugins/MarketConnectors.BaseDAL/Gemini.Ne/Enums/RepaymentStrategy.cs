using CryptoExchange.Net.Attributes;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Repayment strategy
    /// </summary>
    public enum RepaymentStrategy
    {
        /// <summary>
        /// Time priority, repay nearest maturity first
        /// </summary>
        [Map("RECENTLY_EXPIRE_FIRST")]
        RecentlyExpireFirst,
        /// <summary>
        /// Rate priority, repay highest interest rate first
        /// </summary>
        [Map("HIGHEST_RATE_FIRST")]
        HighestRateFirst
    }
}
