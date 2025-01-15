using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class AccountTypeConverter : BaseConverter<AccountType>
    {
	    private readonly bool _useCaps;
        public AccountTypeConverter() : this(true) { }

        public AccountTypeConverter(bool quotes, bool useCaps = false) : base(quotes)
        {
	        _useCaps = useCaps;
        }
        protected override List<KeyValuePair<AccountType, string>> Mapping => new List<KeyValuePair<AccountType, string>>
        {
            new KeyValuePair<AccountType, string>(AccountType.Main, _useCaps ? "MAIN" : "main"),
            new KeyValuePair<AccountType, string>(AccountType.Trade, _useCaps ? "TRADE" : "trade"),
            new KeyValuePair<AccountType, string>(AccountType.Margin, _useCaps ? "MARGIN" : "margin"),
            new KeyValuePair<AccountType, string>(AccountType.Pool, _useCaps ? "POOL" : "pool"),
            new KeyValuePair<AccountType, string>(AccountType.Isolated, _useCaps ? "ISOLATED" : "isolated"),
            new KeyValuePair<AccountType, string>(AccountType.SpotHf, _useCaps ? "TRADE_HF" : "trade_hf")
        };
    }
}
