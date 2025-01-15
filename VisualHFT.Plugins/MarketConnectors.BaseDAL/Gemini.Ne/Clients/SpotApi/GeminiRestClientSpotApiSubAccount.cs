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
    internal class GeminiRestClientSpotApiSubAccount : IGeminiRestClientSpotApiSubAccount
    {
        private static readonly RequestDefinitionCache _definitions = new();
        private readonly GeminiRestClientSpotApi _baseClient;

        internal GeminiRestClientSpotApiSubAccount(GeminiRestClientSpotApi baseClient)
        {
            _baseClient = baseClient;
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiSubUser>>> GetSubAccountsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"api/v2/sub/user", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiSubUser>>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiSubUser>> CreateSubAccountAsync(string subName, string password, string permissions, string? remarks = null, CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"api/v2/sub/user/created", GeminiExchange.RateLimiter.ManagementRest, 15, true);
            return await _baseClient.SendAsync<GeminiSubUser>(request, null, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiSubUserBalances>> GetSubAccountBalancesAsync(string subAccountId, bool? includeZeroBalances = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("includeBaseAmount", includeZeroBalances);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"/api/v1/sub-accounts/{subAccountId}", GeminiExchange.RateLimiter.ManagementRest, 15, true);
            return await _baseClient.SendAsync<GeminiSubUserBalances>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiPaginated<GeminiSubUserBalances>>> GetSubAccountsBalancesAsync(int? page = null, int? pageSize = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("currentPage", page);
            parameters.AddOptional("pageSize", pageSize);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"/api/v2/sub-accounts", GeminiExchange.RateLimiter.ManagementRest, 15, true);
            return await _baseClient.SendAsync<GeminiPaginated<GeminiSubUserBalances>>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<GeminiSubUserKey>>> GetSubAccountApiKeyAsync(string subAccountName, string? apiKey = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("subName", subAccountName);
            parameters.AddOptional("apiKey", apiKey);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"/api/v1/sub/api-key", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            var result = await _baseClient.SendRawAsync<GeminiResult<IEnumerable<GeminiSubUserKey>>>(request, parameters, ct).ConfigureAwait(false);

            if (!result)
                return result.AsError<IEnumerable<GeminiSubUserKey>>(result.Error!);

            if (result.Data.Code != 200000 && result.Data.Code != 200)
                return result.AsError<IEnumerable<GeminiSubUserKey>>(new ServerError(result.Data.Code, result.Data.Message ?? "-"));

            if (!string.IsNullOrEmpty(result.Data.Message))
                return result.AsError<IEnumerable<GeminiSubUserKey>>(new ServerError(result.Data.Message!));

            return result.As(result.Data.Data);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiSubUserKeyDetails>> CreateSubAccountApiKeyAsync(
            string subAccountName, 
            string passphrase, 
            string remark, 
            string? permissions = null, 
            string? ipWhitelist = null,
            string? expire = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("subName", subAccountName);
            parameters.Add("passphrase", passphrase);
            parameters.Add("remark", remark);
            parameters.AddOptional("permissions", permissions);
            parameters.AddOptional("ipWhitelist", ipWhitelist);
            parameters.AddOptional("expire", expire);
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"/api/v1/sub/api-key", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            var result = await _baseClient.SendRawAsync<GeminiResult<GeminiSubUserKeyDetails>>(request, parameters, ct).ConfigureAwait(false);

            if(!result)
                return result.AsError<GeminiSubUserKeyDetails>(result.Error!);

            if (result.Data.Code != 200000 && result.Data.Code != 200)
                return result.AsError<GeminiSubUserKeyDetails>(new ServerError(result.Data.Code, result.Data.Message ?? "-"));

            if (!string.IsNullOrEmpty(result.Data.Message))
                return result.AsError<GeminiSubUserKeyDetails>(new ServerError(result.Data.Message!));

            return result.As(result.Data.Data);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiSubUserKeyEdited>> EditSubAccountApiKeyAsync(
            string subAccountName,
            string apiKey,
            string passphrase,
            string? permissions = null,
            string? ipWhitelist = null,
            string? expire = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("subName", subAccountName);
            parameters.Add("passphrase", passphrase);
            parameters.Add("apiKey", apiKey);
            parameters.AddOptional("permissions", permissions);
            parameters.AddOptional("ipWhitelist", ipWhitelist);
            parameters.AddOptional("expire", expire);
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"/api/v1/sub/api-key/update", GeminiExchange.RateLimiter.ManagementRest, 20, true);
            var result = await _baseClient.SendRawAsync<GeminiResult<GeminiSubUserKeyEdited>>(request, parameters, ct).ConfigureAwait(false);

            if (!result)
                return result.AsError<GeminiSubUserKeyEdited>(result.Error!);

            if (result.Data.Code != 200000 && result.Data.Code != 200)
                return result.AsError<GeminiSubUserKeyEdited>(new ServerError(result.Data.Code, result.Data.Message ?? "-"));

            if (!string.IsNullOrEmpty(result.Data.Message))
                return result.AsError<GeminiSubUserKeyEdited>(new ServerError(result.Data.Message!));

            return result.As(result.Data.Data);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<GeminiSubUserKeyEdited>> DeleteSubAccountApiKeyAsync(
            string subAccountName,
            string apiKey,
            string passphrase,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("subName", subAccountName);
            parameters.Add("passphrase", passphrase);
            parameters.Add("apiKey", apiKey);
            var request = _definitions.GetOrCreate(HttpMethod.Delete, $"/api/v1/sub/api-key", GeminiExchange.RateLimiter.ManagementRest, 30, true);
            var result = await _baseClient.SendRawAsync<GeminiResult<GeminiSubUserKeyEdited>>(request, parameters, ct).ConfigureAwait(false);

            if (!result)
                return result.AsError<GeminiSubUserKeyEdited>(result.Error!);

            if (result.Data.Code != 200000 && result.Data.Code != 200)
                return result.AsError<GeminiSubUserKeyEdited>(new ServerError(result.Data.Code, result.Data.Message ?? "-"));

            if (!string.IsNullOrEmpty(result.Data.Message))
                return result.AsError<GeminiSubUserKeyEdited>(new ServerError(result.Data.Message!));

            return result.As(result.Data.Data);
        }
    }
}