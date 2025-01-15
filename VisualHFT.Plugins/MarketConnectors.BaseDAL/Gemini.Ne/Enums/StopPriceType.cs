namespace Gemini.Net.Enums
{
    /// <summary>
    /// Stop price trigger value
    /// </summary>
    public enum StopPriceType
    {
        /// <summary>
        /// Trigger on the last trade price
        /// </summary>
        TradePrice,
        /// <summary>
        /// Trigger on mark price
        /// </summary>
        MarkPrice,
        /// <summary>
        /// Trigger on index price
        /// </summary>
        IndexPrice
    }
}
