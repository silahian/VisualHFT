using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Gemini.Net.Clients;
using Gemini.Net.Interfaces.Clients;
using Gemini.Net.Objects;
using Gemini.Net.Objects.Models.Spot.Socket;
using Gemini.Net.Objects.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gemini.Net.SymbolOrderBooks
{
    /// <summary>
    /// Gemini order book implementation
    /// </summary>
    public class GeminiSpotSymbolOrderBook: CryptoExchange.Net.OrderBook.SymbolOrderBook
    {
        private readonly IGeminiRestClient _restClient;
        private readonly IGeminiSocketClient _socketClient;
        private readonly TimeSpan _initialDataTimeout;
        private readonly bool _clientOwner;

        /// <summary>
        /// Create a new order book instance
        /// </summary>
        /// <param name="symbol">The symbol the order book is for</param>
        /// <param name="optionsFunc">The options for the order book</param>
        public GeminiSpotSymbolOrderBook(string symbol, Action<GeminiOrderBookOptions>? optionsFunc = null)
            : this(symbol, optionsFunc, null, null, null)
        {
            _clientOwner = true;
        }

        /// <summary>
        /// Create a new order book instance
        /// </summary>
        /// <param name="symbol">The symbol the order book is for</param>
        /// <param name="optionsFunc">The options for the order book</param>
        /// <param name="logger">Logger</param>
        /// <param name="restClient">Rest client instance</param>
        /// <param name="socketClient">Socket client instance</param>
        [ActivatorUtilitiesConstructor]
        public GeminiSpotSymbolOrderBook(
            string symbol,
            Action<GeminiOrderBookOptions>? optionsFunc,
            ILoggerFactory? logger,
            IGeminiRestClient? restClient,
            IGeminiSocketClient? socketClient) : base(logger, "Gemini", "Spot", symbol)
        {
            var options = GeminiOrderBookOptions.Default.Copy();
            if (optionsFunc != null)
                optionsFunc(options);
            Initialize(options);

            _strictLevels = false;
            _sequencesAreConsecutive = options.Limit == null;

            Levels = options.Limit;
            _initialDataTimeout = options.InitialDataTimeout ?? TimeSpan.FromSeconds(30);
            _clientOwner = socketClient == null;
            _socketClient = socketClient ?? new GeminiSocketClient(x =>
            {
                x.ApiCredentials = (GeminiApiCredentials?)options.ApiCredentials?.Copy() ?? (GeminiApiCredentials?)GeminiSocketOptions.Default.ApiCredentials?.Copy();
            });
            _restClient = restClient ?? new GeminiRestClient(x =>
            {
                x.ApiCredentials = (GeminiApiCredentials?)options.ApiCredentials?.Copy() ?? (GeminiApiCredentials?)GeminiRestOptions.Default.ApiCredentials?.Copy();
            });
        }

        /// <inheritdoc />
        protected override async Task<CallResult<UpdateSubscription>> DoStartAsync(CancellationToken ct)
        {
            CallResult<UpdateSubscription> subResult;
            if (Levels == null)
            {
                subResult = await _socketClient.SpotApi.SubscribeToAggregatedOrderBookUpdatesAsync(Symbol, HandleFullUpdate).ConfigureAwait(false);
                if (!subResult)
                    return subResult;

                if (ct.IsCancellationRequested)
                {
                    await subResult.Data.CloseAsync().ConfigureAwait(false);
                    return subResult.AsError<UpdateSubscription>(new CancellationRequestedError());
                }

                Status = OrderBookStatus.Syncing;
                var bookResult = await _restClient.SpotApi.ExchangeData.GetAggregatedFullOrderBookAsync(Symbol).ConfigureAwait(false);
                if (!bookResult)
                {
                    _logger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, $"{Api} order book {Symbol} failed to retrieve initial order book: " + bookResult.Error);
                    await _socketClient.UnsubscribeAsync(subResult.Data).ConfigureAwait(false);
                    return new CallResult<UpdateSubscription>(bookResult.Error!);
                }

                SetInitialOrderBook(bookResult.Data.Sequence!.Value, bookResult.Data.Bids, bookResult.Data.Asks);
            }
            else
            {
                subResult = await _socketClient.SpotApi.SubscribeToOrderBookUpdatesAsync(Symbol, Levels.Value, HandleUpdate).ConfigureAwait(false);
                if (!subResult)
                    return subResult;

                if (ct.IsCancellationRequested)
                {
                    await subResult.Data.CloseAsync().ConfigureAwait(false);
                    return subResult.AsError<UpdateSubscription>(new CancellationRequestedError());
                }

                Status = OrderBookStatus.Syncing;
                var setResult = await WaitForSetOrderBookAsync(_initialDataTimeout, ct).ConfigureAwait(false);
                if (!setResult)
                {

                    await subResult.Data.CloseAsync().ConfigureAwait(false);
                    return setResult.As(subResult.Data);
                }
            }

            if (!subResult)
                return new CallResult<UpdateSubscription>(subResult.Error!);

            return new CallResult<UpdateSubscription>(subResult.Data);
        }

        /// <inheritdoc />
        protected override async Task<CallResult<bool>> DoResyncAsync(CancellationToken ct)
        {
            if (Levels != null)
                return await WaitForSetOrderBookAsync(_initialDataTimeout, ct).ConfigureAwait(false);

            var bookResult = await _restClient.SpotApi.ExchangeData.GetAggregatedFullOrderBookAsync(Symbol).ConfigureAwait(false);
            if (!bookResult)
                return new CallResult<bool>(bookResult.Error!);

            SetInitialOrderBook(bookResult.Data.Sequence!.Value, bookResult.Data.Bids, bookResult.Data.Asks);
            return new CallResult<bool>(true);
        }

        private void HandleFullUpdate(DataEvent<GeminiStreamOrderBook> data)
        {
            UpdateOrderBook(data.Data.SequenceStart, data.Data.SequenceEnd, data.Data.Changes.Bids, data.Data.Changes.Asks);
        }

        private void HandleUpdate(DataEvent<GeminiStreamOrderBookChanged> data)
        {
            SetInitialOrderBook(DateTime.UtcNow.Ticks, data.Data.Bids, data.Data.Asks);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (_clientOwner)
            {
                _socketClient?.Dispose();
                _restClient?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
