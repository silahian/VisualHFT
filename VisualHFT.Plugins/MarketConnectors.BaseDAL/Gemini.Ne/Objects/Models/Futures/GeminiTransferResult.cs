using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Transfer result
    /// </summary>
    public record GeminiTransferResult
    {
        /// <summary>
        /// Request id
        /// </summary>
        [JsonProperty("applyId")]
        public string ApplyId { get; set; } = string.Empty;
        /// <summary>
        /// Business number
        /// </summary>
        [JsonProperty("bizNo")]
        public string BusinessNumber { get; set; } = string.Empty;
        /// <summary>
        /// Pay account type
        /// </summary>
        [JsonProperty("payAccountType")]
        public string PayAccountType { get; set; } = string.Empty;
        /// <summary>
        /// Pay tag
        /// </summary>
        [JsonProperty("payTag")]
        public string PayTag { get; set; } = string.Empty;
        /// <summary>
        /// Remark
        /// </summary>
        [JsonProperty("remark")]
        public string? Remark { get; set; }
        /// <summary>
        /// Remark
        /// </summary>
        [JsonProperty("recAccountType"), JsonConverter(typeof(EnumConverter))]
        public AccountType? ReceiveAccountType { get; set; }
        /// <summary>
        /// Receive tag
        /// </summary>
        [JsonProperty("recTag")]
        public string ReceiveTag { get; set; } = string.Empty;
        /// <summary>
        /// Receive remark
        /// </summary>
        [JsonProperty("recRemark")]
        public string ReceiveRemark { get; set; } = string.Empty;
        /// <summary>
        /// Receive system
        /// </summary>
        [JsonProperty("recSystem")]
        public string ReceiveSystem { get; set; } = string.Empty;
        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
        /// <summary>
        /// Asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonProperty("amount")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Fee
        /// </summary>
        [JsonProperty("fee")]
        public decimal Fee { get; set; }
        /// <summary>
        /// Serial number
        /// </summary>
        [JsonProperty("sn")]
        public long? SerialNumber { get; set; }
        /// <summary>
        /// Reason
        /// </summary>
        [JsonProperty("reason")]
        public string? Reason { get; set; }
        /// <summary>
        /// Create time
        /// </summary>
        [JsonProperty("createdAt"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// Updated time
        /// </summary>
        [JsonProperty("updatedAt"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime? UpdateTime { get; set; }
    }
}
