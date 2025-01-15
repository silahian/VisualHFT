using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Asset info
    /// </summary>
    public record GeminiAssetBase
    {
        /// <summary>
        /// The asset identifier
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// The name of the asset
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// The full name of the asset
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// The precision of the asset
        /// </summary>
        public int Precision { get; set; }
        /// <summary>
        /// Number of block confirmations
        /// </summary>
        public int? Confirms { get; set; }
        /// <summary>
        /// Contract address
        /// </summary>
        public string ContractAddress { get; set; } = string.Empty;
        /// <summary>
        /// Is margin enabled
        /// </summary>
        public bool IsMarginEnabled { get; set; }
        /// <summary>
        /// Is debit enabled
        /// </summary>
        public bool IsDebitEnabled { get; set; }
    }

    /// <summary>
    /// Asset info
    /// </summary>
    public record GeminiAsset: GeminiAssetBase
    {        
        /// <summary>
        /// The minimum quantity of a withdrawal
        /// </summary>
        [JsonProperty("withdrawalMinSize")]
        public decimal WithdrawalMinQuantity { get; set; }
        /// <summary>
        /// The minimum fee of a withdrawal
        /// </summary>
        public decimal WithdrawalMinFee { get; set; }
        /// <summary>
        /// Is withdrawing enabled for this asset
        /// </summary>
        public bool IsWithdrawEnabled { get; set; }
        /// <summary>
        /// Is depositing enabled for this asset
        /// </summary>
        public bool IsDepositEnabled { get; set; }
    }

    /// <summary>
    /// Asset details
    /// </summary>
    public record GeminiAssetDetails: GeminiAssetBase
    {
        /// <summary>
        /// Networks
        /// </summary>
        [JsonProperty("chains")]
        public IEnumerable<GeminiAssetNetwork> Networks { get; set; } = Array.Empty<GeminiAssetNetwork>();
    }
}
