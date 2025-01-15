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
    /// Gemini Spot trading high frequency endpoints, placing and mananging orders.
    /// </summary>
    public interface IGeminiRestClientSpotApiHfTrading
    {
        /// <summary>
        /// Places an order and returns once the order is confirmed. This is the faster version of <see cref="PlaceOrderWaitAsync" />
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/place-hf-order" /></para>
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
        /// Places an order and wait for and return the full order result. This is the slower version of <see cref="PlaceOrderAsync" />
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/sync-place-hf-order" /></para>
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
        Task<WebCallResult<GeminiHfOrder>> PlaceOrderWaitAsync(
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
        /// Place a new test order. Only validates the parameters, but doesn't actually process the order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/place-hf-order-test" /></para>
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
        /// <returns></returns>
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
        /// Modify an order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/modify-hf-order" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="orderId">The id of the order to modify</param>
        /// <param name="clientOrderId">The client id of the order to modify</param>
        /// <param name="newQuantity">New order quantity</param>
        /// <param name="newPrice">New order price</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The id of the modified order</returns>
        Task<WebCallResult<GeminiModifiedOrder>> EditOrderAsync(
            string symbol,
            string? orderId = null,
            string? clientOrderId = null,
            decimal? newQuantity = null,
            decimal? newPrice = null,
            CancellationToken ct = default);

        /// <summary>
        /// Place multiple orders and only wait for confirmation. This is the faster version of <see cref="PlaceMultipleOrdersWaitAsync" />
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/place-multiple-orders" /></para>
        /// </summary>
        /// <param name="orders">Orders to place</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiBulkMinimalResponseEntry>>> PlaceMultipleOrdersAsync(IEnumerable<GeminiHfBulkOrderRequestEntry> orders, CancellationToken ct = default);

        /// <summary>
        /// Place multiple orders and wait for and return the full order results. This is the slower version of <see cref="PlaceMultipleOrdersAsync" />
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/sync-place-multiple-hf-orders" /></para>
        /// </summary>
        /// <param name="orders">Orders to place</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiHfBulkOrderResponse>>> PlaceMultipleOrdersWaitAsync(IEnumerable<GeminiHfBulkOrderRequestEntry> orders, CancellationToken ct = default);

        /// <summary>
        /// Cancel an order and only wait for confirmation. This is the faster version of <see cref="CancelOrderWaitAsync" />
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/cancel-hf-order-by-orderid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="orderId">The id of the order to cancel</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of cancelled orders</returns>
        Task<WebCallResult<GeminiOrderId>> CancelOrderAsync(string symbol, string orderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel an order and wait for and return the full order results. This is the slower version of <see cref="CancelOrderAsync" />
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/sync-cancel-hf-order-by-orderid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="orderId">The id of the order to cancel</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of cancelled orders</returns>
        Task<WebCallResult<GeminiHfOrder>> CancelOrderWaitAsync(string symbol, string orderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel an order by clientOrderId and only wait for confirmation. This is the faster version of <see cref="CancelOrderWaitAsync" />
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/cancel-hf-order-by-clientoid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiClientOrderId>> CancelOrderByClientOrderIdAsync(string symbol, string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel an order by clientOrderId and wait for and return the full order results. This is the slower version of <see cref="CancelOrderByClientOrderIdAsync" />
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/sync-cancel-hf-order-by-clientoid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiHfOrder>> CancelOrderByClientOrderIdWaitAsync(string symbol, string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Get info on a specific order
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/get-hf-order-details-by-orderid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="orderId">The id of the order</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Order info</returns>
        Task<WebCallResult<GeminiHfOrderDetails>> GetOrderAsync(string symbol, string orderId, CancellationToken ct = default);

        /// <summary>
        /// Get info on a specific order by clientOrderId
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/get-hf-order-details-by-clientoid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="clientOrderId">The clientOrderId of the order</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiHfOrderDetails>> GetOrderByClientOrderIdAsync(string symbol, string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel all orders on a symbol
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/cancel-all-hf-orders-by-symbol" /></para>
        /// </summary>
        /// <param name="symbol">The symbol, for example `ETH-USDT`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult> CancelAllOrdersBySymbolAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Cancel all orders on all symbols
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/cancel-all-hf-orders" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiCanceledSymbols>> CancelAllOrdersAsync(CancellationToken ct = default);

        /// <summary>
        /// Get list open orders
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/get-active-hf-orders-list" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiHfOrderDetails>>> GetOpenOrdersAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Get a list of symbols which have open orders
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/get-symbol-with-active-hf-orders-list" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiOpenOrderSymbols>> GetSymbolsWithOpenOrdersAsync(CancellationToken ct = default);

        /// <summary>
        /// Get list of closed orders
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/get-hf-completed-order-list" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="side">Filter by side</param>
        /// <param name="type">Filter by type</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="lastId">Last id of previous result</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiHfPaginated<GeminiHfOrderDetails>>> GetClosedOrdersAsync(string symbol, OrderSide? side = null, OrderType? type = null, DateTime? startTime = null, DateTime? endTime = null, long? lastId = null, int? limit = null, CancellationToken ct = default);

        /// <summary>
        /// Get list of user trades
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/get-hf-completed-order-list" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="side">Filter by side</param>
        /// <param name="type">Filter by type</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="orderId">Filter by order id</param>
        /// <param name="tradeType">Filter by trade type</param>
        /// <param name="lastId">Last id of previous result</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiHfPaginated<GeminiUserTrade>>> GetUserTradesAsync(string symbol, Enums.OrderSide? side = null, Enums.OrderType? type = null, DateTime? startTime = null, DateTime? endTime = null, string? orderId = null, TradeType? tradeType = null, long? lastId = null, int? limit = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel all orders after a certain period. Calling this endpoint again will reset the timer. Using TimeSpan.Zero will disable the timer
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/auto-cancel-hf-order-setting" /></para>
        /// </summary>
        /// <param name="cancelAfter">Cancel after this period</param>
        /// <param name="symbols">Symbols to cancel orders on, for example `ETH-USDT`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiCancelAfter>> CancelAfterAsync(TimeSpan cancelAfter, IEnumerable<string>? symbols = null, CancellationToken ct = default);

        /// <summary>
        /// Get cancel after status
        /// <para><a href="https://www.gemini.com/docs/rest/spot-trading/spot-hf-trade-pro-account/auto-cancel-hf-order-setting-query" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiCancelAfterStatus?>> GetCancelAfterStatusAsync(CancellationToken ct = default);

        /// <summary>
        /// Place a new Margin Order
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-hf-trade/place-hf-order" /></para>
        /// </summary>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="side">The side((buy or sell) of the order</param>
        /// <param name="symbol">The symbol the order is for, for example `ETH-USDT`</param>
        /// <param name="type">The type of the order</param>
        /// <param name="remark">Remark on the order</param>
        /// <param name="selfTradePrevention">Self trade prevention setting</param>
        /// <param name="isIsolated">Is isolated margin</param>
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
            bool? isIsolated = null,
            bool? autoBorrow = null,
            bool? autoRepay = null,
            SelfTradePrevention? selfTradePrevention = null,
            string? clientOrderId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Place a new test Margin Order. Will only validate the parameters but not actually process the order
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-hf-trade/place-hf-order-test" /></para>
        /// </summary>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="side">The side((buy or sell) of the order</param>
        /// <param name="symbol">The symbol the order is for, for example `ETH-USDT`</param>
        /// <param name="type">The type of the order</param>
        /// <param name="remark">Remark on the order</param>
        /// <param name="selfTradePrevention">Self trade prevention setting</param>
        /// <param name="isIsolated">Is isolated margin</param>
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
            bool? isIsolated = null,
            bool? autoBorrow = null,
            bool? autoRepay = null,
            SelfTradePrevention? selfTradePrevention = null,
            string? clientOrderId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Cancel a margin order
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-hf-trade/cancel-hf-order-by-orderid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="orderId">Order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiOrderId>> CancelMarginOrderAsync(string symbol, string orderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel a margin order by clientOrderId
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-hf-trade/cancel-hf-order-by-clientoid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiClientOrderId>> CancelMarginOrderByClientOrderIdAsync(string symbol, string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Cancel all margin orders on a symbol
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-hf-trade/cancel-all-hf-orders-by-symbol" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult> CancelAllMarginOrdersBySymbolAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Get open margin orders
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-hf-trade/get-active-hf-orders-list" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="type">Trade type</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiHfOrderDetails>>> GetOpenMarginOrdersAsync(string symbol, TradeType type, CancellationToken ct = default);

        /// <summary>
        /// Get closed margin orders
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-hf-trade/get-hf-filled-list" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="side">Filter by side</param>
        /// <param name="type">Filter by type</param>
        /// <param name="tradeType">Filter by trade type</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="lastId">Last id of previous result</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiHfPaginated<GeminiHfOrderDetails>>> GetClosedMarginOrdersAsync(string symbol, OrderSide? side = null, OrderType? type = null, TradeType? tradeType = null, DateTime? startTime = null, DateTime? endTime = null, long? lastId = null, int? limit = null, CancellationToken ct = default);

        /// <summary>
        /// Get a margin order by order id
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-hf-trade/get-hf-order-details-by-orderid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="orderId">Order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiHfOrderDetails>> GetMarginOrderAsync(string symbol, string orderId, CancellationToken ct = default);

        /// <summary>
        /// Get a margin order by clientOrderId
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-hf-trade/get-hf-order-details-by-clientoid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiHfOrderDetails>> GetMarginOrderByClientOrderIdAsync(string symbol, string clientOrderId, CancellationToken ct = default);

        /// <summary>
        /// Get list of margin trades
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-hf-trade/get-hf-transaction-records" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="side">Filter by side</param>
        /// <param name="type">Filter by type</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="orderId">Filter by order id</param>
        /// <param name="tradeType">Filter by trade type</param>
        /// <param name="lastId">Last id of previous result</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiHfPaginated<GeminiUserTrade>>> GetMarginUserTradesAsync(string symbol, TradeType tradeType, Enums.OrderSide? side = null, Enums.OrderType? type = null, DateTime? startTime = null, DateTime? endTime = null, string? orderId = null, long? lastId = null, int? limit = null, CancellationToken ct = default);

        /// <summary>
        /// Get symbols with active margin orders
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-hf-trade/get-active-hf-order-symbols" /></para>
        /// </summary>
        /// <param name="isolated">true for isolated margin, false for cross margin</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiMarginOpenOrderSymbols>> GetMarginSymbolsWithOpenOrdersAsync(bool isolated, CancellationToken ct = default);
    }
}