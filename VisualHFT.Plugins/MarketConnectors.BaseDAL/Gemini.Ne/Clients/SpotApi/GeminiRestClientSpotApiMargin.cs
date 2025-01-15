using CryptoExchange.Net.Objects;
using Gemini.Net.Objects.Models.Spot;
using Gemini.Net.Enums;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Gemini.Net.Objects.Models;
using Gemini.Net.Interfaces.Clients.SpotApi;
using System.Collections.Generic;
using Gemini.Net.Objects.Models.Futures;

namespace Gemini.Net.Clients.SpotApi
{
    /// <inheritdoc />
    internal class GeminiRestClientSpotApiMargin : IGeminiRestClientSpotApiMargin
    {
        private readonly GeminiRestClientSpotApi _baseClient;
        private static readonly RequestDefinitionCache _definitions = new();

        internal GeminiRestClientSpotApiMargin(GeminiRestClientSpotApi baseClient)
        {
            _baseClient = baseClient;
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiIndexBase>> GetMarginMarkPriceAsync(string symbol, CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/mark-price/{symbol}/current", GeminiExchange.RateLimiter.PublicRest, 2);
            return await _baseClient.SendAsync<GeminiIndexBase>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiIndexBase>>> GetMarginMarkPricesAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/mark-price/all-symbols", GeminiExchange.RateLimiter.PublicRest, 10);
            return await _baseClient.SendAsync<IEnumerable<GeminiIndexBase>>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiMarginConfig>> GetMarginConfigurationAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/margin/config", GeminiExchange.RateLimiter.SpotRest, 25);
            return await _baseClient.SendAsync<GeminiMarginConfig>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiTradingPairConfiguration>>> GetMarginTradingPairConfigurationAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/isolated/symbols", GeminiExchange.RateLimiter.SpotRest, 20, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiTradingPairConfiguration>>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiCrossRiskLimitConfig>>> GetCrossMarginRiskLimitAndConfig(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "isIsolated", false }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/margin/currencies", GeminiExchange.RateLimiter.SpotRest, 20, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiCrossRiskLimitConfig>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiIsolatedRiskLimitConfig>>> GetIsolatedMarginRiskLimitAndConfig(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "isIsolated", true },
                { "symbol", symbol }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/margin/currencies", GeminiExchange.RateLimiter.SpotRest, 20, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiIsolatedRiskLimitConfig>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiNewBorrowOrder>> BorrowAsync(
            string asset,
            BorrowOrderType timeInForce,
            decimal quantity,
            bool? isIsolated = null,
            string? symbol = null,
            bool? isHf = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "currency", asset },
                { "size", quantity }
            };
            parameters.AddEnum("timeInForce", timeInForce);
            parameters.AddOptional("isIsolated", isIsolated);
            parameters.AddOptional("symbol", symbol);
            parameters.AddOptional("isHf", isHf);

            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v3/margin/borrow", GeminiExchange.RateLimiter.SpotRest, 15, true);
            return await _baseClient.SendAsync<GeminiNewBorrowOrder>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiNewBorrowOrder>> RepayAsync(
            string asset,
            decimal quantity,
            bool? isIsolated = null,
            string? symbol = null,
            bool? isHf = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "currency", asset },
                { "size", quantity }
            };
            parameters.AddOptional("isIsolated", isIsolated);
            parameters.AddOptional("symbol", symbol);
            parameters.AddOptional("isHf", isHf);

            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v3/margin/repay", GeminiExchange.RateLimiter.SpotRest, 10, true);
            return await _baseClient.SendAsync<GeminiNewBorrowOrder>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiBorrowOrderV3>>> GetBorrowHistoryAsync(string asset, bool? isIsolated = null, string? symbol = null, string? orderId = null, DateTime? startTime = null, DateTime? endTime = null, int? page = null, int? pageSize = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "currency", asset }
            };

            parameters.AddOptional("isIsolated", isIsolated);
            parameters.AddOptional("symbol", symbol);
            parameters.AddOptional("orderNo", orderId);
            parameters.AddOptionalMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);
            parameters.AddOptional("currentPage", page);
            parameters.AddOptional("pageSize", pageSize);

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/margin/borrow", GeminiExchange.RateLimiter.SpotRest, 15, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiBorrowOrderV3>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiBorrowOrderV3>>> GetRepayHistoryAsync(string asset, bool? isIsolated = null, string? symbol = null, string? orderId = null, DateTime? startTime = null, DateTime? endTime = null, int? page = null, int? pageSize = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "currency", asset }
            };

            parameters.AddOptional("isIsolated", isIsolated);
            parameters.AddOptional("symbol", symbol);
            parameters.AddOptional("orderNo", orderId);
            parameters.AddOptionalMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);
            parameters.AddOptional("currentPage", page);
            parameters.AddOptional("pageSize", pageSize);

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/margin/repay", GeminiExchange.RateLimiter.SpotRest, 15, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiBorrowOrderV3>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiMarginInterest>>> GetInterestHistoryAsync(string asset, bool? isIsolated = null, string? symbol = null, DateTime? startTime = null, DateTime? endTime = null, int? page = null, int? pageSize = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "currency", asset }
            };

            parameters.AddOptional("isIsolated", isIsolated);
            parameters.AddOptional("symbol", symbol);
            parameters.AddOptionalMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);
            parameters.AddOptional("currentPage", page);
            parameters.AddOptional("pageSize", pageSize);

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/margin/interest", GeminiExchange.RateLimiter.SpotRest, 20, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiMarginInterest>>(request, parameters, ct).ConfigureAwait(false);
        }


        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiLendingAsset>>> GetLendingAssetsAsync(string? asset = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("currency", asset);

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/project/list", GeminiExchange.RateLimiter.SpotRest, 10, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiLendingAsset>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiLendingInterest>>> GetInterestRatesAsync(string asset, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "currency", asset }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/project/marketInterestRate", GeminiExchange.RateLimiter.SpotRest, 5, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiLendingInterest>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiLendingResult>> SubscribeAsync(string asset, decimal quantity, decimal interestRate, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "currency", asset }
            };
            parameters.AddString("size", quantity);
            parameters.AddString("interestRate", interestRate);

            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v3/purchase", GeminiExchange.RateLimiter.SpotRest, 15, true);
            return await _baseClient.SendAsync<GeminiLendingResult>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiLendingResult>> RedeemAsync(string asset, decimal quantity, string subscribeOrderId, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "currency", asset },
                { "purchaseOrderNo", subscribeOrderId }
            };
            parameters.AddString("size", quantity);

            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v3/redeem", GeminiExchange.RateLimiter.SpotRest, 15, true);
            return await _baseClient.SendAsync<GeminiLendingResult>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult> EditSubscriptionOrderAsync(string asset, decimal interestRate, string subscribeOrderId, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "currency", asset },
                { "purchaseOrderNo", subscribeOrderId }
            };
            parameters.AddString("interestRate", interestRate);

            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v3/lend/purchase/update", GeminiExchange.RateLimiter.SpotRest, 15, true);
            return await _baseClient.SendAsync(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiRedemption>>> GetRedemptionOrdersAsync(string asset, string status, string? redeemOrderId = null, int? page = null, int? pageSize = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "currency", asset }
            };

            parameters.AddOptional("redeemOrderNo", redeemOrderId);
            parameters.AddOptional("status", status);
            parameters.AddOptional("currentPage", page);
            parameters.AddOptional("pageSize", pageSize);

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/redeem/orders", GeminiExchange.RateLimiter.SpotRest, 10, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiRedemption>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiLendSubscription>>> GetSubscriptionOrdersAsync(string asset, string status, string? purchaseOrderId = null, int? page = null, int? pageSize = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "currency", asset }
            };

            parameters.AddOptional("purchaseOrderNo", purchaseOrderId);
            parameters.AddOptional("status", status);
            parameters.AddOptional("currentPage", page);
            parameters.AddOptional("pageSize", pageSize);

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/purchase/orders", GeminiExchange.RateLimiter.SpotRest, 10, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiLendSubscription>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult> SetLeverageMultiplierAsync(decimal leverage, string? symbol = null, bool? isolatedMargin = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddString("leverage", leverage);
            parameters.AddOptional("symbol", symbol);
            parameters.AddOptional("isIsolated", isolatedMargin);

            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v3/position/update-user-leverage", GeminiExchange.RateLimiter.SpotRest, 5, true);
            return await _baseClient.SendAsync(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiCrossMarginSymbol>>> GetCrossMarginSymbolsAsync(string? symbol = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("symbol", symbol);

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/margin/symbols", GeminiExchange.RateLimiter.SpotRest, 5, true);
            var result = await _baseClient.SendAsync<GeminiCrossMarginSymbols>(request, parameters, ct).ConfigureAwait(false);
            return result.As<IEnumerable<GeminiCrossMarginSymbol>>(result.Data?.Items);
        }

    }
}
