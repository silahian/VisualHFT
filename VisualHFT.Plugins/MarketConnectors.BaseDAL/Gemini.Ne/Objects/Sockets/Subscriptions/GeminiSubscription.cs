using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Gemini.Net.Objects.Sockets.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gemini.Net.Objects.Sockets.Subscriptions
{
    internal class GeminiSubscription<T> : Subscription<GeminiSocketResponse, GeminiSocketResponse>
    {
        private string _topic;
        private Action<DataEvent<T>> _handler;

        public override HashSet<string> ListenerIdentifiers { get; set;  }

        public GeminiSubscription(ILogger logger, string topic, List<string>? symbols, Action<DataEvent<T>> handler, bool authenticated) : base(logger, authenticated)
        {
            _topic = symbols?.Any() == true ? topic + ":" + string.Join(",", symbols) : topic;
            _handler = handler;

            ListenerIdentifiers = symbols?.Any() == true ? new HashSet<string>(symbols.Select(s => topic + ":" + s)) : new HashSet<string> { topic };
        }

        public override Query? GetSubQuery(SocketConnection connection)
        {
            return new GeminiQuery("subscribe", _topic, Authenticated);
        }

        public override Query? GetUnsubQuery()
        {
            return new GeminiQuery("unsubscribe", _topic, Authenticated);
        }

        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
        {
            var data = (GeminiSocketUpdate<T>)message.Data;
            string? topic = data.Topic.Contains(":") ? data.Topic.Split(':').Last() : null;
            if (string.Equals(topic, "all", StringComparison.Ordinal))
                topic = data.Subject;

            _handler.Invoke(message.As(data.Data, data.Topic, topic, SocketUpdateType.Update));
            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message) => typeof(GeminiSocketUpdate<T>);
    }
}
