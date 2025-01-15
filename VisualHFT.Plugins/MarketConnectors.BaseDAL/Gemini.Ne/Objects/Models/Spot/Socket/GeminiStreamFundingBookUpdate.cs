using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot.Socket
{
    /// <summary>
    /// Stream funding book update
    /// </summary>
    public record GeminiStreamFundingBookUpdate
    {
        /// <summary>
        /// The asset
        /// </summary>
        [JsonProperty("currency")]
        public string Asset { get; set; } = string.Empty;

        /// <summary>
        /// Sequence number
        /// </summary>
        public long Sequence { get; set; }

        /// <summary>
        /// The daily interest rate
        /// </summary>
        [JsonProperty("dailyIntRate")]
        public decimal DailyInterestRate { get; set; }

        /// <summary>
        /// The anual interest rate
        /// </summary>
        [JsonProperty("annualIntRate")]
        public decimal AnnualInterestRate { get; set; }

        /// <summary>
        /// Term (days)
        /// </summary>
        public int Term { get; set; }
        /// <summary>
        /// Current total size
        /// </summary>
        public decimal Size { get; set; }

        /// <summary>
        /// Lend or borrow
        /// </summary>
        public string Side { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp of the data
        /// </summary>
        [JsonProperty("ts"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }
}
