using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Deposit info
    /// </summary>
    public record GeminiDeposit
    {
        /// <summary>
        /// The deposit address
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// A memo for this deposit
        /// </summary>
        public string Memo { get; set; } = string.Empty;
        /// <summary>
        /// A remark for this deposit
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// The chain
        /// </summary>
        [JsonProperty("chain")]
        public string? Network { get; set; }
        /// <summary>
        /// The quantity of the deposit
        /// </summary>
        [JsonProperty("amount")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// The fee of the deposit
        /// </summary>
        public decimal Fee { get; set; }
        /// <summary>
        /// The asset of the deposit
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Whether it is an internal deposit
        /// </summary>
        public bool IsInner { get; set; }
        /// <summary>
        /// The wallet transaction id
        /// </summary>
        [JsonProperty("walletTxId")]
        public string WalletTransactionId { get; set; } = string.Empty;
        /// <summary>
        /// The deposit status
        /// </summary>
        [JsonConverter(typeof(DepositStatusConverter))]
        public DepositStatus Status { get; set; }
        /// <summary>
        /// When the deposit was created
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("createdAt")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// When the deposit was last updated
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("updatedAt")]
        public DateTime UpdateTime { get; set; }
    }
}
