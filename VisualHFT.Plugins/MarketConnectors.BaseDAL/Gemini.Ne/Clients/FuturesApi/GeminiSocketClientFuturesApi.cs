using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Gemini.Net.Enums;
using Gemini.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Threading;
using Gemini.Net.Objects.Internal;
using Gemini.Net.Objects.Models;
using Gemini.Net.Objects.Models.Futures;
using Gemini.Net.Objects.Models.Futures.Socket;
using Gemini.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Authentication;
using Gemini.Net.Interfaces.Clients.FuturesApi;
using System.Linq;
using Gemini.Net.Objects.Options;
using CryptoExchange.Net.Converters;
using Gemini.Net.Objects.Sockets.Queries;
using Gemini.Net.Objects.Sockets.Subscriptions;
using CryptoExchange.Net.Objects.Sockets;
using System.Collections.Generic;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Clients;
using Newtonsoft.Json;
using Gemini.Net.Converters;
using CryptoExchange.Net.SharedApis;

namespace Gemini.Net.Clients.FuturesApi
{
    /// <inheritdoc cref="IGeminiSocketClientFuturesApi" />
    internal partial class GeminiSocketClientFuturesApi : SocketApiClient, IGeminiSocketClientFuturesApi
    {
        private static readonly MessagePath _idPath = MessagePath.Get().Property("id");
        private static readonly MessagePath _typePath = MessagePath.Get().Property("type");
        private static readonly MessagePath _topicPath = MessagePath.Get().Property("topic");

        private readonly GeminiSocketClient _baseClient;

        /// <inheritdoc />
        public new GeminiSocketOptions ClientOptions => (GeminiSocketOptions)base.ClientOptions;

        internal GeminiSocketClientFuturesApi(ILogger logger, GeminiSocketClient baseClient, GeminiSocketOptions options)
            : base(logger, options.Environment.FuturesAddress, options, options.FuturesOptions)
        {
            _baseClient = baseClient;

            AddSystemSubscription(new GeminiWelcomeSubscription(_logger));
            RegisterPeriodicQuery("Ping", TimeSpan.FromSeconds(30), x => new GeminiPingQuery(DateTimeConverter.ConvertToMilliseconds(DateTime.UtcNow).ToString()), null);
        }

        /// <inheritdoc />
        public override string GetListenerIdentifier(IMessageAccessor message)
        {
            var type = message.GetValue<string>(_typePath);
            if (string.Equals(type, "welcome", StringComparison.Ordinal))
                return type!;

            var id = message.GetValue<string>(_idPath);
            if (!string.Equals(type, "message", StringComparison.Ordinal) && id != null)
                return id;

            return message.GetValue<string>(_topicPath)!;
        }

        public IGeminiSocketClientFuturesApiShared SharedClient => this;

