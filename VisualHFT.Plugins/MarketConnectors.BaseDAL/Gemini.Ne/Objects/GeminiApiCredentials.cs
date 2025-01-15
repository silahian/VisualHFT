using System;
using System.IO;
using CryptoExchange.Net.Authentication;
using System.Security;
using CryptoExchange.Net;
using CryptoExchange.Net.Converters.MessageParsing;

namespace Gemini.Net.Objects
{
    /// <summary>
    /// Credentials for the Gemini API
    /// </summary>
    public class GeminiApiCredentials : ApiCredentials
    {
        /// <summary>
        /// The pass phrase
        /// </summary>
        public string PassPhrase { get; }

        /// <summary>
        /// Creates new api credentials. Keep this information safe.
        /// </summary>
        /// <param name="apiKey">The API key</param>
        /// <param name="apiSecret">The API secret</param>
        /// <param name="apiPassPhrase">The API passPhrase</param>
        public GeminiApiCredentials(string apiKey, string apiSecret, string apiPassPhrase): base(apiKey, apiSecret)
        {
            PassPhrase = apiPassPhrase;
        }

        /// <summary>
        /// Create Api credentials providing a stream containing json data. The json data should include three values: apiKey, apiSecret and apiPassPhrase
        /// </summary>
        /// <param name="inputStream">The stream containing the json data</param>
        /// <param name="identifierKey">A key to identify the credentials for the API. For example, when set to `binanceKey` the json data should contain a value for the property `binanceKey`. Defaults to 'apiKey'.</param>
        /// <param name="identifierSecret">A key to identify the credentials for the API. For example, when set to `binanceSecret` the json data should contain a value for the property `binanceSecret`. Defaults to 'apiSecret'.</param>
        /// <param name="identifierPassPhrase">A key to identify the credentials for the API. For example, when set to `kucoinPass` the json data should contain a value for the property `kucoinPass`. Defaults to 'apiPassPhrase'.</param>
        public GeminiApiCredentials(Stream inputStream, string? identifierKey = null, string? identifierSecret = null, string? identifierPassPhrase = null) : base(inputStream, identifierKey, identifierSecret)
        {
            var accessor = new JsonNetStreamMessageAccessor();
            if (!accessor.Read(inputStream, false).Result)
                throw new ArgumentException("Input stream not valid json data");

            var pass = accessor.GetValue<string>(MessagePath.Get().Property(identifierPassPhrase ?? "apiPassPhrase")) ?? throw new ArgumentException("apiKey or apiSecret value not found in Json credential file");
            PassPhrase = pass;
        }

        /// <inheritdoc />
        public override ApiCredentials Copy()
        {
            return new GeminiApiCredentials(Key, Secret, PassPhrase);
        }
    }
}
