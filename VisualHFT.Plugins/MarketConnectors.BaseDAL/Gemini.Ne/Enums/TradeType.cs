using CryptoExchange.Net.Attributes;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Type of trade
    /// </summary>
    public enum TradeType
    {
        /// <summary>
        /// Spot trade
        /// </summary>
        [Map("TRADE")]
        SpotTrade,
        /// <summary>
        /// Margin trade
        /// </summary>
        [Map("MARGIN_TRADE")]
        MarginTrade,
        /// <summary>
        /// Isolated margin trade
        /// </summary>
        [Map("MARGIN_ISOLATED_TRADE")]
        IsolatedMarginTrade
    }
}
