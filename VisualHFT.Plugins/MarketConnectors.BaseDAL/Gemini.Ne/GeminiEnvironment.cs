using CryptoExchange.Net.Objects;
using Gemini.Net.Objects;

namespace Gemini.Net
{
    /// <summary>
    /// Gemini environments
    /// </summary>
    public class GeminiEnvironment: TradeEnvironment
    {
        /// <summary>
        /// Spot API address
        /// </summary>
        public string SpotAddress { get; }

        /// <summary>
        /// Futures API address
        /// </summary>
        public string FuturesAddress { get; }

        internal GeminiEnvironment(string name, string spotAddress, string futuresAddress) : 
            base(name)
        {
            SpotAddress = spotAddress;
            FuturesAddress = futuresAddress;
        }

        /// <summary>
        /// Live environment
        /// </summary>
        public static GeminiEnvironment Live { get; } = new GeminiEnvironment(TradeEnvironmentNames.Live, GeminiApiAddresses.Default.SpotAddress, GeminiApiAddresses.Default.FuturesAddress);

        /// <summary>
        /// Testnet/sandbox environment
        /// </summary>
        public static GeminiEnvironment Testnet { get; } = new GeminiEnvironment(TradeEnvironmentNames.Testnet, GeminiApiAddresses.TestNet.SpotAddress, GeminiApiAddresses.TestNet.FuturesAddress);

        /// <summary>
        /// Create a custom environment
        /// </summary>
        /// <param name="name"></param>
        /// <param name="spotAddress"></param>
        /// <param name="futuresAddress"></param>
        /// <returns></returns>
        public static GeminiEnvironment CreateCustom(string name, string spotAddress, string futuresAddress)
            => new GeminiEnvironment(name, spotAddress, futuresAddress);
    }
}
