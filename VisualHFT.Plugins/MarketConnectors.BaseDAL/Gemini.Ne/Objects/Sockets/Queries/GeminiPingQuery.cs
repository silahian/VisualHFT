using CryptoExchange.Net.Sockets;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Sockets.Queries
{
    internal class GeminiPingQuery : Query<GeminiPong>
    {
        public override HashSet<string> ListenerIdentifiers { get; set; }

        public GeminiPingQuery(string id) : base(new GeminiPing { Id = id, Type = "ping" }, false)
        {
            ListenerIdentifiers = new HashSet<string> { id };
        }
    }
}
