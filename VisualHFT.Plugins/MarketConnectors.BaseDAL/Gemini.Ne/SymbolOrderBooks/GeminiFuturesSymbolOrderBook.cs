using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Gemini.Net.Clients;
using Gemini.Net.Enums;
using Gemini.Net.Interfaces.Clients;
using Gemini.Net.Objects;
using Gemini.Net.Objects.Models.Futures.Socket;
using Gemini.Net.Objects.Models.Spot;
using Gemini.Net.Objects.Models.Spot.Socket;
using Gemini.Net.Objects.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gemini.Net.SymbolOrderBooks
{
    /// <summary>
    /// Gemini order book implementation
    /// </summary>
    public class GeminiFuturesSymbolOrderBook : CryptoExchange.Net.OrderBook.SymbolOrderBook
    {
        private readonly IGeminiRestClient _restClient;
        private readonly IGeminiSocketClient _socketClient;
        private readonly TimeSpan _initialDataTimeout;
        private readonly bool _clientOwner;

        /// <summary>
        /// Create a new order book instance
        /// </summary>
        /// <param name="symbol">The symbol the order book is for</param>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public GeminiFuturesSymbolOrderBook(string symbol, Action<GeminiOrderBookOptions>? optionsDelegate = null)
            : this(symbol, optionsDelegate, null, null, null)
        {
            _clientOwner = true;
        }

        /// <summary>
        /// Create a new order book instance
        /// </summary>
        /// <param name="symbol">The symbol the order book is for</param>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        /// <param name="logger">Logger</param>
        /// <param name="restClient">Rest client instance</param>
        /// <param name="socketClient">Socket client instance</param>
        [ActivatorUtilitiesConstructor]
        public GeminiFuturesSymbolOrderBook(
            string symbol,
            Action<GeminiOrderBookOptions>? optionsDelegate,
            ILoggerFactory? logger = null,
            IGeminiRestClient? restClient = null,
            IGeminiSocketClient? socketClient = null) : base(logger, "Gemini", "Futures", symbol)
        {
            var options = GeminiOrderBookOptions.Default.Copy();
            if (optionsDelegate != null)
                optionsDelegate(options);
            Initialize(options);

            _strictLevels = false;
            _sequencesAreConsecutive = options.Limit == null;

            Levels = options.Limit;
            _initialDataTimeout = options.InitialDataTimeout ?? TimeSpan.FromSeconds(30);
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
                subResult = await _socketClient.FuturesApi.SubscribeToOrderBookUpdatesAsync(Symbol, HandleFullUpdate).ConfigureAwait(false);
                if (!subResult)
                    return subResult;

                if (ct.IsCancellationRequested)
                {
                    await subResult.Data.CloseAsync().ConfigureAwait(false);
                    return subResult.AsError<UpdateSubscription>(new CancellationRequestedError());
                }

                Status = OrderBookStatus.Syncing;
                var bookResult = await _restClient.FuturesApi.ExchangeData.GetAggregatedFullOrderBookAsync(Symbol).ConfigureAwait(false);
                if (!bookResult)
                {
                    await _socketClient.UnsubscribeAllAsync().ConfigureAwait(false);
                    return new CallResult<UpdateSubscription>(bookResult.Error!);
                }

                SetInitialOrderBook(bookResult.Data.Sequence!.Value, bookResult.Data.Bids, bookResult.Data.Asks);
            }
            else
            {
                subResult = await _socketClient.FuturesApi.SubscribeToPartialOrderBookUpdatesAsync(Symbol, Levels.Value, HandleUpdate).ConfigureAwait(false);
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
                    return setResult.As<UpdateSubscription>(default);
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

            var bookResult = await _restClient.FuturesApi.ExchangeData.GetAggregatedFullOrderBookAsync(Symbol).ConfigureAwait(false);
            if (!bookResult)
                return new CallResult<bool>(bookResult.Error!);

            SetInitialOrderBook(bookResult.Data.Sequence!.Value, bookResult.Data.Bids, bookResult.Data.Asks);
            return new CallResult<bool>(true);
        }

        private void HandleFullUpdate(DataEvent<GeminiFuturesOrderBookChange> data)
        {
            var entry = new GeminiOrderBookEntry()
            {
                Price = data.Data.Price,
                Quantity = data.Data.Quantity
            };

            if (data.Data.Side == OrderSide.Buy)
                UpdateOrderBook(data.Data.Sequence, new List<ISymbolOrderBookEntry> { entry }, new List<ISymbolOrderBookEntry>());
            else
                UpdateOrderBook(data.Data.Sequence, new List<ISymbolOrderBookEntry>(), new List<ISymbolOrderBookEntry> { entry });
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
