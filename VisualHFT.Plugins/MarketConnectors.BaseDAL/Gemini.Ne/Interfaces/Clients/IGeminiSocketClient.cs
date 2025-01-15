using CryptoExchange.Net.Interfaces;
using Gemini.Net.Interfaces.Clients.FuturesApi;
using Gemini.Net.Interfaces.Clients.SpotApi;
using Gemini.Net.Objects;

namespace Gemini.Net.Interfaces.Clients
{
    /// <summary>
    /// Client for accessing the Gemini websocket API. 
    /// </summary>
    public interface IGeminiSocketClient : ISocketClient
    {
        /// <summary>
        /// Spot socket api
        /// </summary>
        IGeminiSocketClientSpotApi  SpotApi { get; }
        /// <summary>
        /// Futures socket api
        /// </summary>
        IGeminiSocketClientFuturesApi  FuturesApi { get; }

        /// <summary>
        /// Set the API credentials for this client. All Api clients in this client will use the new credentials, regardless of earlier set options.
        /// </summary>
        /// <param name="credentials">The credentials to set</param>
        void SetApiCredentials(GeminiApiCredentials credentials);
    }
}
