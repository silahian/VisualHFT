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
    /// Gemini Futures account endpoints. Account endpoints include balance info, withdraw/deposit info and requesting and account settings
    /// </summary>
    public interface IGeminiRestClientFuturesApiAccount
    {
        /// <summary>
        /// Gets account overview
        /// <para><a href="https://www.gemini.com/docs/rest/funding/funding-overview/get-account-detail-futures" /></para>
        /// </summary>
        /// <param name="asset">Get the accounts for a specific asset, for example `USDT`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of accounts</returns>
        Task<WebCallResult<GeminiAccountOverview>> GetAccountOverviewAsync(string? asset = null, CancellationToken ct = default);

        /// <summary>
        /// Get transaction history
        /// <para><a href="https://www.gemini.com/docs/rest/account/basic-info/get-account-ledgers-futures" /></para>
        /// </summary>
        /// <param name="asset">Filter by asset, for example `USDT`</param>
        /// <param name="type">Filter by type</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="offset">Result offset</param>
        /// <param name="pageSize">Size of a page</param>
        /// <param name="forward">Forward or backwards direction</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginatedSlider<GeminiAccountTransaction>>> GetTransactionHistoryAsync(string? asset = null, TransactionType? type = null, DateTime? startTime = null, DateTime? endTime = null, int? offset = null, int? pageSize = null, bool? forward = null, CancellationToken ct = default);

        /// <summary>
        /// Transfer funds from futures to main account
        /// <para><a href="https://www.gemini.com/docs/rest/funding/transfer/transfer-to-main-or-trade-account" /></para>
        /// </summary>
        /// <param name="asset">Asset to transfer, for example `USDT`</param>
        /// <param name="quantity">Quantity to transfer</param>
        /// <param name="receiveAccountType">Receiving account type</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Transfer id</returns>
        Task<WebCallResult<GeminiTransferResult>> TransferToMainAccountAsync(string asset, decimal quantity, AccountType receiveAccountType, CancellationToken ct = default);

        /// <summary>
        /// Transfer funds from main or trade account to futures
        /// <para><a href="https://www.gemini.com/docs/rest/funding/transfer/transfer-to-futures-account" /></para>
        /// </summary>
        /// <param name="asset">Asset to transfer, for example `USDT`</param>
        /// <param name="quantity">Quantity to transfer</param>
        /// <param name="payAccountType">Account to move funds from</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult> TransferToFuturesAccountAsync(string asset, decimal quantity, AccountType payAccountType, CancellationToken ct = default);

        /// <summary>
        /// Get transfer to main account history
        /// <para><a href="https://www.gemini.com/docs/rest/funding/transfer/get-futures-transfer-out-request-records" /></para>
        /// </summary>
        /// <param name="asset">Filter by asset, for example `USDT`</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="status">Filter by status</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="pageSize">Size of a page</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginated<GeminiTransfer>>> GetTransferToMainAccountHistoryAsync(string? asset = null, DateTime? startTime = null, DateTime? endTime = null, DepositStatus? status = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get the total value of active orders
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/fills/get-active-order-value-calculation" /></para>
        /// </summary>        
        /// <param name="symbol">Symbol, for example `XBTUSDM`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiOrderValuation>> GetOpenOrderValueAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Get details on a position
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/get-position-details" /></para>
        /// </summary>
        /// <param name="symbol">Contract symbol, for example `XBTUSDM`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Position info</returns>
        Task<WebCallResult<GeminiPosition>> GetPositionAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Get list of positions
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/get-position-list" /></para>
        /// </summary>
        /// <param name="asset">Filter by asset, for example `USDT`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Position info</returns>
        Task<WebCallResult<IEnumerable<GeminiPosition>>> GetPositionsAsync(string? asset = null, CancellationToken ct = default);

        /// <summary>
        /// Get position closure history
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/get-positions-history" /></para>
        /// </summary>
        /// <param name="symbol">Filter by symbol, for example `XBTUSDM`</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="page">Page number</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginated<GeminiPositionHistoryItem>>> GetPositionHistoryAsync(string? symbol = null, DateTime? startTime = null, DateTime? endTime = null, int? limit = null, int? page = null, CancellationToken ct = default);

        /// <summary>
        /// Enable/disable auto deposit margin
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/modify-auto-deposit-margin-status" /></para>
        /// </summary>
        /// <param name="symbol">Symbol to change for, for example `XBTUSDM`</param>
        /// <param name="enabled">Enable or disable</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Position info</returns>
        Task<WebCallResult> ToggleAutoDepositMarginAsync(string symbol, bool enabled, CancellationToken ct = default);

        /// <summary>
        /// Manually add margin to a position
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/add-margin-manually" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `XBTUSDM`</param>
        /// <param name="quantity">Quantity to add</param>
        /// <param name="clientId">A unique ID generated by the user, to ensure the operation is processed by the system only once</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult> AddMarginAsync(string symbol, decimal quantity, string? clientId = null, CancellationToken ct = default);

        /// <summary>
        /// Manually remove margin from a position
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/remove-margin-manually" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `XBTUSDM`</param>
        /// <param name="quantity">Quantity to remove</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult> RemoveMarginAsync(string symbol, decimal quantity, CancellationToken ct = default);

        /// <summary>
        /// Get funding history
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/funding-fees/get-private-funding-history" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `XBTUSDM`</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="offset">Result offset</param>
        /// <param name="pageSize">Size of a page</param>
        /// <param name="forward">Forward or backwards direction</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginatedSlider<GeminiFundingItem>>> GetFundingHistoryAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? offset = null, int? pageSize = null, bool? forward = null, CancellationToken ct = default);

        /// <summary>
        /// Get risk limit level
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/risk-limit/get-futures-risk-limit-level" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `XBTUSDM`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Objects.Models.Futures.GeminiRiskLimit>>> GetRiskLimitLevelAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Set risk limit level
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/risk-limit/modify-risk-limit-level" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example `XBTUSDM`</param>
        /// <param name="level">Risk limit level</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<bool>> SetRiskLimitLevelAsync(string symbol, int level, CancellationToken ct = default);

        /// <summary>
        /// Get the maximum amount of margin that the current position supports withdrawal.
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/get-max-withdraw-margin" /></para>
        /// </summary>
        /// <param name="symbol">The symbol, for example `XBTUSDM`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<decimal>> GetMaxWithdrawMarginAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Get trading fee for a symbol
        /// <para><a href="https://www.gemini.com/docs/rest/funding/trade-fee/trading-pair-actual-fee-futures" /></para>
        /// </summary>
        /// <param name="symbol">The symbol, for example `XBTUSDM`</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiTradeFee>> GetTradingFeeAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Get the current margin mode for a symbol
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/get-margin-mode" /></para>
        /// </summary>
        /// <param name="symbol">The symbol, for example `XBTUSDTM`</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<GeminiMarginMode>> GetMarginModeAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Set the margin mode for a symbol
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/modify-margin-mode" /></para>
        /// </summary>
        /// <param name="symbol">The symbol, for example `XBTUSDTM`</param>
        /// <param name="marginMode">The new margin mode</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<GeminiMarginMode>> SetMarginModeAsync(string symbol, FuturesMarginMode marginMode, CancellationToken ct = default);

        /// <summary>
        /// Get the current cross margin leverage setting
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/get-cross-margin-leverage" /></para>
        /// </summary>
        /// <param name="symbol">The symbol, for example `XBTUSDTM`</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<GeminiLeverage>> GetCrossMarginLeverageAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Set a new cross margin leverage value
        /// <para><a href="https://www.gemini.com/docs/rest/futures-trading/positions/modify-cross-margin-leverage" /></para>
        /// </summary>
        /// <param name="symbol">The symbol, for example `XBTUSDTM`</param>
        /// <param name="leverage">The leverage</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<GeminiLeverage>> SetCrossMarginLeverageAsync(string symbol, decimal leverage, CancellationToken ct = default);

    }
}
