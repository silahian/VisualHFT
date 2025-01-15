using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class WithdrawalStatusConverter : BaseConverter<WithdrawalStatus>
    {
        public WithdrawalStatusConverter() : this(true) { }
        public WithdrawalStatusConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<WithdrawalStatus, string>> Mapping => new List<KeyValuePair<WithdrawalStatus, string>>
        {
            new KeyValuePair<WithdrawalStatus, string>(WithdrawalStatus.Processing, "PROCESSING"),
            new KeyValuePair<WithdrawalStatus, string>(WithdrawalStatus.Success, "SUCCESS"),
            new KeyValuePair<WithdrawalStatus, string>(WithdrawalStatus.WalletProcessing, "WALLET_PROCESSING"),
            new KeyValuePair<WithdrawalStatus, string>(WithdrawalStatus.Failure, "FAILURE"),
        };
    }
}
