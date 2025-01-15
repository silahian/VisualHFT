using Gemini.Net.Interfaces.Clients.SpotApi;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects.Sockets;
using Gemini.Net.Objects.Models.Spot.Socket;
using Gemini.Net.Enums;

namespace Gemini.Net.Clients.SpotApi
{
    internal partial class GeminiSocketClientSpotApi : IGeminiSocketClientSpotApiShared
    {
        public string Exchange => GeminiExchange.ExchangeName;
        public TradingMode[] SupportedTradingModes { get; } = new[] { TradingMode.Spot };

        public void SetDefaultExchangeParameter(string key, object value) => ExchangeParameters.SetStaticParameter(Exchange, key, value);
        public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();

        #region Ticker client
        EndpointOptions<SubscribeTickerRequest> ITickerSocketClient.SubscribeTickerOptions { get; } = new EndpointOptions<SubscribeTickerRequest>(false);
        async Task<ExchangeResult<UpdateSubscription>> ITickerSocketClient.SubscribeToTickerUpdatesAsync(SubscribeTickerRequest request, Action<ExchangeEvent<SharedSpotTicker>> handler, CancellationToken ct)
        {
            var validationError = ((ITickerSocketClient)this).SubscribeTickerOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);

            var symbol = request.Symbol.GetSymbol(FormatSymbol);
            var result = await SubscribeToSnapshotUpdatesAsync(symbol, update => handler(update.AsExchangeEvent(Exchange, new SharedSpotTicker(symbol, update.Data.LastPrice ?? 0, update.Data.HighPrice ?? 0, update.Data.LowPrice ?? 0, update.Data.Volume, update.Data.ChangePercentage * 100))), ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        #region Trade client

        EndpointOptions<SubscribeTradeRequest> ITradeSocketClient.SubscribeTradeOptions { get; } = new EndpointOptions<SubscribeTradeRequest>(false);
        async Task<ExchangeResult<UpdateSubscription>> ITradeSocketClient.SubscribeToTradeUpdatesAsync(SubscribeTradeRequest request, Action<ExchangeEvent<IEnumerable<SharedTrade>>> handler, CancellationToken ct)
        {
            var validationError = ((ITradeSocketClient)this).SubscribeTradeOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);

            var symbol = request.Symbol.GetSymbol(FormatSymbol);
            var result = await SubscribeToTradeUpdatesAsync(symbol, update => handler(update.AsExchangeEvent<IEnumerable<SharedTrade>>(Exchange, new[] { new SharedTrade(update.Data.Quantity, update.Data.Price, update.Data.Timestamp){
                Side = update.Data.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell
            } })), ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        #region Book Ticker client

        EndpointOptions<SubscribeBookTickerRequest> IBookTickerSocketClient.SubscribeBookTickerOptions { get; } = new EndpointOptions<SubscribeBookTickerRequest>(false);
        async Task<ExchangeResult<UpdateSubscription>> IBookTickerSocketClient.SubscribeToBookTickerUpdatesAsync(SubscribeBookTickerRequest request, Action<ExchangeEvent<SharedBookTicker>> handler, CancellationToken ct)
        {
            var validationError = ((IBookTickerSocketClient)this).SubscribeBookTickerOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);

            var symbol = request.Symbol.GetSymbol(FormatSymbol);
            var result = await SubscribeToBookTickerUpdatesAsync(symbol, update => handler(update.AsExchangeEvent(Exchange, new SharedBookTicker(update.Data.BestAsk.Price, update.Data.BestAsk.Quantity, update.Data.BestBid.Price, update.Data.BestBid.Quantity))), ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        #region Kline client
        SubscribeKlineOptions IKlineSocketClient.SubscribeKlineOptions { get; } = new SubscribeKlineOptions(false);
        async Task<ExchangeResult<UpdateSubscription>> IKlineSocketClient.SubscribeToKlineUpdatesAsync(SubscribeKlineRequest request, Action<ExchangeEvent<SharedKline>> handler, CancellationToken ct)
        {
            var interval = (Enums.KlineInterval)request.Interval;
            if (!Enum.IsDefined(typeof(Enums.KlineInterval), interval))
                return new ExchangeResult<UpdateSubscription>(Exchange, new ArgumentError("Interval not supported"));

            var validationError = ((IKlineSocketClient)this).SubscribeKlineOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);

            var symbol = request.Symbol.GetSymbol(FormatSymbol);
            var result = await SubscribeToKlineUpdatesAsync(symbol, interval, update => handler(update.AsExchangeEvent(Exchange, new SharedKline(update.Data.Candles.OpenTime, update.Data.Candles.ClosePrice, update.Data.Candles.HighPrice, update.Data.Candles.LowPrice, update.Data.Candles.OpenPrice, update.Data.Candles.Volume))), ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        #region Order Book client
        SubscribeOrderBookOptions IOrderBookSocketClient.SubscribeOrderBookOptions { get; } = new SubscribeOrderBookOptions(false, new[] { 5, 50 });
        async Task<ExchangeResult<UpdateSubscription>> IOrderBookSocketClient.SubscribeToOrderBookUpdatesAsync(SubscribeOrderBookRequest request, Action<ExchangeEvent<SharedOrderBook>> handler, CancellationToken ct)
        {
            var validationError = ((IOrderBookSocketClient)this).SubscribeOrderBookOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);

            var symbol = request.Symbol.GetSymbol(FormatSymbol);
            var result = await SubscribeToOrderBookUpdatesAsync(symbol, request.Limit ?? 5, update => handler(update.AsExchangeEvent(Exchange, new SharedOrderBook(update.Data.Asks, update.Data.Bids))), ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        #region Balance client
        EndpointOptions<SubscribeBalancesRequest> IBalanceSocketClient.SubscribeBalanceOptions { get; } = new EndpointOptions<SubscribeBalancesRequest>(false);
        async Task<ExchangeResult<UpdateSubscription>> IBalanceSocketClient.SubscribeToBalanceUpdatesAsync(SubscribeBalancesRequest request, Action<ExchangeEvent<IEnumerable<SharedBalance>>> handler, CancellationToken ct)
        {
            var validationError = ((IBalanceSocketClient)this).SubscribeBalanceOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);
            var result = await SubscribeToBalanceUpdatesAsync(
                update => handler(update.AsExchangeEvent<IEnumerable<SharedBalance>>(Exchange, new[] { new SharedBalance(update.Data.Asset, update.Data.Available, update.Data.Total) })),
                ct: ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        #region Spot Order client

        EndpointOptions<SubscribeSpotOrderRequest> ISpotOrderSocketClient.SubscribeSpotOrderOptions { get; } = new EndpointOptions<SubscribeSpotOrderRequest>(false);
        async Task<ExchangeResult<UpdateSubscription>> ISpotOrderSocketClient.SubscribeToSpotOrderUpdatesAsync(SubscribeSpotOrderRequest request, Action<ExchangeEvent<IEnumerable<SharedSpotOrder>>> handler, CancellationToken ct)
        {
            var validationError = ((ISpotOrderSocketClient)this).SubscribeSpotOrderOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);

            var result = await SubscribeToOrderUpdatesAsync(
                update => handler(update.AsExchangeEvent<IEnumerable<SharedSpotOrder>>(Exchange, new[] { ParseOrder(update.Data) })),
                update => handler(update.AsExchangeEvent<IEnumerable<SharedSpotOrder>>(Exchange, new[] { ParseOrder(update.Data) })),
                update => handler(update.AsExchangeEvent<IEnumerable<SharedSpotOrder>>(Exchange, new[] { ParseOrder(update.Data) })),
                ct: ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        private SharedSpotOrder ParseOrder(GeminiStreamOrderBaseUpdate orderUpdate)
        {
            if (orderUpdate is GeminiStreamOrderNewUpdate update)
            {
                return new SharedSpotOrder(
                            update.Symbol,
                            update.OrderId.ToString(),
                            update.OrderType == Enums.OrderType.Limit ? SharedOrderType.Limit : update.OrderType == Enums.OrderType.Market ? SharedOrderType.Market : SharedOrderType.Other,
                            update.Side == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                            ParseStatus(update.Status, update.UpdateType),
                            update.OrderTime)
                {
                    ClientOrderId = update.ClientOrderid?.ToString(),
                    Quantity = update.OriginalQuantity,
                    QuantityFilled = 0,
                    QuoteQuantity = update.OriginalValue,
                    QuoteQuantityFilled = 0,
                    OrderPrice = update.Price,
                    Fee = 0
                };
            }
            if (orderUpdate is GeminiStreamOrderMatchUpdate matchUpdate)
            {
                return new SharedSpotOrder(
                            matchUpdate.Symbol,
                            matchUpdate.OrderId.ToString(),
                            matchUpdate.OrderType == Enums.OrderType.Limit ? SharedOrderType.Limit : matchUpdate.OrderType == Enums.OrderType.Market ? SharedOrderType.Market : SharedOrderType.Other,
                            matchUpdate.Side == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                            ParseStatus(matchUpdate.Status, matchUpdate.UpdateType),
                            matchUpdate.OrderTime)
                {
                    ClientOrderId = matchUpdate.ClientOrderid?.ToString(),
                    Quantity = matchUpdate.OriginalQuantity,
                    QuantityFilled = matchUpdate.QuantityFilled,
                    QuoteQuantity = matchUpdate.OriginalValue,
                    QuoteQuantityFilled = matchUpdate.OriginalValue - (matchUpdate.QuoteQuantityRemaining + matchUpdate.ValueCanceled),
                    OrderPrice = matchUpdate.Price,
                    UpdateTime = matchUpdate.Timestamp,
                    LastTrade = new SharedUserTrade(matchUpdate.Symbol, matchUpdate.OrderId, matchUpdate.TradeId, matchUpdate.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell, matchUpdate.MatchQuantity, matchUpdate.MatchPrice, matchUpdate.Timestamp)
                    {
                        Role = matchUpdate.Liquidity == LiquidityType.Taker ? SharedRole.Taker : SharedRole.Maker
                    }
                };
            }
            if (orderUpdate is GeminiStreamOrderUpdate upd)
            {
                return new SharedSpotOrder(
                            upd.Symbol,
                            upd.OrderId.ToString(),
                            upd.OrderType == Enums.OrderType.Limit ? SharedOrderType.Limit : upd.OrderType == Enums.OrderType.Market ? SharedOrderType.Market : SharedOrderType.Other,
                            upd.Side == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                            ParseStatus(upd.Status, upd.UpdateType),
                            upd.OrderTime)
                {
                    ClientOrderId = upd.ClientOrderid?.ToString(),
                    Quantity = upd.OriginalQuantity,
                    QuantityFilled = upd.QuantityFilled,
                    QuoteQuantity = upd.OriginalValue,
                    QuoteQuantityFilled = upd.OriginalValue - (upd.QuoteQuantityRemaining + upd.ValueCanceled),
                    OrderPrice = upd.Price,
                    UpdateTime = upd.Timestamp
                };
            }

            throw new Exception("Unknown order update type");
        }

        private SharedOrderStatus ParseStatus(ExtendedOrderStatus? status, MatchUpdateType? updateType)
        {
            if (status == ExtendedOrderStatus.New)
                return SharedOrderStatus.Open;

            if (updateType == MatchUpdateType.Canceled)
                return SharedOrderStatus.Canceled;

            if (updateType == MatchUpdateType.Filled)
                return SharedOrderStatus.Filled;

            return SharedOrderStatus.Open;
        }
    }
}
