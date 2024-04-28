using CryptoExchange.Net.Objects.Options;
using System;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Factory for ISymbolOrderBook instances
    /// </summary>
    public interface IOrderBookFactory<TOptions> where TOptions : OrderBookOptions
    {
        /// <summary>
        /// Create a new order book by symbol name
        /// </summary>
        /// <param name="symbol">Symbol name</param>
        /// <param name="options">Options for the order book</param>
        /// <returns></returns>
        public ISymbolOrderBook Create(string symbol, Action<TOptions>? options = null);
        /// <summary>
        /// Create a new order book by base and quote asset names
        /// </summary>
        /// <param name="baseAsset">Base asset name</param>
        /// <param name="quoteAsset">Quote asset name</param>
        /// <param name="options">Options for the order book</param>
        /// <returns></returns>
        public ISymbolOrderBook Create(string baseAsset, string quoteAsset, Action<TOptions>? options = null);
    }
}
