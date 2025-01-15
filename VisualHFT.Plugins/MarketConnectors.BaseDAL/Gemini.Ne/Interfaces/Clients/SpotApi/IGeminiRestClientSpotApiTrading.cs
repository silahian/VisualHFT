using CryptoExchange.Net.Objects;
using Gemini.Net.Enums;
using Gemini.Net.Objects.Models;
using Gemini.Net.Objects.Models.Spot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gemini.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// Gemini Spot trading endpoints, placing and mananging orders.
    /// </summary>
    public interface IGeminiRestClientSpotApiTrading
    {
        /// <summary>
        /// Places an order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/orders/place-order" /></para>
        /// </summary>
        /// <param name="symbol">The symbol the order is for, for example `ETH-USDT`</param>
        /// <param name="side">The side of the order</param>
        /// <param name="type">The type of the order</param>
        /// <param name="price">The price of the order. Only valid for limit orders.</param>
        /// <param name="quantity">The quantity of the order</param>
        /// <param name="quoteQuantity">The quote quantity to use for the order. Only valid for market orders. If used, quantity needs to be empty</param>
        /// <param name="timeInForce">The time the order is in force</param>
        /// <param name="cancelAfter">Cancel after a time</param>
        /// <param name="postOnly">Order is post only</param>
        /// <param name="hidden">Order is hidden</param>
        /// <param name="iceBerg">Order is an iceberg order</param>
        /// <param name="visibleIceBergSize">The maximum visible size of an iceberg order</param>
        /// <param name="remark">Remark on the order</param>
        /// <param name="selfTradePrevention">Self trade prevention setting</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The id of the new order</returns>
        Task<WebCallResult<GeminiOrderId>> PlaceOrderAsync(
            string symbol,
            OrderSide side,
            NewOrderType type,
            decimal? quantity = null,
            decimal? price = null,
            decimal? quoteQuantity = null,
            TimeInForce? timeInForce = null,
            TimeSpan? cancelAfter = null,
            bool? postOnly = null,
            bool? hidden = null,
            bool? iceBerg = null,
            decimal? visibleIceBergSize = null,
            string? remark = null,
            string? clientOrderId = null,
            SelfTradePrevention? selfTradePrevention = null,
            CancellationToken ct = default);

        /// <summary>
        /// Places a test order. Order gets validated but won't be processed
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/orders/place-order-test" /></para>
        /// </summary>
        /// <param name="symbol">The symbol the order is for, for example `ETH-USDT`</param>
        /// <param name="side">The side of the order</param>
        /// <param name="type">The type of the order</param>
        /// <param name="price">The price of the order. Only valid for limit orders.</param>
        /// <param name="quantity">The quantity of the order</param>
        /// <param name="quoteQuantity">The quote quantity to use for the order. Only valid for market orders. If used, quantity needs to be empty</param>
        /// <param name="timeInForce">The time the order is in force</param>
        /// <param name="cancelAfter">Cancel after a time</param>
        /// <param name="postOnly">Order is post only</param>
        /// <param name="hidden">Order is hidden</param>
        /// <param name="iceBerg">Order is an iceberg order</param>
        /// <param name="visibleIceBergSize">The maximum visible size of an iceberg order</param>
        /// <param name="remark">Remark on the order</param>
        /// <param name="selfTradePrevention">Self trade prevention setting</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The id of the new order</returns>
        Task<WebCallResult<GeminiOrderId>> PlaceTestOrderAsync(
            string symbol,
            Enums.OrderSide side,
            NewOrderType type,
            decimal? quantity = null,
            decimal? price = null,
            decimal? quoteQuantity = null,
            TimeInForce? timeInForce = null,
            TimeSpan? cancelAfter = null,
            bool? postOnly = null,
            bool? hidden = null,
            bool? iceBerg = null,
            decimal? visibleIceBergSize = null,
            string? remark = null,
            string? clientOrderId = null,
            SelfTradePrevention? selfTradePrevention = null,
            CancellationToken ct = default);

        /// <summary>
        /// Places a margin order. Order gets validated but won't be processed
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/orders/place-margin-order-test" /></para>
        /// </summary>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="side">The side((buy or sell) of the order</param>
        /// <param name="symbol">The symbol the order is for, for example `ETH-USDT`</param>
        /// <param name="type">The type of the order</param>
        /// <param name="remark">Remark on the order</param>
        /// <param name="selfTradePrevention">Self trade prevention setting</param>
        /// <param name="marginMode">The type of trading, including 'cross' and 'isolated'</param>
        /// <param name="autoBorrow">Auto-borrow to place order.</param>
        /// <param name="autoRepay">Auto-repay to place order.</param>
        /// <param name="price">The price of the order. Only valid for limit orders.</param>
        /// <param name="quantity">Quantity of base asset to buy or sell of the order</param>
        /// <param name="timeInForce">The time the order is in force</param>
        /// <param name="cancelAfter">Cancel after a time</param>
        /// <param name="postOnly">Order is post only</param>
        /// <param name="hidden">Order is hidden</param>
        /// <param name="iceBerg">Order is an iceberg order</param>
        /// <param name="visibleIceBergSize">The maximum visible size of an iceberg order</param>
        /// <param name="quoteQuantity">The quote quantity to use for the order. Only valid for market orders. If used, quantity needs to be empty</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The id of the new order</returns>
        Task<WebCallResult<GeminiNewMarginOrder>> PlaceMarginOrderAsync(
            string symbol,
            OrderSide side,
            NewOrderType type,
            decimal? price = null,
            decimal? quantity = null,
            decimal? quoteQuantity = null,
            TimeInForce? timeInForce = null,
            TimeSpan? cancelAfter = null,
            bool? postOnly = null,
            bool? hidden = null,
            bool? iceBerg = null,
            decimal? visibleIceBergSize = null,
            string? remark = null,
            MarginMode? marginMode = null,
            bool? autoBorrow = null,
            bool? autoRepay = null,
            SelfTradePrevention? selfTradePrevention = null,
            string? clientOrderId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Places a test margin order
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/orders/place-margin-order-test" /></para>
        /// </summary>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="side">The side((buy or sell) of the order</param>
        /// <param name="symbol">The symbol the order is for, for example `ETH-USDT`</param>
        /// <param name="type">The type of the order</param>
        /// <param name="remark">Remark on the order</param>
        /// <param name="selfTradePrevention">Self trade prevention setting</param>
        /// <param name="marginMode">The type of trading, including 'cross' and 'isolated'</param>
        /// <param name="autoBorrow">Auto-borrow to place order.</param>
        /// <param name="autoRepay">Auto-repay to place order.</param>
        /// <param name="price">The price of the order. Only valid for limit orders.</param>
        /// <param name="quantity">Quantity of base asset to buy or sell of the order</param>
        /// <param name="timeInForce">The time the order is in force</param>
        /// <param name="cancelAfter">Cancel after a time</param>
        /// <param name="postOnly">Order is post only</param>
        /// <param name="hidden">Order is hidden</param>
        /// <param name="iceBerg">Order is an iceberg order</param>
        /// <param name="visibleIceBergSize">The maximum visible size of an iceberg order</param>
        /// <param name="quoteQuantity">The quote quantity to use for the order. Only valid for market orders. If used, quantity needs to be empty</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The id of the new order</returns>
        Task<WebCallResult<GeminiNewMarginOrder>> PlaceTestMarginOrderAsync(
            string symbol,
            Enums.OrderSide side,
            NewOrderType type,
            decimal? price = null,
            decimal? quantity = null,
            decimal? quoteQuantity = null,
            TimeInForce? timeInForce = null,
            TimeSpan? cancelAfter = null,
            bool? postOnly = null,
            bool? hidden = null,
            bool? iceBerg = null,
            decimal? visibleIceBergSize = null,
            string? remark = null,
            MarginMode? marginMode = null,
            bool? autoBorrow = null,
            bool? autoRepay = null,
            SelfTradePrevention? selfTradePrevention = null,
            string? clientOrderId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Placec a new OCO order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/oco-order/place-order" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="side">Order side</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="price">Price</param>
        /// <param name="stopPrice">Trigger price</param>
        /// <param name="limitPrice">Limit order price after trigger</param>
        /// <param name="tradeType">Transaction Type, currently only supports TRADE (spot transactions)</param>
        /// <param name="remark">User remark</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiOrderId>> PlaceOcoOrderAsync(
            string symbol,
            OrderSide side,
            decimal quantity,
            decimal price,
            decimal stopPrice,
            decimal limitPrice,
            TradeType? tradeType = null,
            string? remark = null,
            string? clientOrderId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Places bulk orders
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/orders/place-multiple-orders" /></para>
        /// </summary>
        /// <param name="symbol">The symbol the order is for, for example `ETH-USDT`</param>
        /// <param name="orders">Up to 5 orders to be placed at the same time</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of new orders</returns>
        Task<WebCallResult<GeminiBulkOrderResponse>> PlaceBulkOrderAsync(string symbol, IEnumerable<GeminiBulkOrderRequestEntry> orders, CancellationToken ct = default);

        /// <summary>
        /// Cancel an order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/orders/cancel-order-by-orderid" /></para>
        /// </summary>
        /// <param name="orderId">The id of the order to cancel</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of canceled orders</returns>
        Task<WebCallResult<GeminiCanceledOrders>> CancelOrderAsync(string orderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel an OCO order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/oco-order/cancel-order-by-orderid" /></para>
        /// </summary>
        /// <param name="orderId">The id of the order to cancel</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiCanceledOrders>> CancelOcoOrderAsync(string orderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel multiple OCO orders
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/oco-order/cancel-multiple-orders" /></para>
        /// </summary>
        /// <param name="orderIds">Order ids</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiCanceledOrders>> CancelOcoOrdersAsync(IEnumerable<string> orderIds, CancellationToken ct = default);

        /// <summary>
        /// Cancel an order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/orders/cancel-order-by-clientoid" /></para>
        /// </summary>
        /// <param name="clientOrderId">The client order id of the order to cancel</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of canceled orders</returns>
        Task<WebCallResult<GeminiCanceledOrder>> CancelOrderByClientOrderIdAsync(string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel an OCO order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/oco-order/cancel-order-by-clientoid" /></para>
        /// </summary>
        /// <param name="clientOrderId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WebCallResult<GeminiCanceledOrders>> CancelOcoOrderByClientOrderIdAsync(string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel all open orders
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/orders/cancel-all-orders" /></para>
        /// </summary>
        /// <param name="symbol">Only cancel orders for this symbol, for example `ETH-USDT`</param>
        /// <param name="tradeType">Only cancel orders for this type</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of canceled orders</returns>
        Task<WebCallResult<GeminiCanceledOrders>> CancelAllOrdersAsync(string? symbol = null, TradeType? tradeType = null, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of orders
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/orders/get-order-list" /></para>
        /// </summary>
        /// <param name="symbol">Filter list by symbol</param>
        /// <param name="type">Filter list by order type</param>
        /// <param name="side">Filter list by order side</param>
        /// <param name="startTime">Filter list by start time</param>
        /// <param name="endTime">Filter list by end time</param>
        /// <param name="status">Filter list by order status. Defaults to done</param>
        /// <param name="tradeType">The type of orders to retrieve</param>
        /// <param name="currentPage">The page to retrieve</param>
        /// <param name="pageSize">The amount of results per page</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of orders</returns>
        Task<WebCallResult<GeminiPaginated<GeminiOrder>>> GetOrdersAsync(string? symbol = null, OrderSide? side = null, OrderType? type = null, DateTime? startTime = null, DateTime? endTime = null, OrderStatus? status = null, TradeType? tradeType = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of max 1000 orders in the last 24 hours
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/orders/get-recent-orders-list" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of orders</returns>
        Task<WebCallResult<IEnumerable<GeminiOrder>>> GetRecentOrdersAsync(CancellationToken ct = default);

        /// <summary>
        /// Get OCO orders list
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/oco-order/get-order-list" /></para>
        /// </summary>
        /// <param name="symbol">Filter by symbol, for example `ETH-USDT`</param>
        /// <param name="orderIds">Filter by order ids</param>
        /// <param name="startTime">Filter list by start time</param>
        /// <param name="endTime">Filter list by end time</param>
        /// <param name="currentPage">The page to retrieve</param>
        /// <param name="pageSize">The amount of results per page</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginated<GeminiOcoOrder>>> GetOcoOrdersAsync(string? symbol = null, IEnumerable<string>? orderIds = null, DateTime? startTime = null, DateTime? endTime = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get info on a specific order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/orders/get-order-details-by-clientoid" /></para>
        /// </summary>
        /// <param name="clientOrderId">The client order id of the order</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Order info</returns>
        Task<WebCallResult<GeminiOrder>> GetOrderByClientOrderIdAsync(string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Get info on a specific OCO order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/oco-order/get-order-info-by-orderid" /></para>
        /// </summary>
        /// <param name="orderId">Order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiOcoOrder>> GetOcoOrderAsync(string orderId, CancellationToken ct = default);

        /// <summary>
        /// Get info on a specific OCO order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/oco-order/get-order-info-by-clientoid" /></para>
        /// </summary>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiOcoOrder>> GetOcoOrderByClientOrderIdAsync(string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Get details of an OCO order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/oco-order/get-order-details-by-orderid" /></para>
        /// </summary>
        /// <param name="orderId">Order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiOcoOrderDetails>> GetOcoOrderDetailsAsync(string orderId, CancellationToken ct = default);

        /// <summary>
        /// Get info on a specific order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/orders/get-order-details-by-orderid" /></para>
        /// </summary>
        /// <param name="orderId">The id of the order</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Order info</returns>
        Task<WebCallResult<GeminiOrder>> GetOrderAsync(string orderId, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of fills
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/fills/get-filled-list" /></para>
        /// </summary>
        /// <param name="symbol">Filter list by symbol, for example `ETH-USDT`</param>
        /// <param name="type">Filter list by order type</param>
        /// <param name="side">Filter list by order side</param>
        /// <param name="startTime">Filter list by start time</param>
        /// <param name="endTime">Filter list by end time</param>
        /// <param name="orderId">Filter list by order id</param>
        /// <param name="tradeType">The type of orders to retrieve</param>
        /// <param name="currentPage">The page to retrieve</param>
        /// <param name="pageSize">The amount of results per page</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of fills</returns>
        Task<WebCallResult<GeminiPaginated<GeminiUserTrade>>> GetUserTradesAsync(string? symbol = null, OrderSide? side = null, OrderType? type = null, DateTime? startTime = null, DateTime? endTime = null, string? orderId = null, TradeType? tradeType = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of max 1000 fills in the last 24 hours
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/fills/get-recent-filled-list" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of fills</returns>
        Task<WebCallResult<IEnumerable<GeminiUserTrade>>> GetRecentUserTradesAsync(CancellationToken ct = default);

        /// <summary>
        /// Place a new stop order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/stop-order/place-order" /></para>
        /// </summary>
        /// <param name="symbol">The symbol the order is for, for example `ETH-USDT`</param>
        /// <param name="orderSide">The side of the order</param>
        /// <param name="orderType">The type of the order</param>
        /// <param name="price">The price of the order. Only valid for limit orders.</param>
        /// <param name="quantity">The quantity of the order</param>
        /// <param name="quoteQuantity">The funds to use for the order. Only valid for market orders. If used, quantity needs to be empty</param>
        /// <param name="timeInForce">The time the order is in force</param>
        /// <param name="cancelAfter">Cancel after a time</param>
        /// <param name="postOnly">Order is post only</param>
        /// <param name="hidden">Order is hidden</param>
        /// <param name="iceberg">Order is an iceberg order</param>
        /// <param name="visibleSize">The maximum visible size of an iceberg order</param>
        /// <param name="remark">Remark on the order</param>
        /// <param name="selfTradePrevention">Self trade prevention setting</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="stopCondition">Stop price condition</param>
        /// <param name="stopPrice">Price to trigger the order placement</param>
        /// <param name="tradeType">Trade type</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiOrderId>> PlaceStopOrderAsync(
            string symbol,
            OrderSide orderSide,
            NewOrderType orderType,
            StopCondition stopCondition,
            decimal stopPrice,
            string? remark = null,
            SelfTradePrevention? selfTradePrevention = null,
            TradeType? tradeType = null,
            decimal? price = null,
            decimal? quantity = null,
            TimeInForce? timeInForce = null,
            TimeSpan? cancelAfter = null,
            bool? postOnly = null,
            bool? hidden = null,
            bool? iceberg = null,
            decimal? visibleSize = null,
            string? clientOrderId = null,
            decimal? quoteQuantity = null,
            CancellationToken ct = default);

        /// <summary>
        /// Cancel a stop order by order id
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/stop-order/cancel-order-by-orderid" /></para>
        /// </summary>
        /// <param name="orderId">Order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiCanceledOrders>> CancelStopOrderAsync(string orderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel a stop order by client order id
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/stop-order/cancel-order-by-clientoid" /></para>
        /// </summary>
        /// <param name="clientOrderId">The client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiCanceledOrder>> CancelStopOrderByClientOrderIdAsync(string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel all stop orders fitting the provided parameters
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/stop-order/cancel-stop-orders" /></para>
        /// </summary>
        /// <param name="symbol">Symbol to cancel orders on, for example `ETH-USDT`</param>
        /// <param name="orderIds">Order ids of the orders to cancel</param>
        /// <param name="tradeType">Trade type</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiCanceledOrders>> CancelStopOrdersAsync(string? symbol = null, IEnumerable<string>? orderIds = null, TradeType? tradeType = null, CancellationToken ct = default);

        /// <summary>
        /// Get a list of stop orders fitting the provided parameters
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/stop-order/get-stop-orders-list" /></para>
        /// </summary>
        /// <param name="activeOrders">True to return active orders, false for completed orders</param>
        /// <param name="symbol">Symbol of the orders, for example `ETH-USDT`</param>
        /// <param name="side">Side of the orders</param>
        /// <param name="type">Type of the orders</param>
        /// <param name="tradeType">Trade type</param>
        /// <param name="startTime">Filter list by start time</param>
        /// <param name="endTime">Filter list by end time</param>
        /// <param name="orderIds">Filter list by order ids</param>
        /// <param name="currentPage">The page to retrieve</param>
        /// <param name="pageSize">The amount of results per page</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginated<GeminiStopOrder>>> GetStopOrdersAsync(bool? activeOrders = null, string? symbol = null, OrderSide? side = null,
            OrderType? type = null, TradeType? tradeType = null, DateTime? startTime = null, DateTime? endTime = null, IEnumerable<string>? orderIds = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get a stop order by id
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/stop-order/get-order-details-by-orderid" /></para>
        /// </summary>
        /// <param name="orderId">Order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiStopOrder>> GetStopOrderAsync(string orderId, CancellationToken ct = default);

        /// <summary>
        /// Get a stop order by client order id
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/stop-order/get-order-details-by-clientoid" /></para>
        /// </summary>
        /// <param name="clientOrderId">The client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiStopOrder>>> GetStopOrderByClientOrderIdAsync(string clientOrderId, CancellationToken ct = default);

    }
}