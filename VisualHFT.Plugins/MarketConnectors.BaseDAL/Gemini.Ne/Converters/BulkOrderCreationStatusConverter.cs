using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class BulkOrderCreationStatusConverter : BaseConverter<BulkOrderCreationStatus>
    {
        public BulkOrderCreationStatusConverter() : this(true) { }
        public BulkOrderCreationStatusConverter(bool quotes) : base(quotes) { }
        protected override List<KeyValuePair<BulkOrderCreationStatus, string>> Mapping => new List<KeyValuePair<BulkOrderCreationStatus, string>>
        {
            new KeyValuePair<BulkOrderCreationStatus, string>(BulkOrderCreationStatus.Success, "success"),
            new KeyValuePair<BulkOrderCreationStatus, string>(BulkOrderCreationStatus.Fail, "fail")
        };
    }
}
