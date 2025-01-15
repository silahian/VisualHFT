using CryptoExchange.Net.Clients;
using Gemini.Net.Clients.FuturesApi;
using Gemini.Net.Clients.SpotApi;
using Gemini.Net.Interfaces.Clients;
using Gemini.Net.Interfaces.Clients.FuturesApi;
using Gemini.Net.Interfaces.Clients.SpotApi;
using Gemini.Net.Objects;
using Gemini.Net.Objects.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace Gemini.Net.Clients
{
    /// <inheritdoc cref="IGeminiRestClient" />
    public class GeminiRestClient : BaseRestClient, IGeminiRestClient
    {
        /// <inheritdoc />
        public IGeminiRestClientSpotApi SpotApi { get; }

        /// <inheritdoc />
        public IGeminiRestClientFuturesApi FuturesApi { get; }

        /// <summary>
        /// Create a new instance of GeminiClient
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public GeminiRestClient(Action<GeminiRestOptions>? optionsDelegate = null) : this(null, null, optionsDelegate)
        {
        }

        /// <summary>
        /// Create a new instance of GeminiClient
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        /// <param name="loggerFactory">The logger factory</param>
        /// <param name="httpClient">Http client for this client</param>
        public GeminiRestClient(HttpClient? httpClient, ILoggerFactory? loggerFactory, Action<GeminiRestOptions>? optionsDelegate = null) : base(loggerFactory, "Gemini")
        {
            var options = GeminiRestOptions.Default.Copy();
            if (optionsDelegate != null)
                optionsDelegate(options);
            Initialize(options);

            SpotApi = AddApiClient(new GeminiRestClientSpotApi(_logger, httpClient, this, options));
            FuturesApi = AddApiClient(new GeminiRestClientFuturesApi(_logger, httpClient, this, options));
        }

        /// <inheritdoc />
        public void SetApiCredentials(GeminiApiCredentials credentials)
        {
            SpotApi.SetApiCredentials(credentials);
            FuturesApi.SetApiCredentials(credentials);
        }

        /// <summary>
        /// Set the default options to be used when creating new clients
        /// </summary>
        /// <param name="optionsFunc">Option configuration delegate</param>
        public static void SetDefaultOptions(Action<GeminiRestOptions> optionsFunc)
        {
            var options = GeminiRestOptions.Default.Copy();
            optionsFunc(options);
            GeminiRestOptions.Default = options;
        }
    }
}
