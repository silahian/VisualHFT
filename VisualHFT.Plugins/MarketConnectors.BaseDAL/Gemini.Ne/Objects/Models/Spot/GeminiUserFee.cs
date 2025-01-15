namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// User fee
    /// </summary>
    public record GeminiUserFee
    {
        /// <summary>
        /// Fee rate for trades as taker
        /// </summary>
        public decimal TakerFeeRate { get; set; }
        /// <summary>
        /// Fee rate for trades as maker
        /// </summary>
        public decimal MakerFeeRate { get; set; }
    }
}
