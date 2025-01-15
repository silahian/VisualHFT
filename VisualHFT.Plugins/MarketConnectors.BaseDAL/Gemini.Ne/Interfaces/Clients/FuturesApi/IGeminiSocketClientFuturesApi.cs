using CryptoExchange.Net.Objects;
using System;
using System.Threading.Tasks;
using System.Threading;
using Gemini.Net.Enums;
using Gemini.Net.Objects.Models.Futures;
using Gemini.Net.Objects.Models.Futures.Socket;
using Gemini.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Interfaces;
using System.Collections.Generic;
using CryptoExchange.Net.Objects.Sockets;

namespace Gemini.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// Futures socket api
    /// </summary>
    public interface IGeminiSocketClientFuturesApi : ISocketApiClient, IDisposable
    {
        /// <summary>
        /// Get the shared socket subscription client. This interface is shared with other exhanges to allow for a common implementation for different exchanges.
        /// </summary>
        IGeminiSocketClientFuturesApiShared SharedClient { get; }

        /// <summary>
        /// Subscribe to trade updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/match-execution-data" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to subscribe on, for example `XBTUSDTM`</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamFuturesMatch>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to trade updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/match-execution-data" /></para>
        /// </summary>
        /// <param name="symbols">The symbols to subscribe on, for example `XBTUSDTM`</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamFuturesMatch>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to kline updates
        /// </summary>
        /// <param name="symbol">Symbol to subscribe, for example 'XBTUSDTM'</param>
        /// <param name="interval">Kline interval</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<DataEvent<GeminiStreamFuturesKline>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to kline updates
        /// </summary>
        /// <param name="symbols">Symbols to subscribe, for example 'XBTUSDTM'</param>
        /// <param name="interval">Kline interval</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(IEnumerable<string> symbols, KlineInterval interval, Action<DataEvent<GeminiStreamFuturesKline>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to ticker updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/get-ticker-v2" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to subscribe on, for example `XBTUSDTM`</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamFuturesTick>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to ticker updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/get-ticker-v2" /></para>
        /// </summary>
        /// <param name="symbols">The symbol to subscribe on, for example `XBTUSDTM`</param>
        /// <param name="onData">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamFuturesTick>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to full order book updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/level2-market-data" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to subscribe, for example `XBTUSDTM`</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(string symbol, Action<DataEvent<GeminiFuturesOrderBookChange>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to full order book updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/level2-market-data" /></para>
        /// </summary>
        /// <param name="symbols">The symbols to subscribe, for example `XBTUSDTM`</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiFuturesOrderBookChange>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to partial order book updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/level2-5-best-ask-bid-orders" /></para>
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/level2-50-best-ask-bid-orders" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to subscribe, for example `XBTUSDTM`</param>
        /// <param name="limit">The amount of levels to receive, either 5 or 50</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToPartialOrderBookUpdatesAsync(string symbol, int limit, Action<DataEvent<GeminiStreamOrderBookChanged>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to partial order book updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/level2-5-best-ask-bid-orders" /></para>
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/level2-50-best-ask-bid-orders" /></para>
        /// </summary>
        /// <param name="symbols">The symbols to subscribe, for example `XBTUSDTM`</param>
        /// <param name="limit">The amount of levels to receive, either 5 or 50</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToPartialOrderBookUpdatesAsync(IEnumerable<string> symbols, int limit, Action<DataEvent<GeminiStreamOrderBookChanged>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to market data updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/contract-market-data" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to subscribe, for example `XBTUSDTM`</param>
        /// <param name="onMarkIndexPriceUpdate">Mark/Index price update handler</param>
        /// <param name="onFundingRateUpdate">Funding price update handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToMarketUpdatesAsync(string symbol,
            Action<DataEvent<GeminiStreamFuturesMarkIndexPrice>> onMarkIndexPriceUpdate,
            Action<DataEvent<GeminiStreamFuturesFundingRate>> onFundingRateUpdate,
            CancellationToken ct = default);

        /// <summary>
        /// Subscribe to market data updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/contract-market-data" /></para>
        /// </summary>
        /// <param name="symbols">The symbols to subscribe, for example `XBTUSDTM`</param>
        /// <param name="onMarkIndexPriceUpdate">Mark/Index price update handler</param>
        /// <param name="onFundingRateUpdate">Funding price update handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToMarketUpdatesAsync(IEnumerable<string> symbols,
            Action<DataEvent<GeminiStreamFuturesMarkIndexPrice>> onMarkIndexPriceUpdate,
            Action<DataEvent<GeminiStreamFuturesFundingRate>> onFundingRateUpdate,
            CancellationToken ct = default);

        /// <summary>
        /// Subscribe system announcement
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/funding-fee-settlement" /></para>
        /// </summary>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToSystemAnnouncementsAsync(Action<DataEvent<GeminiContractAnnouncement>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to snapshot updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/transaction-statistics-timer-event" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `XBTUSDTM`</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeTo24HourSnapshotUpdatesAsync(string symbol, Action<DataEvent<GeminiStreamTransactionStatisticsUpdate>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to snapshot updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/public-channels/transaction-statistics-timer-event" /></para>
        /// </summary>
        /// <param name="symbols">Symbol, for example `XBTUSDTM`</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeTo24HourSnapshotUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<GeminiStreamTransactionStatisticsUpdate>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to wallet updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/private-channels/account-balance-events" /></para>
        /// </summary>
        /// <param name="onOrderMarginUpdate">DEPRECATED; After the user 【First time】switches the margin mode (switching from isolated margin to cross margin), this will stop pushing and instead the onWalletUpdate event will be pushed</param>
        /// <param name="onBalanceUpdate">DEPRECATED; After the user 【First time】switches the margin mode (switching from isolated margin to cross margin), this will stop pushing and instead the onWalletUpdate event will be pushed</param>
        /// <param name="onWithdrawableUpdate">DEPRECATED; After the user 【First time】switches the margin mode (switching from isolated margin to cross margin), this will stop pushing and instead the onWalletUpdate event will be pushed</param>
        /// <param name="onWalletUpdate">Data handler for wallet update. Will be pushed once user switches margin modes for the first time</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToBalanceUpdatesAsync(
            Action<DataEvent<GeminiStreamOrderMarginUpdate>> onOrderMarginUpdate,
            Action<DataEvent<GeminiStreamFuturesBalanceUpdate>> onBalanceUpdate,
            Action<DataEvent<GeminiStreamFuturesWithdrawableUpdate>> onWithdrawableUpdate,
            Action<DataEvent<GeminiStreamFuturesWalletUpdate>> onWalletUpdate,
            CancellationToken ct = default);

        /// <summary>
        /// Subscribe to position updates for a specific symbol
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/private-channels/position-change-events" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `XBTUSDTM`</param>
        /// <param name="onPositionUpdate">Handler for position changes</param>
        /// <param name="onMarkPriceUpdate">Handler for update when position change due to mark price changes</param>
        /// <param name="onFundingSettlementUpdate">Handler for funding settlement updates</param>
        /// <param name="onRiskAdjustUpdate">Handler for risk adjust updates</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToPositionUpdatesAsync(
            string symbol,
            Action<DataEvent<GeminiPositionUpdate>> onPositionUpdate,
            Action<DataEvent<GeminiPositionMarkPriceUpdate>> onMarkPriceUpdate,
            Action<DataEvent<GeminiPositionFundingSettlementUpdate>> onFundingSettlementUpdate,
            Action<DataEvent<GeminiPositionRiskAdjustResultUpdate>> onRiskAdjustUpdate,
            CancellationToken ct = default);

        /// <summary>
        /// Subscribe to position updates. Note that this overrides any symbol specific position subscriptions
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/private-channels/all-position-change-events" /></para>
        /// </summary>
        /// <param name="onPositionUpdate">Handler for position changes</param>
        /// <param name="onMarkPriceUpdate">Handler for update when position change due to mark price changes</param>
        /// <param name="onFundingSettlementUpdate">Handler for funding settlement updates</param>
        /// <param name="onRiskAdjustUpdate">Handler for risk adjust updates</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToPositionUpdatesAsync(
            Action<DataEvent<GeminiPositionUpdate>>? onPositionUpdate = null,
            Action<DataEvent<GeminiPositionMarkPriceUpdate>>? onMarkPriceUpdate = null,
            Action<DataEvent<GeminiPositionFundingSettlementUpdate>>? onFundingSettlementUpdate = null,
            Action<DataEvent<GeminiPositionRiskAdjustResultUpdate>>? onRiskAdjustUpdate = null,
            CancellationToken ct = default);

        /// <summary>
        /// Subscribe to margin mode updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/private-channels/margin-mode-push" /></para>
        /// </summary>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        Task<CallResult<UpdateSubscription>> SubscribeToMarginModeUpdatesAsync(Action<DataEvent<Dictionary<string, FuturesMarginMode>>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to cross margin leverage updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/private-channels/cross-margin-mode-leverage-modification-push" /></para>
        /// </summary>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        Task<CallResult<UpdateSubscription>> SubscribeToCrossMarginLeverageUpdatesAsync(Action<DataEvent<Dictionary<string, GeminiLeverageUpdate>>> onData, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to order updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/private-channels/trade-orders" /></para>
        /// </summary>
        /// <param name="symbol">[Optional] Symbol, for example `XBTUSDTM`</param>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(string? symbol,
            Action<DataEvent<GeminiStreamFuturesOrderUpdate>> onData,
            CancellationToken ct = default);

        /// <summary>
        /// Subscribe to stop order updates
        /// <para><a href="https://www.gemini.com/docs/websocket/futures-trading/private-channels/stop-order-lifecycle-event" /></para>
        /// </summary>
        /// <param name="onData">Data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToStopOrderUpdatesAsync(Action<DataEvent<GeminiStreamFuturesStopOrderUpdate>> onData, CancellationToken ct = default);
    }
}
