using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class OperationTypeConverter : BaseConverter<OrderOperationType>
    {
        public OperationTypeConverter() : this(true) { }
        public OperationTypeConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<OrderOperationType, string>> Mapping => new List<KeyValuePair<OrderOperationType, string>>
        {
            new KeyValuePair<OrderOperationType, string>(OrderOperationType.Deal, "DEAL"),
            new KeyValuePair<OrderOperationType, string>(OrderOperationType.Cancel, "CANCEL")
        };
    }
}
