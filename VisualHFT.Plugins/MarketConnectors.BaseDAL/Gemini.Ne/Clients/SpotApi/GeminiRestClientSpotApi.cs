using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.CommonClients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using Gemini.Net.Enums;
using Gemini.Net.Interfaces.Clients.SpotApi;
using Gemini.Net.Objects;
using Gemini.Net.Objects.Internal;
using Gemini.Net.Objects.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gemini.Net.Clients.SpotApi
{
    /// <inheritdoc cref="IGeminiRestClientSpotApi" />
    internal partial class GeminiRestClientSpotApi : RestApiClient, IGeminiRestClientSpotApi, ISpotClient
    {
        internal static TimeSyncState _timeSyncState = new TimeSyncState("Spot Api");

        /// <summary>
        /// Event triggered when an order is placed via this client. Only available for Spot orders
        /// </summary>
        public event Action<OrderId>? OnOrderPlaced;
        /// <summary>
        /// Event triggered when an order is canceled via this client. Note that this does not trigger when using CancelAllOrdersAsync. Only available for Spot orders
        /// </summary>
        public event Action<OrderId>? OnOrderCanceled;

        /// <inheritdoc />
        public string ExchangeName => "Gemini";

        /// <inheritdoc />
        public IGeminiRestClientSpotApiAccount Account { get; }
        /// <inheritdoc />
        public IGeminiRestClientSpotApiSubAccount SubAccount { get; }

        /// <inheritdoc />
        public IGeminiRestClientSpotApiExchangeData ExchangeData { get; }

        /// <inheritdoc />
        public IGeminiRestClientSpotApiTrading Trading { get; }

        /// <inheritdoc />
        public IGeminiRestClientSpotApiHfTrading HfTrading { get; }

        /// <inheritdoc />
        public IGeminiRestClientSpotApiMargin Margin { get; }

        internal GeminiRestClientSpotApi(ILogger logger, HttpClient? httpClient, GeminiRestClient baseClient, GeminiRestOptions options)
            : base(logger, httpClient, options.Environment.SpotAddress, options, options.SpotOptions)
        {
            Account = new GeminiRestClientSpotApiAccount(this);
            SubAccount = new GeminiRestClientSpotApiSubAccount(this);
            ExchangeData = new GeminiRestClientSpotApiExchangeData(this);
            Trading = new GeminiRestClientSpotApiTrading(this);
            HfTrading = new GeminiRestClientSpotApiHfTrading(this);
            Margin = new GeminiRestClientSpotApiMargin(this);

            ParameterPositions[HttpMethod.Delete] = HttpMethodParameterPosition.InUri;
        }

        /// <inheritdoc />
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new GeminiAuthenticationProvider((GeminiApiCredentials)credentials);

        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverTime = null)
            => GeminiExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverTime);

        #region common interface

        /// <summary>
        /// Return the Gemini trade symbol name from base and quote asset 
        /// </summary>
        /// <param name="baseAsset"></param>
        /// <param name="quoteAsset"></param>
        /// <returns></returns>
        public string GetSymbolName(string baseAsset, string quoteAsset) => (baseAsset + "-" + quoteAsset).ToUpperInvariant();

        async Task<WebCallResult<IEnumerable<Symbol>>> IBaseRestClient.GetSymbolsAsync(CancellationToken ct)
        {
            var symbols = await ExchangeData.GetSymbolsAsync(ct: ct).ConfigureAwait(false);
            if (!symbols)
                return symbols.As<IEnumerable<Symbol>>(null);

            return symbols.As(symbols.Data.Select(d => new Symbol
            {
                SourceObject = d,
                Name = d.Symbol,
                MinTradeQuantity = d.BaseMinQuantity,
                PriceStep = d.PriceIncrement,
                QuantityStep = d.BaseIncrement
            }));
        }

        async Task<WebCallResult<Ticker>> IBaseRestClient.GetTickerAsync(string symbol, CancellationToken ct)
        {
            var symbols = await ExchangeData.GetTickersAsync(ct: ct).ConfigureAwait(false);
            if (!symbols)
                return symbols.As<Ticker>(null);

            var ticker = symbols.Data.Data.SingleOrDefault(s => s.Symbol == symbol);
            if (ticker == null)
                return symbols.AsError<Ticker>(new ArgumentError("Symbol not found"));

            return symbols.As(new Ticker
            {
                SourceObject = ticker,
                HighPrice = ticker.HighPrice,
                LastPrice = ticker.LastPrice,
                LowPrice = ticker.LowPrice,
                Price24H = ticker.LastPrice - ticker.ChangePrice,
                Symbol = ticker.Symbol,
                Volume = ticker.Volume
            });
        }

        async Task<WebCallResult<IEnumerable<Ticker>>> IBaseRestClient.GetTickersAsync(CancellationToken ct)
        {
            var symbols = await ExchangeData.GetTickersAsync(ct: ct).ConfigureAwait(false);
            if (!symbols)
                return symbols.As<IEnumerable<Ticker>>(null);

            return symbols.As(symbols.Data.Data.Select(t => new Ticker
            {
                SourceObject = t,
                HighPrice = t.HighPrice,
                LastPrice = t.LastPrice,
                LowPrice = t.LowPrice,
                Price24H = t.LastPrice - t.ChangePrice,
                Symbol = t.Symbol,
                Volume = t.Volume
            }));
        }

        async Task<WebCallResult<IEnumerable<Kline>>> IBaseRestClient.GetKlinesAsync(string symbol, TimeSpan timespan, DateTime? startTime, DateTime? endTime, int? limit, CancellationToken ct)
        {
            if (limit != null)
                throw new ArgumentException($"Gemini doesn't support the {nameof(limit)} parameter for the method {nameof(IBaseRestClient.GetKlinesAsync)}", nameof(limit));

            var symbols = await ExchangeData.GetKlinesAsync(symbol, GetKlineIntervalFromTimespan(timespan), startTime, endTime, ct: ct).ConfigureAwait(false);
            if (!symbols)
                return symbols.As<IEnumerable<Kline>>(null);

            return symbols.As(symbols.Data.Select(k => new Kline
            {
                SourceObject = k,
                ClosePrice = k.ClosePrice,
                HighPrice = k.HighPrice,
                LowPrice = k.LowPrice,
                OpenPrice = k.OpenPrice,
                OpenTime = k.OpenTime,
                Volume = k.Volume
            }));
        }

        async Task<WebCallResult<OrderBook>> IBaseRestClient.GetOrderBookAsync(string symbol, CancellationToken ct)
        {
            var book = await ExchangeData.GetAggregatedFullOrderBookAsync(symbol, ct: ct).ConfigureAwait(false);
            if (!book)
                return book.As<OrderBook>(null);

            return book.As(new OrderBook
            {
                SourceObject = book.Data,
                Asks = book.Data.Asks.Select(a => new OrderBookEntry { Price = a.Price, Quantity = a.Quantity }),
                Bids = book.Data.Bids.Select(b => new OrderBookEntry { Price = b.Price, Quantity = b.Quantity })
            });
        }

        async Task<WebCallResult<IEnumerable<Trade>>> IBaseRestClient.GetRecentTradesAsync(string symbol, CancellationToken ct)
        {
            var trades = await ExchangeData.GetTradeHistoryAsync(symbol, ct: ct).ConfigureAwait(false);
            if (!trades)
                return trades.As<IEnumerable<Trade>>(null);

            return trades.As(trades.Data.Select(t => new Trade
            {
                SourceObject = t,
                Price = t.Price,
                Quantity = t.Quantity,
                Symbol = symbol,
                Timestamp = t.Timestamp
            }));
        }

        async Task<WebCallResult<OrderId>> ISpotClient.PlaceOrderAsync(string symbol, CommonOrderSide side, CommonOrderType type, decimal quantity, decimal? price, string? accountId, string? clientOrderId, CancellationToken ct)
        {
            var order = await Trading.PlaceOrderAsync(symbol,
                side == CommonOrderSide.Sell ? OrderSide.Sell : OrderSide.Buy,
                type == CommonOrderType.Limit ? NewOrderType.Limit : NewOrderType.Market,
                quantity,
                price,
                clientOrderId: clientOrderId,
                ct: ct).ConfigureAwait(false);
            if (!order)
                return order.As<OrderId>(null);

            return order.As(new OrderId
            {
                SourceObject = order.Data,
                Id = order.Data.Id
            });
        }

        async Task<WebCallResult<Order>> IBaseRestClient.GetOrderAsync(string orderId, string? symbol, CancellationToken ct)
        {
            var order = await Trading.GetOrderAsync(orderId, ct: ct).ConfigureAwait(false);
            if (!order)
                return order.As<Order>(null);

            return order.As(new Order
            {
                SourceObject = order.Data,
                Id = order.Data.Id,
                Price = order.Data.Price,
                Quantity = order.Data.Quantity,
                QuantityFilled = order.Data.QuantityFilled,
                Timestamp = order.Data.CreateTime,
                Symbol = order.Data.Symbol,
                Side = order.Data.Side == OrderSide.Buy ? CommonOrderSide.Buy : CommonOrderSide.Sell,
                Type = order.Data.Type == OrderType.Market ? CommonOrderType.Market : order.Data.Type == OrderType.Limit ? CommonOrderType.Limit : CommonOrderType.Other,
                Status = order.Data.IsActive == true ? CommonOrderStatus.Active : order.Data.CancelExist ? CommonOrderStatus.Canceled : CommonOrderStatus.Filled
            });
        }

        async Task<WebCallResult<IEnumerable<UserTrade>>> IBaseRestClient.GetOrderTradesAsync(string orderId, string? symbol, CancellationToken ct)
        {
            var trades = await Trading.GetUserTradesAsync(orderId: orderId, ct: ct).ConfigureAwait(false);
            if (!trades)
                return trades.As<IEnumerable<UserTrade>>(null);

            return trades.As(trades.Data.Items.Select(t => new UserTrade
            {
                SourceObject = t,
                Fee = t.Fee,
                FeeAsset = t.FeeAsset,
                Id = t.Id,
                OrderId = t.OrderId,
                Price = t.Price,
                Quantity = t.Quantity,
                Symbol = t.Symbol,
                Timestamp = t.Timestamp
            }));
        }

        async Task<WebCallResult<IEnumerable<Order>>> IBaseRestClient.GetOpenOrdersAsync(string? symbol, CancellationToken ct)
        {
            var orders = await Trading.GetOrdersAsync(status: Enums.OrderStatus.Active, ct: ct).ConfigureAwait(false);
            if (!orders)
                return orders.As<IEnumerable<Order>>(null);

            return orders.As(orders.Data.Items.Select(d => new Order
            {
                SourceObject = d,
                Id = d.Id,
                Price = d.Price,
                Quantity = d.Quantity,
                QuantityFilled = d.QuantityFilled,
                Timestamp = d.CreateTime,
                Symbol = d.Symbol,
                Side = d.Side == OrderSide.Buy ? CommonOrderSide.Buy : CommonOrderSide.Sell,
                Type = d.Type == OrderType.Market ? CommonOrderType.Market : d.Type == OrderType.Limit ? CommonOrderType.Limit : CommonOrderType.Other,
                Status = d.IsActive == true ? CommonOrderStatus.Active : d.CancelExist ? CommonOrderStatus.Canceled : CommonOrderStatus.Filled
            }));
        }

        async Task<WebCallResult<IEnumerable<Order>>> IBaseRestClient.GetClosedOrdersAsync(string? symbol, CancellationToken ct)
        {
            var orders = await Trading.GetOrdersAsync(status: Enums.OrderStatus.Done, ct: ct).ConfigureAwait(false);
            if (!orders)
                return orders.As<IEnumerable<Order>>(null);

            return orders.As(orders.Data.Items.Select(d => new Order
            {
                SourceObject = d,
                Id = d.Id,
                Price = d.Price,
                Quantity = d.Quantity,
                QuantityFilled = d.QuantityFilled,
                Timestamp = d.CreateTime,
                Symbol = d.Symbol,
                Side = d.Side == OrderSide.Buy ? CommonOrderSide.Buy : CommonOrderSide.Sell,
                Type = d.Type == OrderType.Market ? CommonOrderType.Market : d.Type == OrderType.Limit ? CommonOrderType.Limit : CommonOrderType.Other,
                Status = d.IsActive == true ? CommonOrderStatus.Active : d.CancelExist ? CommonOrderStatus.Canceled : CommonOrderStatus.Filled
            }));
        }

        async Task<WebCallResult<OrderId>> IBaseRestClient.CancelOrderAsync(string orderId, string? symbol, CancellationToken ct)
        {
            var result = await Trading.CancelOrderAsync(orderId, ct: ct).ConfigureAwait(false);
            if (!result)
                return result.As<OrderId>(null);

            if (!result.Data.CancelledOrderIds.Any())
                return result.AsError<OrderId>(new ServerError("Order not canceled"));

            return result.As(new OrderId
            {
                SourceObject = result.Data,
                Id = result.Data.CancelledOrderIds.First()
            });
        }

        async Task<WebCallResult<IEnumerable<Balance>>> IBaseRestClient.GetBalancesAsync(string? accountId, CancellationToken ct)
        {
            var result = await Account.GetAccountsAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return result.As<IEnumerable<Balance>>(null);

            return result.As(result.Data.Select(b => new Balance
            {
                SourceObject = b,
                Asset = b.Asset,
                Available = b.Available,
                Total = b.Total
            }));
        }

        private static KlineInterval GetKlineIntervalFromTimespan(TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.FromMinutes(1)) return KlineInterval.OneMinute;
            if (timeSpan == TimeSpan.FromMinutes(5)) return KlineInterval.FiveMinutes;
            if (timeSpan == TimeSpan.FromMinutes(15)) return KlineInterval.FifteenMinutes;
            if (timeSpan == TimeSpan.FromMinutes(30)) return KlineInterval.ThirtyMinutes;
            if (timeSpan == TimeSpan.FromHours(1)) return KlineInterval.OneHour;
            if (timeSpan == TimeSpan.FromHours(2)) return KlineInterval.TwoHours;
            if (timeSpan == TimeSpan.FromHours(4)) return KlineInterval.FourHours;
            if (timeSpan == TimeSpan.FromHours(6)) return KlineInterval.SixHours;
            if (timeSpan == TimeSpan.FromHours(8)) return KlineInterval.EightHours;
            if (timeSpan == TimeSpan.FromHours(12)) return KlineInterval.TwelveHours;
            if (timeSpan == TimeSpan.FromDays(1)) return KlineInterval.OneDay;
            if (timeSpan == TimeSpan.FromDays(7)) return KlineInterval.OneWeek;

            throw new ArgumentException("Unsupported timespan for Gemini kline interval, check supported intervals using Gemini.Net.Objects.GeminiKlineInterval");
        }
        #endregion

        internal void InvokeOrderPlaced(OrderId id)
        {
            OnOrderPlaced?.Invoke(id);
        }

        internal void InvokeOrderCanceled(OrderId id)
        {
            OnOrderCanceled?.Invoke(id);
        }

        internal async Task<WebCallResult> SendAsync(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
        {
            var result = await base.SendAsync<GeminiResult>(BaseAddress, definition, parameters, cancellationToken, null, weight).ConfigureAwait(false);
            if (!result)
                return result.AsDatalessError(result.Error!);

            if (result.Data.Code != 200000 && result.Data.Code != 200)
                return result.AsDatalessError(new ServerError(result.Data.Code, result.Data.Message ?? "-"));

            return result.AsDataless();
        }

        internal async Task<WebCallResult<T>> SendAsync<T>(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
        {
            var result = await base.SendAsync<GeminiResult<T>>(BaseAddress, definition, parameters, cancellationToken, null, weight).ConfigureAwait(false);
            if (!result)
                return result.AsError<T>(result.Error!);

            if (result.Data.Code != 200000 && result.Data.Code != 200)
                return result.AsError<T>(new ServerError(result.Data.Code, result.Data.Message ?? "-"));

            return result.As(result.Data.Data);
        }

        internal async Task<WebCallResult<T>> SendRawAsync<T>(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null) where T : class
        {
            var result = await base.SendAsync<T>(BaseAddress, definition, parameters, cancellationToken, null, weight).ConfigureAwait(false);
            if (!result)
                return result.AsError<T>(result.Error!);

            return result.As(result.Data);
        }

        /// <inheritdoc />
        protected override ServerRateLimitError ParseRateLimitResponse(int httpStatusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders, IMessageAccessor accessor)
        {
            var retryAfterHeader = responseHeaders.SingleOrDefault(r => r.Key.Equals("gw-ratelimit-reset", StringComparison.InvariantCultureIgnoreCase));
            if (retryAfterHeader.Value?.Any() != true)
                return base.ParseRateLimitResponse(httpStatusCode, responseHeaders, accessor);

            var value = retryAfterHeader.Value.First();
            if (!int.TryParse(value, out var milliseconds))
                return base.ParseRateLimitResponse(httpStatusCode, responseHeaders, accessor);

            var msg = accessor.GetValue<string>(MessagePath.Get().Property("msg"));
            return new ServerRateLimitError(msg!)
            {
                RetryAfter = DateTime.UtcNow.AddMilliseconds(milliseconds)
            };
        }

        /// <inheritdoc />
        protected override Error ParseErrorResponse(int httpStatusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders, IMessageAccessor accessor)
        {
            if (!accessor.IsJson)
                return new ServerError(accessor.GetOriginalString());

            var code = accessor.GetValue<int?>(MessagePath.Get().Property("code"));
            var msg = accessor.GetValue<string>(MessagePath.Get().Property("msg"));
            if (msg == null)
                return new ServerError(accessor.GetOriginalString());

            if (code == null)
                return new ServerError(msg);

            return new ServerError(code.Value, msg);
        }

        /// <inheritdoc />
        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync()
            => ExchangeData.GetServerTimeAsync();

        /// <inheritdoc />
        public override TimeSyncInfo? GetTimeSyncInfo()
            => new TimeSyncInfo(_logger, (ApiOptions.AutoTimestamp ?? ClientOptions.AutoTimestamp), (ApiOptions.TimestampRecalculationInterval ?? ClientOptions.TimestampRecalculationInterval), _timeSyncState);

        /// <inheritdoc />
        public override TimeSpan? GetTimeOffset()
            => _timeSyncState.TimeOffset;

        /// <inheritdoc />
        public ISpotClient CommonSpotClient => this;
        public IGeminiRestClientSpotApiShared SharedClient => this;

    }
}
