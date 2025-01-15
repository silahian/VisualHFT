using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Gemini.Net.Objects.Models.Futures.Socket;
using Gemini.Net.Objects.Sockets.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gemini.Net.Objects.Sockets.Subscriptions
{
    internal class GeminiInstrumentSubscription : Subscription<GeminiSocketResponse, GeminiSocketResponse>
    {
        private string _topic;
        private Action<DataEvent<GeminiStreamFuturesMarkIndexPrice>> _markIndexPriceHandler;
        private Action<DataEvent<GeminiStreamFuturesFundingRate>> _fundingRateHandler;
        private readonly MessagePath _subjectPath = MessagePath.Get().Property("subject");

        public override HashSet<string> ListenerIdentifiers { get; set;  }

        public GeminiInstrumentSubscription(ILogger logger,List<string>? symbols, Action<DataEvent<GeminiStreamFuturesMarkIndexPrice>> markIndexPriceHandler, Action<DataEvent<GeminiStreamFuturesFundingRate>> fundingRateHandler) : base(logger, false)
        {
            var topic = "/contract/instrument";
            _topic = symbols?.Any() == true ? topic + ":" + string.Join(",", symbols) : topic;
            _markIndexPriceHandler = markIndexPriceHandler;
            _fundingRateHandler = fundingRateHandler;

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
            if (message.Data is GeminiSocketUpdate<GeminiStreamFuturesMarkIndexPrice> markUpdate)
                _markIndexPriceHandler?.Invoke(message.As(markUpdate.Data, markUpdate.Topic, markUpdate.Topic.Split(new char[] { ':' }).Last(), SocketUpdateType.Update));

            if (message.Data is GeminiSocketUpdate<GeminiStreamFuturesFundingRate> fundingUpdate)
                _fundingRateHandler?.Invoke(message.As(fundingUpdate.Data, fundingUpdate.Topic, fundingUpdate.Topic.Split(new char[] { ':' }).Last(), SocketUpdateType.Update));
            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message)
        {
            var subject = message.GetValue<string>(_subjectPath);
            if (string.Equals(subject, "mark.index.price", StringComparison.Ordinal))
                return typeof(GeminiSocketUpdate<GeminiStreamFuturesMarkIndexPrice>);
            return typeof(GeminiSocketUpdate<GeminiStreamFuturesFundingRate>);
        }
    }
}
