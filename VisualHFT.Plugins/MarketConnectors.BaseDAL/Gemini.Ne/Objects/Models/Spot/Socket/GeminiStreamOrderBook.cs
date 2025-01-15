using System;
using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Interfaces;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot.Socket
{
    /// <summary>
    /// Order book info
    /// </summary>
    public record GeminiStreamOrderBook
    {
        /// <summary>
        /// The sequence id of the first event this order book update covers
        /// </summary>
        public long SequenceStart { get; set; }
        /// <summary>
        /// The sequence id of the last event this order book update covers
        /// </summary>
        public long SequenceEnd { get; set; }

        /// <summary>
        /// Data timestamp
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonProperty("time")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The symbol of the order book
        /// </summary>
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// The changes
        /// </summary>
        public GeminiStreamOrderBookChanged Changes { get; set; } = default!;
    }

    /// <summary>
    /// Order book changes
    /// </summary>
    public record GeminiStreamOrderBookChanged
    {
        /// <summary>
        /// The changes in bids
        /// </summary>
        public IEnumerable<GeminiStreamOrderBookEntry> Bids { get; set; } = Array.Empty<GeminiStreamOrderBookEntry>();
        /// <summary>
        /// The changes in asks
        /// </summary>
        public IEnumerable<GeminiStreamOrderBookEntry> Asks { get; set; } = Array.Empty<GeminiStreamOrderBookEntry>();
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime? Timestamp { get; set; }

        [JsonProperty("ts"), JsonConverter(typeof(DateTimeConverter))]
        internal DateTime? Ts { set => Timestamp = value; }
    }

    /// <summary>
    /// Order book entry
    /// </summary>
    [JsonConverter(typeof(ArrayConverter))]
    public record GeminiStreamOrderBookEntry: ISymbolOrderSequencedBookEntry
    {
        /// <summary>
        /// The price of the change
        /// </summary>
        [ArrayProperty(0)]
        public decimal Price { get; set; }
        /// <summary>
        /// The quantity of the change
        /// </summary>
        [ArrayProperty(1)]
        public decimal Quantity { get; set; }
        /// <summary>
        /// The sequence of the change
        /// </summary>
        [ArrayProperty(2)]
        public long Sequence { get; set; }
    }
}
