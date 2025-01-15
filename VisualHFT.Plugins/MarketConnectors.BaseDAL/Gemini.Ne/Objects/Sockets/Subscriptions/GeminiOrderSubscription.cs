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
using System.Threading.Tasks;

namespace Gemini.Net.Objects.Sockets.Subscriptions
{
    internal class GeminiOrderSubscription : Subscription<GeminiSocketResponse, GeminiSocketResponse>
    {
        private readonly Action<DataEvent<GeminiStreamOrderNewUpdate>>? _onNewOrder;
        private readonly Action<DataEvent<GeminiStreamOrderUpdate>>? _onOrderData;
        private readonly Action<DataEvent<GeminiStreamOrderMatchUpdate>>? _onTradeData;
        private readonly string _topic = "/spotMarket/tradeOrdersV2";
        private static readonly MessagePath _typePath = MessagePath.Get().Property("data").Property("type");

        public override HashSet<string> ListenerIdentifiers { get; set; }

        public GeminiOrderSubscription(
            ILogger logger,
            Action<DataEvent<GeminiStreamOrderNewUpdate>>? onNewOrder,
            Action<DataEvent<GeminiStreamOrderUpdate>>? onOrderData,
            Action<DataEvent<GeminiStreamOrderMatchUpdate>>? onTradeData
            ) : base(logger, true)
        {
            _onOrderData = onOrderData;
            _onTradeData = onTradeData;
            _onNewOrder = onNewOrder;

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
            if (message.Data is GeminiSocketUpdate<GeminiStreamOrderMatchUpdate> matchUpdate)
                _onTradeData?.Invoke(message.As(matchUpdate.Data, matchUpdate.Topic, matchUpdate.Data.Symbol, SocketUpdateType.Update));
            if (message.Data is GeminiSocketUpdate<GeminiStreamOrderUpdate> orderUpdate)
                _onOrderData?.Invoke(message.As(orderUpdate.Data, orderUpdate.Topic, orderUpdate.Data.Symbol, SocketUpdateType.Update));
            if (message.Data is GeminiSocketUpdate<GeminiStreamOrderNewUpdate> newOrderUpdate)
                _onNewOrder?.Invoke(message.As(newOrderUpdate.Data, newOrderUpdate.Topic, newOrderUpdate.Data.Symbol, SocketUpdateType.Update));

            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message)
        {
            var type = message.GetValue<string>(_typePath);
            if (string.Equals(type, "match", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiStreamOrderMatchUpdate>);
            if (string.Equals(type, "received", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiStreamOrderNewUpdate>);
            return typeof(GeminiSocketUpdate<GeminiStreamOrderUpdate>);
        }
    }
}
