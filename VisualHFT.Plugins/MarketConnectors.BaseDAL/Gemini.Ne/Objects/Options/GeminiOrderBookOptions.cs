using CryptoExchange.Net.Objects.Options;
using System;

namespace Gemini.Net.Objects.Options
{
    /// <summary>
    /// Options for the Gemini SymbolOrderBook
    /// </summary>
    public class GeminiOrderBookOptions : OrderBookOptions
    {
        /// <summary>
        /// Default options for new order books
        /// </summary>
        public static GeminiOrderBookOptions Default { get; set; } = new GeminiOrderBookOptions();

        /// <summary>
        /// The top amount of results to keep in sync. If for example limit=10 is used, the order book will contain the 10 best bids and 10 best asks. Leaving this null will sync the full order book
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// After how much time we should consider the connection dropped if no data is received for this time after the initial subscriptions
        /// </summary>
        public TimeSpan? InitialDataTimeout { get; set; }

        /// <summary>
        /// API credentials to use. The Gemini order book endpoint requires authentication
        /// </summary>
        public GeminiApiCredentials? ApiCredentials { get; set; }

        internal GeminiOrderBookOptions Copy()
        {
            var result = Copy<GeminiOrderBookOptions>();
            result.Limit = Limit;
            result.InitialDataTimeout = InitialDataTimeout;
            return result;
        }
    }
}
