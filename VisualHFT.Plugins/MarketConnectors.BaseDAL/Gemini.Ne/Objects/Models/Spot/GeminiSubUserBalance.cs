using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Sub user balances
    /// </summary>
    public record GeminiSubUserBalances
    {
        /// <summary>
        /// The sub user id
        /// </summary>
        [JsonProperty("subUserId")]
        public string SubAccountId { get; set; } = string.Empty;
        /// <summary>
        /// The sub user name
        /// </summary>
        [JsonProperty("subName")]
        public string SubName { get; set; } = string.Empty;
        /// <summary>
        /// Main account balances
        /// </summary>
        [JsonProperty("mainAccounts")]
        public IEnumerable<GeminiSubUserBalance> MainAccounts { get; set; } = Array.Empty<GeminiSubUserBalance>();
        /// <summary>
        /// Trade account balances
        /// </summary>
        [JsonProperty("tradeAccounts")]
        public IEnumerable<GeminiSubUserBalance> TradeAccounts { get; set; } = Array.Empty<GeminiSubUserBalance>();
        /// <summary>
        /// Margin account balances
        /// </summary>
        [JsonProperty("marginAccounts")]
        public IEnumerable<GeminiSubUserBalance> MarginAccounts { get; set; } = Array.Empty<GeminiSubUserBalance>();
    }

    /// <summary>
    /// Sub user info
    /// </summary>
    public record GeminiSubUserBalance
    {
        /// <summary>
        /// Asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Available quantity
        /// </summary>
        [JsonProperty("available")]
        public decimal Available { get; set; }
        /// <summary>
        /// Total balance
        /// </summary>
        [JsonProperty("balance")]
        public decimal Total { get; set; }
        /// <summary>
        /// Frozen quantity
        /// </summary>
        [JsonProperty("holds")]
        public decimal Frozen { get; set; }
        /// <summary>
        /// Base asset
        /// </summary>
        [JsonProperty("baseCurrency")]
        public string BaseAsset { get; set; } = string.Empty;
        /// <summary>
        /// Base asset price
        /// </summary>
        [JsonProperty("baseCurrencyPrice")]
        public decimal BaseAssetPrice { get; set; }
        /// <summary>
        /// Base asset quantity
        /// </summary>
        [JsonProperty("baseAmount")]
        public decimal BaseAssetQuantity { get; set; }
    }
}
