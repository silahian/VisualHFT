using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Order value info
    /// </summary>
    public record GeminiOrderValuation
    {
        /// <summary>
        /// Total number of the unexecuted buy orders
        /// </summary>
        public int OpenOrderBuySize { get; set; }
        /// <summary>
        /// Total number of the unexecuted sell orders
        /// </summary>
        public int OpenOrderSellSize { get; set; }
        /// <summary>
        /// Value of all the unexecuted buy orders
        /// </summary>
        public decimal OpenOrderBuyCost { get; set; }
        /// <summary>
        /// Value of all the unexecuted sell orders
        /// </summary>
        public decimal OpenOrderSellCost { get; set; }
        /// <summary>
        /// settlement asset
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleAsset { get; set; } = string.Empty;
    }
}
