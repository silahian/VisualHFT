using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Gemini.Net.Objects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using System.Threading;
using Gemini.Net.Objects.Internal;
using Gemini.Net.Objects.Models;
using Gemini.Net.Objects.Models.Futures.Socket;
using Gemini.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Authentication;
using Gemini.Net.Interfaces.Clients.SpotApi;
using System.Linq;
using Gemini.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;
using Gemini.Net.Objects.Sockets.Subscriptions;
using CryptoExchange.Net.Converters;
using Gemini.Net.Objects.Sockets.Queries;
using Gemini.Net.ExtensionMethods;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.SharedApis;

namespace Gemini.Net.Clients.SpotApi
{
    /// <inheritdoc cref="IGeminiSocketClientSpotApi" />
    internal partial class GeminiSocketClientSpotApi : SocketApiClient, IGeminiSocketClientSpotApi
    {
        private readonly GeminiSocketClient _baseClient;
        private static readonly MessagePath _idPath = MessagePath.Get().Property("id");
        private static readonly MessagePath _typePath = MessagePath.Get().Property("type");
        private static readonly MessagePath _topicPath = MessagePath.Get().Property("topic");

        /// <inheritdoc />
        public new GeminiSocketOptions ClientOptions => (GeminiSocketOptions)base.ClientOptions;

        internal GeminiSocketClientSpotApi(ILogger logger, GeminiSocketClient baseClient, GeminiSocketOptions options)
            : base(logger, options.Environment.SpotAddress, options, options.SpotOptions)
        {
            _baseClient = baseClient;

            AddSystemSubscription(new GeminiWelcomeSubscription(_logger));
            RegisterPeriodicQuery("Ping", TimeSpan.FromSeconds(30), x => new GeminiPingQuery(DateTimeConverter.ConvertToMilliseconds(DateTime.UtcNow).ToString()), null);
        }

