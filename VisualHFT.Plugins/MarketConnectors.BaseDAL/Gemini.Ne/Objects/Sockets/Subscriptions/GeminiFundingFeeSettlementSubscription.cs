using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Gemini.Net.Objects.Models.Futures.Socket;
using Gemini.Net.Objects.Sockets.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gemini.Net.Objects.Sockets.Subscriptions
{
    internal class GeminiFundingFeeSettlementSubscription : Subscription<GeminiSocketResponse, GeminiSocketResponse>
    {
        private string _topic;
        private Action<DataEvent<GeminiContractAnnouncement>> _dataHandler;

        public override HashSet<string> ListenerIdentifiers { get; set; } = new HashSet<string> { "/contract/announcement" };

        public GeminiFundingFeeSettlementSubscription(ILogger logger, Action<DataEvent<GeminiContractAnnouncement>> dataHandler) : base(logger, false)
        {
            _topic = "/contract/announcement";
            _dataHandler = dataHandler;
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
            var data = (GeminiSocketUpdate<GeminiContractAnnouncement>)message.Data;
            data.Data.Event = data.Subject;
            _dataHandler.Invoke(message.As(data.Data, data.Topic, data.Data.Symbol, SocketUpdateType.Update));
            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message) => typeof(GeminiSocketUpdate<GeminiContractAnnouncement>);
    }
}
