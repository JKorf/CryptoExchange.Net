using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting symbol info
    /// </summary>
    public class GetSpotSymbolsOptions : EndpointOptions<GetSymbolsRequest, ISpotSymbolRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotSymbolsOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotSymbolRestClient.GetSpotSymbolsAsync))
        {
        }
    }
}
