using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class DepositStatusConverter : BaseConverter<DepositStatus>
    {
        public DepositStatusConverter() : this(true) { }
        public DepositStatusConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<DepositStatus, string>> Mapping => new List<KeyValuePair<DepositStatus, string>>
        {
            new KeyValuePair<DepositStatus, string>(DepositStatus.Processing, "PROCESSING"),
            new KeyValuePair<DepositStatus, string>(DepositStatus.Success, "SUCCESS"),
            new KeyValuePair<DepositStatus, string>(DepositStatus.Failure, "FAILURE"),
        };
    }
}
