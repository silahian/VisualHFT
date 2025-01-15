namespace Gemini.Net.Objects.Models.Spot.Socket
{
    /// <summary>
    /// Stream tick
    /// </summary>
    public record GeminiStreamTick: GeminiTick
    {
        /// <summary>
        /// The symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
    }
}
