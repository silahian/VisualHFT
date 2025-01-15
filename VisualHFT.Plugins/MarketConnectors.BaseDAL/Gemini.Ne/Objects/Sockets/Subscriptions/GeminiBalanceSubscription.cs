using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Gemini.Net.Objects.Models.Futures.Socket;
using Gemini.Net.Objects.Models.Spot.Socket;
using Gemini.Net.Objects.Sockets.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gemini.Net.Objects.Sockets.Subscriptions
{
    internal class GeminiBalanceSubscription : Subscription<GeminiSocketResponse, GeminiSocketResponse>
    {
        private readonly Action<DataEvent<GeminiStreamFuturesWalletUpdate>>? _onWalletUpdate;
        private readonly Action<DataEvent<GeminiStreamOrderMarginUpdate>>? _onOrderMarginUpdate;
        private readonly Action<DataEvent<GeminiStreamFuturesBalanceUpdate>>? _onBalanceUpdate;
        private readonly Action<DataEvent<GeminiStreamFuturesWithdrawableUpdate>>? _onWithdrawableUpdate;
        private readonly string _topic = "/contractAccount/wallet";
        private static readonly MessagePath _subjectPath = MessagePath.Get().Property("subject");

        public override HashSet<string> ListenerIdentifiers { get; set; }

        public GeminiBalanceSubscription(
            ILogger logger,
            Action<DataEvent<GeminiStreamOrderMarginUpdate>>? onOrderMarginUpdate,
            Action<DataEvent<GeminiStreamFuturesBalanceUpdate>>? onBalanceUpdate,
            Action<DataEvent<GeminiStreamFuturesWithdrawableUpdate>>? onWithdrawableUpdate,
            Action<DataEvent<GeminiStreamFuturesWalletUpdate>>? onWalletUpdate
            ) : base(logger, true)
        {
            _onOrderMarginUpdate = onOrderMarginUpdate;
            _onBalanceUpdate = onBalanceUpdate;
            _onWithdrawableUpdate = onWithdrawableUpdate;
            _onWalletUpdate = onWalletUpdate;

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
            if (message.Data is GeminiSocketUpdate<GeminiStreamFuturesWalletUpdate> walletUpdate)
                _onWalletUpdate?.Invoke(message.As(walletUpdate.Data, walletUpdate.Topic, null, SocketUpdateType.Update));
            if (message.Data is GeminiSocketUpdate<GeminiStreamOrderMarginUpdate> marginUpdate)
                _onOrderMarginUpdate?.Invoke(message.As(marginUpdate.Data, marginUpdate.Topic, null, SocketUpdateType.Update));
            if (message.Data is GeminiSocketUpdate<GeminiStreamFuturesBalanceUpdate> balanceUpdate)
                _onBalanceUpdate?.Invoke(message.As(balanceUpdate.Data, balanceUpdate.Topic, null, SocketUpdateType.Update));
            if (message.Data is GeminiSocketUpdate<GeminiStreamFuturesWithdrawableUpdate> withdrawableUpdate)
                _onWithdrawableUpdate?.Invoke(message.As(withdrawableUpdate.Data, withdrawableUpdate.Topic, null, SocketUpdateType.Update));

            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message)
        {
            var subject = message.GetValue<string>(_subjectPath);
            if (string.Equals(subject, "walletBalance.change", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiStreamFuturesWalletUpdate>);
            if (string.Equals(subject, "orderMargin.change", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiStreamOrderMarginUpdate>);
            if (string.Equals(subject, "availableBalance.change", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiStreamFuturesBalanceUpdate>);
            return typeof(GeminiSocketUpdate<GeminiStreamFuturesWithdrawableUpdate>);
        }
    }
}
