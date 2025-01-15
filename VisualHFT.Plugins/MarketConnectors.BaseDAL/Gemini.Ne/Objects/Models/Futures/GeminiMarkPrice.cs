namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Mark price
    /// </summary>
    public record GeminiMarkPrice: GeminiIndexBase
    {        
        /// <summary>
        /// Index price
        /// </summary>
        public decimal IndexPrice { get; set; }
    }
}
