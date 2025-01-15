namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Risk limit info
    /// </summary>
    public record GeminiRiskLimit
    {
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Level
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// Max risk limit
        /// </summary>
        public int MaxRiskLimit { get; set; }
        /// <summary>
        /// Min risk limit
        /// </summary>
        public int MinRiskLimit { get; set; }
        /// <summary>
        /// Max leverage
        /// </summary>
        public decimal MaxLeverage { get; set; }
        /// <summary>
        /// Initial margin
        /// </summary>
        public decimal InitialMargin { get; set; }
        /// <summary>
        /// Maintenance margin
        /// </summary>
        public decimal MaintainMargin { get; set; }
    }
}
