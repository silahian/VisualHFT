using Gemini.Net.Clients;
using Gemini.Net.Interfaces.Clients;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Extensions for the ICryptoRestClient and ICryptoSocketClient interfaces
    /// </summary>
    public static class CryptoClientExtensions
    {
        /// <summary>
        /// Get the Gemini REST Api client
        /// </summary>
        /// <param name="baseClient"></param>
        /// <returns></returns>
        public static IGeminiRestClient Gemini(this ICryptoRestClient baseClient) => baseClient.TryGet<IGeminiRestClient>(() => new GeminiRestClient());

        /// <summary>
        /// Get the Gemini Websocket Api client
        /// </summary>
        /// <param name="baseClient"></param>
        /// <returns></returns>
        public static IGeminiSocketClient Gemini(this ICryptoSocketClient baseClient) => baseClient.TryGet<IGeminiSocketClient>(() => new GeminiSocketClient());
    }
}
