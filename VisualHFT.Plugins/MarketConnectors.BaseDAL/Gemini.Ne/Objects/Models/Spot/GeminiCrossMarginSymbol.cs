using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Models.Spot
{
    internal record GeminiCrossMarginSymbols
    {
        [JsonProperty("items")]
        public IEnumerable<GeminiCrossMarginSymbol> Items { get; set; } = Array.Empty<GeminiCrossMarginSymbol>();
    }

    /// <summary>
    /// Cross margin symbol
    /// </summary>
    public record GeminiCrossMarginSymbol
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Trading enabled
        /// </summary>
        [JsonProperty("enableTrading")]
        public bool TradingEnabled { get; set; }
        /// <summary>
        /// Market
        /// </summary>
        [JsonProperty("market")]
        public string Market { get; set; } = string.Empty;
        /// <summary>
        /// Base asset
        /// </summary>
        [JsonProperty("baseCurrency")]
        public string BaseAsset { get; set; } = string.Empty;
        /// <summary>
        /// Quote asset
        /// </summary>
        [JsonProperty("quoteCurrency")]
        public string QuoteAsset { get; set; } = string.Empty;
        /// <summary>
        /// Base asset step
        /// </summary>
        [JsonProperty("baseIncrement")]
        public decimal BaseAssetStep { get; set; }
        /// <summary>
        /// Minimal order quantity (base asset)
        /// </summary>
        [JsonProperty("baseMinSize")]
        public decimal MinOrderQuantity { get; set; }
        /// <summary>
        /// Quote asset step
        /// </summary>
        [JsonProperty("quoteIncrement")]
        public decimal QuoteAssetStep { get; set; }
        /// <summary>
        /// Minimal order value (quote asset)
        /// </summary>
        [JsonProperty("quoteMinSize")]
        public decimal MinQuoteOrderQuantity { get; set; }
        /// <summary>
        /// Max order quantity
        /// </summary>
        [JsonProperty("baseMaxSize")]
        public decimal MaxOrderQuantity { get; set; }
        /// <summary>
        /// Max order value (quote asset)
        /// </summary>
        [JsonProperty("quoteMaxSize")]
        public decimal MaxOrderValue { get; set; }
        /// <summary>
        /// Price step
        /// </summary>
        [JsonProperty("priceIncrement")]
        public decimal PriceStep { get; set; }
        /// <summary>
        /// Fee asset
        /// </summary>
        [JsonProperty("feeCurrency")]
        public string FeeAsset { get; set; } = string.Empty;
        /// <summary>
        /// Price protection threshold
        /// </summary>
        [JsonProperty("priceLimitRate")]
        public decimal PriceLimitRate { get; set; }
        /// <summary>
        /// Minimum trading amount
        /// </summary>
        [JsonProperty("minFunds")]
        public decimal MinOrderValue { get; set; }
    }

}
