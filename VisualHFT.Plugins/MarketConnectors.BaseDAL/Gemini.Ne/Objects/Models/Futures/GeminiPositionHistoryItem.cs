using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Position history entry
    /// </summary>
    public record GeminiPositionHistoryItem
    {
        /// <summary>
        /// Close id
        /// </summary>
        [JsonProperty("closeId")]
        public string CloseId { get; set; } = string.Empty;
        /// <summary>
        /// Position id
        /// </summary>
        [JsonProperty("positionId")]
        public string PositionId { get; set; } = string.Empty;
        /// <summary>
        /// Uid
        /// </summary>
        [JsonProperty("uid")]
        public long Uid { get; set; }
        /// <summary>
        /// User id
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; } = string.Empty;
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Settlement asset
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleAsset { get; set; } = string.Empty;
        /// <summary>
        /// Leverage
        /// </summary>
        [JsonProperty("leverage")]
        public decimal Leverage { get; set; }
        /// <summary>
        /// Side
        /// </summary>
        [JsonProperty("side"), JsonConverter(typeof(EnumConverter))]
        public OrderSide? Side { get; set; }
        /// <summary>
        /// Close quantity
        /// </summary>
        [JsonProperty("closeSize")]
        public decimal? CloseQuantity { get; set; }
        /// <summary>
        /// Profit and loss
        /// </summary>
        [JsonProperty("pnl")]
        public decimal? ProfitAndLoss { get; set; }
        /// <summary>
        /// Realised gross cost
        /// </summary>
        [JsonProperty("realisedGrossCost")]
        public decimal? RealisedGrossCost { get; set; }
        /// <summary>
        /// Withdraw profit and loss
        /// </summary>
        [JsonProperty("withdrawPnl")]
        public decimal? WithdrawPnl { get; set; }
        /// <summary>
        /// Trading fee
        /// </summary>
        [JsonProperty("tradeFee")]
        public decimal? TradeFee { get; set; }
        /// <summary>
        /// Funding fee
        /// </summary>
        [JsonProperty("fundingFee")]
        public decimal? FundingFee { get; set; }
        /// <summary>
        /// Open time
        /// </summary>
        [JsonProperty("openTime"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime OpenTime { get; set; }
        /// <summary>
        /// Close time
        /// </summary>
        [JsonProperty("closeTime"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime? CloseTime { get; set; }
        /// <summary>
        /// Open price
        /// </summary>
        [JsonProperty("openPrice")]
        public decimal? OpenPrice { get; set; }
        /// <summary>
        /// Close price
        /// </summary>
        [JsonProperty("closePrice")]
        public decimal? ClosePrice { get; set; }
        /// <summary>
        /// Closing type
        /// </summary>
        [JsonProperty("type")]
        public string CloseType { get; set; } = string.Empty;
    }
}
