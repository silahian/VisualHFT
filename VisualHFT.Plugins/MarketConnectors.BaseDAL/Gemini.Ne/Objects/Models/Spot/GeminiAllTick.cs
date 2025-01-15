using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Tick info
    /// </summary>
    public record GeminiAllTick
    {
        /// <summary>
        /// The symbol of the tick
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Name of trading pairs, it would change after renaming
        /// </summary>
        public string SymbolName { get; set; } = string.Empty;
        /// <summary>
        /// The best ask price
        /// </summary>
        [JsonProperty("sell")]
        public decimal? BestAskPrice { get; set; }
        /// <summary>
        /// The quantity of the best ask
        /// </summary>
        [JsonProperty("bestAskSize")]
        public decimal? BestAskQuantity { get; set; }
        /// <summary>
        /// The best bid price
        /// </summary>
        [JsonProperty("buy")]
        public decimal? BestBidPrice { get; set; }
        /// <summary>
        /// The quantity of the best bid
        /// </summary>
        [JsonProperty("bestBidSize")]
        public decimal? BestBidQuantity { get; set; }
        /// <summary>
        /// The percentage change
        /// </summary>
        [JsonProperty("changeRate")]
        public decimal? ChangePercentage { get; set; }
        /// <summary>
        /// The price change
        /// </summary>
        public decimal? ChangePrice { get; set; }
        /// <summary>
        /// The highest price
        /// </summary>
        [JsonProperty("high")]
        public decimal? HighPrice { get; set; }
        /// <summary>
        /// The lowest price
        /// </summary>
        [JsonProperty("low")]
        public decimal? LowPrice { get; set; }
        /// <summary>
        /// The volume in this tick
        /// </summary>
        [JsonProperty("vol")]
        public decimal? Volume { get; set; }
        /// <summary>
        /// The value of the volume in this tick
        /// </summary>
        [JsonProperty("volValue")]
        public decimal? QuoteVolume { get; set; }
        /// <summary>
        /// The last trade price
        /// </summary>
        [JsonProperty("last")]
        public decimal? LastPrice { get; set; }
        /// <summary>
        /// The average trade price in the last 24 hours
        /// </summary>
        public decimal? AveragePrice { get; set; }

        /// <summary>
        /// Basic Taker Fee
        /// </summary>
        public decimal? TakerFeeRate { get; set; }
        /// <summary>
        /// Basic Maker Fee
        /// </summary>
        public decimal? MakerFeeRate { get; set; }
        /// <summary>
        /// Taker Fee Coefficient
        /// </summary>
        public decimal? TakerCoefficient { get; set; }
        /// <summary>
        /// Maker Fee Coefficient
        /// </summary>
        public decimal? MakerCoefficient { get; set; }
    }
}
