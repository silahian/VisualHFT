using CryptoExchange.Net;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Objects;
using Gemini.Net.Converters;
using Gemini.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gemini.Net.Objects.Models.Spot;
using Gemini.Net.Interfaces.Clients.SpotApi;
using Gemini.Net.Objects.Models.Futures;
using Gemini.Net.Objects;
using Gemini.Net.ExtensionMethods;
using System.Security.Cryptography;
using Gemini.Net.Objects.Models;

namespace Gemini.Net.Clients.SpotApi
{
    /// <inheritdoc />
    internal class GeminiRestClientSpotApiExchangeData : IGeminiRestClientSpotApiExchangeData
    {
        private readonly GeminiRestClientSpotApi _baseClient;
        private static readonly RequestDefinitionCache _definitions = new();

        internal GeminiRestClientSpotApiExchangeData(GeminiRestClientSpotApi baseClient)
        {
            _baseClient = baseClient;
        }

        /// <inheritdoc />
        public async Task<WebCallResult<DateTime>> GetServerTimeAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/timestamp", GeminiExchange.RateLimiter.PublicRest, 3);
            var result = await _baseClient.SendAsync<long>(request, null, ct).ConfigureAwait(false);
            return result.As(result ? JsonConvert.DeserializeObject<DateTime>(result.Data.ToString(), new DateTimeConverter()) : default);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiSymbol>>> GetSymbolsAsync(string? market = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("market", market);
            // Testnet doesn't support V2
            var apiVersion = _baseClient.BaseAddress == GeminiApiAddresses.TestNet.SpotAddress ? 1 : 2;
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v{apiVersion}/symbols", GeminiExchange.RateLimiter.PublicRest, 4);
            return await _baseClient.SendAsync<IEnumerable<GeminiSymbol>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiSymbol>> GetSymbolAsync(string symbol, CancellationToken ct = default)
        {
            // Testnet doesn't support V2
            var apiVersion = _baseClient.BaseAddress == GeminiApiAddresses.TestNet.SpotAddress ? 1 : 2;
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v{apiVersion}/symbols/{symbol}", GeminiExchange.RateLimiter.PublicRest, 4);
            return await _baseClient.SendAsync<GeminiSymbol>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiTick>> GetTickerAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection { { "symbol", symbol } };
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/market/orderbook/level1", GeminiExchange.RateLimiter.PublicRest, 2);
            var result = await _baseClient.SendAsync<GeminiTick>(request, parameters, ct).ConfigureAwait(false);
            if (result && result.Data == null)
                return result.AsError<GeminiTick>(new ServerError("Symbol doesn't exist"));
            return result;
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiTicks>> GetTickersAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/market/allTickers", GeminiExchange.RateLimiter.PublicRest, 15);
            return await _baseClient.SendAsync<GeminiTicks>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<Gemini24HourStat>> Get24HourStatsAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection { { "symbol", symbol } };
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/market/stats", GeminiExchange.RateLimiter.PublicRest, 15);
            var result = await _baseClient.SendAsync<Gemini24HourStat>(request, parameters, ct).ConfigureAwait(false);
            if (result && result.Data.Volume == null)
                return result.AsError<Gemini24HourStat>(new ServerError("Symbol doesn't exist"));
            return result;
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<string>>> GetMarketsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/markets", GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<IEnumerable<string>>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiOrderBook>> GetAggregatedPartialOrderBookAsync(string symbol, int limit, CancellationToken ct = default)
        {
            limit.ValidateIntValues(nameof(limit), 20, 100);
            var parameters = new ParameterCollection
            {
                { "symbol", symbol }
            };

            var weight = limit == 20 ? 2 : 4;
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/market/orderbook/level2_{limit}", GeminiExchange.RateLimiter.PublicRest, weight);
            var result = await _baseClient.SendAsync<GeminiOrderBook>(request, parameters, ct, weight).ConfigureAwait(false);
            if (result && result.Data.Asks == null)
                return result.AsError<GeminiOrderBook>(new ServerError("Symbol doesn't exist"));
            return result;

        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiOrderBook>> GetAggregatedFullOrderBookAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/market/orderbook/level2", GeminiExchange.RateLimiter.SpotRest, 3, true);
            var result = await _baseClient.SendAsync<GeminiOrderBook>(request, parameters, ct).ConfigureAwait(false);
            if (result && result.Data.Asks == null)
                return result.AsError<GeminiOrderBook>(new ServerError("Symbol doesn't exist"));
            return result;
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiTrade>>> GetTradeHistoryAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/market/histories", GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<IEnumerable<GeminiTrade>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiKline>>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime? startTime = null, DateTime? endTime = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol },
                { "type", JsonConvert.SerializeObject(interval, new KlineIntervalConverter(false)) }
            };
            parameters.AddOptionalParameter("startAt", DateTimeConverter.ConvertToSeconds(startTime));
            parameters.AddOptionalParameter("endAt", DateTimeConverter.ConvertToSeconds(endTime));

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/market/candles", GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<IEnumerable<GeminiKline>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiAssetDetails>>> GetAssetsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/currencies", GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<IEnumerable<GeminiAssetDetails>>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiAssetDetails>> GetAssetAsync(string asset, CancellationToken ct = default)
        {
            asset.ValidateNotNull(nameof(asset));
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/currencies/{asset}", GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<GeminiAssetDetails>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<Dictionary<string, decimal>>> GetFiatPricesAsync(string? fiatBase = null, IEnumerable<string>? assets = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("base", fiatBase);
            parameters.AddOptionalParameter("currencies", assets?.Any() == true ? string.Join(",", assets) : null);

            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v1/prices", GeminiExchange.RateLimiter.PublicRest, 3);
            return await _baseClient.SendAsync<Dictionary<string, decimal>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiLeveragedToken>>> GetLeveragedTokensAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v3/etf/info", GeminiExchange.RateLimiter.SpotRest, 25, true);
            return await _baseClient.SendAsync<IEnumerable<GeminiLeveragedToken>>(request, null, ct).ConfigureAwait(false);
        }

        #region Get Announcements

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiAnnouncement>>> GetAnnouncementsAsync(int? page = null, int? pageSize = null, string? announcementType = null, string? language = null, DateTime? startTime = null, DateTime? endTime = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("currentPage", page);
            parameters.AddOptional("pageSize", pageSize);
            parameters.AddOptional("annType", announcementType);
            parameters.AddOptional("lang", language);
            parameters.AddOptionalMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/api/v3/announcements", GeminiExchange.RateLimiter.PublicRest, 1, false);
            var result = await _baseClient.SendAsync<GeminiPaginated<GeminiAnnouncement>>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        #endregion

    }
}
