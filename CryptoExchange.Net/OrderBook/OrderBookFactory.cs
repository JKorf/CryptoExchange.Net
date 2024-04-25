using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Options;
using System;

namespace CryptoExchange.Net.OrderBook
{
    /// <inheritdoc />
    public class OrderBookFactory<TOptions> : IOrderBookFactory<TOptions> where TOptions: OrderBookOptions
    {
        private readonly Func<string, Action<TOptions>?, ISymbolOrderBook> _symbolCtor;
        private readonly Func<string, string, Action<TOptions>?, ISymbolOrderBook> _assetsCtor;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbolCtor"></param>
        /// <param name="assetsCtor"></param>
        public OrderBookFactory(Func<string, Action<TOptions>?, ISymbolOrderBook> symbolCtor, Func<string, string, Action<TOptions>?, ISymbolOrderBook> assetsCtor)
        {
            _symbolCtor = symbolCtor;
            _assetsCtor = assetsCtor;
        }

        /// <inheritdoc />
        public ISymbolOrderBook Create(string symbol, Action<TOptions>? options = null) => _symbolCtor(symbol, options);

        /// <inheritdoc />
        public ISymbolOrderBook Create(string baseAsset, string quoteAsset, Action<TOptions>? options = null) => _assetsCtor(baseAsset, quoteAsset, options);
    }
}
