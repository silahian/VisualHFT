using System;
using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Futures
{
    /// <summary>
    /// Contract info
    /// </summary>
    public record GeminiContract
    {
        /// <summary>
        /// Base asset
        /// </summary>
        [JsonProperty("baseCurrency")]
        public string BaseAsset { get; set; } = string.Empty;
        /// <summary>
        /// Fair method
        /// </summary>
        public string FairMethod { get; set; } = string.Empty;
        /// <summary>
        /// Funding base symbol
        /// </summary>
        public string FundingBaseSymbol { get; set; } = string.Empty;
        /// <summary>
        /// Funding quote symbol
        /// </summary>
        public string FundingQuoteSymbol { get; set; } = string.Empty;
        /// <summary>
        /// Funding rate symbol
        /// </summary>
        public string FundingRateSymbol { get; set; } = string.Empty;
        /// <summary>
        /// Index symbol
        /// </summary>
        public string IndexSymbol { get; set; } = string.Empty;
        /// <summary>
        /// Initial margin
        /// </summary>
        public decimal InitialMargin { get; set; }
        /// <summary>
        /// Enabled ADL or not
        /// </summary>
        public bool IsDeleverage { get; set; }
        /// <summary>
        /// Reverse contract or not
        /// </summary>
        public bool IsInverse { get; set; }
        /// <summary>
        /// Quanto or not
        /// </summary>
        public bool IsQuanto { get; set; }
        /// <summary>
        /// Minimum lot size
        /// </summary>
        public decimal LotSize { get; set; }
        /// <summary>
        /// Maintenance margin requirement
        /// </summary>
        public decimal MaintainMargin { get; set; }
        /// <summary>
        /// Maker fee
        /// </summary>
        public decimal MakerFeeRate { get; set; }
        /// <summary>
        /// Fixed maker fee
        /// </summary>
        public decimal MakerFixFee { get; set; }
        /// <summary>
        /// Marking method
        /// </summary>
        public string MarkMethod { get; set; } = string.Empty;
        /// <summary>
        /// Maximum order quantity
        /// </summary>
        [JsonProperty("maxOrderQty")]
        public decimal MaxOrderQuantity { get; set; }
        /// <summary>
        /// Maximum price
        /// </summary>
        public decimal MaxPrice { get; set; }
        /// <summary>
        /// Maximum risk limit
        /// </summary>
        public decimal MaxRiskLimit { get; set; }
        /// <summary>
        /// Minimum risk limit
        /// </summary>
        public decimal MinRiskLimit { get; set; }
        /// <summary>
        /// Contract multiplier
        /// </summary>
        public decimal Multiplier { get; set; }
        /// <summary>
        /// Quote asset
        /// </summary>
        [JsonProperty("quoteCurrency")]
        public string QuoteAsset { get; set; } = string.Empty;
        /// <summary>
        /// Risk limit increment
        /// </summary>
        public decimal RiskStep { get; set; }
        /// <summary>
        /// Contract group
        /// </summary>
        public string RootSymbol { get; set; } = string.Empty;
        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; } = string.Empty;
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Taker fee
        /// </summary>
        public decimal TakerFeeRate { get; set; }
        /// <summary>
        /// Taker fixed fee
        /// </summary>
        public decimal TakerFixFee { get; set; }
        /// <summary>
        /// Minimum price change
        /// </summary>
        public decimal TickSize { get; set; }
        /// <summary>
        /// Minimum index price change
        /// </summary>
        public decimal IndexPriceTickSize { get; set; }
        /// <summary>
        /// Type of contract
        /// </summary>
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// Maximum leverage
        /// </summary>
        public decimal MaxLeverage { get; set; }
        /// <summary>
        /// 24 hour volume
        /// </summary>
        [JsonProperty("volumeOf24h")]
        public decimal Volume24H { get; set; }
        /// <summary>
        /// 24 hour turnover
        /// </summary>
        [JsonProperty("turnoverOf24h")]
        public decimal Turnover24H { get; set; }
        /// <summary>
        /// Open interest
        /// </summary>
        public decimal? OpenInterest { get; set; }
        /// <summary>
        /// 24 hour low price
        /// </summary>
        public decimal LowPrice { get; set; }
        /// <summary>
        /// 24 hour high price
        /// </summary>
        public decimal HighPrice { get; set; }
        /// <summary>
        /// 24 hour change percentage
        /// </summary>
        [JsonProperty("priceChgPct")]
        public decimal PriceChangePercentage { get; set; }
        /// <summary>
        /// 24 hour change
        /// </summary>
        [JsonProperty("priceChg")]
        public decimal PriceChange { get; set; }
        /// <summary>
        /// First open time
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? FirstOpenDate { get; set; }
        /// <summary>
        /// Expire time
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? ExpireDate { get; set; }
        /// <summary>
        /// Settle time
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? SettleDate { get; set; }
        /// <summary>
        /// Settle asset
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string? SettleAsset { get; set; }
        /// <summary>
        /// Settlement symbol
        /// </summary>
        public string? SettlementSymbol { get; set; }
        /// <summary>
        /// Settle fee
        /// </summary>
        public decimal? SettlementFee { get; set; }
        /// <summary>
        /// Funding fee rate
        /// </summary>
        public decimal? FundingFeeRate { get; set; }
        /// <summary>
        /// Predicted funding fee rate
        /// </summary>
        public decimal? PredictedFundingFeeRate { get; set; }
        /// <summary>
        /// Next funding rate time. This time may not be accurate up to a couple of seconds.
        /// This is due to the fact that the API returns this value as an offset from the current time, 
        /// but we have no way of knowing the exact time the API returned this value.
        /// </summary>
        [JsonConverter(typeof(NextFundingRateTimeConverter))]
        public DateTime? NextFundingRateTime { get; set; }
        /// <summary>
        /// Source exchanges
        /// </summary>
        public IEnumerable<string> SourceExchanges { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Mark price
        /// </summary>
        public decimal? MarkPrice { get; set; }
        /// <summary>
        /// Index price
        /// </summary>
        public decimal? IndexPrice { get; set; }
        /// <summary>
        /// Last traded price
        /// </summary>
        public decimal? LastTradePrice { get; set; }
        /// <summary>
        /// Premiums symbol
        /// </summary>
        public string? PremiumsSymbol1M { get; set; }
        /// <summary>
        /// Premiums symbol
        /// </summary>
        public string? PremiumsSymbol8H { get; set; }
        /// <summary>
        /// Funding base symbol
        /// </summary>
        public string? FundingBaseSymbol1M { get; set; }
        /// <summary>
        /// Funding quote symbol
        /// </summary>
        public string? FundingQuoteSymbol1M { get; set; }
    }
}