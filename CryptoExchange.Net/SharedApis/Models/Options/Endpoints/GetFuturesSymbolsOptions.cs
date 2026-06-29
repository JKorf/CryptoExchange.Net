using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting symbol info
    /// </summary>
    public class GetFuturesSymbolsOptions : EndpointOptions<GetSymbolsRequest, IFuturesSymbolRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesSymbolsOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesSymbolRestClient.GetFuturesSymbolsAsync))
        {
        }
    }
}
