namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Funding rate info
    /// </summary>
    public record GeminiFundingRate: GeminiIndexBase
    {
        /// <summary>
        /// Predicted value
        /// </summary>
        public decimal PredictedValue { get; set; }
    }
}
