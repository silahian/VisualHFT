using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class BorrowOrderTypeConverter : BaseConverter<BorrowOrderType>
    {
        public BorrowOrderTypeConverter() : this(true) { }
        public BorrowOrderTypeConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<BorrowOrderType, string>> Mapping => new List<KeyValuePair<BorrowOrderType, string>>
        {
            new KeyValuePair<BorrowOrderType, string>(BorrowOrderType.FOK, "FOK"),
            new KeyValuePair<BorrowOrderType, string>(BorrowOrderType.IOC, "IOC")
        };
    }
}
