using CryptoExchange.Net.Attributes;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Account type
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// Main account
        /// </summary>
        [Map("MAIN")]
        Main,
        /// <summary>
        /// Trade account
        /// </summary>
        [Map("TRADE")]
        Trade,
        /// <summary>
        /// Margin account
        /// </summary>
        [Map("MARGIN")]
        Margin,
        /// <summary>
        /// Pool account
        /// </summary>
        Pool,
        /// <summary>
        /// Contract
        /// </summary>
        [Map("CONTRACT")]
        Contract,
        /// <summary>
        /// Isolated marging account
        /// </summary>
        [Map("ISOLATED")]
        Isolated,
        /// <summary>
        /// High Frequency (PRO Account) spot account
        /// </summary>
        [Map("TRADE_HF")]
        SpotHf,
        /// <summary>
        /// High Frequency (PRO Account) margin account
        /// </summary>
        [Map("MARGIN_V2")]
        MarginHf,
        /// <summary>
        /// High Frequency (PRO Account) isolated account
        /// </summary>
        [Map("ISOLATED_V2")]
        IsolatedMarginHf,
    }
}
