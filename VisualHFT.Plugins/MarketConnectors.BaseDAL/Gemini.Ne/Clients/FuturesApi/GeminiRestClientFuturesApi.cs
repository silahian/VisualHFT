using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.CommonClients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using Gemini.Net.Enums;
using Gemini.Net.Interfaces.Clients.FuturesApi;
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

namespace Gemini.Net.Clients.FuturesApi
{
    /// <inheritdoc cref="IGeminiRestClientFuturesApi" />
    internal partial class GeminiRestClientFuturesApi : RestApiClient, IGeminiRestClientFuturesApi, IFuturesClient
    {
        private readonly GeminiRestClient _baseClient;
        private readonly GeminiRestOptions _options;

        internal static TimeSyncState TimeSyncState = new TimeSyncState("Futures Api");

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
        public IGeminiRestClientFuturesApiAccount Account { get; }

        /// <inheritdoc />
        public IGeminiRestClientFuturesApiExchangeData ExchangeData { get; }

        /// <inheritdoc />
        public IGeminiRestClientFuturesApiTrading Trading { get; }

        internal GeminiRestClientFuturesApi(ILogger logger, HttpClient? httpClient, GeminiRestClient baseClient, GeminiRestOptions options)
            : base(logger, httpClient, options.Environment.FuturesAddress, options, options.FuturesOptions)
        {
            _baseClient = baseClient;
            _options = options;

            Account = new GeminiRestClientFuturesApiAccount(this);
            ExchangeData = new GeminiRestClientFuturesApiExchangeData(this);
            Trading = new GeminiRestClientFuturesApiTrading(this);

            ParameterPositions[HttpMethod.Delete] = HttpMethodParameterPosition.InUri;
        }

