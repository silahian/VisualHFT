using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Gemini.Net.Enums;
using Gemini.Net.Objects.Models;
using Gemini.Net.Objects.Models.Futures.Socket;
using Gemini.Net.Objects.Models.Spot.Socket;

namespace Gemini.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// Spot socket api
    /// </summary>
    public interface IGeminiSocketClientSpotApi : ISocketApiClient, IDisposable
    {
        /// <summary>
        /// Get the shared socket subscription client. This interface is shared with other exhanges to allow for a common implementation for different exchanges.
        /// </summary>
        IGeminiSocketClientSpotApiShared SharedClient { get; }

        /// <summary>
        /// Subscribe to updates for a symbol ticker
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/ticker" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to subscribe to, for example `ETH-USDT`</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamTick>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to updates for a symbol ticker
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/ticker" /></para>
        /// </summary>
        /// <param name="symbols">The symbols to subscribe to, for example `ETH-USDT`</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamTick>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to updates for all symbol tickers
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/all-tickers" /></para>
        /// </summary>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToAllTickerUpdatesAsync(Action<DataEvent<GeminiStreamTick>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to updates for symbol or market snapshots
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/symbol-snapshot" /></para>
        /// </summary>
        /// <param name="symbolOrMarket">The symbol (ie KCS-BTC) or market (ie BTC) to subscribe on</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToSnapshotUpdatesAsync(string symbolOrMarket,
            Action<DataEvent<GeminiStreamSnapshot>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to updates for symbol or market snapshots
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/symbol-snapshot" /></para>
        /// </summary>
        /// <param name="symbolOrMarkets">The symbols (ie KCS-BTC) or markets (ie BTC) to subscribe on</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToSnapshotUpdatesAsync(
            IEnumerable<string> symbolOrMarkets, Action<DataEvent<GeminiStreamSnapshot>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to aggregated order book updates
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/level2-market-data" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to subscribe on, for example `ETH-USDT`</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToAggregatedOrderBookUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamOrderBook>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to aggregated order book updates
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/level2-market-data" /></para>
        /// </summary>
        /// <param name="symbols">The symbols to subscribe on, for example `ETH-USDT`</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToAggregatedOrderBookUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamOrderBook>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to trade updates
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/match-execution-data" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to subscribe on, for example `ETH-USDT`</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamMatch>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to trade updates
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/match-execution-data" /></para>
        /// </summary>
        /// <param name="symbols">The symbols to subscribe on, for example `ETH-USDT`</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamMatch>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to kline updates
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/klines" /></para>
        /// </summary>
        /// <param name="symbol">Symbol to subscribe, for example `ETH-USDT`</param>
        /// <param name="interval">Interval of the klines</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<DataEvent<GeminiStreamCandle>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to book ticker (best ask/bid) updates
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/level1-bbo-market-data" /></para>
        /// </summary>
        /// <param name="symbol">Symbol to subscribe, for example `ETH-USDT`</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        Task<CallResult<UpdateSubscription>> SubscribeToBookTickerUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamBestOffers>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to book ticker (best ask/bid) updates
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/level1-bbo-market-data" /></para>
        /// </summary>
        /// <param name="symbols">Symbols to subscribe, for example `ETH-USDT`</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        Task<CallResult<UpdateSubscription>> SubscribeToBookTickerUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamBestOffers>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to full order book updates
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/level2-5-best-ask-bid-orders" /></para>
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/level2-50-best-ask-bid-orders" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to subscribe, for example `ETH-USDT`</param>
        /// <param name="limit">The amount of levels to receive, either 5 or 50</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(string symbol, int limit,
            Action<DataEvent<GeminiStreamOrderBookChanged>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to full order book updates
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/level2-5-best-ask-bid-orders" /></para>
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/public-channels/level2-50-best-ask-bid-orders" /></para>
        /// </summary>
        /// <param name="symbols">The symbols to subscribe, for example `ETH-USDT`</param>
        /// <param name="limit">The amount of levels to receive, either 5 or 50</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(IEnumerable<string> symbols, int limit, Action<DataEvent<GeminiStreamOrderBookChanged>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to order updates for your own orders 
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/private-channels/private-order-change-v2" /></para>
        /// </summary>
        /// <param name="onNewOrder">Data handler for new order updates</param>
        /// <param name="onOrderData">Data handler for order updates</param>
        /// <param name="onTradeData">Data handler for trade updates</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(
            Action<DataEvent<GeminiStreamOrderNewUpdate>>? onNewOrder = null,
            Action<DataEvent<GeminiStreamOrderUpdate>>? onOrderData = null,
            Action<DataEvent<GeminiStreamOrderMatchUpdate>>? onTradeData = null,
            CancellationToken ct = default);

        /// <summary>
        /// Subscribe to balance updates
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/private-channels/account-balance-change" /></para>
        /// </summary>
        /// <param name="onBalanceChange">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToBalanceUpdatesAsync(Action<DataEvent<GeminiBalanceUpdate>> onBalanceChange, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to updates for stop orders
        /// <para><a href="https://www.gemini.com/docs/websocket/spot-trading/private-channels/stop-order-event" /></para>
        /// </summary>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToStopOrderUpdatesAsync(Action<DataEvent<GeminiStreamStopOrderUpdateBase>> onData, CancellationToken ct = default);

        /// <summary>
        /// DEPRECATED
        /// <para><a href="https://www.gemini.com/docs/websocket/margin-trading/public-channels/margin-funding-order-book-change" /></para>
        /// </summary>
        /// <param name="currency">Currencies to subscribe</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToFundingBookUpdatesAsync(string currency, Action<DataEvent<GeminiStreamFundingBookUpdate>> onData, CancellationToken ct = default);

        /// <summary>
        /// DEPRECATED
        /// <para><a href="https://www.gemini.com/docs/websocket/margin-trading/public-channels/margin-funding-order-book-change" /></para>
        /// </summary>
        /// <param name="currencies">Currencies to subscribe</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToFundingBookUpdatesAsync(IEnumerable<string> currencies, Action<DataEvent<GeminiStreamFundingBookUpdate>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to index price updates
        /// <para><a href="https://www.gemini.com/docs/websocket/margin-trading/public-channels/index-price" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to subscribe, for example `USDT-BTC`</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToIndexPriceUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamIndicatorPrice>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to index price updates
        /// <para><a href="https://www.gemini.com/docs/websocket/margin-trading/public-channels/index-price" /></para>
        /// </summary>
        /// <param name="symbols">The symbols to subscribe, for example `USDT-BTC`</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToIndexPriceUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamIndicatorPrice>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to mark price updates
        /// <para><a href="https://www.gemini.com/docs/websocket/margin-trading/public-channels/mark-price" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to subscribe, for example `USDT-BTC`</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        Task<CallResult<UpdateSubscription>> SubscribeToMarkPriceUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamIndicatorPrice>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to mark price updates
        /// <para><a href="https://www.gemini.com/docs/websocket/margin-trading/public-channels/mark-price" /></para>
        /// </summary>
        /// <param name="symbols">The symbols to subscribe, for example `USDT-BTC`</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        Task<CallResult<UpdateSubscription>> SubscribeToMarkPriceUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamIndicatorPrice>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to cross margin position events
        /// <para><a href="https://www.gemini.com/docs/websocket/margin-trading/private-channels/cross-margin-position-event" /></para>
        /// </summary>
        /// <param name="onDebtRatioChange">Data handler for debt ratio change evens. The system will push the current debt message periodically when there is a liability.</param>
        /// <param name="onPositionStatusChange">Data handler for position status change events. The system will push the change event when the position status changes.</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        Task<CallResult<UpdateSubscription>> SubscribeToMarginPositionUpdatesAsync(Action<DataEvent<GeminiMarginDebtRatioUpdate>> onDebtRatioChange, Action<DataEvent<GeminiMarginPositionStatusUpdate>> onPositionStatusChange, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to margin order updates for an asset
        /// <para><a href="https://www.gemini.com/docs/websocket/margin-trading/private-channels/margin-trade-order-event" /></para>
        /// </summary>
        /// <param name="symbol">Asset, for example `ETH-USDT`</param>
        /// <param name="onOrderPlaced">Data handler for order placement updates</param>
        /// <param name="onOrderUpdate">Data handler for order updates</param>
        /// <param name="onOrderDone">Data handler for order done updates</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        Task<CallResult<UpdateSubscription>> SubscribeToMarginOrderUpdatesAsync(string symbol, Action<DataEvent<GeminiMarginOrderUpdate>>? onOrderPlaced = null, Action<DataEvent<GeminiMarginOrderUpdate>>? onOrderUpdate = null, Action<DataEvent<GeminiMarginOrderDoneUpdate>>? onOrderDone = null, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to isolated margin order updates for a symbol
        /// <para><a href="https://www.gemini.com/docs/websocket/margin-trading/private-channels/margin-trade-order-event" /></para>
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="onPositionChange">Position change update handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        Task<CallResult<UpdateSubscription>> SubscribeToIsolatedMarginPositionUpdatesAsync(string symbol, Action<DataEvent<GeminiIsolatedMarginPositionUpdate>> onPositionChange, CancellationToken ct = default);
    }
}