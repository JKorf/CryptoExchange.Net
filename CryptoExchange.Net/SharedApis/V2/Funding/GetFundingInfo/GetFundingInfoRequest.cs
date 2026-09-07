using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the current funding info for a symbol
    /// </summary>
    public record GetFundingInfoRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to request funding info for</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetFundingInfoRequest(
            SharedSymbol symbol,
            ExchangeParameters? exchangeParameters = null) 
            : base(symbol, exchangeParameters)
        {
        }
    }
}
