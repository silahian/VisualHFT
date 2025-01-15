using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.OrderBook;
using CryptoExchange.Net.SharedApis;
using Gemini.Net.Interfaces;
using Gemini.Net.Interfaces.Clients;
using Gemini.Net.Objects.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Gemini.Net.SymbolOrderBooks
{
    /// <summary>
    /// Gemini order book factory
    /// </summary>
    public class GeminiOrderBookFactory : IGeminiOrderBookFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc />
        public IOrderBookFactory<GeminiOrderBookOptions> Spot { get; }

        /// <inheritdoc />
        public IOrderBookFactory<GeminiOrderBookOptions> Futures { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving logging and clients</param>
        public GeminiOrderBookFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            Spot = new OrderBookFactory<GeminiOrderBookOptions>(
                CreateSpot,
                (sharedSymbol, options) => CreateSpot(GeminiExchange.FormatSymbol(sharedSymbol.BaseAsset, sharedSymbol.QuoteAsset, sharedSymbol.TradingMode, sharedSymbol.DeliverTime), options));
            Futures = new OrderBookFactory<GeminiOrderBookOptions>(
                CreateFutures,
                (sharedSymbol, options) => CreateFutures(GeminiExchange.FormatSymbol(sharedSymbol.BaseAsset, sharedSymbol.QuoteAsset, sharedSymbol.TradingMode, sharedSymbol.DeliverTime), options));
        }

        /// <inheritdoc />
        public ISymbolOrderBook Create(SharedSymbol symbol, Action<GeminiOrderBookOptions>? options = null)
        {
            var symbolName = GeminiExchange.FormatSymbol(symbol.BaseAsset, symbol.QuoteAsset, symbol.TradingMode, symbol.DeliverTime);
            if (symbol.TradingMode == TradingMode.Spot)
                return CreateSpot(symbolName, options);

            return CreateFutures(symbolName, options);
        }

        /// <summary>
        /// Create a spot SymbolOrderBook
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="options">Book options</param>
        /// <returns></returns>
        public ISymbolOrderBook CreateSpot(string symbol, Action<GeminiOrderBookOptions>? options = null)
            => new GeminiSpotSymbolOrderBook(symbol,
                                             options,
                                             _serviceProvider.GetRequiredService<ILoggerFactory>(),
                                             _serviceProvider.GetRequiredService<IGeminiRestClient>(),
                                             _serviceProvider.GetRequiredService<IGeminiSocketClient>());

        /// <summary>
        /// Create a futures SymbolOrderBook
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="options">Book options</param>
        /// <returns></returns>
        public ISymbolOrderBook CreateFutures(string symbol, Action<GeminiOrderBookOptions>? options = null)
            => new GeminiFuturesSymbolOrderBook(symbol,
                                                options,
                                                _serviceProvider.GetRequiredService<ILoggerFactory>(),
                                                _serviceProvider.GetRequiredService<IGeminiRestClient>(),
                                                _serviceProvider.GetRequiredService<IGeminiSocketClient>());
    }
}
