using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Gemini.Net.Objects.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gemini.Net.Objects.Sockets.Queries
{
    internal class GeminiQuery : Query<GeminiSocketResponse>
    {
        public override HashSet<string> ListenerIdentifiers { get; set; }

        public GeminiQuery(string type, string topic, bool auth) : base(new GeminiRequest(ExchangeHelpers.NextId().ToString(), type, topic, auth), auth)
        {
            ListenerIdentifiers = new HashSet<string> { ((GeminiRequest)Request).Id };
        }

        public override CallResult<GeminiSocketResponse> HandleMessage(SocketConnection connection, DataEvent<GeminiSocketResponse> message)
        {
            var kucoinResponse = message.Data;
            if (string.Equals(kucoinResponse.Type, "error", StringComparison.Ordinal))
                return new CallResult<GeminiSocketResponse>(new ServerError(kucoinResponse.Code ?? 0, kucoinResponse.Data!));

            return base.HandleMessage(connection, message);
        }
    }
}
