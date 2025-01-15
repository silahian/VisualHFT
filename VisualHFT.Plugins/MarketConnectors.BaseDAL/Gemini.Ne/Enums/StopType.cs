namespace Gemini.Net.Enums
{
    /// <summary>
    /// Stop direction
    /// </summary>
    public enum StopType
    {
        /// <summary>
        /// Down, triggers when the price reaches or goes below the stopPrice
        /// </summary>
        Down,
        /// <summary>
        /// Up, triggers when the price reaches or goes above the stopPrice
        /// </summary>
        Up
    }
}
