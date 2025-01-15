using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Gemini.Net.Objects.Models.Spot.Socket;
using Gemini.Net.Objects.Sockets.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Sockets.Subscriptions
{
    internal class GeminiIsolatedMarginPositionSubscription : Subscription<GeminiSocketResponse, GeminiSocketResponse>
    {
        private readonly Action<DataEvent<GeminiIsolatedMarginPositionUpdate>>? _onPositionChange;
        private string _topic;

        public override HashSet<string> ListenerIdentifiers { get; set; }

        public GeminiIsolatedMarginPositionSubscription(
            ILogger logger,
            string symbol,
            Action<DataEvent<GeminiIsolatedMarginPositionUpdate>>? onPositionChange
            ) : base(logger, true)
        {
            _onPositionChange = onPositionChange;
            _topic = "/margin/isolatedPosition:" + symbol;

            ListenerIdentifiers = new HashSet<string> { _topic };
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
            var data = (GeminiSocketUpdate<GeminiIsolatedMarginPositionUpdate>)message.Data;
            _onPositionChange?.Invoke(message.As(data.Data, data.Topic, data.Data.Tag, SocketUpdateType.Update));

            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message)
        {
            return typeof(GeminiSocketUpdate<GeminiIsolatedMarginPositionUpdate>);
        }
    }
}
