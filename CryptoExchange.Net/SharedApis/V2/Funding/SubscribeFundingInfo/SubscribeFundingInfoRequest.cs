using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to  funding info updates for a symbol
    /// </summary>
    public record SubscribeFundingInfoRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">The symbol to subscribe to</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeFundingInfoRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null)
            : base(symbol, exchangeParameters)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbols">The symbols to subscribe to</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeFundingInfoRequest(IEnumerable<SharedSymbol> symbols, ExchangeParameters? exchangeParameters = null)
            : base(symbols, exchangeParameters)
        {
        }
    }
}
