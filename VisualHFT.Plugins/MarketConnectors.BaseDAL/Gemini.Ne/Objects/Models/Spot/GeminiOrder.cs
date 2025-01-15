using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// Order info
    /// </summary>
    public record GeminiOrder: GeminiOrderBase
    {        
        /// <summary>
        /// The operation type
        /// </summary>
        [JsonProperty("opType"), JsonConverter(typeof(OperationTypeConverter))]
        public OrderOperationType? OperationType { get; set; }        
        /// <summary>
        /// The funds of the order
        /// </summary>
        [JsonProperty("funds")]
        public decimal? QuoteQuantity { get; set; }
        /// <summary>
        /// The funds of the deal
        /// </summary>
        [JsonProperty("dealFunds")]
        public decimal? QuoteQuantityFilled { get; set; }
        /// <summary>
        /// The quantity of the deal
        /// </summary>
        [JsonProperty("dealSize")]
        public decimal QuantityFilled { get; set; }
        /// <summary>
        /// The fee of the order
        /// </summary>
        public decimal Fee { get; set; }
        /// <summary>
        /// The asset of the fee
        /// </summary>
        [JsonProperty("feeCurrency")]
        public string FeeAsset { get; set; } = string.Empty;
        /// <summary>
        /// The stop condition
        /// </summary>
        public StopCondition? Stop { get; set; }
        /// <summary>
        /// Time after which the order is canceled
        /// </summary>
        public int? CancelAfter { get; set; }
        /// <summary>
        /// The source of the order
        /// </summary>
        public string Channel { get; set; } = string.Empty;
        /// <summary>
        /// Tags for the order
        /// </summary>
        public string Tags { get; set; } = string.Empty;        
        /// <summary>
        /// Trade type
        /// </summary>
        [JsonConverter(typeof(TradeTypeConverter))]
        public TradeType TradeType { get; set; }
    }

    /// <summary>
    /// Stop order info
    /// </summary>
    public record GeminiStopOrder: GeminiOrder
    {
        /// <summary>
        /// User id
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        /// <summary>
        /// Status
        /// </summary>
        [JsonConverter(typeof(StopOrderStatusConverter))]
        public StopOrderStatus Status { get; set; }
        /// <summary>
        /// Time after which the order is canceled
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// Domain id
        /// </summary>
        public string DomainId { get; set; } = string.Empty;
        /// <summary>
        /// Trade source
        /// </summary>
        public string TradeSource { get; set; } = string.Empty;
        /// <summary>
        /// Taker fee rate
        /// </summary>
        public decimal TakerFeeRate { get; set; }
        /// <summary>
        /// Taker fee rate
        /// </summary>
        public decimal MakerFeeRate { get; set; }
        /// <summary>
        /// Time stop order was triggered
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? StopTriggerTime { get; set; }
    }
}
