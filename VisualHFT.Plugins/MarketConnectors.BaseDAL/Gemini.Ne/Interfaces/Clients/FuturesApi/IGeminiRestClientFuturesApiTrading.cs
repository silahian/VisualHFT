using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using Gemini.Net.Enums;
using Gemini.Net.Objects.Models;
using Gemini.Net.Objects.Models.Futures;
using Gemini.Net.Objects.Models.Spot;

namespace Gemini.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// Gemini Futures trading endpoints, placing and mananging orders.
    /// </summary>
    public interface IGeminiRestClientFuturesApiTrading
    {
        /// <summary>
        /// Place a new order
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/place-order" /></para>
        /// </summary>
        /// <param name="symbol">The contract for the order, for example `XBTUSDM`</param>
        /// <param name="side">Side of the order</param>
        /// <param name="type">Type of order</param>
        /// <param name="leverage">Leverage of the order</param>
        /// <param name="price">Limit price, only for limit orders</param>
        /// <param name="timeInForce">Time in force, only for limit orders</param>
        /// <param name="postOnly">Post only flag, invalid when timeInForce is IOC</param>
        /// <param name="hidden">Orders not displaying in order book. When hidden chose</param>
        /// <param name="iceberg">Only visible portion of the order is displayed in the order book</param>
        /// <param name="visibleSize">The maximum visible size of an iceberg order</param>
        /// <param name="quantity">Quantity of contract to buy or sell</param>
        /// <param name="remark">Remark for the order</param>
        /// <param name="stopType"></param>
        /// <param name="stopPriceType"></param>
        /// <param name="stopPrice">Stop price</param>
        /// <param name="reduceOnly">A mark to reduce the position size only. Set to false by default</param>
        /// <param name="closeOrder">A mark to close the position. Set to false by default. All the positions will be closed if true</param>
        /// <param name="forceHold">A mark to forcely hold the funds for an order, even though it's an order to reduce the position size. This helps the order stay on the order book and not get canceled when the position size changes. Set to false by default</param>
        /// <param name="selfTradePrevention">Self Trade Prevention mode</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="marginMode">Margin mode, defaults to Isolated</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Order details</returns>
        Task<WebCallResult<GeminiOrderId>> PlaceOrderAsync(
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
            CancellationToken ct = default);

        /// <summary>
        /// Place a test order. The order will not be executed or added to the order book, but can be used to verify the request parameters
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/place-order-test" /></para>
        /// </summary>
        /// <param name="symbol">The contract for the order, for example `XBTUSDM`</param>
        /// <param name="side">Side of the order</param>
        /// <param name="type">Type of order</param>
        /// <param name="leverage">Leverage of the order</param>
        /// <param name="price">Limit price, only for limit orders</param>
        /// <param name="timeInForce">Time in force, only for limit orders</param>
        /// <param name="postOnly">Post only flag, invalid when timeInForce is IOC</param>
        /// <param name="hidden">Orders not displaying in order book. When hidden chose</param>
        /// <param name="iceberg">Only visible portion of the order is displayed in the order book</param>
        /// <param name="visibleSize">The maximum visible size of an iceberg order</param>
        /// <param name="quantity">Quantity of contract to buy or sell</param>
        /// <param name="remark">Remark for the order</param>
        /// <param name="stopType"></param>
        /// <param name="stopPriceType"></param>
        /// <param name="stopPrice">Stop price</param>
        /// <param name="reduceOnly">A mark to reduce the position size only. Set to false by default</param>
        /// <param name="closeOrder">A mark to close the position. Set to false by default. All the positions will be closed if true</param>
        /// <param name="forceHold">A mark to forcely hold the funds for an order, even though it's an order to reduce the position size. This helps the order stay on the order book and not get canceled when the position size changes. Set to false by default</param>
        /// <param name="selfTradePrevention">Self Trade Prevention mode</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Order details</returns>
        Task<WebCallResult<GeminiOrderId>> PlaceTestOrderAsync(
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
            CancellationToken ct = default);


        /// <summary>
        /// Place a new take profit / stop loss order
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/place-take-profit-and-stop-loss-order" /></para>
        /// </summary>
        /// <param name="symbol">The contract for the order, for example `XBTUSDM`</param>
        /// <param name="side">Side of the order</param>
        /// <param name="type">Type of order</param>
        /// <param name="leverage">Leverage of the order</param>
        /// <param name="price">Limit price, only for limit orders</param>
        /// <param name="timeInForce">Time in force, only for limit orders</param>
        /// <param name="postOnly">Post only flag, invalid when timeInForce is IOC</param>
        /// <param name="hidden">Orders not displaying in order book. When hidden chose</param>
        /// <param name="iceberg">Only visible portion of the order is displayed in the order book</param>
        /// <param name="visibleSize">The maximum visible size of an iceberg order</param>
        /// <param name="quantity">Quantity of contract to buy or sell</param>
        /// <param name="remark">Remark for the order</param>
        /// <param name="stopPriceType">Price type</param>
        /// <param name="takeProfitPrice">Take profit price</param>
        /// <param name="stopLossPrice">Stop loss price</param>
        /// <param name="reduceOnly">A mark to reduce the position size only. Set to false by default</param>
        /// <param name="closeOrder">A mark to close the position. Set to false by default. All the positions will be closed if true</param>
        /// <param name="forceHold">A mark to forcely hold the funds for an order, even though it's an order to reduce the position size. This helps the order stay on the order book and not get canceled when the position size changes. Set to false by default</param>
        /// <param name="selfTradePrevention">Self Trade Prevention mode</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Order details</returns>
        Task<WebCallResult<GeminiOrderId>> PlaceTpSlOrderAsync(
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
            CancellationToken ct = default);

        /// <summary>
        /// Place multiple orders
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/place-multiple-orders" /></para>
        /// </summary>
        /// <param name="orders">The orders to place</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Order results. Each result should be checked for success</returns>
        Task<WebCallResult<IEnumerable<GeminiFuturesOrderResult>>> PlaceMultipleOrdersAsync(IEnumerable<GeminiFuturesOrderRequestEntry> orders, CancellationToken ct = default);

        /// <summary>
        /// Cancel an order
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/cancel-order-by-orderid" /></para>
        /// </summary>
        /// <param name="orderId">Id of the order to cancel</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Canceled id</returns>
        Task<WebCallResult<GeminiCanceledOrders>> CancelOrderAsync(string orderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel multiple orders
        /// </summary>
        /// <param name="orderIds">Order ids to cancel</param>
        /// <param name="clientOrderIds">Client order ids to cancel</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiFuturesOrderResult>>> CancelMultipleOrdersAsync(IEnumerable<string>? orderIds = null, IEnumerable<GeminiCancelRequest>? clientOrderIds = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel an order by client order id
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/cancel-order-by-clientoid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `XBTUSDM`</param>
        /// <param name="clientOrderId">Client order id of the order to cancel</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiCanceledOrder>> CancelOrderByClientOrderIdAsync(string symbol, string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel all open orders
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/cancel-multiple-futures-limit-orders" /></para>
        /// </summary>
        /// <param name="symbol">Cancel only orders for this symbol, for example `XBTUSDM`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Canceled ids</returns>
        Task<WebCallResult<GeminiCanceledOrders>> CancelAllOrdersAsync(string? symbol = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel all open stop orders
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/cancel-multiple-futures-stop-orders" /></para>
        /// </summary>
        /// <param name="symbol">Cancel only orders for this symbol</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Canceled ids</returns>
        Task<WebCallResult<GeminiCanceledOrders>> CancelAllStopOrdersAsync(string? symbol = null, CancellationToken ct = default);

        /// <summary>
        /// Get list of orders
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/get-order-list" /></para>
        /// </summary>
        /// <param name="symbol">Filter by symbol, for example `XBTUSDM`</param>
        /// <param name="status">Filter by status</param>
        /// <param name="side">Filter by side</param>
        /// <param name="type">Filter by type</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="pageSize">Size of a page</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of orders</returns>
        Task<WebCallResult<GeminiPaginated<GeminiFuturesOrder>>> GetOrdersAsync(string? symbol = null, OrderStatus? status = null, OrderSide? side = null, OrderType? type = null, DateTime? startTime = null, DateTime? endTime = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get list of untriggered stop orders
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/get-untriggered-stop-order-list" /></para>
        /// </summary>
        /// <param name="symbol">Filter by symbol, for example `XBTUSDM`</param>
        /// <param name="side">Filter by side</param>
        /// <param name="type">Filter by type</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="pageSize">Size of a page</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of orders</returns>
        Task<WebCallResult<GeminiPaginated<GeminiFuturesOrder>>> GetUntriggeredStopOrdersAsync(string? symbol = null, OrderSide? side = null, OrderType? type = null, DateTime? startTime = null, DateTime? endTime = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get list of 1000 most recent orders in the last 24 hours
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/get-list-of-orders-completed-in-24h" /></para>
        /// </summary>
        /// <param name="symbol">Filter by symbol, for example `XBTUSDM`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of orders</returns>
        Task<WebCallResult<IEnumerable<GeminiFuturesOrder>>> GetClosedOrdersAsync(string? symbol = null, CancellationToken ct = default);

        /// <summary>
        /// Get details on an order
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/get-order-details-by-orderid-clientoid" /></para>
        /// </summary>
        /// <param name="orderId">Id of order to retrieve</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of orders</returns>
        Task<WebCallResult<GeminiFuturesOrder>> GetOrderAsync(string orderId, CancellationToken ct = default);

        /// <summary>
        /// Get details on an order
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/orders/get-order-details-by-orderid-clientoid" /></para>
        /// </summary>
        /// <param name="clientOrderId">Client order id of order to retrieve</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of orders</returns>
        Task<WebCallResult<GeminiFuturesOrder>> GetOrderByClientOrderIdAsync(string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Get list of user trades
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/fills/get-filled-list" /></para>
        /// </summary>
        /// <param name="symbol">Filter by symbol, for example `XBTUSDM`</param>
        /// <param name="orderId">Filter by order id</param>
        /// <param name="side">Filter by side</param>
        /// <param name="type">Filter by type</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="pageSize">Size of a page</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of trades</returns>
        Task<WebCallResult<GeminiPaginated<GeminiFuturesUserTrade>>> GetUserTradesAsync(string? orderId = null, string? symbol = null, OrderSide? side = null, OrderType? type = null, DateTime? startTime = null, DateTime? endTime = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get list of 1000 most recent user trades in the last 24 hours
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/fills/get-recent-filled-list" /></para>
        /// </summary>        
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of trades</returns>
        Task<WebCallResult<IEnumerable<GeminiFuturesUserTrade>>> GetRecentUserTradesAsync(CancellationToken ct = default);

        /// <summary>
        /// Get the max position size
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/get-maximum-open-position-size" /></para>
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="price">Price</param>
        /// <param name="leverage">Leverage</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiMaxOpenSize>> GetMaxOpenPositionSizeAsync(string symbol, decimal price, decimal leverage, CancellationToken ct = default);

    }
}
