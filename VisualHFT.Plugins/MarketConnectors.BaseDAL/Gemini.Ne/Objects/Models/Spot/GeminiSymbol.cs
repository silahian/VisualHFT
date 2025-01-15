using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Symbol info
    /// </summary>
    public record GeminiSymbol
    {
        /// <summary>
        /// The symbol identifier
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The name of the symbol
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// The market the symbol is on
        /// </summary>
        public string Market { get; set; } = string.Empty;
        /// <summary>
        /// The base asset
        /// </summary>
        [JsonProperty("baseCurrency")]
        public string BaseAsset { get; set; } = string.Empty;
        /// <summary>
        /// The quote asset
        /// </summary>
        [JsonProperty("quoteCurrency")]
        public string QuoteAsset { get; set; } = string.Empty;
        /// <summary>
        /// The min order quantity in the base asset
        /// </summary>
        [JsonProperty("baseMinSize")]
        public decimal BaseMinQuantity { get; set; }
        /// <summary>
        /// The min order quantity in the quote asset
        /// </summary>
        [JsonProperty("quoteMinSize")]
        public decimal QuoteMinQuantity { get; set; }
        /// <summary>
        /// The max order quantity in the base asset
        /// </summary>
        [JsonProperty("baseMaxSize")]
        public decimal BaseMaxQuantity { get; set; }
        /// <summary>
        /// The max order quantity in the quote asset
        /// </summary>
        [JsonProperty("quoteMaxSize")]
        public decimal QuoteMaxQuantity { get; set; }
        /// <summary>
        /// The quantity of an order when using the quantity field must be a multiple of this
        /// </summary>
        public decimal BaseIncrement { get; set; }
        /// <summary>
        /// The funds of an order when using the funds field must be a multiple of this
        /// </summary>
        public decimal QuoteIncrement { get; set; }
        /// <summary>
        /// The price of an order must be a multiple of this
        /// </summary>
        public decimal PriceIncrement { get; set; }
        /// <summary>
        /// The price limit rate
        /// </summary>
        public decimal PriceLimitRate { get; set; }
        /// <summary>
        /// The asset the fee will be on
        /// </summary>
        [JsonProperty("feeCurrency")]
        public string FeeAsset { get; set; } = string.Empty;
        /// <summary>
        /// Whether margin is enabled
        /// </summary>
        public bool IsMarginEnabled { get; set; }
        /// <summary>
        /// Whether trading is enabled
        /// </summary>
        public bool EnableTrading { get; set; }
        /// <summary>
        /// Minimum spot and margin trade amounts
        /// </summary>
        public decimal? MinFunds { get; set; }
    }
}
