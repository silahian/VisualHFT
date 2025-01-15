using System;
using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot.Socket
{
    /// <summary>
    /// Stream snapshot wrapper
    /// </summary>
    public record GeminiStreamSnapshotWrapper
    {
        /// <summary>
        /// The sequence number of the update
        /// </summary>
        public long Sequence { get; set; }

        /// <summary>
        /// The data
        /// </summary>
        public GeminiStreamSnapshot Data { get; set; } = default!;
    }

    /// <summary>
    /// Stream snapshot
    /// </summary>
    public record GeminiStreamSnapshot
    {
        /// <summary>
        /// Whether the symbol is trading
        /// </summary>
        public bool Trading { get; set; }
        /// <summary>
        /// The symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The current best bid
        /// </summary>
        [JsonProperty("buy")]
        public decimal? BestBidPrice { get; set; }
        /// <summary>
        /// The current best ask
        /// </summary>
        [JsonProperty("sell")]
        public decimal? BestAskPrice { get; set; }

        /// <summary>
        /// The current best ask quantity
        /// </summary>
        [JsonProperty("askSize")]
        public decimal? BestAskQuantity { get; set; }
        /// <summary>
        /// The current best bid quantity
        /// </summary>
        [JsonProperty("bidSize")]
        public decimal? BestBidQuantity { get; set; }
        /// <summary>
        /// Unknown
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// The value of the volume
        /// </summary>
        [JsonProperty("volValue")]
        public decimal VolumeValue { get; set; }
        /// <summary>
        /// The volume
        /// </summary>
        [JsonProperty("vol")]
        public decimal Volume { get; set; }
        /// <summary>
        /// The base asset
        /// </summary>
        [JsonProperty("baseCurrency")]
        public string BaseAsset { get; set; } = string.Empty;
        /// <summary>
        /// The market name
        /// </summary>
        public string Market { get; set; } = string.Empty;
        /// <summary>
        /// The quote asset
        /// </summary>
        [JsonProperty("quoteCurrency")]
        public string QuoteAsset { get; set; } = string.Empty;
        /// <summary>
        /// The symbol code
        /// </summary>
        public string SymbolCode { get; set; } = string.Empty;
        /// <summary>
        /// The timestamp of the data
        /// </summary>
        [JsonProperty("datetime"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The highest price
        /// </summary>
        [JsonProperty("high")]
        public decimal? HighPrice { get; set; }
        /// <summary>
        /// The lowest price
        /// </summary>
        [JsonProperty("low")]
        public decimal? LowPrice { get; set; }
        /// <summary>
        /// The close price
        /// </summary>
        [JsonProperty("close")]
        public decimal? ClosePrice { get; set; }
        /// <summary>
        /// The open price
        /// </summary>
        [JsonProperty("open")]
        public decimal? OpenPrice { get; set; }
        /// <summary>
        /// The last price
        /// </summary>
        [JsonProperty("lastTradedPrice")]
        public decimal? LastPrice { get; set; }
        /// <summary>
        /// The change price
        /// </summary>
        public decimal? ChangePrice { get; set; }
        /// <summary>
        /// The change percentage
        /// </summary>
        [JsonProperty("changeRate")]
        public decimal? ChangePercentage { get; set; }
        /// <summary>
        /// Average price
        /// </summary>
        public decimal? AveragePrice { get; set; }
        /// <summary>
        /// Unknown
        /// </summary>
        public int Board { get; set; }
        /// <summary>
        /// Unknown
        /// </summary>
        public int Mark { get; set; }
        /// <summary>
        /// Maker coefficent
        /// </summary>
        public decimal? MakerCoefficient { get; set; }
        /// <summary>
        /// Taker coefficent
        /// </summary>
        public decimal? TakerCoefficient { get; set; }
        /// <summary>
        /// Maker fee rate
        /// </summary>
        public decimal? MakerFeeRate { get; set; }
        /// <summary>
        /// Taker fee rate
        /// </summary>
        public decimal? TakerFeeRate { get; set; }
        /// <summary>
        /// Margin trade
        /// </summary>
        public bool? MarginTrade { get; set; }
        /// <summary>
        /// Markets
        /// </summary>
        public IEnumerable<string> Markets { get; set; } = new string[0];
        /// <summary>
        /// Change info last hour
        /// </summary>
        [JsonProperty("marketChange1h")]
        public GeminiMarketChange MarketChange1h { get; set; } = null!;
        /// <summary>
        /// Change info last 4 hours
        /// </summary>
        [JsonProperty("marketChange4h")]
        public GeminiMarketChange MarketChange4h { get; set; } = null!;
        /// <summary>
        /// Change info last 24 hours
        /// </summary>
        [JsonProperty("marketChange24h")]
        public GeminiMarketChange MarketChange24h { get; set; } = null!;
    }

    /// <summary>
    /// Change info
    /// </summary>
    public record GeminiMarketChange
    {
        /// <summary>
        /// Change price
        /// </summary>
        public decimal ChangePrice { get; set; }
        /// <summary>
        /// Change percentage
        /// </summary>
        public decimal ChangeRate { get; set; }
        /// <summary>
        /// High
        /// </summary>
        public decimal High { get; set; }
        /// <summary>
        /// Low
        /// </summary>
        public decimal Low { get; set; }
        /// <summary>
        /// Open
        /// </summary>
        public decimal Open { get; set; }
        /// <summary>
        /// Volume
        /// </summary>
        [JsonProperty("vol")]
        public decimal Volume { get; set; }
        /// <summary>
        /// Volume value
        /// </summary>
        [JsonProperty("volValue")]
        public decimal VolumeValue { get; set; }
    }
}
