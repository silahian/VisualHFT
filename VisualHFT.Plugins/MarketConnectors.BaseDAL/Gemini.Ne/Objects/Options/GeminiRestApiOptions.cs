using CryptoExchange.Net.Objects.Options;

namespace Gemini.Net.Objects.Options
{
    /// <inheritdoc />
    public class GeminiRestApiOptions : RestApiOptions<GeminiApiCredentials>
    {
        /// <summary>
        /// The broker reference name to use
        /// </summary>
        public string? BrokerName { get; set; }

        /// <summary>
        /// The private key of the broker
        /// </summary>
        public string? BrokerKey { get; set; }

        internal GeminiRestApiOptions Copy()
        {
            var result = base.Copy<GeminiRestApiOptions>();
            result.BrokerKey = BrokerKey;
            result.BrokerName = BrokerName;
            return result;
        }
    }
}
