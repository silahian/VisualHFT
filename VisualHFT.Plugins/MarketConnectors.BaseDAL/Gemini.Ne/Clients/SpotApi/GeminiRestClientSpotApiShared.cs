using Gemini.Net.Interfaces.Clients.SpotApi;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gemini.Net.Enums;

namespace Gemini.Net.Clients.SpotApi
{
    internal partial class GeminiRestClientSpotApi : IGeminiRestClientSpotApiShared
    {
        public string Exchange => GeminiExchange.ExchangeName;
        public TradingMode[] SupportedTradingModes { get; } = new[] { TradingMode.Spot };

        public void SetDefaultExchangeParameter(string key, object value) => ExchangeParameters.SetStaticParameter(Exchange, key, value);
        public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();

        #region Kline client

        GetKlinesOptions IKlineRestClient.GetKlinesOptions { get; } = new GetKlinesOptions(SharedPaginationSupport.Descending, false)
        {
            MaxRequestDataPoints = 1500
        };

        async Task<ExchangeWebResult<IEnumerable<SharedKline>>> IKlineRestClient.GetKlinesAsync(GetKlinesRequest request, INextPageToken? pageToken, CancellationToken ct)
        {
            var interval = (Enums.KlineInterval)request.Interval;
            if (!Enum.IsDefined(typeof(Enums.KlineInterval), interval))
                return new ExchangeWebResult<IEnumerable<SharedKline>>(Exchange, new ArgumentError("Interval not supported"));

            var validationError = ((IKlineRestClient)this).GetKlinesOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedKline>>(Exchange, validationError);

            // Determine pagination
            // Data is normally returned oldest first, so to do newest first pagination we have to do some calc
            DateTime endTime = request.EndTime ?? DateTime.UtcNow;
            DateTime? startTime = request.StartTime;
            if (pageToken is DateTimeToken dateTimeToken)
                endTime = dateTimeToken.LastTime;

            var limit = request.Limit ?? 1500;
            if (startTime == null || startTime < endTime)
            {
                var offset = (int)interval * limit;
                startTime = endTime.AddSeconds(-offset);
            }

            if (startTime < request.StartTime)
                startTime = request.StartTime;

            var result = await ExchangeData.GetKlinesAsync(
                request.Symbol.GetSymbol(FormatSymbol),
                interval,
                startTime,
                endTime,
                ct: ct
                ).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<IEnumerable<SharedKline>>(Exchange, null, default);

            // Get next token
            DateTimeToken? nextToken = null;
            if (result.Data.Count() == limit)
            {
                var minOpenTime = result.Data.Min(x => x.OpenTime);
                if (request.StartTime == null || minOpenTime > request.StartTime.Value)
                    nextToken = new DateTimeToken(minOpenTime.AddSeconds(-(int)(interval - 1)));
            }

            return result.AsExchangeResult<IEnumerable<SharedKline>>(Exchange, request.Symbol.TradingMode, result.Data.Select(x => new SharedKline(x.OpenTime, x.ClosePrice, x.HighPrice, x.LowPrice, x.OpenPrice, x.Volume)).ToArray(), nextToken);
        }

        #endregion

        #region Spot Symbol client

        EndpointOptions<GetSymbolsRequest> ISpotSymbolRestClient.GetSpotSymbolsOptions { get; } = new EndpointOptions<GetSymbolsRequest>(false);
        async Task<ExchangeWebResult<IEnumerable<SharedSpotSymbol>>> ISpotSymbolRestClient.GetSpotSymbolsAsync(GetSymbolsRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotSymbolRestClient)this).GetSpotSymbolsOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedSpotSymbol>>(Exchange, validationError);

            var result = await ExchangeData.GetSymbolsAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<IEnumerable<SharedSpotSymbol>>(Exchange, null, default);

