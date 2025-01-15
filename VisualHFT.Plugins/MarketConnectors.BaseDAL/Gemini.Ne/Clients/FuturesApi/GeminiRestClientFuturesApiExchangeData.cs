using CryptoExchange.Net;
using CryptoExchange.Net.Converters;
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
using Gemini.Net.Objects.Models.Futures;
using Gemini.Net.Objects.Models.Spot;
using Gemini.Net.Interfaces.Clients.FuturesApi;
using System.Security.Cryptography;
using Gemini.Net.Objects.Models.Futures.Socket;

namespace Gemini.Net.Clients.FuturesApi
{
    /// <inheritdoc />
    internal class GeminiRestClientFuturesApiExchangeData : IGeminiRestClientFuturesApiExchangeData
    {
        private static readonly RequestDefinitionCache _definitions = new();
        private readonly GeminiRestClientFuturesApi _baseClient;

        internal GeminiRestClientFuturesApiExchangeData(GeminiRestClientFuturesApi baseClient)
        {
            _baseClient = baseClient;
        }

        #region Symbol

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiContract>>> GetOpenContractsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/contracts/active", GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<IEnumerable<GeminiContract>>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiContract>> GetContractAsync(string symbol, CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/contracts/" + symbol, GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<GeminiContract>(request, null, ct).ConfigureAwait(false);
        }

        #endregion

        #region Ticker

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiFuturesTick>> GetTickerAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/ticker", GeminiExchange.RateLimiter.PublicRest, 2);
            return await _baseClient.SendAsync<GeminiFuturesTick>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Tickers

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiFuturesTick>>> GetTickersAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/allTickers", GeminiExchange.RateLimiter.PublicRest, 15);
            return await _baseClient.SendAsync<IEnumerable<GeminiFuturesTick>>(request, null, ct).ConfigureAwait(false);
        }

        #endregion

        #region Order book

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiOrderBook>> GetAggregatedFullOrderBookAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/level2/snapshot", GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<GeminiOrderBook>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiOrderBook>> GetAggregatedPartialOrderBookAsync(string symbol, int depth, CancellationToken ct = default)
        {
            depth.ValidateIntValues(nameof(depth), 20, 100);

            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);

            var weight = depth == 20 ? 5 : 10;
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/level2/depth" + depth, GeminiExchange.RateLimiter.PublicRest, weight);
            return await _baseClient.SendAsync<GeminiOrderBook>(request, parameters, ct, weight).ConfigureAwait(false);
        }

        #endregion

        #region Trade history

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiFuturesTrade>>> GetTradeHistoryAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/trade/history", GeminiExchange.RateLimiter.PublicRest, 5);
            return await _baseClient.SendAsync<IEnumerable<GeminiFuturesTrade>>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Index

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginatedSlider<GeminiFuturesInterest>>> GetInterestRatesAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? offset = null, int? pageSize = null, bool? forward = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("offset", offset?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("maxCount", pageSize?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("forward", forward);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/interest/query", GeminiExchange.RateLimiter.PublicRest, 5);
            return await _baseClient.SendAsync<GeminiPaginatedSlider<GeminiFuturesInterest>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginatedSlider<GeminiIndex>>> GetIndexListAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? offset = null, int? pageSize = null, bool? forward = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("offset", offset?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("maxCount", pageSize?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("forward", forward);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/index/query", GeminiExchange.RateLimiter.PublicRest, 2);
            return await _baseClient.SendAsync<GeminiPaginatedSlider<GeminiIndex>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiMarkPrice>> GetCurrentMarkPriceAsync(string symbol, CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/mark-price/{symbol}/current", GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<GeminiMarkPrice>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginatedSlider<GeminiPremiumIndex>>> GetPremiumIndexAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? offset = null, int? pageSize = null, bool? forward = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToMilliseconds(endTime));
            parameters.AddOptionalParameter("offset", offset?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("maxCount", pageSize?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("forward", forward);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/premium/query", GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<GeminiPaginatedSlider<GeminiPremiumIndex>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiFundingRate>> GetCurrentFundingRateAsync(string symbol, CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/funding-rate/{symbol}/current", GeminiExchange.RateLimiter.PublicRest, 2);
            return await _baseClient.SendAsync<GeminiFundingRate>(request, null, ct).ConfigureAwait(false);
        }

        #endregion

        #region Time

        /// <inheritdoc />
        public async Task<WebCallResult<DateTime>> GetServerTimeAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/timestamp", GeminiExchange.RateLimiter.PublicRest, 2);
            var result = await _baseClient.SendAsync<long>(request, null, ct).ConfigureAwait(false);
            return result.As(result ? new DateTime(1970, 1, 1).AddMilliseconds(result.Data) : default);
        }

        #endregion

        #region Server status

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiFuturesServiceStatus>> GetServiceStatusAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/status", GeminiExchange.RateLimiter.PublicRest, 4);
            return await _baseClient.SendAsync<GeminiFuturesServiceStatus>(request, null, ct).ConfigureAwait(false);
        }

        #endregion

        #region Klines

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiFuturesKline>>> GetKlinesAsync(string symbol, FuturesKlineInterval interval, DateTime? startTime = null, DateTime? endTime = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            parameters.AddParameter("granularity", JsonConvert.SerializeObject(interval, new FuturesKlineIntervalConverter(false)));
            parameters.AddOptionalParameter("from", DateTimeConverter.ConvertToMilliseconds(startTime));
            parameters.AddOptionalParameter("to", DateTimeConverter.ConvertToMilliseconds(endTime));

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/kline/query", GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<IEnumerable<GeminiFuturesKline>>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get 24h Transaction Volume

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiTransactionVolume>> Get24HourTransactionVolumeAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/trade-statistics", GeminiExchange.RateLimiter.FuturesRest, 3, true);
            return await _baseClient.SendAsync<GeminiTransactionVolume>(request, null, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Funding Rate History

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiFundingRateHistory>>> GetFundingRateHistoryAsync(string symbol, DateTime startTime, DateTime endTime, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddParameter("symbol", symbol);
            parameters.AddMilliseconds("from", startTime);
            parameters.AddMilliseconds("to", endTime);

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/contract/funding-rates", GeminiExchange.RateLimiter.PublicRest, 5);
            var result = await _baseClient.SendAsync<IEnumerable<GeminiFundingRateHistory>>(request, parameters, ct).ConfigureAwait(false);
            if (result && result.Data == null)
                return result.As<IEnumerable<GeminiFundingRateHistory>>(Array.Empty<GeminiFundingRateHistory>());
            
            return result;
        }

        #endregion
    }
}
