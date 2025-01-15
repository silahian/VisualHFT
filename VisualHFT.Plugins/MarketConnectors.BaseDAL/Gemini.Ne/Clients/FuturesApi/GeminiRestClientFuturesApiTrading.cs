using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gemini.Net.Objects.Models;
using Gemini.Net.Objects.Models.Futures;
using Gemini.Net.Objects.Models.Spot;
using CryptoExchange.Net.Converters;
using Gemini.Net.Interfaces.Clients.FuturesApi;
using System.Security.Cryptography;
using System.Linq;

namespace Gemini.Net.Clients.FuturesApi
{
    /// <inheritdoc />
    internal class GeminiRestClientFuturesApiTrading : IGeminiRestClientFuturesApiTrading
    {
        private static readonly RequestDefinitionCache _definitions = new();
        private readonly GeminiRestClientFuturesApi _baseClient;

        internal GeminiRestClientFuturesApiTrading(GeminiRestClientFuturesApi baseClient)
        {
            _baseClient = baseClient;
        }

        #region Orders

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiOrderId>> PlaceOrderAsync(
            string symbol,
            OrderSide side,
            NewOrderType type,
            decimal leverage,
            int quantity,

            decimal? price = null,
            TimeInForce? timeInForce = null,
            bool? postOnly = null,
            bool? hidden = null,
            bool? iceberg = null,
            decimal? visibleSize = null,

            string? remark = null,
            StopType? stopType = null,
            StopPriceType? stopPriceType = null,
            decimal? stopPrice = null,
            bool? reduceOnly = null,
            bool? closeOrder = null,
            bool? forceHold = null,
            string? clientOrderId = null,
            SelfTradePrevention? selfTradePrevention = null,
            FuturesMarginMode? marginMode = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            parameters.AddParameter("side", JsonConvert.SerializeObject(side, new OrderSideConverter(false)));
            parameters.AddParameter("type", JsonConvert.SerializeObject(type, new NewOrderTypeConverter(false)));
            parameters.AddParameter("leverage", leverage.ToString(CultureInfo.InvariantCulture));
            parameters.AddParameter("size", quantity.ToString(CultureInfo.InvariantCulture));
            parameters.AddParameter("clientOid", clientOrderId ?? Guid.NewGuid().ToString());
            parameters.AddOptionalParameter("remark", remark);
            parameters.AddOptionalParameter("stop", stopType != null ? JsonConvert.SerializeObject(stopType, new StopTypeConverter(false)) : null);
            parameters.AddOptionalParameter("stopPriceType", stopPriceType != null ? JsonConvert.SerializeObject(stopPriceType, new StopPriceTypeConverter(false)) : null);
            parameters.AddOptionalParameter("stopPrice", stopPrice?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("reduceOnly", reduceOnly?.ToString());
            parameters.AddOptionalParameter("closeOrder", closeOrder?.ToString());
            parameters.AddOptionalParameter("forceHold", forceHold?.ToString());
            parameters.AddOptionalParameter("price", price?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("timeInForce", timeInForce != null ? JsonConvert.SerializeObject(timeInForce, new TimeInForceConverter(false)): null);
            parameters.AddOptionalParameter("postOnly", postOnly?.ToString());
            parameters.AddOptionalParameter("hidden", hidden?.ToString());
            parameters.AddOptionalParameter("iceberg", iceberg);
            parameters.AddOptionalParameter("visibleSize", visibleSize?.ToString());
            parameters.AddOptionalEnum("stp", selfTradePrevention);
            parameters.AddOptionalEnum("marginMode", marginMode);

            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/orders", GeminiExchange.RateLimiter.FuturesRest, 2, true);
            return await _baseClient.SendAsync<GeminiOrderId>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiOrderId>> PlaceTestOrderAsync(
            string symbol,
            OrderSide side,
            NewOrderType type,
            decimal leverage,
            int quantity,

            decimal? price = null,
            TimeInForce? timeInForce = null,
            bool? postOnly = null,
            bool? hidden = null,
            bool? iceberg = null,
            decimal? visibleSize = null,

            string? remark = null,
            StopType? stopType = null,
            StopPriceType? stopPriceType = null,
            decimal? stopPrice = null,
            bool? reduceOnly = null,
            bool? closeOrder = null,
            bool? forceHold = null,
            string? clientOrderId = null,
            SelfTradePrevention? selfTradePrevention = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            parameters.AddParameter("side", JsonConvert.SerializeObject(side, new OrderSideConverter(false)));
            parameters.AddParameter("type", JsonConvert.SerializeObject(type, new NewOrderTypeConverter(false)));
            parameters.AddParameter("leverage", leverage.ToString(CultureInfo.InvariantCulture));
            parameters.AddParameter("size", quantity.ToString(CultureInfo.InvariantCulture));
            parameters.AddParameter("clientOid", clientOrderId ?? Guid.NewGuid().ToString());
            parameters.AddOptionalParameter("remark", remark);
            parameters.AddOptionalParameter("stop", stopType != null ? JsonConvert.SerializeObject(stopType, new StopTypeConverter(false)) : null);
            parameters.AddOptionalParameter("stopPriceType", stopPriceType != null ? JsonConvert.SerializeObject(stopPriceType, new StopPriceTypeConverter(false)) : null);
            parameters.AddOptionalParameter("stopPrice", stopPrice?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("reduceOnly", reduceOnly?.ToString());
            parameters.AddOptionalParameter("closeOrder", closeOrder?.ToString());
            parameters.AddOptionalParameter("forceHold", forceHold?.ToString());
            parameters.AddOptionalParameter("price", price?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("timeInForce", timeInForce != null ? JsonConvert.SerializeObject(timeInForce, new TimeInForceConverter(false)) : null);
            parameters.AddOptionalParameter("postOnly", postOnly?.ToString());
            parameters.AddOptionalParameter("hidden", hidden?.ToString());
            parameters.AddOptionalParameter("iceberg", iceberg);
            parameters.AddOptionalParameter("visibleSize", visibleSize?.ToString());
            parameters.AddOptionalEnum("stp", selfTradePrevention);

            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/orders/test", GeminiExchange.RateLimiter.FuturesRest, 2, true);
            return await _baseClient.SendAsync<GeminiOrderId>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiOrderId>> PlaceTpSlOrderAsync(
            string symbol,
            OrderSide side,
            NewOrderType type,
            decimal leverage,
            int quantity,

            decimal? price = null,
            TimeInForce? timeInForce = null,
            bool? postOnly = null,
            bool? hidden = null,
            bool? iceberg = null,
            decimal? visibleSize = null,

            string? remark = null,
            decimal? takeProfitPrice = null,
            decimal? stopLossPrice = null,
            StopPriceType? stopPriceType = null,
            bool? reduceOnly = null,
            bool? closeOrder = null,
            bool? forceHold = null,
            string? clientOrderId = null,
            SelfTradePrevention? selfTradePrevention = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            parameters.AddParameter("side", JsonConvert.SerializeObject(side, new OrderSideConverter(false)));
            parameters.AddParameter("type", JsonConvert.SerializeObject(type, new NewOrderTypeConverter(false)));
            parameters.AddParameter("leverage", leverage.ToString(CultureInfo.InvariantCulture));
            parameters.AddParameter("size", quantity.ToString(CultureInfo.InvariantCulture));
            parameters.AddParameter("clientOid", clientOrderId ?? Guid.NewGuid().ToString());
            parameters.AddOptionalParameter("remark", remark);
            parameters.AddOptionalString("triggerStopUpPrice", takeProfitPrice);
            parameters.AddOptionalParameter("stopPriceType", stopPriceType != null ? JsonConvert.SerializeObject(stopPriceType, new StopPriceTypeConverter(false)) : null);
            parameters.AddOptionalString("triggerStopDownPrice", stopLossPrice);
            parameters.AddOptionalParameter("reduceOnly", reduceOnly?.ToString());
            parameters.AddOptionalParameter("closeOrder", closeOrder?.ToString());
            parameters.AddOptionalParameter("forceHold", forceHold?.ToString());
            parameters.AddOptionalParameter("price", price?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("timeInForce", timeInForce != null ? JsonConvert.SerializeObject(timeInForce, new TimeInForceConverter(false)) : null);
            parameters.AddOptionalParameter("postOnly", postOnly?.ToString());
            parameters.AddOptionalParameter("hidden", hidden?.ToString());
            parameters.AddOptionalParameter("iceberg", iceberg);
            parameters.AddOptionalParameter("visibleSize", visibleSize?.ToString());
            parameters.AddOptionalEnum("stp", selfTradePrevention);

            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/st-orders", GeminiExchange.RateLimiter.FuturesRest, 2, true);
            return await _baseClient.SendAsync<GeminiOrderId>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiFuturesOrderResult>>> PlaceMultipleOrdersAsync(IEnumerable<GeminiFuturesOrderRequestEntry> orders, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "<BODY>", orders }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/orders/multi", GeminiExchange.RateLimiter.FuturesRest, 20, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiFuturesOrderResult>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiCanceledOrders>> CancelOrderAsync(string orderId, CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Delete, $"api/v1/orders/" + orderId, GeminiExchange.RateLimiter.FuturesRest, 1, true);
            return await _baseClient.SendAsync<GeminiCanceledOrders>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiFuturesOrderResult>>> CancelMultipleOrdersAsync(IEnumerable<string>? orderIds = null, IEnumerable<GeminiCancelRequest>? clientOrderIds = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("orderIdsList", orderIds.ToArray());
            parameters.AddOptional("clientOidsList", clientOrderIds.ToArray());
            var request = _definitions.GetOrCreate(HttpMethod.Delete, $"api/v1/orders/multi-cancel", GeminiExchange.RateLimiter.FuturesRest, 30, true, parameterPosition: HttpMethodParameterPosition.InBody);
            return await _baseClient.SendAsync<IEnumerable<GeminiFuturesOrderResult>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiCanceledOrder>> CancelOrderByClientOrderIdAsync(string symbol, string clientOrderId, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "symbol", symbol }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Delete, $"api/v1/orders/client-order/" + clientOrderId, GeminiExchange.RateLimiter.FuturesRest, 1, true);
            return await _baseClient.SendAsync<GeminiCanceledOrder>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiCanceledOrders>> CancelAllOrdersAsync(string? symbol = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Delete, $"api/v1/orders", GeminiExchange.RateLimiter.FuturesRest, 30, true);
            return await _baseClient.SendAsync<GeminiCanceledOrders>(request, parameters, ct).ConfigureAwait(false);
        }


        /// <inheritdoc />
        public async Task<WebCallResult<GeminiCanceledOrders>> CancelAllStopOrdersAsync(string? symbol = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Delete, $"api/v1/stopOrders", GeminiExchange.RateLimiter.FuturesRest, 15, true);
            return await _baseClient.SendAsync<GeminiCanceledOrders>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiFuturesOrder>>> GetOrdersAsync(string? symbol = null, OrderStatus? status = null, OrderSide? side = null, OrderType? type = null, DateTime? startTime = null, DateTime? endTime = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            parameters.AddOptionalParameter("status", status == null ? null : JsonConvert.SerializeObject(status, new OrderStatusConverter(false)));
            parameters.AddOptionalParameter("side", side == null ? null : JsonConvert.SerializeObject(side, new OrderSideConverter(false)));
            parameters.AddOptionalParameter("type", type == null ? null : JsonConvert.SerializeObject(type, new OrderTypeConverter(false)));
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("currentPage", currentPage);
            parameters.AddOptionalParameter("pageSize", pageSize);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/orders", GeminiExchange.RateLimiter.FuturesRest, 2, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiFuturesOrder>>(request, parameters, ct).ConfigureAwait(false);
        }


        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiFuturesOrder>>> GetUntriggeredStopOrdersAsync(string? symbol = null, OrderSide? side = null, OrderType? type = null, DateTime? startTime = null, DateTime? endTime = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            parameters.AddOptionalParameter("side", side == null ? null : JsonConvert.SerializeObject(side, new OrderSideConverter(false)));
            parameters.AddOptionalParameter("type", type == null ? null : JsonConvert.SerializeObject(type, new OrderTypeConverter(false)));
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("currentPage", currentPage);
            parameters.AddOptionalParameter("pageSize", pageSize);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/stopOrders", GeminiExchange.RateLimiter.FuturesRest, 6, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiFuturesOrder>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiFuturesOrder>>> GetClosedOrdersAsync(string? symbol = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/recentDoneOrders", GeminiExchange.RateLimiter.FuturesRest, 5, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiFuturesOrder>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiFuturesOrder>> GetOrderAsync(string orderId, CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/orders/" + orderId, GeminiExchange.RateLimiter.FuturesRest, 5, true);
            return await _baseClient.SendAsync<GeminiFuturesOrder>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiFuturesOrder>> GetOrderByClientOrderIdAsync(string clientOrderId, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("clientOid", clientOrderId);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/orders/byClientOid", GeminiExchange.RateLimiter.FuturesRest, 5, true);
            return await _baseClient.SendAsync<GeminiFuturesOrder>(request, parameters, ct).ConfigureAwait(false);
        }
        #endregion

        #region Fills

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiFuturesUserTrade>>> GetUserTradesAsync(string? orderId = null, string? symbol = null, OrderSide? side = null, OrderType? type = null, DateTime? startTime = null, DateTime? endTime = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            parameters.AddOptionalParameter("orderId", orderId);
            parameters.AddOptionalParameter("side", side == null ? null : JsonConvert.SerializeObject(side, new OrderSideConverter(false)));
            parameters.AddOptionalParameter("type", type == null ? null : JsonConvert.SerializeObject(type, new OrderTypeConverter(false)));
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("currentPage", currentPage);
            parameters.AddOptionalParameter("pageSize", pageSize);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/fills", GeminiExchange.RateLimiter.FuturesRest, 5, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiFuturesUserTrade>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiFuturesUserTrade>>> GetRecentUserTradesAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/recentFills", GeminiExchange.RateLimiter.FuturesRest, 3, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiFuturesUserTrade>>(request, null, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Max Open Position Size

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiMaxOpenSize>> GetMaxOpenPositionSizeAsync(string symbol, decimal price, decimal leverage, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            parameters.Add("price", price);
            parameters.Add("leverage", leverage);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v2/getMaxOpenSize", GeminiExchange.RateLimiter.PublicRest, 2, true);
            return await _baseClient.SendAsync<GeminiMaxOpenSize>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion
    }
}
