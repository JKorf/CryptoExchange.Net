using System.Collections.Generic;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to Index price updates for a symbol
    /// </summary>
    public record SubscribeIndexPriceRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">The symbol to subscribe to</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeIndexPriceRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null)
            : base(symbol, exchangeParameters)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbols">The symbols to subscribe to</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeIndexPriceRequest(IEnumerable<SharedSymbol> symbols, ExchangeParameters? exchangeParameters = null)
            : base(symbols, exchangeParameters)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbols">The symbols to subscribe to</param>
        public SubscribeIndexPriceRequest(params SharedSymbol[] symbols) : base(symbols, null)
        {
        }
    }
}
