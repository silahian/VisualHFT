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
using Gemini.Net.Objects.Internal;
using Gemini.Net.Objects.Models;
using Gemini.Net.Objects.Models.Futures;
using CryptoExchange.Net.Converters;
using Gemini.Net.Interfaces.Clients.FuturesApi;
using Gemini.Net.Objects.Models.Spot;

namespace Gemini.Net.Clients.FuturesApi
{
    /// <inheritdoc />
    internal class GeminiRestClientFuturesApiAccount : IGeminiRestClientFuturesApiAccount
    {
        private static readonly RequestDefinitionCache _definitions = new();
        private readonly GeminiRestClientFuturesApi _baseClient;

        internal GeminiRestClientFuturesApiAccount(GeminiRestClientFuturesApi baseClient)
        {
            _baseClient = baseClient;
        }

        #region Account
        /// <inheritdoc />
        public async Task<WebCallResult<GeminiAccountOverview>> GetAccountOverviewAsync(string? asset = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", asset);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/account-overview", GeminiExchange.RateLimiter.FuturesRest, 5, true);
            return await _baseClient.SendAsync<GeminiAccountOverview>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginatedSlider<GeminiAccountTransaction>>> GetTransactionHistoryAsync(string? asset = null, TransactionType? type = null, DateTime? startTime = null, DateTime? endTime = null, int? offset = null, int? pageSize = null, bool? forward = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", asset);
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("type", type == null ? null : JsonConvert.SerializeObject(type, new TransactionTypeConverter(false)));
            parameters.AddOptionalParameter("offset", offset?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("maxCount", pageSize?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("forward", forward);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/transaction-history", GeminiExchange.RateLimiter.ManagementRest, 2, true);
            return await _baseClient.SendAsync<GeminiPaginatedSlider<GeminiAccountTransaction>>(request, parameters, ct).ConfigureAwait(false);
        }
        #endregion

        #region Transfer
        /// <inheritdoc />
        public async Task<WebCallResult<GeminiTransferResult>> TransferToMainAccountAsync(string asset, decimal quantity, AccountType receiveAccountType, CancellationToken ct = default)
        {
            if (receiveAccountType != AccountType.Main && receiveAccountType != AccountType.Trade)
                throw new ArgumentException("Receiving account type should be Main or Trade");

            var parameters = new ParameterCollection();
            parameters.AddParameter("currency", asset);
            parameters.AddParameter("amount", quantity.ToString(CultureInfo.InvariantCulture));
            parameters.AddParameter("recAccountType", EnumConverter.GetString(receiveAccountType));
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v3/transfer-out", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            return await _baseClient.SendAsync<GeminiTransferResult>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult> TransferToFuturesAccountAsync(string asset, decimal quantity, AccountType payAccountType, CancellationToken ct = default)
        {
            if (payAccountType != AccountType.Main && payAccountType != AccountType.Trade)
                throw new ArgumentException("Receiving account type should be Main or Trade");

            var parameters = new ParameterCollection();
            parameters.AddParameter("currency", asset);
            parameters.AddParameter("amount", quantity.ToString(CultureInfo.InvariantCulture));
            parameters.AddParameter("payAccountType", EnumConverter.GetString(payAccountType));
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/transfer-in", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            return await _baseClient.SendAsync(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiTransfer>>> GetTransferToMainAccountHistoryAsync(string? asset = null, DateTime? startTime = null, DateTime? endTime = null, DepositStatus? status = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", asset);
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("currentPage", currentPage);
            parameters.AddOptionalParameter("pageSize", pageSize);
            parameters.AddOptionalParameter("status", EnumConverter.GetString(status));
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/transfer-list", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiTransfer>>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Positions

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPosition>> GetPositionAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/position", GeminiExchange.RateLimiter.FuturesRest, 2, true);
            return await _baseClient.SendAsync<GeminiPosition>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiPosition>>> GetPositionsAsync(string? asset = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", asset);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/positions", GeminiExchange.RateLimiter.FuturesRest, 2, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiPosition>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiPositionHistoryItem>>> GetPositionHistoryAsync(string? symbol = null, DateTime? startTime = null, DateTime? endTime = null, int? limit = null, int? page = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("symbol", symbol);
            parameters.AddOptionalMilliseconds("from", startTime);
            parameters.AddOptionalMilliseconds("to", endTime);
            parameters.AddOptional("limit", limit);
            parameters.AddOptional("pageId", page);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/history-positions", GeminiExchange.RateLimiter.FuturesRest, 2, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiPositionHistoryItem>>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        /// <inheritdoc />
        public async Task<WebCallResult> ToggleAutoDepositMarginAsync(string symbol, bool enabled, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            parameters.AddParameter("status", enabled.ToString());
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/position/margin/auto-deposit-status", GeminiExchange.RateLimiter.FuturesRest, 4, true);
            return await _baseClient.SendAsync(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult> AddMarginAsync(string symbol, decimal quantity, string? clientId = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            parameters.AddParameter("bizNo", clientId ?? Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
            parameters.AddParameter("margin", quantity.ToString(CultureInfo.InvariantCulture));
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/position/margin/deposit-margin", GeminiExchange.RateLimiter.FuturesRest, 4, true);
            return await _baseClient.SendAsync(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult> RemoveMarginAsync(string symbol, decimal quantity, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            parameters.AddParameter("withdrawAmount", quantity.ToString(CultureInfo.InvariantCulture));
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/margin/withdrawMargin", GeminiExchange.RateLimiter.FuturesRest, 10, true);
            return await _baseClient.SendAsync(request, parameters, ct).ConfigureAwait(false);
        }


        #region Funding fees

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginatedSlider<GeminiFundingItem>>> GetFundingHistoryAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? offset = null, int? pageSize = null, bool? forward = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("offset", offset?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("maxCount", pageSize?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("forward", forward);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/funding-history", GeminiExchange.RateLimiter.FuturesRest, 5, true);
            return await _baseClient.SendAsync<GeminiPaginatedSlider<GeminiFundingItem>>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Open order value
        /// <inheritdoc />
        public async Task<WebCallResult<GeminiOrderValuation>> GetOpenOrderValueAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/openOrderStatistics", GeminiExchange.RateLimiter.FuturesRest, 10, true);
            return await _baseClient.SendAsync<GeminiOrderValuation>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Risk Limit Level
        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<Objects.Models.Futures.GeminiRiskLimit>>> GetRiskLimitLevelAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/contracts/risk-limit/" + symbol, GeminiExchange.RateLimiter.FuturesRest, 5, true);
            return await _baseClient.SendAsync<IEnumerable<Objects.Models.Futures.GeminiRiskLimit>>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Set Risk Limit Level
        /// <inheritdoc />
        public async Task<WebCallResult<bool>> SetRiskLimitLevelAsync(string symbol, int level, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            parameters.AddParameter("level", level);
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/position/risk-limit-level/change", GeminiExchange.RateLimiter.FuturesRest, 4, true);
            return await _baseClient.SendAsync<bool>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Max Withdraw Margin
        /// <inheritdoc />
        public async Task<WebCallResult<decimal>> GetMaxWithdrawMarginAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/margin/maxWithdrawMargin", GeminiExchange.RateLimiter.FuturesRest, 10, true);
            return await _baseClient.SendAsync<decimal>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Trading Fee
        /// <inheritdoc />
        public async Task<WebCallResult<GeminiTradeFee>> GetTradingFeeAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/trade-fees", GeminiExchange.RateLimiter.FuturesRest, 3, true);
            return await _baseClient.SendAsync<GeminiTradeFee>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Margin Mode

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiMarginMode>> GetMarginModeAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/api/v2/position/getMarginMode", GeminiExchange.RateLimiter.FuturesRest, 2, true);
            var result = await _baseClient.SendAsync<GeminiMarginMode>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Set Margin Mode

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiMarginMode>> SetMarginModeAsync(string symbol, FuturesMarginMode marginMode, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            parameters.AddEnum("marginMode", marginMode);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "/api/v2/position/changeMarginMode", GeminiExchange.RateLimiter.FuturesRest, 2, true);
            var result = await _baseClient.SendAsync<GeminiMarginMode>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Get Cross Margin Leverage

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiLeverage>> GetCrossMarginLeverageAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/api/v2/getCrossUserLeverage", GeminiExchange.RateLimiter.FuturesRest, 2, true);
            var result = await _baseClient.SendAsync<GeminiLeverage>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Set Cross Margin Leverage

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiLeverage>> SetCrossMarginLeverageAsync(string symbol, decimal leverage, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            parameters.Add("leverage", leverage);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "/api/v2/changeCrossUserLeverage", GeminiExchange.RateLimiter.FuturesRest, 2, true);
            var result = await _baseClient.SendAsync<GeminiLeverage>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Websocket token

        internal async Task<WebCallResult<GeminiToken>> GetWebsocketTokenPublicAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/bullet-public", GeminiExchange.RateLimiter.PublicRest, 10, false);
            return await _baseClient.SendAsync<GeminiToken>(request, null, ct).ConfigureAwait(false);
        }

        internal async Task<WebCallResult<GeminiToken>> GetWebsocketTokenPrivateAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/bullet-private", GeminiExchange.RateLimiter.FuturesRest, 10, true);
            return await _baseClient.SendAsync<GeminiToken>(request, null, ct).ConfigureAwait(false);
        }

        #endregion
    }
}
