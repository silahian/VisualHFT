using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Options
{
    /// <summary>
    /// Gemini Rest client options
    /// </summary>
    public class GeminiRestOptions : RestExchangeOptions<GeminiEnvironment, GeminiApiCredentials>
    {
        /// <summary>
        /// Default options for new clients
        /// </summary>
        public static GeminiRestOptions Default { get; set; } = new GeminiRestOptions()
        {
            Environment = GeminiEnvironment.Live
        };

        /// <summary>
        /// Spot API options
        /// </summary>
        public GeminiRestApiOptions SpotOptions { get; private set; } = new GeminiRestApiOptions();

        /// <summary>
        /// Futures API options
        /// </summary>
        public GeminiRestApiOptions FuturesOptions { get; private set; } = new GeminiRestApiOptions();

        internal GeminiRestOptions Copy()
        {
            var options = Copy<GeminiRestOptions>();
            options.SpotOptions = SpotOptions.Copy();
            options.FuturesOptions = FuturesOptions.Copy();
            return options;
        }
    }
}