        /// <inheritdoc />
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new GeminiAuthenticationProvider((GeminiApiCredentials)credentials);


        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverTime = null)
            => GeminiExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverTime);

        /// <inheritdoc />

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamFuturesMatch>> onData, CancellationToken ct = default)
            => SubscribeToTradeUpdatesAsync(new[] { symbol }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamFuturesMatch>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiStreamFuturesMatch>(_logger, "/contractMarket/execution", symbols.ToList(), onData, false);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<DataEvent<GeminiStreamFuturesKline>> onData, CancellationToken ct = default)
            => SubscribeToKlineUpdatesAsync(new[] { symbol }, interval, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(IEnumerable<string> symbols, KlineInterval interval, Action<DataEvent<GeminiStreamFuturesKline>> onData, CancellationToken ct = default)
        {
            var symbolTopics = symbols.Select(x => x + "_" + JsonConvert.SerializeObject(interval, new KlineIntervalConverter(false))).ToList();
            var subscription = new GeminiSubscription<GeminiStreamFuturesKlineUpdate>(_logger, "/contractMarket/limitCandle", symbolTopics, x => onData(x.As(x.Data.Klines).WithSymbol(x.Data.Symbol)), false);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamFuturesTick>> onData, CancellationToken ct = default)
            => SubscribeToTickerUpdatesAsync(new[] { symbol }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamFuturesTick>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiStreamFuturesTick>(_logger, "/contractMarket/tickerV2", symbols.ToList(), onData, false);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(string symbol, Action<DataEvent<GeminiFuturesOrderBookChange>> onData, CancellationToken ct = default)
            => SubscribeToOrderBookUpdatesAsync(new[] { symbol }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiFuturesOrderBookChange>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiFuturesOrderBookChange>(_logger, "/contractMarket/level2", symbols.ToList(), onData, false);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToPartialOrderBookUpdatesAsync(string symbol, int limit, Action<DataEvent<GeminiStreamOrderBookChanged>> onData, CancellationToken ct = default)
            => SubscribeToPartialOrderBookUpdatesAsync(new[] { symbol }, limit, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToPartialOrderBookUpdatesAsync(IEnumerable<string> symbols, int limit, Action<DataEvent<GeminiStreamOrderBookChanged>> onData, CancellationToken ct = default)
        {
            limit.ValidateIntValues(nameof(limit), 5, 50);

            var subscription = new GeminiSubscription<GeminiStreamOrderBookChanged>(_logger, $"/contractMarket/level2Depth{limit}", symbols.ToList(), onData, false);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToMarketUpdatesAsync(string symbol,
            Action<DataEvent<GeminiStreamFuturesMarkIndexPrice>> onMarkIndexPriceUpdate,
            Action<DataEvent<GeminiStreamFuturesFundingRate>> onFundingRateUpdate, CancellationToken ct = default)
            => SubscribeToMarketUpdatesAsync(new[] { symbol }, onMarkIndexPriceUpdate, onFundingRateUpdate, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToMarketUpdatesAsync(IEnumerable<string> symbols,
            Action<DataEvent<GeminiStreamFuturesMarkIndexPrice>> onMarkIndexPriceUpdate,
            Action<DataEvent<GeminiStreamFuturesFundingRate>> onFundingRateUpdate,
            CancellationToken ct = default)
        {
            var subscription = new GeminiInstrumentSubscription(_logger, symbols.ToList(), onMarkIndexPriceUpdate, onFundingRateUpdate);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToSystemAnnouncementsAsync(Action<DataEvent<GeminiContractAnnouncement>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiFundingFeeSettlementSubscription(_logger, onData);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeTo24HourSnapshotUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamTransactionStatisticsUpdate>> onData, CancellationToken ct = default)
            => SubscribeTo24HourSnapshotUpdatesAsync(new[] { symbol }, onData, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeTo24HourSnapshotUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamTransactionStatisticsUpdate>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiStreamTransactionStatisticsUpdate>(_logger, $"/contractMarket/snapshot", symbols.ToList(), onData, false);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(string? symbol,
            Action<DataEvent<GeminiStreamFuturesOrderUpdate>> onData,
            CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiStreamFuturesOrderUpdate>(_logger, $"/contractMarket/tradeOrders", symbol != null ? new List<string> { symbol }: null, onData, true);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToStopOrderUpdatesAsync(Action<DataEvent<GeminiStreamFuturesStopOrderUpdate>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<GeminiStreamFuturesStopOrderUpdate>(_logger, $"/contractMarket/advancedOrders", null, onData, true);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToBalanceUpdatesAsync(
            Action<DataEvent<GeminiStreamOrderMarginUpdate>>? onOrderMarginUpdate = null,
            Action<DataEvent<GeminiStreamFuturesBalanceUpdate>>? onBalanceUpdate = null,
            Action<DataEvent<GeminiStreamFuturesWithdrawableUpdate>>? onWithdrawableUpdate = null,
            Action<DataEvent<GeminiStreamFuturesWalletUpdate>>? onWalletUpdate = null,
            CancellationToken ct = default)
        {
            var subscription = new GeminiBalanceSubscription(_logger, onOrderMarginUpdate, onBalanceUpdate, onWithdrawableUpdate, onWalletUpdate);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToPositionUpdatesAsync(
            string symbol,
            Action<DataEvent<GeminiPositionUpdate>>? onPositionUpdate = null,
            Action<DataEvent<GeminiPositionMarkPriceUpdate>>? onMarkPriceUpdate = null,
            Action<DataEvent<GeminiPositionFundingSettlementUpdate>>? onFundingSettlementUpdate = null,
            Action<DataEvent<GeminiPositionRiskAdjustResultUpdate>>? onRiskAdjustUpdate = null,
            CancellationToken ct = default)
        {
            var subscription = new GeminiPositionSubscription(_logger, symbol, onPositionUpdate, onMarkPriceUpdate, onFundingSettlementUpdate, onRiskAdjustUpdate);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToPositionUpdatesAsync(
            Action<DataEvent<GeminiPositionUpdate>>? onPositionUpdate = null,
            Action<DataEvent<GeminiPositionMarkPriceUpdate>>? onMarkPriceUpdate = null,
            Action<DataEvent<GeminiPositionFundingSettlementUpdate>>? onFundingSettlementUpdate = null,
            Action<DataEvent<GeminiPositionRiskAdjustResultUpdate>>? onRiskAdjustUpdate = null,
            CancellationToken ct = default)
        {
            var subscription = new GeminiPositionSubscription(_logger, null, onPositionUpdate, onMarkPriceUpdate, onFundingSettlementUpdate, onRiskAdjustUpdate);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToMarginModeUpdatesAsync(Action<DataEvent<Dictionary<string, FuturesMarginMode>>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<Dictionary<string, FuturesMarginMode>>(_logger, $"/contract/marginMode", null, onData, true);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToCrossMarginLeverageUpdatesAsync(Action<DataEvent<Dictionary<string, GeminiLeverageUpdate>>> onData, CancellationToken ct = default)
        {
            var subscription = new GeminiSubscription<Dictionary<string, GeminiLeverageUpdate>>(_logger, $"/contract/crossLeverage", null, onData, true);
            return await SubscribeAsync("futures", subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override async Task<CallResult<string?>> GetConnectionUrlAsync(string address, bool authenticated)
        {
            if (ClientOptions.Environment.EnvironmentName == "UnitTesting")
                return new CallResult<string?>("wss://ws-api-spot.gemini.com");

            var apiCredentials = (GeminiApiCredentials?)(ApiOptions.ApiCredentials ?? _baseClient.ClientOptions.ApiCredentials ?? GeminiSocketOptions.Default.ApiCredentials ?? GeminiRestOptions.Default.ApiCredentials);
            using (var restClient = new GeminiRestClient((options) =>
            {
                options.ApiCredentials = apiCredentials;
            }))
            {
                WebCallResult<GeminiToken> tokenResult;
                if (authenticated)
                    tokenResult = await ((GeminiRestClientFuturesApiAccount)restClient.FuturesApi.Account).GetWebsocketTokenPrivateAsync().ConfigureAwait(false);
                else
                    tokenResult = await ((GeminiRestClientFuturesApiAccount)restClient.FuturesApi.Account).GetWebsocketTokenPublicAsync().ConfigureAwait(false);
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
