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
    internal class GeminiMarginOrderSubscription : Subscription<GeminiSocketResponse, GeminiSocketResponse>
    {
        private readonly Action<DataEvent<GeminiMarginOrderUpdate>>? _onNewOrder;
        private readonly Action<DataEvent<GeminiMarginOrderUpdate>>? _onOrderData;
        private readonly Action<DataEvent<GeminiMarginOrderDoneUpdate>>? _onOrderDone;
        private readonly string _topic;
        private static readonly MessagePath _subjectPath = MessagePath.Get().Property("subject");

        public override HashSet<string> ListenerIdentifiers { get; set; }

        public GeminiMarginOrderSubscription(
            ILogger logger,
            string asset,
            Action<DataEvent<GeminiMarginOrderUpdate>>? onNewOrder,
            Action<DataEvent<GeminiMarginOrderUpdate>>? onOrderData,
            Action<DataEvent<GeminiMarginOrderDoneUpdate>>? onOrderDone
            ) : base(logger, true)
        {
            _onOrderData = onOrderData;
            _onOrderDone = onOrderDone;
            _onNewOrder = onNewOrder;

            _topic = "/margin/loan:" + asset;
;            ListenerIdentifiers = new HashSet<string> { _topic };
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
            if (message.Data is GeminiSocketUpdate<GeminiMarginOrderDoneUpdate> matchUpdate)
                _onOrderDone?.Invoke(message.As(matchUpdate.Data, matchUpdate.Topic, null, SocketUpdateType.Update));
            if (message.Data is GeminiSocketUpdate<GeminiMarginOrderUpdate> orderUpdate)
            {
                if (string.Equals(orderUpdate.Subject, "order.open", StringComparison.Ordinal))
                    _onNewOrder?.Invoke(message.As(orderUpdate.Data, orderUpdate.Topic, null, SocketUpdateType.Update));
                else
                    _onOrderData?.Invoke(message.As(orderUpdate.Data, orderUpdate.Topic, null, SocketUpdateType.Update));
            }

            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message)
        {
            var type = message.GetValue<string>(_subjectPath);
            if (string.Equals(type, "order.open", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiMarginOrderUpdate>);
            if (string.Equals(type, "order.update", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiMarginOrderUpdate>);
            if (string.Equals(type, "order.done", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiMarginOrderDoneUpdate>);

            return null;
        }
    }
}
