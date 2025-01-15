using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Network details
    /// </summary>
    public record GeminiAssetNetwork
    {
        /// <summary>
        /// Network id
        /// </summary>
        [JsonProperty("chainId")]
        public string NetworkId { get; set; } = string.Empty;
        /// <summary>
        /// Network name
        /// </summary>
        [JsonProperty("chainName")]
        public string NetworkName { get; set; } = string.Empty;
        /// <summary>
        /// Min withdrawal quantity
        /// </summary>
        [JsonProperty("withdrawalMinSize")]
        public decimal WithdrawalMinQuantity { get; set; }
        /// <summary>
        /// Min withdrawal fee
        /// </summary>
        public decimal WithdrawalMinFee { get; set; }
        /// <summary>
        /// Is withdrawing enabled
        /// </summary>
        public bool IsWithdrawEnabled { get; set; }
        /// <summary>
        /// Is deposit enabled
        /// </summary>
        public bool IsDepositEnabled { get; set; }
        /// <summary>
        /// Confirmations needed on the network
        /// </summary>
        public int? Confirms { get; set; }
        /// <summary>
        /// The number of blocks (confirmations) for advance on-chain verification
        /// </summary>
        public int? Preconfirms { get; set; }
        /// <summary>
        /// Contract address
        /// </summary>
        public string ContractAddress { get; set; } = string.Empty;
        /// <summary>
        /// Deposit fee rate
        /// </summary>
        public decimal? DepositFeeRate { get; set; }
        /// <summary>
        /// Min deposit quantity
        /// </summary>
        [JsonProperty("depositMinSize")]
        public decimal? DepositMinQuantity { get; set; }
        /// <summary>
        /// Withdrawal fee rate
        /// </summary>
        [JsonProperty("withdrawFeeRate")]
        public decimal? WithdrawFeeRate { get; set; }
        /// <summary>
        /// Withdraw max fee
        /// </summary>
        [JsonProperty("withdrawMaxFee")]
        public decimal? WithdrawMaxFee { get; set; }
        /// <summary>
        /// Max deposit quantity (only for lightning network)
        /// </summary>
        [JsonProperty("maxDeposit")]
        public decimal? MaxDeposit { get; set; }
        /// <summary>
        /// Maximum amount of single withdrawal
        /// </summary>
        [JsonProperty("maxWithdraw")]
        public decimal? MaxWithdraw { get; set; }
        /// <summary>
        /// Needs a tag
        /// </summary>
        [JsonProperty("needTag")]
        public bool? NeedTag { get; set; }
        /// <summary>
        /// Maximum withdraw precision
        /// </summary>
        [JsonProperty("withdrawPrecision")]
        public decimal? WithdrawPrecision { get; set; }

    }
}
