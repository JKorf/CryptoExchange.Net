using System.Collections.Generic;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to ticker updates
    /// </summary>
    public record SubscribeTickerRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">The symbol to subscribe to</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeTickerRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbols">The symbols to subscribe to</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeTickerRequest(IEnumerable<SharedSymbol> symbols, ExchangeParameters? exchangeParameters = null) : base(symbols, exchangeParameters)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbols">The symbols to subscribe to</param>
        public SubscribeTickerRequest(params SharedSymbol[] symbols) : base(symbols, null)
        {
        }
    }
}
