using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Withdrawal info
    /// </summary>
    public record GeminiWithdrawal
    {
        /// <summary>
        /// The id of the withdrawal
        /// </summary>
        public string Id { get; set; } = string.Empty;
        [JsonProperty("withdrawalId")]
        private string WithdrawalId { set => Id = value; }

        /// <summary>
        /// The address the withdrawal is to
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// The memo for the withdrawal
        /// </summary>
        public string Memo { get; set; } = string.Empty;
        /// <summary>
        /// The remark for the withdrawal
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// The asset of the withdrawal
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// The quantity of the withdrawal
        /// </summary>
        [JsonProperty("amount")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// The fee of the withdrawal
        /// </summary>
        public decimal Fee { get; set; }
        /// <summary>
        /// The wallet transaction id
        /// </summary>
        [JsonProperty("walletTxId")]
        public string WalletTransactionId { get; set; } = string.Empty;
        /// <summary>
        /// Whether it is an internal withdrawal
        /// </summary>
        public bool IsInner { get; set; }
        /// <summary>
        /// Status of the converter
        /// </summary>
        [JsonConverter(typeof(WithdrawalStatusConverter))]
        public WithdrawalStatus Status { get; set; }
        /// <summary>
        /// The time the withdrawal was created
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("createdAt")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// The time the withdrawal was last updated
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("updatedAt")]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Reason
        /// </summary>
        public string? Reason { get; set; }
        /// <summary>
        /// The chain
        /// </summary>
        [JsonProperty("chain")]
        public string? Network { get; set; }
    }
}
