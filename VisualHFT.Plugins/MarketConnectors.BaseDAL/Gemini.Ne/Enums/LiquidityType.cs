namespace Gemini.Net.Enums
{
    /// <summary>
    /// Liquidity type of a trade
    /// </summary>
    public enum LiquidityType
    {
        /// <summary>
        /// Maker, order was on the order book and got filled
        /// </summary>
        Maker,
        /// <summary>
        /// Taker, trade filled an existing order on the order book
        /// </summary>
        Taker
    }
}
