using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Historical withdrawal info
    /// </summary>
    public record GeminiHistoricalWithdrawal
    {
        /// <summary>
        /// The asset of the withdrawal
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// The address the withdrawal was to
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// The status of the withdrawal
        /// </summary>
        [JsonConverter(typeof(WithdrawalStatusConverter))]
        public WithdrawalStatus Status { get; set; }
        /// <summary>
        /// The wallet transaction id
        /// </summary>
        [JsonProperty("walletTxId")]
        public string WalletTransactionId { get; set; } = string.Empty;
        /// <summary>
        /// The time the withdrawal was created
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("createAt")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// Whether it was an internal withdrawal
        /// </summary>
        public bool IsInner { get; set; }
        /// <summary>
        /// The quantity of the withdrawal
        /// </summary>
        [JsonProperty("amount")]
        public decimal Quantity { get; set; }
    }
}
