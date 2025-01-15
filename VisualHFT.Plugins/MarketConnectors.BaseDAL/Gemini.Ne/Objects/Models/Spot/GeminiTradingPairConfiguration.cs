using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Trading Pair Configuration
    /// </summary>
    public record GeminiTradingPairConfiguration
    {
        /// <summary>
        /// The Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// Symbol Name
        /// </summary>
        [JsonProperty("symbolName")]
        public string SymbolName { get; set; } = string.Empty;

        /// <summary>
        /// Base Asset
        /// </summary>
        [JsonProperty("baseCurrency")]
        public string BaseAsset { get; set; } = string.Empty;

        /// <summary>
        /// Quote Asset
        /// </summary>
        [JsonProperty("quoteCurrency")]
        public string QuoteAsset { get; set; } = string.Empty;

        /// <summary>
        /// Max Leverage
        /// </summary>
        [JsonProperty("maxLeverage")]
        public decimal MaxLeverage { get; set; }
        
        /// <summary>
        /// FlDebtRatio
        /// </summary>
        [JsonProperty("flDebtRatio")]
        public decimal FlDebtRatio { get; set; }

        /// <summary>
        /// Trade Enable
        /// </summary>
        [JsonProperty("tradeEnable")]
        public bool TradeEnable { get; set; }

        /// <summary>
        /// Auto Renew Max Debt Ratio
        /// </summary>
        [JsonProperty("autoRenewMaxDebtRatio")]
        public decimal AutoRenewMaxDebtRatio { get; set; }

        /// <summary>
        /// Base Borrow Enable
        /// </summary>
        [JsonProperty("baseBorrowEnable")]
        public bool BaseBorrowEnable { get; set; }

        /// <summary>
        /// Quote Borrow Enable
        /// </summary>
        [JsonProperty("quoteBorrowEnable")]
        public bool QuoteBorrowEnable { get; set; }

        /// <summary>
        /// Base Transfer In Enable
        /// </summary>
        [JsonProperty("baseTransferInEnable")]
        public bool BaseTransferInEnable { get; set; }

        /// <summary>
        /// Quote Transfer In Enable
        /// </summary>
        [JsonProperty("quoteTransferInEnable")]
        public bool QuoteTransferInEnable { get; set; }
    }
}
