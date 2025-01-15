using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Gemini.Net.Objects.Models.Futures;
using Gemini.Net.Objects.Models.Futures.Socket;
using Gemini.Net.Objects.Sockets.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Gemini.Net.Objects.Sockets.Subscriptions
{
    internal class GeminiPositionSubscription : Subscription<GeminiSocketResponse, GeminiSocketResponse>
    {
        private readonly Action<DataEvent<GeminiPositionUpdate>>? _onPositionUpdate;
        private readonly Action<DataEvent<GeminiPositionMarkPriceUpdate>>? _onMarkPriceUpdate;
        private readonly Action<DataEvent<GeminiPositionFundingSettlementUpdate>>? _onFundingSettlementUpdate;
        private readonly Action<DataEvent<GeminiPositionRiskAdjustResultUpdate>>? _onRiskAdjustUpdate;
        private readonly string? _symbol;
        private readonly string _topic;
        private static readonly MessagePath _subjectPath = MessagePath.Get().Property("subject");
        private static readonly MessagePath _changeReasonPath = MessagePath.Get().Property("data").Property("changeReason");

        public override HashSet<string> ListenerIdentifiers { get; set; }

        public GeminiPositionSubscription(
            ILogger logger,
            string? symbol,
            Action<DataEvent<GeminiPositionUpdate>>? onPositionUpdate,
            Action<DataEvent<GeminiPositionMarkPriceUpdate>>? onMarkPriceUpdate,
            Action<DataEvent<GeminiPositionFundingSettlementUpdate>>? onFundingSettlementUpdate,
            Action<DataEvent<GeminiPositionRiskAdjustResultUpdate>>? onRiskAdjustUpdate
            ) : base(logger, true)
        {
            _symbol = symbol;
            _topic = symbol == null ? "/contract/positionAll" : "/contract/position:" + symbol;
            _onPositionUpdate = onPositionUpdate;
            _onMarkPriceUpdate = onMarkPriceUpdate;
            _onFundingSettlementUpdate = onFundingSettlementUpdate;
            _onRiskAdjustUpdate = onRiskAdjustUpdate;

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
            if (message.Data is GeminiSocketUpdate<GeminiPositionMarkPriceUpdate> markUpdate)
                _onMarkPriceUpdate?.Invoke(message.As(markUpdate.Data, markUpdate.Topic, null, SocketUpdateType.Update));
            if (message.Data is GeminiSocketUpdate<GeminiPositionUpdate> positionUpdate)
                _onPositionUpdate?.Invoke(message.As(positionUpdate.Data, positionUpdate.Topic, positionUpdate.Data.Symbol, SocketUpdateType.Update));
            if (message.Data is GeminiSocketUpdate<GeminiPositionFundingSettlementUpdate> fundUpdate)
                _onFundingSettlementUpdate?.Invoke(message.As(fundUpdate.Data, fundUpdate.Topic, null, SocketUpdateType.Update));
            if (message.Data is GeminiSocketUpdate<GeminiPositionRiskAdjustResultUpdate> riskAdjust)
                _onRiskAdjustUpdate?.Invoke(message.As(riskAdjust.Data, riskAdjust.Topic, null, SocketUpdateType.Update));

            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message)
        {
            var subject = message.GetValue<string>(_subjectPath);
            if (string.Equals(subject, "position.change", StringComparison.Ordinal))
            {
                var change = message.GetValue<string>(_changeReasonPath);
                if (change == null || string.Equals(change, "markPriceChange", StringComparison.Ordinal))
                    return typeof(GeminiSocketUpdate<GeminiPositionMarkPriceUpdate>);
                else
                    return typeof(GeminiSocketUpdate<GeminiPositionUpdate>);
            }
            if (string.Equals(subject, "position.settlement", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiPositionFundingSettlementUpdate>);
            if (string.Equals(subject, "position.adjustRiskLimit", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiPositionRiskAdjustResultUpdate>);

            return null;
        }
    }
}