        /// <inheritdoc />
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new GeminiAuthenticationProvider((GeminiApiCredentials)credentials);

        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverTime = null)
            => GeminiExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverTime);

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

        /// <inheritdoc />
        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync()
            => ExchangeData.GetServerTimeAsync();

        /// <inheritdoc />
        public override TimeSyncInfo? GetTimeSyncInfo()
            => new TimeSyncInfo(_logger, (ApiOptions.AutoTimestamp ?? ClientOptions.AutoTimestamp), (ApiOptions.TimestampRecalculationInterval ?? ClientOptions.TimestampRecalculationInterval), TimeSyncState);

        /// <inheritdoc />
        public override TimeSpan? GetTimeOffset()
            => TimeSyncState.TimeOffset;

        /// <summary>
        /// Return the Gemini trade symbol name from base and quote asset 
        /// </summary>
        /// <param name="baseAsset"></param>
        /// <param name="quoteAsset"></param>
        /// <returns></returns>
        public string GetSymbolName(string baseAsset, string quoteAsset) => (baseAsset + "-" + quoteAsset).ToUpperInvariant();
        
        async Task<WebCallResult<IEnumerable<Symbol>>> IBaseRestClient.GetSymbolsAsync(CancellationToken ct)
        {
            var symbols = await ExchangeData.GetOpenContractsAsync(ct: ct).ConfigureAwait(false);
            if (!symbols)
                return symbols.As<IEnumerable<Symbol>>(null);

            return symbols.As(symbols.Data.Select(d => new Symbol
            {
                SourceObject = d,
                Name = d.Symbol,
                MinTradeQuantity = d.LotSize,
                QuantityStep = d.LotSize,
                PriceStep = d.TickSize
            }));
        }

        async Task<WebCallResult<Ticker>> IBaseRestClient.GetTickerAsync(string symbol, CancellationToken ct)
        {
            var ticker = await ExchangeData.GetContractAsync(symbol, ct: ct).ConfigureAwait(false);
            if (!ticker)
                return ticker.As<Ticker>(null);

            return ticker.As(new Ticker
            {
                SourceObject = ticker,
                HighPrice = ticker.Data.HighPrice,
                LowPrice = ticker.Data.LowPrice,
                Symbol = ticker.Data.Symbol,
                Volume = ticker.Data.Volume24H
            });
        }

        async Task<WebCallResult<IEnumerable<Ticker>>> IBaseRestClient.GetTickersAsync(CancellationToken ct)
        {
            var symbols = await ExchangeData.GetOpenContractsAsync(ct: ct).ConfigureAwait(false);
            if (!symbols)
                return symbols.As<IEnumerable<Ticker>>(null);

            return symbols.As(symbols.Data.Select(t => new Ticker
            {
                SourceObject = t,
                HighPrice = t.HighPrice,
                LowPrice = t.LowPrice,                
                Symbol = t.Symbol,
                Volume = t.Volume24H
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
            var book = await ExchangeData.GetAggregatedPartialOrderBookAsync(symbol, 100, ct: ct).ConfigureAwait(false);
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

        async Task<WebCallResult<OrderId>> IFuturesClient.PlaceOrderAsync(string symbol, CommonOrderSide side, CommonOrderType type, decimal quantity, decimal? price, int? leverage, string? accountId, string? clientOrderId, CancellationToken ct)
        {
            if (!leverage.HasValue)
                throw new ArgumentException($"Gemini required the {nameof(leverage)} parameter for {nameof(IFuturesClient.PlaceOrderAsync)}");

            var order = await Trading.PlaceOrderAsync(symbol,
                side == CommonOrderSide.Sell ? OrderSide.Sell : OrderSide.Buy,
                type == CommonOrderType.Limit ? NewOrderType.Limit : NewOrderType.Market,
                leverage.Value,
                (int)quantity,
                price, 
                clientOrderId: clientOrderId,
                ct: ct
                ).ConfigureAwait(false);

            if(!order)
                return order.As<OrderId> (null);

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
            var orders = await Trading.GetOrdersAsync(status: OrderStatus.Active, ct: ct).ConfigureAwait(false);
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
            var orders = await Trading.GetOrdersAsync(status: OrderStatus.Done, ct: ct).ConfigureAwait(false);
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
            var result = await Account.GetAccountOverviewAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return result.As<IEnumerable<Balance>>(null);

            return result.As<IEnumerable<Balance>>(new List<Balance> { new Balance
            {
                Asset = result.Data.Asset,
                Available = result.Data.AvailableBalance,
                Total = result.Data.FrozenFunds + result.Data.AvailableBalance,
                SourceObject = result.Data
            } });
        }

        async Task<WebCallResult<IEnumerable<Position>>> IFuturesClient.GetPositionsAsync(CancellationToken ct)
        {
            var positions = await Account.GetPositionsAsync(ct: ct).ConfigureAwait(false);
            if (!positions)
                return positions.As<IEnumerable<Position>>(null);

            return positions.As(positions.Data.Select(p => new Position
            {
                SourceObject = p,
                Id = p.Id,
                AutoMargin = p.AutoDeposit,
                Leverage = p.RealLeverage,
                Quantity = p.CurrentQuantity,
                Symbol = p.Symbol,
                LiquidationPrice = p.LiquidationPrice,
                MaintananceMargin = p.MaintenanceMargin,
                PositionMargin = p.PositionMargin,
                UnrealizedPnl = p.UnrealizedPnl,
                RealizedPnl = p.RealizedPnl,
                MarkPrice = p.MarkPrice,
                Isolated = !p.CrossMode,
                EntryPrice = p.AverageEntryPrice
            }));
        }

        /// <inheritdoc />
        public IFuturesClient CommonFuturesClient => this;
        public IGeminiRestClientFuturesApiShared SharedClient => this;

        private static FuturesKlineInterval GetKlineIntervalFromTimespan(TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.FromMinutes(1)) return FuturesKlineInterval.OneMinute;
            if (timeSpan == TimeSpan.FromMinutes(5)) return FuturesKlineInterval.FiveMinutes;
            if (timeSpan == TimeSpan.FromMinutes(15)) return FuturesKlineInterval.FifteenMinutes;
            if (timeSpan == TimeSpan.FromMinutes(30)) return FuturesKlineInterval.ThirtyMinutes;
            if (timeSpan == TimeSpan.FromHours(1)) return FuturesKlineInterval.OneHour;
            if (timeSpan == TimeSpan.FromHours(2)) return FuturesKlineInterval.TwoHours;
            if (timeSpan == TimeSpan.FromHours(4)) return FuturesKlineInterval.FourHours;
            if (timeSpan == TimeSpan.FromHours(8)) return FuturesKlineInterval.EightHours;
            if (timeSpan == TimeSpan.FromHours(12)) return FuturesKlineInterval.TwelveHours;
            if (timeSpan == TimeSpan.FromDays(1)) return FuturesKlineInterval.OneDay;
            if (timeSpan == TimeSpan.FromDays(7)) return FuturesKlineInterval.OneWeek;

            throw new ArgumentException("Unsupported timespan for Gemini kline interval, check supported intervals using Gemini.Net.Objects.GeminiKlineInterval");
        }

        /// <inheritdoc />
        protected override void WriteParamBody(IRequest request, IDictionary<string, object> parameters, string contentType)
        {
            if (contentType == "application/json")
            {
                string data = parameters.Count == 1 && parameters.First().Key == "<BODY>"
                    ? CreateSerializer().Serialize(parameters.First().Value)
                    : CreateSerializer().Serialize(parameters);
                request.SetContent(data, contentType);
            }
            else if (contentType == "application/x-www-form-urlencoded")
            {
                string data2 = parameters.ToFormData();
                request.SetContent(data2, contentType);
            }
        }

        internal void InvokeOrderPlaced(OrderId id)
        {
            OnOrderPlaced?.Invoke(id);
        }

        internal void InvokeOrderCanceled(OrderId id)
        {
            OnOrderCanceled?.Invoke(id);
        }
    }
}
