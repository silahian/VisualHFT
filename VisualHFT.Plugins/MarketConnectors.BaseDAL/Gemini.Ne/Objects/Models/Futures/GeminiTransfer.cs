using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Transfer info
    /// </summary>
    public record GeminiTransfer
    {
        /// <summary>
        /// Apply id
        /// </summary>
        public string ApplyId { get; set; } = string.Empty;
        /// <summary>
        /// Asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Status of the transfer
        /// </summary>
        [JsonConverter(typeof(DepositStatusConverter))]
        public DepositStatus Status { get; set; }
        /// <summary>
        /// Quantity of the transfer
        /// </summary>
        [JsonProperty("amount")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Reason if failed
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        /// <summary>
        /// Offset
        /// </summary>
        public long Offset { get; set; }
        /// <summary>
        /// Creation time
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("createdAt")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// User remark
        /// </summary>
        [JsonProperty("remark")]
        public string? Remark { get; set; }
        /// <summary>
        /// Receive account tx remark
        /// </summary>
        [JsonProperty("recRemark")]
        public string? ReceiveRemark { get; set; }
        /// <summary>
        /// Receive system
        /// </summary>
        [JsonProperty("recSystem")]
        public string? ReceiveSystem { get; set; }
    }
}
