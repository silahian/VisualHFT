using CryptoExchange.Net.Attributes;
using Newtonsoft.Json;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Transfer account type
    /// </summary>
    [JsonConverter(typeof(EnumConverter))]
    public enum TransferAccountType
    {
        /// <summary>
        /// Main account
        /// </summary>
        [Map("MAIN")]
        Main,
        /// <summary>
        /// Trade
        /// </summary>
        [Map("TRADE")]
        Trade,
        /// <summary>
        /// Contract
        /// </summary>
        [Map("CONTRACT")]
        Contract,
        /// <summary>
        /// Margin
        /// </summary>
        [Map("MARGIN")]
        Margin,
        /// <summary>
        /// Isolated
        /// </summary>
        [Map("ISOLATED")]
        Isolated,
        /// <summary>
        /// HF trade
        /// </summary>
        [Map("TRADE_HF")]
        TradeHf,
        /// <summary>
        /// Margin
        /// </summary>
        [Map("MARGIN_V2")]
        MarginV2,
        /// <summary>
        /// Isolated
        /// </summary>
        [Map("ISOLATED_V2")]
        IsolatedV2
    }
}
