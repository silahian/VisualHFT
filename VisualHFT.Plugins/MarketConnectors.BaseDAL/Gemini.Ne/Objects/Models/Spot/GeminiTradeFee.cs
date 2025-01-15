namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Trade fee
    /// </summary>
    public record GeminiTradeFee
    {
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
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
