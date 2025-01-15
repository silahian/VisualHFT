using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class AccountDirectionConverter : BaseConverter<AccountDirection>
    {
        public AccountDirectionConverter() : this(true) { }
        public AccountDirectionConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<AccountDirection, string>> Mapping => new List<KeyValuePair<AccountDirection, string>>
        {
            new KeyValuePair<AccountDirection, string>(AccountDirection.In, "in"),
            new KeyValuePair<AccountDirection, string>(AccountDirection.Out, "out")
        };
    }
}
