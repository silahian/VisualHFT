using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Account activity info
    /// </summary>
    public record GeminiAccountActivity
    {
        /// <summary>
        /// Creation timestamp
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("createdAt")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// The quantity of the activity
        /// </summary>
        [JsonProperty("amount")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// The remaining balance after the activity
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// The fee of the activity
        /// </summary>
        public decimal Fee { get; set; }
        /// <summary>
        /// The type of activity
        /// </summary>
        [JsonConverter(typeof(BizTypeConverter))]
        public BizType BizType { get; set; } = default!;
        /// <summary>
        /// The type of activity
        /// </summary>
        [JsonConverter(typeof(AccountTypeConverter))]
        public AccountType AccountType { get; set; } = default!;
        /// <summary>
        /// The unique key for this activity 
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Additional info for this activity
        /// </summary>
        [JsonConverter(typeof(AccountActivityContextConverter))]
        public GeminiAccountActivityContext Context { get; set; } = default!;
        /// <summary>
        /// The asset of the activity
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// The direction of the activity
        /// </summary>
        [JsonConverter(typeof(AccountDirectionConverter))]
        public AccountDirection Direction { get; set; }
    }

    /// <summary>
    /// Account activity details
    /// </summary>
    public record GeminiAccountActivityContext
    {
        /// <summary>
        /// The id for the order
        /// </summary>
        public string OrderId { get; set; } = string.Empty;
        /// <summary>
        /// The id for the trade (for trades)
        /// </summary>
        public string TradeId { get; set; } = string.Empty;
        /// <summary>
        /// The symbol of the order (for trades)
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The transaction id (for withdrawal/deposit)
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;
        /// <summary>
        /// The txId (for orders)
        /// </summary>
        public string TxId { get; set; } = string.Empty;
        /// <summary>
        /// The Description (for pool-x staking rewards)
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
