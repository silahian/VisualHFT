namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Sub transfer info
    /// </summary>
    public record GeminiInnerTransfer
    {
        /// <summary>
        /// The id of the new sub transfer
        /// </summary>
        public string OrderId { get; set; } = string.Empty;
    }
}
