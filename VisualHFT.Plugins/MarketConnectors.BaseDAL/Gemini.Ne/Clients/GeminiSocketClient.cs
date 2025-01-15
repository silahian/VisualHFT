using Gemini.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using Gemini.Net.Interfaces.Clients;
using Gemini.Net.Interfaces.Clients.SpotApi;
using Gemini.Net.Interfaces.Clients.FuturesApi;
using Gemini.Net.Clients.SpotApi;
using Gemini.Net.Clients.FuturesApi;
using Gemini.Net.Objects.Options;
using Microsoft.Extensions.DependencyInjection;
using CryptoExchange.Net.Clients;

namespace Gemini.Net.Clients
{
    /// <inheritdoc cref="IGeminiSocketClient" />
    public class GeminiSocketClient : BaseSocketClient, IGeminiSocketClient
    {
        #region Api clients

        /// <inheritdoc />
        public IGeminiSocketClientSpotApi SpotApi { get; }
        /// <inheritdoc />
        public IGeminiSocketClientFuturesApi FuturesApi { get; }

        #endregion
        /// <summary>
        /// Create a new instance of the OKXSocketClient
        /// </summary>
        /// <param name="loggerFactory">The logger</param>
        public GeminiSocketClient(ILoggerFactory? loggerFactory = null) : this((x) => { }, loggerFactory)
        {
        }

        /// <summary>
        /// Create a new instance of GeminiSocketClient
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public GeminiSocketClient(Action<GeminiSocketOptions> optionsDelegate) : this(optionsDelegate, null)
        {
        }

        /// <summary>
        /// Create a new instance of GeminiSocketClient
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        /// <param name="loggerFactory">The logger factory</param>
        [ActivatorUtilitiesConstructor]
        public GeminiSocketClient(Action<GeminiSocketOptions>? optionsDelegate, ILoggerFactory? loggerFactory = null) : base(loggerFactory, "Gemini")
        {
            var options = GeminiSocketOptions.Default.Copy();
            if (optionsDelegate != null)
                optionsDelegate(options);
            Initialize(options);

            SpotApi = AddApiClient(new GeminiSocketClientSpotApi(_logger, this, options));
            FuturesApi = AddApiClient(new GeminiSocketClientFuturesApi(_logger, this, options));
        }

        /// <summary>
        /// Set the default options to be used when creating new clients
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public static void SetDefaultOptions(Action<GeminiSocketOptions> optionsDelegate)
        {
            var options = GeminiSocketOptions.Default.Copy();
            optionsDelegate(options);
            GeminiSocketOptions.Default = options;
        }

        /// <summary>
        /// Set the API credentials to use in this client
        /// </summary>
        /// <param name="credentials">Credentials to use</param>
        public void SetApiCredentials(GeminiApiCredentials credentials)
        {
            SpotApi.SetApiCredentials(credentials);
            FuturesApi.SetApiCredentials(credentials);
        }
    }
}
