using CryptoExchange.Net.Objects.Options;

namespace Gemini.Net.Objects.Options
{
    /// <summary>
    /// Gemini socket client options
    /// </summary>
    public class GeminiSocketOptions : SocketExchangeOptions<GeminiEnvironment, GeminiApiCredentials>
    {
        /// <summary>
        /// Default options for new clients
        /// </summary>
        public static GeminiSocketOptions Default { get; set; } = new GeminiSocketOptions()
        {
            Environment = GeminiEnvironment.Live,
            SocketSubscriptionsCombineTarget = 10
        };

        /// <summary>
        /// Spot API options
        /// </summary>
        public SocketApiOptions<GeminiApiCredentials> SpotOptions { get; private set; } = new SocketApiOptions<GeminiApiCredentials>()
        {
            MaxSocketConnections = 50
        };

        /// <summary>
        /// Futures API options
        /// </summary>
        public SocketApiOptions<GeminiApiCredentials> FuturesOptions { get; private set; } = new SocketApiOptions<GeminiApiCredentials>()
        {
            MaxSocketConnections = 50
        };

        internal GeminiSocketOptions Copy()
        {
            var options = Copy<GeminiSocketOptions>();
            options.SpotOptions = SpotOptions.Copy<SocketApiOptions<GeminiApiCredentials>>();
            options.FuturesOptions = FuturesOptions.Copy<SocketApiOptions<GeminiApiCredentials>>();
            return options;
        }
    }
}
