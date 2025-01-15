using CryptoExchange.Net;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Objects;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Gemini.Net.Interfaces.Clients.SpotApi;
using Gemini.Net.Objects.Internal;
using Gemini.Net.Objects.Models;
using Gemini.Net.Objects.Models.Spot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Gemini.Net.Clients.SpotApi
{
    /// <inheritdoc />
    internal class GeminiRestClientSpotApiAccount : IGeminiRestClientSpotApiAccount
    {
        private static readonly RequestDefinitionCache _definitions = new();
        private readonly GeminiRestClientSpotApi _baseClient;

        internal GeminiRestClientSpotApiAccount(GeminiRestClientSpotApi baseClient)
        {
            _baseClient = baseClient;
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiUserInfo>> GetUserInfoAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v2/user-info", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            return await _baseClient.SendAsync<GeminiUserInfo>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiAccount>>> GetAccountsAsync(string? asset = null, AccountType? accountType = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", asset);
            parameters.AddOptionalParameter("type", accountType.HasValue ? JsonConvert.SerializeObject(accountType, new AccountTypeConverter(false)) : null);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/accounts", GeminiExchange.RateLimiter.ManagementRest, 5, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiAccount>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiAccountSingle>> GetAccountAsync(string accountId, CancellationToken ct = default)
        {
            accountId.ValidateNotNull(nameof(accountId));
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/accounts/" + accountId, GeminiExchange.RateLimiter.ManagementRest, 5, true);
            return await _baseClient.SendAsync<GeminiAccountSingle>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiUserFee>> GetBasicUserFeeAsync(AssetType? assetType = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currencyType", EnumConverter.GetString(assetType));

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/base-fee", GeminiExchange.RateLimiter.SpotRest, 3, true);
            return await _baseClient.SendAsync<GeminiUserFee>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiTradeFee>>> GetSymbolTradingFeesAsync(string symbol, CancellationToken ct = default)
            => await GetSymbolTradingFeesAsync(new[] { symbol }, ct).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiTradeFee>>> GetSymbolTradingFeesAsync(IEnumerable<string> symbols, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbols",  string.Join(",", symbols) }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/trade-fees", GeminiExchange.RateLimiter.SpotRest, 3, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiTradeFee>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiAccountActivity>>> GetAccountLedgersAsync(string? asset = null, AccountDirection? direction = null, BizType? bizType = null, DateTime? startTime = null, DateTime? endTime = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default)
        {
            pageSize?.ValidateIntBetween(nameof(pageSize), 10, 500);

            string bizTypeString = string.Empty;
            string directionString = string.Empty;

            if (bizType.HasValue)
            {
                bizTypeString = JsonConvert.SerializeObject(bizType, new BizTypeConverter(true));
                bizTypeString.ValidateNullOrNotEmpty(nameof(bizType));
            }

            if (direction.HasValue)
            {
                directionString = JsonConvert.SerializeObject(direction, new AccountDirectionConverter(false)).ToLowerInvariant();
            }

            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", asset);
            parameters.AddOptionalParameter("direction", direction.HasValue ? directionString : null);
            parameters.AddOptionalParameter("bizType", bizType.HasValue ? bizTypeString : null);
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("currentPage", currentPage);
            parameters.AddOptionalParameter("pageSize", pageSize);

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/accounts/ledgers", GeminiExchange.RateLimiter.ManagementRest, 2, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiAccountActivity>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiTransferableAccount>> GetTransferableAsync(string asset, AccountType accountType, string? isolatedMarginSymbol = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "currency", asset },
                { "type", JsonConvert.SerializeObject(accountType, new AccountTypeConverter(false, true))}
            };
            parameters.AddOptional("tag", isolatedMarginSymbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/accounts/transferable", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            return await _baseClient.SendAsync<GeminiTransferableAccount>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiUniversalTransfer>> UniversalTransferAsync(
            decimal quantity,
            TransferAccountType fromAccountType,
            TransferAccountType toAccountType,
            TransferType transferType,
            string? asset = null,
            string? fromUserId = null,
            string? fromAccountTag = null,
            string? toUserId = null,
            string? toAccountTag = null,
            string? clientOrderId = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "fromAccountType", EnumConverter.GetString(fromAccountType) },
                { "toAccountType", EnumConverter.GetString(toAccountType)},
                { "type", EnumConverter.GetString(transferType)},
                { "amount", quantity },
                { "clientOid", clientOrderId ?? Guid.NewGuid().ToString()},
            };
            parameters.AddOptionalParameter("currency", asset);
            parameters.AddOptionalParameter("fromUserId", fromUserId);
            parameters.AddOptionalParameter("fromAccountTag", fromAccountTag);
            parameters.AddOptionalParameter("toUserId", toUserId);
            parameters.AddOptionalParameter("toAccountTag", toAccountTag);

            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v3/accounts/universal-transfer", GeminiExchange.RateLimiter.ManagementRest, 4, true);
            return await _baseClient.SendAsync<GeminiUniversalTransfer>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiInnerTransfer>> InnerTransferAsync(string asset, AccountType from, AccountType to, decimal quantity, string? fromTag = null, string? toTag = null, string? clientOrderId = null, CancellationToken ct = default)
        {
            asset.ValidateNotNull(nameof(asset));
            var parameters = new ParameterCollection
            {
                { "currency", asset },
                { "from", JsonConvert.SerializeObject(from, new AccountTypeConverter(false))},
                { "to", JsonConvert.SerializeObject(to, new AccountTypeConverter(false))},
                { "amount", quantity },
                { "clientOid", clientOrderId ?? Guid.NewGuid().ToString()},
            };
            parameters.AddOptionalParameter("fromTag", fromTag);
            parameters.AddOptionalParameter("toTag", toTag);

            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v2/accounts/inner-transfer", GeminiExchange.RateLimiter.ManagementRest, 10, true);
            return await _baseClient.SendAsync<GeminiInnerTransfer>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiDeposit>>> GetDepositsAsync(string? asset = null, DateTime? startTime = null, DateTime? endTime = null, DepositStatus? status = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default)
        {
            pageSize?.ValidateIntBetween(nameof(pageSize), 10, 500);

            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", asset);
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("status", status.HasValue ? JsonConvert.SerializeObject(status, new DepositStatusConverter(false)) : null);
            parameters.AddOptionalParameter("currentPage", currentPage);
            parameters.AddOptionalParameter("pageSize", pageSize);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/deposits", GeminiExchange.RateLimiter.ManagementRest, 5, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiDeposit>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiHistoricalDeposit>>> GetHistoricalDepositsAsync(string? asset = null, DateTime? startTime = null, DateTime? endTime = null, DepositStatus? status = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default)
        {
            pageSize?.ValidateIntBetween(nameof(pageSize), 10, 500);

            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", asset);
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("status", status.HasValue ? JsonConvert.SerializeObject(status, new DepositStatusConverter(false)) : null);
            parameters.AddOptionalParameter("currentPage", currentPage);
            parameters.AddOptionalParameter("pageSize", pageSize);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/hist-deposits", GeminiExchange.RateLimiter.ManagementRest, 5, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiHistoricalDeposit>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiDepositAddress>> GetDepositAddressAsync(string asset, string? network = null, CancellationToken ct = default)
        {
            asset.ValidateNotNull(nameof(asset));
            var parameters = new ParameterCollection { { "currency", asset } };
            parameters.AddOptionalParameter("chain", network);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/deposit-addresses", GeminiExchange.RateLimiter.ManagementRest, 5, true);
            return await _baseClient.SendAsync<GeminiDepositAddress>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiDepositAddress>>> GetDepositAddressesAsync(string asset, CancellationToken ct = default)
        {
            asset.ValidateNotNull(nameof(asset));
            var parameters = new ParameterCollection { { "currency", asset } };
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v2/deposit-addresses", GeminiExchange.RateLimiter.ManagementRest, 5, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiDepositAddress>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiDepositAddress>> CreateDepositAddressAsync(string asset, string? network = null, CancellationToken ct = default)
        {
            asset.ValidateNotNull(nameof(asset));
            var parameters = new ParameterCollection { { "currency", asset } };
            parameters.AddOptionalParameter("chain", network);
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/deposit-addresses", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            return await _baseClient.SendAsync<GeminiDepositAddress>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiWithdrawal>>> GetWithdrawalsAsync(string? asset = null, DateTime? startTime = null, DateTime? endTime = null, WithdrawalStatus? status = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default)
        {
            pageSize?.ValidateIntBetween(nameof(pageSize), 10, 500);

            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", asset);
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("status", status.HasValue ? JsonConvert.SerializeObject(status, new WithdrawalStatusConverter(false)) : null);
            parameters.AddOptionalParameter("currentPage", currentPage);
            parameters.AddOptionalParameter("pageSize", pageSize);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/withdrawals", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiWithdrawal>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiHistoricalWithdrawal>>> GetHistoricalWithdrawalsAsync(string? asset = null, DateTime? startTime = null, DateTime? endTime = null, WithdrawalStatus? status = null, int? currentPage = null, int? pageSize = null, CancellationToken ct = default)
        {
            pageSize?.ValidateIntBetween(nameof(pageSize), 10, 500);

            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", asset);
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("status", status.HasValue ? JsonConvert.SerializeObject(status, new WithdrawalStatusConverter(false)) : null);
            parameters.AddOptionalParameter("currentPage", currentPage);
            parameters.AddOptionalParameter("pageSize", pageSize);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/hist-withdrawals", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiHistoricalWithdrawal>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiWithdrawalQuota>> GetWithdrawalQuotasAsync(string asset, string? network = null, CancellationToken ct = default)
        {
            asset.ValidateNotNull(nameof(asset));

            var parameters = new ParameterCollection { { "currency", asset } };
            parameters.AddOptionalParameter("chain", network);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/withdrawals/quotas", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            return await _baseClient.SendAsync<GeminiWithdrawalQuota>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiNewWithdrawal>> WithdrawAsync(WithdrawType withdrawalType, string asset, string toAddress, decimal quantity, string? memo = null, bool isInner = false, string? remark = null, string? network = null, FeeDeductType? deductType = null, CancellationToken ct = default)
        {
            asset.ValidateNotNull(nameof(asset));
            toAddress.ValidateNotNull(nameof(toAddress));
            var parameters = new ParameterCollection {
                { "currency", asset },
                { "toAddress", toAddress },
                { "amount", quantity },
            };
            parameters.AddEnum("withdrawType", withdrawalType);
            parameters.AddOptionalParameter("memo", memo);
            parameters.AddOptionalParameter("isInner", isInner);
            parameters.AddOptionalParameter("remark", remark);
            parameters.AddOptionalParameter("chain", network);
            parameters.AddOptionalParameter("feeDeductType", EnumConverter.GetString(deductType));
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v3/withdrawals", GeminiExchange.RateLimiter.ManagementRest, 5, true);
            return await _baseClient.SendAsync<GeminiNewWithdrawal>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult> CancelWithdrawalAsync(string withdrawalId, CancellationToken ct = default)
        {
            withdrawalId.ValidateNotNull(nameof(withdrawalId));
            var request = _definitions.GetOrCreate(HttpMethod.Delete, $"api/v1/withdrawals/{withdrawalId}", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            return await _baseClient.SendAsync(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiMarginAccount>> GetMarginAccountAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/margin/account", GeminiExchange.RateLimiter.SpotRest, 40, true);
            return await _baseClient.SendAsync<GeminiMarginAccount>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiCrossMarginAccount>> GetCrossMarginAccountsAsync(string? quoteAsset = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("quoteCurrency", quoteAsset);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/margin/accounts", GeminiExchange.RateLimiter.SpotRest, 15, true);
            return await _baseClient.SendAsync<GeminiCrossMarginAccount>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiIsolatedMarginAccountsInfo>> GetIsolatedMarginAccountsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/isolated/accounts", GeminiExchange.RateLimiter.SpotRest, 50, true);
            return await _baseClient.SendAsync<GeminiIsolatedMarginAccountsInfo>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiIsolatedMarginAccount>> GetIsolatedMarginAccountAsync(string symbol, CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/isolated/account/{symbol}", GeminiExchange.RateLimiter.SpotRest, 50, true);
            return await _baseClient.SendAsync<GeminiIsolatedMarginAccount>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiMigrateStatus>> GetHfMigrationStatusAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "withAllSubs", true }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/migrate/user/account/status", GeminiExchange.RateLimiter.SpotRest, 10, true);
            return await _baseClient.SendAsync<GeminiMigrateStatus>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiMigrateResult>> MigrateHfAccountAsync(bool? withAllSubAccounts = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("withAllSubs", withAllSubAccounts);
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v3/migrate/user/account", GeminiExchange.RateLimiter.SpotRest, 10, true);
            return await _baseClient.SendAsync<GeminiMigrateResult>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<bool>> GetIsHfAccountAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/hf/accounts/opened", GeminiExchange.RateLimiter.SpotRest, 10, true);
            return await _baseClient.SendAsync<bool>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiApiKey>> GetApiKeyInfoAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/user/api-key", GeminiExchange.RateLimiter.SpotRest, 10, true);
            return await _baseClient.SendAsync<GeminiApiKey>(request, parameters, ct).ConfigureAwait(false);
        }

        internal async Task<WebCallResult<GeminiToken>> GetWebsocketTokenPublicAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/bullet-public", GeminiExchange.RateLimiter.PublicRest, 10, false);
            return await _baseClient.SendAsync<GeminiToken>(request, null, ct).ConfigureAwait(false);
        }

        internal async Task<WebCallResult<GeminiToken>> GetWebsocketTokenPrivateAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v1/bullet-private", GeminiExchange.RateLimiter.SpotRest, 10, true);
            return await _baseClient.SendAsync<GeminiToken>(request, null, ct).ConfigureAwait(false);
        }
    }
}