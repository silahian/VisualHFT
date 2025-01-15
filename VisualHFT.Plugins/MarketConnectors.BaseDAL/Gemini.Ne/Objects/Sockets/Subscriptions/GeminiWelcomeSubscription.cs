using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gemini.Net.Objects.Sockets.Subscriptions
{
    internal class GeminiWelcomeSubscription : SystemSubscription<GeminiWelcome>
    {
        public override HashSet<string> ListenerIdentifiers { get; set; } = new HashSet<string>() { "welcome" };

        public GeminiWelcomeSubscription(ILogger logger) : base(logger, false)
        {
        }

        public override CallResult HandleMessage(SocketConnection connection, DataEvent<GeminiWelcome> message) => new CallResult(null);
    }
}
