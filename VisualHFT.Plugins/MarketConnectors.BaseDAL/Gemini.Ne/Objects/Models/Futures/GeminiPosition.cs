using System;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Base record for position info
    /// </summary>
    public record GeminiPositionBase
    {
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Auto deposit margin or not
        /// </summary>
        public bool AutoDeposit { get; set; }
        /// <summary>
        /// Maintenance margin requirement
        /// </summary>
        [JsonProperty("maintMarginReq")]
        public decimal MaintenanceMarginRequirement { get; set; }
        /// <summary>
        /// Risk limit 
        /// </summary>
        public decimal RiskLimit { get; set; }
        /// <summary>
        /// Leverage off the order
        /// </summary>
        public decimal RealLeverage { get; set; }
        /// <summary>
        /// Cross mode or not
        /// </summary>
        public bool CrossMode { get; set; }
        /// <summary>
        /// ADL ranking percentile
        /// </summary>
        [JsonProperty("delevPercentage")]
        public decimal DeleveragePercentage { get; set; }
        /// <summary>
        /// Opening time
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("openingTimestamp")]
        public DateTime? OpenTime { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("currentTimestamp")]
        public DateTime CurrentTime { get; set; }
        /// <summary>
        /// Current position quantity
        /// </summary>
        [JsonProperty("currentQty")]
        public decimal CurrentQuantity { get; set; }
        /// <summary>
        /// Current position value
        /// </summary>
        public decimal CurrentCost { get; set; }
        /// <summary>
        /// Current commission
        /// </summary>
        [JsonProperty("currentComm")]
        public decimal CurrentCommission { get; set; }
        /// <summary>
        /// Unrealized value
        /// </summary>
        [JsonProperty("unrealisedCost")]
        public decimal UnrealizedCost { get; set; }
        /// <summary>
        /// Accumulated realized gross profit value
        /// </summary>
        [JsonProperty("realisedGrossCost")]
        public decimal RealizedGrossCost { get; set; }
        /// <summary>
        /// Current realized position value
        /// </summary>
        [JsonProperty("realisedCost")]
        public decimal RealizedCost { get; set; }
        /// <summary>
        /// Is open
        /// </summary>
        public bool IsOpen { get; set; }
        /// <summary>
        /// Is inverse
        /// </summary>
        public bool IsInverse { get; set; }
        /// <summary>
        /// Mark price
        /// </summary>
        public decimal MarkPrice { get; set; }
        /// <summary>
        /// Mark value
        /// </summary>
        public decimal MarkValue { get; set; }
        /// <summary>
        /// Position value
        /// </summary>
        [JsonProperty("posCost")]
        public decimal PositionValue { get; set; }
        /// <summary>
        /// Added margin
        /// </summary>
        [JsonProperty("posCross")]
        public decimal PositionCross { get; set; }
        /// <summary>
        /// Leverage margin
        /// </summary>
        [JsonProperty("posInit")]
        public decimal PositionInit { get; set; }
        /// <summary>
        /// Bankruptcy cost
        /// </summary>
        [JsonProperty("posComm")]
        public decimal PositionComm { get; set; }
        /// <summary>
        /// Funding fees paid out
        /// </summary>
        [JsonProperty("posLoss")]
        public decimal PositionLoss { get; set; }
        /// <summary>
        /// Position margin
        /// </summary>
        [JsonProperty("posMargin")]
        public decimal PositionMargin { get; set; }
        /// <summary>
        /// Maintenance margin
        /// </summary>
        [JsonProperty("posMaint")]
        public decimal PositionMaintenance { get; set; }
        /// <summary>
        /// Position margin
        /// </summary>
        [JsonProperty("maintMargin")]
        public decimal MaintenanceMargin { get; set; }
        /// <summary>
        /// Accumulated realized gross profit value
        /// </summary>
        [JsonProperty("realisedGrossPnl")]
        public decimal RealizedGrossPnl { get; set; }
        /// <summary>
        /// realized profit and loss
        /// </summary>
        [JsonProperty("realisedPnl")]
        public decimal RealizedPnl { get; set; }
        /// <summary>
        /// Unrealized profit and loss
        /// </summary>
        [JsonProperty("unrealisedPnl")]
        public decimal UnrealizedPnl { get; set; }
        /// <summary>
        /// Profit-loss ratio of the position
        /// </summary>
        [JsonProperty("unrealisedPnlPcnt")]
        public decimal UnrealizedPnlPercentage { get; set; }
        /// <summary>
        /// Rate of return on investment
        /// </summary>
        [JsonProperty("unrealisedRoePcnt")]
        public decimal UnrealizedRoePercentage { get; set; }
        /// <summary>
        /// Average entry price
        /// </summary>
        [JsonProperty("avgEntryPrice")]
        public decimal AverageEntryPrice { get; set; }
        /// <summary>
        /// Liquidation price
        /// </summary>
        public decimal LiquidationPrice { get; set; }
        /// <summary>
        /// Bankruptcy price
        /// </summary>
        public decimal BankruptPrice { get; set; }
        /// <summary>
        /// Asset used to clear and settle the trades
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleAsset { get; set; } = string.Empty;
        /// <summary>
        /// Risk limit level
        /// </summary>
        public int? RiskLimitLevel { get; set; }
        /// <summary>
        /// User id
        /// </summary>
        public long? UserId { get; set; }
        /// <summary>
        /// Maintenance margin rate
        /// </summary>
        [JsonProperty("maintainMargin")]
        public decimal MaintenanceMarginRate { get; set; }
        /// <summary>
        /// Margin mode
        /// </summary>
        [JsonConverter(typeof(EnumConverter))]
        [JsonProperty("marginMode")]
        public FuturesMarginMode? MarginMode { get; set; }
        /// <summary>
        /// Position side
        /// </summary>
        [JsonConverter(typeof(EnumConverter))]
        [JsonProperty("positionSide")]
        public PositionSide? PositionSide { get; set; }
        /// <summary>
        /// Leverage
        /// </summary>
        [JsonProperty("leverage")]
        public decimal? Leverage { get; set; }
        /// <summary>
        /// The current remaining unsettled funding fee for the position Only applicable to Isolated Margin
        /// </summary>
        [JsonProperty("posFunding")]
        public decimal? PositionFunding { get; set; }
    }

    /// <summary>
    /// Position info
    /// </summary>
    public record GeminiPosition: GeminiPositionBase
    {
        /// <summary>
        /// Position id
        /// </summary>
        public string Id { get; set; } = string.Empty;        
    }

    /// <summary>
    /// Position update
    /// </summary>
    public record GeminiPositionUpdate: GeminiPositionBase
    {
        /// <summary>
        /// Change reason
        /// </summary>
        public string ChangeReason { get; set; } = string.Empty;
    }
}
