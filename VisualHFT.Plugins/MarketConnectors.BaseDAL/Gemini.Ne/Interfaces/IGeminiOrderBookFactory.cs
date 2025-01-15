using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using Gemini.Net.Objects.Options;
using System;

namespace Gemini.Net.Interfaces
{
    /// <summary>
    /// Factory for creating Gemini symbol orderbook instance
    /// </summary>
    public interface IGeminiOrderBookFactory
    {
        /// <summary>
        /// Spot order book factory methods
        /// </summary>
        public IOrderBookFactory<GeminiOrderBookOptions> Spot { get; }

        /// <summary>
        /// Futures order book factory methods
        /// </summary>
        public IOrderBookFactory<GeminiOrderBookOptions> Futures { get; }

        /// <summary>
        /// Create a SymbolOrderBook for the symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="options">Book options</param>
        /// <returns></returns>
        ISymbolOrderBook Create(SharedSymbol symbol, Action<GeminiOrderBookOptions>? options = null);

        /// <summary>
        /// Create a futures ISymbolOrderBook instance for the symbol
        /// </summary>
        /// <param name="symbol">The symbol of the order book</param>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        /// <returns></returns>
        ISymbolOrderBook CreateFutures(string symbol, Action<GeminiOrderBookOptions>? optionsDelegate = null);

        /// <summary>
        /// Create a spot ISymbolOrderBook instance for the symbol
        /// </summary>
        /// <param name="symbol">The symbol of the order book</param>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        /// <returns></returns>
        ISymbolOrderBook CreateSpot(string symbol, Action<GeminiOrderBookOptions>? optionsDelegate = null);
    }
}