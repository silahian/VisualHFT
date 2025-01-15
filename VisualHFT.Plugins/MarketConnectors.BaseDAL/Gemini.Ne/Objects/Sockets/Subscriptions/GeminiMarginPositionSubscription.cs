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
    internal class GeminiMarginPositionSubscription : Subscription<GeminiSocketResponse, GeminiSocketResponse>
    {
        private readonly Action<DataEvent<GeminiMarginDebtRatioUpdate>>? _onDebtRatioChange;
        private readonly Action<DataEvent<GeminiMarginPositionStatusUpdate>>? _onPositionStatusChange;
        private readonly string _topic = "/margin/position";
        private static readonly MessagePath _subjectPath = MessagePath.Get().Property("subject");

        public override HashSet<string> ListenerIdentifiers { get; set; }

        public GeminiMarginPositionSubscription(
            ILogger logger,
            Action<DataEvent<GeminiMarginDebtRatioUpdate>>? onDebtRatioChange,
            Action<DataEvent<GeminiMarginPositionStatusUpdate>>? onPositionStatusChange
            ) : base(logger, true)
        {
            _onDebtRatioChange = onDebtRatioChange;
            _onPositionStatusChange = onPositionStatusChange;

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
            if (message.Data is GeminiSocketUpdate<GeminiMarginDebtRatioUpdate> debtUpdate)
                _onDebtRatioChange?.Invoke(message.As(debtUpdate.Data, debtUpdate.Topic, null, SocketUpdateType.Update));
            if (message.Data is GeminiSocketUpdate<GeminiMarginPositionStatusUpdate> statusUpdate)
                _onPositionStatusChange?.Invoke(message.As(statusUpdate.Data, statusUpdate.Topic, null, SocketUpdateType.Update));

            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message)
        {
            var type = message.GetValue<string>(_subjectPath);
            if (string.Equals(type, "debt.ratio", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiMarginDebtRatioUpdate>);
            if (string.Equals(type, "position.status", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiMarginPositionStatusUpdate>);

            return null;
        }
    }
}
