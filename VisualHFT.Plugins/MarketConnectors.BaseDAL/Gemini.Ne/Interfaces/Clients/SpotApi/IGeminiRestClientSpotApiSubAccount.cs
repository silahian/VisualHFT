using CryptoExchange.Net.Objects;
using Gemini.Net.Enums;
using Gemini.Net.Objects.Models;
using Gemini.Net.Objects.Models.Spot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gemini.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// Gemini Spot sub-account endpoints
    /// </summary>
    public interface IGeminiRestClientSpotApiSubAccount
    {
        /// <summary>
        /// Gets a list of sub account
        /// <para><a href="https://www.gemini.com/docs/rest/account/sub-account/get-all-sub-accounts-info-v2-" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<GeminiPaginated<GeminiSubUser>>> GetSubAccountsAsync(CancellationToken ct = default);

        /// <summary>
        /// Create a new sub account
        /// <para><a href="https://www.gemini.com/docs/rest/account/sub-account/create-sub-account" /></para>
        /// </summary>
        /// <param name="subName">Sub account name</param>
        /// <param name="password">Password (7-24 characters, must contain letters and numbers, cannot only contain numbers or include special characters)</param>
        /// <param name="permissions">Permission (types include Spot, Futures, Margin permissions, which can be used alone or in combination).</param>
        /// <param name="remarks">Remarks(1~24 characters)</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<GeminiSubUser>> CreateSubAccountAsync(string subName, string password, string permissions, string? remarks = null, CancellationToken ct = default);

        /// <summary>
        /// Get balances of a sub account
        /// <para><a href="https://www.gemini.com/docs/rest/account/sub-account/get-a-sub-account-balance" /></para>
        /// </summary>
        /// <param name="subAccountId">Sub account id</param>
        /// <param name="includeZeroBalances">Include zero balance assets or not</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiSubUserBalances>> GetSubAccountBalancesAsync(string subAccountId, bool? includeZeroBalances = null, CancellationToken ct = default);

        /// <summary>
        /// Get balances of all sub accounts
        /// <para><a href="https://www.gemini.com/docs/rest/account/sub-account/get-all-sub-accounts-balance-v2-" /></para>
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">page size, max 100</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiPaginated<GeminiSubUserBalances>>> GetSubAccountsBalancesAsync(int? page = null, int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get info on sub account api keys
        /// <para><a href="https://www.gemini.com/docs/rest/account/sub-account-api/get-sub-account-api-list" /></para>
        /// </summary>
        /// <param name="subAccountName">The sub account name</param>
        /// <param name="apiKey">Filter by API key</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<GeminiSubUserKey>>> GetSubAccountApiKeyAsync(string subAccountName, string? apiKey = null, CancellationToken ct = default);

        /// <summary>
        /// Create a new API key for a sub account
        /// <para><a href="https://www.gemini.com/docs/rest/account/sub-account-api/get-sub-account-api-list" /></para>
        /// </summary>
        /// <param name="subAccountName">Sub account name</param>
        /// <param name="passphrase">Password/passphrase for the key</param>
        /// <param name="remark">Remark</param>
        /// <param name="permissions">Permissions(Only General、Spot、Futures、Margin、InnerTransfer(Flex Transfer) permissions can be set, such as "General, Trade". The default is "General")</param>
        /// <param name="ipWhitelist">IP whitelist(You may add up to 20 IPs. Use a halfwidth comma to each IP)</param>
        /// <param name="expire">Expiration time in days; Never expire(default) -1，30: 30, 90, 180 or 360 days</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiSubUserKeyDetails>> CreateSubAccountApiKeyAsync(
            string subAccountName,
            string passphrase,
            string remark,
            string? permissions = null,
            string? ipWhitelist = null,
            string? expire = null,
            CancellationToken ct = default);

        /// <summary>
        /// Edit an existing API key for a sub account
        /// <para><a href="https://www.gemini.com/docs/rest/account/sub-account-api/modify-sub-account-api" /></para>
        /// </summary>
        /// <param name="subAccountName">Sub account name</param>
        /// <param name="passphrase">Password/passphrase for the key</param>
        /// <param name="apiKey">The api key</param>
        /// <param name="permissions">New permissions</param>
        /// <param name="ipWhitelist">New IP whitelist</param>
        /// <param name="expire">New expire time in days</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiSubUserKeyEdited>> EditSubAccountApiKeyAsync(
            string subAccountName,
            string apiKey,
            string passphrase,
            string? permissions = null,
            string? ipWhitelist = null,
            string? expire = null,
            CancellationToken ct = default);

        /// <summary>
        /// Remove an API key for a sub account
        /// <para><a href="https://www.gemini.com/docs/rest/account/sub-account-api/delete-sub-account-api" /></para>
        /// </summary>
        /// <param name="subAccountName">Sub account name</param>
        /// <param name="passphrase">Password/passphrase for the key</param>
        /// <param name="apiKey">The api key</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<WebCallResult<GeminiSubUserKeyEdited>> DeleteSubAccountApiKeyAsync(
            string subAccountName,
            string apiKey,
            string passphrase,
            CancellationToken ct = default);
    }
}