using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to mark price updates for a symbol
    /// </summary>
    public record SubscribeMarkPriceRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">The symbol to subscribe to</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeMarkPriceRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null)
            : base(symbol, exchangeParameters)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbols">The symbols to subscribe to</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeMarkPriceRequest(IEnumerable<SharedSymbol> symbols, ExchangeParameters? exchangeParameters = null)
            : base(symbols, exchangeParameters)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbols">The symbols to subscribe to</param>
        [Obsolete("Use SubscribeMarkPriceRequest(IEnumerable<SharedSymbol> symbols, ExchangeParameters? exchangeParameters = null) instead")]
        public SubscribeMarkPriceRequest(params SharedSymbol[] symbols) : base(symbols, null)
        {
        }
    }
}
