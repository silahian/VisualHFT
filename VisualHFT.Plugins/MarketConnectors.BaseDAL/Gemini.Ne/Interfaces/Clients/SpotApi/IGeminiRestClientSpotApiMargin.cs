using CryptoExchange.Net.Objects;
using Gemini.Net.Enums;
using Gemini.Net.Objects.Models;
using Gemini.Net.Objects.Models.Futures;
using Gemini.Net.Objects.Models.Spot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gemini.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// Margin borrow and repay endpoints
    /// </summary>
    public interface IGeminiRestClientSpotApiMargin
    {
        /// <summary>
        /// Get margin configuration
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-info/get-margin-configuration-info" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiMarginConfig>> GetMarginConfigurationAsync(CancellationToken ct = default);

        /// <summary>
        /// Get the mark price of a symbol
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-info/get-mark-price" /></para>
        /// </summary>
        /// <param name="symbol">The symbol to retrieve, for example `USDT-BTC`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiIndexBase>> GetMarginMarkPriceAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Get the mark price for all symbols
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-info/get-all-margin-trading-pairs-mark-prices" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiIndexBase>>> GetMarginMarkPricesAsync(CancellationToken ct = default);

        /// <summary>
        /// Get Margin Trading Pair ConfigurationAsync
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/isolated-margin/get-isolated-margin-symbols-configuration" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param> 
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiTradingPairConfiguration>>> GetMarginTradingPairConfigurationAsync(CancellationToken ct = default);

        /// <summary>
        /// Get cross margin risk limit and asset configuration info
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-info/get-cross-isolated-margin-risk-limit-currency-config" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param> 
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiCrossRiskLimitConfig>>> GetCrossMarginRiskLimitAndConfig(CancellationToken ct = default);

        /// <summary>
        /// Get isolated margin risk limit and asset configuration info
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-info/get-cross-isolated-margin-risk-limit-currency-config" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `ETH-USDT`</param>
        /// <param name="ct">Cancellation token</param> 
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiIsolatedRiskLimitConfig>>> GetIsolatedMarginRiskLimitAndConfig(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Borrow an asset
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-trading-v3-/margin-borrowing" /></para>
        /// </summary>
        /// <param name="asset">Currency to Borrow e.g USDT etc</param>
        /// <param name="timeInForce">Time in force (FOK, IOC)</param>
        /// <param name="quantity">Total size</param>
        /// <param name="isIsolated">If isolated margin</param>
        /// <param name="symbol">Isolated margin symbol</param>
        /// <param name="isHf">HighFrequency/ProAccount borrowing</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The id of the new order</returns>
        Task<WebCallResult<GeminiNewBorrowOrder>> BorrowAsync(
            string asset,
            BorrowOrderType timeInForce,
            decimal quantity,
            bool? isIsolated = null,
            string? symbol = null,
            bool? isHf = null,
            CancellationToken ct = default);

        /// <summary>
        /// Repayment for previously borrowed asset
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-trading-v3-/repayment" /></para>
        /// </summary>
        /// <param name="asset">Currency to Repay e.g USDT etc</param>
        /// <param name="quantity">Total size</param>
        /// <param name="isIsolated">If isolated margin</param>
        /// <param name="symbol">Isolated margin symbol</param>
        /// <param name="isHf">HighFrequency/ProAccount repayment</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiNewBorrowOrder>> RepayAsync(
            string asset,
            decimal quantity,
            bool? isIsolated = null,
            string? symbol = null,
            bool? isHf = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get borrow history
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-trading-v3-/get-margin-borrowing-history" /></para>
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <param name="isIsolated">Filter by is isolated margin</param>
        /// <param name="symbol">Filter by isolated margin symbol, for example `ETH-USDT`</param>
        /// <param name="orderId">Filter by borrow order number</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="page">The page to retrieve</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginated<GeminiBorrowOrderV3>>> GetBorrowHistoryAsync(string asset, bool? isIsolated = null, string? symbol = null, string? orderId = null, DateTime? startTime = null, DateTime? endTime = null, int? page = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get repayment history
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-trading-v3-/get-margin-borrowing-history" /></para>
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <param name="isIsolated">Filter by is isolated margin</param>
        /// <param name="symbol">Filter by isolated margin symbol, for example `ETH-USDT`</param>
        /// <param name="orderId">Filter by repay order number</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="page">The page to retrieve</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginated<GeminiBorrowOrderV3>>> GetRepayHistoryAsync(string asset, bool? isIsolated = null, string? symbol = null, string? orderId = null, DateTime? startTime = null, DateTime? endTime = null, int? page = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get margin interest records
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-trading-v3-/get-cross-isolated-margin-interest-records" /></para>
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <param name="isIsolated">Filter by is isolated margin</param>
        /// <param name="symbol">Filter by isolated margin symbol, for example `ETH-USDT`</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="page">The page to retrieve</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginated<GeminiMarginInterest>>> GetInterestHistoryAsync(string asset, bool? isIsolated = null, string? symbol = null, DateTime? startTime = null, DateTime? endTime = null, int? page = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get lending asset info
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/lending-market-v3-/get-currency-information" /></para>
        /// </summary>
        /// <param name="asset">Filter by asset</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiLendingAsset>>> GetLendingAssetsAsync(string? asset = null, CancellationToken ct = default);

        /// <summary>
        /// Get lending interest rates
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/lending-market-v3-/get-interest-rates" /></para>
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiLendingInterest>>> GetInterestRatesAsync(string asset, CancellationToken ct = default);

        /// <summary>
        /// Initiate subscriptions of margin lending
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/lending-market-v3-/subscription" /></para>
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="interestRate">Interest rate</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiLendingResult>> SubscribeAsync(string asset, decimal quantity, decimal interestRate, CancellationToken ct = default);

        /// <summary>
        /// Initiate redemptions of margin lending.
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/lending-market-v3-/redemption" /></para>
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="subscribeOrderId">Subscribe order number</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiLendingResult>> RedeemAsync(string asset, decimal quantity, string subscribeOrderId, CancellationToken ct = default);

        /// <summary>
        /// Update interest rate of a subscription order
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/lending-market-v3-/modify-subscription-orders" /></para>
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <param name="interestRate">New interest rate</param>
        /// <param name="subscribeOrderId">Subscribe order number</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult> EditSubscriptionOrderAsync(string asset, decimal interestRate, string subscribeOrderId, CancellationToken ct = default);

        /// <summary>
        /// Get redemption orders
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/lending-market-v3-/get-redemption-orders" /></para>
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <param name="redeemOrderId">Filter by redeem order id</param>
        /// <param name="status">Status, DONE or PENDING</param>
        /// <param name="page">Page</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginated<GeminiRedemption>>> GetRedemptionOrdersAsync(string asset, string status, string? redeemOrderId = null, int? page = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get subscription orders
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/lending-market-v3-/get-subscription-orders" /></para>
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <param name="status">Status, DONE or PENDING</param>
        /// <param name="purchaseOrderId">Filter by purchase order id</param>
        /// <param name="page">Page</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginated<GeminiLendSubscription>>> GetSubscriptionOrdersAsync(string asset, string status, string? purchaseOrderId = null, int? page = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Modify the leverage multiplier for cross margin or isolated margin.
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-trading-v3-/modify-leverage-multiplier" /></para>
        /// </summary>
        /// <param name="leverage">New leverage multiplier. Must be greater than 1 and up to two decimal places, and cannot be less than the user's current debt leverage or greater than the system's maximum leverage</param>
        /// <param name="symbol">Symbol. Leave empty for cross margin, or specify for isolated margin, for example `ETH-USDT`</param>
        /// <param name="isolatedMargin">Is isolated margin</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult> SetLeverageMultiplierAsync(decimal leverage, string? symbol = null, bool? isolatedMargin = null, CancellationToken ct = default);

        /// <summary>
        /// Get cross margin symbols
        /// <para><a href="https://www.gemini.com/docs/rest/margin-trading/margin-trading-v3-/get-cross-margin-trading-pairs-configuration" /></para>
        /// </summary>
        /// <param name="symbol">Filter by symbol, for example `ETH-USDT`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiCrossMarginSymbol>>> GetCrossMarginSymbolsAsync(string? symbol = null, CancellationToken ct = default);
    }
}