        /// <inheritdoc />
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new GeminiAuthenticationProvider((GeminiApiCredentials)credentials);
        
        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverTime = null)
            => GeminiExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverTime);

        public IGeminiSocketClientSpotApiShared SharedClient => this;

        /// <inheritdoc />
        public override string GetListenerIdentifier(IMessageAccessor message)
        {
            var type = message.GetValue<string>(_typePath);
            if (string.Equals(type, "welcome", StringComparison.Ordinal))
                return type!;

            var topic = message.GetValue<string>(_topicPath);
            var id = message.GetValue<string>(_idPath);
            if (id != null)
            {
                if (string.Equals(topic, "/account/balance", StringComparison.Ordinal)
                    || topic?.StartsWith("/margin/fundingBook", StringComparison.Ordinal) == true)
                {
                    // This update also contain an id field, but should be identified by the topic regardless
                    return topic!;
                }

                return id;
            }

            return topic!;
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamTick>> onData, CancellationToken ct = default) => SubscribeToTickerUpdatesAsync(new[] { symbol }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamTick>> onData, CancellationToken ct = default)
        {
            symbols.ValidateNotNull(nameof(symbols));

            var subscription = new GeminiSubscription<GeminiStreamTick>(_logger, "/market/ticker", symbols.ToList(), onData, false);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToAllTickerUpdatesAsync(Action<DataEvent<GeminiStreamTick>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiStreamTick>(_logger, "/market/ticker:all", null, onData, false);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToSnapshotUpdatesAsync(string symbolOrMarket,
            Action<DataEvent<GeminiStreamSnapshot>> onData, CancellationToken ct = default)
            => SubscribeToSnapshotUpdatesAsync(new[] { symbolOrMarket }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToSnapshotUpdatesAsync(
            IEnumerable<string> symbolOrMarkets, Action<DataEvent<GeminiStreamSnapshot>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiStreamSnapshotWrapper>(_logger, "/market/snapshot", symbolOrMarkets.ToList(), x => onData(x.As(x.Data.Data)), false);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToBookTickerUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamBestOffers>> onData, CancellationToken ct = default) => SubscribeToBookTickerUpdatesAsync(new[] { symbol }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToBookTickerUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamBestOffers>> onData, CancellationToken ct = default)
        {
            symbols.ValidateNotNull(nameof(symbols));

            var subscription = new GeminiSubscription<GeminiStreamBestOffers>(_logger, "/spotMarket/level1", symbols.ToList(), onData, false);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToAggregatedOrderBookUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamOrderBook>> onData, CancellationToken ct = default) => SubscribeToAggregatedOrderBookUpdatesAsync(new[] { symbol }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToAggregatedOrderBookUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamOrderBook>> onData, CancellationToken ct = default)
        {
            symbols.ValidateNotNull(nameof(symbols));

            var subscription = new GeminiSubscription<GeminiStreamOrderBook>(_logger, "/market/level2", symbols.ToList(), onData, false);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamMatch>> onData, CancellationToken ct = default) => SubscribeToTradeUpdatesAsync(new[] { symbol }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamMatch>> onData, CancellationToken ct = default)
        {
            symbols.ValidateNotNull(nameof(symbols));

            var subscription = new GeminiSubscription<GeminiStreamMatch>(_logger, "/market/match", symbols.ToList(), onData, false);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<DataEvent<GeminiStreamCandle>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiStreamCandle>(_logger, $"/market/candles", new List<string> { $"{symbol}_{JsonConvert.SerializeObject(interval, new KlineIntervalConverter(false))}" }, onData, false);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(string symbol, int limit,
            Action<DataEvent<GeminiStreamOrderBookChanged>> onData, CancellationToken ct = default) =>
            SubscribeToOrderBookUpdatesAsync(new[] { symbol }, limit, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(IEnumerable<string> symbols, int limit, Action<DataEvent<GeminiStreamOrderBookChanged>> onData, CancellationToken ct = default)
        {
            limit.ValidateIntValues(nameof(limit), 5, 50);

            var subscription = new GeminiSubscription<GeminiStreamOrderBookChanged>(_logger, $"/spotMarket/level2Depth{limit}", symbols.ToList(), onData, false);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToIndexPriceUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamIndicatorPrice>> onData, CancellationToken ct = default) => SubscribeToIndexPriceUpdatesAsync(new string[] { symbol }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToIndexPriceUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamIndicatorPrice>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiStreamIndicatorPrice>(_logger, $"/indicator/index", symbols.ToList(), onData, false);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToMarkPriceUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamIndicatorPrice>> onData, CancellationToken ct = default) => SubscribeToMarkPriceUpdatesAsync(new string[] { symbol }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToMarkPriceUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamIndicatorPrice>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiStreamIndicatorPrice>(_logger, $"/indicator/markPrice", symbols.ToList(), onData, false);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToFundingBookUpdatesAsync(string asset, Action<DataEvent<GeminiStreamFundingBookUpdate>> onData, CancellationToken ct = default) => SubscribeToFundingBookUpdatesAsync(new string[] { asset }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToFundingBookUpdatesAsync(IEnumerable<string> assets, Action<DataEvent<GeminiStreamFundingBookUpdate>> onData, CancellationToken ct = default)
        {
            foreach (var asset in assets)
                asset.ValidateNotNull(asset);

            var subscription = new GeminiSubscription<GeminiStreamFundingBookUpdate>(_logger, $"/margin/fundingBook", assets.ToList(), onData, false);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(
            Action<DataEvent<GeminiStreamOrderNewUpdate>>? onNewOrder = null,
            Action<DataEvent<GeminiStreamOrderUpdate>>? onOrderData = null,
            Action<DataEvent<GeminiStreamOrderMatchUpdate>>? onTradeData = null,
            CancellationToken ct = default)
        {
            var subscription = new GeminiOrderSubscription(_logger, onNewOrder, onOrderData, onTradeData);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToBalanceUpdatesAsync(Action<DataEvent<GeminiBalanceUpdate>> onBalanceChange, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiBalanceUpdate>(_logger, "/account/balance", null, onBalanceChange, true);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToStopOrderUpdatesAsync(Action<DataEvent<GeminiStreamStopOrderUpdateBase>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiStreamStopOrderUpdateBase>(_logger, "/spotMarket/advancedOrders", null, onData, true);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToIsolatedMarginPositionUpdatesAsync(string symbol, Action<DataEvent<GeminiIsolatedMarginPositionUpdate>> onPositionChange, CancellationToken ct = default)
        {
            var subscription = new GeminiIsolatedMarginPositionSubscription(_logger, symbol, onPositionChange);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToMarginPositionUpdatesAsync(Action<DataEvent<GeminiMarginDebtRatioUpdate>> onDebtRatioChange, Action<DataEvent<GeminiMarginPositionStatusUpdate>> onPositionStatusChange, CancellationToken ct = default)
        {
            var subscription = new GeminiMarginPositionSubscription(_logger, onDebtRatioChange, onPositionStatusChange);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToMarginOrderUpdatesAsync(string symbol, Action<DataEvent<GeminiMarginOrderUpdate>>? onOrderPlaced = null, Action<DataEvent<GeminiMarginOrderUpdate>>? onOrderUpdate = null, Action<DataEvent<GeminiMarginOrderDoneUpdate>>? onOrderDone = null, CancellationToken ct = default)
        {
            var subscription = new GeminiMarginOrderSubscription(_logger, symbol, onOrderPlaced, onOrderUpdate, onOrderDone);
            return await SubscribeAsync("spot", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override async Task<CallResult<string?>> GetConnectionUrlAsync(string address, bool authenticated)
        {
            if (ClientOptions.Environment.EnvironmentName == "UnitTesting")
                return new CallResult<string?>("wss://ws-api-spot.gemini.com");

            var apiCredentials = (GeminiApiCredentials?)(ApiOptions.ApiCredentials ?? _baseClient.ClientOptions.ApiCredentials);
            using (var restClient = new GeminiRestClient((options) =>
            {
                options.ApiCredentials = apiCredentials;
                options.Environment = ClientOptions.Environment;
            }))
            {
                WebCallResult<GeminiToken> tokenResult;
                if (authenticated)
                    tokenResult = await ((GeminiRestClientSpotApiAccount)restClient.SpotApi.Account).GetWebsocketTokenPrivateAsync().ConfigureAwait(false);
                else
                    tokenResult = await ((GeminiRestClientSpotApiAccount)restClient.SpotApi.Account).GetWebsocketTokenPublicAsync().ConfigureAwait(false);
                if (!tokenResult)
                    return tokenResult.As<string?>(null);

                return new CallResult<string?>(tokenResult.Data.Servers.First().Endpoint + "?token=" + tokenResult.Data.Token);
            }
        }

        /// <inheritdoc />
        protected override async Task<Uri?> GetReconnectUriAsync(SocketConnection connection)
        {
            var result = await GetConnectionUrlAsync(connection.ConnectionUri.ToString(), connection.Subscriptions.Any(s => s.Authenticated)).ConfigureAwait(false);
            if (!result)
                return null;

            return new Uri(result.Data);
        }
    }
}