            return result.AsExchangeResult<IEnumerable<SharedSpotSymbol>>(Exchange, TradingMode.Spot, result.Data.Select(s => new SharedSpotSymbol(s.BaseAsset, s.QuoteAsset, s.Symbol, s.EnableTrading)
            {
                MinTradeQuantity = s.BaseMinQuantity,
                MaxTradeQuantity = s.BaseMaxQuantity,
                QuantityStep = s.BaseIncrement,
                PriceStep = s.PriceIncrement,
                MinNotionalValue = s.MinFunds
            }).ToArray());
        }

        #endregion

        #region Ticker client

        EndpointOptions<GetTickerRequest> ISpotTickerRestClient.GetSpotTickerOptions { get; } = new EndpointOptions<GetTickerRequest>(false);
        async Task<ExchangeWebResult<SharedSpotTicker>> ISpotTickerRestClient.GetSpotTickerAsync(GetTickerRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotTickerRestClient)this).GetSpotTickerOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedSpotTicker>(Exchange, validationError);

            var symbol = request.Symbol.GetSymbol(FormatSymbol);
            var result = await ExchangeData.Get24HourStatsAsync(symbol, ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<SharedSpotTicker>(Exchange, null, default);

            return result.AsExchangeResult(Exchange, TradingMode.Spot, new SharedSpotTicker(symbol, result.Data.LastPrice ?? 0, result.Data.HighPrice ?? 0, result.Data.LowPrice ?? 0, result.Data.Volume ?? 0, result.Data.ChangePercentage * 100));
        }

        EndpointOptions<GetTickersRequest> ISpotTickerRestClient.GetSpotTickersOptions { get; } = new EndpointOptions<GetTickersRequest>(false);
        async Task<ExchangeWebResult<IEnumerable<SharedSpotTicker>>> ISpotTickerRestClient.GetSpotTickersAsync(GetTickersRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotTickerRestClient)this).GetSpotTickersOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedSpotTicker>>(Exchange, validationError);

            var result = await ExchangeData.GetTickersAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<IEnumerable<SharedSpotTicker>>(Exchange, null, default);

            return result.AsExchangeResult<IEnumerable<SharedSpotTicker>>(Exchange, TradingMode.Spot, result.Data.Data.Select(x => new SharedSpotTicker(x.Symbol, x.LastPrice ?? 0, x.HighPrice ?? 0, x.LowPrice ?? 0, x.Volume ?? 0, x.ChangePercentage * 100)).ToArray());
        }

        #endregion

        #region Recent Trade client
        GetRecentTradesOptions IRecentTradeRestClient.GetRecentTradesOptions { get; } = new GetRecentTradesOptions(100, false);

        async Task<ExchangeWebResult<IEnumerable<SharedTrade>>> IRecentTradeRestClient.GetRecentTradesAsync(GetRecentTradesRequest request, CancellationToken ct)
        {
            var validationError = ((IRecentTradeRestClient)this).GetRecentTradesOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedTrade>>(Exchange, validationError);

            var result = await ExchangeData.GetTradeHistoryAsync(
                request.Symbol.GetSymbol(FormatSymbol),
                ct: ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<IEnumerable<SharedTrade>>(Exchange, null, default);

            return result.AsExchangeResult<IEnumerable<SharedTrade>>(Exchange, request.Symbol.TradingMode, result.Data.Select(x => new SharedTrade(x.Quantity, x.Price, x.Timestamp)
            {
                Side = x.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell
            }).ToArray());
        }

        #endregion

        #region Balance client
        EndpointOptions<GetBalancesRequest> IBalanceRestClient.GetBalancesOptions { get; } = new EndpointOptions<GetBalancesRequest>(true);

        async Task<ExchangeWebResult<IEnumerable<SharedBalance>>> IBalanceRestClient.GetBalancesAsync(GetBalancesRequest request, CancellationToken ct)
        {
            var validationError = ((IBalanceRestClient)this).GetBalancesOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedBalance>>(Exchange, validationError);

            var result = await Account.GetAccountsAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<IEnumerable<SharedBalance>>(Exchange, null, default);

            var hfAccount = ExchangeParameters.GetValue<bool?>(request.ExchangeParameters, Exchange, "HfTrading");
            var data = result.Data;
            if (hfAccount == false)
                data = result.Data.Where(x => x.Type == AccountType.Trade);
            else
                data = result.Data.Where(x => x.Type == AccountType.SpotHf);

            return result.AsExchangeResult<IEnumerable<SharedBalance>>(Exchange, TradingMode.Spot, data.Select(x => new SharedBalance(x.Asset, x.Available, x.Available + x.Holds)).ToArray());
        }

        #endregion

        #region Spot Order client


        SharedFeeDeductionType ISpotOrderRestClient.SpotFeeDeductionType => SharedFeeDeductionType.DeductFromOutput;
        SharedFeeAssetType ISpotOrderRestClient.SpotFeeAssetType => SharedFeeAssetType.QuoteAsset;
        IEnumerable<SharedOrderType> ISpotOrderRestClient.SpotSupportedOrderTypes { get; } = new[] { SharedOrderType.Limit, SharedOrderType.Market, SharedOrderType.LimitMaker };
        IEnumerable<SharedTimeInForce> ISpotOrderRestClient.SpotSupportedTimeInForce { get; } = new[] { SharedTimeInForce.GoodTillCanceled, SharedTimeInForce.ImmediateOrCancel, SharedTimeInForce.FillOrKill };

        SharedQuantitySupport ISpotOrderRestClient.SpotSupportedOrderQuantity { get; } = new SharedQuantitySupport(
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAndQuoteAsset,
                SharedQuantityType.BaseAndQuoteAsset);

        PlaceSpotOrderOptions ISpotOrderRestClient.PlaceSpotOrderOptions { get; } = new PlaceSpotOrderOptions();
        async Task<ExchangeWebResult<SharedId>> ISpotOrderRestClient.PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).PlaceSpotOrderOptions.ValidateRequest(
                Exchange,
                request,
                request.Symbol.TradingMode,
                SupportedTradingModes,
                ((ISpotOrderRestClient)this).SpotSupportedOrderTypes,
                ((ISpotOrderRestClient)this).SpotSupportedTimeInForce,
                ((ISpotOrderRestClient)this).SpotSupportedOrderQuantity);
            if (validationError != null)
                return new ExchangeWebResult<SharedId>(Exchange, validationError);

            var hfAccount = ExchangeParameters.GetValue<bool?>(request.ExchangeParameters, Exchange, "HfTrading");
            if (hfAccount == false)
            {
                var result = await Trading.PlaceOrderAsync(
                    request.Symbol.GetSymbol(FormatSymbol),
                    request.Side == SharedOrderSide.Buy ? Enums.OrderSide.Buy : Enums.OrderSide.Sell,
                    GetPlaceOrderType(request.OrderType),
                    request.Quantity,
                    request.Price,
                    request.QuoteQuantity,
                    timeInForce: GetTimeInForce(request.TimeInForce),
                    postOnly: request.OrderType == SharedOrderType.LimitMaker ? true : null,
                    clientOrderId: request.ClientOrderId).ConfigureAwait(false);

                if (!result)
                    return result.AsExchangeResult<SharedId>(Exchange, null, default);

                return result.AsExchangeResult(Exchange, request.Symbol.TradingMode, new SharedId(result.Data.Id.ToString()));
            }
            else
            {
                var result = await HfTrading.PlaceOrderAsync(
                    request.Symbol.GetSymbol(FormatSymbol),
                    request.Side == SharedOrderSide.Buy ? Enums.OrderSide.Buy : Enums.OrderSide.Sell,
                    GetPlaceOrderType(request.OrderType),
                    request.Quantity,
                    request.Price,
                    request.QuoteQuantity,
                    timeInForce: GetTimeInForce(request.TimeInForce),
                    postOnly: request.OrderType == SharedOrderType.LimitMaker ? true : null,
                    clientOrderId: request.ClientOrderId).ConfigureAwait(false);

                if (!result)
                    return result.AsExchangeResult<SharedId>(Exchange, null, default);

                return result.AsExchangeResult(Exchange, request.Symbol.TradingMode, new SharedId(result.Data.Id.ToString()));
            }
        }

        EndpointOptions<GetOrderRequest> ISpotOrderRestClient.GetSpotOrderOptions { get; } = new EndpointOptions<GetOrderRequest>(true);
        async Task<ExchangeWebResult<SharedSpotOrder>> ISpotOrderRestClient.GetSpotOrderAsync(GetOrderRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).GetSpotOrderOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedSpotOrder>(Exchange, validationError);

            var hfAccount = ExchangeParameters.GetValue<bool?>(request.ExchangeParameters, Exchange, "HfTrading");
            if (hfAccount == false)
            {
                var order = await Trading.GetOrderAsync(request.OrderId).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<SharedSpotOrder>(Exchange, null, default);

                return order.AsExchangeResult(Exchange, TradingMode.Spot, new SharedSpotOrder(
                    order.Data.Symbol,
                    order.Data.Id.ToString(),
                    ParseOrderType(order.Data.Type, order.Data.PostOnly),
                    order.Data.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                    ParseOrderStatus(order.Data.IsActive ?? true, order.Data.CancelExist),
                    order.Data.CreateTime)
                {
                    ClientOrderId = order.Data.ClientOrderId,
                    Fee = order.Data.Fee,
                    OrderPrice = order.Data.Price,
                    Quantity = order.Data.Quantity,
                    QuantityFilled = order.Data.QuantityFilled,
                    QuoteQuantity = order.Data.QuoteQuantity,
                    QuoteQuantityFilled = order.Data.QuoteQuantityFilled,
                    TimeInForce = ParseTimeInForce(order.Data.TimeInForce),
                    FeeAsset = order.Data.FeeAsset
                });
            }
            else
            {
                var order = await HfTrading.GetOrderAsync(request.Symbol.GetSymbol(FormatSymbol), request.OrderId).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<SharedSpotOrder>(Exchange, null, default);

                return order.AsExchangeResult(Exchange, TradingMode.Spot, new SharedSpotOrder(
                    order.Data.Symbol,
                    order.Data.Id.ToString(),
                    ParseOrderType(order.Data.Type, order.Data.PostOnly),
                    order.Data.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                    ParseOrderStatus(order.Data.IsActive ?? true, order.Data.CancelExist),
                    order.Data.CreateTime)
                {
                    ClientOrderId = order.Data.ClientOrderId,
                    Fee = order.Data.Fee,
                    OrderPrice = order.Data.Price,
                    Quantity = order.Data.Quantity,
                    QuantityFilled = order.Data.QuantityFilled,
                    QuoteQuantity = order.Data.QuoteQuantity,
                    QuoteQuantityFilled = order.Data.QuoteQuantityFilled,
                    TimeInForce = ParseTimeInForce(order.Data.TimeInForce),
                    FeeAsset = order.Data.FeeAsset
                });
            }
        }

        EndpointOptions<GetOpenOrdersRequest> ISpotOrderRestClient.GetOpenSpotOrdersOptions { get; } = new EndpointOptions<GetOpenOrdersRequest>(true);
        async Task<ExchangeWebResult<IEnumerable<SharedSpotOrder>>> ISpotOrderRestClient.GetOpenSpotOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).GetOpenSpotOrdersOptions.ValidateRequest(Exchange, request, request.Symbol?.TradingMode ?? request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedSpotOrder>>(Exchange, validationError);

            var hfAccount = ExchangeParameters.GetValue<bool?>(request.ExchangeParameters, Exchange, "HfTrading");
            if (hfAccount == false)
            {
                var symbol = request.Symbol?.GetSymbol(FormatSymbol);
                var order = await Trading.GetOrdersAsync(symbol: symbol, status: OrderStatus.Active).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<IEnumerable<SharedSpotOrder>>(Exchange, null, default);

                return order.AsExchangeResult<IEnumerable<SharedSpotOrder>>(Exchange, TradingMode.Spot, order.Data.Items.Select(x => new SharedSpotOrder(
                    x.Symbol,
                    x.Id.ToString(),
                    ParseOrderType(x.Type, x.PostOnly),
                    x.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                    ParseOrderStatus(x.IsActive ?? true, x.CancelExist),
                    x.CreateTime)
                {
                    ClientOrderId = x.ClientOrderId,
                    Fee = x.Fee,
                    OrderPrice = x.Price,
                    Quantity = x.Quantity,
                    QuantityFilled = x.QuantityFilled,
                    QuoteQuantity = x.QuoteQuantity,
                    QuoteQuantityFilled = x.QuoteQuantityFilled,
                    TimeInForce = ParseTimeInForce(x.TimeInForce),
                    FeeAsset = x.FeeAsset
                }).ToArray());
            }
            else
            {
                if (request.Symbol == null)
                    return new ExchangeWebResult<IEnumerable<SharedSpotOrder>>(Exchange, new ArgumentError("Symbol parameter is required for HfTrading account"));

                var symbol = request.Symbol.GetSymbol(FormatSymbol);
                var order = await HfTrading.GetOpenOrdersAsync(symbol).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<IEnumerable<SharedSpotOrder>>(Exchange, null, default);

                return order.AsExchangeResult<IEnumerable<SharedSpotOrder>>(Exchange, TradingMode.Spot, order.Data.Select(x => new SharedSpotOrder(
                    x.Symbol,
                    x.Id.ToString(),
                    ParseOrderType(x.Type, x.PostOnly),
                    x.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                    ParseOrderStatus(x.IsActive ?? true, x.CancelExist),
                    x.CreateTime)
                {
                    ClientOrderId = x.ClientOrderId,
                    Fee = x.Fee,
                    OrderPrice = x.Price,
                    Quantity = x.Quantity,
                    QuantityFilled = x.QuantityFilled,
                    QuoteQuantity = x.QuoteQuantity,
                    QuoteQuantityFilled = x.QuoteQuantityFilled,
                    TimeInForce = ParseTimeInForce(x.TimeInForce),
                    FeeAsset = x.FeeAsset
                }).ToArray());
            }
        }

        PaginatedEndpointOptions<GetClosedOrdersRequest> ISpotOrderRestClient.GetClosedSpotOrdersOptions { get; } = new PaginatedEndpointOptions<GetClosedOrdersRequest>(SharedPaginationSupport.Descending, true);
        async Task<ExchangeWebResult<IEnumerable<SharedSpotOrder>>> ISpotOrderRestClient.GetClosedSpotOrdersAsync(GetClosedOrdersRequest request, INextPageToken? pageToken, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).GetClosedSpotOrdersOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedSpotOrder>>(Exchange, validationError);

            var hfAccount = ExchangeParameters.GetValue<bool?>(request.ExchangeParameters, Exchange, "HfTrading");
            if (hfAccount == false)
            {
                // Determine page token
                int page = 1;
                int pageSize = request.Limit ?? 500;
                if (pageToken is PageToken token)
                {
                    page = token.Page;
                    pageSize = token.PageSize;
                }

                // Get data
                var order = await Trading.GetOrdersAsync(
                    request.Symbol.GetSymbol(FormatSymbol), 
                    status: OrderStatus.Done,
                    startTime: request.StartTime,
                    endTime: request.EndTime,
                    currentPage: page,
                    pageSize: pageSize).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<IEnumerable<SharedSpotOrder>>(Exchange, null, default);

                // Get next token
                PageToken? nextToken = null;
                if (order.Data.Items.Any() && order.Data.TotalItems > (page * pageSize))
                    nextToken = new PageToken(page + 1, pageSize);

                return order.AsExchangeResult<IEnumerable<SharedSpotOrder>>(Exchange, TradingMode.Spot, order.Data.Items.Select(x => new SharedSpotOrder(
                    x.Symbol,
                    x.Id.ToString(),
                    ParseOrderType(x.Type, x.PostOnly),
                    x.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                    ParseOrderStatus(x.IsActive ?? true, x.CancelExist),
                    x.CreateTime)
                {
                    ClientOrderId = x.ClientOrderId,
                    Fee = x.Fee,
                    OrderPrice = x.Price,
                    Quantity = x.Quantity,
                    QuantityFilled = x.QuantityFilled,
                    QuoteQuantity = x.QuoteQuantity,
                    QuoteQuantityFilled = x.QuoteQuantityFilled,
                    TimeInForce = ParseTimeInForce(x.TimeInForce),
                    FeeAsset = x.FeeAsset
                }).ToArray(), nextToken);
            }
            else
            {
                // Determine page token
                long? lastId = null;
                if (pageToken is FromIdToken token)
                    lastId = long.Parse(token.FromToken);

                // Get data
                var order = await HfTrading.GetClosedOrdersAsync(
                    request.Symbol.GetSymbol(FormatSymbol),
                    startTime: request.StartTime,
                    endTime: request.EndTime,
                    limit: request.Limit,
                    lastId: lastId).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<IEnumerable<SharedSpotOrder>>(Exchange, null, default);

                // Get next token
                FromIdToken? nextToken = null;
                if (order.Data.LastId != 0)
                    nextToken = new FromIdToken(order.Data.LastId.ToString());

                return order.AsExchangeResult<IEnumerable<SharedSpotOrder>>(Exchange, TradingMode.Spot, order.Data.Items.Select(x => new SharedSpotOrder(
                    x.Symbol,
                    x.Id.ToString(),
                    ParseOrderType(x.Type, x.PostOnly),
                    x.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                    ParseOrderStatus(x.IsActive ?? true, x.CancelExist),
                    x.CreateTime)
                {
                    ClientOrderId = x.ClientOrderId,
                    Fee = x.Fee,
                    OrderPrice = x.Price,
                    Quantity = x.Quantity,
                    QuantityFilled = x.QuantityFilled,
                    QuoteQuantity = x.QuoteQuantity,
                    QuoteQuantityFilled = x.QuoteQuantityFilled,
                    TimeInForce = ParseTimeInForce(x.TimeInForce),
                    FeeAsset = x.FeeAsset
                }).ToArray(), nextToken);
            }
        }

        EndpointOptions<GetOrderTradesRequest> ISpotOrderRestClient.GetSpotOrderTradesOptions { get; } = new EndpointOptions<GetOrderTradesRequest>(true);
        async Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> ISpotOrderRestClient.GetSpotOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).GetSpotOrderTradesOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedUserTrade>>(Exchange, validationError);

            var hfAccount = ExchangeParameters.GetValue<bool?>(request.ExchangeParameters, Exchange, "HfTrading");
            if (hfAccount == false)
            {
                var order = await Trading.GetUserTradesAsync(orderId: request.OrderId).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<IEnumerable<SharedUserTrade>>(Exchange, null, default);

                return order.AsExchangeResult<IEnumerable<SharedUserTrade>>(Exchange, TradingMode.Spot, order.Data.Items.Select(x => new SharedUserTrade(
                    x.Symbol,
                    x.OrderId.ToString(),
                    x.Id.ToString(),
                    x.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                    x.Quantity,
                    x.Price,
                    x.Timestamp)
                {
                    Fee = x.Fee,
                    FeeAsset = x.FeeAsset,
                    Role = x.ForceTaker ? SharedRole.Taker : SharedRole.Taker
                }).ToArray());
            }
            else
            {
                var symbol = request.Symbol.GetSymbol(FormatSymbol);
                var order = await HfTrading.GetUserTradesAsync(symbol, orderId: request.OrderId).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<IEnumerable<SharedUserTrade>>(Exchange, null, default);

                return order.AsExchangeResult<IEnumerable<SharedUserTrade>>(Exchange, TradingMode.Spot, order.Data.Items.Select(x => new SharedUserTrade(
                    x.Symbol,
                    x.OrderId.ToString(),
                    x.Id.ToString(),
                    x.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                    x.Quantity,
                    x.Price,
                    x.Timestamp)
                {
                    Fee = x.Fee,
                    FeeAsset = x.FeeAsset,
                    Role = x.ForceTaker ? SharedRole.Taker : SharedRole.Taker
                }).ToArray());
            }
        }

        PaginatedEndpointOptions<GetUserTradesRequest> ISpotOrderRestClient.GetSpotUserTradesOptions { get; } = new PaginatedEndpointOptions<GetUserTradesRequest>(SharedPaginationSupport.Descending, true);
        async Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> ISpotOrderRestClient.GetSpotUserTradesAsync(GetUserTradesRequest request, INextPageToken? nextPageToken, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).GetSpotUserTradesOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedUserTrade>>(Exchange, validationError);

            var hfAccount = ExchangeParameters.GetValue<bool?>(request.ExchangeParameters, Exchange, "HfTrading");
            if (hfAccount == false)
            {
                // Determine page token
                int page = 1;
                int pageSize = request.Limit ?? 500;
                if (nextPageToken is PageToken pageToken)
                {
                    page = pageToken.Page;
                    pageSize = pageToken.PageSize;
                }

                // Get data
                var order = await Trading.GetUserTradesAsync(request.Symbol.GetSymbol(FormatSymbol),
                    startTime: request.StartTime,
                    endTime: request.EndTime,
                    currentPage: page,
                    pageSize: pageSize).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<IEnumerable<SharedUserTrade>>(Exchange, null, default);

                // Get next token
                PageToken? nextToken = null;
                if (order.Data.Items.Any() && order.Data.TotalItems > (page * pageSize))
                    nextToken = new PageToken(page + 1, pageSize);

                return order.AsExchangeResult<IEnumerable<SharedUserTrade>>(Exchange, TradingMode.Spot, order.Data.Items.Select(x => new SharedUserTrade(
                    x.Symbol,
                    x.OrderId.ToString(),
                    x.Id.ToString(),
                    x.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                    x.Quantity,
                    x.Price,
                    x.Timestamp)
                {
                    Fee = x.Fee,
                    FeeAsset = x.FeeAsset,
                    Role = x.ForceTaker ? SharedRole.Taker : SharedRole.Taker
                }).ToArray(),
                nextToken);
            }
            else
            {
                // Determine page token
                long? lastId = null;
                if (nextPageToken is FromIdToken token)
                    lastId = long.Parse(token.FromToken);

                // Get data
                var order = await HfTrading.GetUserTradesAsync(
                    request.Symbol.GetSymbol(FormatSymbol),
                    startTime: request.StartTime,
                    endTime: request.EndTime,
                    limit: request.Limit,
                    lastId: lastId).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<IEnumerable<SharedUserTrade>>(Exchange, null, default);

                // Get next token
                FromIdToken? nextToken = null;
                if (order.Data.LastId != 0)
                    nextToken = new FromIdToken(order.Data.LastId.ToString());

                return order.AsExchangeResult<IEnumerable<SharedUserTrade>>(Exchange, TradingMode.Spot, order.Data.Items.Select(x => new SharedUserTrade(
                    x.Symbol,
                    x.OrderId.ToString(),
                    x.Id.ToString(),
                    x.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                    x.Quantity,
                    x.Price,
                    x.Timestamp)
                {
                    Fee = x.Fee,
                    FeeAsset = x.FeeAsset,
                    Role = x.ForceTaker ? SharedRole.Taker : SharedRole.Taker
                }).ToArray(),
                nextToken);
            }
        }

        EndpointOptions<CancelOrderRequest> ISpotOrderRestClient.CancelSpotOrderOptions { get; } = new EndpointOptions<CancelOrderRequest>(true);
        async Task<ExchangeWebResult<SharedId>> ISpotOrderRestClient.CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).CancelSpotOrderOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedId>(Exchange, validationError);


            var hfAccount = ExchangeParameters.GetValue<bool?>(request.ExchangeParameters, Exchange, "HfTrading");
            if (hfAccount == false)
            {
                var order = await Trading.CancelOrderAsync(request.OrderId).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<SharedId>(Exchange, null, default);

                return order.AsExchangeResult(Exchange, request.Symbol.TradingMode, new SharedId(request.OrderId));
            }
            else
            {
                var order = await HfTrading.CancelOrderAsync(request.Symbol.GetSymbol(FormatSymbol), request.OrderId).ConfigureAwait(false);
                if (!order)
                    return order.AsExchangeResult<SharedId>(Exchange, null, default);

                return order.AsExchangeResult(Exchange, request.Symbol.TradingMode, new SharedId(request.OrderId));
            }
        }

        private SharedOrderStatus ParseOrderStatus(bool active, bool canceled)
        {
            if (canceled) return SharedOrderStatus.Canceled;
            if (active) return SharedOrderStatus.Open;
            return SharedOrderStatus.Filled;
        }

        private SharedOrderType ParseOrderType(OrderType type, bool? postOnly)
        {
            if (type == OrderType.Market) return SharedOrderType.Market;
            if (type == OrderType.Limit && postOnly == true) return SharedOrderType.LimitMaker;
            if (type == OrderType.Limit) return SharedOrderType.Limit;

            return SharedOrderType.Other;
        }

        private SharedTimeInForce? ParseTimeInForce(TimeInForce? tif)
        {
            if (tif == TimeInForce.ImmediateOrCancel) return SharedTimeInForce.ImmediateOrCancel;
            if (tif == TimeInForce.FillOrKill) return SharedTimeInForce.FillOrKill;
            if (tif == TimeInForce.GoodTillCanceled) return SharedTimeInForce.GoodTillCanceled;

            return null;
        }

        private NewOrderType GetPlaceOrderType(SharedOrderType type)
        {
            if (type == SharedOrderType.Market) return NewOrderType.Market;

            return NewOrderType.Limit;
        }

        private TimeInForce? GetTimeInForce(SharedTimeInForce? tif)
        {
            if (tif == SharedTimeInForce.ImmediateOrCancel) return TimeInForce.ImmediateOrCancel;
            if (tif == SharedTimeInForce.GoodTillCanceled) return TimeInForce.GoodTillCanceled;
            if (tif == SharedTimeInForce.FillOrKill) return TimeInForce.FillOrKill;

            return null;
        }

        #endregion

        #region Asset client
        EndpointOptions<GetAssetRequest> IAssetsRestClient.GetAssetOptions { get; } = new EndpointOptions<GetAssetRequest>(false);
        async Task<ExchangeWebResult<SharedAsset>> IAssetsRestClient.GetAssetAsync(GetAssetRequest request, CancellationToken ct)
        {
            var validationError = ((IAssetsRestClient)this).GetAssetOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedAsset>(Exchange, validationError);

            var assets = await ExchangeData.GetAssetAsync(request.Asset, ct: ct).ConfigureAwait(false);
            if (!assets)
                return assets.AsExchangeResult<SharedAsset>(Exchange, null, default);

            return assets.AsExchangeResult<SharedAsset>(Exchange, TradingMode.Spot, new SharedAsset(assets.Data.Asset)
            {
                FullName = assets.Data.Name,
                Networks = assets.Data.Networks?.Select(x => new SharedAssetNetwork(x.NetworkId)
                {
                    FullName = x.NetworkName,
                    MinConfirmations = x.Confirms,
                    DepositEnabled = x.IsDepositEnabled,
                    MinWithdrawQuantity = x.WithdrawalMinQuantity,
                    WithdrawEnabled = x.IsWithdrawEnabled,
                    WithdrawFee = x.WithdrawalMinFee
                }).ToArray()
            });
        }

        EndpointOptions<GetAssetsRequest> IAssetsRestClient.GetAssetsOptions { get; } = new EndpointOptions<GetAssetsRequest>(false);

        async Task<ExchangeWebResult<IEnumerable<SharedAsset>>> IAssetsRestClient.GetAssetsAsync(GetAssetsRequest request, CancellationToken ct)
        {
            var validationError = ((IAssetsRestClient)this).GetAssetsOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedAsset>>(Exchange, validationError);

            var assets = await ExchangeData.GetAssetsAsync(ct: ct).ConfigureAwait(false);
            if (!assets)
                return assets.AsExchangeResult<IEnumerable<SharedAsset>>(Exchange, null, default);

            return assets.AsExchangeResult<IEnumerable<SharedAsset>>(Exchange, TradingMode.Spot, assets.Data.Select(x => new SharedAsset(x.Asset)
            {
                FullName = x.Name,
                Networks = x.Networks?.Select(x => new SharedAssetNetwork(x.NetworkId)
                {
                    FullName = x.NetworkName,
                    MinConfirmations = x.Confirms,
                    DepositEnabled = x.IsDepositEnabled,
                    MinWithdrawQuantity = x.WithdrawalMinQuantity,
                    WithdrawEnabled = x.IsWithdrawEnabled,
                    WithdrawFee = x.WithdrawalMinFee
                }).ToArray()
            }).ToArray());
        }

        #endregion

        #region Deposit client

        EndpointOptions<GetDepositAddressesRequest> IDepositRestClient.GetDepositAddressesOptions { get; } = new EndpointOptions<GetDepositAddressesRequest>(true);
        async Task<ExchangeWebResult<IEnumerable<SharedDepositAddress>>> IDepositRestClient.GetDepositAddressesAsync(GetDepositAddressesRequest request, CancellationToken ct)
        {
            var validationError = ((IDepositRestClient)this).GetDepositAddressesOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedDepositAddress>>(Exchange, validationError);

            var depositAddresses = await Account.GetDepositAddressAsync(request.Asset, request.Network).ConfigureAwait(false);
            if (!depositAddresses)
                return depositAddresses.AsExchangeResult<IEnumerable<SharedDepositAddress>>(Exchange, null, default);

            return depositAddresses.AsExchangeResult<IEnumerable<SharedDepositAddress>>(Exchange, TradingMode.Spot, new[] { new SharedDepositAddress(request.Asset, depositAddresses.Data.Address)
            {
                TagOrMemo = depositAddresses.Data.Memo,
                Network = depositAddresses.Data.Network
            }
            });
        }

        GetDepositsOptions IDepositRestClient.GetDepositsOptions { get; } = new GetDepositsOptions(SharedPaginationSupport.Descending, true);
        async Task<ExchangeWebResult<IEnumerable<SharedDeposit>>> IDepositRestClient.GetDepositsAsync(GetDepositsRequest request, INextPageToken? pageToken, CancellationToken ct)
        {
            var validationError = ((IDepositRestClient)this).GetDepositsOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedDeposit>>(Exchange, validationError);

            // Determine page token
            int page = 1;
            int pageSize = request.Limit ?? 100;
            if (pageToken is PageToken pagToken) {
                page = pagToken.Page;
                pageSize = pagToken.PageSize;
            }

            // Get data
            var deposits = await Account.GetDepositsAsync(
                request.Asset,
                startTime: request.StartTime,
                endTime: request.EndTime,
                currentPage: page,
                pageSize: pageSize,
                ct: ct).ConfigureAwait(false);
            if (!deposits)
                return deposits.AsExchangeResult<IEnumerable<SharedDeposit>>(Exchange, null, default);

            // Determine next token
            PageToken? nextToken = null;
            if (deposits.Data.TotalPages > page)
                nextToken = new PageToken(page + 1, pageSize);

            return deposits.AsExchangeResult<IEnumerable<SharedDeposit>>(Exchange, TradingMode.Spot, deposits.Data.Items.Select(x => new SharedDeposit(x.Asset, x.Quantity, x.Status == DepositStatus.Success, x.CreateTime)
            {
                Network = x.Network,
                TransactionId = x.WalletTransactionId,
                Tag = x.Memo
            }).ToArray(), nextToken);
        }

        #endregion

        #region Order Book client
        GetOrderBookOptions IOrderBookRestClient.GetOrderBookOptions { get; } = new GetOrderBookOptions(new[] { 20, 100 }, false);
        async Task<ExchangeWebResult<SharedOrderBook>> IOrderBookRestClient.GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct)
        {
            var validationError = ((IOrderBookRestClient)this).GetOrderBookOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedOrderBook>(Exchange, validationError);

            var result = await ExchangeData.GetAggregatedPartialOrderBookAsync(
                request.Symbol.GetSymbol(FormatSymbol),
                limit: request.Limit ?? 20,
                ct: ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<SharedOrderBook>(Exchange, null, default);

            return result.AsExchangeResult(Exchange, request.Symbol.TradingMode, new SharedOrderBook(result.Data.Asks, result.Data.Bids));
        }

        #endregion

        #region Withdrawal client

        GetWithdrawalsOptions IWithdrawalRestClient.GetWithdrawalsOptions { get; } = new GetWithdrawalsOptions(SharedPaginationSupport.Descending, true);
        async Task<ExchangeWebResult<IEnumerable<SharedWithdrawal>>> IWithdrawalRestClient.GetWithdrawalsAsync(GetWithdrawalsRequest request, INextPageToken? pageToken, CancellationToken ct)
        {
            var validationError = ((IWithdrawalRestClient)this).GetWithdrawalsOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<IEnumerable<SharedWithdrawal>>(Exchange, validationError);

            // Determine page token
            int page = 1;
            int pageSize = request.Limit ?? 100;
            if (pageToken is PageToken pagToken)
            {
                page = pagToken.Page;
                pageSize = pagToken.PageSize;
            }

            // Get data
            var withdrawals = await Account.GetWithdrawalsAsync(
                request.Asset,
                startTime: request.StartTime,
                endTime: request.EndTime,
                currentPage: page,
                pageSize: pageSize,
                ct: ct).ConfigureAwait(false);
            if (!withdrawals)
                return withdrawals.AsExchangeResult<IEnumerable<SharedWithdrawal>>(Exchange, null, default);

            // Determine next token
            PageToken? nextToken = null;
            if (withdrawals.Data.TotalPages > page)
                nextToken = new PageToken(page + 1, pageSize);

            return withdrawals.AsExchangeResult<IEnumerable<SharedWithdrawal>>(Exchange, TradingMode.Spot, withdrawals.Data.Items.Select(x => new SharedWithdrawal(x.Asset, x.Address, x.Quantity, x.Status == WithdrawalStatus.Success, x.CreateTime)
            {
                Id = x.Id,
                Network = x.Network,
                Tag = x.Memo,
                TransactionId = x.WalletTransactionId,
                Fee = x.Fee
            }).ToArray(), nextToken);
        }

        #endregion

        #region Withdraw client

        WithdrawOptions IWithdrawRestClient.WithdrawOptions { get; } = new WithdrawOptions();

        async Task<ExchangeWebResult<SharedId>> IWithdrawRestClient.WithdrawAsync(WithdrawRequest request, CancellationToken ct)
        {
            var validationError = ((IWithdrawRestClient)this).WithdrawOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedId>(Exchange, validationError);

            // Get data
            var withdrawal = await Account.WithdrawAsync(
                WithdrawType.Address,
                request.Asset,
                request.Address,
                request.Quantity,
                chain: request.Network,
                memo: request.AddressTag,
                ct: ct).ConfigureAwait(false);
            if (!withdrawal)
                return withdrawal.AsExchangeResult<SharedId>(Exchange, null, default);

            return withdrawal.AsExchangeResult(Exchange, TradingMode.Spot, new SharedId(withdrawal.Data.WithdrawalId));
        }

        #endregion
    }
}
