using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Futures account overview
    /// </summary>
    public record GeminiAccountOverview
    {
        /// <summary>
        /// Account equity = marginBalance + Unrealized PNL 
        /// </summary>
        public decimal AccountEquity { get; set; }
        /// <summary>
        /// Unrealized profit and loss
        /// </summary>
        [JsonProperty("unrealisedPNL")]
        public decimal UnrealizedPnl { get; set; }
        /// <summary>
        /// Margin balance = positionMargin + orderMargin + frozenFunds + availableBalance
        /// </summary>
        public decimal MarginBalance { get; set; }
        /// <summary>
        /// Position margin
        /// </summary>
        public decimal PositionMargin { get; set; }
        /// <summary>
        /// Order margin
        /// </summary>
        public decimal OrderMargin { get; set; }
        /// <summary>
        /// Frozen funds for withdrawal and out-transfer
        /// </summary>
        public decimal FrozenFunds { get; set; }
        /// <summary>
        /// Available balance
        /// </summary>
        public decimal AvailableBalance { get; set; }
        /// <summary>
        /// Asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
    }
}
