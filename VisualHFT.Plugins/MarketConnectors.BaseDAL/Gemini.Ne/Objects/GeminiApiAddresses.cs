namespace Gemini.Net.Objects
{
    /// <summary>
    /// Gemini API addresses
    /// </summary>
    public class GeminiApiAddresses
    {
        /// <summary>
        /// The address used by the GeminiClient for the SPOT API
        /// </summary>
        public string SpotAddress { get; set; } = string.Empty;

        /// <summary>
        /// The address used by the GeminiClient for the futures API
        /// </summary>
        public string FuturesAddress { get; set; } = string.Empty;

        /// <summary>
        /// The default addresses to connect to the gemini.com API
        /// </summary>
        public static GeminiApiAddresses Default = new GeminiApiAddresses
        {
            SpotAddress = "https://api.gemini.com/",
            FuturesAddress = "https://api-futures.gemini.com/",
        };

        /// <summary>
        /// The addresses to connect to the gemini.com sandbox API
        /// </summary>
        public static GeminiApiAddresses TestNet = new GeminiApiAddresses
        {
            SpotAddress = "https://openapi-sandbox.gemini.com/",
            FuturesAddress = "https://api-sandbox-futures.gemini.com/",
        };
    }
}
